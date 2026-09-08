# Version4 有效组件字段和属性统计报告

## 1. 统计结论

本报告将“有效组件”定义为：出现在 canonical 目标索引
`docs/迁移参考表/Version4-component-target-index.json` 中，按完整限定类型名去重，目标文件位于当前
`src/`，并且能够被 Roslyn 精确解析出对应类型和目标成员的组件。未进入该显式目标索引的
`Component.cs`、`State.cs` 候选文件，以及 `dome/src` 中的文件，不自动计入本统计。

| 统计口径 | 有效组件/类型 | 字段 | 属性 | 字段+属性 |
|---|---:|---:|---:|---:|
| 25 个有效组件源码内实际声明 | **25** | **133** | **21** | **154** |
| canonical 目标 manifest 显式绑定成员 | 25 | **78** | **3** | **81** |
| Version4 去除 ID 类文件的源成员基线 | — | **6,414** | **787** | **7,201** |

因此，若“字段和属性数量”指当前 25 个有效组件文件中的全部直接声明，结果是：

> **有效组件 25 个；字段 133 个；属性 21 个；合计 154 个。**

若“字段和属性数量”指迁移账本显式纳入的目标成员，结果是：

> **25 个组件类型；目标字段 78 个；目标属性 3 个；合计 81 个。**

这两组数字不能混称：81 是迁移目标 manifest 的显式成员子集，154 是这 25 个组件源码中的全部直接字段/属性声明。

## 2. 有效组件边界

### 2.1 纳入条件

- 来源是 `docs/迁移参考表/Version4-component-target-index.json` 的 `targetMembers`，而不是按文件名后缀猜测。
- `componentType` 按完整限定类型名去重；25 个类型对应 25 个不同的目标文件。
- 目标文件路径均在当前仓库 `src/` 下并且存在。
- 每个文件均解析出 1 个与 manifest 类型名对应的组件类型；25 个文件的 Roslyn 语法诊断均为 0。
- 目标 manifest 的 81 条成员均能被目标扫描器精确解析；不存在缺失目标成员。

### 2.2 成员计数规则

- **字段**：统计组件类型直接声明的 C# 字段，包含 `public`、`private`、`readonly` 和 `const` 字段；不统计继承字段、编译器生成的 backing field、构造函数参数或局部变量。
- **属性**：统计组件类型直接声明的普通属性，包含表达式主体属性和带 `get`/`set` 的属性；不统计继承属性。
- 构造函数、方法、事件、嵌套类型不计入字段/属性数量。
- `LeashedKiteBehaviorComponent.DefaultKiteDistance` 是 C# `const` 字段，因此按字段计数。
- `PylonRegistryComponent` 的私有存储字段也属于类型直接声明字段，因此计入 4 个字段；其 6 个直接属性也全部计入源码声明统计。

### 2.3 迁移状态边界

目标索引成员来源分布如下：

| origin | 成员数 |
|---|---:|
| migrated | 78 |
| compatibility | 2 |
| new | 1 |
| 合计 | **81** |

25 个组件中，24 个组件至少含有一个 `migrated` 或 `new` 目标成员；
`Terraria.LeashedEntity.LeashedEntityLegacySlotComponent` 是 1 个仅含
`compatibility` 目标成员的兼容性组件。它仍然属于显式目标组件范围，但不能据此宣称已经完成权威迁移。

## 3. 按组件明细

“源码字段/属性”是当前组件类型的全部直接声明；“manifest 字段/属性”是 canonical 目标索引中显式绑定的成员。

