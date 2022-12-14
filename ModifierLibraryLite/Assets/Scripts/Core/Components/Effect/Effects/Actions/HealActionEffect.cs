namespace ModifierLibraryLite.Core
{
	public class HealActionEffect : IEffect
	{
		public void Effect(IUnit target, IUnit acter)
		{
			acter.Heal(target);
		}
	}
}