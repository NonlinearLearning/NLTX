# Projectile 穿透预算联动设计（属性 / 战斗 / 弹体 / 空间）

- 日期：2026-11-23
- 状态：design-proposal（待决策，未实施）
- 适用项目：`src/NSSLC/Terraria.Components.csproj`（`Terraria.Projectile`、`Terraria.Combat`、`Terraria.Attributes`、`Terraria.Physics`）
- 行为参考：`D:\TRbackup\最新的客户端\Terraria\Projectile.cs`（反混淆源码，行号均指该文件）
- 上游决策：[CR-2026-10-26：Projectile 穿透与免疫边界迁入 Attributes](../cr/CR-2026-10-26-projectile-attributes-boundary.md)

本文回答一个问题：**穿透次数由属性给出、由弹体持有、被命中与反弹共同消耗，这三方在 ECS 里如何联动。**

## 1. 问题陈述

需要同时满足：

1. 属性系统里有"这支弹体最多能穿几个"；
2. 弹体携带该预算，命中造成伤害的实体后消耗；
3. 部分弹体的**物块反弹**同样消耗该预算；
4. 部分弹体的物块反弹**不消耗**预算；
5. 预算耗尽后弹体消失（或在配置为"无害飞行"时继续飞但不再伤害）；
6. 被无敌帧 / 阵营关系拒绝的命中**不消耗**预算。

## 2. 原版事实取证

### 2.1 字段与语义

| 字段 | 行号 | 语义 |
| --- | --- | --- |
| `penetrate` | `:366` | 剩余可命中次数；默认 `1`，`-1` = 无限 |
| `maxPenetrate` | `:376` | 生成时快照的初始预算 |
| `usesLocalNPCImmunity` | `:370` | 是否启用"逐弹体逐目标"免疫表 |
| `stopsDealingDamageAfterPenetrateHits` | `:408` | 穿完后转为"继续飞但不伤害" |

`SetDefaults` 末尾统一执行 `maxPenetrate = penetrate;`（`:10675`）——**上限是生成时快照，不在运行期回读**。

### 2.2 命中消耗

```csharp
// :14190-14206
if (penetrate > 0 && type != 317 && type != 866) {
    if (type == 357) damage = (int)(damage * 0.8);   // 每个实体 -20%
    penetrate--;
    if (penetrate == 0) {
        if (stopsDealingDamageAfterPenetrateHits) { penetrate = -1; damage = 0; }
        keepIterating = false;                        // 本帧不再扫其它目标
    }
}
```

要点：
- 扣减发生在**伤害结算之后**，因此耗尽的那一击**仍然造成伤害**；
- `keepIterating = false` 只结束本次碰撞扫描，**不是销毁**；
- `stopsDealingDamageAfterPenetrateHits` 把状态改成 `penetrate = -1` + `damage = 0`，即"无限穿透但零伤害"。

### 2.3 归零后的销毁点

```csharp
// :17308-17315，位于 Update 尾部
timeLeft--;
if (timeLeft <= 0) Kill();
if (penetrate == 0) Kill();
```

**销毁在 tick 末尾，不在命中当场。** 这决定了 NSSLC 里必须是"入队终止请求 + commit 边界销毁"，不能在碰撞处理里直接销毁实体。

### 2.4 免疫先于扣减

```csharp
// :12635
bool flag3 = maxPenetrate == 1 && !usesLocalNPCImmunity && !usesIDStaticNPCImmunity;
if (!((owner < 0 || targetNPC.immune[owner] == 0) | flag3)) return;   // 直接返回，不扣预算
```

被无敌帧挡掉的命中在扣减之前返回。**"扣减"必须由 Combat 的确认结果驱动，不能在提交请求时预扣。**

### 2.5 反弹消耗

同一份预算，三种不同顺序：

```csharp
// 脉冲矢 357 / 蜜蜂 181 / 189 / 566：预算为 0 就不反弹（:19020-19026）
else if ((type == 181 || type == 189 || type == 357 || type == 566) && penetrate > 0) {
    if (type == 357) damage = (int)(damage * 0.9);    // 反弹 -10%
    penetrate--;
    ...
}

// 宝石法杖 121-126 / 597：先反弹，扣完再判断存活（:18108-18168）
velocity.X = (0f - lastVelocity.X) * 0.8f;
penetrate--;
flag7 = penetrate > 0;
if (!flag7) Kill();

// 弹跳飞刀 477：撞墙扣减，耗尽即死，同时 damage *= 0.85（:18757-18765）
if (penetrate <= 0) Kill();
```

