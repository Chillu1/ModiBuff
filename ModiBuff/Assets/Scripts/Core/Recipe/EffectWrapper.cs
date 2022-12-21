namespace ModiBuff.Core
{
	public sealed class EffectWrapper
	{
		private readonly IEffect _effect;
		public EffectOn EffectOn { get; private set; }

		private IEffect _effectClone;

		public EffectWrapper(IEffect effect, EffectOn effectOn)
		{
			_effect = effect;
			EffectOn = effectOn;
		}

		public bool IsSameEffect(IEffect effect, EffectOn effectOn)
		{
			bool isSameEffect = _effect == effect;
			if (isSameEffect)
				EffectOn |= effectOn;
			return isSameEffect;
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

		public void Reset()
		{
			_effectClone = null;
		}
	}
}