using System;
using System.Collections.Generic;

namespace ModiBuff.Core
{
	public sealed class ConditionCheck : ICheck
	{
		private readonly ConditionType _conditionType;
		private readonly StatType _statType;
		private readonly float _statValue;
		private readonly ComparisonType _comparisonType;
		private readonly LegalAction _legalAction;

		private readonly StatusEffectType _statusEffect;

		//private readonly string _modifierName; //For possible save/load feature
		private readonly int _modifierId = -1;
		private readonly Func<IUnit, bool> _checks;

		private const float DeltaTolerance = 0.01f;

		/*public ConditionCheck(ConditionType conditionType) : this(conditionType, StatType.None, -1f)
		{
		}

		public ConditionCheck(StatType statType, float value) : this(ConditionType.None, statType, value)
		{
		}

		public ConditionCheck(ConditionType conditionType, StatType statType, float statValue) :
			this(conditionType, statType, statValue, ComparisonType.GreaterOrEqual, LegalAction.None, StatusEffectType.None, -1)
		{
		}*/

		public ConditionCheck(ConditionType conditionType, StatType statType, float statValue, ComparisonType comparisonType,
			LegalAction legalAction, StatusEffectType statusEffectType, int modifierId)
		{
			_conditionType = conditionType;
			_statType = statType;
			_statValue = statValue;
			_comparisonType = comparisonType;
			_legalAction = legalAction;
			_statusEffect = statusEffectType;
			_modifierId = modifierId;

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
			if (!CheckConditionType(unit))
				return false;

			if (!CheckStatType(unit))
				return false;

			if (_legalAction != LegalAction.None)
			{
				if (unit is IStatusEffectOwner statusEffectOwner)
				{
					if (!statusEffectOwner.StatusEffectController.HasLegalAction(_legalAction))
						return false;
				}
#if DEBUG && !MODIBUFF_PROFILE
				else
					throw new ArgumentException("Unit is not IStatusEffectOwner");
#endif
			}

			if (_statusEffect != StatusEffectType.None)
			{
				if (unit is IStatusEffectOwner statusEffectOwner)
				{
					if (!statusEffectOwner.StatusEffectController.HasStatusEffect(_statusEffect))
						return false;
				}
#if DEBUG && !MODIBUFF_PROFILE
				else
					throw new ArgumentException("Unit is not IStatusEffectOwner");
#endif
			}

			if (_modifierId != -1 && !((IModifierOwner)unit).ModifierController.Contains(_modifierId))
				return false;

			return true;
		}

		private bool CheckConditionType(IUnit unit)
		{
			switch (_conditionType)
			{
				case ConditionType.None:
					break;
				case ConditionType.HealthIsFull:
					if (unit is IDamagable<float, float> damagable)
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
				{
					if (unit is IManaOwner manaOwner)
					{
						if (!CheckValue(manaOwner.Mana, manaOwner.MaxMana, ComparisonType.Equal))
							return false;
					}
#if DEBUG && !MODIBUFF_PROFILE
					else
						throw new ArgumentException("Unit is not IManaUser");
#endif

					break;
				}
				case ConditionType.ManaIsEmpty:
				{
					if (unit is IManaOwner manaOwner)
					{
						if (!CheckValue(manaOwner.Mana, 0f, ComparisonType.Equal))
							return false;
					}
#if DEBUG && !MODIBUFF_PROFILE
					else
						throw new ArgumentException("Unit is not IManaUser");
#endif
					break;
				}
				default:
					throw new ArgumentOutOfRangeException();
			}

			return true;
		}

		private bool CheckStatType(IUnit unit)
		{
			switch (_statType)
			{
				case StatType.None:
					break;
				case StatType.Health:
					if (unit is IDamagable<float, float> damagable)
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
					if (unit is IManaOwner manaUser)
					{
						if (!CheckValue(manaUser.Mana, _statValue, _comparisonType))
							return false;
					}
#if DEBUG && !MODIBUFF_PROFILE
					else
						throw new ArgumentException("Unit is not IManaUser");
#endif
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			return true;
		}

		private static bool CheckValue(float unitValue, float value, ComparisonType comparisonType)
		{
			switch (comparisonType)
			{
				case ComparisonType.None:
					return true;
				case ComparisonType.Greater:
					return unitValue > value;
				case ComparisonType.Equal:
					return Math.Abs(unitValue - value) < DeltaTolerance;
				case ComparisonType.Less:
					return unitValue < value;
				case ComparisonType.GreaterOrEqual:
					return unitValue >= value;
				case ComparisonType.LessOrEqual:
					return unitValue <= value;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}