# ADR-004: Testing Stack

## Status

Accepted

## Context

PRD-001 requires both automated unit tests and automated integration tests
that deterministically pass in CI (Goal G4, AC-4, AC-5), covering the
success response (AC-1–AC-3), negative paths (AC-6, AC-7), and
unauthenticated access (AC-11). No E2E or contract testing need is implied
by the PRD, since there is no UI and no external consumer contract
(PRD non-goals NG4, NG5).

## Decision

- **Unit-test framework:** xUnit.
- **Mocking/test-double approach:** No dedicated mocking library. The
  application has exactly one substitutable dependency
  (`IAppVersionProvider`), which is simple enough to fake by hand in test
  code without a mocking framework.
- **Integration-test approach:** `Microsoft.AspNetCore.Mvc.Testing`
  (`WebApplicationFactory<Program>`) paired with xUnit, booting the real
  application in-memory and issuing real HTTP requests against it.
- **E2E testing:** Not Applicable.
- **Contract testing:** Not Applicable.
- **Local cloud-service emulation:** Not Applicable — no cloud dependency
  exists to emulate.

## Alternatives Considered

| Option | Assessment | Verdict |
| --- | --- | --- |
| xUnit | Mature, most common in modern ASP.NET Core projects, first-class `dotnet test` support, actively maintained | **Selected** |
| NUnit | Equally mature and viable | Rejected — no material advantage over xUnit for this project; avoids picking arbitrarily between two equally good options by following the more common ASP.NET Core convention |
| MSTest | Viable, Microsoft-maintained | Rejected — xUnit has broader community convention in ASP.NET Core projects; no project-specific reason to prefer MSTest |
| `WebApplicationFactory` in-process integration tests | Exercises the full middleware pipeline without a real network port; fast, deterministic, no flakiness from port binding | **Selected** |
| Real-process integration tests (start `dotnet run`, hit a real port over the network) | Would validate actual process startup/port binding, closer to a true E2E check for AC-10 | Rejected as the automated-test approach — adds flakiness (port conflicts, startup timing) with no meaningful additional coverage over in-process testing for this simple contract; the equivalent manual/documented `dotnet run` + `curl` check (AC-10) is satisfied procedurally, not by an automated test, per PRD wording ("manually verified") |
| Dedicated mocking library (Moq/NSubstitute) | Would add a dependency for a single, trivial fake object | Rejected — unnecessary dependency for this scope (Stage 5: avoid redundant tools) |

## Consequences

- Two test projects are created: `tests/HealthApi.UnitTests` and
  `tests/HealthApi.IntegrationTests`, both referencing xUnit and the
  application project.
- AC-10 ("the service can be started locally... and manually verified via
  curl") is satisfied as a documented manual/procedural check, not by an
  automated E2E test, consistent with the PRD's own wording distinguishing
  it from the automated unit/integration suites (AC-4, AC-5).
- If a future revision adds a substitutable external dependency (e.g., a
  real HTTP client to another service), this ADR should be revisited to
  decide whether a mocking library becomes justified.

## Related Decisions

- ADR-001 (language/runtime/framework) — testing stack targets the same
  .NET 10 / ASP.NET Core baseline.
