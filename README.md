# Project Name

Briefly describe the project.

## Purpose

This repository is a reusable production-grade AI-assisted software-engineering template.

The project-specific technology stack is selected during `/architect` and synchronized into the repository during `/project-init`.

## Development Workflow

The standard lifecycle is:

1. `/grill` when discovery is needed (optional)
2. `/prd`
3. `/architect`
4. `/project-init`
5. `/spec`
6. `/implement`
7. `/verify`
8. `/review`

For difficult defects use `/diagnose → /fix → /verify`.

For high-risk decisions use `/adversarial-check` or the automatic risk-triggered checks inside architecture/specification work.

The complete flow is:

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

For the complete command, agent, model-routing, TDD, diagnosis, adversarial-escalation, skill, blocked-state, and **sample invocation prompt** reference, see:

`docs/workflow/AI-ENGINEERING-WORKFLOW.md`

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
├── .gitignore
├── .kilo/
│   ├── rules/
│   └── skills/
└── docs/
    ├── discovery/
    ├── diagnostics/
    ├── prd/
    ├── architecture/
    ├── adr/
    ├── specs/
    ├── reviews/
    └── verification/
```

Project-local concurrent worktrees may use `.kilo/worktrees/`, which is intentionally ignored by Git.

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

## Discovery, TDD, Diagnosis, and Adversarial Checks

Use `/grill` before PRD work when an idea is large, ambiguous, or contains coupled decisions. Confirmed discovery is written under `docs/discovery/` and becomes input to `/prd`.

Specifications define stable observable test seams and whether TDD is applicable. When applicable, `/implement` uses behavioral red → green vertical slices rather than writing tests only after implementation.

Use `/diagnose` when a defect is difficult to reproduce or localize. It creates a red-capable feedback loop before root-cause claims and may persist a concise report under `docs/diagnostics/`.

Use `/adversarial-check` for high-risk architecture/specification/migration/security decisions. It sends only the artifact plus its contract to a fresh-context adversary and then reconciles findings. DeepSeek Flash is the default adversary. You may explicitly request Claude Opus directly for a specific adversarial review; that user request authorizes the premium invocation without a prior DeepSeek pass or Sonnet justification. It is not a substitute for `/verify`, `/review`, CI, or human approval.

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

For a hard defect whose cause is not established:

```text
/diagnose
   ↓
DIAGNOSIS_READY
   ↓
/fix
   ↓
/verify
```

For implementation verification:

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

The approved architecture defines the CI/CD approach.

A greenfield project should include an explicit specification for repository/bootstrap work when CI/CD, IaC bootstrap, or project scaffolding does not yet exist.

CI should enforce the applicable:

- build
- unit/integration/E2E tests
- lint/format/static analysis
- security/dependency checks
- IaC validation

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

For a new project:

1. clone or copy this template
2. replace the project name and description
3. run `/grill` if the product idea is still broad/ambiguous; otherwise start at `/prd`
4. proceed through the workflow in order
5. let `/project-init` populate the actual technology and build/test commands after architecture is approved

Do not manually fill technology choices that have not yet been approved by the architecture stage.
