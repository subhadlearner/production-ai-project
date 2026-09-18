# Project Name

Briefly describe the project.

## Development Workflow

This repository follows the AI-assisted engineering workflow defined in `AGENTS.md`.

The standard lifecycle is:

1. `/prd`
2. `/architect`
3. `/project-init`
4. `/spec`
5. `/implement`
6. `/verify`
7. `/review`

## Final Workflow looks like this

/prd
  ↓
PRD_READY
  ↓
/architect
  ├─ ARCHITECTURE_BLOCKED → resolve → /architect
  └─ ARCHITECTURE_READY
            ↓
      /project-init
  ├─ PROJECT_INIT_BLOCKED → resolve
  └─ PROJECT_INIT_READY
            ↓
          /spec
  ├─ SPEC_BLOCKED
  └─ SPEC_READY
            ↓
       /implement
  ├─ IMPLEMENTATION_BLOCKED
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
            Sonnet review
  ├─ REQUEST CHANGES → /fix → /verify → /review
  └─ APPROVE
       ↓
       CI
       ↓
   PR / merge
       ↓
human-approved production deployment

## Technology Ownership

The technology stack is chosen during:

`/architect`

`/project-init` does not choose the stack.

It synchronizes the approved architecture into:

- `AGENTS.md`
- `README.md`
- `.kilo/rules/`
- `.kilo/skills/`

If required technology decisions are missing, `/project-init` must stop instead of guessing.

## Branch and Worktree Workflow

Each specification should be implemented on its own branch.

Preferred naming:

`spec/<spec-id>-<short-description>`

Example:

`spec/SPEC-001-create-short-url`

Do not implement directly on protected branches such as:

- `main`
- `master`
- `develop`
- `release`

For parallel implementation, use separate Git branches and separate worktrees.

`/fix`, `/verify`, and `/review` continue on the existing implementation branch.

They must not create another branch for the same specification.

Merge should happen through the normal PR/CI process.

## Project Skills

Project-scoped skills are stored under:

`.kilo/skills/`

Skills should only be added when they materially improve implementation quality.

Prefer:

1. official/vendor-maintained skills
2. reputable community skills
3. adapted project-specific skills
4. custom skills only when necessary

Third-party skills must be reviewed and explicitly approved before installation.

Do not install large or unrelated skill collections.

## Verification and Repair Loop

Implementation is not ready for review until `/verify` returns:

`DONE`

The implementation flow is:

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