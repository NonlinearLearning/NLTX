# 19 — SpawnLifecycleAndLoot Version4 独立只读审查与 ECS 拆分设计报告

> 本报告只写入任务包指定的研究文件。所有设计类型、路径和接口均为 status: proposed，不表示当前 NLTX 已存在或已完成迁移。

## 0. 固定任务元数据

| 字段 | 值 |
| --- | --- |
| taskNumber | 19 |
| subsystemId | SpawnLifecycleAndLoot |
| layer | authoritative-simulation |
| currentNltxStatus | missing |
| originalBoundary | 初始生命周期/掉落责任；与 NPC、Projectile、Item、DeathPenaltyAndRevenge 交界 |
| relatedSubsystems | NpcAndTownSimulation、ProjectileSimulation、ItemContainerAndEconomy、DeathPenaltyAndRevenge、WorldStorage、NetworkSessionAndSectionStreaming |
| reportPath | D:\TRbackup\NLTX\docs\research\2026-09-05-version4-spawn-lifecycle-and-loot-public-decomposition.md |
| verificationStatus | not-run |

currentNltxStatus: missing 是任务元数据要求的固定状态，含义是当前 NLTX 尚无可确认的、统一跨域 SpawnLifecycleAndLoot 子系统。当前代码确实存在 NPC、Projectile 和 WorldItem 的局部模型及执行链，以下称为 partial local evidence，不能据此把该子系统标记为已实现。

## 1. 执行摘要

本次审查的权威覆盖基线是 D:\TRbackup\Version4。完整可编译参考只用于补充 Version4 已存在文件中的明确删减或空实现；tModLoader 文档只用于核对公开扩展边界；Space Station 14 只用于参考 ECS 的组件粒度、System/Query 和事件/结果边界，不能用来推断 Terraria 行为。

Version4 的结论：

- NPC.NewNPC 同时处理候选 slot 选择、兼容身份写入、对象重置、默认值初始化、位置/AI/生命周期激活和网络同步标记，是混合职责的遗留入口。
- NPC.checkDead 先做死亡确认和特殊死亡处理，再调用 NPCLoot，最后才写 active = false；失活后仍可能产生 Projectile 或推进世界事件。销毁、掉落和结构性生成不能压成无顺序的 Despawn。
- CommonDrop 是声明式掉落规则，负责资格/随机判断和旧式掉落入口，不是生命周期协调器，也不应拥有实体销毁或库存事务。
- Player 的 NPC 击杀 Hook、玩家死亡物品掉落、Projectile 击杀结果入口、复仇标记重建、网络解析和 WorldFile 存档分属不同边界。把它们集中到 NPC 或 DomeSimulation 巨型对象会重新形成跨域状态聚合。
- Projectile.Kill 在最终 active = false 前还会产生子 Projectile、Tile 转换、粒子、声音和其他类型特定效果。未来 DespawnSystem 应发布显式 CleanupWork，不能吞并全部 Projectile-specific effects。
- 网络身份、兼容 slot、持久化身份和 ECS 实体身份必须分开建模。NetMessage/MessageBuffer 是传输适配器和投影视图，不是权威状态 owner。
- 当前 NLTX 存在局部 NpcSpawnCommitSystem、NpcLifecycleSystem、NpcDeathSystem、NpcLootSystem、WorldItemSpawnSystem 和若干 Command，但仍由 DomeSimulation 统一编排，未形成覆盖 NPC、Projectile、WorldItem、复仇、网络和存档的统一提交协议。

建议的 proposed 流程：

    SpawnRequest
      -> Admission
      -> IdentityAllocation
      -> Initialization
      -> SpawnCommit
      -> Activation
      -> Simulation
      -> DeathResolution
      -> LootDecision
      -> LootCommit
      -> Despawn/Cleanup
      -> ReplicationProjection
      -> PersistenceProjection

这只是最终整合会话的调度候选。统一 SpawnCommitSystem、跨子系统身份值对象、掉落提交 owner、死亡与失活顺序、网络 spawn 语义和 restore 语义均属于 blocking-decision。

## 2. 子系统范围和不负责的内容

### 2.1 负责范围

SpawnLifecycleAndLoot 的候选职责是实体从请求到结果提交的跨域协调边界：

- 自然生成、事件生成、复仇重建、结构性派生生成和其他来源的 SpawnRequest 接纳；
- slot/identity 分配，以及 ECS identity、兼容 slot、network identity、persistent identity 的映射约束；
- 实体重置、定义初始化、初始 AI/位置/目标/生命周期写入和激活；
- 生命周期阶段转换、死亡事实提交、销毁意图和清理工作发布；
- 掉落候选计算、掉落结果幂等提交和失败恢复协议；
- NPC、Projectile、WorldItem 及结构性结果之间的 spawn/cleanup 交接；
- 向 ItemContainerAndEconomy、NetworkSessionAndSectionStreaming、WorldStorage 和领域模拟发送显式 Command、Event 或 Projection。

### 2.2 不负责

本报告不重新设计以下相邻子系统：

- NPC AI、目标选择、城镇/住房规则和完整战斗模拟；
- Projectile 类型特定行为、命中算法、Tile 转换语义和表现效果；
- Item 容器、库存、金币经济、拾取和物品合并的最终权威所有权；
- DeathPenaltyAndRevenge 的金币损失策略、marker 过期和玩家区域判定；
- 网络传输、客户端连接、section streaming 和包版本协商；
- WorldStorage 的文件格式、云存档、版本迁移和 IO；
- 渲染、声音、粒子、UI 和其他表现层副作用；
- 配方注册、制作条件和 Recipe 的运行时经济行为。

相邻子系统若影响本边界，例如掉落提交失败或网络 spawn authority，记录为 cross-subsystem finding 或 integration-risk，不把相邻子系统的最终 owner 写成已裁决事实。

## 3. 证据优先级、来源角色和检索记录

### 3.1 来源角色

| 来源 | 角色 | 状态 | 用途 | evidenceStatus |
| --- | --- | --- | --- | --- |
| D:\TRbackup\Version4 | 唯一行为覆盖基线 | Version4 当前目录 | 私有字段、调用链、生命周期、副作用 | confirmed；冲突另标 |
| D:\TRbackup\无任何删减通过编译 | 成员级补证 | 完整可编译参考 | 只补 Version4 已存在文件的空实现和删减 | version-drift；supplementation: full-reference-supplemented |
| D:\TRbackup\tmodloader-api-docs-stable | 公开 API 交叉验证 | tModLoader v2026.07 | 公开扩展、网络和声明式结果边界 | confirmed 或 partial |
| C:\Users\shan\Downloads\ECS\space-station-14-master | ECS 结构参考 | RobustToolbox 子模块为空 | 组件粒度、System/Query、Event/结果提交模式 | partial |
| D:\TRbackup\NLTX\src、Test、dome\src | 当前实现证据 | 当前工作树只读快照 | 局部模型和未闭合边界 | partial |

### 3.2 检索记录

| source | query | hits/evidence | gaps | stop_reason |
| --- | --- | --- | --- | --- |
| Version4 | CommonDrop、NewNPC、checkDead、NPCLoot、NewProjectile、Kill | 命中目标类型和直接调用链，见第 4 节 | ItemDropSolver 的全部规则不在本次深读范围 | 已取得规则、生命周期和结果提交证据 |
| Version4 | DropItems、OnKillNPC、TryDroppingItems | 命中 Player/EquipmentLoadout | Version4 OnKillNPC 为空体 | 转完整参考补证 |
| Version4 | MessageBuffer case 21/23、NetMessage case 21/23/27 | 命中 Item/NPC/Projectile 包 | case 23 在 1184 提前 break | 保留字段布局并标 evidence-mismatch |
| Version4 | WorldFile、CoinLossRevengeSystem | 命中实际持久化和复仇路径 | WorldFile 实际在 Terraria.IO | 以实际路径为准 |
| 完整参考 | DropEoWLoot、NPCLoot、OnKillNPC | 命中 Version4 同名成员补证 | 不回填 Version4 主基线 | 只补明确空实现/删减 |
| tModLoader | GlobalNPC、ModNPC、Recipe、ModPacket 精确成员 | 命中实际 HTML 类型页和锚点 | 公开 API 不说明私有顺序 | 只做边界交叉验证 |
| Space Station 14 | spawn、despawn、EntitySpawnEntry、ContainerFill | 命中最小相关内容系统 | RobustToolbox 核心为空 | 只保留结构参考 |
| NLTX | NpcSpawnCommitSystem、NpcLifecycleSystem、NpcLootSystem、DomeSimulation | 命中局部实现和集中编排点 | 无统一跨域事务和 focused verifier | 作为当前状态证据 |

D:\TRbackup\NLTX\约束\公共拆分约束.md 在本次读取中不存在。公共协议、ECS 文件组织约束、C# 风格约束和副作用隔离约束已读取；该文件缺失记录为 evidence-gap，不假定其存在。

## 4. Version4 真实代码证据表

以下是实际读取的 Version4 文件和实际行号。Read By/Written By 是源码中的主要读写者，不是 proposed owner。

