# Version4 Component-only Design 会话提示词

## 0. 直接执行说明

你是当前会话的唯一执行者。收到本文件后直接执行任务，不要求用户再填写变量、补充提示词或选择输入文件。

本会话**禁止使用子代理**，禁止调用 `spawn_agent`、创建其他会话、委托并行子任务或让其他 AI 代为读取、分析和写入文件。所有文件读取、证据核对、设计整理和 Markdown 写入必须由当前会话完成。

本任务只生成 **Component-only Design**。除组件组成设计所必需的字段、状态所有权、生命周期、组合关系和证据外，不讨论 System、Query 或其他运行时结构。

---

## 1. 固定工作区和输出规则

仓库根目录：

```text
D:\TRbackup\NLTX
```

研究报告目录：

```text
D:\TRbackup\NLTX\docs\research
```

设计输出目录：

```text
D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\design
```

### 1.1 只使用当前会话产生的研究报告

本任务不得扫描报告目录、不得按修改时间选择最新文件、不得猜测输入报告、不得把其他会话的报告当作当前会话产物。

只允许使用“当前会话已经明确产生或明确提供”的研究报告 Markdown。报告可以位于当前会话指定的任意有效路径，文件名和目录不再强制匹配固定模式。

按以下规则处理：

1. 检查当前会话的用户消息、上文任务信息和当前会话已经产生的文件结果；
2. 找到一个明确的、存在的、由当前会话产生或明确指定的研究报告路径后，使用该文件；
3. 只读取该报告，不扫描 `D:\TRbackup\NLTX\docs\research` 中的其他报告；
4. 读取报告开头和元数据，确认 `subsystemId`、`taskNumber` 和 `reportPath`；
5. 如果报告中的 `reportPath` 与实际文件路径不一致，以实际存在的当前会话报告路径为准，并在输出文档中记录 `evidence-mismatch`；
6. 如果当前会话没有明确产生或提供研究报告，必须立即停止，不得生成设计文件，并向用户询问：

   ```text
   当前会话没有明确的研究报告 Markdown。请提供本会话产生的报告路径。
   ```

在用户提供报告路径之前，不得继续读取其他研究报告，不得选择替代报告，不得创建 `docs\component-decomposition\review-round-1\design` 文件。

### 1.2 自动推导设计文件路径

输出文件统一写入：

```text
D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\design
```

推导规则：

1. 使用当前会话报告的文件名作为基础名称；
2. 如果文件名以 `-public-decomposition.md` 结尾，将该后缀替换为 `-component-design.md`；
3. 如果文件名不是以 `-public-decomposition.md` 结尾，将 `.md` 扩展名替换为 `-component-design.md`；
4. 将推导出的文件写入 `D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\design`，不沿用输入报告的目录；
5. 不覆盖原研究报告；
6. 如果 `docs\component-decomposition\review-round-1\design` 不存在，可以创建该目录；
7. 只允许写入推导出的这一份设计 Markdown。

例如：

```text
输入：
D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-05-version4-liquid-simulation-public-decomposition.md

输出：
D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\design\2026-09-05-version4-liquid-simulation-component-design.md
```

如果当前会话报告为：

```text
D:\TRbackup\NLTX\docs\research\liquid-simulation-review.md
```

则输出为：

```text
D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\design\liquid-simulation-review-component-design.md
```

---

## 2. 必须读取的材料

定位研究报告后，必须读取以下材料。不得只依赖研究报告中的摘要或旧行号。

### 2.1 当前任务材料

读取：

```text
D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\design\2026-09-05-version4-public-decomposition-common-protocol.md
```

根据报告中的 `taskNumber` 和 `subsystemId`，尝试读取对应的子系统任务提示词：

```text
D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-05-version4-<slug>-public-decomposition.md
```

如果该任务提示词不存在，记录 `evidence-gap`，但继续使用公共协议和研究报告执行组件整理。

### 2.2 public-decomposition 证据规则

读取：

```text
D:\TRbackup\NLTX\.agents\skills\public-decomposition\SKILL.md
D:\TRbackup\NLTX\.agents\skills\public-decomposition\references\ecs-evidence-protocol.md
D:\TRbackup\NLTX\.agents\skills\public-decomposition\references\ecs-decomposition-patterns.md
D:\TRbackup\NLTX\.agents\skills\public-decomposition\references\tmodloader-documentation-retrieval.md
```

使用这些材料的成员盘点、状态所有权、组件粒度和证据规则，但将本任务的最终输出严格限制为 Component-only Design。

### 2.3 参考项目

按与当前子系统直接相关的最小范围只读检查：

```text
D:\TRbackup\Version4
D:\TRbackup\无任何删减通过编译
D:\TRbackup\tmodloader-api-docs-stable
C:\Users\shan\Downloads\ECS\space-station-14-master
```

### 2.4 当前 NLTX

只读检查当前实现与报告涉及的组件：

```text
D:\TRbackup\NLTX\src
D:\TRbackup\NLTX\Test
D:\TRbackup\NLTX\dome\src
D:\TRbackup\NLTX\dome\Test
```

---

