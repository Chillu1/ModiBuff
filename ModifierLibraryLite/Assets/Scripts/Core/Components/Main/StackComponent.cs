using UnityEngine;

namespace ModifierLibraryLite.Core
{
	public class StackComponent : IStackComponent
	{
		public int Stacks => _stacks;

		private readonly WhenStackEffect _whenStackEffect;
		private readonly int _maxStacks;
		private readonly bool _isRepeatable;

		private int _stacks;
		private float _value;

		public StackComponent(WhenStackEffect whenStackEffect, int maxStacks, bool isRepeatable)
		{
			_whenStackEffect = whenStackEffect;
			_maxStacks = maxStacks;
			_isRepeatable = isRepeatable;
		}

		public bool Stack()
		{
			if (_stacks >= _maxStacks)
				return false;

			_stacks++;

			switch (_whenStackEffect)
			{
				case WhenStackEffect.Always:
					return true;
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

			return false;
		}

		public IStackComponent ShallowClone() => new StackComponent(_whenStackEffect, _maxStacks, _isRepeatable);
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