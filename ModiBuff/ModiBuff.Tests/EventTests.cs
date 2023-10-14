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
				.Effect(new DamageEffect(5), EffectOn.Event, Targeting.SourceTarget)
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
				.Effect(new DamageEffect(5), EffectOn.Event, Targeting.SourceTarget)
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
				.Effect(new AddDamageEffect(5), EffectOn.Event, Targeting.SourceTarget)
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
				.Effect(new DamageEffect(5), EffectOn.Event, Targeting.SourceTarget)
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
				.Effect(new HealEffect(5), EffectOn.Event, Targeting.SourceTarget)
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

			Assert.AreEqual(UnitHealth - EnemyDamage - UnitDamage, Unit.Health);
		}

		[Test]
		public void PoisonDoT_OnHit()
		{
			AddRecipe("PoisonDoT")
				.Interval(1)
				.Effect(new DamageEffect(5), EffectOn.Interval);
			AddRecipe("PoisonDoT_OnHit_Event")
				.Effect(new ApplierEffect("PoisonDoT"), EffectOn.Event, Targeting.SourceTarget)
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
				.Effect(new DamageEffect(thornsDamage), EffectOn.Event, Targeting.SourceTarget)
				.Event(EffectOnEvent.WhenAttacked);
			Setup();

			Unit.AddModifierSelf("ThornsWhenAttackedEvent");
			Enemy.AddModifierSelf("ThornsWhenAttackedEvent");

			Enemy.Attack(Unit); //Enemy gets thorned, and recursively thorns Unit
			Assert.AreEqual(EnemyHealth - thornsDamage, Enemy.Health);
			Assert.AreEqual(UnitHealth - EnemyDamage - thornsDamage, Unit.Health);

			Enemy.Update(0); //Refresh event count
			Unit.Update(0);

			Unit.Attack(Enemy);
			Assert.AreEqual(UnitHealth - EnemyDamage - thornsDamage - thornsDamage, Unit.Health);
		}

		[Test]
		public void SelfDamage_PreAttack()
		{
			AddRecipe("InitDamageSelf_BeforeAttack_Event")
				.Effect(new DamageEffect(5), EffectOn.Event, Targeting.SourceSource)
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
				.Effect(new ApplierEffect("AddDamage"), EffectOn.Event, Targeting.SourceTarget)
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
			Enemy.Update(0);
			Unit.Attack(Enemy);
			Assert.AreEqual(UnitDamage + 5, Unit.Damage);

			Assert.AreEqual(AllyDamage, Ally.Damage);
			Enemy.Update(0);
			Ally.Attack(Enemy);
			Assert.AreEqual(AllyDamage + 5, Ally.Damage);
		}
	}
}