三种顺序必须由数据表达，而不是靠三个分支写死。

### 2.6 默认撞墙

```csharp
// :19965-19969，HandleMovement 的默认分支
else {
    position += velocity;
    Kill();
}
```

绝大多数弹体撞墙即死：不反弹、不扣预算。

### 2.7 逐目标免疫是独立机制

```csharp
// :14176-14180  逐弹体 × 逐目标 的命中冷却
else if (usesLocalNPCImmunity && localNPCHitCooldown != -2) {
    targetNPC.immune[owner] = 0;
    immuneArray[victimIndex] = localNPCHitCooldown;   // immuneArray == localNPCImmunity
}
// :17379-17385  每 tick 递减
for (int j = 0; j < Main.maxNPCs; j++)
    if (localNPCImmunity[j] > 0) localNPCImmunity[j]--;
```

它与穿透预算正交：**预算决定"还能打几个"，免疫表决定"同一个不能连打"**。原版靠这张表避免一帧内多次命中同一目标重复扣预算。

### 2.8 外部设计参照

- [The Projectile System](https://unheard-software.bearblog.dev/the-projectile-system/) 把 *"Penetration counter — tracks how many surfaces it has punched through (like health for the projectile; when it hits zero, the projectile is removed)"* 列为弹体四要素之一，即**预算是弹体自身的"生命值"**。
- [Godot combat-basics](https://github.com/summerengine/docs/blob/743c7f508b8eb5d6e38127895d7dd4122efe888c/skills/combat-basics/references/projectiles-and-aoe.mdx) 的穿透写法是 `pierce -= 1; if pierce <= 0: queue_free()`，并额外维护"已命中集合"。
- ECS 侧的通用结论是销毁必须走延迟命令缓冲，不能在迭代查询中做结构变更，见 [ecs-framework 的 CommandBuffer 文档](https://github.com/esengine/ecs-framework/blob/2886284e8015c57ad8a0ef89596a058890cfa679/docs/src/content/docs/en/guide/system/command-buffer.md)。NSSLC 已有对应设施：`ProjectileTerminationQueue` + `ProjectileTerminationCommitSystem`。
- 商业实现同样把穿透计数建模为弹体数据而非角色属性（[Ripper 的弹体系统说明](https://steamcommunity.com/games/4298490/announcements/detail/532124583945306889)）。

## 3. 归属划分

| 关注点 | 归属域 | 载体 | 理由 |
| --- | --- | --- | --- |
| 初始预算 | Attributes | `ProjectilePenetrationLimit`（Policy） | 可被装备 / Buff 修饰 |
| 预算快照 | Projectile | 生成时写入 `Remaining` | 与原版 `maxPenetrate = penetrate` 一致 |
| 剩余 / 已耗 | Projectile | `ProjectilePenetrationStateComponent` | 消耗方与所有者同域，规则 2 直接读写 |
| 伤害是否落地 | Combat | `DamageAppliedReceipt` | 弹体不参与减伤计算 |
| 反弹参数 | Projectile | `ProjectileBouncePolicyComponent` | 内容侧策略，不是运行时状态 |
| 速度反射 | Movement | `MovementIntentRequest` | 弹体不写 `VelocityComponent` |
| 结构销毁 | Projectile Lifecycle | `ProjectileTerminationQueue` → Commit | 结构变更只在 commit barrier |

### 3.1 为什么计数不搬进 Attributes 域

`ShieldPoolComponent` 需要独立 + 命令入口，是因为**消耗方（Combat）与所有者（Attributes）不同域**。穿透不同：消耗方和所有者都在 Projectile 域内，按 [ECS 系统输入边界约束](../../Context/约束/ECS系统输入边界约束.md) 规则 2 应直接读写。

把它挪进 Attributes 只会增加一次跨域调用和一次回写，不产生任何隔离收益。

**"属性里有穿透次数"成立，指的是"最多能穿几个"，不是"还剩几个"**——与 `MaxShield` / `ShieldPoolComponent.Current` 是同一组关系。

## 4. 一 tick 时序

```text
Preparation
 ① 生成：Resolve(ProjectilePenetrationLimit) → Remaining（-1 保持 -1）
 ② 写 MovementIntentRequest（不入队即直写属越界）

Simulation
 ③ 空间求解产出 TileContact（CollidedX/Y + 入射速度 + 法线）
 ④ Physics 产出实体接触事实
 ⑤ 弹体请求系统：按 Consumed+PendingConsumed 算衰减，入队 CombatAttackIntent，
    仅 PendingConsumed++（不扣 Remaining）

Commit
 ⑥ CombatAttackRequestSystem → DamageRequest
 ⑦ CombatDamageRequestSystem → 写 Life，发布 receipt
 ⑧ 弹体结果系统：Applied → TryConsume(EntityHit)；Rejected → PendingConsumed--
 ⑨ 弹体瓦片系统：可反弹且预算允许 → TryConsume(TileBounce) + 反射意图
                  否则 → Enqueue(TileCollision)
 ⑩ ProjectileTerminationCommitSystem：按 handle 去重，取原因优先级，提交销毁
```

### 4.1 为什么需要 `PendingConsumed`

原版 `damage *= 0.8` 是同步执行的，第二次命中自然看到衰减后的值。NSSLC 跨阶段异步：Simulation 入队、Commit 才确认，同一 tick 的两次命中会读到同一个 `Consumed`，衰减丢失。

```csharp
public readonly record struct ProjectilePenetrationStateComponent(
    int Remaining,        // 已确认剩余（-1 = 无限）
    int PendingConsumed,  // 本 tick 已入队、等回执
    int Consumed);        // 累计已确认，用于衰减与诊断
```

衰减倍率用 `Consumed + PendingConsumed` 查表。

### 4.2 回执必须能回滚

```text
Applied   → Remaining--, PendingConsumed--, Consumed++
Rejected  → PendingConsumed--          （Remaining 不动）
```

不回滚会出现"打空也扣穿透"，与原版 `:12635` 的 early return 相悖。

### 4.3 扣减判据

`DamageProcessResult.Applied` 为准，不看 `AcceptedDamage`：

- 全部被护盾吸收（`LifeDamage == 0`）**仍算一次命中**——原版扣减处不检查实际扣血；
- 被免疫 / 阵营 / 目标已死拒绝的，不扣。

## 5. 组件形状

```csharp
// Projectile/Component/ProjectilePenetrationStateComponent.cs（改造现有类型）
public readonly record struct ProjectilePenetrationStateComponent(
    int Remaining,
    int PendingConsumed,
    int Consumed)
{
  public const int Unlimited = -1;
  public bool IsExhausted => Remaining == 0;
}

// Projectile/Component/ProjectileBouncePolicyComponent.cs（替换现有 Capability + State 两件套）
public readonly record struct ProjectileBouncePolicyComponent(
    int MaximumBounces,          // -1 = 无限
    int PenetrationCost,         // 每次反弹消耗多少；0 = 不消耗
    float SpeedScale,            // 0.8 / 0.9 / 0.6 / -0.75 ...
    BounceConsumeOrder Order);   // BeforeBounce | AfterBounce

public enum BounceConsumeOrder : byte
{
  BeforeBounce = 0,   // 预算为 0 则不反弹（脉冲矢）
  AfterBounce = 1     // 最后一次反弹照做，反弹后销毁（宝石法杖）
}
```

现有 `ProjectileBounceCapabilityComponent` 与 `ProjectileBounceStateComponent` 全仓零消费者（仅自身声明），可直接替换，不构成破坏性变更。

### 5.1 三态语义

| `Remaining` | 含义 | 是否伤害 | 是否可反弹 |
| --- | --- | --- | --- |
| `> 0` | 还能命中 N 次 | 是 | 是（若 `MaximumBounces` 允许） |
| `0` | 耗尽 | 否（`stopsDealingDamageAfterPenetrateHits` 为真时）| 否 |
| `-1` | 无限 | 是 | 是 |

"继续飞但不伤害"（原版 `penetrate = -1; damage = 0`）建议用独立标志表达，不要复用 `-1` 哨兵——同一个值同时表示"无限"和"无害"，读取方必须同时知道第二个字段才能解释，容易出错。

```csharp
public bool SuppressesDamage;   // 穿完后不再伤害，但仍存活
```

## 6. 四个场景走查

### A. 普通箭（预算 1）

```text
命中 → intent 入队（PendingConsumed=1）
     → Applied → Remaining 1→0
     → Enqueue(HitLimitReached)
     → Commit 销毁
```

即"触碰第一个造成伤害的实体后消失"。若目标处于无敌帧，箭不扣预算继续飞——与原版一致。

### B. 脉冲矢（预算 6，命中与反弹共用）

```text
穿第 1 个：Remaining 6→5，damage ×0.8
撞墙反弹：CanBounce && Remaining > 0 → Remaining 5→4，反射，damage ×0.9
穿第 2 个：Remaining 4→3，damage ×0.8
...
第 6 次消费 → Remaining = 0
下一次接触：CanBounce = false → Enqueue(TileCollision)
```

`BeforeBounce` 语义（先判预算，够了才反弹）。

### C. 宝石法杖（反弹后归零）

```text
撞墙 → 反射速度 → penetrate--（0）→ AfterBounce 判定 → Kill
```

最后一次反弹照常发生，反弹完成后销毁。`AfterBounce` 语义。

### D. 无限反弹不耗预算

`PenetrationCost = 0`，只受 `timeLeft` 约束，无需额外开关。

## 7. 仓库现状与缺口

| 缺口 | 证据 | 影响 |
| --- | --- | --- |
| 空间系统丢弃瓦片碰撞结果 | `SpatialCommit` 仅含 `Location/Velocity/Grounding/LiquidContact`；`SpatialSimulationCoordinatorSystem.cs:101-109` 只写这四项 | 撞墙事实无法流出 |
| 唯一计算 `CollidedX/Y` 的系统未注册 | `ProjectileSimulationSystem` 在全仓无外部消费者 | 即使补 commit 也无数据源 |
| 反弹组件是死代码 | 两个 `ProjectileBounce*Component` 仅自身声明 | 反弹链路整条缺失 |
| 上限在 apply 阶段每击重读 | `ProjectileHitResolutionSystem.cs:146-151` 每次命中调用 `AttributeCalculator.Resolve` | 与"生成时快照"的原版语义不符 |

结论：**第一步不是改穿透，而是补瓦片接触事实。** 建议照 `PhysicsContactQueueComponent` 的模式（专用世界实体 + `BeginCapture/Add/Drain`）新增 `TileContactQueueComponent`，由空间层在 commit 时写入，弹体瓦片系统 Drain。

不要把接触事实塞进 `SpatialCommit`——那是"允许写入的状态"清单，事实与状态是两类东西。

## 8. 待决问题

1. **`ProjectilePenetrationLimit` 是生成时快照还是每帧实时解析？** 原版为快照（`:10675`）。建议快照。
2. **反弹顺序默认取哪种？** 建议默认 `BeforeBounce`（脉冲矢），宝石法杖显式声明 `AfterBounce`。
3. **`stopsDealingDamageAfterPenetrateHits` 是否本轮到？** 若需要，`ProjectilePenetrationStateComponent` 必须增加 `SuppressesDamage`。
4. **命中判据是 `Applied` 还是 `Applied && AcceptedDamage > 0`？** 建议前者，与原版一致。

## 9. 验收标准

- 预算 1 的弹体命中一次后当 tick 末销毁，且该次伤害已落地；
- 预算 N 的弹体可命中 N 个不同目标，第 N 次后销毁；
- 被无敌帧拒绝的命中不扣预算，弹体继续飞行；
- 同一目标在同一免疫窗口内重复接触只扣一次；
- 反弹消耗预算的弹体，反弹次数与命中次数之和达到预算时销毁；
- `PenetrationCost = 0` 的弹体反弹不扣预算；
- 同一 tick 内"命中 + 反弹"同时耗尽时只入队一条终止请求，且原因取优先级高者；
- 所有新增断言落在 `Test/Terraria.Components/Terraria.Projectile`，与现有 35 条并列。
