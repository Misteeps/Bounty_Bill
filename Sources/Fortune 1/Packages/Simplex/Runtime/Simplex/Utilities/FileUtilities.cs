using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Buffers.Binary;
using System.Collections.Generic;

using UnityEngine;


namespace Simplex
{
	public static class FileUtilities
	{
		#region CRC32
		public static class CRC32
		{
			private const uint seed = 0xFFFFFFFF;
			private static readonly uint[] table = new uint[]
			{
				0x00000000, 0x77073096, 0xEE0E612C, 0x990951BA, 0x076DC419, 0x706AF48F, 0xE963A535, 0x9E6495A3, 0x0EDB8832, 0x79DCB8A4, 0xE0D5E91E, 0x97D2D988, 0x09B64C2B, 0x7EB17CBD, 0xE7B82D07, 0x90BF1D91,
				0x1DB71064, 0x6AB020F2, 0xF3B97148, 0x84BE41DE, 0x1ADAD47D, 0x6DDDE4EB, 0xF4D4B551, 0x83D385C7, 0x136C9856, 0x646BA8C0, 0xFD62F97A, 0x8A65C9EC, 0x14015C4F, 0x63066CD9, 0xFA0F3D63, 0x8D080DF5,
				0x3B6E20C8, 0x4C69105E, 0xD56041E4, 0xA2677172, 0x3C03E4D1, 0x4B04D447, 0xD20D85FD, 0xA50AB56B, 0x35B5A8FA, 0x42B2986C, 0xDBBBC9D6, 0xACBCF940, 0x32D86CE3, 0x45DF5C75, 0xDCD60DCF, 0xABD13D59,
				0x26D930AC, 0x51DE003A, 0xC8D75180, 0xBFD06116, 0x21B4F4B5, 0x56B3C423, 0xCFBA9599, 0xB8BDA50F, 0x2802B89E, 0x5F058808, 0xC60CD9B2, 0xB10BE924, 0x2F6F7C87, 0x58684C11, 0xC1611DAB, 0xB6662D3D,
				0x76DC4190, 0x01DB7106, 0x98D220BC, 0xEFD5102A, 0x71B18589, 0x06B6B51F, 0x9FBFE4A5, 0xE8B8D433, 0x7807C9A2, 0x0F00F934, 0x9609A88E, 0xE10E9818, 0x7F6A0DBB, 0x086D3D2D, 0x91646C97, 0xE6635C01,
				0x6B6B51F4, 0x1C6C6162, 0x856530D8, 0xF262004E, 0x6C0695ED, 0x1B01A57B, 0x8208F4C1, 0xF50FC457, 0x65B0D9C6, 0x12B7E950, 0x8BBEB8EA, 0xFCB9887C, 0x62DD1DDF, 0x15DA2D49, 0x8CD37CF3, 0xFBD44C65,
				0x4DB26158, 0x3AB551CE, 0xA3BC0074, 0xD4BB30E2, 0x4ADFA541, 0x3DD895D7, 0xA4D1C46D, 0xD3D6F4FB, 0x4369E96A, 0x346ED9FC, 0xAD678846, 0xDA60B8D0, 0x44042D73, 0x33031DE5, 0xAA0A4C5F, 0xDD0D7CC9,
				0x5005713C, 0x270241AA, 0xBE0B1010, 0xC90C2086, 0x5768B525, 0x206F85B3, 0xB966D409, 0xCE61E49F, 0x5EDEF90E, 0x29D9C998, 0xB0D09822, 0xC7D7A8B4, 0x59B33D17, 0x2EB40D81, 0xB7BD5C3B, 0xC0BA6CAD,
				0xEDB88320, 0x9ABFB3B6, 0x03B6E20C, 0x74B1D29A, 0xEAD54739, 0x9DD277AF, 0x04DB2615, 0x73DC1683, 0xE3630B12, 0x94643B84, 0x0D6D6A3E, 0x7A6A5AA8, 0xE40ECF0B, 0x9309FF9D, 0x0A00AE27, 0x7D079EB1,
				0xF00F9344, 0x8708A3D2, 0x1E01F268, 0x6906C2FE, 0xF762575D, 0x806567CB, 0x196C3671, 0x6E6B06E7, 0xFED41B76, 0x89D32BE0, 0x10DA7A5A, 0x67DD4ACC, 0xF9B9DF6F, 0x8EBEEFF9, 0x17B7BE43, 0x60B08ED5,
				0xD6D6A3E8, 0xA1D1937E, 0x38D8C2C4, 0x4FDFF252, 0xD1BB67F1, 0xA6BC5767, 0x3FB506DD, 0x48B2364B, 0xD80D2BDA, 0xAF0A1B4C, 0x36034AF6, 0x41047A60, 0xDF60EFC3, 0xA867DF55, 0x316E8EEF, 0x4669BE79,
				0xCB61B38C, 0xBC66831A, 0x256FD2A0, 0x5268E236, 0xCC0C7795, 0xBB0B4703, 0x220216B9, 0x5505262F, 0xC5BA3BBE, 0xB2BD0B28, 0x2BB45A92, 0x5CB36A04, 0xC2D7FFA7, 0xB5D0CF31, 0x2CD99E8B, 0x5BDEAE1D,
				0x9B64C2B0, 0xEC63F226, 0x756AA39C, 0x026D930A, 0x9C0906A9, 0xEB0E363F, 0x72076785, 0x05005713, 0x95BF4A82, 0xE2B87A14, 0x7BB12BAE, 0x0CB61B38, 0x92D28E9B, 0xE5D5BE0D, 0x7CDCEFB7, 0x0BDBDF21,
				0x86D3D2D4, 0xF1D4E242, 0x68DDB3F8, 0x1FDA836E, 0x81BE16CD, 0xF6B9265B, 0x6FB077E1, 0x18B74777, 0x88085AE6, 0xFF0F6A70, 0x66063BCA, 0x11010B5C, 0x8F659EFF, 0xF862AE69, 0x616BFFD3, 0x166CCF45,
				0xA00AE278, 0xD70DD2EE, 0x4E048354, 0x3903B3C2, 0xA7672661, 0xD06016F7, 0x4969474D, 0x3E6E77DB, 0xAED16A4A, 0xD9D65ADC, 0x40DF0B66, 0x37D83BF0, 0xA9BCAE53, 0xDEBB9EC5, 0x47B2CF7F, 0x30B5FFE9,
				0xBDBDF21C, 0xCABAC28A, 0x53B39330, 0x24B4A3A6, 0xBAD03605, 0xCDD70693, 0x54DE5729, 0x23D967BF, 0xB3667A2E, 0xC4614AB8, 0x5D681B02, 0x2A6F2B94, 0xB40BBE37, 0xC30C8EA1, 0x5A05DF1B, 0x2D02EF8D
			};

