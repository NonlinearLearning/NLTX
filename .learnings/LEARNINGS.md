## [LRN-20260830-001] correction_item_migration_is_code

**Logged**: 2026-08-30T00:00:00+08:00
**Priority**: high
**Status**: pending
**Area**: backend

### Summary
For the Item field/property migration, the requested deliverable is a bounded C# implementation
slice with focused verification, not documentation reorganization.

### Details
- The active migration report is context and acceptance authority only.
- Each next source-backed field or behavior must land in the Simulation owner chain and be exercised
  by a focused verifier before status is updated.
- Keep full Item parity and explicitly deferred behavior marked partial.

### Suggested Action
When resuming this migration, inspect the current red test and implement the smallest C# owner,
consumer, snapshot, and regression slice; do not create docs-only process artifacts as the primary
deliverable.

### Metadata
- Source: user_feedback
- Related Files: docs/migrations/item-field-property-ecs-migration.md; Test/Terraria.Dome.Items.Verification/Program.cs
- Tags: item-migration, ecs, code-first, correction

---

## [LRN-20260906-003] knowledge_gap

**Logged**: 2026-09-06T12:05:00+08:00
**Priority**: low
**Status**: pending
**Area**: docs

### Summary
用户显式提供的外部技能路径需要先做实际存在性核验，不能仅依据当前会话列出的可用技能目录判定缺失。

### Details
本次先依据可用技能清单把 `C:\Users\shan\.agents\skills\grill-me\SKILL.md` 判断为不可用，随后只读 `Test-Path` 证实该文件存在。正确做法是：用户指定了技能路径时，优先按该路径读取；只有路径确实不可读时才使用等价技能或说明降级。

### Suggested Action
在技能触发路由中保留用户显式路径优先级，并将目录清单视为发现辅助而非存在性证明。

### Metadata
- Source: knowledge_gap
- Related Files: C:\Users\shan\.agents\skills\grill-me\SKILL.md
- Tags: skill-routing, path-verification, user-explicit-skill

---

## [LRN-20260906-001] correction

**Logged**: 2026-09-06T00:00:00+08:00
**Priority**: high
**Status**: pending
**Area**: docs

### Summary
The migration ledger must not introduce physical deletion as a migration requirement.

### Details
The user clarified that the JSON ledger is for mapping, authority cutover, legacy usage, evidence, and continued AI migration. It must not add physical deletion states, deletion gates, or deletion work items. Legacy source members may remain permanently represented in the ledger.

### Suggested Action
Keep physical deletion outside the ledger design. Define completion through mapping and authority/read-write closure only, while retaining explicit compatibility or legacy-use status where needed.

### Metadata
- Source: user_feedback
- Related Files: docs/plans/2026-09-06-version4-member-migration-ledger-design.md; docs/plans/2026-09-06-version4-member-migration-ledger-implementation.md
- Tags: scope-control, migration-ledger, no-deletion

---

## [LRN-20260906-001] correction

**Logged**: 2026-09-06T10:02:00+08:00
**Priority**: medium
**Status**: resolved
**Area**: tests

### Summary
用户要求实现组件但明确不保留测试时，应删除本回合创建的 focused verifier 及其生成物，只保留生产组件代码。

### Details
实现阶段先按 TDD 创建并运行了一个最小 verifier，随后用户明确要求“不写测试”。该要求覆盖了当前实现范围；因此删除本回合新增的测试项目、测试源码和对应 `Build/bin`、`Build/obj` 生成物，不删除仓库中已有的其他测试或验证材料。

### Suggested Action
后续遇到用户明确排除测试的实现任务时，先确认是否允许临时测试；若用户否定，则不创建测试项目，并只对受影响生产项目运行构建验证。

### Metadata
- Source: user_feedback
- Related Files: Test/Terraria.WorldProgression.Components.Verification/; src/WorldSession/WorldProgression/
- Tags: scope-control, testing, user-preference

