# Item ECS Migration Implementation Plan


**Goal:** 将 Version4 的 `Terraria.Item` 物品域拆为可运行的服务器权威 ECS 定义、组件、系统、命令、事件、快照和边界适配器。

**Architecture:** 采用领域优先的 `Items/Definitions|Components|Commands|Events|Systems|Snapshots|Compatibility` 结构。静态物品规则由定义注册表提供，实例状态由 ECS 组件承载，所有客户端输入先进入命令队列，经确定性系统处理后生成快照；Server 只负责协议、PVS、持久化和快照投影。Version3/Version4 目录只读，旧 `Item` 仅在迁移适配器中短期存在。

**Tech Stack:** .NET 10、C#、Arch ECS、现有 `Terraria.Dome.Simulation`、`Terraria.Dome.Server`、V1456 协议和仓库内 executable verification 项目。

> 执行状态（2026-08-19）：Task 0-8 的服务器权威切片已有当前实现与验证证据；Task 9 的
> artifact boundary 已通过（`src/**/Build/(bin|obj|generated)` 当前为 0），但整体仍为
> `PARTIAL`，因为延期的 Version4/UI 成员未被声称迁移。

---

## 执行边界

- 本计划是执行提案，不代表迁移已完成。
- 当前工作树已有大量用户改动；执行时不得回滚或覆盖无关改动。
- 备份源：`D:\TRbackup\Version4物理删除了某些文件`；必要声明/数据源可只读参考
  `D:\TRbackup\Version3删除多余同时人工审查代码`。
- 所有命令从 `D:\TRbackup\NLTX` 执行，并在 serial build/test 中追加
  `-p:UseSharedCompilation=false`。
- 生成物只能进入 `Build/bin`、`Build/obj`、`Build/generated`、`Build/evidence/item-ecs`。

## Task 0: 建立迁移基线和写集清单

**Files:**
- Create: `Build/evidence/item-ecs/item-members.json`
- Create: `Build/evidence/item-ecs/version3-version4-file-diff.txt`
- Create: `Build/evidence/item-ecs/item-reference-index.txt`
- Create: `docs/migrations/item-ecs-member-mapping.md`

### Step 1: 记录当前工作树

Run:

```powershell
git status --short
git diff --stat
```

Expected: 只记录基线，不修改现有文件。

### Step 2: 生成 Version3/Version4 相对路径差异

Run 一个只读脚本，对两个备份目录生成排序后的相对路径集合和删除清单，输出到
`Build/evidence/item-ecs/version3-version4-file-diff.txt`。

Expected: 清单可重放，标注 `Item.cs`、`WorldItem.cs`、`ItemID.cs`、掉落规则、变体和 UI 文件。

### Step 3: 建立 `Item.cs` 成员索引

Run Roslyn 或等价语法树工具，输出类型、字段、属性、方法、依赖命名空间和引用调用方，保存为
`item-members.json` 与 `item-reference-index.txt`。

Expected: 每个成员都有唯一稳定标识；禁止用截断文本替代成员索引。

### Step 4: 编写成员归属表

在 `docs/migrations/item-ecs-member-mapping.md` 中为每个成员选择
`Definition`、`Component`、`System`、`Compatibility` 或 `Deferred`，记录来源文件和目标类型。

Expected: 没有“未调查”成员；UI/渲染/音效成员必须进入排除或延期章节。

## Task 1: 固定定义编译器的失败测试

**Files:**
- Create: `Test/Terraria.Dome.Items.Definitions.Verification/Program.cs`
- Create: `Test/Terraria.Dome.Items.Definitions.Verification/Terraria.Dome.Items.Definitions.Verification.csproj`
- Modify: `Terraria.Dome.sln`

### Step 1: 写定义校验失败场景

覆盖重复类型、零类型、非法堆叠上限、未知引用和互相矛盾的使用/装备属性。

### Step 2: 运行验证并确认失败

```powershell
dotnet run --project Test/Terraria.Dome.Items.Definitions.Verification/Terraria.Dome.Items.Definitions.Verification.csproj -c Release -p:UseSharedCompilation=false
```

Expected: 在定义类型尚未扩展时失败，失败原因必须是缺少目标定义/校验，而不是编译环境异常。

### Step 3: Commit

