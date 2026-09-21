# 01 — LiquidSimulation 子系统审查与 ECS 拆分设计

> subsystemId: LiquidSimulation  
> taskNumber: 01  
> evidenceStatus: partial  
> nltxStatus: partial  
> verificationStatus: not-run

## 1. 执行摘要

Version4 的液体不是 SpatialSimulation 的附带碰撞分支。Terraria.Tile 持有液体数量、液体类型和一次跳过更新标志；Terraria.Liquid 持有独立的工作集、预算、panic/settle 状态、液体反应和网络变更集合；WorldGen、WorldFile 和 NetMessage 分别在生成、加载/恢复和复制边界驱动或消费它。因此 LiquidSimulation 满足独立生命周期、受控提交和跨域边界三个条件，可以从 SpatialSimulation 反向拆出。

主要结论：

- Version4 的权威 Tile 液体事实是 Tile.liquid 与 liquidType() 背后的类型位；checkingLiquid 和 skipLiquid 是工作集去重/调度辅助状态，不是实体系统共同拥有的液体业务状态。
- Version4 的液体写入并非单一入口：Liquid.Update、Liquid.LiquidCheck、Liquid.DelWater、WorldGen 生成/结构路径和 WorldFile 读取都会直接修改 Tile。因此“最终写者已唯一收口”是 partial，也是 proposed LiquidCommitSystem 的关键整合决策。
- 当前 Dome 工作树不是旧任务包所说的 missing，而是 version-drift：已有有界队列、传播、合并、Retry、Panic、转移、Tile 提交、section revision、网络快照和 loopback verifier 源码。但它只覆盖选定 Liquid 行为切片，不等价于完整 Version4 Liquid.cs。
- 建议边界为：Tile/WorldGrid 中的 LiquidState、世界级 LiquidWorkQueue、纯 LiquidReactionQuery、LiquidPropagationSystem、LiquidCommitSystem、接触 LiquidContactQuery、持久化/网络 Adapter 和客户端 Renderer Projection。
- 本轮没有运行构建、测试或任何 compile-capable 命令；所有未来 verifier 均为 status: proposed。历史研究材料只能标记 existing-evidence。

## 2. 子系统范围和不负责的内容

负责：液体数量/类型状态转换；来源进入工作集、去重、有限预算、重排和 Retry；水、熔岩、蜂蜜、微光反应资格；液体对 Tile 的结构性影响请求；原子提交、section dirty/revision、网络快照和存档快照输入；向 Player、NPC、Projectile 提供只读接触查询。

不负责：

- 实体移动、碰撞几何和受伤结算；交给 SpatialSimulation、PlayerGameplay、NpcAndTownSimulation、CombatAndStatus。
- TileEntity、Chest、线路、结构放置和一般 WorldGen.KillTile 事务；液体只提交明确的 Tile mutation command。
- 世界生成 pass、Hardmode 长事务和生态传播；由 WorldGenerationAndEcology/WorldProgressionAndTransition 负责。
- 网络连接、section 可见性和客户端渲染循环；液体只输出不可变变更快照。
- LiquidRenderer、单个碰撞查询、单个液体类型和单个 NetModule；它们分别是 Projection、Query、规则数据和 Adapter。

## 3. 证据优先级和来源角色

证据优先级为 Version4 → 完整可编译参考源码 → tModLoader API → Space Station 14 ECS 参考。Version4 决定真实覆盖和调用链；完整参考只补足 Version4 已存在文件的明确删减；tModLoader 只交叉验证公开边界；Space Station 14 只用于结构粒度。

| 来源 | 路径/版本 | 用途 | 状态 |
| --- | --- | --- | --- |
| Version4 | D:\TRbackup\Version4\Terraria\Liquid.cs、LiquidBuffer.cs、Tile.cs、WorldGen.cs、NetMessage.cs、Collision.cs、Player.cs、NPC.cs、Projectile.cs、Terraria.IO\WorldFile.cs | 液体状态、工作集、调用者、写者、网络/存档/接触边界 | confirmed / partial |
| 完整参考 | D:\TRbackup\无任何删减通过编译\Terraria\Liquid.cs | 补证 UndergroundDesertCheck、LiquidOverwriteStrip、CreateLiquidMergeTile | full-reference-supplemented |
| tModLoader | D:\TRbackup\tmodloader-api-docs-stable，v2026.07 | Tile 字段和 ModSystem 生命周期 | confirmed，仅公开 API |
| Space Station 14 | C:\Users\shan\Downloads\ECS\space-station-14-master 的 Fluids 文件 | 组件、System/Query、状态/表现分离 | 结构参考 |

