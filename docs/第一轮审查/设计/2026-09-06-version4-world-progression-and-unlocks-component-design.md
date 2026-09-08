# Version4 World Progression and Unlocks Component-only Design

## 1. 设计元数据

```yaml
documentType: Component-only Design
designId: WPU.DESIGN.ComponentOnly.2026-09-06
subsystemId: WorldProgressionAndUnlocks
taskNumber: 07
sourceReportPath: D:\TRbackup\NLTX\docs\第一轮审查\2026-09-05-version4-world-progression-and-unlocks-public-decomposition.md
sourceReportMetadataReportPath: missing
evidenceStatus: partial
nltxStatus: partial
designStatus: decision-required
verificationStatus: not-run
componentCount: 2
generatedDate: 2026-09-06
pathStatus: proposed
```

`sourceReportPath` 是当前会话明确提供并实际读取的研究报告路径。该研究报告的元数据没有声明 `reportPath` 字段，因此记录 `sourceReportMetadataReportPath: missing`，形成一个 `evidence-mismatch`；这不改变本设计对实际输入来源的记录。

本文仅表达 Component 层的设计提案。`status: proposed` 表示尚未在 NLTX 中创建或验证对应代码。

## 2. 设计范围与排除范围

本设计的范围是 World Progression and Unlocks 中承载 Bestiary 解锁事实的 Component，以及该 Component 所需的短生命周期扫描缓存。具体包括：

- Component 名称、稳定 `componentId`、归属候选和 World/entity 范围；
- Component 字段的建议类型、默认值、权威性分类、生命周期和不变量；
- 击杀、目击、交谈三类事实在 Component 中的归属；
- 短生命周期目击扫描缓存与持久化事实的边界；
- Entity 与 Component 的组合关系，以及持久 ID、网络 ID 和实体引用的分类；
- 当前 NLTX 已有内容身份、协议、WLD 输入和相邻 World progression 状态与候选 Component 的边界；
- 仍需整合裁决的 Component owner 与字段 owner 问题。

以下内容明确排除，不在本文中定义其结构、执行关系或实现方式：

- System、Query、Command、Event、Adapter、Projection、Port；
- 调度顺序、主循环、网络发送流程、存档流程和客户端渲染流程；
- focused verifier、测试计划、迁移计划和实现步骤；
- `.cs`、`.csproj` 创建计划或可运行代码；
- Bestiary 展示状态、进度报告、UI 状态、成就通知或社交数据作为 Component；
- 任何将网络包、WLD reader 或二进制 reader/writer 直接放入 Component 的设计。

## 3. 组件设计依据

### 3.1 Version4 权威状态证据

Version4 的 `BestiaryUnlocksTracker` 将三个 tracker 作为同一 Bestiary 解锁聚合的一部分，并共同承担保存、加载、校验、重置和玩家加入相关生命周期：

- `D:\TRbackup\Version4\Terraria.GameContent.Bestiary\BestiaryUnlocksTracker.cs:5-11` 声明 `BestiaryUnlocksTracker`、`Kills`、`Sights`、`Chats`；
- `BestiaryUnlocksTracker.cs:13-63` 显示三个 tracker 的共同生命周期委托；
- `D:\TRbackup\Version4\Terraria\Main.cs:1019-1023` 和 `Main.cs:3353-3355` 显示全局持有并初始化 `BestiaryTracker`；
- `D:\TRbackup\Version4\Terraria.GameContent\IPersistentPerWorldContent.cs:5-13` 提供 Save、Load、ValidateWorld、ResetWorld 生命周期契约；
- `D:\TRbackup\Version4\Terraria.GameContent\IOnPlayerJoining.cs:3-5` 提供玩家加入时读取相关状态的生命周期契约。

这些证据支持把三类持久事实放到一个 World 范围的候选 Component 中，但尚不能裁决 NLTX 最终的 World attachment owner 或跨子系统实体身份 owner。

### 3.2 三类事实的字段边界

