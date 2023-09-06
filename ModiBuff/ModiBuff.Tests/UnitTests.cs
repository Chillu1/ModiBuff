using System;
using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class UnitTests : ModifierTests
	{
		[Test]
		public void DamageDamagableUnit()
		{
			var unit = new Unit();
			float unitHealth = unit.Health;

			unit.AddModifierSelf("InitDamage");
			Assert.AreEqual(unitHealth - 5, unit.Health);
		}

		private sealed class NonDamagableUnit : IUnit, IModifierOwner
		{
			public ModifierController ModifierController { get; }

			public NonDamagableUnit()
			{
				ModifierController = new ModifierController(this);
			}
		}

		[Test]
		public void DamageNonDamagableUnit()
		{
#if !DEBUG
			Assert.Ignore("This test is only for debug mode");
#endif

			var unit = new NonDamagableUnit();

			Assert.Throws<ArgumentException>(() => unit.AddModifierSelf("InitDamage"));
		}

		[Test]
		public void UnitDeath_ModifiersPooled()
		{
			Pool.Clear();
			Pool.SetMaxPoolSize(3);
			Pool.Allocate(IdManager.GetId("InitDamage"), 3);
			Pool.Allocate(IdManager.GetId("InitDamage_ApplyCondition_HealthAbove100"), 1);

			var unit = new Unit();

			unit.AddModifierSelf("InitDamage");
			unit.AddApplierModifier(Recipes.GetGenerator("InitDamage"), ApplierType.Attack);
			unit.AddApplierModifier(Recipes.GetGenerator("InitDamage"), ApplierType.Cast);
			unit.AddApplierModifier(Recipes.GetGenerator("InitDamage_ApplyCondition_HealthAbove100"), ApplierType.Attack);

			unit.TakeDamage(unit.Health, unit); //Unit dies, all modifiers should be returned to pool

			Assert.Throws<Exception>(() => Pool.Allocate(IdManager.GetId("InitDamage"), 1));

			Pool.SetMaxPoolSize(Config.MaxPoolSize);
		}
	}
}