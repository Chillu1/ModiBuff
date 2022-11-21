namespace ModifierLibraryLite.Core
{
	public class HealActionEffect : IEffect
	{
		private IUnit _owner;

		public void Effect(IUnit target, IUnit owner)
		{
			owner.Heal(target);
		}
	}
}