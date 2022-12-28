namespace ModiBuff.Core
{
	public sealed class TestModifierRecipes : ModifierRecipes
	{
		protected override void SetupRecipes()
		{
			//For now recipes only supports one interval, one duration.
			Add("InitDoT")
				.Interval(1)
				.Effect(new DamageEffect(10), EffectOn.Init | EffectOn.Interval)
				.Remove(5);

			Add("InitDoTSeparateDamageRemove")
				.Interval(1)
				.Effect(new DamageEffect(10), EffectOn.Init)
				.Effect(new DamageEffect(5), EffectOn.Interval)
				.Remove(5);

			Add("InitHeal")
				.Effect(new HealEffect(5), EffectOn.Init);

			Add("InitDamage")
				.Effect(new DamageEffect(5), EffectOn.Init);

			Add("InitStrongHeal")
				.Effect(new HealEffect(10), EffectOn.Init);

			Add("InitAddDamage")
				.Effect(new AddDamageEffect(5), EffectOn.Init);

			Add("DurationDamage")
				.Effect(new DamageEffect(5), EffectOn.Duration)
				.Duration(5);

			Add("DurationRemove")
				.Remove(5);

			Add("InitAddDamageRevertible")
				.Effect(new AddDamageEffect(5, true), EffectOn.Init)
				.Remove(5);

			Add("DurationRefreshRemove")
				.Remove(5)
				.Refresh();

			Add("IntervalRefreshRemove")
				.Effect(new RemoveEffect(), EffectOn.Interval)
				.Interval(5)
				.Refresh(RefreshType.Interval);

			Add("DurationRefreshRemove_IntervalDamage")
				.Effect(new DamageEffect(5), EffectOn.Interval)
				.Interval(5)
				.Remove(5)
				.Refresh();

			Add("StackDamage")
				.Effect(new DamageEffect(5, StackEffectType.Effect), EffectOn.Stack)
				.Stack(WhenStackEffect.Always);

			Add("StackAddDamage")
				.Effect(new DamageEffect(5, StackEffectType.Add), EffectOn.Stack)
				.Stack(WhenStackEffect.Always);

			Add("ChanceInitDamage")
				.ApplyChance(0.5f)
				.Effect(new DamageEffect(5), EffectOn.Init);

			Add("InitDamage_RemoveFast")
				.Effect(new DamageEffect(5), EffectOn.Init)
				.Remove(1f);

			Add("IntervalDamage_DurationRemove")
				.Interval(4)
				.Effect(new DamageEffect(5), EffectOn.Interval)
				.Remove(5);

			Add("InitAttackAction")
				.Effect(new AttackActionEffect(), EffectOn.Init);

			Add("InitHealAction")
				.Effect(new HealActionEffect(), EffectOn.Init);

			Add("InitDamage_CostHealth")
				.Effect(new DamageEffect(5), EffectOn.Init)
				.ApplyCost(CostType.Health, 5);

			Add("Damage_OnHit") //Thorns
				.Effect(new DamageEffect(5), EffectOn.Init); //Register on init?

			Add("InitDamage_Cooldown")
				.Effect(new DamageEffect(5), EffectOn.Init)
				.ApplyCooldown(1);

			Add("InitDamageSelf")
				.Effect(new ActerDamageEffect(5), EffectOn.Init);

			Add("DamageApplier_Interval")
				.Effect(new ApplierEffect("InitDamage"), EffectOn.Interval)
				.Interval(1);

			Add("InitSelfHeal_DamageTarget")
				.Effect(new ActerHealEffect(5), EffectOn.Init)
				.Effect(new DamageEffect(5), EffectOn.Init);

			Add("DamageOnMaxStacks")
				.Effect(new DamageEffect(5, StackEffectType.Effect), EffectOn.Stack)
				.Stack(WhenStackEffect.OnMaxStacks, value: -1, maxStacks: 2);

			Add("DamageEveryTwoStacks")
				.Effect(new DamageEffect(5, StackEffectType.Effect), EffectOn.Stack)
				.Stack(WhenStackEffect.EveryXStacks, value: -1, maxStacks: -1, everyXStacks: 2);

			Add("StackBasedDamage")
				.Effect(new DamageEffect(5, StackEffectType.Effect | StackEffectType.Add), EffectOn.Stack)
				.Stack(WhenStackEffect.Always, value: 2);

			Add("StackAddDamageRevertible")
				.Effect(new AddDamageEffect(5, true, StackEffectType.Effect | StackEffectType.Add), EffectOn.Stack)
				.Stack(WhenStackEffect.Always, value: 2)
				.Remove(5);

			Add("InitDamage_CostMana")
				.Effect(new DamageEffect(5), EffectOn.Init)
				.ApplyCost(CostType.Mana, 5);

			Add("InitStun")
				.Effect(new StatusEffectEffect(StatusEffectType.Stun, 2), EffectOn.Init);

			Add("InitDisarm")
				.Effect(new StatusEffectEffect(StatusEffectType.Disarm, 2), EffectOn.Init);

			Add("InitSilence")
				.Effect(new StatusEffectEffect(StatusEffectType.Silence, 2), EffectOn.Init);

			Add("InitDamageSelfRemove")
				.Effect(new ActerDamageEffect(5), EffectOn.Init)
				.Remove(5);

			Add("InitDamageCostMana")
				.ApplyCost(CostType.Mana, 5)
				.Effect(new DamageEffect(5), EffectOn.Init);

			Add("InitShortStun")
				.Effect(new StatusEffectEffect(StatusEffectType.Stun, 1), EffectOn.Init);

			Add("InitShortFreeze")
				.Effect(new StatusEffectEffect(StatusEffectType.Freeze, 1), EffectOn.Init);

			Add("IntervalDamage_StackAddDamage")
				.Effect(new DamageEffect(5, StackEffectType.Add), EffectOn.Interval | EffectOn.Stack)
				.Interval(1)
				.Stack(WhenStackEffect.Always, value: 2);

			Add("StunEveryTwoStacks")
				.Effect(new StatusEffectEffect(StatusEffectType.Stun, 2, false, StackEffectType.Effect), EffectOn.Stack)
				.Stack(WhenStackEffect.EveryXStacks, value: -1, maxStacks: -1, everyXStacks: 2);

			Add("InitStun_Revertible")
				.Effect(new StatusEffectEffect(StatusEffectType.Stun, 2, true), EffectOn.Init)
				.Remove(1);

			{
				Add("InitAddDamageBuff")
					.OneTimeInit()
					.Effect(new AddDamageEffect(5, true), EffectOn.Init)
					.Refresh()
					.Remove(1.05f); //TODO standardized aura time & aura effects should always be refreshable

				Add("InitAddDamageBuff_Interval")
					.Interval(1)
					.Effect(new ApplierEffect("InitAddDamageBuff"), EffectOn.Interval);
			}

			Add("DoT")
				.Interval(1)
				.Effect(new DamageEffect(5), EffectOn.Interval);

			Add("OneTimeInitDamage")
				.OneTimeInit()
				.Effect(new DamageEffect(5), EffectOn.Init);

			Add("ChanceEffectInitDamage")
				.EffectChance(0.5f)
				.Effect(new DamageEffect(5), EffectOn.Init);

			Add("ChanceEffectIntervalDamage")
				.EffectChance(0.5f)
				.Interval(1)
				.Effect(new DamageEffect(5), EffectOn.Interval);

			Add("ChanceEffectDurationDamage")
				.EffectChance(0.5f)
				.Effect(new DamageEffect(5), EffectOn.Duration)
				.Remove(1);

			Add("ChanceEffectStackDamage")
				.EffectChance(0.5f)
				.Effect(new DamageEffect(5, StackEffectType.Effect), EffectOn.Stack)
				.Stack(WhenStackEffect.Always);

			Add("InitDamage_CostManaEffect")
				.EffectCost(CostType.Mana, 5)
				.Effect(new DamageEffect(5), EffectOn.Init);

			Add("InitDamage_Cooldown_Effect")
				.EffectCooldown(1)
				.Effect(new DamageEffect(5), EffectOn.Init);

			Add("InitStackDamage")
				.Effect(new DamageEffect(5, StackEffectType.Effect), EffectOn.Init | EffectOn.Stack)
				.Stack(WhenStackEffect.Always);

			//New stack as parent effect approach, making IEffect stateless, but seems to not work? 
			//Add("IntervalDamage_StackAddDamage")
			//	.Effect(new StackEffectNew(StackEffectType.Add, new DamageEffect(5)), EffectOn.Interval)
			//	.Interval(1)
			//	.Stack(WhenStackEffect.Always, value: 2);

			EventRecipes();

			NonTestRecipes();
		}

		private void EventRecipes()
		{
			AddEvent("ThornsOnHitEvent", EffectOnEvent.WhenAttacked)
				.Effect(new ActerDamageEffect(5));

			AddEvent("ThornsOnHitEvent_Remove", EffectOnEvent.WhenAttacked)
				.Effect(new ActerDamageEffect(5))
				.Remove(5);

			AddEvent("AddDamage_OnKill_Event", EffectOnEvent.OnKill)
				.Effect(new ActerAddDamageEffect(5));

			AddEvent("Damage_OnDeath_Event", EffectOnEvent.WhenKilled)
				.Effect(new ActerDamageEffect(5));

			AddEvent("Heal_OnHeal_Event", EffectOnEvent.OnHeal)
				.Effect(new ActerHealEffect(5));

			AddEvent("AttackSelf_OnHit_Event", EffectOnEvent.WhenAttacked)
				.Effect(new SelfAttackActionEffect());

			{
				Add("PoisonDoT")
					.Interval(1)
					.Effect(new DamageEffect(5), EffectOn.Interval);

				AddEvent("PoisonDoT_OnHit_Event", EffectOnEvent.WhenAttacked)
					.Effect(new ActerApplierEffect("PoisonDoT"));
			}
			{
				//Disarm the target for 5 seconds. On 2 stacks, removable in 10 seconds, refreshable.
				Add("ComplexApplier_Disarm")
					.Effect(new StatusEffectEffect(StatusEffectType.Disarm, 5, false, StackEffectType.Effect), EffectOn.Stack)
					.Stack(WhenStackEffect.EveryXStacks, value: -1, maxStacks: -1, everyXStacks: 2)
					.Remove(10).Refresh();
				//rupture modifier, that does DoT. When this gets to 5 stacks, apply the disarm effect.
				Add("ComplexApplier_Rupture")
					.Effect(new DamageEffect(5), EffectOn.Interval)
					.Effect(new ApplierEffect("ComplexApplier_Disarm"), EffectOn.Stack)
					.Stack(WhenStackEffect.EveryXStacks, value: -1, maxStacks: -1, everyXStacks: 5);
				//WhenAttacked ApplyModifier. Every5Stacks this modifier adds a new ^
				AddEvent("ComplexApplier_OnHit_Event", EffectOnEvent.WhenAttacked)
					.Effect(new ActerApplierEffect("ComplexApplier_Rupture"));
			}
			{
				//Add damage on 4 stacks buff, that you give someone when they heal you 5 times, for 60 seconds.

				//AddDamage 5, one time init, remove in 10 seconds, refreshable.
				Add("ComplexApplier2_AddDamage")
					.OneTimeInit()
					.Effect(new AddDamageEffect(5, true), EffectOn.Init)
					.Remove(10).Refresh();

				//On 4 stacks, Add Damage to Unit acter (attacker). TODO Maybe remove the modifier from you/reset stacks?
				Add("ComplexApplier2_AddDamageAdd")
					.Effect(new ApplierEffect("ComplexApplier2_AddDamage"), EffectOn.Stack)
					//.Effect(new RemoveEffect(), EffectOn.Stack)
					.Stack(WhenStackEffect.EveryXStacks, value: -1, maxStacks: -1, everyXStacks: 4)
					.Remove(5).Refresh();

				AddEvent("ComplexApplier2_WhenAttacked_Event", EffectOnEvent.WhenAttacked)
					.Effect(new ActerApplierEffect("ComplexApplier2_AddDamageAdd"))
					.Remove(5).Refresh();

				//Long main buff. Apply the modifier OnAttack.
				AddEvent("ComplexApplier2_OnAttack_Event", EffectOnEvent.OnAttack)
					.Effect(new ApplierEffect("ComplexApplier2_WhenAttacked_Event"))
					.Remove(60).Refresh();

				//On 5 stacks, apply the modifier to self.
				Add("ComplexApplier2_WhenHealed")
					.Effect(new ApplierEffect("ComplexApplier2_OnAttack_Event"), EffectOn.Stack)
					.Stack(WhenStackEffect.EveryXStacks, value: -1, maxStacks: -1, everyXStacks: 5)
					.Remove(5).Refresh();

				//Apply the modifier to acter (healer) WhenHealed
				AddEvent("ComplexApplier2_WhenHealed_Event", EffectOnEvent.WhenHealed)
					.Effect(new ActerApplierEffect("ComplexApplier2_WhenHealed"));
			}
		}

		private void NonTestRecipes()
		{
			//Stun every second
			Add("StunEverySecond")
				.Interval(1)
				.Effect(new StatusEffectEffect(StatusEffectType.Stun, 0.2f), EffectOn.Init | EffectOn.Interval)
				.Remove(5)
				.Refresh();

			//Delayed Silence
			Add("DelayedSilence")
				.Effect(new StatusEffectEffect(StatusEffectType.Silence, 1), EffectOn.Duration)
				.Remove(5);

			//Stacking damage, more damage for every stack, removed after 5 seconds, refreshable duration
			{
				Add("StackingDamage")
					.Effect(new DamageEffect(5, StackEffectType.Effect | StackEffectType.Add), EffectOn.Stack)
					.Stack(WhenStackEffect.Always, value: 2, maxStacks: -1)
					.Remove(5)
					.Refresh();

				Add("StackingDamageApplier")
					.Effect(new ApplierEffect("StackingDamage"), EffectOn.Init);
			}

			//Self Damage, Damage Target
			Add("SelfDamage")
				.Effect(new ActerDamageEffect(5), EffectOn.Init)
				.Effect(new DamageEffect(10), EffectOn.Init);
		}
	}
}