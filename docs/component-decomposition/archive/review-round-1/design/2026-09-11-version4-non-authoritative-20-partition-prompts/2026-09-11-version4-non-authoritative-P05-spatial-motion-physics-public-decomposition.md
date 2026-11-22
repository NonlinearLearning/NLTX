# Version4 非权威组件拆分分区 P05：空间移动与物理 - public-decomposition 专属提示词

> 本文件只包含当前分区的专属任务内容，不复制公共提示词正文。
> 使用时：先提供公共提示词，再附加本文件；公共协议中的通用规则、证据门槛、报告结构、禁止项和 Integration Handoff 继续有效。

## 公共前置（仅引用）

- 公共提示词：`D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\design\2026-09-05-version4-public-decomposition-common-protocol.md`
- 必读技能：`D:\TRbackup\NLTX\.agents\skills\public-decomposition\SKILL.md`
- ECS 证据协议：`D:\TRbackup\NLTX\.agents\skills\public-decomposition\references\ecs-evidence-protocol.md`
- tModLoader 检索协议：`D:\TRbackup\NLTX\.agents\skills\public-decomposition\references\tmodloader-documentation-retrieval.md`
- 本文件不重复上述文件内容；执行前必须实际读取并遵守它们。
- 并行会话保障 skill：D:\\TRbackup\\NLTX\\.agents\\skills\\version4-non-authoritative-partition-session\\SKILL.md
- 并行领取/结算 runner：D:\\TRbackup\\NLTX\\Build\\Tools\\Invoke-Version4NonAuthoritativePartitionSession.ps1
- 必须先按 skill 使用 ClaimNext（或用户明确指定时使用 Claim -PartitionId P05），只写本文件的 outputReport，并用领取返回的同一 sessionId 完成 Complete/Fail/Abandon；不得手工编辑 ledger 或 lock。
- 本任务属于 20 个并行会话中的固定分区；currentTaskCount: 20。公共协议中遗留的 19 会话数量说明在本任务包中不适用。

## 任务元数据

- promptId: P05-non-authoritative-public-decomposition-20260911
- partitionId: P05
- inputPartitionReport: D:\TRbackup\NLTX\docs\migration\ledgers\non-authoritative-component-partitions\05-spatial-motion-physics.md
- outputReport: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P05-spatial-motion-physics-public-decomposition.md
- reportType: public-decomposition
- executionMode: independent-read-only-partition
- currentTaskCount: 20
- formalParentSubsystems: RuntimeComposition, SharedRuntimeMechanisms
- leafSubsystemCount: 10
- fieldCount: 99
- propertyCount: 20
- memberCount: 119
- sourceReportSha256: b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196
- sourceInventoryStatus: source-inventory-confirmed; member-level owner/lifecycle/side-effect evidence still requires this session to close

## 专属范围锁定

- 本会话只审查输入分区报告中的 10 个叶子子系统和 119 条成员记录。以下清单是本会话唯一的成员范围；不得读取或复制其他 P 分区的成员清单。
- 细分子系统仍只是库存边界，不等价于一个组件；允许在本分区内部按访问模式拆成多个 proposed Component、System、Query、Command、Adapter 或 Projection。
- 来源序号、路径、行号和声明是检索线索；必须回到 Version4 重新定位，不得把输入报告直接当作已确认的读者、写者、生命周期或 owner 证据。

