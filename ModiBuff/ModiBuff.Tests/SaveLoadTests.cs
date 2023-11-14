using ModiBuff.Core;
using ModiBuff.Core.Units;
using ModiBuff.Extensions.Serialization.Json;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class SaveLoadTests : ModifierTests
	{
		[Test]
		public void SaveLoadInitExtraDamage()
		{
			AddRecipe("InitStackExtraDamage")
				.Stack(WhenStackEffect.Always)
				.Effect(new DamageEffect(5, StackEffectType.Add, stackValue: 2), EffectOn.Init | EffectOn.Stack);
			Setup();

			var modifier = Pool.Rent(IdManager.GetId("InitStackExtraDamage"));
			modifier.UpdateSingleTargetSource(Unit, Unit);
			modifier.Stack();
			var saveState = modifier.SaveState();
			var loadedModifier = Pool.Rent(IdManager.GetId("InitStackExtraDamage"));
			loadedModifier.LoadState(saveState, Unit);

			loadedModifier.UpdateSingleTargetSource(Unit, Unit);
			loadedModifier.Init();
			Assert.AreEqual(UnitHealth - 5 - 2, Unit.Health);
		}

		[Test]
		public void SaveAddDamageExtraStateLoad()
		{
			AddRecipe("AddDamageExtraState")
				.Stack(WhenStackEffect.Always)
				.Effect(new AddDamageEffect(5, EffectState.IsRevertible, StackEffectType.Add, stackValue: 2),
					EffectOn.Init | EffectOn.Stack)
				.Remove(5);
			Setup();

			var modifier = Pool.Rent(IdManager.GetId("AddDamageExtraState"));
			modifier.UpdateSingleTargetSource(Unit, Unit);
			modifier.Stack();

			var saveState = modifier.SaveState();
			var loadedModifier = Pool.Rent(IdManager.GetId("AddDamageExtraState"));
			loadedModifier.LoadState(saveState, Unit);

			loadedModifier.UpdateSingleTargetSource(Unit, Unit);
			loadedModifier.Init();
			Assert.AreEqual(UnitDamage + 5 + 2, Unit.Damage);

			loadedModifier.Update(5);
			Assert.AreEqual(UnitDamage, Unit.Damage);
		}

		[Test]
		public void SaveUnitLoad()
		{
			AddRecipe("AddDamageExtraState")
				.Stack(WhenStackEffect.Always)
				.Effect(new AddDamageEffect(5, EffectState.IsRevertible, StackEffectType.Add, stackValue: 2),
					EffectOn.Init | EffectOn.Stack)
				.Remove(5);
			Setup();

			Unit.AddModifierSelf("InitDamage");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
			Unit.AddModifierSelf("AddDamageExtraState");
			Unit.AddModifierSelf("AddDamageExtraState");
			Unit.Update(2);
			Assert.AreEqual(UnitDamage + 5 + 5 + 2, Unit.Damage);

			var saveData = Unit.SaveState();
			var loadedUnit = new Unit(0, 0, 0, 0, UnitType.Neutral, UnitTag.None);
			SetupUnitHelper(loadedUnit);
			loadedUnit.LoadState(saveData);

			Assert.AreEqual(UnitHealth - 5, loadedUnit.Health);
			Assert.AreEqual(UnitDamage + 5 + 5 + 2, loadedUnit.Damage);

			loadedUnit.Update(3);
			Assert.AreEqual(UnitDamage, loadedUnit.Damage);
		}

		[Test]
		public void SaveUnitLoadFile()
		{
			AddRecipe("AddDamageExtraState")
				.Stack(WhenStackEffect.Always)
				.Effect(new AddDamageEffect(5, EffectState.IsRevertible, StackEffectType.Add, stackValue: 2),
					EffectOn.Init | EffectOn.Stack)
				.Remove(5);
			Setup();

			Unit.AddModifierSelf("InitDamage");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
			Unit.AddModifierSelf("AddDamageExtraState");
			Unit.AddModifierSelf("AddDamageExtraState");
			Unit.Update(2);
			Assert.AreEqual(UnitDamage + 5 + 5 + 2, Unit.Damage);

			var saveController = new SaveController("test.json");
			saveController.Save(Unit.SaveState());

			var loadData = saveController.Load<Unit.SaveData>();

			var loadedUnit = new Unit(0, 0, 0, 0, UnitType.Neutral, UnitTag.None);
			SetupUnitHelper(loadedUnit);
			loadedUnit.LoadState(loadData);

			Assert.AreEqual(UnitHealth - 5, loadedUnit.Health);
			Assert.AreEqual(UnitDamage + 5 + 5 + 2, loadedUnit.Damage);

			loadedUnit.Update(3);
			Assert.AreEqual(UnitDamage, loadedUnit.Damage);
		}

		[Test]
		public void SaveEffectCooldownCheckLoadFile()
		{
			AddRecipe("AddDamageExtraState")
				.EffectCooldown(2)
				.Stack(WhenStackEffect.Always)
				.Effect(
					new AddDamageEffect(5, EffectState.IsRevertible, StackEffectType.Effect | StackEffectType.Add,
						stackValue: 2), EffectOn.Stack);
			Setup();

			Unit.AddModifierSelf("AddDamageExtraState");
			Unit.Update(1);
			Assert.AreEqual(UnitDamage + 5 + 2, Unit.Damage);

			var saveController = new SaveController("test.json");
			saveController.Save(Unit.SaveState());

			var loadData = saveController.Load<Unit.SaveData>();

			var loadedUnit = new Unit(0, 0, 0, 0, UnitType.Neutral, UnitTag.None);
			SetupUnitHelper(loadedUnit);
			loadedUnit.LoadState(loadData);

			Assert.AreEqual(UnitDamage + 5 + 2, loadedUnit.Damage);
			loadedUnit.AddModifierSelf("AddDamageExtraState");
			Assert.AreEqual(UnitDamage + 5 + 2, loadedUnit.Damage);
			loadedUnit.Update(1);
			loadedUnit.AddModifierSelf("AddDamageExtraState");
			Assert.AreEqual(UnitDamage + 5 + 2 + 5 + 4, loadedUnit.Damage);
		}

		[Test]
		public void SaveLoadStatusEffect()
		{
			AddRecipe("InitStun")
				.Effect(new StatusEffectEffect(StatusEffectType.Stun, 2f), EffectOn.Init)
				.Remove(2);
			Setup();

			Unit.AddModifierSelf("InitStun");
			Unit.Update(1);

			var saveData = Unit.SaveState();
			var loadedUnit = new Unit(0, 0, 0, 0, UnitType.Neutral, UnitTag.None);
			loadedUnit.LoadState(saveData);

			Assert.True(loadedUnit.HasStatusEffectMulti(StatusEffectType.Stun));
			loadedUnit.Update(1);
			Assert.False(loadedUnit.HasStatusEffectMulti(StatusEffectType.Stun));
		}

		//[Test]
		public void SaveNewModifierIdLoad()
		{
			var saveModifierData = IdManager.SaveState();
			AddRecipe("InitDamage2")
				.Effect(new DamageEffect(5), EffectOn.Init);
			var saveModifierData2 = IdManager.SaveState();
			Setup();

			//TODO Save name next to Id (then check if the id has changed since last save)
			//Saves to file what each modifier name was in relation to id
			//So if we change the order of recipes/generators, we can still load the correct modifiers
			//Also will warn us if a recipe is missing / has been renamed
		}

		private void SetupUnitHelper(IUnit loadedUnit)
		{
			UnitHelper.Setup(i =>
			{
				if (i == Unit.Id)
					return loadedUnit;
				if (i == Enemy.Id)
					return Enemy;
				if (i == Ally.Id)
					return Ally;

				Logger.LogError($"Unit with id {i} not found");
				return null;
			});
		}

		//TODO Saving Target&Source Unit Id
		//TODO GenIds will be wrong in some places (StatusEffect), how to fix, feed correct id & genId somehow?
		//TODO Applier check
		//TODO CallbackEffect, CallbackUnit, CallbackState
		//TODO add damage is enabled check
	}
}