using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;


namespace Simplex
{
	public static class GeneralUtilities
	{
#if UNITY_EDITOR
		[UnityEditor.InitializeOnLoadMethod]
		private static void CheckCodeStripping()
		{
			if (UnityEditor.PlayerSettings.GetManagedStrippingLevel(UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(UnityEditor.EditorUserBuildSettings.selectedBuildTargetGroup)) == UnityEditor.ManagedStrippingLevel.Disabled)
				ConsoleUtilities.Warn($"Code Stripping is disabled in player settings. It is <b>HIGHLY RECOMMENDED</b> to enable Managed Stripping Level (Minimal or higher) when using Simplex Package");
		}
#endif

		public static bool Empty(this string str) => str == null || str.Length == 0;
		public static bool Empty<T>(this T[] array) => array == null || array.Length == 0;
		public static bool Empty<T>(this List<T> list) => list == null || list.Count == 0;
		public static bool Empty<T1, T2>(this Dictionary<T1, T2> dictionary) => dictionary == null || dictionary.Count == 0;
		public static bool OutOfRange(this string str, int index) => str == null || index < 0 || index >= str.Length;
		public static bool OutOfRange<T>(this T[] array, int index) => array == null || index < 0 || index >= array.Length;
		public static bool OutOfRange<T>(this List<T> list, int index) => list == null || index < 0 || index >= list.Count;
		public static T[] Append<T>(this T[] array, IEnumerable<T> items, bool checkDuplicate = false)
		{
			if (array.Empty()) return items.ToArray();

			int count = items.Count();
			if (count == 0) return array;

			int i;
			T[] newArray = new T[array.Length + count];
			for (i = 0; i < array.Length; i++)
				newArray[i] = array[i];

			if (checkDuplicate)
				foreach (T item in items)
				{
					if (Array.IndexOf(newArray, item) != -1)
					{
						ConsoleUtilities.Warn($"{typeof(T):type} array already contains {item:ref}. Cancelled append");
						return array;
					}
					newArray[i++] = item;
				}
			else
				foreach (T item in items)
					newArray[i++] = item;

			return newArray;
		}
		public static T[] Append<T>(this T[] array, T item, bool checkDuplicate = false)
		{
			if (array.Empty()) return new T[1] { item };

			if (checkDuplicate && Array.IndexOf(array, item) != -1)
			{
				ConsoleUtilities.Warn($"{typeof(T):type} array already contains {item:ref}. Cancelled append");
				return array;
			}

			T[] newArray = new T[array.Length + 1];
			for (int i = 0; i < array.Length; i++)
				newArray[i] = array[i];

			newArray[array.Length] = item;
			return newArray;
		}
		public static T[] Remove<T>(this T[] array, T item, bool checkExistance = false)
		{
			if (array.Empty()) return array;

			int index = Array.IndexOf(array, item);

			if (checkExistance && index == -1)
			{
				ConsoleUtilities.Warn($"{typeof(T):type} array does not contain {item:ref}. Cancelled Remove");
				return array;
			}

			return (index == -1) ? array : Remove(array, index);
		}
		public static T[] Remove<T>(this T[] array, int index)
		{
			if (array.Empty()) return array;
			if (array.OutOfRange(index)) throw new IndexOutOfRangeException();

			T[] newArray = new T[array.Length - 1];
			for (int i = 0, j = 0; i < array.Length; i++)
				if (i != index)
					newArray[j++] = array[i];

			return newArray;
		}

		public static void ScaleImage(this Texture2D image, int width, int height, bool keepAspectRatio = false, bool mipmap = false, FilterMode filter = FilterMode.Bilinear)
		{
			if (keepAspectRatio)
			{
				float ratioW = image.width / (float)width;
				float ratioH = image.height / (float)height;
				float ratio = Mathf.Max(ratioW, ratioH);

				width = Mathf.RoundToInt(image.width / ratio);
				height = Mathf.RoundToInt(image.height / ratio);
			}

			RenderTexture rt = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
			RenderTexture.active = rt;

			Graphics.Blit(image, rt);
			image.Reinitialize(width, height, image.format, mipmap);
			image.filterMode = filter;

			try
			{
				image.ReadPixels(new Rect(0.0f, 0.0f, width, height), 0, 0);
				image.Apply();
			}
			catch (Exception exception) { exception.Error($"Failed scaling image"); }

			RenderTexture.ReleaseTemporary(rt);
		}

		public static void Quit()
		{
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#elif !UNITY_WEBGL
			Application.Quit();
#endif
		}
	}
}