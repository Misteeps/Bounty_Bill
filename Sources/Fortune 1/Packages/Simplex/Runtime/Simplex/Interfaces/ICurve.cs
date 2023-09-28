using System;

using UnityEngine;


namespace Simplex
{
	public interface ICurve
	{
		public float Evaluate(float x);
		public float Inverse(float y);

		public static ICurve GetEaseCurve(EaseFunction function, EaseDirection direction)
		{
			switch (function)
			{
				case EaseFunction.Linear: return new Linear();

				case EaseFunction.Quadratic:
					switch (direction)
					{
						case EaseDirection.In: return new QuadraticIn();
						case EaseDirection.Out: return new QuadraticOut();
						case EaseDirection.InOut: return new QuadraticInOut();
						default: throw new ArgumentException("Invalid direction").Overwrite($"Failed getting ease curve {function:info} {direction:info}");
					}

				case EaseFunction.Cubic:
					switch (direction)
					{
						case EaseDirection.In: return new CubicIn();
						case EaseDirection.Out: return new CubicOut();
						case EaseDirection.InOut: return new CubicInOut();
						default: throw new ArgumentException("Invalid direction").Overwrite($"Failed getting ease curve {function:info} {direction:info}");
					}

				case EaseFunction.Quartic:
					switch (direction)
					{
						case EaseDirection.In: return new QuarticIn();
						case EaseDirection.Out: return new QuarticOut();
						case EaseDirection.InOut: return new QuarticInOut();
						default: throw new ArgumentException("Invalid direction").Overwrite($"Failed getting ease curve {function:info} {direction:info}");
					}

				case EaseFunction.Quintic:
					switch (direction)
					{
						case EaseDirection.In: return new QuinticIn();
						case EaseDirection.Out: return new QuinticOut();
						case EaseDirection.InOut: return new QuinticInOut();
						default: throw new ArgumentException("Invalid direction").Overwrite($"Failed getting ease curve {function:info} {direction:info}");
					}

				case EaseFunction.Sine:
					switch (direction)
					{
						case EaseDirection.In: return new SineIn();
						case EaseDirection.Out: return new SineOut();
						case EaseDirection.InOut: return new SineInOut();
						default: throw new ArgumentException("Invalid direction").Overwrite($"Failed getting ease curve {function:info} {direction:info}");
					}

				case EaseFunction.Circular:
					switch (direction)
					{
						case EaseDirection.In: return new CircularIn();
						case EaseDirection.Out: return new CircularOut();
						case EaseDirection.InOut: return new CircularInOut();
						default: throw new ArgumentException("Invalid direction").Overwrite($"Failed getting ease curve {function:info} {direction:info}");
					}

				case EaseFunction.Exponential:
					switch (direction)
					{
						case EaseDirection.In: return new ExponentialIn();
						case EaseDirection.Out: return new ExponentialOut();
						case EaseDirection.InOut: return new ExponentialInOut();
						default: throw new ArgumentException("Invalid direction").Overwrite($"Failed getting ease curve {function:info} {direction:info}");
					}

				case EaseFunction.Elastic:
					switch (direction)
					{
						case EaseDirection.In: return new ElasticIn();
						case EaseDirection.Out: return new ElasticOut();
						case EaseDirection.InOut: return new ElasticInOut();
						default: throw new ArgumentException("Invalid direction").Overwrite($"Failed getting ease curve {function:info} {direction:info}");
					}

				case EaseFunction.Back:
					switch (direction)
					{
						case EaseDirection.In: return new BackIn();
						case EaseDirection.Out: return new BackOut();
						case EaseDirection.InOut: return new BackInOut();
						default: throw new ArgumentException("Invalid direction").Overwrite($"Failed getting ease curve {function:info} {direction:info}");
					}

				case EaseFunction.Bounce:
					switch (direction)
					{
						case EaseDirection.In: return new BounceIn();
						case EaseDirection.Out: return new BounceOut();
						case EaseDirection.InOut: return new BounceInOut();
						default: throw new ArgumentException("Invalid direction").Overwrite($"Failed getting ease curve {function:info} {direction:info}");
					}

				default: throw new ArgumentException("Invalid function").Overwrite($"Failed getting ease curve {function:info} {direction:info}");
			}
		}
	}