任务包把加载文件写成 D:\TRbackup\Version4\Terraria\WorldFile.cs，实际文件是 D:\TRbackup\Version4\Terraria.IO\WorldFile.cs。这是 evidence-mismatch；本报告以实际路径为准。

## 4. Version4 真实代码证据

| 事实 | 实际路径与行号 | 读者/写者/生命周期 | 副作用 | evidenceStatus |
| --- | --- | --- | --- | --- |
| Liquid 的预算、工作计数、panic、quick 状态和网络集合 | D:\TRbackup\Version4\Terraria\Liquid.cs:14-54 | UpdateLiquid、AddWater、DelWater、WorldGen/WorldFile | 全局可变状态、网络 pending set | confirmed |
| ReInit 重置运行态并按 reduced-max 设置 curMaxLiquid | 同上:88-109 | WorldGen:10268；世界初始化 | 清空预算、panic、quick 状态 | confirmed |
| QuickWater 遍历 Tile，执行 SettleWaterAt 并恢复生成 solidity | 同上:111-158 | WorldGen、WorldFile:752 | Tile、solidity、进度文本、生成 cleanup | partial |
| 工作项 Update 读取四邻域并分派水/熔岩/蜂蜜/微光 | 同上:470-617、716-986 | UpdateLiquid 消费 | 直接写 Tile、邻居入队、反应 | confirmed |
| StartPanic 和恢复逐行 QuickWater，恢复时清 RemoteClient section | 同上:996-1013、1056-1083 | UpdateLiquid | 控制台输出、section 失效 | confirmed |
| UpdateLiquid 分片消费、skip、DelWater、Buffer 回灌、stuck 和 NetLiquid 广播 | 同上:1015-1188 | WorldGen:15306、20118、59433；WorldFile:775 | Tile、队列、网络、副作用 | confirmed |
| AddWater 检查边界、去重、容量和 TileObjectData 液体死亡 | 同上:1190-1234 | Update、WorldGen、加载恢复 | checking/skip 位、KillTile、发包 | confirmed |
| LiquidCheck 聚合异种液体并选择合并结果 | 同上:1238-1321 | LavaCheck/HoneyCheck/ShimmerCheck、WorldGen | 多 Tile 清空、KillTile、TileSquare | partial |
| Version4 当前文件的 UndergroundDesertCheck 与 CreateLiquidMergeTile 为空体 | 同上:1235-1237、1322-1323 | LavaCheck/LiquidCheck 调用 | Version4 行为缺口 | confirmed |
| GetLiquidMergeTypes 输出四液体合并规则 | 同上:1324-1394 | LiquidCheck、WorldGen:4514 | 纯规则输出 | confirmed |
| DelWater 删除工作项、重新入队邻居并触发反应/网络标记 | 同上:1427-1524 | UpdateLiquid 清理 | 邻居、Tile、网络 | confirmed |
| LiquidBuffer 固定容量、checkingLiquid 去重，DelBuffer 用末项替换 | LiquidBuffer.cs:3-31 | AddWater 满载，UpdateLiquid 消费 | 有界延后、去重 | confirmed |
| Tile.liquid 为 byte，四类型常量为 0/1/2/3 | Tile.cs:6-24、56-62 | Liquid、WorldGen、WorldFile、NetMessage、Collision | 位打包、存档/网络 | confirmed |
| anyWater/anyLava/anyHoney/anyShimmer 仅在 liquid > 0 时有效；checking/skip 是 header 位 | Tile.cs:398-447、512-550 | Liquid 规则和工作集 | 读写 Tile header | confirmed |
| 生成期和普通世界 Tick 都驱动 QuickWater/UpdateLiquid；初始化清空 Buffer | WorldGen.cs:6586、6660、10268、15306、20118、59433、67234-67328 | WorldGen/World Tick | 生成写集和时序 | confirmed |
| 加载后 QuickWater、WaterCheck、quickSettle、循环 UpdateLiquid，再关闭 quickSettle | Terraria.IO\WorldFile.cs:750-780 | WorldFile load | 加载屏障、Tile 重建 | confirmed |
| WorldFile 编解码液体数量和类型 | Terraria.IO\WorldFile.cs:1533-1545、2648-2660、3847-3855 | 持久化读写 | 二进制兼容 | confirmed |
| 区段、单 Tile 和 Tile snapshot 编码液体 | NetMessage.cs:575-633、981-988、2104-2117、2367-2389 | NetMessage/客户端 | wire bytes、section 广播 | confirmed |
| Collision/WetCollision、LavaCollision 被实体读取 | Collision.cs:1001-1110；Player.cs:14144、17101-17370；NPC.cs:67081、79090-79316；Projectile.cs:10282-10288 | 实体 Tick 只读环境，实体写自身 wet 状态 | 移动、伤害和表现派生 | confirmed |

