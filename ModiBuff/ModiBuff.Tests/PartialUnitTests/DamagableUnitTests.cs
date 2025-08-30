using System.Collections.Generic;
using ModiBuff.Core;
using ModiBuff.Core.Units;
using ModiBuff.Core.Units.Interfaces.NonGeneric;
using NUnit.Framework;
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

			private readonly Dictionary<ApplierType, List<(int Id, ICheck[] Checks)>> _modifierAppliers;

			public DamagableUnit(float health, UnitType unitType = UnitType.Good)
			{
				UnitType = unitType;
				UnitTag = UnitTag.Default;
				MaxHealth = Health = health;

				ModifierController = ModifierControllerPool.Instance.Rent();
				ModifierApplierController = ModifierControllerPool.Instance.RentApplier();
				_modifierAppliers = new Dictionary<ApplierType, List<(int, ICheck[])>>
				{
					{ ApplierType.Attack, new List<(int, ICheck[])>() },
					{ ApplierType.Cast, new List<(int, ICheck[])>() }
				};
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

			public void AddApplierModifierNew(int modifierId, ApplierType applierType, ICheck[] checks = null)
			{
				if (checks?.Length > 0)
				{
					if (_modifierAppliers.TryGetValue(applierType, out var list))
					{
						list.Add((modifierId, checks));
						return;
					}

					_modifierAppliers[applierType] =
						new List<(int Id, ICheck[] Checks)>(new[] { (modifierId, checks) });
					return;
				}

				_modifierAppliers[applierType].Add((modifierId, null));
			}

			public bool TryCast(int modifierId, IUnit target) => TryCastInternal(modifierId, target);

			internal bool TryCastNoChecks(int modifierId, IUnit target) => TryCastInternal(modifierId, target, true);

			private bool TryCastInternal(int modifierId, IUnit target, bool skipChecks = false)
			{
				if (!(target is IModifierOwner modifierTarget))
					return false;
				if (!modifierId.IsLegalTarget((IUnitEntity)target, this))
					return false;
				if (!_modifierAppliers.TryGetValue(ApplierType.Cast, out var appliers))
					return false;

				(int Id, ICheck[] Checks)? applier = null;
				for (int i = 0; i < appliers.Count; i++)
				{
					if (appliers[i].Id == modifierId)
					{
						applier = appliers[i];
						break;
					}
				}

				if (applier == null)
					return false;

				if (applier.Value.Checks != null && !skipChecks)
				{
					foreach (var check in applier.Value.Checks)
						if (!check.Check(this))
							return false;

					for (int i = 0; i < applier.Value.Checks.Length; i++)
						applier.Value.Checks[i].Use(this);
				}

				modifierTarget.ModifierController.Add(modifierId, modifierTarget, this);


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
				.Effect(new DamageEffect(5), EffectOn.Init);
			AddRecipe("InitDamageHealthCost")
				.Effect(new DamageEffect(5), EffectOn.Init);
			Setup();

			Unit.AddApplierModifierNew(IdManager.GetId("InitDamageManaCost").Value, ApplierType.Cast, new ICheck[]
			{
				new CostCheck(CostType.Mana, 5)
			});
			Unit.AddApplierModifierNew(IdManager.GetId("InitDamageHealthCost").Value, ApplierType.Cast, new ICheck[]
			{
				new CostCheck(CostType.Health, 5)
			});

			Unit.TryCast(IdManager.GetId("InitDamageManaCost").Value, Unit);
			Assert.AreEqual(UnitHealth, Unit.Health);

			Unit.TryCast(IdManager.GetId("InitDamageHealthCost").Value, Unit);
			Assert.AreEqual(UnitHealth - 5 - 5, Unit.Health);
		}

		[Test, Ignore("Effect checks, aka on target, not source")]
		public void InitDamageEffectCosts_UnitWithoutMana()
		{
			AddRecipe("InitDamageManaCost")
				.Effect(new DamageEffect(5), EffectOn.Init);
			AddRecipe("InitDamageHealthCost")
				.Effect(new DamageEffect(5), EffectOn.Init);
			Setup();

			Unit.AddApplierModifierNew(IdManager.GetId("InitDamageManaCost").Value, ApplierType.Cast, new ICheck[]
			{
				new CostCheck(CostType.Mana, 5)
			});
			Unit.AddApplierModifierNew(IdManager.GetId("InitDamageHealthCost").Value, ApplierType.Cast, new ICheck[]
			{
				new CostCheck(CostType.Health, 5)
			});

			Unit.TryCast(IdManager.GetId("InitDamageManaCost").Value, Unit);
			Assert.AreEqual(UnitHealth, Unit.Health);

			Unit.TryCast(IdManager.GetId("InitDamageHealthCost").Value, Unit);
			Assert.AreEqual(UnitHealth - 5 - 5, Unit.Health);
		}

		[Test]
		public void InitDamageEventCallback_UnitWithoutEvents()
		{
			AddRecipe("InitDamageWhenAttacked")
				.Effect(new DamageEffect(5), EffectOn.CallbackUnit)
				.CallbackUnit(CallbackUnitType.WhenAttacked);
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