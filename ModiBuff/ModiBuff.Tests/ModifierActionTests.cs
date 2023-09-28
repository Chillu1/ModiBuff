using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class ModifierActionTests : ModifierTests
	{
		[Test]
		public void AddDamageDurationRefreshOnStrongHit()
		{
			AddRecipes(add => add("DurationAddDamageStrongHitRefresh")
				.Effect(new AddDamageEffect(5), EffectOn.Duration)
				.ModifierAction(ModifierAction.Refresh, EffectOn.Callback)
				.Callback(CallbackType.StrongHit)
				.Duration(2).Refresh()
			);

			Unit.AddModifierSelf("DurationAddDamageStrongHitRefresh");
			Unit.Update(1);
			Assert.AreEqual(UnitDamage, Unit.Damage);

			Unit.TakeDamage(UnitHealth * 0.6f, Unit); //Refresh timer
			Unit.Update(1);
			Assert.AreEqual(UnitDamage, Unit.Damage);

			Unit.Update(1);
			Assert.AreEqual(UnitDamage + 5, Unit.Damage);
		}

		[Test]
		public void AddDamageOn2StacksResetStacksOnStrongHit()
		{
			AddRecipes(add => add("StackAddDamageStrongHitResetStacks")
				.Effect(new AddDamageEffect(5), EffectOn.Stack)
				.ModifierAction(ModifierAction.ResetStacks, EffectOn.Callback)
				.Callback(CallbackType.StrongHit)
				.Stack(WhenStackEffect.EveryXStacks, everyXStacks: 2)
			);

			Unit.AddModifierSelf("StackAddDamageStrongHitResetStacks");
			Assert.AreEqual(UnitDamage, Unit.Damage);

			Unit.TakeDamage(UnitHealth * 0.6f, Unit); //Reset stacks
			Unit.AddModifierSelf("StackAddDamageStrongHitResetStacks");
			Assert.AreEqual(UnitDamage, Unit.Damage);

			Unit.AddModifierSelf("StackAddDamageStrongHitResetStacks");
			Assert.AreEqual(UnitDamage + 5, Unit.Damage);
		}
	}
}