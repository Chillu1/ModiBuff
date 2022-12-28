using System.Collections.Generic;
using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;
using Unity.PerformanceTesting;

namespace ModiBuff.Tests
{
	public sealed class BenchModifierIteration : BaseModifierTests
	{
		[Test, Performance]
		[TestCase(0.0167f)]
		[TestCase(1f)]
		public void BenchDoTIteration(float delta)
		{
			Pool.Allocate(ModifierIdManager.GetId("DoT"), 5_000);

			var units = new Unit[5_000];
			for (int i = 0; i < units.Length; i++)
			{
				units[i] = new Unit();
				units[i].TryAddModifierSelf("DoT");
			}

			Measure.Method(() =>
				{
					for (int i = 0; i < units.Length; i++)
						units[i].Update(delta);
				})
				.BenchGC(1);
		}

		[Test, Performance]
		[TestCase(0.0167f)]
		[TestCase(1f)]
		public void BenchDoTIterationSingle(float delta)
		{
			var unit = new Unit();
			unit.TryAddModifierSelf("DoT");

			Measure.Method(() => { unit.Update(delta); })
				.BenchGC(5_000);
		}

		private const int CollectionIterations = 500;
		private const int CollectionSize = 100;
		private readonly int[] _indexes = { 25, 50, 75 };


		[Test, Performance]
		public void BenchModifierArrayIterationPerfect()
		{
			var modifiers = new Modifier[CollectionSize];
			modifiers[_indexes[0]] = Recipes.GetRecipe("DoT").Create();
			modifiers[_indexes[0]].SetTargets(Unit, Unit);
			modifiers[_indexes[1]] = Recipes.GetRecipe("DoT").Create();
			modifiers[_indexes[1]].SetTargets(Unit, Unit);
			modifiers[_indexes[2]] = Recipes.GetRecipe("DoT").Create();
			modifiers[_indexes[2]].SetTargets(Unit, Unit);
			int[] indexes = { _indexes[0], _indexes[1], _indexes[2] };

			Measure.Method(() =>
				{
					float delta = 0.0167f;
					for (int i = 0; i < 100; i++) //Simulates 100 units
					{
						int length = indexes.Length;
						for (int j = 0; j < length; j++)
							modifiers[indexes[j]].Update(delta);
					}
				})
				.Bench(CollectionIterations);
		}

		[Test, Performance]
		public void BenchModifierArrayIteration()
		{
			var modifiers = new Modifier[CollectionSize];
			modifiers[_indexes[0]] = Recipes.GetRecipe("DoT").Create();
			modifiers[_indexes[0]].SetTargets(Unit, Unit);
			modifiers[_indexes[1]] = Recipes.GetRecipe("DoT").Create();
			modifiers[_indexes[1]].SetTargets(Unit, Unit);
			modifiers[_indexes[2]] = Recipes.GetRecipe("DoT").Create();
			modifiers[_indexes[2]].SetTargets(Unit, Unit);

			Measure.Method(() =>
				{
					float delta = 0.0167f;
					for (int i = 0; i < 100; i++) //Simulates 100 units
					{
						int length = modifiers.Length;
						for (int j = 0; j < length; j++)
						{
							var modifier = modifiers[j];
							if (modifier == null)
								continue;

							modifier.Update(delta);
						}
					}
				})
				.Bench(CollectionIterations);
		}

		[Test, Performance]
		public void BenchModifierDictionaryIteration()
		{
			var modifiers = new Dictionary<int, Modifier>(CollectionSize);
			modifiers[_indexes[0]] = Recipes.GetRecipe("DoT").Create();
			modifiers[_indexes[0]].SetTargets(Unit, Unit);
			modifiers[_indexes[1]] = Recipes.GetRecipe("DoT").Create();
			modifiers[_indexes[1]].SetTargets(Unit, Unit);
			modifiers[_indexes[2]] = Recipes.GetRecipe("DoT").Create();
			modifiers[_indexes[2]].SetTargets(Unit, Unit);

			Measure.Method(() =>
				{
					float delta = 0.0167f;
					for (int i = 0; i < 100; i++) //Simulates 100 units
						foreach (var modifier in modifiers.Values)
							modifier.Update(delta);
				})
				.Bench(CollectionIterations);
		}

		[Test, Performance]
		public void BenchModifierListIteration()
		{
			var modifiers = new List<Modifier>(CollectionSize);
			modifiers.Add(Recipes.GetRecipe("DoT").Create());
			modifiers[0].SetTargets(Unit, Unit);
			modifiers.Add(Recipes.GetRecipe("DoT").Create());
			modifiers[1].SetTargets(Unit, Unit);
			modifiers.Add(Recipes.GetRecipe("DoT").Create());
			modifiers[2].SetTargets(Unit, Unit);

			Measure.Method(() =>
				{
					float delta = 0.0167f;
					for (int i = 0; i < 100; i++) //Simulates 100 units
					{
						int length = modifiers.Count;
						for (int j = 0; j < length; j++)
							modifiers[j].Update(delta);
					}
				})
				.Bench(CollectionIterations);
		}

		[Test, Performance]
		public void BenchModifierListIndexIteration()
		{
			var modifiers = new Modifier[CollectionSize];
			var indexes = new List<int>();
			modifiers[_indexes[0]] = Recipes.GetRecipe("DoT").Create();
			modifiers[_indexes[0]].SetTargets(Unit, Unit);
			indexes.Add(_indexes[0]);
			modifiers[_indexes[1]] = Recipes.GetRecipe("DoT").Create();
			modifiers[_indexes[1]].SetTargets(Unit, Unit);
			indexes.Add(_indexes[1]);
			modifiers[_indexes[2]] = Recipes.GetRecipe("DoT").Create();
			modifiers[_indexes[2]].SetTargets(Unit, Unit);
			indexes.Add(_indexes[2]);

			Measure.Method(() =>
				{
					float delta = 0.0167f;
					for (int i = 0; i < 100; i++) ////Simulates 100 units
					{
						int length = indexes.Count;
						for (int j = 0; j < length; j++)
							modifiers[indexes[j]].Update(delta);
					}
				})
				.Bench(CollectionIterations);
		}
	}
}