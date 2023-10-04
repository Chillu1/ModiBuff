using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public class ReactCallbackTests : ModifierTests
	{
		[Test]
		public void AddDamageAbove5RemoveDamageBelow5React_Manual()
		{
			AddGenerator("AddDamageAbove5RemoveDamageBelow5React", (id, genId, name) =>
			{
				var effect = new AddDamageEffect(5, true, true);
				var @event = new DamageChangedEvent((unit, newDamage, deltaDamage) =>
				{
					if (newDamage > 9)
						effect.Effect(unit, unit);
					else
						effect.RevertEffect(unit, unit);
				});

				var registerReactEffect = new ReactCallbackRegisterEffect<ReactType>(
					new ReactCallback<ReactType>(ReactType.DamageChanged, @event));
				var initComponent = new InitComponent(false, new IEffect[] { registerReactEffect }, null);
				return new Modifier(id, genId, name, initComponent, null, default(StackComponent), null,
					new SingleTargetComponent(), null);
			}, new ModifierAddData(true, false, false, false));
			Setup();

			Unit.AddModifierSelf("AddDamageAbove5RemoveDamageBelow5React"); //Starts with 10 baseDmg, adds 5 from effect
			Assert.AreEqual(UnitDamage + 5, Unit.Damage);
			Unit.Update(0);

			//Remove 6 damage, should remove the effect, making it 15 - 6 - 5 = 4
			Unit.AddDamage(-6); //Revert
			Assert.AreEqual(UnitDamage - 6, Unit.Damage);

			Unit.Update(0);
			Unit.AddDamage(6);
			Assert.AreEqual(UnitDamage + 5, Unit.Damage);
		}

		//TODO Finish recipe react callbacks
		/*[Test]
		public void AddDamageAbove5RemoveDamageBelow5React()
		{
			AddRecipe("AddDamageAbove5RemoveDamageBelow5React")
				.Effect(new AddDamageEffect(5, true, true), EffectOn.ReactCallback)
				.ReactCallback(ReactType.DamageChanged, (IRevertEffect effectReference) => new DamageChangedEvent((unit, newDamage, deltaDamage) =>
				{
					if (newDamage > 9)
						effectReference.Effect(unit, unit);
					else
						effectReference.RevertEffect(unit, unit);
				}));
			Setup();

			Unit.AddModifierSelf("AddDamageAbove5RemoveDamageBelow5React"); //Starts with 10 baseDmg, adds 5 from effect
			Assert.AreEqual(UnitDamage + 5, Unit.Damage);
			Unit.Update(0);

			//Remove 6 damage, should remove the effect, making it 15 - 6 - 5 = 4
			Unit.AddDamage(-6); //Revert
			Assert.AreEqual(UnitDamage - 6, Unit.Damage);

			Unit.Update(0);
			Unit.AddDamage(6);
			Assert.AreEqual(UnitDamage + 5, Unit.Damage);
		}*/

		[Test]
		public void InitStatusEffectSleep_RemoveOnTenDamageTaken_Manual()
		{
			AddGenerator("InitStatusEffectSleep_RemoveOnTenDamageTaken", (id, genId, name) =>
			{
				var effect = new StatusEffectEffect(StatusEffectType.Sleep, 5f, true);
				effect.SetModifierId(id);
				effect.SetGenId(genId);
				var removeEffect = new RemoveEffect(id, genId);
				removeEffect.SetRevertibleEffects(new IRevertEffect[] { effect });
				float totalDamageTaken = 0f;
				var @event = new HealthChangedEvent((unit, health, deltaHealth) =>
				{
					totalDamageTaken += deltaHealth;
					if (totalDamageTaken >= 10)
						removeEffect.Effect(unit, unit);
				});

				var registerReactEffect = new ReactCallbackRegisterEffect<ReactType>(
					new ReactCallback<ReactType>(ReactType.CurrentHealthChanged, @event));
				var initComponent = new InitComponent(false, new IEffect[] { effect, registerReactEffect }, null);
				return new Modifier(id, genId, name, initComponent, null, default(StackComponent), null,
					new SingleTargetComponent(), null);
			}, new ModifierAddData(true, false, false, false));
			Setup();

			Unit.AddModifierSelf("InitStatusEffectSleep_RemoveOnTenDamageTaken"); //Starts with 10 baseDmg, adds 5 from effect
			Assert.True(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Sleep));
			Unit.Update(4); //Still has sleep

			Unit.TakeDamage(9, Unit); //Still has sleep
			Assert.True(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Sleep));

			Unit.TakeDamage(2, Unit); //Removes and reverts sleep
			Assert.False(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Sleep));
		}
	}
}