namespace ModiBuff.Core
{
	public sealed class SelfAttackActionEffect : IEventTrigger, IEffect
	{
		private bool _isEventBased;

		public void SetEventBased() => _isEventBased = true;

		public void Effect(IUnit target, IUnit source)
		{
			target.Attack(target, !_isEventBased);
		}
	}
}