# 参考客户端掉落表与 NPC 战利品系统调研

**文档 ID**：DOC-2026-11-25-NPC-LOOT-SYSTEM-SURVEY
**逻辑域**：research / 正在考虑的系统
**产物类型**：evidence（静态阅读，未运行参考客户端）
**状态**：proposal——只做现状描述与约束提炼，未提出 NLTX 落地批次
**日期**：2026-11-25（Asia/Shanghai）
**参考客户端**：`D:\TRbackup\最新的客户端`（Terraria v1.4.5.8 反编译快照，1579 个 `.cs`，750,505 行）
**阅读范围**：`Terraria.GameContent.ItemDropRules/**`（37 文件）、`Terraria.GameContent.FishDropRules/**`（9 文件）、`Terraria.GameContent/Luck.cs`、`NPC.cs` 的 `NPCLoot` 链路、`Player.cs` 的 `OpenBossBag`、`Item.cs` 与 `Projectile.cs` 的相关段落

---

## 1. 问题陈述

参考客户端**没有掉落表文件**。全部战利品都是 C# 硬编码，且分散在**至少五个互不相通的位置**。任何"把掉落做成数据"的尝试，若不先分清这五处的边界，会得到一个既不能解释现状、也覆盖不全的 schema。

本文件回答三件事：

1. 掉落决策到底在哪些文件里、各自负责哪一段；
2. 这些代码的真实形状（规则树、条件、随机、链式），以及它们对数据化的抵抗点；
3. 哪些部分是纯数据、哪些部分是**逻辑**，从而划出可机械化提取的边界。

---

## 2. 五处载体：掉落不在一个地方

| # | 载体 | 位置 | 形态 | 规模 |
| --- | --- | --- | --- | --- |
| ① | NPC 掉落规则树 | `ItemDropRules/ItemDropDatabase.cs` | 命令式注册，构造规则对象树 | 425 次注册调用，42 个 `Register*` 方法 |
| ② | 硬编码即时掉落 | `NPC.cs` 的 `NPCLoot*` 与 `DoDeathEvents*` | 直接 `Item.NewItem(...)` | 47 处 `Item.NewItem`（其中 46 处走 `GetItemSource_Loot()`），115 个 `case` 分支 |
| ③ | Boss 袋内容 | `Player.cs:7206 OpenBossBag` | 巨型 `switch(type)` + `QuickSpawnItem` | `Player.cs` 全文件 399 处 `QuickSpawnItem` |
| ④ | 钓鱼掉落 | `FishDropRules/**` | 有序规则列表，首条命中即返回 | 9 文件，约 25 KB |
| ⑤ | 概率总闸 | `Terraria.GameContent/Luck.cs` | 60 行静态类 | 5 个 `Roll*` 方法 |

**关键判断**：①只覆盖"NPC 被击杀时按规则表掉的东西"。金币、红心蓝星、Boss 药水与心、Boss 袋里的实际内容，**都不在①里**。这四类各自有独立的实现路径，且都直接产出物品。把它们当成①的一部分会让 schema 失真。

---

## 3. 载体①：规则树

### 3.1 触发链

`NPCLoot()`（`NPC.cs:79523`）是唯一入口，时序为：

```
NPCLoot()
  ├─ 提前退出：netMode==1 / type >= NPCID.Count / 无地表世界的特定怪
  ├─ 成就 + 图鉴击杀登记 + 旗帜（均需 GetWereThereAnyInteractions()）
  ├─ 雕像过滤（见 §3.10）
  ├─ DoDeathEvents_BeforeLoot()          // 双子残党把 value/boss 清零
  ├─ NPCLoot_DropItems()                 // → 载体①
  ├─ DoDeathEvents()                     // → 载体②（波次推进、事件标志、硬编码掉落）
  ├─ 特殊世界补偿掉落（祭坛/地牢/地狱炉/蜥蜴/神庙陷阱）
  ├─ NPCLoot_DropMoney()                 // → 载体②（金币）
  └─ NPCLoot_DropHeals()                 // → 载体②（红心/蓝星）
```

