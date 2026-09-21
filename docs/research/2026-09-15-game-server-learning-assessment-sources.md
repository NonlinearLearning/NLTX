# 游戏服务器学习评估：网络资料与结论映射

**检索日期：** 2026-09-15  
**用途：** 支持 [`迁移项目与既有项目能力评估报告.md`](../migration/assessments/迁移项目与既有项目能力评估报告.md) 的知识缺口判断。  
**资料边界：** 下列资料用于解释通用多人游戏服务器概念，不直接证明 Terraria 1.4.5/1.4.5.6 的具体协议行为。具体协议结论仍须以本地 Version4 源码、抓包和验证程序为准。

## 一手资料

| 资料 | 关键事实 | 对 NLTX 的直接启发 |
| --- | --- | --- |
| [Roblox：Server authority](https://create.roblox.com/docs/projects/server-authority) | 服务器是权威状态源；客户端输入不应被直接当作事实；服务器需要校验输入，客户端预测错误时需要校正。 | `PlayerControls`、位置、伤害、Projectile 生成必须区分“客户端请求”和“服务器提交的状态”。 |
| [Unity：Netcode](https://docs.unity.com/en-us/multiplayer/netcode/netcode) | Netcode for Entities 采用服务器权威模型，并支持客户端预测。 | 学习预测、服务器校正和重模拟，但不把 Unity 的实现细节直接移植到 Terraria 协议。 |
| [Unreal Engine：Networking and Multiplayer](https://dev.epicgames.com/documentation/en-us/unreal-engine/networking-and-multiplayer-in-unreal-engine) | Replication 负责在服务器与客户端之间同步数据和过程调用；复制策略会影响性能和体验。 | 为每类实体定义可复制字段、可见性、全量同步和增量同步边界，并把带宽作为约束。 |
| [RFC 9000：QUIC](https://www.rfc-editor.org/rfc/rfc9000) | QUIC 在 UDP 之上提供加密、多路复用、流控和丢包检测等传输能力。 | 建立可靠/不可靠、顺序/乱序、流控和重传的概念；不能据此直接决定 NLTX 是否改用 QUIC。 |
| [OpenTelemetry：What is OpenTelemetry?](https://opentelemetry.io/docs/what-is-opentelemetry/) | traces、metrics、logs 可以共同描述系统内部状态；OpenTelemetry 本身不是后端存储或展示系统。 | 为 tick 延迟、队列深度、复制字节、拒绝原因、重连和存档恢复建立结构化观测；不要把“接入 OTel”当成监控系统完成。 |
| [.NET：`dotnet-trace`](https://learn.microsoft.com/dotnet/core/diagnostics/dotnet-trace) | 可采集 .NET 运行时 trace，用于分析线程、GC 和方法热点。 | 性能学习必须落实到 P99 tick、GC 暂停、分配和热点，而不是只看平均吞吐。 |
| [.NET：`dotnet-counters`](https://learn.microsoft.com/dotnet/core/diagnostics/dotnet-counters) | 可实时观察 CPU、GC、异常和运行时计数器。 | 在垂直切片压测中记录运行时预算，形成容量和退化判断的基线。 |
| [tModLoader：NPC API](https://docs.tmodloader.net/docs/stable/class_n_p_c.html) | 官方 API 文档暴露 NPC 的运行时属性和行为入口。 | 用于建立 Player/NPC/Projectile 领域词典；不能把 API 暴露字段直接当作 NLTX 的身份根或权威状态。 |

## 资料如何影响本次判断

1. Roblox、Unity 和 Unreal 的资料共同支持“服务器权威、客户端输入不等于服务器状态、复制与预测必须分别建模”这一通用结论。它们不是 Terraria 线协议规范，因此报告只把它们用于学习方向，不用于断言 Terraria 包的字段语义。
2. RFC 9000 用于补齐传输层概念。NLTX 当前主要代码证据仍是 TCP 会话和 Terraria 帧处理；当前优先级是理解时序、队列、可靠性和流控，不是为了追逐协议替换而引入新传输栈。
3. OpenTelemetry 与 .NET 诊断资料把“可观测性”从抽象建议变成可测量项目：每个垂直切片都应保存 tick 分位数、GC、队列和网络输出数据。
4. tModLoader 资料只用于建立 Terraria 实体生命周期词汇，并要求回到 Version4 源码核对 Spawn、AI、Kill、发送和保存的真实顺序。

## 检索限制

本次先进行了网络资料检索，再进行本地代码比对。Brave Search API 因当前环境未配置 `BRAVE_API_KEY` 未能使用，因此采用可直接访问的官方文档和 RFC URL；没有把搜索摘要或无法复核的二手文章作为关键证据。资料访问日期均为 2026-09-15。

