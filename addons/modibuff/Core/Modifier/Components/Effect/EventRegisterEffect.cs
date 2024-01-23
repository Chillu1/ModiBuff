namespace ModiBuff.Core
{
	public sealed class EventRegisterEffect<TEvent> : IRevertEffect, IRecipeFeedEffects, IEffect,
		IRegisterEffect, IShallowClone<IEffect>
	{
		public bool IsRevertible => true;

		private readonly TEvent _effectOnEvent;
		private IEffect[] _effects;

		private bool _isRegistered;

		public EventRegisterEffect(TEvent effectOnEvent) : this(effectOnEvent, null)
		{
		}

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
			if (!(target is IEventOwner<TEvent> eventTarget))
			{
#if MODIBUFF_EFFECT_CHECK
				EffectHelper.LogImplError(target, nameof(IEventOwner<TEvent>));
#endif
				return;
			}

			if (_isRegistered)
				return;

			_isRegistered = true;
			for (int i = 0; i < _effects.Length; i++)
				eventTarget.AddEffectEvent(_effects[i], _effectOnEvent);
		}

		public void RevertEffect(IUnit target, IUnit source)
		{
			if (!(target is IEventOwner<TEvent> eventTarget))
				return;

			for (int i = 0; i < _effects.Length; i++)
				eventTarget.RemoveEffectEvent(_effects[i], _effectOnEvent);
			_isRegistered = false;
		}

		public IEffect ShallowClone() => new EventRegisterEffect<TEvent>(_effectOnEvent);
		object IShallowClone.ShallowClone() => ShallowClone();
	}
}