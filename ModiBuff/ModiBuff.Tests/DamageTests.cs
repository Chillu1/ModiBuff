using ModiBuff.Core;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class DamageTests : BaseModifierTests
	{
		[Test]
		public void SelfInit_Damage()
		{
			Unit.TryAddModifierSelf("InitDamage"); //Init

			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void TargetInit_Damage()
		{
			Enemy.TryAddModifierTarget("InitDamage", Unit); //Init

			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}
	}
}