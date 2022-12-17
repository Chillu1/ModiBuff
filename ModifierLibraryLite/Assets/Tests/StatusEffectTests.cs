using ModifierLibraryLite.Core;
using NUnit.Framework;

namespace ModifierLibraryLite.Tests
{
	public sealed class StatusEffectTests : BaseModifierTests
	{
		[Test]
		public void InitStun()
		{
			Unit.TryAddModifierSelf("InitStun");

			Assert.True(Unit.HasStatusEffect(StatusEffectType.Stun));
		}

		[Test]
		public void InitStun_CantAttack()
		{
			Unit.TryAddModifierSelf("InitStun");

			Assert.True(Unit.HasStatusEffect(StatusEffectType.Stun));

			Unit.Attack(Enemy);
			Assert.AreEqual(EnemyHealth, Enemy.Health);
		}

		[Test]
		public void InitDisarm_CantAttack()
		{
			Unit.TryAddModifierSelf("InitDisarm");

			Assert.True(Unit.HasStatusEffect(StatusEffectType.Disarm));

			Unit.Attack(Enemy);
			Assert.AreEqual(EnemyHealth, Enemy.Health);
		}

		[Test]
		public void InitSilence_CantCast()
		{
			Unit.AddApplierModifier(Recipes.GetRecipe("InitDamage"), ApplierType.Cast);
			Unit.TryAddModifierSelf("InitSilence");

			Assert.True(Unit.HasStatusEffect(StatusEffectType.Silence));

			Unit.Cast(Enemy);
			Assert.AreEqual(EnemyHealth, Enemy.Health);
		}

		[Test]
		public void InitSilence_CanAct()
		{
			Unit.TryAddModifierSelf("InitSilence");

			Unit.Attack(Enemy);
			Assert.AreEqual(EnemyHealth - UnitDamage, Enemy.Health);
		}

		//TODO X, duration over
		//[Test]
		public void InitStun_DurationOver()
		{
			Unit.TryAddModifierSelf("InitStun");

			Unit.Update(1f);
			Assert.True(Unit.HasStatusEffect(StatusEffectType.Stun));
			Unit.Update(1f);
			Assert.False(Unit.HasStatusEffect(StatusEffectType.Stun));

			Unit.Attack(Enemy);
			Assert.AreEqual(EnemyHealth - UnitDamage, Enemy.Health);
		}
		//TODO X, duration over, but added a new one overwrite
		//TODO X, duration over, tried adding a new shorter one, didn't overwrite
	}
}