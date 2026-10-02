# P20 世界生成动作、条件、形状与结构规划 System 执行文档

~~~yaml
documentType: system-execution-plan
partitionId: P20
taskId: AUTH-SYS-P20
originalSessionId: 0d1738f8ed3b4403b5beee258fd1b263
sourceReport: D:\TRbackup\NLTX\docs\system-decomposition\reports\2026-09-18-system-decomposition-authoritative-P20-world-generation-actions-shapes.md
designDocument: D:\TRbackup\NLTX\docs\plans\system-decomposition\2026-09-30-version4-authoritative-P20-world-generation-actions-shapes-system-design.md
targetRoot: D:\TRbackup\NLTX\src\NSSLC
targetSourceRoot: D:\TRbackup\Version4
completeReferenceRoot: D:\TRbackup\无任何删减通过编译
comparisonReferenceRoot: C:\Users\shan\Downloads\ECS\space-station-14-master
executionStatus: partial
implementationStatus: partial
verificationStatus: not-run
historicalVerificationStatus: partial
migrationStatus: not-claimed
historicalSourceModified: true
documentRevisionReviewedAt: 2026-10-01
historicalTestsRun: true
deliveryMode: implementation-partial
thisTurnSourceModified: true
thisTurnBuild: focused-project-passed
thisTurnTest: focused-verifier-passed
thisTurnVerification: measured-partial
historicalImplementationAndVerification: retained-below
~~~

## 1. 执行边界

本文保留前序 P20 局部实现的执行边界，不是完整迁移日志或行为等价证明。前序记录曾涉及 src/NSSLC 下的 P20 action execution、structure reservation、shape/modifier explicit-input Query、tile-scan Query 及其 focused verifier；本轮在同一迁移目录补充了 S06/S07 的显式同步执行边界并更新 focused verifier 记录。Version4、完整参考项目和原始 claim report 未被修改，S06/S07 也没有接入 P17/P19/P16/P01 的完整运行路径。

P20 的范围固定为 12 个叶子组、115 个字段、4 个属性、119 个成员。执行过程中不得按叶子数量机械创建调度 System，也不得把目标源码中的空方法体解释为“没有副作用”。任何无法由目标源码、查询 API 或可追溯参考源码闭合的关系必须标记为 unknown、partial 或 integration-review。

执行成功的判定是行为、生命周期、所有权和回滚门槛全部闭合。设计文档、执行文档、编译成功、局部 verifier 成功或兼容 facade 可调用，都不能单独称为迁移成功。

## 2. 固定输入与证据冻结

执行前必须冻结下列输入，并在每个阶段的变更记录中引用同一批身份：

1. P20 authoritative report，以及其中的 originalSessionId。
2. Version4 目标源码的文件内容和哈希。该目录没有 Git metadata，不能伪造 commit 或 revision。
3. 完整参考项目 D:\TRbackup\无任何删减通过编译的只读源码快照。
4. 对照参考项目 C:\Users\shan\Downloads\ECS\space-station-14-master。它只能提供任务组织和依赖显式化的参考，不能证明 Terraria 行为。
5. CPG 数据库 D:\TRbackup\Version4-cpg-export\out-dop8-interproc.sqlite、manifest SHA256 6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364，以及 project fingerprint 521CBD4C11E7EB731256C232350A54099F431D0E59EBDF3C38880281F169C11B。
6. 当前 NLTX 的组件、command、query、port、adapter 和注册清单。
7. P17、P19、P16、P01 的 owner、调度阶段、world/session identity 和提交协议决定。

完整参考项目补充出的行为只能作为 corroborated/partial 证据，不能升级成目标 Version4 的 confirmed 事实。已观察到的参考约束包括：GenAction 和 GenShape 的输出与 NextAction 顺序、IgnoreFailures 与 QuitOnFail、`Actions.cs` 中 S06 的 sink/callback/bounds 顺序、S07 的 framing/debug 直接效果、`RemoveWall` 与 `ClearWall` 的独立写集、paint set/clear 的分流和 `UnitApply` 顺序、WorldUtils.Gen/Find 的入口组合、StructureMap 的 protected overlap 和 padding、AddProtectedStructure 的双列表写入、Reset 只清 protected 列表，以及 WorldGenRange 的尺寸缩放和 inclusive random range。

CPG 查询结果存在 partial、virtual dispatch、zero-hit gap 和缺少 SourceSnapshotId 的问题。查询完成只说明索引范围内完成，不说明运行时 scheduler、delegate、reflection、alias 或外部注册已闭合。8 个 partial zero-hit 结果必须保留 NoMatchingFactInScannedScope 语义，不能写成“无调用”。

### 2.1 本次完整参考源码与查询复核

只读复核确认以下参考位置与执行计划相互一致：`GenAction.UnitApply`（`GenAction.cs:15-27`）、`GenShape.UnitApply`（`GenShape.cs:13-21`）、`Actions` S06/S07/wall/paint apply body（`Actions.cs:21-27,39-63,82-92,147-153,352-359,361-380,268-302,420-542,565-574,650-666`）、`WorldUtils.Gen/Find`（`WorldUtils.cs:39-63`）、`StructureMap.CanPlace/AddProtectedStructure/Reset`（`StructureMap.cs:19-109`）、`WorldGenRange.GetRandom/ScaleValue`（`WorldGenRange.cs:38-65`）以及 `WorldGenerator.GenerateWorld/RunPass`（`WorldGenerator.cs:453-497`）。参考树的文件哈希仍以设计文档第 3.2 节为准；没有对参考项目运行构建。

