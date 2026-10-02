# P09 玩家库存、装备、Buff 与防御装载 System 设计

documentKind: system-design  
partitionId: P09  
taskId: AUTH-SYS-P09  
derivedFrom: D:\TRbackup\NLTX\docs\system-decomposition\reports\2026-09-18-system-decomposition-authoritative-P09-player-inventory-equipment.md  
sourceReportSessionId: 811f7b34369747f088123f4f0224b106  
inputReport: D:\TRbackup\NLTX\docs\migration\ledgers\authoritative-20-partitions\P09-Player-Inventory-Equipment.md  
claimInputReport: D:\TRbackup\NLTX\docs\migration\ledgers\authoritative-20-partitions\P09-Player-Inventory-Equipment.md  
outputReport: D:\TRbackup\NLTX\docs\system-decomposition\reports\2026-09-18-system-decomposition-authoritative-P09-player-inventory-equipment.md  
settledSessionId: 811f7b34369747f088123f4f0224b106  
targetSource: D:\TRbackup\Version4  
fullReferenceSource: D:\TRbackup\无任何删减通过编译  
ecsReferenceSource: C:\Users\shan\Downloads\ECS\space-station-14-master  
migrationRoot: D:\TRbackup\NLTX\src\NSSLC  
designStatus: proposed  
systemImplementationStatus: partial-p09-core  
verificationStatus: partial-local-verifier  
sourceModified: true  
sourceChangeContext: mixed-existing-and-current-p09-staged-verifier  
existingSourceChangesObserved: true  
migrationStatus: not-claimed  
buildRun: true  
verifierRun: true  
buildRunScope: through-staged-packet-147  
verifierRunScope: through-staged-packet-147  
stagedPacket147BuildRun: true  
stagedPacket147VerifierRun: true  
testsRun: false  
runnerSettlement: completed  

## 1. 目的与范围

本文把 P09 authoritative System 报告细化为后续实现可以评审的 System 边界、概念 API、提交顺序和跨域合同。范围仍是 `PlayerGameplay` 下 8 个叶子组、105 个字段、0 个属性。本文不是实现记录，不把既有 Component 文件提升为 System owner，也不证明行为等价或迁移成功。

输入 `outputReport` 仍保持原报告的 `designStatus: proposed`、`verificationStatus: not-run`、`sourceModified: false` 和 `migrationStatus: not-claimed`。工作树中已有 P09 局部组合及 loadout/visibility network composition；当前 focused verifier 的结果只作为 observed/partial 记录，本文不把它们回写成报告结算，也不把它们升级为完整实现、API 等价或迁移成功。

P09 的玩家状态必须与 Item payload、Container 内容、Combat 冷却、Environment 能力、Animation 帧、Network 协议和 Persistence schema 分开表达。玩家到 Item/Container 的关系可以由 P09 持有，外部对象的内容和生命周期不因关系字段归入 P09。

P09 runner 会话已经以 `811f7b34369747f088123f4f0224b106` 完成并释放 lease。本设计只保留该 ID 作为来源 provenance；它不是游戏玩家 ID、网络 ID、实体 ID 或任何运行时 API 参数。

## 2. 证据基线

