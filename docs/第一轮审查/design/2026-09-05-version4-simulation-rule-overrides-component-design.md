# SimulationRuleOverrides Component Design

## 1. 设计元数据

| 字段 | 值 |
| --- | --- |
| `subsystemId` | `SimulationRuleOverrides` |
| `taskNumber` | `08` |
| `sourceReport` | `D:\TRbackup\NLTX\docs\research\2026-09-05-version4-simulation-rule-overrides-public-decomposition.md` |
| `outputDesignPath` | `D:\TRbackup\NLTX\docs\design\2026-09-05-version4-simulation-rule-overrides-component-design.md` |
| `designScope` | `component-only` |
| `designStatus` | `decision-required` |
| `evidenceStatus` | `partial` |
| `nltxStatus` | `missing` |
| `verificationStatus` | `not-run` |
| `selectionMethod` | 当前会话明确产生并提供的唯一研究报告路径；未扫描 `docs\research` 选择替代文件 |
| `subagentPolicy` | 本会话未启动子代理 |
| `componentCount` | `5` 个 proposed Component |

`sourceReport` 的文件实际存在，报告开头的 `subsystemId`、`taskNumber` 和 `reportPath` 与本
文件的输入路径一致。研究报告中的 `nltxStatus: partial` 与本次直接检查所得的状态不一致：当前
NLTX 只有相邻世界状态、协议 DTO、内容表现索引和不透明兼容读取，没有本子系统的权威
Creative 状态闭环，因此本设计采用直接检查的 `nltxStatus: missing`，并保留该状态差异为
`evidence-mismatch`。

## 2. 设计范围与排除范围

本文件只整理 `SimulationRuleOverrides` 的 Component 组成、字段、状态分类、生命周期、
实体组合、ID 归类和组件 owner 决策。所有新增 Component 均为 `status: proposed`，不表示
已有实现，也不表示已经创建源码文件。

本文件包含：

- 世界作用域的 Creative/Journey 覆写权威状态；
- 玩家作用域的 Creative 覆写权威状态；
- 按规则键保存的当前/默认权限状态；
- 与权威状态分离的不可变世界快照和玩家快照；
- 字段的权威、派生、缓存、快照、兼容分类；
- Component 的实体范围、初始化、恢复、清理和组合约束；
- 当前 NLTX 组件的 existing/partial 映射和组件级证据缺口。

本文件不把单个 Power、UI 元数据、按钮、网络 DTO、WLD 原始字节、外部会话身份或下游
临时缓存升格为 Component。除最后的强制声明外，本文件不定义 System、Query、Command、
Event、Adapter、Projection、调度、调用图、主循环、网络发送流程、存档流程、客户端渲染
流程、focused verifier、测试计划、迁移计划或可运行 C# 实现。

## 3. 组件设计依据

### 3.1 证据优先级和重新定位结果

本设计使用 Version4 作为真实行为和字段语义的首要证据；完整可编译参考只补充 Version4
已经存在的同路径、同类型、同成员缺口；tModLoader 只交叉核对公开生命周期和边界；
Space Station 14 只参考组件粒度和实体组合方式。研究报告中的旧行号不作为充分证据，以下
路径和行号均由本会话重新读取。

| `sourceId` | 实际读取材料和重新定位结果 | 仅用于本设计的结论 | 状态 |
| --- | --- | --- | --- |
| `v4-manager` | `D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowerManager.cs:35-64`、`:66-85`、`:89-110`、`:114-197` | Power 注册顺序、`ushort PowerId`、配置权限默认/当前值、重置、世界持久化入口、验证入口和加入玩家同步入口 | `confirmed` |
| `v4-power-fields` | `D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs:19-184`、`:215-306`、`:309-737` | per-player/world-shared 作用域、toggle/slider 状态、默认值、缓存、持久化声明和具体 Power 的字段形状 | `confirmed` / `partial` |
| `v4-time-difficulty` | `D:\TRbackup\Version4\Terraria\Main.cs:3159-3178`、`:11362-11375` | 时间速率、冻结时间和难度倍率是下游读取的有效值；Power 自身输入和下游派生值不能混成同一权威字段 | `confirmed` |
| `v4-weather-ecology` | `D:\TRbackup\Version4\Terraria\Main.cs:12050-12080`、`:13015-13046`；`D:\TRbackup\Version4\Terraria\WorldGen.cs:59400-59417`、`:59850-59860`、`:60302-60306` | 风/雨冻结、生态传播开关和世界更新率的消费边界；`AllowedToSpreadInfections` 是临时派生值 | `confirmed` |
| `v4-player-npc` | `D:\TRbackup\Version4\Terraria\Player.cs:15209-15212`、`:19883-19885`、`:20027-20029`；`D:\TRbackup\Version4\Terraria\NPC.cs:264-269`、`:668-674`、`:5802-5807` | Godmode 调用点被注释；FarPlacement 和 per-player spawn-rate 有读取点；Godmode 的运行时效果不能据此确认 | `partial` |
| `v4-persistence-join` | `D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:3278-3285`、`:3513-3526`；`D:\TRbackup\Version4\Terraria\MessageBuffer.cs:592-594` | 实际 WorldFile 位于 `Terraria.IO`；世界验证/保存/加载和加入玩家同步都在组件之外的边界读取组件数据 | `confirmed` |
| `v4-network` | `D:\TRbackup\Version4\Terraria.GameContent.NetModules\NetCreativePowersModule.cs:9-29`；`D:\TRbackup\Version4\Terraria.GameContent.NetModules\NetCreativePowerPermissionsModule.cs:9-24` | Power ID 是兼容协议键；权限接收体在 Version4 为空，不能由该文件确认权限 writer | `partial` |
| `full-reference-supplement` | `D:\TRbackup\无任何删减通过编译\Terraria.GameContent.Creative\CreativePowers.cs:798-890`、`:1013-1158`、`:1182-1397`、`:1399-1589`、`:1705-1888`、`:1889-2086`；`D:\TRbackup\无任何删减通过编译\Terraria.GameContent.Creative\CreativePowerManager.cs:205-282` | 补充玩家保存值、1..24 时间映射、0.5..3 难度映射、风雨副作用和 0.1..10 刷怪率映射；不改写 Version4 当前事实 | `full-reference-supplemented` |
| `tmodloader` | `D:\TRbackup\tmodloader-api-docs-stable\index.html:8`、`:29`；`D:\TRbackup\tmodloader-api-docs-stable\class_mod_system.html#aef069f82d40878596ad922ccdf4d03ec`、`#a293da73b3fc469aee8b830d1f2b84c88`、`#ac9071667bf525e992915380572d2e4c5`、`#a12097aab73db65bd17b2bde505e1022d`、`#a926129e278ac9c460685bd2a326cd998`；`D:\TRbackup\tmodloader-api-docs-stable\class_mod_player.html#a99b4decd672a09196ad922ccdf4d03ec`、`#af5ed4f9aec0aafb9197fc93b62737264`、`#aeaf390061d5840291f92c6fd929dc599`、`#acf05b6b54e5217448160275022d0e98b` | 公开的世界 Tick、世界数据、玩家数据和客户端/服务端边界；不证明 Terraria 私有 Power 语义 | `partial` |
| `ss14` | `C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\GameTicking\Components\GameRuleComponent.cs:8-59`、`C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\GameTicking\Components\ActiveGameRuleComponent.cs:1-8`、`C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\GameTicking\Components\EndedGameRuleComponent.cs:1-8`；`C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Server\GameTicking\Rules\GameRuleSystem.cs:19-109`；`C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\Administration\AdminCommandPermissions.cs:8-75` | Component 只保存内聚状态、生命周期状态可分离、权限按明确键表表达；无 Terraria 私有规则的直接对应证据 | `partial` |
| `nltx-content` | `D:\TRbackup\NLTX\src\Content\ContentPresentationIndex.cs:5-58` | 仅有 Creative 排序和其他表现索引，不是规则覆写状态 | `confirmed`（表现边界） |
| `nltx-world` | `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\World\WorldRuleState.cs:5-97`、`:99-237`；`D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\World\WorldRuleSnapshot.cs:1-3`；`D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\WorldGeneration\Components\WorldRuleSnapshotComponent.cs:5-89` | 已有世界运行状态、时间投影和世界生成输入各自有明确范围，不能替代本设计的 Creative authority | `existing` / `partial` |
| `nltx-protocol` | `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Protocol.V1456\Packets\CreativePowerModulePacket.cs:3-9`、`D:\TRbackup\NLTX\dome\src\Terraria.Dome.Protocol.V1456\Packets\CreativePowerPermissionModulePacket.cs:3-6`、`D:\TRbackup\NLTX\dome\src\Terraria.Dome.Protocol.V1456\Packets\JourneySpawnRatePacket.cs:3-5`；`D:\TRbackup\NLTX\dome\src\Terraria.Dome.Protocol.V1456\Packets\TerrariaPacketCodec.cs:83-211`、`:647-669`；`D:\TRbackup\NLTX\dome\src\Terraria.Dome.Protocol.V1456\Session\TerrariaSession.cs:243-418` | 现有 DTO/解码只说明外部输入边界；权限包当前解码后丢弃，Power 14 只做玩家 slot ownership 检查 | `existing` / `partial` |
| `nltx-wld` | `D:\TRbackup\NLTX\dome\src\Terraria.WorldFile.V319\Format\WldCreativePowerReader.cs:7-21` | Creative Power section 被保留为 opaque compatibility data，不是行为组件 | `existing` |

