# Main.cs 字段与属性 ECS 迁移方案（排除 ID 类型）

**编写日期：** 2026-08-28  
**旧代码基线：** `D:\TRbackup\Version4物理删除了某些文件\Terraria\Main.cs`  
**对比报告：** [`docs/research/2026-08-28-field-property-migration-comparison.md`](../research/2026-08-28-field-property-migration-comparison.md)  
**适用范围：** `src/Terraria.Dome.Simulation`、`src/Terraria.Dome.Server`、必要的
`src/Terraria.Dome.Protocol.V1456` 投影层

## 1. 结论与边界

Version4 的 `Main.cs` 在词法对比口径下有 686 个字段/属性级声明。Roslyn verifier
当前识别 696 个符号；排除身份型 `worldID` 后，权威迁移作用域为 **695 项**。
对比报告中的 **685 项**是词法侧非 ID 基线，不是删除门的符号分母。这不是把 695 项
机械复制到一个新的 `Main` 类，而是把每一个责任族拆到 Definition、Component、
Snapshot、System、Command 或 Compatibility owner。

当前可确认的结论是：Main 字段模型迁移仍为 **partial**。服务端时间、天气、侵袭/
Slime Rain、世界元数据、世界网格、实体 store、快照和部分持久化已经有代码；完整
静态定义表、全局随机流、延迟处理、实体数组兼容语义以及大量客户端字段尚未等价。
本文件是迁移契约和剩余工作清单，不宣称 Main 全量完成。

本主线按 Flowstate 管理：上下文只保留当前 owner、证据和阻断节点（约 10%），验证采用
风险加权的 focused slice（约 30%），不以窄 verifier 的绿色结果替代全量 parity。

### 1.1 ID 排除规则

“忽略 ID 类型”按报告采用“身份语义”解释，而不是看到 `int` 就排除。仅排除下列
四个基线声明：

| 旧声明 | 行号 | 排除理由 |
|---|---:|---|
| `NPC.netID` | — | NPC 网络/定义身份编号 |
| `Main.worldID` | 1810 | 世界身份编号属性/字段 |
| `Projectile.identity` | — | 投射物实例身份编号 |
| `MessageBuffer.whoAmI` | — | 连接/玩家槽位身份编号 |

身份字段仍必须存在于 ECS 的实体寻址、网络复制、存档关联和生命周期契约中；本文件
不删除 `WorldMetadata.WorldId`、实体 handle 或 replication key。`type`、`index`、
`slot` 等普通业务字段不是 ID，继续纳入迁移范围。

### 1.2 ECS 不变量

1. `Simulation` 不引用 `Terraria.Main`、XNA、UI、输入设备、Socket 或 Protocol/Server。
2. Definition 只描述稳定规则和静态表；Component 只保存实体/世界运行时状态；Snapshot
   是不可变的读模型和持久化边界。
3. System 读取 snapshot/component，产生显式 Command；Command 按确定性顺序提交，禁止
   直接改写另一个系统的内部容器。
4. Server 负责 tick、权限、持久化和网络投影；Protocol 只编码/解码快照和事件，不能
   反向成为 Simulation 的状态 owner。
5. 客户端表现字段只保留在客户端适配器；不能因为旧字段位于 Main 就导入服务器 ECS。

## 2. Main 字段/属性责任族迁移矩阵

下表按旧声明族覆盖词法侧 685 项非 ID 基线；Roslyn 权威迁移作用域为 695 项（另有 1 项身份排除）。
`已落地` 表示当前有可定位的 owner；不表示
该族已经完成旧行为的默认值、生命周期、持久化和协议等价。

| 责任族（旧 Main 成员） | 数量口径 | ECS 目标 owner | 当前状态 | 主要缺口与验收条件 |
|---|---:|---|---|---|
| 世界元数据与坐标：`worldName`、`leftWorld/rightWorld`、`topWorld/bottomWorld`、`maxTilesX/Y`、`sectionWidth/Height`、`maxSectionsX/Y`、`spawnTileX/Y`、`worldSurface`、`rockLayer` | 约 20 | `WorldMetadata`、`WorldGrid`、边界/坐标查询系统 | **partial / 已落地窄片** | `rockLayer` 已由 `WorldMetadata.RockLayer` 持有，并由 `WorldGenerationRequest` 从生成入口固化；legacy compatibility import、`WorldPersistenceFormat` 与嵌入式 Dome snapshot 已覆盖传播和 round-trip。其余仍需要完整边界、出生点、存档恢复和 section revision verifier；不得再以静态 Main 字段读取。 |
| 世界种子与模式：`drunkWorld`、`getGoodWorld`、`tenthAnniversaryWorld`、`dontStarveWorld`、`notTheBeesWorld`、`remixWorld`、`noTrapsWorld`、`zenithWorld`、`skyblockWorld`、`vampireSeed`、`infectedSeed`、`teamBasedSpawnsSeed`、`dualDungeonsSeed`、`DefaultSeed`、`WorldGeneratorVersion` | 约 18 | `WorldSeed`、`WorldMetadata`、`WorldProgressionState`、WorldGen Definition registry | **partial** | 当前 `WorldMetadata` 仅覆盖部分 seed variant；必须补齐各 seed 对生成、NPC、掉落和规则的影响，并保存随机流版本。 |
| 时钟与时间速率：`dayLength`、`nightLength`、`dayTime`、`time`、`moonPhase`、`GlobalTimeWrappedHourly`、`GlobalTimerPaused`、`dayRate`、`desiredWorldTilesUpdateRate`、`GameUpdateCount`、`timeForVisualEffects` | 约 15 | `WorldClock`、`WorldClockSnapshot`、`WorldTimeRateSnapshot`、`WorldClockSystem`、`WorldUpdateRatePolicy` | **accepted narrow / 已验证部分** | `desiredWorldTilesUpdateRate` 已由 `WorldUpdateRatePolicy.GetRate` 承接为有界派生策略（冻结时归零、上限 24），不与持久化 `dayRate` 混同。`timeForVisualEffects` 只能作为投影值；需继续验证暂停、跨日、月相、恢复和溢出。`GameUpdateCount` 不能回流为静态全局计数器。 |
| 天气与风：`raining`、`rainTime`、`maxRaining`、`oldMaxRaining`、`windSpeedCurrent/Target`、`windCounter`、`extremeWindCounter`、`windPhysics`、`windPhysicsStrength`、`lowWindLimit`、`cloudAlpha` | 约 20 | `WorldRuleState`、`WorldWeatherSystem`、`WorldEnvironmentTransition` | **accepted narrow / 已验证部分** | 需要旧随机冷却、最大雨量、风物理和重启持久化的 differential；表现用 `WindForVisuals` 不得进入 Simulation。 |
| 世界进度与事件：`hardMode`、`bloodMoon`、`eclipse`、`pumpkinMoon`、`snowMoon`、`invasionType`、`invasionX`、`invasionSize`、`invasionDelay`、`invasionWarn`、`invasionSizeStart`、`invasionProgress`、`invasionProgressMax`、`invasionProgressWave`、`slimeRainTime`、`slimeRain`、`slimeRainKillCount`、`slimeWarningTime/Delay`、`isThereAWorldSurface` | 约 45 | `WorldProgressionState`、事件 Commands、`WorldProgressionSystem`、侵袭/Slime Rain/Meteor systems | **partial / 多个 accepted slice** | 全事件联动、旧进度字段默认值、事件结束副作用、存档兼容和 NPC spawn/loot 联动仍缺。 |
| 世界网格与液体：`tile`、`liquid`、`liquidBuffer`、`liquidAlpha`、`maxLiquidTypes`、`waterStyle`、`Setting_UseReducedMaxLiquids`、`tileFrame`、`tileFrameCounter` | 约 20 | `WorldGrid`、`WorldTile`、`LiquidWorldStateComponent`、液体 input/propagation/commit systems、`WorldGridSnapshot`、`LegacyLiquidTypeCapacity` | **partial / capacity accepted-narrow** | `maxLiquidTypes=15` 已由兼容容量 owner 承接，但 Tile/Wall 完整 frame 与液体旧队列尚未完全等价；所有变更必须走 `TileChangeCommand`/liquid command 后确定性 commit。 |
| 实体存储：`player`、`npc`、`projectile`、`item`、`gore`、`rain`、`dust`、`star`、`cloud`、`combatText`、`chest`、`sign`、`maxPlayers`、`maxNPCs`、`maxProjectiles`、`maxItems`、`maxChests`、`maxGore` | 约 35 | `PlayerStore`、`NpcStore`、`ProjectileStore`、`WorldItemStore`、WorldObjects stores、`SimulationEntityLimits`、Arch entities | **partial** | `maxPlayers/maxNPCs/maxProjectiles/maxItems/maxChests` 已有 bootstrap、运行时拒绝和 focused verifier；旧数组的 slot reuse、active 标志、顺序和完整复制语义仍不能仅以 Dictionary 替代，每个 store 仍需完整 spawn/despawn、快照和容量 verifier。 |
| 实体运行时汇总：`ActivePlayersCount`、`SleepingPlayersCount`、`AnyActiveBossNPC`、`HadAnActiveInteractableProjectile`、`checkForSpawns`、`ProjectileUpdateLoopIndex`、`lastItemUpdate`、`maxItemUpdates` | 约 15 | 派生 query、`SimulationTickContext`、各域统计 snapshot | **partial** | 只能由当前 tick 的查询计算，禁止公共静态可写缓存；需证明统计值在 tick 顺序和实体删除后稳定。 |
| NPC/玩家/投射物定义表：`projHostile`、`projHook`、`projPet`、`projFrames`、`npcFrameCount`、`slimeRainNPC`、`tile...`/`wall...` 数组、`npcCatchable`、`pvpBuff`、`persistentBuff`、`meleeBuff`、`debuff`、`vanityPet`、`buffNoSave`、`buffNoTimeDisplay`、`tileSolid`、`tileSolidTop`、`tileContainer`、`tileSign` 等 | 约 130 | 各域 `Definition`/registry（Projectile、Tile、Buff、NPC） | **deferred / 仅部分定义** | 已有窄片包括 `tileRope`、`tileContainer`、`tileSign`、`tileSolidTop`、`tileSolid`、`tileLavaDeath`、`tileFlame`、`tileLighted`、`npcCatchable`、`tileObsidianKill`、`tileOreFinderPriority`、`tileMergeDirt`、`tileMerge`、`tileBrick`、`wallHouse`、`tileBlockLight`、`wallLargeFrames`、`npcFrameCount`、`slimeRainNPC`、`projPet`、`meleeBuff`、`persistentBuff`、`lightPet`、`vanityPet`、`tileBlendAll`、`tileShine`、`tileShine2`、`tileGlowMask`、`townNPCCanSpawn`；其余静态表仍需逐表补来源、版本、默认值和使用方 verifier。 |
| 背景、天空、星云与视觉缓存：`treeBGSet*`、`corruptBG`、`jungleBG`、`snowBG`、`hallowBG`、`crimsonBG`、`mushroomBG`、`underworldBG`、`treeX/treeStyle`、`caveBack*`、`maxStars`、`numStars`、`maxClouds`、`numClouds` | 约 80 | 客户端 Render/Presentation adapter；服务器仅保留必要世界规则 | **excluded（客户端）** | 不迁入 Simulation。只有能证明改变权威世界状态的子字段，才另建 World Definition；其余保持客户端边界。 |
| 图形、镜头、音频与粒子：`graphics`、`GameViewMatrix`、`Camera`、`screenPosition`、`screenWidth/Height`、`musicPitch`、`musicFade`、`ParticleSystem_*`、`AmbientWindSystem`、`ambient*`、`shimmerAlpha`、`BlackFadeIn` | 约 90 | 客户端表现层 | **excluded（客户端）** | 不创建服务器 Component；协议只传服务器决定的结果，不传渲染缓存。 |
| 输入与光标：`mouseX/Y`、`mouseRight`、`mouseRightRelease`、`keyState`、`SmartCursorWanted_*`、`SmartInteractTileCoords*`、`TileInteraction*`、`cursorOverride`、`mouseColor*`、`MouseScreen` | 约 35 | `PlayerInputComponent`（服务器接收的 typed input batch）+ 客户端输入 adapter | **partial / 输入窄片** | 本地键盘/鼠标对象必须留在客户端；服务器只接受带 tick、序列和玩家权限的输入命令。 |
| UI、菜单、提示与社交：`MenuUI`、`InGameUI`、`gameMenu`、`motd`、`statusText`、`helpText`、`AnnouncementBox*`、`HoverItem`、`showItemText`、`currentNPCShowingChatBubble`、`LocalGolfState`、`Pings`、`chatMonitor` | 约 70 | 客户端 UI；必要聊天/社交消息走 Protocol projection | **excluded / compatibility** | 不把 UI 状态当世界状态；聊天、商店、Golf 等需单独协议/域模型决策，不能隐式挂到 Main。 |
| 资源、文件和加载：`Assets`、`WorldFileMetadata`、`PlayerList`、`ActivePlayerFileData`、`WorldList`、`ActiveWorldFileData`、`WorldPath`、`CloudWorldPath`、`PlayerPath`、`Configuration`、`InputProfiles`、`libPath`、`saveTime`、`AutogenProgress` | 约 35 | Server `WorldBootstrap`、`WorldSaveCoordinator`、`DomeStatePersistence`、Definition loading boundary | **partial / boundary exists** | `WorldRollingBackupsCountToKeep` 已接入 `WorldSaveCoordinator` 的有界 rolling-backup 策略并通过 focused verifier；其余 Simulation 不得访问文件，需把加载结果转为 `WorldBootstrapRequest` 和 value-only snapshot，并完成版本/失败回滚验证。 |
| 随机数与临时随机流：`rand`、`TileFrameSeed`、`_drawRand`、`_tempSeededRandom`、`Main.rand` 调用隐含状态 | 约 10 | 域级 seeded random component/stream、`GenerationRandomState`、`WorldEventRandomState` | **deferred** | 不能用一个全局 `UnifiedRandom` 替代；必须固定 stream owner、seed、版本和调用顺序，完成 WorldGen differential 后再开删除门。 |
| 主线程/延迟工作：`DelayedProcesses`、`DelayedProcessesInGame`、`_mainThreadActions`、`OnEnginePreload`、`OnEngineLoad`、`OnTickForThirdPartySoftwareOnly`、`OnTickForInternalCodeOnly` | 约 15 | `SimulationCommandQueue` 或 Server lifecycle queue；事件订阅在 Host 层 | **partial** | 逐项判定是否权威、是否可重放；禁止把 `IEnumerator`/`Action` 直接放入 Simulation 状态。 |
| 网络/服务器运行参数：`dedServ`、`netMode`、`_targetNetMode`、`MaxTimeout`、`netPlayCounter`、`verboseNetplay`、`stopTimeOuts`、`defaultIP`、`getIP/getPort`、`menuServer`、`menuMultiplayer` | 约 30 | `DomeServer`、Session/transport state、ServerLaunchOptions | **partial / host 已存在** | `maxNetPlayers` 已明确记录为 `deferred-host`：它是 legacy 网络槽位上限，不是 Simulation 世界实体容量；`netMode` 不是 Simulation 规则，连接状态、超时、地址和菜单状态留在 Server/Transport，Simulation 只接收授权命令。 |
| 商店、背包和世界物品辅助：`Inventory*Slots*`、`maxMP`、`mouseItem`、`guideItem`、`reforgeItem`、`shop`、`travelShop`、`TravelShopMaxSlots`、`anglerQuest*`、`itemAnimations*` | 约 45 | Inventory/Equipment/Item Definition、Shop/Quest domain（未完成部分保持 deferred） | **partial** | Item 已有定义和部分 snapshot，但 Shop、完整 SetDefaults、prefix、动画和 Angler 任务不能以 Main 字段别名交付。旧库存布局（物品 50、金币 4、弹药 4、总计 58）与当前 `InventoryComponent` 的 40 槽运行时容量、`PlayerPersistentState` 的 990 槽持久化布局不等价，相关 `Start/Count/Total` 符号保持 deferred，不能新增同名 alias。 |
| 世界对象与机关：`sectionManager`、`chest`、`sign`、`sittingManager`、`sleepingManager`、`TeleportPylonsSystem`、`WireNetwork` 相关缓存 | 约 25 | Chest/Sign/Door/TileEntity components、`WireNetworkComponent`、commands/systems、replication snapshots | **partial / 窄片已落地** | Chest 40 槽、revision、ownership 已有快照；Wiring 传送器、炮、Hopper、PixelBox 和旧 scratch 队列仍缺。 |
| 调试、性能和平台：`fpsTimer`、`dedServFPS`、`dedServCount*`、`renderCount`、`updatesCountedForFPS`、`NoPooling`、`CollectGen0EveryFrame`、NativeMethods、窗口函数 | 约 35 | Host diagnostics/metrics；平台 adapter | **excluded / compatibility** | 不进 ECS 状态，不参与存档或确定性回放；指标可由 Host 订阅 tick trace。 |

