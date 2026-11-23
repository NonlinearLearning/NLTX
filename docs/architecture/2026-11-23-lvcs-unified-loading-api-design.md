# LVCS 统一加载系统 API 设计

文档 ID：DOC-2026-11-23-LVCS-UNIFIED-LOADING-API  
逻辑域：architecture  
产物类型：design  
状态：implemented（首批：ItemUse 族已切换，含分派层；49 项验证通过 + 真实启动对照）  
日期：2026-11-23（Asia/Shanghai）  
关联约束：[ECS 领域层与基础设施架构边界](../../Context/架构设计/ECS领域与基础设施架构边界.md)、[ECS 系统输入边界约束](../../Context/约束/ECS系统输入边界约束.md)、[ECS 文件组织设计约束](../../Context/架构设计/ECS文件组织设计约束.md)、[项目文件组织设计约束](../../Context/架构设计/项目文件组织设计约束.md)、[C# 风格约束](../../Context/约束/Google-CSharp-Style-Guide-约束.md)  
相关研究：[参考客户端元数据普查](../research/2026-11-23-client-metadata-survey.md)、[代码内元数据与业界元数据加载技术调研](../research/2026-11-23-code-embedded-metadata-research.md)

## 1. 命名与定位

**LVCS** 是本系统的名字，对标 VFS（Virtual File System）。其中 `VCS` 部分可读作 Virtual Content System，**`L` 不展开**（见 §1.1）。

### 1.1 关于 `L`

**`L` 没有语义，是历史保留前缀。** 本系统最初在 `VCS` 与 `LVCS` 之间选名，最终采用 `LVCS`；`L` 不展开为任何技术含义，也不承诺"分层"这一设计属性。文档与代码中一律写作 `LVCS` / `Lvcs`，不得反向解释 `L` 为 `Layered`、`Loader`、`Lightweight` 或其他词。

需要分层的需求（多来源叠加、后者覆盖前者）由**来源本身的组合**承载，与名字无关。当前实现不含挂载顺序类型：一个族一个来源，多来源叠加尚未实现（见 §12）。

> 这条记录的目的：避免后续维护者从缩写反推设计意图。缩写不承载语义时，必须显式说明"不承载"，否则每个新人都会自行猜一个展开，产生多种误解。

### 1.2 定位

VFS 的价值不在"统一接口"本身，而在三件事：**接口窄、类型由挂载点决定、操作可组合**。LVCS 复制同样的分工：

- 上层只看到 `ILvcsCodec<TRaw, TValue>` 与 `LvcsLoadResult<TValue>`；
- 下层可以是 JSON 记录、稀疏映射、规则树、配方表；
- 换格式不改加载器，换族不改上层。

### 1.3 大小写

C# 标识符按 [C# 风格约束](../../Context/约束/Google-CSharp-Style-Guide-约束.md) §2.1「缩写按单词处理，例如 `MyRpc`，不要写成 `MyRPC`」取 `Lvcs`（命名空间 `Terraria.Lvcs`、类型 `ILvcsCodec`）。文档正文使用 `LVCS` 作为系统名。

## 2. 分层与命名空间

三层，职责互斥：

```text
src/NSSLC/Share/Lvcs/                    namespace Terraria.Lvcs              契约（端口 + 值类型）
  LvcsFamilyId.cs                          族身份
  ILvcsUnit.cs / LvcsUnit.cs               原始内容单元
  ILvcsSource.cs                           内容来源端口
  ILvcsCodec.cs                            格式驱动契约（含 ValueType / Extensions）
  ILvcsCodecDescriptor.cs                  非泛型 codec 视图
  ILvcsCodecRegistry.cs                    分派端口
  ILvcsLoader.cs                           加载入口端口
  LvcsContentException.cs                  内容拒绝信号
  LvcsDiagnostic.cs / LvcsDiagnosticSeverity.cs / LvcsDiagnosticSink.cs   诊断
  LvcsLoadResult.cs                        加载结果

src/NSSLC.Infrastructure/Lvcs/           namespace NSSLC.Infrastructure.Lvcs   适配器
  FileLvcsSource.cs                        文件枚举（唯一 System.IO）
  LvcsLoader.cs                            管线实现 + 异常转换
  LvcsCodecDescriptor.cs                   泛型 codec 到非泛型视图的包装
  LvcsCodecRegistry.cs                     分派实现

src/NSSLC.Server/Content/                namespace NSSLC.Server.Content        具体内容族
  ItemUse/ItemUseCodec.cs
  ItemUse/ItemUsePrototype.cs
```

**分层依据**（既有约束的推导，非设计偏好）：

| 约束原文 | 出处 | 推导 |
| --- | --- | --- |
| "ECS 领域层不依赖文件/云平台实现" | [架构边界](../../Context/架构设计/ECS领域与基础设施架构边界.md) §5 | `Terraria.Lvcs` 不得引用 `System.IO` |
| "基础设施适配器…执行本地/云端文件访问、格式编解码" | 同上 §3 | 文件枚举属基础设施 |
| "系统构造参数只允许 `ArchWorld`、只读端口、不可变目录对象" | [输入边界](../../Context/约束/ECS系统输入边界约束.md) 规则 3 | 加载结果是不可变目录，可作构造参数 |

**这条分界线在项目里已经存在**：`Terraria.*` 在共享项目、`NSSLC.Server.Content.*` 在服务端项目。

## 3. 身份

```csharp
public readonly record struct LvcsFamilyId
{
  public LvcsFamilyId(string value);      // 空或全空白即抛
  public string Value { get; }
}
```

族身份用字符串而非枚举：新增一个族是"写一个新 codec"，不是"改一个所有程序集都要重编译的枚举"。

**当前实现不含跨族统一键。** 每个族自己决定条目如何索引（`ItemUseDefinitionCatalog` 用 `ItemId`），通用层不介入。早期设计中的 `LvcsKey`（族 + 原生 id）已删除：它只被诊断使用，而诊断用 `string? Id` 已足够，多一层结构没有收益。

## 4. 内容单元与来源

```csharp
public interface ILvcsUnit
{
  string Location { get; }              // 仅用于诊断
  ReadOnlyMemory<byte> Payload { get; } // 交给 codec 的字节
}

public interface ILvcsSource
{
  LvcsFamilyId Family { get; }
  IEnumerable<ILvcsUnit> ReadUnits();
}
```

`Payload` 是字节而非文本，因此 JSON / YAML / 二进制表都用同一个契约，通用层不知道格式。

`Location` 是自由文本，实现在 `FileLvcsSource` 里填相对路径（`items.armor.json`），领域层从不解释它。

`ILvcsSource` 是领域层唯一带 I/O 意味的抽象，但不含任何 `System.IO` 类型——实现只在基础设施。

### 4.1 文件来源的行为

`FileLvcsSource` 的三条明确行为：

| 情况 | 行为 |
| --- | --- |
| 目录不存在 | 抛 `DirectoryNotFoundException` |
| 目录内无匹配文件 | 抛 `InvalidDataException` |
| 正常 | 按完整路径 **Ordinal** 排序，逐文件产出单元 |

排序**仅为诊断可复现**，不表达语义顺序。依赖顺序的族必须自己显式表达，不能靠文件名。

## 5. 格式驱动

```csharp
public interface ILvcsCodec<TRaw, TValue>
    where TRaw : class
    where TValue : class
{
  LvcsFamilyId Family { get; }

  /// <exception cref="LvcsContentException">该单元不是合法内容。</exception>
  IReadOnlyList<TRaw> Parse(ILvcsUnit unit, LvcsDiagnosticSink diagnostics);

  /// <exception cref="LvcsContentException">该批不是合法内容。</exception>
  TValue Project(IReadOnlyList<TRaw> raws, LvcsDiagnosticSink diagnostics);
}
```

### 5.1 为什么 `Parse` 返回列表

一份文件常含多条记录（`item-use.json` 就是 3 条），所以单元 ≠ 条目。`Parse` 返回 `IReadOnlyList<TRaw>`，一个文件产出零到多条。

### 5.2 为什么失败用抛异常，不用返回值

外部加载器最常见的形状是**传入目标对象、格式不对就抛**：

```csharp
void Load(Stream stream, Widget target);   // 不返回对象，失败抛异常
```

若契约用 `TRaw?` / `TValue?` 表示失败，每个 codec 都得包一层 try/catch 把异常翻成 null，等于把同一个适配逻辑复制到每个族。改成"允许抛"，codec 能直通外部 API：

```csharp
public IReadOnlyList<WidgetRaw> Parse(ILvcsUnit unit, LvcsDiagnosticSink diagnostics)
{
    WidgetRaw target = new();                        // 填充目标由 codec 自己造
    using MemoryStream stream = new(unit.Payload.ToArray());
    new ExternalWidgetLoader().Load(stream, target); // 格式不对它自己抛
    return [target];                                 // 没抛就是成功
}
```

**没有 try/catch，没有适配层。**

### 5.3 诊断与异常的分工

| 职责 | 归属 |
| --- | --- |
| 字段级细节（哪条、哪个字段） | `LvcsDiagnosticSink` |
| 控制流（这批能不能用） | `LvcsContentException` |

codec **可以先写若干诊断再抛**，让一份文件里的多个问题一次报全。`ItemUseCodec` 就是这么做的：遍历所有条目、逐条写诊断、最后统一抛。

### 5.4 填充目标的丢弃规则

外部加载器可能**先写一半再抛**。因此 codec 必须每次新建目标、失败即丢弃，不得复用实例，否则下次尝试会从半成品状态开始。

### 5.5 能力声明：`ValueType` 与 `Extensions`

```csharp
/// <summary>Domain value type this codec produces.</summary>
Type ValueType => typeof(TValue);          // 默认实现，一般不必覆写

/// <summary>File extensions this codec reads, with a leading dot.</summary>
IReadOnlyList<string> Extensions { get; }
```

两个成员都是**由 codec 自己报告的能力**，不是外部配置文件：

- 加一种格式，只改该 codec；
- 分派层不需要知道任何格式知识。

这与 Godot 的 `get_recognized_extensions()` / `handles_type()` 同构——那两个也是虚函数，由 loader 返回自身能力。

`Extensions` 的校验发生在注册时：为空、或缺少前导点，都会在 `LvcsCodecDescriptor` 构造时抛 `InvalidOperationException`。放在构造期而非使用期，是为了让配置错误在装配时暴露。

## 6. 分派

### 6.1 为什么需要分派

没有分派时，装配代码必须显式指定 codec：

```csharp
new LvcsLoader().Load(new ItemUseCodec(), source);   // 需要 using 具体命名空间
```

有了分派，调用方只需说明「要什么类型」：

```csharp
registry.TryResolve<ItemUseDefinitionCatalog>(out var descriptor, out string? reason);
```

### 6.2 两个类型

```csharp
public interface ILvcsCodecDescriptor
{
  LvcsFamilyId Family { get; }
  Type ValueType { get; }
  IReadOnlyList<string> Extensions { get; }
  object? Load(ILvcsSource source, out IReadOnlyList<LvcsDiagnostic> diagnostics);
  bool Handles(string extension);
}

public interface ILvcsCodecRegistry
{
  int Count { get; }
  void Register<TRaw, TValue>(ILvcsCodec<TRaw, TValue> codec)
      where TRaw : class where TValue : class;
  bool TryResolve<TValue>(out ILvcsCodecDescriptor? descriptor, out string? reason)
      where TValue : class;
  bool TryResolve<TValue>(string extension,
      out ILvcsCodecDescriptor? descriptor, out string? reason)
      where TValue : class;
}
```

**为什么需要 descriptor**：`Dictionary<Type, ILvcsCodec<,>>` 无法表达——泛型参数不同，不能放进同一个集合。`ILvcsCodecDescriptor` 是非泛型擦除层，内部把 `Load` 捕获为委托，返回 `object?`，由注册表按 `ValueType` 转型。Bevy 的 `ErasedAssetLoader` 解决的是同一个问题。

**不用反射的理由**：委托在注册时闭包生成，分派时零反射、零 `Activator`，AOT 与裁剪环境下也能工作。

### 6.3 解析规则

```text
TryResolve<TValue>()
  candidates = 所有 ValueType == TValue 的 descriptor
  ├─ 1 个 → 命中
  ├─ 0 个 → 失败 "No codec produces X."
  └─ 多个 → 失败 "N codecs produce X; specify an extension to disambiguate."

TryResolve<TValue>(ext)
  candidates = 类型匹配 且 Handles(ext)
  ├─ 1 个 → 命中
  ├─ 0 个 → 失败 "No codec producing X reads 'ext'."
  └─ 多个 → 失败
```

**类型优先，扩展名只做细分。** 这条规则来自 Bevy 的 `total_resolution` 测试，那里明确验证 "Type should override Extension"：调用方给了类型就以类型为准，扩展名只在同类型内筛选。

对本项目更实际的理由是：`ItemDefinition` 从 JSON 改成 YAML 时，产出的 `TValue` 不应变化。**类型是稳定信号，文件后缀是可替换的载体。**

### 6.4 重复注册一律拒绝

同一 `(ValueType, extension)` 注册第二次直接抛 `InvalidOperationException`：

```csharp
if (existing.Handles(extension))
{
  throw new InvalidOperationException(
      $"A codec producing {descriptor.ValueType.Name} already handles '{extension}'. " +
      "Ambiguity is refused rather than resolved by registration order.");
}
```

**这是对 Bevy 的有意背离。** Bevy 的 `get_by_type` / `get_by_extension` 都取 `.last()`，同类型多 loader 时后注册者静默胜出，只打一条 `warn!`。

本项目不能这样做——[项目文件组织设计约束](../../Context/架构设计/项目文件组织设计约束.md) §2.2 规定：

> 执行顺序都不得由目录结构、文件名排序、目录枚举或项目文件顺序隐式决定。

让「哪个 codec 生效」取决于装配代码的书写顺序，与这条同源问题。因此同名冲突在**注册期**失败，而不是**解析期**猜测。

### 6.5 顺序无关是可验证的

「拒绝 `.last()`」不是口号，有可执行证据：验证项目 `registry-order-*` 用两种相反的注册顺序建立两个注册表，断言：

- `TryResolve<ProbeResult>(".a")` 两次结果指向同一 Family；
- `TryResolve<ProbeResult>(".b")` 同理；
- `TryResolve<ProbeResult>()` 在两种顺序下**都失败**。

### 6.6 明确不吸收的三种机制

| 机制 | 出处 | 不吸收的理由 |
| --- | --- | --- |
| `.last()` 静默胜出 | Bevy `get_by_extension` | 顺序隐式决定结果，违反约束 §2.2 |
| `MAX_LOADERS = 64` 定长上限 | Godot `resource_loader.h` | 引擎的内存布局取舍，本项目无此约束 |
| `p_at_front` 插队改变优先级 | Godot `add_resource_format_loader` | 顺序敏感性；若需优先级应显式声明字段并在冲突时报错 |
| `reserve` + 阻塞等待 | Bevy `MaybeAssetLoader::Pending` | 那是为「插件后到达」设计；本项目启动期同步加载，族未注册应直接报错而非挂起 |

### 6.7 注册表是可选设施

`Load(codec, source)` 保留不变。注册表只是「不想在装配代码里写死 codec 名」时的便利层。因此引入分派**不破坏**已有的调用点与验证用例。

## 7. 诊断

```csharp
public readonly record struct LvcsDiagnostic
{
  public LvcsDiagnosticSeverity Severity { get; }   // Warning | Error
  public LvcsFamilyId Family { get; }
  public string? Id { get; }                        // 条目 id；族级为 null
  public string? Field { get; }                     // 字段名；非字段级为 null
  public string Message { get; }
  public string? Source { get; }                    // 来源文本；未知为 null
}
```

`ToString()` 输出：

```text
Error: items.armor.json [Torch].UseAmmo: Entry consumes ammunition but useAmmo is None.
```

`LvcsDiagnosticSink` 提供 `Report`、`Error`、`Warning` 与 `HasErrors`。`HasErrors` 遍历判断而非维护计数：诊断条目数量级在百以内，可读性优先。

## 8. 加载结果

```csharp
public sealed class LvcsLoadResult<TValue>
    where TValue : class
{
  public TValue? Value { get; }                     // 成功时非 null
  public IReadOnlyList<LvcsDiagnostic> Diagnostics { get; }
  public bool Succeeded => Value is not null;
}
```

**内容错误不进异常**：一次加载可能发现几十个问题，逐条抛会丢失前面所有条目。宿主拿到 `Failure` 后打印全部诊断。

## 9. 加载管线

`LvcsLoader.Load` 的执行顺序：

```text
1 校验族一致       source.Family 与 codec.Family 不等 → ArgumentException
2 逐单元 Parse     记录诊断数快照；捕获 LvcsContentException 与其他 Exception
3 空结果检查       单元产出 0 条 → 记错误
4 失败短路         任一单元被拒或已有 Error → Failure，不进入投影
5 Project          整批投影；同样捕获两类异常
6 结果             Diagnostics 有 Error → Failure；否则 Success
```

### 9.1 异常转换是加载器的唯一职责

| 捕获 | 处理 |
| --- | --- |
| `LvcsContentException` 且 codec 未写诊断 | 用异常消息补一条错误诊断 |
| `LvcsContentException` 且 codec 已写诊断 | 沿用 codec 已写的 |
| 其他 `Exception` | 转诊断，消息前缀保留**异常类型名** |
| 正常返回 | 继续 |

保留类型名的理由：`NullReferenceException` 一眼看出是代码缺陷，不是内容写错。这条把两类失败在诊断里区分开。

"codec 未写诊断"靠**捕获前的诊断数快照**判断——这个细节是验证阶段发现并修掉的缺陷：最初实现假定"codec 抛异常前一定写过诊断"，导致直接抛外部异常的 codec 产生零诊断的失败，静默丢信息。

### 9.2 为什么失败后不继续投影

在有错误的内容上投影会构造领域对象、执行构造函数校验，产生二次异常并掩盖原始诊断。所以 `Validate` 等价职责一律在 `Parse`/`Project` 内部完成，加载器只做单向短路。

## 10. 四类数据如何映射到同一契约

| 数据形状 | `TRaw` | `TValue` | codec 要点 |
| --- | --- | --- | --- |
| 标量记录 | 可变 DTO（可空字段） | 按 id 索引的不可变目录 | `Parse` 查单条规则；`Project` 查重复与跨条目引用 |
| 稀疏映射 | `KeyValuePair<string, T>` 列表 | 强类型键字典 | `Parse` 验 key 可解析；`Project` 转键 |
| 关系树 | 递归 DTO | 含子规则的目录 | `Project` 做引用完整性与环检测 |
| 多对多 | 配方 DTO 列表 | 含双向索引的目录 | `Project` 建"产物→配方""材料→配方"两张索引 |
| 单文件配置 | 单 DTO | 单对象 | `Project` 直接返回 |

`TValue` 是开放类型，加载器不知道也不关心它是字典、树还是索引。**这是"抽象 API 只管把字节变成有类型的值"的具体体现。**

### 10.1 已实现的族

| 族 | codec | 条目来源 | 规模 |
| --- | --- | --- | --- |
| `itemUse` | `ItemUseCodec` | `Content/Prototypes/ItemUse/item-use.json` | 3 条（当前夹具） |

## 11. 已实现的验证

验证项目：`Test/NSSLC.Server/Terraria.Server.Lvcs/`，**49 项断言全部通过**。

| 前缀 | 覆盖 | 项数 |
| --- | --- | --- |
| `happy-*` | 正常批次加载、投影、冻结、按 id 查询、无错误诊断 | 5 |
| `invalid-*` | 坏 JSON 转诊断而非异常 | 2 |
| `duplicate-*` | 重复 id 在投影期被发现，诊断带 id 与字段 | 2 |
| `ammo-*` | `consumeAmmo=true` 但 `useAmmo=None` 被拒 | 2 |
| `mismatch-*` | 来源族与 codec 族不一致时抛 `ArgumentException` | 1 |
| `diag-*` | 诊断携带族、来源、消息 | 3 |
| `foreign-*` | 非内容异常仍转诊断，且保留类型名 | 2 |
| `fill-*` | 填充式外部加载器直通，成功与失败两条路径 | 4 |
| `real-*` | 真实文件经 `FileLvcsSource` 加载 | 3 |
| `registry-resolve-*` | 唯一命中、类型与 Family 正确、无 codec 时的失败原因 | 6 |
| `registry-duplicate-*` | 重复注册抛异常、`Count` 不变 | 2 |
| `registry-ext-*` | 扩展名命中、不匹配失败、原因含扩展名 | 3 |
| `registry-ambiguous-*` | 同类型两 codec 歧义、指定扩展名后可区分、Family 正确 | 4 |
| `registry-order-*` | **顺序无关**：两种相反注册顺序结果一致，歧义两边都失败 | 5 |
| `dispatch-*` | 端到端经分派加载真实文件：解析命中、类型正确、已 Seal、无错误诊断 | 5 |

### 11.2 真实服务器启动验证

除单元断言外，还用真实世界做了两次启动对照（世界文件 `src/World/科研.wld`，8400×2400）：

| 场景 | 操作 | 结果 |
| --- | --- | --- |
| 内容正确 | 原始 `item-use.json`（3 条） | `ecs.server.ready listen=127.0.0.1:7777`，stderr 为空 |
| 内容损坏 | 注入重复 `id: Torch` | 启动失败，诊断 `Error: itemUse [Torch].Id: Item use entry is declared twice.`，抛于 `LoadItemUseCatalog` |

**这两次对照证明 LVCS 装配在真实启动路径中执行**，而不是只通过单元测试。第一次证明正常路径可达 ready，第二次证明内容错误会以领域诊断的形式终止启动，且诊断含族、条目 id、字段名与消息。

### 11.3 验证过程中的三个真实缺陷

| 缺陷 | 现象 | 修法 |
| --- | --- | --- |
| `RequiresAmmo` 用作守卫条件 | 定义为 `UseAmmo != None && ConsumeAmmo`，当 `UseAmmo == None` 时恒为 false，弹药校验永不触发 | 改为独立条件 `prototype.ConsumeAmmo && useAmmo == ItemId.None` |
| 加载器假定"codec 抛前已写诊断" | 直接抛 `LvcsContentException` 的 codec 产生零诊断失败 | 捕获前记录诊断数，未新增则用异常消息补一条 |
| 歧义用例注册了不同 `TValue` 的 codec | `ItemUseCodec` 产出 `ItemUseDefinitionCatalog`、`ProbeCodec` 产出 `ProbeResult`，候选集各 1 个，根本不构成歧义 | 改为注册两个都产出 `ProbeResult` 的 codec |

第二条是设计假设错误，不是笔误：**契约允许 codec 不写诊断就抛**，加载器必须兜底。

第三条是验证自身的错误：歧义的前提是"同一 `TValue` 有多个 codec"，用不同产出类型去构造歧义，测的是别的东西。

### 11.4 一个踩到的构建陷阱

`NSSLC.Server.csproj` 用 `<None Update="Content\Prototypes\**\*.json" CopyToOutputDirectory="PreserveNewest" />`。故障注入后恢复源文件时，**输出目录的副本比源文件新，`PreserveNewest` 不会覆盖**，导致：

- 源文件已恢复（3 条）
- 输出目录仍是坏内容（2 条）
- 服务器继续报重复 id

排查时容易误判为"代码没生效"。删掉输出副本再构建即恢复。这条记在这里，避免下次踩同一个坑。

## 12. 三条硬约束

1. **`Terraria.Lvcs` 不引用 `System.IO`、`System.Text.Json`、`System.Text.Encoding`。** 当前 12 个契约文件均为纯类型与接口。
2. **`ILvcsSource` 只由基础设施实现。** 领域层只持接口。
3. **加载结果不得出现在可变形态。** `LvcsLoadResult<TValue>.Value` 由 codec 保证不可变（`ItemUseDefinitionCatalog` 在 `Project` 末尾 `Seal()`）。

第 1 条现在多了一层含义：**分派也必须无 I/O**。`ILvcsCodecRegistry` 是纯内存表，`ILvcsCodecDescriptor.Load` 只是委托调用，不含任何文件访问。

## 13. 与现有代码的关系

| 原类型 | 处理 |
| --- | --- |
| `PrototypeManager` | **删除**，职责拆为 `FileLvcsSource`（枚举）+ codec（解析） |
| `PrototypeAttribute` / `IPrototype` / `IdDataFieldAttribute` / `DataFieldAttribute` | **删除**，族身份改为 `LvcsFamilyId` 常量 |
| `ItemUseDefinitionPrototype` | **删除**，由 `ItemUsePrototype` 取代 |
| `ItemUseDefinitionPrototypeLoader` | **删除**，由 `ItemUseCodec` 取代 |
| `ItemUseDefinitionCatalog` | 保留。它是不变目录，`Project` 末尾 `Seal()` |
| `ItemDefinitionRegistry` | 保留。未纳入本轮，见 §13 |
| `Program.LoadItemUseCatalog()` | 新增，用 LVCS 加载；失败时打印全部诊断并抛 `InvalidDataException` |

删除后 `Content/Prototypes/` 只剩 `ItemUse/item-use.json` 一个资产文件。

## 14. 未纳入本轮的范围

| 项 | 原因 |
| --- | --- |
| `ItemDefinition` 族 | `ItemAttributeModifierDefinition` 的替换设计仍在 proposals 阶段，需先定 `AttributeContribution` 合并方式 |
| NPC / 射弹 / 配方族 | 形状已普查（见[元数据普查](../research/2026-11-23-client-metadata-survey.md)），但字段集未定稿 |
| 多来源叠加 | 当前一族一来源。`LvcsMountOrder` 曾设计，因无需求已删除；需要时再加 |
| 结果缓存 / 增量重载 | 无热重载需求（约束明确排除） |
| 按扩展名自动发现族 | 分派要求调用方给目标类型；「丢一个目录进去自动识别是哪个族」未实现，理由是目录与族已一一绑定，扩展名是更弱的信号 |
| 运行时替换 codec | 注册表只在装配期写；无 `Unregister`。改动 codec 集合需要重建注册表 |

## 15. 完成定义

首批（ItemUse）已完成：

1. `Terraria.Lvcs` 契约落地且不引用 `System.IO` ✓
2. `ItemUse` 族经 `ILvcsCodec` 加载并 `Seal()` ✓
3. 诊断可定位到"族 + 条目 + 字段 + 来源" ✓
4. 分派层落地：按类型解析、扩展名细分、冲突拒绝注册、顺序无关 ✓
5. 验证项目 **49 项**全绿，含真实文件路径、25 项分派用例，并有真实服务器启动对照 ✓
6. `NSSLC.Infrastructure.Lvcs` 与验证项目各 0 警告 0 错误；`Terraria.Inventory`、`Terraria.ItemUse` 回归通过 ✓

**未完成项**：`Terraria.Components` 与 `NSSLC.Server` 当前因外部改动 `src/NSSLC/Share/Entity/System/EcsServer.cs` 调用未定义的 `ValidateCoordinationRules` 而无法完整构建。该文件不属本轮改动（`git diff` 显示外部新增 149 行），LVCS 代码对零引用。按「保留无关修改」的约束未修复。

后续族接入时须重复第 2、3、5、6 条；分派层（第 4 条）只需实现一次。
