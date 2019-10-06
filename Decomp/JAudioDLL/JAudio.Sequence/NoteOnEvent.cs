namespace JAudio.Sequence
{
	public class NoteOnEvent : Event
	{
		public byte Note
		{
			get;
			set;
		}

		public byte Number
		{
			get;
			set;
		}

		public byte Velocity
		{
			get;
			set;
		}

		public NoteOnEvent(byte note, byte number, byte velocity)
		{
			Note = note;
			Number = number;
			Velocity = velocity;
		}
	}
}
