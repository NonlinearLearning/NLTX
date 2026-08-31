# 第 7 节：代码证据与公共契约冻结

## 目的与边界

本节是“协议完整性、客户端 Bootstrap、物理删除门禁”的证据索引，不是
162-message parity 或旧运行时物理删除完成声明。只记录能改变决策的节点，避免把实现
上下文扩张成叙事。

证据来源固定为三类：

1. **Legacy oracle**：`D:\TRbackup\Version4物理删除了某些文件`，只读行为参考。
2. **Current owner**：NLTX 的 Simulation、Server、Protocol、Transport 实际代码。
3. **Executable evidence**：验证器、诊断日志和 ledger；窄场景通过不扩大为全局完成。

## Flowstate 节点与上下文预算

| 节点 | 当前状态 | 只保留的出口 |
| --- | --- | --- |
| N4 开发 | `in_progress_with_deferred_findings` | 当前批次、owner、下一批 |
| N6 验收 | `passed_scoped_review`（CR-2026-08-23） | 针对性验证和残余风险 |
| N8 迭代 | 下一入口 | 未闭合项进入下一批，不重述历史 |

工作规则：每个方向只提交 `source anchor -> owner -> state transition -> verifier ->
diagnostic` 五个节点；过程上下文目标约为完整讨论的 **10%**。测试预算按核心回归的
**30%** 做变更针对性验证，仍必须覆盖主链路和拒绝边界；不得用少测换取更强完成声明。

## 关键代码依据

| 责任 | 当前代码/参考 anchor | 冻结解释 |
| --- | --- | --- |
| 消息目录 | `src/Terraria.Dome.Protocol.V1456/Protocol/TerrariaMessageId.cs`、`TerrariaMessageCatalog.cs` | `0..161` 目录不等于 parity；`Unsupported` 必须显式保留 |
| 帧/grammar | `Protocol/TerrariaFrame*.cs`、`Packets/TerrariaPacketCodec.cs`、`Compatibility/*` | 无 source anchor 的布局只能是 framing/unsupported |
| 会话与 dispatch | `Session/TerrariaSession.cs`、`Dispatch/TerrariaPacketDispatcher.cs` | 阶段、方向、slot 和拒绝由服务端校验 |
| 隔离与顺序 | `Isolation/DomeNetworkIsolation.cs`、`Network*Envelope.cs`、`Server/DomeServer.cs` | 入站有界队列、递增 sequence、tick 边界 drain |
| socket 生命周期 | `Server/Protocol/TerrariaProtocolSessionHost.cs`、`DomeNetworkUpdateBridge.cs` | Transport 不直接写 Simulation |
| Bootstrap/PVS | `Replication/PlayerBootstrapProjection.cs`、`PlayerStateProjection.cs`、`SessionReplicationState.cs` | DTO/frame 是 projection；PVS、cursor、slow-reader 在边界处理 |
| Legacy protocol oracle | `src/Terraria.Dome.Protocol.V1456/LegacyReference/MessageBuffer.cs`、`NetMessage.cs` | 编译排除的只读副本，不是运行时依赖 |

## 冻结契约

### 身份

- `PlayerHandle`/`NpcHandle` 是 Simulation identity；wire slot/ID 只能经 projection 映射。
- Projectile 保留稳定 replication identity、owner、可选 UUID；owner 由 server/session 校验。
- World Item、Chest、TileEntity 使用稳定整数 ID；恢复/重连不得重新分配。
- UUID、owner、slot、wire identity 的每次转换必须落到 source/authority/projection 证据。

### 状态

复制或持久化状态必须定义 `immutable snapshot`、`revision`、`tick number`、`source
sequence`、`expected revision`。冲突拒绝并保留新状态；编码器不读取可变 ECS 对象。

### 执行

```text
Input -> Validation -> System -> Command -> Deterministic Commit -> Snapshot -> Replication
```

Socket 线程只解析边界并入队；Server 在确定 tick drain；Simulation 是 authoritative
owner；Protocol/Transport 只产生输入或读取 projection；未支持消息显式 `Unsupported`
或受控拒绝；客户端不能覆盖 PVS、ownership、direction、revision。

### 证据

