#if NET5_0_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NETCOREAPP1_1_OR_GREATER
#define UNSAFE
#endif

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ModiBuff.Tests")]
[assembly: InternalsVisibleTo("ModiBuff.Units")]
[assembly: InternalsVisibleTo("ModiBuff.Benchmarks")]

namespace ModiBuff.Core
{
	public sealed class Modifier : IModifier, IModifierDataReference, IEquatable<Modifier>, IComparable<Modifier>
	{
		public int Id { get; }
		public int GenId { get; }
		public string Name { get; }

		private readonly InitComponent? _initComponent;

		private readonly ITimeComponent[]? _timeComponents;
		private readonly StackComponent? _stackComponent;

		private readonly ModifierCheck? _effectCheck;

		private ITargetComponent _targetComponent;
		private bool _isTargetSetup;
		private bool _multiTarget;

		private readonly ISetDataEffect[] _setDataEffects;

		//TODO ideally this would be outside of the modifier, but renting (returning) a tuple/wrapper is kinda meh
		private readonly EffectStateInfo? _effectStateInfo;
		private readonly EffectSaveState? _effectSaveState;

		public Modifier(int id, int genId, string name, InitComponent? initComponent,
			ITimeComponent[]? timeComponents, StackComponent? stackComponent, ModifierCheck? effectCheck,
			ITargetComponent targetComponent, ISetDataEffect[] setDataEffects, EffectStateInfo? effectStateInfo,
			EffectSaveState? effectSaveState)
		{
			Id = id;
			GenId = genId;
			Name = name;

			_initComponent = initComponent;

			_timeComponents = timeComponents;
			_stackComponent = stackComponent;
			_effectCheck = effectCheck;

			_targetComponent = targetComponent;
			if (targetComponent is MultiTargetComponent)
				_multiTarget = true;

			_setDataEffects = setDataEffects;

			_effectStateInfo = effectStateInfo;
			_effectSaveState = effectSaveState;
		}

		public void UpdateTargets(IList<IUnit> targetsInRange, IUnit source)
		{
			//In case the user switches from single to multi target, which shouldn't be done, cause it causes GC
			if (!_multiTarget)
			{
				_multiTarget = true;
				_targetComponent = new MultiTargetComponent(targetsInRange, source);

				_isTargetSetup = false;
			}

			if (!_isTargetSetup)
			{
				_isTargetSetup = true;

				if (_timeComponents != null)
					for (int i = 0; i < _timeComponents.Length; i++)
						_timeComponents[i].SetupTarget(_targetComponent);

				_stackComponent?.SetupTarget(_targetComponent);
			}

			//TODO state needs to be reset on pool?
			//There's no need to update targets, since it's a reference to the list that shouldn't change
			_targetComponent.Source = source;
			((MultiTargetComponent)_targetComponent).UpdateTargets(targetsInRange);
			if (_timeComponents != null)
				for (int i = 0; i < _timeComponents.Length; i++)
					_timeComponents[i].UpdateTargetStatusResistance();
		}

		public void UpdateSource(IUnit source) => _targetComponent.Source = source;

