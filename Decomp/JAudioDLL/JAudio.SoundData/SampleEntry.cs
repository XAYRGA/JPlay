using System;

namespace JAudio.SoundData
{
	public struct SampleEntry
	{
		private byte _KeyRangeLimit;

		private int _Id;

		private float _FrequencyMultiplier;

		private byte? _Pan;

		public byte KeyRangeLimit
		{
			get
			{
				return _KeyRangeLimit;
			}
			set
			{
				if (value < 128)
				{
					_KeyRangeLimit = value;
					return;
				}
				throw new ArgumentOutOfRangeException("KeyRangeLimit", "Value must be less than or equal to 127.");
			}
		}

		public int Id
		{
			get
			{
				return _Id;
			}
			set
			{
				_Id = value;
			}
		}

		public float FrequenyMultiplier
		{
			get
			{
				return _FrequencyMultiplier;
			}
			set
			{
				_FrequencyMultiplier = value;
			}
		}

		public byte? Pan
		{
			get
			{
				return _Pan;
			}
			set
			{
				if (value < 128)
				{
					_Pan = value;
					return;
				}
				throw new ArgumentOutOfRangeException("Pan", "Value must be less than or equal to 127.");
			}
		}
	}
}
