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
			AddRecipes(_initStun);

			Unit.AddModifierSelf("InitStun");

			Assert.True(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Stun));
		}

		[Test]
		public void Stun_CantAttack()
		{
			AddRecipes(_initStun);

			Unit.AddModifierSelf("InitStun");

			Assert.True(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Stun));

			Unit.Attack(Enemy);
			Assert.AreEqual(EnemyHealth, Enemy.Health);
		}

		[Test]
		public void Disarm_CantAttack()
		{
			AddRecipes(add => add("InitDisarm")
				.Effect(new StatusEffectEffect(StatusEffectType.Disarm, 2), EffectOn.Init));

			Unit.AddModifierSelf("InitDisarm");

			Assert.True(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Disarm));

			Unit.Attack(Enemy);
			Assert.AreEqual(EnemyHealth, Enemy.Health);
		}

		[Test]
		public void Silence_CantCast()
		{
			AddRecipes(add => add("InitSilence")
				.Effect(new StatusEffectEffect(StatusEffectType.Silence, 2), EffectOn.Init));

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
			AddRecipes(add => add("InitSilence")
				.Effect(new StatusEffectEffect(StatusEffectType.Silence, 2), EffectOn.Init));

			Unit.AddModifierSelf("InitSilence");

			Unit.Attack(Enemy);
			Assert.AreEqual(EnemyHealth - UnitDamage, Enemy.Health);
		}

		[Test]
		public void Stun_DurationOver()
		{
			AddRecipes(_initStun);

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
			AddRecipes(_initStun);

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
			AddRecipes(_initStun, add => add("InitDisarm")
				.Effect(new StatusEffectEffect(StatusEffectType.Disarm, 2), EffectOn.Init));

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
			AddRecipes(_initStun, add => add("InitShortStun")
				.Effect(new StatusEffectEffect(StatusEffectType.Stun, 1), EffectOn.Init));

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
			AddRecipes(
				add => add("InitDisarm")
					.Effect(new StatusEffectEffect(StatusEffectType.Disarm, 2), EffectOn.Init),
				add => add("InitShortFreeze")
					.Effect(new StatusEffectEffect(StatusEffectType.Freeze, 1), EffectOn.Init));

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
			AddRecipes(
				add => add("InitDisarm")
					.Effect(new StatusEffectEffect(StatusEffectType.Disarm, 2), EffectOn.Init),
				add => add("InitShortStun")
					.Effect(new StatusEffectEffect(StatusEffectType.Stun, 1), EffectOn.Init));

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
			AddRecipes(add => add("InitStun_Revertible")
				.Effect(new StatusEffectEffect(StatusEffectType.Stun, 2, true), EffectOn.Init)
				.Remove(1));

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
			AddRecipes(_initStun,
				add => add("InitStunLong_Revertible")
					.Effect(new StatusEffectEffect(StatusEffectType.Stun, 3, true), EffectOn.Init)
					.Remove(1));

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
			AddRecipes(
				add => add("InitStunLong_Revertible")
					.Effect(new StatusEffectEffect(StatusEffectType.Stun, 3, true), EffectOn.Init)
					.Remove(1),
				add => add("InitSilence")
					.Effect(new StatusEffectEffect(StatusEffectType.Silence, 2), EffectOn.Init));

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
	}
}