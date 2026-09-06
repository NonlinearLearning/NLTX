# WorldProgressionAndTransition Component Design

## 1. 设计元数据

~~~text
subsystemId: WorldProgressionAndTransition
taskNumber: 06
sourceReport: D:\TRbackup\NLTX\docs\research\2026-09-05-version4-world-progression-and-transition-public-decomposition.md
outputDesignPath: D:\TRbackup\NLTX\docs\design\2026-09-05-version4-world-progression-and-transition-component-design.md
designScope: component-only
designStatus: decision-required
evidenceStatus: partial
nltxStatus: partial
verificationStatus: not-run
selectionMethod: Version4-first；按状态语义、生命周期、权威 owner、恢复边界和访问字段子集分组；重新定位组件字段证据；不创建运行时实现
~~~

designStatus: decision-required 的原因是 Hardmode 早期规则值与最终提交值的关系、运行时字段与世界元数据字段的 owner、矿阶和 Tile 变更的组合边界仍未由本会话锁定。组件草图可以供架构评审使用，但不能被当作最终 schema 或现有实现。

本文件统计 9 个目标 Component。数量只包括第 5 节和第 12 节列出的目标状态载体，不把当前已有但相邻的世界描述、生成生命周期、区段存储或网络会话对象计入。

## 2. 设计范围与排除范围

### 2.1 设计范围

本设计只整理 WorldProgressionAndTransition 的世界级状态载体：

- Hardmode 规则事实及其与旧字段的兼容分离；
- 感染传播开关这一与 Hardmode 相关、但生命周期不同的世界规则；
- Hardmode 三阶矿种选择的长期状态；
- 一次世界过渡的身份、阶段和后台变换计数；
- 可复查过渡计划的只读数据；
- 提交结果的版本、批次和受影响区段摘要；
- 失败恢复所需的最小元数据；
- 运行时 Hardmode 兼容值与世界文件元数据兼容值；
- 世界、持久化、区段和外部会话 ID 的关系归类。

组件只保存数据和不变量所需的状态，不把 Tile 内容、世界物品、网络连接、随机实例、锁对象、异常对象或外部句柄塞入权威组件。

### 2.2 排除范围

本文件不重新设计 WorldGenerationAndEcology 的感染算法、WorldStorage 的 Tile 存储、ExternalBoundaries 的协议或 WorldSession 的全部规则集合。相邻领域只在组件字段的候选 owner、关系和证据中出现。

研究报告中的运行时结构、调用方向、持久化步骤、网络发送过程和 verifier 建议均不复制到本设计；若某个源码方法必须引用，只作为字段来源、生命周期或状态分类的短证据。

当前 Version4 的 Main.hardMode、WorldFileData.IsHardMode、单个 Tile、单个生成 pass、单个网络消息和背景效果都不单独成为一级 Component。理由在第 8 节集中说明。

## 3. 组件设计依据

### 3.1 来源优先级与本次读取

| sourceId | 实际来源 | 本次用途 | evidenceStatus |
| --- | --- | --- | --- |
| SRC-V4-RUNTIME | D:\TRbackup\Version4\Terraria\Main.cs:495；D:\TRbackup\Version4\Terraria\WorldGen.cs:3318-3332,4145-4151,4368,26052-26124；D:\TRbackup\Version4\Terraria\NPC.cs:65955-65965 | 确认 Hardmode 规则、矿阶默认值、变换计数、Wall of Flesh 触发和后台生命周期 | confirmed / partial |
| SRC-V4-STORAGE | D:\TRbackup\Version4\Terraria.IO\WorldFileData.cs:39-69,332-340；WorldFile.cs:96,479,905-929,1342-1355,2159,2395-2408 | 确认世界元数据、Hardmode 存档字段、三阶矿阶存档字段和保存等待边界 | confirmed / partial |
| SRC-V4-NETWORK | D:\TRbackup\Version4\Terraria\NetMessage.cs:282-290,2418-2431,2432-2450；Netplay.cs:134-143 | 只确认 Hardmode WorldData 位、区段缓存失效和缺失的区域重同步体 | confirmed / evidence-mismatch |
| SRC-FULL-REFERENCE | D:\TRbackup\无任何删减通过编译\Terraria\WorldGen.cs:32145-32325,49650-49700；对应 D:\TRbackup\无任何删减通过编译\Terraria\NPC.cs:80708-80718 | 只补证 Version4 已有同名成员的空体、完整成员邻域和版本漂移，不替代 Version4 主基线 | full-reference-supplemented / version-drift |
| SRC-TML | D:\TRbackup\tmodloader-api-docs-stable\index.html:8-10,27-31；class_mod_system.html:139-141,186-192,258-273,839-896,1078-1105；class_world_file_data.html:193-202 | 交叉核对公开 Hardmode 任务序列、世界生命周期、世界自定义数据、WorldData 和 IsHardMode 字段边界 | confirmed for public boundary |
| SRC-SS14 | C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\GameTicking\Components\GameRuleComponent.cs:9-31；Content.Server\Maps\MapMigrationSystem.cs:12-74 | 只参考状态组件粒度、规则生命周期数据和持久化 ID 分离，不推断 Terraria 行为 | reference-only |
| SRC-NLTX | D:\TRbackup\NLTX\src\WorldSession\WorldSessionComponents.cs:8-52；src\WorldSession\WorldGeneration\WorldGenerationLifecycleState.cs:3-15；src\WorldSession\WorldGeneration\WorldEcologyScheduleState.cs:3-20；dome\src\Terraria.Dome.Simulation\WorldGeneration\WorldGenerationProgressionState.cs:3-25 | 确认当前已有组件/状态以及目标组件的覆盖缺口 | existing-evidence |

本设计只使用当前会话已经明确产生的研究报告 SRC-REPORT：

D:\TRbackup\NLTX\docs\research\2026-09-05-version4-world-progression-and-transition-public-decomposition.md

该报告头部的 subsystemId 为 WorldProgressionAndTransition、taskNumber 为 06，且 reportPath 与实际路径一致。原研究报告保持不变。

### 3.2 字段证据的重新定位

| evidenceId | 重新定位的事实 | 对组件设计的约束 |
| --- | --- | --- |
| E-V4-HARDMODE-FIELD | Main.hardMode 是 bool，声明于 Main.cs:495，无显式初值，运行时默认值为 false；StartHardmode 在 WorldGen.cs:26084-26094 的实际上下文中先短路，再写 true | 规则事实必须与过渡阶段分开表达；不能把早期 true 自动解释为所有世界变更已经完成 |
| E-V4-ORE-DEFAULT | WorldGen.SavedOreTiers 的三阶字段在 WorldGen.cs:3318-3332 初始化为 107/108/111；世界清理实际在 :6485-6497 将包括三阶在内的矿阶和 Main.hardMode 重置 | 目标组件使用 -1 表示未初始化时必须标注为设计约定；不能把静态默认值和新世界清理值混为一谈 |
| E-V4-ORE-PERSISTENCE | WorldFile.cs:1353-1355 写入 Cobalt/Mythril/Adamantite，:2184-2186 读回；:2395-2407 处理低阶矿字段 | 三阶矿阶是长期世界状态，不能作为一次性随机结果或单个 Tile 字段 |
| E-V4-TRANSFORM-COUNT | _transformingWorld 是 int，声明于 WorldGen.cs:4145；TransformingWorld 在 :4368 由计数大于零派生；:26107-26122 增减计数并排队主线程后续动作 | 计数与派生布尔值共享过渡生命周期，但后台任务、锁和回调不属于 Component 字段 |
| E-V4-METADATA | WorldFileData.IsHardMode 是 bool，WorldFileData.cs:67；无效世界构造在 :336 明确设为 false；头部读取在 WorldFile.cs:479 | 元数据兼容值与活动世界规则值必须分开建模；最终同步 owner 未决 |
| E-V4-NETWORK | NetMessage.cs:287 将 Main.hardMode 写入 WorldData 位；Netplay.ResetSections 在 :134-143 清空区段缓存；ResyncTiles(int, Rectangle) 在 :2431 为空体 | 组件只能保存结果/区段摘要，不能拥有网络连接或假定重同步范围 |
| E-V4-RESET-MISMATCH | 研究报告曾引用 WorldGen.cs:7228-7240 作为重置证据，但当前文件该位置是 RandomizeWeather；实际重置为 :6485-6497 | 该旧行号记录为 evidence-mismatch，本设计仅采用当前实际行号 |
| E-NLTX-HARDMODE-ORE | 当前 WorldRulesState 将 HardMode、感染开关和 OreTierState 放在同一类 WorldSessionComponents.cs:40-52；dome 已有 HardmodeOreTierState.Uninitialized = (-1,-1,-1) | 现有容器可映射但不等于目标组件已经存在；按生命周期和 owner 拆分为多个组件 |

