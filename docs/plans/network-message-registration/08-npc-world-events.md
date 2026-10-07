# 类别 08：NPC、Boss、入侵、任务与世界事件消息执行文档

文档 ID：NETMSG-CAT-08
执行模型：gpt-6-luna，reasoning=max
分支：codex/network-message-08-npc-world-events
会话：network-message-08-npc-world-events
类别 owner：NPC 交互、Boss/入侵、世界事件状态和任务效果

## 1. 目标

完成 22 个 NPC/事件相关消息的分类注册和 owner 组合。NPC 快照/战斗由其他类别提供；
本类负责 NPC 交互意图、事件启动/进度/结果和世界事件状态。世界事件不塞进某个 NPC
组件，必须有独立 WorldEventState 或等价 owner。

## 2. 消息范围

40 SyncTalkNPC；60 Unknown60；61 SpawnBossUseLicenseStartEvent；62 Unknown62；
77 TemporaryAnimation；78 InvasionProgressReport；91 SyncEmoteBubble；
100 TeleportNPCThroughPortal；101 UpdateTowerShieldStrengths；107 SmartTextMessage；
111 ToggleParty；113 CrystalInvasionStart；114 CrystalInvasionWipeAllTheThingsss；
116 CrystalInvasionSendWaitTime；126 SyncRevengeMarker；127 RemoveRevengeMarker；
136 SyncCavernMonsterType；140 SetMiscEventValues；141 RequestLucyPopup；
143 CrystalInvasionRequestedToSkipWaitTime；144 RequestQuestEffect。

## 3. 基线与重叠处理

40/91/107/141/144 的部分表现/任务语义可能与第 6/9 类相交；本类拥有 NPC/事件领域
提交和结果来源，第 9 类只能作为 presentation adapter，不得重复 gateway.Register。
131 TamperWithNPC 属于第 6 类的 NPC 经济/交互 owner，本类只提供事件 owner 需要的端口。

60/62 是未知/兼容；61/113/111/143 是高风险启动/跳过请求；78/101/114/116/126/127/
136/140 是服务端事件结果或快照。客户端不能直接设置入侵进度、Tower shield、复仇标记
或 misc event values。

## 4. 实施步骤

1. 建立 22 项登记和唯一 owner 关系，先识别当前 SocialPacketRegistration、NPC event、
   WorldSession、Quest owner 的已有能力。
2. 对 40/111/141/143/144 等请求验证玩家绑定、距离、任务/物品/事件前置、权限和速率；
   事件提交必须是原子或显式可回滚的领域操作。
3. 对 61/113/78/101/114/116/126/127/136/140 提供权威 WorldEventState projection，
   并带 world generation/事件 revision。
4. 对 77/91/107 只传递表现/文本/气泡结果；不让客户端触发 NPC AI 或世界事件。
5. 对 100 传送结果先由服务端提交位置/实体 owner，再发送 projection；不允许 packet
   直接改 NPC Transform。
6. 对 60/62 记录未知字段和运行时命中，保持兼容隔离。

## 5. 非目标

不实现完整 NPC AI、Boss 行为、事件波次算法、任务经济和客户端 UI；不把事件结果当成
客户端可写状态；不与第 4/7 类重复 NPC/伤害 owner。

## 6. 10% 核心测试

只执行两组：

1. 一个事件启动/请求（113 或 111/143）的合法前置、无权限/重复请求拒绝，并验证一个
   权威进度/等待时间 projection。
2. 一个 NPC 交互或传送结果（40/100/141/144）的合法目标、距离/代次失败和结果发布。

优先使用 NPC、WorldSession、SocialNpcEffect 或 Network Verification 中可筛选的 focused
case。只跑选定 case，受影响项目增量构建并记录证据。

## 7. 验收

回报必须给出 22 个 ID 的方向、唯一 owner、WorldEventState 归属、未知包策略、至少一
成功和一失败输出、测试命令/exit code/warning/error/Build/bin，以及与第 6/7/9 类共享
接口的冲突。事件状态若仍写在 NPC 网络 handler 内，不能 complete。

