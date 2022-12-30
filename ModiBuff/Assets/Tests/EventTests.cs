using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class EventTests : BaseModifierTests
	{
		[Test]
		public void ThornsEffect_OnHit()
		{
			Unit.AddEffectEvent(new ActerDamageEffect(5), EffectOnEvent.WhenAttacked);

			Enemy.Attack(Unit);

			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
		}

		[Test]
		public void ThornsEffect_OnHit_Remove()
		{
			var effect = new ActerDamageEffect(5);
			Unit.AddEffectEvent(effect, EffectOnEvent.WhenAttacked);

			Enemy.Attack(Unit);

			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);

			Unit.RemoveEffectEvent(effect, EffectOnEvent.WhenAttacked);
			Enemy.Attack(Unit);
			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
		}

		[Test]
		public void Thorns_OnHit()
		{
			Unit.TryAddModifierSelf("ThornsOnHitEvent");

			Enemy.Attack(Unit);

			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
		}

		[Test]
		public void Thorns_OnHit_DurationRemove()
		{
			Unit.TryAddModifierSelf("ThornsOnHitEvent_Remove");

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
			var weakEnemy = new Unit(1);
			Unit.TryAddModifierSelf("AddDamage_OnKill_Event");

			Assert.AreEqual(UnitDamage, Unit.Damage);

			Unit.Attack(weakEnemy);

			Assert.AreEqual(UnitDamage + 5, Unit.Damage);
		}

		[Test]
		public void Damage_OnDeath()
		{
			var weakUnit = new Unit(1);
			weakUnit.TryAddModifierSelf("Damage_OnDeath_Event");

			Enemy.Attack(weakUnit);

			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
		}

		[Test]
		public void Heal_OnHeal()
		{
			Unit.TryAddModifierSelf("Heal_OnHeal_Event");

			Unit.TakeDamage(5, Enemy);
			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Unit.Heal(Ally);
			Assert.AreEqual(UnitHealth, Unit.Health);
		}

		[Test]
		public void AttackSelf_OnHit()
		{
			Unit.TryAddModifierSelf("AttackSelf_OnHit_Event");

			Enemy.Attack(Unit);

			Assert.AreEqual(UnitHealth - EnemyDamage - UnitDamage, Unit.Health);
		}

		[Test]
		public void PoisonDoT_OnHit()
		{
			Unit.TryAddModifierSelf("PoisonDoT_OnHit_Event");

			Enemy.Attack(Unit);

			Enemy.Update(1);
			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);

			Enemy.Update(1);
			Assert.AreEqual(EnemyHealth - 10, Enemy.Health);
		}

		[Test]
		public void Thorns_OnHit_Recursion()
		{
			Unit.TryAddModifierSelf("ThornsOnHitEvent");
			Enemy.TryAddModifierSelf("ThornsOnHitEvent");

			Enemy.Attack(Unit);
			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);

			Unit.Attack(Enemy);
			Assert.AreEqual(UnitHealth - EnemyDamage - 5, Unit.Health);
		}
	}
}