# 类别 01：会话与连接生命周期消息执行文档

文档 ID：NETMSG-CAT-01
执行模型：gpt-6-luna，reasoning=max
分支：codex/network-message-01-session
会话：network-message-01-session
类别 owner：网络会话、认证、加入和 host 权限边界

## 1. 目标

完成本类别 16 个消息的格式方向、会话阶段准入、handler/owner 接口和最小核心验收，
使连接从 Hello 到 Active 的路径可审计、不会因客户端声明越权，也不会把密码、HostToken
或平台握手数据写入领域实体、诊断日志或缓存。

本类别不是“把 16 个 ID 全部允许通过”。对于没有足够语义证据的包，正确交付可以是
明确拒绝、opaque 隔离或只读兼容路径，但必须有代码和测试证明。

## 2. 消息范围

| ID | 名称 | 方向/性质 | 重点 |
| ---: | --- | --- | --- |
| 1 | Hello | C2S | 版本、连接资格、阶段 0 |
| 2 | Kick | S2C | 终止原因和限时关闭 |
| 3 | PlayerInfo | C2S 后 Relay | 分配槽位后的身份资料校验 |
| 4 | SyncPlayer | S2C/Relay | 静态玩家资料投影 |
| 6 | RequestWorldData | C2S | 世界装载请求 |
| 7 | WorldData | S2C | 世界权威快照 |
| 9 | StatusTextSize | S2C | 装载进度表现 |
| 12 | PlayerSpawn | C2S 后 Relay | 加入/生成确认 |
| 14 | PlayerActive | Both | 连接激活状态，不能改写他人槽位 |
| 37 | RequestPassword | S2C | 登录挑战 |
| 38 | SendPassword | C2S | 密码，不得落日志 |
| 49 | InitialSpawn | S2C | 初始生成指令 |
| 93 | SocialHandshake | Both | 平台会话能力，默认不开放业务语义 |
| 129 | FinishedConnectingToServer | C2S | 同步完成确认 |
| 139 | SetCountsAsHostForGameplay | S2C | 服务端授予 host 权限 |
| 161 | HostToken | C2S | host token，只在明确授权阶段接受 |

## 3. 已知基线与必须先核对的缺口

先核对 PacketGateway 构造函数、FindPolicy、Register 对 1/38/93/161 的特殊处理，
NetworkSession 的阶段迁移，以及 PacketDefinitions 中每个实际方向。当前代码已经对
Hello、密码、HostToken、host authorization 留有内置路径，但这不等于 16 个消息的
完整方向注册；不要直接对 1、38、93、161 调用普通 gateway.Register 绕过安全检查。

核对以下事实后再改代码：

- 生成 codec 的 wire direction 是否与分类审查一致；不能因为 codec 是 Bidirectional
  就自动开放两个方向。
- 3 的线上发送者身份来自 SenderBinding，不得由 packet.Player 替换绑定。
- 6、8/区域同步、12、49、129 的状态屏障必须由应用 owner 的提交结果推进；写完最后
  一帧不能推断客户端已经完成同步。
- 14 的 active/inactive 必须只影响当前连接绑定；拒绝客户端激活另一个槽位。
- 93 如果没有独立 platform adapter 和 capability flag，保持拒绝或 opaque。
- 161 不进入普通 handler payload 日志；验证通过后也不能回显 token。

## 4. 实施步骤

1. 建立本类别 registration/authority seam，优先复用现有 NetworkSessionAuthority、
   SessionAdmission、NetworkSessionStage 和 PacketGateway，不复制第二套阶段机。
2. 明确每个 ID 的实际方向和允许阶段，给每条策略设定最大次数、最大字节数和 owner
   超时；无界限的字符串、密码和大世界快照不能沿用默认无限制配置。
3. 为 3、6、12、129 建立强类型 owner 调用结果，明确 Accepted、Rejected、NextStage、
   OutboundDispatch 和 Release binding 的语义。owner 已提交而发送失败时不回滚领域结果。
4. 为 2、4、7、9、49、139 只保留服务端投影输出；不能允许客户端构造这些 S2C 包作为
   C2S 业务输入。
5. 处理连接关闭、超时、迟到 owner 结果、旧 Epoch 和重复 PlayerActive 的竞态。旧连接
   释放后，迟到的 4/7/49/139 输出不能发给新连接。
6. 仅在事实充分时接入 93；否则实现显式禁用和诊断原因，不造一个“成功但什么都没做”的
   handler。
7. 将本类别注册接入现有 host 组合入口，但避免修改其他类别 registration 的职责。

## 5. 明确非目标

不实现玩家移动、库存、Tile、NPC、Projectile 或完整平台社交协议；不把 MessageID 数值
当作 ECS 实体身份；不保存密码/HostToken；不实现真实客户端全流程压力测试；不因为格式
目录存在就宣称业务互通。

## 6. 10% 核心测试

只执行以下最小核心集合，不执行整个 Network Verification Program：

1. Hello → 可选密码 → PlayerInfo → RequestWorldData → PlayerSpawn 的成功阶段链，
   覆盖一次错误阶段拒绝。
2. HostToken/SetCountsAsHostForGameplay 的正确阶段、伪造槽位和 token 不落日志检查，
   覆盖旧 Epoch 输出被丢弃。

优先复用 Test/NSSLC.Infrastructure.Network.Verification 中的
PlayerAdmissionPacketVerification、PlayerSessionLifecycleVerification、
PlayerLifecycleHandlerAcceptanceVerification；若现有 Program 不支持筛选，只新增最小
case 路由或独立 focused verifier，不运行其他 case。构建只针对
Test/NSSLC.Infrastructure.Network.Verification/NSSLC.Infrastructure.Network.Verification.csproj。

## 7. 验收条件和回报格式

必须能列出 16 个 ID 的方向、阶段、handler/拒绝状态；至少有一条成功会话链和一条拒绝
链的命令输出；密码/token 无敏感值日志；构建 exit 0，报告 warning/error 数及
Build/bin/NSSLC.Infrastructure.Network.Verification/Debug/net10.0/ 输出。

回报中另列：仍未开放的 ID、原因、共享文件、与其他 9 分支潜在冲突、未跑的测试和下一步。
失败两次仍无新证据时必须按 PUA skill 换本质不同的排查方向，不得反复改限流数字。
