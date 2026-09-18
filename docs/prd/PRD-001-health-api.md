# PRD-001: Health API (Engineering Workflow Smoke Test)

## Document Metadata

- **Document ID:** PRD-001
- **Title:** Tiny Production-Grade HTTP Health API
- **Version:** 0.1
- **Status:** Proposed — Ready for Architecture
- **Owner:** Product (workflow-smoke initiative)
- **Related Documents:**
  - `docs/architecture/` (to be created by `/architect`)
  - `docs/adr/` (to be created by `/architect` if a significant decision arises)
  - `docs/specs/` (to be created by `/spec`)

## Problem Statement

The team needs a minimal, realistic project to validate the full engineering
workflow (`/prd → /architect → /project-init → /spec → /implement → /verify →
/review → CI`) end-to-end, using real code, real automated tests, and a real
CI pipeline — without the scope, cost, or risk of a real product feature.

A tiny HTTP Health API is well suited to this purpose: it has a simple,
well-understood contract; it requires genuine implementation, testing, and CI
work; but it carries no data-ownership, authentication, or infrastructure
complexity that would distract from validating the process itself.

This matters because the workflow (PRD → architecture → spec → implementation
→ verification → review) must be exercised and trusted before it is relied on
for real product work.

## Goals

- **G1:** Deliver a working `GET /health` endpoint that returns HTTP 200 with
  a JSON body containing a status indicator and the application's version.
- **G2:** Exercise the complete engineering workflow at least once end-to-end
  (PRD, architecture, project-init, spec, implementation, verification,
  review, CI) using this API as the vehicle.
- **G3:** Keep infrastructure cost at zero or near-zero — the system runs
  only on a developer machine and inside CI compute; no persistent cloud
  infrastructure is provisioned.
- **G4:** Provide automated unit and integration tests that deterministically
  pass in CI.
- **G5:** Provide a CI pipeline that gates merges on build success, test
  success, and code-quality checks (lint/format/static analysis).

## Non-Goals

- **NG1:** No database or persistence layer.
- **NG2:** No authentication or authorization.
- **NG3:** No message queues or asynchronous processing.
- **NG4:** No external system integrations.
- **NG5:** No UI or frontend.
- **NG6:** No production deployment or cloud hosting as part of this work.
- **NG7:** No multi-environment promotion pipeline (staging/production); CI
  validation only.
- **NG8:** No additional business API endpoints beyond `/health`.

## Users and Actors

- **Primary user:** Engineering workflow operator (human or agent) validating
  the pipeline by implementing and running this API.
- **System actor:** CI system executing build/test/quality gates on push/PR.
- **Hypothetical consumer (not exercised here):** monitoring/uptime tooling
  that would call `GET /health` if this were a real deployed service.
- **Operators/admins:** none — there is no runtime configuration surface
  beyond reporting the application version.
- **External systems:** none.

## User Journeys

1. **Implementation:** A developer/agent implements the Health API per the
   approved specification, runs it locally, and issues a manual `GET
   /health` request (e.g., via `curl`), observing HTTP 200 and the expected
   JSON body.
2. **Automated verification:** The CI pipeline runs on push/PR: restores
   dependencies, builds the project, runs unit and integration tests, and
   runs code-quality checks (lint/format/static analysis). The pipeline
   fails the build if any step fails.
3. **Review:** A reviewer (AI pre-review, then senior review) inspects the
   change, confirms CI is green and acceptance criteria are met, and
   approves per the standard review workflow.
4. **(Out of scope) Operation:** In a real deployment, an operator would
   point uptime/monitoring tooling at `/health`. This is not performed as
   part of this workflow smoke test.

## Functional Requirements

- **FR-1:** The system exposes an HTTP `GET` endpoint at path `/health`.
- **FR-2:** A successful request to `GET /health` returns HTTP status code
  `200`.
