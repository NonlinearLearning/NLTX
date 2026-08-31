# NLTX 与旧 TerrariaServer 整体框架迁移对比

**审查日期：** 2026-08-28  
**当前项目：** `D:\TRbackup\NLTX`（NLTX / Dome ECS）  
**对比项目：** `D:\TRbackup\无任何删减通过编译`（旧 TerrariaServer 单体项目）  
**审查方式：** 先盘点目录、项目边界和源码规模，再逐段阅读两边的主要大文件、对应新模块、迁移矩阵和差分报告。旧代码实际阅读了 `NPC.cs`、`WorldGen.cs`、`Main.cs`、`Projectile.cs`、`Player.cs`、`Item.cs`、`Recipe.cs`、`MessageBuffer.cs`、`NetMessage.cs`、`Chest.cs`、`Wiring.cs` 的声明、更新/生成/网络关键区段；新代码阅读了 `DomeSimulation.cs` Tick 管线、`DomeServer.cs` 主机循环、NPC/Projectile/Item/WorldGeneration 系统、协议编解码和 WLD 读取器。

## 结论先行

当前项目已经完成了**框架级拆分和一批可执行的服务端权威切片**，但还没有完成旧项目整体行为迁移。推荐同时使用以下三个数字，避免把“文件拆开”误报成“功能完成”：

| 指标 | 估算结果 | 含义 |
|---|---:|---|
| 结构/框架迁移度 | **约 72%–78%** | God Object 已拆为 Simulation、Server、Protocol、Transport、WorldFile、Compatibility；命令/组件/系统/快照/提交边界已形成，但仍有巨型 `DomeSimulation.cs`、协议 LegacyReference 和未收口的兼容层 |
| 主要旧大文件的责任/行为迁移度 | **约 32.2%** | 对 11 个主要旧文件按源码行数加权，并按“已拥有当前责任切片”保守评分；`partial`/`deferred` 不按完整迁移计分 |
| 旧全量源码文本量对应比例 | **16.0%** | 当前 81,457 行 / 旧项目 508,093 行；仅是规模比，不能表示语义覆盖率 |

**一句话判断：** NLTX 是“架构迁移已成形、服务端核心切片约三成、完整 Terraria 行为仍明显未迁移”的状态，不能称为整体完成或旧版等价替换。

## 1. 基线规模与框架差异

### 1.1 旧项目：单体 God Object

旧项目 `TerrariaServer.csproj` 直接组织 `Terraria` 及大量子命名空间。核心运行时状态集中在静态数组和巨型实体类中：

| 旧文件 | 行数 | 代码形态 | 关键观察 |
|---|---:|---|---|
| `Terraria/NPC.cs` | 97,097 | `NPC : Entity`，实例状态、生成器、目标选择、AI、掉落、网络字段混合 | `AI()` 内直接读写 `Main.player[]`、`Main.tile[,]`，包含大量 `AI_###` 分支 |
| `Terraria/WorldGen.cs` | 88,439 | 静态世界生成/Tile 变异 God Object | 生成、陨石、秘密种子、地形、洞穴、结构、液体副作用混在一起 |
| `Terraria/Main.cs` | 67,447 | 全局状态、客户端、服务端循环、时间、输入、渲染和数组所有者 | `Main.Update`、`UpdateTime`、`UpdateServer` 与 UI/音频/图形共享全局状态 |
| `Terraria/Projectile.cs` | 77,390 | `Projectile : Entity`，字段数组 + `Update` + `AI` | `Update(int)` 处理边界、AI、风、水、主人生命周期和网络标记 |
| `Terraria/Player.cs` | 57,801 | 输入、移动、生命、背包、装备、Buff、UI、网络全混合 | `Update(int)` 直接依赖 `Main`、TileObject、Mount、事件和客户端状态 |
| `Terraria/Item.cs` | 49,766 | 物品字段、SetDefaults、前缀、使用、绘制数据 | 大量静态表和 UI/声音/前缀逻辑没有独立权威边界 |
| `Terraria/Recipe.cs` | 16,873 | 配方表与配方计算 | 当前没有同等完整的配方域 |
| `Terraria/MessageBuffer.cs` | 4,500 | 读缓冲、握手、状态门禁、162 类消息分派 | `GetData` 直接操作 `Netplay`、`Main`、`NetMessage` |
| `Terraria/NetMessage.cs` | 3,038 | 静态发送、压缩 Tile、对象和玩家投影 | `TrySendData`/`SendData` 直接读取全局实体数组 |
| `Terraria/Chest.cs` | 3,746 | 箱子状态、槽位、坐标和网络辅助 | 状态直接挂在旧世界数组上 |
| `Terraria/Wiring.cs` | 3,537 | 静态线逻辑、Tile 变更和副作用 | 线、执行器、泵、逻辑门和世界写入耦合 |

