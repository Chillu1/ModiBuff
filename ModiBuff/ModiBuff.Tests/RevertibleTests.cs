using ModiBuff.Core;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class RevertibleTests : BaseModifierTests
	{
		[Test]
		public void Init_AddDamage_Remove_RevertDamage()
		{
			Unit.TryAddModifierSelf("InitAddDamageRevertible");
			Assert.AreEqual(UnitDamage + 5, Unit.Damage);

			Unit.Update(5);

			Assert.AreEqual(UnitDamage, Unit.Damage);
		}
	}
}