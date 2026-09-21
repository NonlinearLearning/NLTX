# NpcAndTownSimulation Component Design

## 1. 设计元数据

| 项目 | 值 |
|---|---|
| `subsystemId` | `NpcAndTownSimulation` |
| `taskNumber` | `12` |
| `sourceReport` | `D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-05-version4-npc-and-town-simulation-public-decomposition.md` |
| `outputDesignPath` | `D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\design\2026-09-05-version4-npc-and-town-simulation-component-design.md` |
| `designScope` | `component-only` |
| `designStatus` | `decision-required` |
| `evidenceStatus` | `partial` |
| `nltxStatus` | `partial` |
| `verificationStatus` | `not-run` |
| `selectionMethod` | 按权威状态、共同生命周期、共同不变量和访问字段子集分组；不按旧类的字段数量机械切分 |
| `generatedDate` | `2026-09-06` |

本文件只整理当前会话研究报告中的 Component 候选。组件数量为 12 个，其中设计状态统计为 `proposed: 7`、`partial: 5`。`partial` 表示当前 NLTX 有可复用或可映射的局部类型，但字段所有权、边界或证据尚未闭合；它不表示 Version4 行为已经迁移。

## 2. 设计范围与排除范围

本设计覆盖 NPC 个体的身份区分、旧槽位兼容、定义引用、公开 AI 状态、本地 AI 状态、目标关系、个体生命周期、父子关系、生命值、城镇居民资格、居民住房关系，以及城镇住房的世界级索引。

设计的最小单位是一个明确状态范围内的 Component。NPC 个体状态只附着于 NPC Entity；住房索引只附着于 World/Town 范围，不复制到每一个 NPC。组件字段必须明确区分权威状态、派生值、缓存、快照和兼容字段。

本设计不把网络传输值、存档字节、日志、时钟、随机源、客户端表现、每客户端脏状态或外部会话身份混入 NPC 权威组件。只在字段证据中简短引用 Version4 成员、持久化字段和公开 API 成员；不把这些边界扩展成新的设计对象。

以下内容保持未裁决：稳定 NPC 实例身份的最终来源、实例网络 ID、住房按 type 还是按 instance 关联、NPC 生命与通用 Combat 生命的最终 owner，以及死亡/父子清理相关字段的跨域写入归属。上述事项会改变组件组成，因此分别在第 10、11 节记录。

## 3. 组件设计依据

### 3.1 证据来源和可信度

| 来源 | 实际证据 | 用途 | 状态 |
|---|---|---|---|
| Version4 | `D:\TRbackup\Version4\Terraria\Entity.cs:6-24`；`Terraria\NPC.cs:5897-65430、67051-67143、76711-76867`；`Terraria\Main.cs:11450-11526、13645-14075`；`Terraria.GameContent\TownRoomManager.cs:9-177`；`Terraria\WorldGen.cs:4558-5770`；`Terraria.IO\WorldFile.cs:1747-1953、2951-3030、3476-3488`；`Terraria\NetMessage.cs:680-730、1705-1735、2476-2594`；`Terraria\MessageBuffer.cs:1182-1210、2087-2123` | 真实字段、住房 key、生命周期入口、存取字段和网络边界 | `confirmed` / `partial` |
| 完整可编译参考 | `D:\TRbackup\无任何删减通过编译\Terraria\NPC.cs`；`Terraria.GameContent\TownRoomManager.cs` | 仅补证 Version4 已存在文件中相同成员的明确缺口 | `partial` |
| tModLoader API | `D:\TRbackup\tmodloader-api-docs-stable\index.html`，`tModLoader v2026.07`；`class_n_p_c.html` 与 `class_mod_n_p_c.html`、`class_global_n_p_c.html` 的实际成员锚点 | 交叉核对公开字段语义、生命周期和额外 AI 边界；不替代 Version4 私有事实 | `partial` |
| Space Station 14 | `C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\Mobs\Components\MobStateComponent.cs:1-23`；`DamageableComponent.cs:1-86`；`MindComponent.cs:1-112` | 只参考单一内聚状态、权威值与关系分离；不引入其命名、代码、目录或领域语义 | `partial` |
| 当前 NLTX | `D:\TRbackup\NLTX\src\Npc`、`src\Town`、`src\Combat`、`src\WorldSession\WorldGeneration`、`src\Relationships`、`src\Share\Entity\Components` | 核对已有局部类型、重复模型和当前映射 | `partial` |

### 3.2 分组准则

1. 只有共同表达同一个概念、共同创建和清理、共同维护一个不变量的字段才放在同一 Component。
2. 权威状态和可由其他状态计算出的值分开；派生值不再成为第二个可写状态源。
3. NPC 个体、住房关系和世界住房索引具有不同状态范围，不能通过复制字段来假装它们拥有相同生命周期。
4. `whoAmI`、旧 target 索引和 `realLife` 都是兼容语义，不能未经确认转换为稳定实例身份、实体关系或网络身份。
5. Entity GUID、NPC 实例 ID、旧槽位、定义/变体 ID、网络实例 ID、玩家 session ID 和持久化 ID 分别建模；任何共享 ID 的最终 owner 都标为 `crossSubsystemOwner: integration-review`。
6. 默认值只有在源码或现有值对象明确支持时才写成规范值。源码没有闭合初始化路径的字段使用 `unresolved`，不以候选值冒充事实。
7. 目录和文件名不表达更新先后；本文件只描述状态组合，不用隐含顺序解决所有权问题。

### 3.3 公开 API 交叉核对

离线文档确认了 `NPC.life`、`lifeMax`、`ai`、`target`、`type`、`netID`、`realLife`、`timeLeft`、`townNPC`、`homeless`、`homeTileX`、`homeTileY`、`netUpdate` 的公开语义；也确认了 `ModNPC.CanTownNPCSpawn`、`CheckActive`、`OnSpawn`、`ModifyNPCLoot`、`PreKill`、`OnKill`、`SendExtraAI`、`ReceiveExtraAI` 以及全局 NPC 的额外 AI 成员。该证据只能支持字段分类和边界提醒，不能补写 Version4 私有实现。

## 4. Version4 成员到 Component 归属表

表中的 `proposed Component` 是目标设计名称，不代表该类型已在根目录 NLTX 创建。`evidenceStatus` 描述 Version4 对该成员的证据强度；`partial` 的成员不允许在本文件中被扩大解释。

