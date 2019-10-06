namespace JAudio.Sequence
{
	public class LoopEvent : Event
	{
		public int Offset
		{
			get;
			set;
		}

		public LoopEvent(int offset)
		{
			Offset = offset;
		}
	}
}
