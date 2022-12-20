# Notes

5ms± update, avg complexity modifiers
ModifierLibrary Original: 500 modifiers
ModifierLibraryLite : 10_000 (goal)
ModifierLibraryEcs : 100_000 (400_000 time modifiers)

Goals of the libraries:
* Original: Small solo/party vs solo/party games, very feature rich, not optimized (GC/allocations)
  * Ex. games: binding of isaac, tiny rogues, gungeon, dota, witcher 3
  * Ex. genres: moba, arena, duel
* Lite: Solo vs hunreds, or hundreds vs hundreds. Much faster, less features. No GC/allocations (pooled).
  * Ex. games: Rimworld,
  * Ex. genres: small arpg, small rts, pve, colony sim
* Ecs: Solo vs thousands, or thousands vs thousands. Very fast iteration, much less features, optimized, needs ecs
  * Ex. games: PoE, Diablo
  * Ex. genres: arpg, rpg, rts, pve

## Differences to original

* Much better backend and design decisions
* Lightweight
* Smaller Core
* No GC/allocations
* Improved API
* Only cloning statefull objects (less memory, 20 MB for 100_000 modifiers, 7 MB for 100_000 simple modifiers)

* Less features, missing:
  * Unit event (on attacked, on hit, on killed, when attacking...)
  * Aura
  * Condition effects (when low health)
  * Some status effects (taunt, confuse)
  * Fully functioning revertible modifiers

### When to chose which library

#### Benchmarks:
TODO bench Iteration 5000 InitDoTDamageRemove
TODO Put benches from ##Benches here

#### Lite
Smmary: Very optimized, no GC, good featureset.  
Ex. games: Rimworld    
Ex. genres: small arpg, small rts, pve, colony sim

Lite is the best choice 80% of the time. It's fast, lightweight, deeply redesigned core, has no GC/allocations, and is very easy to use.
It's also very well tested for most scenarios.

#### Ecs
Summary: Fastest iteration, small featureset, needs ecs framework. Entities: Solo vs Thousands, or Thousands vs Thousands.  
Ex. games: PoE, Diablo  
Ex. genres: arpg, rpg, rts, pve

Ecs is a good choice if you don't care about about having a lot of features, and if your game will have hundreds of thousands of units.
Or just if you want to use it with an ecs framework.

#### Original
Summary: Not optimized, amazing featureset, entities: Solo vs Solo, Solo vs Party, Party vs Party  
Ex. games: binding of isaac, tiny rogues, gungeon, dota, witcher 3  
Ex. genres: moba, arena, duel

Only choose original if you need the deep featureset, AND you don't expect to have more than 100 units in the game at the same time, all using/applying 10 modifiers each frame.
If you're making a moba or a small PvP arena game, you can use the original library. That being said, Lite is a better choice for the vast majority of games.

## Benches

Pools: preallocated

|      | InitDmg, N:5k | InitDoTSeparateDamageRemove, N:5k | InitDoTSeparateDamageRemove pool, N:5k | InitDoTSeparateDamageRemove pool reset return, N:5k |
|------|---------------|-----------------------------------|----------------------------------------|-----------------------------------------------------|
| Lite | 0.74ms, 1 GC  | 2.84ms, 4 GC                      | 0.12ms, 0 GC                           | 0.25ms, 0 GC                                        |
| Ecs  | 4.00ms, 1 GC  | 5.80ms, 1 GC                      | NaN                                    | NaN                                                 |
| Orig | 46 ms, 45 GC  | 70 ms, 63 GC                      | NaN                                    | NaN                                                 |

Pooling in lite is 280X faster than original (because of pooling & reset)
But it's also faster in case of doing init/stack/refresh on an existing modifier (we don't create a new modifier)
Ecs is a bit on the slow side because we're creating the entities and their components, instead of reusing them, like in the case of lite.

Lite InitDmg (not cloning any components, no state)

### Findings

* Caching and feeding one level (ex. targets to a component) is the same speed as cloning and caching the whole component (iteration-wise)
* Having state in components and effects is fine. Because we only clone on allocate (pool), and having 2x memory usage for 10k modifiers is not a big deal.
* Interface IEffects are as fast as delegates

