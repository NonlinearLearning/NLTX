# NLTX 与旧 TerrariaServer 字段/属性迁移对比

**审查日期：** 2026-08-28  
**当前项目：** `D:\TRbackup\NLTX`  
**字段基线：** `D:\TRbackup\Version4物理删除了某些文件\Terraria`  
**补充对照：** `D:\TRbackup\无任何删减通过编译`（原始单体项目，用于确认旧 God Object 的完整形态）

## 结论

从“旧主要大文件的字段/属性是否已经在新框架中拥有等价状态、定义、快照或协议归属”这一口径看：

- **字段/属性级保守迁移度：约 30.9%（已从统计口径剔除 4 个 ID 字段/属性）**。
- **字段分类/归属覆盖度：约 100%（仅 Item 映射表）**，但分类不等于实现；`Definition`、`System`、`Component`、`Compatibility` 只是目标 owner。
- **核心服务端字段状态覆盖度：约 45%–55%**。排除 Main/Player 中明确的客户端 UI、图形、音频、社交和输入表现字段后，服务器相关字段的可用状态更高，但仍受未完成静态表、AI、掉落、完整事件和协议分支限制。

因此不能写成“字段已经迁移 70%”。当前准确描述是：**字段模型已开始完成从 God Object 到 Definition/Component/Snapshot 的重组，但全量字段语义仍只有约三成落地。**

## 1. 统计口径

### 1.1 旧字段声明如何计数

对 Version4 的 11 个主要旧文件进行可复现的词法声明扫描，并在扫描结果上应用 ID 排除规则：

```powershell
Select-String -Pattern '^\\s*(public|private|protected|internal)...;'
Select-String -Pattern '^\\s*(public|private|protected|internal).+\\{\\s*$'
```

第一条统计字段、常量、静态表和数组字段；第二条统计属性/属性块的声明行。它不是 Roslyn 语义树计数，因此不能识别所有表达式体属性和跨行声明；为避免伪精确，报告把它称为“字段/属性级声明基线”，并同时保留源文件 SHA-256。

| 旧文件 | 行数 | 字段 | 属性块 | 合计 | ID 排除 | 剩余声明 | SHA-256（前 16 位） |
|---|---:|---:|---:|---:|---:|---:|---|
| `NPC.cs` | 79,688 | 360 | 0 | 360 | 1 | 359 | `29869B0390D85CB0` |
| `WorldGen.cs` | 73,355 | 230 | 46 | 276 | 0 | 276 | `A06A8463E39EA065` |
| `Main.cs` | 14,735 | 685 | 1 | 686 | 1 | 685 | `844A862B4EF863FF` |
| `Projectile.cs` | 54,799 | 120 | 9 | 129 | 1 | 128 | `97C3D68586AEF862` |
| `Player.cs` | 26,856 | 1,046 | 6 | 1,052 | 0 | 1,052 | `AF4C868BE773599E` |
| `Item.cs` | 48,927 | 148 | 3 | 151 | 0 | 151 | `932AA78145344BF9` |
| `Recipe.cs` | 98 | 25 | 1 | 26 | 0 | 26 | `FFF0F9715D8029B3` |
| `MessageBuffer.cs` | 3,365 | 21 | 1 | 22 | 1 | 21 | `0F474225FA9D98C5` |
| `NetMessage.cs` | 2,721 | 12 | 0 | 12 | 0 | 12 | `87B596BD8467B9C4` |
| `Chest.cs` | 1,299 | 23 | 2 | 25 | 0 | 25 | `14EAF2C87C3761E2` |
| `Wiring.cs` | 2,781 | 30 | 1 | 31 | 0 | 31 | `3D4D4C75B7A00220` |
| **合计** |  |  |  | **2,770** | **4** | **2,766** |  |

旧 `无任何删减通过编译` 基线中的同文件更完整、更大（例如 `NPC.cs` 97,097 行、`WorldGen.cs` 88,439 行、`Main.cs` 67,447 行），说明 Version4 已经物理裁剪过；本报告选择 Version4 是因为当前迁移矩阵和字段审计以 Version4 为 Oracle，避免把已删除的客户端/无关声明重复计算。

