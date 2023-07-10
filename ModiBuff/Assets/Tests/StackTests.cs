using ModiBuff.Core;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class StackTests : BaseModifierTests
	{
		[Test]
		public void Stack_Damage()
		{
			Unit.TryAddModifierSelf("StackDamage");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Unit.TryAddModifierSelf("StackDamage");
			Assert.AreEqual(UnitHealth - 10, Unit.Health);
		}

		[Test]
		public void Stack_Heal()
		{
			Unit.TakeDamage(UnitHealth - 5, Unit);

			Unit.TryAddModifierSelf("StackHeal");
			Assert.AreEqual(10, Unit.Health);

			Unit.TryAddModifierSelf("StackHeal");
			Assert.AreEqual(15, Unit.Health);
		}

		[Test]
		public void DamageOnMaxStacks()
		{
			Unit.TryAddModifierSelf("DamageOnMaxStacks");
			Assert.AreEqual(UnitHealth, Unit.Health);
			Unit.TryAddModifierSelf("DamageOnMaxStacks");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void DamageEveryTwoStacks()
		{
			Unit.TryAddModifierSelf("DamageEveryTwoStacks");
			Assert.AreEqual(UnitHealth, Unit.Health);
			Unit.TryAddModifierSelf("DamageEveryTwoStacks");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
			Unit.TryAddModifierSelf("DamageEveryTwoStacks");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
			Unit.TryAddModifierSelf("DamageEveryTwoStacks");
			Assert.AreEqual(UnitHealth - 10, Unit.Health);
		}

		[Test]
		public void Stack_DamageStackBased()
		{
			Unit.TryAddModifierSelf("StackBasedDamage");
			Assert.AreEqual(UnitHealth - 5 - 2, Unit.Health); //1 stack = +2 damage == 2
			Unit.TryAddModifierSelf("StackBasedDamage");
			Assert.AreEqual(UnitHealth - 10 - 6, Unit.Health); //2 stacks = +4 damage == 6
		}

		[Test]
		public void StackAddDamageRevertible()
		{
			Unit.TryAddModifierSelf("StackAddDamageRevertible"); //5 base, + 2 on stack
			Assert.AreEqual(UnitDamage + 5 + 2, Unit.Damage);

			Unit.TryAddModifierSelf("StackAddDamageRevertible"); //5 base, + 4 on stack
			Assert.AreEqual(UnitDamage + 10 + 6, Unit.Damage);

			Unit.Update(5); //Modifier removed
			Assert.AreEqual(UnitDamage, Unit.Damage);
		}

		[Test]
		public void StunEveryTwoStacks()
		{
			Unit.TryAddModifierSelf("StunEveryTwoStacks");

			Assert.False(Unit.HasStatusEffect(StatusEffectType.Stun));
			Unit.TryAddModifierSelf("StunEveryTwoStacks");
			Assert.True(Unit.HasStatusEffect(StatusEffectType.Stun));

			Unit.Update(2);

			Assert.False(Unit.HasStatusEffect(StatusEffectType.Stun));
			Unit.TryAddModifierSelf("StunEveryTwoStacks");
			Assert.False(Unit.HasStatusEffect(StatusEffectType.Stun));
			Unit.TryAddModifierSelf("StunEveryTwoStacks");
			Assert.True(Unit.HasStatusEffect(StatusEffectType.Stun));
		}

		[Test]
		public void DamageOnMaxStacks_Limit()
		{
			Unit.TryAddModifierSelf("DamageOnMaxStacks");
			Assert.AreEqual(UnitHealth, Unit.Health);
			Unit.TryAddModifierSelf("DamageOnMaxStacks");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
			Unit.TryAddModifierSelf("DamageOnMaxStacks");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
			Unit.TryAddModifierSelf("DamageOnMaxStacks");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}
	}
}