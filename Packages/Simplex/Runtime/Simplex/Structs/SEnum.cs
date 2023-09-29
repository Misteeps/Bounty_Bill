using System;

using UnityEngine;


namespace Simplex
{
	[Serializable]
	public struct SEnum<T> : IEquatable<SEnum<T>>, IEquatable<T> where T : struct, Enum
	{
		[SerializeField] private string value;

		public T Value
		{
			get
			{
				try { return Enum.Parse<T>(value); }
				catch (Exception exception) { exception.Error($"Failed parsing {value:info} to Enum {typeof(T):type}"); return default; }
			}
			set => this.value = value.ToString();
		}


		public override string ToString() => Value.ToString();

		public override bool Equals(object obj) => (obj is SEnum<T> sEnum && Equals(sEnum)) || (obj is T tEnum && Equals(tEnum));
		public bool Equals(SEnum<T> sEnum) => Value.Equals(sEnum.Value);
		public bool Equals(T tEnum) => Value.Equals(tEnum);

		public override int GetHashCode() => Value.GetHashCode();

		public static explicit operator Enum(SEnum<T> sEnum) => sEnum.Value;
		public static implicit operator T(SEnum<T> sEnum) => sEnum.Value;
		public static implicit operator SEnum<T>(T tEnum) => new SEnum<T>() { Value = tEnum };
	}
}