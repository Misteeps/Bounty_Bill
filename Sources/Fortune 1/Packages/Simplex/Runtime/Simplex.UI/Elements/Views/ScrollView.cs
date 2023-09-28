using System;

using UnityEngine;
using UnityEngine.UIElements;


namespace Simplex.UI
{
	public abstract class ScrollView : Div
	{
		protected override bool DefaultPickable => true;
		public override VisualElement contentContainer => contents;

		public readonly Div container;
		public readonly Div contents;


		public ScrollView()
		{
			container = hierarchy.Attach(new Div() { Classes = "container", Flexible = true });
			contents = container.Attach(new Div() { Classes = "contents" });

			RegisterCallback<WheelEvent>(OnScroll);
		}

		protected abstract void OnScroll(WheelEvent evnt);
	}


	#region Horizontal
	public class HorizontalScrollView : ScrollView
	{
		protected override string[] DefaultClasses => new string[] { "scroll-view", "horizontal" };

		public readonly ScrollBar scrollbar;


		public HorizontalScrollView()
		{
			scrollbar = hierarchy.Attach(new ScrollBar() { Vertical = false }.Bind(contents));
		}

		protected override void OnScroll(WheelEvent evnt)
		{
			if (!scrollbar.Active) return;

			if ((evnt.delta.y < 0 && scrollbar.Factor != 0) || (evnt.delta.y > 0 && scrollbar.Factor != 1))
				evnt.StopPropagation();

			scrollbar.Position += (int)(40 * evnt.delta.y);
		}
	}
	#endregion Horizontal

	#region Vertical
	public class VerticalScrollView : ScrollView
	{
		protected override string[] DefaultClasses => new string[] { "scroll-view", "vertical" };

		public readonly ScrollBar scrollbar;


		public VerticalScrollView()
		{
			scrollbar = hierarchy.Attach(new ScrollBar() { Vertical = true }.Bind(contents));
		}

		protected override void OnScroll(WheelEvent evnt)
		{
			if (!scrollbar.Active) return;

			if ((evnt.delta.y < 0 && scrollbar.Factor != 0) || (evnt.delta.y > 0 && scrollbar.Factor != 1))
				evnt.StopPropagation();

			scrollbar.Position += (int)(40 * evnt.delta.y);
		}
	}
	#endregion Vertical

	#region Dynamic
	internal class DynamicScrollView : ScrollView
	{
		protected override string[] DefaultClasses => new string[] { "scroll-view", "dynamic" };

		public readonly ScrollBar vertical;
		public readonly ScrollBar horizontal;


		public DynamicScrollView()
		{
			vertical = hierarchy.Attach(new ScrollBar() { Vertical = true }.Bind(contents));
			horizontal = hierarchy.Attach(new ScrollBar() { Vertical = false }.Bind(contents));

			throw new NotImplementedException($"Dynamic Scroll View under construction");
		}

		protected override void OnScroll(WheelEvent evnt) { }
	}
	#endregion Dynamic
}