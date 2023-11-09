using System.Collections.Generic;
using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class DestroyableProjectileTests :
		PartialUnitModifierTests<DestroyableProjectileTests.DestroyableProjectile>
	{
		private const int HitsToDestroy = 2;

		protected override void SetupUnitFactory() => UnitFactory = (health, damage, heal, mana, type, tag) =>
			new DestroyableProjectile(HitsToDestroy, type);

		public sealed class DestroyableProjectile : IUnit, IModifierOwner, IDamagable<int, int, float, float>,
			IUpdatable, IUnitEntity, IStatusEffectOwner<LegalAction, StatusEffectType>, IMovable<Vector2>, IKillable
		{
			public UnitTag UnitTag { get; }
			public UnitType UnitType { get; }

			public int Health { get; private set; }
			public int MaxHealth { get; }
			public bool IsDead { get; private set; }

			public Vector2 Position { get; private set; }
			private readonly Vector2 _velocity = new Vector2(1, 0);

			public ModifierController ModifierController { get; }

			//TODO Projectile can't be slept
			public IMultiInstanceStatusEffectController<LegalAction, StatusEffectType> StatusEffectController { get; }

			public DestroyableProjectile(int health, UnitType unitType = UnitType.Good)
			{
				UnitType = unitType;
				UnitTag = UnitTag.Default;
				MaxHealth = Health = health;

				ModifierController = ModifierControllerPool.Instance.Rent();
				StatusEffectController = new MultiInstanceStatusEffectController(this, new List<StatusEffectEvent>(),
					new List<StatusEffectEvent>());
			}

			public void Update(float delta)
			{
				ModifierController.Update(delta);
				StatusEffectController.Update(delta);

				Move(_velocity * delta);
			}

			public void Move(Vector2 velocity)
			{
				if (!StatusEffectController.HasLegalAction(LegalAction.Move))
					return;

				Position += velocity;
			}

			public float TakeDamage(float damage, IUnit source)
			{
				Health -= 1;

				if (Health <= 0 && !IsDead)
				{
					ModifierControllerPool.Instance.Return(ModifierController);
					IsDead = true;
				}

				//Up to us to decide if we deal "1" damage or our own full damage
				return 1;
			}
		}

		[Test]
		public void DestroyProjectileOnXHits()
		{
			Setup();

			Enemy.AddApplierModifier(Recipes.GetGenerator("InitDamage"), ApplierType.Attack);

			Enemy.Attack(Unit);
			Assert.AreEqual(HitsToDestroy - 2, Unit.Health);
			Assert.True(Unit.IsDead);
		}

		[Test]
		public void TryHealProjectile()
		{
			AddRecipe("InitHeal")
				.Effect(new HealEffect(5), EffectOn.Init);
			Setup();

			Enemy.Attack(Unit);
			Assert.AreEqual(HitsToDestroy - 1, Unit.Health);
			Unit.AddModifierSelf("InitHeal");
			Assert.AreEqual(HitsToDestroy - 1, Unit.Health);
			Enemy.Attack(Unit);
			Assert.AreEqual(HitsToDestroy - 2, Unit.Health);
			Assert.True(Unit.IsDead);
		}

		[Test]
		public void FreezeProjectile()
		{
			AddRecipe("Freeze")
				.Effect(new StatusEffectEffect(StatusEffectType.Freeze, 1f), EffectOn.Init);
			Setup();

			Unit.Update(1f);
			AssertExtensions.AreEqual(new Vector2(1, 0), Unit.Position);

			Unit.AddModifierSelf("Freeze");
			for (int i = 0; i < 100; i++)
				Unit.Update(0.01f);
			AssertExtensions.AreEqual(new Vector2(1, 0), Unit.Position);

			Unit.Update(1f);
			AssertExtensions.AreEqual(new Vector2(2, 0), Unit.Position);
		}
	}
}