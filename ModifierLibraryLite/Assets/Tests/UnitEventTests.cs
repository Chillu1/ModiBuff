using ModifierLibraryLite.Core;
using NUnit.Framework;

namespace ModifierLibraryLite.Tests
{
	public sealed class UnitEventTests : BaseModifierTests
	{
		[Test]
		public void Thorns_OnHit()
		{
			Unit.AddEffectEvent(new DamageEffect(5), EffectOnEvent.OnHit, TargetType.Acter);

			Enemy.Attack(Unit);

			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
		}

		//[Test]
		public void Thorns_OnHit_Remove()
		{
			var effect = new DamageEffect(5);
			Unit.AddEffectEvent(effect, EffectOnEvent.OnHit, TargetType.Acter);

			Enemy.Attack(Unit);

			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);

			Unit.RemoveEffectEvent(effect, EffectOnEvent.OnHit, TargetType.Acter);
			Enemy.Attack(Unit);
			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
		}

		//TODO Remove thorns event on hit

		//TODO Add extra damage event on hit, remove after duration
	}
}