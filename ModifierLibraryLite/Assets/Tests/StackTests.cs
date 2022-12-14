using ModifierLibraryLite.Core;
using NUnit.Framework;

namespace ModifierLibraryLite.Tests
{
	public sealed class StackTests : BaseModifierTests
	{
		[Test]
		public void Stack_Damage()
		{
			Unit.TryAddModifierSelf("StackDamage");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Unit.TryAddModifierSelf("StackDamage");
			Assert.AreEqual(UnitHealth - 10, Unit.Health);
		}
	}
}