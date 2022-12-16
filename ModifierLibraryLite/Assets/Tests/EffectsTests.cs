using ModifierLibraryLite.Core;
using NUnit.Framework;

namespace ModifierLibraryLite.Tests
{
	public sealed class EffectsTests : BaseModifierTests
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