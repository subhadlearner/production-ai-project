# SPEC-001: Health Endpoint Implementation and Repository/CI Bootstrap

## Identity

- **Spec ID:** SPEC-001
- **Title:** Health Endpoint Implementation and Repository/CI Bootstrap
- **Objective:** Stand up the repository's application/test scaffolding, CI
  pipeline, and quality/security gates, and implement the single `GET
  /health` endpoint per PRD-001, so that the endpoint is real, automatically
  tested, and continuously verified — end to end, in one coherent unit of
  work.

## Decomposition Note (Why One Specification, Not a Separate Bootstrap Spec)

The approved architecture requires repository bootstrap work that does not
yet exist (solution/project scaffolding, `global.json`, code-quality
configuration, and the CI workflow — see
`docs/architecture/ARCH-001-health-api.md` Stage 10, ADR-003). Per PRD
non-goal NG8, this project will **never** have more than one HTTP endpoint,
so there is no future specification that would depend on a standalone
bootstrap spec, and no parallelism to enable by separating them — bootstrap
and the feature would be implemented, reviewed, and merged as a single
indivisible change regardless. Splitting them into `SPEC-000` +
`SPEC-001` would only add coordination overhead and file-ownership overlap
(both would touch `Program.cs`, the `.csproj`, and the CI workflow) with no
independent value delivered by the bootstrap-only spec. This specification
therefore explicitly includes the bootstrap/CI work in its scope, so it is
never silently assumed to already exist.

## Scope

### Included Behavior

- Solution and project scaffolding: `HealthApi.sln`, `src/HealthApi/`,
  `tests/HealthApi.UnitTests/`, `tests/HealthApi.IntegrationTests/`,
  `global.json`, root `Directory.Build.props`, root `.editorconfig`.
- The `GET /health` Minimal API endpoint returning HTTP 200 with JSON
  `{ "status": "healthy", "version": "<assembly version>" }`.
- The `IAppVersionProvider` abstraction and its assembly-metadata-backed
  implementation.
- Unit tests (xUnit) for the handler logic using a fake version provider.
- Integration tests (xUnit + `WebApplicationFactory<Program>`) exercising
  the full HTTP pipeline for success and negative paths.
- CI workflow (`.github/workflows/ci.yml`) implementing: restore, build,
  unit tests, integration tests, formatting verification, and secret
  scanning (`gitleaks`) on every push and pull request.
- Root code-quality configuration enforcing `Nullable` reference types,
  `TreatWarningsAsErrors`, and built-in analyzers
  (`EnableNETAnalyzers`/`AnalysisLevel=latest-recommended`).

### Excluded Behavior (Non-Goals for This Spec)

- No additional HTTP endpoints (PRD NG8).
- No database, ORM, message queue, external HTTP client integration, or
  UI/frontend project (PRD NG1, NG3, NG4, NG5).
- No authentication/authorization middleware (PRD NG2).
- No Dockerfile, container publish step, IaC, or cloud deployment target
  (PRD NG6; ADR-002).
- No deployment workflow, artifact publishing, or release automation.
- No `.github/dependabot.yml` version-update-PR configuration is created
  by this spec — enabling **Dependabot alerts** is a one-time GitHub
  repository setting (Security → Code security and analysis), not a code
  change, and is called out under Definition of Done as an operator
  action outside automated scope.

## Dependencies

- **Prerequisite specs:** None (this is the first and, per PRD-001 scope,
  only implementation specification for this project).
- **Architecture/ADR references:**
  - `docs/architecture/ARCH-001-health-api.md` (technology baseline,
    major components, data flow, failure modes)
  - `docs/adr/ADR-001-language-runtime-framework.md` (.NET 10, Minimal
    API)
  - `docs/adr/ADR-002-zero-infrastructure-boundary.md` (no cloud/IaC)
  - `docs/adr/ADR-003-ci-provider.md` (GitHub Actions, `ubuntu-latest`)
  - `docs/adr/ADR-004-testing-stack.md` (xUnit, `WebApplicationFactory`,
    no mocking library)
- **External dependencies:** NuGet.org (public package source only);
  GitHub Actions hosted runners; the `gitleaks` CLI binary (downloaded in
  CI, not vendored).
