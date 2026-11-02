# World 特性组件化清单（待确认后实施）

```text
文档 ID：DOC-2026-11-01-world-componentization-checklist
逻辑域：reviews
产物类型：design
状态：active
范围：src/NSSLC/Components/World/**（21 个 .cs）
依据：docs/reviews/audits/2026-11-01-world-componentization-options.md（候选评估）
canonical 路径：docs/reviews/audits/2026-11-01-world-componentization-checklist.md
```

本清单在候选评估基础上**收敛为确定项**，并撤销了一个候选（原 C4）。实施前需你确认。

## 0. 一句话概括

新增 **3 个 World 内部组件**，把 `WorldRuntime` 里的液体状态、变更缓冲、时钟分离出来；
对外只保留 **4 个端口接口**；用 `internal` + 守卫脚本 + 白名单三道手段禁止外部使用。

## 1. 要新增的组件

### W1 `WorldMutationBufferComponent`（低风险）

| 项 | 内容 |
| --- | --- |
| 文件 | `src/NSSLC/Components/World/WorldMutationBufferComponent.cs` |
| 种类 | `internal sealed class` |
| 私有状态 | `Dictionary<WorldTileCoordinate, WorldMutation> _pending` |
| 内部方法 | `void Record(WorldMutation mutation)`（按坐标合并，保留最新）<br>`WorldMutation[] Drain()`（取走并清空）<br>`bool IsEmpty { get; }` |
| 来源 | `WorldRuntime.cs:26`（字段）、`354-368`（`DrainPendingMutations`）、`375-404`（`RecordMutationUnsafe`） |
| 外部如何用 | **不直接持有该类型**，改为依赖新增端口 `IWorldMutationSource` |

### W2 `WorldLiquidStateComponent`（中风险）

| 项 | 内容 |
| --- | --- |
| 文件 | `src/NSSLC/Components/World/WorldLiquidStateComponent.cs` |
| 种类 | `internal sealed class` |
| 私有状态 | `LiquidSimulation _simulation`<br>`LiquidTileStorageBinding _storage` |
| 内部方法 | `bool HasQueuedWork { get; }`<br>`int Advance(int budget)`<br>`void EnqueueNeighbors(WorldTileCoordinate coordinate, bool urgent)` |
| 来源 | `WorldRuntime.cs:27-28`（字段）、`310-348`（`AdvanceLiquidSimulation`）、`351`（`HasQueuedLiquid`）、`370-373`（`EnqueueLiquidNeighborsUnsafe`）、`54-55`（构造） |
| 外部如何用 | 无需端口。液体推进只由 `WorldRuntime` 自身驱动 |

### W3 `WorldClockComponent`（可选，默认**不做**）

| 项 | 内容 |
| --- | --- |
| 文件 | `src/NSSLC/Components/World/WorldClockComponent.cs` |
| 种类 | `internal sealed class` |
| 私有状态 | `LastTick`、`TotalTimeSeconds`、`TimeOfDaySeconds`、`DayTime`、`DayLengthSeconds` |
| **不搬走** | `Revision` 留在 `WorldRuntimeStateComponent` |
| 来源 | `WorldRuntimeStateComponent.cs:60-61`、`74-82`、`111-128` |
| 为什么不建议做 | `Revision` 同时被 3 个方法修改并供给物理域（`SpatialWorldRevision`）；拆时钟若误搬 `Revision` 会破坏版本契约 |

## 2. 明确撤销的候选：掉落物存储

原候选 C4「`WorldItemStore` → `WorldItemStoreComponent`」**撤销**。实证：

| 证据 | 位置 |
| --- | --- |
| 跨程序集创建并注入 | `src/NSSLC.Server/Program.cs:36` `WorldItemStore worldItems = new();` |
| 同程序集消费者 | `Communication/WorldNetworkProjectionSystem.cs:23,31` |
| 是共享服务而非 World 内部特性 | 承载掉落物生命周期、投影、事务，被网络投影与快照读取 |

把它 internal 化会让服务端入口直接编译失败。它不是"World 的特性"，是独立的掉落物存储。

## 3. 禁止外部使用的手段（三道）

### 3.1 第一道：类型 `internal`

| 组件 | 可见性 | 实际挡住什么 |
| --- | --- | --- |
| `WorldMutationBufferComponent` | `internal` | 挡住 `NSSLC.Server`、`MinimalImplementation` 等跨程序集引用 |
| `WorldLiquidStateComponent` | `internal` | 同上 |
| `WorldRuntimeStateComponent` | **`public` → `internal`**（改动） | 同上；需先完成 §4 的 L2 |
| `WorldTileStorageComponent` | **`public` → `internal`**（改动） | 同上 |

**必须承认的局限**：这三道里的第一道**挡不住同程序集**（`Communication/`、`Share/Physics/Liquid/`）。
这是 `internal` 的语义边界，不是实现缺陷。

### 3.2 第二道：对外只暴露端口

| 端口 | 状态 | 消费者 |
| --- | --- | --- |
| `IWorldInteractionWorld` | 已有 | `WorldInteractionSystem` |
| `ISpatialWorldQuery` | 已有 | 物理域 |
| `ISpatialWorldRevisionSource` | 已有 | 物理域 |
| `IWorldMutationSource` | **新增** | `WorldNetworkProjectionSystem` |

端口只暴露方法，不暴露组件类型。`WorldRuntime` 实现全部四个。

### 3.3 第三道：守卫脚本

