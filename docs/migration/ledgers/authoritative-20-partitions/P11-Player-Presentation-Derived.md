# P11 玩家表现、动画、外观与派生属性 - 权威模拟系统成员组件候选分区

> 来源：`docs/migration/ledgers/Version4权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md`；本文件只做成员库存分区和候选组件边界记录。
> 这是候选组件拆分台账，不是运行时迁移完成、行为等价、API 兼容、网络闭合或持久化闭合证明。

## 1. 分区边界

本分区包含 18 个叶子子系统，字段 91 条、属性 73 条、成员合计 164 条；每个叶子子系统保持原正式父级归属，不跨分区拆行。

## 2. 分区覆盖概览

| 叶子子系统 | 正式父级 | 字段 | 属性 | 合计 | 源边界角色 |
|---|---|---:|---:|---:|---|
| `PlayerPoseAndAnimationState` | `PlayerGameplay` | 14 | 0 | 14 | authoritative state/behavior |
| `PlayerNetworkCameraState` | `PlayerGameplay` | 3 | 0 | 3 | registry/projection |
| `PlayerShadowAndArmPresentation` | `PlayerGameplay` | 13 | 0 | 13 | registry/projection |
| `PlayerVisualAndShaderEffects` | `PlayerGameplay` | 21 | 0 | 21 | registry/projection |
| `PlayerFootballPresentationState` | `PlayerGameplay` | 2 | 0 | 2 | presentation state |
| `PlayerAppearanceCustomizationState` | `PlayerGameplay` | 10 | 0 | 10 | authoritative state/behavior |
| `PlayerTraversalColorProjection` | `PlayerGameplay` | 6 | 0 | 6 | registry/projection |
| `PlayerAppearanceCompanionAndEffectProjection` | `PlayerGameplay` | 11 | 0 | 11 | registry/projection |
| `PlayerSpatialDerivedProperties` | `PlayerGameplay` | 0 | 9 | 9 | derived/query |
| `PlayerIdentityAndDerivedProperties` | `PlayerGameplay` | 0 | 2 | 2 | derived/query |
| `PlayerBiomeZoneProperties` | `PlayerGameplay` | 0 | 16 | 16 | derived/query |
| `PlayerVerticalAndWeatherZoneProperties` | `PlayerGameplay` | 0 | 6 | 6 | derived/query |
| `PlayerEventAndShoppingZoneProperties` | `PlayerGameplay` | 0 | 7 | 7 | derived/query |
| `PlayerInteractionAndSelectionProperties` | `PlayerGameplay` | 0 | 9 | 9 | derived/query |
| `PlayerAbilityAndPresentationProperties` | `PlayerGameplay` | 0 | 12 | 12 | derived/query |
| `PlayerItemMountAndRuntimeProperties` | `PlayerGameplay` | 0 | 11 | 11 | derived/query |
| `PlayerEyeAnimationState` | `PlayerGameplay` | 3 | 1 | 4 | registry/projection |
| `PlayerPresentationMessagesAndArms` | `PlayerGameplay` | 8 | 0 | 8 | authoritative state/behavior |

## 3. 候选组件归属与 seam

| 叶子子系统 | 候选组件边界 | 类型分类 | 最小 seam | 独立生命周期 | 证据状态 | 主要风险 |
|---|---|---|---|---|---|---|
| `PlayerPoseAndAnimationState` | `PlayerPoseAndAnimationStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerNetworkCameraState` | `PlayerNetworkCameraStateProjection`/`Adapter` | Projection/Adapter | 单向 Projection/Adapter seam；不得回写权威状态 | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerShadowAndArmPresentation` | `PlayerShadowAndArmPresentationProjection`/`Adapter` | Projection/Adapter | 单向 Projection/Adapter seam；不得回写权威状态 | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerVisualAndShaderEffects` | `PlayerVisualAndShaderEffectsProjection`/`Adapter` | Projection/Adapter | 单向 Projection/Adapter seam；不得回写权威状态 | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerFootballPresentationState` | `PlayerFootballPresentationStateProjection`/`Adapter` | Projection/Adapter | 单向 Projection/Adapter seam；不得回写权威状态 | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerAppearanceCustomizationState` | `PlayerAppearanceCustomizationStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerTraversalColorProjection` | `PlayerTraversalColorProjectionProjection`/`Adapter` | Projection/Adapter | 单向 Projection/Adapter seam；不得回写权威状态 | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerAppearanceCompanionAndEffectProjection` | `PlayerAppearanceCompanionAndEffectProjectionProjection`/`Adapter` | Projection/Adapter | 单向 Projection/Adapter seam；不得回写权威状态 | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerSpatialDerivedProperties` | `PlayerSpatialDerivedPropertiesQuery` | Query | 纯资格或派生 Query；只读 | 候选纯读边界；仍需核对失效/缓存条件 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerIdentityAndDerivedProperties` | `PlayerIdentityAndDerivedPropertiesQuery` | Query | 纯资格或派生 Query；只读 | 候选纯读边界；仍需核对失效/缓存条件 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerBiomeZoneProperties` | `PlayerBiomeZonePropertiesQuery` | Query | 纯资格或派生 Query；只读 | 候选纯读边界；仍需核对失效/缓存条件 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerVerticalAndWeatherZoneProperties` | `PlayerVerticalAndWeatherZonePropertiesQuery` | Query | 纯资格或派生 Query；只读 | 候选纯读边界；仍需核对失效/缓存条件 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerEventAndShoppingZoneProperties` | `PlayerEventAndShoppingZonePropertiesQuery` | Query | 纯资格或派生 Query；只读 | 候选纯读边界；仍需核对失效/缓存条件 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerInteractionAndSelectionProperties` | `PlayerInteractionAndSelectionPropertiesQuery` | Query | 纯资格或派生 Query；只读 | 候选纯读边界；仍需核对失效/缓存条件 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerAbilityAndPresentationProperties` | `PlayerAbilityAndPresentationPropertiesQuery` | Query | 纯资格或派生 Query；只读 | 候选纯读边界；仍需核对失效/缓存条件 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerItemMountAndRuntimeProperties` | `PlayerItemMountAndRuntimePropertiesQuery` | Query | 纯资格或派生 Query；只读 | 候选纯读边界；仍需核对失效/缓存条件 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerEyeAnimationState` | `PlayerEyeAnimationStateProjection`/`Adapter` | Projection/Adapter | 单向 Projection/Adapter seam；不得回写权威状态 | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerPresentationMessagesAndArms` | `PlayerPresentationMessagesAndArmsComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |

## 4. 分区内依赖方向

- 输入、网络命令或世界配置先进入意图/资格 Query，再由本分区的唯一 Owner System 通过 Command/CommitPort 写入权威 Component。
- `derived/query` 和 `definition/query` 只能读权威状态或定义；`registry/projection`、快照、复制和表现边界只做单向输出。
- 跨分区交接使用只读 Query、显式 Command、Event 或 Projection；Markdown 文件顺序不表达运行时执行顺序。