### 3.2 路径和版本差异记录

任务提示中的 `D:\TRbackup\Version4\Terraria\WorldFile.cs` 实际不存在；真实文件为
`D:\TRbackup\Version4\Terraria.IO\WorldFile.cs`。这是 `evidence-mismatch`，不是静默
修正。tModLoader 首页实际标题为 `tModLoader: Main Page`，页眉版本为 `v2026.07`；
`PreUpdateWorld` 的实际成员锚点为
`D:\TRbackup\tmodloader-api-docs-stable\class_mod_system.html#ac9071667bf525e992915380572d2e4c5`。

完整参考中出现、但 Version4 `CreativePowerManager` 没有的玩家持久化成员和
`GetPowerId<T>` 不写入 Version4 当前 Component 事实。它们只作为 `full-reference-supplemented`
证据，且其最终玩家持久化 owner 仍为 `decision-required`。

## 4. Version4 成员到 Component 归属表

下表按真实字段、属性、接口成员、方法入口和直接消费点列出归属。`无（非 Component）`
表示该成员被明确保留为静态兼容资料、边界资料、表现资料或下游临时派生值；这不是遗漏。

| 成员 | 声明类型 | 字段含义 | 权威/派生/缓存/快照/兼容 | 生命周期 | proposed Component | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|---|
| `_powersById`、`_powersByName` | `CreativePowerManager` | 按 `PowerId` 和 server-config name 查找 Power | 兼容注册表；不是实体状态 | 进程初始化至卸载 | 无（非 Component） | `confirmed` | `D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowerManager.cs:21-25`、`:35-64` |
| `_powersCount`、`PowerTypeStorage<T>.Id` | `CreativePowerManager` / nested storage | 注册顺序和兼容 `ushort PowerId` 分配 | 兼容字段；不是稳定领域实体 ID | 注册初始化 | 无（非 Component） | `confirmed` | `D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowerManager.cs:10-17`、`:35-49` |
| `Register<T>`、`Initialize` | `CreativePowerManager` | 创建并注册 15 个 Creative Power | 生命周期/兼容注册行为；不保存当前值 | 进程初始化一次 | 无（非 Component） | `confirmed` | `D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowerManager.cs:35-50`、`:89-112` |
| `ServerConfigName` | `ICreativePower` | 配置解析使用的字符串键 | 兼容配置键；不进入权威运行时状态 | 注册至卸载 | 无（非 Component） | `confirmed` | `D:\TRbackup\Version4\Terraria.GameContent.Creative\ICreativePower.cs:7-21`；`D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowerManager.cs:75-85` |
| `CurrentPermissionLevel` | `ICreativePower` | 当前规则权限级别 | 权威权限状态 | 注册、配置更新、重置、会话同步 | `RuleOverridePermissionStateComponent` | `confirmed` | `D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowerManager.cs:42-44`、`:78-85`、`:118-124` |
| `DefaultPermissionLevel` | `ICreativePower` | 重置时恢复的默认权限级别 | 权威默认策略状态 | 注册、配置更新、重置 | `RuleOverridePermissionStateComponent` | `confirmed` | `D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowerManager.cs:42-44`、`:78-85`、`:118-121` |
| `_defaultToggleState` | `APerPlayerTogglePower` | per-player toggle 新玩家/重置默认值 | 静态默认策略；不是当前状态 | Power 构造至卸载 | 无（由 `PlayerRuleOverrideStateComponent` 字段承接结果） | `confirmed` | `D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs:19-58`；FarPlacement 构造 `D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs:325-332` |
| `_perPlayerIsEnabled` | `APerPlayerTogglePower` | 每个玩家的 toggle 状态数组 | 权威状态/旧实现缓存混合 | 玩家加入、重置、离开 | `PlayerRuleOverrideStateComponent` | `partial` | `D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs:28-46`、`:50-82` |
| `_sliderDefaultValue` | `APerPlayerSliderPower` | per-player slider 默认 normalized 值 | 静态默认策略 | Power 构造、玩家初始化 | 无（由 `PlayerRuleOverrideStateComponent` 字段承接结果） | `confirmed` | `D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs:88-110`；SpawnRate 构造 `D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs:698-707` |
| `_cachePerPlayer` | `APerPlayerSliderPower` | 每个玩家的 slider normalized 缓存 | 权威输入的旧实现缓存；不保留为数组镜像 | 玩家加入、更新、重置、离开 | `PlayerRuleOverrideStateComponent` | `partial` | `D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs:96-123`、`:154-181` |
| `_sliderCurrentValueCache`（shared） | `ASharedSliderPower` | 世界共享 slider normalized 当前值 | 权威 normalized 输入 | 世界初始化、更新、恢复、清理 | `RuleOverrideStateComponent` | `partial` | `D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs:259-306`；具体世界 Power `D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs:406-522`、`:524-564` |
| `_sliderCurrentValueCache`（per-player） | `APerPlayerSliderPower` | 当前玩家的 slider normalized 输入 | 权威玩家输入；旧缓存不直接暴露 | 玩家初始化、更新、恢复、清理 | `PlayerRuleOverrideStateComponent` | `partial` | `D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs:88-182`、`:698-737` |
| `Enabled` | `ASharedTogglePower` | 世界共享 toggle 当前值 | 权威世界状态 | 世界初始化、重置、恢复、变更 | `RuleOverrideStateComponent` | `confirmed` | `D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs:215-240` |
| `SetPowerInfo`、`Reset` | `ASharedTogglePower` | 更新/清理世界 toggle | 权威状态转换入口；不在 Component 内保存行为 | 世界加载、清理、恢复 | `RuleOverrideStateComponent` | `confirmed` | `D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs:227-240` |
| `TargetTimeRate` | `ModifyTimeRate` | normalized slider 映射出的目标时间速率 | 派生值；不与 normalized 输入双存 | 每次输入变化、下游消费 | `RuleOverrideSnapshotComponent` | `confirmed` | `D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs:406-429`；`D:\TRbackup\Version4\Terraria\Main.cs:3169-3178` |
| `StrengthMultiplierToGiveNPCs`（difficulty） | `DifficultySliderPower` | normalized slider 映射出的 NPC 难度倍率 | 派生值/快照字段 | 每次输入变化、下游消费 | `RuleOverrideSnapshotComponent` | `confirmed` / `full-reference-supplemented` | `D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs:461-522`；完整参考 `D:\TRbackup\无任何删减通过编译\Terraria.GameContent.Creative\CreativePowers.cs:1182-1213`；`D:\TRbackup\Version4\Terraria\Main.cs:11366-11374` |
| `StrengthMultiplierToGiveNPCs`（spawn） | `SpawnRateSliderPerPlayerPower` | 每个玩家的刷怪率倍率 | 派生值；不存为玩家权威副本 | 玩家快照生成、NPC 消费 | `PlayerRuleOverrideSnapshotComponent` | `full-reference-supplemented` | Version4 `D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs:698-737`；完整参考 `D:\TRbackup\无任何删减通过编译\Terraria.GameContent.Creative\CreativePowers.cs:1889-1931`；`D:\TRbackup\Version4\Terraria\NPC.cs:668-674`、`:5804-5807` |
| `GetShouldDisableSpawnsFor` | `SpawnRateSliderPerPlayerPower` | slider 为 0 时的玩家禁刷资格 | 派生资格快照 | 每次玩家资格读取 | `PlayerRuleOverrideSnapshotComponent` | `full-reference-supplemented` | Version4 `D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs:709-722`；完整参考 `D:\TRbackup\无任何删减通过编译\Terraria.GameContent.Creative\CreativePowers.cs:1900-1913`；`D:\TRbackup\Version4\Terraria\NPC.cs:264-269` |
| `ModifyWindDirectionAndStrength` 的 slider cache | `ModifyWindDirectionAndStrength` | 风方向/强度 normalized 输入 | 权威世界输入；V4 消费实现不完整 | 世界初始化、变更、清理 | `RuleOverrideStateComponent` | `partial` | `D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs:524-543`；完整参考 `D:\TRbackup\无任何删减通过编译\Terraria.GameContent.Creative\CreativePowers.cs:1399-1427` |
| `ModifyRainPower` 的 slider cache | `ModifyRainPower` | 雨强 normalized 输入 | 权威世界输入；V4 消费实现不完整 | 世界初始化、变更、清理 | `RuleOverrideStateComponent` | `partial` | `D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs:545-564`；完整参考 `D:\TRbackup\无任何删减通过编译\Terraria.GameContent.Creative\CreativePowers.cs:1553-1589` |
| `FreezeTime`、`FreezeWindDirectionAndStrength`、`FreezeRainPower` 的 `Enabled` | concrete shared toggles | 时间、风、雨冻结开关 | 权威世界状态 | 世界恢复、重置、变更、清理 | `RuleOverrideStateComponent` | `confirmed` | `D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs:566-660`；`D:\TRbackup\Version4\Terraria\Main.cs:3169`、`:12074`、`:13017` |
| `StopBiomeSpreadPower.Enabled` | `StopBiomeSpreadPower` | 生态感染传播停止开关 | 权威世界状态 | 世界恢复、重置、变更、清理 | `RuleOverrideStateComponent` | `confirmed` | `D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs:662-696`；`D:\TRbackup\Version4\Terraria\WorldGen.cs:59409-59414` |
| `GodmodePower` 状态 | `GodmodePower` | 每个玩家的无伤/Creative toggle 状态 | 权威玩家状态；效果消费未闭合 | 玩家创建、恢复、离开 | `PlayerRuleOverrideStateComponent` | `partial` | `D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs:309-323`；完整参考 `D:\TRbackup\无任何删减通过编译\Terraria.GameContent.Creative\CreativePowers.cs:798-845`；`D:\TRbackup\Version4\Terraria\Player.cs:15209-15212` 为注释调用点 |
| `FarPlacementRangePower` 状态 | `FarPlacementRangePower` | 每个玩家的远距离放置资格 | 权威玩家状态 | 玩家创建、恢复、离开 | `PlayerRuleOverrideStateComponent` | `partial` | `D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs:325-340`；完整参考 `D:\TRbackup\无任何删减通过编译\Terraria.GameContent.Creative\CreativePowers.cs:847-890`；`D:\TRbackup\Version4\Terraria\Player.cs:19883-19885`、`:20027-20029` |
| `IPersistentPerWorldContent` 成员 | `ModifyTimeRate`、`DifficultySliderPower`、四个冻结/生态 Power | 世界字段的保存、加载、验证资格 | 持久化资格元数据；不是 Component 行为 | 世界保存/恢复/验证 | `RuleOverrideStateComponent` 的字段子集 | `confirmed` | `D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs:406-459`、`:461-522`、`:566-696`；`D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:3282-3285`、`:3513-3526` |
| `IPersistentPerPlayerContent` 成员 | `GodmodePower`、`FarPlacementRangePower`、`SpawnRateSliderPerPlayerPower` | 玩家字段保存、恢复和映射资格 | 持久化资格；最终 owner 未确认 | 玩家保存/恢复/加入 | `PlayerRuleOverrideStateComponent` 的字段子集 | `partial` | Version4 声明 `D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs:309-340`、`:698-737`；完整参考 `D:\TRbackup\无任何删减通过编译\Terraria.GameContent.Creative\CreativePowerManager.cs:205-282` |
| `SaveToWorld`、`LoadFromWorld`、`ValidateWorld` | `CreativePowerManager` | 按 Power ID 分段处理世界数据 | 外部边界资料；不放入 Component | 世界保存、恢复、坏数据验证 | 无（Component 只保存数据） | `confirmed` | `D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowerManager.cs:132-179` |
| `SaveCreativePowers`、`LoadCreativePowers` | `Terraria.IO.WorldFile` | Creative section 的边界和 section pointer | 兼容存档边界；不放入 Component | WLD 验证/保存/加载 | 无（Component 只保存数据） | `confirmed` | `D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:3513-3526` |
| `SyncThingsToJoiningPlayer` | `CreativePowerManager` | 加入玩家时发送权限和 Power 状态 | 兼容同步边界；不放入 Component | 玩家加入 | 无（Component 只保存数据） | `confirmed` | `D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowerManager.cs:180-197`；`D:\TRbackup\Version4\Terraria\MessageBuffer.cs:592-594` |
| `PowerId`、Creative packet 的 `PowerId` | `ICreativePower`、NLTX packet DTO | V1456/Version4 兼容编号 | 兼容 ID，不是实体 ID 或权威规则键 | 注册、编码、解码 | 无（由兼容边界保留） | `confirmed` | Version4 `D:\TRbackup\Version4\Terraria.GameContent.Creative\ICreativePower.cs:7-21`；NLTX `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Protocol.V1456\Packets\CreativePowerModulePacket.cs:3-9` |
| `AllowedToSpreadInfections` | `WorldGen` | 每次世界更新临时允许感染传播的 flag | 下游派生缓存；不属于 SRO authority | 每次世界更新重置和消费 | 无（由生态状态/快照消费） | `confirmed` | `D:\TRbackup\Version4\Terraria\WorldGen.cs:59409-59414`、`:60302-60306` |
| `Main._gameModeDifficultyOverride` | `Main` | 难度覆写下游缓存 | 下游派生值；不属于 SRO authority | 世界更新时重算 | 无（由快照消费） | `confirmed` | `D:\TRbackup\Version4\Terraria\Main.cs:11362-11375` |
| `StartDayImmediately`、`StartNightImmediately`、`StartNoonImmediately`、`StartMidnightImmediately` | `ASharedButtonPower` concrete types | 一次性时间动作，不是持续事实 | 瞬时意图/兼容 Power，不是持久状态 | 创建、点击、效果完成 | 无（非 Component） | `confirmed` / `partial` | `D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs:186-213`、`:342-404`；具体 `UsePower` 为空 |

