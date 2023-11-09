using System.Collections.Generic;
using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;
using IEventOwner = ModiBuff.Core.IEventOwner;

namespace ModiBuff.Tests
{
	public sealed class NoModifierUnitTests : PartialUnitModifierTests<NoModifierUnitTests.NoModifierUnit>
	{
		protected override void SetupUnitFactory() =>
			UnitFactory = (health, damage, heal, mana, type, tag) => new NoModifierUnit(health, type);

		public sealed class NoModifierUnit : IUnit, IDamagable, IEventOwner, IUnitEntity,
			ICallbackRegistrable<CallbackType>, IKillable
		{
			public UnitTag UnitTag { get; }
			public UnitType UnitType { get; }

			public float Health { get; private set; }
			public float MaxHealth { get; }
			public bool IsDead { get; private set; }

			private const int MaxRecursionEventCount = 1;

			private readonly List<HealthChangedEvent> _healthChangedEvents;
			private int _healthChangedCounter;

			public NoModifierUnit(float health, UnitType unitType = UnitType.Good)
			{
				UnitType = unitType;
				UnitTag = UnitTag.Default;
				MaxHealth = Health = health;

				_healthChangedEvents = new List<HealthChangedEvent>();
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
					IsDead = true;

				if (_healthChangedCounter <= MaxRecursionEventCount)
				{
					ResetEventCounters();
					(source as IEventOwner)?.ResetEventCounters();
				}

				return dealtDamage;
			}

			public void RegisterCallbacks(Callback<CallbackType>[] callbacks)
			{
				for (int i = 0; i < callbacks.Length; i++)
				{
					var callback = callbacks[i];
					switch (callback.CallbackType)
					{
						case CallbackType.CurrentHealthChanged:
							if (callback.CheckCallback(out HealthChangedEvent healthEvent))
							{
								healthEvent.Invoke(this, this, Health, 0f);
								_healthChangedEvents.Add(healthEvent);
							}

							break;
						default:
							Logger.Log(
								$"CallbackType {callback.CallbackType} is not implemented. For unit {nameof(NoModifierUnit)}.");
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
							var @event = (HealthChangedEvent)callback.Action;
							if (_healthChangedEvents.Remove(@event))
								@event.Invoke(this, this, Health, 0f);
							break;
						default:
							Logger.Log(
								$"CallbackType {callback.CallbackType} is not implemented. For unit {nameof(NoModifierUnit)}.");
							break;
					}
				}
			}

			public void ResetEventCounters()
			{
				_healthChangedCounter = 0;
			}
		}

		[Test]
		public void TryApplyDamageAppliers_NoModifiersUnit()
		{
			AddRecipe("InitDamageCooldown")
				.ApplyCooldown(1)
				.Effect(new DamageEffect(5), EffectOn.Init);
			Setup();

			Enemy.AddApplierModifier(Recipes.GetGenerator("InitDamage"), ApplierType.Attack);
			Enemy.AddApplierModifier(Recipes.GetGenerator("InitDamageCooldown"), ApplierType.Attack);

			Enemy.Attack(Unit);
			Assert.AreEqual(UnitHealth - EnemyDamage, Unit.Health);
		}

		[Test]
		public void ApplyDamageEffect_NoModifiersUnit()
		{
			AddEffect("InitDamage", new DamageEffect(5));
			Setup();

			Enemy.AddEffectApplier("InitDamage");

			Enemy.TryCastEffect("InitDamage", Unit);
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void TryApplyApplierEffect_NoModifiersUnit()
		{
			AddEffect("InitDamageApplier", new ApplierEffect("InitDamage"));
			Setup();

			Unit.ApplyEffectSelf("InitDamageApplier");
		}
	}
}