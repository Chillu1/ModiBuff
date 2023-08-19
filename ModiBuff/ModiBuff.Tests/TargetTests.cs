using ModiBuff.Core;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class TargetTests : BaseModifierTests
	{
		[Test]
		public void MultiTarget_AddDamage_Revertible()
		{
			var recipe = Recipes.GetRecipe("InitAddDamageRevertible");
			Unit.AddApplierModifier(recipe, ApplierType.Cast);

			Unit.TryCast(recipe.Id, Enemy);
			Unit.TryCast(recipe.Id, Ally);

			Assert.AreEqual(EnemyDamage + 5, Enemy.Damage);
			Assert.AreEqual(AllyDamage + 5, Ally.Damage);

			Enemy.Update(5);
			Assert.AreEqual(EnemyDamage, Enemy.Damage);

			Ally.Update(5);
			Assert.AreEqual(AllyDamage, Ally.Damage);
		}
	}
}