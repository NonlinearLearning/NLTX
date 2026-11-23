# 外部项目元加载技术调研：SS14 / Terasology / Veloren / Valence

文档 ID：DOC-2026-11-23-EXTERNAL-META-LOADING-RESEARCH  
逻辑域：research  
产物类型：evidence  
状态：proposal；源码阅读与文件证据，未运行任何外部项目，未修改本项目业务源码  
日期：2026-11-23（Asia/Shanghai）  
参考源码根：`C:\Users\shan\Downloads\ECS`（四个下载快照，无可靠 Git 提交）

## 1. 结论

四个项目都用"**数据文件 + 类型元数据 + 注册表**"三件套加载内容定义，但三件的分工差异很大：

| 项目 | 数据载体 | 类型元数据来源 | 注册表与装配方式 |
| --- | --- | --- | --- |
| Space Station 14 | YAML 原型（`Resources/Prototypes/**/*.yml`，2112 个文件） | 特性（`[Prototype]`、`[IdDataField]`、`[DataField]`、`[ParentDataField]`、`[AbstractDataField]`）+ 源生成器序列化 | 反射枚举 `IPrototype` 的全部子类型注册 kind，YAML 先合成继承链再反序列化成实例，结果冻结为 `FrozenDictionary` |
| Terasology | JSON prefab（`*.prefab`，引擎内置 42 个） | 反射扫描组件类型（`environment.getSubtypesOf(Component.class)`），`ComponentMetadata` 记录字段 | Gestalt 资产系统按 `AssetType` + `AssetFormat` 注册解析器，`PrefabManager` 以 `ResourceUrn` 查表 |
| Veloren | RON 资产（`assets/**`，2854 个 `.ron`） | Rust `serde` 派生 + `Asset` trait 的 `load(cache, specifier)` | `assets_manager::AssetCache` 按点分 specifier 懒加载并缓存句柄；插件用 `MultiRON` 合并多份目录 |
| Valence | 无运行时内容文件 | 构建期读 `crates/valence_generated/extracted/*.json`（由 Java `extractor` 从 Minecraft 服务端导出） | `build.rs` 代码生成 `ItemKind` 等枚举；运行时注册表是 `Registry<I, V>`（`IndexMap` + 数值索引） |

对本项目最有价值的三个模式，按可借用程度排序：

1. **SS14 的"kind + 继承 + 抽象 + 冻结"四件事**：把"定义种类"与"某条定义实例"分离，实例可 `parent` 继承、`abstract` 只作模板，最终冻结成只读表。这正是"用加载系统取代手写 Definition 模型"的完整骨架。
2. **Terasology 的 delta/override 资产**：同名资产可叠加增量文件，解析器不需要知道覆盖者是谁；适合以后做补丁式内容包。
3. **Veloren 的 specifier 寻址 + 句柄缓存**：以点分路径直接寻址单个定义，不需要一张全局 `Dictionary` 就能按名取用；适合定义量大、按需加载的场景。

不推荐把 Valence 的构建期代码生成作为本项目的首选：它的前提是"内容来自外部权威二进制（Minecraft jar）且必须编译期定死枚举"，而本项目的内容是自有的、需要热改的物品表。

## 2. 证据边界

| 项 | 事实 | 影响 |
| --- | --- | --- |
| SS14 内容侧 | 本地 `space-station-14-master` 可读，`Resources/Prototypes` 共 2112 个 YAML | 内容写法与 kind 名可确认 |
| SS14 引擎侧 | 本地 `RobustToolbox/` 目录为空 | 引擎行为**不由本地源码证明**；本文引擎结论取自 GitHub `space-wizards/RobustToolbox` master 的 `PrototypeManager.cs`、`IPrototype.cs`、`DataFieldAttribute.cs` 三个文件，未固定提交 SHA |
| Terasology | `engine/src/main/java` 有 1938 个 Java 文件，`modules/` 为空 | 可用 `EnvironmentSwitchHandler`、`PrefabFormat`、`PrefabDeltaFormat`、`ComponentLibrary`、`ComponentMetadata`、`ModuleManager` 证明资产与组件注册路径；具体玩法模块缺失 |
| Veloren | `common/assets/src/lib.rs` 完整可读，`assets/**` 2854 个 `.ron` | 资产加载与热重载可确认 |
| Valence | `extractor/`（Java）与 `crates/valence_generated/build/*.rs` 可读 | 构建期生成路径可确认 |

