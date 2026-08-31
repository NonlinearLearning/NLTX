# NPC type-690 inactivity guard boundary (2026-08-30)

## Audit result

The next `NPC.CheckActive` branch is the special guard
`type == 690 && ai[0] == 0f` at `NPC.cs:64442`. It remains `deferred`; no new owner is added.

## Source and runtime evidence

- `D:\TRbackup\Version4物理删除了某些文件\Terraria\NPC.cs:17388-17403` initializes type `690`
  with `aiStyle = 126`, `immortal = true`, and `dontTakeDamage = true`.
- `NPC.cs:64442` checks the type and the mutable `ai[0]` value before the player-range and
  timer-decrement branches. This is not a static type-only predicate.
- The current `NpcLifecycleSystem` already gives `isImmortal` priority over health and timer
  processing, covering the broad no-decrement effect for an explicitly authoritative immortal
  definition.
- The current Simulation definition registry contains no complete type-690 definition or AI-126
  behavior, and `NpcBehaviorStateComponent` has no generic `ai[0]` owner. Adding one would require
  an AI-family/state contract plus definition, persistence, and protocol decisions.
- `LegacyNpcTownRegistry` explicitly excludes type `690`; the type is not `townNPC` and must not be
  inferred from its `isLikeATownNPC` or immortal behavior.

## Boundary decision

Do not add a type-690 boolean or generic AI slot to `NpcLifecycleComponent`, `NpcDefinition`,
snapshots, protocol, or persistence. A future batch may revisit this only after a source-backed
AI-126 owner and its runtime state are established. Full `CheckActive`, player rectangles,
`Main.player` state, type-specific AI, collision/recovery, and legacy source deletion remain
`partial/deferred`; `canRemoveLegacyWorldGen` remains `false`.

## Evidence

`Build/diagnostics/npc-complete/task-10-interaction/20260830-type690-inactivity-audit/type690-audit.log`
records the source anchors and current-owner checks. This is a deferred audit, not a parity claim.
