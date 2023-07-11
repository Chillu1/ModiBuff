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
			var modifierIdManager = new ModifierIdManager();

			Measure.Method(() =>
				{
					var recipes = new TestModifierRecipes(modifierIdManager);
				})
				.CleanUp(() => modifierIdManager.Reset())
				.BenchGC(1);
		}

		[Test, Performance]
		[TestCase(100)]
		[TestCase(500)]
		[TestCase(1000)]
		public void BenchAllocatePool(int n)
		{
			var modifierIdManager = new ModifierIdManager();
			var recipes = new TestModifierRecipes(modifierIdManager);
			ModifierPool pool = null;

			Measure.Method(() => { pool = new ModifierPool(recipes.GetRecipes(), n); })
				.CleanUp(() =>
				{
					modifierIdManager.Reset();
					pool.Dispose();
				})
				.BenchGCLow(1);

			Debug.Log("Allocated: " + ModifierRecipesBase.RecipesCount * n + " modifiers");
		}
	}
}