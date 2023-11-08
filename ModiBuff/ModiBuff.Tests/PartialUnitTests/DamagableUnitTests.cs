using System.Collections.Generic;
using ModiBuff.Core;
using ModiBuff.Core.Units;
using ModiBuff.Core.Units.Interfaces.NonGeneric;
using NUnit.Framework;
using IEventOwner = ModiBuff.Core.IEventOwner;

namespace ModiBuff.Tests
{
	public sealed class DamagableUnitTests : PartialUnitModifierTests<DamagableUnitTests.DamagableUnit>
	{
		protected override void SetupUnitFactory() =>
			UnitFactory = (health, damage, heal, mana, type, tag) => new DamagableUnit(health, type);

		public sealed class DamagableUnit : IUnit, IModifierOwner, IDamagable, IEventOwner,
			ICallbackRegistrable<CallbackType>, IUpdatable, IUnitEntity, IHealthCost
		{
			public UnitTag UnitTag { get; }
			public UnitType UnitType { get; }

			public float Health { get; private set; }
			public float MaxHealth { get; }
			public bool IsDead { get; private set; }

			public ModifierController ModifierController { get; }

			private const int MaxRecursionEventCount = 1;

			private readonly List<HealthChangedEvent> _healthChangedEvents;
			private int _healthChangedCounter;

			public DamagableUnit(float health, UnitType unitType = UnitType.Good)
			{
				UnitType = unitType;
				UnitTag = UnitTag.Default;
				MaxHealth = Health = health;

				ModifierController = new ModifierController(this);
				_healthChangedEvents = new List<HealthChangedEvent>();
			}

			public void Update(float delta)
			{
				ModifierController.Update(delta);
			}

			public float TakeDamage(float damage, IUnit source)
			{
				float oldHealth = Health;
				Health -= damage;

				float dealtDamage = oldHealth - Health;

				if (dealtDamage > 0)
				{
					_healthChangedCounter++;
					if (_healthChangedCounter <= MaxRecursionEventCount)
					{
						for (int i = 0; i < _healthChangedEvents.Count; i++)
							_healthChangedEvents[i](this, source, Health, dealtDamage);
					}
				}

				if (Health <= 0 && !IsDead)
				{
					ModifierController.Clear();
					IsDead = true;
				}

				if (_healthChangedCounter <= MaxRecursionEventCount)
				{
					ResetEventCounters();
					(source as IEventOwner)?.ResetEventCounters();
				}

				return dealtDamage;
			}

			public void UseHealth(float value)
			{
				Health -= value;
			}

			public void RegisterCallbacks(Callback<CallbackType>[] callbacks)
			{
				for (int i = 0; i < callbacks.Length; i++)
				{
					var callback = callbacks[i];
					switch (callback.CallbackType)
					{
						case CallbackType.CurrentHealthChanged:
							if (CheckCallback(callback.Action, out HealthChangedEvent healthEvent))
							{
								healthEvent.Invoke(this, this, Health, 0f);
								_healthChangedEvents.Add(healthEvent);
							}

							break;
						default:
							Logger.Log(
								$"CallbackType {callback.CallbackType} is not implemented. For unit {nameof(DamagableUnit)}.");
							break;
					}
				}
			}

			public void UnRegisterCallbacks(Callback<CallbackType>[] callbacks)
			{
				for (int i = 0; i < callbacks.Length; i++)
				{
					var callback = callbacks[i];
					switch (callback.CallbackType)
					{
						case CallbackType.CurrentHealthChanged:
							if (_healthChangedEvents.Remove((HealthChangedEvent)callback.Action))
								((HealthChangedEvent)callback.Action).Invoke(this, this, Health, 0f);
							break;
						default:
							Logger.Log(
								$"CallbackType {callback.CallbackType} is not implemented. For unit {nameof(DamagableUnit)}.");
							break;
					}
				}
			}

			public void ResetEventCounters()
			{
				_healthChangedCounter = 0;
			}

			private static bool CheckCallback<TCallback>(object callbackObject, out TCallback callbackOut)
			{
				if (!(callbackObject is TCallback callback))
				{
					Logger.LogError($"objectDelegate is not of type {nameof(TCallback)}, use named delegates instead.");
					callbackOut = default;
					return false;
				}

				callbackOut = callback;
				return true;
			}
		}

