# Terraria NPC AI reference differential

This verifier runs selected Eye of Cthulhu, Blue Slime, and Mother Slime scenarios against both the
read-only x86 `TerrariaServer.exe` and a fresh build of a copied reference source tree. It compares
`Terraria.NPC.AI()` and `NPC.AI_001_Slimes` IL plus sampled reference outputs, then runs the matching
production profiles linked into this net10 verifier with the same initial state and captured random
inputs.

From the repository root, run:

```powershell
pwsh -NoLogo -NoProfile -File Test/Terraria.NpcAi.ReferenceVerification/Invoke-ReferenceDifferential.ps1
```

The script accepts `-ReferenceRoot` when the pinned read-only source tree is stored elsewhere.
It copies source and assets into `Build/NpcAiReferenceVerification/`, redirects stale dependency
hint paths only inside that copy, restores and builds the copy, then places logs, captured inputs,
outputs, hashes and the differential report under `Build/diagnostics/NpcAiReferenceVerification/`.

The x86 harness loads `NPC.AI()` directly. It does not call the server entry point. Before any
reference method can construct `CallTracker`, the harness disables its static five-second flush
timer; selected seeds avoid the `Dust.NewDust` branch. The scenario starts in `ai[1]=2`,
`ai[2]=39`, so the first AI invocation executes dash damping without entering servant spawning.

The report distinguishes reference binary/source association, source-build results, profile
differences, and source outputs that the profiles do not model. The source `CallTracker` stores only
the deduplicated first-entry order of method names; it cannot establish repeated call counts or full
effect ordering. The comparison uses it only to check whether `TargetClosest` appeared, and compares
the captured final `netUpdate` value separately. A successful capture does not claim complete NPC AI
behavior parity.
