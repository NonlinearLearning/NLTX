# Version4 `WorldSession` 独立只读审查与 ECS 设计报告

> taskNumber: 04  
> subsystemId: `WorldSession`  
> layer: `authoritative-simulation`  
> currentNltxStatus: `partial`  
> reportPath: `D:\TRbackup\NLTX\docs\research\2026-09-05-version4-world-session-public-decomposition.md`  
> reviewDate: `2026-09-05`  
> verificationStatus: `not-run`

## 1. 执行摘要

本报告的结论是：`WorldSession` 在 Version4 中确实存在，但它不是一个已经具有单一类边界的模块。其可确认的最小责任面是：

- 一世界的持久身份、尺寸、边界和生成元数据；
- 世界基础规则事实；
- 世界时钟与天气事实；
- 世界是否允许完整实体更新的 readiness 门控；
- 供存档与网络发送的世界状态快照输入。

Version4 的实际写入根仍集中在 `Main`、`WorldGen`、`WorldFile`、`WorldFileData`、事件类型和 `NPC` 静态进度字段中。`Main.DoUpdateInWorld` 的真实顺序尤其重要：玩家、刷怪压力和 NPC、投射物、世界物品、绳系实体先更新，随后才调用 `UpdateTime`，再调用 `WorldGen.UpdateWorld`、入侵更新和 `UpdateServer`。因此，不能为了得到“更直观”的 ECS 顺序而把时钟提交无条件前移；这会改变实体在昼夜边界看到的状态。

建议把 `WorldSession` 的写集收窄为基础时钟、基础天气、稳定世界规则和 readiness。昼夜边界事件、Blood Moon/Eclipse/Lantern Night 等事件实例由 `WorldCalendarAndEventOrchestration` 候选负责；Hardmode 的世界转换事务由 `WorldProgressionAndTransition` 候选负责；Creative/Journey 覆写由 `SimulationRuleOverrides` 候选负责。Hardmode 字段可以出现在世界规则快照中，但 `StartHardmode` 的长事务不能退化为对一个布尔值的普通写入。

当前 NLTX 根 `src/WorldSession` 只有状态记录，没有闭合的执行链、受控提交 Port、持久化/复制 Adapter 或 focused verifier；`dome/src` 有较丰富的时钟、天气、进度和服务器材料，但其时钟在 `DomeSimulation.Tick` 中早于玩家输入和实体阶段，不能据此宣称与 Version4 等价。故本报告将 NLTX 状态保持为 `partial`，不宣称迁移完成、行为等价或 API 兼容。

## 2. 审查范围和不负责的内容

### 2.1 本报告负责

本报告只审查固定清单中的 `WorldSession`，重点是：

- Version4 世界身份、规则、时钟、天气、ready/loading 门控的真实字段、方法和调用链；
- Version4 存档临时状态、世界头、恢复失败和网络世界数据边界；
- `WorldSession` 与相邻固定子系统的写入/读取方向；
- 当前 NLTX `src/WorldSession`、相关 `dome/src` 材料和既有验证材料的映射；
- 不落地的 Component/System/Query/Command/Adapter/Projection 设计提示；
- 行为保持风险、focused verifier 计划和最终整合交接事项。

### 2.2 不负责

以下内容只在确认边界所需的最小范围内出现，不在本报告中重新设计：

- Tile、Wall、Liquid、Chest、Sign、TileEntity、WorldSection 的存储与提交；
- Player、NPC、Projectile、WorldItem 的行为算法和实体槽位；
- 完整世界生成 Pass、Biome 传播和住房算法；
- 完整事件资格、事件实例和进度系统的最终 owner；
- 网络连接、账户、区段传输和客户端 UI 的完整设计；
- 单个天气效果、单个事件类型、单个保存字段和客户端粒子效果的独立拆分。

相邻子系统发现的问题只作为 `cross-subsystem finding`、`integration-risk` 或 `blocking-decision` 交给最终整合会话，不作为本报告对相邻子系统的最终裁决。

## 3. 证据优先级和来源角色

| 来源 | 实际路径 | 角色 | 使用边界 | evidenceStatus |
| --- | --- | --- | --- | --- |
| Version4 主代码 | `D:\TRbackup\Version4` | Terraria 真实字段、写者、调用者、生命周期和副作用的首要证据 | 事实判断以此为准 | `confirmed` / `partial` |
| 完整可编译参考 | `D:\TRbackup\无任何删减通过编译` | 只补 Version4 已存在文件的可证明删减实现 | 不把完整参考独有文件当成 Version4 事实 | `full-reference-supplemented` 作为 referenceStatus |
| tModLoader 离线文档 | `D:\TRbackup\tmodloader-api-docs-stable` | 公开生命周期、世界数据网络边界和扩展点的交叉验证 | 不替代 Version4 私有实现 | `confirmed` |
| Space Station 14 | `C:\Users\shan\Downloads\ECS\space-station-14-master` | 最小 ECS 粒度、round 生命周期、状态事件和身份分离参考 | 不复制代码、命名、领域语义或目录结构 | `partial` |
| 当前 NLTX | `D:\TRbackup\NLTX\src`、`Test`、`dome\src`、既有 docs | 当前实现和既有材料的事实映射 | 不因设计骨架存在而提升为已实现 | `partial` |

任务包给出的 `D:\TRbackup\Version4\Terraria\WorldFile.cs` 路径不存在；实际文件是 `D:\TRbackup\Version4\Terraria.IO\WorldFile.cs`。这一点在本报告中单独记为 `evidence-mismatch`，所有 WorldFile 事实均使用实际 `Terraria.IO` 路径。

## 4. Version4 真实代码证据表

