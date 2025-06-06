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
				new EffectData<int>(12, typeof(DamageEffect), 0),
			};

			Unit.AddModifierSelfWithData("InitDamage", data);
			Assert.AreEqual(UnitHealth - 5 - 12, Unit.Health);
		}

		[Test]
		public void AddWithData_Float()
		{
			Setup();

			IData[] data =
			{
				new EffectData<float>(12f, typeof(DamageEffect), 0),
			};

			Unit.AddModifierSelfWithData("InitDamage", data);
			Assert.AreEqual(UnitHealth - 5 - 12f, Unit.Health);
		}
	}
}