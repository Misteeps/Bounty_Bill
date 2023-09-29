using System;

using UnityEngine;


namespace Simplex
{
	public static class PerformanceUtilities
	{
		public static double Benchmark(int count, Action action) => Benchmark($"Benchmark", count, action);
		public static double Benchmark(FormattableString header, int count, Action action)
		{
#if !UNITY_EDITOR
			ConsoleUtilities.Warn($"Simplex Performance Utilities disabled when not in Unity Editor");
			return 0;
#else
			var stopwatch = System.Diagnostics.Stopwatch.StartNew();

			for (int i = 0; i < count; i++)
				action.Invoke();

			stopwatch.Stop();
			ConsoleUtilities.Log($"{header.Format()} - {count} Times - {stopwatch.Elapsed.TotalMilliseconds:info} milliseconds");

			return stopwatch.Elapsed.TotalMilliseconds;
#endif
		}

		public static void TrackGC(object obj) => TrackGC($"{obj:ref}", obj);
		public static void TrackGC(FormattableString name, object obj)
		{
#if !UNITY_EDITOR
			ConsoleUtilities.Warn($"Simplex Performance Utilities disabled when not in Unity Editor");
#else
			if (obj == null) { ConsoleUtilities.Warn($"Cannot track garbage collection of {obj:ref}"); return; }

			Tick(new WeakReference(obj), name.Format());

			static async void Tick(WeakReference instance, string name)
			{
				if (!instance.IsAlive) ConsoleUtilities.Warn($"GC tracker for {name} is already dead");

				ConsoleUtilities.Log($"<color=#00FF00>Live:</color> {name}");

				for (int i = 0; i < 3600; i++)
				{
					if (!instance.IsAlive)
					{
						ConsoleUtilities.Log($"<color=#FF0000>Dead:</color> {name}");
						return;
					}

					await Awaitable.WaitForSecondsAsync(1);
				}

				ConsoleUtilities.Warn($"GC tracker reached limit: {name}");
			}
#endif
		}
	}
}