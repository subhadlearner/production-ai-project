# Project Instructions

## Project Overview

This repository implements **PRD-001: Tiny Production-Grade HTTP Health
API**, a minimal engineering-workflow smoke test service. It exposes a
single `GET /health` endpoint returning HTTP 200 with a JSON body
containing a `status` indicator and the application's version, sourced
from build/assembly metadata rather than a hand-maintained literal.

The project has no business domain beyond validating the health-check
contract itself. It intentionally has no persistence, authentication,
messaging, external integrations, or UI (see
`docs/prd/PRD-001-health-api.md`, non-goals NG1–NG8). Its purpose is to
exercise the full `/prd → /architect → /project-init → /spec → /implement
→ /verify → /review` workflow end-to-end with real code, real automated
tests, and a real CI pipeline, at zero/near-zero infrastructure cost.

Approved project documents:

- PRD: `docs/prd/PRD-001-health-api.md`
- Architecture: `docs/architecture/ARCH-001-health-api.md`
- ADRs: `docs/adr/ADR-001-language-runtime-framework.md`,
  `docs/adr/ADR-002-zero-infrastructure-boundary.md`,
  `docs/adr/ADR-003-ci-provider.md`, `docs/adr/ADR-004-testing-stack.md`

## Technology

The approved technology stack was determined during `/architect` (see
`docs/architecture/ARCH-001-health-api.md`, Stage 10 handoff).

Implementation agents must not invent or silently replace the technology
stack.

- Runtime: .NET 10 (LTS)
- Language: C# (latest supported by the .NET 10 SDK)
- Framework: ASP.NET Core, Minimal API hosting model
- Database: Not Applicable — no persistence (PRD non-goal NG1; ADR-002)
- Cloud: Not Applicable — local + CI execution only, no cloud deployment (PRD non-goal NG6; ADR-002)
- Region: Not Applicable
- Infrastructure as Code: Not Applicable — no infrastructure is provisioned (ADR-002)
- Package Manager: NuGet via the `dotnet` CLI
- Unit Test Framework: xUnit
- Integration Test Framework: `Microsoft.AspNetCore.Mvc.Testing` (`WebApplicationFactory<Program>`) + xUnit
- E2E Test Framework: Not Applicable — no UI or multi-service flow (ADR-004)
- Linting: Built-in Roslyn analyzers (`EnableNETAnalyzers`, `AnalysisLevel=latest-recommended`), enforced via `TreatWarningsAsErrors`
- Formatting: `dotnet format`
- Type Checking / Static Analysis: C# compiler with `<Nullable>enable</Nullable>`, warnings treated as errors
- Security / Dependency Scanning: NuGet Audit (built-in .NET 10 SDK) + GitHub Dependabot alerts + `gitleaks` secret scanning in CI

If a technology decision required for implementation is missing or materially ambiguous, implementation must stop rather than guess.

## Branch and Worktree Policy

Do not implement application changes directly on protected integration branches.

Protected branches include:

- `main`
- `master`
- `develop`
- `release`
- any repository-defined protected integration branch

Each specification should be implemented on a dedicated branch.

Preferred branch naming:

`spec/<spec-id>-<short-description>`

Example:

`spec/SPEC-001-create-short-url`

Before implementation begins:

- confirm the current branch
- confirm the working tree is clean enough to isolate the requested work
- do not mix unrelated uncommitted changes into the implementation
- create or switch to the dedicated specification branch

For parallel implementation:

- use a separate branch for each specification
- use a separate Git worktree for each concurrently mutating implementation
- do not allow multiple implementation agents to modify the same files concurrently

`/fix`, `/verify`, and `/review` must operate on the existing implementation branch.

They must not create a new branch for the same specification.

Do not automatically:

- merge into a protected branch
- rebase shared branches
- reset unrelated changes
- discard unrelated work
- force-push

Merge should happen through the normal PR/CI process.

## Build and Verification Commands

