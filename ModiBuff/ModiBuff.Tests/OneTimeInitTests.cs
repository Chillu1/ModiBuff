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
			AddRecipes(add => add("OneTimeInitDamage")
				.OneTimeInit()
				.Effect(new DamageEffect(5), EffectOn.Init));

			Unit.AddModifierSelf("OneTimeInitDamage");
			Unit.AddModifierSelf("OneTimeInitDamage");

			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}
	}
}