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

		[Test]
		public void Init_AddDamage_ToggleRevert()
		{
			AddRecipe("InitAddDamage_ToggleRevert")
				.Effect(new AddDamageEffect(5, EffectState.IsTogglable), EffectOn.Init)
				.Remove(1);
			Setup();

			Unit.AddModifierSelf("InitAddDamage_ToggleRevert");

			Assert.AreEqual(UnitDamage + 5, Unit.Damage);

			Unit.Update(1f);
			Assert.AreEqual(UnitDamage, Unit.Damage);
		}
	}
}