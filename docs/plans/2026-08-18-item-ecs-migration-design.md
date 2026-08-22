# Item ECS Migration Design

> 状态：已确认设计；服务器权威迁移切片已执行并验证，完整 Terraria parity 保持 `PARTIAL`。

## 目标

将 `D:\TRbackup\Version4物理删除了某些文件\Terraria\Item.cs` 及其相关物品域职责迁移到
`Terraria.Dome.Simulation` 的领域优先 ECS 结构中。最终服务器核心不再实例化旧的
`Terraria.Item`，而是通过不可变定义、运行时组件、确定性系统、命令、事件和快照完成物品
生成、库存、使用、装备、弹药、掉落、变体、持久化和网络投影。

本设计选择一次性建立完整 Item ECS 域，但执行上仍拆成可回滚批次。第一条必须闭环的运行
路径是服务器权威的生成、库存、使用、拾取、装备、弹药、同步和保存恢复。

## 事实基线

- Version4 的 `Terraria\\Item.cs` 存在，约 962 KB。
- Version4 与 Version3 的 `Item.cs` 文件长度相同；Version4 的变化主要还包括物品 ID、世界
  物品、掉落规则、物品变体、创造模式和 UI 相关文件的物理删除。
- 当前 NLTX 已有 `InventoryComponent`、`WorldItemComponent`、`ItemStack`、`ItemDefinition`、
  `ItemDefinitionRegistry`、`UseItemCommand`、`PickupWorldItemCommand`、
  `InventoryTransferSystem` 以及 Item replication/persistence 验证入口。
- 当前 Simulation 使用 Arch ECS，但 Item 的部分运行时状态仍由 `DomeSimulation` 中的
  字典直接持有；本迁移应逐步把这些状态收敛为领域组件和快照。
- Version3 和 Version4 目录只作为只读迁移来源，不在备份目录上直接改写。

## 设计原则

1. 静态属性进入 `Definitions`，实例可变状态进入 `Components`。
2. 客户端只提交意图；最终物品类型、数量、位置、前缀、变体、消耗和装备结果由服务器
   Simulation 决定。
3. 系统读取稳定状态并产生命令，结构变化和状态变更在确定性提交阶段完成。
4. Simulation 不依赖 UI、渲染、音效、`Main` 数组或旧 `Player`/`NPC` 全局对象。
5. Arch Entity、网络 `ReplicationId` 和持久化 ID 必须保持不同的身份边界。
6. 未分类成员进入延期清单，不通过通用组件或 `Manager` 类型隐藏职责。

## 目标目录

```text
src/Terraria.Dome.Simulation/Items/
  Components/
  Definitions/
  Commands/
  Events/
  Systems/
  Snapshots/
  Compatibility/
```

## 静态定义

### `ItemDefinition`

表达类型级基础属性：类型 ID、堆叠上限、宽高、价值、稀有度、使用时间、使用动画、使用样式、
冷却、消耗、自动重复和转身使用。

### `ItemIdentityDefinition`

表达名称键、材料、任务物品、唯一堆叠、专家模式限制和商店货币等身份/经济属性。

### `ItemCombatDefinition`

表达伤害、击退、暴击、护甲穿透、伤害类别、投射物、射速、弹药类型和消耗弹药。

### `ItemPlacementDefinition`

表达创建 Tile/Wall、放置样式和 Tile 增益。

### `ItemRecoveryDefinition`

表达生命、法力、Buff 类型和 Buff 持续时间。

### `ItemEquipmentDefinition`

表达头盔、胸甲、护腿、饰品、翅膀、盾牌等装备槽位，以及防御、Accessory、Vanity 和 Social。

### `ItemDropDefinition` 与 `ItemVariantDefinition`

前者表达掉落类型、数量范围、条件、权重和模式限制；后者表达变体 ID、覆盖字段和合法性
条件。它们都是共享不可变数据，不保存某个实例的当前值。

`ItemDefinitionRegistry` 负责只读查找和启动时校验。Version4 中 `SetDefaults1` 到 `SetDefaults5`
及 `DefaultTo*` 的结果应由定义编译器/导入器生成，而不是把旧方法原样复制进 ECS。

## 运行时组件

