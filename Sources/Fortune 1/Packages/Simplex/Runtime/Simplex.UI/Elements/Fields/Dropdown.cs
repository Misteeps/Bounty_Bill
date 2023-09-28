using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;


namespace Simplex.UI
{
	public class Dropdown : Field
	{
		protected override string[] DefaultClasses => new string[] { "field", "dropdown", "outset", "text", "icon" };

		public string[] PopupClasses { get; set; } = null;
		public bool InheritStyleSheets { get; set; } = true;

		public Func<VisualElement> contentsFactory;


		public Dropdown()
		{
			RegisterCallback<ClickEvent>(OnClick);
		}

		public Dropdown Bind(VisualElement contents) => Bind(() => contents);
		public Dropdown Bind(Func<VisualElement> contentsFactory)
		{
			this.contentsFactory = contentsFactory;

			return this;
		}

		protected virtual void OnClick(ClickEvent evnt)
		{
			if (contentsFactory == null) { ConsoleUtilities.Warn(ConsoleUtilities.uiTag, $"Empty contents in {this:ref}. Dropdown popup will not be shown"); return; }

			Popup popup = new Popup().Class(PopupClasses).Open(this, InheritStyleSheets);
			popup.Attach(contentsFactory.Invoke());

			evnt.StopPropagation();
		}
	}


	public class Dropdown<T> : Field<T>
	{
		protected override string[] DefaultClasses => new string[] { "field", "dropdown", "outset", "text", "icon" };

		public string[] PopupClasses { get; set; } = null;
		public bool InheritStyleSheets { get; set; } = true;
		public bool Searchable { get; set; } = false;

		public IEnumerable<T> values;
		public Func<T, string> stringify;
		public Func<T, Texture2D> iconify;

		public override T CurrentValue
		{
			set
			{
				base.CurrentValue = value;
				Text = stringify.Invoke(value);
			}
		}


		public Dropdown()
		{
			RegisterCallback<ClickEvent>(OnClick);

			BindDirectory((typeof(T).IsEnum) ? (T[])Enum.GetValues(typeof(T)) : new T[] { CurrentValue });
		}

		public Dropdown<T> BindDirectory<TEnum>() where TEnum : Enum, T => BindDirectory((T[])Enum.GetValues(typeof(TEnum)));
		public Dropdown<T> BindDirectory(Dictionary<T, string> dictionary) => BindDirectory(dictionary.Keys, dictionary.Values, null);
		public Dropdown<T> BindDirectory(IEnumerable<(T value, string name, Texture2D icon)> tuples) => BindDirectory(tuples.Select(tuple => tuple.value), tuples.Select(tuple => tuple.name), tuples.Select(tuple => tuple.icon));
		public Dropdown<T> BindDirectory(IEnumerable<(T value, string name)> tuples) => BindDirectory(tuples.Select(tuple => tuple.value), tuples.Select(tuple => tuple.name), null);
		public Dropdown<T> BindDirectory(IEnumerable<T> values, IEnumerable<string> names, IEnumerable<Texture2D> icons)
		{
			T[] valuesArray = values?.ToArray() ?? new T[0];
			string[] namesArray = names?.ToArray() ?? new string[0];
			Texture2D[] iconsArray = icons?.ToArray() ?? new Texture2D[0];

			string Stringify(T value)
			{
				int index = Array.IndexOf(valuesArray, value);
				return (index == -1 || namesArray.OutOfRange(index)) ? null : namesArray[index];
			}

			Texture2D Iconify(T value)
			{
				int index = Array.IndexOf(valuesArray, value);
				return (index == -1 || iconsArray.OutOfRange(index)) ? null : iconsArray[index];
			}

			return BindDirectory(values, Stringify, Iconify);
		}
		public Dropdown<T> BindDirectory(IEnumerable<T> values = null, Func<T, string> stringify = null, Func<T, Texture2D> iconify = null)
		{
			this.values = values;
			this.stringify = stringify ?? (value => value?.ToString().TitleCase());
			this.iconify = iconify ?? (value => null);

			return this;
		}

