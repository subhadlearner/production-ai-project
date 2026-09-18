\# Project Instructions



\## Project Overview



Describe the purpose of this project here.



\## Technology



\- Runtime:

\- Language:

\- Framework:

\- Database:

\- Cloud:

\- Region:

\- Infrastructure as Code:



\## Build and Verification Commands



\### Build



Define the project build command here.



\### Unit Tests



Define the unit-test command here.



\### Integration Tests



Define the integration-test command here.



\### E2E Tests



Define the E2E command here when applicable.



\### Lint



Define the lint command here.



\### Type Checking / Static Analysis



Define the applicable command here.



\## Architecture Constraints



\- Follow approved architecture documents under `docs/architecture/`.

\- Follow approved ADRs under `docs/adr/`.

\- Do not introduce new infrastructure, persistence technology, frameworks, or major abstractions without an approved architecture decision.

\- Prefer the simplest production-grade solution that satisfies the requirements.

\- Do not silently redesign the system during implementation.



\## Specification Discipline



Implementation must be based on a specification under `docs/specs/`.



Specifications should define, where applicable:



\- objective

\- scope

\- non-goals

\- dependencies

\- interfaces

\- data model

\- behavior

\- validation

\- error handling

\- security

\- observability

\- performance

\- cost implications

\- acceptance criteria

\- testing requirements



Target specification size:



\- preferred: 30K–60K tokens

\- warning: 60K–80K tokens

\- hard ceiling: 100K tokens



Split larger work into independently implementable specifications.



\## Implementation Rules



\- Implement only the approved specification.

\- Reuse existing project conventions.

\- Keep changes incremental and reversible.

\- Do not invent requirements.

\- Do not weaken tests merely to make them pass.

\- Do not remove validations or error handling without explicit justification.

\- Avoid unnecessary abstractions and dependencies.

\- Prefer straightforward code over speculative extensibility.



\## Testing Requirements



Use the appropriate combination of:



\- unit tests

\- integration tests

\- contract tests

\- E2E tests

\- negative tests

\- boundary tests

\- security tests



A specification is not complete until all required applicable tests pass.



\## Definition of Done



Applicable checks must pass:



\- implementation complete

\- build successful

\- unit tests pass

\- integration tests pass

\- E2E tests pass when required

\- lint passes

\- type checking/static analysis passes

\- security checks pass

\- specification acceptance criteria satisfied

\- pre-review passes

\- senior review approves

\- CI passes



\## Security



\- Never hard-code credentials.

\- Never log secrets or sensitive tokens.

\- Never bypass authentication or authorization checks.

\- Never disable security checks merely to achieve a passing build.

\- Do not commit `.env`, credentials, private keys, tokens, or cloud credentials.



\## Cloud and Cost Constraints



Cloud cost is a first-class architectural concern.



For AWS or Azure changes evaluate:



\- fixed monthly cost

\- usage-based cost

\- storage

\- network transfer

\- observability/logging

\- scaling behavior

\- operational burden

\- failure and recovery behavior



Challenge unnecessary use of:



\- Kubernetes

\- NAT gateways

\- always-on compute

\- managed search clusters

\- oversized databases

\- premium load balancers

\- provisioned capacity

\- excessive logging

\- cross-region traffic



Prefer managed/serverless approaches when they reduce cost and operational complexity without compromising requirements.



\## Deployment



\- Do not deploy directly to production without explicit human approval.

\- Infrastructure changes must be defined using approved IaC.

\- Production deployment remains a human-controlled action.



\## Review Workflow



The required review flow is:



1\. deterministic verification

2\. DeepSeek `pre-reviewer`

3\. Claude Sonnet `code-reviewer`

4\. CI

5\. human production approval



The pre-reviewer is advisory.



The senior reviewer provides the authoritative AI review decision.



CI remains the deterministic merge gate.

