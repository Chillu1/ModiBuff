# TODO

## General

* Basic pool bench
  * Test arraypool/memorypool
  * Array instead of stack bench

* RemoveEffect reference is shared between modifiers. Probably refactor the remove approach
* Stacking
  * State in effects, how?
* Check components (chance, cooldown, mana use)
* Targeting (deal damage to enemy, heal self)

* Don't make a new modifier before we know we can apply it (improves performance). Because we might just stack/init/refresh the current one.
* Have two ways to apply a modifier? One for modifier's with a state? One for without only through ID?
* How many time modifiers can we run?
* Take modifier ideas & test modifiers from original library. Import their mechanics in an improved way here

## Tests

### Benchmarks
*

### ModifierLibraryOld unit tests
* Thorns on hit

* Check
  * Health cost
  * Health cost not lethal
  * Mana cost
  * Mana cost not enough
  * Cooldown

* Two effects
* Aura

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