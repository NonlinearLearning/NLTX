# Main Server Responsibility Ledger

**Reference root:** D:\TRbackup\Version4物理删除了某些文件

**Reference snapshot:** current directory contents at Task 0 execution time

**Current source evidence:** Build/diagnostics/main-migration/task-0/reference-inventory.json

**Current reference summary:** Build/diagnostics/main-migration/task-0/reference-summary.json

**Classification rule:** A symbol is not considered migrated because a same-named method exists in
Dome. It must have an explicit server authority owner, a state/command/snapshot shape and an
executable verifier. Missing or contradictory reference behavior is Unknown.

## Snapshot facts

| Fact | Observed value |
|---|---|
| Reference exists | yes |
| Reference Git metadata | absent |
| Reference C# files | 980 |
| Terraria directory C# files | 70 |
| Main.cs | 13,996 lines / 379,274 bytes |
| Main.cs SHA-256 | 844A862B4EF863FF848227C1D682E89DAB799EEAD48070ADEA07565EC933254D |
| Inventory timestamp | Task 0 execution; see JSON evidence for per-file timestamps |
| Current worktree | dirty with modified, deleted and untracked files |
| Resolved Simulation target | net10.0 |
| Resolved Simulation output | D:\TRbackup\NLTX\Build\bin\Terraria.Dome.Simulation |
| Resolved Simulation intermediate | D:\TRbackup\NLTX\Build\obj\Terraria.Dome.Simulation |

The reference has no local Git history. A later source recovery must use an external archive, backup
or binary/runtime oracle; it must not pretend that a missing file is an empty implementation.

## Version4 physical deletion audit

The mechanically generated 535-file comparison is recorded in
`docs/migrations/version4-physical-deletion-ledger.csv`, with methodology in the adjacent Markdown
file and the gate result in
`Build/diagnostics/server-ecs-convergence/P4-deletion/20260822-1930/gate-run.md`. The inventory
contains 416 `ClientOnly`, 57 `ServerRelevant`, 3 `ReplacedWithEvidence`, 59 `SharedDefinition`,
and 0 `Unknown` rows. All unresolved rows remain deferred; the completeness gate exits `1` because
57 server-relevant deletions are explicitly deferred. Their removed-source anchors and reasons are
recorded, while the three accepted replacements have explicit ECS owner/state/command/protocol/
verifier evidence.
classification. This ledger does not authorize physical deletion.

## Direct dependency inventory

Full byte counts, line counts, timestamps and SHA-256 values are in the JSON evidence file.

| Reference file | Lines | Server migration relevance |
|---|---:|---|
| Terraria/Main.cs | 13,996 | global host, world state, update loop and client state mixed together |
| Terraria/WorldGen.cs | 73,073 | terrain, Tile changes, structures, liquid and world events |
| Terraria/NPC.cs | 79,302 | NPC lifecycle, AI, combat, drops, progression and replication source |
| Terraria/Projectile.cs | 54,668 | projectile lifecycle, AI, collision, damage and ownership |
| Terraria/Item.cs | 48,768 | item definitions, use, stack, pickup, drops and inventory behavior |
| Terraria/Player.cs | 25,644 | player movement, lifecycle, inventory, buffs and authority assertions |
| Terraria/MessageBuffer.cs | 3,342 | client input decoding and server-side validation paths |
| Terraria/NetMessage.cs | 2,705 | server-to-client state projection and replication paths |
| Terraria/WorldItem.cs | 1,525 | world item lifecycle and pickup state |
| Terraria/Collision.cs | 3,597 | collision calculations used by player/NPC/projectile movement |
| Terraria/Liquid.cs | 1,527 | liquid propagation and Tile/environment effects |
| Terraria/Wiring.cs | 2,749 | wires, switches, pressure plates, doors and mechanisms |
| Terraria/Chest.cs | 1,271 | container state, ownership and item transfer |
| Terraria/Sign.cs | 79 | sign state and interaction |
| Terraria/RemoteClient.cs | 365 | per-session identity and section visibility lifecycle |
| Terraria/Tile.cs | 967 | Tile value shape and mutation helpers |
| Terraria.IO/WorldFile.cs | 3,999 | world metadata, save/load and version compatibility |

