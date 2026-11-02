# Attributes 子系统划分：广义 Buff 与属性加成

```text
文档 ID：DOC-2026-11-01-attributes-subsystem-classification
逻辑域：reviews
产物类型：design
状态：active
范围：src/NSSLC/Components/Share/Attributes 下 27 个 .cs 文件（根 12 + Buff 15），
      以及参考客户端 D:\TRbackup\最新的客户端 中的 buff/前缀/属性聚合机制
证据入口：src/NSSLC/Components/Share/Attributes/**/*.cs
          D:\TRbackup\最新的客户端\Terraria\Player.cs
          D:\TRbackup\最新的客户端\Terraria.ID\BuffID.cs
          D:\TRbackup\最新的客户端\Terraria\Item.cs
canonical 路径：docs/reviews/audits/2026-11-01-attributes-subsystem-classification.md
```

## 1. 问题

`Attributes` 现在同时承担两件事：**数值修饰的聚合**（`AttributeModifier`）和
**广义 buff 的状态管理**（`Buff/` 子目录 15 个文件）。两者混在同一层，且 `Buff/`
的类型散落在 `Terraria.StatusEffects`（13 个）和 `Terraria.Combat`（2 个）两个命名空间。

本文回答两个问题：

1. 广义意义上的 buff 有哪些形态，应如何分类？
2. `Attributes` 实际需要哪些子系统？

## 2. 参考客户端的实证结论

参考客户端是反编译产物，是 **1.3 风格**的扁平字段模型，**不是** 1.4 的
`DamageClass` / `StatModifier` 模型。这一判断有三条硬证据：

| 证据 | 位置 |
| --- | --- |
| 全树搜索 `DamageClass`、`StatModifier`、`damageType` **零命中** | 全客户端 |
| 属性是扁平字段：`public int statDefense;`、`public float meleeDamage = 1f;` | `Terraria\Player.cs:1905`、`:2419`、`:2421` |
| 前缀用 10 个 out 参数逐个传出，再直接乘写回 `Item` 字段 | `Terraria\Item.cs:559` 起 |

### 2.1 Buff 的核心存储与 API

| 项目 | 事实 | 位置 |
| --- | --- | --- |
| 存储 | `int[] buffType`、`int[] buffTime`、`bool[] buffImmune` | `Player.cs:1493`、`:1495`、`:1497` |
| 添加 | `public void AddBuff(int type, int time, bool fromNetPvP = false)` | `Player.cs:5255` |
| 移除 | `public void DelBuff(int b)` | `Player.cs:5443` |
| 清空 | `public void ClearBuff(int type)` | `Player.cs:5464` |
| 查询 | `public bool HasBuff(int type)` **声明但全客户端零调用** | 声明处 `Player.cs` |
| NPC 侧 | 与 Player **完全同构**：`buffType`/`buffTime`/`buffImmune` | `NPC.cs:6205`、`:6207`、`:6209` |

NPC 与 Player 的 buff 存储形状一致，说明"共享一套 buff 规则"是合理抽象，不是强行统一。

### 2.2 Buff 身份注册表与元数据

`Terraria.ID\BuffID.cs` 共 976 行。结构分两块：

**（一）常量区**：约 400 个 `public const int`（`Count = 401`，`BuffID.cs:973`），
纯 id 定义，**自身不含元数据**。另有 `Search = IdDictionary.Create<BuffID, int>()` 供名称反查。

**（二）`BuffID.Sets` 嵌套类**（`BuffID.cs:10` 起）：**这才是元数据所在**，共 12 组表。
这修正了本文早期版本"buff 无任何元数据"的错误判断：

