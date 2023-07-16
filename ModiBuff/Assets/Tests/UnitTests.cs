using System;
using System.Text.RegularExpressions;
using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace ModiBuff.Tests
{
	public sealed class UnitTests : BaseModifierTests
	{
		[Test]
		public void DamageDamagableUnit()
		{
			var unit = new Unit();
			float unitHealth = unit.Health;

			unit.TryAddModifierSelf("InitDamage");
			Assert.AreEqual(unitHealth - 5, unit.Health);
		}

		private sealed class NonDamagableUnit : IUnit, IModifierOwner
		{
			public ModifierController ModifierController { get; }

			public NonDamagableUnit()
			{
				ModifierController = new ModifierController();
			}

			public bool TryAddModifier(int id, IUnit source)
			{
				return ModifierController.TryAdd(id, this, source);
			}
		}

		[Test]
		public void DamageNonDamagableUnit()
		{
			NonDamagableUnit unit = new NonDamagableUnit();

			Assert.Throws<ArgumentException>(() => unit.TryAddModifierSelf("InitDamage"));
		}

		[Test]
		public void UnitDeath_ModifiersPooled()
		{
			Pool.Clear();
			Pool.SetMaxPoolSize(3);
			Pool.Allocate(IdManager.GetId("InitDamage"), 3);
			Pool.Allocate(IdManager.GetId("InitDamage_ApplyCondition_HealthAbove100"), 1);

			var unit = new Unit();

			unit.TryAddModifierSelf("InitDamage");
			unit.AddApplierModifier(Recipes.GetRecipe("InitDamage"), ApplierType.Attack);
			unit.AddApplierModifier(Recipes.GetRecipe("InitDamage"), ApplierType.Cast);
			unit.AddApplierModifier(Recipes.GetRecipe("InitDamage_ApplyCondition_HealthAbove100"), ApplierType.Attack);

			unit.TakeDamage(unit.Health, unit); //Unit dies, all modifiers should be returned to pool

			LogAssert.Expect(LogType.Error, new Regex("Modifier pool for*")); //Assert that they were returned to pool
			Pool.Allocate(IdManager.GetId("InitDamage"), 1);

			Pool.SetMaxPoolSize(ModifierPool.MaxPoolSize);
		}
	}
}