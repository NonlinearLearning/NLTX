# NPC AI Guide source profile checkpoint — 2026-10-07

## Scope

This checkpoint records the first Guide source slice for `type=22 / netID=22 /
aiStyle=7`. It covers the town return pressure, the server-side home-return
gate, the source home destination candidate order, and the observable effect
order for a successful or failed home return.

It does not claim a complete `AI_007_TownEntities` migration. Path prediction,
danger avoidance, conversation, the full resting-spot search, and the complete
sitting helper remain open slices.

## Read-only source fingerprint

- Source: `D:/TRbackup/无任何删减通过编译/Terraria/NPC.cs`
- SHA-256: `ED8AA2302730A9E046EF310E543204FBA391BD35B1C9C0B213C8065DFBBE39F0`
- Identity evidence: `NPC.cs:9163-9169` sets Guide `aiStyle=7`; `NPC.cs:20996-20998` dispatches `aiStyle == 7` to `AI_007_TownEntities`.
- Weather pressure: `NPC.cs:53759-53775`.
- Player and return gate: `NPC.cs:54204-54230`.
- Resting-spot predicate and search: `NPC.cs:53525-53628`.
- Ordinary state branches: `NPC.cs:54406-54623`, including `ai[0] == 0` and `ai[0] == 1`.
- Sitting helper: `NPC.cs:53630-53668`.
- Home teleport helper: `NPC.cs:56443-56472`.
- Extracted evidence files are under `Build/diagnostics/NpcAiRedesign/guide-source-profile-20261007/`.

The reference source directory was read only. No source golden output was
available, so source golden comparison is **not-run**.

## Implemented API

- `NpcGuideSourceProfile.CanHandle(typeId, netId, aiStyle)` requires all three
  identity values `22, 22, 7`.
- `NpcGuideSourceProfileInput` makes weather, server authority, town/housing
  facts, player occupancy, home coordinates, dimensions, and the collision
  query explicit.
- `NpcGuideSourceProfileState` carries the source state slots used by this
  slice. It does not own task lifecycle state or invent a second task owner.
- `NpcGuideSourceProfileResult` returns the pressure flag, eligibility,
  destination, zero velocity, failure reason, and requested effects.
- `NpcGuideSourceProfile.ApplyEffectsAndObserve` uses
  `INpcGuideSourceEffectPort` to preserve effect order and records the owner
  result from the narrow ForceSitting capability.
- `NpcHomeReturnDestinationQuery` is reused for the source `0, -1, +1`
  candidate order, the three-tile vertical clearance, and the source position
  formula.

## Source behavior represented

The profile computes the source return pressure as:

```text
Raining || !DayTime || Eclipse || SlimeRain ||
(IsStorming && Position.Y / 16 < WorldSurface)
```

The home-return gate requires server authority, `townNPC`, a non-homeless
Guide, active return pressure, a home, and a caller-provided negative
`InGoodRestingSpot` result. Either current-NPC or destination-home player
occupancy blocks the request. A successful destination produces zero velocity,
the source destination position, network synchronization, and then an
unconditional narrow force-sitting owner call. When all candidates fail, the
result is `NoPath`; effects mark the Guide homeless before requesting the
existing `QuickFindHome` owner. An eligible request with no collision query is
rejected as a caller contract error rather than converted into `NoPath`.

The source good-resting-spot gate prevents the same completed home operation
from emitting a second teleport on the next evaluation. The caller supplies
that owner-owned fact; task lifecycle remains with the shared task owner.

## Ownership and side effects

| Concern | Read/write owner |
| --- | --- |
| Guide identity selection | `NpcGuideSourceProfile` selector; no new identity store |
| Weather and gate calculation | `NpcGuideSourceProfile` pure calculation |
| Home candidate geometry | `NpcHomeReturnDestinationQuery` |
| Solid tile reads | `INpcHomeReturnCollisionQuery` caller adapter |
| Good resting spot | existing Housing/Town owner, supplied as input |
| Player occupancy | caller snapshot, supplied as input |
| Position and velocity write | `INpcGuideSourceEffectPort` adapter |
| Network update | `INpcGuideSourceEffectPort` network adapter |
| Sitting mutation | existing Town/Housing adapter behind `TryForceSitting`; this checkpoint only carries the source request |
| Homeless and `QuickFindHome` | existing Housing/Town owner behind the effect port |
| Multi-tick task entry, completion, and failure | `NpcTaskLifecycleSystem` and `NpcTaskReference` protocol |

The profile performs no file, network, random, entity-store, or task-owner
side effect. Effects are explicit and tested in source order:

```text
success: home-teleport -> network-sync -> force-sitting(owner)
failure: mark-homeless -> quick-find-home
```

## Independent verifier

Project: `Test/Terraria.NpcAi.GuideProfileVerification/`

Scenarios cover:

1. exact identity selection;
2. night, rain, eclipse, slime rain, surface storm, and above-surface storm;
3. client/server gate, homeless gate, and current/destination player occupancy;
4. source `0, -1, +1` destination order and position formula;
5. odd-width integer destination geometry, missing-query rejection, successful
   teleport, zero velocity, and unconditional sitting capability effect order;
6. one-shot repeat suppression across the next evaluation;
7. all-candidates-blocked `NoPath`, homeless update, and `QuickFindHome` order;
8. shared `GuideReturnHome` task entry, completion, and `NoPath` failure;
9. the narrow good-resting-spot predicate and owner ForceSitting state writes.

Source golden comparison remains `not-run` because no source golden was found.

## Build and run evidence

Evidence directory:
`Build/diagnostics/NpcAiRedesign/guide-source-profile-20261007/`

- `dotnet restore Test/Terraria.NpcAi.GuideProfileVerification/Terraria.NpcAi.GuideProfileVerification.csproj --nologo -v:minimal`: exit `0`.
- `dotnet build src/NSSLC/Component/Npc/Terraria.Npc.csproj --no-restore --nologo -v:minimal`: exit `0`, `0` warnings, `0` errors.
- `dotnet build Test/Terraria.NpcAi.GuideProfileVerification/Terraria.NpcAi.GuideProfileVerification.csproj --no-restore --nologo -v:minimal`: exit `0`, `0` warnings, `0` errors.
- `dotnet run --project Test/Terraria.NpcAi.GuideProfileVerification/Terraria.NpcAi.GuideProfileVerification.csproj --no-build --no-restore`: exit `0`.
- Verifier output: `PASS: Guide source identity, weather return pressure, server/home/player gates, home candidate order, teleport effect order, no-path reassignment, repeat suppression, and shared task composition.`

Build artifacts were emitted under `Build/bin/`; diagnostics and extracted
source evidence were kept under `Build/diagnostics/`.

## Open slices and fallback

- `AI_007_TownEntities_GetWalkPrediction` and its `home X ±35`, crowd,
  drowning, liquid, solid, step, and falling rules are not implemented here.
- The complete `AI_007_TownEntities_IsInAGoodRestingSpot` search is not
  implemented here; the profile consumes an owner-provided fact.
- The complete `AI_007_TryForcingSitting` tile/frame/occupancy mutation is not
  implemented here; the profile emits an ordered request only.
- Conversation, danger response, attacks, doors, random timer consumption,
  and ordinary `ai[0]` movement transitions remain open.
- If a later source differential contradicts this slice, keep the current
  effect port and task owners, isolate the changed branch in a new profile
  checkpoint, and retain this checkpoint as the verified fallback.
