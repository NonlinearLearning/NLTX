# 服务器诊断日志

本目录提供服务器日志的稳定契约和控制台适配器。`ServerLogger` 负责校验事件、取得时间
并创建不可变的 `ServerLogEntry`；`IServerLogSink` 是外部日志效果端口；
`ConsoleServerLogSink` 保留最小服务器原来的单行文本格式。

日志是可观察性副作用，不属于 ECS 权威状态，因此这里不建立日志 Component 或 ECS System。
游戏系统可以通过宿主组合入口注入日志端口；当前独立服务器使用 `ServerLog` 的默认控制台
适配器，以保持已有事件调用和诊断输出兼容。
