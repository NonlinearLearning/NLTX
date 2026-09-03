# Player.cs 字段与属性 ECS 迁移对照（排除 ID 类型）

**审查日期：** 2026-08-28  
**旧字段 Oracle：** `D:\\TRbackup\\Version4物理删除了某些文件\\Terraria\\Player.cs`  
**行为边界：** [`docs/migrations/player-legacy-behavior-map.md`](../migrations/player-legacy-behavior-map.md)  
**基线报告：** [`docs/research/2026-08-28-field-property-migration-comparison.md`](2026-08-28-field-property-migration-comparison.md)

## 结论

本报告只评价 `Player.cs` 的字段和属性语义是否有 ECS owner、运行时读写边界以及必要的快照、持久化或协议投影。按基线报告的词法声明口径，`Player.cs` 有 **1,052 个非 ID 字段/属性声明**，文件级保守估算为 **约 30%**。这不是同名复制率：一个旧字段可能被拆为组件、系统、快照和协议投影；只有名称或 `LegacyReference` 归属不算迁移完成。

按行为族判断：生命周期、输入、移动、生命/法力、死亡/重生、库存/基础物品使用、持久化、复制和部分世界交互已形成可运行的 ECS 链路；装备计算和 Buff 行为仍由其他域委托或只具备容器状态；坐骑、翅膀、绳索、钓鱼、高尔夫、表情等专用机制仍阻塞；绘制、相机、音频、UI 和社交字段是客户端范围，不应移入服务器模拟。

## 排除边界

用户要求忽略 ID 类型的字段和属性。本报告不将 `PlayerHandle`、账户 UUID、网络槽位、实体寻址或其他身份编号计入迁移分数或缺口；它们仍必须保留在实体生命周期、持久化和网络协议契约中。该排除只改变评价口径，不删除源码字段。

## 字段/属性族对照

旧 Oracle 的行号来自 `Player.cs`；当前源码行号来自本仓库 2026-08-28 工作树。

