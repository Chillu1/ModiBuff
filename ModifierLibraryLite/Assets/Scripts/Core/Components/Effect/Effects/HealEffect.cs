namespace ModifierLibraryLite.Core
{
	public class HealEffect : IEffect
	{
		private readonly float _heal;

		public HealEffect(float heal)
		{
			_heal = heal;
		}

		public void Effect(IUnit target, IUnit owner)
		{
			target.Heal(_heal, owner);
		}
	}
}