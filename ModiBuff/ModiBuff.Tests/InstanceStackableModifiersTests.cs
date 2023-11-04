using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class InstanceStackableModifiersTests : ModifierTests
	{
		[Test]
		public void InstanceStackableDoT()
		{
			AddRecipe("InstanceStackableDoT")
				.InstanceStackable()
				.Interval(1)
				.Effect(new DamageEffect(5), EffectOn.Interval)
				.Remove(5);
			Setup();

			Unit.AddModifierSelf("InstanceStackableDoT");

			Unit.Update(1); //4
			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Unit.AddModifierSelf("InstanceStackableDoT");

			Unit.Update(1); //3, 4
			Assert.AreEqual(UnitHealth - 5 * 2 - 5, Unit.Health);

			Unit.Update(1); //2, 3
			Assert.AreEqual(UnitHealth - 5 * 3 - 5 * 2, Unit.Health);

			Unit.Update(1); //1, 2
			Assert.AreEqual(UnitHealth - 5 * 4 - 5 * 3, Unit.Health);

			Unit.Update(1); //0, 1
			Assert.AreEqual(UnitHealth - 5 * 5 - 5 * 4, Unit.Health);

			Unit.Update(1); //0, 0
			Assert.AreEqual(UnitHealth - 5 * 5 - 5 * 5, Unit.Health);
		}

		[Test]
		public void InstanceStackableAddDamage()
		{
			AddRecipe("InstanceStackableAddDamageRevertible")
				.InstanceStackable()
				.Effect(new AddDamageEffect(5, EffectState.IsRevertible), EffectOn.Init)
				.Remove(5);
			Setup();

			Unit.AddModifierSelf("InstanceStackableAddDamageRevertible");
			Assert.AreEqual(UnitDamage + 5, Unit.Damage);

			Unit.AddModifierSelf("InstanceStackableAddDamageRevertible");
			Assert.AreEqual(UnitDamage + 5 + 5, Unit.Damage);

			Unit.Update(5); //Removed both
			Assert.AreEqual(UnitDamage, Unit.Damage);
		}
	}
}