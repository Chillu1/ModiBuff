using System;
using System.Collections.Generic;

namespace ModiBuff.Core
{
	public sealed class ConditionCheck
	{
		private readonly ConditionType _conditionType;
		private readonly StatType _statType;
		private readonly float _statValue;
		private readonly ComparisonType _comparisonType;
		private readonly LegalAction _legalAction;
		private readonly StatusEffectType _statusEffect;
		private readonly string _modifierName; //For possible save/load feature
		private readonly int _modifierId = -1;
		private readonly Func<IUnit, bool> _checks;

		private const float DeltaTolerance = 0.01f;

		public ConditionCheck(ConditionType conditionType) : this(conditionType, StatType.None, -1f)
		{
		}

		public ConditionCheck(StatType statType, float value) : this(ConditionType.None, statType, value)
		{
		}

		public ConditionCheck(ConditionType conditionType, StatType statType, float statValue) :
			this(conditionType, statType, statValue, ComparisonType.GreaterOrEqual, LegalAction.None, StatusEffectType.None, null)
		{
		}

		public ConditionCheck(ConditionType conditionType, StatType statType, float statValue, ComparisonType comparisonType,
			LegalAction legalAction, StatusEffectType statusEffectType, string modifierName)
		{
			_conditionType = conditionType;
			_statType = statType;
			_statValue = statValue;
			_comparisonType = comparisonType;
			_legalAction = legalAction;
			_statusEffect = statusEffectType;
			_modifierName = modifierName;
			if (!string.IsNullOrEmpty(modifierName))
				_modifierId = ModifierIdManager.GetId(modifierName);

			/*var conditionList = new List<Func<IUnit, bool>>();

			switch (_conditionType)
			{
				case ConditionType.None:
					break;
				case ConditionType.HealthIsFull:
					conditionList.Add(unit => Math.Abs(unit.MaxHealth - unit.Health) < DeltaTolerance);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			switch (_statType)
			{
				case StatType.None:
					break;
				case StatType.Health:
					conditionList.Add(unit => unit.Health >= _statValue);
					break;
				case StatType.Mana:
					conditionList.Add(unit => unit.Mana >= _statValue);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			if (_statusEffect != StatusEffectType.None)
				conditionList.Add(unit => unit.HasStatusEffect(_statusEffect));

			if (_modifierId != -1)
				conditionList.Add(unit => unit.ContainsModifier(_modifierId));

			//TODO set all checks into one func, so we don't check for negatives every time. Bench
			//Current bench show that it's slower
			_checks = Delegate.Combine(conditionList.ToArray()) as Func<IUnit, bool>;*/
		}

		public bool Check(IUnit unit)
		{
			switch (_conditionType)
			{
				case ConditionType.None:
					break;
				case ConditionType.HealthIsFull:
					if (unit is IDamagable damagable)
					{
						if (!CheckValue(damagable.Health, damagable.MaxHealth, ComparisonType.Equal))
							return false;
					}
#if DEBUG && !MODIBUFF_PROFILE
					else
						throw new ArgumentException("Unit is not IDamagable");
#endif

					break;
				case ConditionType.ManaIsFull:
					if (!CheckValue(unit.Mana, unit.MaxMana, ComparisonType.Equal))
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
					if (unit is IDamagable damagable)
					{
						if (!CheckValue(damagable.Health, _statValue, _comparisonType))
							return false;
					}
#if DEBUG && !MODIBUFF_PROFILE
					else
						throw new ArgumentException("Unit is not IDamagable");
#endif

					break;
				case StatType.Mana:
					if (!CheckValue(unit.Mana, _statValue, _comparisonType))
						return false;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			if (_legalAction != LegalAction.None && !unit.HasLegalAction(_legalAction))
				return false;

			if (_statusEffect != StatusEffectType.None && !unit.HasStatusEffect(_statusEffect))
				return false;

			if (_modifierId != -1 && !unit.ModifierController.Contains(_modifierId))
				return false;

			return true;

			bool CheckValue(float unitValue, float value, ComparisonType comparisonType)
			{
				return comparisonType switch
				{
					ComparisonType.None => true,
					ComparisonType.Greater => unitValue > value,
					ComparisonType.Equal => Math.Abs(unitValue - value) < DeltaTolerance,
					ComparisonType.Less => unitValue < value,
					ComparisonType.GreaterOrEqual => unitValue >= value,
					ComparisonType.LessOrEqual => unitValue <= value,
					_ => throw new ArgumentOutOfRangeException()
				};
			}
		}
	}
}