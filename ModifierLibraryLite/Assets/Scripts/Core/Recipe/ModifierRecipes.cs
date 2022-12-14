using System.Collections.Generic;
using System.Linq;

namespace ModifierLibraryLite.Core
{
	public sealed class ModifierRecipes
	{
		public static int RecipesCount { get; private set; }

		private readonly IDictionary<string, ModifierRecipe> _modifiers;

		public ModifierRecipes()
		{
			_modifiers = new Dictionary<string, ModifierRecipe>();

			SetupModifiers();
			RecipesCount = _modifiers.Count;
		}

		//public Modifier Get(string id) => _modifiers[id].Create();

		public ModifierRecipe GetRecipe(string id) => _modifiers[id];
		internal ModifierRecipe GetRecipe(int id) => _modifiers.Values.ElementAt(id);

		internal ModifierRecipe[] GetRecipes() => _modifiers.Values.ToArray();

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

			Add("InitDoTSeparateDamageRemove")
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
				.Stack(WhenStackEffect.Always, 5, false);

			Add("StackAddDamage")
				.Effect(new DamageEffect(5, StackEffectType.Add), EffectOn.Stack)
				.Stack(WhenStackEffect.Always, 5, false);

			Add("ChanceInitDamage")
				.Chance(0.5f)
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
				.Cost(CostType.Health, 5);

			Add("Damage_OnHit") //Thorns
				.Effect(new DamageEffect(5), EffectOn.Init); //Register on init?

			Add("InitDamage_Cooldown")
				.Effect(new DamageEffect(5), EffectOn.Init)
				.Cooldown(1);

			Add("InitDamageSelf")
				.Effect(new SelfDamageEffect(5), EffectOn.Init);

			Add("DamageApplier_Interval")
				.Effect(new ApplierEffect("InitDamage"), EffectOn.Interval)
				.Interval(1);

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