真实方向是：

WorldGen/WorldFile load → Liquid.ReInit/QuickWater/AddWater → Liquid.UpdateLiquid → Liquid.Update/LiquidCheck/DelWater → Tile/structure change → NetSendLiquid/NetMessage；Player/NPC/Projectile/Collision 只读稳定 Tile snapshot，再产生实体本地接触状态。

Version4 在生成、加载和普通 Tick 复用 UpdateLiquid，但入口约束不同。加载打开 quickSettle，生成还会改变 solidity 和调用 cleanup，不能把它们合并为无条件同一 ECS 阶段。

## 5. 完整参考源码补证

| 补证 | 完整参考路径/行号 | Version4 对照 | 状态 |
| --- | --- | --- | --- |
| UndergroundDesertCheck | D:\TRbackup\无任何删减通过编译\Terraria\Liquid.cs:1245-1262；Version4:1235-1237 空体 | 补足墙体扫描，但不能写成 Version4 当前行为 | full-reference-supplemented |
| LiquidOverwriteStrip | 同上:1353-1368；Version4 无成员 | 完整参考独有，不能假定 Version4 已有 | missing / full-reference-supplemented |
| CreateLiquidMergeTile | 同上:1369-1420；Version4:1322-1323 空体 | 补证合并 Tile 替换、声音、frame、网络和生成分支 | full-reference-supplemented |
| GetLiquidMergeTypes/DelWater 邻域 | 完整参考:1263-1524 | Version4 同名成员存在 | 只能解释删减影响 | partial |

完整参考不能覆盖 Version4 空体事实；否则会把未确认的 Tile replacement、声音、frame 和网络行为误报为已存在行为。

## 6. tModLoader 公开 API 交叉验证

文档首页 D:\TRbackup\tmodloader-api-docs-stable\index.html:29-31 为 tModLoader v2026.07。

| 页面 | 成员/锚点 | 语义 | 用途 |
| --- | --- | --- | --- |
| struct_tile.html:250-254 | LiquidAmount，a420edd17ed083bfa38db1bfb5a8f5dc7 | byte，0 到 255，位置的液体数量 | 交叉验证 Tile.liquid |
| struct_tile.html:256-260 | LiquidType，a75ef7e5fcc77b3987680367b98d92616 | 只有 LiquidAmount > 0 时有意义；对应 liquidType() | 交叉验证数量/类型一致性 |
| struct_tile.html:272-275 | SkipLiquid，a53d034267b724dc7073497163cfeb7a1 | 跳过一个 Tick 更新 | 交叉验证为调度辅助 |
| class_mod_system.html:257-288 | PostUpdateWorld、PostWorldLoad、PreUpdateEntities | 世界更新、加载完成、实体更新前边界 | 交叉验证调度插槽 |
| class_mod_system.html:131-133、334-336、602-614、1077-1087 | LoadWorldData、SaveWorldData | 世界数据存取钩子 | 交叉验证 Persistence Adapter 接缝 |
| class_mod_system.html:166-168、838-850 | NetReceive | WorldData 成功接收后客户端调用 | 交叉验证网络接收与权威写隔离 |

