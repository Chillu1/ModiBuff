namespace ModiBuff.Core
{
	public sealed class AddDamageEffect : IStackEffect, IRevertEffect
	{
		private float _damage;
		private readonly StackEffectType _stackEffect;

		public bool IsRevertible { get; }
		private float _addedDamage;

		public AddDamageEffect(float damage, bool revertible = false, StackEffectType stackEffect = StackEffectType.None)
		{
			_damage = damage;
			IsRevertible = revertible;
			_stackEffect = stackEffect;
		}

		public void Effect(IUnit target, IUnit acter)
		{
			if (IsRevertible)
				_addedDamage += _damage;
			target.AddDamage(_damage);
		}

		public void RevertEffect(IUnit target, IUnit owner)
		{
			target.AddDamage(-_addedDamage);
		}

		public void StackEffect(int stacks, float value, ITargetComponent targetComponent)
		{
			if ((_stackEffect & StackEffectType.Add) != 0)
				_damage += value;

			if ((_stackEffect & StackEffectType.AddStacksBased) != 0)
				_damage += value * stacks;

			if ((_stackEffect & StackEffectType.Effect) != 0)
				Effect(targetComponent.Target, targetComponent.Owner);
		}

		public IStackEffect ShallowClone() => new AddDamageEffect(_damage, IsRevertible, _stackEffect);
	}
}