```powershell
git add Test/Terraria.Dome.Items.Definitions.Verification Terraria.Dome.sln
git commit -m "test: define item registry migration invariants"
```

## Task 2: 实现不可变 Item Definitions

**Files:**
- Create: `src/Terraria.Dome.Simulation/Items/Definitions/ItemIdentityDefinition.cs`
- Create: `src/Terraria.Dome.Simulation/Items/Definitions/ItemUseDefinition.cs`
- Create: `src/Terraria.Dome.Simulation/Items/Definitions/ItemCombatDefinition.cs`
- Create: `src/Terraria.Dome.Simulation/Items/Definitions/ItemPlacementDefinition.cs`
- Create: `src/Terraria.Dome.Simulation/Items/Definitions/ItemRecoveryDefinition.cs`
- Create: `src/Terraria.Dome.Simulation/Items/Definitions/ItemEquipmentDefinition.cs`
- Create: `src/Terraria.Dome.Simulation/Items/Definitions/ItemDropDefinition.cs`
- Create: `src/Terraria.Dome.Simulation/Items/Definitions/ItemVariantDefinition.cs`
- Modify: `src/Terraria.Dome.Simulation/Items/ItemDefinition.cs`
- Modify: `src/Terraria.Dome.Simulation/Items/ItemDefinitionRegistry.cs`
- Create: `src/Terraria.Dome.Simulation/Items/Definitions/ItemDefinitionCompiler.cs`

### Step 1: Implement value definitions

Keep all definitions immutable and free of `Terraria.Item`, `Main`, `Player`, UI, texture, sound or
Arch Entity references. Use explicit `DamageClass`/slot identifiers owned by the Simulation contract.

### Step 2: Implement registry validation

Reject duplicate IDs, invalid stack limits, missing dependent definitions and contradictory flags during
construction. Do not silently replace an existing definition.

### Step 3: Add one legacy import fixture

Import the existing type 1 healing definition into the new registry. The importer may read Version4 data,
but its output must be a pure definition object.

### Step 4: Run the definition verifier

```powershell
dotnet run --project Test/Terraria.Dome.Items.Definitions.Verification/Terraria.Dome.Items.Definitions.Verification.csproj -c Release -p:UseSharedCompilation=false
```

Expected: PASS for unique definitions and all rejection cases.

### Step 5: Commit

```powershell
git add src/Terraria.Dome.Simulation/Items Test/Terraria.Dome.Items.Definitions.Verification
git commit -m "feat: add immutable item definitions"
```

## Task 3: Split item instance state from inventory state

**Files:**
- Create: `src/Terraria.Dome.Simulation/Items/Components/ItemStackComponent.cs`
- Create: `src/Terraria.Dome.Simulation/Items/Components/ItemInstanceStateComponent.cs`
- Create: `src/Terraria.Dome.Simulation/Items/Components/ItemUseStateComponent.cs`
- Create: `src/Terraria.Dome.Simulation/Items/Components/ItemOwnershipComponent.cs`
- Create: `src/Terraria.Dome.Simulation/Items/Components/ItemEquipmentStateComponent.cs`
- Modify: `src/Terraria.Dome.Simulation/Items/InventoryComponent.cs`
- Modify: `src/Terraria.Dome.Simulation/Items/ItemStack.cs`

### Step 1: Add tests for incompatible stacks

Cover same type merge, stack limit, `UniqueStack`, different prefix, different variant and different dye.

### Step 2: Run tests to confirm failure

```powershell
dotnet run --project Test/Terraria.Dome.Items.Verification/Terraria.Dome.Items.Verification.csproj -c Release -p:UseSharedCompilation=false
```

Expected: new instance-state cases fail before implementation; existing authoritative inventory cases remain
identifiable.

### Step 3: Implement component split and full slot mapping

Keep `ItemStack` small. Move mutable metadata out of it. Extend inventory storage without changing the
server-owned selected-slot contract. Preserve imported slots outside the hotbar.

### Step 4: Run Items Verification

```powershell
dotnet run --project Test/Terraria.Dome.Items.Verification/Terraria.Dome.Items.Verification.csproj -c Release -p:UseSharedCompilation=false
```

Expected: PASS for merge/split/selection and new metadata invariants.

### Step 5: Commit

```powershell
git add src/Terraria.Dome.Simulation/Items Test/Terraria.Dome.Items.Verification
git commit -m "feat: separate item instances from inventory state"
```

