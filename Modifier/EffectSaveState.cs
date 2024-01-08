using System.Collections.Generic;

namespace ModiBuff.Core
{
	public readonly struct EffectSaveState
	{
		public bool Valid => _savableEffects != null;

		private readonly ISavable[] _savableEffects;

		public EffectSaveState(params ISavable[] savableEffects) => _savableEffects = savableEffects;

		public EffectSaveData[] SaveState()
		{
			EffectSaveData[] saveData = new EffectSaveData[_savableEffects.Length];
			for (int i = 0; i < _savableEffects.Length; i++)
			{
				var effect = _savableEffects[i];
				if (!(_savableEffects[i] is ISavable savableEffect))
				{
					if (effect is IStateEffect)
						Logger.LogError(
							$"[ModiBuff] Effect {effect.GetType()} has state (IStateEffect) but is not savable (ISavable)");
					continue;
				}

				//int id = EffectTypeIdManager.Instance.GetId(savableEffect.GetType());
				saveData[i] = new EffectSaveData(savableEffect.SaveState());
			}

			return saveData;
		}

		public void LoadState(IReadOnlyList<EffectSaveData> data)
		{
			for (int i = 0; i < _savableEffects.Length; i++)
			{
				if (!(_savableEffects[i] is ISavable effect))
					continue;

				//if (!EffectTypeIdManager.Instance.MatchesId(effect.GetType(), data[i].Id))
				//{
				//	Logger.LogError(
				//		$"[ModiBuff] Effect type mismatch, expected {effect.GetType()} but got {data[i].Id}");
				//	continue;
				//}

#if MODIBUFF_SYSTEM_TEXT_JSON && (NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_1_OR_GREATER || NET5_0_OR_GREATER || NET462_OR_GREATER)
				if (data[i].Data.FromAnonymousJsonObjectToSaveData(effect))
					continue;
#endif

				effect.LoadState(data[i].Data);
			}
		}

		public readonly struct EffectSaveData
		{
			//public readonly int Id;
			public readonly object Data;

#if MODIBUFF_SYSTEM_TEXT_JSON && (NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_1_OR_GREATER || NET5_0_OR_GREATER || NET462_OR_GREATER)
			[System.Text.Json.Serialization.JsonConstructor]
#endif
			public EffectSaveData(object data)
			{
				//Id = id;
				Data = data;
			}
		}
	}
}