The commands below reflect the actual project configuration, per
`docs/architecture/ARCH-001-health-api.md` (Stage 10 handoff).

Do not invent tools or commands simply because they are common for the language or framework.

Repository layout these commands assume:

- Solution: `HealthApi.sln`
- Application project: `src/HealthApi/HealthApi.csproj`
- Unit test project: `tests/HealthApi.UnitTests/HealthApi.UnitTests.csproj`
- Integration test project: `tests/HealthApi.IntegrationTests/HealthApi.IntegrationTests.csproj`
- SDK pin: `global.json` targeting the .NET 10 SDK

### Dependency Restore / Install

```
dotnet restore
```

(Also triggers the built-in NuGet Audit vulnerability check — see Security / Dependency Checks.)

### Build

```
dotnet build --configuration Release --no-restore
```

Analyzers (`EnableNETAnalyzers`, `AnalysisLevel=latest-recommended`) and
`TreatWarningsAsErrors=true` run as part of this command — there is no
separate lint step (see Lint below).

### Unit Tests

```
dotnet test tests/HealthApi.UnitTests --configuration Release --no-build
```

### Integration Tests

```
dotnet test tests/HealthApi.IntegrationTests --configuration Release --no-build
```

### E2E Tests

Not Applicable — no UI or multi-service flow (PRD non-goals NG5; ADR-004).

### Lint

Included in the Build command above. Static analysis runs in-line via
built-in Roslyn analyzers with warnings treated as errors; there is no
separate lint invocation.

### Formatting Verification

```
dotnet format --verify-no-changes
```

### Type Checking / Static Analysis

Included in the Build command above (Roslyn compiler with
`<Nullable>enable</Nullable>`, warnings treated as errors).

### Security / Dependency Checks

```
dotnet restore
gitleaks detect --no-banner --exit-code 1
```

`dotnet restore` triggers the .NET 10 SDK's built-in NuGet Audit for
known-vulnerable packages. `gitleaks` performs secret scanning in CI.
GitHub Dependabot alerts run natively on the hosted repository (no local
command).

### Infrastructure Validation

Not Applicable — no IaC exists (PRD non-goal NG6; ADR-002).

## Architecture Constraints

- Follow approved architecture documents under `docs/architecture/`.
- Follow approved ADRs under `docs/adr/`.
- Do not introduce new infrastructure, persistence technology, frameworks, runtimes, or major abstractions without an approved architecture decision.
- Prefer the simplest production-grade solution that satisfies the requirements.
- Do not silently redesign the system during implementation.
- Do not silently change public contracts.
- Do not silently change consistency, reliability, security, or persistence guarantees.
- Architecture decisions take precedence over implementation convenience.

### Project-Specific Constraints (PRD-001 / ARCH-001)

- Exactly one HTTP endpoint exists: `GET /health`. Do not add other
  business endpoints (PRD non-goal NG8).
- Use the ASP.NET Core **Minimal API** hosting model. Do not switch to MVC
  controllers (ADR-001).
- Do not introduce a database, ORM, message queue, external HTTP client
  integration, or UI/frontend project (PRD non-goals NG1, NG3, NG4, NG5).
- Do not add a Dockerfile, container publish step, IaC, or any cloud
  deployment target (PRD non-goal NG6; ADR-002). This project runs only as
  a local process and inside CI compute.
- Do not add authentication/authorization middleware (PRD non-goal NG2).
- Source the `version` field from build/assembly metadata (e.g., the
  `.csproj` `Version`/`InformationalVersion` property via
  `System.Reflection`) — never hard-code it as a literal separate from the
  build (PRD FR-5).

## Coding Conventions

- Repository layout: `HealthApi.sln` at the repo root;
  `src/HealthApi/HealthApi.csproj` for the application;
  `tests/HealthApi.UnitTests/` and `tests/HealthApi.IntegrationTests/` for
  tests; `global.json` pinning the .NET 10 SDK at the repo root.
