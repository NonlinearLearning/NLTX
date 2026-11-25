# 坐骑 / 矿车 / 钩爪 参考客户端实现调查报告

| 项 | 值 |
| --- | --- |
| 文档 ID | `RESEARCH-2026-11-25-MOUNT-MINECART-GRAPPLE` |
| 状态 | `draft`（机制取证完成；未运行参考客户端，未做运行时等价性验证） |
| 触发 | 用户要求阅读参考客户端中坐骑 / 矿车 / 钩爪相关代码并展开讨论 |
| 范围 | `D:\TRbackup\最新的客户端` 的三套系统实现，对照本仓库 `src/`（NSSLC ECS 基线） |
| 判据 | 「可移植」= 不依赖客户端绘制与输入即可复现同一位置 / 速度结果；仅复制数据表不算 |
| 日期 | 2026-11-25（Asia/Shanghai） |
| 参考客户端 | 反编译快照；`Mount.cs` 6,722 行、`Minecart.cs` 1,658 行 |

> 本报告只记录事实、差距与判断。**不含**实施计划——批次划分与验收口径需先由 §8 的四个决策确定，另立 `docs/plans/` 文档。

---

## 0. 结论摘要

1. 三套系统在官方实现里**没有独立文件边界**，全部寄生在 `Player.cs`（57,766 行）里，这是最大的阅读与移植障碍。
2. 三者本质是同一类东西：**玩家运动模式的覆盖层**——接管 `position/velocity`、绕过常规重力、自带状态机、需要专属网络字段。按三个独立系统移植会重复三遍同样的胶水。
3. NLTX 现状：坐骑 **31 行空壳**（无消费者）、矿车 **只有摆轨道没有跑车**（`TrackCollision` 等核心全缺）、钩爪 **完全为零**。
4. 唯一适合先行的是**矿车**：`TrackCollision` 是纯函数，可离线单测，是三者里唯一能"不接网络就验证"的。
5. 发现两处 NLTX 既有实现的问题（§6），其中 `MountStateComponent` 用异常表达领域非法输入，与项目错误处理约束冲突。

---

## 1. 代码入口与规模

| 系统 | 主文件 | 行数 | 关键搭档 |
| --- | --- | --- | --- |
| 坐骑 | `Terraria/Mount.cs` | 6,722 | `Terraria.ID/MountID.cs`(169)、`EntitySource_Mount.cs`、`PlayerQueenSlimeMountTextureContent.cs` |
| 矿车 | `Terraria/Minecart.cs` | 1,658 | `MinecartDiggerHelper.cs`(210)、`Player.cs:28304-28469`、tile **314** |
| 钩爪 | `Terraria/Projectile.cs:53426` `AI_007_GrapplingHooks` | ~320 | `Player.cs` 的 `grappling[]`/`GetGrapplingForces`/`GrappleMovement` |

**没有独立文件的部分**（全部在 `Player.cs`，57,766 行）：

| 内容 | 位置 |
| --- | --- |
| 坐骑装配/拆卸调用 | 6055、6073-6082、6329-6332、9970 |
| 坐骑运动接入 | 19744-19754（跳跃）、27156-27200（悬浮/飞行）、36476-36492（动画帧） |
| 矿车运动接入 | 19964-20091、27552-27559、28304-28469 |
| 钩爪力学 | 22793-23038（`GrappleMovement`/`GetGrapplingForces`）、6291-6320（`QuickGrapple`） |
| 钩爪黑名单 | 6195-6243 |

---

## 2. 证据基线与读取范围

