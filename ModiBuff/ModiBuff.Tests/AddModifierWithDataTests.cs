using System;
using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class AddModifierWithDataTests : ModifierTests
	{
		[Test]
		public void AddWithData()
		{
			Setup();

			IData[] data =
			{
				new EffectData<int>(3, new ValueTuple<Type, int>(typeof(DamageEffect), 0)),
			};
			Unit.AddModifierWithDataSelf("InitDamage", data);

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
				new EffectData<int>(3, new ValueTuple<Type, int>(typeof(DamageEffect), 1)),
			};
			Unit.AddModifierWithDataSelf("InitDamageTwo", data);
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
				new EffectData<float>(3f, new ValueTuple<Type, int>(typeof(DamageEffect), 0)),
			};

			Unit.AddModifierWithDataSelf("InitDamage", data);
			Assert.AreEqual(UnitHealth - 5 - 3f, Unit.Health);
		}

		[Test]
		public void AddWithData_Replace()
		{
			Setup();

			Unit.AddModifierSelf("InitDamage");
			IData[] data =
			{
				new EffectData<int>(3, new ValueTuple<Type, int>(typeof(DamageEffect), 0)),
			};
			Unit.AddModifierWithDataSelf("InitDamage", data);

			Assert.AreEqual(UnitHealth - 5 - 5 - 3, Unit.Health);
		}

		[Test]
		public void AddWithData_UnspecifiedEffect()
		{
			Setup();

			IData[] data =
			{
				new EffectData<int>(3),
			};
			Unit.AddModifierWithDataSelf("InitDamage", data);

			Assert.AreEqual(UnitHealth - 5 - 3, Unit.Health);
		}

		[Test]
		public void AddWithData_UnspecifiedEffects()
		{
			AddRecipe("InitDamageIntervalHeal")
				.Effect(new DamageEffect(5), EffectOn.Init)
				.Interval(1)
				.Effect(new HealEffect(3), EffectOn.Interval);
			Setup();

			IData[] data =
			{
				new EffectData<int>(3),
			};
			Unit.AddModifierWithDataSelf("InitDamageIntervalHeal", data);
			Assert.AreEqual(UnitHealth - 5 - 3, Unit.Health);

			Unit.Update(1f);
			Assert.AreEqual(UnitHealth - 5 - 3 + 3 + 3, Unit.Health);
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
			Unit.AddModifierWithDataSelf("IntervalDamage", data);

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
			Unit.AddModifierWithDataSelf("DurationDamage", data);

			Unit.Update(2f);
			Assert.AreEqual(UnitHealth, Unit.Health);

			Unit.Update(1f);
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void AddWithData_StartingStacks()
		{
			AddRecipe("StackedDamage")
				.Stack(WhenStackEffect.Always)
				.Effect(new DamageEffect(5, true, StackEffectType.Effect | StackEffectType.AddStacksBased, 5),
					EffectOn.Stack);
			Setup();

			IData[] data =
			{
				new ModifierStartingStacksData(3)
			};
			Unit.AddModifierWithDataSelf("StackedDamage", data);

			Assert.AreEqual(UnitHealth - 5 - 5 * 4, Unit.Health);
		}

		[Test]
		public void AddWithData_WrongEffect()
		{
			Setup();

			IData[] data = { new EffectData<int>(3, new ValueTuple<Type, int>(typeof(HealEffect), 0)), };

			Assert.Throws<AssertionException>(() => Unit.AddModifierWithDataSelf("InitDamage", data));
		}

		[Test]
		public void AddWithData_WrongEffectNumber()
		{
			Setup();

			IData[] data = { new EffectData<int>(3, new ValueTuple<Type, int>(typeof(DamageEffect), 1)), };

			Assert.Throws<AssertionException>(() => Unit.AddModifierWithDataSelf("InitDamage", data));
		}
	}
}