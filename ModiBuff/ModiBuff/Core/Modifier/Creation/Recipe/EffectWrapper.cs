namespace ModiBuff.Core
{
	public sealed class EffectWrapper
	{
		private readonly IEffect _effect;
		public readonly EffectOn EffectOn;

		private readonly bool _effectUsesMutableState;
		private readonly IShallowClone<IEffect> _effectShallowClone;

		private IEffect _effectClone;

		public EffectWrapper(IEffect effect, EffectOn effectOn)
		{
			_effect = effect;
			EffectOn = effectOn;

			if (_effect is IShallowClone<IEffect> shallowClone)
			{
				if (_effect is IMutableStateEffect stateEffect && !stateEffect.UsesMutableState)
					return;

				_effectUsesMutableState = true;
				_effectShallowClone = shallowClone;
			}
		}

		public IEffect GetEffect()
		{
			if (!_effectUsesMutableState)
				return _effect;

			if (_effectClone == null)
				_effectClone = _effectShallowClone.ShallowClone();

			return _effectClone;
		}

		public void Reset()
		{
			_effectClone = null;
		}
	}
}