### 3.3 组件字段类型的说明

TransitionId、PlanId、WorldEntityId、WorldSectionId、WorldRevision、WorldSectionVersion、TransitionSectionPrecondition 及状态枚举是本设计中的候选值类型或稳定 ID。它们不是当前会话创建的代码类型；凡跨 WorldSession、WorldGenerationAndEcology、WorldStorage 或 ExternalBoundaries 使用者，均写明 crossSubsystemOwner: integration-review。

对于 Version4 没有对应字段的候选值，表中的默认值只是 Component 设计的候选初值，并标记 evidence-gap 或 decision-required，不被写成历史事实。

## 4. Version4 成员到 Component 归属表

| 成员 | 声明类型 | 字段含义 | 权威/派生/缓存/快照/兼容 | 生命周期 | proposed Component | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|---|
| Main.hardMode | Terraria.Main: bool | 活动世界是否已观察到 Hardmode | 当前权威规则值，同时是旧兼容值 | 世界加载、Wall of Flesh 触发、世界清理 | WorldHardmodeRuleState；RuntimeHardmodeCompatibilityState | confirmed | Version4\Terraria\Main.cs:495；WorldGen.cs:26088-26094,6497 |
| WorldFileData.IsHardMode | Terraria.IO.WorldFileData: bool | 世界文件头部/发现信息中的 Hardmode 元数据 | 兼容元数据，不是活动规则的唯一证明 | 世界文件读取、无效世界构造、世界发现 | WorldMetadataHardmodeState | partial | WorldFileData.cs:67,336；WorldFile.cs:479 |
| WorldGen.SavedOreTiers.Cobalt | Terraria.WorldGen.SavedOreTiers: int | 第一 Hardmode 矿阶 Tile 类型 | 权威长期状态 | 静态初始化、祭坛推进、存档读写、世界清理 | HardmodeOreTierState.CobaltTileType | partial | WorldGen.cs:3328,6489；WorldFile.cs:1353,2184 |
| WorldGen.SavedOreTiers.Mythril | Terraria.WorldGen.SavedOreTiers: int | 第二 Hardmode 矿阶 Tile 类型 | 权威长期状态 | 同上 | HardmodeOreTierState.MythrilTileType | partial | WorldGen.cs:3330,6490；WorldFile.cs:1354,2185 |
| WorldGen.SavedOreTiers.Adamantite | Terraria.WorldGen.SavedOreTiers: int | 第三 Hardmode 矿阶 Tile 类型 | 权威长期状态 | 同上 | HardmodeOreTierState.AdamantiteTileType | partial | WorldGen.cs:3332,6491；WorldFile.cs:1355,2186 |
| WorldRulesState.HardMode | WorldSessionComponents.WorldRulesState: bool | 当前 NLTX 世界规则中的 Hardmode 镜像 | 当前规则容器字段，目标权威来源未锁定 | 世界会话生命周期 | WorldHardmodeRuleState.IsHardMode | partial | src\WorldSession\WorldSessionComponents.cs:40-48 |
| WorldRulesState.InfectionSpreadAllowed | WorldRulesState: bool | 是否允许感染传播 | 权威规则候选 | 世界会话和生态更新周期 | WorldInfectionPolicyState.IsInfectionSpreadAllowed | partial | src\WorldSession\WorldSessionComponents.cs:44-48 |
| WorldEcologyScheduleState.IsInfectionSpreadAllowed | WorldEcologyScheduleState: bool | 生态调度读取的感染开关 | 派生/调度镜像候选，不应与规则源无声明地双写 | 生态更新周期 | WorldInfectionPolicyState 的读取镜像 | partial | src\WorldSession\WorldGeneration\WorldEcologyScheduleState.cs:3-20 |
| WorldGen._transformingWorld | Terraria.WorldGen: int | 当前后台世界变换计数 | 运行时门控状态 | 每个后台变换的增加、finally 减少 | ProgressionTransitionState.ActiveTransformationCount | confirmed | WorldGen.cs:4145,26107-26119 |
| WorldGen.TransformingWorld | Terraria.WorldGen: bool | _transformingWorld > 0 的派生值 | 派生值，不单独持久化 | 存档等待和世界更新门控 | ProgressionTransitionState.IsTransforming | confirmed | WorldGen.cs:4368；WorldFile.cs:905-907 |
| WorldGen.isGeneratingOrLoadingWorld | Terraria.WorldGen: volatile bool | 生成或加载期间的世界门控 | 运行时派生/门控，属于相邻生成生命周期 | 加载、生成、Ready 转换 | 现有 WorldGenerationLifecycleState，不新建 Hardmode Component | confirmed | WorldGen.cs:4151；src\WorldSession\WorldGeneration\WorldGenerationLifecycleState.cs:3-15 |
| WorldGenerationProgressionState.GenerationId | long | 当前生成事务的标识 | 生成快照字段，不是 Hardmode 事务 ID | 一次世界生成 | 现有 WorldGenerationProgressionState；作为 TransitionPlanState 的候选基线关系 | confirmed | dome\src\Terraria.Dome.Simulation\WorldGeneration\WorldGenerationProgressionState.cs:21-25 |
| WorldGenerationProgressionState.GenerationCursor | long | 生成快照游标 | 生成快照/进度值 | 一次世界生成 | 现有生成状态；不得直接改名为 Hardmode cursor | confirmed | 同上 |
| WorldProgressionState.IsHardMode | bool | dome 常规事件状态中的 Hardmode 布尔值 | 不可变快照/兼容镜像 | 每次值替换 | WorldHardmodeRuleState 的只读映射，不作为第二写入根 | partial | dome\src\Terraria.Dome.Simulation\World\WorldProgressionState.cs:9-20,116-168 |
| WorldProgressionTransition | record struct | 血月、日食、灯笼夜等常规事件的开始/停止记录 | 事件结果记录，不是 Hardmode 长期状态 | 单个常规事件记录 | 不归入目标 Component | confirmed | dome\src\Terraria.Dome.Simulation\World\WorldProgressionTransition.cs:3-8 |
| WorldSection version | long 候选 | 区段内容版本，用于发现计划基线与提交结果 | 存储权威/快照引用 | 区段变更时递增 | TransitionPlanState.SectionPreconditions；ProgressionCommitState.ChangedSections | partial | LegacyTileRunnerCommandCommitBoundary.cs:85-149,337-344；WorldSectionReplication.cs:99-113 |

表中 proposed Component 一栏只表示目标归属；当前 NLTX 的 WorldRulesState、WorldGenerationLifecycleState、WorldEcologyScheduleState 和 dome 记录并没有因此被宣称已经转换为目标 Component。

## 5. Component 定义

以下 9 个定义全部是设计提案。Component 只保存数据，不包含运行时行为实现。componentOwner 是候选 owner；出现 crossSubsystemOwner: integration-review 时，本会话不宣布最终归属。

### 5.1 WorldHardmodeRuleState

#### 职责

保存活动世界的 Hardmode 长期规则事实，以及为了兼容 Version4 早期规则写入而需要的最小阶段区分。它不保存 Tile 变更、不保存后台对象，也不保存网络或存档句柄。

~~~text
componentId: C-WPT-01
name: WorldHardmodeRuleState
status: proposed
componentOwner: WorldSession candidate；规则写入由 WorldProgressionAndTransition 协调
crossSubsystemOwner: integration-review
entityScope: 活动 WorldEntity
lifecycle: 世界创建时初始化；加载时从世界规则恢复；Hardmode 过渡期间更新；世界清理/卸载时重置或销毁
~~~

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| IsHardMode | bool | false | 权威状态；同时有 Version4 兼容投影 | 活动世界内不能从 true 静默回到 false；清理是新生命周期，不是普通更新 | confirmed for field, partial for phase semantics | Version4\Terraria\Main.cs:495；WorldGen.cs:26088-26094,6497 |
| RulePhase | HardmodeRulePhase | PreHardmode | proposed 阶段区分 | PreHardmode、Announced、Committed 互斥；Announced 不得被当作 Tile 变更已完成 | evidence-gap | Version4 只有 Main.hardMode，没有阶段字段；早期写入见 WorldGen.cs:26092 |
| LastCommittedTransitionId | TransitionId? | empty | 权威幂等关联值候选 | 只有 Committed 结果可写入；空值表示尚无可关联提交 | evidence-gap | Version4 没有对应 ID；研究报告只提出候选 |
| LastCommittedSequence | long | 0 | 权威提交序列候选 | 非负且单调不减；不得用它代替 TransitionId | evidence-gap | Version4 未提供全局提交序列 |

