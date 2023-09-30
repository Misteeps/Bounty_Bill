using System;

using UnityEngine;
#if ENABLE_INPUT_SYSTEM && UNITY_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
#endif
using UnityEngine.UIElements;


namespace Simplex.UI
{
	public abstract class Bind<T> : Field<T>
	{
		public enum State { None, Duplicate, Active, Success, Cancel, Error }

		protected override string[] DefaultClasses => new string[] { "field", "bind", Type, "outset", "text" };
		protected abstract string Type { get; }

		public readonly Button icon;
		public readonly Button clear;

		public virtual bool Locked { get => ClassListContains("locked"); set => EnableInClassList("locked", value); }


		public Bind()
		{
			icon = this.Attach(new Button() { Classes = "icon, left", Pickable = false });
			clear = this.Attach(new Button() { Classes = "clear, right" }.Bind(OnClear));

			RegisterCallback<ClickEvent>(OnClick);
		}

		protected virtual void OnClear(ClickEvent evnt)
		{
			BindedValue = default;
			Pulse(State.Cancel);

			evnt.StopPropagation();
		}
		protected virtual void OnClick(ClickEvent evnt)
		{
			if (Locked)
			{
				Pulse(State.Error);
				return;
			}

			RebindStart();

			evnt.StopPropagation();
		}

		public virtual void RebindStart()
		{
			panel.visualTree.SetEnabled(false);
			Set(State.Active);
		}
		public virtual void RebindEnd()
		{
			panel.visualTree.SetEnabled(true);
			Set(State.None);
		}

		public virtual void Set(State state)
		{
			foreach (string sizeName in Enum.GetNames(typeof(State)))
				RemoveFromClassList(sizeName.ToLower());

			if (state != State.None)
				AddToClassList(state.ToString().ToLower());
		}
		public virtual void Pulse(State state)
		{
			AddToClassList(state.ToString().ToLower());
			schedule.Execute(() => RemoveFromClassList(state.ToString().ToLower())).ExecuteLater(1);
		}
	}


	#region Key Bind
#if ENABLE_LEGACY_INPUT_MANAGER
	public class KeyBindGroup : Field<(KeyCode, KeyCode)>
	{
		protected override string[] DefaultClasses => new string[] { "field", "keycode-group" };

		public readonly KeyBind primary;
		public readonly KeyBind secondary;

		public KeyCode Primary { get => BindedValue.Item1; set => BindedValue = (value, BindedValue.Item2); }
		public KeyCode Secondary { get => BindedValue.Item2; set => BindedValue = (BindedValue.Item1, value); }

		public override (KeyCode, KeyCode) CurrentValue
		{
			set
			{
				base.CurrentValue = value;
				primary.CurrentValue = value.Item1;
				secondary.CurrentValue = value.Item2;
			}
		}


		public KeyBindGroup()
		{
			primary = this.Attach(new KeyBind() { Name = "primary"}.Bind(() => Primary, value => Primary = value));
			secondary = this.Attach(new KeyBind() { Name = "secondary"}.Bind(() => Secondary, value => Secondary = value));
		}
	}
	public class KeyBind : Bind<KeyCode>
	{
		protected override string Type => "keycode";

		public override KeyCode CurrentValue
		{
			set
			{
				base.CurrentValue = value;
				Text = ConvertKeycode(value);
			}
		}


		public override void RebindStart()
		{
			base.RebindStart();

			throw new NotImplementedException();
		}
		public override void RebindEnd()
		{
			base.RebindEnd();

			throw new NotImplementedException();
		}

		public static string ConvertKeycode(KeyCode keycode) => (keycode) switch
		{
			KeyCode.None => string.Empty,
			KeyCode.Mouse0 => "Left Click",
			KeyCode.Mouse1 => "Right Click",
			KeyCode.Mouse2 => "Scroll Click",
			KeyCode.Mouse3 => "Thumb Forward",
			KeyCode.Mouse4 => "Thumb Backward",
			KeyCode.Return => "Enter",
			KeyCode.Alpha0 => "0",
			KeyCode.Alpha1 => "1",
			KeyCode.Alpha2 => "2",
			KeyCode.Alpha3 => "3",
			KeyCode.Alpha4 => "4",
			KeyCode.Alpha5 => "5",
			KeyCode.Alpha6 => "6",
			KeyCode.Alpha7 => "7",
			KeyCode.Alpha8 => "8",
			KeyCode.Alpha9 => "9",
			KeyCode.BackQuote => "`",
			KeyCode.Minus => "-",
			KeyCode.Equals => "=",
			KeyCode.LeftBracket => "[",
			KeyCode.RightBracket => "]",
			KeyCode.Backslash => "\\",
			KeyCode.Semicolon => ";",
			KeyCode.Quote => "'",
			KeyCode.Comma => ",",
			KeyCode.Period => ".",
			KeyCode.Slash => "/",
			KeyCode.Numlock => "Num Lock",
			KeyCode.KeypadDivide => "Num /",
			KeyCode.KeypadMultiply => "Num *",
			KeyCode.KeypadMinus => "Num -",
			KeyCode.KeypadPlus => "Num +",
			KeyCode.KeypadPeriod => "Num .",
			KeyCode.KeypadEnter => "Num Enter",
			KeyCode.Keypad0 => "Num 0",
			KeyCode.Keypad1 => "Num 1",
			KeyCode.Keypad2 => "Num 2",
			KeyCode.Keypad3 => "Num 3",
			KeyCode.Keypad4 => "Num 4",
			KeyCode.Keypad5 => "Num 5",
			KeyCode.Keypad6 => "Num 6",
			KeyCode.Keypad7 => "Num 7",
			KeyCode.Keypad8 => "Num 8",
			KeyCode.Keypad9 => "Num 9",
			_ => keycode.ToString().TitleCase(),
		};
	}
#endif
	#endregion Key Bind

