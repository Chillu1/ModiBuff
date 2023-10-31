namespace ModiBuff.Core.Units
{
	public sealed class HealActionEffect : IEffect
	{
		public void Effect(IUnit target, IUnit source)
		{
			((IHealer<float, float>)source).Heal((IHealable<float, float>)target);
			((IEventOwner)source).ResetEventCounters();
			((IEventOwner)target).ResetEventCounters();
		}
	}
}