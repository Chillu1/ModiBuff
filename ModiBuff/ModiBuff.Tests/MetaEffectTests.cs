using ModiBuff.Core;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class MetaEffectTests : ModifierTests
	{
		[Test]
		public void LifeSteal_DamageEffectInit()
		{
			var recipe = Recipes.GetRecipe("InitDamageLifeStealMeta");
			Unit.AddApplierModifier(recipe, ApplierType.Cast);

			Unit.TakeDamage(2.5f, Unit);

			//Unit.Attack(Enemy);
			Unit.TryCast(recipe.Id, Enemy);

			Assert.AreEqual(UnitHealth, Unit.Health);
			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
		}
	}
}