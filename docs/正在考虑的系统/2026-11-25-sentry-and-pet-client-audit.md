# 参考客户端哨兵 / 宠物：机制事实与移植边界

| 项 | 值 |
| --- | --- |
| 文档 ID | `AUDIT-2026-11-25-SENTRY-PET-CLIENT` |
| 状态 | `open`（机制取证完成；未运行参考客户端，未做运行时一致性验证） |
| 触发 | 用户要求就参考客户端的鞭子 / 召唤物 / 哨兵 / 宠物四套机制展开讨论；鞭子与召唤物已另行成文 |
| 范围 | `D:\TRbackup\最新的客户端` 的哨兵与宠物实现，对照本仓库 `src/`（NSSLC ECS 基线） |
| 判据 | 「可移植」= 不依赖客户端绘制与输入即可复现同一状态转移；仅复制数据表不算 |
| 日期 | 2026-11-25（Asia/Shanghai） |
| 参考客户端 | 反编译快照，1579 个 `.cs` 文件 |

> 本报告只记录事实、差距与判断，**不含**实施计划。批次划分与验收口径需先由 §6 的决策确定。
>
> 姊妹篇：[鞭子 / 召唤物 / 哨兵 / 宠物 总审计](2026-11-25-whip-minion-sentry-pet-client-audit.md)（§3 鞭子、§4 标签系统、§5 召唤物、§8 网络同步事实）。本文对它 §6 / §7 只作摘要处引用，不重复展开。

---

## 0. 结论摘要

1. **宠物不是一套系统，是两种互不相干的实现共享了「宠物」这个词。** 旧宠物是投射物 + `Main.projPet[]` 布尔表（125 项）；新宠物（1.4.5 牵引小动物 / 风筝）是**独立的实体域**，有自己的静态存储、分区分组与 NetModule，根本不在 `Main.projectile` 里。
2. **牵引实体是四套机制里唯一按 ECS 式思路写的。** `LeashedEntity` + `Registry` + `SectionEntityList` + `NetModule` 的分层，几乎可以直接映射成「组件 — 原型注册表 — 空间分区 — 同步适配器」。
3. **哨兵是「有寿命 + 有上限 + 可回收」的消耗品**，与召唤物的常驻占槽模型正交。它的上限裁剪不是立即执行，而是每帧淘汰一个剩余时间最短者。
4. **牵引实体的网络模型与旧宠物完全相反**：旧宠物强制全区广播，牵引实体走**分区裁剪 + 每 1024 tick 部分同步**。
5. **两侧都有脏点**：哨兵的 `maxTurrets` 依赖 `maxTurretsOld != maxTurrets` 这种脏值比较来触发裁剪；牵引实体的 `HandleFullSync` 用 `throw new Exception` 表达协议违规，与项目错误处理约束冲突。

---

## 1. 证据基线与读取范围

| 项 | 内容 |
| --- | --- |
| 源码根 | `D:\TRbackup\最新的客户端` |
| 哨兵主文件 | `Terraria/Projectile.cs`（约 81k 行）、`Terraria/Player.cs`（约 55k 行）、`Terraria.ID/ProjectileID.cs` |
| 宠物（旧）主文件 | `Terraria/Main.cs`（`projPet` 表）、`Terraria/NetMessage.cs`、`Terraria/Player.cs` |
| 宠物（新）主文件 | `Terraria.GameContent/LeashedEntity.cs`（576 行）、`Terraria.GameContent.LeashedEntities/`（23 个文件）、`Terraria.GameContent.Tile_Entities/TE*Anchor.cs` |
| 读取方式 | grep 定位 + 分段读；未整文件通读 |
| 未读取 | 音效 / 粒子 / 绘制细节；`DrawWhip_*` 等纯表现分支 |

**术语澄清**：参考客户端里 `Pet` 一词覆盖两类实体，本文分别称 **旧宠物（投射物宠物）** 与 **牵引实体（LeashedEntity）**。

