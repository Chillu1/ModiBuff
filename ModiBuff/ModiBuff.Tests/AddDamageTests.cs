using ModiBuff.Core;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class AddDamageTests : ModifierTests
	{
		[Test]
		public void Init_AddDamage()
		{
			Unit.TryAddModifierSelf("InitAddDamage");

			Assert.AreEqual(UnitDamage + 5, Unit.Damage);
		}
	}
}