- Enable `<Nullable>enable</Nullable>` and
  `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` for all projects
  (e.g., via a root `Directory.Build.props`).
- Use `System.Text.Json` for response serialization (ASP.NET Core
  default) — do not add a third-party JSON library.
- Isolate version lookup behind a small `IAppVersionProvider` abstraction
  so unit tests can substitute a hand-written fake without a mocking
  framework (ADR-004); do not add Moq/NSubstitute or similar for this.
- Keep `Program.cs` a Minimal API composition root: route registration,
  logging configuration, and host startup only — no business logic beyond
  what the health handler needs.

## Technology Decision Authority

Technology decisions are owned by the architecture stage.

The authority chain is:

1. `/prd` defines requirements and constraints.
2. `/architect` selects and approves the technology stack.
3. `/project-init` records and operationalizes those decisions.
4. `/spec` decomposes the approved design.
5. `/implement` executes the approved specification.

`/project-init`, `/spec`, `/implement`, `/verify`, `/fix`, and `/review` must not independently replace the approved technology stack.

If a required technology decision is missing, return to architecture rather than guessing.

## Project Initialization

Before implementation begins, `/project-init` must prepare the repository using the approved architecture and ADRs.

It is responsible for synchronizing the approved project configuration into:

- `AGENTS.md`
- `README.md`
- `.kilo/rules/`
- `.kilo/skills/`

`/project-init` must not implement application functionality.

It must stop with:

`PROJECT_INIT_BLOCKED`

if a required technology decision is missing or conflicting.

## Project Rules

Project-specific rules may be stored under:

`.kilo/rules/`

Rules should contain focused instructions that apply broadly to the repository.

Examples:

- framework conventions
- cloud conventions
- testing conventions
- security conventions
- observability conventions

Do not create rules that merely duplicate this file.

Prefer concise implementation constraints over tutorials.

## Skills

Project skills are stored under:

`.kilo/skills/`

Skills are a curated dependency layer.

Use skills only when they materially improve implementation quality.

Prefer skills in this order:

1. official/vendor-maintained skills
2. reputable and actively maintained community skills
3. adapted project-specific versions of reputable skills
4. custom project-specific skills when necessary

Potential sources include:

- official vendor repositories
- skills.sh
- reputable open-source Agent Skill collections

Third-party skills must be treated as untrusted until reviewed.

Do not silently install third-party skills.

Explicit user approval is required before installing third-party skills.

Before recommending or installing a skill, evaluate:

- source and maintainer reputation
- whether the source is official/vendor maintained
- maintenance activity
- relevance to the approved stack
- bundled scripts or executable resources
- unexpected shell behavior
- unexpected network behavior
- overlap with existing project instructions

Keep the installed skill set small and relevant.

Do not install broad skill collections unnecessarily.

Project-specific skills should be used for guidance that generic technology skills cannot know, such as:

- repository-specific conventions
- domain rules
- project-specific persistence patterns
- logging conventions
- retry/idempotency policies
- organization-specific API behavior
- approved infrastructure patterns

## Specification Discipline

Implementation must be based on a specification under:

`docs/specs/`

Specifications should define, where applicable:

- objective
- scope
- non-goals
- dependencies
- architecture references
- interfaces
- data model
- behavior
- validation
- error handling
- security
- observability
- performance
- cost implications
- acceptance criteria
- testing requirements
- definition of done

Target specification size:

- preferred: 30K–60K tokens
- warning: 60K–80K tokens
- hard ceiling: 100K tokens

Split larger work into independently implementable specifications.

A specification must remain consistent with the approved architecture.

## Implementation Rules

- Implement only the approved specification.
- Reuse existing project conventions.
- Follow the technology stack recorded in this file.
- Follow relevant project rules and skills.
- Keep changes incremental and reversible.
- Do not invent requirements.
- Do not silently broaden scope.
- Do not weaken tests merely to make them pass.
- Do not remove validation or error handling without explicit justification.
- Avoid unnecessary abstractions and dependencies.
- Prefer straightforward code over speculative extensibility.
- Do not introduce a new major dependency without approval.
- Do not change architecture merely to simplify implementation.

