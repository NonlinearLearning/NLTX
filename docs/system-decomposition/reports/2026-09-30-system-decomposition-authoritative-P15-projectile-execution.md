# Version4 P15 Projectile ECS 拆分执行文档

## 执行状态

| 字段 | 值 |
| --- | --- |
| `executionPlanStatus` | `proposed` |
| `implementationStatus` | `partial`（新增 packet 27 payload 编解码、packet 29 payload 解码；lifecycle 与 ordered tick coordinator 仍是局部切片） |
| `verificationStatus` | `not-run`（P15 行为、集成与迁移验收未执行） |
| `existingFocusedVerifierStatus` | `partial`（lifecycle、hydration、ordered tick coordinator 有历史 focused verifier 通过记录；本次 packet codec 无 verifier，P15 集成未运行） |
| 最近更新 | `2026-10-03` |
| 关联设计 | [P15 ECS 拆分设计](2026-09-30-system-decomposition-authoritative-P15-projectile-design.md) |
| 权威输入 | [P15 权威静态拆分报告](2026-09-18-system-decomposition-authoritative-P15-projectile.md) |
| session | 沿用已结算的 `4426a5ad1b824c2490a832c3bb6726b8`；本文不 claim/settle 新分区 |
| `outputReport` | 本执行文档；设计文档见 [P15 ECS 拆分设计](2026-09-30-system-decomposition-authoritative-P15-projectile-design.md) |
| `sourceModified` | `true`（`src/NSSLC` 的 lifecycle、ordered tick coordinator 局部切片；`src/NSSLC.Infrastructure/Network` 的 packet 27/29 payload codec） |
| `reference project` | `D:\TRbackup\无任何删减通过编译`（只读完整参考源码） |
| 最近文档更新 | 2026-10-03：在 Network Infrastructure 增加 packet 27 payload 编解码和 packet 29 payload 解码；未接入 PacketGateway 或 lifecycle 提交；按用户要求未运行 build/test，P15 整体仍为 proposed/not-run |

本文规定将 P15 proposed 设计落入 `D:\TRbackup\NLTX\src\NSSLC` 的执行次序与验收门槛，并记录 packet payload codec、lifecycle 的 proposed owner 边界、ordered tick coordinator 的 immunity/lifetime 局部切片。`ProjectilePacket27Codec` 已实现 packet 27 body 编解码，`ProjectilePacket29Codec` 已实现 packet 29 body 解码；它们尚未接入 PacketGateway、会话授权策略或 lifecycle 提交。`TryApplyNetwork`/`TryTerminateNetwork` 仍是 lifecycle owner 的局部状态边界，coordinator 也尚未接入权威 Main 的逐槽调度壳。不视为已接入 Version4 完整运行路径。普通 spawn 输入只携带 owner/type/参数、在 lifecycle 选定 free/oldest slot 后派生本地 identity 和已知条件 UUID，仍是待闭合契约。该计划不表示 Version4 完整 `SetDefaults`、真实类型数据、网络授权/relay、Main 调度接线或 Kill 行为已迁移。每阶段必须先满足前序 gate；任何 `unknown`、`partial` 或 `evidence-gap` 未解决时，应停在对应阶段并记录具体缺证据项，而非写成迁移成功。

## 执行约束

- 以 `D:\TRbackup\Version4` 的已确认源码 revision 为目标语义；`D:\TRbackup\无任何删减通过编译` 只作完整行为线索。两者 `Projectile.cs`、`Main.cs` SHA256 不同，本轮未在参考树运行构建，不能仅凭其目录名/完整度套用实现。
- P15 范围锁定在 17 个叶子组和 126 个成员。发现跨分区关系时登记 integration handoff，不悄悄扩张 P15 claim。
- 修改前检查 `src/NSSLC` 的局部 `AGENTS.md`、ECS 文件组织、组件命名、C# 风格和副作用隔离约束；每项只在实际涉及的主题读取并遵循。
- 一个权威字段/索引一个 commit owner。多个 System 可协作，但不得同时直接写同一权威状态。
- `GetNextSlot`、`FindOldestProjectile` 不能在独立 Query 中预选后再提交分配；slot 选择、generation 校验、identity 建立须处于同一 lifecycle Command。
- 保留 Main 升序 slot pass 和单 Projectile 更新内序。不得把逐对象时序改成跨对象全局 AI/collision/combat 多轮。
- 实现每个阶段的 focused verification 之前，遵守仓库 `Context/约束/构建与验证约束.md`。本计划不是对本轮运行 build/test 的授权或记录。

