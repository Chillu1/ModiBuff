namespace ModiBuff.Core
{
	//TODO Might be a bad approach for events, instead of setting up events in a different way, maybe with a timer and effect only?
	public sealed class EventEffect : IRevertEffect, IEffect
	{
		//Always revert event effect?
		public bool IsRevertible => true;

		//private readonly IEffect[] _effects;
		private readonly IEffect _effect;
		private readonly EffectOnEvent _effectOnEvent;

		public EventEffect(IEffect effect, EffectOnEvent effectOnEvent)
		{
			_effect = effect;
			_effectOnEvent = effectOnEvent;
		}

		public void Effect(IUnit target, IUnit acter)
		{
			target.AddEffectEvent(_effect, _effectOnEvent);
		}

		public void RevertEffect(IUnit target, IUnit acter)
		{
			target.RemoveEffectEvent(_effect, _effectOnEvent);
		}
	}
}