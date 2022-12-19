using ModifierLibraryLite.Core;
using NUnit.Framework;

namespace ModifierLibraryLite.Tests
{
	public sealed class StatusEffectTests : BaseModifierTests
	{
		[Test]
		public void Stun()
		{
			Unit.TryAddModifierSelf("InitStun");

			Assert.True(Unit.HasStatusEffect(StatusEffectType.Stun));
		}

		[Test]
		public void Stun_CantAttack()
		{
			Unit.TryAddModifierSelf("InitStun");

			Assert.True(Unit.HasStatusEffect(StatusEffectType.Stun));

			Unit.Attack(Enemy);
			Assert.AreEqual(EnemyHealth, Enemy.Health);
		}

		[Test]
		public void Disarm_CantAttack()
		{
			Unit.TryAddModifierSelf("InitDisarm");

			Assert.True(Unit.HasStatusEffect(StatusEffectType.Disarm));

			Unit.Attack(Enemy);
			Assert.AreEqual(EnemyHealth, Enemy.Health);
		}

		[Test]
		public void Silence_CantCast()
		{
			Unit.AddApplierModifier(Recipes.GetRecipe("InitDamage"), ApplierType.Cast);
			Unit.TryAddModifierSelf("InitSilence");

			Assert.True(Unit.HasStatusEffect(StatusEffectType.Silence));

			Unit.Cast(Enemy);
			Assert.AreEqual(EnemyHealth, Enemy.Health);
		}

		[Test]
		public void Silence_CanAct()
		{
			Unit.TryAddModifierSelf("InitSilence");

			Unit.Attack(Enemy);
			Assert.AreEqual(EnemyHealth - UnitDamage, Enemy.Health);
		}

		[Test]
		public void Stun_DurationOver()
		{
			Unit.TryAddModifierSelf("InitStun");

			Unit.Update(1f);
			Assert.True(Unit.HasStatusEffect(StatusEffectType.Stun));
			Unit.Update(1f);
			Assert.False(Unit.HasStatusEffect(StatusEffectType.Stun));

			Unit.Attack(Enemy);
			Assert.AreEqual(EnemyHealth - UnitDamage, Enemy.Health);
		}

		[Test]
		public void Stun_OverwriteDuration()
		{
			Unit.TryAddModifierSelf("InitStun");

			Unit.Update(1f);
			Assert.True(Unit.HasStatusEffect(StatusEffectType.Stun));
			Unit.TryAddModifierSelf("InitStun");
			Unit.Update(1f);
			Assert.True(Unit.HasStatusEffect(StatusEffectType.Stun));
			Unit.Update(1f);
			Assert.False(Unit.HasStatusEffect(StatusEffectType.Stun));
		}

		[Test]
		public void Stun_OverwriteDuration_DifferentStatus()
		{
			Unit.TryAddModifierSelf("InitStun");

			Unit.Update(1f);
			Assert.True(Unit.HasStatusEffect(StatusEffectType.Stun));
			Unit.TryAddModifierSelf("InitDisarm");
			Unit.Update(1f);
			Assert.False(Unit.HasStatusEffect(StatusEffectType.Stun));
			Assert.True(Unit.HasStatusEffect(StatusEffectType.Disarm));
			Unit.Update(1f);
			Assert.False(Unit.HasStatusEffect(StatusEffectType.Disarm));
		}

		[Test]
		public void Stun_DontOverwriteDuration()
		{
			Unit.TryAddModifierSelf("InitStun");

			Unit.TryAddModifierSelf("InitShortStun");

			Unit.Update(1f);
			Assert.True(Unit.HasStatusEffect(StatusEffectType.Stun));
			Unit.Update(1f);
			Assert.False(Unit.HasStatusEffect(StatusEffectType.Stun));
		}

		//TODO X, duration over, tried adding a new shorter one, didn't overwrite. But added a seperate status effect type
		[Test]
		public void Disarm_DontOverwriteDuration_SeparateStatusEffect()
		{
			Unit.TryAddModifierSelf("InitDisarm");
			Unit.TryAddModifierSelf("InitShortFreeze");

			Assert.True(Unit.HasStatusEffect(StatusEffectType.Disarm));
			Assert.True(Unit.HasStatusEffect(StatusEffectType.Freeze));
			Assert.False(Unit.HasLegalAction(LegalAction.Act));

			Unit.Update(1f);

			Assert.True(Unit.HasStatusEffect(StatusEffectType.Disarm));
			Assert.False(Unit.HasStatusEffect(StatusEffectType.Freeze));
			Assert.False(Unit.HasLegalAction(LegalAction.Act));

			Unit.Update(1f);

			Assert.True(Unit.HasLegalAction(LegalAction.Act));
		}

		[Test]
		public void ShortStun_LongDisarm_CantAct_CanMove()
		{
			Unit.TryAddModifierSelf("InitShortStun");
			Unit.TryAddModifierSelf("InitDisarm");

			Assert.False(Unit.HasLegalAction(LegalAction.Act));
			Assert.False(Unit.HasLegalAction(LegalAction.Move));

			Unit.Update(1f);

			Assert.False(Unit.HasLegalAction(LegalAction.Act));
			Assert.True(Unit.HasLegalAction(LegalAction.Move));
		}
	}
}