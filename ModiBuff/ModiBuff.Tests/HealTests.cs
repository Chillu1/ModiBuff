using ModiBuff.Core;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class HealTests : BaseModifierTests
	{
		[Test]
		public void SelfInit_Heal()
		{
			Unit.TakeDamage(5, Unit);
			Assert.AreEqual(AllyHealth - 5, Unit.Health);

			Unit.TryAddModifierSelf("InitHeal"); //Init

			Assert.AreEqual(UnitHealth, Unit.Health);
		}

		[Test]
		public void TargetInit_Heal()
		{
			Ally.TakeDamage(5, Ally);
			Assert.AreEqual(AllyHealth - 5, Ally.Health);

			Unit.TryAddModifierTarget("InitHeal", Ally); //Init

			Assert.AreEqual(AllyHealth, Ally.Health);
		}
	}
}