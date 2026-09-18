# AI Engineering Workflow

This document defines the production AI-assisted engineering workflow used by this project template.

It describes:

- the lifecycle from discovery through merge
- which command to run at each stage
- which model/agent owns each responsibility
- when TDD is required
- when to diagnose before fixing
- when to run adversarial review
- how GPT-5.6 Sol, GPT-5.6 Luna, DeepSeek, Claude Sonnet, and Claude Opus are routed
- how verification, review, CI, and human approval fit together
- how global and project-specific skills are consumed

The governing rule is simple:

> Product intent is clarified before architecture, architecture is locked before implementation, implementation is verified deterministically before review, and high-risk decisions receive adversarial challenge before they are trusted.

---

## 1. Primary Lifecycle

The default project lifecycle is:

```text
/grill (optional)
   ↓
/prd
   ↓
/architect
   ↓
/project-init
   ↓
/spec
   ↓
/implement
   ↓
/verify
   ↓
/review
   ↓
CI
   ↓
PR / merge
   ↓
human-approved production deployment
```

`/grill` is optional.

All other stages are part of the normal delivery path for a new project or major feature.

---

## 2. Mental Model

Use this shorthand:

```text
Discover → Define → Design → Initialize → Specify → Build → Verify → Review → Merge
```

Side paths are used only when needed:

```text
unclear idea          → /grill
hard-to-diagnose bug  → /diagnose
ordinary blocker      → /fix
high-risk decision    → /adversarial-check
verification failure  → /fix → /verify
review failure        → /fix → /verify → /review
```

---

## 3. User-Controlled Model Selection

For product-design workflows, the user may choose the model directly in natural language.

Examples:

```text
/grill I want to build a finance platform for Indian retail investors. Grill me. Use GPT.

/grill I want to build a finance platform for Indian retail investors. Grill me. Use Claude.

/architect Design the approved system. Use Terra.

/spec Create the next implementation specifications. Use Haiku.

/adversarial-check Use Opus directly for this review.
```

Recognized aliases:

| User phrase | Resolved model |
| --- | --- |
| `use GPT`, `use OpenAI`, `use Sol` | GPT-5.6 Sol |
| `use Terra` | GPT-5.6 Terra |
| `use Luna` | GPT-5.6 Luna |
| `use Claude`, `use Sonnet` | Claude Sonnet 5 |
| `use Haiku` | Claude Haiku 4.5 |
| `use Opus` | Claude Opus 5 |
| `use DeepSeek` | DeepSeek V4.1 Flash |

### How this works

The normal top-level planning command still starts under the configured default planner.

If the user explicitly selects another model, the planner becomes a thin router and delegates the substantive workflow to a model-selectable planning subagent using Kilo's **Task Subagent Model Selection** feature.

The selected model then performs the actual discovery, PRD, architecture, or specification work.

If the workflow needs another round of user input, the child returns the questions to the parent, which relays them to the user and continues the same workflow with the same selected model.

The selected model remains in effect for that workflow session unless the user changes it explicitly.

### Important behavior

The model choice changes the intelligence provider only.

It does **not** change:

- stage authority
- permissions
- architecture constraints
- testing rules
- acceptance criteria
- security requirements
- branch/worktree rules
- deterministic verification
- review/merge/deployment gates

If the requested model is unavailable from the connected provider, the workflow must stop clearly. It must not silently fall back to another model.

### Defaults when no model is specified

- GPT-5.6 Sol: `/grill`, `/prd`, `/architect`, `/spec`
- GPT-5.6 Luna: `/project-init`
- DeepSeek Flash: `/implement`, `/verify`, `/fix`, `/diagnose`, default adversary, pre-review
- GPT-5.6 Sol: senior review

---

## 4. Agent and Model Map

| Stage / Capability | Command | Primary Model / Agent | Supporting Agent / Skill | Purpose |
| --- | --- | --- | --- | --- |
| Product discovery | `/grill` | GPT-5.6 Sol planner | `requirements-grilling` | Resolve ambiguous requirements and trade-offs |
| PRD | `/prd` | GPT-5.6 Sol planner | discovery brief if present | Formalize product requirements |
| Architecture | `/architect` | GPT-5.6 Sol planner | DeepSeek default adversary; Sonnet/Opus optional | Lock production design and technology baseline |
| Project initialization | `/project-init` | GPT-5.6 Luna | Skill Coverage Matrix | Operationalize architecture into repo rules/configuration |
| Specification | `/spec` | GPT-5.6 Sol planner | DeepSeek default adversary; Sonnet/Opus optional; TDD applicability | Create bounded implementation specs |
| Implementation | `/implement` | DeepSeek builder | `tdd` and technology skills | Implement one approved specification |
| Verification | `/verify` | DeepSeek verification path | repository-defined checks | Produce deterministic DONE / NOT_DONE evidence |
| Normal repair | `/fix` | DeepSeek debugger | `diagnosing-bugs` when needed | Apply the smallest safe correction |
| Hard diagnosis | `/diagnose` | DeepSeek debugger | `diagnosing-bugs` | Reproduce, isolate, and establish root cause |
| Pre-review | internal to `/review` | DeepSeek pre-reviewer | review rules | Cost-efficient production review |
| Senior review | internal to `/review` | GPT-5.6 Sol code-reviewer | pre-review report | Final AI code-review decision |
| Adversarial review | `/adversarial-check` or risk-triggered inside architecture/spec | GPT-5.6 Sol orchestrator | DeepSeek adversary by default | Challenge high-risk decisions with fresh context |
| Cross-model adversarial review | user-directed or approved escalation | Claude Sonnet adversary | direct when user requests; approval when agent-proposed | Paid independent model-family second opinion |
| Premium adversarial review | user-directed or approved escalation | Claude Opus adversary | direct when user requests; approval when agent-proposed | Rare critical or deliberately premium challenge |

