# WorldTileDropRules 能否做成组件

```text
文档 ID：DOC-2026-11-01-worldtiledroprules-component-design-question
逻辑域：reviews
产物类型：design
状态：active
范围：src/NSSLC/Components/World/WorldTileDropRules.cs
      src/MinimalImplementation/World/WorldTileDropRules.cs
证据入口：src/NSSLC/Components/World/WorldRuntime.cs:222
      src/MinimalImplementation/Host/Version4InboundRouter.Terrain.cs:66
      D:/TRbackup/最新的客户端/Terraria/WorldGen.cs:64292
canonical 路径：docs/reviews/audits/2026-11-01-worldtiledroprules-component-design-question.md
```

## 1. 问题的准确形式

"`WorldTileDropRules` 能否做成组件"需要先拆成两问，否则会得到一个技术上可行、
语义上错误的答案：

1. **能不能写成一个带 `Component` 后缀的类型？** —— 能。加个空结构体就编译通过。
2. **它是否满足本仓库的组件判定？** —— 不满足。这一问才是决定性的。

本文结论：**不应做成组件**。但它确实存在该改的问题，只是那个问题不在组件轴上。

## 2. 现状实证

仓库里有两个同名实现，都是 `static class`，都无实例状态。

| 项 | NSSLC 版 | MinimalImplementation 版 |
| --- | --- | --- |
| 文件 | `src/NSSLC/Components/World/WorldTileDropRules.cs` | `src/MinimalImplementation/World/WorldTileDropRules.cs` |
| 命名空间 | `Terraria.World` | `MinimalImplementation.Server.World` |
| 入参 | `WorldTileState`（16 字段 record struct） | `WorldTile` |
| 出参 | `out int itemType` | `out int itemType` |
| 明面条目 | 128 | 127 |
| `FrameX` 条件分支 | 3 | 2 |
| 无匹配哨兵 | `0` | `-1` |
| 成功判定 | `itemType > 0` | `itemType >= 0` |
| 调用方 | `WorldRuntime.cs:222` | `Version4InboundRouter.Terrain.cs:66` |

两者只被各自的破块路径调用一次。没有第三个消费者。

## 3. 两表已经分叉

逐条解析两份 switch 后比对，**10 个 tile id 行为不同**，另有 1 个帧条件不同：

| tile | NSSLC | Minimal | 说明 |
| --- | --- | --- | --- |
| 0 | 缺（视为无掉落） | `2` | 参见 §3.1 |
| 3 | 缺（仅帧条件命中） | `0` | NSSLC 无条件分支，Minimal 显式无掉落 |
| 5 | `9` | 缺（未知） | |
| 71 | `5` | 缺（未知） | |
| 73 | 缺（未知） | `0` | |
| 110 | 缺（未知） | `0` | |
| 596 | `9` | 缺（未知） | |
| 616 | `9` | 缺（未知） | |
| 634 | `5215` | 缺（未知） | |
| `73 / FrameX==144` | `5` | 缺（落回 `73 => 0`） | 帧条件集合不同 |

这不是笔误级别的差异：同一块 tile 在两个实现里可能一个有掉落、一个没有。

### 3.1 语义塌陷：三种结果被压成两种

`MinimalImplementation` 版区分三种结果，`NSSLC` 版把它们压成两种：

```text
命中且 itemType > 0   → 掉落该物品          （两版一致）
命中但 itemType == 0  → 已知这一块不掉落     （Minimal 可说，NSSLC 说不出）
无任何匹配            → 规则未覆盖该 tile    （Minimal 用 -1 表达，NSSLC 折叠进 0）
```

`NSSLC` 版的 `_ => 0` 与 `itemType > 0` 让"已知无掉落"和"规则缺失"无法区分。
当规则表只是参考实现的子集时，这个区分恰好是最需要的信号——它告诉你哪些 tile 还没覆盖。

### 3.2 tile 0 究竟是空气还是泥土

两版对 tile id `0` 的解释相互矛盾，而这一点在本仓库内部也不是自明的：

