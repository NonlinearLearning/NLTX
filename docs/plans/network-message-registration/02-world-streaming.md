# 类别 02：初始世界装载与区域流送消息执行文档

文档 ID：NETMSG-CAT-02
执行模型：gpt-6-luna，reasoning=max
分支：codex/network-message-02-world-stream
会话：network-message-02-world-stream
类别 owner：世界快照、区段订阅、传送协调和装载屏障

## 1. 目标

完成世界初始资料、区段快照、区段请求和加载确认的注册与权威路由。所有输出必须绑定
当前 GameSession、WorldRuntimeId、世界代次、区段和 SnapshotRevision；客户端请求不能
越过可见性、边界或阶段验证。Tile 快照属于世界存储/网络投影，不创建替代实体组件。

## 2. 消息范围

| ID | 名称 | 方向/性质 | 重点 |
| ---: | --- | --- | --- |
| 8 | SpawnTileData | S2C | 初始 spawn 区域数据 |
| 10 | TileSection | S2C | 大区段权威快照、受限大包 |
| 11 | TileFrameSection | Legacy | 兼容读取，不新建业务 handler |
| 20 | AreaTileChange | S2C | 提交后的区域增量 |
| 73 | RequestTeleportationByServer | S2C | 传送/区域加载协调 |
| 158 | ExtraSpawnSectionLoaded | C2S | 额外 spawn 区段就绪确认 |
| 159 | RequestSection | C2S | 区段请求、边界和可见性 |

## 3. 基线和边界

先核对 WorldPacketBehaviorVerification、WorldMovementSectionVerification、WorldFrameRepairTcpVerification、
PacketSnapshotCache、SectionSubscribers 和 NetworkGatewayHost 的既有实现。当前文档已说明
编号 10 编解码、缓存和大包资格存在独立额度；不可因想提高吞吐而移除单并发或 16 MiB
缓存边界。

编号 11 标为废弃；默认只保留兼容读取/诊断，不把它注册成新的有效世界快照。编号 73
是服务端协调结果，客户端不能据此自选位置。编号 158/159 必须使用当前连接的世界代次
和订阅投影，迟到旧代次确认直接拒绝。

## 4. 实施步骤

1. 盘点每个包的实际 codec 方向、生成布局和当前 registration，杜绝 10 的 C2S 误开放。
2. 将 159 映射为有界 RequestSectionCommand 或现有世界 section owner 调用；验证玩家绑定、
   Active/同步阶段、世界 key、坐标边界、可见性和速率。
3. 将 158 映射为 section-loaded acknowledgement，只推进当前连接的屏障，不推进整个
   世界或其他玩家。
4. 将 8、10、20、73 作为 owner 已提交后的 S2C projection；输出携带当前代次，缓存键
   必须包含 profile、world、generation、section、revision 和格式变体。
5. 确认读取、编码、缓存命中和发送失败的副作用隔离：缓存失败回退普通编码，领域提交
   不因发送失败回滚。
6. 维持编号 10 的独立大包资格、半帧期限、预算和目标复验；不能通过广播缓存绕过目标
   session/epoch 校验。
7. 若共享 host wiring 不足，新增最小 section registration seam，不重写 PacketGateway
   路由算法。

## 5. 非目标

不实现 Tile 破坏/放置命令、液体模拟、完整世界持久化、玩家物理或客户端渲染；不修改
源目录下的生成输出；不把 TileSection 当作实体列表。

## 6. 10% 核心测试

只执行以下两类核心 smoke：

1. 一个 active 玩家请求合法 section，验证目标世界/区段快照、缓存命中与 revision 输出；
   再用旧 world generation 或越界 section 请求，验证拒绝且不产生领域写入。
2. 编号 10 的分片/大包读写、预算耗尽或调用方取消边界，验证已领取帧正确交付、
   迟到旧代次不投递。

优先使用 Test/NSSLC.Infrastructure.Network.Verification 的
WorldMovementSectionVerification、WorldFrameRepairTcpVerification、WorldPacketBehaviorVerification；
不运行全量 Program sweep。构建只针对该 verifier 项目，验证使用 --no-build --no-restore。

## 7. 验收条件和回报

回报必须包含 7 个 ID 的方向和准入表、section owner 入口、缓存键/失效证据、成功和拒绝
样例、测试命令/exit code/warning/error、Build/bin 输出及 11 的兼容策略。没有实际 revision、
代次和目标复验输出只能标 partial。

