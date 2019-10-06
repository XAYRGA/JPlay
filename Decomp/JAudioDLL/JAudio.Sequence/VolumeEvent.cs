namespace JAudio.Sequence
{
	public class VolumeEvent : Event
	{
		public byte Volume
		{
			get;
			set;
		}

		public VolumeEvent(byte volume)
		{
			Volume = volume;
		}
	}
}
