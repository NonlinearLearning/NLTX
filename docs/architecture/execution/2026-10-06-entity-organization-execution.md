# Entity 组织迁移执行文档

日期：2026-10-07。状态：主验收 receipt 已将 B0–B9 在本文列出的当前支持 caller 与运行范围内记录为
`ACCEPTED_WITH_NAMED_LIMITS`；不表示全 NPC parity、全 projectile 类型支持或权威 NetworkServer gameplay。
本文件及 design、ledger、README 的 source-map 措辞已按 receipt 澄清。receipt 在 15:37 对当时文档快照记录 checker PASS；本轮新增的 B9 参考目录范围说明发生在该检查之后，当前文档修订 READY 交主会话重跑 checker。38 场景 Simulation matrix 使用 DLL
`621C35731250C8790A12CD55B312F5B74145B9FF1444A55A31C6F918BB8E2FDE`；真实 WorldFile controlled late-finalize
rollback/retry 使用 `5BA221555986145102FCB9274805C82355EE39BF159EED08BAA48B792E7E15B4`；当前 Simulation 窄门禁使用
DLL `0B052D8CB8E6D47A0DF4555B44249F26D1A0BE1A787BF7819CB53D71D9981E56`，其 17 source Documents/PDB map 仅为 artifact-time 对应。主验收 receipt 记录 9 项独立 NPC AI 后续 source hash drift，超出本迁移运行验收范围；它们不使该 DLL 的运行证据失效，也不表示当前源码仍等于编译输入。删除旧 identity 后
七个 Projectile mode 的 fresh 验证使用 `5C631F60797CD86E7B23A88CD8B04CF706B3DA2D993EDB75C2ADA65D7CD487B3`。
Mother Slime 的 237/11 比较差异与完整因果、未接线/存根和有限支持集作为具名限制保留。
配套 [代码设计](../2026-10-06-entity-organization-design.md) 与
[正式组织约束](../../../Context/架构设计/ECSEntity组织设计约束.md)。

本文件规定具体入口、依赖、切换、回退和验收记录方式。批次状态、单写边界和验证证据见
[本主题执行 ledger](../../migration/ledgers/2026-10-06-entity-organization-execution-ledger.md)。

正式文档单写由 Audit/documentation 会话负责：entity-organization design、execution、ledger、
统一身份 PRD 冲突协调及 README/manifest 导航。B5 单写 Projectile 生产路径、
`RuntimeProjectileStore` / adapter 与 `ProjectileCombatVerification`；B5 progress card 由 B5 更新。
B5 不拥有这些正式文档或文档导航。

## 1. 开始与批次记录

先读根 AGENTS、progress 和各主题约束，保留工作树已有修改。涉及文件移动、组件命名、
C#、副作用、基础设施及验证时分别读取对应约束。使用
[构建与验证约束](../../../Context/约束/构建与验证约束.md) 规定的 SDK 和输出边界。

实施前固定当前 NLTX 工作树及 `D:/TRbackup/Version4` 的具体文件指纹、源码行号、
已有行为支持和每个真实入口。Version4 只读；不构建旧 net40/x86 工程；不修改
`src/NSSLC.Infrastructure/分类参考/`。SS14 固定提交只作机制参考。

每个小批次应能独立审阅；提交不是验收结果。记录命令、项目、退出码、warning/error 数、
输出路径及场景观察，再升级状态。未经实际执行不填“通过”。

| 批次记录字段 | 必填内容 |
| --- | --- |
| 范围与状态 | batch id；未开始/进行中/待补证/通过；实际起止时间 |
| 源码输入 | 当前源文件、hash/版本、入口 caller/callee；confirmed/partial/unknown |
| 权威表 | 每项能力旧写源、新写源、旧读取、外部效果、提交/失效点 |
| 变更映射 | 源路径 → 目标路径，namespace/API/project reference 影响 |
| 单写切换 | 哪个纵向入口已切；兼容 adapter 的方向、有效期与退出条件 |
| 行为证据 | 原场景/目标场景、预期、实际、顺序、失败结果、测试/报告路径 |
| 构建与回退 | 精确命令/退出码/计数/输出；该批次回退操作与数据兼容边界 |

ledger 放 `docs/migration/ledgers/` 的本主题文件；运行日志与报告放
`Build/diagnostics/EntityOrganization/<batch>/`，Agent 草稿放 `.agent-workplace/`。
不要将生成物提交进源码或把文件级静态映射当行为证明。

证据分为当前源码/调用审计、领域 verifier、Simulation fixture host、实际 NetworkServer host
四类，并将存根/无生产 caller 单列为 unknown。JSON evidence 的命令、Project、exit、warning/error
计数、日志和 artifact hash 是各窄检查的事实来源；没有 successful fresh build 的旧 DLL run
不得计入。窄检查仅关闭其断言覆盖的门禁，不自动升级批次。

## 2. 批次依赖与状态

| 批次 | 当前状态 | 前置条件 | 交付目标 |
| --- | --- | --- | --- |
| B0 基线和冲突同步 | 完成（固定基线与当前 caller map） | 固定输入快照与历史冲突校准 | 523 项 input-copy/hash 与 9 个 Version4 指纹核对、PRD 生命周期措辞校准、B0 exact-argument 3600 tick 对照及当前 caller map 均有记录，详见 ledger。源责任 checkpoint 的 73 changed / 3 missing 是 Integration 的归属交付项，不能由该计数本身推断最终 owner；Mother Slime 11/237 差异保留为行为限制，不以改独立 AI 追求零差异 |
| B1 根身份和作用域 | 完成（当前支持 caller 范围） | B0 基线同步 | EntityUuid/runtime/handle、身份登记、NPC/Player 根与作用域引用已接入；generation exhaustion、WorldFile switch、reset/cleanup、旧引用失效和已支持 projection owner 均有对应源码/场景证据；边界外能力按 B3–B8 具名限制 |
| B2 组件关联与访问协议 | 通过（组件访问协议范围） | B1 基础实现 | 已验证版本化快照、可变引用投影拒绝、借用/重入、查询复用与容量数据；该状态不替代各领域与 host 门禁 |
| B3 NPC 全纵向切片 | 完成（当前 Simulation 支持集） | B2 基础实现 | 38 场景 0/1/2 Player matrix 覆盖 NetId `1,2,3,4,16,22,37,488`；B9 `0B052D…` 的 600 tick/2 Player 完整支持 NPC spatial probe 与两次 small→medium→small switch 通过；artifact-time map 核对该 DLL 的 4 组 PDB/CodeView GUID 与 17 项 source Documents。主验收 receipt 记录 9 项独立 NPC AI 后续 current-tree source hash drift，超出本迁移运行验收范围；该 DLL 的空间运行证据保持有效，不声称当前源码等于编译输入。`aiStyle 28` 不支持；Mother Slime exact-input 比较 237 项有 11 项差异且完整因果/parity 未证 |
| B4 Player 全纵向切片 | 完成（当前 Player lifecycle/switch 范围） | B2、B3 目标/伤害源解析 | 0/1/2 Player、cleanup preflight、两次 WorldFile switch、旧/新 Player reference/owner 隔离、普通复活保根和 destroy/recreate 换根均有通过证据；late-failure rollback 由 B8 的真实 WorldFile 与 Simulation owner 场景闭合 |
| B5 Projectile 全纵向切片 | 完成（Projectile type 1 / ordinary-arrow 声明范围） | B3、B4 owner/target 解析 | `ProjectileEntityState` 删除门禁通过；5C fresh DLL 的七个既有 mode 与 hit-limit tail 专项通过，Application authenticated owner/Gateway/queued-world/epoch fixture 通过；621C 38 场景及 3600 tick 运行实际覆盖普通箭 AI/hit/tile path。domain modes 不直接实例化私有 Simulation adapter；packet-27/29 disabled，其他 projectile type 不支持 |
| B6 Item/库存/掉落 | 完成（当前定义与已验证场景范围） | B4、B5 战斗掉落协作 | 17 场景 owner matrix 和 38 场景 Simulation 的 expiry/physics/partial/full-inventory/combat-drop-pickup 场景通过；coin definitions 不在 Simulation catalog，Version4 两个 GetItem Fill helper 为 stub，不扩展成完整 coin/pickup parity |
| B7 TileEntity/Leashed | 完成（已支持 TileEntity host 范围） | B3、B4；B2 runtime | 38 场景 fresh host 包含两根 TileEntity save/reload/switch、8 项 reload assertion、runtime reference 更换、TrainingDummy binding 与 sensor 状态验证；Leashed 无 production registration/remove/section caller，Version4 Place/NetPlaceEntityAttempt 为 stub，明确不属于已接线支持范围 |
| B8 两个宿主和边界收口 | 完成（声明的 host/runtime 范围） | B3–B7 | 621C 38 场景 matrix 通过；5BA 真实 WorldFile controlled late-finalize failure/rollback/retry 通过；0B Simulation actual-owner/static-immunity late rollback/retry、zero-tick、完整 600 tick/NetId 4 spatial probe/two-switch runs 通过。packet-27/29 仍 disabled、NetworkServer smoke 仅 admission boundary；Mother Slime 11/237 保留为独立行为限制 |
| B9 兼容退出及最终审计 | 完成（当前支持 caller 范围；当前文档 checker 待重跑） | B8 必需场景通过 | 三项旧声明删除；旧字面/配置/alias 审计限于生产源码与 `Test`，排除只读 `src/NSSLC.Infrastructure/分类参考/`，其中同名参考类型不属于生产声明。独立 audit artifact 明确记载该排除，主 receipt 所引 final summary 未序列化排除字段。5C 七 mode 通过；0B PDB source map 仅绑定 artifact-time 的 4 DLL/PDB GUID 与 17 项 source Documents。receipt 记录 9 项独立 NPC AI 后续 current-tree source hash drift，超出本迁移运行验收；0B 运行证据仍有效，不表示当前源码等于编译输入。旧 `WorldSession.Calendar.EntityId` 属另一领域；有限支持集、stubs、未接线 caller 与行为 parity 限制保留 |

