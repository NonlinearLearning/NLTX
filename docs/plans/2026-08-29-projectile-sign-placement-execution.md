# Projectile Sign Placement Atomic Commit Implementation Plan


**Goal:** 为 `aiStyle == 17` 的 Projectile placement 行为建立一个可验证、可排序、原子提交的
world-object command/commit 契约，使多格 TileObject、Sign.TextSign、对象放置复制和 projectile
`Expired` tombstone 在同一个成功事务中完成。

**Architecture:** Projectile 行为只产生不可变的 placement request，不直接写 `WorldGrid`、sign
字典或协议帧。Simulation 层先把 request 解析为包含完整 footprint 的 placement plan，执行
`CanPlace`/版本校验并按确定性顺序提交；成功后发布一个领域 placement event，由 Server/Protocol
适配器分别生成 tile、object-placement 和 sign replication，最后才提交 projectile tombstone。
任何验证或提交失败都丢弃整个 plan，保留 projectile active，且不得留下部分 tile/sign/event 状态。

**Tech Stack:** C# record/只读集合、现有 ECS `DomeSimulation`、`WorldGrid` 与 command buffer、
V1456 replication adapters、现有 Combat/Loopback verifier；构建使用 .NET SDK 和
`-p:UseSharedCompilation=false`。

---

## 1. 范围、非目标与批准门

### 范围

- 只覆盖当前 Oracle 已确认的 placement 链：
  `TileObject.CanPlace -> 多格 tile placement -> Sign.TextSign -> object placement replication -> projectile tombstone`。
- 首个实现族限定为 Oracle 中 `aiStyle == 17` 且 `type == 43` 的 sign projectile；对象类型、尺寸、
  direction 和文本长度必须来自现有 definition/behavior state，而不是由网络客户端决定。
- 保留现有 `TileChangeCommand` 处理单格地形变更；新的 command 只承担 world-object 的跨资源原子性。
- 保留 identity 字段和 replication identity 的运行时/协议契约；ID 仅在迁移统计中排除，不在代码中删除。

### 非目标与明确延期

- 不在本批次声称完成 Terraria 全部 `TileObject` style 表或所有 projectile AI parity。
- 不实现所有 object-placement packet 的历史客户端差异、音效/VFX、多人竞争的完整预测回滚。
- 不把多格对象拆成若干独立 `TileChangeCommand`，也不把当前 `CreateSign()` 当作 placement 验证。
- 若产品负责人不批准新增 world-object 契约，应停止本批次并改选下一个已有 Projectile 字段/行为族；
  不得以窄 verifier 结果替代该批准。

## 2. Oracle 行为锚点

来源：`D:\TRbackup\Version4物理删除了某些文件\Terraria\Projectile.cs`。

- `miscText` 字段在约第 222 行声明，初始化/重置为空字符串（约第 541 行）；迁移后的输入边界为
  非空、最多 100 个字符。
- `aiStyle == 17` 分支约第 20883 行：落地时执行 `velocity.X *= 0.98f`，随后增加重力。
- 同一分支约第 20917–20931 行：
  1. 计算 `(x, y, type=85, style, direction)`；
  2. 调用 `TileObject.CanPlace` 得到完整 `TileObject`；
  3. 只有 `TileObject.Place` 成功才调用 `NetMessage.SendObjectPlacement`；
  4. 读取 sign 后调用 `Sign.TextSign(num149, miscText)` 和 sign 更新帧；
  5. 最后调用 `Kill()`。
- 因此失败路径必须保持 projectile active，成功路径必须把 footprint、sign 文本、复制事件和
  `ProjectileTombstoneReason.Expired` 绑定在同一 commit 结果上。

## 3. 新的契约（实现前冻结）

在写 C# 前先把以下字段和不变量记录到 code review/Flowstate checkpoint；名称可在实现时微调，
但语义不可缩减。

### 3.1 Simulation command/request

建议新文件：`src/Terraria.Dome.Simulation/Commands/ProjectileWorldObjectPlacementCommand.cs`。

不可变 command 至少包含：

- `long Sequence`：由 simulation 的单调序列分配；不得为负或重复。
- `Entity Projectile` 与 projectile replication identity：用于 owner 校验、幂等和 tombstone 关联。
- `Entity Owner`：提交时必须仍然 active；只接受服务器已确认的 owner。
- `int OriginX`, `int OriginY`、`ushort ObjectType`、`int Style`、`int Direction`：对象锚点和
  TileObject 选择参数；首个实现固定 object type 85，拒绝未知 type/style。
- `string SignText`：不可为 null，长度不超过 100；不得从不可信协议帧重新解释。
- `long? ExpectedSectionVersion` 或等价的 footprint 版本前提：检测并发写入。

