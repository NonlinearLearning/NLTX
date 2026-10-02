# Version4 P02 Leashed Entity System 执行文档

~~~yaml
documentType: system-execution-plan
partitionId: P02
taskId: AUTH-SYS-P02
originalSessionId: 7415697df88244a19f3f4eec4c907e35
sourceReport: D:\TRbackup\NLTX\docs\system-decomposition\reports\2026-09-18-system-decomposition-authoritative-P02-leashed-entity.md
designDocument: D:\TRbackup\NLTX\docs\plans\system-decomposition\2026-09-30-version4-p02-leashed-entity-system-design.md
targetRoot: D:\TRbackup\NLTX\src\NSSLC
targetSourceRoot: D:\TRbackup\Version4
completeReferenceRoot: D:\TRbackup\无任何删减通过编译
comparisonReferenceRoot: C:\Users\shan\Downloads\ECS\space-station-14-master
executionStatus: partial-core-slice
implementationStatus: existing-partial-core-slice
verificationStatus: focused-passed-10-percent
verificationScope: focused-10-percent-core
migrationStatus: not-claimed
sourceModified: false
targetModified: true
testsRun: true
buildRun: true
staticEvidenceReview: 2026-10-01
~~~

## 1. 文档定位与执行边界

本文是 P02 System 设计的执行方案，不是行为等价证明。本轮修改了目标核心切片和 focused
verifier，未修改 Version4、完整参考项目、既有报告或输入台账；按仓库 wrapper 串行构建了
受影响项目并运行了 10% focused verifier。目标目录中其余核心切片仍作为
`existing-partial-core-slice` 记录，尚未 runtime-integrated。

P02 的目标是把 Leashed Entity 的注册、legacy slot、world section、critter 共享状态、
Walker/Jumper/Flyer 行为、Kite、Butterfly presentation、网络边界和 anchor/item 边界
整理成可审查的 System 组合。执行阶段不得按叶子组数量机械创建 System；只有在状态所有权、
读写集、生命周期和调度边界闭合后，才可以新增目标构件。

执行完成也不等于迁移成功。设计文档、执行文档、静态 API 映射、编译成功、局部 verifier
成功、兼容 facade 可调用或完整参考源码中的行为线索，都不能单独升级为行为等价、API 兼容或
迁移成功。focused verifier 只支持局部核心切片；未覆盖的关系和真实行为继续保持
partial、unknown 或 integration-review，整体 migrationStatus 仍为 not-claimed。

## 2. 固定输入与证据身份

每个执行阶段必须记录输入身份、源码哈希、查询参数和输出 artifact。不得用新生成的文档或
未来代码覆盖现有 authoritative report。

| 输入 | 作用 | 当前证据限制 |
|---|---|---|
| P02 authoritative report（docs/system-decomposition/reports/2026-09-18-system-decomposition-authoritative-P02-leashed-entity.md） | 固定 8 个叶子组、scope、缺口和结算身份 | report 已结算；不得重新 claim 或重结算原 session |
| P02 System design（docs/plans/system-decomposition/2026-09-30-version4-p02-leashed-entity-system-design.md） | 目标构件、状态矩阵、概念 API 和调度提案 | proposed，不是实现事实 |
| D:\TRbackup\Version4 | 当前 authoritative 声明、调用点和空/存根方法体 | 空方法体的行为是 unknown |
| D:\TRbackup\无任何删减通过编译 | 被裁剪行为的恢复线索 | 只能作为 partial，不能覆盖 Version4 身份 |
| C:\Users\shan\Downloads\ECS\space-station-14-master | System/查询/命令边界的比较参考 | 不证明 Terraria 行为、调度或 API 等价 |
| D:\TRbackup\Version4-cpg-export\out-dop8-interproc.sqlite | 只读 CPG Query API 数据源 | 本轮成功初始化并查询；SourceSnapshotId: null；effects、动态分派和唯一写者不闭合 |

CPG manifest 当前记录为：SchemaVersion: 1、ImportStatus: complete、Shards: 967、
Nodes: 8,166,789、Edges: 71,038,907、Diagnostics: 1,317，manifest SHA-256 为
6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364，project fingerprint 为
521CBD4C11E7EB731256C232350A54099F431D0E59EBDF3C38880281F169C11B。查询完成只说明在
给定 scope 和预算内完成，不证明运行时入口闭合、事件订阅者完整、scheduler phase 或行为等价。

