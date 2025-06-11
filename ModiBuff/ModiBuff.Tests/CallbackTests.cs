using System;
using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;
using TagType = ModiBuff.Core.Units.TagType;

namespace ModiBuff.Tests
{
	public sealed class CallbackTests : ModifierTests
	{
		[Test]
		public void Init_RegisterCallbackHealToFullWhenTakingStrongHit()
		{
			AddRecipe("InitHealToFullHalfHealthCallback")
				.Callback(CallbackType.StrongHit, (target, source) =>
				{
					var damageable = (IDamagable<float, float>)target;
					((IHealable<float, float>)target).Heal(damageable.MaxHealth - damageable.Health, source);
				});
			Setup();

			Unit.AddModifierSelf("InitHealToFullHalfHealthCallback");
			Assert.AreEqual(UnitHealth, Unit.Health);

			//Take enough damage to trigger the callback
			Unit.TakeDamage(UnitHealth * 0.6f, Unit); //Takes 60% of max hp damage, triggers callback, heals to full

			Assert.AreEqual(UnitHealth, Unit.Health);
		}

		[Test]
		public void Init_RegisterCallbackHealToFullWhenTakingStrongHitRevert()
		{
			AddRecipe("InitHealToFullHalfHealthCallback")
				.Callback(CallbackType.StrongHit, (target, source) =>
				{
					var damageable = (IDamagable<float, float>)target;
					((IHealable<float, float>)target).Heal(damageable.MaxHealth - damageable.Health, source);
				})
				.Remove(1);
			Setup();

			Unit.AddModifierSelf("InitHealToFullHalfHealthCallback");
			Assert.AreEqual(UnitHealth, Unit.Health);

			//Take enough damage to trigger the callback
			Unit.TakeDamage(UnitHealth * 0.6f, Unit); //Takes 60% of max hp damage, triggers callback, heals to full
			Assert.AreEqual(UnitHealth, Unit.Health);

			Unit.Update(1);
			Unit.TakeDamage(UnitHealth * 0.6f, Unit);
			Assert.AreEqual(UnitHealth * 0.4f, Unit.Health);
		}

		[Test]
		public void Init_RegisterCallbackHealToFullWhenTakingStrongHitRevert_Twice()
		{
			AddRecipe("InitHealToFullHalfHealthCallback")
				.Callback(CallbackType.StrongHit, (target, source) =>
				{
					var damageable = (IDamagable<float, float>)target;
					((IHealable<float, float>)target).Heal(damageable.MaxHealth - damageable.Health, source);
				})
				.Remove(1);
			Setup();

			Unit.AddModifierSelf("InitHealToFullHalfHealthCallback");
			Unit.AddModifierSelf("InitHealToFullHalfHealthCallback");
			Assert.AreEqual(UnitHealth, Unit.Health);

			//Take enough damage to trigger the callback
			Unit.TakeDamage(UnitHealth * 0.6f, Unit); //Takes 60% of max hp damage, triggers callback, heals to full
			Assert.AreEqual(UnitHealth, Unit.Health);

			Unit.Update(1);
			Unit.TakeDamage(UnitHealth * 0.6f, Unit);
			Assert.AreEqual(UnitHealth * 0.4f, Unit.Health);

			Unit.Heal(UnitHealth, Unit);

			Unit.Update(1);
			Unit.TakeDamage(UnitHealth * 0.6f, Unit);
			Assert.AreEqual(UnitHealth * 0.4f, Unit.Health);
		}

		[Test]
		public void Init_RegisterDamageAll_EffectCheckIgnored()
		{
			AddRecipe("InitRegisterAllDamage")
				.EffectCost(CostType.Mana, 5)
				.EffectChance(float.Epsilon)
				.Effect(new DamageEffect(5), EffectOn.CallbackEffect | EffectOn.CallbackUnit)
				.CallbackUnit(CallbackUnitType.StrongDispel)
				.CallbackEffect(CallbackType.CurrentHealthChanged,
					effect => new HealthChangedEvent((target, source, health, deltaHealth) =>
					{
						if (deltaHealth > 0)
							effect.Effect(target, source);
					}))
				.Callback(CallbackType.CurrentHealthChanged,
					new HealthChangedEvent((target, source, health, deltaHealth) =>
					{
						if (deltaHealth > 0)
							source.TakeDamage(5, target);
					}));
			Setup();

			Unit.AddModifierSelf("InitRegisterAllDamage");
			Assert.AreEqual(UnitHealth, Unit.Health);
			Assert.AreEqual(UnitMana, Unit.Mana);

			Unit.TakeDamage(1, Unit);
			Assert.AreEqual(UnitHealth - 1 - 5 * Unit.MaxEventCount - 5 * Unit.MaxEventCount, Unit.Health);
		}

