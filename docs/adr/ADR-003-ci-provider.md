# ADR-003: CI Provider Selection

## Status

Accepted

## Context

PRD-001 requires a CI pipeline that gates merges on build, tests, and
code-quality checks (Goal G5, AC-9), with zero/near-zero infrastructure
cost. The PRD does not specify a CI provider or a git hosting platform;
this repository is not yet connected to a remote. CI/CD approach is
explicitly listed as an architecture-owned decision (project `AGENTS.md`,
Technology Decision Authority), so it must be decided here rather than
deferred to `/project-init` or `/implement`.

## Decision

Use **GitHub Actions** as the CI provider, running on the
`ubuntu-latest` GitHub-hosted runner, with `actions/setup-dotnet` pinning
the .NET 10 SDK.

**Assumption (AA-1):** the repository is hosted on GitHub, or will be
before CI is enabled. This is a low-risk, easily reversible assumption:
if the repository ends up hosted elsewhere, only the CI workflow file and
this ADR need to change — no application code, tests, or other
architecture decisions are affected.

## Alternatives Considered

| Option | Fixed cost | Fit for this project | Verdict |
| --- | --- | --- | --- |
| GitHub Actions | Free tier covers this pipeline's expected minute usage; no separate account/service needed | Native to GitHub-based PR/merge workflow already implied by project conventions; excellent .NET support via `actions/setup-dotnet` | **Selected** |
| Azure DevOps Pipelines | Free tier available | Would introduce a second vendor surface (Azure) with no other Azure usage in this project (no cloud deployment exists per ADR-002); no material benefit over GitHub Actions here | Rejected — unnecessary vendor surface for no added benefit |
| GitLab CI | Free tier available | Would require moving/also hosting the repository on GitLab absent any stated hosting preference | Rejected — no justification to introduce a second hosting platform |

## Consequences

- CI workflow lives at `.github/workflows/ci.yml`.
- Runner OS is Linux only (`ubuntu-latest`); no Windows/macOS runner is
  used, since .NET is cross-platform and the PRD does not require
  OS-specific behavior — this also minimizes CI minute cost.
- NuGet package caching should be configured in the workflow to keep run
  time (and therefore minute cost) low.
- If the repository is not GitHub-hosted when `/project-init` runs, this
  ADR must be revisited before CI can be operationalized; this is called
  out explicitly in the architecture document (Stage 11) so it is not
  silently missed.

## Related Decisions

- ADR-002 (zero-infrastructure boundary) — CI compute is the only compute
  this system consumes.
