# CR-2026-10-23 使用 Arch World API 的世界系统

## 原始需求

> 使用世界api重写编写世界系统

提出人：用户  
渠道：对话  
提出时间：2026-10-23（Asia/Shanghai）

## 变更决定

| 项目 | 决定 |
| --- | --- |
| 级别 | 重大 |
| 排期 | 当前任务分批实施 |
| 执行策略 | spec |
| 范围 | 混合方案 |

混合方案的定义：Arch `World` 管理实体、世界交互能力、交互请求和结果；专用分块世界存储
继续管理 tile/wall/liquid 数据、WorldFile、脏区块和网络投影。

## 影响范围

- 新增世界交互请求和结果组件。
- `WorldInteractionSystem` 接入 `Arch.System` Tick 生命周期和 `World.Query`。
- 在明确的 Commit 阶段调用 `IWorldInteractionWorld`，将结果写回 Arch 组件；dirty section 仍由专用端口维护。
- 保留现有显式 `TryAcquire`、`TryReadTile`、`TryBreakTile`、`TryPlaceTile` API。
- 更新 Share/World 文档和 EcsServer 装配说明。

## 明确不包含

- 不把整张 tile map 建模为“一 tile 一个 Arch Entity”。
- 不迁移 WorldFile 编解码。
- 不重写液体、wiring、掉落和网络广播的完整实现。
- 不修改 `MinimalImplementation`。
- 不以 Arch `Entity` 替代 `EntityUuid`、存档身份或网络身份。

## 设计依据

典型 Terraria 世界是密集二维 tile 网格，存档和网络以压缩 tile payload、section 和分块
操作为边界。Arch 的 Entity/Archetype/Chunk 适合活动对象和请求状态，不适合直接替代连续
tile 存储。当前 `IWorldInteractionWorld` 和 `ISpatialWorldQuery` 已提供地图规则、版本化
读取和唯一提交边界，因此本变更只把 ECS 侧请求组织起来，不复制地图权威状态。

## 实施批次

1. 请求/结果组件和操作状态。
2. `WorldInteractionSystem` 的 Arch.System 适配，使用 `World.Query` 在 Commit 阶段读取并提交世界请求。
3. 装配与边界文档。

## 验证限制

按用户要求，本次不执行构建、测试、验证器、服务器启动或运行检查。源码交付状态不得描述
为已通过运行时验证。
