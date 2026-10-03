# 网络网关与消息读写设计入口

日期：2026-10-03（Asia/Shanghai）。状态：已实现网络基础设施；独立 AI 57 组验证通过，构建 0 警告/0 错误。

前轮只交付设计文档；用户后续明确要求按报告实现，并将测试交给其他 AI。
生产代码位于本目录上一级 Network，应用端口位于 NSSLC.Application/Network；
未写入或引用分类参考。历史设计提案保留调查时的事实，最终行为及验证以实施报告为准。

## 阅读顺序

1. [领域术语](CONTEXT.md)：区分协议配置、消息、玩家槽位、连接代次和游戏会话。
2. [网络领域模型与网关职责](2026-10-03-network-domain-and-gateway.md)：关系、握手状态机、拦截及转发。
3. [消息读写 API 与文件布局](2026-10-03-packet-io-api.md)：普通 packet 类型桥接、完整帧、读写完成语义和生成代码接入。
4. [缓冲、发送、连接与重试](2026-10-03-buffer-connection-retry.md)：内存拥有期、背压、慢连接、取消和断线恢复。
5. [全部 162 个消息的处理矩阵](D:/ProjectItem/SourceCode/Net/NetWork/docs/plans/2026-10-03-network-special-packet-matrix.md)：逐 ID 源码依据、建议 owner 和开放限制；另列包 82 的 15 个模块。
6. [NetCoreServer 网络研究](D:/ProjectItem/SourceCode/Net/NetWork/docs/research/2026-10-03-netcoreserver-gateway-evidence.md)：版本证据、官方源码、.NET 缓冲和重试资料。
7. [调查与交付报告](D:/ProjectItem/SourceCode/Net/NetWork/docs/reports/2026-10-03-network-gateway-design-report.md)：结论、证据限制、交付清单、实施顺序与验收目标。
8. [实施报告](2026-10-03-network-gateway-implementation.md)：实际实现范围、验证与限制。
9. [宿主接入说明](2026-10-03-network-host-integration.md)：事实表、owner 端口、启动、缓存与重连。

## 本轮基线

保留 NetCoreServer 8.0.7 为适配目标，不进行库迁移；以服务端进程内的权威网络入口为主方案。独立 TCP 代理涉及上游连接对、玩家槽位映射和端到端恢复，是另一个部署模式，本文没有假设它已经存在。

格式编译器继续只处理 wire 数据。`MessageID` 的实际定义是普通类中的 `const byte` 字段；0–161 为编号槽位，162 是 Count。当前 153 个结构化 ID、9 个 opaque ID 不等同于 162 个可开放的业务处理器。

已实现的公开调用为 `ReadPacketAsync()` 返回消息封套、`WritePacketAsync<TPacket>(packet)` 接受当前普通 packet 值，并等待本地发送完成。它们不要求 packet 实现历史原型的 `INetPacket`。实际错误和取消契约详见实施报告，原 API 文档保留设计依据。

正式工程：[NSSLC.Infrastructure.Network.csproj](../NSSLC.Infrastructure.Network.csproj)。
格式目录不自动开放消息；域处理器需由宿主明确注册。真实客户端、游戏 owner 迁移及
工作集/吞吐测试不由完整格式目录推导为完成。
