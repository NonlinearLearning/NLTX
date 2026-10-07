# NPC AI Profile Coverage Ledger

Document ID: DOC-2026-10-06-NPC-AI-PROFILE-COVERAGE-LEDGER
Logical domain: npc-ai-complete
Artifact type: ledger
Status: active
Canonical path: docs/migration/ledgers/2026-10-06-npc-ai-profile-coverage-ledger.md
Design: [NPC AI system redesign](../../system-decomposition/2026-10-05-npc-ai-system-redesign.md)
Execution plan: [NPC AI execution plan](../../plans/system-decomposition/2026-10-06-npc-ai-system-execution-plan.md)
Source inventory: [128-style reference index](../../system-decomposition/2026-10-05-npc-ai-reference-index.md)

This ledger separates style entry inventory from concrete content profiles and from behavioral
verification. It is generated from the canonical index rows; it does not claim that a style entry
is a complete profile or that the current finite simulator is equivalent to the reference.

## Source baseline

| Source file | Current SHA-256 | Index SHA-256 | Result |
| --- | --- | --- | --- |
| Terraria/NPC.cs | ed8aa2302730a9e046ef310e543204fba391bd35b1c9c0b213c8065dfbbe39f0 | same | confirmed |
| Terraria/Main.cs | e24e61c9903bb7995f47e36c51edbe43481b643226861b717020877e7e22b63f | same | confirmed |
| Terraria.ID/NPCID.cs | e040b9cfffd57842c0099f11322ee5ec1dddd605743a25bae5415db31b0c025d | same | confirmed |

Reference root: D:/TRbackup/无任何删减通过编译 (read-only). On 2026-10-07,
`Test/Terraria.NpcAi.SourceIdentityVerification` compared the index, this ledger and the read-only
reference files: 3 files and 6/6 SHA-256 comparisons passed. The report is
`Build/diagnostics/NpcAiSourceIdentity/source-identity-report.json`. This verifies pinned source
identity only. The later G3 Eye dash comparison executed 24 scenarios / 36 ticks and found 9
Expert velocity differences; its scope and actual report are linked below. Other profiles without
their own source-output comparison remain `not-run`. Line numbers below remain bound to the same
source content.

At the initial inventory checkpoint the existing reference `TerrariaServer.exe` had not been
launched. G3 subsequently invoked its actual `NPC.AI()` through an isolated reflection harness and
matched its IL and sampled outputs to a build of the pinned-source copy. The read-only source root
was preserved. The historical comparison report is
`Build/diagnostics/NpcAiReferenceVerification/20261006T195702981Z/differential-report.json`, with
`comparison-complete-with-differences`; see [G3 source differential](../../system-decomposition/2026-10-07-npc-ai-g3-reference-differential.md).

## State definitions

- indexed: style branch is present in the source index.
- mapped: a concrete type/netId/profile and its activation conditions are resolved.
- implemented: that concrete profile is connected to the complete runtime path.
- verified: source-based multi-tick and effect behavior has passed for the declared conditions.
- open: transitive source calls, side effects, conditions, or exits still need closure.
- not-run: no source-output differential has been executed for that row.
- A style can remain indexed while some concrete profiles under it progress independently.
  Shared aiStyle does not enable unregistered types.

## First concrete profile

| Profile | Source identity | Activation evidence | Implementation | Call/effect closure | Verification |
| --- | --- | --- | --- | --- | --- |
| Blue Slime, normal default | type=1, netID=1, aiStyle=1 | NPC.SetDefaults, NPC.cs:8686-8699 assigns aiStyle=1, value=25, width=24 and height=18; netID defaults to type at NPC.cs:17934; NPCID.cs:11112 names BlueSlime=1; dispatch NPC.cs:20121-20125 | `NpcBlueSlimeProfile` implements the finite source-state transition, Dirt Slime `ai[1]=2` grounded counter, and Stone/Cloud (`ai[1]=3/751`) gravity adjustments; `NpcBlueSlimeContainedItemGenerator.SelectForTypeOne` covers normal non-Skyblock first item selection and `RuntimeNpcStore` commits the chosen item id or `-1` pending sentinel to the same NPC `ai[1]`, then syncs before target reacquire | partial/open: finite target adapter still supplies Aggro=0, NoAggro=false, Gross=true, no pet/NPC candidates; LowTiles/Skyblock is gated off; remaining `ai[1]` variant stats/physics/Buff/Tile/Projectile/NPC/presentation effects, full Tile/collision scratch, `UnifiedRandom`, source network sink, authority and reference golden remain open; the scanned NPCLoot method has no `ai[1]`-specific ordinary item drop branch; other style-1 identities remain unregistered | pure verifier covers source choice/effect order, Dirt counter/frozen-sentinel order, and Stone/Cloud gravity conditions; 120 tick finite host smokes observe `ai[1]` pending sentinel; full 36-evidence host verifier passed; not-run against reference output |
| Demon Eye, normal default | type=2, netID=2, aiStyle=2 | NPC.SetDefaults, NPC.cs:8700-8711 assigns aiStyle=2 to type 2; netID defaults to type at NPC.cs:17934; NPCID.cs:11114 names DemonEye=2; dispatch NPC.cs:20126-20130; `AI_002_FloatingEye` NPC.cs:53027-53509 | `NpcFloatingEyeProfile` implements the type=2 path collision bounce, day/surface discouragement, generic scaled acceleration, wet correction, explicit dust roll, and ordered target/despawn effect intents. `RuntimeNpcStore.UpdateMovement` selects only this identity, passes the entity's current target slot, commits direction/target-selection/physics/effect-intent components, then runs TileCollision and commits collision history | partial/open: finite host supplies the same finite player-target adapter and records EncourageDespawn/dust intents; ZoneGraveyard, full TargetClosest candidate inputs, real dust presentation, collision/Tile scratch, network authority, and source golden remain open; style-2 variants remain unregistered | pure profile verifier passed; 60 tick finite host smoke and 33-evidence host verifier passed; current host rebuild is blocked by shared workspace errors; not-run against reference output |
| Zombie, normal Fighter default | type=3, netID=3, aiStyle=3 | NPC.SetDefaults, NPC.cs:8712-8723 assigns aiStyle=3 to type 3; netID defaults to type at NPC.cs:17934; NPCID.cs:11116 names Zombie=3; dispatch NPC.cs:20131-20135; `AI_003_Fighters` NPC.cs:56637-61118 | `NpcFighterProfile` implements the finite type=3 state slice: direction initialization, target reacquire intent, daytime surface despawn intent, two-tick idle turn, scaled 1.0 speed cap/0.07 acceleration, grounded 0.8 damping, one-to-three-tile jump intents, a 60-tick closed-door wait followed by `RequestOpenDoor`, and source target-bottom `DirectionY` plus hit-reset inputs. `RuntimeNpcStore.UpdateMovement` captures Tile facts before AI, consumes the real hit latch before profile evaluation, commits jump impulse before gravity/TileCollision, and routes successful `WorldGen.OpenDoor` mutations through the Tile owner projection | partial/open: source helper conditions, expert/graveyard/blood-moon exceptions, shared Tile scratch, full combat branches, presentation/sound, authority/network sink and reference golden remain open; finite host records daytime despawn and target reacquire together to preserve its scripted combat path; Skeleton/GoblinPeon and other style-3 identities remain unregistered | pure verifier passed with C4 traversal, hit-reset and target-bottom cases; 180 tick finite host smoke/full 35-evidence host verifier are historical pre-continuation evidence; current host rebuild is blocked by shared workspace errors; not-run against reference output or a seeded door map |
| Mother Slime | type=16, netID=16, aiStyle=1 | `NPC.SetDefaults` type 16 sets aiStyle=1 (`NPC.cs:9077-9093`); netID defaults to type (`NPC.cs:17934`); `NPCID.MotherSlime=16` (`NPCID.cs:11142`); dispatch to `AI_001_Slimes` (`NPC.cs:20121-20125`); helper has no explicit type=16 branch and `SlimeCanContainItems` excludes 16 (`NPC.cs:61133-62551`; `NPCID.cs:4820`) | `NpcMotherSlimeProfile` exact identity handler covers finite public wet/landing/jump/counter state, commits through `RuntimeNpcStore.UpdateMovement`, and explicitly rejects contained-item generation | partial/open: finite catalog netID 16 is still Green Slime geometry/stats; target adapter is finite-player only; Tile/collision scratch, authority/network, death/split effects and source golden remain open | pure verifier PASS; 120 tick host smoke PASS with `ai[2]=1`, `ai[0]=-1120`; source comparison not-run |
| Eye of Cthulhu opening / first dash | type=4, netID=4, aiStyle=4 | Inline AI branch `NPC.cs:20136+`; `NPCID.EyeofCthulhu=4` (`NPCID.cs:11118`); opening counter and threshold conditions are recorded in F1 checkpoint | Exact identity handler and `RuntimeNpcStore` dispatch/content/effect trace are source-wired; night Classic/Expert hover and three-dash boundaries now have twelve accepted host replays; full transformation/servants continuation remains in progress | partial/open: full transformation, servants, target inputs, authority/network, persistence and presentation remain open; current 32-scenario/182-tick source sample has zero velocity differences but 19 Slime state/observable differences keep the gate failed | NPC verifier and historical 37-evidence regression PASS; twelve night host replays exit 0 on the fingerprinted Simulation artifact; normal build exit 0 with 17 warnings / 0 errors; full regression for later shared-source changes has not been rerun |
| Lava Slime | type=59, netID=59, aiStyle=1 | `NPC.SetDefaults` type 59 sets aiStyle=1 and applies Remix stat overrides (`NPC.cs:9630-9650`); netID defaults to type (`NPC.cs:17934`); `NPCID.LavaSlime=59` (`NPCID.cs:11228`); dispatch to `AI_001_Slimes` (`NPC.cs:20121-20125`); `SlimeCanContainItems` includes 59 (`NPCID.cs:4820`); normal/Remix differences are explicit in `NPC.cs:61864-61866,62265-62299,62369-62371,62449-62467` | mapped; no source profile implementation | open: Remix and wet/jump variants, contained-item/hellstone effects, common helper calls, host identity and authority remain unclosed | not-run |