## 阶段与退出门槛

| 阶段 | 输入与工作 | 产出 | 退出条件 |
| --- | --- | --- | --- |
| 0. 固定证据基线 | 确认 Version4 commit/源码 hashes；记录完整参考项目 solution、project 和关键源码 hashes；对照 P15 inventory、Main/Projectile 生命周期点、packet 27/29 和外部调用；复查 CPG manifest、`SourceSnapshotId` 及 `SetDefaults` 调用边 | 源码基线记录、CPG/source 差异清单、stub 差异表、`SetDefaults` method hash、126 成员 read/write 初表 | 目标 revision 可重复识别；`NewProjectile -> FindOldestProjectile` 与 `NewProjectile -> SetDefaults` 的索引/源码差异有明确追踪；无法绑定的 API 关系明确标 `evidence-gap`，不当作 negative evidence |
| 1. 状态 owner 与契约 | 对每个 P15 成员列出 writer/reader/lifecycle、持久化和网络来源；盘点 `src/NSSLC` 现有组件、catalog、Query、System 与 scheduler | 单写者矩阵、component schema 草案、Commands/Queries/Adapters 接口契约、冲突/跨域 handoff 清单 | 每个需迁移字段有唯一拟定 owner；owner 未知的字段仍标 `unknown` 并阻止其切换 |
| 2. 定义与 lifecycle | 以 Version4 `SetDefaults` 为目标逐字段对齐 catalog 与实例状态；分别建立 type-default hydration、spawn 输入/source modifier、packet receive/apply 契约与 recycle reset；迁移 slot、owner、identity、UUID、活动与索引不变量；单独归属 Main 启动时的 `projHostile`/`projHook` 派生表 | field-by-field hydration 映射及上下文依赖、`SpawnProjectileCommand`、声明级 `ApplyProjectileNetworkCommand`/`TerminateProjectileCommand`、索引重建策略 | free-slot/full-slot/oldest-slot 与默认值重置、spawn 覆盖顺序均有源码依据；普通 spawn 在 slot commit 后派生 `identity=slot`，按 `NeedsUUID[type]` 初始化 UUID；packet 27/29 payload codec 已实现但尚未接入 lifecycle；PacketGateway framing/dispatch、授权与副作用 handoff 未闭合；slot 分配无 Query/Command TOCTOU；终止幂等性仍待验证 |
| 3. 有序调度壳 | 在权威 Main 调度点接入逐槽位 coordinator；记录 Pre、loop index、ascending pass、Post；先维持兼容委派，不改变 AI/collision/combat 算法 | per-slot scheduler seam、事件顺序表、旧路径到新 owner 的委派映射 | 单槽位内序、extraUpdates、当前帧 spawn 可见性和 slot 顺序均有源码/trace 对照；无全局多轮重排；旧入口仍可追踪 |
| 4. AI 与运动 | 按能力迁移 AI/motion；显式传递随机/时间上下文；逐个处理目标源 stub 与完整参考实现差异；tile/water 经 adapter | `ProjectileMotionAndAiSystem` 的小批实现、stub 决策记录、motion/collision input snapshot | 每个纳入范围行为都有目标 revision 证据和 focused verifier；未决 subtype 不计入已迁移覆盖，继续显示 `unknown` |
| 5. 碰撞与战斗 | 定义 collision candidate、target snapshot、命中资格和伤害 Command；把 NPC/Player damage 交给各自 owner；处理局部/静态 immunity 与 penetration | collision adapter/query、`ResolveProjectileHitCommand`、damage adapter 契约、单写者映射 | Query 没有可观察写入；共享 Collision 标志和懒加载隔离完成；提交重查目标/免疫/penetration；伤害没有双写 |
| 6. 网络、表现与专用能力 | 定义 packet 27/29、section/recipient projection、frame/trail projection 和专用能力边界；已实现 packet 27/29 payload codec，外部 owner 逐个交接 | payload codec、declaration-level network/state matrix、projection、capability handler、跨域 handoff 记录 | payload codec 尚未做验证或接入 PacketGateway；decode-before-commit、重复包和 owner+identity+UUID 策略保持 `not-run`；表现不写 gameplay authority；专用能力有独立 owner 和覆盖记录 |
| 7. 收敛与验收 | 运行有范围的差分/回归验证；检查所有 P15 成员映射、未闭合项、遗留入口和旧 writer；按仓库 build/test 约束执行最终验证 | 验收记录、剩余 unknown 清单、切换/回退决策、最终状态报告 | 只有所有验收项有实际结果后才更新 `verificationStatus`；任何行为、调用关系或构建未验证项仍显式保留，不宣称完整迁移 |

