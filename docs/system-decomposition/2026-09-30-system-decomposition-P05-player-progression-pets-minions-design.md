# P05 玩家进度、召唤物、宠物与伙伴 System 边界设计

文档 ID：DOC-2026-09-30-system-p05-player-progression-pets-minions-design  
逻辑域：system-decomposition  
产物类型：design  
状态：draft  
设计状态：proposed  
证据状态：partial  
验证状态：partial（minion/capacity/fishing 与多组 pet capability 局部核心有 focused verifier 记录；world-object pet、legacy pet/effect、accessory tick、companion rebuild/reset、Golfer/Angler/DD2/unlock-preference reducer 和 progression persistence snapshot mapping 有独立局部验证；完整 save/load、packet 行为及 P05 行为矩阵 `not-run`）  
范围：authoritative P05；17 个叶子组、164 个字段、0 个属性  
证据入口：[P05 System 拆分报告](reports/2026-09-18-system-decomposition-authoritative-P05-player-progression-pets-minions.md)  
关联执行计划：[P05 System 执行计划](../plans/system-decomposition/2026-09-30-P05-player-progression-pets-minions-system-execution-plan.md)  
canonical 路径：`docs/system-decomposition/2026-09-30-system-decomposition-P05-player-progression-pets-minions-design.md`

本设计是已结算 P05 报告的下游设计材料，不是新的 partition claim、实现记录或迁移验收。原报告的 `sessionId` 为 `65bb838ff91b4927a81ee6d3e1500515`；原报告状态和证据限制继续适用。

## 决策摘要

- 将字段按可观察行为和生命周期归入少量能力边界，不按 17 个台账分组机械创建 17 个 System，也不把全部宠物、召唤物和伙伴状态塞进一个大 Component。
- 把持久进度提交、每 tick 派生能力重建、Projectile admission / lifecycle、Combat 观测、Mount / Minecart、持久化和网络投影视为不同的责任边界。
- 持久进度与 tick snapshot 可以提出 P05 owner 候选；minion 容量、pet 生命周期、damage provenance、companion entity 和 Mount / Vehicle 的最终 owner 仍须跨分区确认，并标记 `crossSubsystemOwner: integration-review`。
- Query 只供读取或预判，不作为容量授权、宠物实体创建或进度提交。任何最终修改须落在唯一 owner 或显式协调协议中。
- 迁移、生产调用接入、save/network 兼容和行为等价均未确认；不据此称迁移成功。

## 数据包 API 边界

P05 数据包相关 API 只定义函数签名，不编写具体函数体。此限制覆盖数据包字段编码/解码、序列化/反序列化、协议校验、传输、发送、广播及应用到权威状态的实现。现有 progression reducer、Query 与本地状态提交逻辑不属于数据包实现。

目标 Version4 `NetMessage.cs:179-192` 与 `MessageBuffer.cs:275-287` 显示 10 个 P05 bool 字段的位读写对应关系。限定到 `Player.cs` / `NetMessage.cs` / `MessageBuffer.cs` 的 CPG symbol 与 member-use 查询对这 10 个字段均为 `complete`；每字段有 3 至 5 个 member-use facts，部分访问方向为 `Unknown`，数据库 `SourceSnapshotId=null`。这些材料不能确认完整协议版本、调用者、默认值、错误语义或生产顺序。用户指定的完整参考项目在其独立快照中也显示同一组字段位映射，只作为关系调查线索，不用于补齐目标 Version4 契约。

据此，新增 `IPlayerProgressionPacketProjection.Project` 签名，输入现有 unlock / consumed ledger，返回 `PlayerProgressionPacketFlags`；该 DTO 只承载上述 10 个 P05 bool 字段，接口没有实现类或方法体，也不负责 `UsingBiomeTorches`、`happyFunTorchTime` 等其它 Player 包字段。Version4 当前源码明确给出这些字段在该快照中的槽位：`NetMessage.cs:181-183` 写 `bitsByte21[2-4]`，`NetMessage.cs:186-192` 写 `bitsByte22[0-6]`；`MessageBuffer.cs:277-279,281-287` 以相同槽位读回。该映射是源码观察结果，不由 DTO 或 API 实现 wire 编码。只读 CPG Query API 对 10 个字段的 symbol 与限定 `Player.cs` / `NetMessage.cs` / `MessageBuffer.cs` member-use 查询均为 `complete`，每字段 1 个 symbol、3 至 5 个 facts；`NetMessage.cs` facts 的访问方向有 `Unknown`，且 manifest `SourceSnapshotId=null`，所以静态 CPG 结果不证明完整 caller、owner 或运行顺序，槽位结论以核对的当前 Version4 源码为准。此 API 状态为 `proposed`。跨版本字段协议、旧包/缺字段默认值、读写失败处理、发送时机、生产调用闭包和权威提交顺序仍为 `unknown`；不据签名声明声称 packet 行为实现、兼容完成或生产接入。字节编解码 API 未定义，packet 行为验证状态为 `not-run`。

目标 `NetMessage.cs` / `MessageBuffer.cs` SHA-256 分别为 `87B596BD8467B9C470DD1F2AD6963EBADE257689F87395163AB136C149BEBC9A` / `0F474225FA9D98C539F61249619273A44E73F69A4CF388B44258433A2D15D5EE`；完整参考快照对应文件 hash 为 `F3066C50715D7C49B8BF2CC852B015303AD7F4C12ADC191C72FC0FE46181E7E2` / `48AABBBF4E0967964D97598E0168E9252ADD3DD868C66C6DA46AA87670EACAAB`。两份快照均显示相同的十项 progression 位映射，但不是同一源码身份。Version4 CPG 对这十个字段的 scoped member-use 请求均为 `complete`：`unlockedBiomeTorches` 4 项、`ateArtisanBread` 3 项、`unlockedSuperCart` 3 项、`enabledSuperCart` 4 项、`usedAegisCrystal` 4 项、`usedAegisFruit` 4 项、`usedArcaneCrystal` 5 项、`usedGalaxyPearl` 4 项、`usedGummyWorm` 4 项、`usedAmbrosia` 4 项；facts 仍混有 `partial` / `Unknown`，且索引未绑定这些源码 hash。

## 持久化字段快照投影

`PlayerProgressionPersistenceProjection.Project` 将 `PlayerUnlockProgressionLedgerComponent`、`PlayerConsumedProgressionLedgerComponent`、`PlayerQuestEventProgressComponent` 和显式 `biomeTorchPreference` 输入映射为只读的 `PlayerProgressionPersistenceSnapshot`。快照含 14 个字段：unlock ledger 的 Biome Torch unlock、Artisan Bread 和 Super Cart 两项；consumed ledger 的六个永久升级 flags；quest/event ledger 的 DD2、Angler、Golfer 状态；以及独立的 Biome Torch preference。此模型是内存中的字段映射，不定义文件布局、字段编码、版本号、旧版本默认值、反序列化、恢复或生产保存调用。显式 preference 输入保留它与 unlock 状态的独立性。

完整参考项目 `D:\TRbackup\无任何删减通过编译\Terraria\Player.cs` 提供了独立快照中的序列化关系线索：`:55377-55386` 依次写入十个连续 progression bool；`:55491` 写入 Angler 计数；`:55508` 写入 Golfer 分数；`:55512-55516` 将 Super Cart unlock/preference 写入 `BitsByte`。反序列化分别受 release 门槛控制：Biome Torch 两项 `>=229`，Artisan Bread `>=256`，六个 consumed flags `>=260`，DD2 `>=182`，Angler `>=98`，Golfer `>=206`；Super Cart bits 为 `>=253`，更旧版本的 unlock 从 inventory 派生（`:56321-56329`）。这些门槛和旧版回填逻辑只属于完整参考快照，未移植到目标 Version4。快照字段顺序也不代表这些 legacy 二进制写入顺序。

目标 Version4 `Player.cs` 中 `Serialize` 是空方法体（`:26418`），`Deserialize` 只给 `gotToReadName` 赋默认值（`:26455-26458`），`FixLoadedData` 为空（`:26460`）。本轮只读 CPG 查询对 `Serialize` / `Deserialize` 符号各返回 1 个 `complete` 结果；`usedAegisCrystal` 的限定 member-use 请求为 `complete`、4 条 facts，范围包括 `Player.cs`、`NetMessage.cs` 和 `MessageBuffer.cs`，访问方向同时包含 `Write` 与 `Unknown`。数据库 manifest 为 `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`、`SourceSnapshotId=null`；这些查询只说明索引范围内事实，不能证明目标存档行为或字段恢复关系。源码 stub 和调用方不足以确认目标格式与迁移兼容性，因此 persistence schema、release mapping、recovery order 和 production caller 均为 `unknown`。

本地 verifier 已覆盖 14 个快照字段的映射，以及 unlock 保持为真时 Biome Torch preference 可独立为真或假。该结果只支持局部投影核心；不验证读写字节、版本门槛、默认值、损坏输入、存档恢复顺序、生产调度或目标行为等价。该本地 snapshot API 不属于 packet API；packet 相关 API 仍只有声明，没有函数体或行为实现。

## 范围与证据

权威输入和 164 个成员的逐字段列表以 P05 报告及其 authoritative ledger 为准。这里归纳 System 边界，不复制 ledger，也不根据 `Terraria.Player` 的声明位置推断 NLTX owner。

