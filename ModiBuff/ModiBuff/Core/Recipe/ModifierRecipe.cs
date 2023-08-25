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
		private List<ICheck> _applyCheckList;
		private INoUnitCheck[] _noUnitApplyChecks;
		private IUnitCheck[] _unitApplyChecks;
		private IUsableCheck[] _usableApplyChecks;

		private ConditionCheck _effectCondition;
		private List<ICheck> _effectCheckList;
		private INoUnitCheck[] _noUnitEffectChecks;
		private IUnitCheck[] _unitEffectChecks;
		private IUsableCheck[] _usableEffectChecks;

		public ModifierRecipe(int id, string name, ModifierIdManager idManager)
		{
			Id = id;
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

			return new ModifierCheck(Id, _applyCondition, cooldown, _noUnitApplyChecks, _unitApplyChecks, _usableApplyChecks);
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
				effectCheck = new ModifierCheck(Id, _effectCondition, cooldown, _noUnitEffectChecks, _unitEffectChecks,
					_usableEffectChecks);

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

			return new Modifier(Id, Name, initComponent, _timeComponents.Count == 0 ? null : _timeComponents.ToArray(), stackComponent,
				effectCheck);
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

		public ModifierRecipe ApplyCheck(ICheck check)
		{
			if (_applyCheckList == null)
				_applyCheckList = new List<ICheck>();
			_applyCheckList.Add(check);
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

		public ModifierRecipe EffectCheck(ICheck check)
		{
			if (_effectCheckList == null)
				_effectCheckList = new List<ICheck>();
			_effectCheckList.Add(check);
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

		/// <summary>
		///		How many seconds should pass between the interval effects get applied.
		/// </summary>
		/// <param name="affectedByStatusResistance">Should the interval be affected by status resistance</param>
		public ModifierRecipe Interval(float interval, bool affectedByStatusResistance = false)
		{
			_interval = interval;
			_intervalAffectedByStatusResistance = affectedByStatusResistance;
			_currentIsInterval = true;
			return this;
		}

		/// <summary>
		///		How many seconds should pass before the duration effects get triggered (usually modifier removal)
		/// </summary>
		public ModifierRecipe Duration(float duration)
		{
			_duration = duration;
			_currentIsInterval = false;
			return this;
		}

		/// <summary>
		///		How many seconds should pass before the modifier gets removed.
		/// </summary>
		public ModifierRecipe Remove(float duration)
		{
			Duration(duration);
			_removeEffectWrapper = new EffectWrapper(new RemoveEffect(Id), EffectOn.Duration);
			return this;
		}

		/// <summary>
		///		If a modifier gets applied to a target that already has the modifier, should the interval or duration be reset?
		///		Order matters, call after <see cref="Interval(float,bool)"/> or <see cref="Duration(float)"/>
		/// </summary>
		/// <remarks> This is most often used to refresh duration of the modifier, like refreshing DoT modifiers </remarks>
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

		/// <summary>
		///		If a modifier gets applied to a target that already has the modifier, should the interval or duration be reset?
		/// </summary>
		/// <remarks> This is most often used to refresh duration of the modifier, like refreshing DoT modifiers </remarks>
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

		/// <summary>
		/// 	Adds stack functionality to the modifier. A stack is added every time the modifier gets re-added to the target.
		/// </summary>
		/// <param name="whenStackEffect">When should the stack effects be triggered.</param>
		/// <param name="value">Values that can be used by the stack effects.</param>
		/// <param name="maxStacks">Max amount of stacks that can be applied.</param>
		/// <param name="everyXStacks">If <see cref="whenStackEffect"/> is set to <see cref="whenStackEffect.EveryXStacks"/>, this value will be used to determine when the stack effects should be triggered.</param>
		public ModifierRecipe Stack(WhenStackEffect whenStackEffect, float value = -1, int maxStacks = -1, int everyXStacks = -1)
		{
			_whenStackEffect = whenStackEffect;
			_stackValue = value;
			_maxStacks = maxStacks;
			_everyXStacks = everyXStacks;
			return this;
		}

		//---Effects---

		/// <summary>
		///		Add an effect to the modifier.
		/// </summary>
		/// <param name="effect">Effects that get applied on specific actions (init, stack, interval, duration). </param>
		/// <param name="effectOn">When the effect should trigger (init, stack, interval, duration). Can be multiple.</param>
		/// <param name="targeting">Who should be the target and owner of the applied modifier. For further information, see <see cref="ModiBuff.Core.Targeting"/></param>
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
			{
				var unitChecks = new List<IUnitCheck>();
				var noUnitChecks = new List<INoUnitCheck>();
				var usableChecks = new List<IUsableCheck>();
				if (_applyCheckList != null)
					foreach (var check in _applyCheckList)
					{
						if (check is IUsableCheck usableCheck)
							usableChecks.Add(usableCheck);
						else if (check is IUnitCheck unitCheck)
							unitChecks.Add(unitCheck);
						else if (check is INoUnitCheck noUnitCheck)
							noUnitChecks.Add(noUnitCheck);
						else
							Logger.LogError("Unknown check type: " + check.GetType());
					}

				_unitApplyChecks = unitChecks.ToArray();
				_noUnitApplyChecks = noUnitChecks.ToArray();
				_usableApplyChecks = usableChecks.ToArray();

				_applyCondition = new ConditionCheck(_applyConditionType, _applyConditionStatType, _applyConditionValue,
					_applyConditionComparisonType, _applyConditionLegalAction, _applyConditionStatusEffect,
					_applyConditionModifierId);
			}

			if (_hasEffectChecks)
			{
				var unitChecks = new List<IUnitCheck>();
				var noUnitChecks = new List<INoUnitCheck>();
				var usableChecks = new List<IUsableCheck>();
				if (_effectCheckList != null)
					foreach (var check in _effectCheckList)
					{
						if (check is IUsableCheck usableCheck)
							usableChecks.Add(usableCheck);
						else if (check is IUnitCheck unitCheck)
							unitChecks.Add(unitCheck);
						else if (check is INoUnitCheck noUnitCheck)
							noUnitChecks.Add(noUnitCheck);
						else
							Logger.LogError("Unknown check type: " + check.GetType());
					}

				_unitEffectChecks = unitChecks.ToArray();
				_noUnitEffectChecks = noUnitChecks.ToArray();
				_usableEffectChecks = usableChecks.ToArray();

				_effectCondition = new ConditionCheck(_effectConditionType, _effectConditionStatType, _effectConditionValue,
					_effectConditionComparisonType, _effectConditionLegalAction, _effectConditionStatusEffect,
					_effectConditionModifierId);
			}
		}

		public int CompareTo(ModifierRecipe other)
		{
			return Id.CompareTo(other.Id);
		}
	}
}