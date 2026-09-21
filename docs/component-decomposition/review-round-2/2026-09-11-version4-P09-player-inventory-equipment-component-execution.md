# Version4 P09 玩家库存、装备、Buff 与防御装载执行计划

partitionId: P09
sessionId: c040060f00534e5fac6f66456c4c1d80
inputReport: D:\TRbackup\NLTX\docs\migration\ledgers\authoritative-20-partitions\P09-Player-Inventory-Equipment.md
componentDesignPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\2026-09-11-version4-P09-player-inventory-equipment-component-design.md
componentExecutionPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\2026-09-11-version4-P09-player-inventory-equipment-component-execution.md
designStatus: proposed
executionStatus: failed
implementationStatus: implemented
evidenceStatus: partial
currentNltxStatus: partial
verificationStatus: not-verified
completedComponents: [PlayerInventorySlotsComponent, PlayerContainerRelationComponent, PlayerEquipmentRelationComponent, PlayerBuffSlotsComponent, PlayerBuffImmunityComponent, PlayerResourceStateComponent, PlayerHeldItemPresentationStateComponent, PlayerEquipmentEffectStateComponent, PlayerDisplayEntityModeComponent, PlayerVisibleEquipmentSelectionComponent, PlayerAppearanceSelectionComponent, PlayerEquipmentColorProjectionComponent, PlayerDefenseStateComponent, PlayerInteractionLockStateComponent, PlayerLoadoutStateComponent]
currentComponent: none
pendingComponents: []
lastCheckpointUtc: 2026-09-12T10:31:22Z
evidence-gap: P09 15 个允许范围内的 ECS 组件均已写入 src/Player 并完成文件存在性核对；Terraria.Player.csproj 串行构建被 P09 之外的既有 src/Player/Progression 缺失跨项目引用阻塞，verificationStatus 保持 not-verified。hoveredChestIndex、hurtCooldowns、meleeNPCHitCooldown、GetItemLogger、visual clone serialization workspace、heldProj/stealth、P02/P11 animation、Item/Container、网络/持久化和颜色 metadata 仍保留在对应跨域 owner 或 integration-review 边界。未开始行为迁移、网络/持久化闭合或测试。
blocking-decision: build-blocked-by-pre-existing-player-progression-references; runner settlement must be Fail

## 1. 计划边界

本文件记录 P09 的执行状态和后续执行边界。组件源码必须按最小实现单元逐批保存，每批先确认字段、默认值和 owner 边界，再按仓库构建规则验证；本任务禁止新增 focused verifier、System、Query、Command 或 adapter。任何检查点失败都保留旧兼容路径并记录阻塞，不得通过双写掩盖差异。

文档中的未完成依赖和验证项必须保持真实状态；当前允许范围内的组件源码均已写入，但尚未产生编译或测试证据。

## 2. 建议文件与依赖影响

实现阶段优先复用现有 src/Player、src/Items、src/Combat 领域根目录；只有出现稳定独立边界和足够规模时才增加子目录。一个公开类型一个同名 PascalCase 文件，不建立 Shared/Components 等泛化目录。

