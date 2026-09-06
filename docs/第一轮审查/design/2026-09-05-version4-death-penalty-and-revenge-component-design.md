# DeathPenaltyAndRevenge Component Design

## 1. 设计元数据

```text
subsystemId: DeathPenaltyAndRevenge
taskNumber: 02
sourceReport: D:\TRbackup\NLTX\docs\research\2026-09-05-version4-death-penalty-and-revenge-public-decomposition.md
outputDesignPath: D:\TRbackup\NLTX\docs\design\2026-09-05-version4-death-penalty-and-revenge-component-design.md
designScope: component-only
designStatus: decision-required
evidenceStatus: partial
nltxStatus: missing
verificationStatus: not-run
selectionMethod: current-session-explicit-report; Version4-first; rechecked component fields and lifecycle evidence; no research-directory scan; no subagent
```

本文件只把研究报告中的状态事实收敛为组件组成、字段、所有权、生命周期、组合关系和证据状态。`designStatus` 使用 `decision-required`，因为 marker registry 的聚合 owner、respawn lock 的幂等归属、金币损失与 marker 的关系、ID 类型和持久化范围仍未裁决。

## 2. 设计范围与排除范围

### 2.1 设计范围

本设计覆盖 `DeathPenaltyAndRevenge` 的四类候选权威状态：

1. 世界范围的 marker membership/index；
2. 单个 marker 的稳定身份；
3. 单个 marker 的过期、强制过期和重建尝试锁；
4. 从 NPC 实例复制出的、供未来重建使用的不可变 target snapshot。

四类状态均是 proposed Component 设计，不表示当前 NLTX 已经存在对应实现。

### 2.2 排除范围

以下内容只作为字段来源、兼容边界或 owner 决策依据，不在本文件中设计为组件：

- 玩家死亡事实、死亡原因、PVP 标记、死亡位置和复活状态；
- 玩家金币损失计算、库存扣除、金币物品生成和经济账本提交；
- NPC 实体身份、NPC slot、NPC 类型定义、AI 状态、生命和普通 NPC 生命周期；
- 玩家 proximity 矩形、世界事件资格和 NPC discouragement 的临时计算结果；
- 网络字节、消息 126/127、存档字节、客户端地图图标、鼠标提示和日志；
- 任何运行时行为结构、调度结构、测试或迁移落地。

`Player.KillMe → DropCoins` 与 `NPC.CheckActive → CacheEnemy` 在 Version4 中是可分别确认的链，但当前证据没有确认两者之间存在直接调用或单一事务。因此本设计不把玩家金币损失字段强行并入 marker 组件。

## 3. 组件设计依据

### 3.1 证据来源和使用边界

| 来源 | 实际读取的材料 | 本设计使用方式 | 状态 |
|---|---|---|---|
| Version4 | `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:14-462` | 首要行为基线；确认 marker 字段、registry、生命周期和 target snapshot 来源 | confirmed / partial |
| Version4 | `D:\TRbackup\Version4\Terraria\NPC.cs:5897-6395,64435-64555,64565-64590,66505-66528,67045-67105` | 确认 NPC 清理、NPC 实例字段、重建入口和实体 slot 创建边界 | confirmed / partial |
| Version4 | `D:\TRbackup\Version4\Terraria\Main.cs:11448-11456` | 确认 world tick 外部时间和 marker 更新入口 | confirmed |
| Version4 | `D:\TRbackup\Version4\Terraria\Player.cs:478-521,921-1142,22572-22678,26261-26305` | 确认玩家生命周期和金币字段属于相邻 owner；不证明 marker 关系 | confirmed / evidence-gap |
| Version4 | `D:\TRbackup\Version4\Terraria\NetMessage.cs:78,1494-1510,2359-2365,2548-2563`；`D:\TRbackup\Version4\Terraria\MessageBuffer.cs:3054-3070` | 仅确认兼容字段读取和接收缺口；不把 wire data 变成权威组件 | confirmed / evidence-mismatch |
| 完整参考 | `D:\TRbackup\无任何删减通过编译\Terraria.GameContent\CoinLossRevengeSystem.cs:14-520` | 只补充同名同路径成员差异；`AddMarkerFromReader`、`DestroyMarker`、地图显示和客户端清理不回写为 Version4 当前事实 | version-drift |
| tModLoader v2026.07 | `D:\TRbackup\tmodloader-api-docs-stable\class_mod_player.html#a981918672130f6284403f1468e88fc9a`、`D:\TRbackup\tmodloader-api-docs-stable\class_mod_player.html#af5ed4f9aec0aafb9197fc93b62737264`、`D:\TRbackup\tmodloader-api-docs-stable\class_mod_player.html#aeaf390061d5840291f92c6fd929dc599`、`D:\TRbackup\tmodloader-api-docs-stable\class_mod_player.html#a7893e4b2639c4bf7d6e8872ce346fd38`；`D:\TRbackup\tmodloader-api-docs-stable\class_n_p_c.html#a99d0dc5369c4bf7d6e8872ce346fd38` | 交叉确认公开死亡 Hook、公开存档 Hook、公开变化同步 Hook 和 NPC 死亡/发包边界；不能证明私有 marker 算法 | partial |
| Space Station 14 | `C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\Mobs\Components\MobStateComponent.cs`、`C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\Mobs\Systems\MobStateSystem.cs`、`C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Server\Spawners\Components\SpawnOnDespawnComponent.cs`、`C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Server\Spawners\EntitySystems\SpawnOnDespawnSystem.cs` | 只参考组件内聚、实体组合和生命周期状态隔离；不复制其名称、代码或领域语义 | organizational reference |
| 当前 NLTX | `D:\TRbackup\NLTX\src\Combat`、`src\Player`、`src\Npc`、`src\Items`、`dome\src`、`dome\Test` 的本次定向读取文件 | 判断 existing/partial/missing 映射；不因相邻组件存在而提升 marker 子系统状态 | existing / partial / missing |

### 3.2 Version4 直接字段事实

Version4 `RevengeMarker` 的字段声明位于 `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:18-60`。构造函数在 `:80-100` 保存位置、NPC 重建参数、金币值、base value、statue 标志，按当前 `_gameTime` 和金币值计算 `_expirationTime`，并使用静态自增 `_uniqueIDCounter` 产生 ID。