| 成员 | 声明类型 | 字段含义 | 权威/派生/缓存/快照/兼容 | 生命周期 | proposed Component | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|---|
| `Entity.whoAmI` | `int` | 旧实体数组槽位索引 | 兼容字段 | 实体分配、槽位复用、释放 | `NpcLegacySlotComponent` | `confirmed` | `D:\TRbackup\Version4\Terraria\Entity.cs:6-24`；`NPC.cs:67098-67143` |
| `NPC.active` | `bool` | NPC 当前是否有效 | 权威状态 | 创建、活动期、清理 | `NpcLifecycleComponent` | `confirmed` | `D:\TRbackup\Version4\Terraria\NPC.cs:5897`；`76711-76860` |
| `NPC.timeLeft` | `int` | 活动或离场剩余计时 | 权威状态；精确语义仍需裁决 | 活动期、离场期 | `NpcLifecycleComponent` | `partial` | `D:\TRbackup\Version4\Terraria\NPC.cs:6319`；`76711-76860` |
| `NPC.type` | `int` | NPC 定义类型 | 权威定义引用 | 初始化、变体重设、恢复 | `NpcDefinitionReferenceComponent` | `confirmed` | `D:\TRbackup\Version4\Terraria\NPC.cs:6307`；`8133-8248` |
| `NPC.netID` | `int` | 负值定义/变体 ID 语义 | 权威定义引用；不是实例网络 ID | 初始化、变体选择、同步边界 | `NpcDefinitionReferenceComponent` | `confirmed` | `D:\TRbackup\Version4\Terraria\NPC.cs:6391`；tModLoader `class_n_p_c.html#a7ee822e8438c4882685b54b4781bbebf` |
| `NPC.aiStyle` | `int` | 旧 AI 风格标识 | 权威行为状态 | 初始化、活动期更新 | `NpcBehaviorComponent` | `confirmed` | `D:\TRbackup\Version4\Terraria\NPC.cs:6315` |
| `NPC.aiAction` | `int` | 旧 AI 动作值 | 权威行为状态 | 初始化、活动期更新 | `NpcBehaviorComponent` | `confirmed` | `D:\TRbackup\Version4\Terraria\NPC.cs:6313` |
| `NPC.ai[0..3]` | `float[4]` | 可公开同步的 AI 槽位 | 权威行为状态 | 创建、活动期更新、同步 | `NpcBehaviorComponent` | `confirmed` | `D:\TRbackup\Version4\Terraria\NPC.cs:6309`；tModLoader `class_n_p_c.html#aa057189d345452f8afe82acb7a5dbec3` |
| `NPC.localAI[0..3]` | `float[4]` | 实例本地 AI 槽位 | 权威本地状态；恢复/同步语义未闭合 | 创建、活动期更新、清理 | `NpcLocalBehaviorStateComponent` | `confirmed` | `D:\TRbackup\Version4\Terraria\NPC.cs:6311` |
| `NPC.target` | `int` | 旧目标编码，可指向 Player/NPC 旧索引 | 兼容字段加目标关系投影 | 目标选择、目标失效、实体清理 | `NpcTargetComponent` | `confirmed` | `D:\TRbackup\Version4\Terraria\NPC.cs:6321`；tModLoader `class_n_p_c.html#a24627ed4a99fbc0555b1be4717a9335e` |
| `NPC.oldTarget` | `int` | 前一旧目标编码 | 兼容字段 | 目标切换和重置 | `NpcTargetComponent` | `partial` | `D:\TRbackup\Version4\Terraria\NPC.cs:6363` |
| `NPC.realLife` | `int` | 多段 NPC 的旧父/共享生命关联索引 | 兼容关系字段；不是第二份生命值 | 创建、附着、父失效、清理 | `NpcParentRelationComponent` | `confirmed` | `D:\TRbackup\Version4\Terraria\NPC.cs:6039`；tModLoader `class_n_p_c.html#afcca3c242b80ef23de93ebb6135acc9c` |
| `NPC.life` | `int` | 当前生命值 | 权威状态；与 Combat owner 冲突 | 创建、伤害、死亡、恢复 | `NpcHealthComponent` | `confirmed` | `D:\TRbackup\Version4\Terraria\NPC.cs:6341`；tModLoader `class_n_p_c.html#a067a1a8a65adff35ccb1d12cd1a2c04a` |
| `NPC.lifeMax` | `int` | 最大生命值 | 权威定义/实体状态候选 | 初始化、变体重设、恢复 | `NpcHealthComponent` | `confirmed` | `D:\TRbackup\Version4\Terraria\NPC.cs:6343`；tModLoader `class_n_p_c.html#ab2b2b7364c7f40539661033876cf9f1e` |
| `NPC.townNPC` | `bool` | 是否属于城镇居民类别 | 权威居民资格 | 初始化、变体重设、清理 | `TownResidentComponent` | `confirmed` | `D:\TRbackup\Version4\Terraria\NPC.cs:6397`；tModLoader `class_n_p_c.html#ada99030b93a11b2198ddde81afa1fffe` |
| `NPC.friendly` | `bool` | 是否友好 | 权威居民/战斗关系状态 | 初始化、变体重设、活动期 | `TownResidentComponent` | `confirmed` | `D:\TRbackup\Version4\Terraria\NPC.cs:6423` |
| `NPC.housingCategory` | `int` | 住房类别约束 | 权威居民能力约束 | 初始化、变体重设、住房判断 | `TownResidentComponent` | `confirmed` | `D:\TRbackup\Version4\Terraria\NPC.cs:6415` |
| `NPC.homeless` | `bool` | 是否没有住房 | 权威住房关系状态；`Status` 是派生表达候选 | 住房分配、驱逐、恢复 | `TownHousingRelationComponent` | `confirmed` | `D:\TRbackup\Version4\Terraria\NPC.cs:6403`；tModLoader `class_n_p_c.html#afbae8c290840311f6604857d17b38229` |
| `NPC.homeTileX/Y` | `int,int` | 住房位置 | 权威住房关系状态 | 分配、驱逐、恢复、清理 | `TownHousingRelationComponent` | `confirmed` | `D:\TRbackup\Version4\Terraria\NPC.cs:6411-6413`；tModLoader `class_n_p_c.html#a56cfccd7013db3b040a0c5531f3a83c2`、`a485cd27a7ef656e5d2177160583685a3` |
| `NPC.homelessDespawn` | `bool` | 无家时的离场标记 | 权威住房/生命周期交界状态；owner 未裁决 | 住房失效、离场判断、清理 | `TownHousingRelationComponent` | `confirmed` | `D:\TRbackup\Version4\Terraria\NPC.cs:6405` |
| `NPC.lookForHomeTimeout` | `int` | 寻找住房的冷却/超时 | 权威住房关系状态 | 驱逐或无家后递减、重新查找 | `TownHousingRelationComponent` | `confirmed` | `D:\TRbackup\Version4\Terraria\NPC.cs:6407`；`WorldGen.cs:5361-5417` |
| `TownRoomManager._roomLocationPairs` | `Dictionary` | 居民 key 到房间位置 | 世界级权威索引 | 世界建立、住房扫描、保存、加载 | `TownHousingRegistryComponent` | `confirmed` | `D:\TRbackup\Version4\Terraria.GameContent\TownRoomManager.cs:9-177` |
| `TownRoomManager._hasRoom` | `Dictionary/Set` | 居民 key 是否拥有房间 | 世界级派生/索引状态 | 房间扫描、分配、驱逐 | `TownHousingRegistryComponent` | `confirmed` | `D:\TRbackup\Version4\Terraria.GameContent\TownRoomManager.cs:9-177` |
| `TownHousingResidentKey.NpcType` | `int` | 当前 NLTX 住房 key 的 NPC type | 兼容/候选索引 key | 索引创建、查找、保存 | `TownHousingRegistryComponent` | `confirmed` | `D:\TRbackup\NLTX\src\WorldSession\WorldGeneration\TownHousingResidentKey.cs:3-5` |
| `NPC.netUpdate` | `bool` | NPC 状态需要同步的标记 | 外部边界状态；暂缓，不进入权威组件 | 状态变化、同步边界、清除 | 暂不归属 | `confirmed` | `D:\TRbackup\Version4\Terraria\NPC.cs:6015`；tModLoader `class_n_p_c.html#a8d6296daef89c984bc583ecb51593f4a` |
| `NPC.spawnNeedsSyncing` | `bool` | 生成状态需要同步的标记 | 外部边界状态；暂缓，不进入权威组件 | 生成、同步、清除 | 暂不归属 | `confirmed` | `D:\TRbackup\Version4\Terraria\NPC.cs:6031` |
| `NpcReplicationDirtyState.ClientStates` | `Dictionary<ulong,...>` | 每客户端复制游标和状态 | 缓存/边界状态 | 客户端连接、增量同步、断开 | 暂不归属 | `confirmed` | `D:\TRbackup\NLTX\src\Npc\NpcReplicationDirtyState.cs:5-17` |

没有在 Version4 中找到稳定的 `NpcInstanceId`、`PersistentEntityId` 或 NPC 实例 `NetworkId` 成员。`whoAmI` 只能归到兼容槽位；`netID` 只能归到定义/变体引用。

## 5. Component 定义

以下每个定义只描述状态组成。`componentOwner` 是当前子系统内的候选 owner；凡是会被其他子系统读取、写入或作为共享关系使用的字段，均保留 `crossSubsystemOwner: integration-review`。

### 5.1 NpcIdentityComponent

| 项目 | 值 |
|---|---|
| `componentId` | `NPC-COMP-IDENTITY` |
| `name` | `NpcIdentityComponent` |
| `status` | `proposed` |
| `componentOwner` | `NpcAndTownSimulation`（candidate） |
| `crossSubsystemOwner` | `integration-review` |
| `entityScope` | 单个 NPC Entity |
| `lifecycle` | Entity 建立时创建，实体有效期内保持，实体清理时失效 |

#### 职责

保存 NPC 实例在当前实体生命周期内的稳定身份。它不保存旧数组槽位、定义类型、网络实例号、持久化身份或玩家 session 身份。

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| `NpcInstanceId` | `NpcInstanceId` | `Value = 0` 表示无效；有效值默认未确认 | 权威状态 | 有效实例必须 `Value != 0`；一个实体生命周期内不得改变 | `missing` | `D:\TRbackup\NLTX\src\Npc\NpcInstanceId.cs:3-6` 仅证明值对象约束；Version4 无对应稳定字段 |

#### 字段不变量

`NpcInstanceId` 不得取代 `whoAmI`，也不得从 `netID` 推导。是否跨卸载/加载保持、是否映射到持久化 ID 尚未确认。

#### 生命周期

在 NPC Entity 建立时生成或接收，在实体有效期内保持不变；实体彻底清理时失效。恢复时是否沿用旧值属于 `BD-COMP-01`，本设计不写死。

#### Entity/World 范围

单个 NPC Entity。

#### ID 与关系字段

这是 NPC 实例 ID，不是 Entity GUID、旧槽位、持久化 ID、网络实例 ID 或外部 ID。`crossSubsystemOwner: integration-review`。

#### 当前 NLTX 映射

`src/Npc/NpcEntityIdentityComponent.cs:3-15` 已同时持有 `InstanceId` 和 `LegacySlot`；设计上必须拆为本组件与 `NpcLegacySlotComponent`。当前映射为 `partial`，不能视为已经拆分。

#### 证据

Version4 的 `Entity.whoAmI` 位于 `D:\TRbackup\Version4\Terraria\Entity.cs:6-24`，并在 NPC 槽位分配处使用；它不是稳定实例身份。Version4 没有被确认的 NPC 实例稳定 ID，故本组件保持 `status: proposed` 和 `evidenceStatus: missing`。

### 5.2 NpcLegacySlotComponent

| 项目 | 值 |
|---|---|
| `componentId` | `NPC-COMP-LEGACY-SLOT` |
| `name` | `NpcLegacySlotComponent` |
| `status` | `proposed` |
| `componentOwner` | `NpcAndTownSimulation`（candidate） |
| `crossSubsystemOwner` | `integration-review` |
| `entityScope` | 单个 NPC Entity |
| `lifecycle` | Entity 建立后可未分配，槽位分配时更新，释放时回到未分配 |

#### 职责

保存 NPC 对旧实体数组槽位的兼容引用。该组件只表达当前或最近分配的槽位，不表达实体身份。

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| `LegacySlot` | `NpcSlot` | `Value = -1` 表示未分配 | 兼容字段 | 只有 `Value >= 0` 才能作为旧数组索引；槽位复用时不得改变 `NpcInstanceId` 的语义 | `confirmed` | `D:\TRbackup\Version4\Terraria\Entity.cs:6-24`；`NPC.cs:67098-67143`；`D:\TRbackup\NLTX\src\Npc\NpcSlot.cs:3-6` |

#### 字段不变量

槽位只在兼容边界使用。旧槽位无效不等于 NPC Entity 不存在；槽位分配冲突、复用和跨子系统引用需要整合层裁决。

#### 生命周期

实体创建后可未分配；分配成功后跟随旧数组生命周期；释放或迁移时可回到 `-1`。不得用槽位值判断稳定身份。

#### Entity/World 范围

