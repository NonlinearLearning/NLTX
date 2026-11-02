# World 特性组件化与外部禁用：候选参考清单

```text
文档 ID：DOC-2026-11-01-world-componentization-options
逻辑域：reviews
产物类型：design
状态：active
范围：src/NSSLC/Components/World/**（22 个文件，其中 21 个 .cs）
证据入口：src/NSSLC/Components/World/WorldRuntime.cs
      src/NSSLC/Components/World/WorldRuntimeStateComponent.cs
      src/NSSLC/Components/World/WorldTileStorageComponent.cs
canonical 路径：docs/reviews/audits/2026-11-01-world-componentization-options.md
```

本文按你的要求给出**参考列表**，不代替你决策。三份清单分别对应三个问题：
哪些特性可组件化（§2）、用什么手段禁止外用（§3）、现有越界怎么处理（§4）。
§5 给出三个可直接选的组合套餐。

## 1. 现状基线

### 1.1 已经是组件的五个类型

| 类型 | 种类 | 可变性 | 变更入口 |
| --- | --- | --- | --- |
| `WorldRuntimeStateComponent` | `public sealed class` | 可变 | `internal TryAdvance` / `TryAdvanceRevision` / `TryReload` |
| `WorldTileStorageComponent` | `public sealed class` | 可变 | `internal TryWrite` / `Replace` / `WriteUnchecked` |
| `WorldInteractionCapabilityComponent` | `public record struct` | 不可变 | — |
| `WorldInteractionRequestComponent` | `public record struct` | 不可变 | — |
| `WorldInteractionResultComponent` | `public record struct` | 可变 | 由系统写回 |

已有约定是「**public 读 / internal 写**」，不是「public 完全开放」。

### 1.2 `WorldRuntime` 里尚未组件化的状态

| 状态 | 行号 | 类型 | 当前可见性 |
| --- | --- | --- | --- |
| `_pendingMutations` | 26 | `Dictionary<WorldTileCoordinate, WorldMutation>` | `private`，但经 `public DrainPendingMutations()`（354）对外暴露 |
| `_liquid` | 27 | `LiquidSimulation`（`internal sealed class`） | `private` |
| `_liquidStorage` | 28 | `LiquidTileStorageBinding` | `private`，但该类反向持有 World 组件 |
| `_gate` | 25 | `object` | `private` |

除此之外 `WorldRuntime` 有 21 个方法，其中 9 个是纯几何/碰撞计算（`Move`、`MoveAxis`、
`ResolveAxis`、`Collides`、`ClampToWorld`、`GetTileRange`、`TrySnapToGround`、`IsFinite`、
`TileRange`），不持有状态。

### 1.3 已存在的三处越界

| 越界点 | 位置 | 越界性质 | 跨程序集 |
| --- | --- | --- | --- |
| `LiquidTileStorageBinding` 直接持有 `WorldRuntimeStateComponent` 与 `WorldTileStorageComponent` | `Share/Physics/Liquid/LiquidTileStorageBinding.cs:15-20` | 同程序集；**物理域反向依赖世界域内部状态** | 否 |
| `Program.cs` 使用 `WorldRuntimeStateComponent.DefaultTileSize` | `NSSLC.Server/Program.cs:54-57` | 跨程序集 | 是 |
| `WorldNetworkProjectionSystem` 调用 `DrainPendingMutations()` | `Communication/WorldNetworkProjectionSystem.cs:47` | 同程序集；读取世界内部缓冲 | 否 |

结论：**"不能在 World 外使用"这条规则目前已被违反三次**，其中一次跨程序集。

## 2. 组件化候选清单

每个候选取四个维度判定：状态内聚性、当前访问者、组件化收益、风险。

### C1 待提交变更缓冲 → `WorldMutationBufferComponent`

| 维度 | 内容 |
| --- | --- |
| 现状 | `WorldRuntime.cs:26` 私有字典；`RecordMutationUnsafe`(375) 写；`DrainPendingMutations`(354) 排空 |
| 内聚性 | 高。只服务"世界变更 → 网络投影"这一条链路 |
| 当前访问者 | `WorldNetworkProjectionSystem.cs:47` |
| 收益 | 把"世界变更记录"从 `WorldRuntime` 的 648 行里剥离，缓冲有自己的生命周期（累积 → 排空） |
| 风险 | 低。读写都在同程序集的少数几处 |
| 顺带解决 | 若该组件定为 World 内部，则 §1.3 第三处越界自动需要改为端口调用 |

### C2 液体模拟状态 → `WorldLiquidStateComponent`

| 维度 | 内容 |
| --- | --- |
| 现状 | `_liquid`(27) + `_liquidStorage`(28) 两个私有字段；`AdvanceLiquidSimulation`(310) 驱动 |
| 内聚性 | 高。液体有独立的队列、预算与推进语义（`LiquidSimulation` 已是 `internal sealed class`） |
| 当前访问者 | 只在 `WorldRuntime` 内部；但 `LiquidTileStorageBinding` 反向持有 World 两组件 |
| 收益 | 最大的一块内聚状态；组件化后 `WorldRuntime` 行数显著下降 |
| 风险 | 中。`LiquidSimulation` 已是 internal 类，外面再套一个组件可能只是转发而无实质收益；**先判定是"包一层"还是"直接改用组件"** |
| 关联 | 与 §1.3 第一处越界是同一个问题的两面 |