### 1.2 当前字段模型

当前 `src`（排除 `src/**/Build`）实测为 **1,325 个 `.cs`、81,457 行**。词法扫描得到约 **477 个 public field-like 声明、694 个 public property-like 声明**。这个数字不能直接与旧 2,766 一一相除：新模型大量使用 `private` 字段、`readonly record struct` 主构造参数和小型 component，且同一个旧字段通常被拆成 definition + runtime component + snapshot 三个不同对象。

### 1.3 ID 字段的处理边界

本次“去掉 ID 类型的字段和属性”只作用于迁移百分比的统计分母和加权分子，不修改旧项目或当前项目源码。身份字段仍是实体寻址、网络复制、持久化和快照关联的必要契约；例如当前 `PlayerIdentityComponent`、`NpcDefinitionComponent`、`NpcReplicationSnapshot`、`ChestSnapshot` 中的身份字段必须保留。若要物理删除这些字段，需要另行评估协议兼容性、存档格式和实体生命周期，不属于本次报告。

| 旧文件 | 声明 | 排除原因 |
|---|---|---|
| `NPC.cs` | `public int netID;` | NPC 网络/定义身份编号 |
| `Main.cs` | `public static int worldID => ActiveWorldFileData.WorldId;` | 世界身份编号属性 |
| `Projectile.cs` | `public int identity;` | 投射物实例身份编号 |
| `MessageBuffer.cs` | `public int whoAmI;` | 连接/玩家槽位身份编号 |

当前实际承载字段的典型类型包括：

- NPC：`NpcDefinitionComponent`、`NpcLifecycleComponent`、`NpcBehaviorStateComponent`、`NpcStateSnapshot`、`NpcReplicationSnapshot`。
- Player：`PlayerIdentityComponent`、`PlayerLifecycleComponent`、`PlayerInputComponent`、`PlayerControlStateComponent`、`HealthComponent`、`ManaComponent`、`VelocityComponent`。
- Projectile：`ProjectileDefinition`、`ProjectileDefinitionComponent`、行为状态组件、生命周期/复制快照。
- Item：`ItemDefinition`、`ItemUseDefinition`、`ItemCombatDefinition`、`ItemEquipmentDefinition`、`ItemStackComponent`、`ItemWorldStateComponent`、`ItemInstanceStateComponent`、Inventory/Equipment/WorldItem snapshots。
- World/Wiring/Chest：`WorldRuleState`、`WorldClock`、`WorldGrid`、`WireNetworkComponent`、`ChestSnapshot` 和 revision/ownership 字段。

## 2. 按旧大文件对比

ID 排除规则按“声明名 + 语义”执行：排除 `Id`/`ID`、`Uid`/`Uuid`/`UUID`、`NetId`/`netID`、`Identity`/`identity`、`whoAmI` 以及明确的 `worldID`、`playerId`、`npcId` 等身份编号。仅因类型是 `int`，或名称为普通 `type`、`index`、`slot`，不会自动排除。当前基线实际命中的 4 项为：`NPC.netID`、`Main.worldID`、`Projectile.identity`、`MessageBuffer.whoAmI`。`WhoAmIToTargetingIndex` 是表达式体属性，未被原始属性块扫描计入，因此不改变本次分母。评分含义：`0` 表示没有可确认的等价字段状态；`100` 表示该文件的字段/属性语义已经有当前 owner、读写生命周期和必要快照/验证证据。客户端专属字段不因“没有迁移”而算服务器缺陷，但在“旧文件全体字段”分数中仍不计入完成。仅有 LegacyReference、注释、字段名称或初始归属不算实现。

