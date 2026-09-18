# Health API — Engineering Workflow Smoke Test

A tiny, production-grade HTTP Health API used to validate the full
AI-assisted engineering workflow end-to-end (PRD → architecture →
project-init → spec → implement → verify → review → CI), at zero/near-zero
infrastructure cost.

## Purpose

This repository implements **PRD-001** (`docs/prd/PRD-001-health-api.md`):
a single `GET /health` endpoint that returns HTTP 200 with a JSON body
containing a `status` indicator and the application's version, sourced
from build/assembly metadata. It has no database, authentication, message
queue, external integration, or UI by design (PRD non-goals NG1–NG8) —
its only purpose is to give the engineering workflow a real, minimal, but
genuinely production-shaped piece of software to exercise.

**Status:** PRD, architecture, and ADRs are approved; SPEC-001
(`docs/specs/SPEC-001-health-endpoint-and-ci-bootstrap.md`) is implemented.
The application, its unit and integration test suites, and the CI workflow
exist and pass locally. `/verify` and `/review` have not yet run, so the
implementation is not yet confirmed complete.

## Technology Stack (Approved)

Selected during `/architect` (`docs/architecture/ARCH-001-health-api.md`)
and recorded in `AGENTS.md`:

- **Language/Runtime:** C# on .NET 10 (LTS)
- **Framework:** ASP.NET Core, Minimal API hosting model
- **Package Manager:** NuGet via the `dotnet` CLI
- **Unit Tests:** xUnit
- **Integration Tests:** `Microsoft.AspNetCore.Mvc.Testing`
  (`WebApplicationFactory<Program>`) + xUnit
- **Formatting / Static Analysis:** `dotnet format` + built-in Roslyn
  analyzers, warnings treated as errors
- **Security / Dependency Scanning:** NuGet Audit (built-in) + GitHub
  Dependabot + `gitleaks`
- **CI/CD:** GitHub Actions (`ubuntu-latest`)
- **Database / Cloud / IaC:** Not Applicable — no persistence and no
  production deployment (see `docs/adr/ADR-002-zero-infrastructure-boundary.md`)

See `docs/architecture/ARCH-001-health-api.md` for the full technology
baseline and rationale, and `docs/adr/` for individual decisions.

## Development Workflow

The standard lifecycle is:

1. `/prd`
2. `/architect`
3. `/project-init`
4. `/spec`
5. `/implement`
6. `/verify`
7. `/review`

The complete flow is:

```text
/prd
  ↓
PRD_READY
  ↓
/architect
  ├─ ARCHITECTURE_BLOCKED → resolve as directed → /architect
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

Blocked workflow stages must report the blocker, its owner, the minimum required action, and the exact next command.

## Technology Ownership

The major technology baseline is chosen during:

`/architect`

That includes, where applicable:

- application language/runtime
- framework
- persistence technology
- cloud/provider/region
- infrastructure as code
- testing stack
- code-quality tooling
- CI/CD approach

`/project-init` does not independently choose the stack.

It synchronizes the approved architecture into:

- `AGENTS.md`
- `README.md`
- `.kilo/rules/`
- `.kilo/skills/`

If required technology decisions are missing, `/project-init` must stop instead of guessing.

## Repository Structure

```text
.
├── AGENTS.md
├── README.md
├── HealthApi.sln
├── global.json                          # pins the .NET 10 SDK
├── Directory.Build.props                # nullable + analyzers + warnings-as-errors
├── .editorconfig
├── .gitattributes                       # normalizes tracked text files to LF
├── nuget.config                         # public NuGet.org package source only
├── .github/
│   └── workflows/
│       └── ci.yml                       # restore, build, tests, format, secret scan
├── .kilo/
│   ├── rules/
│   └── skills/
├── docs/
│   ├── prd/            # PRD-001-health-api.md
│   ├── architecture/   # ARCH-001-health-api.md
│   ├── adr/            # ADR-001..004
│   ├── specs/          # SPEC-001-health-endpoint-and-ci-bootstrap.md
│   ├── reviews/
│   └── verification/
├── src/
│   └── HealthApi/
│       ├── Program.cs                   # Minimal API composition root
│       ├── HealthEndpoint.cs            # GET /health
│       ├── HealthResponse.cs            # JSON response contract
│       ├── IAppVersionProvider.cs
│       ├── AssemblyAppVersionProvider.cs
│       ├── appsettings.json
│       ├── appsettings.Development.json
│       └── Properties/launchSettings.json
└── tests/
    ├── .editorconfig                    # test-only analyzer scoping
    ├── HealthApi.UnitTests/
    │   ├── FakeAppVersionProvider.cs
    │   └── HealthEndpointTests.cs
    └── HealthApi.IntegrationTests/
        └── HealthEndpointIntegrationTests.cs