---

## 5. Stage-by-Stage Workflow

## 4.1 `/grill` — Optional Discovery

### When to use

Use `/grill` when:

- the idea is large
- requirements are ambiguous
- multiple decisions depend on each other
- scope is likely to drift
- important business/security/cost decisions are implicit
- the project is high-stakes enough that early assumptions are expensive

Skip it for a small feature whose behavior and boundaries are already clear.

### Model

GPT-5.6 Sol planner.

### Skill

`requirements-grilling`.

### Behavior

The agent builds a dependency-aware decision tree.

It separates:

```text
FACTS     → agent researches
DECISIONS → user decides
```

Questions are asked in frontier rounds: only questions whose prerequisites are already settled are asked together.

Each material question should include:

- the decision
- relevant context
- choices when useful
- the agent's recommended answer
- concise reasoning

### Output

A concise discovery brief under:

```text
docs/discovery/
```

Example:

```text
DISC-001-short-url-service.md
```

The discovery brief records:

- outcome
- users/actors
- success measures
- scope
- non-goals
- confirmed decisions
- binding constraints
- assumptions
- unresolved non-blocking questions
- relevant researched facts

Final status:

```text
DISCOVERY_READY
```

---

## 4.2 `/prd` — Product Requirements

### Model

GPT-5.6 Sol planner.

### Inputs

- confirmed discovery brief when present
- user-provided requirements
- existing product behavior when modifying a system
- explicit business/security/cost constraints

### Important rule

If discovery already settled a question, `/prd` should not re-ask it unless new evidence conflicts with the discovery.

### Output

PRD under:

```text
docs/prd/
```

Expected content includes:

- problem
- users and actors
- goals/non-goals
- journeys
- functional requirements
- validation/error behavior
- security/privacy
- reliability
- performance
- observability
- cost
- acceptance criteria
- assumptions/open questions

Final status:

```text
PRD_READY
```

or:

```text
PRD_BLOCKED
```

For large coupled product ambiguities, route back to `/grill`.

---

## 4.3 `/architect` — Production Architecture

### Model

GPT-5.6 Sol planner.

### Inputs

- approved PRD
- relevant discovery
- existing architecture/ADRs when applicable
- project constraints

### Responsibilities

`/architect` owns the major technology baseline.

It must decide, where applicable:

- language/runtime
- framework
- persistence technology
- cloud/provider/region
- compute/runtime service
- networking
- secrets/configuration
- infrastructure as code
- testing frameworks
- quality tooling
- security scanning
- local development model
- CI/CD approach
- deployment/rollback strategy
- observability
- recovery
- cost characteristics

It also creates architecture documentation and ADRs.

### Risk-triggered adversarial gate

High-risk architecture decisions are challenged before `ARCHITECTURE_READY`.

Examples:

- authentication/authorization
- IAM/cross-account trust
- destructive migrations
- concurrency/idempotency/ordering
- distributed consistency
- public API/event compatibility
- data integrity
- backup/recovery
- security-sensitive networking
- major irreversible platform lock-in
- material fixed-cost commitments

The default challenge is:

```text
GPT-5.6 Sol architecture
       ↓
DeepSeek adversary
       ↓
GPT-5.6 Sol reconciliation
```

If the user explicitly requests Claude Sonnet:

```text
GPT-5.6 Sol architecture
       ↓
Claude Sonnet adversary
       ↓
GPT-5.6 Sol reconciliation
```

If the user explicitly requests Claude Opus:

```text
GPT-5.6 Sol architecture
       ↓
Claude Opus adversary
       ↓
GPT-5.6 Sol reconciliation
```

No prior DeepSeek pass is required for either user-directed Claude invocation.

The adversary receives only:

- the smallest reviewable artifact
- the contract/invariants it must satisfy

It does not receive the author's preferred conclusion or reasoning narrative.

---

## 6. Model Strategy and Adversarial Escalation

## 5.1 Why the workflow changed

