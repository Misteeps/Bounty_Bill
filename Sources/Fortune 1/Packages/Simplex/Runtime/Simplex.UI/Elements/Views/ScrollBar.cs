using System;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;


namespace Simplex.UI
{
	public class ScrollBar : Div
	{
		protected override string[] DefaultClasses => new string[] { "scroll-bar", "vertical" };
		protected override bool DefaultFocusable => true;
		protected override bool DefaultPickable => true;

		public readonly Div startArrow;
		public readonly Div endArrow;
		public readonly Div track;
		public readonly Div bar;

		private (int min, int max) barBounds;
		private (int min, int max) trackBounds;
		private (int min, int max) targetBounds;

		private int targetPosition;
		private float clickOffset;
		private bool dragging;
		private bool locked;

		public VisualElement Container { get; private set; }
		public VisualElement Target { get; private set; }
		public bool Vertical
		{
			get => ClassListContains("vertical");
			set => this.ClassToggle("vertical", "horizontal", value);
		}
		public bool Locked
		{
			get => locked;
			set
			{
				locked = value;
				if (locked)
					Active = false;
			}
		}
		public bool Active
		{
			get => enabledSelf;
			private set
			{
				SetEnabled(!locked && value);

				if (!enabledSelf)
				{
					barBounds = (0, 0);
					trackBounds = (0, 0);
					targetBounds = (0, 0);

					targetPosition = 0;
					if (Target != null) Target.style.translate = new Translate(0, 0, 0);
				}
			}
		}

		public int Position
		{
			get => targetPosition;
			set
			{
				if (!Active || targetPosition == value) return;
				targetPosition = Mathf.Clamp(value, targetBounds.min, targetBounds.max);

				if (Vertical)
				{
					bar.style.translate = new Translate(0, Mathf.Lerp(barBounds.min, barBounds.max, Factor), 0);
					Target.style.translate = new Translate(0, -targetPosition, 0);
				}
				else
				{
					bar.style.translate = new Translate(Mathf.Lerp(barBounds.min, barBounds.max, Factor), 0, 0);
					Target.style.translate = new Translate(-targetPosition, 0, 0);
				}
			}
		}
		public float Factor
		{
			get => Mathf.InverseLerp(targetBounds.min, targetBounds.max, targetPosition);
			set => Position = Mathf.RoundToInt(Mathf.Lerp(targetBounds.min, targetBounds.max, value));
		}

		private static readonly PropertyInfo BoundingBoxProperty = typeof(VisualElement).GetProperty("boundingBox", BindingFlags.Instance | BindingFlags.NonPublic);


		public ScrollBar()
		{
			startArrow = this.Attach(new Div() { Name = "start", Classes = "arrow, icon" });
			track = this.Attach(new Div() { Classes = "track", Flexible = true });
			bar = track.Attach(new Div() { Classes = "bar" });
			endArrow = this.Attach(new Div() { Name = "end", Classes = "arrow, icon" });

			RegisterCallback<PointerUpEvent>(OnPointerUp);
			RegisterCallback<PointerDownEvent>(OnPointerDown);
			RegisterCallback<PointerMoveEvent>(OnPointerMove);
			RegisterCallback<AttachToPanelEvent>(OnAttach);
			RegisterCallback<DetachFromPanelEvent>(OnDetach);

			Active = false;
		}

		public ScrollBar Bind(VisualElement target)
		{
			Container?.UnregisterCallback<GeometryChangedEvent>(OnGeometryChange);
			Target?.UnregisterCallback<GeometryChangedEvent>(OnGeometryChange);

			Container = target?.parent;
			Target = target;

			Container?.RegisterCallback<GeometryChangedEvent>(OnGeometryChange);
			Target?.RegisterCallback<GeometryChangedEvent>(OnGeometryChange);

			return this;
		}

