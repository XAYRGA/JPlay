namespace JAudio.Sequence
{
	public class NoteOffEvent : Event
	{
		public byte Number
		{
			get;
			set;
		}

		public NoteOffEvent(byte number)
		{
			Number = number;
		}
	}
}