上述是实施依赖，不能推断 System 调用图必须无环。分支准备可以独立，但同一纵向路径
不能两个任务同时切换 owner。跨领域先准备最小端口，不依靠双写等最后统一。

## 3. B0：建立可比较基线

1. 搜索 RuntimeNpcEntity、RuntimePlayerEntity、ProjectileEntityState、WorldEntityState、
   EntityIdentityComponent、EntityId、NpcInstanceId、ItemEntityRef、RuntimeEntityId 的声明、
   构造、写入、调用和引用。同时读取项目显式 Compile/ProjectReference，区分 proposed-but-used。
2. 追踪 Tools.Simulation/Program.cs 主装配、世界切换装配、Active*TickPhase；Tools.NetworkServer
   的实际 packet owner、epoch 与能力启用。分别标注已支持内容、未接线 API、未知分支。
3. 为设计映射中的 Version4 入口补充方法闭包和效果基线：NewNPC 选槽、Spawn、NewProjectile
   hydration/满容量、Update 子步、Kill/越界、NewItem/转移、TileEntity/Leashed 清理。
   存根标 unknown；完整参考只能注明新来源，不能冒充当前 Version4 原方法。
4. 记录当前 Movement/Kinematics/WorldItem 的所有 position/velocity writer；查明静态免疫、
   住房、anchor/section 和库存反向索引的清理责任。
5. 同步旧统一身份 PRD 的普通 respawn 和 Arch 强制措辞，明确与 accepted ADR/最新组织约束的
   优先级；不重写历史行为记录。该语义校准已落在 PRD：同实例普通复活保根，真实销毁重建换根；
   Arch 类型/路径仅保留为早期草案历史，不是实现约束。
6. 在相同内容目录、world 文件指纹和脚本输入下，运行受影响现有 verifier 与宿主基线，保留
   支持清单、输出及失败。基线失败先定位来源，不能把旧失败包装成迁移回归通过。

完成条件：入口/权威表没有未归属的已支持写路径；未知项具名；每个后续批次已有可运行
基线场景或明确补证任务。基线事实有变化时更新输入，不能继续按过期行号实施。

基线快照包含 523 项 `input-copy/` 文件及其 hash，捕获时文件全部匹配；较早相对快照记录
70 changed / 1 missing，属于该次捕获时点，不代表当前源码状态。ledger 另记录 2026-10-07
source-review checkpoint 的 73 changed / 3 missing 计数及当前 source/caller map。该计数是差异
统计，不等于 owner 归属；Integration 的责任归属 artifact 单独交付，本文件不据此补造逐项责任。
9 个设计内 Version4 指纹与其核对时的只读文件一致，PRD 生命周期措辞已校准。B0 exact-input
3600 tick 对照保留 Mother Slime 11/237 字段差异，详见 ledger 与 §11；支持 caller、owner 和退出
范围按 ledger 的当前 map 复核。B0 的基线/冲突同步在上述证据边界内完成。

对照输出时，位置、生命、数量、阶段、事件和错误按同样输入逐提交点比较；新增随机 UUID
不与旧槽位 ID 或另一轮 UUID 做字面相等比较。通过场景内引用关联检查“保留/替换/失效”
关系，验证身份的新规则。旧/新结果的预期差异必须具名，不能用忽略全部诊断字段掩盖回归。

## 4. B1：身份、runtime 与投影

文件：Relationships/EntityReference.cs、Share/Entity 的身份类型；WorldStorage/EntitySlotStore.cs、
EntityHandle.cs、ProjectileHandle.cs；各领域 identity/projection caller。

按以下小批次实施：

1. 在 Relationships 新增 EntityUuid、EntityRuntimeId、RuntimeEntityHandle，约束零值与作用域；
   保持 Relationships 无 EntityEcs/领域反向依赖。先编译受影响契约项目。
2. 在 EntityEcs 实现 EntityIdentityRegistry；由创建 owner 生成/登记根，宿主可观测历史内
   冲突和已退休根拒绝；世界切换仅撤销旧 runtime 的当前映射，保留所需历史。
   Handle 有 RuntimeId/local index/generation；达到最大代际退休，不能 wrap-around。
3. EntityReference 增加 RuntimeId/根类型，保留原类别 Scope；修改实际关系构造与解析。
   旧 Guid/slot 仅在显式 runtime adapter 中转换，核心拒绝不完整引用。
4. EntityIdentityComponent 成只读根投影；取消绕过 Commit 的公共独立 Create；EntityId、
   EntityIdentityState 过渡 adapter 单向读取，禁止旧/新根各自可写。
5. 建立 NpcInstanceId、ItemEntityRef 等作用域映射。为保持旧边界值所需的投影分配独立于
   EntityUuid，但不能成为新根；删除 world+slot 和固定前缀计数的根生成用途。

必需场景：零根/重复根/已退休根拒绝；同槽位释放后重建取得新根；旧 handle 失败；
跨 runtime 同 index/generation 失败；session 重建失败；代际耗尽退休；旧投影冲突与清理；
查询明确区分 Constructing/Running/Terminating。这些是新增身份行为，不只检查 Guid 非零。

回退：仅回退本批次新增契约/调用变更；恢复旧入口需要停止该 runtime 并从 DTO 重新建会话。
不能在运行中把新根反向回填成旧根或允许旧 handle 指向新实例。

## 5. B2：组件表和受控访问

文件：Share/Entity 新 runtime/store/access；新增 `Test/Terraria.EntityOrganization.Verification/`；
领域创建工厂注册处。不得先把全部聚合字段搬进一个泛型实体大对象。

1. 实现实体记录、类型表/反向关联、稳定 cell、attachment revision、owner-thread 检查。
   先支持确切类型注册；别名有实际需求和测试后再加。
2. 提供指定实体取能力及 Match<T…>；两侧关联指向同一 cell。Match 返回 handle 候选，
   查询取用时重新校验；在枚举内部表时不允许未声明结构修改。
3. 实现短期 Edit/借用，struct 原 cell 的 ref 提交；局部值提交须校验 attachment/data revision。
   class 写访问只在领域 owner 内；公开读取捕获值快照，防御复制可变集合。
4. 替换、移除、重加和 RemoveEntity 在所有配置中检查受影响借用；有借用时拒绝结构变化。
   释放后调用 reentrant owner/回调，再重新解析；不得依赖调试断言。
5. 创建不可见候选/启动/发布，动态 Attach 补齐生命周期；失败清理所有已分配关联。清理失败
   按明确不确定/隔离结果报告。Delete 逻辑失效同步完成，物理回收单独处理。

必需场景：两个相同定义实体不共享数组/class 状态；struct Edit 被 entity/query 两侧看到；
按值副本改动不提交；缺能力读取不创建；替换/移除重加使旧 revision 失效；查询中删除和重入
按协议处理；构建失败无半成品；动态 Attach 初始化一次；线程违规拒绝；释放后旧访问失败。
增加公开 class 读取不泄漏可写内部实例的场景和代码审查，承认 C# 对内部恶意 alias 的限制。

