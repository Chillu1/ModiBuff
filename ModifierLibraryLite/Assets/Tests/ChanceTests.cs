using NUnit.Framework;

namespace ModifierLibraryLite.Tests
{
	public sealed class ChanceTests : BaseModifierTests
	{
		[Test]
		public void Random_InitDamage()
		{
			Unit.AddApplierModifiers(Recipes.GetRecipe("ChanceInitDamage"));

			for (int i = 0; i < 50; i++)
				Unit.Attack(Enemy);

			float totalDamage = EnemyHealth - Enemy.Health;
			float averageDamage = totalDamage / 50;

			Assert.That(averageDamage, Is.InRange(10f, 15f));
		}
	}
}