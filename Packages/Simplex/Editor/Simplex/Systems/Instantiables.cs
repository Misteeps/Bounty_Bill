using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;

using UnityEditor;


namespace Simplex.Editor
{
	public static class Instantiables<T>
	{
		#region Record
		public readonly struct Record
		{
			public readonly int order;
			public readonly string path;
			public readonly Type type;


			public Record(int order, string path, Type type)
			{
				this.order = order;
				this.path = path;
				this.type = type;
			}

			public override string ToString() => $"[{order}] \"{path}\" <{type}>";
		}
		#endregion Record


		public static Record[] Records { get; } = GetRecords();
		public static int Length => Records.Length;


		public static Record[] GetRecords()
		{
			List<Record> records = new List<Record>();

			foreach (Type type in TypeCache.GetTypesDerivedFrom<T>())
				try
				{
					InstantiableAttribute attribute = type.GetCustomAttribute<InstantiableAttribute>() ?? throw new Exception("Missing instantiable attribute");
					records.Add(new Record(attribute.order, attribute.path, type));
				}
				catch (Exception exception) { exception.Error($"Failed registering instantiable {type:type}"); }

			return records.OrderBy(i => i.order).ThenBy(i => i.path).ToArray();
		}

		private static List<(string path, Record record)> MenuItems(string filter = null, bool includeSeperators = true)
		{
			List<(string path, Record record)> items = new List<(string path, Record record)>();

			Dictionary<string, int> directories = new Dictionary<string, int>();
			foreach (Record record in Records)
				try
				{
					string directory = record.path;
					while (!directory.Empty())
					{
						directory = Path.GetDirectoryName(directory).Replace('\\', '/');
						if (!directories.TryGetValue(directory, out int order))
							directories.Add(directory, record.order);
						else
						{
							directories[directory] = record.order;
							if (record.order - order >= 10)
								items.Add(((directory.Empty()) ? null : $"{directory}/", default));
						}
					}
					items.Add((record.path, record));
				}
				catch (Exception exception) { exception.Error($"Failed registering menu item for {record:info}"); }

			if (items.Count == 0)
				ConsoleUtilities.Warn($"0 Instantiable menu items found. Context/Generic menu may be empty");

			return items;
		}
		public static GenericMenu GenericMenu(Action<Record> onSelect) => GenericMenu(null, onSelect);
		public static GenericMenu GenericMenu(string filter, Action<Record> onSelect)
		{
			GenericMenu menu = new GenericMenu();

			foreach ((string path, Record record) in MenuItems(filter))
				try
				{
					if (record.type == null)
						menu.AddSeparator(path);
					else
						menu.AddItem(new GUIContent(record.path), false, () => onSelect.Invoke(record));
				}
				catch (Exception exception) { exception.Error($"Failed adding {record:info} to generic menu with {path:info}"); }

			return menu;
		}
		public static GenericDropdownMenu GenericDropdownMenu(Action<Record> onSelect) => GenericDropdownMenu(null, onSelect);
		public static GenericDropdownMenu GenericDropdownMenu(string filter, Action<Record> onSelect)
		{
			GenericDropdownMenu menu = new GenericDropdownMenu();

			foreach ((string path, Record record) in MenuItems(filter))
				try
				{
					if (record.type == null)
						menu.AddSeparator(path);
					else
						menu.AddItem(path, false, () => onSelect.Invoke(record));
				}
				catch (Exception exception) { exception.Error($"Failed adding {record:info} to generic dropdown menu with {path:info}"); }

			return menu;
		}
		public static void PopulateMenu(DropdownMenu menu, Action<Record> onSelect) => PopulateMenu(menu, null, onSelect);
		public static void PopulateMenu(DropdownMenu menu, string filter, Action<Record> onSelect)
		{
			foreach ((string path, Record record) in MenuItems(filter))
				try
				{
					if (record.type == null)
						menu.AppendSeparator(path);
					else
						menu.AppendAction(path, action => onSelect.Invoke(record));
				}
				catch (Exception exception) { exception.Error($"Failed adding {record:info} to dropdown menu with {path:info}"); }
		}

		public static bool FindRecord(string path, out Record record) => FindRecord(record => record.path == path, out record);
		public static bool FindRecord(Predicate<Record> match, out Record record)
		{
			int index = Array.FindIndex(Records, match);
			if (index == -1)
			{
				record = default;
				return false;
			}
			else
			{
				record = Records[index];
				return true;
			}
		}
	}
}