| 事实 ID | 实际证据 | 调用者 / 读者 / 写者 | 生命周期与副作用 | evidenceStatus |
| --- | --- | --- | --- | --- |
| `V4-WS-01` | `D:\TRbackup\Version4\Terraria\Main.cs:99-104` 定义 `Main.WorldPreparationState`，只有 `AwaitingData`、`ProcessingData`、`Ready`；`:1035` 持有 `_worldPreparationState`。 | `DoUpdate` 调用 `UpdateWorldPreparationState`；`ShouldUpdateEntities` 读取；写者是 `UpdateWorldPreparationState` 及未在本段闭合的加载路径。 | 主循环每帧可见；只有 `Ready` 才允许完整世界实体更新。 | `confirmed` |
| `V4-WS-02` | `Main.cs:1763-1770` 的 `UpdateWorldPreparationState` 每次调用直接把 `_worldPreparationState` 设置为 `Ready` 并返回。 | `DoUpdate` 在 `Main.cs:11214-11216` 邻域调用；`ShouldUpdateEntities` 在 `:11404-11408` 读取。 | 这是 readiness 的当前实现事实，但没有在该方法中表达 loading/unloading/failed 转移。 | `confirmed` |
| `V4-WS-03` | `Main.cs:11193-11361` 的 `DoUpdate`：更新创意覆写和 readiness，执行 `Netplay.UpdateInMainThread`，处理自动保存、天气/云层，最后以 `ShouldUpdateEntities` 门控 `DoUpdateInWorld`。 | 主游戏循环；网络、保存、天气和实体系统是下游读者。 | 每个主更新；包含网络、文件保存、随机天气和客户端表现副作用。 | `confirmed` |
| `V4-WS-04` | `Main.cs:11400-11409` 的 `ShouldUpdateEntities` 仅在 `_worldPreparationState == Ready` 且 `!WorldGen.generatingWorld` 时返回真。 | `DoUpdate` 读取；实体世界 Tick 依赖该结果。 | 纯资格判断的外观；实际依赖全局生成状态。 | `confirmed` |
| `V4-WS-05` | `Main.cs:11411-11655` 的 `DoUpdateInWorld` 真实顺序是玩家 `:11422-11447`，帧计数 `:11448-11450`，复仇/刷怪/压力板 `:11451-11459`，NPC `:11460-11527`，Projectile `:11528-11552`，WorldItem `:11553-11571`，绳系实体 `:11572`，`UpdateTime` `:11575-11587`，`WorldGen.UpdateWorld`/入侵 `:11589-11604`，`UpdateServer` `:11605-11620`。 | 同一主世界 Tick 的直接下游。 | 顺序是行为契约；实体在 `UpdateTime` 提交前运行。 | `confirmed` |
| `V4-WS-06` | `Main.cs:594-624,652-664` 的 `dayTime`、`time`、`moonPhase`、`bloodMoon`、`raining`、`rainTime`、`maxRaining`、云量、风速和天气计数器均为 `Main` 静态状态。 | `UpdateWeather`、`UpdateTime` 写；NPC、事件、WorldGen、网络、存档和表现读取。 | 世界生命周期；部分字段是规则事实，部分是模拟缓存或视觉投影输入。 | `confirmed` |
| `V4-WS-07` | `Main.cs:12050-12252` 的 `UpdateWeather` 更新风速、风向目标、云量和 `weatherCounter`；`:12074-12171` 读取 Creative 冻结风、Lantern Night、玩家资格和随机数；`:12222-12250` 可发送包 7。 | `DoUpdate` 在 `:11344-11347` 每个 `dayRate` 迭代调用；下游是天气表现、网络和生态条件。 | 每个天气子迭代；随机、玩家读取、网络发送。 | `confirmed` |
| `V4-WS-08` | `Main.cs:12820-12924` 的 `StopRain`、`StartRain`、`ChangeRain` 写 `rainTime`、`raining`、`maxRaining`、`coinRain`；`StartSlimeRain`/`StopSlimeRain` 在 `:12925-12970` 写 Slime Rain 事实并发送包 7。 | `UpdateTime`、消息处理、事件系统和脚本命令可触发；天气、生态、网络读取。 | 事件持续期；随机、公告、网络副作用。 | `confirmed`，但 Slime Rain 的最终 owner 为 `integration-review` |
| `V4-WS-09` | `Main.cs:12972-13292` 的 `UpdateTime` 递减雨/Slime Rain，调用 `UpdateTimeRate`，在 `:13109-13119` 推进时间并调用 Cultist、BirthdayParty、LanternNight、Sandstorm、DD2、Credits、MysticLogs、PylonSystem；`:13191-13271` 执行夜间事件资格和刷怪后果。 | `DoUpdateInWorld` 调用；NPC、WorldGen、事件、网络和成就读取/受影响。 | 每个完整世界 Tick；随机、NPC 生成、网络公告、事件状态写入。 | `confirmed` |
| `V4-WS-10` | `Main.cs:13319-13468` 的 `UpdateTime_StartNight` 处理夜间边界、流星/血月/眼球/Hardmode Boss 待处理标记并在 `:13465-13467` 写 `time=0`、`dayTime=false`、发送包 7；`:13470-13564` 的 `UpdateTime_StartDay` 清理夜间状态、增加月相、可能写 Eclipse、入侵并多次发送包 7。 | `UpdateTime` 调用；事件和 NPC 系统读取。 | 昼夜边界；随机、网络、成就、广播和事件写入。 | `confirmed`；事件 owner `integration-review` |
| `V4-WS-11` | `D:\TRbackup\Version4\Terraria\WorldGen.cs:26084-26125` 的 `StartHardmode` 先在 `:26092` 写 `Main.hardMode=true`，随后后台转换，锁 `WorldFile.IOLock`，完成后通过 `Main.QueueMainThreadAction` 做网络区段重置和后续处理。 | 进度/事件触发；WorldGen、Tile、存档和网络受影响。 | 长事务；线程、锁、主线程回调、网络 sections 和掉落保护副作用。 | `confirmed`；owner `integration-review` |
| `V4-WS-12` | `WorldGen.cs:59400-59470` 的 `UpdateWorld` 先拒绝 `isGeneratingOrLoadingWorld`，读取 Hardmode 与 Creative biome spread 覆写，再更新 Wiring、TileEntity、Liquid 和生态。 | `DoUpdateInWorld` 的 `:11593`/`:11602` 调用；世界结构、液体和生态为读写者。 | 每个世界 Tick；受生成/加载和规则覆写门控。 | `confirmed` |
| `V4-WS-13` | `WorldGen.cs:6431-6501` 的世界清理重置事件、天气、Hardmode、生成标记、OreTiers、`WorldFile.ResetTemps` 和多个进度字段。 | 世界创建/加载/重启路径；所有世界状态消费者受影响。 | 清理边界；必须避免半清理状态继续被实体 Tick 读取。 | `confirmed` |
| `V4-WS-14` | `D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:658-810` 的 `LoadWorld` 检查云存档和自动生成，读取版本/世界头，清理临时 Tile，设置 `WorldGen.isGeneratingOrLoadingWorld`，执行 Liquid settle，再恢复标记；异常写 `LastThrownLoadException` 并置 `WorldGen.loadFailed`。 | 世界加载器、生成器、液体、世界元数据；`Main.ActiveWorldFileData` 是元数据写入点。 | 加载生命周期；文件、云存档、液体和错误副作用。 | `confirmed` |
| `V4-WS-15` | `WorldFile.cs:878-932` 的 `SaveWorld`/`_SaveWorld` 处理云存档、`IOLock`、TransformingWorld 等待和可跳过策略；`:933-991` 的 `InternalSaveWorld` 捕获临时状态、可重置到白天、序列化、回读验证和 rolling backup。 | 自动保存入口 `Main.cs:11307-11310`、手动保存和退出路径。 | 文件 I/O、锁、验证失败回滚和备份副作用；保存不能成为领域状态写者。 | `confirmed` |
| `V4-WS-16` | `WorldFile.cs:98-170,1035-1099` 的 `_tempTime`、`_tempRaining`、`_tempDayTime`、事件和天气临时字段由 `SetTempToOngoing` 捕获、`SetOngoingToTemps` 恢复；`:1186-1205` 固定序列化 header、world header、tiles、chests、signs、NPC、TileEntities、pressure plates、town manager、bestiary、creative powers、footer 顺序。 | 保存 Adapter 的 staging；运行时状态是来源，临时字段不是长期权威组件。 | 保存事务内；失败时应保留旧数据和旧运行时状态。 | `confirmed` |
| `V4-WS-17` | `WorldFile.cs:1263-1385` 的 `SaveWorldHeader` 序列化世界身份、尺寸、模式、时间临时值、邪恶类型、Boss/进度、Hardmode、入侵、Slime Rain、天气临时值、OreTiers、云/风和事件字段。 | `SaveWorld_Version2` 调用；加载对应为 `LoadHeader`。 | 持久化边界；二进制字段顺序和版本兼容是行为契约。 | `confirmed` |
| `V4-WS-18` | `WorldFile.cs:2008-2251` 的 `LoadHeader` 将世界头写回 `Main`、`WorldGen`、`NPC` 以及 WorldFile 临时字段；`:2125-2182` 读取时间、昼夜、月相、血月、Eclipse、Hardmode、雨临时值。 | `LoadWorld_Version2` 在 `:1808-1821` 调用；后续 `SetOngoingToTemps` 恢复运行时事实。 | 加载恢复；版本分支、默认值和旧文件兼容副作用。 | `confirmed` |
| `V4-WS-19` | `D:\TRbackup\Version4\Terraria.IO\WorldFileData.cs:15-118` 有 `LoadStatus`、`LoadException`、`UniqueId`、`WorldId`、`GameMode`、秘密种子、`IsHardMode` 等；`:291-300` 的 `SetAsActive` 只有无异常时才设置 `Main.ActiveWorldFileData`；`:325-349` 构造无效世界数据。 | 世界列表/加载器和 `Main.ActiveWorldFileData`；失败诊断读取。 | 世界元数据生命周期；异常边界和 active 切换副作用。 | `confirmed` |
| `V4-WS-20` | `D:\TRbackup\Version4\Terraria\NetMessage.cs:221-403` 的包 7 序列化时间、昼夜、血月/Eclipse、尺寸/出生点、WorldId/name/GameMode/GUID/generator version、风云、Hardmode、邪恶类型、事件、进度、OreTiers 和 Sandstorm 输入；`:277-281` 在无雨时把 `maxRaining` 写为 0。 | `Main` 昼夜/天气/事件边界和多种服务器操作调用 `SendData(7)`；客户端消费该世界数据。 | 网络 I/O；包 7 是跨域 Projection，不应直接成为核心状态写者。 | `confirmed` |
| `V4-WS-21` | `D:\TRbackup\Version4\Terraria\MessageBuffer.cs:375-389` 在连接消息后请求包 7；`:2114-2245` 对管理员/事件消息直接写血月、Eclipse、入侵和事件状态并转发包 7；`:385-386` 的 `case 7` 是空处理。 | 服务端入站命令和服务端网络广播；包 7 客户端解码路径在本次深读范围内未闭合。 | 入站权限验证、状态写入和广播副作用。 | `partial`，客户端解码 `unresolved` |

## 5. 完整参考源码补证表

| 补证 ID | Version4 位置 | 完整参考位置 | 补证结论 | evidenceStatus | referenceStatus |
| --- | --- | --- | --- | --- | --- |
| `REF-WS-01` | `D:\TRbackup\Version4\Terraria.IO\WorldFile.cs` 全文件 | `D:\TRbackup\无任何删减通过编译\Terraria.IO\WorldFile.cs` | 已做完整文件静态差异核对，结果 `WORLD_FILE_DIFF_COUNT=0`；WorldFile 的 Version4 存档/恢复事实可直接使用 Version4。 | `confirmed` | `confirmed` |
| `REF-WS-02` | `D:\TRbackup\Version4\Terraria.IO\WorldFileData.cs:211,271-280` | `D:\TRbackup\无任何删减通过编译\Terraria.IO\WorldFileData.cs:229-241,301-338` | Version4 的 `EnableSeedOptions`、`TryParseSeedOptionValue`、`TryParseSecretSeed` 是空体/默认返回；完整参考提供种子选项和秘密种子解析。只能把完整实现作为补证，不能声称 Version4 已有该算法。 | `partial` | `full-reference-supplemented` |
| `REF-WS-03` | Version4 `WorldFile` 的保存/加载头和临时 staging | 完整参考同路径对应实现 | 没有发现可证明改变 WorldFile 声明、签名或调用邻域的差异；本报告不把完整参考独有文件扩大为 Version4 证据。 | `confirmed` | `confirmed` |

## 6. tModLoader 公开 API 交叉验证表

文档首页 `D:\TRbackup\tmodloader-api-docs-stable\index.html:8,29` 的标题为 `tModLoader: Main Page`，页眉版本为 `tModLoader v2026.07`。类型页 `D:\TRbackup\tmodloader-api-docs-stable\class_mod_system.html:8,29` 的标题为 `tModLoader: ModSystem Class Reference`，版本同为 `v2026.07`。