| 来源 | 已读或已查询事实 | 允许支持的结论 | 限制 |
| --- | --- | --- | --- |
| Version4 目标源码 | `Terraria/Player.cs`、`EquipmentLoadout.cs`、`Chest.cs`、`MessageBuffer.cs`、`NetMessage.cs`、`Terraria.IO/PlayerFileData.cs` | 字段声明、数组容量、局部分支、可见副作用、网络读写和局部顺序 | 调度、动态派发、异常闭包、卸载闭包和部分保存实现未闭合 |
| Version4 CPG | SQLite manifest `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`；967 shards、8,166,789 nodes、71,038,907 edges、1,317 diagnostics | 精确符号、选定源码范围内的 call-site、member-use 候选 | `SourceSnapshotId=null`；Unknown access、alias、callee effects、dynamic dispatch 不会因查询完成而闭合 |
| 只读 CPG Query API | 通过 `CpgEvidence.ps1` 的 `Initialize-CpgEvidence`、`Start-CpgEvidenceServer`、`Find-CpgSymbols`、`Find-CpgCallSites`、`Get-CpgMemberUses` 和 `Get-CpgCallableFacts` 查询选定范围 | 绑定查询的 source/target 端点、状态、覆盖范围和 gap | 查询只读现有 SQLite 索引；`complete` 只表示索引查询在预算内完成，不证明完整调用图、唯一 writer 或行为等价 |
| CPG 查询结果 | `TrySwitchingLoadout` 的 `MessageBuffer` 入站为 confirmed；`ReadAccessoryVisibility` 在 `MessageBuffer.cs` 选定范围有 2 个 confirmed call-sites；`SavePlayer` 的 `WorldGen` 入站为 confirmed；`UpdateDyes`、`UpdateBuffs`、`SerializedClone` 选定范围为 partial；`GetItem`、`FillAmmo`、`DoCoins` 局部内部 call 为 confirmed | 只固定这些 source/target 端点和查询范围；`ReadAccessoryVisibility` callable facts 仍是 partial | partial 或零命中不证明没有其他调用者 |
| 完整参考源码 | `D:\TRbackup\无任何删减通过编译\Terraria/Player.cs` 含完整的 `Serialize`、`Deserialize`、`InternalSavePlayerFile` 片段、`WorldItem` 重载和额外调用点 | 提供待核对的保存字段顺序、额外入口和补证线索 | 快照 hash、调用点和分支未证明与 Version4 相同；不能替代目标行为基线 |
| SS14 参考项目 | `Content.Shared/Inventory/InventorySystem*.cs` 的 slot lookup、equip/unequip validation、container insert 和事件 relay | 参考窄 API、System/Component 组织和验证前后事件边界 | 不作为 Terraria 业务行为、协议或等价性证据 |
| 当前 NSSLC | `src/NSSLC/Component/Player`、`Component/Items`、`Component/Combat` 已有若干状态和 Query | 记录现有候选形状、重复边界和可复用查询 | 文件存在不证明接入、唯一 writer、调度或行为迁移 |

目标/完整参考的复核指纹：Version4/Terraria/Player.cs 为 634,359 bytes、SHA-256 E5B301E3401F61E37BF3291754670BCC123D6593EDD94597C4E9109C4030FE86；完整参考 Terraria/Player.cs 为 1,491,040 bytes、SHA-256 367E6C3249F064DD9CCF8E421EBEBF6EE3AA745B6521F07A8BEBA4A3C2FF612C。MessageBuffer.cs 的指纹也不同（目标 0F474225FA9D98C539F61249619273A44E73F69A4CF388B44258433A2D15D5EE，完整参考 48AABBBF4E0967964D97598E0168E9252ADD3DD868C66C6DA46AA87670EACAAB），所以完整参考只能提升候选覆盖，不能替代目标源码行为基线。

## 3. 概念行为与不变量

### 3.1 Inventory commit

`GetItem` 是一次带资格判断、槽位优先级、堆叠、ammo、coin、void-vault fallback 和 effect intent 的提交行为。`PlayerItemSpaceQuery` 只能计算资格和候选槽位；它不能写 inventory、Item payload 或副作用端口。`FillAmmo` 只覆盖 54–57 槽的特定提交路径，`DoCoins` 保留 100-stack 升级和递归合并的兼容边界。

拒绝、容量不足和 unique item 必须返回明确的 remaining item/result，不得部分写入。接受提交后，logger、音效、PopupText、Achievements 和 `HandlePostAction` 通过显式 effect port 按旧顺序发出；副作用失败的重试和回滚规则目前 `unknown`。

### 3.2 Container relation

`bank`、`bank2`、`bank3`、`bank4` 是玩家到外部 Chest/Container 的关系和 capability，不是 P09 对容器内容、容量、世界锚点或持久化 Item ID 的所有权。`inventoryChestStack` 是操作 marker，必须有清除时点和 action scope，不得成为长期容器状态。

### 3.3 Equipment commit and loadout

`armor`、`dye`、`miscEquips`、`miscDyes` 是玩家到 Item 实例的关系。装备提交只由一个 Equipment writer 进行，Item type、stack、prefix、favorited、effect definition 留在 Items owner。`TrySwitchingLoadout` 必须保持校验、当前 loadout `Swap`、目标 loadout `Swap`、`CurrentLoadoutIndex` assignment 的事务顺序；失败不能只完成部分交换。

### 3.3A Loadout network ingress and visibility mask

目标 `MessageBuffer` 的 loadout/accessory 入站片段先读取 player 与 loadout 字节，调用 `TrySwitchingLoadout`，随后读取 16-bit accessory visibility mask，并由 `ReadAccessoryVisibility` 按 10 个 bit 展开到 `hideVisibleAccessory`。CPG 查询在选定 `MessageBuffer.cs` 范围确认一个 `TrySwitchingLoadout` 静态 call-site；这只固定局部关系，不闭合包身份、权限、重放、响应或异常语义。