本次 CPG API 复核使用 `Find-CpgSymbols`，显式指定 `SymbolMethod` 和声明 `SourcePath`：`WorldUtils.Gen`、`WorldUtils.Find`、`StructureMap.CanPlace`、`StructureMap.AddProtectedStructure`、`WorldGenerator.GenerateWorld`、`WorldGenerator.RunPass`、`WorldGenRange.GetRandom`、`WorldGenRange.ScaleValue` 查询为 `complete`；`GenShape.UnitApply` 为 `partial`，并返回 `NoMatchingFactInScannedScope`；三个 clear paint 类型、Rainbow 类型和 `RemoveWall` 的 `Apply` surface 为 `complete`，callable facts 为 `partial/CalleeEffectsNotExpanded`。该结果只记录索引事实，不能把 zero-hit 当成无调用，也不能替代入站/动态/生命周期闭包。

## 3. 前置门槛

只有以下门槛全部记录后，才允许从 planned-blocked 进入任何实现阶段：

| 门槛 | 必须回答的问题 | 未满足时 |
|---|---|---|
| 身份 | world、generation、session、entity、random stream 如何区分 | 停在 integration-review |
| 所有权 | 每个状态和副作用的唯一写者、reset owner、持久化 owner 是谁 | 不创建 writer |
| 调度 | P19 pass loop 与 P20 action execution 的交接点在哪里 | 不接入 runtime |
| 提交 | tile/wall/liquid/framing/structure 的 commit barrier 是否唯一 | 只允许静态接口 |
| 兼容 | LegacyWorldGenActionAdapter 是否只转发而不二次写入 | 禁止删除旧 writer |
| 证据 | 每个行为是否有目标源码、CPG 或参考源码的可追溯证据 | 标 unknown/partial |
| 回滚 | 新 owner 失败后能否退回上一个 commit barrier | 阶段保持 shadow-only |

## 4. 未来文件组织

以下是建议路径，不表示本轮已经创建或移动文件：

| 责任 | 建议路径 | 约束 |
|---|---|---|
| P20 Query/Definition | src/NSSLC/WorldGeneration/Queries、Definitions | 只读、显式输入、不得访问 ambient global |
| action command payload | src/NSSLC/WorldGeneration/Commands | 带 generation scope、顺序标识、目标 identity 和验证结果 |
| action execution | src/NSSLC/WorldGeneration/Systems/WorldGenerationActionExecutionSystem.cs | 只协调有序 action，不拥有 terrain/liquid/framing 状态 |
| structure reservation | src/NSSLC/WorldGeneration/Systems/WorldStructureReservationSystem.cs | 候选唯一 owner，待 integration-review 批准 |
| legacy 转换 | src/NSSLC/WorldGeneration/Adapters/LegacyWorldGenActionAdapter.cs | 兼容期单向转发，禁止双写 |
| 结果投影 | src/NSSLC/WorldGeneration/Projections | 不回写 authority |
| P20 行为证据 | docs/evidence/p20 | 保存 Observation tuple、source diff、查询输出和哈希 |

目录名称和组件命名在真正新增、移动或重命名之前，必须重新读取 ECS 文件组织约束、组件命名约束、C# 风格约束和副作用隔离规范。

## 5. 有序执行阶段

每个阶段必须保存输入版本、输出 artifact、停止原因和可回滚点。阶段完成不代表迁移完成；未满足停止条件时必须保持当前旧路径或 shadow-only。

### E0：证据冻结与 source diff

**输入**

- P20 report、originalSessionId 和完整 119-member scope。
- Version4 目标源码、完整参考源码、CPG manifest 和当前 NLTX inventory。
- 已记录的完整参考文件哈希。

**动作**

1. 对目标与完整参考的 WorldGen.cs、Actions.cs、StructureMap.cs、WorldGenerator.cs、WorldGenSnapshot.cs、ModShapes.cs、Modifiers.cs、Conditions.cs、Searches.cs、WorldUtils.cs 做只读 source diff。
2. 将每个差异分为 target-fact、reference-supplement、CPG-fact、unknown，不用参考 body 覆盖目标空实现。
3. 重新运行只读查询 API 仅用于补证据：WorldUtils.Gen、WorldUtils.Find、StructureMap.CanPlace、StructureMap.AddProtectedStructure、WorldGenerator.GenerateWorld、WorldGenerator.RunPass、WorldGenRange.GetRandom、WorldGenRange.ScaleValue。
4. 保存 query 参数、manifest、返回状态、gap 和 source hash；不得把查询结果直接写入生产代码。

**输出**

- evidence freeze manifest；
- target/reference source diff；
- P20 119 成员的证据分类表；
- 每个跨分区关系的 integration-review 清单。

**停止条件**

- 发现 source snapshot 不一致、输入 report 身份不一致、关键 caller 仍处于 unknown，或完整参考行为无法映射到目标版本时，停止后续实现并保留 unknown。

**回滚**

- E0 只产生证据文件。若证据文件错误，删除或修订当前任务生成的证据 artifact，不触碰源码和已有 report。

### E1：Query 与 Definition 边界

**输入**

- E0 冻结的证据；
- 现有 WorldTileMergeCullStateQuery、condition/search query、shape/modifier calculation API；
- P19 提供的 immutable tile/world snapshot、显式 dimensions 和 random input。

**动作**

1. 保留 S01、S08、S09、S10、S11 的纯 Query/Definition 方向。
2. 使 ShapeTraversalQuery 显式接收点集、origin、action descriptor、failure policy、output policy 和 random stream。
3. 使 WorldGenerationConditionsAndSearchesQuery 显式接收 tile snapshot、bounds、conditions、方向和距离，并保留 NOT_FOUND 到 false 的兼容映射。
4. 使 WorldGenRangeQuery 显式接收 minimum、maximum、ScaleWith、world dimensions 和 random stream；禁止读取 ambient Main 或隐式随机源。
5. 将 S06 的 continuation、count、scanner、custom callback 和 bounds sink 保留为 partial；本轮只闭合同步调用 lifetime、异常映射和 writable bounds 边界，取消、NextAction 和 runtime caller 仍未闭合。