- `WorldTileState.Empty => new(false, 0, 0, 0, 0)`（`WorldTileState.cs`）
- `next = current with { Active = false, Type = 0, FrameX = 0, FrameY = 0 }`（`WorldRuntime.cs:220`）
- `if (tileType == 0)` 被当作非法放置（`WorldRuntime.cs:236`）

本仓库用 `Type = 0` 表示**空气**。而 `MinimalImplementation` 的规则表里 `0 => 2`，
按参考客户端的编号 `tile 0` 是**泥土**。两者不可能同时成立。

这不是本文要解决的 bug，但它决定了 `NSSLC` 版"缺 tile 0 条目"是有意省略还是遗漏。
**在这一点定论前，不应把差异中的任何一侧称为错误。**

## 4. 组件判定：逐条对照仓库约束

依据 `docs/architecture/ECSEntity组织设计约束.md` 与
`Context/架构约束/组件命名设计约束.md`。

| 约束 | 原文 | 对照结果 |
| --- | --- | --- |
| 组件的定义 | :62「Component \| 某一实体实例的能力、状态或关系」 | `WorldTileDropRules` 是 `tile → itemType` 的纯映射，不属于任何实体实例 |
| 批量数据不实体化 | :49–50「瓦片值、槽位、采样记录等批量数据可以留在领域组件或专用存储内，不强制逐项实体化」 | 直接点名瓦片值这一类数据 |
| 不可变定义可共享 | :101「不可变定义可以共享，实例状态必须独立」 | 现状 `static` 已是共享的正确形态；改成组件反而引入"每实体一份"的错觉 |
| 组件的提问方式 | 命名约束 :407「先写出组件回答的问题：实体拥有什么能力/状态，或在什么事件上触发什么效果？」 | 答不出。它回答的是"这块瓦片产出什么"，主语是瓦片不是实体 |
| 组件必须有可变状态归属 | 命名约束 :98–100 拒绝无语义收容词 | 若造 `TileDropRuleComponent`，其内容是不可变查表，属于"数据容器伪装成组件" |

**四条独立约束指向同一结论：它不是组件。** 它是一张规则表，属于 :63 定义的
Definition 类别，而不是 Component 类别。

## 5. "世界级单例组件"这个变体也不成立

一个常见折中是把规则表挂到"世界实体"上做成单例组件。本仓库没有这个挂载点：

| 证据 | 位置 |
| --- | --- |
| `WorldRuntime` 是普通类，不是组件 | `WorldRuntime.cs:19` `public sealed class WorldRuntime :` |
| 在入口手工 `new` 出来 | `Program.cs:35` `WorldRuntime worldRuntime = new(worldWidth, worldHeight);` |
| 通过构造函数注入系统 | `WorldTickSystem.cs:20` `WorldTickSystem(ArchWorld ecsWorld, WorldRuntime world)` |

世界状态不驻留 ECS，也没有代表世界的实体。要做世界级组件，必须先发明一个世界实体，
这是远超本问题的架构变更。所以这个折中在当前结构下不可落地。

## 6. 参考客户端揭示的真问题

参考实现的真实签名（`WorldGen.cs:64292`）：

```csharp
public static void KillTile_GetItemDrops(
    int x, int y, Tile tileCache,
    out int dropItem, out int dropItemStack,
    out int secondaryItem, out int secondaryItemStack,
    out bool noPrefix,
    bool includeLargeObjectDrops = false)
```

两个调用点：`WorldGen.cs:63547`、`WorldGen.cs:64278`。

展开成维度对比：

| 维度 | 参考实现 | 本仓库现状 |
| --- | --- | --- |
| 位置 | `x`, `y` | **丢失** |
| 瓦片输入 | 完整 `Tile` | `WorldTileState`（够用） |
| 主产物 | `dropItem` | `itemType` |
| 主产物数量 | `dropItemStack` | **丢失** |
| 副产物 | `secondaryItem` | **丢失** |
| 副产物数量 | `secondaryItemStack` | **丢失** |
| 前缀资格 | `noPrefix` | **丢失** |
| 大型物体模式 | `includeLargeObjectDrops` | **丢失** |

