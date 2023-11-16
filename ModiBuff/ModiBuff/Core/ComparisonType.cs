using System;

namespace ModiBuff.Core
{
	[Flags]
	public enum ComparisonType
	{
		None,
		Greater = 1,
		Equal = 2,
		Less = 4,

		GreaterOrEqual = Greater | Equal,
		LessOrEqual = Less | Equal,
	}

	public static class ComparisonTypeExtensions
	{
		public static bool Check(this ComparisonType comparisonType, float valueOne, float valueTwo)
		{
			switch (comparisonType)
			{
				case ComparisonType.None:
					return true;
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