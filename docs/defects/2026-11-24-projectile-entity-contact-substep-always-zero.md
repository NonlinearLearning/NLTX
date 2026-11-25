# 缺陷：实体接触事实的子步号恒为 0，与瓦片接触语义不一致

```text
文档 ID：DEF-2026-11-24-projectile-entity-contact-substep-always-zero
逻辑域：physics
产物类型：defect-record
状态：open（严重度：低；唯一读取方是队列去重比较，该比较不承担语义）
发现于：CR-2026-11-24-projectile-p0-p1-content-and-shoot-seam §5.1
证据入口：§2 代码证据、§4 根因、§7 闭合口径
canonical 路径：docs/defects/2026-11-24-projectile-entity-contact-substep-always-zero.md
```

## 1. 结论

`TileContactFact` 与 `PhysicsContactFact` 都有 `Substep` 字段，语义应当一致
（"第几个子步发生的接触"）。实际：瓦片接触携带真实子步号（已断言 `substep == 1`），
实体接触**恒为 0**，因为它的捕获发生在全部子步跑完之后。

## 2. 现象（代码证据）

瓦片接触，子步号来自子步循环变量：

```csharp
// src/NSSLC/Share/Physics/SpatialSimulation/SpatialSimulationCoordinatorSystem.cs
// 把 SpatialCommit.Substep（循环变量）写进 TileContactFact
```

```csharp
// src/NSSLC/Share/Physics/SpatialSimulation/System/SpatialCommit.cs
public int Substep { get; init; } = 0;   // 子步循环里逐次赋值
```

实体接触，捕获时机在位移之后，拿不到循环变量：

```csharp
// src/NSSLC/Share/Physics/ContactFact/PhysicsContactCaptureSystem.cs:76-92
PhysicsContactFact fact = new(
    first,
    second,
    pair.Contact,
    context.Tick,
    revision,
    substep: 0);          // ← 硬编码 0
```

两个事实类型定义本身是自洽的：

```csharp
// src/NSSLC/Share/Physics/ContactFact/System/PhysicsContactFact.cs
public int Substep { get; }   // 允许 0..n，ctor 校验 substep < 0 才抛
```

## 3. 复现

1. 造一个 `extraUpdates = 1`（2 个子步）的弹体，让它第 2 个子步撞瓦片、同帧撞到一个实体；
2. 排空两个队列：瓦片事实 `Substep == 1`，实体事实 `Substep == 0`。

## 4. 根因

两套接触的**生产时机不同**：

- 瓦片接触在 `SpatialSimulationSystem` 的子步循环内部产生，循环变量随手可得；
- 实体接触由 `PhysicsContactCaptureSystem` 在位移**完成之后**跑一次整体
  `EntityCollisionSystem.FindContacts`（`:67` 之后），它根本不知道子步这个概念。

不是算错，是架构上少了一层：实体碰撞检测没有被"子步化"。

## 5. 期望行为

两类事实的 `Substep` 都指向真实发生接触的子步。
消费方（反弹顺序、伤害衰减、P5 的命中附加效果）按它排序才不丢"先撞谁"。

## 6. 影响面

- 当前**无实际故障**：唯一读该字段的是 `PhysicsContactQueueComponent.Add` 的去重比较
  （`existing.Substep == contact.Substep`）；而捕获每 tick 只跑一次、每对实体至多产出一条，
  该比较实际不承担语义，属潜伏不一致；
- 一旦有消费方按子步排序混用两类事实，实体接触永远排在子步 0，顺序判定不可信；
- 与瓦片接触的语义差异会被后续维护者当成"设计如此"，长期固化错误语义。

## 7. 闭合口径

二选一，并在文档里写明理由：

- (a) 把实体碰撞检测搬进子步循环（注意结构变更约束：不能在迭代查询里 `World.Create/Destroy`），
  使 `Substep` 取真实值；或
- (b) 明确 `PhysicsContactFact.Substep` 为"位移后整体检测"语义并**删掉该字段**，
  不再假装与瓦片一致。

当前倾向 (b)：实体接触不需要子步精度，删字段比造子步循环成本低，且消除语义歧义。
两条路都必须新增断言固定最终语义。

## 8. 涉及文件

- `src/NSSLC/Share/Physics/ContactFact/PhysicsContactCaptureSystem.cs`（`substep: 0` 硬编码）
- `src/NSSLC/Share/Physics/ContactFact/System/PhysicsContactFact.cs`
- `src/NSSLC/Share/Physics/SpatialSimulation/SpatialSimulationSystem.cs`（子步循环与瓦片接触产生点）
- `src/NSSLC/Share/Physics/SpatialSimulation/SpatialSimulationCoordinatorSystem.cs`（写 `TileContactFact.Substep`）
- `src/NSSLC/Share/Physics/SpatialSimulation/System/SpatialCommit.cs`

## 9. 状态历史

- 2026-11-24：P3/P4 收尾发现，记录为 open。

关闭后本文件删除（按 [项目文件组织设计约束](../../Context/架构约束/项目文件组织设计约束.md) §3.3，缺陷不转归档）。
