# 启动服务器时忽略通信版本限制

文档 ID：CR-2026-10-20-IGNORE-COMMUNICATION-VERSION  
逻辑域：change-control  
状态：completed  
日期：2026-10-20

## 1. 原始需求与授权

> 启动服务器忽略通信版本限制

- 提出人：用户
- 渠道：Codex 对话
- 提出日期：2026-10-20
- 分级建议：中度。变更单一接入规则，但影响所有客户端的协议版本准入。
- 决策：按用户对本次会话中直接提出的实现要求，纳入当前工作并立即实施。
- 后续指令原文：

  > 如果你希望我现在运行服务器，请给出要加载的世界文件路径,使用当前的api接口创建世界

- 决策：使用运行时的 `--create-world` 入口，通过现有世界创建 API 在隔离路径生成世界，并启动本地服务器。
- 追加需求原文：

  > 添加简单的服务器日志已排查问题

- 决策：增加控制台诊断日志，覆盖启动和世界加载、TCP 连接、版本准入和连接关闭；不记录密码或普通游戏数据包内容。

## 2. 影响评估

- 数据库/存档格式：不变。
- 架构重构：不涉及。
- 影响功能：服务器 Hello 握手和客户端会话准入。
- 改动范围：生产服务器组合时忽略 Hello 包中的版本字符串；其它会话策略仍可保留版本校验。
- 兼容性风险：不同版本客户端可能在接入后遇到数据包布局或语义不兼容。现有包解码和数据校验继续执行，但忽略版本字段不构成整体协议兼容保证。
- 回滚：在服务器组合创建的 `SessionAdmissionPolicy` 中关闭版本忽略选项即可恢复原有版本拒绝行为。
- 副作用：改变网络接入规则；不增加网络发送、持久化或后台任务。
- 日志副作用：仅写服务器控制台；不新增日志文件、配置或游戏数据持久化。

## 3. 验收范围

- 服务器生产组合下，版本字符串匹配和不匹配的 Hello 包均可继续进入后续准入流程。
- 默认会话策略仍按版本字符串拒绝不匹配的 Hello 包，避免无意改变其它策略消费者。
- 服务器启动后监听 `127.0.0.1:7777`；世界通过配置中的创建流程生成。

## 4. 实施结果

- `SessionAdmissionPolicy` 增加 `IgnoreProtocolVersion` 策略项，默认关闭。
- `SessionAdmissionCoordinator` 只在该选项关闭时执行 Hello 版本字符串比较。
- `ServerComposition` 为生产服务器启用该选项，因此服务器启动后不会因客户端版本标记不同而拒绝其接入。
- 数据包解析和后续会话阶段校验保持原样。
- 创建命令：`dotnet Build/bin/MinimalImplementation.Server/Debug/net10.0/MinimalImplementation.Server.dll --world Build/runtime/IgnoreProtocolVersionServer-2026-10-20/world.wld --create-world --listen-address 127.0.0.1 --listen-port 7777`。
- 创建入口：`P08BServerRuntime` 调用 `WorldCreationCoordinator`、`WorldDocumentSaver` 及现有文件存储适配器创建并加载世界；生成文件为 `Build/runtime/IgnoreProtocolVersionServer-2026-10-20/world.wld`（733 字节）。
- 构建命令：`dotnet build src/MinimalImplementation/MinimalImplementation.Server.csproj --nologo --verbosity minimal`；项目：`src/MinimalImplementation/MinimalImplementation.Server.csproj`；退出码：0；警告：0；错误：0。
- 构建产物：`Build/bin/MinimalImplementation.Server/Debug/net10.0/MinimalImplementation.Server.dll`。
- 启动状态：重新构建并启动后，PID 10820 监听 `127.0.0.1:7777`；操作系统连接表确认该进程持有监听。
- 未执行不同版本客户端的握手测试。

## 5. 服务器诊断日志

- 控制台日志采用本地时间戳、级别和稳定事件名；详情中的字符串使用 JSON 引号并压成单行。
- `Program` 和 `P08BServerRuntime` 记录启动配置、世界创建/加载/发布、启动阻塞、监听与 Tick 启动、运行失败及停止结果。
- `ServerConnectionHost` 记录连接远端地址、会话开启、关闭原因、最后接收序号、发送确定性和连接异常。
- `Version4InboundRouter` 记录被忽略的客户端版本标记及数据包/线协议拒绝原因；不记录密码、host token 或普通数据包内容。
- 最终构建：`dotnet build src/MinimalImplementation/MinimalImplementation.Server.csproj --nologo --verbosity minimal`；退出码：0；警告：0；错误：0；产物位于 `Build/bin/MinimalImplementation.Server/Debug/net10.0/MinimalImplementation.Server.dll`。
- 已使用该产物重启服务器；控制台输出 `server.ready`，并确认 PID 10820 正在监听 `127.0.0.1:7777`。

## 6. 发现阶段故障诊断

- 客户端 `127.0.0.1:50439` 已建立 TCP 连接，服务器记录忽略了 `Terraria326` 与预期 `Terraria319` 的版本标记差异。
- 世界数据请求随后失败：`WorldCreationCoordinator` 将 `WorldFileTreeTopsSection.Empty`（0 项）写入新世界；Packet 7 适配器要求 `TreeTopStyles` 恰好 13 项，因此无法生成世界数据响应。
- 已修正新世界 API，使初始 `world.tree-tops` 节包含 13 个默认变化值。该修正不改变环境节中独立的 `treeStyle` 字段。
- 同一连接还出现 Packet 5/68 无主机路由、Packet 147 缺少完整装备存储的拒绝。绕过版本字符串校验并不能证明 319 与 326 的报文布局兼容；这些记录仍是后续发现阶段/加入流程的风险。
- 修正后的服务器项目构建命令：`dotnet build src/MinimalImplementation/MinimalImplementation.Server.csproj --nologo --verbosity minimal`；退出码：0；警告：0；错误：0；输出位于 `Build/bin/`。
- 已通过当前世界创建 API 在新路径生成 785 字节世界并启动修正后的服务器：`Build/runtime/IgnoreProtocolVersionServer-2026-10-20/world-tree-tops-fixed.wld`；PID 32216 正在监听 `127.0.0.1:7777`。
- 旧客户端连接在服务器重启时以 `LocalShutdown` 关闭，最后接收序号为 358；新进程启动后尚无客户端重连，因此未确认客户端是否已越过发现阶段。