		protected virtual void OnPointerUp(PointerUpEvent evnt)
		{
			if (!dragging || evnt.button != 0) return;

			dragging = false;
			bar.style.transitionDuration = StyleKeyword.Null;

			this.ReleasePointer(evnt.pointerId);
			evnt.StopPropagation();
		}
		protected virtual void OnPointerDown(PointerDownEvent evnt)
		{
			if (dragging || evnt.button != 0) return;

			dragging = true;
			bar.style.transitionDuration = new StyleList<TimeValue>(new List<TimeValue>() { new TimeValue(0.1f) });

			if (Vertical)
			{
				clickOffset = bar.worldBound.center.y - evnt.position.y;
				if (Mathf.Abs(clickOffset) > bar.resolvedStyle.height / 2)
					clickOffset = 0;

				Factor = Mathf.InverseLerp(trackBounds.min, trackBounds.max, evnt.localPosition.y + clickOffset);
			}
			else
			{
				clickOffset = bar.worldBound.center.x - evnt.position.x;
				if (Mathf.Abs(clickOffset) > bar.resolvedStyle.width / 2)
					clickOffset = 0;

				Factor = Mathf.InverseLerp(trackBounds.min, trackBounds.max, evnt.localPosition.x + clickOffset);
			}

			this.CapturePointer(evnt.pointerId);
			evnt.StopPropagation();
		}
		protected virtual void OnPointerMove(PointerMoveEvent evnt)
		{
			if (!dragging || !this.HasPointerCapture(evnt.pointerId)) return;

			Factor = Mathf.InverseLerp(trackBounds.min, trackBounds.max, ((Vertical) ? evnt.localPosition.y : evnt.localPosition.x) + clickOffset);

			evnt.StopPropagation();
		}
		protected virtual void OnAttach(AttachToPanelEvent evnt)
		{
			Container?.UnregisterCallback<GeometryChangedEvent>(OnGeometryChange);
			Target?.UnregisterCallback<GeometryChangedEvent>(OnGeometryChange);

			Container?.RegisterCallback<GeometryChangedEvent>(OnGeometryChange);
			Target?.RegisterCallback<GeometryChangedEvent>(OnGeometryChange);
		}
		protected virtual void OnDetach(DetachFromPanelEvent evnt)
		{
			Container?.UnregisterCallback<GeometryChangedEvent>(OnGeometryChange);
			Target?.UnregisterCallback<GeometryChangedEvent>(OnGeometryChange);
		}
		protected virtual void OnGeometryChange(GeometryChangedEvent evnt)
		{
			if (Vertical)
			{
				if (evnt.oldRect.height != evnt.newRect.height)
					CalculateScrollArea();
			}
			else
			{
				if (evnt.oldRect.width != evnt.newRect.width)
					CalculateScrollArea();
			}
		}

		public virtual void CalculateScrollArea()
		{
			int length = 0;
			int view = 0;
			if (Target == null || Container == null || float.IsNaN(Container.layout.height) || float.IsNaN(Container.layout.width)) Active = false;
			else
			{
				Rect boundingBox = (Rect)BoundingBoxProperty.GetValue(Target);
				if (Vertical)
				{
					length = (int)boundingBox.height;
					view = (int)Container.layout.height;
				}
				else
				{
					length = (int)boundingBox.width;
					view = (int)Container.layout.width;
				}
				Active = length > view;
			}

			if (!Active)
				return;

			Rect trackLocalBound = track.localBound;
			trackBounds = (Vertical) ? ((int)trackLocalBound.yMin, (int)trackLocalBound.yMax) : ((int)trackLocalBound.xMin, (int)trackLocalBound.xMax);

			int barLength = Mathf.RoundToInt(Mathf.Lerp(0, trackBounds.max - trackBounds.min, Mathf.Clamp(Mathf.InverseLerp(800, 0, length - view), 0.2f, 1)));
			int barMargin = Mathf.RoundToInt(barLength * 0.5f);

			barBounds = (barMargin, (trackBounds.max - trackBounds.min) - barMargin);
			trackBounds = (trackBounds.min + barMargin, trackBounds.max - barMargin);
			targetBounds = (0, length - view);

			if (Vertical)
			{
				bar.style.marginTop = -barMargin;
				bar.style.marginLeft = StyleKeyword.Null;
				bar.style.height = barLength;
				bar.style.width = new Length(100, LengthUnit.Percent);
				bar.style.translate = new Translate(0, Mathf.Lerp(barBounds.min, barBounds.max, Factor), 0);
				Target.style.translate = new Translate(0, -targetPosition, 0);

			}
			else
			{
				bar.style.marginTop = StyleKeyword.Null;
				bar.style.marginLeft = -barMargin;
				bar.style.height = new Length(100, LengthUnit.Percent);
				bar.style.width = barLength;
				bar.style.translate = new Translate(Mathf.Lerp(barBounds.min, barBounds.max, Factor), 0, 0);
				Target.style.translate = new Translate(-targetPosition, 0, 0);
			}
		}
	}
}