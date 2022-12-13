using System;
using System.Collections.Generic;

namespace ModifierLibraryLite.Core
{
	public sealed class ModifierPool : IDisposable
	{
		public static ModifierPool Instance { get; private set; }

		private readonly Stack<Modifier>[] _pools;
		private readonly ModifierRecipe[] _recipes;

		public ModifierPool(ModifierRecipe[] recipes, int initialSize = 64)
		{
			if (Instance != null)
				return;

			Instance = this;

			_pools = new Stack<Modifier>[recipes.Length];
			_recipes = new ModifierRecipe[recipes.Length];

			foreach (var recipe in recipes)
			{
				_pools[recipe.Id] = new Stack<Modifier>(initialSize);
				_recipes[recipe.Id] = recipe;

				Allocate(recipe.Id, initialSize);
			}

			Array.Sort(_pools, (x, y) => x.Peek().Id.CompareTo(y.Peek().Id));
			Array.Sort(_recipes, (x, y) => x.Id.CompareTo(y.Id));
		}

		private void Allocate(int id)
		{
			var recipe = _recipes[id];
			var pool = _pools[id];

			//Double the size of the pool
			for (int i = 0; i < pool.Count; i++)
				pool.Push(recipe.Create());
		}

		internal void Allocate(int id, int count)
		{
			var recipe = _recipes[id];
			var pool = _pools[id];

			for (int i = 0; i < count; i++)
				pool.Push(recipe.Create());
		}

		public Modifier Rent(int id)
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
			for (int i = 0; i < _pools.Length; i++)
				_pools[i].Clear();

			Instance = null;
		}
	}
}