| 完整组件类型 | 文件 | 源码字段 | 源码属性 | 源码合计 | manifest 字段 | manifest 属性 | manifest 合计 |
|---|---|---:|---:|---:|---:|---:|---:|
| `Terraria.Fishing.BobberTimingComponent` | `src/Fishing/BobberTimingComponent.cs` | 5 | 1 | 6 | 1 | 0 | 1 |
| `Terraria.Fishing.FishingAttemptStateComponent` | `src/Fishing/FishingAttemptStateComponent.cs` | 6 | 1 | 7 | 1 | 0 | 1 |
| `Terraria.Fishing.FishingBaitReservationComponent` | `src/Fishing/FishingBaitReservationComponent.cs` | 6 | 1 | 7 | 1 | 0 | 1 |
| `Terraria.LeashedEntity.LeashedCritterBehaviorComponent` | `src/LeashedEntity/LeashedCritterBehaviorComponent.cs` | 7 | 1 | 8 | 5 | 0 | 5 |
| `Terraria.LeashedEntity.LeashedEntityAnchorRelationComponent` | `src/LeashedEntity/LeashedEntityAnchorRelationComponent.cs` | 4 | 1 | 5 | 1 | 0 | 1 |
| `Terraria.LeashedEntity.LeashedEntityLegacySlotComponent` | `src/LeashedEntity/LeashedEntityLegacySlotComponent.cs` | 2 | 1 | 3 | 1 | 0 | 1 |
| `Terraria.LeashedEntity.LeashedEntityLifecycleComponent` | `src/LeashedEntity/LeashedEntityLifecycleComponent.cs` | 3 | 2 | 5 | 1 | 0 | 1 |
| `Terraria.LeashedEntity.LeashedEntityStateComponent` | `src/LeashedEntity/LeashedEntityStateComponent.cs` | 1 | 0 | 1 | 1 | 0 | 1 |
| `Terraria.LeashedEntity.LeashedKiteBehaviorComponent` | `src/LeashedEntity/LeashedKiteBehaviorComponent.cs` | 10 | 0 | 10 | 2 | 0 | 2 |
| `Terraria.Projectile.ProjectileCollisionPolicyComponent` | `src/Projectile/ProjectileCollisionPolicyComponent.cs` | 12 | 0 | 12 | 8 | 0 | 8 |
| `Terraria.Projectile.ProjectileDamagePayloadComponent` | `src/Projectile/ProjectileDamagePayloadComponent.cs` | 11 | 0 | 11 | 11 | 0 | 11 |
| `Terraria.Projectile.ProjectileDefinitionComponent` | `src/Projectile/ProjectileDefinitionComponent.cs` | 7 | 3 | 10 | 3 | 0 | 3 |
| `Terraria.Projectile.ProjectileGeometryStateComponent` | `src/Projectile/ProjectileGeometryStateComponent.cs` | 2 | 0 | 2 | 2 | 0 | 2 |
| `Terraria.Projectile.ProjectileHitImmunityPolicyComponent` | `src/Projectile/ProjectileHitImmunityPolicyComponent.cs` | 7 | 0 | 7 | 6 | 0 | 6 |
| `Terraria.Projectile.ProjectileHitImmunityStateComponent` | `src/Projectile/ProjectileHitImmunityStateComponent.cs` | 3 | 0 | 3 | 3 | 0 | 3 |
| `Terraria.Projectile.ProjectileIdentityComponent` | `src/Projectile/ProjectileIdentityComponent.cs` | 4 | 0 | 4 | 3 | 0 | 3 |
| `Terraria.Projectile.ProjectileLifetimeComponent` | `src/Projectile/ProjectileLifetimeComponent.cs` | 2 | 2 | 4 | 1 | 0 | 1 |
| `Terraria.Projectile.ProjectileNetworkStateComponent` | `src/Projectile/ProjectileNetworkStateComponent.cs` | 6 | 0 | 6 | 5 | 0 | 5 |
| `Terraria.Projectile.ProjectilePenetrationStateComponent` | `src/Projectile/ProjectilePenetrationStateComponent.cs` | 4 | 0 | 4 | 4 | 0 | 4 |
| `Terraria.Projectile.ProjectileSourceMetadataComponent` | `src/Projectile/ProjectileSourceMetadataComponent.cs` | 7 | 0 | 7 | 5 | 0 | 5 |
| `Terraria.Projectile.ProjectileTrailCacheComponent` | `src/Projectile/ProjectileTrailCacheComponent.cs` | 4 | 0 | 4 | 3 | 0 | 3 |
| `Terraria.Projectile.ProjectileTrajectoryStateComponent` | `src/Projectile/ProjectileTrajectoryStateComponent.cs` | 10 | 0 | 10 | 8 | 0 | 8 |
| `Terraria.Teleportation.PortalLinkStateComponent` | `src/Teleportation/PortalLinkStateComponent.cs` | 2 | 1 | 3 | 1 | 0 | 1 |
| `Terraria.Teleportation.PortalTraversalCooldownStateComponent` | `src/Teleportation/PortalTraversalCooldownStateComponent.cs` | 4 | 1 | 5 | 1 | 0 | 1 |
| `Terraria.WorldStorage.PylonRegistryComponent` | `src/WorldStorage/PylonRegistryComponent.cs` | 4 | 6 | 10 | 0 | 3 | 3 |
| **合计** | **25 个组件** | **133** | **21** | **154** | **78** | **3** | **81** |

