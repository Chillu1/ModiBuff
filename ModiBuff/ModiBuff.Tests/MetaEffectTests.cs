using ModiBuff.Core;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class MetaEffectTests : ModifierTests
	{
		[Test]
		public void DamageBasedOnHealth()
		{
			var recipe = Recipes.GetRecipe("InitDamageValueBasedOnStatMeta");
			Unit.AddApplierModifier(recipe, ApplierType.Cast);

			Unit.TryCast(recipe.Id, Enemy); //5 * 1

			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);

			Unit.TakeDamage(UnitHealth / 2f, Unit);

			Unit.TryCast(recipe.Id, Enemy); //5 * 0.5

			Assert.AreEqual(EnemyHealth - 7.5f, Enemy.Health);
		}
	}
}