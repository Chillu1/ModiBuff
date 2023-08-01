using System;

namespace ModiBuff.Core
{
	public sealed class ModifierPool : IDisposable
	{
		public static ModifierPool Instance { get; private set; }

		public const int DefaultMaxPoolSize = 10_000;
		public static int MaxPoolSize = DefaultMaxPoolSize;

		private readonly Modifier[][] _pools;
		private readonly int[] _poolTops;
		private readonly ModifierCheck[][] _checkPools;
		private readonly int[] _checkPoolTops;
		private readonly IModifierRecipe[] _recipes;

		private int _stackCapacity = 64;

		public ModifierPool(IModifierRecipe[] recipes, int initialSize = 64)
		{
			if (Instance != null)
				return;

			Instance = this;

			initialSize = Math.Max(initialSize, 1);

			_pools = new Modifier[recipes.Length][];
			_poolTops = new int[recipes.Length];
			_checkPools = new ModifierCheck[recipes.Length][];
			_checkPoolTops = new int[recipes.Length];
			_recipes = new IModifierRecipe[recipes.Length];

			foreach (var recipe in recipes)
			{
				_pools[recipe.Id] = new Modifier[initialSize];
				_recipes[recipe.Id] = recipe;

				//if (recipe.HasApplyChecks)
				{
					_checkPools[recipe.Id] = new ModifierCheck[initialSize];
					AllocateChecks(recipe.Id, initialSize);
				}

				Allocate(recipe.Id, initialSize);
			}

			_stackCapacity = initialSize;

			Array.Sort(_pools, (x, y) => x[0].Id.CompareTo(y[0].Id));
			Array.Sort(_poolTops, (x, y) => x.CompareTo(y));
			Array.Sort(_recipes, (x, y) => x.Id.CompareTo(y.Id));
			//Array.Sort(_checkPools, (x, y) => x[0].Id.CompareTo(y[0].Id));
			//Array.Sort(_checkPoolTops, (x, y) => x.CompareTo(y));
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

		internal void ResizeChecks(int id, int size)
		{
			var pool = _checkPools[id];

			Array.Resize(ref pool, size);
			_checkPools[id] = pool;
		}

		internal void Allocate(int id, int count)
		{
			var recipe = _recipes[id];
			int poolLength = _pools[id].Length; //Don't cache pool array, it can be resized.

			if (count + _poolTops[id] > poolLength)
			{
				int newSize = poolLength << 1;
				while (newSize < poolLength + count)
					newSize <<= 1;
				Resize(id, newSize);
			}

			for (int i = 0; i < count; i++)
				_pools[id][_poolTops[id]++] = recipe.Create();

			if (_poolTops[id] > MaxPoolSize)
				throw new Exception($"Modifier pool for {recipe.Name} is over the max pool size of {MaxPoolSize}.");
		}

		internal void AllocateChecks(int id, int count)
		{
			var recipe = _recipes[id];
			int poolLength = _checkPools[id].Length; //Don't cache pool array, it can be resized.

			if (count > poolLength)
			{
				int newSize = poolLength << 1;
				while (newSize < poolLength + count)
					newSize <<= 1;

				ResizeChecks(id, newSize);
			}

			if (recipe.HasApplyChecks)
				for (int i = 0; i < count; i++)
					_checkPools[id][_checkPoolTops[id]++] = recipe.CreateApplyCheck();

			if (_checkPoolTops[id] > MaxPoolSize)
				throw new Exception($"Modifier check pool for {recipe.Name} is over the max pool size of {MaxPoolSize}.");
		}

		public Modifier Rent(int id)
		{
			var pool = _pools[id];

			if (_poolTops[id] == 0)
				Allocate(id, _stackCapacity);

			return pool[--_poolTops[id]];
		}

		public ModifierCheck RentModifierCheck(int id)
		{
			var pool = _checkPools[id];

			if (_checkPoolTops[id] == 0)
				AllocateChecks(id, _stackCapacity);

			return pool[--_checkPoolTops[id]];
		}

		public void Return(Modifier modifier)
		{
			modifier.ResetState();

			if (_poolTops[modifier.Id] == _pools[modifier.Id].Length)
				Resize(modifier.Id, _pools[modifier.Id].Length << 1);

			_pools[modifier.Id][_poolTops[modifier.Id]++] = modifier;
		}

		public void ReturnCheck(ModifierCheck check)
		{
			check.ResetState();

			if (_checkPoolTops[check.Id] == _checkPools[check.Id].Length)
				ResizeChecks(check.Id, _checkPools[check.Id].Length << 1);

			_checkPools[check.Id][_checkPoolTops[check.Id]++] = check;
		}

		internal void Clear()
		{
			for (int i = 0; i < _pools.Length; i++)
			{
				Array.Clear(_pools[i], 0, _pools[i].Length);
				_poolTops[i] = 0;
				Array.Clear(_checkPools[i], 0, _checkPools[i].Length);
				_checkPoolTops[i] = 0;
			}
		}

		public void Dispose()
		{
			if (_pools == null) //Somehow this is true in bench?
				return;

			Clear();

			_stackCapacity = 1;
			Instance = null;
		}
	}
}