- `ItemStackComponent`：物品类型、数量和前缀引用。
- `ItemInstanceStateComponent`：收藏、染色、涂层、名称覆盖和变体 ID。
- `ItemUseStateComponent`：冷却、channel、使用动画进度和持续使用状态。
- `ItemEquipmentStateComponent`：当前装备槽、vanity 状态和来源槽位。
- `ItemWorldStateComponent`：激活、拾取延迟、生成来源、广播状态和替换/合并时间戳。
- `ItemOwnershipComponent`：所属玩家/容器、来源实体和所有权变更版本。
- `InventoryComponent`：完整槽位容器、热键栏映射、选中槽位、锁定状态和版本号。

`ItemStack` 保留小型值语义，只表达类型和数量。前缀、变体、染色、收藏和唯一堆叠规则不得
被压入 `ItemStack`。

## 命令、事件和快照

### 命令

`CreateWorldItemCommand`、`DestroyWorldItemCommand`、`MoveWorldItemCommand`、
`PickupWorldItemCommand`、`TransferItemCommand`、`SplitItemStackCommand`、
`MergeItemStackCommand`、`UseItemCommand`、`EquipItemCommand`、`UnequipItemCommand`、
`DropItemCommand`、`ApplyItemPrefixCommand` 和 `ApplyItemVariantCommand` 表示尚未提交的意图。

### 事件

`WorldItemCreatedEvent`、`WorldItemPickedUpEvent`、`WorldItemDestroyedEvent`、
`InventoryChangedEvent`、`ItemUsedEvent`、`ItemEquippedEvent`、`ItemDroppedEvent` 和
`ItemPrefixChangedEvent` 只表示已经提交的事实。

### 快照

`ItemInstanceSnapshot`、`InventorySnapshot`、`EquipmentSnapshot`、`WorldItemSnapshot` 和
`ItemUseSnapshot` 是只读投影。Server 只能从这些快照构造 `SyncItem`、库存、装备和使用状态帧。

## 系统和调度

Item 相关系统按以下顺序插入 Simulation tick：

```text
Input freeze
  -> ItemInputValidationSystem
  -> InventorySelectionSystem
  -> ItemUseCooldownSystem
  -> ItemUseSystem
  -> ItemAmmoConsumptionSystem
  -> ItemEquipmentSystem
  -> ItemDropRuleSystem
  -> WorldItemSpawnSystem
  -> WorldItemMotionSystem
  -> WorldItemPickupSystem
  -> InventoryTransferSystem
  -> ItemVariantSystem
  -> ItemPersistenceSystem
  -> ItemReplicationSnapshotSystem
  -> Arch structural command playback
```

使用失败、弹药不足、非法槽位、越权拾取和装备冲突必须是无副作用拒绝。正常业务拒绝使用
结构化 `ItemCommandRejection`；异常仅用于损坏的注册表、损坏的快照和违反内部不变量。

## 明确排除项

`ToolTip`、`RebuildTooltip`、`ItemSlot`、`ItemSorting`、`GetDrawHitbox`、`GetPhaseColor`、
纹理/Shader/音效对象、`Lang.GetItemNameValue`、`BestiaryNotes`、`ItemTagHandler`、
`CreativeUI` 以及依赖 `Main`、`Player`、`NPC`、`ContentSamples` 全局数组的调用，不进入
Simulation。它们只能通过 Compatibility 或客户端投影读取定义/快照。

## 主要不变量

- `ItemType == 0` 或 `Quantity <= 0` 只表示空槽或非活动 tombstone。
- 库存数量不超过对应定义的 `StackLimit`。
- 不兼容的唯一堆叠、前缀、变体或染色实例不得合并。
- 世界物品 `Revision` 只递增，`ReplicationId` 不等于 Arch Entity。
- 一次成功使用最多扣除一次主物品和一次对应弹药。
- 同一世界物品同一 tick 最多有一个拾取赢家。
- 客户端伪造 `SyncItem`、装备确认或物品状态包不能创建权威状态。

## 完成条件

只有在服务器生成、库存、使用、拾取、装备、弹药、掉落、同步、保存恢复均有验证证据，且
Simulation 不再实例化旧 `Terraria.Item`、所有 `Item.cs` 成员都有迁移或延期归属时，才可声称
Item ECS 迁移完成。