| 证据 | 事实、读写者和生命周期 | 副作用/边界 | evidenceStatus |
| --- | --- | --- | --- |
| D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\CommonDrop.cs:5-63，CommonDrop、CanDrop、TryDroppingItem、ReportDroprates | CommonDrop 实现 IItemDropRule；字段保存 item、概率、数量范围和 ChainedRules。TryDroppingItem 读取 DropAttemptInfo、玩家随机和 NPC，成功时调用 CommonCode.DropItemFromNPC；ReportDroprates 计算概率并报告链式规则。 | 规则尝试可进入旧式物品掉落入口，但不是生命周期协调器。 | confirmed |
| D:\TRbackup\Version4\Terraria\NPC.cs:5897,5949,5951,6015,6031,6039 | active、SpawnedFromStatue、CanBeReplacedByOtherNPCs、netUpdate、spawnNeedsSyncing、realLife 混合承载激活、slot 替换、网络 dirty 和分段关系。 | 权威状态、兼容字段和派生网络标志集中在遗留 NPC 对象上。 | confirmed |
| D:\TRbackup\Version4\Terraria\NPC.cs:67051-67095，NewNPC | 先修正 Type；查找 slot；设置 spawnSlotProtected；创建 NPC 并写入 Main.npc 与 whoAmI；执行 ResetForNewNPC、SetDefaults、城镇数据；写位置、active、timeLeft、湿状态、ai[0..3]、target、spawnNeedsSyncing；成功返回 slot，失败返回 Main.maxNPCs。 | slot、实例初始化、激活、网络同步请求在一个入口内完成；失败是 sentinel，不是显式拒绝结果。 | confirmed |
| D:\TRbackup\Version4\Terraria\NPC.cs:67098-67142，GetAvailableNPCSlot、IsSpawnSlotInUse | 依据类型正向/反向扫描和 slot 0 限制；先找空闲 slot，再找 CanBeReplacedByOtherNPCs 的 slot；spawnSlotProtected 会阻止部分空闲 slot。 | 兼容 slot 选择/回收不能等同 ECS identity。 | confirmed |
| D:\TRbackup\Version4\Terraria\NPC.cs:66509-66520，SpawnNPC | 静态生成循环调用 RevengeManager.CheckRespawns。 | 复仇检查进入周期生成路径，但没有显式 SpawnRequest 类型。 | confirmed |
| D:\TRbackup\Version4\Terraria\NPC.cs:64571-64765，checkDead | 检查 active、realLife、life；处理分段 boss、Good World 派生生成、城镇事件和声音；Eater of Worlds 调用 DropEoWLoot；特殊 NPC 调整掉落位置；调用 NPCLoot；然后 active = false；之后仍可能生成 Projectile 和推进世界事件。 | 死亡确认、掉落、失活和后续结构性副作用有可观察顺序。 | confirmed |
| D:\TRbackup\Version4\Terraria\NPC.cs:64949，DropEoWLoot | Version4 的 DropEoWLoot(bool fromCheckDead = true) 为空体，但 checkDead 保留调用。 | 目标版本明确存在删减；不以完整参考替代 Version4。 | confirmed（空实现事实） |
| D:\TRbackup\Version4\Terraria\NPC.cs:65312-65430，NPCLoot、DoDeathEvents_BeforeLoot、NPCLoot_DropItems | NPCLoot 处理类型/世界条件、最近玩家、成就、Bestiary、banner、雕像资格、死亡前后事件，调用 NPCLoot_DropItems，再处理特殊结构掉落、金币和治疗。NPCLoot_DropItems 构造 DropAttemptInfo，注入玩家、NPC、专家/大师模式、simulation 标志和 Main.rand，调用 ItemDropSolver.TryDropping。 | NPCLoot_DropItems 是规则与生命周期的清晰 seam；金币、治疗、成就等不是单个 item rule 的职责。 | confirmed |
| D:\TRbackup\Version4\Terraria\Player.cs:11960-11992，ApplyDamageToNPC、OnKillNPC | ApplyDamageToNPC 创建 NPCKillAttempt，调用 StrikeNPC，发伤害网络消息；死亡时调用 OnKillNPC。Version4 OnKillNPC 为空体。 | 击杀 Hook 不是完整 loot coordinator。 | confirmed |
| D:\TRbackup\Version4\Terraria\Player.cs:26304-26400，DropItems、TryDroppingSingleItem | 清理 trash item，遍历 inventory、armor、dye、misc equipment、misc dye 和 loadouts，逐项掉落；单项路径创建世界物品、设置随机速度、发网络消息并扣减原容器。 | 物品结果、容器扣减和网络发布存在部分提交风险。 | confirmed |
| D:\TRbackup\Version4\Terraria\EquipmentLoadout.cs:6-68 | EquipmentLoadout 持有 Armor、Dye、Hide；Swap 交换 loadout；TryDroppingItems 遍历并调用玩家单项掉落。 | 装备容器/策略，不是一级生命周期系统。 | confirmed |
| D:\TRbackup\Version4\Terraria\Projectile.cs:10220-10517，NewProjectile | 向量重载转核心重载；核心路径找空 slot 或 oldest projectile；SetDefaults；写 whoAmI、位置、owner、速度、伤害、knockback、identity、Main.projectileIdentity；应用 source stats/banner/minion source，注入 AI，设置生命周期，发 NetMessage 27。 | Projectile slot、owner、network identity 混合在遗留对象和全局数组中。 | confirmed |
| D:\TRbackup\Version4\Terraria\Projectile.cs:12510-12514 | 命中 NPC 时创建 NPCKillAttempt，调用 StrikeNPC/StrikeNPCNoInteraction；击杀成立后调用 Main.player[owner].OnKillNPC。 | ProjectileSimulation 到 NPC/Player 结果 Hook 的跨域边。 | confirmed |
| D:\TRbackup\Version4\Terraria\Projectile.cs:46425-54619，Kill | inactive 时返回；清理 Main.projectileIdentity；timeLeft = 0；继续执行类型特定子 Projectile、Tile 转换、粒子、声音、伤害和其他清理副作用；最终在 54619 写 active = false。 | DespawnSystem 不能吞并 Projectile-specific effects。 | confirmed |
| D:\TRbackup\Version4\Terraria\MessageBuffer.cs:1082-1160 | Item 包 21/90/145/148 读取 slot、位置、速度、stack、prefix、reservation、shimmer 等；slot 400 用 EntitySource_Sync 调用 Item.NewItem，随后设置字段并转发。 | 网络导入/投影不是权威 item spawn transaction。 | confirmed |
| D:\TRbackup\Version4\Terraria\MessageBuffer.cs:1182-1307 | case 23 后续文本读取位置、速度、target、AI、difficulty、生命、statue、boss index、catchable owner；必要时 ResetForNewNPC、active = true、SetDefaults。 | 1184 紧接 case 23 即 break，使 1185-1306 按当前文本不可达；字段布局可用，可达性不可无条件宣称。 | evidence-mismatch |
| D:\TRbackup\Version4\Terraria\NetMessage.cs:638-768、769 起 | 写入 Item 包字段、NPC slot/位置/速度/target/生命/AI/netID/difficulty/statue 和 Projectile identity 等。 | 网络消息是 committed state 的序列化投影。 | confirmed |
| D:\TRbackup\Version4\Terraria\NetMessage.cs:1705-1760、2476-2489、2555-2558、2689-2693 | 根据 NPC/Projectile 状态、section 和玩家加入执行同步，包括 SyncNPCsForSection。 | 同步时机依赖 netUpdate/dirty；未来由 Projection 读取 revision。 | confirmed |
| D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:658-745、878-957 | LoadWorld/SaveWorld 负责文件外壳、版本选择和保存调度，进入 SaveWorld_Version2。 | IO 和临时文件是外部副作用。 | confirmed |
| D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:1186-1205、1747-1797、1808-1845、2951-3027 | 保存顺序包括 header、tiles、chests、signs、NPCs、tile entities、pressure plates、town manager、bestiary、creative powers、footer。SaveNPCs 保存 active town NPC 或 SavesAndLoads NPC；LoadWorld_Version2 按 section 指针调用 LoadNPCs。 | WorldFile 是持久化 Adapter/Projection；存档 ID 不能直接等同 runtime ECS identity。 | confirmed |
| D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:14-60、186-220 | RevengeMarker 保存位置、NPC netID、金币值、基础价值、HP 比例、statue、过期和 respawn lock；SpawnEnemy 调用 NPC.NewNPC，恢复状态并发送 NPC/coin marker 网络消息。 | marker 是缓存/触发记录，不是 runtime identity owner。 | confirmed |
| D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:358-427 | CheckRespawns 收集 active 且非 dead 玩家，清理 marker、检查相交、防重复 attempt、应用 discouraged，调用 SpawnEnemy，删除重建 marker 并发网络删除。 | 复仇策略应输出普通 SpawnRequest，marker 仍归 DeathPenaltyAndRevenge。 | confirmed |

### 4.1 Version4 关键调用链