| 旧文件 | 旧字段/属性形态 | 当前承载 | 已确认迁移 | 主要缺口 | 估算 |
|---|---|---|---|---|---:|
| `NPC.cs` | 360；大量 `active`、zone/spawn flags、AI 临时字段、`ai[]`/交互数组、静态 NPC 表 | `NpcDefinitionComponent`、`NpcLifecycleComponent`、`NpcBehaviorStateComponent`、`NpcTargetComponent`、`NpcSpawnStateComponent`、`NpcStateSnapshot` | 身份、定义 ID、Faction/Category、Health/Facing、Target、Behavior、SpawnSource、Difficulty、TimeLeft、DespawnReason、Home/Segment 和部分 replication | 完整 `NPCID.Sets` 表、所有 zone/spawn flags、AI family 状态、Boss/invasion 表、loot state、对话/房屋、客户端缓存 | **30%** |
| `WorldGen.cs` | 276；秘密种子对象、全局 generation flags、Tile/Wall 表、计数器、树/结构配置和事件 | `WorldGenerationStateComponent`、`WorldGenerationPipeline`、各类 registry/query/system、`WorldRuleState` | stage 状态、确定性请求、Terrain/Cave/Ore/Tree/Structure/Liquid 的部分输入字段、命令序列、trace/restart | 大量静态表和生成器全局状态、完整 biome/structure 参数、frame/side-effect 状态；differential 仍失败 | **28%** |
| `Main.cs` | 686（Version4 已裁剪）；世界元数据、时间/天气/事件、实体数组、静态设置与客户端矩阵/UI | `WorldMetadata`、`WorldClock`、`WorldRuleState`、Dome entity stores、`DomeServer`、snapshots | 服务端时间、天气、侵袭/Slime Rain、实体 ownership、Tick phase、replication revision、持久化字段 | 完整 initializer 静态表、`Main.rand` 全局状态、任意 delayed process、完整数组兼容字段；大量客户端字段被排除 | **25%** |
| `Projectile.cs` | 129；`type`、`owner`、`ai[]`/`localAI[]`、damage、penetrate、immunity、tile/water flags、old-position/render 字段 | `ProjectileDefinition`、definition component、behavior state、lifecycle/replication components | Type/Behavior/Damage/Lifetime/Collider/Friendly/Hostile/Penetration、owner/identity、有限值校验、部分 AI 投影 | 完整 `aiStyle` 字段族、local immunity 数组、reflection/bounce/liquid/slopes、VFX、完整 hostile-player hit state | **38%** |
| `Player.cs` | 1,052；输入、位置/速度、生命/法力、背包、装备、Buff、坐骑/翅膀/高尔夫/社交和客户端字段 | Player components、PlayerPersistentState、input batch、inventory/equipment snapshots、server protocol | Identity、active/dead/respawn、input/control、position/velocity、health/mana、inventory/use、部分 equipment/buff/persistence/replication | 大量专用机制字段、完整装备计算、全 Buff、Mount/Wing/Rope/Fishing/Golf/Emote、客户端 UI 和社交字段 | **30%** |
| `Item.cs` | 151；定义属性和实例状态混在一起，含 SetDefaults、前缀、装备、商店、绘制和 Tooltip 字段 | `ItemDefinition` + item definition records + instance/world/equipment components | `docs/migrations/item-ecs-member-mapping.md` 对 148 行建立了归属：Definition 96、System 20、Component 3、Compatibility 8、Deferred 21；实际 runtime 已覆盖部分使用/恢复/库存/掉落/装备 | 完整 SetDefaults/ItemID 表、全部 prefix/equipment/shop/shimmer、UI/Tooltip/visual；映射表说明归属不等于行为证据 | **55%** |
| `Recipe.cs` | 26；配方、`requiredItem[]`、accepted groups、环境条件、静态配方表 | 未发现同等级 Recipe component/system/registry | 无可确认的完整字段 owner | 配方表、配方条件、消耗、附近箱子、Shimmer/decraft 全部未形成等价域 | **0%** |
| `MessageBuffer.cs` | 22；读写 byte buffer、reader/writer、连接身份、spam/状态和事件 | `Isolation.MessageBuffer`、inbound envelope、`DomeNetworkUpdateBridge` | framed buffer、长度/分片/序列、输入入队、unsupported 诊断 | 旧 162 类消息对应字段、Netplay 状态机、直接 `Main`/`NetMessage` 副作用 | **60%** |
| `NetMessage.cs` | 12；静态 buffer、Tile 压缩 scratch、当前死亡/声音/复仇 marker | `NetMessage` isolation adapter、`TerrariaPacketCodec`、outbound envelope、replication cursors | 有序 frame、payload ownership、PVS、snapshot 投影、ignore-client 和部分 revision 字段 | 全部 legacy SendData message 字段、压缩 scratch 语义、客户端状态和完整包族 | **60%** |
| `Chest.cs` | 25；槽位数组、坐标/index、maxItems、opener/use、名称、frame 和客户端动画 | `ChestSnapshot`、Chest components/commands/systems、replication | 40 槽、坐标、opener、slots、revision、section、locked、rename/persistence/reconnect | 银行/商店、旧 frame/animation 字段、完整客户端回写 | **70%** |
| `Wiring.cs` | 31；静态队列、线颜色、pump/mechanism 数组、gate/lamp/teleport scratch | `WireNetworkComponent`、wiring commands/components/systems、liquid transfer | wire mask、稳定遍历、预算、门/灯/执行器/泵/逻辑门的部分状态和命令 | 传送器/炮/Hopper/PixelBox、旧队列全部字段、生成期和客户端副作用 | **45%** |

