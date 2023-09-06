using ModiBuff.Core;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class ModifierAddReferenceTests : ModifierTests
	{
		[Test]
		public void AddSelfModifier()
		{
			var generator = Recipes.GetGenerator("InitDamage");
			var modifierReference = new ModifierAddReference(generator);

			Unit.ModifierController.TryAdd(modifierReference);

			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void AddTargetModifier()
		{
			var generator = Recipes.GetGenerator("InitDamage");
			var modifierReference = new ModifierAddReference(generator);

			Unit.ModifierController.TryAdd(modifierReference, Enemy);

			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
		}

		[Test]
		public void AddTargetApplyAttackModifier()
		{
			var generator = Recipes.GetGenerator("InitDamage");
			var modifierReference = new ModifierAddReference(generator, ApplierType.Attack);

			Unit.ModifierController.TryAdd(modifierReference);

			Unit.Attack(Enemy);

			Assert.AreEqual(EnemyHealth - UnitDamage - 5, Enemy.Health);
		}

		[Test]
		public void AddSelfApplyAttackModifier()
		{
			var generator = Recipes.GetGenerator("InitDamageSelf");
			var modifierReference = new ModifierAddReference(generator, ApplierType.Attack);

			Unit.ModifierController.TryAdd(modifierReference);

			Unit.Attack(Enemy);

			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void AddTargetApplyCastModifier()
		{
			var generator = Recipes.GetGenerator("InitDamage");
			var modifierReference = new ModifierAddReference(generator, ApplierType.Cast);

			Unit.ModifierController.TryAdd(modifierReference);

			Unit.TryCast(generator.Id, Enemy);

			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
		}
	}
}