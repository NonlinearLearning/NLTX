# Version4 P15 Projectile ECS 拆分执行文档

## 执行状态

| 字段 | 值 |
| --- | --- |
| `executionPlanStatus` | `proposed` |
| `implementationStatus` | `partial`（新增 packet 27 payload 编解码及 DTO 双向映射、packet 29 payload 解码及 DTO 双向映射、soundDelay/reflected 状态归属修正、专用规则常量与 IsADD2Turret/DD2 延寿映射、六项派生属性映射及 OwnerMinionAttackTargetNPC 的槽投影、Damage_CanDealDamage、Damage_GetHitbox、NPC/PVP pre-collision、per-projectile-type static NPC immunity expiry registry 和 54 个 `Projectile.Colliding` 类型的局部几何映射及未列明合法类型的 generic hitbox fallback，并增加 type 871 pellet-storm、AI style 15 golf-ball、AI style 190 cone、AI style 137 visibility-snapshot、基于显式 owner Player 动画输入计算的 AI style 19 spear-extension snapshot、Version4 IsAWhip 类型映射和显式点快照驱动的 whip control-point 几何（whip control-point 生产调用、AI style 137 区域快照投影适配器已实现但生产 Query 调用仍缺，以及 AI style 19 的 Application owner-input projection 与组合扩展 Query 已实现，但运行时 Player owner lookup/caller 仍缺）；trajectory/numUpdates 状态、可扩展子步、逐子步寿命推进、pre-AI 冷却边界及 preparation/substep outcome 契约已落入 coordinator；默认 preparation 处理 type-640 倒计时、显式世界边界快照下非 minion 的提前停用、minion/sentry Player handoff gate、`gfxOffY` 收敛和 inherited `oldVelocity` 到共享 Entity motion-history component 的局部映射；Ready 子步在递减 soundDelay 后清除瞬时 `netUpdate` 并保留延迟 `netUpdate2`；`ProjectileTrailCacheSystem` 已局部映射 Version4 142 个显式 type-to-mode 条目、默认 `-1` 及 `TrailingMode` 0–5 的历史数组更新，并由 definition hydration 投影到 Presentation state；mode 1 Dust effects remain unimplemented; the mode 4 Player movement snapshot is wired through the Tools.Simulation adapter but is unexercised by its type-1-only profile; coordinator 当前仅有 Tools.Simulation 的有限 type-1/type-AI-1 caller，Version4 Main 权威 caller 仍缺；coordinator 已在每轮 `PreUpdateAllProjectiles` 后捕获一次 DD2 `Ongoing` 快照供本轮 IsADD2Turret 子步复用，在 completed 子步调用 trail `Record`，并在寿命到期检查后按 penetrate==0 以 HitLimitReached 终止，且非 hydration 状态的穿透默认值对齐 Version4 的 1；Application `ProjectileSimulationTickPhase` 已封装 coordinator 并从每 tick committed descriptor 投影边界；Tools.Simulation 注册 `ActiveProjectileTickPhase` wrapper，由 `RuntimeProjectileStore` 提供 per-tick adapter factory 并调用 Application phase；仍未接入 Version4 权威 Main 调度。lifecycle 与 ordered tick coordinator 仍是局部切片; Type 1 Magic Quiver owner cadence is wired from scripted Player capability after movement） |
| `verificationStatus` | `not-run`（P15 行为、集成与迁移验收未执行） |
| 最近实现切片 | `partial`（新增唯一瓦片索引、SpatialSimulation 的 `SpatialCanHitQuery`/`SpatialCanHitLineQuery`、Projectile 碰撞环境快照及 NPC/PVP snapshot-based composition；AI 137、type 661 和 CanHit gates 由显式瓦片 snapshot 计算；Application 已增加 Player-owner input projection 和 Kernel Projectile phase adapter，phase 每 tick 从已提交的 world descriptor 捕获像素边界；`NSSLC.Tools.Simulation.Program` 注册 `ActiveProjectileTickPhase` wrapper，由 `RuntimeProjectileStore` 提供有限的 scripted type-1/type-AI-1 per-tick adapter，经 Application phase 执行；当前模拟组合还共享静态 NPC 免疫表给命中查询、成功命中提交和 NPC 槽回收清理；仍未接入 Version4 权威 Main，行为验收仍缺） ；Type 1 Magic Quiver owner capability now promotes projectile cadence for the next regular tick。；mode 4 owner movement delta is captured after Player movement and exposed with a tick-bound Projectile adapter snapshot, but remains unused by the type-1-only host。|
| `existingFocusedVerifierStatus` | `partial`（lifecycle、hydration、ordered tick coordinator 有历史 focused verifier 通过记录；这些记录早于本次 numUpdates/逐子步寿命/outcome/冷却边界、共享 Entity motion-history 映射及 network substep flag 改动；本轮未运行验证，packet codec 无 verifier，P15 集成未运行） |
| 最近更新 | `2026-10-05` |
| 关联设计 | [P15 ECS 拆分设计](2026-09-30-system-decomposition-authoritative-P15-projectile-design.md) |
| 权威输入 | [P15 权威静态拆分报告](2026-09-18-system-decomposition-authoritative-P15-projectile.md) |
| session | 沿用已结算的 `4426a5ad1b824c2490a832c3bb6726b8`；本文不 claim/settle 新分区 |
| `outputReport` | 本执行文档；设计文档见 [P15 ECS 拆分设计](2026-09-30-system-decomposition-authoritative-P15-projectile-design.md) |
| `sourceModified` | `true`（`src/NSSLC.Tools.Simulation` 的有限 Projectile phase/runtime host 和木箭内容映射；`src/NSSLC` 的 lifecycle、ordered tick coordinator、tick adapter preparation/substep contract、trajectory/world-boundary snapshot、local motion rule、trail mode mapping/hydration projection/cache update 与 projectile state/rule/derived-property/damage-collision mapping；`src/NSSLC/Component/SpatialSimulation/SpatialTileLookupSnapshot.cs`、`SpatialCanHitQuery.cs`、`SpatialCanHitLineQuery.cs`；`src/NSSLC/Component/Projectile/ProjectileCollisionEnvironmentQuery.cs` 与 `ProjectileCollisionEnvironmentSnapshot.cs`；`src/NSSLC.Application/Projectile/ProjectilePlayerCollisionInputProjection.cs`、`src/NSSLC.Application/Simulation/ProjectileSimulationTickPhase.cs` 与 Player 项目引用；`src/NSSLC.Application/Network` 的 projectile network command owner port；`src/NSSLC.Infrastructure/Network` 的 packet 27/29 codec、双向 DTO 映射和 Gateway registration adapter；`src/NSSLC.Infrastructure/WorldGeneration/Adapters/LegacySpatialTileSnapshotAdapter.cs` 与 WorldGeneration 到 SpatialSimulation 的契约引用; src/NSSLC.Tools.Simulation/SimulationInputScript.cs; src/NSSLC.Tools.Simulation/RuntimePlayerStore.cs; src/NSSLC.Tools.Simulation/RuntimePlayerEntity.cs; src/NSSLC/Component/Player/PlayerRangedAccessoryCapabilityComponent.cs; src/NSSLC/Component/Projectile/ProjectileMotionAndAiSystem.cs） |
| `reference project` | `D:\TRbackup\无任何删减通过编译`（只读完整参考源码） |
| 最近文档更新 | 2026-10-05：补入 Type 1 `AI_001` 通用分支、移动后 Entity direction 及 spawn 速度归一化；此前工作继续映射受限 Tools.Simulation type-1 dry/wet tile movement、water/honey/shimmer wetVelocity 和碰撞后位置顺序；扩展 Type 1 support manifest，约束缩放后几何与相关默认状态；按用户要求未运行 build/test/verifier；新增 LegacySpatialTileSnapshotAdapter 的区域 Main.tile 投影（生产 Query caller 仍缺）；新增 per-projectile-type static NPC immunity expiry component/System（接受命中写入和 static rows 清理已映射；静态表候选读取、成功命中写入与 NPC 槽释放清理现已接入受限 Tools.Simulation，Type 1 manifest 禁用此分支；普通局部 NPC cooldown 的成功命中写入也已接入，但 Type 1 同样关闭该状态）；packet 27/29 增加 payload/Command 双向 packet DTO 映射、Active-only Gateway handler registration helper 与 Application command-owner port；handler 将普通 packet 27 owner 映射到受信 actor、type 949 映射到 255、packet 29 owner 映射到受信 actor，hostile 判定/lifecycle commit/relay 仍待具体 owner；soundDelay 纳入 projectile 聚合并允许 AI 使用负值；reflected 只保留在专用反射状态；增加两项 Version4 专用规则常量及 IsADD2Turret/DD2 延寿映射；补入 Opacity/MaxUpdates 读写映射、OwnedBySomeone/CareForAttackCD/WipableTurret/NetSectionCoordinates 查询及 OwnerMinionAttackTargetNPC 到 `NpcSlot` 的投影、Damage_CanDealDamage、Damage_GetHitbox 与 NPC/PVP candidate filters；trajectory 状态纳入聚合，coordinator 按可变 numUpdates 循环、逐子步推进寿命，并在 adapter pre-AI preparation 为 `Ready` 后递减正值 soundDelay；默认 preparation 依序处理 type-640 倒计时和显式边界快照下非 minion 停用及 `gfxOffY` 收敛；Ready 子步随后清除瞬时 `netUpdate`、保留 `netUpdate2` 并将其映射到 send-request 状态；世界边界值类型拒绝未初始化的 `default` 快照；默认 preparation 在 AI 前将继承自 Version4 `Entity` 的 `oldVelocity` 写入共享 `EntityEcs.Components.MotionHistoryComponent.PreviousVelocity`；越界 minion 与末子步 minion/sentry 仍需 Player owner adapter；另一份 SpatialSimulation motion-history component、通用实体 writer 和生产调度仍待 integration review；未提供快照时不执行边界判断；54 个 selected Colliding projectile types 包含 31 个线段 types、11 个矩形/扫掠 types、7 个 history-trail types、type 464 六矩形轨迹、type 871 pellet-storm、type 973 radial、type 985 cone、type 661 的 CanHitLine gate、AI style 15 mounted-center ellipse，以及 AI style 190 slow/secondary cone 与 AI style 137 距离/视线快照分支（现有纯 tile visibility query 使用 SpatialCollisionSnapshot；LegacySpatialTileSnapshotAdapter 已从 Main.tile 投影区域 tile facts，但生产 Query caller 仍缺），以及 AI style 19 owner-input extension snapshot calculation（已对齐 Utils.Remap 的 clamped GetLerpValue endpoint 行为）；Application 已增加 Player 动画/近战速度投影、组合扩展 Query caller、mounted-center 投影，以及 Kernel Projectile phase adapter；Tools.Simulation 的 ActiveProjectileTickPhase wrapper 调用 Application phase，runtime adapter factory 仍限 type 1/AI style 1，且未接入 Version4 权威 Main；live owner lookup 仍缺；`CanHit`/`CanHitLine` 由显式瓦片 snapshot 计算；mounted center 仍为显式输入快照；trail cache 已映射 Version4 的 142 个显式 type-to-mode 项、默认 -1 和 mode 0–5 数组更新，并在 definition hydration 时写入 Presentation state；coordinator 已调用 trail Record；mode 1 Dust 效果和 Version4 Main 权威 coordinator caller 仍缺；mode 4 Player movement snapshot 已接入 Tools.Simulation adapter，但 type-1/type-AI-1 host 切片不支持 mode-4 projectile，受限 Tools.Simulation Type 1 host 已接入 explicit-input NPC collision composition；NPC/Player snapshot producer、hit effects/commit、其它 AI、Gateway owner/host 注册和权威 Main 调度仍未接线；按用户要求未运行 build/test/verifier，P15 整体仍为 proposed/not-run |