当前已确认的目标入口只有 CPG 静态调用事实：`RegisterAll` -> `Terraria/Main.cs`
(`spanStart=88239`)、`UpdateEntities` -> `Terraria/Main.cs` (`spanStart=300506`)、
`Clear(bool)` -> `Terraria/WorldGen.cs` (`spanStart=185539`)。`LeashedEntity` type
surface 在选定 shard 内返回 28 个 direct members；`sectionSlot` 有一个
`AccessMode: Write`、`assignment-left-span=3658` 的 member-use。上述 callable 的
effects 均为 `partial`，缺口为 `CalleeEffectsNotExpanded`。`DrawEntities`、
`NetSectionActivated` 等部分关系存在零命中或动态边界，不能把零命中解释成“不存在调用”；
CPG `SourceSnapshotId` 为 null，必须逐项回读当前源码。

完整参考中的 LeashedEntity.cs、LeashedCritter 派生类、LeashedKite 和 anchor 行为
顺序可用于提出待观察场景，例如 slot reuse、section activation、full/partial/remove packet、
critter movement、kite wind/trail 和 item respawn；在同一行为由 Version4 或未来行为测试闭合前，
这些场景的状态必须是 partial 或 unknown。

## 3. 前置证据门槛

执行状态在所有门槛满足前保持 planned-blocked。门槛失败时只追加证据缺口，不创建新的
canonical writer。

| 门槛 | 必须闭合的问题 | 未满足时的动作 |
|---|---|---|
| 身份 | definition id、content id、运行时 entity identity、whoAmI、network slot 和 anchor id 是否分离 | 停在 E1，标 unknown |
| 权威所有权 | 每个字段的唯一写者、reset owner、持久化 owner、网络恢复 owner | 禁止 E2 之后接入 writer |
| 生命周期 | spawn、activate、deactivate、remove、clear、world unload、slot reuse 的顺序与重入 | 只允许静态 API 草案 |
| 读写分类 | Query 是否隐藏缓存、随机、lazy init、队列消费或 effect port | 将入口退回 Command/System 评审 |
| 外部边界 | ActiveSections、RemoteClient、NPC、Projectile、TileEntity、Item、persistence 的 owner | 维持 integration-review |
| 调度 | Main.UpdateEntities 与 section/network/anchor 提交的 phase、barrier、可见性 | 不接入 runtime scheduler |
| 证据 | 目标源码、CPG 查询或可追溯参考源码是否能支撑每条关系 | 保持 partial/unknown |
| 回滚 | 每个新提交点能否在旧 owner 提交前撤销，且不重复提交 | 只允许 shadow-only |

## 4. 目标文件组织提案

下表中的核心切片路径已在本轮创建；其余路径仍是未来建议。真正新增 ECS 类型前必须重新读取
ECS 文件组织、组件命名、C# 风格和副作用隔离约束，并核对 src/NSSLC 的现有目录。

| 领域职责 | 建议路径 | 约束 |
|---|---|---|
| 定义、物种和注册描述 | src/NSSLC/Component/LeashedEntity/LeashedDefinitionDescriptor.cs、LeashedDefinitionCatalog.cs | 本轮核心切片；只放稳定定义，不保存运行时实体状态 |
| 运行时身份、legacy slot、section membership | src/NSSLC/Component/LeashedEntity/LeashedEntityHandle.cs、LeashedEntityRegistrationSnapshot.cs、LeashedSectionIndex.cs | 本轮核心切片；slot/index 是可重建索引 |
| 注册、spawn/remove/clear 协调 | src/NSSLC/Component/LeashedEntity/LeashedEntityRegistrationSystem.cs | 本轮核心切片；不拥有 TileEntity/网络传输 |
| section 查询与激活命令 | src/NSSLC/Component/LeashedEntity/LeashedSectionIndex.cs | 本轮核心切片；全局 section activity 仍是外部输入 |
| critter 共享规则 | src/NSSLC/Component/LeashedEntity/ | 仍为未来边界；共享 authority 统一提交，物种 evaluators 只产出 transition facts |
| kite 与 Projectile 兼容 | src/NSSLC/LeashedEntity/Kite/ | Projectile dummy/local AI 只能经 adapter，不能成为第二 authority |
| 网络 frame 校验、anchor command、item 边界 | src/NSSLC/Component/LeashedEntity/LeashedEntityNetworkAdapter.cs、LeashedAnchorAdapter.cs | 本轮纯边界适配器；不直接持久化或双写 |
| butterfly presentation projection | src/NSSLC/Component/LeashedEntity/LeashedButterflyPresentationProjection.cs | 本轮纯 projection；不回写权威状态 |
| 执行证据 | docs/evidence/p02-leashed-entity/ | 保存查询参数、source hash、Observation tuple；不放生产源码 |

