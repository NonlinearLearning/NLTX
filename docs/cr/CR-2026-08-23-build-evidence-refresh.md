# CR-2026-08-23 Build Evidence Reference Refresh

## Original request

> 刷新陈旧 Build 证据引用，或对重新激活的文档族提出独立变更单。

**提出人：** 用户  
**渠道：** Codex 对话  
**提出时间：** 2026-08-23  
**Flowstate 节点：** N5 变更管控

## Suggested classification

**Level:** moderate  
**Decision:** accepted for the scoped documentation refresh  
**Reason:** This changes documentation evidence references and the manifest classifier. It does
not alter production code, runtime behavior, or the historical source baseline, but it can affect
which verification artifact a reviewer follows.

## Findings from read-only impact assessment

The current Build context manifest contains 515 references extracted from 252 documentation
files. The corrected classifier reports:

| Class | Count | Interpretation |
| --- | ---: | --- |
| `verification-evidence` | 408 | Existing concrete diagnostic/evidence paths |
| `planned-reference` | 87 | Wildcards, `<timestamp>`, `<run-id>`, or template expressions |
| `generated-output` | 11 | `Build/bin`, `obj`, `generated`, or `packages` |
| `historical-or-scratch` | 8 | Legacy or non-authoritative Build paths |
| `stale-reference` | 1 | Missing concrete path requiring replacement |
| `tooling` | 1 | `Build/Tools` path |

The single concrete stale path is:

```text
Build/diagnostics/server-ecs-convergence/P-regression/summary-after-wellfed.json
```

Referenced by:

```text
docs/plans/2026-08-22-server-ecs-convergence-remaining-work.md
```

The current directory contains a newer run under
`Build/diagnostics/server-ecs-convergence/P-regression/20260823-0100/`, including
`summary.md`, `summary.txt`, and `results.json`. These are candidates for replacement, but the
old JSON and new summary are not assumed semantically identical without comparison.

## Impact

- Production code: no
- Project files: no
- Historical documents: one plan reference may be updated after comparison
- Build evidence: one old concrete path; newer run candidates exist
- Generated output: no changes
- Link stability: improved if replacement is source-backed

## Execution batches for `fst-iterate`

| Batch | Work | Acceptance | Status |
| --- | --- | --- | --- |
| B1 | Keep classifier corrections and regenerate manifests | Wildcards are `planned-reference`; no false stale rows | completed |
| B2 | Compare old JSON intent with `20260823-0100` summaries/results | Old JSON is absent; current summary states 49/49 and results.json contains 49 zero exit codes | completed |
| B3 | Update the one plan reference only if B2 proves replacement | Plan points to concrete existing evidence and preserves historical note | completed |
| B4 | Run Flowstate docs gate and update DoD | Manifest/path checks exit 0; current stale count is 0 | completed |

## Non-goals

- Do not fabricate the missing JSON.
- Do not rename or move the 252 documentation files.
- Do not delete Build diagnostics.
- Do not rerun the entire Terraria verification suite as part of this documentation CR.
- Do not claim gameplay or migration completion from a documentation reference update.

## Handoff

Execution was limited to the accepted documentation scope: classifier correction, replacement
of one missing current-evidence link, and a fresh manifest gate. Production code, Build output,
and historical evidence were not modified. The scoped `fst-review` result is recorded in
`docs/cr/CR-2026-08-23-build-evidence-refresh-review.md`.