本文规定将 P15 proposed 设计落入 `D:\TRbackup\NLTX\src\NSSLC` 的执行次序与验收门槛，并记录 packet payload codec、已解码 DTO 映射、Gateway registration helper/Application command-owner port、lifecycle proposed owner 边界、ordered tick coordinator 的 immunity/lifetime 局部切片。`ProjectilePacket27Codec` 可将 packet 27 body 或 profile 已解码 payload 映射为领域命令，也可将领域命令映射为 profile 可发送的生成 packet DTO；`ProjectilePacket29Codec` 提供对应的 payload 到命令和命令到 packet DTO 映射。通过 packet DTO 发送时由 profile binding 使用已绑定的 UUID 协议事实；独立的 packet 27 body `Encode` 仍要求调用方提供正确的 type 规则。`ProjectilePacketGatewayRegistration` 可在 Gateway 启动前注册仅 Active 阶段的 27/29 handler；handler 把 wire owner 投影到可信 actor（packet 27 type 949 使用 255），再委托给 Application owner。具体 owner、hostile/type 策略、lifecycle 提交、relay 和生产注册调用尚未形成；coordinator 也尚未接入权威 Main 的逐槽调度壳。不视为已接入 Version4 完整运行路径。普通 spawn 输入只携带 owner/type/参数、在 lifecycle 选定 free/oldest slot 后派生本地 identity 和已知条件 UUID，仍是待闭合契约。该计划不表示 Version4 完整 `SetDefaults`、真实类型数据、网络授权/relay、Main 调度接线或 Kill 行为已迁移。每阶段必须先满足前序 gate；任何 `unknown`、`partial` 或 `evidence-gap` 未解决时，应停在对应阶段并记录具体缺证据项，而非写成迁移成功。

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
| 2. 定义与 lifecycle | 以 Version4 `SetDefaults` 为目标逐字段对齐 catalog 与实例状态；分别建立 type-default hydration、spawn 输入/source modifier、packet receive/apply 契约与 recycle reset；迁移 slot、owner、identity、UUID、活动与索引不变量；单独归属 Main 启动时的 `projHostile`/`projHook` 派生表 | field-by-field hydration 映射及上下文依赖、`SpawnProjectileCommand`、`ApplyProjectileNetworkCommand`/`TerminateProjectileCommand`、索引重建策略 | free-slot/full-slot/oldest-slot 与默认值重置、spawn 覆盖顺序均有源码依据；普通 spawn 在 slot commit 后派生 `identity=slot`，按 `NeedsUUID[type]` 初始化 UUID；packet codec 与 Gateway registration adapter 已实现，但 Application 具体 owner、生产注册、hostile/type 策略与 relay 未闭合；slot 分配无 Query/Command TOCTOU；终止幂等性仍待验证 |
| 3. 有序调度壳 | 在权威 Main 调度点接入逐槽位 coordinator；记录 Pre、loop index、ascending pass、Post；先维持兼容委派，不改变 AI/collision/combat 算法 | per-slot scheduler seam、事件顺序表、旧路径到新 owner 的委派映射 | 单槽位内序、extraUpdates、当前帧 spawn 可见性和 slot 顺序均有源码/trace 对照；无全局多轮重排；旧入口仍可追踪 |
| 4. AI 与运动 | 按能力迁移 AI/motion；显式传递随机/时间上下文；逐个处理目标源 stub 与完整参考实现差异；tile/water 经 adapter | `ProjectileMotionAndAiSystem` 的小批实现、stub 决策记录、motion/collision input snapshot | 每个纳入范围行为都有目标 revision 证据和 focused verifier；未决 subtype 不计入已迁移覆盖，继续显示 `unknown` |
| 5. 碰撞与战斗 | 定义 collision candidate、target snapshot、命中资格和伤害 Command；把 NPC/Player damage 交给各自 owner；处理局部/静态 immunity 与 penetration | collision adapter/query、`ResolveProjectileHitCommand`、damage adapter 契约、单写者映射 | Query 没有可观察写入；共享 Collision 标志和懒加载隔离完成；提交重查目标/免疫/penetration；伤害没有双写 |
| 6. 网络、表现与专用能力 | 定义 packet 27/29、section/recipient projection、frame/trail projection 和专用能力边界；已实现 payload codec、DTO mapping、Active-only Gateway registration adapter 与 Application owner port | payload codec、network/state matrix、recipient projection、capability handler、跨域 handoff 记录 | handler、port 尚无具体 owner 或生产注册；hostile guard、decode-before-commit、relay、section/recipient、重复包和 owner+identity+UUID 策略保持 `not-run`；表现不写 gameplay authority；专用能力有独立 owner 和覆盖记录 |
| 7. 收敛与验收 | 运行有范围的差分/回归验证；检查所有 P15 成员映射、未闭合项、遗留入口和旧 writer；按仓库 build/test 约束执行最终验证 | 验收记录、剩余 unknown 清单、切换/回退决策、最终状态报告 | 只有所有验收项有实际结果后才更新 `verificationStatus`；任何行为、调用关系或构建未验证项仍显式保留，不宣称完整迁移 |

## 阶段状态（计划）

| 阶段 | 状态 | 本文定义 | 未完成/仍阻塞 |
| --- | --- | --- | --- |
| 0. 固定证据基线 | `partial` | 重查只读 CPG manifest、符号/调用点/预算；比对完整参考项目的 FindOldest、SetDefaults、NewProjectile、packet 27 与 Main 升序更新片段；目标与完整参考关键文件哈希不同 | CPG 与 Version4 revision 绑定仍 `evidence-gap`；NewProjectile->FindOldest/SetDefaults 的索引缺边未闭合；126 成员读写矩阵未形成 |
| 1. 状态 owner 与契约 | `proposed` | 建立 126 成员单写者矩阵、Component schema、Command/Query/Adapter 契约与跨域 handoff | 现有候选 writer 未完成运行时入口闭包；其他 writer 尚未盘清 |
| 2. 定义与 lifecycle | `partial` | 按 `SetDefaults` 与普通 spawn 顺序保留局部 hydration、slot/identity/UUID commit；packet codec、DTO mapping、Gateway registration seam 和 Application owner port 已有代码 | 内容全集 builder、生产 spawn caller、完整默认值分支、Main/difficulty/其余 `ProjectileID.Sets` 派生、source modifiers、wet collision、具体网络 owner/host 注册/授权、banner/minion side effects、1000 sentinel、完整 Kill 与持久化重建均未形成证据 |
| 3. 有序调度壳 | `partial` | `ProjectileTickCoordinator` 固定 Pre/Post、`0..999` 升序槽位读取、regular Update 一次性 immunity 推进；以 `numUpdates = extraUpdates` 并在每个子步前递减的 while 循环委派行为，允许 AI 重置计数延长循环；preparation 按 type-640 倒计时、world-boundary 检查、minion/sentry owner gate 和 `gfxOffY` 收敛的顺序处理前缀，并在其后记录 inherited `oldVelocity` 至共享 Entity motion-history；仅 `Ready` 路径在推进正值 soundDelay 后清除瞬时 `netUpdate` 并保留 `netUpdate2`；正常到尾的子步先记录 trail、对 IsADD2Turret 类型按 DD2 快照抵消一次 timeLeft 递减，再推进 lifetime 并处理 penetrate==0 终止 | 尚未接入 Version4 权威 Main；world-boundary snapshot 缺失时不执行边界规则；越界 minion 与末子步 minion/sentry 需 Player owner adapter；缺少其余 AI 分支和 DD2 Ongoing 的生产 adapter，也没有 AI 内 soundDelay 负值赋值；通用实体 motion-history writer/scheduler 与 Entity 的其它继承状态接线未闭合；自定义 preparation override 必须保留前缀顺序；`ProjectileUpdateLoopIndex`、当前帧 spawn 可见性、完整单槽位事件顺序和 AI/collision/combat 仍未闭合 |
| 4. AI 与运动 | `partial` | `ProjectileMotionAndAiSystem.AdvanceGfxOffY` 映射了 Version4 的逐子步 `gfxOffY` 收敛/限幅；Coordinator 默认 preparation 已接入该局部规则，并将 inherited `oldVelocity` 快照写入共享 Entity motion-history component | 其它 AI/motion 分支、风/Player state 和生产 adapter 未实现；受限 Tools.Simulation type 1 host 新增按当前液体接触选择 water/honey/shimmer TileCollision 与 wetVelocity 缩放，并保留 dry 分段路径；wetCount 和湿态转场/splash 请求资格已映射，实际液体进入/离开效果与权威 adapter 仍未接线；另一份 SpatialSimulation motion-history component、通用实体 writer 与 production scheduler 仍未闭合；局部映射未运行行为验证 |
| 5. 碰撞与战斗 | `partial` | damage admission gate、`Damage_GetHitbox`、NPC/PVP pre-collision filters，以及 `Projectile.Colliding` 中 54 个 projectile type 的选定几何分支及未列明合法类型的 generic hitbox fallback、type 871 pellet-storm、AI style 15 golf-ball ellipse、AI style 190 dual-cone、AI style 137 tile visibility query、基于 owner 输入快照的 AI style 19 spear-extension geometry 和显式点快照驱动的 whip control-point geometry 及 18 个 IsAWhip type 映射已实现；包含 31 个线段类型、11 个矩形/扫掠类型、7 个历史轨迹类型、type 464 六矩形轨迹、type 973 radial、type 985 cone 与 type 661 的 LOS/range gate；`ProjectileDamageCollisionQuery` 组合候选筛选和已映射几何；`ProjectileStaticNpcImmunityRegistryComponent` 与 System 映射按 projectile type/NPC slot 保存的绝对 expiry、accepted-hit cooldown 写入和 slot 清理；`ProjectileHitImmunitySystem.RecordAcceptedNpcHit` 映射普通 local NPC cooldown 写入；trail cache System 映射了 142 个显式 type-to-mode 项、默认 `-1`，并在 definition hydration 时写入 Presentation state；`TrailingMode` 0–5 的位置/旋转/方向历史更新也已映射 | 受限 Tools.Simulation Type 1 host 已调用显式输入的 NPC collision composition；更广范围几何、NPC/Player snapshot producer、mode 1 Dust 效果、mode 4 Player movement snapshot 已接入受限 Runtime adapter 但当前 type-1 host 不会触发、whip control-point、AI style 137 区域快照 adapter 的生产捕获/调用、AI style 19 运行时 owner lookup/caller、AI style 203 stub behavior、权威 Main 调度、powder/cut-tile/jellyfish/reflection/PVP effects 和完整 damage owner handoff 仍缺；静态免疫表的候选读取、成功命中写入和 NPC 槽回收清理已接入该 host，但 Type 1 support manifest 关闭静态免疫，当前受限类型不会执行此分支；普通 local-immunity accepted-hit writer 已接入 RuntimeProjectileStore，但 Type 1 manifest 关闭 local/static immunity；864/866/611/612 等专用分支的其它命中效果仍未映射 |
| 6. 网络、表现与专用能力 | `partial` | packet codec/DTO mapping、Active-only Gateway registration adapter 和 Application owner port；Ready 子步清除瞬时 primary update 请求并保留 secondary 请求；两项专用 Version4 常量；Opacity/MaxUpdates 读写及 OwnedBySomeone/CareForAttackCD/WipableTurret/NetSectionCoordinates 查询映射；OwnerMinionAttackTargetNPC 的 typed `NpcSlot` 投影；lifecycle apply/termination 的 proposed owner 边界 | 具体 owner/host 注册、hostile guard、relay、recipient/section、net spam、表现与专用能力查询生产接线均未完成；NPC 槽到当前实体的解析未实现；本次 network handler/port 未验证 |
| 7. 收敛与验收 | `not-started` | 无 | P15 全量集成、差分和行为等价验收未执行 |

## 2026-10-04 Runtime call-closure check

`ProjectileSimulationTickPhase` now adapts `ProjectileTickCoordinator` to the Application
`WorldSimulationKernel`'s Projectile phase and requires explicit world bounds. The kernel orders
registered phases by `WorldSimulationPhase`, so this phase follows Player/NPC phases when they are
registered. `NSSLC.Tools.Simulation.Program` registers `ActiveProjectileTickPhase`, which forwards to
this Application phase. The phase reads bounds from the committed world snapshot and requests a
per-tick adapter from `RuntimeProjectileStore`; that adapter supports only scripted type 1 / AI style
1 arrows. This is a finite host caller, not the authoritative Version4 `Main` caller.

`NSSLC.Infrastructure.WorldGeneration.Runtime.Main` declares a legacy `Main.projectile` array
(`Runtime/Main.cs:117`). `Runtime/WorldEntities.cs:123-130` defines these `Projectile` instances
with only `active`, `type`, `whoAmI`, `originatedFromActivableTile`, `netUpdate`, and a stub
`NewProjectile`; it has no `Update` method. `MainHost.ResetWorldStorage` creates 1000 compatibility
instances. That array is not the P15 simulation owner. The headless simulation host initializes
WorldGeneration runtime state for world loading, then ticks its registered Player, NPC, Projectile,
and WorldItems phases.

The region capture adapter is a data projection only. Version4 Main integration, broader gameplay
adapter coverage, NPC/Player snapshot producers, combat commit caller, and damage-owner handoff remain
open; P15 stays `partial` and `verificationStatus` stays `not-run`.

## 2026-10-04 Static NPC immunity registry

Version4 allocates `perIDStaticNPCImmunity[ProjectileID.Count][Main.maxNPCs]` with zeroed expiry ticks (`Projectile.cs:388-403`). A slot is immune while its expiry is greater than `Main.GameUpdateCount`; successful PVE hits write `GameUpdateCount + (uint)idStaticNPCHitCooldown` when `penetrate != 1` or `appliesImmunityTimeOnSingleHits` (`Projectile.cs:12696-12701`). `NPC.NewNPC` calls `Projectile.ResetNPCSlotData` when reusing an NPC slot (`NPC.cs:8127-8129`); that reset also clears each projectile local-immunity cell (`Projectile.cs:406-418`).

`ProjectileStaticNpcImmunityRegistryComponent` now holds the world-level per-type expiry rows. `ProjectileStaticNpcImmunitySystem` maps the expiry read and accepted-hit write, including the one-penetration exception; the candidate-query overload consumes the expiry read using the target slot and game update count. `ResetNpcSlotData` combines static-row clearing with clearing the corresponding local-immunity cell, when present, from every occupied projectile state in the supplied slot store. The accepted-hit write and slot reset still have no production damage-commit or NPC slot-reuse caller. NPC owner-immunity clearing and full hit effects remain part of the missing damage handoff. No build, test, or verifier was run.

拟议 identity index 契约应拒绝不同 owner identity 争用同一活动 handle，也应拒绝同一 owner identity 映射至另一个 handle；过期 generation 不能替换或注销当前 handle。`Rebuild` 应在临时索引校验完整后才交换两个方向的映射。该契约仍需绑定 Version4 调用者和调度入口；文件、类型或局部实现存在都不构成实际游戏路径证据。

## 2026-10-04 NPC type 414 collision bounds

Version4 `Damage_PVE_Inner` expands the type 414 NPC rectangle by 8 pixels on each side before invoking `Colliding` (`Projectile.cs:11639-11646`). `ProjectileDamageCollisionQuery.EvaluateNpc` now applies that target-specific rectangle expansion after the candidate filters and before projectile geometry. The PVP composition path is unchanged because this source rule only appears in `Damage_PVE_Inner`. This remains a local composition mapping; it does not provide NPC snapshots or a production caller. No build, test, or verifier was run at the user's request; `verificationStatus` remains `not-run`.

## 分阶段验证矩阵

