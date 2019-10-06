namespace JAudio.Sequence
{
	public class TimeResolutionEvent : Event
	{
		public short TimeResolution
		{
			get;
			set;
		}

		public TimeResolutionEvent(short resolution)
		{
			TimeResolution = resolution;
		}
	}
}
