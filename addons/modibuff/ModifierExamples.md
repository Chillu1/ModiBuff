# Modifier Examples


A list of a lot basic recipe examples.

[//]: # (TODO Recipe examples for usual game mechanics)

[//]: # (DoT, InitDoTSeparateDmg, OnXStacks, StackableDamage, StunEverySecondFor0.2Seconds)

[//]: # ("Absoultely crazy modifiers": applying appliers on events, X stacks, etc)

> Important: Damage being the default effect is just an example, it makes it easier to understand.

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
    .Effect(new HealEffect(5), EffectOn.Init, Targeting.SourceTarget)
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
    .Effect(new HealEffect(5), EffectOn.Init, Targeting.SourceSource);
```

## Event Recipes

Thorns on hit (deal 5 damage to attacker)

```csharp
AddEvent("ThornsOnHitEvent", EffectOnEvent.WhenAttacked)
    .Effect(new DamageEffect(5), Targeting.SourceTarget);
```

Add damage on kill

```csharp
AddEvent("AddDamage_OnKill_Event", EffectOnEvent.OnKill)
    .Effect(new AddDamageEffect(5), Targeting.SourceTarget);
```

Damage attacker on death

```csharp
AddEvent("Damage_OnDeath_Event", EffectOnEvent.WhenKilled)
    .Effect(new DamageEffect(5), Targeting.SourceTarget);
```

Attack self when attacked

```csharp
AddEvent("AttackSelf_OnHit_Event", EffectOnEvent.WhenAttacked)
    .Effect(new SelfAttackActionEffect());
```

Add damage to all attackers when attacked

```csharp
AddRecipe("AddDamage")
	.OneTimeInit()
	.Effect(new AddDamageEffect(5, true), EffectOn.Init)
	.Remove(1).Refresh();
AddEventRecipe("AddDamageToAllAttackers_OnHit_Event", EffectOnEvent.WhenAttacked)
	.Effect(new ApplierEffect("AddDamage"), Targeting.SourceTarget);
```