候选组合必须保持：`parse/validate -> PlayerLoadoutSystem.Switch -> PlayerAccessoryVisibilitySystem.Apply`（输入为显式 mask）。网络 Adapter 不直接写 armor、dye、hidden-accessory 或 `CurrentLoadoutIndex`。目标源码的 `void` 调用后继续读取 mask 支持“即使切换被拒绝也按包顺序尝试 mask”的组合策略；该策略已由 focused verifier 覆盖，但不等于完整 MessageBuffer 行为等价，权限、重放、响应和动态消费者仍为 `unknown`。

工作树中的 `PlayerLoadoutPacket147Adapter.Decode` 采用完整四字节先解码、截断则不生成 request 的安全边界；这与目标源码“switch 后再读 mask”的异常时序不同，不能写成行为等价。`PlayerLoadoutNetworkSystem.ProcessPacket147` 另提供 staged 路径：先读取两字节 header 并执行 loadout switch，再读取 `UInt16` visibility mask；mask 截断时返回 `VisibilityMaskTruncated`，保留已提交的 loadout result，且不提交 visibility。两条路径的差异必须在真实入口接线前由协议 owner 决定，不能把 decode-only 的截断语义替代旧入口时序。

截至本文整理时，staged `ProcessPacket147` 已随 header 截断、mask 截断和完整包路径重新 build，并由 focused verifier 覆盖。该局部证据仍不证明生产接线、权限、重放、响应或完整协议等价；生产接线前必须由协议 owner 选择 staged decoder（保留旧的部分提交时序）或批准 parse-before-commit 的兼容变更，并为截断包定义明确的错误/恢复策略。

### 3.4 Buff/resource

`buffType[j]` 与 `buffTime[j]` 是成对状态，新增、刷新、删除、压缩、网络应用和保存读取都必须同时处理。`buffImmune` 是资格/效果集合，不得当作 buff duration 或 Combat hurt cooldown。`breath*`、`lava*` 属于玩家资源；`ignoreWater`、`lavaVision`、`lavaOpacity` 的最终消费者和重建顺序交 integration review。

### 3.5 Projection and defense

`c*` 颜色、visible equipment selection、robe/vanity/hide/shield fallback、held item presentation 和 armor effect flags 都是从已提交 authority 计算的 projection 或 capability snapshot。Projection 不得反写 armor、dye 或 loadout。shield/parry/tile lock 属于局部防御交互状态，`hurtCooldowns`、`meleeNPCHitCooldown`、Item logger 和 hovered chest 不得由 P09 重复拥有。

### 3.6 工作树中观察到的局部代码形状

当前工作树可见库存、coin merge、equipment commit、loadout、container relation、Buff reset、buff resource、equipment projection/effect、defense interaction、tick coordinator，以及 visibility/network composition 与 packet-147 decode/staged process 局部代码。受影响项目已串行 build（0 warning/0 error），`PlayerItemSpaceVerification` focused verifier 已覆盖 decode-only、header 截断、mask 截断和完整 staged 包路径并通过；该证据仍不覆盖真实 MessageBuffer 接线。本文仍把它们标成 observed/partial，因为真实入口、跨域 adapter、生产调度和完整 P09 行为没有闭合：

