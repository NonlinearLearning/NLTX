# Modules：Packet 82 分发与模块状态

[返回总览](../README.md) · [创造能力](Creative/README.md) · [协议](../Protocol/README.md)

## 作用与协作边界

Packet 82 将多个子协议装进一个普通包中，模块编号决定交给哪个领域处理。
[ModuleDispatcher](ModuleDispatcher.cs) 负责验证分发上下文与选择唯一处理器；
各领域 Owner 负责规则和状态。模块并不都在此目录实现，例如液体在 World、合成在
Inventory、晶塔在 Mechanisms、标签效果在 Combat。

## 分发部件怎样协作

| 部件 | 作用 |
| --- | --- |
| [IModuleHandler](IModuleHandler.cs) | 声明模块编号与 Handle 入口 |
| [ModuleDispatchContext](ModuleDispatchContext.cs) | 提供认证发送者、会话阶段和捕获事实 |
| [ModuleDispatcher](ModuleDispatcher.cs) | 验证 Packet 82、Epoch、Active 阶段及模块注册，拒绝重复所有者 |
| [ModuleDispatchReplayCache](ModuleDispatchReplayCache.cs) | 按信封身份缓存已执行结果，避免重放导致重复修改 |
| [ModuleDispatchResult](ModuleDispatchResult.cs) | 返回提交结果、输出和可选交付结算 |
| [Packet82ModuleIntentProducer](Protocol/Packet82ModuleIntentProducer.cs) | 将模块输出构造成普通出站意图 |
| [IModuleEpochRetirable](IModuleEpochRetirable.cs) | 会话关闭时清理处理器局部状态 |

构造分发器时，每个编号最多有一个所有者；`IsComplete` 判断 0–14 是否全部注册。
注册齐全说明分发覆盖，不代表该编号下每种内容、动作和依赖都可用。

## 当前默认模块表

实际注册入口是 [ServerComposition.CreateModuleDispatcher](../Host/ServerComposition.cs)，
Host 适配器主要在 [CompleteModuleHandlers](../Host/CompleteModuleHandlers.cs)。

| ID | 模块 | 主要处理方与作用 |
| --- | --- | --- |
| 0 | Liquid | WorldLiquidModuleHandler → WorldOwner，液体赋值 |
| 1 | Text | TextModuleHandler，聊天与命令 |
| 2 | Ping | PingModuleHandler，地图标记等 Ping 表现 |
| 3 | Ambience | AmbienceModuleHandler，环境表现 |
| 4 | Bestiary | BestiaryModuleHandler → BestiaryState，图鉴状态 |
| 5 | CreativePowers | CreativePowerModuleHandler → CreativePowerStateOwner |
| 6 | CreativeUnlocksPlayerReport | CreativeUnlockReportModuleHandler，研究上报的认证与转发 |
| 7 | TeleportPylon | 晶塔模块适配器 → PylonInteractionService |
| 8 | Particles | ParticlesModuleHandler，粒子表现及输入限制 |
| 9 | CreativePowerPermissions | 创造能力权限快照，拒绝客户端覆写 |
| 10 | Banners | BannerModuleHandler → BannerState，旗帜状态 |
| 11 | CraftingRequests | Inventory 的 CraftingModule11Handler |
| 12 | TagEffectState | Combat 的 TagEffectStateOwner |
| 13 | LeashedEntity | Mechanisms 的 LeashedEntityModuleHandler |
| 14 | UnbreakableWallScan | UnbreakableWallModuleHandler，不可破坏墙扫描子协议 |

模块 ID 与普通包消息 ID 是两个层级。例如模块 6 的布局出错，应检查 Packet 82 中的模块
载荷，不能将其当成普通 Packet 6 世界信息请求。

## 一次模块请求的执行路径

Protocol 解出 Packet 82 及子模块 → Host 解析当前会话绑定并捕获 ModuleDispatchContext →
分发器校验阶段、Epoch 与重放 → 唯一 Handler 处理领域请求 → 返回已提交结果和输出 →
Host 发送 → 将发送回执交给结算对象。

同一个已识别信封重试不应再次改变状态；关闭时 `RetireEpoch` 同时退休重放状态及处理器
局部状态。网络发送成功和领域提交成功分别记录，不能用一个成功标志覆盖整个流程。
该缓存针对同一个内部捕获信封；客户端再次发送而产生新序列的包仍会正常校验，不能把它
理解为按相同载荷永久去重。

## 此目录内的状态系统

| 领域 | 阅读入口 | 状态与协作 |
| --- | --- | --- |
| 聊天 | [TextModuleHandler](Social/Chat/TextModuleHandler.cs)、[ChatCommandProcessor](Social/Chat/ChatCommandProcessor.cs) | 认证发送者、聊天预算、命令处理和队伍接收者选择 |
| 社交表现 | [PingModuleHandler](Social/Ping/PingModuleHandler.cs)、[EmoteCoordinator](Social/EmoteCoordinator.cs) | Ping 和表情规则；具体入口需继续核对 Host |
| 表现效果 | [ParticlesModuleHandler](Effects/ParticlesModuleHandler.cs)、[ParticleRateLimiter](Effects/ParticleRateLimiter.cs) | 验证表现载荷与频率，避免将表现请求当成地形或战斗状态 |
| 图鉴 | [BestiaryState](Bestiary/BestiaryState.cs) | 图鉴贡献、快照与脏状态；按版本结算交付 |
| 旗帜 | [BannerState](Banners/BannerState.cs) | 旗帜状态、快照与交付结算 |
| 世界事件 | [WorldEventCoordinator](Events/WorldEventCoordinator.cs)、[InvasionState](Events/InvasionState.cs) | 事件转换、入侵进度和效果意图 |
| 时间关联事件 | [LanternNightState](Events/LanternNightState.cs)、[AnglerQuestState](Events/AnglerQuestState.cs) | 灯笼夜和渔夫任务等独立状态 |

## Tick、同步与保存如何连接

默认 Invasion 阶段通过 [WorldTickInvasionBridge](../Host/WorldTickInvasionBridge.cs) 消费世界
Tick 事实；周期同步由 [ModulePeriodicSyncOwner](../Host/ModulePeriodicSyncOwner.cs) 捕获
图鉴、旗帜、创造权限及其他明确注册的脏输出。事件服务存在并不意味着所有普通事件包
或事件触发条件已经接入运行时。

Host 保存时将图鉴、旗帜、入侵、灯笼夜及世界范围创造状态投影进文档。
聊天、重放缓存和每连接交付进度属于在线运行状态，不会因放在 Modules 目录而自动持久化。
阅读单个模块时，应同时定位 Handler、Owner、Producer、Settlement 和保存投影。