## 5. Component 定义

### 5.1 `RuleOverrideStateComponent` (status: proposed)

#### 职责

保存一个世界实体上的 Creative/Journey 世界覆写权威输入。它保存 normalized slider 和
shared toggle 的当前事实，不保存权限判定、UI 状态、外部身份、网络字节、时钟或下游写入
缓存。时间速率、难度倍率、风的有效目标、雨的即时效果和感染允许值均在快照中作为派生
视图出现，不在本组件中形成第二个可变权威副本。

```text
componentId: SRO-COMP-WORLD-OVERRIDE
name: RuleOverrideStateComponent
status: proposed
componentOwner: SimulationRuleOverrides (candidate)
crossSubsystemOwner: integration-review
entityScope: exactly one WorldEntity per simulation world
lifecycle: world creation -> default/recovery initialization -> controlled value replacement -> world clear/unload
```

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
| --- | --- | --- | --- | --- | --- | --- |
| `TimeRateNormalized` | `float` | `0.0f` | 权威输入 | 必须为 finite，范围 `[0, 1]`；对应 Version4 的时间 slider 输入 | `partial` | Version4 `D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs:406-450`；完整参考 `D:\TRbackup\无任何删减通过编译\Terraria.GameContent.Creative\CreativePowers.cs:1013-1150` |
| `DifficultyNormalized` | `float` | `0.0f` | 权威输入 | 必须为 finite，范围 `[0, 1]`；不直接存 NPC 难度倍率 | `partial` | Version4 `D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs:461-514`；完整参考 `D:\TRbackup\无任何删减通过编译\Terraria.GameContent.Creative\CreativePowers.cs:1182-1383` |
| `WindDirectionAndStrengthNormalized` | `float` | 新实例 CLR 默认 `0.0f`；显式重置后值 `unresolved` | 权威输入 | 若有效，必须为 finite，范围 `[0, 1]`；不能把 `-0.8..0.8` 的有效风速当作 normalized 输入 | `partial` | Version4 `D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs:524-543`；完整参考 `D:\TRbackup\无任何删减通过编译\Terraria.GameContent.Creative\CreativePowers.cs:1399-1427` |
| `RainStrengthNormalized` | `float` | 新实例 CLR 默认 `0.0f`；显式重置后值 `unresolved` | 权威输入 | 若有效，必须为 finite，范围 `[0, 1]`；`0` 的即时雨效果语义由完整参考补证，V4 方法体不完整 | `partial` | Version4 `D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs:545-564`；完整参考 `D:\TRbackup\无任何删减通过编译\Terraria.GameContent.Creative\CreativePowers.cs:1553-1589` |
| `FreezeTimeEnabled` | `bool` | `false` | 权威状态 | 只能取 `true/false`；与 `TimeRateNormalized` 并存，不把冻结改写为速率 `0` | `confirmed` | Version4 `ASharedTogglePower` `D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs:215-240`；`FreezeTime` `D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs:566-596` |
| `FreezeWindEnabled` | `bool` | `false` | 权威状态 | 只能取 `true/false`；冻结读取不改变风目标输入 | `confirmed` | `D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs:598-628`；`D:\TRbackup\Version4\Terraria\Main.cs:12074-12080` |
| `FreezeRainEnabled` | `bool` | `false` | 权威状态 | 只能取 `true/false`；冻结读取不改变雨强输入 | `confirmed` | `D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs:630-660`；`D:\TRbackup\Version4\Terraria\Main.cs:13015-13046` |
| `StopBiomeSpreadEnabled` | `bool` | `false` | 权威状态 | 只能取 `true/false`；下游允许感染传播的值为其逻辑反值 | `confirmed` | `D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs:662-696`；`D:\TRbackup\Version4\Terraria\WorldGen.cs:59409-59414` |
| `AuthorityRevision` | `long`（候选） | `unresolved` | 权威版本元数据 | 如果最终保留，必须非负且只在权威值替换后单调增加；Version4 没有对应字段 | `missing` | Version4 组件相关文件未发现 revision 字段；研究报告只提出候选语义 |

#### 字段不变量

- Normalized slider 只能保存 finite 的 `[0, 1]` 值；任何映射后的有效值不得反向覆盖
  normalized 输入。
- `FreezeTimeEnabled`、`FreezeWindEnabled`、`FreezeRainEnabled` 和
  `StopBiomeSpreadEnabled` 是独立 bool；它们不共享一个可空标记，也不把下游临时 flag
  写回本组件。
