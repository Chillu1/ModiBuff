namespace ModiBuff.Core
{
	public interface IEventOwner
	{
		void AddEffectEvent(IEffect effect, EffectOnEvent @event);
		void RemoveEffectEvent(IEffect effect, EffectOnEvent @event);
	}
}