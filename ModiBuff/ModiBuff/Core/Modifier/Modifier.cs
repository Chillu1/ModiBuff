using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ModiBuff.Tests")]
[assembly: InternalsVisibleTo("ModiBuff.Core.Units")]

namespace ModiBuff.Core
{
	public sealed class Modifier : IModifier
	{
		public int Id { get; }
		public string Name { get; }

		private readonly bool _init, _time, _refresh, _stack;

		private readonly InitComponent _initComponent;

		private readonly ITimeComponent[] _timeComponents;

		private readonly IStackComponent _stackComponent;

		private readonly ModifierCheck _effectCheck;
		private readonly bool _check;

		private ITargetComponent _targetComponent;
		private bool _multiTarget;

		public Modifier(int id, string name, InitComponent initComponent, ITimeComponent[] timeComponents, IStackComponent stackComponent,
			ModifierCheck effectCheck)
		{
			Id = id;
			Name = name;

			if (initComponent != null)
			{
				_initComponent = initComponent;
				_init = true;
			}

			if (timeComponents != null && timeComponents.Length > 0)
			{
				_timeComponents = timeComponents;

				for (int i = 0; i < timeComponents.Length; i++)
				{
					if (timeComponents[i].IsRefreshable)
					{
						_refresh = true;
						break;
					}
				}

				_time = true;
			}

			if (stackComponent != null)
			{
				_stackComponent = stackComponent;
				_stack = true;
			}

			if (effectCheck != null)
			{
				_effectCheck = effectCheck;
				_check = true;
			}
		}

		public void SetTarget(ITargetComponent targetComponent)
		{
			_targetComponent = targetComponent;
			_multiTarget = targetComponent is MultiTargetComponent;

			if (_time)
				for (int i = 0; i < _timeComponents.Length; i++)
					_timeComponents[i].SetupTarget(_targetComponent);

			if (_stack)
				_stackComponent.SetupTarget(_targetComponent);
		}

		public void UpdateSource(IUnit source) => _targetComponent.UpdateSource(source);

		public void Init()
		{
			if (!_init)
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

			if (!_time)
				return;

			int length = _timeComponents.Length;
			for (int i = 0; i < length; i++)
				_timeComponents[i].Update(deltaTime);
		}

		public void Refresh()
		{
			if (!_refresh || !_time)
				return;

			int length = _timeComponents.Length;
			for (int i = 0; i < length; i++)
				_timeComponents[i].Refresh();
		}

		public void Stack()
		{
			if (!_stack)
				return;

			_stackComponent.Stack();
		}

		public void ResetState()
		{
			if (_init)
				_initComponent.ResetState();
			if (_time)
				for (int i = 0; i < _timeComponents.Length; i++)
					_timeComponents[i].ResetState();
			if (_stack)
				_stackComponent.ResetState();
			//No need to reset targetComponent references, because we overwrite them on SetTargets
		}
	}
}