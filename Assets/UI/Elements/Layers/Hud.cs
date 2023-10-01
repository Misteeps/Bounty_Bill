using System;

using UnityEngine;

using Simplex;
using Simplex.UI;


namespace Game.UI
{
	public class Hud : Layer<Hud>
	{
		protected override bool DefaultFocusable => false;
		protected override bool DefaultPickable => false;

		public readonly Label wantedSign;
		public readonly Label fortuneSign;
		public readonly Label timeSign;


		public Hud()
		{
			wantedSign = this.Attach(new Label() { Name = "wanted", Classes = "sign" });
			fortuneSign = this.Attach(new Label() { Name = "fortune", Classes = "sign" });
			timeSign = this.Attach(new Label() { Name = "time", Classes = "sign" });
			timeSign.schedule.Execute(UpdateTime).Every(1000);
		}

		public async void UpdateWanted()
		{
			wantedSign.text = $"$ {Monolith.bounty:N0}";
			wantedSign.AddToClassList("pop");
			await Awaitable.NextFrameAsync();
			wantedSign.RemoveFromClassList("pop");
		}
		public async void UpdateFortune()
		{
			fortuneSign.text = $"$ {Monolith.fortune:N0}";
			fortuneSign.AddToClassList("pop");
			await Awaitable.NextFrameAsync();
			fortuneSign.RemoveFromClassList("pop");
		}
		public void UpdateTime() => timeSign.text = $"{(int)(Monolith.time / 60)}:{(int)(Monolith.time % 60):00}<br>0:{(int)(Enemies.difficulty.wavesDelay - Enemies.timer):00}";
	}
}