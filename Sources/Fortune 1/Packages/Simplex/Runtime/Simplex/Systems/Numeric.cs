using System;

using UnityEngine;


namespace Simplex
{
	public interface Numeric<T>
	{
		public T Min { get; }
		public T Max { get; }

		public T Clamp(T min, T max, T value);
		public T Round(T value, int decimals);
		public T Round(T value, float increments);

		public T Lerp(T min, T max, float factor);
		public float LerpInverse(T min, T max, T value);

		public T Parse(string str);
		public bool Valid(string str, out T value);
		public bool Valid(char c);

		public string String(T value) => value.ToString();
	}


	#region Byte Numeric
	public readonly struct ByteNumeric : Numeric<byte>
	{
		public byte Min => byte.MinValue;
		public byte Max => byte.MaxValue;

		public byte Clamp(byte min, byte max, byte value) => (value <= min) ? min : (value >= max) ? max : value;
		public byte Round(byte value, int decimals) => value;
		public byte Round(byte value, float increments) => (byte)(Math.Round(value / increments, MidpointRounding.AwayFromZero) * increments);

		public byte Lerp(byte min, byte max, float factor) => (byte)Math.Round((1 - Mathf.Clamp01(factor)) * min + Mathf.Clamp01(factor) * max, MidpointRounding.AwayFromZero);
		public float LerpInverse(byte min, byte max, byte value) => Mathf.InverseLerp(min, max, value);

		public byte Parse(string str) => byte.TryParse(str, out byte value) ? value : default;
		public bool Valid(string str, out byte value) => byte.TryParse(str, out value);
		public bool Valid(char c) => (c >= '0' && c <= '9');
	}
	#endregion Byte Numeric
	#region SByte Numeric
	public readonly struct SByteNumeric : Numeric<sbyte>
	{
		public sbyte Min => sbyte.MinValue;
		public sbyte Max => sbyte.MaxValue;

		public sbyte Clamp(sbyte min, sbyte max, sbyte value) => (value <= min) ? min : (value >= max) ? max : value;
		public sbyte Round(sbyte value, int decimals) => value;
		public sbyte Round(sbyte value, float increments) => (sbyte)(Math.Round(value / increments, MidpointRounding.AwayFromZero) * increments);

		public sbyte Lerp(sbyte min, sbyte max, float factor) => (sbyte)Math.Round((1 - Mathf.Clamp01(factor)) * min + Mathf.Clamp01(factor) * max, MidpointRounding.AwayFromZero);
		public float LerpInverse(sbyte min, sbyte max, sbyte value) => Mathf.InverseLerp(min, max, value);

		public sbyte Parse(string str) => sbyte.TryParse(str, out sbyte value) ? value : default;
		public bool Valid(string str, out sbyte value) => sbyte.TryParse(str, out value);
		public bool Valid(char c) => (c >= '0' && c <= '9') || c == '-';
	}
	#endregion SByte Numeric

	#region Short Numeric
	public readonly struct ShortNumeric : Numeric<short>
	{
		public short Min => short.MinValue;
		public short Max => short.MaxValue;

		public short Clamp(short min, short max, short value) => (value <= min) ? min : (value >= max) ? max : value;
		public short Round(short value, int decimals) => value;
		public short Round(short value, float increments) => (short)(Math.Round(value / increments, MidpointRounding.AwayFromZero) * increments);

		public short Lerp(short min, short max, float factor) => (short)Math.Round((1 - Mathf.Clamp01(factor)) * min + Mathf.Clamp01(factor) * max, MidpointRounding.AwayFromZero);
		public float LerpInverse(short min, short max, short value) => Mathf.InverseLerp(min, max, value);

		public short Parse(string str) => short.TryParse(str, out short value) ? value : default;
		public bool Valid(string str, out short value) => short.TryParse(str, out value);
		public bool Valid(char c) => (c >= '0' && c <= '9') || c == '-';
	}
	#endregion Short Numeric
	#region UShort Numeric
	public readonly struct UShortNumeric : Numeric<ushort>
	{
		public ushort Min => ushort.MinValue;
		public ushort Max => ushort.MaxValue;

		public ushort Clamp(ushort min, ushort max, ushort value) => (value <= min) ? min : (value >= max) ? max : value;
		public ushort Round(ushort value, int decimals) => value;
		public ushort Round(ushort value, float increments) => (ushort)(Math.Round(value / increments, MidpointRounding.AwayFromZero) * increments);

		public ushort Lerp(ushort min, ushort max, float factor) => (ushort)Math.Round((1 - Mathf.Clamp01(factor)) * min + Mathf.Clamp01(factor) * max, MidpointRounding.AwayFromZero);
		public float LerpInverse(ushort min, ushort max, ushort value) => Mathf.InverseLerp(min, max, value);

		public ushort Parse(string str) => ushort.TryParse(str, out ushort value) ? value : default;
		public bool Valid(string str, out ushort value) => ushort.TryParse(str, out value);
		public bool Valid(char c) => (c >= '0' && c <= '9');
	}
	#endregion UShort Numeric