`NPCLoot_DropItems()` 只有 12 行：构造 `DropAttemptInfo` 后调 `Main.ItemDropSolver.TryDropping(info)`。**这是①与外界的唯一接口。**

### 3.2 解析器

`ItemDropResolver`（59 行）逻辑完整：

```csharp
// 拉取规则
List<IItemDropRule> rules = _database.GetRulesForNPCID(info.npc.netID);
for (每个 rule) ResolveRule(rule, info);

// 单条解析
if (!rule.CanDrop(info)) {
    result = DoesntFillConditions;
    ResolveRuleChains(rule, info, result);   // 条件不满足也走链
    return result;
}
result = rule is INestedItemDropRule n
    ? n.TryDroppingItem(info, ResolveRule)   // 嵌套：递归自己
    : rule.TryDroppingItem(info);
ResolveRuleChains(rule, info, result);
```

注意两点：

- `GetRulesForNPCID` 默认 `includeGlobalDrops: true`，**全局规则（20 条）排在 NPC 规则前面**。全局规则内部自己判断条件。
- 条件不满足时**仍然走链**（`OnFailedConditions` 就靠这个工作），不是短路。

### 3.3 结果四态

`ItemDropAttemptResultState` 是整棵树的状态机：

| 状态 | 含义 | 谁会消费它 |
| --- | --- | --- |
| `Success` | 掉出来了 | `OnSuccess` 链 |
| `FailedRandomRoll` | 条件过了但骰子没过 | `OnFailedRoll` 链（保底逻辑） |
| `DoesntFillConditions` | 条件没过 | `OnFailedConditions` 链 |
| `DidNotRunCode` | **没人干活** | 无——这是个陷阱 |

`DidNotRunCode` 由所有 `INestedItemDropRule` 的单参 `TryDroppingItem` 返回。解析器只对嵌套规则走双参重载，因此正常路径下不会出现；但**任何外部代码直接调单参版本都会静默什么都不掉**。

### 3.4 规则类型：15 个实现，6 个是嵌套

| 类别 | 规则 | 概率来源 |
| --- | --- | --- |
| 基础掉落 | `CommonDrop`（含 4 个子类）、`ItemDropWithConditionRule`、`DropPerPlayerOnThePlayer`、`DropLocalPerClientAndResetsNPCMoneyTo0` | 吃运气 / 不吃 / 只吃坏运气（三种基类） |
| 多选一 | `OneFromOptionsDropRule`、`OneFromOptionsNotScaledWithLuckDropRule`、`FromOptionsWithoutRepeatsDropRule` | 均匀 + 分母 |
| 分组批量 | `DropOneByOne` | 参数结构体，含"每多一个玩家加几件" |
| 条件包装 | `LeadingConditionRule` | 自身恒 `Success`，条件由 `CanDrop` 承担 |
| 空规则 | `DropNothing` | 恒 `DoesntFillConditions` |
| **嵌套** | `DropBasedOnExpertMode`、`DropBasedOnMasterMode`、`DropBasedOnMasterAndExpertMode`、`DropBasedOnExtraGel`、`OneFromRulesRule` | 运行时按 `info.IsExpertMode/IsMasterMode` 或 `SpecialSeedFeatures` 选分支 |
| 特化 | `MechBossSpawnersDropRule`、`SlimeBodyItemDropRule`、`StatueMimicItemDropRule` | 读 `npc.ai[1]` / `npc.value`，**不是静态数据** |

**嵌套规则是最强的数据化抵抗点**：`DropBasedOnExpertMode` 的 `ReportDroprates` 会同时输出专家与普通两套概率并各自附加条件；它的"分支"不是数据里的 `if`，而是 C# 类型。

### 3.5 链式：三种衔接语义

`Chains.cs` 提供三个扩展方法，全库用到的次数：

| 方法 | 次数 | 语义 |
| --- | --- | --- |
| `.OnSuccess(rule)` | 100 | 父规则成功才走 |
| `.OnFailedRoll(rule)` | 11 | 父规则骰子没过才走（保底） |
| `.OnFailedConditions(rule)` | 1 | 父规则条件没过才走 |

