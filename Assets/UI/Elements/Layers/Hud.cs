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


		public Hud()
		{

		}
	}
}