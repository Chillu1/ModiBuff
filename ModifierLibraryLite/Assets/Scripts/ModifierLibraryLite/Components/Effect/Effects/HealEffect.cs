namespace ModifierLibraryLite
{
	public class HealEffect : IEffect
	{
		private readonly float _heal;

		public HealEffect(float heal)
		{
			_heal = heal;
		}

		public void Effect(IUnit target)
		{
			target.Heal(_heal);
		}
	}
}