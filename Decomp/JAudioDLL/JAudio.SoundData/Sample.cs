using System;

namespace JAudio.SoundData
{
	public struct Sample
	{
		private byte _RootKey;

		private int _SampleRate;

		private short _BitsPerSample;

		private short _Channels;

		private bool _IsLooping;

		private int _LoopStart;

		private byte[] _Data;

		public byte RootKey
		{
			get
			{
				return _RootKey;
			}
			set
			{
				if (value < 128)
				{
					_RootKey = value;
					return;
				}
				throw new ArgumentOutOfRangeException("RootKey", "The root key must be less than or qual to 127.");
			}
		}

		public int SamplesPerSecond
		{
			get
			{
				return _SampleRate;
			}
			set
			{
				_SampleRate = value;
			}
		}

		public short BitsPerSample
		{
			get
			{
				return _BitsPerSample;
			}
			set
			{
				_BitsPerSample = value;
			}
		}

		public short Channels
		{
			get
			{
				return _Channels;
			}
			set
			{
				_Channels = value;
			}
		}

		public bool IsLooping
		{
			get
			{
				return _IsLooping;
			}
			set
			{
				_IsLooping = value;
			}
		}

		public int LoopStart
		{
			get
			{
				return _LoopStart;
			}
			set
			{
				_LoopStart = value;
			}
		}

		public byte[] Data
		{
			get
			{
				return _Data;
			}
			set
			{
				_Data = value;
			}
		}
	}
}
