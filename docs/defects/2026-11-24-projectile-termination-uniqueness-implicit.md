# 缺陷：双路径同帧耗尽时的终止唯一性靠隐式守卫维持，无测试固定

```text
文档 ID：DEF-2026-11-24-projectile-termination-uniqueness-implicit
逻辑域：projectile
产物类型：defect-record
状态：open（严重度：中；非已确认 bug，是未固定的隐式契约）
发现于：CR-2026-11-24-projectile-p0-p1-content-and-shoot-seam §5.1
证据入口：§2 代码证据、§3 时序推导、§4 根因、§7 闭合口径
canonical 路径：docs/defects/2026-11-24-projectile-termination-uniqueness-implicit.md
```

## 1. 结论

「同 tick 命中与反弹同时打空预算，只入队一条终止请求」目前**成立**，但成立方式是隐式的：
瓦片路径显式查 `Contains`，命中路径靠 `ApplyCore` 内部调用的 `CheckAdmission` 顺带查。
两处守卫的粒度不同，且 `Enqueue` 本身不做去重，因此这条性质没有任何测试固定，
新增第三个终止生产者时会静默破坏。

## 2. 现象（代码证据）

瓦片路径，显式守卫：

```csharp
// src/NSSLC/Projectile/ProjectileTileResolutionSystem.cs:52
if (Terminations.Contains(projectile, tick))
{
  // 本 tick 已被别的路径判定终止：跳过
}
```

命中路径，守卫藏在准入检查里：

```csharp
// src/NSSLC/Projectile/ProjectileHitResolutionSystem.cs:52-55（CheckAdmission 内）
if (Terminations.Contains(projectile, tick))
{
  return ProjectileHitAdmissionResult.ProjectileInactive;
}
```

```csharp
// src/NSSLC/Projectile/ProjectileHitResolutionSystem.cs:143-150（ApplyCore 开头）
ProjectileHitAdmissionResult admission = CheckAdmission(
    projectileHandle, targetHandle, tick);
if (admission != ProjectileHitAdmissionResult.Accepted)
{
  return admission;      // ← 已终止则提前返回，走不到后面的 Enqueue
}
```

而队列本身不去重：

```csharp
// src/NSSLC/Projectile/Component/ProjectileTerminationQueueComponent.cs:38-53
long requestId = current + 1;
_nextRequestIds[handle] = requestId;
_pending.Add(new ProjectileTerminationRequest(handle, reason, tick, requestId));  // 无条件追加
```

结论：`Enqueue` 无内建幂等，唯一性完全由两个调用方各自守。

## 3. 时序推导（为什么现在是对的）

1. **Simulation** `CombatProjectileTileResolution`（`after: CombatPhysicsContactCapture`）：
   撞瓦片 → `ConsumeFor(cost)` → `IsExhausted` → 入队 `TileCollision`（第 1 条）。
2. **Simulation** `CombatProjectileDamageRequest`（排在瓦片结算之后）：只做预扣与入队攻击请求，
   **不入终止队列**。
3. **Commit** `CombatProjectileDamageResult`（`after: CombatDamageRequest`）：拿到 `Applied` 回执 →
   `ApplyCore` → `CheckAdmission` → `Contains` 为真 → 返回 `ProjectileInactive`，不再入队。
4. **Commit** `CombatProjectileTermination`（`after: CombatProjectileDamageResult`）→ `Drain` 得到 1 条。

正确性依赖两件事：**瓦片结算早于伤害回执落地**，以及 **`ApplyCore` 始终先调 `CheckAdmission`**。
两者都不是类型系统保证的。

## 4. 根因

"本 tick 已判定终止"这一事实只存在于终止队列的 `Contains` 里，且查询责任下放给各生产者。
预算状态组件是共享权威，却不知道"已经决定终止"；队列知道，但不阻止重复写入。
权威被切成两半，中间靠调用方自觉对齐。

对比同族问题：`SingletonComponent` 已在 P4 收紧为重复即抛错——同一个模式（把正确性寄托于
调用方自觉）在终止队列上还留着。

## 5. 期望行为

- `Enqueue` 幂等：同句柄同 tick 第二次入队返回既有 `requestId`，不追加；
- 原因优先级集中定义（当前枚举值 `Expired=0 … AdministrativeCleanup=8` 只是声明顺序，
  不代表优先级，也没有排序表或比较器）；
- 同帧命中 + 反弹打空预算时，`Drain` 只返回 1 条，原因取优先级高者。

## 6. 影响面

- 现在不报错，属**静默脆弱**：任何新增终止生产者（例如 P5 的液体响应、切草）只要忘了查
  `Contains`，就会产生两条终止请求；
- 两条请求下 `requestId` 对同句柄自增，破坏"每句柄每 tick 唯一"的隐含假设，
  依赖它做幂等的逻辑失效；
- 原因统计（掉落、音效、死亡归因）可能取到错误原因。

## 7. 闭合口径

1. `ProjectileTerminationQueueComponent.Enqueue` 内建去重（同句柄同 tick 已存在则返回既有 id）；
2. 集中定义原因优先级并在 `Drain`/提交阶段应用；
3. 新增端到端用例：同帧注入一条 `Applied` 伤害回执 + 一条瓦片接触事实，断言
   `Drain(tick).Count == 1`、原因等于优先级高者、提交后弹体只销毁一次。

第 1、3 条落地即闭合。第 1 条是让缺陷不因"新加生产者"复发的结构性修复，
第 3 条是把隐式契约变成可执行证据。

## 8. 涉及文件

- `src/NSSLC/Projectile/Component/ProjectileTerminationQueueComponent.cs`（Enqueue 无去重，:38-53）
- `src/NSSLC/Projectile/ProjectileHitResolutionSystem.cs`（守卫在 CheckAdmission :52，ApplyCore :143 调用）
- `src/NSSLC/Projectile/ProjectileTileResolutionSystem.cs`（显式守卫 :52）
- `src/NSSLC/Projectile/ProjectileTerminationCommitSystem.cs`
- `src/NSSLC/Projectile/System/ProjectileTerminationReason.cs`（优先级未定义）
- `src/NSSLC.Server/Registration/CombatSystemRegistration.cs`（阶段与 `after` 决定推导前提）
- `Test/Terraria.Components/Terraria.Projectile/Program.cs`

## 9. 状态历史

- 2026-11-24：P4 收尾记录为"缺端到端用例"。
- 2026-11-24：核对代码后确认守卫存在但为隐式（`CheckAdmission` 顺带），
  结论修正为"契约靠隐式守卫维持，无测试固定"，未发现已确认的重复入队。

## 10. 复查提示

关闭后本文件删除（按 [项目文件组织设计约束](../../Context/架构约束/项目文件组织设计约束.md) §3.3，缺陷不转归档）。


不要按"命中路径缺守卫"去修——它经 `CheckAdmission` 已经守住了。
真正该改的是 `Enqueue` 的幂等性与原因优先级，外加一条端到端用例。