The workflow uses the user's connected ChatGPT Pro subscription for OpenAI models inside Kilo, while Anthropic models use a separate metered API budget.

The goal is **not** to reduce reasoning quality or starve models of context.

The goal is:

> Use subscription-covered frontier reasoning for normal high-value work, cheap execution models for high-volume work, and paid Claude calls where independent model diversity or premium scrutiny genuinely adds value.

## 5.2 Default model roles

### GPT-5.6 Sol — primary frontier reasoner

Use by default for:

- `/grill`
- `/prd`
- `/architect`
- `/spec`
- adversarial reconciliation
- senior code review

Sol is the normal model for decisions where reasoning quality matters most.

### GPT-5.6 Terra — balanced optional OpenAI choice

Terra is available in Kilo as `openai/gpt-5.6-terra`.

Use it when the user explicitly wants an OpenAI model between Sol and Luna in capability/latency/compute.

Availability under the user's ChatGPT OAuth connection depends on whether Terra is present in the connected Codex model catalog. If it is absent from the model picker, do not attempt silent fallback.

### GPT-5.6 Luna — lightweight operationalizer

Use by default for:

- `/project-init`
- lightweight Ask/documentation/repository tasks

Luna should apply already-approved decisions. It must not invent missing architecture decisions.

### DeepSeek Flash — execution workhorse

Use by default for:

- `/implement`
- `/verify`
- `/fix`
- `/diagnose`
- default adversarial review
- pre-review

DeepSeek handles high-volume engineering work economically.

### Claude Haiku 4.5 — lower-cost Claude-family option

Haiku is available in Kilo as `anthropic/claude-haiku-4.5`.

Use it for bounded planning/review work when the user wants Claude-family behavior at lower cost and the task fits its smaller context window.

Do not use Haiku for a task whose required context exceeds its supported window.

### Claude Sonnet — optional paid independent second opinion

Sonnet is no longer mandatory in the normal lifecycle.

Use it when:

- the user explicitly requests Sonnet, or
- the GPT-5.6 Sol planner proposes an independent model-family review for a material decision and the user approves the paid call

Typical uses:

- architecture challenge
- security/trust-boundary challenge
- distributed consistency/concurrency challenge
- migration/cutover challenge
- specification challenge

### Claude Opus — optional premium critical escalation

Reserve Opus for:

- user-directed premium review
- rare critical agent-proposed adversarial escalation
- rare architecture-authority escalation when Sol cannot responsibly settle the decision

Agent-proposed Opus always requires explicit user approval.

## 5.3 Adversarial routing

### Default route

```text
High-risk artifact
      ↓
DeepSeek adversary
      ↓
GPT-5.6 Sol reconciliation
      ↓
continue / revise / block
```

### User-directed Sonnet route

```text
User: "Use Sonnet for this adversarial review"
      ↓
Claude Sonnet adversary
      ↓
GPT-5.6 Sol reconciliation
```

The user's explicit request authorizes that specific paid Sonnet invocation.
No DeepSeek pass is required unless the user asks for both.

### User-directed Opus route

```text
User: "Use Opus for this adversarial review"
      ↓
Claude Opus adversary
      ↓
GPT-5.6 Sol reconciliation
```

The user's explicit request authorizes that specific premium Opus invocation.
No DeepSeek or Sonnet pass is required unless the user asks for them.

### Agent-proposed Sonnet escalation

After a DeepSeek pass, the Sol planner may propose Sonnet when material uncertainty remains and an independent model-family perspective would materially improve confidence.

Before invoking it:

1. explain what remains uncertain
2. explain why cross-model review is useful
3. ask for explicit user approval
4. invoke Sonnet only after approval

### Agent-proposed Opus escalation

Propose Opus only for rare critical situations such as:

- broad production auth/authz risk
- cross-account IAM with high blast radius
- destructive/irreversible migration
- serious data-loss/corruption risk
- non-obvious distributed consistency/idempotency guarantees
- extremely expensive-to-reverse external contracts
- recovery designs with material RTO/RPO consequences
- security-sensitive infrastructure with high production impact
- major irreversible platform lock-in or cost exposure
- unresolved disagreement after cheaper reasoning paths

Before invoking Opus:

1. explain why DeepSeek + Sol, and any already-used Sonnet review, are insufficient
2. ask for explicit user approval
3. invoke Opus only after approval

Claude findings are evidence, not authority.

The owning workflow stage retains decision authority.

## 5.4 Context-quality policy

Do **not** shrink materially relevant context merely to save money or subscription usage.

Use this rule:

> Remove irrelevant context, not required context.

For high-quality reasoning:

- include all approved artifacts materially needed for the current decision
- preserve relevant PRD requirements, discovery decisions, architecture, ADRs, invariants, specs, and repository evidence
- exclude unrelated historical conversations, obsolete documents, duplicated text, unrelated source files, and irrelevant logs
- use authoritative handoffs and targeted retrieval
- if a decision genuinely needs a large context, use it rather than guessing

