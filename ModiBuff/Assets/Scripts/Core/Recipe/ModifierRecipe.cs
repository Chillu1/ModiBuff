using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModiBuff.Core
{
	/// <summary>
	///		High level API for creating modifiers.
	/// </summary>
	public sealed class ModifierRecipe : IModifierRecipe, IComparable<ModifierRecipe>
	{
		public int Id { get; }
		public string Name { get; }
		public bool HasChecks { get; private set; }

		public LegalTargetType LegalTargetType { get; private set; } = LegalTargetType.Self;

		private float _applyCooldown = -1f;
		private float _applyCost = -1f;
		private CostCheck _applyCostCheck = null;
		private float _applyChance = -1f;
		private CostType _applyCostType = CostType.None;
		private ChanceCheck _applyChanceCheck = null;

		private bool _hasEffectChecks;
		private float _effectCooldown = -1f;
		private float _effectChance = -1f;
		private CostType _effectCostType = CostType.None;
		private float _effectCost = -1f;

		private bool _oneTimeInit;

		private float _interval;
		private float _duration;

		private RemoveEffect _removeEffect;
		private EffectWrapper _removeEffectWrapper;

		private List<EffectWrapper> _effectWrappers;

		private bool _refreshDuration, _refreshInterval;

		private WhenStackEffect _whenStackEffect;
		private float _stackValue;
		private int _maxStacks;
		private int _everyXStacks;

		private List<ITimeComponent> _timeComponents;
		private ModifierCreator _modifierCreator;

		private CostCheck _effectCostInstance;
		private ChanceCheck _effectChanceInstance;

		public ModifierRecipe(string name)
		{
			Id = ModifierIdManager.GetFreeId(name);
			Name = name;

			_effectWrappers = new List<EffectWrapper>(3);
		}

		//---PostFinish---

		ModifierCheck IModifierRecipe.CreateApplyCheck()
		{
			CooldownCheck cooldown = null;

			if (_applyCooldown > 0f)
				cooldown = new CooldownCheck(_applyCooldown);

			return new ModifierCheck(Id, Name, cooldown, _applyCostCheck, _applyChanceCheck);
		}

		Modifier IModifierRecipe.Create()
		{
			_timeComponents.Clear();
			InitComponent initComponent = null;
			IStackComponent stackComponent = null;

			var creation = _modifierCreator.Create(_removeEffectWrapper);

			CooldownCheck cooldown = null;
			if (_effectCooldown > 0f)
				cooldown = new CooldownCheck(_effectCooldown);
			ModifierCheck effectCheck = null;
			if (_hasEffectChecks)
				effectCheck = new ModifierCheck(Id, Name, cooldown, _effectCostInstance, _effectChanceInstance);

			if (creation.initEffects.Count > 0)
				initComponent = new InitComponent(_oneTimeInit, creation.initEffects.ToArray(), effectCheck);
			if (creation.intervalEffects.Count > 0)
				_timeComponents.Add(new IntervalComponent(_interval, _refreshInterval, creation.intervalEffects.ToArray(), effectCheck));
			if (creation.durationEffects.Count > 0)
				_timeComponents.Add(new DurationComponent(_duration, _refreshDuration, creation.durationEffects.ToArray()));
			if (creation.stackEffects.Count > 0)
				stackComponent = new StackComponent(_whenStackEffect, _stackValue, _maxStacks, _everyXStacks,
					creation.stackEffects.Cast<IStackEffect>().ToArray(), effectCheck);

			_modifierCreator.Clear();

			return new Modifier(Id, Name, initComponent,
				_timeComponents.Count == 0 ? Array.Empty<ITimeComponent>() : _timeComponents.ToArray(), stackComponent, effectCheck);
		}

		//---ApplyChecks---

		/// <summary>
		///		Cooldown set for when we can try to apply the modifier to a target.
		/// </summary>
		public ModifierRecipe ApplyCooldown(float cooldown)
		{
			_applyCooldown = cooldown;
			HasChecks = true;
			return this;
		}

		/// <summary>
		///		Cost for when we can try to apply the modifier to a target.
		/// </summary>
		public ModifierRecipe ApplyCost(CostType costType, float cost)
		{
			_applyCostType = costType;
			_applyCost = cost;
			HasChecks = true;
			return this;
		}

		/// <summary>
		///		Chance for when trying to apply the modifier to a target.
		/// </summary>
		public ModifierRecipe ApplyChance(float chance)
		{
			if (chance > 1)
				chance /= 100;
			Debug.Assert(chance >= 0 && chance <= 1, "Chance must be between 0 and 1");
			_applyChance = chance;
			HasChecks = true;
			return this;
		}

		//---EffectChecks---

		public ModifierRecipe EffectCooldown(float cooldown)
		{
			_effectCooldown = cooldown;
			_hasEffectChecks = true;
			return this;
		}

		public ModifierRecipe EffectCost(CostType costType, float cost)
		{
			_effectCostType = costType;
			_effectCost = cost;
			_hasEffectChecks = true;
			return this;
		}

		public ModifierRecipe EffectChance(float chance)
		{
			if (chance > 1)
				chance /= 100;
			Debug.Assert(chance >= 0 && chance <= 1, "Chance must be between 0 and 1");
			_effectChance = chance;
			_hasEffectChecks = true;
			return this;
		}

		//---Actions---

		/// <summary>
		///		Only trigger Init effects once. When adding modifier.
		/// </summary>
		/// <remarks>Works well for auras</remarks>
		public ModifierRecipe OneTimeInit()
		{
			_oneTimeInit = true;
			return this;
		}

		public ModifierRecipe Interval(float interval)
		{
			_interval = interval;
			return this;
		}

		public ModifierRecipe Duration(float duration)
		{
			_duration = duration;
			return this;
		}

		public ModifierRecipe Remove(float duration)
		{
			Duration(duration);
			_removeEffectWrapper = new EffectWrapper(new RemoveEffect(), EffectOn.Duration);
			return this;
		}

		public ModifierRecipe Refresh(RefreshType type = RefreshType.Duration)
		{
			switch (type)
			{
				case RefreshType.Duration:
					_refreshDuration = true;
					break;
				case RefreshType.Interval:
					_refreshInterval = true;
					break;
				default:
					Debug.LogError($"Unknown refresh type: {type}");
					return this;
			}

			return this;
		}

		public ModifierRecipe Stack(WhenStackEffect whenStackEffect, float value = -1, int maxStacks = -1, int everyXStacks = -1)
		{
			_whenStackEffect = whenStackEffect;
			_stackValue = value;
			_maxStacks = maxStacks;
			_everyXStacks = everyXStacks;
			return this;
		}

		//---Effects---

		public ModifierRecipe Effect(IEffect effect, EffectOn effectOn)
		{
			_effectWrappers.Add(new EffectWrapper(effect, effectOn));
			return this;
		}

		//---Target---
		public ModifierRecipe LegalTarget(LegalTargetType legalTargetType)
		{
			LegalTargetType = legalTargetType;
			return this;
		}

		public void Finish()
		{
			if (_modifierCreator != null)
				Debug.LogError("Modifier recipe already finished, finishing again. Not intended?");

			_timeComponents = new List<ITimeComponent>(2);
			_modifierCreator = new ModifierCreator(_effectWrappers);

			if (_applyCostType != CostType.None && _applyCost > 0)
				_applyCostCheck = new CostCheck(_applyCostType, _applyCost);
			if (_applyChance >= 0f)
				_applyChanceCheck = new ChanceCheck(_applyChance);

			if (_hasEffectChecks)
			{
				if (_effectCostType != CostType.None && _effectCost > 0f)
					_effectCostInstance = new CostCheck(_effectCostType, _effectCost);

				if (_effectChance >= 0f)
					_effectChanceInstance = new ChanceCheck(_effectChance);
			}
		}

		public int CompareTo(ModifierRecipe other)
		{
			return Id.CompareTo(other.Id);
		}
	}
}