#### 字段不变量

- IsHardMode == false 时，RulePhase 只能是 PreHardmode，除非整合会话明确允许双阶段表示。
- RulePhase == Committed 时，IsHardMode 必须为 true。
- 最后提交关联只能描述已提交结果，不能记录仅请求、计划或失败的过渡。
- WorldFileData.IsHardMode 和 WorldProgressionState.IsHardMode 不得在没有明确同步规则的情况下成为第二写入根。

#### 生命周期

创建世界时以 PreHardmode/false 初始化；加载时以有效世界规则恢复；过渡开始时是否进入 Announced 取决于 BD-COMP-01；最终提交后才允许写入 Committed 和最后提交关联；世界清理时随活动世界销毁或重新初始化。

#### Entity/World 范围

每个活动世界一个，不附着到 Wall of Flesh、玩家、NPC、Tile 或区段实体。常规事件实体不得因为也有 IsHardMode 字段而组合此 Component。

#### ID 与关系字段

LastCommittedTransitionId 指向同一 WorldEntity 上的过渡记录。TransitionId 跨持久化和外部结果使用，crossSubsystemOwner: integration-review；本 Component 不重复保存 WorldId 或 UniqueId。

#### 当前 NLTX 映射

src\WorldSession\WorldSessionComponents.cs:40-48 的 WorldRulesState.HardMode 和 dome\src\Terraria.Dome.Simulation\World\WorldProgressionState.cs:116-168 的 IsHardMode 只能提供 partial 映射。前者是可变规则容器，后者是不可变事件状态；当前没有阶段、最后提交 ID 或唯一写入根。

#### 证据

Version4 在 WorldGen.StartHardmode 中先写 Main.hardMode = true，而完整参考在同名文件补出客户端/服务端守卫。这证明字段和入口存在，但不能证明 RulePhase 或提交时机；因此该 Component 为 proposed，整体设计保持 decision-required。

### 5.2 WorldInfectionPolicyState

#### 职责

保存感染传播是否允许这一可被生态更新读取的世界规则。它与 Hardmode 规则相关，但不是 Hardmode 过渡的全部结果，也不承载感染 Tile 内容。

~~~text
componentId: C-WPT-02
name: WorldInfectionPolicyState
status: proposed
componentOwner: WorldSession / WorldGenerationAndEcology candidate
crossSubsystemOwner: integration-review
entityScope: 活动 WorldEntity
lifecycle: 世界创建时初始化；加载和规则覆盖时恢复或更新；世界卸载时销毁
~~~

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| IsInfectionSpreadAllowed | bool | true | 权威规则候选 | 只能表达允许/禁止；不得把当前生态预算或某个 Tile 状态塞入该字段 | partial | src\WorldSession\WorldSessionComponents.cs:44-48；WorldEcologyScheduleState.cs:3-20 |

#### 字段不变量

- 该字段是世界范围的规则值，不能因为某个区段没有预算就被自动改写为 false。
- WorldEcologyScheduleState.IsInfectionSpreadAllowed 只能是有声明来源的派生读取值；不能与本 Component 无记录地双写。
- Hardmode 尚未提交时是否允许传播由规则语义决定，不能从字段名字推导；该关系属于 GAP-COMP-08。

#### 生命周期

新世界默认允许传播；加载时恢复；规则覆盖变化时更新；生态调度读取但不改变权威值；活动世界清理时销毁。

#### Entity/World 范围

每个活动世界一个。不要为每个感染 Tile 创建此 Component；Tile 仍是 WorldStorage 的实例数据。

#### ID 与关系字段

不拥有独立 ID。通过活动 WorldEntityId 与 WorldHardmodeRuleState 关联；规则状态和生态调度跨两个子系统，crossSubsystemOwner: integration-review。

#### 当前 NLTX 映射

WorldRulesState.InfectionSpreadAllowed 已存在，status: partial；WorldEcologyScheduleState 也保存同名字段和预算/采样游标，局部调度状态为 status: existing。当前没有证明哪个字段是唯一写入根，因此目标 Component 仍为 proposed。

#### 证据

Version4 报告只确认 Hardmode 后生态门控读取 Main.hardMode，没有确认完整感染转换体；完整参考的 initializeHardMode 只能补出同名算法形状。该 Component 只保留已能确认的规则开关，不扩大为感染算法设计。

### 5.3 HardmodeOreTierState

#### 职责

保存三个 Hardmode 矿阶对应的 Tile 类型，作为世界长期状态。低阶 Copper/Iron/Silver/Gold 与三阶 Hardmode 矿种虽然在 Version4 的 SavedOreTiers 中同属一个静态类，但生命周期和使用面不同，不在本 Component 中重新聚合。

~~~text
componentId: C-WPT-03
name: HardmodeOreTierState
status: proposed
componentOwner: WorldGenerationAndEcology candidate；长期写入与恢复涉及 WorldStorage
crossSubsystemOwner: integration-review
entityScope: 活动 WorldEntity
lifecycle: 新世界以未初始化值创建；祭坛推进或等价世界进度时选择；存档恢复；世界清理时重置
~~~

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| CobaltTileType | int | -1（目标未初始化约定） | 权威长期状态 | 只能是 -1、默认类型 107 或候选替代类型 221；具体合法值集合仍受版本规则约束 | partial | WorldGen.cs:3328,6489；dome HardmodeOreTierState.cs:3-8；HardmodeOreTierSelectionPolicy.cs:20-57,99-104 |
| MythrilTileType | int | -1（目标未初始化约定） | 权威长期状态 | 只能是 -1、108 或 222，除非版本规则另有已证实扩展 | partial | WorldGen.cs:3330,6490；dome 同上 |
| AdamantiteTileType | int | -1（目标未初始化约定） | 权威长期状态 | 只能是 -1、111 或 223，除非版本规则另有已证实扩展 | partial | WorldGen.cs:3332,6491；dome 同上 |

Version4 静态初始化值为 107/108/111，而当前 dome 值对象把 (-1,-1,-1) 定义为 Uninitialized。因此表中 -1 是目标组件的初始化语义，不是声称 Version4 静态字段的默认值已经变成 -1。

#### 字段不变量

- 三个字段必须作为一个同生命周期的矿阶值集合恢复；不能只恢复其中一阶而把另外两阶默认为当前随机结果。
- -1 表示尚未选择，不表示一个合法 Tile 类型。
- 选择结果和祭坛计数有关，但 AltarCount 不属于本 Component；它是相邻世界进度状态的输入关系。
- 是否与感染/墙体 Tile 变更处于同一提交边界尚未决定，不能在组件层面伪造原子性。

#### 生命周期

新世界创建或兼容旧存档时按版本规则初始化；Hardmode 相关矿阶选择时产生新值；存档时持久化；加载时校验；世界清理时按实际世界生命周期重置。读取失败或非法类型不得静默归零。

#### Entity/World 范围

每个活动世界一个，不附着于矿石 Tile、祭坛 Tile 或单个生成 pass。Tile 类型是规则引用，不是 Tile 实例的拥有者。

#### ID 与关系字段

不拥有独立 ID。通过 WorldEntityId 关联 AltarCount 和世界规则；三阶状态可能被 WorldStorage 和 WorldGenerationAndEcology 共同读写，crossSubsystemOwner: integration-review。

#### 当前 NLTX 映射

src\WorldSession\WorldSessionComponents.cs:48 的 OreTierState 包含七个低阶和高阶字段，属于 status: partial 的过宽容器。dome 的 HardmodeOreTierState 和选择策略已存在，分别是现有值类型/策略证据；尚未作为活动世界 Component、持久化 owner 或过渡结果的一部分闭合。

#### 证据

Version4 WorldGen.SavedOreTiers 和 WorldFile 的三阶读写确认长期状态性质；完整参考 SmashAltar 补出随机选择和 Drunk World 切换形状，但不回填 Version4 空的 initializeHardMode。静态默认值与清理值的冲突记录为 partial，不是行为等价结论。

### 5.4 ProgressionTransitionState

#### 职责

