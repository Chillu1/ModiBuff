using ModiBuff.Core;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class TargetTests : BaseModifierTests
	{
		//[Test]
		public void MultiTarget_AddDamage_Revertible()
		{
			var recipe = Recipes.GetRecipe("InitAddDamageRevertible");
			Unit.AddApplierModifier(recipe, ApplierType.Cast);

			Unit.Cast(Enemy);
			Unit.Cast(Ally);

			Assert.AreEqual(EnemyDamage + 5, Enemy.Damage);
			Assert.AreEqual(AllyDamage + 5, Ally.Damage);

			Enemy.Update(5); //AddDamageEffect has state: _addedDamage, so we need to clone it, but if we do. RemoveEffect will remove the original effect without the state
			Assert.AreEqual(EnemyDamage, Enemy.Damage);

			Ally.Update(5);
			Assert.AreEqual(AllyDamage, Ally.Damage);
		}
	}
}