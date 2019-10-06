using System;

namespace JAudio.SoundData
{
	public class SampleNotFoundException : Exception
	{
		public SampleNotFoundException()
		{
		}

		public SampleNotFoundException(string message)
			: base(message)
		{
		}
	}
}
