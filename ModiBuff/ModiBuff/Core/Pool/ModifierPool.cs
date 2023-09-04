using System;

namespace ModiBuff.Core
{
	public sealed class ModifierPool : IDisposable
	{
		public static ModifierPool Instance { get; private set; }

		public static int MaxPoolSize = Config.MaxPoolSize;

		private readonly Modifier[][] _pools;
		private readonly int[] _poolTops;
		private readonly ModifierCheck[][] _checkPools;
		private readonly int[] _checkPoolTops;
		private readonly IModifierRecipe[] _recipes;
		private readonly IModifierApplyCheckRecipe[] _applyCheckRecipes;

#if DEBUG && !MODIBUFF_PROFILE
		private readonly int[] _maxModifiersCreated;
#endif

		public ModifierPool(ModifierRecipes recipes) : this(recipes.GetRecipes())
		{
		}

		public ModifierPool(IModifierRecipe[] recipes)
		{
			if (Instance != null)
				return;

			Instance = this;

			int initialSize = Math.Max(Config.PoolSize, 1);

			_pools = new Modifier[recipes.Length][];
			_poolTops = new int[recipes.Length];
			_checkPools = new ModifierCheck[recipes.Length][];
			_checkPoolTops = new int[recipes.Length];
			_recipes = new IModifierRecipe[recipes.Length];
			_applyCheckRecipes = new IModifierApplyCheckRecipe[recipes.Length];

#if DEBUG && !MODIBUFF_PROFILE
			_maxModifiersCreated = new int[recipes.Length];
#endif

			foreach (var recipe in recipes)
			{
				if (Config.ModifierAllocationsCount.TryGetValue(recipe.Name, out int count))
					_pools[recipe.Id] = new Modifier[count];
				else
					_pools[recipe.Id] = new Modifier[initialSize];
				_recipes[recipe.Id] = recipe;

				if (recipe is IModifierApplyCheckRecipe applyCheckRecipe)
				{
					if (Config.ModifierAllocationsCount.TryGetValue(applyCheckRecipe.Name, out int applyCount))
						_checkPools[recipe.Id] = new ModifierCheck[applyCount];
					else
						_checkPools[recipe.Id] = new ModifierCheck[initialSize];
					_applyCheckRecipes[recipe.Id] = applyCheckRecipe;

					AllocateDoubleChecks(recipe.Id);
				}

				AllocateDouble(recipe.Id);
			}

			Array.Sort(_pools, (x, y) => x[0].Id.CompareTo(y[0].Id));
			Array.Sort(_poolTops, (x, y) => x.CompareTo(y));
			Array.Sort(_recipes, (x, y) => x.Id.CompareTo(y.Id));
			//Array.Sort(_checkPools, (x, y) => x[0].Id.CompareTo(y[0].Id));
			//Array.Sort(_checkPoolTops, (x, y) => x.CompareTo(y));

#if DEBUG && !MODIBUFF_PROFILE
			for (int i = 0; i < _maxModifiersCreated.Length; i++)
				_maxModifiersCreated[i] = _pools[i].Length;
#endif
		}

		internal void SetMaxPoolSize(int size)
		{
			if (size < 0)
				throw new ArgumentOutOfRangeException(nameof(size), "Max pool size cannot be negative.");

			for (int i = 0; i < _poolTops.Length; i++)
			{
				if (_poolTops[i] > size)
					throw new ArgumentOutOfRangeException(nameof(size), "Max pool size cannot be smaller than the current stack capacity.");
			}

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

#if DEBUG && !MODIBUFF_PROFILE
			if (_pools[id].Length > _maxModifiersCreated[id])
				_maxModifiersCreated[id] = _pools[id].Length;
#endif
		}

		/// <summary>
		///		Doubles the size of the pool, can be very expensive.
		/// </summary>
		internal void AllocateDouble(int id)
		{
			var recipe = _recipes[id];
			int poolLength = _pools[id].Length; //Don't cache pool array, it can be resized.

			if (_poolTops[id] == poolLength)
				Resize(id, poolLength << 1);

			for (int i = 0; i < poolLength; i++)
				_pools[id][_poolTops[id]++] = recipe.Create();

			if (_poolTops[id] > MaxPoolSize)
				throw new Exception($"Modifier pool for {recipe.Name} is over the max pool size of {MaxPoolSize}.");

#if DEBUG && !MODIBUFF_PROFILE
			if (_pools[id].Length > _maxModifiersCreated[id])
				_maxModifiersCreated[id] = _pools[id].Length;
#endif
		}

		internal void AllocateDoubleChecks(int id)
		{
			var recipe = _applyCheckRecipes[id];
			int poolLength = _checkPools[id].Length; //Don't cache pool array, it can be resized.

			if (_checkPoolTops[id] == poolLength)
				ResizeChecks(id, poolLength << 1);

			if (recipe.HasApplyChecks)
				for (int i = 0; i < poolLength; i++)
					_checkPools[id][_checkPoolTops[id]++] = recipe.CreateApplyCheck();

			if (_checkPoolTops[id] > MaxPoolSize)
				throw new Exception($"Modifier check pool for {recipe.Name} is over the max pool size of {MaxPoolSize}.");
		}

		public Modifier Rent(int id)
		{
			if (_poolTops[id] == 0)
				AllocateDouble(id);

			return _pools[id][--_poolTops[id]];
		}

		public ModifierCheck RentModifierCheck(int id)
		{
			if (_checkPoolTops[id] == 0)
				AllocateDoubleChecks(id);

			return _checkPools[id][--_checkPoolTops[id]];
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

#if DEBUG && !MODIBUFF_PROFILE
		public void PrintMaxModifiersCreated()
		{
			//Print out max modifier sorted by biggest pool
			int[] maxModifiersCreated = new int[_recipes.Length];
			Array.Copy(_maxModifiersCreated, maxModifiersCreated, _recipes.Length);
			Array.Sort(maxModifiersCreated, (x, y) => y.CompareTo(x));
			for (int i = 0; i < maxModifiersCreated.Length; i++)
				Logger.Log($"{_recipes[i].Name}: {maxModifiersCreated[i]}");
		}
#endif

		internal void Clear()
		{
			for (int i = 0; i < _pools.Length; i++)
			{
				Array.Clear(_pools[i], 0, _pools[i].Length);
				_poolTops[i] = 0;
				if (_checkPools[i] != null)
				{
					Array.Clear(_checkPools[i], 0, _checkPools[i].Length);
					_checkPoolTops[i] = 0;
				}
			}
		}

		public void Dispose()
		{
			if (_pools == null) //Somehow this is true in bench?
				return;

			Clear();

			Instance = null;
		}
	}
}