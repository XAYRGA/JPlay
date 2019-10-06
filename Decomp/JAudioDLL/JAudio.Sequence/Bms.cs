using JAudio.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JAudio.Sequence
{
	public class Bms
	{
		private BinaryReader reader;

		public List<Event> MetaTrack
		{
			get;
			private set;
		}

		public List<Event> TrackList
		{
			get;
			private set;
		}

		public List<Track> Tracks
		{
			get;
			private set;
		}

		public Bms(Stream stream)
		{
			try
			{
				reader = new BinaryReader(stream);
				int num = 0;
				List<int> list = new List<int>();
				MetaTrack = new List<Event>();
				byte b = byte.MaxValue;
				do
				{
					b = reader.ReadByte();
					switch (b)
					{
					case 193:
						if (num == 0)
						{
							num = Endianness.Swap(reader.ReadInt32());
						}
						else
						{
							reader.BaseStream.Position += 4L;
						}
						break;
					case 216:
					{
						byte b2 = reader.ReadByte();
						if (b2 == 98)
						{
							MetaTrack.Add(new TimeResolutionEvent(Endianness.Swap(reader.ReadInt16())));
						}
						else
						{
							reader.BaseStream.Position += 2L;
						}
						break;
					}
					case 224:
						MetaTrack.Add(new TempoEvent(Endianness.Swap(reader.ReadInt16())));
						break;
					case 240:
					{
						int num4 = 1;
						while (reader.ReadByte() >= 128)
						{
							num4++;
						}
						reader.BaseStream.Position -= num4;
						byte[] data = reader.ReadBytes(num4);
						MetaTrack.Add(new DelayEvent(GetLength(data)));
						break;
					}
					case 199:
					{
						byte[] array3 = new List<byte>(4)
						{
							0,
							reader.ReadByte(),
							reader.ReadByte(),
							reader.ReadByte()
						}.ToArray();
						Array.Reverse(array3);
						BitConverter.ToInt32(array3, 0);
						break;
					}
					case 253:
					{
						List<byte> list2 = new List<byte>();
						while (true)
						{
							byte b3 = reader.ReadByte();
							if (b3 == 0)
							{
								break;
							}
							list2.Add(b3);
						}
						new ASCIIEncoding();
						break;
					}
					case 195:
					{
						byte[] array2 = new List<byte>(4)
						{
							0,
							reader.ReadByte(),
							reader.ReadByte(),
							reader.ReadByte()
						}.ToArray();
						Array.Reverse(array2);
						int num3 = BitConverter.ToInt32(array2, 0);
						list.Add((int)reader.BaseStream.Position);
						reader.BaseStream.Position = num3;
						break;
					}
					case 196:
					{
						reader.ReadByte();
						byte[] array = new List<byte>(4)
						{
							0,
							reader.ReadByte(),
							reader.ReadByte(),
							reader.ReadByte()
						}.ToArray();
						Array.Reverse(array);
						int num2 = BitConverter.ToInt32(array, 0);
						list.Add((int)reader.BaseStream.Position);
						reader.BaseStream.Position = num2;
						break;
					}
					case 197:
						if (list.Count > 0)
						{
							reader.BaseStream.Position = list[list.Count - 1];
							list.RemoveAt(list.Count - 1);
						}
						break;
					case byte.MaxValue:
						MetaTrack.Add(new TerminateEvent());
						break;
					case 194:
						reader.BaseStream.Position++;
						break;
					case 184:
					case 208:
					case 209:
					case 213:
					case 249:
						reader.BaseStream.Position += 2L;
						break;
					case 217:
						reader.BaseStream.Position += 3L;
						break;
					case 200:
					case 218:
						reader.BaseStream.Position += 4L;
						break;
					}
				}
				while (b != byte.MaxValue);
				reader.BaseStream.Position = num;
				TrackList = new List<Event>();
				Tracks = new List<Track>();
				b = byte.MaxValue;
				do
				{
					b = reader.ReadByte();
					switch (b)
					{
					case 193:
					{
						byte trackNumber = reader.ReadByte();
						byte[] array5 = new List<byte>(4)
						{
							0,
							reader.ReadByte(),
							reader.ReadByte(),
							reader.ReadByte()
						}.ToArray();
						Array.Reverse(array5);
						int offset2 = BitConverter.ToInt32(array5, 0);
						Tracks.Add(new Track(trackNumber, offset2));
						break;
					}
					case 240:
					{
						int num5 = 1;
						while (reader.ReadByte() >= 128)
						{
							num5++;
						}
						reader.BaseStream.Position -= num5;
						byte[] data2 = reader.ReadBytes(num5);
						TrackList.Add(new DelayEvent(GetLength(data2)));
						break;
					}
					case 199:
					{
						byte[] array4 = new List<byte>(4)
						{
							0,
							reader.ReadByte(),
							reader.ReadByte(),
							reader.ReadByte()
						}.ToArray();
						Array.Reverse(array4);
						int offset = BitConverter.ToInt32(array4, 0);
						TrackList.Add(new LoopEvent(offset));
						break;
					}
					}
				}
				while (b != byte.MaxValue);
				foreach (Track track in Tracks)
				{
					reader.BaseStream.Position = track.Offset;
					b = byte.MaxValue;
					do
					{
						b = reader.ReadByte();
						if (b <= 128)
						{
							track.Data.Add(new NoteOnEvent(b, reader.ReadByte(), reader.ReadByte()));
						}
						else if (b > 128 && b <= 143)
						{
							track.Data.Add(new NoteOffEvent((byte)(b & 0xF)));
						}
						else
						{
							switch (b)
							{
							case 240:
							{
								int num7 = 1;
								while (reader.ReadByte() >= 128)
								{
									num7++;
								}
								reader.BaseStream.Position -= num7;
								byte[] data3 = reader.ReadBytes(num7);
								track.Data.Add(new DelayEvent(GetLength(data3)));
								break;
							}
							case 226:
								track.Data.Add(new BankSelectEvent(reader.ReadByte()));
								break;
							case 227:
								track.Data.Add(new InstrumentChangeEvent(reader.ReadByte()));
								break;
							case 184:
								switch (reader.ReadByte())
								{
								case 0:
									track.Data.Add(new VolumeEvent(reader.ReadByte()));
									break;
								case 2:
									track.Data.Add(new ReverbEvent(reader.ReadByte()));
									break;
								case 3:
									track.Data.Add(new PanEvent(reader.ReadByte()));
									break;
								default:
									reader.BaseStream.Position++;
									break;
								}
								break;
							case 195:
							{
								byte[] array6 = new List<byte>(4)
								{
									0,
									reader.ReadByte(),
									reader.ReadByte(),
									reader.ReadByte()
								}.ToArray();
								Array.Reverse(array6);
								int num6 = BitConverter.ToInt32(array6, 0);
								list.Add((int)reader.BaseStream.Position);
								reader.BaseStream.Position = num6;
								break;
							}
							case 196:
							{
								reader.ReadByte();
								byte[] array8 = new List<byte>(4)
								{
									0,
									reader.ReadByte(),
									reader.ReadByte(),
									reader.ReadByte()
								}.ToArray();
								Array.Reverse(array8);
								int num8 = BitConverter.ToInt32(array8, 0);
								list.Add((int)reader.BaseStream.Position);
								reader.BaseStream.Position = num8;
								break;
							}
							case 199:
							{
								byte[] array7 = new List<byte>(4)
								{
									0,
									reader.ReadByte(),
									reader.ReadByte(),
									reader.ReadByte()
								}.ToArray();
								Array.Reverse(array7);
								BitConverter.ToInt32(array7, 0);
								break;
							}
							case 197:
								if (list.Count > 0)
								{
									reader.BaseStream.Position = list[list.Count - 1];
									list.RemoveAt(list.Count - 1);
								}
								break;
							case 185:
							{
								byte b5 = reader.ReadByte();
								if (b5 == 1)
								{
									track.Data.Add(new PitchEvent(Endianness.Swap(reader.ReadInt16())));
								}
								else
								{
									reader.BaseStream.Position += 2L;
								}
								break;
							}
							case 216:
							{
								byte b4 = reader.ReadByte();
								if (b4 == 110)
								{
									track.Data.Add(new VibratoEvent(Endianness.Swap(reader.ReadUInt16())));
								}
								else
								{
									reader.BaseStream.Position += 2L;
								}
								break;
							}
							case byte.MaxValue:
								track.Data.Add(new TerminateEvent());
								break;
							case 249:
								reader.BaseStream.Position += 2L;
								break;
							}
						}
					}
					while (b != byte.MaxValue);
				}
				reader.BaseStream.Close();
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				stream.Close();
			}
		}

		private int GetLength(byte[] data)
		{
			int num = 0;
			for (uint num2 = 0u; num2 < data.Length; num2++)
			{
				num += (int)(Math.Pow(128.0, num2) * (double)(data[data.Length - 1 - num2] - ((num2 != 0) ? 128 : 0)));
			}
			return num;
		}
	}
}
