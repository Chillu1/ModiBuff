using ModiBuff.Core;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class DamageTests : ModifierTests
	{
		[Test]
		public void SelfInit_Damage()
		{
			Unit.AddModifierSelf("InitDamage"); //Init

			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void TargetInit_Damage()
		{
			Enemy.AddModifierTarget("InitDamage", Unit); //Init

			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}
	}
}