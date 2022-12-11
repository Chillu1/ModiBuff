using UnityEngine;

namespace ModifierLibraryLite.Core
{
	public class StackComponent : IStackComponent
	{
		private readonly WhenStackEffect _whenStackEffect;
		private readonly int _maxStacks;
		private readonly bool _isRepeatable;
		private readonly IStackEffect[] _effects;

		private ITargetComponent _targetComponent;

		private int _stacks;
		private float _value;

		public StackComponent(WhenStackEffect whenStackEffect, int maxStacks, bool isRepeatable, IStackEffect[] effects)
		{
			_whenStackEffect = whenStackEffect;
			_maxStacks = maxStacks;
			_isRepeatable = isRepeatable;

			_effects = effects;
		}

		public void SetupTarget(ITargetComponent targetComponent)
		{
			_targetComponent = targetComponent;
		}

		public void Stack()
		{
			if (_stacks >= _maxStacks)
				return;

			_stacks++;

			switch (_whenStackEffect)
			{
				case WhenStackEffect.Always:
					for (int i = 0; i < _effects.Length; i++)
						_effects[i].StackEffect(_stacks, _value, _targetComponent);
					break;
				case WhenStackEffect.MaxStacks:
					break;
				case WhenStackEffect.OnXStacks:
					break;
				case WhenStackEffect.EveryXStacks:
					break;
				default:
					Debug.LogError($"Invalid stack effect: {_whenStackEffect}");
					break;
			}
		}
	}


	public enum WhenStackEffect
	{
		None,
		Always,
		MaxStacks,
		OnXStacks,
		EveryXStacks,
		ZeroStacks,
	}
}