The finite simulator's netId=16 currently labels Green Slime; the reference catalog uses type=16
for Mother Slime. This is an identity mismatch, not a profile alias. The finite Guide, Old Man, and
Training Dummy style values also differ from their reference defaults as recorded in the design.
No legacy catalog identity is changed by this ledger.

### Blue Slime C1 implementation boundary

The production profile is split at the source effect boundary instead of silently importing the
whole `AI_001_Slimes` body. `NpcBlueSlimeProfile.CanHandle` requires type=1, netID=1 and aiStyle=1;
`NpcBlueSlimeProfile.Evaluate` is deterministic and receives all facts
needed by the normal type=1 movement slice. It returns the next four source slots, velocity,
direction, `aiAction`, branch flags, and three ordered effect intents:

1. `ContainedItemGenerationRequested` for the non-client `SlimeCanContainItems` entry at
   `NPC.cs:61150-61179`. `NpcBlueSlimeContainedItemGenerator.SelectForTypeOne` now reproduces the
   normal non-Skyblock type=1 outer choice order from `NPC.cs:61150-61488`, calls the item helper
   mapping at `NPC.cs:62552-62669`, and the runtime owner commits the selected id or `-1` pending
   sentinel to this instance's `ai[1]` before the final profile evaluation. This selects variant
   state; it does not spawn a runtime world item.
2. `NetUpdateRequested` for direction initialization and jump submission; when the source sets this
   before a target call, the owner observes it before target reacquire.
3. `TargetClosestRequested` for the `ai[2] == 0` initialization and the wet/night jump boundary.

The transition preserves the source counter thresholds (`0`, `-500..-1000`, `-1500..-2000`),
the normal jump impulses (`-6/-8` and `+2/+3` horizontal), wet vertical cap (`-4`), and the
  `-999` frozen sentinel. The runtime owner selects the item before final evaluation;
  `NpcBlueSlimeProfile.ApplyEffects` commits the resulting network intent before target reacquisition.
  The initial path is item → network → target, while wet-only and jump paths differ. This is a
concrete C1 profile contract and verifier slice, not a claim that the
finite host is source-equivalent or that all type=1 variants share the profile. The finite host now
uses this contract for the exact identity. It additionally runs the normal type=1 item selector using
finite world facts and the grounded `ai[1] == 2` Dirt Slime counter before the `ai[0] == -999` sentinel
and jump-phase update. LowTiles/Skyblock, other SlimeCanContainItems types, and remaining `ai[1]`
variant effects remain out of scope.

### Blue Slime C1 finite-host checkpoint (2026-10-06)

`RuntimeNpcStore.UpdateMovement` gates the profile on `type=1 / netID=1 / aiStyle=1`, captures the
current target slot and movement history, commits the returned ai[4] and direction, and applies the
effect port through `NpcTargetSelectionSystem.Select` for the finite nearest living-player target
selection and network intent. The finite owner then uses its existing gravity and TileCollision path.
At this earlier checkpoint, `CanContainItems=false` was explicit because no runtime owner had accepted
the source contained-item request. The later ordinary-world item-selection checkpoint below supersedes
that boundary.

| Evidence | Command/output | Result |
| --- | --- | --- |
| Host build | `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore --nologo -v:minimal` | exit 0; 0 warnings / 0 errors; artifact under `Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/` |
| C1 smoke | `... 120 --players 0 --spawn-npc 1 --report .../stage-b2-target-selection-runtime-20261006/c1-smoke-120.json` | `Succeeded=true`; 120 ticks; type=1 position/collision state observable; final tick update true |
| C1 player smoke | same host with `--players 1 --spawn-npc 1` | `Succeeded=true`; finite player-target selection call and Tile path execute; report observes type=1 movement in `.../c1-smoke-120-player.json` |
| Host regression | `verify-final.ps1 .../stage-b2-target-selection-runtime-20261006/full-verifier-20261006` | exit 0; `Succeeded=true`; 33 evidence; simulation/clock builds and runs 0 warnings / 0 errors; target commit itself is covered by the host call path, not a separate report field |

This checkpoint does not close contained items, stat/presentation effects, NewNPC/NewProjectile,
Tile placement, real network sink, full authority or source golden comparison; Mother/Lava and other
style-1 identities remain open.

### Blue Slime source closure checkpoint

The style-1 dispatcher calls AI_001_Slimes directly. A lexical scan of that method body
(NPC.cs:61133-62551) found the local helpers AI_001_SetRainbowSlimeColor,
AI_001_Slimes_GenerateItemInsideBody, AnyLifeCrystalSlimes, and TargetClosest. The C1 slice now
implements the normal movement transition, item helper mapping, ordinary-world outer selection that
commits `ai[1]`, and the same-tick grounded Dirt Slime counter. It does not create a world item entity.
A bounded scan of `NPCLoot` found no `ai[1]`-specific ordinary item drop branch; the remaining variant,
death, and effect closure is still open. Other direct call names
include collision/target checks, NewNPC, NewProjectile, tile placement and tile sync, light/dust/
gore/particle requests, and attack damage helpers. These names are an inventory aid, not a closed
transitive graph. The normal Blue Slime conditions still need to be isolated from other style-1
types and world variants, then each reachable helper's reads, writes, authority gate, immediate
result, and order must be recorded before full implementation.

Observable state already visible in the source includes ai[0..1], target/direction, velocity and
combat fields; the broader method also has item-in-body, child-NPC, projectile, Tile, network and
presentation branches. Their reachability for this exact profile is not yet established. The
current finite jump evaluator is not a source-output golden and remains not-run for reference parity.

### Blue Slime ordinary-world contained-item selection checkpoint (2026-10-06)

`NpcBlueSlimeContainedItemGenerator.SelectForTypeOne` now models the normal-world entry guard and
ordered item selection for exact type=1/netID=1: current `ai[1]`, net mode and value gates; Slime Rain
attempt count; helper versus additional item roll; surface/underground and rock-layer branch; special
item chance; birthday party; Remix first attempt; and Vampire seed. It uses reference geometry and
catalog values for this identity (24×18, value 25). The runtime owner supplies WorldSession flags,
world layers, moon phase and GenuineParty, writes the resulting selected ID or `-1` pending sentinel
back into the NPC's `ai[1]` before the profile's final evaluation, and requests network sync before
target reacquisition. This lets the selected Dirt Slime (`ai[1] == 2`) advance `ai[0]` by 9 while
grounded in the same tick, before the frozen-sentinel and jump-phase checks.

The host uses finite .NET `Random` and `NetMode=0`; this is not the source `UnifiedRandom` sequence or
client/server authority. Skyblock/LowTiles remains gated off because the runtime host does not expose
all `WorldGen.Skyblock` rules. A selected item ID in `ai[1]` is variant state, not a world item. The
source Dirt Slime branch at `NPC.cs:61579-61585` adds 9 to grounded `ai[0]` before the `-999` sentinel
check at `61841`, then the general grounded counter and jump-phase logic still execute in that tick
(`62325-62360`). The finite profile now preserves that ordering. Source AI also has other `ai[1]`-dependent
stat, physics, buff, Tile, projectile, NPC-spawn and presentation branches (`61489-61834`); only the Dirt
Slime counter is implemented here. `HitEffect` has a Confetti Slime (`ai[1] == 1345`) presentation
branch at `NPC.cs:86138-86161`. A bounded scan of `NPCLoot` (`NPC.cs:80031-81030`) found no `ai[1]`-
specific ordinary item drop branch; this does not close the full death/effect graph or prove reference
parity. Other `ai[1]` variant effects, LowTiles/Skyblock, source random/authority, and item/stat/presentation
paths remain open.

The Simulation report now includes NPC `Ai0..Ai3` and `LocalAi0..LocalAi3`, allowing the smoke to
observe the AI owner commit. In both 120 tick smokes the spawned Blue Slime starts with `ai[1]=0` and
finishes with `ai[1]=-1`; the player smoke records two accepted hits and life 19. This demonstrates the
finite host state transition and continued tick execution, not that the reference would choose the same
item or random draw.

| Evidence | Command/output | Result |
| --- | --- | --- |
| Simulation host build after 24×18/value=25 mapping | `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore --nologo -v:minimal` | exit 0; 0 warnings / 0 errors; artifact under `Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/` |
| NPC AI verifier | build `Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-restore --nologo -v:minimal`; run with `--no-build --no-restore` | both exit 0; build 0 warnings / 0 errors; PASS for outer choice, party/Remix/No Traps, gates, effect order and existing C1-C4 cases |
| No-player and player smoke | two 120 tick Simulation runs with `--spawn-npc 1`, players 0/1 | both `Succeeded=true`; `ai[1]=-1`; final NPC update true; player run records 2 accepted hits and life 19 |
| Full host regression | `verify-final.ps1 -WorldPath Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld -OutputDirectory Build/diagnostics/NpcAiRedesign/runs/stage-c1-contained-item-selection-20261006-r2/full-verifier-20261006` | exit 0; `Succeeded=true`; 36 evidence; Simulation/clock build and run all 0 warnings / 0 errors |