性能记录使用实际 1000 Projectile 槽规模及 NPC/Player 支持规模：读写/扫描耗时与分配，
对比相同基线。先满足行为，数据不足不声称更快；发现明显回归再优化存储实现。

回退：此批次尚未切主宿主可撤掉 runtime 接线；已经切入的切片必须整体停止并重建，不能
让旧聚合和新表同时接受写入。

## 6. B3：NPC 纵向切片

主要文件：[RuntimeNpcEntity](../../../src/NSSLC.Tools.Simulation/RuntimeNpcEntity.cs)、
[RuntimeNpcStore](../../../src/NSSLC.Tools.Simulation/RuntimeNpcStore.cs)、
[RuntimeNpcProjectileTargetSnapshot](../../../src/NSSLC.Tools.Simulation/RuntimeNpcProjectileTargetSnapshot.cs)、
[RuntimeNpcContactSnapshot](../../../src/NSSLC.Tools.Simulation/RuntimeNpcContactSnapshot.cs)、
[RuntimeNpcTrainingDummyBinding](../../../src/NSSLC.Tools.Simulation/RuntimeNpcTrainingDummyBinding.cs)、
RuntimeNpcNaturalSpawnPass、ActiveNpcTickPhase、ActivePlayerTickPhase、ActiveTileEntityTickPhase、
RuntimePlayerStore、TileEntityRemovalProbeInstaller、Program；Npc 各系统、WorldNpcLoadApi/RestoreSystem、
Projectile adapters 和 Simulation README。

1. 准备 NPC 根/slot/旧 NpcInstanceId 解析和最小位置/生命/目标快照接口；协作者先可编译。
2. 将 Hydrate 的初始组合和需要持续的 natural-spawn/outside-range 状态归 Npc 能力；
   Definition 不可变共享，实例状态独立。移除 world+slot 实例根公式。
3. 在加载、自然生成及 probe 创建上接入相同 Commit。NPC 同槽位替换产生新根，保护、
   正反选槽、住房修复和生成观察顺序保留；未支持分支明确拒绝/记录。
4. 按一次切换迁移 AI→movement→contact/hit→death/drop→release→save。当前 NPC root 已
   附加 Location/Velocity/Collider/grounded，Movement facade 作为一次调用值适配读写共享组件；
   当前空间组件和几何消费者已由 B8 的 38 场景 host matrix 与 B9 fresh 0B spatial probe 覆盖；
   下述 host evidence 仅关闭已声明支持集及断言范围，不扩展为所有 NPC AI/lifecycle 行为等价。
5. Projectile target、Player contact、TileEntity TrainingDummy、自然生成
   读取使用根/最小能力；不得把 RuntimeNpcEntity 作为跨系统关系。Projectile adapter 每次碰撞捕获
   EntityReference、slot generation 与最小碰撞值，命中提交时由 NPC owner 重新解析；Player contact
   使用逐 tick 根/值快照，TrainingDummy 使用根/slot-generation 绑定，自然生成 adapter 读取
   SpawnSnapshot。`ActivePressurePlateTickPhase` 当前只读取玩家位置，压力板没有 NPC 引用入口；
   若后续行为增加 NPC 触发，在引入该入口的批次定义其根快照和验证场景。
6. 将必要 session hydration 纳入 readiness；保存从当前组件捕获 WorldNpcState，SavedState
   只作基底。重载新根与关系修复在发布前完成。

必需场景：load/spawn/hit/save/reload 完整主线；同槽位新实例不继承旧伤害源或任何已支持的实例关系；
Slime/Guide/DemonEye 等当前支持 AI；contact、死亡掉落顺序；natural range 计时与失活；
住房与保存位置；构建失败清理；旧世界命令拒绝。已有 verifier 不覆盖的实例关联需新增场景。

父生命边界：当前 `SimulationContentSupportManifest` 要求 catalog 的 NPC NetId 与 8 项支持集完全一致，
不包含 Wall of Flesh 分段 NPC；Version4 的 `realLife` 父关系赋值属于 `aiStyle 28` 分支，且不在
`WorldNpcState` 持久化，因此 Hydrate 不合成该关系。NPC owner 已接入显式 ParentRelation 的命中路径：
按父 EntityReference、NpcInstanceId、旧 slot 与当前 generation 重解析父根；父/子 Health、Lifecycle
在同一短期 owner 调用内借用，父死亡释放关联子组并把父 NetId/位置传给掉落 adapter。`NpcDamageCombatVerification`
覆盖域层的有效父目标、陈旧身份及复用 slot 拒绝；`--npc-relation-probe` 用合成 Zombie/Blue Slime
关系验证父生命镜像、父 slot 释放/复用后的解绑和命中隔离、父死亡的组释放与掉落归属。该 probe 不表示
当前 Simulation 支持 Wall of Flesh 分段；未来扩展 `aiStyle 28` 时仍需接入真实 spawn/AI 来源。

600 tick 行为判读：旧的 `npc-behavior-components-gameplay-600-spawned-1.json` 是 NPC Behavior
组件切片期间的 host 投影，Blue Slime 仍静止；当前实现接入 Version4 `AI_001_Slimes` 的 grounded
counter/jump 分支后，固定输入末端 X 从 33520 变为 34032。按 `D:/TRbackup/Version4/Terraria/NPC.cs`
中 aiStyle 1 dispatch 与 `AI_001_Slimes` 实现，该变化符合已纳入 B3 的 Slime 行为范围；旧投影不再作为
当前 AI 的等价期望值。保留两个报告作为阶段差异证据，并以当前 600 tick 投影、AI verifier、来源行为和
Movement 切片前后 1 tick 投影作为验收证据；不声明完整 Version4 tick-by-tick 等价。

完成条件：NPC 全支持入口只有新组件表写源，宿主不再长期拥有组件。回退以整个 NPC
纵向切片为单位，运行中不进行反向同步；从原基线 DTO/input 重建旧会话。

空间 host 状态：B8 fresh 38 场景 Simulation matrix 的 `Evidence` 数组含 38 条且 `Succeeded=true`，
其 Simulation、ClockVerification、TileEntityFixture 三项 build 均 exit 0；0/1/2 Player、600 tick 空间
场景覆盖 NetId `1,2,3,4,16,22,37,488`。B9 fresh Simulation DLL `0B052D…` 另通过 600 tick、2 Player、
完整八 NPC 支持集和空间 probe，并完成 small→medium→small 两次 switch；零 tick/零 Player 另行 exit 0。
`current-rollback-profile-pdb-source-map.evidence.json` 在 artifact time 对 4 组 PDB/DLL GUID 和 17 项 source Documents
逐项核对通过。主验收 receipt 记录当前树有 9 项独立 NPC AI source document hash drift；这些后续变化在该迁移运行验收范围之外，不能将此 map 写成当前 SHA 与编译输入一致。它们不使绑定 `0B052D…` 的运行证据失效，也不扩展其 AI 证明范围。证据分别位于 `Build/diagnostics/EntityOrganization/B8/full-host-current-20261007-r1/`
和 `Build/diagnostics/EntityOrganization/B9/final-current-20261007/`；它们关闭当前支持范围内的空间/运行门禁，不声称所有 NPC AI、unsupported style 或 3600 tick 行为 parity。

一次更窄的 NetId 4 spatial/two-switch 尝试因漏传必需输入 `--spawn-npc 1` 而退出 1；该命令错误不代表空间门禁失败。随后带 required NPC 输入的 `simulation-final-spatial-required-input-two-switches-current-run.evidence.json` 在同一 `0B052D…` DLL 上运行 600 tick、2 Player、完整支持集并完成两次切换；独立断言确认两次均有 NetId 4、旧 root/reference 被拒绝、新 Player reference 可解析。

## 7. B4：Player 纵向切片

文件：RuntimePlayerEntity/Store、ActivePlayerTickPhase、PlayerLifecycle/Combat/Movement 系统、
输入/Projectile owner/世界拾取 adapters、PlayerSlotSessionAuthority 的应用边界。

1. Player 创建登记根及 runtime/slot/会话投影；账户关联不作为实例身份。拆出聚合中的
   AdvanceLifecycle/TryTakeNpcContactDamage 行为，调用既有领域系统维护不变量。
2. 将 Lifecycle/Vitals/Physics/ItemUse/Rest/Inventory 等关联到实体；接触免疫、出生点等
   持续事实明确组件归属；ShotsFired 等仅诊断用途的计数仍归宿主。