	#region Delegate Curve
	public struct DelegateCurve : ICurve
	{
		public Func<float, float> evaluate;
		public Func<float, float> inverse;


		public DelegateCurve(Func<float, float> evaluate) : this(evaluate, null) { }
		public DelegateCurve(Func<float, float> evaluate, Func<float, float> inverse)
		{
			this.evaluate = evaluate;
			this.inverse = inverse;
		}

		public float Evaluate(float x) => (evaluate == null) ? x : evaluate.Invoke(x);
		public float Inverse(float y) => (inverse == null) ? y : inverse.Invoke(y);
	}
	#endregion Delegate Curve


	public enum EaseFunction { Linear, Quadratic, Cubic, Quartic, Quintic, Sine, Circular, Exponential, Elastic, Back, Bounce }
	public enum EaseDirection { In, Out, InOut }

	#region Linear
	public readonly struct Linear : ICurve
	{
		public float Evaluate(float x) => x;
		public float Inverse(float y) => y;
	}
	#endregion Linear

	#region Quadratic
	public readonly struct QuadraticIn : ICurve
	{
		public float Evaluate(float x) => x * x;
		public float Inverse(float y) => (float)Math.Sqrt(y);
	}
	public readonly struct QuadraticOut : ICurve
	{
		public float Evaluate(float x) => x * (2 - x);
		public float Inverse(float y) => 1 - (float)Math.Sqrt(-y + 1);
	}
	public readonly struct QuadraticInOut : ICurve
	{
		public float Evaluate(float x) => (x < 0.5f) ? 2 * x * x : (-2 * x * x) + (4 * x) - 1;
		public float Inverse(float y) => (y < 0.5f) ? (float)Math.Sqrt(y / 2) : (-4 + (float)Math.Sqrt(8 * (-y + 1))) / -4;
	}
	#endregion Quadratic

	#region Cubic
	public readonly struct CubicIn : ICurve
	{
		public float Evaluate(float x) => x * x * x;
		public float Inverse(float y) => (float)Math.Pow(y, 1f / 3f);
	}
	public readonly struct CubicOut : ICurve
	{
		public float Evaluate(float x)
		{
			x = x - 1;
			return x * x * x + 1;
		}
		public float Inverse(float y)
		{
			y = y - 1;
			return -(float)Math.Pow(-y, 1f / 3f) + 1;
		}
	}
	public readonly struct CubicInOut : ICurve
	{
		public float Evaluate(float x)
		{
			if (x < 0.5f)
				return 4 * x * x * x;

			x = 2 * x - 2;
			return 0.5f * x * x * x + 1;
		}
		public float Inverse(float y)
		{
			if (y < 0.5f)
				return (float)Math.Pow(y / 4f, 1f / 3f);

			y = y - 1;
			return 0.62996f * -(float)Math.Pow(-y, 1f / 3f) + 1;
		}
	}
	#endregion Cubic

	#region Quartic
	public readonly struct QuarticIn : ICurve
	{
		public float Evaluate(float x) => x * x * x * x;
		public float Inverse(float y) => (float)Math.Pow(y, 1f / 4f);
	}
	public readonly struct QuarticOut : ICurve
	{
		public float Evaluate(float x)
		{
			x = x - 1;
			return 1 - (x * x * x * x);
		}
		public float Inverse(float y)
		{
			y = -y + 1;
			return -(float)Math.Pow(y, 1f / 4f) + 1;
		}
	}
	public readonly struct QuarticInOut : ICurve
	{
		public float Evaluate(float x)
		{
			if (x < 0.5f)
				return 8 * x * x * x * x;

			x = x - 1;
			return -8 * x * x * x * x + 1;
		}
		public float Inverse(float y)
		{
			if (y < 0.5f)
				return (float)Math.Pow(y / 8f, 1f / 4f);

			y = -(y - 1) / 8f;
			return -(float)Math.Pow(y, 1f / 4f) + 1;
		}
	}
	#endregion Quartic

