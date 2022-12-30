using System;
using UnityEngine;

namespace ModiBuff.Core
{
	public sealed class ModifierPool : IDisposable
	{
		public static ModifierPool Instance { get; private set; }

		public static int MaxPoolSize = 10_000;

		private readonly Modifier[][] _pools;
		private readonly int[] _poolTops;
		private readonly IModifierRecipe[] _recipes;

		private int _stackCapacity = 64;

		public ModifierPool(IModifierRecipe[] recipes, int initialSize = 64)
		{
			if (Instance != null)
				return;

			Instance = this;

			initialSize = Mathf.Max(initialSize, 1);

			_pools = new Modifier[recipes.Length][];
			_poolTops = new int[recipes.Length];
			_recipes = new IModifierRecipe[recipes.Length];

			foreach (var recipe in recipes)
			{
				_pools[recipe.Id] = new Modifier[initialSize];
				_recipes[recipe.Id] = recipe;

				Allocate(recipe.Id, initialSize);
			}

			_stackCapacity = initialSize;

			Array.Sort(_pools, (x, y) => x[0].Id.CompareTo(y[0].Id));
			Array.Sort(_recipes, (x, y) => x.Id.CompareTo(y.Id));
		}

		internal void SetMaxPoolSize(int size)
		{
			if (size < 0)
				throw new ArgumentOutOfRangeException(nameof(size), "Max pool size cannot be negative.");

			if (size < _stackCapacity)
				throw new ArgumentOutOfRangeException(nameof(size), "Max pool size cannot be smaller than the current stack capacity.");

			MaxPoolSize = size;
		}

		internal void Resize(int id, int size)
		{
			var pool = _pools[id];

			Array.Resize(ref pool, size);
			_pools[id] = pool;
		}

		internal void Allocate(int id, int count)
		{
			var recipe = _recipes[id];
			int poolLength = _pools[id].Length; //Don't cache pool array, it can be resized.

			if (count > poolLength)
			{
				int newSize = poolLength << 1;
				while (newSize < poolLength + count)
					newSize <<= 1;
				Resize(id, newSize);
			}

			for (int i = 0; i < count; i++)
				_pools[id][_poolTops[id]++] = recipe.Create();

			if (_poolTops[id] > MaxPoolSize)
				Debug.LogError($"Modifier pool for {recipe.Name} is over the max pool size of {MaxPoolSize}.");
		}

		public Modifier Rent(int id)
		{
			var pool = _pools[id];

			if (_poolTops[id] == 0)
				Allocate(id, _stackCapacity);

			return pool[--_poolTops[id]];
		}

		public void Return(Modifier modifier)
		{
			modifier.ResetState();

			if (_poolTops[modifier.Id] == _pools[modifier.Id].Length)
				Resize(modifier.Id, _pools[modifier.Id].Length << 1);

			_pools[modifier.Id][_poolTops[modifier.Id]++] = modifier;
		}

		internal void Clear()
		{
			for (int i = 0; i < _pools.Length; i++)
			{
				Array.Clear(_pools[i], 0, _pools[i].Length);
				_poolTops[i] = 0;
			}
		}

		public void Dispose()
		{
			Clear();

			_stackCapacity = 1;
			Instance = null;
		}
	}
}