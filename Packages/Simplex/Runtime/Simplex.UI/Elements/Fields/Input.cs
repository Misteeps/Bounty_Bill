using System;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;


namespace Simplex.UI
{
	public abstract class Input<T> : Field<T>
	{
		protected override string[] DefaultClasses => new string[] { "field", "input", Type, "inset", "text" };
		protected abstract string Type { get; }

#if UNITY_EDITOR
		private int undoGroup;
#endif

		public ITextSelection Selection => this;
		public ITextEdition Edition => this;

		protected bool focused;
		protected bool richText;
		public override bool RichText
		{
			get => richText;
			set
			{
				richText = value;
				base.RichText = value;
			}
		}

		public int MaxLength { get => Edition.maxLength; set => Edition.maxLength = value; }
		public bool Multiline
		{
			get => (bool)typeof(ITextEdition).GetProperty("multiline", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Edition);
			set => typeof(ITextEdition).GetProperty("multiline", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(Edition, value);
		}
		public bool ReadOnly { get => Edition.isReadOnly; set => Edition.isReadOnly = value; }
		public bool Delayed { get; set; }
		public bool AutoSelectAll
		{
			get => Selection.selectAllOnFocus;
			set
			{
				Selection.selectAllOnFocus = value;
				Selection.selectAllOnMouseUp = value;
			}
		}

		public string Prefix { get; set; }
		public string Suffix { get; set; }
		public string Placeholder { get; set; }
		public Dictionary<T, string> Overrides { get; set; }

		public override T CurrentValue
		{
			set
			{
				base.CurrentValue = value;

				if (!focused)
				{
					string prefix = null;
					if (!Prefix.Empty())
						prefix = (RichText) ? $"<color=#A3A3A3>{Prefix}</color>" : Prefix;

					string suffix = null;
					if (!Suffix.Empty())
						suffix = (RichText) ? $"<color=#A3A3A3>{Suffix}</color>" : Prefix;

					if (Overrides.Empty() || !Overrides.TryGetValue(value, out string text))
					{
						text = ConvertValue(value);
						if (text.Empty() && !Placeholder.Empty())
							text = (RichText) ? $"<color=#A3A3A3>{Placeholder}</color>" : Placeholder;
					}

					Text = $"{prefix}{text}{suffix}";
				}
			}
		}


		public Input()
		{
			Selection.isSelectable = true;
			Selection.doubleClickSelectsWord = true;
			Selection.tripleClickSelectsLine = false;

			typeof(ITextEdition).GetProperty("AcceptCharacter", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(Edition, (Func<char, bool>)AcceptCharacter);
			typeof(ITextEdition).GetProperty("UpdateScrollOffset", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(Edition, (Action<bool>)UpdateScrollOffset);
			typeof(ITextEdition).GetProperty("UpdateValueFromText", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(Edition, (Action)UpdateValueFromText);
			typeof(ITextEdition).GetProperty("UpdateTextFromValue", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(Edition, (Action)UpdateTextFromValue);
			typeof(ITextEdition).GetProperty("MoveFocusToCompositeRoot", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(Edition, (Action)MoveFocusToCompositeRoot);

			MaxLength = -1;
			Multiline = false;
			ReadOnly = false;
			Delayed = false;
			AutoSelectAll = false;
		}

		protected override void OnFocusIn(FocusInEvent evnt)
		{
			focused = true;

			base.RichText = RichText && ReadOnly;
			base.OnFocusIn(evnt);

			Text = ConvertValue(CurrentValue);

#if UNITY_EDITOR
			UnityEditor.Undo.IncrementCurrentGroup();
			undoGroup = UnityEditor.Undo.GetCurrentGroup();
#endif
		}
		protected override void OnFocusOut(FocusOutEvent evnt)
		{
			focused = false;

			base.RichText = RichText;
			base.OnFocusOut(evnt);

			Selection.SelectRange(0, 0);

#if UNITY_EDITOR
			UnityEditor.Undo.CollapseUndoOperations(undoGroup);
#endif
		}

		protected abstract string ConvertValue(T value);
		protected abstract T ConvertText(string text);

		protected virtual bool AcceptCharacter(char c) => !ReadOnly && enabledInHierarchy;
		protected virtual void UpdateScrollOffset(bool isBackspace = false) { }
		protected virtual void UpdateValueFromText()
		{
			if (Delayed) CurrentValue = ConvertText(Text);
			else BindedValue = ConvertText(Text);
		}
		protected virtual void UpdateTextFromValue() { }
		protected virtual void MoveFocusToCompositeRoot() => Blur();
	}


	#region Label Input
	public class LabelInput : Input<string>
	{
		protected override string[] DefaultClasses => new string[] { "field", "input", Type, "text" };
		protected override string Type => "label";


		public LabelInput()
		{
			ReadOnly = true;
		}

		protected override string ConvertValue(string value) => value;
		protected override string ConvertText(string text) => text;
	}
	#endregion Label Input

	#region String Input
	public class StringInput : Input<string>
	{
		protected override string Type => "string";


		protected override string ConvertValue(string value) => value;
		protected override string ConvertText(string text) => text;
	}
	#endregion String Input

	#region Char Input
	public class CharInput : Input<char>
	{
		protected override string Type => "char";


		public CharInput()
		{
			MaxLength = 1;
			AutoSelectAll = true;

			RegisterCallback<ChangeEvent<char>>(OnChange);
		}

		protected virtual void OnChange(ChangeEvent<char> evnt)
		{
			if (evnt.newValue != '\0')
				Blur();
		}

		protected override string ConvertValue(char value) => value.ToString();
		protected override char ConvertText(string text) => text.Empty() ? '\0' : text[0];
	}
	#endregion Char Input

	#region Numeric Input
	public abstract class NumericInput<T> : Input<T>
	{
		protected override string Type => "number";

		public abstract Numeric<T> Numeric { get; }

		public T Min { get; set; }
		public T Max { get; set; }


		public NumericInput()
		{
			Min = Numeric.Min;
			Max = Numeric.Max;

			AutoSelectAll = true;
		}

		protected override string ConvertValue(T value) => Numeric.String(value);
		protected override T ConvertText(string text) => Numeric.Clamp(Min, Max, Numeric.Parse(text));

		protected override bool AcceptCharacter(char c) => base.AcceptCharacter(c) && Numeric.Valid(c);
	}

	public class ByteInput : NumericInput<byte> { public override Numeric<byte> Numeric { get; } = new ByteNumeric(); }
	public class SByteInput : NumericInput<sbyte> { public override Numeric<sbyte> Numeric { get; } = new SByteNumeric(); }

	public class ShortInput : NumericInput<short> { public override Numeric<short> Numeric { get; } = new ShortNumeric(); }
	public class UShortInput : NumericInput<ushort> { public override Numeric<ushort> Numeric { get; } = new UShortNumeric(); }

	public class IntInput : NumericInput<int> { public override Numeric<int> Numeric { get; } = new IntNumeric(); }
	public class UIntInput : NumericInput<uint> { public override Numeric<uint> Numeric { get; } = new UIntNumeric(); }

	public class LongInput : NumericInput<long> { public override Numeric<long> Numeric { get; } = new LongNumeric(); }
	public class ULongInput : NumericInput<ulong> { public override Numeric<ulong> Numeric { get; } = new ULongNumeric(); }

	public class FloatInput : NumericInput<float> { public override Numeric<float> Numeric { get; } = new FloatNumeric(); }
	public class DoubleInput : NumericInput<double> { public override Numeric<double> Numeric { get; } = new DoubleNumeric(); }
	public class DecimalInput : NumericInput<decimal> { public override Numeric<decimal> Numeric { get; } = new DecimalNumeric(); }
	#endregion Numeric Input
}