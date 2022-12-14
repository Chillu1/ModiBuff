using NUnit.Framework;

namespace ModifierLibraryLite.Tests
{
	public sealed class CooldownTests : BaseModifierTests
	{
		[Test]
		public void InitDamage_Cooldown()
		{
			Unit.AddApplierModifiers(Recipes.GetRecipe("InitDamage_Cooldown"));

			Unit.Attack(Enemy);

			Assert.AreEqual(EnemyHealth - UnitDamage - 5, Enemy.Health);

			// 1 second cooldown
			Unit.Attack(Enemy);
			Assert.AreEqual(EnemyHealth - UnitDamage * 2 - 5, Enemy.Health);

			Unit.Update(1); //Cooldown gone
			Unit.Attack(Enemy);
			Assert.AreEqual(EnemyHealth - UnitDamage * 3 - 5 * 2, Enemy.Health);
		}
	}
}