using ModiBuff.Core;
using ModiBuff.Core.Units;

namespace ModiBuff.Tests
{
	public sealed class BenchmarkModifierRecipes : ModifierRecipes
	{
		public BenchmarkModifierRecipes(ModifierIdManager idManager) : base(idManager)
		{
		}

		protected override void SetupRecipes()
		{
			Add("NoOpEffect")
				.Effect(new NoOpEffect(), EffectOn.Init);

			Add("InitDamage")
				.Effect(new DamageEffect(5), EffectOn.Init);

			Add("DoT")
				.Interval(1)
				.Effect(new DamageEffect(2), EffectOn.Interval)
				.Remove(5).Refresh();

			Add("InitDoTSeparateDamageRemove")
				.Interval(1)
				.Effect(new DamageEffect(10), EffectOn.Init)
				.Effect(new DamageEffect(5), EffectOn.Interval)
				.Remove(5);

			Add("InstanceStackableDoT")
				.InstanceStackable()
				.Interval(1)
				.Effect(new DamageEffect(5), EffectOn.Interval)
				.Remove(5);

			Add("StackAddDamage")
				.Effect(new DamageEffect(5, StackEffectType.Add), EffectOn.Stack)
				.Stack(WhenStackEffect.Always);

			Add("InitStackDamage")
				.Effect(new DamageEffect(5, StackEffectType.Effect), EffectOn.Init | EffectOn.Stack)
				.Stack(WhenStackEffect.Always);

			Add("InstanceStackableDoTNoRemove")
				.InstanceStackable()
				.Interval(1)
				.Effect(new DamageEffect(5), EffectOn.Interval);

			Add("IntervalDamage_StackAddDamage")
				.Effect(new DamageEffect(5, StackEffectType.Add), EffectOn.Interval | EffectOn.Stack)
				.Interval(1)
				.Stack(WhenStackEffect.Always, value: 2);

			Add("InitDamage_CostMana")
				.ApplyCost(CostType.Mana, 5)
				.Effect(new DamageEffect(5), EffectOn.Init);

			Add("InitDamage_ApplyCondition_HealthAbove100")
				.ApplyCondition(StatType.Health, 100, ComparisonType.GreaterOrEqual)
				.Effect(new DamageEffect(5), EffectOn.Init);
		}
	}
}