## 3. 加权结果

按排除 ID 后的字段/属性声明数加权，不按当前文件数量加权：

```text
raw declarations = 2,770
excluded ID declarations = 4
denominator = 2,766 non-ID field/property declarations
numerator = 85,477 weighted points
field/property migration = 85,477 / (2,766 × 100) = 30.9%
```

各文件权重和贡献如下：

| 文件 | 非 ID 声明数 | 权重 | 估算 | 加权贡献（百分点） |
|---|---:|---:|---:|---:|
| NPC.cs | 359 | 13.0% | 30% | 3.9 |
| WorldGen.cs | 276 | 10.0% | 28% | 2.8 |
| Main.cs | 685 | 24.8% | 25% | 6.2 |
| Projectile.cs | 128 | 4.6% | 38% | 1.8 |
| Player.cs | 1,052 | 38.0% | 30% | 11.4 |
| Item.cs | 151 | 5.5% | 55% | 3.0 |
| Recipe.cs | 26 | 0.9% | 0% | 0.0 |
| MessageBuffer.cs | 21 | 0.8% | 60% | 0.5 |
| NetMessage.cs | 12 | 0.4% | 60% | 0.3 |
| Chest.cs | 25 | 0.9% | 70% | 0.6 |
| Wiring.cs | 31 | 1.1% | 45% | 0.5 |
| **合计** | **2,766** | **100%** |  | **约 30.9%** |

## 4. 关键字段迁移样例

### 4.1 已完成结构重组但不是同名复制

| 旧字段族 | 当前字段族 | 判定 |
|---|---|---|
| `NPC.active`, `timeLeft`, `netID`, `target`, `ai[]` | `NpcLifecycleComponent`, `NpcDefinitionComponent`, `NpcTargetComponent`, `NpcBehaviorStateComponent`, `NpcStateSnapshot` | 状态已拆分；`ai[]` 只有有限 behavior 投影，不是全量 AI 等价 |
| `Player.controlLeft/controlRight/controlJump` | `PlayerInputComponent` + typed input batch | 输入字段已进入 ECS；完整旧 `Player.Update` 字段仍缺失 |
| `Player.statLife/statMana` | `HealthComponent`, `ManaComponent`, persistent/replication snapshots | 核心生命/法力已拥有明确 owner，专用修正字段未全量迁移 |
| `Projectile.type/owner/damage/penetrate` | `ProjectileDefinition`, owner/identity/lifecycle components | 领域字段已建模；完整 localAI/immunity/reflection 仍缺 |
| `Item.type/stack/prefix/paint` | `ItemStack`, `ItemInstanceStateComponent`, `ItemDefinition` | 实例与定义分离；全部 SetDefaults 和 prefix table 未完成 |
| `Chest.item[]/x/y/name` | `ChestSnapshot.Slots/TileX/TileY` + revision/section | 快照和持久化字段比旧数组更严格，但银行/商店不在范围 |
| `Wiring._wireList/_toProcess` | `WireNetworkComponent` mask + typed commands | 运行时状态改为值类型 mask；复杂旧 scratch 队列未完全对应 |

