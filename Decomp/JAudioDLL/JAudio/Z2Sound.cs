using JAudio.SoundData;
using JAudio.Utils;
using System;
using System.Collections.Generic;
using System.IO;

namespace JAudio
{
	public class Z2Sound
	{
		private enum FileIdentifiers : uint
		{
			HeaderEnd = 1046430017u,
			Bst = 1651733536u,
			Bstn = 1651733614u,
			Wsys = 2004033568u,
			Bank = 1651403552u,
			Bsc = 1651729184u,
			Bfca = 1650877281u
		}

		private const uint MagicNumber = 1094803260u;

		public Dictionary<int, SampleBank> SampleBanks;

		public Dictionary<int, InstrumentBank> InstrumentBanks;

		private BinaryReader reader;

		public static string SoundPath;

		public Z2Sound(Stream stream)
		{
			try
			{
				reader = new BinaryReader(stream);
				if (Endianness.Swap(reader.ReadUInt32()) != 1094803260)
				{
					throw new FileFormatException();
				}
				SampleBanks = new Dictionary<int, SampleBank>();
				InstrumentBanks = new Dictionary<int, InstrumentBank>();
				bool flag = false;
				while (!flag)
				{
					switch (Endianness.Swap(reader.ReadUInt32()))
					{
					case 1651729184u:
					case 1651733536u:
					case 1651733614u:
						reader.BaseStream.Position += 8L;
						break;
					case 2004033568u:
					{
						long position2 = reader.BaseStream.Position + 12;
						int key2 = Endianness.Swap(reader.ReadInt32());
						long num2 = Endianness.Swap(reader.ReadInt32());
						reader.BaseStream.Position = num2 + 4;
						long size2 = Endianness.Swap(reader.ReadInt32());
						reader.BaseStream.Position = position2;
						VirtualStream stream2 = new VirtualStream(reader.BaseStream, num2, size2);
						SampleBanks.Add(key2, new SampleBank(stream2));
						break;
					}
					case 1651403552u:
					{
						long position = reader.BaseStream.Position + 8;
						int wsys = Endianness.Swap(reader.ReadInt32());
						long num = Endianness.Swap(reader.ReadInt32());
						reader.BaseStream.Position = num + 4;
						long size = Endianness.Swap(reader.ReadInt32());
						int key = Endianness.Swap(reader.ReadInt32());
						reader.BaseStream.Position = position;
						InstrumentBanks.Add(key, new InstrumentBank(new VirtualStream(reader.BaseStream, num, size), wsys));
						break;
					}
					case 1650877281u:
						reader.BaseStream.Position += 4L;
						break;
					default:
						flag = true;
						break;
					}
				}
			}
			catch (EndOfStreamException)
			{
				throw new FileFormatException();
			}
			catch (Exception ex2)
			{
				throw ex2;
			}
		}

		~Z2Sound()
		{
			reader.BaseStream.Close();
		}
	}
}
