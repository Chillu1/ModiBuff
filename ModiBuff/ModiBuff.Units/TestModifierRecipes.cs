namespace ModiBuff.Core.Units
{
	public sealed class TestModifierRecipes
	{
		private readonly ModifierRecipes _modifierRecipes;

		public TestModifierRecipes(ModifierIdManager idManager)
		{
			_modifierRecipes = new ModifierRecipes(idManager,
				(effects, @event) => new EventEffect<EffectOnEvent>(effects, (EffectOnEvent)@event));
			SetupRecipes();
			_modifierRecipes.CreateGenerators();
		}

		public static TagType GetTag(int id) => (TagType)ModifierRecipes.GetTag(id);

		private void Register(params string[] names) => _modifierRecipes.Register(names);
		private ModifierRecipe Add(string name) => _modifierRecipes.Add(name);

		private void Add(string name, in ModifierGeneratorFunc createFunc, Core.TagType tag = default)
		{
			_modifierRecipes.Add(name, name, "", in createFunc, tag);
		}

		private ModifierEventRecipe AddEvent(string name, EffectOnEvent effectOnEvent) =>
			_modifierRecipes.AddEvent(name, effectOnEvent);

		private void SetupRecipes()
		{
			Add("StunEverySecond")
				.Interval(1)
				.Effect(new StatusEffectEffect(StatusEffectType.Stun, 0.2f), EffectOn.Init | EffectOn.Interval)
				.Remove(5).Refresh();

			Add("InitDamageManual", (id, genId, name) =>
			{
				var initComponent = new InitComponent(false, new IEffect[] { new DamageEffect(5) }, null);

				var modifier = new Modifier(id, genId, name, initComponent, null, null, null,
					new SingleTargetComponent(), null);

				return modifier;
			}, Core.TagType.IsInit);

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