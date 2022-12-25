using ModiBuff.Core;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class UnitEventTests : BaseModifierTests
	{
		[Test]
		public void Thorns_OnHit()
		{
			Unit.AddEffectEvent(new SelfDamageEffect(5), EffectOnEvent.OnHit);

			Enemy.Attack(Unit);

			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
		}

		[Test]
		public void Thorns_OnHit_Remove()
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
		public void ThornsModifier_OnHit_Duration()
		{
			var recipe = Recipes.GetRecipe("InitDamageSelf");
			var modifier = recipe.Create();
			modifier.SetTargets(Unit, Enemy);
			Unit.AddEffectEvent(modifier, EffectOnEvent.OnHit);

			Enemy.Attack(Unit);

			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);

			Unit.RemoveEffectEvent(modifier, EffectOnEvent.OnHit);
		}

		//[Test]
		public void ThornsModifier_OnHit_DurationRemove()
		{
			var recipe = Recipes.GetRecipe("InitDamageSelfRemove");
			var modifier = recipe.Create();
			modifier.SetTargets(Unit, Enemy);
			Unit.AddEffectEvent(modifier, EffectOnEvent.OnHit);

			Enemy.Attack(Unit);

			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);

			Unit.Update(5);

			Enemy.Attack(Unit);
			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
		}
	}
}