using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModiBuff.Core
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
			if (_modifiers.ContainsKey(id))
			{
				Debug.LogError($"Modifier with id {id} already exists");
				return _modifiers[id];
			}

			_modifiers.Add(id, recipe);
			return recipe;
		}

		private void SetupModifiers()
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

			Add("InitSelfHeal_DamageTarget")
				.Effect(new SelfHealEffect(5), EffectOn.Init)
				.Effect(new DamageEffect(5), EffectOn.Init);

			Add("DamageOnMaxStacks")
				.Effect(new DamageEffect(5, StackEffectType.Effect), EffectOn.Stack)
				.Stack(WhenStackEffect.OnMaxStacks, value: -1, maxStacks: 2);

			Add("DamageEveryTwoStacks")
				.Effect(new DamageEffect(5, StackEffectType.Effect), EffectOn.Stack)
				.Stack(WhenStackEffect.OnXStacks, value: -1, maxStacks: -1, true, everyXStacks: 2);

			Add("StackBasedDamage")
				.Effect(new DamageEffect(5, StackEffectType.Effect | StackEffectType.Add), EffectOn.Stack)
				.Stack(WhenStackEffect.Always, value: 2);

			Add("StackAddDamageRevertible")
				.Effect(new AddDamageEffect(5, true, StackEffectType.Effect | StackEffectType.Add), EffectOn.Stack)
				.Stack(WhenStackEffect.Always, value: 2)
				.Remove(5);

			Add("InitDamage_CostMana")
				.Effect(new DamageEffect(5), EffectOn.Init)
				.Cost(CostType.Mana, 5);

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
				.Cost(CostType.Mana, 5)
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
				.Stack(WhenStackEffect.OnXStacks, value: -1, maxStacks: -1, true, everyXStacks: 2);

			Add("InitStun_Revertible")
				.Effect(new StatusEffectEffect(StatusEffectType.Stun, 2, true), EffectOn.Init)
				.Remove(1);

			{
				Add("InitAddDamageBuff")
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