Full source-output comparison is not run. Remaining `ai[1]` variant, stat and presentation effects,
LowTiles/Skyblock, source random compatibility, full target/collision inputs, death/effect closure,
networking authority and other style-1 identities remain open. The bounded `NPCLoot` scan found no
`ai[1]`-specific ordinary item drop branch; this is not a complete death-path equivalence result.

### Blue Slime C1.1 Dirt Slime same-tick state handoff (2026-10-06)

The owner now selects and commits a normal-world `ai[1]` result before the final profile evaluation.
`NpcBlueSlimeProfile.WithContainedItemSelection` is the shared pure handoff used by the runtime owner
and verifier. The verifier feeds a deterministic selected Dirt item (`ai[1] = 2`) into a grounded
`ai[0] = -999` state and checks the source order: `+9`, grounded cadence, then the `-500..-1000`
jump phase, which sets `ai[0] = -2120`. `ApplyEffects` accepts that already-selected result so it does
not consume a second random draw, while retaining network-before-target order.

The finite 120 tick host smokes used the normal host random seed and both ended with `ai[1] = -1`;
they did not select a Dirt item and therefore do not demonstrate this branch in a runtime smoke. The
deterministic profile verifier covers the branch. Reference-output parity remains not-run.

| Evidence | Command/output | Result |
| --- | --- | --- |
| NPC AI verifier | build `Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-restore --nologo -v:minimal`; run `dotnet run --project Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-build --no-restore` | both exit 0; build 0 warnings / 0 errors; PASS including C1 selected-item handoff and jump phase |
| Simulation build | `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore --nologo -v:minimal` | exit 0; 17 warnings / 0 errors; all warnings are in existing `NSSLC.WorldGeneration` files; artifact under `Build/bin/` |
| 0/1 player C1 smoke | `Build/diagnostics/NpcAiRedesign/runs/stage-c1-dirt-slime-counter-20261006-final/c1-smoke-120-no-player.json` and `c1-smoke-120-player.json` | both `Succeeded=true`, 120 ticks, final NPC update true, `ai[1]=-1`; player run accepted 2 hits; this seed did not select Dirt item |
| Full host regression | `Build/diagnostics/NpcAiRedesign/runs/stage-c1-dirt-slime-counter-20261006-final/full-verifier/summary.json` | exit 0; `Succeeded=true`; 36 evidence; Simulation/clock build and run 0 warnings / 0 errors |

### Demon Eye C2 implementation boundary

The normal profile is fixed to `type=2 / netID=2 / aiStyle=2`; `NpcFloatingEyeProfile.CanHandle`
rejects The Hungry II, Wandering Eye, and other style-2 variants. `NpcFloatingEyeProfile.Evaluate`
receives `collideX/Y`, `oldVelocity`, `noTileCollide`, day/surface and graveyard facts, scale, wet
state, and an explicit `DustRoll`. It returns velocity, directions, `noGravity`, and branch/effect
intents while preserving source order: collision bounce, discouragement or target selection, generic
acceleration, dust roll, then wet correction.

`INpcFloatingEyeProfileEffectPort` owns `EncourageDespawn(10)`, TargetClosest, and dust presentation;
`INpcFloatingEyeRandomPort` supplies one `Next(40)` roll per evaluation. The verifier covers identity
rejection, collision bounce, day/surface discouragement, wet damping, random consumption, and target →
dust → wet-target order. The finite host now invokes this profile for type=2 and stores the effect
intent in `NpcImmediateEffectStateComponent`; its report exposes the intent without claiming a real
presentation sink. This is a C2 contract plus limited host wiring, not a reference-output golden;
type=116 and type=133 remain open.

### Demon Eye C2 finite-host checkpoint (2026-10-06)

`RuntimeNpcStore.UpdateMovement` gates the profile on `type=2 / netID=2 / aiStyle=2`, captures
old velocity/collision/wet facts, consumes an explicit `Next(40)` roll, commits direction and
`NoGravity`/`NoTileCollide`, passes the entity's current target slot into the profile, then applies the
effect port and runs TileCollision or bounded movement.
`NpcMovementTickStateComponent` records `CollideX/CollideY`; `NpcImmediateEffectStateComponent`
records only current-tick despawn and dust intents. The release path captures `NpcInstanceId` before
ECS termination so projectile death can remove the identity index without reading a removed component.

| Evidence | Command/output | Result |
| --- | --- | --- |
| Host build | `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore --nologo -v:minimal` | exit 0; 0 warnings / 0 errors; artifact under `Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/` |
| C2 smoke | `... 60 --players 0 --spawn-npc 2 --report .../stage-b2-target-selection-runtime-20261006/c2-smoke-60.json` | `Succeeded=true`; 60 ticks; final NPC update true; type=2 `NoGravity=true`, moved, `DespawnEncouragementTicks=10` |
| Host regression | `verify-final.ps1 .../stage-b2-target-selection-runtime-20261006/full-verifier-20261006` | exit 0; `Succeeded=true`; 33 evidence; simulation/clock builds and runs 0 warnings / 0 errors |
| NPC AI verifier | `dotnet build/run Test/Terraria.NpcAi.Verification/...` | build 0/0; run PASS including C1/C2 |

This checkpoint does not close ZoneGraveyard, complete TargetClosest/aggro/noAggro/gross inputs,
real dust/network presentation, source golden comparison, other style-2 identities, or full
authority/save semantics. Blue Slime C1 and Demon Eye C2 remain finite host slices rather than
complete source profiles.

### Zombie C3 finite-host checkpoint (2026-10-06)

`NpcFighterProfile.CanHandle` requires `type=3 / netID=3 / aiStyle=3`; a shared `aiStyle=3` never
activates the profile for Skeleton, GoblinPeon, or another Fighter type. The pure transition receives
position/velocity, four source slots, direction, scale, day/surface, grounded, and hit facts. It returns
the updated four slots, direction, velocity, action, target/despawn/network intents, and branch flags.
`RuntimeNpcStore.UpdateMovement` applies the effect port in the order despawn intent → target reacquire →
network sync, then runs finite gravity/TileCollision and commits collision history.

The source helper normally separates its daytime despawn branch from target selection. The finite host
keeps both intents visible in this slice because the existing scripted combat verifier needs a target to
remain selected; this is recorded as a host compatibility difference and is not a reference-equivalence
claim. Tile step/door side effects, complete jump/combat actions, presentation, authority/network sink,
and source golden remain open.

| Evidence | Command/output | Result |
| --- | --- | --- |
| Host build | `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore --nologo -v:minimal` | exit 0; 0 warnings / 0 errors; artifact under `Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/` |
| C3 profile verifier | `dotnet build Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-restore --nologo -v:minimal`; `dotnet run --project Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-build --no-restore` | build 0/0; run PASS with identity rejection, acceleration, idle turn, scale damping, daytime and target intents |
| C3 smoke | `... 120 --players 1 --spawn-npc 3 --report .../stage-c3-fighter-profile-20261006/fighter-smoke-120.json` | `Succeeded=true`; 120 ticks; movement/collision observable; `DespawnEncouragementTicks=10`; final update true |
| C3 cross-day smoke | `... 1 --players 1 --world-time-rate 50000 --spawn-npc 3 --report .../stage-c3-fighter-profile-20261006/fighter-night-1.json` | `Succeeded=true`; day → night transition; type=3 still moves; daytime intent clears |
| Full host verifier | `& Build/diagnostics/NpcAiRedesign/verify-final.ps1 -WorldPath .../generated.wld -OutputDirectory .../stage-c3-fighter-profile-20261006/full-verifier-20261006-r2` | exit 0; `summary.json` `Succeeded=true`; 35 evidence; Simulation/clock builds and runs 0 warnings / 0 errors; scripted Zombie 45 hits, 1 Gel drop and 1 pickup |

C3 rollback removes `NpcFighterProfile*`, `INpcFighterProfileEffectPort`, `NpcFighterSourceBranch`,
and the exact identity host branch, restoring `NpcFighterAiBehavior`; evidence and the open style-3
ledger row remain. This slice does not progress the style-3 row to complete source coverage.

### C2/C3 source-input continuation checkpoint (2026-10-07)

The continuation keeps the same exact identities and finite-host boundary. C2 no longer supplies a
sentinel `TargetSlot=-1` to `NpcFloatingEyeProfileInput`; it captures the NPC's current target slot
before evaluation. This removes a hidden host input while leaving the target candidate and
`ZoneGraveyard` facts explicitly open.

C3 now has a real hit latch. `NpcHitStateComponent` is attached with the NPC entity, a successful
non-lethal projectile strike commits `JustHit`, and the next Fighter movement tick consumes it before profile
evaluation. The input also carries the current target center, player target height, and NPC height;
when the source bottom-alignment condition is true, the profile returns `DirectionY=-1` and the host
commits it. The independent verifier covers hit-before-idle-turn and target-bottom alignment.

| Evidence | Command/output | Result |
| --- | --- | --- |
| NPC focused build | `dotnet build src/NSSLC/Component/Npc/Terraria.Npc.csproj --no-restore --no-dependencies --nologo -v:minimal` | exit 0; 0 warnings / 0 errors; `Build/bin/Terraria.Npc/Debug/net10.0/Terraria.Npc.dll` |
| NPC AI verifier focused build | `dotnet build Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-restore --no-dependencies --nologo -v:minimal` | exit 0; 0 warnings / 0 errors |
| NPC AI verifier run | `dotnet run --project Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-build --no-restore` | exit 0; PASS including C2 Demon Eye, C3 Fighter, hit consumption and target-bottom direction boundary |
| Normal verifier build first attempt | `dotnet build Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-restore --nologo -v:minimal` | exit 1; pre-existing shared `WorldStorageRoot.cs` missing `Dispose` diagnostics and 6 warnings |
| Normal verifier build retry | same command after dependency outputs were refreshed | exit 0; 0 warnings / 0 errors |
| Simulation focused build attempt | `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore --no-dependencies --nologo -v:minimal` | exit 1; first attempt printed 15 shared errors; latest attempt has two `RuntimeProjectileStore.cs` `CS0305` errors at lines 207 and 311; modified host binary smoke not run |