保存一个活动 Hardmode 世界过渡的身份和生命周期状态。它只表达过渡是否存在、处于哪个阶段、关联哪个世界和计划，以及后台变换的计数；不保存计划内容、Tile 批数据或恢复错误详情。

~~~text
componentId: C-WPT-04
name: ProgressionTransitionState
status: proposed
componentOwner: WorldProgressionAndTransition candidate
crossSubsystemOwner: integration-review
entityScope: 活动 WorldEntity；同一世界至多一个活动 Hardmode transition record
lifecycle: 请求时创建或激活；计划建立时关联计划；执行/提交/失败/恢复时更新；终态确认后保留最小结果关联或清理
~~~

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| TransitionId | TransitionId | empty | 权威身份 | 活动过渡非空且在该世界内唯一；同一 ID 不得代表两个不同计划 | evidence-gap | Version4 无过渡 ID；研究报告提出候选 |
| WorldEntityId | WorldEntityId | empty | 关系字段 | 必须指向承载本 Component 的活动世界，不得指向客户端实体 | evidence-gap | current NLTX 世界关系，owner 未统一 |
| TransitionKind | WorldProgressionTransitionKind | None | 权威分类 | 当前目标值为 Hardmode；其他长期过渡必须另行纳入固定子系统清单 | partial | 任务边界；Version4 StartHardmode |
| Phase | TransitionPhase | Idle | 权威生命周期 | Idle 无活动 ID；活动阶段只能是 Requested、Planned、Executing、ReadyToCommit、Committed、Failed、Recovering 之一 | evidence-gap | Version4 只有 _transformingWorld 计数，无显式阶段 |
| RequestedAtTick | long | 0 | 权威审计候选 | 非负；不用于替代 Wall of Flesh 事件或世界时间语义 | evidence-gap | Version4 未保存请求 Tick |
| PlanId | PlanId? | null | 关系字段 | 进入 Planned 及以后时必须有有效关联；Idle/Requested 可为空 | evidence-gap | Version4 没有计划对象 |
| ActiveTransformationCount | int | 0 | 运行时门控状态 | 非负；IsTransforming 由其大于零派生；同一世界的计数不能因重复后续动作负增长 | confirmed for legacy counter shape | WorldGen.cs:4145,26107-26119 |
| IsTransforming | bool | false | 派生值/兼容字段 | 必须等价于 ActiveTransformationCount > 0，不单独持久化 | confirmed for legacy derivation | WorldGen.cs:4368 |

#### 字段不变量

- Phase == Idle 时，TransitionId、PlanId 和活动计数必须为空/零。
- Phase == Committed 时，必须存在有效的 TransitionId，但是否保留该记录由清理策略决定。
- 同一世界不能有两个同时有效的 Hardmode TransitionId；force 不能绕过该身份约束而制造重复状态。
- ActiveTransformationCount 是对 Version4 后台计数的兼容表达，不应把 Task、锁或线程句柄作为字段。

#### 生命周期

请求被接受时建立 Requested；关联计划后进入 Planned；存在后台变换时进入 Executing；必要数据具备提交条件时进入 ReadyToCommit；结果确认后进入 Committed，失败或结果不明则进入 Failed/Recovering。这些阶段名称是 proposed 数据枚举，不是已创建代码。

#### Entity/World 范围

每个活动世界最多一个活动实例。不要为每个后台任务、区段、Tile 或网络客户端复制该 Component。

#### ID 与关系字段

TransitionId -> WorldEntityId 是单向关系；PlanId 指向同一过渡的 TransitionPlanState。这些 ID 供存档和外部边界消费，crossSubsystemOwner: integration-review。

#### 当前 NLTX 映射

dome\src\Terraria.Dome.Simulation\WorldGeneration\WorldGenerationProgressionState.cs:17-25 已有 IsTransforming、ActiveTransformations、GenerationId 和游标，但它是生成进度快照，status: partial，没有 Hardmode TransitionId、阶段、计划关系或失败恢复元数据。

#### 证据

Version4 TransformWorldOnBackgroundThread 的计数和 WorldFile 等待逻辑确认运行时门控的生命周期；finally 排队后续动作但不提供成功/失败结果。其余身份和阶段字段均为 evidence-gap 下的候选设计。

### 5.5 TransitionPlanState

#### 职责

保存一个不可变、可复查的 Hardmode 过渡计划摘要：计划身份、生成基线、随机游标、受影响区段、区段前置版本、批次数量和矿阶候选。它不拥有实际 Tile 内容，也不承担执行中的可变游标。

~~~text
componentId: C-WPT-05
name: TransitionPlanState
status: proposed
componentOwner: WorldProgressionAndTransition / WorldGenerationAndEcology candidate
crossSubsystemOwner: integration-review
entityScope: 活动 WorldEntity 上与一个 ProgressionTransitionState 关联
lifecycle: 计划确定时创建；计划阶段内只读；过渡终态后作为审计快照保留或按存储策略清理
~~~

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| PlanId | PlanId | empty | 权威计划身份 | 非空计划必须唯一；同一 ID 的字段不可变 | evidence-gap | Version4 没有 Plan 对象 |
| TransitionId | TransitionId | empty | 关系字段 | 必须与 ProgressionTransitionState.TransitionId 相等 | evidence-gap | Version4 无对应字段 |
| BaseWorldRevision | WorldRevision? | null | 快照基线 | 计划生效前必须有明确世界版本；未知不可伪装为零 | evidence-gap | NLTX 有区段版本，未有 Hardmode 全局版本证据 |
| BaseGenerationRevision | ulong | 0 | 快照/关系字段 | 只引用生成生命周期修订，不可当作世界内容版本 | partial | WorldGenerationLifecycleState.GenerationRevision:7 |
| RandomCursor | ulong | 0 | 可重放计划元数据候选 | 只能由计划快照记录；不得保存可变随机实例 | evidence-gap | 完整参考使用 genRand，Version4 无 cursor |
| AffectedSections | ImmutableArray<WorldSectionId> | 空集合 | 快照索引 | 不重复；每个区段坐标必须在世界范围内；为空则不能声称有 Tile footprint | partial | NLTX section coordinates/replication；Version4 ResetSections 只清缓存 |
| SectionPreconditions | ImmutableArray<TransitionSectionPrecondition> | 空集合 | 快照前置条件 | 每个受影响区段最多一个基线版本；必须覆盖所有计划写入区段 | partial | 局部提交的 expected version 证据 |
| ExpectedBatchCount | int | 0 | 快照统计 | 非负；完成批次不得超过总批次 | evidence-gap | Version4 没有批次记录 |
| OreTierCandidate | HardmodeOreTierState | Uninitialized | 计划候选快照 | 只是计划候选；未提交前不能覆盖权威矿阶状态 | partial | dome HardmodeOreTierSelection.cs:3-7；Version4 SavedOreTiers |
| RequiresResync | bool | false | 派生/决策字段 | 只有明确受影响区段或规则变更才可为 true；范围未决 | partial | Netplay.ResetSections confirmed；ResyncTiles body missing |

#### 字段不变量

- 计划一旦与 PlanId 关联，其区段集合、前置版本和矿阶候选不得被执行中的可变状态覆写。
- 计划不会把每个 Tile 作为长期关系；Tile 变更只能以区段 footprint 或不可变批次摘要出现。
- BaseWorldRevision、区段前置版本和生成修订的比较语义需要由跨域 owner 统一，不能在此 Component 内自行定义版本递增规则。
- 计划的随机信息只用于重放和审计，不得保存 UnifiedRandom、线程或外部资源。

#### 生命周期

计划建立前不存在；建立时一次性填充；执行和失败恢复只读取；若计划不可再用则标记为过期并与过渡记录一同清理。由于没有 Version4 计划对象，该生命周期完全是 proposed。

#### Entity/World 范围

每个活动过渡至多一个。它是 WorldEntity 范围的不可变快照，不附着到每个 Section 或 Tile；区段只通过 WorldSectionId 关系被引用。

#### ID 与关系字段

PlanId 与 TransitionId 是跨存储和过渡生命周期的 ID；WorldSectionId 和 WorldRevision 连接 WorldStorage。全部标记 crossSubsystemOwner: integration-review。

#### 当前 NLTX 映射

当前没有 Hardmode plan component。WorldGenerationPipeline 的阶段和局部提交、HardmodeOreTierSelectionPolicy 的选择值以及 WorldGrid 区段版本只能作为 partial 组成证据；它们不能被拼接成已存在的计划。

