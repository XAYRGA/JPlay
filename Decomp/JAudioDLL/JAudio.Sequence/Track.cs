using System.Collections.Generic;

namespace JAudio.Sequence
{
	public class Track
	{
		public byte TrackNumber
		{
			get;
			internal set;
		}

		public List<Event> Data
		{
			get;
			internal set;
		}

		internal int Offset
		{
			get;
			private set;
		}

		public Track(byte trackNumber, int offset)
		{
			TrackNumber = trackNumber;
			Offset = offset;
			Data = new List<Event>();
		}
	}
}
