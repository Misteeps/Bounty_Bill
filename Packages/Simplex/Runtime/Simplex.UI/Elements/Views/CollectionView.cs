using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;


namespace Simplex.UI
{
	public abstract class CollectionView<TItem> : CollapsibleView, IBindable
	{
		protected override string[] DefaultClasses => new string[] { "collapsible-view", "collection-view", $"{Type}-view" };
		protected abstract string Type { get; }
		protected override bool RefreshOnAttach => true;

		public readonly Button add;
		public readonly Button delete;
		public readonly IntInput size;

		IValue IBindable.IValue
		{
			get => iValue;
			set
			{
				if (iValue != null)
				{
					iValue.Changed -= OnIValueChanged;
					if (Name == iValue.Name) Name = null;
					if (Title == iValue.Name) Title = null;
				}

				iValue = value;

				if (iValue != null)
				{
					iValue.Changed += OnIValueChanged;
					if (Name.Empty()) Name = iValue.Name;
					if (Title.Empty()) Title = iValue.Name.TitleCase();
				}
			}
		}
		private IValue iValue;
		public virtual IEnumerable<TItem> Collection { get; set; }
		public virtual int CollectionSize { get; protected set; }

		public int MaxSize
		{
			get => size.Max;
			set => size.Max = Mathf.Clamp(value, 0, int.MaxValue);
		}
		public bool LockSize
		{
			get => size.ReadOnly;
			set
			{
				add.Enable(!value);
				delete.Enable(!value);
				size.Enable(!value);
			}
		}
		public bool LockSort
		{
			get;
			set;
		}
		public UnityEngine.Object Origin { get; set; }

		public Func<TItem> itemFactory;
		public Action<TItem> onAddItem;
		public Action<int> onDeleteItem;
		public Action<int, int> onMoveItem;
		public Action<int, TItem> onReplaceItem;
		public Action<int> onResize;


		public CollectionView()
		{
			arrow.Attach(new Div() { Name = "type", Classes = "icon", Size = Size.Medium });
			arrow.Attach(new Div() { Name = "arrow", Classes = "icon", Size = Size.Medium });

			header.Attach(new HorizontalSpace(Size.Mini));
			add = header.Attach(new Button() { Name = "add", Classes = "icon" }.Bind(_ => AddItem(itemFactory.Invoke())));
			header.Attach(new HorizontalSpace(Size.Mini));
			delete = header.Attach(new Button() { Name = "delete", Classes = "icon" }.Bind(_ => DeleteItem(CollectionSize - 1)));
			header.Attach(new HorizontalSpace(Size.Mini));
			size = header.Attach(new IntInput() { Name = "size", Size = Size.Small, Delayed = true }.Bind(() => CollectionSize, Resize));

			MaxSize = 9999;
			LockSize = false;
			LockSort = false;

			itemFactory = DefaultItemFactory;
			onAddItem = DefaultOnAddItem;
			onDeleteItem = DefaultOnDeleteItem;
			onMoveItem = DefaultOnMoveItem;
			onReplaceItem = DefaultOnReplaceItem;
			onResize = DefaultOnResize;

			RegisterCallback<AttachToPanelEvent>(OnAttach);
			RegisterCallback<DetachFromPanelEvent>(OnDetach);
			RegisterCallback<RefreshEvent>(OnRefresh);
		}

		protected virtual void OnAttach(AttachToPanelEvent evnt)
		{
			if (iValue == null) return;

			iValue.Changed -= OnIValueChanged;
			iValue.Changed += OnIValueChanged;
		}
		protected virtual void OnDetach(DetachFromPanelEvent evnt)
		{
			if (iValue == null) return;

			iValue.Changed -= OnIValueChanged;
		}
		protected virtual void OnRefresh(RefreshEvent evnt)
		{
			if (iValue != null) Collection = (IEnumerable<TItem>)iValue.Value;
			CollectionSize = Collection?.Count() ?? 0;

			size.Refresh();
		}
		protected virtual void OnIValueChanged(object source)
		{
			if (source != this)
				this.Refresh();
		}

