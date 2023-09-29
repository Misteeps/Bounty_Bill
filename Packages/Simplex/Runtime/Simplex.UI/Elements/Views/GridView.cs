using System;

using UnityEngine;
using UnityEngine.UIElements;


namespace Simplex.UI
{
	public class GridView<TItem> : GridView<TItem, GridElement> { }
	public class GridView<TItem, TElement> : CollectionView<TItem, TElement> where TElement : GridElement, new()
	{
		protected override string Type => "grid";
		public override VisualElement contentContainer => grid;

		public readonly Div grid;

		public Size TileSize { get => grid.Size; set => grid.Size = value; }


		public GridView()
		{
			grid = body.Attach(new Div() { Classes = "grid" });

			TileSize = Size.Small;
		}
	}
}