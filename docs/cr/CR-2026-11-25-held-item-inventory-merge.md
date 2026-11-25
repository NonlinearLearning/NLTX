# CR-2026-11-25 手持概念并入 Inventory

## 1. 背景

用户要求把"手持"相关代码合并进 Inventory，用 Inventory 表达 Handheld 概念，并对照
`D:\TRbackup\最新的客户端` 的原始实现。

对齐源码结论：

| 出处 | 内容 |
| --- | --- |
| `最新的客户端/Terraria/Player.cs:3835` | `public int selectedItem => selectedItemState.Selected;` |
| `最新的客户端/Terraria/Player.cs:3837` | `public Item HeldItem => inventory[selectedItem];` |
| `最新的客户端/Terraria/Player.cs:42925` | `ItemCheck` 第一行 `Item item = inventory[selectedItem];` |
| `最新的客户端/Terraria/MessageBuffer.cs:996` | 包 13 `player13.selectedItemState.Select(reader.ReadByte());` |

即：手持 = "库存的一个选中索引 + 对该槽的派生读取"。不存在独立的物品容器，也不存在
第二份载荷。本仓库此前把选中槽当作 Player 域内部事务：包 13 的该字节被显式丢弃
（`_ = ReadByte(...) // Selected inventory slot remains Player-owned state.`），物品使用系统
接受任意槽号，`PlayerItemUseStateComponent.Slot` 自行记录"上次用的槽"。

## 2. 变更

### 2.1 领域（Share/Inventory）

| 文件 | 变更 |
| --- | --- |
| `System/HeldSlot.cs` | 新增。只携带槽索引；"未手持"用 `HeldSlot?` 的 `null` 表达，不接受负下标 |
| `Component/InventoryComponent.cs` | 新增 `Held`（`HeldSlot?`）、`TryGetHeld()`、`HasHeldItem`、内部 `CommitHeld` |
| `InventorySystem.cs` | 新增 `TrySetHeldSlot`（校验槽存在，否则 `InvalidSlot`）与 `TryGetHeldStack` |

手持载荷不新增存储：`TryGetHeld` 读的就是 `Slots[i].Content`。

### 2.2 使用（Share/ItemUse）

- `PlayerItemUseSystem.Begin` 先校验请求槽 == 当前手持槽，再读取该槽内容；不匹配即
  `PlayerItemUseOutcomeKind.NotHeld`（新枚举值 7，append-only）。
- 弹药查找、门闸、通道语义未改。

### 2.3 输入（Player / Network）

- `PlayerInputRequestKind.SelectHeldSlot`；`PlayerInputRequestSystem` 转调
  `InventorySystem.TrySetHeldSlot`，负数槽号表示清除。
- `GameCommunicationAdapter.ApplyPlayerControls` 不再丢弃包 13 的选中槽字节，改为提交上述意图。

### 2.4 Legacy 视图

`src/NSSLC.Infrastructure/WorldGeneration/Runtime/WorldEntities.cs` 的 `Player.HeldItem`
由独立字段 `Item HeldItem = new Item()` 改为 `=> inventory[selectedItem]`，与参考实现同形。

## 3. 顺带修复

`ProjectileItemUseSpawnSystem` 仍按旧签名构造 `ProjectileSpawnRequest`（`collider`/`size` 参数），
该签名已由「碰撞体与尺寸来自定义」的重构移除，Terraria.Components 因此编译失败。
已改为只传实例级输入。

## 4. 验证

| 项目 | 结果 |
| --- | --- |
| `dotnet build src/NSSLC/Terraria.Components.csproj` | 0 警告 0 错误 → `Build/bin/Terraria.Components/Debug/net10.0` |
| `Terraria.Inventory` | PASS（含新增 `held-*` 7 条） |
| `Terraria.ItemUse` | PASS（含 `use-without-held-slot-rejected`、`hold-slot-out-of-range-rejected`） |
| `Terraria.WorldItem`、`Terraria.Attributes`、`Terraria.Chat`、`Terraria.Combat.ProtectionWindow`、`Terraria.Entity.Scheduling`、`Terraria.Entity.Definition`、`Terraria.EntitySpawn`、`Terraria.Npc.Ai`、`Terraria.TileEntity` | PASS |
| `dotnet build src/NSSLC.Server/NSSLC.Server.csproj` | 0 错误（28 条既有警告） |
| `dotnet build src/NSSLC.Infrastructure/WorldGeneration/NSSLC.WorldGeneration.csproj` | 0 错误（19 条既有警告） |

`Terraria.Projectile` 现为 PASS（生成侧几何已改为来自定义）。期间该目录另有一次外部在途改动
（`Share/Emission/**` 与 `EmissionDrainSystem`）造成短暂编译失败，待其落盘后构建恢复
0 错误；该改动不属于本轮，未参与修改。

## 5. 未闭合

- 无独立 `HandsComponent`：物品实体路径已于 11-23 删除，重建手部关系会让同一 `ItemStack`
  同时存在于槽与手两处，违反双宿主约束 INV-1/INV-2。
- 客户端表现层（`PlayerHeldItemPresentationStateComponent` 一类）仍未接入；服务端只保证关系与
  使用门闸一致。
