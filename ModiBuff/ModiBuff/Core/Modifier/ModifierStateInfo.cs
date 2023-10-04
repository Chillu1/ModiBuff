namespace ModiBuff.Core
{
	public sealed class ModifierStateInfo
	{
		private readonly IEffect[] _effects;

		public ModifierStateInfo(IEffect[] effects)
		{
			_effects = effects;
		}

		/// <summary>
		///		Gets state from effect
		/// </summary>
		/// <param name="stateNumber">Which state should be returned, 0 = first</param>
		public TState GetState<TState>(int stateNumber = 0) where TState : struct
		{
#if DEBUG && !MODIBUFF_PROFILE
			if (_effects == null)
			{
				Logger.LogError("No state effects stored in ModifierStateInfo");
				return default;
			}

			if (stateNumber < 0 || stateNumber >= _effects.Length)
			{
				Logger.LogError("State number can't be lower than 0 or higher than effects length");
				return default;
			}
#endif

			for (int i = 0; i < _effects.Length; i++)
			{
				if (!(_effects[i] is IModifierStateInfo<TState> stateInfo))
					continue;

				if (stateNumber > 0)
				{
					stateNumber--;
					continue;
				}

				return stateInfo.GetEffectData();
			}

			Logger.LogError($"Couldn't find {typeof(TState)} at number {stateNumber}");
			return default;
		}
	}
}