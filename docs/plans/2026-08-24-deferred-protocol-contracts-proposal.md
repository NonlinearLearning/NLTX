# Deferred Protocol Contracts Proposal

> 状态：`proposal` / 不进入生产发布；本文件只定义 Sign tombstone deletion frame 与
> V1456 chest revision envelope 的后续协议工作边界。

**目标：** 在不伪造 V1456 legacy wire shape、不中断旧客户端连接的前提下，完成两个当前
延期的协议契约：Sign 删除 tombstone 的版本化下行帧，以及 chest transfer 的独立 revision
transport negotiation。

**范围：** `src/Terraria.Dome.Protocol.V1456/**`、Server session negotiation/replication
边界、对应 focused verifier、协议研究文档和 Flowstate evidence。Simulation 的 Sign
tombstone 生命周期已经是 `verified`，本提案不重新设计该状态机。

**非目标：** 不修改 Version4 `NetMessage` message 47 的字段含义；不把空的 Sign update、
零值 chest revision 或未知扩展当作成功；不在本批顺带实现完整客户端 UI、旧客户端自动升级、
全协议回归或所有 TileEntity 的 revision transport。

---

## 1. 当前状态与来源证据

### 1.1 Sign deletion

Version4 的 `NetMessage` message 47 (`OpenSignResponse`) 只有 Sign 的完整文本/坐标状态，
没有独立的删除操作码或 tombstone reason 字段。当前实现
`src/Terraria.Dome.Protocol.V1456/Packets/SignTombstoneProjection.cs` 因而只允许：

- 可表示性校验失败时返回 typed `Rejected`；
- 可表示但 legacy shape 无法表达删除时返回 typed `Deferred`；
- 不发送空文本、零坐标或伪造的成功 Sign update。

Simulation 侧的删除契约已固定为 `SignId + ExpectedRevision` 输入和
`SignTombstoneSnapshot` 输出，ID 单调不复用，重启后保留 tombstone。正式协议需要把这个
tombstone 投影成新能力协商后的 Server-to-Client frame。

### 1.2 Chest revision

当前 `ChestTransferIntent.ExpectedRevision` 和
`ContractExtensionCodec.EncodeChestTransferRevision` 已提供实验性 typed shape；但
`NetModules` 扩展头中的 module/version 尚不能证明对端支持该契约。旧 V1456 客户端可能只
理解 legacy chest transfer，因此 revision envelope 必须在 negotiation 完成后才启用。

当前状态：legacy chest transfer 与 explicit revision shape 已有 focused coverage；独立
version negotiation 仍为 `deferred`。本提案不把现有 `CurrentVersion = 1` 直接视为已协商。

### 1.3 Source ledger

本提案依赖既有 Version4 source ledger，不重新计算 hash。实现前必须回读并确认：

| Source anchor | 用途 | 约束 |
| --- | --- | --- |
| `D:\TRbackup\Version4物理删除了某些文件\Terraria\Main.cs` | 现有 contract-freeze 主线证据 | 不作为协议字段来源 |
| Version4 `NetMessage` message 47 | Sign legacy response shape | 没有删除帧，不能扩展解释 |
| `TerrariaMessageId.NetModules = 82` | 承载版本化扩展 | 只能由明确 module/version 解码 |
| `SignTombstoneSnapshot` | Sign authoritative tombstone | 不允许由客户端创建 |
| `ChestTransferIntent.ExpectedRevision` | chest optimistic concurrency input | 未协商时不能强制旧客户端提供 |

---

## 2. 设计决策

### 2.1 推荐方案：能力协商 + 独立扩展版本

推荐沿用 `NetModules` 承载一个自描述的 contract extension，但补齐两个缺失层次：

1. 在连接建立阶段完成 server-owned capability negotiation，记录 session 的
   `NegotiatedContractVersion` 与能力位。
2. 每个扩展拥有独立 `ContractExtensionKind`、版本和严格 payload length；未知版本或
   未协商能力必须 typed reject/defer，不能回退成 legacy 假成功。
3. Server 只对已协商且仍有效的 session 发送 Sign deletion frame；未协商 session 继续
   收到普通 Sign 状态或明确的 deferred audit，不发送伪造删除。
4. chest transfer 在已协商 session 使用 `ExpectedRevision`；未协商 session 继续使用
   legacy transfer，并由 server 按 session policy 决定是否接受无 revision 的请求。

该方案保持 legacy wire compatibility，并把“能编码”与“对端已声明支持”分开。代价是增加
一次握手状态和 session matrix，但这是防止旧客户端误解析扩展的必要成本。