#### 证据

公开 tModLoader 文档的 ModifyHardmodeTasks(List<GenPass>)（class_mod_system.html:139-141）仅支持 Hardmode 有序任务集合的公开边界；它不证明 Version4 私有计划字段。因此本 Component 的计划字段保持 evidence-gap/partial。

### 5.6 ProgressionCommitState

#### 职责

保存过渡提交的不可变结果元数据和批次计数，供权威状态恢复时判断哪些内容已确认、哪些区段版本已改变。它不保存 Tile 内容，也不把外部发送状态冒充为世界规则。

~~~text
componentId: C-WPT-06
name: ProgressionCommitState
status: proposed
componentOwner: WorldStorage / WorldProgressionAndTransition candidate
crossSubsystemOwner: integration-review
entityScope: 活动 WorldEntity；结果摘要可被持久化快照引用
lifecycle: 过渡准备提交时创建；每次确认批次时更新；成功、失败或未知结果时冻结；新世界生命周期清理
~~~

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| CommitStatus | TransitionCommitStatus | NotStarted | 权威提交状态候选 | Committed、Failed、Unknown 互斥；未知不能被当作失败后自动重做 | evidence-gap | Version4 无全局提交状态 |
| CommitSequence | long | 0 | 权威序列候选 | 非负；同一 TransitionId 内单调递增；恢复不得重复占用序列 | evidence-gap | Version4 无序列字段 |
| AppliedBatchCount | int | 0 | 提交进度快照 | 0 <= AppliedBatchCount <= TotalBatchCount | evidence-gap | Version4 无批次记录 |
| TotalBatchCount | int | 0 | 计划统计镜像 | 非负；应与计划的 ExpectedBatchCount 一致 | evidence-gap | proposed plan only |
| WorldRevisionBefore | WorldRevision? | null | 快照 | 未确认基线时保持空；不以 0 猜测版本 | evidence-gap | NLTX 有 section version，缺 Hardmode global revision |
| WorldRevisionAfter | WorldRevision? | null | 快照 | 仅 Committed 才允许非空；必须不小于 before | evidence-gap | 同上 |
| ChangedSections | ImmutableArray<WorldSectionVersion> | 空集合 | 权威结果快照 | 每个区段一次；版本必须对应同一提交结果 | partial | NLTX WorldGrid section snapshot/version |
| IsResultKnown | bool | false | 权威结果状态 | true 时 CommitStatus 不能是 NotStarted；Unknown 时仍可为 false | evidence-gap | Version4 未显式表达异常结果 |

#### 字段不变量

- 只记录已确认的提交元数据，不记录网络发送成功、聊天或成就副作用。
- ChangedSections 是结果摘要，不拥有区段内容；区段版本最终 owner 仍是 WorldStorage 候选。
- 是否把 HardmodeOreTierState 与 Tile 变更置于同一记录由 BD-COMP-03 决定，当前不能添加隐含的“必然原子”字段。
- CommitStatus == Unknown 时不得产生表示成功的兼容快照。

#### 生命周期

在计划具备提交条件时建立；确认必要批次后更新计数；所有必要结果确认后冻结为 Committed；冲突或异常则冻结为 Failed 或 Unknown，并与恢复数据关联。清理策略未锁定。

#### Entity/World 范围

每个活动过渡一个世界级记录，不为每个 Tile 或网络客户端复制。

#### ID 与关系字段

通过 TransitionId、PlanId 关联过渡和计划，通过 WorldSectionId/WorldSectionVersion 关联存储区段。上述类型跨域使用，crossSubsystemOwner: integration-review。

#### 当前 NLTX 映射

当前 NLTX 只有感染/Tile/Liquid 的局部提交边界和 section version 检查，没有 Hardmode 全局提交记录。WorldSectionReplication 与 WorldSaveCoordinator 可以提供快照读写能力，但没有该 Component 的状态。

#### 证据

Version4 WorldFile.IOLock 和 ResetSections 只确认锁、等待和缓存失效边界，不能证明跨 Tile、矿阶、规则和外部投影的原子结果。因此本 Component 保持 evidence-gap 较多的 proposed 状态。

### 5.7 TransitionRecoveryState

#### 职责

保存失败、崩溃或结果不明时恢复所需的最小元数据。它不保存异常对象、线程对象、重试函数或可变外部资源。

~~~text
componentId: C-WPT-07
name: TransitionRecoveryState
status: proposed
componentOwner: WorldProgressionAndTransition candidate
crossSubsystemOwner: integration-review
entityScope: 活动 WorldEntity；可在持久化恢复快照中表示
lifecycle: 首次出现可恢复失败时创建；恢复尝试时更新；成功提交或明确终止后冻结并清理
~~~

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| RecoveryPhase | RecoveryPhase | None | 权威恢复状态候选 | None、Recoverable、Terminal 互斥 | evidence-gap | Version4 无恢复状态 |
| LastSafePhase | TransitionPhase? | null | 恢复快照 | 只能指向已确认阶段，不能把未知批次写成安全点 | evidence-gap | Version4 无阶段记录 |
| PendingBatchIndexes | ImmutableArray<int> | 空集合 | 恢复快照 | 索引非负、无重复，且不能包含已确认批次 | evidence-gap | Version4 无批次记录 |
| FailureCode | TransitionFailureCode? | null | 权威错误分类 | 只保存稳定分类，不保存异常对象或外部路径 | evidence-gap | Version4 Task 异常传播未闭合 |
| RetryCount | int | 0 | 权威恢复元数据 | 非负；重启恢复不能无声重置；上限需整合裁决 | evidence-gap | Version4 只有 force 短路，无计数 |
| ResultUnknown | bool | false | 权威不确定性标记 | true 时不能发布成功结果；需与 ProgressionCommitState 一致 | evidence-gap | TransformWorldOnBackgroundThread 的 finally 证据 |
| RecoveryRevision | long | 0 | 恢复快照版本候选 | 非负且单调；与世界持久化快照的版本关系未定 | evidence-gap | Version4 无对应字段 |

#### 字段不变量

- ResultUnknown 优先于重试意图；未知结果必须先判定已应用批次，不能直接重复感染或重复矿阶切换。
- PendingBatchIndexes 不能成为 Tile 内容的第二副本，只保存可恢复索引。
- 恢复 Component 与正常过渡 Component 同属一个 WorldEntity，但不能把失败状态覆盖为 Committed。

#### 生命周期

无失败时不存在或为 None；部分批次失败、后台异常或重启恢复时创建；恢复完成后转为终止或随成功结果冻结；新世界清理时销毁。Version4 的异常结果不足以选择具体恢复策略。

#### Entity/World 范围

每个活动过渡最多一个，不附着于异常、Task、区段或客户端。

#### ID 与关系字段

使用所属 TransitionId 和 PlanId 关联提交记录；恢复快照的 PersistentWorldId 关系由存储域候选持有，crossSubsystemOwner: integration-review。

#### 当前 NLTX 映射

当前 WorldGenerationLifecycleState.Failure 只能表示世界生成失败，dome 生成快照也不能表示 Hardmode 部分批次或结果未知；目标 Component 没有实际实现，映射为 missing。

#### 证据

Version4 TransformWorldOnBackgroundThread 在 finally 中减计数并排队后续动作，但没有显式成功/失败结果。该事实支持恢复元数据的必要性，却不能支持任何具体错误枚举值；字段保持 evidence-gap。

### 5.8 RuntimeHardmodeCompatibilityState

#### 职责

隔离 Version4 Main.hardMode 这一旧运行时观察值，使其不再与目标权威规则事实形成无声明的双写。它只表达兼容投影，不拥有 Hardmode 的业务权威性。

~~~text
componentId: C-WPT-08
name: RuntimeHardmodeCompatibilityState
status: proposed
componentOwner: WorldSession candidate
crossSubsystemOwner: integration-review
entityScope: 活动 WorldEntity 的兼容视图
lifecycle: 活动世界加载时建立；兼容字段投影更新时改变；卸载或世界清理时清除
~~~

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| LegacyHardModeValue | bool | false | 兼容字段/运行时投影 | 只能由明确的规则状态投影更新；不得被客户端或普通表现层写入 | confirmed for source field, evidence-gap for target owner | Version4\Terraria\Main.cs:495；NetMessage.cs:287 |

#### 字段不变量

- 该字段与 WorldHardmodeRuleState.IsHardMode 的差异必须可解释；不同值只能在整合会话批准的过渡阶段出现。
- 新的业务读者不得把它当作 Tile、矿阶和存档都已提交的证明。
- 不再增加第三个 Hardmode 布尔镜像。

