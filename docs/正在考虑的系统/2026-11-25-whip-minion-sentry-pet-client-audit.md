# 参考客户端鞭子 / 召唤物 / 哨兵 / 宠物：机制事实与同步边界

| 项 | 值 |
| --- | --- |
| 文档 ID | `AUDIT-2026-11-25-WHIP-MINION-SENTRY-PET-CLIENT` |
| 状态 | `closed`（取证完成；未运行参考客户端，未做运行时一致性验证） |
| 触发 | 用户判断「服务器会同步鞭子的运动轨迹」；需确认线上实际传输内容与权威归属 |
| 范围 | `D:\TRbackup\最新的客户端` 的鞭子 / 召唤物 / 哨兵 / 宠物实现，对照本仓库 `src/`（NSSLC ECS 基线） |
| 判据 | 「同步了轨迹」= 轨迹控制点或其等价序列出现在协议载荷中；仅传输生成参数不算 |
| 日期 | 2026-11-25（Asia/Shanghai） |
| 参考客户端 | 反编译快照，1579 个 `.cs` 文件 |

## 1. 问题与结论速览

调查起点是一个判断上的分歧：**服务器是否会同步鞭子的运动轨迹**。读码结论是：

1. **服务器不发轨迹。** 鞭子投射物（`aiStyle == 165`）在整个 AI 生命周期内不产生任何网络写操作——`AI_165_Whip`（`Projectile.cs:48482-49090`，约 600 行）中唯一的网络相邻语句是 `Kill()`（`48497`），没有 `netUpdate`、没有 `SendData`。
2. **线上只发一次出生包。** 鞭子出生时走 `Projectile.NewProjectile` 收尾的 `SendData(27)`（`Projectile.cs:11098`），载荷为「位置 + 速度 + 类型 + `ai[0..2]` + 伤害族」，此后**不再逐帧同步**。
3. **曲线是本地重算的。** 每台机器各自 `ai[0]++` 并调用 `FillWhipControlPoints`（`Projectile.cs:49098`）现算 20~45 个控制点。控制点从不传输。
4. **但有三个决定曲线形状的输入不在包内**：`itemAnimationMax`（决定 `timeToFlyOut`）、`whipRangeMultiplier`、`HeldItem.useAnimation`。三者都靠「玩家状态已另行同步」这个前提隐式成立。
5. **服务端不判定鞭子命中。** `Damage()` 的 PVE/PVP 分支被 `owner == Main.myPlayer` 挡住（`Projectile.cs:12520`），而专用服 `Main.myPlayer == 255`（`Netplay.cs:250`）。原版鞭子命中是**客户端权威**，服务端只转发结果。

对 NLTX 的直接含义：鞭子应当按「**确定性重建**」建模，而不是「状态同步」；真正的风险点不是带宽，而是第 4 条那三个隐式输入在不同机器上算出不同值。

---

## 2. 证据基线与读取范围

| 项 | 内容 |
| --- | --- |
| 源码根 | `D:\TRbackup\最新的客户端` |
| 规模 | 1579 个 `.cs` 文件；核心文件 `Terraria\Projectile.cs`（约 81k 行）、`Player.cs`（约 55k 行）、`NPC.cs`、`Item.cs`、`Main.cs` |
| 读取方式 | grep 定位 + 分段读；未整文件通读 |
| 未读取 | 音效/粒子/绘制层的完整实现；`Terraria.GameContent.*` 下与主题无关的命名空间 |

**路径修正记录**：调查发起时给出的路径 `D:\TRbackup\最新的客户端鞭子\召唤物\哨兵\宠物` 在磁盘上不存在（`D:\TRbackup\最新的客户端鞭子` 一级即不存在，全盘也无含「鞭子」的目录）。本文读取的是 `D:\TRbackup\最新的客户端`。若原意指向其他目录，本文结论需重新取证。

---

## 3. 鞭子（Whip）

### 3.1 身份与实现分布

鞭子由一张按投射物 id 下标的布尔表定义，共 **18 种**：

```csharp
// Terraria.ID\ProjectileID.cs:35
public static bool[] IsAWhip = Factory.CreateBoolSet(false, 847, 841, 848, 849, 912,
    913, 914, 915, 952, 1028, 1029, 1030, 1031, 1032, 1033, 1034, 1035, 1104);
```

