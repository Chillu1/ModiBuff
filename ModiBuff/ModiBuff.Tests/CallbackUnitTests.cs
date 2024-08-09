using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class CallbackUnitTests : ModifierTests
	{
		[Test]
		public void Thorns_OnHit()
		{
			AddRecipe("ThornsOnHitEvent")
				.Effect(new DamageEffect(5, targeting: Targeting.SourceTarget), EffectOn.CallbackUnit)
				.CallbackUnit(CallbackUnitType.WhenAttacked);
			Setup();

			Unit.AddModifierSelf("ThornsOnHitEvent");

			Enemy.Attack(Unit);

			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
		}

		[Test]
		public void Thorns_OnHit_DurationRemove()
		{
			AddRecipe("ThornsOnHitEvent_Remove")
				.Effect(new DamageEffect(5, targeting: Targeting.SourceTarget), EffectOn.CallbackUnit)
				.CallbackUnit(CallbackUnitType.WhenAttacked)
				.Remove(5);
			Setup();

			Unit.AddModifierSelf("ThornsOnHitEvent_Remove");

			Enemy.Attack(Unit);

			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);

			Unit.Update(5);

			Enemy.Attack(Unit);
			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
		}

		//TODO Reflect X% damage on hit, needs a return value 
		//[Test]
		public void Reflect_OnAttacked()
		{
		}

		[Test]
		public void AddDamage_OnKill()
		{
			AddRecipe("AddDamage_OnKill_Event")
				.Effect(new AddDamageEffect(5, targeting: Targeting.SourceTarget), EffectOn.CallbackUnit)
				.CallbackUnit(CallbackUnitType.OnKill);
			Setup();

			var weakEnemy = new Unit(1, unitType: UnitType.Bad);
			Unit.AddModifierSelf("AddDamage_OnKill_Event");

			Assert.AreEqual(UnitDamage, Unit.Damage);

			Unit.Attack(weakEnemy);

			Assert.AreEqual(UnitDamage + 5, Unit.Damage);
		}

		[Test]
		public void Damage_OnDeath()
		{
			AddRecipe("Damage_OnDeath_Event")
				.Effect(new DamageEffect(5, targeting: Targeting.SourceTarget), EffectOn.CallbackUnit)
				.CallbackUnit(CallbackUnitType.WhenKilled);
			Setup();

			var weakUnit = new Unit(1);
			weakUnit.AddModifierSelf("Damage_OnDeath_Event");

			Enemy.Attack(weakUnit);

			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
		}

		[Test]
		public void Heal_OnHeal()
		{
			AddRecipe("Heal_OnHeal_Event")
				.Effect(new HealEffect(5, targeting: Targeting.SourceTarget), EffectOn.CallbackUnit)
				.CallbackUnit(CallbackUnitType.OnHeal);
			Setup();

			Unit.AddModifierSelf("Heal_OnHeal_Event");

			Unit.TakeDamage(5, Enemy);
			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Unit.Heal(Ally);
			Assert.AreEqual(UnitHealth, Unit.Health);
		}

		[Test]
		public void AttackSelf_OnHit()
		{
			AddRecipe("AttackSelf_OnHit_Event")
				.Effect(new AttackActionEffect(Targeting.TargetTarget), EffectOn.CallbackUnit)
				.CallbackUnit(CallbackUnitType.WhenAttacked);
			Setup();

			Unit.AddModifierSelf("AttackSelf_OnHit_Event");

			Enemy.Attack(Unit);

			Assert.AreEqual(UnitHealth - EnemyDamage - UnitDamage * Unit.MaxEventCount, Unit.Health);
		}

		[Test]
		public void PoisonDoT_OnHit()
		{
			AddRecipe("PoisonDoT")
				.Interval(1)
				.Effect(new DamageEffect(5), EffectOn.Interval);
			AddRecipe("PoisonDoT_OnHit_Event")
				.Effect(new ApplierEffect("PoisonDoT", targeting: Targeting.SourceTarget), EffectOn.CallbackUnit)
				.CallbackUnit(CallbackUnitType.WhenAttacked);
			Setup();

			Unit.AddModifierSelf("PoisonDoT_OnHit_Event");

			Enemy.Attack(Unit);

			Enemy.Update(1);
			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);

			Enemy.Update(1);
			Assert.AreEqual(EnemyHealth - 10, Enemy.Health);
		}

		[Test]
		public void DoubleThorns_WhenAttacked_OneRecursion()
		{
			float thornsDamage = 5;
			AddRecipe("ThornsWhenAttackedEvent")
				.Effect(new DamageEffect(thornsDamage, targeting: Targeting.SourceTarget), EffectOn.CallbackUnit)
				.CallbackUnit(CallbackUnitType.WhenAttacked);
			Setup();

			Unit.AddModifierSelf("ThornsWhenAttackedEvent");
			Enemy.AddModifierSelf("ThornsWhenAttackedEvent");

			Enemy.Attack(Unit); //Enemy gets thorned, and recursively thorns Unit
			Assert.AreEqual(EnemyHealth - thornsDamage * Unit.MaxEventCount, Enemy.Health);
			Assert.AreEqual(UnitHealth - EnemyDamage - thornsDamage * Unit.MaxEventCount, Unit.Health);

			Unit.Attack(Enemy);
			Assert.AreEqual(UnitHealth - EnemyDamage - thornsDamage * 2 * Unit.MaxEventCount, Unit.Health);
		}

		[Test]
		public void SelfDamage_PreAttack()
		{
			AddRecipe("InitDamageSelf_BeforeAttack_Event")
				.Effect(new DamageEffect(5, targeting: Targeting.SourceSource), EffectOn.CallbackUnit)
				.CallbackUnit(CallbackUnitType.BeforeAttack);
			Setup();

			Unit.AddModifierSelf("InitDamageSelf_BeforeAttack_Event");

			Unit.PreAttack(Enemy);

			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void BuffAttackers_WhenHit()
		{
			AddRecipe("AddDamage")
				.OneTimeInit()
				.Effect(new AddDamageEffect(5, EffectState.IsRevertible), EffectOn.Init)
				.Remove(1).Refresh();
			AddRecipe("BuffAttackers_WhenHit_Event")
				.Effect(new ApplierEffect("AddDamage", targeting: Targeting.SourceTarget), EffectOn.CallbackUnit)
				.CallbackUnit(CallbackUnitType.WhenAttacked);
			Setup();

			Enemy.AddModifierSelf("BuffAttackers_WhenHit_Event");
			Unit.Attack(Enemy);
			Assert.AreEqual(UnitDamage + 5, Unit.Damage);
			Assert.AreEqual(EnemyDamage, Enemy.Damage);
			Unit.Update(0.9f);
			Assert.AreEqual(UnitDamage + 5, Unit.Damage);
			Unit.Update(1.1f);
			Assert.AreEqual(UnitDamage, Unit.Damage);
			Unit.Attack(Enemy);
			Assert.AreEqual(UnitDamage + 5, Unit.Damage);

			Assert.AreEqual(AllyDamage, Ally.Damage);
			Ally.Attack(Enemy);
			Assert.AreEqual(AllyDamage + 5, Ally.Damage);
		}

		[Test]
		public void Thorns_OnHit_DurationRemove_Twice()
		{
			AddRecipe("ThornsOnHitEvent_Remove")
				.Effect(new DamageEffect(5, targeting: Targeting.SourceTarget), EffectOn.CallbackUnit)
				.CallbackUnit(CallbackUnitType.WhenAttacked)
				.Remove(5);
			Setup();

			Unit.AddModifierSelf("ThornsOnHitEvent_Remove");

			Enemy.Attack(Unit);

			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);

			Unit.Update(3);

			Unit.AddModifierSelf("ThornsOnHitEvent_Remove");

			Enemy.Attack(Unit);

			Assert.AreEqual(EnemyHealth - 5 - 5, Enemy.Health);

			Unit.Update(2);

			Enemy.Attack(Unit);
			Assert.AreEqual(EnemyHealth - 5 - 5, Enemy.Health);
		}

		[Test]
		public void HealSelfWhenHealed_Recursion()
		{
			const float damage = 50;
			AddRecipe("HealSelfWhenHealed_Recursion")
				.Effect(new HealEffect(5, targeting: Targeting.SourceTarget), EffectOn.CallbackUnit)
				.CallbackUnit(CallbackUnitType.WhenHealed);
			Setup();

			Unit.TakeDamage(damage, Enemy);

			Unit.AddModifierSelf("HealSelfWhenHealed_Recursion");
			Unit.Heal(5, Unit);
			Assert.AreEqual(UnitHealth - damage + 5 + 5 * Unit.MaxEventCount, Unit.Health);
		}

		[Test]
		public void ThornsSelfHeal_Double_Recursion()
		{
			const float damage = 50;
			AddRecipe("HealSourceWhenAttacked_Recursion")
				.Effect(new HealEffect(2, targeting: Targeting.SourceTarget), EffectOn.CallbackUnit)
				.CallbackUnit(CallbackUnitType.WhenAttacked);
			AddRecipe("ThornsSourceWhenHealed_Recursion")
				.Effect(new DamageEffect(1, targeting: Targeting.SourceTarget), EffectOn.CallbackUnit)
				.CallbackUnit(CallbackUnitType.WhenHealed);
			Setup();

			Unit.TakeDamage(damage, Unit);
			Unit.AddModifierSelf("HealSourceWhenAttacked_Recursion");
			Unit.AddModifierSelf("ThornsSourceWhenHealed_Recursion");

			Unit.Heal(Unit);
			Assert.AreEqual(
				UnitHealth - damage + UnitHeal + 2 * Unit.MaxEventCount - 1 * Unit.MaxEventCount,
				Unit.Health);
		}

		[Test]
		public void ThornsSelfHeal_Double_Recursion_Enemy()
		{
			const float damage = 50;
			AddRecipe("HealSourceWhenAttacked_Recursion")
				.Effect(new HealEffect(2, targeting: Targeting.SourceTarget), EffectOn.CallbackUnit)
				.CallbackUnit(CallbackUnitType.WhenAttacked);
			AddRecipe("ThornsSourceWhenHealed_Recursion")
				.Effect(new DamageEffect(1, targeting: Targeting.SourceTarget), EffectOn.CallbackUnit)
				.CallbackUnit(CallbackUnitType.WhenHealed);
			Setup();

			Unit.TakeDamage(damage, Unit);
			Enemy.TakeDamage(damage, Enemy);
			Unit.AddModifierSelf("HealSourceWhenAttacked_Recursion");
			Enemy.AddModifierSelf("ThornsSourceWhenHealed_Recursion");

			Unit.Heal(Enemy);
			Assert.AreEqual(UnitHealth - damage - 1 * Unit.MaxEventCount, Unit.Health);
			Assert.AreEqual(EnemyHealth - damage + UnitHeal + 2 * Unit.MaxEventCount, Enemy.Health);
		}

		[Test]
		public void ThornsSelfWhenAndAfterAttacked_Double_Recursion()
		{
			AddRecipe("ThornsSourceWhenAttacked_Recursion")
				.Effect(new DamageEffect(2, targeting: Targeting.SourceTarget), EffectOn.CallbackUnit)
				.CallbackUnit(CallbackUnitType.WhenAttacked);
			AddRecipe("ThornsSourceAfterAttacked_Recursion")
				.Effect(new DamageEffect(2, targeting: Targeting.SourceTarget), EffectOn.CallbackUnit)
				.CallbackUnit(CallbackUnitType.AfterAttacked);
			Setup();

			Unit.AddModifierSelf("ThornsSourceWhenAttacked_Recursion");
			Unit.AddModifierSelf("ThornsSourceAfterAttacked_Recursion");

			//Unit attack itself, dealing 10 damage. First thorns gets activated, deals 2 damage * Recursion.
			//Second thorns gets activated, deals 2 damage * Recursion.
			Unit.Attack(Unit);
			Assert.AreEqual(
				UnitHealth - UnitDamage - 2 * Unit.MaxEventCount - 2 * Unit.MaxEventCount,
				Unit.Health);

			Unit.Heal(UnitHealth, Unit);

			Unit.Attack(Unit);
			Assert.AreEqual(
				UnitHealth - UnitDamage - 2 * Unit.MaxEventCount - 2 * Unit.MaxEventCount,
				Unit.Health);
		}

		[Test]
		public void Init_AddDamage_HalfHealth_TriggerCallback_RemoveAndRevert()
		{
			AddRecipe("InitAddDamageRevertibleHalfHealthCallback")
				.Effect(new AddDamageEffect(5, EffectState.IsRevertible), EffectOn.Init)
				.Remove(RemoveEffectOn.CallbackUnit)
				.CallbackUnit(CallbackUnitType.StrongHit);
			Setup();

			Unit.AddModifierSelf("InitAddDamageRevertibleHalfHealthCallback");
			Assert.AreEqual(UnitDamage + 5, Unit.Damage);

			//Take enough damage to trigger the callback
			Unit.TakeDamage(UnitHealth * 0.6f, Unit);
			Assert.AreEqual(UnitDamage, Unit.Damage);
		}

		[Test]
		public void Init_RegisterCallbackHealToFullWhenTakingStrongHit_Effect()
		{
			AddRecipe("InitHealToFullHalfHealthCallback")
				.Effect(new HealEffect(0).SetMetaEffects(new AddValueBasedOnStatDiffMetaEffect(StatType.MaxHealth)),
					EffectOn.CallbackUnit)
				.CallbackUnit(CallbackUnitType.StrongHit);
			Setup();

			Unit.AddModifierSelf("InitHealToFullHalfHealthCallback");
			Assert.AreEqual(UnitHealth, Unit.Health);

			//Take enough damage to trigger the callback
			Unit.TakeDamage(UnitHealth * 0.6f, Unit); //Takes 60% of max hp damage, triggers callback, heals to full
			Assert.AreEqual(UnitHealth, Unit.Health);
		}


		[Test]
		public void Init_RegisterCallbackHeal10WhenTakingStrongHit_Effect()
		{
			AddRecipe("InitHeal10HalfHealthCallback")
				.Effect(new HealEffect(10), EffectOn.CallbackUnit)
				.CallbackUnit(CallbackUnitType.StrongHit);
			Setup();

			Unit.AddModifierSelf("InitHeal10HalfHealthCallback");
			Assert.AreEqual(UnitHealth, Unit.Health);

			//Take enough damage to trigger the callback
			float damage = UnitHealth * 0.6f;
			Unit.TakeDamage(damage, Unit); //Takes 60% of max hp damage, triggers callback

			Assert.AreEqual(UnitHealth - damage + 10, Unit.Health);
		}

		[Test]
		public void Init_RegisterCallbackHealTenWhenTakingStrongHit_ThenTakeFiveDamage_Effect()
		{
			AddRecipe("InitHealDamageWhenStrongHitCallback")
				.Effect(new HealEffect(10), EffectOn.CallbackUnit)
				.Effect(new DamageEffect(5), EffectOn.CallbackUnit)
				.CallbackUnit(CallbackUnitType.StrongHit);
			Setup();

			Unit.AddModifierSelf("InitHealDamageWhenStrongHitCallback");
			Assert.AreEqual(UnitHealth, Unit.Health);

			//Take enough damage to trigger the callback
			float damage = UnitHealth * 0.6f;
			Unit.TakeDamage(damage, Unit); //Takes 60% of max hp damage, triggers callbacks

			Assert.AreEqual(UnitHealth - damage + 10 - 5, Unit.Health);
		}

		[Test]
		public void Init_InstanceCheck_Effect()
		{
			AddRecipe("InitHealDamageWhenStrongHitCallback")
				.Effect(new HealEffect(10), EffectOn.CallbackUnit)
				.Effect(new DamageEffect(5), EffectOn.CallbackUnit)
				.CallbackUnit(CallbackUnitType.StrongHit);
			Setup();

			Unit.AddModifierSelf("InitHealDamageWhenStrongHitCallback");
			Enemy.AddModifierSelf("InitHealDamageWhenStrongHitCallback");

			float enemyDamage = EnemyHealth * 0.6f;
			Enemy.TakeDamage(enemyDamage, Enemy); //Takes 60% of max hp damage, triggers callbacks
			Assert.AreEqual(EnemyHealth - enemyDamage + 10 - 5, Enemy.Health);

			Assert.AreEqual(UnitHealth, Unit.Health);

			//Take enough damage to trigger the callback
			float unitDamage = UnitHealth * 0.6f;
			Unit.TakeDamage(unitDamage, Unit); //Takes 60% of max hp damage, triggers callbacks

			Assert.AreEqual(UnitHealth - unitDamage + 10 - 5, Unit.Health);
		}

		[Test]
		public void Init_RegisterCallbackHeal10WhenTakingStrongHitRevert()
		{
			AddRecipe("InitHealToFullHalfHealthCallback")
				.Effect(new HealEffect(10), EffectOn.CallbackUnit)
				.CallbackUnit(CallbackUnitType.StrongHit)
				.Remove(1);
			Setup();

			Unit.AddModifierSelf("InitHealToFullHalfHealthCallback");
			Assert.AreEqual(UnitHealth, Unit.Health);

			//Take enough damage to trigger the callback
			float damage = UnitHealth * 0.6f;
			Unit.TakeDamage(damage, Unit); //Takes 60% of max hp damage, triggers callback

			Assert.AreEqual(UnitHealth - damage + 10, Unit.Health);

			//Heal to full
			Unit.Heal(UnitHealth, Unit);

			Unit.Update(1); //Removes the modifier => reverts the register effect => removes the callback
			Unit.TakeDamage(damage, Unit);
			Assert.AreEqual(UnitHealth - damage, Unit.Health);
		}

		[Test]
		public void Init_AddDamage_HalfHealth_TriggerCallback_RemoveAndRevert_Twice()
		{
			AddRecipe("InitAddDamageRevertibleHalfHealthCallback")
				.Effect(new AddDamageEffect(5, EffectState.IsRevertible), EffectOn.Init)
				.Remove(RemoveEffectOn.CallbackUnit)
				.CallbackUnit(CallbackUnitType.StrongHit);
			Setup();

			Unit.AddModifierSelf("InitAddDamageRevertibleHalfHealthCallback");
			Unit.AddModifierSelf("InitAddDamageRevertibleHalfHealthCallback");
			Assert.AreEqual(UnitDamage + 5 + 5, Unit.Damage);

			//Take enough damage to trigger the callback
			Unit.TakeDamage(UnitHealth * 0.6f, Unit);
			Assert.AreEqual(UnitDamage, Unit.Damage);

			Unit.TakeDamage(UnitHealth * 0.6f, Unit);
			Assert.AreEqual(UnitDamage, Unit.Damage);
		}

		[Test]
		public void MultipleUnitCallbacks()
		{
			AddRecipe("AddDamageUnitCallbacksRemoveOnStrongDispel")
				.Effect(new AddDamageEffect(1, EffectState.IsRevertible), EffectOn.CallbackUnit)
				.CallbackUnit(CallbackUnitType.WhenAttacked)
				.Effect(new AddDamageEffect(2, EffectState.IsRevertible), EffectOn.CallbackUnit2)
				.CallbackUnit(CallbackUnitType.WhenHealed)
				.Effect(new AddDamageEffect(3, EffectState.IsRevertible, targeting: Targeting.SourceTarget),
					EffectOn.CallbackUnit3)
				.CallbackUnit(CallbackUnitType.OnAttack)
				.Remove(RemoveEffectOn.CallbackUnit4)
				.CallbackUnit(CallbackUnitType.StrongDispel);
			Setup();

			Unit.AddModifierSelf("AddDamageUnitCallbacksRemoveOnStrongDispel");
			Enemy.Attack(Unit);
			Assert.AreEqual(UnitDamage + 1, Unit.Damage);
			Unit.Heal(0, Unit);
			Assert.AreEqual(UnitDamage + 1 + 2, Unit.Damage);
			Unit.Attack(Enemy);
			Assert.AreEqual(UnitDamage + 1 + 2 + 3, Unit.Damage);
			Unit.StrongDispel(Unit);
			Assert.AreEqual(UnitDamage, Unit.Damage);
		}
	}
}