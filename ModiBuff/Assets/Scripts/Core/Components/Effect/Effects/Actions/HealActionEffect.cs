namespace ModiBuff.Core
{
	public sealed class HealActionEffect : IEventTrigger, IEffect
	{
		private bool _isEventBased;

		public void SetEventBased() => _isEventBased = true;

		public void Effect(IUnit target, IUnit source)
		{
			((IHealer)source).Heal((IHealable)target, !_isEventBased);
		}
	}
}