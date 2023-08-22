using ModiBuff.Core;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class InitTests : ModifierTests
	{
		[Test]
		public void InitDamage()
		{
			Unit.TryAddModifierSelf("InitDamage");

			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void InitDamage_InitTwice_DamageTwice()
		{
			Unit.TryAddModifierSelf("InitDamage");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Unit.TryAddModifierSelf("InitDamage");
			Assert.AreEqual(UnitHealth - 10, Unit.Health);
		}

		[Test]
		public void OneTimeInitDamage_LingerDuration()
		{
			Unit.TryAddModifierSelf("OneTimeInitDamage_LingerDuration");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Unit.TryAddModifierSelf("OneTimeInitDamage_LingerDuration");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Unit.Update(1f);
			Unit.TryAddModifierSelf("OneTimeInitDamage_LingerDuration");
			Assert.AreEqual(UnitHealth - 10, Unit.Health);
		}
	}
}