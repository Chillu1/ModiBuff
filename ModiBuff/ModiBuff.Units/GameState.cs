using System.Linq;

namespace ModiBuff.Core.Units
{
	public static class GameState
	{
		public static SaveData SaveState(ModifierIdManager idManager, EffectIdManager effectIdManager, Unit[] units)
		{
			return new SaveData(idManager.SaveState(), effectIdManager.SaveState(),
				units.Select(u => u.SaveState()).ToArray());
		}

		public static void LoadState(SaveData saveData, ModifierIdManager idManager, EffectIdManager effectIdManager,
			out Unit[] units)
		{
			idManager.LoadState(saveData.ModifierIdManagerSaveData);
			effectIdManager.LoadState(saveData.EffectIdManagerSaveData);

			units = new Unit[saveData.UnitsSaveData.Length];
			for (int i = 0; i < saveData.UnitsSaveData.Length; i++)
				units[i] = Unit.LoadUnit(saveData.UnitsSaveData[i].Id);
			for (int i = 0; i < saveData.UnitsSaveData.Length; i++)
				units[i].LoadState(saveData.UnitsSaveData[i]);
		}

		public readonly struct SaveData
		{
			public readonly ModifierIdManager.SaveData ModifierIdManagerSaveData;
			public readonly EffectIdManager.SaveData EffectIdManagerSaveData;
			public readonly Unit.SaveData[] UnitsSaveData;

#if JSON_SERIALIZATION && (NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_1_OR_GREATER || NET5_0_OR_GREATER || NET462_OR_GREATER || NETCOREAPP2_1_OR_GREATER)
			[System.Text.Json.Serialization.JsonConstructor]
#endif
			public SaveData(ModifierIdManager.SaveData modifierIdManagerSaveData,
				EffectIdManager.SaveData effectIdManagerSaveData, Unit.SaveData[] unitsSaveData)
			{
				ModifierIdManagerSaveData = modifierIdManagerSaveData;
				EffectIdManagerSaveData = effectIdManagerSaveData;
				UnitsSaveData = unitsSaveData;
			}
		}
	}
}