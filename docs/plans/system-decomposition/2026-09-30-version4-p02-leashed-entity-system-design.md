# Version4 P02 Leashed Entity System Design

status: proposed
designStatus: proposed
executionStatus: partial-core-slice
implementationStatus: existing-partial-core-slice
verificationStatus: focused-passed-10-percent
verificationScope: focused-10-percent-core
sourceModified: false
targetModified: true
testsRun: true
buildRun: true
migrationStatus: not-claimed
staticEvidenceReview: 2026-10-01
partitionId: P02
sessionId: 7415697df88244a19f3f4eec4c907e35
systemReport: D:\TRbackup\NLTX\docs\system-decomposition\reports\2026-09-18-system-decomposition-authoritative-P02-leashed-entity.md
targetArea: D:\TRbackup\NLTX\src\NSSLC
authoritativeSource: D:\TRbackup\Version4
completeReferenceSource: D:\TRbackup\无任何删减通过编译

## 1. 目的与边界

本文把已经完成的 P02 System 静态报告细化为一个可执行的目标边界。目标目录中存在一组
既有的注册核心切片和边界适配器，但本文件只交付 proposed 静态设计；这些既有产物没有在
本轮重新验证，也不代表 Version4 行为已经完整迁移或等价。其余 System、Component、Query、
Command、Adapter 和 Projection 仍是提案。

本设计只覆盖 P02 的八个叶子组：

| 叶子组 | 成员数 | 设计入口 |
|---|---:|---|
| LeashedRegistryAndSections | 15 | definition 注册、legacy slot、section index |
| LeashedCritterCoreState | 15 | critter 通用权威状态、NPC 兼容状态 |
| LeashedWalkerBehavior | 7 | walker 状态机和步行参数 |
| LeashedJumperBehavior | 11 | jumper 跳跃参数和可达性 |
| LeashedFlyerBehavior | 11 | flyer 加速、悬停和飞行目标 |
| LeashedKiteBehavior | 20 | 风筝风场、轨迹、Projectile dummy |
| LeashedButterflyVariants | 6 | butterfly variant、fade、opacity |
| LeashedSpeciesPrototypes | 13 | 物种/定义原型引用 |

跨分区的 TileEntity、Item、NPC、Projectile、network session、persistence、world
section 和共享协议类型只作为接口边界处理，最终 owner 必须保持
crossSubsystemOwner: integration-review。

## 2. 证据基线

### 2.1 权威 Version4 与 CPG

权威声明源的关键文件和 SHA-256：

| 文件 | SHA-256 | 结论 |
|---|---|---|
| D:\TRbackup\Version4\Terraria.GameContent\LeashedEntity.cs | DD8A2B1DCC4C3E73ADB65F5D14A39B4C6AD1D5389D972C2C1DDAB7CFA1E0775E | registry、section、lifecycle、network/render virtual surface；多个方法体为空 |
| D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities\TELeashedEntityAnchor.cs | 8A4F7D2A369B439D8428AC365D2B807EA3BA8E1EE24212AAC438E4EBBCFAE08F | 锚点声明和 API 形状；生命周期方法为空 |
| D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities\TELeashedEntityAnchorWithItem.cs | E9E423FA9876BA6D2F44AEFFBE60AE64872B5F0FA4C073B3421609D076EDCAB1 | item anchor 声明；持久化和重生主体为空 |

已通过只读 CPG Query API 使用下列查询：Find-CpgSymbols、Get-CpgTypeSurface、
Find-CpgCallSites、Get-CpgMemberUses、Get-CpgCallableFacts。数据库为
D:\TRbackup\Version4-cpg-export\out-dop8-interproc.sqlite，manifest SHA-256 为
6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364，project fingerprint
为 521CBD4C11E7EB731256C232350A54099F431D0E59EBDF3C38880281F169C11B，导入状态为
complete，967 shards、8,166,789 nodes、71,038,907 edges、1,317 diagnostics，source
snapshot 为 null。CPG 确认了：