以上 11 个文件合计 **469,634 行**，占旧项目 508,093 行源码的主要部分。旧 `Main.cs` 的声明还显示了大量客户端专属字段（UI、地图、图形、声音、输入），所以不能把所有旧行都当作服务端迁移目标；同时也不能因为客户端被排除就宣称服务器行为已经完整。

### 1.2 当前项目：多项目 + ECS/命令/快照

当前 `src` 的可编译源码（剔除 `src/**/Build`）为 **1,325 个 `.cs`、81,457 行**，分布如下：

| 当前项目 | 文件数 | 行数 | 主要责任 |
|---|---:|---:|---|
| `Terraria.Dome.Simulation` | 1,058 | 50,316 | Arch ECS、组件、系统、命令队列、Tick、快照、世界/NPC/Projectile/Item/Wiring/Liquid |
| `Terraria.Dome.Protocol.V1456` | 168 | 15,668 | V1456 编解码、兼容投影、隔离层、消息目录、协议会话 |
| `Terraria.Dome.Server` | 46 | 6,994 | 主机循环、会话、复制、持久化、启动和输入验证 |
| `Terraria.WorldFile.V319` | 32 | 7,696 | WLD v1-v319 读取、Tile RLE、NPC/箱子/标牌/TileEntity 读取 |
| `Terraria.WorldCompatibility` | 12 | 738 | Legacy world 到 Dome 模型的投影 |
| `Terraria.Dome.Transport` | 4 | 35 | 传输边界 |

新框架的明确落点包括：

- `DomeSimulation.Tick` 的命名阶段：`ApplyWorldClock -> ApplyPlayerInputs -> ApplyPlayerControl -> ResolveTileCollision -> SelectNpcTargets -> ApplyNpcAi -> MoveEntities -> AdvanceProjectiles -> ResolveCombat -> CommitDomainCommands -> PublishSnapshot`。
- `DomeServer` 负责网络和会话，不直接把 Legacy `Main`/`Netplay` 引入 Simulation。
- 世界写入通过 `TileChangeCommand` 和确定性 commit；对外发布不可变 snapshot，而不是暴露旧的全局数组。
- 协议、WLD LegacyReference 被 `.csproj` 明确排除编译，作为行为 Oracle/来源证据，不是假装已经接入运行时。

## 2. 逐大文件迁移对比与估算

评分含义：`0` = 没有对应责任落点；`100` = 该文件服务端相关责任已由当前项目拥有并有完整行为证据。`partial`、`deferred`、仅有声明/兼容副本均不算完整。分数是审查估算，不是项目内既有验收分数。

