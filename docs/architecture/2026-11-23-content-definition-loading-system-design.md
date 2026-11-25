# 内容定义加载系统设计（取代 ItemAttributeModifierDefinition 的 Definition 模型）

文档 ID：DOC-2026-11-23-CONTENT-DEFINITION-LOADING-DESIGN  
逻辑域：architecture  
产物类型：design  
状态：proposal；未实现、未编译、未运行  
日期：2026-11-23（Asia/Shanghai）  
外部调研输入：[外部项目元加载技术调研](../research/2026-11-23-external-meta-loading-research.md)、[代码内元数据与业界元数据加载技术调研](../research/2026-11-23-code-embedded-metadata-research.md)  
关联约束：[ECS 领域层与基础设施架构边界](../../Context/架构约束/ECS领域与基础设施架构边界.md)、[ECS 系统输入边界约束](../../Context/项目约束/ECS系统输入边界约束.md)、[ECS 文件组织设计约束](../../Context/架构约束/ECS文件组织设计约束.md)、[项目文件组织设计约束](../../Context/架构约束/项目文件组织设计约束.md)、[物品载荷与双宿主设计约束](../../docs/architecture/物品载荷与双宿主设计约束.md)、[组件命名设计约束](../../Context/架构约束/组件命名设计约束.md)

## 1. 要解决的问题

### 1.1 现状

物品的定义今天由三处硬编码拼出来：

| 位置 | 内容 | 问题 |
| --- | --- | --- |
| `src/NSSLC.Server/Program.cs:395` `BuildItemDefinitions()` | 对 `Enum.GetValues<ItemId>()` 的 6244 个 id 各注册一条 `new ItemDefinition(itemId, MaxInventoryStack, ItemCategoryMask.Any)` | 所有物品同一数值；改一件物品要改代码并重新编译 |
| `src/NSSLC/Share/Item/ItemDefinition.cs:17` | 构造函数接受 `IEnumerable<ItemAttributeModifierDefinition>` | 定义形状被 C# 类型锁死 |
| `src/NSSLC/Share/Item/ItemAttributeModifierDefinition.cs` | 独立类型，带 `Attribute`/`SourceCategory`/`Operation`/`Value`/`Priority` | 与 `AttributeContribution` 字段形状完全重复 |

`docs/reviews/audits/2026-11-01-attributes-subsystem-classification.md:390` 已判定：`ItemAttributeModifierDefinition` 是 `AttributeContribution` 之外的**第二份定义**，属未迁移技术债。`Context/progress.md` 将"Inventory 属性贡献与装备来源身份"登记为 deferred。

### 1.1.1 参考客户端为什么不能直接照抄

参考客户端（`D:\TRbackup\最新的客户端`）把物品元数据全部写在代码里，共五种形态：

| 形态 | 载体 | 规模 | 可迁移性 |
| --- | --- | --- | --- |
| 身份常量 | `public const` | `ItemID` 6244 项 | 已由 `ItemId.g.cs` 复刻，不需新管线 |
| 按 id 下标的稀疏数组 | `SetFactory.CreateBoolSet/CreateIntSet` | `ItemID.Sets` 119 个、`TileID.Sets` 161 个 | **可机械导出**，正是本设计要接管的形状 |
| 逐物品数值分支 | `Item.SetDefaults1..5` 的 `switch (type)` | 6020 个 `case`、23291 处数字字面量赋值 | **不可自动抽取**，须人工按类别迁移 |
| 内嵌资源 JSON | `TileMaterials` + `Resources` | 2 个 JSON | 已是数据，只需换加载入口 |
| 枚举即分类 | `CreativeHelper.ItemGroup` 等 | — | 可转成配置键，必须保留显式数值 |

结论：本设计只接管第二形态（稀疏表）与第四形态（已有数据），**不承诺自动迁移 `SetDefaults`**。这条边界写进第 6 节迁移步骤与第 7 节风险。证据与计数见 [代码内元数据与业界元数据加载技术调研](../research/2026-11-23-code-embedded-metadata-research.md) §3。

