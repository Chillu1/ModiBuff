using ModiBuff.Core;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class PostEffectTests : ModifierTests
	{
		[Test]
		public void LifeSteal_OnDamageEffectInit()
		{
			var recipe = Recipes.GetRecipe("InitDamageLifeStealPost");
			Unit.AddApplierModifier(recipe, ApplierType.Cast);

			Unit.TakeDamage(2.5f, Unit);

			Unit.TryCast(recipe.Id, Enemy);

			Assert.AreEqual(UnitHealth, Unit.Health);
			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
		}

		[Test]
		public void AddDamage_OnKill_WithDamageEffectInit()
		{
			var recipe = Recipes.GetRecipe("InitDamageAddDamageOnKillPost");
			Unit.AddApplierModifier(recipe, ApplierType.Cast);

			Enemy.TakeDamage(EnemyHealth - 5, Unit);

			Unit.TryCast(recipe.Id, Enemy);

			Assert.AreEqual(UnitDamage + 2, Unit.Damage);
			Assert.AreEqual(0, Enemy.Health);
		}
	}
}