不得预建空的 Systems/、Queries/、Commands/ 或 Helpers/ 目录；只有实际边界、规模
或访问约束需要时才增加子目录。目录名表达领域能力，不能以文件顺序暗示运行时执行顺序。

## 5. 有序执行阶段

每个阶段要保存输入、输出、停止原因和回滚点。阶段结束只表示该阶段 artifact 可审查，
不表示迁移完成。所有阶段都要求单 writer，兼容期禁止新旧路径同时提交同一权威状态。

### E0：证据冻结与关系补查

**输入**

- P02 report、design、原 sessionId 和 8 个叶子组 scope；
- Version4 目标文件、完整参考文件及已记录哈希；
- CPG manifest、project fingerprint 和当前 src/NSSLC inventory。

**动作**

1. 用只读 CPG Query API 重新核对 LeashedEntity type surface、RegisterAll、
   UpdateEntities、Clear、AddNewEntity、Remove、TryGet、section 方法、
   DrawEntities、NetSectionActivated、anchor 调用点和关键成员读写。
2. API 信息不足时，直接在 Version4 中追踪声明、实现、caller/callee、事件订阅、注册和
   lifecycle 入口；查询 zero-hit 时保留 NoMatchingFactInScannedScope，不得改写为不存在。
3. 对目标源码与完整参考源码做只读差异表，分为 authoritative、CPG-partial、
   reference-partial、unknown；空方法体不视为无副作用。
4. 为每个跨分区关系记录 source/target endpoint、位置、关系种类、证据状态和未决问题。

**输出**

- evidence freeze manifest；
- P02 成员到证据类别的矩阵；
- registry/section/network/anchor 的关系补查记录；
- 需要 integration-review 的端点清单。

**停止条件**

发现 source identity 不一致、关键 caller 仍为 unknown、动态事件订阅未闭合，或完整参考
行为无法映射到 Version4 时，停止后续实现，维持 planned-blocked。

**回滚**

E0 只生成证据 artifact；错误 artifact 可修订或删除，但不得改写 report、源码、CPG 或
输入台账以消除缺口。

### E1：身份、Definition Catalog 与注册顺序

**输入**

- E0 冻结的 definition、prototype、registration evidence；
- 完整参考 LeashedEntity.RegisterAll/Register/Get 的 partial 行为线索；
- 当前项目的 definition/content ID 约束。

**动作**

1. 冻结 DefinitionKey、content mapping、null sentinel、注册顺序和重复注册策略的
   概念契约；稳定 ID、mod override、重入和热加载未知时不得给出 confirmed 结论。
2. 设计 LeashedDefinitionCatalog 与只读 LeashedDefinitionQuery。Query 只能读取已提交
   catalog snapshot，不得 lazy register、分配 slot 或返回 live prototype。
3. 设计 LeashedDefinitionBootstrapAdapter，只把旧 RegisterAll/Register 转换为一次
   bootstrap command；catalog 是唯一 definition writer。
4. 把 species prototype、kite prototype、static dummy 标为定义/兼容资源，不混入实体实例
   authority。

**输出**

- definition identity table；
- registration order 与 duplicate/error contract；
- catalog snapshot 版本和恢复边界；
- 未闭合的 mod/reflection/registration gap。

**停止条件**

无法证明稳定 ID、重复注册和 bootstrap 重入，或发现旧路径仍会修改 catalog 时，只保留
proposed API，不进入 E2。

**回滚**

移除未注册的 catalog 草案；兼容 adapter 继续指向旧注册 owner，不删除旧 registry。

### E2：Registration、spawn/remove/clear 与 legacy slot

**输入**

- E1 的 catalog snapshot；
- anchor snapshot、entity identity、section membership intent；
- 完整参考 AddNewEntity/Remove/Clear/TryGet 的 partial 顺序。

**动作**

