# WorldProgressionAndUnlocks 实际代码组件草案

## 1. 草案元数据

```yaml
documentType: actual-code-component-draft
draftId: WPU.CODE-DRAFT.Component.2026-09-06
subsystemId: WorldProgressionAndUnlocks
taskNumber: 07
sourceDesign: D:\TRbackup\NLTX\docs\design\2026-09-06-version4-world-progression-and-unlocks-component-design.md
sourceReport: D:\TRbackup\NLTX\docs\第一轮审查\2026-09-05-version4-world-progression-and-unlocks-public-decomposition.md
outputDraftPath: D:\TRbackup\NLTX\docs\design\2026-09-06-version4-world-progression-and-unlocks-code-component-draft.md
draftScope: component-code-only
draftStatus: implemented-slice
implementationStatus: implemented
compileStatus: passed
verificationStatus: not-run
componentCount: 2
typeBindingStatus: partial
ownerStatus: decision-required
```

本文件把上一份 Component-only Design 转译为实际 `.cs` 文件的代码草案。代码块记录字段容器、命名空间和默认初始化；对应组件文件与项目文件已经按本草案创建，并已完成组件项目编译。

本草案刻意不实现事实写入行为、持久化格式、网络编解码、派生解锁计算、UI 表现或外部副作用。这样可以先评审组件的字段形状，再单独裁决尚未确定的共享类型和项目归属。

## 2. 草案范围与设计前提

### 2.1 本次草案包含

- `ProgressionAggregate` 的实际 C# 类型草案；
- `BestiaryDiscoveryCache` 的实际 C# 类型草案；
- 每个字段的当前 NLTX 类型绑定、默认值和不变量；
- 候选文件路径、命名空间和依赖边界；
- World entity 上的组合关系；
- 为保持可落地性而采用的临时类型绑定，以及必须在实现前重新裁决的 owner。

### 2.2 本次草案不包含

- 任何 `.cs` 或 `.csproj` 文件的创建或修改；
- 事实登记、击杀计数、目击判断、交谈登记等运行时行为；
- 持久化 reader/writer、WorldFile section、网络 packet 或客户端镜像；
- 派生 `BestiaryEntryUnlockState`、完成度或 NPC 资格结果；
- 调度、主循环、运行时接线、测试实现和迁移步骤。

### 2.3 代码草案的保守绑定

上一份设计使用了 `PersistentBestiaryId`、`NetworkId` 和 `Rectangle` 作为语义候选，但当前 NLTX 尚未确认这三个共享类型的最终 owner。因此本草案采用已有代码中最接近且可检查的表示：

| 语义 | 草案绑定 | 依据 | 状态 |
|---|---|---|---|
| 持久 Bestiary 身份 | `string` | Version4 tracker 使用 string key；`ContentIdentityCatalog` 以 string 保存 Bestiary credit ID | 临时兼容表示；最终 `PersistentBestiaryId` owner 未决 |
| NPC 会话网络身份 | `NpcNetId` | `src/Npc/NpcNetId.cs` 已存在；协议当前以 NPC network ID 表达 | 候选共享类型；通用 `NetworkId` owner 未决 |
| 玩家扫描边界 | `EntityHitbox` | `src/Share/Entity/Queries/EntityHitbox.cs` 已存在，表达整数矩形边界 | 候选几何值类型；是否等价于 Version4 `Rectangle` 未决 |

这些绑定不是对未决 owner 的最终裁决。若整合审查确认新的共享值类型，应该替换字段类型，而不是在两个 Component 内复制一份同义值类型。

## 3. 目标文件与代码组织

小型责任域保持扁平目录，不提前创建 `Components/` 子目录。候选文件如下：

| 候选文件 | 核心公开类型 | 命名空间 | 状态 |
|---|---|---|---|
| `src/WorldProgressionAndUnlocks/ProgressionAggregate.cs` | `ProgressionAggregate` | `Terraria.WorldProgressionAndUnlocks` | implemented；本次已创建 |
| `src/WorldProgressionAndUnlocks/BestiaryDiscoveryCache.cs` | `BestiaryDiscoveryCache` | `Terraria.WorldProgressionAndUnlocks` | implemented；本次已创建 |

