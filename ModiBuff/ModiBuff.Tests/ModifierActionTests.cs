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
			AddRecipe("DurationAddDamageStrongHitRefresh")
				.Effect(new AddDamageEffect(5), EffectOn.Duration)
				.ModifierAction(ModifierAction.Refresh, EffectOn.Callback)
				.Callback(CallbackType.StrongHit)
				.Duration(2).Refresh();
			Setup();

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
			AddRecipe("StackAddDamageStrongHitResetStacks")
				.Effect(new AddDamageEffect(5), EffectOn.Stack)
				.ModifierAction(ModifierAction.ResetStacks, EffectOn.Callback)
				.Callback(CallbackType.StrongHit)
				.Stack(WhenStackEffect.EveryXStacks, everyXStacks: 2);
			Setup();

			Unit.AddModifierSelf("StackAddDamageStrongHitResetStacks");
			Assert.AreEqual(UnitDamage, Unit.Damage);

			Unit.TakeDamage(UnitHealth * 0.6f, Unit); //Reset stacks
			Unit.AddModifierSelf("StackAddDamageStrongHitResetStacks");
			Assert.AreEqual(UnitDamage, Unit.Damage);

			Unit.AddModifierSelf("StackAddDamageStrongHitResetStacks");
			Assert.AreEqual(UnitDamage + 5, Unit.Damage);
		}

		[Test]
		public void AddDamageStackTimers_ResetStacks_RevertEffects()
		{
			AddGenerator("AddDamageStackTimerResetStacks", (id, genId, name, tag) =>
			{
				var initComponent = new InitComponent(true,
					new IEffect[]
					{
						CallbackRegisterEffect<CallbackType>.Create(CallbackType.StrongHit,
							ModifierActionEffect.Create(id, genId, ModifierAction.ResetStacks))
					}, null);
				var stackComponent = new StackTimerComponent(WhenStackEffect.Always, 5, -1, -1, -1,
					new IStackEffect[] { new AddDamageEffect(5, true, stackEffect: StackEffectType.Effect) }, null);

				return new Modifier(id, genId, null, initComponent, null, stackComponent, null,
					new SingleTargetComponent(), null);
			});
			Setup();

			Unit.AddModifierSelf("AddDamageStackTimerResetStacks");
			Assert.AreEqual(UnitDamage + 5, Unit.Damage);
			Unit.AddModifierSelf("AddDamageStackTimerResetStacks");
			Assert.AreEqual(UnitDamage + 5 + 5, Unit.Damage);

			Unit.TakeDamage(UnitHealth * 0.6f, Unit); //Reset stacks
			Assert.AreEqual(UnitDamage, Unit.Damage);
		}
	}
}