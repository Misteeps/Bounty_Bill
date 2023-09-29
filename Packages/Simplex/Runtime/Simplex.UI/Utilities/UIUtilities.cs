using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;


namespace Simplex.UI
{
	public enum Size { Null, Mini, Small, Medium, Large, Huge }

	public static class UIUtilities
	{
		public static T Attach<T>(this VisualElement parent, T element) where T : VisualElement
		{
			try { parent.Add(element); }
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed attaching {element:ref} to {parent:ref}"); }
			return element;
		}
		public static T Attach<T>(this VisualElement parent, int index, T element) where T : VisualElement
		{
			try { parent.Insert(index, element); }
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed attaching {element:ref} to {parent:ref}"); }
			return element;
		}
		public static T Attach<T>(this VisualElement.Hierarchy parent, T element) where T : VisualElement
		{
			try { parent.Add(element); }
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed attaching {element:ref} to {parent:ref}"); }
			return element;
		}
		public static T Attach<T>(this VisualElement.Hierarchy parent, int index, T element) where T : VisualElement
		{
			try { parent.Insert(index, element); }
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed attaching {element:ref} to {parent:ref}"); }
			return element;
		}
		public static T Detach<T>(this VisualElement parent, T element) where T : VisualElement
		{
			try { parent.Remove(element); }
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed detaching {element:ref} from {parent:ref}"); }
			return element;
		}
		public static T Detach<T>(this VisualElement.Hierarchy parent, T element) where T : VisualElement
		{
			try { parent.Remove(element); }
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed detaching {element:ref} from {parent:ref}"); }
			return element;
		}
		public static VisualElement Detach(this VisualElement parent, int index)
		{
			try
			{
				VisualElement element = parent[index];
				parent.RemoveAt(index);
				return element;
			}
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed detaching child {index:info} in {parent:ref}"); return null; }
		}
		public static VisualElement Detach(this VisualElement.Hierarchy parent, int index)
		{
			try
			{
				VisualElement element = parent[index];
				parent.RemoveAt(index);
				return element;
			}
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed detaching child {index:info} in {parent:ref}"); return null; }
		}

		public static T AttachTo<T>(this T element, VisualElement parent) where T : VisualElement
		{
			try { parent.Add(element); }
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed attaching {element:ref} to {parent:ref}"); }
			return element;
		}
		public static T AttachTo<T>(this T element, VisualElement.Hierarchy parent) where T : VisualElement
		{
			try { parent.Add(element); }
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed attaching {element:ref} to {parent:ref}"); }
			return element;
		}
		public static T AttachTo<T>(this T element, VisualElement parent, int index) where T : VisualElement
		{
			try { parent.Insert(index, element); }
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed attaching {element:ref} to {parent:ref}"); }
			return element;
		}
		public static T AttachTo<T>(this T element, VisualElement.Hierarchy parent, int index) where T : VisualElement
		{
			try { parent.Insert(index, element); }
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed attaching {element:ref} to {parent:ref}"); }
			return element;
		}
		public static T DetachFrom<T>(this T element) where T : VisualElement
		{
			try { element.RemoveFromHierarchy(); }
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed detaching {element:ref} from panel"); }
			return element;
		}

		public static T AttachField<T>(this VisualElement parent, IValue iValue, T element) where T : Field, IBindable
		{
			try { parent.Add(new Labeled(element.Bind(iValue))); }
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed attaching labeled field {element:ref} to {parent:ref}"); }
			return element;
		}
		public static T AttachField<T>(this VisualElement parent, IValue iValue, string tooltip, T element) where T : Field, IBindable
		{
			try { parent.Add(new Labeled(element.Bind(iValue)) { Tooltip = tooltip }); }
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed attaching labeled field {element:ref} to {parent:ref}"); }
			return element;
		}
		public static T AttachField<T>(this VisualElement.Hierarchy parent, IValue iValue, T element) where T : Field, IBindable
		{
			try { parent.Add(new Labeled(element.Bind(iValue))); }
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed attaching labeled field {element:ref} to {parent:ref}"); }
			return element;
		}
		public static T AttachField<T>(this VisualElement.Hierarchy parent, IValue iValue, string tooltip, T element) where T : Field, IBindable
		{
			try { parent.Add(new Labeled(element.Bind(iValue)) { Tooltip = tooltip }); }
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed attaching labeled field {element:ref} to {parent:ref}"); }
			return element;
		}

