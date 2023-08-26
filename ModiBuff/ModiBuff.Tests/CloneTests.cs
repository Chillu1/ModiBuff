using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class CloneTests : ModifierTests
	{
		[Test]
		public void StackComponentClone()
		{
			var stackComponent = new StackComponent(WhenStackEffect.EveryXStacks, 2, 10, 2,
				new IStackEffect[] { new DamageEffect(5, StackEffectType.Effect | StackEffectType.Add) }, null);
			stackComponent.SetupTarget(new SingleTargetComponent(Unit, Unit));
			var clone = stackComponent.ShallowClone();
			clone.SetupTarget(new SingleTargetComponent(Enemy, Enemy));

			stackComponent.Stack();
			stackComponent.Stack();
			Assert.AreEqual(UnitHealth - 5 - 2, Unit.Health);
			stackComponent.Stack();
			stackComponent.Stack();
			Assert.AreEqual(UnitHealth - 5 - 4 - 5 - 2, Unit.Health);

			clone.Stack();
			clone.Stack();
			Assert.AreEqual(EnemyHealth - 5 - 2, Enemy.Health);
		}
	}
}