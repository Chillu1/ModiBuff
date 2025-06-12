using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class CooldownTests : ModifierTests
	{
		[Test]
		public void InitDamage_Cooldown()
		{
			AddRecipe("InitDamage_Cooldown")
				.ApplyCooldown(1)
				.Effect(new DamageEffect(5), EffectOn.Init);
			Setup();

			Unit.AddApplierModifier(Recipes.GetGenerator("InitDamage_Cooldown"), ApplierType.Attack);

			Unit.Attack(Enemy);

			Assert.AreEqual(EnemyHealth - UnitDamage - 5, Enemy.Health);

			// 1 second cooldown
			Unit.Attack(Enemy);
			Assert.AreEqual(EnemyHealth - UnitDamage * 2 - 5, Enemy.Health);

			Unit.Update(1); //Cooldown gone
			Unit.Attack(Enemy);
			Assert.AreEqual(EnemyHealth - UnitDamage * 3 - 5 * 2, Enemy.Health);
		}

		[Test]
		public void InitDamage_Cooldown_Effect()
		{
			AddRecipe("InitDamage_Cooldown_Effect")
				.EffectCooldown(1)
				.Effect(new DamageEffect(5), EffectOn.Init);
			Setup();

			Unit.AddModifierSelf("InitDamage_Cooldown_Effect"); // 1 second cooldown
			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Unit.AddModifierSelf("InitDamage_Cooldown_Effect"); // On Cooldown
			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Unit.Update(1); //Cooldown gone
			Unit.AddModifierSelf("InitDamage_Cooldown_Effect");
			Assert.AreEqual(UnitHealth - 5 * 2, Unit.Health);
		}

		[Test]
		public void InitDamage_Cooldown_Pool()
		{
			AddRecipe("InitDamage_Cooldown_Pool")
				.EffectCooldown(1)
				.Effect(new DamageEffect(5), EffectOn.Init);
			Setup();

			int id = IdManager.GetId("InitDamage_Cooldown_Pool").Value;
			Pool.Clear();
			Pool.Allocate(id, 1);

			Unit.AddModifierSelf("InitDamage_Cooldown_Pool"); // 1 second cooldown
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
			Unit.ModifierController.Remove(new ModifierReference(id)); //State reset, back in pool, no cooldown

			Unit.AddModifierSelf("InitDamage_Cooldown_Pool"); // No cooldown
			Assert.AreEqual(UnitHealth - 5 - 5, Unit.Health);
		}

		//TODO To recipe, we could hold the state inside ModifierCheck, and feed it to our checks?
		//This way our checks don't need to hold mutable state, and the state can be managed outside.
		[Test]
		public void InitDamage_CooldownLowerWhenStunned_Manual()
		{
			AddGenerator("InitDamage_Cooldown", (id, genId, name, tag) =>
			{
				var cooldownCheck = new CooldownCheck(1);
				var check = new ModifierCheck(id, null, new IUpdatableCheck[] { cooldownCheck },
					new INoUnitCheck[] { cooldownCheck }, null, null, new IStateCheck[] { cooldownCheck });

				bool multiplierApplied = false;
				var callback = new CallbackRegisterEffect<CallbackType>(
					new Callback<CallbackType>(CallbackType.StatusEffectAdded, new AddStatusEffectEvent(
						(target, source, duration, statusEffect, oldLegalAction, newLegalAction) =>
						{
							if (statusEffect.HasStatusEffect(StatusEffectType.Stun) && !multiplierApplied)
							{
								multiplierApplied = true;
								cooldownCheck.SetMultiplier(2f);
							}
						})),
					new Callback<CallbackType>(CallbackType.StatusEffectRemoved, new RemoveStatusEffectEvent(
						(target, source, statusEffect, oldLegalAction, newLegalAction) =>
						{
							if (statusEffect.HasStatusEffect(StatusEffectType.Stun) && multiplierApplied)
							{
								multiplierApplied = false;
								cooldownCheck.SetMultiplier(1f);
							}
						})
					)
				);

				var damageEffect = new DamageEffect(5);
				var initComponent = new InitComponent(false, new IEffect[] { callback, damageEffect }, check);

				return new Modifier(id, genId, name, initComponent, null, null, check, new SingleTargetComponent(),
					null, null, null);
			});
			Setup();

			Unit.AddModifierSelf("InitDamage_Cooldown"); // 1 second cooldown
			Unit.AddModifierSelf("InitDamage_Cooldown");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Unit.Update(1); //Cooldown gone
			Unit.AddModifierSelf("InitDamage_Cooldown");
			Assert.AreEqual(UnitHealth - 5 * 2, Unit.Health);

			Unit.ChangeStatusEffect(StatusEffectType.Stun, 1f, Unit); //Multiplier = 2
			Unit.Update(0.5f); //Cooldown gone

			Unit.AddModifierSelf("InitDamage_Cooldown");
			Assert.AreEqual(UnitHealth - 5 * 3, Unit.Health);

			for (int i = 0; i < 11; i++)
				Unit.Update(0.05f);
			//Stun gone => Multiplier = 1
			Unit.AddModifierSelf("InitDamage_Cooldown");
			Assert.AreEqual(UnitHealth - 5 * 4, Unit.Health);

			Unit.Update(1f);
			Unit.AddModifierSelf("InitDamage_Cooldown");
			Assert.AreEqual(UnitHealth - 5 * 5, Unit.Health);
		}

		[Test]
		public void MultipleEffectsTwoEffectOnCooldownCheck()
		{
			AddRecipe("MultipleEffectsCooldownCheck")
				.EffectCooldown(1)
				.Effect(new DamageEffect(5), EffectOn.Init)
				.Stack(WhenStackEffect.Always)
				.Effect(new DamageEffect(5), EffectOn.Stack);
			Setup();

			Unit.AddModifierSelf("MultipleEffectsCooldownCheck");
			Assert.AreEqual(UnitHealth - 5 - 5, Unit.Health);
		}

		[Test]
		public void InitDamage_ChargesCooldown()
		{
			AddRecipe("InitDamage_Cooldown")
				.ApplyChargesCooldown(1, 2)
				.Effect(new DamageEffect(5), EffectOn.Init);
			Setup();

			Unit.AddApplierModifier(Recipes.GetGenerator("InitDamage_Cooldown"), ApplierType.Cast);

			Unit.TryCast("InitDamage_Cooldown", Enemy);

			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);

			// 1 more charge
			Unit.TryCast("InitDamage_Cooldown", Enemy);
			Assert.AreEqual(EnemyHealth - 5 - 5, Enemy.Health);

			// 0 charges
			Unit.TryCast("InitDamage_Cooldown", Enemy);
			Assert.AreEqual(EnemyHealth - 5 - 5, Enemy.Health);

			Unit.Update(1);
			Unit.Update(1);
			Unit.TryCast("InitDamage_Cooldown", Enemy);
			Unit.TryCast("InitDamage_Cooldown", Enemy);
			Assert.AreEqual(EnemyHealth - 5 - 5 - 5 - 5, Enemy.Health);
		}

		[Test]
		public void InitDamage_ChargesCooldownUseWait()
		{
			AddRecipe("InitDamage_Cooldown")
				.ApplyChargesCooldown(1, 2)
				.Effect(new DamageEffect(5), EffectOn.Init);
			Setup();

			Unit.AddApplierModifier(Recipes.GetGenerator("InitDamage_Cooldown"), ApplierType.Cast);

			Unit.TryCast("InitDamage_Cooldown", Enemy);

			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);

			Unit.Update(1); // 2 charges
			Unit.TryCast("InitDamage_Cooldown", Enemy);
			Assert.AreEqual(EnemyHealth - 5 - 5, Enemy.Health);

			// 1 charge
			Unit.TryCast("InitDamage_Cooldown", Enemy);
			Assert.AreEqual(EnemyHealth - 5 - 5 - 5, Enemy.Health);

			Unit.TryCast("InitDamage_Cooldown", Enemy);
			Assert.AreEqual(EnemyHealth - 5 - 5 - 5, Enemy.Health);
		}
	}
}