## Task 4: Move world item lifecycle into ECS systems

**Files:**
- Create: `src/Terraria.Dome.Simulation/Items/Components/ItemWorldStateComponent.cs`
- Modify: `src/Terraria.Dome.Simulation/Items/WorldItemComponent.cs`
- Create: `src/Terraria.Dome.Simulation/Items/Systems/WorldItemSpawnSystem.cs`
- Create: `src/Terraria.Dome.Simulation/Items/Systems/WorldItemMotionSystem.cs`
- Create: `src/Terraria.Dome.Simulation/Items/Systems/WorldItemPickupSystem.cs`
- Create: `src/Terraria.Dome.Simulation/Items/Commands/CreateWorldItemCommand.cs`
- Create: `src/Terraria.Dome.Simulation/Items/Commands/DestroyWorldItemCommand.cs`
- Create: `src/Terraria.Dome.Simulation/Items/Commands/MoveWorldItemCommand.cs`
- Modify: `src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs`

### Step 1: Add lifecycle and race tests

Cover spawn, pickup delay, range rejection, simultaneous pickup, partial stack acceptance, inactive
tombstone and monotonic revision.

### Step 2: Run Loopback Verification before implementation

```powershell
dotnet run --project Test/Terraria.Dome.Items.Loopback.Verification/Terraria.Dome.Items.Loopback.Verification.csproj -c Release -p:UseSharedCompilation=false
```

Expected: baseline may pass existing behavior but new ECS lifecycle cases fail.

### Step 3: Implement commands and systems

Keep `ReplicationId` and section identity in the domain snapshot/identity boundary. Do not expose Arch
Entity handles to Server. Apply structural changes once per tick.

### Step 4: Run Items and Loopback Verification

```powershell
dotnet run --project Test/Terraria.Dome.Items.Verification/Terraria.Dome.Items.Verification.csproj -c Release -p:UseSharedCompilation=false
dotnet run --project Test/Terraria.Dome.Items.Loopback.Verification/Terraria.Dome.Items.Loopback.Verification.csproj -c Release -p:UseSharedCompilation=false
```

Expected: PASS for one-winner pickup, duplicate rejection, range/PVS behavior and tombstone handling.

### Step 5: Commit

```powershell
git add src/Terraria.Dome.Simulation/Items Test/Terraria.Dome.Simulation/Simulation Test/Terraria.Dome.Items* Test/Terraria.Dome.Items.Loopback.Verification
git commit -m "feat: migrate world item lifecycle to ECS"
```

## Task 5: Implement use, cooldown, equipment and ammunition systems

**Files:**
- Create: `src/Terraria.Dome.Simulation/Items/Systems/ItemInputValidationSystem.cs`
- Create: `src/Terraria.Dome.Simulation/Items/Systems/ItemUseCooldownSystem.cs`
- Create: `src/Terraria.Dome.Simulation/Items/Systems/ItemUseSystem.cs`
- Create: `src/Terraria.Dome.Simulation/Items/Systems/ItemAmmoConsumptionSystem.cs`
- Create: `src/Terraria.Dome.Simulation/Items/Systems/ItemPlacementSystem.cs`
- Create: `src/Terraria.Dome.Simulation/Items/Systems/ItemEquipmentSystem.cs`
- Create: `src/Terraria.Dome.Simulation/Items/Events/ItemUsedEvent.cs`
- Create: `src/Terraria.Dome.Simulation/Items/Events/ItemEquippedEvent.cs`
- Modify: `src/Terraria.Dome.Simulation/Items/Commands/UseItemCommand.cs`
- Modify: `src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs`

### Step 1: Add failing use/equipment tests

Cover healing, mana, cooldown, channel, failed use rollback, ammo shortage, equipment conflict, vanity
state and placement rejection.

### Step 2: Run the current Items Verification

Expected: new cases fail without state-machine implementation; existing type 1 use behavior identifies
the compatibility baseline.

### Step 3: Implement deterministic use transaction

Validate selected slot and definition first. Produce effect commands, consume main item/ammo only when
the effect is accepted, and emit an event after commit. Never mutate Player health directly from a command
handler.

### Step 4: Run verification

```powershell
dotnet run --project Test/Terraria.Dome.Items.Verification/Terraria.Dome.Items.Verification.csproj -c Release -p:UseSharedCompilation=false
```

