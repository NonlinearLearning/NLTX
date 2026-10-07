# 类别 03：玩家身份、输入和角色状态消息执行文档

文档 ID：NETMSG-CAT-03
执行模型：gpt-6-luna，reasoning=max
分支：codex/network-message-03-player-state
会话：network-message-03-player-state
类别 owner：玩家输入、玩家状态投影、Buff/装备意图和角色结果

## 1. 目标

完成本类别消息的唯一注册 owner、可信 Actor 映射和玩家状态/输入的命令与投影边界。
客户端声明只能形成经过授权的 Command；S2C 生命、死亡、Buff 和库存确认不能被客户端
直接写入 ECS。保留现有 PlayerLifecyclePacketRegistration、PlayerExtendedPacketRegistration、
PlayerStatePacketRegistration 的能力，先审计再补缺，禁止重复注册。

## 2. 消息范围

5 SyncEquipment；13 PlayerControls；16 PlayerLifeMana；30 TogglePVP；35 PlayerHeal；
36 SyncPlayerZone；41 ItemRotationAndAnimation；43 ManaEffect；45 TeamChange；
50 PlayerBuffs；55 AddPlayerBuffPvP；80 SyncPlayerChestIndex；84 PlayerStealth；
99 MinionRestTargetUpdate；102 NebulaLevelupRequest；115 MinionAttackTargetUpdate；
117 PlayerHurtV2；118 PlayerDeathV2；120 Emoji；125 SyncTilePicking；
134 UpdatePlayerLuckFactors；135 DeadPlayer；138 ClientSyncedInventory；
147 SyncLoadout；150 SpectatePlayer；157 TeamChangeFromUI。

## 3. 关键基线

当前已有 4/12/13/14/16/42/50、30/35/36/40/41/45/55/66/76/84/99/115/134、
43/51/73/96/102/125/135/147/150/157 等注册。先输出完整 ID→registration 快照，
再决定缺口；不要通过复制一个 handler 类来掩盖同 ID 重复或错误方向。

消息 13 的可信身份必须取 NetworkSessionContext.Actor，packet 的 playerIndex 只是线上
声明。位置/速度/坐骑/交互/镜头输入应进入现有 PlayerPacket13RouteQuery 或对应 owner，
不能直接覆盖完整 Player 状态。120/Emoji 与 132/PlayLegacySound 的表现语义分别由本类
和第 9 类协调，只有一个 gateway key。

## 4. 实施步骤

1. 为 5、13、30、36、41、45、55、84、99、102、115、125、134、147、150、157 审计
   C2S 方向、阶段、频率和权限；为 16、35、43、50、80、117、118、135、138 建立只读
   S2C/确认投影，禁止把结果当输入。
2. 统一 Actor/目标区分，特别检查 PVP 目标、受伤对象、观战对象、minion 目标和 UI 队伍
   变更，不能把所有 player 字段强制改为发送者槽位。
3. 复用现有 Player owner、Buff/Status、Inventory、Combat、Presentation 端口；缺口新增
   最小契约，不在 Network 层复制游戏规则。
4. 对装备、幸运、Buff、loadout、inventory sync 增加服务端校验：槽位、物品来源、前置
   Buff、世界/玩家状态和频率。
5. 保持 13 的自回显和广播目标语义，发送完成不等于客户端确认；旧代次和旧 Actor 输出丢弃。
6. 对 120/Emoji 只实现非权威社交事件；不写入模拟状态。

## 5. 非目标

不实现完整 Player ECS、移动碰撞、库存原子交易、战斗结算、Tile 操作和客户端 UI；
不把 13 当成整个 Player 快照；不新增第二套 Buff/Inventory owner。

## 6. 10% 核心测试

只运行三组核心用例：

1. Packet13 伪造 playerIndex、位置边界和自回显/广播目标检查。
2. 装备或 loadout 变更的合法/非法槽位与权限检查，覆盖一个 PlayerState handler。
3. PlayerHurtV2/PlayerDeathV2/PlayerBuffs 的服务端投影只能由 owner 生成，客户端伪造
   S2C 方向被拒绝。

优先复用 PlayerNetworkOwnerVerification、PlayerLifecycleHandlerAcceptanceVerification、
PlayerExtendedPacketVerification、PlayerAdmissionPacketVerification 中的对应 case；
必要时增加筛选入口，但不跑整个 verifier 集合。受影响项目按实际改动构建。

## 7. 验收

必须提供 26 个 ID 的登记快照、重复 key 检查、Actor/目标映射表、至少一正一反核心测试
输出和完整未覆盖清单。报告包含 exit code、warning/error、Build/bin 输出、共享文件冲突
和未实现原因。未能证明 S2C 方向保护时不得标 complete。