## 阶段状态（计划）

| 阶段 | 状态 | 本文定义 | 未完成/仍阻塞 |
| --- | --- | --- | --- |
| 0. 固定证据基线 | `partial` | 重查只读 CPG manifest、符号/调用点/预算；比对完整参考项目的 FindOldest、SetDefaults、NewProjectile、packet 27 与 Main 升序更新片段；目标与完整参考关键文件哈希不同 | CPG 与 Version4 revision 绑定仍 `evidence-gap`；NewProjectile->FindOldest/SetDefaults 的索引缺边未闭合；126 成员读写矩阵未形成 |
| 1. 状态 owner 与契约 | `proposed` | 建立 126 成员单写者矩阵、Component schema、Command/Query/Adapter 契约与跨域 handoff | 现有候选 writer 未完成运行时入口闭包；其他 writer 尚未盘清 |
| 2. 定义与 lifecycle | `partial` | 按 `SetDefaults` 与普通 spawn 顺序保留局部 hydration、slot/identity/UUID commit；packet 27/29 payload codec 已实现，apply owner 边界仍是 proposed | 内容全集 builder、生产 spawn caller、完整默认值分支、Main/difficulty/其余 `ProjectileID.Sets` 派生、source modifiers、wet collision、PacketGateway 接线与网络授权、banner/minion side effects、1000 sentinel、完整 Kill 与持久化重建均未形成证据 |
| 3. 有序调度壳 | `partial` | `ProjectileTickCoordinator` 固定 Pre/Post、`0..999` 升序槽位读取、regular Update 一次性 immunity/lifetime 推进和 `ExtraUpdates + 1` 子步委派；`TryGetAtSlot` 为只读 slot query，`IProjectileTickAdapter` 为行为委派边界 | 尚未接入 Version4 权威 Main；`ProjectileUpdateLoopIndex`、当前帧 spawn 可见性、完整单槽位事件顺序和 AI/collision/combat 仍未闭合 |
| 4-5 | `not-started` | 无 | AI、motion、collision/combat 的目标行为和跨域 owner 均未实现 |
| 6. 网络、表现与专用能力 | `partial` | packet 27 body 编解码、packet 29 body 解码；lifecycle apply/termination 的 proposed owner 边界 | PacketGateway 接入、服务器 owner guard、relay、recipient/section、net spam、表现和专用能力均未接入；本次 codec 未验证 |
| 7. 收敛与验收 | `not-started` | 无 | P15 全量集成、差分和行为等价验收未执行 |

拟议 identity index 契约应拒绝不同 owner identity 争用同一活动 handle，也应拒绝同一 owner identity 映射至另一个 handle；过期 generation 不能替换或注销当前 handle。`Rebuild` 应在临时索引校验完整后才交换两个方向的映射。该契约仍需绑定 Version4 调用者和调度入口；文件、类型或局部实现存在都不构成实际游戏路径证据。

## 分阶段验证矩阵

本执行文档保留 focused 验证入口：`--lifecycle`、`--hydration`、`--network` 和 `--tick-coordinator`；packet codec verifier 尚未恢复。此前定向 build 后，`--lifecycle`、`--hydration`、`--tick-coordinator` 均返回 `PASS`，其中 tick coordinator 覆盖 adapter 中途终止后停止剩余子步；这些历史结果不覆盖本次 codec 修改，`--network` 仍未运行。本轮按用户要求未运行 build/test。局部 build/runtime 结果不覆盖真实 1000 槽集成、`Main.projectile[1000]`、generation 耗尽、服务器 owner guard、relay、recipient/section、Main 权威调度接线或完整行为等价，P15 总体验证仍为 `not-run`。