		public static T AttachLabeled<T>(this VisualElement parent, string label, T element) where T : VisualElement
		{
			try { parent.Add(new Labeled(label, element)); }
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed attaching labeled {element:ref} to {parent:ref}"); }
			return element;
		}
		public static T AttachLabeled<T>(this VisualElement parent, string label, string tooltip, T element) where T : VisualElement
		{
			try { parent.Add(new Labeled(label, element) { Tooltip = tooltip }); }
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed attaching labeled {element:ref} to {parent:ref}"); }
			return element;
		}
		public static Labeled AttachLabeled(this VisualElement parent, string label, params VisualElement[] elements)
		{
			Labeled labeled = null;
			try { labeled = parent.Attach(new Labeled(label, elements)); }
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed attaching labeled elements to {parent:ref}"); }
			return labeled;
		}
		public static Labeled AttachLabeled(this VisualElement parent, string label, string tooltip, params VisualElement[] elements)
		{
			Labeled labeled = null;
			try { labeled = parent.Attach(new Labeled(label, elements) { Tooltip = tooltip }); }
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed attaching labeled elements to {parent:ref}"); }
			return labeled;
		}
		public static T AttachLabeled<T>(this VisualElement.Hierarchy parent, string label, T element) where T : VisualElement
		{
			try { parent.Add(new Labeled(label, element)); }
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed attaching labeled {element:ref} to {parent:ref}"); }
			return element;
		}
		public static T AttachLabeled<T>(this VisualElement.Hierarchy parent, string label, string tooltip, T element) where T : VisualElement
		{
			try { parent.Add(new Labeled(label, element) { Tooltip = tooltip }); }
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed attaching labeled {element:ref} to {parent:ref}"); }
			return element;
		}
		public static Labeled AttachLabeled(this VisualElement.Hierarchy parent, string label, params VisualElement[] elements)
		{
			Labeled labeled = null;
			try { labeled = parent.Attach(new Labeled(label, elements)); }
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed attaching labeled elements to {parent:ref}"); }
			return labeled;
		}
		public static Labeled AttachLabeled(this VisualElement.Hierarchy parent, string label, string tooltip, params VisualElement[] elements)
		{
			Labeled labeled = null;
			try { labeled = parent.Attach(new Labeled(label, elements) { Tooltip = tooltip }); }
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed attaching labeled elements to {parent:ref}"); }
			return labeled;
		}

		public static T Style<T>(this T element, params StyleSheet[] styles) where T : VisualElement => Style(element, false, styles);
		public static T Style<T>(this T element, bool clearExisting, params StyleSheet[] styles) where T : VisualElement
		{
			try
			{
				if (clearExisting) element.styleSheets.Clear();
				if (!styles.Empty())
					for (int i = 0; i < styles.Length; i++)
						element.styleSheets.Add(styles[i]);
			}
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed applying style to {element:ref}"); }
			return element;
		}
		public static T Class<T>(this T element, params string[] classes) where T : VisualElement => Class(element, false, classes);
		public static T Class<T>(this T element, bool clearExisting, params string[] classes) where T : VisualElement
		{
			try
			{
				if (clearExisting) element.ClearClassList();
				if (!classes.Empty())
					for (int i = 0; i < classes.Length; i++)
						element.AddToClassList(classes[i]);
			}
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed applying classes to {element:ref}"); }
			return element;
		}
		public static T ClassToggle<T>(this T element, string enabledClass, string disabledClass, bool enable) where T : VisualElement
		{
			try
			{
				if (enable)
				{
					element.AddToClassList(enabledClass);
					element.RemoveFromClassList(disabledClass);
				}
				else
				{
					element.RemoveFromClassList(enabledClass);
					element.AddToClassList(disabledClass);
				}
			}
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed toggling classes in {element:ref}"); }
			return element;
		}

		public static T Name<T>(this T element, string name, bool toLower = true, bool replaceSpaces = true) where T : VisualElement
		{
			try
			{
				if (toLower) name = name?.ToLower();
				if (replaceSpaces) name = name?.Replace(' ', '_');
				element.name = name;
			}
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed applying name to {element:ref}"); }
			return element;
		}
		public static T Tooltip<T>(this T element, string tooltip) where T : VisualElement
		{
			try { element.tooltip = tooltip; }
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed applying tooltip to {element:ref}"); }
			return element;
		}
		public static T Focusable<T>(this T element, bool focusable) where T : VisualElement
		{
			try { element.focusable = focusable; }
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed setting focusable of {element:ref}"); }
			return element;
		}
		public static T PickingMode<T>(this T element, bool pickable) where T : VisualElement => PickingMode(element, (pickable) ? UnityEngine.UIElements.PickingMode.Position : UnityEngine.UIElements.PickingMode.Ignore);
		public static T PickingMode<T>(this T element, PickingMode pickingMode) where T : VisualElement
		{
			try { element.pickingMode = pickingMode; }
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed setting picking mode of {element:ref}"); }
			return element;
		}
		public static T UsageHints<T>(this T element, UsageHints usageHints) where T : VisualElement
		{
			try { element.usageHints = usageHints; }
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed setting usage hints of {element:ref}"); }
			return element;
		}