		protected virtual void ChangeCollection()
		{
			IEnumerable<TItem> oldCollection = (iValue == null) ? Collection : (IEnumerable<TItem>)iValue.Value;
			iValue?.Set(Collection, this);

			using ChangeEvent evnt1 = ChangeEvent.GetPooled(oldCollection, Collection);
			evnt1.target = this;
			SendEvent(evnt1);

			using ChangeEvent<IEnumerable<TItem>> evnt2 = ChangeEvent<IEnumerable<TItem>>.GetPooled(oldCollection, Collection);
			evnt2.target = this;
			SendEvent(evnt2);

			this.Refresh();
		}

		public void AddItem(TItem item)
		{
			try
			{
				if (Collection == null) throw new NullReferenceException("Null collection");
				if (onAddItem == null) throw new NullReferenceException("Null action");
				if (LockSize) throw new Exception("Collection size is locked");

				if (CollectionSize < MaxSize)
				{
					onAddItem.Invoke(item);
					ChangeCollection();
				}
			}
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed adding {item:ref} to {this:ref}"); }
		}
		public void DeleteItem(int index)
		{
			try
			{
				if (Collection == null) throw new NullReferenceException("Null collection");
				if (onDeleteItem == null) throw new NullReferenceException("Null action");
				if (LockSize) throw new Exception("Collection size is locked");

				if (CollectionSize > 0)
				{
					onDeleteItem.Invoke(index);
					ChangeCollection();
				}
			}
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed removing [{index:info}] from {this:ref}"); }
		}
		public void MoveItem(int index, int newIndex)
		{
			try
			{
				if (Collection == null) throw new NullReferenceException("Null collection");
				if (onMoveItem == null) throw new NullReferenceException("Null action");
				if (LockSort) throw new Exception("Collection sorting is locked");

				if (index != newIndex)
				{
					onMoveItem.Invoke(index, newIndex);
					ChangeCollection();
				}
			}
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed moving [{index:info}] to [{newIndex:info}] in {this:ref}"); }
		}
		public void ReplaceItem(int index, TItem item)
		{
			try
			{
				if (Collection == null) throw new NullReferenceException("Null collection");
				if (onReplaceItem == null) throw new NullReferenceException("Null action");

				onReplaceItem.Invoke(index, item);
				ChangeCollection();
			}
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed replacing [{index:info}] with {item:ref} in {this:ref}"); }
		}
		public void Resize(int size)
		{
			try
			{
				if (Collection == null) throw new NullReferenceException("Null collection");
				if (onResize == null) throw new NullReferenceException("Null action");
				if (LockSize) throw new Exception("Collection size is locked");

				size = Mathf.Clamp(size, 0, MaxSize);
				if (size != CollectionSize)
				{
					onResize.Invoke(size);
					ChangeCollection();
				}
			}
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed resizing {this:ref} to size {size:info}"); }
		}

