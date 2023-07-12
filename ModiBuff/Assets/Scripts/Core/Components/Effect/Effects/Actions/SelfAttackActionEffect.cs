namespace ModiBuff.Core
{
	public sealed class SelfAttackActionEffect : BaseEffect, IEventTrigger, IEffect
	{
		private bool _isEventBased;

		public void SetEventBased() => _isEventBased = true;

		public override void Effect(IUnit target, IUnit source)
		{
			((IAttacker)target).Attack(target, !_isEventBased);
		}
	}
}