	#region Control Bind
#if ENABLE_INPUT_SYSTEM && UNITY_INPUT_SYSTEM
	public class ControlBind : Bind<string>
	{
		protected override string Type => "control";

		public Type[] Devices { get; set; } = null;
		public Func<InputControl, bool> Validator { get; set; } = null;

		public override string CurrentValue
		{
			set
			{
				base.CurrentValue = value;
				Text = ConvertName(ConvertPath(value));
			}
		}


		public ControlBind Bind(InputActionAsset asset, string action, int index)
		{
			try { return Bind(asset[action], index); }
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed binding {asset:ref} with action {action:info} at index {index:info}"); return this.Refresh(); }
		}
		public ControlBind Bind(InputAction action, int index)
		{
			try
			{
				if (index < 0 || index >= action.bindings.Count)
					throw new ArgumentOutOfRangeException("Binding Index", "Binding index not within action bindings count");

				if (action.bindings[index].isComposite)
					throw new ArgumentException("Composite Binding not Supported");

				return this.Bind(action.name, () => action.bindings[index].effectivePath, value => action.ApplyBindingOverride(index, value));
			}
			catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed binding {action:ref} at index {index:info}"); return this.Refresh(); }
		}

		public override void RebindStart()
		{
			base.RebindStart();

			InputSystem.onEvent -= RebindEvent;
			InputSystem.onEvent += RebindEvent;
		}
		public override void RebindEnd()
		{
			base.RebindEnd();

			InputSystem.onEvent -= RebindEvent;
		}
		protected virtual void RebindEvent(InputEventPtr ptr, InputDevice device)
		{
			InputControl control = ptr.GetFirstButtonPressOrNull();
			if (control == null) return;

			if (control is KeyControl keyControl)
				switch (keyControl.keyCode)
				{
					case Key.Escape:
						RebindEnd();
						Pulse(State.Cancel);
						return;
				}

			if (!Devices.Empty() && Array.FindIndex(Devices, type => type.IsAssignableFrom(device.GetType())) == -1)
			{
				Pulse(State.Error);
				return;
			}

			if (Validator != null && !Validator.Invoke(control))
			{
				Pulse(State.Error);
				return;
			}

			BindedValue = control.path;
			RebindEnd();
			Pulse(State.Success);
		}

		public static string ConvertPath(string path) => (path.Empty()) ? string.Empty : InputControlPath.ToHumanReadableString(path, InputControlPath.HumanReadableStringOptions.OmitDevice);
		public static string ConvertName(string name) => (name.Empty()) ? string.Empty : (name.ToLower().Replace(" ", "")) switch
		{
			"leftbutton" => "Left Click",
			"rightbutton" => "Right Click",
			"middlebutton" => "Scroll Click",
			"scrollup" => "Scroll Up",
			"scrolldown" => "Scroll Down",
			"scrollleft" => "Scroll Left",
			"scrollright" => "Scroll Right",
			"forwardbutton" => "Thumb Forward",
			"backbutton" => "Thumb Backward",

			"backquote" => "`",
			"minus" => "-",
			"equals" => "=",
			"leftbracket" => "[",
			"rightbracket" => "]",
			"backslash" => "\\",
			"semicolon" => ";",
			"quote" => "'",
			"comma" => ",",
			"period" => ".",
			"slash" => "/",
			"contextmenu" => "Menu",
			"numlock" => "Num Lock",
			"numpaddivide" => "Num /",
			"numpadmultiply" => "Num *",
			"numpadminus" => "Num -",
			"numpadplus" => "Num +",
			"numpadperiod" => "Num .",
			"numpadenter" => "Num Enter",
			"numpad0" => "Num 0",
			"numpad1" => "Num 1",
			"numpad2" => "Num 2",
			"numpad3" => "Num 3",
			"numpad4" => "Num 4",
			"numpad5" => "Num 5",
			"numpad6" => "Num 6",
			"numpad7" => "Num 7",
			"numpad8" => "Num 8",
			"numpad9" => "Num 9",
			"f1" => "F1",
			"f2" => "F2",
			"f3" => "F3",
			"f4" => "F4",
			"f5" => "F5",
			"f6" => "F6",
			"f7" => "F7",
			"f8" => "F8",
			"f9" => "F9",
			"f10" => "F10",
			"f11" => "F11",
			"f12" => "F12",
			"oem1" => "OEM 1",
			"oem2" => "OEM 2",
			"oem3" => "OEM 3",
			"oem4" => "OEM 4",
			"oem5" => "OEM 5",

			"buttonnorth" => "North Button",
			"buttonsouth" => "South Button",
			"buttoneast" => "East Button",
			"buttonwest" => "West Button",
			"d-pad/up" => "D-Pad Up",
			"d-pad/down" => "D-Pad Down",
			"d-pad/left" => "D-Pad Left",
			"d-pad/right" => "D-Pad Right",

			"tip" => "Pen Tip",
			"eraser" => "Pen Eraser",
			"barrel1" => "Pen Button 1",
			"barrelbutton#1" => "Pen Button 1",
			"barrel2" => "Pen Button 2",
			"barrelbutton#2" => "Pen Button 2",
			"barrel3" => "Pen Button 3",
			"barrelbutton#3" => "Pen Button 3",
			"barrel4" => "Pen Button 4",
			"barrelbutton#4" => "Pen Button 4",

			_ => name.TitleCase(),
		};
	}
#endif
	#endregion Control Bind
}