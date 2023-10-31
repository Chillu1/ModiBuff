using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class TargetingTests : ModifierTests
	{
		[Test]
		public void SelfInit_Damage()
		{
			Setup();

			Unit.AddModifierSelf("InitDamage"); //Init

			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void TargetInit_Damage()
		{
			Setup();

			Enemy.AddModifierTarget("InitDamage", Unit); //Init

			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void InitSelfHeal_DamageTarget()
		{
			AddRecipe("InitSelfHeal_DamageTarget")
				.Effect(new HealEffect(5, targeting: Targeting.SourceTarget), EffectOn.Init)
				.Effect(new DamageEffect(5), EffectOn.Init);
			Setup();

			Unit.TakeDamage(5, Unit);
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
			Unit.AddModifierTarget("InitSelfHeal_DamageTarget", Enemy);

			Assert.AreEqual(UnitHealth, Unit.Health);
			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
		}
	}
}