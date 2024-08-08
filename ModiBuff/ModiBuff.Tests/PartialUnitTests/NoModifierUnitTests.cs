using System.Collections.Generic;
using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class NoModifierUnitTests : PartialUnitModifierTests<NoModifierUnitTests.NoModifierUnit>
	{
		protected override void SetupUnitFactory() =>
			UnitFactory = (health, damage, heal, mana, type, tag) => new NoModifierUnit(health, type);

		public sealed class NoModifierUnit : IUnit, Core.Units.Interfaces.NonGeneric.IDamagable,
			IUnitEntity, IKillable
		{
			public UnitTag UnitTag { get; }
			public UnitType UnitType { get; }

			public float Health { get; private set; }
			public float MaxHealth { get; }
			public bool IsDead { get; private set; }

			public NoModifierUnit(float health, UnitType unitType = UnitType.Good)
			{
				UnitType = unitType;
				UnitTag = UnitTag.Default;
				MaxHealth = Health = health;
			}

			public float TakeDamage(float damage, IUnit source)
			{
				float oldHealth = Health;
				Health -= damage;

				float dealtDamage = oldHealth - Health;

				if (Health <= 0 && !IsDead)
					IsDead = true;

				return dealtDamage;
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