- 击杀事实由 `NPCKillsTracker.cs:14-16` 的正数上限和 `Dictionary<string,int>` 承载；`NPCKillsTracker.cs:23-55` 显示按 Bestiary credit ID 增加、限制并按 persistent ID 读取；
- 目击事实由 `NPCWasNearPlayerTracker.cs:15` 的 `HashSet<string>` 承载；同文件 `:17`、`:19` 的两个 `List` 只作为扫描和去重缓存，且 `:60-95` 只保存事实集合；
- 交谈事实由 `NPCWasChatWithTracker.cs:14` 的 `HashSet<string>` 承载，`:21-53` 显示幂等登记、直接读取和持久化边界；
- `Player.cs:3378-3386` 与 `NPC.cs:42257-42260` 证明交谈事实来源于交互，但交互动作本身不是 Component 字段；
- `NPC.cs:45907-45909`、`:65323-65329` 和 `:79655-79660` 证明有效死亡路径与 Bestiary credit ID 映射属于事实写入前的领域输入，不应把死亡流程或 network ID 变成 Component 状态。

### 3.3 派生值和外部格式边界

`CommonEnemyUICollectionInfoProvider.cs:38-85`、`CritterUICollectionInfoProvider.cs:14-23`、`TownNPCUICollectionInfoProvider.cs:14-23` 从事实派生五级展示状态或资格结果。`BestiaryEntryUnlockState.cs:3-10`、`BestiaryUnlockProgressReport.cs:3-18` 和 `Main.cs:13625-13644`、`:14001-14004`、`:14167-14170` 均不代表新的权威事实字段，因此不进入候选 Component。

`WorldFile.cs:1186-1205`、`:1881-1892`、`:3278-3281`、`:3491-3512` 说明 Bestiary 是 WorldFile section index `9`，但二进制 section、版本路由、reader/writer 和兼容输入属于边界格式，不属于 Component 本体。`NetBestiaryModule.cs:10-43` 同样只证明网络消息的类型和 payload 形状，不能把消息类型或包字段提升为权威 Component 字段。

### 3.4 当前 NLTX 边界证据

- `D:\TRbackup\NLTX\src\Content\ContentIdentityCatalog.cs:5-19,40-42,54-58,92-95` 已有 NPC network ID 到 Bestiary credit ID 的冻结映射；它是内容身份元数据，不是 World 解锁事实 Component；
- `D:\TRbackup\NLTX\src\Content\ContentPresentationIndex.cs:5-58` 已有排序 ID 和 rarity stars；它是展示元数据，不是解锁事实 Component；
- `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\World\WorldProgressionState.cs:7-186` 已有 Hardmode、Boss defeat、事件、入侵和 Slime Rain 状态，但没有三类 Bestiary 事实，不能与目标 Component 合并；
- `BestiaryUnlockType.cs:3-8`、`BestiaryModulePacket.cs:3-6` 和 `TerrariaPacketCodec.cs:343-383` 形成了局部协议模型；`TerrariaSession.cs:345-349` 解码 module `4` 后丢弃结果，不能证明协议状态已经进入目标 Component；
- `WldBestiaryReader.cs:7-56`、`WldV88ToV319Reader.cs:63-71` 形成了局部兼容输入模型，但不能替代权威 World Component；
- `D:\TRbackup\NLTX\Test` 与 `D:\TRbackup\NLTX\dome\Test` 没有目标权威 Bestiary Component 的实现或覆盖证据。

## 4. Version4 成员到 Component 归属表

