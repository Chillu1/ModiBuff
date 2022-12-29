using System;

namespace ModiBuff.Core
{
	public sealed class ConditionCheck
	{
		private readonly ConditionType _conditionType;
		private readonly StatType _statType;
		private readonly float _value;

		private const float DeltaTolerance = 0.01f;

		public ConditionCheck(ConditionType conditionType) : this(conditionType, StatType.None, -1f)
		{
		}

		public ConditionCheck(StatType statType, float value) : this(ConditionType.None, statType, value)
		{
		}

		public ConditionCheck(ConditionType conditionType, StatType statType, float value)
		{
			_conditionType = conditionType;
			_statType = statType;
			_value = value;
		}

		public bool Check(IUnit unit)
		{
			switch (_conditionType)
			{
				case ConditionType.None:
					break;
				case ConditionType.HealthIsFull:
					if (Math.Abs(unit.Health - unit.MaxHealth) > DeltaTolerance)
						return false;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			switch (_statType)
			{
				case StatType.None:
					break;
				case StatType.Health:
					return unit.Health >= _value;
				case StatType.Mana:
					return unit.Mana >= _value;
				default:
					throw new ArgumentOutOfRangeException();
			}

			return true;
		}
	}
}