### 1.2 目标

把"一件物品有什么数值"从 **C# 类型 + 构造函数** 搬到 **内容文件 + 加载管线**，同时不改变运行时语义：

1. `ItemAttributeModifierDefinition` 被删除，物品侧贡献改用统一的 `AttributeContribution`。
2. 物品定义的权威来源是 `Content/Prototypes/ItemDefinition/**/*.json`。
3. 加载结束后得到的是**已冻结的不可变目录**，可直接作为系统构造依赖。
4. 缺少数据的物品仍有定义（兼容当前"每个 `ItemId` 都有定义"的前提），不再需要 6244 行代码。
5. 同一套管线可被 ItemUse、Buff、CombatAttack、Projectile、Entity 定义复用，但**本轮只接 Item**。

### 1.3 非目标

- 不引入热重载。服务器单进程启动期加载一次。
- 不改网络协议、存档格式、物品双宿主不变量。
- 不把内容加载放进 `Terraria.Components`（领域程序集不得做文件 I/O）。
- 不承诺一次性迁移其余 Definition 族。

## 2. 设计决定

### 2.1 三层切分

```text
内容文件（JSON）                     基础设施 / 宿主边界
  Content/Prototypes/ItemDefinition/       NSSLC.Server.Content.Prototypes
  *.json                                   · PrototypeKindRegistry    kind 注册与特性元数据
        │                                  · PrototypeSource           文件读取（唯一 I/O 点）
        ▼                                  · PrototypeResolver         继承合成 + 校验
  原型 DTO（可变、可空、宽松）
        │ 投影（纯函数，无 I/O）
        ▼
领域定义（不可变）                       领域边界
  ItemDefinition                            Terraria.Item
  AttributeContribution                     · ItemDefinitionRegistry（Seal 后只读）
  ItemAttributeContribution                 · 值对象自校验，重复构造器的全部不变量
        │
        ▼
系统构造参数（只读目录）
  InventorySystem / InventoryAttributeContributionSystem
```

方向固定为 **文件 → DTO → 领域类型 → 目录 → 系统**。任何一层不得反向读取下层的数据文件。

### 2.2 为什么物品侧需要一个新的薄类型

`AttributeContribution` 表达"贡献什么"，刻意不含来源分类（见其 XML 注释）。物品侧还必须知道"这条贡献在哪种槽位生效"，所以新增一个**只做配对**的类型，而不是复刻字段：

```csharp
// src/NSSLC/Share/Item/ItemAttributeContribution.cs
public readonly record struct ItemAttributeContribution
{
  public ItemAttributeContribution(
      AttributeSourceCategory sourceCategory,
      AttributeContribution contribution)
  {
    if (!ItemAttributeContributionRules.IsInventorySource(sourceCategory))
    {
      throw new ArgumentOutOfRangeException(nameof(sourceCategory));
    }

    SourceCategory = sourceCategory;
    Contribution = contribution;
  }

  public AttributeSourceCategory SourceCategory { get; }

  public AttributeContribution Contribution { get; }
}
```

`ItemAttributeModifierDefinition` 的全部字段（`Attribute`、`Operation`、`Value`、`Priority`）由 `Contribution` 承担，`SourceCategory` 由外层承担，`IsInventorySource` 移入规则类型。删除该文件后，`AttributeContribution` 成为唯一形状。

`ItemDefinition` 的构造签名相应改为接受 `IEnumerable<ItemAttributeContribution>?`；`AttributeModifiers` 属性删除，替换为 `AttributeContributions`。`InventoryAttributeContributionSystem.Rebuild`（`src/NSSLC/Share/Inventory/InventoryAttributeContributionSystem.cs:106`）改为遍历新属性并直接构造 `AttributeModifier`。

### 2.3 kind 注册：从"每族一个 Registry"到"一个 kind 表"