		protected virtual void OnClick(ClickEvent evnt)
		{
			if (values == null || values.Count() == 0) { ConsoleUtilities.Warn(ConsoleUtilities.uiTag, $"Empty values in {this:ref}. Dropdown popup will not be shown"); return; }

			Popup popup = new Popup().Class(PopupClasses).Open(this, InheritStyleSheets);
			DirectoryView<T> directory = popup.Attach(new DirectoryView<T>() { Flexible = true, Searchable = Searchable }.Bind(values));

			Type enumType = typeof(T);
			if (enumType.IsEnum && Attribute.IsDefined(enumType, typeof(FlagsAttribute)))
			{
				Type underlyingType = Enum.GetUnderlyingType(enumType);
				Func<T, bool, T> combineValues;
				Func<T[]> getSelected;

				if (underlyingType == typeof(int)) EnumFlagsIntFuncs(enumType, out combineValues, out getSelected);
				else if (underlyingType == typeof(uint)) EnumFlagsUIntFuncs(enumType, out combineValues, out getSelected);
				else if (underlyingType == typeof(byte)) EnumFlagsByteFuncs(enumType, out combineValues, out getSelected);
				else if (underlyingType == typeof(sbyte)) EnumFlagsSByteFuncs(enumType, out combineValues, out getSelected);
				else if (underlyingType == typeof(short)) EnumFlagsShortFuncs(enumType, out combineValues, out getSelected);
				else if (underlyingType == typeof(ushort)) EnumFlagsUShortFuncs(enumType, out combineValues, out getSelected);
				else throw new Exception().Overwrite(ConsoleUtilities.uiTag, $"Dropdown does not support {underlyingType:type} enums");

				directory.onSelect = (value, selected) => { BindedValue = combineValues.Invoke(value, selected); CurrentValue = BindedValue; directory.SetSelected(getSelected.Invoke()); };
				directory.onBindElement = (path, value, element) => element.Bind(stringify.Invoke(value), iconify.Invoke(value), value);
				directory.SetSelected(getSelected.Invoke());
			}
			else
			{
				directory.onSelect = (value, selected) => { BindedValue = value; popup.Close(); };
				directory.onBindElement = (path, value, element) => element.Bind(stringify.Invoke(value), iconify.Invoke(value), value);
				directory.SetSelected(CurrentValue);
			}

			evnt.StopPropagation();
		}

		private void EnumFlagsByteFuncs(Type enumType, out Func<T, bool, T> combineValues, out Func<T[]> getSelected)
		{
			combineValues = (value, selected) =>
			{
				byte currentByte = Convert.ToByte(CurrentValue);
				byte valueByte = Convert.ToByte(value);
				return (T)Enum.ToObject(enumType, (valueByte == 0) ? 0 : (selected) ? currentByte + valueByte : currentByte - valueByte);
			};

			getSelected = () =>
			{
				List<T> list = new List<T>();
				byte currentByte = Convert.ToByte(CurrentValue);
				foreach (T value in (T[])Enum.GetValues(enumType))
				{
					byte valueByte = Convert.ToByte(value);
					if (valueByte == 0) continue;
					if ((currentByte & valueByte) == valueByte)
						list.Add(value);
				}
				return (list.Empty()) ? new T[] { (T)Enum.ToObject(enumType, 0) } : list.ToArray();
			};
		}
		private void EnumFlagsSByteFuncs(Type enumType, out Func<T, bool, T> combineValues, out Func<T[]> getSelected)
		{
			combineValues = (value, selected) =>
			{
				sbyte currentByte = Convert.ToSByte(CurrentValue);
				sbyte valueByte = Convert.ToSByte(value);
				return (T)Enum.ToObject(enumType, (valueByte == 0) ? 0 : (selected) ? currentByte + valueByte : currentByte - valueByte);
			};

			getSelected = () =>
			{
				List<T> list = new List<T>();
				sbyte currentByte = Convert.ToSByte(CurrentValue);
				foreach (T value in (T[])Enum.GetValues(enumType))
				{
					sbyte valueByte = Convert.ToSByte(value);
					if (valueByte == 0) continue;
					if ((currentByte & valueByte) == valueByte)
						list.Add(value);
				}
				return (list.Empty()) ? new T[] { (T)Enum.ToObject(enumType, 0) } : list.ToArray();
			};
		}