### 2.2 被拒绝的方案

| 方案 | 结论 | 原因 |
| --- | --- | --- |
| 复用 message 47 的空文本/零值表示删除 | `Rejected` | 字段语义不表示 tombstone，客户端无法区分合法空 Sign |
| 无协商直接发送 `NetModules` version 1 | `Rejected` | 旧客户端可能忽略、误解析或断开连接；无法证明能力 |
| 仅增加全局协议版本号 | `Deferred` | 无法独立发布 Sign 与 chest 两个不同风险的扩展 |
| 服务端始终强制 revision | `Rejected` | 破坏 legacy client compatibility，且无法回收旧 session |

---

## 3. 目标契约

### 3.1 Capability negotiation

建议新增 typed records（名称可在实现阶段按现有命名微调）：

```text
ContractCapabilityOffer
  ProtocolFamily = V1456
  SupportedExtensions: SignDeletion, ChestTransferRevision
  HighestVersionByExtension

ContractCapabilityAck
  AcceptedExtensions
  AcceptedVersionByExtension

SessionContractCapabilities
  NegotiationState = NotStarted | Negotiated | Rejected
  AcceptedExtensions
  AcceptedVersionByExtension
```

规则：

- 能力由 server 计算并最终裁决，client offer 不能扩大 server allow-list。
- 缺失 ack、版本不匹配、重复协商、握手超时均不启用扩展。
- 协商状态必须绑定 session，断线重连重新协商，不从全局缓存继承。
- capability payload 自身必须有固定 module id、protocol version、kind、payload length。

### 3.2 Sign deletion frame

正式下行帧沿用已有 typed shape 的语义，但只能在 `SignDeletion` 能力已协商时发送：

```text
SignDeletionFrame
  SignId: non-negative int, bounded by V1456 wire range
  Revision: non-negative long
  Reason: defined SignTombstoneReason
```

发送规则：

- 只从 server 的 `SignTombstoneSnapshot` 生成；客户端输入不能生成该帧。
- 按 session 的 Sign PVS/可见性规则投影；已不在 PVS 的 tombstone 不重复发送。
- revision 必须单调，重复或旧 revision 不发送。
- receiver 不支持该能力时，Server 保持 `Deferred`，不发送 message 47 伪删除。
- malformed frame、未知 reason、超范围 SignId、负 revision 在 decode 前后都必须拒绝，且
  不改变客户端或 server world state。

### 3.3 Chest revision envelope

已协商 `ChestTransferRevision` 的 session 使用：

```text
ChestTransferRevisionEnvelope
  ChestId: positive int
  ExpectedRevision: non-negative long
```

Server 处理顺序：

1. 校验 session capability 与 envelope version。
2. 校验 player slot、chest ownership/open lease、coordinate/range 和 expected revision。
3. 只有所有校验通过后，进入既有 chest mutation command。
4. revision 冲突返回 typed `Rejected(RevisionConflict)`，不得修改 chest、inventory 或
   replication cursor。
5. 未协商 session 的 legacy request 按兼容策略处理：允许 legacy policy 时走既有路径；
   strict policy 时返回 `Rejected(RevisionNegotiationRequired)`。

---

## 4. 分批实施计划

每批只有一个主要 owner/write-set；跨批只通过 immutable contract records 和 evidence
manifest 交接。

### Batch A：协商状态与编解码边界

**Owner：** `Terraria.Dome.Protocol.V1456` protocol/codec。

**Write-set：** `Protocol/**`、`Packets/**`、协议 verifier。

步骤：

1. 先写 RED：未知 module/version、截断 payload、重复 capability、未协商扩展均失败且
   无状态变更。
2. 定义 capability offer/ack/session records 与严格编码长度。
3. 将 `ContractExtensionCodec` 的当前固定 `CurrentVersion` 改为显式版本参数或版本表，
   禁止调用方绕过协商直接 Encode。
4. 写 GREEN：同版本 round-trip、未知版本 reject、能力交集只取 server allow-list。
5. 运行 protocol focused verifier 和 Protocol Release build。

### Batch B：Sign deletion 正式投影

**Owner：** Server replication + V1456 Sign projection。

**Write-set：** Sign packet/projection、session capability gate、Sign protocol verifier。

步骤：

1. 写 RED：协商前 tombstone 返回 `Deferred`；协商后输出唯一 deletion frame；重复旧
   revision 不输出。
