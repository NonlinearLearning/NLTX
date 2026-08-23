# Iteration Scope: Documentation Flowstate Normalization

**Scope ID:** REQ-DOC-001..005
**Strategy:** `spec`
**Status:** in_progress
**Owner:** NLTX repository maintainers

## Included

- Initialize the private `.agent-workplace/` layout and ignore boundary.
- Establish `docs/flowstate/` as the canonical documentation process context.
- Classify all current files under `docs/` in `document-manifest.csv`.
- Classify Build paths into evidence, generated output, tools, and historical scratch.
- Add phase, batch, risk, DoD, and retrospective artifacts for this normalization iteration.
- Verify manifest coverage, path existence, lifecycle values, and links to Build evidence.

## Excluded

- Rewriting the contents of all historical research and evidence documents.
- Moving or deleting files under `docs/` or `Build/`.
- Treating compiled binaries, packages, generated source, or diagnostic logs as plans.
- Claiming that documentation normalization changes the Terraria migration completion status.

## Completion boundary

This iteration is complete only when every current `docs/` file has a manifest entry, all
canonical Flowstate artifacts exist, the Build classification is explicit, and the DoD checks
pass. It does not imply gameplay or WorldGen parity.
