# 全量 ECS 组件拆分实施计划

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**目标：** 按已批准的全量 ECS 组件拆分设计，收敛 Simulation 中的重复/混合状态，补齐 Player、NPC、Projectile、Input/Movement、Item 和关系边界，并保持现有行为与协议兼容。

**架构：** 采用领域优先的 Arch ECS 组合模型。共享 Entity 组件只承载真正同质的空间、液体和实体身份状态；Combat、Projectile、Player、NPC、Items 和 WorldObjects 各自拥有领域状态与系统。迁移通过短期单向 Adapter 完成，最终每个权威字段只有一个写者和一个明确归属。

**技术栈：** C# / .NET 10、Arch ECS、仓库现有 Simulation 命令/事件/快照模型、`Build/Tools/Invoke-SerialDotnet.ps1` 串行构建与验证脚本。

---

## 执行前固定规则

- 在每个编译命令前检查活动的 `dotnet.exe` 和 `csc.exe`；若存在其他所有者则等待，不得终止进程。
- 所有 `restore`、`build`、`test` 或 `run` 都从仓库根目录通过 `Build/Tools/Invoke-SerialDotnet.ps1` 执行，并带上 `-m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false`。
- 只构建受影响的 `dome/src/Terraria.Dome.Simulation/Terraria.Dome.Simulation.csproj`，验证程序使用 `--no-build --no-restore`。
- 不触碰当前工作区与本计划无关的既有删除/修改；每个阶段只提交该阶段的文件。
- 组件、系统、查询和验证文件遵守 `架构设计/ECS文件组织设计约束.md`、`约束/Google-CSharp-Style-Guide-约束.md` 和 `约束/非函数式编码副作用隔离规范.md`。

## Task 1: 建立逐字段归属和重复模型基线

**Files:**

- Create: `docs/plans/2026-09-04-component-field-inventory.md`
- Inspect: `docs/组件设计报告.md`
- Inspect: `D:/TRbackup/Version4/Terraria/Entity.cs`
- Inspect: `D:/TRbackup/Version4/Terraria/Player.cs`
- Inspect: `D:/TRbackup/Version4/Terraria/NPC.cs`
- Inspect: `D:/TRbackup/Version4/Terraria/Projectile.cs`
- Inspect: `D:/TRbackup/Version4/Terraria/Item.cs`
- Inspect: `D:/TRbackup/Version4/Terraria/Chest.cs`
- Inspect: `dome/src/Terraria.Dome.Simulation/Components`

**Step 1: 记录当前状态**

使用 `rg` 列出每个候选字段的声明、写入点、读取点、spawn/commit 初始化点、快照/协议投影点和生命周期清理点。把已满足设计的组件标记为“无需新建”。

**Step 2: 编写基线清单**

在 `docs/plans/2026-09-04-component-field-inventory.md` 中为每个字段记录：权威组件、权威写者、只读消费者、作用域、迁移动作（保留/移动/拆分/删除）以及对应验证程序。

**Step 3: 检查重复和双写**

确认没有 `TransformComponent`/`FacingComponent` 等旧公共类型；列出 Player/NPC/Projectile/Items 中仍然将两个语义写入同一组件或同一字段的调用点。

**Step 4: 提交基线**

Run: `git diff --check -- docs/plans/2026-09-04-component-field-inventory.md`

Expected: 无 whitespace 错误；随后提交：

```text
git add docs/plans/2026-09-04-component-field-inventory.md
git commit -m "docs: inventory component field ownership"
```

## Task 2: 收敛 Player/NPC Combat 组件

**Files:**

