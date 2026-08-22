# Wiring、Liquid、Chest ECS 迁移执行提案

> 状态：分阶段迁移中（2026-08-19）。阶段 0-5 的已承诺切片已有代码与可执行证据；
> Liquid 合并消费、结果 Tile commit 和运行时 NetLiquidModule 复制已通过 focused/runtime
> loopback；普通 Liquid 传播已接入由旧版 `tileSolid`/`tileSolidTop` 事实生成的 753-ID
> Tile registry；普通运行时的全局 `tileWaterDeath`/`tileLavaDeath` 接触销毁通过 Tile 命令
> commit；运行时 Liquid 高水位 Panic 调度以显式策略、固定 drain budget 和确定性恢复状态
> 接入有界队列。旧 God Object 的
> 完整行为等价、明确 excluded 行为和物理删除闸门仍未完成。
>
> 本提案只定义迁移边界、组件/系统职责、阶段写集和验收闸门；不修改 `src/`，不物理删除
> `Version4` 源文件，也不把“接口已存在”当作“行为已迁移”。

## 1. 目标与硬约束

将旧版三个 God Object 的行为族拆成可组合的 ECS 组件、确定性系统、值类型命令和不可变快照：

- `Wiring.cs`：电线传播、机关触发、压力板、门/触发器/逻辑门、泵和传送等世界机制；
- `Liquid.cs`：液体更新队列、重力传播、沉降、液体合并、Tile/环境联动和网络变更集合；
- `Chest.cs`：世界箱、银行/商店边界、物品槽、打开占用、创建/销毁、重命名和持久化。

迁移后的权威状态必须归属 `Terraria.Dome.Simulation`。`Server` 只做会话、权限和投影，
`Protocol.V1456` 只做字节编解码，客户端表现、音效和 UI 不进入 Simulation。

仓库约束：新增或改动 C# 时遵守 `约束/Google-CSharp-Style-Guide-约束.md`；所有构建/验证从
仓库根目录串行执行并带 `-p:UseSharedCompilation=false`；生成物只能进入 `Build/`。

## 2. 事实源与当前缺口

### 2.1 旧源文件证据

`D:\TRbackup\Version4物理删除了某些文件\Terraria` 目录名含“物理删除”，但三个文件当前仍可读，
本提案把它们当作只读事实源：

| 文件 | 行数 | 字节数 | SHA-256 | 主要行为族 |
| --- | ---: | ---: | --- | --- |
| `Terraria/Wiring.cs` | 2,781 | 69,871 | `3D4D4C75B7A002207029294D63554B0BF376A29588AC7A06F62A08CC6A998225` | 线色遍历、机关队列、压力板、逻辑门、泵、传送、光源/陷阱切换 |
| `Terraria/Liquid.cs` | 1,550 | 37,986 | `23C27E5B669B99FE225ECFACEBD6F5254A2BA63239B6906CEF2C070010E709B6` | 水/岩浆/蜂蜜/微光传播、队列、沉降、合并、网络变更、panic/quick 模式 |
| `Terraria/Chest.cs` | 1,299 | 28,702 | `14EAF2C87C3761E2585C71EA98D6DF2F653354207BFE726D771E5C353F2918B9` | 世界箱/银行/商店、40 槽物品、锁定/占用、放置/销毁、商店填充、持久化 |

旧类型仍大量依赖 `Main`、`Tile`、`Player`、`WorldGen`、`NetMessage`、音效和绘制。不能按
方法名逐个搬运；必须先把“读取什么、产生什么事实、谁提交副作用”固定下来。

### 2.2 当前可复用锚点

- `WorldGrid` 已有稠密 Tile 存储、section version、`TrySetLiquid` 和排序后的
  `TileChangeCommand` 提交。
- `WorldTile` 已包含 `LiquidAmount`、`LiquidType`、`FrameX`、`FrameY`；不要为了 ECS
  形式强行把每一个 Tile 变成 Arch entity。世界 Tile 继续使用稠密存储，机制和容器等
  有独立生命周期的对象才使用 ECS 组件。