- Version4 的世界持久化声明覆盖时间、难度、三个冻结开关和生态传播开关；风/雨 slider
  在 Version4 concrete type 中没有同样的 `IPersistentPerWorldContent` 声明。该差异是字段
  级持久化资格，不足以证明需要第二份 authority，但必须在最终持久化 owner 裁决后锁定。
- `TargetTimeRate` 的候选派生范围是整数 `1..24`；`DifficultyMultiplier` 的候选派生范围
  是 `0.5..3.0`，步长 `0.05`。这些是快照字段约束，不是本组件的可变字段。
- 风的候选有效目标范围为 `-0.8..0.8`，雨强候选范围为 `0..1`；完整参考只补证映射和
  环境效果，不能把这些补证写成 Version4 已闭合行为。

#### 生命周期

1. 世界实体创建后，组件必须在任何世界规则消费前得到默认值或经验证的恢复值。
2. 新世界使用字段表中的明确默认值；风/雨显式重置语义未被 Version4 完整确认，不得
   用恢复后默认值替代该缺口。
3. 已接受的世界覆写替换当前字段，并保持字段之间的范围不变量；组件本身不执行文件、
   网络、日志、UI 或环境副作用。
4. 世界清理/卸载时移除组件或丢弃其世界实体关联；不得把旧世界值复用到新世界。
5. 保存和加载只读取/恢复标记为 world-persistent 的字段；未知 Power record 的保留、
   跳过或阻止恢复仍由 `BD-COMP-04` 裁决。

#### Entity/World 范围

组件附着一个 WorldEntity，不为每个 Power 建立实体。WorldEntity 本身是跨
`WorldSession`、世界环境、生态、日历和持久化边界共享的对象，其最终 `WorldEntityId`
owner 必须标记为 `crossSubsystemOwner: integration-review`。

#### ID 与关系字段

- `PowerId: ushort` 仅是 Version4/V1456 兼容键，不是 ECS Entity ID，也不能直接成为
  本组件的集合索引 owner。
- `RuleKey` 是候选的稳定领域键，用于权限和兼容映射；它会被网络、持久化和多个消费者
  使用，`crossSubsystemOwner: integration-review`。
- `WorldEntityId` 是实体关联 ID；`PersistentWorldId`、`WorldSectionId`、`NetworkId` 和
  外部 session ID 不进入本组件。

#### 当前 NLTX 映射

- `WorldRuleState`（`dome/src/Terraria.Dome.Simulation/World/WorldRuleState.cs:5-97`）
  已有普通难度、雨、风、PVP 和 game mode 状态，标记为 `status: existing` 的相邻组件/领域
  状态；它不含 Creative 权限、冻结 toggle 或 Creative slider authority。
- `WorldRuleSnapshot`（`:1-3`）只有 `Tick`、`TimeOfDay` 和 `IsDayTime`，标记为
  `status: existing` 的时间投影；不覆盖本组件。
- `WorldRuleSnapshotComponent`（`D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\WorldGeneration\Components\WorldRuleSnapshotComponent.cs:5-89`）是
  world-generation 输入，标记为 `status: existing`；它的 `NoInfection` 和 `WorldIsFrozen`
  不是本组件的 Creative 当前值。
- 当前没有 `RuleOverrideStateComponent`；映射状态为 `missing`。

#### 证据

主要证据为 Version4 `D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowerManager.cs:114-179` 的重置/世界数据边界、
`D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs:406-696` 的世界 Power 字段，以及 `D:\TRbackup\Version4\Terraria\Main.cs`、`D:\TRbackup\Version4\Terraria\WorldGen.cs` 的直接消费。
其中风雨 slider 的有效环境效果来自完整参考同路径成员补证，V4 当前实现保持 `partial`。

### 5.2 `PlayerRuleOverrideStateComponent` (status: proposed)

#### 职责

保存单个玩家实体的 Creative per-player 权威状态。组件只保存玩家状态事实，不保存
`byte playerSlot`、Character UUID、连接句柄、外部 session 身份、下游伤害结算或 NPC 生成
结果。

```text
componentId: SRO-COMP-PLAYER-OVERRIDE
name: PlayerRuleOverrideStateComponent
status: proposed
componentOwner: SimulationRuleOverrides (candidate)
crossSubsystemOwner: integration-review
entityScope: one PlayerEntity
lifecycle: player entity creation -> default/profile recovery -> accepted value replacement -> player despawn
```

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
| --- | --- | --- | --- | --- | --- | --- |
| `GodmodeEnabled` | `bool` | `false` | 权威状态 | 只能取 `true/false`；组件只记录资格/状态，不承诺伤害结算效果 | `partial` | Version4 `D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs:309-323`；完整参考 `D:\TRbackup\无任何删减通过编译\Terraria.GameContent.Creative\CreativePowers.cs:798-845`；`D:\TRbackup\Version4\Terraria\Player.cs:15209-15212` 是注释调用 |
| `FarPlacementRangeEnabled` | `bool` | `true` | 权威状态 | 只能取 `true/false`；只在 Journey/对应难度资格成立时被下游读取 | `partial` | Version4 `D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs:325-340`、`:331`；完整参考 `D:\TRbackup\无任何删减通过编译\Terraria.GameContent.Creative\CreativePowers.cs:847-890`；`D:\TRbackup\Version4\Terraria\Player.cs:19883-19885`、`:20027-20029` |
| `SpawnRateNormalized` | `float` | `0.5f` | 权威输入 | 必须为 finite，范围 `[0, 1]`；`0` 触发 `DisableSpawns`，非零值才使用倍率 | `partial` / `full-reference-supplemented` | Version4 `D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs:698-737`、`:705`；完整参考 `D:\TRbackup\无任何删减通过编译\Terraria.GameContent.Creative\CreativePowers.cs:1889-1931` |

#### 字段不变量

- 玩家状态属于 PlayerEntity，不以外部 slot 数组作为长期实体身份。
- `SpawnRateNormalized == 0` 与“禁刷资格”为同一玩家快照中的两个相关但不同字段；禁止
  用 `SpawnRateMultiplier == 0` 代替禁刷状态，因为完整参考的映射函数在零点仍有独立的
  slider 语义。
- Spawn-rate 的候选有效倍率为 `0.1..10`；该范围和分段映射由完整参考补证，Version4
  当前 `RemapSliderValueToPowerValue` 为空，不能标为 Version4 已实现。
- Godmode 和 FarPlacement 的具体效果不内嵌到该组件；它们的下游消费者只读取同一玩家
  的只读状态视图。

#### 生命周期

1. PlayerEntity 建立时附加组件，并写入 `GodmodeEnabled=false`、
   `FarPlacementRangeEnabled=true`、`SpawnRateNormalized=0.5f`，除非最终持久化策略给出
   已验证的玩家恢复值。
2. 玩家恢复只能把通过格式和范围检查的值写入该组件；不合法或未知值不得覆盖既有有效值。
3. 玩家离开/实体销毁时清理组件；不得以复用的 `LegacySlot` 作为下一名玩家的状态容器。
4. Version4 的 per-player 持久化接口在当前 Version4 Manager 中没有完整调用闭环；玩家
   profile、独立 Creative player section 或首轮延期三种方案由 `BD-COMP-03` 裁决。

#### Entity/World 范围

一个 PlayerEntity 至多有一个该组件。它不附着 WorldEntity，也不为每个 Power 创建独立
玩家实体。多个玩家的同名字段通过多个 PlayerEntity 的组件实例表达，而不是在一个世界
组件中保存可变玩家数组。

#### ID 与关系字段

- `PlayerEntityId` 是 ECS 实体关联，候选 owner 为 Player/Simulation 的整合边界，必须写
  `crossSubsystemOwner: integration-review`。
- `LegacyPlayerSlot: byte` 是协议兼容位置；当前 NLTX 的 `PlayerIdentityState.LegacySlot`
  位于 `D:\TRbackup\NLTX\src\Player\PlayerIdentityState.cs:3-17`，标记为
  `status: existing`，不作为该组件的主键。
- 玩家持久化 UUID/profile ID、`NetworkId`、连接 session ID 和客户端本地 clone 不进入
  组件；它们在身份/持久化/网络边界单独归类。

#### 当前 NLTX 映射

- `PlayerIdentityState` 只有姓名、队伍、难度、host、连接状态和 `LegacySlot`，是
  `status: existing` 的身份状态，不含三个 Creative 字段。
- `PlayerPersistentState`（`dome/src/Terraria.Dome.Simulation/Players/PlayerPersistentState.cs:8-94`）
  已有生命、魔力、buff、物品和 profile 状态，但没有 Creative per-player 字段，不能
  被假定为已承接本组件。
- `NpcSpawnPlayerReadiness`（`dome/src/Terraria.Dome.Simulation/Npc/Snapshots/NpcSpawnPlayerReadiness.cs:3-8`）
  的 `IsSpawnRateDisabled` 是 NPC 资格快照字段，标记为 `status: existing` 的下游快照，
  不能反向成为玩家权威状态。
- 当前没有 `PlayerRuleOverrideStateComponent`；映射状态为 `missing`。

#### 证据

