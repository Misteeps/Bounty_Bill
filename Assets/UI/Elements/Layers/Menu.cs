using System;

using UnityEngine;

using Simplex;
using Simplex.UI;


namespace Game.UI
{
	public class Menu : Layer<Menu>
	{
		public readonly Div title;
		public readonly Div bill;
		public readonly Button play;
		public readonly Button settings;
		public readonly Button quit;


		public Menu()
		{
			title = this.Attach(new Div() { Name = "title" });
			bill = this.Attach(new Div() { Name = "bill" });

			Div main = this.Attach(new Div() { Name = "main" });
			play = main.Attach(new Button() { Name = "play", Size = Size.Huge }).Bind(_ => { Menu.Hide(); Monolith.GameStart(); });
			settings = main.Attach(new Button() { Name = "settings", Size = Size.Huge }).Bind(_ => Settings.Show());
			quit = main.Attach(new Button() { Name = "quit", Size = Size.Huge }).Bind(_ => GeneralUtilities.Quit());

			play.RegisterCallback<UnityEngine.UIElements.MouseEnterEvent>(_ => Audio.UI.global.PlayOneShot(Monolith.Refs.buttonHover));
			settings.RegisterCallback<UnityEngine.UIElements.MouseEnterEvent>(_ => Audio.UI.global.PlayOneShot(Monolith.Refs.buttonHover));
			quit.RegisterCallback<UnityEngine.UIElements.MouseEnterEvent>(_ => Audio.UI.global.PlayOneShot(Monolith.Refs.buttonHover));

			Shown += () => SwapAudio(Monolith.Refs.menuMusic);
			Hidden += () => SwapAudio(Monolith.Refs.gameMusic);
		}

		private async void SwapAudio(AudioClip clip)
		{
			await Audio.Music.global.TransitionVolume().Modify(1, 0, 1, EaseFunction.Linear, EaseDirection.InOut).Await();
			Audio.Music.global.Stop();
			Audio.Music.global.clip = clip;
			Audio.Music.global.Play();
			Audio.Music.global.TransitionVolume().Modify(0, 1, 1, EaseFunction.Linear, EaseDirection.InOut).Run();
		}
	}
}