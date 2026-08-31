# Projectile World-Object Command Boundary Blocked Execution Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan
> task-by-task.

**Goal:** 固化 `TileObject.CanPlace -> 多格 tile placement -> Sign.TextSign -> object placement
replication -> projectile tombstone` 的阻塞边界；只有在负责人明确批准扩展后，才以单一原子
world-object command/commit 契约推进可验证的 Projectile placement 迁移。

**Architecture:** Projectile 行为只能提交不可变 placement request；Simulation 负责完整 footprint
的规范化、计划、冲突检查和原子提交，随后发布领域事件并链接 `Expired` tombstone。Server/Protocol
只消费提交事件并按 session cursor 复制 object-placement 与 sign update。现有单格
`TileChangeCommand` 和 `CreateSign()` 保持原职责，不能被拼接成该跨资源事务的替代品。

**Tech Stack:** C# record/只读集合、现有 ECS `DomeSimulation`、`WorldGrid`/section version、
Simulation command queue、Server replication cursor、V1456 packet projector、Combat/Protocol
focused verifiers；串行构建使用 `-p:UseSharedCompilation=false`。

---

## 0. 当前状态与硬阻塞

状态：`blocked`（整体 Projectile 迁移仍为 `partial/completed_partial`）。

### 0.1 证据基线与本文档性质

- 主报告：`docs/research/2026-08-28-field-property-migration-comparison.md`；其中
  `Projectile.cs` 基线为 120 个字段、9 个属性块，唯一排除的身份声明是
  `Projectile.identity`，因此本计划的非 ID 范围是 128 个声明，整体迁移估算仍为约 38%。
- 关键节点报告：`docs/research/2026-08-28-projectile-field-property-ecs-migration.md`；它把
  Projectile 的权威链限定为 `Definition -> Component -> System -> Snapshot/Protocol`，并将
  完整 AI、免疫、特殊机制、客户端表现和完整协议 cadence 保持为 `partial/deferred`。
- Oracle：`D:\TRbackup\Version4物理删除了某些文件\Terraria\Projectile.cs`，SHA-256
  `97C3D68586AEF862411699E428F5C4B18CEE1F8051B684BE9F86FA7B60FAE83B`。style-17/type-43
  placement 的 `CanPlace`、sign 文本更新和 `Kill()` 顺序来自该 Oracle；它们不是批准当前
  代码扩展的依据。
- 本文件是执行前的边界/批准记录，不是“已完成迁移”报告。现有 placement 类型、focused
  verifier 或历史证据即使存在，也只能作为待审计输入，不能默认为本契约已获批准。

本文档中的“忽略 ID”只适用于统计范围：不得删除或改写 `Projectile.identity`、replication
identity 或 tombstone identity。它们仍是实体寻址、幂等、快照和协议关联的运行时契约。

本计划只在以下决策之一发生后进入实现任务：

1. **批准扩展边界（推荐）**：批准新增原子 world-object command/commit 契约，并允许继续
   style-17/type-43 sign projectile 链；或
2. **切换行为族**：明确指定下一个已有 Projectile 字段/行为族，放弃本链的代码推进。

未得到 1 或 2 前，不得新增跨 `WorldGrid`、sign store、replication event 和 projectile
tombstone 的代码，不得以现有 placement verifier 的绿色结果解除阻塞。

当前已有但不等于本边界完成的证据：

- `docs/plans/2026-08-29-projectile-sign-placement-execution.md` 已描述完整实现草案；
- 工作树已有（可能是既有或未跟踪改动的）`WorldObjectPlacementRequest/Plan/CommitSystem`、
  placement event、server assembler 和 placement loopback verifier；这些文件在批准前只读，
  不得借此扩大 write-set；
- B175 focused evidence 覆盖 object/sign/tombstone 顺序、重试、重复 sequence、actor 校验和
  section-version 冲突；
- 这些证据尚未替代负责人对“新增原子 world-object 契约”的边界批准，也不代表完整
  `TileObject` 表、全部 packet parity 或 Projectile parity。

### 批准记录（执行前必须填写）

| 项目 | 当前值 |
| --- | --- |
| 边界状态 | `blocked` |
| 扩展契约批准人 | `未指定` |
| 批准日期 | `未指定` |
| 执行分支 | `未选择：原子 world-object command/commit 或下一个已有 Projectile 行为族` |
| 允许 write-set | `未批准前为空；批准后按 Task 1 冻结` |
| 不得改变的契约 | `TileChangeCommand` 单格语义、Projectile identity、现有 tombstone identity |

批准只能由负责人在该表或等价的 Flowstate checkpoint 中明确记录。仅有聊天中的“继续”、
单次 verifier 绿色或工作树中已经存在的 placement 类型，均不视为批准。

