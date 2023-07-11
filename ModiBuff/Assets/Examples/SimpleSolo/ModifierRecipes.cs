using ModiBuff.Core;

namespace ModiBuff.Examples.SimpleSolo
{
	public sealed class ModifierRecipes : ModifierRecipesBase
	{
		protected override void SetupRecipes()
		{
			Add("InitDamage")
				.Effect(new DamageEffect(5), EffectOn.Init);

			Add("DoT")
				.Interval(1)
				.Effect(new DamageEffect(2), EffectOn.Interval)
				.Remove(5).Refresh();

			Add("FireSlimeSelfDoT")
				.Interval(1)
				.Effect(new DamageEffect(1), EffectOn.Interval);

			Add("DisarmChance")
				.ApplyChance(0.2f)
				.Effect(new StatusEffectEffect(StatusEffectType.Disarm, 1f), EffectOn.Init)
				.Remove(1f).Refresh();
		}
	}
}