- **Migration ordering:** Not Applicable — no persistence.
- **Deployment ordering:** Not Applicable — no deployment target exists.

## Technology Constraints

This specification must use exactly the approved stack recorded in
`AGENTS.md` and `docs/architecture/ARCH-001-health-api.md`. No substitution
is permitted:

- **Language/runtime:** C# on .NET 10 (LTS)
- **Framework:** ASP.NET Core, Minimal API hosting model (no MVC
  controllers)
- **Serialization:** `System.Text.Json` (no third-party JSON library)
- **Package manager:** NuGet via `dotnet` CLI
- **Unit tests:** xUnit (no mocking framework — hand-written fakes only,
  per ADR-004)
- **Integration tests:** `Microsoft.AspNetCore.Mvc.Testing`
  (`WebApplicationFactory<Program>`) + xUnit
- **Formatting:** `dotnet format`
- **Static analysis:** built-in Roslyn analyzers
  (`EnableNETAnalyzers=true`, `AnalysisLevel=latest-recommended`),
  `TreatWarningsAsErrors=true`, `Nullable=enable`
- **Security/dependency scanning:** NuGet Audit (built-in, via `dotnet
  restore`) + `gitleaks detect --no-banner --exit-code 1`
- **CI provider:** GitHub Actions, `ubuntu-latest` runner,
  `actions/setup-dotnet`

## Repository Layout (Authoritative for This Spec)

```text
.
├── HealthApi.sln
├── global.json
├── Directory.Build.props
├── .editorconfig
├── .github/
│   └── workflows/
│       └── ci.yml
├── src/
│   └── HealthApi/
│       ├── HealthApi.csproj
│       ├── Program.cs
│       ├── HealthResponse.cs
│       ├── IAppVersionProvider.cs
│       ├── AssemblyAppVersionProvider.cs
│       └── HealthEndpoint.cs
└── tests/
    ├── HealthApi.UnitTests/
    │   ├── HealthApi.UnitTests.csproj
    │   ├── FakeAppVersionProvider.cs
    │   └── HealthEndpointTests.cs
    └── HealthApi.IntegrationTests/
        ├── HealthApi.IntegrationTests.csproj
        └── HealthEndpointIntegrationTests.cs
```

File/class names above are the expected shape; minor naming adjustments
during implementation are acceptable as long as the responsibilities,
public contract, and test coverage below are preserved.

## Interfaces

### HTTP Contract

- **Route:** `GET /health`
- **Success response:**
  - Status code: `200 OK`
  - `Content-Type: application/json; charset=utf-8`
  - Body:
    ```json
    {
      "status": "healthy",
      "version": "1.0.0"
    }
    ```
  - `status` is the fixed literal `"healthy"` (PRD assumption A-1,
    finalized here).
  - `version` is the application's informational version as reported by
    `IAppVersionProvider`, sourced from the `.csproj` `<Version>` property
    at build time (PRD FR-5).
- **Non-`GET` methods on `/health`** (e.g., `POST`, `PUT`, `DELETE`):
  handled entirely by ASP.NET Core's built-in endpoint-routing method-not-
  allowed behavior — no application code is required. Expected result:
  `405 Method Not Allowed`.
- **Undefined routes:** handled entirely by default routing (no matching
  endpoint). Expected result: `404 Not Found`.

### C# Contract

```csharp
public interface IAppVersionProvider
{
    string GetVersion();
}
```

```csharp
public sealed record HealthResponse(
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("version")] string Version);
```

`HealthEndpoint` exposes a pure, host-independent method used both by the
Minimal API route registration and directly by unit tests:

```csharp
public static class HealthEndpoint
{
    public const string HealthyStatus = "healthy";

    public static HealthResponse Handle(IAppVersionProvider versionProvider);

    public static void MapHealthEndpoint(this WebApplication app);
}
```

`Program.cs` registers `IAppVersionProvider` in the built-in DI container
(singleton lifetime — the assembly version never changes at runtime) and
calls `app.MapHealthEndpoint()`. `Program.cs` contains only composition-
root code: builder setup, DI registration, logging configuration, route
registration, and `app.Run()` (per `AGENTS.md` Coding Conventions).

