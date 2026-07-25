# Root Agents.md

Brilliant Messaging is a lightweight framework for modern cloud-native applications. It consists of an asynchronous messaging system and a saga framework.

## Implementation rules

Plans typically have acceptance criteria with check boxes. Check each box when you are finished with the corresponding criterion.

## General Rules for the Code Base

- Implicit usings or global usings are not allowed - use explicit using statements for clarity.
- The library is not published in a stable version yet, you can make breaking changes.
- `<TreatWarningsAsErrors>` is enabled in Release builds, so your code changes must not generate warnings.
- Prefer `public` over `internal` when designing types. Users of the library should have access to the same APIs as we do. Hide lower level APIs in plain sight, see https://blog.ploeh.dk/2015/09/21/public-types-hidden-in-plain-sight/.

## Code Analysis

- After finishing a plan, run `./scripts/inspect-code.sh SUGGESTION`. It restores the local ReSharper command line tool, runs solution-wide analysis against the Release configuration, and writes an XML report to `artifacts/inspect-code/inspection-report.xml`. The severity argument is optional and defaults to `WARNING`.
- Address or report findings introduced by your changes. The report also contains pre-existing findings, so compare them against `main` before acting on them.

## Plan Rules

Read ./ai-plans/AGENTS.md for details on how to write plans.

## Test Rules

Read ./tests/AGENTS.md for details on how to write tests.

## Here is Your Space

If you encounter something worth noting while you are working on this code base, write it down here in this section. Once you are finished, I will discuss it with you and we can decide where to put your notes.
