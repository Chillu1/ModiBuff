using System;
using System.Collections.Generic;
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
			string jsonGameState =
				_saveController.Save(GameState.SaveState(IdManager, EffectIdManager, new[] { unit }));
			var loadDataGameState = _saveController.Load(jsonGameState);
			GameState.LoadState(loadDataGameState, IdManager, EffectIdManager, out var loadedUnits);
			loadedUnit = loadedUnits[0];
		}

		private void SaveLoadGameState(Unit[] units, out Unit[] loadedUnits)
		{
			string jsonGameState = _saveController.Save(GameState.SaveState(IdManager, EffectIdManager, units));
			var loadDataGameState = _saveController.Load(jsonGameState);
			GameState.LoadState(loadDataGameState, IdManager, EffectIdManager, out loadedUnits);
		}

		private void SaveGameState(string gameStateFile, Unit unit)
		{
			_saveController.SaveToPath(
				_saveController.Save(GameState.SaveState(IdManager, EffectIdManager, new[] { unit })), gameStateFile);
		}

		private void SaveGameState(string gameStateFile, params Unit[] units)
		{
			_saveController.SaveToPath(_saveController.Save(GameState.SaveState(IdManager, EffectIdManager, units)),
				gameStateFile);
		}

		private void LoadGameState(string gameStateFile, out Unit loadedUnit)
		{
			var loadDataGameState = _saveController.LoadFromPath<GameState.SaveData>(gameStateFile);
			GameState.LoadState(loadDataGameState, IdManager, EffectIdManager, out var loadedUnits);
			loadedUnit = loadedUnits[0];
		}

		private void LoadGameState(string gameStateFile, out Unit[] loadedUnits)
		{
			var loadDataGameState = _saveController.LoadFromPath<GameState.SaveData>(gameStateFile);
			GameState.LoadState(loadDataGameState, IdManager, EffectIdManager, out loadedUnits);
		}

		[Test]
#if !MODIBUFF_SYSTEM_TEXT_JSON
		[Ignore("MODIBUFF_SYSTEM_TEXT_JSON not set. Skipping test")]
#endif
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
#if !MODIBUFF_SYSTEM_TEXT_JSON
		[Ignore("MODIBUFF_SYSTEM_TEXT_JSON not set. Skipping test")]
#endif
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
#if !MODIBUFF_SYSTEM_TEXT_JSON
		[Ignore("MODIBUFF_SYSTEM_TEXT_JSON not set. Skipping test")]
#endif
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
#if !MODIBUFF_SYSTEM_TEXT_JSON
		[Ignore("MODIBUFF_SYSTEM_TEXT_JSON not set. Skipping test")]
#endif
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
#if !MODIBUFF_SYSTEM_TEXT_JSON
		[Ignore("MODIBUFF_SYSTEM_TEXT_JSON not set. Skipping test")]
#endif
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
#if !MODIBUFF_SYSTEM_TEXT_JSON
		[Ignore("MODIBUFF_SYSTEM_TEXT_JSON not set. Skipping test")]
#endif
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
#if !MODIBUFF_SYSTEM_TEXT_JSON
		[Ignore("MODIBUFF_SYSTEM_TEXT_JSON not set. Skipping test")]
#endif
		public void SaveNewModifierIdLoad()
		{
			AddRecipe("DoT")
				.Interval(1)
				.Effect(new DamageEffect(10), EffectOn.Interval);
			Setup();

			const string gameStateFile = "modifierIdGameStateTest.json";

			//TODO save will not have modifier id redirection
			if (!File.Exists(_saveController.Path + "/" + gameStateFile))
			{
				Unit.AddModifierSelf("DoT");
				SaveGameState(gameStateFile, Unit);
			}

			LoadGameState(gameStateFile, out Unit loadedUnit);

			loadedUnit.Update(1);
			Assert.AreEqual(UnitHealth - 10, loadedUnit.Health);
			loadedUnit.Update(1);
			Assert.AreEqual(UnitHealth - 20, loadedUnit.Health);
		}

		[Test]
