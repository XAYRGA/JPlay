using System.Collections.Generic;

namespace JAudio.SoundData
{
	public class Instrument
	{
		public bool IsPercussion
		{
			get;
			private set;
		}

		public List<SampleEntry> Samples
		{
			get;
			internal set;
		}

		public float FrequencyMultiplier
		{
			get;
			internal set;
		}

		public float AttackTime
		{
			get;
			internal set;
		}

		public float ReleaseTime
		{
			get;
			internal set;
		}

		public Instrument(bool isPercussion = false)
		{
			IsPercussion = isPercussion;
			Samples = new List<SampleEntry>();
			FrequencyMultiplier = 1f;
			AttackTime = 0f;
			ReleaseTime = 0.2f;
		}

		public int? GetSampleEntry(byte key)
		{
			for (int i = 0; i < Samples.Count; i++)
			{
				if (!IsPercussion)
				{
					if (Samples[i].KeyRangeLimit >= key)
					{
						return i;
					}
				}
				else if (Samples[i].KeyRangeLimit == key)
				{
					return i;
				}
			}
			return null;
		}
	}
}