| 检查点 | 计划文件/接口 | 源路径到目标边界 | 依赖影响与回滚 |
|---|---|---|---|
| C01 | src/Player/PlayerInventorySlotsComponent.cs；src/Player/PlayerContainerRelationComponent.cs；src/Player/PlayerInventorySystem.cs；src/Player/PlayerInventoryQueries.cs；src/Items/InventoryTransferCommand.cs | Player.inventory/trash/inventoryChestStack/bank* / voidVaultInfo -> 玩家关系与槽位；Item/Chest contents 留在 src/Items | 收敛 PlayerInventoryComponent、PlayerInventoryState、Items Inventory/Container prototypes；若 slot/void vault 顺序或 owner verifier 失败，恢复旧读路径并删除本批新增类型 |
| C02 | src/Player/PlayerEquipmentRelationComponent.cs；src/Player/PlayerEquipmentSystem.cs；src/Player/PlayerEquipmentQueries.cs；必要时 src/Items/EquipmentRelationComponent.cs 收敛 | Player armor/dye/misc* -> 玩家装备关系；Item 详情留在 Items | 解决 PlayerEquipmentComponent、EquipmentComponent、EquipmentRelationComponent 重复；Swap/隐藏不变量失败则保留旧 adapter |
| C03 | src/Player/PlayerBuffSlotsComponent.cs；src/Player/PlayerBuffImmunityComponent.cs；src/Player/PlayerResourceStateComponent.cs；src/Player/PlayerBuffResourceSystem.cs | buffType/time/immune 与 breath/lava -> 窄组件；ignoreWater/lavaVision/lavaOpacity -> derived output | 与 Combat StatusEffectSlots/Immunity 和能力系统协商 owner；slot pairing 或 reset 失败则不切换读者 |
| C04 | src/Player/PlayerHeldItemPresentationProjection.cs；src/Player/PlayerEquipmentEffectProjection.cs；必要时 src/Player/PlayerDisplayEntityModeComponent.cs | itemRotation/location/heldProj/armor effect/social/display fields -> projection/adapter；stealth 交 Combat | renderer、projectile、audio、UI 只读 snapshot；任何反写或 tick 顺序差异则回滚 projection route |
| C05 | src/Player/PlayerVisibleEquipmentSelectionProjection.cs | head/body/legs/coat/accessory selection -> derived snapshot | 输入来自 equipment/hidden/mount/body；不移动到 Equipment authority；显示层差异则恢复 legacy selection reads |
| C06 | src/Player/PlayerAppearanceSelectionComponent.cs；src/Player/PlayerAppearanceSelectionSystem.cs；必要时 P11 adapter | voice/hide preferences -> authority；jump/bodyFrame/legFrame -> animation projection | 网络 bitmask 和 loadout Swap 需先有 packet evidence；保存格式未确认不得实现 persistence |
| C07 | src/Player/PlayerEquipmentColorProjection.cs；src/Player/PlayerEquipmentColorQuery.cs | c* -> one-way projection | 只读 armor/dye/selection，颜色失效或 shader 顺序失败则保留旧 fields |
| C08 | src/Player/PlayerDefenseState.cs；src/Player/PlayerInteractionLockState.cs；src/Player/PlayerLoadoutState.cs；src/Player/PlayerItemAcquisitionLogAdapter.cs；src/Player/PlayerVisualCloneAdapter.cs | defense/loadout -> 分离 authority；logger/clone static -> effects adapter | hurt/melee cooldown 交 Combat；clone stream 资源语义失败则不进入 ECS component |

注意：上述是目标路径计划，不是当前文件状态。路径变化必须在实际迁移前记录 source/target/dependency impact，并按受影响项目的仓库构建规则验证。

## 3. 全局执行不变量

- 先有唯一 writer，再迁移 readers；任何 authority 不允许 legacy component 与新 component 双写。
- ItemEntityRef、RuntimeEntityId、PersistentItemId、NetworkId 和外部 slot/index 保持不同类型和语义；不以数组位置冒充持久化 ID。
- Component 只保存可持续状态；Command 保存一次性意图；Query 纯计算且不写回；Adapter 处理文件、网络、日志、UI、音频和流；Projection 输出不可变 snapshot。
- 时间、随机数、序列化流、日志和消息发送通过显式 adapter/port 注入或集中在执行边界；失败、超时未知、重复和恢复行为写进 focused verifier。
- 每批完成后保存设计和执行两份文档，检查点字段一起更新，再允许开始下一批。
- 不运行用户明确禁止的 git diff --check -- <两份文档>；改用路径、状态、结构、来源序号和重复成员检查。

## 4. 计划验证命令

实现阶段的 compile-capable command 必须从仓库根目录串行运行，并先检查 dotnet.exe/csc.exe：

    pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 build .\src\Player\Terraria.Player.csproj -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false

    pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 test .\Test\<affected-player-verifier>.csproj --no-build --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false

实际验证记录：

