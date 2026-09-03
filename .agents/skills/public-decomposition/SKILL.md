---
name: public-decomposition
description: Use when refactoring a large class, deep inheritance tree, or ECS model into cohesive components, systems, queries, adapters, or projections; when state ownership, access patterns, hidden side effects, or system ordering are unclear. Do not use for ECS concept explanations, rendering-only work, or one-off bug fixes unrelated to decomposition.
---

# Public Decomposition

将大型模块拆成职责清晰、状态所有权明确且可验证的边界。把拆分视为行为保持的设计工作，
而不是按字段数量复制文件。

## 何时使用

- 出现巨型类、深继承、混合职责或“基础组件”重新聚合多个领域状态。
- 进行 OOP 到 ECS 的迁移，或需要决定 Component、System、Query、Command、Adapter、Projection 的归属。
- 多个调用方重复读取/修改状态，存在隐式全局状态、循环依赖或系统顺序不明确。
- 需要审查拆分方案，而不仅是生成更多文件。

## 工作流

1. **锁定上下文**：识别语言、ECS 框架/版本、参考代码目录、目标实现目录和验证命令。参考目录只读。
2. **盘点成员**：列出字段、属性、方法、事件、外部依赖、读者、写者、生命周期、频率和副作用。
3. **标记所有权**：区分权威状态、派生值、缓存、快照、兼容字段和表现数据；记录暂缓成员及理由。
4. **按访问模式分组**：优先按共同读写者、变更原因、生命周期和语义内聚度分组，而非按字段数量分组。
5. **选择边界**：
   - Component/状态模块只保存一个内聚概念的权威数据；
   - System/行为模块显式读取输入并表达状态转换；
   - Query 只做可重复的纯计算或资格判断；
   - Adapter 负责外部协议和第三方类型转换；
   - Projection/Snapshot 只输出视图，不反向成为权威状态。
6. **检查 ECS 特例**：分别建模实体引用、持久化 ID、网络 ID 和外部 ID；避免巨型组件、继承承载行为、查询持有全局可变状态和无必要的结构变更。
7. **写接口契约**：为每个模块记录 Interface、Implementation、Seam、Depth、Leverage、Locality，并明确输入、输出、事件、命令方向。
8. **保持行为**：先建立 focused verifier 或测试，再迁移一个边界；验证状态转换、纯查询、错误边界和必要的 System 顺序。
9. **审查并交付**：检查职责归属、依赖方向、状态一致性、命名/文件约束和测试证据；无法降低耦合时记录“不拆分”理由。

## ECS 拆分前证据门槛

检测到 ECS 或 OOP 转 ECS 语境时，先读取 `references/ecs-evidence-protocol.md` 并完成证据获取，未达门槛不得给出最终组件拆分：

1. 盘点子系统字段、属性、方法、事件和初始化路径，给每个成员标记读写者、生命周期、状态类型、候选归属及 `confirmed`/`partial`/`missing`。
2. 只读学习 `C:\Users\shan\Downloads\ECS\space-station-14-master` 的组件粒度和 System/Query 边界；禁止复制代码、命名或领域语义。
3. 用 <https://docs.tmodloader.net/docs/stable/index.html> 核对 Terraria 字段语义前，先读取 `references/tmodloader-documentation-retrieval.md`；按其中的索引、实际链接和成员锚点流程取得证据，并记录 URL、版本/标题和证据状态。
4. 关键字段缺证据时按序只读回退 `D:\TRbackup\Version4`，再回退 `D:\TRbackup\无任何删减通过编译`；命中充分证据即停止。
5. 权威、公开 API、持久化/网络和生命周期字段必须确认；冲突或仍为 `missing` 时选择匹配的补证据 skill，列出缺口并向调用者反问，暂停实现。

## 决策规则

| 问题 | 采用的判断 |
| --- | --- |
| Component 是否应拆分 | 是否存在只需要 A 而不需要 B 的系统；是否共同更新、持久化和生命周期一致 |
| 是否合并过度 | 查询复杂度、无效状态和跨组件不变量是否因此增加 |
| 是否创建 Entity | 是否需要独立且持续变化的实例状态 |
| Entity 字段还是关系 | 只需正向查找用实体字段；需要反向查找、分组或遍历再用关系 |
| 是否缓存派生值 | 只有在声明所有权、失效条件和一致性验证后才缓存 |
| 是否保留继承 | 仅保留真正的替换关系；组合能表达的行为不放入继承树 |

## 输出契约

交付以下内容：

- 成员归属表：状态、行为、查询、适配、投影、表现层或暂缓；
- 模块边界图或文字说明：调用方向、状态所有权和 System 顺序；
- 每个新模块的最小接口与替换/测试 seam；
- 不拆分项、兼容策略和行为保持风险；
- focused verifier/测试计划及实际结果。没有验证证据时，标记为未验证，不宣称完成。

## 失败模式

- **机械拆文件**：文件变多但耦合不降；回到访问模式和所有权分析。
- **过度原子化**：查询和不变量变复杂；合并语义共同、生命周期一致的数据。
- **巨型组件回流**：多个领域字段再次集中；按系统访问模式重新切分。
- **查询改状态**：把写入移到 System 或显式 Command。
- **外部类型渗透核心**：在边界建立稳定领域类型或 Adapter。
- **假定顺序**：为必须有先后的系统添加显式调度约束并测试。
- **只测编译**：补充状态转换、纯查询、错误边界和兼容投影验证。

## 参考资料

- 项目详细规则：`../../../约束/公共拆分约束.md`
- ECS 访问模式、组件粒度和系统职责：`references/ecs-decomposition-patterns.md`
- 仓库 C# 命名与布局：`../../../约束/Google-CSharp-Style-Guide-约束.md`
- 触发评测样例：`evals/trigger_cases.json`；语义配置：`evals/semantic_config.json`
- 输出风险档案：`reports/output-risk-profile.md`
- ECS 证据记录模板与来源优先级：`references/ecs-evidence-protocol.md`
- tModLoader 文档的类型、成员与证据检索：`references/tmodloader-documentation-retrieval.md`
