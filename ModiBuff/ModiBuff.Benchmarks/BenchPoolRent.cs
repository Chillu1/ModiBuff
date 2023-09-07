using BenchmarkDotNet.Attributes;
using ModiBuff.Core;

namespace ModiBuff.Tests
{
	[MemoryDiagnoser]
	public class BenchPoolRent : ModifierBenches
	{
		private const int Iterations = (int)1e5;

		private int _initDoTSeparateDamageRemoveId;

		public override void GlobalSetup()
		{
			base.GlobalSetup();

			_initDoTSeparateDamageRemoveId = Recipes.GetGenerator("InitDoTSeparateDamageRemove").Id;

			Pool.Clear();
			Pool.SetMaxPoolSize(1_000_000);

			Pool.Allocate(_initDoTSeparateDamageRemoveId, Iterations);
		}

		[IterationSetup]
		public void IterationSetup()
		{
			Pool.Clear();

			Pool.Allocate(_initDoTSeparateDamageRemoveId, Iterations);
		}

		[Benchmark(OperationsPerInvoke = Iterations)]
		public void BenchPooledMediumModifierFromRecipe()
		{
			for (int i = 0; i < Iterations; i++)
			{
				var modifier = Pool.Rent(_initDoTSeparateDamageRemoveId);
			}
		}
	}
}