| 证据源 | 可支持的结论 | 限制 |
|---|---|---|
| `D:\TRbackup\Version4` | 报告记录的同步调用、直接重置/累加和 Player / Projectile 交互；关键文件 SHA-256 见原报告。 | 该目录不是 Git 仓库；部分方法体为空或 stub，调用、调度、事件、存档与网络闭包不完整。 |
| Version4 CPG | `CpgEvidence.ps1` 对选定 shard 的符号、call-site 和成员访问事实。数据库 manifest SHA-256 为 `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，`SourceSnapshotId=null`。 | 查询 `complete` 只代表所选索引范围和预算内完成；不证明完整调用图、唯一 writer、动态分派或效果闭包，也没有绑定当前源码 hash。 |
| 用户指定的完整参考源码 `D:\TRbackup\无任何删减通过编译` | `Player.cs` 中可见完整的 save/serialize/deserialize 实现，可帮助提出待核实的 legacy 字段与版本问题。 | 这是不同源码快照，不可直接代替目标 Version4 行为证据。本轮未构建该项目；目录名称不作为构建验证。 |
| 当前 `D:\TRbackup\NLTX\src\NSSLC` | 有 progression、fishing、capacity、minion/pet/accessory capability 局部核心与 Mount runtime 候选类型。 | 在当前源码范围内未检索到通用 scheduler、System registrar 或 Player runtime loop；`PlayerTickCoordinator` 只查到 PlayerItemSpaceVerification 调用。此搜索不能证明外部 host 或生成注册路径不存在，生产入口仍为 `unknown`。 |
| SS14 `SharedResearchSystem` / `Content.Shared.Follower.FollowerSystem` | 可作组织参考：System 能组合依赖并提供行为 API；Follower 关系由专门 System 维护 reciprocal state，并处理实体终止与目标替换。 | Follower 是 ghost observer 关系，不是 Terraria pet 行为；不证明 Terraria 的 owner、持久化、事件或调度规则。 |

另对用户指定的完整参考项目进行整树字段名搜索，范围为 1,503 个 C# 源文件。命中分布为：minion capacity/damage 在 `Player.cs`、`Projectile.cs`、`Main.cs`、`ArmorSetBonuses.cs`；minion flags 在 `Player.cs`、`Projectile.cs` 与 buff/projectile id、绘制定义；pet flags 在 `Player.cs` / `Projectile.cs`；progression flags 分布在 `Player.cs`、`Main.cs`、`NPC.cs`、`NetMessage.cs`、`MessageBuffer.cs` 等；fishing 在 `Player.cs` / `Projectile.cs`；accessory 字段另涉及 `Main.cs`、`NPC.cs`、`Projectile.cs`、网络和渲染文件；vehicle 字段涉及 `Player.cs`、`Mount.cs`、`Minecart.cs`、`Collision.cs`。这是名称命中范围，用来选择需直接阅读的源码文件，不等于完整调用闭包或唯一 writer 证明。

补充参考源码的关键 SHA-256：`Terraria/Player.cs`=`367E6C3249F064DD9CCF8E421EBEBF6EE3AA745B6521F07A8BEBA4A3C2FF612C`、`Main.cs`=`E24E61C9903BB7995F47E36C51EDBE43481B643226861B717020877E7E22B63F`、`Projectile.cs`=`8D6428FA1D098B26ADFDC1D6CC028251814B40CD3ACE576F0BFF462FB19D6038`、`Mount.cs`=`3F94D523F50A44498BB3E8FC7F98FA18AD11DE5EA1E65AFAD6E0D81F05040964`、`Minecart.cs`=`9E3284347B3CD0BF4C0497A73CDC2639D3B82994E72DF8BCF1B554F145198818`。其 `Player.cs` 与目标 `Version4/Terraria/Player.cs` 的 SHA-256 不同；两份源码不得拼接成一个已确认快照。

报告指出目标 `Version4/Terraria/Player.cs` 中 `InternalSavePlayerFile`、`Serialize`、`Deserialize` 和 `FixLoadedData` 为空或 stub。补充参考项目的 `Player.cs` 则有完整序列化实现，并可见 consumed-upgrade 和部分进度字段的写入与 release 条件。这只能作为后续版本映射的调查线索；P05 字段在目标 Version4 的存档编码、加载兼容和恢复顺序仍为 `unknown`。

### 本轮只读 CPG 查询

| API 与范围 | 结果 | 可支持内容 |
|---|---|---|
| `Find-CpgCallSites`：`Player.Update(int)`，`Main.cs` / `Player.cs` | `complete`，1 个精确 `CallTargets` 到 `Main.cs` | 所选索引范围内确认 `Main -> Player.Update(int)` 直接边；不是完整 runtime caller 图。 |
| `Find-CpgCallSites`：`ResetEffects()`，`Player.cs` | `complete`，1 个 call-site | 所选 Player shard 的直接调用事实；不推出全项目唯一调度入口。 |
| `Find-CpgCallSites`：`InternalSavePlayerFile`，`Player.cs` | `complete`，1 个 call-site | 只确认 Version4 索引中的调用点；目标方法体仍是 stub，不能证明保存行为。 |
| `Find-CpgCallSites`：`UpdateBuffs`、`UpdateEquips`、`UpdateProjectileCaches`，对应 Player 范围 | 均 `partial`、0 命中且有 gap | 源码可见直接调用；查询零命中不能解释为无调用。 |
| `Get-CpgMemberUses`：`maxMinions`、`numMinions`、`slotsMinions`、`petFlagJunimoPet`，`Player.cs` / `Projectile.cs` | 分别 23、3、3、4 条结果；含 `Unknown` access mode 与 `partial` facts | 所选文件中存在多个读写/形状候选，不能据此关闭 writer 集合或选出唯一 owner。 |
| `Get-CpgMemberUses`：两个 minion damage high-water 字段，Player shard | 每字段 3 条：2 个写入、1 个 access mode unknown；查询状态 `complete` | Player shard 可见重置与最大值更新形状；不闭合 Combat/Projectile 来源与唯一 writer。 |
| 同两字段，Projectile shard；`Find-CpgCallSites`：`UpdateProjectileCaches` | Projectile-only 为 `partial`、0 命中并带 gap；方法 call-site 同为 `partial`、0 命中 | 不是“无读取/无调用”的证据；要保留源码回退和 `unknown`。 |
| `Find-CpgCallSites`：`ResetProjectileCaches`，Player / Main 范围 | 索引状态 `complete`，返回 1 条 Player call-site；当前源码文本可见 2 处直接调用 | CPG 没有绑定源码 snapshot；列表与当前源码不一致，不能当作完整 inbound call set。 |
| `Find-CpgSymbols`：`ItemID.AegisCrystal`，`Terraria.ID/ItemID.cs`；`Get-CpgMemberUses`：Player scope | 字段 symbol `complete`，1 项；member-use `partial`、0 项并带 gap | 常量身份可定位，但 CPG 未给出 Player 的 item-use 数据流；须读源文件。 |
| `Find-CpgSymbols`：`ItemCheck_UseShimmerPermanentItems`，`Terraria/Player.cs` | `partial`、0 项并带 gap；`ItemCheck()` 符号存在 | 不能将零命中解释为没有同类入口；当前 Version4 源码搜索未找到该子方法。 |
| `Find-CpgSymbols` / `Get-CpgMemberUses`：六个 `used*` flags，限定 `Player.cs` / `NetMessage.cs` / `MessageBuffer.cs` | symbol 与 member-use 均 `complete`；member-use 数量依次为 4、4、5、4、4、4 | facts 覆盖 Player、NetMessage、MessageBuffer；`AccessMode` 混有 `Write` / `Unknown`、`EvidenceStatus` 混有 `confirmed` / `partial`。这不确认 Version4 的 true writer 或 ItemCheck 调用；manifest 的 `SourceSnapshotId=null`。 |
| `Find-CpgSymbols` + `Get-CpgMemberUses`：7 个 fishing 字段，`Terraria/Player.cs` 与 `Terraria/Projectile.cs` | 所有 symbol/use 查询均为 `complete`；use 数依次为 8、2、2、4、3、4、3，facts 均落在 Player shard；`accFishingBobber` 有 `partial` / `AccessMode=Unknown` fact | 查询仅覆盖两个选定 shard；未返回 Projectile fact，而完整参考 Projectile 中有直接读取。CPG 不足以证明 target 没有消费者；逐份源码关系见后文。 |

这些查询使用 `D:\TRbackup\Version4-cpg-export\out-dop8-interproc.sqlite`。即使单项状态为 `complete`，也只表示该 API 查询在所选 scope 和预算内完成；数据库没有 source snapshot ID，因此 API 结果不能确认与当前文件 hash 同快照。

完整参考项目 `Player.cs:44117` 在 `ItemCheck()` 中调用 `ItemCheck_UseShimmerPermanentItems(sItem)`；该 helper 位于 `:46048-46086`，对 item type `5337` 至 `5342` 分别检查动画、对应 flag 尚未消费和 `ItemTimeIsZero`，再依序调用 `ApplyItemTime`、置 flag、发送 `NetMessage.SendData(4, ...)`。这是独立完整参考快照的关系证据。目标 Version4 的同名 helper symbol 查询为 `partial` 零命中并带 gap，源码搜索也未找到该 helper 或相应 true 写入；六个 flag 的 CPG member-use facts 不能补齐这一缺口。当前 `ConsumedUpgradeEligibilityQuery` 仅覆盖纯预判，未连接物品时间、状态提交或 packet effect。packet API 保持 signature-only；协议实现、生产调用闭包和行为验证均为 `unknown` / `not-run`。

### 2026-10-01 Mount/Minecart owner 复核

目标 Version4 的源码身份为 `Player.cs` SHA-256=`E5B301E3401F61E37BF3291754670BCC123D6593EDD94597C4E9109C4030FE86`、`Mount.cs`=`2DED2B174731DDDD003BCD1B03F83853D0AD39183CD3683B4C5289459AEDCEFC`、`Minecart.cs`=`6E35F5039BBA399C7D6EB758DA7CB2F1242C141B83E68BB1CF17BAE630EED33F`。用户指定的完整参考快照对应 hash 为 `Player.cs`=`367E6C3249F064DD9CCF8E421EBEBF6EE3AA745B6521F07A8BEBA4A3C2FF612C`、`Mount.cs`=`3F94D523F50A44498BB3E8FC7F98FA18AD11DE5EA1E65AFAD6E0D81F05040964`、`Minecart.cs`=`9E3284347B3CD0BF4C0497A73CDC2639D3B82994E72DF8BCF1B554F145198818`。两组 hash 属于不同源码快照，不拼接成一个行为基线。

只读 CPG Query API 使用 manifest `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，scope 为 `Player.cs`、`Mount.cs`、`Minecart.cs`、`Collision.cs`、`NetMessage.cs` 和 `MessageBuffer.cs`；数据库 `SourceSnapshotId=null`：