| 公开成员 | 实际文档证据 | 仅用于交叉验证的边界 | evidenceStatus |
| --- | --- | --- | --- |
| `ModSystem.PostUpdateTime` | `class_mod_system.html:253-255`，锚点 `aef069f82d408785fbde30b0327579879` | 时间更新后的扩展点；文档明确客户端和服务器都会调用。支持把时间提交后的读取者放在后置阶段，但不证明 Version4 私有调用位置。 | `confirmed` |
| `ModSystem.PostUpdateWorld` | `class_mod_system.html:257-259`，锚点 `a293da73b3fc469aee8b830d1f2b84c88` | 世界更新扩展点；文档说明单机或服务器调用。支持服务器权威世界副作用与客户端投影分离。 | `confirmed` |
| `ModSystem.PostWorldLoad` | `class_mod_system.html:265-273`，锚点 `a420a22b570f31900387ca75e60c5256e` | 世界数据加载后、玩家进入前的加载边界；只在单机或服务器调用。支持 `WorldLoaded -> Ready -> 玩家进入` 的门控讨论。 | `confirmed` |
| `ModSystem.PreUpdateEntities` | `class_mod_system.html:1019-1044`，锚点 `a8f00139bd88fae8e8f55c2c2bcf1010c` | UI 更新后、Player/NPC/Projectile/Tile 世界更新前，且只在完整更新帧调用；客户端和服务器都会调用。仅作为阶段语义交叉验证，不能覆盖 Version4 的 `UpdateWeather`/`UpdateTime` 顺序。 | `confirmed` |
| `ModSystem.LoadWorldData(TagCompound)` | `class_mod_system.html:602-630`，锚点 `a12097aab73db65bd17b2bde505e1022d` | 自定义世界数据加载入口；支持持久化 Adapter 的“typed snapshot 先于领域提交”边界。 | `confirmed` |
| `ModSystem.NetReceive(BinaryReader)` | `class_mod_system.html:838-868`，锚点 `a144ef598fa0b3bcc2a0ac6bd1c5467aa` | WorldData 成功接收后的客户端扩展入口；支持“客户端只接收/投影，不成为权威写者”。 | `confirmed` |
| `ModSystem.NetSend(BinaryWriter)` | `class_mod_system.html:872-900`，锚点 `af9ebfea8b152b555b030265946cace70` | WorldData 成功发送时的服务器扩展入口，文档举例包括 Boss、日出和玩家加入；支持网络 Projection 的触发时机讨论。 | `confirmed` |
| `ModSystem.SaveWorldData(TagCompound)` | `class_mod_system.html:1077-1104`，锚点 `a926129e278ac9c460685bd2a326cd998` | 世界专属持久化扩展点；支持 save Adapter 与领域状态分离。 | `confirmed` |

这些页面只提供公开 API 的生命周期和复制边界，不能替代 Version4 `Main`、`WorldFile`、`NetMessage` 和 `MessageBuffer` 的私有行为证据。

## 7. Space Station 14 最小相关 ECS 参考表

Space Station 14 无直接对应证据；以下边界仅由 Version4 真实代码和 NLTX 项目约束决定。

| 实际读取文件 | 类型/方法 | 观察到的参考用途 | 不可推导的内容 |
| --- | --- | --- | --- |
| `C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\GameTicking\SharedGameTicker.cs:14-60` | `SharedGameTicker`、`RoundId`、`RoundStartTimeSpan`、`RoundDuration` | 一个共享系统提供少量 round 身份和时间查询；说明稳定状态查询可以小于完整服务器协调器。 | 不推导 Terraria 的世界时钟、天气或 Hardmode 语义。 |
| `...\Content.Server\GameTicking\GameTicker.cs:32-130` | `GameTicker.Initialize`、`PostInitialize`、`Update` | 服务器协调器的初始化、实体初始化后启动和每帧 round flow 分层；可参考生命周期入口与组合层的分工。 | 不复制 `GameTicker` 命名或 round 领域。 |
| `...\Content.Server\GameTicking\GameTicker.RoundFlow.cs:60-83,334-469` | `GameRunLevel`、`CanUpdateMap`、`ReadyPlayerCount`、`StartRound` | readiness/预加载/开始的资格查询与启动流程分离；地图加载、规则启动、玩家生成和进入运行态有明确顺序。 | 不把玩家 ready 语义替代世界加载 readiness。 |
| `...\Content.Server\GameTicking\GameTicker.RoundFlow.cs:640-743` | `RestartRound`、`ResettingCleanup` | 重启把本地清理事件、网络清理事件、实体 flush、地图选择清理和规则清理集中在生命周期边界。可参考清理提交和客户端通知分离。 | 不证明 Version4 的存档回滚或世界生成锁语义。 |
| `...\Content.Shared\Clock\GlobalTimeManagerComponent.cs:8-15` | `GlobalTimeManagerComponent`、`TimeOffset` | 一个极小的、可网络化/可暂停的时钟偏移组件，说明时间状态应保持内聚。 | 不对应 Terraria `dayTime`、`time`、月相或天气。 |
| `...\Content.Client\GameTicking\Managers\ClientGameTicker.cs:19-165` | `ClientGameTicker`、网络事件订阅和客户端 lobby 投影 | 客户端持有服务器发送的状态投影并触发 UI，而非直接运行服务器 round owner。 | 不证明 WorldSession 客户端实现。 |

该参考只用于 Component 粒度、生命周期事件、只读查询和客户端 Projection 的一般形状；不复制其代码、名称、目录结构或领域语义。

## 8. 成员、字段、方法、读写者和生命周期盘点

### 8.1 成员盘点

| 成员组 | Version4 事实 | 读者 | 写者 | 状态类型 | 候选归属 | 更新频率/副作用 | evidenceStatus |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 世界身份与几何 | `Main.worldName`、`leftWorld/rightWorld/topWorld/bottomWorld`、`maxTilesX/Y`、`worldSurface/rockLayer`、出生点/地牢点；`WorldFileData.WorldId/UniqueId/SeedText/WorldGeneratorVersion` | WorldFile、WorldGen、碰撞、区段、网络和世界选择 | 世界创建、`LoadHeader`、生成结果、元数据设置 | 权威、持久化 | `WorldDescriptorState` 候选；跨域 owner `integration-review` | 加载后稳定；文件和网络输出 | `confirmed` |
| 基础规则 | `Main.GameMode`、秘密种子、`WorldGen.crimson`、`Main.hardMode`、OreTiers、世界模式开关 | NPC、WorldGen、事件、存档和包 7 | 加载头、世界生成、Hardmode 转换、Creative 覆写读取 | 规则事实与跨域进度混合 | `WorldRulesState` 候选；Hardmode transition owner `integration-review` | 世界生命周期；Hardmode 有后台转换副作用 | `confirmed` / `partial` |
| 时钟 | `dayTime`、`time`、`moonPhase` | NPC、事件、旅行 NPC、刷怪、生态、网络和存档 | `UpdateTime`、昼夜边界方法、加载恢复 | 权威世界事实 | `WorldClockState` 候选 | 每完整世界 Tick，边界有事件/广播副作用 | `confirmed` |
| 天气基础事实 | `raining`、`rainTime`、`maxRaining`、`windSpeedTarget`、`windSpeedCurrent` | 天气、生态、NPC、网络和存档 | `UpdateWeather`、`StartRain`、`StopRain`、`ChangeRain`、`UpdateTime` | 规则事实、计时器和派生插值混合 | `WorldWeatherState` 候选 | 天气子 Tick/事件持续期；随机、玩家资格、网络 | `confirmed` / `partial` |
| 天气/云表现输入 | `cloudAlpha`、`numClouds`、`numCloudsTemp`、`cloudBGActive` | 渲染、网络包 7、天气算法 | `UpdateWeather`、加载初始化、网络发送前归零处理 | 派生/缓存/兼容输出 | Weather Projection 或缓存；不作为核心天气 owner | 高频变化；表现和网络副作用 | `partial` |
| 月/天气事件实例 | `bloodMoon`、`eclipse`、`pumpkinMoon`、`snowMoon`、Slime Rain、Lantern Night、Birthday Party、Sandstorm | NPC、事件、网络、存档和表现 | `UpdateTime`、事件方法、消息处理 | 事件运行态 | `WorldCalendarAndEventOrchestration` 或 `WorldProgressionAndTransition` 候选 | 边界和事件持续期；资格、随机、广播、刷怪 | `confirmed`，owner `integration-review` |
| 进度/解锁 | `NPC.downed*`、saved NPC、invasion、tower、DD2、spawn unlocks | NPC、刷怪、事件、WorldGen、存档、包 7 | NPC/事件/消息/WorldGen 多处 | 持久进度与事件事实 | 相邻进度子系统候选 | 事件或玩法触发；跨域写集 | `partial`，owner `integration-review` |
| readiness | `Main.WorldPreparationState`、`_worldPreparationState`、`WorldGen.generatingWorld`、`isGeneratingOrLoadingWorld` | `ShouldUpdateEntities`、Load/WorldGen、Server/网络 | 加载/生成流程和 `UpdateWorldPreparationState` | 生命周期门控、派生资格 | `SessionReadinessState` 候选 | 每帧判断；失败/加载/清理有安全边界 | `confirmed` / `unresolved` |
| 存档临时值 | `WorldFile._temp*` 和临时事件字段 | `SaveWorldHeader`、`SetOngoingToTemps` | `SetTempToOngoing`、`ResetTempsToDayTime`、`ResetTemps` | 兼容 staging，不是权威运行态 | `WorldSessionPersistenceAdapter` 候选 | 保存事务内；文件 I/O、锁、验证、备份 | `confirmed` |
| 网络世界数据 | 包 7 字段和事件转发包 78 | 客户端、服务器连接状态、ModSystem 扩展 | `NetMessage.SendData`；服务端消息处理 | Projection/Adapter | `WorldDataReplicationProjection` 候选 | 连接、昼夜边界、进度变化；网络 I/O | `confirmed` / `unresolved` |

### 8.2 最小权威写集

`WorldSession` 的最小权威写集候选为：

1. `WorldClockState`：`dayTime`、`time`、`moonPhase` 及其单调 tick/revision；
2. `WorldWeatherState`：雨状态、雨计时器、雨强目标、风目标和必要的规则模拟缓存；
3. `WorldRulesState`：稳定模式、秘密种子、世界邪恶类型和可持久化的规则快照；
4. `SessionReadinessState`：是否允许进入完整世界 Tick，以及加载/生成/卸载失败的边界状态；
5. `WorldDescriptorState`：一世界身份、尺寸、边界、地层和锚点。

以下内容不应成为 `WorldSession` 的普通直接写集：

- 昼夜边界事件实例和事件资格；
- Hardmode 的后台世界转换、Tile 转换、section reset 和主线程 follow-up；
- NPC Boss/解锁/入侵进度的最终写入；
- Creative/Journey 的时间、天气、难度和生态覆写；
- Tile/Liquid/Entity 数组和网络连接状态。

