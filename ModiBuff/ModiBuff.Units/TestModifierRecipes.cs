namespace ModiBuff.Core.Units
{
	public class TestModifierRecipes : ModifierRecipes
	{
		public TestModifierRecipes(ModifierIdManager idManager) : base(idManager)
		{
		}

		protected override void SetupRecipes()
		{
			SetupEventEffect((effects, effectOnEvent) => new EventEffect<EffectOnEvent>(effects, (EffectOnEvent)effectOnEvent));

			Add("NoOpFlag");

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
				.Remove(5).Refresh();

			Add("IntervalRefreshRemove")
				.Effect(new RemoveEffect(), EffectOn.Interval)
				.Interval(5).Refresh();

			Add("DurationRefreshRemove_IntervalDamage")
				.Effect(new DamageEffect(5), EffectOn.Interval)
				.Interval(5)
				.Remove(5).Refresh();

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

			Add("InitHealAction")
				.Effect(new HealActionEffect(), EffectOn.Init);

			Add("InitDamage_CostHealth")
				.ApplyCost(CostType.Health, 5)
				.Effect(new DamageEffect(5), EffectOn.Init);

			Add("Damage_OnHit") //Thorns
				.Effect(new DamageEffect(5), EffectOn.Init); //Register on init?

			Add("InitDamage_Cooldown")
				.ApplyCooldown(1)
				.Effect(new DamageEffect(5), EffectOn.Init);

			Add("InitDamageSelf")
				.Effect(new DamageEffect(5), EffectOn.Init, Targeting.SourceTarget);

			Add("DamageApplier_Interval")
				.Effect(new ApplierEffect("InitDamage"), EffectOn.Interval)
				.Interval(1);

			Add("InitSelfHeal_DamageTarget")
				.Effect(new HealEffect(5), EffectOn.Init, Targeting.SourceTarget)
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
				.ApplyCost(CostType.Mana, 5)
				.Effect(new DamageEffect(5), EffectOn.Init);

			Add("InitStun")
				.Effect(new StatusEffectEffect(StatusEffectType.Stun, 2), EffectOn.Init);

			Add("InitDisarm")
				.Effect(new StatusEffectEffect(StatusEffectType.Disarm, 2), EffectOn.Init);

			Add("InitSilence")
				.Effect(new StatusEffectEffect(StatusEffectType.Silence, 2), EffectOn.Init);

			Add("InitDamageSelfRemove")
				.Effect(new DamageEffect(5), EffectOn.Init, Targeting.SourceTarget)
				.Remove(5);

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
				.Stack(WhenStackEffect.EveryXStacks, everyXStacks: 2);

			Add("InitStun_Revertible")
				.Effect(new StatusEffectEffect(StatusEffectType.Stun, 2, true), EffectOn.Init)
				.Remove(1);

			{
				Register("InitAddDamageBuff_Interval", "InitAddDamageBuff");

				Add("InitAddDamageBuff_Interval")
					.Aura()
					.Interval(1)
					.Effect(new ApplierEffect("InitAddDamageBuff"), EffectOn.Interval);

				Add("InitAddDamageBuff")
					.OneTimeInit()
					.Effect(new AddDamageEffect(5, true), EffectOn.Init)
					.Remove(1.05f).Refresh(); //TODO standardized aura time & aura effects should always be refreshable
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

			Add("InitDamage_Cooldown_Effect")
				.EffectCooldown(1)
				.Effect(new DamageEffect(5), EffectOn.Init);

			Add("InitStackDamage")
				.Effect(new DamageEffect(5, StackEffectType.Effect), EffectOn.Init | EffectOn.Stack)
				.Stack(WhenStackEffect.Always);

			Add("InitDamage_ApplyCondition_HealthAbove100")
				.ApplyCondition(StatType.Health, 100, ComparisonType.GreaterOrEqual)
				.Effect(new DamageEffect(5), EffectOn.Init);

			Add("InitDamage_EffectCondition_HealthAbove100")
				.EffectCondition(StatType.Health, 100, ComparisonType.GreaterOrEqual)
				.Effect(new DamageEffect(5), EffectOn.Init);

			Add("InitDamage_EffectCondition_HealthFull")
				.EffectCondition(ConditionType.HealthIsFull)
				.Effect(new DamageEffect(5), EffectOn.Init);
			{
				Register("InitDamage_EffectCondition_ContainsModifier", "Flag");

				Add("InitDamage_EffectCondition_ContainsModifier")
					.EffectCondition("Flag")
					.Effect(new DamageEffect(5), EffectOn.Init);

				Add("Flag");
			}

			{
				Add("InitFreeze")
					.Effect(new StatusEffectEffect(StatusEffectType.Freeze, 2), EffectOn.Init);

				Add("InitDamage_EffectCondition_FreezeStatusEffect")
					.EffectCondition(StatusEffectType.Freeze)
					.Effect(new DamageEffect(5), EffectOn.Init);

				Add("InitDamage_EffectCondition_ActLegalAction")
					.EffectCondition(LegalAction.Act)
					.Effect(new DamageEffect(5), EffectOn.Init);
			}

			Add("InitDamage_EffectCondition_Combination")
				.EffectCondition("Flag")
				.EffectCondition(StatusEffectType.Freeze)
				.Effect(new DamageEffect(5), EffectOn.Init);

			Add("InitDamage_CostHealth_HealSelf")
				.ApplyCost(CostType.Health, 5)
				.Effect(new DamageEffect(5, StackEffectType.Effect), EffectOn.Init)
				.Effect(new HealEffect(5), EffectOn.Init, Targeting.SourceSource);

			//InitDamageOneTime With1Seconds linger, to not work again (global effect cooldown)
			Add("OneTimeInitDamage_LingerDuration")
				.OneTimeInit()
				.Effect(new DamageEffect(5), EffectOn.Init)
				.Remove(1);

			Add("InitDamage_ApplyCondition_ManaBelow100")
				.ApplyCondition(StatType.Mana, 100, ComparisonType.LessOrEqual)
				.Effect(new DamageEffect(5), EffectOn.Init);

			Add("InitDamage_EffectCondition_ManaFull")
				.EffectCondition(ConditionType.ManaIsFull)
				.Effect(new DamageEffect(5), EffectOn.Init);

			Add("StackHeal")
				.Effect(new HealEffect(5, false, StackEffectType.Effect), EffectOn.Stack)
				.Stack(WhenStackEffect.Always);

			Add("InitDamage_ApplyCondition_HealthFull")
				.ApplyCondition(ConditionType.HealthIsFull)
				.Effect(new DamageEffect(5), EffectOn.Init);

			{
				Register("InitDamage_ApplyCondition_ContainsModifier", "FlagApply");

				Add("InitDamage_ApplyCondition_ContainsModifier")
					.ApplyCondition("FlagApply")
					.Effect(new DamageEffect(5), EffectOn.Init);

				Add("FlagApply");
			}

			{
				Add("InitDamage_ApplyCondition_FreezeStatusEffect")
					.ApplyCondition(StatusEffectType.Freeze)
					.Effect(new DamageEffect(5), EffectOn.Init);

				Add("InitDamage_ApplyCondition_ActLegalAction")
					.ApplyCondition(LegalAction.Act)
					.Effect(new DamageEffect(5), EffectOn.Init);
			}

			Add("InitDamage_ApplyCondition_Combination")
				.ApplyCondition("FlagApply")
				.ApplyCondition(StatusEffectType.Freeze)
				.Effect(new DamageEffect(5), EffectOn.Init);

			Add("DoTRemove")
				.Interval(1)
				.Effect(new DamageEffect(5), EffectOn.Interval)
				.Remove(5);

			Add("DoTRemoveStatusResistance")
				.Interval(1, true)
				.Effect(new DamageEffect(5), EffectOn.Interval)
				.Remove(5);

			Add("InitDamageFullHealth")
				.ApplyCondition(ConditionType.HealthIsFull)
				.Effect(new DamageEffect(5), EffectOn.Init);

			Add("InitDamageLifeStealPost")
				.Effect(new DamageEffect(5).SetPostEffects(new LifeStealPostEffect(0.5f, Targeting.SourceTarget)), EffectOn.Init);

			Add("InitDamageAddDamageOnKillPost")
				.Effect(new DamageEffect(5).SetPostEffects(new AddDamageOnKillPostEffect(2, Targeting.SourceTarget)), EffectOn.Init);

			Add("InitDamageValueBasedOnStatMeta")
				.Effect(new DamageEffect(5).SetMetaEffects(new StatPercentMetaEffect(StatType.Health, Targeting.SourceTarget)),
					EffectOn.Init);

			Add("HealDamageSelfPost")
				.Effect(new HealEffect(5).SetPostEffects(new DamagePostEffect(Targeting.SourceTarget)), EffectOn.Init);

			Add("InitDamageValueBasedOnHealthAndManaMeta")
				.Effect(new DamageEffect(5)
						.SetMetaEffects(
							new StatPercentMetaEffect(StatType.Health, Targeting.SourceTarget),
							new StatPercentMetaEffect(StatType.Mana, Targeting.SourceTarget)),
					EffectOn.Init);

			Add("InitDamageValueBasedOnStatusEffectMeta")
				.Effect(new DamageEffect(5)
						.SetMetaEffects(
							new LegalActionMetaEffect(0.5f, LegalAction.Cast, false),
							new LegalActionMetaEffect(2f, LegalAction.Act, false)),
					EffectOn.Init);

			Add("InitDamageValue2XWhenDisarmedMeta")
				.Effect(new DamageEffect(5)
						.SetMetaEffects(new LegalActionMetaEffect(2f, LegalAction.Act, false, Targeting.SourceTarget)),
					EffectOn.Init);

			Add("InstanceStackableDoT")
				.InstanceStackable()
				.Interval(1)
				.Effect(new DamageEffect(5), EffectOn.Interval)
				.Remove(5);

			Add("InstanceStackableAddDamageRevertible")
				.InstanceStackable()
				.Effect(new AddDamageEffect(5, true), EffectOn.Init)
				.Remove(5);

			Add("InstanceStackableDoTNoRemove")
				.InstanceStackable()
				.Interval(1)
				.Effect(new DamageEffect(5), EffectOn.Interval);

			{
				Register("AddModifierApplierInterval", "AddModifierApplier_Flag");

				Add("AddModifierApplierInterval")
					.Effect(new ApplierEffect("AddModifierApplier_Flag"), EffectOn.Interval)
					.Interval(1);

				Add("AddModifierApplier_Flag");
			}

			{
				Register("AddModifierApplierIntervalApplier", "AddModifierApplierDamage");

				Add("AddModifierApplierIntervalApplier")
					.Effect(new ApplierEffect("AddModifierApplierDamage"), EffectOn.Interval)
					.Interval(1);

				Add("AddModifierApplierDamage")
					.Effect(new DamageEffect(5), EffectOn.Interval)
					.Interval(0.1f);
			}

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
				.Effect(new DamageEffect(5), Targeting.SourceTarget);

			AddEvent("ThornsOnHitEvent_Remove", EffectOnEvent.WhenAttacked)
				.Effect(new DamageEffect(5), Targeting.SourceTarget)
				.Remove(5);

			AddEvent("AddDamage_OnKill_Event", EffectOnEvent.OnKill)
				.Effect(new AddDamageEffect(5), Targeting.SourceTarget);

			AddEvent("Damage_OnDeath_Event", EffectOnEvent.WhenKilled)
				.Effect(new DamageEffect(5), Targeting.SourceTarget);

			AddEvent("Heal_OnHeal_Event", EffectOnEvent.OnHeal)
				.Effect(new HealEffect(5), Targeting.SourceTarget);

			{
				Register("PoisonDoT_OnHit_Event", "PoisonDoT");

				AddEvent("PoisonDoT_OnHit_Event", EffectOnEvent.WhenAttacked)
					.Effect(new ApplierEffect("PoisonDoT"), Targeting.SourceTarget);

				Add("PoisonDoT")
					.Interval(1)
					.Effect(new DamageEffect(5), EffectOn.Interval);
			}
			{
				Register("ComplexApplier_OnHit_Event", "ComplexApplier_Rupture", "ComplexApplier_Disarm");

				//WhenAttacked ApplyModifier. Every5Stacks this modifier adds a new ^
				AddEvent("ComplexApplier_OnHit_Event", EffectOnEvent.WhenAttacked)
					.Effect(new ApplierEffect("ComplexApplier_Rupture"), Targeting.SourceTarget);

				//rupture modifier, that does DoT. When this gets to 5 stacks, apply the disarm effect.
				Add("ComplexApplier_Rupture")
					.Effect(new DamageEffect(5), EffectOn.Interval)
					.Effect(new ApplierEffect("ComplexApplier_Disarm"), EffectOn.Stack)
					.Stack(WhenStackEffect.EveryXStacks, everyXStacks: 5);

				//Disarm the target for 5 seconds. On 2 stacks, removable in 10 seconds, refreshable.
				Add("ComplexApplier_Disarm")
					.Effect(new StatusEffectEffect(StatusEffectType.Disarm, 5, false, StackEffectType.Effect), EffectOn.Stack)
					.Stack(WhenStackEffect.EveryXStacks, everyXStacks: 2)
					.Remove(10).Refresh();
			}
			{
				Register("ComplexApplier2_WhenHealed_Event", "ComplexApplier2_WhenHealed", "ComplexApplier2_OnAttack_Event",
					"ComplexApplier2_WhenAttacked_Event", "ComplexApplier2_AddDamageAdd", "ComplexApplier2_AddDamage");
				//Add damage on 4 stacks buff, that you give someone when they heal you 5 times, for 60 seconds.

				//Apply the modifier to source (healer) WhenHealed
				AddEvent("ComplexApplier2_WhenHealed_Event", EffectOnEvent.WhenHealed)
					.Effect(new ApplierEffect("ComplexApplier2_WhenHealed"), Targeting.SourceTarget);

				//On 5 stacks, apply the modifier to self.
				Add("ComplexApplier2_WhenHealed")
					.Effect(new ApplierEffect("ComplexApplier2_OnAttack_Event"), EffectOn.Stack)
					.Stack(WhenStackEffect.EveryXStacks, everyXStacks: 5)
					.Remove(5).Refresh();

				//Long main buff. Apply the modifier OnAttack.
				AddEvent("ComplexApplier2_OnAttack_Event", EffectOnEvent.OnAttack)
					.Effect(new ApplierEffect("ComplexApplier2_WhenAttacked_Event"))
					.Remove(60).Refresh();

				AddEvent("ComplexApplier2_WhenAttacked_Event", EffectOnEvent.WhenAttacked)
					.Effect(new ApplierEffect("ComplexApplier2_AddDamageAdd"), Targeting.SourceTarget)
					.Remove(5).Refresh();

				//On 4 stacks, Add Damage to Unit source (attacker). TODO Maybe remove the modifier from you/reset stacks?
				Add("ComplexApplier2_AddDamageAdd")
					.Effect(new ApplierEffect("ComplexApplier2_AddDamage"), EffectOn.Stack)
					//.Effect(new RemoveEffect(), EffectOn.Stack)
					.Stack(WhenStackEffect.EveryXStacks, everyXStacks: 4)
					.Remove(5).Refresh();

				//AddDamage 5, one time init, remove in 10 seconds, refreshable.
				Add("ComplexApplier2_AddDamage")
					.OneTimeInit()
					.Effect(new AddDamageEffect(5, true), EffectOn.Init)
					.Remove(10).Refresh();
			}

			AddEvent("InitDamageSelf_BeforeAttack_Event", EffectOnEvent.BeforeAttack)
				.Effect(new DamageEffect(5), Targeting.SourceSource);
		}

		private void NonTestRecipes()
		{
			//Stun every second
			Add("StunEverySecond")
				.Interval(1)
				.Effect(new StatusEffectEffect(StatusEffectType.Stun, 0.2f), EffectOn.Init | EffectOn.Interval)
				.Remove(5).Refresh();

			//Delayed Silence
			Add("DelayedSilence")
				.Effect(new StatusEffectEffect(StatusEffectType.Silence, 1), EffectOn.Duration)
				.Remove(5);

			//Stacking damage, more damage for every stack, removed after 5 seconds, refreshable duration
			{
				Register("StackingDamageApplier", "StackingDamage");

				Add("StackingDamageApplier")
					.Effect(new ApplierEffect("StackingDamage"), EffectOn.Init);

				Add("StackingDamage")
					.Effect(new DamageEffect(5, StackEffectType.Effect | StackEffectType.Add), EffectOn.Stack)
					.Stack(WhenStackEffect.Always, value: 2, maxStacks: -1)
					.Remove(5).Refresh();
			}

			//Self Damage, Damage Target
			Add("SelfDamage")
				.Effect(new DamageEffect(5), EffectOn.Init, Targeting.SourceTarget)
				.Effect(new DamageEffect(10), EffectOn.Init);

			//Full recipe
			Add("Full")
				.OneTimeInit()
				.ApplyCondition(ConditionType.HealthIsFull)
				.ApplyCooldown(1)
				.ApplyCost(CostType.Mana, 5)
				.ApplyChance(0.5f)
				.EffectCondition(ConditionType.HealthIsFull)
				.EffectCooldown(1)
				//.EffectCost(CostType.Mana, 5)
				.EffectChance(0.5f)
				.Effect(new DamageEffect(5), EffectOn.Init)
				.Effect(new DamageEffect(5), EffectOn.Stack)
				.Stack(WhenStackEffect.EveryXStacks, everyXStacks: 2)
				.Interval(1)
				.Effect(new DamageEffect(2), EffectOn.Interval)
				.Remove(5).Refresh()
				.Effect(new DamageEffect(8), EffectOn.Duration);
		}
	}
}