本执行文档保留 focused 验证入口：`--lifecycle`、`--hydration`、`--network` 和 `--tick-coordinator`；packet codec verifier 尚未恢复。此前定向 build 后，`--lifecycle`、`--hydration`、`--tick-coordinator` 均返回 `PASS`，其中 tick coordinator 覆盖 adapter 中途终止后停止剩余子步；这些历史结果不覆盖本次 codec 修改，`--network` 仍未运行。本轮按用户要求未运行 build/test。局部 build/runtime 结果不覆盖真实 1000 槽集成、`Main.projectile[1000]`、generation 耗尽、服务器 owner guard、relay、recipient/section、Main 权威调度接线或完整行为等价，P15 总体验证仍为 `not-run`。

| 验证面 | 需要覆盖的观察 | 失败时处理 |
| --- | --- | --- |
| 源码/调用关系 | 126 成员的读写、入口调用点、packet 路径、event/reflection/dynamic dispatch；对照 CPG SourcePath、manifest、预算和 gap | 标 `partial` / `evidence-gap`，补源码浏览或定向 trace；不以零命中作为否定证据 |
| definition hydration | schema 子集字段映射、缩放后尺寸/center 转 top-left、spawn 覆盖、未知 `NeedsUuid` 拒绝、free/oldest-slot identity 与条件 UUID、替换槽数组状态初始化；另需覆盖完整 pooled reset、Main/difficulty/其余 `ProjectileID.Sets` 派生、source modifier、packet apply 顺序 | `partial`；hydration focused verifier 已通过，但不等于内容全集或默认值等价 |
| slot 与 lifecycle | 空槽、全满池、oldest 选择、普通 spawn 的 `identity=selected slot`、按已知 `NeedsUuid` 初始化 UUID、Stardust Dragon 的 parent UUID 到 `ai[0]` 转换、packet 27 UUID 可选字段与远端 identity 保留、替换时 owner identity 索引原子更换、重复 terminate、index rebuild | `partial`；lifecycle focused verifier 已通过局部分配/替换/终止断言；真实调用入口、1000 槽边界和完整副作用仍未核对 |
| tick 顺序 | Pre/Post 次数、0..999 顺序、当前 generation、regular Update 一次性 immunity、`numUpdates` 初值/递减/AI 重置延长、preparation outcome、type-640 倒计时、world-boundary snapshot、非 minion 停用、minion/sentry Player handoff、`gfxOffY`、Entity `oldVelocity` 到共享 motion-history、Ready 后 soundDelay 推进、`netUpdate`/`netUpdate2` 子步边界、每个完成子步的 timeLeft 到期释放、loop index、AI、collision、damage、trail、寿命与终止 | `partial`；world-boundary 分支仅支持显式 snapshot 下的非 minion 停用；末子步 minion/sentry 与越界 minion 会报告缺少 Player owner adapter；`gfxOffY` 和 `oldVelocity` 局部映射、network update flag 组合未验证；通用 motion-history writer/scheduler 与其余 Entity 继承状态未闭合；无 AI adapter 映射其余 Version4 分支，Main 接线、loop index、完整单槽位行为和副作用仍需独立验证 |
| combat | projectile damage gate、damage hitbox transform、NPC/PVP pre-collision filters、54 个显式类型分支及未列明合法类型的 generic hitbox fallback、type 871 pellet-storm、AI style 15 golf-ball ellipse、AI style 190 dual-cone geometry、31 个线段类型、11 个矩形/扫掠类型、7 个历史轨迹类型及 radial/cone/LOS 分支已映射但未验证；AI style 203 stub、反射/Jellyfish 和命中后专用分支未映射 | 组合 Query 无生产 caller；candidate+几何结果不等于最终命中资格，完整目标快照重查、瓦片区域选择/捕获及权威碰撞组合调用、accepted-hit commit 对 local/static immunity、owner immunity 与 penetration 的协调及 Player/NPC owner handoff 仍未实现 |
| network | packet 27/29 payload 编解码、双向 DTO 映射、trusted owner projection、Gateway registration、hostile/type/identity/UUID、section recipient、net spam、损坏/重复/重放数据 | codec、DTO mapping、registration helper 和 owner port 已存在但本次未验证；具体 owner、生产 Gateway 注册、服务器 hostile guard、relay、section/recipient、net spam 和完整重放策略均 `not-run` |
| Query 纯度 | 固定 snapshot 重复性、随机/时钟读取、缓存失效、全局标志、Command 提交重查 | 有副作用的入口重分类为 Command/Adapter；不得公开成纯 Query |
| stub 与专用行为 | 高尔夫球、Shimmer、storm lightning 等当前目标 stub；minion/counterweight/fishing/mining/kite 路径 | 先确认目标 Version4 意图；未解决保留 `unknown`，不套用另一版本实现 |
| 集成与副作用 | 声音、尘埃、粒子、实体 spawn、tile/worldgen、channel、持久化和共享 ID | 归属写入 integration handoff；未对齐前不删除旧 owner |

## 具体阻塞与决策记录

### CPG 与源码差异

2026-10-01 文档补证使用仓库只读 Query API 连接 `D:\TRbackup\Version4-cpg-export\out-dop8-interproc.sqlite`。初始化元数据为 `ReadOnly=true`、`ImportStatus=complete`、manifest `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`、967 shards、`SourceSnapshotId=null`。`Find-CpgSymbols(FindOldestProjectile)` 返回 Projectile `int()` 符号；在 `Projectile.cs`、`MessageBuffer.cs`、`Main.cs`、`Player.cs`、`NPC.cs`、`Item.cs`、`WorldItem.cs` 7 个选定路径的 call-site 查询为 `complete` / 1 条，唯一来源为 MessageBuffer。Version4 当前源码 `Projectile.cs:10263` 还明确显示 `NewProjectile -> FindOldestProjectile()`，故该关系是 CPG/source `evidence-gap`，不是“无调用”。

同一只读 Query API 会话中，`Find-CpgSymbols(Kill:void())` 按 `Terraria/Projectile.cs` 声明路径返回 `complete`；对 `Terraria/MessageBuffer.cs` 的 `Find-CpgCallSites` 返回 2 个 `CallTargets`，与 Version4 Packet 29 分支及另一处 MessageBuffer Kill 路径一致。该静态关系不闭合 `Kill` 的 channel、声音、伤害、子弹、网络等副作用，故实现只提交 `NetworkTermination` 生命周期边界。

同一会话补查 `Update:void(int)` 与 `PreUpdateAllProjectiles:void()`：两个符号按目标声明路径返回 `complete`；`Update` 在选定的 `Terraria/Main.cs` shard 中返回 2 个静态调用点，`PreUpdateAllProjectiles` 返回 1 个静态调用点。`Kill:void()` 在选定的 MessageBuffer/Main/Projectile 路径查询为 `partial`，`MaxItems=100` 命中 `ItemBudgetExhausted`，返回结果包含 MessageBuffer 与 Projectile 内部调用但不构成完整 caller 集。`Update` callable facts 为 `partial` / `CalleeEffectsNotExpanded`；局部操作节点显示先调用 `DecrementLocalImmuneTimeCounters`、设置 `numUpdates = extraUpdates` 并进入循环，但 callee effects 未展开。2026-10-04 逐行复查 Version4 `Projectile.cs:14702-14705`、`:15232-15239` 后更正了此前一次性寿命推进的假设：`numUpdates` 在子步开头递减，`timeLeft--` 位于子步 body 尾部，故每个执行子步都推进寿命；AI/命中路径还可把 `numUpdates` 置为 0 延长循环。NLTX coordinator 已改成这一计数/寿命粒度，并新增 `Completed`/`Continued`/`Returned` 子步结果以表达尾段推进、`continue` 与 `return`。兼容 `void` adapter 默认仍映射为 `Completed`；尚无 AI adapter 将 Version4 分支映射到这些结果。Main 顺序、免疫计数的 regular Update 粒度和源码子步事实已确认；完整单槽行为、`ProjectileUpdateLoopIndex` 可见性及权威 Main 接入仍未闭合，coordinator 继续标为 `partial`。

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

2026-10-04 静态搜索 `src/**/*.cs` 未发现 `ProjectileLifecycleSystem`、`ProjectileTickCoordinator`、`TryApplyNetwork`、`TryTerminateNetwork`、`ProjectilePacketGatewayRegistration.Register` 或其 Application owner 的生产实现/调用点；这些仍是类型、方法和 port 声明。正式 `src` 未找到 `Main.cs`、`MessageBuffer.cs` 或 `NetMessage.cs` 调度/接收实现，唯一 `MainHost` 命中属于 WorldGeneration 基础设施。

Network Infrastructure 已提供通用 `PacketGateway.Register<TPacket>`（`PacketGateway.cs:62`）和 Application `IPacketHandler<TPacket>`；新增的 `ProjectilePacketGatewayRegistration` 可注册 packet 27/29，并将 wire owner 替换为绑定 actor（type 949 除外，owner 为 255）。`NSSLC.Application.csproj` 现引用 Projectile 域并声明 `IProjectileNetworkCommandOwner`，该 port 明确把 hostile policy、world/session 查找、lifecycle commit 和 relay 留给领域组合 owner。当前仍无具体 owner 实现或 `new NetworkGatewayHost`/registration 生产调用；packet handler 只完成接收 DTO 到受信 command 的映射，不构成完整 packet 处理。Main 调度 owner 也仍未建立，故不新增第二个 coordinator。

### 2026-10-03 packet payload codec slice

Version4 `NetMessage.cs:769-842` 给出 packet 27 的字段顺序与可选位；`MessageBuffer.cs:1320-1404` 给出接收顺序。`NetMessage.cs:852-855` 和 `MessageBuffer.cs:1439-1452` 给出 packet 29 的 identity/owner 字段。`src/NSSLC.Infrastructure/Network/ProjectilePacket27Codec.cs` 与 `ProjectilePacket29Codec.cs` 将现有 `D:\ProjectItem\SourceCode\Net\NetWork` 的 `Packet27Codec`/`Packet29Codec` 接到 P15 Command 边界，不再重复实现字段编解码。payload 不含外层 packet frame/header。

Codec 将截断、尾随数据和不能表示为当前 Command 的字段分别报告为 `Truncated`、`TrailingBytes`、`Invalid`；packet 27 的 UUID 小于 0 或不小于 1000 时按 Version4 接收路径规范化为未提供。Codec 保留 payload 中的 owner；Gateway handler 随后按 Version4 入口将 packet 27 的普通类型 owner 替换为绑定 actor、type 949 替换为 255，并将 packet 29 owner 替换为绑定 actor。Application owner 仍须执行 `projHostile[type]` 决策、lifecycle 提交和 relay/recipient 输出。handler 返回 `Accepted=false` 会由当前 Gateway 作为协议拒绝关闭连接；需要保持 Version4 静默无效请求语义时，owner 应返回 accepted 且不产生 dispatch。尚无具体 owner 或生产注册调用。

按用户要求，本轮没有运行 build 或 test。此次 codec、项目引用和字段边界均未执行编译或行为验证，状态保持 `not-run`。

### 2026-10-03 immunity counter mapping

Version4 `Projectile.cs:15292-15315` 的 `DecrementLocalImmuneTimeCounters` 每次 `Update` 都递减 255 个 `playerImmune` 计数；仅当 `usesLocalNPCImmunity` 为真时才递减 `localNPCImmunity` 数组。`ProjectileHitImmunitySystem.AdvanceTick` 保留这两条规则，并停止递减 `restrikeDelay`：目标源码只在 `SetDefaults` 中将该字段设为 0，没有由此方法或其他调用点递减它。组件字段仍保留以记录 P15 成员，但当前不在此免疫计数路径推进。

此静态修正未运行 build/test；行为验证仍为 `not-run`。

### 2026-10-04 packet DTO 双向映射入口

`PacketConnection.ReadPacketAsync` 经 `ProtocolProfile` 的 `PacketBinding<TPacket>.Decode` 返回已解码的 packet 对象；27/29 对应 `Packet27Packet.Payload` 与 `Packet29Packet.Payload`。`ProjectilePacket27Codec.Map(Packet27Payload)` 和 `ProjectilePacket29Codec.Map(Packet29Payload)` 将这些对象映射到现有领域命令，字节 `Decode` 也复用相同映射逻辑，避免未来 handler 对同一 payload 再跑一次 codec。反向 `TryMapToPacket` 将领域命令映射到生成 packet wrapper，profile binding 可直接处理发送编码；packet 27 的真实 UUID 类型规则由 binding 中冻结的协议事实核验。

Codec 映射保留 payload 声明的 owner；新增 Gateway handler 随后按 Version4 接收逻辑把 packet 27 普通类型 owner 投影为 `NetworkSessionContext.Actor.PlayerSlot`，type 949 投影为 255，并把 packet 29 owner 投影为受信 actor。它不执行 packet 27 的 `projHostile[type]` 决策、服务器 projectile spam 计数、生命周期提交或 relay/recipient 选择；这些属于尚未实现的 Application owner。`MessageBuffer.cs:1340-1404` 表明 hostile 过滤、实例选择、desperation fix 与 relay 在旧接收调用路径。独立 packet 27 body `Encode(command, includeUuid)` 只核对传入 UUID 存在性与命令字段一致，调用方仍需传入实际 `NeedsUUID[type]` 事实；profile packet DTO 路径由绑定事实作类型核验。Gateway registration helper 与 Application owner port 已有代码，但无生产 owner/注册调用，集成仍未闭合。

本轮未运行 build 或 test；mapper、此前 codec 与集成行为均未编译或验证，`verificationStatus` 保持 `not-run`。

### 2026-10-04 soundDelay 子步边界接线

Version4 `Projectile.SetDefaults` 将 `soundDelay` 设为 0（`:498`）；`Projectile.Update` 在每个 `extraUpdates` 子步内，仅在世界边界/minion 早退与 type-640 AI 早退之后、调用 `AI()` 之前递减正值（`:14714-14801`）。多个 AI 分支也直接将该字段递减到 `-20`（`:38151-38155`），所以它不是限定于 `-1` 的布尔哨兵。`ProjectileEntityState` 现在承载 `ProjectileEffectCooldownStateComponent`；状态构造默认值为 0，组件不再拒绝 AI 使用的负值。