## 5. 拆分前证据缺口

- 源报告确认了成员、声明类型、来源路径、行列、字段/属性及候选细分归属，但没有闭合每个成员的完整读者、写者、创建/清理/持久化/网络生命周期。
- `confirmed` 仅表示 `source-inventory-confirmed`；组件是否需要拆成多个结构、是否为缓存或兼容投影，必须在迁移前补充 Version4 调用点和 focused verifier 证据。
- 外部 SS14 证据只用于组件/System/Query 的组织粒度；tModLoader `v2026.07` 只用于公开 API 边界交叉参考，不替代 Version4 私有语义证据。

## 6. 兼容策略与验证计划

- 兼容策略：先保持 Version4 原始声明、公共 API、命名空间和外部类型边界不变；每次只迁移一个叶子边界，并以 Adapter/Projection 保留旧读路径，确认新 Owner System 的写入闭合后再移除兼容层。
- 暂不拆分项：源报告已经按声明类型、生命周期或访问边界分开的叶子组不再按字段数量机械切块；`Actions`、`WorkItem` 和短生命周期参数保持 Command payload，定义/catalog/profile/rule 保持只读 Definition/Query。
- focused verifier：权威状态验证状态转换、唯一写者、重复 Command 和清理边界；纯 Query 验证确定性与无写回；Projection/Adapter 验证单向输出；所有成员迁移批次验证来源序号、声明行和原始类型闭包。
- 验证状态：以上是迁移前计划；本分区只完成成员库存逐行一致性检查，未执行 C# 编译、运行时行为、网络复制或持久化恢复测试。

## 7. 逐成员源码声明

### 正式父级子系统：`PlayerGameplay`
- 父级职责：沿用源报告正式父级 `PlayerGameplay`；本分区只收录该父级中列出的叶子子系统。
- 本分区父级局部统计：字段 91；属性 73；合计 164；完整父级统计以源报告为准。

#### 4.13.32 细分子系统：`PlayerPoseAndAnimationState`

- 细分职责：身体姿态、位置速度和动画偏移状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；姿态更新由帧行为系统集中提交。
- 成员文件数：1；声明类型数：1；字段：14；属性：0；合计：14。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（14）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 751 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1099 | 2 | headRotation | float | `public float headRotation;` | `public float headRotation;` |
| 752 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1101 | 2 | bodyRotation | float | `public float bodyRotation;` | `public float bodyRotation;` |
| 753 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1103 | 2 | legRotation | float | `public float legRotation;` | `public float legRotation;` |
| 754 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1105 | 2 | headPosition | Vector2 | `public Vector2 headPosition;` | `public Vector2 headPosition;` |
| 755 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1107 | 2 | bodyPosition | Vector2 | `public Vector2 bodyPosition;` | `public Vector2 bodyPosition;` |
| 756 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1109 | 2 | legPosition | Vector2 | `public Vector2 legPosition;` | `public Vector2 legPosition;` |
| 757 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1111 | 2 | headVelocity | Vector2 | `public Vector2 headVelocity;` | `public Vector2 headVelocity;` |
| 758 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1113 | 2 | bodyVelocity | Vector2 | `public Vector2 bodyVelocity;` | `public Vector2 bodyVelocity;` |
| 759 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1115 | 2 | legVelocity | Vector2 | `public Vector2 legVelocity;` | `public Vector2 legVelocity;` |
| 760 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1117 | 2 | fullRotation | float | `public float fullRotation;` | `public float fullRotation;` |
| 761 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1119 | 2 | fullRotationOrigin | Vector2 | `public Vector2 fullRotationOrigin = Vector2.Zero;` | `public Vector2 fullRotationOrigin = Vector2.Zero;` |
| 762 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1121 | 2 | fartKartCloudDelay | int | `public int fartKartCloudDelay;` | `public int fartKartCloudDelay;` |
| 763 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1124 | 2 | gfxOffY | float | `public float gfxOffY;` | `public float gfxOffY;` |
| 764 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1126 | 2 | stepSpeed | float | `public float stepSpeed = 1f;` | `public float stepSpeed = 1f;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.33 细分子系统：`PlayerNetworkCameraState`

- 细分职责：网络偏移、网络摄像机目标和同步摄像机缓存。
- 边界角色：`registry/projection`；最小 seam：Registry/Projection seam；网络投影只消费姿态快照。
- 成员文件数：1；声明类型数：1；字段：3；属性：0；合计：3。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（3）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 765 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1128 | 2 | netOffset | Vector2 | `public Vector2 netOffset;` | `public Vector2 netOffset;` |
| 766 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1130 | 2 | netCameraTarget | Vector2? | `internal Vector2? netCameraTarget;` | `internal Vector2? netCameraTarget;` |
| 767 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1132 | 2 | lastSyncedNetCameraTarget | Vector2? | `internal Vector2? lastSyncedNetCameraTarget;` | `internal Vector2? lastSyncedNetCameraTarget;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.39 细分子系统：`PlayerShadowAndArmPresentation`

