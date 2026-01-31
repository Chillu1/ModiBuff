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
			Setup();

			var unit = new Unit();
			float unitHealth = unit.Health;

			unit.AddModifierSelf("InitDamage");
			Assert.AreEqual(unitHealth - 5, unit.Health);
		}

		[Test]
		public void UnitDeath_ModifiersPooled()
		{
			AddRecipe("InitDamage_ApplyCondition_HealthAbove100")
				.Effect(new DamageEffect(5), EffectOn.Init);
			Setup();

			Pool.Clear();
			Pool.SetMaxPoolSize(3);
			Pool.Allocate(IdManager.GetId("InitDamage").Value, 3);
			Pool.Allocate(IdManager.GetId("InitDamage_ApplyCondition_HealthAbove100").Value, 1);

			var unit = new Unit();

			unit.AddModifierSelf("InitDamage");
			int id = IdManager.GetId("InitDamage").Value;
			unit.AddApplierModifierNew(id, ApplierType.Attack);
			unit.AddApplierModifierNew(id, ApplierType.Cast);
			unit.AddApplierModifierNew(IdManager.GetId("InitDamage_ApplyCondition_HealthAbove100").Value,
				ApplierType.Cast, new ICheck[]
				{
					new StatCheck(StatType.Health, ComparisonType.GreaterOrEqual, 100)
				});

			unit.TakeDamage(unit.Health, unit); //Unit dies, all modifiers should be returned to pool

			Assert.Throws<Exception>(() => Pool.Allocate(id, 1));

			Pool.SetMaxPoolSize(Config.MaxPoolSize);
		}
	}
}