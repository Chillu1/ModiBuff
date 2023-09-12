using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class AddDamageTests : ModifierTests
	{
		[Test]
		public void Init_AddDamage()
		{
			AddRecipes(add => add("InitAddDamage")
				.Effect(new AddDamageEffect(5), EffectOn.Init));

			Unit.AddModifierSelf("InitAddDamage");

			Assert.AreEqual(UnitDamage + 5, Unit.Damage);
		}
	}
}