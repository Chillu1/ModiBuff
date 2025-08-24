using System;
using System.Runtime.CompilerServices;

namespace ModiBuff.Core
{
	[Flags]
	public enum ComparisonType
	{
		Greater = 1,
		Equal = 2,
		Less = 4,

		GreaterOrEqual = Greater | Equal,
		LessOrEqual = Less | Equal,
	}

	public static class ComparisonTypeExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Check(this ComparisonType comparisonType, float valueOne, float valueTwo)
		{
			switch (comparisonType)
			{
				case ComparisonType.Greater:
					return valueOne > valueTwo;
				case ComparisonType.Equal:
					return Math.Abs(valueOne - valueTwo) < Config.DeltaTolerance;
				case ComparisonType.Less:
					return valueOne < valueTwo;
				case ComparisonType.GreaterOrEqual:
					return valueOne >= valueTwo;
				case ComparisonType.LessOrEqual:
					return valueOne <= valueTwo;
				default:
					Logger.LogError("[ModiBuff] Invalid comparison type: " + comparisonType);
					return false;
			}
		}
	}
}