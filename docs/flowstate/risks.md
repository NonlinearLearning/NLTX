# Documentation Risks

| ID | Risk | Impact | Mitigation | Status |
| --- | --- | --- | --- | --- |
| RISK-DOC-001 | Physical moves break source/evidence links | High | Preserve historical paths; central manifest first | mitigated |
| RISK-DOC-002 | Old plans are mistaken for current authority | High | Mark lifecycle in manifest and point to canonical artifacts | active |
| RISK-DOC-003 | Build output is treated as durable documentation | High | Separate evidence/generated/tool classes | mitigated |
| RISK-DOC-004 | Stale diagnostic paths are cited as fresh proof | High | Require existence checks and verification timestamp | active |
| RISK-DOC-005 | Bulk front matter creates noisy unrelated diffs | Medium | Defer per-file rewrites; use a generated manifest | mitigated |
| RISK-DOC-006 | User/parallel worktree changes are overwritten | High | Read-only inventory first; edit only flowstate-owned files | active |

Risks marked `active` remain visible in the next retrospective and are not silently closed.
