# Notes

5ms± update, avg complexity modifiers
ModifierLibrary Original: 500 modifiers
ModifierLibraryLite : 10_000 (goal)
ModifierLibraryEcs : 100_000 (400_000 time modifiers)

Goals of the libraries:
* Original: Small solo/party vs solo/party games, very feature rich, not optimized
  * Ex. games: binding of isaac, tiny rogues, gungeon, dota, witcher 3
  * Ex. genres: moba, arena, duel
* Lite: Solo vs hunreds, or hundreds vs hundreds. Much faster, less features
  * Ex. games: Rimworld,
  * Ex. genres: small arpg, small rts, pve, colony sim
* Ecs: Solo vs thousands, or thousands vs thousands. Very fast, much less features, optimized, needs ecs
  * Ex. games: PoE, Diablo
  * Ex. genres: arpg, rpg, rts, pve

## Goals

* Lightweight
* Basic++ effects
  * Basic effect manipulation (stack, refresh)

## Benches

Pools: preallocated

|      | InitDmg, N:1k | InitDmg, N:5k | InitDoTSeparateDamageRemove, N:5k | InitDoTSeparateDamageRemove pool, N:5k | InitDoTSeparateDamageRemove pool reset return, N:5k |
|------|---------------|---------------|-----------------------------------|----------------------------------------|-----------------------------------------------------|
| Lite | 0.20ms,  1 GC | 0.93ms, 1 GC  | 3.48ms, 4 GC                      | 0.32ms, 0 GC                           | 0.75ms, 0 GC                                        |
| Ecs  | 2.74ms,  1 GC |               |                                   |                                        |                                                     |
| Orig | 3.35ms, 25 GC |               |                                   |                                        |                                                     |

Creating a simple modifier is 16 times faster, and only allocated new memory for the modifier object (which can easily be pooled)
Also ecs is a bit on the slow side because we're creating the entities and their components, instead of reusing them, like in the case of lite.

Lite InitDmg (not cloning any components, no state)

## Temp Notes

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