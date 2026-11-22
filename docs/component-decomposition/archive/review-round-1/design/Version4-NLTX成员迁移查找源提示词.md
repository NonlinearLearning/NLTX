# Version4 → NLTX 成员迁移查找源提示词

## 用途

将本文件全文交给负责执行任务的主 AI。目标是把 `docs/component-decomposition/review-round-1` 中的审查结论，建立为可从第一轮审查文档追溯到 Version4 源成员，再追溯到 NLTX 显式 target 成员的机器可审计映射。

“持续执行”指：遇到普通错误、单项验证失败或单个 workItem 阻塞时，必须记录现场并继续处理下一个满足条件的 workItem；不指绕过哈希、证据、权限、target manifest 或 verifier 硬门槛。

## 可直接发送给 AI 的提示词

你现在负责在 `D:\TRbackup\NLTX` 中完成一次 Version4 → NLTX 的“第一轮审查文档—Version4 源成员—NLTX target 成员”一一可追溯映射工作。

### 一、必须使用的技能和执行模式

必须全程使用并严格遵守：

`D:\TRbackup\NLTX\.agents\skills\version4-member-migration-ledger\SKILL.md`

在开始任何实际工作前，完整读取该 `SKILL.md`，以及它要求读取的 references、schema、scripts 帮助和仓库相关约束。不能只凭本提示词、历史对话、目录名称、类名相似度或已有报告猜测事实。

1. 全程只使用当前主代理，严禁使用子代理、线程委派、fork、handoff、后台代理或任何形式的 agent delegation。
2. 不得调用或启动 `create_thread`、`fork_thread`、`handoff_thread`、`subagent` 等能力。
3. 不得把独立任务并行分发给其他代理。
4. 所有读取、分析、编辑和验证均由当前主代理完成。
5. 不要因为普通错误、路径问题、解析失败、哈希漂移、历史文档冲突、验证失败或某个 workItem 阻塞而结束整个任务。
6. 每次遇到问题时必须：
   - 保留现场，不删除原始证据；
   - 记录具体路径、成员、错误、命令、退出码和当前状态；
   - 将当前 workItem 标记为 `blocked` 或 `deferred`，不得伪装成 `verified`；
   - 释放当前 active claim，记录 checkpoint、原因和 ledger hash；
   - 按 quick-reference 的确定性队列选择下一个满足前置条件的 workItem；
   - 继续执行剩余安全且可验证的工作。
7. 当前 workItem 的 audit 失败是该 workItem 的停止条件，但不是整个任务的停止条件。
8. 不得通过猜测替换缺失路径、缺失成员、缺失 target、缺失哈希、未知 evidence 或失败 verifier。
9. 如果所有剩余 workItem 都被真实阻塞，输出完整阻塞报告并结束；不得声称完成。
10. 如果 `Version4-member-migration-quick-reference.json` 和 canonical ledger 都不存在，禁止从报告、目录名、文件数、历史对话或类名重建事实。记录全局 bootstrap blocker，完成能安全完成的准备性检查，并明确说明无法继续建立事实映射的原因。
11. 不要等待我逐项确认。对正常的文件读取、扫描、分类、证据登记、生成、审计和继续下一个 workItem，自主完成。

### 二、开始前的仓库检查

1. 先读取仓库根目录 `AGENTS.md` 和 `Context/progress.md`。
2. 如果存在当前 flowstate context 或 plan，再读取与本任务直接相关的内容。
3. 执行 `git status --short`，保护所有既有工作区改动。
4. 本任务不得修改 NLTX 运行时代码、测试、历史文档、构建输出或现有用户未提交改动。
5. 本任务只允许修改迁移 ledger、证据、快照、显式 target manifest、workItems 和由 ledger 生成的受控视图。
6. `D:\TRbackup\Version4` 和 `D:\TRbackup\NLTX\src` 均保持只读；`docs\component-decomposition\review-round-1` 中已有审查文档也保持只读。

### 三、范围和事实边界

1. 唯一完整 Version4 覆盖基线是：

   `D:\TRbackup\Version4`

2. 源文件分母必须使用：

   `D:\TRbackup\NLTX\docs\migration\ledgers\Version4源码覆盖.tsv`

3. 重新从当前 checkout 计算实际数量，不要直接相信历史数字。历史的 967 个 `.cs` 文件只能作为待核对参考，不能直接当作当前结果。
4. `D:\TRbackup\NLTX\src` 是当前 NLTX target 候选范围，但不得把扫描到的所有 `src` 成员自动当成 target manifest 成员。
5. 必须建立或校验显式 target manifest。只有 manifest 中明确列出的 target 成员才可以进入 target reverse index 和映射事实。
6. target manifest 必须为每个 target 记录并校验：
   - `targetMemberId`
   - `componentPath`
   - `componentType`
   - `member`
   - `signature`
   - `kind`
   - `authorityRole`
   - `origin`
   - `designReason`
