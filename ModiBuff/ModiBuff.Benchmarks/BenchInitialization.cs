using BenchmarkDotNet.Attributes;
using ModiBuff.Core;
using ModiBuff.Core.Units;

namespace ModiBuff.Tests
{
	[MemoryDiagnoser]
	public class BenchInitialization
	{
		[Benchmark]
		public void BenchSetupRecipes()
		{
			var idManager = new ModifierIdManager();
			var recipes = new TestModifierRecipes(idManager);
			var pool = new ModifierPool(recipes.GetGenerators());
			idManager.Reset();
		}
	}
}