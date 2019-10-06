namespace JAudio.Sequence
{
	public class DelayEvent : Event
	{
		public int Delay
		{
			get;
			set;
		}

		public DelayEvent(int delay)
		{
			Delay = delay;
		}
	}
}
