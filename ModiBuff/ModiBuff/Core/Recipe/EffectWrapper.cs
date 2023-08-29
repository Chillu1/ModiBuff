namespace ModiBuff.Core
{
	public sealed class EffectWrapper
	{
		private readonly IEffect _effect;
		public EffectOn EffectOn { get; }

		private IEffect _effectClone;

		public EffectWrapper(IEffect effect, EffectOn effectOn)
		{
			_effect = effect;
			EffectOn = effectOn;
		}

		public IEffect GetEffect()
		{
			if (_effect is IShallowClone shallowClone)
			{
				if (_effectClone == null)
					_effectClone = (IEffect)shallowClone.ShallowClone();
				return _effectClone;
			}

			return _effect;
		}

		public void UpdateGenId(int genId)
		{
			if (_effect is IRemoveEffect removeEffect)
				removeEffect.SetGenId(genId);
#if DEBUG && !MODIBUFF_PROFILE
			else
				Logger.LogWarning("EffectWrapper.UpdateGenId: Effect is not a RemoveEffect");
#endif
		}

		public void Reset()
		{
			_effectClone = null;
		}
	}
}