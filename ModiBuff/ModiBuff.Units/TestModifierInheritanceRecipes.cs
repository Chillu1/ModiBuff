namespace ModiBuff.Core.Units
{
	/// <summary>
	///		Example of how to setup recipes for modifier through inheritance
	/// </summary>
	public sealed class TestModifierInheritanceRecipes : ModifierRecipes
	{
		public TestModifierInheritanceRecipes(ModifierIdManager idManager) : base(idManager)
		{
			CreateGenerators();
		}

		public new static TagType GetTag(int id) => (TagType)ModifierRecipes.GetTag(id);

		protected override void SetupRecipes()
		{
			Add("StunEverySecond")
				.Interval(1)
				.Effect(new StatusEffectEffect(StatusEffectType.Stun, 0.2f), EffectOn.Init | EffectOn.Interval)
				.Remove(5).Refresh();

			Add("InitDamageManual", "Weak Punch", "Deals damage to the target", (id, genId, name, tag) =>
			{
				var initComponent = new InitComponent(false, new IEffect[] { new DamageEffect(5) }, null);

				var modifier = new Modifier(id, genId, name, initComponent, null, null, null,
					new SingleTargetComponent(), null);

				return modifier;
			});

			//Delayed Silence
			Add("DelayedSilence")
				.Effect(new StatusEffectEffect(StatusEffectType.Silence, 1), EffectOn.Duration)
				.Remove(5);

			{
				Register("StackingDamageApplier", "StackingDamage");

				Add("StackingDamageApplier")
					.Effect(new ApplierEffect("StackingDamage"), EffectOn.Init);

				Add("StackingDamage")
					.Effect(new DamageEffect(5, StackEffectType.Effect | StackEffectType.Add, 2), EffectOn.Stack)
					.Stack(WhenStackEffect.Always)
					.Remove(5).Refresh();
			}

			Add("ThornsOnHitEvent")
				.Effect(new DamageEffect(5, targeting: Targeting.SourceTarget), EffectOn.Event)
				.Event(EffectOnEvent.WhenAttacked);
		}
	}
}