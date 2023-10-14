namespace ModiBuff.Core
{
	public sealed class EventEffect<TEvent> : IRevertEffect, IRecipeFeedEffects, IEffect
	{
		//Always revert event effect?
		public bool IsRevertible => true;

		private IEffect[] _effects;
		private readonly TEvent _effectOnEvent;

		public EventEffect(TEvent effectOnEvent) => _effectOnEvent = effectOnEvent;

		public EventEffect(IEffect[] effects, TEvent effectOnEvent)
		{
			_effects = effects;
			_effectOnEvent = effectOnEvent;
		}

		public void SetEffects(IEffect[] effects) => _effects = effects;

		public void Effect(IUnit target, IUnit source)
		{
			var eventOwner = (IEventOwner<TEvent>)target;
			for (int i = 0; i < _effects.Length; i++)
				eventOwner.AddEffectEvent(_effects[i], _effectOnEvent);
		}

		public void RevertEffect(IUnit target, IUnit source)
		{
			var eventOwner = (IEventOwner<TEvent>)target;
			for (int i = 0; i < _effects.Length; i++)
				eventOwner.RemoveEffectEvent(_effects[i], _effectOnEvent);
		}
	}
}