7. 每一个 `targetMemberId` 必须能根据 target root、project/assembly、完整类型名和规范化成员签名精确重建。只名称相同、路径相似或类名相似都不算有效匹配。
8. `D:\TRbackup\NLTX\docs\component-decomposition\review-round-1` 是本次审查文档的输入证据目录：
   - 先完整枚举其中的全部文档；
   - 记录每个文件的绝对路径、相对路径、文件哈希和可用的行号/章节位置；
   - 不得修改、覆盖或删除这些第一轮审查文档；
   - 每个审查结论必须能追溯到具体文档位置；
   - 审查文档只作为证据，不自动成为当前运行时事实。
9. `dome/dome1/docs/migrations/` 等历史迁移文档只能作为历史证据。不得把历史结论直接导入当前 ledger，不得覆盖历史文件，也不得扩大当前 Version4 分母。
10. 如果使用 `D:\TRbackup\无任何删减通过编译`，只能补充当前 Version4 分母中已经存在的同路径、同类型文件的行为证据；必须同时记录两个来源路径，不能借此增加 Version4 覆盖范围。

### 四、canonical ledger 和生成视图

1. `Version4-member-migration-map.json` 是唯一允许手工维护的事实源。优先查找仓库中现有的 canonical ledger 路径；如果不存在，按照技能文件和 schema 指定的受控迁移目录创建。
2. 不得把完整生成视图反向复制到 ledger。
3. 事实必须分层保存：
   - `inventory`：当前源/目标扫描事实；
   - `decisions`：经过证据审查的迁移决定；
   - `evidenceCatalog`：文档、声明、读写、生命周期、权限、持久化、网络和风险证据；
   - `verificationCatalog`：结构化、可重现、带哈希绑定的 verifier 结果；
   - `workItems`：可恢复的单条工作单元；
   - `generatedViewMetadata`：生成视图路径、版本、哈希和来源信息。
4. 所有生成的 quick-reference、Markdown view、target reverse index、context packet 或其他查找视图，都必须由 canonical ledger 和现有生成脚本生成，不能手工编辑。
5. 生成输出只能放在技能允许的迁移输出目录或 `Build/generated/`，不得写入 `src/`、Version4 源目录或项目源文件旁边。
6. 第一轮审查专用查找视图必须从 canonical ledger 生成，并至少支持以下查找键：
   - 第一轮审查文档路径 + 行号/章节；
   - `sourceMemberId`；
   - `targetMemberId`；
   - `workItemId`；
   - `disposition`；
   - `status`；
   - `blocker`；
   - `verificationRefs`。
7. 如果现有生成器不支持第一轮审查文档索引，先检查 schema 和现有脚本；只有在不破坏 ledger 协议的前提下，增加受控生成逻辑。不得创建第二个手工事实源。

### 五、恢复和读取顺序

1. 首先读取：

   `Version4-member-migration-quick-reference.json`

2. 只在需要人类阅读时读取：

   `Version4-member-migration-quick-reference.md`

3. 如果 quick-reference 里存在 active claim：
   - 只恢复该 `workItemId`；
   - 读取对应的 `Version4-member-migration-context-packet.json`；
   - 校验 packet 的 `generatedFromLedgerFactSha256`；
   - 校验 source/target snapshot hash；
   - 不得绕过 active claim 直接选择其他 workItem。
4. 如果没有 active claim：
   - 按 quick-reference 的 deterministic queue 选择最高优先级、前置条件已满足的 workItem；
   - queue 排序必须遵循技能定义的稳定排序：active claim、priority、fileOwner、batchId、source path、sourceMemberId、workItemId。
5. 不能为了选择 workItem 而先加载完整历史报告、完整 ledger 或整个 `src` 目录。
6. packet、ledger、snapshot、manifest、source declaration、target declaration、evidence 和 verifier 的哈希不一致时，先记录 drift/blocker，不得猜测修复。
7. 未知 workItem、多个 active claim、被其他 workItem 抢占的 active claim，都必须 fail-closed 记录，不能静默覆盖。

### 六、源成员映射规则

1. 在批准的 `memberScope` 内，Version4 的每一个字段和属性都必须拥有唯一稳定的 `sourceMemberId`。
2. source identity 必须遵循：

   `Version4::<fully-qualified-declaring-type>::<canonical-member-signature>`

