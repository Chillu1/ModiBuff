using System;
using System.Collections.Generic;
using System.Linq;

namespace ModiBuff.Core
{
	public sealed class ModifierGenerator : IModifierGenerator, IModifierApplyCheckGenerator
	{
		public int Id { get; }
		public int GenId { get; private set; }
		public string Name { get; }

		public bool HasApplyChecks { get; }

		private readonly bool _hasEffectChecks;

		private readonly bool _oneTimeInit;

		private readonly float _interval;
		private readonly bool _intervalAffectedByStatusResistance;
		private readonly float _duration;

		private readonly bool _refreshDuration;
		private readonly bool _refreshInterval;

		private readonly WhenStackEffect _whenStackEffect;
		private readonly float _stackValue;
		private readonly int _maxStacks;
		private readonly int _everyXStacks;

		private readonly int _timeComponentCount;
		private int _timeComponentIndex;

		private readonly ModifierEffectsCreator _modifierEffectsCreator;

		private readonly Func<ITargetComponent> _targetComponentFunc;

		private Func<IUnit, bool>[] _applyFuncChecks;
		private IUpdatableCheck[] _updatableApplyChecks;
		private INoUnitCheck[] _noUnitApplyChecks;
		private IUnitCheck[] _unitApplyChecks;
		private IUsableCheck[] _usableApplyChecks;
		private IStateCheck[] _stateApplyChecks;
		private bool _hasStateApplyChecks;

		private Func<IUnit, bool>[] _effectFuncChecks;
		private IUpdatableCheck[] _updatableEffectChecks;
		private INoUnitCheck[] _noUnitEffectChecks;
		private IUnitCheck[] _unitEffectChecks;
		private IUsableCheck[] _usableEffectChecks;
		private IStateCheck[] _stateEffectChecks;
		private bool _hasStateEffectChecks;

		//TODO Refactor
		public ModifierGenerator(int id, string name, List<EffectWrapper> effectWrappers, EffectWrapper removeEffectWrapper,
			bool hasApplyChecks, List<ICheck> applyCheckList, bool hasEffectChecks, List<ICheck> effectCheckList,
			List<Func<IUnit, bool>> applyFuncCheckList, List<Func<IUnit, bool>> effectFuncCheckList, bool isAura, bool oneTimeInit,
			float interval, bool intervalAffectedByStatusResistance, float duration, bool refreshDuration, bool refreshInterval,
			WhenStackEffect whenStackEffect, float stackValue, int maxStacks, int everyXStacks)
		{
			Id = id;
			Name = name;

			HasApplyChecks = hasApplyChecks;
			_hasEffectChecks = hasEffectChecks;

			_oneTimeInit = oneTimeInit;
			_interval = interval;
			_intervalAffectedByStatusResistance = intervalAffectedByStatusResistance;
			_duration = duration;
			_refreshDuration = refreshDuration;
			_refreshInterval = refreshInterval;
			_whenStackEffect = whenStackEffect;
			_stackValue = stackValue;
			_maxStacks = maxStacks;
			_everyXStacks = everyXStacks;

#if DEBUG && !MODIBUFF_PROFILE
			if (effectWrappers.Any(w => w.EffectOn.HasFlag(EffectOn.Interval)) && interval == 0)
				Logger.LogError("Interval not set, but we have interval effects, for modifier: " + Name + " id: " + Id);
			if (effectWrappers.Any(w => w.EffectOn.HasFlag(EffectOn.Duration)) && duration == 0)
				Logger.LogError("Duration not set, but we have duration effects, for modifier: " + Name + " id: " + Id);
#endif
			if (interval > 0)
				_timeComponentCount++;
			if (duration > 0)
				_timeComponentCount++;

			_modifierEffectsCreator = new ModifierEffectsCreator(effectWrappers, removeEffectWrapper);

			if (isAura)
				_targetComponentFunc = () => new MultiTargetComponent();
			else
				_targetComponentFunc = () => new SingleTargetComponent();

			if (HasApplyChecks)
				SetupApplyChecks();

			if (_hasEffectChecks)
				SetupEffectChecks();

			void SetupApplyChecks()
			{
				var updatableChecks = new List<IUpdatableCheck>();
				var noUnitChecks = new List<INoUnitCheck>();
				var unitChecks = new List<IUnitCheck>();
				var usableChecks = new List<IUsableCheck>();
				var stateChecks = new List<IStateCheck>();
				if (applyCheckList != null)
					foreach (var check in applyCheckList)
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

				_applyFuncChecks = applyFuncCheckList?.ToArray();
			}

			void SetupEffectChecks()
			{
				var updatableChecks = new List<IUpdatableCheck>();
				var noUnitChecks = new List<INoUnitCheck>();
				var unitChecks = new List<IUnitCheck>();
				var usableChecks = new List<IUsableCheck>();
				var stateChecks = new List<IStateCheck>();
				if (effectCheckList != null)
					foreach (var check in effectCheckList)
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

				_effectFuncChecks = effectFuncCheckList?.ToArray();
			}
		}

		Modifier IModifierGenerator.Create()
		{
			int genId = GenId++;
			var effects = _modifierEffectsCreator.Create(genId);

			ModifierCheck effectCheck = null;
			if (_hasEffectChecks)
			{
				IStateCheck[] stateChecks = null;
				if (_hasStateEffectChecks)
				{
					stateChecks = new IStateCheck[_stateEffectChecks.Length];
					for (int i = 0; i < _stateEffectChecks.Length; i++)
						stateChecks[i] = (IStateCheck)_stateEffectChecks[i].ShallowClone();
				}

				effectCheck = new ModifierCheck(Id, _effectFuncChecks, _updatableEffectChecks, _noUnitEffectChecks, _unitEffectChecks,
					_usableEffectChecks, stateChecks);
			}

			InitComponent initComponent = null;
			IStackComponent stackComponent = null;
			ITimeComponent[] timeComponents = null;
			if (_timeComponentCount > 0)
			{
				_timeComponentIndex = 0;
				timeComponents = new ITimeComponent[_timeComponentCount];
			}

			if (effects.InitEffects != null)
				initComponent = new InitComponent(_oneTimeInit, effects.InitEffects, effectCheck);
			if (effects.IntervalEffects != null)
				timeComponents[_timeComponentIndex++] = new IntervalComponent(_interval, _refreshInterval, effects.IntervalEffects,
					effectCheck, _intervalAffectedByStatusResistance);
			if (effects.DurationEffects != null)
				timeComponents[_timeComponentIndex++] = new DurationComponent(_duration, _refreshDuration, effects.DurationEffects);
			if (effects.StackEffects != null)
				stackComponent = new StackComponent(_whenStackEffect, _stackValue, _maxStacks, _everyXStacks, effects.StackEffects,
					effectCheck);

			return new Modifier(Id, genId, Name, initComponent, timeComponents, stackComponent, effectCheck, _targetComponentFunc());
		}

		ModifierCheck IModifierApplyCheckGenerator.CreateApplyCheck()
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
	}
}