`CacheEnemy` 在 `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:315-339` 过滤 boss、worm segment、rarity、金币阈值和世界边界，必要时映射 `RespawnEnemyID`，然后从 NPC 实例读取快照字段并追加 `_markers`。它的直接调用点是 `D:\TRbackup\Version4\Terraria\NPC.cs:64435-64555` 中 `CheckActive` 的非主动清理路径。

`Reset`、`Update`、`CheckRespawns`、`RemoveExpiredOrInvalidMarkers` 和 `SendAllMarkersToPlayer` 位于 `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:340-462`。这些成员证明状态的创建、时间推进、锁、失效清理、重建成功移除和 registry 清空边界，但不证明一个可跨进程恢复的 marker 存档身份。

### 3.3 补证和冲突处理

完整参考在相同 `CoinLossRevengeSystem.cs` 中额外出现 `AddMarkerFromReader`、`DestroyMarker`、`DrawMapIcon`、`UseMouseOver`，并在 `CacheEnemy` 增加专用端模式 guard，在 `Update` 增加客户端清理。这些只记录为 `version-drift`，不进入 Version4 当前组件字段。

当前 `dome` 的 `SyncRevengeMarkerPacket` 只有 `short Id`，而 Version4 `WriteSelfTo` 写入 9 个字段：`int` ID、`Vector2` 位置、`int` NPC net ID、`float` HP fraction、`int` NPC type、`int` AI style、`int` coin value、`float` base value、`bool` statue 标志。因此协议占位只能标记为 `partial` / `evidence-mismatch`，不能当作完整 marker 组件映射。

## 4. Version4 成员到 Component 归属表

下表只记录与组件字段直接相关的真实成员。方法、常量和锁对象放在表中仅用于说明字段证据，不把它们转换为新的运行时结构。

| 成员 | 声明类型 | 字段含义 | 权威/派生/缓存/快照/兼容 | 生命周期 | proposed Component | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|---|
| `_uniqueID` | `int` | marker 的 Version4 唯一 ID | 权威身份；wire 兼容值 | marker 创建或带 ID 重建时初始化，marker 移除时失效 | `RevengeMarkerIdentityComponent` | confirmed | `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:18-18,80-100,263-277` |
| `_location` | `Vector2` | marker/NPC 重建中心位置 | 不可变 target snapshot | marker 创建时复制，marker 结束时销毁 | `RevengeTargetSnapshotComponent` | confirmed | `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:36-36,80-90,186-193` |
| `_hitbox` | `Rectangle` | 由 location 和 `EnemyBoxSize` 构成的触发区域 | 派生几何，不是独立权威字段 | marker 创建时派生；location 改变时应失效，但 Version4 location 不可变 | 不单独创建 Component | confirmed | `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:34-38,90,179-185` |
| `_npcNetID` | `int` | 用于 `NPC.NewNPC` 的 net ID，可能是映射后的重建 ID | 兼容字段，属于 target snapshot；不是 NPC 实例 ID | marker 创建时从 `npc.netID` 映射，重建读取 | `RevengeTargetSnapshotComponent` | confirmed | `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:40-40,186-198,315-330` |
| `_npcHPPercent` | `float` | 缓存时 NPC 生命比例 | 快照 | marker 创建时复制，重建时读取并以至少 0.5 的规则使用 | `RevengeTargetSnapshotComponent` | partial | `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:42-42,208-209,330` |
| `_baseValue` | `float` | NPC 的 base value | 快照 | marker 创建时复制，重建时写回 NPC | `RevengeTargetSnapshotComponent` | confirmed | `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:44-44,205-207,330` |
| `_coinsValue` | `int` | 与 NPC 关联的金币值/缓存资格依据 | 快照；跨 Items/NPC 使用的兼容值 | marker 创建时复制，重建时写回 `extraValue` | `RevengeTargetSnapshotComponent` | confirmed | `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:46-46,205-206,273,319-330` |
| `_npcTypeAgainstDiscouragement` | `int` | discouragement 判断使用的 NPC type | 快照 | marker 创建时复制，资格判断时读取 | `RevengeTargetSnapshotComponent` | confirmed | `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:48-48,102-157,221-262,330` |
| `_npcAIStyleAgainstDiscouragement` | `int` | discouragement 判断使用的 AI style | 快照 | marker 创建时复制，资格判断时读取 | `RevengeTargetSnapshotComponent` | confirmed | `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:50-50,221-262,330` |
| `_expirationTime` | `int` | 根据创建时 game time 和 coin value 算出的失效 tick | 权威生命周期状态 | marker 创建时计算，时间比较或强制过期时结束 | `RevengeMarkerLifecycleComponent` | confirmed | `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:52-52,91,158-178` |
| `_spawnedFromStatue` | `bool` | NPC 是否来自 statue spawn | 快照/兼容字段 | marker 创建时复制，重建时写回 NPC | `RevengeTargetSnapshotComponent` | confirmed | `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:54-54,207,275,330` |
| `_forceExpire` | `bool` | discouragement 或其他资格路径要求立即失效 | 权威生命周期状态 | 初始 false；`SetToExpire` 后保持 true 至 marker 清理 | `RevengeMarkerLifecycleComponent` | confirmed | `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:58,66-70,158-168,411-414` |
| `_attemptedRespawn` | `bool` | 当前 proximity 触发是否已经领取重建尝试锁 | 权威生命周期状态；owner 未完全裁决 | 初始 false；靠近时设 true，离开区域时设 false | `RevengeMarkerLifecycleComponent` | confirmed / evidence-gap | `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:60-78,393-410` |
| `RespawnAttemptLocked` | `bool` | `_attemptedRespawn` 的只读派生访问 | 派生读取 | 与 `_attemptedRespawn` 同生命周期 | `RevengeMarkerLifecycleComponent` | confirmed | `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:62-64` |
| `_markers` | `List<RevengeMarker>` | 世界范围 marker 集合 | 权威 registry membership；当前是对象列表 | 构造时空列表，追加 marker，清理/成功重建/Reset 时移除 | `RevengeMarkerRegistryComponent` | confirmed | `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:295-312,330-347,416-447` |
| `_markersLock` | `object` | 对 marker 集合读写的互斥保护 | 同步机制，不是领域状态 | registry 实例创建时初始化，实例销毁时结束 | 不单独创建 Component | confirmed | `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:297,300-307,342-347,430-448` |
| `_gameTime` | `int` | marker 过期比较所用的运行时时间 | 外部 world time 输入，不属于 marker | 每 tick 推进，Reset 归零 | 不放入 marker Component | confirmed | `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:299,351-356`；`D:\TRbackup\Version4\Terraria\Main.cs:11450-11455` |
| `MinimumCoinsForCaching` | `int` | NPC 进入 marker registry 的金币阈值 | 规则/策略常量 | 世界配置或进程生命周期 | 不单独创建 Component | confirmed | `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:283-284,319` |
| `_playerBoxSizeInner` / `_playerBoxSizeOuter` | `Vector2` | 玩家 proximity 资格窗口尺寸 | 规则/派生几何参数 | 进程生命周期 | 不单独创建 Component | confirmed | `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:286-293,373-378` |
| `CacheEnemy(NPC npc)` | method | 从 NPC 实例构造 target snapshot 并写入 registry | 字段写入入口，不是字段 | NPC 非主动清理时调用 | `RevengeTargetSnapshotComponent` + `RevengeMarkerRegistryComponent` | confirmed | `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:315-339`；`D:\TRbackup\Version4\Terraria\NPC.cs:64535-64547` |
| `Reset()` | method | 清空 marker registry 并重置外部时间 | 清理入口 | 世界重置时调用 | `RevengeMarkerRegistryComponent` / lifecycle cleanup | confirmed | `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:340-350`；`D:\TRbackup\Version4\Terraria\WorldGen.cs:6437` |
| `WriteSelfTo(BinaryWriter)` | method | 写出九字段 marker 快照 | 兼容读取视图，不是组件额外状态 | 复制时读取，不改变 marker | 不单独创建 Component | confirmed | `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:263-277`；`D:\TRbackup\Version4\Terraria\NetMessage.cs:1502-1505` |
| `Player.lostCoins` / `lostCoinString` | `long` / `string` | 玩家死亡后金币数值及显示文本 | 相邻玩家/经济状态；不属于已证实 marker 状态 | 玩家死亡后写，之后由玩家生命周期使用 | 不在本设计新增 Component | confirmed for separate chain | `D:\TRbackup\Version4\Terraria\Player.cs:497-499,22572-22678` |