| 验证面 | 需要覆盖的观察 | 失败时处理 |
| --- | --- | --- |
| 源码/调用关系 | 126 成员的读写、入口调用点、packet 路径、event/reflection/dynamic dispatch；对照 CPG SourcePath、manifest、预算和 gap | 标 `partial` / `evidence-gap`，补源码浏览或定向 trace；不以零命中作为否定证据 |
| definition hydration | schema 子集字段映射、缩放后尺寸/center 转 top-left、spawn 覆盖、未知 `NeedsUuid` 拒绝、free/oldest-slot identity 与条件 UUID、替换槽数组状态初始化；另需覆盖完整 pooled reset、Main/difficulty/其余 `ProjectileID.Sets` 派生、source modifier、packet apply 顺序 | `partial`；hydration focused verifier 已通过，但不等于内容全集或默认值等价 |
| slot 与 lifecycle | 空槽、全满池、oldest 选择、普通 spawn 的 `identity=selected slot`、按已知 `NeedsUuid` 初始化 UUID、Stardust Dragon 的 parent UUID 到 `ai[0]` 转换、packet 27 UUID 可选字段与远端 identity 保留、替换时 owner identity 索引原子更换、重复 terminate、index rebuild | `partial`；lifecycle focused verifier 已通过局部分配/替换/终止断言；真实调用入口、1000 槽边界和完整副作用仍未核对 |
| tick 顺序 | Pre/Post 次数、0..999 顺序、当前 generation、regular Update 一次性 immunity、`ExtraUpdates + 1` 子步、timeLeft 到期释放、loop index、AI、collision、damage、trail、寿命与终止 | `partial`；定向 verifier 已通过局部 immunity/lifetime/顺序壳及 adapter 中途终止保护；Main 接线、loop index、完整单槽位行为和副作用仍需独立验证 |
| combat | friendly/hostile、owner hit check、local/static immunity、穿透、目标过期、Player/NPC damage handoff | 阻止 combat writer 切换，修复边界并重跑 focused verifier |
| network | packet 27/29 payload 编解码、owner/type/identity/UUID、section recipient、net spam、损坏/重复/重放数据 | codec 代码已存在但本次未验证；PacketGateway 接入、服务器 guard、relay、section/recipient、net spam 和完整重放策略均 `not-run` |
| Query 纯度 | 固定 snapshot 重复性、随机/时钟读取、缓存失效、全局标志、Command 提交重查 | 有副作用的入口重分类为 Command/Adapter；不得公开成纯 Query |
| stub 与专用行为 | 高尔夫球、Shimmer、storm lightning 等当前目标 stub；minion/counterweight/fishing/mining/kite 路径 | 先确认目标 Version4 意图；未解决保留 `unknown`，不套用另一版本实现 |
| 集成与副作用 | 声音、尘埃、粒子、实体 spawn、tile/worldgen、channel、持久化和共享 ID | 归属写入 integration handoff；未对齐前不删除旧 owner |

## 具体阻塞与决策记录

### CPG 与源码差异

2026-10-01 文档补证使用仓库只读 Query API 连接 `D:\TRbackup\Version4-cpg-export\out-dop8-interproc.sqlite`。初始化元数据为 `ReadOnly=true`、`ImportStatus=complete`、manifest `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`、967 shards、`SourceSnapshotId=null`。`Find-CpgSymbols(FindOldestProjectile)` 返回 Projectile `int()` 符号；在 `Projectile.cs`、`MessageBuffer.cs`、`Main.cs`、`Player.cs`、`NPC.cs`、`Item.cs`、`WorldItem.cs` 7 个选定路径的 call-site 查询为 `complete` / 1 条，唯一来源为 MessageBuffer。Version4 当前源码 `Projectile.cs:10263` 还明确显示 `NewProjectile -> FindOldestProjectile()`，故该关系是 CPG/source `evidence-gap`，不是“无调用”。

同一只读 Query API 会话中，`Find-CpgSymbols(Kill:void())` 按 `Terraria/Projectile.cs` 声明路径返回 `complete`；对 `Terraria/MessageBuffer.cs` 的 `Find-CpgCallSites` 返回 2 个 `CallTargets`，与 Version4 Packet 29 分支及另一处 MessageBuffer Kill 路径一致。该静态关系不闭合 `Kill` 的 channel、声音、伤害、子弹、网络等副作用，故实现只提交 `NetworkTermination` 生命周期边界。

