namespace ModiBuff.Core
{
	public class HealEffect : IStateEffect, IStackEffect, IRevertEffect, IEffect
	{
		public bool IsRevertible { get; }

		private readonly float _heal;
		private readonly StackEffectType _stackEffect;

		private float _extraHeal;
		private float _totalHeal;

		public HealEffect(float heal)
		{
			_heal = heal;
		}

		public void Effect(IUnit target, IUnit acter)
		{
			if (IsRevertible)
				_totalHeal = _heal + _extraHeal;

			target.Heal(_heal + _extraHeal, acter);
		}

		public void RevertEffect(IUnit target, IUnit acter)
		{
			target.Heal(-_totalHeal, acter);
		}

		public void StackEffect(int stacks, float value, ITargetComponent targetComponent)
		{
			if ((_stackEffect & StackEffectType.Add) != 0)
				_totalHeal += value;

			if ((_stackEffect & StackEffectType.AddStacksBased) != 0)
				_totalHeal += value * stacks;

			if ((_stackEffect & StackEffectType.Effect) != 0)
				Effect(targetComponent.Target, targetComponent.Acter);
		}

		public void ResetState()
		{
			_extraHeal = 0;
			_totalHeal = 0;
		}

		public IStateEffect ShallowClone() => new HealEffect(_heal);
		object IShallowClone.ShallowClone() => ShallowClone();
	}
}