这些公开页不能证明 Version4 私有队列、Panic、TileObjectData、反应算法或网络包布局。

## 7. Space Station 14 参考

实际读取：

- C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\Fluids\Components\PuddleComponent.cs：液体实体状态组件。
- ...\Content.Shared\Fluids\SharedPuddleSystem.cs：System、查询和受控写入。
- ...\Content.Server\Fluids\EntitySystems\PuddleSystem.cs：服务端液体行为边界。
- ...\Content.Shared\Fluids\SharedPuddleSystem.Spillable.cs：输入/状态变化分离。
- ...\Content.IntegrationTests\Tests\Fluids\PuddleTest.cs：生成、空间拒绝和边界测试。

Space Station 14 无直接对应 Terraria Tile 液体求解器证据；以下边界仅由 Version4 真实代码和 NLTX 项目约束决定。禁止复制其代码、命名、目录结构或领域语义。

## 8. 成员、读写者、生命周期和所有权

| 成员组 | 归类 | 权威/派生 | 主要读者 | 主要写者 |
| --- | --- | --- | --- | --- |
| Tile.liquid + type bits | Tile 液体状态 | 权威世界事实 | Liquid、WorldGen、WorldFile、NetMessage、Collision | Liquid、WorldGen、WorldFile、Tile helpers |
| checkingLiquid | 工作去重 | 调度缓存 | AddWater、Buffer、UpdateLiquid | Add/Del/Buffer |
| skipLiquid | 一 Tick skip | 调度状态 | UpdateLiquid | Update/消费 |
| Main.liquid/numLiquid/curMaxLiquid | 工作集 | 工作状态 | Update/Add/Del | ReInit/Add/Del/Update |
| LiquidBuffer | 溢出队列 | 有界缓存 | Add/Update/WorldFile | AddBuffer/DelBuffer |
| quickFall/quickSettle/wetCounter | 调度策略 | 行为状态/缓存 | UpdateLiquid、WorldFile | ReInit/WorldFile/Update |
| panic/stuck 字段 | 压力恢复 | 权威运行策略 | UpdateLiquid/StartPanic | ReInit/Update/StartPanic |
| LiquidCheck/GetLiquidMergeTypes | 反应行为/规则 | Query + command 生成 | Update、WorldGen | LiquidCheck 仍直接写 Tile |
| _netChangeSet | 发布缓存 | pending set | NetSendLiquid/UpdateLiquid | NetSend/Swap/Clear |
| Collision wet methods | 接触 Query | 派生纯读 | Player/NPC/Projectile | 实体写自身接触 |
| LiquidRenderer | 客户端表现 | Projection | ClientPresentationAndTools | 客户端渲染循环 |

Tile/WorldSection/LiquidType/NetworkId/EntityReference/PersistentEntityId 是跨子系统候选，最终 owner 一律 integration-review。

## 9. Proposed ECS 设计

以下全部是 status: proposed，不是当前已存在实现。