| Sets 成员 | 含义 | 行号 |
| --- | --- | --- |
| `IsWellFed` | 饱腹类（26/206/207） | 14 |
| `IsFedState` | 进食状态，含甜品（26/206/207/332/333/334） | 16 |
| `SortingPriorityFoodBuffs` | 食物 buff 排序优先级 | 18 |
| `IsAnNPCWhipDebuff` | 鞭子专属对 NPC 减益（310/362） | 20 |
| `TimeLeftDoesNotDecrease` | 永久类，时间不递减 | 22 |
| `CanBeRemovedByNetMessage` | 网络可移除白名单 | 24 |
| `IsAFlaskBuff` | 药剂瓶附魔类（71–79） | 26 |
| `BuffTimeIsExtendedWithGameDifficulty` | 难度延长时长 | 28 |
| `BuffTimeIsExtendedByDeadCellsPotionStationBuff` | 药水站延长时长 | 30 |
| `NurseCannotRemoveDebuff` | 护士无法清除 | 32 |
| `AddBuffTimeAdditivelyToCap` | 叠加封顶时长 | 34 |
| `BuffTextHandlers` | 自定义文本处理器（召唤物计数显示） | 36 |
| `MountType` | buff → 坐骑映射（约 60 项） | 152 |

**重要对照**：本地 `Version4PlayerBuffCatalog.cs` 已复刻其中 6 组，并自建 2 组扩展：

| Sets 成员 | 本地对应方法 |
| --- | --- |
| `TimeLeftDoesNotDecrease` | `TimeLeftDoesNotDecrease` |
| `IsFedState` | `IsFedState` |
| `AddBuffTimeAdditivelyToCap` | `AdditiveTimeCap` |
| `BuffTimeIsExtendedByDeadCellsPotionStationBuff` | `IsExtendedByPotionStation` |
| `BuffTimeIsExtendedWithGameDifficulty` | `IsExtendedByDifficulty` |
| （`IsLightPet`/`IsVanityPet` 在参考端是散列判断，非 Sets 成员） | `IsLightPet`、`IsVanityPet` |
| —（本地扩展） | `IsDebuff`、`PersistsAfterDeath`、`IsStinky`、`IsMeleeBuff` |

本地**尚未覆盖** 6 组：`MountType`、`IsAnNPCWhipDebuff`、`CanBeRemovedByNetMessage`、
`NurseCannotRemoveDebuff`、`IsAFlaskBuff`、`BuffTextHandlers`。
另经全仓搜索，本地**完全没有**坐骑（`MountType`）与旗帜（`BannerStrength`）概念。

### 2.3 效果如何实现——散落硬编码

| 指标 | 数值 |
| --- | --- |
| `Player.cs` 中 `buffType[x] == 数字` 形式的内联比较 | **279 处** |
| `Player.cs` 中 `BuffID.X` 命名引用 | 仅 13 处 |
| 专门的 buff 分组方法 | 仅 `UpdateBuffs_NebulaBuffs` 1 个（`Player.cs:12157`） |
| buff 主逻辑 | `UpdateBuffs(int i)`（`Player.cs:9942`） |

结论：buff 的效果**没有数据驱动表**，而是散落在各更新方法里的数字字面量分支。典型样本：

- `Player.cs:9962` `if (buffType[j] == 1)`（ObsidianSkin）
- `Player.cs:9972` `if (buffType[j] == 131)`（TurtleMount）
- `Player.cs:10014` `if (buffType[j] == 159)`（Sharpened）

### 2.4 描述与显示

本地化键 `BuffName` / `BuffDescription` 走 `Lang.GetBuffName(int)` / `GetBuffDescription(int)`
（`Lang.cs:241`、`:246`），按 `BuffID.Count` 预填缓存（`:521`、`:522`）。

### 2.5 属性聚合机制

核心是 **`ResetEffects()`**（`Player.cs:18389`）：每帧先把派生字段归零或归位，再由装备、
buff、饰品等来源重新累加。这是"属性加成"子系统的机制本质——**每帧重建，而非增量维护**。

`UpdateEquips(int i)`（`Player.cs:12987`）与 `UpdateBuffs(int i)`（`Player.cs:9942`）是
**两个独立方法**，证明"装备加成"与"buff"在源实现里就是两条不同管线。

## 3. 广义 buff 的形态清单

把"广泛意义上的 buff"还原为概念，参考客户端里实际存在以下 **14 类**：

