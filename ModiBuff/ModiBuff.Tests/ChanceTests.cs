using ModiBuff.Core;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class ChanceTests : BaseModifierTests
	{
		[Test]
		public void Random_InitDamage()
		{
			Unit.AddApplierModifier(Recipes.GetRecipe("ChanceInitDamage"), ApplierType.Attack);

			for (int i = 0; i < 50; i++)
				Unit.Attack(Enemy);

			float totalDamage = EnemyHealth - Enemy.Health;
			float averageDamage = totalDamage / 50;

			Assert.That(averageDamage, Is.InRange(10f, 15f));
		}

		[Test]
		public void Random_InitDamage_Effect()
		{
			for (int i = 0; i < 50; i++)
				Unit.TryAddModifierSelf("ChanceEffectInitDamage");

			float totalDamage = UnitHealth - Unit.Health;
			float averageDamage = totalDamage / 50;

			Assert.That(averageDamage, Is.InRange(1f, 4f));
		}

		[Test]
		public void Random_IntervalDamage_Effect()
		{
			Unit.TryAddModifierSelf("ChanceEffectIntervalDamage");

			for (int i = 0; i < 50; i++)
				Unit.Update(1f);

			float totalDamage = UnitHealth - Unit.Health;
			float averageDamage = totalDamage / 50;

			Assert.That(averageDamage, Is.InRange(1f, 4f));
		}

		//[Test]
		public void Random_DurationDamage_Effect()
		{
			for (int i = 0; i < 50; i++)
			{
				Unit.TryAddModifierSelf("ChanceEffectDurationDamage");
				Unit.Update(1f);
			}

			float totalDamage = UnitHealth - Unit.Health;
			float averageDamage = totalDamage / 50;

			Assert.That(averageDamage, Is.InRange(1f, 4f));
		}

		[Test]
		public void Random_StackDamage_Effect()
		{
			for (int i = 0; i < 50; i++)
				Unit.TryAddModifierSelf("ChanceEffectStackDamage");

			float totalDamage = UnitHealth - Unit.Health;
			float averageDamage = totalDamage / 50;

			Assert.That(averageDamage, Is.InRange(1f, 4f));
		}
	}
}