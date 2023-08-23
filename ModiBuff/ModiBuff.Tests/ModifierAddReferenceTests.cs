using ModiBuff.Core;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class ModifierAddReferenceTests : ModifierTests
	{
		//[Test]//TODO Refactor
		public void AddSelfModifier()
		{
			var recipe = Recipes.GetRecipe("InitDamage");
			var modifierReference = new ModifierAddReference(recipe);

			Unit.ModifierController.TryAdd(modifierReference);

			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		//[Test]//TODO Refactor
		public void AddTargetModifier()
		{
			var recipe = Recipes.GetRecipe("InitDamageSelf");
			var modifierReference = new ModifierAddReference(recipe);

			Unit.ModifierController.TryAdd(modifierReference);

			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
		}

		[Test]
		public void AddTargetApplyAttackModifier()
		{
			var recipe = Recipes.GetRecipe("InitDamage");
			var modifierReference = new ModifierAddReference(recipe, ApplierType.Attack);

			Unit.ModifierController.TryAdd(modifierReference);

			Unit.Attack(Enemy);

			Assert.AreEqual(EnemyHealth - UnitDamage - 5, Enemy.Health);
		}

		[Test]
		public void AddSelfApplyAttackModifier()
		{
			var recipe = Recipes.GetRecipe("InitDamageSelf");
			var modifierReference = new ModifierAddReference(recipe, ApplierType.Attack);

			Unit.ModifierController.TryAdd(modifierReference);

			Unit.Attack(Enemy);

			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void AddTargetApplyCastModifier()
		{
			var recipe = Recipes.GetRecipe("InitDamage");
			var modifierReference = new ModifierAddReference(recipe, ApplierType.Cast);

			Unit.ModifierController.TryAdd(modifierReference);

			Unit.TryCast(recipe.Id, Enemy);

			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
		}
	}
}