## 9. 权威状态所有权表

| 状态/事实 | Version4 当前写入根 | proposed 候选 owner | 允许的写方向 | 不能发生的写方向 | 决策状态 |
| --- | --- | --- | --- | --- | --- |
| 世界身份和描述 | `WorldFile.LoadHeader`、世界生成、`WorldFileData.SetAsActive` | `WorldSession` 的 `WorldDescriptorState`，status: `proposed` | loader/generator result -> session lifecycle -> descriptor commit | Network/client/Projection 反写 descriptor | `proposed` |
| `dayTime/time/moonPhase` | `Main.UpdateTime`、`UpdateTime_StartDay/Night`、load restore | `WorldSession` 的 `WorldClockState`，status: `proposed` | clock input -> clock system -> one commit -> boundary event | Calendar、NPC、网络直接写时钟 | `proposed`；共享 `WorldTime` owner `integration-review` |
| 雨与风事实 | `UpdateWeather`、rain methods、`UpdateTime` | `WorldSession` 的 `WorldWeatherState`，status: `proposed` | deterministic input/RNG/override snapshot -> weather system -> one writer | Rendering、client、Network Adapter 写运行态 | `proposed` |
| 稳定世界规则 | `WorldFileData`、`LoadHeader`、生成参数 | `WorldSession` 的 `WorldRulesState`，status: `proposed` | validated load/generation command -> rule commit | 事件 Projection 或客户端直接切难度 | `proposed` |
| Hardmode 事实 | `Main.hardMode`、WorldFile 头、WorldGen | `WorldRulesState.HardMode` 可作为快照字段；事务 owner 候选为 `WorldProgressionAndTransition` | transition command -> progression transaction -> rule snapshot/commit | `WorldSession` 直接把 bool 改完即返回 | `integration-review` |
| 昼夜事件 | `UpdateTime_StartDay/Night` 与事件类型 | `WorldCalendarAndEventOrchestration`，status: `proposed` | clock boundary -> eligibility query -> event command -> event state | Clock system 双写事件实例 | `integration-review` |
| Creative/Journey 覆写 | `Main.UpdateCreativeGameModeOverride`、雨/风/生态读取 Creative Power | `SimulationRuleOverrides`，status: `proposed` | override provider -> immutable effective snapshot -> consuming systems | 覆写值写回基础规则组件 | `integration-review` |
| readiness | `_worldPreparationState` 与 WorldGen flags | `SessionReadinessState` + lifecycle system，status: `proposed` | load/generate result -> lifecycle system -> readiness commit | `RuntimeComposition` 直接把 ready 设真；网络包改变 ready | `proposed`，实际转移来源 `unresolved` |
| Tile/Liquid/section | `Main.tile`、Liquid、WorldSections、WorldFile | `WorldStorage`，非本报告 owner | session readiness/transition -> storage command | WorldSession 直接写结构数组 | `integration-review` |
| 客户端世界状态 | 包 7 解码后的客户端状态 | `WorldDataReplicationProjection`，status: `proposed` | committed snapshot -> server serializer -> client projection | client projection -> authoritative session | `proposed`，包 7 decode `unresolved` |

## 10. 按访问模式分组

| 访问模式 | 共同读写者/变更原因 | 适合的边界 | 关键约束 |
| --- | --- | --- | --- |
| 稳定读取、多处查询 | 世界身份、尺寸、边界、生成版本、出生点 | `WorldDescriptorState`，status: `proposed`；`WorldDescriptorQuery`，status: `proposed` | 不携带 Tile 数组、实体槽位或网络 ID |
| 每 Tick 单调推进 | 时间、昼夜、月相 | `WorldClockState` + `WorldClockSystem`，status: `proposed` | 保持 Version4 `UpdateTime` 槽位，边界只发事件输入 |
| 高频规则模拟 | 雨、风目标/当前值、雨计时器 | `WorldWeatherState` + `WorldWeatherSystem`，status: `proposed` | 区分规则事实、随机状态、视觉 alpha；只能有一个写者 |
| 加载/生成/卸载边界 | readiness、失败、生成中标记 | `SessionReadinessState` + `WorldSessionLifecycleSystem`，status: `proposed` | 所有非 Ready 阶段阻断实体更新；失败不得半提交 |
| 事件/进度触发 | 日夜边界、入侵、血月、Eclipse、Hardmode | 相邻系统的 Command/Transition，status: `proposed` | 本报告只提供只读输入；禁止 WorldSession 与相邻系统双写 |
| 外部文件协议 | WorldFile 头、临时 staging、版本分支、备份 | `WorldSessionPersistenceAdapter`，status: `proposed` | Adapter 消费不可变快照，不能直接写核心组件或跳过提交屏障 |
| 外部网络协议 | 包 7、事件公告、客户端世界投影 | `WorldDataReplicationProjection` + `WorldSessionNetworkAdapter`，status: `proposed` | 入站只生成已验证 Command；出站只消费 committed snapshot |
| 表现和 UI | 云、alpha、粒子、音乐、镜头 | `WorldWeatherPresentationProjection`，status: `proposed` | 不进入存档权威写集，不反写天气/时钟 |

## 11. proposed ECS 拆分表

以下名称、路径和签名均为设计草案，不代表当前已存在实现；每项均为 `status: proposed`。路径遵守按领域组织、一个核心公开类型一个同名 PascalCase 文件的仓库约束。最终路径和共享类型由整合会话裁决。

| proposed 类型 | 候选路径 | 保存内容 | 不保存内容 | 设计状态 |
| --- | --- | --- | --- | --- |
| `WorldDescriptorState` | `src/WorldSession/WorldDescriptorState.cs` | 世界 ID/GUID、名称、种子文本、生成器版本、尺寸、边界、地层、出生/地牢锚点 | Tile、Section 位图、实体/网络 ID | `proposed` |
| `WorldRulesState` | `src/WorldSession/WorldRulesState.cs` | GameMode、秘密种子、WorldEvil、稳定规则标志；Hardmode 是否作为快照字段待整合 | 事件实例、难度覆写临时值、NPC 槽位 | `proposed`；Hardmode `integration-review` |
| `WorldClockState` | `src/WorldSession/WorldClockState.cs` | `DayTime`、`Time`、`MoonPhase`、clock revision/tick | 事件实例、客户端视觉时间、系统调度器引用 | `proposed` |
| `WorldWeatherState` | `src/WorldSession/WorldWeatherState.cs` | `IsRaining`、`RainTime`、`MaximumRainStrength`、风目标/当前值及经验证的 weather revision | cloud alpha、粒子、音乐、客户端状态；随机源本身不存入组件 | `proposed` |
| `SessionReadinessState` | `src/WorldSession/SessionReadinessState.cs` | `AwaitingData`/`ProcessingData`/`Ready` 与失败/卸载投影所需状态、generation/load barrier | 文件异常对象、线程句柄、网络连接 | `proposed` |
| `WorldTickSnapshot` | `src/WorldSession/Snapshots/WorldTickSnapshot.cs` | 某个提交点的只读 clock、weather、rules、descriptor、readiness 和 revision | 可变组件引用、实体槽位、Transport 对象 | `proposed`；`crossSubsystemOwner: integration-review` |

建议不要继续使用当前 `WorldSessionComponents.cs` 作为长期巨型组件文件；实施时应把核心公开类型拆到同名文件，并先处理 `WorldSession` 与 `WorldGeneration` 下同名类型/命名空间的闭合关系。这个建议本身不表示现在执行文件迁移。

## 12. System、Query、Command、Adapter、Projection 边界

### 12.1 System 契约

| proposed System | 输入 | 输出/写集 | Seam | Depth / Leverage / Locality | 设计状态 |
| --- | --- | --- | --- | --- | --- |
| `WorldSessionLifecycleSystem` | `WorldLoadCompleted`、`WorldGenerationCompleted`、`WorldLoadFailed`、`WorldUnloadRequested`，均为 status: `proposed` | 唯一写 `SessionReadinessState`；发布 `WorldSessionReady`/`WorldSessionUnavailable`，均 status: `proposed` | `IWorldLoadPort`、`IWorldResetPort`，status: `proposed` | 深：失败、回滚和 readiness barrier；高：可用 fake loader/reset；局部：只拥有会话阶段 | `proposed` |
| `WorldClockSystem` | 当前 clock、tick delta、暂停、`WorldTimeRateSnapshot`、status: `proposed` | 唯一写 `WorldClockState`；输出 `Dawn`/`Dusk` transition，status: `proposed` | `ITickInput`、`IRandomSource` 不直接持有系统时间，均 status: `proposed` | 深：边界和单调性；高：固定输入即可测试；局部：只拥有基础时钟 | `proposed` |
| `WorldWeatherSystem` | clock snapshot、weather state、雨/风 Command、override snapshot、RNG seam，均 status: `proposed` | 唯一写 `WorldWeatherState`；输出 weather transition，status: `proposed` | `IRandomSource`、`IWeatherEffectCommitPort`，status: `proposed` | 深：雨、风、Lantern Night 门控和随机；高：可替换 RNG；局部：基础天气，不拥有事件 | `proposed` |
| `WorldSessionCommitSystem` | clock/weather/rules/lifecycle delta 和受控提交 Command，status: `proposed` | 原子提交本轮 `WorldTickSnapshot`；清空已消费命令；发出 effect intents | `IWorldStateCommitPort`，status: `proposed` | 深：单写屏障和顺序；高：阻止双写；局部：只提交会话状态和输出意图 | `proposed` |
| `WorldCalendarAndEventOrchestration` | clock boundary、weather/rules/progression read views，status: `proposed` | 事件资格与事件实例 transition，status: `proposed` | `IWorldEventCommandSink`，status: `proposed` | 只在本报告中作为跨域消费者；最终 owner `integration-review` | `integration-review` |
| `WorldProgressionAndTransition` | Hardmode/progression command、规则快照、WorldStorage transition port，status: `proposed` | Hardmode 事务、进度快照、完成/失败 transition，status: `proposed` | `IWorldTransformationPort`，status: `proposed` | 后台转换、锁和主线程 follow-up；最终 owner `integration-review` | `integration-review` |