#### 生命周期

加载时从兼容来源初始化；早期 Hardmode 观察值变化时投影；最终规则提交时对齐；清理时回到 false 或随实体销毁。实际对齐时点由 BD-COMP-01 和 BD-COMP-05 决定。

#### Entity/World 范围

只附着于活动 WorldEntity，不附着于客户端 WorldData 包或单个 NPC。

#### ID 与关系字段

通过 WorldEntityId 指向权威规则 Component；不保存 WorldId/UniqueId 副本。由于它被运行时、存档和网络读者共同观察，crossSubsystemOwner: integration-review。

#### 当前 NLTX 映射

当前没有单独兼容 Component；WorldRulesState.HardMode 与 dome WorldProgressionState.IsHardMode 是 partial 镜像。Version4 的 Main.hardMode 仍是直接运行时字段。

#### 证据

Version4 直接写入并在 WorldData 中读取 Main.hardMode，足以证明兼容字段存在；没有证据证明新目标 owner 或双阶段关系，不能标为 baseline。

### 5.9 WorldMetadataHardmodeState

#### 职责

保存世界描述/世界文件元数据范围内的 Hardmode 值，与活动世界的规则 Component 和运行时兼容值隔离。它不拥有活动模拟状态。

~~~text
componentId: C-WPT-09
name: WorldMetadataHardmodeState
status: proposed
componentOwner: WorldStorage candidate；世界发现读取涉及 ExternalBoundaries
crossSubsystemOwner: integration-review
entityScope: WorldMetadataEntity / 世界描述对象
lifecycle: 创建世界描述时初始化；世界头部读取时恢复；元数据刷新时更新；无效世界/卸载时清理
~~~

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| IsHardMode | bool | false | 持久化/发现元数据兼容字段 | 无效世界默认为 false；不能单独使活动 WorldEntity 进入 Hardmode | partial | WorldFileData.cs:67,336；WorldFile.cs:479；服务器发现读取见研究报告 V4-ServerDiscoveryHardmode |

#### 字段不变量

- 此字段只能描述元数据快照，不得反向写入活动规则。
- 其值与 RuntimeHardmodeCompatibilityState.LegacyHardModeValue 的同步方向、版本和恢复优先级必须由 BD-COMP-05 确定。
- 若头部缺失或版本太旧，必须保留明确的未知/兼容处理，不得把未知默认为已经进入 Hardmode。

#### 生命周期

创建世界描述时为 false；从头部读取时按文件版本恢复；无效世界构造按 Version4 FromInvalidWorld 的 false 初始化；活动世界提交后的刷新时机未由 Version4 闭合；卸载时清理。

#### Entity/World 范围

附着于世界元数据对象，而不是活动 WorldEntity、区段或客户端会话。活动世界和元数据可以通过 PersistentWorldId 关联，但不合并存储范围。

#### ID 与关系字段

不复制 WorldFileData.WorldId 和 UniqueId；通过现有 WorldDescriptorState.WorldId/UniqueId 或最终整合确定的 PersistentWorldId 关联。WorldId、UniqueId、PersistentWorldId 的协议角色仍为 crossSubsystemOwner: integration-review。

#### 当前 NLTX 映射

WorldDescriptorState 有 WorldId、UniqueId、尺寸和准备状态，但没有 IsHardMode；WorldSaveCoordinator 能保存 WorldGrid snapshot，却未接 Hardmode metadata。目标映射为 missing/partial，不能因为 Version4 字段存在就标为现有组件。

#### 证据

Version4 头部读写、无效世界默认值和服务器发现读取证明元数据字段存在；公开 tModLoader WorldFileData.IsHardMode（class_world_file_data.html:193-202）只交叉验证公开字段，不确定私有同步时序。

## 6. Entity 与 Component 组合

以下组合只描述状态范围和组合约束，不描述运行时调用或执行顺序。

| Entity/World 对象 | 必需 Component | 可选 Component | 互斥 Component | 组合理由 |
|---|---|---|---|---|
| 活动 WorldEntity | 现有 WorldDescriptorState；WorldHardmodeRuleState；WorldInfectionPolicyState；HardmodeOreTierState；ProgressionTransitionState；ProgressionCommitState | TransitionPlanState；TransitionRecoveryState；RuntimeHardmodeCompatibilityState | WorldMetadataHardmodeState 不附着在此对象 | 这些状态都以活动世界为范围，但规则、过渡、计划、提交和恢复的生命周期不同；按概念分开可避免巨型世界 Component |
| 计划中的活动 WorldEntity | 上行必需组件；TransitionPlanState | TransitionRecoveryState | 无 | 计划只在有活动过渡时存在；恢复可选，因为无失败时不需要恢复元数据 |
| 恢复中的活动 WorldEntity | 上行必需组件；ProgressionTransitionState；ProgressionCommitState；TransitionRecoveryState | TransitionPlanState | RecoveryPhase == None 与 Recoverable/Terminal 互斥 | 恢复状态必须与同一过渡、计划和提交摘要共存，不能挂到另一个世界 |
| WorldMetadataEntity | 现有 WorldDescriptorState；WorldMetadataHardmodeState | 无 | 活动过渡组件不附着在该对象 | 元数据生命周期独立于活动模拟；避免元数据反向拥有活动规则 |
| WorldSection | 现有 WorldGrid 区段内容和区段版本状态 | 不新增目标 Component；TransitionSectionPrecondition 只作为计划内值 | 不附着 ProgressionTransitionState 或 TransitionRecoveryState | 区段是存储实例范围；计划只引用区段 ID 和版本，不复制过渡生命周期 |
| Tile | 现有 Tile 实例数据 | 不新增 Hardmode Component | 不附着世界级过渡 Component | 单个 Tile 没有独立的 Hardmode 生命周期；其变化由区段摘要和世界结果间接记录 |
| 世界物品集合 | 现有 WorldItem 实例状态 | 不新增长期保护 Component | 不附着 ProgressionTransitionState | Version4 保护逻辑只临时改变 timeSinceItemSpawned；资格实现为空，不能升级为长期世界状态 |

组合约束：

- WorldHardmodeRuleState、RuntimeHardmodeCompatibilityState 和 WorldMetadataHardmodeState 必须是三个可区分的状态范围；是否允许短暂值不一致交由 owner 决策，而不是通过重复字段隐藏。
- TransitionPlanState、ProgressionCommitState 和 TransitionRecoveryState 共享 TransitionId 关系，但不可合并为一个巨型 Component：计划是只读快照，提交是结果摘要，恢复是失败元数据。
- HardmodeOreTierState 不与 WorldEcologyScheduleState 合并；感染规则和生态预算的失效条件不同。
- 不建立“每个 Tile 一个过渡实体”的组合；这会把长生命周期关系和大量实例数据混在一起。

## 7. 组件拆分与合并决策

| decisionId | 组成判断 | 选择 | 依据与影响 |
|---|---|---|---|
| DEC-COMP-01 | Hardmode 最终规则与活动过渡阶段 | 拆分为 WorldHardmodeRuleState + ProgressionTransitionState | 规则是长期世界事实，阶段是短期事务状态；共同放置会让失败/恢复清理污染永久规则 |
| DEC-COMP-02 | Hardmode 规则与感染传播开关 | 拆分为 WorldHardmodeRuleState + WorldInfectionPolicyState | 感染开关已有独立生态读者和规则覆盖；并非每个感染读者需要完整过渡身份 |
| DEC-COMP-03 | 三阶矿种字段之间 | 合并为 HardmodeOreTierState | 三个值共同恢复、共享合法值不变量并共同表达长期矿阶；拆成三个 Component 会制造不必要同步 |
| DEC-COMP-04 | 三阶矿种与低阶矿种 | 拆分 | Version4 静态类相同不等于生命周期相同；Hardmode 过渡只需要高阶字段，低阶字段由相邻状态裁决 |
| DEC-COMP-05 | 计划快照与恢复元数据 | 拆分为 TransitionPlanState + TransitionRecoveryState | 计划在生成后只读，恢复在失败后可变；字段失效条件和持久化时机不同 |
| DEC-COMP-06 | 计划快照与提交结果 | 拆分为 TransitionPlanState + ProgressionCommitState | 预期 section version 与实际 changed section version 不能共用一个可变字段，否则无法区分基线和结果 |
| DEC-COMP-07 | 运行时兼容值与世界元数据兼容值 | 拆分为 RuntimeHardmodeCompatibilityState + WorldMetadataHardmodeState | 活动世界和世界选择/头部元数据有不同对象范围；合并会隐藏双表示冲突 |
| DEC-COMP-08 | 变换计数与 IsTransforming | 放在 ProgressionTransitionState 内，布尔值派生 | Version4 的布尔值由同一计数派生；拆开会形成易失同步，计数不包含 Task 或锁 |
| DEC-COMP-09 | ID 与所有业务字段 | 不创建独立通用 ID Component | 当前 ID 的读写者横跨世界、存储和外部边界；先作为关系字段并标记 integration-review |