链是**多叉**的（`ChainedRules` 是 `List`），且**返回的是链尾**——所以可以 `.OnFailedRoll(a).OnFailedRoll(b)` 串成降级阶梯（例如 `ItemDropDatabase.cs:563` 的骷髅王头：7 分之 1 头 → 7 分之 1 面具 → 7 分之 1 兜底）。

### 3.6 条件：70 个类，分四种性质

`Conditions.cs` 1608 行、70 个条件类。按"可数据化程度"分：

| 性质 | 例子 | 能否做成数据 |
| --- | --- | --- |
| **全局常量查询** | `IsHardmode`、`IsExpert`、`IsMasterMode`、`IsChristmas`、`RemixSeed`、`SkyblockIsUp` | 能——单一布尔 |
| **区域查询** | `JungleKeyCondition`、`SoulOfLight`、`DesertKeyCondition` | 能——区域枚举 |
| **世界进度查询** | `DownedAllMechBosses`、`BeatAnyMechBoss`、`FirstTimeKillingPlantera` | 能——进度标志 |
| **带随机** | `FrostMoonDropGatingChance`（`RollLuck(num2)==0`）、`PumpkinMoonDropGateForTrophies`（`rng.Next(num)==0`） | **不能**——条件本身掷骰 |
| **带个体状态** | `IsUsingSpecificAIValues`（读 `npc.ai[slot]`）、`EmpressOfLightIsGenuinelyEnraged`、`MissingTwin`（`AnyNPCs`）、`MechdusaKill` | **不能**——读运行时实体状态 |
| **带位置几何** | `PirateMap`（`position.Y < worldSurface+10` 且 `X<380 || X>maxTilesX-380`）、`LivingFlames`（`Y > UnderworldLayer`） | 部分——阈值可数据化，`AnyNPCs` 类不行 |

**这是"条件即代码"的本质**：`IItemDropRuleCondition` 只有 `CanDrop(info)` 一个方法，`info` 带着 `NPC` 引用与 `Player` 引用，条件可以读任意东西。

### 3.7 概率与运气

`Luck.cs`（`Terraria.GameContent/Luck.cs`）60 行，全局总闸：

```csharp
public static int RollLuck(float luck, int range) {
    if (luck > 0f && Main.rand.NextFloat() < luck)
        return Main.rand.Next(Main.rand.Next(range / 2, range));   // 分母压到一半~全量
    if (luck < 0f && Main.rand.NextFloat() < -luck)
        return Main.rand.Next(Main.rand.Next(range, range * 2));   // 分母翻倍
    return Main.rand.Next(range);
}
```

规则体系为此**特意分了三套基类**：

- `CommonDrop` → `info.player.RollLuck(...)`：**吃运气**
- `CommonDropNotScalingWithLuck` → `info.rng.Next(...)`：**不吃运气**
- `CommonDropScalingWithOnlyBadLuck` → `RollOnlyBadLuck`：**只吃坏运气**

写规则时选错基类，运气加成就失效或超模。这是三个几乎同名、行为却不同的类，属于最容易误用的点。

金币是**第四套**逻辑（`NPC.cs:80316`）：`num2 = 1; if (rand < |luck|) num2 = 2;`，然后正运气取两次最大值、负运气取两次最小值。**不走 `Luck.cs`**。

### 3.8 图鉴概率：与运行时平行的第二套实现

`IItemDropRule` 除 `TryDroppingItem` 外还有 `ReportDroprates`。两套实现必须语义一致：

- `BestiaryDatabase.Merge()` 遍历 `-65..NPCID.Count` 逐个调 `ReportDroprates`，且 `includeGlobalDrops: false`——**全局规则不进图鉴**。
- 链式概率靠 `DropRateInfoChainFeed.With(multiplier)` 逐层相乘传递。
- `LeadingConditionRule.ReportDroprates` 是 `With(1f)` + 追加条件，概率不变。
- `DropBasedOnExpertMode.ReportDroprates` 会**输出两套条目**（专家 + 非专家），靠 `DropRateInfo.conditions` 区分。