- Main.cs 的 `RegisterAll` 调用点（CPG `CallTargets`，`spanStart=88239`）确认；
- Main.cs 的 `UpdateEntities` 调用点（CPG `CallTargets`，`spanStart=300506`）确认；
- WorldGen.cs 的 `Clear` 调用点（CPG `CallTargets`，`spanStart=185539`）确认；
- `LeashedEntity` type surface 查询在选定 shard 内完成，返回 28 个 direct members；
- 上述三个 callable 的 effects 都是 `partial`，缺口为 `CalleeEffectsNotExpanded`；
- `sectionSlot` 的 member-use 在 `LeashedEntity.cs` 返回一个 `AccessMode: Write` 的
  assignment-left-span（`spanStart=3658`），但这不是唯一写者证明；
- 查询结果的 `SourceSnapshotId` 为 null，因此索引事实需与当前 Version4 源码逐项复核；
- 查询结果的 `complete` 只表示索引查询在给定 scope 和预算内完成，不表示运行时闭包。

它不证明动态分派、反射、完整入站入口、唯一写者、运行时调度或行为等价。

### 2.2 完整参考源码

完整参考项目用于恢复被裁剪的行为顺序，不能覆盖权威 Version4 的版本身份：

| 文件 | SHA-256 | 允许用于设计的行为 |
|---|---|---|
| D:\TRbackup\无任何删减通过编译\Terraria.GameContent\LeashedEntity.cs | 716D60C9AC17137F2E006F7437C2D4963F53E1BD81FF7C6745C57DAC3D55287B | full/partial/remove packet、registry、slot reuse、section add/remove/activation、draw、stream |
| D:\TRbackup\无任何删减通过编译\Terraria.GameContent.LeashedEntities\LeashedCritter.cs | 2D54BC0514C917BC030935EBB110B868F1F6844886AFCBE115F0E436BFD6F02C | NPC content、LCG state、通用 spawn/update、NetSend/Receive、dummy/presentation |
| D:\TRbackup\无任何删减通过编译\Terraria.GameContent.LeashedEntities\WalkerLeashedCritter.cs | 609EE57BBBC3B9D3D68DC3B5F9E3D883EB7C8C4E0064DB35F0325BF7FFFA412F | tile query、falling/walking/recall 状态转移 |
| D:\TRbackup\无任何删减通过编译\Terraria.GameContent.LeashedEntities\JumperLeashedCritter.cs | B55BA1E52CC1CBD4101D38F4575C1F1CA563B857F9A3BEBE3683B1E3E5C40B23 | jump target、reachable tile、collision、cooldown |
| D:\TRbackup\无任何删减通过编译\Terraria.GameContent.LeashedEntities\FlyerLeashedCritter.cs | B4B565261F28BE46065D36A51CC61A653658ACE4AEBE629B20EC27487B938043 | acceleration、braking、hover、ground bias |
| D:\TRbackup\无任何删减通过编译\Terraria.GameContent.LeashedEntities\LeashedKite.cs | 6C7C19EB20519080FF233FEDD8C479E74A43B53F0D2C17A20DC73AD345AA7C95 | wind、trail、fast-forward、Projectile dummy/local AI |
| D:\TRbackup\无任何删减通过编译\Terraria.GameContent.Tile_Entities\TELeashedEntityAnchor.cs | B0D54EF39A2E4C820C4A04DB620EE7574A9DA9569440BF278189C29255739584 | world-load respawn、anchor removal、AddNewEntity 链 |
| D:\TRbackup\无任何删减通过编译\Terraria.GameContent.Tile_Entities\TELeashedEntityAnchorWithItem.cs | BF8400B692E8C340381B534495CDD3803F0913C2AE3BF79A38944073B0D2DF1B | item persistence、insert/drop、placement 消息 |

完整参考项目中的其余 P02 原型文件也已纳入本次只读盘点：
`BirdLeashedCritter`、`CrawlerLeashedCritter`、`CrawlingFlyLeashedCritter`、
`DragonflyLeashedCritter`、`FairyLeashedCritter`、`FireflyLeashedCritter`、
`FishLeashedCritter`、`FlyLeashedCritter`、`HellButterflyLeashedCritter`、
`NormalButterflyLeashedCritter`、`RunnerLeashedCritter`、`ShimmerFlyLeashedCritter`、
`SnailLeashedCritter`、`WaterfowlLeashedCritter` 和 `WaterStriderLeashedCritter`。
它们的完整实现可用于恢复定义/行为顺序，但未逐文件建立 Version4 对应关系的条目保持
`partial` 或 `unknown`，不能因为文件存在就升级为 confirmed。