| 旧字段/属性（Oracle 行） | ECS 当前 owner 与证据 | 状态 | 迁移判断与缺口 |
| --- | --- | --- | --- |
| `active`（478）、`dead`/`respawnTimer`（1134-1142） | `PlayerLifecycleComponent.IsActive/RespawnTicks/Spawn`（`src/Terraria.Dome.Simulation/Player/Components/PlayerLifecycleComponent.cs:5-10`）；死亡门控（`PlayerDeathSystem.cs:11-25`）；重生重置（`PlayerRespawnSystem.cs:8-29`） | **verified** | 生命周期状态、死亡到重生的写入边界和位置/速度/生命重置均有 owner。旧 `deadTime`、完整死亡副作用和全部复活规则仍未逐项对齐。 |
| `controlLeft/Right/Up/Down/Jump/UseItem`（1219-1231）、`controlDash`/释放标志（1241-1249） | `PlayerInputComponent`（`src/Terraria.Dome.Simulation/Components/PlayerInputComponent.cs:3-13`）；输入校验与映射（`PlayerInputApplySystem.cs:11-58`）；控制到速度/意图（`PlayerControlSystem.cs:14-57`） | **verified / partial** | 基础左右移动、跳跃、开火、使用物品和朝向已进入 typed input -> component -> system。上/下、冲刺、释放边沿、`controlUseTile` 等旧控制位没有完整等价字段。 |
| `position`、`velocity`、`gravDir`（1389）、碰撞相关状态 | `TransformComponent`（`src/Terraria.Dome.Simulation/Components/TransformComponent.cs:3-12`）、`VelocityComponent`（`.../Components/VelocityComponent.cs:3-12`）、`PhysicsStateComponent.IsGrounded`（`.../Components/PhysicsStateComponent.cs:3-6`）；重力（`PlayerGravitySystem.cs:12-19`） | **verified / partial** | 位置、速度、接地和基础重力有系统 owner；完整 TileCollision、重力方向、平台穿越、液体/斜坡和坐标修正仍缺。 |
| `statLifeMax/statLife/statMana/statManaMax`（1357-1367）、`lifeRegen`/`manaRegen`（1369-1379） | `HealthComponent.Current/Maximum`（`src/Terraria.Dome.Simulation/Components/HealthComponent.cs:3-12`）；`ManaComponent` 当前值、上限、延迟和累积器（`src/Terraria.Dome.Simulation/Combat/Components/ManaComponent.cs:5-23`）；生命/法力快照（`PlayerStateSnapshot.cs:3-13`） | **verified / partial** | 核心生命/法力状态和再生时序字段已有明确 owner。完整装备修正、伤害防御、再生加成、药水/饰品来源尚未形成等价计算链。 |
| `armor[20]`、`dye[10]`、`miscEquips[5]`（1013-1019） | `EquipmentLoadoutComponent`（`src/Terraria.Dome.Simulation/Inventory/Components/EquipmentLoadoutComponent.cs:3-8`）；`EquipmentStateCollectionComponent.States/Revision`（`.../EquipmentStateCollectionComponent.cs:8-40`）；库存快照（`PlayerInventorySnapshot.cs:7-12`） | **partial / delegated** | 装备槽状态、选中 loadout、可见性和 revision 有容器 owner；`UpdateEquips`、套装效果、染料渲染和完整装备计算由装备域委托，不能称 Player 字段全量完成。 |
| `buffType[]`、`buffTime[]`、`buffImmune[]`（1029-1033） | `BuffCollectionComponent.Entries/Count/MaximumCount/Revision`（`src/Terraria.Dome.Simulation/StatusEffects/Components/BuffCollectionComponent.cs:6-27`，写入见 `:46-72`） | **partial / delegated** | Buff 实例容器、容量和 revision 已建模；完整免疫表、每 Buff 规则、视觉/召唤副作用及 `UpdateBuffs` parity 仍由状态效果域补齐。 |
| `inventory[59]`、`bank`-`bank4`（1083-1095）、`selectedItem`/`HeldItem`（属性 2960-2962） | `InventoryComponent` 40 槽、选中槽、实例状态和 revision（`src/Terraria.Dome.Simulation/Items/InventoryComponent.cs:6-19`，读写/消费 `:21-100`）；`SelectedItemComponent`（`.../Inventory/Components/SelectedItemComponent.cs:3-7`）；`PlayerInventorySnapshot`（`.../Snapshots/PlayerInventorySnapshot.cs:7-12`） | **verified / partial** | 服务器库存、选中槽、实例状态、合并/消费和快照已可验证。旧 59 槽、银行四组、Void Vault、完整 `HeldItem` 兼容形状及所有物品边界未完全覆盖。 |
| `itemAnimation`/`itemAnimationMax`/`itemTime`/`itemTimeMax`（2393-2401）、`ItemTimeIsZero`/`ItemAnimationJustStarted`（3155-3157） | `ItemUseStateComponent.CooldownTicks/IsUsing/UseRevision`（`src/Terraria.Dome.Simulation/Inventory/Components/ItemUseStateComponent.cs:3-8`）；物品使用（`PlayerItemUseSystem.cs:10-47`） | **partial** | 基础使用冷却、状态和生命恢复消费有 owner；旧动画帧、复用边沿、物品风格、弹药/魔力消耗和视觉持握行为仍缺。 |
| `chest`（2270）、`tileInteractAttempted`（1268）及交互状态 | `PlayerInteractionComponent`（`src/Terraria.Dome.Simulation/Player/Components/PlayerInteractionComponent.cs:14-35`）；服务器验证器和交互命令（行为地图“World interaction”族） | **partial / verified boundary** | 目标类型、目标位置和清理边界已是 typed interaction；完整旧箱子/门/标牌/传送分支需按各世界对象 verifier 分别确认，不能由组件存在推导全量 parity。 |
| 持久化生命、法力、装备、Buff、外观资料（旧字段分散于 505、1013-1095、1154、1947-1966 等） | `PlayerPersistentState`（`src/Terraria.Dome.Simulation/Players/PlayerPersistentState.cs:7-89`）；`PlayerPersistentProfile`（`.../PlayerPersistentProfile.cs:3-21`）；`PlayerPersistentItem`（`.../PlayerPersistentItem.cs:3-13`） | **verified / partial** | 生命/法力、990 项持久化槽、最多 44 Buff、loadout、饰品可见性、well-fed 计时和角色颜色/发型等均有验证构造。完整旧存档字段、银行语义、所有 mod/专用机制字段仍未对齐。 |
| 网络可见的 active、坐标、速度、朝向、生命、法力 | `NetworkPlayerSlice`（`src/Terraria.Dome.Protocol.V1456/Isolation/NetworkPlayerSlice.cs:5-29`）；`PlayerStateProjection.CreateFrames`（`src/Terraria.Dome.Server/Replication/PlayerStateProjection.cs:9-37`） | **verified / partial** | 快照到协议 slice 及 active/life-mana/control 帧已存在；控制帧当前使用默认 false/selected item 0，完整装备、Buff、区域、坐骑和专用协议包仍缺。 |
| `mount`（1552）、`wings/wingTime`（894-902）、`grappling[]`（2136）、钓鱼/高尔夫/表情字段（1344-1346 等） | 未发现对应的完整 Player ECS 组件或系统；行为地图将其列为 Specialized mechanics | **deferred / blocked** | 需要独立规则 oracle、确定性系统、协议和客户端适配器；不能通过在 Player 上增加同名字段解决。 |
| `VisualPosition`/`BaseHeight`、`Directions`、`CanBeTalkedTo`、`ShouldNotDraw`、相机/音频/UI/社交属性（2564-3159） | 不进入服务器模拟；行为地图 Presentation and social cosmetics = ClientOnly | **excluded** | 这些声明属于客户端表现或社交范围，迁移目标是客户端 adapter/compatibility，而不是 ECS simulation。 |