1. 由 LeashedEntityRegistrationSystem 统一校验 definition、anchor、host ownership，
   分配或释放 whoAmI compatibility slot，提交 active/spawned/removed 生命周期。
2. 将 slot reuse、ByWhoAmI、anchor link、initial position、section membership 和
   active spawn 视为同一生命周期提交，避免各物种 evaluator 分别写入。
3. 将 Clear 设计为显式 world/session command；必须定义清理顺序、网络 remove、section
   index、slot generation 和重复 clear 语义。
4. TryGetByLegacySlot 只返回不可变 snapshot；generation、stale slot 和 alias 行为
   未闭合前不得承诺旧 slot 永久稳定。

**输出**

- identity/slot ownership matrix；
- lifecycle command 与 commit result；
- stale slot、duplicate remove、world unload 的错误契约；
- 单 writer 注册表草案。

**停止条件**

无法证明 slot 唯一写者、slot generation 或 clear 与 network/section 的先后关系，保持
integration-review，禁止新系统接管旧 slot。

**回滚**

在 lifecycle commit barrier 前丢弃 pending intent；已由旧 owner 提交的实体不重复 remove，
不清除其他 world/session 的 slot。

### E3：Section index、激活与重建

**输入**

- E2 的 entity lifecycle snapshot；
- ActiveSections.SectionActivated、RemoteClient.NetSectionActivated 的目标声明；
- 完整参考 section add/remove/compact/activate/deactivate/sync 顺序。

**动作**

1. 将 BySection、active section list、list/count/empty slot 设计为可重建索引，不能进入
   持久化 authority snapshot。
2. 由 section index owner 处理 add/remove/compact；entity registration system 只提交
   membership intent，不直接维护两个索引。
3. 将 section activation/deactivation 作为外部生命周期输入；activation 触发 spawn/despawn
   的具体 phase 必须由目标调度证据确认。
4. 设计 SyncEntitiesInSection 的网络/section 交接，但不让 section index 直接发送 packet。
5. 对 section 事件零命中、动态订阅和客户端/服务器分支保留 unknown。

**输出**

- section index API 与 rebuild contract；
- activation event adapter；
- section membership observation vectors；
- network/section cross-owner 清单。

**停止条件**

无法确认 activation/deactivation 的实际入口、section index 与 persistence 的边界，或
compact 会改变 legacy slot 语义时，不启用 canonical section writer。

**回滚**

在 section commit barrier 前丢弃 membership intent，恢复旧 section list；不得用新 index
清除未归属当前 session 的实体。

### E4：Shared Critter authority 与 Walker/Jumper/Flyer evaluators

**输入**

- E2 的实体 authority snapshot；
- critter content、random cursor、wait/state/target、position/velocity/recall 状态；
- 完整参考三类 critter 行为和目标 Version4 的实际方法体状态。

**动作**

1. 将 LeashedCritterCoreState 作为共享 authority，集中提交 wait、state、target、random
   cursor、position、velocity、NPC compatibility state 和 network dirty 标记。
2. Walker、Jumper、Flyer evaluator 只读取显式 world/tile snapshot，产出 transition facts；
   不直接写共享状态，不各自消费不可重放的 random stream。
3. Walker 先闭合 solid/liquid/falling/walking/recall；Jumper 先闭合 cooldown、reachable
   tile、collision、gravity/recall；Flyer 先闭合 target、acceleration、braking、hover、
   ground bias。每个行为按目标源码证据重新分类。
4. 将 item -> NPC content、spawn center、LCG 初始化、full/partial network payload 分成
   authority、adapter、projection 三类，避免 presentation 或 network 成为第二写者。

**输出**

- shared state writer matrix；
- 三类 evaluator 的输入/transition/output contract；
- random consumption 与 replay 记录；
- 每个行为的 confirmed/partial/unknown 状态表。

**停止条件**

目标方法体为空、tile query 或 collision 关系只有参考源码支持、或 random/lifecycle 顺序
未闭合时，只允许 shadow evaluator，不启用新 authority writer。

**回滚**

丢弃未提交 transition facts，保留旧 NPC/entity update route；禁止 evaluator 直接修正旧
state 或重复发送 network update。

### E5：Kite、Projectile adapter 与 Butterfly presentation

**输入**

- E4 的共享 authority snapshot；
- Kite wind/current/target/trail/no-wind timer/fast-forward 线索；
- Projectile dummy/local AI 和 butterfly variant/fade/opacity 线索。

