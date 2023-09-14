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
			SetupSystems();

			var effect = new DamageEffect(5);
			effect.SetTargeting(Targeting.SourceTarget);
			Unit.AddEffectEvent(effect, EffectOnEvent.WhenAttacked);

			Enemy.Attack(Unit);

			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
		}

		[Test]
		public void ThornsEffect_OnHit_Remove()
		{
			SetupSystems();

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
			AddEventRecipes(add => add("ThornsOnHitEvent", EffectOnEvent.WhenAttacked)
				.Effect(new DamageEffect(5), Targeting.SourceTarget));

			Unit.AddModifierSelf("ThornsOnHitEvent");

			Enemy.Attack(Unit);

			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
		}

		[Test]
		public void Thorns_OnHit_DurationRemove()
		{
			AddEventRecipes(add => add("ThornsOnHitEvent_Remove", EffectOnEvent.WhenAttacked)
				.Effect(new DamageEffect(5), Targeting.SourceTarget)
				.Remove(5));

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
			AddEventRecipes(add => add("AddDamage_OnKill_Event", EffectOnEvent.OnKill)
				.Effect(new AddDamageEffect(5), Targeting.SourceTarget));

			var weakEnemy = new Unit(1);
			Unit.AddModifierSelf("AddDamage_OnKill_Event");

			Assert.AreEqual(UnitDamage, Unit.Damage);

			Unit.Attack(weakEnemy);

			Assert.AreEqual(UnitDamage + 5, Unit.Damage);
		}

		[Test]
		public void Damage_OnDeath()
		{
			AddEventRecipes(add => add("Damage_OnDeath_Event", EffectOnEvent.WhenKilled)
				.Effect(new DamageEffect(5), Targeting.SourceTarget));

			var weakUnit = new Unit(1);
			weakUnit.AddModifierSelf("Damage_OnDeath_Event");

			Enemy.Attack(weakUnit);

			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
		}

		[Test]
		public void Heal_OnHeal()
		{
			AddEventRecipes(add => add("Heal_OnHeal_Event", EffectOnEvent.OnHeal)
				.Effect(new HealEffect(5), Targeting.SourceTarget));

			Unit.AddModifierSelf("Heal_OnHeal_Event");

			Unit.TakeDamage(5, Enemy);
			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Unit.Heal(Ally);
			Assert.AreEqual(UnitHealth, Unit.Health);
		}

		[Test]
		public void AttackSelf_OnHit()
		{
			AddEventRecipes(add => add("AttackSelf_OnHit_Event", EffectOnEvent.WhenAttacked)
				.Effect(new SelfAttackActionEffect()));

			Unit.AddModifierSelf("AttackSelf_OnHit_Event");

			Enemy.Attack(Unit);

			Assert.AreEqual(UnitHealth - EnemyDamage - UnitDamage, Unit.Health);
		}

		[Test]
		public void PoisonDoT_OnHit()
		{
			AddMixedRecipes(new RecipeAddFunc[]
			{
				add => add("PoisonDoT").Interval(1).Effect(new DamageEffect(5), EffectOn.Interval)
			}, new EventRecipeAddFunc[]
			{
				add => add("PoisonDoT_OnHit_Event", EffectOnEvent.WhenAttacked)
					.Effect(new ApplierEffect("PoisonDoT"), Targeting.SourceTarget)
			});

			Unit.AddModifierSelf("PoisonDoT_OnHit_Event");

			Enemy.Attack(Unit);

			Enemy.Update(1);
			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);

			Enemy.Update(1);
			Assert.AreEqual(EnemyHealth - 10, Enemy.Health);
		}

		[Test]
		public void Thorns_OnHit_Recursion()
		{
			AddEventRecipes(add => add("ThornsOnHitEvent", EffectOnEvent.WhenAttacked)
				.Effect(new DamageEffect(5), Targeting.SourceTarget));

			Unit.AddModifierSelf("ThornsOnHitEvent");
			Enemy.AddModifierSelf("ThornsOnHitEvent");

			Enemy.Attack(Unit);
			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);

			Unit.Attack(Enemy);
			Assert.AreEqual(UnitHealth - EnemyDamage - 5, Unit.Health);
		}


		[Test]
		public void SelfDamage_PreAttack()
		{
			AddEventRecipes(add => add("InitDamageSelf_BeforeAttack_Event", EffectOnEvent.BeforeAttack)
				.Effect(new DamageEffect(5), Targeting.SourceSource));

			Unit.AddModifierSelf("InitDamageSelf_BeforeAttack_Event");

			Unit.PreAttack(Enemy);

			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}
	}
}