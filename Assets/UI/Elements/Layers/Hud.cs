using System;

using UnityEngine;
using StyleBackground = UnityEngine.UIElements.StyleBackground;

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

			public bool Active { get => ClassListContains("active"); set => EnableInClassList("active", value); }


			public Star()
			{
				star = this.Attach(new Div() { Name = "star" });
				spark = this.Attach(new Div() { Name = "spark" });

				star.style.backgroundImage = new UnityEngine.UIElements.StyleBackground(Monolith.Refs.bountyStars[0]);
			}

			public async void Slam()
			{
				star.TransitionScaleX().Modify(3, 0, EaseFunction.Circular, EaseDirection.Out).Run();
				star.TransitionScaleY().Modify(3, 0, EaseFunction.Circular, EaseDirection.Out).Run();

				star.TransitionTranslateX().Modify(0, 60, 0.4f, EaseFunction.Circular, EaseDirection.Out).Run();
				star.TransitionTranslateY().Modify(0, 10, 0.4f, EaseFunction.Circular, EaseDirection.Out).Run();
				star.TransitionScaleX().Modify(1, 4, 0.4f, EaseFunction.Circular, EaseDirection.Out).Run();
				await star.TransitionScaleY().Modify(1, 4, 0.4f, EaseFunction.Circular, EaseDirection.Out).Await();

				star.TransitionTranslateX().Modify(0, 0.2f, EaseFunction.Exponential, EaseDirection.Out).Run();
				star.TransitionTranslateY().Modify(0, 0.2f, EaseFunction.Exponential, EaseDirection.Out).Run();
				star.TransitionScaleX().Modify(1, 0.2f, EaseFunction.Exponential, EaseDirection.Out).Run();
				await star.TransitionScaleY().Modify(1, 0.2f, EaseFunction.Exponential, EaseDirection.Out).Await();

				spark.style.backgroundImage = new StyleBackground(Monolith.Refs.bountySparks[0]);
				await Awaitable.WaitForSecondsAsync(0.05f);
				spark.style.backgroundImage = new StyleBackground(Monolith.Refs.bountySparks[1]);
				await Awaitable.WaitForSecondsAsync(0.05f);
				spark.style.backgroundImage = new StyleBackground(Monolith.Refs.bountySparks[2]);
				await Awaitable.WaitForSecondsAsync(0.1f);
				spark.style.backgroundImage = new StyleBackground(Monolith.Refs.bountySparks[3]);
				await Awaitable.WaitForSecondsAsync(0.1f);
				spark.style.backgroundImage = new StyleBackground(Monolith.Refs.bountySparks[4]);
				await Awaitable.WaitForSecondsAsync(0.2f);
				spark.style.backgroundImage = UnityEngine.UIElements.StyleKeyword.Null;
			}
			public async void Shine()
			{
				star.style.backgroundImage = new StyleBackground(Monolith.Refs.bountyStars[1]);
				await Awaitable.WaitForSecondsAsync(0.1f);
				star.style.backgroundImage = new StyleBackground(Monolith.Refs.bountyStars[2]);
				await Awaitable.WaitForSecondsAsync(0.1f);
				star.style.backgroundImage = new StyleBackground(Monolith.Refs.bountyStars[3]);
				await Awaitable.WaitForSecondsAsync(0.15f);
				star.style.backgroundImage = new StyleBackground(Monolith.Refs.bountyStars[4]);
				await Awaitable.WaitForSecondsAsync(0.15f);
				star.style.backgroundImage = new StyleBackground(Monolith.Refs.bountyStars[5]);
				await Awaitable.WaitForSecondsAsync(0.2f);
				star.style.backgroundImage = new StyleBackground(Monolith.Refs.bountyStars[6]);
				await Awaitable.WaitForSecondsAsync(0.2f);
				star.style.backgroundImage = new StyleBackground(Monolith.Refs.bountyStars[0]);
			}
		}
		#endregion Star


		protected override bool DefaultFocusable => false;
		protected override bool DefaultPickable => false;

		public readonly Label wantedSign;
		public readonly Label fortuneSign;
		public readonly Label timeSign;
		public readonly Star[] stars;
		public readonly Div bullet;
		public readonly Div[] specials;
		public readonly Div noBulletWarning;


		public Hud()
		{
			wantedSign = this.Attach(new Label() { Name = "wanted", Classes = "sign" });
			fortuneSign = this.Attach(new Label() { Name = "fortune", Classes = "sign" });
			timeSign = this.Attach(new Label() { Name = "time", Classes = "sign" });
			timeSign.schedule.Execute(UpdateTime).Every(1000);

			Div sidebar = this.Attach(new Div() { Name = "stars" });
			stars = new Star[5];
			stars[0] = sidebar.Attach(new Star());
			stars[1] = sidebar.Attach(new Star());
			stars[2] = sidebar.Attach(new Star());
			stars[3] = sidebar.Attach(new Star());
			stars[4] = sidebar.Attach(new Star());
			sidebar.schedule.Execute(ShineStars).Every(5000);

			Div bulletOutline = this.Attach(new Div() { Classes = "bullet" });
			bullet = bulletOutline.Attach(new Div() { Classes = "charge" });

			Div specialMeter = this.Attach(new Div() { Name = "special-meter" });
			specials = new Div[7];
			for (int i = 0; i < specials.Length; i++)
			{
				Div specialOutline = specialMeter.Attach(new Div() { Classes = "special" });
				specials[i] = specialOutline.Attach(new Div() { Classes = "charge" });
			}

			noBulletWarning = this.Attach(new Div() { Name = "no-bullet-warning" });
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
		public async void SetStars(int count)
		{
			for (int i = count; i < 5; i++)
				stars[i].Active = false;

			for (int i = 0; i < count; i++)
			{
				Star star = stars[i];
				if (star.Active) continue;

				star.Active = true;
				star.Slam();
				await Awaitable.WaitForSecondsAsync(0.4f);
			}
		}
	}
}