---

## 2. 哨兵（Sentry）

### 2.1 身份与归属

哨兵与召唤物共用 `minionDamage` 缩放，但**完全不进入召唤物的占槽分支**。

```csharp
// Projectile.cs:16810-16814 —— 伤害缩放共用
if (numUpdates == -1 && (minion || sentry))
{
    Player player2 = Main.player[owner];
    damage = (int)((float)originalDamage * player2.minionDamage + 5E-06f);
}
// Projectile.cs:16815 —— 占槽分支只要 minion，sentry 被排除
if (minion && numUpdates == -1 && type != 625 && type != 628) { ... }
```

| 项 | 位置 | 说明 |
| --- | --- | --- |
| `sentry` 字段 | `Projectile.cs:332` | `SetDefaults` 置 false；各类型分别置 true（`4008`、`4624`、`6928`、`6940`、`7152`、`7252`、`7339`、`7353`、`9152`、`9716`） |
| `SentryShot` | `ProjectileID.cs:293` | 14 项，哨兵打出的子弹 |
| `SentryLifeTime` | `Projectile.cs:302` | 常量 `36000`（10 分钟），典型初始化见 `4004` |
| `maxTurrets` | `Player.cs:2831` | 默认 1；加成点在 `10270`、`13105`、`13231`、`13245`、`13258`、`13271`、`13284(+2)`、`13298(+2)`、`13313(+2)`、`13328(+2)`、`13972`；`ResetEffects` 回 1（`18563`） |

与召唤物的三点差异：**不占 `numMinions` / `slotsMinions`**、**有 `timeLeft` 自然消亡**、**不跟随玩家**。

### 2.2 上限裁剪：每帧淘汰一个剩余时间最短者

```csharp
// Player.cs:53698-53726
public void UpdateMaxTurrets()
{
    if (Main.myPlayer != whoAmI) return;          // 只在本地玩家执行
    List<Projectile> list = new List<Projectile>();
    for (int i = 0; i < 1000; i++)
        if (Main.projectile[i].WipableTurret)     // 只收「可清除」的
            list.Add(Main.projectile[i]);
    int num = 0;
    while (list.Count > maxTurrets && ++num < 1000)
    {
        Projectile projectile = list[0];
        for (int j = 1; j < list.Count; j++)
            if (list[j].timeLeft < projectile.timeLeft)   // 淘汰剩余时间最短者
                projectile = list[j];
        projectile.Kill();
        list.Remove(projectile);
    }
}
```

三个约束：

1. **不是一次性清空**。每帧最多淘汰一个，`num < 1000` 防死循环；数量靠后续帧逐步收敛。
2. **淘汰优先级是「剩余时间最短」**，即最接近自然消亡的那个。
3. **谁进淘汰池由 `WipableTurret` 决定**（`Projectile.cs:530-540`）：

```csharp
public bool WipableTurret
{
    get
    {
        if (owner == Main.myPlayer && sentry)
            return !TurretShouldPersist();
        return false;
    }
}
```

`TurretShouldPersist()`（`Projectile.cs:672-692`）只在**撒旦军团持续事件**（`DD2Event.Ongoing`）且类型为 663/665/667/677–679/688–693 时返回 true。即：

- 撒旦军团期间放置的炮塔 → **不参与裁剪，可超出上限**
- 其余所有哨兵 → 参与裁剪

### 2.3 触发时机：脏值比较

```csharp
// Player.cs:25904-25908
if (maxTurretsOld != maxTurrets)
{
    UpdateMaxTurrets();
    maxTurretsOld = maxTurrets;
}
```

裁剪**只在 `maxTurrets` 变化的那一帧执行**。这意味着玩家在哨兵数量已达上限时再放一个，不会立刻触发 `UpdateMaxTurrets`——它依赖 `maxTurrets` 的旧值比较。参考实现里由其它路径兜底，但这是移植时必须显式处理的时序点。