对齐 SS14 的 `[Prototype]` 结构，但保持本项目现有的 JSON 序列化（`System.Text.Json`），不引入 YAML：

```csharp
[AttributeUsage(AttributeTargets.Class)]
internal sealed class PrototypeAttribute : Attribute
{
  public PrototypeAttribute(string kind, int loadPriority = 0) { ... }
  public string Kind { get; }
  public int LoadPriority { get; }
}
```

- `Kind` 是文件里 `"type"` 字段的合法值，全局唯一；重复注册在启动期抛错。
- `LoadPriority` 决定 kind 之间的处理顺序（SS14 `[Prototype(2)]` 的等价物），**不**由文件枚举顺序决定。
- 现状 `PrototypeManager.ValidatePrototype` 用 `prototype is ItemUseDefinitionPrototype` 硬编码判断 kind，改为查 kind 表，新增 kind 不再改加载器。

### 2.4 继承与抽象

比照 `IInheritingPrototype`，DTO 层增加三个保留字段：

| 字段 | 类型 | 语义 |
| --- | --- | --- |
| `id` | `string` | 实例身份；kind + id 全局唯一 |
| `parent` | `string` 或 `string[]` | 一个或多个父原型 id；按数组顺序依次合成 |
| `abstract` | `bool` | 仅作模板；不进入目录，不可被 `Get` 取到 |

合成规则（单父 `push(child, parent)`、多父按声明顺序折叠）：

- 标量：子值非空则覆盖父值；子值缺省则继承父值。
- 数组/对象：整体替换，不做元素级合并（与 SS14 mapping 合成一致，避免"删掉父数组某一项"无法表达）。
- `id`/`parent`/`abstract` 不参与继承，只属于声明本身。
- 环检测在合成前用显式图完成；有环、缺父、`abstract` 被子引用以外的误用都在这一步报错。

**多父的用途**在本项目里是"基础物品 + 材质 + 品质"三段式，例如：

```json
{ "id": "BaseAccessory", "abstract": true, "categories": ["Accessory"], "maxStack": 1 }
{ "id": "SilverTier",   "abstract": true, "value": 2 }
{ "id": "SilverCharm",  "parent": ["BaseAccessory", "SilverTier"], "attributeContributions": [...] }
```

**合并策略是固定的，必须写明**：多父冲突时后声明的父胜出（数组顺序即优先级）。这不是唯一合理的选择——NeoForge Data Maps 用可插拔的 `merger` 让内容作者在"覆盖 / 相加 / 合并集合"之间选，并提供 `"replace": true` 作为逃生口（见调研 §4.5）。本设计第一期只做"后者整体替换"，因为本项目的内容由单一团队维护、尚无多包叠加需求；**如果将来要支持内容包叠加，必须新增显式 merger 字段，而不是悄悄改变合成规则**——改变合成规则会让既有的全部定义静默变值。

**没有"删除"语义，这是已知缺口**：NeoForge 的 `remove` 在 `values` 之后执行，所以能"按 tag 批量加、再逐个排除"。本设计的 `parent` 只能加不能删；`abstract` 模板一旦给了某字段，子原型无法表达"我没有这个字段"。第一期接受这个限制（基线原型只提供 `maxStack`/`categories` 两个字段，覆盖成本低），若后续要在多层原型的数组上做删减，必须补显式的排除字段。

### 2.5 为什么只外置数值、不外置属性键

Unreal GAS 的官方文档明确写"Attributes and Attribute Sets must be created in native code — they cannot be created in Blueprints"，同时允许用 DataTable 的 `AttributeMetaData` 行类型填初值。RimWorld 与 Minecraft 亦如此：字段名由 C# 类或 registry 类型决定，数据只提供值。

本设计沿用同一条界线：

- **留在代码里**：`AttributeKind`、`AttributeModifierOperation`、`AttributeSourceCategory`（受约束的枚举，拼错即编译错误）、`ItemAttributeContributionRules`（来源白名单）。
- **进内容文件**：某个物品贡献哪个属性（写枚举名）、用什么运算、多少数值、优先级。

