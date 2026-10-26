# Item 领域

本目录把物品定义、无实体物品堆和实体化物品状态分开表达。

```text
ItemDefinition
    ↓ definition id
ItemStack                         # 无实体状态，例如库存或容器槽位
    ↓ 作为组件附着到实体
ItemStackComponent                # ECS 实体上的权威堆状态
ItemWorldStateComponent           # 仅实体化后存在的拾取/寿命状态
```

## 无实体物品

`ItemDefinition` 是不可变定义，`ItemStack` 是值对象。`InventoryComponent` 的普通、武器、
护甲或饰品类别槽位，以及其他容器，都可以直接保存 `ItemStack`，不需要为每个普通槽位创建
Arch 实体。数量和前缀属于堆状态，定义只提供规则和最大堆叠数。具有耐久、弹药库存或其他
独立实例组件的物品由库存关系保存 `EntityUuid`，继续使用原实体；这类物品不能被压平为普通
值堆。装备关系因此属于 Inventory 的槽位定义和归属关系，不是 Item 或 Player 的第二套槽位
组件。

## 有实体物品

世界中的物品实体至少附着 `ItemStackComponent`。需要世界生命周期时再附着
`ItemWorldStateComponent`，并组合现有的 `Terraria.Entity.LocationComponent`、
`Terraria.Movement.VelocityComponent` 和 `Terraria.Physics.ColliderComponent`。
Arch `Entity` 是运行时句柄，不被这些共享组件复制保存。

```text
Arch Entity
  ├─ ItemStackComponent
  ├─ ItemWorldStateComponent
  ├─ LocationComponent
  ├─ VelocityComponent
  └─ ColliderComponent
```

`ItemDefinitionRegistry` 只管理不可变定义，不保存物品实例、库存或世界实体。
创建、拾取、堆叠和销毁由宿主系统负责，并在一个明确的提交点更新库存堆与实体堆，避免
出现两份权威数量。物品进入库存后，宿主应停止其世界拾取和寿命处理；物品从库存取出后，
宿主再恢复位置、碰撞和拾取延迟等世界状态。

装备、饰品和武器对属性的贡献由物品定义/实例提供给
`InventoryAttributeContributionSystem`，再作为 `AttributeModifier` 来源提交给持有者的
`AttributeSetComponent`。`Item` 不保存一份玩家装备槽，`Player` 也不拥有装备关系组件；
槽位类别和 `ContainedInInventoryComponent` 的父库存关系是唯一的装备归属事实。
