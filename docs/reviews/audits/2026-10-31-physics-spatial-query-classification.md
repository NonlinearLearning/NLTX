# Physics 大类二：空间查询与液体规则

```text
文档 ID：DOC-2026-10-31-physics-spatial-query-classification
逻辑域：reviews
产物类型：audit
状态：active
范围：src/NSSLC/Components/Share/Physics/SpatialQuery 全部 6 个 .cs 文件，
      加 src/NSSLC/Components/Share/Physics/Liquid 全部 9 个 .cs 文件（液体，并入本类）
证据入口：src/NSSLC/Components/Share/Physics/**/*.cs
canonical 路径：docs/reviews/audits/2026-10-31-physics-spatial-query-classification.md
```

## 1. 大类定义

本类回答："模拟向谁询问世界、询问以什么版本为准、每次询问的输入帧与输出值是什么形状，
以及问回来的液体接触按什么规则解释"。

本类是 Physics 与外部世界的唯一接缝：具体的 Tile 数组、`WorldOwner`、Arch `World` 和网络
连接都留在适配器一侧，不会作为组件进入 ECS。液体不单列大类，因为它在本仓的物理意义就是
**空间查询的第二类输出**：`ISpatialWorldQuery.GetLiquidContact` 是它的唯一入口，
`SpatialLiquidContact.ToComponent()` 是它的唯一出口，不存在独立的液体模拟系统。

## 2. 成员清单

### 2.1 空间查询（Physics）

| 类型 | 文件 | 角色 | 关键成员 | 跨领域依赖 |
| --- | --- | --- | --- | --- |
| `ISpatialWorldQuery` | `ISpatialWorldQuery.cs` | 端口接口（只读世界） | `Revision`、`Move(...)`、`GetLiquidContact(...)` | `Terraria.Movement`、`Terraria.Liquid` |
| `ISpatialWorldRevisionSource` | `ISpatialWorldRevisionSource.cs` | 端口接口（版本源） | `Current` | 无 |
| `SpatialWorldRevision` | `SpatialWorldRevision.cs` | 值对象 | `Epoch`、`Version`、`IsValid`、`Initial` | 无 |
| `SpatialFrame` | `SpatialFrame.cs` | 输入帧（批次级不可变输入） | `Tick`、`Revision`、`World` | 无（经接口间接依赖） |
| `SpatialTileMotionResult` | `SpatialTileMotionResult.cs` | 查询输出值 | `Position`、`Velocity`、`CollidedX`、`CollidedY`、`IsFinite` | 无 |
| `SpatialLiquidContact` | `SpatialLiquidContact.cs` | 查询输出值 | `Flags`、`Amount`、`ToComponent()` | `Terraria.Liquid` |

### 2.2 液体规则（Liquid）

| 类型 | 文件 | 角色 | 关键成员 | 说明 |
| --- | --- | --- | --- | --- |
| `LiquidInteractionCapabilityComponent` | `LiquidInteractionCapabilityComponent.cs` | 能力组件（规则输入） | `Enabled`、`AffectedLiquids`、`Affects(LiquidContactFlags)` | 声明实体受哪几类液体影响 |
| `LiquidContactFlags` | `LiquidContactFlags.cs` | 位标记枚举 | `None`、`Water`、`Honey`、`Lava`、`Shimmer` | 接触类别，`[Flags]`，底层 `byte` |
| `LiquidContactComponent` | `LiquidContactComponent.cs` | 组件（提交后状态） | `Flags`、`Amount`、`IsWet`、`Contains(...)`、`Filter(...)` | 空间模拟提交的本帧接触事实 |
| `LiquidComponent` | `LiquidComponent.cs` | 遗留类型（**未使用**） | `LiquidList` | 见第 5 节，零引用；2026-10-31 命名空间已归入 `Terraria.Liquid` |
| `LiquidKind` | `LiquidKind.cs` | 枚举 | `Nano`、`Water`、`Shimmer`、`Honey`、`Lava` | 2026-10-31 从 `LiquidComponent.cs` 拆出 |
| `LiquidSimulation` | `LiquidSimulation.cs` | 液体运算内核（`internal`） | `Step(...)`、`Enqueue(...)`、`SeedUnsettled(...)`、`GetContact(...)`、`HasQueuedWork`、`Reset()` | 见第 6 节 |
| `LiquidTileStorage` | `LiquidTileStorage.cs` | 存储接缝（端口，`internal`） | `Width`、`Height`、`ToIndex`、`ToCoordinate`、`Read`、`Write` | 见第 6 节 |
| `LiquidTileStorageBinding` | `LiquidTileStorageBinding.cs` | 端口实现（`internal`） | 绑定具体 tile 存储，薄封装无锁无日志 | 见第 6 节 |
| `LiquidBlockingRules` | `LiquidBlockingRules.cs` | 阻塞判定规则表（`internal`） | `BlocksLiquid(WorldTileState)`、`TryNormalize(...)` | 见第 6 节 |

