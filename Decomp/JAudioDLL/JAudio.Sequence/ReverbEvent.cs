namespace JAudio.Sequence
{
	public class ReverbEvent : Event
	{
		public byte Reverb
		{
			get;
			set;
		}

		public ReverbEvent(byte reverb)
		{
			Reverb = reverb;
		}
	}
}
