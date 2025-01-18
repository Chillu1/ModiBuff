using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;
using TagType = ModiBuff.Core.Units.TagType;

namespace ModiBuff.Tests
{
	public sealed class CallbackEffectTests : ModifierTests
	{
		[Test]
		public void AddDamageAbove5RemoveDamageBelow5React()
		{
			AddRecipe("AddDamageAbove5RemoveDamageBelow5React")
				.Effect(new AddDamageEffect(5, EffectState.IsRevertibleAndTogglable), EffectOn.CallbackEffect)
				.CallbackEffect(CallbackType.DamageChanged, effect =>
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
			Unit.AddDamage(-2); //Make sure that we don't revert twice
			Assert.AreEqual(UnitDamage - 6 - 2, Unit.Damage);
			Unit.ResetEventCounters();
			Unit.AddDamage(2);

			Unit.ResetEventCounters();
			Unit.AddDamage(6);
			Assert.AreEqual(UnitDamage + 5, Unit.Damage);
		}

		[Test]
		public void InitStatusEffectSleep_RemoveOnTenDamageTaken_StateReset()
		{
			AddRecipe("InitStatusEffectSleep_RemoveOnTenDamageTaken")
				.Effect(new StatusEffectEffect(StatusEffectType.Sleep, 5f, true), EffectOn.Init)
				.Remove(RemoveEffectOn.CallbackEffect)
				.CallbackEffect(CallbackType.CurrentHealthChanged, removeEffect =>
				{
					float totalDamageTaken = 0f;
					return new CallbackStateContext<float>(
						new HealthChangedEvent((target, source, health, deltaHealth) =>
						{
							//Don't count "negative damage/healing damage"
							if (deltaHealth > 0)
								totalDamageTaken += deltaHealth;
							if (totalDamageTaken >= 10)
							{
								totalDamageTaken = 0f;
								removeEffect.Effect(target, source);
							}
						}), () => totalDamageTaken, value => totalDamageTaken = value);
				});
			Setup();

			Pool.Clear();
			Pool.Allocate(IdManager.GetId("InitStatusEffectSleep_RemoveOnTenDamageTaken"), 1);

			//Starts with 10 baseDmg, adds 5 from effect
			Unit.AddModifierSelf("InitStatusEffectSleep_RemoveOnTenDamageTaken");
			Assert.True(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Sleep));
			Unit.Update(4); //Still has sleep

			Unit.TakeDamage(9, Unit); //Still has sleep
			Assert.True(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Sleep));

			Unit.TakeDamage(2, Unit); //Removes and reverts sleep
			Assert.False(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Sleep));
			Unit.Update(0); //Remove modifier, back to pool

			//Check if state is reset
			Enemy.AddModifierSelf("InitStatusEffectSleep_RemoveOnTenDamageTaken");
			Enemy.TakeDamage(9, Enemy);
			Assert.True(Enemy.StatusEffectController.HasStatusEffect(StatusEffectType.Sleep));

