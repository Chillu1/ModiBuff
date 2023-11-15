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
			_saveController = new SaveController("test.json");
		}

		private Unit LoadUnit(Unit unit)
		{
			string json = _saveController.Save(unit.SaveState());
			var loadData = _saveController.Load(json);
			var loadedUnit = Unit.LoadUnit(loadData.Id);
			loadedUnit.LoadState(loadData);
			return loadedUnit;
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
			IdManager.LoadState(IdManager.SaveState());

			Unit.AddModifierSelf("InitDamage");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
			Unit.AddModifierSelf("AddDamageExtraState");
			Unit.AddModifierSelf("AddDamageExtraState");
			Unit.Update(2);
			Assert.AreEqual(UnitDamage + 5 + 5 + 2, Unit.Damage);

			var loadedUnit = LoadUnit(Unit);

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
			IdManager.LoadState(IdManager.SaveState());

			Unit.AddModifierSelf("AddDamageExtraState");
			Unit.Update(1);
			Assert.AreEqual(UnitDamage + 5 + 2, Unit.Damage);

			var loadedUnit = LoadUnit(Unit);

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
			IdManager.LoadState(IdManager.SaveState());

			Unit.AddModifierSelf("InitStun");
			Unit.Update(1);

			var loadedUnit = LoadUnit(Unit);

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
			IdManager.LoadState(IdManager.SaveState());

			Unit.AddModifierSelf("InitCallbackState");
			Unit.TakeDamage(5, Unit);
			Assert.AreEqual(UnitDamage + 5 + 5, Unit.Damage);

			var loadedUnit = LoadUnit(Unit);

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
			IdManager.LoadState(IdManager.SaveState());

			Unit.AddApplierModifier(Recipes.GetGenerator("InitDamageChecks"), ApplierType.Cast);
			Unit.TryCast("InitDamageChecks", Unit);
			Assert.AreEqual(UnitHealth - 5 - 5, Unit.Health);

			var loadedUnit = LoadUnit(Unit);

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
			IdManager.LoadState(IdManager.SaveState());

			Enemy.AddModifierTarget("DoT", Unit);
			Enemy.Update(1);
			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			//Needed order: new ids for all units assigned.
			//Save old and new ids to id map.
			//Load all units states.

			string jsonEnemy = _saveController.Save(Enemy.SaveState());
			var loadDataEnemy = _saveController.Load(jsonEnemy);
			var loadedEnemy = Unit.LoadUnit(loadDataEnemy.Id);

			string json = _saveController.Save(Unit.SaveState());
			var loadData = _saveController.Load(json);
			var loadedUnit = Unit.LoadUnit(loadData.Id);

			loadedEnemy.LoadState(loadDataEnemy);
			loadedUnit.LoadState(loadData);

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
			{
				_saveController.SaveToPath(_saveController.Save(IdManager.SaveState()), idManagerPath);
			}

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

		//TODO GenIds will be wrong in some places (StatusEffect), how to fix, feed correct id & genId somehow?
		//TODO add damage is enabled check
	}
}