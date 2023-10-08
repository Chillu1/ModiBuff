using ModiBuff.Core;
using ModiBuff.Core.Units;
using TagType = ModiBuff.Core.TagType;

namespace ModiBuff.Tests
{
	public sealed class BenchmarkModifierRecipes : ModifierRecipes
	{
		public BenchmarkModifierRecipes(ModifierIdManager idManager) : base(idManager)
		{
			CreateGenerators();
		}

		protected override void SetupRecipes()
		{
			Add("NoOpEffect")
				.Effect(new NoOpEffect(), EffectOn.Init);

			Add("InitDamage")
				.Effect(new DamageEffect(5), EffectOn.Init);

			Add("InitDamageManual", (id, genId, name) =>
			{
				var initComponent = new InitComponent(false, new IEffect[] { new DamageEffect(5) }, null);

				var modifier = new Modifier(id, genId, name, initComponent, null, null, null,
					new SingleTargetComponent(), null);

				return modifier;
			}, TagType.IsInit);

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

			Add("InstanceStackableInitDamage")
				.InstanceStackable()
				.Effect(new DamageEffect(5), EffectOn.Init);

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