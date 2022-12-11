# TODO

* Currently all the components are being reused in a new modifier object from recipe. This is bad because state will transfer over.
* Don't make a new modifier before we know we can apply it (improves performance). Because we might just stack/init/refresh the current one.
* Have two ways to apply a modifier? One for modifier's with a state? One for without only through ID?
* How many time modifiers can we run?
* Take modifier ideas & test modifiers from original library. Import their mechanics in an improved way here