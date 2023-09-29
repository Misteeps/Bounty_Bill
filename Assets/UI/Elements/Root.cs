using System;

using UnityEngine;

using Simplex;
using Simplex.UI;


namespace Game.UI
{
	public class Root : Div
	{
		public static Root Instance { get; } = Monolith.Refs.uiDocument.rootVisualElement.Attach(new Root().Style(Monolith.Refs.uiStyle));
		public static UnityEngine.UIElements.Focusable Focused => Instance.panel.focusController.focusedElement;

		protected override string[] DefaultClasses => new string[] { "root" };
	}
}