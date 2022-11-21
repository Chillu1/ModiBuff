namespace ModifierLibraryLite
{
	public class HealActionEffect : IEffect
	{
		private IUnit _owner;

		public void Effect(IUnit target)
		{
			_owner.Heal(target);
		}
	}
}