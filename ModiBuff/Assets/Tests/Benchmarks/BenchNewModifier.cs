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
			}).BenchGC();
		}

		[Test, Performance]
		public void BenchNewMediumModifierFromRecipe()
		{
			//We clone two TimeComponents here
			var modifierRecipe = Recipes.GetRecipe("InitDoTSeparateDamageRemove");

			Measure.Method(() =>
			{
				var modifier = modifierRecipe.Create();
			}).BenchGC();
		}

		[Test, Performance]
		public void BenchPooledMediumModifierFromRecipe()
		{
			var recipe = Recipes.GetRecipe("InitDoTSeparateDamageRemove");
			Pool.SetMaxPoolSize(1_000_000);
			Pool.Allocate(recipe.Id, 60 * Iterations);

			Measure.Method(() =>
			{
				var modifier = Pool.Rent(recipe.Id);
			}).BenchGC();
		}

		[Test, Performance]
		public void BenchPooledMediumModifierFromRecipeReturn()
		{
			var recipe = Recipes.GetRecipe("InitDoTSeparateDamageRemove");

			Measure.Method(() =>
			{
				var modifier = Pool.Rent(recipe.Id);
				Pool.Return(modifier);
			}).BenchGC();
		}

		[Test, Performance]
		public void BenchPooledFullStateModifierFromRecipeReturn()
		{
			var recipe = Recipes.GetRecipe("IntervalDamage_StackAddDamage");

			Measure.Method(() =>
			{
				var modifier = Pool.Rent(recipe.Id);
				Pool.Return(modifier);
			}).BenchGC();
		}
	}
}