## 5. Component 定义

### 5.1 RevengeMarkerRegistryComponent

#### 职责

保存当前世界中 marker 实例的 membership/index。它只表达“哪些 marker 属于该世界 registry”，不复制 marker 的全部字段，也不保存玩家、NPC、网络连接或存档 writer。

```text
componentId: DPR-COMP-01
name: RevengeMarkerRegistryComponent
status: proposed
componentOwner: DeathPenaltyAndRevenge (candidate)
crossSubsystemOwner: integration-review
entityScope: one World entity / one active world registry
lifecycle: world initialization -> empty registry -> marker membership changes -> world reset cleanup
```

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| `MarkerIds` | `IReadOnlySet<RevengeMarkerId>` (proposed value shape) | empty set | 权威 registry membership | 每个 ID 在当前 registry 中最多出现一次；每个 ID 必须能解析到一个 marker entity；不存重复对象 | partial | Version4 `_markers` 是 `List<RevengeMarker>`：`D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:295-312,416-447` |

`MarkerIds` 是对 Version4 `_markers` 的组件化状态表达，不声称当前 NLTX 已有集合类型。若整合裁决要求按玩家或 section 建立反向索引，该索引不能未经裁决直接并入本组件，见 `BD-COMP-01`。

#### 字段不变量

- registry 只包含尚未结算、未过期且未被 Reset 清理的 marker membership；清理操作不得留下孤立 ID。
- marker 的身份、生命周期和 target snapshot 位于 marker entity，而不是复制在 world registry 中。
- 一个 active world 只允许一个该 registry 组合；世界切换必须先清理旧 membership。
- registry 不拥有 `_gameTime`、玩家 proximity 矩形、NPC slot、网络连接或持久化文件句柄。

#### 生命周期

- 创建：世界初始化时创建空 membership；Version4 构造函数把 `_markers` 初始化为空列表。
- 初始化：marker 注册被接受后加入一个 ID；当前 Version4 的追加入口是 `AddMarker`。
- 更新：membership 只随 marker 创建、过期/无效清理、成功重建移除或世界重置改变。
- 清理：`Reset` 清空集合；当前 Version4 没有从世界/玩家存档恢复该集合的证据。

#### Entity/World 范围

World 范围。它不是每个 marker entity 上的重复列表，也不是 NPC entity 的组件。

#### ID 与关系字段

`MarkerIds` 只引用 `RevengeMarkerId`。`RevengeMarkerId` 与 `EntityUuid`、`PersistentEntityId`、`NpcInstanceId`、`NpcSlot`、`NetworkId`、`PlayerHandle` 不能互换。`RevengeMarkerId` 的跨边界 owner 标记为 `crossSubsystemOwner: integration-review`。

#### 当前 NLTX 映射

`status: missing`。当前 NLTX 没有 DeathPenaltyAndRevenge world registry；`src/Npc/NpcEntityIdentityComponent.cs` 的现有 NPC identity 不能代替 marker membership。

#### 证据

主要证据是 `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:295-312` 的 `_markers` 与构造函数、`:330-347` 的追加/Reset、`:416-447` 的移除。集合是 Version4 事实，但把对象列表表达为 entity ID set 是 `status: proposed` 的组件形状。

### 5.2 RevengeMarkerIdentityComponent

#### 职责

保存 marker 在当前运行时 registry 和兼容消息中的稳定身份。它不把 marker ID 当作 NPC 实例、玩家 slot、持久化主键或网络连接 ID。

```text
componentId: DPR-COMP-02
name: RevengeMarkerIdentityComponent
status: proposed
componentOwner: DeathPenaltyAndRevenge (candidate)
crossSubsystemOwner: integration-review
entityScope: one marker entity
lifecycle: unassigned construction -> assigned on marker registration -> valid while registry membership exists -> invalid after settlement/cleanup
```

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| `MarkerId` | `RevengeMarkerId` (proposed wrapper over `int`) | `-1` means unassigned; assigned IDs start at `0` in the observed process | 权威身份；兼容字段的 typed form | assigned ID 必须非负，并在一个 active registry 内唯一；不能与 `NpcSlot` 或 `NetworkId` 复用 | confirmed for legacy value / proposed for wrapper | `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:18,56,80-100,263-268` |