- Modify: `dome/src/Terraria.Dome.Simulation/Components/Entity/HealthComponent.cs`
- Modify: `dome/src/Terraria.Dome.Simulation/Combat/Components/DefenseComponent.cs`
- Modify: `dome/src/Terraria.Dome.Simulation/Combat/Components/ImmunityComponent.cs`
- Modify: `dome/src/Terraria.Dome.Simulation/Combat/Components/HitImmunityComponent.cs`
- Modify: `dome/src/Terraria.Dome.Simulation/Combat/Systems/DamageResolutionSystem.cs`
- Modify: `dome/src/Terraria.Dome.Simulation/Combat/Systems/ImmunitySystem.cs`
- Modify: `dome/src/Terraria.Dome.Simulation/Combat/Systems/HitImmunitySystem.cs`
- Modify: `dome/src/Terraria.Dome.Simulation/Combat/Systems/PlayerVitalRegenSystem.cs`
- Modify: `dome/src/Terraria.Dome.Simulation/Player/Components/PlayerDefenseStateComponent.cs`
- Modify: `dome/src/Terraria.Dome.Simulation/Npc/Components/NpcCombatStateComponent.cs`
- Modify: `dome/src/Terraria.Dome.Simulation/StatusEffects/Components/StatusEffectsComponent.cs`
- Modify: `dome/src/Terraria.Dome.Simulation/StatusEffects/Components/TimedStatusEffect.cs`
- Test: `dome/Test/Terraria.Dome.Combat.Verification/Program.cs`
- Test: `dome/Test/Terraria.Dome.PlayerLifecycle.Verification/Program.cs`
- Test: `dome/Test/Terraria.Dome.Npc.Verification/Program.cs`

**Step 1: Add failing invariant checks**

Extend focused verification with cases for `0 <= Current <= Maximum`, defense changes not resetting health, immunity rejecting damage before defense calculation, and status collection expiration without mutating unrelated health state.

**Step 2: Run focused verifier**

Run through the serial script:

```text
pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 test .\dome\Test\Terraria.Dome.Combat.Verification\Terraria.Dome.Combat.Verification.csproj -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false --no-restore
```

Expected: the new invariant checks fail until the boundary is implemented.

**Step 3: Implement the smallest boundary**

Move only combat-owned fields into the combat components. Keep Player and NPC lifecycle components as the sole owners of death/respawn/despawn transitions. Adapt `DamageResolutionSystem` to read Health, Defense and Immunity independently; do not add a generic MobState or GenericResource component.

**Step 4: Migrate spawn and snapshot consumers**

Update Player and NPC spawn/commit, death, respawn, replication and snapshot paths so each entity receives only the components required by its domain. Preserve existing command and event payloads unless a typed adapter is required.

**Step 5: Run verifiers**

Run the Combat, PlayerLifecycle and NPC verification projects serially with `--no-build --no-restore` after one Simulation build. Expected: all pass and no duplicate health/defense authority remains.

**Step 6: Commit**

```text
git add dome/src/Terraria.Dome.Simulation/Combat dome/src/Terraria.Dome.Simulation/Components/Entity/HealthComponent.cs dome/src/Terraria.Dome.Simulation/Player dome/src/Terraria.Dome.Simulation/Npc dome/src/Terraria.Dome.Simulation/StatusEffects dome/Test/Terraria.Dome.Combat.Verification dome/Test/Terraria.Dome.PlayerLifecycle.Verification dome/Test/Terraria.Dome.Npc.Verification
git commit -m "refactor: separate player and npc combat state"
```

## Task 3: 收敛 Projectile 定义、行为和生命周期

**Files:**

- Modify: `dome/src/Terraria.Dome.Simulation/Components/Projectile/ProjectileDefinitionComponent.cs`
- Modify: `dome/src/Terraria.Dome.Simulation/Components/AI/ProjectileBehaviorComponent.cs`
- Modify: `dome/src/Terraria.Dome.Simulation/Components/Projectile/ProjectileLifetimeComponent.cs`
- Modify: `dome/src/Terraria.Dome.Simulation/Components/Projectile/ProjectileOwnerComponent.cs`
- Modify: `dome/src/Terraria.Dome.Simulation/Components/Projectile/ProjectileDamageComponent.cs`
- Modify: `dome/src/Terraria.Dome.Simulation/Components/Projectile/ProjectilePenetrationComponent.cs`
- Modify: `dome/src/Terraria.Dome.Simulation/Components/Projectile/ProjectileDirectionComponent.cs`
- Modify: `dome/src/Terraria.Dome.Simulation/Projectile/Systems/ProjectileSpawnSystem.cs`
- Modify: `dome/src/Terraria.Dome.Simulation/Projectile/Systems/NpcProjectileSpawnSystem.cs`
- Modify: `dome/src/Terraria.Dome.Simulation/Projectile/Systems/ProjectileBehaviorSystem.cs`
- Modify: `dome/src/Terraria.Dome.Simulation/Projectile/Systems/ProjectileCollisionSystem.cs`
- Modify: `dome/src/Terraria.Dome.Simulation/Projectile/Systems/ProjectileReplicationSystem.cs`
- Test: `dome/Test/Terraria.Dome.Combat.Verification/Program.cs`
- Test: `dome/Test/Terraria.Dome.ProjectileHook.Verification/Program.cs`
- Test: `dome/Test/Terraria.Dome.ProjectileHostile.Verification/Program.cs`