The optimization target is **relevant context density**, not minimum tokens.

---

## 7. `/project-init` — Operationalize the Architecture

### Purpose

`/project-init` converts approved architecture into repository policy and setup.

It does not independently choose major technology.

### Responsibilities

It synchronizes the approved baseline into:

- `AGENTS.md`
- `README.md`
- `.kilo/rules/`
- `.kilo/skills/`

It also prepares:

- build/test command definitions
- repository conventions
- skill coverage
- initialization guidance

### Skill Coverage Matrix

Every major technology/engineering concern must receive exactly one status:

```text
ALREADY_AVAILABLE
INSTALL_RECOMMENDED
CUSTOM_SKILL_REQUIRED
NOT_REQUIRED
```

Reusable global skills should generally remain global instead of being copied into every project.

---

## 8. `/spec` — Implementation Specifications

### Model

GPT-5.6 Sol planner.

### Inputs

- PRD
- architecture
- ADRs
- discovery context when relevant
- project initialization state

### Context-size discipline

Target per specification:

```text
preferred: 30K–60K tokens
warning:   60K–80K tokens
hard max:  100K tokens
```

Anything above the hard ceiling must be split.

### Required content

A spec should define, where applicable:

- objective
- scope/non-goals
- dependencies
- technology constraints
- interfaces
- data
- behavior
- validation
- error handling
- security
- observability
- performance/reliability
- cost
- acceptance criteria
- tests
- verification
- definition of done

### Test seams

Before listing tests, identify stable observable seams such as:

- HTTP/API boundary
- message/event handler
- domain/application service
- persistence adapter
- CLI
- browser/user journey

### TDD decision

Every relevant spec should state one of:

```text
TDD: APPLICABLE
```

or:

```text
TDD: NOT_APPLICABLE
Reason: ...
```

### Spec adversarial gate

High-risk specs receive the same default adversarial pattern:

```text
GPT-5.6 Sol spec
   ↓
DeepSeek adversary
   ↓
GPT-5.6 Sol reconciliation
```

The user may directly request Claude Sonnet or Claude Opus for a specific spec adversarial review. No prior DeepSeek pass is required for a user-directed Claude invocation.

When Claude is agent-proposed rather than user-selected, explicit approval is required.

---

## 9. `/implement` — Build One Specification

### Model

DeepSeek builder.

### Responsibilities

- use the approved specification
- preserve architecture/ADRs
- stay on a dedicated spec branch
- load relevant global/project skills
- implement the smallest production-grade change
- write applicable tests
- preserve security/reliability/contracts
- avoid unrelated refactoring
- avoid inventing requirements

### TDD mode

If the spec says:

```text
TDD: APPLICABLE
```

the builder should:

```text
write one behavioral test
       ↓
run it and prove RED
       ↓
implement minimum code
       ↓
prove GREEN
       ↓
refactor while green
       ↓
next vertical slice
```

It should not write a large imagined test suite before implementation.

### Output

The builder does not claim completion.

It hands off with:

```text
RUN_VERIFY
```

---

## 10. `/verify` — Deterministic Completion Gate

### Model

DeepSeek verification path.

### Purpose

`/verify` is the authoritative deterministic completion gate.

It uses the actual project-defined checks, such as:

- build
- unit tests
- integration tests
- E2E tests
- contract tests
- negative/boundary tests
- lint
- format verification
- type/static analysis
- compiler/analyzers
- dependency/security scan
- secret scan
- IaC validation

### Statuses

Only:

```text
DONE
```

or:

```text
NOT_DONE
```

### If NOT_DONE

Use:

```text
/verify
   ↓
NOT_DONE
   ↓
/fix
   ↓
/verify
```

---

## 11. `/fix` — Normal Repair

### Model

DeepSeek debugger.

### Use for

- deterministic test failures
- compilation errors
- validation defects
- review blockers
- local implementation defects
- straightforward CI/quality issues

### Rule

Apply the smallest correct change.

Never make a gate green by weakening the gate.

Forbidden examples include:

- deleting required tests
- skipping/quarantining failing tests just to pass
- weakening assertions
- reducing coverage/performance/security thresholds
- adding warning/lint/analyzer suppressions only to silence checks
- disabling security gates
- mocking away the behavior being tested

After repair:

```text
/fix
  ↓
RUN_VERIFY
  ↓
/verify
```

---

## 12. `/diagnose` — Hard Bug Diagnosis

### Model

DeepSeek debugger.

### Use when

The issue is non-trivial, including:

- intermittent failures
- concurrency/race problems
- difficult integration defects
- serialization/DI/runtime behavior
- performance regressions
- failures where root cause is unclear
- repeated unsuccessful repair attempts

### Core principle

> No red-capable feedback loop, no confident root-cause theory.

### Flow