### C3 世界时钟 → `WorldClockComponent`

| 维度 | 内容 |
| --- | --- |
| 现状 | `WorldRuntimeStateComponent` 同时持有**不可变元数据**（`Width`/`Height`/`TileSize`/`WorldEpoch`/`DayLengthSeconds`）与**可变时钟**（`LastTick`/`TotalTimeSeconds`/`TimeOfDaySeconds`/`DayTime`/`Revision`） |
| 内聚性 | 中。两类字段的更新频率和责任人不同：元数据建世界时定，时钟每 tick 变 |
| 当前访问者 | 时钟部分经 `AdvanceTick`(109) 与 `Revision`(95) 出口 |
| 收益 | 元数据与时钟分离后，`AdvanceTick` 的写入范围更窄 |
| 风险 | **中偏高**。`Revision` 同时被 `TryAdvance`、`TryAdvanceRevision`、`TryReload` 三处修改，且 `Revision` 通过 `SpatialWorldRevision` 供给物理域。拆时钟若把 `Revision` 一起搬走，会牵动物理域的版本契约 |
| 建议 | 若做，只搬时间语义（`LastTick`/`TotalTime`/`TimeOfDay`/`DayTime`），**`Revision` 留在原处** |

### C4 掉落物存储 → `WorldItemStoreComponent`

| 维度 | 内容 |
| --- | --- |
| 现状 | `WorldItemStore.cs` 是 209 行普通类，不是组件；`WorldItemState.cs` 26 行 |
| 内聚性 | 高。掉落物有自己的增删改查与状态机（`WorldItemChangeKind`） |
| 当前访问者 | 待确认（本文未逐一核查其调用方） |
| 收益 | 明确归属，避免 `WorldItemStore` 变成第二个 `WorldRuntime` |
| 风险 | 低（若当前无外部访问者）。**但需先核查调用方**，本文未做 |

### C5 不应组件化的对象（边界参考）

| 对象 | 不组件化理由 |
| --- | --- |
| 瓦片数组 `_tiles` | `ECSEntity组织设计约束.md:49-50` 明确"瓦片值…批量数据…不强制逐项实体化"；tile 数量级不可行 |
| `_gate` 锁 | 同步原语不是领域状态 |
| 9 个几何/碰撞方法 | 无状态纯计算，属系统而非组件（`:112`） |

## 3. 外部禁用手段参考清单

### E1 仅靠现有 `internal`（零改动）

| 项 | 内容 |
| --- | --- |
| 能拦住 | **只能拦住跨程序集**：`MinimalImplementation`、`NSSLC.Server` |
| 拦不住 | 同程序集内的一切：`Communication/`、`Share/Physics/Liquid/`、`Projectile/`、`Npc/` |
| 代价 | 无 |
| 副作用 | 若把 `WorldRuntimeStateComponent` 改成 `internal`，`NSSLC.Server/Program.cs:54-57` 立刻编译失败 |
| 判定 | **本仓库的「World 系统」与其它系统同属一个程序集，`internal` 达不到你要的效果** |

### E2 拆独立程序集 + `InternalsVisibleTo`

| 项 | 内容 |
| --- | --- |
| 能拦住 | 编译器强制。组件在 `Terraria.World.Internal` 内 `internal`，只有 World 系统所在程序集可见 |
| 拦不住 | 被 `InternalsVisibleTo` 显式放行的友元程序集 |
| 代价 | 新增 csproj、调整依赖方向、`Terraria.Components` 需拆分或引用新项目 |
| 参考 | 仓库已有此模式：`src/.../AssemblyInfo.cs:3` `[assembly: InternalsVisibleTo("NSSLC.Infrastructure.Network.Verification")]` |
| 判定 | 唯一的**编译期真强制**，但改动最大 |

### E3 端口接口 + `internal` 组件

| 项 | 内容 |
| --- | --- |
| 能拦住 | 组件类型 `internal` 或 `private`，外部只能看到 `IWorld*` 端口接口 |
| 拦不住 | 同程序集内仍可用反射或直接引用（若类型是 `internal`） |
| 代价 | 中。需要为每个组件设计对应端口，端口方法会随组件字段增长 |
| 参考 | `WorldRuntime` 已实现 `IWorldInteractionWorld`、`ISpatialWorldQuery`、`ISpatialWorldRevisionSource` 三个端口，此模式不是新发明 |
| 判定 | 与 E1 组合后，把"外部能用什么"缩小到端口契约 |

### E4 构建期守卫脚本