### Resolution
- **Resolved**: 2026-09-06T10:02:00+08:00
- **Notes**: 删除本回合新增的 WorldProgression verifier 文件及其生成物，保留 src 下组件实现。

---

## [LRN-20260906-001] correction

**Logged**: 2026-09-06T10:05:00+08:00
**Priority**: medium
**Status**: pending
**Area**: tests

### Summary
用户明确要求本次组件实现不写测试；应撤销本轮新增的测试断言并只验证生产源码。

### Details
实现组件时按 TDD 规则向既有验证入口加入了目标 API 断言。用户随后明确要求“不写测试”，因此这些新增断言和 using 必须精确移除，既有验证文件保持原状；后续不得新增、修改或运行测试。

### Suggested Action
实现阶段只修改 `src` 下生产组件和必要的支持类型；完成前使用生产项目的串行构建与只读源码检查，不把未运行测试描述成已验证。

### Metadata
- Source: user_feedback
- Related Files: Test/Terraria.Npc.Components.Verification/Program.cs; src/Npc; src/Town; src/WorldSession
- Tags: user-scope, no-tests, component-implementation

---

## [LRN-20260906-002] correction

**Logged**: 2026-09-06T00:00:00+08:00
**Priority**: medium
**Status**: pending
**Area**: tests

### Summary
用户明确要求实现组件时不写测试，必须撤回本轮新增的测试夹具和测试项目，不把 TDD 默认流程扩大到用户明确排除的范围。

### Details
本轮按照默认 TDD 流程新增了一个 Liquid 组件验证项目并完成了 RED/GREEN 验证；用户随后明确要求“不写测试”。应保留已请求的 `src` 生产组件实现，删除本轮创建的测试项目和测试入口，后续只执行受影响生产项目的构建检查，不再添加或运行测试。

### Suggested Action
当用户对实现范围作出明确排除时，立即撤回当前轮次新增且属于排除范围的文件；在最终报告中列出撤回结果，并区分构建验证与测试验证。

### Metadata
- Source: user_feedback
- Related Files: Test/Terraria.Liquid.Components.Verification/; src/WorldStorage/; src/Share/Entity/Components/
- Tags: scope-control, testing, correction

---

## [LRN-20260906-002] correction

**Logged**: 2026-09-06T02:45:00+08:00
**Priority**: high
**Status**: pending
**Area**: tests

### Summary
用户明确要求实现 PlayerGameplay 组件时不写测试；测试文件必须从本次变更中排除。

### Details
实现过程中按默认 TDD 规则临时创建了 Player 组件验证项目和测试入口。用户随后明确“不写测试”，因此这些本次创建的测试文件必须删除，不能把通用测试技能优先级置于当前明确范围之上。生产组件实现仍保留，后续只执行用户允许的源码构建或静态检查，不运行测试。

### Suggested Action
当用户明确限定“不写测试”时，不新增测试项目、测试源文件或测试夹具；若已在当前任务中创建，只删除本次新增且已确认的精确目标，不触碰仓库中其他测试内容。

### Metadata
- Source: user_feedback
- Related Files: src/Player; Test/Terraria.Player.Components.Verification
- Tags: correction, tests, scope-control

---

## [LRN-20260906-001] explicit_no_tests_scope

**Logged**: 2026-09-06T00:00:00+08:00
**Priority**: medium
**Status**: pending
**Area**: tests

### Summary
用户明确要求实现 Projectile 组件时不写测试。

### Details
此前为 TDD RED 阶段临时创建了 focused verification 项目和测试程序；用户随后明确要求“不写测试”。测试文件已删除，本轮改为仅实现根 src/Projectile 生产组件，并使用项目级编译和非测试静态检查验证。

### Suggested Action
当用户明确排除测试时，不再继续创建测试项目；最终报告应准确说明未添加、未运行测试，并区分编译验证与行为验证。

