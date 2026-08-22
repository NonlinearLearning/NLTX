# Wiring、Liquid、Chest 行为覆盖表

状态：`in-progress`。`verified` 只表示当前仓库已有可执行证据覆盖该切片；不能外推为
旧 God Object 的完整行为等价。

| 领域 | 行为族 | 状态 | 当前证据或边界 |
| --- | --- | --- | --- |
| Wiring | 单色线遍历、稳定顺序、预算、去重 | verified | `Terraria.Dome.Wiring.Verification`，并由 `DomeSimulation.AdvanceWiring` 消费输入 |
| Wiring | 压力板到门/灯 | verified | `LampComponent` 将亮/灭 Tile 类型显式建模；`WiringLiquidChest.Loopback.Verification` 覆盖压力板到门和灯 Tile commit |
| Wiring | 执行器 Tile 命令 | verified | `ActuatorCommandSystem` 只产生命令，由 Simulation commit 到 `WorldGrid` |
| Wiring | 泵到液体命令 | verified | `WiringLiquidChest.Loopback.Verification` 覆盖泵的值类型命令和 Liquid commit |
| Wiring | 逻辑门 | verified | `WiringLiquidChest.Loopback.Verification` 覆盖上升沿输出和执行器 commit |
| Wiring | 传送器/炮/Hopper/PixelBox | excluded | 本提案第一批不覆盖；必须保持显式拒绝或兼容隔离 |
| Liquid | 单源下落和有界队列 | verified | `Terraria.Dome.Liquid.Verification` 覆盖输入预算和固定邻居顺序 |
| Liquid | 普通传播的 Tile solidity/platform predicate | verified | `TileDefinitionRegistry` 覆盖 Version4 的 753 个 Tile ID；`Terraria.Dome.Liquid.Verification` 验证 active solid 阻挡、active platform 接受、inactive 通过和未知类型拒绝 |
| Liquid | 横向传播、沉降、卡住诊断 | verified | `Terraria.Dome.Liquid.Verification` 覆盖固定邻居、确定性重排和最大三次 retry 证据 |
| Liquid | 水/岩浆/蜂蜜/微光边界和合并规则 | verified | `Liquid.Verification` 覆盖 12 条有序合并结果，并验证运行时同时提交被消费液体与结果 Tile |
| Liquid | 全局表的接触 Tile 销毁 | verified | `TileDefinitionRegistry` 从 Version4 `tileWaterDeath`（10 IDs）和 `tileLavaDeath`（267 IDs，含 `435..439` 循环）生成基础规则。`LiquidPropagationSystem` 只产生带 `PreserveLiquid` 的 `TileChangeCommand.Kill`，由 `DomeSimulation` 的既有 Tile commit 边界提交；focused verifier 覆盖 Water/Honey/Shimmer、水表命中、Lava、inactive、非死亡 Tile 和 kill 后液体保留。 |
| Liquid | 普通运行时 Panic 调度 | verified | `LiquidPanicPolicy` 与 `LiquidWorldStateComponent` 对高水位连续观察、Panic 有界 drain 和恢复进行确定性建模；focused verifier 覆盖连续触发、配置预算和非破坏恢复。它不调用 `QuickWater`，不改变 753-ID 基础定义或普通传播邻居顺序。 |
| Liquid | 旧版完整 Tile/environment side effects | partial | 当前只承诺四种液体的确定性合并消费、Tile projection、全局 death-table 的普通运行时接触销毁和有界运行时 Panic 调度。`TileObjectData` 的 frame/style/subtile/alternate 覆盖、多 Tile object 销毁、`WorldGen.KillTile` 的掉落/音效语义、`tilesIgnoreWater`/`worldGenTilesIgnoreWater`、生成期 `QuickWater`/动态 `StartPanic` 副作用、切草、危险 Tile 和地下沙漠规则仍未迁移；不能把 `WorldTile.IsActive` 或基础 death table 猜测为全部 legacy behavior。 |
| Liquid | 泵输入和跨域转移 | verified | `LiquidTransferSystem` 在 Liquid commit 边界消费 Wiring 命令 |
| Liquid | commit 后 section revision 和网络合并 | verified | `LiquidCommitSystem` 和 `LiquidReplicationSystem` 均在 commit 后生成不可变快照；`Terraria.Dome.Liquid.Loopback.Verification` 通过真实 `DomeServer` 将 32-unit tick delta 投影为可见 module 0，并拒绝远端 section |
| Chest | 世界箱 40 槽、独占 opener、范围 | verified | `Terraria.Dome.WorldObjects.Verification` |
| Chest | 世界箱放置、坐标索引与空箱销毁 | verified | `ChestIndexSystem` 和 `ChestMutationCommitSystem` 以值类型 create/destroy command 维护位置唯一性；`WorldObjects.Verification` 覆盖未知 ID、重复坐标、非空/打开拒绝、关闭空箱删除和坐标复用 |
| Chest | 原子转移和 ItemDefinition/容量校验 | verified | `Terraria.Dome.WorldObjects.Verification` 覆盖 server-owned transfer |
| Chest | 有界重命名、revision 和持久化名称 round-trip | verified | `WiringLiquidChest.Loopback.Verification`、`Terraria.Dome.Persistence.Verification` 覆盖空名、20 字符边界、rename revision 和二进制恢复 |
| Chest | 持久化候选加载、坐标唯一性与 revision | verified | `Persistence.Verification` 拒绝重复坐标候选；`WiringLiquidChest.Loopback.Verification` 验证槽位和 revision 恢复 |
| Chest | 断线释放与重连恢复 | verified | `WiringLiquidChest.Loopback.Verification` 验证重开、关闭与 opener 释放 |
| Chest | 银行账户所有权/商店规则 | excluded | 第一批只承诺世界箱；不得复用普通箱语义 |
| Chest | 协议消息 31-34 投影 | verified | `Terraria.Dome.WorldObjects.Verification` 和 `WorldObjects.Loopback.Verification` 覆盖 typed codec、PVS、独占和 server-owned transfer |
| Chest | 协议消息 69 `ChestName` | excluded | 本轮不实现客户端回写/服务端投影；`NetworkIsolation.Verification` 证明真实 server 边界拒绝恶意 payload 且不改变 chest 状态 |

## 2026-08-19 runtime replication evidence

The server Liquid loopback was run from the repository root with
`-p:UseSharedCompilation=false` after the Simulation and Server builds. It seeds water and lava at
`(2100,300)` and queues a server-owned source through `DomeServer.QueueLiquidSourceAsync`.
The first deterministic propagation tick leaves `32` units in `(2100,299)`, while the merge path
consumes the source and adjacent lava. The observed module 0 contains `(2100,299)=32`,
`(2100,300)=0`, and `(2101,300)=0`; the source Tile is committed to type `56`.
A second session moved to `(100,300)` receives no module 0 update for the source section. This is
runtime evidence for commit ordering, NetLiquidModule projection, and visible-section filtering;
it does not claim full legacy Liquid.cs side-effect parity.

## Excluded behavior policy

被标记为 `excluded` 的旧方法不会被空实现吞掉。兼容层必须返回明确拒绝原因或保留原协议
隔离记录，并且不能计入迁移完成率。
