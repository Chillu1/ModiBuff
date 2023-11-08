using ModiBuff.Core;
using ModiBuff.Core.Units;
using ModiBuff.Core.Units.Interfaces.NonGeneric;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class AttackUnitTests : PartialUnitModifierTests<AttackUnitTests.AttackUnit>
	{
		protected override void SetupUnitFactory() =>
			UnitFactory = (health, damage, heal, mana, type, tag) => new AttackUnit(damage, type);

		public sealed class AttackUnit : IUnit, IModifierOwner, IAttacker, IUpdatable, IUnitEntity
		{
			public UnitTag UnitTag { get; }
			public UnitType UnitType { get; }
			public float Damage { get; }

			public ModifierController ModifierController { get; }

			public AttackUnit(float damage, UnitType unitType = UnitType.Good)
			{
				Damage = damage;
				UnitType = unitType;
				UnitTag = UnitTag.Default;

				ModifierController = new ModifierController(this);
			}

			public void Update(float delta)
			{
				ModifierController.Update(delta);
			}

			public float Attack(IUnit target)
			{
				return ((IDamagable)target).TakeDamage(Damage, this);
			}
		}

		[Test]
		public void InitDamage_NonDamagableUnit()
		{
			AddRecipe("InitDamage_AddDamageBasedOnDamageDealt")
				.Effect(new DamageEffect(5)
					.SetPostEffects(new AddDamageOnValuePostEffect(Targeting.SourceTarget)), EffectOn.Init);
			Setup();

			Enemy.AddModifierTarget("InitDamage_AddDamageBasedOnDamageDealt", Unit);
			Assert.AreEqual(EnemyDamage, Enemy.Damage);
		}

		[Test]
		public void Attack_NotDamagableUnit()
		{
			AddRecipe("AttackAction_AddDamageBasedOnDamageDealt")
				.Effect(new AttackActionEffect()
					.SetPostEffects(new AddDamageOnValuePostEffect(Targeting.SourceTarget)), EffectOn.Init);
			Setup();

			Enemy.AddModifierTarget("AttackAction_AddDamageBasedOnDamageDealt", Unit);
			Assert.AreEqual(EnemyDamage, Enemy.Damage);
		}

		[Test]
		public void Thorns_NotDamagableUnit()
		{
			AddRecipe("AttackAction_AddDamageBasedOnDamageDealt")
				.Effect(new DamageEffect(5, targeting: Targeting.SourceTarget)
					.SetPostEffects(new AddDamageOnValuePostEffect(Targeting.TargetSource)), EffectOn.Event)
				.Event(EffectOnEvent.WhenAttacked);
			Setup();

			Enemy.AddModifierSelf("AttackAction_AddDamageBasedOnDamageDealt");

			Unit.Attack(Enemy);
			Assert.AreEqual(EnemyDamage, Enemy.Damage);
		}
	}
}