### 2.3 只读端口约束

`ISpatialWorldQuery` 只有两个方法，均为查询：

- `Move(...)` 返回 `SpatialTileMotionResult`，是"给定位置/速度/尺寸/能力/重力方向后的
  Tile 运动结果"；
- `GetLiquidContact(...)` 返回 `SpatialLiquidContact`。

接口注释明确"实现提供脱离且带版本的只读世界数据；不接收实体，也不执行提交"。因此
查询实现无法绕过提交屏障写入 ECS。

`GetLiquidContact` 的第三个参数就是 `LiquidInteractionCapabilityComponent`，所以液体
能力组件同时是**空间查询的入参**和**下游规则的解释依据**，这是把它并入本类而非独立
大类的直接证据。

### 2.4 液体规则的三层分工

Liquid 的 `README.md` 声明"液体查询输出和液体规则输入分开保存"，代码与此一致：

```text
LiquidInteractionCapabilityComponent   实体受哪些液体影响（规则/资格）
        ↓ 作为参数传入
ISpatialWorldQuery.GetLiquidContact    → SpatialLiquidContact（查询输出，短期值）
        ↓ ToComponent()
LiquidContactComponent                本帧接触事实（提交后的持久状态）
        ↓ Contains / Filter / IsWet
下游领域各自解释水、蜂蜜、岩浆、微光的效果
```

关键约束：`LiquidContactComponent.IsWet` 只是 `Flags != None` 的派生判断，
`Contains(flags)` 要求**全部**位命中，`Filter(allowed)` 做位与裁剪。README 明确禁止用
一个公共 `Wet` 布尔值替代这些规则——玩家、NPC、Projectile 共享接触事实格式，但各自的
后续效果由各自领域解释。

### 2.5 版本一致性

`SpatialFrame` 构造时校验三件事：`World` 非空、`tick >= 0`、`revision.IsValid` 且
`world.Revision == revision`。这让"帧携带的 revision 与查询端口声明的 revision"在构造期
强制一致，避免一次批次混合两个世界快照。

`SpatialWorldRevision` 以 `Epoch`（代次）+ `Version`（版本号）二元组表达，
`IsValid` 要求 `Epoch != 0 && Version >= 0`，`Initial` 为 `(1, 0)`。

### 2.6 输出的有限性

`SpatialTileMotionResult.IsFinite` 对位置与速度四个分量做有限性检查，是
`SpatialSimulationSystem` 拒绝非有限结果的依据；非有限结果被 `Rejected`，不会进入提交。

## 3. 与其他大类的衔接

| 衔接点 | 说明 |
| --- | --- |
| ← 空间模拟 | `SpatialSimulationSystem` 通过 `SpatialFrame.World` 调用 `Move` / `GetLiquidContact` |
| ← 空间模拟 | `SpatialSimulationCoordinatorSystem` 以 `_spatialWorld.Revision` 构造帧并二次核对 |
| ← 空间模拟 | 协调器仅在实体**已有** `LiquidContactComponent` 时写入，不会凭空创建 |
| → 空间模拟 | `SpatialLiquidContact.ToComponent()` 的产物经 `SpatialCommit.LiquidContact` 回写 |
| → 接触事实 | `PhysicsContactCaptureSystem` 读取 `ISpatialWorldQuery.Revision` 标注接触事实 |
| → 下游领域 | `LiquidContactComponent` 由 Player / Npc / Projectile 各自解释 |

## 4. 归类判定依据

- 6 个 Physics 文件共同只描述"查询端口 + 版本标识 + 帧输入 + 查询输出"，不含碰撞判定
  算法、不含模拟规则、不含跨帧缓冲。