| Player 字段 | symbol / member-use | 结果及限制 |
|---|---|---|
| `onWrongGround` | `complete` / `complete`，7 facts | 全在 Player shard，facts 为 `partial` / `AccessMode=Unknown`。 |
| `onTrack` | `complete` / `complete`，6 facts | 来自 Player 与 Collision shard，facts 为 `partial` / `AccessMode=Unknown`。 |
| `cartRampTime` | `complete` / `partial`，0 facts | `NoMatchingFactInScannedScope`；与源码中的直接读写命中不一致，保留 evidence gap。 |
| `cartFlip` | `complete` / `complete`，2 facts | Mount shard 中为 `confirmed` / `Write`，但不闭合所有调用与清理路径。 |
| `trackBoost` | `complete` / `complete`，3 facts | Player shard facts 混有 `confirmed` / `partial` 和 `Write` / `ReadWrite` / `Unknown`。 |
| `lastBoost` | `complete` / `complete`，2 facts | Mount shard 中为 `confirmed` / `Write`；Minecart 的 ref 参数效果需结合源码理解。 |
| `mount` | `complete` / `partial`，返回上限 200 facts | 出现 `ItemBudgetExhausted`；返回 facts 有 `partial` / `Unknown`，覆盖 Player、Mount、NetMessage、MessageBuffer shard，不能据此确定完整消费者或 owner。 |

Version4 源码显示这六个 scalar 并非同一 `ResetEffects` snapshot：`onWrongGround` 在 `Player.cs:14805-14808` 有条件清除、在坐骑地面检查 `:16240-16255` 更新；`onTrack` 在轨道碰撞入口先清除、再按碰撞位恢复 `:17481-17503`；`cartFlip` 随 bumper 输入翻转 `:17510-17521`，且 `Mount.cs:4745-4749,4793-4797` 在车辆切换时清除；`cartRampTime` 在 `Player.cs:16398,16928-16935,17533-17535` 由落地、tick 衰减和碰撞分别修改；`trackBoost` 在碰撞分支增减并在运动阶段 `:11542-11545` 应用后清零；`lastBoost` 作为 `ref` 传给 `Minecart.TrackCollision` (`Player.cs:17493`)，Minecart 会更新并清除它，坐骑重置路径也会清零。

`Player.mount` 是 `Mount` runtime 对象字段 (`Player.cs:1552`)，在 Player 初始化时创建 (`:26569`)，并被移动、碰撞、物品、buff、动画和表现路径读取或调用。完整参考快照也显示相同的协作形状：`Player.cs:28461-28527` 组合 Minecart 碰撞结果，`Minecart.cs:566,847,906` 通过 `ref lastBoost` 改状态，`Mount.cs:6264-6265,6315-6316` 在切换时复位车辆字段。该对照用于补足目标源码的静态关系，不替代 Version4 行为基线。

NLTX 的 `PlayerMountVehicleIntegrationComponent` 只承载六项 scalar；现有 `PlayerMountState`、`PlayerMountComponent` 和 Mount runtime 类型仍是候选状态结构，未证明它们与 P03/Movement 生产 owner 的唯一绑定。执行计划已要求 Vehicle 等 P03/Movement 决策后接入。因此本轮不添加 P05 写系统、不复制 `Mount` 对象，也不把六项字段做同一 tick reset；它们的生产输入、碰撞提交顺序、网络/存档映射及 owner 保持 `unknown` / `integration-review`。

### Minion capacity 源码对照

Version4 CPG API 对 `Terraria/Player.cs` 中三个容量字段的 symbol 查询均为 `complete`；在选定的 Player / Projectile shard 查询 member uses 得到 `maxMinions` 23 项、`numMinions` 3 项、`slotsMinions` 3 项。Projectile shard 的若干项标记为 `partial` 或 `AccessMode=Unknown`。这足以提示跨文件关系需复核，但不能从索引结果关闭 writer 集合或调度闭包。

两个源码快照都直接保留了容量关系。目标 Version4 `Terraria/Projectile.cs:14741-14767` 在 minion 更新分支读取 `numMinions`、比较 `slotsMinions + minionSlots` 与 `maxMinions`，并在分支中调整 Player 计数；目标 Version4 `Terraria/Player.cs:10024-10025`、`14947-14948`、`17659-17660` 可见多个计数清零点。完整参考项目 `D:\TRbackup\无任何删减通过编译` 的对应代码位于 `Projectile.cs:15508-15535`、`Player.cs:17301-17302`、`24937-24938`、`28647-28648`。这些位置属于两个独立源码快照，不拼接成一个基线。

完整参考的分支细节可作候选行为清单：仅在 `owner == Main.myPlayer` 时执行超容量分支；类型 `626` / `627` 有关联 Projectile 的特殊处理，常规成功分支才累加 `numMinions` 和 `slotsMinions`。这说明容量不变量涉及本地 owner 判定、特殊替换路径和计数提交的先后。目标 Version4 具备相似源码形状，但其运行时调度、跨 Projectile reserve/create/kill 原子性及所有异常/销毁路径仍为 `unknown`；完整参考的细节不能代替这些目标证据，也不能证明 NLTX 候选核心已等价。

CPG 的 `SourceSnapshotId=null` 且 `ResetProjectileCaches` 的索引 call-site 与当前源码调用数已有差异；因此设计继续以逐份源码复核关系端点，并保留查询 gaps。完整参考项目名称不作为该项目曾在本任务中构建或测试的证据。

### Damage high-water 源码复核

NLTX 的 `PlayerMinionDamageHighWaterMarkSystem` 只接受显式 `originalDamage` 输入，以各自 `Math.Max` 更新两个高水位，并单独 reset。P05 focused verifier 覆盖两个 accumulator 的单调性、Storm Tiger / Abigail 状态隔离和 reset 清零；验证仅适用于 reducer 核心，不证明 Version4 的 projectile scan、当前 tick 调用顺序、目标消费边或 owner handoff。

目标 Version4 `Terraria/Player.cs` (`E5B301E3401F61E37BF3291754670BCC123D6593EDD94597C4E9109C4030FE86`) 声明两个 high-water 字段；`UpdateProjectileCaches(int)` 只扫描 active 且 owner 等于当前 player slot 的 projectile，并对 type `831` / `970` 分别将 `originalDamage` 与当前值比较后保留最大值。`ResetProjectileCaches()` 将两个字段清零。源码文本可见两处 `ResetProjectileCaches()` 和两处 `UpdateProjectileCaches(int)` 调用；CPG 分别只返回 1 条 reset call-site、对 update 返回 `partial` / 零命中。由于 CPG `SourceSnapshotId=null`，源码与索引关系不一致，不能用索引 complete 结果关闭入站关系或 tick 调度。

完整参考项目 `D:\TRbackup\无任何删减通过编译` 的 `Player.cs` (`367E6C3249F064DD9CCF8E421EBEBF6EE3AA745B6521F07A8BEBA4A3C2FF612C`) 有对应的扫描和清零逻辑；其 `Projectile.cs` (`8D6428FA1D098B26ADFDC1D6CC028251814B40CD3ACE576F0BFF462FB19D6038`) 可见 Storm Tiger 与 Abigail projectile AI 读取这两个 Player 值。Version4 `Projectile.cs` (`97C3D68586AEF862411699E428F5C4B18CEE1F8051B684BE9F86FA7B60FAE83B`) 的文本搜索无命中，CPG 查询也为 `partial` / 零命中；完整参考的消费侧不能代替 Version4 消费侧证据，也不与目标快照拼接。

### Consumed upgrade item-use 入口复核

目标 Version4 `Terraria.ID/ItemID.cs`（`87EEA4B1C4284F3747F2850A0A563D9F29FADBD1D2215BB04159792558838BEE`）中 Aegis Crystal、Aegis Fruit、Arcane Crystal、Galaxy Pearl、Gummy Worm、Ambrosia 的常量分别为 `5337` 至 `5342`；完整参考项目 `ItemID.cs`（`281C39CA8584D53002D15CC574A65104C87A9D00FDE4B5A710F4538065688035`）中同名常量值一致。目标 Version4 `Player.cs` 声明并读取六个 `used*` 字段，但未找到 `ItemCheck_UseShimmerPermanentItems` 或这些字段设为 `true` 的源码。CPG 的子方法 symbol 查询是 `partial` / 零命中并带 gap；不能据此认定没有其他入口。

