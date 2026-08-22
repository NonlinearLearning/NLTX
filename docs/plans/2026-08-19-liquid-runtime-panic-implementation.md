# Liquid Runtime Panic Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Add a deterministic, bounded ordinary-runtime Panic policy to the Simulation Liquid
queue without importing generation-time Liquid behavior.

**Architecture:** An immutable `LiquidPanicPolicy` supplies high-water, duration, Panic budget,
and recovery values to `LiquidWorldStateComponent`. `LiquidPropagationSystem` observes queue
pressure and drains with the state's current budget; all existing propagation and commit boundaries
remain intact.

**Tech Stack:** C# / .NET 10, existing console verifier, deterministic value types, serial `dotnet`
commands with `-p:UseSharedCompilation=false`.

---

### Task 1: Write the RED Panic-state verifier

**Files:** Modify `Test/Terraria.Dome.Liquid.Verification/Program.cs`.

Add a small explicit Panic policy fixture with a queue larger than the normal budget. Assert that
the first high-water advance remains `Normal` and drains the normal count, the second consecutive
high-water advance becomes `Panic` and drains the configured Panic count in stable order, and a
low queue observation returns the state to `Normal` without clearing the remaining queue.

Run:

```powershell
dotnet run --project .\Test\Terraria.Dome.Liquid.Verification\Terraria.Dome.Liquid.Verification.csproj -p:UseSharedCompilation=false
```

Expected: compilation fails because the policy and Panic-state API do not exist.

### Task 2: Add the immutable Panic policy and state transitions

**Files:** Create `src/Terraria.Dome.Simulation/Liquid/Components/LiquidPanicPolicy.cs`.
Modify `src/Terraria.Dome.Simulation/Liquid/Components/LiquidWorldStateComponent.cs`.

Validate all policy values. Add source-derived default duration, a bounded default Panic drain
budget, high-water observation, and recovery transition methods. Preserve the existing constructor
shape with an optional policy parameter.

Run the focused verifier and confirm the state assertions still fail only because propagation does
not consume the policy yet.

### Task 3: Apply the state budget at the propagation boundary

**Files:** Modify `src/Terraria.Dome.Simulation/Liquid/Systems/LiquidPropagationSystem.cs`.

Before draining, observe the queue length through state. Drain with the state's effective normal or
Panic budget. Do not alter source validation, neighbor order, retry policy, Tile contact commands,
merge evaluation, commits, or replication.

Run the focused verifier and confirm all Panic and existing Liquid assertions pass.

### Task 4: Record scope and run regressions

**Files:** Modify `docs/research/2026-08-18-wiring-liquid-chest-coverage.md`,
`docs/research/2026-08-19-wiring-liquid-chest-member-coverage.md`, and add
`Build/diagnostics/2026-08-19-liquid-runtime-panic-evidence.md`.

Record the configuration values, the unchanged generation-time exclusions, exact commands, and
exit statuses. Run the Simulation build, Liquid focused/loopback, Wiring/Liquid/Chest loopback,
and Completion verifier. Run `git diff --check`.