| 已观察类型 | 当前责任 | 可确认范围 | 尚未闭合 |
| --- | --- | --- | --- |
| `PlayerInventorySlotsComponent` | 59 个主库存槽、`inventoryChestStack` marker、trash 引用的候选 authority | 数组容量和关系形状；文件注释标为 `implemented-isolated-core` | 与 `PlayerInventoryComponent`/`PlayerInventoryState` 的全树唯一 writer、调度和持久化仍为 `partial` |
| `PlayerInventoryCommitCommand` | 一次库存提交意图 | command id、Item snapshot、candidate | 网络入口、重放窗口、权限和 session/entity 映射为 `unknown` |
| `PlayerInventoryCommitSystem` | 从 snapshot/preflight 到一次 commit plan 的库存决策候选 | 静态代码中可见空槽逆向选择、兼容堆叠、unique、ammo 54–57、coin 50–53、VoidVault fallback、commit-port 和 command-id 分支 | 真实 Item/Container 写入、logger/audio/PopupText/Achievements/post-action、scheduler 接线和运行结果为 `unknown` |
| `PlayerInventoryPickupSystem` | 组合库存 commit、coin merge 和外部 effect intents | `PlayerInventoryPickupCommand`、`CanGoIntoVoidVault` 显式门控、`PickupSound -> PickupLog -> PickupText -> Achievement -> PostAction` 顺序、effect-port failure result | 未接入 Version4 `GetItem`/world-item 入口；effect retry/rollback、Item payload、网络身份和 production scheduler 为 `unknown` |
| `PlayerCoinMergeSystem` | `DoCoins` 的 100-stack 升级、0–53 槽目标扫描和递归提交候选 | 静态代码中可见铜/银/金币 71–73、目标槽递增、源槽清空和无目标保留 1 个币的分支 | Item `SetDefaults` 完整 payload、effect 顺序、递归跨 port 失败补偿和真实入口接线为 `partial`/`unknown` |
| `PlayerLoadoutSystem` | `TrySwitchingLoadout` 的当前/目标 loadout 双向交换与 index 提交 | 20 armor、10 dye、10 hidden-accessory 快照；校验、原子 plan、重复命令和 lifecycle reset 已有局部代码 | MessageBuffer 真实接线、网络回写、Item payload、完整保存格式和 renderer/effect 接线为 `partial`/`unknown` |
| `PlayerAccessoryVisibilitySystem` | 把 16-bit network mask 展开为 10 个 hidden-accessory facts | 目标 `ReadAccessoryVisibility` 的 bit 顺序和固定长度；局部 System 已提交 mask、返回 snapshot 并支持 command 去重 | 真实 network adapter、authority、预测/重放和消费者为 `unknown`；rejected switch 的“仍应用 mask”是本组合的显式策略，不是全量等价证明 |
| `PlayerLoadoutNetworkSystem` | 按 packet-147 顺序组合 loadout switch 与 visibility mask，并提供 staged 读取路径 | 局部组合已实现：先 `Switch`，再 `Apply` mask；staged mask 截断保留 loadout result；三条局部路径已由 focused verifier 覆盖 | MessageBuffer binary reader、身份覆盖、响应发送、协议版本和真实调度为 `unknown` |
| `PlayerLoadoutPacket147Adapter` | 提供 decode-only 与 staged header/mask 读取边界 | 局部 Adapter 已保留 wire player index、显式 authority index，并返回显式截断结果；decode-only/staged 局部语义已验证 | 不写 loadout/appearance、不发送响应；权限、重放和调度仍为 `unknown` |
| `PlayerEquipmentRelationComponent` / `PlayerEquipmentCommitSystem` | armor/dye/misc equipment/misc dye 的 Item entity relation 提交 | 20/10/5/5 槽位、共享 revision、expected revision/current item 校验、跨槽重复装备拒绝、equip/replace/unequip、原子写入和 effect-rebuild 标记 | Item payload、装备定义规则、网络/保存入口、effect rebuild 实际执行和全树唯一 writer 为 `partial`/`unknown` |
| `PlayerEquipmentEffectSystem` | 装备派生表现状态的 tick reset、显式事实重建和不可变快照 | 11 个现有 `PlayerEquipmentEffectStateComponent` 字段；reset 不触碰 Item、Combat 或 renderer owner | 完整 `UpdateArmorSets`/Buff/坐骑效果计算、`shroomiteStealth` 等其他字段和下游消费为 `partial`/`unknown` |
| `PlayerEquipmentProjectionSystem` | 从 Item relation、dye relation、hide 输入计算可见头身腿和 `c*` 颜色 | `UpdateDyes` 的 dye 顺序、20 槽隐藏/翅膀例外、shield fallback、vanity 10/11/12 覆盖；缺少 Item metadata 时返回计数 | ArmorID 上界、accessory/robe/mount/frame 完整规则、renderer/network sink 为 `partial`/`unknown` |
| `PlayerContainerRelationSystem` | bank/bank2/bank3/bank4 关系和 VoidVault capability 的绑定/解绑 | 关系引用、VoidVault available/open 校验、重复命令和 lifecycle reset 已有局部代码 | Chest 内容、容量、world anchor、transfer order、持久化 ID 和真实容器 adapter 未闭合 |
| `PlayerStatusEffectSystem` reset boundary | Buff 44 槽 owner 的 immunity reset 与 lifecycle 清理 | `ResetForTick` 清空免疫，`ResetForLifecycle` 清空配对槽和免疫；既有 Apply/Tick 仍是局部 System | Version4 全量 AddBuff 互斥规则、UpdateBuffs effect rebuild、Environment/Combat capability 和网络入口为 `partial`/`unknown` |
| `PlayerBuffResourceSystem` | breath/lava 资源及派生环境 capability 的 reset、rebuild、tick 和 snapshot | `PlayerResourceStateComponent` 的 `breathMax/breath/lavaMax/lavaTime/ignoreWater/lavaVision/lavaOpacity`；目标源码确认 lava recovery 与 opacity step | 环境碰撞、伤害、装备定义和真实 scheduler 未接入，仍为 `integration-review`/`unknown` |
| `PlayerDefenseInteractionSystem` | shield raised/parry/cooldown 与 tile interaction lock 局部状态机 | 目标源码明确的 raise/release 常量 `15/20`、raise 时 parry 起点、item timer reset 输出、3 tick tile lock、重复 command；攻击冷却只作为结果输出 | `shieldParryTimeLeft` 的逐 tick 增长/结束规则、`hasRaisableShield`/输入资格、Combat cooldown 数组、音效/dust、World tile handoff 为 `partial`/`unknown` |
| `IPlayerInventoryItemQuery` / `IPlayerInventoryCommitPort` | Item/VoidVault 读边界和外部提交边界 | 查询快照、VoidVault 能力、原子 plan port | 真实 runtime adapter、错误/重试/回滚语义为 `unknown` |
| `PlayerItemSpaceVerification` | 局部观察向量 verifier 入口 | 约 10% 核心观察向量已通过，覆盖库存、装备、Buff/resource、loadout、visibility/network composition、decode-only、header/mask 截断、完整 staged packet-147、defense 和 container relation | 不是全量 P09 验收；真实 `GetItem`、MessageBuffer 接线、网络权限/重放/响应、保存、scheduler、跨分区 owner 和完整生命周期仍为 `not-run`/`unknown` |

