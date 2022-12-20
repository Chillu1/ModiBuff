namespace ModiBuff.Core
{
	public sealed class SelfHealEffect : IEffect
	{
		private readonly float _heal;

		public SelfHealEffect(float heal)
		{
			_heal = heal;
		}

		public void Effect(IUnit target, IUnit acter)
		{
			acter.Heal(_heal, target);
		}
	}
}