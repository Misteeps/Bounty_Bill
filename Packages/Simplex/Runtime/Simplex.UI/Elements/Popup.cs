using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;


namespace Simplex.UI
{
	public class Popup : Div
	{
		protected override string[] DefaultClasses => new string[] { "popup-background" };
		protected override bool DefaultFocusable => true;
		protected override bool DefaultPickable => true;
		public override VisualElement contentContainer => window;

		public readonly Div window;

		private VisualElement sourceElement;

		public VisualElement SourceElement
		{
			get => sourceElement;
			set
			{
				sourceElement?.UnregisterCallback<DetachFromPanelEvent>(OnSourceDetach);
				sourceElement = value;
				sourceElement?.RegisterCallback<DetachFromPanelEvent>(OnSourceDetach);
			}
		}
		public StyleLength Top { get => window.style.top; set => window.style.top = value; }
		public StyleLength Bottom { get => window.style.bottom; set => window.style.bottom = value; }
		public StyleLength Left { get => window.style.left; set => window.style.left = value; }
		public StyleLength Right { get => window.style.right; set => window.style.right = value; }
		public StyleLength Width { get => window.style.width; set => window.style.width = value; }
		public StyleLength MinWidth { get => window.style.minWidth; set => window.style.minWidth = value; }
		public StyleLength MaxWidth { get => window.style.maxWidth; set => window.style.maxWidth = value; }
		public StyleLength Height { get => window.style.height; set => window.style.height = value; }
		public StyleLength MinHeight { get => window.style.minHeight; set => window.style.minHeight = value; }
		public StyleLength MaxHeight { get => window.style.maxHeight; set => window.style.maxHeight = value; }
		public bool Fit { get; set; } = true;

		public event Action Opened;
		public event Action Closed;


		public Popup()
		{
			window = hierarchy.Attach(new Div() { Classes = "popup-window", Pickable = true });
			window.RegisterCallback<ClickEvent>(e => e.StopPropagation());

			RegisterCallback<ClickEvent>(_ => Close());
			RegisterCallback<KeyDownEvent>(e => { if (e.keyCode is KeyCode.Escape) { Close(); e.StopPropagation(); } });
			RegisterCallback<DetachFromPanelEvent>(OnDetach);
			RegisterCallback<RefreshEvent>(OnRefresh);
		}

		public virtual Popup Open(IPanel panel)
		{
			if (this.panel != null) Close();
			panel.visualTree.Add(this);

			Opened?.Invoke();
			return this.Refresh();
		}
		public virtual Popup Open(VisualElement source, bool inheritStyleSheets = true, bool align = true)
		{
			if (this.panel != null) Close();
			if (source == null) throw new NullReferenceException($"Null element").Overwrite(ConsoleUtilities.uiTag, $"Failed displaying popup under {source:ref}");
			if (source.panel == null) throw new Exception("Element not attached to panel").Overwrite(ConsoleUtilities.uiTag, $"Failed displaying popup under {source:ref}");

			if (inheritStyleSheets)
			{
				List<StyleSheet> styleSheets = new List<StyleSheet>();
				for (VisualElement element = source; element != null; element = element.hierarchy.parent)
					for (int j = element.styleSheets.count - 1; j >= 0; j--)
						styleSheets.Insert(0, element.styleSheets[j]);

				foreach (StyleSheet styleSheet in styleSheets)
					this.styleSheets.Add(styleSheet);
			}

			if (align)
			{
				Rect bounds = source.worldBound;
				Top = bounds.yMax + 1;
				Bottom = StyleKeyword.Auto;
				Left = bounds.xMin;
				Right = StyleKeyword.Auto;
				Width = bounds.width;
			}

			SourceElement = source;
			return Open(source.panel);
		}

		public virtual void Close() => RemoveFromHierarchy();

		protected virtual void OnSourceDetach(DetachFromPanelEvent evnt) => Close();
		protected virtual void OnDetach(DetachFromPanelEvent evnt)
		{
			Closed?.Invoke();
			SourceElement = null;
		}
		protected virtual void OnRefresh(RefreshEvent evnt)
		{
			if (!Fit) return;
			if (float.IsNaN(worldBound.width) || float.IsNaN(worldBound.height))
			{
				schedule.Execute(() => this.Refresh()).ExecuteLater(1);
				return;
			}

			Rect parentBounds = this.worldBound;
			Rect windowBounds = window.worldBound;

			if (parentBounds.Contains(windowBounds.min) && parentBounds.Contains(windowBounds.max))
				return;

			window.style.top = Mathf.Clamp(windowBounds.yMin, parentBounds.yMin, parentBounds.yMax - windowBounds.height);
			window.style.left = Mathf.Clamp(windowBounds.xMin, parentBounds.xMin, parentBounds.xMax - windowBounds.width);
		}
	}
}