| # | 形态 | 参考客户端表现 | 关键差异 |
| --- | --- | --- | --- |
| 1 | **限时增益/减益** | `buffType`/`buffTime` 数组，`AddBuff` | 有倒计时，会自然过期 |
| 2 | **永久被动** | 同样存在 buff 数组，但 `buffTime` 不递减 | `TimeLeftDoesNotDecrease`（`BuffID.cs:22`） |
| 3 | **食物/饱腹** | `WellFed`(26)、`WellFed2`(206)、`WellFed3`(207) | 同组互斥，加新替旧（`IsFedState`） |
| 4 | **装备与套装加成** | `UpdateEquips`，`ArmorSetBonuses` | 无倒计时，随装备装卸生灭 |
| 5 | **饰品加成** | 同上，按饰品槽 | 同上，但槽位规则不同 |
| 6 | **武器前缀** | `Item.Prefix(int)`，10 个 out 参数 | 修饰的是**物品**，非实体 |
| 7 | **武器附魔（imbue）** | `WeaponImbue*`(71,73–79) | 作用于弹幕伤害类型（`IsAFlaskBuff`） |
| 8 | **召唤物/宠物/光照宠** | `IsLightPet`、`IsVanityPet`、`Minion` 系列 | 互斥占用，需实体维护 |
| 9 | **坐骑** | `BuffID.Sets.MountType`（约 60 项映射） | buff 仅作载体，实际状态在 `Mount` 对象 |
| 10 | **幸运值** | `Player.luck` + `RecalculateLuck()` | 独立字段，聚合 6+ 种来源（见 §3.2） |
| 11 | **怪物旗帜减伤** | `HasNPCBannerBuff` + `ItemID.Sets.BannerStrength` | 由**世界级击杀计数**驱动，非时限状态 |
| 12 | **潜行/隐身** | `Player.stealth`、`Player.invis` | 连续量而非布尔；直接影响伤害与仇恨 |
| 13 | **状态布尔镜像** | `onFire`/`venom`/`bleed`/`confused`/`daybreak` | **是 buff 数组的第二份表示**（见 §3.1） |
| 14 | **独立冷却计时器** | `potionDelay`/`restorationDelay`/`mushroomDelay`/`breath` | 不在 buff 数组内，但语义上是限时状态 |

### 3.1 状态布尔镜像——同一状态的两份表示

`Player.cs` 与 `NPC.cs` 都同时保存 buff 数组**和**一组行为布尔：

| 字段 | Player | NPC |
| --- | --- | --- |
| `onFire` / `onFire2` / `onFire3` | `:2309`、`:2311`、`:2313` | `:6219`、`:6221`、`:6223` |
| `venom` | `:2273` | `:6231` |
| `bleed` / `bleeding` | `:2345` | `:6239` |
| `confused` | `:2347` | `:6263` |
| `daybreak` | — | `:6287` |
| `betsysCurse` | — | `:6291` |

这些布尔在 `UpdateBuffs` 中**根据 buff 数组镜像赋值**（如 `Player.cs:10118`、`:11796`、
`:11825`），并在 `ResetEffects` 中清空（`:18783–18809`）。它们是同一状态的第二份表示，
是本仓若要统一时必须处理的重复点。

### 3.2 幸运值的多来源聚合

`Player.RecalculateLuck()`（`Player.cs:29363`）：

```text
luck = GetLadyBugLuck() * 0.2
     + torchLuck * 0.2
     + luckPotion * 0.1
     + kiteLuckLevel * 0.1 / 3
     + (环境/事件加成 0.03 / 0.2 / 0.3)
     + equipmentBasedLuckBonus
     + CalculateCoinLuck()
```

这是"属性聚合"的又一实例——7 类来源汇入一个 `float`。与 `ResetEffects` 的区别是它有
独立的 `RecalculateLuck()` 入口，不走主属性重建流程。

另有 **3 类近似但不同** 的概念，不应并入 buff：