| 旧主要文件 | 当前对应框架 | 证据与已迁移切片 | 明确缺口 | 责任迁移估算 |
|---|---|---|---|---:|
| `NPC.cs` | `Simulation/Npc/{Components,Definitions,Systems,Snapshots}` | `NpcBehaviorRegistry`、目标选择、生成身份/位置、移动碰撞、死亡/反向投影、首个确定性掉落；NPC 覆盖表将这些标为 verified | 旧 `AI_###` 家族、完整类型表、LOS、自然生成率/区域、Boss AI、对话/房屋、全量 loot/events 仍 deferred/excluded | **28%** |
| `WorldGen.cs` | `WorldGeneration/` 493 个文件 + `WorldGenerationPipeline` | 分阶段 Terrain/Cave/Ore/Tree/Structure/Liquid、命令提交、可重放 trace、cursor restart | fresh differential 仍有约 319 万 / 504 万 Tile mismatch，约 104.7 万 extended-state mismatch；559 个方法 unmapped，`canRemoveLegacyWorldGen=false` | **36%** |
| `Main.cs` | `DomeSimulation` + `DomeServer` + World/Clock/Replication | 服务端启动、Tick 边界、时间/天气/侵袭/Slime Rain、玩家/NPC/Projectile/Item 部分生命周期、快照发布 | 客户端排除是设计决策；完整 initializer、`Main.Update` 顺序、任意 delayed process、静态表、完整事件/保存调度、客户端渲染仍未等价 | **32%** |
| `Projectile.cs` | `Projectile/Definitions` + `Projectile/Systems` | 定义注册、spawn/owner/identity、有限数值校验、线性/重力和若干 legacy aiStyle 投影、Tile 碰撞/穿透/复制切片 | 旧 AI 家族、反射/弹跳、液体/斜坡、完整 hostile-player combat、全生命周期和 VFX/network side effects | **24%** |
| `Player.cs` | `Player/Components`、`Player/Systems`、`Players`、Server Protocol | identity/lifecycle、输入/控制、移动/碰撞、生命法力、死亡/重生、背包/物品使用、持久化、协议投影 | 装备完整语义、全量 Buff、坐骑/翅膀/钓鱼/高尔夫/绳索/Emote/mod hooks、客户端表现 | **38%** |
| `Item.cs` | `Items/{Definitions,Components,Systems,Snapshots}` + Inventory | immutable definition、使用/恢复/法力/Buff/Projectile、库存 transfer/split/merge、世界物品 authority、prefix/variant 的部分切片 | 旧 SetDefaults 全表、完整前缀、装备/商店/视觉/UI、shimmer/encumbrance、完整掉落和网络语义 | **42%** |
| `Recipe.cs` | 无同等完整 Recipe 域 | 只有物品定义/使用等间接基础 | 配方表、配方组、条件、制作消耗和 UI 语义未见同等级替代 | **5%** |
| `MessageBuffer.cs` | `Isolation/MessageBuffer` + `DomeNetworkUpdateBridge` | framed input、长度/分片校验、握手边界、命令入队、unsupported diagnostics | 旧 162 message 全分支未迁移；bootstrap 仍有明确 direct writer 边界 | **60%** |
| `NetMessage.cs` | `Isolation/NetMessage` + `TerrariaPacketCodec` + replication | framed imperative output、不可变 outbound envelope、PVS、对象/玩家/Tile/箱子等初始投影 | 完整 `SendData` message parity、Tile 压缩全语义、旧全局数组副作用和所有客户端包 | **58%** |
| `Chest.cs` | `WorldObjects/Chest` command/system/snapshot | 40 槽、范围/独占 opener、坐标索引、原子 transfer、重命名、revision、持久化和重连 | 银行/商店语义、全旧客户端回写和所有 TileObject side effects | **70%** |
| `Wiring.cs` | `Wiring/{Components,Definitions,Systems,Commands}` | 单色线遍历、预算、压力板-门/灯、执行器、泵、逻辑门和 Liquid commit | 传送器/炮/Hopper/PixelBox、完整 TileObject framing、生成期和客户端副作用 | **35%** |

### 2.1 加权计算

按旧文件源码行数加权（不是按当前新文件数量加权）：

```text
sum(old_major_lines) = 469,634
sum(old_major_lines * estimated_score) / 469,634
= 32.2%
```

这个 32.2% 是最接近“从旧主要大文件责任出发”的整体行为迁移估算。它会惩罚 `NPC`、`WorldGen`、`Projectile` 这类代码量大但仍有大量未映射分支的领域，也不会因为新项目拆出了很多小文件就自动增加分数。

## 3. 框架迁移度为何高于行为迁移度

结构分数按四个可观察维度估算：