## Data

Not Applicable in the persistence sense (PRD non-goal NG1). The only
"data" is the in-memory, per-request `HealthResponse` value:

- `status: string` — always `"healthy"` under normal operation.
- `version: string` — assembly informational version string (e.g.,
  `"1.0.0"`).

No entities, indexes, migrations, retention, or TTL apply.

## Behavior

### Normal Behavior

1. Client sends `GET /health`.
2. `HealthEndpoint.Handle` calls `IAppVersionProvider.GetVersion()` and
   returns a `HealthResponse` with `Status = "healthy"` and the resolved
   version.
3. The route delegate wraps the result in `Results.Ok(...)`, producing
   HTTP 200 with the JSON body above.

### Version Resolution

`AssemblyAppVersionProvider.GetVersion()`:

1. Reads `Assembly.GetEntryAssembly()`'s
   `AssemblyInformationalVersionAttribute.InformationalVersion`.
2. If that value is present and non-empty, return it as-is.
3. If unavailable (defensive edge case only — should not occur in a normal
   build), fall back to `Assembly.GetEntryAssembly()?.GetName().Version`
   formatted as a string; if that is also unavailable, return the literal
   `"unknown"` rather than throwing.

This fallback exists solely so `/health` can never fail (violate AC-1/
AC-2) due to a missing version attribute; it must not be relied upon under
normal build conditions, where step 1 always succeeds.

### Build/Version Configuration

- `src/HealthApi/HealthApi.csproj` sets `<Version>1.0.0</Version>`.
- Root `Directory.Build.props` sets
  `<IncludeSourceRevisionInInformationalVersion>false</IncludeSourceRevisionInInformationalVersion>`
  so the informational version deterministically equals `<Version>` with
  no appended source-control suffix. This keeps AC-3 (version field
  matches the actual build/assembly version) unambiguous and
  environment-independent.

### Startup Behavior

- The host binds using the configured `ASPNETCORE_URLS` (or the
  framework/launch-profile default for local development). No hard-coded
  port is embedded in application code (PRD assumption A-3).
- If Kestrel cannot bind the configured port, the host must fail fast: log
  a fatal error and exit with a non-zero process code (default ASP.NET
  Core Minimal API hosting behavior on a bind exception — no custom
  handling is required beyond not suppressing the exception).

### Edge Cases

- Trailing slash (`GET /health/`): follows ASP.NET Core's default routing
  behavior (by default, a distinct path from `/health`; no explicit
  redirect/normalization is added, since the PRD defines only the exact
  path `/health`).
- Query string on `/health` (e.g., `GET /health?x=1`): ignored; the
  endpoint still returns 200, since query parameters are not part of the
  contract and Minimal API route matching by path is unaffected by an
  unused query string.
- Concurrent requests: safe — `HealthEndpoint.Handle` and
  `AssemblyAppVersionProvider.GetVersion()` are pure/side-effect-free and
  read only immutable assembly metadata.

## Validation

Not Applicable — `GET /health` accepts no request body, route parameters,
or query parameters requiring validation (PRD Validation and Error
Behavior).

## Error Handling

- Non-`GET` request to `/health` → `405 Method Not Allowed` (framework
  default routing behavior; no custom code).
- Request to an undefined path → `404 Not Found` (framework default; no
  custom code).
- No application code path in this spec can throw an unhandled exception
  under normal operation; the version-resolution fallback (Behavior
  section) prevents the one theoretical failure point from ever
  propagating an exception to the client.
- The application must not run with the ASP.NET Core Developer Exception
  Page enabled outside `Development` (default framework behavior when
  `ASPNETCORE_ENVIRONMENT` is not set to `Development` — do not override
  this default), so no stack traces are ever exposed in responses (PRD
  Security and Privacy Requirements).

## Security

- **Authentication/authorization:** None — no middleware is added; every
  request to `/health` is served unconditionally (PRD FR-6, NG2).
- **Secrets:** None exist in this codebase; no configuration file in this
  spec may contain a credential, connection string, or token.
