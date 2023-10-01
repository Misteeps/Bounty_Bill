using System;

using UnityEngine;

using Simplex;
using Simplex.UI;


namespace Game.UI
{
	public class Hud : Layer<Hud>
	{
		#region Star
		public class Star : Div
		{
			protected override string[] DefaultClasses => new string[] { "star" };

			public readonly Div star;
			public readonly Div spark;


			public Star()
			{
				star = this.Attach(new Div() { Name = "star" });
				spark = this.Attach(new Div() { Name = "spark" });

				star.style.backgroundImage = new UnityEngine.UIElements.StyleBackground(Monolith.Refs.bountyStars[0]);
			}

			public async void Shine()
			{
				star.style.backgroundImage = new UnityEngine.UIElements.StyleBackground(Monolith.Refs.bountyStars[1]);
				await Awaitable.WaitForSecondsAsync(0.1f);
				star.style.backgroundImage = new UnityEngine.UIElements.StyleBackground(Monolith.Refs.bountyStars[2]);
				await Awaitable.WaitForSecondsAsync(0.1f);
				star.style.backgroundImage = new UnityEngine.UIElements.StyleBackground(Monolith.Refs.bountyStars[3]);
				await Awaitable.WaitForSecondsAsync(0.15f);
				star.style.backgroundImage = new UnityEngine.UIElements.StyleBackground(Monolith.Refs.bountyStars[4]);
				await Awaitable.WaitForSecondsAsync(0.15f);
				star.style.backgroundImage = new UnityEngine.UIElements.StyleBackground(Monolith.Refs.bountyStars[5]);
				await Awaitable.WaitForSecondsAsync(0.2f);
				star.style.backgroundImage = new UnityEngine.UIElements.StyleBackground(Monolith.Refs.bountyStars[6]);
				await Awaitable.WaitForSecondsAsync(0.2f);
				star.style.backgroundImage = new UnityEngine.UIElements.StyleBackground(Monolith.Refs.bountyStars[0]);
			}
		}
		#endregion Star


		protected override bool DefaultFocusable => false;
		protected override bool DefaultPickable => false;

		public readonly Star[] stars;
		public readonly Label wantedSign;
		public readonly Label fortuneSign;
		public readonly Label timeSign;


		public Hud()
		{
			Div sidebar = this.Attach(new Div() { Name = "stars" });
			stars = new Star[5];
			stars[0] = sidebar.Attach(new Star());
			stars[1] = sidebar.Attach(new Star());
			stars[2] = sidebar.Attach(new Star());
			stars[3] = sidebar.Attach(new Star());
			stars[4] = sidebar.Attach(new Star());
			sidebar.schedule.Execute(ShineStars).Every(5000);

			wantedSign = this.Attach(new Label() { Name = "wanted", Classes = "sign" });
			fortuneSign = this.Attach(new Label() { Name = "fortune", Classes = "sign" });
			timeSign = this.Attach(new Label() { Name = "time", Classes = "sign" });
			timeSign.schedule.Execute(UpdateTime).Every(1000);
		}

		public async void ShineStars()
		{
			stars[0].Shine();
			await Awaitable.WaitForSecondsAsync(0.1f);
			stars[1].Shine();
			await Awaitable.WaitForSecondsAsync(0.2f);
			stars[2].Shine();
			await Awaitable.WaitForSecondsAsync(0.3f);
			stars[3].Shine();
			await Awaitable.WaitForSecondsAsync(0.35f);
			stars[4].Shine();
		}
		public void UpdateWanted()
		{
			wantedSign.text = $"$ {Monolith.bounty:N0}";
			wantedSign.AddToClassList("pop");
			wantedSign.schedule.Execute(() => wantedSign.RemoveFromClassList("pop")).ExecuteLater(1);
		}
		public void UpdateFortune()
		{
			fortuneSign.text = $"$ {Monolith.fortune:N0}";
			fortuneSign.AddToClassList("pop");
			fortuneSign.schedule.Execute(() => fortuneSign.RemoveFromClassList("pop")).ExecuteLater(1);
		}
		public void UpdateTime() => timeSign.text = $"{(int)(Monolith.time / 60)}:{(int)(Monolith.time % 60):00}<br>0:{(int)(Enemies.difficulty.wavesDelay - Enemies.timer):00}";
	}
}