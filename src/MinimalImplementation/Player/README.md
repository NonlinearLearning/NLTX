# Player：玩家状态与每帧更新

[返回总览](../README.md) · [会话](../Session/README.md) · [物品与背包](../Inventory/README.md)

## 作用与状态归属

Player 保存在线玩家的生命、外观、移动、队伍、Buff、死亡与出生等状态，负责验证并提交
对应变化。当前 [Player](Player.cs) 是一个聚合运行时对象，内部组合多个状态类型；
这些类型并未全部转成统一 ECS 存储。主要写入入口是 [PlayerOwner](PlayerOwner.cs)。

Session 维护连接阶段与绑定，Inventory 维护物品槽，World 维护地形。Player 使用这些系统
提供的捕获事实或准备对象，不把它们的状态复制成另一份权威存储。

## 组成玩家的主要状态

| 状态 | 表达什么 | 主要使用方 |
| --- | --- | --- |
| [PlayerVitals](PlayerVitals.cs) | 当前及最大生命、魔力 | 生命包处理、治疗和生命周期 |
| [PlayerAppearance](PlayerAppearance.cs) | 名称、外观和颜色等玩家信息 | 入服快照、玩家信息同步 |
| [PlayerMovementInput](PlayerMovementInput.cs) | 客户端控制、位置、速度等输入 | 控制包处理和运行时空间准备 |
| [PlayerRuntimeSpatialState](PlayerRuntimeSpatialState.cs) | 碰撞尺寸、坐骑等运行时空间信息 | 区块捕获、NPC 目标、物品和传送查询 |
| [PlayerTeam](PlayerTeam.cs)、[PlayerPvpStatus](PlayerPvpStatus.cs) | 队伍及 PvP 状态 | 队伍出生、玩家间规则和输出 |
| [PlayerBuffs](PlayerBuffs.cs)、[PlayerImmunities](PlayerImmunities.cs) | Buff 槽、免疫及相关计时 | Buff 协调器、每帧更新 |
| [PlayerLuck](PlayerLuck.cs)、[PlayerZones](PlayerZones.cs) | 幸运和所在环境信息 | 规则计算、场景事实捕获 |
| [PlayerTargetRelation](PlayerTargetRelation.cs)、[PlayerSpectating](PlayerSpectating.cs) | 目标引用和观战关系 | 实体有效性检查、死亡观战和区块需求 |

Player 还维护 Epoch、Handle、Version、LoadoutIndex、出生和死亡计数。
`IsDead` 是生命周期状态，单纯增加生命不会完成复活；`IsHost` 与
`CountsAsHostForGameplay` 分别表达授权和游戏规则中的主机身份。

## 系统部件怎样分工

| 部件 | 作用 |
| --- | --- |
| [PlayerOwner](PlayerOwner.cs) | 创建、查询、移除玩家，验证版本并提交玩家变化 |
| [PlayerStatePacketHandlers](PlayerStatePacketHandlers.cs) | 把认证后的玩家包转为 Owner 操作 |
| [PlayerLifecycleCoordinator](PlayerLifecycleCoordinator.cs) | 组合死亡、出生与会话生命周期相关操作 |
| [PlayerUpdateService](PlayerUpdateService.cs) | 捕获一批玩家输入，计算并提交每帧结果 |
| [PlayerServerTickPhaseOwner](PlayerServerTickPhaseOwner.cs) | 将 PlayerUpdateService 接到 Player 阶段 |
| [PlayerSpatialFactsService](PlayerSpatialFactsService.cs)、[PlayerRuntimeFactsCaptureService](PlayerRuntimeFactsCaptureService.cs) | 用当前玩家状态和外部事实构造明确的读取快照 |
| [PlayerBuffCoordinator](PlayerBuffCoordinator.cs)、[PlayerDodgeCoordinator](PlayerDodgeCoordinator.cs) | 处理 Buff 和闪避变化及其提交边界 |
| [PlayerTeamChangeCoordinator](PlayerTeamChangeCoordinator.cs)、[PlayerSpectatingCoordinator](PlayerSpectatingCoordinator.cs) | 协调队伍或观战变化与区块、出生等参与者 |

## 一次移动的协作过程

1. Host 接收 PlayerControls，Session 认证当前绑定与允许阶段。
2. [PlayerControlsWorldCaptureSource](../Host/PlayerControlsWorldCaptureSource.cs) 捕获需要的世界事实。
3. 玩家处理入口更新移动与运行时空间状态，提交新版本。
4. Host 捕获旁观者所需的玩家输出，并按新位置捕获未交付的区块。
5. NPC 目标选择、世界物品归属等后续阶段读取已经提交的玩家空间事实。

坐骑等变化会影响尺寸。只看包里的位置而忽略 RuntimeSpatial，可能让碰撞、区块范围和
物品距离判断使用不同的玩家形状。跟读入口是
[Version4InboundRouter.PlayerOutput.cs](../Host/Version4InboundRouter.PlayerOutput.cs)。

## 一帧的协作过程

`PlayerUpdateService.Tick` 先捕获整批玩家所需的外部输入，再计算结果并校验来源。
准备完成的状态通过 PlayerOwner 提交，结果由 PlayerTickCommit 等对象传给输出收集方。
需要观战区块变化时，协调器同时准备相关区块操作。

这种顺序让一次计算使用同一批输入；快照中携带的来源版本仍要在提交时检查。
读取已经过期的世界或玩家事实，应返回相应失败结果，不能直接覆写当前状态。

## 和物品、传送如何协作

[PlayerPickupPreparationService](PlayerPickupPreparationService.cs) 为拾取规则提供玩家效果
准备对象；[PlayerPylonTeleportPreparationService](PlayerPylonTeleportPreparationService.cs)
为晶塔传送准备玩家变化。外部系统通过这些接口组合操作，PlayerOwner 保持玩家状态的提交责任。

默认原版世界物品拾取还有客户端归属与背包上传协议，见 [Inventory](../Inventory/README.md)。
不能仅因存在玩家拾取准备服务，就认为运行时所有拾取都由服务端直接增加背包。

## 建议阅读顺序与边界

先读 Player → PlayerOwner → PlayerStatePacketHandlers → PlayerUpdateService，
再按问题进入 Buff、队伍、观战、传送或拾取协调器。网络广播和完整认证路径在 Host、Session。
此目录提供当前已实现的规则与状态入口，不等于拥有原版玩家全部本地模拟和所有装备效果。
