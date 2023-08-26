using ModiBuff.Core;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class InitTests : ModifierTests
	{
		[Test]
		public void InitDamage()
		{
			Unit.AddModifierSelf("InitDamage");

			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void InitDamage_InitTwice_DamageTwice()
		{
			Unit.AddModifierSelf("InitDamage");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Unit.AddModifierSelf("InitDamage");
			Assert.AreEqual(UnitHealth - 10, Unit.Health);
		}

		[Test]
		public void OneTimeInitDamage_LingerDuration()
		{
			Unit.AddModifierSelf("OneTimeInitDamage_LingerDuration");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Unit.AddModifierSelf("OneTimeInitDamage_LingerDuration");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Unit.Update(1f);
			Unit.AddModifierSelf("OneTimeInitDamage_LingerDuration");
			Assert.AreEqual(UnitHealth - 10, Unit.Health);
		}
	}
}