自然/事件 NPC 生成：

    生成来源
      -> NPC.NewNPC
      -> GetAvailableNPCSlot / IsSpawnSlotInUse
      -> ResetForNewNPC / SetDefaults
      -> 位置、AI、target、timeLeft、active
      -> spawnNeedsSyncing / 后续 NetMessage

NPC 死亡：

    checkDead
      -> 死亡资格和特殊分段判断
      -> DropEoWLoot 或位置调整
      -> NPCLoot
         -> NPCLoot_DropItems
            -> ItemDropSolver.TryDropping
         -> 金币/治疗/结构性掉落和事件
      -> active = false
      -> Projectile/世界进度等后续副作用

Projectile 终止：

    Projectile.Kill
      -> identity 清理 / timeLeft = 0
      -> 类型特定子生成、Tile 转换、表现与伤害副作用
      -> active = false

## 5. 完整参考源码补证表

完整参考不改变 Version4 覆盖基线。以下 evidenceStatus 使用规范枚举，并显式记录 supplementation: full-reference-supplemented。

| 完整参考证据 | 补充内容 | 与 Version4 的关系 | evidenceStatus |
| --- | --- | --- | --- |
| D:\TRbackup\无任何删减通过编译\Terraria\NPC.cs:79599-79618，DropEoWLoot | 检查其他 Eater of Worlds 分段是否 active；最后分段设置 boss = true，调用 NPCLoot。 | Version4 NPC.cs:64949 为空体，不能宣称 Version4 已含完整逻辑。 | version-drift；supplementation: full-reference-supplemented |
| D:\TRbackup\无任何删减通过编译\Terraria\NPC.cs:80031-80105，NPCLoot | 补充完整参考的 NPC 掉落调用和死亡事件邻近实现。 | Version4 已有同名成员，只用于说明删减范围。 | version-drift；supplementation: full-reference-supplemented |
| D:\TRbackup\无任何删减通过编译\Terraria\NPC.cs:80782-80852 | 补充 NPCLoot_DropDungeonStuff、NPCLoot_DropTempleTraps、NPCLoot_DropLihzahrdStuff、NPCLoot_DropAltar、NPCLoot_DropHellforge。 | Version4 NPC.cs:66009-66013 有对应空体/删减成员，不回填行为。 | version-drift；supplementation: full-reference-supplemented |
| D:\TRbackup\无任何删减通过编译\Terraria\Player.cs:20750-20766，OnKillNPC | 完整参考只包含少量外部击杀来源 buff 和墓碑 Projectile 击杀城镇 NPC 的成就记录。 | Version4 Player.cs:11988-11992 为空体，证明 Hook 不是 loot coordinator。 | version-drift；supplementation: full-reference-supplemented |
| D:\TRbackup\无任何删减通过编译\Terraria.GameContent\CoinLossRevengeSystem.cs:190-221 | 完整参考在单机增加 moneyPing 分支，网络模式下再发送 NetMessage。 | Version4 :190-214 直接发送网络消息，存在版本漂移；行为基线仍为 Version4。 | version-drift；supplementation: full-reference-supplemented |
| D:\TRbackup\无任何删减通过编译\Terraria.GameContent.ItemDropRules\CommonDrop.cs | 对应 CommonDrop 成员语义与 Version4 一致。 | Version4 已足够作为主事实来源。 | confirmed |

## 6. tModLoader 公开 API 交叉验证

首页 D:\TRbackup\tmodloader-api-docs-stable\index.html:8 标题为 tModLoader: Main Page，页眉显示 tModLoader v2026.07。以下锚点来自实际 HTML。

| 类型页 | 成员实际行/锚点 | 公开语义和用途 | 限制 | evidenceStatus |
| --- | --- | --- | --- | --- |
| class_global_n_p_c.html:8，GlobalNPC Class Reference | CheckActive :163/:1216，EditSpawnPool :184/:1502，EditSpawnRange :187/:1547，EditSpawnRate :190/:1613；实际锚点分别为 ad0d11e4b6e9a895707c2b927902a7a1a、af0e8f0dfeef011bdd97801be5990973f、ae363ef35abb5e21e6babd9336a6164d8、aa03f7f9ed767f027736a2ce7f8063368 | 公开扩展可参与 active 判定、自然生成候选池、范围、速率和数量；支持资格与提交分离。 | 不证明 Version4 NewNPC 的私有顺序或 slot 行为。 | confirmed |
| 同一 GlobalNPC 类型页 | ModifyGlobalLoot :225/:1972，ModifyNPCLoot :251/:2316，NeedSaving :270/:2437；锚点 a909fdc092c79b35d98e516c97223f63a、a8cc5a60710bb10cca0f4f712dcc82063、a720e25c0e5e0fa8bcf47b89dfe6b5d9b | 公开边界区分全局/单 NPC 掉落规则和存档资格；支持 LootDecision 与 PersistenceProjection 分离。 | 不定义 proposed 事务提交方案。 | confirmed |
| 同一 GlobalNPC 类型页 | SendExtraAI :352/约:3336，SpawnNPC :371/:3485，SpecialOnKill :374/:3537；锚点 a69f3f922db2eee1a553abbb9f9b7b7af、a623eb9273b9f0c9f7e62cfdae8cc12b3、a81f052ab1db8ed3677cf6b99c0c9e825 | 公开 API 有生成后扩展、额外同步和击杀 Hook 边界。 | 只交叉验证扩展点。 | confirmed |
| class_mod_n_p_c.html:8，ModNPC Class Reference | CheckActive :178/:1203，ModifyNPCLoot :258/:2085，PreKill :332/:2729，说明约 :2751；锚点 a8690e744dc2cd66099c9d52799f00366、a1363bd911d2f60b9f52eb224db2fc304、ae9109fb61ac3fc600a4bf7c6b12d16ea | PreKill 返回 false 时跳过掉落、OnKill 和 boss flag，且只在单机/服务器执行；支持死亡准入、掉落、Hook 分阶段。 | 是 mod 契约，不是 Version4 checkDead 完整实现。 | confirmed |
| 同一 ModNPC 类型页 | SendExtraAI :349/:2858，SpawnNPC :376/:3079，SpecialOnKill :379/:3125 | 文档区分服务器发送、客户端接收、生成后 Hook 和特殊击杀 Hook。 | 不证明 NLTX 已实现兼容。 | confirmed |
| class_recipe.html:8，Recipe Class Reference | 类型说明 :99-103，Create 约 :1054-1087，Register 约 :1211-1225；锚点 a474a5953b0fbbdec4c68881277228d79、adbbadf559d181a9ed34ce2082c6f8f9f | Recipe 是 ingredients、tiles 和 resulting Item 集合；Create 建立定义，Register 完成注册。 | 不是掉落执行器或实体生成协调器。 | confirmed |
| class_mod_packet.html:8，ModPacket Class Reference | 类型说明 :94-95，Send 目录 :101-102、正文 :114-139；锚点 a54393cf0ee63ef5c497a6850d4e8b688 | ModPacket 用于 server/client 任意数据同步，可指定客户端或广播；支持网络 Adapter/Projection 边界。 | 不赋予消息权威身份所有权。 | confirmed |

annotated.html 的类型入口也已确认：GlobalNPC :853、ModNPC :1349、ModPacket :1351、Recipe :1679。公开文档不替代 Version4 私有实现。

## 7. Space Station 14 最小相关 ECS 参考

RobustToolbox 是 git submodule，当前 C:\Users\shan\Downloads\ECS\space-station-14-master 下该引擎子模块为空，没有可用的 EntityManager、EntitySystem、复制 owner 或持久化实现证据。

| 文件和实际成员 | 读取内容 | 参考用途 | evidenceStatus |
| --- | --- | --- | --- |
| C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Server\Spawners\EntitySystems\SpawnerSystem.cs:16-52，MapInitEvent、Update | 按时间、概率、数量和随机 prototype 生成实体。 | 时间/随机输入由 System 消费，实际生成由实体系统承载。 | partial |
| ...\SpawnOnDespawnSystem.cs:13-26，TimedDespawnEvent | despawn 事件后按坐标生成另一个实体。 | 销毁事件可产生后续 spawn request，但生成仍由 System 负责。 | partial |
| ...\ConditionalSpawnerSystem.cs:26-44、48-100、119-221 | map init、条件、概率、prototype、位置和 EntityTable 展开；文件自称部分逻辑混杂。 | 只参考事件和结果展开边界，不复制实现。 | partial |
| C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\Storage\EntitySpawnEntry.cs:78-145、192-204 | 根据 entry 概率、or-group、数量选择 prototype；注释明确该逻辑不生成实体，由调用者负责实际生成。 | 强力支持 Loot/Spawn rule evaluation 与实体提交分离。 | confirmed（结构模式） |
| ...\Content.Shared\Containers\ContainerFillSystem.cs:20-88 | 生成后插入容器；结果展开、生成、插入失败有处理。 | 生成结果与容器/经济提交分开，部分失败需显式策略。 | partial |

Space Station 14 无直接对应证据；以下 Terraria 边界仅由 Version4 真实代码和 NLTX 项目约束决定。

## 8. 成员、字段、方法、读写者和生命周期盘点