### Metadata
- Source: user_feedback
- Related Files: src/Projectile/
- Tags: scope, tests, projectile-components

---

## [LRN-20260906-002] correction

**Logged**: 2026-09-06T11:05:00+08:00
**Priority**: medium
**Status**: pending
**Area**: tests

### Summary
用户明确要求实现组件但不写测试时，应删除本轮临时创建的测试夹具，并按用户要求直接实现。

### Details
本轮在实现 SpatialSimulation 组件前按 TDD 创建了临时验证项目和 Program.cs。用户随后明确表示“不写测试”。这些测试文件属于本轮新增，不能保留；应删除本轮新增的测试文件和空测试目录，但保留工作区原有测试及其修改。

### Suggested Action
遇到用户明确拒绝测试时，将其作为当前任务的测试例外；先清理本轮新增测试资产，再继续生产代码实现，并在最终报告中明确未运行测试。

### Metadata
- Source: user_feedback
- Related Files: Test/Terraria.SpatialSimulation.Components.Verification
- Tags: scope-control, tdd-exception, user-preference

---

## [LRN-20260906-002] correction

**Logged**: 2026-09-06T10:00:00+08:00
**Priority**: medium
**Status**: pending
**Area**: tests

### Summary
用户明确要求“不要写测试”时，不能继续执行默认的 TDD 测试项目路径。

### Details
本次组件实现已先按 TDD 临时创建验证项目，但用户随后明确要求不写测试。应立即删除本轮创建的测试项目和测试文件，保留生产组件实现；后续只进行源码级检查和用户允许的生产项目编译验证，不新增或运行测试。

### Suggested Action
在实现前优先确认用户是否允许新增测试项目；若用户明确排除测试，停止 TDD 测试文件写入，并将验证范围改为生产项目编译、静态文本检查和输出路径检查。

### Metadata
- Source: user_feedback
- Related Files: Test/Terraria.Teleportation.Components.Verification; src/Teleportation; src/WorldStorage
- Tags: correction, tests, scope-control, tdd

---
## [LRN-20260905-001] correction

**Logged**: 2026-09-05T12:00:00+08:00
**Priority**: high
**Status**: pending
**Area**: docs

### Summary
当用户要求当前会话完成审查报告时，不应自行启动子代理。

### Details
本次根据任务文档生成报告的过程中，错误地把 `research` 技能中的后台研究建议当成了可执行的
子代理授权。用户随后明确要求“不启动子代理”。正确做法是保留工作在当前会话内，直接读取证据、
整理报告并写入用户指定的唯一文档；任何已启动的子代理都应立即关闭，不能让其修改或影响报告。

### Suggested Action
当仓库协议或用户要求限定为单会话时，优先遵守该范围；技能中的委托建议不能覆盖用户的执行方式。

### Metadata
- Source: user_feedback
- Related Files: docs/第一轮审查/2026-09-05-version4-world-progression-and-unlocks-public-decomposition.md
- Tags: correction, no-subagents, docs, scope-control

---
## [LRN-20260904-001] correction

**Logged**: 2026-09-04T00:00:00+08:00
**Priority**: high
**Status**: pending
**Area**: docs

### Summary
`dome` 是旧实现，不能作为 Version4 当前运行时或权威 ECS 状态的证据。

### Details
在子系统盘点中将 `dome/src/Terraria.Dome.Simulation` 误判为当前 Arch ECS 运行时，导致把旧实现的消费者和组件并行关系写成了当前实现事实。正确证据优先级应是 Version4 当前源码与 NLTX 当前目标实现；`dome` 只能作为历史迁移线索、旧设计对照或差异来源，除非用户明确要求审阅旧实现。

### Suggested Action
后续报告将 `dome` 标记为 legacy/reference，单独列出其与当前实现的差异，不用它确认当前组件的读写者、生命周期、调度、复制或持久化行为。