| Version4 成员或概念 | 归属 | 建议字段/边界 | 权威分类 | 归属结论 |
|---|---|---|---|---|
| `BestiaryUnlocksTracker` | World entity | Component attachment 的聚合边界 | 权威 owner 候选 | 不单独成为字段；作为聚合证据 |
| `BestiaryUnlocksTracker.Kills` | `ProgressionAggregate` | `KillCountsByPersistentId` | authoritative persistent state | 纳入 |
| `BestiaryUnlocksTracker.Sights` | `ProgressionAggregate` | `SightedPersistentIds` | authoritative persistent state | 纳入 |
| `BestiaryUnlocksTracker.Chats` | `ProgressionAggregate` | `ChattedPersistentIds` | authoritative persistent state | 纳入 |
| `_killCountsByNpcId` | `ProgressionAggregate` | `Dictionary<PersistentBestiaryId, int>` | authoritative persistent state | 类型需 identity owner 裁决 |
| `_wasNearPlayer` | `ProgressionAggregate` | `HashSet<PersistentBestiaryId>` | authoritative persistent state | 纳入 |
| `_chattedWithPlayer` | `ProgressionAggregate` | `HashSet<PersistentBestiaryId>` | authoritative persistent state | 纳入 |
| `_playerHitboxesForBestiary` | `BestiaryDiscoveryCache` | `PlayerBestiaryBounds` | transient scan cache | 纳入缓存 Component |
| `_wasSeenNearPlayerByNetId` | `BestiaryDiscoveryCache` | `SeenNpcNetworkIds` | transient dedup cache | 纳入缓存 Component |
| NPC Bestiary credit ID | 组件字段的 key 输入 | `PersistentBestiaryId` | persistent identity key | 不复制内容表；owner 未决 |
| NPC network ID | 缓存/协议输入 | `NetworkId` | transient/session identity | 不作为持久 key |
| Player hitbox | 缓存输入 | `PlayerBestiaryBounds` | transient geometry snapshot | 不作为 Player 状态 |
| `BestiaryEntryUnlockState` | 无 | 派生展示结果 | derived value | 不纳入 |
| `BestiaryUnlockProgressReport` | 无 | 派生完成度 | derived value | 不纳入 |
| `NetBestiaryModule` / `BestiaryModulePacket` | 无 | 外部协议模型 | compatibility/input boundary | 不纳入 |
| `WldBestiaryReader` | 无 | 外部兼容输入 | compatibility/input boundary | 不纳入 |
| `ContentIdentityCatalog` Bestiary map | 无 | 内容身份元数据 | content metadata | 不纳入 |
| `ContentPresentationIndex` sorting/rarity | 无 | 内容表现元数据 | presentation metadata | 不纳入 |

归属表中的“纳入”仅表示字段属于候选 Component 的数据模型；不表示 Version4 成员已经迁移，也不表示 NLTX 已有同名实现。

## 5. Component 定义

### 5.1 `ProgressionAggregate`

```yaml
componentId: WPU.COMP.ProgressionAggregate
name: ProgressionAggregate
status: proposed
pathStatus: proposed
componentOwner: WorldProgressionAndUnlocks (candidate)
crossSubsystemOwner: integration-review for World attachment and shared identity types
entityScope: exactly one active World entity
cardinality: at most one per active World entity
authority: authoritative World-local persistent state
```

#### 5.1.1 设计职责

`ProgressionAggregate` 只承载属于当前 World 的 Bestiary 解锁事实：某个持久 Bestiary 身份被击杀了多少次、是否已经被目击、是否已经交谈过。它不承载 NPC 实例、Player 实例、展示条目、网络包、WLD reader 或派生资格结果。

将三类事实放在同一个 Component 中，是基于 Version4 已经存在的 `BestiaryUnlocksTracker` 聚合边界和共同 World 生命周期作出的当前提案。三个字段仍保持独立的事实语义；合并的是 World attachment 和 owner，不是把三类数据压成无区分的集合。

#### 5.1.2 字段定义

| 字段 | 建议类型 | 默认值 | 分类 | 字段不变量 |
|---|---|---|---|---|
| `KillCountsByPersistentId` | `Dictionary<PersistentBestiaryId, int>` | 空 dictionary | authoritative persistent state | 每个 count 必须在 `0..999999999`；key 必须是有效持久 Bestiary 身份；同一 key 只有一个当前 count |
| `SightedPersistentIds` | `HashSet<PersistentBestiaryId>` | 空 set | authoritative persistent state | membership 是幂等的；集合只包含有效持久 Bestiary 身份；不因扫描缓存清空而丢失 |
| `ChattedPersistentIds` | `HashSet<PersistentBestiaryId>` | 空 set | authoritative persistent state | membership 是幂等的；集合只包含有效持久 Bestiary 身份；不因玩家离开或缓存清空而丢失 |

#### 5.1.3 生命周期

- World entity 创建时，三个字段均为空；这对应 Version4 `Main.cs:3353-3355` 的 tracker 初始化证据；
- 外部 World 数据通过边界校验后恢复到三个字段；二进制读写对象本身不进入 Component；
- 活跃 World 生命周期内，三个字段是唯一的 Bestiary 事实候选存储；字段的持久性不依赖 NPC 或 Player entity 是否仍然存在；
- World reset 或 unload 时，三个字段共同清理；清理后不得残留前一个 World 的事实；
- 玩家加入时只是读取当前 World 事实，不产生新的 Component 字段；
- 未提出 `revision`、`schemaVersion` 或 `lastUpdated` 字段，因为当前 Version4 证据没有表明它们是事实的一部分，且加入会把格式/同步元数据混入权威领域状态。

