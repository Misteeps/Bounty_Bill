using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;

using Simplex;

namespace Game
{
	public static class Settings
	{
		#region Setting <T>
		public interface ISetting { public void Default(); }
		public class Setting<T> : VariableValue<T>, ISetting
		{
			public readonly T defaultValue;
			public readonly Action<T> onSet;


			public Setting(string name, T defaultValue) : this(name, defaultValue, null) { }
			public Setting(string name, T defaultValue, Action<T> onSet) : base(name, defaultValue)
			{
				this.defaultValue = defaultValue;
				this.onSet = onSet;

				Changed += _ => onSet?.Invoke(Value);
			}

			public void Default() => Set(defaultValue);

			public override string ToString() => Value.ToString();
			public static implicit operator T(Setting<T> setting) => setting.Value;
		}
		#endregion Setting <T>

		#region Choice
		public class Choice<T> : Setting<T>
		{
			public readonly T[] values;
			public readonly string[] choices;


			public Choice(string name, (T value, string choice)[] array, Action<T> onSet = null) : this(name, array[0].value, array, onSet) { }
			public Choice(string name, T defaultValue, (T value, string choice)[] array, Action<T> onSet = null) : base(name, defaultValue, onSet)
			{
				if (array.Empty()) throw new ArgumentException("Empty or null choice/value array").Overwrite($"Failed constructing choice setting {typeof(T):type}");

				values = new T[array.Length];
				choices = new string[array.Length];
				for (int i = 0; i < array.Length; i++)
				{
					(T value, string choice) = array[i];
					values[i] = value;
					choices[i] = choice;
				}
			}

			public bool Contains(string choice) => Array.IndexOf(choices, choice) != -1;
			public T Find(string choice)
			{
				T value = values[0];

				if (!string.IsNullOrEmpty(choice))
					try { value = values[Array.IndexOf(choices, choice)]; }
					catch (Exception exception) { exception.Error($"Failed finding value linked to {choice:info} in {typeof(T):type}"); return value; }

				return value;
			}

			public bool Contains(T value) => Array.IndexOf(values, value) != -1;
			public string Find(T value)
			{
				string choice = choices[0];

				try { choice = choices[Array.IndexOf(values, value)]; }
				catch (Exception exception) { exception.Error($"Failed finding choice linked to {value:info} in {typeof(T):type}"); return choice; }

				return choice;
			}

			public void Set(string value) => Set(Find(value));

			public override string ToString() => Find(Value);
		}
		#endregion Choice
		#region Keybind
		public class Keybind : Setting<(KeyCode, KeyCode)>
		{
			public readonly bool lockPrimary;

			public KeyCode Primary => Value.Item1;
			public KeyCode Secondary => Value.Item2;

			public bool Up => Input.GetKeyUp(Primary) || Input.GetKeyUp(Secondary);
			public bool Down => Input.GetKeyDown(Primary) || Input.GetKeyDown(Secondary);
			public bool Held => Input.GetKey(Primary) || Input.GetKey(Secondary);


			public Keybind(string name, KeyCode defaultPrimary, KeyCode defaultSecondary = KeyCode.None, bool lockPrimary = false, Action<(KeyCode, KeyCode)> onSet = null) : base(name, (defaultPrimary, defaultSecondary), onSet)
			{
				this.lockPrimary = lockPrimary;
			}

			public override string ToString() => $"{Primary}, {Secondary}";
			public override bool Equals(object obj) => (obj is Keybind keybind) && keybind.Primary == Primary && keybind.Secondary == Secondary;
			public override int GetHashCode() => HashCode.Combine(Primary, Secondary);
			public static bool Equals(Keybind keybind, KeyCode keycode) => keybind.Primary == keycode || keybind.Secondary == keycode;
			public static bool operator ==(Keybind keybind, KeyCode keycode) => Equals(keybind, keycode);
			public static bool operator !=(Keybind keybind, KeyCode keycode) => !Equals(keybind, keycode);
			public static bool operator ==(KeyCode keycode, Keybind keybind) => Equals(keybind, keycode);
			public static bool operator !=(KeyCode keycode, Keybind keybind) => !Equals(keybind, keycode);
		}
		#endregion Keybind