### 3.1 项目边界

当前仓库原先没有已确认的 `Terraria.WorldProgressionAndUnlocks.csproj`。本次实现按领域优先原则新增了 `src/WorldProgressionAndUnlocks/Terraria.WorldProgressionAndUnlocks.csproj`，没有把两个 Component 偷挂到 `Terraria.WorldSession`、`Terraria.Content` 或 `dome` 项目。

如果最终采用上述命名空间和字段类型，代码项目至少需要能够引用：

- `System.Collections.Generic`；
- `Terraria.Npc` 中的 `NpcNetId`；
- `EntityEcs.Queries` 中的 `EntityHitbox`。

代码项目当前实际引用 `Terraria.Npc` 和 `Terraria.EntityEcs`；共享类型的最终 owner 仍属于 `BD-CODE-01`，不能因为本次项目已经编译就视为架构裁决完成。

## 4. `ProgressionAggregate.cs` 代码草案

候选路径：`src/WorldProgressionAndUnlocks/ProgressionAggregate.cs`

```csharp
using System;
using System.Collections.Generic;

namespace Terraria.WorldProgressionAndUnlocks;

public sealed class ProgressionAggregate
{
  public const int MaxKillCount = 999_999_999;

  public ProgressionAggregate()
  {
    KillCountsByPersistentId = new Dictionary<string, int>(StringComparer.Ordinal);
    SightedPersistentIds = new HashSet<string>(StringComparer.Ordinal);
    ChattedPersistentIds = new HashSet<string>(StringComparer.Ordinal);
  }

  public Dictionary<string, int> KillCountsByPersistentId { get; }

  public HashSet<string> SightedPersistentIds { get; }

  public HashSet<string> ChattedPersistentIds { get; }
}
```

### 4.1 代码形状说明

- 使用 `sealed class` 而不是 `struct`，因为组件拥有三个可变集合，避免 ECS 取值复制造成集合 owner 误解；
- 每个集合在默认构造时为空，且使用 `StringComparer.Ordinal`，与 `ContentIdentityCatalog` 的持久身份查找语义一致；
- 属性只有 getter，禁止替换集合实例，但当前草案仍暴露可变集合本身，以适配现有 `StatusEffectsComponent` 等数据组件的容器写法；
- `MaxKillCount` 是 Version4 `NPCKillsTracker.POSITIVE_KILL_COUNT_CAP` 的代码级常量表达；它不是一个新的运行时规则对象；
- 草案不添加 `RecordKill`、`MarkSighted`、`MarkChatted`、`Load`、`Save` 或网络方法。组件只提供状态容器，事实转移的行为边界不在本草案中；
- 草案不添加 `WorldId`、`EntityId`、`PlayerId`、`NpcNetId` 或内容目录引用，避免把不同 ID 类别或第二份内容身份表放进权威事实组件。

### 4.2 字段契约

| C# 字段 | 当前类型 | 默认值 | 状态分类 | 实现前必须保持的不变量 | 证据状态 |
|---|---|---|---|---|---|
| `KillCountsByPersistentId` | `Dictionary<string, int>` | 空 dictionary，Ordinal comparer | World 权威持久事实 | key 为有效且非空的持久 Bestiary 身份；count 在 `0..MaxKillCount`；同一 key 只有一个当前值 | partial；`PersistentBestiaryId` owner 未决 |
| `SightedPersistentIds` | `HashSet<string>` | 空 set，Ordinal comparer | World 权威持久事实 | membership 幂等；成员是有效且非空的持久 Bestiary 身份；缓存清理不能清除成员 | partial |
| `ChattedPersistentIds` | `HashSet<string>` | 空 set，Ordinal comparer | World 权威持久事实 | membership 幂等；成员是有效且非空的持久 Bestiary 身份；玩家离开不能清除成员 | partial |
| `MaxKillCount` | `const int` | `999999999` | 字段约束常量 | 不能被实例状态改变；不能把大于上限的值存入击杀字典 | confirmed for Version4 cap |

