using System;

namespace ModiBuff.Core
{
	public sealed class ConditionCheck
	{
		private readonly StatType _statType;
		private readonly float _value;

		public ConditionCheck(StatType statType, float value)
		{
			_statType = statType;
			_value = value;
		}

		public bool Check(IUnit unit)
		{
			switch (_statType)
			{
				case StatType.Health:
					return unit.Health >= _value;
				case StatType.Mana:
					return unit.Mana >= _value;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}