- `WorldEnvironmentChange` 已有 `Liquid` 变化类别；`WorldLiquidSnapshot` 已可用于
  NetLiquidModule 投影。
- `ChestComponent`、`ChestSnapshot`、`ChestPersistentState`、`ChestReplicationCursor`、
  `ContainerInteractionValidator` 和 `DomeSimulation` 已覆盖打开占用、40 槽和一部分转移。
- 门机制已有 `DoorSnapshot`、`DoorTransition` 和消息 19 的验证/复制闭环。
- `DomeSimulationSnapshot` 已包含世界、箱子、标牌、TileEntity 和实体持久化记录。

当前明确未完成的范围包括：旧版完整 Liquid Tile/environment side effects、Wire 的未覆盖
行为族（传送器、炮、Hopper、PixelBox）、银行/商店语义、`ChestName` 客户端回写和广泛
协议覆盖。已迁移的液体传播、泵到液体跨域闭环、压力板/逻辑门、世界箱重连恢复和
NetLiquidModule PVS 投影必须以本仓库的 focused verifier 为准；`progress.md` 中较早的
“液体、wire 和 actuator 尚未支持”记录不覆盖本轮新增证据，不能被新接口掩盖或反向解释为
完整 Terraria 行为等价。

## 3. 目标架构

### 3.1 组件放置原则

领域优先目录，组件只保存状态，系统只协调规则：

```text
src/Terraria.Dome.Simulation/
  Wiring/
    Components/  Commands/  Definitions/  Systems/  Snapshots/
  Liquid/
    Components/  Commands/  Definitions/  Systems/  Snapshots/
  WorldObjects/Chest/
    Components/  Commands/  Systems/  Snapshots/
```

`WorldGrid` 是稠密世界存储和 Tile commit 边界；Wire/Liquid 的大规模网格数据使用 section
级数组或 world snapshot，不创建数百万个空实体。Chest、触发器、泵、逻辑门等有身份、占用或
生命周期的对象使用 Arch entity 组件。

### 3.2 Wiring 组件与系统

| 组件/定义 | 状态职责 | 备注 |
| --- | --- | --- |
| `WireNetworkComponent` | tile 坐标、四种线色 mask、遍历版本 | 不保存全局 `_wireList` |
| `MechanismComponent` | 机关类型、激活状态、冷却、来源 | 门/陷阱/灯等共享能力 |
| `PressurePlateComponent` | 感应范围、触发条件、去抖 tick | 玩家/NPC/物品过滤必须显式定义 |
| `ActuatorComponent` | 当前启用状态、受控 Tile 集合 | 只发 Tile 命令，不直接改 Grid |
| `TriggerComponent` | 触发器类型、目标机制 ID、一次性标记 | 目标不存在时报告拒绝原因 |
| `LogicGateComponent` | 输入灯集合、逻辑运算、输出状态 | 逻辑门 pass 必须有确定性排序 |
| `PumpComponent` | 输入/输出坐标、容量、冷却、拥有者 | 只产生 `LiquidTransferCommand` |
| `WireTraversalStateComponent` | 当前 tick 的有界队列/访问集 | 每 tick 清空，不进入持久化 |
| `MechanismDefinition` | 类型、耗时、Tile 影响、事件类型 | 只读注册表，拒绝未知类型 |

系统顺序：

1. `WiringInputValidationSystem`：验证来源玩家、可见 section、范围、线色和每 tick 预算。
2. `PressurePlateDetectionSystem`：从玩家/NPC/世界物品快照生成触发事实。
3. `WireTraversalSystem`：按 `Sequence -> WireColor -> X -> Y` 遍历，限制节点数和深度。
4. `LogicGateEvaluationSystem`：消费稳定输入，生成机制激活命令。
5. `MechanismActivationSystem`：去重、冷却和一次性规则，产生门/灯/陷阱/执行器命令。
6. `ActuatorCommandSystem`：把受控 Tile 变化转成 `TileChangeCommand`，禁止直接写 Tile。
7. `PumpCommandSystem`：把泵动作转成 `LiquidTransferCommand`，禁止调用 `Liquid.Update()`。
8. `WiringEventProjectionSystem`：从 committed 事实生成门、机关和兼容事件快照。