当前代码草案没有在集合容器上强行实现非法 key 和非法 count 的运行时拒绝/截断 API，因为那会引入事实写入行为并决定跨项目写入 owner。实现阶段必须在 owner 裁决后补齐一种一致策略：所有写入统一拒绝非法数据，或统一按 Version4 规则规范化；不能由不同调用方各自处理。

### 4.3 生命周期契约

1. World entity 创建时调用无参构造，三个集合均为空。
2. World 数据恢复完成后，三个集合表达当前 World 的 Bestiary 事实；恢复输入不直接改变字段类型。
3. 活动 World 生命周期内，集合是三类事实的唯一候选存储；不依赖单个 NPC 或 Player entity 存活。
4. World reset/unload 时，集合必须清空或随 World entity 一起销毁，不能泄漏到下一个 World。
5. 玩家加入只读取集合，不改变集合的字段组成。

以上是数据生命周期约束，不是本草案要加入组件的行为方法。

## 5. `BestiaryDiscoveryCache.cs` 代码草案

候选路径：`src/WorldProgressionAndUnlocks/BestiaryDiscoveryCache.cs`

```csharp
using System.Collections.Generic;
using EntityEcs.Queries;
using Terraria.Npc;

namespace Terraria.WorldProgressionAndUnlocks;

public sealed class BestiaryDiscoveryCache
{
  public List<EntityHitbox> PlayerBestiaryBounds { get; } = new();

  public List<NpcNetId> SeenNpcNetworkIds { get; } = new();
}
```

### 5.1 代码形状说明

- 使用 `sealed class`，因为两个列表是可重建的引用型扫描缓存，不应通过值拷贝表达生命周期；
- `PlayerBestiaryBounds` 对应 Version4 `_playerHitboxesForBestiary`，但使用当前 NLTX 已有的 `EntityHitbox` 作为候选几何值类型；
- `SeenNpcNetworkIds` 对应 Version4 `_wasSeenNearPlayerByNetId`，使用当前 NLTX 已有的 `NpcNetId` 保留 NPC 网络身份语义；
- 两个列表都默认为空，且不进入 `ProgressionAggregate`；
- 草案不添加 `Reset`、`Prepare`、`Scan`、`Add`、`Contains` 或序列化方法，避免把缓存行为和 Component 数据混在一起；
- `SeenNpcNetworkIds` 只能用于会话/扫描去重，不能作为 `ProgressionAggregate` 的持久 key，也不能当作 NPC entity ID。

### 5.2 字段契约

| C# 字段 | 当前类型 | 默认值 | 状态分类 | 实现前必须保持的不变量 | 证据状态 |
|---|---|---|---|---|---|
| `PlayerBestiaryBounds` | `List<EntityHitbox>` | 空 list | transient scan cache / geometry snapshot | 每次扫描上下文重建前允许清空；不进入 WorldFile；不代表已目击事实 | partial；`EntityHitbox` 与 `Rectangle` 语义仍需核对 |
| `SeenNpcNetworkIds` | `List<NpcNetId>` | 空 list | transient dedup cache / session identity | 只用于当前扫描上下文的去重；不进入 WorldFile；不替代 persistent identity | partial；通用 `NetworkId` owner 未决 |

### 5.3 生命周期契约

1. active World entity 建立目击扫描能力时，缓存可以用无参构造创建。
2. 每次扫描开始前，两个列表允许清空并依据当前扫描上下文重建。
3. World reset、unload 或扫描上下文失效时，缓存可以直接丢弃。
4. 缓存丢弃不得清除 `ProgressionAggregate.SightedPersistentIds`。
5. 缓存不附着到 Player 或 NPC entity；其范围是当前 World 的扫描上下文。

## 6. 两个 Component 的组合草案

