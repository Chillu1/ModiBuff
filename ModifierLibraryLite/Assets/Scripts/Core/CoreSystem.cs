namespace ModifierLibraryLite.Core
{
	public sealed class CoreSystem
	{
		public ModifierRecipes Recipes { get; }
		public ModifierPool Pool { get; }

		public CoreSystem(int initialPoolSize = 64)
		{
			var idManager = new ModifierIdManager();
			Recipes = new ModifierRecipes();

			var modifierRecipes = Recipes.GetRecipes();
			idManager.SetupRecipeIds(modifierRecipes);
			Pool = new ModifierPool(modifierRecipes, initialPoolSize);
		}

		public void Dispose()
		{
			Pool.Dispose();
			ModifierIdManager.Reset();
		}
	}
}