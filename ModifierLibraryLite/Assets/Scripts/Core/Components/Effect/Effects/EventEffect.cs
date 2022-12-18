namespace ModifierLibraryLite.Core
{
	//TODO Might be a bad approach for events, instead of setting up events in a different way, maybe with a timer and effect only?
	public sealed class EventEffect : IRevertEffect
	{
		//Always revert event effect, but we'll run into trouble if we want to revert the effect
		public bool IsRevertible => true;

		//private readonly IEffect[] _effects;
		private readonly IEffect _effect;

		public void Effect(IUnit target, IUnit acter)
		{
			target.AddEffectEvent(_effect, EffectOnEvent.OnHit);
		}

		public void RevertEffect(IUnit target, IUnit owner)
		{
			target.RemoveEffectEvent(_effect, EffectOnEvent.OnHit);
		}
	}
}