数量为责任族的审计分组，不应相加后替代词法侧 685 项基线或 Roslyn 的 695 项作用域；跨族拆分和表达式体
属性会造成词法计数与语义计数差异。最终验收必须回到 Roslyn 符号清单。

## 3. 目标数据流

```text
WorldBootstrapRequest
    -> WorldMetadata + WorldSeed + WorldGrid
    -> WorldClock / WorldRuleState / WorldProgressionState
    -> System 读取快照和 Components
    -> typed Command（带 tick/sequence/authority）
    -> deterministic commit
    -> DomeSimulationSnapshot
    -> Server persistence / Protocol projection
```

实体字段使用同一条链路：

```text
Definition（不可变规则）
    + Component（Arch entity runtime state）
    -> Domain System
    -> Lifecycle/Replication/Persistence Snapshot
```

禁止的替代方案是 `MainFieldsComponent`、`MainStateManager` 或一个重新聚合全部字段的
God Object。这些类型会隐藏 owner，破坏 ECS 的读写边界，也无法证明字段的生命周期和
序列化语义。

## 4. 分阶段实施计划

### 阶段 A：Roslyn 清单与证据冻结

- 从 Version4 `Main.cs` 建立 `SymbolId、声明行、可见性、静态性、类型、默认值、读写次数、
  客户端/服务器证据` 清单。
- 用语义规则排除身份声明；词法基线保留 685 行，但删除门使用 Roslyn 的 695 项迁移作用域。
- 对表达式体属性、跨行声明、嵌套类型和事件单独建记录，不能继续依赖纯正则计数。
- 输出版本化 manifest、源文件 SHA-256 和每项 owner 状态；新旧声明不做同名率评分。

### 阶段 B：先完成世界运行时骨架

1. `WorldMetadata`/`WorldGrid`：尺寸、section、出生点、名称、seed variant、生成器版本。
2. `WorldClock`/`WorldClockSystem`：时间、日夜、月相、暂停、速率和快照恢复。
3. `WorldRuleState`/`WorldWeatherSystem`：雨、风、模式和 PVP 规则。
4. `WorldProgressionState` 及事件系统：Hardmode、Blood Moon、Eclipse、Invasion、Slime
   Rain、Meteor、Lantern Night。
5. 每一步都写 snapshot/persistence/loopback verifier，再扩展下一族。

### 阶段 C：实体 stores 与边界

- 将 `Main.player/npc/projectile/item/chest/sign` 数组迁移到有容量限制的 domain store。
- `active`、despawn、slot reuse 和复制 revision 由生命周期 Component/快照表达。
- 读路径只暴露 `IReadOnlyList`/快照；写路径只接受 spawn/despawn/interaction command。
- 先验证 Player/NPC/Projectile 的核心移动、生命、伤害和复制，再补专用机制。

### 阶段 D：Definition 表与随机流

- 把 `tileSolid` 等静态数组按 Tile/NPC/Projectile/Buff 域拆成不可变 registry。
- 为每个 registry 增加来源锚点、默认值、版本和 verifier；未覆盖的表标记 `deferred`。
- 将 `Main.rand`、`TileFrameSeed` 和 WorldGen 随机调用拆为域级 stream，记录 seed、stream
  version 和消费序列。

### 阶段 E：删除门与兼容层

只有满足以下条件，才允许删除对应 Main 字段或引用：

- 有 Definition/Component/System/Snapshot 的明确 owner；
- 默认值、可变性、生命周期和异常路径均有证据；
- 持久化 round-trip、协议投影和重启恢复已验证；
- 与 Version4 的窄行为 differential 通过；
- MainBoundary verifier 确认 Simulation 无旧 Main/客户端/Server/Protocol 依赖；
- 未覆盖行为进入 `deferred` ledger，而不是静默删除。

## 5. 验证矩阵

| 验证层 | 必须证明的事实 | 当前证据/状态 |
|---|---|---|
| 结构边界 | Simulation 不引用 `Terraria.Main`、XNA、UI、网络和 Server | `Test/Terraria.Dome.MainBoundary.Verification` 已提供边界扫描；持续运行 |
| 世界规则 | 时钟、天气、事件按 tick 顺序产生稳定 transition | `WorldClock`、`WorldWeatherSystem`、`WorldProgressionSystem` 有 focused verifier；全量 Main parity 未完成 |
| 持久化 | metadata、clock、rules、progression、grid 可 round-trip | `DomeStatePersistenceFormat` 与 Persistence tests 已覆盖部分字段；新增字段必须版本化 |
| 实体生命周期 | spawn/despawn、active、容量、复制 revision 和 slot reuse 不丢状态 | Player/NPC/Projectile stores 为 partial；需要独立 lifecycle/replication verifier |
| 定义表 | registry 的长度、默认值、版本和调用方与旧表一致 | 当前仅部分定义已落地；其余 deferred |
| 随机确定性 | 相同 seed/输入/stream version 生成相同结果，且不受客户端路径影响 | WorldGen differential 仍失败；`canRemoveLegacyWorldGen=false` |
| 协议投影 | 只从 snapshot/event 投影，客户端字段不反向写 Simulation | Protocol/Server adapter 已有窄片；完整 legacy message parity partial |

## 6. 当前状态与下一抓手

### 已确认可复用的 owner

- `src/Terraria.Dome.Simulation/World/WorldMetadata.cs`
- `src/Terraria.Dome.Simulation/World/WorldClock.cs`
- `src/Terraria.Dome.Simulation/World/WorldRuleState.cs`
- `src/Terraria.Dome.Simulation/World/WorldProgressionState.cs`
- `src/Terraria.Dome.Simulation/World/WorldGrid.cs` 与 `WorldGridSnapshot`
- `src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs`
- `src/Terraria.Dome.Simulation/World/Systems/WorldClockSystem.cs`
- `src/Terraria.Dome.Simulation/World/Systems/WorldWeatherSystem.cs`
- `src/Terraria.Dome.Server/Startup/WorldBootstrap.cs`
- `src/Terraria.Dome.Server/Persistence/DomeStatePersistenceFormat.cs`