### 2.4 落点：格中心吸附 + 向下探测

```csharp
// Player.cs:53652-53696（节选）
Vector2 pointPosition = Main.MouseWorld;
LimitPointToPlayerReachableArea(ref pointPosition);   // 先夹到可触及范围
int num = (int)pointPosition.X / 16;
int i   = (int)pointPosition.Y / 16;
worldX = num * 16 + 8;                                // X 吸附格中心
pushYUp = 41;
switch (checkProj) {                                  // 按类型微调
    case 665: pushYUp += 2; break;
    case 667: pushYUp += 3; break;
    case 691: case 692: case 693:
        pushYUp = 20; pushYUp += 2; break;            // 闪电光环更矮
}
// 向下扫到地面：左右相邻三列都必须可穿透
for (; i < Main.maxTilesY - 10
       && Main.tile[num, i] != null     && !WorldGen.SolidTile2(num, i)
       && Main.tile[num - 1, i] != null && !WorldGen.SolidTile2(num - 1, i)
       && Main.tile[num + 1, i] != null && !WorldGen.SolidTile2(num + 1, i); i++) { }
i--;
pushYUp -= 14;
worldY = i * 16;
```

- **X 吸附到格中心** `num * 16 + 8`，不是像素级自由放置
- **Y 向下扫**，条件是 `num` / `num-1` / `num+1` **三列同时**可穿透——防止哨兵卡墙或悬在单像素缝隙
- `pushYUp` 是向上补偿量，闪电光环 20、普通 41，最后统一 `-14`

### 2.5 重叠保护

```csharp
// Player.cs:53620-53650（节选）
Point value  = new Point(worldX,      worldY - 8);
Point value2 = new Point(worldX + 16, worldY - 8);
Point value3 = new Point(worldX - 16, worldY - 8);
for (int i = 0; i < 1000; i++) {
    if (projectile.active && projectile.sentry) {
        Rectangle hitbox = projectile.Hitbox;
        if (lightningAura) hitbox.Inflate(8, 0);
        else if (hitbox.X > 30) {
            hitbox.X += hitbox.Width / 2;  hitbox.Width = 30;
            hitbox.X -= hitbox.Width / 2;   // 宽塔临时收窄到 30
        }
        if (hitbox.Contains(value) || hitbox.Contains(value2) || hitbox.Contains(value3))
            return true;
    }
}
```

用**三个点**（中心、左 16、右 16）而非整矩形：等价于把待放位置当成一条 32 像素宽的横条。已存在的哨兵若宽度 > 30，**临时收窄到 30 再判定**，避免宽塔霸占过大禁放区。

调用链（`Player.cs:52720-52739`）：`IsADD2Turret` 且 `DD2Event.Ongoing` 且本地玩家时，先判重叠，再判下落点上方 2 格是否实心。

### 2.6 哨兵背包：回收再部署

```csharp
// Projectile.cs:80411-80441
public bool IsSentryBeingCarried() => ai[2] == 1f;

public bool CanGetPickedUpBySentryBackpack()
{
    if (!sentry) return false;
    if (owner != Main.myPlayer) return false;
    Player player = Main.player[owner];
    if (!player.accSentryBackpack || player.accSentryBackpackGrabCooldown > 0) return false;
    if (!base.Hitbox.Intersects(player.Hitbox)) return false;   // 必须碰到人
    return true;
}

public void SetAsSelectedSentryBackpackTarget()
{
    Main.player[owner].accSentryBackpackGrabCooldown = 2;       // 2 tick 冷却
}
```

冷却递减在 `Update_SentryBackpack()`（`Player.cs:28638-28644`），每帧减一。三个条件缺一不可：**自己放的 + 碰到人 + 冷却已过**。拾起后 `ai[2] = 1f`，`Main.cs:34114` 与 `Projectile.cs:31955` 走「携带中」分支（如重型吊索重定位 `AI_Sentries_HeavySlingReposition`）。