| Entity/World 对象 | 必需 Component | 可选 Component | 互斥 Component | 组合理由 |
|---|---|---|---|---|
| active World entity | `ProgressionAggregate` | `BestiaryDiscoveryCache` | 无 | World 级持久事实与临时扫描缓存需要同一 World 范围，但生命周期和权威性不同，因此分为两个 Component |
| NPC entity | 无 | 无 | `ProgressionAggregate`、`BestiaryDiscoveryCache` | NPC 实例的 network ID 只是缓存输入；NPC 销毁/重建不能改变 World unlock facts |
| Player entity | 无 | 无 | `ProgressionAggregate`、`BestiaryDiscoveryCache` | 目击/交谈结果是 World 级 membership；Player bounds 只是扫描快照，不是 Player 持久组件 |

组合关系保持以下边界：

```text
active World entity
├── ProgressionAggregate       // persistent authority
└── BestiaryDiscoveryCache     // transient cache, optional
```

`ProgressionAggregate` 与 `BestiaryDiscoveryCache` 不能因为都服务于 Bestiary 就合并成单个大组件：前者需要跨加载周期保留，后者可以在扫描边界丢弃；前者的 key 是持久身份，后者的去重值是会话身份。

## 7. 依赖方向与状态所有权

### 7.1 `ProgressionAggregate`

| 依赖/消费者 | 允许关系 | 禁止关系 |
|---|---|---|
| 内容身份目录 | 只提供 Bestiary credit ID 到持久身份的输入 | 不把整个 `ContentIdentityCatalog` 复制为组件字段 |
| NPC/交互事实来源 | 未来可提供事实输入 | 不把 NPC 或 Player 实例引用存为持久字段 |
| World 生命周期 | 持有组件并管理其存在范围 | 不把 World lifecycle marker 混成三个事实集合的成员 |
| 外部持久化/网络边界 | 只在边界读取/恢复组件状态 | 不把 reader、writer、packet 或 wire bytes 存进组件 |
| UI/资格计算 | 只读派生输入 | 不把展示状态或完成度回写为事实字段 |

### 7.2 `BestiaryDiscoveryCache`

| 依赖/消费者 | 允许关系 | 禁止关系 |
|---|---|---|
| Player bounds 采集 | 提供本次扫描的几何快照 | 不把 Player entity 或 Player ID 存进缓存作为持久关系 |
| NPC 网络身份 | 提供会话内去重值 | 不把 `NpcNetId` 升格为持久 Bestiary 身份 |
| `ProgressionAggregate` | 扫描结果最终对应持久事实，但两者存储边界分离 | 不用清空缓存代替清空/修改权威事实 |
| World 生命周期 | 决定缓存何时可丢弃 | 不要求缓存跨 WorldFile 恢复 |

## 8. 从 Component-only 设计到代码草案的差异

上一份设计中的语义候选与本代码草案的具体绑定如下：

| Component-only 语义候选 | 代码草案绑定 | 变化原因 | 是否已最终锁定 |
|---|---|---|---|
| `Dictionary<PersistentBestiaryId, int>` | `Dictionary<string, int>` | 当前 NLTX 只有 string Bestiary identity 证据 | 否；等待 `BD-CODE-02` |
| `HashSet<PersistentBestiaryId>` | `HashSet<string>` | 同上 | 否；等待 `BD-CODE-02` |
| `List<Rectangle>` | `List<EntityHitbox>` | 当前 NLTX 已有整数 hitbox 值类型，避免引入新几何类型 | 否；等待 `BD-CODE-03` |
| `List<NetworkId>` | `List<NpcNetId>` | 当前 NLTX 已有 NPC network ID 值类型 | 否；等待 `BD-CODE-04` |
| `ProgressionAggregate` | `sealed class` | 三个可变集合需要引用型组件语义 | 草案选择；实现仍需编译验证 |
| `BestiaryDiscoveryCache` | `sealed class` | 两个可重建列表需要引用型缓存语义 | 草案选择；实现仍需编译验证 |