| 结构维度 | 观察结果 | 估算 |
|---|---|---:|
| 项目/层次边界 | 6 个 net10 项目，Simulation 与 Protocol/Server/WorldFile 依赖方向清楚，LegacyReference 编译隔离 | 85% |
| 状态模型 | ECS component、typed command、immutable snapshot、revision/identity guard 已广泛使用 | 78% |
| Tick/提交/复制边界 | Tick phase、commit-before-publish、PVS、输入验证已有聚焦 verifier | 75% |
| 旧责任收口 | Main/NPC/Projectile/WorldGen 大量行为仍 deferred；`DomeSimulation.cs` 仍约 7,264 行，协议 LegacyReference 仍是 Oracle | 55% |

取保守区间后，框架/边界迁移约 **72%–78%**。这回答的是“代码框架有没有迁到新架构”，不是“Terraria 是否已经能完整运行”。

## 4. 已验证事实与不能外推的事实

### 已验证/有直接证据

- `docs/research/2026-08-18-npc-migration-coverage.md` 明确把 NPC 身份、生成、目标、行为注册、移动、死亡等标成 verified，但把完整 `AI_###`、Boss、对话、全量网络列为 excluded/partial。
- `docs/research/2026-08-20-main-member-coverage-matrix.md` 明确区分 `accepted`、`deferred`、`excluded`，并说明这不是 gameplay-parity percentage。
- `docs/protocol/message-buffer-net-message-status.md` 明确指出 framed path 已可执行，但 full legacy message parity 仍 partial。
- `docs/worldgen/worldgen-parity-report.md` 明确记录 WorldGen differential 失败，且 `canRemoveLegacyWorldGen=false`；这直接否定“WorldGen 已完整迁移”。
- `docs/research/2026-08-18-wiring-liquid-chest-coverage.md` 明确规定 verified 只适用于已验证切片，不能外推为旧 God Object 完整等价。
- `docs/migrations/player-legacy-behavior-map.md` 将 Player 分为 Migrated、Delegated、ClientOnly、Blocked，说明 Player 不是全量完成。

### 不能外推

- 新项目 `.cs` 数量远大于旧项目，不等于功能更多；其中大量小文件是组件、命令、快照、验证器和边界卡片。
- LegacyReference 文件存在，不等于它们在运行时执行；两个项目文件显式 `Compile Remove`。
- focused verifier/build 通过，只证明对应切片和契约，不证明完整 NPC/Projectile/WorldGen/Main parity。
- 旧客户端职责被排除，不应计入服务端 ECS 的缺口；但服务端依赖的旧静态表、事件顺序、掉落和网络消息仍必须单独计入缺口。

## 5. 风险排序与下一步抓手

1. **WorldGen 是最大结构/语义风险。** 先以现有 differential 和 559 unmapped 方法为清单，按 mismatch 最大的 TileType/IsActive/Wall/Liquid/Frame 分区收敛；在 parity gate 关闭前不要删除 Legacy Oracle。
2. **NPC/Projectile 的“框架有了、AI 不全”风险最高。** 先补 AI family/type table/碰撞后果/死亡-掉落-复制闭环，再扩展数量；不要用更多 registry 声明代替行为。
3. **Main 的全局初始化和 Tick 顺序仍是系统性风险。** 继续把 deferred initializer、事件和 delayed process 转成 typed owner；保持 `CommitDomainCommands` 先于 `PublishSnapshot` 的已验证不变量。
4. **Recipe 与完整 Item/Equipment 是明显空洞。** 物品使用切片不能代表 `Recipe.cs` 或完整 `Item.cs` 已迁移，应建立独立配方/装备覆盖矩阵。
5. **协议应从“framed isolated path”走向消息族闭环。** 逐族减少 unsupported IDs 和 direct bootstrap writer，同时保留 malformed/slow-reader/reconnect 验证。

## 最终判定

截至 2026-08-28，NLTX 的正确状态标签是：

> **Framework migration: substantially established (约 72%–78%).**  
> **Major-file behavior migration: partial (约 32.2%).**  
> **Full TerrariaServer parity: not achieved; WorldGen、NPC AI、Projectile AI、Main 全局行为、Recipe 和大量客户端/专用机制仍 deferred 或 excluded。**

任何发布说明若只写“已迁移 70%+”，都会把框架拆分度误读为行为完成度；建议对外同时展示上述三个指标及对应证据链接。
