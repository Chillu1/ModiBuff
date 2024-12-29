namespace ModiBuff.Core.Units
{
	public sealed class DebuffEffect : IEffect, IRevertEffect, IStateEffect
	{
		public bool IsRevertible { get; }

		private readonly DebuffType _debuffType;

		private int _stacksApplied;

		public DebuffEffect(DebuffType debuffType)
		{
			_debuffType = debuffType;
		}

		public void Effect(IUnit target, IUnit source)
		{
			((IDebuffable)target).AddDebuff(_debuffType, source);
			if (IsRevertible)
				_stacksApplied++;
		}

		public void RevertEffect(IUnit target, IUnit source)
		{
			((IDebuffable)target).RemoveDebuff(_debuffType, _stacksApplied, source);
			_stacksApplied = 0;
		}

		public void ResetState() => _stacksApplied = 0;

		public IEffect ShallowClone() => new DebuffEffect(_debuffType);
		object IShallowClone.ShallowClone() => ShallowClone();
	}
}