#### 字段不变量

- `MarkerId` 一旦分配，在该 marker entity 的生命周期内不可变。
- Version4 的静态 `_uniqueIDCounter` 只证明进程范围的生成方式，不证明跨世界、跨重启或跨存档稳定。
- 网络 126/127 兼容使用的旧 `int` 值是该字段的 compatibility representation，不是新的全局实体身份。

#### 生命周期

- 创建：构造 marker 时传入 `uniqueID = -1` 表示生成新 ID，或传入已有 ID 进行兼容重建。
- 初始化：注册进入 registry 后必须与唯一 marker entity 一一对应。
- 更新：身份字段不随过期、lock 或 target snapshot 变化。
- 清理：marker 从 registry 移除后，ID 不再代表 active marker；是否进入持久化 tombstone 尚未裁决。

#### Entity/World 范围

单个 marker entity。world registry 只持有它的引用/索引，不复制字段。

#### ID 与关系字段

`MarkerId` 是运行时 marker ID。它不是 `EntityUuid`、`PersistentEntityId`、`NpcInstanceId`、`NpcNetId`、`NpcSlot` 或 `PlayerHandle`。跨网络/持久化/整合边界的最终 ID owner 为 `crossSubsystemOwner: integration-review`。

#### 当前 NLTX 映射

`status: missing`。当前 `src` 中没有 `RevengeMarkerId` 或 marker identity component。`src/Npc/NpcInstanceId.cs` 只覆盖 NPC 实例 ID，不能替代此字段。

#### 证据

`D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:18-18` 声明 `_uniqueIDCounter = 0`，`:56-64` 声明 ID 和读取属性，`:80-100` 分配 ID，`:263-268` 写入 ID。持久化稳定性是 `evidence-gap`。

### 5.3 RevengeMarkerLifecycleComponent

#### 职责

保存 marker 自身的失效边界和一次重建尝试锁。它不保存外部 world clock，也不把 NPC spawn 结果、网络状态或经济事务结果塞入 marker。

```text
componentId: DPR-COMP-03
name: RevengeMarkerLifecycleComponent
status: proposed
componentOwner: DeathPenaltyAndRevenge (candidate)
crossSubsystemOwner: integration-review
entityScope: one marker entity
lifecycle: initialized at marker creation -> time/qualification reads -> forced expiry or attempt lock mutation -> cleanup on expiry/invalidity/success/reset
```

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| `ExpiresAtGameTime` | `int` | constructor-calculated from creation `gameTime` and `CoinValue`; no independent empty-state default | 权威生命周期状态 | marker 在 `currentGameTime >= ExpiresAtGameTime` 时可过期；不能把网络快照的缺省值当作有效时间 | confirmed | `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:52,91,158-178` |
| `ForceExpire` | `bool` | `false` | 权威生命周期状态 | 一旦为 true，`IsExpired` 立即返回 true；不得被普通时间比较重置 | confirmed | `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:58,66-70,158-168` |
| `RespawnAttemptLocked` | `bool` | `false` | 权威生命周期状态；attempt owner unresolved | 同一个 marker 在同一 proximity 触发窗口内不能重复领取未完成尝试；离开窗口后 Version4 会释放该 bool | confirmed for bool / partial for idempotency | `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:60-78,393-410` |

#### 字段不变量

- `ExpiresAtGameTime` 由 marker 创建时的外部 game time 和 `CoinValue` 共同确定；marker 不自行拥有时钟。
- `ForceExpire == true` 是单向失效事实，直到 marker 被清理；不得解释为可重试 lock。
- `RespawnAttemptLocked == true` 只能阻止重复领取 observed attempt；Version4 没有 attempt token、owner handle、revision 或显式成功/失败结果，因此幂等边界仍是 `evidence-gap`。
- 生命周期组件不能在没有 target snapshot 的情况下表示可重建 marker。

#### 生命周期

- 创建：marker 构造时计算 `ExpiresAtGameTime`，两个 bool 为 false。
- 初始化：marker 进入 registry 后，生命周期字段与 identity/target snapshot 共同有效。
- 更新：读取外部 game time 判断到期；资格路径可以设置 `ForceExpire`；proximity 路径可以设置或释放 `RespawnAttemptLocked`。
- 清理：过期、世界事件无效、discouragement、成功重建和 `Reset` 都会结束 marker 生命周期。失败后是否释放 lock、保留 lock 或写 attempt token，必须由 `BD-COMP-02` 裁决。

#### Entity/World 范围

单个 marker entity。外部 world time 和玩家 proximity 状态不附着到 marker 上。

#### ID 与关系字段

本组件不拥有 ID。潜在的 attempt token 如果最终需要，是一次事务关系值而不是默认新增的独立 Component；owner 标记 `crossSubsystemOwner: integration-review`。

#### 当前 NLTX 映射

`status: missing`。`src/Player/PlayerLifecycleState.cs` 和 `src/Npc/NpcLifetimeComponent.cs` 具有相邻生命周期字段，但它们分别属于玩家/NPC entity，不能覆盖 marker 的过期和尝试锁。

#### 证据

Version4 `IsExpired`、`SetToExpire`、`SetRespawnAttemptLock` 和 `CheckRespawns` 位于 `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:66-78,158-168,358-427`。当前文件没有明确 settlement phase 或失败回滚字段，这部分保持 unresolved。

### 5.4 RevengeTargetSnapshotComponent

#### 职责

保存从 NPC 实例复制出的最小重建资料，使 marker 不依赖已经 inactive、被清理或 slot 已复用的 NPC entity。它只保存事实快照和旧协议所需的兼容字段，不保存原 NPC entity 引用。

