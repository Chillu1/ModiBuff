using ModifierLibraryLite.Core;
using NUnit.Framework;

namespace ModifierLibraryLite.Tests
{
	public sealed class AddDamageTests : BaseModifierTests
	{
		[Test]
		public void Init_AddDamage()
		{
			Unit.TryAddModifierSelf("InitAddDamage");

			Assert.AreEqual(UnitDamage + 5, Unit.Damage);
		}
	}
}