# Projectile.cs 字段/属性 ECS 迁移：Flowstate 重要节点版

**日期：** 2026-08-28  **状态：** `partial`  **策略：** Flowstate `spec`（B73 后续迭代）  
**上下文预算：** `10-percent-key-nodes`  **测试预算：** `30-percent-high-value-boundaries`  
**Oracle：** `D:\TRbackup\Version4物理删除了某些文件\Terraria\Projectile.cs`  
**SHA-256：** `97C3D68586AEF862411699E428F5C4B18CEE1F8051B684BE9F86FA7B60FAE83B`  
**详细基线：** [`2026-08-28-field-property-migration-comparison.md`](2026-08-28-field-property-migration-comparison.md)

## 一句话结论

旧 `Projectile` 同时承担实例、默认值、AI、战斗、网络和客户端表现；ECS 只保留权威状态链。按总报告口径，旧文件为 **120 字段 + 9 属性**，排除 `identity` 后为 **128 个非 ID 声明**，模型迁移保守估算 **约 38%，仍未完成 parity**。

## Flowstate 当前节点（只保留抓手）

```text
N4/N8 迭代
  -> B73 生命周期合同（已部分验证）
  -> Definition/Spawn
  -> Behavior/Motion
  -> Collision/Damage/Penetration
  -> Lifetime/Tombstone
  -> Snapshot/Protocol 投影
  -> N6 验收：保持 partial，直到全量类型/AI/协议证据齐全
```

| 重要节点 | 当前 owner | 状态 | 验收抓手 |
| --- | --- | --- | --- |
| 类型/默认值 | `ProjectileDefinition`、`ProjectileDefinitionRegistry` | 部分 | 正 type/behavior、有限 collider、lifetime、damage、penetration；完整 `SetDefaults` 表延期 |
| 生成/归属 | `ProjectileSpawnSystem`、`ProjectileOwnerComponent` | 已承载 | owner/type/数值校验后再分配实体；NPC owner 走独立路径 |
| 行为/AI | `ProjectileBehaviorComponent` + typed behaviors | 部分 | linear、gravity、已审计 aiStyle 家族；`ai[0..2]` 仅协议投影 |
| 运动/碰撞 | `TransformComponent`、`VelocityComponent`、`ColliderComponent`、CollisionSystem | 部分 | 有限值运动、固体扫掠停止；坡面/液体/特殊 tile/完整反射延期 |
| 战斗 | `ProjectileDamageComponent`、Penetration、DamageSystem | 部分 | friendly→NPC、1-hit 穿透、部分 area/status；local immunity 与 hostile-player 全路径延期 |
| 生命周期 | `ProjectileLifetimeComponent`、tombstone policy | 已承载 | 零/负寿命拒绝；递减到零发布 inactive/tombstone |
| 复制 | `ProjectileReplicationSnapshot`、V1456 projector | 部分 | 存活实体、正 replication ID、revision、Definition/Behavior 一致性；cadence/重连 UUID 延期 |

## 字段/属性归并（非 ID；重要语义）

| 旧成员族 | ECS 归属 | 判定 |
| --- | --- | --- |
| `active`、`timeLeft`、`numUpdates`、`extraUpdates` | Lifetime component + tick policy | `timeLeft` 已承载；extra-update 仅部分 |
| `type`、`aiStyle`、`scale`、`stepSpeed` | Definition + behavior definition | type 已承载；完整类型表延期 |
| `owner`、`npcProj`、`minion*`、`sentry`、`trap` | Owner/marker/capability components | player/NPC owner 已分离；专用组合延期 |
| `damage`、`originalDamage`、`knockBack`、`friendly`、`hostile`、`penetrate` | Definition + damage/penetration components | friendly→NPC、penetration 已验证；PVP/免疫/修正延期 |
| `tileCollide`、`ignoreWater`、`reflected`、`correctSlopeCollision` | Collision/liquid/bounce policy | 固体停止与部分 bounce；完整 TileCollision/liquid/slope 延期 |
| `ai[]`、`localAI[]`、`localNPCImmunity`、`playerImmune` | Typed behavior / hit-immunity resources | 不复制数组；完整 AI 和 immunity 延期 |
| `netUpdate*`、`netSpam`、`netSyncSkippedForPlayer`、`NetSectionCoordinates` | Revision/PVS/session projection | PVS/cursor/section 部分承载；旧 cadence/netSpam 延期 |
| `alpha`、`light`、`rotation`、`frame*`、`oldPos/oldRot`、`drawLayer`、`hide`、`Name` | Client presentation/content adapter | 服务端 ECS `excluded`，不计为迁移缺口 |
| 静态免疫表、目标 List、javelin/lightning/whip/kite 缓存 | Definition/resource/query/专用 behavior | 不进入实体；大多延期或表现排除 |

## 属性处理

| 属性 | 处理 |
| --- | --- |
| `Name`、`Opacity` | 客户端内容/表现适配；`excluded` |
| `WipableTurret`、`OwnerMinionAttackTargetNPC`、`CareForAttackCD` | 等待 Sentry、Targeting、HitCooldown 权威合同；`deferred` |
| `MaxUpdates`、`OwnedBySomeone`、`NetSectionCoordinates` | 分别映射 tick policy、ownership policy、snapshot section；`partial` |

## Flowstate DoD / 30% 窄验证

- [x] 工作区已有 `.agent-workplace`；过程态使用 `state/projectile-lifecycle-task.json`，不新增代码。
- [x] 重要节点保留：Definition → Spawn → System → Commit → Snapshot/Protocol。
- [x] `identity` 只作为协议身份契约保留，不计入统计；不删除 `type/owner/projUUID`。
- [x] 保留 `partial/deferred/excluded`，未把边界卡或 LegacyReference 当成 parity。
- [x] 文档覆盖检查：Oracle 非 ID 字段名全部能在本文件或详细基线中定位。
- [x] 文档格式检查：`git diff --check` 退出 0。
- [ ] 完整 Projectile type/AI、immunity、specialized mechanics、packet cadence：下一迭代，不在本批伪完成。

## 下一批（B74 候选）

1. 只选一个高价值族：先补 `local/static NPC immunity + restrike` 的 typed contract；
2. 每批新增一个 source anchor、一个最小 verifier、一个 snapshot/protocol 证据；
3. 仅运行约 30% 高价值检查：文档/schema、Simulation 定向 build、Projectile Combat verifier；不跑全量回归；
4. 通过后更新 `.agent-workplace/state/projectile-lifecycle-task.json`，再进入 N6/N8 评审。

## 最终状态

**Field/property migration：`partial`，约 38%（128 个非 ID 声明的既有加权估算）。** 这是权威核心切片的 Flowstate 管理记录，不是完整 Terraria Projectile 等价实现。