| proposed 类型/路径 | 最小职责 | 不包含 |
| --- | --- | --- |
| LiquidStateComponent，dome/src/Terraria.Dome.Simulation/Liquid/Components/LiquidStateComponent.cs | coordinate、Amount、LiquidTypeId、revision | queue、网络 DTO、实体 wet |
| LiquidWorldStateComponent，.../Liquid/Components/LiquidWorldStateComponent.cs | tick budget、maximum queue、panic/recovery policy | 单 Tile 状态 |
| LiquidWorkQueueComponent，.../Liquid/Components/LiquidWorkQueueComponent.cs | bounded nodes、dedup、sequence、retry、drain order | Tile 权威事实 |
| LiquidReactionStateComponent，.../Liquid/Components/LiquidReactionStateComponent.cs | 必要时跨 Tick 的 reaction token | 规则表、网络状态 |
| LiquidContactComponent，.../Liquid/Components/LiquidContactComponent.cs | 实体当前 wet/lava/honey/shimmer 接触 | 世界 Tile 写集 |
| LiquidDirtySectionComponent，.../Liquid/Components/LiquidDirtySectionComponent.cs | section 到 latest revision | wire packet/visibility 权限 |
| LiquidInputSystem，status: proposed | 来源校验和 queue admission | Tile 写 |
| LiquidPropagationSystem，status: proposed | read view + queue + budget → LiquidChangeCommand[] | 权威写 |
| LiquidReactionQuery，status: proposed | 邻域 snapshot → reaction decision | I/O/写状态 |
| LiquidReactionSystem，status: proposed | decision → consumed changes + structure commands | 网络发送 |
| LiquidSettleSystem，status: proposed | blocked source → bounded retry | 无限重排 |
| LiquidCommitSystem，status: proposed | validated batch → WorldGrid/section revision | 规则计算 |
| LiquidContactQuery，status: proposed | bounds + immutable Tile view → contact result | 写实体或液体 |
| LiquidTickCoordinator，status: proposed | phase/load mode → 系统顺序 | 拥有液体事实 |
| LiquidChangeCommand，status: proposed | sequence、coordinate、amount、type、source、expected revision | BinaryWriter |
| LiquidStructureMutationCommand，status: proposed | Kill/merge/frame 等结构请求 | 直接 KillTile |
| LiquidPersistenceAdapter，status: proposed | committed snapshot ↔ Version4 bytes | 修改运行态 |
| LiquidReplicationProjection，status: proposed | committed changes + visibility → immutable snapshot | 反写模拟 |
| LiquidRendererProjection，status: proposed | client snapshot → visual state | 权威数量/类型 |
| ILiquidWorldCommitPort，status: proposed | validated liquid/structure batch → atomic result | 规则算法 |

Interface/Implementation/Seam/Depth/Leverage/Locality：传播使用 proposed ILiquidPropagationPolicy；实现为确定性邻域搜索；seam 是 fake WorldGrid read view；depth 中等、leverage 高、locality 为 Liquid domain。Reaction 使用 proposed ILiquidReactionQuery；seam 是 fixed neighbor snapshot；depth 深、leverage 高、locality 是 Liquid/WorldInteraction seam。Commit 使用 proposed ILiquidWorldCommitPort；seam 是 all-or-nothing batch result；depth 深、leverage 极高、locality 是 WorldStorage integration seam。Contact 使用 proposed ILiquidContactQuery；seam 是 immutable spatial read view；depth 中等、leverage 高。Replication/Persistence 使用 proposed Adapter/Projection；seam 是 visibility policy、in-memory stream 和 golden bytes。

## 10. 调用方向和显式 System 顺序

普通服务器 Tick：

WorldSession phase snapshot → LiquidInputSystem → pressure/budget → LiquidPropagationSystem → LiquidReactionSystem → LiquidSettleSystem → LiquidCommitSystem → section revision/dirty → LiquidReplicationProjection/Persistence snapshot → Player/NPC/Projectile LiquidContactQuery。

加载/生成：

WorldStorage/WorldGeneration → RebuildInputAdapter → QuickWater/Rebuild budget → Propagation/Reaction → explicit commit barriers → WorldReady transition。

约束：

- Contact Query 只读稳定 snapshot，不能观察半提交 Tile。
- Commit 先于网络和存档 projection。
- 结构 Tile mutation 与液体消费必须在同一事务 batch，或返回明确补偿结果。
- QuickWater、solidity override、LiquidInteractionsCleanup 和普通 Tick 必须是显式模式。
- Panic section invalidation 必须经 network/section adapter。

## 11. 持久化、网络和客户端投影

Version4 WorldFile 在 Terraria.IO\WorldFile.cs:1533-1545 编码液体数量，:2648-2660、:3847-3855 读取数量和类型位。proposed LiquidPersistenceAdapter 应接受 committed immutable snapshot，保留零液体省略、类型编码、版本条件和失败不污染运行态的语义；加载完成前通过 rebuild adapter 触发 quick settle。

NetMessage.cs:575-633 发送 Tile 区段液体，:981-988 发送单 Tile，:2104-2117 编码 snapshot，:2367-2389 为 TileSquare 入口。proposed LiquidReplicationProjection 只从 committed revision 生成快照，按 section visibility 过滤，保留 wire order；入站只产生经过权限、坐标、类型、sequence 检查的 command。