Expected: PASS for cooldown, no-side-effect rejection, equipment conflicts and ammo rules.

### Step 5: Commit

```powershell
git add src/Terraria.Dome.Simulation/Items src/Terraria.Dome.Simulation/Simulation Test/Terraria.Dome.Items.Verification
git commit -m "feat: add authoritative item use and equipment systems"
```

## Task 6: Convert drops, prefixes and variants to data-driven rules

**Files:**
- Create: `src/Terraria.Dome.Simulation/Items/Systems/ItemDropRuleSystem.cs`
- Create: `src/Terraria.Dome.Simulation/Items/Systems/ItemPrefixSystem.cs`
- Create: `src/Terraria.Dome.Simulation/Items/Systems/ItemVariantSystem.cs`
- Modify: `src/Terraria.Dome.Simulation/Items/Definitions/ItemDropDefinition.cs`
- Modify: `src/Terraria.Dome.Simulation/Items/Definitions/ItemVariantDefinition.cs`
- Modify: `src/Terraria.Dome.Simulation/Loot/`

### Step 1: Add deterministic rule tests

Use the same world seed, source entity and tick to assert identical results. Add cases for conditions,
chain weights, quantity ranges, expert/master restrictions, prefix application and one-time variants.

### Step 2: Implement pure rule evaluation

Read definitions and immutable source snapshots. Derive RNG from world seed/source/tick. Emit
`CreateWorldItemCommand`; do not create or mutate legacy `Item` objects.

### Step 3: Run loot and item verification

```powershell
dotnet run --project Test/Terraria.Dome.Items.Verification/Terraria.Dome.Items.Verification.csproj -c Release -p:UseSharedCompilation=false
```

Expected: deterministic drop, prefix and variant assertions pass.

### Step 4: Commit

```powershell
git add src/Terraria.Dome.Simulation/Items src/Terraria.Dome.Simulation/Loot Test/Terraria.Dome.Items.Verification
git commit -m "feat: migrate item drops prefixes and variants"
```

## Task 7: Add item snapshots, persistence and network projection

**Files:**
- Create: `src/Terraria.Dome.Simulation/Items/Snapshots/ItemInstanceSnapshot.cs`
- Create: `src/Terraria.Dome.Simulation/Items/Snapshots/InventorySnapshot.cs`
- Create: `src/Terraria.Dome.Simulation/Items/Snapshots/EquipmentSnapshot.cs`
- Create: `src/Terraria.Dome.Simulation/Items/Snapshots/WorldItemSnapshot.cs`
- Create: `src/Terraria.Dome.Simulation/Items/Snapshots/ItemUseSnapshot.cs`
- Modify: `src/Terraria.Dome.Simulation/Snapshots/ItemReplicationSnapshot.cs`
- Modify: `src/Terraria.Dome.Server/Replication/ItemReplicationAssembler.cs`
- Create: `src/Terraria.Dome.Server/Replication/InventoryReplicationAssembler.cs`
- Create: `src/Terraria.Dome.Server/Replication/EquipmentReplicationAssembler.cs`
- Modify: `src/Terraria.Dome.Server/Persistence/DomeStatePersistenceFormat.cs`

### Step 1: Add snapshot round-trip and spoofing tests

Cover complete instance metadata, inventory slots outside hotbar, revision ordering, PVS filtering,
save/reload and forged client `SyncItem`/equipment packets.

### Step 2: Implement immutable snapshot projection

Simulation publishes snapshots only after committed state. Server encodes `SyncItem` and related frames;
incoming client state frames remain non-authoritative.

### Step 3: Run protocol and persistence verification

```powershell
dotnet run --project Test/Terraria.Dome.Protocol.Compatibility.Verification/Terraria.Dome.Protocol.Compatibility.Verification.csproj -c Release -p:UseSharedCompilation=false
dotnet run --project Test/Terraria.Dome.Persistence.Verification/Terraria.Dome.Persistence.Verification.csproj -c Release -p:UseSharedCompilation=false
dotnet run --project Test/Terraria.Dome.Items.Loopback.Verification/Terraria.Dome.Items.Loopback.Verification.csproj -c Release -p:UseSharedCompilation=false
```

