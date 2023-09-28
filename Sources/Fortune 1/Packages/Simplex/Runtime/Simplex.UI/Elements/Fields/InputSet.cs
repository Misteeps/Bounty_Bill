using System;
using System.Linq;
using System.Globalization;

using UnityEngine;
using UnityEngine.UIElements;


namespace Simplex.UI
{
	public abstract class InputSet<TValue, TInput, TField> : Field<TValue> where TField : Input<TInput>, new()
	{
		protected override string[] DefaultClasses => new string[] { "field", "input-set" };
		protected virtual int ForceCount => -1;

		protected TField[] fields;
		private bool flexibleFields;
		private int maxLength;
		private bool readOnly;
		private bool delayed;

		public virtual int Count
		{
			get => fields.Length;
			set
			{
				if (ForceCount != -1) throw new Exception($"Count not settable").Overwrite(ConsoleUtilities.uiTag, $"Failed setting input set count of {this:ref}");
				SetFieldsCount(value);
			}
		}
		public bool FlexibleFields
		{
			get => flexibleFields;
			set
			{
				flexibleFields = value;
				if (!fields.Empty())
					foreach (TField field in fields)
						field.Flexible = value;
			}
		}
		public int MaxLength
		{
			get => maxLength;
			set
			{
				maxLength = value;
				if (!fields.Empty())
					foreach (TField field in fields)
						field.MaxLength = value;
			}
		}
		public bool ReadOnly
		{
			get => readOnly;
			set
			{
				readOnly = value;
				if (!fields.Empty())
					foreach (TField field in fields)
						field.ReadOnly = value;
			}
		}
		public bool Delayed
		{
			get => delayed;
			set
			{
				delayed = value;
				if (!fields.Empty())
					foreach (TField field in fields)
						field.Delayed = value;
			}
		}

		public override TValue CurrentValue
		{
			set
			{
				base.CurrentValue = value;

				for (int i = 0; i < Count; i++)
					fields[i].CurrentValue = GetInput(i);
			}
		}


		public InputSet()
		{
			FlexibleFields = true;
			MaxLength = -1;
			ReadOnly = false;
			Delayed = false;

			SetFieldsCount(Mathf.Max(0, ForceCount));
		}

		private void SetFieldsCount(int count)
		{
			TField[] newFields = new TField[count];
			for (int i = 0; i < count; i++)
				newFields[i] = (fields.OutOfRange(i)) ? NewField(i) : fields[i];

			fields = newFields;
			SetFieldsClasses();
		}
		protected virtual void SetFieldsClasses()
		{
			for (int i = 0; i < Count; i++)
			{
				TField field = fields[i];

				field.RemoveFromClassList("first");
				field.RemoveFromClassList("middle");
				field.RemoveFromClassList("last");

				bool Edge(int index) => index < 0 || index >= childCount || this[index] is not TField;
				bool first = Edge(this.IndexOf(field) - 1);
				bool last = Edge(this.IndexOf(field) + 1);

				if (first && last) return;
				else if (first) field.AddToClassList("first");
				else if (last) field.AddToClassList("last");
				else field.AddToClassList("middle");
			}
		}

		protected virtual TField NewField(int index)
		{
			TField field = new TField() { Name = $"input{index}", Flexible = FlexibleFields, MaxLength = MaxLength, Multiline = false, ReadOnly = ReadOnly, Delayed = Delayed, AutoSelectAll = true };
			field.Bind(() => GetInput(index), value => { if (Delayed) CurrentValue = SetInput(index, value); else BindedValue = SetInput(index, value); });
			field.RegisterCallback<KeyDownEvent>(evnt => OnKeyDown(index, evnt));
			return this.Attach(field);
		}
		public TField GetField(int index) => fields[index];

