using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

using UnityEditor;

using Simplex.UI;


namespace Simplex.Editor
{
	public class CustomWindow : EditorWindow
	{
		public VisualElement Root => rootVisualElement;

		public string guid;
		public Object Target
		{
			get
			{
				try
				{
					if (guid.Empty()) throw new Exception("Null guid");
					Object target = AssetUtilities.LoadGuid<Object>(guid);
					return target ?? throw new Exception("Asset not found");
				}
				catch (Exception exception)
				{
					try
					{
						Close();
						exception.Error(ConsoleUtilities.uiTag, $"Failed getting target object of custom window. Closing {this:ref}");
					}
					catch { }

					return null;
				}
			}
			set
			{
				if (value == null)
				{
					guid = null;
					Title = this.GetTypeName();
				}
				else
				{
					guid = value.GetGuid();
					Title = value.name;
				}
			}
		}
		public string Title
		{
			get => titleContent.text;
			set => titleContent.text = value;
		}

		public event Action Closed;
		public event Action Focused;
		public event Action Unfocused;


		public CustomWindow OpenTab(params Type[] dockNeighbors)
		{
			if (typeof(EditorWindow).GetField("m_Parent", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this) == null)
				if (dockNeighbors.Empty() || !Dock(dockNeighbors))
					Show();

			Focus();

			return this;
		}
		public CustomWindow OpenModal()
		{
			ShowModal();

			return this;
		}
		public CustomWindow OpenWindow(bool closeOnUnfocus)
		{
			ShowUtility();

			if (closeOnUnfocus)
				Unfocused += Close;

			return this;
		}
		public CustomWindow OpenPopup(bool closeOnUnfocus)
		{
			ShowPopup();

			if (closeOnUnfocus)
				Unfocused += Close;

			return this;
		}
		public CustomWindow OpenDropdown(VisualElement source, float width = 0, float height = 440) => OpenDropdown(source.worldBound, width, height);
		public CustomWindow OpenDropdown(Rect source, float width = 0, float height = 440)
		{
			Rect position = GUIUtility.GUIToScreenRect(source);
			position.xMin += 1;
			position.xMax -= 1;
			position.yMin -= 1;
			position.yMax += 1;

			ShowAsDropDown(position, new Vector2((width == 0) ? position.width : width, height));

			return this;
		}

		public CustomWindow Size(float width = 560, float height = 320)
		{
			position = new Rect(position.x, position.y, width, height);
			return this;
		}
		public CustomWindow Position(float x = 0, float y = 0)
		{
			position = new Rect(x, y, position.width, position.height);
			return this;
		}
		public CustomWindow Center()
		{
			Rect main = EditorGUIUtility.GetMainWindowPosition();
			int x = Mathf.RoundToInt((main.x + main.width - position.width) / 2);
			int y = Mathf.RoundToInt((main.y + main.height - position.height) / 2);
			position = new Rect(x, y, position.width, position.height);

			return this;
		}
		public CustomWindow Clear()
		{
			Root.styleSheets.Clear();
			Root.ClearClassList();
			Root.Clear();

			return this;
		}

		protected virtual void OnEnable() => Undo.undoRedoPerformed += RefreshAll;
		protected virtual void OnDisable() => Undo.undoRedoPerformed -= RefreshAll;
		protected virtual void OnDestroy() => Closed?.Invoke();
		protected virtual void OnFocus() => Focused?.Invoke();
		protected virtual void OnLostFocus() => Unfocused?.Invoke();
		private void RefreshAll() => Root?.Refresh(true, true);

