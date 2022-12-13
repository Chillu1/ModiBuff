using NUnit.Framework;

namespace ModifierLibraryLite.Tests
{
	public sealed class ApplierTests : BaseModifierTests
	{
		[Test]
		public void DamageApplier_Attack_Damage()
		{
			var applier = Recipes.GetRecipe("InitDamage");
			Unit.AddApplierModifiers(applier);

			Unit.Attack(Enemy);

			Assert.AreEqual(EnemyHealth - UnitDamage - 5, Enemy.Health);
		}

		[Test]
		public void HealApplier_Attack_Heal()
		{
			var applier = Recipes.GetRecipe("InitStrongHeal");
			Unit.AddApplierModifiers(applier);

			Unit.Attack(Enemy);

			Assert.AreEqual(EnemyHealth, Enemy.Health);
		}

		//[Test]
		public void DamageSelfApplier_Attack_DamageSelf()
		{
			//TODO
			var applier = Recipes.GetRecipe("InitDamageSelf");
			Unit.AddApplierModifiers(applier);

			Unit.Attack(Enemy);

			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}
		
		//No checks applier
	}
}