3. 一次切换 input→movement→contact→death→respawn 主线和所有空间读取；当前 Player root 已
   附加 Location/Velocity/Collider/grounded，Movement facade 读写公共组件，普通 respawn 保根。
   这些结构事实已由 38 场景当前支持范围 host matrix 覆盖；B9 0B two-switch run 进一步确认旧/新
   Player references 与 owners 的隔离。此结论限于报告中的 caller 和场景。
4. 库存 owner 以玩家根/SlotsComponent 工作；Projectile owner 由作用域 Player 投影解析，
   不能仅凭 byte slot 命中新连接玩家。死亡是否影响已有 Projectile 按领域规则保留。

必需场景：0/1/2 玩家装配，独立输入，jump/landing，contact immunity，死亡计时/复活位置与
根保留；真正销毁重建换根；断线重连旧 session/owner 失败；世界切换；库存与动作重置。
保持当前 non-authoritative 脚本参数，不把它们转换为未经实现的 server authority。

回退：Player 组件、宿主读写和 owner adapter 一起回退；停止会话重建，不保留新旧位置双写。

空间 host 矩阵与 B3 共用；38 场景 600 tick 的 0/1/2 Player 场景已实际通过。2 Player 的
PlayerCleanupProbe 覆盖借用 Item 时拒绝 Initialize，借用 Player 时拒绝 Clear/Dispose，借用 Player/Item
时拒绝 destroy root，释放后允许 retry，Player root 以新 generation 重建且 NPC root 与 World RuntimeId
保持。B9 0B 完整输入 two-switch run 也通过并记录旧 owner/reference 失效与 replacement reference 解析。
结论限于已列 Player owner/lifecycle/switch 范围，不扩展成权威多人网络行为。

## 8. B5：Projectile 纵向切片

文件：B0 快照中的旧 ProjectileEntityState、DefinitionHydrationSystem、LifecycleSystem、TickCoordinator、
IProjectileTickAdapter 的全部实现/调用；RuntimeProjectileStore；Application Projectile owner；
Infrastructure packet-27/29 与 GatewayRegistration。

1. 将 hydration 输出变初始组件组合；保持 Definition defaults、位置换算、wet/source、
   aiStyle=1 的限速、可选能力和 NeedsUUID 规则。能力不存在/关闭/默认值区分清楚。
2. 扩展现有 Lifecycle owner 协调组件/root/slot/protocol index。空槽分配失败、登记冲突、
   满容量 oldest 替换、slot/index 回滚及 generation 耗尽仍按原返回契约执行。
3. packet-27 同类型 Active 更新只改允许字段并保留根/handle；类型改变或非 Active
   hydrate 替换用新根及代际。packet-29 缺失/陈旧 no-op 与 accepted-no-dispatch 保留。
4. TickAdapter 拆成实际能力输入/访问；每个 API 明确 read/write、callback、effect 和结束点。
   Coordinator 仍按 0..999 当前槽位扫描，不捕获整帧 Projectile 列表代替。
5. 保留可变 numUpdates、continue/return、穿透/寿命/越界分支与尾部跳过；回调前释放，
   回调后重新解析原 handle；新占槽实体不能继续执行旧实例的剩余子步。
6. NPC/PVP 命中仅借用所需能力，旧 ProjectileEntityState 不再成为长期状态所有者。
   网络包仍是协议 DTO，由边界 owner 解析，不向 existing Terraria wire 增加 EntityUuid。

必需场景：同定义两实例状态独立、可选能力；1000 槽满容量替换与 tie 顺序；失败索引/slot/root
无泄漏；同类型网络更新保根、不同类型换根；跨 owner identity 与陈旧包；子步修改 cadence；
提前 continue/return；回调删除/替换自己或目标；越界保 timeLeft；寿命/penetration Kill 清理；
扫描时在已过/未过槽位创建的可见性。Simulation 当前只声明 Projectile type 1 ordinary arrow；所有
其他 Simulation 类型/adapter 组合显式拒绝。domain API 可表示的能力不得被写成 Simulation 支持集。

回退：生命周期存储、adapter/协议 owner 和 tick 同批回退；禁止将新运行中组件倒灌旧聚合。

当前 typed source 已交付：旧 `ProjectileEntityState` 文件已删除；`src/`、`Test/` 的 C# 与 csproj
静态搜索没有该类型引用。兼容退出是全仓源码 caller/signature 审计结论；API type-safety build 是补充，
没有单个 mode 能证明所有调用关系。历史 typed FixtureHost artifact
`E613A23F2449E5EF504433E1D5742BFEE8CAC599444E3DC3032A244219F7BA1E` 的 build 与 default、
identity-index、lifecycle、hydration、network、tick-coordinator 六个模式均 exit 0、0 warning/0 error。
`--damage-candidates` 修复后的专项 build/run 也通过，build 有 6 条既有 `WorldSectionState` warnings、
0 errors，DLL SHA-256 为 `BB05EB295ED20CC12CD5917117FA6A644D75A57182A1A30966472BA74A1A2813`。
这些是各自 FixtureHost/domain 范围的历史通过结果，不是 Simulation adapter/host hit PASS。

真实 Application Projectile runtime owner 的 authenticated Gateway fixture 以及 queued-world/epoch
invalidation 专项 build/run 已通过；同一 fresh DLL 的 Network default regression 也有 70 groups passed、
0 failed。删除旧 identity 后，B9 fresh DLL `5C631F60797CD86E7B23A88CD8B04CF706B3DA2D993EDB75C2ADA65D7CD487B3`
的七个既有 mode（default、identity-index、lifecycle、hydration、network、tick-coordinator、damage-candidates）
均有单独 run evidence 且通过；对应 build/run 记录位于
`Build/diagnostics/EntityOrganization/B9/final-current-20261007/projectile-*-old-identity-exit-*`。
此 owner fixture 是独立 Application+Gateway 链路，不与 `--network` domain lifecycle/apply mode 合并。
NetworkServer Projectile packet 仍禁用；不据此推断 packet-27/29 ingress 或其他 Projectile 类型支持。

### B5 verifier mode 与证据边界

以下是 7 个 Projectile mode 对应的断言映射。历史 `E613…`/damage-candidates artifacts 仍仅证明
各自当时的 FixtureHost/domain 范围；当前删除旧 identity 后的 `5C631F…` build/run 又逐 mode 通过，
详见上列 B9 evidence 与 ledger。新加入的穿透尾部断言也有独立 fresh FixtureHost build/run 记录；任何
domain mode 都不直接替代私有 Simulation adapter 的 host 断言。

