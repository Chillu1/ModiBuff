using ModiBuff.Core;
using ModiBuff.Core.Units;

namespace ModiBuff.Tests
{
	internal static class UnitTestExtensions
	{
		internal static void AddModifierSelf(this IModifierOwner unit, string name)
		{
			unit.ModifierController.Add(ModifierIdManager.GetIdOld(name), unit, unit);
		}

		internal static void AddModifierTarget(this IModifierOwner unit, string name, IUnit target)
		{
			unit.ModifierController.Add(ModifierIdManager.GetIdOld(name), target, unit);
		}

		internal static bool ContainsModifier(this IModifierOwner unit, string name)
		{
			return unit.ModifierController.Contains(ModifierIdManager.GetIdOld(name));
		}

		internal static bool ContainsApplier(this IModifierOwner unit, string name)
		{
			return unit.ModifierController.ContainsApplier(ModifierIdManager.GetIdOld(name));
		}

		internal static bool AddApplierModifier(this IModifierOwner unit, IModifierGenerator generator,
			ApplierType applierType)
		{
			return unit.ModifierController.TryAddApplier(generator.Id,
				((IModifierApplyCheckGenerator)generator).HasApplyChecks, applierType);
		}

		internal static void TryCast(this IModifierOwner unit, string name, IModifierOwner target)
		{
			unit.TryCast(ModifierIdManager.GetIdOld(name), target);
		}

		internal static float AttackN(this IAttacker<float, float> unit, IUnit target, int n)
		{
			var updatableUnit = unit as IUpdatable;
			var updatableTarget = target as IUpdatable;
			float totalDamage = 0;
			for (int i = 0; i < n; i++)
			{
				updatableUnit?.Update(0);
				updatableTarget?.Update(0);
				totalDamage += unit.Attack(target);
			}

			return totalDamage;
		}

		internal static float HealN(this IHealer<float, float> unit, IHealable<float, float> target, int n)
		{
			var updatableUnit = unit as IUpdatable;
			var updatableTarget = target as IUpdatable;
			float totalHeal = 0;
			for (int i = 0; i < n; i++)
			{
				updatableUnit?.Update(0);
				updatableTarget?.Update(0);
				totalHeal += unit.Heal(target);
			}

			return totalHeal;
		}
	}
}