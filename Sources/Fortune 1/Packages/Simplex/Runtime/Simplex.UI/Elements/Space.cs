using System;

using UnityEngine;
using UnityEngine.UIElements;


namespace Simplex.UI
{
	public class HorizontalSpace : Div
	{
		protected override string[] DefaultClasses => new string[] { "space", "horizontal" };
		protected override Size DefaultSize => Size.Medium;


		public HorizontalSpace() { }
		public HorizontalSpace(Size size) => Size = size;
	}

	public class VerticalSpace : Div
	{
		protected override string[] DefaultClasses => new string[] { "space", "vertical" };
		protected override Size DefaultSize => Size.Medium;


		public VerticalSpace() { }
		public VerticalSpace(Size size) => Size = size;
	}
}