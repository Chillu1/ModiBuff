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

		private readonly bool _hasInit;
		private InitComponent _initComponent;

		private readonly ITimeComponent[] _timeComponents;
		private readonly StackComponent _stackComponent;
		private readonly IUpdatable _stackTimerComponent;

		private readonly ModifierCheck _effectCheck;

		private ITargetComponent _targetComponent;
		private bool _isTargetSetup;
		private bool _multiTarget;

		//TODO ideally this would be outside of the modifier, but renting (returning) a tuple/wrapper is kinda meh
		private readonly ModifierStateInfo _effectStateInfo;

		public Modifier(int id, int genId, string name, InitComponent? initComponent,
			ITimeComponent[] timeComponents, StackComponent stackComponent, ModifierCheck effectCheck,
			ITargetComponent targetComponent, ModifierStateInfo effectStateInfo)
		{
			Id = id;
			GenId = genId;
			Name = name;

			if (initComponent != null)
			{
				_initComponent = initComponent.Value;
				_hasInit = true;
			}

			_timeComponents = timeComponents;
			_stackComponent = stackComponent;
			if (stackComponent != null && stackComponent.UsesStackTime)
				_stackTimerComponent = stackComponent;
			_effectCheck = effectCheck;

			_targetComponent = targetComponent;
			if (targetComponent is MultiTargetComponent)
				_multiTarget = true;

			_effectStateInfo = effectStateInfo;
		}

		public void UpdateTargets(List<IUnit> targetsInRange, IUnit source)
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

		public void Init()
		{
#if UNSAFE
			if (_multiTarget)
				_initComponent.Init(Unsafe.As<MultiTargetComponent>(_targetComponent).Targets, _targetComponent.Source);
			else
				_initComponent.Init(Unsafe.As<SingleTargetComponent>(_targetComponent).Target, _targetComponent.Source);
#else
			if (_multiTarget)
				_initComponent.Init(((MultiTargetComponent)_targetComponent).Targets, _targetComponent.Source);
			else
				_initComponent.Init(((SingleTargetComponent)_targetComponent).Target, _targetComponent.Source);
#endif
		}

		public void Update(float deltaTime)
		{
			_effectCheck?.Update(deltaTime);
			_stackTimerComponent?.Update(deltaTime);

			if (_timeComponents == null)
				return;

			int length = _timeComponents.Length;
			for (int i = 0; i < length; i++)
				_timeComponents[i].Update(deltaTime);
		}

		public void Refresh()
		{
			int length = _timeComponents.Length;
			for (int i = 0; i < length; i++)
				_timeComponents[i].Refresh();
		}

		public void Stack() => _stackComponent.Stack();
		public void ResetStacks() => _stackComponent.ResetStacks();

		public ITimeComponent[] GetTimers() => _timeComponents;

		/// <summary>
		///		Gets a timer reference, used to update UI/UX
		/// </summary>
		/// <param name="timeComponentNumber">Which timer should be returned, first = 0</param>'
		//TODO Any way to make sure that references get invalidated when the modifier is pooled?
		public ITimeReference GetTimer<TTimeComponent>(int timeComponentNumber = 0)
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

		public IStackReference GetStackReference()
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
		public TData GetEffectState<TData>(int stateNumber = 0) where TData : struct
		{
			if (_effectStateInfo == null)
			{
				Logger.LogWarning("[ModiBuff] Trying to get state info from a modifier that doesn't have any.");
				return default;
			}

			return _effectStateInfo.GetEffectState<TData>(stateNumber);
		}

		public SaveData SaveState()
		{
			if (_effectStateInfo == null)
			{
				Logger.LogError("[ModiBuff] Trying to get state info from a modifier that doesn't have any.");
				return default;
			}

			var targetSaveData = _targetComponent.SaveState();
			InitComponent.SaveData? initSaveData =
				_hasInit ? (InitComponent.SaveData?)_initComponent.SaveState() : null;
			var stackSaveData = _stackComponent?.SaveState();

			TimeComponentSaveData[] timeComponentsSaveData = null;
			if (_timeComponents != null && _timeComponents.Length > 0)
			{
				timeComponentsSaveData = new TimeComponentSaveData[_timeComponents.Length];
				for (int i = 0; i < _timeComponents.Length; i++)
					timeComponentsSaveData[i] = _timeComponents[i].SaveState();
			}

			//TODO SaveLoad effect checks mutable state

			return new SaveData(Id, targetSaveData, _multiTarget, initSaveData, stackSaveData, timeComponentsSaveData,
				_effectStateInfo.SaveState());
		}

		public void LoadState(SaveData data, IUnit owner)
		{
			_isTargetSetup = false;
			_multiTarget = data.IsMultiTarget;

			//_targetComponent.LoadState(data.TargetSaveData);
			UpdateSingleTargetSource(owner, owner); //TODO Temporary
			// switch (data.TargetSaveData)
			// {
			// 	case SingleTargetComponent.SaveData _:
			// 		//UpdateSingleTargetSource
			// 		break;
			// 	case MultiTargetComponent.SaveData multiSaveData:
			// 		//UpdateTargets(multiSaveData.Targets, multiSaveData.Source);
			// 		break;
			// }

			if (data.InitSaveData != null)
				_initComponent.LoadState(data.InitSaveData.Value);

			if (data.StackSaveData != null)
				_stackComponent.LoadState(data.StackSaveData.Value);

			for (int i = 0; i < _timeComponents?.Length; i++)
				_timeComponents[i].LoadState(data.TimeComponentsSaveData[i]);
			_effectStateInfo.LoadState(data.EffectsSaveData);
		}

		public void ResetState()
		{
			if (_hasInit)
				_initComponent.ResetState();
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

		public override bool Equals(object obj)
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
			public readonly InitComponent.SaveData? InitSaveData;
			public readonly StackComponent.SaveData? StackSaveData;
			public readonly TimeComponentSaveData[] TimeComponentsSaveData;
			public readonly object[] EffectsSaveData;

#if JSON_SERIALIZATION && (NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_1_OR_GREATER || NET5_0_OR_GREATER)
			[System.Text.Json.Serialization.JsonConstructor]
#endif
			public SaveData(int id, object targetSaveData, bool isMultiTarget,
				InitComponent.SaveData? initSaveData, StackComponent.SaveData? stackSaveData,
				TimeComponentSaveData[] timeComponentsSaveData, object[] effectsSaveData)
			{
				Id = id;
				TargetSaveData = targetSaveData;
				IsMultiTarget = isMultiTarget;
				InitSaveData = initSaveData;
				StackSaveData = stackSaveData;
				TimeComponentsSaveData = timeComponentsSaveData;
				EffectsSaveData = effectsSaveData;
			}
		}
	}
}