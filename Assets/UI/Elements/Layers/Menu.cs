using System;

using UnityEngine;

using Simplex;
using Simplex.UI;


namespace Game.UI
{
	public class Menu : Layer<Menu>
	{
		public readonly Div title;
		public readonly Button play;
		public readonly Button settings;
		public readonly Button quit;


		public Menu()
		{
			title = this.Attach(new Div() { Name = "title" });

			Div main = this.Attach(new Div() { Name = "main" });
			play = main.Attach(new Button() { Name = "play", Text = "Play", Size = Size.Huge }).Bind(_ => Menu.Hide());
			settings = main.Attach(new Button() { Name = "settings", Text = "Settings", Size = Size.Huge }).Bind(_ => Settings.Show());
			quit = main.Attach(new Button() { Name = "quit", Text = "Quit", Size = Size.Huge }).Bind(_ => GeneralUtilities.Quit());

			Shown += () => Monolith.Instance.enabled = false;
			Hidden += () => { Monolith.Instance.enabled = true; Monolith.Paused = false; };
		}
	}
}