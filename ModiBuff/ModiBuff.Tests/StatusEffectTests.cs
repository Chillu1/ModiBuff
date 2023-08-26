using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class StatusEffectTests : ModifierTests
	{
		[Test]
		public void Stun()
		{
			Unit.AddModifierSelf("InitStun");

			Assert.True(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Stun));
		}

		[Test]
		public void Stun_CantAttack()
		{
			Unit.AddModifierSelf("InitStun");

			Assert.True(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Stun));

			Unit.Attack(Enemy);
			Assert.AreEqual(EnemyHealth, Enemy.Health);
		}

		[Test]
		public void Disarm_CantAttack()
		{
			Unit.AddModifierSelf("InitDisarm");

			Assert.True(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Disarm));

			Unit.Attack(Enemy);
			Assert.AreEqual(EnemyHealth, Enemy.Health);
		}

		[Test]
		public void Silence_CantCast()
		{
			var recipe = Recipes.GetRecipe("InitDamage");

			Unit.AddApplierModifier(recipe, ApplierType.Cast);
			Unit.AddModifierSelf("InitSilence");

			Assert.True(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Silence));

			Unit.TryCast(recipe.Id, Enemy);
			Assert.AreEqual(EnemyHealth, Enemy.Health);
		}

		[Test]
		public void Silence_CanAct()
		{
			Unit.AddModifierSelf("InitSilence");

			Unit.Attack(Enemy);
			Assert.AreEqual(EnemyHealth - UnitDamage, Enemy.Health);
		}

		[Test]
		public void Stun_DurationOver()
		{
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
			int modifierId = IdManager.GetId("InitStun_Revertible");
			Unit.AddModifierSelf("InitStun_Revertible");

			Assert.True(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Stun));
			Assert.False(Unit.StatusEffectController.HasLegalAction(LegalAction.Act));

			Unit.Update(1f); //Should trigger remove => revert

			Assert.False(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Stun));
			Assert.True(Unit.StatusEffectController.HasLegalAction(LegalAction.Act));
		}
	}
}