		protected virtual async void OnKeyDown(int index, KeyDownEvent evnt)
		{
			switch (evnt.keyCode)
			{
				case KeyCode.LeftArrow:
					if (MaxLength != 1)
					{
						TField field = fields[index];
						int start = field.Selection.cursorIndex;
						int end = field.Selection.selectIndex;
						if (start != end || start > 0) return;
					}
					Focus(index - 1);
					break;

				case KeyCode.RightArrow:
					if (MaxLength != 1)
					{
						TField field = fields[index];
						int start = field.Selection.cursorIndex;
						int end = field.Selection.selectIndex;
						int length = field.Text.Length;
						if (start != end || start < length) return;
					}
					Focus(index + 1);
					break;

				case KeyCode.Tab:
				case KeyCode.Return:
				case KeyCode.KeypadEnter:
					await Awaitable.NextFrameAsync();
					Focus(index + 1);
					break;

				case KeyCode.Backspace:
					if (MaxLength == 1)
					{
						if (evnt.ctrlKey)
						{
							for (int i = 0; i <= index; i++)
								CurrentValue = SetInput(i, default);
							Focus(0);
						}
						else
						{
							CurrentValue = SetInput(index, default);
							Focus(index - 1);
						}
					}
					else if (fields[index].Text.Empty())
					{
						Focus(index - 1);
					}
					break;

				default:
					if (evnt.character == '\0' || MaxLength == -1) return;
					await Awaitable.NextFrameAsync();
					string str = fields[index].Text;
					if (!str.Empty() && str[0] != '\0' && str.Length >= MaxLength)
						Focus(index + 1);
					break;
			}

			evnt.StopPropagation();
		}

		protected abstract TInput GetInput(int index);
		protected abstract TValue SetInput(int index, TInput value);

		public virtual void Focus(int index)
		{
			if (index < 0) fields[0].Focus();
			else if (index > Count - 1) Blur();
			else fields[index].Focus();
		}
	}


	#region Input Set <TInput, TField>
	public class InputSet<TInput, TField> : InputSet<TInput[], TInput, TField> where TField : Input<TInput>, new()
	{
		protected override TInput GetInput(int index) => (CurrentValue.OutOfRange(index)) ? default : CurrentValue[index];
		protected override TInput[] SetInput(int index, TInput value)
		{
			TInput[] newValue = new TInput[Count];

			if (!CurrentValue.Empty())
				Array.Copy(CurrentValue, newValue, Count);

			newValue[index] = value;
			return newValue;
		}
	}

	public class StringInputSet : InputSet<string, StringInput> { }

	public class CharInputSet : InputSet<char, CharInput> { public CharInputSet() { FlexibleFields = false; MaxLength = 1; } }

	public class ByteInputSet : InputSet<byte, ByteInput> { }
	public class SByteInputSet : InputSet<sbyte, SByteInput> { }

	public class ShortInputSet : InputSet<short, ShortInput> { }
	public class UShortInputSet : InputSet<ushort, UShortInput> { }

	public class IntInputSet : InputSet<int, IntInput> { }
	public class UIntInputSet : InputSet<uint, UIntInput> { }

	public class LongInputSet : InputSet<long, LongInput> { }
	public class ULongInputSet : InputSet<ulong, ULongInput> { }

	public class FloatInputSet : InputSet<float, FloatInput> { }
	public class DoubleInputSet : InputSet<double, DoubleInput> { }
	public class DecimalInputSet : InputSet<decimal, DecimalInput> { }
	#endregion Input Set <TInput, TField>

	#region Vector 2 Input
	public class Vector2Input : InputSet<Vector2, float, FloatInput>
	{
		protected override int ForceCount => 2;

		public Vector2 Min { get => new Vector2(fields[0].Min, fields[1].Min); set => (fields[0].Min, fields[1].Min) = (value.x, value.y); }
		public Vector2 Max { get => new Vector2(fields[0].Max, fields[1].Max); set => (fields[0].Max, fields[1].Max) = (value.x, value.y); }


		public Vector2Input()
		{
			fields[0].Prefix = "X: ";
			fields[1].Prefix = "Y: ";
		}

		protected override float GetInput(int index) => (index) switch
		{
			0 => CurrentValue.x,
			1 => CurrentValue.y,
			_ => throw new IndexOutOfRangeException(),
		};
		protected override Vector2 SetInput(int index, float value) => (index) switch
		{
			0 => new Vector2(value, CurrentValue.y),
			1 => new Vector2(CurrentValue.x, value),
			_ => throw new IndexOutOfRangeException(),
		};
	}
	#endregion Vector 2 Input
	#region Vector 3 Input
	public class Vector3Input : InputSet<Vector3, float, FloatInput>
	{
		protected override int ForceCount => 3;

