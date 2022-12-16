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
	}
}