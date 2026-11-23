# 参考客户端元数据普查：可疑点清单与分级

文档 ID：DOC-2026-11-23-CLIENT-METADATA-SURVEY  
逻辑域：research  
产物类型：evidence  
状态：proposal；静态扫描，未运行参考客户端  
日期：2026-11-23（Asia/Shanghai）  
参考客户端：`D:\TRbackup\最新的客户端`（Terraria 1.4.5.8 反编译快照，1579 个 `.cs` 文件）

## 1. 扫描方法与覆盖面

对全部 1579 个 `.cs` 文件逐一读入全文，用 8 组正则检测"元数据可能的载体形态"。**每个文件都扫，不是抽样。**

| 检测器 | 正则（简化） | 命中总数 | 含义 |
| --- | --- | --- | --- |
| SetFactory | `Factory\.Create(Bool\|Int\|Custom\|Float\|Ushort)Set` | 530 | 按 id 下标的稀疏数组 |
| case 分支 | `(?m)^\s*case -?\d+:` | 22545 | switch 表 |
| 按 id 比较 | `\b(type\|Type\|npcType\|...)\s*==\s*\d+` | 16297 | 逐 id 分支 |
| 注册调用 | `\bRegister(To)?\w*\(\s*-?\d+` | 390 | 数值 id 注册 |
| 身份常量 | `public const (short\|int\|byte\|ushort) \w+ = -?\d+;` | 14132 | id 常量 |
| 静态数组 | `static readonly \w+\[\]\s+\w+\s*=\s*(new\|\[\|\{)` | 40 | 硬编码数组表 |
| int 字典 | `Dictionary<[^>]*int[^>]*>\s+\w+\s*=\s*new` | 74（51 个文件） | 字典查找表 |
| 字符串常量 | `public const string \w+ = "` | ≥5 才计入 | 名称表 |

原始结果：`.agent-workplace/metadata-scan.csv`（1579 行）。

**扫描的局限**：正则只能发现**形态**，无法判断语义。本文件的每一类都做了抽样复核，复核结论在各节标注。

## 2. 分级清单

### P0：服务端现在就需要

| 元数据 | 位置 | 规模 | 形态 | 复核 |
| --- | --- | --- | --- | --- |
| 物品标量 | `Item.cs` `SetDefaults1..5` | 3511 个物品有赋值 | `switch(type)` + 字段赋值 | 已抽样，见 §3.1 |
| 物品属性贡献 | 同上（`defense` 273、`damage` 669） | — | 同上 | 未单独提取 |
| NPC 标量 | `NPC.cs` `SetDefaults`（8459 行起） | `damage` 658、`defense` 654、`lifeMax` 641、`knockBackResist` 559、`value` 432 | 同上 | **部分区间**，未全量 |
| NPC 掉落 | `ItemDropDatabase.cs` | 429 处 `RegisterToNPC`，40 个 `Register*` 方法 | `RegisterToNPC(id, ItemDropRule.X(...))` | 已抽样 |
| 物品使用 | 同上 `SetDefaults` | `useTime` 3502、`channel` 1113、`useAmmo` 1821 | 同上 | 已提取 |

### P1：有明确消费场景，但系统尚未建立

| 元数据 | 位置 | 规模 | 形态 | 复核 |
| --- | --- | --- | --- | --- |
| 旗帜击杀门槛 | `Terraria.GameContent/BannerSystem.cs` | 447 个 `case` | `case 102: return 1;` → `id → int` | **已抽样确认**，纯映射无副作用 |
| NPC 特殊刷怪规则 | `Terraria.ID/NPCID.cs:206` `SpecialSpawningRules` | 6 条 | `Dictionary<int,int>` 初始化器 `{259,0}` | 已确认 |
| 物品祭品表 | `...Sacrifices.tsv` | 121 KB，以 `ItemID` 字符串为键 | **TSV，已是数据** | 已确认格式 |
| 前缀适用表 | `Terraria.ID/PrefixLegacy.cs` | 7 处 `CreateBoolSet` | 与 `ItemID.Sets` 不同的第二套前缀表 | 已抽样 |
| 射弹按 id 覆盖 | `Projectile.cs` | 4916 处 `type == N` | 混合参数赋值与 AI 状态 | 已抽样，见 §3.2 |
| **合成配方表** | `Terraria/Recipe.cs` | **2764 个 `AddRecipe()`** | `currentRecipe.createItem.SetDefaults(id)` + `requiredItem[n].SetDefaults(id)` + `RequireGroup(...)` + `AddRecipe()` | **已抽样确认**，见 §3.5 |

### P2：依赖尚未存在的服务端系统