第一批只覆盖：一条线色、压力板 -> 门/灯、基础执行器和有限泵。逻辑门、传送器、炮、
PixelBox、Hopper、特殊光源和所有 `AI`/音效行为列为后续行为族，不能用 no-op 伪装迁移。

### 3.3 Liquid 组件与系统

现有 `WorldTile.LiquidAmount/LiquidType` 是权威单元状态，新增组件只承载调度和规则状态：

| 组件/定义 | 状态职责 | 备注 |
| --- | --- | --- |
| `LiquidWorldStateComponent` | 最大队列、tick 预算、quick/settle/panic 状态 | 替代旧静态计数器 |
| `LiquidUpdateQueueComponent` | 有界待处理坐标、重试次数、来源序号 | 不允许无界 `Queue<Point>` |
| `LiquidDirtySectionComponent` | section dirty version 和网络合并集合 | 与 PVS/section cursor 对接 |
| `LiquidRuleDefinition` | 水/岩浆/蜂蜜/微光的重力、合并和危险规则 | 只读注册表 |
| `LiquidSourceComponent` | 世界生成/泵/Tile 变化产生的来源 | 记录来源命令序号 |
| `LiquidMergeComponent` | 合并 Tile 类型、待提交 frame 变化 | 只产生 Tile 命令 |
| `LiquidReplicationSnapshot` | 坐标、amount、type、revision | 复用或扩展 `WorldLiquidSnapshot` |

系统顺序：

1. `LiquidInputSystem`：消费 Tile、泵和生成阶段的液体来源，校验坐标和预算。
2. `LiquidPropagationSystem`：按固定邻居顺序执行下落/横向传播，写入候选变化。
3. `LiquidSettleSystem`：处理延迟沉降、重试和卡住诊断，不直接写 Grid。
4. `LiquidMergeSystem`：按定义计算水/岩浆/蜂蜜/微光合并和 Tile 联动。
5. `LiquidCommitSystem`：排序并提交 `LiquidChangeCommand`、`TileChangeCommand`，递增受影响 section。
6. `LiquidReplicationSystem`：合并同一 tick 的网络变化，生成 `WorldLiquidSnapshot` 列表。

`panicMode`、`quickFall`、`quickSettle` 必须变成显式策略输入或状态，不得复制为可被任意
调用方改写的 static flag。当前已验证普通运行时 Panic：高水位连续观察触发、Panic drain
budget 与恢复阈值均由不可变策略定义。生成期 `QuickWater`、动态 Tile 覆盖与完整
`UpdateLiquid` 仍是独立 partial 行为。液体系统不得读 `Main`, `WorldGen`, `NetMessage` 或旧
`Tile`。

### 3.4 Chest 组件与系统

当前 `ChestComponent` 已包含身份、坐标、40 槽和 opener。迁移采用兼容性拆分，不立即破坏
现有调用形状：

| 组件 | 状态职责 | 迁移策略 |
| --- | --- | --- |
| `ChestIdentityComponent` | 稳定 ChestId、坐标、section | 从现有 `ChestComponent` 投影 |
| `ChestInventoryComponent` | 有界槽位、容量、ItemStack | 第一阶段固定 40 槽，保留扩容接口 |
| `ChestAccessComponent` | 当前 `PlayerHandle`、锁、访问版本 | 替代 `_chestInUse` 静态集合 |
| `ChestDefinitionComponent` | 世界箱/银行/商店类型、命名上限 | 商店库存规则进入定义注册表 |
| `ChestRevisionComponent` | 内容、占用和定义变更 revision | 驱动复制和持久化 |
| `ChestPresentationComponent` | 帧、打开动画等可选投影 | 不参与权威交易 |

系统顺序：