`bool` 返回值加单 `int` 出参，是对一个 **4 出参 + 2 上下文入参** 的函数的损失投影。

这才是本文件真正该讨论的设计问题：**如何承载一个还在长输入、长输出的纯函数。**
把它做成组件不仅不解决这个问题，还会让签名更难扩展——组件字段是持久状态，
而这里需要的是调用期上下文。

## 7. 四种形态评估

| 形态 | 是否符合仓库约束 | 能否承载 §6 的多维签名 | 判定 |
| --- | --- | --- | --- |
| A. 静态纯函数（现状） | 是 | 可以，但 `out` 参数会继续膨胀 | 保留，扩展签名 |
| B. 定义表类 + 结构化结果 | 是（Definition 类别） | 是，结果用 record struct | **推荐** |
| C. 世界级单例组件 | 否，无挂载点（§5） | 否 | 排除 |
| D. 每瓦片实体组件 | 否，违反 :49–50 | 否，且瓦片数量级不可行 | 排除 |

推荐形态 B 的具体形状（**未实施，仅设计**）：

```text
TileDropRuleSet            不可变查表，按 (type, frameX) 索引
TileDropContext            调用期上下文：坐标、大物体模式
TileDropResult             结构化结果：主产物+数量、副产物+数量、无前缀
TileDropRuleSet.TryResolve(context, out result) -> bool
```

三条要点：

1. **单一权威实现。** 现在两份表 10 处不一致，根源是同一规则被抄了两遍。
   形态 B 要求只有一份表，另一个项目引用它。这是本次分析里收益最确定的一条。
2. **区分"无掉落"与"未覆盖"。** 三态结果（掉落 / 已知无掉落 / 规则未覆盖）要显式表达，
   不要用 `0` 同时承担两种含义（§3.1）。
3. **坐标进上下文但不进表。** 位置相关规则（生物群系等）属于解析阶段，不是查表阶段；
   查表只按瓦片身份，符合 §4 的 Definition 定位。

## 8. 待决策项

| 项 | 待定内容 | 影响 |
| --- | --- | --- |
| tile 0 语义 | 空气还是泥土（§3.2） | 决定两份表的差异里哪些是遗漏 |
| 谁是权威表 | NSSLC 版还是 Minimal 版 | 合并前必须定，否则合并即引入行为变更 |
| 是否现在扩签名 | 补 `x/y`、堆叠、副产物、`noPrefix` | 扩签名改变 `WorldRuntime.cs:222` 的记录内容 |
| 两份实现是否合并 | 合并需跨项目引用 | `MinimalImplementation` 目前不依赖 NSSLC 共享层 |

## 9. 未验证事项

- 未逐条核对两份表的 128 个条目是否都符合参考客户端；只验证了两者**相互**差异。
- 未确认参考客户端 `KillTile_GetItemDrops` 中 `x/y` 具体影响哪些分支。
- 未确认 `tile 634 → 5215` 等 NSSLC 独有条目是否有参考依据。
- `WorldTile`（MinimalImplementation）与 `WorldTileState`（NSSLC）字段差异未逐一比对。
- 本文为设计分析，未修改任何源码，未执行构建。

## 10. 结论

`WorldTileDropRules` **不应做成组件**，四条独立仓库约束一致支持这个判定，且仓库没有
可供挂载的世界实体。

它该改的是另外三件事，按收益排序：

1. **消除双份表**（10 处已分叉，且无任何机制会阻止继续分叉）；
2. **恢复三态结果**，别再让"已知无掉落"和"规则未覆盖"共用 `0`；
3. **扩签名**，现在的单 `int` 出参丢了堆叠、副产物和前缀资格。

"做成组件"解决不了其中任何一条——组件是持久状态的载体，而这里需要的是规则表
和调用期上下文。
