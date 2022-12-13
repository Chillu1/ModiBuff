using NUnit.Framework;

namespace ModifierLibraryLite.Tests
{
	public sealed class StateTests : BaseModifierTests
	{
		[Test]
		public void TwoDurationModifiers_DifferentState()
		{
			Unit.TryAddModifierSelf("DurationDamage");
			Enemy.TryAddModifierSelf("DurationDamage");

			Unit.Update(5);
			Enemy.Update(2);

			Assert.AreEqual(UnitHealth - 5, Unit.Health);
			Assert.AreEqual(EnemyHealth, Enemy.Health);
		}

		[Test]
		public void TwoInitModifiers_DifferentState()
		{
			Unit.TryAddModifierSelf("InitDamage");
			Enemy.TryAddModifierSelf("InitDamage");

			Assert.AreEqual(UnitHealth - 5, Unit.Health);
			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
		}
	}
}