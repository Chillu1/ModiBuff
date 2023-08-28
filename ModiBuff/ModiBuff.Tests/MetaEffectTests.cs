using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class MetaEffectTests : ModifierTests
	{
		[Test]
		public void DamageBasedOnHealth()
		{
			var recipe = Recipes.GetRecipe("InitDamageValueBasedOnStatMeta");
			Unit.AddApplierModifier(recipe, ApplierType.Cast);

			Unit.TryCast(recipe.Id, Enemy); //5 * 1

			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);

			Unit.TakeDamage(UnitHealth / 2f, Unit);

			Unit.TryCast(recipe.Id, Enemy); //5 * 0.5

			Assert.AreEqual(EnemyHealth - 5 - 2.5f, Enemy.Health);
		}

		[Test]
		public void DamageBasedOnHealthAndMana()
		{
			var recipe = Recipes.GetRecipe("InitDamageValueBasedOnHealthAndManaMeta");
			Unit.AddApplierModifier(recipe, ApplierType.Cast);

			Unit.TryCast(recipe.Id, Enemy); //5 * 1

			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);

			Unit.TakeDamage(UnitHealth / 2f, Unit);
			Unit.UseMana(UnitMana / 2f);

			Unit.TryCast(recipe.Id, Enemy); //5 * 0.5 * 0.5

			Assert.AreEqual(EnemyHealth - 5 - 1.25f, Enemy.Health);
		}

		[Test]
		public void CanCastHalfMulti_IsStunnedDoubleMulti()
		{
			var recipe = Recipes.GetRecipe("InitDamageValueBasedOnStatusEffectMeta");
			Unit.AddApplierModifier(recipe, ApplierType.Cast);

			Unit.TryCast(recipe.Id, Enemy);
			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);

			Enemy.StatusEffectController.ChangeStatusEffect(StatusEffectType.Disarm, 1f);

			Unit.TryCast(recipe.Id, Enemy); //5 * 2f
			Assert.AreEqual(EnemyHealth - 5 - 10f, Enemy.Health);

			Enemy.Update(1f);
			Enemy.StatusEffectController.ChangeStatusEffect(StatusEffectType.Silence, 1f);

			Unit.TryCast(recipe.Id, Enemy); //5 * 0.5f
			Assert.AreEqual(EnemyHealth - 5 - 10f - 2.5f, Enemy.Health);
		}

		[Test]
		public void DoubleMultiplierWhenSilenced()
		{
			var recipe = Recipes.GetRecipe("InitDamageValue2XWhenDisarmedMeta");
			Unit.AddApplierModifier(recipe, ApplierType.Cast);

			Unit.TryCast(recipe.Id, Enemy);
			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);

			Unit.StatusEffectController.ChangeStatusEffect(StatusEffectType.Disarm, 1f);

			Unit.TryCast(recipe.Id, Enemy); //5 * 2f
			Assert.AreEqual(EnemyHealth - 5 - 10f, Enemy.Health);
		}
	}
}