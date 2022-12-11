using ModifierLibraryLite.Core;
using NUnit.Framework;
using Unity.PerformanceTesting;

namespace ModifierLibraryLite.Tests
{
	public sealed class Benchmarks : BaseModifierTests
	{
		private const int Iterations = 1000;

		[Test, Performance]
		public void BenchNewBasicModifierFromRecipe()
		{
			//No cloning right now
			var modifierRecipe = Recipes.GetRecipe("InitDamage");

			Measure.Method(() =>
				{
					var modifier = modifierRecipe.Create();
				})
				.WarmupCount(10)
				.MeasurementCount(80)
				.IterationsPerMeasurement(Iterations)
				.GC()
				.Run()
				;
		}

		[Test, Performance]
		public void BenchNewMediumModifierFromRecipe()
		{
			//We clone two TimeComponents here
			var modifierRecipe = Recipes.GetRecipe("InitDoTSeparateDamageRemove");

			Measure.Method(() =>
				{
					var modifier = modifierRecipe.Create();
				})
				.WarmupCount(10)
				.MeasurementCount(80)
				.IterationsPerMeasurement(Iterations)
				.GC()
				.Run()
				;
		}
	}
}