- 细分职责：玩家残影、手臂合成和动画表现缓存。
- 边界角色：`registry/projection`；最小 seam：Registry/Projection seam；表现缓存只读取输入快照，不拥有玩法状态。
- 成员文件数：1；声明类型数：1；字段：13；属性：0；合计：13。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（13）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 837 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1296 | 2 | cursorItemIconReversed | bool | `public bool cursorItemIconReversed;` | `public bool cursorItemIconReversed;` |
| 838 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1300 | 2 | runSoundDelay | int | `public int runSoundDelay;` | `public int runSoundDelay;` |
| 839 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1304 | 2 | shadowPos | Vector2[] | `public Vector2[] shadowPos = new Vector2[3];` | `public Vector2[] shadowPos = new Vector2[3];` |
| 840 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1306 | 2 | shadowRotation | float[] | `public float[] shadowRotation = new float[3];` | `public float[] shadowRotation = new float[3];` |
| 841 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1308 | 2 | shadowOrigin | Vector2[] | `public Vector2[] shadowOrigin = new Vector2[3];` | `public Vector2[] shadowOrigin = new Vector2[3];` |
| 842 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1310 | 2 | shadowDirection | int[] | `public int[] shadowDirection = new int[3];` | `public int[] shadowDirection = new int[3];` |
| 843 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1312 | 2 | shadowCount | int | `public int shadowCount;` | `public int shadowCount;` |
| 850 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1327 | 2 | skipAnimatingValuesInPlayerFrame | bool | `public bool skipAnimatingValuesInPlayerFrame;` | `public bool skipAnimatingValuesInPlayerFrame;` |
| 853 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1334 | 2 | availableAdvancedShadowsCount | int | `public int availableAdvancedShadowsCount;` | `public int availableAdvancedShadowsCount;` |
| 854 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1336 | 2 | _advancedShadows | Terraria.DataStructures.EntityShadowInfo[] | `private EntityShadowInfo[] _advancedShadows = new EntityShadowInfo[60];` | `private EntityShadowInfo[] _advancedShadows = new EntityShadowInfo[60];` |
| 855 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1338 | 2 | _lastAddedAvancedShadow | int | `private int _lastAddedAvancedShadow;` | `private int _lastAddedAvancedShadow;` |
| 856 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1340 | 2 | compositeFrontArm | Terraria.Player.CompositeArmData | `public CompositeArmData compositeFrontArm;` | `public CompositeArmData compositeFrontArm;` |
| 857 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1342 | 2 | compositeBackArm | Terraria.Player.CompositeArmData | `public CompositeArmData compositeBackArm;` | `public CompositeArmData compositeBackArm;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.44 细分子系统：`PlayerVisualAndShaderEffects`

- 细分职责：玩家着色器、光环、光标、音乐盒和钓鱼钩表现标志。
- 边界角色：`registry/projection`；最小 seam：Registry/Projection seam；表现输出单向读取，不反写玩家权威状态。
- 成员文件数：1；声明类型数：1；字段：21；属性：0；合计：21。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（21）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 902 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1433 | 2 | dontStarveShader | bool | `public bool dontStarveShader;` | `public bool dontStarveShader;` |
| 903 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1435 | 2 | noirShader | bool | `public bool noirShader;` | `public bool noirShader;` |
| 904 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1437 | 2 | eyebrellaCloud | bool | `public bool eyebrellaCloud;` | `public bool eyebrellaCloud;` |
| 905 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1439 | 2 | yoraiz0rEye | int | `public int yoraiz0rEye;` | `public int yoraiz0rEye;` |
| 906 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1441 | 2 | yoraiz0rDarkness | bool | `public bool yoraiz0rDarkness;` | `public bool yoraiz0rDarkness;` |
| 907 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1443 | 2 | hasUnicornHorn | bool | `public bool hasUnicornHorn;` | `public bool hasUnicornHorn;` |
| 908 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1445 | 2 | hasAngelHalo | bool | `public bool hasAngelHalo;` | `public bool hasAngelHalo;` |
| 909 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1447 | 2 | hasRainbowCursor | bool | `public bool hasRainbowCursor;` | `public bool hasRainbowCursor;` |
| 910 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1449 | 2 | leinforsHair | bool | `public bool leinforsHair;` | `public bool leinforsHair;` |
| 911 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1451 | 2 | musicBoxSilence | bool | `public bool musicBoxSilence;` | `public bool musicBoxSilence;` |
| 912 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1453 | 2 | stardustMonolithShader | bool | `public bool stardustMonolithShader;` | `public bool stardustMonolithShader;` |
| 913 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1455 | 2 | nebulaMonolithShader | bool | `public bool nebulaMonolithShader;` | `public bool nebulaMonolithShader;` |
| 914 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1457 | 2 | vortexMonolithShader | bool | `public bool vortexMonolithShader;` | `public bool vortexMonolithShader;` |
| 915 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1459 | 2 | solarMonolithShader | bool | `public bool solarMonolithShader;` | `public bool solarMonolithShader;` |
| 916 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1461 | 2 | moonLordMonolithShader | bool | `public bool moonLordMonolithShader;` | `public bool moonLordMonolithShader;` |
| 917 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1463 | 2 | bloodMoonMonolithShader | bool | `public bool bloodMoonMonolithShader;` | `public bool bloodMoonMonolithShader;` |
| 918 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1465 | 2 | shimmerMonolithShader | bool | `public bool shimmerMonolithShader;` | `public bool shimmerMonolithShader;` |
| 919 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1467 | 2 | CRTMonolithShader | bool | `public bool CRTMonolithShader;` | `public bool CRTMonolithShader;` |
| 920 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1469 | 2 | retroMonolithShader | bool | `public bool retroMonolithShader;` | `public bool retroMonolithShader;` |
| 921 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1471 | 2 | musicBox | int | `public int musicBox;` | `public int musicBox;` |
| 922 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1473 | 2 | overrideFishingBobber | int | `public int overrideFishingBobber = -1;` | `public int overrideFishingBobber = -1;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.71 细分子系统：`PlayerFootballPresentationState`

- 细分职责：足球配饰持有和绘制表现状态。
- 边界角色：`presentation state`；最小 seam：Player Presentation Projection；表现状态不反向拥有配饰装备。
- 成员文件数：1；声明类型数：1；字段：2；属性：0；合计：2。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（2）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1184 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2008 | 2 | hasFootball | bool | `public bool hasFootball;` | `public bool hasFootball;` |
| 1185 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2010 | 2 | drawingFootball | bool | `public bool drawingFootball;` | `public bool drawingFootball;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.67 细分子系统：`PlayerAppearanceCustomizationState`

- 细分职责：发型、染色和角色颜色定制状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；外观变更通过玩家配置命令提交。
- 成员文件数：1；声明类型数：1；字段：10；属性：0；合计：10。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（10）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1156 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1947 | 2 | hairDye | byte | `public byte hairDye;` | `public byte hairDye;` |
| 1157 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1950 | 2 | skinDyePacked | int | `public int skinDyePacked;` | `public int skinDyePacked;` |
| 1158 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1952 | 2 | hairColor | Color | `public Color hairColor = new Color(215, 90, 55);` | `public Color hairColor = new Color(215, 90, 55);` |
| 1159 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1954 | 2 | skinColor | Color | `public Color skinColor = new Color(255, 125, 90);` | `public Color skinColor = new Color(255, 125, 90);` |
| 1160 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1956 | 2 | eyeColor | Color | `public Color eyeColor = new Color(105, 90, 75);` | `public Color eyeColor = new Color(105, 90, 75);` |
| 1161 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1958 | 2 | shirtColor | Color | `public Color shirtColor = new Color(175, 165, 140);` | `public Color shirtColor = new Color(175, 165, 140);` |
| 1162 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1960 | 2 | underShirtColor | Color | `public Color underShirtColor = new Color(160, 180, 215);` | `public Color underShirtColor = new Color(160, 180, 215);` |
| 1163 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1962 | 2 | pantsColor | Color | `public Color pantsColor = new Color(255, 230, 175);` | `public Color pantsColor = new Color(255, 230, 175);` |
| 1164 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1964 | 2 | shoeColor | Color | `public Color shoeColor = new Color(160, 105, 60);` | `public Color shoeColor = new Color(160, 105, 60);` |
| 1165 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1966 | 2 | hair | int | `public int hair;` | `public int hair;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.84 细分子系统：`PlayerTraversalColorProjection`