Renderer 只消费客户端快照。任务包指定的 LiquidRenderer 路径在当前 Version4 目录中未直接找到同名文件，因此这里只提出 Projection，不引用未核实成员。

## 12. 当前 NLTX 映射

旧任务包把 LiquidSimulation 标为 missing，但当前工作树存在液体切片，故 version-drift 后为 nltxStatus: partial。

| 当前路径 | 事实 | 状态 |
| --- | --- | --- |
| dome/src/Terraria.Dome.Simulation/Liquid/Components/LiquidWorldStateComponent.cs:5-91 | budget、Panic Normal/Recovery | partial |
| .../LiquidUpdateQueueComponent.cs:6-146 | 去重、sequence、有限容量、Retry、稳定排序 | partial |
| .../Liquid/Systems/LiquidInputSystem.cs:8-27 | 坐标/数量/类型/sequence/容量校验 | partial |
| .../Liquid/Systems/LiquidPropagationSystem.cs:13-300 | 有预算传播、固体/platform 条件、TileObject rule command、requeue | partial |
| .../Liquid/Systems/LiquidMergeSystem.cs、Definitions/LiquidRuleRegistry.cs、LiquidInteractionClassifier.cs | 合并资格、四类型规则、分类 | partial |
| .../Liquid/Systems/LiquidCommitSystem.cs:9-61 与 World/WorldGrid.cs:116-187 | 命令校验、排序、Tile 写、section revision/dirty | partial |
| .../Liquid/Systems/LiquidTransferSystem.cs | 泵/跨域转移的双 Tile change | partial |
| .../Liquid/Systems/LiquidReplicationSystem.cs、Snapshots/LiquidReplicationSnapshot.cs | 最新命令选择和不可变复制快照 | partial |
| .../Liquid/Components/LiquidContactComponent.cs:5-59、src/Share/Entity/Components/LiquidComponent.cs:3-34 | 实体接触状态 | partial，不是 Tile solver |
| dome/Test/Terraria.Dome.Liquid.Verification/Program.cs、Liquid.Loopback.Verification/Program.cs | focused verifier 源码和 loopback 场景 | existing-evidence，本轮未运行 |

Dome WorldTile.cs:3-27 保存 LiquidAmount/LiquidType；WorldGrid.TrySetLiquid:116-137 推进 section version；CommitLiquidChanges:150-187 排序和提交。它是当前模型的 WorldGrid 事实载体，根 src 的 LiquidComponent 是实体接触状态，不应合并为巨型组件。

## 13. focused verifier 设计

所有下列 verifier 为 status: proposed，本轮不创建、不运行：

| ID | 断言 |
| --- | --- |
| LS-TILE-STATE | amount/type 0/255 边界和无液体类型语义 |
| LS-QUEUE-BOUNDS | 固定容量、去重、sequence 排序和满载延后 |
| LS-PROPAGATION | 固体/platform/inactive/边界/不同类型邻居 |
| LS-RETRY | 只有成功 requeue 才增加 Retry，达到上限后稳定 |
| LS-MERGE-MATRIX | 四类型 12 条规则、消费数量和结果 Tile |
| LS-COMMIT-ATOMICITY | 非法 sequence/坐标/类型/version 整批拒绝 |
| LS-TILE-SIDE-EFFECT | KillTile、frame、merge Tile 失败不遗留已消费液体 |
| LS-PANIC | high-water、恢复阈值、队列不静默丢失 |
| LS-LOAD-REBUILD | QuickWater/quickSettle/WaterCheck/ready barrier 顺序 |
| LS-REPLICATION | committed revision、visibility、golden bytes、出站只读 |
| LS-CONTACT-READ | Query 只读 WorldGrid，实体接触不反写 |
| LS-FULLREF-GAPS | Version4 空体和完整参考补证不混淆 |

建议命令形状（status: proposed，未执行）：

    pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 test .\dome\Test\Terraria.Dome.Liquid.Verification\Terraria.Dome.Liquid.Verification.csproj --no-build --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false

本轮没有运行任何 restore/build/test/run/publish/pack/msbuild/watch。

