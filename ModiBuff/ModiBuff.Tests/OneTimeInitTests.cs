using ModiBuff.Core;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class OneTimeInitTests : ModifierTests
	{
		[Test]
		public void OneTimeInitDamage()
		{
			Unit.AddModifierSelf("OneTimeInitDamage");
			Unit.AddModifierSelf("OneTimeInitDamage");

			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}
	}
}