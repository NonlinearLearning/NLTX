# 第一轮 Version4 子系统审查：公共提示词协议

> 用途：供 19 个游戏模拟子系统的独立并行审查会话复用。
>
> 使用方式：将本文件全文放在每个子系统专属任务包之前，再补充任务编号、固定子系统 ID、子系统专属证据、边界问题和报告路径。
>
> 本文件只定义共同约束，不定义任何单一子系统的事实、组件或最终 owner。

## 1. 会话目标与范围

本会话负责一个固定 Version4 游戏模拟子系统的只读审查、证据整理和 ECS 拆分设计。

“拆分为组件”仅表示提出 `proposed` 的 Component、System、Query、Command、Adapter 和 Projection 设计，不表示创建或修改实际代码。

本会话必须生成一份独立研究报告，但不得把报告结论伪装成迁移完成、行为等价、API 兼容或当前 NLTX 已实现能力。

本轮 19 个子系统会话全部并行执行。各会话只能写入自己的唯一报告文件，不能互相读取正在生成的中间报告。最终整合会话在 19 份报告完成后，统一裁决跨子系统共享类型、接口、owner 和 System 顺序。

## 2. 只读与并行限制

本会话允许读取：

- `D:\TRbackup\Version4`
- `D:\TRbackup\无任何删减通过编译`
- `D:\TRbackup\tmodloader-api-docs-stable`
- `C:\Users\shan\Downloads\ECS\space-station-14-master`
- `D:\TRbackup\NLTX\src`
- `D:\TRbackup\NLTX\Test`
- `D:\TRbackup\NLTX\dome\src`
- NLTX 中已存在的规则、计划、研究、报告和验证材料

本会话只能写入当前任务包指定的独立报告文件。

本会话禁止修改：

