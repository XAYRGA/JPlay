namespace JAudio.SoundData
{
	internal struct WaveFormat
	{
		public ushort Format
		{
			get;
			set;
		}

		public ushort Channels
		{
			get;
			set;
		}

		public uint SamplesPerSecond
		{
			get;
			set;
		}

		public ushort BitsPerSample
		{
			get;
			set;
		}

		public ushort BlockAlign => (ushort)(Channels * ((int)BitsPerSample / 8));

		public uint BytesPerSecond => BlockAlign * SamplesPerSecond;
	}
}