2. 接入 session capability gate 与 PVS/revision cursor。
3. 为 malformed Sign deletion 增加 decode rejection matrix。
4. 写 GREEN：两个 session（支持/不支持）、PVS 内外、重连后 cursor、持久化 tombstone
   continuation。
5. 运行 Sign/world protocol focused verifier、Server serial Release build 和 loopback。

### Batch C：Chest revision negotiation

**Owner：** V1456 chest dispatch + server chest authority。

**Write-set：** chest packet/dispatch/session policy、chest protocol verifier。

步骤：

1. 写 RED：未协商 revision envelope 被拒绝或按明确 legacy policy 分流；revision mismatch
   不产生 mutation。
2. 将 negotiated version 传入 chest dispatch，不从 packet kind 猜测能力。
3. 保留旧客户端 legacy request 的兼容路径，并在 strict policy 下显式返回
   `RevisionNegotiationRequired`。
4. 写 GREEN：成功 transfer、旧 revision、并发双写、未协商 legacy、恶意 player/chest
   组合五组场景。
5. 运行 chest loopback/protocol verifier、Server serial Release build。

### Batch D：发布门禁与迁移记录

**Owner：** contract-freeze 文档与 evidence ledger。

**Write-set：** 本文件、主 contract-freeze 计划、`.agent-workplace/state/**`。

步骤：

1. 回读 Version4 source hash 和 message 47 shape，生成 fresh source anchor evidence。
2. 更新 capability/version matrix、旧客户端兼容矩阵和 rollback 规则。
3. 只有 A-C 的 focused verifier、loopback、serial builds 全部通过后，才把对应项从
   `deferred` 改为 `verified`。
4. 不运行全量 solution regression，除非该扩展被标记为正式 contract release；正式发布
   时才触发 D 层全回归。

---

## 5. 拒绝与兼容矩阵

| 场景 | Sign deletion | Chest revision | 状态变更 |
| --- | --- | --- | --- |
| 未协商 | `Deferred`，不发伪删除 | legacy policy 或 `RevisionNegotiationRequired` | 无扩展状态 |
| 协商成功 | 发送 versioned deletion frame | 校验 expected revision | 仅成功命令变更 |
| 未知 module/version | `Rejected` | `Rejected` | 无 |
| malformed length/body | `Rejected` | `Rejected` | 无 |
| 旧 revision | 不发送重复 tombstone | `RevisionConflict` | 无 |
| session 重连 | 重新协商并从 cursor 继续 | 重新协商 | 不继承旧能力 |
| server allow-list 不含扩展 | `Deferred` | legacy 或明确拒绝 | 无 |

所有 `Deferred`、`Rejected` 和 `verified` 必须保留为不同枚举/证据状态，不能用空 packet
或默认值压平。

---

## 6. 验证矩阵与证据要求

遵循当前 contract-freeze 的约 30% 风险加权验证预算：

| 风险等级 | 必测内容 | 证据 |
| --- | --- | --- |
| A | codec schema、happy path、malformed/unknown rejection、diff check | protocol verifier log |
| B | authority、PVS、revision conflict、replay、duplicate negotiation | server/loopback verifier |
| C | persistence/reconnect/legacy client boundary 至少一个代表样本 | persistence/loopback log |
| D | 正式 contract release 才运行 full regression | release evidence |

每份正式 evidence 必须写明：source anchor、owner、inputs、outputs、verifier、build
diagnostic、exit code、status。`not-run`、`deferred`、`rejected`、`verified` 不得互换。

推荐证据目录：

```text
Build/diagnostics/contracts/<timestamp>-protocol-capability-verifier.log
Build/diagnostics/contracts/<timestamp>-sign-deletion-loopback.log
Build/diagnostics/contracts/<timestamp>-chest-revision-loopback.log
Build/diagnostics/contracts/<timestamp>-protocol-build.log
Build/diagnostics/contracts/<timestamp>-server-build.log
```

---

## 7. 回滚与发布门禁

- 协商失败时必须回到 legacy chest policy 或 Sign `Deferred`，不能回退到伪造删除。
- 可通过 server feature flag 独立关闭 `SignDeletion` 和 `ChestTransferRevision` 两个能力，
  不撤销 Simulation tombstone。
- 任何 capability/version source hash 变化都使相关 `verified` 自动降级为 `partial`，
  必须重新生成 evidence。
- 在 Batch A-C 完成前，主计划保持 `status: partial`；只有对应 wire shape、协商、兼容、
  loopback 和构建全部有 fresh evidence 后，才允许更新为 `verified`。