			public static uint Calculate(byte[] bytes) => Calculate(bytes, 0, bytes.Length);
			public static uint Calculate(byte[] bytes, int offset, int length)
			{
				if (bytes == null) throw new ArgumentNullException("bytes").Overwrite(ConsoleUtilities.fileTag, $"Failed calculating CRC32");
				if (offset < 0 || length < 0 || offset + length > bytes.Length) throw new ArgumentOutOfRangeException().Overwrite(ConsoleUtilities.fileTag, $"Failed calculating CRC32");

				uint crc = 0;
				crc ^= seed;

				while (--length >= 0)
					crc = table[(crc ^ bytes[offset++]) & 0xFF] ^ (crc >> 8);

				crc ^= seed;
				return crc;
			}
		}
		#endregion CRC32

		#region INI
		public class INI
		{
			public abstract class Line { }

			#region Section
			public class Section : Line
			{
				public string name;

				public Section(string name)
				{
					this.name = name;
				}
				public override string ToString() => $"\n[{name}]";
			}
			#endregion Section
			#region Comment
			public class Comment : Line
			{
				public string text;

				public Comment(string text)
				{
					this.text = text;
				}
				public override string ToString() => $"; {text}";
			}
			#endregion Comment
			#region Property
			public class Property : Line
			{
				public string key;
				public object value;

				public Property(string key, object value)
				{
					this.key = key;
					this.value = value;
				}
				public override string ToString() => $"{key}: {value}";
			}
			#endregion Property


			public List<Line> lines;
			public Line this[int index] => lines[index];


			public INI() => this.lines = new List<Line>();
			public INI(string header) => this.lines = new List<Line>() { new Comment(header) };
			public INI(List<Line> lines) => this.lines = lines;

			public static INI Create(string[] strings)
			{
				INI ini = new INI();

				for (int i = 0; i < strings.Length; i++)
				{
					string str = strings[i].Trim();

					if (str.Empty()) continue;
					else if (str.StartsWith(';')) ini.AddComment(str[1..].Trim());
					else if (str.StartsWith('[') && str.EndsWith(']')) ini.AddSection(str[1..^1].Trim());
					else if (str.Contains(':'))
					{
						int index = str.IndexOf(':');
						ini.AddProperty(str[..index].Trim(), str[(index + 1)..].Trim());
					}
					else ConsoleUtilities.Warn(ConsoleUtilities.fileTag, $"Unexpected INI line '{str}'");
				}

				return ini;
			}