3. source identity 不包含源文件路径。文件移动或行号变化不能自动产生新成员。
4. 必须覆盖符合 scope 的：
   - private/public/internal/protected 字段；
   - static、readonly、const 字段；
   - 自动属性；
   - expression-bodied property；
   - 嵌套类型成员；
   - partial declaration 中的成员。
5. 不得登记：
   - 继承来的重复成员；
   - 编译器生成的 backing field；
   - 局部变量；
   - 生成代码成员；
   - 解析失败后伪造出来的零候选。
6. 如果 Roslyn 语法或语义扫描无法确认完整性，必须记录 `unsupported-syntax` 或 scan failure gap；不能把解析失败转换成“没有成员”。
7. 源成员必须保存当前声明位置、规范化签名、类型、访问性、修饰符、文件哈希和成员 fingerprint。
8. 第一轮审查文档只要提到某个类、字段、属性、状态或职责，就必须尽量定位到精确的 Version4 声明和 `sourceMemberId`。
9. 如果审查文档只提到类名或模糊职责、无法安全定位到具体字段/属性：
   - 不得按命名相似度猜成员；
   - 建立明确的 scope/evidence gap；
   - 记录文档位置；
   - 为该 gap 创建 workItem；
   - 继续处理其他可以精确定位的审查条目。
10. 方法、事件、构造函数和调用链属于行为覆盖，不得伪装成字段/属性映射。必要时可以把它们记录为 behavior evidence 或独立 behavior workItem，但不能把方法名称直接当成字段映射事实。
11. 每个源成员只能有一个最终 decision row；不得重复创建同一 `sourceMemberId` 的多个互相竞争的最终决定。
12. 允许的 disposition 只有：
   - `move`
   - `split`
   - `merge`
   - `derive`
   - `compatibility`
   - `excluded`
   - `deferred`
13. `sourceMemberId` 的一行一决定不等于可以强行制造虚假的一对一语义：
   - `move` 必须恰好一个 component-member 或 value-object target；
   - `split` 必须至少两个 target edge，并写明 `splitReason`；
   - `merge` 必须使用共同的 `mergeGroupId` 和 `mergeReason`；
   - `derive` 只能指向 query 或 none，不能形成新的 authoritative state root；
   - `compatibility` 只能指向 adapter、projection 或 none，不能形成新的 authority root；
   - `excluded` 必须指向 none，并有证据和排除原因；
   - `deferred` 必须有证据缺口、阻塞原因和后续 workItem。

### 七、targetMappings 规则

1. `targetMappings` 是唯一允许记录映射事实的地方。
2. 每一条 mapping edge 必须完整记录：
   - `mappingId`；
   - `targetRef`；
   - `relation`；
   - `role`；
   - 完整 `mappingContract`；
   - `evidenceRefs`；
   - `verificationRefs`。
3. 每一个 mappingContract 都必须明确：
   - `kind`；
   - `sourceType`；
   - `targetType`；
   - `conversion`；
   - `unit`；
   - `nullability`；
   - `defaultSemantics`；
   - `rangeSemantics`；
   - `normalization`；
   - `lossiness`；
   - `reversible`；
   - `invariants`；
   - `authorityTransfer`；
   - `dualWritePolicy`。
4. 必须区分 authoritative state、query、adapter、projection、value object、integration 和 compatibility。
5. 不得因为 target 成员名字相似就认定它继承了 Version4 的 authority、持久化 ID、网络 ID、slot ID、housing key、ownership 或稳定身份。
6. entity identity、persistence identity、network identity、slot identity 和 external identity 必须分别建模，不得合并成一个“看起来相同”的 ID。
7. target origin 必须明确为：
   - `migrated`
   - `new`
   - `derived`
   - `integration`
   - `compatibility`
8. `origin=migrated` 的 authoritative target 没有合法 source mapping 时，必须记录 orphan error。
9. `origin=new` 的 authoritative target 必须有明确 design reason，不能为了消除 orphan 而伪造 source。
10. `derived`、`compatibility` 和非 authority target 不能被计入“已完成迁移成员”。
11. 如果两个 source mapping 争夺同一个 target 且 ownership 不兼容：
   - 保留双方证据；
   - 标记 target conflict；
   - 不按命名、目录或历史报告强行选择；
   - 建立 blocker/workItem；
   - 继续其他无冲突条目。

### 八、证据要求

对于每个第一轮审查映射，至少建立可追溯的 document evidence，并根据成员性质补足以下证据轴：

