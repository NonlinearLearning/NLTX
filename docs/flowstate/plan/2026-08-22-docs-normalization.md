# Documentation Normalization Plan

**Strategy:** `spec`
**Iteration:** Documentation Flowstate normalization

## Phase 1: Inventory and boundary

Create the private workplace, count and classify `docs/` and `Build/`, and freeze the rule
that historical paths are preserved. Acceptance: inventory counts are reproducible.

## Phase 2: Canonical Flowstate artifacts

Create requirements, scope, risks, plan, task, DoD, technical-debt, and retrospective outputs.
Acceptance: each artifact links to the next lifecycle stage and has no unresolved authority
ambiguity.

## Phase 3: Manifest and evidence mapping

Generate one manifest row for every current `docs/` file and explicit rows for the Build
artifact classes. Acceptance: all referenced paths exist and every lifecycle value is allowed.

## Phase 4: Gate and handoff

Run manifest coverage, path, link, and schema checks; record results in the DoD and close with
a retrospective. Acceptance: no missing docs entry, no generated-output path classified as a
plan, and all remaining concerns are recorded as technical debt or deferred scope.
