using System;

namespace ModiBuff.Core
{
	public static class Random
	{
		private static int _seed;
		private static System.Random _random;

		static Random()
		{
			_seed = Environment.TickCount;
			_random = new System.Random(_seed);
		}
		
		public static float Value => (float)_random.NextDouble();
	}
}