2026-10-01 通过只读 CPG Query API 复查 `usedAegisCrystal`：field symbol 查询 `complete`，在 `Terraria/Player.cs`、`Terraria/Item.cs`、`Terraria/Main.cs` 与 `Terraria.GameContent.Golf/GolfHelper.cs` 选定 scope 内的 member-use 查询 `complete` / 2 项；两项均位于 Player shard，其中一项为 `confirmed` / `Write`（只表示赋值左侧，不确认赋值值为 `true`），另一项为 `partial` / `Unknown`。该数据库 manifest 为 `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，`SourceSnapshotId=null`；索引 facts 不绑定当前源码 hash，也不证明 item type 到 flag 的调用关系或完整 writer closure。

完整参考项目 `Player.cs` (`367E6C3249F064DD9CCF8E421EBEBF6EE3AA745B6521F07A8BEBA4A3C2FF612C`) 的 `ItemCheck_UseShimmerPermanentItems(Item)` 对六种匹配物品都要求 `itemAnimation > 0`、对应状态尚未使用、`ItemTimeIsZero`；命中后先 `ApplyItemTime(sItem)`，再设相应 flag，最后 `NetMessage.SendData(4, ..., whoAmI)`。该顺序仅由完整参考快照支持；item stack 是否在其他 `ItemCheck` 路径消耗、Version4 对应方法的具体行为仍为 `unknown`。

NLTX 的 `ConsumedUpgradeEligibilityQuery.TryResolveEligibleUpgrade` 只映射已核对的 item type，并按显式传入的 animation / item-time 状态和 ledger 通过 `bool + out` 返回候选 upgrade；`out` 只写调用方自己的值，不改可观察状态。它是预判 Query，commit 仍由 `PlayerProgressionCommitSystem` 再次校验；两者间状态变化时，后者拒绝重复消费。当前未找到生产 caller 或承载 `ApplyItemTime` / net sync 的 adapter。

因此当前只实现 Player 侧的局部 reducer：分别累加两个已命名来源的 `originalDamage` 最大值，并提供局部 reset。它不扫描实体、不检查 owner、不决定伤害 provenance，也不接入 scheduler。生产调用入口、Combat/Projectile 最终 writer、调用 phase 和消费者在 Version4 的关系继续为 `unknown` / `crossSubsystemOwner: integration-review`。

### 2026-10-01 Angler / Golfer / DD2 源码补查

完整参考项目 `D:\TRbackup\无任何删减通过编译` 用作独立行为线索，不与目标 Version4 拼成同一快照。目标 `Version4/Terraria/Main.cs` (`66DBE1D1E6FB89A24A512309BB039DFEC7C06D4678D2A22D7ADECBAD3AFD6520`) 与 `Player.cs` (`E5B301E3401F61E37BF3291754670BCC123D6593EDD94597C4E9109C4030FE86`) 中保留相关字段/读取，但源码检索未找到 Angler 交付递增、`AccumulateGolfingScore` 方法或 DD2 flag 的 true 写入；不把零命中解释为这些行为在运行时不存在。

| 完整参考源码事实 | 可支持的候选行为 | 对目标 Version4 的限制 |
|---|---|---|
| `Main.cs` (`E24E61C9903BB7995F47E36C51EDBE43481B643226861B717020877E7E22B63F`) 的 Angler 交付路径先检查当日/任务状态并查找所需物品，随后扣除并清空耗尽的 stack、播放声音、递增 `anglerQuestsFinished`、调用 `GetAnglerReward`；之后置每日完成状态并在客户端发送 `NetMessage.SendData(75)`。`Player.GetAnglerReward` 读取递增后的 quest count 决定奖励。 | 这是有序、多状态的 Command + adapter 行为；单独递增 P05 counter 不能代表任务结算。 | Version4 对应 Main 交付/奖励 caller 未由源码或 CPG 闭合。NLTX 目前的 `RecordAnglerQuestCommand` 只提交计数，不扣物品、不播放声音、不发奖励或网络消息。 |
| `GolfState.cs` (`B67B71333BD280CDAACC8E9EBCEBEDD6A1156421D928E2277C6BC82219BB25D8`) 中 `GolfBallTrackRecord.GetAccumulatedScore()` 根据非负距离与命中数计算分值；`ScoreAdjustment` 的计时值从 0 增至 `golfScoreTimeMax` 后停止，或被设为最大值/清零。`GetGolfBallScore` 返回该分值乘以 adjustment 的截断整数。`GolfHelper.cs` (`28E5C2E654521B0B58D216DD7CC48E4B8DAED825380FDE0CCB65CC4A8FA0953A`) 在球入洞路径调用 `GetGolfBallScore`，并在 `proj.ai[1] > 0` 时调用 `Player.AccumulateGolfingScore`。 | 在完整参考的该调用路径中，输入分值非负；Player 累加只将总分封顶至 `1,000,000,000`。当前 NLTX reducer 对负值额外返回 `RejectedInvalidScore`，不影响该已见调用路径，但不能据此证明 Version4 command 边界语义。 | Version4 的 Player 累加方法、GolfState 得分实现与 GolfHelper 调用组合未由源码/CPG 闭合。CPG 方法 symbol 查询为 `partial` / 零命中并带 gap。 |
| 完整参考 `Player.cs` (`367E6C3249F064DD9CCF8E421EBEBF6EE3AA745B6521F07A8BEBA4A3C2FF612C`) 的玩家更新读取 `DD2Event.DownedInvasionAnyDifficulty`，为 true 时将 `downedDD2EventAnyDifficulty` 设为 true。 | DD2 来源是世界事件，P05 命令只能作为 Player 局部写入核心。 | Version4 setter/生产事件调用未知；CPG 对该字段只有一条 partial member-use，`AccessMode=Unknown` 且 `NoAssignmentEvidence`，不证明写入。 |

本轮只读 CPG 查询使用 manifest `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`。`Player` type 与 `anglerQuestsFinished`、`golferScoreAccumulated`、`downedDD2EventAnyDifficulty` 字段符号均可定位；Player surface 查询须将 `MaxItems` 提至 10000 后才完整。`Get-CpgMemberUses` 对 Angler/Golfer 字段查询的精确 scope 为 `Terraria/Player.cs`、`Terraria/Main.cs`、`Terraria.GameContent.Golf/GolfHelper.cs`，状态为 `partial` / 零项并带 `NoMatchingFactInScannedScope`；DD2 字段返回 1 项，但该事实本身是 `partial`、`AccessMode=Unknown`。`Find-CpgSymbols` 对 `AccumulateGolfingScore`（`Terraria/Player.cs`）、`GetGolfBallScore`（`Terraria.GameContent.Golf/GolfState.cs`）、`GetAnglerReward`（`Terraria/Player.cs`）的查询均为 `partial` / 零项并带 gap；`GolfState` type surface 查询 complete 但未返回 `GetGolfBallScore`。数据库 `SourceSnapshotId=null`，这些结果只能记录为索引范围事实和缺口。

## 行为边界

以下 17 组均保留原报告的完整字段清单；本表只规定候选行为切片。

| P05 字段组 | 字段数 | 候选行为边界 | 当前状态 |
|---|---:|---|---|
| `PlayerConsumedProgressionFlags` | 6 | 一次性消耗进度提交 | commit core 与 item-use 预判 query 已有；target Version4 item-use writer 缺失，授权来源、item-time/network effects 和持久化 unknown |
| `PlayerUnlockProgressionState` | 4 | 解锁及启用偏好提交 | typed local commit/query core and direct focused verifier are present; external command authorization, persistence and network projection remain unknown |
| `PlayerQuestAndEventCounters` | 3 | Quest / event progress commit | local Angler/Golfer/DD2 commit core exists; source event, reward order, Version4 writer and persistence/network remain unknown |
| `PlayerFishingCapabilityState` | 7 | skill 累加与 fishing capability flags 的每 tick rebuild/query | partial；skill 来源虽能在 Player 源码中定位，完整生产输入、调用次数、consumer 和刷新点仍 unknown |
| `PlayerMinionCapacityState` | 3 | minion admission 与计数不变量 | `crossSubsystemOwner: integration-review` |
| `PlayerCoreMinionSummonFlags` | 22 | minion capability rebuild | 局部显式输入 rebuild/reset core 已实现并经 verifier；buff/content 输入映射、caller 与 tick phase unknown |
| `PlayerCrossoverMinionSummonFlags` | 3 | crossover minion capability adapter | 局部显式输入 rebuild/reset core 已实现并经 verifier；跨内容 owner、输入映射与 caller 仍为 `integration-review` / unknown |
| `PlayerMinionDamageTrackingState` | 2 | minion damage high-water tracking | Player 局部 reset / max reducer 已实现并由 focused verifier 验证核心语义；来源授权、Projectile consumer、phase 与最终 owner 仍为 `unknown`，`crossSubsystemOwner: integration-review` |
| `PlayerLegacyPetState` | 21 | pet eligibility / presentation capability | 20 resettable tick flags have a local rebuild/reset core; garden-gnome network state has a separate lifecycle; persistent eligibility meaning and production owner remain unknown |
| `PlayerBossPetFlags` | 16 | boss pet capability | 显式输入 rebuild/reset core 与 focused verifier 已有；capability 到 projectile 的映射未闭合 |
| `PlayerSeasonalAndEventPetFlags` | 9 | seasonal / event pet capability | 显式输入 rebuild/reset core 已实现并经 focused verifier；event/buff 输入来源、Projectile death cleanup 与 entity lifecycle owner 未闭合 |
| `PlayerStandardNamedPetFlags` | 13 | named pet capability | 显式输入 rebuild/reset core 与 focused verifier 已有；catalog 映射和缺失内容策略 partial |
| `PlayerCrossoverPetFlags` | 13 | crossover pet capability adapter | 显式输入 rebuild/reset core 与 focused verifier 已有；跨内容 owner unknown |
| `PlayerWorldObjectPetFlags` | 4 | world object pet capability | `PlayerWorldObjectPetCapabilityRebuildSystem` 只重建显式输入的四个当前 tick flag；tile/world-object 与 pet entity owner unknown，production caller/scheduler 未确认 |
| `PlayerCompanionState` | 14 | companion capability / entity lifecycle | 14 resettable flags have a local rebuild/reset core; `crossSubsystemOwner: integration-review` still owns live entity, death cleanup and handoff |
| `PlayerMountAndMinecartEffects` | 7 | Mount / Minecart runtime integration | 六项 scalar integration component 已存在；`Player.mount` 的 runtime owner、更新顺序与生产接入仍为 `crossSubsystemOwner: integration-review`，不设 P05 final owner |
| `PlayerAccessoryProgressionEffects` | 17 | accessory effect snapshot 与下游 handoff | 15 `ResetEffects` tick flags have a local rebuild/reset core; `brokenMirrorBadLuck` and `wearsRobe` have distinct source lifecycles; multiple effect owners keep production boundary partial |

字段应再按状态语义分开：持久解锁、当前 tick 能力、实体是否存活、计数/容量、显示/派生效果不可互换。宠物旗标名称不能证明它是永久解锁；共享 Component 也不能证明谁是唯一写者。

## System 职责与边界

| 能力 | P05 候选职责 | 明确不拥有 / 必须协作 |
|---|---|---|
| Player progression commit | 验证并提交 consumed upgrade、unlock/preference、quest/event command；返回明确的 committed / duplicate / rejected 结果。 | Item 消耗、奖励回调、事件授权、存档写入和网络发布分别由来源与边界 owner 管理；提交顺序尚待确认。 |
| Fishing capability rebuild | 在 durable progression 之外维护一个 tick snapshot；按源贡献累计 fishing skill、合并 flags，查询只读已提交 revision。 | 输入适配、贡献次序和 revision 发布点仍 unknown；不拥有鱼竿、bait、liquid、bobber 或外部钓鱼资格写入。 |
| Minion capability | `PlayerMinionCapabilityRebuildSystem` 将上游解析的 22 个 core 与 3 个 crossover 当前 tick flags 写入对应 snapshot Component，并能 reset 到默认值。 | 不解析 buff/content eligibility，不拥有生产 caller/tick phase，不等同于现存 Projectile，也不最终批准超额 admission。 |
| Minion capacity admission | 当前候选 `PlayerMinionCapacityComponent` 保存不可变目标 Player `EntityReference`；commit System 在组件锁内提交已授权的局部容量 delta，capacity Query 读取同一受保护快照。 | 命令的 `Owner` 和 `ProjectileOwner` 都必须匹配组件 owner。最终 admission 与 Projectile 创建/失败/销毁的提交协议仍待 integration review；本地锁不覆盖跨 Projectile 的 reserve/create/release 原子性，本地 owner 校验也不决定跨域最终 owner。 |
| Pet / companion | `PlayerBossPetCapabilityRebuildSystem`、`PlayerCrossoverPetCapabilityRebuildSystem`、`PlayerSeasonalEventPetCapabilityRebuildSystem`、`PlayerStandardNamedPetCapabilityRebuildSystem`、`PlayerWorldObjectPetCapabilityRebuildSystem` 与 `PlayerCompanionCapabilityRebuildSystem` 分别将上游解析的 16、13、9、13、4、14 个当前 tick flags 写入对应 capability Component，并可 reset 到目标已确认的默认值。 | 这些核心不解析 buff/event、不生成 Projectile。Projectile heartbeat、死亡清理、创建/销毁、重连和世界迁移的最终责任未知。 |
| Minion damage tracking | 局部 System 将已由上游验证的 Storm Tiger / Abigail original-damage observation 累加为最大值，并能清零自己的两个字段。 | 不扫描或归属 Projectile；Combat / Projectile 的 observation 授权、死亡/重置 phase、消费者和唯一最终 writer 仍待 integration review。 |
| Accessory effect rebuild | 产生不可变或受控的 per-tick 输出快照。 | Combat、Movement、Luck / Environment、Presentation 各自提交自己的权威状态；snapshot 不是第二个写者。 |
| Mount / Minecart | 提供所需 Player effect 的 typed integration input。 | Mount / Minecart、Movement 与碰撞的状态 owner、更新顺序归跨分区评审。 |

### API 组合

这些名称表达行为，不规定最终 C# 签名或必须新建独立类型。

| 行为入口 | 建议组合 | 不变量 |
|---|---|---|
| consumed / unlock / quest progress | 授权来源 -> 有序应用边界 -> typed command -> progression state commit -> 后续投影/网络效果 | Reducer 只提交 Player 状态，不代表相关物品、奖励或事件效果已完成；重复和部分成功语义必须由应用边界定义。 |
| consumed upgrade item use | item-use caller -> `ConsumedUpgradeEligibilityQuery.TryResolveEligibleUpgrade` 预判 -> `ApplyItemTime` -> `ConsumePlayerUpgradeCommand` / `PlayerProgressionCommitSystem.Commit` 写 flag -> Player net update | 完整参考快照顺序为 item-time、flag、网络更新；Query 不是授权或提交。若提交可能并发拒绝，必须先确定串行化、reservation 或补偿策略。Version4 caller、phase 和该失败策略仍 `unknown`，暂不接生产 adapter。 |
| Angler / Golfer / DD2 progress | Angler：验证日状态/交付物 -> 扣除物品 -> 计数 commit -> 按新计数发奖励 -> 标记当日完成并投影；Golfer：落洞结算 -> 分数 commit -> 后续效果；DD2：世界事件 -> 单调进度 commit | 完整参考只支持这些候选顺序。NLTX 的 `PlayerProgressionCommitSystem` 只提交局部状态；Angler 消耗与奖励的原子边界、Golf caller identity、DD2 事件生产者在 Version4 均未闭合。 |
| tick effects | tick begin/reset -> buff/equipment/content contributions -> rebuild -> publish snapshot -> read-only consumers | fishing contribution 的 skill 使用加法、flags 使用 OR；每 tick 必须从 reset 开始且各来源调用次数/先后须由目标调度证据确定。同一 revision 才能供消费者读取。 |
| minion admission | 预判 Query -> 最终 admission command 与 Projectile create/kill 协议 -> owner-bound capacity commit | `PlayerMinionCapacityComponent` 固定保存目标 Player `EntityReference`；command 的 `Owner` 与 `ProjectileOwner` 均须匹配该身份。同组件锁覆盖 delta、reset、maximum rebuild、token 去重与 capacity Query 快照；去重 token 随容量 Component 生命周期保存并在 reset 时清除。预判不能授权；创建失败和销毁需能恢复计数；本地锁和身份校验均不闭合最终 admission owner 或跨 Projectile 原子性。 |
| pet / companion sync | 已解析的 Player tick flags -> 对应 pet capability rebuild System -> owner-controlled synchronize / despawn request -> entity owner result | 局部 rebuild 不代表 entity handoff 已存在；永久解锁、当前资格和 live entity 分开，死亡清理不能误清持久解锁。 |
| minion damage high-water | 已验证来源的 projectile observation -> `PlayerMinionDamageHighWaterMarkSystem.AccumulateStormTigerGemOriginalDamage` / `AccumulateAbigailCounterOriginalDamage` -> Player 局部高水位 Component；source reset -> `Reset` | observation 必须先由 integration owner 验证归属；实际 source phase、最终 writer、Projectile 消费者与 reset 先后仍 `unknown`。 |
| persistence / network | adapter 读写带版本的 P05 snapshot -> 校验 -> 调各权威 owner 提交 -> 只读投影 | 版本、字段编码、错误恢复及发送可见时点须先由匹配的 legacy 证据确认。 |

`GetFishingCapability`、`GetRemainingMinionCapacity` 一类读操作可以留在 owner 的 System API 或共享规则 Query 中，不为每个字段创建 Query 类型。Query 不写入状态、不发布事件，也不替代 admission/commit。

## 依赖与时序

### 目标源码中可见的关系

```text
Main.DoUpdateInWorld -> active Player.Update(i)
Player.Update -> ResetEffects -> buff/equipment/update work
Projectile minion admission -> reads Player capacity -> success path updates Player counters
selected pet Projectile AI -> reads Player death/flag -> extends projectile lifetime;
                                  some death paths write Player pet flags
