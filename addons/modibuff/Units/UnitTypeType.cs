using System;
using System.Linq;

namespace ModiBuff.Core.Units
{
	public enum UnitTypeType //TODO rename
	{
		None,
		Player,
		BasicEnemy,
	}

	public static class UnitTypeTypeHelper
	{
		public static readonly UnitTypeType Last = Enum.GetValues(typeof(UnitTypeType)).Cast<UnitTypeType>().Last();
	}
}