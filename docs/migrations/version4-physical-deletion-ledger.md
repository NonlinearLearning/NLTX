# Version4 physical deletion ledger

Generated from read-only trees:
- Version3: `D:\TRbackup\Version3删除多余同时人工审查代码` (1515 C# files)
- Version4: `D:\TRbackup\Version4物理删除了某些文件` (980 C# files)
- Removed files: **535**

The CSV is a mechanical path/hash/declaration/reference inventory. Classification is intentionally conservative: UI/graphics/social/test namespaces are `ClientOnly`; ID/data namespaces are `SharedDefinition`; generation/world/physics/network namespaces are `ServerRelevant`; all 535 rows have a classification and a reason. Three rows have accepted replacement evidence; the remaining server-relevant rows remain `deferred`.

| Classification | Count |
|---|---:|
| ClientOnly | 416 |
| ServerRelevant | 57 |
| SharedDefinition | 59 |
| RecoveredFromCompleteOracle | 0 |
| ReplacedWithEvidence | 3 |
| Unknown | 0 |

Physical deletion safety claim: **false**. The completeness gate requires a legacy anchor, current owner, state shape, command/commit, persistence/protocol consequence, verifier, and status for every server-relevant row.