**Step 1: Add component-isolation checks**

Verify that changing lifetime does not mutate behavior state, changing owner does not mutate damage, definition values are immutable after spawn, and penetration depletion only affects the penetration component.

**Step 2: Migrate spawn initialization**

Ensure both player- and NPC-originated projectile spawn paths initialize definition, behavior, lifetime, owner, damage, penetration and direction independently. Keep protocol identity/update components separate.

**Step 3: Migrate tick and collision writers**

Make behavior systems write only behavior state, lifetime systems write only lifetime, collision systems write damage/penetration outcomes through explicit commands or results, and owner-hit checks resolve typed owner references without changing targeting semantics.

**Step 4: Preserve snapshots and compatibility**

Keep existing `ProjectileReplicationSnapshot` fields and protocol identity values stable; use adapters when a snapshot needs values from more than one component. Do not add EntityUuid to Terraria packets.

**Step 5: Run verifiers and commit**

Build Simulation serially, run the three projectile verifiers plus Combat with `--no-build --no-restore`, confirm `ProjectileDamageComponent` is never used as target health, then commit:

```text
git add dome/src/Terraria.Dome.Simulation/Components/AI dome/src/Terraria.Dome.Simulation/Components/Projectile dome/src/Terraria.Dome.Simulation/Projectile dome/Test/Terraria.Dome.Combat.Verification dome/Test/Terraria.Dome.ProjectileHook.Verification dome/Test/Terraria.Dome.ProjectileHostile.Verification
git commit -m "refactor: isolate projectile definition behavior and lifetime"
```

## Task 4: 收敛 InputIntent、Movement、Physics 和方向

**Files:**

- Modify: `src/Share/Entity/Components/LocationComponent.cs`
- Modify: `src/Share/Entity/Components/VelocityComponent.cs`
- Modify: `src/Share/Entity/Components/DirectionComponent.cs`
- Modify: `src/Share/Entity/Queries/EntityGeometryQuery.cs`
- Modify: `src/Share/Entity/Queries/EntitySpatialQuery.cs`
- Modify: `dome/src/Terraria.Dome.Simulation/Components/Player/ControlInputComponent.cs`
- Modify: `dome/src/Terraria.Dome.Simulation/Components/Player/PlayerControlStateComponent.cs`
- Modify: `dome/src/Terraria.Dome.Simulation/Player/Systems/PlayerControlSystem.cs`
- Modify: `dome/src/Terraria.Dome.Simulation/Player/Systems/PlayerInputApplySystem.cs`
- Modify: `dome/src/Terraria.Dome.Simulation/Movement/Systems/MovementSystem.cs`
- Modify: `dome/src/Terraria.Dome.Simulation/Physics/Systems/GroundCollisionSystem.cs`
- Modify: `dome/src/Terraria.Dome.Simulation/Physics/Systems/TileCollisionSystem.cs`
- Modify: `dome/src/Terraria.Dome.Simulation/Npc/Systems/NpcMovementIntentSystem.cs`
- Test: `dome/Test/Terraria.Dome.PlayerPhysics.Verification/Program.cs`
- Test: `dome/Test/Terraria.Dome.PlayerAuthority.Verification/Program.cs`
- Test: `dome/Test/Terraria.Dome.Verification/Program.cs`

**Step 1: Add intent/result tests**

Cover input suppression by death/collision, AI or knockback changing velocity without input changes, direction normalization, and geometry queries remaining write-free.

**Step 2: Remove semantic coupling**

Keep the existing shared Location/Velocity/Direction types as the public vocabulary. Rename only local variables or private helpers where they incorrectly call location “transform” or direction “facing”; do not introduce compatibility aliases for duplicate public types.

**Step 3: Make system ownership explicit**

Input systems write control intent; AI writes movement intent or velocity according to existing behavior; physics and movement write velocity/location; projectile and NPC special directions remain domain components.

**Step 4: Verify**

Run PlayerPhysics, PlayerAuthority and general ECS geometry verifiers after a serial Simulation build; inspect `git diff --check`; commit:

```text
git add src/Share/Entity dome/src/Terraria.Dome.Simulation/Components/Player dome/src/Terraria.Dome.Simulation/Player dome/src/Terraria.Dome.Simulation/Movement dome/src/Terraria.Dome.Simulation/Physics dome/src/Terraria.Dome.Simulation/Npc dome/Test/Terraria.Dome.PlayerPhysics.Verification dome/Test/Terraria.Dome.PlayerAuthority.Verification dome/Test/Terraria.Dome.Verification
git commit -m "refactor: separate movement intent from physical result"
```

## Task 5: 收敛 Item 定义、堆叠、库存、手部、装备、武器和容器

**Files:**

- Modify: `dome/src/Terraria.Dome.Simulation/Items/ItemDefinition.cs`
- Modify: `dome/src/Terraria.Dome.Simulation/Items/ItemStack.cs`
- Modify: `dome/src/Terraria.Dome.Simulation/Items/InventoryComponent.cs`
- Modify: `dome/src/Terraria.Dome.Simulation/Items/Components/ItemInstanceStateComponent.cs`
- Modify: `dome/src/Terraria.Dome.Simulation/Items/Components/ItemStackComponent.cs`
- Modify: `dome/src/Terraria.Dome.Simulation/Items/Components/ItemEquipmentStateComponent.cs`
- Modify: `dome/src/Terraria.Dome.Simulation/Items/Components/ItemUseStateComponent.cs`
- Modify: `dome/src/Terraria.Dome.Simulation/Items/Components/ItemOwnershipComponent.cs`
- Create or modify: `dome/src/Terraria.Dome.Simulation/Items/Components/ItemDefinitionComponent.cs`
- Create or modify: `dome/src/Terraria.Dome.Simulation/Items/Components/StackableItemComponent.cs`
- Create or modify: `dome/src/Terraria.Dome.Simulation/Items/Components/HandsComponent.cs`
- Create or modify: `dome/src/Terraria.Dome.Simulation/Items/Components/EquipmentComponent.cs`
- Create or modify: `dome/src/Terraria.Dome.Simulation/Items/Components/WeaponComponent.cs`
- Create or modify: `dome/src/Terraria.Dome.Simulation/Items/Components/ContainerComponent.cs`
- Modify: `dome/src/Terraria.Dome.Simulation/Items/Systems/InventoryCommandSystem.cs`
- Modify: `dome/src/Terraria.Dome.Simulation/Items/Systems/InventoryTransferSystem.cs`
- Modify: `dome/src/Terraria.Dome.Simulation/Items/Systems/ItemEquipmentSystem.cs`
- Modify: `dome/src/Terraria.Dome.Simulation/Items/Systems/ItemAmmoConsumptionSystem.cs`
- Modify: `dome/src/Terraria.Dome.Simulation/Items/Systems/ItemUseSystem.cs`
- Modify: `dome/src/Terraria.Dome.Simulation/WorldObjects/ChestDefinitionComponent.cs`
- Modify: `dome/src/Terraria.Dome.Simulation/WorldObjects/ChestInventoryComponent.cs`
- Modify: `dome/src/Terraria.Dome.Simulation/WorldObjects/ChestAccessComponent.cs`
- Test: `dome/Test/Terraria.Dome.Items.Verification/Program.cs`
- Test: `dome/Test/Terraria.Dome.Items.Definitions.Verification/Program.cs`
- Test: `dome/Test/Terraria.Dome.Items.Loopback.Verification/Program.cs`
- Test: `dome/Test/Terraria.Dome.MainFieldPropertyInventory.Verification/Program.cs`

**Step 1: Add failing state-separation tests**

Test that applying a prefix/definition variant does not change stack quantity, splitting/merging does not replace item definition, equipping does not rewrite inventory structure, hands can reference an item without exposing the entire inventory, and Chest capacity does not imply Player inventory semantics.

**Step 2: Map existing types before creating files**

Use the Task 1 inventory to decide whether an existing type is the target component under a clearer name. Do not create `ItemDefinitionComponent`, `StackableItemComponent`, `HandsComponent`, `EquipmentComponent`, `WeaponComponent` or `ContainerComponent` if the existing type already owns exactly that boundary; move or rename only with all consumers updated.

**Step 3: Migrate command and event writers**

Update create, transfer, split, merge, equip, unequip, use, pickup and drop commands so each command writes only the component it owns. Preserve `InventoryChangedEvent`, `ItemEquippedEvent`, item snapshots and loopback behavior.

**Step 4: Migrate Chest and world-item adapters**

