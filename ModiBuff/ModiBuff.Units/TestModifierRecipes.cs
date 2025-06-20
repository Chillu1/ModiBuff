namespace ModiBuff.Core.Units
{
	public sealed class TestModifierRecipes
	{
		private readonly ModifierRecipes _modifierRecipes;

		public TestModifierRecipes(ModifierIdManager idManager, EffectTypeIdManager effectTypeIdManager)
		{
			_modifierRecipes = new ModifierRecipes(idManager, effectTypeIdManager);
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

		private void SetupRecipes()
		{
			Add("StunEverySecond")
				.Interval(1)
				.Effect(new StatusEffectEffect(StatusEffectType.Stun, 0.2f), EffectOn.Init | EffectOn.Interval)
				.Remove(5).Refresh();

			Add("InitDamageManual", (id, genId, name, tag) =>
			{
				var initComponent = new InitComponent(new IEffect[] { new DamageEffect(5) }, null);

				var modifier = new Modifier(id, genId, name, initComponent, null, null, null,
					new SingleTargetComponent(), null, null, null);

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
					.Effect(new DamageEffect(5, false, StackEffectType.Effect | StackEffectType.Add, 2), EffectOn.Stack)
					.Stack(WhenStackEffect.Always)
					.Remove(5).Refresh();
			}

			Add("ThornsOnHitEvent")
				.Effect(new DamageEffect(5, targeting: Targeting.SourceTarget), EffectOn.CallbackUnit)
				.CallbackUnit(CallbackUnitType.WhenAttacked);
		}
	}
}