using System.Collections.Generic;
using System.Linq;

namespace ModiBuff.Core.Units
{
	public abstract class UnitRecipesBase
	{
		public static int RecipesCount { get; private set; }

		protected readonly ModifierRecipesBase ModifierRecipes;
		private readonly IDictionary<string, UnitRecipe> _recipes;

		public UnitRecipesBase(ModifierRecipesBase modifierRecipes)
		{
			ModifierRecipes = modifierRecipes;
			_recipes = new Dictionary<string, UnitRecipe>();

			SetupRecipes();

			RecipesCount = _recipes.Count;
			//TODO LOG
			//Debug.Log($"Loaded {RecipesCount} unit recipes.");
		}

		protected abstract void SetupRecipes();

		public UnitRecipe GetRecipe(string id) => _recipes[id];
		internal UnitRecipe GetRecipe(int id) => _recipes.Values.ElementAt(id);

		internal UnitRecipe[] GetRecipes() => _recipes.Values.ToArray();

		protected UnitRecipe Add(string name)
		{
			if (_recipes.TryGetValue(name, out var localRecipe))
			{
				//TODO LOG
				//Debug.LogError($"Unit with id {name} already exists");
				return localRecipe;
			}

			var recipe = new UnitRecipe(name);
			_recipes.Add(name, recipe);
			return recipe;
		}
	}
}