		public static T Size<T>(this T element, Size size) where T : VisualElement
		{
			try
			{
				foreach (Size sizeValue in Enum.GetValues(typeof(Size)))
					element.RemoveFromClassList(sizeValue.ToString().ToLower());

				if (size != UI.Size.Null)
					element.AddToClassList(size.ToString().ToLower());
			}
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed applying size enum to {element:ref}"); }
			return element;
		}
		public static T Flex<T>(this T element, bool flexible) where T : VisualElement
		{
			try { element.EnableInClassList("flexible", flexible); }
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed setting flexible class of {element:ref}"); }
			return element;
		}

		public static T Text<T>(this T element, string text) where T : VisualElement
		{
			try
			{
				if (element is TextElement textElement) textElement.text = text;
				else throw new ArgumentException($"Unexpected type");
				element.AddToClassList("text");
			}
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed applying text to {element:ref}"); }
			return element;
		}
		public static T Text<T>(this T element, string text, Size size) where T : VisualElement
		{
			try
			{
				element.Text(text);
				element.Size(size);
			}
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed applying text and size to {element:ref}"); }
			return element;
		}
		public static T Icon<T>(this T element, StyleBackground icon) where T : VisualElement
		{
			try
			{
				element.style.backgroundImage = icon;
				element.AddToClassList("icon");
			}
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed applying icon to {element:ref}"); }
			return element;
		}
		public static T Icon<T>(this T element, StyleBackground icon, Size size) where T : VisualElement
		{
			try
			{
				element.Icon(icon);
				element.Size(size);
			}
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed applying icon and size to {element:ref}"); }
			return element;
		}

		public static T Bind<T, TItem>(this T element, Type enumType, params TItem[] selected) where T : DirectoryView<TItem>, IBindable => Bind(element, (TItem[])Enum.GetValues(enumType), selected);
		public static T Bind<T, TItem>(this T element, Dictionary<TItem, string> collection, params TItem[] selected) where T : DirectoryView<TItem>, IBindable
		{
			try
			{
				Bind(element, collection.Keys, selected);
				element.itemPath = item => collection[item];
			}
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed binding iValue to {element:ref}"); }
			return element;
		}
		public static T Bind<T, TItem>(this T element, IEnumerable<TItem> collection, params TItem[] selected) where T : DirectoryView<TItem>, IBindable
		{
			try
			{
				Bind(element, collection);
				element.SetSelected(selected);
			}
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed binding iValue to {element:ref}"); }
			return element;
		}
		public static T Bind<T, TItem>(this T element, IEnumerable<TItem> collection, Action<IEnumerable<TItem>> setCollection = null) where T : CollectionView<TItem>, IBindable => Bind(element, new DelegateValue<IEnumerable<TItem>>($"{typeof(TItem).GetTypeName()}s", () => collection, setCollection));
		public static T Bind<T, TValue>(this T element, Func<TValue> getValue, Action<TValue> setValue) where T : VisualElement, IBindable => Bind(element, new DelegateValue<TValue>(getValue, setValue));
		public static T Bind<T, TValue>(this T element, string name, Func<TValue> getValue, Action<TValue> setValue) where T : VisualElement, IBindable => Bind(element, new DelegateValue<TValue>(name, getValue, setValue));
		public static T Bind<T>(this T element, IValue iValue) where T : VisualElement, IBindable
		{
			try { element.IValue = iValue; }
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed binding iValue to {element:ref}"); }
			return element.Refresh();
		}

		public static T RegisterChangeCallback<T>(this T element, EventCallback<ChangeEvent> evnt) where T : VisualElement
		{
			try { element.RegisterCallback(evnt); }
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed registering change event to {element:ref}"); }
			return element;
		}
		public static T RegisterRefreshCallback<T>(this T element, EventCallback<RefreshEvent> evnt) where T : VisualElement
		{
			try { element.RegisterCallback(evnt); }
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed registering refresh event to {element:ref}"); }
			return element;
		}

