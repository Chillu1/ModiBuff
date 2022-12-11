using ModifierLibraryLite.Core;
using NUnit.Framework;
using Unity.PerformanceTesting;

namespace ModifierLibraryLite.Tests
{
	public sealed class Benchmarks : BaseModifierTests
	{
		private const int Iterations = 1000;

		[Test, Performance]
		public void BenchNewModifierFromRecipe()
		{
			var modifierRecipe = Recipes.GetRecipe("InitDamage");

			Measure.Method(() =>
				{
					var modifier = modifierRecipe.Create();
				})
				.WarmupCount(5)
				.MeasurementCount(50)
				.IterationsPerMeasurement(Iterations)
				.GC()
				.Run()
				;
		}
	}
}