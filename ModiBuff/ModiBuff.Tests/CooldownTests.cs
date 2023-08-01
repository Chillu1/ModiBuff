using ModiBuff.Core;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class CooldownTests : BaseModifierTests
	{
		[Test]
		public void InitDamage_Cooldown()
		{
			Unit.AddApplierModifier(Recipes.GetRecipe("InitDamage_Cooldown"));

			Unit.Attack(Enemy);

			Assert.AreEqual(EnemyHealth - UnitDamage - 5, Enemy.Health);

			// 1 second cooldown
			Unit.Attack(Enemy);
			Assert.AreEqual(EnemyHealth - UnitDamage * 2 - 5, Enemy.Health);

			Unit.Update(1); //Cooldown gone
			Unit.Attack(Enemy);
			Assert.AreEqual(EnemyHealth - UnitDamage * 3 - 5 * 2, Enemy.Health);
		}

		[Test]
		public void InitDamage_Cooldown_Effect()
		{
			Unit.TryAddModifierSelf("InitDamage_Cooldown_Effect"); // 1 second cooldown
			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Unit.TryAddModifierSelf("InitDamage_Cooldown_Effect"); // On Cooldown
			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Unit.Update(1); //Cooldown gone
			Unit.TryAddModifierSelf("InitDamage_Cooldown_Effect");
			Assert.AreEqual(UnitHealth - 5 * 2, Unit.Health);
		}
	}
}