```

Version4 源码和 CPG API 的交叉证据：选定范围的 `Player.Update(int)` call-site 查询找到 `Main.cs` 中一个精确 call target；`ResetEffects` 在 `Player.cs` 的选定 scope 找到一个 call-site。对 `UpdateBuffs`、`UpdateEquips`、`UpdateProjectileCaches` 的 call-site 查询为 `partial`/零命中且带 gap，源码仍可直接看到这些调用，因此零命中不代表无调用。`Get-CpgMemberUses` 在 `Player.cs` / `Projectile.cs` 范围发现容量和宠物旗标的 `Unknown`、`ReadWrite`、`Write` 事实；不能从这些结果推断完整 writer 集合。

### 提议的协作方向

```text
authorized intent / event
  -> progression validation and commit owner
  -> authoritative progress state
  -> persistence and network adapters

tick begin/reset
  -> confirmed buff/equipment/content inputs
  -> capability rebuild owners
  -> versioned read-only snapshots
  -> consumer Systems

Projectile spawn/kill <-> one integration-reviewed minion admission commit
pet/companion capability -> integration-reviewed entity lifecycle owner
Mount/Minecart state <-> P03/Movement owner
Combat/Projectile observation -> integration-reviewed damage high-water writer
```

第二张关系只是 proposed contract，不是已验证调度 DAG。执行器注册、并行策略、barrier、队列 flush、失败恢复、网络可见时点和异常路径均为 `unknown`。在证据补齐前不依赖文件顺序或类型名推断 tick 顺序。

## 生命周期与副作用约束

- 持久进度：其 player/session/world scope、断线重连恢复和 save release gate 尚未确认；不要因字段叫“progression”就当作已持久化。
- Fishing / minion / pet / accessory capability：候选为每 tick 或事件驱动的派生状态，须记录输入 revision、重建点、有效期与失效方式；不假定可跨 tick 缓存。
- Minion counts：与 Projectile create/kill 共同形成不变量；创建、销毁、重复事件、异常和 owner disconnect 路径都需要显式提交或重建规则。
- Pet / companion：需要区分解锁、启用、资格和存活实体；clear flag、respawn、reconnect、换世界和实体销毁语义未闭合。
- Save / network：只由边界 adapter 负责编码和 I/O；恢复先校验，再请求各权威 owner 提交。提交与发布顺序、重复/丢失处理均未确定。
- 外部效果、事件、随机、时间和音视频应留在显式执行边界；纯计算只依赖显式输入。记录读取/写入、责任边界、顺序、失败和重复语义。

## 未决集成决定

下列事项必须保留为 `crossSubsystemOwner: integration-review`，不能由 P05 单独裁定：

| 集成面 | 必须确认 |
|---|---|
| P05 与 ProjectileSimulation | admission、slot reserve/release、创建失败、销毁和 owner identity 的一个提交协议。 |
| P05 与 Pet / Companion entity owner | flag 的持久/临时语义、内容映射、实体同步、死亡/重连/换世界恢复。 |
| P05 与 Combat | damage 来源、目标 Player provenance、high-water 更新和 reset 时点。 |
| P05 与 P03 / Movement | `Mount mount`、cart effects、轨道/碰撞写入、身体状态与 phase 顺序。 |
| P05 与 Equipment / Fishing / Buff / Content | 七项 fishing 与 17 项 accessory 字段的完整输入、写者、刷新点和消费者。 |
| P05 与 Player lifecycle / persistence / network | save schema/release gate、旧字段兼容、加载校验、网络字段与提交可见顺序。 |

## 反向方案

- 不为 17 个成员组各设 System：这会按库存结构切分，无法表达跨组不变量与共享生命周期。
- 不将所有状态交给一个 `PlayerProgressionSystem`：每 tick snapshot、Projectile admission、durable commit 与 Vehicle 更新的时机和效果不同。
- 不以只读 capacity Query 的结果直接生成 minion：检查与创建间可能状态已变化。
- 不由 Query / Projection 回写权威状态，也不让 Player 与 Projectile 双写同一容量不变量。
- 不直接把 SS14 的注册、事件或组件模式作为 Terraria 规则；它只作 System 组织参考。

### 2026-10-01 seasonal/event pet capability 局部核心

目标 Version4 `Player.UpdateBuffs` 中，9 个 seasonal/event pet flags 通过 `BuffHandle_SpawnPetIfNeededAndSetTime` 更新；外层 helper 会写 `buffTime[buffIndex]`，内层 `BuffHandle_SpawnPetIfNeeded` 会置位 flag、查询 `ownedProjectileCounts` 并计算 `center`，但目标 Version4 方法体到此结束，没有显示 `Projectile.NewProjectile` 分支。`Player.ResetEffects` 会清零这些当前 tick flags。实体创建是否在目标快照的其他入口发生仍为 `unknown`；完整参考项目的创建分支不能补成 Version4 事实。

Version4 `Projectile.cs` 还会在玩家死亡时直接清除 `petFlagEverscreamPet`、`petFlagMartianPet`、`petFlagDD2OgrePet` 与 `petFlagDD2BetsyPet`，并按 owner flag 决定是否延长 Projectile lifetime。完整参考快照有相应的 Player / Projectile 形状，但它的两个文件 hash 与 Version4 不同：Version4 `Player.cs`=`E5B301E3401F61E37BF3291754670BCC123D6593EDD94597C4E9109C4030FE86`、`Projectile.cs`=`97C3D68586AEF862411699E428F5C4B18CEE1F8051B684BE9F86FA7B60FAE83B`；完整参考 `Player.cs`=`367E6C3249F064DD9CCF8E421EBEBF6EE3AA745B6521F07A8BEBA4A3C2FF612C`、`Projectile.cs`=`8D6428FA1D098B26ADFDC1D6CC028251814B40CD3ACE576F0BFF462FB19D6038`。两套快照保持独立，完整参考项目未在本轮构建或测试。

只读 CPG Query API 使用 manifest `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，对 9 个字段分别执行 `Find-CpgSymbols -Kind SymbolField` 与 `Get-CpgMemberUses`（限定 `Terraria/Player.cs`、`Terraria/Projectile.cs`）。symbol 与选定 scope 的查询均返回 `complete`，但事实中包含 `EvidenceStatus=partial` / `AccessMode=Unknown`；Everscream、Martian、DD2 Ogre 和 DD2 Betsy 有 Projectile confirmed-write candidates。manifest 的 `SourceSnapshotId=null`，CPG 查询不能闭合动态引用或证明与本次源码 hash 同快照。完整参考 `Projectile.cs` 中额外可见 DD2 Gato / Ghost / Dragon flag 读取；因快照不同，不将这些读取回填为 Version4 关系。

