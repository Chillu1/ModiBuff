namespace ModiBuff.Core
{
	/// <summary>
	///		Holds all effects that have state information, used for UI/UX
	/// </summary>
	public readonly struct EffectStateInfo
	{
		private readonly (EffectOn On, IEffectStateInfo Info)[] _effects;

		public EffectStateInfo(params (EffectOn, IEffectStateInfo)[] effects) => _effects = effects;

		/// <summary>
		///		Gets state from effect
		/// </summary>
		/// <param name="stateNumber">Which state should be returned, 0 = first</param>
		public (EffectOn EffectOn, TData Data)? GetEffectState<TData>(int stateNumber, EffectOn effectOn)
			where TData : struct
		{
#if DEBUG && !MODIBUFF_PROFILE
			if (stateNumber < 0 || stateNumber >= _effects.Length)
			{
				Logger.LogError("[ModiBuff] State number can't be lower than 0 or higher than effects length");
				return null;
			}
#endif

			int currentNumber = stateNumber;
			for (int i = 0; i < _effects.Length; i++)
			{
				var effect = _effects[i];
				if (!(effect.Info is IEffectStateInfo<TData> stateInfo))
					continue;

				if (effectOn != EffectOn.None && effect.On != effectOn)
					continue;

				if (currentNumber > 0)
				{
					currentNumber--;
					continue;
				}

				return (effect.On, stateInfo.GetEffectData());
			}

			Logger.LogError(
				$"[ModiBuff] Couldn't find {typeof(TData)} at number {stateNumber} {(effectOn == EffectOn.None ? "" : $"with EffectOn {effectOn}")}");
			return null;
		}

		public (EffectOn EffectOn, object Data)[] GetEffectStates()
		{
			var states = new (EffectOn EffectOn, object Data)[_effects.Length];
			for (int i = 0; i < _effects.Length; i++)
			{
				var effect = _effects[i];
				states[i] = (effect.On, effect.Info.GetEffectData());
			}

			return states;
		}
	}
}