1. `declaration`：精确类型、成员名、成员种类、类型、修饰符、稳定 identity、文件和行号。
2. `reader`：所有有意义的读取点，包括间接访问、反射访问、序列化访问和生成代码风险。
3. `writer`：所有有意义的写入点，包括初始化、更新、重置、死亡、重载、恢复和跨域提交。
4. `authority`：cutover 后由哪个层拥有该值；authority 是 server、world、entity、component、query、adapter 还是其他边界。
5. `lifecycle`：spawn、initialization、update、reset、death/despawn 和 reload/load。
6. `persistence`：save/load，或者明确证明该成员不持久化。
7. `network`：serialization/replication，或者明确证明该成员不网络化。
8. `dynamic/reflection/generated-code risk`：不能确认时必须标记为 unknown/partial/blocker。
9. `verifier`：可重现命令、实际执行结果、exit code、source snapshot hash、target snapshot hash 和 ledger fact hash。

声明证据本身只能证明符号存在，不能单独证明语义等价、authority 转移、读写闭包、持久化、网络同步或行为保留。

第一轮审查文档证据至少应记录：

- 文件绝对路径；
- 相对路径；
- 行号、章节或稳定定位；
- 文档哈希；
- 原文结论或准确摘要；
- 它支持的具体 claim；
- 对应的 `evidenceId`；
- 对应的 `sourceMemberId` 或明确 scope gap。

### 九、workItem 单条循环

1. 每次只能有一个 active workItem。
2. 开始一个 workItem 时先将其设为 `in-progress`，记录 owner、checkpoint、当前 ledger hash、source/target snapshot hash、允许修改的文件和 completion criteria。
3. 一个 workItem 只处理它声明的 sourceMemberRefs、targetMappingRefs、targetMemberRefs 和 evidence refs。
4. 不得借一个 workItem 顺便修改无关组件、无关文档、无关历史报告或 runtime code。
5. 完成当前 workItem 前必须：
   - 读取对应源声明；
   - 读取所有相关 readers/writers；
   - 读取 target declaration；
   - 关闭所要求的 evidence axes；
   - 处理 fingerprint/reflection/serialization/generated-code 风险；
   - 更新 decision、evidenceCatalog、verificationCatalog 和 workItem checkpoint；
   - 运行 ledger audit；
   - 重新生成所有视图；
   - 校验生成视图哈希；
   - 重新读取 quick-reference recovery header。
6. 只能由 audit 工具根据证据推导 `verified`。AI 不能直接把 status 写成 `verified` 来制造完成感。
7. `verified` 必须同时满足：
   - audit 通过；
   - declaration/reader/writer/authority/lifecycle 证据闭合；
   - persistence/network applicability 已明确；
   - dynamic-access risk 已关闭；
   - verifier `passed=true`；
   - `exitCode=0`；
   - verifier hashes 与当前 ledger/source/target snapshots 匹配；
   - 没有 fingerprint drift。
8. `verificationStatus=not-run`、`unknown`、`partial`、`failed`、`scan-failed` 不能支持 `verified`。
9. 如果源 fingerprint 变化、成员改名、成员消失、target manifest 漂移或快照哈希漂移：
   - 保留旧决策；
   - 不删除旧 ledger row；
   - 创建 identity/evidence review checkpoint；
   - 标记当前 workItem blocked/deferred；
   - 继续其他可处理 workItem。
10. 禁止创建任何 physical-delete workItem。源成员必须永久保留在 ledger 中。旧代码是否仍存在不是删除 ledger row 的理由，应使用 `compatibility-only`、`unused-but-retained` 或协议允许的等价状态记录当前 authority。

### 十、扫描、构建和验证

1. 优先使用技能提供的 scanner、audit 脚本和 generator，不要重新手写等价扫描器。
2. 对源代码成员扫描，必须使用 Roslyn syntax tree 和 semantic model，并保存不可变 source snapshot。
3. source snapshot 至少要绑定 parser configuration、nullable/documentation settings、preprocessor symbols、encoding、Roslyn version、metadata-reference manifest、文件哈希、member fingerprints、configuration variants 和 diagnostics。
4. 如果需要执行 compile-capable 的 dotnet 命令：
   - 必须从 `D:\TRbackup\NLTX` 仓库根目录执行；
   - 必须先检查活动的 `dotnet.exe` 和 `csc.exe`；
   - 如果已有构建进程或 owner 不明确，等待并不得终止它；
   - 一次只能有一个 compile-capable 进程使用本 checkout；
   - 禁止后台执行；
   - 禁止并行执行；
   - 禁止 raw `dotnet`；
   - 必须经过 `pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1`；
   - 必须使用 `-m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false`；
   - 只构建受影响的项目，不要为增量工作构建整个 solution；
   - 正常增量工作禁止 `-t:Rebuild`。