Version4 的 `UpdateWeather` 在实体更新前运行，而雨计时器和时钟推进在 `UpdateTime` 槽位运行。若保持行为，`WorldWeatherSystem` 可以由同一 owner 提供两个显式调度阶段：`PreWorldFrame` 只更新风/云模拟输入，`AtTimeSlot` 更新雨计时器和天气边界；这不是两个写者，而是同一个系统 owner 的两个调度入口。最终是否采用此两阶段接口是 `blocking-decision`。

### 12.2 Query 契约

| proposed Query | 输入 | 输出 | 纯度和用途 | 设计状态 |
| --- | --- | --- | --- | --- |
| `SessionReadyQuery` | `SessionReadinessState`、generation barrier，status: `proposed` | `CanUpdateEntities`、失败原因值，status: `proposed` | 纯资格判断；不得改变 readiness | `proposed` |
| `WorldTimeQuery` | `WorldClockState`，status: `proposed` | `WorldTimeView`、昼夜边界和时间段资格，status: `proposed` | 纯计算；不推进时钟 | `proposed`；`WorldTimeView` `crossSubsystemOwner: integration-review` |
| `WeatherQualificationQuery` | `WorldWeatherState`、clock/rules/override view，status: `proposed` | 是否下雨、风力资格、是否允许天气启动，status: `proposed` | 纯资格；不调用 RNG、不发送网络 | `proposed` |
| `WorldDescriptorQuery` | `WorldDescriptorState`，status: `proposed` | 边界、区段尺寸、出生点和持久身份视图，status: `proposed` | 纯视图；不加载文件 | `proposed` |

### 12.3 Command 与 Port 方向

| proposed Command/Port | 方向 | 约束 | 设计状态 |
| --- | --- | --- | --- |
| `WorldLoadCommand` / `WorldUnloadCommand` | Composition/Storage -> `WorldSessionLifecycleSystem` | 只提交请求和外部结果，不把 FileStream/Task 放入组件 | `proposed` |
| `WorldRainStartCommand` / `WorldRainStopCommand` / `WorldWindChangeCommand` | 玩家/事件/网络 Adapter -> validated command queue -> `WorldWeatherSystem` | 按 sequence 验证、串行消费、重复命令幂等；不允许 Adapter 直接改 `WorldWeatherState` | `proposed` |
| `WorldTimeRateCommand` | `SimulationRuleOverrides` -> time-rate resolver -> clock system | 覆写是本轮有效输入，不覆盖基础时钟事实 | `integration-review` |
| `HardmodeTransitionCommand` | 进度/事件 -> `WorldProgressionAndTransition` | 事务化、可观察完成/失败、必须在转换结束前阻止冲突保存/结构写入 | `integration-review` |
| `IWorldStateCommitPort` | Domain systems -> `WorldSessionCommitSystem` | 只有一个会话提交屏障；输出 transition/effect intent，不把 I/O 直接带进核心 | `proposed`；`crossSubsystemOwner: integration-review` |
| `IWorldTransformationPort` | Progression -> WorldStorage/WorldGen adapter | 包含锁、后台工作、主线程 follow-up 和 section reset 的显式语义 | `integration-review` |

### 12.4 Adapter 与 Projection 契约

| proposed Adapter/Projection | 输入 | 输出/副作用 | 失败/重试边界 | 设计状态 |
| --- | --- | --- | --- | --- |
| `WorldSessionPersistenceAdapter` | `WorldTickSnapshot`、WorldStorage snapshot、save options，status: `proposed` | 版本化 WorldFile header/typed payload；临时 staging、验证和 rolling backup | 读取失败返回 typed failure；写失败保留旧文件；不得半提交运行时 | `proposed` |
| `WorldSessionLoadAdapter` | 文件/云数据，status: `proposed` | 已验证的 descriptor/rules/clock/weather/readiness load result | 版本过新、截断、云不可用和校验失败均回到 failed，不设置 Ready | `proposed` |
| `WorldDataReplicationProjection` | committed `WorldTickSnapshot`、跨域 event/progression read views，status: `proposed` | 包 7/事件公告的 wire DTO | 出站失败由网络层处理；Projection 不重试领域写入 | `proposed`；`crossSubsystemOwner: integration-review` |
| `WorldSessionNetworkAdapter` | 入站 wire data，status: `proposed` | 已验证的 `World*Command` 或拒绝结果 | 权限、序列、客户端身份和消息版本失败时丢弃/记录；不直接写组件 | `proposed` |
| `WorldWeatherPresentationProjection` | weather snapshot，status: `proposed` | cloud alpha、粒子、音乐和 UI 输入 | 客户端丢帧不影响服务器事实；不持久化 | `proposed` |

### 12.5 跨域候选类型

下列类型会被两个或以上子系统读取或提交，只能保留候选 owner；均明确标记 `crossSubsystemOwner: integration-review`，不在本报告中宣布最终归属。

| proposed 类型 | candidate owner | consumers | writers | unresolved ownership | 对整合的影响 |
| --- | --- | --- | --- | --- | --- |
| `WorldDescriptorView` | `WorldSession` | WorldStorage、WorldGenerationAndEcology、Network、Persistence、spawn/空间查询 | Lifecycle/descriptor commit | `crossSubsystemOwner: integration-review` | 需决定值对象是否与持久化 descriptor 共用，避免把 Tile/Section 放入描述组件 |
| `WorldRulesView` | `WorldSession` | Calendar、Progression、WorldGen、SimulationRuleOverrides、Network、Persistence | validated rule commit、Hardmode transition | `crossSubsystemOwner: integration-review` | 需决定 Hardmode 是否只读快照字段，以及 override 是否作为独立 effective view |
| `WorldTimeView` | `WorldSession` | Player、NPC、Projectile、Calendar、Progression、WorldGen、Network、Persistence | `WorldClockSystem` | `crossSubsystemOwner: integration-review` | 需锁定时钟 revision 和 `UpdateTime` 槽位 |
| `WorldWeatherView` | `WorldSession` | NPC、WorldGen、Calendar、Network、Persistence、表现 | `WorldWeatherSystem` | `crossSubsystemOwner: integration-review` | 需区分雨/风规则事实与 cloud/粒子表现缓存 |
| `SessionReadinessView` | `WorldSession` | RuntimeComposition、实体门控、WorldStorage、Network、Persistence | Lifecycle system | `crossSubsystemOwner: integration-review` | 需统一 `Ready` 与生成/加载 barrier 的判定，防止组合器直接写状态 |
| `WorldTickSnapshot` | `WorldSession` | 所有需要同一提交点的下游、Network、Persistence | Session commit boundary | `crossSubsystemOwner: integration-review` | 需决定只含会话状态，还是聚合事件/进度只读视图 |
| `WorldStateCommitBatch` | Session commit boundary | WorldStorage、Persistence、Network、Calendar、Progression | owning systems via Port | `crossSubsystemOwner: integration-review` | 需锁定提交的原子性、失败和重试语义 |

## 13. 调用方向和真实 System 顺序

### 13.1 Version4 事实顺序

```text
Main.Update
  -> Main.DoUpdate
    -> UpdateCreativeGameModeOverride
    -> UpdateWorldPreparationState
    -> Netplay.UpdateInMainThread
    -> DoUpdate_AutoSave
    -> UpdateWeather (before entities; once per dayRate iteration)
    -> ShouldUpdateEntities
      -> Main.DoUpdateInWorld
        -> Player.Update
        -> CurrentFrameFlags / game update count
        -> NPC.RevengeManager.Update / NPC.SpawnNPC / PressurePlateHelper.Update
        -> NPC.UpdateNPC
        -> Projectile.Update
        -> WorldItem.UpdateItem
        -> LeashedEntity.UpdateEntities
        -> UpdateTime
          -> rain/slime timers and time advance
          -> calendar/event updates and day/night boundary methods
        -> WorldGen.UpdateWorld / UpdateInvasion
        -> UpdateServer
        -> chest / ambient wind / camera / SceneState
```

### 13.2 proposed 调用方向

```text
WorldLoadAdapter / WorldGeneration result
  -> WorldSessionLifecycleSystem
    -> SessionReadinessState commit
      -> SessionReadyQuery
        -> host scheduler allows full world tick

Pre-world phase
  -> WorldWeatherSystem.PreWorldFrame
  -> weather presentation/network inputs

DoUpdateInWorld compatibility slot
  -> Player/NPC/Projectile/WorldItem/LeashedEntity systems
  -> WorldClockSystem
  -> WorldWeatherSystem.AtTimeSlot
  -> WorldCalendarAndEventOrchestration (boundary/event commands)
  -> WorldProgressionAndTransition (when a progression command is due)
  -> WorldGenerationAndEcology / WorldStorage systems
  -> WorldSessionCommitSystem
  -> server/network projections
```

这里的 `proposed` 顺序必须由最终整合会话与相邻报告共同锁定。最小行为保持约束是：

1. 在 Version4 中，`UpdateWeather` 对风/云的影响发生在玩家、NPC、Projectile 和 WorldItem 之前；
2. 在 Version4 中，`UpdateTime` 发生在这些实体之后、`WorldGen.UpdateWorld` 之前；
3. `WorldGen.UpdateWorld` 必须看到已提交的天气/时钟/规则事实，但不得在生成/加载期间运行；
4. `UpdateServer` 和包 7 Projection 只能读取本轮已提交的跨域快照；
5. 文件保存不能从临时 staging 反向覆盖运行时权威状态，除非经过显式恢复 Command。

## 14. 持久化、网络和客户端投影边界

### 14.1 持久化

