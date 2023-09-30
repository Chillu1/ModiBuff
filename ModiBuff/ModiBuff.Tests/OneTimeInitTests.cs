using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class OneTimeInitTests : ModifierTests
	{
		[Test]
		public void OneTimeInitDamage()
		{
			AddRecipe("OneTimeInitDamage")
				.OneTimeInit()
				.Effect(new DamageEffect(5), EffectOn.Init);
			Setup();

			Unit.AddModifierSelf("OneTimeInitDamage");
			Unit.AddModifierSelf("OneTimeInitDamage");

			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}
	}
}