**动作**

1. Kite evaluator 只产生风场、距离 remap、trail shift、collision、movement 和 local AI
   transition facts；Projectile dummy 调用经 adapter 隔离，不能成为 kite authority。
2. 设计 NearbySectionsMissing、cloud/wind 初始化、spawn position/velocity 的显式输入；
   读取 ambient world 或隐式随机的实现暂列 unknown。
3. Butterfly variant、fade、opacity 和 render-only interpolation 归 presentation projection；
   不回写 critter authority。
4. 记录 Kite/Butterfly 与 section activation、network local AI、render client/server 分支
   的 integration-review 关系。

**输出**

- Kite adapter contract；
- presentation projection contract；
- trail/opacity/network observation fixture；
- Projectile/renderer 的 owner 决策记录。

**停止条件**

无法证明 Projectile dummy、local AI 或 presentation 不会写回同一 movement/state 字段时，
保持 adapter/projection proposed，不接入 runtime。

**回滚**

关闭 Kite/presentation route，恢复旧 Projectile/NPC presentation；未提交 trail 或 local AI
intent 不进入 authority snapshot。

### E6：Network、anchor、item 与 persistence adapters

**输入**

- E2/E3 的 lifecycle/section authority；
- MessageBuffer、RemoteClient、TELeashedEntityAnchor、TELeashedEntityAnchorWithItem
  的目标声明和完整参考 payload/respawn/drop/insert 顺序。

**动作**

1. 设计 full/partial/remove frame 的 validated adapter；full frame 可以请求创建，partial
   frame 必须先校验 slot/type/anchor，remove 必须交由 registration authority 提交。
2. 将 network projection 与 transport 分离：adapter 解码并验证，authority commit，projection
   只产生发送快照；malformed packet、unknown type、stale slot 和 duplicate frame 的错误语义
   必须显式记录。
3. anchor adapter 只转换 placement、world-load respawn、despawn/respawn、item read/write、
   tile break drop、insert 和 placement message；不拥有 item storage、world persistence 或
   network transport。
4. TECritterAnchor 的 item -> makeNPC 和 TEKiteAnchor 的 item -> shoot 作为
   integration-review 端点；目标空实现或版本差异必须保持 unknown/partial。
5. 任何 snapshot/restore 仅生成 projection 草案，未确认 persistence owner 前不接管恢复。

**输出**

- packet/frame schema 与 validation matrix；
- anchor/item/persistence adapter contract；
- malformed/stale/duplicate error mapping；
- network、TileEntity、Item、NPC、Projectile 的 owner 交接表。

**停止条件**

无法证明 full/partial/remove 的可见性、异常和重入，或 anchor respawn 与 slot/section commit
顺序未闭合时，不接入实际收包、存档或 tile break 路径。

**回滚**

在 authority commit 前拒绝 frame/anchor intent，恢复旧 MessageBuffer/TileEntity route；
不得删除已由旧 owner 提交的 item 或实体。

### E7：调度、更新与跨分区集成

**输入**

- E2-E6 的 authority、adapter、projection 和 commit contract；
- Main.RegisterAll、Main.UpdateEntities、WorldGen.Clear 的目标调用点；
- 现有项目 scheduler、world/session identity 和相邻分区 owner 决定。

**动作**

1. 只把 RegisterAll 适配到 definition bootstrap，把 UpdateEntities 适配到明确的
   read/evaluate/commit barrier，把 Clear 适配到 world/session cleanup；不按源文件顺序推断 phase。
2. 依赖 DAG 只表达已确认方向：catalog -> registration -> section/entity snapshot ->
   evaluators -> authority commit -> network/presentation projections；动态事件和 cross-subsystem
   edges 标为 partial。
3. 明确每帧顺序：读取 world/tile/section snapshot、评估 transition、统一提交 shared state、
   更新 section/network dirty、再生成 presentation projection；若目标调度不同，先记录差异。
4. 约束 Clear、world unload、section deactivation、network remove 和 anchor despawn 的
   barrier 与 idempotence，避免遗留 slot、section index 或 projection。
5. 兼容期保持单向 facade/adapter；禁止旧实现和新 System 同时写同一 field、slot、section list、
   NPC/Projectile state 或 item anchor storage。

**输出**

