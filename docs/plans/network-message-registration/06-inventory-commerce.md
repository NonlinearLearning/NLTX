# 类别 06：容器、库存、装备、Buff 与经济消息执行文档

文档 ID：NETMSG-CAT-06
执行模型：gpt-6-luna，reasoning=max
分支：codex/network-message-06-inventory-commerce
会话：network-message-06-inventory-commerce
类别 owner：Chest/Inventory、装备交易、NPC 商店、任务和 Buff 交互

## 1. 目标

完成容器、库存、装备、Buff 和经济消息的注册与权威 owner 边界。所有客户端请求都必须
转换成有身份、有范围、有版本/占用检查的 Command；所有物品、商店、任务和 Buff 结果
都必须来自领域提交后的 projection。不能因为消息可解码就允许客户端覆盖库存或价格。

## 2. 消息范围

31 RequestChestOpen；32 SyncChestItem；33 SyncPlayerChest；46 OpenSignRequest；
47 OpenSignResponse；51 MiscDataSync；53 AddNPCBuff；54 NPCBuffs；
56 UniqueTownNPCInfoSyncRequest；58 InstrumentSound；69 ChestName；
72 TravelMerchantItems；74 AnglerQuest；75 AnglerQuestFinished；76 QuestsCountSync；
104 ShopOverride；131 TamperWithNPC；137 RequestNPCBuffRemoval；152 ItemUseSound。

## 3. 基线与重叠处理

先核对现有 NetworkWorldItem、Player lifecycle/state、SocialPacketRegistration、
ReservedPacketRegistration，以及 component-decomposition 中 P09/P13 的设计和执行文档。
58/152 是表现或社交边界；本类只接入其与容器/物品使用状态有关的已证明 owner，不把
音效包当作经济逻辑。69 的 ChestName 与 31-33 必须使用同一容器资格和 session owner。

51 MiscDataSync 字段宽且语义混合，缺乏完整证据时只能按已证明子域开放并记录未知字段；
不能接受它作为“万能状态覆盖”。85 QuickStack 不在本类范围，由第 5 类负责世界/容器
交互准入，本类只提供 Inventory owner 端口。

## 4. 实施步骤

1. 建立 19 项登记，按请求、权威快照、表现/兼容三种类型标注方向和阶段。
2. 对 31/46/56/75/131/137 建立交互资格、距离、对象版本、任务/Buff 前置和 owner
   提交接口；拒绝后不得产生部分物品或任务效果。
3. 对 32/33/47/54/69/72/74/76/104/152 提供 detached projection，确保广播目标、
   胸箱占用、商店版本和任务计数不被客户端提供的目标字段替换。
4. 对 51 分解已证明字段或接入 opaque adapter；对未知字段建立诊断但不写入新 ECS。
5. 复用现有 Item/Inventory/Chest/Buff/Town/Quest owner，不在网络层新建库存事实源。
6. 处理并发开箱、重复请求、断线、超时和 owner 已提交但输出失败的语义；不以 session
   串行假设替代世界对象的原子提交。

## 5. 非目标

不实现完整经济系统、商店算法、Chest 存档、Buff 模拟或所有 NPC 交互；不运行全量真实
客户端交易压力测试；不把表现包当作 authoritative command。

## 6. 10% 核心测试

只执行两组核心用例：

1. RequestChestOpen → SyncChestItem/SyncPlayerChest 的合法、距离/占用失败和重复请求，
   检查物品提交原子性。
2. 一个 NPC Buff/任务/商店请求（53 或 75/104/137）验证前置条件、非法来源和权威结果
   投影，覆盖一个请求和一个 S2C 快照。

优先使用现有 NetworkWorldOwnerVerification、PlayerNetworkOwnerVerification、
ReservedPacketVerification 或对应 P09/P13 focused verifier。不得运行整个 Network
Verification Program；构建受影响项目并记录 Build/bin 输出。

## 7. 验收

报告必须列 19 个 ID 的唯一 owner、方向、交互资格、版本/占用策略和未知字段；提供重复
开箱或伪造物品失败证据、测试命令及 exit code、warning/error、产物路径、未覆盖项目和
共享接口冲突。发现网络层直接写 Inventory/Chest 时必须标 failed。