| Member | Declaring Type | Read By | Written By | Lifecycle | State Kind / Access Pattern | Candidate | Evidence / Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| active | Terraria.NPC | checkDead、slot 查询、NetMessage、WorldFile | NewNPC、checkDead、MessageBuffer | create/activate/death/persist | 权威生命周期状态，亦被投影读取 | EntityLifecycleState + ReplicationProjection | NPC.cs:5897,64571-64765；confirmed |
| whoAmI / NPC slot | NPC / Main.npc | 全局调用者、NetMessage、WorldFile | NewNPC、网络导入、替换 | create/reuse/cleanup | 兼容索引 | SpawnIdentityState.CompatibilitySlot | NPC.cs:67071-67073；confirmed |
| netID | Terraria.NPC | NetMessage、MessageBuffer、存档、marker | SetDefaults、网络导入、复仇恢复 | initialize/replicate/persist | 兼容定义/网络身份 | DefinitionReference / NetworkIdentity | MessageBuffer.cs:1185-1307、NetMessage.cs:692；confirmed；可达性另标 |
| spawnSlotProtected、CanBeReplacedByOtherNPCs | NPC / Main | slot 分配器 | NewNPC、保护 slot 更新 | allocate/reuse | slot 占用/替换策略 | slot allocator / admission | NPC.cs:67041-67142；confirmed |
| timeLeft、realLife | Terraria.NPC | 生命周期、死亡、网络 | NewNPC、复仇、AI/死亡 | initialize/update/expire | 生命周期计时、分段关系 | EntityLifecycleState、EntityReference | NPC.cs:6039,64575,67078；confirmed |
| spawnNeedsSyncing、netUpdate | Terraria.NPC | NetMessage、NPC update | NewNPC、AI/特殊状态 | activate/update/replicate | dirty/cache | ReplicationProjectionState | NPC.cs:6015,6031,67088；confirmed |
| DropAttemptInfo、CommonDrop fields | ItemDropRules | ItemDropSolver、规则链 | NPCLoot_DropItems、规则构造 | death/resolve | 声明式规则输入 | LootDecision / LootResolutionState | CommonDrop.cs:5-63；NPC.cs:65416-65430；confirmed |
| NPCKillAttempt | Player/Projectile 路径 | OnKillNPC | ApplyDamageToNPC、Projectile 命中 | hit/death | 击杀事实/Hook 输入 | DeathResolutionEvent | Player.cs:11970-11985；Projectile.cs:12510-12514；confirmed |
| Main.item、库存、loadout | Main / Player / EquipmentLoadout | 掉落、网络、拾取、经济 | Item.NewItem、TryDroppingSingleItem、容器扣减 | death/drop/commit | 经济权威状态 | ILootCommitPort 消费者 | Player.cs:26304-26400；confirmed |
| Projectile identity、owner、slot | Terraria.Projectile | NewProjectile、Kill、NetMessage | NewProjectile、Kill 清理 map | create/update/kill/reuse | 混合 slot/owner/network identity | 独立 SpawnIdentityState | Projectile.cs:10244-10517,46425-54619；confirmed |
| RevengeMarker | CoinLossRevengeSystem | CheckRespawns、区域判断 | cache、lock、expire、SpawnEnemy | cache/respawn/expire | 缓存/策略记录 | DeathPenaltyAndRevenge，输出 SpawnRequest | CoinLossRevengeSystem.cs:14-220,358-427；confirmed |
| network payload | NetMessage / MessageBuffer | server/client transport | serializer/parser | replicate/import | Adapter/Projection 快照 | ReplicationAdapter | NetMessage.cs:638-768；MessageBuffer.cs:1082-1307；confirmed/evidence-mismatch |
| saved NPC sections | WorldFile | load/save pipeline | SaveNPCs、LoadNPCs | persist/restore | 持久化快照 | PersistenceAdapter/Projection | WorldFile.cs:1747-1845,2951-3027；confirmed |

状态分类：权威状态是生命周期阶段、死亡事实、request 接纳、loot resolution/commit、ECS identity 和关系；派生值是 network dirty、active 投影视图、概率报告和 slot 查询；缓存是 RevengeMarker、projectileIdentity、slot protection 和 loot emission ledger；快照是 WorldFile/network payload；兼容字段是 whoAmI、slot、netID、Projectile identity、item slot 400；声音、粒子、死亡消息和 UI 是表现副作用。

## 9. 权威状态所有权表

| 状态/行为 | Version4 当前写入根 | proposed owner 候选 | 跨子系统情况 |
| --- | --- | --- | --- |
| SpawnRequest 接纳/拒绝 | NPC.NewNPC、Projectile.NewProjectile、RevengeMarker.SpawnEnemy | SpawnAdmissionSystem | crossSubsystemOwner: integration-review |
| 兼容 slot 分配/回收 | GetAvailableNPCSlot、Projectile slot 扫描、Item slot 规则 | SpawnIdentityAllocationSystem + 领域 Adapter | crossSubsystemOwner: integration-review |
| ECS identity | 当前 NLTX 局部 World/entity registry | SpawnCommitSystem 读取 identity allocator | crossSubsystemOwner: integration-review |
| 生命周期阶段/终止原因 | NPC.active/timeLeft、Projectile.active、NLTX NPC lifecycle | EntityLifecycleState + DespawnSystem | crossSubsystemOwner: integration-review |
| 死亡事实 | checkDead、StrikeNPC、NPCKillAttempt | DeathResolutionSystem -> DeathResolutionEvent | crossSubsystemOwner: integration-review |
| 掉落候选 | CommonDrop、ItemDropSolver、NPCLoot_DropItems | LootDecisionSystem/规则 Adapter | 结果 owner 未裁决 |
| 物品/金币提交 | CommonCode.DropItemFromNPC、Item.NewItem、Player drop | ItemContainerAndEconomy 经 ILootCommitPort | crossSubsystemOwner: integration-review |
| Projectile/结构清理 | Projectile.Kill 类型分支 | CleanupWorkSystem 发布，领域系统消费 | crossSubsystemOwner: integration-review |
| 网络 payload | NetMessage、MessageBuffer | ReplicationAdapter/Projection | owner 需整合裁决 |
| 存档快照 | WorldFile.SaveNPCs/LoadNPCs | PersistenceAdapter/Projection | WorldStorage 不拥有 runtime entity |
| 复仇 marker | CoinLossRevengeSystem | 保留在 DeathPenaltyAndRevenge | marker 不升级为 lifecycle owner |

SpawnLifecycleAndLoot 可以拥有跨域流程事实/命令边界，但不能未经整合裁决夺取 NPC AI、Projectile 特定行为、Item 经济、网络传输或存档 IO 的最终权威所有权。

## 10. Proposed ECS 组件拆分

以下均为候选设计，路径不是当前存在文件，全部标记 status: proposed。路径按 SpawnLifecycle 能力组织，不创建通用 Shared/Components、Common 或 Misc 聚合目录。

| proposed Component/值对象 | 候选路径 | 最小字段/不变量 | 状态 |
| --- | --- | --- | --- |
| EntityLifecycleState | dome/src/Terraria.Dome.Simulation/SpawnLifecycle/Components/EntityLifecycleState.cs | LifecyclePhase、IsActive、TerminationReason、PendingCleanup、Revision；终止幂等 | status: proposed；crossSubsystemOwner: integration-review |
| SpawnAdmissionState | dome/src/Terraria.Dome.Simulation/SpawnLifecycle/Components/SpawnAdmissionState.cs | request key、source kind、authority、admission、拒绝原因、attempt/retry | status: proposed |
| SpawnIdentityState | dome/src/Terraria.Dome.Simulation/SpawnLifecycle/Components/SpawnIdentityState.cs | EcsEntityId、CompatibilitySlot、NetworkId、PersistentId 分开；记录 authority/revision | status: proposed；crossSubsystemOwner: integration-review |
| LootResolutionState | dome/src/Terraria.Dome.Simulation/SpawnLifecycle/Components/LootResolutionState.cs | 规则版本、resolution sequence、resolved tick、RandomContext、幂等 key、commit state | status: proposed |
| ReplicationProjectionState | dome/src/Terraria.Dome.Simulation/SpawnLifecycle/Components/ReplicationProjectionState.cs | last committed revision、dirty fields、network mapping、last result | status: proposed；crossSubsystemOwner: integration-review |
| EntityReference | dome/src/Terraria.Dome.Simulation/SpawnLifecycle/References/EntityReference.cs | target entity、reference kind、expected revision；禁止 slot 代替 entity ID | status: proposed；crossSubsystemOwner: integration-review |
| LootAttribution | dome/src/Terraria.Dome.Simulation/SpawnLifecycle/Components/LootAttribution.cs | killer/source entity、interaction set、authority、award policy key | status: proposed；crossSubsystemOwner: integration-review |

不创建包含 NPC、Projectile、Item、网络和存档全部字段的巨型 SpawnLifecycleComponent。共同生命周期概念可共享，领域属性和关系应分离。

## 11. Proposed System、Query、Command、Adapter 和 Projection 边界

### 11.1 System 契约

