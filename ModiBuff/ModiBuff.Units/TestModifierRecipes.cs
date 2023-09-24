namespace ModiBuff.Core.Units
{
	public sealed class TestModifierRecipes : ModifierRecipes
	{
		public TestModifierRecipes(ModifierIdManager idManager) : base(idManager)
		{
		}

		protected override void SetupRecipes()
		{
			SetupEventEffect<EffectOnEvent>((effects, effectOnEvent) =>
				new EventEffect<EffectOnEvent>(effects, (EffectOnEvent)effectOnEvent));

			NonTestRecipes();
		}

		private void NonTestRecipes()
		{
			//Stun every second
			Add("StunEverySecond")
				.Interval(1)
				.Effect(new StatusEffectEffect(StatusEffectType.Stun, 0.2f), EffectOn.Init | EffectOn.Interval)
				.Remove(5).Refresh();

			Add("InitDamageManual", (id, genId, name) =>
			{
				var initComponent = new InitComponent(false, new IEffect[] { new DamageEffect(5) }, null);

				var modifier = new Modifier(id, genId, name, initComponent, null, default(StackComponent), null,
					new SingleTargetComponent());

				return modifier;
			}, new ModifierAddData(true, false, false, false));

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