```text
define exact symptom
      ↓
build red-capable repro
      ↓
reproduce
      ↓
minimize
      ↓
3–5 falsifiable hypotheses
      ↓
test one variable at a time
      ↓
establish root cause from evidence
      ↓
define regression-test seam
      ↓
/fix
      ↓
/verify
```

### Diagnostic artifact

For non-trivial diagnosis:

```text
docs/diagnostics/
```

Example:

```text
DIAG-001-duplicate-dynamodb-write.md
```

It records:

- exact symptom
- repro command
- minimized case
- hypotheses tested
- root cause and evidence
- regression strategy
- next action

Final status:

```text
DIAGNOSIS_READY
```

---

## 13. `/review` — AI Review Pipeline

Users normally run only:

```text
/review
```

Do not manually run the internal reviewers unless diagnosing the workflow itself.

### Stage 1 — DeepSeek pre-review

```text
pre-reviewer
```

Possible result:

```text
READY_FOR_SENIOR_REVIEW
```

or:

```text
CHANGES_REQUIRED
```

If `CHANGES_REQUIRED`:

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

The senior reviewer is skipped to control cost.

### Stage 2 — GPT-5.6 Sol senior review

Runs only after:

```text
READY_FOR_SENIOR_REVIEW
```

Possible result:

```text
APPROVE
```

or:

```text
REQUEST CHANGES
```

If changes are requested:

```text
REQUEST CHANGES
      ↓
/fix
      ↓
/verify
      ↓
/review
```

---

## 14. CI, PR, Merge, and Deployment

AI approval is not the final deterministic gate.

Required order:

```text
/verify → DONE
       ↓
/review → APPROVE
       ↓
CI passes
       ↓
PR / merge
       ↓
explicit human production approval
       ↓
production deployment
```

Production deployment must never be implied by AI approval.

---

## 15. Branch and Worktree Rules

Each specification should use a dedicated branch:

```text
spec/<spec-id>-<short-description>
```

Examples:

```text
spec/SPEC-004-create-checkout-api
spec/SPEC-012-dynamodb-event-consumer
```

Do not implement directly on:

- `main`
- `master`
- `develop`
- `release`
- other protected integration branches

For parallel mutating specifications:

- one branch per spec
- one worktree per concurrently mutating spec
- avoid multiple agents modifying the same files concurrently

`/fix`, `/verify`, and `/review` stay on the existing implementation branch.

---

## 16. Skill Behavior

Skills are expected to be selected automatically by the configured agents.

Examples:

```text
/grill
  → requirements-grilling

/spec
  → technology skills as relevant
  → decides TDD applicability

/implement
  → tdd when applicable
  → dotnet-production / python-production / nextjs-production / etc.

/diagnose
  → diagnosing-bugs

/architect or /adversarial-check
  → adversarial-check
  → relevant cloud/database/security skill
```

Installed reusable global skills should not need manual user invocation during ordinary workflow execution.

The command/agent instructions intentionally direct agents to load relevant approved skills.

Project-specific skills under:

```text
.kilo/skills/
```

can supplement or override generic guidance for repository/domain-specific behavior.

---

## 17. Current Global Skill Foundation

Typical reusable global skills include:

- `requirements-grilling`
- `tdd`
- `diagnosing-bugs`
- `adversarial-check`
- `dotnet-production`
- `python-production`
- `postgresql-production`
- `sqlite-production`
- `frontend-design`
- `web-design-guidelines`
- `react-best-practices`
- `nextjs-production`
- `aws-serverless`
- `aws-iam`
- `amazon-dynamodb`
- `azure-architecture`

The Skill Coverage Matrix in `/project-init` decides whether additional project-specific or external skills are required.

---

## 18. Blocked-State Routing

When a stage cannot safely continue, it must identify:

- blocking issue
- owner
- why it blocks
- required action
- exact next command

Common routes:

```text
product ambiguity
→ /grill or /prd

architecture ambiguity
→ /architect

project initialization mismatch
→ /project-init

spec ambiguity
→ /spec

repository/branch/environment problem
→ resolve repository issue → rerun current command

hard defect with unknown root cause
→ /diagnose

known implementation defect
→ /fix
```

Do not silently solve an upstream authority problem in a downstream stage.

---

## 19. How to Invoke Commands — Prompt Templates

You normally invoke a workflow command and then describe the specific job in plain language.

General pattern:

```text
/<command>

What I want:
...

Relevant context:
...

Known constraints:
...

Specific concern or desired emphasis:
...
```

You do **not** need to paste PRDs, architecture documents, specs, ADRs, or source files that are already in the repository. Prefer referencing their IDs or paths so the agent can inspect the authoritative version.

You also do not need to guess the root cause of a bug or construct the adversarial contract yourself. Give the agent the observable problem or the decision you want challenged.

### 19.1 `/grill`

Use when the product idea is still broad, coupled, or ambiguous.

