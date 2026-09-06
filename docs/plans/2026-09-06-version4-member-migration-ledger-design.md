# Version4 成员迁移覆盖账本设计

> 状态：已确认设计（2026-09-06）
>
> 本文是 AI 持续推进 Version4 → NLTX ECS 迁移的设计基线。它只定义成员级映射、AI
> 速查入口和审计门禁，不改变运行时代码。

## 1. 目标

建立一个以 Version4 原始字段/属性为起点的迁移覆盖账本，使每一次新的 AI 会话都能回答：

1. Version4 哪些字段和属性已经盘点；
2. 每个源成员当前的唯一迁移处置是什么；
3. 它是否已经对应到 NLTX 的组件字段、派生值、兼容适配或明确排除项；
4. 哪些读者、写者、生命周期和证据仍未关闭；
5. 当前应该继续处理哪一个批次、哪一个成员以及哪一个具体动作。

这个机制的目标不是让 AI 根据组件名称猜归属，而是让“没有映射记录”直接成为可检测的失败状态。

## 2. 范围和非目标

### 2.1 范围

- `D:\TRbackup\Version4` 是唯一完整覆盖基线。
- 继续沿用 `docs/Version4源码覆盖.tsv` 的文件级分类和责任子系统口径。
- 对进入组件迁移范围的 Version4 类型，登记其所有声明字段和属性，包括 private、public、
  static、readonly、const、auto-property 和 expression-bodied property；如果成员确实不属于
  ECS 运行时，仍然必须记录处置，不能静默跳过。
- NLTX 目标代码扫描范围包括 `src/` 和 `dome/src/` 中由目标索引声明的组件目录；测试、生成物、
  `Build/bin/`、`Build/obj/`、`Build/generated/` 和 `Build/packages/` 不作为组件目标。
- 方法、事件、构造函数和调用链不放入字段一一对应表，但必须保留在独立的行为/调用覆盖工作中，
  不能因为字段账本通过就宣称整个类型迁移完成。

### 2.2 非目标

- 不在本设计中修改 `src/` 或 `dome/src/` 的运行时行为。
- 不把文件存在、组件数量、命名相似或目录位置当作迁移完成证据。
- 不把派生属性、缓存、快照或兼容投影冒充为权威 ECS 状态。
- 不通过强行一对一来掩盖合理的拆分、合并或跨边界适配。

## 3. 产物布局

实现阶段在当前根项目的 `docs/migrations/` 下创建以下文件。第一个是唯一事实源，第二个是 AI 优先读取的速查视图，第三个用于反向
检查目标组件字段是否存在或成为无来源的权威字段。

```text
docs/migrations/
├── Version4-member-migration-map.json
├── Version4-member-migration-quick-reference.md
└── Version4-component-target-index.json
```

仓库中的 `dome/docs/migrations/` 是已有的历史迁移证据区。实现时可以读取其中与当前成员对应的
记录作为迁移上下文，但不能把它自动当作当前根项目账本，也不能覆盖或重写其中的历史记录；当前
根项目的 `docs/migrations/` 才是本设计定义的 AI 持续迁移入口。

审计和生成工具放在：

```text
Build/Tools/
├── New-Version4MemberMigrationMap.ps1
└── Test-Version4MemberMigrationMap.ps1
```

`Version4-member-migration-map.json` 是提交的权威输入；速查表和目标索引是可复查的派生产物，
生成结果应稳定排序，并由门禁检查是否与账本一致。中间文件只能写入 `Build/generated/`，不能
写入源码目录或项目目录旁边。

## 4. 成员身份和映射关系

### 4.1 源成员主键

每一条源字段/属性记录使用稳定的 `sourceMemberId`：

```text
Version4::<normalized-relative-path>::<fully-qualified-declaring-type>::<member-signature>
```

例如，格式示意如下：

```text
Version4::Terraria/Player.cs::Terraria.Player::statLife
```

规范化要求：

