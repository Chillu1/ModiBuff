namespace ModiBuff.Core
{
	public interface IEventOwner
	{
		void ResetEventCounters();
	}

	public interface IEventOwner<in TEvent> : IEventOwner
	{
		void AddEffectEvent(IEffect effect, TEvent @event);
		void RemoveEffectEvent(IEffect effect, TEvent @event);
	}
}