```text
componentId: DPR-COMP-04
name: RevengeTargetSnapshotComponent
status: proposed
componentOwner: DeathPenaltyAndRevenge (candidate)
crossSubsystemOwner: integration-review
entityScope: one marker entity
lifecycle: captured from eligible NPC -> immutable while marker is active -> read for recreation/compatibility -> discarded with marker
```

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| `Location` | `WorldPosition` (proposed shared value) | constructor input; zero is not an asserted valid default | 不可变快照 | marker 位置在 active 生命周期内不变；坐标类型 owner 为 `crossSubsystemOwner: integration-review` | confirmed for source value / partial for shared type | `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:36,80-90,186-193`；当前 `D:\TRbackup\NLTX\src\Items\WorldPosition.cs` 与 `D:\TRbackup\NLTX\src\Player\PlayerValueTypes.cs` 存在同名坐标值类型 |
| `NpcNetId` | `Terraria.Npc.NpcNetId` | constructor input; zero is rejected by the observed cache path | 兼容快照字段 | 允许负值表示变体；不是 `NpcInstanceId`、`NpcSlot` 或新 NPC entity ID | confirmed for legacy semantics / proposed typed mapping | `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:40,315-330`；`D:\TRbackup\NLTX\src\Npc\NpcNetId.cs` |
| `NpcTypeId` | `Terraria.Npc.NpcTypeId` | constructor input; no empty marker target | 快照字段 | 保留原 NPC type 供 discouragement 判断；不得用映射后的 net ID 代替原 type | confirmed for source value / proposed typed mapping | `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:48,102-157,330`；`D:\TRbackup\NLTX\src\Npc\NpcTypeId.cs` |
| `LegacyAiStyle` | `int` | constructor input | 快照字段 | 只用于保留 observed discouragement 分类；不等同于完整 AI 状态或四个 AI slot | confirmed | `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:50,221-262,330` |
| `LifeFraction` | `float` | constructor input from `NPC.GetLifePercent()` | 派生后固化的快照 | 候选值域为 `0..1`；源方法的全部边界语义尚未独立确认，异常/NaN 处理不能猜测 | partial | `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:42,208-209,330` |
| `CoinValue` | `int` | constructor input from `NPC.extraValue` | 快照字段；同时是缓存资格输入 | 被接受的 cache path 必须满足 `CoinValue >= MinimumCoinsForCaching`；不等同于玩家 `lostCoins` 或经济 balance | confirmed | `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:46,283-284,319-330`；`D:\TRbackup\Version4\Terraria\NPC.cs:6383-6385` |
| `BaseValue` | `float` | constructor input from `NPC.value` | 快照字段 | 重建时只能写回 target NPC 的 value 语义；源值的完整下限和精度契约未扩展推断 | confirmed for source value / partial for domain invariant | `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:44,205-207,330`；`D:\TRbackup\Version4\Terraria\NPC.cs:6383-6385` |
| `SpawnedFromStatue` | `bool` | constructor input from source NPC | 兼容快照字段 | 重建时保持 source flag；不能把它当作 marker 来源或玩家死亡标志 | confirmed | `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:54,207,275,330`；`D:\TRbackup\Version4\Terraria\NPC.cs:5949` |

#### 字段不变量

- 所有字段在 marker 创建时从 NPC 实例复制；marker active 后不得再读取原 NPC entity 的可变字段来补全 snapshot。
- `NpcNetId` 是 legacy spawn compatibility value，`NpcTypeId` 是 discouragement 参照 value；两者必须保持分离。
- `CoinValue` 属于 NPC target snapshot。它不能因为名称包含 coin 就自动连接 `Player.lostCoins`、`DropCoins` 或 `CurrencyBalanceComponent`。
- `Location` 用于 marker 几何位置和重建位置；触发盒是由 location 与固定尺寸派生的临时几何，不作为 snapshot 的第二份可变状态。
- snapshot 不含 `NpcInstanceId`、`NpcSlot`、`EntityUuid` 或网络连接 ID；重建出的 NPC 是新的实体实例。

#### 生命周期

- 创建：`CacheEnemy` 在通过 boss、segment、rarity、金币和世界边界过滤后，从 NPC 当前字段一次性取值。
- 初始化：`Location`、NPC 兼容字段、生命比例、金币/base value、statue flag 和 identity 一起形成 marker entity。
- 更新：snapshot 字段不可变；只允许被读取用于资格参照、重建输入或兼容快照。
- 清理：marker 过期、无效、重建成功或世界 Reset 后，snapshot 与 marker entity 一起结束。

#### Entity/World 范围

单个 marker entity。源 NPC entity 只提供一次快照输入；world registry 只保存 membership/index。

#### ID 与关系字段

- `NpcNetId`：legacy network/variant compatibility field，owner `crossSubsystemOwner: integration-review`。
- `NpcTypeId`：NPC catalog/type reference，最终与 NPC definition owner 的边界需整合裁决。
- 不引用 `NpcInstanceId` 或 `NpcSlot`；当前 NLTX 的 `NpcEntityIdentityComponent` 和 `NpcDefinitionReferenceComponent` 只属于 NPC entity。
- `Location` 的共享坐标类型和 `WorldPosition` 重复定义问题，见 `BD-COMP-04`。

#### 当前 NLTX 映射

目标组件 `status: proposed`，当前覆盖为 `partial`：

- `D:\TRbackup\NLTX\src\Npc\NpcDefinitionReferenceComponent.cs` 已有 `NpcTypeId`、`NpcNetId` 和 catalog revision；它描述活 NPC definition reference，不是 marker snapshot。
- `D:\TRbackup\NLTX\src\Npc\NpcBehaviorStateComponent.cs` 已有 `LegacyAiStyle` 和 AI slots；marker 只需 cache 时的 AI style，不应复制完整行为状态。
- `D:\TRbackup\NLTX\src\Npc\NpcHealthComponent.cs` 能表达当前生命，但没有 marker 的固化 `LifeFraction`。
- `D:\TRbackup\NLTX\src\Items\WorldPosition.cs` 能表达坐标，但和 Player 命名空间中同名值类型的 owner 尚未统一。
- 以上既有组件均不提供不可变的 marker target snapshot 和 registry 关系。

#### 证据

`D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:315-339` 明确以 `npc.Center`、映射后的 net ID、`GetLifePercent()`、`npc.type`、`npc.aiStyle`、`npc.extraValue`、`npc.value` 和 `npc.SpawnedFromStatue` 构造 marker。`:186-220` 明确从这些字段恢复 NPC；`:263-277` 明确其中九项被写入 wire snapshot。

