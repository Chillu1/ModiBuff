namespace ModiBuff.Core.Units
{
	public sealed class AttackActionEffect : IEventTrigger, IEffect
	{
		private bool _isEventBased;

		public void SetEventBased() => _isEventBased = true;

		public void Effect(IUnit target, IUnit source)
		{
			((IAttacker<float, float>)source).Attack(target, !_isEventBased);
		}
	}
}