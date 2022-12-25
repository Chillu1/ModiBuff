using ModiBuff.Core;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class IntervalTests : BaseModifierTests
	{
		[Test]
		public void Init_DoT()
		{
			Unit.TryAddModifierSelf("InitDoT"); //Init

			Assert.AreEqual(UnitHealth - 10, Unit.Health);

			Unit.Update(1);
			Assert.AreEqual(UnitHealth - 10 * 2, Unit.Health);
		}
	}
}