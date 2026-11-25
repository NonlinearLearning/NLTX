# 错误处理机制选型研究：整数码、类型化返回值与两者的组合

文档 ID：DOC-2026-11-25-ERROR-HANDLING-MECHANISM-RESEARCH  
逻辑域：research  
产物类型：evidence / design analysis  
状态：proposal；外部资料研究与仓库现状对照，不是实施完成或运行验收  
日期：2026-11-25（Asia/Shanghai）  
canonical 路径：`docs/research/2026-11-25-error-handling-mechanism-research.md`

## 1. 问题与结论速览

问题来自一次机制选型讨论：函数失败时，返回整数码（C 传统）还是返回携带 error 的值（Rust `Result` 传统）；
一个系统是否应统一使用一种机制；是否存在能把“码的稳定可判别”与“Result 的载荷能力”结合起来的第三种机制。
本文不讨论语言自带异常机制（用户已排除），异常只作为对照出现。

结论：

1. 整数码与类型化返回值不是两种哲学，而是同一条原则——失败是普通返回值——的两种编码方式。
   真正的取舍点是三个正交维度：**载荷能力**（错误能带多少上下文）、**检查强制力**（忘记处理会不会被发现）、
   **边界稳定性**（编码能否原样穿越网络、磁盘、FFI 与版本升级）。
2. 设想中的“第三种机制”已经存在，它是组合而非新发明：**稳定码（机器判别）+ 结构化载荷（诊断用）+ 分类学
   （severity / facility / retriable）+ 只增不改的演进规则**。工业界有四个成熟样板：SQLite 主码/扩展码、
   Windows HRESULT 位域、gRPC status code + details、HTTP Problem Details（RFC 9457）。
3. “一个系统统一一种机制”是错误的提问方式。应该统一的是**错误模型**（分类学、码空间、语义与转换规则），
   而**编码**按层选择：领域内用类型化结果，系统边界用稳定码，观测层用码作稳定键。
4. 一句话判据：**跨边界（网络 / 磁盘 / FFI / 跨团队 / 日志指标）用码；进程内需要穷尽分支与载荷用类型化结果；
   预期内的业务拒绝永远走返回值；bug 与不变量破坏走 panic / 异常，不算错误处理。**

## 2. 外部资料事实基线

### 2.1 C / POSIX 整数码

