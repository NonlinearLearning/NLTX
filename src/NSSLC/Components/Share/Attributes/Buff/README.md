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

本子域**不**负责：效果的具体表现（周期伤害、治疗、击退、召唤物维护）、
接收策略的内容来源、属性数值的最终结算。这些由 Buff 定义、Combat 与
调用方各自承担。

## 与根层的关系

Buff 定义不持有自己的贡献类型。`BuffDefinition` 直接用根层共有的
`IReadOnlyList<AttributeContribution>`，写入时通过
`AttributeSourceDescriptor` + `AttributeContributionSet` +
`AttributeContributionSystem` 走同一条路径，与十一类独立管线形态共用一套机制。

## 写入闭包

```text
BuffApplyIntent
    → BuffAdmissionSystem       检查接收能力、免疫、来源
    → BuffSystem                更新 BuffListComponent、增删 AttributeModifier
    → AttributeSystem           计算并推进属性
```

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
