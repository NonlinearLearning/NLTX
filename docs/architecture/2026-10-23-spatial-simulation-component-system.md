# 空间模拟、移动与物理组件系统

状态：共享层第一阶段已实现；尚未接入 `MinimalImplementation` 服务器调度。

## 目标

把实体空间状态、移动能力、移动意图、世界查询和最终提交分开，使 Player、NPC、Projectile
和 WorldItem 能共享几何规则而不共享各自不同的生命周期、AI、战斗或网络状态。

## 核心边界

```text
Component     保存状态或能力声明
Intent        表示本 tick 的请求
Snapshot      提供本次计算的只读输入
System        计算规则和结果
Commit port   在 revision/world version 校验后集中写入
Projection    读取提交结果并生成网络或存档数据
```

共享代码位于 `src/NSSLC/Share/` 的 `Physics` 等领域，采用 `LocationComponent` 作为公共位置名称，
`VelocityComponent` 位于 `Terraria.Movement`，`ColliderComponent` 位于
`Terraria.Physics`。它没有复制最新客户端的字段、协议包命名或客户端业务对象。

## 系统协作

1. `PlayerInputSystem` 和 `AiEntitySystem` 产生 `MovementIntent`。
2. `MovementIntentResolver` 按 tick 和优先级选择请求。
3. `GravityModifierSystem` 维护修饰器并生成 `GravityStateComponent`。
4. `SpatialSimulationSystem` 使用 `SpatialFrame`、实体快照和 `ISpatialWorldQuery` 计算
   速度、Tile 位移、液体接触和接地结果。
5. `SpatialSimulationCoordinatorSystem` 使用 Arch 组合查询完成捕获、调用纯计算并在同一
   查询回调中原子更新位置、速度和接触状态。
6. Combat、Interaction、Replication 和 Persistence 只读取提交后的状态。

## 当前代码的接入方式

当前服务端已有 `WorldSpatialSnapshot`、NPC motion snapshot 和 Projectile movement service。
它们应通过适配器实现 `ISpatialWorldQuery`，而不是把世界对象放进组件。Player 的输入、坐骑、
便携凳、抓钩和重力状态需要先拆成能力/意图/事实快照，再接入统一提交端口。

## 一致性

每个空间结果带 `SpatialWorldRevision`。提交前世界版本失效时必须拒绝整次提交；不能只写
位置而留下旧速度或旧液体状态。实体级空间 revision 不作为组件保存；`SpatialFrame` 是
一次 tick 的短期输入，也不应保存为实体组件。

## 实现状态

本次新增的是共享层组件、确定性空间计算和提交接口；没有修改 Player/NPC/Projectile owner，
没有改变协议或存档格式，也没有运行测试或构建。正式运行时接入前仍需补行为验证，尤其是
Tile 斜坡/平台、液体接触、重力反向、版本冲突和多实体批次边界。
