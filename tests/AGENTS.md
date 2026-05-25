# AGENTS.md for Tests

## General Rules

- Please do not use mocking frameworks like Moq or NSubstitute for test doubles, use hand-crafted test doubles instead.
- Do not write nested test classes. All tests should reside in a class which is directly placed in a namespace.
- Use FluentAssertions instead of xunit's `Assert` class.
- Prefer Sociable Tests instead of Solitary Tests. Create as much test coverage as possible by calling higher level production APIs.
