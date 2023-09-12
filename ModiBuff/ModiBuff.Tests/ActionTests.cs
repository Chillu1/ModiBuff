using System;
using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class ActionTests : ModifierTests
	{
		[Test]
		public void AttackSelf_Action()
		{
			AddRecipes(add => add("InitAttackAction")
				.Effect(new AttackActionEffect(), EffectOn.Init));

			Unit.AddModifierSelf("InitAttackAction");

			Assert.AreEqual(UnitHealth - UnitDamage, Unit.Health);
		}

		[Test]
		public void AttackEnemy_Action()
		{
			AddRecipes(add => add("InitAttackAction")
				.Effect(new AttackActionEffect(), EffectOn.Init));

			Unit.AddModifierTarget("InitAttackAction", Enemy);

			Assert.AreEqual(EnemyHealth - UnitDamage, Enemy.Health);
		}

		[Test]
		public void HealSelf_Action()
		{
			AddRecipes(add => add("InitHealAction")
				.Effect(new HealActionEffect(), EffectOn.Init));

			Unit.TakeDamage(UnitHeal + 5, Unit);

			Unit.AddModifierSelf("InitHealAction");

			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void AttackSelfTarget_Action()
		{
			AddRecipes(add => add("InitAttackAction_Self")
				.Effect(new AttackActionEffect(), EffectOn.Init, Targeting.TargetTarget));

			Unit.AddModifierSelf("InitAttackAction_Self");

			Assert.AreEqual(UnitHealth - UnitDamage, Unit.Health);
		}
	}
}