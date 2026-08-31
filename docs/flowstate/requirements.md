# Documentation Requirements

| ID | Requirement |
| --- | --- |
| REQ-DOC-001 | Active NLTX documentation is discoverable from `docs/flowstate/`; evidence paths remain stable. |
| REQ-DOC-002 | Each `docs/` file has one manifest row with stage, status, evidence class, owner, and canonical artifact. Unknowns are `deferred`. |
| REQ-DOC-003 | Plans/decisions are separate from Build output and diagnostics; evidence cannot silently become a plan. |
| REQ-DOC-004 | Normalization is batchable and reproducible: inventory, canonical artifacts, manifest, links, acceptance. |
| REQ-DOC-005 | Historical Markdown and source/evidence links are not moved or deleted for formatting. |

## Deferred

Per-file front matter, renaming, or physical moves require a separate `fst-change` because
they may invalidate external links and generated references.