		public static T Enable<T>(this T element, bool enabled) where T : VisualElement => Enable(element, enabled, (enabled) ? UnityEngine.UIElements.PickingMode.Position : UnityEngine.UIElements.PickingMode.Ignore);
		public static T Enable<T>(this T element, bool enabled, PickingMode pickingMode) where T : VisualElement
		{
			try
			{
				element.SetEnabled(enabled);
				element.pickingMode = pickingMode;
			}
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed setting enabled of {element:ref}"); }
			return element;
		}
		public static T Collapse<T>(this T element, bool collapsed, bool collapseChildren = false) where T : VisualElement
		{
			try
			{
				if (element is ICollapsible collapsible) collapsible.Collapsed = collapsed;
				if (collapseChildren)
				{
					for (int i = 0; i < element.hierarchy.childCount; i++)
						element.hierarchy[i].Collapse(collapsed, true);
				}
			}
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed collapsing {element:ref}"); }
			return element;
		}
		public static T Refresh<T>(this T element, bool refreshChildren = false, bool requirePanel = false) where T : VisualElement
		{
			try
			{
				if (element.panel == null) return (requirePanel) ? throw new NullReferenceException("Null panel") : element;

				using (RefreshEvent evnt = RefreshEvent.GetPooled())
				{
					evnt.target = element;
					element.SendEvent(evnt);
				}

				if (refreshChildren)
					for (int i = 0; i < element.hierarchy.childCount; i++)
						element.hierarchy[i].Refresh(true, true);
			}
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed refreshing {element:ref}"); }
			return element;
		}


		public static T Display<T>(this T element, bool display) where T : VisualElement => Display(element, (display) ? DisplayStyle.Flex : DisplayStyle.None);
		public static T Display<T>(this T element, StyleEnum<DisplayStyle> display) where T : VisualElement
		{
			try { element.style.display = display; }
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed setting display of {element:ref}"); }
			return element;
		}

		public static T Visible<T>(this T element, bool visible) where T : VisualElement => Visible(element, (visible) ? Visibility.Visible : Visibility.Hidden);
		public static T Visible<T>(this T element, StyleEnum<Visibility> visible) where T : VisualElement
		{
			try { element.style.visibility = visible; }
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed setting visibility of {element:ref}"); }
			return element;
		}

		public static T Flex<T>(this T element, bool grow, bool shrink) where T : VisualElement
		{
			try
			{
				element.style.flexGrow = (grow) ? 1 : 0;
				element.style.flexShrink = (shrink) ? 1 : 0;
			}
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed setting flex of {element:ref}"); }
			return element;
		}
		public static T Flex<T>(this T element, bool grow, bool shrink, FlexDirection direction) where T : VisualElement
		{
			try
			{
				element.style.flexGrow = (grow) ? 1 : 0;
				element.style.flexShrink = (shrink) ? 1 : 0;
				element.style.flexDirection = direction;
			}
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed setting flex of {element:ref}"); }
			return element;
		}
		public static T Flex<T>(this T element, bool grow, bool shrink, FlexDirection direction, Wrap wrap) where T : VisualElement
		{
			try
			{
				element.style.flexGrow = (grow) ? 1 : 0;
				element.style.flexShrink = (shrink) ? 1 : 0;
				element.style.flexDirection = direction;
				element.style.flexWrap = wrap;
			}
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed setting flex of {element:ref}"); }
			return element;
		}

		public static T Size<T>(this T element, int size) where T : VisualElement => Size(element, size, size);
		public static T Size<T>(this T element, int width, int height) where T : VisualElement
		{
			try
			{
				element.style.width = width;
				element.style.height = height;
			}
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed setting size of {element:ref}"); }
			return element;
		}
		public static T Size<T>(this T element, int? width = null, int? height = null) where T : VisualElement
		{
			try
			{
				if (width != null) element.style.width = width.Value;
				if (height != null) element.style.height = height.Value;
			}
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed setting size of {element:ref}"); }
			return element;
		}

		public static T Margin<T>(this T element, int size) where T : VisualElement => Margin(element, size, size, size, size);
		public static T Margin<T>(this T element, int top, int right, int bottom, int left) where T : VisualElement
		{
			try
			{
				element.style.marginTop = top;
				element.style.marginRight = right;
				element.style.marginBottom = bottom;
				element.style.marginLeft = left;
			}
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed setting margin of {element:ref}"); }
			return element;
		}
		public static T Margin<T>(this T element, int? top = null, int? right = null, int? bottom = null, int? left = null) where T : VisualElement
		{
			try
			{
				if (top != null) element.style.marginTop = top.Value;
				if (right != null) element.style.marginRight = right.Value;
				if (bottom != null) element.style.marginBottom = bottom.Value;
				if (left != null) element.style.marginLeft = left.Value;
			}
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed setting margin of {element:ref}"); }
			return element;
		}