**输出**

- Query/Definition API 草案；
- 不变量、输入快照和随机消耗说明；
- 每个 Query 的纯读证明或 deferred adapter 清单。

**停止条件**

- 任何 Query 需要写回 cache、兼容字段、队列、tile、structure 或 callback sink 时，立即退回 Command/owner 评审，不以“看起来是读取”继续。

**回滚**

- 删除未接入 runtime 的新 Query/Definition 草案即可；不得修改现有 NLTX Query 的行为以迎合未证实的 reference 结果。

### E2：Action command validation 与 ordered execution result

**输入**

- E1 的显式 shape/action 结果；
- 现有六类 P20 command payload；
- P19 的 sequence、generation scope、cancellation 和 retry policy。

**动作**

1. 为 tile set、wall mutation、placement/paint、liquid/neighbor、scan/control、framing/debug command 统一补充 scope、order identity、目标坐标和 well-formed validation。
2. 设计 ActionExecutionResult，至少保留 first failure、continued/quit、relative OutputData、shape output、callback exception policy、consumed random facts 和未提交 command。
3. WorldGenerationActionExecutionSystem 只负责按 sequence 驱动 Query 与 command，收集结果并在一个定义明确的 barrier 输出 commit intents。
4. 按完整参考的 GenAction.UnitApply、GenShape.UnitApply、IgnoreFailures、QuitOnFail 和 NextAction 顺序建立行为样例；目标源码缺失的部分必须记录为待验证。
5. 不在 P20 execution System 中直接写 Main.tile、wall、liquid、frame、SpriteBatch 或 P19 pass cursor。

**输出**

- command validation contract；
- ordered execution result schema；
- action sequence 与失败策略的 Observation fixture；
- P17/P01 所需的 commit intent 清单。

**停止条件**

- action order、random draw、失败后继续/退出、callback lifetime 或异常策略无法从目标证据闭合时，只允许 shadow mode，不能启用新 writer。

**回滚**

- 在 sequence commit barrier 丢弃未提交 intent，恢复 LegacyWorldGenActionAdapter；不回滚已经由旧 owner 提交的 world state。

### E3：P17/P01 commit port

**输入**

- E2 的有序 tile、wall、paint、liquid、neighbor、framing intent；
- P17 terrain/framing owner 与 P01 liquid owner 的 integration-review 决定；
- commit acknowledgement、拒绝理由和 retry token。

**动作**

1. 为 P17 提供 tile/wall/paint/slope/half-block/frame intent port，为 P01 或批准的集成 owner 提供 liquid/neighbor intent port。
2. 明确每个 intent 的原子提交边界、坐标快照、冲突检测、拒绝结果和重试语义。
3. 保留 framing/cull 的 revision 输入，S01 只计算 flags，S07 通过显式 framing port/diagnostic sink 执行 intent；真实 P17/渲染 owner 仍由集成阶段接入。
4. 让 Legacy adapter 在兼容期只转换旧调用，不与 P17/P01 同时写同一状态。
5. 记录提交顺序：action sequence 完成、terrain/liquid commit、framing update、result projection。

**输出**

- P17/P01 commit port contract；
- writer matrix 和 crossSubsystemOwner；
- commit acknowledgement/error mapping；
- 单 writer 的运行时注册草案。

**停止条件**

- P17/P01 owner、调度阶段或失败重试未确定；不能以一个 P20 System 代替相邻分区的 owner。

**回滚**

- 禁用新 port route，在最近的 commit barrier 回退到旧 adapter；保留未提交 command 供诊断，不重复提交。

### E4：WorldStructureReservationSystem

**输入**

- candidate rectangle、padding、world bounds；
- protected reservation snapshot；
- tile validity snapshot；
- E0 中 StructureMap 的参考行为和原子性缺口。

**动作**

1. 将 StructureMap 的 eligibility、protected overlap、padding、valid tile、AddStructure、AddProtectedStructure 和 Reset 集中到一个候选 owner。
2. 把 check + reserve + result 放在同一个 owner operation，避免公开两个可独立重入的 CanPlace/Add 方法。
3. 返回 accepted/rejected reason、generation scope、reservation identity 和 immutable snapshot。
4. 处理 duplicate、冲突、取消、reset、异常和 multi-world 隔离；普通 structures 与 protected structures 的生命周期要分别记录。
5. 为 WorldGenSnapshot 提供 serializable projection，但在 P16/P19 确认持久化 owner 前不接管 restore。

**输出**

- StructurePlacementQuery；
- WorldStructureReservationSystem API；
- StructureReservationSnapshotProjection；
- 原子性、锁、reset 和持久化 integration-review 记录。

**停止条件**

- 若无法证明 CanPlace 与后续 Add 的原子边界、world lifetime 或 snapshot owner，保持 proposed，不启用 canonical writer。

**回滚**

- 只撤销当前 generation scope 的未提交 reservation；不得清除其他 world 或其他 generation 的 protected state。恢复旧 StructureMap adapter，并记录潜在竞态。

### E5：P19/P16/P01 集成

**输入**

- E2/E3 的 action result 和 commit acknowledgement；
- E4 的 reservation snapshot；
- P19 pass loop、P16 snapshot/recovery、P01 liquid lifecycle 的批准契约。

**动作**