| proposed System | 输入 | 输出/方向 | Seam | Depth/Leverage/Locality | 状态 |
| --- | --- | --- | --- | --- | --- |
| SpawnAdmissionSystem | SpawnRequest、资格 Query、authority policy | AdmissionResult；不创建实体 | ISpawnAdmissionPolicy | 深/高/只写 admission | status: proposed |
| SpawnIdentityAllocationSystem | 已接纳 request、slot/identity allocator | SpawnIdentityAllocated；失败可重试 | ICompatibilitySlotAllocator、IEntityIdentityAllocator | 深/高/只写身份 | status: proposed；crossSubsystemOwner: integration-review |
| SpawnInitializationSystem | identity、definition、位置、初始 AI/目标、source stats | initialized facts；不发网络 | IDefinitionInitializer、IInitialStatePolicy | 中深/中高/输入明确 | status: proposed |
| SpawnCommitSystem | validated identity、initialized facts、revision | ECS entity、SpawnCommitted | ISpawnCommitPort 或 integration World facade | 深/极高/跨域 | status: proposed；crossSubsystemOwner: integration-review |
| SpawnActivationSystem | committed entity、初始 lifecycle | EntityActivated、dirty | IActivationPolicy | 中/中/创建与模拟分开 | status: proposed |
| DeathResolutionSystem | health/lifecycle、击杀来源、领域结果 | DeathResolutionEvent；不掉落/删除 | IDeathResolutionPolicy | 深/高/只提交事实 | status: proposed；crossSubsystemOwner: integration-review |
| LootDecisionSystem | death event、规则、RandomContext、attribution | LootDecision；不改 inventory/world item | ILootRuleEvaluator、legacy Adapter | 深/高/可重放 | status: proposed |
| LootCommitSystem | LootResult、经济 port、ledger | committed/failed、补偿或 retry | ILootCommitPort | 深/极高/跨 Item | status: proposed；crossSubsystemOwner: integration-review |
| DespawnSystem | 终止事实、reason、revision | CleanupWork、DeactivationRequested | ICleanupEffectPort | 深/高/不吞领域副作用 | status: proposed；crossSubsystemOwner: integration-review |
| CleanupWorkSystem | CleanupWork、child spawn、relationship/tile work | 领域 effect commands、完成/失败 | ICleanupEffectPort、retry policy | 中深/高/保持 locality | status: proposed；crossSubsystemOwner: integration-review |

### 11.2 Query 契约

Query 只能进行可重复的纯计算或资格判断，不写权威状态。以下签名草图均为 status: proposed：

    CanAdmitSpawnQuery(SpawnRequest, WorldSnapshot) -> AdmissionDecision
    ResolveAvailableIdentityQuery(SpawnRequest, IdentitySnapshot) -> CandidateIdentity
    ResolveLootCandidatesQuery(DeathResolutionEvent, LootRuleSnapshot, RandomContext) -> LootDecision
    CanCommitLootQuery(LootResult, EconomySnapshot) -> CommitEligibility
    CanRestoreSnapshotQuery(PersistenceSnapshot, IdentitySnapshot) -> RestoreDecision

Query 必须把时钟、随机数、网络和存档输入显式化；不得调用 Item.NewItem、修改 active、扣库存或发网络。

### 11.3 Command/Event 契约

| 类型 | 最小内容 | 方向/约束 | 状态 |
| --- | --- | --- | --- |
| SpawnRequest | source kind、definition、position、initial state、requested identity、request key、authority、expected revision | 来源只提交请求；同 key 只能提交一次 | status: proposed；crossSubsystemOwner: integration-review |
| SpawnCommit | request key、allocated identity、initialized facts、commit revision | 只能由 validated request 进入 | status: proposed；crossSubsystemOwner: integration-review |
| DeathResolutionEvent | source entity、death fact、reason、attribution、event key | 只表达死亡事实 | status: proposed；crossSubsystemOwner: integration-review |
| LootDecision | source entity、候选结果、规则版本、RandomContext、attribution、resolution key | 可重放，不改 Item/WorldItem | status: proposed |
| LootResult | result ID、item/gold/container effect、source、attribution、commit state、expected revision | 每个结果幂等提交 | status: proposed；crossSubsystemOwner: integration-review |
| CleanupWork | target entity、reason、child spawn、tile work、relationship cleanup、retry policy、work key | 领域系统消费各自部分 | status: proposed；crossSubsystemOwner: integration-review |
| EntityDeactivationRequested | entity reference、expected revision、reason、after-effects complete | 清理协议满足后才最终 inactive | status: proposed；crossSubsystemOwner: integration-review |

### 11.4 Adapter/Projection 契约

| proposed 边界 | 职责 | 禁止事项 | 状态 |
| --- | --- | --- | --- |
| ILootCommitPort | Commit(LootResult, EconomyCommitContext) -> LootCommitOutcome，交给 ItemContainerAndEconomy | 规则直接改库存、创 WorldItem 或发网络 | status: proposed；crossSubsystemOwner: integration-review |
| ICleanupEffectPort | Submit(CleanupWork) -> CleanupOutcome，分发 Projectile/tile/relationship effects | 通用 DespawnSystem 实现全部 Projectile 分支 | status: proposed；crossSubsystemOwner: integration-review |
| ReplicationAdapter | committed facts/revision -> 网络 payload，处理 network identity | 网络 payload 成为权威 root；客户端反写 server | status: proposed；crossSubsystemOwner: integration-review |
| PersistenceAdapter | committed snapshot -> WorldStorage；restore 产生 decision/request | WorldFile 拥有 runtime entity；PersistentId 代替 ECS ID | status: proposed；crossSubsystemOwner: integration-review |
| LegacyItemDropAdapter | CommonDrop/legacy ItemDropRule -> LootDecision 候选 | 宣称 API 兼容或规则成为提交 owner | status: proposed；当前同名局部 Adapter 仅为 partial evidence |

## 12. 调用方向和 Proposed System 顺序

### 12.1 调用方向

    NaturalSpawn / EventSpawn / RevengePolicy / NetworkAdapter / PersistenceAdapter
                            |
                            v
                      SpawnRequest
                            |
                            v
                   SpawnAdmissionSystem
                            v
               SpawnIdentityAllocationSystem
                            v
                 SpawnInitializationSystem
                            v
                    SpawnCommitSystem
                            v
                 SpawnActivationSystem
                            v
                    Domain Simulation
                            v
                 DeathResolutionSystem
                            v
                   LootDecisionSystem
                            v
          LootCommitSystem -> ILootCommitPort -> ItemContainerAndEconomy
                            v
                      DespawnSystem
                            v
              CleanupWorkSystem / ICleanupEffectPort
                            v
                ReplicationProjection / PersistenceProjection

候选调度顺序：

    Admission
      -> IdentityAllocation
      -> Initialization
      -> SpawnCommit
      -> Activation
      -> Simulation
      -> DeathResolution
      -> LootDecision
      -> LootCommit
      -> Despawn/Cleanup
      -> ReplicationProjection
      -> PersistenceProjection

约束：

1. 未通过 Admission 和 IdentityAllocation 的 request 不得进入实体提交。
2. SpawnCommit 先于 Activation，避免观察到半初始化实体。
3. DeathResolution 只发布事实；LootDecision 不修改生命周期或经济。
4. LootCommit 默认先于需要掉落结果的最终清理；若改为异步必须有补偿协议。
5. CleanupWork 的 child spawn 进入下一次 commit 子阶段或 next-tick 队列，不能递归修改当前遍历集合。
6. Replication 只读 committed revision；Persistence 只读 committed snapshot。
7. 顺序由调度契约和 verifier 表达，不能由文件/目录顺序表达。

NPC.checkDead 和 Projectile.Kill 都支持“特定死亡效果先于最终 inactive”的行为保持要求。建议保留 Dying/PendingCleanup 中间阶段，但最终 active=false、loot、cleanup、replication 的准确顺序仍是 blocking decision。

## 13. 持久化、网络和客户端投影边界

### 13.1 网络

Version4 NetMessage 序列化 NPC、Projectile、Item 的兼容字段，MessageBuffer 解析并可能创建/激活客户端对象。候选方向：

    committed authoritative facts
      -> ReplicationAdapter
      -> network payload
      -> MessageBuffer adapter
      -> client projection/import state

约束：

- NetworkId、NPC slot、Projectile identity 和 EcsEntityId 分开保存；
- client projection 只能写 client-side projection state；
- 重复包按 committed revision/request key 幂等，旧 revision 不覆盖新 revision；
- MessageBuffer.cs:1184 的 break 必须由最终整合会话确认真实来源和可达性；
- 网络生成是权威 SpawnRequest 还是 client import，不能由现有 MessageBuffer 写入动作单方面决定。

### 13.2 持久化

WorldFile.SaveNPCs 只保存 active town NPC 或 SavesAndLoads NPC，LoadWorld_Version2 按 section 指针加载。候选边界：

