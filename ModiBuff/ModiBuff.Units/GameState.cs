using System.Linq;

namespace ModiBuff.Core.Units
{
	public static class GameState
	{
		public static SaveData SaveState(ModifierIdManager idManager, Unit[] units)
		{
			return new SaveData(idManager.SaveState(), units.Select(u => u.SaveState()).ToArray());
		}

		public static void LoadState(SaveData saveData, ModifierIdManager idManager, out Unit[] units)
		{
			idManager.LoadState(saveData.ModifierIdManagerSaveData);

			units = new Unit[saveData.UnitsSaveData.Length];
			for (int i = 0; i < saveData.UnitsSaveData.Length; i++)
				units[i] = Unit.LoadUnit(saveData.UnitsSaveData[i].Id);
			for (int i = 0; i < saveData.UnitsSaveData.Length; i++)
				units[i].LoadState(saveData.UnitsSaveData[i]);
		}

		public readonly struct SaveData
		{
			public readonly ModifierIdManager.SaveData ModifierIdManagerSaveData;
			public readonly Unit.SaveData[] UnitsSaveData;

#if JSON_SERIALIZATION && (NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_1_OR_GREATER || NET5_0_OR_GREATER || NET462_OR_GREATER || NETCOREAPP2_1_OR_GREATER)
			[System.Text.Json.Serialization.JsonConstructor]
#endif
			public SaveData(ModifierIdManager.SaveData modifierIdManagerSaveData, Unit.SaveData[] unitsSaveData)
			{
				ModifierIdManagerSaveData = modifierIdManagerSaveData;
				UnitsSaveData = unitsSaveData;
			}
		}
	}
}