# ARCH-001: Health API — Production Architecture

## Document Metadata

- **Document ID:** ARCH-001
- **Title:** Architecture for the Tiny HTTP Health API (Engineering Workflow Smoke Test)
- **Version:** 0.1
- **Status:** Proposed — Ready for Project Initialization
- **Owner:** Architecture
- **Related Documents:**
  - `docs/prd/PRD-001-health-api.md` (approved PRD)
  - `docs/adr/ADR-001-language-runtime-framework.md`
  - `docs/adr/ADR-002-zero-infrastructure-boundary.md`
  - `docs/adr/ADR-003-ci-provider.md`
  - `docs/adr/ADR-004-testing-stack.md`

## Stage 1–2: Context and Constraints

### Explicit Requirements (from PRD-001)

- Single `GET /health` endpoint returning HTTP 200 with JSON `{status,
  version}` (FR-1–FR-10).
- No database, authentication, message queues, external integrations, or UI
  (NG1–NG5).
- No production deployment or cloud hosting; CI validation only (NG6, NG7).
- Automated unit and integration tests required (G4, AC-4, AC-5).
- CI must gate on build, tests, and code-quality checks (G5, AC-9).
- Zero or near-zero infrastructure cost (G3, Cost Constraints).
- **Explicit user technology constraint:** application must be built in
  **C#/.NET**.

### Architecture Assumptions (Non-Blocking, Carried or Added Here)

- PRD assumptions A-1 through A-9 apply (status literal, version source,
  port, test taxonomy, CI provider ownership, code-quality tooling
  ownership, no HTTPS requirement, local-only exposure, single environment).
- **New architecture assumption AA-1:** the repository will be hosted on
  GitHub (or migrated there before CI is enabled). This determines the CI
  provider choice (ADR-003). If the repository is hosted elsewhere, this is
  a low-risk, reversible decision to revisit at `/project-init` — it does
  not change application code, tests, or the technology baseline below.

### Unresolved Decisions

None required for this stage to proceed. All PRD non-goals (NG1, NG3, NG4,
NG5, NG6) explicitly rule out the categories that would otherwise require a
decision (persistence, messaging, external integration, UI, cloud runtime),
so those categories are correctly resolved as **Not Applicable** rather than
left open.

## Stage 3: Architecture Options Considered

Two decisions in this project have genuine, meaningful alternatives. All
other categories have only one realistic choice given the PRD's explicit
non-goals (e.g., there is no viable alternative to "no database" when
persistence is an explicit non-goal), so no alternatives are manufactured
for those.

### Option Set A — API Hosting Style

| Criterion | Minimal API (ASP.NET Core) | MVC Controllers (ASP.NET Core) |
| --- | --- | --- |
| Simplicity | One file, no controller/attribute ceremony for a single route | Requires controller class, routing attributes, more boilerplate |
| Reliability | Same underlying Kestrel/hosting pipeline — equivalent | Equivalent |
| Security | Equivalent (same middleware pipeline available) | Equivalent |
| Maintainability | Simple for 1 endpoint; would need discipline if it grew | Better suited to many endpoints with shared conventions |
| Operational burden | None added | None added |
| Implementation complexity | Lower | Higher for no functional benefit at this scale |
| Cost | Identical (same runtime) | Identical |

**Decision:** Minimal API. It is a fully supported, production-grade ASP.NET
Core hosting model (not a toy feature) and is the simplest approach that
satisfies a single-endpoint service. MVC's benefits (controller conventions,
filters, model binding for complex inputs) are not needed because there is
exactly one route with no input to bind. Choosing MVC here would add
ceremony without improving reliability, security, or testability.

### Option Set B — .NET Runtime Version

| Criterion | .NET 8 (LTS) | .NET 9 (STS) | .NET 10 (LTS) |
| --- | --- | --- | --- |
| Support status as of this decision (2026-09-18) | LTS, support ends Nov 2026 (~6 weeks away) | STS, support already ended ~May 2026 | LTS, released Nov 2025, supported to Nov 2028 |
| Production-safe to start a new project today | No — would be out of support within ~6 weeks | No — already unsupported | Yes |
| Tooling/ecosystem maturity | Mature | Mature but EOL | Mature, current SDK |
| Cost | Free | Free | Free |