The continuation is verified only for the NPC domain assembly and the independent deterministic
verifier. It does not promote C2/C3 to source-complete or verified: ZoneGraveyard, full
TargetClosest candidate/aggro inputs, expert/graveyard/blood-moon helper conditions, complete combat
actions, Tile scratch parity, presentation, authority/network delivery, save/load/unload cleanup,
other style-2/style-3 identities, and reference golden comparison remain open.

The first blocking diagnostics were: normal verifier build `CS1061` at `WorldStorageRoot.cs:68,26`,
`:72,23`, and `:73,20` because `ProjectileIdentityIndex`, `TileEntityUpdateSchedule`, and
`WorldPressurePlateRegistryComponent` have no `Dispose`; Simulation focused build reported
`RuntimeWorldItemStore.cs:61,39`/`:119,39`/`:247,39` `CS0453`, `:61,62`/`:119,62`/`:247,62`
`CS0029`, `:236,51` `CS1503`, `:254,53` `CS7036` (`timeLeftTicks`),
`RuntimePlayerStore.cs:219,45`/`:314,36` `CS7036` (`itemRegistry`), `:530,29` `CS7036`
(`commandId`), `RuntimeProjectileStore.cs:207,21`/`:311,21` `CS0305` (six generic type
arguments required), `:654,43` `CS0246` (`ProjectileScaleComponent`), and `:662,57` `CS1729`
(`ProjectileCollisionGeometryInput` has no ten-argument constructor). These files are outside the
C2/C3 change boundary and were not repaired. After dependency outputs were refreshed, the normal
verifier rebuilt successfully; the latest Simulation attempt still fails on the two `CS0305` sites.

### Guide D1 finite task lifecycle checkpoint (2026-10-06)

| Profile | Source/host identity | Implementation | Closure | Verification |
| --- | --- | --- | --- | --- |
| Guide finite task lifecycle | Simulation Guide `netID=22`; day patrol and night home branches | `NpcTaskKind`/`NpcTaskPhase`/`NpcTaskFailureReason`/`NpcTaskStateComponent`; `NpcTaskLifecycleSystem` (`Enter`/`Advance`/`Complete`/`Fail`/`Interrupt`/`Reset`); `RuntimeNpcEntity` task snapshot; `RuntimeNpcStore.SyncGuideTask` | finite only: day `GuideDayPatrol`, night with home `GuideReturnHome`, no task interrupts with `TargetUnavailable`; cursor advances once per real tick; repeated entry is idempotent and replacement passes through the interruption path; housing allocation, teleport, seating, dialogue, rescue and complete town source effects remain open | NPC AI verifier PASS with idempotent entry, progress, completion, replacement, failure, interruption and reset cases; Guide 200 tick report `Succeeded=true`, `TaskPhase=Running`, `TaskCursor=200`; full host verifier r4 `summary.json` `Succeeded=true`, 33 evidence; not-run against reference output |

The D1 task state is an instance execution record, not a housing authority or general behavior-tree
blackboard. It proves the state owner and lifecycle API at one real town host boundary; it does not
promote style 7 or style 0 entries to complete town profile coverage.

### Guide D2 finite home-return checkpoint (2026-10-06)

| Profile | Source/host identity | Implementation | Closure | Verification |
| --- | --- | --- | --- | --- |
| Guide finite home return | Simulation Guide `netID=22`; `GuideReturnHome` cursor 60 boundary | `INpcHomeReturnCollisionQuery`, `NpcHomeReturnDestination`, and `NpcHomeReturnDestinationQuery` implement the bounded candidate order `0, -1, +1` and three-cell clearance check; `RuntimeNpcEntity` and `NpcImmediateEffectStateComponent` expose submit/result snapshots; `RuntimeNpcStore` commits teleport, zero velocity, network update, task completion, or `NoPath` failure plus homeless fallback | finite host slice only: housing fact is supplied by the probe, collision query is tile-backed, and immediate effects are observable; full housing allocation/validation, path search, seating, dialogue, danger preemption, real network delivery, and town profile source closure remain open | NPC AI verifier PASS with candidate-order and all-blocked cases; success/blocked/invalid 60-tick host probes exit 0; success reports `GuideReturnHome/Completed` and `HomeTeleportSucceeded`; blocked/invalid report `TaskFailureReason=NoPath`, `HomeTeleportFailed`, and homeless housing; full 35-evidence host verifier `Succeeded=true`; not-run against reference output |

D2 does not promote the Guide style-7 row to complete coverage. It verifies one bounded return-home
effect protocol after the D1 task lifecycle boundary and keeps housing ownership outside the task state.

### Guide D3 housing revalidation and owner synchronization checkpoint (2026-10-06)

| Profile | Source/host identity | Implementation | Closure | Verification |
| --- | --- | --- | --- | --- |
| Guide housing fact revalidation | Simulation Guide `netID=22`; town NPC housing relation before task selection | `RuntimeNpcStore.RevalidateHousing` calls `WorldGen.IsHousingRoomValidAt` before `SyncGuideTask`; valid rooms synchronize `TownHousingRegistrySystem.AssignRoom`; invalid rooms clear ECS home, mark homeless in both owners, request network update, and capture `HousingRevalidationFailed`/`HousingRegistrySynchronized`; homeless persistence projects `(-1,-1)` | finite owner boundary only: revalidation runs for town NPCs and registry remains keyed by NPC type; complete allocation, unload cleanup, seating, dialogue, rescue and reference parity remain open; invalid housing now prevents night `GuideReturnHome` entry | NPC AI verifier PASS with housing effects; valid/invalid host probes exit 0; save/reload probe preserves Guide `Homeless=true` and `HomeX/HomeY=-1/-1`; full host verifier `stage-d3-housing-revalidation-20261006/full-verifier/summary.json` is `Succeeded=true` with 35 evidence and 0 build/run warnings/errors; not-run against reference output |

D3 is a fact-owner checkpoint after D2. It does not promote the Guide style-7 row to complete coverage and
does not reinterpret the historical D2 invalid report; the updated ordering is recorded in the verification
document and the D3 invalid report.

## Style entry inventory

The following 128 rows reproduce the canonical style branch ranges, direct helper names, and example
types from the source index. Rows are initial inventory only; concrete type/world/difficulty/authority
profiles and transitive call/effect closures remain unmapped until individually reviewed.

