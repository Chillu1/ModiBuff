using BenchmarkDotNet.Attributes;
using ModiBuff.Core;

namespace ModiBuff.Tests
{
	[MemoryDiagnoser]
	public class BenchPoolRent : ModifierBenches
	{
		private const int Iterations = (int)1e5;

		private IModifierGenerator _initDoTSeparateDamageRemoveGenerator;

		public override void GlobalSetup()
		{
			base.GlobalSetup();

			_initDoTSeparateDamageRemoveGenerator = Recipes.GetGenerator("InitDoTSeparateDamageRemove");

			Pool.Clear();
			Pool.SetMaxPoolSize(1_000_000);

			Pool.Allocate(_initDoTSeparateDamageRemoveGenerator.Id, Iterations);
		}

		[IterationSetup]
		public void IterationSetup()
		{
			Pool.Clear();

			Pool.Allocate(_initDoTSeparateDamageRemoveGenerator.Id, Iterations);
		}

		[Benchmark(OperationsPerInvoke = Iterations)]
		public void BenchPooledMediumModifierFromRecipe()
		{
			for (int i = 0; i < Iterations; i++)
			{
				var modifier = Pool.Rent(_initDoTSeparateDamageRemoveGenerator.Id);
			}
		}
	}
}