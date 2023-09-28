using System;
using System.Linq;

using UnityEngine;
using UnityEngine.UIElements;


namespace Simplex.UI
{
	public class ListView<TItem> : ListView<TItem, ListElement> { }
	public class ListView<TItem, TElement> : CollectionView<TItem, TElement> where TElement : ListElement, new()
	{
		protected override string Type => "list";

		protected TElement[] dragTargets;
		protected Div dragline;
		protected int draglineIndex;
		protected int dragStart;
		protected bool dragging;


		public ListView()
		{
			dragline = new Div() { Classes = "dragline" };
			MoveDragLine(-1);
		}

		protected virtual ContextualMenuManipulator ElementContextMenu(TElement element, int index) => new ContextualMenuManipulator(contextEvent =>
		{
			DropdownMenuAction.Status moveUpStatus = (LockSort || index == 0) ? DropdownMenuAction.Status.Disabled : DropdownMenuAction.Status.Normal;
			DropdownMenuAction.Status moveDownStatus = (LockSort || index == CollectionSize - 1) ? DropdownMenuAction.Status.Disabled : DropdownMenuAction.Status.Normal;
			DropdownMenuAction.Status deleteStatus = (LockSize) ? DropdownMenuAction.Status.Disabled : DropdownMenuAction.Status.Normal;

			contextEvent.menu.AppendSeparator();
			contextEvent.menu.AppendAction("Move Up", e => MoveItem(index, index - 1), moveUpStatus);
			contextEvent.menu.AppendAction("Move Down", e => MoveItem(index, index + 1), moveDownStatus);
			contextEvent.menu.AppendSeparator();
			contextEvent.menu.AppendAction("Move to Top", e => MoveItem(index, 0), moveUpStatus);
			contextEvent.menu.AppendAction("Move to Bottom", e => MoveItem(index, CollectionSize - 1), moveDownStatus);
			contextEvent.menu.AppendSeparator();
			contextEvent.menu.AppendAction("Delete", e => DeleteItem(index), deleteStatus);
		});
		protected virtual void OnElementPointerUp(TElement element, int index, PointerUpEvent pointerEvent)
		{
			if (LockSort || !dragging || pointerEvent.button != 0) return;

			int newIndex = CalculateDrag(element);
			if (newIndex < 0 || newIndex >= CollectionSize)
				newIndex = dragStart;

			element.RemoveFromClassList("drag");
			dragTargets = null;
			dragStart = -1;
			dragging = false;

			MoveDragLine(-1);
			element.style.top = StyleKeyword.Null;
			element.style.left = StyleKeyword.Null;

			element.index.ReleasePointer(pointerEvent.pointerId);
			pointerEvent.StopPropagation();

			element.RemoveFromHierarchy();
			onDisposeElement.Invoke(element);

			MoveItem(index, newIndex);
		}
		protected virtual void OnElementPointerDown(TElement element, int index, PointerDownEvent pointerEvent)
		{
			if (LockSort || dragging || pointerEvent.button != 0) return;

			element.AddToClassList("drag");
			dragTargets = body.Query<TElement>().Where(e => e != element).Build().ToArray();
			dragStart = index;
			dragging = true;

			for (int i = 0; i < dragTargets.Length; i++)
			{
				TElement dragTarget = dragTargets[i];
				dragTarget.Index = i;
				dragTarget.Draggable = false;
			}

			MoveDragLine(body.IndexOf(element));
			MoveElement(element, pointerEvent.localPosition);
			body.Add(element);

			element.index.CapturePointer(pointerEvent.pointerId);
			pointerEvent.StopPropagation();
		}
		protected virtual void OnElementPointerMove(TElement element, PointerMoveEvent pointerEvent)
		{
			if (LockSort || !dragging || !element.index.HasMouseCapture()) return;

			MoveElement(element, pointerEvent.localPosition);
			MoveDragLine(CalculateDrag(element));

			pointerEvent.StopPropagation();
		}

		protected virtual void MoveDragLine(int index)
		{
			if (index == draglineIndex) return;

			if (index == -1)
				dragline.RemoveFromHierarchy();
			else
				body.Insert(index, dragline);

			draglineIndex = index;
		}
		protected virtual void MoveElement(TElement element, Vector2 pointerPosition)
		{
			element.style.top = element.layout.y - (element.layout.height / 2) + pointerPosition.y;
			element.style.left = element.layout.x - (element.index.layout.width / 2) + pointerPosition.x;
		}
		protected virtual int CalculateDrag(TElement element)
		{
			if (dragTargets.Empty()) return -1;

			TElement first = dragTargets[0];
			float left = first.layout.xMin;
			float right = first.layout.xMax;

			for (int i = -1; i < dragTargets.Length; i++)
			{
				float top = (i == -1) ? first.layout.yMin - 40 : dragTargets[i].layout.center.y;
				float bottom = (i + 1 == dragTargets.Length) ? dragTargets[i].layout.yMax + 40 : dragTargets[i + 1].layout.center.y;

				if (Rect.MinMaxRect(left, top, right, bottom).Contains(element.layout.center))
					return i + 1;
			}

			return -1;
		}

		protected override void DefaultOnInitializeElement(int index, TItem item, TElement element)
		{
			base.DefaultOnInitializeElement(index, item, element);

			element.ContextMenu = ElementContextMenu(element, index);
			element.Draggable = !LockSort;
			element.onPointerUp = e => OnElementPointerUp(element, index, e);
			element.onPointerDown = e => OnElementPointerDown(element, index, e);
			element.onPointerMove = e => OnElementPointerMove(element, e);
		}
	}
}