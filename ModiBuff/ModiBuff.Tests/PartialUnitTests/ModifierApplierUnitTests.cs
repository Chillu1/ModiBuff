using ModiBuff.Core;
using ModiBuff.Core.Units;
using ModiBuff.Core.Units.Interfaces.NonGeneric;
using NUnit.Framework;
using IDamagable = ModiBuff.Core.Units.Interfaces.NonGeneric.IDamagable;

namespace ModiBuff.Tests
{
	public sealed class ModifierApplierUnitTests :
		PartialUnitModifierTests<ModifierApplierUnitTests.ModifierApplierUnit>
	{
		protected override void SetupUnitFactory() =>
			UnitFactory = (health, damage, heal, mana, type, tag) => new ModifierApplierUnit(health, damage, type);

		public sealed class ModifierApplierUnit : IUnit, IModifierApplierOwner, IDamagable, IUnitEntity, IAttacker
		{
			public UnitTag UnitTag { get; }
			public UnitType UnitType { get; }

			public float Health { get; private set; }
			public float MaxHealth { get; }
			public float Damage { get; }

			public ModifierApplierController ModifierApplierController { get; }

			public ModifierApplierUnit(float health, float damage, UnitType unitType = UnitType.Good)
			{
				UnitType = unitType;
				UnitTag = UnitTag.Default;
				MaxHealth = Health = health;
				Damage = damage;

				ModifierApplierController = new ModifierApplierController();
			}

			public float Attack(IUnit target)
			{
				if (target is IModifierOwner modifierOwner)
					this.ApplyAllAttackModifier(modifierOwner);

				return ((IDamagable)target).TakeDamage(Damage, this);
			}

			public float TakeDamage(float damage, IUnit source)
			{
				float oldHealth = Health;
				Health -= damage;

				float dealtDamage = oldHealth - Health;

				return dealtDamage;
			}
		}

		[Test]
		public void TryApplyDamageAppliers_ModifierAppliersUnit()
		{
			AddRecipe("InitDamageCooldown")
				.ApplyCooldown(1)
				.Effect(new DamageEffect(5), EffectOn.Init);
			Setup();

			Unit.AddApplierModifier(Recipes.GetGenerator("InitDamage"), ApplierType.Attack);
			Unit.AddApplierModifier(Recipes.GetGenerator("InitDamageCooldown"), ApplierType.Attack);

			Unit.Attack(Unit);
			Assert.AreEqual(UnitHealth - UnitDamage, Unit.Health);

			Enemy.AddApplierModifier(Recipes.GetGenerator("InitDamage"), ApplierType.Attack);
			Enemy.AddApplierModifier(Recipes.GetGenerator("InitDamageCooldown"), ApplierType.Attack);

			Enemy.Attack(Unit);
			Assert.AreEqual(UnitHealth - UnitDamage - EnemyDamage, Unit.Health);
		}
	}
}