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
Creating a simple modifier is 13 times faster, and only allocated new memory for the modifier object (which can easily be pooled)
Also ecs is a bit on the slow side because we're creating the entities and their components, instead of reusing them, like in the case of lite.
ECS:
new modifier from recipe (InitDamage). 1_000 iters = 2.74ms, 1 GC
Lite:
new modifier from recipe (InitDamage). 1_000 iters = 0.26ms. 1 GC. bugged approch bench
Orig:
new modifier from properties (InitDamageApplier). 1_000 iters = 3.35ms, 25 GC

## Temp Notes

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