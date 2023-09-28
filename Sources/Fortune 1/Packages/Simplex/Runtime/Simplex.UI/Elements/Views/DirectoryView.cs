using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;


namespace Simplex.UI
{
	public class DirectoryView<TItem> : DirectoryView<TItem, DirectoryElement> { }
	public class DirectoryView<TItem, TElement> : CollectionView<TItem, TElement> where TElement : DirectoryElement, new()
	{
		protected override string Type => "directory";

		public readonly StringInput searchbar;

		protected TItem[] selected;
		public Dictionary<string, TElement> Paths { get; protected set; }
		public Dictionary<TItem, TElement> Items { get; protected set; }

		public bool Searchable
		{
			get => searchbar.resolvedStyle.display == DisplayStyle.Flex;
			set
			{
				searchbar.Display(value);
				if (!value) Search = null;
			}
		}
		public virtual string Search
		{
			get => searchbar.CurrentValue;
			set
			{
				if (value == searchbar.CurrentValue) return;
				searchbar.CurrentValue = value;
			}
		}

		public Func<TItem, string> itemPath;
		public Action<TItem, bool> onSelect;


		public DirectoryView()
		{
			header.Clear();
			header.Attach(arrow);
			header.Attach(new HorizontalSpace(Size.Mini));
			header.Attach(title);
			header.Attach(new HorizontalSpace(Size.Mini));
			searchbar = header.Attach(new StringInput() { Name = "searchbar", Classes = "icon", Size = Size.Small }.Bind(() => Search, value => Search = value));

			LockSize = true;
			LockSort = true;

			itemPath = DefaultItemPath;
			onSelect = DefaultOnSelect;
		}

		protected override void OnRefresh(RefreshEvent evnt)
		{
			Paths = new Dictionary<string, TElement>();
			Items = new Dictionary<TItem, TElement>();

			base.OnRefresh(evnt);
		}

		public virtual void SetSelected(params TItem[] selected)
		{
			this.selected = selected;

			if (selected.Empty()) return;
			if (Items.Empty()) return;

			foreach (KeyValuePair<TItem, TElement> item in Items)
				item.Value.Selected = Array.IndexOf(selected, item.Key) != -1;
		}

		protected override void AttachElements()
		{
			if (Collection == null) return;

			int index = 0;
			foreach (TItem item in Collection)
				try
				{
					if (item == null) throw new NullReferenceException("Null item");

					string path = itemPath.Invoke(item)?.Replace('\\', '/');
					if (path.Empty()) throw new NullReferenceException("Null path");

					TElement element;
					Paths.TryGetValue(path, out TElement pathElement);
					Items.TryGetValue(item, out TElement itemElement);

					if (pathElement == null && itemElement == null) element = elementFactory.Invoke(item);
					else
					{
						Paths.Remove(path);
						Items.Remove(item);

						if (pathElement != itemElement)
						{
							ConsoleUtilities.Warn(ConsoleUtilities.uiTag, $"Nonmatching element path {path:info} and item {item:ref} found in {this:ref}");
							element = elementFactory.Invoke(item);
						}
						else
						{
							ConsoleUtilities.Warn(ConsoleUtilities.uiTag, $"Duplicate item {item:ref} found in {this:ref}");
							element = pathElement;
						}
					}

					onInitializeElement?.Invoke(index, item, element);
					onBindElement?.Invoke(index, item, element);
					AddElement(element, path);
					index++;

					Paths.Add(path, element);
					Items.Add(item, element);
				}
				catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed attaching/binding {typeof(TElement):type} to {item:ref} in {this:ref}"); }
		}
		protected virtual void AddElement(TElement element, string path)
		{
			string directory = System.IO.Path.GetDirectoryName(path)?.Replace('\\', '/');
			if (directory.Empty()) { body.Add(element); return; }

			if (!Paths.TryGetValue(directory, out TElement parent))
			{
				parent = elementFactory.Invoke(default);
				parent.Selected = false;
				parent.Collapsed = false;
				parent.Origin = Origin;
				parent.onClick = null;
				parent.Bind(System.IO.Path.GetFileName(directory), null);

				AddElement(parent, directory);
				Paths.Add(directory, parent);
			}

			parent.Add(element);
		}

		protected virtual string DefaultItemPath(TItem item)
		{
#if UNITY_EDITOR
			if (item is UnityEngine.Object obj && UnityEditor.AssetDatabase.Contains(obj))
				return UnityEditor.AssetDatabase.GetAssetPath(obj);
#endif

			return item.ToString();
		}
		protected virtual void DefaultOnSelect(TItem item, bool selected) { }

		protected override void DefaultOnInitializeElement(int index, TItem item, TElement element)
		{
			base.DefaultOnInitializeElement(index, item, element);

			element.Selected = !selected.Empty() && Array.IndexOf(selected, item) != -1;
			element.Collapsed = false;
			element.onClick = () => onSelect.Invoke(item, element.Selected);
		}
		protected override void DefaultOnBindElement(int index, TItem item, TElement element) => element.Bind(itemPath.Invoke(item), item);
	}
}