### 与当前 Projectile 批次的关系

当前 `B247-projectile-type1091-damage-eligibility` 只完成 type-1091 的
`localAI[0] > 0` 伤害资格边界，状态仍为 `completed_partial`；它不授予本计划的
world-object 扩展权限，也不改变本计划的 `blocked` 状态。若选择切换行为族，应以 B247 的
证据为前置背景另开批次，不能把 type-1091 伤害资格与 placement commit 混入同一 write-set。
B247 的完整 Combat verifier 仍在既有 `VerifyBoundedVitalRegeneration` fixture 处失败，且在
进入 Projectile 检查前退出；该 baseline failure 不能被本计划的窄验证覆盖或归因于 placement。

### 0.2 批准前唯一可执行动作（只读）

1. 复核主报告、关键节点报告、`docs/plans/2026-08-29-projectile-sign-placement-execution.md`
   以及当前 `TileChangeCommand`、`CreateSign()`、placement verifier 的真实实现。
2. 将已存在的 request/plan/commit/event 类型列入审计清单，标注其来源、编译状态和是否属于
   本批次；不得移动、重命名、删除或接线。
3. 在下方批准表或等价 Flowstate checkpoint 中记录负责人、日期、选择的分支和精确 write-set。
4. 在批准记录为空时，只能更新本计划的证据/状态文字；不得运行会写入 source、project、
   checkpoint 或生成新的 placement implementation 的命令。

### 0.3 为什么现有单格 command/`CreateSign()` 不能解除阻塞

| 现有路径 | 当前能力 | 无法表达的原子语义 | 证据位置 |
| --- | --- | --- | --- |
| `TileChangeCommand` | 一个 `X/Y` 与一个 tile replacement | 完整多格 footprint、sign mutation、projectile linkage 和同一 commit 的 tombstone | `src/Terraria.Dome.Simulation/Commands/TileChangeCommand.cs:5-29` |
| `CreateSign()` | 分配 sign id 并立即写入 sign store | `CanPlace`/占位冲突、footprint 版本前提、tile 与 sign 同时提交 | `src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs:1507-1524` |
| 现有 placement commit | 先逐格调用 `WorldGrid.TrySetTile`，再提交 sign/event | 后续 tile 写失败时无法撤销已经成功的前序 tile；不是跨 tile/sign 的 staged transaction | `src/Terraria.Dome.Simulation/WorldObjects/Placement/WorldObjectPlacementCommitSystem.cs:160-179` |
| 现有 sign reservation | 先递增 `_nextSignId`，失败时最多回收 id | 不能把 sign reservation、tile version、event 与 tombstone 作为一个可观察 commit | `src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs:1527-1557`、`1677-1717` |
| V1456 object packet | 可发送 object placement 与紧随其后的 sign frame | 当前 wire packet 本身不携带本契约要求的 sequence、section version 和 projectile linkage | `src/Terraria.Dome.Protocol.V1456/Packets/ObjectPlacementPacket.cs:3-13` |
| 现有 replication assembler | 在 `TryProject` 中先登记 `(sequence, sectionVersion)`，再校验/编码帧 | 编码或发送失败可能先消耗去重键，不能保证由 session cursor 重发同一 event | `src/Terraria.Dome.Server/Replication/ObjectPlacementReplicationAssembler.cs:13-21`、`69-117`；`src/Terraria.Dome.Server/DomeServer.cs:2190-2196` |
| projectile placement command | 允许调用方携带任意 `TombstoneReason` | placement 成功只能链接 `Expired`；当前参数形状不能强制该边界 | `src/Terraria.Dome.Simulation/Commands/ProjectileWorldObjectPlacementCommand.cs:7-18`、`src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs:1710-1721` |

因此，“把多格对象拆成多个 `TileChangeCommand`”或“先 `CreateSign()`、再补 tile”只能得到
顺序上的近似，不能得到失败无副作用、重复可幂等、跨资源同一提交点和 tombstone 关联。该
缺口正是本计划的阻塞边界，不应通过继续扩大现有单格 command 来规避。

## 1. 不变量与禁止事项

### 必须保持的不变量

- `identity`、projectile replication identity 和 tombstone identity 保留在运行时/协议契约中，
  只从迁移统计排除。
- request、plan、commit result、event 均不可变；footprint 按 `(y, x)` 稳定排序。
- `CanPlace` 必须覆盖完整 footprint；任一 tile、section version、owner 或 projectile 校验失败，
  都不能产生部分 tile/sign/event 状态。
- object-placement frame 先于 sign update frame；两者带同一 sequence、section version 和
  projectile linkage。
