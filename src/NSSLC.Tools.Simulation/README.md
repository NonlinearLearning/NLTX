# Headless finite simulation

Build the affected simulation project graph in its isolated fixture output:

```powershell
dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore -p:FixtureHostBuild=true
```

Run a published WorldFile 319 world for a fixed number of ticks using that build:

```powershell
dotnet Build/bin/FixtureHost/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll <world.wld> <ticks> [--players <count>] [--seed <integer>] [--input-script <script.json>] [--spawn-npc <net-id>]... [--world-time-rate <ticks-per-step>] [--npc-slot-probe <true|false>] [--npc-relation-probe <true|false>] [--npc-relation-chain-probe <true|false>] [--npc-night-probe <true|false>] [--npc-home-return-probe <success|blocked|invalid>] [--npc-housing-revalidation-probe <valid|invalid>] [--tile-entity-removal-probe <true|false>] [--npc-despawn-probe <true|false>] [--npc-despawn-probe-net-id <id>] [--world-item-probe <expired|physics|partial-pickup|full-inventory>] [--save <world.wld>] [--autosave-interval <ticks>] [--cancel-after-ms <milliseconds>] [--cancel-after-ticks <ticks>] [--switch-world <world.wld>]... [--report <report.json>]
```

`--autosave-interval` must be a positive tick count and requires `--save`. The host captures and saves the active world snapshot after each matching committed tick. The final save reuses the same snapshot coordinator and is skipped when the latest committed tick was already autosaved. Reports include the interval, successful autosave count, and last autosaved tick.

When Ctrl+C is received during simulation, the host finishes the current committed tick, saves the latest snapshot when `--save` is set, writes a report with `Canceled: true`, and exits with code 130.

`--cancel-after-ms` schedules cooperative cancellation after argument validation. It can exercise cancellation during world loading or a finite run without relying on a manual Ctrl+C timing window. A cancellation before loading still writes a failure report with tick 0.

`--cancel-after-ticks <ticks>` stops after that many committed ticks, writes the report, and saves the latest owner snapshot when `--save` is set. The tick must be below the finite-run tick limit and cannot be combined with `--cancel-after-ms`.

Each repeated `--switch-world <world.wld>` reloads and publishes a fresh session in the same process after the finite run stops, creates new runtime owners, and commits one tick before the next switch. The host writes switch results to `<report>.switch.json`; the report includes prior/new world ids, session identity, publication, player/NPC counts, and the committed phase order.

`--npc-slot-probe true` fills the runtime NPC owner to its 200-slot capacity, confirms an overflow spawn is rejected, releases and reallocates one slot, and checks that stale roots, handles, components, Projectile targets, and TrainingDummy bindings cannot affect the replacement. Probe entities are removed before simulation starts. It requires at least one free NPC slot when the probe begins.