		public void Notification(string text, double duration = 4) => ShowNotification(new GUIContent(text), duration);
		private bool Dock(params Type[] neighbors)
		{
			Type containerWindowType = Type.GetType("UnityEditor.ContainerWindow, UnityEditor.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
			PropertyInfo windowsProperty = containerWindowType.GetProperty("windows");
			PropertyInfo rootViewProperty = containerWindowType.GetProperty("rootView");

			Type viewType = Type.GetType("UnityEditor.View, UnityEditor.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
			PropertyInfo allChildrenProperty = viewType.GetProperty("allChildren");

			Type dockAreaType = Type.GetType("UnityEditor.DockArea, UnityEditor.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
			FieldInfo panesField = dockAreaType.GetField("m_Panes", BindingFlags.Instance | BindingFlags.NonPublic);
			MethodInfo addTabMethod = dockAreaType.GetMethod("AddTab", new Type[] { typeof(EditorWindow), typeof(bool) });

			foreach (Type type in neighbors)
				try
				{
					foreach (object containerWindow in windowsProperty.GetValue(null) as Array)
						foreach (object view in allChildrenProperty.GetValue(rootViewProperty.GetValue(containerWindow)) as Array)
							if (view.GetType() == dockAreaType)
							{
								List<EditorWindow> windows = panesField.GetValue(view) as List<EditorWindow>;
								if (windows.Any(window => window != this && window.GetType() == type))
								{
									addTabMethod.Invoke(view, new object[] { this, true });
									return true;
								}
							}
				}
				catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed docking {this:ref} next to {type:type}"); }

			return false;
		}
	}


	public abstract class CustomWindow<TWindow> : CustomWindow where TWindow : CustomWindow
	{
		public new TWindow OpenTab(params Type[] dockNeighbors) => base.OpenTab(dockNeighbors) as TWindow;
		public new TWindow OpenModal() => base.OpenModal() as TWindow;
		public new TWindow OpenWindow(bool closeOnUnfocus) => base.OpenWindow(closeOnUnfocus) as TWindow;
		public new TWindow OpenPopup(bool closeOnUnfocus) => base.OpenPopup(closeOnUnfocus) as TWindow;
		public new TWindow OpenDropdown(VisualElement source, float width = 0, float height = 440) => base.OpenDropdown(source, width, height) as TWindow;
		public new TWindow OpenDropdown(Rect source, float width = 0, float height = 440) => base.OpenDropdown(source, width, height) as TWindow;

		public new TWindow Size(float width = 560, float height = 320) => base.Size(width, height) as TWindow;
		public new TWindow Position(float x = 0, float y = 0) => base.Position(x, y) as TWindow;
		public new TWindow Center() => base.Center() as TWindow;
		public new TWindow Clear() => base.Clear() as TWindow;

		public static TWindow Get(Object target) => Find(target.GetGuid()) ?? Create(target);
		public static TWindow Get(string guid) => Find(guid) ?? Create(guid);
		public static TWindow Get() => Find() ?? Create();

		public static TWindow Create(Object target)
		{
			if (target == null) throw new NullReferenceException("Null target").Overwrite(ConsoleUtilities.uiTag, $"Failed creating custom window {typeof(TWindow):type}");

			TWindow window = Create();
			window.Target = target;

			return window;
		}
		public static TWindow Create(string guid)
		{
			if (guid.Empty()) throw new ArgumentException("Null guid").Overwrite(ConsoleUtilities.uiTag, $"Failed creating custom window {typeof(TWindow):type}");

			TWindow window = Create();
			window.guid = guid;

			return window;
		}
		public static TWindow Create()
		{
			TWindow window = CreateInstance<TWindow>();
			window.hideFlags = HideFlags.HideAndDontSave;
			window.Title = window.GetTypeName();

			return window;
		}

		public static TWindow Find() => Find(null);
		public static TWindow Find(string guid)
		{
			TWindow[] windows = (guid.Empty()) ? FindAll() : FindAll(guid);

			if (windows.Length == 0) return null;
			if (windows.Length == 1) return windows[0];

			for (int i = 1; i < windows.Length; i++)
				try { windows[i].Close(); }
				catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed closing custom window {windows[i]:ref}"); }

			return windows[0];
		}
		public static TWindow[] FindAll() => Resources.FindObjectsOfTypeAll<TWindow>();
		public static TWindow[] FindAll(string guid) => Resources.FindObjectsOfTypeAll<TWindow>().Where(window => window.guid == guid).ToArray();
	}


	public abstract class CustomWindow<TWindow, TElement> : CustomWindow<TWindow> where TWindow : CustomWindow where TElement : VisualElement, new()
	{
		public virtual StyleSheet[] StyleSheets => new StyleSheet[] { UnityStyleSheets.Common };
		public TElement Element { get; private set; }


		protected virtual TElement CreateElement() => new TElement().Flex(true);

		protected override void OnEnable()
		{
			base.OnEnable();

			Element ??= CreateElement();

			Root.Clear();
			Root.Style(true, StyleSheets);
			Root.Add(Element);
		}
	}
}