以上类型没有被真实 `GetItem`、网络处理器、保存流程或 scheduler 接入；因此状态只能写为 `partial`，不能写为 API 等价、行为等价或迁移成功。

工作树中已观察到 `PlayerTickCoordinator` 的资源组合重载。旧 `Tick` 签名保持不变；新重载要求调用方提供已由 Buff/Environment owner 计算的 `PlayerBuffResourceRebuildInput` 和 `PlayerBuffResourceTickInput`，coordinator 只执行 `ResetForTick -> projection -> status reset/tick -> resource rebuild/advance -> effect rebuild -> defense advance` 的局部顺序，并在结果中返回可选资源 snapshot/rebuild result。rebuild 被拒绝时不会继续 advance，避免把资源验证或环境碰撞责任隐式移入 coordinator。防御 coordinator 的 `AdvanceTick` 目前仅可作为隔离代码形状观察；目标源码查询没有闭合 `shieldParryTimeLeft` 的逐 tick 演进，因此该演进不得写入行为等价结论。当前 focused verifier 已覆盖 staged packet-147 的局部顺序，但不能替代真实 MessageBuffer 行为验收。

## 4. Proposed System 边界

| System/边界 | 权威责任 | 只读输入 | 输出或副作用 | 状态 |
| --- | --- | --- | --- | --- |
| `PlayerTickCoordinator` | 固定 reset、commit、rebuild、projection 的 phase 和 barrier | player entity/session handle、已提交 facts | 调度调用，不拥有 P09 数组 | proposed |
| `PlayerInventoryCommitSystem` | inventory/trash、ammo、coin、void-vault 的原子提交 | Item/Container Query、pickup command、settings snapshot | slot delta、remaining item、effect intents | partial isolated core observed；真实入口和 Item/Container adapter 未接入 |
| `PlayerInventoryPickupSystem` | 把一次 pickup 组合为 commit、coin merge 和有序 effect intents | `PlayerInventoryPickupCommand`、`PlayerInventoryTransferSettings`、Item/VoidVault Query | `PlayerInventoryPickupResult`；通过 effect port 发出 sound/log/text/achievement/post-action intent | partial isolated core observed；未接 Version4 world-item/GetItem 入口 |
| `PlayerContainerRelationSystem` | 四个 bank/vault 关系和 capability | Container registry、world anchor、validated relation command | relation snapshot、Container transfer command | proposed separate；内容 owner integration-review |
| `PlayerEquipmentCommitSystem` | armor/dye/misc relation 的唯一提交者 | Item definition、slot Query、equip command | equipment committed fact、effect invalidation | proposed；工作树有候选代码形状，Item payload、effect rebuild 和真实入口未接入 |
| `PlayerBuffResourceSystem` | Buff pair、immunity、breath/lava resource | Buff definition、equipment/environment facts、buff command | paired slot delta、capability snapshot | partial isolated core observed；环境/Combat capability 未接入 |
| `PlayerEquipmentEffectSystem` | reset/rebuild 可重建装备派生效果 | equipment/buff facts、Item definitions | immutable effect capability | proposed；工作树有候选代码形状，完整效果计算未接入 |
| `PlayerEquipmentProjectionSystem` | visible IDs、`c*` colors、hide/robe/shield fallback | committed equipment/dye/loadout facts、只读 Item visual Query | renderer/network/UI snapshot | partial isolated core observed；只覆盖目标源码可确认的投影核心 |
| `PlayerAppearanceProjectionSystem` | frame/voice/hide 的组合投影 | Animation、Movement、Loadout、Network facts | appearance snapshot | proposed partial；owner integration-review |
| `PlayerLoadoutSystem` | 三套 armor/dye/hide 的原子交换 | validated index、local/dead/usage state | loadout result、index committed fact | proposed separate |
| `PlayerAccessoryVisibilitySystem` | 从显式 16-bit mask 生成固定 10 位 hidden-accessory snapshot | validated visibility mask | visibility snapshot；不写 loadout 或 equipment | partial isolated core observed；真实 network consumer unknown |
| `PlayerLoadoutNetworkSystem` | 组合 `PlayerLoadoutSystem` 与 `PlayerAccessoryVisibilitySystem`，保留 packet-147 局部顺序 | explicit request with packet/authority player indices and mask | loadout result + visibility result；不直接解析或发送 binary packet | partial isolated core observed；真实 MessageBuffer adapter/scheduler unknown |
| `PlayerLoadoutPacket147Adapter` | 把 binary packet 变成显式 `PlayerLoadoutNetworkRequest` | `BinaryReader`、command id、authority/main player facts | decoded request 或 truncated result；不写 authority | partial isolated adapter；真实 transport error/authority contract unknown |
| `PlayerDefenseInteractionSystem` | shield/parry/interaction lock 状态机 | validated shield command、equipment capability、Combat result | defense snapshot、attack cooldown request、tile release result | partial isolated core observed；Combat/World integration-review |
| `PlayerPersistenceAndCloneAdapter` | 文件、加密流、clone stream 和异常边界 | committed persistence snapshot、PlayerFileData | save/load/clone result | proposed separate；schema unknown |