### 必须保持 deferred 的高风险族

- 完整 `tile...`/`wall...`/`proj...`/`buff...` 静态表；
- `Main.rand` 的全局调用顺序和 WorldGen 生成随机流；
- `DelayedProcesses`、`_mainThreadActions` 和事件委托的真实服务器语义；
- 实体数组的 slot reuse、active 标志与旧复制顺序；
- 商店、Angler、Golf、Mount、完整装备/Prefix 和专用事件；
- 所有纯客户端图形、音频、UI、输入、窗口和渲染缓存。

下一步最高价值工作是完成 Roslyn 级 695 项迁移作用域清单（另有 1 项身份声明排除），并为每一项补齐五列：
`default value`、`mutability`、`lifecycle owner`、`serialization`、`verification evidence`。
没有这五列，新增字段数量不能构成迁移完成证据。

### 6.1 Roslyn 清单首个可复现产物

已加入只读 verifier：
`Test/Terraria.Dome.MainFieldPropertyInventory.Verification/`。运行后生成
`Build/diagnostics/main-field-property/inventory.json`，记录源文件 SHA-256、声明行、
字段/属性类型、静态性、默认值、可变性，以及 owner/serialization/verification 状态。
同一 verifier 还对 `Main.worldID`、`NPC.netID`、`Projectile.identity`、
`MessageBuffer.whoAmI` 做跨文件语法审计；四项均必须实际找到声明，否则运行失败。
当前已将 115 个可由现有代码直接证明的字段/属性符号标记为 `accepted-narrow`：世界名称、尺寸、
边界、section 尺寸、出生点、世界表面、生成器版本，以及 Hardmode、Blood Moon、Eclipse、
Slime Rain、侵袭类型/规模/延迟/位置，以及雨状态/时长/最大强度和风速目标/当前值；
实体容量（玩家、NPC、投射物、世界物品）、Remix 世界标志，时钟的日夜、时间、月相、暂停状态、
周期长度和权威时间速率，以及世界 GameMode、Journey/Expert/Master 派生状态和 Difficulty，
以及侵袭/Slime Rain 瞬时警告计时、按 `>50` 阈值计算的世界表面存在性和侵袭进度的数值/上限/图标投影，
以及 `wallLargeFrames` 的静态墙体 frame-size 值表、`anglerQuestItemNetIDs` 的有序任务鱼
Item 类型表、`projFrames` 的投射物 frame-count 覆盖表、`npcFrameCount` 的 NPC frame-count
默认表、`slimeRainNPC` 的 Slime Rain NPC 类型集合、`projPet` 的投射物宠物类型集合、
`debuff` 的 debuff 类型集合、`meleeBuff` 的 melee buff 类型集合、`persistentBuff` 的持久化
分类集合、`lightPet` 的 light-pet buff 类型集合、`vanityPet` 的 vanity-pet buff 类型集合，以及
`pvpBuff` 的 message-55 PvP relay allowlist 与服务端权威状态窄片，以及 `tileBlendAll`、
`tileShine`、`tileShine2`、`tileGlowMask`、`maxLiquidTypes` 的 source-backed 静态/容量窄片；
警告计时、世界表面存在性和进度投影不进入持久化字段。
其余符号继续保持 `unmapped/deferred`，
不因名称相似而自动提升状态。边界值是从 `WorldMetadata`/`WorldGrid` 派生的只读值，
不增加独立持久化字段。

#### 6.1.1 背包布局差异审计（2026-08-28）

Roslyn manifest 定位到旧 `Main` 的 7 个库存布局常量：
`InventoryItemSlotsStart=0`、`InventoryItemSlotsCount=50`、
`InventoryCoinSlotsStart=50`、`InventoryCoinSlotsCount=4`、
`InventoryAmmoSlotsStart=54`、`InventoryAmmoSlotsCount=4`、
`InventorySlotsTotal=58`（其中 `Start/Count/Total` 为同一布局契约）。

当前 ECS 证据不是同一布局：

- `src/Terraria.Dome.Simulation/Items/InventoryComponent.cs` 的权威运行时容量为
  `SlotCount=40`，其中 `HotbarSlotCount=10`；`InventorySnapshot` 对 selected slot 也按
  10 槽 hotbar 校验。
- `src/Terraria.Dome.Simulation/Players/PlayerPersistentState.cs` 的持久化 `ItemSlotCount=990`，
  构造器要求完整 990 项并按 slot id 校验。
- `src/Terraria.Dome.Server/Protocol/PlayerPersistentStateMapper.cs` 将 990 槽状态投影为连接可见的
  基础槽 `0..98` 与 loadout 槽 `900..989`；没有 50/4/4 的旧 Main 分段 adapter。
- `Test/Terraria.Dome.Items.Verification` 已验证 40 槽 runtime、10 槽 hotbar、990 槽持久化导入以及
  可见投影，不能把这些通过结果解释成 58 槽旧布局 parity。

因此这 7 个 `Main.Inventory*Slots*` 符号当前记录为 `unmapped/deferred`。只有在建立显式的
legacy layout contract、边界 adapter、版本化持久化/协议映射，并补齐 50/4/4 分段 differential
验证后，才允许提升状态或开启删除门。

#### 6.1.2 最近服务器历史与旅行商店容量审计（2026-08-28）

`Main.maxMP=10` 的旧声明紧邻 `recentWorld`、`recentIP` 和 `recentPort` 三组客户端最近连接
历史数组；它不是 `ManaComponent` 或玩家 `MaximumMana` 的容量。当前仓库没有对应的
服务器最近历史持久化 owner，也没有把客户端菜单历史导入 Simulation 的理由，因此该符号
保持 `unmapped/deferred`，不能误接到玩家战斗状态。

`Main.TravelShopMaxSlots=40` 只被 legacy `NetMessage` 的旅行商店消息（消息 72）用于遍历
`Main.travelShop` 并编码固定长度数组。当前 Protocol 仍保留兼容编解码边界，但 Simulation/Server
没有旅行商店库存、刷新生命周期、持久化或权威商店定义 owner。该容量因此保持
`unmapped/deferred`；消息兼容存在不等于 Main 字段已经迁移。

后续若实现这两类功能，必须分别建立 Host 最近历史 contract、Shop definition/state/system、
协议 projection 和持久化/刷新 verifier，再重新评估删除门。

`Main.tileRope` 的窄片证据：旧数组初始化的 9 个 rope 类型与
`TileRopeQuery.RegisterDefaults()` 完全相同，WorldGeneration verifier 已覆盖集合长度、关键
类型、重复注册稳定性以及 `FindEnds`/`IsRope` 查询。该映射只覆盖 rope 子表；其他 tile
静态表必须分别依据各自 registry 和 verifier 判定，不能仅凭名称外推完成。

`Main.tileContainer` 已形成精确窄片：旧表的 5 个类型（21、88、467、470、475）与
`LegacyTileContainerRegistry.RegisterDefaults()` 完全一致，Wiring verifier 做双向集合断言，
并显式拒绝现代 protection 集合额外的 441、468。`TileBreakabilityProtectionRuleSystem`
仍保留自身的 7 类型运行时保护集合，不能反向把现代集合当作 legacy table。
`Main.tileSign` 也已形成精确窄片：旧表的 4 个类型（55、85、425、573）与
`LegacySignTileRegistry.RegisterDefaults()` 完全一致，并由 focused verifier 做集合和负例
断言。`SignValidationQuery` 继续负责 frame/attachment 几何行为，不能把类型 registry
误当作完整 sign 生命周期、存档或协议 parity。

`Main.tileSolidTop` 已形成精确窄片：旧数组的 79 个显式类型与
`LegacySolidTopTileRegistry.RegisterDefaults()` 双向一致；当前 `TileDefinitionRegistry`
的 platform 集合额外包含 435、436、437、438、439，仍不作为 legacy alias。WorldGeneration
verifier 覆盖完整集合和 435 负例，故该映射只承接旧静态表，不改变现代 platform 语义。

`Main.tileSolid` 已形成精确窄片：旧表的 299 个显式类型与
`LegacySolidTileRegistry.RegisterDefaults()` 双向一致；现代 `TileDefinitionRegistry` 的
solid 集合额外包含 `255..268`、`435..439` 和 `727..732`，这些版本新增项不纳入 legacy
owner。WorldGeneration verifier 覆盖完整集合及 `255` 负例；该映射只承接旧静态固体表。

`Main.tileNoAttach` 已形成精确窄片：Legacy 显式赋值加上 `435..439` 动态循环共 67 个
类型，与 `TileDefinitionRegistry` 的 `IsNoAttach` 集合双向一致；WorldGeneration verifier
覆盖完整集合、动态范围边界和非成员负例。该映射只承接 no-attach 静态分类，不替代局部
几何、平台和液体规则。

`Main.tileBrick` 已形成精确窄片：旧初始化有 214 次显式 `true` 写入，其中 5 个类型重复，
最终得到 209 个唯一类型；该集合与 `LegacyBrickTileRegistry.RegisterDefaults()` 双向一致。
该 registry 独立于 cracked-brick、solid 和 dungeon 子集，不把这些局部定义拼接成旧表 owner；
本窄片也不宣称完整 brick 生成、墙体或其他运行时行为 parity 已完成。

`Main.tileFrameImportant` 是本轮可接受的精确窄片：旧数组的显式集合与
`TileFrameImportantRegistry.RegisterDefaults()` 均为 397 项，机械比较的双向差集为空。
WorldGeneration verifier 已覆盖 registry 稳定性和 `Contains` 查询；该映射只覆盖
`tileFrameImportant`，不改变其他 tile 静态表仍为 deferred 的结论。

`Main.tileCut` 也已形成精确窄片：旧数组的显式集合为 41 项，与
`LegacyTileRunnerCandidateRegistry.IsTileCut` 背后的不可变集合双向一致；WorldGeneration
verifier 已覆盖 `TileCutCount` 和 `IsTileCut`。该映射只覆盖切割工具分类，不代表
其他工具静态表已完成。

`Main.tileHammer` 已形成精确窄片：旧数组的 4 个类型（26、31、695、696）与
`LegacyHammerTileRegistry.RegisterDefaults()` 双向一致；WorldGeneration verifier 已覆盖
集合、边界类型和 `FrozenSet` 不可变投影。该 registry 只表达旧 hammer 目标分类，
不替代 `TilePoundingEligibilityQuery` 的位置、邻接和生成阶段行为。

`Main.tileAxe` 已形成精确窄片：旧数组的 16 个类型（5、72、80、323、488、583--589、
596、616、634、704）与 `LegacyAxeTileRegistry.RegisterDefaults()` 双向一致；
WorldGeneration verifier 已覆盖集合、边界类型和 `FrozenSet` 不可变投影。该 registry
只表达旧 axe 目标分类，不替代树木 profile/eligibility 查询或 `ItemToolDefinition.AxePower`。

`Main.tileMoss` 是本轮可接受的精确窄片：旧数组与 `MossTileTypeRegistry.TileTypes` 均为
11 个类型（179、180、181、182、183、381、534、536、539、625、627），双向集合差集为空。
WorldGeneration verifier 已覆盖 moss registry、颜色查询和相关生成行为；该映射只覆盖
moss 标记集合，不代表 moss 颜色、随机选择或其他 tile 表已经完成。

`Main.tileStone` 也已形成精确窄片：旧数组显式集合为 9 个类型（63、64、65、66、67、68、
130、131、566），与 `LegacyTileRunnerTargetRegistry` 的 stone 集合双向一致。现有
WorldGeneration verifier 已覆盖 stone 数量及关键类型查询；该映射只覆盖 TileRunner 的
stone target 分类，不外推为 `tileSolid` 或其他 tile 静态表 parity。