Version4 的 `WorldFile` 不是一个只保存时钟的简单文件。`SaveWorld_Version2` 有固定 payload 顺序，World header 同时包含世界身份、规则、时间、天气、事件、进度和生成相关值。保存流程还会：

- 用 `SetTempToOngoing` 从运行时捕获值；
- 在 `resetTime` 时用 `ResetTempsToDayTime` 改写保存快照，而不是直接改变运行时时钟；
- 在 `IOLock` 内写入；
- 回读并验证；
- 验证失败时把旧数据保留为备份；
- 通过 `LastThrownLoadException`、`LoadStatus`、`loadFailed` 表达失败。

因此 proposed Adapter 必须把“运行时快照”“保存快照”“恢复命令”分开。`WorldFileData` 只能成为边界 DTO/错误结果的来源，不能继续作为各个 System 共同可变写入口。

### 14.2 网络

包 7 的 Version4 wire layout 同时混合了：

- 时钟和天气：`time`、昼夜、血月、Eclipse、月相、风、云、`maxRaining`；
- 世界描述：尺寸、出生点、WorldId、名称、GameMode、GUID、生成版本；
- 规则和进度：Hardmode、邪恶类型、Boss、入侵、Slime Rain、事件、OreTiers、秘密种子和解锁；
- 表现/生态输入：背景、树、云、Sandstorm、DD2 等。

这证明包 7 应是跨子系统 Projection，而不是 `WorldSession` 组件的序列化镜像。出站方向应为：

```text
committed domain snapshots
  -> field-owner-specific projection assembly
  -> WorldDataReplicationProjection
  -> NetMessage-compatible serializer
  -> client projection
```

入站方向应为：

```text
wire message
  -> WorldSessionNetworkAdapter
  -> identity/permission/sequence validation
  -> WorldEvent/Weather/Progression Command
  -> owning System
```

本次在 `MessageBuffer.cs:385-386` 看到包 7 的空 `case 7`，未在当前最小路径内闭合客户端实际解码位置。因此不得把“服务器包 7 已编码”升级为“客户端网络恢复已确认”。

### 14.3 单机、服务器、客户端

| 运行角色 | 可以写入的事实 | 只能读取/投影的事实 | 说明 |
| --- | --- | --- | --- |
| 单机/服务器权威模拟 | WorldSession 基础状态；经 Command 触发事件/进度；通过 adapter 提交存档/网络 | 无 | `Main`/WorldGen 的权威写集在 Version4 主要位于此侧 |
| 服务器网络层 | 解析并排队已验证 Command；发送已提交快照 | 不应直接写核心组件 | MessageBuffer 当前存在直接改事件字段的兼容路径，迁移时必须收口 |
| 客户端 | 本地投影、表现缓存、UI、客户端解码状态 | 权威时钟、天气、Hardmode、世界进度 | tModLoader `NetReceive` 文档边界支持这一方向；Version4 客户端 decode 仍有 evidence-gap |
| Persistence Adapter | 读取文件生成 load result；消费快照写文件 | 不应直接成为运行时写者 | `WorldFile` 临时 staging 是兼容实现，不应泄漏为领域 API |

## 15. 当前 NLTX 映射

### 15.1 根 `src/WorldSession`

实际读取到 `D:\TRbackup\NLTX\src\WorldSession\WorldSessionComponents.cs:1-89`，其中包含 readiness 枚举、世界描述、规则、时钟/天气、事件进度和刷怪压力记录。但该文件目前只有状态记录：

- 没有实际 `WorldSessionSystem`；
- 没有 `WorldTickSnapshot`；
- 没有受控 `WorldStateCommit` Port；
- 没有持久化 Adapter；
- 没有复制 Adapter/Projection；
- 没有本子系统 focused verifier；
- 没有生产调用方把这些记录接入 Version4 兼容 Tick。

同一项目中还存在：

- `src/WorldSession/WorldGeneration/WorldRulesState.cs:1-18`；
- `src/WorldSession/WorldGeneration/WorldPreparationState.cs:1-11`；
- `src/WorldSession/WorldGeneration/WorldDescriptorState.cs:1-25`；
- `src/WorldSession/WorldGeneration/WorldBounds.cs:1-7`。

这些类型与 `WorldSessionComponents.cs` 使用不同命名空间，并与其存在同名概念。由于本轮未运行编译，不能把该命名冲突或项目引用关系升级为已验证编译失败；但它已经是明确的 `integration-risk`。当前根 `src/WorldSession` 状态只能记为 `partial`，不能因为存在 `WorldTimeWeatherState` 或 `WorldEventProgressState` 就标为 `confirmed`。

### 15.2 `dome/src` 现有材料

以下是实际存在并读取的模型/执行材料，不是根 `src/WorldSession` 的已闭合实现：

| 文件与行 | 实际内容 | 对本报告的映射 | nltxStatus |
| --- | --- | --- | --- |
| `dome/src/Terraria.Dome.Simulation/World/WorldClock.cs:5-180` | `WorldClock`、`Advance`、`Restore`、`WorldClockSnapshot`，含 tick、时间、昼夜、暂停、月相和长度校验 | 可作为时钟状态/不变量材料 | `partial` |
| `dome/src/Terraria.Dome.Simulation/World/Systems/WorldClockSystem.cs:6-37` | `Tick` 后生成 `Dawn`/`Dusk` transition | 可作为边界 transition 材料 | `partial` |
| `dome/src/Terraria.Dome.Simulation/World/Systems/WorldWeatherSystem.cs:6-149` | 基于 clock/rules/Command 列表计算雨与风，并输出 environment transition | 可作为天气纯计算 seam 材料 | `partial` |
| `dome/src/Terraria.Dome.Simulation/World/WorldRuntimeSnapshot.cs:5-59` | clock/rules/generation completion 的不变快照 | 可作为 readiness 与快照不变量材料 | `partial` |
| `dome/src/Terraria.Dome.Simulation/World/WorldRuleState.cs:5-237` | game mode、难度、Crimson、雨、风、PVP 的不可变规则记录 | 可作为规则/天气边界材料；与根项目仍未闭合 | `partial` |
| `dome/src/Terraria.Dome.Simulation/World/WorldProgressionState.cs:7-186,188-805` | Hardmode、Boss、Blood Moon、Eclipse、入侵、Slime Rain、Lantern Night 等进度记录 | 证明跨域状态丰富；最终 owner 仍须整合 | `partial` |
| `dome/src/Terraria.Dome.Simulation/World/Systems/WorldProgressionSystem.cs:7-305` | 事件、入侵、Slime Rain 的命令消费和状态变换 | 可作为相邻进度系统材料，非本报告 owner | `integration-review` |
| `dome/src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs:119-143,5002-5139` | 将 clock、weather、progression 系统放入统一 `Tick`；当前先 clock/weather/progression，再 ApplyPlayerInputs | 有执行链，但顺序与 Version4 `UpdateTime` 在实体之后的事实不同 | `partial`，`integration-risk` |
| `dome/src/Terraria.Dome.Server/Startup/ServerHostState.cs:6-120` | `IsReady`、`IsAcceptingConnections`、会话计数、`MarkReady`、`MarkStopped`、恢复失败 | 可作为服务器 host readiness 投影材料 | `partial` |
| `dome/src/Terraria.Dome.Server/DomeServer.cs:196-217,300-337,772-782` | 暴露 `IsReady`，生成 WorldData context、世界规则快照，`Start` 中调用 `MarkReady` | 服务器组合器/投影材料；不能替代 WorldSession owner | `partial` |
| `dome/src/Terraria.Dome.Server/DomeServer.cs:1034-1077,2121-2149` | Simulation loop 顺序、环境变化复制、包化世界时间复制 | 可作为复制 Adapter 材料；当前世界规则复制只发送特定时间点 | `partial` |
| `dome/src/Terraria.Dome.Server/Persistence/DomeStatePersistenceFormat.cs:20-55,73-165,1519-1634` | 版本化世界时钟、规则、进度、天气、元数据和事件随机状态读写 | 可作为 typed persistence 材料；不是 Version4 WLD 等价证明 | `partial` |

特别注意：`DomeSimulation.Tick` 的实际顺序是当前实现材料中的行为，不是 Version4 事实。它不能绕过 `V4-WS-05` 的兼容顺序要求。

## 16. focused verifier 设计和建议命令

本轮不创建测试或项目文件。以下路径、类型和命令仅为未来 verifier 设计，均为 `status: proposed`。

### 16.1 建议的 focused verifier 集

| verifier ID | proposed 验证目标 | 关键断言 | 设计状态 |
| --- | --- | --- | --- |
| `WSV-READINESS` | `SessionReadinessState` 生命周期 | Awaiting/Processing/Failed/Unloading 不允许实体 Tick；Ready 且 generation barrier 清除才允许；重复完成/失败命令幂等或显式拒绝 | `proposed` |
| `WSV-TICK-ORDER` | Version4 Tick 顺序 | 用 trace recorder 证明 `UpdateWeather -> Player -> NPC -> Projectile -> WorldItem -> UpdateTime -> WorldGen -> UpdateServer`；昼夜边界只在原时序可见 | `proposed` |
| `WSV-CLOCK` | 时钟边界与暂停 | 时间增量、昼夜切换、月相、暂停、`dayRate=0`、fast-forward 和 tick revision 无越界/重复提交 | `proposed` |
| `WSV-WEATHER` | 雨/风纯计算与覆写 | 雨启动/停止、endless rain 阈值、Freeze Rain/Wind、Lantern Night、确定性 RNG 和天气 sequence | `proposed` |
| `WSV-PERSISTENCE` | save/load Adapter | header 字段顺序、旧版本分支、temp staging、`resetTime`、截断/未知版本、验证失败保留旧文件、backup 行为 | `proposed` |
| `WSV-REPLICATION` | 包 7/Projection | committed snapshot 生成稳定 wire bytes；出站不改状态；入站无权限/错误 sequence 不产生领域写入；客户端是 projection | `proposed` |
| `WSV-HARDMODE-SEAM` | Hardmode 事务 seam | `StartHardmode` 的字段事实、后台转换、I/O lock、主线程 follow-up、section reset 和保存互斥均可观测；不被普通 bool setter 替代 | `proposed`，`integration-review` |
| `WSV-OWNER-GUARD` | 写者收口 | WorldSession 核心字段只有一个写入系统；Calendar/Progression/Override/Storage/Network 只能经读视图或 Command/Port 接触 | `proposed` |

