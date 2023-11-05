using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;
using TagType = ModiBuff.Core.Units.TagType;

namespace ModiBuff.Tests
{
	public class ReactCallbackTests : ModifierTests
	{
		[Test]
		public void AddDamageAbove5RemoveDamageBelow5React_Manual()
		{
			AddGenerator("AddDamageAbove5RemoveDamageBelow5React", (id, genId, name, tag) =>
			{
				var effect = new AddDamageEffect(5, EffectState.IsRevertibleAndTogglable);
				bool effectActive = false;
				var @event = new DamageChangedEvent((unit, newDamage, deltaDamage) =>
				{
					if (newDamage > 9)
					{
						if (effectActive)
							return;
						effectActive = true;
						effect.Effect(unit, unit);
					}
					else if (effectActive)
					{
						effectActive = false;
						effect.RevertEffect(unit, unit);
					}
				});

				var registerReactEffect = new ReactCallbackRegisterEffect<ReactType>(
					new ReactCallback<ReactType>(ReactType.DamageChanged, @event));
				var initComponent = new InitComponent(false, new IEffect[] { registerReactEffect }, null);
				return new Modifier(id, genId, name, initComponent, null, null, null,
					new SingleTargetComponent(), null);
			});
			Setup();

			Unit.AddModifierSelf("AddDamageAbove5RemoveDamageBelow5React"); //Starts with 10 baseDmg, adds 5 from effect
			Assert.AreEqual(UnitDamage + 5, Unit.Damage);

			//Remove 6 damage, should remove the effect, making it 15 - 6 - 5 = 4
			Unit.AddDamage(-6); //Revert
			Assert.AreEqual(UnitDamage - 6, Unit.Damage);

			Unit.ResetEventCounters();
			Unit.AddDamage(6);
			Assert.AreEqual(UnitDamage + 5, Unit.Damage);
		}

		[Test]
		public void AddDamageAbove5RemoveDamageBelow5React()
		{
			AddRecipe("AddDamageAbove5RemoveDamageBelow5React")
				.Effect(new AddDamageEffect(5, EffectState.IsRevertibleAndTogglable), EffectOn.CustomCallback)
				.CustomCallback(CustomCallbackType.DamageChanged, effect =>
					new DamageChangedEvent((unit, damage, deltaDamage) =>
					{
						if (damage > 9)
							effect.Effect(unit, unit);
						else
							((IRevertEffect)effect).RevertEffect(unit, unit);
					}));
			Setup();

			Unit.AddModifierSelf("AddDamageAbove5RemoveDamageBelow5React"); //Starts with 10 baseDmg, adds 5 from effect
			Assert.AreEqual(UnitDamage + 5, Unit.Damage);

			//Remove 6 damage, should remove the effect, making it 15 - 6 - 5 = 4
			Unit.AddDamage(-6); //Revert
			Assert.AreEqual(UnitDamage - 6, Unit.Damage);

			Unit.ResetEventCounters();
			Unit.AddDamage(6);
			Assert.AreEqual(UnitDamage + 5, Unit.Damage);
		}

		[Test]
		public void InitStatusEffectSleep_RemoveOnTenDamageTaken_Manual()
		{
			AddGenerator("InitStatusEffectSleep_RemoveOnTenDamageTaken", (id, genId, name, tag) =>
			{
				var effect = StatusEffectEffect.Create(id, genId, StatusEffectType.Sleep, 5f, true);
				var removeEffect = RemoveEffect.Create(id, genId);
				float totalDamageTaken = 0f;
				var @event = new HealthChangedEvent((target, source, health, deltaHealth) =>
				{
					totalDamageTaken += deltaHealth;
					if (totalDamageTaken >= 10)
						removeEffect.Effect(target, source);
				});
				var registerReactEffect = new ReactCallbackRegisterEffect<ReactType>(
					new ReactCallback<ReactType>(ReactType.CurrentHealthChanged, @event));
				//Order of reverts matters here, if we revert the captured variable after
				//it will trigger a recursive effect, because the captured variable will never be reset
				removeEffect.SetRevertibleEffects(new IRevertEffect[]
					{ effect, new RevertActionEffect(() => { totalDamageTaken = 0f; }), registerReactEffect });

				var initComponent = new InitComponent(false, new IEffect[] { effect, registerReactEffect }, null);
				return new Modifier(id, genId, name, initComponent, null, null, null,
					new SingleTargetComponent(), null);
			});
			Setup();

			//Starts with 10 baseDmg, adds 5 from effect
			Unit.AddModifierSelf("InitStatusEffectSleep_RemoveOnTenDamageTaken");
			Assert.True(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Sleep));
			Unit.Update(4); //Still has sleep

			Unit.TakeDamage(9, Unit); //Still has sleep
			Unit.ResetEventCounters();
			Assert.True(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Sleep));

