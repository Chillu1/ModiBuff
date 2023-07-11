using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModiBuff.Core
{
	public abstract class ModifierRecipesBase
	{
		public static int RecipesCount { get; private set; }

		private readonly IDictionary<string, IModifierRecipe> _recipes;

		public ModifierRecipesBase()
		{
			_recipes = new Dictionary<string, IModifierRecipe>();

			SetupRecipes();
			foreach (var modifier in _recipes.Values)
				modifier.Finish();

			RecipesCount = _recipes.Count;
			Debug.Log($"[ModiBuff] Loaded {RecipesCount} recipes.");
		}

		protected abstract void SetupRecipes();

		public IModifierRecipe GetRecipe(string id) => _recipes[id];
		internal IModifierRecipe GetRecipe(int id) => _recipes.Values.ElementAt(id);

		internal IModifierRecipe[] GetRecipes() => _recipes.Values.ToArray();

		protected ModifierRecipe Add(string name)
		{
			if (_recipes.TryGetValue(name, out var localRecipe))
			{
				Debug.LogError($"Modifier with id {name} already exists");
				return (ModifierRecipe)localRecipe;
			}

			var recipe = new ModifierRecipe(name);
			_recipes.Add(name, recipe);
			return recipe;
		}

		protected ModifierEventRecipe AddEvent(string name, EffectOnEvent effectOnEvent)
		{
			if (_recipes.TryGetValue(name, out var localRecipe))
			{
				Debug.LogError($"Modifier with id {name} already exists");
				return (ModifierEventRecipe)localRecipe;
			}

			var recipe = new ModifierEventRecipe(name, effectOnEvent);
			_recipes.Add(name, recipe);
			return recipe;
		}
	}
}