```text
/grill

I want to build a production-grade personal finance application for Indian retail investors.

The application should help users track goals, investments, and progress over time.

Known constraints:
- web application
- cost-conscious architecture
- security and privacy matter
- maintained by a very small team

Please research facts you can determine yourself and ask me only for product decisions, priorities, trade-offs, and constraints that genuinely need my input.
```

Short form:

```text
/grill

I want to add cross-account AWS event ingestion.
Grill me until the product behavior, scope, security boundaries, failure expectations, and non-goals are clear enough for /prd.
```

Model-selected examples:

```text
/grill I want to build a finance platform for Indian retail investors. Grill me. Use GPT.

/grill I want to build a finance platform for Indian retail investors. Grill me. Use Claude.

/grill I want to build a finance platform for Indian retail investors. Grill me. Use Terra.

/grill I want to build a finance platform for Indian retail investors. Grill me. Use Haiku.
```

### 19.2 `/prd`

Use when product intent is sufficiently clear.

```text
/prd

Create the PRD for the capability we just completed discovery for.

Use the latest approved discovery brief under docs/discovery/.
Preserve confirmed decisions and non-goals.
Do not design implementation architecture yet.
```

For a clear feature that skipped grilling:

```text
/prd

Create a production PRD for adding CSV export to the reporting module.

Requirements:
- export the currently filtered report
- preserve displayed column order
- UTF-8 CSV
- no new persistence
- maximum export size: 50,000 rows

Capture edge cases, security/privacy concerns, measurable acceptance criteria, and explicit non-goals.
```

### 19.3 `/architect`

Use after the PRD is ready.

```text
/architect

Design the production architecture for the latest approved PRD.

Priorities:
- minimize operational burden
- minimize unnecessary fixed cloud cost
- preserve security and reliability
- make all major technology choices explicit
- create ADRs for material decisions

Use the normal cost-controlled adversarial policy for high-risk decisions. GPT-5.6 Sol is the architecture model; DeepSeek is the default adversary.
```

User-directed Opus architecture review:

```text
/architect

Design the production architecture for the latest approved PRD.

For the final adversarial review of the authentication, data-integrity, and cross-account IAM decisions, use Opus directly.
My request authorizes those specific Opus adversarial checks.
```

### 19.4 `/project-init`

Use after `ARCHITECTURE_READY`.

```text
/project-init

Initialize this repository from the latest approved architecture and ADRs.

Populate the project technology baseline, build/test/quality commands, rules, and Skill Coverage Matrix.

Do not invent or replace architecture decisions.
Do not implement product functionality.
```

### 19.5 `/spec`

Use to turn approved architecture into implementable work.

```text
/spec

Create implementation specifications for the approved PRD and architecture.

Keep each specification independently implementable and verifiable.
Respect the 30K–60K preferred context range and 100K hard ceiling.
Define stable test seams and explicitly decide TDD applicability for each spec.
Identify dependencies and safe parallelism.
```

Targeted spec:

```text
/spec

Create the next implementation spec for the order-submission idempotency capability defined in the architecture and ADR-009.

Pay particular attention to concurrency, duplicate delivery, retries, observability, and deterministic acceptance criteria.
Use the normal adversarial policy because this is data-integrity sensitive.
```

User-directed Opus spec review:

```text
/spec

Create SPEC-012 for the production database migration and cutover.

Use Opus directly for the adversarial review of this specification because rollback/data integrity are critical.
My request authorizes this specific Opus review.
```

### 19.6 `/implement`

Reference the exact specification.

```text
/implement

Implement SPEC-012.

Follow the approved architecture, ADRs, project rules, and relevant skills.
Use the TDD mode defined by the specification.
Do not broaden scope or change architecture.
```

For a parallel worktree:

```text
/implement

Implement SPEC-021 in its dedicated branch/worktree.

Do not modify files owned by concurrently running SPEC-022.
Follow the dependencies and file-ownership guidance in the spec.
```

### 19.7 `/verify`

Usually only the spec reference is needed because verification reads the project-defined commands and acceptance criteria.

```text
/verify

Verify SPEC-012 completely against its acceptance criteria and the repository-defined build, test, static-analysis, formatting, security, and IaC checks.

Return DONE only with deterministic evidence.
```

If a particular environment matters:

```text
/verify

Verify SPEC-012.

The reported failure occurs only with the local integration-test profile.
Include that profile in the required deterministic checks.
```

### 19.8 `/fix`

Use when the blocker is already known from verification or review.

```text
/fix

Fix the blockers from the latest /verify result for SPEC-012.

Make the smallest correct change.
Do not weaken tests, assertions, analyzers, thresholds, security gates, or acceptance criteria.
Return control to /verify when the blockers appear resolved.
```

For a review finding:

```text
/fix

Address only the BLOCKING findings from the latest /review for SPEC-012.

Preserve the approved architecture and public contracts.
Do not implement the non-blocking suggestions unless they are required by the fix.
```

### 19.9 `/diagnose`