- 细分职责：翅膀、飞毯、浮筒、抓钩、坐骑和矿车颜色投影槽。
- 边界角色：`registry/projection`；最小 seam：Registry/Projection seam；移动表现只读消费颜色快照。
- 成员文件数：1；声明类型数：1；字段：6；属性：0；合计：6。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（6）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1343 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2332 | 2 | cWings | int | `public int cWings;` | `public int cWings;` |
| 1344 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2334 | 2 | cCarpet | int | `public int cCarpet;` | `public int cCarpet;` |
| 1345 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2336 | 2 | cFloatingTube | int | `public int cFloatingTube;` | `public int cFloatingTube;` |
| 1349 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2344 | 2 | cGrapple | int | `public int cGrapple;` | `public int cGrapple;` |
| 1350 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2346 | 2 | cMount | int | `public int cMount;` | `public int cMount;` |
| 1351 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2348 | 2 | cMinecart | int | `public int cMinecart;` | `public int cMinecart;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.85 细分子系统：`PlayerAppearanceCompanionAndEffectProjection`

- 细分职责：宠物、光源、配饰特效和特殊外观投影槽。
- 边界角色：`registry/projection`；最小 seam：Projection seam；表现输出单向生成。
- 成员文件数：1；声明类型数：1；字段：11；属性：0；合计：11。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（11）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1352 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2350 | 2 | cPet | int | `public int cPet;` | `public int cPet;` |
| 1353 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2352 | 2 | cLight | int | `public int cLight;` | `public int cLight;` |
| 1354 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2354 | 2 | cYorai | int | `public int cYorai;` | `public int cYorai;` |
| 1355 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2356 | 2 | cPortableStool | int | `public int cPortableStool;` | `public int cPortableStool;` |
| 1356 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2358 | 2 | cUnicornHorn | int | `public int cUnicornHorn;` | `public int cUnicornHorn;` |
| 1357 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2360 | 2 | cAngelHalo | int | `public int cAngelHalo;` | `public int cAngelHalo;` |
| 1358 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2362 | 2 | cBeard | int | `public int cBeard;` | `public int cBeard;` |
| 1359 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2364 | 2 | cMinion | int | `public int cMinion;` | `public int cMinion;` |
| 1360 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2366 | 2 | cLeinShampoo | int | `public int cLeinShampoo;` | `public int cLeinShampoo;` |
| 1361 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2368 | 2 | cFlameWaker | int | `public int cFlameWaker;` | `public int cFlameWaker;` |
| 1362 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2370 | 2 | cCoat | int | `public int cCoat;` | `public int cCoat;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.91 细分子系统：`PlayerSpatialDerivedProperties`

- 细分职责：位置、碰撞盒、站立和视觉位置派生属性。
- 边界角色：`derived/query`；最小 seam：只读快照/纯资格 Query；不得写入该分组或相邻权威状态。
- 成员文件数：1；声明类型数：1；字段：0；属性：9；合计：9。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（0）

无该类型成员记录。

##### 属性（9）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1427 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2503 | 2 | BlehOldPositionFixer | Vector2 | `public Vector2 BlehOldPositionFixer => -Vector2.UnitY;` | `public Vector2 BlehOldPositionFixer => -Vector2.UnitY;` |
| 1428 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2507 | 2 | HeightOffsetHitboxCenter | float | `public float HeightOffsetHitboxCenter { get { if (mount.Active) { return mount.PlayerOffsetHitbox; } if (portableStoolInfo.IsInUse) { return portableStoolInfo.HeightBoost - portableStoolInfo.VisualYOffset; } return 0f; } }` | `public float HeightOffsetHitboxCenter { get { if (mount.Active) { return mount.PlayerOffsetHitbox; } if (portableStoolInfo.IsInUse) { return portableStoolInfo.HeightBoost - portableStoolInfo.VisualYOffset; } return 0f; } }` |
| 1429 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2525 | 2 | HeightOffsetBoost | int | `public int HeightOffsetBoost { get { if (mount.Active) { return mount.HeightBoost; } if (portableStoolInfo.IsInUse) { return portableStoolInfo.HeightBoost; } return 0; } }` | `public int HeightOffsetBoost { get { if (mount.Active) { return mount.HeightBoost; } if (portableStoolInfo.IsInUse) { return portableStoolInfo.HeightBoost; } return 0; } }` |
| 1430 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2542 | 2 | HitboxForBestiaryNearbyCheck | Rectangle | `public Rectangle HitboxForBestiaryNearbyCheck { get { Rectangle result = new Rectangle((int)position.X, (int)position.Y, width, height); result.Inflate(300, 200); return result; } }` | `public Rectangle HitboxForBestiaryNearbyCheck { get { Rectangle result = new Rectangle((int)position.X, (int)position.Y, width, height); result.Inflate(300, 200); return result; } }` |
| 1431 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2552 | 2 | IsConsideredStandingStill | bool | `public bool IsConsideredStandingStill { get { if ((double)Math.Abs(velocity.X) < 0.05) { return (double)Math.Abs(velocity.Y) < 0.05; } return false; } }` | `public bool IsConsideredStandingStill { get { if ((double)Math.Abs(velocity.X) < 0.05) { return (double)Math.Abs(velocity.Y) < 0.05; } return false; } }` |
| 1432 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2564 | 2 | BaseHeight | float | `public float BaseHeight => height - HeightOffsetBoost;` | `public float BaseHeight => height - HeightOffsetBoost;` |
| 1433 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2566 | 2 | MountedCenter | Vector2 | `public Vector2 MountedCenter { get { return new Vector2(position.X + (float)(width / 2), position.Y + BaseHeight / 2f + HeightOffsetHitboxCenter); } set { position = new Vector2(value.X - (float)(width / 2), value.Y - BaseHeight / 2f - HeightOffsetHitboxCenter); } }` | `public Vector2 MountedCenter { get { return new Vector2(position.X + (float)(width / 2), position.Y + BaseHeight / 2f + HeightOffsetHitboxCenter); } set { position = new Vector2(value.X - (float)(width / 2), value.Y - BaseHeight / 2f - HeightOffsetHitboxCenter); } }` |
| 1434 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2579 | 2 | VisualPosition | Vector2 | `public override Vector2 VisualPosition => position + new Vector2(0f, gfxOffY);` | `public override Vector2 VisualPosition => position + new Vector2(0f, gfxOffY);` |
| 1435 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2581 | 2 | CCed | bool | `public bool CCed { get { if (!frozen && !webbed) { return stoned; } return true; } }` | `public bool CCed { get { if (!frozen && !webbed) { return stoned; } return true; } }` |


#### 4.13.92 细分子系统：`PlayerIdentityAndDerivedProperties`

- 细分职责：性别投影和计数归一化派生属性。
- 边界角色：`derived/query`；最小 seam：纯派生 Query；属性不得形成第二份权威状态。
- 成员文件数：1；声明类型数：1；字段：0；属性：2；合计：2。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（0）

无该类型成员记录。

##### 属性（2）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1436 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2593 | 2 | miscCounterNormalized | float | `public float miscCounterNormalized => (float)miscCounter / 300f;` | `public float miscCounterNormalized => (float)miscCounter / 300f;` |
| 1437 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2595 | 2 | Male | bool | `public bool Male { get { return PlayerVariantID.Sets.Male[skinVariant]; } set { if (value) { if (!Male) { skinVariant = PlayerVariantID.Sets.AltGenderReference[skinVariant]; } } else if (Male) { skinVariant = PlayerVariantID.Sets.AltGenderReference[skinVariant]; } } }` | `public bool Male { get { return PlayerVariantID.Sets.Male[skinVariant]; } set { if (value) { if (!Male) { skinVariant = PlayerVariantID.Sets.AltGenderReference[skinVariant]; } } else if (Male) { skinVariant = PlayerVariantID.Sets.AltGenderReference[skinVariant]; } } }` |


