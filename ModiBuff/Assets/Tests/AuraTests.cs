using ModiBuff.Core;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class AuraTests : BaseModifierTests
	{
		//[Test]//TODO
		public void AuraInterval()
		{
			var recipe = Recipes.GetRecipe("InitAddDamageBuff_Interval");
			Unit.AddCloseTargets(Ally);
			Unit.AddAuraModifier(recipe);

			Unit.Update(1f);

			Assert.AreEqual(UnitDamage + 5, Unit.Damage);
			Assert.AreEqual(AllyDamage + 5, Ally.Damage);
		}

		//[Test]//TODO
		public void Aura_AddDamage_Timeout()
		{
			var recipe = Recipes.GetRecipe("InitAddDamageBuff_Interval");
			Unit.AddCloseTargets(Ally);
			Unit.AddAuraModifier(recipe);

			Unit.Update(1f);

			Assert.AreEqual(UnitDamage + 5, Unit.Damage);
			Assert.AreEqual(AllyDamage + 5, Ally.Damage);

			Ally.Update(1.1f);

			Assert.AreEqual(AllyDamage, Ally.Damage);
		}

		//TODO Ally Same AddedDamage Refresh
		//TODO Ally Same AddedDamage Aura Timeout then Add again
	}
}