#### 5.1.4 ID 分类与边界

- `PersistentBestiaryId` 是三类持久事实的 key 候选；它对应 Version4 Bestiary credit/persistent ID 语义，但在 NLTX 中的共享 owner 尚未裁决；
- NPC network ID 不是本 Component 的 key。它只用于会话内定位或缓存去重，不能跨 World、跨加载周期或跨网络会话代表同一个 Bestiary 身份；
- Player ID 不是本 Component 的 key。目击和交谈事实属于 World 级 membership，而不是某个玩家的持久个人进度；
- NPC entity reference 不是本 Component 的字段。NPC 实例可能销毁、重建或改变 network ID，不应成为持久解锁事实的 owner；
- Content identity map 可以提供身份映射，但不应复制到本 Component 中形成第二份内容元数据。

#### 5.1.5 明确不纳入的相邻值

不把 `BestiaryEntryUnlockState`、`BestiaryUnlockProgressReport`、完成度百分比、UI 状态、portrait/icon、drops、Achievement 通知、Social API 状态、网络包字段或 WLD 版本 marker 加入本 Component。这些值分别属于派生结果、外部表现、外部副作用或兼容边界，不能成为 World 权威事实的字段。

### 5.2 `BestiaryDiscoveryCache`

```yaml
componentId: WPU.COMP.BestiaryDiscoveryCache
name: BestiaryDiscoveryCache
status: proposed
pathStatus: proposed
componentOwner: WorldProgressionAndUnlocks (candidate)
crossSubsystemOwner: integration-review for geometry value and NetworkId
entityScope: at most one optional cache on the active World entity
cardinality: zero or one per active World entity
authority: transient scan and dedup cache; never authoritative persistence
```

#### 5.2.1 设计职责

`BestiaryDiscoveryCache` 只承载目击判定所需的短生命周期扫描数据。它与 `ProgressionAggregate` 分离，是为了防止把临时几何快照和 network ID 混入持久事实。它不代表已目击状态；已目击状态只存在于 `ProgressionAggregate.SightedPersistentIds`。

#### 5.2.2 字段定义

| 字段 | 建议类型 | 默认值 | 分类 | 字段不变量 |
|---|---|---|---|---|
| `PlayerBestiaryBounds` | `List<Rectangle>`（候选） | 空 list | transient scan cache / geometry snapshot | 每次扫描开始前清空并按当前 active Player bounds 重建；不得作为持久身份或 World unlock 事实 |
| `SeenNpcNetworkIds` | `List<NetworkId>`（候选；Version4 原始类型为 `List<int>`） | 空 list | transient dedup cache / session identity | 只在当前扫描/World 会话的去重范围内有效；不得写入 WorldFile；不得替代 `PersistentBestiaryId` |

#### 5.2.3 生命周期

- 缓存随目击能力所在的 active World entity 创建或按需初始化；
- 每次目击扫描开始前，`PlayerBestiaryBounds` 和 `SeenNpcNetworkIds` 都可清空并重建；
- Version4 `NPCWasNearPlayerTracker.cs:106-137` 证明缓存由 active Player bounds、active critter NPC 和 network ID 参与扫描；
- Version4 `NPCWasNearPlayerTracker.cs:97-104` 证明两个缓存与事实集合一起清理，但 `:60-95` 证明保存/加载/校验只覆盖 `_wasNearPlayer`；
- World reset、unload 或扫描上下文失效时，缓存可以丢弃；丢弃缓存不得清除 `ProgressionAggregate` 中已持久化的 sight membership；
- 缓存不附着于 Player entity 或 NPC entity，因为其范围是一次 World 级扫描上下文，且 Version4 的两个列表不是某个单一实体的持久状态。

#### 5.2.4 类型保留与未决项

当前只保留 `Rectangle` 和 `NetworkId` 作为字段类型候选，不在本设计中创建新的几何值类型或网络 ID 类型。`Rectangle` 是否应替换为 NLTX 已有的领域几何值类型、`NetworkId` 是否已有稳定公共 owner，分别记录为 evidence-gap 和 owner decision，不能通过猜测解决。

## 6. Entity 与 Component 组合

目标组合是一个 active World entity 上的两个可区分 Component：

