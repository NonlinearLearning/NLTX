# Contracts：各系统共同使用的契约

[返回总览](../README.md)。本目录定义跨系统传递的身份、版本、输入输出及存档数据结构。
Owner 和适配器通过这些契约协作；这里没有玩家、世界或物品的运行时管理循环。

## 主要类型

| 类型 | 表达什么 | 主要使用方 |
| --- | --- | --- |
| [EntityUuid](IdentityContracts.cs) | 一次实体实例的身份根 | Identity、Player、Combat、Mechanisms |
| [SessionEpoch](SessionEpoch.cs) | 一次网络连接的有效期 | Session、Protocol、物品归属 |
| [CommitVersion](CommitVersion.cs) | 已提交状态的版本 | Owner、快照、事务、发送结算 |
| [PacketSequence](PacketSequence.cs) | 入站/出站序列信息 | 连接、认证与重放控制 |
| [InboundPacketEnvelope](InboundPacketEnvelope.cs) | 接收时捕获的连接、序号、阶段和消息上下文 | Host 路由和领域入口 |
| [OwnerCommitResult](OwnerCommitResult.cs) | 提交状态、版本、错误和原因 | 各领域 |
| [OutboundPacketIntent](OutboundPacketIntent.cs) | 已捕获的发送内容、目标及版本 | Producer、连接宿主 |
| [SendReceipt](SendReceipt.cs) | 某次发送尝试的本地结果 | 发送结算 |
| [OutboundDelivery](OutboundDelivery.cs) | 多条输出及其结算接口 | Host、模块和物品同步 |

## 结果如何被系统使用

[OwnerCommitStatus](OwnerCommitStatus.cs) 区分 Applied、Rejected、Stale、Conflict、Unavailable。
调用者应读取具体状态与 Detail：Unavailable 常表示缺少事实或尚未就绪；Conflict 表示捕获后
状态变化；两者都不应被记录为操作已经成功。

[TransportSendCertainty](TransportSendCertainty.cs) 包含 NotSubmitted、Queued、LocallySent、
OutcomeUnknown、Failed。它表达本地传输确定性，不能作为客户端已应用数据的证明。
Settlement 依据领域约定决定确认、保留脏状态或重试。

## 文件契约怎样协作

[Persistence](Persistence/) 内的文件存储、解码、校验和编码接口，将应用协调逻辑与实际
文件实现分开。[WorldData](Persistence/WorldData/) 描述世界文档的各个存档节，
[WorldStorage](WorldStorage/) 包含坐标及图格实体传递结构。

保存流程将领域快照转换成 WorldPersistenceDocument，再交给编码与存储端口；读取流程反向
进行。存档文档、网络 DTO 和权威运行时对象具有不同的有效期与责任，不应互相替代。

## 推荐阅读顺序

先读 OwnerCommitResult 和 InboundPacketEnvelope，再读 OutboundPacketIntent、SendReceipt
和 OutboundDelivery；需要理解加载保存时再进入 Persistence 子目录。
