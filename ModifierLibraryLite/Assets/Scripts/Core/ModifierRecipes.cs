using System.Collections.Generic;

namespace ModifierLibraryLite.Core
{
	public class ModifierRecipes
	{
		private readonly IDictionary<string, ModifierRecipe> _modifiers;

		public ModifierRecipes()
		{
			_modifiers = new Dictionary<string, ModifierRecipe>();

			SetupModifiers();
		}

		public Modifier Get(string id) => _modifiers[id].Create();

		public ModifierRecipe GetRecipe(string id) => _modifiers[id];

		private ModifierRecipe Add(string id)
		{
			var recipe = new ModifierRecipe(id);
			_modifiers.Add(id, recipe);
			return recipe;
		}

		private void SetupModifiers()
		{
			//For now recipes only supports one interval, one duration.
			Add("InitDoT")
				.Interval(1)
				.Effect(new DamageEffect(10), EffectOn.Init | EffectOn.Interval)
				.Remove(5)
				;

			Add("InitDoTSeparateDamage")
				.Interval(1)
				.Effect(new DamageEffect(10), EffectOn.Init)
				.Effect(new DamageEffect(5), EffectOn.Interval)
				.Remove(5)
				;

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

			//TODO TargetHeal

			foreach (var modifier in _modifiers.Values)
				modifier.Finish();
		}

		private void LowLevelApiModifiers()
		{
			//Low level example
			{
				//string id = "InitDoT";
				//var parameters = new ModifierInternalRecipe(id);
				//var target = new TargetComponent();
				//var damage = new DamageEffect(10);
				//parameters.SetInitComponent(
				//	new InitComponent(target, damage)
				//);
				//parameters.SetTimeComponents(
				//	new IntervalComponent(1, target, damage),
				//	new DurationComponent(5, target, new RemoveEffect())
				//);
				//_modifiers.Add(id, parameters);
			}
		}
	}
}