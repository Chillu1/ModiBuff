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
				.Effect(new DamageEffect(5), EffectOn.Init);
			Setup();

			int id = IdManager.GetId("InitDamage_ApplyCondition_HealthAbove100").Value;

			Unit.TakeDamage(UnitHealth - 6, Unit); //6hp left

			Unit.AddApplierModifierNew(id, ApplierType.Cast, new ICheck[]
			{
				new StatCheck(StatType.Health, ComparisonType.GreaterOrEqual, 100)
			});
			Unit.TryCast(id, Unit);
			Assert.AreEqual(UnitHealth - UnitHealth + 6, Unit.Health);
		}

		[Test]
		public void ManaCondition_OnApply_InitDamage()
		{
			AddRecipe("InitDamage_ApplyCondition_ManaBelow100")
				.Effect(new DamageEffect(5), EffectOn.Init);
			Setup();

			int id = IdManager.GetId("InitDamage_ApplyCondition_ManaBelow100").Value;

			Unit.AddApplierModifierNew(id, ApplierType.Cast, new ICheck[]
			{
				new StatCheck(StatType.Mana, ComparisonType.LessOrEqual, 100)
			});
			Unit.TryCast(id, Unit);
			Assert.AreEqual(UnitHealth, Unit.Health);

			Unit.UseMana(UnitMana - 100); //100 mana left

			Unit.AddApplierModifierNew(id, ApplierType.Cast, new ICheck[]
			{
				new StatCheck(StatType.Mana, ComparisonType.LessOrEqual, 100)
			});
			Unit.TryCast(id, Unit);
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
			int flagId = AddRecipe("FlagApply").Id;
			AddRecipe("InitDamage_ApplyCondition_ContainsModifier")
				.Effect(new DamageEffect(5), EffectOn.Init);
			Setup();

			int id = IdManager.GetId("InitDamage_ApplyCondition_ContainsModifier").Value;

			Unit.AddApplierModifierNew(id, ApplierType.Cast, new ICheck[]
			{
				new ModifierIdCheck(flagId)
			});
			Unit.TryCast(id, Unit);
			Assert.AreEqual(UnitHealth, Unit.Health);

			Unit.AddModifierSelf("FlagApply");
			Unit.TryCast(id, Unit);
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
				.Effect(new DamageEffect(5), EffectOn.Init);
			AddRecipe("InitDamage_ApplyCondition_ActLegalAction")
				.Effect(new DamageEffect(5), EffectOn.Init);
			Setup();

			int id = IdManager.GetId("InitDamage_ApplyCondition_FreezeStatusEffect").Value;

			Unit.AddApplierModifierNew(id, ApplierType.Cast,
				new ICheck[] { new StatusEffectCheck(StatusEffectType.Freeze) });
			Unit.TryCast(id, Unit);
			Assert.AreEqual(UnitHealth, Unit.Health);

			Unit.AddModifierSelf("InitFreeze");
			Unit.TryCast(id, Unit);
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
				.Effect(new DamageEffect(5), EffectOn.Init);
			Setup();

			int id = IdManager.GetId("InitDamage_ApplyCondition_ActLegalAction").Value;

			Unit.AddApplierModifierNew(id, ApplierType.Cast,
				new ICheck[] { new LegalActionCheck(LegalAction.Act) });
			Unit.TryCast(id, Unit);
			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Unit.AddModifierSelf("InitFreeze");
			Unit.TryCast(id, Unit);
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
			int flagId = AddRecipe("FlagApply").Id;
			AddRecipe("InitFreeze")
				.Effect(new StatusEffectEffect(StatusEffectType.Freeze, 2), EffectOn.Init);
			AddRecipe("InitDamage_ApplyCondition_Combination")
				.Effect(new DamageEffect(5), EffectOn.Init);
			Setup();

			int id = IdManager.GetId("InitDamage_ApplyCondition_Combination").Value;

			Unit.AddApplierModifierNew(id, ApplierType.Cast, new ICheck[]
			{
				new ModifierIdCheck(flagId),
				new StatusEffectCheck(StatusEffectType.Freeze)
			});
			Unit.TryCast(id, Unit);
			Assert.AreEqual(UnitHealth, Unit.Health);

			Unit.AddModifierSelf("InitFreeze");
			Unit.TryCast(id, Unit);
			Assert.AreEqual(UnitHealth, Unit.Health);

			Unit.AddModifierSelf("FlagApply");
			Unit.TryCast(id, Unit);
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		//TODO Stat is lower/higher/equal than X%
	}
}