同一会话补查 `Update:void(int)` 与 `PreUpdateAllProjectiles:void()`：两个符号按目标声明路径返回 `complete`；`Update` 在选定的 `Terraria/Main.cs` shard 中返回 2 个静态调用点，`PreUpdateAllProjectiles` 返回 1 个静态调用点。`Kill:void()` 在选定的 MessageBuffer/Main/Projectile 路径查询为 `partial`，`MaxItems=100` 命中 `ItemBudgetExhausted`，返回结果包含 MessageBuffer 与 Projectile 内部调用但不构成完整 caller 集。`Update` callable facts 为 `partial` / `CalleeEffectsNotExpanded`；局部操作节点显示先调用 `DecrementLocalImmuneTimeCounters`、设置 `numUpdates = extraUpdates` 并进入循环，但 callee effects 未展开。上述结果与 Version4 `Main.cs` 的 `PreUpdateAllProjectiles -> 0..999 projectile[n].Update(n) -> PostUpdateAllProjectiles` 源码片段相互印证了 coordinator 的局部顺序、一次性 immunity/lifetime 推进依据，但不闭合 `Projectile.Update` 内部动态/间接行为、`ProjectileUpdateLoopIndex` 可见性或完整 Main 接入，因此 coordinator 仍只标 `partial`。

补充的 `NewProjectile` call-site 查询仍限于上述 7 个 shard。`MaxItems=300` 时 `Vector2` overload 为 `complete` / 17 条（Projectile 13、NPC 4），标量 overload 为 `complete` / 227 条（Projectile 78、NPC 143、Player 6）；此前默认 200 项查询曾因预算耗尽返回 `partial` / 200 条。`SetDefaults:void(int)` 选定符号的 7 路径 call-site 查询为 `complete` / 2 条（Main、MessageBuffer），但 Version4 `Projectile.cs:10270` 的 NewProjectile 自调用未被返回；源码/索引关系标 `evidence-gap`。`Find-CpgSymbols(maxProjectiles, SymbolField, Terraria/Main.cs)` 既有查询为 `partial` / 零命中，gap=`NoMatchingFactInScannedScope`。当前 Version4 源码树全文搜索没有 `maxProjectiles` 声明；完整参考树才有该常量。故此前“Version4 `Main.maxProjectiles=1000` 已由源码常量确认”的表述不成立，应以当前源码中的 `Projectile[1001]` 与 `< 1000` 硬编码循环为准。查询预算、shard 范围和 snapshot 缺失都阻止闭合调用关系。

### 完整参考项目与目标源码差异

完整参考项目根 `D:\TRbackup\无任何删减通过编译` 含 `TerrariaServer.sln` 与 `TerrariaServer.csproj`；本轮只读比对 `Projectile.cs`、`Main.cs`、`MessageBuffer.cs` 的分配、packet 27、Main update pass 和 `SetDefaults` 片段，没有在该项目构建或测试。三份关键文件 hashes 均不同于 Version4。两树 `FindOldestProjectile` 都扫描 0..999、跳过 `netImportant`、以严格 `<` 挑选最小 `timeLeft`，无候选返回 1000；两树数组长度都是 1001，Main update pass 都按 0..999 升序且保持 Pre/Post 调用位置。满池且所有普通槽均 `netImportant` 时，`NewProjectile` 或 packet 27 接收路径会取索引 1000；它不进入普通 update pass，其生命周期/网络/清理语义仍 `unknown`。完整参考 `Main.maxProjectiles=1000` 不可当作 Version4 声明。

两树 `SetDefaults(int)` 从方法声明到 `DefaultToSpray()` 声明前均为 172,296 字符，SHA256 相同：`DC4505F2E87C02030681E8247472A15A7FC4CF1EEC2FD601A13078D58B0402D1`。该证据只覆盖方法文本。spawn 的主体顺序也可对应，但完整参考在本地 packet 27 发送前增加 `Main.netMode != 0`；Version4 `Projectile.cs:10510` 没有此条件，完整参考 `:10518-10520` 有条件分支。接收 packet 27 时，完整参考在 `MessageBuffer.cs:1746-1758` 将 hostile 校验限定服务器模式，在 `:1788-1791` 将 spam 计数限定服务器模式，并在 `:1813-1815` 将 relay 限定服务器模式；Version4 对应 `:1342-1351`、`:1381-1382`、`:1403` 未见这些外围 guard。迁移必须以 Version4 当前源码定义目标行为；这些参考树差异不可静默回填。

