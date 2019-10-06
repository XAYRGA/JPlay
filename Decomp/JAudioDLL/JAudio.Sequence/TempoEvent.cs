namespace JAudio.Sequence
{
	public class TempoEvent : Event
	{
		public short Tempo
		{
			get;
			set;
		}

		public TempoEvent(short tempo)
		{
			Tempo = tempo;
		}
	}
}