| 概念 | 说明 |
| --- | --- |
| **免疫表** `buffImmune` | 是"拒绝资格"，不是状态；与 buff 生命周期无关 |
| **受击无敌/闪避冷却** | `ImmunityCooldownID`（6 槽）+ `immuneTime`，是战斗时序 |
| **机制冷却** | `MechanismCooldownService`，属世界机制，非实体状态 |

### 3.3 `ImmunityCooldownID` 与本地实现的对应

`ImmunityCooldownID.cs` 定义 6 个槽位：`General`(-1)、`TileContactDamage`(0)、
`BossNoCheese`(1)、`LegacyUnused2`(2)、`WrongBugNet`(3)、`Lava`(4)、`PaladinsShield`(5)。

本地 `PlayerHurtCooldowns.cs` 的六个字段（`TileContactDamage`、`BossNoCheese`、
`LegacyUnused2`、`WrongBugNet`、`Lava`、`PaladinsShield`）与之**逐一对应**，已迁移到位。

参考客户端还带了 `ImmunityCooldownID.Sets` 四组行为表：
`Retaliate`、`Counter`、`TeamDamageShare`、`ImmuneTimerOnlyLimitsEffects`。
本地**尚未迁移**这四组。

### 3.4 `ArmorSetBonuses`：约 50 个具名套装效果

`Terraria.DataStructures\ArmorSetBonuses.cs`（547 行）含 `Benefits` 静态类，约 50 个
`public static void` 方法（`Tiki`、`Spooky`、`Bee`、`Solar`、`Vortex`、`MythrilMelee`…
`Wood`、`AshWood`），每个方法**直接改 `player` 的字段**，例如：

```csharp
public static void Tiki(Player player) {
    player.maxMinions++;
    player.whipRangeMultiplier += 0.2f;
}
```

配套还有 `Initialize()`、`BuildLookup()`、`Add(...)` 以及 `ArmorSetBonus.ArmorSetEffect`
枚举和本地化 `TextKey`。这是**数据驱动 + 手写效果**的混合体：查找是表的，效果是代码的。

## 4. 本仓现状核查

### 4.1 已实现

| 子系统 | 文件 | 状态 |
| --- | --- | --- |
| 属性聚合 | `AttributeModifier`/`AttributeCalculator`/`AttributeSetComponent` | ✅ 已实现，**有消费者** |
| 属性推进 | `AttributeTickSystem` | ⚠️ 已实现但**未注册** |
| Buff 状态 | `BuffListComponent`/`ActiveBuff`/`BuffSystem` | ✅ 已实现，**无消费者** |
| Buff 准入 | `BuffAdmissionSystem`/`BuffReceptionCapabilityComponent` | ✅ 已实现 |
| Buff 推进 | `BuffTickSystem` | ⚠️ 已实现但**未注册** |
| 免疫 | `BuffImmunityComponent` | ✅ 已实现 |
| 装备/饰品/武器加成 | `InventoryAttributeContributionSystem` | ✅ 已实现，有消费者 |
| 重力修饰 | `Movement/GravityModifier*` | ✅ 独立平行系统 |

### 4.2 三项结构问题（实测）

**问题一：`Terraria.StatusEffects` 全仓零外部引用（已修复）。**

`Buff/` 下原有 13 个文件声明 `namespace Terraria.StatusEffects;`，ripgrep 全 `src/`
未发现任何该命名空间的 `using`。即：**这套 Buff 系统是孤岛，没有任何生产代码使用它**。

对照：`Terraria.Attributes`（Core 层）有 8 处外部消费者——
`Inventory`（1）、`Item`（2）、`Combat`（4）、`Projectile`（3）。
两相对比可确认：**Core 已接线，Buff 未接线**。

另有 2 个文件（`BuffId.cs`、`BuffImmunityComponent.cs`）声明 `namespace Terraria.Combat;`，
与同目录其余 13 个文件不同源，构成目录内命名空间分裂。

