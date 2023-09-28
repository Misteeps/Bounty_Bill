using System;
using System.Linq;
using System.Reflection;

using UnityEngine;
using UnityEngine.UIElements;

using UnityEditor;

using Simplex.UI;


namespace Simplex.Editor
{
	public class CustomInspector : UnityEditor.Editor
	{
		private EditorWindow editorWindow;
		public EditorWindow EditorWindow
		{
			get
			{
				if (editorWindow == null)
				{
					EditorWindow[] windows = WindowUtilities.InspectorWindows;
					PropertyInfo trackerProperty = WindowUtilities.InspectorType.GetProperty("tracker");

					for (int i = 0; i < windows.Length; i++)
					{
						EditorWindow window = windows[i];
						ActiveEditorTracker tracker = trackerProperty.GetValue(window) as ActiveEditorTracker;
						if (tracker.activeEditors.Contains(this))
						{
							editorWindow = window;
							break;
						}
					}
				}

				return editorWindow;
			}
		}

		protected TemplateContainer TemplateContainer { get; set; }
		protected UnityEngine.UIElements.ScrollView ScrollViewContainer { get; set; }
		public Div DefaultContainer { get; protected set; }
		public Div Root { get; protected set; }

		protected override bool ShouldHideOpenButton() => true;
		public override bool UseDefaultMargins() => false;
		public virtual bool UseDefaultContainer => false;
		public override VisualElement CreateInspectorGUI() => DefaultContainer;


		protected virtual void OnEnable()
		{
			Undo.undoRedoPerformed += RefreshAll;

			DefaultContainer ??= new Div();
			Root ??= new Div() { Flexible = true };

			if (UseDefaultContainer)
				RestoreDefaultContainer();
			else
				DisplayCustomInspector();
		}
		protected virtual void OnDisable()
		{
			Undo.undoRedoPerformed -= RefreshAll;

			Root.Clear();
			RestoreDefaultContainer();
		}
		private void RefreshAll() => Root?.Refresh(true, true);

		protected virtual void DisplayCustomInspector()
		{
			try
			{
				if (TemplateContainer == null || ScrollViewContainer == null)
				{
					TemplateContainer = EditorWindow?.rootVisualElement.Q<TemplateContainer>();
					ScrollViewContainer = TemplateContainer?.Q<UnityEngine.UIElements.ScrollView>();
				}

				ScrollViewContainer.Display(false);
				TemplateContainer.Insert(1, Root);
			}
			catch (Exception exception)
			{
				exception.Error(ConsoleUtilities.uiTag, $"Failed displaying custom inspector for {target:ref}");
				RestoreDefaultContainer();
			}
		}
		protected virtual void RestoreDefaultContainer()
		{
			ScrollViewContainer?.Display(StyleKeyword.Null);
			Root.RemoveFromHierarchy();
			DefaultContainer.Add(Root);
		}
	}


	public abstract class CustomInspector<TElement> : CustomInspector where TElement : VisualElement, new()
	{
		public virtual StyleSheet[] StyleSheets { get; }
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