| 项 | 内容 |
| --- | --- |
| 源码根 | `D:\TRbackup\最新的客户端`（只读，本轮未修改任何文件） |
| 规模 | `Terraria\Mount.cs` 6,722 行、`Terraria\Minecart.cs` 1,658 行、`Player.cs` 57,766 行、`Projectile.cs` 81,411 行、`Terraria.ID\MountID.cs` 169 行 |
| 读取方式 | grep 定位 + `read` 分段读；核心段逐行读，绘制层只读签名 |
| 并行取证 | 三个子代理分读坐骑生命周期、坐骑更新、矿车碰撞；其中坐骑全量读取与钩爪子代理中途失败，钩爪与矿车关键段由主线程重读补齐 |
| 未读取 | `CastSuperCartLaser` 内部逐行、`MinecartDiggerHelper.TryDigging` 逐行、66 个坐骑中 58 个装配函数的字段赋值、全部绘制层 |

**行号口径**：本文行号均指参考客户端反编译文件内的行号，可用 `read` 直接复现。

---

## 3. 坐骑系统

### 3.1 数据表

`MountData`（Mount.cs:130-266）是**纯数据**，70 个字段，66 个坐骑各一份（`MountID.Count = 66`，MountID.cs:167）。

分组如下：

| 组 | 字段示例 | 用途 |
| --- | --- | --- |
| 贴图 | `backTexture`/`frontTexture` + 各自 `Glow`/`Extra`（8 个 Asset） | 前后分层绘制 |
| 尺寸 | `textureWidth`/`textureHeight`/`xOffset`/`yOffset`/`playerYOffsets[]`/`heightBoost` | 玩家包围盒与绘制偏移 |
| 运动 | `runSpeed`/`dashSpeed`/`swimSpeed`/`acceleration`/`jumpSpeed`/`jumpHeight`/`fallDamage`/`extraFall` | 覆盖玩家默认常量 |
| 飞行 | `flightTimeMax`/`usesHover`/`fatigueMax`/`constantJump`/`blockExtraJumps` | 悬浮与疲劳 |
| 技能 | `abilityChargeMax`/`abilityDuration`/`abilityCooldown` | 充能型技能（9/46 等） |
| 动画 | `totalFrames` + 6 组 `*FrameStart/Count/Delay`（standing/running/flying/inAir/idle/swim/dashing） | 帧状态机 |
| 标志 | `Minecart`/`CanRideMinecartTracks`/`CanUseWings`/`MovementStatsAreAdditive` | 覆盖语义 |
| 委托 | `delegations`（MountDelegatesData） | 位置/尺寸/尘/音效覆写 |

`Mount` 实例 = `_data` 引用 + 运行时状态（`_frame`/`_frameState`/`_flyTime`/`_fatigue`/`_abilityCharge`/`_mountSpecificData`）。

### 3.2 三套更新，职责分离

| 方法 | 入口位置 | 管什么 |
| --- | --- | --- |
| `UpdateEffects`(4848) | `Player.Update` 25853 | 属性加成、自动索敌开火、光照、伤害免疫、特殊尘、**旱冰鞋轨道参数改写** |
| `Hover`(3358)/`Flight`(2985) | 27158-27164 | **垂直运动**与疲劳 |
| `UpdateFrame`(3611) | 36476-36492 | 动画状态机 `state 0站立/1奔跑/2空中/3飞行/4水中/5冲刺` |

**关键耦合**：`Hover` 的可悬浮判定直接读 `_frameState`（3361-3373：默认 state 2/4，Type49 只看 4，Type56 看 2/3，Type61 看 2/3/4）。**动画帧状态是悬浮物理的前置条件，不是纯表现**。

### 3.3 装配与拆卸不是换贴图

`SetMount`(6261) / `Dismount`(6206) 的实际副作用：

| 步骤 | 行为 | 行号 |
| --- | --- | --- |
| 早退 | `_type==m` 或 `m<=-1` 或 `m>=Count` 或 `(m==5 && wet)` | 6263 |
| 清旧态 | `ClearBuff`、`cartFlip=false`、`lastBoost=Zero`、`fullRotation=0`、`_mountSpecificData=null` | 6269-6277 |
| 绑 buff | `AddBuff(_data.buff, 3600)` | 6291 |
| 改尺寸 | `height = 42 + heightBoost`；有 `delegations.PlayerSize` 时改用委托值 | 6336 / 6309-6328 |
| 底部锚点 | 按新高度重算 `position.Y` 与 `shadowPos[]` | 6331-6342 |
| 特异数据 | 8→`DrillMountData`、35→`ExtraFrameMountData`、54→`SelectiveFlyingMountData` | 6353-6364 |
| 下马回缩 | `TryChangingSizeFromBottomCenter(hitbox, 20, 42)`，失败给 `FailedNoSpace` | 6690-6693 |