两者现已统一为 `Terraria.Attributes`：因 `Terraria.StatusEffects` 零外部消费者，
且 `BuffId`/`BuffImmunityComponent` 的引用全部位于 `Buff/` 内部，统一零破坏，见 §9。

**问题二：Buff 期限被存了两份（冗余状态，非活动缺陷）。**

`BuffSystem.AddAttributeModifiers` 把 `activeBuff.RemainingTicks` 传给
`AttributeModifier`（原 `BuffSystem.cs:122`），而 `BuffListComponent.AdvanceOneTick()`
与 `AttributeSystem.Advance()` 各自递减同一个期限。

> **更正**：本文早期版本断言这会导致"数值贡献比 buff 提前一半时间失效"。该断言**错误**。
> 已用一次性校验工程实测：两个推进点同时启用时，Buff 剩余与修饰器剩余保持同步
> （10→9→…→1，同一 tick 到期），防御值在第 10 tick 才回落。不存在提前失效。
>
> 真实问题是**重复状态**：只要只启用其中一个推进点，两份期限就会分叉，出现
> "Buff 在列表里但贡献已失效"或反之。已按"单一持有者"收敛，见 §9。

**问题三：`GravityModifier` 与 `AttributeModifier` 结构同构但完全平行。**

| 字段 | `AttributeModifier` | `GravityModifier` |
| --- | --- | --- |
| 来源标识 | `SourceId` | `SourceId` |
| 运算 | `AttributeModifierOperation`（Additive/Multiplicative/Override） | `GravityModifierKind`（MagnitudeAdditive/Multiplier/Override…） |
| 数值 | `Value` | `Value` |
| 优先级 | `Priority` | `Priority` |
| 剩余期限 | `RemainingTicks` | `RemainingFrames` |
| 序号 | `Sequence` | `Sequence` |
| 过期推进 | `AdvanceOneTick()` | `AdvanceOneFrame()` |

两者是同一模式的两次实现。

## 5. 子系统划分

按"十四类形态共有什么"划分：根层只放共有抽象，形态专属部分进两个子域。
本结构已实施，见 §9。

```text
Attributes/               # 根层 = 十四类共有抽象
  Buff/                   # 子域一：3 类走 Buff 列表（限时、永久、食物）
  Contribution/           # 子域二：11 类独立管线形态
```

### 5.1 根层：十四类共有抽象

| 类型 | 种类 | 职责 |
| --- | --- | --- |
| `AttributeKind`、`DamageElement` | 枚举 | 属性键定义 |
| `AttributeModifier`、`AttributeModifierOperation`、`AttributeSourceCategory` | 值对象 | 修饰器与来源分类 |
| `AttributeContribution` | readonly record struct | 一条贡献：属性键 + 运算 + 数值 + 优先级 |
| `AttributeSourceDescriptor` | readonly record struct | 来源身份 + 分类 + 剩余期限 |
| `AttributeContributionSet` | sealed class | 一个来源的一组贡献 |
| `AttributeContributionSystem` | static class | 贡献 → 修饰器，委托 `AttributeSystem` |
| `AttributeBaseValue`、`AttributeSetComponent` | 组件 | 唯一的 ECS 状态容器 |
| `AttributeCalculator`、`AttributeCombatCalculator`、`CriticalRollInput` | 纯计算 | 无副作用 |
| `AttributeSystem`、`AttributeTickSystem` | 系统 | 修饰器生命周期 |

### 5.2 Buff：有时限的状态机

15 个文件，期限由 `BuffListComponent` 单一持有（见 §4.2 问题二）。
**只承载 §3 中第 1–3 类**（限时、永久、食物）。其余形态不塞进这里。

### 5.3 Contribution：独立管线的分类法

原先把"定义 + 集合 + 写入"都放在此子域，导致与 Buff 子域、物品侧各存一份形状相同的
定义。现把机制上提到根层，本子域只保留分类法与映射：

| 文件 | 职责 |
| --- | --- |
| `AttributeContributionKind` | 十一类形态的分类 |
| `AttributeContributionKindMapping` | 形态 → `AttributeSourceCategory` |

