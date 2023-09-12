using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class StackTests : ModifierTests
	{
		[Test]
		public void Stack_Damage()
		{
			AddRecipes(add => add("StackDamage")
				.Effect(new DamageEffect(5, StackEffectType.Effect), EffectOn.Stack)
				.Stack(WhenStackEffect.Always));

			Unit.AddModifierSelf("StackDamage");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Unit.AddModifierSelf("StackDamage");
			Assert.AreEqual(UnitHealth - 10, Unit.Health);
		}

		[Test]
		public void Stack_Heal()
		{
			AddRecipes(add => add("StackHeal")
				.Effect(new HealEffect(5, false, StackEffectType.Effect), EffectOn.Stack)
				.Stack(WhenStackEffect.Always));

			Unit.TakeDamage(UnitHealth - 5, Unit);

			Unit.AddModifierSelf("StackHeal");
			Assert.AreEqual(10, Unit.Health);

			Unit.AddModifierSelf("StackHeal");
			Assert.AreEqual(15, Unit.Health);
		}

		[Test]
		public void DamageOnMaxStacks()
		{
			AddRecipes(add => add("DamageOnMaxStacks")
				.Effect(new DamageEffect(5, StackEffectType.Effect), EffectOn.Stack)
				.Stack(WhenStackEffect.OnMaxStacks, value: -1, maxStacks: 2));

			Unit.AddModifierSelf("DamageOnMaxStacks");
			Assert.AreEqual(UnitHealth, Unit.Health);
			Unit.AddModifierSelf("DamageOnMaxStacks");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void DamageEveryTwoStacks()
		{
			AddRecipes(add => add("DamageEveryTwoStacks")
				.Effect(new DamageEffect(5, StackEffectType.Effect), EffectOn.Stack)
				.Stack(WhenStackEffect.EveryXStacks, value: -1, maxStacks: -1, everyXStacks: 2));

			Unit.AddModifierSelf("DamageEveryTwoStacks");
			Assert.AreEqual(UnitHealth, Unit.Health);
			Unit.AddModifierSelf("DamageEveryTwoStacks");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
			Unit.AddModifierSelf("DamageEveryTwoStacks");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
			Unit.AddModifierSelf("DamageEveryTwoStacks");
			Assert.AreEqual(UnitHealth - 10, Unit.Health);
		}

		[Test]
		public void Stack_DamageStackBased()
		{
			AddRecipes(add => add("StackBasedDamage")
				.Effect(new DamageEffect(5, StackEffectType.Effect | StackEffectType.Add), EffectOn.Stack)
				.Stack(WhenStackEffect.Always, value: 2));

			Unit.AddModifierSelf("StackBasedDamage");
			Assert.AreEqual(UnitHealth - 5 - 2, Unit.Health); //1 stack = +2 damage == 2
			Unit.AddModifierSelf("StackBasedDamage");
			Assert.AreEqual(UnitHealth - 10 - 6, Unit.Health); //2 stacks = +4 damage == 6
		}

		[Test]
		public void StackAddDamageRevertible()
		{
			AddRecipes(add => add("StackAddDamageRevertible")
				.Effect(new AddDamageEffect(5, true, StackEffectType.Effect | StackEffectType.Add), EffectOn.Stack)
				.Stack(WhenStackEffect.Always, value: 2)
				.Remove(5));

			Unit.AddModifierSelf("StackAddDamageRevertible"); //5 base, + 2 on stack
			Assert.AreEqual(UnitDamage + 5 + 2, Unit.Damage);

			Unit.AddModifierSelf("StackAddDamageRevertible"); //5 base, + 4 on stack
			Assert.AreEqual(UnitDamage + 10 + 6, Unit.Damage);

			Unit.Update(5); //Modifier removed
			Assert.AreEqual(UnitDamage, Unit.Damage);
		}

		[Test]
		public void StunEveryTwoStacks()
		{
			AddRecipes(add => add("StunEveryTwoStacks")
				.Effect(new StatusEffectEffect(StatusEffectType.Stun, 2, false, StackEffectType.Effect), EffectOn.Stack)
				.Stack(WhenStackEffect.EveryXStacks, everyXStacks: 2));

			Unit.AddModifierSelf("StunEveryTwoStacks");

			Assert.False(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Stun));
			Unit.AddModifierSelf("StunEveryTwoStacks");
			Assert.True(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Stun));

			Unit.Update(2);

			Assert.False(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Stun));
			Unit.AddModifierSelf("StunEveryTwoStacks");
			Assert.False(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Stun));
			Unit.AddModifierSelf("StunEveryTwoStacks");
			Assert.True(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Stun));
		}

		[Test]
		public void DamageOnMaxStacks_Limit()
		{
			AddRecipes(add => add("DamageOnMaxStacks")
				.Effect(new DamageEffect(5, StackEffectType.Effect), EffectOn.Stack)
				.Stack(WhenStackEffect.OnMaxStacks, value: -1, maxStacks: 2));

			Unit.AddModifierSelf("DamageOnMaxStacks");
			Assert.AreEqual(UnitHealth, Unit.Health);
			Unit.AddModifierSelf("DamageOnMaxStacks");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
			Unit.AddModifierSelf("DamageOnMaxStacks");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
			Unit.AddModifierSelf("DamageOnMaxStacks");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}
	}
}