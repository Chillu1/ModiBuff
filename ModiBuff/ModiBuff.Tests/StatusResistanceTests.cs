using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;
using TagType = ModiBuff.Core.TagType;

namespace ModiBuff.Tests
{
	public sealed class StatusResistanceTests : ModifierTests
	{
		[Test, Ignore("Skip until Status Resistance is reworked")]
		public void Dot_NoResistance()
		{
			AddRecipe("DoTRemoveStatusResistance")
				//.RemoveTag(TagType.IntervalIgnoresStatusResistance)
				.Interval(1)
				.Effect(new DamageEffect(5), EffectOn.Interval)
				.Remove(5);
			Setup();

			Unit.AddModifierSelf("DoTRemoveStatusResistance");

			for (int i = 0; i < 6; i++)
				Unit.Update(1f);

			Assert.AreEqual(UnitHealth - 5 * 5, Unit.Health);
			Assert.False(Unit.ContainsModifier("DoTRemoveStatusResistance"));
		}

		[Ignore("Skip until Status Resistance is reworked")]
		[TestCase(0.5f)]
		[TestCase(0.25f)]
		[TestCase(0.1f)]
		public void Dot_XResistance(float resistance)
		{
			AddRecipe("DoTRemoveStatusResistance")
				//.RemoveTag(TagType.IntervalIgnoresStatusResistance)
				.Interval(1)
				.Effect(new DamageEffect(5), EffectOn.Interval)
				.Remove(5);
			Setup();

			Unit.AddModifierSelf("DoTRemoveStatusResistance");
			//Unit.ChangeStatusResistance(resistance);

			for (int i = 0; i < 6; i++)
				Unit.Update(resistance);

			Assert.AreEqual(UnitHealth - 5 * 5, Unit.Health);
			Assert.False(Unit.ContainsModifier("DoTRemoveStatusResistance"));
		}

		[Test, Ignore("Skip until Status Resistance is reworked")]
		public void Dot_StatusResistance_IntervalNotAffected()
		{
			AddRecipe("DoTRemove")
				.Interval(1)
				.Effect(new DamageEffect(5), EffectOn.Interval)
				.Remove(5);
			Setup();

			Unit.AddModifierSelf("DoTRemove");
			//Unit.ChangeStatusResistance(0.5f);

			for (int i = 0; i < 12; i++)
				Unit.Update(0.5f);

			//Activates twice, because 5 * 0.5 = 2.5, which makes it activates 2 times before being removed
			Assert.AreEqual(UnitHealth - 5 * 2, Unit.Health);
			Assert.False(Unit.ContainsModifier("DoTRemove"));
		}

		[Ignore("Skip until Status Resistance is reworked")]
		[TestCase(0.5f)]
		[TestCase(0.25f)]
		[TestCase(0.1f)]
		public void DurationXResistance(float resistance)
		{
			AddRecipe("DurationRemoveStatusResistance")
				//.RemoveTag(TagType.IntervalIgnoresStatusResistance)
				.Interval(1)
				.Effect(new DamageEffect(0), EffectOn.Interval)
				.Effect(new DamageEffect(5), EffectOn.Duration)
				.Remove(5);
			Setup();

			Unit.AddModifierSelf("DurationRemoveStatusResistance");
			//Unit.ChangeStatusResistance(resistance);

			for (int i = 0; i < 6; i++)
				Unit.Update(resistance);

			Assert.AreEqual(UnitHealth - 5, Unit.Health);
			Assert.False(Unit.ContainsModifier("DurationRemoveStatusResistance"));
		}
	}
}