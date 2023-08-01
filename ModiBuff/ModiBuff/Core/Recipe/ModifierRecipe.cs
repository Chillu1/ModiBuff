using System;
using System.Collections.Generic;
using System.Linq;

namespace ModiBuff.Core
{
	/// <summary>
	///		High level API for creating modifiers.
	/// </summary>
	public sealed class ModifierRecipe : IModifierRecipe, IComparable<ModifierRecipe>
	{
		public int Id { get; }
		public string Name { get; }
		public bool HasApplyChecks { get; private set; }

		private readonly ModifierIdManager _idManager;

		public LegalTargetType LegalTargetType { get; private set; } = LegalTargetType.Self;

		private ConditionType _applyConditionType;
		private StatType _applyConditionStatType;
		private float _applyConditionValue = -1;
		private ComparisonType _applyConditionComparisonType;
		private LegalAction _applyConditionLegalAction;
		private StatusEffectType _applyConditionStatusEffect;
		private int _applyConditionModifierId = -1;
		private float _applyCooldown = -1f;

		private bool _hasEffectChecks;
		private ConditionType _effectConditionType;
		private StatType _effectConditionStatType;
		private float _effectConditionValue = -1;
		private ComparisonType _effectConditionComparisonType;
		private LegalAction _effectConditionLegalAction;
		private StatusEffectType _effectConditionStatusEffect;
		private int _effectConditionModifierId = -1;
		private float _effectCooldown = -1f;

		private bool _oneTimeInit;

		private bool _currentIsInterval;
		private float _interval;
		private bool _intervalAffectedByStatusResistance;
		private float _duration;

		private EffectWrapper _removeEffectWrapper;

		private List<EffectWrapper> _effectWrappers;

		private bool _refreshDuration, _refreshInterval;

		private WhenStackEffect _whenStackEffect;
		private float _stackValue;
		private int _maxStacks;
		private int _everyXStacks;

		private List<ITimeComponent> _timeComponents;
		private ModifierCreator _modifierCreator;

		private ConditionCheck _applyCondition;
		private CostCheck _applyCostCheck;
		private ChanceCheck _applyChanceCheck;

		private ConditionCheck _effectCondition;
		private CostCheck _effectCost;
		private ChanceCheck _effectChance;

		public ModifierRecipe(string name, ModifierIdManager idManager)
		{
			Id = idManager.GetFreeId(name);
			Name = name;
			_idManager = idManager;

			_effectWrappers = new List<EffectWrapper>(3);
		}

		//---PostFinish---

		ModifierCheck IModifierRecipe.CreateApplyCheck()
		{
			CooldownCheck cooldown = null;

			if (_applyCooldown > 0f)
				cooldown = new CooldownCheck(_applyCooldown);

			return new ModifierCheck(Id, Name, _applyCondition, cooldown, _applyCostCheck, _applyChanceCheck);
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
				effectCheck = new ModifierCheck(Id, Name, _effectCondition, cooldown, _effectCost, _effectChance);

			if (creation.initEffects.Count > 0)
				initComponent = new InitComponent(_oneTimeInit, creation.initEffects.ToArray(), effectCheck);
			if (creation.intervalEffects.Count > 0)
				_timeComponents.Add(new IntervalComponent(_interval, _refreshInterval, creation.intervalEffects.ToArray(), effectCheck,
					_intervalAffectedByStatusResistance));
			if (creation.durationEffects.Count > 0)
				_timeComponents.Add(new DurationComponent(_duration, _refreshDuration, creation.durationEffects.ToArray()));
			if (creation.stackEffects.Count > 0)
				stackComponent = new StackComponent(_whenStackEffect, _stackValue, _maxStacks, _everyXStacks,
					creation.stackEffects.Cast<IStackEffect>().ToArray(), effectCheck);

			_modifierCreator.Clear();

			return new Modifier(Id, Name, initComponent,
				_timeComponents.Count == 0 ? new ITimeComponent[0] : _timeComponents.ToArray(), stackComponent, effectCheck);
		}

		//---ApplyChecks---

		public ModifierRecipe ApplyCondition(ConditionType conditionType)
		{
			_applyConditionType = conditionType;
			HasApplyChecks = true;
			return this;
		}

		public ModifierRecipe ApplyCondition(StatType statType, float value, ComparisonType comparisonType = ComparisonType.GreaterOrEqual)
		{
			_applyConditionStatType = statType;
			_applyConditionValue = value;
			_applyConditionComparisonType = comparisonType;
			HasApplyChecks = true;
			return this;
		}

		public ModifierRecipe ApplyCondition(LegalAction legalAction)
		{
			_applyConditionLegalAction = legalAction;
			HasApplyChecks = true;
			return this;
		}

		public ModifierRecipe ApplyCondition(StatusEffectType statusEffectType)
		{
			_applyConditionStatusEffect = statusEffectType;
			HasApplyChecks = true;
			return this;
		}

		public ModifierRecipe ApplyCondition(string modifierName)
		{
			_applyConditionModifierId = _idManager.GetId(modifierName);
			HasApplyChecks = true;
			return this;
		}

		/// <summary>
		///		Cooldown set for when we can try to apply the modifier to a target.
		/// </summary>
		public ModifierRecipe ApplyCooldown(float cooldown)
		{
			_applyCooldown = cooldown;
			HasApplyChecks = true;
			return this;
		}