代码草案的具体类型绑定比 Component-only 设计更接近当前仓库，但它不减少证据缺口；它只是让后续评审可以针对真实 C# 签名讨论替换成本。

## 9. 未决代码级 owner 与决策点

### `BD-CODE-01`：项目与命名空间 owner

- 冲突范围：`src/WorldProgressionAndUnlocks/` 是否成为独立项目，还是归入已有 `Terraria.WorldSession`。
- 当前候选：`WorldProgressionAndUnlocks` 独立领域目录与 `Terraria.WorldProgressionAndUnlocks` 命名空间。
- 若归入独立项目：需要新增项目边界和引用关系。
- 若归入 `WorldSession`：需要保持既有 World 组件命名空间一致，并重新评估目录归属。
- 当前不能宣布最终 owner，因为仓库没有目标项目的既有入口，且 `WorldSession` 是相邻但不等同的责任域。

### `BD-CODE-02`：`string` 与 `PersistentBestiaryId`

- 冲突字段：`KillCountsByPersistentId`、`SightedPersistentIds`、`ChattedPersistentIds`。
- 当前候选：暂时使用 `string`，最大程度保留 Version4 和 `ContentIdentityCatalog` 的事实表示。
- 若改用 `PersistentBestiaryId`：三个字段获得类型级身份约束，但需要确认共享值类型 owner 和跨项目引用。
- 当前不能宣布最终 owner，因为现有 NLTX 只有字符串 identity map，没有冻结的 Bestiary-specific value type。

### `BD-CODE-03`：`EntityHitbox` 与 `Rectangle`

- 冲突字段：`PlayerBestiaryBounds`。
- 当前候选：使用现有 `EntityHitbox`，避免在 WorldProgressionAndUnlocks 内新建重复矩形类型。
- 若保留 `Rectangle`：需要确认 Terraria 兼容边界类型可被当前领域安全引用。
- 若采用其他共享几何类型：会改变组件项目依赖和字段签名。
- 当前不能宣布最终 owner，因为现有 `EntityHitbox` 的语义与 Version4 `Rectangle` 的扫描边界语义尚未完成逐字段核对。

### `BD-CODE-04`：`NpcNetId` 与通用 `NetworkId`

- 冲突字段：`SeenNpcNetworkIds`。
- 当前候选：使用已存在的 `NpcNetId`，保留 NPC 网络身份的显式类型。
- 若改用通用 `NetworkId`：需要说明 NPC 专属值是否转换为通用值，以及该类型由哪个共享领域拥有。
- 当前不能宣布最终 owner，因为协议 payload、NPC 实例身份和通用网络身份尚未形成单一共享契约。

### `BD-CODE-05`：集合暴露方式

- 冲突范围：三个权威集合和两个缓存列表是否继续以可变集合属性暴露。
- 当前候选：保留 getter-only 属性加可变容器，以贴合现有 NLTX 数据组件的最小形状。
- 若收紧为只读视图：需要提供同程序集恢复/更新边界，或采用组件替换；这会改变跨项目访问方式。
- 若增加组件方法：会把事实转移规则放入 Component，扩大组件职责。
- 当前不能宣布最终 owner，因为写者所在项目、ECS storage API 和持久化恢复边界尚未锁定。

## 10. 代码草案的明确排除项

以下对象不应被追加到这两个 `.cs` 文件中：

- `BestiaryEntryUnlockState`、`BestiaryUnlockProgressReport` 和完成度百分比；
- `ContentIdentityCatalog`、`ContentPresentationIndex` 或完整内容条目对象；
- `WorldProgressionState` 的 Hardmode、Boss、事件或入侵字段；
- `NetBestiaryModule`、`BestiaryModulePacket`、`BinaryReader`、`BinaryWriter` 或 WLD record；
- `EntityReference`、Player ID、NPC entity ID、NPC slot 或 WorldFile section ID；
- Achievement、Social、UI icon、portrait、drops 和日志状态；
- 事实登记、校验/修复流程、网络同步、持久化读写和扫描算法方法；
- 为“以后使用”预留的 revision、schema、timestamp、dirty flag 或客户端镜像字段。

