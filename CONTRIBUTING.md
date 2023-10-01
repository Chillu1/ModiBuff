# Contributing

## Reporting Bugs

First make sure that the bug is not already reported in [Issues](https://github.com/Chillu1/ModiBuff/issues?q=is%3Aissue+is%3Aopen+label%3Abug).  
If it's not, then make a [new bug issue](https://github.com/Chillu1/ModiBuff/issues/new?assignees=&labels=bug&projects=&template=bug_report.md&title=)  
Try to give as much context as possible, and explain exactly what was expected and what happened instead.

## Contributing Modifier Implementations

When contributing modifier implementations one should first check [ModifierExamples.md](ModifierExamples.md) to see if the modifier 
implementation is already present.

It's important to use use recipes if possible, and avoid using manual modifier generation.
The reason behind this is that recipes are much easier to use and understand.
Manual modifier generation should only be used if the modifier is too complex to be made with recipes
or it's impossible to make with recipes.

One should comment what the modifier does in two sentences, and maybe where the mechanic comes from (if applicable).
More sophisticated modifiers can have a longer explanation.

## Contributing Documentation

Currently the main documentation is the [README](README.md), [ModifierExamples.md](ModifierExamples.md) files, and code comments.
But more documentation is needed, mostly when it comes to game samples/examples. A wiki page might be made if the README gets too big.

## Contributing Code

Before a PR can be merged, it needs to pass all the tests.

It's important to understand the distinction between ModiBuff.Core and ModiBuff.Units.
All Core logic should have as little opinions as possible (ideally none) on game logic specifications.
This means that all Core code should not assume anything, and if we want to support some specific logic that requires some specifics,
those specifics should be fed through interfaces or generics. An example of this is
[EventEffect](https://github.com/Chillu1/ModiBuff/blob/133e524e8a51431d273765294b79c5030ec054be/ModiBuff/ModiBuff/Core/Components/Effect/EventEffect.cs#L3)
which needs a generic type for what kind of events the unit might have.

A common case of this is user supplied values, we shouldn't assume that the value is of specific type (float, int, etc).
But again instead use generics.

### Getting Started

A lot of ModiBuff issues are hard to implement, and require a lot of time and effort.
So for easier issues, one should look at the [good first issues](https://github.com/Chillu1/ModiBuff/contribute) and pick one of those.

### Style Guide

We use the default C# style guide for the most part.
Make sure to clean any whitespace, and format the code before submitting a PR.

### Submitting Code

We currently use standard GitHub pull requests to master.
This might be a subject to change in the future.

### Errors and exceptions

Exceptions should be used for exceptional cases, and not for control flow.
Meaning that an exception should be thrown if the code is in a state that it shouldn't be in,
that will break the program/ModiBuff. 
There are some exceptions (to exceptions), like having invalid state in recipes should not throw exceptions, even if it breaks the recipe.

In general errors should be handled by writing them to the `Logger`.
Otherwise the code should not fail.

### New Modifier Logic

**Before** coding new modifier logic, one should make an issue with
[this](https://github.com/Chillu1/ModiBuff/issues/new?assignees=&labels=enhancement%2C+modifier+logic+request&projects=&template=modifier-feature-request.md&title=)
template about it.
It's possible that the logic is opinionated, and is actually more of a game logic feature than a ModiBuff feature.
These will not be accepted towards ModiBuff.Core,
but can maybe be shown as examples in ModiBuff.Units or [ModifierExamples.md](ModifierExamples.md).

> Note that the approach below is not required, but it's highly recommended.

When adding new modifier logic, one should start first by creating the modifier manually and using the `ManualModifierGenerator`.
It's **much** easier to better understand the functionality and design problems when you skip the QoL recipe system.
The recipe system adds a lot of extra mental overhead.

The usual workflow is by making the modifier manually in a integration test, then using that modifier on a unit or multiple.
And asserting that the resulting state is correct.
This confirms that the functionality is behaving as expected from the start position to the end position.
Only after that works, you could start thinking about how to add the modifier recipe equivalent functionality.

It's common that new modifier logic requires some new code in ModiBuff.Units as well.
And if that's the case, we usually need to make a new interface in Core that specifies the functionality.
And implement that interface in Units.Unit.

#### Tests

**When adding new logic, tests of scenarios need to be made and must pass.**
If you're having troubles with making them, mention @Chillu1 in the PR for help.

### Changing Units Logic

Sometimes the ModiBuff.Units.Unit implementation is limiting, or we need a different approach to test some ModiBuff.Core functionality.
For example

[MultiInstanceStatusEffectController](https://github.com/Chillu1/ModiBuff/blob/133e524e8a51431d273765294b79c5030ec054be/ModiBuff/ModiBuff.Units/MultiInstanceStatusEffectController.cs#L12)
and
[StatusEffectController](https://github.com/Chillu1/ModiBuff/blob/133e524e8a51431d273765294b79c5030ec054be/ModiBuff/ModiBuff.Units/StatusEffectController.cs#L9).
The old status effect controller wasn't aware that there could be multiple status effects that work on the same types.
So it just chose the higher duration ones, and was able to revert not it's own status effects.
So to fully test the functionality of unique status effects, `Unit` needed a new implementation of the controller.
Note that `MultiInstanceStatusEffectController` is 10X slower than `StatusEffectController`, but that's fine because such implementation 
should be used if needed.

When changing units logic, you need to make sure all the previous tests still pass.