	#region Int Numeric
	public readonly struct IntNumeric : Numeric<int>
	{
		public int Min => int.MinValue;
		public int Max => int.MaxValue;

		public int Clamp(int min, int max, int value) => (value <= min) ? min : (value >= max) ? max : value;
		public int Round(int value, int decimals) => value;
		public int Round(int value, float increments) => (int)(Math.Round(value / increments, MidpointRounding.AwayFromZero) * increments);

		public int Lerp(int min, int max, float factor) => (int)Math.Round((1 - Mathf.Clamp01(factor)) * min + Mathf.Clamp01(factor) * max, MidpointRounding.AwayFromZero);
		public float LerpInverse(int min, int max, int value) => Mathf.InverseLerp(min, max, value);

		public int Parse(string str) => int.TryParse(str, out int value) ? value : default;
		public bool Valid(string str, out int value) => int.TryParse(str, out value);
		public bool Valid(char c) => (c >= '0' && c <= '9') || c == '-';
	}
	#endregion Int Numeric
	#region UInt Numeric
	public readonly struct UIntNumeric : Numeric<uint>
	{
		public uint Min => uint.MinValue;
		public uint Max => uint.MaxValue;

		public uint Clamp(uint min, uint max, uint value) => (value <= min) ? min : (value >= max) ? max : value;
		public uint Round(uint value, int decimals) => value;
		public uint Round(uint value, float increments) => (uint)(Math.Round(value / increments, MidpointRounding.AwayFromZero) * increments);

		public uint Lerp(uint min, uint max, float factor) => (uint)Math.Round((1 - Mathf.Clamp01(factor)) * min + Mathf.Clamp01(factor) * max, MidpointRounding.AwayFromZero);
		public float LerpInverse(uint min, uint max, uint value) => Mathf.InverseLerp(min, max, value);

		public uint Parse(string str) => uint.TryParse(str, out uint value) ? value : default;
		public bool Valid(string str, out uint value) => uint.TryParse(str, out value);
		public bool Valid(char c) => (c >= '0' && c <= '9');
	}
	#endregion UInt Numeric

	#region Long Numeric
	public readonly struct LongNumeric : Numeric<long>
	{
		public long Min => long.MinValue;
		public long Max => long.MaxValue;

		public long Clamp(long min, long max, long value) => (value <= min) ? min : (value >= max) ? max : value;
		public long Round(long value, int decimals) => value;
		public long Round(long value, float increments) => (long)(Math.Round(value / increments, MidpointRounding.AwayFromZero) * increments);

		public long Lerp(long min, long max, float factor) => (long)Math.Round((1 - Mathf.Clamp01(factor)) * min + Mathf.Clamp01(factor) * max, MidpointRounding.AwayFromZero);
		public float LerpInverse(long min, long max, long value) => Mathf.InverseLerp(min, max, value);

		public long Parse(string str) => long.TryParse(str, out long value) ? value : default;
		public bool Valid(string str, out long value) => long.TryParse(str, out value);
		public bool Valid(char c) => (c >= '0' && c <= '9') || c == '-';
	}
	#endregion Long Numeric
	#region ULong Numeric
	public readonly struct ULongNumeric : Numeric<ulong>
	{
		public ulong Min => ulong.MinValue;
		public ulong Max => ulong.MaxValue;

		public ulong Clamp(ulong min, ulong max, ulong value) => (value <= min) ? min : (value >= max) ? max : value;
		public ulong Round(ulong value, int decimals) => value;
		public ulong Round(ulong value, float increments) => (ulong)(Math.Round(value / increments, MidpointRounding.AwayFromZero) * increments);

		public ulong Lerp(ulong min, ulong max, float factor) => (ulong)Math.Round((1 - Mathf.Clamp01(factor)) * min + Mathf.Clamp01(factor) * max, MidpointRounding.AwayFromZero);
		public float LerpInverse(ulong min, ulong max, ulong value) => Mathf.InverseLerp(min, max, value);

		public ulong Parse(string str) => ulong.TryParse(str, out ulong value) ? value : default;
		public bool Valid(string str, out ulong value) => ulong.TryParse(str, out value);
		public bool Valid(char c) => (c >= '0' && c <= '9');
	}
	#endregion ULong Numeric