Version4 `Projectile.SetDefaults(int)` 位于 `Projectile.cs:444`，完整参考位于 `Projectile.cs:454`；两者从声明到 `DefaultToSpray()` 前的源码片段均为 172,296 个字符且 SHA256 相同（`DC4505F2E87C02030681E8247472A15A7FC4CF1EEC2FD601A13078D58B0402D1`）。该方法包含池化实例重置、按 type 的默认字段分支、读取 Main/difficulty/ProjectileID.Sets 上下文和尾部宽高缩放/maxPenetrate 赋值；它不是缺失实现的 stub。此一致性只限方法文本，不能消除两个树中其他 helper、静态数据、调用链和整类源码的差异。

Version4 `NewProjectile` 在 `SetDefaults(Type)` 后继续覆盖 slot/owner、位置/速度、damage/knockback、identity 与 AI 输入，再运行 wet collision、source stats、banner/minion source、UUID、免疫和 subtype 逻辑。`MessageBuffer.cs:1381` 的 packet 27 路径按 owner/identity 找到实例后，仅在实例不活动或 type 不同的分支调用 `SetDefaults`；随后写入 packet 字段、可选 UUID/index 并执行 `ProjectileFixDesperation`。Main 启动路径（`Main.cs:3388-3400`）另外逐类型调用 `SetDefaults` 并建立 `projHostile`、`projHook` 派生表。默认值初始化必须与普通 spawn、条件式 packet apply 和 Main 派生表分开映射。

对完整参考树全体 C# 文件的限定名文本扫描，在 12 个源文件中找到 427 个至少一处 `Projectile.NewProjectile(` 的匹配行，横跨 `Main`、`Mount`、`NPC`、`Player`、`Wiring`、`WorldGen`、Cinematics 与 GameContent。该结果帮助发现跨域入口，但不是调用闭包证明；CPG 的选定 shard/预算限制和源码外动态调用仍保留为 gap。Version4 与完整参考树的行为结论需逐符号核对，不能把参考版本当成目标快照。

### `SetDefaults` 与 content hydration 缺口

NLTX `src` 中当前 content schema 由 `Terraria.Content.ProjectileDefinition` 和 `ProjectileDefinitionCatalog(IEnumerable<ProjectileDefinition>)` 表达；未找到 `ProjectileDefinition` 数据构造、content snapshot build call-site 或向 `ProjectileLifecycleSystem.TryCreate` 的生产接线。`Terraria.SpatialMotionPhysics.ProjectileDefinitionCatalog` 是不同命名空间下只含 frame/pet 数组的类型，也没有找到 adapter 调用点。现有 `ProjectileEntityState` 只有 Identity、Lifetime、Network，lifecycle create 仅接收这些状态；因此当前实现没有表示或装载完整 `SetDefaults` 输出的路径。

CPG `Find-CpgSymbols(SetDefaults)` 结果含 `Terraria.Projectile:void(int)` 和一个 `Terraria/Item.cs` 同名记录，按声明路径/签名区分后选用 Projectile symbol。对 `Projectile.cs`、`Main.cs`、`MessageBuffer.cs`、`NetMessage.cs` 的 `Find-CpgCallSites` 返回 `Main.cs`、`MessageBuffer.cs` 两处；源码还在 `Projectile.cs:10270` 明确显示 `NewProjectile` 自调用该方法。故此处标记为 CPG/source `evidence-gap`。Projectile callable facts 为 `partial` / `CalleeEffectsNotExpanded`，不可用于闭合 reset helper 和静态数据读取效果。

阶段 2 在以下证据形成前保持 `partial`：Version4 `SetDefaults` 每个目标字段的默认值/覆盖分支；真实 catalog type 数据构造/加载与生产 spawn caller；复用实例数组/cache 的完整 reset；Main、difficulty、`ProjectileID.Sets` 等上下文依赖及初始化顺序；spawn/source modifier 和 packet 字段覆盖次序；packet 27 active+same-type 条件路径；Main `projHostile`/`projHook` 派生表 owner 与时机；缺失/invalid type 的目标错误边界。完整参考项目的相同方法文本是交叉核对材料，不替代目标源码和行为验证。

### 未知所有者

若 `src/NSSLC` 中已有 `ProjectileLifetimeSystem`、`ProjectileNetworkStateSystem`、identity index 或其他候选代码，阶段 1 必须先找到它们的真实调用者与调度入口。未确认谁负责整个权威 Main loop 前，不新增第二个 coordinator，不并行切换多个状态 writer。

