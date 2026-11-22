# Player、NPC、Projectile 共享组件合并

本次把四组重复模型收敛到按能力归属的共享组件。Player、NPC 和 Projectile 仍通过 Arch
组件组合表达实体类别，不建立新的实体聚合对象。

## 防御与减伤

- 删除 `PlayerDefenseStateComponent`，不再为 Player 保存独立的 Defense、Resistance 或保护窗口。
- `AttributeSetComponent` 是 Defense、DamageReduction、Endurance 等数值属性的唯一权威来源。
- 新增 `ProtectionWindowComponent` 和 `ProtectionWindowSystem`；保护窗口是 Combat 准入状态，
  不是属性数值。
- 删除 `HarmReductionComponent`。`DamageMitigationSystem` 直接解析目标
  `AttributeSetComponent`，没有 Attributes 的实体按零减伤处理。
- `ProjectileCombatCommitSystem` 对所有目标使用同一 Attributes 读取路径，并按目标是否拥有
  `ProtectionWindowComponent` 执行共享保护窗口检查。

## 碰撞层和掩码

`ProjectileCollisionPolicyComponent` 只保留 Tile/Entity 碰撞开关、Owner 排除和同阵营排除。
`Layer`、`Mask`、`Enabled` 统一由共享 `EntityCollisionCapabilityComponent` 提供。Projectile
定义现在同时携带自己的碰撞策略和通用碰撞能力；碰撞候选查询从两者读取对应字段。

## AI 行为选择

- 删除 `NpcBehaviorComponent` 和 `ProjectileBehaviorComponent`。
- 新增 `AiBehaviorSelectionComponent`，统一保存 `AiBehaviorId` 与 `AiBehaviorProfileId`。
- NPC 的 `NpcBehaviorStateComponent`、Projectile 的 `ProjectileBehaviorStateComponent` 以及
  Homing/Bounce 状态继续保留，因为它们的运行语义不同。
- `AiComponent` 仍只负责运行时 C# 行为绑定；选择键和运行时类型不是同一事实。
- Projectile profile/catalog 改用共享 AI 键类型，Projectile 适配器只在内容目录边界转换为
  Projectile 专属行为实现。

## 验证

已运行：

```text
dotnet build src/NSSLC/Terraria.Components.csproj --no-restore
dotnet build .agent-workplace/buff-smoke/Smoke.csproj --no-restore
dotnet run --project .agent-workplace/buff-smoke/Smoke.csproj --no-build --no-restore
git diff --check
```

主项目和 smoke 项目均为 0 警告、0 错误；smoke 输出包含 Buff/NPC 和 Projectile ECS 两条验证
通过。生产源码和 smoke 中已无旧四组组件的引用。