Version4 `APerPlayerTogglePower`、`APerPlayerSliderPower` 的数组/cache 和 concrete
Power 声明确认了玩家作用域；完整参考 `D:\TRbackup\无任何删减通过编译\Terraria.GameContent.Creative\CreativePowerManager.cs:205-282` 与
`D:\TRbackup\无任何删减通过编译\Terraria.GameContent.Creative\CreativePowers.cs:813-888`、`:2057-2086` 只补证玩家保存字段。Godmode 的调用点仍是
注释代码，因此其行为消费者保持 `evidence-gap`。

### 5.3 `RuleOverridePermissionStateComponent` (status: proposed)

#### 职责

保存每个稳定 `RuleKey` 的默认权限和当前权限。它与覆写当前值分开，因为权限的初始化、
配置更新、重置和加入同步生命周期不同于规则值；组件不保存调用者、不读取网络身份、不
承担外部权限写入效果。

```text
componentId: SRO-COMP-PERMISSION
name: RuleOverridePermissionStateComponent
status: proposed
componentOwner: SimulationRuleOverrides (candidate)
crossSubsystemOwner: integration-review
entityScope: one world-authority entity, or one server-session authority entity if integration review selects that scope
lifecycle: authority initialization -> config/reviewed update -> reset/recovery -> world/session teardown
```

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
| --- | --- | --- | --- | --- | --- | --- |
| `DefaultPermissionByRule` | `IReadOnlyDictionary<RuleKey, PowerPermissionLevel>`（内部权威存储） | 每个已注册 Power 初始为 `CanBeChangedByEveryone`；注册键集合未独立建模 | 权威默认策略 | key 必须是已注册稳定 `RuleKey`；值只能是 `LockedForEveryone`、`CanBeChangedByHostAlone` 或 `CanBeChangedByEveryone` | `confirmed` / `partial` | Version4 `D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowerManager.cs:35-49`；`D:\TRbackup\Version4\Terraria.GameContent.Creative\PowerPermissionLevel.cs:3-8` |
| `CurrentPermissionByRule` | `IReadOnlyDictionary<RuleKey, PowerPermissionLevel>`（内部权威存储） | 注册后与默认值相同；配置覆盖后以经校验值为准 | 权威当前策略 | key 集合与默认表一致；当前值不能由未经信任的客户端身份直接写入 | `confirmed` / `partial` | `D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowerManager.cs:42-44`、`:66-85`、`:114-129`；权限网络接收体 `D:\TRbackup\Version4\Terraria.GameContent.NetModules\NetCreativePowerPermissionsModule.cs:22-24` 为空 |
| `PermissionRevision` | `long`（候选） | `unresolved` | 权威版本元数据 | 如果保留，必须非负，并在权限实际替换后单调增加；Version4 未提供对应字段 | `missing` | Version4 `D:\TRbackup\Version4\Terraria.GameContent.Creative\ICreativePower.cs:7-21` 只有 current/default 两个权限属性 |

#### 字段不变量

- 两个映射必须拥有相同的规则键集合；缺少规则键不能通过默认枚举值静默补齐。
- `CurrentPermissionByRule` 与 `DefaultPermissionByRule` 的值域严格对应 Version4 三值
  `PowerPermissionLevel`，不把未知整数作为合法权限保存。
- Version4 `TryListingPermissionsFrom` 将配置整数 clamp 到 `0..2`，并同时设置 default/current；
  配置键不存在时不得新增一个没有注册定义的权限条目。
- 外部 `PowerId`、server-config 字符串和 caller/session ID 不成为权限组件字段；它们只在
  边界映射到 `RuleKey` 或权限上下文。

#### 生命周期

1. 在任何规则状态可接受前，组件必须拥有经过注册表验证的规则键集合。
2. 初始化时 current/default 使用 Version4 的默认权限；受审查的配置更新可以同时改变
   两个值，具体可信 writer 仍由 `BD-COMP-02` 决定。
3. 世界/会话 reset 将 current 恢复为 default；不能把客户端收到的权限展示值当作新的
   default。
4. WorldEntity 或 authority entity 清理时一并丢弃映射，不能跨世界复用。

#### Entity/World 范围

Version4 的 `CurrentPermissionLevel` 和 `DefaultPermissionLevel` 按 Power 全局共享，而
不是每个玩家一个权限字段。因此候选组合是一个 world-authority entity 上的单个权限
Component。若最终确认权限是服务器会话级而非世界级，组件应移动到 session-authority
entity；这是 owner 决策，不在本文件中强行选择。

#### ID 与关系字段

- `RuleKey` 是稳定领域规则键候选，服务于权限、兼容映射和持久化，
  `crossSubsystemOwner: integration-review`。
- `PowerId: ushort` 只能作为 V1456 兼容映射输入；不能依赖注册顺序作为新的 ECS 规则键。
- 外部管理员/玩家身份、`NetworkId`、连接 session ID 和 server-config name 不进入组件。

#### 当前 NLTX 映射

- `CreativePowerPermissionModulePacket`（`D:\TRbackup\NLTX\dome\src\Terraria.Dome.Protocol.V1456\Packets\CreativePowerPermissionModulePacket.cs:3-6`）
  是 `status: existing` 的 DTO，不是权限组件。
- `TerrariaPacketCodec.DecodeCreativePowerPermissionModule`（`D:\TRbackup\NLTX\dome\src\Terraria.Dome.Protocol.V1456\Packets\TerrariaPacketCodec.cs:647-669`）已有 module、selector、
  固定长度和字段解码；`TerrariaSession.AcceptNetModule`（`D:\TRbackup\NLTX\dome\src\Terraria.Dome.Protocol.V1456\Session\TerrariaSession.cs:369-373`）解码后丢弃，未写入
  权限状态，故不能标为 partial authority。
- 当前没有 `RuleOverridePermissionStateComponent`；映射状态为 `missing`。

#### 证据

Version4 Manager 的注册/配置/重置和加入同步确认 current/default 的存在；Version4 权限
网络模块 `Deserialize` 为空，完整参考的实现也不能替代 Version4 当前 writer。权限组件
owner 和可信 writer 因此保持 `decision-required`。

### 5.4 `RuleOverrideSnapshotComponent` (status: proposed)

#### 职责

保存单个 Tick 对世界覆写的不可变值视图。它不是权威输入的第二份可变副本；只包含已经
由当前权威状态和已确认世界上下文得到的值，供多个下游领域一致读取。

```text
componentId: SRO-COMP-WORLD-SNAPSHOT
name: RuleOverrideSnapshotComponent
status: proposed
componentOwner: SimulationRuleOverrides (candidate)
crossSubsystemOwner: integration-review
entityScope: one WorldEntity per published snapshot
lifecycle: created after world authority is valid -> replaced atomically per accepted snapshot boundary -> discarded on world unload
```

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
| --- | --- | --- | --- | --- | --- | --- |
| `Tick` | `long`（候选） | 无有效默认值；未采样时不可消费 | 快照 | 必须标识一次完整的世界规则观察；确切 tick owner 未由 Version4 Creative 代码规定 | `partial` / `unresolved` | 当前 NLTX `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\World\WorldRuleSnapshot.cs:3` 有 `long Tick`；Version4 无统一 Creative snapshot |
| `SourceRevision` | `long`（候选） | `unresolved` | 快照元数据 | 必须对应同一批已接受世界状态；Version4 无 revision 语义 | `missing` | Version4 仅有 Power 当前值和缓存，无 snapshot revision |
| `TargetTimeRate` | `int` | 无有效默认值；候选范围 `1..24` | 派生快照 | 必须为整数 `1..24`；不得写回 `TimeRateNormalized` | `confirmed` / `full-reference-supplemented` | Version4 `D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs:408-429`；完整参考 `D:\TRbackup\无任何删减通过编译\Terraria.GameContent.Creative\CreativePowers.cs:1015-1036`；`D:\TRbackup\Version4\Terraria\Main.cs:3169-3178` |
| `DifficultyMultiplier` | `float` | 无有效默认值；候选范围 `0.5..3.0` | 派生快照 | finite，范围 `0.5..3.0`，步长 `0.05` | `full-reference-supplemented` | 完整参考 `D:\TRbackup\无任何删减通过编译\Terraria.GameContent.Creative\CreativePowers.cs:1184-1213`；Version4 下游读取 `D:\TRbackup\Version4\Terraria\Main.cs:11369-11372` |
| `WindSpeedTarget` | `float` | 无有效默认值 | 派生快照 | finite，候选范围 `-0.8..0.8`；不等同于 normalized slider | `partial` / `full-reference-supplemented` | 完整参考 `D:\TRbackup\无任何删减通过编译\Terraria.GameContent.Creative\CreativePowers.cs:1408-1420`；Version4 weather consumer `D:\TRbackup\Version4\Terraria\Main.cs:12050-12080` |
| `RainStrength` | `float` | 无有效默认值 | 派生快照 | finite，候选范围 `0..1`；是否立即启动/停止雨不属于快照字段责任 | `partial` / `full-reference-supplemented` | 完整参考 `D:\TRbackup\无任何删减通过编译\Terraria.GameContent.Creative\CreativePowers.cs:1562-1581`；Version4 `D:\TRbackup\Version4\Terraria\Main.cs:13015-13046` |
| `FreezeTimeEnabled` | `bool` | 无有效默认值 | 快照 | 必须与同一 Tick 的世界覆写状态一致 | `confirmed` | Version4 `D:\TRbackup\Version4\Terraria\Main.cs:3169-3178` |
| `FreezeWindEnabled` | `bool` | 无有效默认值 | 快照 | 必须与同一 Tick 的世界覆写状态一致 | `confirmed` | Version4 `D:\TRbackup\Version4\Terraria\Main.cs:12074-12080` |
| `FreezeRainEnabled` | `bool` | 无有效默认值 | 快照 | 必须与同一 Tick 的世界覆写状态一致 | `confirmed` | Version4 `D:\TRbackup\Version4\Terraria\Main.cs:13017-13046` |
| `AllowInfectionSpread` | `bool` | 无有效默认值 | 派生快照 | 由 `!StopBiomeSpreadEnabled` 得到；不得从 `WorldRuleSnapshotComponent.NoInfection` 猜测 | `confirmed` | Version4 `D:\TRbackup\Version4\Terraria\WorldGen.cs:59409-59414`、`:60302-60306` |

