using System.Collections.Generic;
using ModiBuff.Core;
using ModiBuff.Core.Units;
using ModiBuff.Core.Units.Interfaces.NonGeneric;
using NUnit.Framework;
using IEventOwner = ModiBuff.Core.IEventOwner;
using IDamagable = ModiBuff.Core.Units.Interfaces.NonGeneric.IDamagable;

namespace ModiBuff.Tests
{
	public sealed class DamagableUnitTests : PartialUnitModifierTests<DamagableUnitTests.DamagableUnit>
	{
		protected override void SetupUnitFactory() =>
			UnitFactory = (health, damage, heal, mana, type, tag) => new DamagableUnit(health, type);

		public sealed class DamagableUnit : IUnit, IModifierOwner, IDamagable, IUpdatable,
			IUnitEntity, IHealthCost, IModifierApplierOwner, IKillable
		{
			public UnitTag UnitTag { get; }
			public UnitType UnitType { get; }

			public float Health { get; private set; }
			public float MaxHealth { get; }
			public bool IsDead { get; private set; }

			public ModifierController ModifierController { get; }
			public ModifierApplierController ModifierApplierController { get; }

			public DamagableUnit(float health, UnitType unitType = UnitType.Good)
			{
				UnitType = unitType;
				UnitTag = UnitTag.Default;
				MaxHealth = Health = health;

				ModifierController = ModifierControllerPool.Instance.Rent();
				ModifierApplierController = ModifierControllerPool.Instance.RentApplier();
			}

			public void Update(float delta)
			{
				ModifierController.Update(delta);
				ModifierApplierController.Update(delta);
			}

			public float TakeDamage(float damage, IUnit source)
			{
				float oldHealth = Health;
				Health -= damage;

				float dealtDamage = oldHealth - Health;

				if (Health <= 0 && !IsDead)
				{
					ModifierController.Clear();
					IsDead = true;
				}

				return dealtDamage;
			}

			public void UseHealth(float value)
			{
				Health -= value;
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