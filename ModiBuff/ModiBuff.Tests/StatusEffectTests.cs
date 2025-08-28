using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class StatusEffectTests : ModifierTests
	{
		private readonly RecipeAddFunc _initStun = add => add("InitStun")
			.Effect(new StatusEffectEffect(StatusEffectType.Stun, 2), EffectOn.Init);

		[Test]
		public void Stun()
		{
			AddRecipe(_initStun);
			Setup();

			Unit.AddModifierSelf("InitStun");

			Assert.True(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Stun));
		}

		[Test]
		public void Stun_CantAttack()
		{
			AddRecipe(_initStun);
			Setup();

			Unit.AddModifierSelf("InitStun");

			Assert.True(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Stun));

			Unit.Attack(Enemy);
			Assert.AreEqual(EnemyHealth, Enemy.Health);
		}

		[Test]
		public void Disarm_CantAttack()
		{
			AddRecipe("InitDisarm")
				.Effect(new StatusEffectEffect(StatusEffectType.Disarm, 2), EffectOn.Init);
			Setup();

			Unit.AddModifierSelf("InitDisarm");

			Assert.True(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Disarm));

			Unit.Attack(Enemy);
			Assert.AreEqual(EnemyHealth, Enemy.Health);
		}

		[Test]
		public void Silence_CantCast()
		{
			AddRecipe("InitSilence")
				.Effect(new StatusEffectEffect(StatusEffectType.Silence, 2), EffectOn.Init);
			Setup();

			var generator = Recipes.GetGenerator("InitDamage");

			Unit.AddApplierModifier(generator, ApplierType.Cast);
			Unit.AddModifierSelf("InitSilence");

			Assert.True(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Silence));

			Unit.TryCast(generator.Id, Enemy);
			Assert.AreEqual(EnemyHealth, Enemy.Health);
		}

		[Test]
		public void Silence_CanAct()
		{
			AddRecipe("InitSilence")
				.Effect(new StatusEffectEffect(StatusEffectType.Silence, 2), EffectOn.Init);
			Setup();

			Unit.AddModifierSelf("InitSilence");

			Unit.Attack(Enemy);
			Assert.AreEqual(EnemyHealth - UnitDamage, Enemy.Health);
		}

		[Test]
		public void Stun_DurationOver()
		{
			AddRecipe(_initStun);
			Setup();

			Unit.AddModifierSelf("InitStun");

			Unit.Update(1f);
			Assert.True(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Stun));
			Unit.Update(1f);
			Assert.False(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Stun));

			Unit.Attack(Enemy);
			Assert.AreEqual(EnemyHealth - UnitDamage, Enemy.Health);
		}

		[Test]
		public void Stun_OverwriteDuration()
		{
			AddRecipe(_initStun);
			Setup();

			Unit.AddModifierSelf("InitStun");

			Unit.Update(1f);
			Assert.True(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Stun));
			Unit.AddModifierSelf("InitStun");
			Unit.Update(1f);
			Assert.True(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Stun));
			Unit.Update(1f);
			Assert.False(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Stun));
		}

		[Test]
		public void Stun_OverwriteDuration_DifferentStatus()
		{
			AddRecipe(_initStun);
			AddRecipe("InitDisarm")
				.Effect(new StatusEffectEffect(StatusEffectType.Disarm, 2), EffectOn.Init);
			Setup();

			Unit.AddModifierSelf("InitStun");

			Unit.Update(1f);
			Assert.True(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Stun));
			Unit.AddModifierSelf("InitDisarm");
			Unit.Update(1f);
			Assert.False(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Stun));
			Assert.True(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Disarm));
			Unit.Update(1f);
			Assert.False(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Disarm));
		}

		[Test]
		public void Stun_DontOverwriteDuration()
		{
			AddRecipe(_initStun);
			AddRecipe("InitShortStun")
				.Effect(new StatusEffectEffect(StatusEffectType.Stun, 1), EffectOn.Init);
			Setup();

			Unit.AddModifierSelf("InitStun");

			Unit.AddModifierSelf("InitShortStun");

			Unit.Update(1f);
			Assert.True(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Stun));
			Unit.Update(1f);
			Assert.False(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Stun));
		}

		[Test]
		public void Disarm_DontOverwriteDuration_SeparateStatusEffect()
		{
			AddRecipe("InitDisarm")
				.Effect(new StatusEffectEffect(StatusEffectType.Disarm, 2), EffectOn.Init);
			AddRecipe("InitShortFreeze")
				.Effect(new StatusEffectEffect(StatusEffectType.Freeze, 1), EffectOn.Init);
			Setup();

			Unit.AddModifierSelf("InitDisarm");
			Unit.AddModifierSelf("InitShortFreeze");

			Assert.True(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Disarm));
			Assert.True(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Freeze));
			Assert.False(Unit.StatusEffectController.HasLegalAction(LegalAction.Act));

			Unit.Update(1f);

			Assert.True(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Disarm));
			Assert.False(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Freeze));
			Assert.False(Unit.StatusEffectController.HasLegalAction(LegalAction.Act));

			Unit.Update(1f);

			Assert.True(Unit.StatusEffectController.HasLegalAction(LegalAction.Act));
		}

		[Test]
		public void ShortStun_LongDisarm_CantAct_CanMove()
		{
			AddRecipe("InitDisarm")
				.Effect(new StatusEffectEffect(StatusEffectType.Disarm, 2), EffectOn.Init);
			AddRecipe("InitShortStun")
				.Effect(new StatusEffectEffect(StatusEffectType.Stun, 1), EffectOn.Init);
			Setup();

			Unit.AddModifierSelf("InitShortStun");
			Unit.AddModifierSelf("InitDisarm");

			Assert.False(Unit.StatusEffectController.HasLegalAction(LegalAction.Act));
			Assert.False(Unit.StatusEffectController.HasLegalAction(LegalAction.Move));

			Unit.Update(1f);

			Assert.False(Unit.StatusEffectController.HasLegalAction(LegalAction.Act));
			Assert.True(Unit.StatusEffectController.HasLegalAction(LegalAction.Move));
		}

		[Test]
		public void Stun_Revert()
		{
			AddRecipe("InitStun_Revertible")
				.Effect(new StatusEffectEffect(StatusEffectType.Stun, 2, true), EffectOn.Init)
				.Remove(1);
			Setup();

			Unit.AddModifierSelf("InitStun_Revertible");

			Assert.True(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Stun));
			Assert.False(Unit.StatusEffectController.HasLegalAction(LegalAction.Act));

			Unit.Update(1f); //Should trigger remove => revert

			Assert.False(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Stun));
			Assert.True(Unit.StatusEffectController.HasLegalAction(LegalAction.Act));
		}

		[Test]
		public void StunTwice_RevertLonger_StillStunned()
		{
			AddRecipe(_initStun);
			AddRecipe("InitStunLong_Revertible")
				.Effect(new StatusEffectEffect(StatusEffectType.Stun, 3, true), EffectOn.Init)
				.Remove(1);
			Setup();

			Unit.AddModifierSelf("InitStunLong_Revertible"); //3s
			Unit.AddModifierSelf("InitStun"); //2s

			Assert.True(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Stun));
			Assert.False(Unit.StatusEffectController.HasLegalAction(LegalAction.Act));

			Unit.Update(1f); //Should trigger remove => revert, "InitStun" should still be at 1s

			Assert.True(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Stun));
			Assert.False(Unit.StatusEffectController.HasLegalAction(LegalAction.Act));
		}

		[Test]
		public void StunSilence_RevertStun_StillCantCast()
		{
			AddRecipe("InitStunLong_Revertible")
				.Effect(new StatusEffectEffect(StatusEffectType.Stun, 3, true), EffectOn.Init)
				.Remove(1);
			AddRecipe("InitSilence")
				.Effect(new StatusEffectEffect(StatusEffectType.Silence, 2), EffectOn.Init);
			Setup();

			Unit.AddModifierSelf("InitStunLong_Revertible"); //2s
			Unit.AddModifierSelf("InitSilence"); //2s

			Assert.True(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Stun));
			Assert.True(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Silence));
			Assert.False(Unit.StatusEffectController.HasLegalAction(LegalAction.Act));
			Assert.False(Unit.StatusEffectController.HasLegalAction(LegalAction.Cast));

			Unit.Update(1f); //Should trigger remove => revert, "InitSilence_Revertible" should still be at 1s

			Assert.False(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Stun));
			Assert.True(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Silence));
			Assert.True(Unit.StatusEffectController.HasLegalAction(LegalAction.Act));
			Assert.False(Unit.StatusEffectController.HasLegalAction(LegalAction.Cast));
		}

		[Test]
		public void StunTwice_SameIdDifferentGenId_RevertFirst()
		{
			AddRecipe("InitStunInstanceStackable_Revertible")
				.InstanceStackable()
				.Effect(new StatusEffectEffect(StatusEffectType.Stun, 3, true), EffectOn.Init)
				.Remove(2);
			Setup();

			int id = IdManager.GetId("InitStunInstanceStackable_Revertible").Value;
			Ally.AddApplierModifierNew(id, ApplierType.Cast);

			Unit.AddModifierSelf("InitStunInstanceStackable_Revertible"); //3s
			Assert.True(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Stun));
			Unit.Update(1);
			Ally.TryCast(id, Unit); //3s 2nd instance, 2s 1st instance
			Assert.True(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Stun));

			Unit.Update(1); //First instance removed & reverted, second instance at 2s
			Assert.True(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Stun));

			Unit.Update(1); //Second instance removed & reverted
			Assert.False(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Stun));
		}

		[Test]
		public void DurationLessStatusEffect_DurationOver()
		{
			AddRecipe("InitDurationLessStun")
				.Effect(new DurationLessStatusEffectEffect(StatusEffectType.Stun), EffectOn.Init)
				.Remove(2);
			Setup();

			Unit.AddModifierSelf("InitDurationLessStun");

			Unit.Update(1f);
			Assert.True(Unit.HasStatusEffectDurationLess(StatusEffectType.Stun));
			Unit.Update(1f);
			Assert.False(Unit.HasStatusEffectDurationLess(StatusEffectType.Stun));

			Unit.Attack(Enemy);
			Assert.AreEqual(EnemyHealth - UnitDamage, Enemy.Health);
		}

		//[Test]
		public void Stun_StackAddTime()
		{
			//TODO We need to sync remove timer with status effect timer
			AddRecipe("InitStunStackAddTime")
				.Stack(WhenStackEffect.Always)
				.Effect(new StatusEffectEffect(StatusEffectType.Stun, 2, true,
					StackEffectType.Effect | StackEffectType.Add, 1), EffectOn.Stack)
				.Remove(2).Refresh();
			Setup();

			Unit.AddModifierSelf("InitStunStackAddTime");
			Assert.True(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Stun));

			Unit.Update(2.5f);
			Assert.True(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Stun));

			Unit.Update(0.5f);
			Assert.False(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Stun));
		}
	}
}