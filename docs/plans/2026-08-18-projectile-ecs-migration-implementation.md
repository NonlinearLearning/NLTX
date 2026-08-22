# Projectile ECS Migration Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Migrate the selected Terraria projectile behavior slice from the legacy `Projectile.cs`
into deterministic ECS components and systems while preserving V1456 projectile replication.

**Architecture:** Use a Projectile-first migration. Shared transform, velocity, collider, health and
target contracts remain ECS capabilities; projectile-specific definition, typed behavior, network
identity, damage, penetration, immunity and lifetime are separate components. Systems run in an
explicit tick pipeline and communicate through commands/events. Protocol DTOs project snapshots and
never access the legacy type.

**Tech Stack:** .NET 10, C#, Arch ECS 2.1, executable verification projects, V1456 packet codecs.

---

### Task 1: Freeze the legacy behavior inventory

**Files:**
- Read: `D:\TRbackup\Version4物理删除了某些文件\Terraria\Projectile.cs`
- Create: `Build/diagnostics/projectile-migration-<timestamp>/behavior-index.json`
- Create: `Build/diagnostics/projectile-migration-<timestamp>/behavior-index.md`
- Test: `Test/Terraria.Dome.Combat.Verification/Program.cs`

**Step 1: Write the failing inventory assertions.**

Assert that the generated inventory records `SetDefaults`, `NewProjectile`, `Update`, `AI`,
`Damage`, `Kill`, all discovered `AI_###` entries, and all protocol-relevant fields.

**Step 2: Run the inventory verifier.**

Run `dotnet run --project Test/Terraria.Dome.Combat.Verification/Terraria.Dome.Combat.Verification.csproj -c Release -p:UseSharedCompilation=false`.
Expected: fail until the inventory fixture and reader are present.

**Step 3: Implement the read-only Roslyn inventory tool.**

Index declarations and references by syntax node and source line. Do not infer coverage from a
string count or modify the source file.

**Step 4: Re-run and store evidence.**

Expected: the report is reproducible and explicitly marks unsupported behavior families.

**Step 5: Commit.**

Commit only the inventory tool, fixture and evidence manifest.

### Task 2: Add definition and network identity components

**Files:**
- Create: `src/Terraria.Dome.Simulation/Components/ProjectileDefinitionComponent.cs`
- Create: `src/Terraria.Dome.Simulation/Components/ProjectileBehaviorComponent.cs`
- Create: `src/Terraria.Dome.Simulation/Components/ProjectileNetworkIdentityComponent.cs`
- Create: `src/Terraria.Dome.Simulation/Components/ProjectilePenetrationComponent.cs`
- Modify: `src/Terraria.Dome.Simulation/Snapshots/ProjectileReplicationSnapshot.cs`
- Test: `Test/Terraria.Dome.Combat.Verification/Program.cs`

**Step 1: Add failing construction assertions.**

Assert that a projectile can carry typed behavior state and a stable `owner + identity + optional
UUID` independently of its Arch entity handle.

**Step 2: Run the focused verifier.**

Expected: compile failure because the components do not exist.

**Step 3: Implement immutable definitions and minimal mutable runtime state.**

Keep public input collections read-only and use named fields for protocol identity. Do not add
`float[] ai` to the component.

**Step 4: Re-run the verifier.**

Expected: construction, snapshot projection and identity stability pass.

**Step 5: Commit.**

### Task 3: Extract spawn, lifetime and replication systems

**Files:**
- Create: `src/Terraria.Dome.Simulation/Projectile/Definitions/ProjectileDefinition.cs`
- Create: `src/Terraria.Dome.Simulation/Projectile/Definitions/ProjectileDefinitionRegistry.cs`
- Create: `src/Terraria.Dome.Simulation/Projectile/Systems/ProjectileSpawnSystem.cs`
- Create: `src/Terraria.Dome.Simulation/Projectile/Systems/ProjectileLifetimeSystem.cs`
- Create: `src/Terraria.Dome.Simulation/Projectile/Systems/ProjectileReplicationSystem.cs`
- Modify: `src/Terraria.Dome.Simulation/Commands/SpawnProjectileCommand.cs`
- Modify: `src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs`
- Test: `Test/Terraria.Dome.Combat.Verification/Program.cs`

**Step 1: Add failing tick assertions.**

Cover spawn, deterministic movement, expiration, explicit despawn and stable replication tombstone.

**Step 2: Run the focused verifier and capture the failure.**

Expected: existing inline spawn/move behavior does not expose typed definition or stable identity.

**Step 3: Move only structural lifecycle work into the new systems.**

Keep command playback at one commit boundary. Preserve existing public simulation entry points until
all callers are migrated.

**Step 4: Run focused combat and completion verifiers.**

Expected: prior projectile generation/despawn checks remain green and new identity assertions pass.

**Step 5: Commit.**

### Task 4: Separate collision candidates from damage resolution

