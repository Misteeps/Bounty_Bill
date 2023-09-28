using System;

using UnityEngine;
using UnityEngine.UIElements;

using UnityEditor;
using UnityEditor.UIElements;

using Simplex.UI;


namespace Simplex.Editor
{
	public class ColorPicker : Field<Color>
	{
		protected override string[] DefaultClasses => new string[] { "field", "color" };

		public readonly IMGUIContainer colorField;

		public bool Alpha { get; set; } = true;
		public bool HDR { get; set; } = false;
		public bool EyeDropper { get; set; } = true;
		public bool Delayed { get; set; } = false;


		public ColorPicker()
		{
			colorField = this.Attach(new IMGUIContainer(DrawGUI));
		}

		protected virtual void DrawGUI()
		{
			CurrentValue = EditorGUILayout.ColorField(GUIContent.none, (Delayed) ? CurrentValue : BindedValue, EyeDropper, Alpha, HDR, GUILayout.Width(resolvedStyle.width), GUILayout.Height(resolvedStyle.height));

			if (!Delayed && BindedValue != CurrentValue)
				BindedValue = CurrentValue;
		}
	}
}