#### 字段不变量

- Snapshot Component 内所有字段在发布后只读；不能把它当作可变 authority。
- 世界字段必须来自同一个有效观察版本；不能把一个 Tick 的时间值和另一个 Tick 的天气
  或生态值拼接。
- `TargetTimeRate`、`DifficultyMultiplier`、`WindSpeedTarget`、`RainStrength` 和
  `AllowInfectionSpread` 是派生值；它们不得成为 `RuleOverrideStateComponent` 的第二个
  可变写集。
- 未完成首次采样时，实体不应持有可被消费者误读的“默认 snapshot”；`default(T)` 不能
  表示合法的世界规则事实。

#### 生命周期

1. WorldEntity 的权威覆写状态和必要世界上下文均有效后，创建一份完整快照。
2. 每次快照替换都产生一个新的不可变值视图；旧快照在没有消费者引用后丢弃。
3. 世界清理/卸载时丢弃快照，禁止下一世界沿用旧 Tick 或旧 revision。
4. 该 Component 不保存网络 payload、BinaryReader、WLD section offset、UI slider 目标或
   外部 session identity。

#### Entity/World 范围

快照候选附着 WorldEntity，因为时间、天气、难度和生态值是世界共享的。但它被
`WorldSession`、`WorldGenerationAndEcology`、`WorldCalendarAndEventOrchestration`、
`NpcAndTownSimulation` 和玩家相关消费边界读取，最终 owner 必须是
`crossSubsystemOwner: integration-review`。

#### ID 与关系字段

- `WorldEntityId` 只表达快照宿主实体关系，不作为快照业务字段。
- `Tick` 是模拟时间标识；`PersistentWorldId`、`WorldSectionId`、`NetworkId` 和
  外部 session ID 不进入快照。
- `RuleKey`、`PowerId` 和原始 packet 不进入快照；快照只保存已映射、已校验的值。

#### 当前 NLTX 映射

- 当前 `WorldRuleSnapshot` 只有时间三元组，是 `status: existing` 的窄时间投影，不应
  扩展性地把它改写成本组件的完整 Creative 快照。
- `DomeSimulation.CreateWorldRuleSnapshot` 使用 `TickNumber`、`TimeOfDay` 和 `IsDayTime`，
  只能证明已有时间投影，不证明 Creative snapshot authority。
- `WorldRuleSnapshotComponent` 属于 world generation 输入，不能作为本组件的替代物。
- 当前没有本组件；映射状态为 `missing`。

#### 证据

Version4 的直接消费点证明了快照字段的候选内容，但 Version4 没有一个名为统一规则快照
的现成类型。tModLoader 的 `PreUpdateWorld` 和 `PostUpdateTime` 只证明公开生命周期边界，
不锁定 NLTX 的 Tick owner、revision 或发布时机；因此这些字段保持候选和未决标记。

### 5.5 `PlayerRuleOverrideSnapshotComponent` (status: proposed)

#### 职责

保存单个 PlayerEntity 在同一规则观察版本上的不可变玩家视图。将玩家快照独立于世界
快照，避免在 WorldEntity 中维护一个按玩家 ID 索引的巨型可变集合，也避免把玩家权威状态
和派生资格混在一起。

```text
componentId: SRO-COMP-PLAYER-SNAPSHOT
name: PlayerRuleOverrideSnapshotComponent
status: proposed
componentOwner: SimulationRuleOverrides (candidate)
crossSubsystemOwner: integration-review
entityScope: one PlayerEntity for the corresponding world snapshot
lifecycle: created/replaced with a valid world snapshot -> discarded on player/world teardown
```

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
| --- | --- | --- | --- | --- | --- | --- |
| `Tick` | `long`（候选） | 无有效默认值；无对应世界快照时不可消费 | 快照 | 必须与关联 `RuleOverrideSnapshotComponent.Tick` 相等 | `partial` / `unresolved` | 当前 NLTX `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\World\WorldRuleSnapshot.cs:3`；Version4 per-player Power 没有统一 Tick 字段 |
| `SourceRevision` | `long`（候选） | `unresolved` | 快照元数据 | 必须与关联世界状态版本一致；Version4 未确认 revision | `missing` | Version4 无统一快照版本字段 |
| `GodmodeEnabled` | `bool` | 无有效默认值 | 快照 | 只读复制玩家权威状态；不产生伤害效果 | `partial` | Version4 `D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs:38-49`；`D:\TRbackup\Version4\Terraria\Player.cs:15209-15212` 为注释调用 |
| `FarPlacementRangeEnabled` | `bool` | 无有效默认值 | 快照 | 只读复制玩家权威状态；具体 placement 逻辑留在玩家消费边界 | `partial` | Version4 `D:\TRbackup\Version4\Terraria\Player.cs:19883-19885`、`:20027-20029` |
| `SpawnRateMultiplier` | `float` | 无有效默认值 | 派生快照 | 玩家未禁刷时 finite，候选范围 `0.1..10`；禁刷必须由独立 bool 表达 | `full-reference-supplemented` | 完整参考 `D:\TRbackup\无任何删减通过编译\Terraria.GameContent.Creative\CreativePowers.cs:1921-1930`；Version4 NPC 读取 `D:\TRbackup\Version4\Terraria\NPC.cs:668-674`、`:5804-5807` |
| `DisableSpawns` | `bool` | 无有效默认值 | 派生快照 | 与同一玩家的 `SpawnRateNormalized == 0` 一致；不能反写 authority | `partial` / `full-reference-supplemented` | Version4 `D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs:709-722`；完整参考 `D:\TRbackup\无任何删减通过编译\Terraria.GameContent.Creative\CreativePowers.cs:1900-1913` |

#### 字段不变量

- `Tick` 和 `SourceRevision` 必须同时对应同一份有效世界快照；任一个无法确认时，整个
  玩家快照不可消费。
- `SpawnRateMultiplier` 是派生字段，`DisableSpawns` 是独立资格字段；二者都不能反向
  写入 `PlayerRuleOverrideStateComponent`。
- 该组件只读；不保存外部 `playerSlot`、网络包、profile 字节或玩家连接身份。

#### 生命周期

1. 只有在对应 WorldEntity 快照有效时，PlayerEntity 才创建或替换玩家快照。
2. 玩家加入、离开、恢复或世界卸载造成的实体集合变化，必须让快照与 PlayerEntity 的
   存在性保持一致。
3. 当玩家权威状态未能恢复或权限/作用域尚未确认时，不用 `default` 快照冒充有效值；
   组件可以缺席，直到有效快照可构造。

#### Entity/World 范围

快照附着 PlayerEntity，而不是附着 WorldEntity 的玩家字典。世界快照与玩家快照通过
`WorldEntityId`、`PlayerEntityId` 和相等的 `Tick`/`SourceRevision` 形成关系；这三个关系
字段都属于跨子系统整合范围。

#### ID 与关系字段

- `PlayerEntityId` 是快照宿主实体关系；它不是 `byte playerSlot`。
- `PlayerEntityId -> WorldEntityId` 是模拟实体关系；`PersistentPlayerId`、`NetworkId`、
  `LegacyPlayerSlot` 和外部 session identity 单独保存。
- 快照不包含 Power ID 或未经验证的规则键。

#### 当前 NLTX 映射

- `NpcSpawnPlayerReadiness` 是现有 NPC 下游快照，包含 `IsSpawnRateDisabled`，标记为
  `status: existing`，但它不拥有玩家 Creative 状态。
- `JourneySpawnRatePacket`（`D:\TRbackup\NLTX\dome\src\Terraria.Dome.Protocol.V1456\Packets\JourneySpawnRatePacket.cs:3-5`）
  是 `status: existing` 的输入 DTO，不是玩家快照。
- 当前没有 `PlayerRuleOverrideSnapshotComponent`；映射状态为 `missing`。

#### 证据

Version4 的 NPC 直接读取 per-player spawn-rate；Player 的 FarPlacement 读取也按玩家
作用域进行。完整参考补充倍率映射，但没有为 NLTX 提供现成玩家规则快照，因此 Tick、
revision 和跨实体关系仍是 `evidence-gap`。

## 6. Entity 与 Component 组合