| Style | Branch lines | Direct helper(s) | Index examples | Entry | Profiles | Closure | Verification |
| ---: | --- | --- | --- | --- | --- | --- | --- |
| 0 | 20001–20120 | `AI_000_TransformBoundNPC` (45503) | BoundGoblin (105)、BoundWizard (106)、BoundMechanic (123) | indexed | unmapped | open | not-run |
| 1 | 20121–20125 | `AI_001_Slimes` (61133) | BlueSlime (1)、MotherSlime (16)、LavaSlime (59) | partial (type/netId/aiStyle identities mapped for all three examples) | partial (Blue Slime and Mother Slime finite host wiring; Lava unmapped) | open | partial (Blue Slime and Mother Slime pure verifier + finite host smoke; no reference golden) |
| 2 | 20126–20130 | `AI_002_FloatingEye` (53027) | DemonEye (2)、TheHungryII (116)、WanderingEye (133) | partial (Demon Eye mapped) | partial (Demon Eye finite host wiring) | open | partial (pure verifier + finite host smoke; no reference golden) |
| 3 | 20131–20135 | `AI_003_Fighters` (56637) | Zombie (3)、Skeleton (21)、GoblinPeon (26) | partial (Zombie mapped) | partial (Zombie finite host wiring) | open | partial (pure verifier + finite host smoke; no reference golden) |
| 4 | 20136–20985 | 内联 | EyeofCthulhu (4) | partial (type/netId/aiStyle identity mapped) | partial (opening/first-dash profile and finite host source wiring) | open | partial (NPC verifier and daytime host smokes passed; source dash comparison found 9 Expert velocity differences) |
| 5 | 20986–20990 | `AI_005_EaterOfSouls` (50966) | ServantofCthulhu (5)、EaterofSouls (6)、MeteorHead (23) | indexed | unmapped | open | not-run |
| 6 | 20991–20995 | `AI_006_Worms` (51709) | DevourerHead (7)、DevourerBody (8)、DevourerTail (9) | indexed | unmapped | open | not-run |
| 7 | 20996–21000 | `AI_007_TownEntities` (53740) | Merchant (17)、Nurse (18)、ArmsDealer (19) | indexed | unmapped | open | not-run |
| 8 | 21001–21575 | `AI_AttemptToFindTeleportSpotNearBooks` (19166)<br>`AI_AttemptToFindTeleportSpot` (19092)<br>`AI_FindNearbyBook` (63141) | FireImp (24)、GoblinSorcerer (29)、DarkCaster (32) | indexed | unmapped | open | not-run |
| 9 | 21576–21779 | 内联 | BurningSphere (25)、ChaosBall (30)、WaterSphere (33) | indexed | unmapped | open | not-run |
| 10 | 21780–22130 | 内联 | CursedSkull (34)、GiantCursedSkull (289)、WaterBoltMimic (694) | indexed | unmapped | open | not-run |
| 11 | 22131–22515 | 内联 | SkeletronHead (35)、DungeonGuardian (68) | indexed | unmapped | open | not-run |
| 12 | 22516–22830 | 内联 | SkeletronHand (36) | indexed | unmapped | open | not-run |
| 13 | 22831–23121 | 内联 | ManEater (43)、Snatcher (56)、Clinger (101) | indexed | unmapped | open | not-run |
| 14 | 23122–23770 | 内联 | Harpy (48)、CaveBat (49)、JungleBat (51) | indexed | unmapped | open | not-run |
| 15 | 23771–23775 | `AI_015_KingSlime` (43670) | KingSlime (50) | indexed | unmapped | open | not-run |
| 16 | 23776–24305 | 内联 | Goldfish (55)、CorruptGoldfish (57)、Piranha (58) | indexed | unmapped | open | not-run |
| 17 | 24306–24436 | 内联 | Vulture (61)、Raven (301) | indexed | unmapped | open | not-run |
| 18 | 24437–24691 | 内联 | BlueJellyfish (63)、PinkJellyfish (64)、GreenJellyfish (103) | indexed | unmapped | open | not-run |
| 19 | 24692–24822 | 内联 | Antlion (69) | indexed | unmapped | open | not-run |
| 20 | 24823–24900 | 内联 | SpikeBall (70) | indexed | unmapped | open | not-run |
| 21 | 24901–24952 | 内联 | BlazingWheel (72) | indexed | unmapped | open | not-run |
| 22 | 24953–25542 | 内联 | Pixie (75)、Wraith (82)、Gastropod (122) | indexed | unmapped | open | not-run |
| 23 | 25543–25622 | 内联 | CursedHammer (83)、EnchantedSword (84)、CrimsonAxe (179) | indexed | unmapped | open | not-run |
| 24 | 25623–25847 | 内联 | Bird (74)、BirdBlue (297)、BirdRed (298) | indexed | unmapped | open | not-run |
| 25 | 25848–25940 | 内联 | Mimic (85)、PresentMimic (341)、IceMimic (629) | indexed | unmapped | open | not-run |
| 26 | 25941–25944 | `AI_026_Unicorns` (63201) | Unicorn (86)、Wolf (155)、HeadlessHorseman (315) | indexed | unmapped | open | not-run |
| 27 | 25945–26367 | 内联 | WallofFlesh (113) | indexed | unmapped | open | not-run |
| 28 | 26368–26525 | 内联 | WallofFleshEye (114) | indexed | unmapped | open | not-run |
| 29 | 26526–26723 | 内联 | TheHungry (115) | indexed | unmapped | open | not-run |
| 30 | 26724–27342 | 内联 | Retinazer (125) | indexed | unmapped | open | not-run |
| 31 | 27343–27962 | 内联 | Spazmatism (126) | indexed | unmapped | open | not-run |
| 32 | 27963–28281 | 内联 | SkeletronPrime (127) | indexed | unmapped | open | not-run |
| 33 | 28282–28586 | 内联 | PrimeSaw (129) | indexed | unmapped | open | not-run |
| 34 | 28587–28866 | 内联 | PrimeVice (130) | indexed | unmapped | open | not-run |
| 35 | 28867–29102 | 内联 | PrimeCannon (128) | indexed | unmapped | open | not-run |
| 36 | 29103–29337 | 内联 | PrimeLaser (131) | indexed | unmapped | open | not-run |
| 37 | 29338–29341 | `AI_037_Destroyer` (50467) | TheDestroyer (134)、TheDestroyerBody (135)、TheDestroyerTail (136) | indexed | unmapped | open | not-run |
| 38 | 29342–29483 | 内联 | SnowmanGangsta (143)、MisterStabby (144)、SnowBalla (145) | indexed | unmapped | open | not-run |
| 39 | 29484–30010 | 内联 | GiantTortoise (153)、IceTortoise (154)、SolarSroller (417) | indexed | unmapped | open | not-run |
| 40 | 30011–30243 | 内联 | WallCreeperWall (165)、JungleCreeperWall (237)、BlackRecluseWall (238) | indexed | unmapped | open | not-run |
| 41 | 30244–30507 | 内联 | Herpling (174)、Derpling (177)、ChatteringTeethBomb (378) | indexed | unmapped | open | not-run |
| 42 | 30508–30538 | 内联 | LostGirl (195) | indexed | unmapped | open | not-run |
| 43 | 30539–31225 | 内联 | QueenBee (222) | indexed | unmapped | open | not-run |
| 44 | 31226–31495 | 内联 | FlyingFish (224)、GiantFlyingAntlion (509)、FlyingAntlion (581) | indexed | unmapped | open | not-run |
| 45 | 31496–31499 | `AI_045_Golem` (19677) | Golem (245) | indexed | unmapped | open | not-run |
| 46 | 31500–31722 | 内联 | GolemHead (246) | indexed | unmapped | open | not-run |
| 47 | 31723–31726 | `AI_047_GolemFist` (19399) | GolemFistLeft (247)、GolemFistRight (248) | indexed | unmapped | open | not-run |
| 48 | 31727–31968 | 内联 | GolemHeadFree (249) | indexed | unmapped | open | not-run |
| 49 | 31969–32033 | 内联 | AngryNimbus (250) | indexed | unmapped | open | not-run |
| 50 | 32034–32099 | 内联 | FungiSpore (261)、Spore (265) | indexed | unmapped | open | not-run |
| 51 | 32100–32464 | 内联 | Plantera (262) | indexed | unmapped | open | not-run |
| 52 | 32465–32632 | 内联 | PlanterasHook (263) | indexed | unmapped | open | not-run |
| 53 | 32633–32763 | 内联 | PlanterasTentacle (264) | indexed | unmapped | open | not-run |
| 54 | 32764–33054 | 内联 | BrainofCthulhu (266) | indexed | unmapped | open | not-run |
| 55 | 33055–33141 | 内联 | Creeper (267) | indexed | unmapped | open | not-run |
| 56 | 33142–33163 | 内联 | DungeonSpirit (288) | indexed | unmapped | open | not-run |
| 57 | 33164–33472 | 内联 | MourningWood (325)、Everscream (344) | indexed | unmapped | open | not-run |
| 58 | 33473–33631 | 内联 | Pumpking (327) | indexed | unmapped | open | not-run |
| 59 | 33632–33814 | 内联 | PumpkingBlade (328) | indexed | unmapped | open | not-run |
| 60 | 33815–34128 | 内联 | IceQueen (345) | indexed | unmapped | open | not-run |
| 61 | 34129–34381 | 内联 | SantaNK1 (346) | indexed | unmapped | open | not-run |
| 62 | 34382–34434 | 内联 | ElfCopter (347) | indexed | unmapped | open | not-run |
| 63 | 34435–34482 | 内联 | Flocko (352) | indexed | unmapped | open | not-run |
| 64 | 34483–34712 | 内联 | Firefly (355)、LightningBug (358)、Lavafly (654) | indexed | unmapped | open | not-run |
| 65 | 34713–34716 | `AI_065_Butterflies` (45517) | Butterfly (356)、GoldButterfly (444)、HellButterfly (653) | indexed | unmapped | open | not-run |
| 66 | 34717–34812 | 内联 | Worm (357)、TruffleWorm (374)、GoldWorm (448) | indexed | unmapped | open | not-run |
| 67 | 34813–35087 | 内联 | Snail (359)、GlowingSnail (360)、MagmaSnail (655) | indexed | unmapped | open | not-run |
| 68 | 35088–35344 | 内联 | Duck2 (363)、DuckWhite2 (365)、Seagull2 (603) | indexed | unmapped | open | not-run |
| 69 | 35345–35348 | `AI_069_DukeFishron` (49479) | DukeFishron (370) | indexed | unmapped | open | not-run |
| 70 | 35349–35422 | 内联 | DetonatingBubble (371) | indexed | unmapped | open | not-run |
| 71 | 35423–35544 | 内联 | Sharkron (372)、Sharkron2 (373) | indexed | unmapped | open | not-run |
| 72 | 35545–35566 | 内联 | ForceBubble (384) | indexed | unmapped | open | not-run |
| 73 | 35567–35678 | 内联 | MartianTurret (387) | indexed | unmapped | open | not-run |
| 74 | 35679–35971 | 内联 | MartianDrone (388)、SolarCorite (418) | indexed | unmapped | open | not-run |
| 75 | 35972–36548 | 内联 | ScutlixRider (390)、MartianSaucer (392)、MartianSaucerTurret (393) | indexed | unmapped | open | not-run |
| 76 | 36549–37000 | 内联 | MartianSaucerCore (395) | indexed | unmapped | open | not-run |
| 77 | 37001–37425 | 内联 | MoonLordCore (398) | indexed | unmapped | open | not-run |
| 78 | 37426–37938 | 内联 | MoonLordHand (397) | indexed | unmapped | open | not-run |
| 79 | 37939–38355 | 内联 | MoonLordHead (396) | indexed | unmapped | open | not-run |
| 80 | 38356–38450 | 内联 | MartianProbe (399) | indexed | unmapped | open | not-run |
| 81 | 38451–38894 | 内联 | MoonLordFreeEye (400) | indexed | unmapped | open | not-run |
| 82 | 38895–39019 | 内联 | MoonLordLeechBlob (401) | indexed | unmapped | open | not-run |
| 83 | 39020–39189 | 内联 | CultistTablet (437)、CultistDevote (438) | indexed | unmapped | open | not-run |
| 84 | 39190–39193 | `AI_084_LunaticCultist` (65260) | CultistBoss (439)、CultistBossClone (440) | indexed | unmapped | open | not-run |
| 85 | 39194–39487 | 内联 | StardustCellBig (405)、NebulaHeadcrab (421)、DeadlySphere (467) | indexed | unmapped | open | not-run |
| 86 | 39488–39746 | 内联 | ShadowFlameApparition (472)、AncientCultistSquidhead (521) | indexed | unmapped | open | not-run |
| 87 | 39747–40102 | `AI_87_BigMimic_FireStuffCannonBurst` (45420) | BigMimicCorruption (473)、BigMimicCrimson (474)、BigMimicHallow (475) | indexed | unmapped | open | not-run |
| 88 | 40103–40633 | 内联 | Mothron (477) | indexed | unmapped | open | not-run |
| 89 | 40634–40676 | 内联 | MothronEgg (478) | indexed | unmapped | open | not-run |
| 90 | 40677–40912 | 内联 | MothronSpawn (479) | indexed | unmapped | open | not-run |
| 91 | 40913–41097 | 内联 | GraniteFlyer (483) | indexed | unmapped | open | not-run |
| 92 | 41098–41144 | 内联 | TargetDummy (488) | indexed | unmapped | open | not-run |
| 93 | 41145–41255 | 内联 | PirateShip (491) | indexed | unmapped | open | not-run |
| 94 | 41256–41671 | 内联 | LunarTowerVortex (422)、LunarTowerStardust (493)、LunarTowerNebula (507) | indexed | unmapped | open | not-run |
| 95 | 41672–41719 | 内联 | StardustCellSmall (406) | indexed | unmapped | open | not-run |
| 96 | 41720–41762 | 内联 | StardustJellyfishBig (407) | indexed | unmapped | open | not-run |
| 97 | 41763–41912 | `AI_AttemptToFindTeleportSpot` (19092) | NebulaBrain (420) | indexed | unmapped | open | not-run |
| 98 | 41913–42223 | 内联 | 未得到示例 | indexed | unmapped | open | not-run |
| 99 | 42224–42290 | 内联 | SolarGoop (519) | indexed | unmapped | open | not-run |
| 100 | 42291–42369 | 内联 | AncientLight (522) | indexed | unmapped | open | not-run |
| 101 | 42370–42450 | 内联 | AncientDoom (523) | indexed | unmapped | open | not-run |
| 102 | 42451–42847 | 内联 | SandElemental (541) | indexed | unmapped | open | not-run |
| 103 | 42848–43033 | 内联 | SandShark (542)、SandsharkCorrupt (543)、SandsharkCrimson (544) | indexed | unmapped | open | not-run |
| 104 | 43034–43037 | 内联 | DD2AttackerTest (547) | indexed | unmapped | open | not-run |
| 105 | 43038–43289 | 内联 | DD2EterniaCrystal (548) | indexed | unmapped | open | not-run |
| 106 | 43290–43372 | 内联 | DD2LanePortal (549) | indexed | unmapped | open | not-run |
| 107 | 43373–43376 | `AI_107_ImprovedWalkers` (63766) | DD2GoblinT1 (552)、DD2GoblinT2 (553)、DD2GoblinT3 (554) | indexed | unmapped | open | not-run |
| 108 | 43377–43380 | `AI_108_DivingFlyer` (66300) | DD2WyvernT1 (558)、DD2WyvernT2 (559)、DD2WyvernT3 (560) | indexed | unmapped | open | not-run |
| 109 | 43381–43384 | `AI_109_DarkMage` (66705) | DD2DarkMageT1 (564)、DD2DarkMageT3 (565) | indexed | unmapped | open | not-run |
| 110 | 43385–43388 | `AI_110_Betsy` (62670) | DD2Betsy (551) | indexed | unmapped | open | not-run |
| 111 | 43389–43392 | `AI_111_DD2LightningBug` (67105) | DD2LightningBugT3 (578) | indexed | unmapped | open | not-run |
| 112 | 43393–43396 | `AI_112_FairyCritter` (48779) | FairyCritterPink (583)、FairyCritterGreen (584)、FairyCritterBlue (585) | indexed | unmapped | open | not-run |
| 113 | 43397–43400 | `AI_113_WindyBalloon` (48575) | WindyBalloon (594) | indexed | unmapped | open | not-run |
| 114 | 43401–43404 | `AI_114_Dragonflies` (48400) | 未得到示例 | indexed | unmapped | open | not-run |
| 115 | 43405–43408 | `AI_115_LadyBugs` (48262) | LadyBug (604)、GoldLadyBug (605)、Stinkbug (669) | indexed | unmapped | open | not-run |
| 116 | 43409–43412 | `AI_116_WaterStriders` (48198) | WaterStrider (612)、GoldWaterStrider (613) | indexed | unmapped | open | not-run |
| 117 | 43413–43416 | `AI_117_BloodNautilus` (47800) | BloodNautilus (618) | indexed | unmapped | open | not-run |
| 118 | 43417–43420 | `AI_118_Seahorses` (47742) | Seahorse (626)、GoldSeahorse (627) | indexed | unmapped | open | not-run |
| 119 | 43421–43424 | `AI_119_Dandelion` (47652) | Dandelion (628) | indexed | unmapped | open | not-run |
| 120 | 43425–43428 | `AI_120_HallowBoss` (46601) | HallowBoss (636) | indexed | unmapped | open | not-run |
| 121 | 43429–43432 | `AI_121_QueenSlime` (45835) | QueenSlimeBoss (657) | indexed | unmapped | open | not-run |
| 122 | 43433–43436 | `AI_122_PirateGhost` (45459) | PirateGhost (662) | indexed | unmapped | open | not-run |
| 123 | 43437–43440 | `AI_123_Deerclops` (44590) | Deerclops (668) | indexed | unmapped | open | not-run |
| 124 | 43441–43444 | `AI_124_ElderSlimeChest` (44246) | 未得到示例 | indexed | unmapped | open | not-run |
| 125 | 43445–43448 | `AI_125_ClumsySlimeBalloon` (44253) | BoundTownSlimePurple (686) | indexed | unmapped | open | not-run |
| 126 | 43449–43452 | `AI_126_StatueMimic` (44000) | StatueMimic (690) | indexed | unmapped | open | not-run |
| 127 | 43453–43456 | `AI_127_Pal` (43459) | PalworldCattivaDistressed (695)、PalworldFoxsparksDistressed (696) | indexed | unmapped | open | not-run |