单个 NPC Entity；槽位集合的唯一性属于旧世界容器边界，不由本组件单独宣布。

#### ID 与关系字段

`NpcSlot` 是旧索引，不是 NPC 实例 ID。`src\WorldStorage\NpcSlot.cs:1-3` 还存在另一个 `Terraria.WorldStorage.NpcSlot`，与 `Terraria.Npc.NpcSlot` 同名但值对象约束不同；`crossSubsystemOwner: integration-review`。

#### 当前 NLTX 映射

当前由 `src/Npc/NpcEntityIdentityComponent.cs:5-15` 与 `src/Npc/NpcSlot.cs:3-6` 联合表达，尚未独立成目标组件；`src/WorldStorage/NpcSlot.cs:1-3` 的 owner 未解析。

#### 证据

`whoAmI`、`NewNPC` 的槽位分配和 slot protection 提供了旧槽位证据；没有提供稳定实例身份证据。因此本组件只承担兼容槽位。

### 5.3 NpcDefinitionReferenceComponent

| 项目 | 值 |
|---|---|
| `componentId` | `NPC-COMP-DEFINITION` |
| `name` | `NpcDefinitionReferenceComponent` |
| `status` | `partial` |
| `componentOwner` | `NpcAndTownSimulation`（candidate） |
| `crossSubsystemOwner` | `integration-review` |
| `entityScope` | 单个 NPC Entity |
| `lifecycle` | 定义初始化时创建，变体/恢复时更新，实体清理时移除 |

#### 职责

保存 NPC 的定义类型、变体定义引用、初始定义和 catalog 版本。它不保存个体活动状态、AI 槽位或实例网络号。

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| `TypeId` | `NpcTypeId` | 无效 `Value <= 0` | 权威状态 | 必须是有效定义才能作为已初始化 NPC 定义 | `confirmed` | `D:\TRbackup\Version4\Terraria\NPC.cs:6307`；`src/Npc/NpcTypeId.cs:3-6` |
| `NetId` | `NpcNetId` | `Value = 0`；负值表示变体的具体含义由定义表提供 | 权威定义引用 | 不得解释为实例网络 ID | `confirmed` | `D:\TRbackup\Version4\Terraria\NPC.cs:6391`；`src/Npc/NpcNetId.cs:3-6`；tModLoader `class_n_p_c.html#a7ee822e8438c4882685b54b4781bbebf` |
| `InitialTypeId` | `NpcTypeId` | 与创建时 `TypeId` 相同；加载/变体覆盖语义需保持 | 兼容/恢复字段 | 初始化时有效；与当前 `TypeId` 的差异必须可解释 | `partial` | `D:\TRbackup\NLTX\src\Npc\NpcDefinitionReferenceComponent.cs:5-23`；Version4 `SetDefaults` `NPC.cs:8133-8248` |
| `CatalogRevision` | `int` | `0` | 快照/兼容字段 | 不能用负值表示已加载定义；版本不匹配不能静默当作兼容 | `partial` | `D:\TRbackup\NLTX\src\Npc\NpcDefinitionReferenceComponent.cs:5-23`；Version4 无对应直接成员 |

#### 字段不变量

`TypeId` 和 `InitialTypeId` 均有效时 `IsInitialized` 才成立。`NetId` 只能说明定义变体语义；实例网络 ID 仍为 `EG-COMP-02`。

#### 生命周期

创建和 `SetDefaults` 时建立；定义变体或兼容加载时可能更新；实体清理时一起移除。`CatalogRevision` 的持久化范围仍需定义 owner。

#### Entity/World 范围

单个 NPC Entity；定义目录本身不由该 Component 持有。

#### ID 与关系字段

`NpcTypeId`、`NpcNetId` 都是定义层值，不是实体引用。定义目录版本若由多个子系统读取，`crossSubsystemOwner: integration-review`。

#### 当前 NLTX 映射

`src/Npc/NpcDefinitionReferenceComponent.cs:3-23` 已提供大部分字段和派生属性，状态为 `partial`；它不能证明 Version4 的 catalog 版本语义。

#### 证据

Version4 `NPC.type`、`netID` 和 `SetDefaults` 成员直接支持此边界；tModLoader 对 `netID` 的公开语义进一步确认它不是实例网络号。

### 5.4 NpcBehaviorComponent

| 项目 | 值 |
|---|---|
| `componentId` | `NPC-COMP-BEHAVIOR` |
| `name` | `NpcBehaviorComponent` |
| `status` | `partial` |
| `componentOwner` | `NpcAndTownSimulation`（candidate） |
| `crossSubsystemOwner` | `integration-review` |
| `entityScope` | 单个 NPC Entity |
| `lifecycle` | 定义初始化时创建，活动期更新，实体清理时移除 |

#### 职责

保存需要作为 NPC 行为权威状态表达的旧 AI 风格、动作和四个公开 AI 槽位。只保留行为状态，不把本地 AI、目标、生命周期或居民状态塞入其中。

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| `AiStyle` | `int` | `0` 作为候选初始化值；创建路径仍需复核 | 权威状态 | 取值必须能被 NPC 定义解释；未知值不得静默映射为另一风格 | `confirmed` | `D:\TRbackup\Version4\Terraria\NPC.cs:6315` |
| `Action` | `int` | `0` 作为候选初始化值；创建路径仍需复核 | 权威状态 | 动作值与定义的 AI 语义一致；不承载生命周期阶段 | `confirmed` | `D:\TRbackup\Version4\Terraria\NPC.cs:6313` |
| `AiSlots` | `float[4]` | 四个 `0f`；长度固定为 4 | 权威状态 | 长度必须为 4；槽位顺序不可改变；非有限值处理规则未确认 | `confirmed` | `D:\TRbackup\Version4\Terraria\NPC.cs:6309`；tModLoader `class_n_p_c.html#aa057189d345452f8afe82acb7a5dbec3` |

#### 字段不变量

`AiSlots` 与 `LocalAiSlots` 不得互为镜像；本组件不拥有 `LastUpdatedTick` 这类记录性字段。`BehaviorKind` 不是 Version4 直接字段，若保留只能作为定义映射的派生值，不能成为第二权威状态。

#### 生命周期

在 NPC 定义初始化时建立，活动期间更新；恢复和外部同步只改变有公开证据的槽位。清理时随 NPC Entity 移除。

#### Entity/World 范围

单个 NPC Entity。

#### ID 与关系字段

不持有 Entity ID 或外部 ID。AI 槽位如被网络边界读取，其边界 owner 仍需与 `BD-COMP-04` 一起裁决。

#### 当前 NLTX 映射

`src/Npc/NpcBehaviorStateComponent.cs:3-23` 已有 `LegacyAiStyle`、`Action` 和四槽位，状态为 `partial`。`BehaviorKind` 与 `LastUpdatedTick` 不直接映射进目标权威字段。

#### 证据

Version4 `NPC.aiStyle`、`aiAction`、`ai[4]` 的成员位置已确认；tModLoader `NPC.ai` 页面确认四 slot 的公开边界。

### 5.5 NpcLocalBehaviorStateComponent

| 项目 | 值 |
|---|---|
| `componentId` | `NPC-COMP-LOCAL-BEHAVIOR` |
| `name` | `NpcLocalBehaviorStateComponent` |
| `status` | `proposed` |
| `componentOwner` | `NpcAndTownSimulation`（candidate） |
| `crossSubsystemOwner` | `integration-review` |
| `entityScope` | 单个 NPC Entity |
| `lifecycle` | NPC 建立时创建，活动期更新，实体清理时移除 |

#### 职责

保存只属于当前 NPC 实例本地的四个 AI 槽位，避免把同步语义不同的状态与公开 AI 槽位合并。

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| `LocalAiSlots` | `float[4]` | 四个 `0f`；长度固定为 4 | 权威本地状态 | 长度必须为 4；不得自动当作公开同步字段；非有限值处理规则未确认 | `confirmed` | `D:\TRbackup\Version4\Terraria\NPC.cs:6311` |

#### 字段不变量

该数组与 `NpcBehaviorComponent.AiSlots` 具有不同同步/恢复证据，不能用一个数组同时满足两种语义。是否持久化由恢复边界裁决，当前不假定。

#### 生命周期

NPC 建立时创建，活动期由实例状态更新，实体清理时移除。加载后如何恢复仍是 `partial` 证据。

#### Entity/World 范围

单个 NPC Entity。

#### ID 与关系字段

不持有 ID 或关系。

#### 当前 NLTX 映射

`src/Npc/NpcAiStateComponent.cs:3-26` 有四个 state 字段、`Style` 和 `Timer`，但与 `NpcBehaviorStateComponent` 存在重叠；它只能作为局部证据，不能原样合并进本组件。目标状态为 `proposed`。

#### 证据

Version4 有独立 `localAI` 数组，足以支持与 `ai` 分开；没有充分证据支持 `Timer` 或 `Style` 的最终归属。

### 5.6 NpcTargetComponent

| 项目 | 值 |
|---|---|
| `componentId` | `NPC-COMP-TARGET` |
| `name` | `NpcTargetComponent` |
| `status` | `partial` |
| `componentOwner` | `NpcAndTownSimulation`（candidate） |
| `crossSubsystemOwner` | `integration-review` |
| `entityScope` | 单个 NPC Entity |
| `lifecycle` | NPC 创建时为空，目标选择时更新，目标失效或实体清理时清除 |

#### 职责

