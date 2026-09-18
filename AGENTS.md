# Project Instructions

## Project Overview

Describe the purpose of this project here.

## Technology

The approved technology stack is determined during `/architect`.

`/project-init` records the approved stack here.

Implementation agents must not invent or silently replace the technology stack.

- Runtime:
- Language:
- Framework:
- Database:
- Cloud:
- Region:
- Infrastructure as Code:
- Package Manager:
- Unit Test Framework:
- Integration Test Framework:
- E2E Test Framework:
- Linting:
- Formatting:
- Type Checking / Static Analysis:
- Security / Dependency Scanning:

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

The commands below must reflect the actual project configuration.

`/project-init` is responsible for populating these when the technology stack is initialized.

Do not invent tools or commands simply because they are common for the language or framework.

### Dependency Restore / Install

Define the dependency restore/install command here.

### Build

Define the project build command here.

### Unit Tests

Define the unit-test command here.

### Integration Tests

Define the integration-test command here.

### E2E Tests

Define the E2E command here when applicable.

### Lint

Define the lint command here.

### Formatting Verification

Define the formatting verification command here when configured.

### Type Checking / Static Analysis

Define the applicable command here.

### Security / Dependency Checks

Define configured security, dependency, or vulnerability checks here.

### Infrastructure Validation

Define applicable IaC validation commands here.

## Architecture Constraints

- Follow approved architecture documents under `docs/architecture/`.
- Follow approved ADRs under `docs/adr/`.
- Do not introduce new infrastructure, persistence technology, frameworks, runtimes, or major abstractions without an approved architecture decision.
- Prefer the simplest production-grade solution that satisfies the requirements.
- Do not silently redesign the system during implementation.
- Do not silently change public contracts.
- Do not silently change consistency, reliability, security, or persistence guarantees.
- Architecture decisions take precedence over implementation convenience.

## Technology Decision Authority

Technology decisions are owned by the architecture stage.

The authority chain is:

1. `/grill` optionally clarifies product intent and coupled decisions before PRD work.
2. `/prd` defines requirements and constraints.
3. `/architect` selects and approves the technology stack.
4. `/project-init` records and operationalizes those decisions.
5. `/spec` decomposes the approved design.
6. `/implement` executes the approved specification.

`/diagnose` localizes difficult defects without redefining intended behavior.

`/adversarial-check` challenges high-risk artifacts but does not replace the authority of `/prd`, `/architect`, `/spec`, `/verify`, or `/review`.

`/project-init`, `/spec`, `/implement`, `/verify`, `/fix`, and `/review` must not independently replace the approved technology stack.

If a required technology decision is missing, return to architecture rather than guessing.

## Discovery

For large, ambiguous, or high-stakes product work, use `/grill` before `/prd`.

Discovery briefs are stored under:

`docs/discovery/`

A discovery brief should preserve confirmed product decisions, scope/non-goals, constraints, assumptions, and unresolved non-blocking questions without duplicating the full PRD.

The agent should research discoverable facts itself; product decisions and trade-offs remain human decisions.

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

## Test-First and Adversarial Design

Specifications should identify stable observable test seams and explicitly state either:

- `TDD: APPLICABLE`
- `TDD: NOT_APPLICABLE` with a short reason

When TDD applies, implementation should work in thin red → green behavioral slices and avoid tests coupled to private implementation details.

High-risk design/specification decisions should receive fresh-context adversarial challenge when they involve areas such as:

- authentication/authorization or IAM
- concurrency, ordering, idempotency, or distributed consistency
- destructive migrations
- data integrity/recovery
- public API or event-contract compatibility
- financial/irreversible behavior
- security-sensitive infrastructure

Adversarial findings are evidence to reconcile, not authority. The owning lifecycle stage remains responsible for the decision.

Adversarial model routing is cost-controlled:

- default adversary: DeepSeek Flash
- agent-proposed escalation adversary: Claude Opus, only for rare critical decisions after the default DeepSeek pass
- user-directed override: the user may explicitly choose Claude Opus for an architecture, specification, or other adversarial review
- a user-directed Opus request authorizes that specific invocation directly; it does not require a prior DeepSeek pass or Sonnet justification
- when Opus is agent-proposed, explicit user approval is still required
- Opus output remains evidence; it does not replace the authority of the owning workflow stage

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
- Follow relevant global/project skills, including TDD when marked applicable.
- Keep changes incremental and reversible.
- Do not invent requirements.
- Do not silently broaden scope.
- Do not weaken tests, assertions, quality thresholds, analyzers, or security gates merely to make implementation pass.
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

## Diagnostic Workflow

Use `/diagnose` for difficult runtime, integration, concurrency, performance, or intermittent failures where the root cause is not obvious.

The diagnostic flow is:

1. define the exact symptom and expected behavior
2. build a tight red-capable feedback loop
3. reproduce and minimize
4. generate falsifiable hypotheses
5. test one variable at a time
6. establish root cause from evidence
7. define a regression-test strategy
8. hand off to `/fix`

Store non-trivial diagnostic reports under:

`docs/diagnostics/`

The normal path is:

`/diagnose → DIAGNOSIS_READY → /fix → /verify`

Do not use diagnosis to redefine product behavior or architecture.

## Repair Workflow

Use `/fix` after any of the following:

- `/diagnose` returns `DIAGNOSIS_READY`
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

## Deployment

Infrastructure and deployment must follow approved architecture and IaC decisions.

Do not manually create production cloud resources when IaC is required.

CI/CD must enforce applicable build, test, quality, security, and IaC gates.

Do not auto-deploy to production.

Production deployment requires explicit human approval.

## Workflow Ownership

The normal lifecycle is:

```text
/grill (optional)
  ↓
DISCOVERY_READY
  ↓
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

Auxiliary paths:

```text
hard bug → /diagnose → /fix → /verify
high-risk decision → /adversarial-check → owning stage continues or resolves findings
```

Blocked stages must state the owner, required action, and exact next command rather than leaving the operator to infer the recovery path.
