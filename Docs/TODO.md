# TODO

## General

* Stacking/modifier state (added damage, etc)

* Basic pool bench
  * Test arraypool/memorypool
  * Array instead of stack bench

* Don't make a new modifier before we know we can apply it (improves performance). Because we might just stack/init/refresh the current one.

## Tests

### Benchmarks
*

### ModifierLibraryOld unit tests

* Check
  * Mana cost
  * Mana cost not enough

* Two effects
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
  * Act, Silence, Stun, Confuse, Disarm, Taunt

## Misc/low prio
* Standarized modifier unit tests, fitting both ModifierLibraryNew and ModifierLibraryEcs
* Appliers with state