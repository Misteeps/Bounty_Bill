using System;
using System.Linq;

using UnityEngine;
using UnityEngine.UIElements;


namespace Simplex.UI
{
	public class Div : VisualElement
	{
		protected virtual string[] DefaultClasses => null;
		protected virtual Size DefaultSize => Size.Null;
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


		public Div()
		{
			this.Class(true, DefaultClasses);

			Size = DefaultSize;
			Focusable = DefaultFocusable;
			Pickable = DefaultPickable;
			Navigable = DefaultNavigable;
			Flexible = DefaultFlexible;
			UsageHints = DefaultUsageHints;

			if (RefreshOnAttach)
				RegisterCallback<AttachToPanelEvent>(_ => this.Refresh());
		}
	}
}