只要一侧写错，就会出现"实机掉得对但图鉴显示错"或反之。

### 3.9 负 netID：变体怪

`ItemDropDatabase` 遍历 `-65..-1`，这些是**同一 `type` 的属性变体**（`NPC.cs:7686 SetDefaultsFromNetId` 里每个 case 改 `damage/defense/life/value` 等）。

- `RegisterToNPC(type>0)` 会**同时**注册到 `type` 与所有对应负 netID（`ItemDropDatabase.cs:61-72`）。
- `TrimDuplicateRulesForNegativeIDs()` 对负 netID 做 `Distinct()` 去重——用的是**引用相等**（`IItemDropRule` 未重写 `Equals`），依赖"同实例被重复注册"这一前提。
- 例：`RegisterToNPC(-4, ...)` 单独注册；`npcNetIds13 = {-6,-7,-8,-9,-4}` 被 `RemoveFromMultipleNPCs` 排除。

这套机制意味着"一个 type 可以有 65 套不同掉落"，§3.9 的 `type//netID` 双键在数据模型里必须保留。

### 3.10 雕像

`NPCLoot()` 在跑规则树之前做了两道过滤（`NPC.cs:79542`）：

```csharp
if ((SpawnedFromStatue && NPCID.Sets.NoEarlymodeLootWhenSpawnedFromStatue[type] && !Main.hardMode)
 || (SpawnedFromStatue && NPCID.Sets.StatueSpawnedDropRarity[type] != -1f
     && (Main.rand.NextFloat() >= NPCID.Sets.StatueSpawnedDropRarity[type] || !AnyInteractions())))
    return;
```

也就是说：**雕像刷的怪，先掷一次"是否给战利品"的骰子，没过就整棵树都不跑**（金币/红心/药水也都不掉）。`StatueSpawnedDropRarity` 是 `NPCID.cs:4799` 的一张浮点表（默认 -1f，27 个条目）。

---

## 4. 载体②：硬编码即时掉落

### 4.1 金币（`NPCLoot_DropMoney`，130 行）

流程：运气决定掷 1 次还是 2 次 → 每次对 `value` 叠乘随机档位 → 取最大/最小 → 加 `extraValue` → 按 1 华丽/1 金/1 银/1 铜**逐位拆成实际掉落的物品**（71/72/73/74），每拆一步还有 1/5 概率削减数量。

**这段逻辑不可数据化**——它是"面额换算 + 概率磨圆"，是算法不是表。

### 4.2 红心与蓝星（`NPCLoot_DropHeals`，115 个 `case`）

`NPCLoot_DropCommonLifeAndMana` 给所有怪一个通用 1/6 红心-蓝星逻辑，然后 `switch(type)` 覆盖特例（霜月怪 1/4 心、南瓜王 100% 心、雪人军团掉 6~11 颗等）。

### 4.3 Boss 药水与心（`DoDeathEvents_DropBossPotionsAndHearts`）

**任何 `boss == true` 的怪**都掉：5~15 瓶治疗药水（按 type 选 28/188/499/1134/3544）+ 5~10 颗心。仍在 `DoDeathEvents` 末尾由 `if (boss)` 触发。

### 4.4 事件与特殊掉落

`DoDeathEvents` 的 `switch(type)` 里还有若干直接 `Item.NewItem(..., 3822)`（DD2 水晶）之类的掉落，以及大量**非掉落**副作用（设 `downed*` 标志、触发月之末日、开困难模式、播报）。

**边界**：这段代码里"掉落"与"世界进度变更"混在同一个 `switch`。抽掉落时必须把副作用留下。

---

## 5. 载体③：Boss 袋

`ItemDropDatabase` 里 `ItemDropRule.BossBag(itemId)` 只是**在专家模式掉出一个袋子物品**（17 处）：

```csharp
BossBag(id) => new DropBasedOnExpertMode(
    DropNothing(),
    new DropLocalPerClientAndResetsNPCMoneyTo0(id, 1, 1, 1, null));
```