- scheduler/barrier contract；
- cross-partition owner matrix；
- feature gate 与 shadow route；
- 生命周期、reset、unload、retry 和 multi-world 记录。

**停止条件**

任一状态出现双 writer、未解析 scheduler phase、重复 random 消耗、snapshot restore 越权、
session identity 丢失或跨分区 owner 未批准时，停止集成并保持旧 route。

**回滚**

在 pass/frame/world barrier 前关闭新 route，恢复旧 facade/adapter；只清理由当前 session/generation
产生的未提交 intent 和 projection，不清理其他 world 的 index 或 anchor。

### E8：聚焦行为验证（未来阶段）

**输入**

- 真实迁移入口和批准的 build/test/verifier 流程；
- E0 的目标/参考证据及 source hash；
- old/new Observation tuple 记录器。

**动作与观察向量**

1. Registry/slot：registration order、null sentinel、duplicate type、slot reuse、stale
   generation、TryGet、AddNew、Remove、Clear、重复清理。
2. Section：add/remove/compact、activation/deactivation、spawn/despawn、section sync、空槽
   重建、跨 world/session 隔离。
3. Critter：item/content mapping、spawn center、wait/state/target、random replay、Walker
   falling/walking/recall、Jumper reachable/cooldown/collision、Flyer target/acceleration/
   braking/hover/ground bias。
4. Kite/presentation：wind/current、no-wind timer、distance remap、trail shift、fast-forward、
   collision、local AI、butterfly variant/fade/opacity，且确认 presentation 不回写 authority。
5. Network/anchor：full/partial/remove、malformed/stale/duplicate frame、anchor respawn/despawn、
   item read/write/drop/insert、placement message、world unload 和恢复失败。
6. Integration：Main 注册/更新、WorldGen 清理、section/network barrier、NPC/Projectile adapter、
   save/load projection、cancel/retry 和 multi-world isolation。

每个场景必须保存 old/new 输入、可观察输出、状态 delta、事件/packet 顺序、random 消耗、
错误结果、source evidence、命令、stdout/stderr 和退出码。只有静态文档、CPG 查询、完整参考
源码或局部 Query 结果的场景，状态仍为 unknown 或 not-run。

**停止条件**

没有真实入口、只运行到兼容 facade、只证明编译、或只通过局部 verifier 时，不能升级
verificationStatus，也不能进入删除门。

**回滚**

关闭新 feature gate，恢复旧 adapter；保留失败 fixture 和原始输出供后续修复，不覆盖权威证据。

### E9：迁移验收与删除门（未授权）

**输入**

- E8 的完整 Observation diff；
- P02 的所有成员 owner matrix；
- 生命周期、网络、anchor、persistence、调度和回滚证据；
- 相邻分区 integration-review 结论。

**动作**

1. 检查每个成员是否有真实观察到的唯一 owner，或有明确批准的 deferred adapter；命名推断、
   完整参考补充和 zero-hit 不能作为 owner 证据。
2. 检查所有确认的 legacy entry point 是否到达新组合且没有第二 authority；检查 reset、unload、
   slot reuse、network restore 和 presentation side effect。
3. 关闭 partial/unknown/integration-review，或记录批准的长期缺口和风险；未关闭的关键缺口
   阻止迁移验收。
4. 只有行为、生命周期、网络、persistence、retry、multi-world 和 deletion gate 全部通过后，
   才能另行提出旧 writer 删除顺序；本文不执行删除。

**停止条件**

任一关键 Observation 未通过、任一字段仍有多个 writer、任何行为只有参考源码支持，或
verificationStatus 不是实际测量结果时，保持 migrationStatus: not-claimed。

**回滚**

删除门未通过时不删除旧实现；若未来获准删除后发现差异，按记录的 commit barrier 恢复旧
adapter、snapshot 和 session/generation-scoped state。

## 6. API、Command/Query 与错误契约

未来每个公开 Query、Definition、Command、System、Adapter 和 Projection 都必须明确：

- world/session/generation scope、entity identity、slot generation 和 owner；
- 输入 snapshot、坐标系、顺序标识、random stream 和可见性 barrier；
- precondition、validation error、duplicate、stale、unknown type 和 retry 行为；
- authoritative delta、唯一 commit point、dirty/notification 语义；
- cancellation、callback exception、timeout、world unload 和重复调用语义；
- network/save projection 版本、恢复失败和兼容期方向。