对应 **18 种鞭子物品**，其在 `ItemCheck_Shoot` 中走同一条专用分支（`Player.cs:49144`）：

```
5688, 4672, 5473, 5474, 5475, 5476, 5477, 5478, 5479, 5480,
5074, 4911, 4912, 4913, 4914, 4678, 4679, 4680
```

实现分布：

| 位置 | 职责 |
| --- | --- |
| `Projectile.cs:48482` | `AI_165_Whip`：单 tick 推进 `ai[0]`、设置 `Center`、处理多段鞭与音效 |
| `Projectile.cs:49098` | `FillWhipControlPoints`：由参数生成控制点序列 |
| `Projectile.cs:49162` | `GetWhipSettings`：按类型给出 `timeToFlyOut` / `segments` / `rangeMultiplier` |
| `Player.cs:49144` | 生成入口：编码 `ai[1]`（摆动）与 `ai[2]`（段/时长覆盖） |
| `Terraria.GameContent.Items\` | 标签系统：`TagEffectStack`、`TagEffectState`、`WhipTagEffect` + 6 个具体子类 |

### 3.2 几何：一条由 `ai[0]` 参数化的曲线

`AI_165_Whip` 每 tick 只做三件事：

```csharp
// Projectile.cs:48489-48498
rotation = velocity.ToRotation() + (float)Math.PI / 2f;
ai[0]++;
GetWhipSettings(this, out var timeToFlyOut, out var _, out var _);
base.Center = player.GetArmPosition() + velocity * (ai[0] - 1f);
spriteDirection = ((!(Vector2.Dot(velocity, Vector2.UnitX) < 0f)) ? 1 : (-1));
if (ai[0] >= timeToFlyOut) { Kill(); return; }
```

`FillWhipControlPoints`（`49098`）是纯函数式重建，输入只有六个：

| 输入 | 来源 | 作用 |
| --- | --- | --- |
| `ai[0]` | 本地自增 | 展开进度 `num = (ai[0] + frameOffset) / timeToFlyOut`（`49105`） |
| `ai[1]` | 出生包 / 本地 | 摆动幅度系数 `num13`（`49132`，仅 `getActualCollisionPoints: false` 时生效） |
| `ai[2]` | 出生包 / 本地 | `>= 1000` 时覆盖 `timeToFlyOut`（`49169-49172`） |
| `velocity` | 出生包 | 决定 `rotation` 与 `spriteDirection` |
| `owner.GetArmPosition()` | 玩家同步状态 | 曲线起点（`51202`，依赖 `bodyFrame`、`mount`、`direction`、`gravDir`） |
| `owner.HeldItem.useAnimation` × `owner.whipRangeMultiplier` | 物品 + 属性 | 曲线长度（`49121`） |

关键的中段结构（`49110-49119`）：前 2/3 甩出、后 1/3 回抽。

```csharp
float num2 = 0.5f;
float num3 = 1f + num2;          // 1.5
float num4 = PI * 10f * (1f - num * num3) * (-spriteDirection) / segments;
float num5 = num * num3;
if (num5 > 1f) { num6 = (num5 - 1f) / num2; num5 = MathHelper.Lerp(1f, 0f, num6); }
```

类型特化全部集中在 `GetWhipSettings` 的 switch（`49175-49243`）：默认 `segments = 20`、`rangeMultiplier = 1`；长鞭覆写为 12/30/36/40/45 段与 0.75~2.2 倍范围。`type == 1035` 且 `ai[2] >= 10` 时把 `timeToFlyOut` 固定为 `30 * MaxUpdates`（`49238-49241`）。

### 3.3 命中判定：前后帧控制点并集

鞭子不是碰撞体推进，而是**扫掠矩形求交**（`Projectile.cs:15803-15824`）：

```csharp
if (ProjectileID.Sets.IsAWhip[type]) {
    WhipPointsForCollision.Clear();
    FillWhipControlPoints(this, WhipPointsForCollision, null, getActualCollisionPoints: true);
    int count = WhipPointsForCollision.Count;
    FillWhipControlPoints(this, WhipPointsForCollision, null, getActualCollisionPoints: true, -1);
    for (int num31 = 0; num31 < count; num31++) {
        Point point  = WhipPointsForCollision[num31].ToPoint();
        Point point2 = WhipPointsForCollision[num31 + count].ToPoint();
        myRect.Location = new Point(point.X - myRect.Width / 2, point.Y - myRect.Height / 2);
        Rectangle value = myRect;
        value.Location = new Point(point2.X - myRect.Width / 2, point2.Y - myRect.Height / 2);
        Rectangle attacker3 = Rectangle.Union(myRect, value);
        if (attacker3.Intersects(targetRect)) return true;
    }
    return false;
}
```

即「本帧第 i 点」与「上帧第 i 点」的并集矩形，逐点求交。`getActualCollisionPoints: true` 会强制 `ai[1] = 1f`（`49133-49136`），使碰撞几何与视觉摆动解耦。

切块判定同源（`15293-15303`），`UpdateEnchantmentVisuals` 只取曲线末端点（`17414-17420`）。

### 3.4 生成与参数编码

代表分支（`Player.cs:49144-49182`）：

```csharp
float num164 = 0.4f;
float num165 = 0.6f + num164 * Main.rand.NextFloat();   // ai[1]：0.6~1.0 的摆动
if (sItem.type != 4680 && Main.rand.Next(3) == 0) num165 *= -2.5f;
float ai6 = 0f;
if (!withAudioVisualFeedback) {                          // 隐身/连挥的第二段
    Vector2 v = new Vector2(num4, num5).RotatedByRandom(PI / 10f);
    num4 = v.X; num5 = v.Y;
    ai6 = 1000f + (float)itemAnimationMax * 2f;          // ai[2]：时长覆盖
    Damage = Math.Max(3, (int)(Damage * 0.3f));
    KnockBack *= 0.25f;
}
Projectile.NewProjectile(..., num4, num5, projToShoot, Damage, KnockBack, i, 0f, num165 * num166, ai6);
if (withAudioVisualFeedback) { repeatWhipSwings++; repeatWhipsResetCooldown = 120; }
```

`ai[2]` 因此是**双义**的：`0` = 普通鞭（时长由 `itemAnimationMax` 决定）；`>= 1000` = 自带时长 `ai[2] - 1000` 的「回声鞭」（降伤 30%、降击退 75%）；`1035` 还用 `10..` 区间表示段号（`48501-48529`）。

另外两条与鞭子相关的玩家侧逻辑：
- `maxTagEffects > 1` 时，回声鞭从**背包中下一把鞭子**取物品（`43042-43052`）；
- 蛇饰带（`accSnakeBand`）在 `itemAnimation == itemAnimationMax * 0.5` 且 `repeatWhipSwings % 3 == 0` 时补一次挥击（`43035-43056`）。

### 3.5 属性来源

`Player.cs` 中只有 8 处鞭子相关：

| 行 | 内容 |
| --- | --- |
| `1123` / `1125` | `whipRangeMultiplier` / `whipUseTimeMultiplier` 字段 |
| `4461` | `SetItemAnimation(useAnimation, summonerWeaponSpeedBonus * whipUseTimeMultiplier)` |
| `13077` | 莫比乌斯（`accMobiusStrip`）：`whipRangeMultiplier += 0.1f` |
| `13953` / `13978` | 护甲（`1159`、`3382/3383`）：`+= 0.1f` / `+= 0.15f` |
| `18612` / `18613` | `ResetEffects` 中重置为 `1f` |

注意：`ApplyItemAnimation` 走的是 `sItem.summon && ItemID.Sets.SummonerWeaponThatScalesWithAttackSpeed` 分支，**不是** `melee`，所以鞭子不吃近战速度加成。

---

## 4. 鞭子标签系统（TagEffectStack）

四套机制里设计最现代、也最适合直接进 ECS 的部分。它不是「挂在 NPC 上的 buff」，而是**玩家 × NPC 的二维稀疏计时表**。

### 4.1 结构

```
TagEffectStack（每玩家一个，MaxEffects = 5）        —— TagEffectStack.cs:8
  └─ TagEffectState[]（按使用顺序排序，最近使用排 0）  —— TagEffectStack.cs:42-58
       ├─ UniqueTagEffect _effect                      —— 一个鞭子一个子类
       ├─ int[] TimeLeftOnNPC[Main.maxNPCs]            —— 标记剩余 tick
       └─ int[] ProcTimeLeftOnNPC[Main.maxNPCs]        —— 一次性触发剩余 tick