- **FR-3:** The response body is valid JSON containing, at minimum, a
  `status` field and a `version` field.
- **FR-4:** The `status` field's value indicates the service is healthy
  (exact literal value is a non-blocking implementation detail — see
  Assumptions).
- **FR-5:** The `version` field reports the application's current version,
  sourced from build/assembly/package metadata rather than a value
  hard-coded independently of the build.
- **FR-6:** The `/health` endpoint does not require authentication or
  authorization to access.
- **FR-7:** The endpoint does not require any request body, query
  parameters, or non-standard headers to produce a successful response.
- **FR-8:** The endpoint responds within the bounds defined in Performance
  Requirements under normal local/CI operating conditions.
- **FR-9:** The application can be started and stopped locally via a
  documented command and listens on a default or configurable local port.
- **FR-10:** Requests to undefined routes return HTTP `404`. Non-`GET`
  methods on `/health` (e.g., `POST`, `PUT`, `DELETE`) return HTTP `405` or
  an equivalent client-error status, per standard framework routing
  behavior.

## Validation and Error Behavior

- `GET /health` accepts no user-supplied input, so no request-body or
  parameter validation rules apply.
- If the service fails to start (e.g., port already in use), it must fail
  fast with a non-zero process exit code and a clear log message; it must
  not silently hang or begin serving traffic in a broken state.
- `GET /health` is idempotent and safe to call repeatedly; repeated calls
  must not produce side effects or state changes.
- No application-level retry logic is required; callers may apply their own
  retry policy.
- Requests using an unsupported HTTP method or an undefined path must return
  a standard HTTP client-error status rather than HTTP 200 or an unhandled
  exception (HTTP 500) with an exposed stack trace.

## Security and Privacy Requirements

- No authentication or authorization is required for `/health` (explicit
  non-goal).
- No secrets, credentials, or sensitive data are handled, stored, or logged
  by this service.
- No personally identifiable information (PII) is processed, stored, or
  logged.
- Error responses and normal responses must not leak internal implementation
  details (stack traces, file paths, environment variable values,
  connection strings) in the response body.
- Third-party dependency/security scanning must run in CI per project-wide
  policy, even though this service has minimal external dependencies.

## Reliability Requirements

- The service runs as a stateless, single-instance process for local and CI
  use; no clustering, high availability, or automated failover is required
  (consistent with the non-goal of production deployment).
- No RTO/RPO targets apply — the service holds no persistent state and is
  not deployed to production as part of this work.
- The service must start and become ready to serve `/health` within a
  bounded, short startup time suitable for local development and CI (see
  Performance Requirements).

## Performance Requirements

- **Latency:** In a local/CI environment, `GET /health` should respond in
  well under 1 second under single-request, non-concurrent load (informal
  target: under 200ms). No formal production load/throughput SLA applies,
  since there is no production traffic.
- **Payload size:** The response body is small — well under 1 KB.
- **Scale:** No specific concurrency or throughput target is required beyond
  handling normal automated test traffic (unit/integration test runs and
  occasional manual verification calls).

## Observability Requirements

- The application should emit basic structured startup/shutdown log lines
  (e.g., "listening on port X", "shutting down") to standard output,
  consistent with typical framework defaults.
- No metrics, tracing, or alerting infrastructure is required, given the
  non-goal of production deployment.
- Test and CI output must be readable enough for a human or an agent to
  determine pass/fail without additional tooling.

## Cost Constraints

- **Target cost:** $0 recurring infrastructure cost. The service runs
  entirely on a developer machine and inside CI compute already covered by
  existing CI tooling/minutes.
- No cloud compute, storage, or networking resources may be provisioned as
  part of this workflow smoke test.
- CI must use standard, minimal-cost runners/images; no premium or
  dedicated infrastructure is permitted.

## Acceptance Criteria

- **AC-1:** A `GET /health` request against a running instance returns HTTP
  `200`.
