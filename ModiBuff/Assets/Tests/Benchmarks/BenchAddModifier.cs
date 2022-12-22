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
		public void BenchAddSimpleModifier()
		{
			int modifierId = ModifierIdManager.GetId("InitDamage");

			Measure.Method(
					() => Unit.TryAddModifier(modifierId, Unit, Unit))
				.BenchGC(Iterations);
		}

		[Test, Performance]
		public void BenchAddMediumModifier()
		{
			int modifierId = ModifierIdManager.GetId("InitDoTSeparateDamageRemove");

			Measure.Method(
					() => Unit.TryAddModifier(modifierId, Unit, Unit))
				.BenchGC(Iterations);
		}
	}
}