using ModiBuff.Core;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class StatusResistanceTests : BaseModifierTests
	{
		[Test]
		public void Dot_NoResistance()
		{
			Unit.TryAddModifierSelf("DoTRemove");

			for (int i = 0; i < 6; i++)
				Unit.Update(1f);

			Assert.AreEqual(UnitHealth - 5 * 5, Unit.Health);
			Assert.False(Unit.ContainsModifier("DoTRemove"));
		}

		[TestCase(0.5f)]
		[TestCase(0.25f)]
		[TestCase(0.1f)]
		public void Dot_XResistance(float resistance)
		{
			Unit.TryAddModifierSelf("DoTRemove");
			Unit.ChangeStatusResistance(resistance);

			for (int i = 0; i < 6; i++)
				Unit.Update(resistance);

			Assert.AreEqual(UnitHealth - 5 * 5, Unit.Health);
			Assert.False(Unit.ContainsModifier("DoTRemove"));
		}
	}
}