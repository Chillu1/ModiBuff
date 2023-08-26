namespace ModiBuff.Core.Units
{
	public sealed class SelfAttackActionEffect : IEventTrigger, IEffect
	{
		private bool _isEventBased;

		public void SetEventBased() => _isEventBased = true;

		public void Effect(IUnit target, IUnit source)
		{
			((IAttacker<float>)target).Attack(target, !_isEventBased);
		}
	}
}