本节的合并/拆分只决定数据内聚和状态范围，不决定任何运行时结构或最终跨子系统 owner。

## 8. 不单独创建 Component 的对象

| objectId | 对象 | 不单独创建 Component 的理由 | 状态归类 |
|---|---|---|---|
| HardModeBoolean | Main.hardMode 单个布尔字段 | 它是长期规则事实的兼容观察值，不拥有计划、提交或恢复生命周期；单独创建会制造第三个 Hardmode 镜像 | 规则字段 + 兼容字段 |
| SingleInfectionTile | 单个感染 Tile | Tile 是 WorldStorage 的实例数据；感染规则、批次和区段版本不属于一个 Tile 实例 | 实例数据 |
| SingleGenerationPass | 单个生成 pass | pass 是生成能力/策略的执行单元，不能独自拥有 Hardmode 资格、世界规则或恢复状态 | 策略/能力 |
| SingleNetworkMessage | 单个 WorldData 或区段消息 | 消息是外部协议数据，生命周期随连接和协议版本变化，不是世界权威状态 | 兼容边界数据 |
| BackgroundEffect | 广播、成就、音效、粒子或 UI 效果 | 它们是结果的观察性数据，不能反向成为 Hardmode 写入根；且不同效果生命周期不一致 | 表现/外部观察 |
| SavedOreTierField | 单个 Cobalt、Mythril 或 Adamantite 字段 | 三阶值共同恢复和校验；单字段 Component 会把共同不变量变成隐含同步 | HardmodeOreTierState 的字段 |
| WorldProgressionTransition | dome 中普通日历事件的 record | 它表达血月、日食等事件的开始/停止，不是 Hardmode 长事务；同名不代表同一状态语义 | 相邻事件结果记录 |
| WorldSectionVersion | 单个区段版本值 | 它是区段存储的版本标识，被计划作为前置条件、被提交摘要作为结果引用；没有独立 Hardmode 生命周期 | 存储值/关系字段 |
| TransitionSectionPrecondition | 一个区段前置条件值 | 它只有在计划中才有意义，拆为独立实体会复制区段 owner 和关系 | 计划内值对象 |
| WorldItemProtection | 过渡前后临时物品保护值 | Version4 的资格方法为空，保护修改的是实例的 timeSinceItemSpawned；缺少可确认的长期语义 | 临时兼容效果 |

## 9. 当前 NLTX 组件覆盖

| currentPath | 当前内容 | 对目标 Component 的覆盖 | 当前状态 |
|---|---|---|---|
| src\WorldSession\WorldSessionComponents.cs:40-52 WorldRulesState | HardMode、InfectionSpreadAllowed、七项 SavedOreTiers、祭坛计数等 | 覆盖三个目标状态的部分字段，但把不同 owner/生命周期放在一个容器 | partial |
| src\WorldSession\WorldSessionComponents.cs:8-31 WorldDescriptorState | WorldId、UniqueId、尺寸、边界、准备状态 | 覆盖世界身份和范围关系；没有 Hardmode metadata 字段 | existing |
| src\WorldSession\WorldGeneration\WorldGenerationLifecycleState.cs:3-15 | Loading/Generating/Ready/Failed 和 GenerationRevision | 是过渡资格所需的相邻准备状态；不覆盖 ProgressionTransitionState | existing |
| src\WorldSession\WorldGeneration\WorldEcologyScheduleState.cs:3-20 | 感染开关、采样游标、更新率和生态预算 | 覆盖感染读取镜像和生态调度局部状态；不覆盖 Hardmode 规则 owner | existing / partial |
| dome\src\Terraria.Dome.Simulation\WorldGeneration\WorldGenerationProgressionState.cs:3-25 | IsTransforming、ActiveTransformations、生成 ID、游标和阶段 | 只部分覆盖过渡计数/生成快照；没有过渡身份、计划、提交和恢复 | partial |
| dome\src\Terraria.Dome.Simulation\WorldGeneration\HardmodeOreTierState.cs:3-8 | 三阶值对象和 Uninitialized | 可直接作为目标字段语义的现有参考；尚未证明是 WorldEntity Component 或持久化 owner | existing value type / partial component coverage |
| dome\src\Terraria.Dome.Simulation\World\WorldProgressionState.cs:9-20,116-168 | 不可变 IsHardMode 与常规事件进度 | 是 Hardmode 规则的只读镜像候选；不能作为第二权威写入根 | partial |
| dome\src\Terraria.Dome.Simulation\World\WorldProgressionTransition.cs:3-8 | 普通事件开始/停止 record | 不覆盖 ProgressionTransitionState；保留相邻领域语义 | existing, not target |
| dome\src\Terraria.Dome.Server\Replication\WorldSectionReplication.cs:99-150 | changed/requested section snapshot 编码 | 仅提供区段快照能力；不覆盖 ProgressionCommitState 或 Hardmode result | partial |
| dome\src\Terraria.Dome.Server\Persistence\WorldSaveCoordinator.cs:12-106 | 临时文件、写穿、备份和恢复读取 | 提供存档边界参考；未保存目标过渡、提交和恢复字段 | partial |
| dome\src\Terraria.Dome.Simulation\WorldGeneration\WorldGenerationPipeline.cs:39-70,409-452 | 生成阶段、局部提交、Committed 验证，并拒绝 Rules.IsHardmode | 说明当前生成管线不是 Hardmode 目标 Component 的替代品 | partial |

当前没有 9 个目标 Hardmode Component 的实际注册/实现。HardmodeOreTierState 仅是现有值类型，不应被误报为目标 Component 已落地。

## 10. 组件级 evidence-gap

| gapId | affectedComponent | 缺口 | 对字段设计的影响 | 状态 |
|---|---|---|---|---|
| GAP-COMP-01 | WorldHardmodeRuleState、RuntimeHardmodeCompatibilityState | Version4 在后台变换前写 Main.hardMode = true，但没有 PreHardmode/Announced/Committed 阶段 | RulePhase、兼容字段对齐时点不能锁定 | evidence-gap, decision-required |
| GAP-COMP-02 | WorldHardmodeRuleState、TransitionPlanState | Version4 initializeHardMode 为空，感染、墙体、随机方向和完整变更集合缺失 | 不能从 Version4 证明计划 footprint、批次数或结果字段 | evidence-mismatch |
| GAP-COMP-03 | ProgressionTransitionState、TransitionRecoveryState | EligibleForSpawnProtection 为空，物品保护资格和边界未知 | 保护值不能形成长期 Component；只能保留为临时实例兼容说明 | evidence-mismatch |
| GAP-COMP-04 | ProgressionTransitionState、ProgressionCommitState、TransitionRecoveryState | 后台任务异常是否仍排队 follow-up 没有显式结果 | 阶段、提交状态、ResultUnknown 和恢复点只能候选化 | unresolved |
| GAP-COMP-05 | HardmodeOreTierState | Version4 静态默认 107/108/111、清理值 -1 与 dome Uninitialized 语义处于不同来源 | -1 只能写为目标初始化约定；不能宣称版本行为已统一 | partial |
| GAP-COMP-06 | WorldMetadataHardmodeState、RuntimeHardmodeCompatibilityState | Main.hardMode 与 WorldFileData.IsHardMode 有不同读者和读写边界 | 双 Component 是否短暂不一致、谁覆盖谁未确定 | partial, decision-required |
| GAP-COMP-07 | TransitionPlanState、ProgressionCommitState | Version4 没有 TransitionId、PlanId、全局 WorldRevision 或批次序列；NLTX 只有局部 section version | 新增 ID/版本的默认值和持久化范围不能冒充历史事实 | missing |
| GAP-COMP-08 | WorldInfectionPolicyState | 当前规则容器和生态调度各有感染开关，Hardmode 对其 owner/更新语义未闭合 | 必须保留独立 Component，但 owner 与镜像关系需整合裁决 | partial, decision-required |