#### 4.13.93 细分子系统：`PlayerBiomeZoneProperties`

- 细分职责：地牢、邪恶、神圣、丛林、雪地和地下沙漠区域属性。
- 边界角色：`derived/query`；最小 seam：纯资格 Query；从区域快照读取并返回只读结果。
- 成员文件数：1；声明类型数：1；字段：0；属性：16；合计：16。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（0）

无该类型成员记录。

##### 属性（16）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1438 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2617 | 2 | ZoneDungeon | bool | `public bool ZoneDungeon { get { return zone1[0]; } set { zone1[0] = value; } }` | `public bool ZoneDungeon { get { return zone1[0]; } set { zone1[0] = value; } }` |
| 1439 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2629 | 2 | ZoneCorrupt | bool | `public bool ZoneCorrupt { get { return zone1[1]; } set { zone1[1] = value; } }` | `public bool ZoneCorrupt { get { return zone1[1]; } set { zone1[1] = value; } }` |
| 1440 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2641 | 2 | ZoneHallow | bool | `public bool ZoneHallow { get { return zone1[2]; } set { zone1[2] = value; } }` | `public bool ZoneHallow { get { return zone1[2]; } set { zone1[2] = value; } }` |
| 1441 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2653 | 2 | ZoneMeteor | bool | `public bool ZoneMeteor { get { return zone1[3]; } set { zone1[3] = value; } }` | `public bool ZoneMeteor { get { return zone1[3]; } set { zone1[3] = value; } }` |
| 1442 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2665 | 2 | ZoneJungle | bool | `public bool ZoneJungle { get { return zone1[4]; } set { zone1[4] = value; } }` | `public bool ZoneJungle { get { return zone1[4]; } set { zone1[4] = value; } }` |
| 1443 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2677 | 2 | ZoneSnow | bool | `public bool ZoneSnow { get { return zone1[5]; } set { zone1[5] = value; } }` | `public bool ZoneSnow { get { return zone1[5]; } set { zone1[5] = value; } }` |
| 1444 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2689 | 2 | ZoneCrimson | bool | `public bool ZoneCrimson { get { return zone1[6]; } set { zone1[6] = value; } }` | `public bool ZoneCrimson { get { return zone1[6]; } set { zone1[6] = value; } }` |
| 1445 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2701 | 2 | ZoneWaterCandle | bool | `public bool ZoneWaterCandle { get { return zone1[7]; } set { zone1[7] = value; } }` | `public bool ZoneWaterCandle { get { return zone1[7]; } set { zone1[7] = value; } }` |
| 1446 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2713 | 2 | ZonePeaceCandle | bool | `public bool ZonePeaceCandle { get { return zone2[0]; } set { zone2[0] = value; } }` | `public bool ZonePeaceCandle { get { return zone2[0]; } set { zone2[0] = value; } }` |
| 1447 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2725 | 2 | ZoneTowerSolar | bool | `public bool ZoneTowerSolar { get { return zone2[1]; } set { zone2[1] = value; } }` | `public bool ZoneTowerSolar { get { return zone2[1]; } set { zone2[1] = value; } }` |
| 1448 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2737 | 2 | ZoneTowerVortex | bool | `public bool ZoneTowerVortex { get { return zone2[2]; } set { zone2[2] = value; } }` | `public bool ZoneTowerVortex { get { return zone2[2]; } set { zone2[2] = value; } }` |
| 1449 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2749 | 2 | ZoneTowerNebula | bool | `public bool ZoneTowerNebula { get { return zone2[3]; } set { zone2[3] = value; } }` | `public bool ZoneTowerNebula { get { return zone2[3]; } set { zone2[3] = value; } }` |
| 1450 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2761 | 2 | ZoneTowerStardust | bool | `public bool ZoneTowerStardust { get { return zone2[4]; } set { zone2[4] = value; } }` | `public bool ZoneTowerStardust { get { return zone2[4]; } set { zone2[4] = value; } }` |
| 1451 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2773 | 2 | ZoneDesert | bool | `public bool ZoneDesert { get { return zone2[5]; } set { zone2[5] = value; } }` | `public bool ZoneDesert { get { return zone2[5]; } set { zone2[5] = value; } }` |
| 1452 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2785 | 2 | ZoneGlowshroom | bool | `public bool ZoneGlowshroom { get { return zone2[6]; } set { zone2[6] = value; } }` | `public bool ZoneGlowshroom { get { return zone2[6]; } set { zone2[6] = value; } }` |
| 1453 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2797 | 2 | ZoneUndergroundDesert | bool | `public bool ZoneUndergroundDesert { get { return zone2[7]; } set { zone2[7] = value; } }` | `public bool ZoneUndergroundDesert { get { return zone2[7]; } set { zone2[7] = value; } }` |


#### 4.13.94 细分子系统：`PlayerVerticalAndWeatherZoneProperties`

- 细分职责：高度、海滩、降雨和沙尘暴区域属性。
- 边界角色：`derived/query`；最小 seam：纯资格 Query；不直接修改世界或玩家状态。
- 成员文件数：1；声明类型数：1；字段：0；属性：6；合计：6。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（0）

无该类型成员记录。

##### 属性（6）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1454 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2809 | 2 | ZoneSkyHeight | bool | `public bool ZoneSkyHeight { get { return zone3[0]; } set { zone3[0] = value; } }` | `public bool ZoneSkyHeight { get { return zone3[0]; } set { zone3[0] = value; } }` |
| 1455 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2821 | 2 | ZoneOverworldHeight | bool | `public bool ZoneOverworldHeight { get { return zone3[1]; } set { zone3[1] = value; } }` | `public bool ZoneOverworldHeight { get { return zone3[1]; } set { zone3[1] = value; } }` |
| 1456 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2834 | 2 | ZoneUnderworldHeight | bool | `public bool ZoneUnderworldHeight { get { return zone3[4]; } set { zone3[4] = value; } }` | `public bool ZoneUnderworldHeight { get { return zone3[4]; } set { zone3[4] = value; } }` |
| 1457 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2846 | 2 | ZoneBeach | bool | `public bool ZoneBeach { get { return zone3[5]; } set { zone3[5] = value; } }` | `public bool ZoneBeach { get { return zone3[5]; } set { zone3[5] = value; } }` |
| 1458 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2858 | 2 | ZoneRain | bool | `public bool ZoneRain { get { return zone3[6]; } set { zone3[6] = value; } }` | `public bool ZoneRain { get { return zone3[6]; } set { zone3[6] = value; } }` |
| 1459 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2870 | 2 | ZoneSandstorm | bool | `public bool ZoneSandstorm { get { return zone3[7]; } set { zone3[7] = value; } }` | `public bool ZoneSandstorm { get { return zone3[7]; } set { zone3[7] = value; } }` |