1. P19 负责 pass cursor、result、progress、pause、abort、retry 和 sequence barrier；P20 不重新实现 pass loop。
2. P16/P19 负责 snapshot adapter 与 restore orchestration；P20 只提供可序列化 reservation projection 和 action result projection。
3. P01/P17 接收相应 intent 并返回 commit result；P20 不回写跨分区组件。
4. 统一 generation/session identity、random replay、cancellation、world unload 和 multi-world isolation。
5. 完成 legacy facade 到新组合的入口映射，但在行为对照通过前保留旧 writer。

**输出**

- 跨分区调用图与顺序图；
- lifecycle/reset/retry contract；
- save/load/network projection 的 owner 记录；
- shadow route 与 feature gate。

**停止条件**

- 任一分区出现双 writer、未知 scheduler phase、重复 random 消耗、snapshot restore 越权或 session identity 丢失时，集成阶段停止。

**回滚**

- 关闭 feature gate，回退到旧 facade/adapter；清理只属于当前 generation 的新 projection 和未提交 reservation，保留 snapshot 版本兼容。

### E6：focused behavior verification

**输入**

- 真实迁移入口；
- E0 冻结的目标/参考证据；
- 已批准的 build/test/verifier 流程和构建并发约束；
- Observation tuple 记录器。

**动作**

1. 对 action/shape 执行顺序验证 OutputData、shape output、NextAction、IgnoreFailures、QuitOnFail、first failure 和 random draw。
2. 对 condition/search 验证 all/any、bounds/fluff、NOT_FOUND、方向、距离和边界拒绝。
3. 对 modifier/range 验证点集顺序、duplicate、scale、inclusive random range、重试 replay。
4. 对 structure 验证 world bounds、padding、protected overlap、valid tile、ordinary/protected list、Reset、duplicate 和原子 reservation。
5. 对跨分区验证 action commit、liquid/framing order、pass barrier、snapshot/restore、reset、network projection 和 multi-world isolation。
6. 每个场景保存 old/new Observation tuple、source evidence、执行命令和原始输出；未知行为不得用通过的相邻场景代替。

**输出**

- focused behavior report；
- old/new Observation diff；
- 未覆盖项和残余 unknown；
- 可复现的失败输入。

**停止条件**

- 任何关键行为缺少真实入口证据、只跑到兼容 facade、只验证局部 Query 或只通过编译时，verificationStatus 保持 not-run 或 measured-partial，不能进入 E7。

**回滚**

- 关闭新 route 并恢复旧 adapter；保留失败 fixture 和日志用于修复，不覆盖原始证据。

### E7：迁移验收与删除门

**输入**

- E6 完整 Observation diff；
- 119 成员 owner matrix；
- 生命周期、持久化、网络和回滚证据；
- P17/P19/P16/P01 的集成签字记录。

**动作**

1. 检查所有 119 成员是否有一个已观察 owner，或有明确批准的 deferred adapter。
2. 检查所有 confirmed legacy entry point 是否到达新组合，且没有第二个 authority。
3. 检查 source diff 中的 unknown/partial 是否已经关闭或被批准保留为兼容边界。
4. 仅在行为、生命周期、snapshot、network、retry、multi-world 和 deletion gate 全部通过后，制定旧 writer 删除顺序。
5. 记录删除前快照、恢复点和 feature gate；删除操作另行执行，不在本文档阶段完成。

**输出**

- migration acceptance record；
- deletion gate decision；
- 最终 owner/reader/writer matrix；
- 若未通过，则输出 blocked record 和下一阶段输入。

**停止条件**

- 任一成员仍只有命名推断、参考项目补充或 zero-hit 结果，或任一关键 Observation 未通过，禁止标记 migration-success。

**回滚**

- 删除门未通过时不删除旧 writer；若删除后发现差异，按记录的 commit barrier 恢复旧 adapter、snapshot 和 generation-scoped state。

## 6. API 与错误契约

每个公开 Query、Definition、Command、System、Adapter 和 Projection 都必须写清：

- world/generation/session scope 和 identity；
- 输入快照、坐标系、顺序标识和 random stream；
- precondition、validation error、duplicate 行为和 retry 行为；
- authoritative delta、唯一 commit point 和可见性；
- effect request、callback exception、cancellation 和 timeout；
- reset、unload、entity reuse 和 multi-world 行为；
- save/network projection、版本兼容和恢复失败处理。

Query 必须对同一已提交 snapshot 可重复，不能懒初始化、消费队列、写兼容字段或调用 effect port。读操作一旦隐藏修改，必须重新分类为 Command 并回到 owner 评审。

## 7. 回滚矩阵

| 阶段 | 新增状态 | 允许的回滚点 | 禁止的回滚 |
|---|---|---|---|
| E0 | evidence artifact | 删除当前任务生成的错误 artifact | 修改 source/report 以消除 unknown |
| E1 | Query/Definition 草案 | 删除未注册类型 | 让 Query 直接写 authority |
| E2 | command/result 与 pending intent | sequence commit barrier 前丢弃 | 重放已提交旧 action |
| E3 | P17/P01 port route | commit barrier 前关闭 feature gate | P20 直接写 terrain/liquid |
| E4 | generation-scoped reservation | 当前 scope 的未提交 reservation | 清除其他 world/protected state |
| E5 | lifecycle/projection route | pass/snapshot barrier 前恢复旧 adapter | P20 接管 P19 loop 或 snapshot restore |
| E6 | verifier fixture/log | 保留失败输入并关闭新 route | 用局部通过覆盖关键失败 |
| E7 | deletion gate metadata | 不删除旧 writer，返回 blocked | 以编译成功代替行为验收 |

## 8. 只读查询与未来验证命令模板

下列命令仅是未来执行模板，本轮没有运行。实际执行前必须重新读取构建与验证约束，并确认 CPG 数据库、source hash 和目标 checkout 身份。

