using System;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using UnityEngine;


namespace Simplex
{
	public static class ConsoleUtilities
	{
		#region Tag
		public class Tag
		{
			public readonly string text;


			public Tag(string name, Color color) : this(name, ColorUtility.ToHtmlStringRGB(color)) { }
			public Tag(string name, string colorHex) => text = $"<b><color=#{colorHex}>{name}</color></b>";

			public override string ToString() => text;
		}
		#endregion Tag

		#region Formatter
		private class Formatter : IFormatProvider, ICustomFormatter
		{
			public object GetFormat(Type formatType) => (formatType == typeof(ICustomFormatter)) ? this : null;
			public string Format(string format, object arg, IFormatProvider formatProvider)
			{
				if (format == "ref") return RefFormat(arg);
				if (format == "info") return InfoFormat(arg);
				if (format == "type") return TypeFormat(arg);

				return null;
			}

			public string RefFormat(object obj)
			{
				if (NullOrEmptyObject(obj, out string str)) return str;

				if (obj is UnityEngine.Object unityObj)
					str = unityObj.name;

				return $"<color=#A3A3A3><{obj.GetType().Name}> {str}</color>";
			}
			public string InfoFormat(object obj)
			{
				if (NullOrEmptyObject(obj, out string str)) return str;

				if (obj is UnityEngine.Object unityObj)
					str = unityObj.name;

				return $"<color=#A3A3A3>{str}</color>";
			}
			public string TypeFormat(object obj)
			{
				if (NullOrEmptyObject(obj, out string str)) return str;

				str = (obj is Type type) ? type.Name : obj.GetType().Name;

				return $"<color=#A3A3A3><{str}></color>";
			}

			public bool NullOrEmptyObject(object obj, out string str)
			{
				if (obj == null)
				{
					str = nullTag.ToString();
					return true;
				}

				str = obj.ToString();

				if (string.IsNullOrWhiteSpace(str))
				{
					str = emptyTag.ToString();
					return true;
				}

				return false;
			}
		}
		#endregion Formatter


		public readonly static Tag uiTag = new Tag("[UI]", new Color(1, 1, 1));
		public readonly static Tag fileTag = new Tag("[File]", new Color(1, 1, 1));
		public readonly static Tag transitionTag = new Tag("[Transition]", new Color(1, 1, 1));
		public readonly static Tag nullTag = new Tag("(null)", new Color(0.7f, 0.2f, 0.2f));
		public readonly static Tag emptyTag = new Tag("(?)", new Color(0.7f, 0.2f, 0.2f));

		private static Formatter formatter = new Formatter();


		public static string Format(this FormattableString str) => str?.ToString(formatter);
		public static string Format(this FormattableString str, Tag tag) => (tag == null) ? str.Format() : $"{tag} {str.Format()}";

		public static string TitleCase(this string str) => (str.Empty()) ? str : System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Regex.Replace(str, @"(\p{Ll})(\p{Lu})|(\p{Lu})(\p{Lu}\p{Ll})", "$1$3 $2$4").ToLower());
		public static string GetTypeName(this object obj) => TitleCase((obj is Type type) ? type.Name : obj?.GetType().Name);

		public static void Log(FormattableString message) => Debug.Log(message.Format());
		public static void Log(Tag tag, FormattableString message) => Debug.Log(message.Format(tag));

		public static void Warn(FormattableString message) => Debug.LogWarning(message.Format());
		public static void Warn(Tag tag, FormattableString message) => Debug.LogWarning(message.Format(tag));

		public static void Error(FormattableString message) => Debug.LogError(message.Format());
		public static void Error(Tag tag, FormattableString message) => Debug.LogError(message.Format(tag));

		public static void Error(this Exception exception) => Debug.LogException(exception);
		public static void Error(this Exception exception, FormattableString message) => Debug.LogException(exception.Overwrite(null, message));
		public static void Error(this Exception exception, Tag tag, FormattableString message) => Debug.LogException(exception.Overwrite(tag, message));

		public static Exception Overwrite(this Exception exception, FormattableString message) => Overwrite(exception, null, message);
		public static Exception Overwrite(this Exception exception, Tag tag, FormattableString message)
		{
			FieldInfo field = exception.GetType().GetField("_message", BindingFlags.Instance | BindingFlags.NonPublic);
			field.SetValue(exception, $"{message.Format(tag)}\n> {exception.Message}");

			return exception;
		}


		public static void LogCollection<T>(this IEnumerable<T> collection) => LogCollection(collection, null, item => $"{item}");
		public static void LogCollection<T>(this IEnumerable<T> collection, FormattableString header) => LogCollection(collection, header, item => $"{item}");
		public static void LogCollection<T>(this IEnumerable<T> collection, FormattableString header, Func<T, FormattableString> stringify)
		{
			StringBuilder builder = new StringBuilder();
			builder.Append((header.Format().Empty()) ? $"{typeof(T).Name} Collection" : header.Format());
			builder.Append(" - ");
			builder.Append((collection == null) ? "null" : $"{collection.Count()} Items");

			if (collection != null)
			{
				int index = 0;
				foreach (T item in collection)
					try { builder.Append($"\n{index++}: {stringify.Invoke(item).Format()}"); }
					catch (Exception exception) { exception.Error($"Failed logging collection item [{index}] {item}"); }
			}

			Debug.Log(builder);
		}
	}
}