## 6. Entity 与 Component 组合

| Entity/World 对象 | 必需 Component | 可选 Component | 互斥 Component | 组合理由 |
|---|---|---|---|---|
| active World entity | `RevengeMarkerRegistryComponent` | future index only after `BD-COMP-01` | 第二个同域 registry | membership 是 world 范围；不能让每个 marker 保存全局列表 |
| marker entity | `RevengeMarkerIdentityComponent`、`RevengeMarkerLifecycleComponent`、`RevengeTargetSnapshotComponent` | 无默认可选项；持久化字段只有在 `BD-COMP-05` 后候选加入 | 直接引用原 NPC entity 的替代 snapshot；把客户端显示状态作为权威组件 | 三者共同描述一个 active marker，但 identity、lifecycle、target snapshot 的生命周期和访问字段子集不同 |
| active NPC entity | 当前已有 `NpcEntityIdentityComponent`、`NpcDefinitionReferenceComponent`、`NpcBehaviorStateComponent`、`NpcHealthComponent`、`NpcLifetimeComponent` | marker target snapshot 不附着到正常 NPC；只在 cache 边界复制 | 把 marker identity/lifecycle 直接并入 NPC entity | NPC entity 拥有当前实例和 slot；marker 保存的是可独立存活的快照 |
| active Player entity | 当前已有 `PlayerIdentityState`、`PlayerLifecycleState` 及相邻生命/库存状态 | 无 DeathPenaltyAndRevenge 专属组件；proximity 资料是临时输入 | 把 `RespawnAttemptLocked` 放到 Player lifecycle | marker lock 的 observed 读写者围绕 marker 实例，不能根据玩家数量复制 lock |
| client-side marker view | 当前没有 NLTX authoritative Component | future client-only view after protocol decision | 把客户端收到的 packet 当作 World registry | 客户端数据只表示视图；当前 126 payload 与 Version4 快照不匹配，不能宣布组合完成 |

不得从该表推导运行时调度顺序。表格只声明实体范围、组件共存条件和所有权边界。

## 7. 组件拆分与合并决策

### 7.1 必须拆开的成员

- `_markers` 与 `RevengeMarker` 字段必须拆开：一个是 World membership，一个是单个 marker 的实例状态；合并会把全局索引、NPC snapshot 和生命周期做成巨型组件。
- `_uniqueID` 必须与 target snapshot 拆开：网络/日志/索引可能只读 identity，不需要 NPC 类型、金币或位置。
- target snapshot 必须与 lifecycle 拆开：snapshot 在 marker active 期间不可变，过期/lock 会变化；两者失效原因和访问字段不同。
- lifecycle 必须与 Player/NPC lifecycle 拆开：marker 可能在原 NPC inactive 后继续存活，玩家可以死亡或断线而不直接拥有 marker 状态。
- marker snapshot 必须与 wire/persistence/client view 拆开：外部格式是兼容或视图字段，不应成为权威模拟状态。

### 7.2 可以保留在同一 Component 的成员

- `_location`、`_npcNetID`、`_npcHPPercent`、`_npcTypeAgainstDiscouragement`、`_npcAIStyleAgainstDiscouragement`、`_coinsValue`、`_baseValue` 和 `_spawnedFromStatue` 共同构成一次 NPC target snapshot：它们在 Version4 构造时共同捕获、在重建时共同读取，并在 wire 快照中共同出现。
- `_expirationTime`、`_forceExpire` 和 `_attemptedRespawn` 共同属于 marker lifecycle：它们都决定 marker 是否仍可被处理，但独立的 attempt owner/token 尚未被证实。

### 7.3 不新增组件的理由

- proximity 矩形、`_hitbox`、阈值和事件资格是可由快照与外部 world/player 输入得到的派生或策略数据；把每个计算结果缓存成组件会引入失效同步。
- 玩家金币损失不是 marker snapshot 的字段。当前直接证据只到 `Player.KillMe → DropCoins → lostCoins`，没有 `Player.KillMe → RevengeManager`。
- NPC 实例 identity、slot、behavior、health 和 lifetime 已在 NLTX 具有相邻组件；marker 不应复制一个完整 NPC entity。

## 8. 不单独创建 Component 的对象

| 对象 | 处理 | 原因与证据 |
|---|---|---|
| 单个 `RevengeMarker` | 表达为一个 marker entity 加三类 marker Component | 它是 Version4 的实例对象；identity、lifecycle、snapshot 访问模式不同，不能把对象本身当作一级 Component |
| 单个 NPC 缓存条目 | 作为 `RevengeTargetSnapshotComponent` 的一次实例 | 它没有独立于 marker 的 registry、过期或结算生命周期；来源是 `CacheEnemy` 的一次复制 |
| 单个金币掉落 | 保持 Items/经济领域的物品或结果值 | `DropCoins` 产生玩家死亡金币结果；它没有被证明是 marker 创建事务 |
| 单个 respawn lock | 保持在 `RevengeMarkerLifecycleComponent` | observed lock 只有 marker 级 bool；单独组件会制造 marker 与 lock 的隐式同步，token 归属仍待 `BD-COMP-02` |
| 单个死亡 Hook | 保持 Player/Combat 边界输入 | tModLoader `PreKill` 只证明公开死亡拦截点，不证明 marker owner 或金币事务 |
| 单个网络消息 | 保持兼容快照/外部格式值 | 126/127 是传输兼容边界；Version4 126 写九字段，当前 dome 126 只有两字节 |
| 单个 UI marker | 保持客户端视图数据 | 完整参考才有 `DrawMapIcon`/`UseMouseOver`，Version4 当前文件没有这些成员；不能把显示数据变成权威 marker state |
| 单个 NPC 类型 | 保持 NPC definition/catalog 值 | `NpcTypeId`、`NpcNetId` 只是 snapshot 中的兼容/定义引用，不拥有 marker lifecycle |
| 单个 `ItemDropRule` | 保持 Items/NPC loot 规则值 | 它是掉落策略或规则，不表达 marker identity、membership、expiration 或 target snapshot |
| 玩家 proximity 矩形 | 不创建组件 | Version4 的 inner/outer box 是固定几何参数；`Intersects` 实际读取 outer box 与 marker hitbox，结果是临时资格计算 |
| marker hitbox | 不创建组件字段 | `_hitbox` 由 location 和 `EnemyBoxSize` 派生；没有独立写者或独立恢复语义 |
| marker serialization bytes | 不创建组件 | `WriteSelfTo` 只读取九项快照字段；expiration、lock 和 force-expire 不在该序列中 |

