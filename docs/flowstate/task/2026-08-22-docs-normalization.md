# Documentation Normalization Tasks

| Batch | Task | Acceptance | Status |
| --- | --- | --- | --- |
| B1 | Initialize `.agent-workplace/` and Git ignore boundary | Private state exists; Git does not report it | completed |
| B2 | Add canonical Flowstate artifacts | Requirements, scope, risks and plan exist and cross-link | completed |
| B3 | Generate `document-manifest.csv` for all docs files | Manifest count equals current docs file count | completed |
| B4 | Classify Build evidence and generated paths | Evidence/tool/output/stale classes are explicit | completed |
| B5 | Run documentation gate and write DoD | Gate exits 0; 15 stale references are recorded, not hidden | completed |
| B6 | Write retrospective and next backlog | Results, debt and next iteration are recorded | completed |

Each batch is independently verifiable. A failed batch must be repaired before the next batch
is marked complete.
