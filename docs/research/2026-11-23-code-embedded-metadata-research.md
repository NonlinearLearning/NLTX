# 代码内元数据与业界元数据加载技术调研

文档 ID：DOC-2026-11-23-CODE-EMBEDDED-METADATA-RESEARCH  
逻辑域：research  
产物类型：evidence  
状态：proposal；源码阅读与官方文档阅读，未运行参考客户端，未修改本项目源码  
日期：2026-11-23（Asia/Shanghai）  
参考客户端：`D:\TRbackup\最新的客户端`（Terraria 1.4.5.8 反编译快照，`Main.versionNumber` 与程序集版本均为 1.4.5.8）  
配套文档：[外部项目元加载技术调研](2026-11-23-external-meta-loading-research.md)、[内容定义加载系统设计](../architecture/2026-11-23-content-definition-loading-system-design.md)

## 1. 结论

参考客户端把元数据写在代码里，但不是一种写法，而是**五种可区分的形态**，各有不同的复用与迁移代价：

| 形态 | 载体 | 规模证据 | 迁移难度 |
| --- | --- | --- | --- |
| **A. 身份常量** | `public const short/int` 常量类 | `ItemID` 6244 项、`NPCID` 782、`ProjectileID` 1137、`TileID` 754、`WallID` 367、`BuffID` 400、`ArmorIDs` 736 | 低：是纯整数，已由本项目的 `ItemId.g.cs` 复刻 |
| **B. 按 id 下标的稀疏元数据数组** | `SetFactory.CreateBoolSet(default, ids...)` 等 | `ItemID.Sets` 119 个、`TileID.Sets` 161 个、`NPCID.Sets` 69 个、`ArmorIDs` 50 个、`ProjectileID.Sets` 50 个、`WallID.Sets` 27 个、`BuffID.Sets` 12 个、`MountID.Sets` 12 个 | 中：形状规整，可机械导出为表 |
| **C. 逐物品数值分支** | `Item.SetDefaults1..5` 内的 `switch (type)` + 字段赋值 | 6020 个 `case`、23291 处数字字面量赋值 | 高：控制流与数据混写，需人工判定哪些是数据、哪些是逻辑 |
| **D. 内嵌资源 JSON** | 程序集内嵌资源，运行时反序列化 | `TileMaterials` 读 2 个 JSON（3 KB + 20 KB） | 低：已经是数据，只需换加载入口 |
| **E. 枚举即设计分类** | 带显式数值的 `enum` | `ContentSamples.CreativeHelper.ItemGroup` 等 | 低：分类词表，可转成配置键 |

对本项目的直接含义：**能机械迁移的是 A、B、D、E；C 必须人工判定**。因此"加载系统取代 Definition 模型"必须把"数据形状"与"逐物品特例逻辑"分开处理，不能指望一条管线吃掉 `SetDefaults`。

业界主流做法印证了同一分工：本文考察的 Unreal / RimWorld / Minecraft 三者都不鼓励把逐物品数值写进代码分支，但**也都没有把"属性本身的定义"完全外置**——Unreal 的 Gameplay Attribute 与 Attribute Set 必须用原生代码声明，DataTable 只能填初值。这是三条独立官方文档的共同指向，不是对"所有引擎"的断言（Unity 与 Godot 本轮未取到可用正文，见 §2 与 §4.4）。

## 2. 证据边界

| 项 | 事实 |
| --- | --- |
| 参考客户端 | 1579 个 `.cs` 文件，可读；`Item.cs` 981834 字节 / 49964 行 |
| 网络检索 | `web_search` 工具本轮因 API key 无效不可用（HTTP 401）；下文外部资料全部由 `web_fetch` 直接读取官方文档页 |
| 未取到 | RimWorld 的 `abstract`/`parentName` 继承机制未找到一手页面（`Modding_Tutorials/Def_inheritance` 为空页）；Unity ScriptableObject 官方页多次重定向/404，未取到正文；Godot `Resources` 页可取得，但返回内容以大段导航为主，未取到可靠正文，故未引用 |
| 证据等级 | 外部资料仅为"官方文档怎么描述"，不证明这些引擎的具体实现细节或性能特征 |

## 3. 参考客户端的五种代码内元数据形态

### 3.1 形态 A：身份常量

`Terraria.ID/ItemID.cs` 内 6244 个 `public const short` 常量；`NPCID.cs` 782 个，`ProjectileID.cs` 1137 个，`TileID.cs` 754 个，`WallID.cs` 367 个，`BuffID.cs` 400 个，`ArmorIDs.cs` 736 个，`MountID.cs` 67 个，`PrefixID.cs` 97 个。

这是最"无害"的一类：常量只是给整数起名，程序语义完全由常量值决定。本项目已用 `ItemId.g.cs`（6244 项，注释说明由 `Terraria.ID.ItemID` 的 `public const` 生成）复刻，**这一形态不需要新的加载系统**——它是身份层，不是内容层。

