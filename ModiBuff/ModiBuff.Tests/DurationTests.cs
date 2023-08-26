using ModiBuff.Core;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class DurationTests : ModifierTests
	{
		[Test]
		public void Duration_Damage()
		{
			Unit.AddModifierSelf("DurationDamage");

			Unit.Update(5);

			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void Duration_Remove()
		{
			Unit.AddModifierSelf("DurationRemove");

			Unit.Update(5);

			Assert.False(Unit.ContainsModifier("DurationRemove"));
		}

		[Test]
		public void Duration_Damage_Once()
		{
			Unit.AddModifierSelf("DurationDamage");

			Unit.Update(5);

			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Unit.Update(5);

			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}
	}
}