		public Vector3 Min { get => new Vector3(fields[0].Min, fields[1].Min, fields[2].Min); set => (fields[0].Min, fields[1].Min, fields[2].Min) = (value.x, value.y, value.z); }
		public Vector3 Max { get => new Vector3(fields[0].Max, fields[1].Max, fields[2].Max); set => (fields[0].Max, fields[1].Max, fields[2].Max) = (value.x, value.y, value.z); }


		public Vector3Input()
		{
			fields[0].Prefix = "X: ";
			fields[1].Prefix = "Y: ";
			fields[2].Prefix = "Z: ";
		}

		protected override float GetInput(int index) => (index) switch
		{
			0 => CurrentValue.x,
			1 => CurrentValue.y,
			2 => CurrentValue.z,
			_ => throw new IndexOutOfRangeException(),
		};
		protected override Vector3 SetInput(int index, float value) => (index) switch
		{
			0 => new Vector3(value, CurrentValue.y, CurrentValue.z),
			1 => new Vector3(CurrentValue.x, value, CurrentValue.z),
			2 => new Vector3(CurrentValue.x, CurrentValue.y, value),
			_ => throw new IndexOutOfRangeException(),
		};
	}
	#endregion Vector 3 Input
	#region Vector 4 Input
	public class Vector4Input : InputSet<Vector4, float, FloatInput>
	{
		protected override int ForceCount => 4;

		public Vector4 Min { get => new Vector4(fields[0].Min, fields[1].Min, fields[2].Min, fields[3].Min); set => (fields[0].Min, fields[1].Min, fields[2].Min, fields[3].Min) = (value.x, value.y, value.z, value.w); }
		public Vector4 Max { get => new Vector4(fields[0].Max, fields[1].Max, fields[2].Max, fields[3].Max); set => (fields[0].Max, fields[1].Max, fields[2].Max, fields[3].Max) = (value.x, value.y, value.z, value.w); }


		public Vector4Input()
		{
			fields[0].Prefix = "X: ";
			fields[1].Prefix = "Y: ";
			fields[2].Prefix = "Z: ";
			fields[3].Prefix = "W: ";
		}

		protected override float GetInput(int index) => (index) switch
		{
			0 => CurrentValue.x,
			1 => CurrentValue.y,
			2 => CurrentValue.z,
			3 => CurrentValue.w,
			_ => throw new IndexOutOfRangeException(),
		};
		protected override Vector4 SetInput(int index, float value) => (index) switch
		{
			0 => new Vector4(value, CurrentValue.y, CurrentValue.z, CurrentValue.w),
			1 => new Vector4(CurrentValue.x, value, CurrentValue.z, CurrentValue.w),
			2 => new Vector4(CurrentValue.x, CurrentValue.y, value, CurrentValue.w),
			3 => new Vector4(CurrentValue.x, CurrentValue.y, CurrentValue.z, value),
			_ => throw new IndexOutOfRangeException(),
		};
	}
	#endregion Vector 4 Input

	#region Vector 2 Int Input
	public class Vector2IntInput : InputSet<Vector2Int, int, IntInput>
	{
		protected override int ForceCount => 2;

		public Vector2Int Min { get => new Vector2Int(fields[0].Min, fields[1].Min); set => (fields[0].Min, fields[1].Min) = (value.x, value.y); }
		public Vector2Int Max { get => new Vector2Int(fields[0].Max, fields[1].Max); set => (fields[0].Max, fields[1].Max) = (value.x, value.y); }


		public Vector2IntInput()
		{
			fields[0].Prefix = "X: ";
			fields[1].Prefix = "Y: ";
		}

		protected override int GetInput(int index) => (index) switch
		{
			0 => CurrentValue.x,
			1 => CurrentValue.y,
			_ => throw new IndexOutOfRangeException(),
		};
		protected override Vector2Int SetInput(int index, int value) => (index) switch
		{
			0 => new Vector2Int(value, CurrentValue.y),
			1 => new Vector2Int(CurrentValue.x, value),
			_ => throw new IndexOutOfRangeException(),
		};
	}
	#endregion Vector 2 Int Input
	#region Vector 3 Int Input
	public class Vector3IntInput : InputSet<Vector3Int, int, IntInput>
	{
		protected override int ForceCount => 3;