| Mode | 具体调用/场景 | 证据边界 |
| --- | --- | --- |
| default | 顶层 inline assertions：disposition、有限/无限 penetration、immunity tick/reset、trail reset、collision policy、network flags、animation、typed arrow AI/movement、Magic Quiver cadence admission；末尾调用一次 `ProjectileIdentityIndexVerification.Run()` | 局部 domain assertions；不实例化真实 Simulation adapter |
| `--identity-index` | `ProjectileIdentityIndexVerification.Run()`：双向 register/read、幂等、冲突拒绝、identity 换 handle、identity+handle 同时替换、旧 generation no-op、Rebuild 原子替换/重复 key 拒绝、非法 slot/id/generation、unregister | identity index domain 行为，不等于 host packet ingress |
| `--lifecycle` | `ProjectileLifecycleVerification.Run()`：`VerifyOrderedAllocationAndGenerationReuse`、`VerifyCapacityAndIdentityRollback`、`VerifyGenerationExhaustionPreventsReplacement`、`VerifyProjectileWorldCapacityAndRootCleanup`、`VerifyOldestReplacementAndIdentityChange`、`VerifyReplacementMayRetainTheVictimIdentity`、`VerifyProtectedAndIneligiblePoolsRemainUnchanged`、`VerifyBorrowedFullPoolRejectsCreateWithoutLeakingCandidate`、`VerifyBorrowedTerminationIsNoOpAndCanRetry`、`VerifyBorrowedNetworkTypeReplacementIsNoOpAndCanRetry` | lifecycle/root/slot/index domain 行为；不覆盖 private Simulation tick adapter |
| `--hydration` | `ProjectileDefinitionHydrationVerification.Run()`：缩放几何与 center→top-left、lifetime/cadence/AI/damage、definition/UUID/capability hydration、immunity/network/trail/presentation 初值、不同 Definition 实例隔离、oldest-slot replacement reset、未知 UUID policy 与失败候选无泄漏 | hydration domain behavior；无 Simulation content host claim |
| `--network` | `ProjectileNetworkApplyVerification.Run()`：packet-27 初建、同 owner/type 更新保根、UUID omission 保留、换 type 换 generation/root、非法/未知 UUID 拒绝且无泄漏、packet-29 missing/wrong owner no-op、成功终止及重复包 no-op | 是 domain lifecycle/network apply；真实 Application authenticated owner + Gateway 是独立 fixture，不能与此 mode 合并表述 |
| `--tick-coordinator` | inline Pre/Post、slot 顺序、cadence、每次 regular Update 的 immunity、lifetime tail；具名场景：`VerifyAdapterTerminationStopsRemainingSubsteps`、`VerifyReplacementDuringCallbackStopsOldSubsteps`、`VerifyCallbackReplacementInUnvisitedSlotIsScanned`、`VerifyCallbackReplacementInVisitedSlotWaitsUntilNextPass`、`VerifyPreparationContinueAndReturnSkipUpdateAndTail`、`VerifyPreparationChangesPreserveCallbackComponentWrites`、`VerifyType640PreparationContinuesBeforeUpdateAndTail`、`VerifyUpdateContinueAndReturnSkipLifetimeTail`、`VerifyWorldBoundaryDeactivationPreservesTimeLeft`、`VerifyOwnerMovementAdapterReceivesReferenceOnlyForModeFour`、`VerifyTrailDustEffectUsesTypedRequest`；test adapters 为 Recording/Terminating/Replacing/SpawnDuringCallback/ControlFlow/PreparationMutation/OwnerMovement/TrailDust | B5 新增 `VerifyHitLimitTailCleansEntityRoot`：typed test adapter 提交末次 accepted hit 后断言旧 handle、slot、identity index 与 EntityRuntime root 清理，并确认不是 lifetime expiry。生产 coordinator 已有 lifetime tail 后 `RemainingHits == 0 → TryTerminate(HitLimitReached)` 分支；新断言已通过独立 fresh build/run。该 mode 不实例化 `RuntimeProjectileTickAdapter` |
| `--damage-candidates` | `ProjectileDamageCandidateVerification.Run()` inline：typed NPC/PVP candidate 与 collision composition、detached snapshot 不随 live Location/Collider 改变、local/player/static immunity、owner/slot gate 顺序、相同 Definition 实例免疫隔离、static immunity expiry | candidate/query domain coverage；不是 `RuntimeProjectileStore` 完整 gameplay/最终命中 host 证据 |

Simulation Manifest 与私有 `RuntimeProjectileTickAdapter` 当前只准入 Projectile type `1` / ordinary arrow，其他组合显式拒绝；此 manifest/adapter 边界不是七个 typed mode 的直接断言。`RuntimeProjectileTickAdapter` 私有于 `RuntimeProjectileStore`，实际调用 coordinator、普通箭 AI/movement、wet/tile/NPC hit 及 accepted-hit penetration commit；七个 mode 均未直接实例化它。B8 fresh build DLL `621C35731250C8790A12CD55B312F5B74145B9FF1444A55A31C6F918BB8E2FDE` 的 38 场景 matrix 与同 DLL exact-argument 3600 tick run 均通过运行；3600 run 触发真实 ordinary-arrow AI/NPC-hit/tile path。与 B0 比较的 237 项仍有 11 项 Mother Slime / NetId 16 差异，因此仅 ordinary-arrow host 路径执行及支持范围门禁通过，不宣称完整 AI parity。NetworkServer packet-27/29 仍禁用。

新增 tail assertion 已由 Coordinator 对 `Test/Terraria.ProjectileCombatVerification/Terraria.ProjectileCombatVerification.csproj` 串行 build + `--tick-coordinator` run 通过，DLL SHA `6554391BED980EAD773720A903FCF29F4C8FC6A21139976C4F48DA3C4AB71F01`。实际命令如下：

```powershell
dotnet build Test/Terraria.ProjectileCombatVerification/Terraria.ProjectileCombatVerification.csproj --nologo -v:minimal --no-restore -p:FixtureHostBuild=true
dotnet run --project Test/Terraria.ProjectileCombatVerification/Terraria.ProjectileCombatVerification.csproj --no-build --no-restore -p:FixtureHostBuild=true -- --tick-coordinator
```

FixtureHost 输出路径为 `Build/bin/FixtureHost/Terraria.ProjectileCombatVerification/Debug/net10.0/`；build 和 run
均 exit 0、0 warnings / 0 errors。精确验收记录见 ledger 中两份 `projectile-hit-limit-tail-current` evidence。

## 9. B6：Item、库存和世界掉落

文件：RuntimeItemRegistry、RuntimeWorldItemStore、RuntimePlayerInventoryOwner；Items/Instances、
WorldDrops、WorldItemComponent；PlayerInventoryCommit/Pickup/CoinMerge 的相关端口。

1. 清点 ItemState/ItemInstance/ItemStack/StackableItem 与实际 payload 路径；选定同一实例/堆叠
   权威组件，旧 dictionary snapshot 改为组件→DTO 查询，停止独立 ID 生成。
2. 世界掉落在 Item 实体附着空间/掉落/保留组件；世界槽位保存该实例引用。清掉三个现有
   可写空间表示的重复写源，保持计时、保留者与特殊掉落属性的实际支持范围。
3. 完整转移到空槽保 incoming 根；合并保 destination 根，incoming 耗尽才删；部分分拆
   新建根。同步提交量、关系和世界槽位，校验 source/destination/revision。
4. 过期和消费耗尽执行实例终止；全拾取只去掉世界能力，不能一概删根。NPC drop spawn
   失败仍撤销已分配 payload、根和能力。货币升级语义另按既有领域结果验证。
5. 保留 PickupSystem 的状态/效果结果区分和命令幂等；不得以 effect 失败假装数量回滚。

必需场景：spawn allocation 失败无残留；空槽完整转移保根；已占堆叠完整/部分合并和剩余；
满库存拒绝数量不变；分拆新实例可变状态独立；消费耗尽清引用；世界 expiry；重复命令和
效果失败不重复转移；数量守恒；相同 Guid 的跨 runtime 旧投影拒绝。当前 17 场景 owner matrix 与
621C 38 场景 host matrix 对已支持 Item definitions/拾取/掉落场景通过；coin definitions 不在
Simulation catalog，原 GetItem Fill stubs 属具名不支持范围，不阻挡已声明支持 caller 的组织门禁。

回退：transfer owner、库存关系/查询和世界存在一起回退，以快照重建；不能只换 registry
而保留旧 inventory/world refs。

owner static trace 与现有 `RuntimeItemOwnershipVerification.Run()` 的 17 场景矩阵已接受，覆盖 Item
hydration/geometry、split 与运行时隔离、inventory relation scope、full pickup relation/slot consistency、
effect retry/order、partial/exhausted merge、false/throw rollback、borrowed-root preflight、rejected pickup、
synthetic coin relation、consumption/revision、expiry 与 allocation-failure cleanup。实际 Simulation caller 为
`RuntimeWorldItemStore.Tick` → `RuntimePlayerInventoryOwner.TryPickup`，Player slot/lifecycle release 由
`RuntimePlayerStore` → `RuntimePlayerInventoryOwner.ReleaseAllItems` 进入，Item root/presence/relation 写入由
`RuntimeItemRegistry` 提交。静态 owner trace 还确认 containment 与 transfer rollback 的对应断言已存在。

修正后的 `player-item-owner-slot49-fixture-current-build.evidence.json` 与
`player-item-owner-slot49-fixture-current-run.evidence.json` 均 exit 0、0 warnings / 0 errors，使用 DLL SHA
`3E6797A2FC2943E7BDE9D6E688944357812878B474CBD7F75230D7105AA9B761`。旧 slot-0 fixture 的
`player-item-owner-current-relations-rollback-expiry-run.evidence.json` 失败保留为历史诊断；修正后的 17 场景
owner matrix 与 B8 38 场景 Simulation host matrix 在各自范围通过。`SimulationContentBootstrap` 没有 coin
definitions，Version4 `GetItem_FillIntoOccupiedSlot` 与 `GetItem_FillEmptyInventorySlot` 是旧 stub；这些是具名
支持限制，不把 owner/host 已通过场景扩大为 coin host wiring 或完整 Terraria pickup parity。

## 10. B7：TileEntity 与 Leashed

