namespace ModiBuff.Core.Units
{
	public sealed class SelfAttackActionEffect : IEffect
	{
		public void Effect(IUnit target, IUnit source)
		{
			((IAttacker<float, float>)target).Attack(target);
			((IEventOwner)source).ResetEventGenId();
			((IEventOwner)target).ResetEventGenId();
		}
	}
}