建议放在 `src/Terraria.Dome.Simulation/WorldObjects/Placement/` 的领域类型：

- `WorldObjectPlacementRequest`：行为层输入，不能携带已写入的 tile。
- `WorldObjectPlacementPlan`：`CanPlace` 之后的完整 footprint、每格旧值/新值、sign mutation、
  replication payload 和 tombstone linkage；所有集合只读并按 `(y, x)` 排序。
- `WorldObjectPlacementResult`：`Committed`/`Rejected`/`Deferred`、稳定 failure code、提交序列、
  受影响 section 版本和可供复制层使用的 event；不得只返回 bool。

### 3.2 原子 commit 边界

建议新文件：`src/Terraria.Dome.Simulation/WorldObjects/Placement/WorldObjectPlacementCommitSystem.cs`。

系统必须提供下列阶段，阶段之间不产生可见的部分状态：

1. **Normalize**：检查坐标、有限数值、文本长度、owner/projectile active、type/style 白名单。
2. **Plan**：调用 TileObject/footprint registry 的 `CanPlace` 适配器，展开所有 tile mutation，
   读取 sign anchor；不调用 `CreateSign()`，不写 `WorldGrid`。
3. **Conflict check**：验证 footprint 内所有 tile 仍等于 plan 的预期版本/旧值，section version
   未溢出，且同一 tick 没有重复 sequence 或重叠 placement 冲突。
4. **Commit**：一次性应用完整 footprint；创建/更新 sign state；递增受影响 section/revision。
5. **Publish**：追加一个不可变 `WorldObjectPlacementCommittedEvent`，事件携带 object placement
   参数、footprint、sign snapshot/revision 和 projectile identity。
6. **Tombstone link**：只有前五步成功后才允许 `DomeSimulation` 产生 `DespawnEntityCommand`
   (`ProjectileTombstoneReason.Expired`)；失败/Deferred 均不得销毁 projectile。

`WorldGrid` 的现有 `TrySetTile`/`CommitTileChanges` 仍可作为底层受控写入，但不能让外部调用者
在 commit 中途观察到一半 footprint。若现有 commit system 无法提供回滚或预检，先增加 staged
write/transaction buffer，再接入 projectile 行为。

### 3.3 Replication 契约

- Simulation 只发布领域 event；不要在 `Terraria.Dome.Simulation` 引用 V1456 packet 类型。
- Server 侧建议在 `src/Terraria.Dome.Server/Replication/WorldSectionReplication.cs` 或新增
  `ObjectPlacementReplicationAssembler.cs` 消费 event，按 session visibility 和 cursor 去重。
- Protocol 侧新增 isolation/packet projection（建议位于
  `src/Terraria.Dome.Protocol.V1456/Isolation/` 与 `Packets/`），映射到现有
  `NetMessage.SendObjectPlacement` 形状，并紧随其后发 sign update；客户端看到的顺序必须与
  commit event 一致。
- event/frame 必须携带 sequence、section version、origin、object type/style/alternate/random/
  direction、sign id/revision（若有）以及 projectile identity linkage，便于重放和丢包后的幂等。
- 复制失败不能回写 simulation 成功状态；应记录 rejected/deferred telemetry，并由 cursor 在
  下一个合法 frame 重发。

## 4. 执行步骤

### Task 1: 冻结契约与现有边界

**Files:**

- Read: `src/Terraria.Dome.Simulation/Commands/TileChangeCommand.cs`
- Read/Modify: `src/Terraria.Dome.Simulation/World/WorldGrid.cs`
- Read: `src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs` (`CreateSign`, `CommitCommands`,
  `MoveProjectiles`)
- Read: `src/Terraria.Dome.Server/DomeServer.cs`
- Read: `src/Terraria.Dome.Protocol.V1456/LegacyReference/NetMessage.cs`

**Steps:**

1. 列出现有单格 command 的序列、section version、失败语义和清空时机。
2. 标出 `CreateSign()` 当前只写 sign snapshot、不会验证/占用 tile 的证据。
3. 在 Flowstate 状态中登记新批次 `B168-projectile-style17-sign-placement-atomic-commit`，
   节点建议从 `N139` 开始，状态保持 `completed_partial`。
4. 由负责人批准“新增 world-object command/commit 契约”边界；未批准不得进入实现任务。

### Task 2: 编写失败优先的契约 verifier

**Files:**

- Create: `src/Terraria.Dome.Simulation/Commands/ProjectileWorldObjectPlacementCommand.cs`
- Create: `src/Terraria.Dome.Simulation/WorldObjects/Placement/WorldObjectPlacementResult.cs`
- Test: `Test/Terraria.Dome.Combat.Verification/Program.cs`

