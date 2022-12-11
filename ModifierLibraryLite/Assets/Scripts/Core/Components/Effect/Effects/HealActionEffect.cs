namespace ModifierLibraryLite.Core
{
	public class HealActionEffect : IEffect
	{
		public void Effect(IUnit target, IUnit owner)
		{
			owner.Heal(target);
		}
	}
}