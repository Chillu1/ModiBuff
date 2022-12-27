using ModiBuff.Core;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine;

namespace ModiBuff.Tests
{
	public sealed class BenchInitialization
	{
		[Test, Performance]
		public void BenchSetupRecipes()
		{
			var _ = new ModifierIdManager();

			Measure.Method(() =>
				{
					var recipes = new TestModifierRecipes();
				})
				.CleanUp(() => ModifierIdManager.Reset())
				.BenchGC(1);
		}

		[Test, Performance]
		[TestCase(100)]
		[TestCase(1000)]
		[TestCase(5000)]
		public void BenchAllocatePool(int n)
		{
			var _ = new ModifierIdManager();
			var recipes = new TestModifierRecipes();
			ModifierPool pool = null;

			Measure.Method(() => { pool = new ModifierPool(recipes.GetRecipes(), n); })
				.CleanUp(() =>
				{
					ModifierIdManager.Reset();
					pool.Dispose();
				})
				.BenchGCLow(1);

			Debug.Log("Allocated: " + ModifierRecipes.RecipesCount * n + " modifiers");
		}
	}
}