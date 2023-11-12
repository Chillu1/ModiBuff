namespace ModiBuff.Core
{
	public sealed class RemoveEffectWrapper
	{
		public EffectOn EffectOn { get; private set; }

		private readonly RemoveEffect _effect;

		public RemoveEffectWrapper(RemoveEffect effect, EffectOn effectOn)
		{
			_effect = effect;
			EffectOn = effectOn;
		}

		public void AddEffectOn(EffectOn effectOn) => EffectOn |= effectOn;

		public RemoveEffect GetEffect() => _effect;
	}
}