保存 NPC 的目标关系及其旧索引兼容信息。它不保存通用目标缓存，也不把目标实体的状态复制到 NPC。

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| `TargetReference` | `EntityReference?` | `null` 或 `EntityReference.None` | 权威关系状态 | 非空引用的 scope 必须与 `TargetKind` 相容；空引用时不得声称已有目标 | `partial` | `D:\TRbackup\NLTX\src\Npc\NpcTargetComponent.cs:5-25`；`src/Relationships/EntityReference.cs:5-10` |
| `TargetKind` | `NpcTargetKind` | `None` | 权威关系分类 | `None` 必须与空目标一致；`Player`、`Npc`、`PlayerTankPet` 的范围不得混淆 | `confirmed` | `D:\TRbackup\Version4\Terraria\NPC.cs:6321`；`src/Npc/NpcTargetKind.cs:3-8` |
| `LegacyTargetIndex` | `int` | `-1` | 兼容字段 | `-1` 表示无旧目标；正值的 Player/NPC 解释必须由 `TargetKind` 约束 | `confirmed` | `D:\TRbackup\Version4\Terraria\NPC.cs:6321`；tModLoader `class_n_p_c.html#a24627ed4a99fbc0555b1be4717a9335e` |
| `PreviousLegacyTargetIndex` | `int` | `-1` | 兼容字段 | 只记录上一次旧编码，不可代替当前 `TargetReference` | `partial` | `D:\TRbackup\Version4\Terraria\NPC.cs:6363` |

#### 字段不变量

仅保留 EntityReference 会丢失 Version4 `target` 可编码的 Player/NPC 旧索引语义；仅保留 int 又无法表达稳定关系。因此两者必须显式区分。`SelectedAtTick` 是记录性元数据，不进入目标权威组件。

#### 生命周期

NPC 创建时为空；目标选择时建立；目标失效、目标类型变化或实体清理时同步清除兼容字段和关系字段。

#### Entity/World 范围

单个 NPC Entity；目标对象属于其他 Entity，不被本组件拥有。

#### ID 与关系字段

`TargetReference.EntityId` 是 Entity GUID 引用，不能直接视为 NPC 实例 ID；`EntityReferenceScope` 必须保留。跨 NPC、Player 和通用关系的 owner 为 `crossSubsystemOwner: integration-review`。

#### 当前 NLTX 映射

`src/Npc/NpcTargetComponent.cs:5-25` 已有分类、引用和选择 tick，状态为 `partial`；`src/Npc/TargetingComponent.cs:5-14` 又有一个无分类的 `Target` 字段。两者不能同时作为权威目标源。

#### 证据

Version4 `target`、`oldTarget` 和 tModLoader `NPC.target` 公开语义支持旧索引字段；当前 NLTX 的 EntityReference 支持稳定关系形式，但不闭合旧索引编码。

### 5.7 NpcLifecycleComponent

| 项目 | 值 |
|---|---|
| `componentId` | `NPC-COMP-LIFECYCLE` |
| `name` | `NpcLifecycleComponent` |
| `status` | `proposed` |
| `componentOwner` | `NpcAndTownSimulation`（candidate） |
| `crossSubsystemOwner` | `integration-review` |
| `entityScope` | 单个 NPC Entity |
| `lifecycle` | Entity 建立时进入未初始化/活动前状态，活动期间更新，清理完成后失效 |

#### 职责

保存 NPC 是否活动、活动剩余计时和生命周期阶段。它不保存生命、掉落、住房、网络脏标记或人口成本。

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| `IsActive` | `bool` | `false` 作为未初始化候选；初始化路径未闭合 | 权威状态 | `false` 不等于槽位一定空闲；实体阶段和活动标志必须一致 | `confirmed` | `D:\TRbackup\Version4\Terraria\NPC.cs:5897` |
| `RemainingActiveTicks` | `int` | `0`；是否从 `timeLeft` 直接映射未裁决 | 权威状态 | 不得小于 0；计时耗尽与阶段转换的边界需一致 | `partial` | `D:\TRbackup\Version4\Terraria\NPC.cs:6319`；`76711-76860` |
| `Stage` | `NpcLifecycleStage` | 未确认；禁止从枚举首项推断 | 权威状态 | 阶段只能按允许的状态集合转换；`Despawned` 不得继续被当作活动实体 | `partial` | `D:\TRbackup\NLTX\src\Npc\NpcLifecycleStage.cs:3-8`；Version4 `active/timeLeft` |

#### 字段不变量

`IsActive`、`RemainingActiveTicks` 和 `Stage` 是同一生命周期概念的三个表达面，不能被三个独立 owner 写入。生命归零、住房无家、父失效和世界卸载等原因不直接写入本组件的细节；它们只可能导致阶段变化，具体边界待裁决。

#### 生命周期

实体建立时进入未初始化或活动前状态；初始化成功后进入活动状态；离场请求后进入待清理状态；清理完成后进入不可用状态。具体初始 `Stage` 和 `timeLeft` 转换未由当前证据完全确认。

#### Entity/World 范围

单个 NPC Entity。

#### ID 与关系字段

不持有 ID。人口槽位成本、despawn reason 和网络同步标记不作为本组件字段。

#### 当前 NLTX 映射

`src/Npc/NpcLifecycleComponent.cs:3-12` 已有 `Stage` 与 `RemainingDespawnTicks`，但没有独立 `IsActive`，且字段命名和 `timeLeft` 的对应关系未闭合；当前映射为 `partial`。`NpcLifetimeComponent` 的剩余时间和人口成本不直接合并。

#### 证据

Version4 的 `active`、`timeLeft`、`UpdateNPC`、`ResetForNewNPC` 和 `SetDefaults` 提供成员与生命周期入口；清理写入的跨域边界仍属于 `EG-COMP-06`。

### 5.8 NpcParentRelationComponent

| 项目 | 值 |
|---|---|
| `componentId` | `NPC-COMP-PARENT-RELATION` |
| `name` | `NpcParentRelationComponent` |
| `status` | `proposed` |
| `componentOwner` | `NpcAndTownSimulation`（candidate） |
| `crossSubsystemOwner` | `integration-review` |
| `entityScope` | 单个子 NPC Entity |
| `lifecycle` | 子实体建立时可为空，附着时创建/更新，父关系失效或实体清理时移除 |

#### 职责

保存 NPC 与父 NPC 的关系，包括稳定实例关系候选、旧父槽位和附着时刻。它不复制父实体的生命值，也不把 `realLife` 解释成生命聚合本身。

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| `ParentInstanceId` | `NpcInstanceId` | 无效 `Value = 0` 表示无父关系 | 权威关系状态候选 | 有效时必须指向仍存在的 NPC 实例；不能只凭 ID 判断父实体活动 | `missing` | Version4 只有 `realLife` 旧索引；`D:\TRbackup\NLTX\src\Npc\NpcParentRelationComponent.cs:5-21` |
| `ParentLegacySlot` | `NpcSlot` | `Value = -1` | 兼容字段 | 只用于旧 `realLife` 映射和兼容查找；无效不等于没有稳定父关系 | `confirmed` | `D:\TRbackup\Version4\Terraria\NPC.cs:6039`；`src/Npc/NpcSlot.cs:3-6` |
| `AttachedAtTick` | `long` | `0`；tick 起点语义未确认 | 关系元数据 | 附着时刻不得晚于当前世界时刻；世界时钟 owner 未定 | `partial` | `D:\TRbackup\NLTX\src\Npc\NpcParentRelationComponent.cs:5-21`；Version4 `realLife` |

#### 字段不变量

`realLife` 是旧父/共享生命关联索引的证据，不足以证明 `ParentInstanceId` 的稳定性，也不足以证明多个实体共享同一个 `NpcHealthComponent`。父关系与生命 owner 必须在 `BD-COMP-03` 一起裁决。

#### 生命周期

子 NPC 建立时可没有关系；附着时写入；父关系失效、父实体清理或子实体清理时移除或转入明确的兼容状态。关系删除不得隐式复制或重置生命值。

#### Entity/World 范围

单个子 NPC Entity 上的关系 Component；反向父到子索引不在本组件内。

#### ID 与关系字段

`ParentInstanceId` 是候选稳定 NPC 实例关系，`ParentLegacySlot` 是旧索引。两个字段的最终 owner 为 `crossSubsystemOwner: integration-review`。

#### 当前 NLTX 映射

`src/Npc/NpcParentRelationComponent.cs:3-21` 已有三字段，状态为 `partial`；它不能补足 Version4 没有的稳定实例父 ID 语义。

#### 证据

Version4 `NPC.realLife` 位于 `NPC.cs:6039`；tModLoader `NPC.realLife` 页面确认公开成员存在，但未提供当前 Version4 稳定实例身份证据。

### 5.9 NpcHealthComponent

| 项目 | 值 |
|---|---|
| `componentId` | `NPC-COMP-HEALTH` |
| `name` | `NpcHealthComponent` |
| `status` | `partial` |
| `componentOwner` | `NpcAndTownSimulation`（candidate） |
| `crossSubsystemOwner` | `integration-review` |
| `entityScope` | 单个 NPC Entity |
| `lifecycle` | NPC 初始化时创建，生命变化期间更新，实体清理时移除 |

#### 职责

保存 NPC 的当前生命和最大生命。它只承担生命值，不承担死亡原因、掉落、父级共享生命、战斗通用健康状态或网络快照。

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| `CurrentLife` | `int` | `0` 作为值对象安全默认；初始化路径的过渡值未确认 | 权威状态 | 稳定状态候选为 `0 <= CurrentLife <= MaximumLife`；过渡期是否允许越界未确认 | `confirmed` | `D:\TRbackup\Version4\Terraria\NPC.cs:6341`；`src/Npc/NpcHealthComponent.cs:3-15` |
| `MaximumLife` | `int` | `0` 作为值对象安全默认；定义初始化应提供非负值 | 权威状态 | 不得为负；最大值变更必须说明对当前生命的影响 | `confirmed` | `D:\TRbackup\Version4\Terraria\NPC.cs:6343`；tModLoader `class_n_p_c.html#ab2b2b7364c7f40539661033876cf9f1e` |

#### 字段不变量