#if !MODIBUFF_SYSTEM_TEXT_JSON
		[Ignore("MODIBUFF_SYSTEM_TEXT_JSON not set. Skipping test")]
#endif
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

			const string gameStateFile = "modifierApplierIdGameStateTest.json";

			//TODO save will not have modifier id redirection
			if (!File.Exists(_saveController.Path + "/" + gameStateFile))
			{
				Unit.AddApplierModifier(Recipes.GetGenerator("DoT"), ApplierType.Cast);
				Unit.AddApplierModifier(Recipes.GetGenerator("DoTHealthCost"), ApplierType.Cast);
				SaveGameState(gameStateFile, Unit);
			}

			LoadGameState(gameStateFile, out Unit loadedUnit);

			loadedUnit.TryCast("DoT", loadedUnit);
			loadedUnit.TryCast("DoTHealthCost", loadedUnit);
			loadedUnit.Update(1);
			Assert.AreEqual(UnitHealth - 10 - 5 - 10, loadedUnit.Health);
			loadedUnit.Update(1);
			Assert.AreEqual(UnitHealth - 10 - 5 - 10 - 10 - 10, loadedUnit.Health);
		}

		[Test]
#if !MODIBUFF_SYSTEM_TEXT_JSON
		[Ignore("MODIBUFF_SYSTEM_TEXT_JSON not set. Skipping test")]
#endif
		public void SaveNewEffectIdLoad()
		{
			AddEffect("InitDamage", new DamageEffect(5));
			AddEffect("InitBigDamage", new DamageEffect(10));
			Setup();

			const string gameStateFile = "effectIdGameStateTest.json";

			//TODO save will not have modifier id redirection
			if (!File.Exists(_saveController.Path + "/" + gameStateFile))
			{
				Unit.AddEffectApplier("InitBigDamage");
				SaveGameState(gameStateFile, Unit);
			}

			LoadGameState(gameStateFile, out Unit loadedUnit);

			loadedUnit.TryCastEffect("InitBigDamage", loadedUnit);
			Assert.AreEqual(UnitHealth - 10, loadedUnit.Health);
		}

		[Test]
#if !MODIBUFF_SYSTEM_TEXT_JSON
		[Ignore("MODIBUFF_SYSTEM_TEXT_JSON not set. Skipping test")]
#endif
		public void SaveModifierNewEffectLoad()
		{
			AddRecipe("InitHeal")
				.Stack(WhenStackEffect.Always)
				.Effect(new HealEffect(5, HealEffect.EffectState.IsRevertible,
					StackEffectType.Effect | StackEffectType.Add, 2), EffectOn.Stack);
			AddRecipe("AddDamage")
				.Effect(new AddDamageEffect(5), EffectOn.Init);
			Setup();

			const int damage = 100;

			const string gameStateFile = "modifierNewEffectIdGameStateTest.json";

			//TODO save will not have modifier id redirection
			if (!File.Exists(_saveController.Path + "/" + gameStateFile))
			{
				Unit.TakeDamage(damage, Unit);
				Unit.AddModifierSelf("InitHeal");
				SaveGameState(gameStateFile, Unit);
			}

			LoadGameState(gameStateFile, out Unit loadedUnit);

			loadedUnit.AddModifierSelf("InitHeal");
			Assert.AreEqual(UnitHealth - damage + 5 + 2 + 5 + 4, loadedUnit.Health);
		}

		//[Test]
		public void SaveStatusEffectGenIdLoad()
		{
			AddRecipe("InitStun")
				.Effect(new StatusEffectEffect(StatusEffectType.Stun, 2f, true), EffectOn.Init)
				.Remove(RemoveEffectOn.CallbackUnit)
				.CallbackUnit(CallbackUnitType.StrongDispel);
			Setup();

			Unit.AddModifierSelf("InitStun");

			SaveLoadGameState(Unit, out var loadedUnit);
			//TODO Save Id & GenId, update it to correct values
			//StatusEffectEffect needs to save it's id and genId when saving state
			//Then we need to feed the new updated id and genId to StatusEffectController

			Assert.True(loadedUnit.HasStatusEffectMulti(StatusEffectType.Stun));
			loadedUnit.StrongDispel(loadedUnit);
			Assert.False(loadedUnit.HasStatusEffectMulti(StatusEffectType.Stun));
		}

		[Test]
