using System;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using System.Security.Cryptography;

using UnityEngine;


namespace Simplex
{
	public class RNG
	{
		public static RNG Generic = new RNG();
		public static RNG Seeded = new RNG();

		public readonly System.Random random;
		public readonly int seed;


		public RNG() : this(UnityEngine.Random.Range(0, int.MaxValue)) { }
		public RNG(string seed)
		{
			if (seed.Empty())
			{
				ConsoleUtilities.Warn($"RNG should not be instantiated with null or empty seed");
				this.seed = UnityEngine.Random.Range(0, int.MaxValue);
			}
			else
			{
				using SHA256 sha256 = SHA256.Create();
				byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(seed));
				this.seed = BitConverter.ToInt32(hash, 0);
			}

			this.random = new System.Random(this.seed);
		}
		public RNG(int seed)
		{
			this.seed = seed;
			this.random = new System.Random(this.seed);
		}

		public bool Bool() => random.Next(2) == 0;
		public int Int(int min, int max) => random.Next(min, max);
		public float Float(float min, float max) => (float)random.NextDouble() * (max - min) + min;
		public double Double(double min, double max) => random.NextDouble() * (max - min) + min;
		public Vector2 Vector2(Vector2 min, Vector2 max) => new Vector2(Float(min.x, max.x), Float(min.y, max.y));
		public Vector3 Vector3(Vector3 min, Vector3 max) => new Vector3(Float(min.x, max.x), Float(min.y, max.y), Float(min.z, max.z));
		public Vector4 Vector4(Vector4 min, Vector4 max) => new Vector4(Float(min.x, max.x), Float(min.y, max.y), Float(min.z, max.z), Float(min.w, max.w));
		public Vector2Int Vector2Int(Vector2Int min, Vector2Int max) => new Vector2Int(Int(min.x, max.x), Int(min.y, max.y));
		public Vector3Int Vector3Int(Vector3Int min, Vector3Int max) => new Vector3Int(Int(min.x, max.x), Int(min.y, max.y), Int(min.z, max.z));
		public Color Color(Color min, Color max) => new Color(Float(min.r, max.r), Float(min.g, max.g), Float(min.b, max.b), Float(min.a, max.a));
		public Color Color(float hueMin = 0, float hueMax = 1, float saturationMin = 0, float saturationMax = 1, float brightnessMin = 0, float brightnessMax = 1, float alphaMin = 1, float alphaMax = 1)
		{
			float hue = (hueMin == hueMax) ? hueMin : Float(hueMin, hueMax);
			float saturation = (saturationMin == saturationMax) ? saturationMin : Float(saturationMin, saturationMax);
			float brightness = (brightnessMin == brightnessMax) ? brightnessMin : Float(brightnessMin, brightnessMax);
			float alpha = (alphaMin == alphaMax) ? alphaMin : Float(alphaMin, alphaMax);

			Color color = UnityEngine.Color.HSVToRGB(hue, saturation, brightness);
			color.a = alpha;
			return color;
		}

		public T From<T>(T[] array) => (array.Empty()) ? throw new IndexOutOfRangeException("Cannot get random item from empty array") : array[random.Next(array.Length)];
		public T From<T>(List<T> list) => (list.Empty()) ? throw new IndexOutOfRangeException("Cannot get random item from empty list") : list[random.Next(list.Count)];

		public void Save(out int seed, out int inext, out int inextp, out int[] seedArray)
		{
			FieldInfo inextField = typeof(System.Random).GetField("_inext", BindingFlags.Instance | BindingFlags.NonPublic);
			FieldInfo inextpField = typeof(System.Random).GetField("_inextp", BindingFlags.Instance | BindingFlags.NonPublic);
			FieldInfo seedArrayField = typeof(System.Random).GetField("_seedArray", BindingFlags.Instance | BindingFlags.NonPublic);

			seed = this.seed;
			inext = (int)inextField.GetValue(random);
			inextp = (int)inextpField.GetValue(random);
			seedArray = (int[])((int[])seedArrayField.GetValue(random)).Clone();
		}
		public void Load(int seed, int inext, int inextp, int[] seedArray)
		{
			FieldInfo seedField = typeof(RNG).GetField("seed", BindingFlags.Instance | BindingFlags.Public);
			FieldInfo inextField = typeof(System.Random).GetField("_inext", BindingFlags.Instance | BindingFlags.NonPublic);
			FieldInfo inextpField = typeof(System.Random).GetField("_inextp", BindingFlags.Instance | BindingFlags.NonPublic);
			FieldInfo seedArrayField = typeof(System.Random).GetField("_seedArray", BindingFlags.Instance | BindingFlags.NonPublic);

			seedField.SetValue(this, seed);
			inextField.SetValue(random, inext);
			inextpField.SetValue(random, inextp);
			seedArrayField.SetValue(random, seedArray.Clone());
		}
	}
}