NLTX 当前 `EntityReference` 是身份值类型，`EntityRelationState` 与 `SpawnAdmissionState` 保存被动数据，没有跨 Player / Projectile pet lifecycle commit API。SS14 `FollowerSystem` 可作为系统组织参照：它维护 follower/followed 双向组件并响应 terminating、polymorph 和 remote-entity replacement；该 ghost-follower 语义不套用到 Terraria pet。新增 `PlayerSeasonalEventPetCapabilityRebuildInput` / `PlayerSeasonalEventPetCapabilityRebuildSystem` 只把调用方已解析的 9 个当前 tick flags 重建到 `PlayerSeasonalEventPetCapabilityComponent`，`Reset` 等于应用默认输入；不解析 buff/event、不改 buff time、不创建或销毁 Projectile，也未接入 scheduler。此核心经单个 P05 focused verifier 覆盖 true rebuild 与 reset；其输入适配、Projectile 协调、实体生命周期与行为等价仍为 `unknown` / `not-run`。

### 2026-10-01 standard named pet capability 局部核心

目标 Version4 `Player.ResetEffects` 清零 13 个 standard named pet flags（`Player.cs:10536-10547,10569`）；`Player.UpdateBuffs` 根据多个 buff 分支调用 `BuffHandle_SpawnPetIfNeededAndSetTime`（`Player.cs:5329-5374,5538-5539`）。完整参考快照对应重置位于 `Player.cs:18924-18935,18957`，更新调用位于 `:10963-11008,11172-11173`。Version4 `Player.cs` SHA-256=`E5B301E3401F61E37BF3291754670BCC123D6593EDD94597C4E9109C4030FE86`，完整参考 `Player.cs` SHA-256=`367E6C3249F064DD9CCF8E421EBEBF6EE3AA745B6521F07A8BEBA4A3C2FF612C`；它们是独立源码快照，不将完整参考中的行为闭包回填为 Version4 事实。

只读 CPG 查询使用 manifest `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，对 13 个字段分别查询 Player symbol 与 Player / Projectile member uses；symbol 和所选 scope 查询均为 `complete`。`petFlagUpbeatStar` 返回 2 个 Player facts；其余 12 个字段各返回 4 个 Player / Projectile facts。每个字段结果都含 `EvidenceStatus=partial` / `AccessMode=Unknown` facts；`SourceSnapshotId=null`，不关闭动态引用、唯一 writer 或 consumer 闭包。

NLTX 新增 `PlayerStandardNamedPetCapabilityRebuildInput` 和 `PlayerStandardNamedPetCapabilityRebuildSystem`，只复制调用方已解析的 13 个当前 tick flags，并以 default input 清零；focused verifier 已验证 13 项全部置真及 reset 后全部置假。它不解析 buff/content、不延长 buff time、不创建 Projectile，也没有生产 caller 或 scheduler。buff-to-flag 映射适配、pet entity lifecycle owner、Projectile 创建/销毁、death/reconnect/world-transfer 以及行为等价仍为 `unknown` / `crossSubsystemOwner: integration-review`。

### 2026-10-01 boss pet capability 局部核心

目标 Version4 `Terraria/Player.cs`（SHA-256=`E5B301E3401F61E37BF3291754670BCC123D6593EDD94597C4E9109C4030FE86`）在 `UpdateBuffs` 的 `:5378-5462` 将 16 个 boss-pet flags 传给 `BuffHandle_SpawnPetIfNeededAndSetTime`，并在 `ResetEffects` 的 `:10514-10535` 清零这些字段。完整参考项目 `D:\TRbackup\无任何删减通过编译` 的独立 `Player.cs`（SHA-256=`367E6C3249F064DD9CCF8E421EBEBF6EE3AA745B6521F07A8BEBA4A3C2FF612C`）在 `:11012-11096` 有对应 helper 调用、在 `:18902-18923` 清零。两份源文件 hash 不同，参考快照不补齐 Version4 的缺失行为。

Version4 CPG Query API 使用 manifest `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，对 16 个 field symbols 逐项执行 `Find-CpgSymbols -Kind SymbolField`，并以 `Get-CpgMemberUses` 查询 `Terraria/Player.cs` 和 `Terraria/Projectile.cs`。全部 symbol/use 请求为 `complete`，各字段返回 2 至 5 项；13 个字段结果包含 Player 与 Projectile shard facts，3 个字段只在 Player shard 返回 facts。每个字段的 facts 同时包含 `EvidenceStatus=confirmed` / `partial` 与 `AccessMode=Write` / `Unknown`，因此不推出完整 writer、consumer 或唯一 owner。数据库 `SourceSnapshotId=null`，CPG facts 未绑定当前源码 hash。

NLTX 新增 `PlayerBossPetCapabilityRebuildInput` 与 `PlayerBossPetCapabilityRebuildSystem`，只把调用方已解析的 16 个当前 tick flags 复制到既有 `PlayerBossPetCapabilityComponent`，`Reset` 通过 default input 清零。focused verifier 覆盖 16 项全量置真和 reset 后全量清零。该实现不映射 buff id、不延长 buff、不检查 owned Projectile、不创建或销毁实体，也未接入生产 caller/scheduler。上述旧 helper 的实体生命周期效果、调用顺序、生产输入适配、唯一 writer 和行为等价仍为 `unknown` / `crossSubsystemOwner: integration-review`。

### 2026-10-01 crossover pet capability 局部核心

目标 Version4 `Player.cs` 在 `UpdateBuffs` 的 `:5466-5523` 通过共享 pet helper 更新 11 个 crossover flags，在 `ResetEffects` 的 `:10551-10567` 清零组件中的全部 13 个字段。`Projectile.cs` 在 `:38807-38908` 对多种宠物在死亡时清除 Player flag 并读取 flag 延长存活时间；Chillet / Chillet Ignis 对应 `:38510-38523`。因此这些值具有 Player tick 重建与 Projectile 生命周期写入交错。目标 `Projectile.cs` SHA-256=`97C3D68586AEF862411699E428F5C4B18CEE1F8051B684BE9F86FA7B60FAE83B`。

完整参考项目是独立快照：其 `Player.cs` SHA-256=`367E6C3249F064DD9CCF8E421EBEBF6EE3AA745B6521F07A8BEBA4A3C2FF612C`、`Projectile.cs` SHA-256=`8D6428FA1D098B26ADFDC1D6CC028251814B40CD3ACE576F0BFF462FB19D6038`。对应 Player helper / reset 位于 `:11100-11157` 与 `:18939-18955`，Projectile death/lifetime 路径位于 `:56337-56438` 与 `:56040-56053`。该快照同样显示 Chillet 两字段被 Projectile 清理/读取，却不在 Player 的这些 helper 调用中；它不能补足 Version4 的同 tick 写入顺序或生产输入映射。