Use when you know the symptom but **do not know the root cause**.

You are not expected to diagnose the issue yourself.

Include whatever you know:

- observed behavior
- expected behavior
- environment
- frequency
- known reproduction steps
- sanitized error/log output
- relevant endpoint/event/spec/test/recent change

Concurrency example:

```text
/diagnose

We have a concurrency bug in the create-customer flow.

Observed:
Three POST requests arriving almost simultaneously sometimes create three different customer partition keys even though only one customer should be created.

Expected:
All concurrent requests for the same logical customer should converge on one record according to the existing specification.

Environment:
.NET API using DynamoDB.

Known clue:
Each request appears to perform a read/check first, and all three can observe "not found" before writing.

Please do not jump straight to a fix.
First build the tightest reproducible test or harness for the exact race, minimize it, generate falsifiable hypotheses, and establish the root cause.
```

Intermittent integration example:

```text
/diagnose

Our integration test for order submission fails around 1 in 20 runs.

Observed:
The API returns 202, but the expected downstream event is occasionally not visible before the test times out.

Expected:
The event should be observable within the contractually defined timeout.

I do not know whether the problem is the application, test synchronization, queue/eventual consistency, or environment.

Use the existing spec and repository to establish expected behavior.
Build a red-capable repro before proposing a fix.
```

Minimal form:

```text
/diagnose

GET /orders/{id} occasionally returns stale status for several seconds after an update.
Expected behavior is defined in SPEC-014.
Please reproduce and establish the root cause before /fix.
```

### 19.10 `/adversarial-check`

Use when you want a fresh second opinion on a high-risk decision.

Default DeepSeek adversary:

```text
/adversarial-check

Review the authentication and authorization design in the current architecture.

Focus especially on:
- privilege escalation
- tenant isolation
- token/session failure modes
- operational recovery

Use the normal adversarial path.
```

Concurrency/data-integrity example:

```text
/adversarial-check

Challenge the DynamoDB idempotency and concurrency design in ADR-007 and the related architecture section.

Try to find any sequence of concurrent requests, retries, or duplicate events that can violate the stated uniqueness/data-integrity guarantees.
```

Direct user-selected Claude Sonnet:

```text
/adversarial-check

Use Claude Sonnet directly for this review.

Challenge ADR-007 and the related architecture section for concurrency, retry, idempotency, and data-integrity failures.
I want an independent Anthropic-model second opinion.
```

Direct user-selected Opus:

```text
/adversarial-check

Use Opus directly for this review.

Review the production cross-account IAM and event-ingestion architecture in ADR-011 and the current architecture document.

I want a premium fresh-context challenge focused on trust boundaries, confused-deputy risks, privilege escalation, failure recovery, and assumptions that could create a large production blast radius.
```

The phrase `Use Opus directly for this review` is sufficient authorization for that specific premium adversarial invocation.

### 19.11 `/review`

Run only after `/verify` returns `DONE`.

```text
/review

Review the implementation of SPEC-012 using the latest successful verification result.

Run the normal cost-controlled review pipeline:
DeepSeek pre-review first, then GPT-5.6 Sol senior review only if the pre-review is clean.
```

You normally do not invoke the internal `pre-reviewer` or `code-reviewer` directly.

### 19.12 Choosing Between `/fix` and `/diagnose`

Use:

```text
/fix
```

when the blocker and cause are already reasonably clear.

Examples:

- compile error
- incorrect validation condition
- missing required test
- straightforward review finding
- lint/format failure

Use:

```text
/diagnose
```

when you have a symptom but the root cause is uncertain.

Examples:

- intermittent failure
- race condition
- stale data
- performance regression
- distributed-system timing issue
- integration failure with several plausible causes
- repeated unsuccessful fix attempts

A useful shorthand is:

```text
I know what is wrong and why → /fix

I know what is wrong but not why → /diagnose
```

---

## 20. Decision Guide — Which Command Should I Run?

| Situation | Command |
| --- | --- |
| I have a rough product idea and many unanswered questions | `/grill` |
| Requirements are clear and I need a formal PRD | `/prd` |
| PRD is ready and I need production architecture | `/architect` |
| Architecture is ready and the repo needs baseline setup | `/project-init` |
| Architecture is ready and I need implementable work units | `/spec` |
| I have an approved spec and need code | `/implement` |
| Implementation looks complete and needs deterministic proof | `/verify` |
| Verification/review found an obvious/local issue | `/fix` |
| A bug is hard, intermittent, concurrent, or unexplained | `/diagnose` |
| A decision is unusually risky and needs a fresh challenge | `/adversarial-check` |
| Verification is DONE and code needs AI production review | `/review` |

---

## 21. Full Workflow Diagram