## 14. 不拆分项

- LiquidBuffer：容量受限的待处理坐标，生命周期依附 solver；是 proposed LiquidWorkQueueComponent。
- LiquidRenderer：消费客户端快照的 proposed LiquidRendererProjection。
- 单个液体类型：值域成员和规则输入，是 proposed LiquidTypeId/registry。
- 单个反应：规则条目，归 proposed LiquidReactionQuery/registry。
- 单个 Tile：网格状态实例，归 WorldGrid/Tile state。
- 单个 NetLiquidModule：wire Adapter/Projection，不能拥有模拟状态。
- 单个碰撞查询：纯 proposed LiquidContactQuery。
- 单个渲染快照：不可变表现输入，不是权威状态。

## 15. 兼容策略和行为保持风险

兼容策略：先锁定 queue/reaction/commit verifier，再迁移一个写集；保留数量/类型 0/255、四类型编码、SkipLiquid 一 Tick、section revision 和 wire order；TileCoordinate、WorldSectionId、LiquidTypeId、NetworkId、EntityReference 分开建模并标记 integration-review；普通 Tick、生成期、加载期和 Panic recovery 使用显式 mode；网络、存档、声音、TileEntity/结构副作用和 UI 经过 Adapter/Port/Projection。

主要风险：

- RISK-LS-MULTI-WRITER：多处直接写 Tile，导致状态漂移。
- RISK-LS-REACTION-ATOMICITY：先清空液体、后创建合并 Tile 失败，造成液体丢失。
- RISK-LS-FULLREF-DRIFT：完整参考补实现被误报为 Version4 当前行为。
- RISK-LS-LOAD-ORDER：普通传播替代加载 QuickWater，过早释放 WorldReady。
- RISK-LS-PANIC：把 Panic 当普通队列清空，丢失 section reset。
- RISK-LS-CONTACT-WRITE：实体接触反向成为液体写者。
- RISK-LS-NET-REENTRY：网络适配器直接写 Tile。
- RISK-LS-DOME-SCOPE：Dome 切片被误判为完整 Liquid.cs。

## 16. 未决问题、evidence-gap 和 blocking-decision

普通缺口：

- GAP-LS-TILE-WRITE-ROOT：Version4 生成、结构、加载和运行时多写者未闭合为单一 commit owner，partial。
- GAP-LS-MERGE-COMPLETE：CreateLiquidMergeTile、UndergroundDesertCheck 存在 Version4/完整参考差异，version-drift。
- GAP-LS-QUICKWATER：完整 QuickWater、solidity override、生成 cleanup 未全部迁移，partial。
- GAP-LS-TILEOBJECT：TileObjectData、多 Tile、危险 Tile 和 KillTile 副作用未完整迁移，partial。
- GAP-LS-RENDERER-PATH：LiquidRenderer 实际路径/成员未闭合，missing。
- GAP-LS-ROOT-DOME-CLOSURE：根 src 与 dome 装配未本轮编译确认，unresolved。

blocking-decision：

- BD-LS-01 Tile 液体最终写入 owner：A WorldStorage/WorldGrid 统一 commit；B Liquid 拥有液体字段、结构系统拥有 Tile type；C integration-level WorldTileCommitPort。三者会改变提交边界。
- BD-LS-02 完整参考补证基线：A 严格保持 Version4 空体；B 以完整参考为迁移目标；C 只纳入独立确认的规则部分。会改变 reaction、WorldGen 和网络行为。
- BD-LS-03 接触读取时点：A 上一 committed snapshot；B 同 Tick commit 后；C 双 revision snapshot。会改变 Spatial 与 Liquid 的顺序。