		public Vector3Int Min { get => new Vector3Int(fields[0].Min, fields[1].Min, fields[2].Min); set => (fields[0].Min, fields[1].Min, fields[2].Min) = (value.x, value.y, value.z); }
		public Vector3Int Max { get => new Vector3Int(fields[0].Max, fields[1].Max, fields[2].Max); set => (fields[0].Max, fields[1].Max, fields[2].Max) = (value.x, value.y, value.z); }


		public Vector3IntInput()
		{
			fields[0].Prefix = "X: ";
			fields[1].Prefix = "Y: ";
			fields[2].Prefix = "Z: ";
		}

		protected override int GetInput(int index) => (index) switch
		{
			0 => CurrentValue.x,
			1 => CurrentValue.y,
			2 => CurrentValue.z,
			_ => throw new IndexOutOfRangeException(),
		};
		protected override Vector3Int SetInput(int index, int value) => (index) switch
		{
			0 => new Vector3Int(value, CurrentValue.y, CurrentValue.z),
			1 => new Vector3Int(CurrentValue.x, value, CurrentValue.z),
			2 => new Vector3Int(CurrentValue.x, CurrentValue.y, value),
			_ => throw new IndexOutOfRangeException(),
		};
	}
	#endregion Vector 3 Int Input
	#region Vector 4 Int Input
	// Why is there no Vector4Int .-.
	#endregion Vector 4 Int Input

	#region Quaternion Input
	public class QuaternionInput : InputSet<Quaternion, float, FloatInput>
	{
		protected override int ForceCount => 4;

		public Quaternion Min { get => new Quaternion(fields[0].Min, fields[1].Min, fields[2].Min, fields[3].Min); set => (fields[0].Min, fields[1].Min, fields[2].Min, fields[3].Min) = (value.x, value.y, value.z, value.w); }
		public Quaternion Max { get => new Quaternion(fields[0].Max, fields[1].Max, fields[2].Max, fields[3].Max); set => (fields[0].Max, fields[1].Max, fields[2].Max, fields[3].Max) = (value.x, value.y, value.z, value.w); }


		public QuaternionInput()
		{
			fields[0].Prefix = "X: ";
			fields[1].Prefix = "Y: ";
			fields[2].Prefix = "Z: ";
			fields[3].Prefix = "W: ";
		}

		protected override float GetInput(int index) => (index) switch
		{
			0 => CurrentValue.x,
			1 => CurrentValue.y,
			2 => CurrentValue.z,
			3 => CurrentValue.w,
			_ => throw new IndexOutOfRangeException(),
		};
		protected override Quaternion SetInput(int index, float value) => (index) switch
		{
			0 => new Quaternion(value, CurrentValue.y, CurrentValue.z, CurrentValue.w),
			1 => new Quaternion(CurrentValue.x, value, CurrentValue.z, CurrentValue.w),
			2 => new Quaternion(CurrentValue.x, CurrentValue.y, value, CurrentValue.w),
			3 => new Quaternion(CurrentValue.x, CurrentValue.y, CurrentValue.z, value),
			_ => throw new IndexOutOfRangeException(),
		};
	}
	#endregion Quaternion Input

	#region Date Input
	public class DateInput : CharInputSet
	{
		public enum DateFormat { DMY, MDY, YMD }

		protected override int ForceCount => 8;

		public readonly Label slash1;
		public readonly Label slash2;

		private DateFormat format;

		public DateFormat Format
		{
			get => format;
			set
			{
				slash1.RemoveFromHierarchy();
				slash2.RemoveFromHierarchy();

				switch (value)
				{
					case DateFormat.DMY:
						fields[0].Tooltip = "Day";
						fields[1].Tooltip = "Day";
						Insert(2, slash1);
						fields[2].Tooltip = "Month";
						fields[3].Tooltip = "Month";
						Insert(5, slash2);
						fields[4].Tooltip = "Year";
						fields[5].Tooltip = "Year";
						fields[6].Tooltip = "Year";
						fields[7].Tooltip = "Year";
						break;

					case DateFormat.MDY:
						fields[0].Tooltip = "Month";
						fields[1].Tooltip = "Month";
						Insert(2, slash1);
						fields[2].Tooltip = "Day";
						fields[3].Tooltip = "Day";
						Insert(5, slash2);
						fields[4].Tooltip = "Year";
						fields[5].Tooltip = "Year";
						fields[6].Tooltip = "Year";
						fields[7].Tooltip = "Year";
						break;

					case DateFormat.YMD:
						fields[0].Tooltip = "Year";
						fields[1].Tooltip = "Year";
						fields[2].Tooltip = "Year";
						fields[3].Tooltip = "Year";
						Insert(4, slash1);
						fields[4].Tooltip = "Month";
						fields[5].Tooltip = "Month";
						Insert(7, slash2);
						fields[6].Tooltip = "Day";
						fields[7].Tooltip = "Day";
						break;

					default: throw new Exception($"Unexpected date format {value}");
				}

				format = value;
				SetFieldsClasses();
				this.Refresh();
			}
		}

