using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModifierLibraryLite.Core
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

		private bool _init;
		private float _interval;
		private float _duration;

		private RemoveEffect _removeEffect;

		private List<IEffect>[] _effectBinds;

		private bool _refreshDuration, _refreshInterval;
		//private List<bool> _refreshables;

		private WhenStackEffect _whenStackEffect;
		private float _stackValue;
		private int _maxStacks;
		private bool _isRepeatable;
		private int _everyXStacks;

		private ModifierInternalRecipe _internalRecipe;

		public ModifierRecipe(string name)
		{
			Id = ModifierIdManager.GetFreeId(name);
			Name = name;

			var allEffectOns = Enum.GetValues(typeof(EffectOn)).Cast<EffectOn>().ToArray();
			_effectBinds = new List<IEffect>[allEffectOns.Length];
			for (int i = 0; i < allEffectOns.Length; i++)
				_effectBinds[i] = new List<IEffect>(2);
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

		internal Modifier Create() => new Modifier(_internalRecipe);

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
			_removeEffect = new RemoveEffect();
			Effect(_removeEffect, EffectOn.Duration);
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
			if (!Utilities.Utilities.IsPowerOfTwo((int)effectOn))
			{
				foreach (var baseEffectOn in Enum.GetValues(typeof(EffectOn)).Cast<EffectOn>())
				{
					if ((effectOn & baseEffectOn) != 0)
						Effect(effect, baseEffectOn);
				}

				return this;
			}

			_effectBinds[Utilities.Utilities.FastLog2((double)effectOn)].Add(effect);
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
			if (_internalRecipe != null)
				Debug.LogError("Modifier recipe already finished, finishing again. Not intended?");

			InitComponent initComponent = null;
			IList<ITimeComponent> timeComponents = new List<ITimeComponent>(2);
			IStackComponent stackComponent = null;

			var revertibleList = new List<IRevertEffect>(2);

			for (int i = 0; i < _effectBinds.Length; i++)
			{
				var effects = _effectBinds[i];
				if (effects.Count == 0)
					continue;

				for (int j = 0; j < effects.Count; j++)
				{
					if (effects[j] is IRevertEffect { IsRevertible: true } revertEffect)
						revertibleList.Add(revertEffect);
				}

				var effectOn = (EffectOn)(1 << i);

				if (effectOn == EffectOn.Init)
				{
					initComponent = new InitComponent(effects.ToArray());
				}

				if (effectOn == EffectOn.Interval)
				{
					Debug.Assert(_interval > 0, "Interval must be greater than 0");
					timeComponents.Add(new IntervalComponent(_interval, _refreshInterval, effects.ToArray()));
				}

				if (effectOn == EffectOn.Duration)
				{
					Debug.Assert(_duration > 0, "Duration must be greater than 0");
					timeComponents.Add(new DurationComponent(_duration, _refreshDuration, effects.ToArray()));
				}

				if (effectOn == EffectOn.Stack)
				{
					Debug.Assert(_maxStacks == -1 || _maxStacks > 0, "Max stacks must be greater than 0");
					stackComponent = new StackComponent(_whenStackEffect, _stackValue, _maxStacks, _isRepeatable, _everyXStacks,
						effects.Cast<IStackEffect>().ToArray());
				}
			}

			//TODO, the stack effects are getting cloned after this
			_removeEffect?.SetRevertibleEffects(revertibleList.ToArray());

			_internalRecipe = new ModifierInternalRecipe(Id, Name, initComponent, timeComponents.ToArray(), stackComponent);
		}

		public int CompareTo(ModifierRecipe other)
		{
			//Will we have troubles with not comparing references?
			//if (ReferenceEquals(this, other)) return 0;
			//if (ReferenceEquals(null, other)) return 1;
			return Id.CompareTo(other.Id);
		}
	}
}