`CurrentLife <= 0` 的 `IsDead` 是派生值，不单独存储。`realLife` 不复制为第二组生命字段。`src/Combat/HealthComponent.cs:3-14` 与本组件存在 `Current/Maximum` 重叠，最终 owner 未决。

#### 生命周期

NPC 初始化时建立；伤害、治疗、定义变化和死亡判定期间更新；实体清理时移除。父子共享生命的组合规则不得由本组件自行猜测。

#### Entity/World 范围

单个 NPC Entity；如果最终决定由 Combat 统一拥有，则本组件可能被删除或变为明确的 NPC 专用视图，当前不宣布。

#### ID 与关系字段

不持有 ID。与 Combat 的共享生命字段、死亡边界和父关系属于 `crossSubsystemOwner: integration-review`。

#### 当前 NLTX 映射

`src/Npc/NpcHealthComponent.cs:3-15` 已有同名字段，状态为 `partial`；`src/Combat/HealthComponent.cs:3-14` 也存在相同形状，不能让两者同时成为权威源。

#### 证据

Version4 `life/lifeMax` 与公开 tModLoader 页面直接支持字段语义；完整 owner、共享父生命和清理边界仍为 `EG-COMP-06`、`BD-COMP-03`。

### 5.10 TownResidentComponent

| 项目 | 值 |
|---|---|
| `componentId` | `TOWN-COMP-RESIDENT` |
| `name` | `TownResidentComponent` |
| `status` | `partial` |
| `componentOwner` | `NpcAndTownSimulation`（candidate） |
| `crossSubsystemOwner` | `integration-review` |
| `entityScope` | 单个城镇居民 NPC Entity |
| `lifecycle` | 居民资格确认时创建，定义变体/恢复时更新，资格移除或实体清理时移除 |

#### 职责

保存 NPC 是否具有城镇居民资格、友好性、住房类别和居民能力集合。它不保存具体房间坐标、无家倒计时或世界房间索引。

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| `IsTownResident` | `bool` | Component 附着时为 `true`；无 Component 表示 `false` | 权威状态 | `true` 只能在居民 Component 存在时成立 | `confirmed` | `D:\TRbackup\Version4\Terraria\NPC.cs:6397`；`D:\TRbackup\NLTX\src\Town\TownResidentComponent.cs:5-16` |
| `IsFriendly` | `bool` | `false` 作为安全默认；定义初始化覆盖 | 权威状态 | 友好性不能推导住房分配结果 | `confirmed` | `D:\TRbackup\Version4\Terraria\NPC.cs:6423`；`src/Town/TownResidentComponent.cs:5-22` |
| `HousingCategory` | `int` | `0` 作为当前值对象默认；有效范围由定义目录提供 | 权威约束 | 类别值只能由 NPC 定义解释；不能用坐标代替类别 | `confirmed` | `D:\TRbackup\Version4\Terraria\NPC.cs:6415` |
| `Capabilities` | `TownResidentCapabilities` | `None` | 权威能力集合 | 未设置能力不得推断可住房、对话、交易或幸福度资格 | `partial` | `D:\TRbackup\NLTX\src\Town\TownResidentCapabilities.cs:5-14`；Version4 townNPC/friendly/housingCategory |

#### 字段不变量

居民能力与具体住房关系分离。`IsTownResident`、`IsFriendly` 和 `HousingCategory` 可以被定义重设，但具体 `HomeTile` 不在本组件中。

#### 生命周期

NPC 定义确认居民资格后创建；定义变体或兼容恢复时更新；不再是居民或实体清理时移除。具体居民资格条件由 Version4 `Main.cs:13645-14075` 与 `NPC.cs:6397` 支持，完整判定仍为局部证据。

#### Entity/World 范围

单个 NPC Entity。

#### ID 与关系字段

不持有 ID；住房 registry 的 resident key 不由居民能力 Component 拥有。

#### 当前 NLTX 映射

`src/Town/TownResidentComponent.cs:3-29` 已有大部分字段和能力派生属性，状态为 `partial`。

#### 证据

Version4 `townNPC`、`friendly`、`housingCategory` 成员已确认；当前 NLTX `Capabilities` 的具体位含义属于设计层语义，不能反向证明 Version4 私有条件。

### 5.11 TownHousingRelationComponent

| 项目 | 值 |
|---|---|
| `componentId` | `TOWN-COMP-HOUSING-RELATION` |
| `name` | `TownHousingRelationComponent` |
| `status` | `proposed` |
| `componentOwner` | `NpcAndTownSimulation`（candidate） |
| `crossSubsystemOwner` | `integration-review` |
| `entityScope` | 单个城镇居民 NPC Entity |
| `lifecycle` | 有居民资格时创建，分配/驱逐/恢复时更新，居民移除或实体清理时清空 |

#### 职责

保存单个城镇居民与住房之间的直接关系：无家状态、住房位置、无家离场标记、寻找住房计时和分配修订号。它不保存所有居民的房间索引。

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| `IsHomeless` | `bool` | `false` 作为候选初始化值；加载路径需复核 | 权威状态 | `true` 时 `HomeTile` 必须为空或仅作为待清理兼容值；不能与已确认房间同时成立 | `confirmed` | `D:\TRbackup\Version4\Terraria\NPC.cs:6403`；tModLoader `class_n_p_c.html#afbae8c290840311f6604857d17b38229` |
| `HomeTile` | `TownRoomTilePoint?` | `null` | 权威关系状态 | 非空坐标必须属于有效房间；具体房间合法性由世界住房范围确认 | `confirmed` | `D:\TRbackup\Version4\Terraria\NPC.cs:6411-6413`；`WorldGen.cs:5310-5639` |
| `HomelessDespawn` | `bool` | `false` | 权威交界状态 | 不能直接等同 `IsHomeless`；只能在有明确无家离场语义时为 `true` | `confirmed` | `D:\TRbackup\Version4\Terraria\NPC.cs:6405` |
| `HomeSearchTimeout` | `int` | `0` | 权威状态 | 不得小于 0；为正时不得声称当前可立即重新查找 | `confirmed` | `D:\TRbackup\Version4\Terraria\NPC.cs:6407`；`WorldGen.cs:5361-5417` |
| `AssignmentRevision` | `uint` | `0` | 权威修订元数据 | 每次关系事实变化必须单调递增；跨加载是否连续需要 owner 决定 | `partial` | `D:\TRbackup\NLTX\src\Town\NpcHousingAssignmentComponent.cs:5-35` |

#### 字段不变量

`TownHousingStatus` 只能作为规范化派生表达或明确兼容值，不能与 `IsHomeless`、`HomeTile` 形成两个可写权威源。住房关系与居民能力分开；居民可能具备住房能力但当前无家。

#### 生命周期

仅在有 `TownResidentComponent` 的 NPC Entity 上创建；分配、驱逐、无家、重新查找和恢复时更新；居民移除或实体清理时清空。房间是否合法由世界级住房范围提供事实，本 Component 不拥有扫描缓存。

#### Entity/World 范围

单个 NPC Entity；房间集合、反向索引和房间扫描缓存属于 `TownHousingRegistryComponent` 或仍待裁决的世界边界。

#### ID 与关系字段

`HomeTile` 是 tile 坐标，不是房间实例 ID；当前 `TownHousingResidentKey` 的 type/instance 选择仍是 `BD-COMP-02`，`crossSubsystemOwner: integration-review`。

#### 当前 NLTX 映射

`src/Town/NpcHousingAssignmentComponent.cs:3-35` 已有状态、坐标、冷却、无家离场和 revision，状态为 `partial`；设计名称强调它是 NPC 居民的直接关系，`TownHousingStatus` 不得成为第二权威源。

#### 证据

Version4 NPC 成员、`TownRoomManager`、`WorldGen.RoomNeeds/QuickFindHome` 和 `WorldFile` 的住房复核支持这些字段；房间 key 的最终范围未闭合。

### 5.12 TownHousingRegistryComponent

| 项目 | 值 |
|---|---|
| `componentId` | `TOWN-COMP-HOUSING-REGISTRY` |
| `name` | `TownHousingRegistryComponent` |
| `status` | `proposed` |
| `componentOwner` | `NpcAndTownSimulation`（candidate） |
| `crossSubsystemOwner` | `integration-review` |
| `entityScope` | World/Town 住房集合 |
| `lifecycle` | 世界建立或住房容器创建时初始化，住房事实变化时更新，世界卸载时销毁 |

#### 职责

保存 World/Town 范围的住房关系索引和修订信息。它是房间分配事实的集合范围状态，不是某个 NPC 的居民 Component。

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| `RoomsByResidentKey` | `Dictionary<TownHousingResidentKey, TilePosition>` | 空字典 | 权威索引状态 | 一个 key 在同一修订中至多对应一个房间；key 语义未裁决 | `confirmed` | `D:\TRbackup\Version4\Terraria.GameContent\TownRoomManager.cs:9-177`；`src/WorldSession/WorldGeneration/TownHousingRegistry.cs:5-14` |
| `HomelessResidentKeys` | `HashSet<TownHousingResidentKey>` | 空集合 | 权威索引状态/派生集合候选 | 不得同时出现在有房和无家集合，除非明确表示过渡态；当前未确认过渡语义 | `confirmed` | 同上 |
| `Revision` | `ulong` | `0` | 权威修订元数据 | 住房事实变化时单调递增；回滚/加载分支需要保留可观察关系 | `partial` | `D:\TRbackup\NLTX\src\WorldSession\WorldGeneration\TownHousingRegistry.cs:7-14` |
| `ResidentKeyMode` | `TownHousingKeyMode`（proposed 值对象/枚举） | `unresolved`；不得默认选择 type、instance 或 dual | 配置/兼容状态 | 当前所有 key 必须遵守同一种已裁决模式；模式切换需有版本语义 | `unresolved` | Version4 `TownRoomManager` type-level key 证据；当前 NLTX `TownHousingResidentKey.cs:3-5` 固定 `NpcType` |

