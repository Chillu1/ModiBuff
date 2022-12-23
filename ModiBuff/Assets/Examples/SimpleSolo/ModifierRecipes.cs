using ModiBuff.Core;

namespace ModiBuff.Examples.SimpleSolo
{
	public sealed class ModifierRecipes : ModiBuff.Core.ModifierRecipes
	{
		protected override void SetupRecipes()
		{
			Add("InitDamage")
				.Effect(new DamageEffect(5), EffectOn.Init);

			Add("DoT")
				.Interval(1)
				.Effect(new DamageEffect(2), EffectOn.Interval)
				.Duration(5);
		}
	}
}