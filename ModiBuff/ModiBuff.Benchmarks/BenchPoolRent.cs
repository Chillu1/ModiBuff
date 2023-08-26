using BenchmarkDotNet.Attributes;
using ModiBuff.Core;

namespace ModiBuff.Tests
{
	[MemoryDiagnoser]
	public class BenchPoolRent : ModifierBenches
	{
		private const int Iterations = (int)1e5;

		private IModifierRecipe _initDoTSeparateDamageRemoveRecipe;

		public override void GlobalSetup()
		{
			base.GlobalSetup();

			_initDoTSeparateDamageRemoveRecipe = Recipes.GetRecipe("InitDoTSeparateDamageRemove");

			Pool.Clear();
			Pool.SetMaxPoolSize(1_000_000);

			Pool.Allocate(_initDoTSeparateDamageRemoveRecipe.Id, Iterations);
		}

		[IterationSetup]
		public void IterationSetup()
		{
			Pool.Clear();

			Pool.Allocate(_initDoTSeparateDamageRemoveRecipe.Id, Iterations);
		}

		[Benchmark(OperationsPerInvoke = 1)]
		public void BenchPooledMediumModifierFromRecipe()
		{
			for (int i = 0; i < Iterations; i++)
			{
				var modifier = Pool.Rent(_initDoTSeparateDamageRemoveRecipe.Id);
			}
		}
	}
}