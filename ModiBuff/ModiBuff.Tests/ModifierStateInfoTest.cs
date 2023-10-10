using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class ModifierStateInfoTest : ModifierTests
	{
		[Test]
		public void InitDamage_CorrectBaseDamage_Recipe()
		{
			Setup();

			Unit.AddModifierSelf("InitDamage");
			var state = Unit.ModifierController.GetState<DamageEffect.Data>(IdManager.GetId("InitDamage"));
			Assert.AreEqual(5, state.BaseDamage);
			Assert.AreEqual(0, state.ExtraDamage);
		}

		[Test]
		public void InitDamage_CorrectBaseDamage_Manual()
		{
			AddGenerator("InitDamageManual", (id, genId, name, tag) =>
			{
				var damageEffect = new DamageEffect(5);
				var initComponent = new InitComponent(false, new IEffect[] { damageEffect }, null);

				return new Modifier(id, genId, name, initComponent, null, null, null,
					new SingleTargetComponent(), new ModifierStateInfo(damageEffect));
			});
			Setup();

			Unit.AddModifierSelf("InitDamageManual");

			var state = Unit.ModifierController.GetState<DamageEffect.Data>(IdManager.GetId("InitDamageManual"));
			Assert.AreEqual(5, state.BaseDamage);
			Assert.AreEqual(0, state.ExtraDamage);
		}

		[Test]
		public void InitDoubleStackDamage_CorrectBaseDamage()
		{
			AddRecipe("DoubleStackDamage")
				.Interval(1)
				.Effect(new DamageEffect(5), EffectOn.Init | EffectOn.Interval)
				.Effect(new DamageEffect(10, StackEffectType.Add), EffectOn.Stack)
				.Stack(WhenStackEffect.Always, value: 2);
			Setup();

			int id = IdManager.GetId("DoubleStackDamage");
			Unit.AddModifierSelf("DoubleStackDamage");

			var firstDamageState = Unit.ModifierController.GetState<DamageEffect.Data>(id);
			Assert.AreEqual(5, firstDamageState.BaseDamage);
			Assert.AreEqual(0, firstDamageState.ExtraDamage);
			var secondDamageState = Unit.ModifierController.GetState<DamageEffect.Data>(id, stateNumber: 1);
			Assert.AreEqual(10, secondDamageState.BaseDamage);
			Assert.AreEqual(2, secondDamageState.ExtraDamage);

			Unit.AddModifierSelf("DoubleStackDamage");

			var secondUpdatedDamageState = Unit.ModifierController.GetState<DamageEffect.Data>(id, stateNumber: 1);
			Assert.AreEqual(10, secondUpdatedDamageState.BaseDamage);
			Assert.AreEqual(4, secondUpdatedDamageState.ExtraDamage);
		}

		[Test]
		public void IntervalDurationTimerReferences()
		{
			AddRecipe("IntervalDurationDamage")
				.Interval(1)
				.Duration(5)
				.Effect(new DamageEffect(5), EffectOn.Interval | EffectOn.Duration);
			Setup();

			Unit.AddModifierSelf("IntervalDurationDamage");
			int id = IdManager.GetId("IntervalDurationDamage");

			var intervalReference = Unit.ModifierController.GetTimer<IntervalComponent>(id);
			var durationReference = Unit.ModifierController.GetTimer<DurationComponent>(id);
			Assert.AreEqual(intervalReference.Time, 1);
			Assert.AreEqual(durationReference.Time, 5);
			Assert.AreEqual(0, intervalReference.Timer);
			Assert.AreEqual(0, durationReference.Timer);
			Unit.Update(0.5f);
			Assert.AreEqual(0.5f, intervalReference.Timer);
			Assert.AreEqual(0.5f, durationReference.Timer);
			Unit.Update(0.5f);
			Assert.AreEqual(0f, intervalReference.Timer);
			Assert.AreEqual(1f, durationReference.Timer);
		}

		[Test]
		public void StackReference()
		{
			AddRecipe("StackDamage")
				.Effect(new DamageEffect(5), EffectOn.Stack)
				.Stack(WhenStackEffect.Always, maxStacks: 5);
			Setup();

			int id = IdManager.GetId("StackDamage");
			Unit.AddModifierSelf("StackDamage");

			var stackReference = Unit.ModifierController.GetStackReference(id);
			Assert.AreEqual(1, stackReference.Stacks);

			Unit.AddModifierSelf("StackDamage");
			Assert.AreEqual(2, stackReference.Stacks);
			Assert.AreEqual(5, stackReference.MaxStacks);
		}
	}
}