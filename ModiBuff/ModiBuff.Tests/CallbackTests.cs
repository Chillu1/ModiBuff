using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;
using TagType = ModiBuff.Core.Units.TagType;

namespace ModiBuff.Tests
{
	public sealed class CallbackTests : ModifierTests
	{
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
		public void AddDamageAbove5RemoveDamageBelow5React()
		{
			AddRecipe("AddDamageAbove5RemoveDamageBelow5React")
				.Effect(new AddDamageEffect(5, EffectState.IsRevertibleAndTogglable), EffectOn.CallbackEffect)
				.CallbackEffect(CallbackType.DamageChanged, effect =>
					new DamageChangedEvent((unit, damage, deltaDamage) =>
					{
						if (damage > 9)
							effect.Effect(unit, unit);
						else
							((IRevertEffect)effect).RevertEffect(unit, unit);
					}));
			Setup();

			Unit.AddModifierSelf("AddDamageAbove5RemoveDamageBelow5React"); //Starts with 10 baseDmg, adds 5 from effect
			Assert.AreEqual(UnitDamage + 5, Unit.Damage);

			//Remove 6 damage, should remove the effect, making it 15 - 6 - 5 = 4
			Unit.AddDamage(-6); //Revert
			Assert.AreEqual(UnitDamage - 6, Unit.Damage);

			Unit.ResetEventCounters();
			Unit.AddDamage(-2); //Make sure that we don't revert twice
			Assert.AreEqual(UnitDamage - 6 - 2, Unit.Damage);
			Unit.ResetEventCounters();
			Unit.AddDamage(2);

			Unit.ResetEventCounters();
			Unit.AddDamage(6);
			Assert.AreEqual(UnitDamage + 5, Unit.Damage);
		}

		[Test]
		public void InitStatusEffectSleep_RemoveOnTenDamageTaken_StateReset()
		{
			AddRecipe("InitStatusEffectSleep_RemoveOnTenDamageTaken")
				.Effect(new StatusEffectEffect(StatusEffectType.Sleep, 5f, true), EffectOn.Init)
				.Remove(RemoveEffectOn.CallbackEffect)
				.CallbackEffect(CallbackType.CurrentHealthChanged, removeEffect =>
				{
					float totalDamageTaken = 0f;
					return new HealthChangedEvent((target, source, health, deltaHealth) =>
					{
						//Don't count "negative damage/healing damage"
						if (deltaHealth > 0)
							totalDamageTaken += deltaHealth;
						if (totalDamageTaken >= 10)
						{
							totalDamageTaken = 0f;
							removeEffect.Effect(target, source);
						}
					});
				});
			Setup();

			Pool.Clear();
			Pool.Allocate(IdManager.GetId("InitStatusEffectSleep_RemoveOnTenDamageTaken"), 1);

			//Starts with 10 baseDmg, adds 5 from effect
			Unit.AddModifierSelf("InitStatusEffectSleep_RemoveOnTenDamageTaken");
			Assert.True(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Sleep));
			Unit.Update(4); //Still has sleep

			Unit.TakeDamage(9, Unit); //Still has sleep
			Assert.True(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Sleep));

