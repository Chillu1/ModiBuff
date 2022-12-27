namespace ModiBuff.Core
{
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