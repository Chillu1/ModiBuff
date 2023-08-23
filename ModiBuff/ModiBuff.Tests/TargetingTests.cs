using ModiBuff.Core;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class TargetingTests : ModifierTests
	{
		[Test]
		public void InitSelfHeal_DamageTarget()
		{
			Unit.TakeDamage(5, Unit);
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
			Unit.AddModifierTarget("InitSelfHeal_DamageTarget", Enemy);

			Assert.AreEqual(UnitHealth, Unit.Health);
			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
		}
	}
}