namespace JAudio.Sequence
{
	public class PanEvent : Event
	{
		public byte Pan
		{
			get;
			set;
		}

		public PanEvent(byte pan)
		{
			Pan = pan;
		}
	}
}
