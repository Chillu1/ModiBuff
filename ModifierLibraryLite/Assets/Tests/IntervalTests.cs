using NUnit.Framework;

namespace ModifierLibraryLite.Tests
{
	public sealed class IntervalTests : BaseModifierTests
	{
		[Test]
		public void Init_DoT()
		{
			var modifier = Recipes.Get("InitDoT");
			Unit.TryAddModifier(modifier, Unit); //Init

			Assert.AreEqual(UnitHealth - 10, Unit.Health);

			Unit.Update(1);
			Assert.AreEqual(UnitHealth - 10 * 2, Unit.Health);
		}
	}
}