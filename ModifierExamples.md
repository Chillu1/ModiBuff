# Modifier Examples

> Important: If you're not using the master branch, it might be smart to go to the correct versions, they'll have the most accurate modifier recipes.
> [V0.2.0](https://github.com/Chillu1/ModiBuff/blob/0.2.0/ModifierExamples.md)
> [V0.1.2](https://github.com/Chillu1/ModiBuff/blob/0.1.2/ModifierExamples.md)

- [Basic Recipes](#basic-recipes)
- [Conditional Recipes](#conditional-recipes)
- [Event Recipes](#event-recipes)
- [Callback Recipes](#callback-recipes)
- [Modifier Action Recipes](#modifier-action-recipes)
- [Advanced Recipes](#advanced-recipes)

[//]: # (TODO Recipe examples for usual game mechanics)
[//]: # (DoT, InitDoTSeparateDmg, OnXStacks, StackableDamage, StunEverySecondFor0.2Seconds)
[//]: # ("Absoultely crazy modifiers": applying appliers on events, X stacks, etc)
[//]: # (Meta, Post, Stack modifiers)

> Important: Damage being the default effect is just an example, it makes it easier to understand.

## Basic Recipes

Delayed damage

```csharp
Add("DurationDamage")
    .Effect(new DamageEffect(5), EffectOn.Duration)
    .Duration(5);
```

Add damage to unit, revert it on remove, remove it after 5 seconds.

```csharp
Add("InitAddDamageRevertible")
    .Effect(new AddDamageEffect(5, true), EffectOn.Init)
    .Remove(5);
```

50% chance to deal 5 damage on init

```csharp
Add("ChanceInitDamage")
    .ApplyChance(0.5f)
    .Effect(new DamageEffect(5), EffectOn.Init);
```

Apply a 5 damage modifier every 1s interval. We could be adding any type of modifier.

```csharp
Add("DamageApplier_Interval")
    .Effect(new ApplierEffect("InitDamage"), EffectOn.Interval)
    .Interval(1);
```

Self heal for 5, deal damage to target for 5

```csharp
Add("InitSelfHeal_DamageTarget")
    .Effect(new HealEffect(5, targeting: Targeting.SourceTarget), EffectOn.Init)
    .Effect(new DamageEffect(5), EffectOn.Init);
```

5 damage when reaches max stacks (2)

```csharp
Add("DamageOnMaxStacks")
    .Effect(new DamageEffect(5, StackEffectType.Effect), EffectOn.Stack)
    .Stack(WhenStackEffect.OnMaxStacks, value: -1, maxStacks: 2);
```

Every 2 stacks, deal 5 damage

```csharp
Add("DamageEveryTwoStacks")
    .Effect(new DamageEffect(5, StackEffectType.Effect), EffectOn.Stack)
    .Stack(WhenStackEffect.OnXStacks, value: -1, maxStacks: -1, true, everyXStacks: 2);
```

Every stack, add 2 damage to the effect, then deal 5 base damage + 2 added damage.

```csharp
Add("StackBasedDamage")
    .Effect(new DamageEffect(5, StackEffectType.Effect | StackEffectType.Add), EffectOn.Stack)
    .Stack(WhenStackEffect.Always, value: 2);
```

Costs 5 mana, deal 5 damage on init

```csharp
Add("InitDamage_CostMana")
    .ApplyCost(CostType.Mana, 5)
    .Effect(new DamageEffect(5), EffectOn.Init);
```

Every two stacks, stun.

```csharp
Add("StunEveryTwoStacks")
    .Effect(new StatusEffectEffect(StatusEffectType.Stun, 2, false, StackEffectType.Effect), EffectOn.Stack)
    .Stack(WhenStackEffect.EveryXStacks, value: -1, maxStacks: -1, everyXStacks: 2);
```

Init Damage. With 1 second linger. Won't work again if applied within 1 second, aka "global cooldown", shared between all X modifier
instaces.

```csharp
Add("OneTimeInitDamage_LingerDuration")
    .OneTimeInit()
    .Effect(new DamageEffect(5), EffectOn.Init)
    .Remove(1);
```

50% chance to trigger effect (init damage)

```csharp
Add("ChanceEffectInitDamage")
    .EffectChance(0.5f)
    .Effect(new DamageEffect(5), EffectOn.Init);
```

1 second cooldown, on effect. Basically the effect can only be triggered once every 1 second (not apply).

```csharp
Add("InitDamage_Cooldown_Effect")
    .EffectCooldown(1)
    .Effect(new DamageEffect(5), EffectOn.Init);
```

Deal 5 damage only when health is above 100

```csharp
Add("InitDamage_EffectCondition_HealthAbove100")
    .EffectCondition(StatType.Health, 100, ComparisonType.GreaterOrEqual)
    .Effect(new DamageEffect(5), EffectOn.Init);
```

Deal 5 damage only when unit has a modifier "Flag"

```csharp
Add("Flag");

Add("InitDamage_EffectCondition_ContainsModifier")
    .EffectCondition("Flag")
    .Effect(new DamageEffect(5), EffectOn.Init);
```

Deal 5 damage only when unit is frozen

```csharp
Add("InitDamage_EffectCondition_FreezeStatusEffect")
    .EffectCondition(StatusEffectType.Freeze)
    .Effect(new DamageEffect(5), EffectOn.Init);
```

Deal 5 damage only when unit can act (move, attack, cast, etc)

```csharp
Add("InitDamage_EffectCondition_ActLegalAction")
    .EffectCondition(LegalAction.Act)
    .Effect(new DamageEffect(5), EffectOn.Init);
```

Combination of unit frozen and has a modifier "Flag"

```csharp
Add("InitDamage_EffectCondition_Combination")
    .EffectCondition("Flag")
    .EffectCondition(StatusEffectType.Freeze)
    .Effect(new DamageEffect(5), EffectOn.Init);
``` 

Costs 5 health, deal 5 damage, heals 5 back. Basically a "barrier" for activating.

```csharp
Add("InitDamage_CostHealth_HealSelf")
    .ApplyCost(CostType.Health, 5)
    .Effect(new DamageEffect(5, StackEffectType.Effect), EffectOn.Init)
    .Effect(new HealEffect(5, targeting: Targeting.SourceSource), EffectOn.Init);
```

Instance stackable DoT, as in each new applied DoT will be a separate instance.

```csharp
AddRecipe("InstanceStackableDoT")
	.InstanceStackable()
	.Interval(1)
	.Effect(new DamageEffect(5), EffectOn.Interval)
	.Remove(5);
```

Instance stackable add damage, uniquely revertible.

```csharp
AddRecipe("InstanceStackableAddDamageRevertible")
	.InstanceStackable()
	.Effect(new AddDamageEffect(5, EffectState.IsRevertible), EffectOn.Init)
	.Remove(5);
```

## Conditional Recipes

50% chance to deal damage

```csharp
AddRecipe("ChanceInitDamage")
	.ApplyChance(0.5f)
	.Effect(new DamageEffect(5), EffectOn.Init);
```

50% change to deal damage every 1 second

```csharp
AddRecipe("ChanceEffectIntervalDamage")
	.EffectChance(0.5f)
	.Interval(1)
	.Effect(new DamageEffect(5), EffectOn.Interval);
```

Deal damage only when targets mana is below or equal to 100

```csharp
AddRecipe("InitDamage_ApplyCondition_ManaBelow100")
	.ApplyCondition(StatType.Mana, 100, ComparisonType.LessOrEqual)
	.Effect(new DamageEffect(5), EffectOn.Init);
```

Deal damage only when target has full health

```csharp
AddRecipe("InitDamage_EffectCondition_HealthFull")
	.EffectCondition(ConditionType.HealthIsFull)
	.Effect(new DamageEffect(5), EffectOn.Init);
```

Deal damage only when target has a modifier "Flag"

```csharp
AddRecipe("Flag");
AddRecipe("InitDamage_EffectCondition_ContainsModifier")
	.EffectCondition("Flag")
	.Effect(new DamageEffect(5), EffectOn.Init);
```

Deal damage only when target is frozen

```csharp
AddRecipe("InitDamage_EffectCondition_FreezeStatusEffect")
	.EffectCondition(StatusEffectType.Freeze)
	.Effect(new DamageEffect(5), EffectOn.Init);
```

Deal damage only when target can act (move, attack, cast, etc)

```csharp
AddRecipe("InitDamage_EffectCondition_ActLegalAction")
	.EffectCondition(LegalAction.Act)
	.Effect(new DamageEffect(5), EffectOn.Init);
```

Can deal damage once every 1 second (cooldown)

```csharp
AddRecipe("InitDamage_Cooldown")
	.ApplyCooldown(1)
	.Effect(new DamageEffect(5), EffectOn.Init);
```

Costs health to deal damage, can't be activated if health is below the cost

```csharp
AddRecipe("InitDamage_CostHealth")
	.ApplyCost(CostType.Health, 5)
	.Effect(new DamageEffect(5), EffectOn.Init);
```

Same as above, but with mana

```csharp
AddRecipe("InitDamage_CostMana")
	.ApplyCost(CostType.Mana, 5)
	.Effect(new DamageEffect(5), EffectOn.Init);
```

## Event Recipes

Thorns on hit (deal 5 damage to attacker)

```csharp
Add("ThornsOnHitEvent")
    .Effect(new DamageEffect(5, targeting: Targeting.SourceTarget), EffectOn.Event)
    .Event(EffectOnEvent.WhenAttacked);
```

Add damage on kill

```csharp
AddEvent("AddDamage_OnKill_Event")
    .Effect(new AddDamageEffect(5, targeting: Targeting.SourceTarget), EffectOn.Event)
    .Event(EffectOnEvent.OnKill);
```

Damage attacker on death

```csharp
AddEvent("Damage_OnDeath_Event")
    .Effect(new DamageEffect(5, targeting: Targeting.SourceTarget), EffectOn.Event)
    .Event(EffectOnEvent.WhenKilled);
```

Attack self when attacked

```csharp
AddEvent("AttackSelf_OnHit_Event")
    .Effect(new SelfAttackActionEffect(), EffectOn.Event)
    .Event(EffectOnEvent.WhenAttacked);
```

When attacked, add damage to all attackers, for 1 second (refreshes)

```csharp
Add("AddDamage")
	.OneTimeInit()
	.Effect(new AddDamageEffect(5, EffectState.IsRevertible), EffectOn.Init)
	.Remove(1).Refresh();
AddEventRecipe("AddDamageToAllAttackers_OnHit_Event")
	.Effect(new ApplierEffect("AddDamage", targeting: Targeting.SourceTarget), EffectOn.Event)
    .Event(EffectOnEvent.WhenAttacked);
```

## Callback Recipes

Add damage on init, remove and revert it if we take a strong hit (damage >= 50% max hp)

```csharp
Add("InitAddDamageRevertibleHalfHealthCallback")
    .Effect(new AddDamageEffect(5, EffectState.IsRevertible), EffectOn.Init)
    .Remove(RemoveEffectOn.CallbackUnit)
    .CallbackUnit(CallbackUnitType.StrongHit);
```

Heal back to full health on a strong hit

```csharp
Add("InitHealToFullHalfHealthCallback")
    .Callback(CallbackType.StrongHit, (target, source) =>
    {
        var damageable = (IDamagable)target;
        ((IHealable)target).Heal(damageable.MaxHealth - damageable.Health, source);
    });
```

Same as above, but with a healeffect and a meta effect instead

```csharp
Add("InitHealToFullHalfHealthCallback")
    .Effect(new HealEffect(0).SetMetaEffects(
        new AddValueBasedOnStatDiffMetaEffect(StatType.MaxHealth)), EffectOn.CallbackUnit)
    .CallbackUnit(CallbackUnitType.StrongHit);
```

Add damage when our game is above 5, remove it when it's below 5. Reacts to damage changes.

```csharp
Add("AddDamageAbove5RemoveDamageBelow5React")
    .Effect(new AddDamageEffect(5, EffectState.IsRevertibleAndTogglable), EffectOn.CallbackEffect)
    .CallbackEffect(CallbackType.DamageChanged, effect =>
        new DamageChangedEvent((unit, damage, deltaDamage) =>
        {
            if (damage > 5)
                effect.Effect(unit, unit);
            else
                ((IRevertEffect)effect).RevertEffect(unit, unit);
        }));
```

Apply a sleep status effect, remove and revert it after taking 10 total damage. Reacts to health changes.

```csharp
AddRecipe("InitStatusEffectSleep_RemoveOnTenDamageTaken")
	.Effect(new StatusEffectEffect(StatusEffectType.Sleep, 5f, true), EffectOn.Init)
	.Remove(RemoveEffectOn.CallbackEffect)
	.CallbackEffect(CallbackType.CurrentHealthChanged, removeEffect =>
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
```

Apply a sleep status effect, remove and revert it on a basic dispel.

```csharp
AddRecipe("InitStatusEffectSleep_RemoveOnDispel")
    .Tag(TagType.BasicDispel)
    .Effect(new StatusEffectEffect(StatusEffectType.Sleep, 5f, true), EffectOn.Init)
    .Remove(RemoveEffectOn.CallbackEffect)
    .CallbackEffect(CallbackType.Dispel, removeEffect =>
        new DispelEvent((target, source, eventTag) =>
        {
            if ((TagType.BasicDispel & eventTag) != 0)
                removeEffect.Effect(target, source);
        }));
```

## Modifier Action Recipes

Add damage, remove after 2 seconds, refresh on strong hit

```csharp
AddRecipe("DurationAddDamageStrongHitRefresh")
	.Effect(new AddDamageEffect(5), EffectOn.Duration)
	.ModifierAction(ModifierAction.Refresh, EffectOn.CallbackUnit)
	.CallbackUnit(CallbackUnitType.StrongHit)
	.Duration(2).Refresh();
```

Add damage every 2 stacks, remove all stacks on strong hit

```csharp
AddRecipe("StackAddDamageStrongHitResetStacks")
	.Effect(new AddDamageEffect(5), EffectOn.Stack)
	.ModifierAction(ModifierAction.ResetStacks, EffectOn.CallbackUnit)
	.CallbackUnit(CallbackUnitType.StrongHit)
	.Stack(WhenStackEffect.EveryXStacks, everyXStacks: 2);
```

Add damage every stack, each stack has a 5 second independent timer, reset all stacks on strong hit

```csharp
AddRecipe("AddDamageStackTimerResetStacks")
	.Effect(new AddDamageEffect(5, EffectState.IsRevertible), EffectOn.Stack)
	.ModifierAction(ModifierAction.ResetStacks, EffectOn.CallbackUnit)
	.CallbackUnit(CallbackUnitType.StrongHit)
	.Stack(WhenStackEffect.Always, independentStackTime: 5);
```

## Advanced Recipes

### Centralized Effect

In case we want to have a few "global" common effects on each unit.

Basic poison damage effect, that holds the poison stacks states.

```csharp
Add("Poison")
    .Stack(WhenStackEffect.Always)
    .Effect(new PoisonDamageEffect(StackEffectType.None, 5), EffectOn.Interval | EffectOn.Stack)
    .Interval(1)
    .Remove(5).Refresh();
```

When poison damage ticks, heal based off the poison stacks

```csharp
AddRecipe("HealPerPoisonStack")
	.Effect(new HealEffect(0, false, StackEffectType.Effect | StackEffectType.SetStacksBased, 1),
		EffectOn.CallbackEffect)
	.CallbackEffect(CallbackType.PoisonDamage, effect =>
		new PoisonEvent((target, source, stacks, totalStacks, damage) =>
		{
			((IStackEffect)effect).StackEffect(stacks, target, source);
		}));
```

Apply poison to target, then heal self based on poison stacks

```csharp
AddRecipe("PoisonHealHeal")
	.Stack(WhenStackEffect.Always)
	.Effect(new HealEffect(0, false, StackEffectType.Effect | StackEffectType.SetStacksBased, 1)
		.SetMetaEffects(new AddValueBasedOnPoisonStacksMetaEffect(1f)), EffectOn.Stack);
AddEffect("PoisonHeal",
	new ApplierEffect("Poison"),
	new ApplierEffect("PoisonHealHeal", Targeting.SourceTarget));
```

When poison damage ticks, deal damage to all attackers that applied poison to us

```csharp
AddRecipe("PoisonThorns")
	.Callback(new Callback<CallbackType>(CallbackType.PoisonDamage,
		new PoisonEvent((target, source, stacks, totalStacks, damage) =>
		{
			((IDamagable<float, float, float, float>)source).TakeDamage(damage, target);
		})));
```