System 名称是概念身份，不表示目标树中已有类型。实现时应优先使用 `D:\TRbackup\NLTX\src\NSSLC` 中现有领域目录，只有存在真实规模和边界时才新增文件或子目录。

## 5. Query、Command、Projection 与 Adapter 合同

| 类型 | proposed API | 强制约束 |
| --- | --- | --- |
| Query | 已观察 `PlayerItemSpaceQuery`、`IPlayerInventoryItemQuery`；其余为 `EquipmentDefinitionQuery`、`BuffDefinitionQuery`、`PlayerInventorySnapshotQuery`、`PlayerDefenseSnapshotQuery` 候选 | 只读、无 lazy write、返回快照；发现缓存写入时降级为 partial；库存提交仍通过 port 承担写入 |
| Command | 已观察 `PlayerInventoryCommitCommand`、`PlayerInventoryPickupCommand`、`PlayerEquipmentCommitCommand`；其余为 `ApplyBuffCommand`、`SwitchLoadoutCommand`、`SetShieldCommand`、`SetAppearanceCommand` 候选 | 由唯一 owner 校验并一次 commit；不携带 Item/Container 内容所有权；pickup 的 effect failure 不伪造回滚；装备 command 的 revision、expected current item 和 duplicate 语义只在隔离核心中确认，仍未接入 legacy/API 入口 |
| Projection | `EquipmentColorSnapshot`、`VisibleEquipmentSnapshot`、`BuffCapabilitySnapshot`、`DefenseSnapshot`、`NetworkPlayerSnapshot` | 单向读取 authority；不得 setter 回写或绕过 owner 发 command |
| Adapter/Port | `ItemEffectPort`、`ContainerRelationPort`、`PlayerNetworkAdapter`、`RendererSink`、`PlayerPersistenceAndCloneAdapter` | 只转换外部 ID、流、网络和渲染；不复制领域不变量或直接写 authority |

所有游戏对象 ID 必须保持语义分离：runtime entity ID、ItemInstanceId、persistent container ID、network slot 和 runner `sessionId` 不得互相替代。

## 6. API 组合与调度 DAG

### 6.1 Legacy 到 proposed composition