```text
World entity
├── ProgressionAggregate       (required candidate; persistent authority)
└── BestiaryDiscoveryCache     (optional candidate; transient cache)
```

组合约束如下：

- `ProgressionAggregate` 的 cardinality 是 exactly one（在目标 World progression 模型启用时），其三个事实分区共享同一个 World owner；
- `BestiaryDiscoveryCache` 的 cardinality 是 zero or one，只在需要扫描缓存时存在；
- 两个 Component 不能附着于单个 NPC entity 或 Player entity；NPC、Player 的生命周期变化不应改变 World 事实的身份；
- `ProgressionAggregate` 与 `BestiaryDiscoveryCache` 之间只有数据边界关系：缓存中的 network ID 和几何快照不能直接成为持久字段；
- `ContentIdentityCatalog`、`ContentPresentationIndex` 和已有 `WorldProgressionState` 不作为这两个 Component 的隐式字段；它们保持各自已有的内容元数据或相邻 World progression 边界；
- Entity ID、persistent Bestiary ID、network ID 和 Player ID 不可互换。只有 `PersistentBestiaryId` 候选可作为三类事实的持久 key。

## 7. 组件拆分与合并决策

### 7.1 三类事实合并为一个 `ProgressionAggregate`

当前选择一个聚合 Component，内部保留三个语义独立的字段分区，理由是：

- Version4 已将 `Kills`、`Sights`、`Chats` 暴露在同一个 `BestiaryUnlocksTracker` 下；
- 三类事实共享同一 World 持久化、校验、重置和初始化生命周期；
- 三类事实都按持久 Bestiary 身份索引，属于同一个 World-local unlock aggregate；
- 三个独立公开 Component 会产生多个 World 状态入口，增加 attachment、序列化 owner 和 reset owner 漂移风险；
- 当前证据没有显示三类事实需要不同的 entity 范围或不同的跨子系统 owner。

这不是把 `KillCountsByPersistentId`、`SightedPersistentIds` 和 `ChattedPersistentIds` 合并成一个无类型 map；字段仍分别保留 count、sight membership 和 chat membership 的不变量。

### 7.2 将目击扫描缓存拆为 `BestiaryDiscoveryCache`

缓存单独拆分，理由是：

- `List<Rectangle>` 和 `List<int>` 在 Version4 中用于扫描/去重，不在保存/加载/校验路径中作为权威数据；
- 缓存的清理和重建频率不同于三类持久事实；
- network ID 的会话身份语义与 persistent Bestiary ID 的持久身份语义不同；
- 单独的缓存边界能避免把临时数据误认为 WorldFile state。

### 7.3 暂不提出更多 Component

当前不把每种事实再拆成独立 Component，也不把每个 Bestiary entry、每个 NPC、每个资格状态或每个 UI 状态建成 Component。现有证据只支持两个清晰的 World 范围边界；继续拆分会在缺少 owner、entity scope 和生命周期证据的情况下制造更多状态入口。

## 8. 不单独创建 Component 的对象

| 对象 | 不单独创建 Component 的原因 |
|---|---|
| 单个 Bestiary entry | 它是内容条目/展示对象，不是 World 级事实 owner；解锁状态由事实派生 |
| 单个 NPC kill count | count 是按 `PersistentBestiaryId` 聚合的字段，不需要为每个 NPC 实例复制 Component |
| 单个 sight membership | 它是 `SightedPersistentIds` 的集合成员，不具备独立 entity 范围 |
| 单个 chat membership | 它是 `ChattedPersistentIds` 的集合成员，不具备独立 entity 范围 |
| `BestiaryEntryUnlockState` | 五级展示派生值，不是持久权威事实 |
| `BestiaryUnlockProgressReport` | 完成度派生值；零条目返回 `1f` 的规则也不产生独立事实状态 |
| UI icon、portrait、drops | 内容表现或 UI 资源，不属于 World progression authority |
| Achievement notification | 外部通知/副作用结果，不属于解锁事实字段 |
| Social API 状态 | 外部社交能力状态，不属于本 Component 的 World-local facts |
| `NetBestiaryModule` | 网络协议边界，不是 entity state |
| `BestiaryModulePacket` | 网络 payload DTO，不是权威 Component |
| `WldBestiaryReader` | 旧 WorldFile 输入/兼容边界，不是运行时事实存储 |
| `ContentIdentityCatalog` 的 Bestiary credit map | 内容身份元数据；复制它会形成第二份 identity authority |
| `ContentPresentationIndex` 的 sorting/rarity map | 内容表现元数据，不是解锁状态 |
| lock 对象 | 并发协调资源，不是领域事实 |
| `BinaryReader` / `BinaryWriter` | 外部格式工具，不是 Component 字段 |
| 异常对象 | 控制流/诊断结果，不是持久状态 |

