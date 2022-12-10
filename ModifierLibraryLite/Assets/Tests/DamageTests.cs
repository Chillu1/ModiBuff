using NUnit.Framework;

namespace ModifierLibraryLite.Tests
{
	public sealed class DamageTests : BaseModifierTests
	{
		[Test]
		public void SelfInit_Damage()
		{
			var modifier = Recipes.Get("InitDamage");

			Unit.TryAddModifier(modifier, Unit); //Init

			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void TargetInit_Damage()
		{
			var modifier = Recipes.Get("InitDamage");

			Enemy.TryAddModifier(modifier, Unit); //Init

			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}
	}
}