#if !MODIBUFF_SYSTEM_TEXT_JSON
		[Ignore("MODIBUFF_SYSTEM_TEXT_JSON not set. Skipping test")]
#endif
		public void SaveCallbackLocalVarState()
		{
			AddRecipe("InitTakeFiveDamageOnTenDamageTaken")
				.Callback(CallbackType.CurrentHealthChanged, () =>
				{
					float totalDamageTaken = 0f; //state != null ? (float)state : 0f;

					return new CallbackStateContext<float>(new HealthChangedEvent(
						(target, source, health, deltaHealth) =>
						{
							//Don't count "negative damage/healing damage"
							if (deltaHealth > 0)
								totalDamageTaken += deltaHealth;
							if (totalDamageTaken >= 10)
							{
								totalDamageTaken = 0f;
								target.TakeDamage(5, source);
							}
						}), () => totalDamageTaken, value => totalDamageTaken = value);
				});
			Setup();

			Unit.AddModifierSelf("InitTakeFiveDamageOnTenDamageTaken");
			Unit.TakeDamage(5, Unit);

			SaveLoadGameState(Unit, out var loadedUnit);

			loadedUnit.TakeDamage(5, loadedUnit);
			Assert.AreEqual(UnitHealth - 5 - 5 - 5, loadedUnit.Health);
		}

		[Test]
		public void SavePoisonEffectLoad()
		{
#if !MODIBUFF_SYSTEM_TEXT_JSON
			Setup();
			Assert.Ignore("MODIBUFF_SYSTEM_TEXT_JSON not defined. Skipping test.");
#else
			SerializationExtensions.AddCustomValueType<IReadOnlyDictionary<int, int>>(element =>
			{
				var dictionary = new Dictionary<int, int>();
				foreach (var kvp in element.EnumerateObject())
					dictionary.Add(int.Parse(kvp.Name), kvp.Value.GetInt32());
				return dictionary;
			});
#endif

			AddRecipe(CentralizedCustomLogicTests.PoisonRecipe);
			AddRecipe("PoisonThorns")
				.Callback(CallbackType.PoisonDamage,
					new PoisonEvent((target, source, stacks, totalStacks, damage) =>
					{
						((IAttackable<float, float>)source).TakeDamage(damage, target);
					}));
			Setup();

			const string gameStateFile = "poisonEffectGameStateTest.json";

			//TODO save will not have unit id redirection
			if (!File.Exists(_saveController.Path + "/" + gameStateFile))
			{
				Ally.AddApplierModifier(Recipes.GetGenerator("Poison"), ApplierType.Cast);
				Enemy.AddApplierModifier(Recipes.GetGenerator("Poison"), ApplierType.Cast);
				Unit.AddModifierSelf("PoisonThorns");

				Ally.TryCast("Poison", Unit);
				Enemy.TryCast("Poison", Unit);
				Enemy.TryCast("Poison", Unit);
				SaveGameState(gameStateFile, Ally, Enemy, Unit);
			}

			LoadGameState(gameStateFile, out Unit[] loadedUnits);

			var loadedAlly = loadedUnits[0];
			var loadedEnemy = loadedUnits[1];
			var loadedUnit = loadedUnits[2];
			loadedUnit.Update(1);
			Assert.AreEqual(UnitHealth - 5 - 5 * 2, loadedUnit.Health);
			Assert.AreEqual(AllyHealth - 5, loadedAlly.Health);
			Assert.AreEqual(EnemyHealth - 5 * 2, loadedEnemy.Health);
		}

		[Test]
		public void SaveCallbackLocalTupleVarState()
		{
#if !MODIBUFF_SYSTEM_TEXT_JSON
			Setup();
			Assert.Ignore("MODIBUFF_SYSTEM_TEXT_JSON not defined. Skipping test.");
#else
			SerializationExtensions.AddCustomValueType<Tuple<float, float>>(element =>
			{
				float[] array = new float[2];
				int i = 0;
				foreach (var kvp in element.EnumerateObject())
					array[i++] = kvp.Value.GetSingle();
				return new Tuple<float, float>(array[0], array[1]);
			});
#endif

			AddRecipe("InitTakeFiveDamageOnTenDamageTaken")
				.Callback(CallbackType.CurrentHealthChanged, () =>
				{
					float totalDamageTaken = 0f;
					float maxDamageTaken = 0f;

					return new CallbackStateContext<Tuple<float, float>>(new HealthChangedEvent(
							(target, source, health, deltaHealth) =>
							{
								//Don't count "negative damage/healing damage"
								if (deltaHealth > 0)
								{
									totalDamageTaken += deltaHealth;
									if (deltaHealth > maxDamageTaken)
									{
										totalDamageTaken = 0f;
										maxDamageTaken = deltaHealth;
									}
								}

								if (totalDamageTaken >= 10)
								{
									totalDamageTaken = 0f;
									target.TakeDamage(5, source);
								}
							}),
						() => new Tuple<float, float>(totalDamageTaken, maxDamageTaken),
						value =>
						{
							totalDamageTaken = value.Item1;
							maxDamageTaken = value.Item2;
						});
				});
			Setup();

			Unit.AddModifierSelf("InitTakeFiveDamageOnTenDamageTaken");
			Unit.TakeDamage(5, Unit);
			Unit.TakeDamage(5, Unit);

			SaveLoadGameState(Unit, out var loadedUnit);

			loadedUnit.TakeDamage(5, loadedUnit);
			Assert.AreEqual(UnitHealth - 5 - 5 - 5 - 5, loadedUnit.Health);
		}

		[Test]