		private void EnumFlagsShortFuncs(Type enumType, out Func<T, bool, T> combineValues, out Func<T[]> getSelected)
		{
			combineValues = (value, selected) =>
			{
				short currentShort = Convert.ToInt16(CurrentValue);
				short valueShort = Convert.ToInt16(value);
				return (T)Enum.ToObject(enumType, (valueShort == 0) ? 0 : (selected) ? currentShort + valueShort : currentShort - valueShort);
			};

			getSelected = () =>
			{
				List<T> list = new List<T>();
				short currentShort = Convert.ToInt16(CurrentValue);
				foreach (T value in (T[])Enum.GetValues(enumType))
				{
					short valueShort = Convert.ToInt16(value);
					if (valueShort == 0) continue;
					if ((currentShort & valueShort) == valueShort)
						list.Add(value);
				}
				return (list.Empty()) ? new T[] { (T)Enum.ToObject(enumType, 0) } : list.ToArray();
			};
		}
		private void EnumFlagsUShortFuncs(Type enumType, out Func<T, bool, T> combineValues, out Func<T[]> getSelected)
		{
			combineValues = (value, selected) =>
			{
				ushort currentShort = Convert.ToUInt16(CurrentValue);
				ushort valueShort = Convert.ToUInt16(value);
				return (T)Enum.ToObject(enumType, (valueShort == 0) ? 0 : (selected) ? currentShort + valueShort : currentShort - valueShort);
			};

			getSelected = () =>
			{
				List<T> list = new List<T>();
				ushort currentShort = Convert.ToUInt16(CurrentValue);
				foreach (T value in (T[])Enum.GetValues(enumType))
				{
					ushort valueShort = Convert.ToUInt16(value);
					if (valueShort == 0) continue;
					if ((currentShort & valueShort) == valueShort)
						list.Add(value);
				}
				return (list.Empty()) ? new T[] { (T)Enum.ToObject(enumType, 0) } : list.ToArray();
			};
		}

		private void EnumFlagsIntFuncs(Type enumType, out Func<T, bool, T> combineValues, out Func<T[]> getSelected)
		{
			combineValues = (value, selected) =>
			{
				int currentInt = Convert.ToInt32(CurrentValue);
				int valueInt = Convert.ToInt32(value);
				return (T)Enum.ToObject(enumType, (valueInt == 0) ? 0 : (selected) ? currentInt + valueInt : currentInt - valueInt);
			};

			getSelected = () =>
			{
				List<T> list = new List<T>();
				int currentInt = Convert.ToInt32(CurrentValue);
				foreach (T value in (T[])Enum.GetValues(enumType))
				{
					int valueInt = Convert.ToInt32(value);
					if (valueInt == 0) continue;
					if ((currentInt & valueInt) == valueInt)
						list.Add(value);
				}
				return (list.Empty()) ? new T[] { (T)Enum.ToObject(enumType, 0) } : list.ToArray();
			};
		}
		private void EnumFlagsUIntFuncs(Type enumType, out Func<T, bool, T> combineValues, out Func<T[]> getSelected)
		{
			combineValues = (value, selected) =>
			{
				uint currentInt = Convert.ToUInt32(CurrentValue);
				uint valueInt = Convert.ToUInt32(value);
				return (T)Enum.ToObject(enumType, (valueInt == 0) ? 0 : (selected) ? currentInt + valueInt : currentInt - valueInt);
			};

			getSelected = () =>
			{
				List<T> list = new List<T>();
				uint currentInt = Convert.ToUInt32(CurrentValue);
				foreach (T value in (T[])Enum.GetValues(enumType))
				{
					uint valueInt = Convert.ToUInt32(value);
					if (valueInt == 0) continue;
					if ((currentInt & valueInt) == valueInt)
						list.Add(value);
				}
				return (list.Empty()) ? new T[] { (T)Enum.ToObject(enumType, 0) } : list.ToArray();
			};
		}
	}
}