### 2.7 山羊哨兵背包暴击

```csharp
// Projectile.cs:12962-12965
if (ownedBySomeone && (sentry || ProjectileID.Sets.SentryShot[type])
    && Main.player[owner].accGolemSentryBackpack && Main.rand.Next(10) == 0)
{
    crit = true;
}
```

**10% 无条件暴击**，对哨兵本体与哨兵弹幕均生效。

---

## 3. 旧宠物（投射物宠物）

### 3.1 形态

旧宠物本体是 `Projectile`，靠布尔表标记，没有独立系统。

| 项 | 位置 | 说明 |
| --- | --- | --- |
| `Main.projPet` | 声明 `Main.cs:1301`（`new bool[ProjectileID.Count]`）；置 true 自 `Main.cs:8903` 起 | **125 项为 true**（`Main.cs` 内 `projPet[N] = true` 共 125 条） |
| `correctSlopeCollision` | `Projectile.cs:709` | `= Main.projPet[Type]` |
| 门 / 斜坡忽略 | `Projectile.cs:17589-17597` | 宠物忽略门，按玩家相对高度决定是否站上斜坡 |
| 「算不算宠物」排除表 | `Projectile.cs:12495` | 一长串排除项 |
| 强制全区广播 | `NetMessage.cs:1790-1794` | `main.projPet[type]` 命中时 `Array.Clear(netSyncSkippedForPlayer)`，不吃分区裁剪 |

### 3.2 召唤入口：逐个硬编码

`ItemCheck_ApplyPetBuffs`（`Player.cs:53728` 起）是一长串 `if (sItem.type == XXX) AddBuff(sItem.buffType, 3600)`，**按物品逐个写死，无统一规则**。

### 3.3 存在性判定混用两种风格

- 宠物 flag（`petFlag*` / `lightPetFlag*`，数量级为百）
- `ownedProjectileCounts`

例：buff 216 的雏鸟在 `ownedProjectileCounts[759] > 0` 时才算存在，否则在 `numMinions < maxMinions` 下补召唤（`Player.cs:10784-10814`）。

### 3.4 与召唤物的本质差异

**不占槽、不吃 `minionDamage`、无目标选择、不参与战斗。**

---

## 4. 牵引实体（新宠物 / 风筝）—— 参考客户端里最接近 ECS 的一套

### 4.1 目录与规模

```
Terraria.GameContent/LeashedEntity.cs                    576 行（基类 + NetModule + Registry + 分区容器）
Terraria.GameContent.LeashedEntities/                   23 个文件
  LeashedCritter.cs          308 行（小动物基类，状态机 + 网络字段）
  LeashedKite.cs             风筝（唯一继承 LeashedEntity 而非 LeashedCritter 的）
  WalkerLeashedCritter.cs    184 行（地面行走）
  ... 20 个具体行为类
Terraria.GameContent.Tile_Entities/
  TELeashedEntityAnchor.cs            52 行（锚基类）
  TELeashedEntityAnchorWithItem.cs    68 行（带物品的锚）
  TECritterAnchor.cs                 100 行（小动物锚，含原型映射表）
  TEKiteAnchor.cs
```

### 4.2 类层次

