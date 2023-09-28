using System;
using System.Reflection;

using UnityEngine;
using UnityEngine.UIElements;

using UnityEditor;

using Simplex.UI;


namespace Simplex.Editor
{
	public static class UnityStyleSheets
	{
		private static Type UIElementsEditorUtilityType { get; } = Type.GetType("UnityEditor.UIElements.UIElementsEditorUtility, UnityEditor.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
		private static MethodInfo CommonDarkMethod { get; } = UIElementsEditorUtilityType.GetMethod("GetCommonDarkStyleSheet", BindingFlags.Static | BindingFlags.NonPublic);
		private static MethodInfo CommonLightMethod { get; } = UIElementsEditorUtilityType.GetMethod("GetCommonLightStyleSheet", BindingFlags.Static | BindingFlags.NonPublic);

		public static StyleSheet Common => (EditorGUIUtility.isProSkin) ? CommonDark : CommonLight;
		public static StyleSheet CommonDark => CommonDarkMethod.Invoke(null, null) as StyleSheet;
		public static StyleSheet CommonLight => CommonLightMethod.Invoke(null, null) as StyleSheet;
	}
}