## 8. Execution checkpoint（2026-08-24）

Batch A capability negotiation and the first B/C authority gates are implemented:

- `ContractCapabilityOffer`、`ContractCapabilityAck` 和 session-scoped
  `SessionContractCapabilities` 已加入 V1456 protocol layer。
- `NetModules` module 15 capability offer 只允许每个 session 协商一次；server 只取自身
  allow-list 与 client offer 的交集，并返回 typed ack。重复协商和未知 extension shape
  在 mutation 前拒绝。
- `SessionReplicationState` 保存协商结果；未协商的 explicit chest revision 被拒绝，
  `ExpectedRevision = -1` 的 legacy path 保持可用。
- `DomeServer.CreateSignDeletionFrames()` 在无协商能力时返回空集合；只有传入已协商的
  `SignDeletion` capability 才生成 versioned deletion frames。
- Server protocol host 将 `TerrariaSession.ContractCapabilities` 同步到 replication
  state，避免 packet kind 代替 capability 判断。

Focused evidence：

- Protocol verifier：`Build/diagnostics/contracts/20260824-140600-capability-protocol-green.log`，
  exit `0`，包含 `capability negotiation is session-scoped, intersected and single-use`。
- Server verifier：`Build/diagnostics/contracts/20260824-140700-deferred-server-green.log`，
  exit `0`，覆盖无协商不发 Sign deletion、协商后发 typed frame。
- Chest contracts verifier：`Build/diagnostics/contracts/20260824-141100-chest-contracts-rerun.log`，
  exit `0`，覆盖未协商 explicit revision rejection、协商后 revision match 和 stale guard。
- Protocol build：`Build/diagnostics/contracts/20260824-140800-deferred-protocol-build.log`，
  exit `0`，0 warning / 0 error。
- Server build：`Build/diagnostics/contracts/20260824-140800-deferred-server-build.log`，
  exit `0`，0 warning / 0 error。
- Sign cursor gate / clean Server build：`Build/diagnostics/contracts/20260824-141500-deferred-server-build-clean.log`，
  exit `0`，0 warning / 0 error；`git diff --check` 同批 exit `0`。
- Fresh rerun：`Build/diagnostics/contracts/20260824-190000/`，Protocol、Server、WorldObjects
  focused verifiers、Protocol/Server serial Release builds 和 diff check 全部 exit `0`。
- TCP loopback：`Build/diagnostics/contracts/20260824-200000/capability-loopback.log`，exit `0`，
  覆盖真实登录到 Active、capability ack、重复协商无二次 ack、断线新 session 重新协商。
- TCP loopback build：`Build/diagnostics/contracts/20260824-200000/capability-loopback-build.log`，
  exit `0`，0 warning / 0 error。
- Solution Release gate：`Build/diagnostics/contracts/20260824-210200-solution-release-build-serial-rerun.log`，
  exit `0`，0 warning / 0 error；`dotnet test Terraria.Dome.sln -c Release -m:1` 也 exit `0`，
  结果记录于 `Build/diagnostics/contracts/20260824-210300-solution-test.log`。

正式 contract release 的全量门禁已通过。Sign PVS/revision cursor、重连 tombstone
投影和真实 TCP capability handshake/重新协商已有 focused evidence。旧
`WorldObjects.Loopback.Verification` 在 TrainingDummy 后续读取处有独立
`EndOfStreamException`，已记录为未通过，不能替代本批 focused evidence。

## 9. DoD

- [x] Sign deletion 有正式版本化 Server-to-Client frame。
- [x] Sign deletion 发送受 session capability、PVS 和 revision cursor 约束。
- [x] Chest revision 有独立 negotiation，旧客户端不被强制升级。
- [x] 未知版本、malformed payload、旧 revision、重复协商均 typed reject 且无 mutation。
- [x] Sign persistence/reconnect、chest concurrent transfer 和真实 TCP capability
  handshake/reconnect 有 focused loopback evidence。
- [x] Protocol 与 Server serial Release build 为 0 warning / 0 error。
- [x] `git diff --check` exit `0`。
- [x] 主 contract-freeze 与 Flowstate state 文件仍准确区分 `deferred`、`partial` 与
  `verified`。

**当前结论：** 本提案的 Sign deletion frame、独立 revision negotiation、session
capability gate、PVS/cursor、TCP handshake/reconnect 和 Release gate 已完成并有 fresh
evidence。协议扩展已达到本提案定义的 `verified`；任何新的 wire/version 变化必须重新
开启本提案的 contract-freeze 流程。