只读 CPG Query API 使用 manifest `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，对 13 个字段逐项查询 Player symbols 和 Player / Projectile member uses。所有 symbol/use 查询状态为 `complete`，各字段有 3 至 4 项；facts 同时包含 `EvidenceStatus=confirmed` / `partial`、`AccessMode=Write` / `Unknown`。CPG 查询范围显示这组字段分布在 Player 与 Projectile shards，但不证明所有动态写入、唯一 owner 或同 tick 顺序；数据库 `SourceSnapshotId=null`，facts 未绑定当前源码 hash。

NLTX 新增 `PlayerCrossoverPetCapabilityRebuildInput` 与 `PlayerCrossoverPetCapabilityRebuildSystem`，将上游已解析的 13 个 bool 显式复制到 `PlayerCrossoverPetCapabilityComponent`，reset 应用 default input。局部 verifier 验证 13 项全量置真和 reset 清零。该核心不映射 buff id、不处理 death clear、Projectile lifetime、生成/销毁或调度。Chillet 两字段的 Version4 true 写入者、Player/Projectile 交错顺序、生产输入适配、生命周期 owner 与行为等价仍为 `unknown` / `crossSubsystemOwner: integration-review`。

### 2026-10-01 world-object pet capability

P05 claim 中的四个字段为 `petFlagDirtiestBlock`、`petFlagBoulderPet`、`petFlagRainbowBoulderPet`、`petFlagAxeFairyPet`。目标 Version4 `Terraria/Player.cs`（SHA-256=`E5B301E3401F61E37BF3291754670BCC123D6593EDD94597C4E9109C4030FE86`）在 `UpdateBuffs` 分别从 buff `354/373/382/372` 调用共享 helper，映射 projectile type `1018/1056/1090/1050`（`:5507-5527`）；`ResetEffects` 将四项清零（`:10560-10565`）。目标 helper（`:6324` 起）将传入 flag 置真、检查 `ownedProjectileCounts` 并计算 `center`，随后结束；该方法体没有创建调用。目标 `Projectile.cs` 文本检索未命中这四个字段。

完整参考项目 `D:\TRbackup\无任何删减通过编译` 是独立快照：其 `Player.cs` SHA-256=`367E6C3249F064DD9CCF8E421EBEBF6EE3AA745B6521F07A8BEBA4A3C2FF612C`、`Projectile.cs` SHA-256=`8D6428FA1D098B26ADFDC1D6CC028251814B40CD3ACE576F0BFF462FB19D6038`。该 Player helper 在 `:12160-12188` 包含 `flag && whoAmI == Main.myPlayer` 后调用 `Projectile.NewProjectile`；其 Projectile 对 Dirtiest Block、Boulder、Rainbow Boulder 在 `:47082-47108` 有 death clear/lifetime 分支，并在 Axe Fairy 分支 `:67361` 读取 flag 延长 lifetime。这些只支持完整参考快照的行为线索，不能回填为 Version4 事实。

只读 Version4 CPG Query API 使用 manifest `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，对四个字段逐项执行 `Find-CpgSymbols -Kind SymbolField`，再以 `Get-CpgMemberUses` 限定 `Terraria/Player.cs` 与 `Terraria/Projectile.cs`。四个 symbol/use 查询均为 `complete`，每字段各 1 个 symbol、2 个 use facts；每组 facts 都只有 Player shard 的一个 `confirmed/Write` 与一个 `partial/Unknown`，Projectile shard 未返回 facts。`SourceSnapshotId=null`；查询完成仅表示所选索引 scope 内完成，不能由 Projectile 零命中推出没有读取/写入或关闭动态访问与唯一 owner。

NLTX 当前候选 `PlayerWorldObjectPetCapabilityRebuildInput` / `PlayerWorldObjectPetCapabilityRebuildSystem` 只将调用方解析的四个 bool 写入 `PlayerWorldObjectPetCapabilityComponent`，`Reset` 应用 default input。P05 aggregate verifier 中有 `--world-object-pet-only` 分支，本次未执行该 aggregate 项目；独立验证项目链接这三个候选源码文件，已验证四项 rebuild 与 reset。该核心不解析 tile/world-object、buff、Projectile eligibility，不创建或清理实体，也未接入生产 caller/scheduler。目标版本的 Projectile consumer/death cleanup、pet 实体所有权、buff 输入适配与完整行为仍为 `unknown` / `crossSubsystemOwner: integration-review`。

### 2026-10-01 fishing capability contribution 与消费关系补查

目标 Version4 `Terraria/Player.cs` 的 SHA-256 为 `E5B301E3401F61E37BF3291754670BCC123D6593EDD94597C4E9109C4030FE86`。该文件声明 7 个字段（`:818-830`），`ResetEffects` 在 `:10493-10499` 清零。可见 skill 输入包括 buff `+15`（`:4573`）、永久 boost 条件 `+3`（`:6830`）、bobber 条件 `+10`（`:6933-6935`）、装备 `+5`（`:7377`）及装备分支中的 `+10` 增量和多个 capability flag 写入（`:8301-8330`）。这些是目标 Player 文件中的直接写入路径；各输入是否在同一 tick 恰好应用一次、如何映射为新 System contribution、实际 refresh phase 和完整消费者仍为 `unknown`。

用户指定的完整参考项目 `D:\TRbackup\无任何删减通过编译` 是另一个源码快照：`Player.cs` SHA-256=`367E6C3249F064DD9CCF8E421EBEBF6EE3AA745B6521F07A8BEBA4A3C2FF612C`，`Projectile.cs` SHA-256=`8D6428FA1D098B26ADFDC1D6CC028251814B40CD3ACE576F0BFF462FB19D6038`。其 Player fishing-level 组合将 `fishingSkill` 与 bait/pole power 相加（`Player.cs:42706`）；line protection 与 tackle-box effects 分别可见于 `:53016`、`:53063`。Projectile 直接读取 lava capability、sonar flag 与 crate flag（`Projectile.cs:19420,19532,19548,19571,19632,20318`）。完整参考说明这些 Player flags 会影响钓鱼资格、反馈和随机结果路径，但不能据此断言 Version4 有相同调用闭包或顺序。完整参考目录名中的“通过编译”不是本轮构建/测试证据。

