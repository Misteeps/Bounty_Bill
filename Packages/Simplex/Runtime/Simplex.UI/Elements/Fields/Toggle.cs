using System;

using UnityEngine;
using UnityEngine.UIElements;


namespace Simplex.UI
{
	public abstract class Toggle : Field<bool>
	{
		public override bool CurrentValue
		{
			set
			{
				base.CurrentValue = value;
				this.ClassToggle("on", "off", CurrentValue);
			}
		}


		public Toggle()
		{
			RegisterCallback<ClickEvent>(OnClick);
		}

		protected virtual void OnClick(ClickEvent evnt)
		{
			if (evnt.button == 0)
				BindedValue = !BindedValue;

			evnt.StopPropagation();
		}
	}


	#region Toggle Slide
	public class ToggleSlide : Toggle
	{
		protected override string[] DefaultClasses => new string[] { "field", "toggle", "slide", "inset" };

		public readonly Div fill;
		public readonly Div knob;


		public ToggleSlide()
		{
			fill = this.Attach(new Div() { Classes = "fill" });
			knob = fill.Attach(new Div() { Classes = "knob" });
		}
	}
	#endregion Toggle Slide

	#region Toggle Check
	public class ToggleCheck : Toggle
	{
		protected override string[] DefaultClasses => new string[] { "field", "toggle", "check", "inset", "icon" };
	}
	#endregion Toggle Check

	#region Toggle Button
	public class ToggleButton : Toggle
	{
		protected override string[] DefaultClasses => new string[] { "field", "toggle", "button", "outset", "text" };

		public string EnabledText { get; set; } = "Enabled";
		public string DisabledText { get; set; } = "Disabled";

		public override bool CurrentValue
		{
			set
			{
				base.CurrentValue = value;
				Text = (CurrentValue) ? EnabledText : DisabledText;
			}
		}
	}
	#endregion Toggle Button
}