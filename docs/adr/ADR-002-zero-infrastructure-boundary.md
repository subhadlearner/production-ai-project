# ADR-002: Zero-Infrastructure, No-Production-Deployment Boundary

## Status

Accepted

## Context

PRD-001 explicitly excludes production deployment and cloud hosting
(non-goal NG6) and mandates zero/near-zero infrastructure cost (Cost
Constraints, Goal G3). The purpose of the project is to validate the
engineering workflow itself, not to operate a real service. Absent this
explicit boundary, an implementer could be tempted to add a Dockerfile,
a cloud deployment target, or IaC "for completeness," which would
contradict the PRD's non-goals and introduce unjustified cost and
operational burden.

## Decision

This architecture provisions **no cloud infrastructure of any kind**. The
application:

- runs only as a local process (`dotnet run`) or inside CI runner compute;
- is never packaged as a container image or deployment artifact;
- has no IaC, no cloud provider, no region, and no environment beyond a
  single implicit local/CI environment;
- has no production deployment path defined in this project.

Any future need for real deployment requires a **new PRD revision and a
new architecture pass**, followed by the standard human-approved
production deployment gate. This ADR does not pre-authorize any such
extension.

## Alternatives Considered

| Option | Assessment | Verdict |
| --- | --- | --- |
| No cloud infrastructure, local/CI only | Meets PRD non-goal NG6 and zero-cost goal exactly | **Selected** |
| Containerize and deploy to a minimal serverless target (e.g., a managed container/serverless compute service) for realism | Would exercise more of the workflow (deployment, IaC) but directly contradicts explicit PRD non-goal NG6 and introduces cost/operational surface the PRD forbids | Rejected — out of scope per PRD, not an architecture trade-off to make unilaterally |

## Consequences

- IaC, cloud runtime, region, and deployment strategy are all
  legitimately "Not Applicable" in the Technology Baseline — this is a
  deliberate decision, not a gap.
- If the smoke test is later extended to also validate a deployment
  pipeline (see PRD OQ-2), that is new product scope requiring its own
  PRD and architecture revision, not a silent addition during
  implementation.
- CI cost is bounded to GitHub Actions compute minutes only; no recurring
  cloud bill exists.

## Related Decisions

- ADR-003 (CI provider) — the only compute this system uses besides the
  developer's own machine.