只读 Version4 CPG Query API 使用 manifest `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，`SourceSnapshotId=null`。对 7 个字段分别执行 `Find-CpgSymbols -Kind SymbolField` 与 `Get-CpgMemberUses`，member-use scope 为 `Terraria/Player.cs` 和 `Terraria/Projectile.cs`；各 symbol/use 查询状态均为 `complete`，查询到的 use facts 全在 Player shard（`fishingSkill` 8、`cratePotion` 2、`sonarPotion` 2、`accFishingLine` 4、`accFishingBobber` 3、`accTackleBox` 4、`accLavaFishing` 3）。`accFishingBobber` 的一个 fact 是 `EvidenceStatus=partial` / `AccessMode=Unknown`。当前 Version4 全树文本搜索未找到这些字段在 Player 以外的直接引用；CPG 与源码索引未绑定快照，零外部 fact 不能证明无消费者，target 消费关系保持 `unknown`。

NLTX 已有 `PlayerFishingCapabilityContributionInput` 与 `PlayerFishingCapabilityRebuildSystem`：`Contribute` 对 skill 做 unchecked 加法、对 6 个 flags 做 OR，`Reset` 清零；重复调用会重复累加 skill，Component 没有来源 token 去重。局部 verifier 使用 buff `+15`、永久 upgrade `+3`、equipment `+10`，验证 skill `28`、7 项 snapshot 合并和 reset 清零。该输入集合只是代表性样例，不覆盖目标源码中的所有增量分支或重复调用语义；代码搜索只找到 verifier 使用，没有生产 caller / scheduler 接入证据。因此方案仍为 `proposed`，输入生产者、调用次数、revision、Projectile 消费者交接和完整行为等价均为 `unknown` / `not-run`。

### 2026-10-01 legacy pet/effect flags snapshot

目标 Version4 `Terraria/Player.cs`（SHA-256=`E5B301E3401F61E37BF3291754670BCC123D6593EDD94597C4E9109C4030FE86`）声明这 21 个相邻领域字段：20 个每 tick flag（`suspiciouslookingTentacle`、`crimsonHeart`、`lightOrb`、`blueFairy`、`redFairy`、`greenFairy`、`bunny`、`turtle`、`eater`、`penguin`、`magicLantern`、`rabid`、`sunflower`、`wellFed`、`puppy`、`grinch`、`miniMinotaur`、`blackCat`、`spider`、`squashling`）和独立生命周期的 `HasGardenGnomeNearby`。`UpdateBuffs` 写入 pet/buff flags（`:5298-6127`），`ResetEffects` 清除上述 20 项（`:10409-10576`），但不清除 `HasGardenGnomeNearby`。该字段在 `NetMessage` packet 134 中写出（`:1552`），由 `MessageBuffer` packet 134 读入并写回 Player 后调用 `RecalculateLuck`（`:3142`）；`Player.RecalculateLuck` 读取该值（`:17914`）。因此 tick reset 必须保留 garden-gnome 状态。

这些 flag 并非单一宠物生命周期：Version4 `Projectile.cs` 在宠物死亡分支清除多个 Player flags 并据 flag 处理 Projectile lifetime（例如 `:38235-38490,38939-38941`）；`NPC.cs:618` 读取 `sunflower`，而 `wellFed` / `rabid` 还参与 Player 的食物与伤害行为。当前实现只把调用方解析后的值复制到现有 Component，不据名称合并这些下游行为、不接管死亡清理或效果计算；Player / Projectile / NPC 之间的最终 owner、顺序和同步协议仍为 `crossSubsystemOwner: integration-review`。

用户指定的完整参考项目 `D:\TRbackup\无任何删减通过编译` 是独立快照：其 `Player.cs` SHA-256=`367E6C3249F064DD9CCF8E421EBEBF6EE3AA745B6521F07A8BEBA4A3C2FF612C`，`Projectile.cs` SHA-256=`8D6428FA1D098B26ADFDC1D6CC028251814B40CD3ACE576F0BFF462FB19D6038`。参考 `Player.cs:17019-17022` 还可见通过 `SceneMetrics.HasGardenGnome` 写入 `HasGardenGnomeNearby`；目标 Version4 的可见直接写入则来自 `MessageBuffer`。快照差异不用于回填 Version4 事实；完整参考项目本轮仅用于源码对照，未构建或测试。

只读 CPG Query API 使用 manifest `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`（967 shards，`SourceSnapshotId=null`）。21 个 `SymbolField` 查找和限定 `Terraria/Player.cs` / `Terraria/Projectile.cs` 的 member-use 请求均为 `complete`，每个字段有 1 个 symbol、1 至 6 个 use facts；facts 含 `EvidenceStatus=confirmed` / `partial` 与 `AccessMode=Write` / `Unknown`。另对 `HasGardenGnomeNearby` 扩展到 `MessageBuffer.cs` / `NetMessage.cs`、对 `sunflower` 扩展到 `NPC.cs` 的查询也为 `complete`。`complete` 仅表示所选 shard 内索引请求完成；CPG 与源码快照未绑定，不能据此确定完整动态闭包、唯一 writer 或运行时顺序。省略 `SourcePath` 的全数据集查询在该 reader 实例返回 `SourcePathMustContainStrings`，故扩展查询显式列出相关 source paths。

新增 `PlayerLegacyPetCapabilityRebuildInput` / `PlayerLegacyPetCapabilityRebuildSystem` 作为局部 snapshot 核心：`Rebuild` 复制 21 项显式输入；`ResetTickFlags` 清除源码在 `ResetEffects` 清除的 20 项，同时保留 `HasGardenGnomeNearby`。这不是 buff 输入适配器，也不处理 `ownedProjectileCounts`、buff time、`Projectile.NewProjectile`、death clear、NPC effects、网络序列化或生产调度。专用 verifier 只覆盖所有输入置真、重建和 20 项 reset/gnome preservation；生产输入来源、调度和行为等价仍为 `unknown` / `not-run`。

### 2026-10-01 accessory tick-effect snapshot

目标 Version4 `Player.cs` SHA-256=`E5B301E3401F61E37BF3291754670BCC123D6593EDD94597C4E9109C4030FE86`。17 项候选字段中，有 15 项在 `ResetEffects` 归零：`flowerBoots`、`fairyBoots`、`hellfireTreads`、`moonLordLegs`、`deadMansSweater`、`arcticDivingGear`（`:10314-10319`）、`coolWhipBuff` / `cobWhipBuff`（`:10398-10399`）、`magicCuffs`、`coldDash`、`desertDash`、`desertBoots`、`sailDash`、`eyeSpring`、`scope`（`:10577-10591`）。`Projectile.cs` 还会写入 / 清除 `coolWhipBuff`、`cobWhipBuff` 和 `eyeSpring`，`NPC.cs` 读取 `scope`；因此输入解析、Player/Projectile 顺序和最终 owner 未闭合。

`wearsRobe` 在 Player 的装备匹配路径先清零再由 `SetMatch` 写入（`:20084-20100`），不在上述 `ResetEffects` 清单中。Version4 的 `brokenMirrorBadLuck` 只见声明和 `RecalculateLuck` 读取（`:1505,17924`），目标源码未见其 writer；完整参考项目包含独立 `brokenMirrorBadLuckTime` 与计时更新（参考 `Player.cs:29432-29446`），不能回填为 Version4 行为。完整参考 `Player.cs` SHA-256=`367E6C3249F064DD9CCF8E421EBEBF6EE3AA745B6521F07A8BEBA4A3C2FF612C`，与目标 hash 不同。

只读 CPG Query API 使用 manifest `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`（967 shards，`SourceSnapshotId=null`）。17 个 field symbol 查找及 `Player.cs` / `Projectile.cs` / `NPC.cs` member-use scope 均为 `complete`，每字段 1 个 symbol、1 至 5 个 facts；facts 混有 `confirmed` / `partial` 与 `Write` / `Unknown`。CPG 与 Version4 源码 hash 未绑定，查询结果不证明 writer/consumer 闭包或执行顺序。

新增 `PlayerAccessoryEffectSnapshotRebuildInput` / `PlayerAccessoryEffectSnapshotRebuildSystem` 只重建 15 个 `ResetEffects` tick flags；`ResetTickFlags` 只清这些字段，保留 `brokenMirrorBadLuck` 和 `wearsRobe`。不处理 buff/equipment 映射、Projectile 冲突、NPC scope consumer 或生产调度。专用 verifier 覆盖 15 项 rebuild/reset 并断言两项独立 lifecycle 字段保持原值；完整生产 handoff 与行为等价为 `unknown` / `not-run`。

### 2026-10-01 companion capability snapshot

目标 Version4 `Player.cs` SHA-256=`E5B301E3401F61E37BF3291754670BCC123D6593EDD94597C4E9109C4030FE86`。14 个字段在 `Player.cs:1670-1710` 声明；`UpdateBuffs` 的 pet helper 与 flag 分支更新它们（`:5322,5678-5865`），`ResetEffects` 在 `:10415-10428,10568-10594` 清除全部 14 项。Version4 `Projectile.cs` 对 13 项有 pet death clear / lifetime 访问（例如 `:38301-38501`）；文本和 CPG 的 Player / Projectile scope 对 `companionCube` 没有 Projectile use fact。最终 Player / Projectile 写入顺序与 pet entity lifecycle owner 仍为 `crossSubsystemOwner: integration-review`。

用户指定的完整参考项目是不同快照；其 `Player.cs` SHA-256=`367E6C3249F064DD9CCF8E421EBEBF6EE3AA745B6521F07A8BEBA4A3C2FF612C`、`Projectile.cs` SHA-256=`8D6428FA1D098B26ADFDC1D6CC028251814B40CD3ACE576F0BFF462FB19D6038`。完整参考 `Projectile.cs:47070-47072` 对 `companionCube` 可见额外死亡清除 / lifetime 读取；Version4 无对应文本证据，不移植该路径。

只读 Version4 CPG Query API 使用 manifest `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`（967 shards，`SourceSnapshotId=null`）。14 个 field symbol 查询与限定 `Terraria/Player.cs` / `Terraria/Projectile.cs` 的 member-use 查询均为 `complete`；每字段 1 个 symbol、2 至 4 个 facts。facts 混有 `confirmed` / `partial` 和 `Write` / `Unknown`，不证明动态 caller、owner 或执行顺序，且未绑定当前源码 hash。

新增 `PlayerCompanionCapabilityRebuildInput` / `PlayerCompanionCapabilityRebuildSystem`，只将调用方解析的 14 个 bool 复制到现有 Component，`Reset` 清零全部 14 项；不生成 / 销毁 Projectile，不做 death clear，不负责 buff/content 映射或 scheduler。独立 verifier 覆盖 14 项全量置真与 reset 清零；生产输入映射、Player/Projectile 交错顺序、entity owner 和完整行为仍为 `unknown` / `not-run`。

### 2026-10-01 unlock/preference implementation status

`PlayerProgressionCommitSystem` 有三种 `UnlockPlayerProgressionKind` typed commit，并以 `UnlockedBiomeTorches`、`AteArtisanBread`、`UnlockedSuperCart` 当前状态拒绝重复解锁；Super Cart preference command 直接设置 `EnabledSuperCart`，只读 Query 分别组合 Biome Torch 外部偏好和 Super Cart unlock/preference。Version4 `Player.cs:1475-1481,3042-3070` 声明四项字段和 effective accessors；`NetMessage.cs:179-192` / `MessageBuffer.cs:275-287` 有网络位序读写。只读 CPG Query API 在 Player / NetMessage / MessageBuffer scope 中对四个字段的 symbol 与 member-use 请求均为 `complete`，每字段返回 3 至 4 facts；facts 混有 `confirmed` / `partial` 和 `Write` / `Unknown`，数据库 `SourceSnapshotId=null`，不关闭来源、消费关系或网络顺序。用户指定的完整参考源码 `Player.cs:45852-45881` 有 Super Cart、Artisan Bread 和 Biome Torch 的 item-use writer，包含各自的动画 / item-time / 未解锁条件；这些行为属于不同参考快照，不能视为目标 Version4 已证实入口。

`Test/Terraria.Player.Progression.Verification/Program.cs` 现直接验证 Golfer 上限/重复 token/拒绝后重试、Angler 计数递增与重复 token 拒绝、DD2 flag 设置与重复 token 拒绝，以及三种 unlock 分支、未知 progression、空 token、Biome Torch preference 与 unlock 分离、Super Cart 默认 preference、禁用/启用、同 token preference 重复赋值和拒绝时状态不变。上述只将各自本地 reducer/commit 核心标为 `local-core-verified`；Angler 物品与奖励顺序、DD2 生产事件 writer、命令授权、生产 caller、存档恢复、网络 projection 顺序和完整行为仍为 `unknown`，不表示迁移接入或 Version4 行为等价。token 只校验非空而不做独立去重；偏好重复赋值依赖 owner 串行提交和最后写入顺序。

## 验收状态

本设计当前为 `proposed` / `partial`；minion/capacity/fishing 与多组 pet capability focused cores、world-object pet、legacy pet/effect flags、15 项 accessory tick flags、14 项 companion flags、Golfer/Angler/DD2 与 unlock/preference 局部核心，以及 14 字段 persistence snapshot mapping 均有对应 verifier 记录。完整 save/load、packet 行为和 P05 行为矩阵仍为 `not-run`。局部结果仅适用于各自执行过的 reducer/rebuild/commit/projection 用例。既有 consumed-upgrade 断言覆盖六种 item type 映射、`itemAnimation` / `ItemTimeIsZero` 门槛、一次提交、重复消费拒绝和 unsupported upgrade。这些不验证 Version4 行为或生产组合。升级边界结论前需补齐关键入站调用、动态/事件入口、source-to-target 快照身份、save/network 字段映射、跨分区 owner 和运行时调度证据。迁移完成必须由实际 `src/NSSLC` 生产组合上的必需行为测试支持；文档、源码形状、CPG `complete`、编译或 verifier 单独通过均不充分。
