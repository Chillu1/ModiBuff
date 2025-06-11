using ModiBuff.Core;
using ModiBuff.Core.Units;

namespace ModiBuff.Tests
{
	internal static class UnitTestExtensions
	{
		private static void CheckForSetup(IUnit unit)
		{
			if (unit == null)
				Logger.LogError("Unit is null, you most likely forgot to call Setup() in your test");
		}

		internal static void AddModifierSelf(this IModifierOwner unit, string name)
		{
			CheckForSetup(unit);
			unit.ModifierController.Add(ModifierIdManager.GetIdByName(name).Value, unit, unit);
		}

		internal static void ApplyEffectSelf(this IUnit unit, string name)
		{
			CheckForSetup(unit);
			unit.ApplyEffect(EffectIdManager.GetIdOld(name).Value, unit);
		}

		internal static void AddModifierTarget(this IModifierOwner unit, string name, IUnit target)
		{
			CheckForSetup(unit);
			unit.ModifierController.Add(ModifierIdManager.GetIdByName(name).Value, target, unit);
		}

		internal static void ApplyEffectTarget(this IUnit unit, string name, IUnit target)
		{
			CheckForSetup(unit);
			target.ApplyEffect(EffectIdManager.GetIdOld(name).Value, unit);
		}

		internal static bool ContainsModifier(this IModifierOwner unit, string name)
		{
			return unit.ModifierController.Contains(ModifierIdManager.GetIdByName(name).Value);
		}

		internal static bool ContainsApplier(this IModifierApplierOwner unit, string name)
		{
			return unit.ModifierApplierController.ContainsApplier(ModifierIdManager.GetIdByName(name).Value);
		}

		internal static bool AddApplierModifier(this IModifierApplierOwner unit, IModifierGenerator generator,
			ApplierType applierType)
		{
			CheckForSetup(unit);
			return unit.ModifierApplierController.TryAddApplier(generator.Id,
				((IModifierApplyCheckGenerator)generator).HasApplyChecks, applierType);
		}

		internal static bool AddEffectApplier(this IModifierApplierOwner unit, string name)
		{
			CheckForSetup(unit);
			return unit.ModifierApplierController.TryAddEffectApplier(EffectIdManager.GetIdOld(name).Value);
		}

		internal static void TryCast(this Unit unit, string name, IModifierOwner target)
		{
			unit.TryCast(ModifierIdManager.GetIdByName(name).Value, target);
		}

		internal static void TryCast(this IModifierApplierOwner unit, string name, IModifierOwner target)
		{
			unit.TryCast(ModifierIdManager.GetIdByName(name).Value, target);
		}

		internal static void TryCastEffect(this IModifierApplierOwner unit, string name, IUnit target)
		{
			unit.TryCastEffect(EffectIdManager.GetIdOld(name).Value, target);
		}

		internal static void ChangeStatusEffect(this IStatusEffectOwner<LegalAction, StatusEffectType> owner,
			StatusEffectType statusEffect, float duration, IUnit source)
		{
			owner.StatusEffectController.ChangeStatusEffect(1000, 1000, statusEffect, duration, source);
		}

		internal static float AttackN(this IAttacker<float, float> unit, IUnit target, int n)
		{
			float totalDamage = 0;
			for (int i = 0; i < n; i++)
				totalDamage += unit.Attack(target);

			return totalDamage;
		}

		internal static float HealN(this IHealer<float, float> unit, IHealable<float, float> target, int n)
		{
			float totalHeal = 0;
			for (int i = 0; i < n; i++)
				totalHeal += unit.Heal(target);

			return totalHeal;
		}
	}
}