using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class HealTests : ModifierTests
	{
		[Test]
		public void SelfInit_Heal()
		{
			AddRecipe("InitHeal")
				.Effect(new HealEffect(5), EffectOn.Init);
			Setup();

			Unit.TakeDamage(5, Unit);
			Assert.AreEqual(AllyHealth - 5, Unit.Health);

			Unit.AddModifierSelf("InitHeal"); //Init

			Assert.AreEqual(UnitHealth, Unit.Health);
		}

		[Test]
		public void TargetInit_Heal()
		{
			AddRecipe("InitHeal")
				.Effect(new HealEffect(5), EffectOn.Init);
			Setup();

			Ally.TakeDamage(5, Ally);
			Assert.AreEqual(AllyHealth - 5, Ally.Health);

			Unit.AddModifierTarget("InitHeal", Ally); //Init

			Assert.AreEqual(AllyHealth, Ally.Health);
		}
	}
}