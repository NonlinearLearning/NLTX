# 函数式编程中的副作用隔离：一手资料研究笔记

## 研究目的

为编写“如何在非函数式（命令式/面向对象）代码中隔离副作用”的团队规范，收集可追溯的一手资料，
并把资料中的概念转换成可执行的工程规则。本文只记录来源和推论；规范正文见
[`约束/非函数式编码副作用隔离规范.md`](../../../约束/非函数式编码副作用隔离规范.md)。

## 来源与关键事实

| 来源 | 一手事实 | 对规范的启示 |
| --- | --- | --- |
| [Microsoft Learn: Functional Programming Concepts in F#](https://learn.microsoft.com/en-us/dotnet/fsharp/introduction-to-functional-programming/) | 函数式编程强调函数、不可变数据、表达式而非语句，以及声明式编程。 | 把业务计算写成输入到输出的显式变换；默认不共享可变状态。 |
| [Microsoft Learn: Functions (F#)](https://learn.microsoft.com/en-us/dotnet/fsharp/language-reference/functions/) | 函数是程序执行的基本单元；函数可作为值、组合并进行部分应用。 | 通过小函数组合业务规则，避免把 I/O、状态写入和计算揉成一个大方法。 |
| [Elm Guide: Commands and Subscriptions](https://guide.elm-lang.org/effects/) | Elm 将 DOM 操作留给运行时；HTTP、随机数、时间等外部交互通过 `Cmd`/`Sub` 描述，再由运行时执行。 | 效果应先被表示为值或命令，再在边界统一解释；核心逻辑不直接调用外部设备。 |
| [Alistair Cockburn: The Original Hexagonal Architecture paper](https://alistair.cockburn.us/hexagonal-architecture/) | 应用应能在没有 UI 或数据库时运行自动化回归测试；端口接收语义操作，适配器转换技术细节。 | 定义端口接口，把数据库、网络、时钟、文件系统放进适配器；测试使用内存适配器。 |
| [Microsoft Learn: .NET dependency injection](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection) | .NET 内置支持 DI；依赖注入用于在类之间实现控制反转，依赖应通过构造函数等方式提供。 | 禁止业务类自行 `new` 基础设施；通过构造函数注入可替换的端口。 |
| [Microsoft Learn: Deferred execution and lazy evaluation](https://learn.microsoft.com/en-us/dotnet/standard/linq/deferred-execution-lazy-evaluation) | LINQ 查询通常延迟执行，表达式在枚举或需要结果时才求值。 | 纯查询与执行时机必须明确；不要在查询表达式中藏写入、网络调用或依赖枚举次数的副作用。 |
| [Destroy All Software: Functional Core, Imperative Shell](https://www.destroyallsoftware.com/screencasts/catalog/functional-core-imperative-shell) | 纯函数核心只依赖参数和返回值；命令式壳层处理 stdin/stdout、数据库、网络，并根据核心产出的值行动。 | 采用“纯核心 + 薄壳层”：壳层负责采集输入、解释命令、提交副作用和记录失败。 |

## 综合结论

1. **隔离不是消灭副作用。** 真实系统必须读写数据库、网络、时钟和日志；目标是让副作用集中在少数可见边界，
   而不是散落在每个业务分支。
2. **先计算，后提交。** 业务函数先返回值、决策或命令列表；提交器按明确顺序解释这些结果。这样可以单测、重放、
   审计，并在提交失败时定义补偿策略。
3. **依赖显式化。** 时钟、随机数、ID 生成器、文件、网络客户端都通过端口注入；禁止隐式读取静态全局对象。
4. **副作用要有分类和顺序。** 持久化、消息发布、缓存失效、日志、指标、UI/音频等效果应分类；规范必须说明哪些效果
   是权威状态、哪些只是通知，并规定提交顺序和重试语义。
5. **延迟执行是风险点。** `IEnumerable<T>`、LINQ、事件回调和异步任务可能把执行推迟到边界之外；查询对象不得携带
   写入行为，必要时在边界物化（如 `ToArray()`）并记录一次性执行。

## 适用范围与非目标

- 适用于 C# 命令式、面向对象和 ECS 系统；不要求引入纯函数语言、Monad 或特定效果库。
- 不要求所有代码都无状态；允许在明确的聚合/组件 owner 内修改状态，但修改必须集中并可追踪。
- 不替代事务、并发、可靠消息和安全规范；这些主题只在副作用边界处标出接口要求。

研究访问日期：2026-09-02。