| 叶子子系统 | 父级 | 原章节 | 边界角色 | 字段 | 属性 | 成员 | 细分职责 |
|---|---|---|---|---:|---:|---:|---|
| `MainSpawnAndProjectileCaches` | `RuntimeComposition` | `4.1.19` | `catalog reference` | 7 | 0 | 7 | 刷怪检查和投射物帧/宠物缓存。 |
| `SharedTeleportAndPortalSupport` | `SharedRuntimeMechanisms` | `4.9.5` | `query/adapter` | 12 | 1 | 13 | 传送门、水晶塔和脱困支持。 |
| `SharedPhysicsCollisionQueries` | `SharedRuntimeMechanisms` | `4.9.35` | `query` | 11 | 0 | 11 | 球体碰撞和穿透查询事件。 |
| `MinecartCustomizationState` | `SharedRuntimeMechanisms` | `4.9.93` | `definition/presentation` | 3 | 1 | 4 | 矿车纹理、轮距和定制表现状态。 |
| `TrackedProjectileReferenceState` | `SharedRuntimeMechanisms` | `4.9.94` | `relation/state` | 0 | 5 | 5 | 投射物本地索引和拥有者引用跟踪。 |
| `EnvironmentDamageAndSeatState` | `SharedRuntimeMechanisms` | `4.9.102` | `state` | 6 | 0 | 6 | 环境黑暗伤害和额外座位信息状态。 |
| `MinecartMotionAndTrackState` | `SharedRuntimeMechanisms` | `4.9.140` | `state/query` | 17 | 0 | 17 | 矿车速度、轨道连接、加速和轨道类型状态。 |
| `MinecartDecorationAndSwitchState` | `SharedRuntimeMechanisms` | `4.9.141` | `definition/presentation` | 16 | 0 | 16 | 矿车装饰帧、端点、纹理和轨道切换状态。 |
| `SharedGeneralTeleportAndInterceptionUtilities` | `SharedRuntimeMechanisms` | `4.9.157` | `query/value object` | 20 | 0 | 20 | 传送候选、追逐结果和拦截计算值对象。 |
| `EntityBoundsAndFluidState` | `SharedRuntimeMechanisms` | `4.9.195` | `state` | 7 | 13 | 20 | 实体尺寸、碰撞边界、液体状态和空间范围。 |

来源成员的分区内序号线索范围：229..3975；序号不定义领域边界，必须逐条回到输入表和 Version4 覆盖。

## 分区专属目标

围绕“空间移动与物理”完成独立只读的 Version4 成员审查和 ECS 拆分设计。
- 围绕实体边界/液体状态、碰撞查询、投射物/刷怪缓存、矿车轨道与运动、传送门/晶塔支持、传送拦截和环境伤害/座位状态，核对空间事实与行为系统的真实读写关系。
- 回到 Terraria/Collision.cs、矿车相关类型、传送/门户类型、Main.cs 及直接调用者，重新确认查询缓存、接触结果、运动历史、轨道连接和传送资格的生命周期。
- 区分纯几何 Query、实体空间 Component、矿车运动 System、传送 Command/Adapter、缓存和表现定制；禁止由名字推断 teleport 或 portal 的最终 owner。
- 核对碰撞/运动提交对伤害、液体、Tile、网络同步、死亡和地图/UI 的边界，记录缓存失效、重试和顺序要求。

## 专属证据焦点

- 先完整读取本分区输入报告的成员表，再回到 D:\TRbackup\Version4 重新核对字段、属性、声明类型、初始化/注册、直接读者、直接写者、清理路径和副作用。
- 下列路径只作为本分区的检索种子；路径存在不等于成员语义已确认，必须按公共证据协议记录实际行号、符号、调用者、读者、写者、生命周期和 evidenceStatus。
  - `D:\TRbackup\Version4\Terraria.DataStructures/TrackedProjectileReference.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent/DontStarveDarknessDamageDealer.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent/ExtraSeatInfo.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent/PortalHelper.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent/ShimmerUnstuckHelper.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent/TeleportPylonInfo.cs`
  - `D:\TRbackup\Version4\Terraria.Physics/BallCollisionEvent.cs`
  - `D:\TRbackup\Version4\Terraria.Physics/BallPassThroughEvent.cs`
  - `D:\TRbackup\Version4\Terraria.Physics/PhysicsProperties.cs`
  - `D:\TRbackup\Version4\Terraria/Entity.cs`
  - `D:\TRbackup\Version4\Terraria/Main.cs`
  - `D:\TRbackup\Version4\Terraria/Minecart.cs`
  - `D:\TRbackup\Version4\Terraria/Utils.cs`

- 输入分区抽取出的声明类型焦点：
  - `Terraria.DataStructures.TrackedProjectileReference`
  - `Terraria.Entity`
  - `Terraria.GameContent.DontStarveDarknessDamageDealer`
  - `Terraria.GameContent.ExtraSeatInfo`
  - `Terraria.GameContent.PortalHelper`
  - `Terraria.GameContent.ShimmerUnstuckHelper`
  - `Terraria.GameContent.TeleportPylonInfo`
  - `Terraria.Main`
  - `Terraria.Minecart`
  - `Terraria.Minecart.Customization`
  - `Terraria.Physics.BallCollisionEvent`
  - `Terraria.Physics.BallPassThroughEvent`
  - `Terraria.Physics.PhysicsProperties`
  - `Terraria.Utils`
  - `Terraria.Utils.ChaseResults`
  - `Terraria.Utils.RandomTeleportationAttemptSettings`

