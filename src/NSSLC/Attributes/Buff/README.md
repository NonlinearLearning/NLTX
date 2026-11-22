# Buff 子域：有时限的状态

本子域只承载"真正走 Buff 列表"的三类形态，其余广义 Buff 形态归
`Contribution/` 子域。

| 形态 | 存放 | 期限行为 |
| --- | --- | --- |
| 限时增益/减益 | `BuffListComponent` 条目 | `RemainingTicks` 每 tick 递减，归零移除 |
| 永久被动 | 同上 | `RemainingTicks = -1`，不递减 |
| 食物/饱腹 | 同上 | 同组互斥，由接收策略保证加新替旧 |

## 职责边界

本子域负责：效果标识、免疫键、剩余时间、层数、准入判定、刷新与叠层、过期移除。

本子域不直接提交生命、伤害或网络状态，但定义可以同时声明数值贡献、行为贡献和周期效果。
行为由对应行为组件保存，由目标域系统通过请求组件提交伤害或治疗。

## 定义的四类内容

| 内容 | 类型 | 用途 |
| --- | --- | --- |
| 数值贡献 | `AttributeContribution[]` | 进入 `AttributeSetComponent`，随来源整体撤销 |
| 行为贡献 | `BuffBehaviorContribution[]` | 物化为 `InvisibilityComponent`、`StunComponent`、`DamageImmunityStatusComponent` 等 |
| 周期效果 | `BuffPeriodicEffect[]` | 物化为 `LifeRegenerationComponent` / `DamageOverTimeComponent`，再由周期系统产生请求 |
| 即时效果 | 尚未建模 | 迁移债；当前用明确请求组件表达 |

`BuffBehaviorKind` 是行为分类键，不是属性键：行为不会以浮点 `StatusFlag` 写进属性系统。

## 与根层的关系

Buff 定义不持有自己的数值贡献类型。`BuffDefinition` 直接用根层共有的
`IReadOnlyList<AttributeContribution>`，并额外声明 `Behaviors` 与 `PeriodicEffects`，写入时通过
`AttributeSourceDescriptor` + `AttributeContributionSet` +
`AttributeContributionSystem` 走同一条路径，与十一类独立管线形态共用一套机制。

## 写入闭包

```text
BuffApplyRequestComponent                  （跨域输入，挂在目标实体上）
    → BuffApplyRequestSystem               （Attributes 域消费，移除请求）
        → BuffAdmissionSystem              检查接收能力、免疫、来源
        → BuffSystem                       更新 BuffListComponent、增删 AttributeModifier
    → BuffTickSystem                       推进唯一期限
    → StatusEffectBehaviorSystem           按来源同步行为组件
    → StatusEffectPeriodicSystem           产生 Life / Combat 请求
    → LifeRestoreRequestSystem / CombatDamageRequestSystem
```

`BuffSystem.TryApply` 只允许 `BuffApplyRequestSystem` 调用；外部域不得直接取用可变
`BuffListComponent` 或 `AttributeSetComponent`。

## 计时期限的唯一持有者

`BuffListComponent.AdvanceOneTick` 是 Buff 期限的**唯一**推进点。因此
`BuffSystem` 写入的 `AttributeModifier` 一律为永久修饰器（`RemainingTicks = -1`），
过期时由 `BuffSystem.Advance` 按 `SourceId` 整体撤销。

这样处理的目的是消除重复状态，不是修复已经存在的错误：实测表明两个推进点同时启用时，
Buff 剩余与修饰器剩余会保持同步（10→9→…→1，同一 tick 到期），并不会提前失效。
但只要只启用其中一个推进点，两份期限就会分叉——出现"Buff 在列表里但贡献已失效"
或反之。把期限收敛到一个持有者可以消除这个分叉面。

## 文件

| 文件 | 角色 |
| --- | --- |
| `BuffId` | 免疫条目标识 |
| `BuffEffectId` | 效果定义标识 |
| `ActiveBuff` | 一个活动条目（效果、免疫键、来源、剩余、层数） |
| `BuffListComponent` | 实体持有的条目集合 |
| `BuffDefinition` | 不可变内容定义 |
| `BuffDefinitionRegistry` | 定义查找 |
| `BuffAdmissionSystem` | 准入判定（纯函数） |
| `BuffAdmissionResult` | 准入结果 |
| `BuffApplicationResult` | 应用结果 |
| `BuffReceptionCapabilityComponent` | 接收能力声明 |
| `BuffReceptionMode` | 接收模式 |
| `BuffImmunityComponent` | 免疫表 |
| `BuffSystem` | 应用、刷新、叠层、过期 |
| `BuffTickSystem` | ECS 推进适配 |
| `BuffApplyRequestComponent` | 跨域应用意图（目标实体持有的请求组件） |
| `BuffApplyRequestSystem` | Attributes 域消费请求的唯一入口 |
| `BuffBehaviorContribution` / `BuffBehaviorKind` | 行为贡献声明与分类键 |
| `BuffPeriodicEffect` | 周期效果声明 |
| `LifeRegenerationComponent` | 周期治疗行为状态（按来源） |
| `DamageOverTimeComponent` | 周期伤害行为状态（按来源） |
| `InvisibilityComponent` | 隐身行为状态（按来源） |
| `StunComponent` | 眩晕 / 移动限制行为状态（按来源） |
| `DamageImmunityStatusComponent` | 状态来源提供的伤害免疫 |
| `StatusEffectBehaviorSystem` | 行为贡献 → 具体行为组件 |
| `StatusEffectPeriodicSystem` | 行为组件 → Life / Combat 请求 |
