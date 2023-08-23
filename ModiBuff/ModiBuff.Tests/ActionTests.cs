using ModiBuff.Core;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class ActionTests : ModifierTests
	{
		[Test]
		public void AttackSelf_Action()
		{
			Unit.AddModifierSelf("InitAttackAction");

			Assert.AreEqual(UnitHealth - UnitDamage, Unit.Health);
		}

		[Test]
		public void AttackEnemy_Action()
		{
			Unit.AddModifierTarget("InitAttackAction", Enemy);

			Assert.AreEqual(EnemyHealth - UnitDamage, Enemy.Health);
		}

		[Test]
		public void HealSelf_Action()
		{
			Unit.TakeDamage(UnitHeal + 5, Unit);

			Unit.AddModifierSelf("InitHealAction");

			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void AttackSelfTarget_Action()
		{
			Unit.AddModifierSelf("InitAttackAction_Self");

			Assert.AreEqual(UnitHealth - UnitDamage, Unit.Health);
		}
	}
}