TileEntity 文件：TileEntityRecord/Store/UpdateSchedule，ActiveTileEntityTickPhase、load/save API
和锚点校验；对应已支持类型能力。Leashed 文件：RegistrationSystem、各行为/关系组件、
section 和卸载入口。

1. TileEntity 建新根，ID/anchor 双索引指向实例；Record 只保存关联/投影，不替换持久化 DTO
   来推进实时状态。捕获 DTO 与 runtime 组件分开，Tile anchor 保持坐标语义。
2. 保持当前 schedule 的按 ID 捕获、临用校验、anchor 失效/取消调度。比较 Version4 列表
   顺序另留证据缺口，不写成排序已证明原等价；补齐已支持传感器/dummy/放液等场景。
3. Leashed 沿用已有多向 registration 清理和 section 激活；EntityId 定向改根，关系双方提交
   和目标销毁保持既有领域协议；取消登记/世界卸载不能遗留 slot、section、订阅。

必需场景：TileEntity load/save/reload 新根、ID/anchor 冲突、anchor 移除后不再更新、capture
后删除不访问旧实例、更新中新建的可见时点；Leashed 同槽复用、section 失活再激活、
取消登记、目标销毁与双向关系清理、世界卸载。Version4 Place/Remove/网络存根补证单列。

回退：runtime 关联与所有 ID/anchor/section 投影共同回退；保持存档格式，通过 DTO 重建。

TileEntity corrected fixture 的 domain build/run 与 Leashed corrected fixture 的 domain build/run
通过各自窄断言。Version4 `TileEntity.Remove` 有 ID/position index 与 update list 清理实现，
`Place`/`NetPlaceEntityAttempt` 是存根；Player 两个 GetItem Fill helper 是存根；Leashed Remove、
StreamNetUpdates 与基类 NetSend/NetReceive 为空。当前支持 TileEntity 路径随后由 B8 fresh host matrix
实证 save/reload/switch（见下列 host report）；Leashed 仍没有 production registration/remove/section caller，
故明确标为未接线范围，不作为已支持运行路径的 pending 门禁。

最新 `tile-host-helper-current-pair-caller-build.evidence.json` build exit 0、0 warnings / 0 errors，
FixtureHost DLL SHA `831FCEDE4E8FD75A9F160687A5CF2BB0DDE81C9ADFBB5EAD4E8958D5E8DFF7C7`；
`tile-host-helper-prepare-current-run.evidence.json` 也 exit 0、0 warnings / 0 errors，已生成
`Build/diagnostics/EntityOrganization/B7/parallel-20261007/tile-host-fixture-current.wld`
（SHA-256 `DB9E14472323CCB1965B6E3EAEC4A12FD49DA4A95BDFA6504F245A524BF0F417`，2 TileEntities）。
这是早期 host fixture preparation 通过记录；后续 B8 fresh host matrix 对已支持 TileEntity 实际执行 save/reload/
switch，故此旧 fixture 不是当前完整验收结论的唯一依据。

新增 `tile-domain-disposed-store-reuse-current-build.evidence.json` / `-run.evidence.json` 以另一份 fresh
FixtureHost DLL `B6A61DAF7585724567B1937DF6D12851CB7D02501E5AC51C10ACAD1C479C0012` 通过 domain store dispose/reuse
窄场景（两项 exit 0、0 warnings / 0 errors）。fixture 验证 dispose 后统一 captured store access 均抛
`ObjectDisposedException`，不是以 `Count == 0` 作为诊断；17 类旧读写访问均拒绝且不分配 EntityRuntime root。
它仍仅是 domain 证据；TileEntity save/reload/world-switch 另由 B8 fresh host matrix 覆盖。

## 11. B8：宿主、加载、网络与保存收口

1. Tools.Simulation 的正常启动和切换世界均注入一个 EntityRuntime；各 Active*Phase 使用
   同一个 runtime，保留 Kernel 顺序和 owner thread。不要通过 constructor 扫描顺序调度。
2. 将实体 hydration/readiness 纳入已有 load/publication 路径；各 owner 完成后才允许普通
   查询/外部接入。失败不发布半完整世界；旧 runtime/commands 清理后新 session 不受污染。
3. 保存从领域 owner 捕获不可变快照，再交 Application DTO/存储端口/Codec；重载产生新根
   并修关系。改变根组织不要求改 `.wld` 格式或保存 runtime handle。
4. NetworkServer 已启用的玩家/世界 owner 使用相同身份边界；Infrastructure 类型不反向
   进入 Application。Projectile disabled 等实际能力状态保留，未完成权威模拟继续记缺口。
5. 网络异步请求携带连接 epoch 与作用域意图，经 CommandDrain 或已定义同步 owner 路径
   提交；迟到、断线、取消和 accepted-no-dispatch 结果不能改成自动成功写入。

必需场景：相同 fixture 的全主线 smoke、3600 tick combat/drop/pickup/death/respawn；保存
当前位置/库存/锚点并重载；连续世界切换与旧引用/命令失败；取消/加载失败不发布；协议
packet 身份/信任/epoch；组件 query 与各宿主实体访问一致。协议探针通过仅证明对应边界。

Fresh partial-candidate-borrow verifier 已证明候选失败路径清理、借用旧 session 时拒绝 `CommitPublishedSession` 退休并恢复既有 session、issued-root history 和随后成功切换。随后 B9 的真实 WorldFile controlled late-finalize failure/rollback/retry 在 fresh DLL `5BA221555986145102FCB9274805C82355EE39BF159EED08BAA48B792E7E15B4` 上通过，验证完整 WorldFile 切换中的 candidate disposal、legacy projection reproject 与成功 retry；Simulation actual-owner/static-immunity late rollback/retry 也在 `0B052D…` 上通过。对应证据为 `Build/diagnostics/EntityOrganization/B9/final-current-20261007/real-world-late-finalize-rollback-candidate-disposal-repair-current-run.evidence.json`、`real-world-late-finalize-rollback-diagnostics-current-run.evidence.json`、`simulation-owner-static-immunity-late-rollback-current-run.evidence.json` 和 `simulation-owner-static-immunity-rollback-independent-assertions.evidence.json`；rollback assertion JSON 共验证 14 个 boolean 与 8 个 count assertions。

截至 2026-10-07 的窄 host evidence：

- `LoadedWorldSession` borrowed-NPC dispose fixture build/run 通过（0/0 warnings/errors），只证明
  拒绝时不部分清理和释放后的幂等 dispose。
- 实际 Application authenticated gateway 到生产 Projectile runtime owner fixture 与 queued-world/
  epoch invalidation build/run 通过（0/0）；同一 fresh DLL 的 network default regression 70 groups
  passed、0 failed。
- 实际 NetworkServer Terraria319 TCP smoke 使用固定 small WorldFile、ephemeral port 0（实际端点
  `127.0.0.1:53278`），Hello admission Packet 3 分配 slot 0，disconnect 后进程 clean exit。NetworkServer
  build/run 各 exit 0、0 warning/error，DLL SHA-256
  `8A3DA4299A13DC95B93604C9E77FAC1633BBFE656F2D7CCF07739C7E6CB48CB0`。本次仍无 packet-27/29
  ingress，Projectile 维持 disabled；它是有限 network boundary smoke，不是权威 gameplay。
- B8 `world-session-generated-failure-import-current-build.evidence.json` build exit 0、0 warnings /
  0 errors，FixtureHost DLL SHA `3A5EBEC3B06EC4C2B380541A71412A6A26ED148C17BC766CD47B06E18EA9C7B1`。
  同 DLL `world-session-generated-failure-current-run.evidence.json` 退出码 `-532462766`，在
  `VerifyPreparationFailureCleanup` 断言 candidate cleanup 必须 dispose TileEntity projection 时失败；
  此次旧 fixture 失败保留为历史，后续独立 narrow run 见下。
- 新 `world-session-partial-candidate-borrow-current-build.evidence.json` / `-run.evidence.json` 均 exit 0、
  0 warnings / 0 errors，DLL SHA `3CC305B5E349F382A05C001D786FB72EFA3D02D2FA25CE28CF283D30974FE1E3`。
  默认 EntityOrganization verification run 通过，覆盖 generation exhaustion、disposed session 对所有 captured
  store access 抛错、真实 generated handler 在 partial prepare/commit/finalize/publication/cancel 的清理、旧 session
  存在借用时生产 `CommitPublishedSession` 拒绝退休并恢复原 session、最终 root 数为零、fresh dirty factory、真实
  issued-root history 和最终成功 switch。该窄 run 关闭这些列明的 root/session assertion；真实 WorldFile
  failure/rollback/retry 后续由 5BA run 独立验证，不以此 session fixture 代替。