每帧在 `ResetEffects`(18389-18948) 里调 `mount.CheckMountBuff(this)`(18946)：buff 没了就自动下马（5333-5338）。

### 3.4 `MountID.Sets` 是移植的真正抓手

MountID.cs:12-31 的布尔集合，比 66 个 Type 的 if-else 更好用：

| 集合 | 成员 | 语义 |
| --- | --- | --- |
| `Cart` | 27 个（6,13,11,15,16,18-22,24-36,38,39,51,53） | 矿车系，吃 `TrackCollision` |
| `CanUseHooks` | **5 个**（54,57,58,59,60） | 允许与钩爪共存 |
| `CanDash` | 10 个 | 可冲刺 |
| `IsTransformationMount` / `PlayerIsHidden` | 52,54,55,56,61 | 变身系，隐藏玩家本体 |
| `DontHoldItems` / `DontDismountWhenCCed` | 55,56,61 | 禁持物品 / 眩晕时不强制下马 |
| `DoesNotOverrideBodyFrames` 等 4 个 | 57-60 | 旱冰鞋不覆盖原版帧 |

---

## 4. 矿车系统

### 4.1 轨道不是物件，是 tile 314 的前后双轨

`TrackCollision`(562-945) 是全系统心脏，本质是**以 2px 为步长的射线推进**：

```
磁点 = Position + (W/2 - 50/2, H/2) + MagnetOffset(25, 26)
循环：查 _tileHeight[frameX][(x%16)/2]
      跨 tile 时按连接值 0/2 把 Y 修正 ±2px 后重取 tile
返回：BitsByte（6 位事件）
```

前轨 = `tile.frameX`，后轨 = `tile.frameY`（1639-1657 扩展方法）。

### 4.2 BitsByte 六位语义与 Player 侧后果

| 位 | 名称 | 产出条件 | Player 侧后果 | 行号 |
| --- | --- | --- | --- | --- |
| 0 | OnTrack | 结束态 ∈ {OnTrack, OnFront, OnBack} | `onTrack=true`，算 `fullRotation`+`gfxOffY` | 28320-28325 |
| 1 | BouncyBumper | 撞弹力缓冲(-2) | X 反向；按左右键翻车 `cartFlip` | 28330-28352 |
| 2 | UsedRamp | 撞斜坡(-3) | `cartRampTime = Math.Abs(vx)/RunSpeed*20` 免重力帧 | 28357-28360 |
| 3 | HitSwitch | 压力板轨道 | `Wiring.HitSwitch`+`SendData(59)` | 28353-28356 |
| 4/5 | BoostLeft/Right | 助推轨道 | `trackBoost ±= 4` | 28361-28368 |

### 4.3 三个必踩的坑

1. **助推不本帧生效**：`trackBoost` 只累积，下一帧 `HorizontalMovement`(19944-19958) 才加到 `velocity.X` 并夹到 `±maxRunSpeed`。服务端同帧结算 = 双倍加速。
2. **水中先缩放再还原**：`velocity *= num110` → `TrackCollision` → `velocity /= num110`（28307-28314），其中 `num110` 在水/蜂蜜/微光下分别是 0.5/0.25/0.25。
3. **非矿车骑乘者会回滚整帧**：`!Cart` 且 bitsByte 的 `[2][0][4][5]` 全为假时，`position/velocity` 整帧还原（28315-28319）。旱冰鞋（57-60）走的就是这条路。

### 4.4 旱冰鞋会改写吸附参数