这样，内容错误只可能是"写了一个不存在的枚举名"或"数值越界"，都能在加载期用 `Enum.TryParse` + `Enum.IsDefined` 拦下；而不是引用一个不存在的属性。

### 2.6 物品定义的"全覆盖"策略

当前 `BuildItemDefinitions()` 给全部 6244 个 `ItemId` 建定义，`InventorySystem` 依赖"写入必有定义"（`src/NSSLC/Share/Item/README.md` 亦以此为前提）。若改成"只加载文件里写了的东西"，绝大多数物品会立刻失去定义。

**值域陷阱（必须按 6244 全覆盖，不能按 6196）**：`ItemId` 的 6244 个成员里有 48 个负值（`PlatinumBowOld = -48`、`YellowPhasesaberOld = -24` 等 "Old" 物品），其余 6196 个是 `0..6195`。参考客户端的 `ItemID.Count = 6196`，`SetFactory` 建的数组长度也是 6196，**负 id 根本不在那些数组里**。但本项目 `ItemIdRules.IsKnown` 用 `Enum.IsDefined` 判断，负 id 同样"已知"，所以当前 `BuildItemDefinitions()` 也给它们建了定义。补全与验证都必须枚举 `Enum.GetValues<ItemId>()`（6244 项），不能用参考客户端的 `Count` 做上界；否则负 id 会在 `Get` 时抛 `KeyNotFoundException`，与当前行为不等价。

因此加载器必须做两件事：

1. **基线原型**：目录内必须存在一条 `id` 等于约定常量（如 `"__default"`）的 `abstract` 原型，作为所有未显式声明物品的隐式父。
2. **补全**：加载完成后，对每个 `ItemId`（除 `None`）检查是否已有定义；缺失者用基线原型投影出一条"仅含 id 与基线字段"的 `ItemDefinition`。补全发生在本项目内、加载器内部，且结果同样进入冻结目录。

这样，内容的"显式集合"可以随迁移逐步长大，而运行时前提不被破坏。基线原型的 `maxStack` 与 `categories` 必须与当前 `MaxInventoryStack` / `ItemCategoryMask.Any` 一致，以保证首批迁移的**行为等价**。

### 2.7 文件组织

```text
src/NSSLC.Server/Content/Prototypes/
  IPrototype.cs                       # 保留，增加 kind 语义注释
  PrototypeAttributes.cs              # PrototypeAttribute 增加 Kind / LoadPriority
  PrototypeManager.cs                 # 拆为 Source / Resolver；对外只保留 LoadDirectory<T>
  ItemUse/
    item-use.json                     # 现状，不动
  ItemDefinition/
    _base.json                        # 基线抽象原型（文件名不代表加载顺序）
    items.armor.json
    items.accessories.json
    items.weapons.json
  ItemDefinition/ItemDefinitionPrototype.cs
  ItemDefinition/ItemDefinitionPrototypeLoader.cs
```

新文件按 `项目文件组织设计约束` §3.3 落在宿主项目的内容边界内；`Share/Item` 只增加 `ItemAttributeContribution.cs` 与一个规则类型，不引入任何 JSON/IO 类型。

`NSSLC.Server.csproj:16` 的 `Content\Prototypes\**\*.json` 复制规则已覆盖新目录，无需改动。

### 2.8 原型 DTO

```csharp
[Prototype("itemDefinition", loadPriority: 10)]
internal sealed class ItemDefinitionPrototype : IPrototype
{
  [IdDataField]
  public string Id { get; init; } = string.Empty;

  [DataField]
  public string[]? Parent { get; init; }

  [DataField]
  public bool Abstract { get; init; }

  [DataField]
  public int? MaxStack { get; init; }

  [DataField]
  public string[]? Categories { get; init; }

  [DataField]
  public ItemAttributeContributionPrototype[]? AttributeContributions { get; init; }
}

internal sealed class ItemAttributeContributionPrototype
{
  [DataField(required: true)]
  public string Attribute { get; init; } = string.Empty;

  [DataField(required: true)]
  public string Operation { get; init; } = string.Empty;

  [DataField(required: true)]
  public float Value { get; init; }

  [DataField]
  public int Priority { get; init; }

  [DataField(required: true)]
  public string SourceCategory { get; init; } = string.Empty;
}
```

