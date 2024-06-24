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
			AddRecipe("StackDamage")
				.Effect(new DamageEffect(5, StackEffectType.Effect), EffectOn.Stack)
				.Stack(WhenStackEffect.Always);
			Setup();

			Unit.AddModifierSelf("StackDamage");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Unit.AddModifierSelf("StackDamage");
			Assert.AreEqual(UnitHealth - 10, Unit.Health);
		}

		[Test]
		public void Stack_Heal()
		{
			AddRecipe("StackHeal")
				.Effect(new HealEffect(5, HealEffect.EffectState.None, StackEffectType.Effect), EffectOn.Stack)
				.Stack(WhenStackEffect.Always);
			Setup();

			Unit.TakeDamage(UnitHealth - 5, Unit);

			Unit.AddModifierSelf("StackHeal");
			Assert.AreEqual(10, Unit.Health);

			Unit.AddModifierSelf("StackHeal");
			Assert.AreEqual(15, Unit.Health);
		}

		[Test]
		public void DamageOnMaxStacks()
		{
			AddRecipe("DamageOnMaxStacks")
				.Effect(new DamageEffect(5), EffectOn.Stack)
				.Stack(WhenStackEffect.OnMaxStacks, maxStacks: 2);
			Setup();

			Unit.AddModifierSelf("DamageOnMaxStacks");
			Assert.AreEqual(UnitHealth, Unit.Health);
			Unit.AddModifierSelf("DamageOnMaxStacks");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void DamageEveryTwoStacks()
		{
			AddRecipe("DamageEveryTwoStacks")
				.Effect(new DamageEffect(5), EffectOn.Stack)
				.Stack(WhenStackEffect.EveryXStacks, everyXStacks: 2);
			Setup();

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
			AddRecipe("StackBasedDamage")
				.Effect(new DamageEffect(5, StackEffectType.Effect | StackEffectType.Add, 2), EffectOn.Stack)
				.Stack(WhenStackEffect.Always);
			Setup();

			Unit.AddModifierSelf("StackBasedDamage");
			Assert.AreEqual(UnitHealth - 5 - 2, Unit.Health); //1 stack = +2 damage == 2
			Unit.AddModifierSelf("StackBasedDamage");
			Assert.AreEqual(UnitHealth - 10 - 6, Unit.Health); //2 stacks = +4 damage == 6
		}

		[Test]
		public void StackAddDamageRevertible()
		{
			AddRecipe("StackAddDamageRevertible")
				.Effect(
					new AddDamageEffect(5, EffectState.IsRevertible, StackEffectType.Effect | StackEffectType.Add, 2),
					EffectOn.Stack)
				.Stack(WhenStackEffect.Always)
				.Remove(5);
			Setup();

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
			AddRecipe("StunEveryTwoStacks")
				.Effect(new StatusEffectEffect(StatusEffectType.Stun, 2, false, StackEffectType.Effect), EffectOn.Stack)
				.Stack(WhenStackEffect.EveryXStacks, everyXStacks: 2);
			Setup();

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
			AddRecipe("DamageOnMaxStacks")
				.Effect(new DamageEffect(5), EffectOn.Stack)
				.Stack(WhenStackEffect.OnMaxStacks, maxStacks: 2);
			Setup();

			Unit.AddModifierSelf("DamageOnMaxStacks");
			Assert.AreEqual(UnitHealth, Unit.Health);
			Unit.AddModifierSelf("DamageOnMaxStacks");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
			Unit.AddModifierSelf("DamageOnMaxStacks");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
			Unit.AddModifierSelf("DamageOnMaxStacks");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void IndependentStackTimerRevert()
		{
			AddRecipe("AddDamageStackTimer")
				.Effect(new AddDamageEffect(5, EffectState.IsRevertible), EffectOn.Stack)
				.Stack(WhenStackEffect.Always, independentStackTime: 5);
			Setup();

			Unit.AddModifierSelf("AddDamageStackTimer");
			Assert.AreEqual(UnitDamage + 5, Unit.Damage);
			Unit.Update(5); //Stack timer expired, remove that stack
			Assert.AreEqual(UnitDamage, Unit.Damage);

			Unit.AddModifierSelf("AddDamageStackTimer");
			Unit.Update(1);
			Unit.AddModifierSelf("AddDamageStackTimer");
			Assert.AreEqual(UnitDamage + 5 + 5, Unit.Damage);
			Unit.Update(4);
			Assert.AreEqual(UnitDamage + 5, Unit.Damage);
			Unit.Update(1);
			Assert.AreEqual(UnitDamage, Unit.Damage);
		}

		[Test]
		public void IndependentStackTimerAddValueEffectRevert()
		{
			AddRecipe("AddDamageStackTimer")
				.Effect(
					new AddDamageEffect(5, EffectState.IsRevertible, StackEffectType.Effect | StackEffectType.Add, 2),
					EffectOn.Stack)
				.Stack(WhenStackEffect.Always, independentStackTime: 5);
			Setup();

			Unit.AddModifierSelf("AddDamageStackTimer");
			Assert.AreEqual(UnitDamage + 5 + 2, Unit.Damage);
			Unit.Update(5);
			Assert.AreEqual(UnitDamage, Unit.Damage);

			Unit.AddModifierSelf("AddDamageStackTimer");
			Unit.Update(1);
			Unit.AddModifierSelf("AddDamageStackTimer");
			Assert.AreEqual(UnitDamage + 5 + 2 + 5 + 2 + 2, Unit.Damage);
			Unit.Update(4);
			Assert.AreEqual(UnitDamage + 5 + 2, Unit.Damage);
			Unit.Update(1);
			Assert.AreEqual(UnitDamage, Unit.Damage);
		}

		[Test]
		public void SingleStackTimerAddDamageAddEffectRevert()
		{
			AddRecipe("AddDamageSingleStackTimer")
				.Effect(
					new AddDamageEffect(5, EffectState.IsRevertible, StackEffectType.Effect | StackEffectType.Add, 2),
					EffectOn.Stack)
				.Stack(WhenStackEffect.Always, singleStackTime: 5);
			Setup();

			Unit.AddModifierSelf("AddDamageSingleStackTimer");
			Unit.Update(4);
			Unit.AddModifierSelf("AddDamageSingleStackTimer");
			Assert.AreEqual(UnitDamage + 5 + 2 + 5 + 4, Unit.Damage);

			Unit.Update(4);
			Unit.AddModifierSelf("AddDamageSingleStackTimer");
			Assert.AreEqual(UnitDamage + 5 + 2 + 5 + 4 + 5 + 6, Unit.Damage);

			Unit.Update(5);
			Assert.AreEqual(UnitDamage, Unit.Damage);
			Unit.AddModifierSelf("AddDamageSingleStackTimer");
			Assert.AreEqual(UnitDamage + 5 + 2, Unit.Damage);
		}

		[Test]
		public void IndependentAndLongSingleStackTimerAddValueEffectRevert()
		{
			AddRecipe("AddDamageStackTimer")
				.Effect(
					new AddDamageEffect(5, EffectState.IsRevertible, StackEffectType.Effect | StackEffectType.Add, 2),
					EffectOn.Stack)
				.Stack(WhenStackEffect.Always, independentStackTime: 2, singleStackTime: 5);
			Setup();

			Unit.AddModifierSelf("AddDamageStackTimer");
			Assert.AreEqual(UnitDamage + 5 + 2, Unit.Damage);

			Unit.Update(1);
			Unit.AddModifierSelf("AddDamageStackTimer");
			Assert.AreEqual(UnitDamage + 5 + 2 + 5 + 4, Unit.Damage);

			Unit.Update(1);
			Assert.AreEqual(UnitDamage + 5 + 2, Unit.Damage);

			Unit.Update(4);
			Unit.AddModifierSelf("AddDamageStackTimer");
			Assert.AreEqual(UnitDamage + 5 + 2, Unit.Damage);

			Unit.Update(2);
			Unit.Update(3);
			Assert.AreEqual(UnitDamage, Unit.Damage);
		}

		[Test]
		public void IndependentAndShortSingleStackTimerAddValueEffectRevert()
		{
			AddRecipe("AddDamageStackTimer")
				.Effect(
					new AddDamageEffect(5, EffectState.IsRevertible, StackEffectType.Effect | StackEffectType.Add, 2),
					EffectOn.Stack)
				.Stack(WhenStackEffect.Always, independentStackTime: 4, singleStackTime: 3);
			Setup();

			Unit.AddModifierSelf("AddDamageStackTimer");
			Assert.AreEqual(UnitDamage + 5 + 2, Unit.Damage);

			Unit.Update(2);
			Unit.AddModifierSelf("AddDamageStackTimer");
			Assert.AreEqual(UnitDamage + 5 + 2 + 5 + 4, Unit.Damage);

			Unit.Update(2);
			Assert.AreEqual(UnitDamage + 5 + 2, Unit.Damage);

			Unit.AddModifierSelf("AddDamageStackTimer");

			Unit.Update(1);
			Assert.AreEqual(UnitDamage + 5 + 2 + 5 + 4, Unit.Damage);

			Unit.Update(1);
			Assert.AreEqual(UnitDamage + 5 + 2, Unit.Damage);

			Unit.Update(1);
			Assert.AreEqual(UnitDamage, Unit.Damage);
		}

		[Test]
		public void RemoveOnMaxStacks()
		{
			AddRecipe("RemoveStack")
				.Stack(WhenStackEffect.OnMaxStacks, 2)
				.Remove(RemoveEffectOn.Stack);
			Setup();

			Unit.AddModifierSelf("RemoveStack");
			Assert.True(Unit.ContainsModifier("RemoveStack"));
			Unit.AddModifierSelf("RemoveStack");
			Unit.Update(0);
			Assert.False(Unit.ContainsModifier("RemoveStack"));
		}

		[Test]
		public void RemoveOnMaxStacksEffect()
		{
			AddRecipe("RemoveStack")
				.Effect(new RemoveEffect(), EffectOn.Stack)
				.Stack(WhenStackEffect.OnMaxStacks, 2);
			Setup();

			Unit.AddModifierSelf("RemoveStack");
			Assert.True(Unit.ContainsModifier("RemoveStack"));
			Unit.AddModifierSelf("RemoveStack");
			Unit.Update(0);
			Assert.False(Unit.ContainsModifier("RemoveStack"));
		}
	}
}