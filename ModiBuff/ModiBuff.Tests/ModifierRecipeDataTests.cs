using System.Collections.Generic;
using System.Linq;
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
			AddRecipe("AddDamageApplier" + enemyType)
				.Effect(new AddDamageEffect(5), EffectOn.Init)
				.Data(new AddModifierCommonData<EnemyUnitType>(ModifierAddType.Applier, enemyType));
			AddEnemySelfBuff("AddDamage", EnemyUnitType.Slime)
				.Effect(new AddDamageEffect(5), EffectOn.Init);
			AddRecipe("AddDamageAdvanced" + enemyType)
				.Effect(new AddDamageEffect(5), EffectOn.Init)
				.Data(new AddModifierCommonData<GoblinModifierActionType, EnemyUnitType>(
					GoblinModifierActionType.OnSurrender, EnemyUnitType.Goblin));
			Setup();

			Unit.AddModifierSelf("AddDamage" + enemyType);

			Assert.AreEqual(UnitDamage + 5, Unit.Damage);

			var enemySelfModifiers = new List<int>();
			var addModifierCommonData = ModifierRecipes.GetModifierData<AddModifierCommonData<EnemyUnitType>>();
			Assert.AreEqual(addModifierCommonData.Length, 3);
			Assert.AreEqual(addModifierCommonData.Count(d => d.Data.UnitType == enemyType), 2);
			foreach ((int id, var data) in addModifierCommonData)
				if (data.UnitType == enemyType && data.ModifierType == ModifierAddType.Self)
					enemySelfModifiers.Add(id);

			Assert.AreEqual(enemySelfModifiers.Count, 1);
			Assert.AreEqual(enemySelfModifiers[0], IdManager.GetId("AddDamage" + enemyType));

			ModifierRecipe AddEnemySelfBuff(string name, EnemyUnitType enemyUnitType) =>
				AddRecipe(name + enemyUnitType)
					.Data(new AddModifierCommonData<EnemyUnitType>(ModifierAddType.Self, enemyUnitType));
		}

		[Test]
		public void SpecialUnitEvent()
		{
			const EnemyUnitType enemyType = EnemyUnitType.Goblin;
			AddGoblinModifier("RemoveDamage", GoblinModifierActionType.OnSurrender)
				.Effect(new AddDamageEffect(-5), EffectOn.Init);
			Setup();

			Unit.AddModifierSelf("RemoveDamage" + enemyType);

			Assert.AreEqual(UnitDamage - 5, Unit.Damage);

			var goblinSurrenderModifiers = new List<int>();
			foreach ((int id, var data) in ModifierRecipes
				         .GetModifierData<AddModifierCommonData<GoblinModifierActionType, EnemyUnitType>>())
				if (data.UnitType == enemyType && data.ModifierType == GoblinModifierActionType.OnSurrender)
					goblinSurrenderModifiers.Add(id);

			Assert.AreEqual(goblinSurrenderModifiers.Count, 1);
			Assert.AreEqual(goblinSurrenderModifiers[0], IdManager.GetId("RemoveDamage" + enemyType));

			ModifierRecipe AddGoblinModifier(string name, GoblinModifierActionType modifierActionType) =>
				AddRecipe(name + EnemyUnitType.Goblin)
					.Data(new AddModifierCommonData<GoblinModifierActionType, EnemyUnitType>(modifierActionType,
						EnemyUnitType.Goblin));
		}

		[Test]
		public void LegalActionUnitTypeGenerator()
		{
			const EnemyUnitType enemyType = EnemyUnitType.Goblin;
			AddGenerator("AddDamage" + enemyType, (id, genId, name, tag) =>
				{
					var addDamageEffect = new AddDamageEffect(5);
					var initComponent = new InitComponent(new IEffect[] { addDamageEffect }, null);

					return new Modifier(id, genId, name, initComponent, null, null, null,
						new SingleTargetComponent(), null, new EffectStateInfo(addDamageEffect), null);
				}, Core.Units.TagType.Default,
				customModifierData: new AddModifierCommonData<EnemyUnitType>(ModifierAddType.Self, enemyType));
			Setup();

			Unit.AddModifierSelf("AddDamage" + enemyType);

			Assert.AreEqual(UnitDamage + 5, Unit.Damage);

			var enemySelfModifiers = new List<int>();
			foreach ((int id, var data) in ModifierRecipes.GetModifierData<AddModifierCommonData<EnemyUnitType>>())
				if (data.UnitType == enemyType && data.ModifierType == ModifierAddType.Self)
					enemySelfModifiers.Add(id);

			Assert.AreEqual(enemySelfModifiers.Count, 1);
			Assert.AreEqual(enemySelfModifiers[0], IdManager.GetId("AddDamage" + enemyType));
		}
	}
}