		[Test]
		public void TryDamageAndHeal_UnHealableUnit()
		{
			AddRecipe("InitDamageHeal")
				.Effect(new DamageEffect(5), EffectOn.Init)
				.Effect(new HealEffect(5, HealEffect.EffectState.IsRevertible), EffectOn.Init)
				.Remove(1);
			Setup();

			Unit.AddModifierSelf("InitDamageHeal");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
			Unit.Update(1);
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void TryAddDamage_NoDamageUnit()
		{
			AddRecipe("InitAddDamage")
				.Effect(new AddDamageEffect(5, EffectState.IsRevertible), EffectOn.Init)
				.Remove(1);
			Setup();

			Unit.AddModifierSelf("InitAddDamage"); //Try add damage, can't
			Unit.Update(1); //Try revert, can't
		}

		[Test]
		public void TryStun_NoStatusEffectsUnit()
		{
			AddRecipe("InitStun")
				.Effect(new StatusEffectEffect(StatusEffectType.Stun, 2f, true), EffectOn.Init)
				.Remove(1);
			Setup();

			Unit.AddModifierSelf("InitStun");
			Unit.Update(1);
			Assert.False(Unit.ContainsModifier("InitStun"));
		}

		[Test]
		public void TryDamagePostHeal_UnHealableUnit()
		{
			AddRecipe("InitDamagePostHeal")
				.Effect(new DamageEffect(5).SetPostEffects(new LifeStealPostEffect(1f)), EffectOn.Init);
			Setup();

			Unit.AddModifierSelf("InitDamagePostHeal");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void InitDamageApplyCosts_UnitWithoutMana()
		{
			AddRecipe("InitDamageManaCost")
				.ApplyCost(CostType.Mana, 5)
				.Effect(new DamageEffect(5), EffectOn.Init);
			AddRecipe("InitDamageHealthCost")
				.ApplyCost(CostType.Health, 5)
				.Effect(new DamageEffect(5), EffectOn.Init);
			Setup();

			Unit.AddApplierModifier(Recipes.GetGenerator("InitDamageManaCost"), ApplierType.Cast);
			Unit.AddApplierModifier(Recipes.GetGenerator("InitDamageHealthCost"), ApplierType.Cast);

			Unit.TryCast("InitDamageManaCost", Unit);
			Assert.AreEqual(UnitHealth, Unit.Health);

			Unit.TryCast("InitDamageHealthCost", Unit);
			Assert.AreEqual(UnitHealth - 5 - 5, Unit.Health);
		}

		[Test]
		public void InitDamageEffectCosts_UnitWithoutMana()
		{
			AddRecipe("InitDamageManaCost")
				.EffectCost(CostType.Mana, 5)
				.Effect(new DamageEffect(5), EffectOn.Init);
			AddRecipe("InitDamageHealthCost")
				.EffectCost(CostType.Health, 5)
				.Effect(new DamageEffect(5), EffectOn.Init);
			Setup();

			Unit.AddApplierModifier(Recipes.GetGenerator("InitDamageManaCost"), ApplierType.Cast);
			Unit.AddApplierModifier(Recipes.GetGenerator("InitDamageHealthCost"), ApplierType.Cast);

			Unit.TryCast("InitDamageManaCost", Unit);
			Assert.AreEqual(UnitHealth, Unit.Health);

			Unit.TryCast("InitDamageHealthCost", Unit);
			Assert.AreEqual(UnitHealth - 5 - 5, Unit.Health);
		}

		[Test]
		public void InitDamageEventCallback_UnitWithoutEvents()
		{
			AddRecipe("InitDamageWhenAttacked")
				.Effect(new DamageEffect(5), EffectOn.Event)
				.Event(EffectOnEvent.WhenAttacked);
			Setup();

			Unit.AddModifierSelf("InitDamageWhenAttacked");
			Unit.TakeDamage(0f, Unit);
			Assert.AreEqual(UnitHealth, Unit.Health);
		}

		[Test]
		public void AttackAction_UnitWithoutAttack()
		{
			AddRecipe("AttackAction")
				.Effect(new AttackActionEffect(), EffectOn.Init);
			Setup();

			Unit.AddModifierSelf("AttackAction");
			Assert.AreEqual(UnitHealth, Unit.Health);
		}
	}
}