#### 字段不变量

registry 只保存世界级索引，不复制每个 NPC 的 `HomeTile` 作为第二权威值。`RoomsByResidentKey` 与 `HomelessResidentKeys` 的一致性必须可以按 `Revision` 判断；具体 type key、instance key 或 dual key 留给 `BD-COMP-02`。

#### 生命周期

世界建立或住房容器创建时初始化为空；房间扫描和居民分配期间更新；世界保存/加载时恢复；世界卸载时销毁。房间扫描期间使用的临时缓存不自动成为 Component 字段。

#### Entity/World 范围

World/Town 集合范围；不附着在单个 NPC Entity 上。

#### ID 与关系字段

`TownHousingResidentKey` 当前包含 `NpcType`，但 type/instance/dual 仍未裁决。`TilePosition` 是世界 tile 坐标，不是 Entity ID。`crossSubsystemOwner: integration-review`。

#### 当前 NLTX 映射

`src/WorldSession/WorldGeneration/TownHousingRegistry.cs:5-14` 已有房间字典、无家集合和 revision，状态为 `partial`；它当前不是带 `Component` 后缀的类型，且 key 模式仍固定为 type，不能视为目标设计已锁定。

#### 证据

Version4 `TownRoomManager` 的 `_roomLocationPairs`、`_hasRoom`、`SetRoom`、`KickOut`、`Save`、`Load` 和 `GetHouseholdStatus` 支持世界级住房索引。当前 NLTX type key 也已确认，但最终 key 选择属于阻塞决策。

## 6. Entity 与 Component 组合

下表描述静态组合约束，不描述执行先后。`必需 Component` 是目标实体类别的最低状态集合；`可选 Component` 只在相应关系存在时附着。

| Entity/World 对象 | 必需 Component | 可选 Component | 互斥 Component | 组合理由 |
|---|---|---|---|---|
| 普通 NPC Entity | `NpcIdentityComponent`、`NpcLegacySlotComponent`、`NpcDefinitionReferenceComponent`、`NpcBehaviorComponent`、`NpcLocalBehaviorStateComponent`、`NpcTargetComponent`、`NpcLifecycleComponent`、`NpcHealthComponent` | `NpcParentRelationComponent` | 无 | 这些字段属于单个 NPC，但身份、槽位、定义、行为、目标、生命周期和生命的生命周期/不变量不同，不能合并成一个巨型 Component |
| 有父级的 NPC Entity | 普通 NPC Entity 的必需集合 | `NpcParentRelationComponent` | 无 | 父关系是可选关系；不能通过复制父生命或替换 NPC 身份表达 |
| 城镇居民 NPC Entity | 普通 NPC Entity 的必需集合 | `TownResidentComponent`、`TownHousingRelationComponent` | 无 `TownResidentComponent` 时不得附着 `TownHousingRelationComponent` | 居民资格与具体住房关系不同；没有居民资格时住房关系没有合法实体范围 |
| 暂无住房的居民 NPC Entity | 普通 NPC Entity 的必需集合、`TownResidentComponent`、`TownHousingRelationComponent` | 无 | `HomeTile` 非空的“已分配”状态与 `IsHomeless = true` 的稳定状态互斥 | 无家是住房关系的一种状态，不应创建单独无家 Component |
| 有住房的居民 NPC Entity | 普通 NPC Entity 的必需集合、`TownResidentComponent`、`TownHousingRelationComponent` | `NpcParentRelationComponent` | `IsHomeless = true` 与已确认有效的 `HomeTile` 互斥 | 住房位置属于居民关系，不属于居民能力或世界 registry 的复制字段 |
| World/Town 住房集合 | `TownHousingRegistryComponent` | 无 | 不得附着到单个 NPC Entity | 房间字典、无家集合和 revision 是集合范围状态，生命周期不同于任何单个 NPC |
| 具有通用 Entity GUID 的 NPC Entity | `EntityIdentityComponent`（来自共享实体域）与 NPC 组件集合 | 无 | 不得把 `EntityIdentityComponent` 静默替换为 `NpcIdentityComponent` | Entity GUID 与 NPC 实例 ID 的语义不同；是否建立显式映射属于 `BD-COMP-01` |
| 具有通用 Combat 生命的对象 | 通用 `HealthComponent` 或 NPC 专用 `NpcHealthComponent`，最终仅允许一个权威当前/最大生命源 | 另一种形式只能作为明确的只读映射 | `HealthComponent` 与 `NpcHealthComponent` 同时可写 | 当前 `src/Combat/HealthComponent.cs:3-14` 与 `src/Npc/NpcHealthComponent.cs:3-15` 重叠，owner 必须整合裁决 |

## 7. 组件拆分与合并决策

### 7.1 必须拆分

| 决策 | 拆分对象 | 原因 | 结果 |
|---|---|---|---|
| `DEC-COMP-01` | `NpcInstanceId` 与 `NpcSlot` | 一个是候选稳定实例身份，一个是旧数组索引；槽位可复用而实例身份不应随之改变 | `NpcIdentityComponent` + `NpcLegacySlotComponent` |
| `DEC-COMP-02` | `ai` 与 `localAI` | Version4 明确保存为两个数组，公开同步语义不同，恢复证据也不同 | `NpcBehaviorComponent` + `NpcLocalBehaviorStateComponent` |
| `DEC-COMP-03` | 目标关系与目标旧索引 | EntityReference 不能表达旧 Player/NPC 编码，int 也不能替代稳定关系 | `NpcTargetComponent` 内的关系字段与兼容字段分区保存 |
| `DEC-COMP-04` | 生命周期与生命 | 活动/计时和生命归零具有不同写入原因、边界和清理责任 | `NpcLifecycleComponent` + `NpcHealthComponent` |
| `DEC-COMP-05` | 居民能力、住房关系、住房 registry | 单个 NPC 能力、单个 NPC 房屋关系和 World/Town 集合索引具有不同范围与生命周期 | `TownResidentComponent` + `TownHousingRelationComponent` + `TownHousingRegistryComponent` |
| `DEC-COMP-06` | 父关系与生命值 | `realLife` 是关系证据，不足以证明共享生命；复制生命会产生双写 | `NpcParentRelationComponent` 单独保存关系 |

### 7.2 暂不合并

| 决策 | 候选合并对象 | 当前判断 | 保留理由 |
|---|---|---|---|
| `DEC-COMP-07` | `NpcDefinitionReferenceComponent` 与 `NpcBehaviorComponent` | 不合并 | 定义引用由类型/变体变化驱动，AI 槽位由活动状态驱动；需要不同字段子集的实体不能被迫携带另一组状态 |
| `DEC-COMP-08` | `NpcLifecycleComponent` 与 `NpcLifetimeComponent` | 不合并 | `RemainingTicks`、人口成本、despawn reason 的 owner 和语义没有由 Version4 当前证据闭合；合并会把人口/离场规则塞入生命周期核心 |
| `DEC-COMP-09` | `NpcHealthComponent` 与 `Combat.HealthComponent` | 不合并，等待 `BD-COMP-03` | 两者都可能成为当前/最大生命的权威源；在 owner 未裁决前合并或同时保留可写副本都会制造双写 |
| `DEC-COMP-10` | `TownHousingRelationComponent` 与 `TownHousingRegistryComponent` | 不合并 | 一个是单实体关系，一个是世界级索引；复制关系会造成房间事实漂移 |

### 7.3 不作为目标字段

`BehaviorKind`、`LastUpdatedTick`、`SelectedAtTick`、`TownHousingStatus`、每客户端复制游标和 `netUpdate/spawnNeedsSyncing` 不被静默加入上述权威字段。它们分别是分类/记录性元数据、派生状态候选、缓存或外部边界状态；如果未来需要保存，必须先声明新的 owner、失效条件和与权威字段的关系。

## 8. 不单独创建 Component 的对象

| 对象或成员 | 处理 | 理由 |
|---|---|---|
| `whoAmI` | 放入 `NpcLegacySlotComponent` | 它是旧槽位索引，不是稳定身份；单独创建“whoAmI Component”会混淆兼容和身份 |
| `NPC.netID` | 放入 `NpcDefinitionReferenceComponent` | tModLoader 公开语义和 Version4 字段位置都支持定义/变体 ID，不是 NPC 实例网络 ID |
| 单个 `bool`：`townNPC`、`friendly`、`homeless` | 分别放在居民或住房关系 Component | 单个布尔值没有独立生命周期；原子化会产生无意义的组合约束 |
| 单个 AI 槽位 | 保留在四槽位数组 Component 内 | Version4 的数组形状和槽位顺序是共同不变量；拆成四个 Component 会制造镜像关系 |
| 单个 `homeTileX` 或 `homeTileY` | 合并为可空 `HomeTile` 值 | 坐标对必须共同表达住房位置；缺少任一坐标时不得伪造有效房间 |
| 单个 target index | 保留在 `NpcTargetComponent` 的兼容字段区 | 它必须与目标类别、EntityReference 和 previous index 一起解释 |
| `NpcLifetimeComponent` 的人口成本和离场原因 | 当前不新增目标 Component | Version4 证据不足以把人口成本、剩余时间和生命周期阶段锁为一个权威概念 |
| `TownHousingResidentKey` | 作为 registry 的 key 值对象候选，不创建独立 Component | key 是索引关系的值，不是持续变化的实体状态；type/instance/dual 仍需裁决 |
| `TownRoomManager` 的房间扫描临时缓存 | 不进入 `TownHousingRegistryComponent` | 扫描缓存是短期计算状态，不能伪装成已提交的房间事实 |
| `NpcReplicationDirtyState` 的 per-client 字典、cursor、dirty flags | 不进入 NPC 权威 Component | 这是客户端相关缓存和边界状态；实例网络 ID与脏状态 owner 尚未确认 |
| `EntityIdentityComponent` 的 GUID | 不与 `NpcIdentityComponent` 合并 | 通用 Entity identity 与 NPC 领域实例 identity 可能有不同恢复、复制和生命周期 |

