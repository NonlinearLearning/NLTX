# Requirements Layer

## REQ-DOC-001: Single process context

All active NLTX documentation work must be discoverable from `docs/flowstate/` and its
manifest. Existing evidence paths remain stable.

## REQ-DOC-002: Explicit lifecycle state

Every document must have a manifest entry with a Flowstate stage, lifecycle status, evidence
class, owner area, and canonical artifact. Unknown values are recorded as `deferred`, never
guessed.

## REQ-DOC-003: Evidence separation

Build output and diagnostics must be classified separately from plans and decisions. A build
artifact can prove a verification claim but cannot silently become a requirement or plan.

## REQ-DOC-004: Batchable normalization

Normalization must be performed in small batches: inventory, canonical outputs, manifest,
cross-link checks, then acceptance. Each batch has a reproducible check.

## REQ-DOC-005: Historical preservation

The 223 existing Markdown documents and their source/evidence links must not be physically
moved or deleted solely for formatting consistency.

## Deferred requirements

Whether historical documents should receive individual YAML front matter, be renamed, or be
physically moved is intentionally deferred. Such a change requires a separate `fst-change`
record because it can invalidate external links and generated references.