- 首次命令：`pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 build .\src\Player\Terraria.Player.csproj -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false`；wrapper 在 PowerShell 参数绑定阶段退出码 `1`，提示 `-p` 参数模糊，未启动 dotnet，警告/错误计数不适用。
- 实际构建命令：`$dotnetArguments = @('build', '.\\src\\Player\\Terraria.Player.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false'); & .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArguments`；项目 `src/Player/Terraria.Player.csproj`；退出码 `1`；`0` 个警告、`5` 个错误。错误均来自既有 `src/Player/Progression/PlayerMinionCapacityCommitSystem.cs` 和 `src/Player/Progression/SubmitMinionCapacityDeltaCommand.cs` 的缺失跨项目类型引用，未出现 P09 新增组件错误。
- 预期 artifact：`D:\\TRbackup\\NLTX\\Build\\bin\\Terraria.Player\\Debug\\net10.0\\Terraria.Player.dll` 存在，但本次构建失败，因此标记为 `stale/unverified`，不作为成功构建证据。
- 未运行测试或 focused verifier；仓库没有 P09 专用 focused verifier。未运行网络复制、持久化往返或行为等价验证。

## 5. 组件检查点

### C01 PlayerInventoryAndContainerSlots

状态：implemented；PlayerInventorySlotsComponent、PlayerContainerRelationComponent 和 PlayerEquipmentRelationComponent 已写入，尚未编译或运行 focused verifier。

目标：把玩家库存、垃圾槽、四个命名容器关系和 void-vault capability 从 Item 内容 owner 中分离。

执行步骤：

1. 先为 59 槽 inventory、trash、四个 bank relation、voidVault capability 和 chest-stack marker 建立不变量 verifier；验证初始化、selected/slot 约束、void vault 路径和 pickup 顺序。
2. 明确 Item/Container 端的 read-only Query 与 ItemTransferCommand；将 GetItem 的 stack、coin merge、favorite、logger、UI/audio effect 分为计算结果和 effect adapter。
3. 引入一个 PlayerInventorySystem writer，旧 PlayerInventoryComponent/State 只读适配；迁移 HasItem、FindItem、CanPullItem、FillAmmo、GetItem 读者。
4. 用 MessageBuffer/NetMessage slot evidence 和持久化 evidence 决定 adapter；Version4 Save/Serialize 空实现时保持 persistence blocked。

验收：slot count 59、四个 bank relation 不丢失、trash/void vault fallback 与原顺序相同、重复 transfer 不双扣、不发生 Player/Items 双写。

### C02 PlayerEquipmentAndDyeSlots

状态：implemented；PlayerEquipmentRelationComponent 已写入，尚未编译或运行 focused verifier。

目标：建立一个唯一的玩家装备关系 owner，覆盖 armor 20、dye 10、miscEquips 5、miscDyes 5，并将 Item payload 留给 Items。

执行步骤：

1. 先测试空槽初始化、装备/染料数组容量、hidden/loadout 交接和 Item relation 版本。
2. 选择 PlayerEquipmentComponent、Items EquipmentComponent、EquipmentRelationComponent 中唯一 authority；其余保留只读兼容 view，不可并行写入。
3. 将 UpdateArmorSets、UpdateDyes、PlayerFrame 和 mount/effect 查询改为消费 EquipmentQuery；将装备改变表示为 EquipmentCommand。
4. 在网络/保存格式证据确认前不改 packet/file adapter；验证染料更新前后投影失效。

验收：装备关系原子提交、loadout swap 不丢 armor/dye/hide、装备 effect 只在 commit 后重算，并证明 PlayerEquipmentComponent、Items EquipmentComponent、EquipmentRelationComponent 没有双写。

### C03 PlayerBuffAndResourceSlots

状态：implemented；PlayerBuffSlotsComponent、PlayerBuffImmunityComponent 和 PlayerResourceStateComponent 已写入，尚未编译或运行 focused verifier。

目标：保持 Buff type/time 槽不变量，并分别表达免疫、呼吸/岩浆资源和由状态推导的环境能力。

执行步骤：

1. 建立 44 槽 pairing verifier：Add/refresh/Delete/compact/net apply 任一操作同时更新 type/time，空槽不产生残留 effect。
2. 建立 immunity verifier：buffImmune 不等同于 hurt cooldown，ResetEffects 与 equipment/status effect commit 的清空/重建顺序显式化。
3. 迁移 breathMax/breath/lavaMax/lavaTime 的 tick/环境 writer；ignoreWater/lavaVision/lavaOpacity 仅作为 derived snapshot 输出。
4. 对照 MessageBuffer case 50、NetMessage case 50 和 full-reference persistence supplement；Version4 直接 Save 仍标记 blocked。

