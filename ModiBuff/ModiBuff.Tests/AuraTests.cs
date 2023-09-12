using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class AuraTests : ModifierTests
	{
		[Test]
		public void AuraInterval()
		{
			AddRecipes(
				add => add("InitAddDamageBuff")
					.OneTimeInit()
					.Effect(new AddDamageEffect(5, true), EffectOn.Init)
					//TODO standardized aura time & aura effects should always be refreshable
					.Remove(1.05f).Refresh(),
				add => add("InitAddDamageBuff_Interval")
					.Aura()
					.Interval(1)
					.Effect(new ApplierEffect("InitAddDamageBuff"), EffectOn.Interval)
			);

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
			AddRecipes(
				add => add("InitAddDamageBuff")
					.OneTimeInit()
					.Effect(new AddDamageEffect(5, true), EffectOn.Init)
					//TODO standardized aura time & aura effects should always be refreshable
					.Remove(1.05f).Refresh(),
				add => add("InitAddDamageBuff_Interval")
					.Aura()
					.Interval(1)
					.Effect(new ApplierEffect("InitAddDamageBuff"), EffectOn.Interval)
			);

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
			AddRecipes(
				add => add("InitAddDamageBuff")
					.OneTimeInit()
					.Effect(new AddDamageEffect(5, true), EffectOn.Init)
					//TODO standardized aura time & aura effects should always be refreshable
					.Remove(1.05f).Refresh(),
				add => add("InitAddDamageBuff_Interval")
					.Aura()
					.Interval(1)
					.Effect(new ApplierEffect("InitAddDamageBuff"), EffectOn.Interval)
			);

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
			AddRecipes(
				add => add("InitAddDamageBuff")
					.OneTimeInit()
					.Effect(new AddDamageEffect(5, true), EffectOn.Init)
					//TODO standardized aura time & aura effects should always be refreshable
					.Remove(1.05f).Refresh(),
				add => add("InitAddDamageBuff_Interval")
					.Aura()
					.Interval(1)
					.Effect(new ApplierEffect("InitAddDamageBuff"), EffectOn.Interval)
			);

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