### 16.2 未来建议命令

未来若开始实施，应从仓库根目录按 `AGENTS.md` 的串行规则执行；下面仅是未执行的命令形状：

```powershell
# status: proposed; 未执行
pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 `
  test .\Test\WorldSession\WorldSession.FocusedVerifier.csproj `
  -m:1 -nr:false `
  -p:UseSharedCompilation=false `
  -p:MSBuildNodeReuse=false `
  -p:BuildInParallel=false

# status: proposed; 只有在实现后且已完成一次受控 build 才能使用
pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 `
  test .\Test\WorldSession\WorldSession.FocusedVerifier.csproj `
  --no-build --no-restore `
  -m:1 -nr:false `
  -p:UseSharedCompilation=false `
  -p:MSBuildNodeReuse=false `
  -p:BuildInParallel=false
```

本轮没有运行上述命令，也没有运行任何 compile-capable `dotnet` 命令；不能记录 build artifact、warning/error 数或测试通过结果。

## 17. 本次验证状态

```text
verificationStatus: not-run
```

本轮已完成的仅是只读证据工作：重新读取 Version4 关键源码段、tModLoader 离线 HTML、SS14 最小参考、当前 NLTX 文件和既有研究材料，并复核实际路径和行号。本轮未运行：

- `dotnet restore`、`build`、`test`、`run`、`publish`、`pack`、`msbuild` 或 `watch`；
- focused verifier、集成测试或行为回放；
- 任何源码迁移、组件创建或生产代码修改。

已有材料中的静态覆盖/差异结果只能作为 `existing-evidence` 使用，不能转写为本轮独立验证。尤其不能把 `WorldFile.cs` 静态差异为零或 `dome` 中存在模型，解释为 NLTX 已实现或行为等价。

## 18. 不拆分项

| 对象 | 不作为一级 `WorldSession` 子系统的理由 | 正确候选 |
| --- | --- | --- |
| 单个 `Main` 静态字段 | 一个字段没有独立生命周期、读写边界或替换价值；只有按共同写者和不变量分组才形成组件 | `WorldClockState`、`WorldWeatherState`、`WorldRulesState` 等 proposed 组件 |
| 单个天气效果或单个雨粒子 | 是天气规则的一次输出或表现，不拥有天气事实、随机、计时器和网络边界 | `WorldWeatherSystem` 或 `WorldWeatherPresentationProjection`，status: `proposed` |
| 单个事件类型，如 Blood Moon | 是事件实例/资格的一种值，不是独立世界会话根；它还与 NPC、广播和进度相连 | `WorldCalendarAndEventOrchestration` 的事件策略/状态，status: `proposed`，owner `integration-review` |
| 单个保存字段 | 只反映二进制协议中的一个位置，不能独立承担版本迁移、临时 staging、校验和回滚 | `WorldSessionPersistenceAdapter`，status: `proposed` |
| 客户端天气粒子和云 alpha | 由服务器天气事实派生，允许丢帧，不应进入权威存档或网络回写 | `WorldWeatherPresentationProjection`，status: `proposed` |
| `weatherCounter`、`numCloudsTemp` 等单个缓存 | 是天气策略中的调度/缓存成员，只有在确认持久化或跨系统读者后才提升为状态组件字段 | `WorldWeatherState` 内部字段或策略缓存，status: `proposed` |
| `WorldFileData.LoadException` | 是边界诊断和失败传输，不是可供模拟系统共同修改的世界事实 | load result/Adapter diagnostics，status: `proposed` |

## 19. 兼容策略和行为保持风险

### 19.1 兼容策略

1. 先建立只读 legacy facade：旧调用者继续读取旧字段，新的 owner 只提供单向镜像；禁止旧字段和新组件双向同步。
2. 先锁定 focused verifier，再迁移一个写集；任何字段迁移都必须证明旧写者已退出或改为 Command/Port。
3. 保存 Adapter 保留 Version4 header 的字段顺序、版本条件、temp staging、`resetTime`、验证和 rolling backup 语义。
4. 网络 Projection 保留包 7 的 wire order、位标志和发送触发点；把跨域组装留在 Projection，不让包 DTO 渗入核心组件。
5. 把 `WorldId`、`UniqueId`、持久化文件键、实体 ID、网络 ID、账户 ID 和区段 ID 分别建模；任何共享值对象最终标为 `crossSubsystemOwner: integration-review`。
6. 所有 RNG、文件、时钟、日志、网络、后台任务和 UI 效果都经显式 Port/Adapter；核心规则使用传入值和确定性计算。

### 19.2 主要风险

| 风险 ID | 风险 | 影响 |
| --- | --- | --- |
| `RISK-WS-ORDER` | 把 `WorldClockSystem` 前移到实体更新前 | NPC/Projectile/WorldItem 在昼夜边界读取到不同状态；会改变刷怪、掉落、事件和网络时序 |
| `RISK-WS-DOUBLE-WRITE` | `WorldSession`、Calendar、Progression 同时写 Blood Moon/Eclipse/Hardmode 或 `dayTime` | 状态漂移、重复广播、存档快照与运行态不一致 |
| `RISK-WS-HARDMODE` | 只迁移 `Main.hardMode` 布尔值，丢失后台地形转换和 section reset | 世界结构、网络 sections、存档锁和后续生态不一致 |
| `RISK-WS-SAVE-TEMP` | 把 `_temp*` 直接当运行时组件，或在 save 失败后保留临时值 | `resetTime`、失败回滚和继续游戏语义改变 |
| `RISK-WS-PACKET7` | 以新快照字段顺序替代旧包 7，或让客户端 projection 反写 | 旧客户端/服务器无法解释，或客户端获得非权威写权限 |
| `RISK-WS-READINESS` | `Ready` 只由每帧默认设置，未绑定 load/generation barrier | 世界尚未完成加载时进入实体 Tick；生成/液体/Tile 状态被并发读取 |
| `RISK-WS-DOME-ORDER` | 直接复用 `DomeSimulation.Tick` 的 clock-first 顺序 | 当前 dome 模型与 Version4 行为基线不一致，不能直接作为兼容实现 |
| `RISK-WS-SEED-STUB` | 用完整参考的秘密种子解析实现替代 Version4 空体而未做策略裁决 | 生成参数行为被改变；应明确记录为补证而不是静默修复 |

## 20. 未决问题、evidence-gap 和 blocking-decision

### 20.1 普通 evidence-gap

| gap ID | 缺口 | 状态 | 对本报告的影响 |
| --- | --- | --- | --- |
| `GAP-WS-PACKET7-CLIENT` | `MessageBuffer.cs:385-386` 的包 7 case 为空；本次未闭合客户端实际 WorldData 解码调用者 | `unresolved` | 只能确认服务器编码和发送触发，不能确认客户端恢复写入边界 |
| `GAP-WS-READINESS-SOURCE` | `Main.UpdateWorldPreparationState` 每帧设 Ready，但加载/生成/卸载所有转移的完整调用图未在本子系统范围内闭合 | `unresolved` | proposed lifecycle system 需由 RuntimeComposition/WorldStorage 整合 |
| `GAP-WS-PROGRESS-OWNER` | `NPC.downed*`、事件和入侵字段跨多个类写入 | `partial` | 只能提出 read view 和 Command 方向，不能宣布最终 owner |
| `GAP-WS-OVERRIDE-OWNER` | Creative/Journey 冻结时间、天气、难度、Biome spread 的调用分散 | `partial` | `SimulationRuleOverrides` 需在整合会话中锁定快照接口 |
| `GAP-WS-ROOT-DOME-CLOSURE` | 根 `src/WorldSession` 与 `dome` 模型/服务器是否同一运行时装配未由编译或项目引用验证 | `unresolved` | 不能把 dome 材料当成根项目实现 |
| `GAP-WS-FULLREF-SEED` | 完整参考补足的是 Version4 空体意图，不等于 Version4 当前运行行为 | `partial` | 种子解析迁移需单独行为裁决 |

### 20.2 blocking-decision

以下事项会改变必须保持的系统顺序、权威 owner 或事务边界，留给最终整合会话/用户裁决；本报告不替它们作最终选择。

#### `BD-WS-01`: 时钟和天气提交时序

- 方案 A：严格兼容。风/云在 Version4 的实体前阶段更新；雨/时钟在实体之后的 `UpdateTime` 槽位提交。优点是行为风险最低；代价是一个逻辑系统有两个明确调度入口。
- 方案 B：单一前置世界时钟。把完整时钟/天气提交移到实体前，实体读取本轮快照。优点是 ECS 读取一致；代价是改变 Version4 昼夜边界、刷怪和事件可见性。
- 方案 C：两阶段计算、旧槽位提交。前置阶段只计算 provisional delta，实体阶段使用旧快照，原 `UpdateTime` 槽位统一提交并发出边界事件。优点是保留可测顺序；代价是需要 provisional snapshot 和额外一致性验证。

最终必须锁定一个方案，否则 `WorldClockSystem`、`WorldWeatherSystem`、Calendar 和下游 NPC/WorldGen 的先后关系无法实现。

#### `BD-WS-02`: Hardmode 字段与事务 owner

- 方案 A：`WorldSession` 同时拥有 Hardmode 字段和转换事务。接口简单，但会把后台 WorldGen、WorldStorage、section reset 和进度事件重新塞回 WorldSession。
- 方案 B：`WorldSession` 只提供规则快照中的 Hardmode 事实，`WorldProgressionAndTransition` 拥有转换事务和完成状态。边界更深，但需要跨域 `WorldRulesSnapshot` 和 transformation Port。
- 方案 C：建立独立跨域 transition aggregate，由 WorldSession/Progression/Storage 共同读写。表达力强，但共享 owner 和调度复杂度最高。

本报告只确认 Version4 的事务事实，不宣布 A/B/C 的最终归属；候选 owner 标为 `integration-review`。

#### `BD-WS-03`: `WorldTickSnapshot` 的范围和 owner

- 方案 A：只包含 WorldSession 自有 descriptor/rules/clock/weather/readiness；事件和进度由各自 snapshot 单独传递。耦合小，但下游需要组合多个快照。
- 方案 B：建立包含事件/进度只读视图的跨域 `WorldTickSnapshot`。下游消费简单，但共享类型和版本化范围扩大。
- 方案 C：保留一个会话快照加多个跨域 Projection，网络/存档在边界组装。边界最清晰，但需要整合会话维护统一 revision。

`WorldTickSnapshot`、`WorldTimeView`、`WorldStateCommitBatch` 和网络/持久化值对象均不得由本报告宣布最终 owner，统一标记 `crossSubsystemOwner: integration-review`。

## 21. Integration Handoff

```text
subsystemId: WorldSession
taskNumber: 04
reportPath: D:\TRbackup\NLTX\docs\research\2026-09-05-version4-world-session-public-decomposition.md