| Entity/World 对象 | 必需 Component | 可选 Component | 互斥 Component | 组合理由 |
| --- | --- | --- | --- | --- |
| 一个世界权威实体 `WorldEntity` | `RuleOverrideStateComponent`、`RuleOverridePermissionStateComponent` | `RuleOverrideSnapshotComponent`（首次有效快照后才存在） | 不与另一个同义 Creative world authority Component 并存 | 世界当前覆写和权限映射都按世界/authority 生命周期存在；快照是同一实体的不可变视图，不反向成为当前值 |
| 一个玩家实体 `PlayerEntity` | `PlayerRuleOverrideStateComponent` | `PlayerRuleOverrideSnapshotComponent`（关联世界快照有效时存在） | 不与按 `LegacyPlayerSlot` 索引的第二份 Creative 玩家 authority 并存 | 玩家 toggle/slider 共同属于玩家实体生命周期；玩家快照只保存派生/只读视图 |
| 普通世界实体 | 无本子系统 Component | 无 | 不应因为拥有天气、Tile 或生成字段而附加 Creative rule Component | 本子系统只附着唯一 world authority；普通实体不是 Power 容器 |
| 单个 Power 记录 | 无 | 无 | 不创建 `PowerEntity` 或每 Power 一组 Component | Power 是兼容注册项/规则键和字段集合，不拥有独立实体生命周期 |
| 世界生成输入对象 | 无本子系统 Component | 既有 `WorldRuleSnapshotComponent` | 不把 `WorldRuleSnapshotComponent` 与 Creative snapshot 合并 | 生成输入的 `NoInfection`、`WorldIsFrozen` 与运行时 Creative 当前值生命周期不同 |

组合关系只表达数据范围，不表达执行顺序。`WorldEntityId`、`PlayerEntityId`、
`PersistentWorldId`、`PersistentPlayerId`、`NetworkId`、`WorldSectionId`、
`LegacyPlayerSlot` 和外部 session identity 必须分别建模；不能以同一个 `int`/`byte` 字段
替代这些不同生命周期的 ID。

## 7. 组件拆分与合并决策

### 7.1 必须拆开

1. **世界 authority 与权限 state 拆开。** 当前/默认权限由注册、配置和 reset 驱动，世界
   slider/toggle 由恢复和规则变更驱动；合并会让权限策略和规则值共享错误的持久化/恢复
   不变量。
2. **authority state 与 snapshot 拆开。** authority 可以在受控边界被替换，快照在发布后
   必须只读；合并会允许下游通过快照引用反向改变当前值。
3. **世界 scope 与玩家 scope 拆开。** Version4 的 shared Power 与 per-player Power
   生命周期、持久化字段和读者集合不同；不能在世界组件中保留按 slot 的玩家数组。
4. **世界快照与玩家快照拆开。** 世界消费者只需要世界值，NPC/Player 消费者只需要单个
   PlayerEntity 的值；把玩家字典塞进世界 Component 会扩大访问范围、引入集合失效条件
   和实体关系同步负担。
5. **normalized input 与 mapped effective value 拆开。** `TargetTimeRate`、难度倍率、
   风目标、雨强和刷怪倍率都有派生语义；同时保存输入和派生值会产生双写和失效顺序问题。

### 7.2 可以合并

1. 世界的时间速率、难度、天气 slider 和冻结/生态 toggle 暂保留在一个
   `RuleOverrideStateComponent` 中，而不是为 15 个 Power 建立 15 个组件。它们共同依附
   WorldEntity、共同经过 world authority 生命周期，并共享 normalized/toggle 范围不变量。
2. `GodmodeEnabled`、`FarPlacementRangeEnabled` 和 `SpawnRateNormalized` 暂保留在一个
   `PlayerRuleOverrideStateComponent` 中。它们共同依附 PlayerEntity、共同受玩家创建/离开
   影响；不同下游读取者不构成拆出实体组件的充分理由。
3. `CurrentPermissionByRule` 和 `DefaultPermissionByRule` 保留在一个权限 Component 中，
   因为两者必须拥有相同规则键集合，且 reset 明确要求 current 回到 default。

### 7.3 不因字段数量机械原子化的理由

如果把时间、难度、风、雨、生态各自拆为独立 Component，当前尚未确认的恢复、快照和
权限键集合会增加跨 Component 一致性要求，而没有证据证明它们需要不同实体生命周期。
因此本候选设计先保留三个内聚状态域：世界覆写、玩家覆写、权限；把 authority/snapshot
和 world/player 的真实生命周期差异作为必须拆开的边界。最终整合可依据恢复 owner、
消费者访问集合和持久化策略重新裁决，但本文件不把细分结果写成已锁定架构。

## 8. 不单独创建 Component 的对象

| 对象 | 不单独创建 Component 的原因 |
| --- | --- |
| 单个 Creative Power | 它是一个兼容注册项和规则键对应的字段集合；15 个 Power 共享 world/player scope 和相同实体生命周期，拆成 15 个实体会复制权限、快照和恢复关系 |
| 单个 `PowerId` / server-config name | 它是兼容编号或配置键，不是实体身份，也不保存持续领域状态 |
| 单个权限 bool 或单个权限消息 | 权限需要按规则键维护 current/default 不变量；孤立 bool 无法表达键集合、默认值和 owner |
| 单个客户端 slider 或按钮 | 它是外部输入/表现控制，不是服务器权威事实；slider 的 normalized 值归世界或玩家 authority |
| `StartDayImmediately` 等即时按钮 Power | 它是一次性动作意图，没有可持续当前值、持久化事实或独立实体生命周期 |
| `CreativePowerModulePacket`、`CreativePowerPermissionModulePacket`、`JourneySpawnRatePacket` | 它们是已存在的协议 DTO，字段携带外部兼容输入，不应渗入核心 Component |
| `WldCreativePowerReader` 的 opaque payload | 它是兼容恢复资料；字节、section offset 和未知记录策略不属于运行时 Component |
| `AllowedToSpreadInfections` | 它是 WorldGen 每次更新临时重置的派生 flag，不是 Creative authority |
| `WorldRuleState` 当前雨/风/难度字段 | 它是 NLTX 已有通用世界状态；与 Creative normalized authority 不是同一个生命周期，不能用合并避免设计决策 |
| `WorldRuleSnapshot` 当前时间三元组 | 它是已有窄时间投影，不包含 Creative 字段，不能借名扩容为本设计组件 |
| `NpcSpawnPlayerReadiness` | 它是 NPC 资格快照；它消费玩家规则结果，不拥有玩家规则 authority |
| UI 排序项、`ContentPresentationIndex` 条目 | 它们是表现元数据；当前源码没有权限、覆写、恢复或玩家状态闭环 |

## 9. 当前 NLTX 组件覆盖

| 当前对象 | 状态 | 覆盖内容 | 与目标 Component 的关系 |
| --- | --- | --- | --- |
| `D:\TRbackup\NLTX\src\Content\ContentPresentationIndex.cs:5-58` | `status: existing` | Creative sorting、bestiary、animation、glow 等 frozen presentation index | 不覆盖任何目标 Component；不能从内容排序推断权限或覆写状态 |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\World\WorldRuleState.cs:5-97` | `status: existing` | 普通 difficulty、game mode、雨、风、PVP 和相关不变量 | 与 `RuleOverrideStateComponent` 相邻；保持独立，最终共享字段 owner 需 `crossSubsystemOwner: integration-review` |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\World\WorldRuleSnapshot.cs:3` | `status: existing` | `long Tick`、`double TimeOfDay`、`bool IsDayTime` | 只能映射为已有时间投影证据；不覆盖 `RuleOverrideSnapshotComponent` |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\WorldGeneration\Components\WorldRuleSnapshotComponent.cs:5-89` | `status: existing` | world generation 的 difficulty、secret seed、`NoInfection`、`WorldIsFrozen` 等输入 | 不覆盖运行时 Creative authority 或玩家规则快照 |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Npc\Snapshots\NpcSpawnPlayerReadiness.cs:3-8` | `status: existing` | NPC 读取用的 `IsSpawnRateDisabled` | 是下游派生快照，不覆盖 `PlayerRuleOverrideStateComponent` 或 `PlayerRuleOverrideSnapshotComponent` |
| `D:\TRbackup\NLTX\src\Player\PlayerIdentityState.cs:3-17` | `status: existing` | 玩家身份、连接状态和可空 `LegacySlot` | 只提供外部/兼容身份边界，不承接 Creative 玩家字段 |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Players\PlayerPersistentState.cs:8-94` | `status: existing` | profile、生命/魔力、buff、物品和 well-fed 状态 | 当前没有 Creative 玩家字段；最终是否扩展该 profile 由 `BD-COMP-03` 决定 |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Protocol.V1456\Packets\CreativePowerModulePacket.cs:3-9` | `status: existing` | Power ID、payload kind、slot、toggle/slider 数据 | 仅外部协议形状，不是 authority Component |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Protocol.V1456\Packets\CreativePowerPermissionModulePacket.cs:3-6` | `status: existing` | Power ID 和权限 byte | 解码后当前被丢弃；不是 `RuleOverridePermissionStateComponent` |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Protocol.V1456\Packets\JourneySpawnRatePacket.cs:3-5` | `status: existing` | per-player slot 和 normalized slider | 只有 DTO；Power 14 路径做了 ownership 检查，未形成玩家 authority |
| `D:\TRbackup\NLTX\dome\src\Terraria.WorldFile.V319\Format\WldCreativePowerReader.cs:7-21` | `status: existing` | opaque Creative section compatibility data | 不形成 world state 或 snapshot Component |
| `RuleOverrideStateComponent`、`PlayerRuleOverrideStateComponent`、`RuleOverridePermissionStateComponent`、`RuleOverrideSnapshotComponent`、`PlayerRuleOverrideSnapshotComponent` | `status: proposed` | 本设计提出的五个候选 Component | 当前源码没有这些类型；本文件不创建实现 |