**Steps:**

1. 先加入 verifier：负 sequence、重复 sequence、越界 origin、未知 type/style、超过 100 字符、
   inactive owner/projectile 均返回稳定 rejection code。
2. 运行 focused verifier，确认在 commit system 尚不存在时以预期失败方式暴露缺口。
3. 实现最小不可变 command/result 和输入规范化，不写 world state。
4. 重新运行 focused verifier；预期所有 rejection case PASS，且没有 tile/sign/tombstone 副作用。

### Task 3: 实现 footprint plan 与原子 world commit

**Files:**

- Create: `src/Terraria.Dome.Simulation/WorldObjects/Placement/WorldObjectPlacementRequest.cs`
- Create: `src/Terraria.Dome.Simulation/WorldObjects/Placement/WorldObjectPlacementPlan.cs`
- Create: `src/Terraria.Dome.Simulation/WorldObjects/Placement/WorldObjectPlacementCommitSystem.cs`
- Modify: `src/Terraria.Dome.Simulation/World/WorldGrid.cs`
- Modify: `src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs`
- Reuse/Read: `src/Terraria.Dome.Simulation/WorldObjects/SignSnapshot.cs` 及 sign mutation 类型

**Steps:**

1. 添加只读 footprint mutation 集合，固定排序并保存旧值/新值及 section version。
2. 实现 `CanPlace` 适配器；拒绝越界、任一占用 tile、非法 frame、版本冲突和重复 footprint。
3. 添加 staged transaction，使多格写入、sign state/revision 和 section version 在单一 commit
   中完成；任何异常都清空 staged state 并返回 rejection。
4. 将 style-17 behavior 改为仅 enqueue request；禁止 behavior 直接调用 `WorldGrid` 或 `CreateSign`。
5. 只有 `Committed` 结果才 enqueue `Expired` despawn；`Rejected/Deferred` 保持 projectile active。
6. 运行 Simulation build，预期 0 errors；若出现现有 command 顺序回归，先恢复原单格路径，
   不扩大本批次范围。

### Task 4: 发布领域事件并接入对象/sign 复制

**Files:**

- Create: `src/Terraria.Dome.Simulation/WorldObjects/Placement/WorldObjectPlacementCommittedEvent.cs`
- Create/Modify: `src/Terraria.Dome.Server/Replication/ObjectPlacementReplicationAssembler.cs`
- Modify: `src/Terraria.Dome.Server/DomeServer.cs`
- Create/Modify: `src/Terraria.Dome.Protocol.V1456/Isolation/NetworkObjectPlacementSlice.cs`
- Create/Modify: `src/Terraria.Dome.Protocol.V1456/Packets/ObjectPlacementReplication*.cs`
- Read/Modify: `src/Terraria.Dome.Server/Replication/SessionReplicationState.cs`

**Steps:**

1. 将 commit event 转为 session 可见的 object-placement frame；以 sequence + section version 去重。
2. 保证 object placement frame 先于 sign update frame，且两者来自同一 commit event。
3. 对迟到、重复、过期 section frame 返回 deferred/rejected，不重复创建 sign 或对象。
4. 在 loopback verifier 中断言客户端收到完整 footprint、sign 文本/revision 与 placement 顺序。

### Task 5: 端到端 projectile tombstone linkage

**Files:**

- Modify: `src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs`
- Read/Modify: `src/Terraria.Dome.Server/Replication/CombatReplicationAssembler.cs`
- Test: `Test/Terraria.Dome.Combat.Verification/Program.cs`

**Steps:**

1. 成功 placement 后验证 projectile snapshot 为 inactive、reason 为 `Expired`，并保留既有 tombstone retention。
2. 失败 placement 后验证 projectile 仍 active、reason 为 `None`，world/sign snapshot 完全不变。
3. 验证重复 tick/重复 sequence 不会再次放置、增加 sign revision 或产生第二个 tombstone。
4. 验证 owner 在 request 与 commit 之间变 inactive 时整笔事务拒绝。

### Task 6: 约 30% 高价值验证与收口

**Files:**

- Modify: `Test/Terraria.Dome.Combat.Verification/Program.cs`
- Modify: `Test/Terraria.Dome.Hardening.Loopback.Verification/Program.cs`
- Add/Modify: `Test/Terraria.Dome.Combat.Placement.Loopback.Verification/Program.cs`
- Add: `Test/Terraria.Dome.Combat.Placement.Loopback.Verification/Terraria.Dome.Combat.Placement.Loopback.Verification.csproj`
- Optional read-only evidence: `Test/Terraria.Dome.WorldGeneration.Verification/Program.cs`
- Modify: `.agent-workplace/state/projectile-lifecycle-task.json`（仅在实现完成对应节点后）