## 9. 当前 NLTX 组件覆盖

| 候选 Component | 当前 NLTX 覆盖 | 证据 | 结论 |
|---|---|---|---|
| `ProgressionAggregate.KillCountsByPersistentId` | absent | `ContentIdentityCatalog.cs` 只有身份映射；无 kill count 字段或权威 World unlock state | 需要新建候选字段；identity owner 未决 |
| `ProgressionAggregate.SightedPersistentIds` | absent | 未发现目标 Bestiary sight 集合 | 需要新建候选字段 |
| `ProgressionAggregate.ChattedPersistentIds` | absent | 未发现目标 Bestiary chat 集合 | 需要新建候选字段 |
| `BestiaryDiscoveryCache.PlayerBestiaryBounds` | absent | 未发现目标 Component 缓存；Version4 有 `List<Rectangle>` 证据 | 需要新建候选字段；几何类型未决 |
| `BestiaryDiscoveryCache.SeenNpcNetworkIds` | partial | `BestiaryModulePacket.NpcNetId` 和 `TerrariaPacketCodec` 具有局部网络 ID 解析；没有 World 扫描缓存 owner | 只可作为候选缓存字段，不能复用协议 DTO 代替 |
| `PersistentBestiaryId` | partial/owner unresolved | `ContentIdentityCatalog` 具有 Bestiary credit identity 映射；没有明确的领域值类型 owner | 需要跨子系统 owner 裁决 |
| `NetworkId` | partial/owner unresolved | 协议模型使用 `NpcNetId`；没有确认 NLTX 统一网络 ID 值类型 | 需要跨子系统 owner 裁决 |
| `World` attachment | partial | 已有 `WorldProgressionState`，但未覆盖 Bestiary facts | 不能把相邻状态直接扩展为目标 Component |

当前 NLTX 的协议和 WLD 代码只构成输入覆盖的局部片段：它们不能证明 `ProgressionAggregate` 或 `BestiaryDiscoveryCache` 已经存在，也不能证明 `TerrariaSession` 解码结果已经被保存为权威状态。

## 10. 组件级 evidence-gap

以下问题是组件级缺口，不在本文中用猜测补齐。总数为 10。

| ID | 组件级缺口 | 影响范围 | 当前证据与未决问题 |
|---|---|---|---|
| `EG-COMP-01` | `PersistentBestiaryId` 的 NLTX 领域类型及 owner 未确定 | 两个持久集合/字典字段 | Version4 有 string persistent ID 语义，NLTX 有内容身份映射，但没有冻结的共享值类型 |
| `EG-COMP-02` | World entity 的正式 attachment owner 未确定 | 两个 Component 的 entityScope | 已有 `WorldProgressionState`，但没有目标 Bestiary Component 的 attachment 约定 |
| `EG-COMP-03` | `ProgressionAggregate` 的三分区是否需要独立访问边界未确定 | 字段封装与 owner | Version4 聚合证据支持一个 Component，但没有 NLTX API/封装规范证据 |
| `EG-COMP-04` | `BestiaryDiscoveryCache` 是否允许 absent、何时创建未确定 | cache cardinality/lifecycle | Version4 有初始化和 reset 证据，但没有 NLTX entity lifecycle 规则 |
| `EG-COMP-05` | `Rectangle` 是否是 NLTX 可接受的跨层几何值类型未确定 | `PlayerBestiaryBounds` | Version4 使用 `Rectangle`，当前 NLTX 目标 Component 没有几何值类型映射证据 |
| `EG-COMP-06` | 网络 module `4` 的解码结果是否应进入 World Component 未确定 | `SeenNpcNetworkIds` 与事实镜像边界 | `TerrariaSession.cs:345-349` 解码后丢弃结果；Version4 `Deserialize` 为空 |
| `EG-COMP-07` | 客户端只读 Bestiary 镜像是否需要独立 Component 未确定 | authority/cardinality | Version4 完整参考有客户端写入逻辑，但 Version4 当前文件的接收行为不足以裁决 NLTX 镜像模型 |
| `EG-COMP-08` | 旧版本 Bestiary records 是否需要持久 compatibility marker 未确定 | `ProgressionAggregate` 字段集合 | `FillBasedOnVersionBefore210` 当前为空；不能据此增加兼容字段 |
| `EG-COMP-09` | `NetworkId` 的稳定类型和所有权未确定 | `SeenNpcNetworkIds` | 当前 NLTX 主要证据是 `int` payload/`NpcNetId`，尚无统一 `NetworkId` owner |
| `EG-COMP-10` | empty dictionary/set 的构造、校验和非法 ID 处理契约未确定 | defaults/invariants | Version4 有空初始化、count clamp 和 validate 证据，但 NLTX Component value validation contract 尚未提供 |

