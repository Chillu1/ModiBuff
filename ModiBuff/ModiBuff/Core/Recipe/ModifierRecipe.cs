using System;
using System.Collections.Generic;

namespace ModiBuff.Core
{
	/// <summary>
	///		High level API for creating modifiers.
	/// </summary>
	public sealed class ModifierRecipe : IModifierApplyCheckRecipe, IEquatable<ModifierRecipe>, IComparable<ModifierRecipe>
	{
		public int Id { get; }
		public int GenId { get; private set; }
		public string Name { get; }
		public bool HasApplyChecks { get; private set; }

		public readonly ModifierIdManager IdManager; //TODO Refactor to make it private/not needed

		public bool IsInstanceStackable { get; private set; }
		private bool _isAura;

		private bool _hasEffectChecks;

		private bool _oneTimeInit;

		private bool _currentIsInterval;
		private float _interval;
		private bool _intervalAffectedByStatusResistance;
		private float _duration;

		private EffectWrapper _removeEffectWrapper; //TODO Remove effect can come from other means than Remove function

		private List<EffectWrapper> _effectWrappers;

		private bool _refreshDuration, _refreshInterval;

		private WhenStackEffect _whenStackEffect;
		private float _stackValue;
		private int _maxStacks;
		private int _everyXStacks;

		private List<ITimeComponent> _timeComponents;
		private ModifierCreator _modifierCreator;

		private List<ICheck> _applyCheckList;
		private List<Func<IUnit, bool>> _applyFuncCheckList;
		private Func<IUnit, bool>[] _applyFuncChecks;
		private IUpdatableCheck[] _updatableApplyChecks;
		private INoUnitCheck[] _noUnitApplyChecks;
		private IUnitCheck[] _unitApplyChecks;
		private IUsableCheck[] _usableApplyChecks;
		private IStateCheck[] _stateApplyChecks;
		private bool _hasStateApplyChecks;

		private List<ICheck> _effectCheckList;
		private List<Func<IUnit, bool>> _effectFuncCheckList;
		private Func<IUnit, bool>[] _effectFuncChecks;
		private IUpdatableCheck[] _updatableEffectChecks;
		private INoUnitCheck[] _noUnitEffectChecks;
		private IUnitCheck[] _unitEffectChecks;
		private IUsableCheck[] _usableEffectChecks;
		private IStateCheck[] _stateEffectChecks;
		private bool _hasStateEffectChecks;

		public ModifierRecipe(int id, string name, ModifierIdManager idManager)
		{
			Id = id;
			Name = name;
			IdManager = idManager;

			_effectWrappers = new List<EffectWrapper>(3);
		}

		//---PostFinish---

		ModifierCheck IModifierApplyCheckRecipe.CreateApplyCheck()
		{
			IStateCheck[] stateChecks = null;
			if (_hasStateApplyChecks)
			{
				stateChecks = new IStateCheck[_stateApplyChecks.Length];
				for (int i = 0; i < _stateApplyChecks.Length; i++)
				{
					stateChecks[i] = (IStateCheck)_stateApplyChecks[i].ShallowClone();
					// if (stateCheck is IUsableCheck usableCheck)
					// 	usableChecks.Add(usableCheck);
					// if (stateCheck is IUnitCheck unitCheck)
					// 	unitChecks.Add(unitCheck);
					// if (stateCheck is IUpdatableCheck updatableCheck)
					// 	updatableChecks.Add(updatableCheck);
					// if (stateCheck is INoUnitCheck noUnitCheck)
					// 	noUnitChecks.Add(noUnitCheck);
				}
			}

			return new ModifierCheck(Id, _applyFuncChecks, _updatableApplyChecks, _noUnitApplyChecks, _unitApplyChecks,
				_usableApplyChecks, stateChecks);
		}

		Modifier IModifierRecipe.Create()
		{
			_timeComponents.Clear();
			InitComponent initComponent = null;
			IStackComponent stackComponent = null;

			int genId = GenId++;
			var creation = _modifierCreator.Create(genId);

			IStateCheck[] stateChecks = null;
			if (_hasStateEffectChecks)
			{
				stateChecks = new IStateCheck[_stateEffectChecks.Length];
				for (int i = 0; i < _stateEffectChecks.Length; i++)
					stateChecks[i] = (IStateCheck)_stateEffectChecks[i].ShallowClone();
			}

			ModifierCheck effectCheck = null;
			if (_hasEffectChecks)
				effectCheck = new ModifierCheck(Id, _effectFuncChecks, _updatableEffectChecks, _noUnitEffectChecks, _unitEffectChecks,
					_usableEffectChecks, stateChecks);

			if (creation.InitEffects != null)
				initComponent = new InitComponent(_oneTimeInit, creation.InitEffects, effectCheck);
			if (creation.IntervalEffects != null)
				_timeComponents.Add(new IntervalComponent(_interval, _refreshInterval, creation.IntervalEffects, effectCheck,
					_intervalAffectedByStatusResistance));
			if (creation.DurationEffects != null)
				_timeComponents.Add(new DurationComponent(_duration, _refreshDuration, creation.DurationEffects));
			if (creation.StackEffects != null)
				stackComponent = new StackComponent(_whenStackEffect, _stackValue, _maxStacks, _everyXStacks, creation.StackEffects,
					effectCheck);

			_modifierCreator.Reset();

			return new Modifier(Id, genId, Name, initComponent, _timeComponents.Count == 0 ? null : _timeComponents.ToArray(),
				stackComponent, effectCheck, _isAura ? (ITargetComponent)new MultiTargetComponent() : new SingleTargetComponent());
		}