组件级缺口不允许把 designStatus 提升为 baseline；本设计保留 decision-required。它们不导致生成另一个研究报告，也不授权创建实现文件。

## 11. 未决组件 owner

以下每项都说明冲突字段、候选 owner、组成变化和当前不能定案的原因。所有跨域候选统一标记 crossSubsystemOwner: integration-review。

### BD-COMP-01 Hardmode 规则与过渡阶段的 owner

- 冲突字段：WorldHardmodeRuleState.IsHardMode、RulePhase、ProgressionTransitionState.Phase、RuntimeHardmodeCompatibilityState.LegacyHardModeValue。
- 当前候选 owner：长期规则由 WorldSession 持有；过渡阶段由 WorldProgressionAndTransition 持有；兼容值由 WorldSession 投影。
- 组成变化：若保留 Version4 的早期 true，必须保留规则 Component 与过渡 Component 的双阶段组合；若延后唯一规则写入，可以减少兼容字段，但会改变旧读者看到的时序。
- 未决原因：Version4 没有 phase 字段，且完整参考只补证同名方法，不能替本会话裁决行为保持策略。crossSubsystemOwner: integration-review。

### BD-COMP-02 感染传播规则的 owner

- 冲突字段：WorldInfectionPolicyState.IsInfectionSpreadAllowed 与现有 WorldRulesState/WorldEcologyScheduleState 同名字段。
- 当前候选 owner：WorldSession 规则状态，或 WorldGenerationAndEcology 生态状态；生态预算不应拥有长期规则。
- 组成变化：规则 owner 方案保留一个世界规则 Component 和一个生态读取镜像；生态 owner 方案可能把字段放回调度 Component，但会让 Hardmode 和规则覆盖依赖调度生命周期。
- 未决原因：当前源码显示两个容器，但没有唯一写者或覆盖优先级证据。crossSubsystemOwner: integration-review。

### BD-COMP-03 Hardmode 矿阶的 owner 与原子范围

- 冲突字段：HardmodeOreTierState 三个类型，以及 TransitionPlanState.OreTierCandidate、ProgressionCommitState.ChangedSections 的关联。
- 当前候选 owner：选择语义归 WorldGenerationAndEcology，长期存储归 WorldStorage，规则可见性归 WorldSession。
- 组成变化：若矿阶独立提交，HardmodeOreTierState 可在规则 Component 外单独恢复；若与 Tile 结果同记录，需在过渡和提交摘要中保持共同 TransitionId。
- 未决原因：Version4 SmashAltar 和存档字段证明矿阶长期存在，但没有证明 Hardmode 感染/墙体变更与矿阶共享原子提交。crossSubsystemOwner: integration-review。

### BD-COMP-04 计划前置条件与区段版本的 owner

- 冲突字段：TransitionPlanState.BaseWorldRevision、SectionPreconditions、ProgressionCommitState.WorldRevisionBefore/After、ChangedSections。
- 当前候选 owner：WorldStorage 拥有真实区段版本；WorldProgressionAndTransition 只拥有计划关系和结果摘要。
- 组成变化：存储 owner 方案保留跨域值对象并在计划/结果中引用；过渡 owner 方案会复制存储版本，增加双写同步；本设计不选择后者。
- 未决原因：当前 NLTX 的 section version 能力只覆盖局部提交，尚无全局 WorldRevision 规范。crossSubsystemOwner: integration-review。

### BD-COMP-05 运行时与元数据兼容值的 owner

- 冲突字段：RuntimeHardmodeCompatibilityState.LegacyHardModeValue 与 WorldMetadataHardmodeState.IsHardMode，以及现有 WorldFileData.IsHardMode。
- 当前候选 owner：活动运行时由 WorldSession，头部元数据由 WorldStorage，世界发现读取由 ExternalBoundaries。
- 组成变化：保持拆分可表达不同生命周期，但需要明确同步版本；合并为一个 Component 会错误地把元数据对象和活动世界放在同一实体。
- 未决原因：Version4 WorldFile 写读 Main.hardMode，而 WorldFileData 和发现广播读取另一字段；私有同步时序未闭合。crossSubsystemOwner: integration-review。

### BD-COMP-06 ID 和关系字段的 owner

- 冲突字段：WorldEntityId、TransitionId、PlanId、PersistentWorldId、WorldSectionId、WorldRevision。
- 当前候选 owner：现有世界 descriptor 保留 WorldId/UniqueId；过渡记录持有 TransitionId；存储持有持久化和区段版本；外部会话只持有网络标识。
- 组成变化：不创建通用 ID Component，按状态载体保存关系字段；若整合会话定义统一 identity Component，需重新评估第 6 节的 WorldMetadataEntity 与 WorldEntity 组合。
- 未决原因：这些 ID 横跨至少两个子系统，单一会话不能宣布共享基础类型最终归属。crossSubsystemOwner: integration-review。

## 12. 最终 Component 清单

以下是本 Component-only Design 的完整候选清单。每一项的 status 都是 proposed，表示没有创建同名代码或注册实际 ECS 组件。

| componentId | name | status | entityScope | componentOwner | 当前覆盖 | 设计状态 |
|---|---|---|---|---|---|---|
| C-WPT-01 | WorldHardmodeRuleState | proposed | 活动 WorldEntity | WorldSession candidate；过渡写入待整合 | WorldRulesState.HardMode partial；dome Hardmode snapshot partial | decision-required |
| C-WPT-02 | WorldInfectionPolicyState | proposed | 活动 WorldEntity | WorldSession / WorldGenerationAndEcology candidate | 两个现有感染开关字段 partial | decision-required |
| C-WPT-03 | HardmodeOreTierState | proposed | 活动 WorldEntity | WorldGenerationAndEcology / WorldStorage candidate | dome 值类型 existing；活动组件和 owner partial | decision-required |
| C-WPT-04 | ProgressionTransitionState | proposed | 活动 WorldEntity，每世界至多一个活动过渡 | WorldProgressionAndTransition candidate | dome 生成进度只覆盖计数/快照 partial | decision-required |
| C-WPT-05 | TransitionPlanState | proposed | 活动 WorldEntity 的过渡快照 | WorldProgressionAndTransition / WorldGenerationAndEcology candidate | 无 Hardmode plan component；missing | decision-required |
| C-WPT-06 | ProgressionCommitState | proposed | 活动 WorldEntity 的提交结果摘要 | WorldStorage / WorldProgressionAndTransition candidate | 只有局部 Tile/区段版本能力 partial | decision-required |
| C-WPT-07 | TransitionRecoveryState | proposed | 活动 WorldEntity 的恢复快照 | WorldProgressionAndTransition candidate | 无 Hardmode 恢复状态 missing | decision-required |
| C-WPT-08 | RuntimeHardmodeCompatibilityState | proposed | 活动 WorldEntity 的兼容视图 | WorldSession candidate | Main.hardMode 直接字段 partial | decision-required |
| C-WPT-09 | WorldMetadataHardmodeState | proposed | WorldMetadataEntity | WorldStorage / ExternalBoundaries candidate | WorldFileData.IsHardMode partial；NLTX metadata missing | decision-required |

跨子系统字段和关系的统一说明：

- 以上 9 个 Component 中凡涉及 WorldEntityId、TransitionId、PlanId、PersistentWorldId、WorldSectionId、WorldRevision 或区段版本的字段，均保持 crossSubsystemOwner: integration-review。
- WorldDescriptorState、WorldGenerationLifecycleState、WorldEcologyScheduleState、HardmodeOreTierState 值类型和 WorldProgressionState 是当前 NLTX 的 existing/partial 映射，不计入本清单的目标 Component 数量，也不被本设计改名或移动。
- 本清单没有创建 Shared/Components/、Common/ 或 Misc/ 聚合目录，也没有为单个 Tile、单个事件消息或背景效果增加状态载体。

## 13. 最终声明

本文件是 Component-only Design。
本文件不定义 System、Query、Command、Event、Adapter、Projection、
调度顺序、运行时实现、测试计划或迁移计划。

所有 status: proposed 的 Component 都只是设计提案。
本文件不声明代码已创建、已迁移、行为等价或验证通过。

本设计的 verificationStatus 为 not-run。本次只读整理未运行构建或测试，未创建 .cs、.csproj、测试或运行时实现文件，未修改 Version4、完整参考源码、tModLoader 文档、Space Station 14、当前 NLTX 源码、原研究报告或其他设计文档。