```

- `_activeEffectCount` 上限由 `_owner.maxTagEffects` 控制，`MaxEffects = 5` 为硬上限（`TagEffectStack.cs:33-40`）。
- 每次 `TrySetActiveEffect` 会把命中的效果**冒泡到索引 0**（`TagEffectStack.cs:54-57`），所以「最近使用的鞭子」永远是第 0 位。
- `Update` 每 tick 对 200 个 NPC 槽逐个递减（`TagEffectState.cs:73-89`）——这是 `5 × 200` 的固定遍历成本，与原版其他稀疏状态风格不同。

### 4.2 生产与消费

| 环节 | 位置 |
| --- | --- |
| 鞭子命中 → 打标 | `Projectile.cs:13635-13638` → `TagEffectStack.TryApplyTagToNPC(tagEffectType, targetNPC)` |
| 免疫判定 | `WhipTagEffect.cs:24-35`：`NPCID.Sets.DebuffImmunitySets[npcType].ImmuneToWhips` |
| 召唤物命中 → 加伤 | `Projectile.cs:12947-12961` → `TagEffectStack.ModifyHit` → `TagDamageChanges` |
| 结算公式 | `num15 += (int)((num18 + bonusTagDamage) * SummonTagDamageMultiplier[type])`（`12954`） |
| 命中后效果 | `Projectile.cs:14003-14013`：`CanProcOnNPC` → `OnHit` → `RemoveAllProcs` |

`tagEffectType` / `bonusTagDamage` 是 `Projectile` 的字段（`472` / `474`），在生成时从物品复制（`11127-11129`）。

### 4.3 多鞭叠加规则

`TagDamageChanges`（`Terraria.GameContent.Items\TagDamageChanges.cs`）的语义：

```csharp
public struct TagDamageChanges {
    public bool AddProjectileTagDamage;
    public int AddedBaseDamage;
    public int AddedFlatDamage;
    public float TotalDamageMultiplier;
    public bool? Crit;
    public int HighestAddedBaseDamage;   // ← 关键：最高值单独记账
}
```

`WhipTagEffect.ModifyTaggedHit`（`WhipTagEffect.cs:62-74`）同时累加 `AddedBaseDamage` 与 `HighestAddedBaseDamage`。于是 `Projectile.cs:12950-12953`：

```csharp
int highestAddedBaseDamage = changes.HighestAddedBaseDamage;
int num16 = changes.AddedBaseDamage - highestAddedBaseDamage;
float num18 = highestAddedBaseDamage + num16 * 1f;   // num17 = 1f
```

即 **多把鞭子的标签伤害只取最高值，其余按系数（当前 1.0）叠加**。这是一条硬规则，不是「全部相加」。

### 4.4 只对召唤体系生效

`WhipTagEffect.CanRunHitEffects`（`WhipTagEffect.cs:76-87`）：

```csharp
if (!optionalProjectile.minion && !ProjectileID.Sets.MinionShot[optionalProjectile.type] && !optionalProjectile.sentry)
    return ProjectileID.Sets.SentryShot[optionalProjectile.type];
