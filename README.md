<p align="center">
<img src="Docs/LogoTest.png" width="500"/>
<img src="Docs/ModiBuff.png" width="500"/>
</p>

<h1></h1>

![Coverage](Docs/badge_linecoverage.svg)

- [What is this?](#what-is-this)
- [Features](#features)
- [Benchmarks](#benchmarks)
- [Requirements](#requirements)
- [Installation](#installation)
- [Usage](#usage)
	- [Recipe](#recipe)
	- [Adding Modifiers to Units](#adding-modifiers-to-units)
	- [Effect](#effect)
- [FAQ](#faq)
- [Examples](#examples)
- [Differences to ModiBuffEcs and Old](#differences-to-modibuffecs-and-old)

# What is this?

This zero dependency library was made to make a standardized powerful system that allows for manipulation of effects on entities.

**It focuses on Feature Set, Performance and Ease of use, in that order.**

The library is split into two core parts:

ModiBuff is the core backend part that handles all the modifier logic and is mostly unopinionated when it comes to the game logic.

Meanwhile ModiBuff.Units is a fully featured implementation of the library, that showcases how to tie the library into a game. 

> Note: The library is currently in development, and it will most likely encounter breaking API changes.

## Why do I need this?

The vast majority of games make their own buff/debuff systems, to fit their game logic.
Examples of this are: Dota 2, League of Legends, Path of Exile, Diablo, World of Warcraft, etc.

While this is a great approach, it's also very time consuming, and requires a fair amount of research to design well.

This library solves that, but also allows for more complex and deeper modifiers than the aforementioned games.

# Features

* No GC/heap allocations (fully pooled with state reset)
* Low memory usage (2-5 MB for 10_000 modifiers)
* Fast effects [10_000 damage modifiers in 0.24ms](#benchmarks)
* Fast iteration [10_000 interval modifiers & 10_000 units in 1.37ms](#benchmarks)
* Easy high level API [recipes](#recipe)
* Instance Stackable modifiers (multiple instances of the same modifier)
* Effects on actions
	* Init
	* Interval
	* Duration
	* Stack
    * Callback (soon)
* Effect implementation examples
	* Damage (& self damage)
	* Heal
	* Status effects (stun, silence, disarm, etc.)
	* Add stat (Damage, Heal)
	* Actions (Attack, Heal, etc.)
	* [Special Applier (another Modifier)](#applier-effect)
	* And more, see [the rest](ModiBuff/ModiBuff.Units/Effects)
* Meta & Post effect manipulation (ex. lifesteal)
* Conditions (checks)
	* Chance 0-100%
	* Cooldown
	* Health/Mana cost
	* General:
		* Stat (health/mana/damage) >/=/< than X
		* Stat is full/empty
		* Has LegalAction (can attack, cast spell, move, etc.)
		* Has StatusEffect (stunned, silenced, disarmed, etc.)
		* Has Modifier
* Applier Modifiers
	* OnAttack
	* Cast
* [Event based Effects](#event-recipe)
	* When Attacked/Cast/Killed/Healed
	* On Attack/Cast/Kill/Heal
* Fully revertible effects

# Benchmarks

TL;DR: It's fast. Takes 1ms± to add & update 10000 modifiers every frame.

BenchmarkDotNet v0.13.6, EndeavourOS  
Intel Core i7-4790 CPU 3.60GHz (Haswell), 1 CPU, 8 logical and 4 physical cores  
.NET SDK 6.0.120  
.NET 6.0.20 (6.0.2023.36801), X64 RyuJIT AVX2

N: 10_000  
Delta: 0.0167 * N

#### Add/Apply/Update Modifier table

Pre-allocated Pools

| Library                                               | NoOp* <br/>(1 unit) | Apply<br/>InitDmg<br/>(1 unit) | Apply<br/>InitStackDmg<br/>(1 unit) | Apply Multi<br/>instance DoT |
|-------------------------------------------------------|---------------------|--------------------------------|-------------------------------------|------------------------------|
| ModiBuff (this)                                       | 0.16ms, 0 B         | 0.24ms, 0 B                    | 0.43ms, 0 B                         | 0.98ms, 0 B                  |
| [ModiBuffEcs](https://github.com/Chillu1/ModiBuffEcs) | ?                   | 1.02ms, 0 GC                   | ?                                   | X                            |
| [Old](https://github.com/Chillu1/ModifierLibrary)     | ?                   | 21.4ms, 24 GC                  | ?                                   | X                            |

| Library                                               | Update DoT**<br/>(10_000 units, N:1) | Update Instance<br/>Stackable DoT |
|-------------------------------------------------------|--------------------------------------|-----------------------------------|
| ModiBuff (this)                                       | 1.37ms, 0 B                          | 0.12ms, 0 B                       |
| [ModiBuffEcs](https://github.com/Chillu1/ModiBuffEcs) | 0.44ms, 0 B                          | X                                 |
| [Old](https://github.com/Chillu1/ModifierLibrary)     | ?                                    | X                                 |

#### New Modifier/Pool table

| Library                                               | DoT pool rent | DoT pool<br/>reset return |
|-------------------------------------------------------|---------------|---------------------------|
| ModiBuff (this)                                       | 0.04ms, 0 GC  | 0.17ms, 0 B               |
| [ModiBuffEcs](https://github.com/Chillu1/ModiBuffEcs) | 1.64ms, 0 GC  | 4.26ms, 0 GC              |
| [Old](https://github.com/Chillu1/ModifierLibrary)     | X             | X                         |

> Important: Non-pool ("New") benchmarks below don't matter for runtime performance in ModiBuff and ModiBuffEcs, since it will only be 
> slower when allocating the new modifiers in the pools. Which if handled correctly, should only happen on initialization.

| Library                                               | New<br/>InitDmg | New<br/>DoT*   |
|-------------------------------------------------------|-----------------|----------------|
| ModiBuff (this)                                       | 0.97ms, 2.4 MB  | 2.36ms, 5.5 MB |
| [ModiBuffEcs](https://github.com/Chillu1/ModiBuffEcs) | 10.4ms,   2 GC  | 16.7ms,   2 GC |
| [Old](https://github.com/Chillu1/ModifierLibrary)     | 92.0ms,  90 GC  | 140 ms, 126 GC |

Setting up all recipes, with 64 pool allocation per recipe takes 60ns, and 104KB.

Pre-allocating 1_000 modifiers of each recipe (currently 100±) takes 67ms, and 35MB.

Pooling in ModiBuff is 700X faster than original old version (because of pool rent & return)  
But it's also much faster in cases of doing init/stack/refresh on an existing modifier (we don't create a new modifier anymore)  
ModiBuffEcs is a bit on the slow side for now, because of how pooling works, with enabling and disabling entities.

*NoOp is an empty effect, so it just measures the benchmark time of the library without unit logic (ex. taking damage).  
**DoT = InitDoTSeparateDamageRemove

# Requirements

ModiBuff is compatible with .NETStandard 1.1, C# 7.2 (C# 7.0 is possible by removing readonly from ModifierReference)

For development net 6.0 is required to build and run all tests. The tests depend on NUnit, and benchmarks depend on BenchmarkDotNet.

# Installation

Currently the library is not on NuGet or any other package manager.

DLLs are available in the [Releases](https://github.com/Chillu1/ModiBuff/releases) tab.

For finer control you can download the source code and add it to your project directly.

For a full implementation of all library features with Units, you should get ModiBuff.Units DLL.

## Step by step installation

1. Download the latest DLL from [Releases](https://github.com/Chillu1/ModiBuff/releases) or ModiBuff source code.
2. Add the DLL to your project.
3. Make your own `ModifierRecipes` class that inherits from `ModiBuffModifierRecipes` and fill it with your modifier recipes.
4. Make your own logger implementation, by inheriting `ILogger`, or use one of the built-in ones.
5. Call ModiBuff setup systems in the initialization of your game.  
5.1. You can change the internal config values inside `Config`
```csharp
Logger.SetLogger<MyLogger>();
//Config.MaxPoolSize = 10000;

var idManager = new ModifierIdManager();
_recipes = new ModifierRecipes(idManager);
_pool = new ModifierPool(_recipes);
```
If you want to use the Units implementation, go to [ModiBuff.Units](#modibuff.units).
Otherwise go to [Custom Units](#custom-units).

### ModiBuff.Units
6. Download the latest ModiBuff.Units DLL from [Releases](https://github.com/Chillu1/ModiBuff/releases) or ModiBuff source code.
7. Add the DLL to your project.
8. Now you can create your units, and apply modifiers to them.

### Custom Units
6. Implement `IUnit` and `IModifierOwner` interfaces on your unit class.
6.1. Optionally add some of the ModiBuff.Units `IUnit` [interfaces](ModiBuff/ModiBuff.Units/Unit/Interfaces) that you want to use.
7. Create your own interfaces that your effects will use, and implement them on your unit class.

## Setup

1. Make your own "ModifierRecipes" class that inherits from "ModifierRecipesBase" and fill it with your modifier recipes.
2. Create your own "Unit" class/implement "IUnit" interfaces or use the "Unit" class from CoreUnits.

# Usage

## Recipe

ModifierRecipes are the high level API for creating modifiers, they use the builder pattern/method chaining/fluent interface to create
modifiers (without the need for calling a Finish/Complete method).

Easiest modifier, that does 5 damage when added, can be created like this:

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
> Note: init will be triggered each time we try to add the modifier to the unit (unless we set `.OneTimeInit()`).

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

[//]: # (TODO ## EventRecipe)

## In-depth Recipe Creation

Every modifier needs a unique name.
It will be registered under this name in the backend,
and assigned an unique ID.

```csharp
Add("ModifierName")
```

Recipes have a methods that determine the functionality of the made modifier.

### Effect

The only method to setup effects is `Effect(IEffect, EffectOn, Targeting)`.  
`IEffect` is the effect that will be applied to the unit, it can be anything, as long as it implements IEffect interface.  
`EffectOn` is the action that will trigger the effect: Init, Interval, Duration, Stack.
`EffectOn` is a flag enum, so the effect can be triggered on multiple actions.  
`Targeting` tells the effect how it should be targeted, if we should target the owner (source) of the modifier, or the targeted unit.
If you're unsure what this means, leave it at default. There will be more examples of this later.

```csharp
Add("InitDamage")
    .Effect(new DamageEffect(5), EffectOn.Init);
```

### Interval

The first most common action method is `Interval(float)`. It's used to set the interval of the modifier.
And interval effects will be triggered every X seconds.

```csharp
Add("IntervalDamage")
	.Interval(1)
	.Effect(new DamageEffect(5), EffectOn.Interval);
```

### Duration

Next is `Duration(float)`. It's used to set the duration of the duration effects. It's usually used to remove the modifier after X seconds.
But it can be used for any effect.
> Note: When we want to remove the modifier after X seconds, it's simpler to use the `Remove(float)` method,
> which is just a QoL wrapper for `Duration(float)`.

```csharp
Add("InitDamageDurationRemove")
	.Effect(new DamageEffect(5), EffectOn.Init)
	.Effect(new RemoveEffect(), EffectOn.Duration)
	.Duration(5);
```

### Refresh

Then we have `Refresh(RefreshType)` method. That makes either the interval or duration component refreshable.
Meaning that if a modifier gets added again to a unit, it will refresh the timer. This is most often used with the duration timer.

```csharp
Add("DamageOverTimeRefreshableDuration")
	.Interval(1)
	.Effect(new DamageEffect(5), EffectOn.Interval)
	.Effect(new RemoveEffect(), EffectOn.Duration)
	.Duration(5)
	.Refresh(RefreshType.Duration);
```

Calling `Refresh()` without any arguments will make the last time (interval/duration) component refreshable.

```csharp
Add("DamageOverTimeRefreshableDuration")
	.Interval(1)
	.Effect(new DamageEffect(5), EffectOn.Interval)
	.Effect(new RemoveEffect(), EffectOn.Duration)
	.Duration(5).Refresh();
```

### Stack

Then there's `Stack(WhenStackEffect whenStackEffect, float value, int maxStacks, int everyXStacks)`.
It's used for tracking how many times the modifier has been re-added to the unit, or other stacking logic.

`WhenStackEffect` tells the modifier when the stack action should be triggered: Always, OnMaxStacks, EveryXStacks, etc.  
`StackEffectType` tells the effect what to do when the stack action is triggered: 
Trigger it's effect, add to it's effect, or both or all, etc.

In this example we deal 5 damage every 1 second, but each time we add the modifier, we add 2 damage to the effect.
Resulting in 7 damage every 1 second with 1 stack. 9 with 2 stacks, etc.

```csharp
Add("StackableDamage_DamageOverTime")
	.Interval(1)
	.Effect(new DamageEffect(5, StackEffectType.Add), EffectOn.Interval)
	.Stack(WhenStackEffect.Always, value: 2);
```

### OneTimeInit & Aura

`OneTimeInit()` makes the modifier only trigger the init effect once, when it's added to the unit.
Any subsequent adds will not trigger the init effects, but refresh and stack effects will still work as usual.
This is very useful for aura modifiers, where we don't want to stack the aura effect.

For partial aura functionality, we can tell the recipe that it will trigger effects on multiple units at once with `Aura()`.

```csharp
Add("InitAddDamageBuff")
	.OneTimeInit()
	.Effect(new AddDamageEffect(5, true), EffectOn.Init)
	.Remove(1.05f).Refresh();
Add("InitAddDamageBuff_Interval")
	.Aura()
	.Interval(1)
	.Effect(new ApplierEffect("InitAddDamageBuff"), EffectOn.Interval);
```

### Meta-Effect

Effects can store meta effects, that will manipulate the effect values with optional conditions.
Meta effects, just like normal effects, are user-generated. Note that meta-effect can't have mutable state.
Any mutable state should be stored in the effect itself. Effects support many meta effects after each other.
Allowing for very complex interactions.

This example scales our 5 damage value based on the source unit's health * some multiplier.

```csharp
Add("InitDamageValueBasedOnHealthMeta")
	.Effect(new DamageEffect(5)
		.SetMetaEffects(new StatPercentMetaEffect(StatType.Health, Targeting.SourceTarget)), EffectOn.Init);
```

### Post-Effect

Post effects are effects that are applied after the main effect. And use the main effect as a source for their values.
Most common example of this is lifesteal, where we deal damage, and then heal for a percentage of the damage dealt.
Post effects also can't have any mutable state, and work fully in conjunction with meta effects.

This example deals 5 damage on init, and then heals for 50% of the damage dealt.

```csharp
Add("InitDamageLifeStealPost")
	.Effect(new DamageEffect(5)
    	.SetPostEffects(new LifeStealPostEffect(0.5f, Targeting.SourceTarget)) , EffectOn.Init);
```

### Apply & Effect Condition (checks)

Modifiers can have conditions, that will check if the modifier/target/source fulfills the condition before applying the modifier.
`ModiBuff.Units` has a few built-in conditions, and custom conditions are fully supported.
The common conditions are: cooldown, mana cost, chance, status effect, etc.

This example deals 5 damage on init apply, only if:
the source unit has at least 5 mana, passes the 50% roll, is not on 1 second cooldown, source is able to act (attack, heal), and target is silenced.

```csharp
Add("InitDamage_CostMana")
	.ApplyCost(CostType.Mana, 5)
	.ApplyChance(0.5f)
	.ApplyCooldown(1f)
	.ApplyCondition(LegalAction.Act)
	.EffectCondition(LegalAction.Silence)
	.Effect(new DamageEffect(5), EffectOn.Init);
```

### Order

Functions should be used in the operation order, for clarity.
This is optional, except for parameterless refresh functions, which should be called right after interval/duration.

```csharp
Add("Full")
    .OneTimeInit()
    .ApplyCondition(ConditionType.HealthIsFull)
    .ApplyCooldown(1)
    .ApplyCost(CostType.Mana, 5)
    .ApplyChance(0.5f)
    .EffectCondition(ConditionType.HealthIsFull)
    .EffectCooldown(1)
    .EffectCost(CostType.Mana, 5)
    .EffectChance(0.5f)
    .Effect(new DamageEffect(5), EffectOn.Init)
    .Effect(new DamageEffect(5), EffectOn.Stack)
    .Stack(WhenStackEffect.EveryXStacks, everyXStacks: 2)
    .Interval(1)
    .Effect(new DamageEffect(2), EffectOn.Interval)
    .Remove(5).Refresh()
    .Effect(new DamageEffect(8), EffectOn.Duration);
```

Each modifier should have at least one effect, unless it's used as a flag.

## Recipe Limitations

> Note that these limitations don't matter for 95% of the use cases.

* One Interval Component
* One Duration Component
* One Modifier Check for all effects
* Same Checks (cost, chance, cooldown) for all effects

## Adding Modifiers To Units

There's multiple ways to add modifiers to a unit.

For normal modifiers, the best approach is to use `IModifierOwner.TryAddModifier(int, IUnit)`.
By feeding the modifier ID, and the source unit.

For applier (attack, cast, etc) modifiers, `IModifierOwner.ModifierController.TryAddApplier(int, bool, ApplierType)` should be used.

Currently for aura modifiers it has to be implemented directly into the unit. An example of this can be found
in `CoreUnits.Unit.AddAuraModifier(int)`.

This is also the case for unit events, like `OnKill`, `OnAttack`, `WhenDeath`, etc.
Through `IEventOwner.AddEffectEvent(IEffect, EffectOnEvent)`.

## Effect

### Making New Effects

The library allows for easy creation of new effects.
Which are needed for using custom game-based logic.

Effects have to implement `IEffect`.
They can also implement `ITargetEffect` for event targeting owner/source, `IEventTrigger` to avoid event recursion, `IStackEffect` for
stacking functionality, `IStateEffect` for resetting runtime state.

For fully featured effect implementation, look at
[DamageEffect](https://github.com/Chillu1/ModiBuff/blob/master/ModiBuff/ModiBuff.Units/Effects/DamageEffect.cs)

### Applier Effect

Hands down, the most powerful effect is the ApplierEffect.  
It's used to apply other modifiers to units. While being able to use modifier logic, like stacks.  
This can create some very sophisticated modifiers:

```csharp
//WhenAttacked ApplyModifier. Every5Stacks this modifier adds a new ^ rupture modifier
AddEvent("ComplexApplier_OnHit_Event", EffectOnEvent.WhenAttacked)
    .Effect(new ActerApplierEffect("ComplexApplier_Rupture"));

//rupture modifier, that does DoT. When this gets to 5 stacks, apply the disarm effect.
Add("ComplexApplier_Rupture")
    .Effect(new DamageEffect(5), EffectOn.Interval)
    .Effect(new ApplierEffect("ComplexApplier_Disarm"), EffectOn.Stack)
    .Stack(WhenStackEffect.EveryXStacks, everyXStacks: 5);

//Disarm the target for 5 seconds. On 2 stacks, removable in 10 seconds, refreshable.
Add("ComplexApplier_Disarm")
    .Effect(new StatusEffectEffect(StatusEffectType.Disarm, 5, false, StackEffectType.Effect), EffectOn.Stack)
    .Stack(WhenStackEffect.EveryXStacks, everyXStacks: 2)
    .Remove(10)
    .Refresh();
```

The next one is very complex, but it shows the power of the ApplierEffect.  
Obviously the effects can be whatever we want. Damage, Stun, etc.

Add damage on 4 stacks buff, that you give someone when they heal you 5 times, for 60 seconds.  
To clarify:

* Player heals ally 5 times, gets buff
* Player attacks an enemy 4 times, gets damage buff
* Player buff gets removed after 60 seconds

```csharp            
//Apply the modifier to source (healer) WhenHealed                                   
AddEvent("ComplexApplier2_WhenHealed_Event", EffectOnEvent.WhenHealed)               
    .Effect(new ApplierEffect("ComplexApplier2_WhenHealed"), Targeting.SourceTarget);

//On 5 stacks, apply the modifier to self.                                           
Add("ComplexApplier2_WhenHealed")                                                    
    .Effect(new ApplierEffect("ComplexApplier2_OnAttack_Event"), EffectOn.Stack)     
    .Stack(WhenStackEffect.EveryXStacks, everyXStacks: 5)  
    .Remove(5).Refresh();   

//Long main buff. Apply the modifier OnAttack.                                       
AddEvent("ComplexApplier2_OnAttack_Event", EffectOnEvent.OnAttack)                   
    .Effect(new ApplierEffect("ComplexApplier2_WhenAttacked_Event"))                 
    .Remove(60).Refresh();  

AddEvent("ComplexApplier2_WhenAttacked_Event", EffectOnEvent.WhenAttacked)           
    .Effect(new ApplierEffect("ComplexApplier2_AddDamageAdd"), Targeting.SourceTarget
    .Remove(5).Refresh();   

//On 4 stacks, Add Damage to Unit source (attacker).
Add("ComplexApplier2_AddDamageAdd")                                                  
    .Effect(new ApplierEffect("ComplexApplier2_AddDamage"), EffectOn.Stack)                                 
    .Stack(WhenStackEffect.EveryXStacks, everyXStacks: 4)  
    .Remove(5).Refresh();      

//AddDamage 5, one time init, remove in 10 seconds, refreshable.                     
Add("ComplexApplier2_AddDamage")                                                     
    .OneTimeInit()                                                                   
    .Effect(new AddDamageEffect(5, true), EffectOn.Init)                             
    .Remove(10).Refresh();
```

## Modifier

Modifiers are the core backend part of the library, they are the things that are applied to entities with effects on certain actions.    
Ex. Init, Interval, Duration, Stack.  
You should **NOT** use the Modifier class directly, but instead use the recipe system.
Recipe system fixes a lot of internal complexity of setting up modifiers for you.

[//]: # (It's possible to use the Modifier class directly in cases where you'd want multiple interval/duration components.)

> Note: Currently it's impossible to use internal modifiers directly with the library, since the systems rely on the recipe system
> (pooling). This will be changed in the near future. Before the 1.0 release.

# FAQ

Q: ModiBuff.Units seems to use excessive casting in effects. Why not have a master unit interface?  
A: This was a tough solution to make custom user effects work with their own unit implementations.
And not force users to implement all methods for functionality, where it's not used.

Q: How do I make "insert mechanic from a game" in ModiBuff?  
A: First check [ModifierExamples.md](ModifierExamples.md). Then if it isn't there, ask about how to make it in issues, will make a better 
platform for discussion if needed.

Q: It's 100% not possible to make "mechanic from a game" in ModiBuff.  
A: If the mechanic is lacking internal ModiBuff functionality to work, and isn't an effect implementation problem, make an issue about it.
The goal of ModiBuff is to support as many unique mechanics as possible, that don't rely on game logic.

Q: My stack effect is not working, what's wrong?  
A: StackEffectType needs to be set in all: `IEffect` (ex. DamageEffect), `Recipe.Effect.EffectOn.Stack` and `Recipe.Stack()`  
Ex:

```csharp
Add("StackDamage")
    .Effect(new DamageEffect(5, StackEffectType.Effect/*<<THIS*/), EffectOn.Stack)
    .Stack(WhenStackEffect.Always);
```

# Examples

## Modifier/Effect/Recipe

For a big list of implementation examples, see [ModifierExamples.md](ModifierExamples.md)

## Full

> Note: The current examples are very very bare bones, a proper implementation with custom game logic will be added soon. 

[All examples](https://github.com/Chillu1/ModiBuff/tree/master/ModiBuff/ModiBuff.Examples)

[Simple solo](https://github.com/Chillu1/ModiBuff/tree/master/ModiBuff/ModiBuff.Examples/SimpleSolo)
example, of player unit fighting a single enemy unit

# Differences to ModiBuffEcs and Old

## [ModiBuffEcs](https://github.com/Chillu1/ModiBuffEcs)

ModiBuff has:

* No GC/allocations
* No ECS framework needed
* Worse iteration speed, 25_000 interval modifiers compared to 100_000 modifiers, 5ms update, average complexity modifiers
* Many more features

## [Old Modifier Library](https://github.com/Chillu1/ModifierLibrary)

ModiBuff has:

* **Much** better backend and design decisions
* Lightweight
* Smaller Codebase
* No GC/allocations
* Redesigned Improved API
	* [Recipes](#recipe)
	  vs [Properties](https://github.com/Chillu1/ModifierLibrary/blob/master/ModifierLibrary/Assets/Scripts/ModifierLibrary/ModifierPrototypes.cs#L126)
* Better iteration speed, 25_000 interval modifiers (from 500), 5ms update, average complexity modifiers
* Better memory management (1MB for 5_000 modifiers)
* No arbitrary name constraints

---

* Missing features:
	* Two status effects: taunt, confuse
	* Tags