- PersistenceProjection 只输出 snapshot；
- PersistenceAdapter 负责版本字段、缺省值和读取失败；
- PersistentId 不复用 EcsEntityId、NPC slot 或 network identity；
- restore 必须明确重新走 Admission/Allocation/Initialization，还是受控 RestoreCommit；
- restore 不隐式发网络包或触发 loot，派生 spawn 必须是有 source kind 的显式 request。

### 13.3 公开扩展

tModLoader 的 GlobalNPC/ModNPC 将 CheckActive、SpawnNPC、PreKill、ModifyNPCLoot、SendExtraAI 和 SpecialOnKill 分开，支持 active 判定、生成候选、死亡准入、掉落规则和同步 payload 使用不同 seam。该事实只用于公开边界交叉验证，不是 NLTX API 兼容证明。

## 14. 当前 NLTX 状态映射

### 14.1 src 局部模型

当前 src 具有以下局部类型：

- D:\TRbackup\NLTX\src\Npc\NpcLifecycleComponent.cs
- D:\TRbackup\NLTX\src\Npc\NpcLifecycleStage.cs
- D:\TRbackup\NLTX\src\Npc\NpcLifetimeComponent.cs
- D:\TRbackup\NLTX\src\Npc\NpcDespawnReason.cs
- D:\TRbackup\NLTX\src\Npc\NpcEntityIdentityComponent.cs
- D:\TRbackup\NLTX\src\Npc\NpcInstanceId.cs
- D:\TRbackup\NLTX\src\Npc\NpcSlot.cs
- D:\TRbackup\NLTX\src\Npc\NpcNetId.cs
- D:\TRbackup\NLTX\src\Npc\NpcDefinitionReferenceComponent.cs
- D:\TRbackup\NLTX\src\Projectile\ProjectileLifetimeComponent.cs
- D:\TRbackup\NLTX\src\Projectile\ProjectileEndReason.cs
- D:\TRbackup\NLTX\src\Projectile\ProjectileOwnerComponent.cs
- D:\TRbackup\NLTX\src\Items\Loot\LootSourceComponent.cs
- D:\TRbackup\NLTX\src\Items\Loot\LootAttributionComponent.cs
- D:\TRbackup\NLTX\src\Items\Loot\LootSourceKind.cs
- D:\TRbackup\NLTX\src\Items\WorldItemComponent.cs
- D:\TRbackup\NLTX\src\Items\WorldItemReservationComponent.cs
- D:\TRbackup\NLTX\src\Items\ItemInstanceComponent.cs

这些文件证明局部身份、生命周期、loot attribution 和 WorldItem 状态已分开表达，不能证明统一 admission、identity allocation、loot commit 或 cleanup transaction 已实现。

### 14.2 dome 局部实现

| 当前文件/行 | 当前事实 | 结论 | 状态 |
| --- | --- | --- | --- |
| D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Npc\Systems\NpcSpawnCommitSystem.cs:17-168 | TryCommit 验证、检查占用、创建 ECS Entity 并写入多个 NPC 组件，返回 NpcSpawnCommitResult。 | NPC-specific commit，不是跨域提交器。 | partial |
| ...\Npc\Systems\NpcLifecycleSystem.cs:6-55 | Advance 在死亡/timeout 时直接写 lifecycle.IsActive=false 和 reason；DespawnCommand 一直为 null。 | 生命周期写入与统一销毁 Command 未闭合。 | partial |
| ...\Npc\Systems\NpcDeathSystem.cs:5-22 | 有 NpcDeathInput、NpcDeathResult、Evaluate。 | 局部死亡决策 seam，未连接全域 cleanup/loot/replication。 | partial |
| ...\Npc\Systems\NpcLootSystem.cs:12-112 | registry、Roll、CreateDrop，输出简化 NpcLootCommand。 | 已有局部规则/命令分离，但不等于完整 Terraria loot 语义。 | partial |
| ...\Npc\Systems\NpcLootEmissionLedger.cs:8-51 | 按 NPC/tick 去重，失败恢复 ledger。 | 局部幂等/恢复，不是全局 transaction。 | partial |
| ...\Items\Systems\ItemDropRuleSystem.cs:10-199 | Evaluate 用规则、条件和确定性随机生成 CreateWorldItemCommand。 | 规则计算与 WorldItem 创建已局部分离。 | partial |
| ...\Items\Systems\WorldItemSpawnSystem.cs:8-75 | TryCreate 验证、分配 replication ID、创建 WorldItem。 | WorldItem-specific commit，仍由 DomeSimulation 调用。 | partial |
| ...\Items\Systems\WorldItemDestroySystem.cs:6-39 | TryDestroy 采用 revision、active 和幂等检查。 | 仅覆盖 WorldItem cleanup。 | partial |
| ...\Items\Compatibility\LegacyItemDropAdapter.cs:9-126 | LegacyItemDropRecord 转换、验证和 command 创建。 | 有 legacy-to-command 方向，不证明兼容/等价。 | partial |
| ...\Npc\Commands\SpawnNpcCommand.cs:5-17、DespawnNpcCommand.cs:5、NpcLootCommand.cs:5-10、...\Events\NpcDeathEvent.cs:6-12 | 已有 NPC-specific Command/Event。 | 不能当作本报告 proposed 跨域类型完成版。 | partial |
| ...\Npc\Systems\NpcPipeline.cs / DomeSimulation.NpcPipeline.cs:14-135 | NPC 局部顺序含 Spawn、Damage、Lifecycle、Death、Loot、Replication。 | 不能直接升级为全局顺序契约。 | partial |
| ...\Tick\SimulationCommandQueue.cs:13-329 | 一个队列容纳 Damage、Projectile、Item、NPC、Liquid、Wiring 等 Command。 | DomeSimulation 仍是跨域 orchestration 聚合点。 | partial |
| ...\Simulation\DomeSimulation.cs:475-489、5002-5187、5811-5836 | 持有 NPC loot/lifecycle/death/ledger 并推进多个 pipeline。 | 尚无显式跨域提交层。 | partial |
| ...\Simulation\DomeSimulation.cs:6794-6861 | NPC spawn/despawn commit 有 slot、replication、revision 和 training dummy 处理。 | 与 NpcLifecycleSystem 直接写 inactive 的边界重复。 | partial |
| ...\Simulation\DomeSimulation.cs:7062-7100 | CommitItemDrops 先 WorldItem 创建，再扣库存并发 ItemDroppedEvent。 | 有部分提交风险，缺统一 rollback/compensation/retry。 | partial |
| ...\Simulation\DomeSimulation.cs:9450-9473 | SpawnNpcLoot 经 death event、ledger、loot system、WorldItem commit 后发 ItemDroppedEvent。 | 是 NPC 局部链，不是全域 owner。 | partial |

当前局部链：

    NPC death observation
      -> NpcDeathSystem
      -> NpcLootEmissionLedger
      -> NpcLootSystem
      -> CreateWorldItemCommand
      -> CommitWorldItemSpawn
      -> ItemDroppedEvent

主要缺口：

- 无统一跨 NPC、Projectile、WorldItem 的 SpawnRequest admission/identity allocation；
- NpcLifecycleSystem 与 CommitNpcDespawnCommands 双重写 inactive/reason/revision；
- NPC death -> loot -> cleanup -> replication 无统一契约；
- Player death drop、Projectile cleanup、revenge、network spawn、persistence restore 未接入同一验证模型；
- CommitItemDrops 的部分提交没有明确事务/补偿/retry；
- SimulationCommandQueue 是多领域设施，不是 SpawnLifecycleAndLoot owner；
- 现有 NPC verifier 不覆盖本报告第 15 节的全域场景。

## 15. Focused verifier 设计和建议命令

本节只提出 verifier，不创建测试项目或源码。候选路径 D:\TRbackup\NLTX\Test\SpawnLifecycleAndLoot.Verification\，status: proposed。

### 15.1 场景矩阵

| 场景 | 最小断言 |
| --- | --- |
| 自然生成 | 合法 request 经过 Admission、Allocation、Initialization、Commit、Activation，不产生半初始化实体 |
| 事件生成 | source kind 可区分，但与自然生成共享 commit boundary |
| 复仇重建 | marker 只负责策略，重建进入普通 SpawnRequest，不绕过 identity allocation |
| 网络 spawn | authority 明确，重复包按 revision/request key 幂等，client 不改 authoritative root |
| NPC 死亡掉落 | 一个 DeathResolutionEvent 只有一个 resolution sequence |
| Projectile 销毁 | child/tile/relationship cleanup 先有明确结果，再最终 inactive |
| 玩家死亡掉落 | 容器结果与扣减有明确 commit/补偿关系，WorldItem 失败不静默丢库存 |
| 重复 spawn request | 同 request key 只有一个 committed entity |
| 重复 death/loot | 同 event/resolution key 只产生一次 LootDecision 和结果 |
| slot/ID 回收 | slot 可复用但 ECS identity 不重叠，旧 revision 不命中新实体 |
| identity collision | ECS/network/persistent/compatibility ID 冲突分别报告 |
| partial loot commit | 部分失败可恢复、补偿或重试，不出现静默物品丢失 |
| client projection | 只应用 committed revision，客户端消息不能反写 authority |
| persistence handoff | save 只读 committed snapshot，restore identity 策略明确 |
| failed request retry | retry 不产生第二实体或第二份掉落 |
| timeout/unknown result | unknown 不当 success，不无限重试 |
| child Projectile/structural spawn | child spawn 进入明确 commit 子阶段，不递归破坏当前遍历 |
| death -> loot -> cleanup | 记录 revision/event 阶段序列，逆序即失败 |
| source convergence | 自然、事件、复仇、网络/restore 使用同一 commit contract |