套装加成（§3.4 的约 50 个 `Benefits` 方法）今后应表达为多条
`AttributeContribution`，而不是直改实体字段；该迁移尚未进行，见 §9.5。

## 6. 待决策项

| 项 | 选项 | 说明 |
| --- | --- | --- |
| 命名空间 | 已定：统一为 `Terraria.Attributes` | 已完成，`BuffId`/`BuffImmunityComponent` 已从 `Terraria.Combat` 迁出 |
| `GravityModifier` | (a) 收编为 `AttributeKind` 的通道；(b) 保持独立 | 收编可消重，但重力有 `Direction` 这类非数值维度 |
| 物品侧定义 | (a) 迁移到 `AttributeContribution`；(b) 保留 `ItemAttributeModifierDefinition` | 它带 `SourceCategory` 与 `IsInventorySource`，需先定物品侧分类归属 |
| 前缀系统 | (a) 接入 Attributes；(b) 保持现状 | 当前共享层只有 `byte Prefix` 字段，无定义与应用（`ItemStack.cs:27`），属未实现 |
| 遗留类型 | `LiquidComponent` 同类的占位是否清理 | 与本域无关，仅登记 |

## 7. 未验证事项

以下**未**核实，不应作为结论引用：

- 未统计参考客户端全部文件的 `buffType[x] ==` 总数（仅精确统计了 `Player.cs` 的 279 处）；
- 未逐一列举 401 个 `BuffID` 归属到 §3 的 14 类形态（只抽样验证了每类形态存在）；
- 两个并行调研子代理在运行约 45 分钟后被手动中断，全文引用的证据均由主进程独立复核，
  未采用子代理未经核验的中间结论。

## 8. 结论

1. 参考客户端**没有**统一的属性抽象：buff 是数组 + 散落数字分支，属性是扁平字段 + 每帧
   `ResetEffects` 重建，前缀是 10 个 out 参数直写物品字段。
2. 广义 buff 可归为 **14 类形态**。其中只有 3 类真正走 buff 数组（限时、永久、食物）；
   其余 11 类各有独立管线：装备套装、饰品、前缀、附魔、召唤物、坐骑、幸运值、旗帜、
   潜行、状态布尔镜像、独立冷却计时器。
3. `BuffID.Sets` 提供了 12 组元数据表，是"哪些 buff 属于哪一类"的权威依据。本地已复刻
   6 组并自建 2 组扩展，尚缺 6 组（`MountType`、`IsAnNPCWhipDebuff`、
   `CanBeRemovedByNetMessage`、`NurseCannotRemoveDebuff`、`IsAFlaskBuff`、`BuffTextHandlers`）。
4. 同一状态存在**双份表示**：buff 数组 + 行为布尔（`onFire`/`venom`/`bleed`/`confused`…），
   后者由前者镜像赋值。本仓若要统一，必须明确哪一份是权威。
5. 本仓的 `Attributes` **已经**把"数值聚合"这一层做得比源实现好；缺的是把上述形态收拢
   到明确子域。该收拢已按 §9 实施。
6. `Buff/` 目前是零消费者的孤岛，这是划分工作的最大风险点：任何重构都没有回归基线。

## 9. 实施记录

按"3 类走 buff 数组 / 11 类独立管线"的划分完成了代码改动。

### 9.1 目录

```text
Attributes/               # 根层 = 统一抽象
  Buff/                   # 子域一：3 类（限时、永久、食物）
  Contribution/           # 子域二：11 类独立管线形态
```

### 9.2 改动清单

