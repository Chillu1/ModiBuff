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
				.Effect(new AddDamageEffect(5, EffectState.IsRevertibleAndTogglable), EffectOn.Init)
				//TODO standardized aura time & aura effects should always be refreshable
				.Remove(1.05f).Refresh(),
			add => add("InitAddDamageBuff_Interval")
				.Aura()
				.Interval(1)
				.Effect(new ApplierEffect("InitAddDamageBuff"), EffectOn.Interval),
			add => add("InitAddDamageBuff_2")
				.Effect(new AddDamageEffect(5, EffectState.IsRevertibleAndTogglable), EffectOn.Init)
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
		//TODO Pool aura test
	}
}