| 元数据 | 位置 | 规模 | 说明 |
| --- | --- | --- | --- |
| 光照衰减系数 | `Terraria.Graphics.Light/TileLightScanner.cs` | 534 个 `case` | `case 357: num = 0.15f;` → tile id → 亮度。**纯客户端绘制** |
| 地图颜色表 | `Terraria.Map/MapHelper.cs` | 106 KB，约 987 对元组 | `array[624][0] = new Color(210, 91, 77);` → tile id → 地图颜色。**纯客户端小地图** |
| 房屋黑/白名单 | `Terraria.GameContent/HouseUtils.cs` | 2 处 | `BlacklistedTiles` / `BeelistedTiles`，依赖 NPC 住房系统 |
| 血腥/尘埃过滤 | `Terraria/ChildSafety.cs` | 2 处 | `SafeGore` / `SafeDust`，纯客户端 |
| 城镇 NPC 头像 | `Terraria.GameContent/TownNPCProfiles.cs` | 4 个 int 数组 + 1 字典 | 头像帧索引，纯客户端 |
| 标记 tile 类型 | `Terraria.ID/TileID.cs` | 161 处 | 大部分是绘制/碰撞分类，需逐项判定 |

### P3：确认不做

| 内容 | 位置 | 理由 |
| --- | --- | --- |
| 世界生成算法 | `WorldGen.cs`（5038 个 case、2778 处 `type ==`） | 抽样确认是**地形算法分支**（如 `tile.type == 225` 判断某块地砖），不是 id→参数表 |
| 输入按键名 | `Terraria.GameInput/TriggerNames.cs`（60 个 const string） | 输入绑定，非内容 |
| 资源包图片尺寸 | `ResourcePacksDefaultInfo.tsv`（383 KB） | 客户端资源，服务端不需要 |
| 本地化文本 | `Terraria.Localization.Content.*.json`（共 80+ 文件） | 文本，非数值元数据 |
| UI 布局 | `ItemSlot.cs`、`UICreativeInfiniteItemsDisplay.cs` | 纯客户端 |

## 3. 抽样复核记录

### 3.1 `Item.SetDefaults` 的字段分布

`Item.SetDefaults1..5` 区间内，数字字面量赋值共 23291 处，分布在**59 个不同字段**：

| 字段 | 赋值数 | 字段 | 赋值数 |
| --- | --- | --- | --- |
| `width` | 3874 | `defense` | 273 |
| `height` | 3874 | `headSlot` | 268 |
| `useAnimation` | 2008 | `scale` | 215 |
| `useTime` | 2003 | `bodySlot` | 199 |
| `useStyle` | 1964 | `legSlot` | 167 |
| `rare` | 1700 | `createWall` | 132 |
| `value` | 1502 | `buffType` | 115 |
| `createTile` | 1095 | `mana` | 81 |
| `placeStyle` | 784 | `glowMask` | 72 |
| `damage` | 669 | `buffTime` | 51 |
| `knockBack` | 556 | `wingSlot` | 47 |
| `shoot` | 546 | `holdStyle` | 41 |
| `shootSpeed` | 476 | 其余 34 个字段 | ≤41 各 |

**关键判断**：`width`/`height`/`scale`/`alpha`/`glowMask`/`stringColor`/`holdStyle`/`useStyle`/`useSoundPitch` 等是客户端绘制参数。服务端实际读取的仅 6 个成员（见 §4）。

### 3.2 按 id 分支的两类内容

抽样 `NPC.cs:29179` 与 `Projectile.cs` 的 `type == N` 块，确认混有两类：

**可外置的参数赋值**：
```csharp
if (type == 145) { num586 = 3.5f; num587 = 0.8f; }
if (type == 1106) { num6 = 15; num7 = 15; num8 *= 0.5f; }
```

**不可外置的 AI 状态操作**：
```csharp
if (type == 143) { ai[2]++; if (confused) ai[2] = 0f; if (ai[2] >= 120f) ... }
if (type == 154) { position += netOffset; Dust.NewDust(...); }
if (type == 417) { position.Y += height; height = 32; ai[0] = 6f; }
```

第二类操作 `ai[]`、`position`、`Dust`、`netUpdate`，是 tick 逻辑而非静态数据。**机器无法区分这两类，必须人工标注。**

### 3.3 `WorldGen.cs` 为什么不算元数据

随机抽 3 处 `type == N`：

```csharp
if (type == 4 && TileID.Sets.Conversion.Torch(theTile, conversionType))  // 转换算法分支
if (Main.tile[i, j].type == 225 && (!Main.tile[i-1, j-1].active() || ...))  // 结构检测
if (tileCache.type == 33 || tileCache.type == 34 || ...)  // 绘制分类
```

都是**当场判断并执行动作**，没有"id → 一组参数"的查表结构。因此 5038 个 case 中绝大多数不是元数据。

### 3.4 `BannerSystem` 是最干净的样本

```csharp
case 102: return 1;
case 250: return 2;
case 257: return 3;
```

纯 `id → int` 映射，无副作用、无嵌套、无外部依赖。**建议作为"从 switch 提取元数据"的第一个试点。**

### 3.5 `Recipe.cs`：2764 个配方的写法

`Recipe.cs` 共 651 KB、2764 处 `AddRecipe()`。每个配方的典型片段：