Query 必须在同一已提交 snapshot 上可重复，不能懒初始化、消费队列、写兼容字段、分配 slot、
接收网络、重生实体或调用 effect port。若读取入口隐藏了缓存、随机、注册、事件发布或其他
修改，必须重新分类为 Command/System，并回到 owner 评审。

Transition evaluator 可以返回候选事实，但不能提交共享 authority。Command 只在唯一 owner
的 barrier 中提交；Adapter 不得绕过验证直接写 authority；Projection 不得反向写回。

## 7. 依赖与回滚矩阵

| 阶段 | 新增状态/边界 | 依赖 | 允许回滚点 | 禁止的回滚或行为 |
|---|---|---|---|---|
| E0 | evidence artifact | report、目标/参考源码、CPG | 删除当前任务错误 artifact | 修改源码/report 消除 unknown |
| E1 | catalog/identity 草案 | E0、definition evidence | 未注册前撤回草案 | Query lazy register 或改写旧 registry |
| E2 | lifecycle/slot intent | E1、anchor snapshot | lifecycle commit 前丢弃 intent | 多个 writer 分配 slot |
| E3 | section membership/index | E2、section event evidence | section barrier 前恢复旧 index | 将 index 当 persistence authority |
| E4 | critter transition facts | E2、tile/world snapshot | authority commit 前丢弃 facts | evaluator 直接写 shared state |
| E5 | kite/presentation intent | E4、Projectile/renderer contract | route gate 前丢弃 intent | dummy/presentation 二次写 authority |
| E6 | packet/anchor adapters | E2/E3、network/TileEntity evidence | authority commit 前拒绝 frame | 未验证 frame 直接创建/删除实体 |
| E7 | scheduler/projection route | E2-E6、相邻分区 owner | frame/world barrier 前关闭 route | 新旧路径双写或跨区越权 |
| E8 | verifier fixture/log | E7、批准验证流程 | 保留失败输入并关闭 route | 用编译或局部通过覆盖行为缺口 |
| E9 | deletion metadata | E8 全量观察 | 不删除旧 writer，返回 blocked | 以 proposed 或 reference 结果宣称成功 |

## 8. 未来只读查询和验证命令模板

以下命令仅供获准执行阶段使用，本轮没有运行。执行前必须重新读取构建与验证约束，并确认
CPG manifest、source hash、目标 checkout、输出目录和会话身份。

~~~powershell
# 只读 CPG 查询模板；zero-hit 仍需保留为证据缺口
$cpgTool = 'D:\TRbackup\NLTX\.agents\skills\ecs-system\tools\CpgEvidence.ps1'
$cpgDb = 'D:\TRbackup\Version4-cpg-export\out-dop8-interproc.sqlite'
. $cpgTool
try {
  Initialize-CpgEvidence -DatabasePath $cpgDb | Out-Null
  Start-CpgEvidenceServer | Out-Null
  $entity = Find-CpgSymbols -Name 'LeashedEntity' -Kind SymbolType -SourcePath 'Terraria.GameContent/LeashedEntity.cs'
  Get-CpgTypeSurface -TypeSymbolId $entity.Items[0].Node.id -SourcePath 'Terraria.GameContent/LeashedEntity.cs'
  foreach ($name in @('RegisterAll', 'UpdateEntities', 'Clear')) {
    $symbol = Find-CpgSymbols -Name $name -Kind SymbolMethod -SourcePath 'Terraria.GameContent/LeashedEntity.cs'
    $method = $symbol.Items | Where-Object { $_.Node.filePath -eq 'Terraria.GameContent/LeashedEntity.cs' } | Select-Object -First 1
    Get-CpgCallableFacts -MethodSymbolId $method.Node.id -SourcePath 'Terraria.GameContent/LeashedEntity.cs'
    Find-CpgCallSites -MethodSymbolId $method.Node.id -SourcePath @('Terraria/Main.cs', 'Terraria/WorldGen.cs')
  }
  $slot = Find-CpgSymbols -Name 'sectionSlot' -Kind SymbolField -SourcePath 'Terraria.GameContent/LeashedEntity.cs'
  Get-CpgMemberUses -SymbolId $slot.Items[0].Node.id -SourcePath 'Terraria.GameContent/LeashedEntity.cs'
}
finally {
  Close-CpgEvidence
}
~~~

