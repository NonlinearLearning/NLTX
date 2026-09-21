# 项目代码规范主题化 Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** 将副作用隔离规范改造成 `约束/项目代码规范/` 下的 10 个主题文档，并让 `Context/约束/非函数式编码副作用隔离规范.md` 成为 AI 可执行的唯一入口。

**Architecture:** 入口文档只保存总原则、加载协议、任务映射、优先级和最低门禁；主题文档按边界、纯计算、依赖、状态、ECS、command、一致性、异步和验证拆分。主题文档使用统一结构，并通过相对链接互相引用；不修改源代码。

**Tech Stack:** Markdown、PowerShell 基础检查、现有仓库文档目录约定。

---

### Task 1: 建立项目代码规范目录和入口迁移骨架

**Files:**
- Create: `约束/项目代码规范/`
- Modify: `Context/约束/非函数式编码副作用隔离规范.md`

**Step 1: 读取现有入口和相关仓库约束**

确认现有入口中的原则、链接和文件命名约定；不得覆盖用户已有的无关修改。

**Step 2: 写入口文档**

保留目标、状态和适用范围，加入：AI 加载协议、主题索引、任务映射、风险等级、优先级、冲突处理、最低规则和规范变更流程。入口不得重复主题正文。

**Step 3: 运行入口链接和结构检查**

Run:

```powershell
Test-Path 'Context/约束/非函数式编码副作用隔离规范.md'
Select-String -Path 'Context/约束/非函数式编码副作用隔离规范.md' -Pattern 'AI 加载协议|主题索引|高风险|冲突'
```

Expected: 文件存在，四类入口字段均出现。

### Task 2: 编写加载协议主题

**Files:**
- Create: `约束/项目代码规范/00-规范加载协议.md`

**Step 1: 定义 AI 加载顺序**

写明“先入口、再主题”，输出已加载主题、适用规则、预计副作用、owner、顺序和失败策略。

**Step 2: 定义任务映射和全量加载条件**

覆盖 Domain、Application、Component、System、tick/回放、网络/持久化、适配器、诊断、迁移和未分类任务。

**Step 3: 定义冲突和停止条件**

写明规则优先级、两个“必须”冲突时的停止流程，以及未分类副作用按最高风险处理。

### Task 3: 编写作用域与副作用分类主题

**Files:**
- Create: `约束/项目代码规范/01-作用域与副作用分类.md`

**Step 1: 定义三档作用域**

分别规定 `Domain/Policy/Query`、`Application/System`、`Adapters/Infrastructure` 的职责、允许依赖和禁止行为。

**Step 2: 建立副作用分类矩阵**

覆盖权威状态、持久化、可靠消息、进程内 command、缓存、日志指标、UI/VFX、时钟、随机数和线程调度，并说明可靠性、丢失和顺序属性。

**Step 3: 加入反例和评审清单**

至少包含业务规则直接访问数据库、查询方法写缓存、适配器承载业务规则三个反例。

### Task 4: 编写纯计算与功能核心主题

**Files:**
- Create: `约束/项目代码规范/02-纯计算与功能核心.md`

**Step 1: 规定显式输入和不可变输出**

说明纯函数、Decision、Plan、Command、Domain Event 和功能核心/命令式壳层。

**Step 2: 规定时间、随机数和 ID 传递**

Application/System 从端口取得值，Domain 只接收值；随机规则写明 seed/state、消费顺序和回放契约。

**Step 3: 加入 C# 规则示例**

提供纯策略、带副作用反例和“读取 → 决策 → 提交”的改写。

### Task 5: 编写端口适配器与依赖注入主题

**Files:**
- Create: `约束/项目代码规范/03-端口适配器与依赖注入.md`

**Step 1: 定义最小端口和能力分离**

区分 reader、writer、publisher、clock、random 和 ID generator，禁止使用同时读写发布的大接口绕过边界。

**Step 2: 规定构造函数注入和组合根**

说明具体 SDK 只能在 Adapter，组合根负责注册和生命周期；业务类禁止自行创建基础设施。

**Step 3: 加入生命周期和测试替身要求**

覆盖内存实现、契约测试、资源释放、连接池和配置注入。

### Task 6: 编写状态所有权与不可变快照主题

**Files:**
- Create: `约束/项目代码规范/04-状态所有权与不可变快照.md`

**Step 1: 定义唯一 owner**

规定字段、集合、组件和聚合的唯一写入者、读写权限和别名禁止规则。

**Step 2: 定义快照和深度不可变性**

说明只读接口不等于不可变、复制时机、集合元素、`ComponentState`/`ComponentSnapshot` 和对象池例外。

**Step 3: 加入并发与生命周期契约**