		[Test]
		public void Init_RegisterTimerCallback_TogglableState()
		{
			AddRecipe("AddDamageTogglableBasedOnDistance")
				.Effect(new AddDamageEffect(5, EffectState.IsRevertible | EffectState.IsTogglable),
					EffectOn.CallbackEffectUnits)
				.CallbackEffectUnits(CallbackType.Update, effect => (target, source) =>
				{
					var positionTarget = (IPosition<Vector2>)target;
					var positionSource = (IPosition<Vector2>)source;

					return new UpdateTimerEvent(() =>
					{
						if (positionTarget.Position.DistanceTo(positionSource.Position) < 3f)
							effect.Effect(target, source);
						else
							((IRevertEffect)effect).RevertEffect(target, source);
					});
				});
			Setup();

			//One of a kind setup, usually source and owner are the same
			Enemy.ModifierController.Add(IdManager.GetId("AddDamageTogglableBasedOnDistance").Value, Enemy, Unit);
			Enemy.Update(Unit.CallbackTimerCooldown);
			Assert.AreEqual(EnemyDamage + 5, Enemy.Damage);

			Enemy.Move(3, 3);
			Enemy.Update(Unit.CallbackTimerCooldown);
			Assert.AreEqual(EnemyDamage, Enemy.Damage);

			Enemy.Move(-2, -2);
			Enemy.Update(Unit.CallbackTimerCooldown);
			Assert.AreEqual(EnemyDamage + 5, Enemy.Damage);
		}

		[Test]
		public void MultipleEffectUnitCallbacks()
		{
			AddRecipe("DamageEffectUnitCallbacks")
				.Effect(new DamageEffect(1), EffectOn.CallbackEffectUnits)
				.CallbackEffectUnits(CallbackType.StatusEffectAdded, effect => (origTarget, origSource) =>
					new AddStatusEffectEvent((target, source, duration, statusEffect, oldLegalAction, newLegalAction) =>
					{
						if (statusEffect.HasStatusEffect(StatusEffectType.Stun))
							((ICallbackEffect)effect).CallbackEffect(origTarget, origSource);
					}))
				.Effect(new DamageEffect(2), EffectOn.CallbackEffectUnits2)
				.CallbackEffectUnits(CallbackType.StatusEffectAdded, effect => (origTarget, origSource) =>
					new AddStatusEffectEvent((target, source, duration, statusEffect, oldLegalAction, newLegalAction) =>
					{
						if (statusEffect.HasStatusEffect(StatusEffectType.Freeze))
							((ICallbackEffect)effect).CallbackEffect(origTarget, origSource);
					}))
				.Effect(new DamageEffect(3), EffectOn.CallbackEffectUnits3)
				.CallbackEffectUnits(CallbackType.StatusEffectAdded, effect => (origTarget, origSource) =>
					new AddStatusEffectEvent((target, source, duration, statusEffect, oldLegalAction, newLegalAction) =>
					{
						if (statusEffect.HasStatusEffect(StatusEffectType.Root))
							((ICallbackEffect)effect).CallbackEffect(origTarget, origSource);
					}));
			Setup();

			Unit.AddModifierSelf("DamageEffectUnitCallbacks");
			Unit.StatusEffectController.ChangeStatusEffect(0, 0, StatusEffectType.Stun, 1f, Unit);
			Assert.AreEqual(UnitHealth - 1, Unit.Health);
			Unit.StatusEffectController.ChangeStatusEffect(1, 0, StatusEffectType.Freeze, 1f, Unit);
			Assert.AreEqual(UnitHealth - 1 - 2, Unit.Health);
			Unit.StatusEffectController.ChangeStatusEffect(2, 0, StatusEffectType.Root, 1f, Unit);
			Assert.AreEqual(UnitHealth - 1 - 2 - 3, Unit.Health);
		}
	}
}