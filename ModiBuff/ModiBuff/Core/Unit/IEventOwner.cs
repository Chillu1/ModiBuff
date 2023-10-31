namespace ModiBuff.Core
{
	public interface IEventOwner
	{
		void ApplyEffectGenId(int effectGenId);
		void ResetEventGenId();
	}

	public interface IEventOwner<in TEvent> : IEventOwner
	{
		void AddEffectEvent(IEffect effect, TEvent @event);
		void RemoveEffectEvent(IEffect effect, TEvent @event);
	}
}