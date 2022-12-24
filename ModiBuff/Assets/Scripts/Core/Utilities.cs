using System;
using System.Collections.Generic;
using System.Linq;

namespace ModiBuff.Utilities
{
	public static class Utilities
	{
		public static long FastLog2(double value)
		{
			return (BitConverter.DoubleToInt64Bits(value) >> 52) + 1 & 0xFF;
		}

		/// <summary>
		///     Is power of two, and isn't zero
		/// </summary>
		/// <param name="x"></param>
		public static bool IsPowerOfTwo(int x)
		{
			return x != 0 && (x & (x - 1)) == 0;
		}
	}
}