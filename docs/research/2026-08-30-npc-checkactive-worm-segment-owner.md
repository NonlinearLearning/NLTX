# NPC `CheckActive_WormSegments` typed owner boundary

## Source oracle

- Path: `D:\TRbackup\Version4物理删除了某些文件\Terraria\NPC.cs`
- Source SHA-256: `29869B0390D85CB0E627552ECBED929D90508C8EF0DA55D9660133F91DA42D17`
- Method: `CheckActive_WormSegments`, lines `64548-64570`
- Source excerpt SHA-256 (UTF-8, LF-normalized):
  `B316E3FFA9931F6641AD5D39766F9967177F04ABAA2F311D03609E84EAEA515C`

The legacy method is reached after `CheckActive` has already decided to deactivate the root. It
first requires `aiStyle == 6`, projects the trigger's `ai[0]` to an integer segment index, and
then follows the child chain while the projected index is not the trigger, is positive, and is
less than `Main.maxNPCs` (`200` in Version 1.4.5.6). A child is deactivated and synchronized only
when it is active and also has `aiStyle == 6`; any missing, inactive, non-worm, self, repeated, or
out-of-range link terminates the walk.

## Accepted typed boundary

The Simulation owner is split into:

- `NpcCheckActiveWormSegmentState`: one observed segment handle, source `AiStyle`, projected
  `NextSegment` value, and active state;
- `NpcCheckActiveWormSegmentInput`: the trigger identity/style/link, an immutable segment
  observation list, and the bounded `MaxNpcCount` contract;
- `NpcCheckActiveWormSegmentPolicy`: a pure chain query that preserves source ordering, integer
  projection/truncation, self/range/repeated-link guards, and active `aiStyle == 6` qualification;
- `NpcCheckActiveWormSegmentDecision`: typed `DespawnNpcCommand` output.

Each accepted child produces `DespawnNpcCommand(child, NpcDespawnReason.SegmentRootRemoved)`. This
reason represents a child removed because its deactivated worm root no longer owns the chain; the
policy does not mutate the observation list, `NpcLifecycleComponent`, or an Arch world.

The policy rejects invalid handles, negative AI styles, non-finite trigger links, duplicate
observations, and values outside the bounded `1..199` NPC slot domain. A non-finite child link is
treated as a terminal link after the already-qualified child is emitted, matching the source's
deactivate-then-follow ordering while keeping the typed boundary fail-closed.

## Verification

TDD RED was observed with the standard NPC verifier build exiting `1` and reporting 16 missing
owner/API symbols in the new verifier block:

`Build/diagnostics/npc-complete/task-10-interaction/20260830-checkactive-worm-segments-red/verifier-build-red.log`

The focused verifier then built with zero warnings/errors and ran with exit `0`. The current
verifier output contains `PASS: NPC CheckActive worm-segment follow-up policy` together with the
existing NPC checks:

`Build/diagnostics/npc-complete/task-10-interaction/20260830-checkactive-worm-segments-green/`

The focused cases cover non-worm trigger rejection, source float-to-integer truncation, first
inactive/non-worm child termination, self-link termination, repeated-link cycle protection,
caller-input immutability, invalid identity/style/link rejection, and the bounded command reason.

## Deferred scope

This owner does not claim complete worm construction, shared-life semantics, segment movement,
`NPC.NewNPC` slot allocation, `Main.npc` array projection, message-23 encoding/cadence, loot,
revenge caching, network/client presentation, or full NPC/AI parity. Existing
`NpcSegmentLifecycleSystem.GetWormFollowUpDespawns` remains the separate typed death-follow-up
owner and keeps its explicit caller-supplied worm classification and `Killed` reason.

The overall NPC field/property migration remains **partial**. `canRemoveLegacyWorldGen` remains
`false`, the 44 deferred `ServerRelevant` rows remain deferred, and no legacy source deletion is
authorized by this slice.
