using ModifierLibraryLite.Core;
using NUnit.Framework;
using Unity.PerformanceTesting;

namespace ModifierLibraryLite.Tests
{
	public sealed class BenchAddModifier : BaseModifierTests
	{
		private const int Iterations = 5000;

		[Test, Performance]
		public void BenchAddSimpleModifier()
		{
			int modifierId = ModifierIdManager.GetId("InitDamage");

			Measure.Method(() => Unit.TryAddModifier(modifierId, Unit, Unit))
				.WarmupCount(10)
				.MeasurementCount(50)
				.IterationsPerMeasurement(Iterations)
				.GC()
				.Run()
				;
		}

		[Test, Performance]
		public void BenchAddMediumModifier()
		{
			int modifierId = ModifierIdManager.GetId("InitDoTSeparateDamageRemove");

			Measure.Method(() => Unit.TryAddModifier(modifierId, Unit, Unit))
				.WarmupCount(10)
				.MeasurementCount(50)
				.IterationsPerMeasurement(Iterations)
				.GC()
				.Run()
				;
		}
	}
}