`Main.tileDungeon` 形成精确窄片：旧数组显式集合为 6 个类型（41、43、44、677、678、679），
与 `LegacyEvilReplacementDefinitions.CreateDefault().DungeonTileTypes` 双向一致。现有
WorldGeneration verifier 已断言该集合及 dungeon replacement 边界；该映射只覆盖 dungeon
tile 分类，不外推为 dungeon wall、platform 或其他 tile 表 parity。

`Main.tileWaterDeath` 形成精确窄片：旧数组显式集合为 10 个类型（4、51、93、98、215、
372、405、552、646、697），与 `TileDefinitionRegistry` 的 `waterDeathTypes` 双向一致，
并由 `TileDefinition.WaterDestroysTile` 暴露语义。该映射只覆盖 water-destroys 分类。

`Main.tileLavaDeath` 已形成精确窄片：legacy 的 262 个显式类型加上 `435..439` 范围
初始化后共 267 个类型，与 `TileDefinition.LavaDestroysTile` 的 267 项集合一致。Liquid
verifier 覆盖总数及范围边界类型；该映射只承接全局液体死亡定义，不替代 `TileObject`
局部液体规则。

`Main.tileBouncy` 已形成精确窄片：旧数组的 4 个类型（371、446、447、448）与
`LegacyBouncyTileRegistry.RegisterDefaults()` 双向一致；WorldGeneration verifier 已覆盖
集合、边界类型和 `FrozenSet` 不可变投影。该 registry 只表达 tile-type bounce 分类，
不替代 projectile bounce component 的反弹次数和速度行为。

`Main.tileAlch` 已形成精确窄片：旧数组的 3 个类型（82、83、84）与
`LegacyAlchemicalTileRegistry.RegisterDefaults()` 双向一致；WorldGeneration verifier 已覆盖
集合、边界类型和 `FrozenSet` 不可变投影。该 registry 只表达 alchemical tile 分类，
不替代 style、时间、天气、支撑和液体接触组成的 harvestability 行为。

`Main.tilePile` 已形成精确窄片：Legacy 默认表只标记 `330..333`，与
`LegacyPileTileRegistry.RegisterDefaults()` 双向一致；WorldGeneration verifier 已覆盖集合、
边界类型和 `FrozenSet` 不可变投影。该 registry 只表达旧 pile 静态分类，不把当前
`BoulderTileRegistry` 或 `TilePileValidationQuery` 的局部几何规则误当作同一 owner。

`Main.tileSand` 形成精确窄片：旧数组显式集合为 4 个类型（53、112、116、234），与
`ConversionSandTileRegistry.RegisterDefaults()` 双向一致。WorldGeneration verifier 已覆盖
该 registry 的稳定性及其在转换沙、植物支撑和 tile-frame 查询中的使用；该映射仅覆盖
conversion-sand 分类，不外推为所有 `IsSand` 规则。

`Main.tileCracked` 形成精确窄片：旧数组显式集合为 481、482、483，与
`TileSolidityOverrideQuery.RegisterCrackedBrickDefaults()` 双向一致；同一集合也由
`LegacyEvilReplacementDefinitions.CrackedBrickTileTypes` 复用。WorldGeneration verifier 已
覆盖数量及关键类型查询；该映射只覆盖 cracked-brick 分类，不外推为完整 tile solidity 表。

`Main.wallDungeon` 形成精确窄片：旧数组显式集合为 7、8、9、94、95、96、97、98、99，
与 `LegacyEvilReplacementDefinitions.CreateDefault().DungeonWallTypes` 双向一致。现有
WorldGeneration verifier 已覆盖 dungeon-wall 集合及 replacement 边界；该映射只覆盖 dungeon
wall 分类，不外推为 `wallHouse` 或 `wallLight`。

`Main.wallLight` 已形成精确窄片：旧表的 16 个类型（0、21、106、107、138、139、140、141、
145、150、152、168、245、315、317、318）与 `LegacyWallLightRegistry.RegisterDefaults()`
双向一致；WorldGeneration verifier 已覆盖集合、边界类型和 `FrozenSet` 不可变投影。
`Main.wallHouse` 已形成精确窄片：旧初始化有 267 次显式 `true` 写入，其中 `149` 和 `168`
各重复一次，并通过确定性 `153..166` 循环补充类型，最终得到 279 个唯一 house-wall 类型；
该集合与 `LegacyHouseWallRegistry.RegisterDefaults()` 双向一致。该 registry 只表达旧静态
house-wall 分类，不替代房屋结构、房屋评分、入住资格或完整 housing 行为 parity。

`Main.tileMergeDirt` 已形成精确窄片：旧表有 127 次显式写入，其中 `177` 重复一次，最终
得到 126 个唯一类型；没有 false 覆盖、循环或后续写入，与
`LegacyMergeDirtTileRegistry.RegisterDefaults()` 双向一致。WorldGeneration verifier 已覆盖
集合、边界类型和 `FrozenSet` 不可变投影。该 registry 只表达旧单维 merge-dirt 分类，
不替代 `TileMergeQuery` 的邻接合并算法。

`Main.tileMerge` 的静态默认部分已形成精确窄片。Version4 的 `TileID.Count` 为 `753`；
`SetupTileMerge()` 先创建 `753×753` 的全 `false` 矩阵，随后唯一写入是 `k=0..5` 生成的
六组对称边：`(426,727)`、`(430,728)`、`(431,729)`、`(432,730)`、`(433,731)`、
`(434,732)`，即 12 个有向 `true` 单元，没有其它 `tileMerge[...]` 写入或覆盖。
`LegacyTileMergeRegistry.RegisterDefaults()` 以 `FrozenSet<LegacyTileMergePair>` 保存这 12 条
边，并通过 `TileTypeCount=753` 保留完整默认矩阵的维度和其它单元默认为 `false` 的语义；
WorldGeneration verifier 已覆盖完整边集合、对称性、非成员负例、维度和不可变投影。
该 registry 只承接旧静态默认关系；`TileMergeQuery` 仍是邻接替换/frametest 算法消费者，
不把算法误当作数据 owner。旧源码没有对该表做持久化写入，因此 serialization 边界记录为
derived immutable registry，而不是伪造存档字段；完整 tile framing/runtime parity 仍是独立边界。

`Main.tileObsidianKill` 已形成精确窄片：Legacy 在复制当时的 `tileLavaDeath` 集合后，先以
`[88] = false` 覆盖，再加入 25 个显式类型、9 个 `AddEchoFurnitureTile` 类型和后续的
`324`，得到 276 个有效类型；复制点之后对 `tileLavaDeath` 的赋值不回溯进入该表。该集合与
`LegacyObsidianKillTileRegistry.RegisterDefaults()` 双向一致，WorldGeneration verifier 已覆盖
`88` 排除、`706` helper 边界和 `FrozenSet` 不可变投影。该 registry 只表达旧静态
obsidian-kill 分类，不宣称液体破坏或 Echo 家具行为已完成 parity。

`Main.tileOreFinderPriority` 已形成精确窄片：Legacy 有 36 个唯一 tile-type→short 优先级
赋值，且没有动态写入或覆盖；与 `LegacyOreFinderPriorityRegistry.RegisterDefaults()` 的
36 项 FrozenDictionary 双向键值一致。WorldGeneration verifier 已覆盖完整值域、未登记
类型的缺省边界和关键高优先级值。该 registry 只承接旧探测器优先级数值，不把语义不同的
`OrePatchTileDefinition` eligibility 或 `OreDefinition.Priority` 生成事务排序误当作同一 owner。

`Main.tileBlendAll` 已建立 source-backed 的 `LegacyTileBlendAllRegistry`，精确承接旧表唯一
显式标记的 tile type `357`；这只是 server-owned membership contract，不代表客户端混合渲染
已经迁移。`Main.tileShine2` 已建立 source-backed 的 `LegacyTileShine2Registry`，承接当前
源中 52 个显式标记与 `262..268` 确定性循环，最终得到 59 个唯一 tile type；这仍只是静态
membership contract，不代表客户端 shine 渲染 parity。`Main.tileShine` 已建立 source-backed
的 `LegacyTileShineRegistry`，精确承接当前源中的 55 个 tile-to-value 数值映射，并通过
`FrozenDictionary` 提供未登记类型的零值边界；这仍只是静态值契约，不代表客户端 shine 渲染
parity。`Main.tileGlowMask` 的旧表包含 37 个 tile-to-mask 数值映射，且该字段主要属于
客户端渲染语义。
`Main.tileGlowMask` 现已建立 source-backed 的 `LegacyTileGlowMaskRegistry`，精确承接 37 个
覆盖值，并保留初始化默认值 `-1` 及未登记/越界类型边界；该 owner 只表达静态 mask 数据，
不宣称客户端 glow-mask 绘制 parity。

`Main.wallLargeFrames`（声明于 Version4 `Main.cs:926`）已形成精确窄片：Version4 使用
`WallID.Count=367` 的 byte 表，默认值为 0，初始化阶段 `Main.cs:6889-7076` 只有 22 个
字面量非零写入（值为 1 或 2），且源码没有其它写入点；当前 owner 为
`LegacyLargeFrameWallRegistry.RegisterDefaults()`，返回冻结的稀疏值表。运行时
墙体 frame 算法和客户端渲染仍不在本片范围内。

`Main.anglerQuestItemNetIDs`（声明于 Version4 `Main.cs:1200`）已形成精确窄片：旧表长度为
41，索引 `0..40` 到 Item 类型的有序映射为 `2450..2488` 后接 `4393, 4394`，所有值均落在
Version4 `ItemID.Count=6147` 内且没有重复。唯一读取点位于 `AnglerQuestSwap`，先按该索引表
随机选择，再由 hardmode、crimson 与 world-surface 条件过滤；这些动态选择、完成记录、存档
和协议生命周期仍不在本片范围内。当前 owner 为
`LegacyAnglerQuestItemRegistry.RegisterDefaults()`，以 `FrozenDictionary<byte, ushort>` 保留
索引语义；`ItemID.Sets.IsQuestFish` 只有相同集合但不提供旧数组的顺序 owner，不能直接 alias。

`Main.projFrames`（声明于 Version4 `Main.cs:797`）已形成静态默认窄片：旧数组容量为
`ProjectileID.Count=1111`，`Initialize_TileAndNPCData1` 在 `Main.cs:5627` 先把全部槽位设为
`1`，随后有 276 次字面量覆盖、275 个唯一索引（`221` 重复但值同为 `3`），所有索引均在
`0..1110` 内，且源码没有其它读取或写入点。当前 owner 为
`LegacyProjectileFrameRegistry.RegisterDefaults()`，以 `FrozenDictionary<ushort, byte>` 保存
显式覆盖并由 `GetFrameCount` 对未覆盖类型返回 `1`。该片只承接静态 frame-count 默认值，
不替代投射物动画、行为、渲染或网络生命周期。

`Main.npcFrameCount`（声明于 Version4 `Main.cs:1297`）已形成完整静态窄片：旧数组长度为
`NPCID.Count=697`，初始化器提供全部 697 项，值域为 `1..30`，源码没有初始化器之外的
写入；`NPC.cs` 的帧选择分支通过该表计算循环、攻击和特殊帧边界。当前 owner 为
`LegacyNpcFrameRegistry.RegisterDefaults()`，以 `FrozenDictionary<int, byte>` 保留每个 NPC
类型的 exact frame-count 值，并对 `-1` 与 `697` 明确拒绝。该 owner 只承接静态帧数量，
不替代 NPC 动画推进、纹理/渲染、AI 或网络生命周期。

