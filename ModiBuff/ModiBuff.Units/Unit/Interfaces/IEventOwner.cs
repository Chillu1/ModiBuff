namespace ModiBuff.Core.Units
{
	public interface IEventOwner<in TEvent>
	{
		void AddEffectEvent(IEffect effect, TEvent @event);
		void RemoveEffectEvent(IEffect effect, TEvent @event);
	}
}