本文不声称这些项目可编译或可运行；`confirmed` 只表示"所列文件里能直接读到的结构"。

## 3. Space Station 14：原型管理器

### 3.1 内容侧写法

`Resources/Prototypes/Entities/Clothing/Head/hats.yml` 的实写形态：

```yaml
- type: entity
  parent: ClothingHeadBase
  id: ClothingHeadHatBeret
  name: beret
  description: An artist's favorite headwear.
  components:
  - type: Sprite
    sprite: Clothing/Head/Hats/beret.rsi
  - type: Clothing
    sprite: Clothing/Head/Hats/beret.rsi
```

`Entities/Clothing/*.yml` 里还能看到纯模板条目：

```yaml
- type: entity
  abstract: true
  parent: BaseItem
  id: Clothing
  components:
  - type: Item
    size: Normal
```

三个字段构成元加载的全部输入：`type` 选择 kind（决定用哪个 C# 类型反序列化），`id` 是实例身份，`parent`/`abstract` 决定继承与是否可作为实例被索引。

### 3.2 类型侧元数据

`Content.Shared/Access/AccessLevelPrototype.cs` 是最小完整样本：

```csharp
[Prototype]
public sealed partial class AccessLevelPrototype : IPrototype
{
    [ViewVariables]
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public string? Name { get; set; }
}
```

`Content.Shared/Damage/Prototypes/DamageGroupPrototype.cs` 展示了带优先级的写法 `[Prototype(2)]`，以及 `[DataField(required: true)]`。

`IPrototype.cs`（引擎侧）把三个特性与三个语义绑定起来：

- `IdDataFieldAttribute`：字段名固定为 `id`，值为 `string?`，永远必填。
- `ParentDataFieldAttribute`：字段名固定为 `parent`，类型是 `string?` 或 `string[]?`，不必填。
- `AbstractDataFieldAttribute`：字段名固定为 `abstract`，类型是 `bool`，不必填。
- `IInheritingPrototype`：`string[]? Parents` 按数组顺序依次应用父数据；`bool Abstract` 的实例不进索引、不出现在枚举里。

`DataFieldAttribute` 的可用参数：`tag`（YAML 键名，缺省时取字段名首字母小写）、`readOnly`、`priority`（反序列化顺序，与序列化无关）、`required`、`serverOnly`、`customTypeSerializer`。

### 3.3 加载流水线

`PrototypeManager.cs` 的装配顺序是本文最重要的参照：

1. `ReloadPrototypeKinds()` 调 `_reflectionManager.GetAllChildren<IPrototype>()`，对每个类型执行 `RegisterKind`。
2. `RegisterKind` 用 `[Prototype]` 得到 kind 名；缺失则抛 `InvalidImplementationException`。同时校验：必须有且只有一个 `[IdDataField]`；若类型实现 `IInheritingPrototype`，必须有 `[ParentDataField]` 与 `[AbstractDataField]`；`id` 与 `parent` 不得落在同一成员上。
3. 读入的 YAML 先存为 `MappingDataNode`（`RawResults`），继承合成单独一轮：`PushCompositionWithGenericNode(kind, parent, rawChild)`，多父时传 `MappingDataNode[]` 并按顺序合成。
4. 合成结果再 `ISerializationManager.Read(kind, mapping, ...)` 反序列化成实例；`abstract: true` 的条目在这一步之前就被 `TryReadPrototype` 直接跳过。
5. 每个 kind 的实例表 `FrozenDictionary` 冻结，变更后通过 `PrototypesReloaded` 事件广播。
6. `ValidateDirectory`（`Content.YAMLLinter/Program.cs` 使用）与 `ValidateStaticFields` 提供离线校验入口。