- [errno(3)](https://man7.org/linux/man-pages/man3/errno.3.html)：errno 是线程局部的整型全局量，
  “仅在返回值指示失败时才有意义”“成功调用也允许改写 errno”“任何系统调用都不会把它清零”。
  另有少数调用（如 `getpriority`）成功时合法返回 -1，哨兵值与业务值冲突。
- 数值本身跨 UNIX 系统甚至跨架构不同，文本化靠 `strerror` / `perror`。
- [SQLite result codes](https://www.sqlite.org/rescode.html)：错误码是“result codes”的子集
  （非错误码只有 `SQLITE_OK` / `SQLITE_ROW` / `SQLITE_DONE`）；低 8 位是主码（类别），高位是扩展码（细分），
  **主码永远是扩展码的一部分**——整数码同样可以内嵌层级分类学。
- [MS-ERREF：HRESULT](https://learn.microsoft.com/en-us/openspecs/windows_protocols/ms-erref/705fb797-2175-4a90-b5a3-3918024b10b8)：
  32 位按 severity / customer / facility / code 分区，是“码自带分类学”的另一个样本。

### 2.2 Rust `Result` 与 `std::error::Error`

- [`std::result`](https://doc.rust-lang.org/std/result/)：成功/失败是枚举的两个变体，取值必须先匹配；
  `Result` 带 `#[must_use]`，忽略返回值产生编译警告——这是整数码方案做不到的强制力。
- [`std::error::Error`](https://doc.rust-lang.org/std/error/trait.Error.html)：`Debug + Display`，
  提供 `source()`（错误链）与 `downcast_ref`（按具体类型取回载荷）。
- [API Guidelines（C-GOOD-ERR）](https://rust-lang.github.io/api-guidelines/interoperability.html)：
  错误类型应实现 `Error` trait 以便被上层作为 `source` 抽象；明确“永远不要用 `()` 当错误类型”；
  trait 对象建议 `Error + Send + Sync + 'static` 以支持 `downcast_ref`。
- [The Rust PL book ch9-03](https://doc.rust-lang.org/book/ch09-03-to-panic-or-not-to-panic.html)：
  “panic 时无法恢复……返回 `Result` 是把选择权交给调用方”，并给出结论：**定义可能失败的函数时，返回 Result 是默认选择**。
- [BurntSushi: Rust error handling](https://burntsushi.net/rust-error-handling/)：库与应用的错误处理取向不同——
  库倾向具体错误类型，应用顶端倾向 `Box<dyn Error>` / `anyhow` 这类动态上下文链。
- [thiserror](https://docs.rs/thiserror)（枚举错误类型的 derive 生成）与
  [anyhow](https://docs.rs/anyhow)（应用级 context 链）是当前生态的两个默认件。

### 2.3 邻居方案

- Go：error 是携带 `Error() string` 的接口值。
  [Go 1.13 errors](https://go.dev/blog/go1.13-errors) 引入 `%w` 包装与 `errors.Is` / `errors.As`：
  包装保留底层错误用于判等与解包——这正是“码（判别）+ 载荷（上下文）”的接口版实现；
  [`errors`](https://pkg.go.dev/errors) 标准库提供 `As` / `Is` / `Join` / `Unwrap`。
- Zig：[语言参考](https://ziglang.org/documentation/master/)把 error set（纯标签集合）与
  error union（`!T`）作为一等类型，配 `catch` / `try` / `errdefer` 与错误返回轨迹；error 本身不携带载荷。
- C++23：[`std::expected<T,E>`](https://en.cppreference.com/w/cpp/utility/expected) 把“值或错误”变成标准库类型，
  是 Result 在 C++ 的对应物；[C++ Core Guidelines](https://isocpp.github.io/CppCoreGuidelines/CppCoreGuidelines#S-errors)
  则仍以异常为主要通道（E 系列），两者并存说明社区对“预期失败”与“异常失败”的分层共识。
- Swift：[SE-0413 Typed throws](https://github.com/swiftlang/swift-evolution/blob/main/proposals/0413-typed-throws.md)
  （Swift 6.0 实现）让 `throws` 可声明具体错误类型，但默认仍是类型擦除后的 `any Error`；提案自述擦除
  “对大多数代码仍是合适的默认值”，只有必须穷尽处理、或擦除代价过高的窄处才需要精确类型。
  连异常阵营也在“宽处擦除、窄处精确”，与本研究“统一模型、按层选编码”的裁决同向。

### 2.4 跨进程协议层

- [gRPC status codes](https://grpc.io/docs/guides/status-codes/)：“status 对象由整数码与字符串描述组成”，
  码集合有限且部分码规定只允许应用层产生（库层错误与应用层错误可区分）；
  [error handling 指南](https://grpc.io/docs/guides/error-handling/)允许在 status 上附加结构化 details。
- [RFC 9457 Problem Details](https://www.rfc-editor.org/rfc/rfc9457)（取代
  [RFC 7807](https://www.rfc-editor.org/rfc/rfc7807)）：HTTP 错误对象由 `type`（URI 标识的错误类型）、
  `title`、`status`、`detail`、`instance` 加扩展成员组成——“类型标识 + 载荷”的 Web 版。

## 3. 维度对比

| 维度 | 整数码（C 传统） | 类型化返回值（Rust Result / C++ expected / Go error） | 异常（对照） |
| --- | --- | --- | --- |
| 载荷能力 | 无（需 out 参数或全局量，如 errno） | 任意结构化数据；可多诊断聚合 | 任意（异常对象） |
| 忘记检查的后果 | 静默；errno 还可能被后续调用覆盖 | 编译警告（must_use）或根本无法取值（必须 match） | 隐式传播，忘捕获即崩 |
| 机器判别 | 最强：整码可比较、可落日志、可 i18n、可监控告警 | 强：类型/变体匹配，编译期穷尽 | 弱：按类型 catch，易漏 |
| 传播成本 | 每层手写 if + 透传；样板多 | `?` / `try` / `catch` 一符传播 | 零样板，隐式 |
| 性能 | 返回寄存器，零分配 | 成功路径零成本；错误路径构造结构体；大错误类型需装箱 | 抛出路径昂贵（栈展开）；热路径应避免 |
| 跨语言 / ABI / 网络 | 天然可穿越（协议、syscall、FFI） | 不可直接穿越，必须降级为码或序列化 | 不可穿越 |
| 演进兼容 | 追加码即可（需 append-only 纪律 + 未知码降级规则） | 加枚举变体是破坏性变更；需 `#[non_exhaustive]` 或抽象层 | 新异常类型对旧 catch 不可见 |
| 可观测性 | 码即稳定键 | 需要额外映射层才能进日志/指标 | 文本不稳定，难统计 |
| 组合性 | 差：多错误需自造聚合返回值 | 好：错误是一等值，可收集、映射、短路 | 差：一次只能抛一个 |

## 4. 优缺点清单

### 4.1 整数码

优点：

- ABI 与版本稳定：跨语言、跨进程、跨年版本唯一可靠的错误契约。
- 零分配、可位域编码（SQLite / HRESULT 证明整码可承载分类学）。
- 天然适配日志、指标、告警、i18n 文案表：码是稳定键。

缺点：

- 不携带载荷——正是本仓库讨论的痛点；补充上下文只能靠 out 参数、全局量（errno）或第二个调用（`GetLastError`、`sqlite3_errmsg`），
  这些都是新的隐式通道，有线程局部性与时效性问题。
- 无强制检查：C 层面调用方可以无视返回值；errno 甚至“成功调用也可能改写”，检查窗口极窄。
- 语义靠文档约定：新增码没有编译器强迫所有 switch 分支处理。
- 码空间治理成本：分配、冲突、废弃、复用都必须人工纪律。

### 4.2 类型化返回值

优点：

- 载荷丰富且类型安全：错误变体自带上下文（槽位号、实体 id、诊断列表）。
- 强制检查：`#[must_use]` 警告 + match 穷尽性，新增失败原因时所有分支点编译报错，漏网即编译失败。
- 错误是普通值：可存储、排队、聚合、延迟处理——异常做不到的事。
- 领域拒绝（库存不足、槽位满）表达自然，与正常控制流同构。

缺点：

- 类型污染：`Result<T, E>` 会传染整条调用链，E 变体集合膨胀后常退化为 `Box<dyn Error>`，丢失类型信息。
- 库边界不稳定：给公开错误枚举加变体是破坏性变更，必须 `#[non_exhaustive]` 或码化。
- 不可穿越 ABI / 网络：FFI 与协议层必须降级。
- 不表达“部分成功 / 结果未知”，需要额外状态字段。
- 生态依赖：没有 `thiserror` / `anyhow` 这类辅助时样板偏多。

### 4.3 panic / 异常的定位（对照）

panic 与异常表达的是“程序自身出了问题”（不变量破坏、索引越界、第三方库契约破裂），不是可预期的业务结果。
Rust book 的立场是：可能失败的函数默认返回 `Result`，把“是否可恢复”的决定权留给调用方。
把业务拒绝塞进异常（或把 bug 塞进返回值）都会模糊这条线。

## 5. 什么时候用哪种：判据与场景表

按顺序问五个问题：

1. **这是 bug 还是预期失败？** bug → panic / 异常 / 断言，不进入错误处理通道。
2. **失败要穿越边界吗？**（网络、磁盘、FFI、跨进程、跨团队版本）是 → 稳定码 + 可选 detail。
3. **调用方要按失败原因分支吗？** 不分支只要记录 → 布尔 / 码；要分支 → 封闭枚举或类型化变体。
4. **需要载荷吗？**（坐标、槽位、数量、多诊断）需要 → 结构化错误类型；纯分类 → 枚举码。
5. **要收集全部问题吗？**（内容校验场景）→ `Result<T, Vec<Diagnostic>>` 聚合式。

| 场景 | 推荐机制 | 理由 |
| --- | --- | --- |
| 纯 C ABI、驱动、插件 | 整数码 + out 参数 | ABI 唯一稳定契约 |
| 网络协议帧错误 | 码 + 结构化 detail | 码跨语言稳定，detail 供诊断 |
| 存档格式 / 持久化 | 码 + 版本化 detail | 存档寿命超过代码版本 |
| 领域规则拒绝（库存不足、槽位满） | 类型化结果，带载荷 | 需要分支 + 载荷，且在进程内 |
| 内容加载校验 | `Result<T, Vec<Diagnostic>>` | 报出全部问题而非第一个 |
| 配置解析启动失败 | 聚合诊断后统一终止 | 一次修完所有配置错 |
| 解析 / 算术溢出等库函数 | `Option` / `Result` | 预期失败，调用方分支 |
| 不变量破坏（索引越界、空引用） | panic / 异常 | 是 bug，不是业务结果 |
| 第三方 IO / 网络技术失败 | 转稳定类别再上抛 | 不让底层文本泄漏进领域 |
| 遥测 / 告警 / 文案 | 码为稳定键 + detail 为字段 | 码可统计，detail 可读 |

## 6. “统一还是分层”的裁决

**统一错误模型，不统一编码。** 模型包括四件事，全仓库唯一：

1. 分类学：severity（fatal / error / warning / info）、facility（哪一层产生）、retriable 与否。
2. 码空间：谁分配、区间怎么划、废弃码如何处理。
3. 语义：每个码/变体的确切含义与文档位置。
4. 转换规则：层与层之间如何映射，损失什么、保留什么。

编码按层选择：

| 层 | 编码 | 形态 |
| --- | --- | --- |
| L1 领域内 | 类型化错误（枚举 + 载荷） | 穷尽匹配，载荷最全 |
| L2 模块 / 程序集边界 | 封闭枚举（少量类别） | 收敛变体，加 non_exhaustive |
| L3 系统边界（协议 / 磁盘 / FFI） | 稳定整数码 + 可选 detail 块 | append-only，未知码按 facility 降级 |
| L4 观测 / 文案 | 码作稳定键 | 日志、指标、i18n 全走码 |

转换纪律：只允许从内向外（L1→L2→L3）单向有损转换；反向恢复必须查表并校验；
每张跨层映射表用测试锁死——新增 L1 变体而未更新映射时，测试必须失败。

## 7. 码 + 载荷的组合模板

这就是“第三种机制”的落地形态：**类型化变体承载载荷，映射函数产出稳定码，码空间独立治理。**

Rust 形态：

```rust
#[derive(Debug, Clone, PartialEq, Eq)]
#[non_exhaustive]
pub enum InventoryError {
    SlotFull { slot: u16, capacity: u16 },
    MissingDefinition { id: u32 },
    Busy { retry_after_ms: u32 },
}

impl InventoryError {
    /// 稳定码：跨版本不变，用于协议、日志与文案表。
    pub fn code(&self) -> u32 { match self { ... } }
    pub fn severity(&self) -> Severity { ... }
    pub fn retriable(&self) -> bool { match self { InventoryError::Busy { .. } => true, _ => false } }
}
```

码位域方案（二选一或混合）：

- SQLite 式：`code = (primary << 24) | extended`，主码是扩展码前缀，未知扩展按主码处理。
- HRESULT 式：`severity(2) | customer(1) | facility(6) | code(23)`，facility 决定归属层。

无论哪种，都必须写死三条治理规则：码只增不改不复用；未知码按其 facility/主码降级为类别失败；
detail 块自描述版本号，旧读者忽略不认识的扩展。

C# 形态（贴本仓库现有风格）：

```csharp
public readonly record struct InventoryRejection(
    InventoryTransferFailure Reason,   // 判别：封闭枚举
    InventoryErrorDetail? Detail)      // 载荷：可选结构化上下文
{
    public int Code => InventoryErrorCodes.For(Reason);   // 边界映射
}
```

## 8. 对照本仓库现状

仓库已有多处“码 + 载荷”雏形，与本研究的结论方向一致：

| 现有形态 | 位置 | 评价 |
| --- | --- | --- |
| `ProtocolErrorCode` 枚举 + `SendReceipt.Certainty/Epoch/Sequence/ErrorCode` | NSSLC.Network | 网络边界的码 + 收据载荷；构造器强制“失败必带码”，是好的边界形态 |
| `InventoryTransferResult(Succeeded, Quantity, Failure)` + `InventoryTransferFailure` | NSSLC.Share.Inventory | 码 + 载荷 + 数量，领域拒绝的标准形态 |
| `LvcsLoadResult<T>` + `LvcsDiagnostic`（severity/family/id/field/message/source） | NSSLC.Infrastructure.Lvcs | 聚合诊断式，等价 Rust 的 `Result<T, Vec<E>>`；注释明确“内容错误返回而不抛出” |
| `DamageAppliedReceipt` | NSSLC.Share.Combat | 成功路径的富载荷收据 |
| `WorldInteractionResultComponent(Status, string? Failure)` + `WorldInteractionResultStatus` 枚举 | NSSLC.World | **风险点**：状态枚举管“成没成”，失败原因却走自由文本，码的稳定性与类型的穷尽性同时丢失；`Failure` 与 `Status` 形成两套并行语义 |

另两条既有规范条文与本研究的结论同向：副作用隔离规范 §4“可预期的业务拒绝建议使用结果值或明确错误码；
外部依赖失败不应伪装成业务拒绝”；§8“技术异常建议转换为稳定、可测试的错误类别，不要让调用方依赖第三方库的异常文本”。

粗略统计（2026-11-25 实测）：按 `*.cs` 递归统计字面量 `throw new` 出现次数，
`src/NSSLC/Share` 373、`src/NSSLC/Projectile` 124、`src/NSSLC.Infrastructure` 865（其中 `Network` 子目录 547）、
`src/NSSLC` 整项目 604。数字随提交漂移，只用来看通道占比的量级，不作为验收基线。
说明技术异常仍是主要通道，§8 的“转稳定类别”尚缺具体转换点清单；这是后续落地工作的输入，不属于本研究范围。

## 9. 未决问题

- L3 码空间是否与现有 `ProtocolErrorCode` 合并为统一注册表，取决于协议版本策略，需单独评估。
- C# 侧是否引入统一 `Result<T,E>` 泛型，还是沿用各域 result 类型，属于编码风格决策，本文不裁决。
- `WorldInteractionResultComponent.Failure` 的 stringly 失败原因与 `WorldInteractionResultStatus` 的并行语义，建议单开整改轮次处理。

## 10. 参考资料

- Rust：[The Book ch9](https://doc.rust-lang.org/book/ch09-00-error-handling.html) ·
  [ch9-03 panic 或 Result](https://doc.rust-lang.org/book/ch09-03-to-panic-or-not-to-panic.html) ·
  [std::result](https://doc.rust-lang.org/std/result/) ·
  [std::error::Error](https://doc.rust-lang.org/std/error/trait.Error.html) ·
  [API Guidelines](https://rust-lang.github.io/api-guidelines/interoperability.html) ·
  [BurntSushi 长文](https://burntsushi.net/rust-error-handling/) ·
  [thiserror](https://docs.rs/thiserror) · [anyhow](https://docs.rs/anyhow)
- 邻居语言：[Go 1.13 errors](https://go.dev/blog/go1.13-errors) ·
  [pkg.go.dev/errors](https://pkg.go.dev/errors) ·
  [Go error handling and Go](https://go.dev/blog/error-handling-and-go) ·
  [Zig language reference](https://ziglang.org/documentation/master/) ·
  [std::expected](https://en.cppreference.com/w/cpp/utility/expected) ·
  [C++ Core Guidelines](https://isocpp.github.io/CppCoreGuidelines/CppCoreGuidelines#S-errors) ·
  [SE-0413 Typed throws](https://github.com/swiftlang/swift-evolution/blob/main/proposals/0413-typed-throws.md)
- 整数码传统：[errno(3)](https://man7.org/linux/man-pages/man3/errno.3.html) ·
  [SQLite result codes](https://www.sqlite.org/rescode.html) ·
  [MS-ERREF HRESULT](https://learn.microsoft.com/en-us/openspecs/windows_protocols/ms-erref/705fb797-2175-4a90-b5a3-3918024b10b8)
- 协议层：[gRPC status codes](https://grpc.io/docs/guides/status-codes/) ·
  [gRPC error handling](https://grpc.io/docs/guides/error-handling/) ·
  [RFC 9457](https://www.rfc-editor.org/rfc/rfc9457) ·
  [RFC 7807](https://www.rfc-editor.org/rfc/rfc7807)