| Legacy entry | Proposed composition | 必须保持的兼容条件 |
| --- | --- | --- |
| `Player.Update(int)` | Coordinator -> reset -> dye/equipment projection -> buff/resource advance -> effect rebuild -> visible/appearance projection | 保留已观察的 `ResetEffects -> UpdateDyes -> clear buffImmune -> UpdateBuffs -> UpdateArmorSets` 局部顺序；完整 scheduler unknown |
| `Player.GetItem` | item-space Query -> `PlayerInventoryPickupSystem` -> inventory commit -> coin merge -> Item effect port | unique/favorite/coin/ammo/void-vault、remaining item 和 `PickupSound -> PickupLog -> PickupText -> Achievement -> PostAction` 顺序；effect failure 的回滚/重试 unknown |
| `FillAmmo` / `DoCoins` | inventory commit 的窄 command | ammo 54–57、100-stack merge、递归边界和拒绝结果 |
| `AddBuff` / `DelBuff` / `UpdateBuffs` | buff command -> paired slot commit -> capability rebuild | type/time pairing、免疫清理、44 槽扫描顺序 |
| `UpdateDyes` / `PlayerFrame` | equipment/appearance snapshot -> projection | projection 不写回 authority；间接 renderer/animation closure partial |
| `TrySwitchingLoadout` | validated `SwitchLoadoutCommand` -> atomic loadout swap | 本地/死亡/索引检查、两次 Swap、最后写 index |
| `MessageBuffer` loadout/accessory packet | decode-only `PlayerLoadoutPacket147Adapter.Decode`，或 staged header -> `PlayerLoadoutSystem.Switch` -> 16-bit mask -> `PlayerAccessoryVisibilitySystem.Apply` | 保留目标源码的读取顺序和 10-bit 展开；组合选择 rejected switch 后仍应用 mask；decode-only 与 staged 的截断/部分提交差异、身份/权限/重放/响应和完整协议行为为 `unknown` |
| `SavePlayer` / `SerializedClone` | persistence/clone adapter | server-side branch、stream isolation、字段 schema、异常包装目前 unknown |
| `MessageBuffer` / `NetMessage` | parse/validate adapter -> command 或 committed snapshot -> transport | adapter 不直接写 authority；包字段、重放、recipient 和版本 unknown |

### 6.2 Proposed DAG

```text
definition/catalog Query
  -> validated Command
  -> Inventory/Container/Equipment/Buff/Loadout/Defense commit
  -> PlayerTickCoordinator barrier
  -> EquipmentEffect rebuild
  -> Equipment/Appearance Projection
  -> Network / Persistence / Renderer / UI / Effect adapters
```

这是 proposed 顺序，不是已证明的运行时调度图。`Player.Update` 的局部源码顺序是 must-preserve 约束；相对 Main scheduler、Item、Combat、Environment、Network flush、Persistence 和 Renderer 的 phase 仍为 `unknown`。

## 7. 生命周期与副作用

| 阶段 | proposed 责任 | 当前缺口 |
| --- | --- | --- |
| Create/rehydrate | 建立 entity、Item relation、Container relation，校验数组长度和空 Item | 构造、反序列化和 hydration 顺序 partial |
| Activate/session bind | API 接收显式 entity/session handle | session/entity/network/persistent identity 关系 unknown |
| Tick/update | Coordinator 按 barrier 调用 commit、reset、rebuild、projection | full scheduler、重入和 client/server split unknown |
| Inventory/equipment command | owner 校验并一次提交，之后发 effect intent | retry、duplicate command、effect failure unknown |
| Buff/resource tick | 先 reset immunity，再成对更新 Buff 和资源 | Environment/Combat capability consumers partial |
| Projection | 消费 immutable committed snapshot | renderer/network 的完整生命周期 partial |
| Save/load | adapter 承担文件、map、server-side 和异常边界 | Version4 当前 serializer 不足，字段闭合 unknown |
| Clone | 每次请求使用隔离 reader/writer/stream | static stream concurrency、reset、失败语义 unknown |
| Destroy/unload | 清理 Item/Container relation、projection snapshot 和 clone resources | multi-world、disconnect/reconnect、unload closure unknown |

可见副作用包括 `GetItemLogger`、`SoundEngine`、`PopupText`、Achievements、`HandlePostAction`、网络 binary I/O、文件/加密流和 renderer sink。它们必须位于 effect boundary，不进入纯 Query 或长期玩家组件。

## 8. Integration Handoff

以下事项由 P09 单独裁决会越权，统一标记为 `crossSubsystemOwner: integration-review`：

