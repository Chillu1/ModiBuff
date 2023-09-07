namespace ModiBuff.Core.Units
{
	public sealed class EventEffect<TEvent> : IRevertEffect, IEffect
	{
		//Always revert event effect?
		public bool IsRevertible => true;

		private readonly IEffect[] _effects;
		private readonly TEvent _effectOnEvent;

		public EventEffect(IEffect[] effects, TEvent effectOnEvent)
		{
			for (int i = 0; i < effects.Length; i++)
				if (effects[i] is IEventTrigger eventTrigger)
					eventTrigger.SetEventBased();

			_effects = effects;
			_effectOnEvent = effectOnEvent;
		}

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