using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class InitTests : ModifierTests
	{
		[Test]
		public void InitDamage()
		{
			Setup();

			Unit.AddModifierSelf("InitDamage");

			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void InitDamage_InitTwice_DamageTwice()
		{
			Setup();

			Unit.AddModifierSelf("InitDamage");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Unit.AddModifierSelf("InitDamage");
			Assert.AreEqual(UnitHealth - 10, Unit.Health);
		}

		[Test]
		public void OneTimeInitDamage_LingerDuration()
		{
			AddRecipe("OneTimeInitDamage_LingerDuration")
				.OneTimeInit()
				.Effect(new DamageEffect(5), EffectOn.Init)
				.Remove(1);
			Setup();

			Unit.AddModifierSelf("OneTimeInitDamage_LingerDuration");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Unit.AddModifierSelf("OneTimeInitDamage_LingerDuration");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Unit.Update(1f);
			Unit.AddModifierSelf("OneTimeInitDamage_LingerDuration");
			Assert.AreEqual(UnitHealth - 10, Unit.Health);
		}

		[Test]
		public void Init_DoT()
		{
			AddRecipe("InitDoT")
				.Interval(1)
				.Effect(new DamageEffect(10), EffectOn.Init | EffectOn.Interval)
				.Remove(5);
			Setup();

			Unit.AddModifierSelf("InitDoT"); //Init

			Assert.AreEqual(UnitHealth - 10, Unit.Health);

			Unit.Update(1);
			Assert.AreEqual(UnitHealth - 10 * 2, Unit.Health);
		}
	}
}