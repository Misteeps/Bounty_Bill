using System;

using UnityEngine;
using UnityEngine.UIElements;


namespace Simplex.UI
{
	public class CollapsibleView : Div, ICollapsible
	{
		protected override string[] DefaultClasses => new string[] { "collapsible-view" };
		public override VisualElement contentContainer => body;

		public readonly Div header;
		public readonly VerticalScrollView body;

		public readonly Button arrow;
		public readonly Label title;

		public virtual string Title { get => title.Text; set => title.Text = value; }
		public virtual int MaxHeight
		{
			get => (body.style.maxHeight == StyleKeyword.None) ? -1 : (int)body.style.maxHeight.value.value;
			set => body.style.maxHeight = (value == -1) ? StyleKeyword.None : value;
		}
		public virtual bool Collapsed
		{
			get => ClassListContains("collapsed");
			set => EnableInClassList("collapsed", value);
		}


		public CollapsibleView()
		{
			header = hierarchy.Attach(new Div() { Classes = "header, toolbar" });
			body = hierarchy.Attach(new VerticalScrollView() { Classes = "body" });

			arrow = header.Attach(new Button() { Name = "arrow", Classes = "icon" }.Bind(e => this.Collapse(!Collapsed, e.altKey)));
			header.Attach(new HorizontalSpace(Size.Mini));
			title = header.Attach(new Label() { Name = "title", Flexible = true });
		}
	}
}