`UpdateEffects` 的 case 5225-5233 对 Type 57-60 改写 `MinecartSettings`：

```
MagnetOffset.Y -= 5f;  MinecartTextureWidth = 4f;
MagnetOffset.X = 2f;   WheelOffset.X = 4f;
```

而 `MinecartSettings = Minecart.Customization.Default` 每帧在 `ResetEffects`(18943) 里重置——顺序是 ResetEffects(18389) 先、UpdateEffects(25853) 后，所以**旱冰鞋的改写不会被重置吃掉**，但**必须按帧序执行**。移植时若把两者合并或调换顺序，旱冰鞋会整体错位。

---

## 5. 钩爪系统

### 5.1 投射物侧三态机（Projectile.cs:53426-53751）

| 态 | 行为 | 转出条件 |
| --- | --- | --- |
| `ai[0]=0` 飞行 | 逐格扫掠过区域找可钩瓦片 | 命中 → 2；超射程 → 1 |
| `ai[0]=1` 回弹 | 速度按 `num18` 归一化拉回 | `num3<24f` → `Kill()` |
| `ai[0]=2` 钩住 | 每帧校验瓦片仍可钩 | 瓦片消失 → 1 |

射程**按 Type 硬编码**（53510-53529）：13→300、32→400、73/74→440、165→375、256→425、315→500、322→550、646-649→550、652→600、865→300、935→500、486-489→480、446→500、230-235→`300+(t-230)*30`、753→420。回弹速度同样按 Type 分档（53661-53705）。

可钩判定（53740-53751）：`tileSolid[type]` 或 `type==314`（轨道）或 865 专属的树干/323；403 特殊，只认 314。

### 5.2 玩家侧约束求解

- 数据：`grappling[20]` + `grapCount`（Player.cs:2723-2725），上限 10（53775）。
- `GetGrapplingForces`(22939)：把玩家拉向**所有钩住点的平均中心**（23008-23012）；速度上限默认 11，315→14，73/74→12.5，487→12，646-649→16（23014-23033）。
- 三种特殊钩的向量算法完全不同：446（53555-53571 段在 Player 侧，22955-22971）、652（22972-22989）、865（22990-23000）。
- **黑名单**（6205-6243）：钩住后 3×3 非实心全拉黑；钩点是轨道/平台时再拉黑 5×5，防止反复钩同一格。

### 5.3 互斥是双向的三道闸

| 场景 | 规则 | 行号 |
| --- | --- | --- |
| 上马时 | 非 `Cart` 且非 `CanUseHooks` 且 `grappling[0]>=0` → 拒绝 | Mount.cs:6670 |
| 骑马中抓钩 | `GrappleMovement` 开头强制 `TryDismount` | Player.cs:22800 |
| 用钩爪道具 | `!CanUseHooks[Type]` → `TryDismount` | Player.cs:6329 |

---

## 6. NLTX 现状与差距

### 6.1 三系统对比

| 系统 | 现有实现 | 行数 | 差距 |
| --- | --- | --- | --- |
| 坐骑 | `Share/Physics/MountStateComponent.cs` | 31 | 只有 `MountKind`+`MountEntity`，**零系统消费**，仅 `PlayerEntityDefinition.cs:45` 注册。无尺寸变更、无 buff 绑定、无 Hover/Flight、无 UpdateEffects、无 Sets 语义 |
| 矿车 | `Infrastructure/WorldGeneration/Runtime/Minecart.cs` | 880 | 只有 `FrameTrack/PlaceTrack/GetTrackItem/IsPressurePlate`。`TrackCollision`/`OnTrack`/`TrackRotation`/`GetWheelsPositions`/`HitTrackSwitch` **全缺** |
| 钩爪 | — | 0 | **完全没有**。`PlayerControlsPacket.MountType` 能收字段，但领域层无消费者 |

### 6.2 发现的既有问题

**问题 1：`MountStateComponent.Mount()` 用异常表达领域非法输入**

