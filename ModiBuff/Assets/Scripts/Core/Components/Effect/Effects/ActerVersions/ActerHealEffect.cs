namespace ModiBuff.Core
{
	public sealed class ActerHealEffect : IEventTrigger, IEffect
	{
		private readonly float _heal;
		private bool _isEventBased;

		public ActerHealEffect(float heal)
		{
			_heal = heal;
		}

		public void SetEventBased() => _isEventBased = true;

		public void Effect(IUnit target, IUnit acter)
		{
			acter.Heal(_heal, target, !_isEventBased);
		}
	}
}