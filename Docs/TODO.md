# TODO

## General

* Rethink the whole recipe effects process, being able to use the same effect clone instance for multiple components & remove.
  * Create modifiers that pass the unit tests for states, and remove effect, and same effect multiple places uses stacks.

* Stacking/modifier state (added damage, etc)
* Reset stacks state
* Stack timer? Resets stacks after x seconds, can be refreshed

* Basic pool bench
  * Test arraypool/memorypool
  * Array instead of stack bench

## Tests
* StatusEffectEffect.Revert

### Benchmarks
*

### ModifierLibraryOld unit tests

* Aura

* Effects on events
  * Thorns on hit
  * AddDamageOnKill
  * DealDamageOnDeath
  * HealSelfWhenHealingAction
  * AttackYourselfOnHit

* Extra damage on low health

* Automatic casting

* Multitarget add damage, reverible

* Stack
  * Stacking damage
  * Silence on X stacks

* Status resistance

* Status effects
  * Confuse, Taunt

## Misc/low prio
* Standarized modifier unit tests, fitting both ModifierLibraryNew and ModifierLibraryEcs
* Appliers with state