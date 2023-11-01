using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class EventTests : ModifierTests
	{
		[Test]
		public void Thorns_OnHit()
		{
			AddRecipe("ThornsOnHitEvent")
				.Effect(new DamageEffect(5, targeting: Targeting.SourceTarget), EffectOn.Event)
				.Event(EffectOnEvent.WhenAttacked);
			Setup();

			Unit.AddModifierSelf("ThornsOnHitEvent");

			Enemy.Attack(Unit);

			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
		}

		[Test]
		public void Thorns_OnHit_DurationRemove()
		{
			AddRecipe("ThornsOnHitEvent_Remove")
				.Effect(new DamageEffect(5, targeting: Targeting.SourceTarget), EffectOn.Event)
				.Event(EffectOnEvent.WhenAttacked)
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
				.Effect(new AddDamageEffect(5, targeting: Targeting.SourceTarget), EffectOn.Event)
				.Event(EffectOnEvent.OnKill);
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
				.Effect(new DamageEffect(5, targeting: Targeting.SourceTarget), EffectOn.Event)
				.Event(EffectOnEvent.WhenKilled);
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
				.Effect(new HealEffect(5, targeting: Targeting.SourceTarget), EffectOn.Event)
				.Event(EffectOnEvent.OnHeal);
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
				.Effect(new SelfAttackActionEffect(), EffectOn.Event)
				.Event(EffectOnEvent.WhenAttacked);
			Setup();

			Unit.AddModifierSelf("AttackSelf_OnHit_Event");

			Enemy.Attack(Unit);

			Assert.AreEqual(UnitHealth - EnemyDamage - UnitDamage * Unit.MaxRecursionEventCount, Unit.Health);
		}

		[Test]
		public void PoisonDoT_OnHit()
		{
			AddRecipe("PoisonDoT")
				.Interval(1)
				.Effect(new DamageEffect(5), EffectOn.Interval);
			AddRecipe("PoisonDoT_OnHit_Event")
				.Effect(new ApplierEffect("PoisonDoT", targeting: Targeting.SourceTarget), EffectOn.Event)
				.Event(EffectOnEvent.WhenAttacked);
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
				.Effect(new DamageEffect(thornsDamage, targeting: Targeting.SourceTarget), EffectOn.Event)
				.Event(EffectOnEvent.WhenAttacked);
			Setup();

			Unit.AddModifierSelf("ThornsWhenAttackedEvent");
			Enemy.AddModifierSelf("ThornsWhenAttackedEvent");

			Enemy.Attack(Unit); //Enemy gets thorned, and recursively thorns Unit
			Assert.AreEqual(EnemyHealth - thornsDamage * Unit.MaxRecursionEventCount, Enemy.Health);
			Assert.AreEqual(UnitHealth - EnemyDamage - thornsDamage * Unit.MaxRecursionEventCount, Unit.Health);

			Unit.Attack(Enemy);
			Assert.AreEqual(UnitHealth - EnemyDamage - thornsDamage * 2 * Unit.MaxRecursionEventCount, Unit.Health);
		}

		[Test]
		public void SelfDamage_PreAttack()
		{
			AddRecipe("InitDamageSelf_BeforeAttack_Event")
				.Effect(new DamageEffect(5, targeting: Targeting.SourceSource), EffectOn.Event)
				.Event(EffectOnEvent.BeforeAttack);
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
				.Effect(new AddDamageEffect(5, true), EffectOn.Init)
				.Remove(1).Refresh();
			AddRecipe("BuffAttackers_WhenHit_Event")
				.Effect(new ApplierEffect("AddDamage", targeting: Targeting.SourceTarget), EffectOn.Event)
				.Event(EffectOnEvent.WhenAttacked);
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
				.Effect(new DamageEffect(5, targeting: Targeting.SourceTarget), EffectOn.Event)
				.Event(EffectOnEvent.WhenAttacked)
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
				.Effect(new HealEffect(5, targeting: Targeting.SourceTarget), EffectOn.Event)
				.Event(EffectOnEvent.WhenHealed);
			Setup();

			Unit.TakeDamage(damage, Enemy);

			Unit.AddModifierSelf("HealSelfWhenHealed_Recursion");
			Unit.Heal(5, Unit);
			Assert.AreEqual(UnitHealth - damage + 5 + 5 * Unit.MaxRecursionEventCount, Unit.Health);
		}

		[Test]
		public void ThornsSelfHeal_Double_Recursion()
		{
			const float damage = 50;
			AddRecipe("HealSourceWhenAttacked_Recursion")
				.Effect(new HealEffect(2, targeting: Targeting.SourceTarget), EffectOn.Event)
				.Event(EffectOnEvent.WhenAttacked);
			AddRecipe("ThornsSourceWhenHealed_Recursion")
				.Effect(new DamageEffect(1, targeting: Targeting.SourceTarget), EffectOn.Event)
				.Event(EffectOnEvent.WhenHealed);
			Setup();

			Unit.TakeDamage(damage, Unit);
			Unit.AddModifierSelf("HealSourceWhenAttacked_Recursion");
			Unit.AddModifierSelf("ThornsSourceWhenHealed_Recursion");

			Unit.Heal(Unit);
			Assert.AreEqual(
				UnitHealth - damage + UnitHeal + 2 * Unit.MaxRecursionEventCount - 1 * Unit.MaxRecursionEventCount,
				Unit.Health);
		}

		[Test]
		public void ThornsSelfHeal_Double_Recursion_Enemy()
		{
			const float damage = 50;
			AddRecipe("HealSourceWhenAttacked_Recursion")
				.Effect(new HealEffect(2, targeting: Targeting.SourceTarget), EffectOn.Event)
				.Event(EffectOnEvent.WhenAttacked);
			AddRecipe("ThornsSourceWhenHealed_Recursion")
				.Effect(new DamageEffect(1, targeting: Targeting.SourceTarget), EffectOn.Event)
				.Event(EffectOnEvent.WhenHealed);
			Setup();

			Unit.TakeDamage(damage, Unit);
			Enemy.TakeDamage(damage, Enemy);
			Unit.AddModifierSelf("HealSourceWhenAttacked_Recursion");
			Enemy.AddModifierSelf("ThornsSourceWhenHealed_Recursion");

			Unit.Heal(Enemy);
			Assert.AreEqual(UnitHealth - damage - 1 * Unit.MaxRecursionEventCount, Unit.Health);
			Assert.AreEqual(EnemyHealth - damage + UnitHeal + 2 * Unit.MaxRecursionEventCount, Enemy.Health);
		}

		[Test]
		public void ThornsSelfWhenAndAfterAttacked_Double_Recursion()
		{
			AddRecipe("ThornsSourceWhenAttacked_Recursion")
				.Effect(new DamageEffect(2, targeting: Targeting.SourceTarget), EffectOn.Event)
				.Event(EffectOnEvent.WhenAttacked);
			AddRecipe("ThornsSourceAfterAttacked_Recursion")
				.Effect(new DamageEffect(2, targeting: Targeting.SourceTarget), EffectOn.Event)
				.Event(EffectOnEvent.AfterAttacked);
			Setup();

			Unit.AddModifierSelf("ThornsSourceWhenAttacked_Recursion");
			Unit.AddModifierSelf("ThornsSourceAfterAttacked_Recursion");

			Unit.Attack(Unit);
			Assert.AreEqual(
				UnitHealth - UnitDamage - 2 * Unit.MaxRecursionEventCount - 2 * Unit.MaxRecursionEventCount,
				Unit.Health);

			Unit.Heal(UnitHealth, Unit);

			Unit.Attack(Unit);
			Assert.AreEqual(
				UnitHealth - UnitDamage - 2 * Unit.MaxRecursionEventCount - 2 * Unit.MaxRecursionEventCount,
				Unit.Health);
		}
	}
}