继承图是显式的多根图：`EnumerateParents` 用队列做广度遍历，允许一个原型声明多个父；这与"只支持单继承"的常见原型系统不同。

### 3.4 值得注意的约束

- kind 名由 `[Prototype]` 特性或类型名计算（`PrototypeUtility.CalculatePrototypeName`），**不是**手写字符串常量表。
- 重复 kind、重复 `id` 都会在加载期报错，不会静默覆盖。
- 内容错误（YAML 结构、字段类型）与代码错误（缺特性、缺 `id` 字段）分属不同异常类型，前者可被独立的 linter 进程发现。

## 4. Terasology：Prefab 资产 + 反射组件库

### 4.1 prefab 数据形态

`engine/src/main/resources/org/terasology/engine/assets/prefabs/floatingCube.prefab`：

```json
{
  "skeletalMesh": { "mesh": "floatingCube", "heightOffset": 0.5, "loop": true },
  "persisted": false,
  "location": {},
  "Network": {},
  "Health": { "destroyEntityOnNoHealth": true, "currentHealth": 7 },
  "BoxShape": { "extents": [1.5, 1.5, 1.5] },
  "Trigger": { "detectGroups": ["engine:debris", "engine:sensor"] }
}
```

关键点：**顶层键就是组件名**，值是该组件的字段对象。没有 `components:` 数组、没有 `type:` 字段——组件身份直接由 JSON 键表达。

### 4.2 类型侧元数据

- 组件类型由模块环境反射枚举：`EnvironmentSwitchHandler.registerComponents` 遍历 `environment.getSubtypesOf(Component.class)`，跳过带 `@DoNotAutoRegister` 的、接口与抽象类，再用 `MetadataUtil.getComponentClassName` 生成注册名（`ResourceUrn(moduleName, componentName)`）。
- 字段元数据由 `ComponentLibrary extends ModuleClassLibrary<Component>` 构建，每个类型生成 `ComponentMetadata`；`ComponentMetadata` 从类注解读出 `persisted`（`@DoNotPersist` 取反）、`replicated`（`@Replicate`）、`@ForceBlockActive` 等语义。
- 字段拷贝策略由 `CopyStrategyLibrary` 提供——元数据不只描述"怎么读"，还描述"怎么复制"。

### 4.3 资产格式与 delta

`PrefabFormat extends AbstractAssetFileFormat<PrefabData>`，扩展名为 `prefab`；`load()` 用 `EntityDataJSONFormat.readPrefab` 读 JSON，再用 `PrefabSerializer(componentLibrary, typeHandlerLibrary)` 反序列化。缺 `name` 时以 `ResourceUrn` 补上。

`PrefabDeltaFormat extends AbstractAssetAlterationFileFormat<PrefabData>`，`apply()` 用 `PrefabSerializer.deserializeDeltaOnto(delta, assetData)` 把增量叠加到已加载资产上。

两者的注册在 `EnvironmentSwitchHandler.handleSwitchToGameEnvironment` 中成对出现：

```java
assetTypeManager.getAssetFileDataProducer(prefabAssetType).addAssetFormat(registeredPrefabFormat);
assetTypeManager.getAssetFileDataProducer(prefabAssetType).addDeltaFormat(registeredPrefabDeltaFormat);
```

注释说明了顺序约束：格式必须在 `ComponentLibrary` 存在之后、环境切换之前注册，否则解析器引用的是旧组件库。

### 4.4 规模线索

