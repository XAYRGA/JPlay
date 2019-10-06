namespace JAudio.Sequence
{
	public class VibratoEvent : Event
	{
		public ushort Vibrato
		{
			get;
			set;
		}

		public VibratoEvent(ushort vibrato)
		{
			Vibrato = vibrato;
		}
	}
}