- **Sensitive data/logging:** Request logging (default ASP.NET Core
  request logging, if enabled) must log only method, path, and status
  code — never headers, environment variable values, or stack traces.
- **Abuse controls:** Not Applicable — the service is not exposed to an
  untrusted network in this workflow's scope (PRD assumption A-8).
- **Supply chain:**
  - `dotnet restore` must run with NuGet Audit enabled (default in the
    .NET 10 SDK) — do not disable it (e.g., do not set
    `NuGetAuditMode`/`NuGetAuditLevel` in a way that suppresses findings).
  - CI must run `gitleaks detect --no-banner --exit-code 1` against the
    full repository history (checkout with full history, not a shallow
    clone) and fail the build on any finding.
  - No new third-party runtime dependency may be added beyond the ASP.NET
    Core framework reference itself (implicit via
    `Microsoft.NET.Sdk.Web`) without updating ADR-004/AGENTS.md first.

## Observability

- Use the default ASP.NET Core console logging provider
  (`Microsoft.Extensions.Logging`); no external logging sink is added.
- Log at minimum: a startup line indicating the listening address, and
  (via default framework behavior) one line per request with method,
  path, and status code.
- No metrics, tracing, alarms, or dashboards are added (PRD non-goal
  NG6-driven scope; Not Applicable per architecture Observability
  section).
- `HttpContext.TraceIdentifier` (built-in) is sufficient for correlating
  log lines within a single request; no additional correlation ID scheme
  is introduced.

## Performance and Reliability

- **Latency:** `GET /health` must respond in well under 200ms locally
  under single-request, non-concurrent load (informal target per PRD
  Performance Requirements) — no load test is required; this is expected
  to hold trivially given the endpoint does no I/O.
- **Throughput:** No specific target beyond handling normal test-suite and
  manual-verification traffic.
- **Timeouts/retries:** Not Applicable — no outbound calls exist from this
  service.
- **Idempotency:** `GET /health` is naturally idempotent and side-effect
  free; no idempotency key or special handling is needed.
- **Concurrency:** The handler and version provider must remain stateless
  and thread-safe (no mutable shared state) so Kestrel's default
  concurrent request handling is safe without additional synchronization.
- **Recovery:** If the process crashes or fails to start, no automatic
  restart logic is implemented (Not Applicable — no orchestrator/host
  exists in this workflow's scope; a human/CI simply re-runs `dotnet run`
  or the CI job).

## Cost Constraints

- No cloud resource may be provisioned by this spec (ADR-002).
- The CI workflow must run as a single job on `ubuntu-latest` (no build
  matrix), with NuGet package caching enabled (via `actions/setup-dotnet`
  cache options or `actions/cache`) to minimize CI-minute usage, per
  `AGENTS.md` Project-Specific Cloud and Cost Constraints.
- No paid GitHub Action or third-party service may be introduced for
  `gitleaks` scanning; install and run the open-source CLI binary directly
  in the workflow rather than depending on a licensed marketplace Action.

## Acceptance Criteria

Each SPEC-level criterion below maps to a PRD-001 acceptance criterion
(see Traceability) and is objectively verifiable by an automated check
unless explicitly marked procedural.

- **SPEC-AC-1:** `GET /health` on a running instance returns HTTP `200`.
  *(→ PRD AC-1)*
- **SPEC-AC-2:** The response body is valid JSON containing exactly the
  fields `status` and `version` (extra fields are not required but their
  absence is verified, not their prohibition). *(→ PRD AC-2)*
- **SPEC-AC-3:** The `version` field value equals the value returned by
  `Assembly.GetEntryAssembly()`'s informational version at test-run time
  (asserted by reading the same attribute in the integration test, not a
  hard-coded literal). *(→ PRD AC-3)*
- **SPEC-AC-4:** A unit test (`HealthEndpointTests`) using
  `FakeAppVersionProvider` asserts `HealthEndpoint.Handle(...)` returns a
  `HealthResponse` with `Status == "healthy"` and `Version` equal to the
  fake's configured value. *(→ PRD AC-4)*
