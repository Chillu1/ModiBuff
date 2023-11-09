using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class CastTests : ModifierTests
	{
		[Test]
		public void CastInitDamageNoChecks_OnEnemy()
		{
			Setup();

			Unit.AddApplierModifier(Recipes.GetGenerator("InitDamage"), ApplierType.Cast);

			Unit.TryCast(IdManager.GetId("InitDamage"), Enemy);

			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
		}

		[Test]
		public void CastInitDamageChecks_OnEnemy()
		{
			AddRecipe("InitDamageFullHealth")
				.ApplyCondition(ConditionType.HealthIsFull)
				.Effect(new DamageEffect(5), EffectOn.Init);
			Setup();

			Unit.AddApplierModifier(Recipes.GetGenerator("InitDamageFullHealth"), ApplierType.Cast);

			Unit.TryCast(IdManager.GetId("InitDamageFullHealth"), Enemy);

			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);

			Unit.TakeDamage(5, Enemy);

			Unit.TryCast(IdManager.GetId("InitDamageFullHealth"), Enemy);

			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
		}

		[Test]
		public void AttackInitDamageNoChecks_OnEnemy()
		{
			Setup();

			Unit.AddApplierModifier(Recipes.GetGenerator("InitDamage"), ApplierType.Attack);

			Unit.Attack(Enemy);

			Assert.AreEqual(EnemyHealth - UnitDamage - 5, Enemy.Health);
		}

		[Test]
		public void AttackInitDamageChecks_OnEnemy()
		{
			AddRecipe("InitDamageFullHealth")
				.ApplyCondition(ConditionType.HealthIsFull)
				.Effect(new DamageEffect(5), EffectOn.Init);
			Setup();

			Unit.AddApplierModifier(Recipes.GetGenerator("InitDamageFullHealth"), ApplierType.Attack);

			Unit.Attack(Enemy);

			Assert.AreEqual(EnemyHealth - UnitDamage - 5, Enemy.Health);

			Unit.TakeDamage(5, Enemy);

			Unit.Attack(Enemy);

			Assert.AreEqual(EnemyHealth - UnitDamage * 2 - 5, Enemy.Health);
		}

		[Test]
		public void CastInitDamageChecksDelayedUse_OnEnemy()
		{
			AddRecipe("InitDamageFullHealth")
				.ApplyCondition(ConditionType.HealthIsFull)
				.Effect(new DamageEffect(5), EffectOn.Init);
			Setup();

			int id = IdManager.GetId("InitDamageFullHealth");

			Unit.AddApplierModifier(Recipes.GetGenerator("InitDamageFullHealth"), ApplierType.Cast);

			Assert.True(Unit.TryCastCheck(id));
			Assert.AreEqual(EnemyHealth, Enemy.Health);

			Assert.True(Unit.TryCastNoChecks(id, Enemy));

			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
		}
	}
}