evidenceStatus: partial
nltxStatus: partial
verificationStatus: not-run

confirmedOwners:
- Version4 的基础世界身份/世界头由 Terraria.IO.WorldFile、WorldFileData 和 Main/WorldGen 加载写入；候选领域 owner 是 WorldSession，但跨域 descriptor owner 仍需整合确认。
- Version4 的基础 dayTime/time/moonPhase 写入集中在 Main.UpdateTime 及昼夜边界方法；候选领域 owner 是 WorldSession 的 clock writer。
- Version4 的基础雨/风事实写入集中在 Main.UpdateWeather、StartRain、StopRain、ChangeRain 和 UpdateTime；候选领域 owner 是 WorldSession 的 weather writer。
- Version4 的完整实体更新资格由 Main.ShouldUpdateEntities 读取 readiness 与 generating barrier；实际 lifecycle 转移来源尚未闭合。

proposedTypes:
- proposed Component: WorldDescriptorState, path status: proposed
- proposed Component: WorldRulesState, path status: proposed
- proposed Component: WorldClockState, path status: proposed
- proposed Component: WorldWeatherState, path status: proposed
- proposed Component: SessionReadinessState, path status: proposed
- proposed Snapshot: WorldTickSnapshot, path status: proposed; crossSubsystemOwner: integration-review
- proposed System: WorldSessionLifecycleSystem, status: proposed
- proposed System: WorldClockSystem, status: proposed
- proposed System: WorldWeatherSystem, status: proposed
- proposed System: WorldSessionCommitSystem, status: proposed
- proposed Query: SessionReadyQuery, status: proposed
- proposed Query: WorldTimeQuery, status: proposed
- proposed Command: WorldRainStartCommand / WorldRainStopCommand / WorldWindChangeCommand, status: proposed
- proposed Port: IWorldStateCommitPort, status: proposed; crossSubsystemOwner: integration-review
- proposed Adapter: WorldSessionPersistenceAdapter / WorldDataReplicationProjection, status: proposed

sharedTypesForIntegrationReview:
- WorldTickSnapshot / WorldTimeView：candidate owner WorldSession，consumers 为 NPC、Calendar、Progression、WorldGen、Network、Persistence；writers 仅为 clock/weather/session commit；最终 owner unresolved。
- WorldRulesSnapshot：candidate owner WorldSession，consumers 为 Calendar、Progression、WorldGen、Override、Network、Persistence；Hardmode writer 可能属于 Progression；最终 owner integration-review。
- WorldStateCommitBatch / IWorldStateCommitPort：candidate owner WorldSession commit boundary，consumers 为 Storage、Network、Persistence、Calendar/Progression；不能在本报告中创建共享基础类型。
- WorldDataSnapshot / packet 7 DTO：candidate owner NetworkSessionAndSectionStreaming plus domain projections；跨 WorldSession、Calendar、Progression、WorldStorage；最终 owner integration-review。
- WorldPersistentId / NetworkId / EntityReference / WorldSectionId：必须保持身份分离；最终共享类型 owner integration-review。

crossSubsystemReaders:
- Player、NPC、Projectile、WorldItem、LeashedEntity：读取 clock/weather/rules/readiness 视图。
- WorldCalendarAndEventOrchestration：读取 clock boundary、weather/rules 和 readiness。
- WorldProgressionAndTransition：读取 rules/clock，并消费 Hardmode/progression command。
- WorldGenerationAndEcology、WorldStorage：读取 readiness/rules/weather，按顺序接收结构或生态提交。
- NetworkSessionAndSectionStreaming、PersistenceAndRecovery：消费 committed snapshots 和 projections。
- RuntimeComposition：读取 lifecycle/phase，只负责编排，不应直接写 WorldSession 状态。

crossSubsystemWriters:
- WorldFile/WorldStorage/WorldGeneration：只能通过 load/generation result 或受控 Port 触发 descriptor/readiness/结构提交。
- WorldCalendarAndEventOrchestration：可能触发雨、昼夜边界事件和日历命令，但不写 clock。
- WorldProgressionAndTransition：可能提交 Hardmode/progression transition；不直接双写 WorldSession 的 clock/weather。
- SimulationRuleOverrides：只提供 effective override snapshot；不覆盖基础 rules/weather 字段。
- Network Adapter：只产生已验证 Command；客户端和出站 serializer 不得写权威状态。

orderingConstraints:
- Main.DoUpdate 的 readiness 更新、Netplay 主线程更新、自动保存、UpdateWeather、ShouldUpdateEntities 顺序必须可观察。
- DoUpdateInWorld 中必须保持 Player -> NPC/spawn -> Projectile -> WorldItem -> LeashedEntity -> UpdateTime -> WorldGen/侵袭 -> UpdateServer 的 Version4 真实顺序，除非 BD-WS-01 由整合会话明确裁决。
- WorldGen.UpdateWorld 不得在 isGeneratingOrLoadingWorld 或 generatingWorld barrier 期间运行。
- Hardmode transformation 的后台锁、主线程 follow-up、section reset 和保存互斥必须先于其后依赖系统观察完成状态。
- Persistence/Replication 只消费 committed snapshot；入站网络只产生 Command。

boundaryChallenges:
- 当前 src/WorldSession/WorldSessionComponents.cs 与 WorldGeneration 下存在同名状态类型和命名空间分裂，实施前需整合路径/owner。
- dome/src 的 DomeSimulation.Tick 目前 clock/weather/progression 早于 ApplyPlayerInputs，与 Version4 的 UpdateTime 槽位不一致，不能直接复用为行为等价实现。
- 任务包 WorldFile 路径与实际 Terraria.IO/WorldFile.cs 不一致，已记录 evidence-mismatch。
- Version4 包 7 服务器编码已确认，但客户端 decode 调用链未闭合。

evidenceGaps:
- GAP-WS-PACKET7-CLIENT：包 7 客户端恢复写入路径 unresolved。
- GAP-WS-READINESS-SOURCE：加载/生成/卸载所有 readiness 转移 unresolved。
- GAP-WS-PROGRESS-OWNER：事件/进度/Hardmode 的跨域 writer partial。
- GAP-WS-OVERRIDE-OWNER：Creative/Journey effective snapshot owner partial。
- GAP-WS-ROOT-DOME-CLOSURE：根 src 与 dome 运行时装配未编译验证 unresolved。
- Version4 WorldFileData 的种子解析空体只能由完整参考 supplemented，不能作为当前 Version4 行为确认。

blockingDecisions:
- BD-WS-01：时钟/天气采用严格旧槽位、前置提交或两阶段 provisional/commit。
- BD-WS-02：Hardmode 字段与 Hardmode 转换事务的 owner 选择。
- BD-WS-03：WorldTickSnapshot 是 WorldSession-only、跨域聚合还是多 Projection 组合。

notImplemented:
- 当前根 src/WorldSession 没有已接线的 WorldSessionSystem、WorldTickSnapshot、WorldStateCommitPort、Persistence Adapter、Replication Projection 或 focused verifier。
- 当前报告没有创建 Component、System、Query、Command、Adapter、Projection、测试或项目文件。
- 当前报告没有修改 src、Test、dome/src、Version4、完整参考源码、tModLoader 文档或其他共享审查材料。

verifierPlan:
- WSV-READINESS：阻断/放行实体 Tick 的生命周期和生成 barrier。
- WSV-TICK-ORDER：记录并断言 Version4 的真实顺序和昼夜边界可见性。
- WSV-CLOCK / WSV-WEATHER：边界、暂停、随机、雨风和 override 的确定性行为。
- WSV-PERSISTENCE：header 顺序、temp staging、resetTime、验证失败和 rolling backup。
- WSV-REPLICATION：包 7 golden bytes、出站只读、入站 Command 权限和客户端 projection。
- WSV-HARDMODE-SEAM：后台转换、IOLock、主线程 follow-up、section reset 和保存互斥。
- WSV-OWNER-GUARD：单写者、无双向同步和跨域 Port 约束。
- 所有 verifier 和建议命令均为 status: proposed，本轮 verificationStatus 为 not-run。
```

## 22. 最终声明

本报告是基于 Version4、完整参考源码、tModLoader 公开文档和有限 ECS 结构参考形成的子系统边界与 ECS 拆分设计。它不是迁移完成报告，不是行为等价证明，不是 API 兼容证明，也不是当前 NLTX 已实现能力的声明。

本报告只写入了任务指定的唯一报告文件，未修改生产代码、测试代码、`dome/src`、Version4、完整参考源码、tModLoader 文档或其他共享审查材料。本轮未运行构建或测试，验证状态为 `not-run`。