1. `ChestIndexSystem`：从导入/放置 Tile 建立坐标到稳定 ID 的索引，拒绝重复坐标。
2. `ChestOpenSystem`：校验范围、可见 section、锁和独占 opener，发出完整快照投影。
3. `ChestTransferSystem`：在当前 opener、槽位、ItemDefinition 和容量检查通过后原子转移。
4. `ChestCloseSystem`：断线、距离和显式关闭都释放 opener，并递增 revision。
5. `ChestMutationCommitSystem`：提交放置、销毁、重命名和库存变更；禁止中途写网络。
6. `ChestPersistenceSystem`：从组件生成 `ChestPersistentState`，加载时先构造候选快照。
7. `ChestReplicationSystem`：消费不可变 `ChestSnapshot` 和 `ChestReplicationCursor`，投影消息 31/32/33/34/69。

银行和商店不能复用“普通世界箱”语义：银行的账户所有权、商店的只读/购买规则必须是
独立验证策略；第一批执行范围只承诺世界箱 40 槽和已有协议路径。

## 4. 跨域数据流与 Tick 顺序

所有客户端输入都遵循：`Protocol decode -> Server validation -> typed command -> Simulation systems -> deterministic commit -> snapshot -> replication`。

建议的权威 tick 顺序：

1. `InputDecode/Validation`：门、箱、Tile、Wire 请求先验证身份、范围、PVS 和预算。
2. `ChestOpen/Close/Transfer`：容器命令先锁定 opener，交易结果进入 revision。
3. `PressurePlateDetection`：从本 tick 的玩家/NPC/物品快照产生触发事实。
4. `WireTraversal -> LogicGate -> MechanismActivation`：只产生机制、Tile、门和泵命令。
5. `LiquidInput -> Propagation -> Settle -> Merge`：消费泵/Tile 来源，生成液体候选变化。
6. `TileChangeCommit/LiquidCommit/ChestMutationCommit`：按序号、坐标、类型稳定提交。
7. `WorldRule/Physics/Combat`：现有系统继续运行，读取已提交世界快照。
8. `SnapshotProjection -> Section/PVS Replication`：只从不可变快照编码，不读可变 ECS storage。

跨域唯一允许的直接依赖是值类型命令：Wire -> `LiquidTransferCommand`、Wire ->
`DoorTransitionCommand`、Liquid -> `TileChangeCommand`。禁止 Wiring 直接调用 Liquid/Chest
方法，禁止 Liquid 直接触发网络，禁止 Chest 直接改 Tile。

## 5. 分阶段执行与并行写集

### 阶段 0：证据冻结和行为覆盖表

**写集**：`docs/research/2026-08-18-wiring-liquid-chest-source-manifest.md`、
`docs/research/2026-08-18-wiring-liquid-chest-coverage.md`、
`docs/research/2026-08-19-wiring-liquid-chest-member-coverage.md`、
`Build/diagnostics/*`。

记录三文件哈希、成员索引、旧类型引用、协议 case、持久化字段和行为状态：`planned`、
`partial`、`verified`、`excluded`、`blocked`。每个 excluded 必须写原因和替代边界。

**闸门**：清单可重复生成；任何未识别的旧方法不能计入迁移完成率。
`2026-08-19-wiring-liquid-chest-member-coverage.md` 是冻结成员索引的逐项状态账本；
`Build/diagnostics/2026-08-19-wiring-liquid-chest-member-index-audit.md` 记录索引复核，
`Build/diagnostics/2026-08-19-wiring-liquid-chest-legacy-reference-compile-audit.md`
记录 Stage 6 的非编译 LegacyReference 调用审计。

### 阶段 1：冻结共享契约

**写集**：

- `src/Terraria.Dome.Simulation/Commands/LiquidChangeCommand.cs`
- `src/Terraria.Dome.Simulation/Commands/LiquidTransferCommand.cs`
- `src/Terraria.Dome.Simulation/Commands/MechanismActivationCommand.cs`
- `src/Terraria.Dome.Simulation/World/WorldTile.cs`（仅增加已批准的线/机关字段）
- `src/Terraria.Dome.Simulation/Snapshots/DomeSimulationSnapshot.cs`
- `src/Terraria.Dome.Simulation/Tick/SimulationCommandQueue.cs`