- 只有 commit 成功后才产生 `ProjectileTombstoneReason.Expired`；失败或 deferred 时 projectile
  保持 active。
- replication 失败只影响 cursor 重发，不回写 simulation 的 world mutation。

### 明确禁止

- 将多格对象拆成多个独立 `TileChangeCommand` 以伪造原子性；
- 用 `CreateSign()` 单独验证/占用 TileObject footprint；
- 在 behavior 中直接写 `WorldGrid`、sign dictionary 或 V1456 packet；
- 将 focused verifier、单次 loopback 或 B175 结果宣称为完整 Projectile migration parity；
- 在未批准时修改 `TileChangeCommand` 的通用语义或扩大到全量 `TileObject` registry。

### 失败代码（批准后冻结，不得用异常代替）

实现分支应使用稳定的 `WorldObjectPlacementFailureCode`（名称可与现有枚举合并，但语义不可
缩减）：`InvalidSequence`、`DuplicateSequence`、`InvalidOrigin`、`UnsupportedObject`、
`InvalidStyle`、`InvalidDirection`、`InvalidSignText`、`OwnerInactive`、`ProjectileInactive`、
`OccupiedTile`、`VersionConflict`、`SectionVersionOverflow`、`InvalidFootprint`、
`InvalidFrame` 和 `MissingSupport`。若需要更细的 `OutOfBounds`、`TextTooLong` 或
`OverlappingFootprint` 诊断，应作为这些稳定语义的细分而不是替换既有结果。每个拒绝/延期结果
都必须明确说明 world、sign、event 和 tombstone 是否保持不变；禁止只返回 `false` 或在
commit 中途抛出未分类异常。

## 2. 批准后的执行任务

### Task 1: 冻结输入契约并补 RED verifier

**Files:**

- Read: `docs/research/2026-08-28-projectile-field-property-ecs-migration.md`
- Read: `docs/plans/2026-08-29-projectile-sign-placement-execution.md`
- Modify: `Test/Terraria.Dome.Combat.Verification/Program.cs`
- Modify: `.agent-workplace/state/projectile-lifecycle-task.json`

**Steps:**

1. 记录批准人、批准日期、选择的分支（扩展契约或切换行为族）和 write-set。
2. 先加入负用例：负/重复 sequence、越界 origin、未知 object/style、超过 100 字符、inactive
   owner/projectile、stale section version、occupied footprint。
3. 运行 focused verifier，确认缺口以稳定 failure code 暴露，而不是异常或部分写入。
4. 只有 RED 证据保存后，才开始修改 Simulation owner。

### Task 2: 完成 Simulation 原子 commit

**Files:**

- `src/Terraria.Dome.Simulation/Commands/ProjectileWorldObjectPlacementCommand.cs`
- `src/Terraria.Dome.Simulation/WorldObjects/Placement/WorldObjectPlacementRequest.cs`
- `src/Terraria.Dome.Simulation/WorldObjects/Placement/WorldObjectPlacementPlan.cs`
- `src/Terraria.Dome.Simulation/WorldObjects/Placement/WorldObjectPlacementResult.cs`
- `src/Terraria.Dome.Simulation/WorldObjects/Placement/WorldObjectPlacementCommitSystem.cs`
- `src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs`
- `src/Terraria.Dome.Simulation/World/WorldGrid.cs`（仅在预检/暂存不足时修改）

**Steps:**

1. Normalize request：验证 finite 坐标/参数、文本长度、owner/projectile active、type/style
   白名单和单调 sequence。
2. Plan：调用 footprint registry 的 `CanPlace` 适配器，展开所有旧值/新值、sign mutation 和
   section versions；此阶段不写 world。
3. Conflict check：重复 sequence、重叠 footprint、旧值变化、section version 冲突和溢出均返回
   稳定 rejection/deferred code。
4. Commit：在 staged buffer 中一次性应用完整 footprint、sign state/revision 和 section version；
   任一失败都丢弃 staged buffer。
5. Publish：提交成功后追加不可变 placement event；随后才 enqueue `Expired` despawn。
6. 将 style-17 behavior 限制为 enqueue request，禁止直接调用 `CreateSign()` 或 world mutation。

### Task 3: 接入 object/sign replication

**Files:**

- `src/Terraria.Dome.Simulation/WorldObjects/Placement/WorldObjectPlacementCommittedEvent.cs`
- `src/Terraria.Dome.Server/Replication/ObjectPlacementReplicationAssembler.cs`
- `src/Terraria.Dome.Server/DomeServer.cs`
- `src/Terraria.Dome.Server/Replication/SessionReplicationState.cs`
- `src/Terraria.Dome.Protocol.V1456/Packets/ObjectPlacementPacket.cs`
- `src/Terraria.Dome.Protocol.V1456/Packets/TerrariaPacketCodec.cs`