`PrefabManager` 接口只有四个方法：`getPrefab(name)`、`exists(name)`、`listPrefabs()`、`listPrefabs(Class<? extends Component>)`。最后一个是按"包含某组件"过滤——说明 prefab 在运行期被当作可查询的组件组合集合使用，而不只是数据表。

## 5. Veloren：RON 资产 + specifier 寻址

### 5.1 定义文件形态

`assets/common/items/weapons/axe/malachite_axe-0.ron`：

```ron
ItemDef(
    legacy_name: "Malachite Axe",
    legacy_description: "...",
    kind: Tool((
        kind: Axe,
        hands: Two,
        stats: (
            equip_time_secs: 0.25,
            power: 2.175,
            effect_power: 1.2,
            speed: 0.7,
            range: 1.0,
            energy_efficiency: 0.8,
            buff_strength: 1.2,
        ),
    )),
    quality: Epic,
    tags: [],
    ability_spec: None,
)
```

`assets/common/items/armor/adventurer/chest.ron` 展示复用写法：

```ron
ItemDef(
    legacy_name: "Adventurer Tunic",
    kind: Armor((kind: Chest, stats: FromSet("Adventurer"))),
    quality: Low,
    tags: [],
)
```

`FromSet("Adventurer")` 是"按集合名引用另一份数值表"，等价于本项目关心的"定义引用定义"，但引用的是数值集而非完整定义。

### 5.2 类型侧

`common/src/comp/inventory/item/mod.rs:787` 的 `ItemDef` 是运行时结构，`RawItemDef`（同文件 `:988`）是反序列化结构，带 `#[serde(rename = "ItemDef", deny_unknown_fields)]`：

- `deny_unknown_fields` 让拼错的字段直接报错，而不是被静默忽略——这是"内容错误必须在加载期暴露"的直接体现。
- `Asset for ItemDef` 的 `load()` 先 `cache.load::<Ron<_>>(specifier)` 读原始值，再补 `item_definition_id = specifier.replace('\\', ".")`，并显式拒绝 `veloren.core.` 前缀（保留给引擎函数）。
- 运行时结构比原始结构多出 `item_definition_id`，即**身份由寻址路径派生**，不在文件里重复写。

### 5.3 加载与重载

`common/assets/src/lib.rs`：

- `trait AssetExt: Asset` 提供 `load`、`load_cloned`、`load_expect`、`load_or_insert_with`；`load_expect` 在缺资产时 panic 并打印 id 与原因。
- 全局 `ASSETS: AssetCache` 由 `lazy_static` 持有；插件模式下换成 `plugin_cache::CombinedCache`，支持 `register_tar`。
- `AssetCombined` / `MultiRon<T>` 用 `Concatenate` 把多份来源的同一 specifier 合并（`HashMap` 取并集，后者覆盖前者）。
- `ReloadWatcher` 直接 re-export 自 `assets_manager`，客户端侧（`voxygen/src/render/renderer/mod.rs`、`voxygen/src/hud/item_imgs.rs` 等）用它做热重载。
- `ASSETS_PATH` 按 6 种部署形态依次探测（环境变量、可执行文件目录、仓库根、XDG 路径……），找不到就 panic 并打印搜索过的全部路径。

## 6. Valence：构建期生成 + 运行期索引注册表

- `extractor/src/main/java/rs/valence/extractor/extractors/Items.java` 从 Minecraft 服务端的 `DynamicRegistryManager` 导出 `items.json`：`id`、`name`、`translation_key`、`max_stack`、`max_durability`、`enchantability`、`fireproof`、`components`。
- `crates/valence_generated/build/item.rs` 在构建期 `include_str!("../extracted/items.json")`，用 `quote!` 生成 `ItemKind` 枚举与 `from_raw_id` / `to_raw_id` / `from_str` 三组匹配臂；`build/main.rs` 汇总 `attributes`、`block`、`item`、`sound`、`packet_id`、`chunk_view`、`status_effects` 七个生成器。
- 运行期 `crates/valence_registry/src/lib.rs` 的 `Registry<I, V>` 是 `IndexMap<Ident<String>, V>` + `PhantomData<I>`，`insert` 时检查 `I::MAX` 上限并返回数值索引；`RegistryPlugin` 在 `PostUpdate` 的 `RegistrySet` 重建 `RegistryCodec` 与 `TagsRegistry`。

