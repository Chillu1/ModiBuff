namespace ModiBuff.Core
{
	public sealed class CoreSystem
	{
		public ModifierRecipes Recipes { get; }
		public ModifierPool Pool { get; }

		public CoreSystem(int initialPoolSize = 64)
		{
			var idManager = new ModifierIdManager();
			Recipes = new ModifierRecipes();
			Pool = new ModifierPool(Recipes.GetRecipes(), initialPoolSize);
		}

		public void Dispose()
		{
			Pool.Dispose();
			ModifierIdManager.Reset();
		}
	}
}