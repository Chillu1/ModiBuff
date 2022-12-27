using ModiBuff.Core;
using NUnit.Framework;
using Unity.PerformanceTesting;

namespace ModiBuff.Tests
{
	/// <summary>
	///		Only tests for subsequent inits, not new modifier each time
	/// </summary>
	public sealed class BenchAddModifier : BaseModifierTests
	{
		private const int Iterations = 5000;

		[Test, Performance]
		public void BenchAddInitDamage()
		{
			int modifierId = ModifierIdManager.GetId("InitDamage");

			Measure.Method(
					() => Unit.TryAddModifier(modifierId, Unit))
				.Bench(Iterations);
		}

		[Test, Performance]
		public void BenchAddInitStackDamage()
		{
			int modifierId = ModifierIdManager.GetId("InitStackDamage");

			Measure.Method(
					() => Unit.TryAddModifier(modifierId, Unit))
				.Bench(Iterations);
		}
	}
}