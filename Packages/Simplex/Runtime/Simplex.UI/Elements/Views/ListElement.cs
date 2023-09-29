using System;

using UnityEngine;
using UnityEngine.UIElements;


namespace Simplex.UI
{
	public class ListElement : CollectionElement
	{
		protected override string[] DefaultClasses => new string[] { "list-element" };
		public override VisualElement contentContainer => contents;

		public readonly Label index;
		public readonly Div contents;
		private Div[] rows;

		public override int Index
		{
			set
			{
				base.Index = value;
				index.Text = value.ToString();
				this.ClassToggle("even", "odd", value % 2 == 0);
			}
		}
		public override ContextualMenuManipulator ContextMenu
		{
			set
			{
				index.RemoveManipulator(contextMenu);
				contextMenu = value;
				index.AddManipulator(contextMenu);
			}
		}
		public bool Draggable
		{
			get => index.ClassListContains("draggable");
			set => index.EnableInClassList("draggable", value);
		}

		public EventCallback<PointerUpEvent> onPointerUp;
		public EventCallback<PointerDownEvent> onPointerDown;
		public EventCallback<PointerMoveEvent> onPointerMove;


		public ListElement()
		{
			index = hierarchy.Attach(new Label() { Classes = "index, icon", Pickable = true, Text = "0" });
			contents = hierarchy.Attach(new Div() { Classes = "contents", Flexible = true });

			index.RegisterCallback<PointerUpEvent>(OnPointerUp);
			index.RegisterCallback<PointerDownEvent>(OnPointerDown);
			index.RegisterCallback<PointerMoveEvent>(OnPointerMove);
		}

		protected virtual void OnPointerUp(PointerUpEvent evnt) => onPointerUp?.Invoke(evnt);
		protected virtual void OnPointerDown(PointerDownEvent evnt) => onPointerDown?.Invoke(evnt);
		protected virtual void OnPointerMove(PointerMoveEvent evnt) => onPointerMove?.Invoke(evnt);

		public virtual Div Row(int index)
		{
			if (rows.Empty())
			{
				rows = new Div[index + 1];
				for (int i = 0; i < rows.Length; i++)
					rows[i] = this.Attach(new Div() { Name = i.ToString(), Classes = "row" });
			}

			if (rows.OutOfRange(index))
			{
				Array.Resize(ref rows, index + 1);
				for (int i = 0; i < rows.Length; i++)
					if (rows[i] == null)
						rows[i] = this.Attach(new Div() { Name = i.ToString(), Classes = "row" });
			}

			return rows[index];
		}
	}
}