	#region Quintic
	public readonly struct QuinticIn : ICurve
	{
		public float Evaluate(float x) => x * x * x * x * x;
		public float Inverse(float y) => (float)Math.Pow(y, 1f / 5f);
	}
	public readonly struct QuinticOut : ICurve
	{
		public float Evaluate(float x)
		{
			x = x - 1;
			return x * x * x * x * x + 1;
		}
		public float Inverse(float y)
		{
			y = y - 1;
			return -(float)Math.Pow(-y, 1f / 5f) + 1;
		}
	}
	public readonly struct QuinticInOut : ICurve
	{
		public float Evaluate(float x)
		{
			if (x < 0.5f)
				return 16 * x * x * x * x * x;

			x = 2 * x - 2;
			return 0.5f * x * x * x * x * x + 1;
		}
		public float Inverse(float y)
		{
			if (y < 0.5f)
				return (float)Math.Pow(y / 16f, 1f / 5f);

			y = y - 1;
			return 0.57435f * -(float)Math.Pow(-y, 1f / 5f) + 1;
		}
	}
	#endregion Quintic

	#region Sine
	public readonly struct SineIn : ICurve
	{
		public float Evaluate(float x) => (float)Math.Sin((x - 1) * ((float)Math.PI / 2)) + 1;
		public float Inverse(float y) => (2 / (float)Math.PI) * (float)Math.Asin(y - 1) + 1;
	}
	public readonly struct SineOut : ICurve
	{
		public float Evaluate(float x) => (float)Math.Sin(x * ((float)Math.PI / 2));
		public float Inverse(float y) => (2 / (float)Math.PI) * (float)Math.Asin(y);
	}
	public readonly struct SineInOut : ICurve
	{
		public float Evaluate(float x) => 0.5f * (1 - (float)Math.Cos(x * (float)Math.PI));
		public float Inverse(float y) => (1 / (float)Math.PI) * (float)Math.Acos(1 - (2 * y));
	}
	#endregion Sine

	#region Circular
	public readonly struct CircularIn : ICurve
	{
		public float Evaluate(float x) => 1 - (float)Math.Sqrt(1 - (x * x));
		public float Inverse(float y) => (float)Math.Sqrt(y * (2 - y));
	}
	public readonly struct CircularOut : ICurve
	{
		public float Evaluate(float x) => (float)Math.Sqrt((2 - x) * x);
		public float Inverse(float y) => 1 - (float)Math.Sqrt(1 - (y * y));
	}
	public readonly struct CircularInOut : ICurve
	{
		public float Evaluate(float x) => (x < 0.5f) ? 0.5f * (1 - (float)Math.Sqrt(1 - 4 * (x * x))) : 0.5f * ((float)Math.Sqrt(-((2 * x) - 3) * ((2 * x) - 1)) + 1);
		public float Inverse(float y) => (y < 0.5f) ? (float)Math.Sqrt(-y * (y - 1)) : 1 - (float)Math.Sqrt(y * (-y + 1));

	}
	#endregion Circular

	#region Exponential
	public readonly struct ExponentialIn : ICurve
	{
		public float Evaluate(float x) => (x == 0) ? x : (float)Math.Pow(2, 10 * (x - 1));
		public float Inverse(float y) => new QuinticIn().Inverse(y);
	}
	public readonly struct ExponentialOut : ICurve
	{
		public float Evaluate(float x) => (x == 1) ? x : 1 - (float)Math.Pow(2, -10 * x);
		public float Inverse(float y) => new QuinticOut().Inverse(y);
	}
	public readonly struct ExponentialInOut : ICurve
	{
		public float Evaluate(float x)
		{
			if (x == 0 || x == 1)
				return x;
			else if (x < 0.5f)
				return 0.5f * (float)Math.Pow(2, (20 * x) - 10);
			else
				return -0.5f * (float)Math.Pow(2, (-20 * x) + 10) + 1;
		}
		public float Inverse(float y) => new QuinticInOut().Inverse(y);
	}
	#endregion Exponential