| 项 | 内容 |
| --- | --- |
| 能拦住 | 任何路径不在白名单内的引用；可精确到"哪些文件允许引用哪些 World 组件" |
| 拦不住 | 绕过脚本（不跑它就没人管） |
| 代价 | 一个 PowerShell 脚本 + 接入验证流程 |
| 参考 | 仓库 `Build/diagnostics/` 下已有同类脚本（`find-cross.ps1`、`find-bad-type.ps1`），文化上不突兀 |
| 判定 | **今天就能生效且能覆盖同程序集**，是 E1 的实用补充 |

### E5 Roslyn 分析器

| 项 | 内容 |
| --- | --- |
| 能拦住 | 编译期，且可给精确诊断信息 |
| 代价 | 最高。需新建分析器项目并接入所有项目 |
| 参考 | 仓库只有源生成器（`PacketDesignCompiler.Generator`），无分析器基础设施 |
| 判定 | 收益与 E2 重叠但成本更高，**不建议** |

### 对照总表

| 手段 | 拦跨程序集 | 拦同程序集 | 编译期 | 改动量 |
| --- | --- | --- | --- | --- |
| E1 `internal` | 是 | **否** | 是 | 无 |
| E2 独立程序集 | 是 | 是 | 是 | 大 |
| E3 端口接口 | 部分 | 部分 | 部分 | 中 |
| E4 守卫脚本 | 是 | 是 | 否（构建后） | 小 |
| E5 分析器 | 是 | 是 | 是 | 大 |

## 4. 现有越界处理参考清单

### L1 `LiquidTileStorageBinding` 持有 World 两组件（同程序集）

| 方案 | 做法 | 代价 | 备注 |
| --- | --- | --- | --- |
| L1-a 移进 World | 把 `LiquidTileStorageBinding` 移入 `World/` | 小 | 但它实现的是液体存储绑定，语义上属液体 |
| L1-b 反转依赖 | World 持有绑定，对外只暴露液体端口 | 中 | 与 C2 组件化天然合并 |
| L1-c 白名单 | 在规则里显式允许该文件 | 无 | 等于承认这是设计例外 |
| L1-d 不动 | 只登记 | 无 | 规则形同虚设 |

### L2 `Program.cs` 使用 `DefaultTileSize`（跨程序集）

| 方案 | 做法 | 代价 |
| --- | --- | --- |
| L2-a 常量移到中立位置 | 移到 `WorldTileCoordinate` 或新 `WorldConstants` | 小 |
| L2-b 白名单 | 允许 Server 入口引用该类型 | 无 |
| L2-c 公开只读门面 | 新增一个只含常量的 public 类型 | 小 |

### L3 `WorldNetworkProjectionSystem` 调 `DrainPendingMutations`（同程序集）

| 方案 | 做法 | 代价 | 备注 |
| --- | --- | --- | --- |
| L3-a 改为端口 | World 暴露 `IWorldMutationSource` 端口 | 小 | 与现有三端口模式一致 |
| L3-b 白名单 | 允许通信域拉取变更 | 无 | 网络投影本来就需要世界变更 |
| L3-c 事件推送 | World 变更时推给订阅者 | 中 | 反转了当前的拉取模型 |

**注意**：L3 的性质与 L1/L2 不同。网络投影**必须**知道世界变更，这不是"越界"而是"正当消费者"。
所以规则若写成"任何 World 外引用都禁止"，会直接与这一需求冲突。规则需要区分
**读端口**（允许）与**内部状态类型**（禁止）。

## 5. 三个组合套餐

| 套餐 | 组件化 | 强制 | 越界处理 | 改动量 |
| --- | --- | --- | --- | --- |
| **A 最小** | C1 | E1 + E4 | L2-a、L3-a、L1-d 登记 | 小 |
| **B 推荐** | C1 + C2 + C4 | E1 + E3 + E4 | L1-b、L2-a、L3-a | 中 |
| **C 彻底** | C1 + C2 + C3 + C4 | E2 | 全部改端口 | 大 |

推荐 B 的理由：它把"外部能用什么"从"任何 public 成员"收敛到端口接口，
且用 E4 覆盖了 `internal` 拦不住的同程序集场景，同时避免 C3 牵动 `Revision` 契约。

## 6. 待决策项

| 项 | 需要你定 |
| --- | --- |
| 组件化范围 | C1 / C2 / C3 / C4 选哪些（§2） |
| 强制手段 | E1 / E2 / E3 / E4 组合（§3） |
| 规则边界 | "禁止外用"是否排除读端口（§4 注意事项） |
| 三处越界 | L1 / L2 / L3 各选哪个方案（§4） |

## 7. 未验证事项

- 未核查 `WorldItemStore` 的调用方（C4 的风险项）。
- 未核查 `WorldRuntime` 21 个方法各自的外部调用者，只核查了组件类型与 `WorldRuntime` 本身。
- 未验证 C2 中 `LiquidSimulation` 与 `LiquidTileStorageBinding` 的耦合是否允许直接组件化。
- 未评估 C3 拆分对 `SpatialWorldRevision` 消费方（`Share/Physics/` 内 5 个文件）的影响。
- 本文为设计分析，未修改任何源码，未执行构建。