`Main.projPet`（声明于 Version4 `Main.cs:799`）已形成精确静态窄片：旧数组容量为
`ProjectileID.Count=1111`，`Initialize_TileAndNPCData1` 在 `Main.cs:5905-6025` 写入
121 次 `true`，对应 121 个唯一类型，未发现 `false` 覆盖、动态索引或其它写入点，全部
落在 `0..1110`。`MessageBuffer.cs:582`、`NetMessage.cs:1763` 以及 `Projectile.cs` 的
`456`、`11512`、`15525`、`50838` 行是按 projectile type 查询该分类的消费者。当前 owner
为 `LegacyPetProjectileRegistry.RegisterDefaults()`，以 `FrozenSet<int>` 保留完整集合并
保持未登记类型（包括 `-1` 和 `1111`）的默认 `false`。当前 `ProjectileDefinition.IsMinion`
是实体定义/组件能力，默认 registry 没有任何 `IsMinion=true` 定义，不能替代旧表；本窄片
不宣称 minion/sentry AI、召唤槽位、owner 权限、网络生命周期或完整投射物 parity。

`Main.slimeRainNPC`（声明于 Version4 `Main.cs:675`）已形成精确静态窄片：旧数组容量为
`NPCID.Count=697`，初始化阶段只有 `Main.cs:6037` 的 `slimeRainNPC[1] = true`，其中
`NPCID.BlueSlime=1`，其它 696 个槽位保持默认 `false`；Legacy 全源码没有发现动态索引、
`false` 覆盖、重复写入或其它 producer。`NPC.cs:64462-64464` 使用该分类计算 Slime Rain
期间的 nearby NPC slot multiplier，`NPC.cs:65558` 使用它推进 Slime Rain death event；这些
调用方证明的是 type classification，不是完整事件 owner。当前 owner 为
`LegacySlimeRainNpcRegistry.RegisterDefaults()`，以 `FrozenSet<int>` 保留唯一类型 1，
并保持未登记类型（包括 `-1`、`0`、`696` 和 `697`）的默认 `false`。该 registry 独立于
`NpcDefinition.IsLikeTownNpc`、`NpcSlotCost` 与 `WorldProgressionState`，不宣称
`slimeRainNPCSlots=0.65f`、spawn selection、kill counter、Slime King 生命周期、loot、
网络复制或完整 Slime Rain/NPC parity。

`Main.meleeBuff`（声明于 Version4 `Main.cs:487`）已形成精确静态窄片：旧数组容量为
`BuffID.Count=389`，初始化阶段只有 `Main.cs:6125-6132` 的 8 次 `true` 写入，唯一类型
集合为 `{71,73,74,75,76,77,78,79}`，其余 381 个槽位保持默认 `false`；Legacy 全源码
没有 `false` 覆盖、动态索引、重复冲突或其它 producer。`Player.cs:3613` 与 `3619` 是
唯一消费者，用于添加 buff 时移除其它 melee buff。当前 owner 为
`LegacyMeleeBuffRegistry.RegisterDefaults()`，以 `FrozenSet<int>` 保留 exact-set，并对
未登记类型（包括 `-1`、`0`、`388` 和 `389`）返回默认 `false`。`BuffID.Sets.IsAFlaskBuff`
额外包含 `72`，不能作为旧表 owner；`BuffCollectionComponent` 只保存实体 buff 实例的
类型、持续时间和来源，也不能替代该静态分类表。Combat focused verifier 已覆盖容量、完整
集合、代表值、边界缺省值和 immutable 容器；本窄片不宣称 buff 添加/替换、免疫、持续时间、
持久化、网络复制或其它 Main buff 表的完整 parity。

`Main.persistentBuff`（声明于 Version4 `Main.cs:481`）已形成精确静态窄片：旧数组容量为
`BuffID.Count=389`，初始化阶段只有 `Main.cs:6345-6352` 的 8 次 `true` 写入，唯一类型
集合为 `{71,73,74,75,76,77,78,79}`，其余 381 个槽位保持默认 `false`；Legacy 全源码
没有 `false` 覆盖、动态索引、重复冲突或其它 producer。`Player.cs:10018` 是明确的
分类消费者，在玩家 reset 时丢弃非持久化 buff 实例。当前 owner 为
`LegacyPersistentBuffRegistry.RegisterDefaults()`，以 `FrozenSet<int>` 保留 exact-set，并对
未登记类型（包括 `-1`、`0`、`388` 和 `389`）返回默认 `false`。该 owner 与使用相同类型集合
但语义不同的 `LegacyMeleeBuffRegistry` 保持独立，也不替代 `BuffCollectionComponent` 的
实例、持续时间、save/load、网络复制或完整玩家 reset 生命周期。TDD RED/GREEN、Simulation、
Combat focused verifier 与 Main inventory 的新鲜证据见
`Build/diagnostics/main-field-property/persistent-buff-20260830-01/`；完整 Buff persistence
和 Main parity 仍为 `partial/deferred`。

`Main.debuff`（声明于 Version4 `Main.cs:489`）已形成精确静态窄片：旧数组容量为
`BuffID.Count=389`，初始化阶段只有 `Main.cs:6038-6108` 的 71 次 `true` 写入，唯一类型
集合为 `{20,21,22,23,24,25,28,30,31,32,33,34,35,36,37,38,39,43,44,46,47,67,68,69,70,72,80,86,87,88,89,94,103,119,120,137,144,145,146,147,148,149,153,156,157,158,160,163,164,169,183,186,189,194,195,196,197,199,203,204,215,320,321,323,324,332,333,334,344,350,353}`，其余 318 个槽位保持默认 `false`；Legacy 全源码
没有 `false` 覆盖、动态索引、重复冲突或其它 producer。`Player.cs:3581` 与 `NPC.cs:76411`
是两个明确的分类消费者，用于选择可复用的 Buff 槽位。当前 owner 为
`LegacyDebuffRegistry.RegisterDefaults()`，以 `FrozenSet<int>` 保留 exact-set，并对未登记
类型（包括 `-1`、`0`、`388` 和 `389`）返回默认 `false`。该 owner 只表达静态 debuff
分类，不替代减益效果应用/移除、免疫、叠加、持续时间、玩家/NPC 生命周期、save/load、
网络复制或完整 Buff parity。TDD RED/GREEN、Simulation、Combat focused verifier 与 Main
inventory 的新鲜证据见 `Build/diagnostics/main-field-property/debuff-20260830-01/`；完整
debuff behavior 和 Main parity 仍为 `partial/deferred`。

`Main.lightPet`（声明于 Version4 `Main.cs:485`）已形成精确静态窄片：旧数组容量为
`BuffID.Count=389`，初始化阶段只有 `Main.cs:6435-6446` 的 12 次 `true` 写入，唯一类型
集合为 `{19,27,57,101,102,152,155,190,201,294,298,299}`，其余 377 个槽位保持默认
`false`；Legacy 全源码没有 `false` 覆盖、动态索引、重复冲突或其它 producer。`Player.cs:3631`
与 `3635`、`Item.cs:980` 是按 buff type 查询该分类的消费者。当前 owner 为
`LegacyLightPetBuffRegistry.RegisterDefaults()`，以 `FrozenSet<int>` 保留 exact-set，并对
未登记类型（包括 `-1`、`0`、`388` 和 `389`）返回默认 `false`。该 registry 只承接静态
light-pet 类型分类，不替代 `BuffCollectionComponent` 的实体实例、宠物 spawn、光照/渲染、
网络复制或完整状态效果 parity。TDD RED/GREEN、Simulation/Combat/inventory 串行门和定向
diff check 的新鲜证据见 `Build/diagnostics/main-field-property/light-pet-20260830-01/`。

`Main.vanityPet`（声明于 Version4 `Main.cs:483`）已形成精确静态窄片：旧数组容量为
`BuffID.Count=389`，初始化阶段只有 `Main.cs:6361-6434` 的 74 次 `true` 写入，唯一类型
集合为 `{40,41,42,45,50,51,52,53,54,55,56,61,65,66,81,82,84,85,91,92,127,136,154,191,
200,202,217,218,219,258,259,260,261,262,264,266,267,268,274,284,285,286,287,288,289,290,
291,292,293,295,296,297,300,301,302,303,304,317,327,328,329,330,331,341,345,349,351,352,
354,356,371,372,373,382}`，其余 315 个槽位保持默认 `false`；Legacy 全源码没有 `false`
覆盖、动态索引、重复冲突或其它 producer。`Item.cs:978` 与 `Player.cs:3641/3647` 是按
buff type 查询该分类的消费者。当前 owner 为 `LegacyVanityPetBuffRegistry.RegisterDefaults()`，
以 `FrozenSet<int>` 保留 exact-set，并对未登记类型（包括 `-1`、`0`、`388` 和 `389`）返回
默认 `false`。该 owner 只表达静态 vanity-pet 分类，不替代 `BuffCollectionComponent` 的
实体实例、宠物 spawn、AI、渲染、save/load、网络复制或完整 Buff/Main parity。TDD RED/GREEN、
Simulation、Combat focused verifier 与 Main inventory 的新鲜证据见
`Build/diagnostics/main-field-property/vanity-pet-20260830-01/`；完整 vanity-pet behavior
和 Main parity 仍为 `partial/deferred`。

`Main.maxLiquidTypes` 已形成兼容容量窄片：`LegacyLiquidTypeCapacity.MaxLiquidTypes` 精确
保留旧编译期容量 `15`，并与当前只定义 4 个已知 `LiquidType` 的 registry 分离；该 owner
只表达协议/数组容量兼容边界，不把 15 误报为 15 个已实现液体类型，也不宣称完整 liquid
行为 parity。

`Main.MaxWorldViewSizeWidth`、`Main.MaxWorldViewSizeHeight` 与 `Main.MaxWorldViewSize` 继续
保持 deferred：旧客户端视野上限为 `1920x1200`，当前服务器仅有按距离的 section replication
边界（512），两者不是同一契约，也没有可持久化的 ECS owner。

`Main.TileFrameSeed` 继续保持 deferred：旧值由客户端启动时 `Guid.NewGuid().GetHashCode()`
派生，当前没有对应的权威 world-generation seed 或持久化字段；不能把普通 `WorldSeed` 直接
当作 tile-frame 随机种子。

`Main.tileLighted` 已形成精确窄片：旧初始化有 146 个显式 true 类型，并通过确定性
`255..268` 循环补充 `262..268`，有效集合共 153 个类型；该集合与
`LegacyLightedTileRegistry.RegisterDefaults()` 双向一致。WorldGeneration verifier 已覆盖
完整集合、循环边界和 `FrozenSet` 不可变投影。该 registry 只承接旧 tile-type lighted 分类，
不把客户端光照渲染或 `TileDefinition` 固体属性误当作同一行为 owner。

`Main.npcStreamSpeed` 继续保持 `deferred-host`：旧值为 30，属于旧客户端/网络流式节流
状态。`NpcStreamSpeedPolicy` 现在在 Server 启动边界提供默认值和 `1..600` 有界归一化，
并由 `ServerLaunchOptions` 的可选 `--npc-stream-speed` 参数承接；但当前仍没有同语义的
NPC stream consumer，因此不能把它提升为 Simulation owner。