- `D:\TRbackup\NLTX\src\`
- `D:\TRbackup\NLTX\Test\`
- `D:\TRbackup\NLTX\dome\src\`
- `D:\TRbackup\Version4`
- `D:\TRbackup\无任何删减通过编译`
- `D:\TRbackup\tmodloader-api-docs-stable`
- `C:\Users\shan\Downloads\ECS\space-station-14-master`
- 全量审查报告
- `Version4子系统索引.json`
- `Version4源码覆盖.tsv`
- 完整源码差异附录
- 子系统发现生成器和覆盖校验器
- 其他子系统报告

禁止创建或修改实际的 `.cs`、`.csproj`、测试、源代码生成或迁移文件。

本会话不得读取其他并行子系统会话正在生成的报告，不得等待其他会话，不得把其他会话的中间设计当作事实或接口约定。

## 3. 禁止编译和共享工作树写入

本轮并行审查阶段不得运行任何 compile-capable 命令：

- `dotnet restore`
- `dotnet build`
- `dotnet rebuild`
- `dotnet test`
- `dotnet run`
- `dotnet publish`
- `dotnet pack`
- `dotnet msbuild`
- 任何会写入 `Build/bin`、`Build/obj`、测试结果、源代码生成物或共享缓存的命令

可以读取既有 `Build/bin`、`Build/obj`、测试输出和历史验证报告。

本次验证状态默认写为：

```text
verificationStatus: not-run
```

如果引用既有、可审计的历史验证结果，只能写为：

```text
verificationStatus: existing-evidence
```

不能把源码存在、设计完整、历史编译通过或既有测试通过写成行为等价。

## 4. 必须阅读的共同规则

在分析前必须完整阅读：

1. `D:\TRbackup\NLTX\AGENTS.md`
2. `D:\TRbackup\NLTX\Context\progress.md`
3. `D:\TRbackup\NLTX\.agents\skills\public-decomposition\SKILL.md`
4. `D:\TRbackup\NLTX\.agents\skills\public-decomposition\references\ecs-evidence-protocol.md`
5. `D:\TRbackup\NLTX\.agents\skills\public-decomposition\references\tmodloader-documentation-retrieval.md`
6. `D:\TRbackup\NLTX\Context/架构设计\ECS文件组织设计约束.md`
7. `D:\TRbackup\NLTX\Context/约束\Google-CSharp-Style-Guide-约束.md`
8. `D:\TRbackup\NLTX\Context/约束\非函数式编码副作用隔离规范.md`
9. `D:\TRbackup\NLTX\docs\plans\component-decomposition\2026-09-05-version4-all-subsystem-discovery-new-session-guide.md`
10. `D:\TRbackup\NLTX\docs\Version4权威游戏模拟子系统全量审查报告-2026-09-05.md`
11. `D:\TRbackup\NLTX\docs\migration\ledgers\Version4子系统索引.json`
12. `D:\TRbackup\NLTX\docs\migration\ledgers\Version4源码覆盖.tsv`
13. `D:\TRbackup\NLTX\docs\Version4与完整源码差异附录-2026-09-05.md`
14. `D:\TRbackup\NLTX\Build\Tools\Test-Version4SubsystemCoverage.ps1`

## 5. 固定证据角色和优先级

### Version4

`D:\TRbackup\Version4` 是唯一覆盖基线，也是 Terraria 真实行为、调用链、状态和生命周期的首要证据。

Version4 决定：

- 子系统是否存在；
- 子系统覆盖范围；
- 实际字段、方法和调用关系；
- 权威状态和写入者；
- 真实 System 顺序和副作用。

### 完整可编译参考

`D:\TRbackup\无任何删减通过编译` 只能补证 Version4 中已经存在文件的成员级删减、空实现或调用链缺口。

必须匹配：

- 路径；
- 类型；
- 成员签名；
- 直接或邻近调用链。

完整参考源码独有文件不得加入 Version4 主覆盖基线，也不得因为完整参考存在实现就假定 Version4 已具有相同实现。

### tModLoader API 文档

`D:\TRbackup\tmodloader-api-docs-stable` 只能用于公开 API、字段语义、生命周期、网络、存档和扩展边界的交叉验证。

必须记录：

- 实际 HTML 路径；
- 页面标题；
- 文档版本；
- 成员或锚点；
- 交叉验证用途。

不得用公开 API 文档替代 Terraria 私有实现证据。

### Space Station 14

`C:\Users\shan\Downloads\ECS\space-station-14-master` 只能用于 ECS 组件粒度、System/Query 边界、事件、关系、持久化和网络访问模式参考。

禁止：

- 复制代码；
- 复制命名；
- 复制目录结构；
- 复制领域语义；
- 用 Space Station 14 推断 Terraria 行为。

只检索与当前子系统直接相关的最小证据集。报告必须列出实际读取的文件、类型/方法名和参考用途。

如果找不到直接对应证据，写明：

```text
Space Station 14 无直接对应证据；以下边界仅由 Version4 真实代码和 NLTX 项目约束决定。
```

固定证据优先级：

```text
Version4
→ 完整可编译参考源码
→ tModLoader API 文档
→ Space Station 14 ECS 参考
```

来源冲突时必须：

1. 记录冲突；
2. 以 Version4 决定覆盖和真实行为；
3. 以完整参考补充 Version4 已存在文件的明确缺口；
4. 以 tModLoader 只交叉验证公开边界；
5. 以 Space Station 14 只参考结构粒度；
6. 未解决时标记 `unresolved`，不得强行定论。

## 6. 行号和源码证据规则

任务包中给出的行号只是检索线索，不是已验证事实。

必须重新读取真实文件并重新定位：

- 类型；
- 字段；
- 属性；
- 方法；
- 构造函数；
- 初始化和注册路径；
- 调用者；
- 读者；
- 写者；
- 网络边界；
- 持久化边界；
- 客户端投影边界。

报告中的每条关键证据必须包含：

- 绝对路径；
- 实际行号；
- 类型或方法名；
- 调用者；
- 读者和写者；
- 生命周期；
- 副作用；
- `evidenceStatus`。

如果行号或源版本发生变化：

- 以实际源码为准；
- 记录 `version-drift`；
- 路径存在但符号不存在时记录 `evidence-mismatch`；
- 不得静默修正；
- 不得因为任务包提供了行号就声称证据已经确认。

## 7. 深读和边界范围

当前子系统的核心文件、直接调用者和直接被调用者必须深读，至少盘点：

- 字段、属性、方法、事件和初始化路径；
- 读者、写者和生命周期；
- 更新频率和副作用；
- 网络、存档、客户端和外部 API 边界；
- 失败、重试、幂等性和提交顺序。

相邻子系统只读到确认边界所需的最小证据，不得在当前报告中重新设计相邻子系统。

发现相邻子系统问题时：

- 记录为 `cross-subsystem finding` 或 `integration-risk`；
- 说明对当前子系统的影响；
- 不复制相邻子系统的字段清单和设计方案；
- 不宣布相邻子系统的最终 owner。

## 8. public-decomposition 分析要求

必须遵循以下顺序：

1. 锁定上下文；
2. 盘点成员、读者、写者、生命周期和副作用；
3. 区分权威状态、行为状态、派生值、缓存、查询、Command、Adapter、Projection、兼容字段和暂缓成员；
4. 按共同读写者、变更原因、生命周期和语义内聚度分组；
5. 选择 Component、System、Query、Command、Adapter、Projection 边界；
6. 分别建模实体引用、持久化 ID、网络 ID、外部 ID 和关系；
7. 写出 Interface、Implementation、Seam、Depth、Leverage、Locality、输入、输出、事件、Command 方向、失败/重试行为和 System 顺序；
8. 设计 focused verifier，但本轮不运行；
9. 检查职责归属、依赖方向、写入所有权、状态一致性、文件组织和副作用隔离。

## 9. 共享类型和跨子系统 owner 规则

单个会话只能确定本子系统独占状态的 proposed 归属。

跨两个或以上子系统使用的类型只能提出候选，不能宣布最终归属。典型类型包括：

- `EntityId`；
- `PlayerEntityId`；
- `NpcEntityId`；
- `PersistentEntityId`；
- `NetworkId`；
- `TileCoordinate`；
- `WorldSectionId`；
- `EntityReference`；
- `WorldTime`；
- `ItemInstanceId`；
- 网络快照和值对象；
- 持久化快照和值对象。

报告中使用：

```text
crossSubsystemOwner: integration-review
```

并列出：

- candidate owner；
- consumers；
- writers；
- unresolved ownership；
- 对最终整合的影响。

不得在单个报告中创建或宣布共享基础类型。

## 10. 设计表达规则

允许：

- Component 字段表；
- 值对象字段表；
- System 输入/输出契约；
- Query 的纯函数签名；
- Command/Event 数据结构草图；
- Adapter/Projection 接口草图；
- Mermaid 或纯文本调用方向图；
- 非编译性的伪代码；
- 带 `Proposed` 或 `status: proposed` 标记的 C# 风格接口示例。

禁止：

- 创建或修改实际 `.cs` 文件；
- 输出可直接提交到 `src/` 的完整实现；
- 输出带真实业务算法的可运行代码；
- 把 proposed path 当成已存在路径；
- 把 proposed 设计写成当前 NLTX 实现；
- 使用“已迁移”“已完成”“已通过”替代证据状态；
- 通过代码片段绕过 public-decomposition 证据门槛。

## 11. 事实、状态、设计和验证必须分区

报告必须严格分成四类内容：

### A. Version4 / 完整参考源码事实

只记录实际读取到的代码、调用链、字段、读者、写者和生命周期；每条事实带路径和实际行号。

### B. 当前 NLTX 状态

只记录当前 `src/`、`Test/` 和既有验证材料中的实际内容。不能因为 proposed 设计存在就标为已实现。

### C. proposed ECS 设计

只提出 Component、System、Query、Command、Adapter、Projection 设计。所有类型、路径和接口标记 `status: proposed`。

### D. 验证结果

只记录实际已经执行的命令或既有可审计输出。并行审查本次统一为 `not-run`，历史材料只能记为 `existing-evidence`。

## 12. 固定状态枚举

证据状态只能使用：

- `confirmed`
- `partial`
- `missing`
- `unresolved`
- `evidence-mismatch`
- `version-drift`
- `blocked`

设计状态只能使用：

- `proposed`
- `deferred`
- `rejected`
- `integration-review`

验证状态只能使用：

- `not-run`
- `existing-evidence`
- `independently-verified`
- `failed`

## 13. 证据缺口和阻塞决策

普通证据缺口不得让会话停止。应继续交付报告，并标记：

- `partial`；
- `unresolved`；
- `evidence-gap`。

只有会改变以下内容的缺口，才标记 `blocking-decision`：

- 子系统是否存在；
- 权威状态归属；
- 事务提交边界；
- 跨子系统共享 owner；
- 必须保持的 System 顺序。

遇到 `blocking-decision` 时：

- 写入报告；
- 给出 2~3 个可选解释及影响；
- 不自行选择会改变架构方向的答案；
- 留给用户或最终整合会话裁决。

## 14. ECS 文件和副作用约束

在提出任何路径、类型或 System 边界前，必须遵守：

- 按游戏领域或能力组织；
- 不创建通用 `Shared/Components/`、`Common/` 或 `Misc/` 聚合目录；
- 一个同名 PascalCase 文件只设计一个核心 public 类型；
- 不依赖文件或目录顺序表达运行时执行顺序；
- System 顺序必须在调度契约中显式表达；
- I/O、时钟、随机数、日志、持久化、网络和 UI 必须通过 Port、Adapter 或 Projection 隔离；
- Query 不得写入权威状态；
- Projection 不得反向写入模拟状态；
- 外部协议类型不得渗透核心状态模型。

所有目标路径均必须写为：

```text
status: proposed
```

## 15. 统一报告最低结构

每个子系统报告至少包含：

1. 执行摘要；
2. 子系统范围和不负责的内容；
3. 证据优先级和来源角色；
4. Version4 真实代码证据表；
5. 完整参考源码补证表；
6. tModLoader 公开 API 交叉验证表；
7. Space Station 14 最小相关 ECS 参考表；
8. 成员、字段、方法、读写者和生命周期盘点表；
9. 权威状态所有权表；
10. proposed 组件拆分表；
11. System、Query、Command、Adapter、Projection 边界；
12. 调用方向和 System 顺序；
13. 持久化、网络和客户端投影边界；
14. 当前 NLTX 映射；
15. focused verifier 设计和建议命令；
16. 本次验证状态；
17. 不拆分项；
18. 兼容策略和行为保持风险；
19. 未决问题、evidence-gap 和 blocking-decision；
20. Integration Handoff；
21. 最终声明。

报告使用中文说明和英文稳定 ID。接口、字段、路径和伪代码必须明确标记为 proposed，不得写成实际实现。

## 16. Integration Handoff 固定格式

每份报告末尾必须包含以下交接摘要。任务包应替换尖括号占位符：

```text
subsystemId: <固定子系统 ID>
taskNumber: <固定任务编号>
reportPath: <当前唯一报告路径>

