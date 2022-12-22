using NUnit.Framework;
using Unity.PerformanceTesting;

namespace ModiBuff.Tests
{
	public sealed class BenchNewModifier : BaseModifierTests
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
			}).BenchGC(Iterations);
		}

		[Test, Performance]
		public void BenchNewMediumModifierFromRecipe()
		{
			var modifierRecipe = Recipes.GetRecipe("InitDoTSeparateDamageRemove");

			Measure.Method(() =>
			{
				var modifier = modifierRecipe.Create();
			}).BenchGC(Iterations);
		}

		[Test, Performance]
		public void BenchPooledMediumModifierFromRecipe()
		{
			Pool.Clear();
			var recipe = Recipes.GetRecipe("InitDoTSeparateDamageRemove");
			Pool.SetMaxPoolSize(1_000_000);
			Pool.Allocate(recipe.Id, 60 * Iterations);

			Measure.Method(() =>
			{
				var modifier = Pool.Rent(recipe.Id);
			}).BenchGC(Iterations);
		}

		[Test, Performance]
		public void BenchPooledMediumModifierFromRecipeReturn()
		{
			Pool.Clear();
			var recipe = Recipes.GetRecipe("InitDoTSeparateDamageRemove");
			Pool.Allocate(recipe.Id, Iterations);

			Measure.Method(() =>
			{
				var modifier = Pool.Rent(recipe.Id);
				Pool.Return(modifier);
			}).BenchGC(Iterations);
		}

		[Test, Performance]
		public void BenchPooledFullStateModifierFromRecipeReturn()
		{
			Pool.Clear();
			var recipe = Recipes.GetRecipe("IntervalDamage_StackAddDamage");
			Pool.Allocate(recipe.Id, Iterations);

			Measure.Method(() =>
			{
				var modifier = Pool.Rent(recipe.Id);
				Pool.Return(modifier);
			}).BenchGC(Iterations);
		}
	}
}