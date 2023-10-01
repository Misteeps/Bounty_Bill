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
		}
	}
}