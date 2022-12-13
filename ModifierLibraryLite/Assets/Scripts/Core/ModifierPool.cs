using System;
using System.Collections.Generic;

namespace ModifierLibraryLite.Core
{
	public sealed class ModifierPool : IDisposable
	{
		public static ModifierPool Instance { get; private set; }

		private readonly Stack<Modifier>[] _poolsArray;
		private readonly ModifierRecipe[] _recipesArray;

		public ModifierPool(ModifierRecipe[] recipes, int initialSize = 64)
		{
			if (Instance != null)
				return;

			Instance = this;

			_poolsArray = new Stack<Modifier>[recipes.Length];
			_recipesArray = new ModifierRecipe[recipes.Length];

			foreach (var recipe in recipes)
			{
				_poolsArray[recipe.Id] = new Stack<Modifier>(initialSize);
				_recipesArray[recipe.Id] = recipe;

				Allocate(recipe.Id, initialSize);
			}

			Array.Sort(_poolsArray, (x, y) => x.Peek().Id.CompareTo(y.Peek().Id));
			Array.Sort(_recipesArray, (x, y) => x.Id.CompareTo(y.Id));
		}

		private void Allocate(int id)
		{
			var recipe = _recipesArray[id];
			var pool = _poolsArray[id];

			//Double the size of the pool
			for (int i = 0; i < pool.Count; i++)
				pool.Push(recipe.Create());
		}

		internal void Allocate(int id, int count)
		{
			var recipe = _recipesArray[id];
			var pool = _poolsArray[id];

			for (int i = 0; i < count; i++)
				pool.Push(recipe.Create());
		}

		public Modifier Rent(int id)
		{
			var pool = _poolsArray[id];

			if (pool.Count > 0)
				return pool.Pop();

			Allocate(id);
			return pool.Pop();
		}

		public void Return(Modifier modifier)
		{
			modifier.ResetState();

			_poolsArray[modifier.Id].Push(modifier);
		}

		public void Dispose()
		{
			for (int i = 0; i < _poolsArray.Length; i++)
				_poolsArray[i].Clear();

			Instance = null;
		}
	}
}