## 9. 当前 NLTX 组件覆盖

下表标记的是当前实际源码中的相邻组件状态，不是对目标四个 Component 的实现声明。

| 当前 NLTX 组件/值 | status | 已覆盖内容 | 对 DeathPenaltyAndRevenge 的缺口 |
|---|---|---|---|
| `D:\TRbackup\NLTX\src\Combat\DeathResolution.cs` / `DeathCause.cs` | existing | `IsDead`、伤害来源和死亡原因值 | 没有 marker 注册、金币损失结果、target snapshot 或 marker lifecycle |
| `D:\TRbackup\NLTX\src\Player\PlayerLifecycleState.cs` | existing | 玩家 dead/respawning、PVP/PVE death count、死亡位置和 tick | 不拥有 marker 的锁、过期或 registry membership |
| `D:\TRbackup\NLTX\src\Player\PlayerIdentityState.cs` | existing | player difficulty、连接状态和 legacy slot | 不提供 marker owner 或 proximity snapshot |
| `D:\TRbackup\NLTX\src\Npc\NpcEntityIdentityComponent.cs` | existing | `NpcInstanceId` 和 `NpcSlot` | 不提供 `RevengeMarkerId`；NPC 实例 ID 不能作为 marker ID |
| `D:\TRbackup\NLTX\src\Npc\NpcDefinitionReferenceComponent.cs` | existing | `NpcTypeId`、`NpcNetId`、initial type 和 catalog revision | 只描述活 NPC definition；没有 marker target snapshot 生命周期 |
| `D:\TRbackup\NLTX\src\Npc\NpcBehaviorStateComponent.cs` | existing | legacy AI style 和四个 authoritative AI slots | marker 只需缓存 AI style，不应把完整行为状态复制进 marker |
| `D:\TRbackup\NLTX\src\Npc\NpcHealthComponent.cs` | existing | current/max life 与 dead 派生值 | 没有固化的 marker `LifeFraction` |
| `D:\TRbackup\NLTX\src\Npc\NpcLifetimeComponent.cs` | existing | NPC remaining ticks、population cost、despawn reason | 生命周期属于 NPC entity，不等于 marker expiration/attempt lock |
| `D:\TRbackup\NLTX\src\Items\Commerce\CurrencyBalanceComponent.cs` | existing | account ID、货币 balance、revision、last transaction ID | 没有 Version4 `Player.DropCoins` 的死亡掉落事务，也不能证明 marker 关系 |
| `D:\TRbackup\NLTX\src\Items\Commerce\CommerceLedgerComponent.cs` | existing | commerce entries、sequence、unknown outcome | 没有 marker identity、target snapshot 或重建结算字段 |
| `D:\TRbackup\NLTX\src\Items\InventoryComponent.cs` | existing | inventory/coin slot 引用和 revision | 不等于 Version4 `lostCoins`，也不是 marker registry |
| `D:\TRbackup\NLTX\src\Items\WorldItemComponent.cs` | existing | world position、velocity、spawn/despawn tick、revision | 可承载普通 world item，但不承载 NPC revenge marker |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Protocol.V1456\Packets\SyncRevengeMarkerPacket.cs` | partial | message 126 的 `short Id` 编解码占位 | Version4 126 是九字段 snapshot；没有 marker client store |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Protocol.V1456\Packets\RemoveRevengeMarkerPacket.cs` | partial | message 127 的 `int UniqueId` 编解码占位 | 没有与 authoritative registry 或客户端 marker view 的组合 |

结论：当前 NLTX 对四个目标组件均为 `missing`；相邻组件覆盖只能标记 `existing`，不能提升目标子系统的 `nltxStatus`。

## 10. 组件级 evidence-gap

| gapId | 影响组件 | 未确认事实 | 当前处理 | designStatus |
|---|---|---|---|---|
| EG-COMP-01 | `RevengeMarkerIdentityComponent`、`RevengeMarkerRegistryComponent` | `_uniqueIDCounter` 是否需要跨 world/process/save 稳定；marker 是否有持久化主键 | 保留 legacy `int` 兼容值；不声明 `PersistentEntityId` 归属 | decision-required |
| EG-COMP-02 | `RevengeMarkerLifecycleComponent` | `_attemptedRespawn` 是 marker-local gate、玩家 owner 状态还是事务 token；失败是否释放或保留 | 当前只保留 observed bool；不补造 token、owner 或 revision 字段 | decision-required |
| EG-COMP-03 | `RevengeTargetSnapshotComponent` | `NPC.GetLifePercent()` 的所有值域、NaN/异常处理及重建时 min-0.5 规则的最终 typed contract | 字段标为 `partial`；不锁定超出源码直接事实的默认值 | candidate |
| EG-COMP-04 | `RevengeTargetSnapshotComponent` | `WorldPosition` 的共享类型 owner；NLTX `src/Items` 与 `src/Player` 各有同名值类型 | 暂用 proposed shared value 名称，owner 标 `crossSubsystemOwner: integration-review` | decision-required |
| EG-COMP-05 | `RevengeMarkerRegistryComponent` | registry 是单一 World aggregate、按 player 的索引，还是需要双向索引；Version4 只证明 `_markers` 列表 | 只设计 World membership set；反向索引不进入最终清单 | decision-required |
| EG-COMP-06 | 全部 marker components | Version4 `WorldFile`/`PlayerFileData` 没有 marker 字段或恢复调用；完整参考的 wire snapshot 也不含 lifecycle 字段 | persistence identity、恢复边界和默认值保持 unresolved | decision-required |
| EG-COMP-07 | `RevengeMarkerIdentityComponent`、`RevengeTargetSnapshotComponent` | Version4 `MessageBuffer` case 126/127 没有客户端 marker store；完整参考的补充接收语义不足以证明当前行为 | 现有 dome packet 只标 `partial` / `evidence-mismatch` | decision-required |
| EG-COMP-08 | target snapshot / registry | `NPC.checkDead` 与 marker cache 的关系没有直接调用证据；当前直接入口是 `NPC.CheckActive` | 不把普通 NPC death 或玩家金币 loss 自动并入 marker 创建 | decision-required |