`Main.drunkWorld`、`tenthAnniversaryWorld`、`dontStarveWorld`、`notTheBeesWorld`、
`zenithWorld`、`vampireSeed` 与 `infectedSeed` 继续保持 deferred：当前只有
`WorldMetadata.SeedVariant` 和分散的生成策略参数，尚无逐字段的持久化/客户端投影契约；
不能把一个 seed variant 字符串直接伪映射为多组独立 bool owner。

`Main.getGoodWorld` 已形成一个 `completed_partial` 窄边界：WLD v227+ header index 1 经
`LegacyWorldMetadata`、`CompatibilityWorldMetadata`、`CompatibilityToDomeProjection`、
`WorldMetadata` 和 `WorldGenerationRequest.IsGoodWorld` 传播；`WorldPersistenceFormat` v8
以及 Dome 外层 v37 可选 tail 保存显式 true/false，v226 与 Dome V36 仍恢复为 unknown。
`LegacyWorldDataContext` 已携带该字段，但 V1456 WorldData 仍 deferred：当前 synthetic
`worldVariantFlags` 只表达 no-traps bit0 与 Skyblock bit6，不能把 legacy
`bitsByte12[7]` 伪装成 bit7。该窄片的 source contract、focused verifier 与直接项目构建证据
见 `Build/diagnostics/main-field-property/good-world-20260831-01/`、`...-20260831-02/` 和
`...-20260831-03/`；
完整 Good World 生成、NPC/掉落联动、客户端同步、协议 wire parity 与 Main parity 仍未完成。

`Main._gameModeDifficultyOverride` 现在有 `NpcDifficultyOverridePolicy` 的 typed nullable
解析边界：无 override 时保留基础 multiplier，有 override 时要求 finite 且为正值。该
policy 只冻结临时值语义；旧生命周期、实际 NPC damage consumer、清理时机和持久化仍为
deferred，不能据此提升 Main difficulty parity。

补充核对：`SecretSeedRuntimeProjection` 已把 `vampirism`、`world-is-infected`、
`team-based-spawns` 与 `dual-dungeons` 投影为运行时布尔值，但该 projection 仍不是
`Main` 旧字段的逐字段持久化和完整客户端同步 owner；因此这四项也不能仅凭运行时投影
提升为 accepted-narrow。`skyblockWorld`、`teamBasedSpawnsSeed` 与 `dualDungeonsSeed`
的旧 world-file 字段仍需独立的保存/加载与协议 parity 证据。

`Main.townNPCCanSpawn` 已形成候选宇宙窄片：`LegacyTownNpcSpawnCandidateRegistry` 精确
承接当前初始化逻辑可能写入的 37 个 NPC 类型，并明确独立于承载 `NPC.townNPC` 静态定义的
`LegacyNpcTownRegistry`。运行时解锁条件、占用计数、优先级选择、房屋与世界状态 eligibility
以及 spawn 生命周期仍保持 deferred；该 registry 不宣称完整候选数组行为。

`Main.noTrapsWorld` 已形成持久化/出站投影窄片：WLD v266+ header 的原始布尔值经
`LegacyWorldMetadata`、`CompatibilityWorldMetadata` 与
`CompatibilityToDomeProjection` 进入 `WorldMetadata.IsNoTrapsWorld`；
`WorldPersistenceFormat` v6 保持 nullable 旧格式兼容，而 V1456 WorldData 使用 flags15 bit0
投影该值。该 owner 不替代 `TrapGenerationGatePolicy` 的陷阱放置、secret-seed 组合、完整
WorldGen differential 或客户端全部状态同步；这些行为继续保持 deferred。

`Main.skyblockWorld` 已形成 metadata/request/protocol 窄片：WLD v302+ header 的第 9 个
seed-variant 布尔值经 `LegacyWorldMetadata`、`CompatibilityWorldMetadata` 与
`CompatibilityToDomeProjection` 进入 `WorldMetadata.IsSkyblockWorld`；
`WorldPersistenceFormat` v7 保存该 nullable 标志，`WorldGenerationRequest` 在显式参数缺省
时继承 metadata，V1456 WorldData 使用 flags15 bit6 投影。该 owner 只固定世界规则输入和
传输边界，不替代 `SkyblockRuleQuery`/`SkyblockPolicyQuery` 的完整生成副作用、随机顺序、
TileRunner/WLD differential 或客户端同步，这些行为继续保持 deferred。

inventory verifier 现对 `SecretSeedRuntimeProjection` 做结构级防回归检查：四个运行时
布尔成员和四个 variant 常量必须同时存在。该检查只证明 projection 的运行时输入边界，
不放宽字段持久化、协议同步或删除门条件。

同时，verifier 将当前 resolver 的 `accepted-narrow=118` 作为清单基线并在运行时断言；
若映射数量变化会直接失败，防止 stale inventory artifact 在未更新证据时被误认为通过。
写出 `inventory.json` 后还会立即反序列化并核对 source hash、成员总数、迁移范围和
accepted 数，确保磁盘 artifact 与本次内存清单一致。
verifier 也冻结并校验当前 Version4 `Main.cs` 的 SHA-256
`844A862B4EF863FF848227C1D682E89DAB799EEAD48070ADEA07565EC933254D`；基线源变化时
必须显式重审，不能静默把另一份源文件当作同一迁移基线。
清单排序后还会断言 `SymbolId` 唯一，避免重复声明记录污染删除门的 Roslyn manifest。
当前显式 deferred boundary 名单为 0 项；verifier 输出 `DEFERRED-GUARD-AUDIT: 0/0`。
该 guard 机制要求任何仍列入名单的符号实际存在且保持 `unmapped`，防止未完成 exact-set
审计的字段被静默提升；名单为空只表示本轮已审计的边界没有遗留项，不表示 695 项 Main
迁移范围已经全部完成。名单按 `StringComparer.Ordinal` 排序并冻结 SHA-256
`01BA4719C80B6FE911B091A7C05124B64EEECE964E09C058EF8F9805DACA546B`（空名单的换行
序列化）；后续新增或替换边界名称仍会触发失败。
resolver 还校验状态与证据字段一致：`accepted-narrow` 必须同时具备具体 owner、序列化
边界和验证入口；`unmapped` 必须明确标记为 `deferred/unverified`，禁止出现无证据的
乐观状态。
accepted 字段名集合也有独立 SHA-256 基线
`C1B2FEDE2ECA1858F9694814E7BCD41F639E71490C1C6C5490D33BDA0548E7CC`；本轮加入
`vanityPet` 后，若只替换某个 accepted 字段但保持总数不变，
verifier 仍会失败并要求重新审计。
每个 `accepted-narrow` 的 verification 标识还必须对应仓库中的 `Test` 项目目录，
避免清单引用不存在的验证入口。
`tileMerge` 已从显式 deferred guard 提升为 accepted-narrow；提升依据是 `TileID.Count=753`、
唯一 `SetupTileMerge()` 分配点、六组对称写入的 exact-set 以及 `LegacyTileMergeRegistry`
的 immutable verifier。其余尚未建立 owner/serialization/verification 契约的 Main 成员仍按
各自 resolver 状态保持 `deferred` 或 `unmapped`，不能因为本窄片提升而打开删除闸门。
`wallLargeFrames` 同样已从未映射状态提升为 accepted-narrow；提升依据是 `WallID.Count=367`、
22 个唯一字面量非零初始化写入、默认零值和 `LegacyLargeFrameWallRegistry` 的
FrozenDictionary exact-map verifier。该 registry 只表达静态 frame-size 默认投影，运行时墙体
frame 算法和客户端渲染仍保持边界外。
`anglerQuestItemNetIDs` 也已从未映射状态提升为 accepted-narrow；提升依据是 41 项有序
index→Item 类型 exact map、`ItemID.Count=6147` 值域边界、唯一 `AnglerQuestSwap` 读取点和
`LegacyAnglerQuestItemRegistry` 的 FrozenDictionary verifier。hardmode/crimson/world-surface
过滤、完成记录、存档和协议生命周期仍保持边界外。
`projFrames` 也已从未映射状态提升为 accepted-narrow；提升依据是 `ProjectileID.Count=1111`、
默认 frame-count `1`、275 个唯一字面量覆盖（含同值重复索引）和
`LegacyProjectileFrameRegistry` 的 FrozenDictionary exact-map verifier。该 owner 不承诺
投射物动画/行为/渲染和网络生命周期 parity。
`npcFrameCount` 同样已从未映射状态提升为 accepted-narrow；提升依据是 `NPCID.Count=697`、
完整 697 项初始化值、`NPC.cs` 的帧选择消费者和 `LegacyNpcFrameRegistry` 的
FrozenDictionary exact-map verifier。该 owner 不承诺 NPC 动画、纹理渲染、AI 或网络生命周期
parity。
`projPet` 也已从未映射状态提升为 accepted-narrow；提升依据是 `ProjectileID.Count=1111`、
121 个唯一静态 `true` 类型、无动态/冲突写入、六个 Legacy 消费者和
`LegacyPetProjectileRegistry` 的 FrozenSet exact-set verifier。该 registry 独立于
`ProjectileDefinition.IsMinion`，不承诺 minion/sentry 行为或投射物网络生命周期 parity。
`slimeRainNPC` 也已从未映射状态提升为 accepted-narrow；提升依据是 `NPCID.Count=697`、
唯一静态 `slimeRainNPC[1] = true` 初始化写入、`NPC.cs` 的两个类型分类消费者和
`LegacySlimeRainNpcRegistry` 的 FrozenSet exact-set verifier。该 registry 不替代
Slime Rain slot multiplier、spawn/death progression、loot 或 NPC 网络生命周期 parity。
verifier 同时冻结当前基线结构计数：696 个成员、659 个字段声明、37 个属性、659 个
字段变量、695 个迁移范围成员和 1 个身份排除项；任何计数漂移都要求显式重审。

`Main.dungeonX` 与 `Main.dungeonY` 已形成 `accepted-narrow` 的 `DungeonSpawnPoint` owner：
它保留 Demolitionist dungeon 房屋/NPC 生成坐标的 `-1/-1` unset 语义，并与
`DungeonBoundsSnapshot.X/Y` 明确分离，后者仍表示区域左上角边界。地牢生成、持久化和 NPC
spawn integration 继续保持 deferred，不能把坐标 owner 误报为完整 parity。

`Main.liquid` 继续保持 deferred：旧字段是 `Liquid.maxLiquid` 容量的全局活动 liquid 数组；
当前 ECS 使用 `LiquidUpdateQueueComponent` 与 typed command systems，队列容量、调度和回压
语义不同，不能仅以液体更新功能相同声称数组 parity。

`Main.maxMusic` 继续保持 deferred：旧值为 105，仅用于客户端 `musicFade` 数组容量；当前
Simulation/Server 没有音乐槽生命周期或持久化 owner，不属于权威 ECS 状态。

`Main.MaxShopIDs` 继续保持 deferred：旧值为 100，但当前 Simulation/Server 没有 shop catalog
生命周期或容量 owner；不能将协议中的 shop override 或物品身份字段误当作该容量的替代。

`Main.gamePaused` 继续保持 deferred：旧字段在客户端 `DoUpdate_WhilePaused` 流程中切换，
而已映射的 `GlobalTimerPaused`/`WorldClock.IsPaused` 是权威世界时间暂停；两者边界不同，
不能将客户端 UI 暂停直接传播为服务器世界时钟状态。

`Main.projHostile` 与 `Main.projHook` 已各自形成 `accepted-narrow` 的 immutable definition
owner：`LegacyProjectileHostileRegistry` 保留 167 个 source-backed hostile types，
`LegacyProjectileHookRegistry` 保留 20 个 `aiStyle == 7` hook types。两者都只覆盖静态
分类；完整 `Projectile.SetDefaults` 条件分支、combat/lifecycle、网络投影和未建模类型仍
保持 deferred，不能据此打开 legacy 删除门。

