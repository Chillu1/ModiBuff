using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModifierLibraryLite.Core
{
	/// <summary>
	///		High level API for creating modifiers.
	/// </summary>
	public sealed class ModifierRecipe : IComparable<ModifierRecipe>
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
		private int _maxStacks;
		private bool _stacksRepeatable;

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

		public ModifierCheck CreateCheck()
		{
			CooldownCheck cooldown = null;
			CostCheck cost = null;
			ChanceCheck chance = null;

			if (_cooldown > 0f)
				cooldown = new CooldownCheck(_cooldown);

			if (_costType != CostType.None && _cost > 0f)
				cost = new CostCheck(_costType, _cost);

			if (_chance >= 0f)
				chance = new ChanceCheck(_chance);

			return new ModifierCheck(Id, Name, cooldown, cost, chance);
		}

		public Modifier Create() => new Modifier(_internalRecipe);

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

		public ModifierRecipe Stack(WhenStackEffect whenStackEffect, int maxStacks, bool repeatable)
		{
			_whenStackEffect = whenStackEffect;
			_maxStacks = maxStacks;
			_stacksRepeatable = repeatable;
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

		public void Finish()
		{
			if (_internalRecipe != null)
				Debug.LogError("Modifier recipe already finished, finishing again. Not intended?");

			IInitComponent initComponent = null;
			IList<ITimeComponent> timeComponents = new List<ITimeComponent>(2);
			IStackComponent stackComponent = null;
			StackEffects stackEffects = null;

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
					Debug.Assert(_maxStacks > 0, "Max stacks must be greater than 0");
					stackComponent = new StackComponent(_whenStackEffect, _maxStacks, _stacksRepeatable);
					stackEffects = new StackEffects(effects.Cast<IStackEffect>().ToArray());
				}
			}

			_removeEffect?.SetRevertibleEffects(revertibleList.ToArray());

			_internalRecipe = new ModifierInternalRecipe(Id, Name, initComponent, timeComponents.ToArray(), stackComponent, stackEffects);
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