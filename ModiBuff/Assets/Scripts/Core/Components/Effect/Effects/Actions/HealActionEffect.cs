namespace ModiBuff.Core
{
	public sealed class HealActionEffect : IEventTrigger, IEffect
	{
		private bool _isEventBased;

		public void SetEventBased() => _isEventBased = true;

		public void Effect(IUnit target, IUnit acter)
		{
			acter.Heal(target, !_isEventBased);
		}
	}
}