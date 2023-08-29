using System;
using BenchmarkDotNet.Attributes;
using ModiBuff.Core;

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
			_modifierIdManager = new ModifierIdManager();
			_recipes = new TestModifierRecipes(_modifierIdManager);

			Console.WriteLine("Allocated: " + ModifierRecipes.RecipesCount + " recipes, count: " + AllocationCount + " modifiers");
		}

		[Benchmark]
		public void BenchAllocatePool()
		{
			_pool = new ModifierPool(_recipes.GetRecipes(), AllocationCount);
		}

		[IterationCleanup]
		public void IterationCleanUp()
		{
			//_modifierIdManager.Reset();
			_pool.Dispose();
		}
	}
}