- 路径统一使用 `/`，相对于 `D:\TRbackup\Version4`；
- 类型名使用完整命名空间和类型名；嵌套类型使用明确的嵌套分隔形式；
- 字段/属性名称不能只靠名称区分重载，索引器、显式接口实现和特殊属性必须包含签名；
- 同一声明的证据行号不是身份的一部分，行号变化不能产生新成员；
- `sourceFingerprint` 保存声明签名或声明文本的稳定哈希，用于检测基线变化并触发复审。

### 4.2 目标成员身份

目标成员使用 `targetMemberId`：

```text
<target-root>::<component-relative-path>::<fully-qualified-component-type>::<member-signature>
```

目标组件字段必须通过实际文件和声明检查；不能只记录一个推测的组件名称。

### 4.3 一对一默认规则和显式例外

关系约束分两层：

```text
每个 sourceMemberId 必须且只能有一条账本记录。
每条账本记录必须且只能有一个 disposition。
disposition=move 时，必须恰好有一个权威 targetMember。
```

以下关系可以偏离一对一，但必须显式填写原因：

| disposition | 允许的目标关系 | 约束 |
| --- | --- | --- |
| `move` | 一个源成员 → 一个目标成员 | 默认路径，目标必须是实际权威组件字段 |
| `split` | 一个源成员 → 多个目标成员 | 必须填写 `splitReason`，并说明每个目标字段的语义 |
| `merge` | 多个源成员 → 一个目标成员 | 每个源记录都标记 `merge`，填写相同的 `mergeGroupId` 和合并理由 |
| `derive` | 一个源成员 → 派生属性/Query，或无权威目标 | 不得把派生值登记成持久化/网络权威字段 |
| `compatibility` | 一个源成员 → 兼容适配字段或投影 | 目标不能成为新的权威状态根 |
| `excluded` | 无目标 | 必须有排除理由和证据 |
| `deferred` | 暂无目标 | 必须有证据缺口、阻塞原因和可执行 `nextAction` |

因此，“一一对应”真正强制的是“每个源成员有且只有一个最终处置”，而不是对合理的语义拆分
和合并装作不存在。任何非一对一关系没有声明就视为审计失败。

## 5. 权威账本记录格式

实现阶段的 JSON 记录至少包含以下字段：

```json
{
  "sourceMemberId": "Version4::<path>::<type>::<member>",
  "source": {
    "path": "Terraria/SomeType.cs",
    "declaringType": "Terraria.SomeType",
    "member": "SomeField",
    "kind": "field",
    "type": "int",
    "accessibility": "private",
    "modifiers": ["readonly"],
    "sourceFingerprint": "sha256:..."
  },
  "classification": {
    "fileOwner": "NpcAndTownSimulation",
    "stateKind": "authoritative",
    "lifecycle": ["spawn", "update"],
    "scope": "entity"
  },
  "migration": {
    "disposition": "move",
    "status": "todo",
    "targetMembers": [
      {
        "targetMemberId": "src::Npc/NpcHealthComponent.cs::NpcHealthComponent::Current",
        "componentType": "NpcHealthComponent",
        "componentPath": "src/Npc/NpcHealthComponent.cs",
        "member": "Current",
        "role": "authoritative"
      }
    ],
    "evidence": [
      {
        "path": "D:/TRbackup/Version4/Terraria/SomeType.cs",
        "line": 1234,
        "reason": "声明和写入路径证据"
      }
    ],
    "readerWriterStatus": "open",
    "verificationStatus": "not-run",
    "batchId": "npc-health-001",
    "priority": 1,
    "nextAction": "迁移旧字段的权威写入者并复查快照读取",
    "blocker": null,
    "notes": null
  }
}
```

规则：

