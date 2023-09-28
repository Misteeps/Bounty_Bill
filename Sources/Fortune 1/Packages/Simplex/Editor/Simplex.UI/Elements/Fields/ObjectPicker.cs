using System;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

using UnityEditor;
using UnityEditor.UIElements;

using Simplex.UI;


namespace Simplex.Editor
{
	public class ObjectPicker : ObjectPicker<Object> { }
	public class ObjectPicker<T> : Field<T> where T : Object
	{
		protected override string[] DefaultClasses => new string[] { "field", "object", "inset", "text", "icon" };

		public readonly UI.Button search;

		public string[] PopupClasses { get; set; } = null;
		public bool InheritStyleSheets { get; set; } = true;
		public string Filter { get; set; } = null;
		public bool Searchable { get; set; } = true;
		public bool AllowSceneObjects { get; set; } = true;

		public override T CurrentValue
		{
			set
			{
				base.CurrentValue = value;
				Text = (value) ? ConsoleUtilities.Format($"{value:type} {value.name}") : ConsoleUtilities.Format($"{typeof(T):type} {ConsoleUtilities.nullTag}");
				style.backgroundImage = value.GetIcon();
			}
		}


		public ObjectPicker()
		{
			search = this.Attach(new UI.Button() { Classes = "search, right" }.Bind(OnSearch));

			RegisterCallback<ClickEvent>(OnClick);
			RegisterCallback<KeyDownEvent>(OnKeyDown);
			RegisterCallback<DragEnterEvent>(OnDragEnter);
			RegisterCallback<DragLeaveEvent>(OnDragLeave);
			RegisterCallback<DragUpdatedEvent>(OnDragUpdated);
			RegisterCallback<DragPerformEvent>(OnDragPerform);
		}

		protected virtual void OnSearch(ClickEvent evnt)
		{
			if (evnt.shiftKey)
			{
				Type selectorType = Type.GetType("UnityEditor.ObjectSelector, UnityEditor.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
				PropertyInfo getProperty = selectorType.GetProperty("get", BindingFlags.Static | BindingFlags.Public);
				MethodInfo showMethod = selectorType.GetMethod("Show", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(Object), typeof(Type), typeof(Object), typeof(bool), typeof(List<int>), typeof(Action<Object>), typeof(Action<Object>), typeof(bool) }, null);
				object selector = getProperty.GetValue(null, null);
				Action<Object> onSet = value => BindedValue = value as T;
				showMethod.Invoke(selector, new object[] { CurrentValue, typeof(T), null, AllowSceneObjects, null, null, onSet, true });
			}
			else
			{
				Popup popup = new Popup().Class(PopupClasses).Open(this, InheritStyleSheets);
				popup.Attach(new DirectoryView<T>() { Flexible = true, Searchable = Searchable, onSelect = (value, selected) => { BindedValue = value; popup.Close(); } }.Bind(AssetUtilities.Find<T>(Filter), CurrentValue));
			}

			evnt.StopPropagation();
		}
		protected virtual void OnClick(ClickEvent evnt)
		{
			if (CurrentValue == null) return;

			if (evnt.clickCount == 1)
			{
				if (!evnt.shiftKey && !evnt.ctrlKey)
					EditorGUIUtility.PingObject(CurrentValue);
			}
			else if (evnt.clickCount == 2)
			{
				AssetDatabase.OpenAsset(CurrentValue);
				GUIUtility.ExitGUI();
			}

			evnt.StopPropagation();
		}
		protected virtual void OnKeyDown(KeyDownEvent evnt)
		{
			switch (evnt.keyCode)
			{
				case KeyCode.Space:
				case KeyCode.Return:
				case KeyCode.KeypadEnter:
					break;

				case KeyCode.Delete:
					BindedValue = null;
					break;

				default: return;
			}

			evnt.StopPropagation();
		}
		protected virtual void OnDragEnter(DragEnterEvent evnt)
		{
			this.ClassToggle("valid", "invalid", ValidateDrag(out _));

			evnt.StopPropagation();
		}
		protected virtual void OnDragLeave(DragLeaveEvent evnt)
		{
			RemoveFromClassList("valid");
			RemoveFromClassList("invalid");

			evnt.StopPropagation();
		}
		protected virtual void OnDragUpdated(DragUpdatedEvent evnt)
		{
			DragAndDrop.visualMode = (ValidateDrag(out _)) ? DragAndDropVisualMode.Generic : DragAndDropVisualMode.Rejected;

			evnt.StopPropagation();
		}
		protected virtual void OnDragPerform(DragPerformEvent evnt)
		{
			RemoveFromClassList("valid");
			RemoveFromClassList("invalid");

			if (ValidateDrag(out T value))
			{
				DragAndDrop.AcceptDrag();
				BindedValue = value;
			}

			evnt.StopPropagation();
		}

		protected virtual bool ValidateDrag(out T value)
		{
			Object[] objectReferences = DragAndDrop.objectReferences;

			if (objectReferences.Empty())
			{
				value = null;
				return false;
			}

			value = objectReferences[0] as T;
			return value != null;
		}
	}
}