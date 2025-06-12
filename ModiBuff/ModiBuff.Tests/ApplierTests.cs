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
			Setup();

			var applier = Recipes.GetGenerator("InitDamage");
			Unit.AddApplierModifier(applier, ApplierType.Attack);

			Unit.Attack(Enemy);

			Assert.AreEqual(EnemyHealth - UnitDamage - 5, Enemy.Health);
		}

		[Test]
		public void HealApplier_Attack_Heal()
		{
			AddRecipe("InitStrongHeal")
				.Effect(new HealEffect(10), EffectOn.Init);
			Setup();

			var applier = Recipes.GetGenerator("InitStrongHeal");
			Unit.AddApplierModifier(applier, ApplierType.Attack);

			Enemy.TakeDamage(10, Enemy);
			Unit.Attack(Enemy); //Heal appliers triggers first, then attack damage

			Assert.AreEqual(EnemyHealth - 10, Enemy.Health);
		}

		[Test]
		public void DamageSelfApplier_Attack_DamageSelf()
		{
			AddRecipe("InitDamageSelf")
				.Effect(new DamageEffect(5, targeting: Targeting.SourceTarget), EffectOn.Init);
			Setup();

			Unit.AddApplierModifier(Recipes.GetGenerator("InitDamageSelf"), ApplierType.Attack);
			Unit.AddApplierModifier(Recipes.GetGenerator("InitDamage"), ApplierType.Attack);

			Unit.Attack(Enemy);

			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void DamageApplier_Cast_Damage()
		{
			Setup();

			var applier = Recipes.GetGenerator("InitDamage");
			Unit.AddApplierModifier(applier, ApplierType.Cast);

			Unit.TryCast(applier.Id, Enemy);

			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
		}

		[Test]
		public void DamageApplier_Interval()
		{
			AddRecipe("DamageApplier_Interval")
				.Effect(new ApplierEffect("InitDamage"), EffectOn.Interval)
				.Interval(1);
			Setup();

			Unit.AddModifierTarget("DamageApplier_Interval", Enemy);

			Unit.Update(1f);

			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);

			Unit.Update(1f);

			Assert.AreEqual(EnemyHealth - 10, Enemy.Health);
		}

		[Test]
		public void InitDamageCostMana()
		{
			AddRecipe("InitDamage_CostMana")
				.ApplyCost(CostType.Mana, 5)
				.Effect(new DamageEffect(5), EffectOn.Init);
			Setup();

			var generator = Recipes.GetGenerator("InitDamage_CostMana");

			Unit.AddApplierModifier(generator, ApplierType.Cast);

			Unit.TryCast(generator.Id, Enemy);

			Assert.AreEqual(UnitMana - 5, Unit.Mana);
			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
		}

		[Test]
		public void NestedStackApplier()
		{
			AddRecipe("ComplexApplier_Disarm")
				.Effect(new StatusEffectEffect(StatusEffectType.Disarm, 5, false, StackEffectType.Effect),
					EffectOn.Stack)
				.Stack(WhenStackEffect.EveryXStacks, everyXStacks: 2)
				.Remove(10).Refresh();
			AddRecipe("ComplexApplier_Rupture")
				.Interval(1)
				.Effect(new DamageEffect(5), EffectOn.Interval)
				.Effect(new ApplierEffect("ComplexApplier_Disarm"), EffectOn.Stack)
				.Stack(WhenStackEffect.EveryXStacks, everyXStacks: 5);
			AddRecipe("ComplexApplier_OnHit_Event")
				.Effect(new ApplierEffect("ComplexApplier_Rupture", targeting: Targeting.SourceTarget),
					EffectOn.CallbackUnit)
				.CallbackUnit(CallbackUnitType.WhenAttacked);
			Setup();

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
			AddRecipe("ComplexApplier2_AddDamage")
				.OneTimeInit()
				.Effect(new AddDamageEffect(5, EffectState.IsRevertible), EffectOn.Init)
				.Remove(10).Refresh();
			AddRecipe("ComplexApplier2_AddDamageAdd")
				.Effect(new ApplierEffect("ComplexApplier2_AddDamage"), EffectOn.Stack)
				.Stack(WhenStackEffect.EveryXStacks, everyXStacks: 4)
				.Remove(5).Refresh();
			AddRecipe("ComplexApplier2_WhenAttacked_Event")
				.Effect(new ApplierEffect("ComplexApplier2_AddDamageAdd", targeting: Targeting.SourceTarget),
					EffectOn.CallbackUnit)
				.CallbackUnit(CallbackUnitType.WhenAttacked)
				.Remove(5).Refresh();
			AddRecipe("ComplexApplier2_OnAttack_Event")
				.Effect(new ApplierEffect("ComplexApplier2_WhenAttacked_Event"), EffectOn.CallbackUnit)
				.CallbackUnit(CallbackUnitType.OnAttack)
				.Remove(60).Refresh();
			AddRecipe("ComplexApplier2_WhenHealed")
				.Effect(new ApplierEffect("ComplexApplier2_OnAttack_Event"), EffectOn.Stack)
				.Stack(WhenStackEffect.EveryXStacks, everyXStacks: 5)
				.Remove(5).Refresh();
			AddRecipe("ComplexApplier2_WhenHealed_Event")
				.Effect(new ApplierEffect("ComplexApplier2_WhenHealed", targeting: Targeting.SourceTarget),
					EffectOn.CallbackUnit)
				.CallbackUnit(CallbackUnitType.WhenHealed);
			Setup();

			//Add damage on 4 stacks buff, that you give someone when they heal you 5 times, for 60 seconds.
			Ally.AddModifierSelf("ComplexApplier2_WhenHealed_Event");

			Unit.HealN(Ally, 5);

			Unit.AttackN(Enemy, 4);

			Assert.AreEqual(UnitDamage + 5, Unit.Damage);
		}

		[Test]
		public void ModifierDoesntExist()
		{
			Setup();

			Assert.Catch<KeyNotFoundException>(() => Recipes.GetGenerator("NonExistentApplier"));
		}

		[Test]
		public void ApplierDoesntExist()
		{
			Setup();

			Assert.Throws<AssertionException>(() => _ = new ApplierEffect("NonExistentApplier"));
		}

		[Test]
		public void ApplyNewModifierOnIteration() //Checks that our collection is not modified during iteration
		{
			AddRecipe("AddModifierApplier_Flag");
			AddRecipe("AddModifierApplierInterval")
				.Effect(new ApplierEffect("AddModifierApplier_Flag"), EffectOn.Interval)
				.Interval(1);
			Setup();


			Config.ModifierArraySize = 1;
			var unit = new Unit();

			unit.AddModifierSelf("AddModifierApplierInterval");

			Assert.False(unit.ContainsModifier("AddModifierApplier_Flag"));

			unit.Update(1); //Adding modifier, forced resize

			Assert.True(unit.ContainsModifier("AddModifierApplier_Flag"));

			Config.ModifierArraySize = Config.DefaultModifierArraySize;
		}

		[Test]
		public void Cast_AddApplier()
		{
			AddRecipe("AddApplier_Effect")
				.Effect(new ApplierEffect("InitDamage"), EffectOn.Init)
				.RemoveApplier(5, ApplierType.Cast, false);
			AddEffect("AddApplier_ApplierEffect", new ApplierEffect("AddApplier_Effect", ApplierType.Cast, false));
			Setup();

			Unit.AddEffectApplier("AddApplier_ApplierEffect");
			Unit.TryCast("AddApplier_Effect", Enemy);
			Unit.TryCastEffect("AddApplier_ApplierEffect", Unit);

			Unit.TryCast("AddApplier_Effect", Enemy);
			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);

			Unit.Update(5);
			Assert.False(Unit.ContainsApplier("AddApplier_Effect"));
			Assert.False(Unit.ContainsModifier("AddApplier_Effect"));
		}

		[Test]
		public void ConditionalApplierBasedOnUnitType()
		{
			AddRecipe("InitHeal")
				.Effect(new HealEffect(5), EffectOn.Init);
			AddRecipe("ConditionalApplierBasedOnUnitType")
				.Effect(new ApplierEffect("InitDamage").SetMetaEffects(new ModifierIdBasedOnUnitTypeMetaEffect(
						new Dictionary<UnitType, int>()
						{
							{ UnitType.Bad, IdManager.GetId("InitDamage").Value },
							{ UnitType.Good, IdManager.GetId("InitHeal").Value }
						})),
					EffectOn.Init)
				.Remove(5).Refresh();
			Setup();

			Unit.AddApplierModifier(Recipes.GetGenerator("ConditionalApplierBasedOnUnitType"), ApplierType.Cast);

			Enemy.TakeDamage(5, Enemy);
			Ally.TakeDamage(5, Ally);

			Unit.TryCast("ConditionalApplierBasedOnUnitType", Enemy);
			Assert.AreEqual(EnemyHealth - 5 - 5, Enemy.Health);
			Unit.TryCast("ConditionalApplierBasedOnUnitType", Ally);
			Assert.AreEqual(AllyHealth - 5 + 5, Ally.Health);
		}
	}
}