	#region Elastic
	public readonly struct ElasticIn : ICurve
	{
		public float Evaluate(float x) => (float)Math.Sin(13 * ((float)Math.PI / 2) * x) * (float)Math.Pow(2, 10 * (x - 1));
		public float Inverse(float y) => new QuinticIn().Inverse(y);
	}
	public readonly struct ElasticOut : ICurve
	{
		public float Evaluate(float x) => (float)Math.Sin(-13 * ((float)Math.PI / 2) * (x + 1)) * (float)Math.Pow(2, -10 * x) + 1;
		public float Inverse(float y) => new QuinticOut().Inverse(y);
	}
	public readonly struct ElasticInOut : ICurve
	{
		public float Evaluate(float x) => (x < 0.5f) ? 0.5f * (float)Math.Sin(13 * ((float)Math.PI / 2) * (2 * x)) * (float)Math.Pow(2, 10 * ((2 * x) - 1)) : 0.5f * ((float)Math.Sin(-13 * ((float)Math.PI / 2) * ((2 * x - 1) + 1)) * (float)Math.Pow(2, -10 * (2 * x - 1)) + 2);
		public float Inverse(float y) => new QuinticInOut().Inverse(y);
	}
	#endregion Elastic

	#region Back
	public readonly struct BackIn : ICurve
	{
		public float Evaluate(float x) => x * x * x - x * (float)Math.Sin(x * (float)Math.PI);
		public float Inverse(float y) => new QuinticIn().Inverse(y);
	}
	public readonly struct BackOut : ICurve
	{
		public float Evaluate(float x)
		{
			x = -x + 1;
			return 1 - (x * x * x - x * (float)Math.Sin(x * (float)Math.PI));
		}
		public float Inverse(float y) => new QuinticOut().Inverse(y);
	}
	public readonly struct BackInOut : ICurve
	{
		public float Evaluate(float x)
		{
			x = x * 2;
			if (x < 1)
				return 0.5f * (x * x * x - x * (float)Math.Sin(x * (float)Math.PI));

			x = -(x - 2);
			return 0.5f * (1 - (x * x * x - x * (float)Math.Sin(x * (float)Math.PI))) + 0.5f;
		}
		public float Inverse(float y) => new QuinticInOut().Inverse(y);
	}
	#endregion Back

	#region Bounce
	public readonly struct BounceIn : ICurve
	{
		public float Evaluate(float x) => 1 - new BounceOut().Evaluate(1 - x);
		public float Inverse(float y) => new QuinticIn().Inverse(y);
	}
	public readonly struct BounceOut : ICurve
	{
		public float Evaluate(float x)
		{
			if (x < 0.3636f)
				return (121 * x * x) / 16.0f;
			else if (x < 0.7272f)
				return (9.075f * x * x) - (9.9f * x) + 3.4f;
			else if (x < 0.9f)
				return (12.0665f * x * x) - (19.6355f * x) + 8.8981f;
			else
				return (10.8f * x * x) - (20.52f * x) + 10.72f;
		}
		public float Inverse(float y) => new QuinticOut().Inverse(y);
	}
	public readonly struct BounceInOut : ICurve
	{
		public float Evaluate(float x) => (x < 0.5f) ? 0.5f * new BounceIn().Evaluate(x * 2) : 0.5f * new BounceOut().Evaluate(x * 2 - 1) + 0.5f;
		public float Inverse(float y) => new QuinticInOut().Inverse(y);
	}
	#endregion Bounce
}