这些 gap 不是“没有搜到就算没有”的推断：已重新读取 Version4 对应文件、完整参考同路径文件、公开 API 页面和当前 NLTX 相邻文件；仍缺失的部分明确保留为 unresolved。

## 11. 未决组件 owner

### BD-COMP-01：World registry 聚合 owner

- 冲突字段：`RevengeMarkerRegistryComponent.MarkerIds` 及其潜在按玩家/section 反向索引。
- 当前候选 owner：`DeathPenaltyAndRevenge` world entity；候选替代是 Network/Section 或 Player 侧维护索引。
- 组成影响：采用 World aggregate 时只保留一个 `MarkerIds` set；采用按玩家索引时会新增索引组件或关系值，并引入 membership 双写风险。
- 不能裁决原因：Version4 只直接证明 `_markers: List<RevengeMarker>`，没有按玩家或 section 的权威索引。

### BD-COMP-02：respawn attempt lock owner

- 冲突字段：`RespawnAttemptLocked`，以及是否需要 attempt token、owner handle、revision。
- 当前候选 owner：marker lifecycle；候选替代是玩家生命周期或跨域事务记录。
- 组成影响：marker-local 方案维持一个 bool；事务方案可能增加独立 transient relation/value；玩家方案会按玩家复制状态并改变多玩家竞争语义。
- 不能裁决原因：Version4 只提供 `_attemptedRespawn` 和进入/离开 proximity 时的 bool 写入，没有失败回滚、并发 owner 或重试幂等证据。

### BD-COMP-03：玩家死亡/金币结果 owner

- 冲突字段：`Player.lostCoins`、`DropCoins()` 返回值、`DeathResolution`、`CurrencyBalanceComponent` 的 balance/revision 与 marker 的 `CoinValue`。
- 当前候选 owner：PlayerGameplay、ItemContainerAndEconomy、CombatAndStatus；DeathPenaltyAndRevenge 只能消费已确认的不可变输入，不能单方面拥有这些相邻字段。
- 组成影响：若证实金币损失创建 marker，可能需要 transient death/coin context；若保持 Version4 当前直接证据，则 marker 只接受 NPC `extraValue` snapshot，不增加死亡组件。
- 不能裁决原因：当前 Version4 没有 `Player.KillMe → RevengeManager` 证据；把名称“CoinLoss”当成调用关系会越过证据边界。

### BD-COMP-04：共享 ID 与坐标类型 owner

- 冲突字段：`RevengeMarkerId`、`WorldPosition`、`NpcNetId`、`NpcTypeId` 与 `NpcInstanceId`/`NpcSlot` 的边界。
- 当前候选 owner：DeathPenaltyAndRevenge 维护 marker ID 和 snapshot，NPC 维护 NPC identity/definition，整合层维护跨域 value types。
- 组成影响：若共用现有 `NpcNetId`/`WorldPosition`，可减少新值类型但保留旧命名冲突；若建立独立 typed values，需要明确 namespace、转换和 wire compatibility。
- 不能裁决原因：当前 NLTX 已有同名 `WorldPosition` 定义，且 Version4 的 marker ID 只有进程范围语义。

### BD-COMP-05：marker 持久化身份 owner

- 冲突字段：`MarkerId`、`ExpiresAtGameTime`、registry membership 和可能的 `PersistentEntityId`。
- 当前候选 owner：WorldStorage/PersistenceAndRecovery；候选替代是运行时 marker registry 只在 world session 内存在。
- 组成影响：选择持久化会在 identity/lifecycle/registry 组合上增加存档版本和恢复字段；选择运行时-only 则保留四个核心组件，不添加持久化标识。
- 不能裁决原因：`WorldFile.SaveWorld_Version2`/`SaveNPCs` 与 `PlayerFileData` 的当前证据没有 marker 字段或恢复调用，不能猜测重启语义。

## 12. 最终 Component 清单

以下是当前可以提交架构评审的最小候选清单；四项全部是 proposed，并不代表已创建。

| componentId | Component | status | entityScope | 核心字段 | owner 状态 |
|---|---|---|---|---|---|
| DPR-COMP-01 | `RevengeMarkerRegistryComponent` | proposed | World entity | `MarkerIds` | component owner candidate；跨域索引 `crossSubsystemOwner: integration-review` |
| DPR-COMP-02 | `RevengeMarkerIdentityComponent` | proposed | marker entity | `MarkerId` | component owner candidate；ID 语义 `crossSubsystemOwner: integration-review` |
| DPR-COMP-03 | `RevengeMarkerLifecycleComponent` | proposed | marker entity | `ExpiresAtGameTime`、`ForceExpire`、`RespawnAttemptLocked` | component owner candidate；lock/幂等 `crossSubsystemOwner: integration-review` |
| DPR-COMP-04 | `RevengeTargetSnapshotComponent` | proposed | marker entity | `Location`、`NpcNetId`、`NpcTypeId`、`LegacyAiStyle`、`LifeFraction`、`CoinValue`、`BaseValue`、`SpawnedFromStatue` | component owner candidate；NPC/坐标/经济边界 `crossSubsystemOwner: integration-review` |

明确不列入最终清单的候选：玩家死亡输入、金币损失结果、proximity/eligibility 计算值、attempt token、网络/存档/client view。它们或者是跨域 transient value，或者缺少直接 Version4 证据，或者属于外部表示；在 `BD-COMP-02` 至 `BD-COMP-05` 裁决前不新增组件。

## 13. 最终声明

本文件是 Component-only Design。
本文件不定义 System、Query、Command、Event、Adapter、Projection、
调度顺序、运行时实现、测试计划或迁移计划。

所有 `status: proposed` 的 Component 都只是设计提案。
本文件不声明代码已创建、已迁移、行为等价或验证通过。

当前文件的 `verificationStatus: not-run` 是有意保留的状态：本会话只进行了文档、源码和结构的只读证据核对，没有运行构建或测试，也没有创建 `.cs`、`.csproj` 或测试文件。