		public void UpdateSingleTargetSource(IUnit target, IUnit source)
		{
			//In case the user switches from multi to single target, which shouldn't be done, cause it causes GC
			if (_multiTarget)
			{
				_multiTarget = false;
				_targetComponent = new SingleTargetComponent(target, source);

				_isTargetSetup = false;
			}

			if (!_isTargetSetup)
			{
				_isTargetSetup = true;

				if (_timeComponents != null)
					for (int i = 0; i < _timeComponents.Length; i++)
						_timeComponents[i].SetupTarget(_targetComponent);

				_stackComponent?.SetupTarget(_targetComponent);
			}

			_targetComponent.Source = source;
			((SingleTargetComponent)_targetComponent).Target = target;
			if (_timeComponents != null)
				for (int i = 0; i < _timeComponents.Length; i++)
					_timeComponents[i].UpdateTargetStatusResistance();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Init()
		{
#if UNSAFE
			if (_multiTarget)
				_initComponent!.Init(Unsafe.As<MultiTargetComponent>(_targetComponent).Targets,
					_targetComponent.Source);
			else
				_initComponent!.Init(Unsafe.As<SingleTargetComponent>(_targetComponent).Target,
					_targetComponent.Source);
#else
			if (_multiTarget)
				_initComponent.Init(((MultiTargetComponent)_targetComponent).Targets, _targetComponent.Source);
			else
				_initComponent.Init(((SingleTargetComponent)_targetComponent).Target, _targetComponent.Source);
#endif
		}

		/// <summary>
		///		Special init to register callbacks/events on load
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void InitLoad()
		{
#if UNSAFE
			if (_multiTarget)
				_initComponent!.InitLoad(Unsafe.As<MultiTargetComponent>(_targetComponent).Targets,
					_targetComponent.Source);
			else
				_initComponent!.InitLoad(Unsafe.As<SingleTargetComponent>(_targetComponent).Target,
					_targetComponent.Source);
#else
			if (_multiTarget)
				_initComponent.InitLoad(((MultiTargetComponent)_targetComponent).Targets, _targetComponent.Source);
			else
				_initComponent.InitLoad(((SingleTargetComponent)_targetComponent).Target, _targetComponent.Source);
#endif
		}

		public void Update(float deltaTime)
		{
			_effectCheck?.Update(deltaTime);
			if (_stackComponent is { UsesStackTime: true })
				_stackComponent.Update(deltaTime);

			if (_timeComponents == null)
				return;

			for (int i = 0; i < _timeComponents.Length; i++)
				_timeComponents[i].Update(deltaTime);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Refresh()
		{
			for (int i = 0; i < _timeComponents!.Length; i++)
				_timeComponents[i].Refresh();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Stack() => _stackComponent!.Stack();

		public void ResetStacks() => _stackComponent!.ResetStacks();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void UseScheduledCheck() => _effectCheck?.Use(_targetComponent.Source);

		public void SetData(IList<IData> data)
		{
			if (data == null || data.Count == 0)
			{
				Logger.LogError("[ModiBuff] Trying to set data on modifier with no data.");
				return;
			}

			foreach (var d in data)
			{
				switch (d)
				{
					case ModifierData modifierData:
						for (int i = 0; i < _timeComponents?.Length; i++)
							_timeComponents[i].SetData(modifierData);

						_stackComponent?.SetData(modifierData);

						break;
					case EffectData effectData:
						int currentCount = 0;
						bool success = false;
						for (int i = 0; i < _setDataEffects.Length; i++)
						{
							var effect = _setDataEffects[i];
							if (!(effect.GetType() == effectData.EffectType))
								continue;

							if (effectData.EffectNumber > 0 && currentCount++ != effectData.EffectNumber)
								continue;

							effect.SetData(effectData);
							success = true;
							break;
						}

						if (!success)
							Logger.LogError($"[ModiBuff] Couldn't find effect {effectData.EffectType} " +
							                $"at number {effectData.EffectNumber}");

						break;
					default:
						Logger.LogError($"[ModiBuff] Unsupported data type: {d.GetType()}");
						break;
				}
			}
		}

		public ITimeComponent[] GetTimers() => _timeComponents!;

		/// <summary>
		///		Gets a timer reference, used to update UI/UX
		/// </summary>
		/// <param name="timeComponentNumber">Which timer should be returned, first = 0</param>'
		//TODO Any way to make sure that references get invalidated when the modifier is pooled?
		public ITimeReference? GetTimer<TTimeComponent>(int timeComponentNumber = 0)
			where TTimeComponent : ITimeComponent
		{
			if (_timeComponents == null)
			{
				Logger.LogError("[ModiBuff] Trying to get timer from a modifier that doesn't have any.");
				return null;
			}
#if DEBUG && !MODIBUFF_PROFILE
			if (timeComponentNumber < 0 || timeComponentNumber >= _timeComponents.Length)
			{
				Logger.LogError("[ModiBuff] Time component number can't be lower than 0 or higher " +
				                "than time components length");
				return null;
			}
#endif

			int currentNumber = timeComponentNumber;
			for (int i = 0; i < _timeComponents.Length; i++)
			{
				if (!(_timeComponents[i] is TTimeComponent))
					continue;

				if (currentNumber > 0)
				{
					currentNumber--;
					continue;
				}

				return _timeComponents[i];
			}

			Logger.LogError($"[ModiBuff] Couldn't find {typeof(TTimeComponent)} at number {timeComponentNumber}");
			return null;
		}

		public IStackReference? GetStackReference()
		{
			if (_stackComponent == null)
			{
				Logger.LogError("[ModiBuff] Trying to get stack reference from a modifier that doesn't have any.");
				return null;
			}

			return _stackComponent;
		}

		/// <summary>
		///		Gets state from effect
		/// </summary>
		/// <param name="stateNumber">Which state should be returned, 0 = first</param>
		public TData? GetEffectState<TData>(int stateNumber = 0) where TData : struct
		{
			if (_effectStateInfo == null)
			{
				Logger.LogWarning("[ModiBuff] Trying to get state info from a modifier that doesn't have any.");
				return null;
			}

			return _effectStateInfo.Value.GetEffectState<TData>(stateNumber);
		}

		public object[]? GetEffectStates()
		{
			if (_effectStateInfo == null)
			{
				Logger.LogWarning("[ModiBuff] Trying to get effect states from a modifier that doesn't have any.");
				return null;
			}

			return _effectStateInfo.Value.GetEffectStates();
		}

		public SaveData SaveState()
		{
			var targetSaveData = _targetComponent.SaveState();
			var stackSaveData = _stackComponent?.SaveState();

			TimeComponentSaveData[]? timeComponentsSaveData = null;
			if (_timeComponents != null && _timeComponents.Length > 0)
			{
				timeComponentsSaveData = new TimeComponentSaveData[_timeComponents.Length];
				for (int i = 0; i < _timeComponents.Length; i++)
					timeComponentsSaveData[i] = _timeComponents[i].SaveState();
			}

			var effectCheckSaveData = _effectCheck?.SaveState();
			var effectSaveState = _effectSaveState?.SaveState();

			return new SaveData(Id, targetSaveData, _multiTarget, effectCheckSaveData, stackSaveData,
				timeComponentsSaveData, effectSaveState);
		}

		public void LoadState(SaveData data, IUnit owner)
		{
			_isTargetSetup = false;
			_multiTarget = data.IsMultiTarget;

#if MODIBUFF_SYSTEM_TEXT_JSON
			if (!data.TargetSaveData.FromAnonymousJsonObjectToSaveData(_targetComponent))
#endif
			{
				_targetComponent.LoadState(data.TargetSaveData);
			}

			switch (_targetComponent)
			{
				case SingleTargetComponent singleTargetComponent:
					UpdateSingleTargetSource(singleTargetComponent.Target, _targetComponent.Source);
					break;
				case MultiTargetComponent multiTargetComponent:
					UpdateTargets(multiTargetComponent.Targets, _targetComponent.Source);
					break;
				default:
					Logger.LogError("[ModiBuff] Trying to load target component that isn't single or multi target");
					break;
			}

			if (data.StackSaveData != null)
				_stackComponent!.LoadState(data.StackSaveData.Value);

			for (int i = 0; i < _timeComponents?.Length; i++)
				_timeComponents[i].LoadState(data.TimeComponentsSaveData![i]);

			if (data.EffectCheckSaveData != null)
				_effectCheck?.LoadState(data.EffectCheckSaveData.Value);

			if (data.EffectsSaveData != null)
				_effectSaveState!.Value.LoadState(data.EffectsSaveData);
		}

		public void ResetState()
		{
			_initComponent?.ResetState();
			if (_timeComponents != null)
				for (int i = 0; i < _timeComponents.Length; i++)
					_timeComponents[i].ResetState();
			_stackComponent?.ResetState();
			_effectCheck?.ResetState();
			_targetComponent.ResetState();
		}

		public bool Equals(Modifier other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Id == other.Id && GenId == other.GenId;
		}

		public override bool Equals(object? obj)
		{
			return ReferenceEquals(this, obj) || obj is Modifier other && Equals(other);
		}

		public int CompareTo(Modifier other)
		{
			if (ReferenceEquals(this, other)) return 0;
			if (ReferenceEquals(null, other)) return 1;
			int idComparison = Id.CompareTo(other.Id);
			if (idComparison != 0) return idComparison;
			return GenId.CompareTo(other.GenId);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (Id * 397) ^ GenId;
			}
		}

		public readonly struct SaveData
		{
			public readonly int Id;
			public readonly object TargetSaveData;
			public readonly bool IsMultiTarget;
			public readonly ModifierCheck.SaveData? EffectCheckSaveData;
			public readonly StackComponent.SaveData? StackSaveData;
			public readonly IReadOnlyList<TimeComponentSaveData>? TimeComponentsSaveData;
			public readonly IReadOnlyList<EffectSaveState.EffectSaveData>? EffectsSaveData;

#if MODIBUFF_SYSTEM_TEXT_JSON
			[System.Text.Json.Serialization.JsonConstructor]
#endif
			public SaveData(int id, object targetSaveData, bool isMultiTarget,
				ModifierCheck.SaveData? effectCheckSaveData, StackComponent.SaveData? stackSaveData,
				IReadOnlyList<TimeComponentSaveData>? timeComponentsSaveData,
				IReadOnlyList<EffectSaveState.EffectSaveData>? effectsSaveData)
			{
				Id = id;
				TargetSaveData = targetSaveData;
				IsMultiTarget = isMultiTarget;
				EffectCheckSaveData = effectCheckSaveData;
				StackSaveData = stackSaveData;
				TimeComponentsSaveData = timeComponentsSaveData;
				EffectsSaveData = effectsSaveData;
			}
		}
	}
}