			public string[] ToStrings() => lines.Select(line => line.ToString()).ToArray();

			public void AddSection(string name) => lines.Add(new Section(name));
			public void AddComment(string text) => lines.Add(new Comment(text));
			public void AddProperty(string key, object value) => lines.Add(new Property(key, value));

			public Section FindSection(string name) => (Section)lines.Find(line => line is Section section && section.name == name);
			public Comment FindComment(string text) => (Comment)lines.Find(line => line is Comment comment && comment.text == text);
			public Property FindProperty(string key) => (Property)lines.Find(line => line is Property property && property.key == key);
		}
		#endregion INI
		#region PNG
		public class PNG
		{
			#region Chunk
			public readonly struct Chunk
			{
				public readonly byte[] bytes;

				public int LengthFull => bytes.Length;
				public int Length => bytes.Length - 12;
				public string Type => Encoding.ASCII.GetString(new ReadOnlySpan<byte>(bytes, 4, 4));
				public byte[] Data => bytes[8..(LengthFull - 4)];


				public Chunk(byte[] bytes) => this.bytes = bytes;

				public static Chunk Create(string type, byte[] data)
				{
					List<byte> bytes = new List<byte>(data.Length + 12);
					Span<byte> span = new Span<byte>(new byte[4]);

					BinaryPrimitives.WriteUInt32BigEndian(span, (uint)data.Length);
					bytes.AddRange(span.ToArray());

					bytes.AddRange(Encoding.ASCII.GetBytes(type));

					bytes.AddRange(data);

					BinaryPrimitives.WriteUInt32BigEndian(span, CRC32.Calculate(bytes.ToArray()[4..]));
					bytes.AddRange(span.ToArray());

					return new Chunk(bytes.ToArray());
				}
				public static Chunk Create(string type, string data)
				{
					return Create(type, Encoding.GetBytes(data));
				}
				public static Chunk Create(DateTime dateTime)
				{
					byte[] data = new byte[7]
					{
						(byte)(dateTime.Year >> 8),
						(byte)(dateTime.Year & 0xff),
						(byte)dateTime.Month,
						(byte)dateTime.Day,
						(byte)dateTime.Hour,
						(byte)dateTime.Minute,
						(byte)dateTime.Second,
					};

					return Create("tIME", data);
				}
			}
			#endregion Chunk


			public static byte[] header = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

			public List<Chunk> chunks;
			public Chunk this[int index] => chunks[index];

			public DateTime Time
			{
				get
				{
					Chunk chunk = Find("tIME");
					if (chunk.bytes == null || chunk.Length != 7) return default;

					ReadOnlySpan<byte> data = chunk.Data;
					int year = BinaryPrimitives.ReadInt16BigEndian(data[0..2]);
					return new DateTime(year, data[2], data[3], data[4], data[5], data[6], DateTimeKind.Local);
				}
			}


			public PNG() => this.chunks = new List<Chunk>();
			public PNG(List<Chunk> chunks) => this.chunks = chunks;

			public static PNG Create(Texture2D texture, params Chunk[] chunks) => Create(texture.EncodeToPNG(), chunks);
			public static PNG Create(byte[] bytes, params Chunk[] chunks)
			{
				PNG png = new PNG();

				ReadOnlySpan<byte> header = new ReadOnlySpan<byte>(bytes, 0, 8);
				int index = (header.SequenceEqual(PNG.header)) ? 8 : 0;

				while (index < bytes.Length)
				{
					int length = (int)BinaryPrimitives.ReadUInt32BigEndian(new ReadOnlySpan<byte>(bytes, index, 4));
					Chunk chunk = new Chunk(bytes[index..(index + length + 12)]);
					index += chunk.LengthFull;
					png.Append(chunk);
				}

				foreach (Chunk chunk in chunks)
					png.Add(chunk);

				return png;
			}

			public Texture2D ToTexture()
			{
				Texture2D texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
				if (!texture.LoadImage(ToBytes())) ConsoleUtilities.Warn(ConsoleUtilities.fileTag, $"Error converting PNG to Texture2D");

				return texture;
			}
			public byte[] ToBytes()
			{
				List<byte> bytes = header.ToList();

				foreach (Chunk chunk in chunks)
					bytes.AddRange(chunk.bytes);

				return bytes.ToArray();
			}

