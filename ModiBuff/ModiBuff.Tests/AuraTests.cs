using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class AuraTests : ModifierTests
	{
		private readonly RecipeAddFunc[] _defaultAuraRecipeAddFuncs =
		{
			add => add("InitAddDamageBuff")
				.Effect(new AddDamageEffect(5, EffectState.IsRevertible | EffectState.IsTogglable), EffectOn.Init)
				//TODO standardized aura time & aura effects should always be refreshable
				.Remove(1.05f).Refresh(),
			add => add("InitAddDamageBuff_Interval")
				.Aura()
				.Interval(1)
				.Effect(new ApplierEffect("InitAddDamageBuff"), EffectOn.Interval),
			add => add("InitAddDamageBuff_2")
				.Effect(new AddDamageEffect(5, EffectState.IsRevertible | EffectState.IsTogglable), EffectOn.Init)
				//TODO standardized aura time & aura effects should always be refreshable
				.Remove(1.05f).Refresh(),
			add => add("InitAddDamageBuff_Interval_2")
				.Aura(id: 1)
				.Interval(1)
				.Effect(new ApplierEffect("InitAddDamageBuff_2"), EffectOn.Interval)
		};

		private void SetupAuraTest()
		{
			for (int i = 0; i < _defaultAuraRecipeAddFuncs.Length; i++)
				AddRecipe(_defaultAuraRecipeAddFuncs[i]);
			Setup();
		}

		[Test]
		public void AuraInterval()
		{
			SetupAuraTest();

			Unit.AddAuraTargets(0, Ally);
			Unit.AddModifierSelf("InitAddDamageBuff_Interval");
			Assert.AreEqual(UnitDamage, Unit.Damage);

			Unit.Update(1f);

			Assert.AreEqual(UnitDamage + 5, Unit.Damage);
			Assert.AreEqual(AllyDamage + 5, Ally.Damage);
			Assert.AreEqual(EnemyDamage, Enemy.Damage);
		}

		[Test]
		public void Aura_AddDamage_Timeout()
		{
			SetupAuraTest();

			Unit.AddAuraTargets(0, Ally);
			Unit.AddModifierSelf("InitAddDamageBuff_Interval");

			Unit.Update(1f);

			Assert.AreEqual(UnitDamage + 5, Unit.Damage);
			Assert.AreEqual(AllyDamage + 5, Ally.Damage);

			Ally.Update(1.1f);

			Assert.AreEqual(AllyDamage, Ally.Damage);
		}

		[Test]
		public void AuraAddedDamageRefresh()
		{
			SetupAuraTest();

			Unit.AddAuraTargets(0, Ally);
			Unit.AddModifierSelf("InitAddDamageBuff_Interval");

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
			SetupAuraTest();

			Unit.AddAuraTargets(0, Ally);
			Unit.AddModifierSelf("InitAddDamageBuff_Interval");

			Unit.Update(1f);

			Assert.AreEqual(AllyDamage + 5, Ally.Damage);

			Ally.Update(1.1f);

			Assert.AreEqual(AllyDamage, Ally.Damage);

			Unit.Update(1f);

			Assert.AreEqual(AllyDamage + 5, Ally.Damage);
		}

		[Test]
		public void Two_Auras_Two_Ids()
		{
			SetupAuraTest();

			Unit.AddAuraTargets(0, Ally);
			Unit.AddAuraTargets(0, Enemy);
			Unit.AddAuraTargets(1, Enemy);
			Unit.AddModifierSelf("InitAddDamageBuff_Interval");
			Unit.AddModifierSelf("InitAddDamageBuff_Interval_2");
			Assert.AreEqual(UnitDamage, Unit.Damage);

			Unit.Update(1f);

			Assert.AreEqual(UnitDamage + 5 + 5, Unit.Damage);
			Assert.AreEqual(AllyDamage + 5, Ally.Damage);
			Assert.AreEqual(EnemyDamage + 5 + 5, Enemy.Damage);
		}

		[Test]
		public void Aura_Pool_ClearTargets()
		{
			SetupAuraTest();

			int id = IdManager.GetId("InitAddDamageBuff_Interval");
			Pool.Clear();
			Pool.Allocate(id, 1);

			Unit.AddAuraTargets(0, Ally);
			Unit.AddModifierSelf("InitAddDamageBuff_Interval");
			Unit.Update(1f);

			Assert.AreEqual(AllyDamage + 5, Ally.Damage);

			Unit.ModifierController.Remove(new ModifierReference(id, -1));
			Enemy.AddModifierSelf("InitAddDamageBuff_Interval");
			Enemy.Update(1f);
			Unit.Update(1.05f);
			Ally.Update(1.05f);

			Assert.AreEqual(EnemyDamage + 5, Enemy.Damage);
			Assert.AreEqual(AllyDamage, Ally.Damage);
			Assert.AreEqual(UnitDamage, Unit.Damage);
		}
	}
}