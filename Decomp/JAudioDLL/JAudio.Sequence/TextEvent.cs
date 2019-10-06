namespace JAudio.Sequence
{
	public class TextEvent : Event
	{
		public string Text
		{
			get;
			set;
		}

		public TextEvent(string text)
		{
			Text = text;
		}
	}
}