## Testing Requirements

Use the appropriate combination of:

- unit tests
- integration tests
- contract tests
- E2E tests
- negative tests
- boundary tests
- security tests

A specification is not complete until all required applicable tests pass.

Do not classify a missing required test as `NOT_APPLICABLE` merely because it has not yet been implemented.

### Project-Specific Testing Requirements

- Unit tests (xUnit) must cover the `/health` handler's success response
  shape and status code using a fake `IAppVersionProvider` (PRD AC-4).
- Integration tests (xUnit + `WebApplicationFactory<Program>`) must cover:
  successful `GET /health` returning 200 with valid JSON (AC-1, AC-2), the
  `version` field matching the real assembly version (AC-3), a non-`GET`
  method returning a non-200 status (AC-6), an undefined route returning
  404 (AC-7), and an unauthenticated request succeeding (AC-11).
- Contract tests, E2E tests, and security-specific test suites are Not
  Applicable for this project (no external consumer contract, no UI, no
  auth to security-test) — do not report them as missing; they are
  correctly out of scope per ADR-004.
- Manually starting the service (`dotnet run`) and verifying `GET /health`
  via `curl` (AC-10) is a documented procedural check, not an automated
  test.

## Verification Workflow

`/verify` provides deterministic evidence about the implementation.

Verification must use the actual project commands defined in this file or repository configuration.

Do not invent verification tools merely because they are common for the technology stack.

The only valid final verification statuses are:

`DONE`

or

`NOT_DONE`

`DONE` means:

- all required applicable verification checks passed
- every required acceptance criterion has deterministic passing evidence
- no verification blocker remains

`NOT_DONE` means one or more required conditions are not satisfied.

If `/verify` returns `NOT_DONE`, do not proceed to `/review`.

Use `/fix`.

## Repair Workflow

Use `/fix` after any of the following:

- `/verify` returns `NOT_DONE`
- pre-review returns `CHANGES_REQUIRED`
- senior review returns `REQUEST CHANGES`

The repair flow is:

1. inspect the latest verification or review blockers
2. determine the smallest correct repair
3. repair locally when consistent with the approved specification and architecture
4. use root-cause debugging when the cause is unclear or repeated attempts fail
5. run focused validation
6. return control to `/verify`

`/fix` must not decide completion.

Only `/verify` may return `DONE`.

The successful repair handoff is:

`RUN_VERIFY`

If repair requires an upstream product, architecture, project-init, or specification change, `/fix` must return `FIX_BLOCKED` and identify:

- blocking issue
- owner
- why it blocks
- required action
- exact next command

Do not silently redesign the system.

Avoid repeated speculative repair attempts.

## Verification and Repair Loop

The required implementation loop is:

```text
/implement
   ↓
RUN_VERIFY
   ↓
/verify
   │
   ├── DONE ──────────────→ /review
   │
   └── NOT_DONE
          ↓
        /fix
          ↓
       RUN_VERIFY
          ↓
        /verify
```

## Review Workflow

Review occurs only after `/verify` returns `DONE`.

The review pipeline is:

1. DeepSeek pre-review
2. Claude Sonnet senior review only if pre-review returns `READY_FOR_SENIOR_REVIEW`

Possible pre-review outcomes:

- `READY_FOR_SENIOR_REVIEW`
- `CHANGES_REQUIRED`

Possible senior-review outcomes:

- `APPROVE`
- `REQUEST CHANGES`

If pre-review returns `CHANGES_REQUIRED`:

```text
/review
   ↓
CHANGES_REQUIRED
   ↓
/fix
   ↓
/verify
   ↓
/review
```

If senior review returns `REQUEST CHANGES`:

```text
/review
   ↓
REQUEST CHANGES
   ↓
/fix
   ↓
/verify
   ↓
/review
```

The senior reviewer must not be invoked when the pre-review has blocking findings.