## 9. 当前 NLTX 组件覆盖

下表只报告当前文件实际存在的类型，不把目标设计当成已实现能力。`existing` 表示该类型/值对象已经存在且可以直接作为当前源码证据；`partial` 表示存在但与目标字段、所有权或命名不完全一致。

| 当前路径 | 当前类型 | 当前状态 | 目标映射 | 覆盖结论 |
|---|---|---|---|---|
| `src/Npc/NpcEntityIdentityComponent.cs:3-15` | `NpcEntityIdentityComponent` | `partial` | `NpcIdentityComponent` + `NpcLegacySlotComponent` | 身份和旧槽位被合并，必须拆分；当前不能直接视为目标组合 |
| `src/Npc/NpcInstanceId.cs:3-6` | `NpcInstanceId` | `existing` | `NpcIdentityComponent.NpcInstanceId` | 只有非零值对象约束，没有 Version4 稳定 ID 证据 |
| `src/Npc/NpcSlot.cs:3-6` | `Terraria.Npc.NpcSlot` | `existing` | `NpcLegacySlotComponent.LegacySlot` | 可表达已分配槽位；与 WorldStorage 同名类型冲突未解决 |
| `src/WorldStorage/NpcSlot.cs:1-3` | `Terraria.WorldStorage.NpcSlot` | `existing` | 暂不宣布归属 | 与 NPC 域同名，值对象不含同一不变量，owner unresolved |
| `src/Npc/NpcDefinitionReferenceComponent.cs:3-23` | `NpcDefinitionReferenceComponent` | `partial` | `NpcDefinitionReferenceComponent` | 主要字段存在；catalog revision 与 Version4 的对应关系未证实 |
| `src/Npc/NpcBehaviorStateComponent.cs:3-23` | `NpcBehaviorStateComponent` | `partial` | `NpcBehaviorComponent` | 有旧风格、动作、四槽位；额外 `BehaviorKind/LastUpdatedTick` 不直接纳入 |
| `src/Npc/NpcAiStateComponent.cs:3-26` | `NpcAiStateComponent` | `partial` | `NpcLocalBehaviorStateComponent` 的局部证据 | `Style`、`State0..3`、`Timer` 与行为状态重叠，owner 未闭合 |
| `src/Npc/NpcTargetComponent.cs:5-25` | `NpcTargetComponent` | `partial` | `NpcTargetComponent` | 已有 EntityReference、类别和选择 tick；缺旧 target/previous target 字段 |
| `src/Npc/TargetingComponent.cs:5-14` | `TargetingComponent` | `partial` | 不作为第二权威源 | 与 NPC target 重复，缺类别和旧编码语义 |
| `src/Npc/NpcLifecycleComponent.cs:3-12` | `NpcLifecycleComponent` | `partial` | `NpcLifecycleComponent` | 已有阶段和 despawn ticks；缺独立 active，`timeLeft` 对应关系未锁定 |
| `src/Npc/NpcLifetimeComponent.cs:3-29` | `NpcLifetimeComponent` | `partial` | 不直接纳入 12 个目标 Component | 人口成本、剩余时间和离场原因的边界仍属整合裁决 |
| `src/Npc/NpcHealthComponent.cs:3-15` | `NpcHealthComponent` | `partial` | `NpcHealthComponent` | 字段形状吻合；与 Combat health owner 冲突 |
| `src/Combat/HealthComponent.cs:3-14` | `HealthComponent` | `partial` | 与 `NpcHealthComponent` 互斥候选 | 通用生命类型存在，不能与 NPC 专用类型同时可写 |
| `src/Npc/NpcParentRelationComponent.cs:3-21` | `NpcParentRelationComponent` | `partial` | `NpcParentRelationComponent` | 已有稳定 ID 候选、旧槽位和 tick；Version4 只确认旧 `realLife` |
| `src/Town/TownResidentComponent.cs:3-29` | `TownResidentComponent` | `partial` | `TownResidentComponent` | 居民、友好、住房类别和能力已存在；Version4 完整资格条件仍为 partial |
| `src/Town/NpcHousingAssignmentComponent.cs:3-35` | `NpcHousingAssignmentComponent` | `partial` | `TownHousingRelationComponent` | 主要字段存在；`Status` 必须降为派生/兼容表达，不能与 bool/坐标双写 |
| `src/WorldSession/WorldGeneration/TownHousingRegistry.cs:5-14` | `TownHousingRegistry` | `partial` | `TownHousingRegistryComponent` | 房间、无家集合和 revision 已存在；当前类不是 Component，且 key 固定为 type |
| `src/WorldSession/WorldGeneration/TownHousingResidentKey.cs:3-5` | `TownHousingResidentKey` | `partial` | registry key 候选 | 当前只有 `NpcType`，type/instance/dual 未裁决 |
| `src/Npc/NpcReplicationDirtyState.cs:5-17` | `NpcReplicationDirtyState` | `partial` | 不进入 12 个权威 Component | per-client 字典和脏标记属于边界缓存，实例网络 ID也缺证据 |
| `src/Share/Entity/Components/EntityIdentityComponent.cs:5-17` | `EntityIdentityComponent` | `existing` | 与 `NpcIdentityComponent` 并存，是否映射待定 | 通用 GUID identity 已存在，不能静默当作 NPC identity |
| `src/Relationships/EntityReference.cs:5-10` | `EntityReference` | `existing` | `NpcTargetComponent.TargetReference`、父关系候选 | 只有 GUID + scope 值对象约束，不提供 NPC 旧索引语义 |

当前 `src` 覆盖结论：身份、定义、行为、目标、生命周期、生命、父关系、居民和住房均有局部类型，但存在重复字段、重名值对象和未裁决 owner；因此总体仍为 `nltxStatus: partial`。`dome/src` 中存在更完整的 NPC 原型类型，但它不是根目录 NLTX 已接线能力，不能替代本表的当前覆盖结论。

## 10. 组件级 evidence-gap

本节共有 9 个未闭合缺口。它们不被静默填充；其中会改变组件组成或共享 owner 的事项在第 11 节重复列为阻塞决策。

### `EG-COMP-01`：稳定 NPC 实例身份缺失

- 缺口：Version4 只有 `whoAmI` 旧槽位，当前没有被确认的稳定 `NpcInstanceId` 或 `PersistentEntityId`。
- 影响：`NpcIdentityComponent` 是否能独立存在、加载后是否保持同一值、父关系和目标关系使用何种 ID 都无法锁定。
- 证据状态：`missing`。
- 关联字段：`Entity.whoAmI`、`NpcInstanceId`、Entity GUID、持久化身份。

### `EG-COMP-02`：NPC 实例网络 ID 缺失

- 缺口：Version4 `NPC.netID` 是定义/变体 ID；当前未找到 NPC 实例网络 ID。`MessageBuffer.cs:1182-1210` 在 message 23 读取字段前立即 `break`，接收链不闭合。
- 影响：不能把 `netID` 放进身份 Component，也不能决定是否需要独立网络身份字段。
- 证据状态：`partial` / `missing`。
- 关联字段：`NPC.netID`、`netUpdate`、`spawnNeedsSyncing`、`NpcReplicationDirtyState`。

### `EG-COMP-03`：住房 key 模式未裁决

- 缺口：Version4/TownRoomManager 和当前 NLTX 都提供 type-level key 证据，但尚未排除 instance key 或 dual key。
- 影响：`TownHousingRegistryComponent.RoomsByResidentKey` 的 key 类型、持久化稳定性、一个 type 多居民的表达方式都会改变。
- 证据状态：`unresolved`。
- 关联字段：`TownHousingResidentKey.NpcType`、`HomeTile`、房间字典、无家集合。

### `EG-COMP-04`：Version4 三个空体 hook

- 缺口：`NPC.CheckDialogue`、`TrySyncingUniqueTownNPCData` 和 `DropEoWLoot` 在当前 Version4 中为空体或没有可用实现证据。
- 影响：不能从这些名称推断额外居民字段、独特同步字段或掉落相关字段属于本次 12 个 Component。
- 证据状态：`confirmed`（空体事实）/ `missing`（预期语义）。
- 关联字段：居民能力、网络边界、生命/清理边界；不新增字段。

### `EG-COMP-05`：完整参考缺少 NPC 成员级差异补证记录

- 缺口：完整参考源码确有相同文件和成员，但当前差异附录没有 NPC 成员级 `full-reference-supplemented` 记录。
- 影响：完整参考不能静默补写 Version4 私有默认值、调用链或成员语义。
- 证据状态：`partial`。
- 处理：只使用同路径、同成员作为缺口提示；没有对应 Version4 证据的内容保持 unresolved。

### `EG-COMP-06`：活动、计时、生命、父关系和清理写集未闭合

- 缺口：`active/timeLeft/life/lifeMax/realLife` 分布在多个路径和边界，清理时谁最后写入、父失效如何影响子实体和生命值尚未闭合。
- 影响：`NpcLifecycleComponent`、`NpcHealthComponent`、`NpcParentRelationComponent` 的组合不变式和清理生命周期可能改变。
- 证据状态：`partial`。
- 关联成员：`NPC.checkDead`、`NPCLoot`、`NewNPC`、`UpdateNPC`、`Main` 的 NPC 遍历和 `WorldFile` 恢复。

### `EG-COMP-07`：当前 NLTX 有重复/重叠状态类型

