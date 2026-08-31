# Documentation Normalization Tasks

| Batch | Task | Acceptance | Status |
| --- | --- | --- | --- |
| B1 | Establish private workplace and ignore boundary | State is private and ignored | completed |
| B2 | Create canonical Flowstate artifacts | Required files exist and cross-link | completed |
| B3 | Generate document manifest | Count equals current `docs/` files | completed |
| B4 | Classify Build references | Evidence/output/tool/stale classes explicit | completed |
| B5 | Run documentation gate | Exit 0; stale refs remain visible | completed |
| B6 | Record retrospective/backlog | Findings and next work are explicit | completed |

Batches are independently reproducible; do not promote a failed or stale batch silently.
