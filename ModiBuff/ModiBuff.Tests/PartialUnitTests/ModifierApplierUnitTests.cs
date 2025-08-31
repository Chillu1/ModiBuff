using System.Collections.Generic;
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

			private readonly Dictionary<ApplierType, List<(int Id, ICheck[] Checks)>> _modifierAppliers;

			public ModifierApplierUnit(float health, float damage, UnitType unitType = UnitType.Good)
			{
				UnitType = unitType;
				UnitTag = UnitTag.Default;
				MaxHealth = Health = health;
				Damage = damage;

				_modifierAppliers = new Dictionary<ApplierType, List<(int, ICheck[])>>
				{
					{ ApplierType.Attack, new List<(int, ICheck[])>() },
					{ ApplierType.Cast, new List<(int, ICheck[])>() }
				};
			}

			public float Attack(IUnit target)
			{
				if (target is IModifierOwner modifierOwner)
				{
					foreach ((int id, ICheck[] checks) in _modifierAppliers[ApplierType.Attack])
					{
						bool checksPassed = true;
						if (checks != null)
							foreach (var check in checks)
							{
								if (!check.Check(this))
								{
									checksPassed = false;
									break;
								}
							}

						if (!checksPassed)
							continue;

						if (checks != null)
							foreach (var check in checks)
								check.Use(this);

						modifierOwner.ModifierController.Add(id, target, this);
					}
				}

				return ((IDamagable)target).TakeDamage(Damage, this);
			}

			public float TakeDamage(float damage, IUnit source)
			{
				float oldHealth = Health;
				Health -= damage;

				float dealtDamage = oldHealth - Health;

				return dealtDamage;
			}

			public bool ContainsApplier(int modifierId, ApplierType applierType)
			{
				return _modifierAppliers.TryGetValue(applierType, out var list) && list.Exists(c => c.Id == modifierId);
			}

			public bool TryApply(int modifierId, IUnit target)
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

				if (applier.Value.Checks != null)
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
		}

		[Test]
		public void TryApplyDamageAppliers_ModifierAppliersUnit()
		{
			AddRecipe("InitDamageCooldown")
				.Effect(new DamageEffect(5), EffectOn.Init);
			Setup();

			Unit.AddApplierModifierNew(IdManager.GetId("InitDamage").Value, ApplierType.Attack);
			Unit.AddApplierModifierNew(IdManager.GetId("InitDamageCooldown").Value, ApplierType.Attack, new ICheck[]
			{
				new CooldownCheck(1)
			});

			Unit.Attack(Unit);
			Assert.AreEqual(UnitHealth - UnitDamage, Unit.Health);

			Enemy.AddApplierModifierNew(IdManager.GetId("InitDamage").Value, ApplierType.Attack);
			Enemy.AddApplierModifierNew(IdManager.GetId("InitDamageCooldown").Value, ApplierType.Attack, new ICheck[]
			{
				new CooldownCheck(1)
			});

			Enemy.Attack(Unit);
			Assert.AreEqual(UnitHealth - UnitDamage - EnemyDamage, Unit.Health);
		}
	}
}