- 缺口：`NpcEntityIdentityComponent` 合并身份和槽位；`NpcBehaviorStateComponent` 与 `NpcAiStateComponent` 重叠；`NpcTargetComponent` 与 `TargetingComponent` 重叠；NPC/Combat health 重叠；住房 assignment 的 `Status` 与 bool/坐标也可能双写。
- 影响：无法把任一当前类型直接宣布为目标 Component owner。
- 证据状态：`confirmed`（源码重叠）/ `unresolved`（最终 owner）。
- 处理：本设计显式拆分和标记互斥，不把重复类型视为覆盖完成。

### `EG-COMP-08`：公共拆分约束文件缺失

- 缺口：当前 checkout 未找到 `D:\TRbackup\NLTX\约束\公共拆分约束.md`；本设计只能依据已读取的 `public-decomposition` 规则、ECS 文件组织约束和任务提示词。
- 影响：项目专属公共拆分细则无法作为额外证据，设计状态不能升为 `baseline`。
- 证据状态：`missing`。

### `EG-COMP-09`：本会话未运行验证

- 缺口：本会话没有运行构建、测试、验证程序或其他编译型命令。
- 影响：不能声称字段组合已编译、行为等价、数据恢复正确或当前覆盖已接线。
- 证据状态：`confirmed`（`verificationStatus: not-run`）。

## 11. 未决组件 owner

以下 4 项都是 `blocking-decision`。每项均设置 `crossSubsystemOwner: integration-review`，本文件只列候选方案和影响，不宣布最终 owner。

### `BD-COMP-01`：稳定身份、旧槽位、Entity GUID 与持久化身份

- 冲突字段：`NpcInstanceId`、`NpcSlot`、`EntityIdentityComponent.UUID`、候选 `PersistentEntityId`、父/目标关系引用。
- `crossSubsystemOwner: integration-review`。
- 候选方案：
  1. 以 `NpcInstanceId` 作为 NPC 稳定实例身份，`NpcSlot` 只作兼容索引，Entity GUID 和持久化 ID 保持独立映射。
  2. 以通用 Entity GUID 作为唯一实例身份，`NpcInstanceId` 仅作为 NPC 域别名或兼容值。
  3. 同时保留 NPC 实例 ID、Entity GUID 和持久化 ID，要求显式映射表和生命周期规则。
- 对 Component 组成的影响：方案 1 保持 `NpcIdentityComponent` 与通用 Entity identity 分离；方案 2 会削弱 NPC 专用 ID 的必要性；方案 3 会增加身份映射字段或关系，但能分别表达运行期、通用实体和跨加载身份。
- 当前不能定案的原因：Version4 没有稳定实例 ID或持久化 ID证据，`whoAmI` 可复用，当前两个 `NpcSlot` 类型的 owner 也未解析。

### `BD-COMP-02`：住房 key 和 registry owner

- 冲突字段：`TownHousingResidentKey.NpcType`、`RoomsByResidentKey`、`HomelessResidentKeys`、NPC 的 `HomeTile`、居民实例身份。
- `crossSubsystemOwner: integration-review`。
- 候选方案：
  1. 继续使用 type key；registry 表达每个 NPC type 的住房事实，兼容 Version4/TownRoomManager。
  2. 改用稳定 NPC instance key；每个居民实例拥有独立住房关系和反向索引。
  3. 使用 dual key：type key 保留兼容索引，instance key 作为现代权威关系，二者由明确 revision 关联。
- 对 Component 组成的影响：方案 1 允许 registry 维持当前字典形状但难表达同 type 多实例；方案 2 要求 `NpcIdentityComponent` 完整并使 registry key 与实体关系绑定；方案 3 会增加两个索引及一致性不变式。
- 当前不能定案的原因：Version4 的 type-level 证据充分，但没有足够成员级证据确认所有城镇居民是否按 type 唯一；稳定实例身份本身也缺证据。

### `BD-COMP-03`：生命、`realLife`、多段共享生命和死亡边界

- 冲突字段：`NPC.life`、`lifeMax`、`realLife`、`NpcHealthComponent`、`Combat.HealthComponent`、父子关系状态。
- `crossSubsystemOwner: integration-review`。
- 候选方案：
  1. NPC 专用 `NpcHealthComponent` 是 NPC 当前/最大生命的唯一权威；`realLife` 只在父关系中保存，不复制生命。
  2. 通用 `Combat.HealthComponent` 是唯一权威；NPC 只保留定义/关系引用，删除重复 NPC health 字段。
  3. 保留 NPC health 与通用 health 的明确单向映射，并另建共享生命聚合关系；禁止两个字段同时可写。
- 对 Component 组成的影响：方案 1 保留 `NpcHealthComponent`；方案 2 会让 NPC 组件组合依赖通用 health；方案 3 增加聚合关系和值同步约束，且需要明确父子清理语义。
- 当前不能定案的原因：Version4 确认 NPC `life/lifeMax/realLife`，当前 NLTX 同时存在两个 health 形状，但完整死亡、共享生命和清理写集尚未闭合。

### `BD-COMP-04`：实例网络 ID、同步标记与边界状态 owner

- 冲突字段：`NPC.netID`、`netUpdate`、`spawnNeedsSyncing`、message 23 的字段、`NpcReplicationDirtyState`、候选实例网络 ID。
- `crossSubsystemOwner: integration-review`。
- 候选方案：
  1. `NpcDefinitionReferenceComponent` 只保留定义/变体 ID；实例网络 ID和每客户端脏状态完全留在未决外部边界，不进入 12 个权威 Component。
  2. 为 NPC 增加独立的实例网络身份 Component，并让 `NpcReplicationDirtyState` 只保存边界缓存。
  3. 使用统一 Entity 网络身份，并在 NPC 定义引用中只保留类型/变体映射。
- 对 Component 组成的影响：方案 1 保持当前清单不增项但依赖整合边界；方案 2 会新增网络身份 Component，超出本次 12 个 Component；方案 3 依赖通用 Entity identity 与网络生命周期的共享 owner。
- 当前不能定案的原因：Version4 `netID` 明确是定义/变体语义，message 23 接收链在当前证据中不闭合，无法确认实例网络身份的实际来源。

## 12. 最终 Component 清单

以下是本次唯一的 12 个 Component 设计对象。状态统计为 `proposed: 7`、`partial: 5`；`partial` 仅表示当前 NLTX 有部分对应源码，不表示设计已成为可运行实现。

| # | componentId | name | status | entityScope | 当前覆盖 |
|---:|---|---|---|---|---|
| 1 | `NPC-COMP-IDENTITY` | `NpcIdentityComponent` | `proposed` | 单个 NPC Entity | `partial`；当前身份与槽位合并 |
| 2 | `NPC-COMP-LEGACY-SLOT` | `NpcLegacySlotComponent` | `proposed` | 单个 NPC Entity | `partial`；当前为身份组件字段/值对象 |
| 3 | `NPC-COMP-DEFINITION` | `NpcDefinitionReferenceComponent` | `partial` | 单个 NPC Entity | `partial`；同名组件已存在 |
| 4 | `NPC-COMP-BEHAVIOR` | `NpcBehaviorComponent` | `partial` | 单个 NPC Entity | `partial`；由 `NpcBehaviorStateComponent` 映射 |
| 5 | `NPC-COMP-LOCAL-BEHAVIOR` | `NpcLocalBehaviorStateComponent` | `proposed` | 单个 NPC Entity | `partial`；有重叠 AI 状态但无清晰 owner |
| 6 | `NPC-COMP-TARGET` | `NpcTargetComponent` | `partial` | 单个 NPC Entity | `partial`；缺旧索引字段 |
| 7 | `NPC-COMP-LIFECYCLE` | `NpcLifecycleComponent` | `proposed` | 单个 NPC Entity | `partial`；现有字段不足以闭合 active/timeLeft |
| 8 | `NPC-COMP-PARENT-RELATION` | `NpcParentRelationComponent` | `proposed` | 单个子 NPC Entity | `partial`；稳定父 ID 只有候选证据 |
| 9 | `NPC-COMP-HEALTH` | `NpcHealthComponent` | `partial` | 单个 NPC Entity | `partial`；与 Combat health 冲突 |
| 10 | `TOWN-COMP-RESIDENT` | `TownResidentComponent` | `partial` | 单个城镇居民 NPC Entity | `partial`；同名组件已存在 |
| 11 | `TOWN-COMP-HOUSING-RELATION` | `TownHousingRelationComponent` | `proposed` | 单个城镇居民 NPC Entity | `partial`；由 housing assignment 映射 |
| 12 | `TOWN-COMP-HOUSING-REGISTRY` | `TownHousingRegistryComponent` | `proposed` | World/Town 住房集合 | `partial`；现有 registry 不是目标 Component |

清单外的 `NpcReplicationDirtyState`、`NpcLifetimeComponent`、通用 `HealthComponent`、`TargetingComponent`、`TownHousingResidentKey`、`EntityIdentityComponent` 和 `EntityReference` 是当前源码证据或共享值对象，不被隐式增加为本次目标 Component；它们的交界关系已在第 8、9、10、11 节说明。

## 13. 最终声明

本文件是 Component-only Design。

本文件不定义 System、Query、Command、Event、Adapter、Projection、
调度顺序、运行时实现、测试计划或迁移计划。

所有 `status: proposed` 的 Component 都只是设计提案。
本文件不声明代码已创建、已迁移、行为等价或验证通过。

本文件只基于当前会话明确产生的研究报告、Version4 真实源码、同路径完整参考源码的有限补证、tModLoader v2026.07 公开文档和有限 ECS 结构参考生成。输出只写入本文件指定的设计路径；未修改 Version4、完整参考源码、tModLoader 文档、Space Station 14、NLTX 生产源码、测试源码、dome 源码或原研究报告。本会话未启动子代理、未创建其他会话、未运行构建或测试，`verificationStatus` 保持 `not-run`。
