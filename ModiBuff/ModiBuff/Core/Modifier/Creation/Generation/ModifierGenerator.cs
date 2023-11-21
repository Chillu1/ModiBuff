using System;
using System.Collections.Generic;

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
		private readonly TagType _tag;
		private readonly bool _oneTimeInit;

		private readonly float _interval;
		private readonly float _duration;

		private readonly bool _refreshDuration;
		private readonly bool _refreshInterval;

		private readonly WhenStackEffect _whenStackEffect;
		private readonly int _maxStacks;
		private readonly int _everyXStacks;
		private readonly float _singleStackTime;
		private readonly float _independentStackTime;

		private readonly int _timeComponentCount;
		private int _timeComponentIndex;

		private readonly ModifierEffectsCreator _modifierEffectsCreator;

		private readonly Func<IUnit, bool>[] _applyFuncChecks;
		private readonly List<IUpdatableCheck> _updatableApplyChecksList;
		private readonly List<INoUnitCheck> _noUnitApplyChecksList;
		private readonly List<IUnitCheck> _unitApplyChecksList;
		private readonly List<IUsableCheck> _usableApplyChecksList;
		private readonly IUpdatableCheck[] _updatableApplyChecks;
		private readonly INoUnitCheck[] _noUnitApplyChecks;
		private readonly IUnitCheck[] _unitApplyChecks;
		private readonly IUsableCheck[] _usableApplyChecks;
		private readonly IStateCheck[] _stateApplyChecks;

		private readonly Func<IUnit, bool>[] _effectFuncChecks;
		private readonly List<IUpdatableCheck> _updatableEffectChecksList;
		private readonly List<INoUnitCheck> _noUnitEffectChecksList;
		private readonly List<IUnitCheck> _unitEffectChecksList;
		private readonly List<IUsableCheck> _usableEffectChecksList;
		private readonly IUpdatableCheck[] _updatableEffectChecks;
		private readonly INoUnitCheck[] _noUnitEffectChecks;
		private readonly IUnitCheck[] _unitEffectChecks;
		private readonly IUsableCheck[] _usableEffectChecks;
		private readonly IStateCheck[] _stateEffectChecks;

		public ModifierGenerator(in ModifierRecipeData data)
		{
			Id = data.Id;
			Name = data.Name;

			HasApplyChecks = data.HasApplyChecks;
			_hasEffectChecks = data.HasEffectChecks;

			_isAura = data.IsAura;
			_tag = data.Tag;
			_oneTimeInit = data.OneTimeInit;
			_interval = data.Interval;
			_duration = data.Duration;
			_refreshDuration = data.RefreshDuration;
			_refreshInterval = data.RefreshInterval;
			_whenStackEffect = data.WhenStackEffect;
			_maxStacks = data.MaxStacks;
			_everyXStacks = data.EveryXStacks;
			_singleStackTime = data.SingleStackTime;
			_independentStackTime = data.IndependentStackTime;

			if (data.Interval > 0)
				_timeComponentCount++;
			if (data.Duration > 0)
				_timeComponentCount++;

			_modifierEffectsCreator = new ModifierEffectsCreator(data.EffectWrappers, data.RemoveEffectWrapper,
				data.DispelRegisterWrapper, data.EventRegisterWrapper, data.CallbackUnitRegisterWrapper,
				data.CallbackEffectRegisterWrapper, data.CallbackEffectUnitsRegisterWrapper);

			if (HasApplyChecks)
			{
				_applyFuncChecks = data.ApplyFuncCheckList?.ToArray();
				SetupChecks(data.ApplyCheckList, out _updatableApplyChecksList, out _noUnitApplyChecksList,
					out _unitApplyChecksList, out _usableApplyChecksList, out _updatableApplyChecks,
					out _noUnitApplyChecks, out _unitApplyChecks, out _usableApplyChecks, out _stateApplyChecks);
			}

			if (_hasEffectChecks)
			{
				_effectFuncChecks = data.EffectFuncCheckList?.ToArray();
				SetupChecks(data.EffectCheckList, out _updatableEffectChecksList, out _noUnitEffectChecksList,
					out _unitEffectChecksList, out _usableEffectChecksList, out _updatableEffectChecks,
					out _noUnitEffectChecks, out _unitEffectChecks, out _usableEffectChecks, out _stateEffectChecks);
			}

			return;

			void SetupChecks(in List<ICheck> checksList, out List<IUpdatableCheck> updatableChecksList,
				out List<INoUnitCheck> noUnitChecksList, out List<IUnitCheck> unitChecksList,
				out List<IUsableCheck> usableChecksList, out IUpdatableCheck[] updatableChecks,
				out INoUnitCheck[] noUnitChecks, out IUnitCheck[] unitChecks, out IUsableCheck[] usableChecks,
				out IStateCheck[] stateChecks)
			{
				updatableChecksList = new List<IUpdatableCheck>();
				noUnitChecksList = new List<INoUnitCheck>();
				unitChecksList = new List<IUnitCheck>();
				usableChecksList = new List<IUsableCheck>();
				var stateChecksList = new List<IStateCheck>();
				if (checksList != null)
					foreach (var check in checksList)
					{
						if (check is IStateCheck stateCheck)
						{
							stateChecksList.Add(stateCheck);
							//Special case, we need to clone state checks (because of the state)
							continue;
						}

						if (check is IUpdatableCheck updatableCheck)
							updatableChecksList.Add(updatableCheck);
						if (check is INoUnitCheck noUnitCheck)
							noUnitChecksList.Add(noUnitCheck);
						if (check is IUnitCheck unitCheck)
							unitChecksList.Add(unitCheck);
						if (check is IUsableCheck usableCheck)
							usableChecksList.Add(usableCheck);
					}

				updatableChecks = updatableChecksList.ToArray();
				noUnitChecks = noUnitChecksList.ToArray();
				unitChecks = unitChecksList.ToArray();
				usableChecks = usableChecksList.ToArray();
				stateChecks = stateChecksList.ToArray();
			}
		}

		Modifier IModifierGenerator.Create()
		{
			int genId = GenId++;

			ModifierCheck effectCheck = null;
			if (_hasEffectChecks)
			{
				effectCheck = CreateCheck(_stateEffectChecks, _effectFuncChecks, _updatableEffectChecks,
					_noUnitEffectChecks, _unitEffectChecks, _usableEffectChecks, _updatableEffectChecksList,
					_noUnitEffectChecksList, _unitEffectChecksList, _usableEffectChecksList);
			}

			InitComponent? initComponent = null;
			ITimeComponent[] timeComponents = null;
			StackComponent stackComponent = null;
			if (_timeComponentCount > 0)
			{
				_timeComponentIndex = 0;
				timeComponents = new ITimeComponent[_timeComponentCount];
			}

			var effects = _modifierEffectsCreator.Create(genId);

			if (effects.InitEffects != null)
				initComponent = new InitComponent(_oneTimeInit, effects.InitEffects, effectCheck);

			if (effects.IntervalEffects != null)
				timeComponents[_timeComponentIndex++] = new IntervalComponent(_interval,
					_refreshInterval, effects.IntervalEffects, effectCheck,
					!_tag.HasTag(TagType.IntervalIgnoresStatusResistance));

			if (effects.DurationEffects != null)
				timeComponents[_timeComponentIndex++] = new DurationComponent(_duration,
					_refreshDuration, effects.DurationEffects,
					!_tag.HasTag(TagType.DurationIgnoresStatusResistance));

			if (effects.StackEffects != null)
				stackComponent = new StackComponent(_whenStackEffect, _maxStacks,
					_everyXStacks, _singleStackTime, _independentStackTime, effects.StackEffects, effectCheck);

			var targetComponent = !_isAura ? (ITargetComponent)new SingleTargetComponent() : new MultiTargetComponent();

			return new Modifier(Id, genId, Name, initComponent, timeComponents, stackComponent,
				effectCheck, targetComponent, effects.ModifierStateInfo);
		}

		ModifierCheck IModifierApplyCheckGenerator.CreateApplyCheck()
		{
			return CreateCheck(_stateApplyChecks, _applyFuncChecks, _updatableApplyChecks,
				_noUnitApplyChecks, _unitApplyChecks, _usableApplyChecks, _updatableApplyChecksList,
				_noUnitApplyChecksList, _unitApplyChecksList, _usableApplyChecksList);
		}

		private ModifierCheck CreateCheck(IStateCheck[] stateChecks, Func<IUnit, bool>[] funcChecks,
			IUpdatableCheck[] updatableChecks, INoUnitCheck[] noUnitChecks, IUnitCheck[] unitChecks,
			IUsableCheck[] usableChecks, List<IUpdatableCheck> updatableChecksList, List<INoUnitCheck> noUnitChecksList,
			List<IUnitCheck> unitChecksList, List<IUsableCheck> usableChecksList)
		{
			//If has state checks, need to make a clone of arrays where state checks will be added
			int stateUpdatableChecksCount = 0,
				stateNoUnitChecksCount = 0,
				stateUnitChecksCount = 0,
				stateUsableChecksCount = 0;
			IStateCheck[] clonedStateChecks = null;
			if (stateChecks != null && stateChecks.Length > 0)
			{
				clonedStateChecks = new IStateCheck[stateChecks.Length];
				for (int i = 0; i < stateChecks.Length; i++)
				{
					var stateCheckClone = (IStateCheck)stateChecks[i].ShallowClone();
					if (stateCheckClone is IUpdatableCheck updatableCheck)
					{
						stateUpdatableChecksCount++;
						updatableChecksList.Add(updatableCheck);
					}

					if (stateCheckClone is INoUnitCheck noUnitCheck)
					{
						stateNoUnitChecksCount++;
						noUnitChecksList.Add(noUnitCheck);
					}

					if (stateCheckClone is IUnitCheck unitCheck)
					{
						stateUnitChecksCount++;
						unitChecksList.Add(unitCheck);
					}

					if (stateCheckClone is IUsableCheck usableCheck)
					{
						stateUsableChecksCount++;
						usableChecksList.Add(usableCheck);
					}

					clonedStateChecks[i] = stateCheckClone;
				}
			}

			IUpdatableCheck[] updatableChecksArray;
			if (stateUpdatableChecksCount > 0)
			{
				updatableChecksArray = updatableChecksList.ToArray();
				updatableChecksList.RemoveRange(updatableChecksList.Count - stateUpdatableChecksCount,
					stateUpdatableChecksCount);
			}
			else
				updatableChecksArray = updatableChecks;

			INoUnitCheck[] noUnitChecksArray;
			if (stateNoUnitChecksCount > 0)
			{
				noUnitChecksArray = noUnitChecksList.ToArray();
				noUnitChecksList.RemoveRange(noUnitChecksList.Count - stateNoUnitChecksCount,
					stateNoUnitChecksCount);
			}
			else
				noUnitChecksArray = noUnitChecks;

			IUnitCheck[] unitChecksArray;
			if (stateUnitChecksCount > 0)
			{
				unitChecksArray = unitChecksList.ToArray();
				unitChecksList.RemoveRange(unitChecksList.Count - stateUnitChecksCount,
					stateUnitChecksCount);
			}
			else
				unitChecksArray = unitChecks;

			IUsableCheck[] usableChecksArray;
			if (stateUsableChecksCount > 0)
			{
				usableChecksArray = usableChecksList.ToArray();
				usableChecksList.RemoveRange(usableChecksList.Count - stateUsableChecksCount,
					stateUsableChecksCount);
			}
			else
				usableChecksArray = usableChecks;

			return new ModifierCheck(Id, funcChecks, updatableChecksArray, noUnitChecksArray,
				unitChecksArray, usableChecksArray, clonedStateChecks);
		}
	}
}