每个交付方向必须有：旧源码文件/member/case 与 hash、当前 owner、输入 DTO/验证结果/
command/commit snapshot/projection、可执行 verifier（通过和拒绝断言）、
`Build/diagnostics/...` 命令/退出码/产物路径。仅代码、单测、矩阵文字或 build 绿灯均
不足以升级为 `Complete`/`ReplacedWithEvidence`。

## 验收门禁（30% 定向集）

入口：

- `Test/Terraria.Dome.FullClientBootstrap.Verification`
- `Test/Terraria.Dome.Protocol.Compatibility.Verification`
- `Test/Terraria.Dome.Hardening.Loopback.Verification`

必须保留的最小场景：malformed/非法长度、unknown/unsupported 显式拒绝、rapid reconnect、
slow reader 隔离、Bootstrap 顺序（WorldData→section→NPC→player/equipment/inventory→
complete）、PVS/owner/direction/revision。通过只证明这些场景。

## 物理删除门禁

当前 ledger（CSV 与最新诊断为准）：`535` 行，`ServerRelevant=44`，
`ServerRelevant deferred=44`，`serverRelevantWithoutEvidence=0`，`physical deletion gate=false`。
历史 `57` 仅作 checkpoint，不覆盖当前事实。

删除顺序冻结为：刷新 current manifest/summary/matrix → deferred 归零 → 旧依赖扫描 →
完整构建 → 回放证据闭合 → 才能评估物理删除。不得按文件数、分类名或兼容编码能力删除。

## 当前可声明 / 不可声明

**可声明**：V1456 目录和部分 grammar（含 message 65 `TeleportEntity`、message 66 `PlayerHealOther`、message 67 `Unused67` explicit unsupported、message 69 `ChestName` grammar-only、message 70 `BugCatching` grammar-only、message 71 `BugReleasing` grammar-only、message 72 `TravelMerchantItems` grammar-only、message 73 `RequestTeleportationByServer`、message 74 `AnglerQuest`、message 75 `AnglerQuestFinished`、message 76 `QuestsCountSync`、message 77 `TemporaryAnimation`、message 78
`InvasionProgressReport`、message 80 `SyncPlayerChestIndex`、message 81 `CombatTextInt`、message 84 `PlayerStealth`、message 92 `SyncExtraValue`、message 96 `TeleportPlayerThroughPortal`、message 99 `MinionRestTargetUpdate`、message 100 `TeleportNpcThroughPortal`、message 102 `NebulaLevelupRequest`、message 103 `MoonlordHorror`、message 104 `ShopOverride`、message 105 `GemLockToggle`、message 106 `PoofOfSmoke`、message 108 `WiredCannonShot`、message 109 `MassWireOperation`、message 110 `MassWireOperationPay`、message 112 `SpecialFX`、message 113 `CrystalInvasionStart`、message 114 `CrystalInvasionWipeAllTheThingsss`、message 115 `MinionAttackTargetUpdate`、message 116 `CrystalInvasionSendWaitTime`、message 117 `PlayerHurtV2`、message 118 `PlayerDeathV2`（variable reason opaque）、message 119 `CombatTextString`（variable text opaque）、message 120 `Emoji`、message 121 `TEDisplayDollDataSync`（variable tile-entity data opaque）、message 122 `RequestTileEntityInteraction`、message 123 `WeaponsRackTryPlacing`、message 124 `TEHatRackItemSync`、message 125 `SyncPlayerChestLocation`、message 126 `SyncRevengeMarker`、message 127 `RemoveRevengeMarker`、message 128 `LandGolfBallInCup`、message 130 `FishOutNPC`）有
source-backed 实现；当前支持事件族的显式
invasion spawn table、Bootstrap、malformed、
slow-reader、rapid-reconnect 有验证入口；network isolation、session ownership、PVS、
revision cursor 已有运行时边界；删除 ledger 分类完整。

**不可声明**：legacy 全量 invasion spawn table；Boss/full AI；完整 162-message parity（本次
仅闭合 message 78）；
所有 compatibility 字段均已迁移为 Simulation 语义；`ServerRelevant deferred=0`；Version4
旧运行时可物理删除。

## 下一 checkpoint

N4 当前批次：NPC Task 9 explicit invasion spawn table owner；随后 Task 5 behavior family。
任何新消息族必须先补五节点证据，再进入 N6；deferred 未归零前保持删除 gate 关闭。
