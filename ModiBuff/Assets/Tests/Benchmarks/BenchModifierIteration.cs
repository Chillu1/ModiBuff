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
	}
}