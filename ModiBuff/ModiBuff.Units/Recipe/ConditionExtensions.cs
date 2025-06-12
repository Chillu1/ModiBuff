using System;

namespace ModiBuff.Core.Units
{
	public static class ConditionExtensions
	{
		public static bool CheckLegalAction(this LegalAction legalAction, IUnit unit)
		{
			if (legalAction != LegalAction.None)
			{
				if (unit is IStatusEffectOwner<LegalAction, StatusEffectType> statusEffectOwner)
				{
					if (!statusEffectOwner.StatusEffectController.HasLegalAction(legalAction))
						return false;
				}
#if DEBUG && !MODIBUFF_PROFILE
				else
					throw new ArgumentException("Unit is not IStatusEffectOwner");
#endif
			}

			return true;
		}

		public static bool CheckStatusEffectType(this StatusEffectType statusEffectType, IUnit unit)
		{
			if (statusEffectType != StatusEffectType.None)
			{
				if (unit is IStatusEffectOwner<LegalAction, StatusEffectType> statusEffectOwner)
				{
					if (!statusEffectOwner.StatusEffectController.HasStatusEffect(statusEffectType))
						return false;
				}
#if DEBUG && !MODIBUFF_PROFILE
				else
					throw new ArgumentException("Unit is not IStatusEffectOwner");
#endif
			}

			return true;
		}

		public static bool CheckModifierId(this int? modifierId, IUnit unit) =>
			modifierId == null || ((IModifierOwner)unit).ModifierController.Contains(modifierId.Value);

		public static bool CheckConditionType(this ConditionType conditionType, IUnit unit)
		{
			switch (conditionType)
			{
				case ConditionType.None:
					break;
				case ConditionType.HealthIsFull:
					if (unit is IDamagable<float, float> damagable)
					{
						if (!ComparisonType.Equal.Check(damagable.Health, damagable.MaxHealth))
							return false;
					}
#if DEBUG && !MODIBUFF_PROFILE
					else
						throw new ArgumentException("Unit is not IDamagable");
#endif

					break;
				case ConditionType.ManaIsFull:
				{
					if (unit is IManaOwner<float, float> manaOwner)
					{
						if (!ComparisonType.Equal.Check(manaOwner.Mana, manaOwner.MaxMana))
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
					if (unit is IManaOwner<float, float> manaOwner)
					{
						if (!ComparisonType.Equal.Check(manaOwner.Mana, 0f))
							return false;
					}
#if DEBUG && !MODIBUFF_PROFILE
					else
						throw new ArgumentException("Unit is not IManaUser");
#endif
					break;
				}
				default:
					Logger.LogError("[ModiBuff.Units] Invalid condition type: " + conditionType);
					return false;
			}

			return true;
		}

		public static bool CheckStatType(this StatType statType, IUnit unit, ComparisonType comparisonType,
			float statValue)
		{
			switch (statType)
			{
				case StatType.None:
					break;
				case StatType.Health:
					if (unit is IDamagable<float, float> damagable)
					{
						if (!comparisonType.Check(damagable.Health, statValue))
							return false;
					}
#if DEBUG && !MODIBUFF_PROFILE
					else
						throw new ArgumentException("Unit is not IDamagable");
#endif

					break;
				case StatType.Mana:
					if (unit is IManaOwner<float, float> manaUser)
					{
						if (!comparisonType.Check(manaUser.Mana, statValue))
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
	}
}