# Guard Clause Consolidation

## Rationale

BrilliantMessaging currently repeats private guard helpers and inline precondition checks across Core, OpenTelemetry, and the transport projects. Consolidate these checks behind a curated, public source export from a compatible Light.GuardClauses checkout, compiled once into BrilliantMessaging.Core. The resulting API should live under `BrilliantMessaging.GuardClauses` and its generated sub-namespaces, giving BrilliantMessaging, its transports, and third-party extensions the same guard-clause surface without adding a runtime package dependency.

The generated surface should embrace the Light.GuardClauses exception taxonomy. BrilliantMessaging is not stable yet, so consistent, purpose-specific exceptions and reduced duplication are more valuable than preserving the current mixture of base exception types.

## Acceptance Criteria

- [ ] A committed single-file Light.GuardClauses export is compiled into BrilliantMessaging.Core and exposes public types under `BrilliantMessaging.GuardClauses`.
- [ ] Generated exception and exception-factory types use `BrilliantMessaging.GuardClauses.Exceptions` and `BrilliantMessaging.GuardClauses.ExceptionFactory`.
- [ ] The export is generated for `netstandard2.0` from a clean, compatible Light.GuardClauses checkout, records the actual source version and Git commit, and contains only the selected assertion roots and their transitive dependencies.
- [ ] A repository-owned generation script passes the complete transformation configuration as command-line arguments and does not create or modify `settings.local.json` in the Light.GuardClauses checkout.
- [ ] The generation script accepts an explicit Light.GuardClauses repository path and otherwise defaults to a `Light.GuardClauses` sibling of the BrilliantMessaging repository root.
- [ ] Normal restore, build, pack, and consumer workflows do not reference the Light.GuardClauses NuGet package or require the Light.GuardClauses repository.
- [ ] Core, OpenTelemetry, the in-memory transport, the NATS transport, and the RabbitMQ transport consume the public guard API from BrilliantMessaging.Core.
- [ ] Caller argument expressions and nullable flow annotations work on `netstandard2.0` without generating attribute types already supplied to Core by Polyfill.
- [ ] Inline argument, state, and disposal guards are replaced with the corresponding generated assertions while operational failures, aggregate topology validation, branching predicates, and exhaustive switch failures remain explicit.
- [ ] Duplicated private helpers such as `RequireText`, `RequirePositive`, `EnsureRoutingKey`, and `ThrowIfDisposed` are removed when their only purpose is covered by the consolidated assertions.
- [ ] The exactly-one-present routing-key validation is expressed locally with `Check.InvalidArgument`; no generic xor assertion is added to the public guard surface.
- [ ] Existing XML documentation and tests are updated for the Light.GuardClauses exception taxonomy and any caller-visible breaking changes.
- [ ] The generated file is treated as generated code and is not edited by hand.
- [ ] Automated tests need to be written, following the repository test rules.
- [ ] All test projects pass when run directly with the Microsoft Testing Platform runner.
- [ ] Release builds stay warning-clean with `TreatWarningsAsErrors`.

## Technical Details

Place the generated file at `src/BrilliantMessaging.Core/GuardClauses/BrilliantMessaging.GuardClauses.g.cs` and compile it only in Core. Every transport and BrilliantMessaging.OpenTelemetry already references Core and should import `BrilliantMessaging.GuardClauses` directly; no copy of the generated source should be compiled into those assemblies. BrilliantMessaging.Abstractions remains unchanged because it does not depend on Core and currently has no guard clauses requiring this surface.

Run `Light.GuardClauses.SourceCodeTransformation` from a compatible Light.GuardClauses checkout as an explicit maintainer operation. Add a small generation script under `scripts` and make it the BrilliantMessaging-specific configuration source of truth. The script should accept the Light.GuardClauses repository path as an optional argument. When omitted, resolve the default as `../Light.GuardClauses` relative to the BrilliantMessaging repository root, not relative to the caller's current working directory; this works whether the local BrilliantMessaging folder is named `BrilliantMessaging` or `BMF`. Validate the expected source-export project, source folder, configuration catalog, and every selected assertion and option before generating. Require the Light.GuardClauses worktree to be clean so its commit identifies the exact input. Do not require an exact Light.GuardClauses version; compatibility is determined by the capabilities used by the generation script. Preserve the exporter's version header and add the source commit to the generated provenance comment.

Pass all transformation settings as command-line overrides to `dotnet run --project <Light.GuardClauses>/tools/source-export/Light.GuardClauses.SourceCodeTransformation -- ...`. Do not create, overwrite, or depend on the upstream repository's ignored `settings.local.json`. The script should read the assertion catalog from the upstream committed `settings.json` and pass an explicit `Include` value for every entry, plus the selected per-entry `IncludeExceptionFactoryOverload` values, so a default-enabled or newly added assertion cannot enter the BrilliantMessaging export accidentally. Pass the upstream `src/Light.GuardClauses` folder as `SourceFolder`, target a temporary output file, and replace the committed Core file only after generation and the exporter's matching build validation succeed.