			Enemy.TakeDamage(2, Enemy);
			Assert.False(Enemy.StatusEffectController.HasStatusEffect(StatusEffectType.Sleep));
		}

		private static readonly RecipeAddFunc[] dispelAddDamageRecipes =
		{
			//Most control-oriented dispel solution
			add => add("InitStatusEffectSleep_RemoveOnDispel")
				.Tag(TagType.BasicDispel)
				.Effect(new StatusEffectEffect(StatusEffectType.Sleep, 5f, true), EffectOn.Init)
				.Remove(RemoveEffectOn.CallbackEffect)
				.CallbackEffect(CallbackType.Dispel, removeEffect =>
					new DispelEvent((target, source, eventTag) =>
					{
						//TODO We might need to get the modifiers actual tag here?
						//Could feed it through DispelEvent, but that's meh
						if ((TagType.BasicDispel & eventTag) != 0)
							removeEffect.Effect(target, source);
					})),
			add => add("InitStatusEffectSleep_RemoveOnDispel")
				.Tag(TagType.StrongDispel)
				.Effect(new StatusEffectEffect(StatusEffectType.Sleep, 5f, true), EffectOn.Init)
				.Remove(RemoveEffectOn.CallbackEffect)
				.CallbackEffect(CallbackType.StrongDispel, removeEffect => new StrongDispelEvent(removeEffect.Effect)),
			//Easiest dispel solution, note that we can't control for what else can happen in the dispel event
			add => add("InitStatusEffectSleep_RemoveOnDispel")
				.Tag(TagType.StrongDispel)
				.Effect(new StatusEffectEffect(StatusEffectType.Sleep, 5f, true), EffectOn.Init)
				.Remove(RemoveEffectOn.CallbackUnit)
				.CallbackUnit(CallbackUnitType.StrongDispel)
		};

		[TestCaseSource(nameof(dispelAddDamageRecipes))]
		public void DispelAddDamageReact(RecipeAddFunc recipe)
		{
			AddRecipe(recipe);
			Setup();

			Unit.AddModifierSelf("InitStatusEffectSleep_RemoveOnDispel");
			Unit.Dispel(TagType.IsStack | TagType.IsRefresh, Unit);
			Assert.True(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Sleep));
			Unit.Dispel(TagType.BasicDispel, Unit);
			Unit.StrongDispel(Unit);
			Assert.False(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Sleep));
		}

		private static readonly RecipeAddFunc[] stunStackingHealRecipes =
		{
			//Stack redirection version
			add => add("StunHealStackReset")
				.Tag(Core.TagType.CustomStack)
				.Stack(WhenStackEffect.Always)
				.Effect(new HealEffect(0, HealEffect.EffectState.ValueIsRevertible,
					StackEffectType.Effect | StackEffectType.Add, 5), EffectOn.Stack)
				.CallbackEffect(CallbackType.StatusEffectAdded, effect => new AddStatusEffectEvent(
					(target, source, duration, appliedStatusEffect, oldLegalAction, newLegalAction) =>
					{
						if (appliedStatusEffect.HasStatusEffect(StatusEffectType.Stun))
							effect.Effect(target, source);
					}))
				.ModifierAction(ModifierAction.ResetStacks, EffectOn.Interval)
				.ModifierAction(ModifierAction.Refresh | ModifierAction.Stack, EffectOn.CallbackEffect)
				.Interval(10).Refresh(),
			//Non-stack version
			add => add("StunHealStackReset")
				.Tag(Core.TagType.CustomStack)
				.Stack(WhenStackEffect.OnMaxStacks, 10000)
				.Effect(new HealEffect(0, HealEffect.EffectState.ValueIsRevertible,
					StackEffectType.Effect | StackEffectType.Add, 5), EffectOn.Stack | EffectOn.CallbackEffect)
				.CallbackEffect(CallbackType.StatusEffectAdded, effect => new AddStatusEffectEvent(
					(target, source, duration, appliedStatusEffect, oldLegalAction, newLegalAction) =>
					{
						if (appliedStatusEffect.HasStatusEffect(StatusEffectType.Stun))
							((ICallbackEffect)effect).CallbackEffect(target, source);
					}))
				.ModifierAction(ModifierAction.ResetStacks, EffectOn.Interval)
				.ModifierAction(ModifierAction.Refresh | ModifierAction.Stack, EffectOn.CallbackEffect)
				.Interval(10).Refresh()
		};

		[TestCaseSource(nameof(stunStackingHealRecipes))]
		public void StunStackingHeal_IntervalStackReset_RefreshIntervalOnStun(RecipeAddFunc recipe)
		{
			AddRecipe(recipe);
			Setup();

			Unit.TakeDamage(UnitHealth / 2f, Unit);
			Unit.AddModifierSelf("StunHealStackReset");

			Unit.StatusEffectController.ChangeStatusEffect(0, 0, StatusEffectType.Stun, 5f, Unit);
			Unit.AddModifierSelf("StunHealStackReset"); //Check stack behaviour
			Assert.AreEqual(UnitHealth / 2f + 5, Unit.Health);

			Unit.Update(1);
			Unit.StatusEffectController.ChangeStatusEffect(0, 0, StatusEffectType.Stun, 5f, Unit);
			Assert.AreEqual(UnitHealth / 2f + 5 + 10, Unit.Health);

			Unit.Update(9); //Doesn't reset stacks
			Unit.StatusEffectController.ChangeStatusEffect(0, 1, StatusEffectType.Stun, 5f, Unit);
			Assert.AreEqual(UnitHealth / 2f + 5 + 10 + 15, Unit.Health);

			Unit.Update(10); //Resets stacks
			Unit.StatusEffectController.ChangeStatusEffect(0, 2, StatusEffectType.Stun, 5f, Unit);
			Assert.AreEqual(UnitHealth / 2f + 5 + 10 + 15 + 5, Unit.Health);

			Unit.StatusEffectController.ChangeStatusEffect(0, 2, StatusEffectType.Stun, 5f, Unit);
			Assert.AreEqual(UnitHealth / 2f + 5 + 10 + 15 + 5 + 10, Unit.Health);
		}

		[Test]
		public void SilenceSourceWhenSilenced()
		{
			AddRecipe("Silence")
				.Effect(new StatusEffectEffect(StatusEffectType.Silence, 5), EffectOn.Init);
			AddRecipe("SilenceSourceWhenSilenced")
				.Effect(new StatusEffectEffect(StatusEffectType.Silence, 2f), EffectOn.CallbackEffect)
				.CallbackEffect(CallbackType.StatusEffectAdded, effect => new AddStatusEffectEvent(
					(target, source, duration, appliedStatusEffect, oldLegalAction, newLegalAction) =>
					{
						if (appliedStatusEffect.HasStatusEffect(StatusEffectType.Silence))
							effect.Effect(source, target);
					}));
			Setup();

			Unit.AddModifierSelf("SilenceSourceWhenSilenced");
			Enemy.AddApplierModifier(Recipes.GetGenerator("Silence"), ApplierType.Cast);
			Enemy.TryCast("Silence", Unit);
			Assert.True(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Silence));
			Assert.True(Enemy.StatusEffectController.HasStatusEffect(StatusEffectType.Silence));

			Unit.Update(2);
			Enemy.Update(2);
			Assert.True(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Silence));
			Assert.False(Enemy.StatusEffectController.HasStatusEffect(StatusEffectType.Silence));

			Unit.Update(3);
			Assert.False(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Silence));
		}

		private static readonly RecipeAddFunc[] stunnedFourTimesDispelAllRecipes =
		{
			//CallbackState version, preferred version for one-off effects
			add => add("StunnedFourTimesDispelAllStatusEffects")
				.Callback(CallbackType.StatusEffectAdded, () =>
				{
					float totalTimesStunned = 0f;
					return new CallbackStateContext<float>(new AddStatusEffectEvent(
						(target, source, duration, statusEffect, oldLegalAction, newLegalAction) =>
						{
							if (statusEffect.HasStatusEffect(StatusEffectType.Stun))
							{
								totalTimesStunned++;
								if (totalTimesStunned >= 4)
								{
									totalTimesStunned = 0f;
									((IStatusEffectOwner<LegalAction, StatusEffectType>)target)
										.StatusEffectController
										.DispelAll(source);
								}
							}
						}), () => totalTimesStunned, value => totalTimesStunned = value);
				}),
			//CallbackEffect version, preferred version with general effects
			add => add("StunnedFourTimesDispelAllStatusEffects")
				.Effect(new DispelStatusEffectEffect(StatusEffectType.All), EffectOn.CallbackEffect)
				.CallbackEffect(CallbackType.StatusEffectAdded, effect =>
				{
					float totalTimesStunned = 0f;
					return new CallbackStateContext<float>(new AddStatusEffectEvent(
						(target, source, duration, statusEffect, oldLegalAction, newLegalAction) =>
						{
							if (statusEffect.HasStatusEffect(StatusEffectType.Stun))
							{
								totalTimesStunned++;
								if (totalTimesStunned >= 4)
								{
									totalTimesStunned = 0f;
									effect.Effect(target, source);
								}
							}
						}), () => totalTimesStunned, value => totalTimesStunned = value);
				}),
			//Stack version
			add => add("StunnedFourTimesDispelAllStatusEffects")
				.Tag(TagType.CustomStack)
				.Stack(WhenStackEffect.EveryXStacks, everyXStacks: 4)
				.Effect(new DispelStatusEffectEffect(StatusEffectType.All), EffectOn.Stack)
				.ModifierAction(ModifierAction.Stack, EffectOn.CallbackEffect)
				.CallbackEffect(CallbackType.StatusEffectAdded, effect => new AddStatusEffectEvent(
					(target, source, duration, statusEffect, oldLegalAction, newLegalAction) =>
					{
						if (statusEffect.HasStatusEffect(StatusEffectType.Stun))
							effect.Effect(target, source);
					}))
		};

		[TestCaseSource(nameof(stunnedFourTimesDispelAllRecipes))]
		public void StunnedFourTimes_DispelAllStatusEffects(RecipeAddFunc addFunc)
		{
			AddRecipe("Freeze")
				.Effect(new StatusEffectEffect(StatusEffectType.Freeze, 5), EffectOn.Init);
			AddRecipe(addFunc);
			Setup();

			//Add 3 times to check if adding will add stacks
			for (int i = 0; i < 3; i++)
				Unit.AddModifierSelf("StunnedFourTimesDispelAllStatusEffects");
			Unit.AddModifierSelf("Freeze"); //Different effect to dispel
			Assert.False(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Stun));
			Unit.StatusEffectController.ChangeStatusEffect(0, 0, StatusEffectType.Stun, 5f, Unit);
			Assert.True(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Freeze));

			for (int i = 0; i < 3; i++)
				Unit.StatusEffectController.ChangeStatusEffect(0, 0, StatusEffectType.Stun, 5f, Unit);

			Assert.False(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Freeze));
			Assert.False(Unit.StatusEffectController.HasStatusEffect(StatusEffectType.Stun));
		}

		[Test]
		public void DamageOnStun_HealOnAnyNotStunStatusEffectRemoved()
		{
			AddRecipe("DamageOnStun_HealOnAnyNotStunStatusEffectRemoved")
				.Effect(new DamageEffect(5), EffectOn.CallbackEffect)
				.CallbackEffect(CallbackType.StatusEffectAdded, effect => new AddStatusEffectEvent(
					(target, source, duration, statusEffect, oldLegalAction, newLegalAction) =>
					{
						if (statusEffect.HasStatusEffect(StatusEffectType.Stun))
							((ICallbackEffect)effect).CallbackEffect(target, source);
					}))
				.Effect(new HealEffect(5), EffectOn.CallbackEffect2)
				.CallbackEffect(CallbackType.StatusEffectRemoved, effect => new RemoveStatusEffectEvent(
					(target, source, statusEffect, oldLegalAction, newLegalAction) =>
					{
						if (!statusEffect.HasStatusEffect(StatusEffectType.Stun))
							((ICallbackEffect)effect).CallbackEffect(target, source);
					}));
			AddRecipe("Stun")
				.Effect(new StatusEffectEffect(StatusEffectType.Stun, 1), EffectOn.Init)
				.Remove(1).Refresh();
			AddRecipe("Freeze")
				.Effect(new StatusEffectEffect(StatusEffectType.Freeze, 1), EffectOn.Init)
				.Remove(1).Refresh();
			Setup();

			Unit.AddModifierSelf("DamageOnStun_HealOnAnyNotStunStatusEffectRemoved");
			Unit.AddModifierSelf("Stun");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
			Unit.Update(1);
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
			Unit.AddModifierSelf("Freeze");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
			Unit.Update(1);
			Assert.AreEqual(UnitHealth, Unit.Health);
		}

		[Test]
		public void DamageOnStun_HealOnAnyNotStunStatusEffectRemoved_StackDamageWhenLongStunned()
		{
			AddRecipe("DamageOnStun_HealOnAnyNotStunStatusEffectRemoved_StackDamageWhenLongStunned")
				.Stack(WhenStackEffect.Always)
				.CustomStack(CustomStackEffectOn.CallbackEffect)
				.CallbackEffect(CallbackType.StatusEffectAdded, effect => new AddStatusEffectEvent(
					(target, source, duration, statusEffect, oldLegalAction, newLegalAction) =>
					{
						if (statusEffect.HasStatusEffect(StatusEffectType.Stun) && duration >= 5f)
							effect.Effect(target, source);
					}))
				.Effect(new DamageEffect(5, StackEffectType.Add, 2), EffectOn.Stack | EffectOn.CallbackEffect2)
				.CallbackEffect(CallbackType.StatusEffectAdded, effect => new AddStatusEffectEvent(
					(target, source, duration, statusEffect, oldLegalAction, newLegalAction) =>
					{
						if (statusEffect.HasStatusEffect(StatusEffectType.Stun))
							effect.Effect(target, source);
					}))
				.Effect(new HealEffect(5, HealEffect.EffectState.None, StackEffectType.Add, 2),
					EffectOn.Stack | EffectOn.CallbackEffect3)
				.CallbackEffect(CallbackType.StatusEffectRemoved, effect => new RemoveStatusEffectEvent(
					(target, source, statusEffect, oldLegalAction, newLegalAction) =>
					{
						if (!statusEffect.HasStatusEffect(StatusEffectType.Stun))
							effect.Effect(target, source);
					}));

			AddRecipe("Stun")
				.Effect(new StatusEffectEffect(StatusEffectType.Stun, 1), EffectOn.Init)
				.Remove(1).Refresh();
			AddRecipe("LongStun")
				.Effect(new StatusEffectEffect(StatusEffectType.Stun, 5), EffectOn.Init)
				.Remove(5).Refresh();
			AddRecipe("Freeze")
				.Effect(new StatusEffectEffect(StatusEffectType.Freeze, 1), EffectOn.Init)
				.Remove(1).Refresh();
			Setup();

			Unit.AddModifierSelf("DamageOnStun_HealOnAnyNotStunStatusEffectRemoved_StackDamageWhenLongStunned");
			Unit.AddModifierSelf("Stun");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
			Unit.Update(1);
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
			Unit.AddModifierSelf("Freeze");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
			Unit.Update(1);
			Assert.AreEqual(UnitHealth, Unit.Health);

			Unit.AddModifierSelf("LongStun");
			Assert.AreEqual(UnitHealth - 5 - 2, Unit.Health);
			Unit.Update(5);
			Assert.AreEqual(UnitHealth - 5 - 2, Unit.Health);
			Unit.AddModifierSelf("Freeze");
			Assert.AreEqual(UnitHealth - 5 - 2, Unit.Health);
			Unit.Update(1);
			Assert.AreEqual(UnitHealth, Unit.Health);
		}

		[Test]
		public void MultipleEffectCallbacks()
		{
			AddRecipe("DamageEffectCallbacksRemoveOnDisarm")
				.Effect(new DamageEffect(1), EffectOn.CallbackEffect)
				.CallbackEffect(CallbackType.StatusEffectAdded, effect => new AddStatusEffectEvent(
					(target, source, duration, statusEffect, oldLegalAction, newLegalAction) =>
					{
						if (statusEffect.HasStatusEffect(StatusEffectType.Stun))
							((ICallbackEffect)effect).CallbackEffect(target, source);
					}))
				.Effect(new DamageEffect(2), EffectOn.CallbackEffect2)
				.CallbackEffect(CallbackType.StatusEffectAdded, effect => new AddStatusEffectEvent(
					(target, source, duration, statusEffect, oldLegalAction, newLegalAction) =>
					{
						if (statusEffect.HasStatusEffect(StatusEffectType.Freeze))
							((ICallbackEffect)effect).CallbackEffect(target, source);
					}))
				.Effect(new DamageEffect(3), EffectOn.CallbackEffect3)
				.CallbackEffect(CallbackType.StatusEffectAdded, effect => new AddStatusEffectEvent(
					(target, source, duration, statusEffect, oldLegalAction, newLegalAction) =>
					{
						if (statusEffect.HasStatusEffect(StatusEffectType.Root))
							((ICallbackEffect)effect).CallbackEffect(target, source);
					}))
				.Remove(RemoveEffectOn.CallbackEffect4)
				.CallbackEffect(CallbackType.StatusEffectAdded, effect => new AddStatusEffectEvent(
					(target, source, duration, statusEffect, oldLegalAction, newLegalAction) =>
					{
						if (statusEffect.HasStatusEffect(StatusEffectType.Disarm))
							effect.Effect(target, source);
					}));
			Setup();

			Unit.AddModifierSelf("DamageEffectCallbacksRemoveOnDisarm");
			Unit.StatusEffectController.ChangeStatusEffect(0, 0, StatusEffectType.Stun, 1f, Unit);
			Assert.AreEqual(UnitHealth - 1, Unit.Health);
			Unit.StatusEffectController.ChangeStatusEffect(1, 0, StatusEffectType.Freeze, 1f, Unit);
			Assert.AreEqual(UnitHealth - 1 - 2, Unit.Health);
			Unit.StatusEffectController.ChangeStatusEffect(2, 0, StatusEffectType.Root, 1f, Unit);
			Assert.AreEqual(UnitHealth - 1 - 2 - 3, Unit.Health);
			Unit.StatusEffectController.ChangeStatusEffect(3, 0, StatusEffectType.Disarm, 1f, Unit);
			Unit.Update(0);
			Assert.AreEqual(UnitHealth - 1 - 2 - 3, Unit.Health);
			Assert.False(Unit.ContainsModifier("DamageEffectCallbacksRemoveOnDisarm"));
		}
	}
}