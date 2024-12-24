namespace ModiBuff.Core.Units
{
	public sealed class DebuffEffect : IEffect, IRevertEffect
	{
		public bool IsRevertible { get; }

		private readonly DebuffType _debuffType;

		public DebuffEffect(DebuffType debuffType)
		{
			_debuffType = debuffType;
		}

		public void Effect(IUnit target, IUnit source)
		{
			((IDebuffable)target).AddDebuff(_debuffType, source);
		}

		public void RevertEffect(IUnit target, IUnit source)
		{
			((IDebuffable)target).RemoveDebuff(_debuffType, source);
		}
	}
}