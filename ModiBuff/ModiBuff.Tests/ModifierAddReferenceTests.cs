using ModiBuff.Core;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class ModifierAddReferenceTests : BaseModifierTests
	{
		[Test]
		public void AddSelfModifier()
		{
			var recipe = Recipes.GetRecipe("InitDamage");
			var modifierReference = new ModifierAddReference(recipe);

			Unit.TryAddModifier(modifierReference, Enemy);

			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void AddTargetModifier()
		{
			var recipe = Recipes.GetRecipe("InitDamageSelf");
			var modifierReference = new ModifierAddReference(recipe);

			Unit.TryAddModifier(modifierReference, Enemy);

			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
		}

		[Test]
		public void AddTargetApplyAttackModifier()
		{
			var recipe = Recipes.GetRecipe("InitDamage");
			var modifierReference = new ModifierAddReference(recipe, ApplierType.Attack);

			Unit.TryAddModifier(modifierReference, Unit);

			Unit.Attack(Enemy);

			Assert.AreEqual(EnemyHealth - UnitDamage - 5, Enemy.Health);
		}

		[Test]
		public void AddSelfApplyAttackModifier()
		{
			var recipe = Recipes.GetRecipe("InitDamageSelf");
			var modifierReference = new ModifierAddReference(recipe, ApplierType.Attack);

			Unit.TryAddModifier(modifierReference, Enemy);

			Unit.Attack(Enemy);

			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void AddTargetApplyCastModifier()
		{
			var recipe = Recipes.GetRecipe("InitDamage");
			var modifierReference = new ModifierAddReference(recipe, ApplierType.Cast);

			Unit.TryAddModifier(modifierReference, Unit);

			Unit.CastAll(Enemy);

			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
		}
	}
}