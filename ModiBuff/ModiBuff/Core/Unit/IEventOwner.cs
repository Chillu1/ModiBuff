namespace ModiBuff.Core
{
	public interface IEventOwner //TODO make more generic (to fit callback naming as well)
	{
		/// <summary>
		///		Resets all event/callback counters, so we can trigger them again
		/// </summary>
		/// <remarks>We always reset all counters because event effects might trigger other callbacks as well</remarks>
		void ResetEventCounters();
	}

	public interface IEventOwner<in TEvent> : IEventOwner
	{
		void AddEffectEvent(IEffect effect, TEvent @event);
		void RemoveEffectEvent(IEffect effect, TEvent @event);
	}
}