~~~powershell
# 只读 CPG 查询模板；不得把 zero-hit 当成 no-caller
$cpgTool = 'C:\Users\shan\.agents\skills\ecs-system\tools\CpgEvidence.ps1'
$cpgDb = 'D:\TRbackup\Version4-cpg-export\out-dop8-interproc.sqlite'
& $cpgTool -Database $cpgDb -QuerySymbol 'WorldUtils.Gen'
& $cpgTool -Database $cpgDb -QuerySymbol 'WorldUtils.Find'
& $cpgTool -Database $cpgDb -QuerySymbol 'StructureMap.CanPlace'
& $cpgTool -Database $cpgDb -QuerySymbol 'StructureMap.AddProtectedStructure'
& $cpgTool -Database $cpgDb -QuerySymbol 'WorldGenerator.GenerateWorld'
& $cpgTool -Database $cpgDb -QuerySymbol 'WorldGenerator.RunPass'
& $cpgTool -Database $cpgDb -QuerySymbol 'WorldGenRange.GetRandom'
& $cpgTool -Database $cpgDb -QuerySymbol 'WorldGenRange.ScaleValue'
~~~

~~~powershell
# 未来构建/测试模板；本轮禁止执行
Get-Content 'D:\TRbackup\NLTX\Context\约束\构建与验证约束.md'
dotnet build <approved-project-or-solution>
dotnet test <approved-test-project> --no-restore
<approved-verifier-command>
~~~

命令输出必须保存原始 stdout/stderr、退出码、source hash、构建并发身份和执行时间。若验证命令未执行，verificationStatus 必须仍为 not-run。

## 9. 验收观察向量

完成 E6 前至少要覆盖：

- action/shape：point order、relative OutputData、shape output、NextAction、IgnoreFailures、QuitOnFail、first failure、callback 异常和 random draw；
- condition/search：all/any、NOT_FOUND、bounds/fluff、方向、距离、边界和空结果；
- modifier/range：点集、outline、offset、expand、mask、dither、blotches、flip、scale、inclusive range 和 retry replay；
- structure：world bounds、padding、protected overlap、valid tile、AddStructure、AddProtectedStructure、Reset、duplicate、原子性和 multi-world；
- integration：P19 pass barrier、P17 terrain、P01 liquid、framing/cull revision、P16/P19 snapshot、network projection、cancel、reset、unload 和 compatibility read。

每项都要有真实迁移入口和 old/new Observation tuple。没有真实入口、只有静态报告或只有参考源码的项目，状态为 unknown 或 not-run。

## 10. 前序执行记录与状态（历史）

前序记录已完成：

- 读取项目入口和变更约束；
- 读取 pua 自监督要求并按“先查证据、记录缺口、不给未验证结论”的方式执行；
- 只读检查完整参考项目 D:\TRbackup\无任何删减通过编译；
- 保留 CPG 查询的 partial、virtual dispatch、zero-hit 和 source identity 缺口；
- 编写 P20 System 设计文档和本执行文档；
- 新增显式 generation/sequence 的 action payload、commit port、ordered execution result 和 failure policy；execution result 现在保留 first failure reason、continued/quit 和 StopOnFailure 后的未提交 action；
- 新增 generation-scoped、加锁的 WorldStructureReservationSystem，覆盖 padding、world bounds、protected overlap、tile validity、snapshot 和 reset；本轮修正 reset 为释放 protected membership、保留 ordinary structure history，并增加独立的 `DiscardGeneration` unload/abort 边界，与完整参考 `StructureMap.Reset` 的双列表生命周期一致；
- 为旧 InMemoryStructureReservationAdapter 增加 Reserve/CanPlace/snapshot 的锁保护；
- 新增 `Queries/WorldGenRangeQuery.cs`，显式接收 minimum、maximum、ScalingMode、world dimensions 和 `IGenerationRandomSource`，保留完整参考项目的 WorldArea、WorldWidth、None 缩放和 inclusive maximum；
- 补强 `WorldGenerationConditionsAndSearchesQuery` 的 `RequireAll(bool)`：搜索定义显式保存 all/any 模式，`WithConditions`/`Chain` 保留模式，方向与矩形扫描按参考 `GenSearch.Check` 的短路规则求值；默认 all 行为不变；
- 新增 `Adapters/WorldGenerationSearchCompatibilityAdapter.cs`：把显式 snapshot 搜索结果映射为旧式 `bool + out TilePosition`，未找到时稳定返回 `NOT_FOUND`；adapter 不访问 ambient world，也不实现第二套搜索逻辑；
- 新增 `Queries/ShapeTraversalQuery.cs`，以 immutable shape definition 为输入，记录 relative shape output、每个 action 的 UnitApply output、NextAction 链结果、first failure、QuitOnFail 和 callback exception policy；该 Query 不调用 commit port；
- 新增 `Adapters/WorldGenerationActionCommitRouter.cs`，要求六类显式 owner port，按 payload kind 单路由并透传 generation/sequence；不实现 P17/P01 写入；
- 新增 `Systems/WorldGenerationActionExecutionBatch.cs`，把 action 的 prepare/validate 与 owner commit 分成两个显式阶段；`Prepare` 复制输入并且不调用 port，`Commit` 才进入 commit barrier；
- 新增 `Adapters/WorldGenerationActionResultProjection.cs` 与 `Adapters/WorldStructureReservationSnapshotProjection.cs`，为诊断/持久化边界提供复制后的只读 projection，不回写 authority；
- 修正 `WorldGenerationActionExecutionResult` 的可选 `uncommittedActions` 契约：省略参数时投影为空只读列表，不再先对可选 `null` 参数执行 `ThrowIfNull`；保留 `records` 的必填非空校验。
- 增强 `WorldGenerationActionExecutionResult` 和 `WorldGenerationActionResultProjection` 的 scope 契约：经 `Prepare`/`Commit` 生成的结果显式携带 `GenerationId`；保留旧手工构造入口并将其 scope 标记为 `null/unknown`，不把缺失身份补猜为某个 generation。
- 加强 execution result 的副作用隔离：`Records` 与 `UncommittedActions` 在结果构造时防御性复制，调用方后续修改原集合不会改变已提交结果；focused verifier 已覆盖这一边界。
- 增加 execution result scope 校验：带 `GenerationId` 的结果拒绝来自其他 generation 的 `UncommittedActions`，避免停止策略结果跨 scope 重放；兼容构造的 `null` scope 仍保持 unknown。
- 新增并扩展 P20 focused verifier 断言，覆盖 action order、stop/continue failure policy、range scaling/inclusive bound、shape chain/output、条件 all/any 与 NOT_FOUND、结构 overlap、generation isolation、invalid tile、protected reset 和 generation discard。
- 2026-10-01 增量复核读取完整参考源码并调用只读 CPG API；随后按仓库串行构建约束重新构建 P20 verification 项目并运行既有 focused verifier。该验证只覆盖局部 action/shape/range/structure 核心，不升级 authoritative output report 的 `verificationStatus: not-run`。
- 2026-10-01 继续对照完整参考 `Terraria.WorldBuilding/Modifiers.cs`，补充 P20 focused verifier 对 scale/expand、offset/flip、rectangle mask、dither、radial dither 和 blotches 显式输入的观察。验证发现并修正了两个测试假设（scale 的参考 doubling 结果为 12 个点；radial dither 的阈值边界保留中心点），最终串行 build 与 `--no-build --no-restore` verifier 均退出码 0。CPG 对 `Modifiers` 方法名查询仍为 `partial/NoMatchingFactInScannedScope`，因此没有把该观察升级为目标运行时 random parity。
- 2026-10-01 读取完整参考 `Actions.TileScanner.Apply/GetCount`，并通过只读 CPG 查询 `TileScanner`（`partial/NoMatchingFactInScannedScope`）确认目标调用关系未闭合；新增 `WorldGenerationTileScanQuery`，把 active/type 计数、重复 requested ID 去重、total matches 和未知 tile 的 `-1` sentinel 固定在显式 snapshot Query 中。focused verifier 覆盖该纯边界，串行 build 与 `--no-build --no-restore` verifier 均退出码 0。