```text
                           ┌────────────────────────────┐
                           │        Product Idea         │
                           └─────────────┬──────────────┘
                                         │
                          ambiguous / large / high-stakes?
                              ┌───────────┴───────────┐
                              │                       │
                             YES                     NO
                              │                       │
                              ▼                       │
                    ┌─────────────────┐               │
                    │ /grill          │               │
                    │ GPT-5.6 Sol planner  │               │
                    │ requirements-   │               │
                    │ grilling skill  │               │
                    └────────┬────────┘               │
                             │ DISCOVERY_READY        │
                             ▼                        │
                    docs/discovery/...                │
                             │                        │
                             └───────────┬────────────┘
                                         ▼
                               ┌─────────────────┐
                               │ /prd            │
                               │ GPT-5.6 Sol planner  │
                               └────────┬────────┘
                                        │ PRD_READY
                                        ▼
                               ┌─────────────────┐
                               │ /architect      │
                               │ GPT-5.6 Sol planner  │
                               └────────┬────────┘
                                        │
                              high-risk architecture?
                                 ┌──────┴──────┐
                                 │             │
                                YES           NO
                                 │             │
                                 ▼             │
                      ┌────────────────────┐   │
                      │ DeepSeek adversary │   │
                      └─────────┬──────────┘   │
                                │              │
                         rare critical / unresolved?
                         ┌──────┴──────┐
                         │             │
                        YES           NO
                         │             │
                         ▼             │
                 user approves Opus?   │
                    ┌────┴────┐        │
                    │         │        │
                   YES       NO        │
                    │         │        │
                    ▼         │        │
              ┌──────────────┐ │        │
              │ Opus         │ │        │
              │ adversary    │ │        │
              └──────┬───────┘ │        │
                     └──────────┴────────┘
                                │
                                ▼
                        ARCHITECTURE_READY
                                │
                                ▼
                       ┌──────────────────┐
                       │ /project-init    │
                       │ Skill matrix     │
                       └────────┬─────────┘
                                │ PROJECT_INIT_READY
                                ▼
                       ┌──────────────────┐
                       │ /spec            │
                       │ GPT-5.6 Sol planner   │
                       │ decides TDD      │
                       └────────┬─────────┘
                                │
                         high-risk spec?
                          ┌─────┴─────┐
                          │           │
                         YES         NO
                          │           │
                          ▼           │
                    DeepSeek adversary
                          │
                    Opus only if
                    rare + approved
                          │
                          └──────┬────┘
                                 ▼
                            SPEC_READY
                                 │
                                 ▼
                       ┌──────────────────┐
                       │ /implement       │
                       │ DeepSeek builder │
                       │ TDD if applicable│
                       └────────┬─────────┘
                                │ RUN_VERIFY
                                ▼
                       ┌──────────────────┐
                       │ /verify          │
                       │ deterministic    │
                       └───────┬──────────┘
                               │
                       ┌───────┴────────┐
                       │                │
                      DONE          NOT_DONE
                       │                │
                       │                ▼
                       │        ┌──────────────┐
                       │        │ /fix         │
                       │        │ DeepSeek     │
                       │        └──────┬───────┘
                       │               │
                       │               └──────→ /verify
                       ▼
                 ┌─────────────────┐
                 │ /review         │
                 └────────┬────────┘
                          ▼
              ┌────────────────────────┐
              │ DeepSeek pre-reviewer  │
              └──────────┬─────────────┘
                         │
              ┌──────────┴───────────┐
              │                      │
       CHANGES_REQUIRED    READY_FOR_SENIOR_REVIEW
              │                      │
              ▼                      ▼
            /fix            ┌────────────────────┐
              │             │ GPT-5.6 Sol reviewer    │
              │             └─────────┬──────────┘
              │                       │
              │              ┌────────┴─────────┐
              │              │                  │
              │       REQUEST CHANGES        APPROVE
              │              │                  │
              └──────────────┘                  ▼
                                             CI
                                              │
                                           PR/Merge
                                              │
                                    Human prod approval
```

---

## 22. Hard-Bug Diagram

```text
Observed defect
      │
      ▼
Root cause obvious?
  ┌───┴───┐
  │       │
 YES      NO
  │       │
  ▼       ▼
/fix   /diagnose
          │
          ▼
   exact symptom contract
          │
          ▼
   red-capable repro
          │
          ▼
      minimize
          │
          ▼
  falsifiable hypotheses
          │
          ▼
  evidence-based root cause
          │
          ▼
  regression-test strategy
          │
          ▼
         /fix
          │
          ▼
       /verify
```

---

## 23. Production Safety Summary

Before calling work complete:

- approved requirements exist
- architecture and ADRs are settled
- project-init matches architecture
- specification is bounded and testable
- implementation is on a dedicated branch
- TDD was used where applicable
- high-risk decisions received adversarial review where required
- difficult bugs were diagnosed before speculative fixing
- `/verify` returned `DONE`
- `/review` returned `APPROVE`
- CI passed
- PR/merge followed normal process
- production deployment has explicit human approval

No AI status token alone authorizes a production deployment.