完整参考中的行为均标为 partial，直到同一行为在权威 Version4 或后续行为验证中闭合。

### 2.3 SS14 比较参考（非行为证据）

`C:\Users\shan\Downloads\ECS\space-station-14-master` 只用于检查 ECS 边界的表达方式：

- `Content.Shared/ActionBlocker/ActionBlockerSystem.cs:22-60` 将 `EntityQuery<T>` 作为
  System 依赖；`CanComplexInteract` 只读查询，而 `UpdateCanMove` 会 raise event、`Dirty`
  并写回 Component，因此返回 `bool` 不能单独证明 Query 纯度。
- `Content.Shared/Abilities/Mime/MimePowersSystem.cs:18-58` 在 `Initialize` 注册事件，在
  `Update` 用 `EntityQueryEnumerator<T>` 遍历并写状态/发 popup；这支持“事件输入、System
  提交、Query 只读”的边界表达，但不证明 Terraria 的调度或行为。

SS14 的类型、事件和 scheduler 约定不移植为 P02 事实；它只作为 proposed API 形状的比较
参考，P02 的 owner、顺序和副作用仍以 Version4 源码、只读查询结果和完整参考的 partial
恢复线索为准。

## 3. 设计原则

1. DefinitionId、content item/NPC/Projectile ID、运行时 entity identity、whoAmI
   legacy slot、network slot 和 persistent anchor ID 必须是不同概念。
2. Registry.Prototypes、species prototype 和 static dummy 是定义/兼容资源，不是
   运行时实体权威状态。
3. BySection、ActiveSectionList、section list/count/emptySlots 是可重建索引，
   不进入持久化权威快照。
4. 一个不变量只允许一个提交者。Walker、Jumper、Flyer 只能产生 transition facts，
   不能分别写 WaitTime、State、TargetPosition 和 random cursor。
5. Query 只能返回不可变快照或标量；创建 bucket、分配 slot、接收网络、重生实体和
   删除实体均是 Command/System effect，不因返回 bool 就成为 Query。
6. ActiveSections.SectionActivated 和 RemoteClient.NetSectionActivated 都是外部
   生命周期输入。P02 不自行拥有 world section activity。
7. 空方法体、未解析调用、CPG 零命中和完整参考代码都不能单独升级为 confirmed。

## 4. 目标构件

### 4.1 Definition catalog

拟议文件保持 P02 领域优先，先放在 src/NSSLC/LeashedEntity/ 或现有 NSSLC 等价领域
目录；若实际项目结构不允许，执行阶段先记录路径决策，不预建空的 Systems/ 或
Queries/ 子目录。

| 构件 | 责任 | 权威写入 |
|---|---|---|
| LeashedDefinitionCatalog | 保存 13 个物种/能力定义、kite 定义和 registration order 映射 | 只在 bootstrap 阶段写入 |
| LeashedDefinitionQuery | 按稳定 definition key/content mapping 返回不可变定义 | 无 |
| LeashedDefinitionBootstrapAdapter | 将旧 RegisterAll、Register<T>、Register(prototype) 转成 catalog bootstrap | 只调用 catalog bootstrap，不暴露 live prototype |

完整参考中 Registry.RegisterAll 在 LeashedEntity.cs:125-149 先加入 null sentinel，
再注册 kite、walker/flyer/butterfly/species/jumper/water-strider；Register 在
151-177 以 Prototypes.Count 分配 Type，Get(int) 返回 prototype。稳定 ID、重复
注册、mod override 和重入语义仍是 blocking evidence gap。

### 4.2 Registration and lifecycle

LeashedEntityRegistrationSystem 是 P02 的拟议本地协调者，负责：

- 校验 definition、anchor snapshot 和 host ownership；
- 创建/水合 entity-side relation；
- 分配或释放 whoAmI compatibility slot；
- 写入 lifecycle active/spawned/removed；
- 向 section index 提交 membership intent；
- 将 network full sync、anchor respawn 和 remove 归一到一个 commit port。

它不直接拥有 TileEntity item storage、world persistence、network transport、NPC/Projectile
runtime 或 section activity。

### 4.3 Section index and activation

