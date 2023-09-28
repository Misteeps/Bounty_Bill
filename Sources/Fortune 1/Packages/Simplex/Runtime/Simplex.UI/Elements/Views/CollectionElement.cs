using System;

using UnityEngine;
using UnityEngine.UIElements;


namespace Simplex.UI
{
	public class CollectionElement : Div
	{
		protected override string[] DefaultClasses => new string[] { $"collection-element" };
		protected override bool DefaultPickable => true;
		protected override bool RefreshOnAttach => true;

		protected ContextualMenuManipulator contextMenu;

		public virtual int Index { get; set; }
		public virtual UnityEngine.Object Origin { get; set; }
		public virtual ContextualMenuManipulator ContextMenu
		{
			get => contextMenu;
			set
			{
				this.RemoveManipulator(contextMenu);
				contextMenu = value;
				this.AddManipulator(contextMenu);
			}
		}


		public virtual CollectionElement Bind(object item)
		{
			Clear();

			this.Attach(new Label() { Flexible = true, Text = item?.ToString() });

			return this.Refresh();
		}
	}
}