using BenchmarkDotNet.Attributes;
using ModiBuff.Core;

namespace ModiBuff.Tests
{
	[MemoryDiagnoser]
	public class BenchInitialization
	{
		[GlobalSetup]
		public void GlobalSetup()
		{
			Config.PoolSize = 1024;
		}

		[Benchmark]
		public void BenchSetupRecipes()
		{
			var idManager = new ModifierIdManager();
			var recipes = new BenchmarkModifierRecipes(idManager);
			var pool = new ModifierPool(recipes.GetGenerators());
			idManager.Reset();
		}
	}
}