`--npc-relation-probe true` creates a Zombie parent and Blue Slime child, attaches both the source `NpcParentRelationComponent` and generic `EntityRelationState`, routes a child hit through the parent's life root, verifies safe parent-slot reuse and local child damage after detachment, checks lethal group release and parent-owned drop selection, and fills the remaining slots to prove a rejected child spawn does not leak its parent. `--npc-night-probe true` gives a generated Guide a home and, with a high `--world-time-rate`, verifies the dusk transition enters `GuideReturnHome` on the real NPC tick path. `--npc-home-return-probe success|blocked|invalid` runs the Guide return-home slice at cursor 60: `success` checks the bounded `0,-1,+1` home-floor candidate order and records a teleport, `blocked` records the all-blocked `NoPath` failure, and `invalid` now verifies D3 housing invalidation before task selection (`Task=None/Idle`); the historical pre-D3 invalid-effect result remains in the D2 evidence. `--npc-housing-revalidation-probe valid|invalid` runs the town housing owner check before Guide task selection: `valid` synchronizes a registry that was deliberately homeless, while `invalid` clears the ECS home, marks both owners homeless, requests a network update, and proves night task selection stays `None/Idle`. Housing probes require `--players 0`; the home-return and housing probes report task/effect snapshots.
`--npc-relation-chain-probe true` creates a Zombie root with two Blue Slime descendants, verifies stable adjacent references and attach ticks, routes a tail hit through the root life owner, releases the complete chain on root death, rejects a stale tail target after same-slot root reuse, releases the middle node and confirms the tail detaches and returns to local life, and fills the remaining slots to prove a partially created chain is fully rolled back. It is a relation-chain slice and does not claim Worm or other multi-part source parity. `--npc-night-probe true` gives a generated Guide a home and, with a high `--world-time-rate`, verifies the dusk transition enters `GuideReturnHome` on the real NPC tick path. `--npc-home-return-probe success|blocked|invalid` runs the Guide return-home slice at cursor 60: `success` checks the bounded `0,-1,+1` home-floor candidate order and records a teleport, `blocked` records the all-blocked `NoPath` failure, and `invalid` now verifies D3 housing invalidation before task selection (`Task=None/Idle`); the historical pre-D3 invalid-effect result remains in the D2 evidence. `--npc-housing-revalidation-probe valid|invalid` runs the town housing owner check before Guide task selection: `valid` synchronizes a registry that was deliberately homeless, while `invalid` clears the ECS home, marks both owners homeless, requests a network update, and proves night task selection stays `None/Idle`. Housing probes require `--players 0`; the home-return and housing probes report task/effect snapshots.

`--tile-entity-removal-probe true` requires at least three ticks. It creates a Logic Sensor and TrainingDummy TileEntity, checks the TrainingDummy NPC binding after the first tick, invalidates both anchors, and reports whether the next update removed both TileEntities and released the dummy NPC.

`--npc-despawn-probe true` creates a naturally spawned NPC and runs it without players for at least 300 ticks. `--npc-despawn-probe-net-id <id>` selects any NPC in the support manifest; omitting it defaults to Blue Slime. Blue Slime, Demon Eye, Zombie, Green Slime, and Servant of Cthulhu use the 3,000-pixel / 300-tick living-player-distance policy. Guide, Old Man, Eye of Cthulhu, and Target Dummy are persistent under natural despawn. The report includes the selected type, policy, and whether its runtime slot was released.

`--npc-eye-transform-probe` runs one Expert transformation slice after configuring the Eye's initial AI slots: `expert-summon` checks the 20-tick Servant trigger and same-tick update; `phase-one-boundary` checks the first transformation transition and effects; `phase-two-boundary` checks the transition into phase three; `capacity-rejection` fills all 200 NPC slots and requires an explicit rejected spawn; `next-tick` requires `--spawn-npc 3` before `--spawn-npc 4` and checks that a Servant allocated into the released lower slot begins updating on tick 2. Each probe requires `--npc-eye-scenario expert-night`, one local player, one requested Eye, and exactly one tick (two for `next-tick`).

The pressure-plate probe places an unsupported Timer in the connected circuit and verifies that its tile type ID appears in the unsupported-device report while the actuator effect still commits.

`--chest-item-probe set` writes a Gel stack into slot 0 of the first loaded chest and requires `--save`; `--chest-item-probe inspect` reports that slot without mutating it. The verification runs `set` in one process, saves the world, then uses `inspect` in a fresh process to check the persisted container state.

`--world-item-probe physics` drops one Gel item 160 pixels above the world spawn and runs it without players for at least 300 ticks. The report includes its initial position and final position, velocity, and remaining lifetime so the verification can confirm gravity, tile collision, and lifetime advancement.

Without an input script, local players use the built-in deterministic movement and bow-use sequence. With a script, unspecified player/tick pairs are neutral. A frame selects horizontal movement (`-1` left, `0` idle, `1` right), jump, and item use for one player on one 1-based tick:

```json
{
  "players": [
    { "playerSlot": 0, "magicQuiver": true }
  ],
  "frames": [
    { "tick": 1, "playerSlot": 0, "horizontal": 1, "jump": false, "useItem": false },
    { "tick": 2, "playerSlot": 0, "horizontal": 1, "jump": true, "useItem": false },
    { "tick": 30, "playerSlot": 0, "horizontal": 0, "jump": false, "useItem": true }
  ]
}
```

