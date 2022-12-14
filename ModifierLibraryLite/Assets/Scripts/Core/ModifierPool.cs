using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModifierLibraryLite.Core
{
	public sealed class ModifierPool : IDisposable
	{
		public static ModifierPool Instance { get; private set; }

		private const int MaxPoolSize = 100_000;

		private readonly Stack<Modifier>[] _pools;
		private readonly ModifierRecipe[] _recipes;

		private int _stackCapacity = 64;

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

			_stackCapacity = initialSize;

			Array.Sort(_pools, (x, y) => x.Peek().Id.CompareTo(y.Peek().Id));
			Array.Sort(_recipes, (x, y) => x.Id.CompareTo(y.Id));
		}

		private void Allocate(int id)
		{
			var recipe = _recipes[id];
			var pool = _pools[id];

			//Double the size of the pool
			//TODO
			throw new NotImplementedException();
		}

		internal void Allocate(int id, int count)
		{
			var recipe = _recipes[id];
			var pool = _pools[id];

			for (int i = 0; i < count; i++)
				pool.Push(recipe.Create());

			_stackCapacity += count;
			if (_stackCapacity > MaxPoolSize)
				Debug.LogError("ModifierPool exceeded max size of " + MaxPoolSize);
		}

		public Modifier Rent(int id)
		{
			var pool = _pools[id];

			if (pool.Count > 0)
				return pool.Pop();

			Allocate(id, _stackCapacity);
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

			_stackCapacity = 0;
			Instance = null;
		}
	}
}