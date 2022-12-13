using System;
using System.Collections.Generic;
using ModifierLibraryLite.Core;
using NUnit.Framework;
using Unity.PerformanceTesting;

namespace ModifierLibraryLite.Tests
{
	public class BenchContainsModifier : BaseModifierTests
	{
		private const int Iterations = 20000;

		private static int[] _modifierCount = { 5, 15, 50 };

		//Conclusions:
		//Find and LinearSearch scales terribly and is overall bad
		//FullArray > Dictionary > BinarySearch
		//FullArray & Dictionary force single-modifier logic
		//But tbh, don't think we want duplicate modifier's on one Unit anyway

		[Test, Performance]
		[TestCaseSource(nameof(_modifierCount))]
		public void ContainsModifier_Dictionary(int n)
		{
			var dictionary = new Dictionary<int, Modifier>();
			var recipe = Recipes.GetRecipe("DurationRefreshRemove_IntervalDamage");

			for (int k = 0; k < n; k++)
			for (int i = 0; i < 10; i++)
			{
				//if (k * 10 + i == recipe.Id)
				//	continue;
				dictionary.Add(k * 10 + i, Recipes.GetRecipe(i).Create());
			}

			//dictionary.Add(recipe.Id, recipe.Create());

			Measure.Method(() =>
				{
					if (dictionary.TryGetValue(recipe.Id, out var modifier))
					{
						var m = modifier;
					}
				})
				.WarmupCount(10)
				.MeasurementCount(50)
				.IterationsPerMeasurement(Iterations)
				.Run()
				;
		}

		[Test, Performance]
		[TestCaseSource(nameof(_modifierCount))]
		public void ContainsModifier_BinarySearch(int n)
		{
			var list = new List<Modifier>();

			for (int k = 0; k < n; k++)
			for (int i = 0; i < 10; i++)
				list.Add(Recipes.GetRecipe(i).Create());

			var recipe = Recipes.GetRecipe("DurationRefreshRemove_IntervalDamage");
			var comparer = Comparer<Modifier>.Create((a, b) => a.Id.CompareTo(b.Id));
			list.Sort(comparer);

			var modifier = recipe.Create();

			Measure.Method(() =>
				{
					int index = list.BinarySearch(modifier, comparer);
					if (index >= 0)
					{
						var m = list[index];
					}
				})
				.WarmupCount(10)
				.MeasurementCount(50)
				.IterationsPerMeasurement(Iterations)
				.Run()
				;
		}

		[Test, Performance]
		[TestCaseSource(nameof(_modifierCount))]
		public void ContainsModifier_FullArray(int n)
		{
			//int lastRecipeId = Recipes.GetRecipe("ChanceInitDamage").Id;
			var modifiers = new Modifier[n * 10];
			for (int k = 0; k < n; k++)
			for (int i = 0; i < 10; i++)
				modifiers[k * 10 + i] = Recipes.GetRecipe(i).Create();

			var recipe = Recipes.GetRecipe("DurationRefreshRemove_IntervalDamage");
			//modifiers[recipe.Id] = recipe.Create();


			Measure.Method(() =>
				{
					var m = modifiers[recipe.Id];
				})
				.WarmupCount(10)
				.MeasurementCount(50)
				.IterationsPerMeasurement(Iterations)
				.Run()
				;
		}

		//[Test, Performance]
		//[TestCaseSource(nameof(_modifierCount))]
		public void ContainsModifier_LinearSearch(int n)
		{
			var list = new List<Modifier>();
			for (int k = 0; k < n; k++)
			for (int i = 0; i < 10; i++)
				list.Add(Recipes.GetRecipe(i).Create());

			var recipe = Recipes.GetRecipe("DurationRefreshRemove_IntervalDamage");
			//list.Add(recipe.Create());

			Measure.Method(() =>
				{
					int count = list.Count;
					for (int i = 0; i < count; i++)
					{
						if (list[i].Id == recipe.Id)
						{
							var m = list[i];
							break;
						}
					}
				})
				.WarmupCount(10)
				.MeasurementCount(50)
				.IterationsPerMeasurement(Iterations)
				.Run()
				;
		}
	}
}