## Temp Notes

Logo creation.
Keywords: modifier, buff, debuff.

Half a gear icon left-right to top, inside a muscle icon. Poison drop (green drop)?

Name:
modifier library ecs buff debuff mod lib gear tech modify
Ideas:
* Modify (common)
* Modifyable (ify)
* DeBuffs (common)
* DeMod (used)
* ModiBuff/MoDeBuFf/ModeBuff
* ModBuff (used)
* Buffodify

Initinterval stackable dmg. Same damage effect, adds dmg on stack
Stackeffect being a parent of ieffects (try to make IEffects stateless)

RemoveEffect problem:
We need to add revertible effect clones after making them to remove effect.
Also we need a clone of remove effect for every new modifier instance, since itself will hold references to clones effects.
Problem is, that the remove effect is fed on Finish, and it will be the non-cloned version, with references to the non-cloned effects.

Not using Components in ModifierRecipe, but arrays of effects, and component data.
Then when cloning we have an easier time? Because we could clone the revertable effects once, and feed them?
The responsibility from cloning would go away from Modifier ctr.

How to know when to add a modifier as an act (attack/cast) applier, and when as a normal modifier?

Applier: ApplyExtraDamageToTarget on cast, with 5 mana cost

Stateful appliers. how and why?
Ex. ApplyOnHit. But we can do this without extra Applier logic?

EffectOnEvents setup works differently than ModifierRecipe. Should probably have a ModifierEventRecipe setup instead.
Ex. always on init, always revert effect when modifier is remvoed?

EffectOn Event in ModifierRecipe?
Or modifiers don't know about events, and we do the add/remove manually?

We could merge interval & duration to one component, by having a separate duration timer (that updates every interval amount?)
But then we're kind of forcing interval to be a multiple of duration.

Having state is actually fine in effects/components? Because we allocate modifiers on init and then every so often.
So it will slightly slow down allocations (but we can allocate a lot on init to counteract that). And increase memory usage. By a fair amount (which doesn't really matter)

What's better, cloning init component, or feeding the target through the function?

Problem: one remove effect for multiple modifiers.

We can reverse the effect order with making reverse effects. Or we could have a wrapper instead. Trade-offs...

Entire DurationComponent needs to be cloned because of _timer & _targetComponent. If we off-load them, we won't need to clone it.
Same with IntervalComponent.

How to design appliers?
Apply can happen on attack, cast. Or "normal" functions? Do we want to apply modifiers ex. on interval? Probably?
Should appliers be set on init in recipe? Or chosen to be appliers on add?

Should appliers be stored as effects? Or in ModifierController?
As effects gives more flexibility?

Array mapping of modifier's kinda hard. We could have a slot for each modifier type, then either iterate through all of these slots (skip nulls).
Or have an array of modifier ids that we currently have, and iterate this way.
Or double array mapping?

Translating string IDs to int on init. Then only using the modifier ID's internally. And string names in init, testing. 

Check components:
Cooldown has state (timer)
Mana use doesn't have state
Chance doesn't have state

How to avoid having stack state in effects? Have it outside of the effect, and feed it each time?
This won't work if we're calling it from Init/Time/Non-stack components
We could store the stack information in the modifier, and feed it each time there.
Or we could store the effect data outside? Then we only clone & reset that data, and not the whole effect.

When cloning, all components should use the same instance of TargetComponent.
Try to keep as mmuch logic stateless as possible.
Only feed the target component after creating the modifier, making it so we don't have to clone it. And feed it to everyone automatically

How to fix the state problem?
* Clone modifier components
* Make modifier components structs (they're copied automatically), can be bad with boxing (throw out interfaces then? But can't for ITimeController)

How to do remove logic in RemoveEffect.
Reference to Modifier?

Who should own the targetComp?
Init/Time, or Effect?
Effect shouldn't care about it?

20_000 basic timers in one array: 5ms

Effect on: GetHit, OnHit, Interval
Effect type: damage, heal

Basic functionality:
* Damage, Heal, Effects
* Time Effects
* Interval Effects
* 