evidenceStatus:
nltxStatus:
verificationStatus: not-run

confirmedOwners:
- 本报告中可以由 Version4 证据明确归属当前子系统的权威状态和行为

proposedTypes:
- proposed Component
- proposed System
- proposed Query
- proposed Command
- proposed Adapter
- proposed Projection

sharedTypesForIntegrationReview:
- 跨子系统共享候选

crossSubsystemReaders:
- 读取本子系统状态的其他子系统

crossSubsystemWriters:
- 可能写入或触发本子系统状态的其他子系统

orderingConstraints:
- 必须保持的 System 先后关系

boundaryChallenges:
- 对当前子系统边界提出的拆分、合并或保留建议

evidenceGaps:
- 缺失、冲突或未闭合的证据

blockingDecisions:
- 需要最终整合会话或用户裁决的事项

notImplemented:
- 当前 NLTX 明确不存在的实现

verifierPlan:
- 建议的 focused verifier，不代表已经执行
```

Integration Handoff 只是交接摘要，不是最终架构裁决。跨子系统 owner、共享类型、接口和最终执行顺序由最终整合会话统一决定。

## 17. 最终声明和交付说明

每份报告末尾必须明确：

```text
本报告是基于 Version4、完整参考源码、tModLoader 公开文档和有限 ECS 结构参考形成的子系统边界与 ECS 拆分设计。它不是迁移完成报告，不是行为等价证明，不是 API 兼容证明，也不是当前 NLTX 已实现能力的声明。
```

最终回复只需说明：

- 报告路径；
- 已读取的主要证据；
- 本次未运行构建/测试；
- 关键 evidence-gap；
- 关键 blocking-decision；
- 未修改生产代码。
