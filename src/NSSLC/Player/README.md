# Player 领域组件与系统

Player 在 NSSLC 中是由组件组合表达的实体能力集合，不是一个复制客户端 `Player` 聚合的
大对象。客户端输入、摄像机、瞄准光标、表现状态和网络投影不进入这些组件。

## Player 专属组件

| 组件 | 权威职责 |
| --- | --- |
| `PlayerResourceStateComponent` | Mana 等真正由 Player 拥有的资源和确定性恢复进度 |
| `PlayerDeathStateComponent` | 死亡转换、死亡 Tick 和原因身份 |
| `PlayerRespawnStateComponent` | 同一实体复活的延迟和可选出生点身份；不直接写位置 |
| `PlayerTargetingStateComponent` | 服务器选择的目标实体关系；不是客户端锁定框或光标状态 |
| `PlayerInteractionCapabilityComponent` | 使用物品、拾取、制作和 Tile 操作等权威能力声明 |
| `PlayerCraftingContextComponent` | 当前工作台实体和配方身份；不保存配方库存 |
| `AttributeSetComponent` + `ImmunityWindowComponent` | 防御/减伤属性与共享受击免疫窗口 |

以下职责不属于 Player 私有组件：

- 武器、护甲、饰品和弹药属于 `InventoryComponent` 与物品实体关系；
- 防御、抗性、耐力和数值 Buff/Debuff 的来源修饰属于 `AttributeSetComponent` 的
  `AttributeModifier`；Player 的基础防御和基础抗性由 `AttributeSetComponent` 保存属性，
  受击免疫窗口由共享 `ImmunityWindowComponent` 保存；
- 无敌帧、受击免疫、闪避冷却和护盾消耗属于共享 Combat 状态：窗口按 6 个伤害通道分槽，
  NPC 侧另按攻击者分槽（`AttackerImmunityComponent`）；
- MovementIntent 由宿主提交，空间写入由 `SpatialSimulationCoordinatorSystem` 完成。

## 系统边界

`PlayerResourceSystem` 只推进 Player 资源恢复；共享 `ImmunityWindowExpirySystem` 只把过期
通道戳归零、不承担「是否被挡」的判断（判定发生在读取点，按 tick 比较），
`AttackerImmunityExpirySystem` 同理清理攻击者稀疏表；`PlayerTargetingSystem` 只清理当前世界中
已经失效的目标身份；`PlayerDeathRespawnSystem` 只提交生命和死亡/复活状态，不直接修改
位置、库存、属性或外部表现。Inventory、Attributes、Combat、Movement 和 World 系统仍由
各自 owner 负责。

当前没有为了接口完整性创建空的 `PlayerMovementIntentSystem`、`PlayerCombatSystem` 或
`PlayerInteractionSystem`：MovementIntent 由宿主提交给共享空间 owner，Combat 和 World
交互分别由共享 Combat/World 系统在获得真实请求组件后处理。

建议的本地 Tick 关系为：

```text
宿主 MovementIntent
→ SpatialSimulationCoordinatorSystem
→ PlayerResourceSystem
→ PlayerDefenseStateSystem
→ Combat DamageCommitSystem
→ PlayerDeathRespawnSystem
→ PlayerTargetingSystem
```

注册顺序由宿主通过 `EcsServer` 明确提供；文件名和目录顺序不表达调度语义。