`IProjectileTickAdapter` 现在提供 `PrepareProjectileStep` 阶段，以准备结果表达是否到达 soundDelay 边界、是否应结束当前 projectile update、是否需提交世界边界停用，或是否缺少跨域 owner。Coordinator 只在 `Ready` 且同一 generation 仍活动时调用 `ProjectileEffectCooldownSystem.Advance`，然后再进入 `UpdateProjectileStep`；提前结束会跳过冷却、AI 和寿命尾段。默认实现通过 `PrepareProjectileStep` 依序处理 Version4 type-640/ai[1] 倒计时（`:14706-14709`）、可选世界边界停用（`:14711-14728`）、末子步 minion/sentry 的 Player owner handoff（`:14736-14769`），再推进 `gfxOffY`（`:14770-14800`）。在该前缀准备结果为 `Ready` 后，coordinator 才递减正值 soundDelay。自定义 preparation override 必须保留这些已接入规则及顺序。其它 AI 写入和权威 Main 接入仍未实现，字段/行为仍为 `partial`。按用户要求没有运行 build 或 test，当前接线未编译验证。

### 2026-10-04 reflected 单一状态归属

Version4 `Projectile.reflected` 在目标文件中只命中字段声明（`:150`）与 `SetDefaults` 重置为 `false`（`:514`），没有其他源码读写点。NLTX 先前同时把同名状态放在 `ProjectileGeometryStateComponent` 和 `ProjectileReflectionStateComponent`，但 geometry 副本没有消费者。现已移除 geometry 副本，并把专用 `ProjectileReflectionStateComponent` 纳入 `ProjectileEntityState`；默认/重新 hydration 状态仍为 `false`。这只修正状态表示，不证明存在运行时反射行为，字段 owner 与 System 写入仍属 proposed。

本轮未运行 build 或 test；状态归属和构造器签名修改尚未编译验证。

### 2026-10-04 specialized rule constants

Version4 `Projectile.cs:296,304` 将 `StormLightningLiquidDamageRadius` 固定为 `500`、`MinimumWindStrengthToFlyKite` 固定为 `0.2f`。按 P15 成员表所提的 `ProjectileSpecializedDefinitionQuery` owner 增加同名常量。目标 `AI_203_StormLightning` 仍为空方法，kite/storm 行为及其调用关系没有因此实现或验证；当前查询只映射这两个无上下文依赖的规则值。

本轮未运行 build 或 test；常量定义尚未编译或接入行为调用点。

### 2026-10-04 derived properties slice

Version4 `Projectile.cs:312-386` defines `Opacity` as `1 - alpha / 255`; its setter clamps
`(1 - value) * 255` to `[0, 255]` before integer conversion. The presentation component now
maps both accessors to its `Alpha` field. `MaxUpdates` reads `extraUpdates + 1` and writes
`extraUpdates = value - 1`; the cadence component now exposes both directions. These component
accessors preserve the Version4 transforms but do not establish an authoritative production
writer or Main scheduling path. Version4's setter performs no range check; the coordinator now
uses the mutable `numUpdates` loop, so negative initial values produce no delegated substep. Full
behavior for these values remains unverified.

`OwnedBySomeone` evaluates to `!npcProj && !trap`. `CareForAttackCD` requires owner melee hit
cooldown, that ownership result, and `owner < 255`. The new pure query reads explicit component
snapshots: `Source.IsNpcProjectile`, `Trap.IsTrap`, `HitImmunityPolicy.UsesOwnerMeleeCooldown`,
and `Identity.OwnerSlot`. Definition hydration already maps `combat.NpcProjectile` into source
metadata; Version4 NPC spawn paths also set `npcProj = true` after projectile creation
(`NPC.cs:44639,44907,44945,45062,45078,45094,45101,45107`), and those runtime callers are not
connected here. `WipableTurret` now has a pure mapping for the local-owner/sentry gates and
Version4 `TurretShouldPersist` rule when given an explicit local player slot and Defender event
snapshot. Its special projectile IDs come from `Projectile.cs:420-437`. `NetSectionCoordinates`
now maps the Version4 float-position-to-tile and tile-to-section transforms using section sizes
from `Netplay.cs:497-509`. These queries have no production call sites. `Name` remains
unimplemented because localization is not available through this boundary. The owner-minion
target slot projection is recorded below; resolving it to current NPC entity state remains an
NPC-owner integration.

This slice was not built or tested. The query has no production call sites; verification remains
`not-run`.

### 2026-10-04 OwnerMinionAttackTargetNPC slot projection

Version4 `Projectile.OwnerMinionAttackTargetNPC` reads
`Main.player[owner].MinionAttackTargetNPC`, returns null for a negative slot, and otherwise
indexes `Main.npc` (`Projectile.cs:350-359`); `Player.MinionAttackTargetNPC` defaults to `-1`
(`Player.cs:2388`). `ProjectileDerivedPropertiesQuery.GetOwnerMinionAttackTargetNpcSlot` now
maps an explicitly supplied owner-player slot and target slot to `NpcSlot?`, rejecting a snapshot
whose player slot does not match `ProjectileIdentityComponent.OwnerSlot`. This keeps the query
pure and passes the legacy slot to the NPC owner for lookup. It does not resolve slot occupancy,
NPC generation, or a live entity reference, and it has no production caller. No build or test was
run; verification remains `not-run`.

### 2026-10-04 trajectory state, `numUpdates`, and per-substep lifetime

Version4 `SetDefaults` resets `spriteDirection` to 1 (`Projectile.cs:499`), `numUpdates` to 0
(`:519`), and `rotation` to 0 (`:533`). `stepSpeed` defaults to 1 (`:134`) and spawn resets
`gfxOffY`/`stepSpeed` to 0/1 (`:10280-10281`). Each `Update` assigns
`numUpdates = extraUpdates` then decrements it before each loop body (`:14702-14705`). Hit and
AI paths also assign `numUpdates = 0` (`:12706,16036,25758,25849`), so fixed `ExtraUpdates + 1`
iteration counts can omit body calls. `timeLeft--` is inside that same loop body (`:15232`), and
expiry/zero penetration are checked there (`:15233-15239`); lifetime therefore advances once per
executed substep. Immunity counters remain once per regular `Update` before the loop (`:14701`).

`ProjectileTrajectoryStateComponent` no longer duplicates `ai`/`localAI`, already owned by
`ProjectileBehaviorStateComponent`; it now holds `Rotation`, `SpriteDirection`, `StepSpeed`,
`NumUpdates`, and `GfxOffY` and is included in `ProjectileEntityState`. The coordinator initializes
`NumUpdates` from `ExtraUpdates`, decrements before adapter preparation, permits the subsequent AI
owner to reset it, advances positive `soundDelay` only after preparation returns `Ready`, and
advances lifetime only after a `Completed` outcome. It no longer rejects negative initial cadence
values. `ProjectileTickPreparationResult` represents pre-AI `Ready`/`Continued`/`Returned`;
`ProjectileTickSubstepResult` represents post-preparation `Completed`/`Continued`/`Returned`.
Existing `void` adapters default to the type-640 countdown, supported world-boundary and owner
gates, and local `gfxOffY` motion before returning `Completed`. These semantics have not been built
or tested; the coordinator can consume an explicit world-boundary snapshot and commit non-minion
early deactivation, while out-of-bounds minions and terminal-substep minion/sentry cases require a
Player owner adapter. Calls without a snapshot do not apply the boundary check. Other AI branches remain unmapped, and no
production adapter or authoritative Main caller exists. Coordinator parity remains partial.

### 2026-10-04 world-boundary preparation slice

Version4 `Projectile.Update` checks world bounds after the type-640 countdown and before
enchantment/minion updates (`Projectile.cs:14706-14728`). The check is skipped for `aiStyle == 3`.
An out-of-bounds non-minion sets `active = false` and returns; it does not call `Kill` or clear
`timeLeft`. The minion branch additionally reads Player active/dead/center state and may request a
network update, so it remains an owner-adapter handoff.

`ProjectileWorldBounds` carries a validated immutable copy of the four world edges in
`ProjectileTickContext`. `ProjectileTickCoordinator.Tick(adapter, worldBounds)` makes that input
explicit. Default preparation preserves type-640 ordering, exempts behavior key 3, then identifies
an out-of-bounds instance. The coordinator asks the lifecycle owner to deactivate and release a
non-minion while preserving its `timeLeft`; the minion path reports `IntegrationRequired` until an
adapter supplies the Player owner behavior. The legacy `Tick(adapter)` overload has no world
snapshot and therefore does not run this check. This slice does not connect a production Main
caller, model the inactive Version4 slot object, or implement minion reposition/network effects;
it is not a full `Update` migration.

No build, test, or verifier was run; `verificationStatus` remains `not-run`.

### 2026-10-04 `gfxOffY` motion slice

Version4 `Projectile.Update` adjusts `gfxOffY` once per entered substep after the type-640,
world-boundary, and minion/sentry branches (`Projectile.cs:14770-14794`). The adjustment is
`(1 + abs(velocity.X) / 3) * stepSpeed`; it moves a positive or negative offset toward zero,
clamps a sign crossing to zero, then clamps the result to `[-16, 16]`. The default preparation
path applies this rule before the shared positive `soundDelay` decrement. This is a local motion
mapping only; it does not implement the subsequent wet-state sampling, `HandleMovement`, tile
collision, or the AI branches that may mutate `gfxOffY` and `stepSpeed`.

`ProjectileWorldBounds` also exposes validity explicitly and rejects the all-zero `default` value
at the coordinator/context boundary, so the validated-constructor invariant cannot be bypassed by
passing a default struct. No build, test, or verifier was run; these source changes remain
uncompiled and `verificationStatus` remains `not-run`.

### 2026-10-04 inherited motion-history ownership check

Version4 declares `oldVelocity`, `oldPosition`, `oldDirection`, and `direction` on the base
`Terraria.Entity` (`Entity.cs:6-20`); `Projectile` inherits them (`Projectile.cs:32`). Although
`Projectile.Update` refreshes `oldVelocity` after the early-out prefix (`Projectile.cs:14795-14801`),
it is outside the authoritative P15 inventory of 126 declared Projectile members. The default
preparation now writes the current velocity into the already referenced
`EntityEcs.Components.MotionHistoryComponent.PreviousVelocity`; it does not add a duplicate field
to `ProjectileKinematicsStateComponent`. `SpatialSimulation` contains a second motion-history
component marked `crossSubsystemOwner=integration-review`, while no generic Entity writer or
production tick caller is connected. Selection of the shared component for this local mapping
does not close the broader history owner, direction/position snapshot, or scheduling integration.

No build, test, or verifier was run; `verificationStatus` remains `not-run`.

### 2026-10-04 primary network update substep boundary

Version4 decrements positive `soundDelay` and then clears `netUpdate` before entering `AI()`
(`Projectile.cs:14798-14805`). Earlier type-640, world-boundary, or Player-owner returns skip this
reset. The `Ready` path in `ProjectileTickCoordinator` now advances `EffectCooldown` first, then
calls `ProjectileNetworkStateSystem.BeginProjectileUpdateSubstep`: it clears
`PrimaryUpdatePending` (`netUpdate`) while preserving `SecondaryUpdatePending` (`netUpdate2`) and
keeps `SendRequested` true only for that deferred request. This matches the start of an entered
normal substep; it does not implement the later local-owner gate, `netSpam` decrement/send budget,
packet emission, or relay at `Projectile.cs:15241-15269`.

No build, test, or verifier was run; the network state transition remains uncompiled and
`verificationStatus` remains `not-run`.

### 2026-10-04 damage admission gate slice

Version4 `Projectile.Damage_CanDealDamage` (`Projectile.cs:11480-11516`) first checks
projectile-type and `aiStyle` suppressions, then state-dependent `ai`/`localAI`/penetration/velocity
conditions, followed by the `Main.projPet[type]` exception list. `ProjectileDamageGateQuery`
maps these state predicates from the projectile aggregate and receives the pet-definition bit as an
explicit snapshot; pet animation exceptions use the mapped `Animation.Frame` and `FrameCount`.
Projectile types 833, 834, and 835 suppress damage only when `ai[0] == 4`, matching the source
condition rather than suppressing every `ai[0]` value.

This query only decides whether execution may enter the damage path. It has no production caller and
does not implement `Damage()` target enumeration, NPC/Player eligibility, owner hit checks,
collision, immunity, hit commitment, penetration consumption, damage dispatch, or kill/relay side
effects. The combat stage remains partial. No build, test, or verifier was run;
`verificationStatus` remains `not-run`.

### 2026-10-04 NPC damage candidate filters

Version4 `Damage()` only enters its NPC/Player damage enumeration when `owner == Main.myPlayer`
(`Projectile.cs:11523-11536`). `Damage_PVE` then rejects non-positive damage, selects a shared
Stardust Dragon local-immunity array for types 626-628 when available, checks projectile local or
static immunity and the owner's melee hit cooldown, and skips inactive/invulnerable NPCs or the
`aiStyle == 112 && ai[2] > 1` state (`:11554-11590`). `Damage_PVE_Inner` applies friendly/hostile
and owner-target rules, the one-hit owner-immunity exception, projectile/NPC-type and trap/immortal
immunity, and the owner-hit-check rule before calling `Colliding` (`:11594-11655`).

`ProjectileNpcDamageCandidateQuery` maps those pre-collision filters from projectile state plus
explicit NPC, Player, selected local-immunity, pet-definition, and collision-line snapshots. Its
registry overload reads ordinary local immunity from the projectile component and static immunity
from `NpcSlot` and the supplied game update count instead of trusting those context flags. Types
626-628 still require the context to capture Version4's selected local array (shared Dragon head
when found, otherwise the projectile's own array). `ProjectileDamageCollisionQuery` has a matching
overload that uses this registry-backed candidate path before geometry evaluation. A
`ReadyForCollisionTest`
result means only that the source filters reached the geometry call; it does
not mean the projectile intersects the NPC or may commit damage. The query does not enumerate NPCs,
capture/validate target generations, execute `CutTiles` or powder effects, apply the post-collision
Jellyfish/reflection/type-876 behavior, implement PVP, or commit NPC/Player damage, penetration, or
hit immunity. It has no production caller. No build, test, or verifier was run;
`verificationStatus` remains `not-run`.

