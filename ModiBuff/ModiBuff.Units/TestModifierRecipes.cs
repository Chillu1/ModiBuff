namespace ModiBuff.Core.Units
{
	public class TestModifierRecipes : ModifierRecipes
	{
		public TestModifierRecipes(ModifierIdManager idManager) : base(idManager)
		{
		}

		protected override void SetupRecipes()
		{
			SetupEventEffect<EffectOnEvent>((effects, effectOnEvent) =>
				new EventEffect<EffectOnEvent>(effects, (EffectOnEvent)effectOnEvent));

			Add("InitDoTSeparateDamageRemove")
				.Interval(1)
				.Effect(new DamageEffect(10), EffectOn.Init)
				.Effect(new DamageEffect(5), EffectOn.Interval)
				.Remove(5);

			Add("StackAddDamage")
				.Effect(new DamageEffect(5, StackEffectType.Add), EffectOn.Stack)
				.Stack(WhenStackEffect.Always);

			Add("InitDamage_RemoveFast")
				.Effect(new DamageEffect(5), EffectOn.Init)
				.Remove(1f);

			Add("Damage_OnHit") //Thorns
				.Effect(new DamageEffect(5), EffectOn.Init); //Register on init?

			Add("InitDamageSelfRemove")
				.Effect(new DamageEffect(5), EffectOn.Init, Targeting.SourceTarget)
				.Remove(5);

			Add("DoT")
				.Interval(1)
				.Effect(new DamageEffect(5), EffectOn.Interval);

			Add("InitStackDamage")
				.Effect(new DamageEffect(5, StackEffectType.Effect), EffectOn.Init | EffectOn.Stack)
				.Stack(WhenStackEffect.Always);

			Add("InitDamage_EffectCondition_HealthAbove100")
				.EffectCondition(StatType.Health, 100, ComparisonType.GreaterOrEqual)
				.Effect(new DamageEffect(5), EffectOn.Init);

			{
				Register("InitDamage_EffectCondition_ContainsModifier", "Flag");

				Add("InitDamage_EffectCondition_ContainsModifier")
					.EffectCondition("Flag")
					.Effect(new DamageEffect(5), EffectOn.Init);

				Add("Flag");
			}

			Add("InitDamage_ApplyCondition_HealthFull")
				.ApplyCondition(ConditionType.HealthIsFull)
				.Effect(new DamageEffect(5), EffectOn.Init);

			Add("InitDamageValueBasedOnStatMeta")
				.Effect(new DamageEffect(5).SetMetaEffects(new StatPercentMetaEffect(StatType.Health, Targeting.SourceTarget)),
					EffectOn.Init);

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

			//Remove at 2 stacks? If reapplied, refresh to 0 stacks. Can't work, we're using stacks for both remove and refresh.
			//Could use two effects instead then. But not ideal
			Add("DamageBonusFor2AttacksRefreshable")
				.Effect(new AddDamageEffect(5), EffectOn.Init)
				.Stack(WhenStackEffect.EveryXStacks, everyXStacks: 2)
				.Remove(5).Refresh();

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
					.Interval(1)
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