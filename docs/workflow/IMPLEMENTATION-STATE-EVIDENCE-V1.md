# Implementation-State Evidence Contract v1

This is the Stable v1 repository-content freshness contract for `/verify`, `/review`, and `/waive`.

## Authority

The **canonical implementation-state manifest** is authoritative.

The **implementation-state fingerprint** is only a compact checksum/identifier of that manifest. A matching fingerprint alone is never sufficient when the canonical manifest is available.

## Verification Base

Every reusable verification report records:

- current branch
- verification base HEAD SHA captured before checks
- canonical pre-check implementation-state manifest
- implementation-state fingerprint
- pre-check/post-check freshness outcome

The base HEAD is provenance and the reconstruction anchor. Commit identity alone is not implementation-state identity.

## Included Paths

Build the manifest from the union of:

1. tracked paths whose effective current contents differ from the recorded verification base HEAD
2. untracked, non-ignored paths

A deleted tracked path is included with state `DELETED`.

Git-ignored paths are intentionally outside implementation-state identity.

## Workflow Evidence Exclusion Set

Exclude **only**:

- `docs/verification/**`
- `docs/reviews/**`
- `docs/diagnostics/**`

This set is normative for Contract v1. No workflow stage may independently add exclusions.

Identity-bearing paths therefore include changed specifications, architecture/ADRs, `AGENTS.md`, `.kilo/**`, source/tests, build/package configuration, dependency manifests/lockfiles, CI, and IaC configuration.

## Per-Path State

For each included path record:

- repository-relative path using `/`
- current Git blob/content hash produced with read-only `git hash-object --no-filters`, or literal `DELETED`

Do not use timestamps, file size, mtime, branch name, or commit SHA as per-path identity.

## Canonical Manifest Serialization

Serialize every entry exactly as:

`<repository-relative-path><TAB><git-blob-hash-or-DELETED><LF>`

Rules:

- repository-relative paths
- `/` path separator
- preserve case
- TAB (U+0009) field separator
- LF (U+000A) record terminator
- no CR
- UTF-8, no BOM
- no blank records
- no duplicate paths
- sort ascending by the UTF-8 byte sequence of the repository-relative path
- an empty manifest is valid and represented by zero bytes

A participating path containing TAB, CR, or LF is not canonically representable in Contract v1. Freshness is then `UNRECONSTRUCTABLE`.

## Fingerprint

The implementation-state fingerprint is the Git blob object ID of the exact canonical manifest bytes and is persisted as:

`GIT_BLOB_OID:<object-id>`

It may be computed with `git hash-object --stdin` when the execution environment can supply the bytes directly, or from an exact manifest sidecar under the excluded `docs/verification/**` evidence path using `git hash-object --no-filters`.

The repository's Git object format determines the object-ID algorithm.

The full canonical manifest must always be persisted. Exact manifest equality, not fingerprint equality, is the authoritative freshness test.

## Freshness Outcomes

Freshness has exactly three outcomes.

### MATCH

`MATCH` requires:

- specification/change scope matches
- branch matches
- verification base HEAD is available
- current canonical manifest can be reconstructed
- current and persisted canonical manifests are byte-for-byte identical

A later commit of the exact verified contents does not invalidate `MATCH`.

### MISMATCH

`MISMATCH` means reconstruction succeeded but canonical manifests differ.

Verification evidence is stale.

### UNRECONSTRUCTABLE

`UNRECONSTRUCTABLE` applies when freshness cannot be proven reliably, including:

- base HEAD unavailable
- required Git inspection failed
- required report fields missing or malformed
- malformed/duplicate manifest entries
- identity-bearing path contains TAB, CR, or LF
- repository state cannot be determined reliably

`MISMATCH` and `UNRECONSTRUCTABLE` both fail closed and require a fresh `/verify` before review or waiver use.

## Required Verification Evidence Schema

Reusable verification evidence contains:

- verification ID
- specification/change identity
- branch
- verification base HEAD SHA
- Contract version: `implementation-state-evidence-v1`
- canonical implementation-state manifest
- implementation-state fingerprint
- pre/post freshness outcome
- factual verification result: `DONE` or `NOT_DONE`
- effective delivery gate
- commands/checks and outcomes
- materially relevant verification-environment facts when applicable

Missing required fields make freshness `UNRECONSTRUCTABLE`.

## Verification Environment Boundary

Repository-content freshness does **not** prove execution-environment identity.

Git-ignored files, credentials, mutable databases, external services, OS/runtime/SDK/tool versions, and environment variables may affect verification while remaining outside the manifest.

Record only materially relevant environment assumptions/evidence, without secrets or full environment dumps.

## Waiver Binding

A waiver is logically bound to:

- exact immutable verification report
- its canonical implementation-state manifest
- implementation-state fingerprint
- exact accepted failure set
- expiry
- human-authorized justification and residual-risk acceptance

The waiver may reference the immutable verification report instead of duplicating the full manifest, but current freshness must be `MATCH`.

A later commit of identical contents does not invalidate the waiver. `MISMATCH` or `UNRECONSTRUCTABLE` does.

## Fail-Closed Rule

When evidence is missing, malformed, ambiguous, or cannot be reconstructed reliably:

- do not infer freshness from HEAD or fingerprint alone
- do not substitute a merely newer report
- do not continue review or waiver use
- require a fresh `/verify`

This contract controls implementation-state freshness for Stable v1.
