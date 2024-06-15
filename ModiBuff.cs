using System;

namespace ModiBuff.Core
{
	/// <summary>
	///		Helper class to access most of the ModiBuff library
	/// </summary>
	public static class MB
	{
		public static ModifierIdManager IdManager => _core.IdManager;
		public static IModifierRecipes Recipes => _core.Recipes;
		public static ModifierPool Pool => _core.Pool;
		public static ModifierControllerPool ModifierControllerPool => _core.ModifierControllerPool;

		private static ModiBuffCore _core;

		public static void Init(ILogger logger, Func<ModifierIdManager, IModifierRecipes> modifierRecipesSetup)
		{
			_core = new ModiBuffCore(logger, modifierRecipesSetup);
		}

		public static void Init<TLogger>(Func<ModifierIdManager, IModifierRecipes> modifierRecipesSetup)
			where TLogger : ILogger, new()
		{
			Init(new TLogger(), modifierRecipesSetup);
		}
	}

	public sealed class ModiBuffCore
	{
		public ModifierIdManager IdManager { get; }
		public IModifierRecipes Recipes { get; }
		public ModifierPool Pool { get; }
		public ModifierControllerPool ModifierControllerPool { get; }

		public ModiBuffCore(ILogger logger, Func<ModifierIdManager, IModifierRecipes> modifierRecipesSetup)
		{
			Logger.SetLogger(logger);

			IdManager = new ModifierIdManager();
			Recipes = modifierRecipesSetup(IdManager);
			Pool = new ModifierPool(Recipes.GetGenerators());
			ModifierControllerPool = new ModifierControllerPool();
		}
	}
}