			Unit.TakeDamage(2, Unit); //Removes and reverts sleep
			Assert.False(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Sleep));
			Unit.Update(0); //Remove modifier, back to pool

			//Check if state is reset
			Enemy.AddModifierSelf("InitStatusEffectSleep_RemoveOnTenDamageTaken");
			Enemy.TakeDamage(9, Enemy);
			Assert.True(Enemy.StatusEffectController.HasStatusEffect(StatusEffectType.Sleep));

			Enemy.TakeDamage(2, Enemy);
			Assert.False(Enemy.StatusEffectController.HasStatusEffect(StatusEffectType.Sleep));
		}

		[Test]
		public void DispelAddDamageReact()
		{
			AddRecipe("InitStatusEffectSleep_RemoveOnDispel")
				.Tag(TagType.BasicDispel)
				.Effect(new StatusEffectEffect(StatusEffectType.Sleep, 5f, true), EffectOn.Init)
				.Remove(RemoveEffectOn.CallbackEffect)
				.CallbackEffect(CallbackType.Dispel, removeEffect =>
					new DispelEvent((target, source, eventTag) =>
					{
						//TODO We might need to get the modifiers actual tag here?
						//Could feed it through DispelEvent, but that's meh
						if ((TagType.BasicDispel & eventTag) != 0)
							removeEffect.Effect(target, source);
					}));
			Setup();

			Unit.AddModifierSelf("InitStatusEffectSleep_RemoveOnDispel");
			Unit.Dispel(TagType.IsStack | TagType.IsRefresh, Unit);
			Assert.True(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Sleep));
			Unit.Dispel(TagType.BasicDispel, Unit);
			Assert.False(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Sleep));
		}

		[Test]
		public void StunStackingHeal_IntervalStackReset_RefreshIntervalOnStun()
		{
			//TODO If we add the modifier again, it will trigger the stack effect, which we don't want
			//Stack redirection version
			AddRecipe("StunHealStackReset")
				.Tag(Core.TagType.ZeroDefaultStacks)
				.Stack(WhenStackEffect.Always)
				.Effect(new HealEffect(0, HealEffect.EffectState.ValueIsRevertible,
					StackEffectType.Effect | StackEffectType.Add, 5), EffectOn.Stack)
				.CallbackEffect(CallbackType.StatusEffectAdded, effect =>
					new StatusEffectEvent((target, source, appliedStatusEffect, oldLegalAction, newLegalAction) =>
					{
						if (appliedStatusEffect.HasStatusEffect(StatusEffectType.Stun))
							effect.Effect(target, source);
					}))
				.ModifierAction(ModifierAction.ResetStacks, EffectOn.Interval)
				.ModifierAction(ModifierAction.Refresh | ModifierAction.Stack, EffectOn.CallbackEffect)
				.Interval(10).Refresh();
			//Non-stack version
			/*AddRecipe("StunHealStackReset")
				.Tag(Core.TagType.ZeroDefaultStacks)
				.Stack(WhenStackEffect.OnMaxStacks, 10000)
				.Effect(
					new HealEffect(0, HealEffect.EffectState.ValueIsRevertible,
						StackEffectType.Effect | StackEffectType.Add, 5), EffectOn.Stack | EffectOn.CallbackEffect)
				.CallbackEffect(CallbackType.StatusEffectAdded, effect =>
					new StatusEffectEvent((target, source, appliedStatusEffect, oldLegalAction, newLegalAction) =>
					{
						if (appliedStatusEffect.HasStatusEffect(StatusEffectType.Stun))
						{
							if (effect is HealEffect healEffect)
								healEffect.StackEffect(1, target, source);
							if (effect is ModifierActionEffect modifierActionEffect)
								modifierActionEffect.Effect(target, source);
						}
					}))
				.ModifierAction(ModifierAction.ResetStacks, EffectOn.Interval)
				.ModifierAction(ModifierAction.Refresh | ModifierAction.Stack, EffectOn.CallbackEffect)
				.Interval(10).Refresh();*/
			Setup();

			Unit.TakeDamage(UnitHealth / 2f, Unit);
			Unit.AddModifierSelf("StunHealStackReset");

			Unit.StatusEffectController.ChangeStatusEffect(0, 0, StatusEffectType.Stun, 5f, Unit);
			Assert.AreEqual(UnitHealth / 2f + 5, Unit.Health);

			Unit.Update(1);
			Unit.StatusEffectController.ChangeStatusEffect(0, 0, StatusEffectType.Stun, 5f, Unit);
			Assert.AreEqual(UnitHealth / 2f + 5 + 10, Unit.Health);

			Unit.Update(9); //Doesn't reset stacks
			Unit.StatusEffectController.ChangeStatusEffect(0, 1, StatusEffectType.Stun, 5f, Unit);
			Assert.AreEqual(UnitHealth / 2f + 5 + 10 + 15, Unit.Health);

			Unit.Update(10); //Resets stacks
			Unit.StatusEffectController.ChangeStatusEffect(0, 2, StatusEffectType.Stun, 5f, Unit);
			Assert.AreEqual(UnitHealth / 2f + 5 + 10 + 15 + 5, Unit.Health);

			Unit.StatusEffectController.ChangeStatusEffect(0, 2, StatusEffectType.Stun, 5f, Unit);
			Assert.AreEqual(UnitHealth / 2f + 5 + 10 + 15 + 5 + 10, Unit.Health);
		}
	}
}