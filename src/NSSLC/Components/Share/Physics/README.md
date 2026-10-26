# Physics 与空间模拟

`Physics` 定义跨实体复用的几何、碰撞和空间提交契约。它只依赖抽象的
`ISpatialWorldQuery`，所以不会把 `WorldOwner`、Tile 数组、Arch `World` 或网络连接带入
组件。

## 状态与能力

```text
Entity.LocationComponent
Movement.VelocityComponent
ColliderComponent
GroundingComponent
LiquidContactComponent
        ↑ SpatialSimulationCoordinatorSystem 唯一常规空间写入者

MovementCapabilityComponent
GravityCapabilityComponent
TileCollisionCapabilityComponent
EntityCollisionCapabilityComponent
LiquidInteractionCapabilityComponent
        ↑ 定义实体可以采用哪些规则
```

能力组件只声明配置和资格，不保存系统实例、回调或世界引用。`ColliderComponent` 只描述
形状、尺寸和偏移，碰撞响应由系统计算。

## 一次空间帧

`SpatialFrame` 携带 tick、世界代次、世界版本和只读查询端口。`SpatialEntitySnapshot<TEntity>`
是从 ECS 复制出的短生命周期输入，包含位置、速度、碰撞体和上一次接触结果。世界快照不
作为长期实体组件保存。`SpatialSimulationCoordinatorSystem` 使用一个 Arch 组合查询捕获
这些输入，调用纯空间规则并在同一查询回调内提交空间结果。

```text
SpatialFrame + SpatialEntitySnapshot
        ↓
SpatialSimulationSystem.Simulate
        ├─ 意图与速度限制
        ├─ 跳跃与重力
        ├─ Tile 运动查询
        ├─ 实体 AABB 接触查询
        ├─ 液体接触查询
        └─ 接地结果
        ↓ SpatialSimulationResult
SpatialSimulationCoordinatorSystem
        ↓
Arch Location / Velocity / Grounding / LiquidContact
```

## 提交不变量

`SpatialSimulationCoordinatorSystem` 是 Arch 空间状态的唯一常规写入端口。它在一次查询中
读取完整基础组合，并在提交前确认世界 revision 未发生变化，然后一起写入 `Location`、
`Velocity`、`Grounding` 和已有的 `LiquidContact`。实体级空间 revision 不再建模为组件；
需要跨帧冲突控制时使用统一系统或世界快照元数据。

因此，网络包、AI、战斗和表现层都不能直接写 `LocationComponent` 或
`VelocityComponent`。传送是非连续位移，应使用独立的传送命令和提交规则，不伪装成普通
速度。

`EntityCollisionSystem` 负责共享几何碰撞：验证 Collider/Layer/Mask，计算 AABB 是否相交，
并返回接触点和法线。空间模拟可用 `FindContacts` 得到 `SpatialContactPair<TEntity>`；
其他领域系统可用 `TryGetContact` 复用同一套几何规则。它不决定谁受伤、谁被击退、
是否触发武器效果或是否删除实体；这些结果分别由 Combat、Interaction 和生命周期系统提交。

## 当前实现边界

这些类型是共享层合同和确定性计算，不代表已经接入当前 `MinimalImplementation` 服务端。
现有 `WorldSpatialSnapshot.Move`、NPC motion snapshot 和 Projectile movement service 可以
分别实现 `ISpatialWorldQuery` 适配器。接入顺序建议为 NPC、WorldItem、Player、Projectile；
Projectile 的 extra updates、穿透、命中、寿命和归属仍由 Projectile 专属系统处理。

## 不建模为长期组件的内容

- Tile 扫描 scratch、临时接触列表和一次查询的命中结果；
- `WorldSpatialSnapshot` 或 Arch `World` 引用；
- `oldPosition`、表现位置、网络位置和客户端预测位置；
- 跨实体碰撞的伤害结算；
- 传送、结构性 Tile 修改和实体销毁。

这些内容分别属于查询生命周期、投影、Combat、WorldInteraction 或实体生命周期边界。

Physics 不要求实体具备 LifeComponent、EntityFactionComponent 或 Attributes；死亡、阵营和伤害准入由生命周期与 Combat 系统解释。若死亡实体需要立即退出空间，由生命周期或碰撞能力 owner 禁用 Collider，不能把 Life 重新耦合进 Physics 查询。