**Decision:** .NET 10 (LTS). Starting a new production-grade service on a
runtime that is already out of support (.NET 9) or about to go out of
support within weeks (.NET 8) is not production-safe, regardless of cost.
.NET 10 is the current LTS release with the longest support runway
available at this decision date. This satisfies the user's C#/.NET
constraint while selecting the specific supported version within it.

No other options were manufactured: persistence, messaging, authentication,
UI framework, and cloud hosting all have a single realistic choice —
"none" — because the PRD explicitly excludes them (NG1–NG6).

## Stage 4–6: Technology Baseline

The following are architecture decisions. `/project-init`, `/spec`, and
`/implement` must use these values as-is and must not choose alternatives.

### Application Stack

- **Language:** C#
- **Language version:** Latest supported by the .NET 10 SDK (C# 14),
  `<LangVersion>latest</LangVersion>` (implicit SDK default).
- **Runtime:** .NET 10 (LTS)
- **Application framework:** ASP.NET Core, Minimal API hosting model
- **Framework version policy:** Track the .NET 10 LTS servicing releases
  (apply patch updates; do not move to a new major version without a new
  ADR)
- **API style:** REST-style single-resource endpoint (`GET /health`); no
  API versioning scheme required for a single stable endpoint
- **Serialization:** `System.Text.Json` (ASP.NET Core default; no
  third-party serializer needed)
- **Dependency injection:** Built-in `Microsoft.Extensions.DependencyInjection`
  container (ASP.NET Core default), used minimally — only to register the
  version-provider abstraction described in Major Components
- **Package/dependency manager:** NuGet via the `dotnet` CLI

### Persistence Stack

- **Database/storage technology:** Not Applicable — no persistence per
  PRD non-goal NG1.
- **Data-access library/SDK strategy:** Not Applicable.
- **Schema/model strategy:** Not Applicable.
- **Migration strategy:** Not Applicable.
- **Consistency model:** Not Applicable (no shared/mutable state).

### Cloud Runtime

- **Cloud provider:** Not Applicable — no production deployment per PRD
  non-goal NG6. The application runs only as a local process and as a
  process inside CI runners.
- **Region:** Not Applicable.
- **Compute/runtime service:** Local process (`dotnet run` /
  self-contained-free framework-dependent executable) and GitHub Actions
  Linux runner (`ubuntu-latest`) for CI execution.
- **CPU architecture:** x64 (default GitHub-hosted runner architecture);
  no architecture-specific code is used, so this is not a hard constraint.
- **Deployment packaging model:** Not Applicable — no deployment artifact
  is published or shipped anywhere (see ADR-002). `dotnet build` output is
  used only to run tests locally/in CI.
- **Networking model:** Loopback/local only. Kestrel binds to a local
  development port (`ASPNETCORE_URLS`, default `http://localhost:5000` or
  the framework's current default); never bound to a public interface.
- **Secrets/configuration storage:** Not Applicable — no secrets exist.
  Configuration (if any is later needed) uses standard ASP.NET Core
  configuration (`appsettings.json` + environment variables), with no
  values ever containing credentials.

### Infrastructure as Code

- **IaC technology:** Not Applicable — no infrastructure is provisioned
  (PRD non-goal NG6; ADR-002).
- **IaC language:** Not Applicable.
- **Deployment organization:** Not Applicable.
- **Environment strategy:** Single implicit environment (local/CI); no
  staging or production environment exists (PRD assumption A-9).

### Testing Stack

- **Unit-test framework:** xUnit
- **Mocking/test-double framework:** Not Applicable as a separate library.
  The only substitutable dependency is the version-provider abstraction
  (see Major Components), which is trivial enough to fake by hand in test
  code; no mocking library (e.g., Moq/NSubstitute) is needed because there
  are no complex external dependencies to isolate.
- **Integration-test framework/approach:** `Microsoft.AspNetCore.Mvc.Testing`
  (`WebApplicationFactory<T>`) combined with xUnit, exercising the full
  ASP.NET Core middleware pipeline in-memory (no real socket/port binding
  required) for `GET /health` and negative-path requests.
- **E2E-test framework/approach:** Not Applicable — there is no UI and no
  multi-service flow; the in-process integration tests already exercise
  the full HTTP request pipeline end-to-end for this single endpoint.
- **Contract-test approach:** Not Applicable — no external consumer
  contract is defined at this stage; the JSON shape is verified directly
  by unit and integration tests instead.
- **Local cloud-service emulation:** Not Applicable — no cloud service
  dependency exists.

### Code Quality Tooling

- **Formatter:** `dotnet format` (uses an `.editorconfig` committed at the
  repository root)
- **Linter/static analysis:** Built-in Roslyn analyzers via
  `<EnableNETAnalyzers>true</EnableNETAnalyzers>` and
  `<AnalysisLevel>latest-recommended</AnalysisLevel>` in a root
  `Directory.Build.props`, with `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`
  and `<Nullable>enable</Nullable>` so the standard `dotnet build` step
  itself acts as the static-analysis gate. No separate third-party linter
  is introduced (keeps the toolchain minimal per Stage 5 rules).
- **Compiler/static analysis:** Roslyn compiler with nullable reference
  types enabled, warnings elevated to errors (above).
- **Type checking:** Native to the C# compiler; nullable-reference-type
  analysis enabled as the practical equivalent for this stack.
- **Code-quality analyzers:** Included in the built-in .NET analyzer set
  above; no additional analyzer package is required at this scope.
- **Security/dependency scanning:** Built-in NuGet Audit
  (`NuGetAudit`, enabled by default in the .NET 10 SDK on `dotnet
  restore`/`dotnet build`) to flag known-vulnerable NuGet packages, plus
  GitHub Dependabot version updates/alerts (free, native to GitHub, no
  added infrastructure).
- **Secret scanning:** `gitleaks` run as a CI step (open source, free,
  deterministic regardless of GitHub plan/visibility tier).

### Local Development

- **Required SDK/runtime versions:** .NET 10 SDK (pinned via
  `global.json`)
- **Package manager:** NuGet via `dotnet` CLI
- **Local execution approach:** `dotnet run --project src/HealthApi`
- **Environment/configuration approach:** `appsettings.json` +
  `ASPNETCORE_URLS`/`ASPNETCORE_ENVIRONMENT` environment variables (no
  secrets involved)
- **Docker:** Not required. Local development and CI both run directly on
  the .NET SDK; introducing a container adds operational surface with no
  benefit for a single-process, no-dependency service (Stage 5: avoid
  unnecessary infrastructure). This can be revisited only if a future PRD
  revision requires containerized deployment (see PRD OQ-2), which would
  need its own ADR.
- **Cloud credentials for local development:** Not Applicable — none
  required.

### CI/CD Baseline

- **CI provider:** GitHub Actions (see ADR-003 and assumption AA-1)
- **Build strategy:** `dotnet build --configuration Release` on the
  `ubuntu-latest` GitHub-hosted runner; NuGet package caching enabled to
  minimize CI minutes
- **Test gates:** `dotnet test` for both the unit-test project and the
  integration-test project; the pipeline fails if either fails
- **Security gates:** NuGet Audit (via restore/build), Dependabot alerts,
  and a `gitleaks` secret-scan step; the pipeline fails on detected
  secrets and should fail (or clearly report) on high/critical
  vulnerability audit findings
- **IaC validation:** Not Applicable — no IaC exists
- **Artifact packaging:** Not required for this smoke test (no deployment
  target exists); CI does not publish a container image or deployment
  artifact (deferred per PRD OQ-2)
- **Deployment approach:** Not Applicable — no deployment occurs (ADR-002)
- **Production approval model:** Not Applicable now; any future extension
  toward real deployment requires a new PRD/ADR and explicit human
  approval per the global production-deployment policy

## Stage 6: Technology Completeness Gate

All categories required by Stage 6 have been explicitly resolved:

| Required Category | Status |
| --- | --- |
| Application language | Decided — C# |
| Language/runtime version | Decided — .NET 10 (LTS) |
| Application framework | Decided — ASP.NET Core Minimal API |
| Package manager | Decided — NuGet / `dotnet` CLI |
| Cloud/runtime platform | Decided — Not Applicable (local + CI only, per NG6) |
| Database/storage technology | Decided — Not Applicable (per NG1) |
| IaC technology | Decided — Not Applicable (per NG6) |
| Unit-test framework | Decided — xUnit |
| Integration-test approach | Decided — `WebApplicationFactory` + xUnit |
| Formatting/lint/static-analysis approach | Decided — `dotnet format` + built-in analyzers, warnings-as-errors |
| CI/CD approach | Decided — GitHub Actions |

No decision required for implementation remains unresolved. `/project-init`
can populate `AGENTS.md` directly from this document without guessing.

## Stage 7: Architecture Design

### Architecture Overview

The system is a single, stateless ASP.NET Core process exposing exactly one
HTTP endpoint. There is no distributed topology, no data tier, and no
network boundary beyond the loopback interface used for local/CI execution.

```text
┌─────────────────────────────────────────────┐
│              HealthApi process               │
│                                               │
│   Kestrel HTTP listener (loopback only)      │
│              │                               │
│              ▼                               │
│   ASP.NET Core routing/middleware pipeline   │
│              │                               │
│              ▼                               │
│   Minimal API handler: GET /health           │
│              │                               │
│              ▼                               │
│   IAppVersionProvider (assembly metadata)     │
│              │                               │
│              ▼                               │
│   JSON response: { status, version }         │
└─────────────────────────────────────────────┘
```

The only "critical path" is the request path for `GET /health`: routing →
handler → version lookup → JSON serialization → response. There is no
write path, no asynchronous path, and no cross-service call.

### Major Components

1. **Application Host (`src/HealthApi`, `Program.cs`)**
   - **Responsibility:** Bootstrap the ASP.NET Core Minimal API host,
     configure logging, register the single route, start/stop Kestrel.
   - **Technology:** ASP.NET Core 10 Minimal API.
   - **Key configuration:** `ASPNETCORE_URLS` (listen address/port),
     `ASPNETCORE_ENVIRONMENT` (Development/Production naming only — no
     environment-specific behavior beyond default framework logging
     verbosity).
   - **Dependencies:** None external; depends on the Health Endpoint
     Handler and Version Provider described below.
   - **Scale/reliability considerations:** Single instance; must fail fast
     with a non-zero exit code and a logged error if it cannot bind its
     configured port (PRD validation requirement).

2. **Health Endpoint Handler**
   - **Responsibility:** Handle `GET /health`, assemble the response body
     `{ status, version }`, return HTTP 200.
   - **Technology:** A Minimal API route delegate registered on the
     `WebApplication` builder.
   - **Key configuration:** None beyond the route registration itself.
   - **Dependencies:** `IAppVersionProvider` (below).
   - **Scale/reliability considerations:** Pure, side-effect-free
     function; safe under concurrent/repeated calls (idempotent).

3. **Version Provider (`IAppVersionProvider`)**
   - **Responsibility:** Resolve the application's current version from
     build/assembly metadata (e.g., `Assembly.GetEntryAssembly()` informational
     version, sourced from the `.csproj` `Version`/`InformationalVersion`
     properties) so the value is never hand-maintained separately from the
     build.
   - **Technology:** Plain C# class using `System.Reflection`.
   - **Key configuration:** `.csproj` `<Version>` property, set per
     release/build.
   - **Dependencies:** None.
   - **Scale/reliability considerations:** Introduced specifically so unit
     tests can substitute a hand-written fake implementation (no mocking
     framework required) to test the handler's response shape
     independently of the actual build version, while integration tests
     verify the real assembly-sourced value end-to-end (AC-3).

4. **Test Suites**
   - **Unit tests (`tests/HealthApi.UnitTests`):** xUnit tests exercising
     the Health Endpoint Handler logic directly (status code, JSON shape)
     using a fake `IAppVersionProvider`.
   - **Integration tests (`tests/HealthApi.IntegrationTests`):** xUnit
     tests using `WebApplicationFactory<Program>` to boot the real app
     in-memory and issue real HTTP requests, covering AC-1, AC-2, AC-3,
     AC-6, AC-7, AC-11.

5. **CI Pipeline (`.github/workflows/ci.yml`)**
   - **Responsibility:** On every push/PR, restore, build, run both test
     suites, run formatting verification, and run the secret scan; fail
     the workflow on any failure.
   - **Technology:** GitHub Actions, `ubuntu-latest` runner,
     `actions/setup-dotnet` for the pinned .NET 10 SDK.
   - **Dependencies:** NuGet package sources (public NuGet.org only — no
     private feed needed).
   - **Scale/reliability considerations:** Single job, short-running
     (target well under 5 minutes); NuGet caching keeps runs fast and
     within any free-tier CI minute budget.

### Data Model

Not Applicable. There are no entities, no persisted resources, no keys or
indexes, and no retention/TTL or migration considerations — the service is
entirely stateless (PRD non-goal NG1). The only "data" involved is the
in-memory, per-request response object `{ status: string, version: string
}`, which is constructed fresh on every request and never stored.

### Data Flow

- **Read (the only flow):** Client issues `GET /health` → Kestrel accepts
  the connection on the loopback interface → ASP.NET Core routing matches
  `/health` with method `GET` → Minimal API handler invoked → handler
  calls `IAppVersionProvider.GetVersion()` → handler builds `{ status,
  version }` → `System.Text.Json` serializes the response → HTTP 200
  returned with `Content-Type: application/json`.
- **Authentication/authorization:** None — every request is treated as
  authorized (PRD non-goal NG2, FR-6).
- **Asynchronous flows:** None exist.
- **Failure flow — wrong method:** Client issues `POST /health` (or any
  non-GET) → ASP.NET Core routing matches the path but not the method →
  framework returns HTTP 405 (or equivalent) without invoking the handler.
- **Failure flow — unknown route:** Client requests an undefined path →
  routing finds no match → framework returns HTTP 404.
- **Failure flow — startup failure:** Kestrel fails to bind the configured
  port at startup → host logs a fatal error and the process exits with a
  non-zero code → no traffic is ever served in a broken state.

### Security Model

- **Authentication:** None (PRD non-goal NG2). Explicitly documented so
  this is a deliberate decision, not an oversight.
- **Authorization:** None — `/health` is open to any caller able to reach
  the bound loopback address.
- **Secrets:** None exist in this system; no secret storage mechanism is
  required.
- **Encryption:** Not required for local/CI HTTP traffic (PRD assumption
  A-7). If the framework's default HTTPS redirection is active in a given
  environment, tests call the HTTP listener directly rather than
  configuring TLS certificates.
- **Network boundaries:** The process binds only to a local/loopback
  address; it is never exposed to a public network interface as part of
  this workflow smoke test (PRD assumption A-8).
- **Least privilege:** The process runs as the invoking local user/CI
  runner account with no elevated privileges and no access to any
  external resource.
- **Input validation:** Not Applicable — the endpoint accepts no request
  body, query parameters, or path parameters to validate.
- **Sensitive data/logging:** No PII or secrets exist to leak; log
  statements are limited to operational lines (startup, shutdown, request
  method/path/status) and must never include environment variable values
  or stack traces in responses.
- **Supply-chain controls:** NuGet Audit (vulnerable-package detection),
  GitHub Dependabot (dependency update alerts), and `gitleaks` (secret
  scanning) run in CI on every push/PR.
- **Abuse/throttling:** Not Applicable — the service is never reachable
  from an untrusted network in this workflow's scope; rate limiting would
  be a new requirement if a future revision introduces real deployment.

### Observability

- **Logs:** Built-in `Microsoft.Extensions.Logging` writing structured log
  lines to the console (framework default). Minimum lines: application
  startup (listening address), a line per request (method, path, status
  code — the ASP.NET Core default request logging), and application
  shutdown.
- **Metrics:** Not Applicable — no metrics backend is provisioned; this
  would only become relevant if the service were ever deployed to a real
  environment (out of scope, PRD non-goal NG6).
- **Traces:** Not Applicable, same rationale as Metrics.
- **Alarms/dashboards:** Not Applicable — nothing is monitored in
  production because nothing is deployed to production.
- **Retention:** Console log output is ephemeral locally and captured only
  as part of the CI job's run log (subject to the CI provider's own log
  retention default; no additional retention configuration is introduced).
- **Correlation/request IDs:** ASP.NET Core's built-in
  `HttpContext.TraceIdentifier` is sufficient for correlating log lines
  within a single request; no external correlation/tracing system is
  added.
- **Cost controls:** Zero added cost — no telemetry backend, log
  aggregation service, or APM tool is introduced.

### Deployment Strategy

- **Environments:** A single implicit environment covering local
  development and CI execution. No staging or production environment
  exists for this workflow smoke test (PRD non-goal NG6, NG7).
- **IaC:** Not Applicable.
- **CI/CD:** GitHub Actions runs restore → build → unit tests →
  integration tests → format verification → secret scan on every push and
  pull request (see ADR-003).
- **Release strategy:** Version is tracked via the `.csproj` `<Version>`
  property, bumped manually as part of a change when relevant; no
  automated release/tagging pipeline is required at this scope.
- **Rollback:** Not Applicable — nothing is deployed to roll back.
- **Configuration:** `appsettings.json` plus standard environment
  variables; no per-environment configuration split is needed since only
  one environment exists.
- **Secrets:** Not Applicable — none exist.
- **Migrations:** Not Applicable — no persistent store exists.
- **Production approval:** Not Applicable now. If a future PRD revision
  introduces real deployment, that revision must go through a new
  `/architect` pass and the standard human-approved production deployment
  gate defined in the global policy — this architecture does not
  pre-authorize any such extension.

### Failure Modes

| Failure | Detection | Behavior | Mitigation | Recovery |
| --- | --- | --- | --- | --- |
| Configured port already in use at startup | Kestrel throws a bind exception during host start | Host logs a fatal error and the process exits with a non-zero code; no partial/broken serving occurs | Port is configurable via `ASPNETCORE_URLS` | Operator sets a free port and restarts the process |
| CI job fails (test failure, format violation, audit/secret finding) | GitHub Actions job reports failure | PR is blocked from merge per the standard review/CI gate | Keep tests deterministic (no time/network dependence) to avoid flakiness | Fix the underlying cause and re-run the CI job |
| Known-vulnerable NuGet package introduced | NuGet Audit warning/error at restore/build time; Dependabot alert | CI reports the finding; pipeline should fail on high/critical findings | Keep the dependency set minimal (currently zero third-party runtime dependencies) | Update or replace the affected package, re-run CI |
| Secret accidentally committed | `gitleaks` CI step detects a match | CI fails and blocks merge | Keep `.gitignore` current for local secret-bearing files (e.g., `.env`) | Remove/rotate the secret; re-run CI once clean |

### Recovery Strategy

- **Backup/recovery:** Not Applicable — the service is entirely stateless;
  there is no data to back up.
- **RTO/RPO:** Not Applicable — no production SLA exists for this
  workflow smoke test (PRD non-goal NG6).
- **Restore process:** Not Applicable.
- **Deployment rollback:** Not Applicable — nothing is deployed.
- **Infrastructure reconstruction:** Not Applicable in the cloud-resource
  sense, since none is provisioned. The entire system (application code,
  tests, and CI configuration) is version-controlled in Git; the working
  system can always be reconstructed by cloning the repository and running
  `dotnet build`/`dotnet test` — no external state needs to be restored.

### Cost Characteristics

- **Fixed recurring cost:** $0. No cloud resources are provisioned
  (compute, storage, networking, observability backends). GitHub Actions
  is used within its free-tier minutes for the expected low run frequency
  of this smoke-test pipeline.
- **Variable usage-based cost:** Effectively $0 under expected usage. The
  only variable cost driver is GitHub Actions CI minutes; a single CI run
  for this project (restore + build + two small test projects + format
  check + secret scan) is expected to complete in well under 5 minutes on
  a Linux runner, which comfortably fits within GitHub's free monthly
  minutes even for a private repository under normal (non-high-frequency)
  usage.
- **Primary cost drivers:** None beyond CI minutes; there are no other
  metered services in this architecture.
- **Cost risks:** If CI run frequency becomes very high (e.g., very large
  numbers of pushes/PRs per day) on a private repository, free-tier CI
  minutes could theoretically be exhausted; this is a low-probability risk
  for a smoke-test project and is mitigated by keeping the pipeline single
  job / single OS target (no build matrix) and caching NuGet packages to
  minimize run duration.
- **Optimization levers already applied:** `ubuntu-latest` (cheapest
  GitHub-hosted runner tier), no cross-OS build matrix (not required by
  the PRD), NuGet package caching, and no container build/publish step
  (avoided per Stage 5 — no speculative infrastructure).

These cost figures are explicit assumptions based on typical GitHub Actions
free-tier allowances and expected low run frequency for a smoke-test
project; they are not a precision cost commitment and should be
reconfirmed at `/project-init` if actual usage patterns differ materially.

## Stage 9: Technology Baseline Summary

| Area | Decision | Version / Policy | Rationale |
| --- | --- | --- | --- |
| Application Language | C# | Latest supported by .NET 10 SDK (C# 14) | Explicit user constraint (C#/.NET); current language version tracks the selected LTS SDK |
| Runtime | .NET | .NET 10 (LTS) | Only LTS release with a safe support runway (to Nov 2028) as of this decision date; .NET 8 is ~6 weeks from EOL and .NET 9 (STS) is already EOL |
| Framework | ASP.NET Core, Minimal API | Tracks .NET 10 servicing releases | Simplest production-grade hosting model sufficient for one endpoint (see Option Set A) |
| Package Manager | NuGet / `dotnet` CLI | Current .NET 10 SDK tooling | Standard, zero-cost, no alternative needed |
| Database / Storage | Not Applicable | Not Applicable | Explicit PRD non-goal (NG1) |
| Data Access | Not Applicable | Not Applicable | No persistence exists |
| Cloud | Not Applicable | Not Applicable | Explicit PRD non-goal (NG6); local + CI only |
| Region | Not Applicable | Not Applicable | No cloud deployment |
| Compute | Local process + GitHub Actions `ubuntu-latest` runner | N/A | Zero fixed cost, sufficient for local/CI execution |
| IaC | Not Applicable | Not Applicable | No infrastructure to provision |
| IaC Language | Not Applicable | Not Applicable | No infrastructure to provision |
| Unit Testing | xUnit | Current stable release compatible with .NET 10 | Mature, standard, well-supported .NET unit-test framework |
| Integration Testing | `Microsoft.AspNetCore.Mvc.Testing` (`WebApplicationFactory`) + xUnit | Current stable release matching ASP.NET Core 10 | Idiomatic in-process ASP.NET Core integration testing; no extra infrastructure needed |
| E2E Testing | Not Applicable | Not Applicable | No UI or multi-service flow; integration tests cover the full HTTP pipeline |
| Formatting | `dotnet format` | Built into .NET 10 SDK | Zero-cost, standard, `.editorconfig`-driven |
| Lint / Static Analysis | Built-in Roslyn analyzers (`EnableNETAnalyzers`, `AnalysisLevel=latest-recommended`), warnings-as-errors | .NET 10 SDK default analyzer set | Avoids adding a third-party linter; sufficient for this scope |
| Security / Dependency Scan | NuGet Audit + GitHub Dependabot + `gitleaks` | Built-in (.NET 10 SDK) / GitHub native / current `gitleaks` release | Zero-cost, deterministic, covers vulnerable packages and secret leakage |
| CI/CD | GitHub Actions | `ubuntu-latest` runner, `actions/setup-dotnet` | Zero fixed cost, integrates with existing PR/merge workflow (see ADR-003) |

## Stage 10: Project Initialization Handoff

`/project-init` must record the following exact values into `AGENTS.md`
(Technology section and Build/Verification Commands section) without
choosing alternatives:

- **Runtime:** .NET 10 (LTS)
- **Language:** C# (latest supported by the .NET 10 SDK)
- **Framework:** ASP.NET Core, Minimal API hosting model
- **Database:** Not Applicable
- **Cloud:** Not Applicable (local + CI execution only)
- **Region:** Not Applicable
- **Infrastructure as Code:** Not Applicable
- **Package Manager:** NuGet via `dotnet` CLI
- **Unit Test Framework:** xUnit
- **Integration Test Framework:** `Microsoft.AspNetCore.Mvc.Testing`
  (`WebApplicationFactory<Program>`) + xUnit
- **E2E Test Framework:** Not Applicable
- **Linting:** Built-in Roslyn analyzers (`EnableNETAnalyzers`,
  `AnalysisLevel=latest-recommended`), enforced via `TreatWarningsAsErrors`
  during build
- **Formatting:** `dotnet format`
- **Type Checking / Static Analysis:** C# compiler with
  `<Nullable>enable</Nullable>` and warnings-as-errors
- **Security / Dependency Scanning:** NuGet Audit (built-in) + GitHub
  Dependabot alerts + `gitleaks` secret scanning in CI

Recorded repository/project layout for `/project-init` to create:

- Solution: `HealthApi.sln`
- Application project: `src/HealthApi/HealthApi.csproj`
- Unit test project: `tests/HealthApi.UnitTests/HealthApi.UnitTests.csproj`
- Integration test project:
  `tests/HealthApi.IntegrationTests/HealthApi.IntegrationTests.csproj`
- SDK pin: `global.json` targeting the .NET 10 SDK
- CI workflow: `.github/workflows/ci.yml`

Recorded command patterns for `/project-init` to place in AGENTS.md:

- **Dependency Restore / Install:** `dotnet restore`
- **Build:** `dotnet build --configuration Release --no-restore`
- **Unit Tests:** `dotnet test tests/HealthApi.UnitTests --configuration Release --no-build`
- **Integration Tests:** `dotnet test tests/HealthApi.IntegrationTests --configuration Release --no-build`
- **E2E Tests:** Not Applicable
- **Lint:** Included in the Build command above (analyzers run in-line,
  warnings treated as errors — no separate lint command exists)
- **Formatting Verification:** `dotnet format --verify-no-changes`
- **Type Checking / Static Analysis:** Included in the Build command above
- **Security / Dependency Checks:** `dotnet restore` (triggers NuGet
  Audit) plus a dedicated CI step running `gitleaks detect --no-banner
  --exit-code 1`
- **Infrastructure Validation:** Not Applicable

## Stage 11: Architecture Completeness Check

- PRD constraints covered: Yes — every FR, NG, and constraint in PRD-001
  is reflected in the design above.
- Major architecture decided: Yes — single-process Minimal API service,
  no distributed components.
- Technology baseline complete: Yes — every Stage 6 required category is
  explicitly decided (including explicit "Not Applicable" where the PRD
  rules the category out).
- Implementation agents will not need to choose the stack: Yes — language,
  runtime, framework, package manager, test frameworks, code-quality
  tooling, and CI provider are all fixed above.
- Architecture and ADRs agree: Yes — see ADR-001 through ADR-004, which
  restate and justify the same decisions.
- Cost assumptions documented: Yes — see Cost Characteristics; assumptions
  are explicit and flagged as non-precise estimates.
- Security model defined: Yes — see Security Model (explicitly minimal
  given no auth/secrets/data, per PRD non-goals).
- Reliability/recovery defined: Yes — see Reliability requirements
  (inherited from PRD) and Recovery Strategy (Not Applicable, justified).
- Test tooling defined: Yes — xUnit + `WebApplicationFactory`.
- CI/CD approach defined: Yes — GitHub Actions pipeline described above
  and in ADR-003.
- Unresolved decisions clearly identified: None remain. The only
  assumption an implementer should double-check is AA-1 (GitHub-hosted
  repository); if false, only the CI provider changes, not the
  application architecture or technology baseline.

Architecture is implementation-ready. No required technology decision is
missing or deferred to a later stage.
