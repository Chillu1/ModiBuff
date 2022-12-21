namespace ModiBuff.Core
{
	public sealed class AddDamageEffect : IStackEffect, IStateReset, IRevertEffect
	{
		private readonly float _damage;
		private float _extraDamage;
		private readonly StackEffectType _stackEffect;

		public bool IsRevertible { get; }
		private float _totalAddedDamage;

		public AddDamageEffect(float damage, bool revertible = false, StackEffectType stackEffect = StackEffectType.None)
		{
			_damage = damage;
			IsRevertible = revertible;
			_stackEffect = stackEffect;
		}

		public void Effect(IUnit target, IUnit acter)
		{
			if (IsRevertible)
				_totalAddedDamage += _damage + _extraDamage;
			target.AddDamage(_damage + _extraDamage);
		}

		public void RevertEffect(IUnit target, IUnit owner)
		{
			target.AddDamage(-_totalAddedDamage);
		}

		public void StackEffect(int stacks, float value, ITargetComponent targetComponent)
		{
			if ((_stackEffect & StackEffectType.Add) != 0)
				_extraDamage += value;

			if ((_stackEffect & StackEffectType.AddStacksBased) != 0)
				_extraDamage += value * stacks;

			if ((_stackEffect & StackEffectType.Effect) != 0)
				Effect(targetComponent.Target, targetComponent.Owner);
		}

		public void ResetState()
		{
			_extraDamage = 0;
			_totalAddedDamage = 0;
		}

		public IStackEffect ShallowClone() => new AddDamageEffect(_damage, IsRevertible, _stackEffect);
	}
}