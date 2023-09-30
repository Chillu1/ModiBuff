using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class ChanceTests : ModifierTests
	{
		[Test]
		public void Random_InitDamage()
		{
			AddRecipe("ChanceInitDamage")
				.ApplyChance(0.5f)
				.Effect(new DamageEffect(5), EffectOn.Init);
			Setup();

			Unit.AddApplierModifier(Recipes.GetGenerator("ChanceInitDamage"), ApplierType.Attack);

			for (int i = 0; i < 50; i++)
				Unit.Attack(Enemy);

			float totalDamage = EnemyHealth - Enemy.Health;
			float averageDamage = totalDamage / 50;

			Assert.That(averageDamage, Is.InRange(10f, 15f));
		}

		[Test]
		public void Random_InitDamage_Effect()
		{
			AddRecipe("ChanceEffectInitDamage")
				.EffectChance(0.5f)
				.Effect(new DamageEffect(5), EffectOn.Init);
			Setup();

			for (int i = 0; i < 50; i++)
				Unit.AddModifierSelf("ChanceEffectInitDamage");

			float totalDamage = UnitHealth - Unit.Health;
			float averageDamage = totalDamage / 50;

			Assert.That(averageDamage, Is.InRange(1f, 4f));
		}

		[Test]
		public void Random_IntervalDamage_Effect()
		{
			AddRecipe("ChanceEffectIntervalDamage")
				.EffectChance(0.5f)
				.Interval(1)
				.Effect(new DamageEffect(5), EffectOn.Interval);
			Setup();

			Unit.AddModifierSelf("ChanceEffectIntervalDamage");

			for (int i = 0; i < 50; i++)
				Unit.Update(1f);

			float totalDamage = UnitHealth - Unit.Health;
			float averageDamage = totalDamage / 50;

			Assert.That(averageDamage, Is.InRange(1f, 4f));
		}

		//[Test]
		public void Random_DurationDamage_Effect()
		{
			AddRecipe("ChanceEffectDurationDamage")
				.EffectChance(0.5f)
				.Effect(new DamageEffect(5), EffectOn.Duration)
				.Remove(1);
			Setup();

			for (int i = 0; i < 50; i++)
			{
				Unit.AddModifierSelf("ChanceEffectDurationDamage");
				Unit.Update(1f);
			}

			float totalDamage = UnitHealth - Unit.Health;
			float averageDamage = totalDamage / 50;

			Assert.That(averageDamage, Is.InRange(1f, 4f));
		}

		[Test]
		public void Random_StackDamage_Effect()
		{
			AddRecipe("ChanceEffectStackDamage")
				.EffectChance(0.5f)
				.Effect(new DamageEffect(5), EffectOn.Stack)
				.Stack(WhenStackEffect.Always);
			Setup();

			for (int i = 0; i < 50; i++)
				Unit.AddModifierSelf("ChanceEffectStackDamage");

			float totalDamage = UnitHealth - Unit.Health;
			float averageDamage = totalDamage / 50;

			Assert.That(averageDamage, Is.InRange(1f, 4f));
		}
	}
}