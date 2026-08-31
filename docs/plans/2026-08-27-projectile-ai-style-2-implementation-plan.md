# Legacy AiStyle 2 Implementation Plan


**Goal:** Implement the source-backed generic legacy `aiStyle = 2` behavior for projectile type 3.

**Architecture:** Add a third behavior to the existing behavior registry and explicit wire-state
projection. Replace only the old type-3 fixture with its source definition; retain NPC hostile
coverage through a locally constructed test definition.

**Tech Stack:** C# `net10.0`, Arch ECS, focused console verifiers.

---

### Task 1: Lock the source behavior with a failing verifier

**Files:**
- Modify: `Test/Terraria.Dome.Combat.Verification/Program.cs`

1. Add direct behavior assertions for the 20-tick delayed-gravity threshold, horizontal drag,
   vertical cap, and behavior state timer.
2. Run `dotnet build` and `dotnet run` for the combat verifier with
   `-p:UseSharedCompilation=false`; confirm the new assertion fails because behavior ID 3 is
   unknown.

### Task 2: Implement behavior and typed state projection

**Files:**
- Create: `src/Terraria.Dome.Simulation/Projectile/Behaviors/LegacyAiStyle2ProjectileBehavior.cs`
- Modify: `src/Terraria.Dome.Simulation/Projectile/Systems/ProjectileBehaviorSystem.cs`
- Modify: `src/Terraria.Dome.Simulation/Projectile/Systems/ProjectileBehaviorStateProjection.cs`

1. Register behavior ID 3 and implement the source timing, acceleration, drag, cap, finite-input
   rejection, transform update, timer state, and phase update.
2. Project timer and phase through typed replication state.
3. Re-run the combat verifier until the new assertions pass.

### Task 3: Replace the default type-3 fixture with the source definition

**Files:**
- Modify: `src/Terraria.Dome.Simulation/Projectile/Definitions/ProjectileDefinitionRegistry.cs`
- Modify: `Test/Terraria.Dome.Combat.Verification/Program.cs`
- Modify: `Test/Terraria.Dome.Npc.Verification/Program.cs`

1. Assert type 3 uses source dimensions, friendly faction, penetration, and behavior ID 3.
2. Move NPC hostile firing coverage to an explicit hostile fixture registry.
3. Run the combat and NPC verifiers; both must pass.

### Task 4: Record the narrow completion boundary

**Files:**
- Modify: `docs/research/2026-08-24-projectile-lifecycle-gap-and-implementation-design.md`
- Create: `.agent-workplace/docs/task/projectile-b49-legacy-ai-style-2-core.md`
- Modify: `.agent-workplace/state/projectile-lifecycle-task.json`

1. Record generic type-3 migration as complete.
2. Keep type-specific `aiStyle = 2` branches explicitly deferred to later batches.