- `source` 描述原始声明事实；
- `classification` 描述状态语义和生命周期，不等于目标归属；
- `migration` 描述当前决定和工作进度；
- `evidence` 至少引用声明位置，权威、持久化、网络和生命周期成员还必须引用相应读者/写者证据；
- `targetMembers` 为空只允许用于 `derive`、`excluded` 或 `deferred`；
- `nextAction` 对未达到 `verified` 的记录必填；
- `notes` 不能替代结构化字段；
- `sourceFingerprint` 变化时，原记录自动进入 `needs-review`，不能继续沿用旧的 `verified`。

## 6. 状态机

`disposition` 和 `status` 分开。前者回答“如何处理”，后者回答“工作走到哪一步”。

```text
todo → in-progress → migrated → verified
  │          │            │
  ├──────────┴────────────┴──→ blocked
  └───────────────────────────→ deferred
```

补充规则：

- `excluded` 是处置，不是偷懒状态；必须有理由和证据；
- `deferred` 表示当前不能安全决策，不表示已完成；
- `blocked` 表示已确认有缺口，必须保留 `nextAction`；
- `migrated` 只表示目标字段和必要消费者已建立，不能替代行为验证；
- `verified` 必须有 verifier、结果或可复查证据；
- `verificationStatus=not-run` 时不得把记录升级为 `verified`；
- 某个 batch 只有在其中所有成员达到 `verified`、`excluded` 或带批准理由的 `deferred` 后，才可关闭；
- 存在 `missing` 证据的关键权威成员时，批次不能被 AI 标记为完成。

## 7. AI 速查视图

`Version4-member-migration-quick-reference.md` 不是第二个事实源，而是从 JSON 生成的、面向 AI
恢复工作的索引。它应按以下顺序组织：

1. 当前总览：源成员总数、已处理数、未处理数、阻塞数、各处置数量；
2. `nextAction` 可执行队列，按 `priority`、子系统、batchId 和源路径稳定排序；
3. 每个 batch 的成员表；
4. 当前阻塞项和证据缺口；
5. 已完成但需要复审的 fingerprint 变化；
6. 目标组件字段反向索引和无来源目标字段；
7. AI 禁止自行推断的规则。

每一个速查条目至少显示：

```text
sourceMemberId
source path/type/member
disposition
status
targetMemberId(s)
stateKind
readerWriterStatus
verificationStatus
evidence location
nextAction
blocker
```

AI 会话的恢复流程固定为：

```text
读取 quick-reference
  → 选择 priority 最高的 todo/in-progress/blocked 项
  → 阅读 Version4 声明及读写证据
  → 阅读目标组件字段和消费者
  → 更新账本记录
  → 执行成员映射审计
  → 仅在证据满足时更新状态
  → 重新生成 quick-reference
```

AI 不得从组件目录、文件数量、类名相似度或上一轮对话推导“已完成”。

## 8. 目标组件反向索引

审计器还要从 `src/` 和 `dome/src/` 的目标组件目录提取声明成员，形成：

```json
{
  "targetMemberId": "src::Npc/NpcHealthComponent.cs::NpcHealthComponent::Current",
  "componentPath": "src/Npc/NpcHealthComponent.cs",
  "componentType": "NpcHealthComponent",
  "member": "Current",
  "kind": "property",
  "authorityRole": "authoritative",
  "origin": "migrated"
}
```

目标成员需要明确来源类别：

- `migrated`：至少一个源成员通过 `move`、`split` 或 `merge` 指向它；
- `new`：新建的必要组件结构，必须填写设计理由，不能被误报为漏拆；
- `derived`：派生属性或计算视图，不得作为权威源；
- `integration`：跨子系统集成字段，必须标记所有权待审查；
- `compatibility`：兼容投影或适配字段，不得反向写入权威组件。

审计器要同时做正向检查和反向检查：

```text
Version4 member → target/disposition     防止源成员漏拆
target component member → source/origin  防止目标字段无来源或重复权威
```

## 9. 审计门禁

`Test-Version4MemberMigrationMap.ps1` 返回非零即表示账本不能用于继续迁移。至少检查：

### 9.1 源覆盖