		public static T Padding<T>(this T element, int size) where T : VisualElement => Padding(element, size, size, size, size);
		public static T Padding<T>(this T element, int top, int right, int bottom, int left) where T : VisualElement
		{
			try
			{
				element.style.paddingTop = top;
				element.style.paddingRight = right;
				element.style.paddingBottom = bottom;
				element.style.paddingLeft = left;
			}
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed setting padding of {element:ref}"); }
			return element;
		}
		public static T Padding<T>(this T element, int? top = null, int? right = null, int? bottom = null, int? left = null) where T : VisualElement
		{
			try
			{
				if (top != null) element.style.paddingTop = top.Value;
				if (right != null) element.style.paddingRight = right.Value;
				if (bottom != null) element.style.paddingBottom = bottom.Value;
				if (left != null) element.style.paddingLeft = left.Value;
			}
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed setting padding of {element:ref}"); }
			return element;
		}

		public static T BorderWidth<T>(this T element, int width) where T : VisualElement => BorderWidth(element, width, width, width, width);
		public static T BorderWidth<T>(this T element, int top, int right, int bottom, int left) where T : VisualElement
		{
			try
			{
				element.style.borderTopWidth = top;
				element.style.borderRightWidth = right;
				element.style.borderBottomWidth = bottom;
				element.style.borderLeftWidth = left;
			}
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed setting border width of {element:ref}"); }
			return element;
		}
		public static T BorderWidth<T>(this T element, int? top = null, int? right = null, int? bottom = null, int? left = null) where T : VisualElement
		{
			try
			{
				if (top != null) element.style.borderTopWidth = top.Value;
				if (right != null) element.style.borderRightWidth = right.Value;
				if (bottom != null) element.style.borderBottomWidth = bottom.Value;
				if (left != null) element.style.borderLeftWidth = left.Value;
			}
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed setting border width of {element:ref}"); }
			return element;
		}

		public static T BorderColor<T>(this T element, Color color) where T : VisualElement => BorderColor(element, color, color, color, color);
		public static T BorderColor<T>(this T element, Color top, Color right, Color bottom, Color left) where T : VisualElement
		{
			try
			{
				element.style.borderTopColor = top;
				element.style.borderRightColor = right;
				element.style.borderBottomColor = bottom;
				element.style.borderLeftColor = left;
			}
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed setting border color of {element:ref}"); }
			return element;
		}
		public static T BorderColor<T>(this T element, Color? top = null, Color? right = null, Color? bottom = null, Color? left = null) where T : VisualElement
		{
			try
			{
				if (top != null) element.style.borderTopColor = top.Value;
				if (right != null) element.style.borderRightColor = right.Value;
				if (bottom != null) element.style.borderBottomColor = bottom.Value;
				if (left != null) element.style.borderLeftColor = left.Value;
			}
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed setting border color of {element:ref}"); }
			return element;
		}

		public static T BorderRadius<T>(this T element, int radius) where T : VisualElement => BorderRadius(element, radius, radius, radius, radius);
		public static T BorderRadius<T>(this T element, int topLeft, int topRight, int bottomRight, int bottomLeft) where T : VisualElement
		{
			try
			{
				element.style.borderTopLeftRadius = topLeft;
				element.style.borderTopRightRadius = topRight;
				element.style.borderBottomRightRadius = bottomRight;
				element.style.borderBottomLeftRadius = bottomLeft;
			}
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed setting border radius of {element:ref}"); }
			return element;
		}
		public static T BorderRadius<T>(this T element, int? topLeft = null, int? topRight = null, int? bottomRight = null, int? bottomLeft = null) where T : VisualElement
		{
			try
			{
				if (topLeft != null) element.style.borderTopLeftRadius = topLeft.Value;
				if (topRight != null) element.style.borderTopRightRadius = topRight.Value;
				if (bottomRight != null) element.style.borderBottomRightRadius = bottomRight.Value;
				if (bottomLeft != null) element.style.borderBottomLeftRadius = bottomLeft.Value;
			}
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed setting border radius of {element:ref}"); }
			return element;
		}

		public static T Background<T>(this T element, Color color) where T : VisualElement
		{
			try { element.style.backgroundColor = color; }
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed setting background color of {element:ref}"); }
			return element;
		}
		public static T Background<T>(this T element, Texture2D image) where T : VisualElement
		{
			try { element.style.backgroundImage = image; }
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed setting background image of {element:ref}"); }
			return element;
		}
		public static T Background<T>(this T element, Texture2D image, BackgroundSizeType size) where T : VisualElement
		{
			try
			{
				element.style.backgroundImage = image;
				element.style.backgroundSize = new BackgroundSize(size);
			}
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed setting background image of {element:ref}"); }
			return element;
		}
	}
}