- 4 个 Liquid 文件全部围绕"液体接触"这一条链路：一个能力输入、一个位标记、一个接触
  状态、一个遗留类型。它们没有独立系统、没有独立 tick 循环、没有独立提交端口，
  因此不构成与"空间模拟"并列的大类，而应并入本类。
- `SpatialLiquidContact` 是查询**输出**而非持久组件；它通过 `ToComponent()` 才转为
  液体领域的持久状态，转换边界清晰，这也说明两处代码属于同一职责链。
- `SpatialFrame` 虽由模拟侧构造，但它是批次共享的只读输入载体，语义是"这一次查询的
  世界视图"，故归本类而非空间模拟类。

## 5. 已发现的 Liquid 归档问题

以下为静态审阅发现，**本次未修改 `LiquidComponent.cs`**，仅记录问题供后续决策：

| 问题 | 证据 | 影响 |
| --- | --- | --- |
| 命名空间与目录不符 | `LiquidComponent.cs` 声明 `namespace EntityEcs.Components;`，同目录其余 3 个文件均为 `Terraria.Liquid` | 按 ECS 文件组织约束第 7 节，目录路径不自动决定命名空间；此为历史残留，需独立评估 |
| 一文件双类型 | `LiquidComponent.cs` 同时声明 `LiquidComponent`（struct）与 `LiquidKind`（enum） | 违反"一个核心公开类型对应一个同名 PascalCase 文件"的验收项 |
| 零引用 | 全 `src/NSSLC/**` 与 `Test/**` 搜索 `LiquidComponent`、`LiquidKind` 均无命中 | 未被任何系统或测试使用，属未接线状态 |
| 字段为公开可变列表 | `public List<LiquidKind> LiquidList;`，且与 `LiquidContactFlags` 的液体类别表达重复 | 与 `LiquidContactFlags` 形成同义状态（`Water`/`Honey`/`Lava`/`Shimmer` 重复，`Nano` 独有） |
| 文档注释与实现不符 | 注释称"保存实体液体类别、接触标记和接触计时"，实际只有 `LiquidList` 字段 | 注释描述的能力尚未实现 |

**建议**：`LiquidComponent` / `LiquidKind` 在正式接线前不应作为有效液体状态参与分类结论。
上表按现有源码如实记录，不做删除或合并。

## 6. 液体运算下沉（2026-10-31）

本节记录把 `WorldRuntime` 内联液体算法迁入 Liquid 领域的实际结构变更。

### 6.1 变更前后

| 项目 | 变更前 | 变更后 |
| --- | --- | --- |
| 算法位置 | `WorldRuntime` 私有方法（`TryFlowLiquidUnsafe`、`TryEqualizeLiquidHorizontallyUnsafe` 等） | `Liquid/LiquidSimulation.cs` |
| 队列与去重表 | `WorldRuntime` 五个私有字段（`_liquidQueue`、`_urgentLiquidQueue`、`_queuedLiquidFlags`、`_urgentLiquidFlags`、`_liquidProcessedPasses`） | `LiquidSimulation` 内部字段 |
| 阻塞规则 | `World/WorldLiquidRules.cs` | `Liquid/LiquidBlockingRules.cs` |
| 存储访问 | 算法直接调 `Tiles.ReadUnchecked` / `WriteUnchecked` | 经 `LiquidTileStorage` 端口 |
| 命名空间 | 曾为 `Terraria.World` / `World*` 前缀 | 全部统一为 `Terraria.Liquid`，去掉 `World` 前缀 |

### 6.2 保留在世界侧的部分

以下职责**没有**迁入算法，仍由 `WorldRuntime` 独占，因为它们是世界所有权的组成部分：

- `_gate` 锁与全部线程同步；
- `revision` 推进（`State.TryAdvanceRevision`）；
- `RecordMutationUnsafe` 变更日志与 `_pendingMutations`；
- `DrainPendingMutations` 供网络投影取走变更。

### 6.3 行为保持要点

- 算法主体逐行保留：预算计数含被阻塞/干燥条目、urgent 优先出队、pass 去重、
  向下流动、水平均衡（余数优先自身）、不可混合液体、满格保留（`retainFullSource`）。
- 队列上限 `75_000`、默认预算 `3_500` 保持原值。
- **revision 耗尽语义已保留**：原实现在无法推进 revision 时不写任何 tile；迁移版在
  `Step` 前先检查 `CanAdvanceRevision()`，避免"先写后失败"的偏差。
