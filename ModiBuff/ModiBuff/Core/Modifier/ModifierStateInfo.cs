using System.Linq;

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

		public EffectSaveData[] SaveState()
		{
			EffectSaveData[] saveData = new EffectSaveData[_effects.Length];
			for (int i = 0; i < _effects.Length; i++)
			{
				//TODO Temp Remove
				if (!(_effects[i] is ISavableEffect effect))
					continue;

				int id = EffectTypeIdManager.Instance.GetId(effect.GetType());
				saveData[i] = new EffectSaveData(id, effect.SaveState());
			}

			return saveData;
		}

		public void LoadState(EffectSaveData[] data)
		{
			for (int i = 0; i < _effects.Length; i++)
			{
				//TODO Temp Remove
				if (!(_effects[i] is ISavableEffect effect))
					continue;

				if (!EffectTypeIdManager.Instance.MatchesId(effect.GetType(), data[i].Id))
				{
					Logger.LogError(
						$"[ModiBuff] Effect type mismatch, expected {effect.GetType()} but got {data[i].Id}");
					continue;
				}

				object effectData = null;

#if JSON_SERIALIZATION && (NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_1_OR_GREATER || NET5_0_OR_GREATER)
				if (data[i].Data is System.Text.Json.JsonElement jsonElement)
				{
					var enumerateObject = jsonElement.EnumerateObject();
					object[] parameters = enumerateObject.Select<System.Text.Json.JsonProperty, object>(j =>
					{
						switch (j.Value.ValueKind)
						{
							//TODO int, double, etc
							case System.Text.Json.JsonValueKind.String:
								return j.Value.GetString();
							case System.Text.Json.JsonValueKind.Number:
								return j.Value.GetSingle();
							case System.Text.Json.JsonValueKind.True:
								return true;
							case System.Text.Json.JsonValueKind.False:
								return false;
							default:
								return null;
						}
					}).ToArray();

					effectData = effect.Create(parameters);
					effect.LoadState(effectData);
				}
#endif
				//effect.LoadState(effectData);
			}
		}

		public readonly struct EffectSaveData
		{
			public readonly int Id;
			public readonly object Data;

#if JSON_SERIALIZATION && (NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_1_OR_GREATER || NET5_0_OR_GREATER)
			[System.Text.Json.Serialization.JsonConstructor]
#endif
			public EffectSaveData(int id, object data)
			{
				Id = id;
				Data = data;
			}
		}
	}
}