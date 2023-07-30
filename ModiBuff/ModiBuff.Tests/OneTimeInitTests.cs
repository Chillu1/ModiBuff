using ModiBuff.Core;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class OneTimeInitTests : BaseModifierTests
	{
		[Test]
		public void OneTimeInitDamage()
		{
			Unit.TryAddModifierSelf("OneTimeInitDamage");
			Unit.TryAddModifierSelf("OneTimeInitDamage");

			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}
	}
}