- **AC-2:** The response body is valid JSON and contains a `status` field
  and a `version` field.
- **AC-3:** The `version` field's value matches the application's actual
  build/assembly version at request time, verifiable by an automated test
  comparing the response to a known build version.
- **AC-4:** An automated unit test suite exists and passes, covering at
  minimum the `/health` handler's success response (status code and body
  shape).
- **AC-5:** An automated integration test suite exists and passes, exercising
  the running HTTP server end-to-end for `GET /health`.
- **AC-6:** An automated test verifies that a non-`GET` request to `/health`
  (e.g., `POST`) returns a non-200 HTTP status (405 or equivalent).
- **AC-7:** An automated test verifies that an undefined route returns HTTP
  `404`.
- **AC-8:** The application builds successfully via the documented build
  command with zero errors.
- **AC-9:** A CI pipeline runs on push/PR performing, at minimum: dependency
  restore, build, unit tests, integration tests, and a code-quality check
  (lint/format/static analysis); the pipeline fails if any step fails.
- **AC-10:** The service can be started locally via a documented command and
  manually verified (e.g., via `curl`) to respond correctly on `/health`.
- **AC-11:** An automated test verifies that `/health` is reachable without
  any authentication credentials.
- **AC-12:** Code/dependency review during `/review` confirms no database,
  message queue, external integration, or UI component was introduced.
- **AC-13:** Review of changed files confirms no cloud infrastructure or
  production-deployment artifacts were created or applied as part of this
  work.

## Assumptions (Non-Blocking)

- **A-1:** The `status` field's value will be a fixed literal such as `"ok"`
  or `"healthy"`; the exact wording is finalized at `/spec` and is not
  product-critical.
- **A-2:** The `version` value will be a semantic-version-style string
  (e.g., `"1.0.0"`) sourced from project/assembly metadata (e.g., the
  `.csproj` `Version` property), not from a database or external service.
- **A-3:** The service listens on a standard local default port (e.g., the
  .NET/Kestrel default) or an environment-configurable port; no specific
  port number is mandated by product requirements.
- **A-4:** "Automated tests" means unit tests plus HTTP-level integration
  tests using standard .NET testing conventions; no browser-based E2E
  testing is needed since there is no UI.
- **A-5:** The specific CI provider (e.g., GitHub Actions or another CI
  system) is an architecture/project-init decision, not a product decision.
- **A-6:** "Code-quality checks" means, at minimum, formatting verification
  (e.g., `dotnet format --verify-no-changes`) and static analysis/linting
  appropriate to C#/.NET; exact tool selection is an architecture/
  project-init decision.
- **A-7:** HTTPS/TLS is not required for local/CI validation. If the chosen
  framework enables HTTPS redirection by default, tests may call the HTTP
  endpoint directly; no certificate management is in scope.
- **A-8:** This service is never exposed to the public internet as part of
  this workflow; "runs locally" means developer machine and CI runners
  only.
- **A-9:** There is exactly one environment (local/CI); no staging or
  production environment configuration is required at this stage.

## Open Questions

### Blocking

None. No unresolved decision prevents safe architecture work.

### Non-Blocking

- **OQ-1:** Should the health response include additional fields in the
  future (e.g., uptime, dependency sub-checks) to more closely resemble a
  real production health endpoint? Deferred — out of current scope; may be
  addressed in a future PRD revision if the smoke test is extended.
- **OQ-2:** Should CI also publish a build artifact (e.g., a container
  image) even though no deployment occurs, to further exercise the
  pipeline? Deferred to `/architect`; not required by this PRD.

## Technical Constraints (Explicit, User-Imposed)

- The application technology is constrained to **C#/.NET** per explicit user
  instruction. This is a product-level constraint carried into architecture,
  not an architecture-stage technology choice; `/architect` selects the
  specific .NET version, ASP.NET Core hosting model, test frameworks, and
  code-quality tooling within this constraint.