- 每个纳入范围的源字段/属性恰好一条记录；
- `sourceMemberId` 无重复；
- 源路径、完整类型、成员名、成员类型和成员种类与当前 Version4 声明一致；
- fingerprint 变化的成员被标记为需要复审；
- 没有无法解释的源字段/属性遗漏。

### 9.2 处置和基数

- 每条记录恰好一个 `disposition`；
- `move` 恰好一个目标；
- `split`、`merge` 有理由和关系组；
- `derive`、`excluded`、`deferred` 没有伪造的权威目标；
- `deferred`、`blocked` 有证据缺口和 `nextAction`；
- 没有未经声明的多源共享权威目标字段。

### 9.3 目标反向覆盖

- 每个被引用的目标文件、组件类型和字段实际存在；
- 权威目标字段有合法来源，或明确标记为 `new` 并有理由；
- 同一个 `targetMemberId` 不被多个不相容的权威语义重复拥有；
- `derived` 和 `compatibility` 目标不能被当成权威状态根。

### 9.4 AI 续作安全

- 未完成记录都有稳定排序所需的 `priority` 和 `nextAction`；
- quick-reference 可由账本重新生成，且没有手工漂移；
- 当前 batch 的完成判断不能绕过 `verificationStatus`；
- 审计输出明确列出下一批应处理的成员和阻塞原因。

## 10. 迁移批次规则

批次以一个可独立审查的状态边界组织，而不是按文件数量机械切分。一个批次至少应包含：

- 相关源成员；
- 目标组件字段；
- 主要读者和写者；
- 生命周期边界；
- 证据入口；
- 下一步和验证器。

批次关闭条件是：

```text
所有源成员都有最终处置
∧ 所有 move/split/merge 目标存在
∧ 权威目标无未声明双写
∧ 关键读者/写者已迁移或显式阻塞
∧ verificationStatus 已有实际结果
```

只完成组件字段声明，不能关闭批次。

## 11. 失败和安全策略

- 无法确认字段语义时使用 `deferred` 或 `blocked`，不能用相似命名猜测归属；
- 发现源文件发生变化时暂停该成员的自动推进，先复核 fingerprint 和证据；
- 发现多个组件都声称拥有同一旧字段时，保持 `blocked`，不自动选择一个；
- 发现目标组件字段没有来源时，标记 `new`、`derived` 或 `integration`，并要求设计理由；
- 发现已有报告和当前源码不一致时，以当前只读源码和新证据为准，记录差异，不覆盖历史报告；
- 审计工具只写 `Build/generated/` 中的派生输出；不得自动删除源代码、组件代码或既有文档。

## 12. 验收标准

设计实现完成后，至少应满足：

1. 可以从 Version4 源码生成稳定的字段/属性候选集；
2. 可以从账本生成 AI 速查表；
3. 可以检测重复 `sourceMemberId`、缺失处置和非法目标基数；
4. 可以检测目标字段不存在、目标字段无来源和未声明的权威重复；
5. 任意一个新增或修改的源成员都会因 fingerprint 或覆盖差异进入待复审；
6. AI 能依据 `nextAction` 从上次中断位置继续，而不需要依赖聊天历史；
7. 审计输出不会把 `excluded`、`derived`、`compatibility` 或 `deferred` 混入“已迁移完成”；
8. 本阶段不修改运行时代码，且现有未提交改动保持不变。

## 13. 后续实现顺序

后续实施计划应按以下顺序拆成小批次：

1. 定义 JSON schema 和稳定身份规则；
2. 生成 Version4 成员候选集；
3. 生成 NLTX 目标组件字段索引；
4. 实现正向/反向审计门禁；
5. 生成 AI quick-reference；
6. 以一个已存在的组件边界做小范围回填和负例验证；
7. 扩展到全部已纳入范围的子系统；
8. 更新文档索引并记录实际验证结果。

任何运行时代码迁移都属于后续独立批次，必须先通过本账本的成员覆盖门禁。
