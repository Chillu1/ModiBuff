using System.Collections.Generic;
using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class ModifierRecipeDataTests : ModifierTests
	{
		[Test]
		public void LegalActionUnitType()
		{
			const EnemyUnitType enemyType = EnemyUnitType.Goblin;
			AddEnemySelfBuff("AddDamage", enemyType)
				.Effect(new AddDamageEffect(5), EffectOn.Init);
			Setup();

			Unit.AddModifierSelf("AddDamage" + enemyType);

			Assert.AreEqual(UnitDamage + 5, Unit.Damage);

			var enemySelfModifiers = new List<int>();
			foreach ((int id, var data) in ModifierRecipes.GetModifierData<AddModifierCommonData<EnemyUnitType>>())
				if (data.UnitType == enemyType && data.ModifierType == ModifierAddType.Self)
					enemySelfModifiers.Add(id);

			Assert.AreEqual(enemySelfModifiers.Count, 1);
			Assert.AreEqual(enemySelfModifiers[0], IdManager.GetId("AddDamage" + enemyType));

			ModifierRecipe AddEnemySelfBuff(string name, EnemyUnitType enemyUnitType) =>
				AddRecipe(name + enemyUnitType)
					.Data(new AddModifierCommonData<EnemyUnitType>(ModifierAddType.Self, enemyUnitType));
		}
	}
}