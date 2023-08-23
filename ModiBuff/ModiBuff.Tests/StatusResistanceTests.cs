using ModiBuff.Core;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class StatusResistanceTests : ModifierTests
	{
		[Test]
		public void Dot_NoResistance()
		{
			Unit.AddModifierSelf("DoTRemoveStatusResistance");

			for (int i = 0; i < 6; i++)
				Unit.Update(1f);

			Assert.AreEqual(UnitHealth - 5 * 5, Unit.Health);
			Assert.False(Unit.ContainsModifier("DoTRemoveStatusResistance"));
		}

		[TestCase(0.5f)]
		[TestCase(0.25f)]
		[TestCase(0.1f)]
		public void Dot_XResistance(float resistance)
		{
			Unit.AddModifierSelf("DoTRemoveStatusResistance");
			Unit.ChangeStatusResistance(resistance);

			for (int i = 0; i < 6; i++)
				Unit.Update(resistance);

			Assert.AreEqual(UnitHealth - 5 * 5, Unit.Health);
			Assert.False(Unit.ContainsModifier("DoTRemoveStatusResistance"));
		}

		[Test]
		public void Dot_StatusResistance_IntervalNotAffected()
		{
			Unit.AddModifierSelf("DoTRemove");
			Unit.ChangeStatusResistance(0.5f);

			for (int i = 0; i < 12; i++)
				Unit.Update(0.5f);

			//Activates twice, because 5 * 0.5 = 2.5, which makes it activates 2 times before being removed
			Assert.AreEqual(UnitHealth - 5 * 2, Unit.Health);
			Assert.False(Unit.ContainsModifier("DoTRemove"));
		}
	}
}