拟议 LeashedSectionIndexSystem 维护：

- entity -> section membership；
- section bucket 和 recyclable sectionSlot；
- compaction；
- ActiveSections.IsSectionActive 对当前 index 的观察；
- activate/deactivate 时的 lifecycle effect intent。

完整参考 LeashedEntity.cs:196-283 显示 section list 扩容、slot 写入、空槽删除、半空
时压缩、activate 时对 list 调 Spawn(false)、deactivate 时调 Despawn()。完整参考
340-350 还显示 ActiveSections.SectionActivated 与 RemoteClient.NetSectionActivated
的订阅。权威 Version4 的 LeashedEntity.cs 没有同样完整的注册闭包，因此 section
事件关系仍为 partial。

### 4.4 Shared critter authority

拟议 LeashedCritterSystem 是 generic critter state 的唯一提交者：

- npcType/size/content binding 由 definition query 和 NPC adapter 提供；
- WaitTime、State、TargetPosition、LCG state、position/velocity/direction 由
  transition result 统一提交；
- frame、frameCounter、spriteDirection、netOffset 中的 presentation/interpolation
  部分只能由明确标记的 presentation writer 写入；
- Recall 的 position/velocity 变化必须以显式 transition/effect 结果表达。

LeashedWalkerSystem、LeashedJumperSystem 和 LeashedFlyerSystem 只接收 immutable
world snapshot、definition 和 authority snapshot，输出一个 transition result。是否拆
成独立 scheduler node，取决于后续确认的 distinct phase/input；默认先保留 capability
evaluator，避免增加第二写者。

完整参考证据：

- LeashedCritter.cs:47-109 将 item -> NPC content、position offset、direction、LCG、
  wait/state/target 写入网络；111-125 初始化 anchor center、target 和 random。
- WalkerLeashedCritter.cs:29-109 使用 WorldGen.SolidTile2、AnyLiquidAt 和 random
  选择目标，执行 falling、recall、walking、dummy frame。
- JumperLeashedCritter.cs:44-98,100-275 维护 jump cooldown、reachable tile、
  collision、gravity 和 recall。
- FlyerLeashedCritter.cs:42-143 使用目标、加速度、braking、solid tile 和 ground bias。

上述所有完整参考关系均为 partial，因为权威 Version4 对应方法体可能为空且
WorldGen/random/NPC effect closure 尚未绑定。

### 4.5 Kite

拟议 LeashedKiteSystem 独立于 critter authority，负责 kite-owned transition：

- wind target/current、cloud alpha、time counter、time without wind；
- kite distance、position、velocity、rotation、sprite direction；
- trail arrays 的派生/缓存更新；
- fast-forward 与 normal update 两种输入模式。

IProjectileCompatibilityPort 负责 Projectile dummy 的 set defaults、KiteLogic、
HandleMovement、collision params 和 localAI[0/1] 映射。Projectile 不是 kite state
component 的第二写者。完整参考 LeashedKite.cs:127-246 显示 NearbySectionsMissing、
wind/world checks、trail shift、fast-forward 和 dummy copy 的顺序；248-257 显示 spawn
先定位、设 velocity、调用 update，再采样 global wind/cloud。当前权威 Version4 需要
进一步恢复这些效果，不能把完整参考直接当成已确认 target behavior。

### 4.6 Butterfly and presentation

LeashedButterflyVariantComponent 只保存 normal butterfly 的 item/style-derived variant。
NormalButterflyLeashedCritter.cs:11-45（完整参考）显示 variant 来自 sample.placeStyle，
且只在 full network payload 后缀传输。

Empress fade/opacity 属于 presentation candidate。完整参考
EmpressButterflyLeashedCritter.cs:13-58 显示 fade 依赖 local-player distance、lighting、
random dust 和 Opacity；这些 effect 不应被一个纯 Query 隐藏。拟议
LeashedPresentationProjection 只消费 authority snapshot，生成 render snapshot，不回写
gameplay authority。

### 4.7 Network and anchor adapters

LeashedEntityNetworkAdapter：

- full/partial/remove packet framing；
- slot/type/anchor validation；
- full sync 创建或水合 entity 的 command；
- partial sync 必须先检查 slot/type；
- remove 只产生 registration remove command。

