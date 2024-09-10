using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class GradualEffectTests : ModifierTests
	{
		[Test]
		public void GradualAddDamage()
		{
			//How to design gradual effects?
			//* Keep a timer in Unit
			//* Use OnUpdate (or unit ticking events, ex every 0.2s/1s) events (messy?)
			//* Use special effect with interval? Reverting the effect partially with each interval tick?
			//* Something new like effect effects? Effects that change modifier state
			AddRecipe("AddDamageGradual")
				.Effect(new AddDamageEffect(5), EffectOn.Init);
			Setup();

			Unit.AddModifierSelf("AddDamageGradual");

			Assert.AreEqual(UnitDamage + 5, Unit.Damage);
			Unit.Update(1);
			Assert.AreEqual(UnitDamage + 4, Unit.Damage);
			Unit.Update(3);
			Assert.AreEqual(UnitDamage + 1, Unit.Damage);
			Unit.Update(1);
			Assert.AreEqual(UnitDamage, Unit.Damage);
		}
	}
}