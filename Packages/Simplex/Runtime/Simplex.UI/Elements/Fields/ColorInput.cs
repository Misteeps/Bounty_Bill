using System;

using UnityEngine;
using UnityEngine.UIElements;


namespace Simplex.UI
{
	public class ColorInput : Field<Color>
	{
		protected override string[] DefaultClasses => new string[] { "field", "color", "half" };

		public readonly Div preview;
		public readonly IntInput red;
		public readonly IntInput green;
		public readonly IntInput blue;
		public readonly FloatInput alpha;
		public readonly StringInput hex;

		public int Red
		{
			get => Mathf.RoundToInt(CurrentValue.r * 255f);
			set => BindedValue = new Color(value / 255f, CurrentValue.g, CurrentValue.b, CurrentValue.a);
		}
		public int Green
		{
			get => Mathf.RoundToInt(CurrentValue.g * 255f);
			set => BindedValue = new Color(CurrentValue.r, value / 255f, CurrentValue.b, CurrentValue.a);
		}
		public int Blue
		{
			get => Mathf.RoundToInt(CurrentValue.b * 255f);
			set => BindedValue = new Color(CurrentValue.r, CurrentValue.g, value / 255f, CurrentValue.a);
		}
		public float Alpha
		{
			get => CurrentValue.a;
			set => BindedValue = new Color(CurrentValue.r, CurrentValue.g, CurrentValue.b, value);
		}
		public string Hex
		{
			get => ColorUtility.ToHtmlStringRGB(CurrentValue);
			set => BindedValue = (ColorUtility.TryParseHtmlString($"#{value.TrimStart('#')}", out Color color)) ? color : CurrentValue;
		}

		public override Color CurrentValue
		{
			set
			{
				base.CurrentValue = value;
				preview.style.backgroundColor = value;
				red.CurrentValue = Red;
				green.CurrentValue = Green;
				blue.CurrentValue = Blue;
				alpha.CurrentValue = Alpha;
				hex.CurrentValue = Hex;
			}
		}


		public ColorInput()
		{
			preview = this.Attach(new Div() { Name = "preview", Classes = "field, inset, preview", Pickable = true, Navigable = true, Focusable = true });
			red = this.Attach(new IntInput() { Name = "red", Classes = "rgb", Prefix = "R:", Min = 0, Max = 255 }.Bind(() => Red, value => Red = value));
			green = this.Attach(new IntInput() { Name = "green", Classes = "rgb", Prefix = "G:", Min = 0, Max = 255 }.Bind(() => Green, value => Green = value));
			blue = this.Attach(new IntInput() { Name = "blue", Classes = "rgb", Prefix = "B:", Min = 0, Max = 255 }.Bind(() => Blue, value => Blue = value));
			alpha = this.Attach(new FloatInput() { Name = "alpha", Classes = "alpha", Prefix = "A:", Min = 0, Max = 255 }.Bind(() => Alpha, value => Alpha = value));
			hex = this.Attach(new StringInput() { Name = "hex", Classes = "hex", Prefix = "#", AutoSelectAll = true }.Bind(() => Hex, value => Hex = value));

			preview.RegisterCallback<ClickEvent>(OnClick);
			RegisterCallback<GeometryChangedEvent>(OnGeometryChange);
		}

		protected virtual void OnClick(ClickEvent evnt)
		{
			Popup popup = new Popup().Open(this, true);
			popup.Attach(new VerticalSpace(Size.Huge));
			popup.Attach(new Label() { Flexible = true, Text = "Color View\nUnder Construction" });
			popup.Attach(new VerticalSpace(Size.Huge));

			evnt.StopPropagation();
		}
		protected virtual void OnGeometryChange(GeometryChangedEvent evnt)
		{
			if (evnt.oldRect.width != evnt.newRect.width)
				this.ClassToggle("full", "half", layout.width > 320);
		}
	}
}