这套做法把"内容"完全前移到构建期：运行期没有 YAML/RON 解析，也没有热重载，换来的是零解析开销与编译期类型安全。代价是内容改动必须重新构建。

## 7. 横向对比

| 维度 | SS14 | Terasology | Veloren | Valence |
| --- | --- | --- | --- | --- |
| 数据格式 | YAML | JSON | RON | JSON（仅构建期输入） |
| kind 如何确定 | `[Prototype]` 特性 | JSON 顶层键 + 反射组件名 | Rust 类型（`ItemDef`） | 代码生成的枚举 |
| 实例身份 | `id` 字段 | `ResourceUrn` | specifier（文件路径点分） | 数值 id + `Ident` |
| 继承 | 多父 + `abstract` | 组件级合并 + delta 覆盖 | `FromSet` 式引用（非定义继承） | 无 |
| 必填与未知字段 | `required` 元数据 + linter | `ComponentMetadata` 字段表 | `deny_unknown_fields` | 编译期 |
| 加载时机 | 启动 + 可重载 | 启动 + 环境切换重载 | 懒加载 + `ReloadWatcher` | 编译期 |
| 错误可见性 | 加载异常 + 独立 YAML linter | 资产加载异常 | panic（`load_expect`） | 编译错误 |
| 运行期形态 | 冻结字典 | AssetManager 缓存 | 全局 AssetCache 句柄 | `IndexMap` 注册表 |

## 8. 对本项目的启示

1. **kind 与实例必须分离。** SS14 的 `[Prototype]`+`IPrototype` 结构让"新增一类定义"只需新增一个类型，不需要改加载器；这直接对应本项目 `ItemDefinition`、`BuffDefinition`、`ItemUseDefinition` 各自手写 `Registry` 的重复。
2. **继承是内容作者的主要复用手段，但必须是显式的多父 + 抽象标记。** 单靠"同名字段覆盖"无法表达"基础物品 + 材质 + 品质"三层组合。
3. **未知字段必须报错。** Veloren 的 `deny_unknown_fields` 与本项目 `PrototypeManager` 现有 `JsonUnmappedMemberHandling.Disallow` 是同一策略，应保留。
4. **冻结要在加载结束时发生。** SS14 的 `FrozenDictionary` 与 Terasology 的"环境切换后不再注册"都说明：运行期不应再有注册写入。本项目 `ItemDefinitionRegistry` 当前没有 `Seal()`，与 `ItemUseDefinitionCatalog` 不一致。
5. **热重载是可选项，不是必需项。** 只有 Veloren 提供；SS14 通过 reload 事件支持但代价是冻结表重建。本项目当前是单进程服务器 + 客户端硬编码物品表，不需要在第一阶段引入。
6. **校验要能离线跑。** SS14 把 YAML linter 做成独立可执行项目，说明"内容错误"值得独立于服务器启动来检查。本项目的 `Test/` 独立控制台验证程序正好可以承担这个角色。

以上六点构成 [内容定义加载系统设计](../architecture/2026-11-23-content-definition-loading-system-design.md) 的直接输入。

本文只覆盖**外部开源项目**的元加载实现。参考客户端（`D:\TRbackup\最新的客户端`）把元数据写在代码里的五种形态、以及 Unreal / Unity / RimWorld / Minecraft / NeoForge 等商业与主流引擎的做法，见配套研究 [代码内元数据与业界元数据加载技术调研](2026-11-23-code-embedded-metadata-research.md)。