#### 4.13.95 细分子系统：`PlayerEventAndShoppingZoneProperties`

- 细分职责：事件区域、微光区域和商店区域派生属性。
- 边界角色：`derived/query`；最小 seam：纯资格 Query；经济系统只消费结果。
- 成员文件数：1；声明类型数：1；字段：0；属性：7；合计：7。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（0）

无该类型成员记录。

##### 属性（7）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1460 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2882 | 2 | ZoneOldOneArmy | bool | `public bool ZoneOldOneArmy { get { return zone4[0]; } set { zone4[0] = value; } }` | `public bool ZoneOldOneArmy { get { return zone4[0]; } set { zone4[0] = value; } }` |
| 1461 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2895 | 2 | ZoneLihzhardTemple | bool | `public bool ZoneLihzhardTemple { get { return zone4[5]; } set { zone4[5] = value; } }` | `public bool ZoneLihzhardTemple { get { return zone4[5]; } set { zone4[5] = value; } }` |
| 1462 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2907 | 2 | ZoneGraveyard | bool | `public bool ZoneGraveyard { get { return zone4[6]; } set { zone4[6] = value; } }` | `public bool ZoneGraveyard { get { return zone4[6]; } set { zone4[6] = value; } }` |
| 1463 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2919 | 2 | ZoneShadowCandle | bool | `public bool ZoneShadowCandle { get { return zone4[7]; } set { zone4[7] = value; } }` | `public bool ZoneShadowCandle { get { return zone4[7]; } set { zone4[7] = value; } }` |
| 1464 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2931 | 2 | ZoneShimmer | bool | `public bool ZoneShimmer { get { return zone5[0]; } set { zone5[0] = value; } }` | `public bool ZoneShimmer { get { return zone5[0]; } set { zone5[0] = value; } }` |
| 1465 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2943 | 2 | ShoppingZone_AnyBiome | bool | `public bool ShoppingZone_AnyBiome { get { if (!ZoneDungeon && !ZoneCorrupt && !ZoneCrimson && !ZoneGlowshroom && !ZoneHallow && !ZoneJungle && !ZoneSnow && !ZoneBeach) { return ZoneDesert; } return true; } }` | `public bool ShoppingZone_AnyBiome { get { if (!ZoneDungeon && !ZoneCorrupt && !ZoneCrimson && !ZoneGlowshroom && !ZoneHallow && !ZoneJungle && !ZoneSnow && !ZoneBeach) { return ZoneDesert; } return true; } }` |
| 1466 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2955 | 2 | ShoppingZone_BelowSurface | bool | `public bool ShoppingZone_BelowSurface => (double)position.Y > Main.worldSurface * 16.0;` | `public bool ShoppingZone_BelowSurface => (double)position.Y > Main.worldSurface * 16.0;` |


#### 4.13.96 细分子系统：`PlayerInteractionAndSelectionProperties`

- 细分职责：选中物品、持有物品、交谈、浮水和交互资格派生属性。
- 边界角色：`derived/query`；最小 seam：只读快照/纯资格 Query；不得写入该分组或相邻权威状态。
- 成员文件数：1；声明类型数：1；字段：0；属性：9；合计：9。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（0）

无该类型成员记录。

##### 属性（9）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1467 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2958 | 2 | Directions | Vector2 | `public Vector2 Directions => new Vector2(direction, gravDir);` | `public Vector2 Directions => new Vector2(direction, gravDir);` |
| 1468 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2960 | 2 | selectedItem | int | `public int selectedItem => selectedItemState.Selected;` | `public int selectedItem => selectedItemState.Selected;` |
| 1469 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2962 | 2 | HeldItem | Terraria.Item | `public Item HeldItem => inventory[selectedItem];` | `public Item HeldItem => inventory[selectedItem];` |
| 1470 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2965 | 2 | ShouldFloatInWater | bool | `public bool ShouldFloatInWater { get { if (canFloatInWater && !controlDown) { if (mount.Active) { return mount.Type == 37; } return true; } return false; } }` | `public bool ShouldFloatInWater { get { if (canFloatInWater && !controlDown) { if (mount.Active) { return mount.Type == 37; } return true; } return false; } }` |
| 1471 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2981 | 2 | CanBeTalkedTo | bool | `public bool CanBeTalkedTo { get { if (active && !dead && !ShouldNotDraw) { return stealth == 1f; } return false; } }` | `public bool CanBeTalkedTo { get { if (active && !dead && !ShouldNotDraw) { return stealth == 1f; } return false; } }` |
| 1472 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2993 | 2 | IsVoidVaultEnabled | bool | `public bool IsVoidVaultEnabled { get { return voidVaultInfo[0]; } set { voidVaultInfo[0] = value; } }` | `public bool IsVoidVaultEnabled { get { return voidVaultInfo[0]; } set { voidVaultInfo[0] = value; } }` |
| 1473 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 3005 | 2 | ReportedCameraPosition | Vector2 | `public Vector2 ReportedCameraPosition { get { if (!netCameraTarget.HasValue) { return position; } return netCameraTarget.Value; } }` | `public Vector2 ReportedCameraPosition { get { if (!netCameraTarget.HasValue) { return position; } return netCameraTarget.Value; } }` |
| 1474 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 3017 | 2 | TryingToHoverUp | bool | `public bool TryingToHoverUp { get { if (!controlUp) { return tryKeepingHoveringUp; } return true; } }` | `public bool TryingToHoverUp { get { if (!controlUp) { return tryKeepingHoveringUp; } return true; } }` |
| 1475 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 3029 | 2 | TryingToHoverDown | bool | `public bool TryingToHoverDown { get { if (!controlDown) { return tryKeepingHoveringDown; } return true; } }` | `public bool TryingToHoverDown { get { if (!controlDown) { return tryKeepingHoveringDown; } return true; } }` |


#### 4.13.97 细分子系统：`PlayerAbilityAndPresentationProperties`

- 细分职责：坐骑车、有效伤害、飞行能力和表现层可见性派生属性。
- 边界角色：`derived/query`；最小 seam：只读快照/纯资格 Query；不得写入该分组或相邻权威状态。
- 成员文件数：1；声明类型数：1；字段：0；属性：12；合计：12。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（0）

无该类型成员记录。