```
LeashedEntity                 (抽象基类：position/velocity/active/whoAmI/AnchorPosition)
├── LeashedCritter            (小动物基类：状态机 + npcType + rand + WaitTime + State)
│   ├── WalkerLeashedCritter      (State_Standing / PickDirection / Walking / Falling / Recalling)
│   ├── FlyerLeashedCritter
│   ├── AquaticWalkerLeashedCritter
│   ├── CrawlerLeashedCritter / SnailLeashedCritter / TurtleLeashedCritter
│   ├── RunnerLeashedCritter
│   ├── NormalButterflyLeashedCritter / EmpressButterflyLeashedCritter / HellButterflyLeashedCritter
│   ├── FireflyLeashedCritter / ShimmerFlyLeashedCritter / DragonflyLeashedCritter
│   ├── CrawlingFlyLeashedCritter / BirdLeashedCritter / WaterfowlLeashedCritter
│   ├── FishLeashedCritter / FairyLeashedCritter / JumperLeashedCritter
│   └── WaterStriderLeashedCritter
└── LeashedKite               (风筝：有自己的物理、风场、拖尾历史)
```

### 4.3 存储结构：静态分区表 + 双重索引

```csharp
// LeashedEntity.cs:273-277
private static readonly SectionEntityList[,] BySection;      // [x, y] 二维分区
private static readonly List<SectionEntityList> ActiveSectionList;
private static readonly List<LeashedEntity> ByWhoAmI;         // 全局槽表，空位复用

// LeashedEntity.cs:330
BySection = new SectionEntityList[Main.maxTilesX / 200 + 1, Main.maxTilesY / 150 + 1];
```

分区大小 **200×150 格**，与 `Netplay.GetSectionX/Y` 一致。每个 `SectionEntityList`：

- `LeashedEntity[] list`，初始 32，满则翻倍
- `Add` 写入 `e.sectionSlot = count`，`Remove` 置 null 并记 `emptySlots`
- `CompactIfNecesary()`：空位过半时压实并重排 `sectionSlot`

**这是典型的「分区容器 + 就地压实」写法，可以直接映射成 ECS 的稀疏集或分桶存储。**

### 4.4 激活生命周期

| 阶段 | 方法 | 行为 |
| --- | --- | --- |
| 分区激活 | `SectionEntityList.Activate()`（`221-240`） | `active = true`；非纯客户端时对分区内所有 `active` 实体调 `Spawn(newlyAdded: false)`；加入 `ActiveSectionList` |
| 分区停用 | `Deactivate()`（`242-257`） | 纯客户端直接返回；否则对分区内实体调 `Despawn()` |
| 订阅 | `LeashedEntity` 静态构造（`328-338`） | `ActiveSections.SectionActivated` → `GetSection(...).Activate()`；`RemoteClient.NetSectionActivated` → `SyncEntitiesInSection` |
| 每帧 | `UpdateEntities()`（`445-451`） | `RecheckActiveSections()` → `_UpdateEntities()`，结果进 `TimeLogger.UpdateLeashedEntities` |

`_UpdateEntities`（`473-496`）只遍历**活跃分区**：对每个 `active` 实体调 `Update()` 与 `StreamNetUpdates()`；若 `Update()` 把 `active` 置 false，随即 `Remove(entity)`。

### 4.5 网络模型：分帧部分同步 + 分区裁剪

```csharp
// LeashedEntity.cs:498-504
private void StreamNetUpdates()
{
    if (Main.netMode == 2 && ((Main.GameUpdateCount + whoAmI) & 0x3FF) == 0L)
    {
        NetModule.Sync(this, full: false);
    }
}
```

- **每 1024 tick（`0x3FF`）触发一次部分同步**，用 `whoAmI` 做错峰，避免所有实体同帧发包
- `NetModule.Sync`（`56-74`）广播时按 `Netplay.Clients[i].IsSectionActive(entity.SectionCoordinates)` **只发给能看到该分区的客户端**
- 三条消息类型：`Remove=0` / `FullSync=1` / `PartialSync=2`

**与旧宠物的强制全区广播（`NetMessage.cs:1790`）正好相反。**

### 4.6 同步载荷

