using ModiBuff.Core;
using NUnit.Framework;
using UnityEngine;

namespace ModiBuff.Tests
{
	public sealed class ApplierTests : BaseModifierTests
	{
		[Test]
		public void DamageApplier_Attack_Damage()
		{
			var applier = Recipes.GetRecipe("InitDamage");
			Unit.AddApplierModifier(applier, ApplierType.Attack);

			Unit.Attack(Enemy);

			Assert.AreEqual(EnemyHealth - UnitDamage - 5, Enemy.Health);
		}

		[Test]
		public void HealApplier_Attack_Heal()
		{
			var applier = Recipes.GetRecipe("InitStrongHeal");
			Unit.AddApplierModifier(applier, ApplierType.Attack);

			Unit.Attack(Enemy);

			Assert.AreEqual(EnemyHealth, Enemy.Health);
		}

		[Test]
		public void DamageSelfApplier_Attack_DamageSelf()
		{
			Unit.AddApplierModifier(Recipes.GetRecipe("InitDamageSelf"), ApplierType.Attack);
			Unit.AddApplierModifier(Recipes.GetRecipe("InitDamage"), ApplierType.Attack);

			Unit.Attack(Enemy);

			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void DamageApplier_Cast_Damage()
		{
			var applier = Recipes.GetRecipe("InitDamage");
			Unit.AddApplierModifier(applier, ApplierType.Cast);

			Unit.Cast(Enemy);

			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
		}

		[Test]
		public void DamageApplier_Interval()
		{
			Unit.TryAddModifierTarget("DamageApplier_Interval", Enemy);

			Unit.Update(1f);

			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);

			Unit.Update(1f);

			Assert.AreEqual(EnemyHealth - 10, Enemy.Health);
		}

		[Test]
		public void InitDamageCostMana()
		{
			Unit.AddApplierModifier(Recipes.GetRecipe("InitDamageCostMana"), ApplierType.Cast);

			Unit.Cast(Enemy);

			Assert.AreEqual(UnitMana - 5, Unit.Mana);
			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
		}
	}
}