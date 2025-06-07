using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class AddWithDataTests : ModifierTests
	{
		[Test]
		public void AddWithData()
		{
			Setup();

			IData[] data =
			{
				new EffectData<int>(3, typeof(DamageEffect), 0),
			};
			Unit.AddModifierSelfWithData("InitDamage", data);

			Assert.AreEqual(UnitHealth - 5 - 3, Unit.Health);
		}

		[Test]
		public void AddWithData_SecondEffect()
		{
			AddRecipe("InitDamageTwo")
				.Effect(new DamageEffect(5), EffectOn.Init)
				.Interval(1)
				.Effect(new DamageEffect(0), EffectOn.Interval);
			Setup();

			IData[] data =
			{
				new EffectData<int>(3, typeof(DamageEffect), 1),
			};
			Unit.AddModifierSelfWithData("InitDamageTwo", data);
			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Unit.Update(1f);
			Assert.AreEqual(UnitHealth - 5 - 3, Unit.Health);
		}

		[Test]
		public void AddWithData_Float()
		{
			Setup();

			IData[] data =
			{
				new EffectData<float>(3f, typeof(DamageEffect), 0),
			};

			Unit.AddModifierSelfWithData("InitDamage", data);
			Assert.AreEqual(UnitHealth - 5 - 3f, Unit.Health);
		}

		[Test]
		public void AddWithData_Replace()
		{
			Setup();

			Unit.AddModifierSelf("InitDamage");
			IData[] data =
			{
				new EffectData<int>(3, typeof(DamageEffect), 0),
			};
			Unit.AddModifierSelfWithData("InitDamage", data);

			Assert.AreEqual(UnitHealth - 5 - 5 - 3, Unit.Health);
		}

		[Test]
		public void AddWithData_CustomInterval()
		{
			AddRecipe("IntervalDamage")
				.Interval(1)
				.Effect(new DamageEffect(5), EffectOn.Interval);
			Setup();

			IData[] data =
			{
				new ModifierIntervalData(2f)
			};
			Unit.AddModifierSelfWithData("IntervalDamage", data);

			Unit.Update(1f);
			Assert.AreEqual(UnitHealth, Unit.Health);

			Unit.Update(1f);
			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Unit.Update(2f);
			Assert.AreEqual(UnitHealth - 5 - 5, Unit.Health);
		}

		[Test]
		public void AddWithData_CustomDuration()
		{
			AddRecipe("DurationDamage")
				.Duration(2f)
				.Effect(new DamageEffect(5), EffectOn.Duration);
			Setup();

			IData[] data =
			{
				new ModifierDurationData(3f)
			};
			Unit.AddModifierSelfWithData("DurationDamage", data);

			Unit.Update(2f);
			Assert.AreEqual(UnitHealth, Unit.Health);

			Unit.Update(1f);
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}
	}
}