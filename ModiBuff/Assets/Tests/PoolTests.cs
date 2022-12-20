using ModiBuff.Core;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class PoolTests : BaseModifierTests
	{
		//TODO Pool time reset
		[Test]
		public void TimeStateReset()
		{
			Pool.Clear();
			Pool.Allocate(ModifierIdManager.GetId("DurationRemove"), 1);

			Unit.TryAddModifierSelf("DurationRemove");

			Unit.Update(1);
			Assert.True(Unit.ContainsModifier("DurationRemove"));

			Unit.Update(4);
			Assert.False(Unit.ContainsModifier("DurationRemove")); //Return to pool

			Enemy.TryAddModifierSelf("DurationRemove"); //State should be reset
			Assert.True(Enemy.ContainsModifier("DurationRemove"));
			Enemy.Update(1);
			Assert.True(Enemy.ContainsModifier("DurationRemove"));

			Enemy.Update(4);
			Assert.False(Enemy.ContainsModifier("DurationRemove"));
		}

		//[Test] //State not reset because of remove component
		public void StackStateReset()
		{
			Pool.Clear();
			Pool.Allocate(ModifierIdManager.GetId("StackBasedDamage"), 1);

			Unit.TryAddModifierSelf("StackBasedDamage");
			Assert.AreEqual(UnitHealth - 5 - 2, Unit.Health); //1 stack = +2 damage == 2

			Unit.TryAddModifierSelf("StackBasedDamage");
			Assert.AreEqual(UnitHealth - 10 - 6, Unit.Health); //2 stacks = +4 damage == 4

			Unit.RemoveModifier(ModifierIdManager.GetId("StackBasedDamage")); //Return to pool

			Enemy.TryAddModifierSelf("StackBasedDamage"); //State should be reset
			Assert.AreEqual(EnemyHealth - 5 - 2, Enemy.Health);
		}

		//TODO Pool AddedDamage revertible state reset
	}
}