using UnityEngine;

namespace ModifierLibraryLite.Core
{
	public sealed class StackEffects
	{
		private readonly IStackEffect[] _effects;

		private float _value;

		public StackEffects(IStackEffect[] effects)
		{
			_effects = effects;
		}

		public void StackEffect(int stacks, IUnit target, IUnit owner)
		{
			for (int i = 0; i < _effects.Length; i++)
				_effects[i].StackEffect(stacks, _value, target, owner);
		}
	}
}