		protected virtual TItem DefaultItemFactory()
		{
			if (typeof(TItem).IsClass)
				try { return Activator.CreateInstance<TItem>(); }
				catch { }

			return default;
		}
		protected virtual void DefaultOnAddItem(TItem item)
		{
			switch (Collection)
			{
				default: throw new NotImplementedException($"Add Item action required for {Collection:type}");
				case null: throw new NullReferenceException("Null collection");

				case TItem[] array:
					Collection = array.Append(item);
					break;

				case IList<TItem> iList when !iList.IsReadOnly:
					iList.Add(item);
					break;

				case ICollection<TItem> iCollection when !iCollection.IsReadOnly:
					goto default;
			}
		}
		protected virtual void DefaultOnDeleteItem(int index)
		{
			switch (Collection)
			{
				default: throw new NotImplementedException($"Remove Item action required for {Collection:type}");
				case null: throw new NullReferenceException("Null collection");

				case TItem[] array:
					Collection = array.Remove(index);
					break;

				case IList<TItem> iList when !iList.IsReadOnly:
					iList.RemoveAt(index);
					break;

				case ICollection<TItem> iCollection when !iCollection.IsReadOnly:
					goto default;
			}
		}
		protected virtual void DefaultOnMoveItem(int index, int newIndex)
		{
			switch (Collection)
			{
				default: throw new NotImplementedException($"Move Item action required for {Collection:type}");
				case null: throw new NullReferenceException("Null collection");

				case TItem[] array:
					TItem arrayItem = array[index];
					if (index < newIndex)
						for (int i = index; i < newIndex; i++)
							array[i] = array[i + 1];
					else
						for (int i = index; i > newIndex; i--)
							array[i] = array[i - 1];
					array[newIndex] = arrayItem;
					Collection = array;
					break;

				case IList<TItem> iList when !iList.IsReadOnly:
					TItem iListItem = iList[index];
					iList.RemoveAt(index);
					iList.Insert(newIndex, iListItem);
					break;

				case ICollection<TItem> iCollection when !iCollection.IsReadOnly:
					goto default;
			}
		}
		protected virtual void DefaultOnReplaceItem(int index, TItem item)
		{
			switch (Collection)
			{
				default: throw new NotImplementedException($"Replace Item action required for {Collection:type}");
				case null: throw new NullReferenceException("Null collection");

				case TItem[] array:
					array[index] = item;
					break;

				case IList<TItem> iList when !iList.IsReadOnly:
					iList[index] = item;
					break;

				case ICollection<TItem> iCollection when !iCollection.IsReadOnly:
					goto default;
			}
		}
		protected virtual void DefaultOnResize(int size)
		{
			switch (Collection)
			{
				default: throw new NotImplementedException($"Resize action required for {Collection:type}");
				case null: throw new NullReferenceException("Null collection");

				case TItem[] array:
					TItem[] newArray = new TItem[size];
					for (int i = 0; i < size; i++)
						newArray[i] = (array.OutOfRange(i)) ? itemFactory.Invoke() : array[i];
					Collection = newArray;
					break;

				case List<TItem> list:
					list.Capacity = Mathf.Max(list.Count, size);
					while (list.Count < size) list.Add(itemFactory.Invoke());
					while (list.Count > size) list.RemoveAt(list.Count - 1);
					list.TrimExcess();
					break;

				case IList<TItem> iList when !iList.IsReadOnly:
					while (iList.Count < size) iList.Add(itemFactory.Invoke());
					while (iList.Count > size) iList.RemoveAt(iList.Count - 1);
					break;

				case ICollection<TItem> iCollection when !iCollection.IsReadOnly:
					goto default;
			}
		}
	}


	public abstract class CollectionView<TItem, TElement> : CollectionView<TItem> where TElement : CollectionElement, new()
	{
		public readonly UQueryState<TElement> elements;

		public Func<TItem, TElement> elementFactory;
		public Action<TElement> onDisposeElement;
		public Action<int, TItem, TElement> onInitializeElement;
		public Action<int, TItem, TElement> onBindElement;


		public CollectionView()
		{
			elements = this.Query().Descendents<TElement>().Build();

			elementFactory = DefaultElementFactory;
			onDisposeElement = DefaultOnDisposeElement;
			onInitializeElement = DefaultOnInitializeElement;
			onBindElement = DefaultOnBindElement;
		}

		protected override void OnDetach(DetachFromPanelEvent evnt)
		{
			base.OnDetach(evnt);

			DetachElements();
		}
		protected override void OnRefresh(RefreshEvent evnt)
		{
			base.OnRefresh(evnt);

			DetachElements();
			AttachElements();
		}

		protected virtual void AttachElements()
		{
			if (Collection == null) return;

			int index = 0;
			foreach (TItem item in Collection)
				try
				{
					TElement element = elementFactory.Invoke(item);
					onInitializeElement?.Invoke(index, item, element);
					onBindElement?.Invoke(index, item, element);
					contentContainer.Attach(element);
					index++;
				}
				catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed attaching/binding {typeof(TElement):type} to {item:ref} in {this:ref}"); }
		}
		protected virtual void DetachElements()
		{
			elements.ForEach(element =>
			{
				try
				{
					onDisposeElement?.Invoke(element);
					element.RemoveFromHierarchy();
				}
				catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed detaching/disposing {typeof(TElement):type} from {typeof(TItem):type} in {this:ref}"); }
			});
		}

		protected virtual TElement DefaultElementFactory(TItem item) => GenericPool<TElement>.Get();
		protected virtual void DefaultOnDisposeElement(TElement element) => GenericPool<TElement>.Release(element);
		protected virtual void DefaultOnInitializeElement(int index, TItem item, TElement element)
		{
			element.Name = item?.ToString();
			element.Index = index;
			element.Origin = Origin;
			element.ContextMenu = null;
		}
		protected virtual void DefaultOnBindElement(int index, TItem item, TElement element) => element.Bind(item);
	}
}