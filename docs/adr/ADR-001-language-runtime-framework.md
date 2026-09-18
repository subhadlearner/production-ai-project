# ADR-001: Application Language, Runtime, and Framework

## Status

Accepted

## Context

PRD-001 imposes an explicit user constraint: the application must be built
in C#/.NET. It does not specify a .NET version or an ASP.NET Core hosting
model. The service is a single-endpoint, stateless HTTP health check with
no persistence, authentication, or UI (PRD non-goals NG1–NG5), so the
simplest production-grade hosting model that satisfies the contract should
be preferred (global policy: prefer the simplest architecture that
satisfies approved requirements).

Decision date: 2026-09-18.

## Decision

- **Language:** C#, using the language version shipped with the selected
  SDK (implicit `LangVersion=latest`).
- **Runtime:** .NET 10 (LTS).
- **Framework:** ASP.NET Core using the **Minimal API** hosting model
  (not MVC controllers).

## Alternatives Considered

### Runtime version

| Option | Support status (as of 2026-09-18) | Verdict |
| --- | --- | --- |
| .NET 8 (LTS) | Ends ~Nov 2026, roughly 6 weeks from this decision | Rejected — starting a new service on a runtime about to lose support is not production-safe |
| .NET 9 (STS) | Already out of support (~May 2026) | Rejected — already unsupported |
| .NET 10 (LTS) | Released Nov 2025, supported to ~Nov 2028 | **Selected** — longest safe support runway available |

### Framework hosting model

| Option | Assessment | Verdict |
| --- | --- | --- |
| ASP.NET Core Minimal API | Single-file route registration, no controller ceremony, fully supported production hosting model | **Selected** — simplest approach that fully satisfies a one-endpoint contract |
| ASP.NET Core MVC Controllers | Adds controller classes, routing attributes, and conventions oriented toward many endpoints | Rejected — unnecessary ceremony for a single route; no reliability/security/testability benefit at this scale |

No alternative language was considered; the language is an explicit,
non-negotiable user constraint from PRD-001.

## Consequences

- The project targets .NET 10 and must be re-evaluated (new ADR) before
  its LTS window ends (~Nov 2028) or if Microsoft's support policy changes.
- Minimal API keeps the codebase small; if the API surface grows
  materially beyond a single endpoint in a future revision, this decision
  should be revisited (a controller-based approach may become justified).
- `global.json` must pin the .NET 10 SDK so local, CI, and any future
  contributor environments resolve the same SDK version.

## Related Decisions

- ADR-002 (zero-infrastructure boundary) — this runtime never runs inside
  a provisioned cloud service.
- ADR-004 (testing stack) — test frameworks are chosen to match this
  runtime/framework.
