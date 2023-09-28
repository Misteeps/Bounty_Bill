using System;

using UnityEngine;
using UnityEngine.UIElements;


namespace Simplex.UI
{
	public class DirectoryElement : CollectionElement, ICollapsible
	{
		protected override string[] DefaultClasses => new string[] { "directory-element" };
		public override VisualElement contentContainer => body;

		public readonly Div header;
		public readonly Div body;

		public readonly Div arrow;
		public readonly Div check;
		public readonly Div icon;
		public readonly Label title;

		public virtual string Title { get => title.Text; set => title.Text = value; }
		public virtual Texture2D Icon
		{
			get => icon.resolvedStyle.backgroundImage.texture;
			set
			{
				if (value == null)
				{
					icon.style.backgroundImage = StyleKeyword.Null;
					icon.Enable(false);
				}
				else
				{
					icon.style.backgroundImage = value;
					icon.Enable(true);
				}
			}
		}
		public virtual bool Selected
		{
			get => ClassListContains("selected");
			set
			{
				EnableInClassList("selected", value);
				check.Enable(value);
			}
		}
		public virtual bool Collapsed
		{
			get => ClassListContains("collapsed");
			set => EnableInClassList("collapsed", value);
		}

		public Action onClick;


		public DirectoryElement()
		{
			header = hierarchy.Attach(new Div() { Classes = "header", Pickable = true });
			body = hierarchy.Attach(new Div() { Classes = "body" });

			arrow = header.Attach(new Div() { Name = "arrow", Classes = "icon" }.Enable(false));
			check = header.Attach(new Div() { Name = "check", Classes = "icon" }.Enable(false));
			icon = header.Attach(new Div() { Name = "icon", Classes = "icon" }.Enable(false));
			title = header.Attach(new Label() { Name = "title", Pickable = true, Flexible = true });

			header.RegisterCallback<ClickEvent>(OnClick);
			arrow.RegisterCallback<ClickEvent>(OnCollapse);
			RegisterCallback<RefreshEvent>(OnRefresh);
		}

		public virtual DirectoryElement Bind(string path, object item)
		{
			if (item is UnityEngine.Object obj)
			{
#if UNITY_EDITOR
				return Bind(obj.name.TitleCase(), (Texture2D)UnityEditor.EditorGUIUtility.ObjectContent(obj, obj.GetType()).image, item);
#else
				return Bind(obj.name.TitleCase(), null, item);
#endif
			}

			return Bind(System.IO.Path.GetFileName(path).TitleCase(), null, item);
		}
		public virtual DirectoryElement Bind(string title, Texture2D icon, object item)
		{
			body.Clear();

			Title = title;
			Icon = icon;

			return this.Refresh();
		}

		protected virtual void OnClick(ClickEvent evnt)
		{
			if (onClick == null) OnCollapse(evnt);
			else
			{
				Selected = !Selected;
				onClick.Invoke();
			}

			evnt.StopPropagation();
		}
		protected virtual void OnCollapse(ClickEvent evnt)
		{
			if (!arrow.enabledSelf) return;
			this.Collapse(!Collapsed, evnt.altKey);
			evnt.StopPropagation();
		}
		protected virtual void OnRefresh(RefreshEvent evnt) => arrow.Enable(childCount != 0);
	}
}