AI review does not replace deterministic CI.

## Definition of Done

A specification is complete only when:

- implementation is complete
- required applicable tests exist
- `/verify` returns `DONE`
- `/review` reaches `APPROVE`
- CI passes
- merge occurs through the normal PR process

Production deployment remains a separate human-approved action.

## Security

Never:

- hard-code credentials or secrets
- print secrets in logs or command output
- weaken authentication or authorization to obtain a pass
- disable required security checks
- commit local credential material

Treat these as sensitive by default:

- `.env`
- `.env.*`
- `*.pem`
- `*.key`
- cloud credentials
- API tokens
- private certificates

### Project-Specific Security Constraints

- `/health` requires no authentication or authorization by design (PRD
  non-goal NG2) — do not add any.
- No secrets or credentials exist in this project; none should ever be
  introduced.
- Responses and logs must never include stack traces, file paths, or
  environment variable values.
- CI must run `gitleaks detect --no-banner --exit-code 1` on every
  push/PR, and `dotnet restore` (built-in NuGet Audit) to catch
  known-vulnerable packages.

## Cloud and Cost

Cloud cost is a first-class implementation concern.

Where applicable, consider:

- fixed monthly cost
- variable usage cost
- storage cost
- data-transfer cost
- observability cost
- scaling behavior
- operational burden
- failure and recovery cost

Prefer managed or serverless services when they provide the best balance of reliability, simplicity, and cost.

Do not introduce always-on or premium infrastructure unless justified by approved requirements.

### Project-Specific Cloud and Cost Constraints

- Target cost is $0 recurring (PRD Goal G3; ADR-002). No cloud compute,
  storage, or networking resource may be provisioned.
- The only compute used is the developer's local machine and GitHub
  Actions `ubuntu-latest` CI runners (ADR-003).
- Keep CI to a single job/single OS target with NuGet caching to minimize
  CI-minute usage — do not add a build matrix without a new architecture
  decision.

## Deployment

Infrastructure and deployment must follow approved architecture and IaC decisions.

Do not manually create production cloud resources when IaC is required.

CI/CD must enforce applicable build, test, quality, security, and IaC gates.

Do not auto-deploy to production.

Production deployment requires explicit human approval.

### Project-Specific Deployment Constraints

- There is no production deployment target for this project (PRD
  non-goal NG6; ADR-002). Do not add deployment workflows, cloud
  provisioning, or IaC.
- The only environment is local/CI (PRD assumption A-9). Do not add
  staging/production configuration.
- CI (`.github/workflows/ci.yml`) enforces build, unit tests, integration
  tests, formatting verification, and secret scanning on every push/PR
  (ADR-003). Do not bypass it to merge.

## Workflow Ownership

The normal lifecycle is:

```text
/prd
  ↓
PRD_READY
  ↓
/architect
  ├─ ARCHITECTURE_BLOCKED → resolve → /architect
  └─ ARCHITECTURE_READY
            ↓
      /project-init
  ├─ PROJECT_INIT_BLOCKED → resolve as directed
  └─ PROJECT_INIT_READY
            ↓
          /spec
  ├─ SPEC_BLOCKED → resolve as directed
  └─ SPEC_READY
            ↓
       /implement
  ├─ IMPLEMENTATION_BLOCKED → resolve as directed
  └─ RUN_VERIFY
            ↓
         /verify
  ├─ NOT_DONE → /fix → /verify
  └─ DONE
       ↓
     /review
       ↓
DeepSeek pre-review
  ├─ CHANGES_REQUIRED → /fix → /verify → /review
  └─ READY_FOR_SENIOR_REVIEW
                   ↓
            Sonnet senior review
  ├─ REQUEST CHANGES → /fix → /verify → /review
  └─ APPROVE
       ↓
       CI
       ↓
   PR / merge
       ↓
human-approved production deployment
```

Blocked stages must state the owner, required action, and exact next command rather than leaving the operator to infer the recovery path.
