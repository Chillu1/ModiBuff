namespace ModiBuff.Core.Units
{
	/// <summary>
	///		Used for legal targeting
	/// </summary>
	public enum UnitType
	{
		None,
		Good = 1,
		Bad = 2,
		Neutral = 3,
	}

	/// <summary>
	///		Example of how to use custom modifier data for advanced tagging purposes
	/// </summary>
	public enum AllyUnitType
	{
		Warrior,
		Archer,
		Mage,
	}

	/// <summary>
	///		Example of how to use custom modifier data for advanced tagging purposes
	/// </summary>
	public enum EnemyUnitType
	{
		Slime,
		Goblin,
		Orc,
	}

	public static class UnitTypeExtensions
	{
		public static bool IsLegalTarget(this UnitType unitType, UnitType target)
		{
			return unitType != target;

			// switch (unitType)
			// {
			// 	case UnitType.Good when target == UnitType.Bad || target == UnitType.Neutral:
			// 		return true;
			// 	case UnitType.Bad when target == UnitType.Good || target == UnitType.Neutral:
			// 		return true;
			// 	case UnitType.Neutral when target == UnitType.Good || target == UnitType.Bad:
			// 		return true;
			// }
		}
	}
}