袋子里装什么，在 `Player.cs:7206 OpenBossBag(int type)` 的巨型 `switch` 里，全部是 `Main.rand.Next(...)` + `QuickSpawnItem`。

**这是一条独立的掉落路径**：它和规则树没有任何复用，且**只在玩家使用物品时执行**（`Player.cs` 共 399 处 `QuickSpawnItem`）。任何"专家模式掉什么"的完整答案都必须同时读 `ItemDropDatabase` 和 `OpenBossBag`。

---

## 6. 载体④：钓鱼

完全独立的第二套系统，与 `ItemDropRules` **零复用**：

- `FishDropRuleList.TryGetItemDropType()` **按注册顺序逐条试，首条命中即返回**——顺序即优先级。
- `FishDropRule.IsStopper`：`PossibleItems` 为空，或 `HackedIsAny` 且分子==分母 时为"到此为止"的兜底（`AddStopper(AnyEnemies)`、`AddStopper(ocean)`）。
- 条件用 `AFishingCondition`（区域/高度/难度/水格数），稀有度用 `FishRarityCondition`（由 `FishingCheck_RollDropLevels` 按钓力算出六个布尔）。
- `Projectile.cs:21033 FishingCheck_RollItemDropOld` **是死代码**，新路径 `FishingCheck_RollItemDrop`（`Projectile.cs:20960`）走 `Main.FishDropsDB`。

钓鱼的稀有度 `legendary/veryrare/rare/…` 是**每次抛竿重算的布尔集合**（`150*30/fishingLevel` 等），不是固定概率——"某鱼多少概率"无法从代码里静态读出，只能仿真。

---

## 7. 可疑点清单

以下为静态阅读发现，按影响面排序。**复核状态**：均已读源码确认，但未运行验证。

| # | 位置 | 现象 | 判断 |
| --- | --- | --- | --- |
| 1 | `ItemDropDatabase.cs:1193-1207` | `Conditions.NeverTrue` + `OneFromOptions(8, new int[0])`，空选项数组 | **潜在崩溃**：`NeverTrue` 若被改成真条件，`dropIds[rng.Next(0)]` 立即越界。看着像被禁用的事件掉落（4367-4371/4343/4344/4612/4674） |
| 2 | `ItemDropDatabase.cs:869` | `RegisterBloodMoonFishingEnemies()` 空方法，`Populate()` 里仍调用 | 残留；对应内容已在 `RegisterBloodMoonFishing()` |
| 3 | `ItemDropDatabase.cs:92 / :101` | `RegisterToMultipleNPCsNotRemixSeed` / `…RemixSeed` 全工程零调用 | 被 `new LeadingConditionRule(...).OnSuccess(...)` 手写替代 |
| 4 | `Conditions.cs:1202 YoyosKraken` | 定义了条件类，但 `RegisterYoyos()` 只注册 4 把（3282/3289/3286/3290），无 Kraken | 未接入 |
| 5 | `ItemDropDatabase.cs:15` | `_masterModeDropRng = 4` 魔法数字 | 所有大师模式宠物/坐骑共用 1/4 |
| 6 | `ItemDropDatabase.cs:30 TrimDuplicateRulesForNegativeIDs` | 用引用相等做 `Distinct()` | 依赖"同实例重复注册"前提，脆弱 |
| 7 | `NPC.cs:79530` | `if (0 == 0)` | 反编译残留 |
| 8 | `CommonDrop` 三兄弟 | 名字近似、行为三分 | 最高误用风险 |
| 9 | `ReportDroprates` 与 `TryDroppingItem` | 每类都要手写两套，且必须语义一致 | 最大维护负担 |
| 10 | `ItemDropResolver` | 嵌套规则的单参 `TryDroppingItem` 恒返 `DidNotRunCode` | 外部误调会静默不掉 |

---

## 8. 对 NLTX 设计的直接影响

### 8.1 掉落不是一张表，是"树 + 算法 + 脚本"三类