| 改动 | 文件 | 说明 |
| --- | --- | --- |
| 扩展属性通道 | `AttributeKind.cs` | 28 → 40，新增 `Luck`、`Stealth`、`Aggro`、`BannerDamageReduction`、`MinionCapacity`、`WhipRange`、`PotionCooldownTicks`、`RestorationCooldownTicks`、`MushroomCooldownTicks`、`BreathCapacityTicks`、`MountKind`、`StatusFlag` |
| 扩展来源分类 | `AttributeSourceCategory.cs` | 8 → 16，新增 `SetBonus`、`Prefix`、`Imbue`、`Pet`、`Mount`、`Environment`、`StatusFlag`、`Cooldown` |
| 提取共有抽象 | 根层 3 个新文件 | `AttributeContribution`、`AttributeSourceDescriptor`、`AttributeContributionSet` |
| 共有写入边界 | 根层 `AttributeContributionSystem.cs` | `Apply`/`Revoke`/`RevokeCategory`，内部委托 `AttributeSystem` |
| 子域只留分类法 | `Contribution/` | 收敛为 `AttributeContributionKind`（11 类）+ `AttributeContributionKindMapping` |
| 删除重复定义 | 4 个文件 | `BuffAttributeModifierDefinition`、`AttributeContributionDefinition`，以及被上提的旧 `AttributeContributionSet`/`AttributeContributionSystem` |
| 统一命名空间 | `Buff/` 15 个文件 | `Terraria.StatusEffects` 与 `Terraria.Combat` → `Terraria.Attributes`；`Terraria.Combat.BuffId` → `BuffId` |
| 期限单一持有 | `BuffSystem.cs` | 修饰器改为永久（`remainingTicks: -1`），期限只由 `BuffListComponent` 推进 |
| Buff 复用共有抽象 | `BuffDefinition.cs` | `IReadOnlyList<BuffAttributeModifierDefinition>` → `IReadOnlyList<AttributeContribution>` |
| 文档 | `Attributes/README.md`、`Buff/README.md`、`Contribution/README.md` | 记录共有抽象与子域边界 |

### 9.3 共有抽象清单

十四类形态共有的四件事，各对应一个根层类型：

| 类型 | 种类 | 共有的事实 |
| --- | --- | --- |
| `AttributeContribution` | readonly record struct | 贡献什么：属性键 + 运算 + 数值 + 优先级 |
| `AttributeSourceDescriptor` | readonly record struct | 谁贡献、哪一类、活多久 |
| `AttributeContributionSet` | sealed class | 一个来源的一组贡献，整体加入/撤销 |
| `AttributeContributionSystem` | static class | 贡献 → 修饰器的翻译，委托 `AttributeSystem` 增删 |

`AttributeSetComponent` 是唯一的 ECS 状态组件；以上四个是定义侧与写入侧值对象，不入实体存储。

提取后三个近似重复的定义收敛为一个：原 `BuffAttributeModifierDefinition`（Buff）、
`ItemAttributeModifierDefinition`（物品，仍在 `Terraria.Item`）、
`AttributeContributionDefinition`（独立管线）字段形状相同，前两者已由
`AttributeContribution` 取代一个；物品侧尚未迁移，见 §9.5。

### 9.4 验证

- `src/NSSLC/Terraria.Components.csproj`：0 警告 0 错误。
- `src/NSSLC.Server/NSSLC.Server.csproj`：0 警告 0 错误。
- 一次性校验工程 **28 条断言**全部通过，运行后已连同输出删除。覆盖：贡献值与层数放大、
  来源描述符的永久/限时推进与到期信号、共写入边界的幂等重登记（替换而非叠加）、
  按来源与按分类撤销、11 类形态映射全覆盖、Buff 复用共有抽象后的期限单一持有、
  免疫准入拒绝、刷新叠层。

### 9.5 未覆盖

- `Contribution/` 只提供**分类法与映射**，不包含具体内容实现。哪些套装给什么加成、
  哪些前缀对应哪些数值，仍留在各自领域，尚未迁移。
- `ItemAttributeModifierDefinition` 仍是 `AttributeContribution` 之外的第二份定义，
  未迁移（它带 `SourceCategory` 与 `IsInventorySource`，需先确定物品侧的分类归属）。
- `GravityModifier`、旗帜、坐骑、幸运值尚未接入共有抽象。
- `BuffTickSystem` 与 `AttributeTickSystem` 仍未在服务端注册，因此没有端到端运行基线。
