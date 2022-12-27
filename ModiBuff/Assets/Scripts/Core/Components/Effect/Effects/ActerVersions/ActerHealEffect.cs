namespace ModiBuff.Core
{
	public sealed class ActerHealEffect : IEffect
	{
		private readonly float _heal;

		public ActerHealEffect(float heal)
		{
			_heal = heal;
		}

		public void Effect(IUnit target, IUnit acter)
		{
			acter.Heal(_heal, target);
		}
	}
}