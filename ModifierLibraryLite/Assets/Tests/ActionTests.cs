using ModifierLibraryLite.Core;
using NUnit.Framework;

namespace ModifierLibraryLite.Tests
{
	public sealed class ActionTests : BaseModifierTests
	{
		[Test]
		public void AttackSelf_Action()
		{
			Unit.TryAddModifierSelf("InitAttackAction");

			Assert.AreEqual(UnitHealth - UnitDamage, Unit.Health);
		}

		[Test]
		public void AttackEnemy_Action()
		{
			Unit.TryAddModifier("InitAttackAction", Enemy);

			Assert.AreEqual(EnemyHealth - UnitDamage, Enemy.Health);
		}

		[Test]
		public void HealSelf_Action()
		{
			Unit.TakeDamage(UnitHeal + 5, Unit);

			Unit.TryAddModifierSelf("InitHealAction");

			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}
	}
}