## 17. Integration Handoff

    subsystemId: LiquidSimulation
    taskNumber: 01
    reportPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-05-version4-liquid-simulation-public-decomposition.md

    evidenceStatus: partial
    nltxStatus: partial
    verificationStatus: not-run

    confirmedOwners:
    - Tile.liquid/liquidType 位是 Version4 数量/类型存储载体。
    - Liquid.UpdateLiquid 是普通 Tick、生成和加载恢复共同进入的协调入口。
    - LiquidBuffer 是有界工作集，不是液体世界事实。
    - Collision 接触方法和实体 wet 字段是派生读取边界。
    - NetMessage 与 WorldFile 是编码边界，不是纯规则 owner。

    proposedTypes:
    - proposed LiquidStateComponent, status: proposed; Tile/WorldGrid owner integration-review
    - proposed LiquidWorldStateComponent, LiquidWorkQueueComponent, LiquidContactComponent, LiquidDirtySectionComponent, status: proposed
    - proposed LiquidInputSystem, LiquidPropagationSystem, LiquidReactionSystem, LiquidSettleSystem, LiquidCommitSystem, status: proposed
    - proposed LiquidReactionQuery, LiquidContactQuery, status: proposed
    - proposed LiquidChangeCommand, LiquidStructureMutationCommand, status: proposed
    - proposed LiquidPersistenceAdapter, LiquidReplicationProjection, LiquidRendererProjection, status: proposed

    sharedTypesForIntegrationReview:
    - TileCoordinate/WorldTileCoordinate、WorldSectionId、LiquidTypeId、NetworkId、EntityReference、PersistentEntityId。
    - ILiquidWorldCommitPort/WorldTileCommitBatch；consumers 为 Liquid、WorldStorage、WorldInteraction、WorldGeneration、Network、Persistence。

    crossSubsystemReaders:
    - SpatialSimulation、PlayerGameplay、NpcAndTownSimulation、ProjectileSimulation 读取稳定 Tile/contact view。
    - NetworkSessionAndSectionStreaming、PersistenceAndRecovery、ClientPresentationAndTools 消费 committed snapshot。

    crossSubsystemWriters:
    - WorldGenerationAndEcology、WorldStorage/PersistenceAndRecovery、WorldInteractionAndStructures 可能产生输入或结构 mutation。
    - Network adapter 只能转换为 validated command，不直接写 Tile。

    orderingConstraints:
    - Input → pressure/budget → propagation → reaction → settle → commit → revision → network/persistence projection。
    - Load/generation rebuild 必须在 WorldReady barrier 释放前完成。
    - Contact Query 不得观察半提交状态，也不得写回液体。
    - 结构副作用与液体消费需同一事务或明确补偿。

    boundaryChallenges:
    - 任务包 WorldFile 路径为 evidence-mismatch；实际为 Terraria.IO\WorldFile.cs。
    - 任务包 missing 与当前 Dome 液体切片发生 version-drift，当前应为 partial。
    - 根 src LiquidComponent 是接触状态，不应吞并 LiquidSimulation。
    - 完整参考空体补证不能静默成为 Version4 主基线。

    evidenceGaps:
    - 多写者 Tile 写集、完整 QuickWater/生成 cleanup、TileObjectData、多 Tile、危险 Tile、Renderer 路径和根/dome 装配尚未闭合。

    blockingDecisions:
    - BD-LS-01、BD-LS-02、BD-LS-03。

    notImplemented:
    - 本报告未创建或修改 Component、System、Query、Command、Adapter、Projection、测试或项目文件。
    - 不声明完整 Liquid.cs 已迁移、行为等价或 API 兼容。

    verifierPlan:
    - LS-TILE-STATE、LS-QUEUE-BOUNDS、LS-PROPAGATION、LS-RETRY、LS-MERGE-MATRIX。
    - LS-COMMIT-ATOMICITY、LS-TILE-SIDE-EFFECT、LS-PANIC、LS-LOAD-REBUILD、LS-REPLICATION、LS-CONTACT-READ、LS-FULLREF-GAPS。
    - 全部 status: proposed；本轮 verificationStatus: not-run。

## 18. 最终声明

本报告是基于 Version4、完整参考源码、tModLoader 公开文档和有限 ECS 结构参考形成的子系统边界与 ECS 拆分设计。它不是迁移完成报告，不是行为等价证明，不是 API 兼容证明，也不是当前 NLTX 已实现能力的声明。

本报告只写入任务指定的唯一报告文件；未修改 src、Test、dome/src、Version4、完整参考源码、tModLoader 文档或其他共享审查材料。本轮未运行构建或测试，验证状态为 not-run。
