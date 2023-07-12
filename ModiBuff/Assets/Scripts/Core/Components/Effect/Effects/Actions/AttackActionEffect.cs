namespace ModiBuff.Core
{
	public sealed class AttackActionEffect : BaseEffect, IEventTrigger, IEffect
	{
		private bool _isEventBased;

		public void SetEventBased() => _isEventBased = true;

		public override void Effect(IUnit target, IUnit source)
		{
			((IAttacker)source).Attack(target, !_isEventBased);
		}
	}
}