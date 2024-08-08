using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class CallbackUnitTests : ModifierTests
	{
		[Test]
		public void Init_AddDamage_HalfHealth_TriggerCallback_RemoveAndRevert()
		{
			AddRecipe("InitAddDamageRevertibleHalfHealthCallback")
				.Effect(new AddDamageEffect(5, EffectState.IsRevertible), EffectOn.Init)
				.Remove(RemoveEffectOn.CallbackUnit)
				.CallbackUnit(CallbackUnitType.StrongHit);
			Setup();

			Unit.AddModifierSelf("InitAddDamageRevertibleHalfHealthCallback");
			Assert.AreEqual(UnitDamage + 5, Unit.Damage);

			//Take enough damage to trigger the callback
			Unit.TakeDamage(UnitHealth * 0.6f, Unit);
			Assert.AreEqual(UnitDamage, Unit.Damage);
		}

		[Test]
		public void Init_RegisterCallbackHealToFullWhenTakingStrongHit_Effect()
		{
			AddRecipe("InitHealToFullHalfHealthCallback")
				.Effect(new HealEffect(0).SetMetaEffects(new AddValueBasedOnStatDiffMetaEffect(StatType.MaxHealth)),
					EffectOn.CallbackUnit)
				.CallbackUnit(CallbackUnitType.StrongHit);
			Setup();

			Unit.AddModifierSelf("InitHealToFullHalfHealthCallback");
			Assert.AreEqual(UnitHealth, Unit.Health);

			//Take enough damage to trigger the callback
			Unit.TakeDamage(UnitHealth * 0.6f, Unit); //Takes 60% of max hp damage, triggers callback, heals to full
			Assert.AreEqual(UnitHealth, Unit.Health);
		}


		[Test]
		public void Init_RegisterCallbackHeal10WhenTakingStrongHit_Effect()
		{
			AddRecipe("InitHeal10HalfHealthCallback")
				.Effect(new HealEffect(10), EffectOn.CallbackUnit)
				.CallbackUnit(CallbackUnitType.StrongHit);
			Setup();

			Unit.AddModifierSelf("InitHeal10HalfHealthCallback");
			Assert.AreEqual(UnitHealth, Unit.Health);

			//Take enough damage to trigger the callback
			float damage = UnitHealth * 0.6f;
			Unit.TakeDamage(damage, Unit); //Takes 60% of max hp damage, triggers callback

			Assert.AreEqual(UnitHealth - damage + 10, Unit.Health);
		}

		[Test]
		public void Init_RegisterCallbackHealTenWhenTakingStrongHit_ThenTakeFiveDamage_Effect()
		{
			AddRecipe("InitHealDamageWhenStrongHitCallback")
				.Effect(new HealEffect(10), EffectOn.CallbackUnit)
				.Effect(new DamageEffect(5), EffectOn.CallbackUnit)
				.CallbackUnit(CallbackUnitType.StrongHit);
			Setup();

			Unit.AddModifierSelf("InitHealDamageWhenStrongHitCallback");
			Assert.AreEqual(UnitHealth, Unit.Health);

			//Take enough damage to trigger the callback
			float damage = UnitHealth * 0.6f;
			Unit.TakeDamage(damage, Unit); //Takes 60% of max hp damage, triggers callbacks

			Assert.AreEqual(UnitHealth - damage + 10 - 5, Unit.Health);
		}

		[Test]
		public void Init_InstanceCheck_Effect()
		{
			AddRecipe("InitHealDamageWhenStrongHitCallback")
				.Effect(new HealEffect(10), EffectOn.CallbackUnit)
				.Effect(new DamageEffect(5), EffectOn.CallbackUnit)
				.CallbackUnit(CallbackUnitType.StrongHit);
			Setup();

			Unit.AddModifierSelf("InitHealDamageWhenStrongHitCallback");
			Enemy.AddModifierSelf("InitHealDamageWhenStrongHitCallback");

			float enemyDamage = EnemyHealth * 0.6f;
			Enemy.TakeDamage(enemyDamage, Enemy); //Takes 60% of max hp damage, triggers callbacks
			Assert.AreEqual(EnemyHealth - enemyDamage + 10 - 5, Enemy.Health);

			Assert.AreEqual(UnitHealth, Unit.Health);

			//Take enough damage to trigger the callback
			float unitDamage = UnitHealth * 0.6f;
			Unit.TakeDamage(unitDamage, Unit); //Takes 60% of max hp damage, triggers callbacks

			Assert.AreEqual(UnitHealth - unitDamage + 10 - 5, Unit.Health);
		}

		[Test]
		public void Init_RegisterCallbackHeal10WhenTakingStrongHitRevert()
		{
			AddRecipe("InitHealToFullHalfHealthCallback")
				.Effect(new HealEffect(10), EffectOn.CallbackUnit)
				.CallbackUnit(CallbackUnitType.StrongHit)
				.Remove(1);
			Setup();

			Unit.AddModifierSelf("InitHealToFullHalfHealthCallback");
			Assert.AreEqual(UnitHealth, Unit.Health);

			//Take enough damage to trigger the callback
			float damage = UnitHealth * 0.6f;
			Unit.TakeDamage(damage, Unit); //Takes 60% of max hp damage, triggers callback

			Assert.AreEqual(UnitHealth - damage + 10, Unit.Health);

			//Heal to full
			Unit.Heal(UnitHealth, Unit);

			Unit.Update(1); //Removes the modifier => reverts the register effect => removes the callback
			Unit.TakeDamage(damage, Unit);
			Assert.AreEqual(UnitHealth - damage, Unit.Health);
		}

		[Test]
		public void Init_AddDamage_HalfHealth_TriggerCallback_RemoveAndRevert_Twice()
		{
			AddRecipe("InitAddDamageRevertibleHalfHealthCallback")
				.Effect(new AddDamageEffect(5, EffectState.IsRevertible), EffectOn.Init)
				.Remove(RemoveEffectOn.CallbackUnit)
				.CallbackUnit(CallbackUnitType.StrongHit);
			Setup();

			Unit.AddModifierSelf("InitAddDamageRevertibleHalfHealthCallback");
			Unit.AddModifierSelf("InitAddDamageRevertibleHalfHealthCallback");
			Assert.AreEqual(UnitDamage + 5 + 5, Unit.Damage);

			//Take enough damage to trigger the callback
			Unit.TakeDamage(UnitHealth * 0.6f, Unit);
			Assert.AreEqual(UnitDamage, Unit.Damage);

			Unit.TakeDamage(UnitHealth * 0.6f, Unit);
			Assert.AreEqual(UnitDamage, Unit.Damage);
		}

		[Test]
		public void MultipleUnitCallbacks()
		{
			AddRecipe("AddDamageUnitCallbacksRemoveOnStrongDispel")
				.Effect(new AddDamageEffect(1, EffectState.IsRevertible), EffectOn.CallbackUnit)
				.CallbackUnit(CallbackUnitType.WhenAttacked)
				.Effect(new AddDamageEffect(2, EffectState.IsRevertible), EffectOn.CallbackUnit2)
				.CallbackUnit(CallbackUnitType.WhenHealed)
				.Effect(new AddDamageEffect(3, EffectState.IsRevertible, targeting: Targeting.SourceTarget),
					EffectOn.CallbackUnit3)
				.CallbackUnit(CallbackUnitType.OnAttack)
				.Remove(RemoveEffectOn.CallbackUnit4)
				.CallbackUnit(CallbackUnitType.StrongDispel);
			Setup();

			Unit.AddModifierSelf("AddDamageUnitCallbacksRemoveOnStrongDispel");
			Enemy.Attack(Unit);
			Assert.AreEqual(UnitDamage + 1, Unit.Damage);
			Unit.Heal(0, Unit);
			Assert.AreEqual(UnitDamage + 1 + 2, Unit.Damage);
			Unit.Attack(Enemy);
			Assert.AreEqual(UnitDamage + 1 + 2 + 3, Unit.Damage);
			Unit.StrongDispel(Unit);
			Assert.AreEqual(UnitDamage, Unit.Damage);
		}
	}
}