完整参考 LeashedEntity.cs:22-119 显示：full sync 在 slot 为空时
Registry.Get(type).NewInstance()、AddNewEntity，类型或 anchor 不匹配抛异常；partial
sync 校验 type；remove 通过 TryGet 后调用 Remove。当前权威 Deserialize 与
NetReceive 是 stub/空体，故 packet error semantics 为 unknown。

LeashedAnchorAdapter：

- TELeashedEntityAnchor.OnWorldLoaded -> respawn；
- OnRemoved -> despawn；
- item anchor WriteExtraData/ReadExtraData；
- InsertItem、DropItemForTileBreak；
- TECritterAnchor item -> makeNPC -> critter prototype；
- TEKiteAnchor item -> shoot -> kite prototype。

完整参考 TELeashedEntityAnchor.cs:17-62、TELeashedEntityAnchorWithItem.cs:10-78、
TECritterAnchor.cs:65-115 和 TEKiteAnchor.cs:63-83 支持这一适配边界。最终
TileEntity/persistence/item owner 为 integration-review。

## 5. 状态所有权矩阵

| 状态 | 生命周期 | 拟议 owner | 允许读取 | 不允许 |
|---|---|---|---|---|
| catalog definitions / definition IDs | process/content bootstrap | LeashedDefinitionCatalog | registration, behavior, adapter | runtime behavior 改 catalog |
| entity lifecycle | entity instance | LeashedEntityRegistrationSystem | section, behavior, projections | section index 直接改 durable lifecycle |
| legacy whoAmI slot | entity/network session | registration + compatibility adapter | network query | 作为 persistent identity |
| section bucket/slot | derived per-world index | LeashedSectionIndexSystem | activation/update/network projection | persistence authority |
| anchor relation | entity/anchor lifecycle | entity-side registration; host external | behavior, network, presentation | P02 直接写 TileEntity storage |
| critter Wait/State/Target/random | tick-to-tick entity authority | LeashedCritterSystem commit port | capability evaluators | multiple behavior writers |
| Walker/Jumper/Flyer tuning | immutable definition | catalog | evaluator/query | per-entity mutation |
| kite wind/timers/motion | entity tick authority | LeashedKiteSystem | presentation/network adapter | Projectile adapter direct authority write |
| butterfly variant | entity/content binding | registration/network validation | presentation | renderer changes variant |
| frame/opacity/trail/interpolation | derived/presentation | projection or explicit presentation writer | renderer | persistence/network authority unless confirmed |
| item storage/persistence/network transport | external session/domain | integration-review | P02 adapters | P02 invent final owner |

## 6. Conceptual APIs

下列 API 是 proposed P02 契约；其中部分名称已出现在目标目录的既有核心切片中，但尚未
证明它们已接入 Version4 的真实入口、满足旧行为或形成唯一 authority，因此不能据此写成
已迁移实现。

| API | Kind | Input | Output/effect | Purity/status |
|---|---|---|---|---|
| RegisterDefinitions | bootstrap command | ordered definition descriptors | catalog commit | effectful; ID policy unknown |
| TryResolveDefinition | Query | stable key/content mapping | immutable definition snapshot | proposed pure only after alias/cache closure |
| TryHydrateEntity | command | anchor snapshot, definition key, optional network slot | entity/lifecycle/slot/index intent | one commit; validation failure must be atomic |
| TryGetByLegacySlot | Query | slot | immutable entity snapshot | no alias; slot generation semantics unknown |
| ReconcileSections | command | section activity snapshot | activate/deactivate/index transitions | writes derived index and lifecycle effect intents |
| EvaluateWalker/Jumper/Flyer | rule/query-like evaluator | authority snapshot + world snapshot + definition + random input | transition result | must not write world/entity |
| CommitCritterTransition | command | transition result | one authority commit | sole generic writer |
| EvaluateKite | evaluator/system method | kite snapshot + wind/world/projectile port snapshot | kite transition + compatibility intents | external effect separated |
| ApplyNetworkFrame | command | validated full/partial/remove frame | registration/authority commit | adapter cannot allocate unvalidated state |
| BuildFull/PartialFrame | projection | committed snapshot | bytes/transport intent | no owner write |
| ApplyAnchorItem | command | anchor identity + item content | item host commit + respawn intent | host ownership integration-review |
| BuildRenderSnapshot | projection | committed entity/presentation state | immutable render snapshot | no gameplay write |

