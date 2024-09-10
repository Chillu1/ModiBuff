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
			AddRecipe("AddDamage")
				.Data(new AddModifierCommonData<EnemyUnitType>(ModifierAddType.Self, EnemyUnitType.Goblin))
				.Effect(new DamageEffect(5), EffectOn.Init);
			AddEnemySelfBuff("AddDamage", EnemyUnitType.Goblin)
				.Effect(new DamageEffect(5), EffectOn.Init);
			Setup();

			Unit.AddModifierSelf("AddDamage");

			Assert.AreEqual(UnitHealth - UnitDamage, Unit.Health);

			ModifierRecipe AddEnemySelfBuff(string name, EnemyUnitType enemyUnitType, string displayName = "",
				string description = "") =>
				Add(name + enemyUnitType, displayName, description)
					.Data(new AddModifierCommonData<EnemyUnitType>(ModifierAddType.Self, enemyUnitType));
		}
	}
}