### 2026-10-04 PVP damage candidate filters

Version4 `Damage_PVP` returns when damage is non-positive or the local damage owner is not
hostile, then scans player slots 0-254 in order. It skips the projectile owner and targets that are
inactive, dead, generally immune, non-hostile, under this projectile's `playerImmune` cooldown,
or on the local owner's nonzero team (`Projectile.cs:13155-13181`). When `ownerHitCheck` is set, it
also requires `CanHitWithMeleeWeapon(player)` before `Colliding`.

`ProjectilePvpDamageCandidateQuery` maps these per-target pre-collision filters from explicit local
owner and Player snapshots plus the projectile's player-immunity array. It rejects slots outside
the Version4 0-254 scan. `ReadyForCollisionTest` still requires the adapter's projectile/player
geometry check. The query does not reproduce PVP status effects, hit damage randomization, Player
hurt/dodge, enchantment/heal procs, network hurt messages, immunity writes, penetration, or
AI-specific hit mutations; it has no production caller. No build, test, or verifier was run;
`verificationStatus` remains `not-run`.

### 2026-10-04 damage hitbox transformation

Version4 `Damage_GetHitbox` (`Projectile.cs:13398-13459`) starts with integer-truncated
position and current width/height, then applies the phaseblade `ai[0] == 2` shrink, types 301/383/262
growth hitbox and `localAI[0] = -1` write, type 101/1024/1023/85/1106/188/967 expansions, and the
`aiStyle == 29` expansion. The type 85 and 1106 radii use clamped `Utils.Remap` over
`localAI[0]` in `[0, 72]` (`Utils.cs:249-285`).

`ProjectileDamageHitboxSystem` maps these ordered rectangle transforms and the three local-AI
consumption branches. The phaseblade set bit is explicit input because the production definition
catalog does not yet supply the Version4 `ProjectileID.Sets.IsAPhaseblade[type]` table. The hitbox
value preserves non-positive width/height produced by legacy rectangle inflation. This is only the
input rectangle builder. `ProjectileCollidingGeometryQuery` maps selected
`Projectile.Colliding` branches (`:13735-14176`) for 47 projectile types: 455, 461, 464, 466, 537,
580, 598, 607, 611, 614, 623, 632, 636, 642, 684, 686, 687, 697, 699, 707, 711, 756, 758, 802, 842,
872, 877-879, 919, 923, 927, 932, 933, 938-945, 961, 963, 974, 1041, 1093, and 1100. This includes 31 line-geometry types, 9
rectangle/sweep types, 7 history-trail types, and type 464's six-rectangle orbit. It preserves each branch's
center/velocity/rotation, scale, local-AI and generic-hitbox inputs, and follows
`Collision.CheckAABBvLineCollision` (`Collision.cs:97-182`) including its expanded-AABB broad phase
and rotated edge checks. The `ProjectileCollidingGeometryResult.Unsupported` result remains for
other types and style-driven branches that take precedence, including melee/thrust and AI styles
19/137/203. AI style 190 now has its own slow-cone mapping below. Whip geometry now consumes the
cached control-point snapshot when supplied; snapshot production and the production Query caller
remain absent. The existing `SpatialCollisionQuery` only provides AABB checks, and its
`SpatialGeometrySnapshot` rejects negative dimensions even though `Damage_GetHitbox` can preserve
non-positive rectangles after inflation; the searched production tree has no reusable projectile
line-collision helper, so this query keeps those legacy rectangle and line semantics local.
`ProjectileDamageCollisionQuery` composes NPC/PVP candidate filters with this geometry result;
the caller supplies the already-built damage hitbox, target rectangle, inherited `Entity.direction`,
owner `MountedCenter`, and `Collision.CanHit`/`CanHitLine` results as snapshots. The whip set bit
is mapped from Version4's `ProjectileID.Sets.IsAWhip` values, while whip control points still need
an external snapshot producer.
This composition does not enumerate target slots, provide production snapshots, or perform line of
sight itself. It also does not run post-collision effects, recheck mutable state at commit, or apply
damage. The composition
has no production caller. `PlayerCombatResolutionSystem` requires a committed-hit command with
event/source identity, timestamp, cooldown and proc inputs; `NpcCombatSystem.ResolveAndCommit`
requires its health owner, contributor and damage-policy context. The projectile module has no
target snapshot producer or wiring to either owner, and `WorldStorageRoot.Npcs` still stores generic
world entity state. The coordinator now invokes `ProjectileTrailCacheSystem.Record` after completed
substeps, but no authoritative tick caller or mode 4 Player snapshot exists, so no live projectile
path supplies trail history yet. No safe
damage-commit adapter can be formed from the current inputs without inventing those owners and facts.
No build, test, or verifier was run; source changes remain uncompiled and
`verificationStatus` remains `not-run`.

### 2026-10-04 selected trail, radial, and bounded-line collision geometry

The history branches follow Version4 dispatch order. Types 466, 580, and 686 use the generic
`myRect` intersection before scanning `oldPos` until the first `(0, 0)` sentinel; type 711 uses the
same generic intersection and scans history only when `penetrate != -1`. Type 872 checks exactly
`oldPos[0], [2], ..., [78]`, using the projectile's base hitbox and skipping zero positions.
Types 933 and 1100 inspect every fifteenth cached position starting at index 14, accept trail ages
from 0 through 60, test the rotated 40-unit historical line, then test the current line.

Type 623 combines the generic hitbox with an 80×40 rectangle offset by
`Entity.direction * 40` when `ai[0] == 2`; the query receives inherited direction as explicit input.
AI style 15 with nonzero `ai[0]` skips this inner rectangle and uses the generic hitbox path; AI
style 190 takes its earlier dual-cone branch, which is mapped below. Type 464 first uses the generic
hitbox, then—unless `ai[1] == 1`—tests six 30×30 rectangles around
the rotating offset derived from `ai[0] % 45` and velocity rotation. Types 877-879 test a 95-unit
rotated line from the projectile center within the 300×300 lance bounds; types 919 and 932 test a
40-unit line in both directions with the same bounds. Type 923 tests three forward lines with
lengths 510/660/800 times scale and widths 70/42/7 times scale. Type 974 tests a 46-times-scale
line in both directions, width 8 times scale, after broad-phase intersection with the base hitbox
inflated by the truncated 46-times-scale amount. The type 877-879, 919/932, 923, and 974 branches
return their own result without the generic rectangle fallback; AI style 15 skips these inner
branches, so its nonzero-AI path remains the generic rectangle check. These mappings use supplied
projectile state and target rectangles. `ProjectileTrailCacheSystem` maps Version4's 142 explicit
type-to-mode entries and the local history-array updates for `TrailingMode` 0-5; definition
hydration stores the type-derived mode in presentation state. Its post-behavior `Record` method has
no production caller. Mode 1 Dust effects and the mode 4 Player movement snapshot remain outside
this mapping, so history-backed branches still lack runtime trail data. The composite NPC/PVP
query also has no production caller or snapshot producer; effects, commit-time rechecks, and damage
owner handoff remain unmapped. Local immunity and penetration mutators plus the static registry
accepted-hit/reset APIs exist, but no accepted-hit commit calls them or coordinates them with
owner-immunity clearing. No tests, build, or verifier were run at the user's
request; these source changes remain uncompiled and `verificationStatus` remains `not-run`.

### 2026-10-04 trail cache mode mapping

Version4 records trail state after behavior-specific work (`Projectile.cs:15122-15226`). The local
`Record` now mirrors the six array-update modes: mode 0 shifts position only; mode 1 applies the
`frameCounter == 0 || oldPos[0] == Vector2.Zero` gate and shifts position only; mode 2 shifts and
records position, rotation, and sprite direction; mode 3 performs that full update followed by the
0.65 positional interpolation and per-entry rotation update; mode 4 shifts full history and applies
the supplied owner movement delta to nonzero cached positions when `numUpdates == 0`; mode 5 records
rotation from velocity. Unknown modes leave history unchanged. Mode 1's type 466/580 Dust emission
stays outside this state System because it creates randomized effects.

The local `GetTrailingMode` maps all 142 explicit type/value pairs in Version4's
`ProjectileID.Sets.TrailingMode` initializer (`ProjectileID.cs:291`) and returns the set's default
`-1` for unlisted types.
Definition hydration stores this type-derived mode in `ProjectilePresentationStateComponent`.
The coordinator now calls `Record` after an adapter reports a completed substep and before advancing
the projectile lifetime; `Continued` and `Returned` outcomes skip trail recording. For mode 4, the
new `TryGetOwnerMovementDelta` adapter input must provide
`Main.player[owner].position - Main.player[owner].oldPosition`. Its default reports unavailable and
the coordinator fails explicitly if a mode 4 projectile reaches this point without the snapshot.
No Player snapshot producer or authoritative Version4 Main coordinator caller exists for history-backed projectile paths; the current Tools.Simulation adapter supports only type 1 / AI style 1. Mode 1 Dust effects also
remain unmapped. The local call therefore closes the coordinator-to-trail-state edge but does not
provide a complete runtime trail path. No tests, build, or verifier were run at the user's request;
`verificationStatus` remains `not-run`.

The completed-substep tail now also terminates a projectile whose penetration reached zero, after
the lifetime decrement/expiry check, using `ProjectileEndReason.HitLimitReached`. This follows
Version4's trail, `timeLeft--`, expiry, then `penetrate == 0` order (`Projectile.cs:15122-15237`).
The non-hydrated `ProjectileEntityState` constructor now starts penetration at 1, matching the
Version4 field default (`Projectile.cs:156`), so lifecycle-created states are not mistaken for an
already depleted projectile.
`IsADD2Turret[type] && DD2Event.Ongoing` time-left extension (`:15228-15230`) is now mapped locally;
the production event-snapshot adapter and authoritative coordinator caller remain absent, as does
the production hit commit that consumes penetration.
No tests, build, or verifier were run at the user's request; `verificationStatus` remains
`not-run`.

### 2026-10-04 Old One's Army turret projectile lifetime

Version4's `ProjectileID.Sets.IsADD2Turret` set contains types 663, 665, 667, 677, 678, 679, 688,
689, 690, 691, 692, and 693 (`ProjectileID.cs:301`). After trail recording, `Projectile.Update`
increments `timeLeft` for those types while `DD2Event.Ongoing`, then performs the ordinary decrement,
expiry check, and penetration-zero check (`Projectile.cs:15228-15237`).
`ProjectileSpecializedDefinitionQuery.IsAdd2Turret` maps the set, and the tick coordinator requests
the event snapshot through `IProjectileTickAdapter.TryGetDefenderEventOngoing` only for those
projectiles. When the snapshot says the event is ongoing, the coordinator applies the matching
increment before `ProjectileLifetimeSystem.Advance`. `ProjectileDerivedPropertiesQuery` now reuses
the same type mapping for `WipableTurret`, avoiding a second set of type IDs.

`Terraria.WorldSession.Calendar.Dd2RunStateComponent.Ongoing` is the current world-session source
for this event fact, but no adapter projects it to the projectile tick. The coordinator still has
no authoritative Main caller. A missing event snapshot fails explicitly. No tests, build, or
verifier were run at the user's request; `verificationStatus` remains `not-run`.

### 2026-10-04 AI-style, mounted-center, and visibility-gated collision branches

The selected geometry mapping now covers 54 projectile types. AI style 15 with `ai[0] == 0`
uses the owner Player's `MountedCenter`: it finds the target rectangle's closest point, scales the
vertical offset by `1 / 0.8`, then compares its length with 55. That center is an explicit input.
For AI style 15 with nonzero `ai[0]`, the early type branches for 85, 871, 872, 933, 973, 985, 1100,
1106 and the other nested geometry types resolve through the generic hitbox only; the type 661 gate
is also skipped because it is inside that nested branch. The history lines for 933 and 1100 are not
evaluated on this path.

Types 85 and 1106 require the supplied damage hitbox to intersect before using the explicit
`Collision.CanHit(projectile center, target center)` snapshot. Type 985 accepts its fast cone test
only when that same visibility snapshot is true, then retains the generic hitbox fallback. For
type 973, Version4 calls `SafeNormalize` on the closest-point offset before checking its length;
the Query preserves the strict raw-offset range test; the discarded `UnitX` fallback does not alter its
result.

Type 661 first gates on the distance between integer rectangle centers (at most 500) and the
explicit `Collision.CanHitLine(myRect.Center, targetRect.Center)` snapshot, then falls through to
the generic hitbox intersection. Type ordering also determines when the whip and AI-style 19/137/203
branches can take precedence; the query keeps later line/trail branches unsupported for those styles,
while preserving earlier type branches. Target-size checks on 598/614/636 and the `ai[0] >= 2`
condition on 963 also determine whether those AI-style branches take over. The query API accepts the
owner mounted center and both LOS results as explicit snapshots; it has no snapshot producer or
production caller. The mapping is
still local geometry only: target enumeration, cache recording, post-hit effects, commit-time
rechecks, and damage ownership remain open. Local/static immunity and penetration write APIs exist,
but an accepted-hit commit that coordinates them with the NPC/Player damage owner remains missing.
No build, test, or
verifier was run at the user's request; source remains uncompiled and `verificationStatus` remains
`not-run`.