### 4.2 仍然只是 LegacyReference 或归属声明

- `src/Terraria.Dome.Protocol.V1456/LegacyReference/MessageBuffer.cs` 和 `NetMessage.cs` 由项目文件 `Compile Remove`，是行为 Oracle，不是运行时字段迁移。
- `src/Terraria.WorldFile.V319/LegacyReference/WorldFile.cs` 同样不参与编译；WLD reader/model 承载的是文件格式字段，不是完整旧 `WorldFile` 静态 API。
- `docs/migrations/item-ecs-member-mapping.md` 明确写出“归属不等于行为已经迁移”，并将 21 个成员标记为 Deferred。
- Main 成员矩阵将 30 个责任族分为 accepted/deferred/excluded；其中 accepted 是服务端切片证据，不是 Main 全量字段等价。

## 5. 证据与限制

### 已核对的证据

- `docs/migrations/item-ecs-member-mapping.md`：Item 字段/属性归属和 148 行成员映射。
- `docs/migrations/player-legacy-behavior-map.md`：Player identity/input/movement/vitals/inventory/persistence/replication 的 owner 分类，以及专用机制和客户端字段的边界。
- `docs/research/2026-08-18-npc-migration-coverage.md`：NPC definition、target、behavior、movement、death/loot 的 verified/partial/excluded 边界。
- `docs/protocol/message-buffer-net-message-status.md`：MessageBuffer/NetMessage framed path 已覆盖，但完整 legacy message parity 仍 partial。
- `docs/worldgen/worldgen-parity-report.md`：WorldGen differential 仍存在数百万 Tile mismatch，`canRemoveLegacyWorldGen=false`。
- 当前实际代码：`NpcStateSnapshot`、`ItemDefinition`、`ItemWorldStateComponent`、`ProjectileDefinition`、`PlayerInputComponent`、`ChestSnapshot`、`WireNetworkComponent`。

### 不能从本报告推出的结论

- 不能用当前 477 个 field-like + 694 个 property-like 去除以旧 2,766 得出完成率；新旧对象粒度不同，且新组件字段大量为 `private` 或 record 参数。2,766 是从原始 2,770 中剔除 4 个身份声明后的统计分母。
- 不能因为 Item 148 行全部有分类就称 Item 字段全部完成；其中 Deferred 明确保留，且映射文件自己声明分类不是实现证据。
- 不能因为字段被映射到 `Definition` 就认为默认值、SetDefaults、持久化和网络投影都正确；需要对应运行时 verifier。
- 不能把客户端字段删除算成服务端字段迁移；它们是 scope exclusion，必须和服务端缺口分开报告。

## 最终判定

截至 2026-08-28，字段/属性迁移应标记为：

> **Field/property model migration: partial, approximately 30.9% by non-ID declaration-weighted estimate (2,766 declarations; 4 ID declarations excluded).**  
> **Server-relevant field ownership: approximately 45%–55% after excluding client-only state.**  
> **Item has the most explicit field mapping, while Recipe has effectively no equivalent domain.**  
> **NPC/WorldGen/Main/Player/Projectile remain incomplete because static tables, AI state, specialized mechanics, lifecycle side effects, and full persistence/protocol fields are not yet equivalent.**

下一阶段最有价值的抓手不是继续增加同名字段，而是为 NPC/Projectile/Player/Main 建立 Roslyn 级字段清单，逐项绑定 `Definition -> Component -> Snapshot -> Persistence/Protocol`，并把 `default value`、`mutability`、`lifecycle owner`、`serialization`、`verification evidence` 五列补齐。没有这五列，字段数量增长只会制造迁移假象。
