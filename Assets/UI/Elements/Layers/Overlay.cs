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


		public Overlay()
		{
			fps = this.Attach(new Label() { Name = "fps", Size = Size.Small });
			fps.schedule.Execute(UpdateFPS).Every(1000);
		}

		private void UpdateFPS()
		{
			int fpsCount = (int)(1f / Time.unscaledDeltaTime);
			fps.Text = $"FPS: {fpsCount}";
		}
	}
}