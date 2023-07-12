namespace ModiBuff.Core
{
	public sealed class HealActionEffect : BaseEffect, IEventTrigger, IEffect
	{
		private bool _isEventBased;

		public void SetEventBased() => _isEventBased = true;

		public override void Effect(IUnit target, IUnit source)
		{
			((IHealer)source).Heal((IHealable)target, !_isEventBased);
		}
	}
}