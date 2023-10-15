namespace ModiBuff.Core
{
	public sealed class EventRegisterEffect<TEvent> : IRevertEffect, IRecipeFeedEffects, IEffect,
		IShallowClone<EventRegisterEffect<TEvent>>
	{
		//Always revert event effect?
		public bool IsRevertible => true;

		private readonly TEvent _effectOnEvent;
		private IEffect[] _effects;

		public EventRegisterEffect(TEvent effectOnEvent) => _effectOnEvent = effectOnEvent;

		/// <summary>
		///		Manual modifier generation constructor
		/// </summary>
		public static EventRegisterEffect<TEvent> Create(TEvent effectOnEvent, params IEffect[] effects) =>
			new EventRegisterEffect<TEvent>(effectOnEvent, effects);

		private EventRegisterEffect(TEvent effectOnEvent, IEffect[] effects)
		{
			_effectOnEvent = effectOnEvent;
			_effects = effects;
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

		public EventRegisterEffect<TEvent> ShallowClone() => new EventRegisterEffect<TEvent>(_effectOnEvent);
		object IShallowClone.ShallowClone() => ShallowClone();
	}
}