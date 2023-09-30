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
			AddRecipe("InitAddDamage")
				.Effect(new AddDamageEffect(5), EffectOn.Init);
			Setup();

			Unit.AddModifierSelf("InitAddDamage");

			Assert.AreEqual(UnitDamage + 5, Unit.Damage);
		}
	}
}