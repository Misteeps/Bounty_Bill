using System;

using UnityEngine;


namespace Simplex
{
	[Serializable]
	public struct SGuid : IEquatable<SGuid>, IEquatable<Guid>, IFormattable
	{
		[SerializeField] private string value;

		public Guid Value
		{
			get => new Guid(value);
			set => this.value = value.ToString("D");
		}


		public override string ToString() => Value.ToString();
		public string ToString(string format) => Value.ToString(format);
		public string ToString(string format, IFormatProvider provider) => Value.ToString(format, provider);

		public override bool Equals(object obj) => (obj is SGuid sGuid && Equals(sGuid)) || (obj is Guid guid && Equals(guid));
		public bool Equals(SGuid guid) => Value == guid.Value;
		public bool Equals(Guid guid) => Value == guid;

		public override int GetHashCode() => Value.GetHashCode();

		public static implicit operator Guid(SGuid sGuid) => sGuid.Value;
		public static implicit operator SGuid(Guid guid) => new SGuid() { Value = guid };
	}
}