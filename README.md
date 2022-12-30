<img src="Docs/LogoTest.png" width="500"/>
<img src="Docs/ModiBuff.png" width="500"/>

![Coverage](Docs/badge_linecoverage.svg)

- [What is this?](#what-is-this)
- [Features](#features)
- [Benchmarks](#benchmarks)
- [Installation](#installation)
- [Usage](#usage)
	- [Recipe](#recipe)
	- [Modifier](#modifier)
	- [Effect](#effect)
- [Differences to ModiBuffEcs and Old](#differences-to-modibuffecs-and-old)
- [When to use which library](#when-to-use-which-library)
- [FAQ](#faq)
- [Examples](#examples)
- [Limitations](#limitations)

# What is this?

This library was made to make a standarized powerful system that allows for manipulation of both effects and entities.

> Note: The library is currently in development, and it will most likely encounter breaking API changes.

> Note: The library currently depends on Unity Engine for logging and unit testing, but this is temporary, the 0.1 release will be
> completely engine agnostic.

# Features

* No GC/allocations (fully pooled with state reset)
* Low memory usage (1 MB for 5_000 modifiers)
* Fast effects [5_000 damage modifiers in 0.14ms](#benchmarks)
* Fast iteration [5_000 interval modifiers & 5_000 units in 0.95ms](#benchmarks)
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
	* Add stat (Damage, Heal)
	* And many more, see [the rest](ModiBuff/Assets/Scripts/Core/Components/Effect/Effects)
* Conditions (checks)
	* Chance 0-100%
	* Cooldown
	* Health/Mana cost
	* General:
		* Stat (health/mana/damage) >/=/< than X
		* Stat is full/empty
* Applier Modifiers
	* OnAttack
	* Cast
* [Event based Effects](#event-recipe)
	* WhenAttacked/Cast/Killed/Healed
	* OnAttack/Cast/Kill/Heal
* Fully revertible effects

# Benchmarks

Intel Core i7-4790 CPU @ 3.60GHz  
Preallocated Pools  
WarmupCount: 10  
MeasurementCount: 50  
N: 5_000

#### Add/Apply/Update Modifier table

| Library                                               | Apply<br/>InitDmg<br/>(1 unit) | Apply<br/>InitStackDmg<br/>(1 unit) | Update DoT*<br/>(5_000 units, N:1) |
|-------------------------------------------------------|--------------------------------|-------------------------------------|------------------------------------|
| ModiBuff (this)                                       | 0.14ms, 0 GC                   | 0.27ms, 0 GC                        | 0.95ms, 0 GC                       |
| [ModiBuffEcs](https://github.com/Chillu1/ModiBuffEcs) | ?                              | ?                                   | ?                                  |
| [Old](https://github.com/Chillu1/ModifierLibrary)     | ?                              | ?                                   | ?                                  |

#### New Modifier/Pool table

| Library                                               | New<br/>InitDmg | New<br/>DoT*  | DoT pool     | DoT pool<br/>reset return |
|-------------------------------------------------------|-----------------|---------------|--------------|---------------------------|
| ModiBuff (this)                                       | 4.07ms,  4 GC   | 10.3ms, 11 GC | 0.03ms, 0 GC | 0.16ms, 0 GC              |
| [ModiBuffEcs](https://github.com/Chillu1/ModiBuffEcs) | 4.00ms,  1 GC   | 5.80ms,  1 GC | X            | X                         |
| [Old](https://github.com/Chillu1/ModifierLibrary)     | 46.0ms, 45 GC   | 70.0ms, 63 GC | X            | X                         |

> Important: Non-pool benchmarks don't matter for ModiBuff, since it will only be slower when allocating the new modifiers in the pools.

Pooling in ModiBuff is 430X faster than original old version (because of pool & reset)  
But it's also much faster in cases of doing init/stack/refresh on an existing modifier (we don't create a new modifier anymore)  
ModiBuffEcs is a bit on the slow side for now, because we're creating the entities and their components, instead of reusing them, like in
the case of ModiBuff.

Mixed modifier = N of each. Ex. 100 instances * 58 recipes = 5_800 modifiers  
5_000 mixed modifiers = 1MB  
Modifier Recipes setup = 0.2ms  
Preallocating 5_800 mixed modifiers = 7ms  
So with 5_800 preallocated modifiers, the library will add 8ms to the game startup time.

*DoT = InitDoTSeparateDamageRemove

# Installation

Currently the library is not on NuGet or any other package manager. You can download the source code and add it to your project directly.

Specifically, you should get [Core](https://github.com/Chillu1/ModiBuff/tree/master/ModiBuff/Assets/Scripts/Core).
[Download Link](https://download-directory.github.io/?url=https%3A%2F%2Fgithub.com%2FChillu1%2FModiBuff%2Ftree%2Fmaster%2FModiBuff%2FAssets%2FScripts%2FCore)  
And [Core Units](https://github.com/Chillu1/ModiBuff/tree/master/ModiBuff/Assets/Scripts/CoreUnits), if you want an implementation example.

# Usage

## Recipe

ModifierRecipes are the high level API for creating modifiers, they use the builder pattern/method chaining/fluent interface to create
modifiers (without the need for calling a Finish/Complete method).

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
> Note: init will be triggered each time we try to add the modifier to the entity (unless we set `.OneTimeInit()`).

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

## Event Recipe

### Recipe Limitations

> Note that these limitations don't matter for 95% of the use cases.

* One Interval Component
* One Duration Component
* One Modifier Check for all effects
* Same Checks (cost, chance, cooldown) for all effects

## Effect

### Applier Effect

Hands down, the most powerful effect is the ApplierEffect.  
It's used to apply other modifiers to units. While being able to use modifier logic, like stacks.  
This can create some very sophisticated modifiers:

```csharp
//Disarm the target for 5 seconds. On 2 stacks, removable in 10 seconds, refreshable.
Add("ComplexApplier_Disarm")
	.Effect(new StatusEffectEffect(StatusEffectType.Disarm, 5, false, StackEffectType.Effect), EffectOn.Stack)
	.Stack(WhenStackEffect.EveryXStacks, value: -1, maxStacks: -1, everyXStacks: 2)
	.Remove(10)
	.Refresh();
//rupture modifier, that does DoT. When this gets to 5 stacks, apply the disarm effect.
Add("ComplexApplier_Rupture")
	.Effect(new DamageEffect(5), EffectOn.Interval)
	.Effect(new ApplierEffect("ComplexApplier_Disarm"), EffectOn.Stack)
	.Stack(WhenStackEffect.EveryXStacks, value: -1, maxStacks: -1, everyXStacks: 5);
//WhenAttacked ApplyModifier. Every5Stacks this modifier adds a new ^
AddEvent("ComplexApplier_OnHit_Event", EffectOnEvent.WhenAttacked)
	.Effect(new ActerApplierEffect("ComplexApplier_Rupture"));
```

## Modifier

Modifiers are the core backend part of the library, they are the things that are applied to entities with effects on certain actions.    
Ex. Init, Interval, Duration, Stack.  
You should **NOT** use the Modifier class directly, but instead use the recipe system.
Recipe system fixes a lot of internal complexity of setting up modifiers for you.  
It's possible to use the Modifier class directly in cases where you'd want multiple interval/duration components.

# Differences to ModiBuffEcs and Old

## [ModiBuffEcs]((https://github.com/Chillu1/ModiBuffEcs))

ModiBuff has:

* No GC/allocations
* No ECS framework needed
* Worse iteration speed, 25_000 interval modifiers compared to 100_000 modifiers, 5ms update, average complexity modifiers

---

* More features
	* ...

## [Old Modifier Library]((https://github.com/Chillu1/ModifierLibrary))

ModiBuff has:

* **Much** better backend and design decisions
* Lightweight
* Smaller Codebase
* No GC/allocations
* Redesigned Improved API
	* [Recipes](#recipe)
	  vs [Properties](https://github.com/Chillu1/ModifierLibrary/blob/master/ModifierLibrary/Assets/Scripts/ModifierLibrary/ModifierPrototypes.cs#L126)
* Better iteration speed, 25_000 interval modifiers (from 500), 5ms update, average complexity modifiers
* Better memory managment (1MB for 5_000 modifiers)
* No arbitrary name constraints

---

* Less features, missing:
	* Aura
	* Two status effects: taunt, confuse
	* Tags

# When to use which library

## ModiBuff

Smmary: Very optimized, no GC, great featureset.  
Ex. games: Rimworld, Hades, Binding of Isaac, Tiny Rogues, Enter the Gungeon  
Ex. genres: small arpg, small rts, pve, colony sim

ModiBuff is the best choice 95% of the time. It's fast, lightweight, deeply redesigned core, has no GC/allocations, and is very easy to
use.  
It's also very well tested for most scenarios.

## ModiBuffEcs

Summary: Fastest iteration, small featureset, needs ecs framework.  
Entities: Solo vs Thousands, or Thousands vs Thousands.  
Ex. games: Path of Exile, Diablo, StarCraft, Warcraft  
Ex. genres: arpg, rpg, rts, pve

ModiBuffEcs is a good choice if you don't care about about having a lot of features, and if your game will have hundreds of thousands of
units.
Or just if you want to use it with an ecs framework.

## Original

Summary: Not optimized, amazing featureset.  
Entities: Solo vs Solo, Solo vs Party, Party vs Party  
Ex. games: dota, witcher 3  
Ex. genres: moba, arena, duel

Only choose original if you need the deep featureset, AND you don't expect to have more than 100 units in the game at the same time, all
using/applying 5 modifiers each frame.
If you're making a moba or a small PvP arena game, you can use the original library. That being said, **ModiBuff is a better choice for the
vast majority of games.**

# FAQ

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

[//]: # (TODO Recipe examples for usual game mechanics)

[//]: # (DoT, InitDoTSeparateDmg, OnXStacks, StackableDamage, StunEverySecondFor0.2Seconds)

[//]: # ("Absoultely crazy modifiers": applying appliers on events, X stacks, etc)

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

[All examples](https://github.com/Chillu1/ModiBuff/tree/master/ModiBuff/Assets/Examples)

[Simple solo](https://github.com/Chillu1/ModiBuff/tree/master/ModiBuff/Assets/Examples/SimpleSolo)
example, of player unit fighting a single enemy unit

# Limitations

Currently the system is designed to max have **one** modifier type per Unit.  
This can be configured by using special containing logic in ModifierController, but then you'll need to keep track of all the instances, and
init/stack/refresh them.