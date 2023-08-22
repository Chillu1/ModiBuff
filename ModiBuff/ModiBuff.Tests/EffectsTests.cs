using ModiBuff.Core;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class EffectsTests : ModifierTests
	{
		[Test]
		public void TwoEffects()
		{
			Unit.TryAddModifierSelf("InitDoTSeparateDamageRemove");

			Assert.AreEqual(UnitHealth - 10, Unit.Health);

			Unit.Update(1);

			Assert.AreEqual(UnitHealth - 10 - 5, Unit.Health);
		}
	}
}