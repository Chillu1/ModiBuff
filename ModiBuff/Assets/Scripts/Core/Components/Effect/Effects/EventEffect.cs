namespace ModiBuff.Core
{
	public sealed class EventEffect : BaseEffect, IRevertEffect, IEffect
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

		public override void Effect(IUnit target, IUnit source)
		{
			var eventOwner = (IEventOwner)target;
			for (int i = 0; i < _effects.Length; i++)
				eventOwner.AddEffectEvent(_effects[i], _effectOnEvent);
		}

		public void RevertEffect(IUnit target, IUnit source)
		{
			var eventOwner = (IEventOwner)target;
			for (int i = 0; i < _effects.Length; i++)
				eventOwner.RemoveEffectEvent(_effects[i], _effectOnEvent);
		}
	}
}