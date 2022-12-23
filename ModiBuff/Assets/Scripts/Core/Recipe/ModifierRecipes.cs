using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModiBuff.Core
{
	public abstract class ModifierRecipes
	{
		public static int RecipesCount { get; private set; }

		private readonly IDictionary<string, ModifierRecipe> _recipes;

		public ModifierRecipes()
		{
			_recipes = new Dictionary<string, ModifierRecipe>();

			SetupRecipes();
			foreach (var modifier in _recipes.Values)
				modifier.Finish();

			RecipesCount = _recipes.Count;
			Debug.Log($"[ModiBuff] Loaded {RecipesCount} recipes.");
		}

		protected abstract void SetupRecipes();

		public ModifierRecipe GetRecipe(string id) => _recipes[id];
		internal ModifierRecipe GetRecipe(int id) => _recipes.Values.ElementAt(id);

		internal ModifierRecipe[] GetRecipes() => _recipes.Values.ToArray();

		protected ModifierRecipe Add(string id)
		{
			var recipe = new ModifierRecipe(id);
			if (_recipes.ContainsKey(id))
			{
				Debug.LogError($"Modifier with id {id} already exists");
				return _recipes[id];
			}

			_recipes.Add(id, recipe);
			return recipe;
		}
	}
}