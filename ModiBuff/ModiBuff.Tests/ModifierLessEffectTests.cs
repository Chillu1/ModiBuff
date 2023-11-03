using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class ModifierLessEffectTests : ModifierTests
	{
		[Test]
		public void InitDamageModifierLess()
		{
			AddEffect("5Damage", new DamageEffect(5f));
			AddEffect("10Damage", new DamageEffectNoState(10f));
			Setup();

			Unit.ApplyEffectSelf("5Damage");
			Assert.AreEqual(UnitHealth - 5f, Unit.Health);

			Unit.ApplyEffectSelf("10Damage");
			Assert.AreEqual(UnitHealth - 5f - 10f, Unit.Health);
		}
	}
}