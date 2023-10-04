namespace ModiBuff.Core.Units
{
	/// <summary>
	///		Example of how to setup recipes for modifier through inheritance
	/// </summary>
	public sealed class TestModifierInheritanceRecipes : ModifierRecipes
	{
		public TestModifierInheritanceRecipes(ModifierIdManager idManager, EventEffectFactory eventEffectFunc = null) :
			base(idManager, eventEffectFunc)
		{
			CreateGenerators();
		}

		protected override void SetupRecipes()
		{
			Add("StunEverySecond")
				.Interval(1)
				.Effect(new StatusEffectEffect(StatusEffectType.Stun, 0.2f), EffectOn.Init | EffectOn.Interval)
				.Remove(5).Refresh();

			Add("InitDamageManual", (id, genId, name) =>
			{
				var initComponent = new InitComponent(false, new IEffect[] { new DamageEffect(5) }, null);

				var modifier = new Modifier(id, genId, name, initComponent, null, default(StackComponent), null,
					new SingleTargetComponent(), null);

				return modifier;
			}, new ModifierAddData(true, false, false, false));

			//Delayed Silence
			Add("DelayedSilence")
				.Effect(new StatusEffectEffect(StatusEffectType.Silence, 1), EffectOn.Duration)
				.Remove(5);

			{
				Register("StackingDamageApplier", "StackingDamage");

				Add("StackingDamageApplier")
					.Effect(new ApplierEffect("StackingDamage"), EffectOn.Init);

				Add("StackingDamage")
					.Effect(new DamageEffect(5, StackEffectType.Effect | StackEffectType.Add), EffectOn.Stack)
					.Stack(WhenStackEffect.Always, value: 2, maxStacks: -1)
					.Remove(5).Refresh();
			}

			AddEvent("ThornsOnHitEvent", EffectOnEvent.WhenAttacked)
				.Effect(new DamageEffect(5), Targeting.SourceTarget);
		}
	}
}