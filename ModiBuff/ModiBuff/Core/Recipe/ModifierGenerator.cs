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

		private readonly bool _isAura;
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

		public ModifierGenerator(in ModifierRecipeData data)
		{
			Id = data.Id;
			Name = data.Name;

			HasApplyChecks = data.HasApplyChecks;
			_hasEffectChecks = data.HasEffectChecks;

			_isAura = data.IsAura;
			_oneTimeInit = data.OneTimeInit;
			_interval = data.Interval;
			_intervalAffectedByStatusResistance = data.IntervalAffectedByStatusResistance;
			_duration = data.Duration;
			_refreshDuration = data.RefreshDuration;
			_refreshInterval = data.RefreshInterval;
			_whenStackEffect = data.WhenStackEffect;
			_stackValue = data.StackValue;
			_maxStacks = data.MaxStacks;
			_everyXStacks = data.EveryXStacks;


#if DEBUG && !MODIBUFF_PROFILE
			if (data.EffectWrappers.Any(w => w.EffectOn.HasFlag(EffectOn.Interval)) && data.Interval == 0)
				Logger.LogError("Interval not set, but we have interval effects, for modifier: " + Name + " id: " + Id);
			if (data.EffectWrappers.Any(w => w.EffectOn.HasFlag(EffectOn.Duration)) && data.Duration == 0)
				Logger.LogError("Duration not set, but we have duration effects, for modifier: " + Name + " id: " + Id);
#endif
			if (data.Interval > 0)
				_timeComponentCount++;
			if (data.Duration > 0)
				_timeComponentCount++;

			_modifierEffectsCreator = new ModifierEffectsCreator(data.EffectWrappers, data.RemoveEffectWrapper);

			if (HasApplyChecks)
				SetupApplyChecks(in data);

			if (_hasEffectChecks)
				SetupEffectChecks(in data);

			return;

			void SetupApplyChecks(in ModifierRecipeData localData)
			{
				var updatableChecks = new List<IUpdatableCheck>();
				var noUnitChecks = new List<INoUnitCheck>();
				var unitChecks = new List<IUnitCheck>();
				var usableChecks = new List<IUsableCheck>();
				var stateChecks = new List<IStateCheck>();
				if (localData.ApplyCheckList != null)
					foreach (var check in localData.ApplyCheckList)
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

				_applyFuncChecks = localData.ApplyFuncCheckList?.ToArray();
			}

			void SetupEffectChecks(in ModifierRecipeData localData)
			{
				var updatableChecks = new List<IUpdatableCheck>();
				var noUnitChecks = new List<INoUnitCheck>();
				var unitChecks = new List<IUnitCheck>();
				var usableChecks = new List<IUsableCheck>();
				var stateChecks = new List<IStateCheck>();
				if (localData.EffectCheckList != null)
					foreach (var check in localData.EffectCheckList)
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

				_effectFuncChecks = localData.EffectFuncCheckList?.ToArray();
			}
		}

		Modifier IModifierGenerator.Create()
		{
			int genId = GenId++;

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

			InitComponent initComponent = default;
			ITimeComponent[] timeComponents = null;
			StackComponent stackComponent = default;
			if (_timeComponentCount > 0)
			{
				_timeComponentIndex = 0;
				timeComponents = new ITimeComponent[_timeComponentCount];
			}

			var effects = _modifierEffectsCreator.Create(genId);

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

			ITargetComponent targetComponent;
			if (!_isAura)
				targetComponent = new SingleTargetComponent();
			else
				targetComponent = new MultiTargetComponent();

			return new Modifier(Id, genId, Name, initComponent, timeComponents, stackComponent, effectCheck, targetComponent);
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