`Main.ServerSideCharacter` 继续保持 deferred：旧字段是全局客户端开关，当前协议模型只有
`SetUserSlotPacket.IsServerSideCharacter` 的用户级值；两者生命周期和信任边界不同，不能
直接映射为同一 ECS 状态。

`Main.cloudAlpha` 与 `Main.cloudBGActive` 继续保持 deferred：旧字段属于客户端背景/天气
视觉状态，当前 `WorldRuleState` 只承载权威降雨与风速，不承载这些渲染 alpha，不能把视觉
状态直接写入服务器 ECS。

`Main.mapTimeMax` 与 `Main.mapTime` 继续保持 deferred：旧值/倒计时用于客户端地图刷新，
当前 server 没有地图缓存或同语义同步生命周期；世界 tick 和 section replication 不能替代
地图计时器。

`Main.mapDelay` 现在由 Protocol compatibility owner `LegacyMapRefreshDelayPolicy` 保留
为默认值 `2`，并提供 `0..120` 的有界归一化。该 owner 只冻结客户端地图刷新延迟；地图
缓存生命周期和表现状态继续 excluded/deferred，不进入 Simulation ECS。

`Main.versionNumber` 与 `Main.versionNumber2` 继续保持 deferred：旧字段仅用于 CLI/server
标题显示；当前协议版本和存档格式版本由独立契约常量管理，显示字符串不能替代 wire 或
persistence version owner。`LegacyTerrariaVersion` 现在集中保留 `v1.4.5.6`、`1.4.5.6`
和 release `319` 供 compatibility/display 使用；这些值仍 excluded/deferred，不进入
Simulation ECS。

`Main.MaxWorldViewSizeWidth` 与 `Main.MaxWorldViewSizeHeight` 现在由 Protocol compatibility
owner `LegacyWorldViewLimits` 保留为 `1920x1200`，并提供 `1..maximum` clamp。该 owner 只
冻结旧客户端 viewport 上限；镜头、渲染和客户端视图生命周期继续 excluded/deferred，不进入
Simulation ECS。

`Main.tileNoSunLight` 已形成精确窄片：旧数组的 6 个类型（11、197、386、389、630、631）
与 `LegacyNoSunLightTileRegistry.RegisterDefaults()` 双向一致；WorldGeneration verifier 已
覆盖集合、边界类型和 `FrozenSet` 不可变投影。该 registry 只承载静态 no-sunlight 分类，
不宣称客户端光照行为已完成。

`Main.tileLargeFrames` 已形成精确窄片：旧表的 24 个 tile type frame-size 值（1 或 2）与
`LegacyLargeFrameTileRegistry.RegisterDefaults()` 双向一致；WorldGeneration verifier 已覆盖
完整键值、缺省值和 `FrozenDictionary` 不可变投影。该 registry 只表达 large-frame 静态表，
不替代局部 tile-frame 查询。

`Main.tileSpelunker` 已形成精确窄片：Legacy 源实际包含 49 个唯一 tile type（6、7、8、9、
12、21、28、37、63、64、65、66、67、68、83、84、105、107、108、111、166、167、168、
169、178、211、221、222、223、227、236、240、242、245、246、337、349、404、407、441、
467、468、506、531、566、639、702、751、752），与 `LegacySpelunkerTileRegistry` 双向一致；
WorldGeneration verifier 已覆盖集合、边界类型和 `FrozenSet` 不可变投影。该 registry 不替代
ore-finder priority 或客户端显示行为。

`Main.tileFlame` 已形成精确窄片：旧数组显式标记 13 个类型（4、33、34、35、42、49、
93、98、100、173、174、372、646），与 `LegacyFlameTileRegistry.RegisterDefaults()`
双向一致；WorldGeneration verifier 已覆盖集合、边界类型和 `FrozenSet` 不可变投影。
该 registry 只表达旧 tile-type flame 分类，不把语义不同的
`TorchDefinitionRegistry`（TorchId、DustType、IsBiomeTorch）误当作同一 owner，亦不宣称
客户端光照或 torch 行为 parity 已完成。

`Main.tileTable` 已形成精确窄片：旧 `tileTable` 显式集合有 76 项，与
`LegacyTileTableRegistry.RegisterDefaults()` 双向一致；WorldGeneration verifier 已覆盖
集合、边界类型和 `FrozenSet` 不可变投影。该 registry 独立于只有 12 项的
`RoomNeedsTileRegistry.RegisterTableDefaults()`，不把房屋需求子集当作完整静态表。
`Main.tileNoFail` 已形成精确窄片：旧表的 68 项显式集合与
`LegacyNoFailTileRegistry.RegisterDefaults()` 双向一致；WorldGeneration verifier 已覆盖
集合、边界类型和 `FrozenSet` 不可变投影。该 registry 只表达旧 no-fail 分类，不替代具体
tile 破坏流程或其他保护规则。

`Main.tileBlockLight` 已形成静态默认窄片：初始化阶段有 280 次字面量 `true` 写入、4 次
`false` 覆盖（`162`、`541`、`546`、`634`），并通过确定性 `727..732` 循环补充类型，
最终得到 283 个有效静态 `true` 类型；该集合与 `LegacyBlockLightTileRegistry.RegisterDefaults()`
双向一致。运行时昼夜逻辑仍会独立切换 `718`，不属于该静态 registry；因此本映射只承接
初始化默认分类，不宣称完整光照、渲染或昼夜行为 parity。
光照/渲染表现不能直接回流为服务器 Tile definition；需要先建立来源、版本和服务端实际调用方边界。

`Main.npcCatchable` 已形成精确窄片：Legacy 先将数组清零，再显式标记 85 个 NPC 类型，
并通过确定性循环补充 `442..448`，有效集合共 92 项；该集合与
`LegacyCatchableNpcRegistry.RegisterDefaults()` 双向一致。WorldGeneration verifier 已覆盖
完整集合、范围边界和 `FrozenSet` 不可变投影。该 registry 只承接旧 catchable 类型分类，
不把 `NpcDefinition.IsLikeTownNpc` 或协议层 catchable 判断误当作同一 owner；完整捕获交互
行为和客户端投影 parity 仍不在本窄片声明内。

#### 6.1.3 节日与特殊月相字段审计（2026-08-28）

旧 `Main` 的 `xMas`、`halloween`、`forceXMasForToday`、`forceHalloweenForToday`、
`forceXMasForever`、`forceHalloweenForever`、`pumpkinMoon` 和 `snowMoon` 不是单一的
静态世界规则：它们分别受日期判定、客户端强制设置和特殊事件生命周期影响。当前
`WorldRuleState` 只承载模式、PVP、天气和风，`WorldProgressionState` 虽有 Lantern Night
等已验证事件，但没有这些节日开关的来源、优先级、事件副作用、存档版本或客户端投影契约。

因此上述字段继续记录为 `unmapped/deferred`。不能仅因名称接近世界规则或月相就把它们
写入 `WorldRuleState`；后续必须先建立可重放的季节/节日输入、事件 system、持久化版本和
针对 NPC spawn/loot 影响的 differential verifier。

#### `Main.pvpBuff` 消息 55 服务端权威 ECS 窄片（2026-08-30）

本批完成了 `Main.pvpBuff` 的 source inventory：Legacy 在
`Main.cs:479` 以 `new bool[BuffID.Count]` 初始化，`BuffID.Count` 在
`Terraria.ID/BuffID.cs:933` 为 `389`，所以未显式标记的槽位默认是 `false`。初始化阶段
只有 16 次 `true` 写入（`Main.cs:6109-6124`），精确集合为
`20, 24, 30, 31, 36, 39, 44, 69, 70, 103, 119, 120, 137, 320, 323, 324`，没有
`false` 覆盖、动态索引、重复写入或其他 producer。

该数组不是玩家运行时 Buff 实例的 owner，而是旧消息 55 `AddPlayerBuffPvP` relay 的
`buff type` allowlist。`MessageBuffer.cs:2018-2027` 读取 `byte player`、`ushort buff`
和 `int duration`，仅在 `Main.netMode != 2 || Main.pvpBuff[num88]` 时 relay；
`NetMessage.cs:1035-1039` 按 `byte/ushort/int` 原样编码。当前 Protocol 以
`AddPlayerBuffPvpPacket`、`TerrariaMessageId.AddPlayerBuffPvp=55` 和严格 7-byte codec
承接 wire contract；active session/dispatcher 保留 packet target，session host 将原始
frame 放入带 `NetworkInboundEnvelope.PlayerSlot` provenance 的 isolation queue。

`DomeNetworkUpdateBridge` 把 envelope slot 写入 `AddPlayerBuffPvpCommand.SenderSlot`，
`DomeServer` 解析 sender/target active player 并拒绝未知、非 active 或 self-target 请求。
`LegacyPvpBuffRegistry` 以不可变 `FrozenSet<int>` 承接精确 16 项 allowlist；
`DomeSimulation.TryQueuePlayerPvpBuff` 通过既有
`ApplyTargetStatusEffectCommand` 确定性写入 `BuffCollectionComponent`，再由
`PlayerStatusEffectStateSnapshot` 暴露目标状态。

因此 `Main.pvpBuff` 当前状态为 **`completed_partial` / `accepted-narrow`**，不是完整
`Main` 或 Buff parity。客户端 message-55 relay/presentation、消息 50 `PlayerBuffs`
bootstrap、完整 Buff lifecycle/effects/immunity/stacking/duration、world-level
`WorldRuleState.IsPvpEnabled` gate、save/load projection、完整 Main parity、Legacy 删除
与 WorldGen 删除门仍为 deferred；`accepted-narrow` 当前为 `115`，Main 删除门不变。
本批 source contract、focused loopback verifier、Protocol/Simulation/Server Release build
和 inventory verifier 的 fresh 证据集中在
`Build/diagnostics/main-field-property/pvp-buff-20260830-02/`，其中
`pvp-buff-source-contract-20260830.md` 与 `verification-summary.json` 为入口。

2026-08-28 首次运行得到 `Main` 类型 696 个 Roslyn 符号（659 个字段声明、37 个属性声明），
当前可在 `Main.cs` 中定位的身份排除项为 `Main.worldID` 1 项，因此迁移作用域为 695 项。
对比报告中的 685 个字段 + 1 个属性是词法扫描口径，不能与 Roslyn 符号数直接相减。此次
reconciliation 使用同一 `Main.cs` 做名称集合比对：词法侧多出的 10 项是 4 个计数槽、2 个
常量和 4 个 event；Roslyn 侧额外识别 21 项（11 个属性和 10 个多行/复杂字段声明），
所以总数从 686 到 696 的净差为 10。两种口径已解释但仍不可互换。删除门必须使用带源
hash 的 Roslyn manifest，不得把 695 或词法基线 685 直接当作完成条件。`NPC.netID`、
`Projectile.identity`、
`MessageBuffer.whoAmI` 不属于该 `Main` 语法树，仍保留在身份排除规则中作为跨文件审计项。

#### 6.1.4 WorldGen `RockLayerCaves` 窄片（2026-08-30）