			public void Append(Chunk chunk) => chunks.Add(chunk);
			public void Add(Chunk chunk)
			{
				int index = 0;
				for (int i = 0; i < chunks.Count; i++)
					if (chunks[i].Type == "IDAT")
					{
						index = i;
						break;
					}

				chunks.Insert(index, chunk);
			}

			public Chunk Find(string type) => chunks.Find(chunk => chunk.Type == type);
		}
		#endregion PNG


		public static Encoding Encoding => Encoding.UTF8;
		public static string UserDataRoot => Application.persistentDataPath;
		public static string GameDataRoot => Application.dataPath;


		public static bool Save(string path, byte[] bytes)
		{
			try
			{
				path = ResolvePath(path);
				Directory.CreateDirectory(Path.GetDirectoryName(path));
				File.WriteAllBytes(path, bytes);
				return true;
			}
			catch (Exception exception) { exception.Error(ConsoleUtilities.fileTag, $"Failed saving file to {path:info}"); return false; }
		}
		public static bool Save(string path, IEnumerable<string> lines)
		{
			try
			{
				path = ResolvePath(path);
				Directory.CreateDirectory(Path.GetDirectoryName(path));
				File.WriteAllLines(path, lines, Encoding);
				return true;
			}
			catch (Exception exception) { exception.Error(ConsoleUtilities.fileTag, $"Failed saving file to {path:info}"); return false; }
		}
		public static bool Save(string path, string contents)
		{
			try
			{
				path = ResolvePath(path);
				Directory.CreateDirectory(Path.GetDirectoryName(path));
				File.WriteAllText(path, contents, Encoding);
				return true;
			}
			catch (Exception exception) { exception.Error(ConsoleUtilities.fileTag, $"Failed saving file to {path:info}"); return false; }
		}
		public static bool Save(string path, INI ini)
		{
			try
			{
				if (ini == null) throw new ArgumentNullException("ini", "Missing INI reference");
				return Save(ResolvePath(path, ".ini"), ini.ToStrings());
			}
			catch (Exception exception) { exception.Error(ConsoleUtilities.fileTag, $"Failed saving INI file to {path:info}"); return false; }
		}
		public static bool Save(string path, PNG png)
		{
			try
			{
				if (png == null) throw new ArgumentNullException("png", "Missing PNG reference");
				return Save(ResolvePath(path, ".png"), png.ToBytes());
			}
			catch (Exception exception) { exception.Error(ConsoleUtilities.fileTag, $"Failed saving PNG file to {path:info}"); return false; }
		}

		public static bool Load(string path, out byte[] bytes)
		{
			try { bytes = File.ReadAllBytes(ResolvePath(path)); }
			catch (Exception exception) { exception.Error(ConsoleUtilities.fileTag, $"Failed loading file at {path:info}"); bytes = null; }
			return bytes != null;
		}
		public static bool Load(string path, out string[] lines)
		{
			try { lines = File.ReadAllLines(ResolvePath(path), Encoding); }
			catch (Exception exception) { exception.Error(ConsoleUtilities.fileTag, $"Failed loading file at {path:info}"); lines = null; }
			return lines != null;
		}
		public static bool Load(string path, out string contents)
		{
			try { contents = File.ReadAllText(ResolvePath(path), Encoding); }
			catch (Exception exception) { exception.Error(ConsoleUtilities.fileTag, $"Failed loading file at {path:info}"); contents = null; }
			return contents != null;
		}
		public static bool Load(string path, out INI ini)
		{
			try { ini = INI.Create(File.ReadAllLines(ResolvePath(path, ".ini"), Encoding)); }
			catch (Exception exception) { exception.Error(ConsoleUtilities.fileTag, $"Failed loading INI file at {path:info}"); ini = null; }
			return ini != null;
		}
		public static bool Load(string path, out PNG png)
		{
			try { png = PNG.Create(File.ReadAllBytes(ResolvePath(path, ".png"))); }
			catch (Exception exception) { exception.Error(ConsoleUtilities.fileTag, $"Failed loading PNG file at {path:info}"); png = null; }
			return png != null;
		}

		private static string ResolvePath(string path) => (Path.IsPathRooted(path)) ? path : Path.Combine(UserDataRoot, path);
		private static string ResolvePath(string path, string extension)
		{
			string fullPath = ResolvePath(path);
			return (Path.GetExtension(path) == extension) ? fullPath : throw new ArgumentException(ConsoleUtilities.Format($"Path missing {extension:info} extension"));
		}
	}
}