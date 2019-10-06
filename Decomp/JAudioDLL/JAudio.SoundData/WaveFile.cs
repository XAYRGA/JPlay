using System;
using System.IO;

namespace JAudio.SoundData
{
	internal class WaveFile
	{
		public WaveFormat Format;

		private BinaryReader br;

		private BinaryWriter bw;

		public int AudioDataSize
		{
			get
			{
				uint chunkPosition = (uint)GetChunkPosition(1635017060u);
				if (chunkPosition == 0)
				{
					return 0;
				}
				br.BaseStream.Position = chunkPosition + 4;
				return br.ReadInt32();
			}
		}

		public WaveFile()
		{
		}

		public WaveFile(Stream stream)
		{
			try
			{
				br = new BinaryReader(stream);
				if (br.ReadInt32() != 1179011410)
				{
					throw new Exception("'RIFF' expected.");
				}
				br.BaseStream.Position += 4L;
				if (br.ReadInt32() != 1163280727)
				{
					throw new Exception("'WAVE' expected.");
				}
				uint chunkPosition = (uint)GetChunkPosition(544501094u);
				if (chunkPosition == 0)
				{
					throw new Exception();
				}
				br.BaseStream.Position = chunkPosition + 8;
				Format = default(WaveFormat);
				Format.Format = (ushort)br.ReadInt16();
				Format.Channels = (ushort)br.ReadInt16();
				Format.SamplesPerSecond = (uint)br.ReadInt32();
				br.BaseStream.Position += 6L;
				Format.BitsPerSample = (ushort)br.ReadInt16();
				if (Format.Format != 1)
				{
					throw new Exception("Only PCM encoding is supported.");
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public byte[] GetAudioData()
		{
			uint chunkPosition = (uint)GetChunkPosition(1635017060u);
			if (chunkPosition == 0)
			{
				throw new Exception("Could not find audio chunk.");
			}
			br.BaseStream.Position = chunkPosition + 4;
			int count = br.ReadInt32();
			return br.ReadBytes(count);
		}

		public void Save(string file, byte[] audioData)
		{
			bw = new BinaryWriter(File.OpenWrite(file));
			bw.Write(1179011410);
			bw.Write(36 + audioData.Length);
			bw.Write(1163280727);
			bw.Write(544501094);
			bw.Write(16);
			bw.Write(Format.Format);
			bw.Write(Format.Channels);
			bw.Write(Format.SamplesPerSecond);
			bw.Write(Format.BytesPerSecond);
			bw.Write(Format.BlockAlign);
			bw.Write(Format.BitsPerSample);
			bw.Write(1635017060);
			bw.Write(audioData.Length);
			bw.Write(audioData);
		}

		private int GetChunkPosition(uint chunkID)
		{
			for (int i = 12; i < br.BaseStream.Length - 4; i += br.ReadInt32() + 8)
			{
				br.BaseStream.Position = i;
				if (br.ReadInt32() == chunkID)
				{
					return i;
				}
			}
			return 0;
		}
	}
}