## 4. 按领域汇总

| 目标领域（命名空间第二段） | 组件数 | 源码字段 | 源码属性 | 源码合计 | manifest 字段 | manifest 属性 | manifest 合计 |
|---|---:|---:|---:|---:|---:|---:|---:|
| `Fishing` | 3 | 17 | 3 | 20 | 3 | 0 | 3 |
| `LeashedEntity` | 6 | 27 | 5 | 32 | 11 | 0 | 11 |
| `Projectile` | 13 | 79 | 5 | 84 | 62 | 0 | 62 |
| `Teleportation` | 2 | 6 | 2 | 8 | 2 | 0 | 2 |
| `WorldStorage` | 1 | 4 | 6 | 10 | 0 | 3 | 3 |
| **合计** | **25** | **133** | **21** | **154** | **78** | **3** | **81** |

## 5. 来源与验证记录

### 5.1 输入来源

| 输入 | 用途 | SHA-256 |
|---|---|---|
| `docs/迁移参考表/Version4-component-target-index.json` | canonical 目标索引和组件范围 | `0ee34993b67ec92b6a61208d864d56cad143dec7aa26ee4e336989b56ddd2364` |
| `Build/generated/nltx-target-manifest.json` | 目标 Roslyn scanner 的显式 manifest 输入 | `7c112840b47df8250e9f61026c9217c996011ad56c901ebc9b31af9896e5369e` |
| `docs/迁移参考表/Version4字段属性按系统子系统拆分报告-去除ID类文件.md` | Version4 字段/属性总量基线 | `85cc6b9429d4edce38c09cb566625e9dd315d12c8b5e6a9871e889f5e1c44033` |

### 5.2 实际验证

- 目标 scanner：`Version4MemberMigrationScanner.exe --mode target`；退出码 `0`。
- scanner 结果：`complete`；解析文件 `25`；精确命中 manifest 成员 `81`；诊断 `0`。
- Roslyn 语法盘点：25 个组件类型声明，25 个类型声明与目标类型一一对应；语法诊断 `0`。
- 独立算术复核：`133 + 21 = 154`，`78 + 3 = 81`，Version4 基线 `6,414 + 787 = 7,201`。
- `Build/generated/Version4-component-target-index.json` 当前是旧的 65 条成员视图，不作为本次 canonical 统计输入；本报告使用 `docs/迁移参考表` 下的 81 条 canonical 索引及其对应 manifest。

## 6. 解释边界

本报告是当前源码声明和迁移目标索引的数量盘点，不是迁移完成、行为等价、权威转移、网络/持久化闭合或运行时可用性证明。目标索引中的 `migrated`、`compatibility`、`new` 只表示 manifest 的来源/边界分类；是否达到 `verified` 仍需按
`version4-member-migration-ledger` 的证据、快照哈希和验证器契约单独审计。
