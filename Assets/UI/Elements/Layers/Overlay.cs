using System;

using UnityEngine;

using Simplex;
using Simplex.UI;


namespace Game.UI
{
	public class Overlay : Layer<Overlay>
	{
		protected override bool DefaultFocusable => false;
		protected override bool DefaultPickable => false;

		public readonly Label fps;
		public readonly Div crosshair;

		public static bool Faded { get => Instance.ClassListContains("fade"); set => Instance.EnableInClassList("fade", value); }


		public Overlay()
		{
			fps = this.Attach(new Label() { Name = "fps", Size = Size.Small });
			fps.schedule.Execute(UpdateFPS).Every(1000);

			crosshair = this.Attach(new Div() { Name = "crosshair" });
		}

		public void ShowFPS(bool show) => fps.Display(show);
		private void UpdateFPS()
		{
			int fpsCount = (int)(1f / Time.unscaledDeltaTime);
			fps.Text = $"FPS: {fpsCount}";
		}

		public void UpdateCrosshair(bool? showHardware = null)
		{
			bool visible = (showHardware != null) ? (bool)showHardware : Game.Settings.customCursor && !Monolith.Paused;
			UnityEngine.Cursor.visible = !visible;

			if (visible)
			{
				Inputs.UpdateScreenCursor(Screen.width, Screen.height);
				crosshair.style.top = Inputs.screenCursor.y;
				crosshair.style.left = Inputs.screenCursor.x;
			}
			else
			{
				crosshair.style.top = -100;
				crosshair.style.left = -100;
			}
		}
		public void ColorCrosshair(Color color) => crosshair.style.unityBackgroundImageTintColor = color;
	}
}