当前 NLTX 的总体结论是 `nltxStatus: missing`：存在相邻模型和协议材料，但没有本子系统
的权威状态、玩家/世界组合闭环和有效规则快照。

## 10. 组件级 evidence-gap

| ID | 缺口 | 影响的 Component | 当前证据 | 设计处理 |
| --- | --- | --- | --- | --- |
| `E-GAP-COMP-01` | Version4 `CreativePowerManager` 当前文件没有完整 per-player 保存/恢复入口；完整参考有同路径补证，但不能证明 Version4 当前闭环 | `PlayerRuleOverrideStateComponent` | `partial` | 只确认字段形状和持久化资格，不确认最终保存 owner |
| `E-GAP-COMP-02` | `D:\TRbackup\Version4\Terraria\Player.cs:15209-15212` 的 Godmode 消费点是注释代码 | `PlayerRuleOverrideStateComponent`、`PlayerRuleOverrideSnapshotComponent` | `partial` | 只保存/投影状态，不宣称伤害效果已存在 |
| `E-GAP-COMP-03` | Version4 风/雨 slider 的更新方法为空；完整参考补充环境效果，但显式 reset、持久化和恢复时点未闭合 | `RuleOverrideStateComponent` | `partial` | 保留 normalized 字段，默认/持久化分支标注 unresolved |
| `E-GAP-COMP-04` | Version4 没有统一 Creative Tick snapshot、revision 或 WorldEntity/PlayerEntity 关系类型 | 两个 snapshot Component | `missing` / `unresolved` | 使用候选 `Tick`/`SourceRevision` 字段，但不写数值默认或最终 shared owner |
| `E-GAP-COMP-05` | 权限网络接收体为空；服务端 config-only、受控服务端 writer 或其他可信 owner 尚未确定 | `RuleOverridePermissionStateComponent` | `partial` | 客户端 DTO 不得直接写 Component；权限 owner 留给 `BD-COMP-02` |
| `E-GAP-COMP-06` | 未知 Power ID/WLD opaque record 在恢复时的保留、跳过或拒绝策略未裁决 | 世界 state、权限 state、兼容边界 | `partial` | 不把兼容记录放入 Component；恢复策略必须先有整合决策 |

上述六项均是组件级 `evidence-gap`，不把候选设计提升为 `baseline`。其中 `E-GAP-COMP-02`
和 `E-GAP-COMP-04` 还会影响快照字段是否可消费，属于需要保持 `decision-required` 的原因。

## 11. 未决组件 owner

### `BD-COMP-01`：世界覆写字段与普通世界状态的 owner

- 冲突字段：Creative time rate、difficulty、wind/rain normalized input、freeze toggles、
  `StopBiomeSpreadEnabled` 与现有 NLTX `WorldRuleState` 的普通天气/难度字段可能共同影响
  同一消费者。
- 候选 owner A：`SimulationRuleOverrides` 持有所有 Creative normalized authority，
  `WorldRuleState` 继续持有普通天气/难度事实；组合维持本设计的两个相邻 Component 域。
- 候选 owner B：由 `WorldSession` 合并其中一部分为统一世界规则状态；这会减少目标 Component
  字段，但改变 `RuleOverrideStateComponent` 的组成、恢复边界和跨域写入责任。
- 候选 owner C：按时间、天气、生态、难度进一步拆成多个世界 Component；这会增加组件数和
  跨 Component 一致性要求。
- 当前不能宣布最终 owner：`WorldRuleState` 已有普通状态且被多个 NLTX 消费者读取，单会话
  无权把其字段迁移或与 Creative authority 合并。相关字段和 `WorldEntityId` 必须标记
  `crossSubsystemOwner: integration-review`。

### `BD-COMP-02`：权限 Component 的可信 writer 和实体范围

- 冲突字段：`CurrentPermissionByRule` 的更新者是 server config、WorldEntity authority、
  server session authority，还是受控管理入口。
- 候选 owner A：世界实体上的 `RuleOverridePermissionStateComponent`，只允许配置/主机
  authority 修改。
- 候选 owner B：服务器会话实体上的同名 Component，允许受审查的服务端管理写入。
- 候选 owner C：保留协议接收语义但先经过可信 session boundary；这会让权限 Component
  与 NetworkSession 的生命周期产生关系。
- 当前不能宣布最终 owner：Version4 权限模块 `Deserialize` 为空，完整参考的实现不能
  代替 Version4 当前 writer；错误选择会把客户端权限 DTO 提升为权威写入。

### `BD-COMP-03`：玩家 Creative 字段的持久化 owner

- 冲突字段：`GodmodeEnabled`、`FarPlacementRangeEnabled`、`SpawnRateNormalized` 的
  profile/section 归属。
- 候选 owner A：扩展既有 `PlayerPersistentState`；玩家组件与 profile 同生命周期，但
  会扩大 Player persistence owner。
- 候选 owner B：独立 Creative player section；scope 清晰，但会改变保存记录和玩家恢复
  组合关系。
- 候选 owner C：首轮只恢复 world scope，玩家 Component 暂不具备 persistence-ready
  语义；这会明确降低 per-player parity 范围。
- 当前不能宣布最终 owner：Version4 当前 Manager 缺少完整玩家存档成员，完整参考只提供
  同路径补证；当前 NLTX profile 没有 Creative 字段。

### `BD-COMP-04`：快照 owner、revision 和未知兼容记录

- 冲突字段：`Tick`、`SourceRevision`、WorldEntity/PlayerEntity 关系，以及 opaque WLD
  record 是否能参与恢复。
- 候选 owner A：本设计的 WorldEntity world snapshot + PlayerEntity player snapshot；
  访问范围窄，不在世界组件中存玩家字典。
- 候选 owner B：由跨域 Simulation snapshot 持有世界和玩家只读视图；目标 Component 数量
  减少，但共享类型和 owner 扩大。
- 候选 owner C：只保留现有 `WorldRuleSnapshot` 时间投影，延期 Creative snapshot；这会
  推迟多消费者的一致读取能力，不能宣称本子系统完成。
- 当前不能宣布最终 owner：Version4 没有统一 snapshot/revision 类型，多个下游领域会读取
  快照，`RuleSnapshot` 及实体 ID 必须写 `crossSubsystemOwner: integration-review`；未知
  record policy 还会改变恢复时的 Component 创建结果。

## 12. 最终 Component 清单

| Component ID | 名称 | status | componentOwner | crossSubsystemOwner | entityScope | 核心状态 |
| --- | --- | --- | --- | --- | --- | --- |
| `SRO-COMP-WORLD-OVERRIDE` | `RuleOverrideStateComponent` | `status: proposed` | `SimulationRuleOverrides`（candidate） | `integration-review` | one WorldEntity | normalized time/difficulty/wind/rain input、四个世界 toggle、候选 authority revision |
| `SRO-COMP-PLAYER-OVERRIDE` | `PlayerRuleOverrideStateComponent` | `status: proposed` | `SimulationRuleOverrides`（candidate） | `integration-review` | one PlayerEntity | Godmode、FarPlacement、per-player spawn-rate normalized input |
| `SRO-COMP-PERMISSION` | `RuleOverridePermissionStateComponent` | `status: proposed` | `SimulationRuleOverrides`（candidate） | `integration-review` | world-authority 或 session-authority entity（未决） | 按 `RuleKey` 的 default/current permission、候选 permission revision |
| `SRO-COMP-WORLD-SNAPSHOT` | `RuleOverrideSnapshotComponent` | `status: proposed` | `SimulationRuleOverrides`（candidate） | `integration-review` | one WorldEntity | Tick、revision、时间/难度/天气派生值、冻结和生态允许视图 |
| `SRO-COMP-PLAYER-SNAPSHOT` | `PlayerRuleOverrideSnapshotComponent` | `status: proposed` | `SimulationRuleOverrides`（candidate） | `integration-review` | one PlayerEntity | Tick、revision、玩家 toggle 只读视图、spawn multiplier 和禁刷视图 |

该清单是候选组合，不是源码文件创建清单。最终整合必须在 `BD-COMP-01` 至
`BD-COMP-04` 裁决后，重新确认 WorldEntity/PlayerEntity、shared ID、快照值对象和
持久化字段的归属；在裁决前不得把任何一项标记为 `baseline` 或 persistence-ready。

## 13. 最终声明

本文件是 Component-only Design。

本文件不定义 System、Query、Command、Event、Adapter、Projection、
调度顺序、运行时实现、测试计划或迁移计划。

所有 `status: proposed` 的 Component 都只是设计提案。
本文件不声明代码已创建、已迁移、行为等价或验证通过。