## 2026-10-06 D1 夜间与 E1 关系切片

### D1 夜间宿主证据

宿主新增 `--npc-night-probe true`：创建 Guide 后显式提交住房事实，使用
`--world-time-rate 50000` 在真实时钟阶段跨过日落，再由 `ActiveNpcTickPhase` 调用
`RuntimeNpcStore.SyncGuideTask`。报告观察到 `InitialDayTime=true`、`FinalDayTime=false`、
`Task=GuideReturnHome`、`TaskPhase=Running`、`TaskCursor=1`。这只证明夜间任务选择在真实
更新顺序中生效，不证明住房合法性、传送落点、坐具或完整 Guide 来源 AI。

### E1 父子关系生成切片

`RuntimeNpcStore.TrySpawnParentChild` 先取得父/子真实实例，再在子实体上同时挂载
`NpcParentRelationComponent` 与 `EntityRelationState(EntityRelationKind.Parent)`；绑定结果携带
父/子 `EntityReference`、`NpcInstanceId`、兼容槽位和 `AttachedAtTick`。父实体释放前，owner
扫描活动子节点并解绑两种关系组件，随后才终止父实体；子实体仍可独立释放。若子生成或 attach
失败，已创建实体按该切片回退策略立即释放，不发布半成品关系。Combat owner 通过同一关系解析
把子节点受击路由到父节点生命根；父槽复用后旧子引用只作用于已解绑的子节点，父节点致死时
释放整组并把掉落归属到父节点。容量探针填满其余 197 个槽位后，父已创建而子拒绝的路径
恢复原活动数，没有留下半成品父节点。

| Evidence | Command/output | Result |
| --- | --- | --- |
| Simulation host build | `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore --nologo -v:minimal` | exit 0; 0 warnings / 0 errors; artifact under `Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/` |
| D1 night host probe | `dotnet Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll <world> 1 --players 0 --world-time-rate 50000 --npc-night-probe true --report Build/diagnostics/NpcAiRedesign/runs/stage-d-guide-night-20261006/guide-night-1.json` | exit 0; `Succeeded=true`; dusk transition; `GuideReturnHome/Running`; cursor 1 |
| E1 parent relation probe | `dotnet Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll <world> 1 --players 0 --npc-relation-probe true --report Build/diagnostics/NpcAiRedesign/runs/stage-e1-npc-parent-relation-20261006/relation-probe-r3.json` | exit 0; `Succeeded=true`; reference match, parent life-root routing, slot-reuse isolation, local child hit after detach, lethal group release, parent-owned drop, capacity rejection and active-count restoration all true |
| Full host verifier | `& Build/diagnostics/NpcAiRedesign/verify-final.ps1 -WorldPath <world> -OutputDirectory Build/diagnostics/NpcAiRedesign/runs/stage-e1-parent-relation-20261006/full-verifier-20261006-r3` | exit 0; `summary.json` `Succeeded=true`; 35 evidence; Simulation/clock builds and runs 0 warnings / 0 errors |

E1 仍是一个父子生成/解绑切片，不覆盖虫链相邻节、多部位共享生命、召唤者关系、容量不足的
部分链保留策略、奖励/变换、保存加载或联机 authority。回退范围为移除
`RuntimeNpcStore.TrySpawnParentChild`、`RuntimeNpcEntity` 的父关系挂载/快照/解绑入口和两个
宿主 probe；现有普通 NPC 生成与槽位代际校验保持不变。

### E2 三节点关系链切片

