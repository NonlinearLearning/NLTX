# Contribution 子域：独立管线形态的分类

本子域承载"不在 Buff 列表里存放、但同样以 `AttributeModifier` 向实体贡献数值"的
十一类形态。共有机制已上提到 `Attributes` 根层，本子域只保留**分类法**。

| 形态 | `AttributeContributionKind` | 来源分类 | 状态 owner |
| --- | --- | --- | --- |
| 装备与套装加成 | `SetBonus` | `SetBonus` | Inventory / 装备槽 |
| 饰品加成 | `Accessory` | `Accessory` | Inventory 饰品槽 |
| 武器前缀 | `Prefix` | `Prefix` | 物品实例 |
| 武器附魔 | `Imbue` | `Imbue` | 物品实例 |
| 召唤物/宠物/光照宠 | `Summon` | `Pet` | 召唤物实体 |
| 坐骑 | `Mount` | `Mount` | 坐骑子系统 |
| 幸运值 | `Luck` | `Environment` | 本域聚合 |
| 怪物旗帜减伤 | `Banner` | `Environment` | 世界级击杀计数 |
| 潜行/隐身 | `Concealment` | `StatusFlag` | 本域聚合 |
| 状态布尔镜像 | `StatusFlag` | `StatusFlag` | 各自的运行态 |
| 独立冷却计时器 | `Cooldown` | `Cooldown` | 冷却子系统 |

## 为什么不放进 Buff 子域

这些形态没有"效果标识 + 剩余时间 + 层数"的 Buff 列表语义：前缀修饰的是物品而不是
实体，旗帜由世界级计数驱动且不随时限过期，幸运值是多个来源汇入的标量。
把它们塞进 Buff 列表会强迫它们伪造一个不存在的定义和期限。

## 与根层的关系

十一类形态**没有**自己的贡献类型、集合类型或写入函数，全部复用根层的共有抽象：

```csharp
AttributeContributionSet set = new(
    new AttributeSourceDescriptor(
        7001,
        AttributeContributionKindMapping.ToSourceCategory(AttributeContributionKind.Luck)),
    [
      new AttributeContribution(AttributeKind.Luck, AttributeModifierOperation.Additive, 0.3f),
    ]);

AttributeContributionSystem.Apply(attributes, set);
```

| 共用类型 | 位置 |
| --- | --- |
| `AttributeContribution` | 根层 |
| `AttributeSourceDescriptor` | 根层 |
| `AttributeContributionSet` | 根层 |
| `AttributeContributionSystem` | 根层 |

本子域只补一个映射：把形态名翻译成 `AttributeSourceCategory`。

## 文件

| 文件 | 角色 |
| --- | --- |
| `AttributeContributionKind` | 十一类形态的分类 |
| `AttributeContributionKindMapping` | 形态 → 来源分类的映射 |

## 边界

本子域不持有状态、不推进时钟、不读写 ECS、不访问网络。来源的建立与失效由调用方决定。
内容实现（具体哪些套装给什么加成）留在各自领域，本子域只规定"如何分类"。