验收：BuffNet 的 0 terminator、持久 Buff 清理、资源 tick 和 reset 顺序可重复；Buff type/time 永不分离；无 Combat status effect 或环境能力双写。

### C04 PlayerEquipmentPresentationState

状态：implemented；PlayerHeldItemPresentationStateComponent、PlayerEquipmentEffectStateComponent 和 PlayerDisplayEntityModeComponent 已写入，尚未编译或运行 focused verifier。

目标：把持有物位置/旋转、held projectile link、装备效果绘制 flags、潜行交接和 display-doll 模式从玩家 authority 与 Item 行为中隔离。

执行步骤：

1. 建立 item-use -> immutable held-item snapshot verifier，覆盖 net item rotation、animation end、heldProj invalidation。
2. 以 ResetEffects -> SetArmorEffectVisuals 的明确顺序生成 effect projection；renderer/audio/UI 只消费 snapshot。
3. 将 stealthTimer/stealth/shroomiteStealth 与 Combat/Ability 做 integration-review，未定 owner 前不新增第二个 writer。
4. 将 lastVisualizedSelectedItem 作为兼容 snapshot；禁止将完整 Item clone 变成持续 Player component。

验收：每 tick reset 可重复、表现投影不反写 equipment、item use/animation 期间 snapshot 不读取已释放引用；stealth 和 heldProj 没有第二个 authority writer。

### C05 PlayerEquipmentSelectionSlots

状态：implemented；PlayerVisibleEquipmentSelectionComponent 已写入，尚未编译或运行 focused verifier。

目标：把 21 个 head/body/legs/饰品字段明确定义为可重建可见装备选择 projection，而不是第二份装备 authority。

执行步骤：

1. 以 armor/dye/hidden/mount/body state 构造纯 VisibleEquipmentSelectionQuery，测试默认 -1、遮挡、坐骑、盾牌、披风和性别鞋槽规则。
2. 替换 PlayerFrame/UpdateVisibleAccessories 的内部写回为 projection builder；旧字段只保留 adapter 直到输出 verifier 通过。
3. 检查 C07 color projection 对最终可见 slot 的依赖，禁止 color 或 renderer 写回 selection。

验收：相同输入产生相同 21 项输出，隐藏/坐骑变化正确失效，死者或展示实体路径不产生未初始化选择；selection projection 不写回装备或外观 preference。

### C06 PlayerAppearanceSelectionState

状态：implemented；PlayerAppearanceSelectionComponent 已写入，尚未编译或运行 focused verifier。

目标：分离外观偏好/语音 authority 与 jump/bodyFrame/legFrame 动画投影。

执行步骤：

1. 为 hideVisibleAccessory[10]、hideMisc 和 voiceOverride 建立验证与 bitmask round-trip 夹具；网络 packet 147 只调用命令。
2. 将 loadout Swap 的隐藏数组交换纳入 Equipment/Appearance transaction，不能只交换 Item。
3. 将 jump、bodyFrame、legFrame 作为 P02/P11 integration seam 的输入/输出；未确定 owner 前不复制写者。
4. 保存格式未由 Version4 直接实现确认前，只生成 in-memory snapshot，不写文件 adapter。

验收：隐藏 bitmask 16 位编码/解码、loadout 交换和默认 reset 一致；frame projection 不改变 authority；未确认保存格式不被写入新 adapter。

### C07 PlayerEquipmentColorProjection

状态：implemented；PlayerEquipmentColorProjectionComponent 已写入，尚未编译或运行 focused verifier。

目标：从已提交 armor/dye/hidden/visible selection 计算 20 个 c* 值，建立单向、可失效的颜色快照。

执行步骤：

1. 先用 UpdateDyes 的默认清零、robe、hidden、slot 分类和 shield fallback 建立纯查询测试。
2. EquipmentColorProjection 只读 Item definition slot metadata 和 dye facts；所有 c* 写入集中于一个 projection builder。
3. 在 equipment/dye/hidden/mount/selection commit 后发送 invalidation；renderer/shader 只消费 snapshot。

已写入源码：src/Player/PlayerEquipmentColorProjectionComponent.cs，包含 20 个 C* 颜色投影字段；CShieldFallback 默认值为 -1。