覆盖跨线程共享、可见性、结构化共享、零拷贝和快照过期。

### Task 7: 编写 ECS 读写集与系统阶段主题

**Files:**
- Create: `约束/项目代码规范/05-ECS读写集与系统阶段.md`

**Step 1: 定义 ReadSet/WriteSet**

要求每个 System 声明读写组件集，同阶段写集不得相交，跨阶段冲突改用 command。

**Step 2: 定义阶段和调度顺序**

覆盖 Input、Simulation、Validate/Plan、Commit、Publish 等阶段，禁止依赖注册顺序和线程时序。

**Step 3: 定义 Simulation 与 Bridge 边界**

网络、持久化和呈现只消费快照/事件，不回写 Simulation；加入 ECS 读写集示例和检查清单。

### Task 8: 编写 Command 队列与确定性提交主题

**Files:**
- Create: `约束/项目代码规范/06-Command队列与确定性提交.md`

**Step 1: 定义 command envelope**

包含 `CommandId`、`SemanticKey`、实体/聚合 ID、tick、phase、systemOrder、producerSequence、可靠性和重复策略。

**Step 2: 定义稳定排序和队列溢出**

采用 `(tick, phase, systemOrder, producerSequence)`；按权威状态、可合并快照、呈现/诊断分类处理溢出。

**Step 3: 定义消费、幂等和两阶段提交**

覆盖按 command 隔离、按一致性组控制、重复处理、`Validate/Plan → Apply`、fault/quarantine 和跨实体补偿。

**Step 4: 定义确定性级别**

区分权威模拟与普通业务，规定稳定遍历、固定随机流、浮点聚合和版本化。

### Task 9: 编写持久化、消息一致性与 Outbox 主题

**Files:**
- Create: `约束/项目代码规范/07-持久化消息一致性与Outbox.md`

**Step 1: 定义三类效果**

区分可靠业务消息、进程内 ECS command、呈现/诊断效果，并标明持久化、重复、丢失、顺序和延迟属性。

**Step 2: 规定一致性模式**

说明 Outbox 默认、同事务模式和显式最终一致模式的适用条件、失败路径和恢复方式。

**Step 3: 规定幂等和观测字段**

要求命令 ID、语义键、聚合 ID、投递语义、消费者去重和审计状态。

### Task 10: 编写异步、取消、异常与重试主题

**Files:**
- Create: `约束/项目代码规范/08-异步取消异常与重试.md`

**Step 1: 区分同步纯计算和异步 I/O**

Domain 默认同步；外部 I/O 端口必须异步并接受 `CancellationToken`。

**Step 2: 定义取消边界**

规定令牌传播、已提交副作用不可撤回、取消后不得开始后续 command，以及资源生命周期。

**Step 3: 定义异常分类和重试**

禁止吞异常；要求幂等键、重试上限、退避、不可重试错误、隔离和后台任务可追踪性。

### Task 11: 编写测试、静态检查、迁移与例外主题

**Files:**
- Create: `约束/项目代码规范/09-测试静态检查迁移与例外.md`

**Step 1: 定义测试层次**

覆盖纯核心、端口契约、应用编排、ECS 确定性、适配器故障和回放测试。

**Step 2: 定义静态门禁**

覆盖程序集依赖、纯度 API、ReadSet/WriteSet、command 字段、空异常捕获和 IQueryable 泄漏。

**Step 3: 定义 Level 0-4 实施策略**

规定登记、新代码门禁、修改触及即治理、关键路径强制和存量收敛。

**Step 4: 定义正式例外记录**

包含编号、路径、放宽规则、原因、风险、owner、补偿控制、测试、创建/复审/到期时间和状态。

### Task 12: 交叉引用、链接和 Markdown 验收

**Files:**
- Modify: `Context/约束/非函数式编码副作用隔离规范.md`
- Verify: `约束/项目代码规范/*.md`

**Step 1: 检查主题文件是否齐全**

Run:

```powershell
$expected = 0..9 | ForEach-Object { '{0:D2}' -f $_ }
$actual = Get-ChildItem '约束/项目代码规范' -Filter '*.md' | ForEach-Object { [regex]::Match($_.Name, '^\d{2}').Value }
Compare-Object $expected $actual
```

Expected: 无差异。

**Step 2: 检查代码围栏和本地链接**

统计每个文件的 ` ``` ` 数量，必须为偶数；提取相对链接并确认目标存在。

**Step 3: 检查入口去重和主题映射**

确认入口有任务映射、全量加载条件和优先级；主题正文不被复制回入口。

**Step 4: 记录文档验收结果**

在最终回复中报告文件清单、链接/围栏检查结果，以及未运行 .NET 构建的原因（本次仅文档变更）。

