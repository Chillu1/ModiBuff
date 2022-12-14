# TODO

## General

* Better appliers (attack, cast)

* Basic pool bench
  * Test arraypool/memorypool
  * Array instead of stack bench

* RemoveEffect reference is shared between modifiers. Probably refactor the remove approach
* Stacking
  * State in effects, how?
* Check components (cooldown)
* Targeting (deal damage to enemy, heal self)

* Don't make a new modifier before we know we can apply it (improves performance). Because we might just stack/init/refresh the current one.

## Tests

### Benchmarks
*

### ModifierLibraryOld unit tests

* Check
  * Mana cost
  * Mana cost not enough
  * Cooldown

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
* Better appliers (non-attack only)
* Appliers with state