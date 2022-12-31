namespace ModiBuff.Core
{
	public sealed class EventEffect : IRevertEffect, IEffect
	{
		//Always revert event effect?
		public bool IsRevertible => true;

		private readonly IEffect[] _effects;
		private readonly EffectOnEvent _effectOnEvent;

		public EventEffect(IEffect effect, EffectOnEvent effectOnEvent) : this(new[] { effect }, effectOnEvent)
		{
		}

		public EventEffect(IEffect[] effects, EffectOnEvent effectOnEvent)
		{
			for (int i = 0; i < effects.Length; i++)
				if (effects[i] is IEventTrigger eventTrigger)
					eventTrigger.SetEventBased();

			_effects = effects;
			_effectOnEvent = effectOnEvent;
		}

		public void Effect(IUnit target, IUnit source)
		{
			for (int i = 0; i < _effects.Length; i++)
				target.AddEffectEvent(_effects[i], _effectOnEvent);
		}

		public void RevertEffect(IUnit target, IUnit source)
		{
			for (int i = 0; i < _effects.Length; i++)
				target.RemoveEffectEvent(_effects[i], _effectOnEvent);
		}
	}
}