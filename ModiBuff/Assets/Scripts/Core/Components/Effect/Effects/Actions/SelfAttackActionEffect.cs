namespace ModiBuff.Core
{
	public sealed class SelfAttackActionEffect : IEffect
	{
		public void Effect(IUnit target, IUnit acter)
		{
			target.Attack(target, false);
		}
	}
}