### Metadata
- Source: user_feedback
- Related Files: dome/src; D:\\TRbackup\\Version4; src
- Tags: correction, scope-control, legacy-implementation, ecs

---
## [LRN-20260902-001] correction

**Logged**: 2026-09-02T18:05:00+08:00
**Priority**: medium
**Status**: pending
**Area**: docs

### Summary
文件结构设计审查必须保持在目录、归属、迁移和验收边界内，不应扩展为字段或数据模型设计。

### Details
在对 Entity ECS 文件组织提案进行 grill 时，将 `LiquidKind` 的枚举拆分和访问级别当成了需要用户决策的文件结构问题。用户明确指出当前任务是文件结构设计文档，而不是字段设计文档。正确做法是只审查类型应归属的领域/职责目录，不继续追问字段命名、枚举值或组件 API。

### Suggested Action
在文档审查开始时声明范围门禁；发现字段级问题时仅记录为后续实现风险，除非它直接影响文件归属，否则不将其纳入当前决策树。

### Metadata
- Source: user_feedback
- Related Files: 架构设计/Entity ECS文件组织设计提案.md
- Tags: scope-control, documentation, grilling

---

## [LRN-20260903-001] correction

**Logged**: 2026-09-03T00:00:00+08:00
**Priority**: high
**Status**: resolved
**Area**: docs

### Summary
When removing architecture-specific constraints, preserve the requested general side-effect coding rules.

### Details
The request to remove concrete architecture design from `约束/项目代码规范/` was incorrectly treated as authorization to delete every document in that directory. The user clarified that side-effect coding constraints must remain. The correct scope is to remove ECS, layered-model, scheduling, and other implementation-architecture prescriptions while retaining general rules for explicit dependencies, state mutation, I/O, cancellation, retries, resources, idempotency, and testing.

### Suggested Action
Classify every rule by whether it constrains code behavior or prescribes a concrete architecture before deleting or consolidating documentation. Preserve the former unless the user explicitly asks to remove it.

### Metadata
- Source: user_feedback
- Related Files: 约束/项目代码规范/副作用编码约束.md; 约束/非函数式编码副作用隔离规范.md
- Tags: scope-control, documentation, side-effects, correction

### Resolution
- **Resolved**: 2026-09-03T00:00:00+08:00
- **Notes**: Restored side-effect coding constraints in architecture-neutral documents and repaired the entry-point link.

---

## [LRN-20260907-002] correction

**Logged**: 2026-09-07T21:20:00+08:00
**Priority**: medium
**Status**: resolved
**Area**: tooling

### Summary
字段/属性数量必须按语法成员声明统计，不能用最后一个成员的源码行号除以平均行距估算。

### Details
核对 `Version4/Terraria/Main.cs` 时，行号约 1480 的 `OnTickForInternalCodeOnly` 是事件，不是字段或属性；该文件的字段在 1296 行结束，属性在 1472 行结束，1474-1480 行还有 4 个事件，1481 行开始进入方法。Roslyn 快照的 542 是 504 个 `Terraria.Main` 直接字段、32 个直接属性和 6 个嵌套类型字段的文件内合计；若把事件也纳入，文件内源成员为 546。

### Suggested Action
报告必须先冻结成员口径（field/property/event、是否包含嵌套类型），再按语法树声明计数；使用源码行号只能定位边界，不能推导成员数量。

### Metadata
- Source: user_feedback
- Related Files: D:\\TRbackup\\Version4\\Terraria\\Main.cs; Build/Tools/Version4MemberMigrationScanner/Program.cs; Build/generated/version4-source-full-coverage-2026-09-07.json
- Tags: correction, member-count, roslyn, scope-control

### Resolution
- **Resolved**: 2026-09-07T21:20:00+08:00
- **Notes**: 已按 `field`、`property`、`event` 分离核对，并确认当前 542 的组成与源码行区间。

---