E2 新增 `TrySpawnParentChildChain`，以 Zombie 为根、两个 Blue Slime 为相邻后代。binding 返回
链顺序的 `EntityReference`、`NpcInstanceId`、槽位和 attach tick；每个子节点的来源关系组件与
通用关系组件都指向前一节点。Combat owner 从尾节点递归解析根生命 owner，根死亡按后代逆序释放
整链并由根承担掉落。根槽位复用后旧尾节点 projectile target 被拒绝；中段节点单独释放会解绑
尾节点，后续尾段伤害只更新本地生命，不影响根。容量保持两个空槽时，三节点链只创建的前缀被
完整回滚，活动数和填充节点释放后恢复。中段释放是当前通用关系 API 的行为证据，不宣称匹配
Worm 或其他具体家族的断链规则。

| Evidence | Command/output | Result |
| --- | --- | --- |
| E2 chain probe | `dotnet Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll <world> 1 --players 0 --npc-relation-chain-probe true --report Build/diagnostics/NpcAiRedesign/runs/stage-e2-npc-relation-chain-20261006/relation-chain-probe-r2.json` | exit 0；`Succeeded=true`；相邻引用、尾击归根、根死亡整链释放、中段释放尾段解绑/本地伤害、根槽复用旧目标拒绝、容量回滚和活动数恢复全部为 true |
| E2 full host verifier | `& Build/diagnostics/NpcAiRedesign/verify-final.ps1 -WorldPath Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld -OutputDirectory Build/diagnostics/NpcAiRedesign/runs/stage-e2-npc-relation-chain-20261006/full-verifier-20261006-r2` | exit 0；`summary.json` `Succeeded=true`；36 项 evidence；Simulation/clock build 与 run 均 0 warning / 0 error；E1 与 E2 relation probes 通过 |

E2 状态为 implemented / partially verified：只覆盖有限三节点关系链、递归生命根和容量回滚，
不推进 128-style profile rows 到 verified，也不覆盖 Worm、多部位来源规则、节点变换、召唤者
关系、奖励次数、保存加载或网络 authority。回退移除链创建、递归根解析、后代释放和 chain probe，
保留 E1 父子路径与槽位代际校验。

### Blue Slime C1.2 Stone / Cloud Slime gravity branches (2026-10-06)

At `NPC.cs:61592-61603`, the source applies `gravity * 2` to positive Stone Slime vertical
velocity (`ai[1] == 3`) and subtracts `gravity * 0.6` from nonzero Cloud Slime vertical velocity
(`ai[1] == 751`). These variant adjustments precede the `-999` sentinel and common wet / grounded
movement path. `NpcBlueSlimeProfileInput.Gravity` receives the same tick's `NpcGravityResult.Gravity`
from the host owner; the pure profile applies the adjustments before its later movement branches.

The verifier covers falling and rising Stone / Cloud Slimes, the Cloud stationary gate, and both
variant adjustments before the `ai[0] == -999` return with a fixed gravity input. The standard host
smokes and full host regression passed, but the random host seed did not force either variant. Names,
color, other item variants and their effects, reference multi-tick parity, `UnifiedRandom`, and network
authority remain open.

| Evidence | Command/output | Result |
| --- | --- | --- |
| NPC AI verifier | build `Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-restore --nologo -v:minimal`; run with `--no-build --no-restore` | both exit 0; build 0 warnings / 0 errors; PASS with C1.2 cases |
| Simulation build | `Build/diagnostics/NpcAiRedesign/runs/stage-c1-stone-cloud-gravity-20261006/simulation-build.log` | exit 0; 0 warnings / 0 errors; artifact under `Build/bin/` |
| 0/1 player smoke | `.../c1-smoke-120-no-player.json`, `.../c1-smoke-120-player.json` | both succeeded for 120 ticks and updated NPCs on final tick; no-player `ai[0]=-1120, ai[1]=-1`; player `ai[0]=-1106, ai[1]=-1`, two accepted hits; neither run forced Stone / Cloud selection |
| Full host regression | `Build/diagnostics/NpcAiRedesign/runs/stage-c1-stone-cloud-gravity-20261006/full-verifier/summary.json` | exit 0; `Succeeded=true`; 36 evidence; Simulation / clock build and run all 0 warnings / 0 errors |
| Reference fingerprint | SHA-256 of NPC.cs, Main.cs, NPCID.cs | all three match `2026-10-05-npc-ai-reference-index.md` |

### Mother Slime C1.3 finite state slice (2026-10-07)

`NpcMotherSlimeProfile` is gated to `type=16 / netID=16 / aiStyle=1` and covers the declared public
Slime wet, landing, jump and counter transitions. The result explicitly rejects contained-item
generation because the reference `SlimeCanContainItems` set excludes type 16. `RuntimeNpcStore` now
commits this exact profile through the real tick path and records network/target intents. The finite
catalog still calls netID 16 Green Slime and uses different stats and geometry, so the host smoke is
identity-path evidence only; it is not Mother Slime content parity.

| Evidence | Command/output | Result |
| --- | --- | --- |
| NPC verifier | `dotnet build Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-restore --nologo -v:minimal -p:BuildProjectReferences=false`; `dotnet run --project Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-build --no-restore` | exit 0; 0 warning / 0 error; PASS including C1.3 cases |
| Mother Slime host smoke | `Build/diagnostics/NpcAiRedesign/runs/stage-c1-3-mother-slime-20261007/mother-slime-smoke-120.json` | previous Simulation artifact run succeeded for 120 ticks; netID 16 `ai[2]=1`, `ai[0]=-1120`, final tick updated |
| Source fingerprint | NPC.cs, Main.cs, NPCID.cs | hashes match the reference index |

Open work remains catalog identity correction, complete target/Tile/collision inputs, authority and
network sinks, death/split effects, and source golden comparison. See the detailed
[Mother Slime checkpoint](2026-10-07-npc-ai-c1-3-mother-slime-checkpoint.md).

### Eye of Cthulhu F1 opening / first dash slice (2026-10-07)

`NpcEyeOfCthulhuProfile` is gated to `type=4 / netID=4 / aiStyle=4` and covers the opening hover
counter, first dash intent, target loss/death exits and normal/expert life thresholds. The finite
host dispatch, content registration and ordered effect trace are present in source. The NPC component
and verifier build/run pass, but the Simulation host cannot currently compile because shared Items and
Projectile dependencies are inconsistent; host runtime evidence is therefore not claimed.

| Evidence | Command/output | Result |
| --- | --- | --- |
| NPC component/verifier | `dotnet build src/NSSLC/Component/Npc/Terraria.Npc.csproj --no-restore --nologo -v:minimal -p:BuildProjectReferences=false`; `dotnet build Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-restore --nologo -v:minimal -p:BuildProjectReferences=false`; verifier run | all exit 0; 0 warning / 0 error; PASS including `NpcEyeOfCthulhuProfileVerification` |
| Simulation full build | `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore --nologo -v:minimal` | exit 1; shared `Terraria.Items` has 7 `WorldItemReservationSystem`/`WorldItemStateComponent` errors |
| Simulation restricted build | same command with `-p:BuildProjectReferences=false` | exit 1; `ItemMutationRevision`, `ItemEntityRef` and `IProjectileTickAdapter.UpdateProjectile` dependency errors |

The detailed [F1 checkpoint](2026-10-07-npc-ai-f1-eye-of-cthulhu-checkpoint.md) records the exact
rollback scope. Later transformation, servants, full target input, authority/network, presentation,
and reference golden remain open.

### G1 coverage and registration gate (2026-10-07)

`NpcAiProfileCoverageRegistry` and the focused `Terraria.NpcAi.CoverageVerification` executable now
validate the exact 0–127 style inventory, explicit `(type, netID, aiStyle)` identities, negative-ID
variants, duplicate/conflicting registrations, finite handler presence, and verified/open promotion
rules. The gate does not treat fallback behavior as completion.

| Evidence | Command/output | Result |
| --- | --- | --- |
| Focused verifier run | `dotnet run --project Test/Terraria.NpcAi.CoverageVerification/Terraria.NpcAi.CoverageVerification.csproj --no-build --no-restore -- --repo-root D:\\TRbackup\\NLTX --report Build/diagnostics/NpcAiCoverage/coverage-gate-report-root.json` | exit 0; 27 assertions; 128 style rows; 8 mapped profiles; 3 finite handlers; 0 implemented/verified |
| Machine report | `Build/diagnostics/NpcAiCoverage/coverage-gate-report-root.json` | PASS; 125 indexed, 3 mapped style rows, all 128 open |
| Build | normal project build | currently blocked by unrelated `WorldItemStateComponent` errors in `Terraria.Items`; the existing no-build verifier artifact was used for the focused run |

The gate checkpoint is [NPC AI coverage and registration gate](../../system-decomposition/2026-10-07-npc-ai-coverage-gate.md).
Network authority, source golden, save/load, unload cleanup and deletion gates remain open.

### G2 source identity preflight and current coverage baseline (2026-10-07)

The G1 machine report above is its initial snapshot. After F1 registered the Eye of Cthulhu `aiStyle=4`
row as partial/mapped, the focused coverage verifier was rerun against the current ledger. The current
baseline is 128/128 ordered style rows, 124 indexed, 4 mapped, 0 implemented, 0 verified, and 128 open;
the profile registry has 8 mapped profiles, 3 exact finite handlers, and 8 open profiles. The fourth
mapped style row is style 4. The earlier G1 count of 125 indexed / 3 mapped is retained as history and
does not describe this current baseline.

G2 also verifies that the source index, this ledger, and the read-only reference tree identify the same
three source files. This is source provenance evidence; no profile is promoted and no behavior golden
is claimed.