### 15.2 断言集合

至少验证：

- requestKey 只允许一个 SpawnCommitted；
- CompatibilitySlot、EcsEntityId、NetworkId、PersistentId 不互相冒充；
- DeathResolutionEvent immutable，重复事件不重新调用 loot rule；
- LootDecision 使用显式 deterministic RandomContext，可离线重放；
- LootCommit 部分失败有恢复、补偿或重试；
- NPC 最终 inactive 不早于协议要求的死亡/掉落/cleanup；
- Projectile child spawn 与 terminal cleanup 顺序确定；
- Revenge respawn 使用普通 spawn commit；
- client projection 不产生 authoritative write；
- persistence adapter 不重新分配 runtime identity，除非 restore policy 明确允许。

### 15.3 未来命令草案

仅供未来执行参考，本次没有执行。候选项目尚未创建：

    pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 test .\Test\SpawnLifecycleAndLoot.Verification\SpawnLifecycleAndLoot.Verification.csproj --no-build --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false

该命令不代表项目存在；本轮未运行 restore、build、test、run、publish、pack、msbuild 或其他 compile-capable 命令。

## 16. 本次验证状态

    verificationStatus: not-run

本次只读审查未运行构建、测试或其他 compile-capable 命令，未生成 Build/bin、Build/obj、测试结果或源代码生成物。源码和既有材料只是 existing-evidence 输入，不是本轮独立测试结果；没有对 proposed verifier 做编译或运行验证。

## 17. 明确不拆分为一级子系统的对象

| 对象 | 不拆分理由 | 边界 |
| --- | --- | --- |
| 单个 ItemDropRule | 声明式规则实例，不拥有生命周期 | LootDecisionSystem/legacy Adapter |
| CommonDrop | 概率、数量、链式规则；不是 coordinator | ItemDropRules/LootDecision |
| 单个 NPC spawn | 一次 request/result，不是持续能力 | SpawnRequest/SpawnCommit |
| 单个金币或物品掉落 | LootResult 结果项，经济提交属于 Item | ILootCommitPort -> ItemContainerAndEconomy |
| 单个实体 slot | 可回收兼容索引 | Identity allocation mapping |
| 单个网络消息 | 传输单元，不拥有权威状态 | ReplicationAdapter/Projection |
| 单个死亡 Hook | 扩展回调/事实观察点，不是 loot coordinator | DeathResolution/公开 API Adapter |
| 单个 revenge marker | 缓存/策略记录，不拥有实体 identity | DeathPenaltyAndRevenge -> SpawnRequest |
| EquipmentLoadout | 装备容器/遍历策略 | ItemContainerAndEconomy 交界 |
| Recipe | 声明式定义/注册对象 | 配方/制作子系统 |
| ModPacket | 网络 Adapter | NetworkSessionAndSectionStreaming |
| WorldFile | 持久化 Adapter/Projection | WorldStorage |
| SimulationCommandQueue | 多领域 orchestration 设施 | 最终整合层裁决 |

不创建 Shared/Components、Common 或 Misc catch-all 目录；按真实游戏能力组织。

## 18. 兼容策略和行为保持风险

### 18.1 兼容策略

- 保留 NPC.NewNPC、Projectile.NewProjectile、ItemDropRule、Player.DropItems 作为 Adapter，先转换为 request/fact/decision，再迁移内部写入；这不是 API 兼容证明。
- 分开映射 NpcSlot/Projectile slot/item slot、NetworkId、PersistentId 和 EcsEntityId。
- 记录旧式随机 seed/roll context，使 LootDecision 可重放，不静默改变随机序列。
- CommonDrop/legacy ItemDropSolver 只产生候选结果，结果层保留 source、attribution、revision 和 idempotency key。
- Projectile.Kill 的类型副作用转为 CleanupWork，由 Projectile/WorldInteraction 消费。
- 网络和存档只使用 projection snapshot，先校验 authority、revision 和外部 ID。

### 18.2 风险表

| 风险 | 证据 | 可能偏差 | 必须验证 |
| --- | --- | --- | --- |
| NPC inactive 提前 | NPC.cs:64747-64765 | 掉落/Bestiary/派生生成顺序改变 | Dying -> Loot -> Cleanup -> inactive |
| Projectile effect 丢失 | Projectile.cs:46425-54619 | child/tile/表现效果消失或重复 | CleanupWork child result/重试 |
| loot 重复 | CommonDrop.cs:36-52、NLTX ledger | 重复 death/retry 产生多份物品 | resolution key + result idempotency |
| loot 部分提交 | Player.cs:26304-26400、DomeSimulation.cs:7062-7100 | WorldItem 与库存一成一败不一致 | transaction/compensation/retry |
| slot/identity 混用 | NPC.NewNPC、Projectile.NewProjectile | 旧引用命中新实体 | ID collision/reuse verifier |
| MessageBuffer 分支误判 | MessageBuffer.cs:1182-1307 | 把不可达代码当行为 | 裁决 1184 break |
| 完整参考回填过度 | NPC.cs:64949 与完整参考 :79599-79618 | 误报 Version4 行为 | 主基线/补证分区 |
| 版本漂移 | CoinLoss 完整参考 :190-221 | 单机/网络 marker 行为改变 | Version4 定向回归 |
| restore 隐式 spawn | WorldFile.cs:1808-1845、2951-3027 | 读档重复 loot/网络/identity | restore policy verifier |
| 写入根重复 | NpcLifecycleSystem.cs:32-50 与 DomeSimulation.cs:6843-6861 | lifecycle/command 互相覆盖 | 单写入根/revision invariant |

## 19. 未决问题、evidence-gap 和 blocking-decision

### 19.1 普通 evidence-gap

1. D:\TRbackup\NLTX\约束\公共拆分约束.md 缺失；已依据公共协议和其他已读约束继续交付。
2. RobustToolbox 核心子模块为空，缺少 SS14 引擎级实体管理、删除、复制和持久化直接证据。
3. MessageBuffer.cs:1184 的 case 23 提前 break 与后续 NPC 解析冲突；字段布局可确认，可达性为 evidence-mismatch。
4. 完整参考的 DropEoWLoot、NPC helper、OnKillNPC 只能作为 Version4 已存在成员的补证。
5. CoinLossRevengeSystem 在 Version4 与完整参考之间存在 version-drift。
6. NLTX 没有覆盖全域的 loot transaction、Projectile cleanup transaction 或 focused verifier；局部 ledger/revision 不能证明全局一致性。

### 19.2 Blocking decisions

1. 统一 SpawnCommitSystem 的 owner
   - A：由本子系统持有，所有 NPC/Projectile/WorldItem 通过统一 port。
   - B：由最终 integration layer 持有，本子系统只定义 request/fact contract。
   - 影响：A 统一幂等和 identity，但可能夺取相邻 owner；B 保持领域自治，但依赖整合层的强一致 gate。

2. EntityLifecycleState、SpawnRequest、CleanupWork、EntityReference 的 owner
   - A：SpawnLifecycleAndLoot 独占。
   - B：integration layer 作为跨 NPC/Projectile/Item 契约。
   - C：各领域特化，Adapter 转换。
   - 影响：A 简化调度，B 统一引用/顺序，C 降低共享耦合但可能重复映射。

3. Loot commit owner
   - A：本子系统拥有 LootCommitSystem/ILootCommitPort，Item/Economy 实际写入。
   - B：完全由 ItemContainerAndEconomy commit，本子系统只输出 LootDecision/LootResult。
   - C：NPC、Player death drop 各自提交。
   - 影响：A 易统一幂等，B 最符合库存权威隔离，C 的 partial commit/retry 风险最高。

4. NPC 死亡时 active=false、loot、cleanup、replication 顺序
   - A：保持 Version4 可观察顺序，先死亡/掉落，后最终 inactive，再投影。
   - B：先 Dying/inactive projection，再异步 loot/cleanup，用补偿。
   - C：loot 和 cleanup 并行，revision 合并。
   - 影响：A 行为保持成本最低；B/C 吞吐高但改变时序并增加补偿风险。

5. 网络 spawn 语义
   - A：网络输入转换为待审查 SpawnRequest。
   - B：server committed facts，client 只 projection/import。
   - C：特定兼容场景允许受限 client import，用 authority/revision 隔离。
   - 影响：A 强化合法性，B authority 最清晰，C 兼容性好但 collision/作弊面复杂。

6. persistence restore 是否重新 admission/allocation/initialization
   - A：所有 restore 重新走完整流程。
   - B：validated snapshot 进入 RestoreCommit。
   - C：按实体类型区分，town NPC restore，临时实体重新 spawn。
   - 影响：A 不变量统一但可能改 identity；B 兼容性好但需严格 snapshot；C 贴近 Version4 保存条件但规则复杂。