- B3/B4 Simulation spatial live host probe 使用 fresh build 产物，build exit 0、0 warnings/errors，DLL
  SHA-256 `5BDEDFACD8B1E50898D15343C9A503912F5B1AB48F911D291197356D18E8014D`。同一产物 600 tick 的
  0 Player、1 Player、2 Player 三次运行均 exit 0、0 warnings/errors。0 Player 场景启用空间 probe，加载
  artifact 当时的 NPC 集合 `{1,2,3,16,22,37,488}` 和第二 Blue Slime；调用输入虽含两个 NetId 1，唯一 ID
  不含当前后来加入的 NetId 4。1 Player 场景另启用 NPC Reset preflight，证明 borrowed reset 在
  无写入时拒绝、释放后 retry 可行、Player/Item/Projectile 引用与 RuntimeId 保持、NPC local index 重用
  generation 改变且 stale reference 拒绝；2 Player 场景启用 Player cleanup preflight，验证借用 Item/Player
  时的 Initialize/Clear/Dispose/destroy 拒绝、释放后 destroy retry、Player root generation 递增而 NPC root
  与 RuntimeId 保留。完整命令、报告路径和 SHA-256 见 ledger。
- 先前两次失败 fresh build（缺 `PlayerZoneAndEnvironmentStateComponent`、缺 `PersistedNpcSlot`）保留为历史
  诊断记录；它们没有旧 DLL run。之后的 fresh build/run 和最终 621C/0B evidence 取代这些失败作为当前状态。
  B8 38 场景 summary 的 `Evidence` 数组共 38 项、`Succeeded=true`，其 Simulation、ClockVerification 与
  TileEntityFixture build 均 exit 0；它包含 load/publication/save/reload/switch 主线。B9 又以 0B 完成 zero-tick、
  NetId 4 spatial probe、required NPC 输入与两次 WorldFile switch 窄回归。

B8 当前支持范围的实际 Simulation host load/publication/save/reload/world-switch 场景已有 38 项 fresh matrix
证据；真实 WorldFile late-failure rollback/retry 由 5BA 证据单独闭合。NetworkServer smoke 与 Application
owner fixture 仍只证明各自 boundary，不外推为 projectile packet ingress 或 authority gameplay。

独立的 3600 tick Simulation combat/drop/pickup/save run `simulation-baseline-comparison-combat-3600-current-run.evidence.json`
exit 0、`Succeeded=true`，核心计数与 B0 baseline 相等：99 shots、45 accepted NPC hits、54 tile collisions、
1 death drop、1 full pickup、0 partial pickup、final `RuntimeProjectileCount=0`，save committed。该场景实际走过
`RuntimeProjectileStore` 的普通箭 AI、NPC hit 与 tile collision 路径。
但 `combat-3600-behavior-comparison.evidence.json` 比较 237 项有 11 项差异，全部位于
`RuntimeNpcStates[2]` 的 Action/Ai0/Ai1/Ai2、位置/旧位置/垂直速度、CollideY、DirectionY。早期单独 run 的
Simulation artifact SHA `03096536CC7FCB1DA1FD07B47926342F10AFDB20F1872A2B39583649ACBDE060` 与当时空间
probe DLL 不同，该记录保留为历史观察。之后 `combat-3600-exact-baseline-arguments-run.evidence.json` 在
fresh 621C full-host build DLL `621C35731250C8790A12CD55B312F5B74145B9FF1444A55A31C6F918BB8E2FDE` 上用
完全相同的 B0 arguments 成功运行；对照仍为 237 项中的相同 11 项 Mother Slime 差异。因此 ordinary-arrow
path 的 fresh host 执行有同 hash build 证据，但行为 parity gate 仍未通过。该行为差异的 source route 已命名，
逐 tick 完整因果与旧算法等价仍未证明。对照报告记录在
`Build/diagnostics/EntityOrganization/B8/parallel-20261007/combat-3600-behavior-comparison.evidence.json`；
不得把空间矩阵 artifact 当成本次 3600 run 的产物。

较早的 `5BDEDF…` 0/1/2 Player artifact 确实未生成后来纳入的 NetId 4；此为历史范围记录。最终 B8 38 场景
matrix 已覆盖支持集 NetId 4，B9 fresh `0B052D…` 的 artifact-time PDB source map 与 600 tick/two-player 完整 NPC spatial
probe 进一步核实该 DLL 构建中的 NetId 4 owner/source 和运行路径。Eye profile 的 artifact-time source map 对旧 `621C…`
仍不可追溯；`0B052D…` 的 4 PDB/DLL GUID 与 17 source Documents 在该 artifact capture time 核对通过。主验收 receipt 记录 9 项独立 NPC AI 后续 current-tree hash drift，超出本迁移运行验收；它们不使该 DLL 的 spatial run 失效，也不表示当前源码仍与编译输入相同。此结论不延伸为全 NPC AI parity。

Fresh partial-candidate-borrow default run 已通过 generated-handler partial prepare/commit/finalize/publication/cancel
清理、borrowed old-session rollback、issued-root history 与最终成功 switch 的窄断言。真实 WorldFile controlled
late-finalize failure/retry、Simulation actual-owner/static-immunity failure/retry，以及 load/publication/save/reload/
world-switch matrix 随后的独立 evidence 均通过；具体 artifact 与 assertions 见前文及 B8/B9 evidence refs。

## 12. 构建与行为验证命令

以下从 `D:/TRbackup/NLTX` 运行；SDK 由 global.json 选择。仅构建本批受影响项目，所有
构建串行。需要 restore 时先解决依赖；已构建 verifier 使用 `--no-build --no-restore`。
不 build solution，不运行 `-t:Rebuild`。实际执行的命令和结果见执行 ledger。

EntityRuntime verifier 命令：

```powershell
dotnet build Test/Terraria.EntityOrganization.Verification/Terraria.EntityOrganization.Verification.csproj --nologo -v:minimal
dotnet run --project Test/Terraria.EntityOrganization.Verification/Terraria.EntityOrganization.Verification.csproj --no-build --no-restore
```

| 批次 | 已存在的必需验证入口（新增场景另列） |
| --- | --- |
| B3 | Test/Terraria.NpcDamageCombatVerification；Test/Terraria.NpcAi.Verification；宿主 NPC/housing/save 场景 |
| B4 | Test/Terraria.Player.LifecycleInteraction.Verification；Test/Terraria.SpatialSimulation.Verification；宿主 contact/respawn/input |
| B5 | Test/Terraria.ProjectileCombatVerification：默认及 --identity-index、--lifecycle、--hydration、--network、--tick-coordinator、--damage-candidates |
| B6 | Test/Terraria.PlayerItemSpaceVerification（已有库存提交/拾取场景）；宿主 drop/pickup/partial/full-inventory/expiry；补充实例关系/幂等/效果失败场景 |
| B7 | Test/Terraria.LeashedEntityVerification；宿主 TileEntity anchor/schedule/save 场景 |
| B8 | Test/NSSLC.Application.Simulation.Verification；Test/NSSLC.Infrastructure.Network.Verification；Tools.Simulation 的 verify.ps1 |

每项 verifier 的 csproj 与目录同名。先对对应 csproj 执行单项目 build，再执行 run；Projectile
追加的每个选项分别跑一次，不重复构建。网络默认 verifier 包括 ProjectileGateway 和
PlayerSlotSessionAuthority；真实客户端测试仅在实际环境/范围需要时运行并注明客户端版本。

Projectile 示例：

```powershell
dotnet build Test/Terraria.ProjectileCombatVerification/Terraria.ProjectileCombatVerification.csproj --nologo -v:minimal
dotnet run --project Test/Terraria.ProjectileCombatVerification/Terraria.ProjectileCombatVerification.csproj --no-build --no-restore -- --tick-coordinator
```

真实 WorldFile 319 宿主验证：现有脚本参数和输出如下。执行前将两个 fixture 路径换为
实际存在且有 hash 的样本；要验收世界切换，SecondWorldPath 是必需输入。脚本自身会串行
构建宿主及 clock verifier，因此无需先重复构建。输出为本批次独立目录。

```powershell
pwsh -NoProfile -File Test/NSSLC.Tools.Simulation.Verification/verify.ps1 `
  -WorldPath '<实际 WorldFile319 样本路径>' `
  -SecondWorldPath '<第二个实际 WorldFile319 样本路径>' `
  -OutputDirectory 'Build/diagnostics/EntityOrganization/B8/simulation'