#if !MODIBUFF_SYSTEM_TEXT_JSON
		[Ignore("MODIBUFF_SYSTEM_TEXT_JSON not set. Skipping test")]
#endif
		public void SaveCallbackEffectLocalFloatVarState()
		{
			AddRecipe("StunnedFourTimesDispelAllStatusEffects")
				.Effect(new DispelStatusEffectEffect(StatusEffectType.All), EffectOn.CallbackEffect)
				.CallbackEffect(CallbackType.StatusEffectAdded, effect =>
				{
					float totalTimesStunned = 0f;
					return new CallbackStateContext<float>(
						new StatusEffectEvent((target, source, statusEffect, oldLegalAction, newLegalAction) =>
						{
							if (statusEffect.HasStatusEffect(StatusEffectType.Stun))
							{
								totalTimesStunned++;
								if (totalTimesStunned >= 4)
								{
									totalTimesStunned = 0f;
									effect.Effect(target, source);
								}
							}
						}), () => totalTimesStunned, value => totalTimesStunned = value);
				});
			Setup();

			Unit.AddModifierSelf("StunnedFourTimesDispelAllStatusEffects");
			Unit.ChangeStatusEffect(StatusEffectType.Stun, 1f, Enemy);
			Unit.ChangeStatusEffect(StatusEffectType.Stun, 1f, Enemy);
			Assert.True(Unit.HasStatusEffectMulti(StatusEffectType.Stun));

			SaveLoadGameState(Unit, out var loadedUnit);

			loadedUnit.ChangeStatusEffect(StatusEffectType.Stun, 1f, Enemy);
			loadedUnit.ChangeStatusEffect(StatusEffectType.Stun, 1f, Enemy);
			Assert.False(loadedUnit.HasStatusEffectMulti(StatusEffectType.Stun));
		}

		//TODO GenIds will be wrong in some places (StatusEffect), how to fix, feed correct id & genId somehow?
		//TODO add damage is enabled check
	}
}