namespace ModiBuff.Core
{
	public class HealEffect : IEffect
	{
		private readonly float _heal;

		public HealEffect(float heal)
		{
			_heal = heal;
		}

		public void Effect(IUnit target, IUnit acter)
		{
			target.Heal(_heal, acter);
		}
	}
}