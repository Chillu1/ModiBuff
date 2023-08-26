namespace ModiBuff.Core.Units
{
	public interface IEventOwner
	{
		void AddEffectEvent(IEffect effect, EffectOnEvent @event);
		void RemoveEffectEvent(IEffect effect, EffectOnEvent @event);
	}
}