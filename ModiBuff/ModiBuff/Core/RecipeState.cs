namespace ModiBuff.Core
{
	public static class RecipeState
	{
		public static SaveData SaveState(ModifierRecipes recipes)
		{
			return new SaveData(recipes.SaveState());
		}

		public static void LoadState(SaveData saveData, ModifierRecipes recipes)
		{
			recipes.LoadState(saveData.ModifierRecipeSaveData);
		}


		public readonly struct SaveData
		{
			public readonly ModifierRecipes.SaveData ModifierRecipeSaveData;

#if MODIBUFF_SYSTEM_TEXT_JSON
			[System.Text.Json.Serialization.JsonConstructor]
#endif
			public SaveData(ModifierRecipes.SaveData modifierRecipeSaveData)
			{
				ModifierRecipeSaveData = modifierRecipeSaveData;
			}
		}
	}
}