		public static (FullScreenMode, string)[] WindowModes
		{
			get
			{
#if !UNITY_WEBGL
				return new (FullScreenMode, string)[] { (FullScreenMode.ExclusiveFullScreen, "Fullscreen"), (FullScreenMode.Windowed, "Normal") };
#elif UNITY_STANDALONE_OSX
				return new (FullScreenMode, string)[] { (FullScreenMode.ExclusiveFullScreen, "Fullscreen"), (FullScreenMode.FullScreenWindow, "Borderless Window"), (FullScreenMode.MaximizedWindow, "Maximized Window"), (FullScreenMode.Windowed, "Windowed"), };
#else
				return new (FullScreenMode, string)[] { (FullScreenMode.ExclusiveFullScreen, "Fullscreen"), (FullScreenMode.FullScreenWindow, "Borderless Window"), (FullScreenMode.Windowed, "Windowed"), };
#endif
			}
		}
		public static ((int, int), string)[] Resolutions
		{
			get
			{
#if UNITY_WEBGL
				return new ((int, int), string)[] { ((0, 0), "WebGL") };
#else
				List<Vector2Int> newResolutions = new List<Vector2Int>();
				foreach (Resolution res in Screen.resolutions)
				{
					Vector2Int size = new Vector2Int(res.width, res.height);
					if (!newResolutions.Contains(size))
						newResolutions.Add(size);
				}

				return newResolutions
				.ConvertAll(res => ((res.x, res.y), $"{res.x} x {res.y}"))
				.OrderByDescending(res => res.Item1.x).ThenByDescending(res => res.Item1.y)
				.ToArray();
#endif
			}
		}


		[Header("General")]
		public static Setting<bool> cameraEffects = new Setting<bool>("Camera Effects", true);

		[Header("Graphics")]
		public static Choice<FullScreenMode> windowMode = new Choice<FullScreenMode>("Window Mode", FullScreenMode.FullScreenWindow, WindowModes, _ => ApplyResolution());
		public static Choice<(int width, int height)> resolution = new Choice<(int width, int height)>("Resolution", Resolutions, _ => ApplyResolution());
		public static Setting<int> fpsLimit = new Setting<int>("FPS Limit", 144, value => Application.targetFrameRate = (value == 301) ? 0 : value);
		public static Setting<bool> fpsCounter = new Setting<bool>("FPS Counter", false, value => UI.Overlay.Instance.ShowFPS(value));
		public static Setting<bool> vSync = new Setting<bool>("V-Sync", true, value => QualitySettings.vSyncCount = (value) ? 1 : 0);

		[Header("Audio")]
		public static Setting<int> masterVolume = new Setting<int>("Master Volume", 100, Audio.Master.SetVolume);
		public static Setting<int> uiVolume = new Setting<int>("UI Volume", 50, Audio.UI.SetVolume);
		public static Setting<int> sfxVolume = new Setting<int>("SFX Volume", 50, Audio.SFX.SetVolume);
		public static Setting<int> voiceVolume = new Setting<int>("Voice Volume", 50, Audio.Voice.SetVolume);
		public static Setting<int> ambianceVolume = new Setting<int>("Ambiance Volume", 50, Audio.Ambiance.SetVolume);
		public static Setting<int> musicVolume = new Setting<int>("Music Volume", 50, Audio.Music.SetVolume);

		[Header("Keybinds")]
		public static Keybind shoot = new Keybind("Shoot", KeyCode.Mouse0, KeyCode.None, true);
		public static Keybind dodge = new Keybind("Dodge", KeyCode.Space, KeyCode.LeftShift);
		public static Keybind moveUp = new Keybind("Move Up", KeyCode.W, KeyCode.UpArrow);
		public static Keybind moveDown = new Keybind("Move Down", KeyCode.S, KeyCode.DownArrow);
		public static Keybind moveLeft = new Keybind("Move Left", KeyCode.A, KeyCode.LeftArrow);
		public static Keybind moveRight = new Keybind("Move Right", KeyCode.D, KeyCode.RightArrow);
		public static Keybind escape = new Keybind("Escape", KeyCode.Escape, KeyCode.Mouse3, true);


		public static void Defaults()
		{
			foreach (FieldInfo field in typeof(Settings).GetFields())
				if (field.GetValue(null) is ISetting iSetting)
					iSetting.Default();
		}

		private static void ApplyResolution()
		{
#if UNITY_WEBGL
			Screen.fullScreenMode = windowMode;
#else
			Screen.SetResolution(resolution.Value.width, resolution.Value.height, windowMode);
#endif
		}
	}
}