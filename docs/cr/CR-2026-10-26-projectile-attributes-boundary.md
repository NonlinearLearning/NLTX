# CR-2026-10-26：Projectile 穿透与免疫边界迁入 Attributes

## 原始需求

> Projectile 只处理穿透/免疫/终止的边界这些移动到属性系统计算

> Projectile只声明免疫组件能力list,里面存放免疫的东西

提出人：用户  
渠道：对话  
提出时间：2026-10-26（Asia/Shanghai）

## 分级与排期

- 分级：重大变更。
- 排期：当前轮实施。
- 方略：`spec`。
- 用户确认：用户确认先做二次评审，并批准评审方案在本轮实施。

## 二次评审决定

采用“Attributes 计算数值边界，Projectile 提交运行状态”的部分迁移：

| 状态/规则 | 权威 owner | 决定 |
| --- | --- | --- |
| 穿透命中上限 | Attributes | 使用 `ProjectilePenetrationLimit` 属性计算有效阈值 |
| 已接受命中数 | Projectile | `ProjectilePenetrationStateComponent` 只保存运行中的命中计数 |
| 命中免疫时长 | Attributes | 使用 `ProjectileHitImmunityTicks` 属性计算有效 Tick |
| 每目标免疫关系 | Projectile | `ProjectileHitImmunityStateComponent` 保存多个目标 handle 和到期 Tick |
| 是否请求终止 | Projectile | accepted receipt 提交后按属性阈值判断并写入终止请求 |
| 实际终止与销毁 | Projectile Lifecycle | 保持 `ProjectileTerminationCommitSystem` 与 `ProjectileLifecycleSystem` 边界 |
| 目标生命与伤害 | Combat | 保持 Damage admission/mitigation/commit 先提交，Projectile 再消费 receipt |

免疫组件的存在表示 Projectile 具备免疫能力，组件内部 list 使用带 world scope/generation 的
`EntityRuntimeHandle` 区分目标。同一目标刷新到期 Tick，不同目标同时保存；过期条目在提交时清理。
免疫目标关系不进入通用 Attributes 数值集合。

## 影响与验收

- `AttributeKind` 增加穿透上限与命中免疫 Tick 两个属性键。
- Projectile 定义为每个实例提供独立的 `AttributeSetComponent` 和穿透计数状态，不能跨实例共享可变属性集合。
- `ProjectileCombatCommitSystem` 不再保存系统级 `immunityTicks`，数值只从 Projectile Attributes 读取。
- 穿透属性按正值向上取整，`-1` 表示无限；免疫 Tick 按正值向上取整，零仍阻止同一 Tick 的重复命中。
- 需要覆盖：多目标免疫并存、同目标刷新、到期恢复、generation 隔离、属性修饰、穿透耗尽和单次终止请求。

## 非目标与回滚

Attributes 不保存目标关系、命中计数、终止状态或实体删除。回滚时恢复旧的单目标免疫组件、旧穿透
状态语义和 Combat 构造参数，并撤回新增属性键与 Projectile Attributes 接线；不回退本 CR 之外的工作区变更。

## 状态

已实施并完成本轮验收。构建、最小 Arch/Combat 冒烟和文档同步均通过；完整宿主 Tick 装配和
网络/存档行为仍不在本 CR 范围内。验收记录见
[Projectile Attributes 命中边界验收记录](../reviews/2026-10-26-projectile-attributes-boundary-review.md)。