			Unit.TakeDamage(2, Unit); //Removes and reverts sleep
			Assert.False(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Sleep));
		}

		[Test]
		public void InitStatusEffectSleep_RemoveOnTenDamageTaken_Recipe()
		{
			AddRecipe("InitStatusEffectSleep_RemoveOnTenDamageTaken")
				.Effect(new StatusEffectEffect(StatusEffectType.Sleep, 5f, true), EffectOn.Init)
				.Remove(RemoveEffectOn.CustomCallback)
				.CustomCallback(CustomCallbackType.CurrentHealthChanged, removeEffect =>
				{
					float totalDamageTaken = 0f;
					return new HealthChangedEvent((target, source, health, deltaHealth) =>
					{
						//Don't count "negative damage/healing damage"
						if (deltaHealth > 0)
							totalDamageTaken += deltaHealth;
						if (totalDamageTaken >= 10)
						{
							totalDamageTaken = 0f;
							removeEffect.Effect(target, source);
						}
					});
				});
			Setup();

			//Starts with 10 baseDmg, adds 5 from effect
			Unit.AddModifierSelf("InitStatusEffectSleep_RemoveOnTenDamageTaken");
			Assert.True(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Sleep));
			Unit.Update(4); //Still has sleep

			Unit.TakeDamage(9, Unit); //Still has sleep
			Assert.True(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Sleep));

			Unit.TakeDamage(2, Unit); //Removes and reverts sleep
			Assert.False(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Sleep));

			//Check if state is reset
			Enemy.AddModifierSelf("InitStatusEffectSleep_RemoveOnTenDamageTaken");
			Enemy.TakeDamage(9, Enemy);
			Assert.True(Enemy.StatusEffectController.HasStatusEffect(StatusEffectType.Sleep));

			Enemy.TakeDamage(2, Enemy);
			Assert.False(Enemy.StatusEffectController.HasStatusEffect(StatusEffectType.Sleep));
		}

		[Test]
		public void InitStatusEffectSleep_RemoveOnTenDamageTaken_Manual_StateReset()
		{
			AddGenerator("InitStatusEffectSleep_RemoveOnTenDamageTaken", (id, genId, name, tag) =>
			{
				var effect = StatusEffectEffect.Create(id, genId, StatusEffectType.Sleep, 5f, true);
				var removeEffect = RemoveEffect.Create(id, genId);
				float totalDamageTaken = 0f;
				var @event = new HealthChangedEvent((target, source, health, deltaHealth) =>
				{
					totalDamageTaken += deltaHealth;
					if (totalDamageTaken >= 10)
						removeEffect.Effect(target, source);
				});
				var registerReactEffect = new ReactCallbackRegisterEffect<ReactType>(
					new ReactCallback<ReactType>(ReactType.CurrentHealthChanged, @event));
				//Order of reverts matters here, if we revert the captured variable after
				//it will trigger a recursive effect, because the captured variable will never be reset
				removeEffect.SetRevertibleEffects(new IRevertEffect[]
					{ effect, new RevertActionEffect(() => { totalDamageTaken = 0f; }), registerReactEffect });

				var initComponent = new InitComponent(false, new IEffect[] { effect, registerReactEffect }, null);
				return new Modifier(id, genId, name, initComponent, null, null, null,
					new SingleTargetComponent(), null);
			});
			Setup();

			Pool.Clear();
			Pool.Allocate(IdManager.GetId("InitStatusEffectSleep_RemoveOnTenDamageTaken"), 1);

			Unit.AddModifierSelf("InitStatusEffectSleep_RemoveOnTenDamageTaken");
			Assert.True(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Sleep));
			Unit.Update(4);

			Unit.TakeDamage(12, Unit);
			Unit.Update(0); //Remove modifier
			Assert.False(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Sleep));
			Assert.False(Unit.ContainsModifier("InitStatusEffectSleep_RemoveOnTenDamageTaken"));

			Unit.AddModifierSelf("InitStatusEffectSleep_RemoveOnTenDamageTaken");
			Unit.TakeDamage(9, Unit);
			Assert.True(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Sleep));
		}

		[Test]
		public void DispelAddDamageReact()
		{
			AddGenerator("InitStatusEffectSleep_RemoveOnDispel", (id, genId, name, tag) =>
			{
				var effect = StatusEffectEffect.Create(id, genId, StatusEffectType.Sleep, 5f, true);
				var removeEffect = RemoveEffect.Create(id, genId);

				var @event = new DispelEvent((target, source, eventTag) =>
				{
					if ((tag & eventTag.ToInternalTag()) != 0)
						removeEffect.Effect(target, source);
				});
				var registerReactEffect = new ReactCallbackRegisterEffect<ReactType>(
					new ReactCallback<ReactType>(ReactType.Dispel, @event));

				removeEffect.SetRevertibleEffects(new IRevertEffect[] { effect, registerReactEffect });

				var initComponent = new InitComponent(false, new IEffect[] { effect, registerReactEffect }, null);
				return new Modifier(id, genId, name, initComponent, null, null, null,
					new SingleTargetComponent(), null);
			});
			Setup();

			Unit.AddModifierSelf("InitStatusEffectSleep_RemoveOnDispel");
			Unit.Dispel(TagType.IsStack | TagType.IsRefresh, Unit);
			Assert.True(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Sleep));
			Unit.Dispel(TagType.IsInit, Unit);
			Assert.False(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Sleep));
		}
	}
}