		protected string StringFormat => (Format) switch
		{
			DateFormat.DMY => "dd/MM/yyyy",
			DateFormat.MDY => "MM/dd/yyyy",
			DateFormat.YMD => "yyyy/MM/dd",
			_ => throw new Exception($"Unexpected date format {Format}"),
		};
		public string StringValue
		{
			get
			{
				char[] chars = BindedValue;
				char Char(int index)
				{
					char c = chars[index];
					return (c == '\0') ? ' ' : c;
				}

				return (Format) switch
				{
					DateFormat.DMY => $"{Char(0)}{Char(1)}/{Char(2)}{Char(3)}/{Char(4)}{Char(5)}{Char(6)}{Char(7)}",
					DateFormat.MDY => $"{Char(0)}{Char(1)}/{Char(2)}{Char(3)}/{Char(4)}{Char(5)}{Char(6)}{Char(7)}",
					DateFormat.YMD => $"{Char(0)}{Char(1)}{Char(2)}{Char(3)}/{Char(4)}{Char(5)}/{Char(6)}{Char(7)}",
					_ => throw new Exception($"Unexpected date format {Format}"),
				};
			}
			set
			{
				char[] source = value?.Replace("/", "").ToCharArray();
				char[] destination = new char[8];

				if (!source.Empty())
					Array.Copy(source, destination, Mathf.Min(source.Length, destination.Length));

				BindedValue = destination;
			}
		}
		public DateTime DateValue
		{
			get => (DateTime.TryParseExact(StringValue, StringFormat, DateTimeFormatInfo.CurrentInfo, DateTimeStyles.None, out DateTime value)) ? value : DateTime.MinValue;
			set
			{
				char[] source = value.ToString(StringFormat).Replace("/", "").ToCharArray();
				char[] destination = new char[8];

				if (!source.Empty())
					Array.Copy(source, destination, Mathf.Min(source.Length, destination.Length));

				BindedValue = destination;
			}
		}
		public bool Valid => DateTime.TryParseExact(StringValue, StringFormat, DateTimeFormatInfo.CurrentInfo, DateTimeStyles.None, out _);


		public DateInput() : this(DateFormat.MDY) { }
		public DateInput(DateFormat format)
		{
			slash1 = new Label() { Text = "/", Size = Size.Large };
			slash2 = new Label() { Text = "/", Size = Size.Large };

			Format = format;
		}
	}
	#endregion Date Input
	#region Phone Input
	public class PhoneInput : CharInputSet
	{
		protected override int ForceCount => 10;

		public string StringValue
		{
			get
			{
				char[] chars = BindedValue;
				char Char(int index)
				{
					char c = chars[index];
					return (c == '\0') ? ' ' : c;
				}

				return $"({Char(0)}{Char(1)}{Char(2)}){Char(3)}{Char(4)}{Char(5)}-{Char(6)}{Char(7)}{Char(8)}{Char(9)}";
			}
			set
			{
				char[] source = value?.Replace("(", "").Replace(")", "").Replace("-", "").ToCharArray();
				char[] destination = new char[10];

				if (!source.Empty())
					Array.Copy(source, destination, Mathf.Min(source.Length, destination.Length));

				BindedValue = destination;
			}
		}
		public bool Valid => BindedValue.All(c => char.IsDigit(c));


		public PhoneInput()
		{
			this.Attach(0, new Label() { Text = "(", Size = Size.Large });
			this.Attach(4, new Label() { Text = ")", Size = Size.Large });
			this.Attach(8, new Label() { Text = "-", Size = Size.Large });
		}
	}
	#endregion Phone Input
}