	#region Float Numeric
	public readonly struct FloatNumeric : Numeric<float>
	{
		public float Min => float.MinValue;
		public float Max => float.MaxValue;

		public float Clamp(float min, float max, float value) => (value <= min) ? min : (value >= max) ? max : value;
		public float Round(float value, int decimals) => (float)Math.Round(value, decimals, MidpointRounding.AwayFromZero);
		public float Round(float value, float increments) => (float)(Math.Round(value / increments, MidpointRounding.AwayFromZero) * increments);

		public float Lerp(float min, float max, float factor) => (1 - Mathf.Clamp01(factor)) * min + Mathf.Clamp01(factor) * max;
		public float LerpInverse(float min, float max, float value) => Mathf.InverseLerp(min, max, value);

		public float Parse(string str) => float.TryParse(str, out float value) ? value : default;
		public bool Valid(string str, out float value) => float.TryParse(str, out value);
		public bool Valid(char c) => (c >= '0' && c <= '9') || c == '-' || c == '.';
	}
	#endregion Float Numeric
	#region Double Numeric
	public readonly struct DoubleNumeric : Numeric<double>
	{
		public double Min => double.MinValue;
		public double Max => double.MaxValue;

		public double Clamp(double min, double max, double value) => (value <= min) ? min : (value >= max) ? max : value;
		public double Round(double value, int decimals) => Math.Round(value, decimals, MidpointRounding.AwayFromZero);
		public double Round(double value, float increments) => Math.Round(value / increments, MidpointRounding.AwayFromZero) * increments;

		public double Lerp(double min, double max, float factor) => (1 - Mathf.Clamp01(factor)) * min + Mathf.Clamp01(factor) * max;
		public float LerpInverse(double min, double max, double value) => (min == max) ? 0 : Mathf.Clamp01((float)(((value / 4) - (min / 4)) / ((max / 4) - (min / 4))));

		public double Parse(string str) => double.TryParse(str, out double value) ? value : default;
		public bool Valid(string str, out double value) => double.TryParse(str, out value);
		public bool Valid(char c) => (c >= '0' && c <= '9') || c == '-' || c == '.';
	}
	#endregion Double Numeric
	#region Decimal Numeric
	public readonly struct DecimalNumeric : Numeric<decimal>
	{
		public decimal Min => decimal.MinValue;
		public decimal Max => decimal.MaxValue;

		public decimal Clamp(decimal min, decimal max, decimal value) => (value <= min) ? min : (value >= max) ? max : value;
		public decimal Round(decimal value, int decimals) => Math.Round(value, decimals, MidpointRounding.AwayFromZero);
		public decimal Round(decimal value, float increments) => Math.Round(value / (decimal)increments, MidpointRounding.AwayFromZero) * (decimal)increments;

		public decimal Lerp(decimal min, decimal max, float factor) => (1 - (decimal)Mathf.Clamp01(factor)) * min + (decimal)Mathf.Clamp01(factor) * max;
		public float LerpInverse(decimal min, decimal max, decimal value) => (min == max) ? 0 : Mathf.Clamp01((float)(((value / 4) - (min / 4)) / ((max / 4) - (min / 4))));

		public decimal Parse(string str) => decimal.TryParse(str, out decimal value) ? value : default;
		public bool Valid(string str, out decimal value) => decimal.TryParse(str, out value);
		public bool Valid(char c) => (c >= '0' && c <= '9') || c == '-' || c == '.';
	}
	#endregion Decimal Numeric
}