值域要注意：`ItemID.Count`（`ItemID.cs:13938`）是 `6196`，等于最大值 `6195 + 1`；6244 个常量里有 48 个是负值（`-48..-1`，即 "Old" 物品）。也就是说 **`Count` 不等于常量个数**，且负 id 落在所有 `SetFactory` 数组的范围之外。任何"按 id 建表"的代码只要用 `Count` 做上界，就会漏掉这 48 个。

另一个副作用：常量类同时承担了"合法 id 全集"的职责，于是下游代码可以假设"任何 id 都有定义"。这正是 `Program.BuildItemDefinitions()` 给全部 6244 个 id 注册宽松定义的原因。

### 3.2 形态 B：按 id 下标的稀疏元数据数组

`ItemID.Sets`（`ItemID.cs:42` 起）是本形态的典型。它的基础设施是 `SetFactory`（`Terraria.ID/SetFactory.cs`），四个构造方法：

```csharp
public bool[] CreateBoolSet(bool defaultState, params int[] types);      // 先全填 defaultState，再把列出的 id 取反
public int[]  CreateIntSet(int defaultState, params int[] inputs);       // inputs 是 (id, value) 成对
public ushort[] CreateUshortSet(ushort defaultState, params ushort[] inputs);
public float[] CreateFloatSet(float defaultState, params float[] inputs);
public T[]    CreateCustomSet<T>(T defaultState, params object[] inputs); // inputs 是 (id, value) 成对
```

`CreateIntSet` 对奇数长度的 `inputs` 直接抛异常（`"You have a bad length for inputs on CreateArraySet"`）——这是这套结构里仅有的两项校验之一，另一项是构造函数里 `size == 0` 时抛 `"size cannot be 0, the intializer for Count must run first"`。除此之外没有越界检查：`CreateBoolSet(false, 7000)` 在长度 6196 的数组上会直接抛 `IndexOutOfRangeException`，`CreateCustomSet` 里甚至还用 `(short)inputs[j]` 强转下标。

数组的语义就是"某件物品是否/有多少某属性"。以下是 `ItemID.Sets` 里可直接读出语义的字段（全部 119 个中的一部分）：

| 字段 | 语义 | 与物品定义的关系 |
| --- | --- | --- |
| `IsAMaterial` | 是否材料 | 分类 |
| `IsFood` | 是否食物 | 分类 + 使用行为 |
| `CanGetPrefixes` | 是否可获前缀 | 前缀规则 |
| `IsCrafted` / `IsCraftedCorruption` / `IsCraftedCrimson` | 是否可制作及其阵营 | 配方归属 |
| `BossBag` | 是否 Boss 袋 | 掉落规则 |
| `IsQuestFish` / `IsFishingCrate` / `IsFishingCrateHardmode` | 钓鱼分类 | 分类 |
| `ShimmerTransformToItem` | 微光转化目标 | 定义引用定义 |
| `BonusMeleeSpeedMultiplier` | 近战速度加成倍率 | **数值修饰** |
| `ToolTipDamageMultiplier` | 提示伤害倍率 | **数值修饰** |
| `KillsToBanner` | 击杀多少解锁旗帜 | 计数规则 |
| `SortingPriority*`（十余个） | 分类排序优先级 | 呈现排序 |
| `Gamepad*`（四个） | 手柄操作范围与快捷行为 | 输入行为 |

两类值得注意：

1. **数值修饰**（`BonusMeleeSpeedMultiplier`、`ToolTipDamageMultiplier`）确实以"按 id 的数组"形式存在，这与本项目要替换的 `ItemAttributeModifierDefinition` 目标一致——说明"物品对属性的贡献"本来就是稀疏表，天然适合外置。
2. **定义引用定义**（`ShimmerTransformToItem`）用 `CreateIntSet(-1, 源id, 目标id, ...)` 表达一对多映射，默认值 `-1` 表示"无"。这是"引用"的最小表达，但失去了类型约束（任何 int 都能写进去）。

规模：`ItemID.Sets` 119 个、`TileID.Sets` 161 个、`NPCID.Sets` 69 个、`ArmorIDs` 50 个、`ProjectileID.Sets` 50 个、`WallID.Sets` 27 个、`BuffID.Sets` 12 个、`MountID.Sets` 12 个。以上计数为"字段声明数"与"`Factory.Create*Set` 调用数"两种口径，`TileID.cs` 因一处调用跨行写作，两种口径相差 1，其余文件一致。

### 3.3 形态 C：逐物品数值分支

`Item.SetDefaults(int Type, ItemVariant variant)`（`Item.cs:48666`）先 `ResetStats(type)` 把上百个字段归零，再按 id 区间分派：

```csharp
if (type == 0) { stack = 0; }
else if (ItemID.Sets.IsFood[type]) { SetFoodDefaults(type); }
else if (type <= 1000)  { SetDefaults1(type); }
else if (type <= 2001)  { SetDefaults2(type); }
else if (type <= 3000)  { SetDefaults3(type); }
else if (type <= 3989)  { SetDefaults4(type); }
else             { SetDefaults5(type); }
```