Placement 复制使用独立的 placement-specific TCP verifier：它保留现有 Combat loopback
对通用战斗链路的覆盖，同时用两个真实 socket client 固定验证
`object-placement -> sign update -> projectile tombstone` 的顺序、可见性和 footprint。
这样不会把 placement 的时序断言耦合到旧 Combat loopback 的输入/帧噪声，也不替换旧 verifier。

**Commands (serial, from repository root):**

```powershell
dotnet build src/Terraria.Dome.Simulation/Terraria.Dome.Simulation.csproj -c Release -p:UseSharedCompilation=false
dotnet build src/Terraria.Dome.Server/Terraria.Dome.Server.csproj -c Release -p:UseSharedCompilation=false
dotnet build Test/Terraria.Dome.Combat.Verification/Terraria.Dome.Combat.Verification.csproj -c Release -p:UseSharedCompilation=false
dotnet run --project Test/Terraria.Dome.Combat.Verification/Terraria.Dome.Combat.Verification.csproj -c Release --no-build
dotnet build Test/Terraria.Dome.Combat.Protocol.Verification/Terraria.Dome.Combat.Protocol.Verification.csproj -c Release -p:UseSharedCompilation=false
dotnet run --project Test/Terraria.Dome.Combat.Protocol.Verification/Terraria.Dome.Combat.Protocol.Verification.csproj -c Release --no-build
dotnet build Test/Terraria.Dome.Hardening.Loopback.Verification/Terraria.Dome.Hardening.Loopback.Verification.csproj -c Release -p:UseSharedCompilation=false
dotnet run --project Test/Terraria.Dome.Hardening.Loopback.Verification/Terraria.Dome.Hardening.Loopback.Verification.csproj -c Release --no-build
dotnet build Test/Terraria.Dome.Combat.Placement.Loopback.Verification/Terraria.Dome.Combat.Placement.Loopback.Verification.csproj -c Release -p:UseSharedCompilation=false
dotnet run --project Test/Terraria.Dome.Combat.Placement.Loopback.Verification/Terraria.Dome.Combat.Placement.Loopback.Verification.csproj -c Release --no-build
git diff --check
```

**Expected evidence:**

- focused contract verifier：输入拒绝、原子失败无副作用、成功 tombstone linkage 均 PASS；
- loopback integration verifier：object-placement → sign update → projectile tombstone 顺序 PASS；
- 独立 placement-specific TCP loopback verifier：两个真实 session 观察到完整 footprint，并验证
  object-placement → sign update → projectile tombstone 顺序；旧 Combat loopback 仍保留；
- Simulation/Combat/Loopback 构建无新增 warning/error；
- 不运行全量回归，不把上述窄 verifier 标记为完整 Terraria parity。

## 5. 失败、回滚与安全边界

- **Invalid coordinates / occupied tiles / unsupported style:** 返回 rejection；不写任何 tile/sign，
  projectile 保持 active。
- **Duplicate sequence / overlapping footprint:** 以 deterministic key 拒绝或返回已提交结果；不重复复制。
- **Section version overflow / stale expected version:** 返回 deferred/rejected，等待下一 tick 重试；
  不截断版本，也不强制覆盖外部写入。
- **Text > 100 或 null:** 在 normalize 阶段拒绝；不调用 `Sign.TextSign`。
- **Owner inactive / projectile 已非 active:** 拒绝并记录 reason；不得伪造 tombstone。
- **Replication adapter failure:** simulation commit 不回滚为“未发生”；保留 event/cursor 可重发，
  同时不产生第二次 world mutation。
- 回滚只撤销本批次新增 command、placement system、event/packet adapter 和 verifier；不得删除或
  重置既有 `TileChangeCommand`、sign tombstone 或 projectile identity 数据。使用 `git diff` 精确
  复核后逐文件回退，禁止 broad reset/clean。

## 6. Definition of Done

- [ ] 负责人已明确批准新增原子 world-object command/commit 边界。
- [ ] style-17 request 不再直接写 `WorldGrid` 或调用 `CreateSign()`。
- [ ] `CanPlace` 覆盖完整 footprint，commit 对 tile + sign + section version 原子可见。
- [ ] 成功路径产生单一 placement event，并链接 `Expired` projectile tombstone。
- [ ] 任一失败路径无部分 world/sign mutation，projectile 保持 active。
- [ ] focused + loopback verifier 通过，Simulation/Combat build 结果已记录。
- [ ] 完整 style 表、全部 packet parity、VFX/audio 明确标记 `deferred`；批次状态仍为
  `completed_partial`，不得宣称完整 Projectile 迁移完成。
