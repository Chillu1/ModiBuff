using System;
using BenchmarkDotNet.Attributes;
using ModiBuff.Core;
using ModiBuff.Core.Units;

namespace ModiBuff.Tests
{
	[MemoryDiagnoser]
	public class BenchPoolInitialization
	{
		[Params(100, 1000, 10000)]
		public int AllocationCount;

		private ModifierIdManager _modifierIdManager;
		private TestModifierRecipes _recipes;
		private ModifierPool _pool;

		[GlobalSetup]
		public void GlobalSetup()
		{
			Config.PoolSize = AllocationCount;

			_modifierIdManager = new ModifierIdManager();
			_recipes = new TestModifierRecipes(_modifierIdManager);

			Console.WriteLine("Allocated: " + ModifierRecipes.RecipesCount + " recipes, count: " + AllocationCount + " modifiers");
		}

		[Benchmark]
		public void BenchAllocatePool()
		{
			_pool = new ModifierPool(_recipes.GetGenerators());
		}

		[IterationCleanup]
		public void IterationCleanUp()
		{
			//_modifierIdManager.Reset();
			_pool.Reset();
		}
	}
}