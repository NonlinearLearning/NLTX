# ECS Domain Owner 收窄与项目配置清理执行计划

文档 ID：PLAN-2026-11-08-ECS-DOMAIN-BOUNDARY-PROJECT-CONFIG
逻辑域：plans
产物类型：plan
状态：active
范围：`Terraria.Components`、`WorldRuntime` contracts、网络/基础设施依赖方向、活动 csproj 的旧路径声明
证据入口：`src/NSSLC/Terraria.Components.csproj`；`src/NSSLC/Components/World/WorldRuntime.cs`；`src/NSSLC/Components/World/Contract/`；`src/NSSLC.Network/WorldNetworkProjectionSystem.cs`；根级 ECS 领域与基础设施边界约束
canonical 路径：`docs/plans/2026-11-08-ecs-domain-boundary-project-config-plan.md`

## 1. 目标

在不复制权威 World 状态、不恢复历史应用层的前提下，收窄组件程序集和 World owner 的公开职责；同时清理活动项目中无效的旧 `Component/**/*.cs` 声明和可确认的旧路径引用。

## 2. 当前边界

`Terraria.Components` 同时包含组件、System、EntityDefinition、队列、World runtime、AI、Inventory、Combat、Physics 和部分调度入口。该程序集目前是可工作的共享领域运行时，但不是纯组件程序集。

`WorldRuntime` 同时实现 `IWorldInteractionWorld`、`ISpatialWorldQuery`、`ISpatialWorldRevisionSource`、`IWorldTileGrid` 和 `IWorldProjectionSource`，并拥有 tile storage、liquid simulation、pending mutation、revision、tick、空间查询和 projection facts。

网络 projection 已经通过 `IWorldProjectionSource` 和 `IWorldItemProjectionSource` 读取窄端口，这是保留的正确方向。真正的拆分需要新增功能需求或可测耦合证据驱动，不能仅按类名拆出多个共享状态 owner。

## 3. 执行阶段

### Phase 1：依赖和公开面盘点

- 生成 `Terraria.Components` 中按 namespace、System、component、runtime service 的清单，标记跨项目引用者和可变状态 owner。
- 对 `WorldRuntime` 的每个 public method/property 标注读、写、revision、锁、projection 或 persistence 语义。
- 验证 `NSSLC.Network`、`NSSLC.Server` 和基础设施只依赖稳定 contract/port，不通过具体 World storage 读取权威状态。

### Phase 2：窄端口和 owner 约束

- 对已有 `IWorldProjectionSource`、`IWorldTileGrid`、`ISpatialWorldQuery` 做接口最小化审查；删除只为方便调用而暴露的可变集合或具体 store。
- 若发现同一接口同时提供读和权威写，拆成读 port 与 owner-only command，保持 `WorldRuntime` 仍是唯一实现者。
- 为 persistence、chunk streaming、snapshot 等未来能力只定义应用/基础设施端口，不把文件句柄、编码器、字节缓冲或平台异常放进 ECS。
- 只有当审计证明一个职责有独立生命周期、部署/测试边界和稳定契约时，才物理拆分 class/project；否则保留内部协作者并记录 proposed 状态。

### Phase 3：活动项目配置清理

- `src/NSSLC/Terraria.Components.csproj` 删除已经不存在的 `Component/**/*.cs` Include，保留并验证 `Components/**/*.cs` 的显式编译范围。
- 扫描活动生产项目中的 `src/NSSLC/Component/`、旧 project path 和失效 `ProjectReference`；只修复当前任务允许且不在保护目录中的声明。
- `NSSLC.Infrastructure/` 受保护目录中的历史项目引用只记录审计结果，除非用户另行授权，不在本批改写。
- 更新文档 manifest 或相关导航，使计划、配置变更和后续验证可追踪。

## 4. 验收标准

- 依赖图证明 ECS 领域不引用具体文件/网络/平台实现，基础设施依赖稳定 port/DTO；没有新复制的权威 World state。
- World projection、spatial、interaction 和 tile tests 的结果不变；锁、revision、pending mutation 不变量有针对性测试。
- 活动 `Terraria.Components` 项目构建不再把不存在的路径当作源码入口；编译文件清单与实际目录一致。
- 组件程序集的职责收窄结果有明确的 `confirmed/partial/proposed` 状态；不以改名或移动文件声称架构已经拆分完成。
- 记录未拆分的原因、触发条件和未来迁移入口，避免为了“看起来更 ECS”进行无证据的大范围重构。

## 5. 执行记录（2026-11-09）

- confirmed：Terraria.Components.csproj 已删除失效的 Component/**/*.cs 编译入口，保留 Components/**/*.cs。
- confirmed：新增 IWorldTileReader；IWorldInteractionWorld 仅保留 TryBreakTile 与 TryPlaceTile；WorldRuntime 仍是唯一权威 owner，并同时实现两个端口。
- confirmed：WorldInteractionSystem 的读取依赖与写入依赖已分离；Server 装配仍注入同一个 WorldRuntime，不改变锁、revision、pending mutation 或执行阶段。
- confirmed：World projection 仍只依赖 IWorldProjectionSource；活动生产项目中未发现新的旧 Component 路径声明，NSSLC.Infrastructure 历史引用未修改。
- partial：已完成源码扫描与 git diff --check；换行提示来自现有工作树格式差异，没有发现空白错误。
- unknown：Terraria.Components、NSSLC.Network、NSSLC.Server 构建被现有 CommunicationSessionIngress.cs 的 BoundedChannelFullMode.Reject 错误阻断；本次变更尚未获得编译与验证程序集结果。
- proposed：待并行 Communication 修改修复后，重新构建三个受影响项目，再运行 WorldItem、NpcSpawnEligibility、WorldSession.TileEntity、PlayerItemUse 验证，并补充 Projectile 与 NpcAi.MinimalVerification。