```csharp
// LeashedCritter.cs:63-77
public override void NetSend(BinaryWriter writer, bool full)
{
    if (full)
    {
        writer.Write7BitEncodedInt(npcType);
        writer.WriteVector2(base.Size);
    }
    writer.WritePackedVector2(position - base.AnchorPosition.ToWorldCoordinates());  // 相对锚点的偏移
    writer.Write(direction > 0);
    writer.Write(rand.state);          // LCG 状态也同步
    writer.Write(WaitTime);
    writer.Write(State);
    writer.Write((sbyte)(TargetPosition.X - base.AnchorPosition.X));
    writer.Write((sbyte)(TargetPosition.Y - base.AnchorPosition.Y));
}
```

三个值得注意的设计：

1. **位置发的是相对锚点的偏移量**，不是世界坐标——锚点不动时精度更高、也更省。
2. **随机数生成器的状态 `rand.state` 一起同步**。这是为了让客户端能复现同一套 `LCG32Random` 序列，属于「确定性重建」思路。
3. **`TargetPosition` 只发 `sbyte` 相对偏移**（±127 格），与 `strayingRangeInBlocksX/Y = 3` 的取值范围匹配。

`NetReceive`（`79-105`）在非 full 时记录 `netOffset += vector - position`，随后每帧 `netOffset = netOffset.MoveTowards(Vector2.Zero, 2f)`（`114-117`）——**平滑插值**，避免瞬移。

### 4.7 锚点绑定

```csharp
// TELeashedEntityAnchor.cs:44-49
protected void RespawnLeashedEntity()
{
    DespawnLeashedEntity();
    leashedEntity = CreateLeashedEntity();
    LeashedEntity.AddNewEntity(leashedEntity, Position);
}
```

生命周期由**方块实体**驱动：

| 事件 | 行为 |
| --- | --- |
| `OnWorldLoaded()`（`31-34`） | `RespawnLeashedEntity()` |
| `OnRemoved()`（`15-18`） | `DespawnLeashedEntity()`（置 `active = false`，下一次 `Update` 时被 `Remove`） |
| `InsertItem()`（`WithItem:32-36`） | 换物品后重生成 |

方块类型：`723`（风筝锚）/ `724`（小动物锚），见 `TECritterAnchor.IsTileValidForEntity`（`TECritterAnchor.cs:23-31`，返回 `tile.type == 724`）与 `WorldGen.cs:64019`。

### 4.8 行为原型注册表：`NPCID → 原型实例`

```csharp
// TECritterAnchor.cs:64-86（节选）
static TECritterAnchor()
{
    CritterPrototypes = NPCID.Sets.Factory.CreateCustomSet((LeashedCritter)WalkerLeashedCritter.Prototype, new object[0]);
    SetPrototypeCollection(FlyerLeashedCritter.Prototype, 444, 653, 661);
    SetPrototypeCollection(NormalButterflyLeashedCritter.Prototype, 356);
    SetPrototypeCollection(EmpressButterflyLeashedCritter.Prototype, 661);
    SetPrototypeCollection(DragonflyLeashedCritter.Prototype, 595, 596, 601, 597, 598, 599, 600);
    SetPrototypeCollection(AquaticWalkerLeashedCritter.Prototype, 361, 445, 148, 149);
    SetPrototypeCollection(BirdLeashedCritter.Prototype, 74, 297, 298, 442, 611, 671, 672, 673, 675, 674);
    ...
}

// TECritterAnchor.cs:96-99
public static LeashedCritter GetLeashedCritterPrototype(int itemType)
    => CritterPrototypes[ContentSamples.ItemsByType[itemType].makeNPC];
```

**物品 → `makeNPC` → `NPCID` → 原型实例 → `NewInstance()`**，是一条完整的内容驱动链。同类于 `Registry.RegisterAll()`（`LeashedEntity.cs:118-142`）为每个行为类型分配 `Type` 序号。

### 4.9 即时行为：`WalkerLeashedCritter.AdvanceTargetPosition()`

