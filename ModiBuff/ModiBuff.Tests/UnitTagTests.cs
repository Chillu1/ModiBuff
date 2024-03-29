using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class UnitTagTests : ModifierTests
	{
		[Test]
		public void LifestealableUnit()
		{
			AddRecipe("InitDamageLifesteal")
				.Effect(new DamageEffect(5)
					.SetPostEffects(new LifeStealPostEffect(1f, Targeting.SourceTarget)), EffectOn.Init);
			Setup();

			Unit.AddApplierModifier(Recipes.GetGenerator("InitDamageLifesteal"), ApplierType.Cast);

			Unit.TakeDamage(5, Unit);
			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Unit.TryCast("InitDamageLifesteal", Enemy);
			Assert.AreEqual(UnitHealth, Unit.Health);
		}

		[Test]
		public void NonLifestealableUnit()
		{
			AddRecipe("InitDamageLifesteal")
				.Effect(new DamageEffect(5).SetPostEffects(new LifeStealPostEffect(1f, Targeting.SourceTarget)),
					EffectOn.Init);
			Setup();
			var nonLifeStealableUnit = new Unit(unitTag: UnitTag.None);

			Unit.AddApplierModifier(Recipes.GetGenerator("InitDamageLifesteal"), ApplierType.Cast);

			Unit.TakeDamage(UnitDamage, Unit);
			Assert.AreEqual(UnitHealth - UnitDamage, Unit.Health);

			Unit.TryCast("InitDamageLifesteal", nonLifeStealableUnit);
			Assert.AreEqual(UnitHealth - UnitDamage, Unit.Health);
		}

		[Test]
		public void LifestealableUnit_NonLifestealableSource()
		{
			AddRecipe("InitDamageLifesteal")
				.Effect(new DamageEffect(5).SetPostEffects(new LifeStealPostEffect(1f, Targeting.SourceTarget)),
					EffectOn.Init);
			Setup();

			const float health = 100f, damage = 5f;
			var nonLifeStealableUnit = new Unit(health, damage, unitTag: UnitTag.None);

			nonLifeStealableUnit.AddApplierModifier(Recipes.GetGenerator("InitDamageLifesteal"), ApplierType.Cast);

			nonLifeStealableUnit.TakeDamage(damage, nonLifeStealableUnit);
			Assert.AreEqual(health - damage, nonLifeStealableUnit.Health);

			nonLifeStealableUnit.TryCast("InitDamageLifesteal", Unit);
			Assert.AreEqual(health, nonLifeStealableUnit.Health);
		}
	}
}