- Item payload、stack/prefix/favorited、Container contents/capacity/world anchor 和 transfer ID；
- Buff definition、Environment capability、Combat immunity/cooldown 和 armor effect consumer；
- Animation frame、voice、renderer、UI 和 visible accessory 的最终 writer；
- MessageBuffer/NetMessage 的 authority、协议版本、重放、坏包响应、recipient 和排序；
- save/reload schema、server-side character branch、clone stream isolation、失败恢复和字段版本；
- runtime entity ID、persistent Item/Container ID、network slot、player slot 和 session identity；
- tick barrier、multi-world、disconnect/reconnect、destroy/unload 和跨分区事件注册。

P09 不删除或重命名当前 `src/NSSLC` 组件，不把现有 Component 文档、参考项目或文件存在性当作唯一 owner 证明。

## 9. Migration Behavior Contract

后续获得实现授权后，至少要对以下观察向量进行旧入口与新组合对照：

| 行为 | 输入/输出 | 观察要求 |
| --- | --- | --- |
| Inventory pickup | command + snapshot -> slot delta + remaining item | 59 槽优先级、stack、unique、coin/ammo、void-vault、拒绝无部分写、effect 顺序 |
| Equipment commit | equip/unequip -> relation delta | 20/10/5/5 长度、Item relation、原子提交、effect invalidation |
| Buff/resource | apply/remove/tick -> paired delta + capability | 44 槽 pairing、timer、immunity reset、breath/lava、网络重放 |
| Loadout switch | validated index -> swapped set + index | invalid/dead/local reject、current/target swap 和 index assignment 顺序 |
| Projection | committed facts -> visible IDs/colors | robe/vanity/hide/shield fallback，projection 前后 authority 不变 |
| Network | bytes/snapshot -> command/framed bytes | parse before commit、坏包不部分写、authority、recipient、ordering |
| Persistence/clone | snapshot -> file/clone result | schema、reload、server-side、stream isolation、异常和重试 |

本设计没有执行上述迁移、行为验证或运行时接线，因此不得写入 API 等价、行为等价或迁移成功结论。

## 10. Evidence Gaps and Blocking Decisions

| 缺口 | 状态 | 实施前动作 |
| --- | --- | --- |
| CPG 未绑定当前每个 Version4 文件快照 | partial | 对关键 source span 做 hash/line reconciliation 或重新绑定同一 revision |
| 数组 member-use 的 AccessMode、alias 和 callee effects 不完整 | partial | 针对 inventory/equipment/buff/loadout 做 focused read/write 查询并回源码核对 |
| `UpdateDyes`、`UpdateBuffs`、`PlayerFrame` 入站 caller 与 scheduler closure 不完整 | unknown | 继续枚举 Main、event、network、virtual/dynamic caller；零命中不作为负证据 |
| 当前 Version4 save/clone 方法体不完整 | unknown | 获取同一 Version4 接受版本的完整 schema；完整参考只能作补证 |
| 跨分区 owner、ID、协议、异常和卸载未闭合 | crossSubsystemOwner: integration-review | 由整合会话确定唯一 writer、snapshot、barrier 和失败合同 |
| 当前 NSSLC 存在重复 inventory/equipment/buff shapes | partial | 全树 writer search 和类型级 authority review；不得以双写过渡 |
| `VoidVaultItems` 的真实 runtime adapter、容器内容和 transfer contract 未闭合 | partial/unknown | 工作树存在按 `IPlayerInventoryItemQuery` 组织的候选夹具形状；仍需在真实 adapter 接入时确认类型、容量、持久化和失败语义，不能把夹具形状升级为行为等价 |
| Bank relation 的 `IsAvailable=true` 与空 `Container` 是否允许 | unknown | 当前隔离 System 只拒绝“不可用但仍带容器”的组合；需回到 Version4 bank 创建/打开路径和整合合同确认空关系的合法性，未确认前不能扩展为持久化或容量语义 |

## 11. Verification Plan

本设计的 `verificationStatus` 为 `partial-local-verifier`：受影响项目已串行 build 成功（0 warning/0 error），`PlayerItemSpaceVerification` focused verifier 输出 `PASS: player inventory, equipment, defense, loadout-switch, and container-relation core semantics`，并覆盖 decode-only、header 截断、mask 截断和完整 staged packet-147 路径。该 verifier 仍只覆盖局部组合，不能替代真实 MessageBuffer 接线、协议/权限/重放、保存/clone、scheduler、跨分区 owner 或完整行为测试。原 `outputReport` 仍是 `verificationStatus: not-run`。未来必须先完成 owner/入口/API 合同，再以真实迁移项目覆盖 `GetItem`、装备提交、loadout、Buff/resource、defense、网络、保存/clone、scheduler 和生命周期；局部 verifier 不能升级原报告、P09 或整个迁移为 verified/success。
