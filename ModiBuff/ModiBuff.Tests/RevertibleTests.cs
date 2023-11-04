using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class RevertibleTests : ModifierTests
	{
		[Test]
		public void Init_AddDamage_Remove_RevertDamage()
		{
			AddRecipe("InitAddDamageRevertible")
				.Effect(new AddDamageEffect(5, EffectState.IsRevertible), EffectOn.Init)
				.Remove(5);
			Setup();

			Unit.AddModifierSelf("InitAddDamageRevertible");
			Assert.AreEqual(UnitDamage + 5, Unit.Damage);

			Unit.Update(5);

			Assert.AreEqual(UnitDamage, Unit.Damage);
		}
	}
}