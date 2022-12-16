using ModifierLibraryLite.Core;
using NUnit.Framework;

namespace ModifierLibraryLite.Tests
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
	}
}