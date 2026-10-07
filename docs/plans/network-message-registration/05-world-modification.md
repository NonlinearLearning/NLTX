# 类别 05：世界修改、Tile、连线与 Tile Entity 消息执行文档

文档 ID：NETMSG-CAT-05
执行模型：gpt-6-luna，reasoning=max
分支：codex/network-message-05-world-modification
会话：network-message-05-world-modification
类别 owner：世界写命令、Tile/液体/连线、容器关联世界对象和 Tile Entity

## 1. 目标

把世界修改消息注册为经过权限、距离、边界、资源和版本校验的 Command；把服务端已提交
结果注册为 S2C projection。消息层不能直接改 Tile、液体、连线或 TileEntity 组件，也不
能把客户端选择的 remote target 当成广播授权。

## 2. 消息范围

17 TileManipulation；19 ToggleDoorState；34 ChestUpdates；42 Unknown42；48 LiquidUpdate；
52 LockAndUnlock；59 HitSwitch；63 SyncTilePaintOrCoating；64 SyncWallPaintOrCoating；
79 PlaceObject；85 QuickStackChests；86 TileEntitySharing；87 TileEntityPlacement；
89 ItemFrameTryPlacing；105 GemLockToggle；108 WiredCannonShot；109 MassWireOperation；
110 MassWireOperationPay；121 TEDisplayDollDataSync；122 RequestTileEntityInteraction；
123 WeaponsRackTryPlacing；124 TEHatRackItemSync；128 LandGolfBallInCup；
133 FoodPlatterTryPlacing；146 ShimmerActions；149 DeadCellsDisplayJarTryPlacing；
154 Ping；155 SyncChestSize；156 TELeashedEntityAnchorPlaceItem。

## 3. 基线与重叠处理

当前已有 TilePlacementPacketVerification、LiveTilePlacementVerification、WorldTileMetricsVerification、
NetworkWorldOwnerVerification 等材料；先盘点 Tile placement/section/Ping 的当前 owner。
109、154 在分类审查中也出现于 UI/社交或 NetModules 周边，必须保证 gateway key 只有
一个注册 owner：本类持有 109 的世界写操作与 154 的地图标记权限校验，第 9 类只能
消费结果/表现，不得再注册相同 ID。

42、48 属于未知/废弃兼容路径，不能新建 ECS 业务系统。85、94 等高风险/证据不足项
默认保持禁用，除非本执行会话找到真实 wire/owner/测试证据并在报告中说明。

## 4. 实施步骤

1. 按 C2S command、S2C result、Legacy/opaque 建立 29 项登记，不按文件名推断权限。
2. 为 17/19/52/59/79/87/89/105/108/109/122/123/128/133/149/156 统一接入可信 Actor、
   世界代次、距离/空间、权限、工具/资源和一次性提交边界。
3. 为 34/85 把容器/库存原子性留给 Chest/Inventory owner；网关只做交互资格和有界准入。
4. 为 63/64/86/110/121/124/146/155/154 接入已提交 projection，目标由 owner 的 section
   subscriber 或明确目标产生，网关再次复验。
5. 对 17/20/63/64 的顺序和 revision 做至少一次邻近区段回读；不写源目录生成物。
6. 保留 42/48 的兼容读取和诊断，不把它们注册为新的可写 handler。

## 5. 非目标

不实现完整 Tile/液体/连线模拟、Chest/Inventory 交易、TileEntity 的全部类型、Ping UI
绘制；不把网关串行会话当作世界级事务锁。

## 6. 10% 核心测试

只执行三组：

1. 17 附近普通 Tile 的合法修改、越界/距离拒绝及 20/63/64 结果投影。
2. 87/122/123/156 中一个 TileEntity 交互链的合法、错误实体和旧代次拒绝。
3. 109 批量连线或 154 Ping 的权限/目标范围边界，确保不接受客户端自定义广播目标。

优先使用 TilePlacementPacketVerification、LiveTilePlacementVerification、
WorldPacketBehaviorVerification；运行前构建受影响项目，验证只跑选定 case。85/94/未知
包不因测试缺席而标为 complete。

## 7. 验收

报告列出 29 项的唯一 owner、读写方向、权限条件、revision/目标规则和 Legacy 处理；
提供至少一条世界提交前后证据，包含命令、exit code、warning/error、Build/bin 输出和
共享文件冲突。若领域写入由网络层直接执行，验收必须失败并返工。

