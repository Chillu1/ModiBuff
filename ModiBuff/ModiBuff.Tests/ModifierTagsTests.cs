using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class ModifierTagsTests : ModifierTests
	{
		[Test]
		public void StatusResistanceTag_DurationIgnores()
		{
			AddRecipe("DurationDamageIgnoresStatusResistance")
				.Tag(Core.Units.TagType.DurationIgnoresStatusResistance)
				.Effect(new DamageEffect(5f), EffectOn.Duration)
				.Duration(1f);
			Setup();

			Unit.ChangeStatusResistance(0.5f);
			Unit.AddModifierSelf("DurationDamageIgnoresStatusResistance");

			Unit.Update(0.6f);
			Assert.AreEqual(UnitHealth, Unit.Health);
		}

		[Test]
		public void StatusResistanceTag_Duration()
		{
			AddRecipe("DurationDamage")
				.Effect(new DamageEffect(5f), EffectOn.Duration)
				.Duration(1f);
			Setup();

			Unit.ChangeStatusResistance(0.5f);
			Unit.AddModifierSelf("DurationDamage");

			Unit.Update(0.6f);
			Assert.AreEqual(UnitHealth - 5f, Unit.Health);
		}

		[Test]
		public void StatusResistanceTag_IntervalIgnores_DurationDoesnt()
		{
			AddRecipe("IntervalDamageDurationRemove")
				.Tag(Core.Units.TagType.IntervalIgnoresStatusResistance)
				.Interval(1f)
				.Effect(new DamageEffect(5f), EffectOn.Interval)
				.Remove(5f);
			Setup();

			Unit.ChangeStatusResistance(0.5f);
			Unit.AddModifierSelf("IntervalDamageDurationRemove");

			Unit.Update(1f);
			Assert.AreEqual(UnitHealth - 5f, Unit.Health);
			Unit.Update(1f);
			Assert.AreEqual(UnitHealth - 5f * 2f, Unit.Health);

			Unit.Update(0.6f);
			Assert.AreEqual(UnitHealth - 5f * 2f, Unit.Health);
			Assert.False(Unit.ContainsModifier("IntervalDamageDurationRemove"));
		}

		[Test]
		public void CastInitDamageOnAlly_EnemyOnlyLegalTarget()
		{
			AddRecipe("InitDamageEnemyOnly")
				.LegalTarget(LegalTarget.Enemy)
				.Effect(new DamageEffect(5f), EffectOn.Init);
			Setup();

			Unit.AddApplierModifier(Recipes.GetGenerator("InitDamageEnemyOnly"), ApplierType.Cast);
			Unit.TryCast(IdManager.GetId("InitDamageEnemyOnly"), Enemy);
			Assert.AreEqual(EnemyHealth - 5f, Enemy.Health);

			Unit.TryCast(IdManager.GetId("InitDamageEnemyOnly"), Ally);
			Assert.AreEqual(AllyHealth, Ally.Health);
		}

		[Test]
		public void CastInitAddDamageOnSelf_SelfOnlyLegalTarget()
		{
			AddRecipe("InitAddDamageSelfOnly")
				.LegalTarget(LegalTarget.Self)
				.Effect(new AddDamageEffect(5f), EffectOn.Init);
			Setup();

			Unit.AddApplierModifier(Recipes.GetGenerator("InitAddDamageSelfOnly"), ApplierType.Cast);
			Unit.TryCast(IdManager.GetId("InitAddDamageSelfOnly"), Ally);
			Assert.AreEqual(AllyDamage, Ally.Damage);
			Unit.TryCast(IdManager.GetId("InitAddDamageSelfOnly"), Enemy);
			Assert.AreEqual(EnemyDamage, Enemy.Damage);

			Unit.TryCast(IdManager.GetId("InitAddDamageSelfOnly"), Unit);
			Assert.AreEqual(UnitDamage + 5f, Unit.Damage);
		}
	}
}