## Server state extracted from Main

These values must become explicit world state, definitions or snapshots. They must not remain
process-global static fields in Simulation.

| Legacy state family | Examples in Main.cs | Target owner |
|---|---|---|
| World dimensions | maxTilesX, maxTilesY, maxSectionsX, maxSectionsY, section dimensions | WorldMetadata and WorldGrid |
| Spawn and coordinates | spawnTileX, spawnTileY, leftWorld, rightWorld, topWorld, bottomWorld | WorldMetadata and WorldCoordinateRules |
| Seed flags | drunkWorld, getGoodWorld, remixWorld, zenithWorld, skyblockWorld and related flags | WorldMetadata or WorldProgressionState |
| World mode | game mode, hardMode, world ID, world name, generator version | WorldMetadata and WorldProgressionState |
| Clock | time, dayTime, moonPhase, dayRate, GlobalTimeWrappedHourly | WorldClock and WorldRuleSnapshot |
| Weather | raining, rainTime, maxRaining, wind state, sandstorm state | WorldRuleState and deterministic weather systems |
| Progression | defeated bosses, invasion status, event flags and world unlocks | WorldProgressionState |
| Entity limits | maxPlayers, maxNPCs, maxItems, maxProjectiles, maxChests | explicit bounded configuration |
| Tile storage | tile array, tile flags, section revisions and Tile object state | WorldGrid, WorldTile and section snapshots |
| Entity storage | player, npc, projectile, item arrays and active flags | domain stores and Arch entities |
| World objects | chests, signs, TileEntity values and open/owner state | WorldObjects components, commands and snapshots |
| Randomness | Main.rand and implicit random calls | seeded, domain-scoped deterministic random streams |
| Definition tables | tileSolid, tileSolidTop, projectile flags, buff flags and type tables | domain Definitions/registries, only for supported behavior |
| Thread/main queue | DelayedProcesses, DelayedProcessesInGame and main-thread actions | explicit simulation commands or Server lifecycle queues |

## Server systems and methods

These Main methods are behavior references. Each row requires a focused verifier before its
implementation can be called complete.

| Legacy methods or region | Classification | Target |
|---|---|---|
| Initialize, Initialize_AlmostEverything | SimulationSystem / bootstrap | Simulation definition registration; client setup excluded |
| Initialize_Entities | ServerState bootstrap | domain stores and deterministic bootstrap |
| Initialize_Items | Definition | Items definitions and registry |
| Initialize_TileAndNPCData1/2 | Definition | Tile/Npc definitions consumed by implemented systems |
| DedServ, SetWorld, SetWorldName, autoCreate | ServerState / bootstrap | Server Startup and WorldBootstrap |
| Update, DoUpdate, DoUpdateInWorld | SimulationSystem | SimulationTickSchedule and named phases |
| UpdateServer | SimulationSystem / Server host | DomeServer tick host; no protocol mutation from Simulation |
| UpdateTime, UpdateTime_StartNight, UpdateTime_StartDay | SimulationSystem | WorldClockSystem and WorldProgressionSystem |
| UpdateWeather, StartRain, StopRain, ChangeRain | SimulationSystem | deterministic WorldWeatherSystem |
| StartInvasion, UpdateInvasion | SimulationSystem | explicit progression state machine |
| StartSlimeRain, StopSlimeRain | SimulationSystem | progression event family or explicit unsupported route |
| HandleMeteorFall | SimulationSystem | deterministic world event command |
| GetMoonPhase, IsItDay | Pure calculation | WorldClock or WorldCoordinate/Rule calculation |
| DamageVar, CalculateDamageNPCsTake | Pure calculation | Combat damage calculation |
| QueueMainThreadAction, ConsumeAllMainThreadActions | CommandInput | simulation command queue or Server lifecycle queue |
| UpdateWorldPreparationState | ServerState / bootstrap | WorldBootstrap state machine; verify intended server use |
| GetWorldPathFromName | Projection / Server startup | Server launch path handling; no Simulation file access |
| World save/load calls | Projection | Server Persistence and value-only snapshots |