- 若路径、类型或行号漂移，记录 version-drift 或 evidence-mismatch；不得静默沿用库存报告行号。Version4 是 Terraria 私有实现的事实来源，完整源码只补证 Version4 已存在文件，tModLoader 只交叉验证公开边界，SS14 只参考结构粒度。

## 专属审查问题

- 碰撞结果、穿透查询、接触伤害和实体边界分别是纯查询、派生结果、权威状态还是短期事件 payload？缓存的失效条件是什么？
- 矿车速度、轨道连接、加速、装饰和切换状态是否共享生命周期；轨道 Query 与运动 System 的输入/输出如何隔离？
- 传送候选、拦截结果、水晶塔注册和脱困支持谁拥有写入权？传送是否是 Command/事务边界，而非直接改位置的 helper？
- 空间状态与实体身份、Tile/液体、战斗、死亡惩罚、网络和地图相机之间哪些引用需要 integration-review？

## 专属不拆分边界

- 不要把 Collision、Minecart、Teleportation、Fluid 和 EntityBounds 合成一个基础物理组件。
- 不要把一次碰撞 Contact、传送候选、拥有者引用、追踪缓存或矿车装饰状态当作持久权威组件。
- 不要让纯 Query 修改实体位置、伤害或传送状态；所有提交方向必须显式可测试。

专属跨域提醒：重点记录与实体生命周期、Tile/液体、战斗、死亡惩罚、网络和地图相机的 integration-risk；EntityReference、TeleportCandidate、CollisionResult、WorldPosition 等跨域类型标 crossSubsystemOwner: integration-review。

## 输出要求

- 将完整研究报告只写入：D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P05-spatial-motion-physics-public-decomposition.md
- 不要写入输入分区报告、公共提示词、其他分区报告、源代码、测试、项目文件或迁移文件。
- 报告必须逐条覆盖本分区成员，给出成员归属表、状态所有权、Component/System/Query/Command/Adapter/Projection 边界、接口契约、依赖方向、顺序约束、不拆分项、兼容策略、evidence-gap、blocking-decision、focused verifier 计划和 Integration Handoff。
- 本轮只读审查不运行编译型命令；没有实际执行的 verifier 必须写 verificationStatus: not-run。
- proposed 类型、路径和接口必须明确标记 proposed；不得写成当前 NLTX 已存在或已经迁移。
- 跨两个或以上分区使用的类型、接口、ID、快照、值对象和 System 顺序只能提出候选，必须写 crossSubsystemOwner: integration-review，不得在本报告单方面裁决。
- 不得修改任何生产代码、测试、项目文件、生成器、公共协议、分区报告或其他并行会话输出。

## 分区完成检查

- [ ] 逐条覆盖本分区全部 119 条成员，成员事实来自输入表但关键语义已回到 Version4 复核。
- [ ] 只使用本分区叶子清单；没有复制、重新设计或宣布其他 P 分区成员。
- [ ] Component 只承载内聚权威状态；派生值、缓存、查询、短期 payload、日志、网络/存档/UI 输出按正确角色隔离。
- [ ] 所有共享候选写 crossSubsystemOwner: integration-review；没有把 proposed 设计写成已实现能力。
- [ ] 未执行的构建、测试和 verifier 均明确 verificationStatus: not-run；证据缺口、冲突和 blocking-decision 已单列。
- [ ] 输出文件路径唯一为：D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P05-spatial-motion-physics-public-decomposition.md

## 会话结束回报

最终消息只回报以下字段：
- partitionId: P05
- outputReport: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P05-spatial-motion-physics-public-decomposition.md
- evidenceSourcesRead: 实际读取的 Version4 文件/类型、必要的完整源码补证、tModLoader 页面和 SS14 参考
- evidenceGaps: 仍缺失或冲突的关键证据
- blockingDecisions: 需要最终整合会话裁决的 owner/顺序/事务边界
- verificationStatus: not-run
- productionCodeModified: no

本提示词只定义本分区的专属范围和审查抓手；公共协议负责共同证据规则、报告结构、禁止项和 Integration Handoff。