~~~powershell
# 未来完整构建/测试模板；本轮没有运行
Get-Content 'D:\TRbackup\NLTX\Context\约束\构建与验证约束.md'
$dotnetArguments = @(
  'build', '<approved-project.csproj>',
  '-m:1', '-nr:false', '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArguments
$testArguments = @(
  'test', '<approved-test-project.csproj>', '--no-build', '--no-restore',
  '-m:1', '-nr:false', '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $testArguments
<approved-behavior-verifier-command>
~~~

命令输出必须保存原始 stdout/stderr、退出码、执行时间、source hash、CPG manifest 和构建
并发身份。没有实际执行时，不能填写通过、等价或迁移成功。

## 9. 当前执行记录与结论

本轮完成的静态工作：

- 复核 AGENTS.md、P02 session contract、ECS System/Query/API splitting 和 PUA 自监督要求；
- 成功使用只读 CPG Query API 复核 `LeashedEntity` type、`RegisterAll`、`UpdateEntities`、
  `Clear` 的声明/调用点、`sectionSlot` 的一个写入事实，以及 callable 的
  `CalleeEffectsNotExpanded` 缺口；初始化结果为 `ImportStatus: complete`，manifest、
  project fingerprint、967 shards、8,166,789 nodes、71,038,907 edges、1,317 diagnostics
  已记录；
- 直接阅读 Version4 `LeashedEntity.cs` 和完整参考项目 `D:\TRbackup\无任何删减通过编译` 中
  的 registry、section、critter、kite、anchor 文件，修正了完整参考文件路径和 SHA-256；
- 将完整参考恢复出的顺序保留为 `partial` 设计证据，把 Version4 空/存根方法体、动态事件、
  CPG source snapshot 缺失和跨分区 owner 保留为 `unknown`/`integration-review`；
- 修正 `LeashedSectionIndex.Add` 为空槽优先复用，并通过 verifier 覆盖移除后复用且不增长
  logical bucket；
- 为 `LeashedEntityNetworkAdapter` 增加只产出 validated command 的结果路径，并覆盖 unknown
  definition、invalid slot、stale generation、type/section mismatch、full registration 和
  remove missing entity；该路径不直接写 authority，也未接入 transport/commit；
- 修改了目标核心切片、focused verifier 和本设计/执行文档；没有修改 Version4、完整参考、
  输入报告或 session ledger。

本轮明确未完成：

- 未运行完整行为对照；本轮只完成 focused 10% verifier；
- 未接入 Main/WorldGen/ActiveSections/RemoteClient 的真实运行时调用路径；
- 未完成 Walker/Jumper/Flyer、Kite/Projectile、Butterfly、network、anchor/item persistence；
- 未完成 scheduler barrier、multi-world、unload、异常、重连或删除门验证；
- 未重新领取、恢复或结算已完成的 P02 session；
- 不把目标目录已有代码、局部构建/verifier 输出、静态查询或完整参考源码称为迁移成功。

本轮验证记录：

~~~text
Build command:
  .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @(
    'build', 'src/NSSLC/Component/LeashedEntityVerification/Terraria.LeashedEntityVerification.csproj',
    '-m:1', '-nr:false', '-p:UseSharedCompilation=false',
    '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
Exit code: 0
Errors: 0
Warnings: 14 (既有 WorldStorage 未赋值/未使用字段)
Artifacts:
  Build/bin/Terraria.LeashedEntity/Debug/net10.0/Terraria.LeashedEntity.dll
  Build/bin/Terraria.LeashedEntityVerification/Debug/net10.0/Terraria.LeashedEntityVerification.dll

Focused verifier command:
  .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @(
    'run', '--project', 'src/NSSLC/Component/LeashedEntityVerification/Terraria.LeashedEntityVerification.csproj',
    '--no-build', '--no-restore', '-m:1', '-nr:false',
    '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
Exit code: 0
Output: PASS: P02 leashed entity catalog, slot lifecycle, section activation, compaction and network validation core
~~~

当前状态保持：

~~~yaml
status: proposed
designStatus: proposed
executionStatus: partial-core-slice
implementationStatus: existing-partial-core-slice
verificationStatus: focused-passed-10-percent
verificationScope: focused-10-percent-core
migrationStatus: not-claimed
sourceModified: false
targetModified: true
testsRun: true
buildRun: true
staticEvidenceReview: 2026-10-01
~~~