```csharp
currentRecipe.createItem.SetDefaults(974);      // 产物
currentRecipe.createItem.stack = 3;
currentRecipe.requiredItem[0].SetDefaults(8);   // 材料 1
currentRecipe.requiredItem[0].stack = 3;
currentRecipe.requiredItem[1].SetDefaults(664); // 材料 2
AddRecipe();
```

`RecipeGroups.Wood` 这类前缀还会用 `RequireGroup(...)` 表达"任一木材"的等价组。因此配方的形状是：

```
Recipe { produceItem, produceStack, required[ (item | group) × stack ], requireTile?, conditions? }
```

这是**关系型元数据**（多对多 + 数量 + 分组替代），与标量字段完全不同，需要独立的族。它此前完全不在清单里，属于本次扫描的重大遗漏补充。

## 4. 服务端实际消费面（已 grep 核实）

| 定义 | 被读取的成员 | 证据 |
| --- | --- | --- |
| `ItemDefinition` | `MaxStack`、`Categories`、`AttributeModifiers` | `InventorySystem.cs:97,252,315`、`InventorySlotDefinition.cs:47-49`、`InventoryAttributeContributionSystem.cs:106` |
| `ItemUseDefinition` | `UseTime`、`Channel`、`RequiresAmmo` | `PlayerItemUseSystem.cs:161,183,188,190`、`ItemUseRules.cs:28,35` |
| `ProjectileDefinition` | 8 个组件（无标量字段） | `ProjectileDefinition.cs:12-59` |
| `EntityDefinition` | 组件工厂集合（无数据字段） | `EntityDefinition.cs:11-31` |

`ManaCost` 只在注释中出现（"travels as data only"），无消费者。`UseAnimation`、`AutoReuse` 只被加载与校验。

**这条是分级依据**：P0 对应"有消费者"，P1 对应"系统即将建立"，P2 对应"系统不存在且短期不做"。

## 5. 普查未覆盖的形态

以下形态本次**未**专项扫描，可能仍有遗漏：

1. **`if/else if` 长链**：`else if (type == N)` 形式已包含在 `type ==` 检测器内，但链式结构可能被计入同一统计而未区分层次。
2. **方法参数里的 id**：`RegisterToNPC(620, ...)` 已计入 `reg=390`，但用 `NPCID.Slime` 这种常量名传参的调用未计入——实际掉落注册数是 429 而非 390，差额即此类。
3. **`ItemDropRule` 的嵌套深度**：429 处注册里有多少层 `LeadingConditionRule.OnSuccess(...)` 未统计。
4. **`NPC.SetDefaults` 全量**：仅扫到 40000 行（文件共 96190 行），`lifeMax`/`damage`/`defense` 的计数是**部分区间值**。
5. **代码生成之外的资源**：`.xnb`（1 个）、二进制资源未解析。

## 6. 对元数据设计的直接影响

1. **字段准入必须自动化**：59 个字段里服务端只用 6 个。靠人记住"哪些能进"不可靠，需用 DTO 无该字段 + `UnmappedMemberHandling.Disallow` 结构性挡住。
2. **按 id 分支要人工标注**：`type == N` 共 16297 处（射弹 4916、NPC 3651、WorldGen 2778），其中只有一部分是可外置参数。
3. **`ai[]` 是最大结构障碍**：参考客户端的 `ai[0..3]` 同时承担"入参"与"状态"两种职责，ECS 里必须拆开。未统计其用法分布。
4. **已有外置先例可循**：`Sacrifices.tsv` 证明官方自己也在把代码表搬成数据文件，且用 `ItemID` 字符串作键。
5. **元数据不是一类，是四类**：本次普查得到四类形状，各自需要独立格式与加载器——标量字段（`SetDefaults`）、稀疏映射（`SetFactory` / `BannerSystem`）、关系树（`ItemDropDatabase` 的掉落规则）、多对多（`Recipe.cs` 的配方）。把它们塞进一个 schema 会得到无法维护的万能容器。

## 7. 面积估算

按各类载体涉及的代码量估算工作量级，用于排期而非精确预测：

| 类别 | 涉及文件 | 代码量级 | 可机械化程度 |
| --- | --- | --- | --- |
| 标量字段（Item / NPC / Projectile） | 3 | 约 3 万行赋值 | 中——需人工标注参数 vs 逻辑 |
| 稀疏映射（`SetFactory` 26 文件） | 26 | 530 处调用 | **高**——形状规整 |
| switch 表（Banner / LightScanner） | 2 | 981 个 case | **高**——无副作用 |
| 掉落规则树 | 1 + 37 | 429 处注册 | 低——嵌套需人工 |
| 配方表 | 1 | 2764 个配方 | 中——结构规整但量大 |

**优先级建议**：先做"可机械化程度高"的两类（稀疏映射 + switch 表），用它们验证整条管线（提取 → JSON → 加载 → 冻结 → 验证），再碰标量字段与关系树。