## 3. 任务目标

将当前研究报告整理为一份可以供架构评审使用的 **Component-only Design Markdown**。

设计文档只回答以下问题：

1. 该子系统有哪些 Component；
2. 每个 Component 保存哪些状态；
3. 每个字段的类型、默认值和不变量是什么；
4. 字段是权威状态、派生值、缓存、快照还是兼容字段；
5. 哪些成员必须放在一起；
6. 哪些成员必须拆开；
7. Component 属于单个实体、实体集合、Tile、World、Section 还是其他明确的状态范围；
8. Component 的创建、初始化、更新和清理生命周期是什么；
9. Component 与 Entity 的组合关系是什么；
10. Entity ID、持久化 ID、网络 ID 和外部 ID 如何归类；
11. 当前 NLTX 已有组件与目标组件如何映射；
12. 哪些组件归属仍然需要整合裁决。

最终结果必须是设计文档，而不是研究摘要、代码实现或迁移计划。

---

## 4. 允许写入设计文档的内容

可以写入以下内容：

- Component 名称；
- Component 稳定 ID；
- Component 职责；
- Component 的实体范围或世界范围；
- Component 字段；
- 字段类型和默认值；
- 字段不变量；
- 权威状态、派生值、缓存、快照和兼容字段分类；
- Component 生命周期；
- Entity 与 Component 的组合关系；
- Component 之间的组合约束；
- ID 和关系字段的归属；
- 组件拆分理由；
- 组件合并理由；
- 不应单独创建 Component 的对象；
- 当前 NLTX 已有组件及其覆盖情况；
- 组件级 evidence-gap；
- 组件 owner 尚未确定时的候选 owner；
- 最终 Component 清单。

如果必须引用方法、调用点或外部边界，只能把它们作为字段证据简短列出，不得展开为运行时结构设计。

---

## 5. 严格禁止的内容

新的设计 Markdown 不得包含以下设计内容：

- System；
- Query；
- Command；
- Event；
- Adapter；
- Projection；
- Port；
- System 顺序；
- 调度阶段；
- 调用图；
- 主循环设计；
- 网络发送流程；
- 存档流程；
- 客户端渲染流程；
- focused verifier；
- 测试计划；
- 迁移计划；
- 实现步骤；
- `.cs` 文件创建计划；
- `.csproj` 修改计划；
- 可运行 C# 代码；
- 完整类实现；
- 其他子系统的完整设计。

如果原研究报告包含这些内容，只能忽略，不得复制到新的 Component-only Design 中。

以下词语不得作为新的设计对象出现：

```text
System
Query
Command
Event
Adapter
Projection
Coordinator
Pipeline
Scheduler
Verifier
```

源码事实中出现的类名或方法名可以作为证据路径出现，但不能把它们转换为新的运行时设计章节。

---

## 6. 证据和状态规则

### 6.1 Version4 优先级

`D:\TRbackup\Version4` 是真实行为和覆盖范围的首要基线。

完整参考源码只能补充 Version4 中已经存在文件的明确成员缺口。完整参考源码独有的成员不得写成 Version4 当前事实。

### 6.2 行号复核

研究报告和任务提示词中的行号只是检索线索。必须重新定位实际：

- 类型；
- 字段；
- 属性；
- 方法；
- 初始化路径；
- 生命周期；
- 读写位置。

如果旧路径或旧行号不正确，必须记录：

```text
evidence-mismatch
```

或：

```text
version-drift
```

不得静默修正并假装旧证据仍然有效。

### 6.3 tModLoader

`D:\TRbackup\tmodloader-api-docs-stable` 只用于交叉核对公开字段语义、公开生命周期和公开边界。

必须使用实际页面路径和成员锚点。不得使用 tModLoader 文档替代 Version4 私有实现证据。

### 6.4 Space Station 14

Space Station 14 只用于参考 Component 粒度、Entity 与 Component 组合、状态内聚和 ID 分离。

禁止复制其代码、名称、目录结构或领域语义。

### 6.5 当前 NLTX

当前 NLTX 中已经存在的组件必须标记：

```text
status: existing
```

或：

```text
status: partial
```

新提出但尚未创建的组件必须标记：

```text
status: proposed
```

不得因为研究报告曾经写过 `missing`，就忽略当前实际源码。

### 6.6 跨子系统 owner

跨两个或以上子系统使用的字段、类型、ID 或关系不得在当前文档中宣布最终 owner，必须写：

```text
crossSubsystemOwner: integration-review
```

### 6.7 证据不足

如果关键字段的类型、含义、owner 或生命周期无法确认：

- 不得猜测；
- 不得补造默认值；
- 不得声称设计已经锁定；
- 必须写入 `evidence-gap`；
- `designStatus` 必须为 `candidate` 或 `decision-required`；
- 不得使用 `baseline`。

本会话不得向用户追加提问。无法确认的事项直接标记为 `unresolved` 或 `decision-required`，并继续生成候选设计。

---

## 7. Component 拆分判断标准

判断字段是否属于同一个 Component 时，必须检查：

