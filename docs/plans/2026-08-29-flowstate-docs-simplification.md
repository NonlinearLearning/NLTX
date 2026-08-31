# Flowstate Documentation Simplification Implementation Plan

**Goal:** Reduce duplicate Flowstate context while preserving document paths, historical evidence, and verifier contracts.

**Approach:** Keep `docs/flowstate/` as the compact authority layer; retain task/plan files as short execution records; regenerate manifests instead of hand-editing inventory rows. Historical documents and diagnostic artifacts remain in place.

**Verification:** Run `Build/Tools/VerifyFlowstateDocs.ps1`, then inspect the diff for removed claims, broken paths, and accidental changes outside Flowstate-owned documentation.

### Task 1: Compact the authority documents

Modify `docs/flowstate/README.md`, `requirements.md`, `scope.md`, `risks.md`, `dod-checklist.md`, `tech-debt.md`, `retrospective.md`, and `manifest-summary.md` to remove repeated prose while retaining authority, status, boundaries, evidence, and deferred items.

### Task 2: Compact execution records

Modify the two Flowstate plan files and two task files to retain only objective, dependency/order, acceptance evidence, status, and current blockers.

### Task 3: Refresh generated inventories

Run `Build/Tools/GenerateFlowstateManifest.ps1` and retain the generated CSV schema and coverage required by the verifier.

### Task 4: Verify and review

Run the Flowstate verifier and `git diff --check`; review changed paths and report any stale references without deleting historical evidence.