For AI style 190, Version4's dual-cone rule runs after the four earlier type branches (85, 973,
985 and 1106) and returns its own result without a generic-hitbox fallback. The query maps its
94-times-scale cone, rotation offset from `ai[0]`, strict range/angle checks over the closest cone-end
point and four target corners, and the second cone driven by clamped `Remap(localAI[0], ai[1] * 0.3,
ai[1] * 0.5, 1, 0)`. AI styles 19/137/203 remain unresolved where their ordered branches own the
result. The whip branch now tests a rectangle of the supplied hitbox size centered on each cached
control point, matching Version4's `ToPoint` and `myRect.Location` loop (`Projectile.cs:13887-13895`).
An empty/unavailable control-point snapshot returns `Unsupported`; no production snapshot producer
or geometry caller exists. This remains an uncalled calculation mapping; no tests, build, or verifier
were run at the user's request.

Type 871 maps the pellet-storm branch before whip and AI-style collision checks. The query uses the
existing `ProjectileStormDefinitionFactory` for six storms, checks each of the three bullet
rectangles only when its normalized progress is in `[0, 1]`, and tests those rectangles against the
target. Version4's `AI_172_GetPelletStormsCount` returns six and its storm definition uses
`localAI[0]` (`Projectile.cs:13869-13877,33700-33721`). When no storm bullet intersects, the source
returns `false` immediately; it does not try the generic hitbox or AI style 137/19/203. AI style 15
bypasses this branch and uses the generic hitbox for nonzero `ai[0]`. The query now preserves both
paths. This geometry remains uncalled by a production damage path. No tests, build, or verifier were
run at the user's request.

### 2026-10-04 AI style 137 visibility-snapshot geometry

Version4 `Projectile.Colliding` reaches the AI style 137 branch after the earlier type-specific
branches and before the generic-hitbox fallback (`Projectile.cs:13942-13954`). It first requires
`myRect` to intersect the target and the target rectangle's distance from `Projectile.Center` to be
strictly less than `height / 2 - 20`. It then checks `AI_137_CanHit` at the target center, followed
by the center of the target's top edge. That helper reads solid-tile state and calls
`Collision.CanHitLine`, including its two-segment curved-path fallback (`Projectile.cs:46329-46357`).

`ProjectileCollidingGeometryQuery` maps the ordered hitbox/range checks and consumes the visibility
results through `ProjectileAi137VisibilitySnapshot`. `ProjectileAi137VisibilityQuery` now computes
those results from an explicit `SpatialCollisionSnapshot` and the world tile dimensions. It maps the
`WorldGen.SolidTile` target gate and the tile walk in `Collision.CanHitLine`, including the two
curved fallback paths (`WorldGen.cs:58770-58793`, `Collision.cs:414-606`,
`Projectile.cs:46329-46357`). `SpatialTileSnapshot` now carries nullable `IsInactive`, which both
source helpers read; omitted values remain unknown. The calculation follows Version4's center-first
short circuit; it returns an incomplete snapshot when a required tile fact is absent or the tile list
contains duplicate coordinates, so the collision query reports
`Unsupported` instead of guessing. It performs no live world reads and is repeatable for the supplied
tile snapshot. `LegacySpatialTileSnapshotAdapter.TryCapture` now projects a bounded runtime `Main.tile` region
to `SpatialTileSnapshot`, including activeness, actuation, type collision flags, shape and liquid
facts. It returns no partial result when a requested coordinate or tile fact is unavailable.
Capture must run on the tile-owning thread at the same simulation boundary as the subject
snapshot. The production damage-collision Query caller remains missing. AI
style 19 geometry is recorded in the following section; AI style 203 remains unresolved because
the Version4 `StormLightningCollisionCheck` body is stubbed, so its intended behavior remains
`unknown`. No build, test, or verifier was run at the user's request; `verificationStatus` remains
`not-run`.

### 2026-10-04 shared tile CanHit and CanHitLine queries

Static source comparison against Version4 `Collision.CanHit` (`Collision.cs:197-305`) and
`Collision.CanHitLine` (`Collision.cs:414-606`) found that the shared queries preserve endpoint
integer conversion, world-border clamps, dominant-axis stepping, neighbor checks, and the respective
solid/top/inactive/shape rules. `SpatialCanHitQuery` and `SpatialCanHitLineQuery` now live in
SpatialSimulation. A missing tile record or required `IsInactive` fact returns an incomplete
calculation; a recorded absent tile maps the legacy null-tile false result. The Projectile collision
environment query computes `CanHit` for types 85, 973, 985, and 1106, `CanHitLine` for type 661,
and AI style 137 visibility from one explicit `SpatialCollisionSnapshot`. The NPC/PVP composition
overloads invoke that producer after candidate filtering; type 661's 500-pixel range gate stays in
the geometry query.

`LegacySpatialTileSnapshotAdapter.TryCapture` projects only the tile-aligned region supplied by its
caller. No runtime caller under `src/` chooses and captures the required region, invokes the NPC/PVP
composition overload, or connects it to an authoritative projectile update loop. Thus the snapshot
calculations exist, but production world-to-query integration and authoritative Main scheduling do
not. The new Queries consume explicit snapshots and perform no live reads or writes; they do not
reproduce Version4 `CallTracker` diagnostic writes from `Collision.CanHit`, `Collision.CanHitLine`,
`Utils.ToPoint`, or Tile getters. No tests, build, or verifier were run at the user's request;
`verificationStatus` remains `not-run`.

### 2026-10-04 AI style 19 spear-extension geometry

Version4 Projectile.Colliding calls AI_019_Spears_GetExtensionHitbox with the owning Player before
its generic-hitbox fallback (Projectile.cs:13955-13977). The helper only creates an extension hitbox
for projectile types 105, 46, and 153, using the Player's item-animation window and melee speed
plus projectile center and velocity (Projectile.cs:33755-33791). Its reversed, clamped Utils.Remap
uses GetLerpValue's (value - from) / (to - from) interpolation for the descending animation
interval (Utils.cs:249-287). `ProjectileAi19ExtensionQuery` now follows GetLerpValue's endpoint
checks before interpolation. With equal endpoints, Version4 returns 0 or 1 for values outside the
shared endpoint and preserves NaN at exact equality; clamping only the quotient chooses a different
bound for the outside cases.
The collision branch tests centered rectangles along the segment from
the projectile center to the extension center, with a step at least 12 pixels, then tests the
extension rectangle itself.

ProjectileAi19ExtensionQuery computes that helper result from ProjectileEntityState and an explicit
ProjectileAi19OwnerSnapshot containing only item animation, maximum animation, and melee speed. It
preserves the helper's type gate, animation threshold, reach/width parameters, velocity rotation,
and Utils.CenteredRectangle truncation. Only types 46, 105, and 153 can produce an extension
rectangle. For those types, a missing owner snapshot is Unsupported when the supplied projectile
hitbox misses; a hitbox overlap is already a decisive collision regardless of the extension result.
Other AI style 19 projectile types always fail the helper's type switch, so their collision path
does not require owner animation input and continues through the generic hitbox or later type-specific
geometry. `ProjectilePlayerCollisionInputProjection` now combines explicit Player item-animation
input and attack-speed state into the Projectile owner snapshot, offers a composed spear-extension
query caller, and projects mounted center from the Player spatial snapshot. It does not read a live
Player store; authoritative owner lookup and runtime caller remain absent. The AI style 203 branch
remains unmapped because Version4 StormLightningCollisionCheck is stubbed. No build, test, or verifier
was run at the user's request; verificationStatus remains not-run.

### 2026-10-04 generic hitbox fallback for unlisted projectile types

A scan of Version4 `Projectile.Colliding` from its first AI-style dispatch through the outer
`myRect.Intersects(targetRect)` fallback (`Projectile.cs:13658-14179`) found explicit type branches
for the mapped geometry cases and a generic hitbox result for otherwise-unlisted valid types. The
Query's `switch` default now returns that supplied damage-hitbox intersection for ordinary styles.
AI style 137 uses its visibility snapshot at this fallback. AI style 19 needs owner input only
for spear types 46, 105, and 153 when the generic hitbox misses. Earlier type-specific branches
remain ahead of this fallback, and mapped late branches retain their source precedence. AI style 203 remains
`Unsupported` because its Version4 collision helper is stubbed.

This closes the local boolean fallback only. The collision Query still has no production caller;
production world-to-`SpatialCollisionSnapshot` capture remains absent. The AI style 19 Player input projection and composed extension Query now exist in Application, but runtime owner lookup
is still absent, and the stubbed style 203 behavior remains `unknown`. No build, test, or
verifier was run at the user's request;
`verificationStatus` remains `not-run`.

### 2026-10-04 collision fallback precedence

Static review found two ordering details in the Version4 Projectile.Colliding source. Type 871
checks pellet-storm hitboxes before the AI style 137/19/203 branches and returns false when no
storm bullet hits (Projectile.cs:13869-13877); the shared projectile hitbox fallback is not
reached for that type. Separately, for branches that do fall through, the shared projectile hitbox
check precedes the late type-specific geometry branches (Projectile.cs:14007-14011). The query
preserves the type 871 short circuit, skips that branch for AI style 15, and preserves hitbox
precedence for mapped late geometry. Static re-reading also showed that the AI style 19 owner helper
can produce a hitbox only for types 46, 105, and 153 (Projectile.cs:33755-33791); other types
continue without owner-dependent geometry. The Query now requires that owner snapshot only for those
three spear types. This remains a static source mapping; no build, test, or verifier was run, and
verificationStatus remains not-run.

### 2026-10-04 type 973 radial range correction

Version4 type 973 calls `SafeNormalize` but discards its returned vector, then compares the original
closest-point offset length against `100 * scale` (`Projectile.cs:13669-13678`). `SafeNormalize`
returns a value and does not mutate the input vector (`Utils.cs:1172-1181`). The geometry Query now
uses that raw offset for the strict range check before applying the explicit LOS result; the shared
projectile hitbox fallback still applies when the radial test misses. This is a static source mapping
only; no build, test, or verifier was run and `verificationStatus` remains `not-run`.

### 2026-10-04 SafeNormalize zero and NaN fallback

Version4 Utils.SafeNormalize returns its supplied fallback when a vector is zero or contains NaN
(Utils.cs:1172-1181). ProjectileCollidingGeometryQuery now uses the same rule for the mapped
velocity-derived geometry of types 611, 684, 756, 961, 1041, and 927, with each branch's
Version4 fallback vector. This is a static source comparison only. No build, test, or verifier was
run at the user's request; verificationStatus remains not-run.

### 2026-10-04 pass-scoped DD2 event snapshot

The coordinator now requests `IProjectileTickAdapter.TryGetDefenderEventOngoing` once after
`PreUpdateAllProjectiles`, then reuses that immutable result for each completed substep of every
`IsADD2Turret` projectile in the pass. Version4 `Main.Update` calls `PreUpdateAllProjectiles`
immediately before scanning projectile slots (`Main.cs:11530-11531`); that hook only updates
Spelunker and Chum Bucket helpers (`Main.cs:11386-11392`). Each projectile substep checks
`ProjectileID.Sets.IsADD2Turret[type] && DD2Event.Ongoing`, increments `timeLeft` when true, and
then decrements it (`Projectile.cs:15228-15232`). The event snapshot is optional for passes without
these projectile types, but a missing snapshot fails when one reaches the lifetime tail. The
production adapter that captures `Dd2RunStateComponent.Ongoing` and the authoritative Main caller
remain absent. This is a static source comparison only; no build, test, or verifier was run, and
`verificationStatus` remains `not-run`.

The pass boundary is consistent with the inspected Version4 update order: NPC slots run before
`PreUpdateAllProjectiles` (`Main.cs:11511-11530`), while `UpdateTime` runs after projectile and
item updates (`Main.cs:11575-11586`). The `Ongoing` assignments are confined to
`ResetProgressEntirely`, `StartInvasion`, and `StopInvasion` in `DD2Event.cs`; reviewed runtime
callers start/stop the event in NPC update, update-time, or message handling paths, not inside the
projectile slot loop. This supports a pass-scoped value for the inspected source flow, but does not
replace production caller or execution evidence.

### 2026-10-04 world-descriptor bounds at the Projectile phase

`ProjectileSimulationTickPhase` now reads the committed `WorldDescriptorSnapshotValue.Bounds`
from the current `WorldSimulationTickContext` on each execution and projects its four edges into
`ProjectileWorldBounds`. The values remain in pixels: Version4 `WorldGen.setWorldSize` assigns
`rightWorld` and `bottomWorld` from tile dimensions multiplied by 16 (`WorldGen.cs:6225-6226`),
and `Projectile.Update` compares projectile pixel positions directly with those edges
(`Projectile.cs:14711-14712`). The world-header load API preserves the four saved edge values in
the descriptor. The validated Projectile value rejects non-finite or collapsed bounds.

This removes the phase's constructor-frozen bounds input. Tools.Simulation registers an
`ActiveProjectileTickPhase` wrapper over the Application phase, which creates a narrow type-1 / AI
style 1 runtime adapter per committed tick. Neither path is the authoritative Version4 Main caller.
No build, test, or verifier was run at the user's request; `verificationStatus` remains `not-run`.

## 静态证据与交付边界

