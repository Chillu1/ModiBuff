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
			SetupSystems();

			var applier = Recipes.GetGenerator("InitDamage");
			Unit.AddApplierModifier(applier, ApplierType.Attack);

			Unit.Attack(Enemy);

			Assert.AreEqual(EnemyHealth - UnitDamage - 5, Enemy.Health);
		}

		[Test]
		public void HealApplier_Attack_Heal()
		{
			AddRecipes(add => add("InitStrongHeal")
				.Effect(new HealEffect(10), EffectOn.Init));

			var applier = Recipes.GetGenerator("InitStrongHeal");
			Unit.AddApplierModifier(applier, ApplierType.Attack);

			Unit.Attack(Enemy);

			Assert.AreEqual(EnemyHealth, Enemy.Health);
		}

		[Test]
		public void DamageSelfApplier_Attack_DamageSelf()
		{
			AddRecipes(add => add("InitDamageSelf")
				.Effect(new DamageEffect(5), EffectOn.Init, Targeting.SourceTarget)
			);

			Unit.AddApplierModifier(Recipes.GetGenerator("InitDamageSelf"), ApplierType.Attack);
			Unit.AddApplierModifier(Recipes.GetGenerator("InitDamage"), ApplierType.Attack);

			Unit.Attack(Enemy);

			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void DamageApplier_Cast_Damage()
		{
			SetupSystems();

			var applier = Recipes.GetGenerator("InitDamage");
			Unit.AddApplierModifier(applier, ApplierType.Cast);

			ModifierOwnerExtensions.TryCast(Unit, applier.Id, Enemy);

			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
		}

		[Test]
		public void DamageApplier_Interval()
		{
			AddRecipes(add => add("DamageApplier_Interval")
				.Effect(new ApplierEffect("InitDamage"), EffectOn.Interval)
				.Interval(1));

			Unit.AddModifierTarget("DamageApplier_Interval", Enemy);

			Unit.Update(1f);

			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);

			Unit.Update(1f);

			Assert.AreEqual(EnemyHealth - 10, Enemy.Health);
		}

		[Test]
		public void InitDamageCostMana()
		{
			AddRecipes(add => add("InitDamage_CostMana")
				.ApplyCost(CostType.Mana, 5)
				.Effect(new DamageEffect(5), EffectOn.Init));

			var generator = Recipes.GetGenerator("InitDamage_CostMana");

			Unit.AddApplierModifier(generator, ApplierType.Cast);

			ModifierOwnerExtensions.TryCast(Unit, generator.Id, Enemy);

			Assert.AreEqual(UnitMana - 5, Unit.Mana);
			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
		}

		[Test]
		public void NestedStackApplier()
		{
			Assert.Fail("Combination of Event & Non-Event");

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
			Assert.Fail("Combination of Event & Non-Event");

			//Add damage on 4 stacks buff, that you give someone when they heal you 5 times, for 60 seconds.
			Ally.AddModifierSelf("ComplexApplier2_WhenHealed_Event");

			Unit.HealN(Ally, 5);

			Unit.AttackN(Enemy, 4);

			Assert.AreEqual(UnitDamage + 5, Unit.Damage);
		}

		[Test]
		public void ModifierDoesntExist()
		{
			SetupSystems();

			Assert.Catch<KeyNotFoundException>(() => Recipes.GetGenerator("NonExistentApplier"));
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
			SetupSystems();

			var testLogger = new TestLogger();
			Logger.SetLogger(testLogger);

			var applier = new ApplierEffect("NonExistentApplier");
			Assert.True(testLogger.ErrorLogged);

			Logger.SetLogger<NUnitLogger>();
		}

		[Test]
		public void ApplyNewModifierOnIteration() //Checks that our collection is not modified during iteration
		{
			AddRecipes(
				add => add("AddModifierApplier_Flag"),
				add => add("AddModifierApplierInterval")
					.Effect(new ApplierEffect("AddModifierApplier_Flag"), EffectOn.Interval)
					.Interval(1));


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