| Evidence | Command/output | Result |
| --- | --- | --- |
| Source identity build | `dotnet build Test/Terraria.NpcAi.SourceIdentityVerification/Terraria.NpcAi.SourceIdentityVerification.csproj --no-restore --nologo -v:minimal` | exit 0; 0 warnings / 0 errors; artifact under `Build/bin/Terraria.NpcAi.SourceIdentityVerification/Debug/net10.0/` |
| Source identity verifier | `dotnet run --project Test/Terraria.NpcAi.SourceIdentityVerification/Terraria.NpcAi.SourceIdentityVerification.csproj --no-build --no-restore` | exit 0; 3 files and 6/6 index/ledger/source SHA-256 comparisons passed; report `Build/diagnostics/NpcAiSourceIdentity/source-identity-report.json` |
| Current coverage verifier | `dotnet run --project Test/Terraria.NpcAi.CoverageVerification/Terraria.NpcAi.CoverageVerification.csproj --no-build --no-restore` | exit 0; 27 assertions; 124 indexed / 4 mapped / 0 implemented / 0 verified; 8 mapped profiles / 3 exact finite handlers / 8 open profiles; report `Build/diagnostics/NpcAiCoverage/coverage-gate-report.json` |

Source-output differential remains `not-run`. Network authority/replication, save/load, unload cleanup,
and old-implementation deletion gates remain open. See the [G2 checkpoint](../../system-decomposition/2026-10-07-npc-ai-coverage-gate.md#G2-source-identity-preflight)
and [execution plan](../../plans/system-decomposition/2026-10-06-npc-ai-system-execution-plan.md#g2固定来源身份前置门禁2026-10-07) for the matching stage summary.

### Main-session current acceptance (2026-10-07)

After the shared integration sources settled, the main session reran the current worktree rather than
relying on an earlier owner binary. The current evidence is kept under
`Build/diagnostics/NpcAiRedesign/current-acceptance-root/` and the complete host summary is
`Build/diagnostics/NpcAiRedesign/runs/current-acceptance-root/simulation-verification/summary.json`.

| Evidence | Result |
| --- | --- |
| Simulation project build | exit 0; 0 warnings / 0 errors; `simulation-build.log` |
| NPC AI verifier build/run | exit 0 / exit 0; 0 warnings / 0 errors; PASS; `npc-verifier-build.log`, `npc-verifier-run.log` |
| Full Simulation verification | `Succeeded=true`; 37 evidence; Simulation, clock, and TileEntity fixture builds all exit 0 with 0 warnings / 0 errors |
| G1 coverage gate | 128 styles; 124 indexed, 4 mapped, 0 implemented, 0 verified; 8 mapped profiles; 3 finite handlers; 27 assertions; 128 open |
| G2 source identity gate | 3 reference files; 6/6 index/ledger/source SHA-256 comparisons passed |

These results are acceptance evidence for the current host and finite slices. They do not promote any
row to `implemented` or `verified`, and do not close source-output differential, authority/replication,
presentation, full NPC lifecycle, save/load/unload, deletion, or the remaining open styles. Historical
root15/root17/root28/root30 failures and passes remain retained above for traceability.

### Coverage completion gate continuation (2026-10-07)

Current registrations now include the exact Mother Slime and Eye finite handlers already present in
`RuntimeNpcStore`. Inventory verification reports 9 mapped/open profiles, 5 finite handlers and 31
assertions; the 128 style rows remain 124 indexed / 4 mapped / 0 implemented / 0 verified. Historical
8/3 snapshots above are retained. No profile or style is promoted.

The new `--require-complete` mode returns exit 2 for the current 128 incomplete styles and 9 incomplete
profiles. A diagnostics fixture marking every ledger row verified/closed/passed still fails with 9
incomplete runtime registrations. Build exit 0, 0 warnings / 0 errors; inventory exit 0; completion
and ledger-only-promotion fixture exits 2 (expected). Reports and exit-code records are under
`Build/diagnostics/NpcAiRedesign/coverage-completion-20261007/`. See the
[completion checkpoint](../../system-decomposition/2026-10-07-npc-ai-coverage-gate.md#g1-coverage-completion-gate-continuation-2026-10-07)
and [verifier usage](../../../Test/Terraria.NpcAi.CoverageVerification/README.md).

### Source differential and task owner continuation (2026-10-07)

The source harness has now captured Eye, Blue Slime and Mother Slime in 32 scenarios / 182 ticks.
The actual original executable and isolated pinned-source build have matching NPC.AI and
AI_001_Slimes IL and matching sampled source outputs. Production profile comparison has 0 velocity
differences, 8 state differences and 11 effects/observable differences; the gate remains failed.
The 8 state differences concern the two slime profiles' ai[2] cooldown. The 11 final netUpdate
differences must also account for TargetClosest helper composition rather than automatically adding
duplicate profile network requests. CallTracker records only the first entry of each method per tick,
so these captures do not establish all invocation counts or complete effect ordering.

The main session independently ran the Windows PowerShell CompareOnly entry against real captures
from `Build/diagnostics/NpcAiReferenceVerification/20261007T035310365Z/`. The script propagated exit 2
and preserved a report with the same 19 differences. Main review evidence is
`Build/diagnostics/NpcAiRedesign/runs/task-reference-host-20261007/source-gate-root-review.json`,
with `source-gate-root-review-exit.json` and the full log beside it. This is a verified failing gate,
not profile parity evidence.

Task termination and reference-bound advance/complete/fail/interrupt are now integrated into the
finite production NPC owner. The main session passed a fresh Simulation build (exit 0, 17 warnings,
0 errors) and seven targeted real-host runs. Guide success at tick 60 completes once; ticks 61/120
do not replay teleport. Blocked return retains Failed/NoPath; invalid housing stays None/Idle.
Stale same-kind task runs, stale entities, borrowed release, real lethal parent-child cleanup,
Reset and repeated Dispose are covered. See the
[host checkpoint](2026-10-07-npc-ai-task-host-checkpoint.md) and
`Build/diagnostics/NpcAiRedesign/runs/task-reference-host-20261007/acceptance-result.json`.

All coverage stages remain unchanged: 9 mapped/open profiles, 5 finite handlers and zero
implemented/verified profiles. Full Simulation regression was not rerun for this task batch.
Source parity, actual type transformation, target/effect cleanup through transformation,
authority/network, persistence/unload and deletion gates remain open.

### Eye night host acceptance continuation (2026-10-07)

The main session accepted eight owner night replays and independently ran four intermediate
second/third dash boundaries. Classic reaches the first dash at tick 601, later dashes at 752/903
and returns to hover at 1053; Expert uses 211, 312/413 and 513. Each run has exit 0 and an unchanged
input world. Dash magnitudes are 6 and approximately 7 respectively. Evidence is tied to the
fingerprinted normal Simulation artifact built with exit 0, 17 warnings / 0 errors, rather than
later unbuilt edits in the shared checkout. See the
[night host acceptance](2026-10-07-npc-ai-f1-night-host-acceptance.md).

These are twelve independent replays ending at selected boundaries, not a complete per-tick source
trace. Full F1 and all coverage stages remain partial/open. The Eye owner continues transformation
and servants; a new independent Luna 6 Max goal owns B2 target inputs and host mapping so that the
current finite target adapter defaults can be replaced with actual owner facts.

### Slime cooldown host acceptance continuation (2026-10-07)

Blue and Mother now implement the source `ai[2] > 1` decrement after the frozen-sentinel early
return. The verifier covers `3→2→1→1` and freezing at 3. Blue's target effect adapter now commits
the selection result's conditional network intent. The main session independently reran four
real-host 1/120-tick cases and the NPC AI verifier; all exit 0 on the fingerprinted normal
Simulation artifact, built with 17 warnings / 0 errors. See the
[cooldown host acceptance](2026-10-07-npc-ai-slime-cooldown-host-acceptance.md).

The source differential has not yet been rerun for this fix. The 8 state / 11 observable differences
remain the last measured historical result, and source-gate rejection semantics are being repaired
after the main session's isolated counterexamples. No coverage stage is promoted. Variant/authority,
death split, physics and save/unload behavior continue in the independent Slime goal.

### Main-session continuation verification (2026-10-07)

The current worktree was rechecked after the Eye servant registration and Guide query additions.
`Terraria.Npc`, `Terraria.NpcAi.ReferenceVerification`, `Terraria.NpcAi.CoverageVerification`, and
`Terraria.NpcAi.GuideProfileVerification` each build with exit 0 and 0 warnings / 0 errors. The
current coverage inventory reports 128 style rows, 124 indexed / 4 mapped / 0 implemented / 0
verified, 10 mapped/open profiles, 6 finite handlers, and 31 assertions; the completion gate remains
closed. Evidence is `Build/diagnostics/NpcAiRedesign/runs/main-progress-build-20261007/coverage-current.json`
and its `coverage-current.log`.

The NPC AI verifier build is currently blocked by one duplicate test helper declaration introduced
while the Eye servant verifier is being extended: `NpcEyeOfCthulhuProfileVerification.cs:321`
reports CS0111 for `RecordingEffectPort.TrySpawnServant`. This is a verifier-source compile issue,
not a production `Terraria.Npc` build failure; the production NPC project still builds cleanly. The
Eye owner has been asked to merge the duplicate and rerun the verifier.

The latest source differential captures use the current cooldown implementation and now compare 32
scenarios / 182 ticks with 0 state differences, 0 velocity differences, and 11 effect/observable
differences. Source trust is valid: the read-only executable and isolated pinned-source build have
matching `NPC.AI` and `AI_001_Slimes` IL and matching sampled source outputs. The gate remains failed
because of the 11 `netUpdate`/effect differences. A 24-scenario / 36-tick Eye control passes with
zero differences; the mutation path returns exit 2 with `invalid-evidence` after provenance mismatch,
so the verifier still needs a fresh-build negative-path checkpoint before this gate can be accepted.
Reports are under `Build/diagnostics/NpcAiReferenceVerification/20261007T035310365Z/rebuilt-profile-compare-only/`.
