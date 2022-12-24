using UnityEngine;

namespace ModiBuff.Core
{
	public sealed class StackComponent : IStackComponent, IStateReset
	{
		private readonly WhenStackEffect _whenStackEffect;
		private readonly int _maxStacks;
		private readonly bool _isRepeatable;
		private readonly int _everyXStacks;
		private readonly IStackEffect[] _effects;
		private readonly ModifierCheck _modifierCheck;
		private readonly bool _check;

		private ITargetComponent _targetComponent;

		private int _stacks;
		private float _value;

		public StackComponent(WhenStackEffect whenStackEffect, float value, int maxStacks, bool repeatable, int everyXStacks,
			IStackEffect[] effects, ModifierCheck check)
		{
			_whenStackEffect = whenStackEffect;
			_value = value;
			_maxStacks = maxStacks;
			_isRepeatable = repeatable;
			_everyXStacks = everyXStacks;

			_effects = effects;
			_modifierCheck = check;

			_check = check != null;
		}

		public void SetupTarget(ITargetComponent targetComponent)
		{
			_targetComponent = targetComponent;
		}

		public void Stack()
		{
			if (_maxStacks != -1 && _stacks >= _maxStacks)
				return;

			_stacks++;

			if (_check && !_modifierCheck.Check(_targetComponent.Owner))
				return;

			//Redundancy because of performance
			switch (_whenStackEffect)
			{
				case WhenStackEffect.Always:
					for (int i = 0; i < _effects.Length; i++)
						_effects[i].StackEffect(_stacks, _value, _targetComponent);
					break;
				case WhenStackEffect.OnMaxStacks:
					if (_stacks == _maxStacks)
					{
						for (int i = 0; i < _effects.Length; i++)
							_effects[i].StackEffect(_stacks, _value, _targetComponent);
						if (_isRepeatable)
							_stacks = 0;
					}

					break;
				case WhenStackEffect.OnXStacks:
					if (_everyXStacks > 0 && _stacks % _everyXStacks == 0)
					{
						for (int i = 0; i < _effects.Length; i++)
							_effects[i].StackEffect(_stacks, _value, _targetComponent);
					}

					break;
				default:
					Debug.LogError($"Invalid stack effect: {_whenStackEffect}");
					break;
			}
		}

		public void ResetState()
		{
			_stacks = 0;
			_targetComponent = null;
			for (int i = 0; i < _effects.Length; i++)
				if (_effects[i] is IStateReset effect)
					effect.ResetState();
		}

		public IStackComponent ShallowClone()
		{
			var effects = new IStackEffect[_effects.Length];
			for (int i = 0; i < _effects.Length; i++)
				effects[i] = _effects[i].ShallowClone();

			return new StackComponent(_whenStackEffect, _value, _maxStacks, _isRepeatable, _everyXStacks, effects, _modifierCheck);
		}

		object IShallowClone.ShallowClone() => ShallowClone();
	}


	public enum WhenStackEffect
	{
		None,
		Always,
		OnMaxStacks,
		OnXStacks,
		//OnZeroStacks,
	}
}