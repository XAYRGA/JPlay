using System;

namespace JAudio.Utils
{
	public static class Endianness
	{
		public static short Swap(short val)
		{
			return (short)(((val >> 8) & 0xFF) + ((val << 8) & 0xFF00));
		}

		public static ushort Swap(ushort val)
		{
			return (ushort)Swap((short)val);
		}

		public static int Swap(int val)
		{
			return ((val & 0xFF) << 24) + ((val & 0xFF00) << 8) + ((val & 0xFF0000) >> 8) + ((val >> 24) & 0xFF);
		}

		public static uint Swap(uint val)
		{
			return (uint)Swap((int)val);
		}

		public static float Swap(float val)
		{
			return BitConverter.ToSingle(BitConverter.GetBytes(Swap(BitConverter.ToUInt32(BitConverter.GetBytes(val), 0))), 0);
		}
	}
}