当前验证：组件源码存在且结构已核对；尚未编译或运行 focused verifier，整体 verificationStatus 保持 not-verified。

验收：重复计算确定性、cShieldFallback 的 -1/回退规则、shield/face/balloon layer 分类一致；不出现反向写装备，renderer 不成为颜色 writer。

### C08 PlayerDefenseLoadoutAndCloneState

状态：implemented；PlayerDefenseStateComponent、PlayerInteractionLockStateComponent 和 PlayerLoadoutStateComponent 已写入，尚未编译或运行 focused verifier。

目标：拆开防御交互、tile interaction lock、loadout、Combat cooldown、logger 和 visual clone 资源。

执行步骤：

1. 建立 shield guard/parry timer/cooldown 的状态机 verifier，并验证 hasRaisableShield 的 reset/ability 输入。
2. 建立 interaction lock 的 tick/释放边界和 hovered chest 的 UI snapshot；不把 UI hover 写入持久权威。
3. 在 EquipmentLoadout Swap 上验证 armor/dye/hide 的原子交换、CurrentLoadoutIndex 范围和网络同步。
4. 与 Combat owner 对 hurtCooldowns/meleeNPCHitCooldown 做 integration-review；与 Item owner 对 GetItemLogger 做 effect seam。
5. 把 visual clone 的 stream/writer/reader 改为调用范围内可释放的 VisualCloneAdapter 设计候选；在序列化失败/异常/重复时保持资源安全。

已写入源码：src/Player/PlayerDefenseStateComponent.cs、src/Player/PlayerInteractionLockStateComponent.cs、src/Player/PlayerLoadoutStateComponent.cs。PlayerLoadoutStateComponent 为三个 loadout 槽分别初始化 20 个 equipment、10 个 dye 和 10 个 hidden accessory 关系。

deferred/blocked 成员：hoveredChestIndex、hurtCooldowns、meleeNPCHitCooldown、GetItemLogger、_visualCloneDummyData、_visualCloneStream、_visualCloneWriter、_visualCloneReader 分别保持 UI/Combat/Item effect/serialization workspace owner；本任务不创建对应非组件代码。

验收：防御状态转换、loadout 原子性、Combat cooldown 单一 owner 和 clone resource lifetime 全部有 verifier 后，才允许实现；hover/logger/stream 不进入权威组件。

## 6. 回滚与完成条件

每个检查点的回滚单位是本检查点新增的类型、adapter 和 reader route；恢复旧 authority writer 和兼容读路径，不删除其他分区文件。失败场景包括：slot/index 变化、网络 bitmask/Item snapshot 变化、Buff type/time 解耦、Projection 反写、Combat 双写、保存字节格式未知、流资源泄漏或消息重复造成状态漂移。

本 P09 会话完成的含义是：允许范围内的组件源码已写入并按实际证据记录；它不表示未授权的 System、Query、Command、Adapter、Projection、网络、持久化或行为等价工作已完成。

## 7. 当前检查点状态

设计层 C01-C08 检查点已完成；实际实现已完成 PlayerInventorySlotsComponent、PlayerContainerRelationComponent、PlayerEquipmentRelationComponent、PlayerBuffSlotsComponent、PlayerBuffImmunityComponent、PlayerResourceStateComponent、PlayerHeldItemPresentationStateComponent、PlayerEquipmentEffectStateComponent、PlayerDisplayEntityModeComponent、PlayerVisibleEquipmentSelectionComponent、PlayerAppearanceSelectionComponent、PlayerEquipmentColorProjectionComponent、PlayerDefenseStateComponent、PlayerInteractionLockStateComponent 和 PlayerLoadoutStateComponent。当前没有 pending 组件；Player 项目构建被既有 Progression 引用错误阻塞，未运行 focused verifier，verificationStatus 保持 not-verified。runner 已使用原始 `partition=P09` 与 `sessionId=c040060f00534e5fac6f66456c4c1d80` 调用 `Fail`，返回 `status=failed`、`lockReleased=true`。每次验证或状态变化时，本节和顶部的 completedComponents、currentComponent、pendingComponents、lastCheckpointUtc、evidence-gap、blocking-decision、verificationStatus 必须同步更新。