##### 属性（12）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1476 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 3042 | 2 | UsingBiomeTorches | bool | `public bool UsingBiomeTorches { get { if (!unlockedBiomeTorches) { return false; } return builderAccStatus[11] == 0; } set { builderAccStatus[11] = ((!value) ? 1 : 0); } }` | `public bool UsingBiomeTorches { get { if (!unlockedBiomeTorches) { return false; } return builderAccStatus[11] == 0; } set { builderAccStatus[11] = ((!value) ? 1 : 0); } }` |
| 1477 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 3058 | 2 | UsingSuperCart | bool | `public bool UsingSuperCart { get { if (!unlockedSuperCart) { return false; } return enabledSuperCart; } set { enabledSuperCart = value; } }` | `public bool UsingSuperCart { get { if (!unlockedSuperCart) { return false; } return enabledSuperCart; } set { enabledSuperCart = value; } }` |
| 1478 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 3074 | 2 | bowEffectiveDamage | float | `public float bowEffectiveDamage => (rangedDamage / rangedMultDamage + arrowDamageAdditiveStack) * rangedMultDamage * arrowDamage;` | `public float bowEffectiveDamage => (rangedDamage / rangedMultDamage + arrowDamageAdditiveStack) * rangedMultDamage * arrowDamage;` |
| 1479 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 3076 | 2 | gunEffectiveDamage | float | `public float gunEffectiveDamage => rangedDamage * bulletDamage;` | `public float gunEffectiveDamage => rangedDamage * bulletDamage;` |
| 1480 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 3078 | 2 | specialistEffectiveDamage | float | `public float specialistEffectiveDamage => rangedDamage * rocketDamage;` | `public float specialistEffectiveDamage => rangedDamage * rocketDamage;` |
| 1481 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 3080 | 2 | CanUseBootFlyingAbilities | bool | `public bool CanUseBootFlyingAbilities => !isPerformingJump_DownDash;` | `public bool CanUseBootFlyingAbilities => !isPerformingJump_DownDash;` |
| 1482 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 3082 | 2 | CanUseWingAbilities | bool | `public bool CanUseWingAbilities { get { if (!merman) { return !isPerformingJump_DownDash; } return false; } }` | `public bool CanUseWingAbilities { get { if (!merman) { return !isPerformingJump_DownDash; } return false; } }` |
| 1483 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 3094 | 2 | ShouldNotDraw | bool | `public bool ShouldNotDraw { get { if (invis && itemAnimation == 0) { if (!isDisplayDollOrInanimate) { return !isHatRackDoll; } return false; } return false; } }` | `public bool ShouldNotDraw { get { if (invis && itemAnimation == 0) { if (!isDisplayDollOrInanimate) { return !isHatRackDoll; } return false; } return false; } }` |
| 1484 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 3110 | 2 | talkNPC | int | `public int talkNPC { get; private set; }` | `public int talkNPC { get; private set; }` |
| 1485 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 3112 | 2 | isLockedToATile | bool | `public bool isLockedToATile { get { if (!sitting.isSitting) { return sleeping.isSleeping; } return true; } }` | `public bool isLockedToATile { get { if (!sitting.isSitting) { return sleeping.isSleeping; } return true; } }` |
| 1486 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 3124 | 2 | PortalPhysicsEnabled | bool | `public bool PortalPhysicsEnabled { get { if (_portalPhysicsTime > 0) { return !mount.Active; } return false; } }` | `public bool PortalPhysicsEnabled { get { if (_portalPhysicsTime > 0) { return !mount.Active; } return false; } }` |
| 1487 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 3136 | 2 | MountFishronSpecial | bool | `public bool MountFishronSpecial { get { if (statLife >= statLifeMax2 / 2 && (!wet \|\| lavaWet \|\| honeyWet) && !dripping && !(MountFishronSpecialCounter > 0f)) { if (Main.raining) { return WorldGen.InAPlaceWithWind(position, width, height); } return false; } return true; } }` | `public bool MountFishronSpecial { get { if (statLife >= statLifeMax2 / 2 && (!wet \|\| lavaWet \|\| honeyWet) && !dripping && !(MountFishronSpecialCounter > 0f)) { if (Main.raining) { return WorldGen.InAPlaceWithWind(position, width, height); } return false; } return true; } }` |


#### 4.13.98 细分子系统：`PlayerItemMountAndRuntimeProperties`

- 细分职责：物品时序、坐骑/轨道、场景指标和运行时表现派生属性。
- 边界角色：`derived/query`；最小 seam：只读快照/纯资格 Query；不得写入该分组或相邻权威状态。
- 成员文件数：1；声明类型数：1；字段：0；属性：11；合计：11。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（0）

无该类型成员记录。