- **SPEC-AC-5:** An integration test boots the app via
  `WebApplicationFactory<Program>` and asserts `GET /health` returns 200
  with a deserializable JSON body matching the contract. *(→ PRD AC-5)*
- **SPEC-AC-6:** An integration test asserts `POST /health` returns
  `405`. *(→ PRD AC-6)*
- **SPEC-AC-7:** An integration test asserts `GET /not-a-real-route`
  returns `404`. *(→ PRD AC-7)*
- **SPEC-AC-8:** `dotnet build --configuration Release --no-restore`
  succeeds with zero errors and zero analyzer warnings (warnings are
  errors). *(→ PRD AC-8)*
- **SPEC-AC-9:** `.github/workflows/ci.yml` runs on push and pull_request
  and executes, at minimum: restore, build, unit tests, integration
  tests, `dotnet format --verify-no-changes`, and `gitleaks detect
  --no-banner --exit-code 1`; the workflow fails if any step fails.
  *(→ PRD AC-9)*
- **SPEC-AC-10 (procedural, not automated):** `dotnet run --project
  src/HealthApi` starts successfully and a manual `curl
  http://localhost:<port>/health` (or the configured URL) returns 200 with
  the expected JSON body. This is documented in `README.md` as a manual
  check, consistent with PRD wording. *(→ PRD AC-10)*
- **SPEC-AC-11:** The integration test for `GET /health` (SPEC-AC-5) sends
  no `Authorization` header and still receives 200, demonstrating no
  authentication is required. *(→ PRD AC-11)*
- **SPEC-AC-12:** Review of the diff confirms no database, ORM, message
  queue, external HTTP client integration, or UI/frontend project was
  added. *(→ PRD AC-12; verified at `/review`, not by an automated test)*
- **SPEC-AC-13:** Review of the diff confirms no Dockerfile, IaC file, or
  cloud deployment configuration was added. *(→ PRD AC-13; verified at
  `/review`, not by an automated test)*
- **SPEC-AC-14:** `dotnet format --verify-no-changes` passes with no
  formatting violations.
- **SPEC-AC-15:** `gitleaks detect --no-banner --exit-code 1` passes
  locally/in CI with no findings.

## Testing Requirements

- **Unit tests (xUnit, `tests/HealthApi.UnitTests`):**
  - `HealthEndpointTests`: covers SPEC-AC-4, using
    `FakeAppVersionProvider` (a hand-written `IAppVersionProvider`
    implementation returning a fixed test string) — no mocking framework
    (ADR-004).
- **Integration tests (xUnit + `WebApplicationFactory<Program>`,
  `tests/HealthApi.IntegrationTests`):**
  - Success path: `GET /health` → 200, valid JSON, correct field
    presence (SPEC-AC-2, SPEC-AC-5).
  - Version correctness: response `version` equals the real assembly
    informational version (SPEC-AC-3).
  - Negative path — method: `POST /health` → 405 (SPEC-AC-6).
  - Negative path — route: `GET /not-a-real-route` → 404 (SPEC-AC-7).
  - No-auth path: `GET /health` without any `Authorization` header → 200
    (SPEC-AC-11).
- **Contract tests:** Not Applicable — no external consumer contract
  (ADR-004).
- **E2E tests:** Not Applicable — no UI or multi-service flow (ADR-004).
- **Boundary tests:** query-string and trailing-slash cases (Behavior →
  Edge Cases) may be included as additional integration test cases but
  are not required beyond the primary paths above, since they follow
  unmodified framework default behavior.
- **Security tests:** Not Applicable as a dedicated test category (no
  auth to test); supply-chain checks (NuGet Audit, `gitleaks`) serve as
  the security verification for this spec, run via CI rather than xUnit.

## Verification Requirements

`/verify` must produce deterministic evidence for each SPEC-AC by running
the exact commands recorded in `AGENTS.md`:

1. `dotnet restore` — succeeds; NuGet Audit reports no unaddressed
   vulnerable-package findings.
2. `dotnet build --configuration Release --no-restore` — succeeds with
   zero errors/warnings (SPEC-AC-8).
3. `dotnet test tests/HealthApi.UnitTests --configuration Release
   --no-build` — all tests pass (SPEC-AC-4).
