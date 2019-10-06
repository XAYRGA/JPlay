using JAudio.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JAudio.SoundData
{
	public class SampleBank
	{
		private struct SampleLocation
		{
			private int _Wsys;

			private int _Index;

			private int _Offset;

			public int Wsys
			{
				get
				{
					return _Wsys;
				}
				set
				{
					_Wsys = value;
				}
			}

			public int Index
			{
				get
				{
					return _Index;
				}
				set
				{
					_Index = value;
				}
			}

			public int Offset
			{
				get
				{
					return _Offset;
				}
				set
				{
					_Offset = value;
				}
			}
		}

		private enum ChunkIdentifiers : uint
		{
			Wbct = 1463960404u,
			Winf = 1464421958u,
			CDf = 1127040070u,
			CEx = 1127040344u,
			CSt = 1127043924u,
			Scne = 1396919877u
		}

		private const uint MagicNumber = 1465080147u;

		private Dictionary<short, SampleLocation> Samples;

		private List<string> SoundFiles;

		private BinaryReader reader;

		private static FileFormatException FormatExc = new FileFormatException("The file might be corrupted truncated or in an unexpected format.");

		public Sample this[short sample]
		{
			get
			{
				if (!Samples.ContainsKey(sample))
				{
					throw new SampleNotFoundException($"Sample {sample:x4} not found.");
				}
				reader.BaseStream.Position = WinfChunkOffset + 8 + 4 * Samples[sample].Wsys;
				reader.BaseStream.Position = Endianness.Swap(reader.ReadUInt32()) + 116 + 4 * Samples[sample].Index;
				reader.BaseStream.Position = Endianness.Swap(reader.ReadUInt32()) + 2;
				byte rootKey = reader.ReadByte();
				reader.BaseStream.Position += 2L;
				Endianness.Swap(reader.ReadInt16());
				reader.BaseStream.Position++;
				Endianness.Swap(reader.ReadInt32());
				Endianness.Swap(reader.ReadInt32());
				bool flag = (reader.ReadUInt32() == uint.MaxValue) ? true : false;
				int loopStart = Endianness.Swap(reader.ReadInt32());
				int num = Endianness.Swap(reader.ReadInt32());
				string path = string.Format(Z2Sound.SoundPath + "\\Waves\\{0}_{1:x8}.wav", SoundFiles[Samples[sample].Wsys], Samples[sample].Index);
				WaveFile waveFile = new WaveFile(File.OpenRead(path));
				byte[] array;
				if (flag)
				{
					int num2 = num * ((int)waveFile.Format.BitsPerSample / 8) * waveFile.Format.Channels;
					array = new byte[num2];
					Array.Copy(waveFile.GetAudioData(), array, num2);
				}
				else
				{
					array = waveFile.GetAudioData();
				}
				Sample result = default(Sample);
				result.BitsPerSample = 16;
				result.Channels = 1;
				result.IsLooping = flag;
				result.LoopStart = loopStart;
				result.RootKey = rootKey;
				result.SamplesPerSecond = (int)waveFile.Format.SamplesPerSecond;
				result.Data = array;
				return result;
			}
		}

		private uint WinfChunkOffset
		{
			get;
			set;
		}

		private uint WbctChunkOffset
		{
			get;
			set;
		}

		public SampleBank(Stream stream)
		{
			try
			{
				reader = new BinaryReader(stream);
				if (Endianness.Swap(reader.ReadUInt32()) != 1465080147)
				{
					throw new FileFormatException("Unrecognized file type. The file might be corrupted truncated or in an unexpected format.");
				}
				reader.BaseStream.Position += 12L;
				WinfChunkOffset = Endianness.Swap(reader.ReadUInt32());
				WbctChunkOffset = Endianness.Swap(reader.ReadUInt32());
				reader.BaseStream.Position = WinfChunkOffset;
				if (Endianness.Swap(reader.ReadUInt32()) != 1464421958)
				{
					throw FormatExc;
				}
				int num = Endianness.Swap(reader.ReadInt32());
				SoundFiles = new List<string>(num);
				for (int i = 0; i < num; i++)
				{
					reader.BaseStream.Position = WinfChunkOffset + 8 + 4 * i;
					reader.BaseStream.Position = Endianness.Swap(reader.ReadUInt32());
					List<byte> list = new List<byte>();
					while (true)
					{
						byte b = reader.ReadByte();
						if (b == 0)
						{
							break;
						}
						list.Add(b);
					}
					SoundFiles.Add(Encoding.ASCII.GetString(list.ToArray()));
				}
				Samples = new Dictionary<short, SampleLocation>();
				reader.BaseStream.Position = WbctChunkOffset;
				if (Endianness.Swap(reader.ReadUInt32()) != 1463960404)
				{
					throw FormatExc;
				}
				reader.BaseStream.Position += 4L;
				if (Endianness.Swap(reader.ReadUInt32()) != SoundFiles.Count)
				{
					throw FormatExc;
				}
				uint num2 = 0u;
				while (true)
				{
					if (num2 >= (uint)SoundFiles.Count)
					{
						return;
					}
					reader.BaseStream.Position = WbctChunkOffset + 12 + 4 * num2;
					reader.BaseStream.Position = Endianness.Swap(reader.ReadUInt32());
					if (Endianness.Swap(reader.ReadUInt32()) != 1396919877)
					{
						throw FormatExc;
					}
					reader.BaseStream.Position += 8L;
					reader.BaseStream.Position = Endianness.Swap(reader.ReadUInt32());
					if (Endianness.Swap(reader.ReadUInt32()) != 1127040070)
					{
						break;
					}
					uint num3 = Endianness.Swap(reader.ReadUInt32());
					uint num4 = (uint)reader.BaseStream.Position;
					for (int j = 0; j < num3; j++)
					{
						reader.BaseStream.Position = num4 + 4 * j;
						reader.BaseStream.Position = Endianness.Swap(reader.ReadUInt32());
						int wsys = Endianness.Swap(reader.ReadInt16());
						short key = Endianness.Swap(reader.ReadInt16());
						if (!Samples.ContainsKey(key))
						{
							Samples.Add(key, new SampleLocation
							{
								Index = j,
								Wsys = wsys
							});
						}
					}
					num2++;
				}
				throw FormatExc;
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
	}
}