**Files:**
- Create: `src/Terraria.Dome.Simulation/Combat/Events/DamageRequestedEvent.cs`
- Create: `src/Terraria.Dome.Simulation/Projectile/Systems/ProjectileDamageSystem.cs`
- Create: `src/Terraria.Dome.Simulation/Combat/Systems/HitImmunitySystem.cs`
- Modify: `src/Terraria.Dome.Simulation/Projectile/Systems/ProjectileCollisionSystem.cs`
- Modify: `src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs`
- Test: `Test/Terraria.Dome.Combat.Verification/Program.cs`

**Step 1: Add failing assertions for tile collision, one-hit damage, penetration and immunity.**

Include deterministic ordering when multiple projectiles overlap one target in the same tick.

**Step 2: Run the focused verifier.**

Expected: damage is still applied inline or immunity is not represented.

**Step 3: Implement candidate events and commit-time damage.**

The collision system reports candidates only. The damage system applies faction, cooldown,
penetration and amount rules, then enqueues despawn or damage commands.

**Step 4: Run combat, loopback and hardening verifiers.**

Expected: all existing behavior remains green and the new event ordering checks pass.

**Step 5: Commit.**

### Task 5: Migrate the first behavior families

**Files:**
- Create: `src/Terraria.Dome.Simulation/Projectile/Behaviors/LinearProjectileBehavior.cs`
- Create: `src/Terraria.Dome.Simulation/Projectile/Behaviors/GravityProjectileBehavior.cs`
- Create: `src/Terraria.Dome.Simulation/Projectile/Systems/ProjectileBehaviorSystem.cs`
- Create: `Test/Terraria.Dome.Combat.Verification/Fixtures/ProjectileBehaviorFixtures.cs`
- Modify: `Build/diagnostics/.../behavior-index.md`

**Step 1: Add failing fixture assertions for one linear and one gravity behavior.**

Assert typed state transitions, position/velocity output and lifetime behavior against captured
legacy inputs. Do not compare only final position; include intermediate ticks.

**Step 2: Run the fixture verifier.**

Expected: the behavior registry has no implementation for the selected IDs.

**Step 3: Implement the two strategies behind a typed behavior interface.**

Keep `ai[]` mapping outside the behavior implementation. Unsupported behavior IDs must return an
explicit result rather than silently falling back.

**Step 4: Re-run the fixture and broad focused verifiers.**

Record exact output snapshots and unsupported behavior counts.

**Step 5: Commit.**

### Task 6: Implement protocol projection for messages 27 and 29

**Files:**
- Create: `src/Terraria.Dome.Protocol.V1456/Packets/ProjectileSyncPacket.cs`
- Create: `src/Terraria.Dome.Protocol.V1456/Packets/ProjectileStateProjection.cs`
- Modify: `src/Terraria.Dome.Protocol.V1456/Packets/TerrariaPacketCodec.cs`
- Modify: `src/Terraria.Dome.Server/Replication/CombatReplicationAssembler.cs`
- Test: `Test/Terraria.Dome.Combat.Protocol.Verification/Program.cs`

**Step 1: Add failing byte-exact assertions.**

Cover sparse flags for `ai0..ai2`, banner, damage, knockback, original damage, UUID and exact
three-byte client `KillProjectile` payload.

**Step 2: Run the protocol verifier.**

Expected: the new DTO/projection does not exist or differs from the recorded payload.

**Step 3: Implement DTO projection and codec boundaries.**

The codec receives only the DTO. Validate owner/identity before routing message 29; do not mutate
authoritative state for an unowned client termination.

**Step 4: Re-run protocol and full-client bootstrap verifiers.**

Expected: exact payload assertions and owner-forgery rejection pass.

**Step 5: Commit.**

### Task 7: Prove clean integration and publish the migration gate

**Files:**
- Modify: `progress.md`
- Create: `Build/diagnostics/projectile-migration-<timestamp>/verification.json`
- Create: `docs/plans/2026-08-18-projectile-ecs-migration-evidence.md`

**Step 1: Build the Simulation project serially.**

Run `dotnet build src/Terraria.Dome.Simulation/Terraria.Dome.Simulation.csproj -c Release -p:UseSharedCompilation=false`.
Record exit code, warnings, and actual `Build/bin` output.

**Step 2: Run all affected verifiers serially.**

Run combat, combat protocol, completion, loopback and hardening verification projects. Record each
exit code; a green build alone is not a rewrite-safety proof.

**Step 3: Scan for forbidden legacy dependencies.**

Search the Simulation project for `Terraria.Projectile`, `Main.`, `NetMessage`, `SoundEngine`,
`AI(` and `Update(`. Any match requires triage before the phase is accepted.

**Step 4: Write evidence and residual scope.**

Include behavior coverage, packet fixtures, source paths, commands, exit statuses and unsupported
families. Mark the result partial until all planned behavior families and protocol gates pass.

**Step 5: Commit the evidence and gate decision.**

Do not physically delete the Version4 source in this task. Schedule deletion only as a separate
change after an explicit review of the gate report.

