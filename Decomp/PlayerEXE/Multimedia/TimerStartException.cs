using System;

namespace Multimedia
{
	public class TimerStartException : ApplicationException
	{
		public TimerStartException(string message)
			: base(message)
		{
		}
	}
}