1. 是否表达同一个内聚概念；
2. 是否具有相同生命周期；
3. 是否具有相同权威 owner；
4. 是否共同创建和销毁；
5. 是否共同恢复或持久化；
6. 是否共享相同的不变量；
7. 拆开后是否会产生隐含同步要求；
8. 合并后是否会形成巨型 Component；
9. 是否存在只需要其中一部分字段的实体；
10. 是否会把表现、网络、日志或外部对象混入权威状态。

优先拆分：

- 生命周期不同；
- owner 不同；
- 权威事实和缓存混合；
- 实体状态和世界状态混合；
- 表现状态和模拟状态混合；
- 需要不同字段子集的实体不同；
- 字段失效条件不同。

优先合并：

- 字段永远共同存在；
- 字段共同创建、更新和清理；
- 字段共同维护一个不可分割的不变量；
- 分开后只会产生重复镜像和脆弱同步关系。

---

## 8. 输出文件固定结构

输出文件必须使用以下结构，不得增加运行时设计章节。

```markdown
# <SubsystemId> Component Design

## 1. 设计元数据

## 2. 设计范围与排除范围

## 3. 组件设计依据

## 4. Version4 成员到 Component 归属表

## 5. Component 定义

### 5.1 <ComponentName>

#### 职责

#### 字段

#### 字段不变量

#### 生命周期

#### Entity/World 范围

#### ID 与关系字段

#### 当前 NLTX 映射

#### 证据

## 6. Entity 与 Component 组合

## 7. 组件拆分与合并决策

## 8. 不单独创建 Component 的对象

## 9. 当前 NLTX 组件覆盖

## 10. 组件级 evidence-gap

## 11. 未决组件 owner

## 12. 最终 Component 清单

## 13. 最终声明
```

### 8.1 设计元数据

必须包含：

```text
subsystemId:
taskNumber:
sourceReport:
outputDesignPath:
designScope: component-only
designStatus:
evidenceStatus:
nltxStatus:
verificationStatus: not-run
selectionMethod:
```

`designStatus` 只能使用：

```text
candidate
decision-required
baseline
```

未解决组件 owner、字段语义或关键生命周期冲突时，不得使用 `baseline`。

### 8.2 成员到 Component 归属表

尽可能按真实成员列出：

| 成员 | 声明类型 | 字段含义 | 权威/派生/缓存/快照/兼容 | 生命周期 | proposed Component | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|---|

不得用“核心状态”“行为状态”等抽象词替代所有真实成员。

### 8.3 Component 定义

每个 Component 必须明确：

```text
componentId:
name:
status:
componentOwner:
crossSubsystemOwner:
entityScope:
lifecycle:
```

字段表必须包含：

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|

### 8.4 Entity 与 Component 组合

使用以下表格：

| Entity/World 对象 | 必需 Component | 可选 Component | 互斥 Component | 组合理由 |
|---|---|---|---|---|

不得在这一节描述 System、Query 或执行顺序。

### 8.5 未决组件 owner

每个未决事项使用稳定 ID，例如：

```text
BD-COMP-01
BD-COMP-02
```

每项必须说明：

- 冲突字段；
- 当前候选 owner；
- 各候选方案会怎样改变 Component 组成；
- 为什么当前会话不能宣布最终 owner。

---

## 9. 文件和执行限制

只允许写入自动推导的：

```text
<OutputDesignPath>
```

不得修改：

- Version4 源码；
- 完整参考源码；
- tModLoader 文档；
- Space Station 14；
- 当前 NLTX 的 `src`、`Test`、`dome\src`、`dome\Test`；
- 原研究报告；
- 公共协议；
- 子系统任务提示词；
- 其他设计文档。

不得运行：

- `dotnet restore`；
- `dotnet build`；
- `dotnet test`；
- `dotnet run`；
- `dotnet publish`；
- `dotnet pack`；
- `dotnet msbuild`；
- 其他编译型命令。

不得创建 `.cs`、`.csproj`、测试文件或运行时实现。

---

## 10. 完成条件

完成时必须已经：

1. 从当前会话明确产生或明确提供的路径定位一个研究报告；
2. 自动推导唯一输出路径；
3. 重新核对与 Component 字段直接相关的证据；
4. 生成 Component-only Design Markdown；
5. 保留原研究报告不变；
6. 未使用子代理；
7. 未修改源码；
8. 未运行构建或测试。

最终回复只需说明：

```text
已生成 Component-only Design：<OutputDesignPath>
来源研究报告：<ResearchReportPath>
subsystemId：<SubsystemId>
Component 数量：<数量>
designStatus：<状态>
仍未解决的组件级 evidence-gap：<数量>
仍未解决的组件 owner 决策：<数量>
未使用子代理，未修改源码，未运行构建或测试。
```

---

## 11. 最终声明

输出的 Markdown 必须明确写出：

```text
本文件是 Component-only Design。
本文件不定义 System、Query、Command、Event、Adapter、Projection、
调度顺序、运行时实现、测试计划或迁移计划。

所有 status: proposed 的 Component 都只是设计提案。
本文件不声明代码已创建、已迁移、行为等价或验证通过。
```
