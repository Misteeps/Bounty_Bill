using System;

using UnityEngine;
using UnityEngine.UIElements;


namespace Simplex.UI
{
	public class Button : Field
	{
		protected override string[] DefaultClasses => new string[] { "field", "button", "outset", "text", "icon" };

		public override string Text
		{
			set
			{
				if (Name == Text) Name = null;
				base.Text = value;
				if (Name.Empty()) Name = value;
			}
		}
		public Texture2D Icon
		{
			get => resolvedStyle.backgroundImage.texture;
			set
			{
				if (Icon != null && Name == Icon.name) Name = null;
				style.backgroundImage = (value == null) ? StyleKeyword.Null : value;
				if (Icon != null && Name.Empty()) Name = Icon.name;
			}
		}

		public EventCallback<ClickEvent> onClick;


		public Button()
		{
			RegisterCallback<ClickEvent>(OnClick);
		}

		public Button Bind(EventCallback<ClickEvent> onClick)
		{
			this.onClick = onClick;

			return this;
		}

		protected virtual void OnClick(ClickEvent evnt)
		{
			onClick.Invoke(evnt);

			evnt.StopPropagation();
		}
	}
}