Keep `ChestDefinitionComponent` and `ChestInventoryComponent` scoped to WorldObjects. Use `ContainerComponent` only for the capacity/containment relation; do not make Chest inherit Player inventory semantics.

**Step 5: Verify and commit**

Run all four item/inventory verifiers serially with `--no-build --no-restore`, inspect generated artifacts under `Build/bin/`, and commit:

```text
git add dome/src/Terraria.Dome.Simulation/Items dome/src/Terraria.Dome.Simulation/WorldObjects/Chest dome/Test/Terraria.Dome.Items.Verification dome/Test/Terraria.Dome.Items.Definitions.Verification dome/Test/Terraria.Dome.Items.Loopback.Verification dome/Test/Terraria.Dome.MainFieldPropertyInventory.Verification
git commit -m "refactor: separate item inventory and equipment state"
```

## Task 6: 收敛 Targeting、AI、Owner 和关系引用

**Files:**

- Modify: `dome/src/Terraria.Dome.Simulation/Npc/Components/NpcTargetComponent.cs`
- Modify: `dome/src/Terraria.Dome.Simulation/Components/AI/NpcAiStateComponent.cs`
- Modify: `dome/src/Terraria.Dome.Simulation/Components/AI/ProjectileBehaviorComponent.cs`
- Modify: `dome/src/Terraria.Dome.Simulation/Npc/Systems/NpcTargetSelectionSystem.cs`
- Modify: `dome/src/Terraria.Dome.Simulation/Npc/Systems/NpcTargetRoutingSystem.cs`
- Modify: `dome/src/Terraria.Dome.Simulation/Npc/Systems/NpcBehaviorSystem.cs`
- Modify: `dome/src/Terraria.Dome.Simulation/Projectile/Systems/ProjectileOwnerHitCheckSystem.cs`
- Modify: `dome/src/Terraria.Dome.Simulation/Projectile/Systems/ProjectileOwnerAnchoredMeleeSystem.cs`
- Modify: `dome/src/Terraria.Dome.Simulation/Combat/Events/DamageRequestedEvent.cs`
- Test: `dome/Test/Terraria.Dome.Npc.Boundary.Verification/Program.cs`
- Test: `dome/Test/Terraria.Dome.Npc.Composition.Verification/Program.cs`
- Test: `dome/Test/Terraria.Dome.PlayerOwnership.Verification/Program.cs`
- Test: `dome/Test/Terraria.Dome.Combat.Protocol.Verification/Program.cs`

**Step 1: Add failure-mode tests**

Cover target destruction, stale slot reuse, owner/target mismatch and cross-domain handle misuse. Every failure must be explicit (`NotFound`, `Expired`, `ScopeMismatch` or `Conflict`) rather than silently binding to a replacement entity.

**Step 2: Separate state writers**

Target selection writes only `NpcTargetComponent`; AI behavior writes only `NpcAiStateComponent`; Projectile owner is initialized at spawn and is not reused as current target.

**Step 3: Replace untyped relationships at boundaries**

Where a relation crosses protocol, compatibility or persistence boundaries, introduce/consume the existing typed handle or adapter. Do not expose a naked `int` or `Guid` as a cross-domain relationship in new code.

**Step 4: Verify and commit**

Run the four focused verifiers after a serial Simulation build, then commit:

```text
git add dome/src/Terraria.Dome.Simulation/Npc dome/src/Terraria.Dome.Simulation/Components/AI dome/src/Terraria.Dome.Simulation/Projectile/Systems dome/src/Terraria.Dome.Simulation/Combat/Events dome/Test/Terraria.Dome.Npc.Boundary.Verification dome/Test/Terraria.Dome.Npc.Composition.Verification dome/Test/Terraria.Dome.PlayerOwnership.Verification dome/Test/Terraria.Dome.Combat.Protocol.Verification
git commit -m "refactor: isolate targeting ai and entity relations"
```

## Task 7: 审计身份 Registry、复制、持久化和表现边界

**Files:**

