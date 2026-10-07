# 类别 09：UI、聊天、社交、音效与调试消息执行文档

文档 ID：NETMSG-CAT-09
执行模型：gpt-6-luna，reasoning=max
分支：codex/network-message-09-ui-social
会话：network-message-09-ui-social
类别 owner：表现层、聊天/社交、传送结果和受控调试入口

## 1. 目标

完成 UI/社交/音效/调试消息的受控注册，保证表现包不修改权威模拟；调试命令只有显式
开发授权才能出现；传送/地图标记等包必须先由领域 owner 提交，再生成表现或结果。
本类不拥有 NetModules 82 的中央注册，82 的唯一 gateway owner 在第 10 类。

## 2. 消息范围

65 TeleportEntity；66 Unknown66；67 Unknown67；68 Unknown68；82 NetModules；
94 DevCommands；95 MurderSomeoneElsesPortal；96 TeleportPlayerThroughPortal；
109 MassWireOperation；120 Emoji；132 PlayLegacySound；154 Ping。

实际唯一注册归属约束：

- 82：第 10 类持有中央 module/action registry；本类只提供经过授权的 presentation
  module adapter，不能再次注册 packet 82。
- 109、154：第 5 类持有世界写/地图范围准入；本类只消费结果或注入 UI projection。
- 120：第 3 类持有玩家社交输入 registration；本类提供 presentation 输出契约。
- 132：第 7 类持有战斗结果来源；本类提供音效播放 adapter。

## 3. 实施步骤

1. 为 65/66/67/68/94/95/96/120/132 建立方向、阶段、速率和授权表；未知包不新建
   ECS 组件。
2. 65/96 先由 Player/NPC/World owner 提交位置或传送结果，再输出客户端 projection；
   不接受客户端自选目标实体或位置。
3. 94 默认关闭，只有显式 development capability、host 权限、审计和速率后才可开放；
   不把命令文本直接传给生产游戏 owner。
4. 95 按实体所有权、距离、目标可见性和世界代次验证；不能杀/删其他玩家实体。
5. 120/132 只生成非权威表现事件，不能改生命、库存、世界或任务。
6. 66/67/68 记录未知命中和原始字段范围，按第 10 类兼容策略处理，不把 opaque 变成
   通用“成功”。
7. 与第 5/7/10 类对齐唯一注册表和 handler type，解决重叠后再接入 host。

## 4. 非目标

不实现完整 UI、聊天渲染、声音资源、世界连线批量操作和 NetModule 协议编解码；
不开放公网 DevCommands；不运行真实客户端所有表现包。

## 5. 10% 核心测试

只执行一组小集合：

1. 传送请求/结果（65 或 96）验证领域先提交、目标可见性/旧代次拒绝和只读 projection。
2. 94 未授权拒绝，以及 120/132 一个表现事件不改变权威状态；若 82 由第 10 类测试，
   本类只做 adapter contract 静态检查，不重复运行。

优先使用 RoutingVerification、SocialPacketVerification、SocialNpcEffectVerification
或对应 focused case；不运行全量网络验证。构建受影响项目，报告 exit code、warning/error
和 Build/bin 输出。

## 6. 验收

必须列出 12 个 ID 的实际唯一 owner、重复 ID 处理、授权条件和 presentation-only 证据；
特别说明 82/109/120/132/154 未重复注册。无调试授权拒绝、无领域写入证明或存在第二个
packet-82 key 时验收失败。

