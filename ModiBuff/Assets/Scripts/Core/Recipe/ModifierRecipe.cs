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

		private float _cooldown = -1f;
		private float _chance = -1f;
		private CostType _costType = CostType.None;
		private float _cost = -1f;

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
		private bool _isRepeatable;
		private int _everyXStacks;

		private List<ITimeComponent> _timeComponents;
		private ModifierCreator _modifierCreator;

		public ModifierRecipe(string name)
		{
			Id = ModifierIdManager.GetFreeId(name);
			Name = name;

			_effectWrappers = new List<EffectWrapper>(3);
		}

		//---PostFinish---

		internal ModifierCheck CreateCheck()
		{
			CooldownCheck cooldown = null;
			CostCheck cost = null;
			ChanceCheck chance = null;

			if (_cooldown > 0f)
				cooldown = new CooldownCheck(_cooldown);

			//TODO If cost and chance don't have state, we can use the same instance for all IDx modifiers
			if (_costType != CostType.None && _cost > 0f)
				cost = new CostCheck(_costType, _cost);

			if (_chance >= 0f)
				chance = new ChanceCheck(_chance);

			return new ModifierCheck(Id, Name, cooldown, cost, chance);
		}

		internal Modifier Create()
		{
			_timeComponents.Clear();
			InitComponent initComponent = null;
			IStackComponent stackComponent = null;

			var creation = _modifierCreator.Create(_removeEffectWrapper);

			if (creation.initEffects.Count > 0)
				initComponent = new InitComponent(_oneTimeInit, creation.initEffects.ToArray());
			if (creation.intervalEffects.Count > 0)
				_timeComponents.Add(new IntervalComponent(_interval, _refreshInterval, creation.intervalEffects.ToArray()));
			if (creation.durationEffects.Count > 0)
				_timeComponents.Add(new DurationComponent(_duration, _refreshDuration, creation.durationEffects.ToArray()));
			if (creation.stackEffects.Count > 0)
				stackComponent = new StackComponent(_whenStackEffect, _stackValue, _maxStacks, _isRepeatable, _everyXStacks,
					creation.stackEffects.Cast<IStackEffect>().ToArray());

			_modifierCreator.Clear();

			return new Modifier(Id, Name, initComponent,
				_timeComponents.Count == 0 ? Array.Empty<ITimeComponent>() : _timeComponents.ToArray(), stackComponent);
		}

		//---Checks---

		public ModifierRecipe Cooldown(float cooldown)
		{
			_cooldown = cooldown;
			HasChecks = true;
			return this;
		}

		public ModifierRecipe Cost(CostType costType, float cost)
		{
			_costType = costType;
			_cost = cost;
			HasChecks = true;
			return this;
		}

		public ModifierRecipe Chance(float chance)
		{
			if (chance > 1)
				chance /= 100;
			Debug.Assert(chance >= 0 && chance <= 1, "Chance must be between 0 and 1");
			_chance = chance;
			HasChecks = true;
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

		public ModifierRecipe Stack(WhenStackEffect whenStackEffect, float value = -1, int maxStacks = -1, bool repeatable = false,
			int everyXStacks = -1)
		{
			_whenStackEffect = whenStackEffect;
			_stackValue = value;
			_maxStacks = maxStacks;
			_isRepeatable = repeatable;
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

		internal void Finish()
		{
			if (_modifierCreator != null)
				Debug.LogError("Modifier recipe already finished, finishing again. Not intended?");

			_timeComponents = new List<ITimeComponent>(2);
			_modifierCreator = new ModifierCreator(_effectWrappers);
		}

		public int CompareTo(ModifierRecipe other)
		{
			return Id.CompareTo(other.Id);
		}
	}
}