本轮在 Main/WorldGen 随机流仍为 deferred 的总边界内，补齐了 Version4
`GenPassNameID.RockLayerCaves` 的非 Remix base loop owner。`LegacyRockLayerCavesPassDefinition`
固定密度 `0.00013`、`[6,20)` strength、`[50,300)` steps、`Next(10)` 的 `-1/-2` 选择和
`(int)(width * height * density)` 截断计数；`LegacyRockLayerCavesPass` 按源码顺序
`type -> strength -> steps -> X -> Y` 从世界种子重置 pass-scoped random stream，读取
不可变 Clay-stage `WorldGridSnapshot`，用私有 projected tile map 生成带
`worldgen.cave.RockLayerCaves.rock-layer` provenance 的 `TileChangeCommand`，最后经
deterministic commit。profile-enabled 路径会跳过旧 generic fallback，避免同一 pass 重复执行，
并在 Clay commit 后调用专用 owner。

fresh oracle 证据位于
`Build/diagnostics/server-ecs-convergence/P9-worldgen/rock-layer-caves-boundary-20260830-05/`：
`Rock Layer Caves` stage trace、Clay 输入与 Rock 输出 snapshot、`655` 个默认
`4200x1200` invocation、seed-1456 random start/end checkpoint，以及
`rock-layer-caves-source-contract-20260830.json` 的当前 instrumented source hash。聚焦
verifier 的不可变 B4 artifact `...-06/rock-layer-caves-focused.log` 以 exit `0` 记录 `22445`
条 bounded commands；本次轻量 current-tree rerun 位于
`...-07/focused-verifier-rerun.log`，同样 exit `0`，当前 `800x300` fixture（仅 seed 一个
active tile）产出 `1` 条命令。该 rerun 只复核同一 contract，不替换历史 B4 计数。Simulation
与 verifier Release build 均为 exit `0`、零 warning/error；标准 bounded WorldGeneration
verifier 为 exit `0`、`280` 个 PASS、无 FAIL/ERROR。

这只是 `completed_partial` 的 pass-specific owner：Remix 的额外 `0.00013 * 0.4` paired
no-Y-change loop、`-2` 液体副作用、完整 TileRunner traversal、aggregate cave ordering、WLD
differential、legacy 删除和 `canRemoveLegacyWorldGen` 仍未闭合，44 条 `ServerRelevant` rows
也继续保持 deferred。该窄片不会改变 Main 的 `accepted-narrow` 计数或删除闸门。

#### 6.1.5 WorldGen `SurfaceCaves` owner exclusion（2026-08-30）

本轮在 Main/WorldGen 随机流仍为 deferred 的总边界内，修正了 profile-backed
`SurfaceCaves` 的重复 owner。Version4 当前 instrumented source 的契约范围为
`WorldGen.cs:12676-12785`，source guard 为
`!Skyblock.denyAllGeneration && !SecretSeed.noSurface.Enabled`，顺序为 vertical families、
horizontal `noYChange`、Caverer；source pass 本身没有 generic `surface-desert` recipe。
因此 `LegacyCavePassSystem` 只在存在 `LegacyTerrainRuntimeProfile` 时跳过 generic
`SurfaceCaves` schedule entry，专用 vertical/Caverer/Mountain owners 继续由
`WorldGenerationPipeline` 承担，无 profile 的兼容 fallback 保留。

`--surface-caves-only` focused verifier 的 RED/GREEN 证据位于
`Build/diagnostics/server-ecs-convergence/P9-worldgen/surface-caves-boundary-20260830-01/`
、`...-02/` 和 fresh rerun `...-03/`：seed `1456`、`800x300` fixture 下，profile path 为 `2,907` commands 且
generic `surface-desert` 为 `0`，dedicated vertical provenance 存在；no-profile path 保留
`4,342` generic commands。fresh `...-03/` 还记录 unified-owner `125,479` commands、
`412,918` random samples、Release build 零 warning/error，以及 bounded verifier
`280 PASS / 40 CHECK / 0 FAIL/ERROR`。该 slice 是 `completed_partial` owner correction，不宣称完整
SurfaceCaves/TileRunner/WLD parity、Remix/no-surface 规则、aggregate ordering、legacy
删除或 `canRemoveLegacyWorldGen`；44 条 `ServerRelevant` rows 仍 deferred，当前 Main inventory
`accepted-narrow=115`（该 WorldGen slice 本身不改变计数）。

#### `Main.projHook` 投射物钩爪定义窄片（2026-08-31）

Legacy `Main.projHook` is populated during `Projectile.SetDefaults`, where `aiStyle == 7`
marks hook projectiles. The source-backed registry records the 20 explicit Version4 types
(`13, 32, 72, 165, 229, 256, 315, 322, 331, 332, 372, 396, 403, 446, 489, 645, 652,
753, 865, 935`) across the bounded `ProjectileID.Count = 1111` domain. Invalid ids fail
closed, and the focused `Terraria.Dome.ProjectileHook.Verification` checks the exact set.

This is an `accepted-narrow` definition owner only. Hook behavior, ai-style movement,
grapple lifecycle, network projection, and any legacy types not represented by the current
source-backed table remain `partial/deferred`; this slice does not authorize legacy deletion.

#### `Main.projHostile` 投射物敌对定义窄片（2026-08-31）

Legacy `Main.projHostile` is populated from `Projectile.SetDefaults` cases that assign
`hostile = true`. `LegacyProjectileHostileRegistry` records the 167 explicit Version4
projectile types in the bounded `ProjectileID.Count = 1111` domain and exposes immutable,
fail-closed lookup. The focused `Terraria.Dome.ProjectileHostile.Verification` checks the
source-derived count and bounds.

This is an `accepted-narrow` definition owner only. Conditional seed-dependent behavior,
projectile combat/lifecycle rules, network projection, and unmodeled `SetDefaults` semantics
remain `partial/deferred`; no legacy deletion gate is opened.

#### `Main.buffNoSave` 状态效果持久化分类窄片（2026-08-31）

`LegacyBuffNoSaveRegistry` now has a focused acceptance boundary for the Version4
`buffNoSave` table: 99 unique buff types in the bounded `BuffID.Count = 389` domain,
including the explicit `173..181` range. The verifier checks representative membership,
default false behavior, and out-of-range fail-closed lookup.

This promotes only the immutable classification owner to `accepted-narrow`. Complete buff
reset, save/load, network persistence, and effect lifecycle parity remain `partial/deferred`.

#### `Main.buffNoTimeDisplay` 状态效果显示分类窄片（2026-08-31）

`LegacyBuffNoTimeDisplayRegistry` now has a focused acceptance gate for the Version4
`buffNoTimeDisplay` table: 118 unique buff types in the bounded `BuffID.Count = 389` domain,
including the explicit `284..304` range. The verifier checks representative membership,
default false behavior, and out-of-range fail-closed lookup.

Only the immutable classification owner is promoted to `accepted-narrow`; client rendering,
buff timers, network projection, and complete buff lifecycle parity remain `partial/deferred`.

#### `Main.offLimitBorderTiles` 世界边界策略窄片（2026-08-31）

`LegacyWorldBorderPolicy` now owns the Version4 immutable `offLimitBorderTiles = 40`
boundary offset and its inclusive lower/exclusive upper gameplay-bounds predicate. The
focused verifier checks edge coordinates and the constant contract.

This is an `accepted-narrow` world-generation policy only. Full placement scheduling,
tile mutation, and generated-world parity remain `partial/deferred`; the legacy deletion gate
is unchanged.

#### `Main.dungeonX` / `Main.dungeonY` 地牢生成锚点窄片（2026-08-31）

`DungeonSpawnPoint` separates the legacy Demolitionist spawn anchor from
`DungeonBoundsSnapshot`: both coordinates use the Version4 `-1/-1` unset sentinel, and a set
anchor requires non-negative `X` and `Y`. The focused verifier covers sentinel, valid, and
invalid states.

This promotes only the typed coordinate boundary. Dungeon generation, persistence, NPC spawn
integration, and full dungeon parity remain `partial/deferred`; the legacy deletion gate stays
closed.

#### `Main.invasionProgressWave` 侵袭进度波次瞬时投影窄片（2026-08-31）

Version4 `Main.cs:1289` declares `invasionProgressWave`; `ReportInvasionProgress` receives
`progressWave` and assigns it at `Main.cs:12254-12262`, while a newly started invasion resets the
display wave to `0` at `Main.cs:13082`. Regular invasion progress passes `0` from `NPC.cs:64796`,
and Pumpkin/Snow progress passes `waveNumber` from `NPC.cs:65088` and `NPC.cs:65218`.

`WorldInvasionProgressProjectionSystem.Resolve(WorldProgressionState, int progressWave)` now
preserves that source value in the immutable `WorldInvasionProgressResult.Wave`. The existing
no-wave overload delegates with `0`; invalid or unavailable progression returns an unavailable
result with `Wave=0` and does not mutate `WorldProgressionState`. The Main inventory resolver maps
`invasionProgressWave` to this result with `serialization=transient-projection` and the focused
`Terraria.Dome.WorldRules.Verification` owner.

This is an `accepted-narrow` projection only. The V1456 `InvasionProgressReportPacket.Wave` field
and codec already preserve the final wave byte (`TerrariaPacketCodec.cs:3620-3701`), so this slice
does not claim new wire publication, display lifetime, persistence, or loopback behavior. Wave
scheduling, NPC wave-counter ownership, complete regular/Pumpkin/Snow/Old invasion parity, full
Main parity, legacy deletion, and the 44 deferred `ServerRelevant` rows remain open. Fresh
evidence is under
`Build/diagnostics/main-field-property/invasion-progress-wave-20260831-01/`; the inventory gate is
`members=696`, `migratedScope=695`, `identityExcluded=1`, `accepted-narrow=126`.

#### `Main.GameUpdateCount` 服务器更新游标窄片（2026-08-31）

Version4 source declares the private `uint _gameUpdateCount` at `Main.cs:916`, exposes the
read-only property at `Main.cs:2931`, resets it to `65536u` in `ResetGameCounter()` at
`Main.cs:6587`, and increments it at `Main.cs:18048` after the active player update and before
NPC processing. The narrow server owner is the instance-local
`WorldGameUpdateCountProjection.Value`, exposed as read-only `DomeSimulation.GameUpdateCount`.
It advances once for each unpaused outer Simulation tick, independently of the WorldClock rate
(rate `0` and rate `3` both advance by one), and uses explicit `uint` wraparound. A paused
Simulation returns before the increment.

The cursor is transient and is intentionally excluded from `DomeSimulationSnapshot` and
`DomeStatePersistenceFormat`; a restored Simulation starts its local cursor at `0`. This slice
is `completed_partial`: focused WorldClock, adjacent WorldRules, and source-scanning inventory
verification pass under
`Build/diagnostics/main-field-property/game-update-count-20260831-01/`, with inventory
`members=696`, `migratedScope=695`, `identityExcluded=1`, and `accepted-narrow=127`. The
`ResetGameCounter` server/client lifecycle, `GlobalTimerPaused`/`gamePaused` client semantics,
visual time, full Main parity, and legacy deletion remain deferred.

## 7. 复盘与交付判定

- **目标：** 将 Main 的服务器责任拆成可验证 ECS 状态，而不是复制 God Object。
- **本次交付：** 一份覆盖 695 项 Roslyn 迁移作用域（另有 1 项身份声明排除）的责任族迁移契约，明确 owner、边界、缺口、
  实施阶段和验证门。
- **尚未交付：** Main 全量行为 parity、完整定义表、随机流等价和所有字段的 Roslyn
  逐项证据；状态必须保持 `partial/deferred`。
- **可复用规律：** 字段迁移的最小闭环是 `Definition -> Component -> System ->
  Command -> Snapshot -> Persistence/Protocol -> Verifier`；只写映射表或只建同名属性
  都不算迁移。