Correct the project path to the repository-relative `.csproj` if the verifier is located directly under
the project directory; the expected result is PASS, not a path failure.

### Step 4: Commit

```powershell
git add src/Terraria.Dome.Simulation src/Terraria.Dome.Server Test
git commit -m "feat: project item ECS state to persistence and protocol"
```

## Task 8: Contract adapters and old Item dependency audit

**Files:**
- Create: `src/Terraria.Dome.Simulation/Items/Compatibility/LegacyItemDefinitionAdapter.cs`
- Create: `src/Terraria.Dome.Simulation/Items/Compatibility/LegacyItemImportAdapter.cs`
- Create: `src/Terraria.Dome.Simulation/Items/Compatibility/LegacyItemDropAdapter.cs`
- Create: `Build/evidence/item-ecs/legacy-item-reference-audit.txt`
- Modify: `docs/migrations/item-ecs-member-mapping.md`

### Step 1: Keep compatibility at the boundary

Adapters may consume Version4/Version3 objects or data and output definitions, instance state or commands.
No ECS system may reference `Terraria.Item`, `Main.item[]`, `Player.inventory[]` or legacy static arrays.

### Step 2: Run reference audit

```powershell
rg -n --hidden -S "Terraria\.Item|Main\.item|Player\.inventory|ContentSamples|ItemID\.Sets" src/Terraria.Dome.Simulation
```

Expected: zero matches in production Item systems; any remaining match must be in a named adapter and
listed in the mapping document.

### Step 3: Close the member mapping

Every `Item.cs` member must be marked migrated, adapter-only, excluded UI/client concern or deferred with
reason and owner. No member may remain unclassified.

### Step 4: Commit

```powershell
git add src/Terraria.Dome.Simulation/Items/Compatibility docs/migrations/item-ecs-member-mapping.md Build/evidence/item-ecs
git commit -m "refactor: isolate legacy item compatibility boundary"
```

## Task 9: Full verification and completion evidence

**Files:**
- Create: `Build/evidence/item-ecs/final-verification.txt`
- Create: `Build/evidence/item-ecs/final-manifest.json`
- Modify: `progress.md`

### Step 1: Build serially from repository root

Run the relevant solution/project build commands with:

```powershell
-p:UseSharedCompilation=false
```

Expected: exit code 0 for the projects in scope; record warnings and errors, not just the final summary.

### Step 2: Run the item verification matrix

Run Definitions, Items, Loopback, Protocol Compatibility and Persistence verifiers. Capture command,
exit code, stdout/stderr summary and artifact paths in `final-verification.txt`.

### Step 3: Check artifact boundaries

```powershell
rg --files src | rg "(^|[\\/])(bin|obj|generated)([\\/]|$)"
```

Expected: no compiled output or generated files under `src/`.

### Step 4: Produce the final manifest

`final-manifest.json` must include source baseline, target files, verification commands, exit codes,
warnings, deferred members, remaining adapters and Version4 deletion conclusions.

### Step 5: Update progress only with verified facts

Mark unrun or partial batches as `planned`/`unverified`; do not claim the old Item implementation is
removed until the reference audit and runtime verification both pass.

### Step 6: Commit documentation and evidence references

```powershell
git add docs/plans/2026-08-18-item-ecs-migration-design.md docs/plans/2026-08-18-item-ecs-migration-execution.md docs/migrations progress.md
git commit -m "docs: add item ECS migration execution plan"
```

## Rollback boundaries

- Before each task commit, revert only that task's explicitly listed files if its verification fails.
- Never use `git reset --hard`, broad cleanup, or deletion of `Build/` entries from other tasks.
- If a failure is caused by existing dirty files outside the task write set, preserve the files and record
  the blocker in the evidence manifest.
- Do not restore all Version4 deleted files. Restore only a narrowly identified declaration/data source
  into a compatibility adapter, then convert it to ECS data.

## Completion statement template

Use this only after evidence exists:

```text
Item ECS migration: VERIFIED/PARTIAL
Definition registry: PASS/FAIL
Inventory and instance state: PASS/FAIL
World item and pickup: PASS/FAIL
Use/equipment/ammo: PASS/FAIL
Drops/prefixes/variants: PASS/FAIL
Persistence/network projection: PASS/FAIL
Legacy Item reference audit: PASS/FAIL
Deferred members: <count and manifest path>
```