前序记录仍未完成：

- 未修改 Version4 或任何参考项目；
- 未把 action commit router 接入 P17 terrain/P01 liquid/framing owner；当前只验证了六路 port 的路由边界；
- 未把 action execution 接入 P19 pass loop、random replay 或 lifecycle barrier；
- 未将 reservation snapshot 接入 P16/P19 persistence，也未证明与完整 StructureMap 的全局 parity；
- 前序记录未覆盖 S06 continuation/count sink/scanner callback/bounds lifetime、S07 callback/debug、WorldUtils.Gen/Find 的真实兼容入口、snapshot/network/multi-world 全链路；本轮已补充 S06/S07 的显式同步执行边界，但目标 Version4 的 GenSearch body、旧 action chain、P19/P17/P01 接入仍缺失，因此 any 模式尚未获得目标运行时确认；
- `WorldGenRangeQuery`、`ShapeTraversalQuery`、`WorldGenerationShapeModifierStateDefinitionQuery`、`WorldGenerationTileScanQuery`、`WorldGenerationActionExecutionBatch` 和两个 projection 已写入源码，并纳入当前 P20 focused verifier；本轮 generation scope、modifier explicit-input 与 tile-scan snapshot 增量也已通过同一项目的串行编译确认；
- 未把 proposed 设计、局部编译或 focused verifier 通过称为迁移成功。

### 当前验证记录

