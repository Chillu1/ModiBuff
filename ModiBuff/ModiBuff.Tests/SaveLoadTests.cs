using System.IO;
using ModiBuff.Core;
using ModiBuff.Core.Units;
using ModiBuff.Extensions.Serialization.Json;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class SaveLoadTests : ModifierTests
	{
		private SaveController _saveController;

		public override void IterationSetup()
		{
			base.IterationSetup();
			_saveController = new SaveController("fullSave.json");
		}

		private void SaveLoadGameState(Unit unit, out Unit loadedUnit)
		{
			string jsonGameState = _saveController.Save(GameState.SaveState(IdManager, new[] { unit }));
			var loadDataGameState = _saveController.Load(jsonGameState);
			GameState.LoadState(loadDataGameState, IdManager, out var loadedUnits);
			loadedUnit = loadedUnits[0];
		}

		private void SaveLoadGameState(Unit[] units, out Unit[] loadedUnits)
		{
			string jsonGameState = _saveController.Save(GameState.SaveState(IdManager, units));
			var loadDataGameState = _saveController.Load(jsonGameState);
			GameState.LoadState(loadDataGameState, IdManager, out loadedUnits);
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

			SaveLoadGameState(Unit, out var loadedUnit);

			Assert.AreEqual(UnitHealth - 5, loadedUnit.Health);
			Assert.AreEqual(UnitDamage + 5 + 5 + 2, loadedUnit.Damage);

			loadedUnit.Update(3);
			Assert.AreEqual(UnitDamage, loadedUnit.Damage);
		}

		[Test]
		public void SaveEffectCooldownCheckLoad()
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

			SaveLoadGameState(Unit, out var loadedUnit);

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
				.Effect(new SingleInstanceStatusEffectEffect(StatusEffectType.Stun, 2f), EffectOn.Init)
				.Remove(2);
			Setup();

			Unit.AddModifierSelf("InitStun");
			Unit.Update(1);

			SaveLoadGameState(Unit, out var loadedUnit);

			Assert.True(loadedUnit.HasStatusEffectMulti(StatusEffectType.Stun));
			Assert.True(loadedUnit.HasStatusEffectSingle(StatusEffectType.Stun));
			loadedUnit.Update(1);
			Assert.False(loadedUnit.HasStatusEffectMulti(StatusEffectType.Stun));
			Assert.False(loadedUnit.HasStatusEffectSingle(StatusEffectType.Stun));
			Assert.False(loadedUnit.ContainsModifier("InitStun"));
		}

		[Test]
		public void SaveLoadEventCallbackState()
		{
			AddRecipe("InitCallbackState")
				.Effect(new AddDamageEffect(5, EffectState.IsRevertible), EffectOn.Event | EffectOn.CallbackEffect)
				.Event(EffectOnEvent.WhenAttacked)
				.CallbackEffect(CallbackType.CurrentHealthChanged,
					effect => new HealthChangedEvent((target, source, health, deltaHealth) =>
					{
						if (deltaHealth > 0)
							effect.Effect(target, source);
					}))
				.Remove(5);
			Setup();

			Unit.AddModifierSelf("InitCallbackState");
			Unit.TakeDamage(5, Unit);
			Assert.AreEqual(UnitDamage + 5 + 5, Unit.Damage);

			SaveLoadGameState(Unit, out var loadedUnit);

			loadedUnit.TakeDamage(5, loadedUnit);
			Assert.AreEqual(UnitDamage + 5 + 5 + 5 + 5, loadedUnit.Damage);

			loadedUnit.Update(5);
			Assert.AreEqual(UnitDamage, loadedUnit.Damage);
		}

		[Test]
		public void SaveLoadApplierState()
		{
			AddRecipe("InitDamageChecks")
				.ApplyCooldown(1)
				.ApplyCost(CostType.Health, 5)
				.Effect(new DamageEffect(5), EffectOn.Init);
			Setup();

			Unit.AddApplierModifier(Recipes.GetGenerator("InitDamageChecks"), ApplierType.Cast);
			Unit.TryCast("InitDamageChecks", Unit);
			Assert.AreEqual(UnitHealth - 5 - 5, Unit.Health);

			SaveLoadGameState(Unit, out var loadedUnit);

			loadedUnit.TryCast("InitDamageChecks", loadedUnit);
			Assert.AreEqual(UnitHealth - 5 - 5, loadedUnit.Health);

			loadedUnit.Update(1);
			loadedUnit.TryCast("InitDamageChecks", loadedUnit);
			Assert.AreEqual(UnitHealth - 5 - 5 - 5 - 5, loadedUnit.Health);
		}

		[Test]
		public void SaveLoadTargetId()
		{
			AddRecipe("DoT")
				.Interval(1)
				.Effect(new DamageEffect(5), EffectOn.Interval);
			Setup();

			Enemy.AddModifierTarget("DoT", Unit);
			Enemy.Update(1);
			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			//Needed order: new ids for all units assigned.
			//Save old and new ids to id map.
			//Load all units states.

			SaveLoadGameState(new[] { Enemy, Unit }, out var loadedUnits);

			var loadedEnemy = loadedUnits[0];
			var loadedUnit = loadedUnits[1];

			loadedEnemy.Update(1);
			Assert.AreEqual(UnitHealth - 5 - 5, loadedUnit.Health);
		}

		[Test]
		public void SaveNewModifierIdLoad()
		{
			AddRecipe("DoT")
				.Interval(1)
				.Effect(new DamageEffect(10), EffectOn.Interval);
			Setup();

			const string idManagerPath = "idManagerTest.json";
			const string unitPath = "unitIdTest.json";

			//TODO save will not have modifier id redirection
			if (!File.Exists(_saveController.Path + "/" + idManagerPath))
				_saveController.SaveToPath(_saveController.Save(IdManager.SaveState()), idManagerPath);

			if (!File.Exists(_saveController.Path + "/" + unitPath))
			{
				Unit.AddModifierSelf("DoT");
				_saveController.SaveToPath(_saveController.Save(Unit.SaveState()), unitPath);
			}

			var idManagerData = _saveController.LoadFromPath<ModifierIdManager.SaveData>(idManagerPath);
			IdManager.LoadState(idManagerData);

			var loadData = _saveController.LoadFromPath<Unit.SaveData>(unitPath);
			var loadedUnit = Unit.LoadUnit(loadData.Id);
			loadedUnit.LoadState(loadData);

			loadedUnit.Update(1);
			Assert.AreEqual(UnitHealth - 10, loadedUnit.Health);
			loadedUnit.Update(1);
			Assert.AreEqual(UnitHealth - 20, loadedUnit.Health);
		}

		[Test]
		public void SaveNewModifierApplierIdLoad()
		{
			AddRecipe("DoT")
				.Interval(1)
				.Effect(new DamageEffect(10), EffectOn.Interval);
			AddRecipe("DoTHealthCost")
				.ApplyCost(CostType.Health, 5)
				.Interval(1)
				.Effect(new DamageEffect(10), EffectOn.Interval);
			Setup();

			const string idManagerPath = "idManagerApplierTest.json";
			const string unitPath = "unitIdApplierTest.json";

			//TODO save will not have modifier id redirection
			if (!File.Exists(_saveController.Path + "/" + idManagerPath))
				_saveController.SaveToPath(_saveController.Save(IdManager.SaveState()), idManagerPath);

			if (!File.Exists(_saveController.Path + "/" + unitPath))
			{
				Unit.AddApplierModifier(Recipes.GetGenerator("DoT"), ApplierType.Cast);
				Unit.AddApplierModifier(Recipes.GetGenerator("DoTHealthCost"), ApplierType.Cast);
				_saveController.SaveToPath(_saveController.Save(Unit.SaveState()), unitPath);
			}

			var idManagerData = _saveController.LoadFromPath<ModifierIdManager.SaveData>(idManagerPath);
			IdManager.LoadState(idManagerData);

			var loadData = _saveController.LoadFromPath<Unit.SaveData>(unitPath);
			var loadedUnit = Unit.LoadUnit(loadData.Id);
			loadedUnit.LoadState(loadData);

			loadedUnit.TryCast("DoT", loadedUnit);
			loadedUnit.TryCast("DoTHealthCost", loadedUnit);
			loadedUnit.Update(1);
			Assert.AreEqual(UnitHealth - 10 - 5 - 10, loadedUnit.Health);
			loadedUnit.Update(1);
			Assert.AreEqual(UnitHealth - 10 - 5 - 10 - 10 - 10, loadedUnit.Health);
		}

		//TODO GenIds will be wrong in some places (StatusEffect), how to fix, feed correct id & genId somehow?
		//TODO add damage is enabled check
	}
}