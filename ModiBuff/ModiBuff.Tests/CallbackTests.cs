using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class CallbackTests : ModifierTests
	{
		//[Test]
		public void Init_AddDamage_Disarmed_TriggerCallback_RemoveAndRevert()
		{
			Unit.AddModifierSelf("InitAddDamageRevertibleDisarmCallback");
			Assert.AreEqual(UnitDamage + 5, Unit.Damage);

			//Unit.StatusEffectController.ChangeStatusEffect(StatusEffectType.Disarm, 1f);

			Assert.AreEqual(UnitDamage, Unit.Damage);
		}

		[Test]
		public void Init_AddDamage_HalfHealth_TriggerCallback_RemoveAndRevert()
		{
			AddGenerators(new ManualGeneratorData("InitAddDamageRevertibleHalfHealthCallback", (id, genId, name) =>
			{
				var effect = new AddDamageEffect(5, true);
				var removeEffect = new RemoveEffect(id, genId);
				removeEffect.SetRevertibleEffects(new IRevertEffect[] { effect });
				var registerEffect = new CallbackRegisterEffect((target, source) => removeEffect.Effect(target, source));
				var initComponent = new InitComponent(false, new IEffect[] { effect, registerEffect }, null);
				//var durationComponent = new DurationComponent(5, false, new IEffect[] { removeEffect });
				return new Modifier(id, genId, name, initComponent, null, default(StackComponent), null, new SingleTargetComponent());
			}, new ModifierAddData(true, false, false, false)));

			Unit.AddModifierSelf("InitAddDamageRevertibleHalfHealthCallback");
			Assert.AreEqual(UnitDamage + 5, Unit.Damage);

			//Take enough damage to trigger the callback
			Unit.TakeDamage(UnitHealth * 0.6f, Unit);
			Unit.Update(0);
			Assert.AreEqual(UnitDamage, Unit.Damage);
		}
	}
}