Do not invoke generation from the regular MSBuild graph; consumers and contributors should build the committed output without the sibling checkout or transformation project.

Configure the exporter with `TargetFramework` set to `NetStandard2_0`, `ChangePublicTypesToInternalTypes` set to `false`, `BaseNamespace` set to `BrilliantMessaging.GuardClauses`, and `IncludeVersionComment` enabled. Remove JetBrains contract annotations and validated-null annotations from the export. Do not emit code-analysis nullable attributes or `CallerArgumentExpressionAttribute`, because Core's private Polyfill dependency already supplies those types; retain their usages so nullable flow analysis and inferred parameter names continue to work.

Enable `AssertionWhitelist` and explicitly disable every catalog entry that is not selected, because omitted entries default to enabled. Use these assertion roots:

- `InvalidArgument`
- `InvalidOperation`
- `MustBeAssignableTo`
- `MustBeConcreteClass`
- `MustBeIn`
- `MustBeGreaterThanOrEqualTo`
- `MustBeOfType`
- `MustBePositive`
- `MustBePositiveOrInfinite`
- `MustBeUri`
- `MustBeValidEnumValue`
- `MustNotBe`
- `MustNotBeDefault`
- `MustNotBeDefaultOrEmpty`
- `MustNotBeEmpty`
- `MustNotBeNegative`
- `MustNotBeNull`
- `MustNotBeNullOrEmpty`
- `MustNotBeNullOrWhiteSpace`
- `MustNotBeNullReference`
- `MustNotContainNull`
- `MustNotStartWith`
- `ObjectDisposed`

Let source reachability retain required helpers, exception types, and factory members rather than selecting those dependencies manually. Keep exception-factory overloads only for assertions used to preserve a domain or state exception, initially `MustBeOfType`, `MustBeUri`, `MustNotBeDefault`, `MustNotBeEmpty`, `MustNotBeNull`, and `MustNotBeNullOrWhiteSpace`; disable factory overloads for the other roots unless migration reveals a concrete call site. This keeps the public generated surface deliberate while still supporting `CloudEventMetadataException` and null-result `InvalidOperationException` cases.

Use the assertions as value-returning expressions where this simplifies constructors and assignments. Null references map to `MustNotBeNull` or, for unconstrained/reference-capable generic values, `MustNotBeNullReference`; text maps to `MustNotBeNullOrWhiteSpace`; immutable-array and collection invariants map to `MustNotBeDefaultOrEmpty`, `MustNotBeNullOrEmpty`, and `MustNotContainNull`. Numeric constraints map to `MustBePositive`, `MustBePositiveOrInfinite`, `MustNotBeNegative`, `MustBeGreaterThanOrEqualTo`, or `MustBeIn`. Enum, type, URI, reserved-prefix, and forbidden-value checks map to their named assertions. State and disposal checks map to `InvalidOperation` and `ObjectDisposed`.

Use `MustBeAssignableTo` followed by `MustBeConcreteClass` for handler and service implementation types where both relationships matter. Preserve predicate-style `IsAssignableFrom`, `IsNullOrWhiteSpace`, and similar checks when they classify values or control normal branching rather than reject invalid input.

Keep compound and cross-parameter rules readable at their call sites with `Check.InvalidArgument`. In particular, the RabbitMQ routing-key value and routing-key factory must have different presence states; testing equality of those presence states is the invalid condition. Do not introduce a general exactly-one-present assertion or a RabbitMQ-specific assertion into Core.

Adopt the generated exception hierarchy as part of the public Core API. For example, empty and whitespace strings, invalid enum values, invalid URIs, empty collections, and null collection entries should throw their corresponding types from `BrilliantMessaging.GuardClauses.Exceptions` instead of preserving the previous base `ArgumentException`, `ArgumentOutOfRangeException`, or `ArgumentNullException` inconsistencies. Preserve domain exceptions through configured factory overloads only where the failure describes messaging semantics rather than an ordinary caller precondition.

Tests should primarily exercise migrated public builders and runtime entry points so parameter names and exception behavior are covered sociably. Add focused Core tests proving that the generated `Check` API and generated exception types are public from a separate assembly, caller argument expressions capture the original expression, selected custom exception-factory overloads work, and representative null, text, numeric, enum, type, URI, state, and disposal guards behave correctly. Do not duplicate the full upstream Light.GuardClauses test suite.
