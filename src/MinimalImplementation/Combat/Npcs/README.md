# NPC：AI、生命与网络同步

[返回 Combat](../README.md) · [服务器总览](../../README.md)

## 作用与主要状态

[Npc](Npc.cs) 表达一个 NPC 实例的权威状态，由 [NpcManager](NpcManager.cs) 管理。
实例拥有战斗身份、槽位、类型、位置、速度、目标、AI 数组、生命、朝向、住房信息、
活动和 Buff 状态，以及 Revision、CommitVersion、IsDirty。
其他系统主要读取 [NpcView](NpcView.cs)，不自行修改实例。

| 部件 | 作用 | 协作关系 |
| --- | --- | --- |
| [NpcDefinitionCatalog](NpcDefinitionCatalog.cs) | 具体类型的尺寸、生命、防御、AI 等内容事实 | Manager 创建和规则计算使用 |
| [NpcAiProfileCatalog](NpcAiProfileCatalog.cs) | 已迁移 AI 档案 | 为支持的类型选择规则 |
| [NpcManager](NpcManager.cs) 及 partial | 出生、状态、AI、Buff、受击、死亡、活动与退休的准备和提交 | 校验身份、版本、作用域并维护脏状态 |
| [NpcTickBatchOwner](NpcTickBatchOwner.cs) | 组合一批 NPC 更新 | 使用捕获的目标、运动事实和世界版本守卫 |
| [NpcTargetingRules](NpcTargetingRules.cs) | 目标选择 | 使用玩家事实，不读取网络连接对象 |
| [NpcDamageRules](NpcDamageRules.cs)、[NpcCombatPacketHandler](NpcCombatPacketHandler.cs) | 伤害规则和受击入口 | 将请求转换为 Manager 可提交的变化 |
| [NpcLootService](NpcLootService.cs) | 战利品规则 | 返回掉落意图，实际物品创建需要后续衔接 |
| [NpcSyncPacketProducer](NpcSyncPacketProducer.cs)、[NpcPacketPayloadCodec](NpcPacketPayloadCodec.cs) | NPC 网络输出构造 | Host 负责接收者和交付结算 |

## 默认内容范围

`NpcDefinitionCatalog.CreateVersion4Core` 当前包含类型 1 史莱姆、2 恶魔眼、3 僵尸、22 向导。
这是一批明确迁移的内容事实。其余类型需要目录和规则支持，不能仅改类型编号生成。

史莱姆、眼球、僵尸和向导的实现分布在 Manager partial 与对应 AI 规则中。向导目前包含
无家时的停留、行走、逃离及重力碰撞等已实现部分，不能据此认定商店、完整住房和全部城镇
NPC 行为都已完成。诊断出生入口见 [NpcDiagnosticSpawn](../../Host/NpcDiagnosticSpawn.cs)。

## 一帧 AI 如何协作

1. [DefaultServerTickPhaseOwners](../../Host/DefaultServerTickPhaseOwners.cs) 中的 NPC owner
   捕获当前玩家目标和 NPC 分布。
2. 从 WorldOwner 捕获需要范围的空间快照和世界版本，构造各类 NPC 输入。
3. NpcTickBatchOwner 计算支持的 AI、运动和其他变化，并准备批次。
4. 校验世界来源版本与 NPC 身份、Revision 后提交，变化进入脏状态。
5. [ModulePeriodicSyncOwner](../../Host/ModulePeriodicSyncOwner.cs) 按周期捕获 NPC 脏快照，
   交给输出泵；尚有待结算批次时避免重复捕获同一批。

空间范围不可用或来源失效时，应追踪相应拒绝结果；空输入不能被当成真实地形继续模拟。

## 攻击与扣血的真实调用顺序

[Version4InboundRouter.Npcs.cs](../../Host/Version4InboundRouter.Npcs.cs) 是默认受击网络入口：

1. Session 验证 Packet 28 来自当前 Active 绑定。
2. 先为认证后的攻击请求产生 Packet 162 预测确认，包括部分已过期目标请求。
3. 从包中解析 NPC 槽和网络代次，检查 NPC、玩家存活及目标区块完整基线的排队记录。
4. NpcCombatPacketHandler 和 NpcDamageRules 处理并提交生命、击退等变化。
5. 给旁观者的 Packet 28 放在 Packet 23 生命快照前；攻击者收到的 Packet 162 也在快照前。
6. [NpcSnapshotDeliverySettlement](../../Host/NpcSnapshotDeliverySettlement.cs) 根据已捕获
   版本和发送结果结算脏状态及致死退休。

Packet 28 中的伤害属于客户端攻击报告。当前入口的认证和区块检查并不构成完整武器、
弹幕或攻击频率验证。诊断时可从 `server.npc.damage` 的输入、实际扣血和前后生命开始。
区块门禁使用 `HasQueuedSection`，表示完整基线已进入有序发送流，不是客户端应用确认。

## 身份与网络代次

[NpcNetworkProjection](../../Host/NpcNetworkProjection.cs) 将运行时实例映射为网络槽和
字节代次，防止旧攻击作用于槽位重用后的新 NPC。战斗实体身份与网络代次的取值空间不同，
不能直接互换。

Packet 23 没有任意独立最大生命值字段，见 [NpcPacket23ProtocolFacts](NpcPacket23ProtocolFacts.cs)。
客户端表现会依赖本地类型默认值。修改服务端最大生命前，应同时检查原版客户端计算受击
表现时所用的默认生命；当前诊断史莱姆采用原生生命。

## 死亡、活动与保存的边界

受击致死、活动范围退休和会话/世界清理是不同路径，应分别查看
[NpcManager.Death](NpcManager.Death.cs)、[NpcManager.Activity](NpcManager.Activity.cs) 与 Host
的发送结算。不能用一种死亡流程推断所有离场方式均完成终态交付。
诊断 NPC 的运行时生成也不代表它会自动写回 WorldFile；保存范围由持久化投影决定。

建议先读 Npc、NpcView、NpcManager，再进入 TickBatchOwner、伤害路由和快照结算。