| 项目 | 状态 |
| --- | --- |
| 设计与执行文档 | `proposed`；只记录 System 边界、执行顺序、验收门槛和剩余缺口 |
| CPG API 只读查询 | 2026-10-01 通过 `CpgEvidence.ps1` 查询 `NeedsUUID`（`Terraria.ID/ProjectileID.cs`，symbol=`complete`）、选定 3 个路径的 member uses（`complete` / 1 条）与标量 `NewProjectile` overload call-sites（选定 `Projectile.cs`/`NetMessage.cs`/`MessageBuffer.cs` 路径下 `complete` / 78 条，来源路径为 `Projectile.cs`）；callable facts=`partial` / `CalleeEffectsNotExpanded`。数据库 manifest=`6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`、`SourceSnapshotId=null`；这些状态不证明闭合调用或数据初始化行为 |
| Version4 目标源码 | `D:\TRbackup\Version4` 是目标语义来源；源码关系不足时以目标声明、实现和调用点回查，并保留 `partial`/`unknown`/`evidence-gap` |
| 完整参考源码 | 只读检查 `D:\TRbackup\无任何删减通过编译` 的 `TerrariaServer.sln` / `.csproj`、`Projectile.cs`、`ProjectileID.cs`、`NetMessage.cs`、`Main.cs`、`MessageBuffer.cs`；关键文件与 Version4 hashes 不同，提取的 `SetDefaults` 方法文本 hash 相同；本交付不在参考项目构建或测试 |
| 本轮文档与实现补证 | 2026-10-01 的只读 CPG/源码对照和历史定向 build/lifecycle/hydration/tick coordinator focused verifier 仅代表当时状态；2026-10-03 新增 packet 27/29 payload codec，2026-10-04 增加 packet DTO 双向映射、Gateway registration adapter、Application command-owner port、trusted owner projection、soundDelay/reflected 状态归属、两项专用规则常量、六项派生属性映射、一项 owner-target slot 投影及 Damage_CanDealDamage/Damage_GetHitbox/NPC/PVP local mappings、trajectory/numUpdates 状态、动态子步/逐子步寿命调度、pre-AI soundDelay 边界、preparation/substep outcome 契约、默认 type-640 倒计时、显式快照下非 minion world-boundary deactivation、`gfxOffY` motion 规则、pre-AI inherited `oldVelocity` 到共享 Entity motion-history 的投影、Ready 子步的瞬时 `netUpdate` reset/`netUpdate2` 保留，以及 54 个 projectile type 的 selected `Colliding` geometry（含 type 871 pellet-storm）、AI style 15 mounted-center geometry、AI style 190 dual-cone geometry、whip control-point geometry/18-type IsAWhip mapping、AI style 137 visibility-snapshot、基于显式 owner 输入快照计算的 AI style 19 spear-extension geometry 与 Utils.Remap endpoint correction、未列明类型 generic hitbox fallback、NPC/PVP damage-collision composition API，以及基于 SpatialCollisionSnapshot 的 AI style 137 Tile 可见性计算（读取 IsInactive、按 Version4 CanHitLine 路径遍历；LegacySpatialTileSnapshotAdapter 已提供区域投影但生产 Query caller 仍缺）、以及 142 项 Version4 TrailingMode type 映射、hydration Presentation 投影和 mode 0–5 trail history 更新；type 661 与 `CanHit`/`CanHitLine` 已有基于瓦片 snapshot 的计算；运行时区域捕获/组合调用仍缺，AI style 137 的运行时区域捕获/组合调用，以及 AI style 19 Application owner-input projection/组合扩展 Query 已实现，但运行时 Player owner lookup/caller 仍缺；coordinator 已调用 trail Record，但 history-backed branches 暂无权威 tick caller 和 mode 4 Player 快照；默认世界边界值会被 coordinator/context 拒绝；本轮将 Tools.Simulation host bounds 改为当前 committed snapshot；此前只检查 type == 1 专属 AI 分支而移除重力/rotation/ai[0] 写入的判断已由 AI_001 通用分支复核更正，host 现于湿态查询和移动前映射 ai[0] 递增、15 tick 后每步 +0.1f 重力、通用 rotation 和 velocity.Y 上限 16，复用共享 Entity DirectionComponent，移动后按 velocity.X 更新且用于 NPC 命中方向，并在 hydration 中映射 Type 1 spawn 速度限制；gfxOffY 仍由 preparation 推进，且不改 spriteDirection，对齐 10×10、timeLeft=1200、Ranged/IsArrow defaults，并为 support manifest 加入 type-1 defaults 约束；受限 host 新增当前 wet/honey/shimmer 接触运动映射与 type-1 碰撞后位置提交；Tools host wrapper 已接入 Application ProjectileSimulationTickPhase，由 factory 按 committed tick 创建有限 adapter，但未接入 Version4 Main。按用户要求未运行 build/test/verifier；历史 focused verifier 未验证当前任何改动，不把它标记为已验证或视作 P15 行为等价证据 |
| 设计输入 | 17 个叶子组、126 个成员、原 `sessionId=4426a5ad1b824c2490a832c3bb6726b8` 和已结算输入报告；不创建新的 claim |
| 源码/项目变更 | `true`；包含 `src/NSSLC.Infrastructure/Network` 的 packet payload codec、双向 packet DTO 映射和对 `Terraria.Projectile` 项目的引用；另有 lifecycle、ordered tick coordinator、tick preparation、motion System、world-boundary snapshot、ProjectileAi137VisibilityQuery、SpatialTileSnapshot.IsInactive、Projectile 对 SpatialSimulation 的项目引用与 verifier 入口改动；未改 Version4、完整参考项目或 runner 状态 |
| 局部 build / runtime | `partial`；2026-10-01 前序执行已通过 `Build/Tools/Invoke-SerialDotnet.ps1` 定向构建 Projectile focused verifier，exit code `0`、`0 warnings`、`0 errors`，产物位于 `Build/bin/Terraria.ProjectileCombatVerification/Debug/net10.0/`；`--lifecycle`、`--hydration`、`--tick-coordinator` 当时均输出 `PASS`。这些历史结果不覆盖 2026-10-03 的 codec/project-reference 修改或 2026-10-04 的 DTO mapper、Gateway registration adapter/Application port、trusted owner projection、state/query mapping、动态 `numUpdates`/逐子步寿命调度、type-640/world-boundary/GfxOffY preparation、pre-AI cooldown 边界、preparation/substep outcome 契约、OwnerMinionAttackTargetNPC typed-slot 投影、`gfxOffY`/`oldVelocity` preparation、`netUpdate`/`netUpdate2` substep reset、default world-boundary rejection、54-type selected collision mapping、type 871 pellet-storm geometry、AI style 190 cone mapping、AI style 137 tile visibility calculation、AI style 19 remap endpoint correction 与 Projectile/SpatialSimulation reference change（已补运行时区域 tile capture adapter；生产 caller 仍未实现）；type 661 的 LOS/range 已能从显式瓦片 snapshot 计算，但 Main.tile 区域捕获和权威组合调用缺失；AI style 15 的 owner-mounted-center 仍无生产快照来源，trail cache 生产记录仍缺失。2026-10-04 Type 1 液体运动与显式 `ProjectileWetCollisionQuery` 仅做源码静态映射，未运行 build/test/verifier。结果不覆盖 solution 全量、`--network`、Gateway owner path、其它 AI 分支 outcome 映射、world-boundary snapshot/deactivation、GfxOffY motion、minion/sentry Player handoff、NPC 实体解析、完整 `Colliding` 分派/几何、真实 Main 接线或 P15 行为等价 |
| 迁移/行为等价结论 | 未作出；P15 保持 `proposed`，不能称迁移成功 |

### 2026-10-04 Tools.Simulation Projectile 接线与 type 1 修正

静态核对发现 `NSSLC.Tools.Simulation.Program` 已将 `ActiveProjectileTickPhase` 注册进 `WorldSimulationKernel` 的 Player/NPC/Projectile 顺序。该 wrapper 转发给 Application 的 `ProjectileSimulationTickPhase`；phase 使用当前已提交 snapshot 的边界，并从 `RuntimeProjectileStore` 获取本 tick 的 adapter。这个 runtime adapter 仍只覆盖脚本弓箭路径中的 type 1、AI style 1；它是受限 host caller，不是 Version4 `Main` 的权威调度路径。

`RuntimeProjectileStore.Update` 现在接收当前 `WorldSimulationTickContext`，要求 snapshot 已提交，并从该 snapshot 的 descriptor 读取像素边界。Version4 `SetDefaults` 将 type 1 配置为 10×10、AI style 1、`timeLeft = 1200`，Simulation content 中的对应几何和寿命已对齐这些字段。之前的静态核对只检查了 `AI_001` 中是否存在 `type == 1` 专属分支，遗漏了 Type 1 实际经过的通用分支；本轮按 `Projectile.cs:36128-36130, 37640-37650, 37654-37840` 更正：每次 AI 调用递增 `ai[0]`，达到 15 后封顶并每步增加 `velocity.Y` 0.1，按更新后的速度设置通用 rotation，再把 `velocity.Y` 上限限制为 16。受限 adapter 已在湿态查询和移动前调用 `AdvanceOrdinaryArrowAi`。`ProjectileTickPreparationSystem` 仍在 adapter 前推进 `gfxOffY`，所以 `CommitOrdinaryArrowStep` 不重复推进；Type 1 该 AI 路径不改 `spriteDirection`。

对 type 1 defaults 的字段复查又发现 Simulation content 的简化构造器会把 `Ranged` 和 `IsArrow` 留为 `false`，而 Version4 分支明确置 `ranged = true`、`arrow = true`。catalog 现显式投影这两个字段；`SimulationContentSupportManifest` 同时约束 type 1 的有效几何、移动策略、寿命、AI cadence、penetration/immunity、trail/presentation、network flag、combat flags 与 projectile capabilities。该 manifest 约束尚未运行验证。

此项只修复有限 type-1 host 路径中可由源码确认的状态输入/写入。host 按 Type 1 dry movement 路径执行分段 tile sweep 和 `SlopeCollision`；现又按当前液体接触映射 water、honey、shimmer 的湿态 `TileCollision` 与 `wetVelocity` 比例，并在碰撞终止前提交解析后的位置/速度。碰撞路径按 Version4 type 1 默认分支先推进一次位置，再执行 `UpdatePosition` 的速度推进（湿态采用 `wetVelocity`）。湿态推进现在提交 `wetCount` 并返回进入/离开与 splash 请求资格；headless host 没有执行该请求的效果 owner。Version4 的 `Shimmer()` 是空方法，不产生额外行为。host 仍没有 `Kill` 效果 owner，未映射 `Collision.SwitchTiles` 或 tile-cut；NPC damage host 仍使用简化的 owner/目标快照和命中提交。其他 AI、完整 projectile type defaults、effect/owner side effects、network owner commit，以及 Version4 Main 调度集成仍未闭合。

按用户要求只做源码静态核对，未运行 build、test 或 verifier；`verificationStatus` 继续为 `not-run`，P15 仍是 `partial / proposed`。

### 2026-10-04 type 1 trail-cache defaults

Version4 `Terraria.ID/ProjectileID.cs:291-293` initializes `TrailingMode` with default `-1` and `TrailCacheLength` with default `10`; type 1 has no override in either set. `Projectile.SetDefaults` reads `TrailCacheLength[Type]` and resizes/clears its position, rotation, and sprite-direction histories (`Projectile.cs:467-482`). The type 1 `ProjectilePresentationDefinition` already defaults its trail cache capacity to 10, and hydration obtains trailing mode from `ProjectileTrailCacheSystem.GetTrailingMode(1)`, whose unlisted-type result is `-1`.

`SimulationContentSupportManifest` now pins both values for the only supported host projectile type. This closes catalog drift for those two source-backed defaults; it does not implement trail rendering or make the limited Tools.Simulation caller authoritative. No build, test, or verifier was run; `verificationStatus` remains `not-run`, and P15 remains `partial / proposed`.

### 2026-10-04 ordinary-arrow platform collision flags

Version4 `Projectile.HandleMovement` initializes the platform fall-through flag to `true`, leaves it unchanged for type 1 (`Main.projPet[1]` is not set in the projectile-pet table), then passes that flag twice to `Collision.TileCollision` in the ordinary dry path (`Projectile.cs:15519-15530, 15890-15900`). The tile query uses these two arguments together when deciding whether a solid-top tile blocks downward motion (`Collision.cs:1727`). The simulation host previously omitted both arguments and therefore used the helper defaults `false`/`false`; it now passes `fallThrough: true, fall2: true` to match Version4 for the supported arrow.

This aligns the platform pass-through decision. The dry Type 1 slope and high-speed step path is recorded below; the host still uses the legacy world-generation collision helper and lacks collision side effects and the Kill lifecycle contract. No build, test, or verifier was run; `verificationStatus` remains `not-run`, and P15 remains `partial / proposed`.

### 2026-10-04 ordinary-arrow dry slope movement

For a dry Type 1 projectile, Version4 takes the ordinary `HandleMovement` branch: it clamps the collision step size to 3–16 pixels, subdivides movement above that distance into at most 300 calls, applies `SlopeCollision` after each step and once at the end, then commits the adjusted position/velocity before the collision response (`Projectile.cs:15748-15900`). Type 1's `GetCollisionParams` leaves the 10×10 hitbox unchanged (`Projectile.cs:17825-17905`). The host adapter now follows that dry path through its existing WorldGeneration `TileCollision` and `SlopeCollision` helpers; static inspection confirms those helpers use the same argument and result shapes as Version4.

The dry Type 1 tile sweep and slope adjustment follow Version4's ordinary movement path; the liquid movement mapping is recorded in the following section. The host commits the adjusted position before termination, including the ordinary Type 1 collision response's position advance. `Collision.SwitchTiles`, tile-cut effects, and the `Kill` effect owner remain absent. No build, test, or verifier was run; `verificationStatus` remains `not-run`, and P15 remains `partial / proposed`.

### 2026-10-04 ordinary-arrow liquid movement and impact ordering

Version4 `Projectile.Update` refreshes `wet` with `Collision.WetCollision` when `ignoreWater` is false, then calls `HandleMovement` (`Projectile.cs:14827, 15045`). For wet movement, `HandleMovement` calls `TileCollision` once and sets `wetVelocity` to `velocity * 0.5` in water, `* 0.25` in honey, or `* 0.375` in shimmer, except an axis changed by collision keeps its full resolved velocity (`Projectile.cs:15664-15692`). The slope adjustment still applies to the ordinary projectile hitbox. Dry movement retains the 3–16 pixel subdivision, 300-step cap, and per-step/final slope calls (`Projectile.cs:15748-15900`).

