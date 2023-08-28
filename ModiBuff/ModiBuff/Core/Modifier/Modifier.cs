using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ModiBuff.Tests")]
[assembly: InternalsVisibleTo("ModiBuff.Units")]
[assembly: InternalsVisibleTo("ModiBuff.Benchmarks")]

namespace ModiBuff.Core
{
	public sealed class Modifier : IModifier
	{
		public int Id { get; }
		public int GenId { get; }
		public string Name { get; }

		private readonly bool _refresh;

		private readonly InitComponent _initComponent;

		private readonly ITimeComponent[] _timeComponents;

		private readonly IStackComponent _stackComponent;

		private readonly ModifierCheck _effectCheck;
		private readonly bool _check;

		private ITargetComponent _targetComponent;
		private bool _isTargetSetup;
		private bool _multiTarget;

		public Modifier(int id, int genId, string name, InitComponent initComponent, ITimeComponent[] timeComponents,
			IStackComponent stackComponent, ModifierCheck effectCheck, ITargetComponent targetComponent)
		{
			Id = id;
			GenId = genId;
			Name = name;

			_initComponent = initComponent;

			_timeComponents = timeComponents;

			if (timeComponents != null)
				for (int i = 0; i < timeComponents.Length; i++)
				{
					if (timeComponents[i].IsRefreshable)
					{
						_refresh = true;
						break;
					}
				}

			_stackComponent = stackComponent;

			if (effectCheck != null)
			{
				_effectCheck = effectCheck;
				_check = true;
			}

			_targetComponent = targetComponent;
			if (targetComponent is IMultiTargetComponent)
				_multiTarget = true;
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

				if (_stackComponent != null)
					_stackComponent.SetupTarget(_targetComponent);
			}

			_targetComponent.UpdateSource(source);
			((MultiTargetComponent)_targetComponent).UpdateTargets(targetsInRange);
			if (_timeComponents != null)
				for (int i = 0; i < _timeComponents.Length; i++)
					_timeComponents[i].UpdateOwner(source);
		}

		public void UpdateSource(IUnit source) => _targetComponent.UpdateSource(source);

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

				if (_stackComponent != null)
					_stackComponent.SetupTarget(_targetComponent);
			}

			_targetComponent.UpdateSource(source);
			((SingleTargetComponent)_targetComponent).UpdateTarget(target);
			if (_timeComponents != null)
				for (int i = 0; i < _timeComponents.Length; i++)
					_timeComponents[i].UpdateOwner(source);
		}

		public void Init()
		{
			if (_initComponent == null)
				return;

			if (_multiTarget)
				_initComponent.Init(((MultiTargetComponent)_targetComponent).Targets, _targetComponent.Source);
			else
				_initComponent.Init(((SingleTargetComponent)_targetComponent).Target, _targetComponent.Source);
		}

		public void Update(float deltaTime)
		{
			if (_check)
				_effectCheck.Update(deltaTime);

			if (_timeComponents == null)
				return;

			int length = _timeComponents.Length;
			for (int i = 0; i < length; i++)
				_timeComponents[i].Update(deltaTime);
		}

		public void Refresh()
		{
			if (!_refresh || _timeComponents == null)
				return;

			int length = _timeComponents.Length;
			for (int i = 0; i < length; i++)
				_timeComponents[i].Refresh();
		}

		public void Stack()
		{
			if (_stackComponent == null)
				return;

			_stackComponent.Stack();
		}

		public void ResetState()
		{
			if (_initComponent != null)
				_initComponent.ResetState();
			if (_timeComponents != null)
				for (int i = 0; i < _timeComponents.Length; i++)
					_timeComponents[i].ResetState();
			if (_stackComponent != null)
				_stackComponent.ResetState();
			_targetComponent.ResetState();
		}
	}
}