- SDK：仓库要求的 10.0.400；本机原先仅有 10.0.100，因此安装到 Build/dotnet-sdk-10.0.400 后通过 PATH 显式使用，未修改 global.json。
- 历史基线记录：在本轮新增 range/shape Query 前，生产项目和旧版 focused verifier 曾通过同一 wrapper 构建，产物位于 `Build/bin/`；该记录不覆盖本轮新增源码。
- 当前串行构建：使用仓库要求的 `Build/Tools/Invoke-SerialDotnet.ps1`、显式 `Build/dotnet-sdk-10.0.400` PATH 和 `Terraria.WorldGeneration.P20.Verification.csproj`，退出码 0；`Terraria.WorldSession`、P20 verifier 及依赖均生成到 `Build/bin/`，0 warning/0 error。
- 构建阻断记录：首次串行 build 暴露工作树中 `src/NSSLC/Component/WorldSession/WorldGeneration/Housing/HousingRoomScoreSnapshot.cs:49` 的 `CS0019`（`ReadOnlyCollection<T> ?? T[]`）。已将该属性改为显式 getter，在保留 `_candidates` 引用及空数组回退语义的前提下消除无效的 `??` 类型组合；随后按同一串行 wrapper 重跑，构建退出码 0、0 warning/0 error。该修复未改变 P20 owner 或运行时接入边界。
- 构建命令参数：`$env:PATH = (Resolve-Path 'Build/dotnet-sdk-10.0.400').Path + ';' + $env:PATH; & .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @('build', '.\Test\Terraria.WorldGeneration.P20.Verification\Terraria.WorldGeneration.P20.Verification.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')`。
- 产物检查：`Build/bin/Terraria.WorldSession/Debug/net10.0/Terraria.WorldSession.dll` 与 `Build/bin/Terraria.WorldGeneration.P20.Verification/Debug/net10.0/Terraria.WorldGeneration.P20.Verification.dll` 均存在；产物仅位于 `Build/bin/`。
- 当前 focused verifier：使用同一 wrapper 执行 `run --project --no-build --no-restore`，退出码 0，输出 `PASS: P20 core action and structure reservation checks`。
- verifier 命令参数：同一 SDK PATH 下调用 `Invoke-SerialDotnet.ps1 -DotnetArguments @('run', '--project', '.\Test\Terraria.WorldGeneration.P20.Verification\Terraria.WorldGeneration.P20.Verification.csproj', '--no-build', '--no-restore', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')`。
- 当前核心覆盖：action order、Prepare/Commit barrier、输入复制、single-owner route、generation/sequence 透传、StopOnFailure/ContinueAfterFailure、optional empty result、action result projection、reservation overlap/generation isolation/invalid tile/protected reset/discard、reservation snapshot projection、inclusive range/scaling、shape 去重/输出/NextAction、modifier 的 scale/expand/offset/flip/mask/dither/radial/blotches 显式输入、tile-scan active/type count、duplicate requested ID 和 unknown sentinel、conditions all/any/NOT_FOUND、QuitOnFail 和 `TreatAsFailureAndStop`。
- generation scope 增量的 focused 覆盖：execution system 从 `WorldGenerationActionExecutionBatch.GenerationId` 写入结果，projection 复制该 scope；旧兼容构造的 `GenerationId == null` 保留为 unknown。verifier 已断言运行结果 scope、projection scope 与兼容构造的 unknown scope，仍只作为局部实现证据。
- 本轮只读 CPG 查询：使用 `.agents/skills/ecs-system/tools/CpgEvidence.ps1` 的 `Find-CpgSymbols`/`Find-CpgCallSites`，数据库 `D:\TRbackup\Version4-cpg-export\out-dop8-interproc.sqlite`，manifest `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`。`WorldGenRange.GetRandom`、`WorldGenRange.ScaleValue`、`WorldUtils.Gen`、`StructureMap.CanPlace` 的 method symbol 均可定位；`WorldUtils.Gen` 在 `Terraria/WorldGen.cs` 范围返回 3 个 `internal-static-exact` confirmed call-site，其他调用闭包仍需扩大范围。新增查询的 `GenSearch.Check`、`GenSearch.RequireAll` 和 `TileScanner` 均为 `partial/NoMatchingFactInScannedScope`；GetRandom/ScaleValue 的声明文件 call-site 查询、`GenShape.UnitApply` 以及本轮 `Modifiers` 名称查询也保留 partial gap，因此没有把 zero-hit 当作无调用。
- 测试范围仍限制为约 10% 的 action/shape/range/structure 核心行为；不代表 119 成员、完整旧入口或跨分区行为已验证。
- optional-list 修复已由空结果断言覆盖；projection、两阶段 batch、modifier explicit-input 和 tile-scan snapshot 边界也只经过该 focused verifier，不能外推为完整运行时验证。完整参考的每次 `Modifiers.*.Apply` random draw 次数、顺序和 retry replay，以及 TileScanner sink/callback/lifecycle 仍是 `unknown`。

前序局部实现记录的历史状态为：

~~~yaml
designStatus: proposed
executionStatus: partial
implementationStatus: partial
verificationStatus: partial
migrationStatus: not-claimed
~~~

## 11. 本轮实现边界（2026-10-01）

本轮目标已进入代码实现。第 10 节及其“当前验证记录”保留前序局部实现与验证的历史轨迹；本节单独记录本轮新增 S06/S07 代码、action commit 取消屏障和 focused 验证：

~~~yaml
deliveryMode: implementation-partial
thisTurnSourceModified: true
thisTurnBuild: focused-project-passed
thisTurnTest: focused-verifier-passed
thisTurnVerification: measured-partial
migrationStatus: not-claimed
~~~

本轮使用只读 CPG 查询 API 复核的结果：

- `WorldUtils.Gen` 在 `Terraria/WorldGen.cs` 范围返回 `complete`、3 个静态调用点；`GenShape.Perform` 的 virtual 目标、外部注册和 scheduler 仍未闭合。
- `StructureMap.CanPlace` 的两个重载分别返回一个 `complete` 调用结果，以及 `partial`、0 item、`NoMatchingFactInScannedScope`；后者保留为缺口，不解释为没有调用。
- `StructureMap.AddProtectedStructure` 返回 `complete`、2 个调用点；这不能证明与前置 `CanPlace` 组成原子事务。
- `WorldGenRange.GetRandom` 的声明可定位，但在声明文件调用扫描为 `partial`、0 item、`NoMatchingFactInScannedScope`；random draw、caller 闭包和 retry replay 仍为 `unknown`。
- `TileScanner` 的声明可定位，但在 `Actions.cs` 调用扫描为 `partial`、0 item、`NoMatchingFactInScannedScope`；完整参考项目能确认 active/type 计数与未知类型 `-1` sentinel，不能确认目标运行时 sink、callback 或 lifecycle。
- `SetFrames` 的声明可定位，但在 `Actions.cs` 调用扫描为 `partial`、0 item、`NoMatchingFactInScannedScope`；完整参考项目只能确认 framing 调用语义，目标 framing owner、线程约束和 runtime registration 仍为 `unknown`。
- `DebugDraw` 的声明可定位，但在 `Actions.cs` 调用扫描为 `partial`、0 item、`NoMatchingFactInScannedScope`；完整参考项目只能确认 debug draw 的直接效果，目标 diagnostic owner、生命周期和渲染线程约束仍为 `unknown`。

本轮新增实现：