这些排除项不是遗漏，而是为了保持 Component 只拥有其声明的 World 事实或短生命周期缓存。

## 11. 代码落地前检查清单

以下清单只用于审查该代码草案是否可以继续扩展，不表示本次已经实现完整子系统：

- [ ] `WorldProgressionAndUnlocks` 的项目 owner 已裁决；
- [ ] `PersistentBestiaryId` 或 `string` 的最终字段类型已裁决；
- [ ] `EntityHitbox` 与 Version4 `Rectangle` 的字段语义已逐项确认；
- [ ] `NpcNetId` 与通用 `NetworkId` 的共享 owner 已裁决；
- [ ] 权威集合的写入封装方式已裁决，且不会产生第二个事实写者；
- [ ] World entity attachment owner 已裁决；
- [ ] 兼容输入与权威 Component 之间的边界已保持单向；
- [x] 实际 `.cs` 文件已创建，并已针对受影响组件项目执行串行编译；
- [ ] 不创建测试项目；后续若需要验证，应由用户单独授权测试范围；
- [x] 编译结果已记录；本次不宣称完整子系统行为已验证。

## 12. 当前状态与验证记录

| 项目 | 当前状态 | 证据 |
|---|---|---|
| `ProgressionAggregate.cs` | 已创建；实现三个 World 级事实集合 | `src/WorldProgressionAndUnlocks/ProgressionAggregate.cs` |
| `BestiaryDiscoveryCache.cs` | 已创建；实现两个临时扫描缓存列表 | `src/WorldProgressionAndUnlocks/BestiaryDiscoveryCache.cs` |
| 目标 `.csproj` | 已创建；引用 `Terraria.Npc` 和 `Terraria.EntityEcs` | `src/WorldProgressionAndUnlocks/Terraria.WorldProgressionAndUnlocks.csproj` |
| C# 编译 | 通过；0 个警告、0 个错误 | 串行 `build`，退出码 0 |
| 测试/验证程序 | 未创建；按用户要求不写测试 | 本次只执行组件项目编译 |
| Version4 行为等价 | 未声明 | 本次只实现数据组件，不实现完整 Version4 行为 |
| NLTX 运行时接线 | 未实现 | 组件已创建，但尚无 World attachment、持久化或网络接线 |

### 12.1 本次实际验证命令

构建命令：

```text
pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 build .\src\WorldProgressionAndUnlocks\Terraria.WorldProgressionAndUnlocks.csproj -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false --no-restore
```

构建结果：退出码 `0`；`0` 个警告；`0` 个错误；产物位于：
`Build/bin/Terraria.WorldProgressionAndUnlocks/Debug/net10.0/Terraria.WorldProgressionAndUnlocks.dll`。

本次没有创建或运行测试项目；按用户要求只保留组件项目的编译结果。

## 13. 最终声明

本文件是实际代码组件草案，并记录了本次两个数据组件切片的落地状态；它不等同于完整的 WorldProgressionAndUnlocks 子系统实现。

本次已经创建 `ProgressionAggregate.cs`、`BestiaryDiscoveryCache.cs` 和对应的 `Terraria.WorldProgressionAndUnlocks.csproj`；尚未创建组件注册、World attachment、持久化接线、网络接线或运行时事实写入行为。

两个组件的字段容器实现已经通过本次组件项目编译，但没有测试结论；`typeBindingStatus: partial` 和 `ownerStatus: decision-required` 仍然有效。

本草案不声明 Version4 行为等价、完整 API 兼容、持久化兼容或网络兼容；编译结论仅限于本次新增的组件项目。

在 `BD-CODE-01` 至 `BD-CODE-05` 完成裁决前，代码中的 `string`、`NpcNetId` 和 `EntityHitbox` 只能作为当前 NLTX 的临时绑定，不能视为最终共享类型 owner。