```

该脚本已有 0/1/2 玩家、pause、移动/跳跃、NPC AI、combat/drop/pickup、contact/respawn、
世界物品、TileEntity anchor、保存/重载和切换等场景，但没有自动覆盖本文全部新 identity/
borrow 不变量。B2 与各领域扩展 verifier 必须补足。最终按真实日志记录 warning/error 计数，
确认 DLL 实际在 `Build/bin/<project>/Debug/net10.0/`，不从历史文件或零退出码推断本轮通过。

## 13. B9：兼容删除门禁与最终交付

B9 在当前已声明支持的 caller、内容与宿主范围内通过；以下勾选只确认该有限范围，不把
unsupported、未接线入口或 Version4 stub 变成已实现能力。

- [x] 当前支持的 load/spawn/update/hit/remove/save/network/unload callers 已切换并登记；source/caller
  map 与每批 owner 细节见 [执行 ledger](../../migration/ledgers/2026-10-06-entity-organization-execution-ledger.md)。
  73 changed / 3 missing 是 source-review checkpoint 的差异计数；Integration 责任归属 artifact 独立维护，
  本勾选不将差异统计冒充逐项 owner assignment。
- [x] 当前实体引用与 component/query 访问共享对应 EntityRuntime cell；公开快照/投影不提供第二权威写状态。
- [x] 已支持空间、生命、库存 owner 的写路径已切到当前权威组件/owner；临时输入、DTO 与协议 projection
  保持为边界值，不作为并行实时状态。
- [x] 三项旧身份生产声明/聚合 `ProjectileEntityState.cs`、`EntityIdentityState.cs`、`EntityId.cs` 已删除；
  literal/config/alias audit 限于生产源码与 `Test`，排除只读 `src/NSSLC.Infrastructure/分类参考/`。`WorldSession.Calendar.EntityId` 是独立领域类型；参考目录中的同名 `EntityIdentityState` 不属于生产声明。
- [x] 根/runtime/generation/revision 失效、槽复用、旧引用、普通 respawn 保根、destroy/recreate 换根和跨世界
  switch 在对应 domain/host 场景通过。
- [x] 当前支持范围内关系、反向索引、owner、session 和 section 清理在指定退出/rollback 场景通过；未启用的
  packet ingress 和无 production caller 的 Leashed 路径不在支持 caller 集合。
- [x] Simulation Projectile type 1 ordinary-arrow adapter 的 AI/movement/hit/tile/penetration 与 coordinator
  子步/扫描尾部有 621C same-build 3600 host run、5C 七 mode 和独立 hit-limit tail evidence；B0 对照的
  Mother Slime 11/237 差异不掩饰为 parity。
- [x] 当前支持 Items/库存的数量、完整/部分 pickup、掉落、expiry 与 owner rollback 场景通过；coin host wiring
  与 Version4 两个 GetItem Fill helper 不支持/为 stub，明确排除。
- [x] 621C 38 场景 host matrix、5BA 真实 WorldFile controlled late-finalize rollback/retry、0B Simulation
  actual-owner/static-immunity rollback/retry、零 tick及 NetId 4 两次 switch narrow runs 均通过。
- [x] unsupported 内容、协议、存根和未接线 caller 均在本节具名；不以它们制造无界组织迁移 pending。
- [x] 本批证据的命令、artifact、退出状态与场景报告保存在 B5/B8/B9 diagnostics；本次文档改动限定在执行文档、
  README 与本主题 manifest 行。相对路径与 manifest 结构由本会话作文本级自审；主 checker 由主验收会话运行。

artifact-time source-to-artifact map 绑定 fresh Simulation DLL `0B052D8CB8E6D47A0DF4555B44249F26D1A0BE1A787BF7819CB53D71D9981E56`：
该 capture 中四组 Portable PDB GUID 匹配 DLL CodeView GUID，17 项 source Documents SHA 与当时编译输入相同，证据为
`Build/diagnostics/EntityOrganization/B9/final-current-20261007/current-rollback-profile-pdb-source-map.evidence.json`。
主验收 receipt 记录 9 项独立 NPC AI 后续 current-tree source document hash drift，属于该运行验收范围之外；因此此 map 不证明当前树 SHA 与编译输入一致。后续 drift 不使绑定该 0B DLL 的运行证据失效，也不扩展 AI 覆盖范围。它不追溯建立历史 621C profile source map；历史 621C full-host matrix 有自身 build checkpoint，来源稳定性以对应 checkpoint 实际记录范围为准。

最终支持声明限于：NPC manifest NetIds `1,2,3,4,16,22,37,488` 的已实现 handler/scenario（不含 NPC
`aiStyle 28`）；Simulation TileEntity types `0/2`；Projectile type `1` ordinary arrow；当前 Items `23,39,40`；
已启用的有限 NetworkServer admission/default behavior。Projectile packet-27/29 disabled，Simulation host
非权威。Leashed 当前无 production registration/remove/section caller；Version4 Leashed placement 与两个
GetItem Fill helpers 为 stub。coin definitions 缺失。Mother Slime exact-input comparison 仍为 237 项中 11 项
差异；source route 已命名，完整逐 tick 因果和旧算法 parity 未证。以上是具名行为/能力限制，不阻挡本次声明
范围内的 Entity 组织 caller 门禁，也不代表全 Terraria 内容或权威服务器全量行为完成。

## 14. 初稿文档交付记录

初稿于 2026-10-06 编写时，已直接阅读设计表所列 Version4 创建/更新/终止、空间、物品、
TileEntity、Leashed 片段及 NLTX 对应主宿主、组件/槽位/索引、应用和网络项目；当时尚未
实施源码或运行 C# 验证。以下是初稿阶段的文档检查记录，不代表当前批次状态。后续代码证据
见[本主题执行 ledger](../../migration/ledgers/2026-10-06-entity-organization-execution-ledger.md)。

| 本轮实际检查 | 结果与范围 |
| --- | --- |
| 初稿文档检查脚本 `pwsh -NoProfile -File .agent-workplace/verify-entity-organization-plan-docs.ps1`（2026-10-06 记录） | 退出 0；48 个本地链接、13 个源码行链接、22 个方法锚点、15 个 SHA256；manifest 391 个路径唯一；围栏和尾随空白检查通过。仅保留为初稿历史记录 |
| 主验收会话对当前旧版文档检查脚本的复核 | 退出 1；56 个本地链接、13 个源码行链接、22 个源锚点、15 个指纹、393 个 manifest 唯一路径。失败由历史 NLTX hash/current-source 比较、三个移动源码行号锚点和 planned 状态硬限制触发；私有脚本已修订，待主验收会话重跑确认 |
| 主验收会话后续、修复完成前的第二次检查 | 退出 1；57 个本地链接、15 个源码行链接、22 个源锚点、15 个指纹、393 个 manifest 唯一路径。指出三项 2026-10-06 设计指纹与 2026-10-07 B0 快照的捕获时间差异，以及已移动的 `RuntimeNpcEntity` 锚点；原历史 hash 未改写 |
| 主验收会话修复后的第二轮检查（2026-10-07） | 退出 0；57 个本地链接、15 个源码行链接、22 个源锚点、15 个指纹、393 个 manifest 唯一路径。输出明确列出三项 2026-10-06→2026-10-07 capture-time gaps；9 个 Version4 当前源码指纹和 6 个 NLTX 历史指纹按快照核验，设计文档中的历史 hash 保持不变 |
| 原 Entity 约束/SS14 研究检查脚本 | 退出 0；21 个本地链接、26 个固定提交锚点、9 个内容快照 SHA256，原导航仍有效 |
| `git -c core.safecrlf=false diff --check -- docs/README.md docs/document-manifest.tsv` | 退出 0；两份新增未跟踪文档的空白由文档脚本另行检查 |
| 全工作树 `git diff --check` | 报告现有 WorldGen.cs 变更的 6037/71686 行尾随空白；未改该源码，不计作本批文档通过结果 |
| C# 构建和行为验证 | 初稿阶段未执行；后续状态见执行 ledger |

上述通过/失败结果均为各次历史主验收会话的原始记录，不覆盖彼此，也不作为本次最终文档状态。本次最终
支持范围与证据同步见本文第 1–13 节；本会话没有运行 checker，当前 design、execution、ledger 与 README 四份文档
快照交由主验收会话重跑最终检查。
