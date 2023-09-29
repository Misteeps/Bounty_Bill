using System;

using UnityEngine;
using UnityEngine.UIElements;


namespace Simplex.UI
{
	public abstract class InputSlider<T> : Field<T>
	{
		protected override string[] DefaultClasses => new string[] { "field", "input-slider" };

		protected abstract NumericInput<T> InputFactory { get; }
		protected abstract Slider<T> SliderFactory { get; }

		public readonly NumericInput<T> input;
		public readonly Slider<T> slider;

		public T Min { get => input.Min; set => (input.Min, slider.Min) = (value, value); }
		public T Max { get => input.Max; set => (input.Max, slider.Max) = (value, value); }
		public T InputMin { get => input.Min; set => input.Min = value; }
		public T InputMax { get => input.Max; set => input.Max = value; }
		public T SliderMin { get => slider.Min; set => slider.Min = value; }
		public T SliderMax { get => slider.Max; set => slider.Max = value; }
		public float SliderIncrements { get => slider.Increments; set => slider.Increments = value; }
		public bool Delayed { get; set; } = false;

		public override T CurrentValue
		{
			set
			{
				base.CurrentValue = value;
				input.CurrentValue = value;
				slider.CurrentValue = value;
			}
		}


		public InputSlider()
		{
			DelegateValue<T> iValue = new DelegateValue<T>(() => CurrentValue, value =>
			{
				if (Delayed) CurrentValue = input.Numeric.Clamp(Min, Max, value);
				else BindedValue = input.Numeric.Clamp(Min, Max, value);
			});

			input = this.Attach(InputFactory.Bind(iValue));
			slider = this.Attach(SliderFactory.Flex(true).Bind(iValue));
		}
	}


	public class ByteInputSlider : InputSlider<byte> { protected override NumericInput<byte> InputFactory => new ByteInput(); protected override Slider<byte> SliderFactory => new ByteSlider(); }
	public class SByteInputSlider : InputSlider<sbyte> { protected override NumericInput<sbyte> InputFactory => new SByteInput(); protected override Slider<sbyte> SliderFactory => new SByteSlider(); }

	public class ShortInputSlider : InputSlider<short> { protected override NumericInput<short> InputFactory => new ShortInput(); protected override Slider<short> SliderFactory => new ShortSlider(); }
	public class UShortInputSlider : InputSlider<ushort> { protected override NumericInput<ushort> InputFactory => new UShortInput(); protected override Slider<ushort> SliderFactory => new UShortSlider(); }

	public class IntInputSlider : InputSlider<int> { protected override NumericInput<int> InputFactory => new IntInput(); protected override Slider<int> SliderFactory => new IntSlider(); }
	public class UIntInputSlider : InputSlider<uint> { protected override NumericInput<uint> InputFactory => new UIntInput(); protected override Slider<uint> SliderFactory => new UIntSlider(); }

	public class LongInputSlider : InputSlider<long> { protected override NumericInput<long> InputFactory => new LongInput(); protected override Slider<long> SliderFactory => new LongSlider(); }
	public class ULongInputSlider : InputSlider<ulong> { protected override NumericInput<ulong> InputFactory => new ULongInput(); protected override Slider<ulong> SliderFactory => new ULongSlider(); }

	public class FloatInputSlider : InputSlider<float> { protected override NumericInput<float> InputFactory => new FloatInput(); protected override Slider<float> SliderFactory => new FloatSlider(); }
	public class DoubleInputSlider : InputSlider<double> { protected override NumericInput<double> InputFactory => new DoubleInput(); protected override Slider<double> SliderFactory => new DoubleSlider(); }
	public class DecimalInputSlider : InputSlider<decimal> { protected override NumericInput<decimal> InputFactory => new DecimalInput(); protected override Slider<decimal> SliderFactory => new DecimalSlider(); }
}