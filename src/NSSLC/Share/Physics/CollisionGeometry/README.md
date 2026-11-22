# CollisionGeometry 碰撞几何

回答"实体占什么形状、能否相撞、交点与法线怎么算"。不涉及世界状态提交。

| 类型 | 角色 |
| --- | --- |
| `ColliderComponent` | 形状、尺寸与偏移（持续组件） |
| `ColliderShape` | 形状枚举 |
| `EntityCollisionCapabilityComponent` | 实体侧碰撞层与掩码（能力） |
| `TileCollisionCapabilityComponent` | 实体与 Tile 碰撞的资格与规则（能力） |
| `EntityCollisionContact` | 交点和法线的结果值 |
| `EntityCollisionSystem` | 纯几何服务：校验层掩码、算 AABB 相交、返回接触 |

`EntityCollisionSystem` 不决定谁受伤、谁被击退、是否触发武器效果或是否删除实体；
这些分别由 Combat、Interaction 和生命周期系统提交。

命名空间：`Terraria.Physics`。
