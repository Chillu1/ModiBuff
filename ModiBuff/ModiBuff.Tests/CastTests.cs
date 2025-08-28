using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class CastTests : ModifierTests
	{
		[Test]
		public void CastInitDamageNoChecks_OnEnemy()
		{
			Setup();

			int id = IdManager.GetId("InitDamage").Value;
			Unit.AddApplierModifierNew(id, ApplierType.Cast);

			Unit.TryCast(id, Enemy);

			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
		}

		[Test]
		public void CastInitDamageChecks_OnEnemy()
		{
			AddRecipe("InitDamageFullHealth")
				.Effect(new DamageEffect(5), EffectOn.Init);
			Setup();

			int id = IdManager.GetId("InitDamageFullHealth").Value;
			Unit.AddApplierModifierNew(id, ApplierType.Cast,
				new ICheck[] { new ConditionCheck(ConditionType.HealthIsFull) });

			Unit.TryCast(id, Enemy);

			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);

			Unit.TakeDamage(5, Enemy);

			Unit.TryCast(id, Enemy);

			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
		}

		[Test]
		public void AttackInitDamageNoChecks_OnEnemy()
		{
			Setup();

			Unit.AddApplierModifierNew(IdManager.GetId("InitDamage").Value, ApplierType.Attack);

			Unit.Attack(Enemy);

			Assert.AreEqual(EnemyHealth - UnitDamage - 5, Enemy.Health);
		}

		[Test]
		public void AttackInitDamageChecks_OnEnemy()
		{
			AddRecipe("InitDamageFullHealth")
				.Effect(new DamageEffect(5), EffectOn.Init);
			Setup();

			Unit.AddApplierModifierNew(IdManager.GetId("InitDamageFullHealth").Value, ApplierType.Attack,
				new ICheck[] { new ConditionCheck(ConditionType.HealthIsFull) });

			Unit.Attack(Enemy);

			Assert.AreEqual(EnemyHealth - UnitDamage - 5, Enemy.Health);

			Unit.TakeDamage(5, Enemy);

			Unit.Attack(Enemy);

			Assert.AreEqual(EnemyHealth - UnitDamage * 2 - 5, Enemy.Health);
		}

		[Test]
		public void CastInitDamageChecksDelayedUse_OnEnemy()
		{
			AddRecipe("InitDamageFullHealth")
				.Effect(new DamageEffect(5), EffectOn.Init);
			Setup();

			int id = IdManager.GetId("InitDamageFullHealth").Value;

			Unit.AddApplierModifierNew(id, ApplierType.Cast,
				new ICheck[] { new ConditionCheck(ConditionType.HealthIsFull) });

			Assert.True(Unit.TryCast(id, Unit));
			Assert.AreEqual(EnemyHealth, Enemy.Health);

			Assert.True(Unit.TryCastNoChecks(id, Enemy));

			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
		}

		[Test]
		public void CastOnCastEventRecursion()
		{
			AddRecipe("CastInitDamageEvent")
				.Effect(new CastActionEffect("InitDamage"), EffectOn.CallbackUnit)
				.CallbackUnit(CallbackUnitType.OnCast);
			Setup();

			int id = IdManager.GetId("InitDamage").Value;
			Unit.AddApplierModifierNew(id, ApplierType.Cast);
			Unit.AddModifierSelf("CastInitDamageEvent");

			Unit.TryCast(id, Enemy);
			Assert.AreEqual(EnemyHealth - 5 - 5 * Unit.MaxEventCount, Enemy.Health);
		}

		//[Test]
		public void SelfApplyIfNotCast()
		{
			//Trigger duration timer, if duration timer ends, damages self
			//If we cast this modifier, remove the debuff modifier of us, and apply the damage effect to the enemy 

			Recipes.Register("DurationDamageSelfCast");
			int modId = IdManager.GetId("DurationDamageSelfCast").Value;

			AddRecipe("DurationDamageSelfCast")
				.Effect(new ApplierEffect("InitDamage"), EffectOn.Duration | EffectOn.CallbackEffect)
				.Remove(1)
				.CallbackEffect(CallbackType.OnCast, effect => new CastEvent((target, source, id) =>
				{
					if (id == modId)
						switch (effect)
						{
							case ApplierEffect applierEffect:
								applierEffect.Effect(target, source);
								break;
							case RemoveEffect removeEffect:
								removeEffect.Effect(target, source);
								break;
						}
				}))
				.Remove(RemoveEffectOn.CallbackEffect);

			//AddEffect("DurationDamageCast", new ApplierEffect("DurationDamageSelfCast", ApplierType.Cast));
			AddRecipe("DurationDamageCast")
				.Effect(new ApplierEffect("DurationDamageSelfCast", ApplierType.Cast), EffectOn.Init)
				//.RemoveApplier(RemoveEffectOn.CallbackEffect, ApplierType.Cast, false)
				.CallbackEffect(CallbackType.OnCast, effect => new CastEvent((target, source, id) =>
				{
					if (id == modId)
						effect.Effect(target, source);
				}));
			//.Effect(new RemoveEffect("DurationDamageSelfCast"), EffectOn.Init);
			Setup();

			//Unit.AddEffectApplier("DurationDamageCast");
			//Unit.TryCastEffect("DurationDamageCast", Unit); //Adds modifier, starts duration
			Unit.AddApplierModifier(Recipes.GetGenerator("DurationDamageCast"), ApplierType.Cast);
			Unit.TryCast("DurationDamageCast", Unit); //Adds modifier, starts duration

			Unit.TryCast("DurationDamageSelfCast", Enemy); //Removes modifier, applies damage to enemy
			Unit.Update(0);
			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
			Assert.AreEqual(UnitHealth, Unit.Health);
			//TODO Enemy has SelfCast (shouldn't), Unit doesn't get his appliers removed
			Assert.False(Unit.ContainsModifier("DurationDamageSelfCast"));
			Assert.False(Unit.ContainsApplier("DurationDamageSelfCast"));

			//Unit.TryCastEffect("DurationDamageCast", Unit); //Adds modifier, starts duration
			Unit.TryCast("DurationDamageCast", Unit);
			Unit.Update(1);
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
			Assert.False(Unit.ContainsModifier("DurationDamageSelfCast"));
			Assert.False(Unit.ContainsApplier("DurationDamageSelfCast"));
		}
	}
}