The Type 1 collision fallback advances `position` by the resolved velocity and calls `Kill`; `Projectile.Update` then reaches `UpdatePosition(wetVelocity)`, which applies the second movement using `wetVelocity` while wet (`Projectile.cs:17808-17816, 17995-18020`). The initial host mapping captured contact through legacy `Collision.WetCollision` and read its `honey`/`shimmer` flags; the later explicit-query entry below replaces that shared-flag path. The later wet-state entry maps `wetCount` and returns splash-request eligibility, but the headless caller discards that result and no splash dust/sounds execute. Version4 `Shimmer()` is empty (`Projectile.cs:18676`), so it contributes no behavior to implement. `Collision.SwitchTiles`, tile cutting, and Type 1 `Kill` sound/dust effects remain unimplemented. Static source review only; no build, test, or verifier was run, `verificationStatus` remains `not-run`, and P15 remains `partial / proposed`.

### 2026-10-04 Type 1 support-profile drift guard

`ProjectileDefinitionHydrationSystem` scales definition width and height before storing the hitbox. The manifest previously checked raw dimensions `10×10`, so a `Scale` change could pass while producing a non-Version4 hitbox. `SimulationContentSupportManifest` now pins `Scale == 1`, owner-hit distance, AI/update cadence, manual fall-through defaults, network importance, presentation defaults, immunity/penetration settings, combat classification, and projectile capabilities for the supported Type 1 profile. Version4 sets the base fields in `Projectile.SetDefaults` (`Projectile.cs:451-550`); its projectile frame table defaults each type to one frame (`Main.cs:5219`).

This guard rejects catalog drift that the limited host does not model; it does not add the omitted state or make the caller authoritative. The source/content comparison was static. No build, test, or verifier was run; `verificationStatus` remains `not-run`, and P15 remains `partial / proposed`.

### 2026-10-04 Type 1 explicit liquid query and persistent wet state

`Collision.LavaCollision` and `Collision.WetCollision` scan x-major/y-minor tiles with the same world-edge clamps, but use different rectangles: lava checks the full projectile hitbox (`Collision.cs:1085-1119`), while wet contact checks a centered probe and includes the sloped-tile puddle fallback (`Collision.cs:1001-1084`). `ProjectileWetCollisionQuery` now returns both predicates from captured tile facts, keeps the first wet-contact liquid classification, and does not use shared `Collision.honey` or `Collision.shimmer` flags. `LegacySpatialTileSnapshotAdapter` captures the requested region plus the row needed to inspect the tile above a slope; the caller uses dimensions from the committed `WorldSimulationTickContext` snapshot and fails if required tile facts are incomplete.

`ProjectileWetStateComponent` and `ProjectileWetStateSystem` now own the per-projectile `wet`, `lavaWet`, `honeyWet`, `shimmerWet`, and `wetCount` state. The update latches liquid-kind flags while wet, clears them on exit, starts the 10-tick exit cooldown for Type 1 when its counter is zero, and decrements the counter in the same update, matching the inspected ordering in `Projectile.Update` (`Projectile.cs:14871-15037`). It now returns a typed transition (enter/leave and the selected liquid) plus whether Version4 permits a splash request. The Type 155 exception is preserved: it suppresses splash requests and does not start the exit cooldown (`Projectile.cs:14873, 14950-14956`). With zero `wetCount`, Type 1 lava entry requests a splash, while lava exit starts the cooldown but suppresses the splash (`Projectile.cs:14873-14900, 14954-15020`). The current `RuntimeProjectileStore` caller discards this transition result, so no splash dust or sound is executed. This is an explicit state-to-effect handoff contract, not completed effect behavior; the available WorldGeneration `Dust.NewDust` returns reserved slot `6000` and `SoundEngine.PlaySound` is a default-returning stub (`LegacyApi.cs:118-124, 337-339`), with no particle/audio owner in Tools.Simulation. Version4 `Projectile.Shimmer()` is empty (`Projectile.cs:18676`), so its callback adds no target behavior; Type 1 still uses shimmer liquid for wet movement scaling. `Collision.SwitchTiles`, tile cutting, and `Kill` effects also remain unimplemented; `TileCollision` and `SlopeCollision` still use the legacy runtime tile map. Static source comparison only; at the user's request no build, test, or verifier was run, `verificationStatus` remains `not-run`, and P15 remains `partial / proposed`.

### 2026-10-04 ordinary-arrow `AI_001` generic fallback

Version4 `AI()` dispatches AI style 1 to `AI_001()` (`Projectile.cs:18706-18713`). Type 1 is absent from the `flag3 = false` type list, so that method increments `ai[0]` on each AI call (`Projectile.cs:36035-36130`). Type 1 also falls through the specialized gravity branches: when `ai[0] >= 15`, the common branch clamps it to 15 and adds 0.1 to `velocity.Y` per AI call (`Projectile.cs:36963-37652`). The generic rotation path assigns `atan2(velocity.Y, velocity.X) + 1.57f`, then the common tail clamps `velocity.Y` to a maximum of 16 (`Projectile.cs:37654-37840`).

`ProjectileMotionAndAiSystem.AdvanceOrdinaryArrowAi` now maps this bounded Type 1 AI slice. `RuntimeProjectileStore` calls it before liquid contact and tile movement, matching Version4's AI-before-`HandleMovement` order; it leaves `spriteDirection` unchanged. This does not close all AI style 1 behavior: owner-driven `extraUpdates`, wind, AI effects, Type 1 Kill/effect side effects, and the authoritative Version4 Main schedule remain unmapped. Static source review only; no build, test, or verifier was run, `verificationStatus` remains `not-run`, and P15 remains `partial / proposed`.
### 2026-10-05 ordinary-arrow spawn/motion and owner cadence

Version4 `Entity.direction` defaults to 1 (`Entity.cs:20`). After `HandleMovement`, `Projectile.Update` updates it to -1 when `velocity.X < 0`, otherwise 1; `AutomaticallyChangesDirection()` allows this for AI style 1 (`Projectile.cs:15045-15056, 15316-15325`). `Damage_PVE_Inner` uses `direction` as the ordinary NPC strike direction (`Projectile.cs:12475-12511`). This state is distinct from `spriteDirection`.

The projectile aggregate reuses the shared `DirectionComponent.Horizontal` state instead of duplicating `spriteDirection` or adding a projectile-only direction type. The ordinary-arrow movement commit applies the post-move update, and `RuntimeProjectileStore` passes `Direction.Horizontal` to the collision query and NPC hit owner instead of using `Trajectory.SpriteDirection`. It initializes to Version4's Entity default of 1.
Version4 `NewProjectile` normalizes AI style 1 spawn velocity after source-stat application: while X is `>= 16` or `<= -16`, or Y is `>= 16` or `< -16`, it multiplies both components by `0.97f` (`Projectile.cs:10290-10300`). Hydration now applies the same condition and scaling to the supported Type 1 profile before storing kinematics. This maps the command velocity normalization; the host still lacks the Version4 owner/source-stat effects that precede it. No build, test, or verifier was run.

One adjacent Type 1 owner path remains open: after movement, Version4 raises arrow `extraUpdates` to 1 when the owner has `magicQuiver` (`Projectile.cs:15062-15065`). `RuntimePlayerStore` has no MagicQuiver input or projection, and the existing `PlayerRangedAccessoryCapabilityComponent` is not connected to this host. This turn does not infer that owner state. Static source inspection only; at the user's request no build, test, or verifier was run. `verificationStatus` remains `not-run`; P15 remains `partial / proposed`.

### 2026-10-05 static NPC immunity host wiring

The simulation composition now creates one `ProjectileStaticNpcImmunityRegistryComponent` sized from the content catalog's maximum projectile type and the runtime NPC slot capacity, then shares it with `RuntimeNpcStore` and `RuntimeProjectileStore`. The projectile collision adapter uses the registry-backed `ProjectileDamageCollisionQuery.EvaluateNpc` overload with the current simulation tick projected to Version4's `uint` update count. After `NpcCombatSystem` accepts a hit on a surviving NPC, the adapter calls `ProjectileStaticNpcImmunitySystem.RecordAcceptedNpcHit` before consuming penetration, preserving the source's single-penetration exception while the original remaining-hit value is available (`Projectile.cs:12696-12701`). When the NPC owner commits an immediate death transition, it releases the slot and clears its immunity data before control returns; the adapter does not recreate a cooldown row for that now-empty slot.

`RuntimeNpcStore.TryRelease` first validates the NPC slot generation and identity, then calls `ResetNpcSlotData` against the same session projectile store before releasing the slot. This clears that slot's static expiry row and every occupied projectile's local-immunity cell, preventing a reused slot from inheriting either immunity. `RuntimeNpcStore.Reset` clears all static rows and drops the old session store reference. This host cleanup occurs when a slot is released, before it can be allocated again; the authoritative source performs the reset as part of `NPC.NewNPC` slot reuse (`NPC.cs:8127-8129`).

The runtime adapter still rejects every projectile except Type 1 with AI style 1, and the Type 1 support manifest explicitly requires static NPC immunity to be disabled. The production call sites now exist, but no currently supported host projectile exercises the registry-backed branch. No build, test, or verifier was run at the user's request; `verificationStatus` remains `not-run`, and P15 remains `partial / proposed`.

### 2026-10-05 local NPC immunity accepted-hit wiring

Version4's ordinary PVE hit path writes `localNPCImmunity[victimIndex] = localNPCHitCooldown` only when `usesLocalNPCImmunity` is true and the cooldown is not `-2` (`Projectile.cs:12844-12848`). The existing `ProjectileHitImmunityPolicyComponent.WritesLocalNpcImmunity` encodes that predicate. `ProjectileHitImmunitySystem.RecordAcceptedNpcHit` now applies it through `SetLocalNpcImmunity`; this retains `-1` as permanent immunity and lets `AdvanceTick` decrement positive values once per regular update (`Projectile.cs:15306-15313`). `RuntimeProjectileStore` calls this writer for an accepted hit on a surviving NPC, before penetration is consumed. Immediate NPC death releases the slot and clears its local immunity cells first, so the adapter skips a write to the empty slot.

This maps the ordinary cooldown policy branch only. Version4's special hit branches for types 864, 866, 611 and 612 carry additional owner-immunity, penetration, AI and targeting effects which remain unmapped. The finite simulation host still accepts only Type 1/AI style 1, and its support manifest disables local NPC immunity with cooldown `-2`; therefore the new writer is connected but not exercised by the current host profile. No build, test, or verifier was run at the user's request; `verificationStatus` remains `not-run`, and P15 remains `partial / proposed`.

### 2026-10-05 Type 1 Magic Quiver owner cadence

Version4 Projectile.Update raises extraUpdates to 1 when the projectile is friendly, is an arrow, is not an NPC projectile, its owner has magicQuiver, and extraUpdates is below 1 (Projectile.cs:15062-15065). The regular update initializes numUpdates from extraUpdates before entering its loop (Projectile.cs:14702-14703), so this promotion affects the next regular projectile update.

The simulation input script now accepts an optional top-level players array with a magicQuiver setting per player slot. RuntimePlayerStore hydrates that value into the existing PlayerRangedAccessoryCapabilityComponent; the projectile adapter reads it by the projectile owner slot. ProjectileMotionAndAiSystem owns the cadence write and retains the source predicates. The finite host calls this rule after movement and before the accepted-hit damage path; its supported Type 1 profile satisfies the arrow, friendly, and non-NPC conditions. The coordinator observes the promoted cadence on the next tick and schedules two substeps.

Static source review only. No build, test, or verifier was run at the user's request. verificationStatus remains not-run; P15 remains partial / proposed.

### 2026-10-05 trail mode 4 Player movement snapshot producer

Version4 records trail history after behavior for each completed projectile substep. Trailing mode 4 captures Main.player[owner].position - oldPosition, shifts position/rotation/sprite-direction history, and applies the captured delta to cached nonzero positions only when numUpdates == 0 (Projectile.cs:15199-15214). The coordinator requests this input only for mode 4 and performs trail recording after each completed substep.

RuntimePlayerStore now captures each living player's position before movement and stores the resolved post-collision displacement for the current simulation tick. It records zero movement for dead players and exposes the snapshot only when the requested tick matches. RuntimeProjectileTickAdapter implements TryGetOwnerMovementDelta using the projectile owner slot and its tick-bound player snapshot. This keeps Player movement as an input and leaves trail-history mutation in ProjectileTrailCacheSystem.

The finite host still supports only Type 1 / AI style 1, whose trailing mode is -1, so no supported host projectile exercises mode 4. The source-level Player delta producer and adapter edge are connected; mode-4 projectile behavior and the authoritative Version4 Main caller remain unverified and outside the host profile. Static source review only. No build, test, or verifier was run at the user's request; verificationStatus remains not-run and P15 remains partial / proposed.

### 2026-10-05 mode 1 trail Dust request handoff

Version4 updates mode 1 trail history only when `frameCounter == 0` or `oldPos[0]` is zero. After that history shift, types 466 and 580 with zero velocity create Dust type 229 at the last cached position, using randomized velocity derived from projectile rotation; the resulting Dust has `noGravity = true` and scale 1.7 (`Projectile.cs:15130-15153`). Both projectile types have trail cache length 20 (`ProjectileID.cs:293`).

`ProjectileTrailCacheSystem.Record` now returns a typed request for that exact type/velocity/gate branch. `ProjectileTickCoordinator` supplies the hydrated projectile type and forwards the request synchronously after committing trail history, preserving its location before the time-left update. The tick adapter is the effect boundary; its default rejects the request when no particle owner is connected, so the effect cannot be silently discarded.

The random draws and `Dust.NewDust` mutation still require a concrete presentation adapter. The only current Tools.Simulation projectile profile is type 1, so it cannot exercise this request. This closes the source-backed trigger and handoff only; mode 1 Dust execution and the authoritative Version4 Main caller remain open. Static source review only. No build, test, or verifier was run at the user's request; `verificationStatus` remains `not-run`, and P15 remains `partial / proposed`.
