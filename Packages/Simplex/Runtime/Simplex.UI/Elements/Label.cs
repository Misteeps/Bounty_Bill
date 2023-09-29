using System;
using System.Linq;

using UnityEngine;
using UnityEngine.UIElements;


namespace Simplex.UI
{
	public class Label : TextElement
	{
		protected virtual string[] DefaultClasses => new string[] { "label", "text" };
		protected virtual Size DefaultSize => Size.Medium;
		protected virtual bool DefaultFocusable => false;
		protected virtual bool DefaultPickable => false;
		protected virtual bool DefaultNavigable => false;
		protected virtual bool DefaultFlexible => false;
		protected virtual UsageHints DefaultUsageHints => UsageHints.None;
		protected virtual bool RefreshOnAttach => false;

		public virtual string Name { get => base.name; set => base.name = value; }
		public virtual string Tooltip { get => base.tooltip; set => base.tooltip = value; }
		public virtual string Classes
		{
			get => string.Join(", ", GetClasses());
			set => this.Class(false, value.Split(',').Select(s => s.Trim()).ToArray());
		}
		public virtual Size Size
		{
			get
			{
				foreach (Size size in Enum.GetValues(typeof(Size)))
					if (ClassListContains(size.ToString().ToLower()))
						return size;

				return Size.Null;
			}
			set
			{
				foreach (Size size in Enum.GetValues(typeof(Size)))
					RemoveFromClassList(size.ToString().ToLower());

				if (value != Size.Null)
					AddToClassList(value.ToString().ToLower());
			}
		}
		public virtual bool Focusable { get => base.focusable; set => base.focusable = value; }
		public virtual bool Pickable { get => base.pickingMode == PickingMode.Position; set => base.pickingMode = (value) ? PickingMode.Position : PickingMode.Ignore; }
		public virtual bool Navigable { get; set; }
		public virtual bool Flexible { get => ClassListContains("flexible"); set => EnableInClassList("flexible", value); }
		public virtual UsageHints UsageHints { get => base.usageHints; set => base.usageHints = value; }
		public virtual bool RichText { get => base.enableRichText; set => base.enableRichText = value; }
		public virtual string Text { get => base.text; set => base.text = value; }


		public Label()
		{
			this.Class(true, DefaultClasses);

			Size = DefaultSize;
			Focusable = DefaultFocusable;
			Pickable = DefaultPickable;
			Navigable = DefaultNavigable;
			Flexible = DefaultFlexible;
			UsageHints = DefaultUsageHints;
			RichText = true;

			if (RefreshOnAttach)
				RegisterCallback<AttachToPanelEvent>(_ => this.Refresh());
		}
		public Label(string text) : this() { Text = text; }
	}


	#region Labeled
	public class Labeled : Div
	{
		public enum Shape { None, Bar, Square, Circle }
		public enum Color { None, White, Black, Red, Orange, Yellow, Green, Cyan, Blue, Purple, Pink }

		protected override string[] DefaultClasses => new string[] { "labeled" };
		protected override Size DefaultSize => Size.Medium;

		public Div tag;
		public readonly Label label;

		public bool Highlight
		{
			get => ClassListContains("highlight");
			set
			{
				EnableInClassList("highlight", value);
				pickingMode = (value) ? PickingMode.Position : PickingMode.Ignore;
			}
		}
		public Shape TagShape
		{
			get
			{
				if (tag != null)
					foreach (Shape shape in Enum.GetValues(typeof(Shape)))
						if (tag.ClassListContains(shape.ToString().ToLower()))
							return shape;

				return Shape.None;
			}
			set
			{
				if (value == Shape.None) tag?.RemoveFromHierarchy();
				else
				{
					tag ??= this.Attach(0, new Div() { Classes = "tag, bar, white" });

					foreach (Shape shape in Enum.GetValues(typeof(Shape)))
						tag.RemoveFromClassList(shape.ToString().ToLower());

					tag.AddToClassList(value.ToString().ToLower());
				}
			}
		}
		public Color TagColor
		{
			get
			{
				if (tag != null)
					foreach (Color color in Enum.GetValues(typeof(Color)))
						if (tag.ClassListContains(color.ToString().ToLower()))
							return color;

				return Color.None;
			}
			set
			{
				if (value == Color.None) tag?.RemoveFromHierarchy();
				else
				{
					tag ??= this.Attach(0, new Div() { Classes = "tag, bar, white" });

					foreach (Color color in Enum.GetValues(typeof(Color)))
						tag.RemoveFromClassList(color.ToString().ToLower());

					tag.AddToClassList(value.ToString().ToLower());
				}
			}
		}
		public bool RichText { get => label.enableRichText; set => label.enableRichText = value; }
		public string Text { get => label.Text; set => label.Text = value; }


		public Labeled()
		{
			label = this.Attach(new Label());

			Highlight = true;
		}
		public Labeled(string text) : this()
		{
			Text = text;

			if (Name.Empty())
				this.Name(Text);
		}
		public Labeled(params VisualElement[] elements) : this()
		{
			if (elements.Empty())
				return;

			if (elements.Length == 1)
			{
				VisualElement element = elements[0];
				this.Attach(element.Flex(true));

				if (Text.Empty() && element is IBindable bindable && bindable.IValue != null)
					Text = bindable.IValue.Name.TitleCase();

				return;
			}

			for (int i = 0; i < elements.Length; i++)
			{
				VisualElement element = elements[i];
				if (element == null) continue;

				if (i == 0) element.AddToClassList("first");
				else if (i == elements.Length - 1) element.AddToClassList("last");
				else element.AddToClassList("middle");

				this.Attach(element);
			}
		}
		public Labeled(string text, params VisualElement[] elements) : this(elements)
		{
			Text = text;

			if (Name.Empty())
				this.Name(Text);
		}
	}
	#endregion Labeled
}