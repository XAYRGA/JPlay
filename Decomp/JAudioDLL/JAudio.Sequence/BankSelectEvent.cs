namespace JAudio.Sequence
{
	public class BankSelectEvent : Event
	{
		public byte Bank
		{
			get;
			set;
		}

		public BankSelectEvent(byte bank)
		{
			Bank = bank;
		}
	}
}
