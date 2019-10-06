namespace JAudio.Sequence
{
	public class MarkerEvent : Event
	{
		public string Text
		{
			get;
			set;
		}

		public MarkerEvent(string text)
		{
			Text = text;
		}
	}
}