		//---Misc---

		public ModifierRecipe InstanceStackable()
		{
			IsInstanceStackable = true;
			return this;
		}

		public ModifierRecipe Aura()
		{
			_isAura = true;
			return this;
		}

		//---ApplyChecks---

		public ModifierRecipe ApplyCheck(Func<IUnit, bool> check)
		{
			if (_applyFuncCheckList == null)
				_applyFuncCheckList = new List<Func<IUnit, bool>>();
			_applyFuncCheckList.Add(check);
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


		public ModifierRecipe EffectCheck(Func<IUnit, bool> check)
		{
			if (_effectFuncCheckList == null)
				_effectFuncCheckList = new List<Func<IUnit, bool>>();
			_effectFuncCheckList.Add(check);
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
#if DEBUG && !MODIBUFF_PROFILE
				Logger.LogWarning("Refresh() called without a duration or interval set, defaulting to duration");
#endif
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
#if DEBUG && !MODIBUFF_PROFILE
					Logger.LogError($"Unknown refresh type: {type}");
#endif
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

		public void Finish()
		{
#if DEBUG && !MODIBUFF_PROFILE
			if (_modifierCreator != null)
				Logger.LogError("Modifier recipe already finished, finishing again. Not intended?");
#endif

			_timeComponents = new List<ITimeComponent>(2);
			_modifierCreator = new ModifierCreator(_effectWrappers, _removeEffectWrapper);

			if (HasApplyChecks)
			{
				var updatableChecks = new List<IUpdatableCheck>();
				var noUnitChecks = new List<INoUnitCheck>();
				var unitChecks = new List<IUnitCheck>();
				var usableChecks = new List<IUsableCheck>();
				var stateChecks = new List<IStateCheck>();
				if (_applyCheckList != null)
					foreach (var check in _applyCheckList)
					{
						if (check is IStateCheck stateCheck)
							stateChecks.Add(stateCheck);
						else if (check is IUsableCheck usableCheck)
							usableChecks.Add(usableCheck);
						else if (check is IUnitCheck unitCheck)
							unitChecks.Add(unitCheck);
						else if (check is IUpdatableCheck updatableCheck)
							updatableChecks.Add(updatableCheck);
						else if (check is INoUnitCheck noUnitCheck)
							noUnitChecks.Add(noUnitCheck);
						else
							Logger.LogError("Unknown check type: " + check.GetType());
					}

				_updatableApplyChecks = updatableChecks.ToArray();
				_noUnitApplyChecks = noUnitChecks.ToArray();
				_unitApplyChecks = unitChecks.ToArray();
				_usableApplyChecks = usableChecks.ToArray();
				_stateApplyChecks = stateChecks.ToArray();
				_hasStateApplyChecks = _stateApplyChecks.Length > 0;

				_applyFuncChecks = _applyFuncCheckList?.ToArray();
			}

			if (_hasEffectChecks)
			{
				var updatableChecks = new List<IUpdatableCheck>();
				var noUnitChecks = new List<INoUnitCheck>();
				var unitChecks = new List<IUnitCheck>();
				var usableChecks = new List<IUsableCheck>();
				var stateChecks = new List<IStateCheck>();
				if (_effectCheckList != null)
					foreach (var check in _effectCheckList)
					{
						if (check is IStateCheck stateCheck)
							stateChecks.Add(stateCheck);
						else if (check is IUsableCheck usableCheck)
							usableChecks.Add(usableCheck);
						else if (check is IUnitCheck unitCheck)
							unitChecks.Add(unitCheck);
						else if (check is IUpdatableCheck updatableCheck)
							updatableChecks.Add(updatableCheck);
						else if (check is INoUnitCheck noUnitCheck)
							noUnitChecks.Add(noUnitCheck);
						else
							Logger.LogError("Unknown check type: " + check.GetType());
					}

				_updatableEffectChecks = updatableChecks.ToArray();
				_noUnitEffectChecks = noUnitChecks.ToArray();
				_unitEffectChecks = unitChecks.ToArray();
				_usableEffectChecks = usableChecks.ToArray();
				_stateEffectChecks = stateChecks.ToArray();
				_hasStateEffectChecks = _stateEffectChecks.Length > 0;

				_effectFuncChecks = _effectFuncCheckList?.ToArray();
			}
		}

		public int CompareTo(ModifierRecipe other) => Id.CompareTo(other.Id);

		public bool Equals(ModifierRecipe other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Id == other.Id;
		}

		public override bool Equals(object obj)
		{
			return ReferenceEquals(this, obj) || obj is ModifierRecipe other && Equals(other);
		}

		public override int GetHashCode() => Id;
	}
}