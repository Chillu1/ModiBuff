using NUnit.Framework;

namespace ModifierLibraryLite.Tests
{
	public sealed class HealTests : BaseModifierTests
	{
		[Test]
		public void SelfInit_Heal()
		{
			var modifier = Recipes.Get("InitHeal");
			Unit.TakeDamage(5, Unit);
			Assert.AreEqual(AllyHealth - 5, Unit.Health);

			Unit.TryAddModifier(modifier, Unit); //Init

			Assert.AreEqual(UnitHealth, Unit.Health);
		}

		[Test]
		public void TargetInit_Heal()
		{
			var modifier = Recipes.Get("InitHeal");
			Ally.TakeDamage(5, Ally);
			Assert.AreEqual(AllyHealth - 5, Ally.Health);

			Unit.TryAddModifier(modifier, Ally); //Init

			Assert.AreEqual(AllyHealth, Ally.Health);
		}
	}
}