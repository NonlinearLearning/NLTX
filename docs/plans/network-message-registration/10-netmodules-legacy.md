# 类别 10：NetModules、未知与废弃包执行文档

文档 ID：NETMSG-CAT-10
执行模型：gpt-6-luna，reasoning=max
分支：codex/network-message-10-netmodules-legacy
会话：network-message-10-netmodules-legacy
类别 owner：82 中央二级分派、opaque/legacy 隔离、未知命中诊断

## 1. 目标

建立消息 82 NetModules 的唯一中央注册/准入/二级分派，并把未知、废弃和空语义包置于
可审计的兼容隔离层。通用 MessageBuffer/PacketGateway 不得在没有 module/action 白名单
时执行任意模块行为。未证明有业务语义的包可以被读取、记录有限元数据或转发兼容，但
不得因为“有 codec”就成为 gameplay handler。

## 2. 范围

中央 packet：82 NetModules。

兼容/未知 ID：0 NeverCalled、11 TileFrameSection、15 Unknown15、24 UnusedMeleeStrike、
25 Unused25、26 Unused26、42 Unknown42、44 Unknown44、48 LiquidUpdate、57 Unknown57、
60 Unknown60、62 Unknown62、66 Unknown66、67 Unknown67、68 Unknown68、83 Unused83。

第 9 类提供 82 的具名 UI/社交 adapter，第 5/7/8 类可能消费其中的领域结果，但所有
gateway 注册、module/action key 和默认拒绝策略由本类唯一持有。

## 3. 已知模块约束

复用当前 PacketGateway.ResolveModuleId、Options.UseSteamModuleIds、
Packet82KnownModuleCodecsV4 和已有 module/action policy 校验。逐项核对模块 1..14、
保留/拒绝模块、action 范围、Steam 映射和 C2S/S2C 方向；不能手抄一个与生成定义不一致
的模块枚举。

当前设计已经把 82 视为二级协议容器；本类的交付重点是：

- module type → decoder → adapter → validate/authorize → Command 或只读 UI event；
- module/action 与实际方向、阶段、速率、host 条件绑定；
- 空 body、有未知 module、未知 action、重复注册和 codec/方向冲突均有明确拒绝；
- 第 9 类 adapter 能被注入，但无法绕过中央 policy。

## 4. 实施步骤

1. 读取生成定义、模块 codec、Gateway policy 和已有 SteamModuleVerification，建立中央
   registry 快照；先记录当前哪些模块已存在而非直接全部开放。
2. 设计最小 immutable ModuleDispatchContext，携带 Actor、Session/Epoch、世界代次、
   module/action、方向和原始长度；不携带可直接改 ECS 的回调。
3. 为每个已证明模块接入专用 adapter，分离 world write、player effect、creative/
   teleport、liquid 和 presentation；每个 adapter 负责自己的权限/owner，不在中央 switch
   堆积领域规则。
4. 为未知/废弃 ID 建立 OpaqueCompatibilityHandler 或等价隔离，限制长度/频率，记录
   message id、方向、长度和拒绝原因，不记录密码/token/完整敏感 payload。
5. 对 0/11/15/24/25/26/42/44/48/57/60/62/66/67/68/83 保持默认不产生 ECS 写入；
   若发现真实命中，只补证据和最小兼容路径，不擅自赋予新语义。
6. 暴露只读 registration snapshot 供主会话验收，能证明 82 只有一个 gateway key。

## 5. 非目标

不实现每个模块的完整 gameplay 行为、不创建未知包的 ECS 组件、不开放公网 DevCommands、
不替换生成器、不复制其他类别 owner；没有事实的模块维持 disabled/opaque。

## 6. 10% 核心测试

只执行两组：

1. 一个已证明 module/action 的成功解码、策略准入和 adapter 调用；同一模块的未知 action、
   错方向或无权限输入拒绝。
2. 一个未知/废弃 ID 的 bounded opaque/拒绝路径，验证长度、频率、诊断元数据和零领域
   写入；再验证重复 packet-82 registration 在启动时失败。

优先使用 SteamModuleVerification、GatewayPolicyVerification、PocketWireVerification 或
新增的 focused module cases；不运行全量网络验证。构建只针对受影响的 Network 项目/其
verifier，使用 --no-build --no-restore 运行选定 case。

## 7. 验收

回报必须附中央 module registry 快照、82 唯一 key 证据、已开放和禁用模块表、未知包
处理表、正向/负向测试输出、命令/exit code/warning/error/Build/bin、敏感数据审计和
与第 5/7/9 类的合并冲突点。任何未知包触发 ECS 写入、任何重复 82 key 或任何无 action
白名单的通用执行路径都属于 failed。

