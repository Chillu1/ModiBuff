![Library Logo](Docs/LogoTest.png)
![Library Text](Docs/ModiBuff.png)
![Coverage](Docs/badge_linecoverage.svg)

- [What is this?](#what-is-this)
- [Features](#features)
- [Benchmarks](#benchmarks)
- [Installation](#installation)
- [Differences to Ecs and Old](#differences-to-ecs-and-old)
- [Usage](#usage)
  - [Recipe](#recipe)
  - [Modifier](#modifier)
  - [Effect](#effect)
- [When to use which library](#when-to-use-which-library)
- [FAQ](#faq)
- [Examples](#examples)

# What is this?
This library was made to make a standarized powerful system that allows for manipulation of both effects and entities.

> Note: The library is currently in development, and it will most likely encounter breaking API changes.
 
> Note: The library currently depends on Unity Engine for logging and unit testing, but this is temporary, the 0.1 release will be completely engine agnostic.

# Features
* No GC/allocations (fully pooled)
* Low memory usage (20 MB for 100 000 modifiers)
* Fast iteration [10 000 modifiers in 5ms](#benchmarks)
* Easy high level API [recipes](#recipe)
* Effects on actions
  * Init
  * Interval
  * Duration
  * Stack
* Effects
  * Damage (& self damage)
  * Heal
  * Status effects (stun, silence, disarm, etc.)
  * And many more, see [the rest](ModiBuff/Assets/Scripts/Core/Components/Effect/Effects)

# Benchmarks

Preallocated Pools

|      | InitDmg, N:5k | DoT, N:5k    | DoT pool, N:5k | DoT pool reset return, N:5k |
|------|---------------|--------------|----------------|-----------------------------|
| Lite | 0.74ms, 1 GC  | 2.84ms, 4 GC | 0.12ms, 0 GC   | 0.25ms, 0 GC                |
| Ecs  | 4.00ms, 1 GC  | 5.80ms, 1 GC | NaN            | NaN                         |
| Old  | 46.0ms, 45 GC | 70 ms, 63 GC | NaN            | NaN                         |

Pooling in lite is 280X faster than original (because of pooling & reset)
But it's also faster in case of doing init/stack/refresh on an existing modifier (we don't create a new modifier)
Ecs is a bit on the slow side because we're creating the entities and their components, instead of reusing them, like in the case of lite.

Lite InitDmg (not cloning any components, no state)
DoT = InitDoTSeparateDamageRemove

# Installation
Currently the library is not on NuGet or any other package manager. You can download the source code and add it to your project directly.

# Differences to Ecs and Old
## Ecs
Lite has:
* No GC/allocations
* No ECS framework needed
* Worse iteration speed, 10 000 modifiers compared to 100 000 modifiers, 5ms update, average complexity modifiers

* More features
  * ... 
## Old
Lite has:
* **Much** better backend and design decisions
* Lightweight
* Smaller Codebase
* No GC/allocations
* Improved API
* Better iteration speed, 10 000 modifiers (from 500), 5ms update, average complexity modifiers
* Only cloning statefull objects (less memory, 20 MB for 100_000 modifiers, 7 MB for 100_000 simple modifiers)

* Less features, missing:
  * Unit event (on attacked, on hit, on killed, when attacking...)
  * Aura
  * Condition effects (when low health)
  * Some status effects (taunt, confuse)
  * Fully functioning revertible modifiers

# Usage
## Recipe
ModifierRecipes are the high level API for creating modifiers, they use the builder pattern/method chaining/fluent interface to create modifiers (without the need for calling a Finish/Complete method).

Easiest modifier, that does 5 damage when added, is created like this:
```csharp
Add("InitDamage")
    .Effect(new DamageEffect(5), EffectOn.Init);
```

More advanced damage over time modifier, with 10 damage when added, and 5 damage per interval.  
Removed after 3 seconds, with the duration being refreshable.
```csharp
Add("Init_DoT_Remove_Refreshable")
    .Interval(1)
    .Effect(new DamageEffect(10), EffectOn.Init)
    .Effect(new DamageEffect(5), EffectOn.Interval)
    .Remove(3)
    .Refresh();
```

You're also able to create modifiers with same effect instance on multiple actions.  
Ex. Same damage on Init and Interval.  
> Note: init will be triggered each time we try to add the modifier to the entity.
```csharp
Add("InitDoT")
    .Interval(1)
    .Effect(new DamageEffect(10), EffectOn.Init | EffectOn.Interval);
``` 

Any IEffect can be added.  
Ex. simple stun effect
```csharp
Add("InitStun")
    .Effect(new StatusEffectEffect(StatusEffectType.Stun, 2), EffectOn.Init)
    .Remove(2);
```


## Effect
## Modifier
Modifiers are the core backend part of the library, they are the things that are applied to entities with effects on certain actions.    
Ex. Init, Interval, Duration, Stack.  
You should **NOT** use the Modifier class directly, but instead use the recipe system. 
Recipe system fixes a lot of internal complexity of setting up modifiers for you.  
It's possible to use the Modifier class directly in cases where you'd want multiple interval/duration components.

# When to use which library
## Lite
Smmary: Very optimized, no GC, good featureset.  
Ex. games: Rimworld    
Ex. genres: small arpg, small rts, pve, colony sim

Lite is the best choice 80% of the time. It's fast, lightweight, deeply redesigned core, has no GC/allocations, and is very easy to use.
It's also very well tested for most scenarios.

## Ecs
Summary: Fastest iteration, small featureset, needs ecs framework. Entities: Solo vs Thousands, or Thousands vs Thousands.  
Ex. games: PoE, Diablo  
Ex. genres: arpg, rpg, rts, pve

Ecs is a good choice if you don't care about about having a lot of features, and if your game will have hundreds of thousands of units.
Or just if you want to use it with an ecs framework.

## Original
Summary: Not optimized, amazing featureset, entities: Solo vs Solo, Solo vs Party, Party vs Party  
Ex. games: binding of isaac, tiny rogues, gungeon, dota, witcher 3  
Ex. genres: moba, arena, duel

Only choose original if you need the deep featureset, AND you don't expect to have more than 100 units in the game at the same time, all using/applying 10 modifiers each frame.
If you're making a moba or a small PvP arena game, you can use the original library. That being said, Lite is a better choice for the vast majority of games.


# FAQ

# Examples

## Modifier/Effect/Recipe
[//]: # (TODO Recipe examples for usual game mechanics)
[//]: # (DoT, InitDoTSeparateDmg, OnXStacks, StackableDamage, StunEverySecondFor0.2Seconds)

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
    .Chance(0.5f)
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
    .Effect(new SelfHealEffect(5), EffectOn.Init)
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

## Full