		/// <summary>
		///		Cost for when we can try to apply the modifier to a target.
		/// </summary>
		public ModifierRecipe ApplyCost(CostType costType, float cost)
		{
			_applyCostCheck = new CostCheck(costType, cost);
			HasApplyChecks = true;
			return this;
		}

		/// <summary>
		///		Chance for when trying to apply the modifier to a target.
		/// </summary>
		public ModifierRecipe ApplyChance(float chance)
		{
			if (chance > 1)
				chance /= 100;
			//Debug.Assert(chance >= 0 && chance <= 1, "Chance must be between 0 and 1");//TODO
			_applyChanceCheck = new ChanceCheck(chance);
			HasApplyChecks = true;
			return this;
		}

		//---EffectChecks---

		public ModifierRecipe EffectCondition(ConditionType conditionType)
		{
			_effectConditionType = conditionType;
			_hasEffectChecks = true;
			return this;
		}

		public ModifierRecipe EffectCondition(StatType statType, float value, ComparisonType comparisonType = ComparisonType.GreaterOrEqual)
		{
			_effectConditionStatType = statType;
			_effectConditionValue = value;
			_effectConditionComparisonType = comparisonType;
			_hasEffectChecks = true;
			return this;
		}

		public ModifierRecipe EffectCondition(LegalAction legalAction)
		{
			_effectConditionLegalAction = legalAction;
			_hasEffectChecks = true;
			return this;
		}

		public ModifierRecipe EffectCondition(StatusEffectType statusEffectType)
		{
			_effectConditionStatusEffect = statusEffectType;
			_hasEffectChecks = true;
			return this;
		}

		public ModifierRecipe EffectCondition(string modifierName)
		{
			_effectConditionModifierId = _idManager.GetId(modifierName);
			_hasEffectChecks = true;
			return this;
		}

		public ModifierRecipe EffectCooldown(float cooldown)
		{
			_effectCooldown = cooldown;
			_hasEffectChecks = true;
			return this;
		}

		public ModifierRecipe EffectCost(CostType costType, float cost)
		{
			_effectCost = new CostCheck(costType, cost);
			_hasEffectChecks = true;
			return this;
		}

		public ModifierRecipe EffectChance(float chance)
		{
			if (chance > 1)
				chance /= 100;
			//Debug.Assert(chance >= 0 && chance <= 1, "Chance must be between 0 and 1");//TODO
			_effectChance = new ChanceCheck(chance);
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

		public ModifierRecipe Interval(float interval, bool affectedByStatusResistance = false)
		{
			_interval = interval;
			_intervalAffectedByStatusResistance = affectedByStatusResistance;
			_currentIsInterval = true;
			return this;
		}

		public ModifierRecipe Duration(float duration)
		{
			_duration = duration;
			_currentIsInterval = false;
			return this;
		}

		public ModifierRecipe Remove(float duration)
		{
			Duration(duration);
			_removeEffectWrapper = new EffectWrapper(new RemoveEffect(Id), EffectOn.Duration);
			return this;
		}

		public ModifierRecipe Refresh()
		{
			if (_interval <= 0 && _duration <= 0)
			{
				Logger.LogWarning("Refresh() called without a duration or interval set, defaulting to duration");
				Refresh(RefreshType.Duration);
				return this;
			}

			if (_currentIsInterval)
			{
				Refresh(RefreshType.Interval);
				return this;
			}

			Refresh(RefreshType.Duration);
			return this;
		}

		public ModifierRecipe Refresh(RefreshType type)
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
					Logger.LogError($"Unknown refresh type: {type}");
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

		public ModifierRecipe Effect(IEffect effect, EffectOn effectOn, Targeting targeting = Targeting.TargetSource)
		{
			if (effect is ITargetEffect effectTarget)
				effectTarget.SetTargeting(targeting);
			if (effect is IModifierIdOwner modifierIdOwner)
				modifierIdOwner.SetModifierId(Id);
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
				Logger.LogError("Modifier recipe already finished, finishing again. Not intended?");

			_timeComponents = new List<ITimeComponent>(2);
			_modifierCreator = new ModifierCreator(_effectWrappers);

			if (HasApplyChecks)
				if (_applyConditionType != ConditionType.None
				    || (_applyConditionStatType != StatType.None && _applyConditionValue != -1f)
				    || _applyConditionLegalAction != LegalAction.None
				    || _applyConditionStatusEffect != StatusEffectType.None
				    || _applyConditionModifierId != -1)
					_applyCondition = new ConditionCheck(_applyConditionType, _applyConditionStatType, _applyConditionValue,
						_applyConditionComparisonType, _applyConditionLegalAction, _applyConditionStatusEffect,
						_applyConditionModifierId);

			if (_hasEffectChecks)
				//Checking if condition is legal
				if (_effectConditionType != ConditionType.None
				    || (_effectConditionStatType != StatType.None && _effectConditionValue != -1 &&
				        _effectConditionComparisonType != ComparisonType.None)
				    || _effectConditionLegalAction != LegalAction.None
				    || _effectConditionStatusEffect != StatusEffectType.None
				    || _effectConditionModifierId != -1)
					_effectCondition = new ConditionCheck(_effectConditionType, _effectConditionStatType, _effectConditionValue,
						_effectConditionComparisonType, _effectConditionLegalAction, _effectConditionStatusEffect,
						_effectConditionModifierId);
		}

		public int CompareTo(ModifierRecipe other)
		{
			return Id.CompareTo(other.Id);
		}
	}
}