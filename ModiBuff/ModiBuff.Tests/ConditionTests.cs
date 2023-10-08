using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class ConditionTests : ModifierTests
	{
		[Test]
		public void HealthCondition_OnApply_InitDamage()
		{
			AddRecipe("InitDamage_ApplyCondition_HealthAbove100")
				.ApplyCondition(StatType.Health, 100, ComparisonType.GreaterOrEqual)
				.Effect(new DamageEffect(5), EffectOn.Init);
			Setup();

			var generator = Recipes.GetGenerator("InitDamage_ApplyCondition_HealthAbove100");

			Unit.TakeDamage(UnitHealth - 6, Unit); //6hp left

			Unit.AddApplierModifier(generator, ApplierType.Cast);
			Unit.TryCast(generator.Id, Unit);
			Assert.AreEqual(UnitHealth - UnitHealth + 6, Unit.Health);
		}

		[Test]
		public void ManaCondition_OnApply_InitDamage()
		{
			AddRecipe("InitDamage_ApplyCondition_ManaBelow100")
				.ApplyCondition(StatType.Mana, 100, ComparisonType.LessOrEqual)
				.Effect(new DamageEffect(5), EffectOn.Init);
			Setup();

			var generator = Recipes.GetGenerator("InitDamage_ApplyCondition_ManaBelow100");

			Unit.AddApplierModifier(generator, ApplierType.Cast);
			Unit.TryCast(generator.Id, Unit);
			Assert.AreEqual(UnitHealth, Unit.Health);

			Unit.UseMana(UnitMana - 100); //100 mana left

			Unit.AddApplierModifier(generator, ApplierType.Cast);
			Unit.TryCast(generator.Id, Unit);
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void HealthCondition_OnEffect_InitDamage()
		{
			AddRecipe("InitDamage_EffectCondition_HealthAbove100")
				.EffectCondition(StatType.Health, 100, ComparisonType.GreaterOrEqual)
				.Effect(new DamageEffect(5), EffectOn.Init);
			Setup();

			Unit.AddModifierSelf("InitDamage_EffectCondition_HealthAbove100");
			Assert.AreEqual(UnitHealth - 5, Unit.Health); //995

			Unit.TakeDamage(UnitHealth - 6, Unit); //1000-6=994 => 1 hp left
			Unit.AddModifierSelf("InitDamage_EffectCondition_HealthAbove100");
			Assert.AreEqual(1, Unit.Health); //Still 1hp left
		}

		[Test]
		public void HealthIsFullCondition_OnEffect_InitDamage()
		{
			AddRecipe("InitDamage_EffectCondition_HealthFull")
				.EffectCondition(ConditionType.HealthIsFull)
				.Effect(new DamageEffect(5), EffectOn.Init);
			Setup();

			Unit.AddModifierSelf("InitDamage_EffectCondition_HealthFull");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Unit.AddModifierSelf("InitDamage_EffectCondition_HealthFull");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void HealthIsFullCondition_OnApply_InitDamage()
		{
			AddRecipe("InitDamage_EffectCondition_HealthFull")
				.EffectCondition(ConditionType.HealthIsFull)
				.Effect(new DamageEffect(5), EffectOn.Init);
			Setup();

			Unit.AddModifierSelf("InitDamage_EffectCondition_HealthFull");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Unit.AddModifierSelf("InitDamage_EffectCondition_HealthFull");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void ManaIsFullCondition_OnEffect_InitDamage()
		{
			AddRecipe("InitDamage_EffectCondition_ManaFull")
				.EffectCondition(ConditionType.ManaIsFull)
				.Effect(new DamageEffect(5), EffectOn.Init);
			Setup();

			Unit.AddModifierSelf("InitDamage_EffectCondition_ManaFull");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Unit.UseMana(5);
			Unit.AddModifierSelf("InitDamage_EffectCondition_ManaFull"); //Not full
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void HasModifier_OnEffect_InitDamage()
		{
			AddRecipe("Flag");
			AddRecipe("InitDamage_EffectCondition_ContainsModifier")
				.EffectCondition("Flag")
				.Effect(new DamageEffect(5), EffectOn.Init);
			Setup();

			Unit.AddModifierSelf("InitDamage_EffectCondition_ContainsModifier");
			Assert.AreEqual(UnitHealth, Unit.Health);

			Unit.AddModifierSelf("Flag");
			Unit.AddModifierSelf("InitDamage_EffectCondition_ContainsModifier");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void HasModifier_OnApply_InitDamage()
		{
			AddRecipe("FlagApply");
			AddRecipe("InitDamage_ApplyCondition_ContainsModifier")
				.ApplyCondition("FlagApply")
				.Effect(new DamageEffect(5), EffectOn.Init);
			Setup();

			var generator = Recipes.GetGenerator("InitDamage_ApplyCondition_ContainsModifier");

			Unit.AddApplierModifier(generator, ApplierType.Cast);
			Unit.TryCast(generator.Id, Unit);
			Assert.AreEqual(UnitHealth, Unit.Health);

			Unit.AddModifierSelf("FlagApply");
			Unit.TryCast(generator.Id, Unit);
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void HasStatusEffect_OnEffect_InitDamage()
		{
			AddRecipe("InitFreeze")
				.Effect(new StatusEffectEffect(StatusEffectType.Freeze, 2), EffectOn.Init);
			AddRecipe("InitDamage_EffectCondition_FreezeStatusEffect")
				.EffectCondition(StatusEffectType.Freeze)
				.Effect(new DamageEffect(5), EffectOn.Init);
			AddRecipe("InitDamage_EffectCondition_ActLegalAction")
				.EffectCondition(LegalAction.Act)
				.Effect(new DamageEffect(5), EffectOn.Init);
			Setup();

			Unit.AddModifierSelf("InitDamage_EffectCondition_FreezeStatusEffect");
			Assert.AreEqual(UnitHealth, Unit.Health);

			Unit.AddModifierSelf("InitFreeze");
			Unit.AddModifierSelf("InitDamage_EffectCondition_FreezeStatusEffect");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void HasStatusEffect_OnApply_InitDamage()
		{
			AddRecipe("InitFreeze")
				.Effect(new StatusEffectEffect(StatusEffectType.Freeze, 2), EffectOn.Init);
			AddRecipe("InitDamage_ApplyCondition_FreezeStatusEffect")
				.ApplyCondition(StatusEffectType.Freeze)
				.Effect(new DamageEffect(5), EffectOn.Init);
			AddRecipe("InitDamage_ApplyCondition_ActLegalAction")
				.ApplyCondition(LegalAction.Act)
				.Effect(new DamageEffect(5), EffectOn.Init);
			Setup();

			var generator = Recipes.GetGenerator("InitDamage_ApplyCondition_FreezeStatusEffect");

			Unit.AddApplierModifier(generator, ApplierType.Cast);
			Unit.TryCast(generator.Id, Unit);
			Assert.AreEqual(UnitHealth, Unit.Health);

			Unit.AddModifierSelf("InitFreeze");
			Unit.TryCast(generator.Id, Unit);
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void HasLegalAction_OnEffect_InitDamage()
		{
			AddRecipe("InitFreeze")
				.Effect(new StatusEffectEffect(StatusEffectType.Freeze, 2), EffectOn.Init);
			AddRecipe("InitDamage_EffectCondition_FreezeStatusEffect")
				.EffectCondition(StatusEffectType.Freeze)
				.Effect(new DamageEffect(5), EffectOn.Init);
			AddRecipe("InitDamage_EffectCondition_ActLegalAction")
				.EffectCondition(LegalAction.Act)
				.Effect(new DamageEffect(5), EffectOn.Init);
			Setup();

			Unit.AddModifierSelf("InitDamage_EffectCondition_ActLegalAction");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Unit.AddModifierSelf("InitFreeze");
			Unit.AddModifierSelf("InitDamage_EffectCondition_ActLegalAction");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void HasLegalAction_OnApply_InitDamage()
		{
			AddRecipe("InitFreeze")
				.Effect(new StatusEffectEffect(StatusEffectType.Freeze, 2), EffectOn.Init);
			AddRecipe("InitDamage_ApplyCondition_ActLegalAction")
				.ApplyCondition(LegalAction.Act)
				.Effect(new DamageEffect(5), EffectOn.Init);
			Setup();

			var generator = Recipes.GetGenerator("InitDamage_ApplyCondition_ActLegalAction");

			Unit.AddApplierModifier(generator, ApplierType.Cast);
			Unit.TryCast(generator.Id, Unit);
			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Unit.AddModifierSelf("InitFreeze");
			Unit.TryCast(generator.Id, Unit);
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void Combination_OnEffect_InitDamage()
		{
			AddRecipe("Flag");
			AddRecipe("InitFreeze")
				.Effect(new StatusEffectEffect(StatusEffectType.Freeze, 2), EffectOn.Init);
			AddRecipe("InitDamage_EffectCondition_Combination")
				.EffectCondition("Flag")
				.EffectCondition(StatusEffectType.Freeze)
				.Effect(new DamageEffect(5), EffectOn.Init);
			Setup();

			Unit.AddModifierSelf("InitDamage_EffectCondition_Combination");
			Assert.AreEqual(UnitHealth, Unit.Health);

			Unit.AddModifierSelf("InitFreeze");
			Unit.AddModifierSelf("InitDamage_EffectCondition_Combination");
			Assert.AreEqual(UnitHealth, Unit.Health);

			Unit.AddModifierSelf("Flag");
			Unit.AddModifierSelf("InitDamage_EffectCondition_Combination");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void Combination_OnApply_InitDamage()
		{
			AddRecipe("FlagApply");
			AddRecipe("InitFreeze")
				.Effect(new StatusEffectEffect(StatusEffectType.Freeze, 2), EffectOn.Init);
			AddRecipe("InitDamage_ApplyCondition_Combination")
				.ApplyCondition("FlagApply")
				.ApplyCondition(StatusEffectType.Freeze)
				.Effect(new DamageEffect(5), EffectOn.Init);
			Setup();

			var generator = Recipes.GetGenerator("InitDamage_ApplyCondition_Combination");

			Unit.AddApplierModifier(generator, ApplierType.Cast);
			Unit.TryCast(generator.Id, Unit);
			Assert.AreEqual(UnitHealth, Unit.Health);

			Unit.AddModifierSelf("InitFreeze");
			Unit.TryCast(generator.Id, Unit);
			Assert.AreEqual(UnitHealth, Unit.Health);

			Unit.AddModifierSelf("FlagApply");
			Unit.TryCast(generator.Id, Unit);
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		//TODO Stat is lower/higher/equal than X%
	}
}