return true;
```

被标 NPC 只对「召唤物 / 召唤弹 / 哨兵 / 哨兵弹」四类投射物加伤，近战与远程命中**不触发**。

### 4.5 目录规模

`ItemID.Sets.UniqueTagEffects`（`Terraria.ID\ItemID.cs:1313`）以 `new object[36]` 注册 **18 组**「物品 id → 效果实例」，与 18 种鞭子一一对应。具体子类 6 个：`Starcrash`、`DarkHarvest`、`Firecracker`、`Kaleidoscope`、`ElectricEel`、`Possession`、`ViolentDisplayOfFlower`、`SwordWhip`（`Terraria.GameContent.Items\`）。默认 `TagDuration = 240`（`WhipTagEffect.cs:17-22`），实际时长再乘 `owner.tagEffectDuration`（默认 1，饰品可 +2，`Player.cs:13071`）。

---

## 5. 召唤物（Minion）

### 5.1 占槽：投射物自己向玩家「报名」

```csharp
// Projectile.cs:16810-16845
if (numUpdates == -1 && (minion || sentry)) {
    Player player2 = Main.player[owner];
    damage = (int)((float)originalDamage * player2.minionDamage + 5E-06f);
}
if (minion && numUpdates == -1 && type != 625 && type != 628) {
    minionPos = Main.player[owner].numMinions;
    if (Main.player[owner].slotsMinions + minionSlots > (float)Main.player[owner].maxMinions
        && owner == Main.myPlayer) {
        // 627/626（双足飞龙/星尘龙段）在此处先做合并，再 Kill
        Kill();
    } else {
        Main.player[owner].numMinions++;
        Main.player[owner].slotsMinions += minionSlots;
    }
}
```

反直觉点：

1. **槽位是 `float`**（`Player.cs:1247`）。因为存在 `0.5` 槽的召唤物（`Projectile.cs:4709`、`4725`、`6739`）与 `0` 槽的（`6711`、`8093`、`8107`、`8121`）。
2. **计数写在 `Update` 里、由投射物主动加**，不是召唤时校验一次。每 tick 清零（`Player.cs:17123-17124`、`24723-24724`、`28487-28488`），由存活投射物重新累加。**权威在投射物，不在玩家。**
3. **超员不是拒绝新召唤，而是杀死新投射物**（`16838`）。
4. **伤害每 tick 重算**：`damage = originalDamage * minionDamage`（`16813`），只吃玩家召唤伤害加成，不吃物品前缀暴击。

### 5.2 跟随站位

`minionPos` 同时充当「我是第几只」与站位偏移：

```csharp
// Projectile.cs:50241-50276（节选）
vector.X -= (45 + player.width / 2) * player.direction;
vector.X -= minionPos * 30 * player.direction;   // 体型大的用 30
... 其余分支用 20 / 40 两档间距
```

其他使用点：`50909`（环形排布）、`59106-59110`、`59863`、`61223`、`61369`、`62497`、`62702`、`65043`。

### 5.3 目标与休息点

| 能力 | 位置 |
| --- | --- |
| 指定攻击目标 | `MinionNPCTargetAim`（`Player.cs:57587`）：取鼠标到 **`CanBeChasedBy` 的最近 NPC**，再按 `doNotDisableIfTheTargetIsTheSame` 决定是否清空 |
| 设置休息点 | `MinionRestTargetAim`（`Player.cs:57608`）：从鼠标所在列向上/下找实心面，落在 `Distance <= 1000` 内才写入 |
| 每 tick 失效判定 | `UpdateMinionTarget`（`Player.cs:57672`）：休息点距离 > 1000 清空；攻击目标 `!CanBeChasedBy` 或 `Hitbox.Distance > 3000` 清空 |
| 可被打标/受控的召唤物集合 | `ProjectileID.Sets.MinionTargetingFeature`（`ProjectileID.cs:399`，57 项） |

出世界保护：`minion` 投射物出界时被拉回玩家中心并置 `netUpdate = true`（`Projectile.cs:16792-16804`），非召唤物直接 `active = false` 并发 packet 29。

---

## 6. 哨兵（Sentry）

哨兵与召唤物共用 `minionDamage`（`16810` 的 `(minion || sentry)`），但**完全不进入第 5.1 的占槽分支**。

| 项 | 位置 | 说明 |
| --- | --- | --- |
| `sentry` 字段 | `Projectile.cs:332` | `SetDefaults` 置 false（`703`），各类型分别置 true（`4008`、`4624`、`6928`、`6940`、`7152`、`7252`、`7339`、`7353`、`9152`、`9716`） |
| `SentryShot` | `ProjectileID.cs:293` | 14 项，哨兵打出的子弹 |
| `WipableTurret` | `Projectile.cs:530-540` | `owner == Main.myPlayer && sentry && !TurretShouldPersist()` |
| `TurretShouldPersist` | `Projectile.cs:672-692` | 12 个撒旦军团炮塔在 `DD2Event.Ongoing` 期间免疫清除 |
| `UpdateMaxTurrets` | `Player.cs:53698-53726` | 收集全部 `WipableTurret`，超上限时**杀 `timeLeft` 最小者**，循环直到数量合规 |
| `maxTurrets` | `Player.cs:2831`，默认 1 | 加成点：`10270`、`13105`、`13231`、`13245`、`13258`、`13271`、`13284(+2)`、`13298(+2)`、`13313(+2)`、`13328(+2)`、`13972`；`ResetEffects` 回到 1（`18563`） |

与召唤物的三点差异：**不占 `numMinions`/`slotsMinions`**、**有 `timeLeft` 自然消亡**、**不跟随玩家**。

---

## 7. 宠物（Pet）

宠物是四套里最「没有设计」的一套：只有标记表 + 硬编码物品分支。

- 标记表：`Main.projPet`（`Main.cs:1301`），**125 项为 true**。
- 消费点：
  - `Projectile.cs:709` —— `correctSlopeCollision = Main.projPet[Type]`；
  - `Projectile.cs:17589-17597` —— 宠物忽略门，且按玩家相对高度决定是否站上斜坡；
  - `Projectile.cs:12495` —— 一长串排除项，决定「这个投射物算不算宠物」；
  - `NetMessage.cs:1790` —— 宠物**强制全区广播**（`Array.Clear(netSyncSkippedForPlayer)`），不吃分区裁剪。
- 召唤入口：`ItemCheck_ApplyPetBuffs`（`Player.cs:53728`）是一长串 `if (sItem.type == XXX) AddBuff(sItem.buffType, 3600)`，**按物品逐个硬编码**，无统一规则。
- 存在性判定混合两种风格：宠物 flag（`petFlag*` / `lightPetFlag*`，数量级为百）与 `ownedProjectileCounts`。例：buff 216 的雏鸟在 `ownedProjectileCounts[759] > 0` 时才算存在，否则在 `numMinions < maxMinions` 下补召唤（`Player.cs:10784-10814`）。

宠物与召唤物的本质差异：**不占槽、不吃 `minionDamage`、无目标选择、不参与战斗**。

---

## 8. 网络同步事实（核心结论）

### 8.1 鞭子只有两个包：出生、死亡

| 时机 | 包 | 位置 |
| --- | --- | --- |
| 出生 | 27 `SyncProjectile` | `Projectile.cs:11098`（`NewProjectile` 收尾，无条件） |
| 死亡 | 29 `KillProjectile` | `Projectile.cs:78868-78871`（`owner == Main.myPlayer && netMode != 0`） |
| 飞行中 | **无** | `AI_165_Whip` 内无任何 `netUpdate` |

飞行中若要产生 27，只能依赖通用的「`netUpdate` 提交」路径（`Projectile.cs:17317-17345`），而该路径要求 `owner == Main.myPlayer` 且 `netUpdate` 为真；`netUpdate` 在每次 `AI()` 前被清零（`16878`），鞭子 AI 从不重新置位。

### 8.2 packet 27 的真实载荷

发送侧（`NetMessage.cs:779-845`）：

| 字段 | 条件 |
| --- | --- |
| `key`（spawner + index + generation） | 恒发 |
| `position`、`velocity` | 恒发 |
| `type`（`short`） | 恒发 |
| `bitsByte25` 标志位 | 恒发 |
| `ai[0]` | `ai[0] != 0` |
| `ai[1]` | `ai[1] != 0` |
| `bannerId` | 非 0 |
| `damage` | 非 0 |
| `knockBack` | 非 0 |
| `originalDamage` | 非 0 |
| `ai[2]` | `ai[2] != 0`（走 `bitsByte26[0]`） |

接收侧（`MessageBuffer.cs:1722-1777`）：若 key 不存在则 `SetDefaults` 新建；服务端（`Main.netMode == 2`）收到后**原样转发**（`1773-1776`）。

### 8.3 决定曲线形状、但不在包里的输入

```csharp
// Projectile.cs:49162-49167
public static void GetWhipSettings(Projectile proj, out float timeToFlyOut, out int segments, out float rangeMultiplier)
{
    timeToFlyOut = Main.player[proj.owner].itemAnimationMax * proj.MaxUpdates;
```

| 输入 | 是否在 packet 27 | 同步途径 |
| --- | --- | --- |
| `itemAnimationMax` | **否** | 各机自行跑 `ItemCheck` → `ApplyItemAnimation` → `SetItemAnimation`（`Player.cs:4459-4461`） |
| `whipRangeMultiplier` | **否** | 各机自行从装备汇总（`Player.cs:13077`、`13953`、`13978`） |
| `HeldItem.useAnimation` | **否** | 手持槽由 packet 5 同步，物品定义本地已知 |
| `owner.GetArmPosition()` | **否** | 玩家位置/身体帧由 packet 13 同步 |

其中 `itemAnimationMax` 的缺口最值得注意：**packet 13 不携带它**。`MessageBuffer.cs` 的 `case 13`（`957-1064`）只读控制位、`selectedItem`、`position`、`velocity`、`mount` 等，全文没有 `itemAnimation`。它之所以看起来能对上，是因为**每台机器都对所有玩家跑 `ItemCheck`**（`Player.cs:32060` → `42915`，只有输入读取段受 `Main.myPlayer == whoAmI` 限制）。

这意味着：只要 `summonerWeaponSpeedBonus` 或 `whipUseTimeMultiplier` 在两侧算出不同值，同一条鞭子的长度就会静默不一致——而画面不会有任何报错。

### 8.4 伤害权威：鞭子是客户端权威

```csharp
// Projectile.cs:12512-12530
if (Damage_CanDealDamage()) {
    Rectangle projRectangle = Damage_GetHitbox();
    if (friendly && owner == Main.myPlayer && !npcProj && !minion && type != 1054) CutTiles();
    Damage_TryUsingPowders(projRectangle);
    if (owner == Main.myPlayer) {
        Damage_PVE(projRectangle, ...);
        Damage_PVP(projRectangle, ...);
    }
    if (Main.netMode != 2) Damage_EVP(projRectangle);
}
```

专用服 `Main.myPlayer = 255`（`Netplay.cs:250`），而 `Projectile.Update` 在所有 `netMode` 下都会执行（`Main.cs:18209` → `Projectile.cs:16881`）。两者的组合结果：

> **服务端会完整跑一遍鞭子 AI、会算出整条曲线，但永远不会用它判定命中。** 服务端侧的鞭子计算在伤害意义上是空转；伤害由 `owner` 客户端算好后经 packet 24/28 上报转发。

### 8.5 广播范围

- packet 27 默认按**分区**裁剪（`NetMessage.cs:1801-1809`）：`IsSectionActive(NetSectionCoordinates)` 为假时跳过并记 `netSyncSkippedForPlayer`，等分区激活后补发（`Projectile.cs:17354-17364`）。
- 例外：`type == 12`、`Main.projPet[type]`、`aiStyle == 11`、`netImportant` 四类**取消裁剪**（`1790-1794`）。**鞭子不在此列**，所以鞭子只在同分区可见。

---

## 9. NLTX 现状对照

| 能力 | NLTX 现状 | 证据 |
| --- | --- | --- |
| 包 27 / 29 收发 | **已具备** | `ProjectilePacketGatewayRegistration.cs`：`Packet27Handler` 把 `Ai0/Ai1/Ai2` 映射进 `ProjectileNetworkApplyCommand`，owner 取会话身份（`76-78`） |
| 包 99 / 115（召唤目标） | **已具备**（仅协议层） | `PlayerStatePacketRegistration.cs:24-25`；`PlayerStatePacketHandlers.cs:489-524` |
| 召唤物槽位 | **无** | `grep MinionCapacity/MinionSlot` 仅命中枚举声明 |
| 鞭子几何 | **无** | 无 `WhipPoints`/`FillControlPoints` 对应物 |
| 鞭子标签 | **无** | 无 `TagEffect` 对应物 |
| 哨兵 | **无** | `grep Sentry` 在 `src` 下 0 命中 |
| 宠物 | **无** | 仅 `ItemId.g.cs` 的物品名与 `Main.projPet` 无关 |
| 属性键 | **已声明、零消费者** | `AttributeKind.MinionCapacity`（`49`）、`WhipRange`（`51`）、`SummonDamage`（`15`）；仅 `DamageMitigationSystem.cs:181` 把 `DamageType.Summon` 映射到 `SummonDamage` |

计划侧已登记为未开工：`docs/plans/2026-11-24-projectile-component-systems-execution-plan.md` 第 380-381 行（能力并入方式）、第 394-395 行（P5 状态「未开工」，阻塞点写「`AttributeKind.MinionCapacity` 零消费者，需先定槽位归属」）。

---

## 10. 对 NLTX 的设计含义

### 10.1 鞭子按「确定性重建」建模，不要建模成「状态同步」

曲线是 `(ai[0], ai[1], ai[2], velocity, type, owner 状态)` 的纯函数，控制点数量 20~45。逐帧同步控制点比一次 packet 27 贵两个数量级，且**不解决 `itemAnimationMax` 的漂移**——传点只会掩盖它。

建议：线上沿用 packet 27 的六个参数；服务端把 `itemAnimationMax` / `whipRangeMultiplier` / `HeldItem.useAnimation` 变成**显式输入**而不是从别处隐式读，从而让「服务端曲线 == 客户端曲线」可被断言。

### 10.2 服务端重算命中是新增工作量，不是移植

原版服务端从不判定鞭子命中（§8.4）。NLTX 若要服务端权威，必须复刻：

1. `FillWhipControlPoints`（含 `getActualCollisionPoints` 的 `ai[1] = 1f` 强制）；
2. 前后帧（`frameOffset = -1`）控制点并集矩形；
3. 逐点 `Intersects` 对 NPC 矩形求交。

这三步是鞭子手感的全部来源，替换成「沿曲线采样圆」会直接改变手感。

### 10.3 槽位归属与「投射物写玩家状态」冲突

原版是投射物在 `Update` 里直接 `Main.player[owner].numMinions++`（§5.1），属于「一个域写另一个域」。与 `Context/项目约束/ECS系统输入边界约束.md` 的正向约束冲突。

可选路径（需拍板）：
- A：召唤物生成时提交「占槽请求」，玩家侧系统消费后写容量组件，召唤物只读自己是否获批；
- B：把槽位账本做成独立域（不属 Player 也不属 Projectile），双方都只读写账本。

A 会引入原版不存在的「请求被拒绝」状态；B 不引入新语义但多一个域。

### 10.4 Tag 系统建议独立成域

它同时被鞭子（生产者）与召唤物（消费者）依赖，且是「玩家 × NPC」的二维稀疏状态。放 Projectile 域会造成双向耦合，放 Player 域会让 NPC 域反向依赖。

需要注意原版的实现细节：状态数组是 `Main.maxNPCs` 定长、每 tick 全量递减（§4.1），而 NLTX 的 ECS 惯例更倾向稀疏事件。这是一个需要显式决策的语义差。

### 10.5 宠物不要照搬硬编码

`ItemCheck_ApplyPetBuffs` 是按物品逐个 `if` 的硬编码（§7）。照搬等于把反编译的债搬进新架构。建议先只做数据驱动的 `PetDefinition` 表，把硬编码压成配置——但这是内容侧大改，需单独立项。

### 10.6 优先级建议

- **低风险起步**：哨兵（只读玩家属性 + 有上限裁剪 + 有 `timeLeft`，边界最干净）。
- **次之**：鞭子几何（纯函数、无跨域写入）。
- **最后**：鞭子标签（跨域）与宠物（内容量）。

---

## 11. 局限与未验证项

1. **纯静态读码**，未运行参考客户端，未做多人实测；§8.3 的「`itemAnimationMax` 可能漂移」是代码推理，没有实测反例。
2. **未通读** `Projectile.cs` 全文（约 81k 行），结论建立在 grep 命中点及其上下文；可能存在本文未覆盖的、按类型特化的鞭子分支。
3. **未核对** `ContentSamples.ItemsByType` 在服务端的初始化时机与内容一致性。
4. **未核对** 服务端 `ItemCheck` 是否对所有远程玩家完整执行（本文按 `Player.cs:32060` 的调用链推断，未跟踪服务端的玩家更新入口）。
5. **路径问题**详见 §2；若用户原意指向另一套源码，本报告需重新取证。