```csharp
// WalkerLeashedCritter.cs:29-67
protected bool AdvanceTargetPosition()
{
    if (Math.Abs(TargetPosition.X - base.AnchorPosition.X) >= strayingRangeInBlocksX)
        direction = Math.Sign(base.AnchorPosition.X - TargetPosition.X);   // 超出漫游范围就折返
    if (!WorldGen.InWorld(TargetPosition.X + direction, TargetPosition.Y))
        direction *= -1;                                                   // 撞世界边界
    spriteDirection = direction;
    int num = TargetPosition.X + direction;
    short y = TargetPosition.Y;
    bool num2 = !WorldGen.SolidTileAllowTopSlope(num, y - 1);
    bool flag  = !WorldGen.SolidTileNoPlatforms(num, y);
    bool flag2 = !WorldGen.SolidTileAllowBottomSlope(num, y + 1);
    bool flag3 = WorldGen.AnyLiquidAt(num, y + 1);
    bool flag4 = !WorldGen.SolidTileAllowBottomSlope(num, y + 2);
    bool flag5 = num2 && !flag;                       // 前方上方可走 → 上台阶
    bool flag6 = (flag & flag2) && !flag3 && !flag4;  // 前方有地且下一格也有地、无液体 → 下台阶
    bool flag7 = flag && !flag2;                      // 前方悬空 → 掉落
    ...
}
```

**纯函数式的格坐标决策**（输入是周围方块状态，输出是下一个目标格），是所有行为类里最容易离线单测的一段。

---

## 5. 两套宠物的横向对照

| 维度 | 旧宠物（`Main.projPet`） | 牵引实体（`LeashedEntity`） |
| --- | --- | --- |
| 载体 | `Projectile` | 独立实体域 |
| 存储 | `Main.projectile[]` 共享 1000 槽 | `BySection` + `ByWhoAmI` 双索引 |
| 上限 | 受投射物总数约束 | 每分区独立、可扩容 |
| 存在性 | `petFlag*` / `ownedProjectileCounts` 混用 | 锚点方块实体 |

| 同步 | 强制全区广播（`NetMessage.cs:1790` `Array.Clear`） | 分区裁剪 + 每 1024 tick 部分同步 |
| 位置表示 | 绝对世界坐标 | **相对锚点偏移** |
| 确定性 | 无随机状态同步 | **`rand.state` 随包同步** |
| 绑定方式 | buff 挂在玩家身上 | 方块实体绑定，破坏即消失 |
| 战斗参与 | 无 | 无 |

一句话：**旧宠物是「玩家身上的 buff 投射物」，牵引实体是「方块上的可寻址实体」。**

---

## 6. 对 NLTX 的设计含义（待决策）

以下不是实施计划，是**必须先定的判断点**。

### 6.1 牵引实体是四套机制里最适合先做的

理由：

1. 结构已经接近 ECS（分区容器 + 原型注册表 + 独立同步适配器），映射成本最低
2. `AdvanceTargetPosition()` 是纯函数，可离线单测，不依赖网络与绘制
3. 它与现有 `Projectile` 系统完全解耦，接入不会污染投射物链路

### 6.2 哨兵的三个必须先定的时序点

1. **裁剪触发条件**：参考实现靠 `maxTurretsOld != maxTurrets` 脏值比较。ECS 里应改成「上限变化事件」还是「每帧查询超额」？前者精确但需要事件源，后者简单但有 1 帧延迟。
2. **「剩余时间最短优先淘汰」的策略归属**：属于哨兵域自己的策略，还是通用「上限裁剪」策略？如果将来有第二类有上限的实体，值得抽成策略接口。
3. **`TurretShouldPersist` 的事件耦合**：当前依赖 `DD2Event.Ongoing` 这个全局事件状态。ECS 里应表达成「裁剪豁免标签」由事件系统授予，而不是让哨兵直接读事件单例。

### 6.3 牵引实体的网络字段与项目约束的冲突

