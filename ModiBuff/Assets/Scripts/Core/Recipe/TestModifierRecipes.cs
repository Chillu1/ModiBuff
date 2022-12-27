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
				.Effect(new SelfDamageEffect(5), EffectOn.Init);

			Add("DamageApplier_Interval")
				.Effect(new ApplierEffect("InitDamage"), EffectOn.Interval)
				.Interval(1);

			Add("InitSelfHeal_DamageTarget")
				.Effect(new SelfHealEffect(5), EffectOn.Init)
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
				.Effect(new SelfDamageEffect(5), EffectOn.Init)
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

			//Add("InitDamageSelfEvent")
			//	.Effect(new EventEffect(new SelfDamageEffect(5)), EffectOn.Init)
			//	.Remove(5);

			Add("ThornsOnHitEvent")
				.Effect(new EventEffect(new SelfDamageEffect(5), EffectOnEvent.OnHit), EffectOn.Init);

			Add("ThornsOnHitEvent_Remove")
				.Effect(new EventEffect(new SelfDamageEffect(5), EffectOnEvent.OnHit), EffectOn.Init)
				.Remove(5);

			//EventRecipes
			//AddEvent("ThornsOnHitEvent", EffectOnEvent.OnHit)
			//	.Effect(new SelfDamageEffect(5));

			//New stack as parent effect approach, making IEffect stateless, but seems to not work? 
			//Add("IntervalDamage_StackAddDamage")
			//	.Effect(new StackEffectNew(StackEffectType.Add, new DamageEffect(5)), EffectOn.Interval)
			//	.Interval(1)
			//	.Stack(WhenStackEffect.Always, value: 2);

			//Add("InitDamageSelfRemoveEvent")
			//	.Effect(new SelfDamageEffect(5), EffectOn.Init)
			//	.Event(EffectOnEvent.OnHit)
			//	.Remove(5);

			//TODO TargetHeal

			NonTestRecipes();
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
				.Effect(new SelfDamageEffect(5), EffectOn.Init)
				.Effect(new DamageEffect(10), EffectOn.Init);
		}
	}
}