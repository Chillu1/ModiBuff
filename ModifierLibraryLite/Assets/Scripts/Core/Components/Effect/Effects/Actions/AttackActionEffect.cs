namespace ModifierLibraryLite.Core
{
	public sealed class AttackActionEffect : IEffect
	{
		public void Effect(IUnit target, IUnit owner)
		{
			owner.Attack(target);
		}
	}
}