## 7. Schedule and barriers

The proposed order is:

    definition bootstrap
      -> anchor/item snapshot and entity hydration
      -> section membership rebuild
      -> section activity reconciliation
      -> capability evaluation
      -> one authority commit
      -> removal/slot/index cleanup
      -> network/persistence/render projections

For the visible complete-reference tick, UpdateEntities performs
RecheckActiveSections before _UpdateEntities; each active entity calls Update, then
StreamNetUpdates, then inactive cleanup. This is source evidence from
LeashedEntity.cs:455-509 in the complete reference, not a confirmed scheduler contract for
the current Version4.

Required barriers:

- catalog must be committed before any definition lookup;
- relation and slot must be committed before full network publish;
- section index must be rebuilt before activation/deactivation effects;
- behavior evaluation must read one stable snapshot;
- authority commit must complete before network/persistence/render projection;
- remove must not expose a stale slot to a subsequent full/partial frame.

ActiveSections.SectionActivated and RemoteClient.NetSectionActivated are event inputs, not
P02-owned clocks. Their registration, unsubscribe and multi-world behavior remain
partial/unknown.

## 8. Rejected alternatives

| Alternative | Rejection |
|---|---|
| One LeashedEntitySystem for all fields, network, anchor and render | mixes registry, durable state, external effects and presentation; hides multiple writers |
| One runtime System per species prototype | turns definitions into inheritance fan-out and duplicates generic authority |
| Store BySection/ActiveSectionList in persistence | indexes are rebuildable and can become stale across reset/load |
| Let each Walker/Jumper/Flyer write generic critter state | creates competing writers for timer, target, random and lifecycle |
| Put Projectile dummy/local AI inside LeashedKiteBehaviorComponent | conflates compatibility runtime with kite authority and prevents integration review |
| Treat GetSection, TryGet, or IsSectionActive as always-pure Query | missing bucket creation, aliasing, time and shared cache writes are not closed |
| Use complete reference as authoritative implementation | source hash and current Version4 declaration differ; complete reference is behavior recovery only |

## 9. Blocking decisions

Implementation remains blocked until the following are resolved:

1. stable definition and content mapping contract, including sentinel and repeated bootstrap;
2. anchor TileEntity and item persistence owner;
3. section activation/event subscription and client/server timing;
4. sole writer for generic critter state and random semantics;
5. kite Projectile compatibility ownership;
6. full/partial/remove network and malformed frame behavior;
7. scheduler barrier and remove-before-reuse rule;
8. multi-world, unload, exception and session-boundary cleanup.

## 10. Verification intent

verificationStatus: focused-passed-10-percent. 本轮只运行了目标核心切片的 focused verifier；
它只覆盖 catalog、legacy slot/generation、section activation/compaction/hole reuse、clear
和 network frame validation/command result。它没有证明 Version4 运行时接入、完整观察向量、
行为等价或迁移成功。

本轮修复并验证了 `LeashedSectionIndex.Add` 的空槽优先复用，并为
`LeashedEntityNetworkAdapter` 增加了不写权威状态的 `TryValidateAndCreateCommand`；网络命令
仍未接入 transport 或 registration commit，保持 `partial`/`integration-review`。

Later implementation work must add focused checks for:

- definition ID determinism and catalog immutability;
- slot reuse, full/partial/remove validation and stale-slot rejection;
- section add/remove/compaction/activation/deactivation/reset;
- Walker/Jumper/Flyer transitions with controlled tile and random snapshots;
- Kite normal/fast-forward/wind/no-wind and Projectile adapter effects;
- anchor load/insert/break/persist and respawn atomicity;
- butterfly full-only variant suffix and presentation purity;
- phase/barrier ordering and one-writer assertions.

已运行的局部 verifier 不覆盖 network transport、anchor/item persistence、Walker/Jumper/Flyer 行为、
Kite/Projectile 兼容、butterfly presentation、scheduler integration 或完整 Observation tuples。
这些区域保持 `partial`/`unknown`，`migrationStatus` 保持
`not-claimed`。本设计不作行为等价或迁移成功声明。
