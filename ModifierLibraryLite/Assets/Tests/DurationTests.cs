using NUnit.Framework;

namespace ModifierLibraryLite.Tests
{
	public sealed class DurationTests : BaseModifierTests
	{
		[Test]
		public void Duration_Damage()
		{
			Unit.TryAddModifierSelf("DurationDamage");

			Unit.Update(5);

			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void Duration_Remove()
		{
			Unit.TryAddModifierSelf("DurationRemove");

			Unit.Update(5);

			Assert.False(Unit.ContainsModifier("DurationRemove"));
		}
	}
}