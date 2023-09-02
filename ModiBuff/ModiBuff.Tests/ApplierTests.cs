using System.Collections.Generic;
using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class ApplierTests : ModifierTests
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

			ModifierOwnerExtensions.TryCast(Unit, applier.Id, Enemy);

			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
		}

		[Test]
		public void DamageApplier_Interval()
		{
			Unit.AddModifierTarget("DamageApplier_Interval", Enemy);

			Unit.Update(1f);

			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);

			Unit.Update(1f);

			Assert.AreEqual(EnemyHealth - 10, Enemy.Health);
		}

		[Test]
		public void InitDamageCostMana()
		{
			var recipe = Recipes.GetRecipe("InitDamage_CostMana");

			Unit.AddApplierModifier(recipe, ApplierType.Cast);

			ModifierOwnerExtensions.TryCast(Unit, recipe.Id, Enemy);

			Assert.AreEqual(UnitMana - 5, Unit.Mana);
			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
		}

		[Test]
		public void NestedStackApplier()
		{
			Unit.AddModifierSelf("ComplexApplier_OnHit_Event");

			Enemy.Attack(Unit); //Gets rupture modifier

			Enemy.Update(1f); //Rupture modifier interval ticks
			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);

			Enemy.AttackN(Unit, 9); //Gets 9 more stacks

			Assert.True(Enemy.StatusEffectController.HasStatusEffect(StatusEffectType.Disarm));

			Enemy.Update(1f); //Rupture modifier interval ticks
			Assert.AreEqual(EnemyHealth - 5 - 5, Enemy.Health);

			Enemy.Update(4f);
			Assert.False(Enemy.StatusEffectController.HasStatusEffect(StatusEffectType.Disarm));

			Enemy.Update(5f);
			Enemy.AttackN(Unit, 5);

			//Only 1 stack of Disarm
			Assert.False(Enemy.StatusEffectController.HasStatusEffect(StatusEffectType.Disarm));
		}

		[Test]
		public void AddDamageStacksEventsAppliers()
		{
			//Add damage on 4 stacks buff, that you give someone when they heal you 5 times, for 60 seconds.
			Ally.AddModifierSelf("ComplexApplier2_WhenHealed_Event");

			Unit.HealN(Ally, 5);

			Unit.AttackN(Enemy, 4);

			Assert.AreEqual(UnitDamage + 5, Unit.Damage);
		}

		[Test]
		public void ModifierDoesntExist()
		{
			Assert.Catch<KeyNotFoundException>(() => Recipes.GetRecipe("NonExistentApplier"));
		}

		private sealed class TestLogger : ILogger
		{
			public bool ErrorLogged;

			public void Log(string message)
			{
			}

			public void LogWarning(string message)
			{
			}

			public void LogError(string message)
			{
				ErrorLogged = true;
			}
		}

		[Test]
		public void ApplierDoesntExist()
		{
#if !DEBUG
			Assert.Ignore("This test is only for debug mode");
#endif

			var testLogger = new TestLogger();
			Logger.SetLogger(testLogger);

			var applier = new ApplierEffect("NonExistentApplier");
			Assert.True(testLogger.ErrorLogged);

			Logger.SetLogger<NUnitLogger>();
		}

		[Test]
		public void ApplyNewModifierOnIteration() //Checks that our collection is not modified during iteration
		{
			Config.ModifierArraySize = 1;
			var unit = new Unit();

			unit.AddModifierSelf("AddModifierApplierInterval");

			Assert.False(unit.ContainsModifier("AddModifierApplier_Flag"));

			unit.Update(1); //Adding modifier, forced resize

			Assert.True(unit.ContainsModifier("AddModifierApplier_Flag"));

			Config.Reset();
		}
	}
}