The optional `players` array configures persistent ranged-accessory capabilities; the example enables Magic Quiver for player 0. The loader rejects duplicate frames or player configurations, unsupported player slots or directions, unknown JSON fields, and entries targeting a player that was not created. The seed controls the host's deterministic random source.

## Simulation support

The current NPC catalog supports Blue Slime (1), Demon Eye (2), Zombie (3), Eye of Cthulhu (4), Servant of Cthulhu (5), Green Slime (16), Guide (22), Old Man (37), and Target Dummy (488). Both slimes jump toward nearby players, Zombie uses the exact type=3 Fighter profile for finite ground movement, obstacle-jump, and closed-door effect slices, and Demon Eye alternates between hovering and diving. Eye of Cthulhu supports its night hover and dash loop, two transformation stages, and Expert Servant creation. Servants use the type=5, netID=5, aiStyle=5 steering slice. These NPC profiles remain finite migration slices; they do not claim complete reference AI parity. Guide follows a limited daytime patrol, revalidates town housing before task selection, enters a cross-tick `GuideReturnHome` task at night only when the revalidated home is valid, and has a bounded cursor-60 home-teleport slice with success and `NoPath` failure probes; Old Man remains stationary. Full town schedules, housing allocation, Fighter helper exceptions, seeded door-map coverage, combat actions, seating, dialogue, and pathfinding are not implemented. Each supported NPC has an explicit natural-despawn policy: Blue Slime, Demon Eye, Zombie, Green Slime, and Servant of Cthulhu release their runtime slots after 300 ticks with no living player within 3,000 pixels; Guide, Old Man, Eye of Cthulhu, and Target Dummy remain allocated.

NPC AI now consumes explicit target and environment snapshots through `NpcAiSystem`; the host commits its decisions before applying gravity and tile collision. The design and declared coverage are recorded in [NPC AI redesign](../../docs/system-decomposition/2026-10-05-npc-ai-system-redesign.md). The default registry accepts only the seven explicit NPC net IDs and rejects other content even when it shares an AI style. The [128-style source index](../../docs/system-decomposition/2026-10-05-npc-ai-reference-index.md) and [integration evidence](../../docs/system-decomposition/2026-10-05-npc-ai-redesign-verification.md) distinguish these finite rules from full reference AI parity, including the catalog identity/style differences that must be resolved during migration.

The item support set is Gel (23), Wooden Bow (39), and Wooden Arrow (40); the only supported projectile is ordinary arrow (1). Blue Slime, Green Slime, Demon Eye, Zombie, Eye of Cthulhu, and Servant of Cthulhu have finite simulation behavior. Unsupported content definitions are rejected when the content catalog is built.

The world clock advances according to `--world-time-rate`, including rain, Slime Rain, sandstorm, wind, day/night boundaries, and the supported event countdown fields in the run report. Event wave scheduling, boss/event gameplay effects, and full seasonal or invasion simulation are not implemented.

Training dummy (TileEntity type 0) and logic sensor (type 2) have active tick handlers. The host rejects other TileEntity kinds and unsupported logic checks during startup, before the first tick. Logic sensor rising edges and player presses on registered pressure plates (tile types 135 and 428) traverse connected wire colors and toggle actuated tiles through the session tile owner. Per-player pressed state lives in the session owner and is projected to the legacy pressure-plate helper; release clears that state without retriggering the wire. Other wired device effects and TileEntity behaviors are not implemented. When a triggered network contains a recognized unsupported wired device, the run report lists its tile type IDs in `RecognizedUnsupportedWiredDeviceTileTypes`; this recognized set does not imply support for unlisted effects.

World item reports include each active drop's item type, stack, position, velocity, and remaining lifetime. A living player can collect a drop when it overlaps the player hitbox or comes within the host's 48-pixel pickup range.

Runtime NPC reports include `Ai0..Ai3` and `LocalAi0..LocalAi3` snapshots alongside movement and effect state. These fields expose the finite host's last committed NPC AI slots for diagnostics; they do not imply reference parity.
