using System;
using System.Collections.Generic;

namespace ModiBuff.Core
{
	public sealed class ConditionCheck
	{
		private readonly ConditionType _conditionType;
		private readonly StatType _statType;
		private readonly float _statValue;
		private readonly LegalAction _legalAction;
		private readonly StatusEffectType _statusEffect;
		private readonly string _modifierName;
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
			this(conditionType, statType, statValue, LegalAction.None, StatusEffectType.None, null)
		{
		}

		public ConditionCheck(ConditionType conditionType, StatType statType, float statValue, LegalAction legalAction,
			StatusEffectType statusEffectType, string modifierName)
		{
			_conditionType = conditionType;
			_statType = statType;
			_statValue = statValue;
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
			//return _checks == null || _checks(unit);
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
					if (unit.Health < _statValue)
						return false;
					break;
				case StatType.Mana:
					if (unit.Mana < _statValue)
						return false;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			if (_legalAction != LegalAction.None && !unit.HasLegalAction(_legalAction))
				return false;

			if (_statusEffect != StatusEffectType.None && !unit.HasStatusEffect(_statusEffect))
				return false;

			if (_modifierId != -1 && !unit.ContainsModifier(_modifierId))
				return false;

			return true;
		}
	}
}