```

Project-local concurrent worktrees may use `.kilo/worktrees/`, which is intentionally ignored by Git.

## Prerequisites

- .NET 10 SDK (pinned by `global.json` to the .NET 10 feature band)

## Local Development

```
dotnet restore
dotnet build --configuration Release --no-restore
dotnet test tests/HealthApi.UnitTests --configuration Release --no-build
dotnet test tests/HealthApi.IntegrationTests --configuration Release --no-build
dotnet format --verify-no-changes
```

Run the service and verify it manually (PRD AC-10):

```
ASPNETCORE_URLS=http://localhost:5080 dotnet run --project src/HealthApi
```

In a second terminal:

```
curl http://localhost:5080/health
```

Expected response:

```json
{"status":"healthy","version":"1.0.0"}
```

`curl -i` shows `HTTP/1.1 200 OK` and `Content-Type: application/json; charset=utf-8`.
A `POST` to `/health` returns `405`, and an undefined path returns `404`.

These commands are authoritative per `AGENTS.md` — do not substitute
different tooling.

## Branch and Worktree Workflow

Each specification should be implemented on its own branch.

Preferred naming:

`spec/<spec-id>-<short-description>`

Example:

`spec/SPEC-001-create-short-url`

Do not implement directly on protected integration branches such as:

- `main`
- `master`
- `develop`
- `release`

For parallel implementation:

- use separate branches
- use separate Git worktrees
- avoid concurrent mutation of the same files

`/fix`, `/verify`, and `/review` continue on the existing implementation branch.

They do not create another branch for the same specification.

Merge should happen through the normal PR/CI process.

## Project Skills

Project-scoped skills are stored under:

`.kilo/skills/`

Skills should only be added when they materially improve implementation quality.

Preference order:

1. official/vendor-maintained skills
2. reputable community skills
3. adapted project-specific skills
4. custom skills only when necessary

Third-party skills must be reviewed and explicitly approved before installation.

Do not install large or unrelated skill collections.

## Verification and Repair

Implementation is not ready for review until `/verify` returns:

`DONE`

Normal repair flow:

```text
/implement
   ↓
RUN_VERIFY
   ↓
/verify
   │
   ├── DONE ─────────────→ /review
   │
   └── NOT_DONE
          ↓
        /fix
          ↓
       RUN_VERIFY
          ↓
        /verify
```

`/fix` is also used when review finds blocking issues:

```text
CHANGES_REQUIRED or REQUEST CHANGES
                ↓
              /fix
                ↓
             /verify
                ↓
             /review
```

`/verify` is the authoritative deterministic completion gate.

It must validate all required applicable checks and provide deterministic evidence for the specification's acceptance criteria.

## Review Model

The review pipeline is cost-controlled:

1. DeepSeek performs the first-pass pre-review.
2. Claude Sonnet runs only when pre-review returns `READY_FOR_SENIOR_REVIEW`.
3. CI remains the deterministic merge gate.
4. Production deployment still requires human approval.

The AI review results are:

- pre-review: `READY_FOR_SENIOR_REVIEW` or `CHANGES_REQUIRED`
- senior review: `APPROVE` or `REQUEST CHANGES`

## CI and Merge

CI is implemented in `.github/workflows/ci.yml` (GitHub Actions,
`ubuntu-latest`, single job — see `docs/adr/ADR-003-ci-provider.md`). On
every push and pull request it runs:

- dependency restore (with built-in NuGet Audit)
- Release build with analyzers and warnings-as-errors
- unit tests
- integration tests
- `dotnet format --verify-no-changes`
- `gitleaks detect --no-banner --exit-code 1` secret scan (full history)

For a greenfield project, an explicit specification for repository/bootstrap
work is required when CI/CD, IaC bootstrap, or project scaffolding does not
yet exist — for this project that is SPEC-001.

Do not bypass CI to merge implementation changes.

## Production Deployment

Production deployment is never implied by an AI `APPROVE` result.

The required order is:

```text
AI review APPROVE
      ↓
CI passes
      ↓
PR / merge
      ↓
explicit human production approval
      ↓
production deployment
```

## Getting Started

Project status for this repository:

1. `/prd` — done (`docs/prd/PRD-001-health-api.md`)
2. `/architect` — done (`docs/architecture/ARCH-001-health-api.md`, `docs/adr/`)
3. `/project-init` — done (`AGENTS.md`, this file)
4. `/spec` — done (`docs/specs/SPEC-001-health-endpoint-and-ci-bootstrap.md`)
5. `/implement` — done (application, tests, and CI workflow exist)
6. `/verify` — next: deterministic verification of SPEC-001
7. `/review` — after `/verify` returns `DONE`

Do not manually fill technology choices; they are approved in `AGENTS.md`.
Do not proceed to `/review` before `/verify` returns `DONE`.
