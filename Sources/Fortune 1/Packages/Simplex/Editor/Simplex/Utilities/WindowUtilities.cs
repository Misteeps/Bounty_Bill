using System;

using UnityEngine;

using UnityEditor;


namespace Simplex.Editor
{
	public static class WindowUtilities
	{
		public static Type InspectorType { get; } = Type.GetType("UnityEditor.InspectorWindow, UnityEditor.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
		public static Type ProjectType { get; } = Type.GetType("UnityEditor.ProjectBrowser, UnityEditor.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
		public static Type ConsoleType { get; } = Type.GetType("UnityEditor.ConsoleWindow, UnityEditor.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
		public static Type HierarchyType { get; } = Type.GetType("UnityEditor.SceneHierarchyWindow, UnityEditor.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
		public static Type SceneType { get; } = typeof(SceneView);
		public static Type GameType { get; } = Type.GetType("UnityEditor.GameView, UnityEditor.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

		public static EditorWindow[] InspectorWindows => (EditorWindow[])Resources.FindObjectsOfTypeAll(InspectorType);
		public static EditorWindow[] ProjectWindows => (EditorWindow[])Resources.FindObjectsOfTypeAll(ProjectType);
		public static EditorWindow[] ConsoleWindows => (EditorWindow[])Resources.FindObjectsOfTypeAll(ConsoleType);
		public static EditorWindow[] HierarchyWindows => (EditorWindow[])Resources.FindObjectsOfTypeAll(HierarchyType);
		public static EditorWindow[] SceneWindows => (EditorWindow[])Resources.FindObjectsOfTypeAll(SceneType);
		public static EditorWindow[] GameWindows => (EditorWindow[])Resources.FindObjectsOfTypeAll(GameType);
	}
}