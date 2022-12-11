using System;
using System.Collections.Generic;

namespace ModifierLibraryLite.Core
{
	public sealed class ModifierPool : IDisposable
	{
		private readonly Dictionary<string, Stack<Modifier>> _pools;
		private readonly Dictionary<string, ModifierRecipe> _recipes;

		public ModifierPool(ModifierRecipe[] recipes, int initialSize = 64)
		{
			_pools = new Dictionary<string, Stack<Modifier>>(recipes.Length);
			_recipes = new Dictionary<string, ModifierRecipe>(recipes.Length);

			foreach (var recipe in recipes)
			{
				_pools.Add(recipe.Id, new Stack<Modifier>(initialSize));

				_recipes.Add(recipe.Id, recipe);

				Allocate(recipe.Id, initialSize);
			}
		}

		private void Allocate(string id)
		{
			var recipe = _recipes[id];
			var pool = _pools[id];

			//Double the size of the pool
			for (int i = 0; i < pool.Count; i++)
				pool.Push(recipe.Create());
		}

		internal void Allocate(string id, int count)
		{
			var recipe = _recipes[id];
			var pool = _pools[id];

			for (int i = 0; i < count; i++)
				pool.Push(recipe.Create());
		}

		public Modifier Rent(string id)
		{
			var pool = _pools[id];

			if (pool.Count > 0)
				return pool.Pop();

			Allocate(id);
			return pool.Pop();
		}

		public void Return(Modifier modifier)
		{
			modifier.ResetState();

			_pools[modifier.Id].Push(modifier);
		}

		public void Dispose()
		{
			foreach (var pool in _pools.Values)
				pool.Clear();
		}
	}
}