### 2026-10-03 packet payload codec slice

Version4 `NetMessage.cs:769-842` 给出 packet 27 的字段顺序与可选位；`MessageBuffer.cs:1320-1404` 给出接收顺序。`NetMessage.cs:852-855` 和 `MessageBuffer.cs:1439-1452` 给出 packet 29 的 identity/owner 字段。基于这些源码片段，`src/NSSLC.Infrastructure/Network/ProjectilePacket27Codec.cs` 实现 packet 27 body 编解码，`ProjectilePacket29Codec.cs` 实现 packet 29 body 解码。payload 不含外层 packet frame/header。

Codec 将截断、尾随数据和不能表示为当前 Command 的字段分别报告为 `Truncated`、`TrailingBytes`、`Invalid`；packet 27 的 UUID 小于 0 或不小于 1000 时按 Version4 接收路径规范化为未提供。Codec 保留 payload 中的 owner，不执行 Version4 `whoAmI` 替换、hostile 检查、relay、recipient/section 选择或 lifecycle 提交；这些仍需 Network/Application 集成证据与明确策略。

按用户要求，本轮没有运行 build 或 test。此次 codec、项目引用和字段边界均未执行编译或行为验证，状态保持 `not-run`。

## 静态证据与交付边界

| 项目 | 状态 |
| --- | --- |
| 设计与执行文档 | `proposed`；只记录 System 边界、执行顺序、验收门槛和剩余缺口 |
| CPG API 只读查询 | 2026-10-01 通过 `CpgEvidence.ps1` 查询 `NeedsUUID`（`Terraria.ID/ProjectileID.cs`，symbol=`complete`）、选定 3 个路径的 member uses（`complete` / 1 条）与标量 `NewProjectile` overload call-sites（选定 `Projectile.cs`/`NetMessage.cs`/`MessageBuffer.cs` 路径下 `complete` / 78 条，来源路径为 `Projectile.cs`）；callable facts=`partial` / `CalleeEffectsNotExpanded`。数据库 manifest=`6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`、`SourceSnapshotId=null`；这些状态不证明闭合调用或数据初始化行为 |
| Version4 目标源码 | `D:\TRbackup\Version4` 是目标语义来源；源码关系不足时以目标声明、实现和调用点回查，并保留 `partial`/`unknown`/`evidence-gap` |
| 完整参考源码 | 只读检查 `D:\TRbackup\无任何删减通过编译` 的 `TerrariaServer.sln` / `.csproj`、`Projectile.cs`、`ProjectileID.cs`、`NetMessage.cs`、`Main.cs`、`MessageBuffer.cs`；关键文件与 Version4 hashes 不同，提取的 `SetDefaults` 方法文本 hash 相同；本交付不在参考项目构建或测试 |
| 本轮文档与实现补证 | 2026-10-01 的只读 CPG/源码对照、历史定向 build 和 lifecycle/hydration/tick coordinator focused verifier 结果继续作为局部历史证据；2026-10-03 新增 packet 27/29 payload codec，并按用户要求未运行 build/test，不将其标记为已验证，也不把局部结果当作 P15 行为等价证据 |
| 设计输入 | 17 个叶子组、126 个成员、原 `sessionId=4426a5ad1b824c2490a832c3bb6726b8` 和已结算输入报告；不创建新的 claim |
| 源码/项目变更 | `true`；包含 `src/NSSLC.Infrastructure/Network` 的 packet payload codec 和对 `Terraria.Projectile` 项目的引用；另有既有 lifecycle、ordered tick coordinator 与 verifier 入口改动；未改 Version4、完整参考项目或 runner 状态 |
| 局部 build / runtime | `partial`；2026-10-01 前序执行已通过 `Build/Tools/Invoke-SerialDotnet.ps1` 定向构建 Projectile focused verifier，exit code `0`、`0 warnings`、`0 errors`，产物位于 `Build/bin/Terraria.ProjectileCombatVerification/Debug/net10.0/`；`--lifecycle`、`--hydration`、`--tick-coordinator` 当时均输出 `PASS`。这些历史结果不覆盖 2026-10-03 的 codec/project-reference 修改；本轮未运行 build/test。结果不覆盖 solution 全量、`--network`、真实 Main 接线或 P15 行为等价 |
| 迁移/行为等价结论 | 未作出；P15 保持 `proposed`，不能称迁移成功 |