| 类别 | 载体 | 可数据化 | 建议 |
| --- | --- | --- | --- |
| **树** | `ItemDropDatabase` 的 425 次注册 | 结构可数据化（树形），**条件与嵌套分支不可** | 需要独立的"规则树"内容族，节点类型 = 参考端 15 个规则类 |
| **算法** | 金币拆分、`DropOneByOne` 的玩家数加成、`Luck.RollLuck` | **不可** | 保留为服务端代码，内容侧只给参数 |
| **脚本** | `DoDeathEvents` 的 `switch`、`OpenBossBag` | **不可**，且与副作用混杂 | 事件掉落与进度变更必须留在代码里，不能进内容族 |

### 8.2 三处必须同时改才算"改了掉落"

散在五处 ⇒ **改一个 NPC 的掉落，可能要动 `ItemDropDatabase`、`NPCLoot_DropHeals`、`OpenBossBag` 三处**。任何"掉落内容族"的验收口径都必须明确覆盖范围，否则会出现"数据改了但 Boss 袋没变"这类缺陷。

### 8.3 条件是最难的一层

`IItemDropRuleCondition` 拿到 `DropAttemptInfo`（含 `NPC` 与 `Player` 引用）后可以读任意状态。70 个条件类里：

- 约一半是**纯查询**（全局常量 / 区域 / 进度），可以直接变成数据上的谓词；
- 约四分之一**自带随机**（波次门控），是"条件即概率"，数据模型里必须允许条件携带分母；
- 剩下**读个体状态**（`ai[]`、`AnyNPCs`、位置几何），只能是代码。

### 8.4 概率有两个读者，必须同时满足

`ReportDroprates`（图鉴）与 `TryDroppingItem`（实机）是两套独立实现。若 NLTX 要让掉落数据化，**只有一套可执行的描述是不够的**：要么让同一份描述同时生成两者，要么接受图鉴概率另行计算。参考端的做法是手写两遍，这是它最大的维护负担。

### 8.5 现有 NLTX 侧的落位参考

当前 `src/NSSLC` 已具备：

| 能力 | 位置 | 说明 |
| --- | --- | --- |
| 世界掉落物生成 | `Share/Item/System/ItemStore.cs:63/87 TryCreate` | 唯一的世界物品生成入口，入队 `ItemMutation(ItemChangeKind.Spawn)` |
| 物品栈身份 | `Share/Item/ItemStack.cs` | `ItemId + quantity + prefix` |
| 图格掉落 | `World/System/WorldTileDropRules.cs` | 单 tile → item 的 `switch` 表达式，**形状上最接近"可数据化"**，可作为规则族格式的第一个试验品 |
| 变更投影 | `WorldNetworkProjectionSystem.cs:113` | `DropItemType/Stack` 只表达意图，实体由 World 域在 Commit 阶段生成 |

**缺口**：目前 NLTX 没有 NPC 死亡 → 掉落 的链路，也没有任何掉落规则族。`WorldTileDropRules` 是唯一的掉落表，且只覆盖单格破坏。

---

## 9. 未闭合

1. **"掉落"的边界未定**。是只做 `ItemDropRules` 那一棵树，还是连金币/红心/Boss 袋一起？本文件只给证据，不给结论。
2. **`ReportDroprates` 的替代方案未定**。NLTX 若不做图鉴则无此负担；若要做，需要先决定"单描述双消费"还是"双描述"。
3. **负 netID 的建模未定**。参考端是 `type` 与 `netID` 双键 + 变体属性表；NLTX 的实体身份是否需要同样表达，取决于是否要支持属性变体怪。
4. **雕像与交互判定未定**。`SpawnedFromStatue` 与 `GetWereThereAnyInteractions()` 会整体短路掉落，属于触发层而非规则层，落在哪一侧需要先定。
5. **钓鱼系统未评估**。本文件只做了结构描述；它与 `ItemDropRules` 零复用，是否共用一个内容族格式未定。
6. **未统计嵌套深度**。425 次注册里多少层 `LeadingConditionRule.OnSuccess(...)` 未做统计，规则树的实际深度不明。