| 项 | 内容 |
| --- | --- |
| 文件 | `Build/guards/check-world-internal.ps1` |
| 规则 | 扫描 `src/**/*.cs`，若 `World/` 之外出现下列类型名则报错并列出 `文件:行`：<br>`WorldMutationBufferComponent`、`WorldLiquidStateComponent`、`WorldRuntimeStateComponent`、`WorldTileStorageComponent` |
| 白名单 | 见 §4 处理后的结果；脚本内以路径列表显式声明 |
| 退出码 | 有违规返回 1，可接入构建流程 |

这是唯一能覆盖"同程序集越界"的手段，因为编译器在这里帮不上忙。

## 4. 现有越界的处置（三处，逐一确定）

| # | 越界 | 位置 | 处置 |
| --- | --- | --- | --- |
| L1 | `LiquidTileStorageBinding` 的构造参数是两个 World 组件 | `Share/Physics/Liquid/LiquidTileStorageBinding.cs:15-20` | **消除**。W2 落地后绑定由 `WorldLiquidStateComponent` 私有持有，`WorldRuntime.cs:55` 的构造与 `13-16` 的字段引用一并移除 |
| L2 | `Program.cs` 使用 `WorldRuntimeStateComponent.DefaultTileSize` | `NSSLC.Server/Program.cs:54-57` | **消除**。新增 `WorldDefaults`（`public static class`，**只放常量**），把 `DefaultTileSize`、`DefaultDayLengthSeconds` 迁入；`WorldRuntimeStateComponent` 再转 `internal` |
| L3 | `WorldNetworkProjectionSystem` 直接调 `WorldRuntime.DrainPendingMutations` | `Communication/WorldNetworkProjectionSystem.cs:47` | **改端口**。改为依赖 `IWorldMutationSource`。注意 `:108` 还用了 `CommitRevision`，该属性已在 `ISpatialWorldRevisionSource` 覆盖范围内，需确认取值一致 |

**L3 的性质必须写进规则**：网络投影读取世界变更是**正当消费**，不是越界。
所以规则表述为——**禁止外部持有内部状态类型；允许外部依赖端口接口**。

## 5. `WorldRuntime` 改造后的形态

```text
WorldRuntime（唯一世界所有者，public sealed class）
  ├─ State     : WorldRuntimeStateComponent   internal
  ├─ Tiles     : WorldTileStorageComponent    internal
  ├─ _mutations: WorldMutationBufferComponent internal  ← 新增
  ├─ _liquid   : WorldLiquidStateComponent    internal  ← 新增
  └─ _gate     : object                       private

  public 实现：IWorldInteractionWorld
              ISpatialWorldQuery
              ISpatialWorldRevisionSource
              IWorldMutationSource            ← 新增
```

`WorldRuntime` 自身保持 `public`——它是唯一对外的世界入口，服务端 `Program.cs` 继续用它。
组件才是内部细节。

## 6. 改动文件清单

| 文件 | 动作 |
| --- | --- |
| `World/WorldMutationBufferComponent.cs` | 新增 |
| `World/WorldLiquidStateComponent.cs` | 新增 |
| `World/IWorldMutationSource.cs` | 新增 |
| `World/WorldDefaults.cs` | 新增（常量） |
| `World/WorldRuntimeStateComponent.cs` | `public` → `internal`；常量迁出 |
| `World/WorldTileStorageComponent.cs` | `public` → `internal` |
| `World/WorldRuntime.cs` | 字段与 4 组方法迁移到新组件；实现 `IWorldMutationSource` |
| `Communication/WorldNetworkProjectionSystem.cs` | 依赖改为 `IWorldMutationSource` |
| `Share/Physics/Liquid/LiquidTileStorageBinding.cs` | 移除对 World 组件的直接依赖 |
| `NSSLC.Server/Program.cs` | 常量引用改为 `WorldDefaults` |
| `Build/guards/check-world-internal.ps1` | 新增 |
| `World/README.md` | 补充组件清单与访问规则 |

## 7. 验收清单

- [ ] `src/NSSLC/Terraria.Components.csproj` 构建 0 警告 0 错误
- [ ] `src/NSSLC.Server/NSSLC.Server.csproj` 构建 0 警告 0 错误
- [ ] `check-world-internal.ps1` 返回 0 违规
- [ ] `World/` 之外无任何 `WorldMutationBufferComponent` / `WorldLiquidStateComponent` / `WorldRuntimeStateComponent` / `WorldTileStorageComponent` 引用
- [ ] 行为等价：液体推进、变更排空、tick 推进三处行为不变（纯搬移，不改逻辑）
- [ ] `WorldItemStore`、`WorldTileDropRules` 未被改动

## 8. 明确不做

| 项 | 理由 |
| --- | --- |
| 瓦片数组组件化 | 违反 `ECSEntity组织设计约束.md:49-50`；数量级不可行 |
| `WorldItemStore` 内部化 | 是跨程序集共享服务（§2） |
| 拆独立程序集（原 E2） | 本轮不做；代价大且与 3 道手段重叠 |
| Roslyn 分析器（原 E5） | 收益与守卫脚本重叠，成本最高 |
| W3 世界时钟 | 默认不做，除非你明确要 |

## 9. 需要你确认的点

1. **W1 + W2 是否可做？** W3 默认不做，如需请说明。
2. **`WorldRuntimeStateComponent` / `WorldTileStorageComponent` 转 `internal`** 会改变它们的公共可见性——这属于公共 API 变更，需你确认。
3. **守卫脚本放 `Build/guards/`** 是否符合你的目录偏好。
4. L3 改端口后，`WorldNetworkProjectionSystem` 是否仍需要 `CommitRevision` 的直接访问，还是改走 `Revision`。
