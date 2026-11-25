# 缺陷：追踪弹转向速率单调衰减（实测 8 → 7.000），断言按实测值放宽

```text
文档 ID：DEF-2026-11-24-projectile-homing-speed-drift
逻辑域：projectile / physics
产物类型：defect-record
状态：open（严重度：中）
发现于：CR-2026-11-24-projectile-p0-p1-content-and-shoot-seam §5.1
证据入口：§2 实测输出、§4 根因、§7 闭合口径
canonical 路径：docs/defects/2026-11-24-projectile-homing-speed-drift.md
```

## 1. 结论

追踪弹转向时速率会衰减：`homing-spark` 入场 8 px/s，实测稳定在 **7.000**，损耗 12.5%。
衰减是**单调的**（只降不升），因为每一帧都拿"已经变短的当前速度"当目标模长。
现有断言 `homing-keeps-speed-within-tolerance` 放成 `6.5 < speed <= 8.05`，
**把缺陷观测值写成了容差**，测试因此是绿的。

## 2. 实测输出

```text
$ dotnet Test/.../Terraria.Projectile.dll
PASS homing-turns-toward-target | velocity=(-0.01,7.00)（目标在正下方，应转到接近满速的 Y 方向）
PASS homing-keeps-speed-within-tolerance | speed=7.000（目标值 8.000；两次 MoveTowards 叠加导致漂移，见未闭合项）
```

断言原样：

```csharp
// Test/Terraria.Components/Terraria.Projectile/Program.cs:944-951
// 速率口径：目标值是"转向过程严格不掉速"（|speed-8| < 0.05）。当前实测会漂到 7.0 左右，
float p2HomingSpeed = p2HomingVelocity.Length();
Check(
    "homing-keeps-speed-within-tolerance",
    p2HomingSpeed > 6.5f && p2HomingSpeed <= 8.05f,
    $"speed={p2HomingSpeed:0.000}（目标值 8.000；两次 MoveTowards 叠加导致漂移，见未闭合项）");
```

注释自己写着"目标值是严格不掉速"，断言却接受 6.5 —— 测试固定的是错误行为。

## 3. 复现

1. 生成 `homing-spark`（速度 8，`turnRate = 60`）；
2. 目标放在正下方，使每帧期望方向与当前速度都有夹角；
3. 跑 1 帧读速度模长 → 7.000。

## 4. 根因

速度被**连续两次弦逼近**，且第一次的归一化锚在"当前速度"而不是"出厂速度"：

**第一步**：行为侧先把方向转一点，然后按当前速度归一化：

```csharp
// src/NSSLC/Projectile/System/ProjectileHomingAiBehavior.cs:67-76
Vector2 nextVelocity = SimulationVector.MoveTowards(
    velocity.LinearVelocity, desired, turnRate * deltaTime);
// 转向不得掉速：MoveTowards 走的是弦……（实测 40 tick 从 8 掉到 3.84）
float speed = velocity.LinearVelocity.Length();      // ← 锚点 = 当前速度
if (speed > 0f && nextVelocity != Vector2.Zero)
{
  nextVelocity = Vector2.Normalize(nextVelocity) * speed;
}
```

这里已经把弦缩短的模长补回 `speed`，所以 AI 输出模长 == 上一帧速度。**没有恢复出厂 8。**

**第二步**：空间侧再逼近一次，模长又被弦缩短，且没人补：

```csharp
// src/NSSLC/Share/Physics/SpatialSimulation/SpatialSimulationSystem.cs:185（自由速度解析，另见 :170 同形）
return SimulationVector.MoveTowards(current, desired, movement.Acceleration * deltaTime);
```

于是每帧净损失一步弦长，`nextVelocity.Length()` 成为下一帧的锚点，**单调递降并收敛**，
不会回到 8。第一帧损耗最明显（8 → 7.000），后续在 7 附近趋于稳定，因为方向差变小。

**关键**：这是**速率没有单一权威写入点**的结构问题。AI 认为自己在保速（补回当前速度），
空间系统认为自己只是执行加速度逼近（加速度为 0 时本应等价于保持），
两者的"保持"定义不同：一个是保模长，一个是保插值距离。

## 5. 期望行为

转向只改方向、不改模长：`|speed - 8| < 0.05`，且与转向频率无关。

## 6. 影响面

- 追踪弹越飞越慢，远距离目标追不上；转向越频繁掉得越多，行为不可预测；
- 内容层无法参数补偿：损耗率取决于方向变化频率，不是常数，调 `turnRate` / `maximumSpeed` 都无效；
- 断言放宽掩盖了问题：任何速率相关的后续回归都测不出来；
- 潜在连带：若将来有内容依赖"命中时的速率"做伤害缩放，伤害会随飞行轨迹漂移。

## 7. 闭合口径

1. **先红后绿**：把 `homing-keeps-speed-within-tolerance` 改回 `|speed - 8| < 0.05`，
   确认它失败——证明断言确实钉住了缺陷；
2. 收敛到单一权威写入点，二选一：
   - (a) 空间系统在 `Acceleration == 0` 且意图带 `hasDesiredVelocity` 时直接采纳 `desired`
     （等价于"无加速度即无逼近"），或
   - (b) AI 侧归一化锚点改为出厂速度（`ProjectileHomingPolicy` 的速度），由内容声明保速口径；
3. 断言转绿，且额外加一条"多帧转向后速率不变"的断言，防复发。

## 8. 涉及文件

- `src/NSSLC/Projectile/System/ProjectileHomingAiBehavior.cs`（第一步逼近 + 锚点，:67-76）
- `src/NSSLC/Share/Physics/SpatialSimulation/SpatialSimulationSystem.cs`（第二步逼近，:185）
- `src/NSSLC/Projectile/ProjectileAiAdapter.cs`（意图安装）
- `src/NSSLC/Projectile/System/ProjectileHomingPolicy.cs`
- `Test/Terraria.Components/Terraria.Projectile/Program.cs`（:944-951）

## 9. 状态历史

- 2026-11-24：P4 收尾发现，断言按实测区间放宽；记录为 open 并单独立文。

关闭后本文件删除（按 [项目文件组织设计约束](../../Context/架构约束/项目文件组织设计约束.md) §3.3，缺陷不转归档）。
