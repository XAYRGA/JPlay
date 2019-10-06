using JAudio.Utils;
using System;
using System.Collections.Generic;
using System.IO;

namespace JAudio.SoundData
{
	public class InstrumentBank
	{
		private enum ChunkIdentifiers : uint
		{
			Envt = 1162761812u,
			Osct = 1330856788u,
			Rand = 1380011588u,
			Sens = 1397050963u,
			Inst = 1229869908u,
			InstEntry = 1231975284u,
			Pmap = 1347240272u,
			PmapEntry = 1349345648u,
			Perc = 1346720323u,
			PercEntry = 1348825699u,
			List = 1279873876u
		}

		private const uint MagicNumber = 1229082187u;

		private uint ListOffset;

		private BinaryReader reader;

		public Instrument this[int instrument]
		{
			get
			{
				reader.BaseStream.Position = ListOffset;
				if (instrument >= Endianness.Swap(reader.ReadInt32()))
				{
					throw new ArgumentOutOfRangeException("instrument", "The instrument number must be less than the total amount of instruments.");
				}
				reader.BaseStream.Position += 4 * instrument;
				reader.BaseStream.Position = Endianness.Swap(reader.ReadUInt32());
				switch (Endianness.Swap(reader.ReadUInt32()))
				{
				case 1231975284u:
				{
					Instrument instrument3 = new Instrument();
					reader.BaseStream.Position += 12L;
					int num3 = Endianness.Swap(reader.ReadInt32());
					if (num3 == 0)
					{
						num3 = Endianness.Swap(reader.ReadInt32());
					}
					long position2 = reader.BaseStream.Position;
					for (int j = 0; j < num3; j++)
					{
						reader.BaseStream.Position = position2 + 24 * j;
						SampleEntry item2 = default(SampleEntry);
						item2.KeyRangeLimit = reader.ReadByte();
						reader.BaseStream.Position += 11L;
						item2.Id = Endianness.Swap(reader.ReadInt32());
						reader.BaseStream.Position += 4L;
						item2.FrequenyMultiplier = Endianness.Swap(reader.ReadSingle());
						instrument3.Samples.Add(item2);
					}
					reader.BaseStream.Position = position2 + num3 * 24 + 4;
					instrument3.FrequencyMultiplier = Endianness.Swap(reader.ReadSingle());
					return instrument3;
				}
				case 1348825699u:
				{
					Instrument instrument2 = new Instrument(isPercussion: true);
					int num = Endianness.Swap(reader.ReadInt32());
					long position = reader.BaseStream.Position;
					for (int i = 0; i < num; i++)
					{
						reader.BaseStream.Position = position + 4 * i;
						int num2 = Endianness.Swap(reader.ReadInt32());
						if (num2 != 0)
						{
							reader.BaseStream.Position = num2;
							if (Endianness.Swap(reader.ReadUInt32()) != 1349345648)
							{
								throw new Exception("A percussion entry was ecpected.");
							}
							reader.BaseStream.Position += 4L;
							SampleEntry item = default(SampleEntry);
							item.FrequenyMultiplier = Endianness.Swap(reader.ReadSingle());
							item.Pan = reader.ReadByte();
							reader.BaseStream.Position += 15L;
							item.Id = Endianness.Swap(reader.ReadInt32());
							item.KeyRangeLimit = (byte)i;
							instrument2.Samples.Add(item);
						}
					}
					return instrument2;
				}
				default:
					throw new FileFormatException("Neither an instrument nor percussion data was found. The file might be corrupted, truncated or in an unexpected format.");
				}
			}
		}

		public int Wsys
		{
			get;
			private set;
		}

		public InstrumentBank(Stream stream, int wsys)
		{
			try
			{
				Wsys = wsys;
				reader = new BinaryReader(stream);
				if (Endianness.Swap(reader.ReadUInt32()) != 1229082187)
				{
					throw new FileFormatException("Unrecognized file type. The file might be corrupted, truncated or in an unexpected format.");
				}
				uint chunkPosition = GetChunkPosition(1279873876u);
				if (chunkPosition == 0)
				{
					throw new FileFormatException("The instrument list could not be found. The file might be corrupted, truncated or in an unexpected format.");
				}
				ListOffset = chunkPosition;
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public uint[] GetSamples(uint instrument)
		{
			reader.BaseStream.Position = ListOffset + 4 + 4 * instrument;
			reader.BaseStream.Position = Endianness.Swap(reader.ReadUInt32());
			switch (Endianness.Swap(reader.ReadUInt32()))
			{
			case 1231975284u:
			{
				reader.BaseStream.Position += 12L;
				uint num5 = Endianness.Swap(reader.ReadUInt32());
				if (num5 == 0)
				{
					num5 = Endianness.Swap(reader.ReadUInt32());
				}
				uint num6 = (uint)reader.BaseStream.Position;
				List<uint> list2 = new List<uint>();
				for (int i = 0; i < num5; i++)
				{
					reader.BaseStream.Position = num6 + 24 * i;
					reader.BaseStream.Position += 12L;
					list2.Add(Endianness.Swap(reader.ReadUInt32()));
				}
				return list2.ToArray();
			}
			case 1348825699u:
			{
				uint num = Endianness.Swap(reader.ReadUInt32());
				uint num2 = (uint)reader.BaseStream.Position;
				List<uint> list = new List<uint>();
				for (uint num3 = 0u; num3 < num; num3++)
				{
					reader.BaseStream.Position = num2 + num3 * 4;
					uint num4 = Endianness.Swap(reader.ReadUInt32());
					if (num4 != 0)
					{
						reader.BaseStream.Position = num4;
						if (Endianness.Swap(reader.ReadUInt32()) != 1349345648)
						{
							throw new Exception("\"Pmap\" expected.");
						}
						reader.BaseStream.Position += 24L;
						list.Add(Endianness.Swap(reader.ReadUInt32()));
					}
				}
				return list.ToArray();
			}
			default:
				throw new FileFormatException("Neither an instrument nor percussion data was found. The file might be corrupted, truncated or in an unexpected format.");
			}
		}

		private uint GetChunkPosition(uint chunkID)
		{
			uint num2;
			for (uint num = 32u; num < reader.BaseStream.Length - 4; num += num2 + 8)
			{
				reader.BaseStream.Position = num;
				if (Endianness.Swap(reader.ReadInt32()) == chunkID)
				{
					return num + 8;
				}
				num2 = Endianness.Swap(reader.ReadUInt32());
				if (num2 % 4u != 0)
				{
					num2 += num2 % 4u;
				}
			}
			return 0u;
		}
	}
}