##### 属性（11）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1488 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 3152 | 2 | HasMinionRestTarget | bool | `public bool HasMinionRestTarget => MinionRestTargetPoint != Vector2.Zero;` | `public bool HasMinionRestTarget => MinionRestTargetPoint != Vector2.Zero;` |
| 1489 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 3155 | 2 | ItemTimeIsZero | bool | `public bool ItemTimeIsZero => itemTime == 0;` | `public bool ItemTimeIsZero => itemTime == 0;` |
| 1490 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 3157 | 2 | ItemAnimationJustStarted | bool | `public bool ItemAnimationJustStarted => itemAnimation == itemAnimationMax - 1;` | `public bool ItemAnimationJustStarted => itemAnimation == itemAnimationMax - 1;` |
| 1491 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 3159 | 2 | UsingOrReusingItem | bool | `public bool UsingOrReusingItem { get { if (itemAnimation <= 0 && reuseDelay <= 0 && !channel) { return pendingItemReuse; } return true; } }` | `public bool UsingOrReusingItem { get { if (itemAnimation <= 0 && reuseDelay <= 0 && !channel) { return pendingItemReuse; } return true; } }` |
| 1492 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 3171 | 2 | SceneMetrics | Terraria.SceneMetrics | `public static SceneMetrics SceneMetrics => Main.PlayerSceneMetrics;` | `public static SceneMetrics SceneMetrics => Main.PlayerSceneMetrics;` |
| 1493 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 3173 | 2 | SpectatingCameraPosition | Vector2 | `public Vector2 SpectatingCameraPosition { get { if (spectating < 0) { return position; } Player player = Main.player[spectating]; return player.Bottom + new Vector2(0f, player.gfxOffY - 21f) + player.netOffset; } }` | `public Vector2 SpectatingCameraPosition { get { if (spectating < 0) { return position; } Player player = Main.player[spectating]; return player.Bottom + new Vector2(0f, player.gfxOffY - 21f) + player.netOffset; } }` |
| 1494 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 3187 | 2 | SlimeDontHyperJump | bool | `public bool SlimeDontHyperJump { get { if (mount.Active && mount.IsConsideredASlimeMount && wetSlime > 0) { return !controlJump; } return false; } }` | `public bool SlimeDontHyperJump { get { if (mount.Active && mount.IsConsideredASlimeMount && wetSlime > 0) { return !controlJump; } return false; } }` |
| 1495 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 3199 | 2 | hasBreathingReed | bool | `private bool hasBreathingReed { get { if (inventory[selectedItem].type == 186) { if (mount.Active) { return !MountID.Sets.DontHoldItems[mount.Type]; } return true; } return false; } }` | `private bool hasBreathingReed { get { if (inventory[selectedItem].type == 186) { if (mount.Active) { return !MountID.Sets.DontHoldItems[mount.Type]; } return true; } return false; } }` |
| 1496 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 3215 | 2 | IsRidingTracks | bool | `public bool IsRidingTracks { get { if (!mount.Active) { return false; } if (mount.Cart) { return true; } if (mount.CanGrindRails && onTrack) { return true; } return false; } }` | `public bool IsRidingTracks { get { if (!mount.Active) { return false; } if (mount.Cart) { return true; } if (mount.CanGrindRails && onTrack) { return true; } return false; } }` |
| 1497 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 3235 | 2 | MouthPosition | Vector2? | `public Vector2? MouthPosition { get { if (mount.Active) { Mount.MountDelegatesData.OverridePositionMethod mouthPosition = mount.Delegations.MouthPosition; if (mouthPosition != null && mouthPosition(this, out var result)) { return result; } } Vector2 spinningpoint = new Vector2(direction * 8, gravDir * -4f); return RotatedRelativePoint(MountedCenter, reverseRotation: false, addGfxOffY: false) + spinningpoint.RotatedBy(fullRotation); } }` | `public Vector2? MouthPosition { get { if (mount.Active) { Mount.MountDelegatesData.OverridePositionMethod mouthPosition = mount.Delegations.MouthPosition; if (mouthPosition != null && mouthPosition(this, out var result)) { return result; } } Vector2 spinningpoint = new Vector2(direction * 8, gravDir * -4f); return RotatedRelativePoint(MountedCenter, reverseRotation: false, addGfxOffY: false) + spinningpoint.RotatedBy(fullRotation); } }` |
| 1498 | property | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 3252 | 2 | HandPosition | Vector2? | `public Vector2? HandPosition { get { if (mount.Active) { Mount.MountDelegatesData.OverridePositionMethod handPosition = mount.Delegations.HandPosition; if (handPosition != null && handPosition(this, out var result)) { return result; } } Vector2 vector = Main.OffsetsPlayerOnhand[bodyFrame.Y / 56] * 2f; if (direction != 1) { vector.X = (float)bodyFrame.Width - vector.X; } if (gravDir != 1f) { vector.Y = (float)bodyFrame.Height - vector.Y; } vector -= new Vector2(bodyFrame.Width - width, bodyFrame.Height - 42) / 2f; Vector2 vector2 = -new Vector2(20f, 42f) / 2f + vector; Vector2 pos = MountedCenter + vector2; ApplyItemPositionOffsetFromMount(ref pos); return RotatedRelativePoint(pos); } }` | `public Vector2? HandPosition { get { if (mount.Active) { Mount.MountDelegatesData.OverridePositionMethod handPosition = mount.Delegations.HandPosition; if (handPosition != null && handPosition(this, out var result)) { return result; } } Vector2 vector = Main.OffsetsPlayerOnhand[bodyFrame.Y / 56] * 2f; if (direction != 1) { vector.X = (float)bodyFrame.Width - vector.X; } if (gravDir != 1f) { vector.Y = (float)bodyFrame.Height - vector.Y; } vector -= new Vector2(bodyFrame.Width - width, bodyFrame.Height - 42) / 2f; Vector2 vector2 = -new Vector2(20f, 42f) / 2f + vector; Vector2 pos = MountedCenter + vector2; ApplyItemPositionOffsetFromMount(ref pos); return RotatedRelativePoint(pos); } }` |


#### 4.13.103 细分子系统：`PlayerEyeAnimationState`

- 细分职责：眼睛状态和受伤/中毒/睡眠驱动的眼部动画投影。
- 边界角色：`registry/projection`；最小 seam：Registry/Projection seam；表现状态只读取玩家事实。
- 成员文件数：1；声明类型数：1；字段：3；属性：1；合计：4。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（3）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 370 | field | Terraria.GameContent.PlayerEyeHelper | Terraria.GameContent/PlayerEyeHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\PlayerEyeHelper.cs | 24 | 2 | _state | Terraria.GameContent.PlayerEyeHelper.EyeState | `private EyeState _state;` | `private EyeState _state;` |
| 371 | field | Terraria.GameContent.PlayerEyeHelper | Terraria.GameContent/PlayerEyeHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\PlayerEyeHelper.cs | 26 | 2 | _timeInState | int | `private int _timeInState;` | `private int _timeInState;` |
| 372 | field | Terraria.GameContent.PlayerEyeHelper | Terraria.GameContent/PlayerEyeHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\PlayerEyeHelper.cs | 28 | 2 | TimeToActDamaged | int | `private const int TimeToActDamaged = 20;` | `private const int TimeToActDamaged = 20;` |

##### 属性（1）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1417 | property | Terraria.GameContent.PlayerEyeHelper | Terraria.GameContent/PlayerEyeHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\PlayerEyeHelper.cs | 30 | 2 | EyeFrameToShow | int | `public int EyeFrameToShow { get; private set; }` | `public int EyeFrameToShow { get; private set; }` |


#### 4.13.108 细分子系统：`PlayerPresentationMessagesAndArms`

- 细分职责：头顶消息和复合手臂表现参数。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System 读取输入并通过显式 Command/CommitPort 写入；跨组只用事件、Query 或命令交接。
- 成员文件数：1；声明类型数：2；字段：8；属性：0；合计：8。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（8）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 418 | field | Terraria.Player.CompositeArmData | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 226 | 3 | enabled | bool | `public bool enabled;` | `public bool enabled;` |
| 419 | field | Terraria.Player.CompositeArmData | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 228 | 3 | stretch | Terraria.Player.CompositeArmStretchAmount | `public CompositeArmStretchAmount stretch;` | `public CompositeArmStretchAmount stretch;` |
| 420 | field | Terraria.Player.CompositeArmData | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 230 | 3 | rotation | float | `public float rotation;` | `public float rotation;` |
| 440 | field | Terraria.Player.OverheadMessage | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 457 | 3 | chatText | string | `public string chatText;` | `public string chatText;` |
| 441 | field | Terraria.Player.OverheadMessage | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 459 | 3 | snippets | Terraria.UI.Chat.TextSnippet[] | `public TextSnippet[] snippets;` | `public TextSnippet[] snippets;` |
| 442 | field | Terraria.Player.OverheadMessage | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 461 | 3 | messageSize | Vector2 | `public Vector2 messageSize;` | `public Vector2 messageSize;` |
| 443 | field | Terraria.Player.OverheadMessage | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 463 | 3 | timeLeft | int | `public int timeLeft;` | `public int timeLeft;` |
| 444 | field | Terraria.Player.OverheadMessage | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 465 | 3 | color | Color | `public Color color;` | `public Color color;` |

##### 属性（0）

无该类型成员记录。


## 8. 本分区自检

- 叶子子系统：18 个；指定组均已写入且没有重复分区。
- 字段/属性/成员：91 / 73 / 164。
- 来源序号范围：370..1498；本分区内唯一序号：True。
- 成员声明表逐组从源报告提取，未改写来源序号、类型、路径、行列、成员、C# 类型、成员声明或原始声明。
- 运行时读写者、生命周期、持久化、网络和调度：未验证，保留为迁移前风险。
