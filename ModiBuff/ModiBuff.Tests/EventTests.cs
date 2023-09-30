using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class EventTests : ModifierTests
	{
		[Test]
		public void ThornsEffect_OnHit()
		{
			Setup();

			var effect = new DamageEffect(5);
			effect.SetTargeting(Targeting.SourceTarget);
			Unit.AddEffectEvent(effect, EffectOnEvent.WhenAttacked);

			Enemy.Attack(Unit);

			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
		}

		[Test]
		public void ThornsEffect_OnHit_Remove()
		{
			Setup();

			var effect = new DamageEffect(5);
			effect.SetTargeting(Targeting.SourceTarget);
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
			AddEventRecipe("ThornsOnHitEvent", EffectOnEvent.WhenAttacked)
				.Effect(new DamageEffect(5), Targeting.SourceTarget);
			Setup();

			Unit.AddModifierSelf("ThornsOnHitEvent");

			Enemy.Attack(Unit);

			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
		}

		[Test]
		public void Thorns_OnHit_DurationRemove()
		{
			AddEventRecipe("ThornsOnHitEvent_Remove", EffectOnEvent.WhenAttacked)
				.Effect(new DamageEffect(5), Targeting.SourceTarget)
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
			AddEventRecipe("AddDamage_OnKill_Event", EffectOnEvent.OnKill)
				.Effect(new AddDamageEffect(5), Targeting.SourceTarget);
			Setup();

			var weakEnemy = new Unit(1);
			Unit.AddModifierSelf("AddDamage_OnKill_Event");

			Assert.AreEqual(UnitDamage, Unit.Damage);

			Unit.Attack(weakEnemy);

			Assert.AreEqual(UnitDamage + 5, Unit.Damage);
		}

		[Test]
		public void Damage_OnDeath()
		{
			AddEventRecipe("Damage_OnDeath_Event", EffectOnEvent.WhenKilled)
				.Effect(new DamageEffect(5), Targeting.SourceTarget);
			Setup();

			var weakUnit = new Unit(1);
			weakUnit.AddModifierSelf("Damage_OnDeath_Event");

			Enemy.Attack(weakUnit);

			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
		}

		[Test]
		public void Heal_OnHeal()
		{
			AddEventRecipe("Heal_OnHeal_Event", EffectOnEvent.OnHeal)
				.Effect(new HealEffect(5), Targeting.SourceTarget);
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
			AddEventRecipe("AttackSelf_OnHit_Event", EffectOnEvent.WhenAttacked)
				.Effect(new SelfAttackActionEffect());
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
			AddEventRecipe("PoisonDoT_OnHit_Event", EffectOnEvent.WhenAttacked)
				.Effect(new ApplierEffect("PoisonDoT"), Targeting.SourceTarget);
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
			AddEventRecipe("ThornsWhenAttackedEvent", EffectOnEvent.WhenAttacked)
				.Effect(new DamageEffect(thornsDamage), Targeting.SourceTarget);
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
			AddEventRecipe("InitDamageSelf_BeforeAttack_Event", EffectOnEvent.BeforeAttack)
				.Effect(new DamageEffect(5), Targeting.SourceSource);
			Setup();

			Unit.AddModifierSelf("InitDamageSelf_BeforeAttack_Event");

			Unit.PreAttack(Enemy);

			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void TwoUniqueDamageEffectsActivatingThorns()
		{
			AddEventRecipe("ThornsWhenAttackedEvent", EffectOnEvent.WhenAttacked)
				.Effect(new DamageEffect(5), Targeting.SourceTarget);
			Setup();

			Unit.AddModifierSelf("ThornsWhenAttackedEvent");

			Enemy.AddApplierModifier(Recipes.GetGenerator("InitDamage"), ApplierType.Cast);
			Ally.AddApplierModifier(Recipes.GetGenerator("InitDamage"), ApplierType.Cast);

			int initDamageId = Recipes.GetGenerator("InitDamage").Id;
			Enemy.TryCast(initDamageId, Unit);
			Ally.TryCast(initDamageId, Unit);

			Assert.AreEqual(UnitHealth - 5 - 5, Unit.Health);
			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
			Assert.AreEqual(AllyHealth - 5, Ally.Health);
		}
	}
}