5. 每一个支持结论的构建、测试、扫描或 verifier 命令都必须记录完整命令、project、exit code、warning 数、error 数、输出路径以及生成 artifact 是否确实位于 `Build/bin/`。
6. ledger audit 命令必须先检查脚本帮助，再按脚本实际参数执行，例如：

   `python .agents\skills\version4-member-migration-ledger\scripts\audit_ledger.py --help`

7. 如果执行 verifier，必须使用结构化 `verificationSpec`，不能只记录一段无法重现的自由文本 command。
8. 如果没有运行构建或 verifier，必须明确写入 `verificationStatus=not-run`，最终报告不得使用“已验证”“已通过”“行为等价”等词。
9. 不得把目录存在、组件数量、文档模板完整、历史审查通过或单个局部测试通过当成 Version4→NLTX 迁移完成证据。

### 十一、持续推进循环

重复以下循环，直到没有任何安全且满足前置条件的 workItem：

1. 读取 recovery header。
2. 确认 active claim 和 ledger/snapshot/manifest hashes。
3. 选择一个且仅一个 eligible workItem。
4. claim 该 workItem。
5. 读取 context packet 指定的最小源、目标和证据范围。
6. 对照第一轮审查文档定位具体 claim。
7. 在 Version4 中核对精确声明。
8. 在显式 target manifest 中核对精确 target declaration。
9. 补充 readers/writers/lifecycle/authority/persistence/network/risk evidence。
10. 写入或更新 decision 和 targetMappings。
11. 运行 audit。
12. 如果 audit 失败，记录失败并将该 item 标记为 `blocked`/`deferred`，释放 claim，继续下一个 eligible item。
13. 如果 audit 通过但 verifier 未通过或未运行，不得 `verified`；保留真实状态并继续其他 item。
14. 重新生成 quick-reference、Markdown view、target reverse index、context packet 和其他受控视图。
15. 校验 ledger fact hash、scope revision、source snapshot hash、target snapshot hash、targetManifestSha256 和 generated view hashes。
16. 重新读取 recovery header，确认队列仍然确定。
17. 继续下一项，不要在单个成功或单个失败后结束整个任务。

### 十二、完成标准和最终输出

最终必须生成或更新技能协议要求的 canonical ledger、source snapshot、target snapshot、显式 target manifest、quick-reference、Markdown view、context packet、target reverse index 及第一轮审查可追溯查找视图；具体路径以仓库现有约定和生成器为准。

最终报告必须包含：

1. 实际扫描的 Version4 文件数量和来源；
2. 当前 `docs/migration/ledgers/Version4源码覆盖.tsv` 的 hash；
3. memberScope 的 sourceMember 数量；
4. 每种 disposition 的数量；
5. `todo`、`in-progress`、`migrated`、`verified`、`blocked`、`deferred` 数量；
6. 第一轮审查文档总数；
7. 已成功绑定到精确 `sourceMemberId` 的文档条目数；
8. 已绑定到精确 `targetMemberId` 的条目数；
9. 未能定位的文档条目和原因；
10. source orphan、target orphan、mapping conflict、fingerprint drift、snapshot drift、manifest drift 数量；
11. 当前 active claim 或确认没有 active claim；
12. 下一个确定性的 `nextWorkItemRef`；
13. 所有未解决 blocker；
14. 每条 blocker 对应的文件、成员、证据、命令和下一步；
15. 每个编译、扫描、audit、verifier 命令的完整命令、退出码、warning/error 数和 artifact 路径；
16. 明确说明哪些结果是 `verified`，哪些只是 `migrated`、`blocked`、`deferred` 或 `not-run`；
17. 明确声明：
   - 没有删除任何 Version4 源成员；
   - 没有修改 NLTX runtime code；
   - 没有修改第一轮审查原文；
   - 没有使用子代理；
   - 没有通过猜测填补映射；
   - 审计成功不等于整个迁移完成，除非所有协议门槛确实满足。

除非所有必要 source members、target mappings、evidence、verifiers、hashes 和 workItems 都真实闭合，否则最终不得使用“全部完成”“全部迁移”“行为等价”“运行时就绪”或“已验证完成”等表述。若仍有 blocker，必须以 blocker 为事实输出；但只要存在可处理 workItem，就继续推进，不要主动停工。
