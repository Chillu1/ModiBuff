namespace ModiBuff.Core
{
	public sealed class CoreSystem
	{
		public ModifierIdManager IdManager { get; }
		public ModifierRecipes Recipes { get; }
		public ModifierPool Pool { get; }

		public CoreSystem(int initialPoolSize = 64)
		{
			IdManager = new ModifierIdManager();
			Recipes = new TestModifierRecipes(IdManager);
			Pool = new ModifierPool(Recipes.GetRecipes(), initialPoolSize);
		}

		public void Dispose()
		{
			Pool.Dispose();
			IdManager.Reset();
		}
	}
}