using System.Collections.Generic;
using System.Linq;

namespace ModiBuff.Core.Units
{
	public abstract class UnitRecipes
	{
		public static int RecipesCount { get; private set; }

		private readonly ModifierRecipes _modifierRecipes;
		private readonly IDictionary<string, UnitRecipe> _recipes;

		public UnitRecipes(ModifierRecipes modifierRecipes)
		{
			_modifierRecipes = modifierRecipes;
			_recipes = new Dictionary<string, UnitRecipe>();

			SetupRecipes();

			RecipesCount = _recipes.Count;

			Logger.Log($"Loaded {RecipesCount} unit recipes.");
		}

		protected abstract void SetupRecipes();

		public UnitRecipe GetRecipe(string id) => _recipes[id];
		internal UnitRecipe GetRecipe(int id) => _recipes.Values.ElementAt(id);

		internal UnitRecipe[] GetRecipes() => _recipes.Values.ToArray();

		protected UnitRecipe Add(string name, UnitType unitType)
		{
			if (_recipes.TryGetValue(name, out var localRecipe))
			{
#if DEBUG && !MODIBUFF_PROFILE
				Logger.LogError($"[ModiBuff.Units] Unit with id {name} already exists");
#endif
				return localRecipe;
			}

			var recipe = new UnitRecipe(name, unitType, _modifierRecipes);
			_recipes.Add(name, recipe);
			return recipe;
		}
	}
}