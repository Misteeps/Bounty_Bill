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

		public readonly Label[] intro;
		public readonly Div results;
		public readonly Label time;
		public readonly Label fortune;
		public readonly Label bounty;
		public readonly Label fps;
		public readonly Div crosshair;

		public static bool Faded { get => Instance.ClassListContains("fade"); set => Instance.EnableInClassList("fade", value); }


		public Overlay()
		{
			Div intros = this.Attach(new Div() { Name = "intro" });
			intro = new Label[5];
			intro[0] = intros.Attach(new Label() { Text = "Everyone" }.Visible(false));
			intro[1] = intros.Attach(new Label() { Text = "Only" }.Visible(false));
			intro[2] = intros.Attach(new Label() { Text = "Gets" }.Visible(false));
			intro[3] = intros.Attach(new Label() { Text = "ONE" }.Visible(false));
			intro[4] = intros.Attach(new Label() { Text = "SHOT" }.Visible(false));

			results = this.Attach(new Div() { Name = "results" }.Display(false));
			results.Attach(new Label() { Name = "title", Text = "Results", Size = Size.Huge });
			results.Attach(new Label() { Name = "continue", Text = "Click to Continue...", Size = Size.Huge });
			Div left = results.Attach(new Div() { Name = "left", Classes = "column", Flexible = true });
			left.Attach(new Label() { Name = "time", Text = "Time:", Size = Size.Huge });
			left.Attach(new Label() { Name = "fortune", Text = "Fortune:", Size = Size.Huge });
			left.Attach(new Label() { Name = "bounty", Text = "Bounty:", Size = Size.Huge });
			Div right = results.Attach(new Div() { Name = "right", Classes = "column", Flexible = true });
			time = right.Attach(new Label() { Name = "time", Text = "0:00", Size = Size.Huge });
			fortune = right.Attach(new Label() { Name = "fortune", Text = "$ 0", Size = Size.Huge });
			bounty = right.Attach(new Label() { Name = "bounty", Text = "$ 0", Size = Size.Huge });

			fps = this.Attach(new Label() { Name = "fps", Size = Size.Small });
			fps.schedule.Execute(UpdateFPS).Every(1000);

			crosshair = this.Attach(new Div() { Name = "crosshair" });
		}

		public async void ShowIntro()
		{
			intro[0].Visible(true);
			await Awaitable.WaitForSecondsAsync(0.4f);
			intro[1].Visible(true);
			await Awaitable.WaitForSecondsAsync(0.4f);
			intro[2].Visible(true);
			await Awaitable.WaitForSecondsAsync(0.8f);
			intro[3].Visible(true);
			await Awaitable.WaitForSecondsAsync(0.8f);
			intro[4].Visible(true);
			await Awaitable.WaitForSecondsAsync(1.6f);

			for (int i = 0; i < intro.Length; i++)
				intro[i].Visible(false);
		}
		public void ShowResults()
		{
			time.text = $"{(int)(Monolith.time / 60)}:{(int)(Monolith.time % 60):00}";
			fortune.text = $"$ {Monolith.fortune:N0}";
			bounty.text = $"$ {Monolith.bounty:N0}";

			results.Display(true);
			results.AddToClassList("show");
		}
		public void HideResults()
		{
			results.RemoveFromClassList("show");
			results.schedule.Execute(() => results.Display(false)).ExecuteLater(400);
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