namespace ModiBuff.Core
{
	public sealed class AttackActionEffect : IEffect
	{
		public void Effect(IUnit target, IUnit acter)
		{
			acter.Attack(target);
		}
	}
}