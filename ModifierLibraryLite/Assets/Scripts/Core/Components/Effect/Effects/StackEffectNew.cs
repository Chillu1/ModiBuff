namespace ModifierLibraryLite.Core
{
	public sealed class StackEffectNew : IStackEffect
	{
		private readonly StackEffectType _stackEffect;
		private readonly IEffect[] _effects;

		public StackEffectNew(StackEffectType stackEffect, IEffect effect)
		{
			_stackEffect = stackEffect;
			_effects = new[] { effect };
		}

		public StackEffectNew(StackEffectType stackEffect, IEffect[] effects)
		{
			_stackEffect = stackEffect;
			_effects = effects;
		}

		public void StackEffect(int stacks, float value, ITargetComponent targetComponent)
		{
			//We would need to add it to effect value in IEffect, but we still want it to be stateless?
			//if ((_stackEffect & StackEffectType.Add) != 0)
			//	for._effects.ExtraValue += value;
			//
			//if ((_stackEffect & StackEffectType.AddStacksBased) != 0)
			//	for._effects.ExtraValue += value * stacks;

			if ((_stackEffect & StackEffectType.Effect) != 0)
				for (int i = 0; i < _effects.Length; i++)
					_effects[i].Effect(targetComponent.Target, targetComponent.Owner);
		}

		public IStackEffect ShallowClone() => new StackEffectNew(_stackEffect, _effects);
	}
}