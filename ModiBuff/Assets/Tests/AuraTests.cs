using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class AuraTests : BaseModifierTests
	{
		[Test]
		public void AuraInterval()
		{
			Unit.AddCloseTargets(Ally);
			Unit.AddAuraModifier(IdManager.GetId("InitAddDamageBuff_Interval"));

			Assert.AreEqual(UnitDamage, Unit.Damage);

			Unit.Update(1f);

			Assert.AreEqual(UnitDamage + 5, Unit.Damage);
			Assert.AreEqual(AllyDamage + 5, Ally.Damage);
			Assert.AreEqual(EnemyDamage, Enemy.Damage);
		}

		[Test]
		public void Aura_AddDamage_Timeout()
		{
			Unit.AddCloseTargets(Ally);
			Unit.AddAuraModifier(IdManager.GetId("InitAddDamageBuff_Interval"));

			Unit.Update(1f);

			Assert.AreEqual(UnitDamage + 5, Unit.Damage);
			Assert.AreEqual(AllyDamage + 5, Ally.Damage);

			Ally.Update(1.1f);

			Assert.AreEqual(AllyDamage, Ally.Damage);
		}

		[Test]
		public void AuraAddedDamageRefresh()
		{
			Unit.AddCloseTargets(Ally);
			Unit.AddAuraModifier(IdManager.GetId("InitAddDamageBuff_Interval"));

			Unit.Update(1f);

			Assert.AreEqual(AllyDamage + 5, Ally.Damage);

			Ally.Update(0.8f);
			Unit.Update(1f);
			Ally.Update(0.8f);

			Assert.AreEqual(AllyDamage + 5, Ally.Damage);
		}

		[Test]
		public void Aura_AddDamage_Timeout_AddAgain()
		{
			Unit.AddCloseTargets(Ally);
			Unit.AddAuraModifier(IdManager.GetId("InitAddDamageBuff_Interval"));

			Unit.Update(1f);

			Assert.AreEqual(AllyDamage + 5, Ally.Damage);

			Ally.Update(1.1f);

			Assert.AreEqual(AllyDamage, Ally.Damage);

			Unit.Update(1f);

			Assert.AreEqual(AllyDamage + 5, Ally.Damage);
		}
	}
}