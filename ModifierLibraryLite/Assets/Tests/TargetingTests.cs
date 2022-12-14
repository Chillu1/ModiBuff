using ModifierLibraryLite.Core;
using NUnit.Framework;

namespace ModifierLibraryLite.Tests
{
	public sealed class TargetingTests : BaseModifierTests
	{
		[Test]
		public void InitSelfHeal_DamageTarget()
		{
			Unit.TakeDamage(5, Unit);
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
			Unit.TryAddModifier("InitSelfHeal_DamageTarget", Enemy);

			Assert.AreEqual(UnitHealth, Unit.Health);
			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
		}
	}
}