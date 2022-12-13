using NUnit.Framework;
using Unity.PerformanceTesting;

namespace ModifierLibraryLite.Tests
{
	public sealed class Benchmarks : BaseModifierTests
	{
		private const int Iterations = 5000;

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
				.MeasurementCount(50)
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
				.MeasurementCount(50)
				.IterationsPerMeasurement(Iterations)
				.GC()
				.Run()
				;
		}

		[Test, Performance]
		public void BenchPooledMediumModifierFromRecipe()
		{
			var recipe = Recipes.GetRecipe("InitDoTSeparateDamageRemove");
			Pool.Allocate(recipe.IdInt, 60 * Iterations);

			Measure.Method(() =>
				{
					var modifier = Pool.Rent(recipe.IdInt);
				})
				.WarmupCount(10)
				.MeasurementCount(50)
				.IterationsPerMeasurement(Iterations)
				.GC()
				.Run()
				;
		}

		[Test, Performance]
		public void BenchPooledMediumModifierFromRecipeReturn()
		{
			var recipe = Recipes.GetRecipe("InitDoTSeparateDamageRemove");

			Measure.Method(() =>
				{
					var modifier = Pool.Rent(recipe.IdInt);
					Pool.Return(modifier);
				})
				.WarmupCount(10)
				.MeasurementCount(50)
				.IterationsPerMeasurement(Iterations)
				.GC()
				.Run()
				;
		}
	}
}