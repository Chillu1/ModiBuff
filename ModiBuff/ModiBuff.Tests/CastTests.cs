using ModiBuff.Core;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class CastTests : BaseModifierTests
	{
		[Test]
		public void CastInitDamageNoChecks_OnEnemy()
		{
			Unit.AddApplierModifier(Recipes.GetRecipe("InitDamage"), ApplierType.Cast);

			Unit.TryCast(IdManager.GetId("InitDamage"), Enemy);

			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
		}

		[Test]
		public void CastInitDamageChecks_OnEnemy()
		{
			Unit.AddApplierModifier(Recipes.GetRecipe("InitDamageFullHealth"), ApplierType.Cast);

			Unit.TryCast(IdManager.GetId("InitDamageFullHealth"), Enemy);

			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);

			Unit.TakeDamage(5, Enemy);

			Unit.TryCast(IdManager.GetId("InitDamageFullHealth"), Enemy);

			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
		}

		[Test]
		public void AttackInitDamageNoChecks_OnEnemy()
		{
			Unit.AddApplierModifier(Recipes.GetRecipe("InitDamage"), ApplierType.Attack);

			Unit.Attack(Enemy);

			Assert.AreEqual(EnemyHealth - UnitDamage - 5, Enemy.Health);
		}

		[Test]
		public void AttackInitDamageChecks_OnEnemy()
		{
			Unit.AddApplierModifier(Recipes.GetRecipe("InitDamageFullHealth"), ApplierType.Attack);

			Unit.Attack(Enemy);

			Assert.AreEqual(EnemyHealth - UnitDamage - 5, Enemy.Health);

			Unit.TakeDamage(5, Enemy);

			Unit.Attack(Enemy);

			Assert.AreEqual(EnemyHealth - UnitDamage * 2 - 5, Enemy.Health);
		}
	}
}