4. `dotnet test tests/HealthApi.IntegrationTests --configuration Release
   --no-build` — all tests pass (SPEC-AC-2, SPEC-AC-3, SPEC-AC-5,
   SPEC-AC-6, SPEC-AC-7, SPEC-AC-11).
5. `dotnet format --verify-no-changes` — passes (SPEC-AC-14).
6. `gitleaks detect --no-banner --exit-code 1` — passes (SPEC-AC-15).
7. Inspection that `.github/workflows/ci.yml` exists and its steps match
   the commands above (SPEC-AC-9).
8. Manual/documented check of `dotnet run` + `curl` is reported as a
   procedural confirmation, not a `/verify` pass/fail gate (SPEC-AC-10).
9. Diff/dependency review confirming no excluded technology was
   introduced (SPEC-AC-12, SPEC-AC-13) — performed at `/review`, but
   `/verify` should note that `HealthApi.csproj` and the test projects'
   `<PackageReference>` lists contain no unexpected packages beyond those
   named in this spec.

## Definition of Done

This specification is complete only when:

- All files in the Repository Layout section exist with the described
  responsibilities.
- All SPEC-ACs above have deterministic passing evidence from the
  commands in Verification Requirements (except SPEC-AC-10, which is
  procedural).
- `/verify` returns `DONE`.
- `/review` reaches `APPROVE` (DeepSeek pre-review
  `READY_FOR_SENIOR_REVIEW` → Sonnet `APPROVE`).
- The GitHub Actions CI workflow passes on the PR.
- Merge occurs through the normal PR process.
- **Outside this spec's automated scope, but required before CI security
  posture is complete:** the repository owner enables GitHub Dependabot
  alerts in repository settings (Security → Code security and analysis).
  This is a one-time GitHub configuration action, not a code change, and
  must be tracked as a follow-up operator action rather than blocking
  `/verify`/`/review` for this spec.

## Parallelism and Branch Safety

Not Applicable beyond a single branch. This is the only specification for
this project (PRD non-goal NG8 guarantees no future endpoint-adding spec).
Implement on a single dedicated branch:

`spec/SPEC-001-health-endpoint-and-ci-bootstrap`

No concurrent worktree/parallel-implementation split is needed or
recommended for this spec.

## Traceability

| SPEC-AC | PRD Reference | Architecture/ADR Reference |
| --- | --- | --- |
| SPEC-AC-1 | PRD AC-1, FR-1, FR-2 | ARCH-001 Data Flow |
| SPEC-AC-2 | PRD AC-2, FR-3 | ARCH-001 Major Components (Health Endpoint Handler) |
| SPEC-AC-3 | PRD AC-3, FR-5 | ARCH-001 Major Components (Version Provider); ADR-001 |
| SPEC-AC-4 | PRD AC-4 | ADR-004 (unit testing approach) |
| SPEC-AC-5 | PRD AC-5 | ADR-004 (`WebApplicationFactory`) |
| SPEC-AC-6 | PRD AC-6, FR-10 | ARCH-001 Failure Modes/Data Flow |
| SPEC-AC-7 | PRD AC-7, FR-10 | ARCH-001 Failure Modes/Data Flow |
| SPEC-AC-8 | PRD AC-8 | ARCH-001 Code Quality Tooling |
| SPEC-AC-9 | PRD AC-9, Goal G5 | ADR-003 (CI provider) |
| SPEC-AC-10 | PRD AC-10 | ARCH-001 Local Development |
| SPEC-AC-11 | PRD AC-11, FR-6, NG2 | ARCH-001 Security Model |
| SPEC-AC-12 | PRD AC-12, NG1/NG3/NG4/NG5 | ARCH-001 Technology Baseline |
| SPEC-AC-13 | PRD AC-13, NG6 | ADR-002 |
| SPEC-AC-14/15 | PRD Goal G5 | ARCH-001 Code Quality Tooling |

No new product requirement is introduced by this specification; every
behavior above is a direct implementation of an existing PRD-001
requirement or an explicitly architecture-owned technical detail (e.g.,
the exact `status` literal, per PRD assumption A-1).
