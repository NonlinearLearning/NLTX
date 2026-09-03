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