7. MessageBuffer.cs:1184 break 的解释
   - A：Version4 真实运行时代码，case 23 后续不可达。
   - B：删减/生成残留，真实运行时另有可达实现。
   - C：外层控制或编译处理导致静态文本不代表执行。
   - 影响：A 是网络导入缺口，B/C 需额外运行时/版本证据；裁决前不得无条件采用后续逻辑。

8. NpcLifecycleSystem.Advance 直接写 inactive 与 DespawnCommand 的关系
   - A：Advance 只写 pending termination，最终 inactive 由 commit。
   - B：保留直接写入，DespawnCommand 只作通知/投影。
   - C：NPC-specific 直接写，跨域 cleanup 由 integration command 补充。
   - 影响：A 最易形成单写入根；B/C 有双写入/顺序风险。

9. 失败掉落协议
   - A：事务 rollback WorldItem 和库存。
   - B：一侧成功保持，另一侧 compensation command。
   - C：retry ledger 暂存结果直到 economy port 确认。
   - 影响：A 一致性强但要求可回滚；B 易实现但需可观测补偿；C 适合异步但必须防重复。

## 20. Integration Handoff

    subsystemId: SpawnLifecycleAndLoot
    taskNumber: 19
    reportPath: D:\TRbackup\NLTX\docs\research\2026-09-05-version4-spawn-lifecycle-and-loot-public-decomposition.md

    evidenceStatus: partial
    nltxStatus: missing
    verificationStatus: not-run

    confirmedOwners:
    - Version4 NPC.NewNPC：遗留 NPC slot 选择、实例初始化和激活入口，不等于 proposed ECS owner
    - Version4 NPC.checkDead/NPCLoot：NPC 死亡判定、掉落调用和部分死亡后事件，顺序需保持
    - Version4 CommonDrop：声明式掉落随机尝试和概率报告，不是生命周期 owner
    - Version4 WorldFile：NPC 存档 section 的保存/加载边界
    - Version4 NetMessage/MessageBuffer：网络 payload 序列化/解析边界
    - Version4 CoinLossRevengeSystem：marker 缓存、判定和重建策略

    proposedTypes:
    - EntityLifecycleState Component, status: proposed
    - SpawnAdmissionState Component, status: proposed
    - SpawnIdentityState Component, status: proposed
    - LootResolutionState Component, status: proposed
    - ReplicationProjectionState Component, status: proposed
    - EntityReference value object, status: proposed
    - SpawnRequest Command, status: proposed
    - SpawnCommit Command/Event, status: proposed
    - DeathResolutionEvent Event, status: proposed
    - LootDecision value object/Command, status: proposed
    - LootResult value object/Command, status: proposed
    - CleanupWork Command, status: proposed
    - SpawnAdmissionSystem, status: proposed
    - SpawnIdentityAllocationSystem, status: proposed
    - SpawnInitializationSystem, status: proposed
    - SpawnCommitSystem, status: proposed
    - SpawnActivationSystem, status: proposed
    - DeathResolutionSystem, status: proposed
    - LootDecisionSystem, status: proposed
    - LootCommitSystem, status: proposed
    - DespawnSystem, status: proposed
    - CleanupWorkSystem, status: proposed
    - ILootCommitPort, status: proposed
    - ICleanupEffectPort, status: proposed
    - ReplicationAdapter, status: proposed
    - PersistenceAdapter, status: proposed

    sharedTypesForIntegrationReview:
    - EntityLifecycleState、EntityReference、EcsEntityId、NetworkId、PersistentId、CompatibilitySlot；crossSubsystemOwner: integration-review
    - SpawnRequest、SpawnCommit、DeathResolutionEvent、CleanupWork；crossSubsystemOwner: integration-review
    - LootResult、ItemInstanceId、WorldSectionId、snapshot 值对象；crossSubsystemOwner: integration-review
    - ReplicationProjectionState；crossSubsystemOwner: integration-review

    crossSubsystemReaders:
    - NpcAndTownSimulation 读取 NPC 生命周期、死亡事实、spawn 结果和城镇提交
    - ProjectileSimulation 读取 Projectile lifecycle、CleanupWork 和 child spawn 结果
    - ItemContainerAndEconomy 读取 LootResult、Player death drop 和 WorldItem commit
    - DeathPenaltyAndRevenge 读取 death attribution 并触发 revenge SpawnRequest
    - NetworkSessionAndSectionStreaming 读取 committed revision 和 replication projection
    - WorldStorage 读取 committed snapshot 和持久化资格

    crossSubsystemWriters:
    - NpcAndTownSimulation、ProjectileSimulation、Player/Item 领域产生 spawn、death 和 cleanup facts
    - DeathPenaltyAndRevenge 产生复仇重建 request
    - Network Adapter 产生待验证网络输入或 client projection
    - WorldStorage Adapter 产生 restore snapshot
    - ItemContainerAndEconomy 提交 item/gold/container 结果

    orderingConstraints:
    - Admission 先于 IdentityAllocation；未接纳 request 不分配身份
    - Allocation/Initialization 先于 SpawnCommit；SpawnCommit 先于 Activation
    - DeathResolution 先于 LootDecision；LootDecision 不直接改经济或生命周期
    - LootCommit、CleanupWork 和最终 inactive 的严格顺序由 integration review 裁决，默认保持 Version4 可观察顺序
    - Projectile-specific cleanup effects 有明确消费顺序；child spawn 不递归修改当前集合
    - Replication/Persistence 只读已提交 revision/snapshot

    boundaryChallenges:
    - NPC、Projectile、Item 均有局部生成/销毁路径，需要决定统一 commit gate 与领域 commit 的关系
    - DomeSimulation 和 SimulationCommandQueue 是跨域 orchestration 聚合点，不应直接升级为本子系统 owner
    - ItemDropRule、CommonDrop、EquipmentLoadout、revenge marker、WorldFile、ModPacket 和网络消息保留为规则/容器/缓存/Adapter/Projection
    - 不创建 generic Shared/Components、Common 或 Misc 目录

    evidenceGaps:
    - 缺失 D:\TRbackup\NLTX\约束\公共拆分约束.md
    - RobustToolbox 子模块为空，缺少 SS14 引擎级直接证据
    - MessageBuffer.cs:1184 case 23 提前 break，状态 evidence-mismatch
    - 完整参考 DropEoWLoot、NPCLoot helper、OnKillNPC 只能补证/version-drift
    - CoinLossRevengeSystem 存在 Version4/完整参考 version-drift
    - 当前 NLTX 没有全域 loot transaction、cleanup transaction 或 focused verifier

    blockingDecisions:
    - SpawnCommitSystem 由本子系统还是最终 integration layer 持有
    - EntityLifecycleState、SpawnRequest、CleanupWork、EntityReference 和多种 ID 的共享 owner
    - LootCommitSystem/ILootCommitPort 由本子系统持有还是完全交给 ItemContainerAndEconomy
    - NPC/Projectile inactive、loot、cleanup、replication 的严格顺序
    - network spawn 是权威 SpawnRequest、server projection 还是受限 client import
    - persistence restore 是重新 admission 还是 validated snapshot restore commit
    - MessageBuffer.cs:1184 break 的来源和可达性
    - NpcLifecycleSystem.Advance 直接 inactive 与 DespawnCommand 的关系
    - failed loot commit 采用 rollback、compensation command 还是 retry ledger

    notImplemented:
    - 当前 NLTX 未确认存在统一跨 NPC/Projectile/Item 的 SpawnAdmission、IdentityAllocation、SpawnCommit、Despawn/Cleanup、LootCommit owner
    - 当前 NLTX 未确认存在跨来源 request/death/loot 全局幂等协议
    - 当前 NLTX 未确认存在 network/persistence identity 与 ECS identity 的完整映射和 restore policy
    - 本报告所有 proposed 类型、路径、接口和 verifier 均未创建

    verifierPlan:
    - proposed SpawnLifecycleAndLoot.Verification 覆盖自然/事件/复仇/网络 spawn、NPC death loot、Projectile cleanup、Player death drop、重复请求/事件、ID 回收/冲突、partial commit、client projection、persistence handoff、retry/timeout、child spawn 和 death-loot-cleanup 顺序
    - 使用 deterministic RandomContext、request/event/result key、revision 和 cleanup work 验证幂等及可重放
    - 本轮 verificationStatus: not-run；未来编译型命令必须经 Build/Tools/Invoke-SerialDotnet.ps1 串行执行

## 21. 最终声明

本报告是基于 Version4、完整参考源码、tModLoader 公开文档和有限 ECS 结构参考形成的子系统边界与 ECS 拆分设计。它不是迁移完成报告，不是行为等价证明，不是 API 兼容证明，也不是当前 NLTX 已实现能力的声明。

本轮仅生成本文件；未启动子代理，未修改生产代码、测试代码、既有研究文件或参考源码，未运行构建/测试及其他 compile-capable 命令。所有未决跨子系统 owner、接口和最终调度顺序留给 Integration Review。