## 状态与证据规则

- **verified**：存在权威组件或快照，并有系统/协议/持久化路径能证明读写边界；不等于所有旧副作用已完成。
- **partial**：只覆盖字段族的一部分，或缺少完整规则、数组语义、默认值、边沿行为或协议分支。
- **delegated**：责任已明确交给装备、状态效果、持久化或兼容性域，但本报告不把域 owner 的存在当作 Player parity。
- **deferred / blocked**：专用规则或 adapter 尚未形成，必须保留后续任务边界。
- **excluded**：客户端专属表现/社交；不应为了提高服务器字段迁移率而伪造 ECS 状态。

## 可复核的迁移链

输入链为 `PlayerInput` 批次校验 -> `PlayerInputComponent` -> `PlayerControlSystem` 产生速度/朝向/意图；重力随后由 `PlayerGravitySystem` 更新速度。生命链为 `HealthComponent`/`ManaComponent` -> 死亡/再生/物品使用系统 -> `PlayerStateSnapshot` -> `NetworkPlayerSlice` 与 `PlayerStateProjection`。库存链为 `InventoryComponent` 的槽位/实例状态 -> `PlayerInventorySnapshot`；持久化链则通过 `PlayerPersistentState` 和 profile/item 记录导入恢复。

这些链路说明核心字段已有 ECS 形态，但不能推出装备计算、Buff 免疫、银行/Void Vault、坐骑/翅膀/绳索、完整旧碰撞以及客户端表现属性已经等价。下一步应以字段清单逐项补齐 `Definition -> Component -> System -> Snapshot -> Persistence/Protocol -> verifier` 六列，并继续把 ID 字段作为独立的寻址/协议契约维护，而不是混入本次非 ID 迁移率。

