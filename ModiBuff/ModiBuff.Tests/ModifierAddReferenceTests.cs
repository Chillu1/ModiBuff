using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class ModifierAddReferenceTests : ModifierTests
	{
		[Test]
		public void AddSelfModifier()
		{
			Setup();

			var generator = Recipes.GetGenerator("InitDamage");
			var modifierReference = new ModifierAddReference(generator);

			Unit.TryAddModifier(modifierReference);

			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void AddTargetModifier()
		{
			Setup();

			var generator = Recipes.GetGenerator("InitDamage");
			var modifierReference = new ModifierAddReference(generator);

			Unit.TryAddModifier(modifierReference, Enemy);

			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
		}

		[Test]
		public void AddTargetApplyAttackModifier()
		{
			Setup();

			var generator = Recipes.GetGenerator("InitDamage");
			var modifierReference = new ModifierAddReference(generator, ApplierType.Attack);

			Unit.TryAddModifier(modifierReference);

			Unit.Attack(Enemy);

			Assert.AreEqual(EnemyHealth - UnitDamage - 5, Enemy.Health);
		}

		[Test]
		public void AddSelfApplyAttackModifier()
		{
			AddRecipe("InitDamageSelf")
				.Effect(new DamageEffect(5, targeting: Targeting.SourceTarget), EffectOn.Init);
			Setup();

			var generator = Recipes.GetGenerator("InitDamageSelf");
			var modifierReference = new ModifierAddReference(generator, ApplierType.Attack);

			Unit.TryAddModifier(modifierReference);

			Unit.Attack(Enemy);

			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void AddTargetApplyCastModifier()
		{
			Setup();

			var generator = Recipes.GetGenerator("InitDamage");
			var modifierReference = new ModifierAddReference(generator, ApplierType.Cast);

			Unit.TryAddModifier(modifierReference);

			Unit.TryCast(generator.Id, Enemy);

			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
		}
	}
}