五个方法内共 6020 个 `case`、23291 处数字字面量字段赋值（统计口径：`SetDefaults1..5` 方法体区间内，行首 3–6 个空白后接 `标识符 = 数字`）。典型片段：

```csharp
defense = 1;      // item 1822
defense = 2;      // 2326
defense = 3;      // 2333
```

这是**最贵的一类**。原因不是量大，而是数据与控制流混在一起：

- 同一个 `case` 里既有 `damage = 20;` 这种纯数据，也有 `if (Main.myPlayer == ...)` 这种逻辑；
- 数值之间有隐含依赖，例如先设 `useTime` 再按 `melee` 调整；
- 部分字段只在特定条件下生效，无法用静态 schema 表达。

因此这一形态**不能靠"自动抽取成 JSON"解决**。可行路径只有两条：按类别（如护甲、武器）人工迁移并保留行为等价快照，或维持代码并只在边界处读取。

### 3.4 形态 D：内嵌资源 JSON

`Terraria.GameContent.Metadata/TileMaterials.cs` 是本形态最清晰的样本（同类的还有 `WorldGen.cs:11029` 用 `WorldGenConfiguration.FromEmbeddedPath("Terraria.GameContent.WorldBuilding.Configuration.json")` 读世界生成配置）：

```csharp
static TileMaterials()
{
    MaterialsByTileId = new TileMaterial[TileID.Count];
    _materialsByName = DeserializeEmbeddedResource<Dictionary<string, TileMaterial>>(
        "Terraria.GameContent.Metadata.MaterialData.Materials.json");
    TileMaterial tileMaterial = _materialsByName["Default"];
    for (int i = 0; i < MaterialsByTileId.Length; i++) { MaterialsByTileId[i] = tileMaterial; }

    foreach (KeyValuePair<string, string> item in DeserializeEmbeddedResource<Dictionary<string, string>>(
        "Terraria.GameContent.Metadata.MaterialData.Tiles.json"))
    {
        SetForTileId((ushort)TileID.Search.GetId(item.Key), _materialsByName[item.Value]);
    }
}

private static T DeserializeEmbeddedResource<T>(string path)
{
    using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
    // ... Newtonsoft.Json 反序列化
}
```

这套写法有四个可借鉴点，同时暴露了两个坑：

**可借鉴**：

1. **两份文件分工**：`Materials.json` 定义"有哪些材质、每种材质什么物理参数"（字典：名字 → 结构）；`Tiles.json` 定义"哪个 tile 用哪种材质"（字典：tile 名 → 材质名）。定义与映射分离。
2. **字符串键 → id 的显式解析**：`TileID.Search.GetId(key)`，找不到时的行为由 `GetId` 决定，调用方不猜。
3. **默认值兜底**：先整表填 `Default` 材质，再按映射覆盖。
4. **嵌入资源**：随程序集分发，不需要运行时文件路径。

**两个坑**：

1. `GetManifestResourceStream` 返回 `null` 时没有检查，`new StreamReader(null)` 会抛 `ArgumentNullException`，堆栈里看不到"缺哪个资源"。
2. 两个资源名是硬编码字符串，重命名文件时编译期不会报错。

这两点与本项目 `PrototypeManager` 的做法形成对比：它先检查目录存在、目录内至少有一个 JSON、每条原型的 `id` 非空且不重复，出错时抛出带文件路径与条目索引的 `InvalidDataException`；字段名的定位发生在投影阶段（`ItemUseDefinitionPrototypeLoader.ParseItemId` 报"目录 + 条目索引 + 字段名"）。即"文件 + 索引"在解析层已有，"字段"在投影层补齐。

### 3.5 形态 E：枚举即分类

`ContentSamples.CreativeHelper.ItemGroup`（`ContentSamples.cs`）把创造的物品分组写成带显式数值的枚举：

```csharp
public enum ItemGroup
{
    Coin = 10, CraftingObjects = 11, Torches = 20, Glowsticks = 25, Wood = 30,
    Bombs = 40, LifePotions = 50, ManaPotions = 51, BuffPotion = 52, Flask = 53,
    Food = 54, Crates = 60, BossBags = 70, GoodieBags = 80, ...
}
```

显式数值说明这些值本身是契约（用于排序或存档），不能随意重排。这类"枚举即分类词表"的东西，迁移到配置时**必须保留数值**，否则等价性无法证明。

## 4. 业界元数据加载技术

### 4.1 Unreal：DataTable / DataRegistry / GAS