**Steps:**

1. 把 event 投影为 session 可见 frame，以 sequence + section version 去重。
2. 固定 object-placement -> sign update 的发送顺序，并保留 projectile identity linkage。
3. 处理迟到、重复、过期 section frame：只返回 rejected/deferred，不重复 world mutation。
4. 验证复制失败不会产生第二个 sign、第二个 event 或第二个 tombstone。

### Task 4: 闭合 tombstone 与失败重试

**Files:**

- `src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs`
- `Test/Terraria.Dome.Combat.Placement.Loopback.Verification/Program.cs`
- `Test/Terraria.Dome.Combat.Protocol.Verification/Program.cs`
- `Test/Terraria.Dome.Hardening.Loopback.Verification/Program.cs`

**Steps:**

1. 成功路径断言完整 footprint、sign text/revision、单一 event 和 `Expired` tombstone。
2. 失败路径断言 world/sign/event 不变，projectile 仍 active 且 reason 为 `None`。
3. owner 在 request 与 commit 之间失活时，整笔事务拒绝且不留下副作用。
4. 重复 tick/sequence 只返回确定性 rejection 或既有 commit 结果，不重复放置。

## 3. 约 30% 高价值验证命令

从仓库根目录串行运行，所有诊断输出写入 `Build/diagnostics/projectile-world-object-boundary/<run-id>/`：

```powershell
dotnet build src/Terraria.Dome.Simulation/Terraria.Dome.Simulation.csproj -c Release -p:UseSharedCompilation=false
dotnet build src/Terraria.Dome.Server/Terraria.Dome.Server.csproj -c Release -p:UseSharedCompilation=false
dotnet build Test/Terraria.Dome.Combat.Verification/Terraria.Dome.Combat.Verification.csproj -c Release -p:UseSharedCompilation=false
dotnet run --project Test/Terraria.Dome.Combat.Verification/Terraria.Dome.Combat.Verification.csproj -c Release --no-build
dotnet build Test/Terraria.Dome.Combat.Protocol.Verification/Terraria.Dome.Combat.Protocol.Verification.csproj -c Release -p:UseSharedCompilation=false
dotnet run --project Test/Terraria.Dome.Combat.Protocol.Verification/Terraria.Dome.Combat.Protocol.Verification.csproj -c Release --no-build
dotnet build Test/Terraria.Dome.Combat.Placement.Loopback.Verification/Terraria.Dome.Combat.Placement.Loopback.Verification.csproj -c Release -p:UseSharedCompilation=false
dotnet run --project Test/Terraria.Dome.Combat.Placement.Loopback.Verification/Terraria.Dome.Combat.Placement.Loopback.Verification.csproj -c Release --no-build
git diff --check
```

验收只记录实际 exit code、warning/error 数和 verifier PASS/FAIL 行。任何窄验证通过都只能把
本批次标为 `completed_partial`，不能把总体 Projectile 迁移标为 complete。

## 4. 回滚与转向

- 未批准：只保留本计划和现有证据，不修改 C#、项目文件或通用 command 语义。
- 已批准但 RED：回滚仅限本批次新增 request/plan/commit/event/replication/verifier 文件，保留
  既有 Projectile identity、tombstone 和 `TileChangeCommand`。
- 已批准且 commit 契约仍无法提供原子可见性：停止 Task 2，不以补偿性删除或多 command 拼接
  代替；状态回到 `blocked`。
- 用户指定切换到另一 Projectile 行为族时，冻结本计划的 write-set 和证据，另开独立 batch；
  不在同一批次混入新的行为族。

## 5. Definition of Done

- [ ] 明确记录了扩展契约批准，或明确记录了切换行为族。
- [ ] style-17 request 不再直接写 world/sign/packet。
- [ ] 完整 footprint、sign state、section version 和 placement event 在单一 commit 中可见。
- [ ] 成功 commit 唯一链接 `Expired` projectile tombstone；失败路径无部分副作用。
- [ ] object-placement -> sign update -> projectile tombstone 顺序由真实 loopback 观察并验证。
- [ ] focused build/verifier 证据写入新鲜 `Build/diagnostics/` 路径。
- [ ] 批次保持 `completed_partial`，完整 TileObject 表、全部 packet parity、VFX/audio 和
  其他 Projectile 行为继续标记 `deferred`。

**当前结论：** 在批准门被明确打开前，本边界保持 `blocked`；下一可执行动作是获得批准或
   指定另一个已存在的 Projectile 字段/行为族，而不是继续扩大现有单格 command。