先写契约测试，再让三条迁移线共享：坐标边界、序号、容量、revision、未知定义拒绝和
确定性排序必须冻结。此阶段不实现完整传播或机关行为。

### 阶段 2：Chest 线（低风险先闭环）

**写集**：`WorldObjects/Chest/Components/*`、`WorldObjects/Chest/Systems/*`、
`WorldObjects/Chest/Commands/*`、`Snapshots/ChestSnapshot.cs`、现有
`ChestComponent.cs`/`DomeSimulation.cs` 的适配层、WorldObjects verifier 和 loopback verifier。

顺序：拆分槽位/占用/版本状态 -> 命令化打开/关闭/转移 -> 持久化候选加载 -> 复制游标 ->
断线重连验证。只有世界箱路径达到回归门槛后，才单独处理银行和商店。

### 阶段 3：Liquid 线（稠密网格 + 有界队列）

**写集**：`Liquid/Components/*`、`Liquid/Definitions/*`、`Liquid/Systems/*`、
`Liquid/Commands/*`、`WorldGrid.cs`、`WorldEnvironmentChange*`、液体 verifier。

顺序：单源下落 -> 横向传播 -> 四种液体边界 -> 合并 Tile -> 泵输入 -> 队列预算/卡住诊断 ->
网络变化合并。每一步都用同一 snapshot/input/tick 做 hash 对比；不以“水能动”作为等价证明。

### 阶段 4：Wiring 线（事件图 + 机制命令）

**写集**：`Wiring/Components/*`、`Wiring/Definitions/*`、`Wiring/Systems/*`、
`Wiring/Commands/*`、门/TileInteraction 适配层、Wire verifier。

顺序：单色线遍历 -> 压力板 -> 门/灯 -> 执行器 -> 泵 -> 逻辑门。每个行为族要有触发输入、
遍历预算、输出命令、冷却和 replication 证据；未覆盖的 `Teleport/Hopper/Cannon/PixelBox`
必须显式拒绝或列为兼容隔离，不得吞掉请求。

### 阶段 5：跨域集成和协议投影

**写集**：`Terraria.Dome.Server/Validation/*`、`Replication/*`、
`Terraria.Dome.Protocol.V1456/Packets/*`、dispatcher/catalog、三域 loopback verifier。

重点覆盖消息 19、31-34、69、82/NetLiquidModule、108/109/110；协议只编码 server-owned
snapshot，客户端不能伪造 opener、液体结果、线色、机关状态或库存。重复/乱序/超预算输入
必须在 Simulation mutation 之前拒绝。

### 阶段 6：下线与物理删除闸门

物理删除 `Version4` 文件不属于本提案的默认动作。只有在以下条件全部满足时，另开独立提交
处理旧工程编译项或旧文件删除：调用点清单为空或只剩批准的兼容 facade；coverage 没有未解释
的 core `blocked`；持久化往返、重连、PVS、协议字段和 deterministic replay 均有证据；旧
`Main`/`Tile`/`NetMessage` 引用从 Simulation 清零。删除动作失败或证据不足时只回滚注册项，
不回退无关工作树。

当前 `LegacyReference` 编译审计只证明其中的旧调用不参与
`Terraria.WorldFile.V319` 和 `Terraria.Dome.Protocol.V1456` 的 Compile 项；它不是物理删除
批准，也不能替代本阶段的完整调用点、兼容 facade 与回滚审查。
WLD 导入链的保留边界记录在
`Build/diagnostics/2026-08-19-stage6-world-import-facade-audit.md`：
`DomeServer -> WorldCompatibility -> WorldFile.V319 -> DomeSimulationSnapshot` 是批准保留的
文件导入 facade，不属于 Simulation 的旧 God Object 运行时依赖。
按章节的当前完成审计记录在
`Build/diagnostics/2026-08-19-wiring-liquid-chest-completion-audit.md`；它明确区分已验证的
选定切片、partial/excluded 行为和未满足的物理删除闸门。

