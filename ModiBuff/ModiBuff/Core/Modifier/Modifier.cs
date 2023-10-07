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
	public sealed class Modifier : IModifier, IEquatable<Modifier>, IComparable<Modifier>
	{
		public int Id { get; }
		public int GenId { get; }
		public string Name { get; }

		private readonly bool _hasInit;
		private InitComponent _initComponent;

		private readonly ITimeComponent[] _timeComponents;

		private readonly bool _hasStack;
		private StackComponent _stackComponent;

		private readonly ModifierCheck _effectCheck;

		private ITargetComponent _targetComponent;
		private bool _isTargetSetup;
		private bool _multiTarget;

		//TODO ideally this would be outside of the modifier, but renting (returning) a tuple/wrapper is kinda meh
		private readonly ModifierStateInfo _stateInfo;

		public Modifier(int id, int genId, string name, InitComponent? initComponent,
			ITimeComponent[] timeComponents, StackComponent? stackComponent, ModifierCheck effectCheck,
			ITargetComponent targetComponent, ModifierStateInfo stateInfo)
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
			if (stackComponent != null)
			{
				_stackComponent = stackComponent.Value;
				_hasStack = true;
			}

			_effectCheck = effectCheck;

			_targetComponent = targetComponent;
			if (targetComponent is MultiTargetComponent)
				_multiTarget = true;

			_stateInfo = stateInfo;
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

				if (_hasStack)
					_stackComponent.SetupTarget(_targetComponent);
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

				if (_hasStack)
					_stackComponent.SetupTarget(_targetComponent);
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

		/// <summary>
		///		Gets state from effect
		/// </summary>
		/// <param name="stateNumber">Which state should be returned, 0 = first</param>
		public TData GetState<TData>(int stateNumber = 0) where TData : struct
		{
			if (_stateInfo == null)
			{
				Logger.LogWarning("Trying to get state info from a modifier that doesn't have any.");
				return default;
			}

			return _stateInfo.GetState<TData>(stateNumber);
		}

		public void ResetState()
		{
			if (_hasInit)
				_initComponent.ResetState();
			if (_timeComponents != null)
				for (int i = 0; i < _timeComponents.Length; i++)
					_timeComponents[i].ResetState();
			if (_hasStack)
				_stackComponent.ResetState();
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
	}
}