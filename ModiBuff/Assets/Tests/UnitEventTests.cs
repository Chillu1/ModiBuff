using ModiBuff.Core;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class UnitEventTests : BaseModifierTests
	{
		[Test]
		public void ThornsEffect_OnHit()
		{
			Unit.AddEffectEvent(new SelfDamageEffect(5), EffectOnEvent.OnHit);

			Enemy.Attack(Unit);

			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
		}

		[Test]
		public void ThornsEffect_OnHit_Remove()
		{
			var effect = new SelfDamageEffect(5);
			Unit.AddEffectEvent(effect, EffectOnEvent.OnHit);

			Enemy.Attack(Unit);

			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);

			Unit.RemoveEffectEvent(effect, EffectOnEvent.OnHit);
			Enemy.Attack(Unit);
			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
		}

		[Test]
		public void Thorns_OnHit()
		{
			Unit.TryAddModifierSelf("ThornsOnHitEvent");

			Enemy.Attack(Unit);

			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
		}

		[Test]
		public void Thorns_OnHit_DurationRemove()
		{
			Unit.TryAddModifierSelf("ThornsOnHitEvent_Remove");

			Enemy.Attack(Unit);

			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);

			Unit.Update(5);

			Enemy.Attack(Unit);
			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
		}
	}
}