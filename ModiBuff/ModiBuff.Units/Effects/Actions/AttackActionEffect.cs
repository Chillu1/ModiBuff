namespace ModiBuff.Core.Units
{
	public sealed class AttackActionEffect : IEffect
	{
		public void Effect(IUnit target, IUnit source)
		{
			((IAttacker<float, float>)source).Attack(target);
			((IEventOwner)source).ResetEventGenId();
			((IEventOwner)target).ResetEventGenId();
		}
	}
}