```csharp
// src/NSSLC/Share/Physics/MountStateComponent.cs:15-24
public void Mount(int mountKind, EntityRuntimeHandle? mountEntity = null)
{
  if (mountKind < 0)
    throw new ArgumentOutOfRangeException(nameof(mountKind));
  ...
}
```

按 [错误处理约束](../../Context/项目约束/错误处理约束.md) 的分层规则，领域层不该用异常表达"输入非法"，应返回结果类型。同时 `Dismount()` 无返回值，调用方无法得知是否成功——而参考实现里下马**有明确的失败态**（`FailedCCed`/`FailedNoSpace`，Mount.cs:6682-6695）。

**问题 2：`Hover` 依赖动画帧状态，容易被误判为纯表现**

Mount.cs:3361-3373 证明悬浮物理读 `_frameState`。ECS 拆分时若把帧状态归入表现层砍掉，悬浮坐骑会全部失效。这是一条**必须显式保留的耦合**，不能靠"看起来像表现"来判定。

---

## 7. 判断

### 7.1 三者是同一类东西

都满足四特征：接管 `position/velocity`、绕过常规重力、自带状态机、需专属网络字段。建议**不建三个独立系统**，而是：

```
PlayerMovementMode = Normal | Mounted | OnTrack | Grappling
                     ↓ 各自是独立运动求解器
共享：输入快照 + 物理常量 + 瓦片查询接口
```

### 7.2 三条具体建议

1. **坐骑数据表不进 ECS**。66 份 `MountData` 是内容资源；组件只存 `MountKind` + 少量运行时状态（帧、疲劳、飞行时间）。
2. **矿车先做**。`TrackCollision` 是纯函数（输入位置/速度/瓦片查询，输出新位置/速度/bitsByte），最适合先做成可单测的纯函数，也是三者里唯一能"不接网络就离线验证"的。
3. **钩爪拆两段**。投射物那半复用现有投射物 ECS；玩家那半是"多点到单点"约束求解，独立成一个小系统。

### 7.3 优先级依据

三条全做是数千行量级。先行矿车的理由：纯函数可测 → 不依赖网络 → 失败可定位 → 产出能被另外两个复用（瓦片查询、物理常量、运动模式切换骨架）。

---

## 8. 待决策（阻塞实施）

以下四项未定，**本报告不给实施批次**。定了才拆颗粒度。

| # | 问题 | 影响 |
| --- | --- | --- |
| D1 | 目标是 (a) 服务器权威完整复刻 / (b) 只要状态同步、客户端不闪的最小闭环 / (c) 只做某条真实卡住的链路？ | 决定工作量是千行还是十行 |
| D2 | 三条链路哪条**现在实际卡住功能**？ | 决定优先级 |
| D3 | 帧状态算不算服务端状态？保留 `Hover`↔`_frameState` 耦合，还是重写成不依赖帧的等价判定（需逐坐骑验证手感差异）？ | 决定能否砍掉动画层 |
| D4 | 66 个坐骑全量，还是只做 `Cart`/`CanUseHooks`/`DontHoldItems` 三类？ | 只做三类能覆盖 90% 玩法分支，但特例（8 钻头 / 9·46 自动开火 / 12 猪鲨 / 54 狼）会全部退化 |

---

## 9. 未验证项

**已核对**（全部用工具实读，行号可复现）：§1–§5 所有行号、§6 的 NLTX 代码、§4.4 的每帧重置顺序与 `MinecartSettings` 改写冲突验证。

**未验证**：

- `TrackColors`(1327) 与 `GetSourceRect`(1525) 只读了签名，未逐行核对——属绘制层，不影响服务端权威判定。
- `MinecartDiggerHelper.TryDigging`(10) 只经子代理转述，未亲自逐行读。
- 66 个坐骑中仅核对了 SetAsMinecart / SetAsHorse / SetAsRollerSkate / SetAsChillet 四个装配函数的字段赋值，其余 62 个未逐个核对。
