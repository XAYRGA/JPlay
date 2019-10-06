namespace JAudio.Sequence
{
	public class PitchEvent : Event
	{
		public short Pitch
		{
			get;
			set;
		}

		public PitchEvent(short pitch)
		{
			Pitch = pitch;
		}
	}
}