- 变更日志改由算法返回 `LiquidTileChange(Coordinate, Previous, Current)`，解决
  事后读取同坐标会得到相同前后值、导致日志丢失的问题。

### 6.4 命名统一（2026-10-31 第二轮）

`Liquid` 目录内全部 9 个 `.cs` 已统一到 `Terraria.Liquid` 命名空间，并去掉多余的
`World` 前缀：

| 旧名 | 新名 |
| --- | --- |
| `WorldLiquidSimulation` | `LiquidSimulation` |
| `WorldLiquidStorage` | `LiquidTileStorage` |
| `WorldLiquidStorageBinding` | `LiquidTileStorageBinding` |
| `WorldLiquidRules` | `LiquidBlockingRules` |

另外：

- `LiquidComponent.cs` 原为 `namespace EntityEcs.Components;`，已改为 `Terraria.Liquid`；
- 其内嵌的 `LiquidKind` 已按"一类型一文件"拆出为 `LiquidKind.cs`；
- `LiquidComponent.cs` 补上显式 `using System.Collections.Generic;`。

### 6.5 目录并入物理大类（2026-11-01）

把 `Share/Liquid/` 整体移入 `Share/Physics/Liquid/`，使液体成为物理大类的子目录：

| 项目 | 变更前 | 变更后 |
| --- | --- | --- |
| 目录 | `Components/Share/Liquid/` | `Components/Share/Physics/Liquid/` |
| C# 命名空间 | `Terraria.Liquid` | 不变（仍为 `Terraria.Liquid`） |
| 编译配置 | `Components/**/*.cs` 通配 | 不变，移动无需改 csproj |
| 类型名与成员 | — | 全部不变 |

按 ECS 文件组织约束第 7 节"目录路径表达归属，不自动决定 C# 命名空间"，本次只改物理
位置，不改命名空间、不改公共 API、不改行为。`Share/` 顶层不再有 `Liquid` 目录。

### 6.6 附带清理：重复的 Program.cs（2026-11-01）

`src/NSSLC/Components/Program.cs` 与 `src/NSSLC.Server/Program.cs` 字节完全相同
（MD5 `0C097172…`），是误置的服务器入口副本，且未被 git 跟踪。它被
`Compile Include="Components/**/*.cs"` 通配纳入编译，引用了仅存在于服务器项目中的
`ServerWorldLoader` / `LoadedServerWorld`，导致组件库编译失败。

经用户确认后已删除该副本。`src/NSSLC.Server/Program.cs` 不受影响，内容未变。

关于改名后的类型命名：`LiquidSimulation` 与 `MinimalImplementation.Server.World.LiquidSimulation`
同名但不同命名空间；`WorldRuntime` 不引用后者，故当前无二义。若将来同一文件同时引用两者，
需以别名或限定名消解。

### 6.7 验证

| 项目 | 命令 | 结果 |
| --- | --- | --- |
| 组件层 | `dotnet build src/NSSLC/Terraria.Components.csproj` | 成功，0 警告 0 错误 |
| 服务器 | `dotnet build src/NSSLC.Server/NSSLC.Server.csproj` | 成功，0 警告 0 错误 |

按约定本轮只要求编译通过，**未**运行 `LiquidBudgetProbe` / `LiquidPerfProbe` /
`WaterFlowProbe`，因此不宣称液体流动行为已完成等价性验证。

## 7. 校验记录

- 本类合计 15 个 `.cs` 文件（Physics 顶层 6 + `Physics/Liquid` 9）。Physics 顶层 2 个文件
  依赖 `Terraria.Movement` / `Terraria.Liquid`，其余 4 个无 `Terraria.*` 依赖；Liquid 侧
  9 个文件全部位于 `Terraria.Liquid`，其中运算内核、端口、绑定与规则表为 `internal`。
- 两个接口均为 `public interface`，未混入 Arch 类型，未混入实体标识。
- `LiquidContactFlags` 与 `LiquidInteractionCapabilityComponent` 的位运算契约一致：
  `Affects` 用 `(AffectedLiquids & liquid) != None`，`Contains` 用 `(Flags & liquid) == liquid`。
- 新增的液体运算类型均为 `internal`，不扩大公共 API 面。
- 第 6 节的源码变更为本轮实际实施；本文件其余章节仍为静态审阅结论。
- 按照约定只执行了编译验证，未运行行为探针，未执行 liquid 行为等价性验证。