**DataTable 与 CurveTable**（[Data Driven Gameplay Elements](https://dev.epicgames.com/documentation/en-us/unreal-engine/data-driven-gameplay-elements-in-unreal-engine)）。官方描述：

- 数据可放在 Excel（`.xlsm`）里维护，导出 `.csv` 后导入引擎；
- 行结构必须是继承自 `FTableRowBase` 的 `USTRUCT`，"程序员必须先创建行容器告诉引擎如何解释数据"；
- 第一列固定名为 `Name`，是行的访问键；
- 导入选项含 `Ignore Extra fields`、`Ignore Missing Fields`、`Import Key Field`——即**未知字段与缺失字段默认会告警**，但可显式关闭。

与参考客户端形态 C 的对比：DataTable 要求的"先声明行结构，再填数据"正是把 `switch` 里的字段赋值抽出来后的样子。官方还明确要求"返回的结构与曲线不要缓存超出函数作用域"，因为重新导入会让指针失效。

**DataRegistry**（[Data Registries](https://dev.epicgames.com/documentation/en-us/unreal-engine/data-registries-in-unreal-engine)）。这是比 DataTable 更贴近本项目需求的一层：

- 定位是"供只读数据使用的全局存储"，支持同步/异步访问与自定义缓存；
- 数据来自 **Data Registry Sources** 与 **Meta Data Registry Sources** 两类；
- **"数据源在列表中的顺序很重要：如果某一项在某个源里没找到，就会去它后面的源里找。这形成了覆盖与回退（override and fallback）的可能，让上下文相关的源覆盖通用源。"**
- 标识分两层：`FDataRegistryType`（哪个注册表）与 `FDataRegistryId`（表内哪一项）；
- 同名注册表只有一个会被识别；同 id 的多项会全部读入，但取用时只返回第一个加载的。

这一条"源顺序 = 覆盖与回退"直接支撑本项目设计里的"基线原型 + 覆盖"策略：不需要发明新语义，业界已有明确先例，且顺序是**显式配置**而不是文件枚举顺序。

**GAS：属性定义必须在代码里**（[Gameplay Attributes and Attribute Sets](https://dev.epicgames.com/documentation/en-us/unreal-engine/gameplay-attributes-and-attribute-sets-for-the-gameplay-ability-system-in-unreal-engine)）：

- "Attributes and Attribute Sets must be created in native code — they cannot be created in Blueprints."
- 属性值分 current 与 base 两份，修饰器作用于 base，current 是计算结果；
- 初值可以用 DataTable 的 `AttributeMetaData` 行类型填，CSV 形如：

  ```
  ---,BaseValue,MinValue,MaxValue,DerivedAttributeInfo,bCanStack
  MyAttributeSet.Health,"100.000000","0.000000","150.000000","","False"
  ```

- 但官方同时说明 `MinValue` / `MaxValue` 这两列**在默认 GAS 插件里没有任何效果**——列存在不等于被消费。

这条对本文最有价值：**连引擎都承认"属性名与语义"属于代码，"数值"才属于表格**。本项目的 `ItemAttributeModifierDefinition` 问题恰好是把"属性名 + 运算 + 数值 + 来源分类"四件事捆在一个 C# 类型里；正确切法是让属性名与运算保持受约束的代码枚举，让"哪个物品贡献多少"进数据。

**Gameplay Effects**（[Gameplay Effects](https://dev.epicgames.com/documentation/en-us/unreal-engine/gameplay-effects-for-the-gameplay-ability-system-in-unreal-engine)）补充一个概念区分：Gameplay Effect 是**资产**（运行期不可变），Gameplay Effect Spec 是它的运行期实例。这与本项目"Definition vs. 运行期状态"的边界完全同构，也印证 `ItemDefinition` 应是不可变目录项，而不是可变对象。

### 4.2 RimWorld：XML Def

[RimWorld Wiki: XML Defs](https://rimworldwiki.com/wiki/Modding_Tutorials/XML_Defs)：

- Defs 是 XML 文件，根节点 `<Defs>`，其子节点类型名对应一个继承自 `Def` 的 C# 类（如 `<ThingDef>` → `ThingDef`）；
- "All XML nodes inside the `<ThingDef>` tag are a field in the `ThingDef` class"——**XML 结构与 C# 字段一一对应**；
- `defName` 是身份字符串，定义在基类 `Def` 上；
- 引用按"类型 + defName"解析（例：`<ingestSound>Slurp</ingestSound>` 解析为 `SoundDef`）；
- **XML 错误会级联**：游戏把所有 Defs 合并进一个巨型 XML 文档，一处未闭合标签会导致整个解析失败，"一墙红字"；
- 官方建议先修第一个错误再重载。

对本项目的含义：RimWorld 证明了"字段名即配置键"的可行性，但也展示了代价——**没有 schema 就只剩级联错误**。本项目的 `UnmappedMemberHandling.Disallow` 加逐条报错，正是为了避免这种级联。

（`abstract`/`parentName` 继承机制的一手页面本轮未取到，故不在此断言其细节。）

### 4.3 Minecraft：datapack 与注册表

[Data pack](https://minecraft.wiki/w/Data_pack)：

- 结构：`data/<namespace>/<registry>/<path>.json` → 注册表 `<registry>` 中的 id `<namespace>:<path>`；
- 命名空间用于"避免不同包的文件互相干扰"；`minecraft` 命名空间可用来覆盖原版；
- **按加载顺序合并**："If a file exists in multiple data packs only the file in the last data pack is used"——即后者覆盖前者；
- 例外：不含 `"replace": true` 的 tag 文件会**合并**而不是替换；
- 损坏的包会弹"安全模式"询问，而不是静默跳过；
- 部分特性（"experimental settings"）走**动态注册表**，改完必须重启世界，`/reload` 无效。

三条对本项目有用：

1. **命名空间隔离**：`<namespace>:<path>` 的两段式 id 天然避免内容包冲突；
2. **覆盖语义要分类**：默认整体替换，tag 类默认合并——不是"一律覆盖"也不是"一律合并"；
3. **静态/动态分层**：能在 `/reload` 生效的与必须重启的，在系统里是两套注册表。本项目当前不需要热重载，正好落在"静态"一侧，与 Minecraft 的静态注册表对应。

### 4.4 Unity：ScriptableObject

[Unity Manual: ScriptableObject](https://docs.unity3d.com/6000.2/Documentation/Manual/class-ScriptableObject.html)（Unity 6.2 / 6000.2）：

- "ScriptableObject is a serializable Unity type derived from `UnityEngine.Object`"；不直接实例化该类，而是派生自定义类，再"usually through the **Assets** menu in the Unity Editor"创建实例。
- 与 MonoBehaviour 的关键差别：**不挂在 GameObject 上，而是作为项目中的资产独立存在**。
- "The main value of a ScriptableObject is as a data store, but they can also define behavior."——定位就是数据容器，且可以作为共享数据的单一副本避免复制（"there is one copy of the data in memory"）。
- 运行期权限：**"In a standalone Player at runtime, you can only read saved data from the ScriptableObject assets."** 即打包后只读。
- 通过 `[CreateAssetMenu]` 特性把派生类暴露成"Assets > Create > …"菜单项，每个实例成为一个资产。
- 保存注意：编辑器里改 ScriptableObject 要调 `EditorUtility.SetDirty`，否则改动只存在内存里、重启编辑器后回退。

对本项目的含义：Unity 走的是"资产即定义、运行期只读"的路子，与本设计"加载后 `Seal()` 冻结目录"一致；差别是 Unity 由编辑器负责编辑与持久化，本项目没有编辑器，所以需要自己实现"文件 → DTO → 不可变定义"这条投影链。

### 4.5 NeoForge：Data Maps 与物品属性修饰器

这一节与本项目目标最接近：它正是"**让数据包给物品附加属性修饰器**"的成熟实现。

**Data Maps**（[NeoForged: Data Maps](https://docs.neoforged.net/docs/1.21.3/resources/server/datamaps/)）——官方定义：

> "A data map contains data-driven, reloadable objects that can be attached to a registered object."

几个直接可借的机制：

- **定位**："You can think of tags as registry object ➜ boolean maps, while data maps are more flexible registry object ➜ object maps."——即"注册表条目 → 任意对象"的稀疏映射，正是物品 → 属性修饰器所需形状。
- **叠加而非覆盖**："Similar to tags, data maps will add to their corresponding data map rather than overwriting."——默认累加。
- **冲突有显式合并器**：`AdvancedDataMapType` 可挂 `merger`，默认 `DataMapValueMerger#defaultMerger` 是"后来覆盖先来"；自定义 merger 可改成相加、合并集合等。想强制覆盖则在该条目写 `"replace": true`。
- **删除是一等操作**：`"remove": [...]` 列表，且"**Removals run after additions**"，因此可以先按 tag 批量加、再逐个排除。
- **条目值必须不可变**："Data map entries must be immutable, making records ideal for this."——与本项目 `ItemDefinition` 必须是不可变目录项同一要求。
- **文件路径即身份**：`<mapNamespace>/data_maps/<registryNamespace>/<registryPath>/<mapPath>.json`；例 `mymod/data_maps/item/drop_healing.json`。
- **可同步到客户端**：`.synced(codec, mandatory)`，`mandatory=false` 时缺该映射的客户端不会被踢；也可用更小的 codec 只同步客户端需要的字段。
- **可 `/reload` 热重载**。

**物品属性修饰器的实际用法**：NeoForge 的护甲几乎完全由 data component 实现，其中 `#ATTRIBUTE_MODIFIERS` 承担"护甲值、韧性、击退抗性"（见 [NeoForged: Armor](https://docs.neoforged.net/docs/1.21.3/items/armor/)）。也就是说**物品对属性的贡献本身就是一条数据条目**，不是写在物品类里的字段。

**`item_attribute_modifier` 的具体 JSON 形状**（Thermoo 的 datapack 定义，[Item Attribute Modifiers](https://thermoo.thedeathlycow.com/datapacks/item_attribute_modifier_definition/)）：

```json5
{
    "attribute": "thermoo:generic.heat_resistance",
    "modifier": {
        "uuid": "413a10a0-bf0b-47db-a9a9-2eb3dda3bbaf",
        "name": "Test",
        "value": -1.0,
        "operation": "addition"   // addition | multiply_base | multiply_total
    },
    "item": {
        "items": ["minecraft:diamond_helmet", "minecraft:iron_helmet"],
        "tag": "example:all_helmets"     // 也可用 tag 批量指定
    },
    "slot": "head",
    "require_preferred_slot": false
}
```

这份结构几乎逐项对应本项目要建的东西：

| Thermoo 字段 | 本项目对应 |
| --- | --- |
| `attribute` | `AttributeKind`（写枚举名） |
| `modifier.operation` | `AttributeModifierOperation` |
| `modifier.value` | `AttributeContribution.Value` |
| `item.items` / `item.tag` | 本设计用 `parent`/基线原型 + 逐物品覆盖（更细粒度） |
| `slot` | `AttributeSourceCategory`（Equipment/Accessory/Weapon） |
| `require_preferred_slot` | `ItemAttributeContributionRules` 的槽位白名单 |

值得注意的是它**同时支持"按 id 列表"和"按 tag 批量"**两种挂载方式，且 `require_preferred_slot` 专门处理"tag 覆盖了多种装备、但只想在某个槽位生效"这类歧义。这正是本项目 `IsContributionActive(slotKind, sourceCategory)` 在做的事。

另需记录一条**版本教训**：该特性在 Thermoo 4.0（对应 MC 1.21）被**移除**，官方给的理由是"due to the massive changes to items"，随后改为推荐 event 与 data component 方案。这说明"物品属性修饰器的数据格式"会随物品模型演进被迫重做——本项目若把格式设计得过窄（例如硬编码槽位概念），未来同样会被迫重写。

#### 4.5.1 框架结构

下图按源码类名绘制，取自 NeoForge `1.21.11` 分支的 `net.neoforged.neoforge.registries.datamaps` 包。

因为中文是双宽字符，框内一律只写英文类名与成员，中文说明放在框外，这样在等宽字体下才能真正对齐。

```text
<<sealed>>  DataMapType<R, T>
------------------------------------------------------------
fields:
  ResourceKey<Registry<R>>  registryKey
  Identifier                id
  Codec<T>                  codec          (JSON decode)
  Codec<T>                  networkCodec   (nullable; non-null = syncable)
  boolean                   mandatorySync
------------------------------------------------------------
factory:
  builder(id, registry, codec) -> Builder
accessors:
  registryKey() / id() / codec() / networkCodec() / mandatorySync()

                              |
                              |  sealed ... permits  (only one subclass)
                              v
<<final>>  AdvancedDataMapType<R, T, VR>
------------------------------------------------------------
extra fields:
  Codec<VR>                remover    (remover decode)
  DataMapValueMerger<R, T> merger     (conflict merge)
------------------------------------------------------------
accessors:
  remover() / merger()
factory:
  builder(id, registry, codec).remover(..).merger(..)

                              |
        +---------------------+---------------------+
        |                                           |
        v                                           v
(see the two interfaces below)
```

两个分支各自展开：

```text
<<interface>> DataMapValueMerger<R, T>           <<interface>> DataMapValueRemover<R, T>
-------------------------------------------     -------------------------------------------
merge(registry,                                 remove(value, registry,
      first, firstValue,                              source, object)
      second, secondValue) -> T                       -> Optional<T>
-------------------------------------------     -------------------------------------------
static helpers:                                 nested default impl:
  defaultMerger() = last-come-wins                Default<R, T>.INSTANCE
  listMerger()    = list concat                     (always returns empty
  setMerger()     = set union                        = drop value entirely)
  mapMerger()     = map putAll
```

框外说明：`Merger` 决定"两个数据包给同一对象赋值时怎么办"，`Remover` 决定"`remove` 列表里带参数时删到什么程度"。

另外三个角色不属于 `datamaps` 包内继承树，但同属一套框架：

```text
<<event>>  RegisterDataMapTypesEvent
------------------------------------------------------------
Map<RegistryKey, Map<Identifier, DataMapType>> attachments
register(DataMapType<R, T> type)
------------------------------------------------------------

<<interface>>  IWithData<R>
------------------------------------------------------------
getData(DataMapType<R, T> type) -> T      (null when absent)
------------------------------------------------------------
              ^
              |  implements
<<vanilla>>  Holder<R>                        (NeoForge makes it implement IWithData)

<<abstract>>  DataMapProvider                 (datagen side)
------------------------------------------------------------
gather()
builder(DataMapType<R, T> type)
------------------------------------------------------------
```

这三个分别是：注册入口（在 mod event bus 上触发）、运行时读取入口（原版 `Holder` 实现它，所以取值写成 `holder.getData(TYPE)`）、datagen 侧生成 JSON 的抽象基类。

三个类型参数的分工是理解整套框架的关键：

| 参数 | 含义 | 实例 |
| --- | --- | --- |
| `R` | 被挂载的**注册表类型** | `Item`、`DamageType`、`EntityType` |
| `T` | **挂载的值类型** | `FurnaceFuel`、`VibrationFrequency`、`MekaSuitAbsorption` |
| `VR` | **删除器类型**，仅 `AdvancedDataMapType` 有 | 默认 `Default<T, R>` |

继承是 `sealed ... permits`：`DataMapType` 声明为 `sealed class ... permits AdvancedDataMapType`，**只允许这一个子类**。要定制合并或删除行为，只能实现 `DataMapValueMerger` / `DataMapValueRemover` 接口并挂到 builder 上，不能靠继承扩展类型体系。

#### 4.5.2 从声明到运行时的完整链路

```text
===================  I. DECLARE AND REGISTER  (startup, mod code)  ===================

  [mod code]
      |
      |  DataMapType.builder(id, registry, codec)
      v
  +--------------------------------------+
  | needs merger / remover ?             |
  +--------------------------------------+
      | no                          | yes
      v                             v
  DataMapType                 AdvancedDataMapType.builder()
  (last-come-wins,              .merger(..)
   no targeted removal)         .remover(..)
      |                             |
      +--------------+--------------+
                     |
                     |  register(type)
                     v
  [RegisterDataMapTypesEvent]   <-- fired on mod event bus
                     |
                     |  grouped by registryKey
                     v
  attachments : Map<RegistryKey, Map<Identifier, DataMapType>>


===================  II. LOAD JSON AND RESOLVE CONFLICTS  (datapack)  ================

  [datapack file]
  <mapNS>/data_maps/<regNS>/<regPath>/<mapPath>.json
      |
      |  read "values"
      v
  +--------------------------------------+
  | decode each entry via codec -> T      |
  +--------------------------------------+
      |
      v
  +--------------------------------------+
  | value already attached ?              |
  +--------------------------------------+
      | no                          | yes
      v                             v
  [attach directly]            [call merger]
                               defaultMerger = last-come-wins
                                     |
                                     v
                               [merged value]
      |                             |
      +--------------+--------------+
                     |
                     |  then process "remove"  (strictly after "values")
                     v
  +--------------------------------------+
  | entry carries a remover object ?      |
  +--------------------------------------+
      | no                          | yes
      v                             v
  [drop the value entirely]    remover.remove(..)
                               -> Optional<T>
                               empty     = drop entirely
                               non-empty = replace with returned value
      |                             |
      +--------------+--------------+
                     |
                     v
  final map : RegistryKey -> (Identifier -> T)


===================  III. CONSUME AT RUNTIME  (in game)  ==============================

  final map
      |
      +--> Holder#getData(type) --> value, or null when absent
      |
      +--> networkCodec != null ?
              | yes                                | no
              v                                    v
      [sync to clients]                     [server side only]
      mandatory=true: clients lacking
      this map cannot connect
      (vanilla clients included)
```

图里有三处容易被忽略的顺序与约束，都是源码里明确写的：

1. **`remove` 严格晚于 `values`**（`DataMapType` 类注释原文："Entries specified here will be removed after the data of the current json file is attached"）。所以"先按 tag 批量加、再逐个排除"是官方支持的写法，不是巧合。
2. **`replace: true` 清空的是"此前所有数据"**，不只是本文件。注释写 "If true, all previous data will be cleared"，它是内容包覆盖整张表的手段；官方还明确建议模组不要发布带 `replace: true` 的数据，否则会把别人的贡献一并抹掉。
3. **`mandatorySync=true` 但没有 `networkCodec`** 会在 `DataMapType` 构造函数里被 `Preconditions.checkArgument` 直接拦下——"强制同步"的前提是"能同步"。

#### 4.5.3 注册期的两道校验

`RegisterDataMapTypesEvent.register` 在写入前会检查两件事，都属于"配置错误必须在启动期失败"：

- **重复 ID**：同一个注册表下用同一 `id` 注册两次 → `IllegalArgumentException`。
- **同步约束**：给"非同步的数据包注册表"注册带 `networkCodec` 的 Data Map → `UnsupportedOperationException`，原文是 `"Cannot register synced data map ... for datapack registry ... that is not synced!"`。

另有一条构造期断言：`mandatorySync` 为 `true` 但 `networkCodec` 为 `null` 时，`DataMapType` 构造函数直接 `Preconditions.checkArgument` 失败。

#### 4.5.4 内建 Data Map 与自建 Data Map 的分工

NeoForge 自己用这套机制替换了若干原版硬编码字段（`compostables`、`furnace_fuels`、`monster_room_mobs`、`oxidizables`、`parrot_imitations`、`raid_hero_gifts`、`vibration_frequencies`、`villager_types`、`waxables`）。官方文档对此的表述是"replacing hardcoded vanilla fields"，且"Vanilla values are shipped by data map files in NeoForge, so there is no functional difference to the player."

这句话对迁移策略有直接参考价值：**NeoForge 把"原版写死在代码里的表"搬到数据文件时，是连同默认值一起搬的**，所以玩家感知不到差异。本项目从 `BuildItemDefinitions()` 迁移时也应如此——基线原型必须精确复刻当前常量，否则"没感觉变化"这个前提不成立。

### 4.6 Godot：未取到可用正文

`https://docs.godotengine.org/en/stable/tutorials/scripting/resources.html` 返回 HTTP 200，但正文被大段文档导航占据，两次抓取（含 `#creating-your-own-resources` 锚点）都未取到关于自定义 `Resource`、`.tres` 保存格式与 `ResourceLoader` 的实质描述。因此本文**不对 Godot 的加载机制下结论**。

## 5. 横向对比

| 维度 | 参考客户端（Terraria） | Unreal | RimWorld | Minecraft | NeoForge Data Maps | Unity |
| --- | --- | --- | --- | --- | --- | --- |
| 身份 | 整数常量 | 行名 / `FDataRegistryId` | `defName` 字符串 | `namespace:path` | 注册表对象 → 值（含 tag 批量） | 资产路径 |
| 数据形状 | C# 数组 + switch 分支 | `USTRUCT` 行 + CSV | XML 节点对应字段 | JSON | JSON（codec 决定） | `.asset` 序列化对象 |
| 继承/覆盖 | 无（靠代码分支） | 数据源顺序决定 override/fallback | 未取到证据 | 后者覆盖前者；tag 默认合并 | 默认叠加；merger 决定冲突；`replace`/`remove` | 无继承，按资产引用复用 |
| 属性语义归属 | 代码（字段名） | 代码（`UAttributeSet`） | 代码（`Def` 子类） | 代码（registry 类型） | 代码（`DataMapType` + codec） | 代码（C# 类字段） |
| 数值归属 | 代码 | 表格 | XML | JSON | JSON | 资产 |
| 错误处理 | 运行期静默（默认值） | 导入时告警，可关 | 级联失败 | 安全模式 | codec 校验 | 未述 |
| 热重载 | 无 | 重新导入 | 重载游戏 | `/reload` 部分生效 | `/reload` | 运行期只读 |

**共同点**：六个系统都把"有哪些字段"放在代码里，把"每个条目的值"放在数据里。没有一个是纯数据驱动的 schema。

**差异点**：只有参考客户端把"逐条目的数值"也放进代码，代价是 6020 个 case、23291 处数字字面量赋值。

**与本项目最接近的是 NeoForge Data Maps**：同样是"注册表条目 → 稀疏对象映射"、同样要求条目不可变、同样有明确的合并与删除语义，且它的 `ATTRIBUTE_MODIFIERS` 正是"物品 → 属性修饰器"的官方实现。

**证据强度**：Unreal（4.1）、RimWorld（4.2）、Minecraft（4.3）、NeoForge（4.5）、Unity（4.4）五列均有一手官方文档正文支撑并给出链接；只有 RimWorld 的 `abstract`/`parentName` 继承细节未取到，Godot 未取到正文故未列入本表。

## 6. 对本项目的启示

1. **属性键与运算保持代码枚举**（`AttributeKind`、`AttributeModifierOperation`），只把"谁贡献多少"外置。这与 Unreal GAS 的做法一致，也避免把拼错的属性名变成运行期错误。
2. **稀疏表天然适合数据化**：`ItemID.Sets` 的 119 个数组里有明确的数值修饰（`BonusMeleeSpeedMultiplier` 等），说明物品属性贡献本来就是稀疏数据，不是逐物品全覆盖。
3. **覆盖语义要显式配置**：Unreal 的"源顺序"与 Minecraft 的"后者覆盖"都表明，覆盖不是 bug 而是特性，但必须由配置决定顺序，不能靠文件名排序。
4. **定义与映射分开**：`TileMaterials` 的 `Materials.json` + `Tiles.json` 是最小可行样板，对应本项目"基线原型 + 逐物品覆盖"。
5. **错误必须逐条可定位**：RimWorld 的级联失败与 `TileMaterials` 的空流未检查是反面教材；Minecraft 的安全模式是正面做法。
6. **不要试图自动抽取 `SetDefaults`**：6020 个 case 里数据与控制流混写，任何自动抽取都会产出不可维护的巨型 JSON。应按类别人工迁移并留行为等价快照。
7. **"删除"要是内容文件的一等字段**：NeoForge 的 `remove` 在 `values` 之后执行，因此可以"按 tag 批量加、再逐个排除"。本设计的 `parent` 只能加不能删；若将来要支持内容包覆盖，需先补一条显式的排除语义，而不是让后者"少写几个字段"。
8. **冲突合并要有显式策略**：NeoForge 用 `merger` 决定两个包给同一物品赋值时是覆盖还是相加，`replace: true` 是逃生口。本设计目前的多父合成是"后者整体替换"，等于固定了一种策略；若出现"两个来源都要保留"的内容需求，必须扩展而不是暗中改变合成规则。
9. **格式不要绑死易变的物品模型**：Thermoo 的 `item_attribute_modifier` 在 MC 1.21 因物品模型大改而被整体移除，官方理由是 "due to the massive changes to items"。本设计把 `slot` 概念压进 `AttributeSourceCategory` 枚举、把物品身份压进 `ItemId`，都属于会被上游模型变化冲击的耦合点，应在迁移时保留"重新解释旧字段"的空间。

这九点已并入 [内容定义加载系统设计](../architecture/2026-11-23-content-definition-loading-system-design.md)。
