using ModiBuff.Core;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class ConditionTests : BaseModifierTests
	{
		[Test]
		public void HealthCondition_OnApply_InitDamage()
		{
			var recipe = Recipes.GetRecipe("InitDamage_ApplyCondition_HealthAbove100");

			Unit.TakeDamage(UnitHealth - 6, Unit); //6hp left

			Unit.AddApplierModifier(recipe, ApplierType.Cast);
			Unit.TryCast(recipe.Id, Unit);
			Assert.AreEqual(UnitHealth - UnitHealth + 6, Unit.Health);
		}

		[Test]
		public void ManaCondition_OnApply_InitDamage()
		{
			var recipe = Recipes.GetRecipe("InitDamage_ApplyCondition_ManaBelow100");

			Unit.AddApplierModifier(recipe, ApplierType.Cast);
			Unit.TryCast(recipe.Id, Unit);
			Assert.AreEqual(UnitHealth, Unit.Health);

			Unit.UseMana(UnitMana - 100); //100 mana left

			Unit.AddApplierModifier(recipe, ApplierType.Cast);
			Unit.TryCast(recipe.Id, Unit);
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void HealthCondition_OnEffect_InitDamage()
		{
			Unit.TryAddModifierSelf("InitDamage_EffectCondition_HealthAbove100");
			Assert.AreEqual(UnitHealth - 5, Unit.Health); //995

			Unit.TakeDamage(UnitHealth - 6, Unit); //1000-6=994 => 1 hp left
			Unit.TryAddModifierSelf("InitDamage_EffectCondition_HealthAbove100");
			Assert.AreEqual(1, Unit.Health); //Still 1hp left
		}

		[Test]
		public void HealthIsFullCondition_OnEffect_InitDamage()
		{
			Unit.TryAddModifierSelf("InitDamage_EffectCondition_HealthFull");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Unit.TryAddModifierSelf("InitDamage_EffectCondition_HealthFull");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void HealthIsFullCondition_OnApply_InitDamage()
		{
			Unit.TryAddModifierSelf("InitDamage_EffectCondition_HealthFull");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Unit.TryAddModifierSelf("InitDamage_EffectCondition_HealthFull");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void ManaIsFullCondition_OnEffect_InitDamage()
		{
			Unit.TryAddModifierSelf("InitDamage_EffectCondition_ManaFull");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Unit.UseMana(5);
			Unit.TryAddModifierSelf("InitDamage_EffectCondition_ManaFull"); //Not full
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void HasModifier_OnEffect_InitDamage()
		{
			Unit.TryAddModifierSelf("InitDamage_EffectCondition_ContainsModifier");
			Assert.AreEqual(UnitHealth, Unit.Health);

			Unit.TryAddModifierSelf("Flag");
			Unit.TryAddModifierSelf("InitDamage_EffectCondition_ContainsModifier");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void HasModifier_OnApply_InitDamage()
		{
			var recipe = Recipes.GetRecipe("InitDamage_ApplyCondition_ContainsModifier");

			Unit.AddApplierModifier(recipe, ApplierType.Cast);
			Unit.TryCast(recipe.Id, Unit);
			Assert.AreEqual(UnitHealth, Unit.Health);

			Unit.TryAddModifierSelf("FlagApply");
			Unit.TryCast(recipe.Id, Unit);
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void HasStatusEffect_OnEffect_InitDamage()
		{
			Unit.TryAddModifierSelf("InitDamage_EffectCondition_FreezeStatusEffect");
			Assert.AreEqual(UnitHealth, Unit.Health);

			Unit.TryAddModifierSelf("InitFreeze");
			Unit.TryAddModifierSelf("InitDamage_EffectCondition_FreezeStatusEffect");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void HasStatusEffect_OnApply_InitDamage()
		{
			var recipe = Recipes.GetRecipe("InitDamage_ApplyCondition_FreezeStatusEffect");

			Unit.AddApplierModifier(recipe, ApplierType.Cast);
			Unit.TryCast(recipe.Id, Unit);
			Assert.AreEqual(UnitHealth, Unit.Health);

			Unit.TryAddModifierSelf("InitFreeze");
			Unit.TryCast(recipe.Id, Unit);
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void HasLegalAction_OnEffect_InitDamage()
		{
			Unit.TryAddModifierSelf("InitDamage_EffectCondition_ActLegalAction");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Unit.TryAddModifierSelf("InitFreeze");
			Unit.TryAddModifierSelf("InitDamage_EffectCondition_ActLegalAction");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void HasLegalAction_OnApply_InitDamage()
		{
			var recipe = Recipes.GetRecipe("InitDamage_ApplyCondition_ActLegalAction");

			Unit.AddApplierModifier(recipe, ApplierType.Cast);
			Unit.TryCast(recipe.Id, Unit);
			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Unit.TryAddModifierSelf("InitFreeze");
			Unit.TryCast(recipe.Id, Unit);
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void Combination_OnEffect_InitDamage()
		{
			Unit.TryAddModifierSelf("InitDamage_EffectCondition_Combination");
			Assert.AreEqual(UnitHealth, Unit.Health);

			Unit.TryAddModifierSelf("InitFreeze");
			Unit.TryAddModifierSelf("InitDamage_EffectCondition_Combination");
			Assert.AreEqual(UnitHealth, Unit.Health);

			Unit.TryAddModifierSelf("Flag");
			Unit.TryAddModifierSelf("InitDamage_EffectCondition_Combination");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void Combination_OnApply_InitDamage()
		{
			var recipe = Recipes.GetRecipe("InitDamage_ApplyCondition_Combination");

			Unit.AddApplierModifier(recipe, ApplierType.Cast);
			Unit.TryCast(recipe.Id, Unit);
			Assert.AreEqual(UnitHealth, Unit.Health);

			Unit.TryAddModifierSelf("InitFreeze");
			Unit.TryCast(recipe.Id, Unit);
			Assert.AreEqual(UnitHealth, Unit.Health);

			Unit.TryAddModifierSelf("FlagApply");
			Unit.TryCast(recipe.Id, Unit);
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		//TODO Stat is lower/higher/equal than X%
	}
}