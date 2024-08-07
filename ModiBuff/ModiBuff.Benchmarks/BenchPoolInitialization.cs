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
		private EffectTypeIdManager _effectTypeIdManager;
		private BenchmarkModifierRecipes _recipes;
		private ModifierPool _pool;

		[GlobalSetup]
		public void GlobalSetup()
		{
			Config.PoolSize = AllocationCount;

			_modifierIdManager = new ModifierIdManager();
			_effectTypeIdManager = new EffectTypeIdManager();
			_effectTypeIdManager.RegisterAllEffectTypesInAssemblies();
			_recipes = new BenchmarkModifierRecipes(_modifierIdManager, _effectTypeIdManager);

			Console.WriteLine("Allocated: " + ModifierRecipes.GeneratorCount + " recipes, count: " + AllocationCount +
			                  " modifiers");
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