## Explicitly excluded client responsibilities

The following Main fields and methods are not migration targets for the server ECS:

| Excluded family | Examples |
|---|---|
| Graphics and presentation | graphics, GameViewMatrix, Camera, shaders, background styles, lighting, draw state |
| UI and menus | MenuUI, InGameUI, title state, map UI, inventory UI and menu callbacks |
| Local input | mouseX, mouseY, keyState, mouse buttons, gamepad and local PlayerInput state |
| Asset/runtime loading | Assets, LoadContent, UnloadContent, resource packs and content repositories |
| Window/platform | native menu functions, SetThreadExecutionState, FindWindow, ShowWindow |
| Client update/render | ClientInitialize, Draw, EndDraw, camera pan and scene metrics |
| Client-only effects | dust, particles, ambience rendering, sound, music and visual teleport effects |
| Local diagnostics | FPS timers, render counters, net diagnostics UI and presentation text |

An excluded value may still have a server-side analogue only if a separate reference row proves that
it changes authoritative world state. The client field itself must not be imported.

## Unknown and evidence gaps

These items require a separate decision or a stronger reference before migration:

| Unknown | Why it is not yet accepted |
|---|---|
| Full event progression semantics | Main mixes progression with NPC, UI, audio and network side effects |
| Complete seed flag behavior | Some flags affect WorldGen, NPC selection, loot and client presentation together |
| Exact Main random stream ordering | Main.rand is global and call order changes when client paths are removed |
| Complete entity array semantics | Legacy active slots, index reuse and replication IDs are coupled |
| Full definition table parity | Static initialization includes client-only and unsupported game families |
| Main-thread delayed process semantics | Delayed enumerators may represent client or server work |
| World preparation lifecycle | Current source mixes world loading, UI status and network sequencing |
| Complete weather and event persistence | Save/load fields must be mapped before claiming restart parity |

Until these rows receive a reference/authority/projection/verifier decision, they remain Unknown and
do not contribute to a completion claim.

## Current Dome mapping baseline

| Dome area | Existing owner | Task 0 assessment |
|---|---|---|
| WorldGrid and Tile sections | Terraria.Dome.Simulation.World.WorldGrid | existing partial authority |
| World metadata and seed | WorldMetadata, WorldSeed and snapshots | existing partial authority |
| World rules | WorldRuleSnapshot and DomeSimulation.AdvanceWorldRules | needs clock extraction |
| Tick orchestration | DomeSimulation.Tick | exists but is still a large façade |
| Player state | PlayerHandle, components and PlayerSnapshot | existing narrow slice |
| NPC state | NpcHandle, Npc snapshots and inline simulation logic | narrow slice; store extraction needed |
| Projectile state | Projectile snapshots and collision system | narrow slice |
| Items | ItemDefinitionRegistry, InventoryComponent and WorldItem | narrow slice |
| World objects | chest, sign, door and TileEntity snapshots | partial slice |
| Server startup | ServerLaunchOptions, Program and DomeServer | existing host; bootstrap needs one boundary |
| Protocol projection | TerrariaPacketCodec, Dispatcher and replication assemblers | adapter only; not authority |
| Persistence | DomeStatePersistenceFormat and coordinators | existing value-only slice |

## Task 0 acceptance record

Task 0 is accepted when:

- the JSON inventory exists and contains all direct dependency files;
- every server-relevant Main responsibility has a target classification;
- excluded client responsibilities are listed and cannot be silently reintroduced;
- unknown behavior is explicit;
- the current worktree and reference limitations are recorded in progress.md;
- no C# source or project behavior changed in this task.
