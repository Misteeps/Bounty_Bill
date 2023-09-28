using System;

using UnityEngine;
using UnityEngine.UIElements;


namespace Simplex.UI
{
	public abstract class Slider<T> : Field<T>
	{
		protected override string[] DefaultClasses => new string[] { "field", "slider" };

		public readonly Div gauge;
		public readonly Div fill;
		public readonly Div knob;

		public abstract Numeric<T> Numeric { get; }

		private bool dragging;

		public T Min { get; set; }
		public T Max { get; set; }
		public float Increments { get; set; } = 1;
		public bool Delayed { get; set; } = false;

		public override T CurrentValue
		{
			set
			{
				base.CurrentValue = value;
				fill.style.width = new Length(Numeric.LerpInverse(Min, Max, value) * 100, LengthUnit.Percent);
			}
		}


		public Slider()
		{
			gauge = this.Attach(new Div() { Classes = "gauge", Flexible = true });
			fill = gauge.Attach(new Div() { Classes = "fill" });
			knob = fill.Attach(new Div() { Classes = "knob" });

			Min = Numeric.Parse("0");
			Max = Numeric.Parse("10");

			RegisterCallback<PointerUpEvent>(OnPointerUp);
			RegisterCallback<PointerDownEvent>(OnPointerDown);
			RegisterCallback<PointerMoveEvent>(OnPointerMove);
		}

		protected virtual void OnPointerUp(PointerUpEvent evnt)
		{
			if (!dragging || evnt.button != 0) return;

			dragging = false;

			BindedValue = CalculateValue(worldBound, evnt.position.x);

			this.ReleasePointer(evnt.pointerId);
			evnt.StopPropagation();
		}
		protected virtual void OnPointerDown(PointerDownEvent evnt)
		{
			if (dragging || evnt.button != 0) return;

			dragging = true;

			if (Delayed) CurrentValue = CalculateValue(worldBound, evnt.position.x);
			else BindedValue = CalculateValue(worldBound, evnt.position.x);

			this.CapturePointer(evnt.pointerId);
			evnt.StopPropagation();
		}
		protected virtual void OnPointerMove(PointerMoveEvent evnt)
		{
			if (!dragging || !this.HasPointerCapture(evnt.pointerId)) return;

			if (Delayed) CurrentValue = CalculateValue(worldBound, evnt.position.x);
			else BindedValue = CalculateValue(worldBound, evnt.position.x);

			evnt.StopPropagation();
		}

		protected virtual T CalculateValue(Rect bounds, float position) => Numeric.Round(Numeric.Lerp(Min, Max, Mathf.InverseLerp(bounds.xMin, bounds.xMax, position)), Increments);
	}


	public class ByteSlider : Slider<byte> { public override Numeric<byte> Numeric { get; } = new ByteNumeric(); }
	public class SByteSlider : Slider<sbyte> { public override Numeric<sbyte> Numeric { get; } = new SByteNumeric(); }

	public class ShortSlider : Slider<short> { public override Numeric<short> Numeric { get; } = new ShortNumeric(); }
	public class UShortSlider : Slider<ushort> { public override Numeric<ushort> Numeric { get; } = new UShortNumeric(); }

	public class IntSlider : Slider<int> { public override Numeric<int> Numeric { get; } = new IntNumeric(); }
	public class UIntSlider : Slider<uint> { public override Numeric<uint> Numeric { get; } = new UIntNumeric(); }

	public class LongSlider : Slider<long> { public override Numeric<long> Numeric { get; } = new LongNumeric(); }
	public class ULongSlider : Slider<ulong> { public override Numeric<ulong> Numeric { get; } = new ULongNumeric(); }

	public class FloatSlider : Slider<float> { public override Numeric<float> Numeric { get; } = new FloatNumeric(); }
	public class DoubleSlider : Slider<double> { public override Numeric<double> Numeric { get; } = new DoubleNumeric(); }
	public class DecimalSlider : Slider<decimal> { public override Numeric<decimal> Numeric { get; } = new DecimalNumeric(); }
}