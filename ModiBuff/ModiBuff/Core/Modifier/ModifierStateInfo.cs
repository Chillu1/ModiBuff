namespace ModiBuff.Core
{
	/// <summary>
	///		Holds all effects that have state information, used for UI/UX
	/// </summary>
	public sealed class ModifierStateInfo
	{
		private readonly IModifierStateInfo[] _effects;

		public ModifierStateInfo(params IModifierStateInfo[] effects)
		{
			_effects = effects;
		}

		/// <summary>
		///		Gets state from effect
		/// </summary>
		/// <param name="stateNumber">Which state should be returned, 0 = first</param>
		public TData GetEffectState<TData>(int stateNumber = 0) where TData : struct
		{
#if DEBUG && !MODIBUFF_PROFILE
			if (stateNumber < 0 || stateNumber >= _effects.Length)
			{
				Logger.LogError("[ModiBuff] State number can't be lower than 0 or higher than effects length");
				return default;
			}
#endif

			int currentNumber = stateNumber;
			for (int i = 0; i < _effects.Length; i++)
			{
				if (!(_effects[i] is IModifierStateInfo<TData> stateInfo))
					continue;

				if (currentNumber > 0)
				{
					currentNumber--;
					continue;
				}

				return stateInfo.GetEffectData();
			}

			Logger.LogError($"[ModiBuff] Couldn't find {typeof(TData)} at number {stateNumber}");
			return default;
		}

		public object[] GetSaveState()
		{
			object[] saveData = new object[_effects.Length];
			for (int i = 0; i < _effects.Length; i++)
			{
				//TODO Temp Remove
				if (!(_effects[i] is ISavableEffect effect))
					continue;

				saveData[i] = effect.GetSaveData();
			}

			return saveData;
		}

		public void LoadSaveState(object[] saveData)
		{
			for (int i = 0; i < _effects.Length; i++)
			{
				//TODO Temp Remove
				if (!(_effects[i] is ISavableEffect effect))
					continue;

				effect.LoadSaveData(saveData[i]);
			}
		}
	}
}