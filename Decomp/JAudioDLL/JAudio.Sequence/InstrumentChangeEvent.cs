namespace JAudio.Sequence
{
	public class InstrumentChangeEvent : Event
	{
		public byte Instrument
		{
			get;
			set;
		}

		public InstrumentChangeEvent(byte instrument)
		{
			Instrument = instrument;
		}
	}
}
