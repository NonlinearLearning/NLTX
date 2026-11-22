# 服务器诊断日志

本目录由 `NSSLC.Infrastructure.Diagnostics.csproj` 编译，是独立的 .NET 10 基础设施
项目，不是 ECS 领域。原位置 `src/NSSLC/Diagnostics/` 属于共享组件程序集；日志是可观察性
副作用，不属于 ECS 权威状态，因此移到基础设施侧并保持 `Terraria.Diagnostics` 命名空间和
公共 API 不变。

`ServerLogger` 负责校验事件、取得时间并创建不可变的 `ServerLogEntry`；
`IServerLogSink` 是外部日志效果端口；`ConsoleServerLogSink` 保留最小服务器原来的单行
文本格式。

日志是可观察性副作用，不属于 ECS 权威状态，因此这里不建立日志 Component 或 ECS System。
游戏系统可以通过宿主组合入口注入日志端口；当前独立服务器使用 `ServerLog` 的默认控制台
适配器，以保持已有事件调用和诊断输出兼容。

## 依赖方向

本工程不引用任何其他项目，也不引用 ECS 组件、Arch 或世界持久化类型。反向引用来自使用方：
`NSSLC.Server` 与 `NSSLC.Network` 通过 `ServerLog` 输出可观察性事件，它们只消费日志端口，
不通过本工程读写领域状态。
