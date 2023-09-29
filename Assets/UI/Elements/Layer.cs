using System;

using UnityEngine;

using Simplex;
using Simplex.UI;


namespace Game.UI
{
	public abstract class Layer<T> : Layer where T : Layer, new()
	{
		public static T Instance { get; } = Root.Instance.Attach(new T());


		public static new void Show() => Instance.Show();
		public static new void Hide() => Instance.Hide();
	}


	public abstract class Layer : Div
	{
		protected override string[] DefaultClasses => new string[] { "layer" };
		protected override bool DefaultFocusable => true;
		protected override bool DefaultPickable => true;

		protected virtual float ShowDuration => 0.32f;
		public event Action Shown;
		protected virtual float HideDuration => 0.24f;
		public event Action Hidden;


		public Layer()
		{
			this.Name(GetType().Name, toLower: false).Display(false);

			style.opacity = 0;

			Hidden += () => this.Display(false);
		}

		public void Show() => Show(ShowDuration);
		public void Hide() => Hide(HideDuration);
		public virtual void Show(float duration)
		{
			this.Enable(true, pickingMode).Display(true).Refresh().Focus();
			this.TransitionOpacity().Modify(0, 1, duration, EaseFunction.Circular, EaseDirection.Out, realTime: true).Run();

			Shown?.Invoke();
		}
		public virtual void Hide(float duration)
		{
			this.Enable(false);
			this.TransitionOpacity().Modify(1, 0, duration, EaseFunction.Circular, EaseDirection.Out, realTime: true, onEnd: Hidden).Run();
		}
	}
}