枚举一律用**成员名**而非整数：`"Defense"`、`"Additive"`、`"Accessory"`。理由与 `ItemUseDefinitionPrototype` 现状一致（`"useAmmo": "WoodenArrow"`），且避免枚举重排导致内容静默错位。解析用 `Enum.TryParse(..., ignoreCase: false)` 并要求 `Enum.IsDefined`。

`Expression-bodied`、`init`、`required` 的使用遵守 [C# 风格约束](../../Context/项目约束/Google-CSharp-Style-Guide-约束.md)：DTO 是 Plain Old Data，允许对象初始化器；领域类型仍走构造函数校验。

### 2.9 加载阶段与失败语义

```text
1 Survey      枚举 Content/Prototypes/ItemDefinition/**/*.json
              按 Ordinal 排序，只为错误信息可复现；不表达任何语义顺序
2 Parse       逐文件 JsonSerializer.Deserialize<List<ItemDefinitionPrototype>>
              未知字段 → JsonException（沿用 UnmappedMemberHandling.Disallow）
3 Shape       校验 id 非空、kind 匹配、type 字段一致、id 全局唯一
4 Graph       收集 parent 引用 → 缺父 / 环 / 自引用 → 错误
5 Compose     按拓扑序合成，多父按声明顺序折叠
6 Project     逐条投影为 ItemDefinition；领域构造函数完成最终校验
7 Complete    对未声明 ItemId 用基线原型补全
8 Seal        ItemDefinitionRegistry.Seal()，此后 Register 抛异常
9 Handoff     宿主把冻结目录交给系统构造
```

失败一律抛 `InvalidDataException` 并携带 `文件 + 条目索引 + 字段名`。这条要求比现状更严：现有 `PrototypeManager` 在解析层只报"文件 + 索引"，字段名要到 `ItemUseDefinitionPrototypeLoader` 的投影层才出现；本设计把字段名提前到阶段 2–3 就带上，因为一次 `JsonException` 只说明"某个文件第 N 条坏了"，不足以定位是哪个字段。**不允许**跳过坏条目继续启动：内容是权威数值，静默降级会让平衡问题在运行期才暴露。

阶段 3 与阶段 6 的分工：阶段 3 只做"数据形状"判断（能不能构成一条定义），阶段 6 才做"领域规则"判断（数值范围、来源分类白名单、堆叠上限）。领域规则只在领域构造函数里写一次，加载器不复制。

### 2.10 目录冻结

`ItemDefinitionRegistry` 现状（`src/NSSLC/Share/Item/ItemDefinitionRegistry.cs`）没有 `Seal()`，而 `ItemUseDefinitionCatalog` 有。按 [ECS 系统输入边界约束](../../Context/项目约束/ECS系统输入边界约束.md) 的术语，没有 `Seal()` 的注册表"不算不可变目录对象"，不能作为系统构造依赖。

因此本设计把 `ItemDefinitionRegistry` 收紧为：

- 增加 `_sealed` 与 `Seal()`；`Register` 在冻结后抛 `InvalidOperationException`。
- 增加 `Definitions`（`IReadOnlyCollection<ItemDefinition>`）与 `Count`。
- `TryGet` / `Get` 不变。

冻结时机由宿主在加载结束、系统构造之前显式调用，**不**依赖"构造函数里最后一次 Register"这种隐式边界。

## 3. 与现有结构的关系

| 现有类型 | 处理 |
| --- | --- |
| `ItemAttributeModifierDefinition` | **删除**；字段并入 `AttributeContribution` + `ItemAttributeContribution` |
| `ItemDefinition.AttributeModifiers` | 改名 `AttributeContributions`，元素类型换为 `ItemAttributeContribution` |
| `ItemDefinitionRegistry.Register` | 保留签名，增加冻结检查 |
| `Program.BuildItemDefinitions()` | 改为 `ItemDefinitionPrototypeLoader.Load(new PrototypeManager(), path)`；函数体删除 |
| `InventoryAttributeContributionSystem.Rebuild` | 遍历 `AttributeContributions`，用 `contribution.Contribution.ToModifier(category, sourceId)` 生成修饰器 |
| `PrototypeManager.ValidatePrototype` | 去掉 `is ItemUseDefinitionPrototype` 硬编码，改为查 kind 表 |
| `ItemUseDefinitionPrototypeLoader` | 不动；下一轮可迁到同一 kind 表（不属本轮） |
| `BuffDefinitionRegistry` | 不动；它不在本轮范围，且同样是"无 Seal"的迁移债 |

`AttributeContribution.ToModifier` 已经接受 `sourceCategory` 与 `stacks`，因此新的消费路径无需在物品侧再拼一次 `AttributeModifier` 构造参数——比现状更短。

## 4. 不变量

| ID | 不变量 | 判定方式 |
| --- | --- | --- |
| INV-A | **单一形状**：物品侧不再存在与 `AttributeContribution` 同形的第二份定义 | 静态：`rg "record struct .*AttributeModifierDefinition"` 零命中 |
| INV-B | **目录冻结**：加载结束后目录不可写入 | 运行：`Seal()` 后 `Register` 抛 `InvalidOperationException` |
| INV-C | **全覆盖**：每个非 `None` 的 `ItemId` 都能 `Get` 到定义，含 48 个负值 id | 运行：遍历 `Enum.GetValues<ItemId>()`，不是遍历 `0..6195` |
| INV-D | **可判定失败**：缺父、环、重复 id、未知字段、未知枚举名、非白名单来源分类均在启动期失败 | 运行：六类负例夹具各一条 |
| INV-E | **顺序无关**：文件顺序、目录顺序不改变任何一条定义的结果 | 运行：打乱文件顺序两次加载，目录快照逐字段相等 |
| INV-F | **载荷不回流**：`ItemDefinition` 仍只承载按种类的规则，不出现位置、速度、归属 | 静态：字段清单评审 |

INV-E 是 [项目文件组织设计约束](../../Context/架构约束/项目文件组织设计约束.md) §5"不得依赖文件枚举顺序"在加载器上的具体落点：排序只用于可复现的错误输出，语义顺序全部来自 `parent` 与 `loadPriority`。

## 5. 验证设计

### 5.1 新增验证项目

`Test/NSSLC.Server/Terraria.Server.ContentPrototypes/`（`Test/README.md` 允许"其他边界等第一个验证项目出现再建"；这是 `NSSLC.Server/` 下的第二个项目，目标是内容加载）。

检查项（`Program.cs` 打印 `PASS`/`FAIL`，失败返回 1）：

| 前缀 | 检查 |
| --- | --- |
| `shape-*` | 缺 `id`、重复 `id`、`type` 与 kind 不符、未知字段、未知枚举名各一条 |
| `graph-*` | 缺父、环、自引用、抽象原型被当作实例 `Get` |
| `compose-*` | 单父覆盖、多父按声明顺序折叠、数组整体替换 |
| `complete-*` | 全覆盖：遍历 `Enum.GetValues<ItemId>()` 的**全部 6244 项**（含 48 个负 id）都能 `Get`；再随机 50 个未声明 id 取到基线定义；基线字段等于 `MaxInventoryStack` / `Any` |
| `seal-*` | `Seal()` 后 `Register` 抛异常；`Seal()` 幂等 |
| `order-*` | 同一内容集两种文件顺序，产物逐字段相等 |
| `equiv-*` | 迁移夹具：一条显式声明的装备定义，其 `AttributeModifier` 与旧 `ItemAttributeModifierDefinition` 路径逐字段相等 |

`equiv-*` 是本次迁移的**行为等价闸门**：先冻结旧路径的输出快照，再切换新路径比对。

### 5.2 既有验证回归

受影响项目按 [构建与验证约束](../../Context/项目约束/构建与验证约束.md) 增量构建：

```powershell
dotnet build src/NSSLC/Terraria.Components.csproj
dotnet build src/NSSLC.Server/NSSLC.Server.csproj
dotnet run --project Test/Terraria.Components/Terraria.Inventory/Terraria.Inventory.csproj --no-build --no-restore
dotnet run --project Test/Terraria.Components/Terraria.WorldItem/Terraria.WorldItem.csproj --no-build --no-restore
dotnet run --project Test/NSSLC.Server/Terraria.Server.ContentPrototypes/Terraria.Server.ContentPrototypes.csproj --no-build --no-restore
```

`Terraria.Inventory` 的 `invariant-*` / `slot-*` 断言必须全绿——它们直接比对 `ItemDefinition` 的堆叠与类别行为。

## 6. 迁移步骤

| 步 | 内容 | 可独立回滚 |
| --- | --- | --- |
| P0 | 给 `ItemDefinitionRegistry` 加 `Seal()` / `Count`，`Program` 加载后调用；行为不变 | 是 |
| P1 | 新增 `ItemAttributeContribution` 与规则类型；`ItemDefinition` 双属性并存（旧 `AttributeModifiers` 标 `[Obsolete]` 转发） | 是 |
| P2 | 新增 DTO、kind 表、加载器与 `ItemDefinition/_base.json`（内容仅一条抽象基线）；加载结果与 `BuildItemDefinitions()` 输出比对，不等则失败 | 是 |
| P3 | `Program` 切换到加载器；旧 `BuildItemDefinitions()` 删除 | 是（恢复函数体） |
| P4 | `InventoryAttributeContributionSystem` 切到新属性；删除 `ItemAttributeModifierDefinition` 与 `[Obsolete]` 转发 | 是（需回退 P1–P4） |
| P5 | 分批录入真实物品数据（护甲 → 饰品 → 武器），每批附 `equiv-*` 快照 | 是（删除该批 JSON） |

P2 的"比对"是关键：在删掉旧路径之前，新路径必须能**逐字段复现**旧路径对 6244 个 id 的输出。

**不做的迁移**：不尝试把 `Item.SetDefaults1..5` 的 6020 个 `case`、23291 处数字字面量赋值自动抽取成 JSON。该处数据与控制流混写（同一 `case` 内既有 `damage = 20;` 也有 `if (Main.myPlayer == ...)` 的条件逻辑），自动抽取会产出不可维护且行为不等价的巨型文件。正确做法是按类别（护甲 → 饰品 → 武器）人工迁移，每批附 `equiv-*` 快照。`SetDefaults` 中的非属性字段（使用音效、绘制偏移、商店价格等）本轮不迁移。

## 7. 风险与代价

| 风险 | 说明 | 缓解 |
| --- | --- | --- |
| 内容错误从编译期移到启动期 | 拼错的属性名不再是编译错误 | `Disallow` 未知字段 + 枚举名精确匹配 + 独立验证项目 6 类负例 |
| 6244 条定义的内容量 | 全量录入不现实 | 基线原型 + 按需覆盖；C 不变量的补全保证运行期前提 |
| 多父合成的歧义 | 两个父都给 `maxStack` 时谁赢 | 以声明顺序为准并写入文档；验证 `compose-*` 覆盖 |
| 冻结与补全的时序 | 补全必须在 `Seal()` 前 | 加载器单点负责，宿主不单独调 `Register` |
| `ItemDefinition` 同时承载类别与贡献 | 定义类会随内容增长变胖 | 超过 5 个字段族时再拆 `ItemCategoryDefinition`，不在本轮预拆 |
| 跨轮冲突 | `BuffDefinitionRegistry` 同样是"无 Seal 的注册表" | 本轮只动 Item；在技术债登记 Buff 侧同类问题 |
| 误以为能吃掉 `SetDefaults` | 6020 个 case 内含条件逻辑，自动抽取会产出行为不等价的数据 | 已在第 6 节明确排除；只迁移稀疏表形态 |
| 覆盖顺序被文件名绑架 | 多文件内容需要确定谁覆盖谁 | 覆盖只由 `parent` 与 `loadPriority` 决定；文件排序仅用于错误复现（Unreal DataRegistry 与 Minecraft datapack 均以显式源顺序表达覆盖） |
| 无"删除"语义 | `parent` 只能加不能删，子原型无法表达"我没有这个字段" | 第一期基线原型只提供 `maxStack`/`categories`，覆盖成本低；多包叠加需求出现时补显式排除字段（NeoForge `remove` 是现成参照） |
| 合并策略被写死 | 多父固定"后者整体替换"，无 merger 逃生口 | 单团队维护、无多包叠加前不引入；引入时必须新增字段，禁止改变既有合成规则 |
| 格式与物品模型耦合 | `AttributeSourceCategory` 编码了槽位概念，Thermoo 的同类格式曾因 MC 物品模型大改被整体移除 | 迁移时保留"重新解释旧字段"的空间；不把槽位判断散落到内容文件里 |

## 8. 未决问题

1. **数值单位**：`AttributeContribution.Value` 目前是 `float` 绝对值；内容里是否允许写百分比（如 `"+5%"`）需要先定运算通道语义。本设计暂定"只写绝对值，百分比仍由 `Operation` 表达"，需用户确认。
2. **数据录入来源与顺序**：参考客户端可提供的形状是"按 id 下标的稀疏数组"（`ItemID.Sets` 119 个）与"逐物品 switch 分支"（6020 个 case）。前者可半自动转成 `attributeContributions` 条目，后者必须人工。需确认第一批只做护甲（`ArmorIDs` 50 个集合 + `defense` 赋值）是否足够验证管线，还是先做一个更小的手工子集（如 8 件饰品）打通端到端。
3. **是否本轮就引入 YAML**：本设计选 JSON，理由是宿主项目已有 `System.Text.Json` 管线与 `item-use.json` 先例，零新依赖。若后续内容规模需要注释与多行文本，需重新评估。
4. **是否给 `ItemUse` 一并迁到同一 kind 表**：能减少一套 loader，但会把两个迁移绑在一起，风险更高。建议下一轮单独做。
5. **`ItemDefinition` 是否需要前缀（Prefix）规则**：`ItemStack.Prefix` 目前是未实现的 `byte` 字段（见属性审计 §6），前缀贡献若要进内容文件，需先定前缀定义模型。
6. **是否需要"按 tag 批量挂载"**：Thermoo 的 `item_attribute_modifier` 支持 `item.items`（显式列表）与 `item.tag`（批量）两种挂载方式，并配 `require_preferred_slot` 处理 tag 跨槽位重叠。本设计用"基线原型 + 逐物品覆盖"代替 tag，前提是物品分类已足够准确；若发现大量"同一批物品加同一条贡献"的内容，应考虑补 `tag` 字段而不是复制 N 条记录。

## 9. 完成定义

当且仅当以下条件全部满足，本设计才可从 proposal 变为已实施：

1. INV-A 至 INV-F 各有可执行证据。
2. `Test/NSSLC.Server/Terraria.Server.ContentPrototypes` 全绿，`Test/Terraria.Components/Terraria.Inventory` 与 `Terraria.WorldItem` 回归全绿。
3. `ItemAttributeModifierDefinition` 文件已删除，全仓零引用。
4. 至少一类真实物品（护甲）以文件数据驱动，且其 `AttributeModifier` 与迁移前快照逐字段相等。
5. 构建 0 warning / 0 error，产物位于 `Build/bin/`。
6. 文档、`Context/progress.md` 与技术债登记已同步。