## 6. 验证矩阵

最小验证集合（每阶段按实际项目存在性执行，均从仓库根目录串行运行）：

```powershell
dotnet build .\src\Terraria.Dome.Simulation\Terraria.Dome.Simulation.csproj -p:UseSharedCompilation=false
dotnet run --project .\Test\Terraria.Dome.TileInteraction.Verification\Terraria.Dome.TileInteraction.Verification.csproj -p:UseSharedCompilation=false
dotnet run --project .\Test\Terraria.Dome.WorldMechanics.Verification\Terraria.Dome.WorldMechanics.Verification.csproj -p:UseSharedCompilation=false
dotnet run --project .\Test\Terraria.Dome.WorldMechanics.Loopback.Verification\Terraria.Dome.WorldMechanics.Loopback.Verification.csproj -p:UseSharedCompilation=false
dotnet run --project .\Test\Terraria.Dome.WorldObjects.Verification\Terraria.Dome.WorldObjects.Verification.csproj -p:UseSharedCompilation=false
dotnet run --project .\Test\Terraria.Dome.WorldObjects.Loopback.Verification\Terraria.Dome.WorldObjects.Loopback.Verification.csproj -p:UseSharedCompilation=false
dotnet run --project .\Test\Terraria.Dome.Completion.Verification\Terraria.Dome.Completion.Verification.csproj -p:UseSharedCompilation=false
```

新增 verifier 至少断言：

- Wire 遍历在相同输入下顺序、预算、去重和冷却一致；非法来源不产生命令；
- Liquid 传播、合并、泵转移和卡住恢复不越界，重复 tick 不重复提交；
- Chest opener 独占、槽位/容量/ItemDefinition 校验、原子转移、断线释放和重连恢复；
- 所有 section revision、replication cursor 和 persistence snapshot 只在 commit 后变化；
- 协议字节逐字段匹配，server-owned 字段不会接受客户端回写；
- Simulation 不引用旧 `Wiring`、`Liquid`、`Chest`、`Main`、`NetMessage`、XNA/UI 类型。

每个阶段记录命令、退出码、警告/错误、fixture、输入 snapshot hash、输出 hash 或协议字节；
绿色 verifier 只能证明声明的切片，不能证明完整 Terraria 行为等价。

## 7. 风险、停止条件和回滚

立即停止当前阶段并保留证据的情况：

- 系统绕过 command/commit 直接写 `WorldGrid`、Chest 或 socket；
- Wire/Liquid 队列无界增长，或 panic/quick 状态可由任意调用方篡改；
- 客户端输入可以改变 server-owned opener、库存、液体 amount/type 或机关状态；
- 大删除没有 source-derived 方法覆盖和目标引用证据；
- deterministic replay 在没有记录规则变更时发散；
- 未知旧行为被静默映射为空操作。

回滚按阶段进行：撤销新系统注册和适配器，保留 typed command/snapshot 契约；恢复原有
Chest/Door/Tile 投影路径；不使用 `git reset --hard`、不清理整个 `Build/`、不覆盖用户无关改动。

## 8. 完成定义

这次迁移只有在以下条件同时满足时才可称为“完成”：

1. 三个源文件的成员/行为覆盖表可追溯，所有核心条目都有 `verified` 或书面 `excluded`；
2. Wiring、Liquid、Chest 均有组件、系统、命令、快照、持久化和协议投影边界；
3. 跨域只通过值类型命令通信，tick 顺序和 commit 排序是显式且可重放的；
4. Chest 重连、Liquid deterministic replay、Wire loopback 和协议恶意输入都有可执行证据；
5. Simulation 不依赖旧 God Object 或客户端框架，Server/Protocol 不暴露 Arch 可变实体；
6. 物理删除旧文件前，旧工程引用、兼容 facade 和回滚点均已单独审查并批准。

在上述条件满足前，准确状态只能写为“分阶段迁移中”或“某行为族已验证”，不能写成“已完成
Terraria Wiring/Liquid/Chest 迁移”。