## 11. 未决组件 owner

以下 5 项 owner 决策必须在组件设计进入实现前由整合方裁决。它们不是本文的运行时实现内容。

| 决策 ID | 决策问题 | 当前建议 | owner 状态 |
|---|---|---|---|
| `BD-COMP-01` | 三类事实保持一个 `ProgressionAggregate`，还是拆成三个公开 Component | 暂选一个聚合 Component，内部保持三个独立字段分区；依据是 Version4 的共同 tracker/lifecycle | `componentOwner: WorldProgressionAndUnlocks (candidate)`；最终需 integration-review |
| `BD-COMP-02` | World 权威 Component 与客户端只读镜像的关系 | 本文只定义 World 权威候选，不新增客户端镜像 Component；等待 Version4 接收语义和 NLTX 网络 ownership 证据 | `crossSubsystemOwner: integration-review` |
| `BD-COMP-03` | `PersistentBestiaryId`、`NetworkId`、`NpcEntityId`/`EntityReference` 的共享 owner | 不在本设计中重复定义；分别保留为外部共享身份类型候选 | `crossSubsystemOwner: integration-review` |
| `BD-COMP-04` | `PlayerBestiaryBounds` 保留 `Rectangle`，还是采用 NLTX 既有几何值类型 | 暂保留 `Rectangle` 候选，不创建新的值类型 | `crossSubsystemOwner: integration-review` |
| `BD-COMP-05` | 旧版本 compatibility marker/records 是否成为 Component 字段 | 暂不增加兼容字段；兼容数据停留在输入边界，直到有字段级证据 | `componentOwner: WorldProgressionAndUnlocks (candidate)`；最终需 integration-review |

两个 Component 的 `componentOwner` 当前只能写为 `WorldProgressionAndUnlocks (candidate)`。这表示领域归属方向明确，但不表示已经完成项目级 owner 注册或代码目录落位。

## 12. 最终 Component 清单

| 序号 | `componentId` | 名称 | status | entityScope | 主要字段 | authority |
|---:|---|---|---|---|---|---|
| 1 | `WPU.COMP.ProgressionAggregate` | `ProgressionAggregate` | proposed | exactly one active World entity | `KillCountsByPersistentId`、`SightedPersistentIds`、`ChattedPersistentIds` | authoritative World-local persistent state |
| 2 | `WPU.COMP.BestiaryDiscoveryCache` | `BestiaryDiscoveryCache` | proposed | zero or one active World entity | `PlayerBestiaryBounds`、`SeenNpcNetworkIds` | transient scan/dedup cache |

清单之外不再提出 Component。特别是，派生 unlock state、完成度报告、网络 packet、WLD reader、内容身份/表现元数据和单个条目/实体都不属于本次 Component-only 设计清单。

## 13. 最终声明

本文件是 Component-only Design。
本文件不定义 System、Query、Command、Event、Adapter、Projection、
调度顺序、运行时实现、测试计划或迁移计划。

所有 status: proposed 的 Component 都只是设计提案。
本文件不声明代码已创建、已迁移、行为等价或验证通过。

本文件仅依据当前会话明确提供并读取的研究报告及其列出的 Version4、NLTX 和相关边界证据生成。由于证据状态为 `partial`，且存在 10 个组件级 evidence-gap、5 个组件 owner 决策和一个 `evidence-mismatch`，本设计的 `designStatus` 保持为 `decision-required`，`verificationStatus` 保持为 `not-run`。