参考实现同步了 **`rand.state`（LCG 随机数状态）**。这在原版是合理的（客户端要复现同一随机序列来画同一个动作），但在 NLTX 的「服务端权威」模型下需要先判定：

- 若采用确定性重建 → 同步 `rand.state` 是对的，客户端按同样序列重算
- 若采用状态同步 → 客户端不需要 `rand`，只收结果位置，可以省掉这个字段但会浮点漂移

**这与鞭子面临的判断是同一个**（见姊妹篇 §10.1），建议两处采用同一口径。

### 6.4 一处违反项目错误处理约束的写法

```csharp
// LeashedEntity.cs:96-99 / 106-109
throw new Exception(string.Concat("LeashedEntity type mismatch for full sync. ..."));
throw new Exception("LeashedEntity type mismatch for full sync. ...");
```

用裸 `Exception` 表达协议违规。按 [错误处理约束](../../Context/项目约束/错误处理约束.md)，协议层失败应先分类再选机制，不能以技术异常跨越层边界。移植时须改成封闭失败原因枚举或结果类型。

### 6.5 旧宠物不要照搬

125 项布尔表 + 逐个物品硬编码的 `if` 链，是原版历史包袱。若是复刻需要它们，建议：

- 数据表（`projPet` / `makeNPC` 映射）可以照抄
- **控制流不要照抄**，改成一表驱动的「宠物装配」查询

---

## 7. 局限与未验证项

1. **未运行参考客户端**，所有结论均来自静态读码。
2. `Npc` 帧动画与 `Dummy` 复制逻辑（`CopyToDummy` / `CopyFromDummy`）只读到签名，未验证帧序列正确性。
3. `LeashedKite` 的风场物理（`windTarget` / `windCurrent` / `timeWithoutWind`）未展开，只确认字段存在。
4. 哨兵的 14 种具体 AI 行为未逐个读，仅覆盖了放置 / 裁剪 / 回收 / 暴击四条横切逻辑。
5. 未核对 NLTX 侧现有代码对上述机制的实际覆盖度——本文 §6 的判断基于总审计的 NLTX 现状对照，未重新取证。

---

## 8. 附：本报告涉及的关键位置索引

| 主题 | 位置 |
| --- | --- |
| 哨兵伤害缩放共用 | `Projectile.cs:16810` |
| 哨兵排除出占槽 | `Projectile.cs:16815` |
| `WipableTurret` | `Projectile.cs:530-540` |
| `TurretShouldPersist` | `Projectile.cs:672-692` |
| `SentryLifeTime = 36000` | `Projectile.cs:302` |
| 上限裁剪 | `Player.cs:53698-53726` |
| 裁剪触发（脏值） | `Player.cs:25904-25908` |
| 落点探测 | `Player.cs:53652-53696` |
| 重叠保护 | `Player.cs:53620-53650` |
| 放置前校验 | `Player.cs:52720-52739` |
| 背包抓取 | `Projectile.cs:80411-80441` |
| 背包冷却更新 | `Player.cs:28638-28644` |
| 山羊暴击 | `Projectile.cs:12962-12965` |
| `projPet` 表 | 声明 `Main.cs:1301`；填充 `Main.cs:8903` 起，125 项 |
| 宠物强制广播 | `NetMessage.cs:1787-1794` |
| 宠物召唤硬编码 | `Player.cs:53728` 起 |
| 牵引实体存储 | `LeashedEntity.cs:273-277`、`330` |
| 分区激活 | `LeashedEntity.cs:221-257` |
| 每帧更新 | `LeashedEntity.cs:445-496` |
| 分帧同步 | `LeashedEntity.cs:498-504` |
| 同步载荷 | `LeashedCritter.cs:63-105` |
| 原型注册 | `TECritterAnchor.cs:64-99` |
| 锚点生命周期 | `TELeashedEntityAnchor.cs:31-49` |
| 行走决策 | `WalkerLeashedCritter.cs:29-67` |