- `WorldGenerationTileScanAndControlSystem`：以显式 observation 执行 `Continue`、`Count`/`Scanner`、`TileScanner`、`Custom` 和 `UpdateBounds`，同步调用、调用返回即释放引用；`Propagate`、`TreatAsFailure`、`TreatAsFailureAndStop` 三种异常策略均由 API 显式选择。
- `WorldGenerationTileFramingAndDebugSystem`：将 `SetFrames` 路由到 `IFramingPort`，将 `DebugDraw` 路由到 diagnostic sink；缺少 framing owner 返回稳定拒绝，不访问 ambient tile 或 graphics。
- `WorldGenerationActionExecutionSystem`：在每个 owner commit 前检查显式 `CancellationToken`，并把取消时尚未提交的 action 保持为 generation-scoped、有序后缀；owner 抛出 `OperationCanceledException` 时映射为 `CommitPortCancelled`，不吞掉其他异常。
- `WorldGenerationActionExecutionResult` 与 `WorldGenerationActionResultProjection`：保留 `Cancelled`、`CancellationReason`、未提交后缀和完成/成功状态的一致语义。
- `WorldGenerationTilePlacementAndPaintActionsCommand`：依据完整参考 `Actions.SetTilePaint`、`SetWallPaint` 和 `SetTileAndWallPaint` 的 `paintID == 0` `Fail()` 分支拒绝零 paint；`ClearTilePaint`、`ClearWallPaint` 和 `ClearTileAndWallPaint` 已作为独立 operation 纳入 payload，但 terrain/placement owner 的 commit wiring 仍未闭合。`SetTileAndWallRainbowPaint` 仍为 `unknown`，不能用猜测实现 `WorldGen.GetRainbowPaintIDForPosition`。
- `WorldGenerationWallMutationActionsCommand`：增加 `RemoveWall` 独立 operation，保留 direct wall-zero 与带 framing 语义的 `ClearWall` 差异；terrain/wall owner 的 commit wiring 仍未闭合。
- focused verifier 源码包含 count/scanner、Continue、TileScanner、bounds、异常策略、framing port、diagnostic sink、缺失 owner、action cancellation barrier、owner cancellation exception、paint validation 以及本次新增 clear operation 断言；本轮已在成功构建产物上重新执行 verifier。

完整参考项目固定为 `D:\TRbackup\无任何删减通过编译`，目标源码固定为 `D:\TRbackup\Version4`，迁移目标目录固定为 `D:\TRbackup\NLTX\src\NSSLC`。本轮没有向 Version4、完整参考项目、权威 report、task table、ledger、lock 或 session 状态写入内容；生产代码和 focused verifier 的新增均在 NLTX 工作区内。

本执行文档的最终交付仍是 `proposed`/`partial` 计划；`unknown`、`partial` 和 `integration-review` 缺口保持原样。局部取消屏障只覆盖 P20 action commit 边界，不证明 P19 scheduler cancellation、旧 action chain cleanup 或跨分区回滚。文档完成、局部实现、局部编译或 focused verifier 记录都不构成迁移成功。

### 2026-10-01 参考语义纠偏记录

- 完整参考 `D:\TRbackup\无任何删减通过编译\Terraria.WorldBuilding\Actions.cs` 的
  `SetTileAndWallPaint.Apply` 在 `paintID == 0` 时与单独 tile/wall paint 一样进入 `Fail()`；此前把
  该值解释为组合清除的局部假设已移除。
- `WorldGenerationTilePlacementAndPaintActionsCommand` 与 focused verifier 现已对
  `SetTilePaint(0)`、`SetWallPaint(0)`、`SetTileAndWallPaint(0)` 一致拒绝；三个独立 clear action
  已加入 operation enum 和工厂方法。它们只表达有序 intent，尚未接入 terrain/placement owner，
  因此没有把 command 的存在称为清除行为已迁移。
- 完整参考中 `ClearTilePaint`、`ClearWallPaint`、`ClearTileAndWallPaint` 的直接效果分别是写入
  tile color 0、wall color 0、以及两个颜色 0 后执行 `UnitApply`。目标 CPG 对三种类型和 `Apply`
  surface 均返回 `complete`，但 callable facts 是 `partial/CalleeEffectsNotExpanded`；Rainbow
  action 也只有 `complete` surface 加 `partial` callable facts。该 evidence gap 不允许升级为
  目标运行时等价，也不允许实现 Rainbow 的位置/random policy。
- 重新执行仓库串行 build 与 `--no-build --no-restore` focused verifier：build 0 warning/0 error，
  verifier 输出 `PASS: P20 core action and structure reservation checks`。这仍是约 10% P20 核心边界证据，
  不改变权威报告的 `verificationStatus: not-run` 或 `migrationStatus: not-claimed`。

### 2026-10-01 clear paint operation 增量（当前延续）

- 本次只在 `D:\TRbackup\NLTX\src\NSSLC` 与 focused verifier 源码中加入三个独立 clear operation 的契约与断言；没有修改 Version4、完整参考项目、权威 report 或 runner 状态。
- 完整参考的清除动作直接写入 tile/wall paint 0 后继续 `UnitApply`；目标 CPG 对类型和 `Apply` surface 可定位为 `complete`，callable facts 为 `partial/CalleeEffectsNotExpanded`。因此当前实现仍是有序 intent，owner commit 和目标运行时等价保持 `unknown`。
- 本次增量已按仓库串行 wrapper 重新运行 `--no-build --no-restore` focused verifier，输出为 `PASS: P20 core action and structure reservation checks`、退出码 0。该结果仍只覆盖约 10% P20 核心边界；执行状态和实现状态保持 `partial`，权威报告的 `verificationStatus: not-run` 与迁移状态 `not-claimed` 不变。
