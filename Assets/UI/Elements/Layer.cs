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

		protected virtual int ShowMilliseconds => 320;
		public event Action Shown;
		protected virtual int HideMilliseconds => 240;
		public event Action Hidden;


		public Layer()
		{
			this.Name(GetType().Name, toLower: false).Display(false);

			style.opacity = 0;

			Hidden += () => this.Display(false);
		}

		public void Show() => Show(ShowMilliseconds);
		public void Hide() => Hide(HideMilliseconds);
		public virtual void Show(int milliseconds)
		{
			this.Enable(true, pickingMode).Display(true).Refresh().Focus();
			this.TransitionOpacity().Modify(0, 1, milliseconds, EaseFunction.Circular, EaseDirection.Out, realTime: true).Run();

			if (focusable)
				Root.Layer = this;

			Shown?.Invoke();
		}
		public virtual void Hide(int milliseconds)
		{
			this.Enable(false);
			this.TransitionOpacity().Modify(1, 0, milliseconds, EaseFunction.Circular, EaseDirection.Out, realTime: true, onEnd: Hidden).Run();

			if (Root.Layer == this)
				Root.Layer = null;
		}
	}
}