- Inspect/modify: `src/Share/Entity/Components/EntityIdentityComponent.cs`
- Inspect/modify: `dome/src/Terraria.Dome.Simulation/Identity`
- Inspect/modify: `dome/src/Terraria.Dome.Simulation/Player/Components/PlayerIdentityComponent.cs`
- Inspect/modify: `dome/src/Terraria.Dome.Simulation/Npc/Components/NpcReplicationComponent.cs`
- Inspect/modify: `dome/src/Terraria.Dome.Simulation/Components/Projectile/ProjectileNetworkIdentityComponent.cs`
- Inspect/modify: `dome/src/Terraria.Dome.Simulation/Components/Projectile/ProjectileNetworkUpdateComponent.cs`
- Inspect/modify: `dome/src/Terraria.Dome.Simulation/Snapshots`
- Inspect/modify: `dome/src/Terraria.WorldCompatibility`
- Test: `dome/Test/Terraria.Dome.NetworkIsolation.Verification/Program.cs`
- Test: `dome/Test/Terraria.Dome.Persistence.Verification/Program.cs`
- Test: `dome/Test/Terraria.Dome.Persistence.Loopback.Verification/Program.cs`
- Test: `dome/Test/Terraria.Dome.Protocol.Compatibility.Verification/Program.cs`

**Step 1: Add identity/replication isolation checks**

Verify that EntityUuid is server-created and nonzero, protocol identity remains scoped, replication cursors do not mutate domain state, persistence restores through a new runtime entity, and prediction/presentation objects do not receive the authority component.

**Step 2: Audit adapters**

Document each conversion direction and failure result. Keep `EntityIdentityComponent` out of default Terraria packets and save keys. Remove any new implicit two-way synchronization.

**Step 3: Audit presentation/history**

Confirm rotation, old position/velocity, `gfxOffY`, old projectile presentation fields and render state are either derived or presentation-owned, with no server component dependency added.

**Step 4: Verify and commit**

Run the four identity/protocol/persistence verifiers serially, record command, exit code, warning/error counts and artifact paths, then commit:

```text
git add src/Share/Entity/Components/EntityIdentityComponent.cs dome/src/Terraria.Dome.Simulation dome/src/Terraria.WorldCompatibility dome/Test/Terraria.Dome.NetworkIsolation.Verification dome/Test/Terraria.Dome.Persistence.Verification dome/Test/Terraria.Dome.Persistence.Loopback.Verification dome/Test/Terraria.Dome.Protocol.Compatibility.Verification
git commit -m "refactor: enforce identity and replication boundaries"
```

## Task 8: 全量回归、重复模型清理和文档收口

**Files:**

- Modify: affected files identified by `rg` in Tasks 1-7
- Create: `docs/plans/2026-09-04-component-split-verification.md`
- Inspect: `src/Share/Entity/Terraria.EntityEcs.csproj`
- Inspect: `dome/src/Terraria.Dome.Simulation/Terraria.Dome.Simulation.csproj`

**Step 1: Search for forbidden leftovers**

Run searches for duplicate public declarations, aggregate component names, naked cross-domain IDs, `TransformComponent`, `FacingComponent`, `GenericLifetimeComponent`, `GenericResourceComponent` and component methods that perform cross-entity behavior.

**Step 2: Build affected project serially**

```text
pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 build .\dome\src\Terraria.Dome.Simulation\Terraria.Dome.Simulation.csproj -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false
```

Expected: exit code 0; record warning/error counts and confirm the expected DLL/PDB exists under `Build/bin/`.

**Step 3: Run focused verifier batch serially**

Run the affected Combat, Player, NPC, Projectile, Item, Network, Persistence, Protocol, Geometry and TickOrder verification projects one at a time through the serial script with `--no-build --no-restore`. Do not use parallel jobs or solution-wide build.

**Step 4: Record evidence**

Write exact commands, exit codes, warning/error counts, output paths, and any intentionally skipped verifier with its reason to `docs/plans/2026-09-04-component-split-verification.md`.

**Step 5: Final diff review and commit**

Run `git diff --check`, inspect `git diff --stat` and `git status --short`, confirm no generated output is staged, then commit:

```text
git add docs/plans/2026-09-04-component-split-verification.md <only-task-files>
git commit -m "test: verify full ecs component split"
```

## 完成定义

- 所有报告确认的组件边界均已完成字段归属审计并落地或明确记录为已有实现。
- 没有新的巨型聚合组件、双写权威字段或无作用域跨域关系。
- Player、NPC、Projectile、Item、WorldObject、协议和持久化现有行为验证通过。
- Simulation 构建和所有 focused verifier 均遵守串行构建契约，构建产物位于 `Build/bin/`。
- 设计、实施计划和验证证据均已提交，且不包含当前任务之外的工作区改动。

