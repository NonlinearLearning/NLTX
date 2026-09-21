# P03 坐骑与车辆定义、运行时、动画和特殊载具 - 权威模拟系统成员组件候选分区

> 来源：`docs/migration/ledgers/Version4权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md`；本文件只做成员库存分区和候选组件边界记录。
> 这是候选组件拆分台账，不是运行时迁移完成、行为等价、API 兼容、网络闭合或持久化闭合证明。

## 1. 分区边界

本分区包含 17 个叶子子系统，字段 136 条、属性 27 条、成员合计 163 条；每个叶子子系统保持原正式父级归属，不跨分区拆行。

## 2. 分区覆盖概览

| 叶子子系统 | 正式父级 | 字段 | 属性 | 合计 | 源边界角色 |
|---|---|---:|---:|---:|---|
| `MountFrameAndDrawCatalog` | `MountAndVehicleSimulation` | 11 | 0 | 11 | definition/query |
| `MountSpecialVehicleCatalog` | `MountAndVehicleSimulation` | 5 | 0 | 5 | definition/query |
| `MountDrillConstants` | `MountAndVehicleSimulation` | 9 | 0 | 9 | definition/query |
| `MountSuperCartConstants` | `MountAndVehicleSimulation` | 5 | 0 | 5 | definition/query |
| `MountRuntimeFrameAndFlightState` | `MountAndVehicleSimulation` | 15 | 0 | 15 | authoritative state/behavior |
| `MountFatigueAndAbilityState` | `MountAndVehicleSimulation` | 8 | 0 | 8 | authoritative state/behavior |
| `MountRuntimeIdentityAndFrameProjection` | `MountAndVehicleSimulation` | 2 | 11 | 13 | derived/query |
| `MountRuntimeMobilityAndAbilityProjection` | `MountAndVehicleSimulation` | 0 | 16 | 16 | derived/query |
| `MountGeometryAndOffsetCatalog` | `MountAndVehicleSimulation` | 9 | 0 | 9 | definition/query |
| `MountGroundAnimationFrames` | `MountAndVehicleSimulation` | 11 | 0 | 11 | definition/query |
| `MountAerialAndWaterAnimationFrames` | `MountAndVehicleSimulation` | 9 | 0 | 9 | definition/query |
| `MountDashAnimationFrames` | `MountAndVehicleSimulation` | 3 | 0 | 3 | definition/query |
| `MountMovementAndAbilityCatalog` | `MountAndVehicleSimulation` | 18 | 0 | 18 | definition/query |
| `MountVehicleAndPresentationCatalog` | `MountAndVehicleSimulation` | 9 | 0 | 9 | definition/query |
| `MountDelegateContract` | `MountAndVehicleSimulation` | 8 | 0 | 8 | authoritative state/behavior |
| `DrillMountRuntime` | `MountAndVehicleSimulation` | 9 | 0 | 9 | authoritative state/behavior |
| `MountVariantFlags` | `MountAndVehicleSimulation` | 5 | 0 | 5 | authoritative state/behavior |

## 3. 候选组件归属与 seam

| 叶子子系统 | 候选组件边界 | 类型分类 | 最小 seam | 独立生命周期 | 证据状态 | 主要风险 |
|---|---|---|---|---|---|---|
| `MountFrameAndDrawCatalog` | `MountFrameAndDrawCatalogDefinition`/`Query` | Definition/Query | 只读 Definition/Catalog view | 候选纯读边界；仍需核对失效/缓存条件 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `MountSpecialVehicleCatalog` | `MountSpecialVehicleCatalogDefinition`/`Query` | Definition/Query | 只读 Definition/Catalog view | 候选纯读边界；仍需核对失效/缓存条件 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `MountDrillConstants` | `MountDrillConstantsDefinition`/`Query` | Definition/Query | 只读 Definition/Catalog view | 候选纯读边界；仍需核对失效/缓存条件 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `MountSuperCartConstants` | `MountSuperCartConstantsDefinition`/`Query` | Definition/Query | 只读 Definition/Catalog view | 候选纯读边界；仍需核对失效/缓存条件 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `MountRuntimeFrameAndFlightState` | `MountRuntimeFrameAndFlightStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `MountFatigueAndAbilityState` | `MountFatigueAndAbilityStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `MountRuntimeIdentityAndFrameProjection` | `MountRuntimeIdentityAndFrameProjectionQuery` | Query | 纯资格或派生 Query；只读 | 候选纯读边界；仍需核对失效/缓存条件 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `MountRuntimeMobilityAndAbilityProjection` | `MountRuntimeMobilityAndAbilityProjectionQuery` | Query | 纯资格或派生 Query；只读 | 候选纯读边界；仍需核对失效/缓存条件 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `MountGeometryAndOffsetCatalog` | `MountGeometryAndOffsetCatalogDefinition`/`Query` | Definition/Query | 只读 Definition/Catalog view | 候选纯读边界；仍需核对失效/缓存条件 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `MountGroundAnimationFrames` | `MountGroundAnimationFramesDefinition`/`Query` | Definition/Query | 只读 Definition/Catalog view | 候选纯读边界；仍需核对失效/缓存条件 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `MountAerialAndWaterAnimationFrames` | `MountAerialAndWaterAnimationFramesDefinition`/`Query` | Definition/Query | 只读 Definition/Catalog view | 候选纯读边界；仍需核对失效/缓存条件 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `MountDashAnimationFrames` | `MountDashAnimationFramesDefinition`/`Query` | Definition/Query | 只读 Definition/Catalog view | 候选纯读边界；仍需核对失效/缓存条件 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `MountMovementAndAbilityCatalog` | `MountMovementAndAbilityCatalogDefinition`/`Query` | Definition/Query | 只读 Definition/Catalog view | 候选纯读边界；仍需核对失效/缓存条件 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `MountVehicleAndPresentationCatalog` | `MountVehicleAndPresentationCatalogDefinition`/`Query` | Definition/Query | 只读 Definition/Catalog view | 候选纯读边界；仍需核对失效/缓存条件 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `MountDelegateContract` | `MountDelegateContractComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `DrillMountRuntime` | `DrillMountRuntimeComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `MountVariantFlags` | `MountVariantFlagsComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |

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

### 正式父级子系统：`MountAndVehicleSimulation`
- 父级职责：沿用源报告正式父级 `MountAndVehicleSimulation`；本分区只收录该父级中列出的叶子子系统。
- 本分区父级局部统计：字段 136；属性 27；合计 163；完整父级统计以源报告为准。

#### 4.5.1 细分子系统：`MountFrameAndDrawCatalog`

- 细分职责：坐骑通用帧状态、绘制层级和特殊老鼠帧序列常量。
- 边界角色：`definition/query`；最小 seam：只读 Definition/Catalog view；表现系统按帧定义消费。
- 成员文件数：1；声明类型数：1；字段：11；属性：0；合计：11。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（11）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 269 | field | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 273 | 2 | FrameStanding | int | `public const int FrameStanding = 0;` | `public const int FrameStanding = 0;` |
| 270 | field | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 275 | 2 | FrameRunning | int | `public const int FrameRunning = 1;` | `public const int FrameRunning = 1;` |
| 271 | field | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 277 | 2 | FrameInAir | int | `public const int FrameInAir = 2;` | `public const int FrameInAir = 2;` |
| 272 | field | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 279 | 2 | FrameFlying | int | `public const int FrameFlying = 3;` | `public const int FrameFlying = 3;` |
| 273 | field | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 281 | 2 | FrameSwimming | int | `public const int FrameSwimming = 4;` | `public const int FrameSwimming = 4;` |
| 274 | field | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 283 | 2 | FrameDashing | int | `public const int FrameDashing = 5;` | `public const int FrameDashing = 5;` |
| 275 | field | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 285 | 2 | DrawBack | int | `public const int DrawBack = 0;` | `public const int DrawBack = 0;` |
| 276 | field | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 287 | 2 | DrawBackExtra | int | `public const int DrawBackExtra = 1;` | `public const int DrawBackExtra = 1;` |
| 277 | field | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 289 | 2 | DrawFront | int | `public const int DrawFront = 2;` | `public const int DrawFront = 2;` |
| 278 | field | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 291 | 2 | DrawFrontExtra | int | `public const int DrawFrontExtra = 3;` | `public const int DrawFrontExtra = 3;` |
| 323 | field | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 381 | 2 | idleFrames_Rat | int[] | `public static int[] idleFrames_Rat = new int[11]  	{  		0, 1, 3, 2, 3, 2, 3, 2, 1, 0,  		0  	};` | `public static int[] idleFrames_Rat = new int[11] { 0, 1, 3, 2, 3, 2, 3, 2, 1, 0, 0 };` |

##### 属性（0）

无该类型成员记录。


#### 4.5.2 细分子系统：`MountSpecialVehicleCatalog`

- 细分职责：坐骑注册表、Scutlix 和 Santank 的专用车辆/战斗定义。
- 边界角色：`definition/query`；最小 seam：只读 Definition/Catalog view；车辆适配器单向读取。
- 成员文件数：1；声明类型数：1；字段：5；属性：0；合计：5。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（5）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 279 | field | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 293 | 2 | mounts | Terraria.Mount.MountData[] | `private static MountData[] mounts;` | `private static MountData[] mounts;` |
| 280 | field | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 295 | 2 | scutlixEyePositions | Vector2[] | `private static Vector2[] scutlixEyePositions;` | `private static Vector2[] scutlixEyePositions;` |
| 281 | field | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 297 | 2 | scutlixTextureSize | Vector2 | `private static Vector2 scutlixTextureSize;` | `private static Vector2 scutlixTextureSize;` |
| 282 | field | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 299 | 2 | scutlixBaseDamage | int | `public const int scutlixBaseDamage = 50;` | `public const int scutlixBaseDamage = 50;` |
| 292 | field | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 319 | 2 | santankTextureSize | Vector2 | `private static Vector2 santankTextureSize;` | `private static Vector2 santankTextureSize;` |

##### 属性（0）

无该类型成员记录。


#### 4.5.3 细分子系统：`MountDrillConstants`

- 细分职责：钻头二极管、钻取长度、功率、时间和光束数量定义。
- 边界角色：`definition/query`；最小 seam：只读 Definition/Catalog view；钻头行为系统按配置读取。
- 成员文件数：1；声明类型数：1；字段：9；属性：0；合计：9。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（9）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 283 | field | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 301 | 2 | drillDiodePoint1 | Vector2 | `public static Vector2 drillDiodePoint1 = new Vector2(36f, -6f);` | `public static Vector2 drillDiodePoint1 = new Vector2(36f, -6f);` |
| 284 | field | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 303 | 2 | drillDiodePoint2 | Vector2 | `public static Vector2 drillDiodePoint2 = new Vector2(36f, 8f);` | `public static Vector2 drillDiodePoint2 = new Vector2(36f, 8f);` |
| 285 | field | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 305 | 2 | drillTextureSize | Vector2 | `public static Vector2 drillTextureSize;` | `public static Vector2 drillTextureSize;` |
| 286 | field | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 307 | 2 | drillTextureWidth | int | `public const int drillTextureWidth = 80;` | `public const int drillTextureWidth = 80;` |
| 287 | field | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 309 | 2 | drillRotationChange | float | `public const float drillRotationChange = (float)Math.PI / 60f;` | `public const float drillRotationChange = (float)Math.PI / 60f;` |
| 288 | field | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 311 | 2 | drillPickPower | int | `public static int drillPickPower = 210;` | `public static int drillPickPower = 210;` |
| 289 | field | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 313 | 2 | drillPickTime | int | `public static int drillPickTime = 1;` | `public static int drillPickTime = 1;` |
| 290 | field | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 315 | 2 | amountOfBeamsAtOnce | int | `public static int amountOfBeamsAtOnce = 2;` | `public static int amountOfBeamsAtOnce = 2;` |
| 291 | field | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 317 | 2 | maxDrillLength | float | `public const float maxDrillLength = 48f;` | `public const float maxDrillLength = 48f;` |

##### 属性（0）

无该类型成员记录。


#### 4.5.4 细分子系统：`MountSuperCartConstants`

- 细分职责：超级矿车速度、加速度和跳跃能力常量。
- 边界角色：`definition/query`；最小 seam：只读 Definition/Catalog view；矿车移动系统单向读取。
- 成员文件数：1；声明类型数：1；字段：5；属性：0；合计：5。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（5）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 317 | field | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 369 | 2 | SuperCartRunSpeed | float | `public static float SuperCartRunSpeed = 20f;` | `public static float SuperCartRunSpeed = 20f;` |
| 318 | field | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 371 | 2 | SuperCartDashSpeed | float | `public static float SuperCartDashSpeed = 20f;` | `public static float SuperCartDashSpeed = 20f;` |
| 319 | field | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 373 | 2 | SuperCartAcceleration | float | `public static float SuperCartAcceleration = 0.1f;` | `public static float SuperCartAcceleration = 0.1f;` |
| 320 | field | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 375 | 2 | SuperCartJumpHeight | int | `public static int SuperCartJumpHeight = 15;` | `public static int SuperCartJumpHeight = 15;` |
| 321 | field | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 377 | 2 | SuperCartJumpSpeed | float | `public static float SuperCartJumpSpeed = 5.15f;` | `public static float SuperCartJumpSpeed = 5.15f;` |

##### 属性（0）

无该类型成员记录。


#### 4.5.5 细分子系统：`MountRuntimeFrameAndFlightState`

- 细分职责：坐骑运行时类型、帧、飞行、闲置和激活状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；移动阶段集中写入运行时状态。
- 成员文件数：1；声明类型数：1；字段：15；属性：0；合计：15。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（15）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 293 | field | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 321 | 2 | _data | Terraria.Mount.MountData | `private MountData _data;` | `private MountData _data;` |
| 294 | field | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 323 | 2 | _type | int | `private int _type;` | `private int _type;` |
| 295 | field | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 325 | 2 | _flipDraw | bool | `private bool _flipDraw;` | `private bool _flipDraw;` |
| 296 | field | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 327 | 2 | _frame | int | `private int _frame;` | `private int _frame;` |
| 297 | field | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 329 | 2 | _frameCounter | float | `private float _frameCounter;` | `private float _frameCounter;` |
| 298 | field | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 331 | 2 | _frameExtra | int | `private int _frameExtra;` | `private int _frameExtra;` |
| 299 | field | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 333 | 2 | _frameExtraCounter | float | `private float _frameExtraCounter;` | `private float _frameExtraCounter;` |
| 300 | field | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 335 | 2 | _frameState | int | `private int _frameState;` | `private int _frameState;` |
| 301 | field | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 337 | 2 | _flyTime | int | `private int _flyTime;` | `private int _flyTime;` |
| 302 | field | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 339 | 2 | _idleTime | int | `private int _idleTime;` | `private int _idleTime;` |
| 303 | field | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 341 | 2 | _idleTimeNext | int | `private int _idleTimeNext;` | `private int _idleTimeNext;` |
| 312 | field | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 359 | 2 | _shouldSuperCart | bool | `private bool _shouldSuperCart;` | `private bool _shouldSuperCart;` |
| 313 | field | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 361 | 2 | _walkingGraceTimeLeft | int | `private int _walkingGraceTimeLeft;` | `private int _walkingGraceTimeLeft;` |
| 315 | field | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 365 | 2 | _mountSpecificData | object | `private object _mountSpecificData;` | `private object _mountSpecificData;` |
| 316 | field | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 367 | 2 | _active | bool | `private bool _active;` | `private bool _active;` |

##### 属性（0）

无该类型成员记录。


#### 4.5.6 细分子系统：`MountFatigueAndAbilityState`

- 细分职责：坐骑疲劳、蓄力、冷却、持续时间和瞄准状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；能力转换按显式输入和冷却阶段更新。
- 成员文件数：1；声明类型数：1；字段：8；属性：0；合计：8。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（8）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 304 | field | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 343 | 2 | _fatigue | float | `private float _fatigue;` | `private float _fatigue;` |
| 305 | field | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 345 | 2 | _fatigueMax | float | `private float _fatigueMax;` | `private float _fatigueMax;` |
| 306 | field | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 347 | 2 | _abilityCharging | bool | `private bool _abilityCharging;` | `private bool _abilityCharging;` |
| 307 | field | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 349 | 2 | _abilityCharge | int | `private int _abilityCharge;` | `private int _abilityCharge;` |
| 308 | field | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 351 | 2 | _abilityCooldown | int | `private int _abilityCooldown;` | `private int _abilityCooldown;` |
| 309 | field | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 353 | 2 | _abilityDuration | int | `private int _abilityDuration;` | `private int _abilityDuration;` |
| 310 | field | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 355 | 2 | _abilityActive | bool | `private bool _abilityActive;` | `private bool _abilityActive;` |
| 311 | field | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 357 | 2 | _aiming | bool | `private bool _aiming;` | `private bool _aiming;` |

##### 属性（0）

无该类型成员记录。


#### 4.5.7 细分子系统：`MountRuntimeIdentityAndFrameProjection`

- 细分职责：坐骑活动、类型、帧、玩家偏移和几何投影。
- 边界角色：`derived/query`；最小 seam：Mount Projection/Query；只读消费 Mount 运行时快照。
- 成员文件数：1；声明类型数：1；字段：2；属性：11；合计：13。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（2）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 314 | field | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 363 | 2 | _debugDraw | System.Collections.Generic.List<Terraria.DataStructures.DrillDebugDraw> | `public List<DrillDebugDraw> _debugDraw;` | `public List<DrillDebugDraw> _debugDraw;` |
| 322 | field | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 379 | 2 | _defaultDelegatesData | Terraria.Mount.MountDelegatesData | `private MountDelegatesData _defaultDelegatesData = new MountDelegatesData();` | `private MountDelegatesData _defaultDelegatesData = new MountDelegatesData();` |

##### 属性（11）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 324 | property | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 387 | 2 | Active | bool | `public bool Active => _active;` | `public bool Active => _active;` |
| 325 | property | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 389 | 2 | Type | int | `public int Type => _type;` | `public int Type => _type;` |
| 326 | property | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 391 | 2 | Frame | int | `public int Frame => _frame;` | `public int Frame => _frame;` |
| 327 | property | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 393 | 2 | FlyTime | int | `public int FlyTime => _flyTime;` | `public int FlyTime => _flyTime;` |
| 328 | property | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 395 | 2 | BodyFrame | int | `public int BodyFrame => _data.bodyFrame;` | `public int BodyFrame => _data.bodyFrame;` |
| 329 | property | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 397 | 2 | RunningGraceTime | int | `public int RunningGraceTime => _walkingGraceTimeLeft;` | `public int RunningGraceTime => _walkingGraceTimeLeft;` |
| 330 | property | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 399 | 2 | PlayerXOFfset | int | `public int PlayerXOFfset => _data.playerXOffset;` | `public int PlayerXOFfset => _data.playerXOffset;` |
| 331 | property | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 401 | 2 | PlayerOffset | int | `public int PlayerOffset { get { if (!_active) { return 0; } if (_frame >= _data.totalFrames) { return 0; } return _data.playerYOffsets[_frame]; } }` | `public int PlayerOffset { get { if (!_active) { return 0; } if (_frame >= _data.totalFrames) { return 0; } return _data.playerYOffsets[_frame]; } }` |
| 332 | property | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 417 | 2 | PlayerOffsetHitbox | int | `public int PlayerOffsetHitbox { get { if (!_active) { return 0; } return -PlayerOffset + _data.heightBoost; } }` | `public int PlayerOffsetHitbox { get { if (!_active) { return 0; } return -PlayerOffset + _data.heightBoost; } }` |
| 333 | property | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 429 | 2 | PlayerHeadOffset | int | `public int PlayerHeadOffset { get { if (!_active) { return 0; } return _data.playerHeadOffset; } }` | `public int PlayerHeadOffset { get { if (!_active) { return 0; } return _data.playerHeadOffset; } }` |
| 334 | property | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 441 | 2 | HeightBoost | int | `public int HeightBoost => _data.heightBoost;` | `public int HeightBoost => _data.heightBoost;` |


#### 4.5.8 细分子系统：`MountRuntimeMobilityAndAbilityProjection`

- 细分职责：坐骑移动、轨道、翅膀和能力计时投影。
- 边界角色：`derived/query`；最小 seam：Mount Ability Projection/Query；不复制 Mount 权威状态。
- 成员文件数：1；声明类型数：1；字段：0；属性：16；合计：16。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（0）

无该类型成员记录。

##### 属性（16）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 335 | property | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 443 | 2 | RunSpeed | float | `public float RunSpeed { get { if (_type == 4 && _frameState == 4) { return _data.swimSpeed; } if ((_type == 12 \|\| _type == 44 \|\| _type == 49) && _frameState == 4) { return _data.swimSpeed; } if (_type == 12 && _frameState == 2) { return _data.runSpeed + 13.5f; } if (_type == 44 && _frameState == 2) { return _data.runSpeed + 4f; } if (_type == 5 && _frameState == 2) { float num = _fatigue / _fatigueMax; return _data.runSpeed + 4f * (1f - num); } if (_type == 50 && _frameState == 2) { return _data.runSpeed + 2f; } if (_shouldSuperCart) { return SuperCartRunSpeed; } return _data.runSpeed; } }` | `public float RunSpeed { get { if (_type == 4 && _frameState == 4) { return _data.swimSpeed; } if ((_type == 12 \|\| _type == 44 \|\| _type == 49) && _frameState == 4) { return _data.swimSpeed; } if (_type == 12 && _frameState == 2) { return _data.runSpeed + 13.5f; } if (_type == 44 && _frameState == 2) { return _data.runSpeed + 4f; } if (_type == 5 && _frameState == 2) { float num = _fatigue / _fatigueMax; return _data.runSpeed + 4f * (1f - num); } if (_type == 50 && _frameState == 2) { return _data.runSpeed + 2f; } if (_shouldSuperCart) { return SuperCartRunSpeed; } return _data.runSpeed; } }` |
| 336 | property | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 480 | 2 | DashSpeed | float | `public float DashSpeed { get { if (_shouldSuperCart) { return SuperCartDashSpeed; } return _data.dashSpeed; } }` | `public float DashSpeed { get { if (_shouldSuperCart) { return SuperCartDashSpeed; } return _data.dashSpeed; } }` |
| 337 | property | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 492 | 2 | Acceleration | float | `public float Acceleration { get { if (_shouldSuperCart) { return SuperCartAcceleration; } return _data.acceleration; } }` | `public float Acceleration { get { if (_shouldSuperCart) { return SuperCartAcceleration; } return _data.acceleration; } }` |
| 338 | property | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 504 | 2 | AutoJump | bool | `public bool AutoJump => _data.constantJump;` | `public bool AutoJump => _data.constantJump;` |
| 339 | property | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 506 | 2 | BlockExtraJumps | bool | `public bool BlockExtraJumps => _data.blockExtraJumps;` | `public bool BlockExtraJumps => _data.blockExtraJumps;` |
| 340 | property | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 508 | 2 | IsConsideredASlimeMount | bool | `public bool IsConsideredASlimeMount { get { if (_type != 3) { return _type == 50; } return true; } }` | `public bool IsConsideredASlimeMount { get { if (_type != 3) { return _type == 50; } return true; } }` |
| 341 | property | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 520 | 2 | Cart | bool | `public bool Cart { get { if (_data == null \|\| !_active) { return false; } return _data.Minecart; } }` | `public bool Cart { get { if (_data == null \|\| !_active) { return false; } return _data.Minecart; } }` |
| 342 | property | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 532 | 2 | CanGrindRails | bool | `public bool CanGrindRails { get { if (_data == null \|\| !_active) { return false; } return _data.CanRideMinecartTracks; } }` | `public bool CanGrindRails { get { if (_data == null \|\| !_active) { return false; } return _data.CanRideMinecartTracks; } }` |
| 343 | property | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 544 | 2 | AnyTrackRider | bool | `public bool AnyTrackRider { get { if (_data == null \|\| !_active) { return false; } if (!_data.Minecart) { return _data.CanRideMinecartTracks; } return true; } }` | `public bool AnyTrackRider { get { if (_data == null \|\| !_active) { return false; } if (!_data.Minecart) { return _data.CanRideMinecartTracks; } return true; } }` |
| 344 | property | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 560 | 2 | CanUseWings | bool | `public bool CanUseWings { get { if (_data == null \|\| !_active) { return true; } return _data.CanUseWings; } }` | `public bool CanUseWings { get { if (_data == null \|\| !_active) { return true; } return _data.CanUseWings; } }` |
| 345 | property | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 572 | 2 | Delegations | Terraria.Mount.MountDelegatesData | `public MountDelegatesData Delegations { get { if (_data == null) { return _defaultDelegatesData; } return _data.delegations; } }` | `public MountDelegatesData Delegations { get { if (_data == null) { return _defaultDelegatesData; } return _data.delegations; } }` |
| 346 | property | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 584 | 2 | AbilityCharging | bool | `public bool AbilityCharging => _abilityCharging;` | `public bool AbilityCharging => _abilityCharging;` |
| 347 | property | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 586 | 2 | AbilityActive | bool | `public bool AbilityActive => _abilityActive;` | `public bool AbilityActive => _abilityActive;` |
| 348 | property | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 588 | 2 | AbilityCharge | float | `public float AbilityCharge => (float)_abilityCharge / (float)_data.abilityChargeMax;` | `public float AbilityCharge => (float)_abilityCharge / (float)_data.abilityChargeMax;` |
| 349 | property | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 590 | 2 | AllowDirectionChange | bool | `public bool AllowDirectionChange { get { int type = _type; if (type == 9) { return _abilityCooldown < _data.abilityCooldown / 2; } return true; } }` | `public bool AllowDirectionChange { get { int type = _type; if (type == 9) { return _abilityCooldown < _data.abilityCooldown / 2; } return true; } }` |
| 350 | property | Terraria.Mount | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 603 | 2 | DismountOnItemUse | bool | `public bool DismountOnItemUse { get { if (!Active) { return false; } return _data.dismountsOnItemUse; } }` | `public bool DismountOnItemUse { get { if (!Active) { return false; } return _data.dismountsOnItemUse; } }` |


#### 4.5.9 细分子系统：`MountGeometryAndOffsetCatalog`

- 细分职责：坐骑纹理尺寸、玩家偏移、碰撞高度和身体定位定义。
- 边界角色：`definition/query`；最小 seam：只读 Definition/Catalog view；移动和绘制系统按几何快照消费。
- 成员文件数：1；声明类型数：1；字段：9；属性：0；合计：9。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（9）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 210 | field | Terraria.Mount.MountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 147 | 3 | textureWidth | int | `public int textureWidth;` | `public int textureWidth;` |
| 211 | field | Terraria.Mount.MountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 149 | 3 | textureHeight | int | `public int textureHeight;` | `public int textureHeight;` |
| 212 | field | Terraria.Mount.MountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 151 | 3 | xOffset | int | `public int xOffset;` | `public int xOffset;` |
| 213 | field | Terraria.Mount.MountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 153 | 3 | yOffset | int | `public int yOffset;` | `public int yOffset;` |
| 214 | field | Terraria.Mount.MountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 155 | 3 | playerYOffsets | int[] | `public int[] playerYOffsets;` | `public int[] playerYOffsets;` |
| 215 | field | Terraria.Mount.MountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 157 | 3 | bodyFrame | int | `public int bodyFrame;` | `public int bodyFrame;` |
| 216 | field | Terraria.Mount.MountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 159 | 3 | playerHeadOffset | int | `public int playerHeadOffset;` | `public int playerHeadOffset;` |
| 217 | field | Terraria.Mount.MountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 161 | 3 | heightBoost | int | `public int heightBoost;` | `public int heightBoost;` |
| 268 | field | Terraria.Mount.MountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 263 | 3 | playerXOffset | int | `public int playerXOffset;` | `public int playerXOffset;` |

##### 属性（0）

无该类型成员记录。


#### 4.5.10 细分子系统：`MountGroundAnimationFrames`

- 细分职责：站立、奔跑和地面空闲动画帧目录。
- 边界角色：`definition/query`；最小 seam：Mount Animation Query；地面帧目录只读消费。
- 成员文件数：1；声明类型数：1；字段：11；属性：0；合计：11。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（11）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 239 | field | Terraria.Mount.MountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 205 | 3 | totalFrames | int | `public int totalFrames;` | `public int totalFrames;` |
| 240 | field | Terraria.Mount.MountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 207 | 3 | standingFrameStart | int | `public int standingFrameStart;` | `public int standingFrameStart;` |
| 241 | field | Terraria.Mount.MountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 209 | 3 | standingFrameCount | int | `public int standingFrameCount;` | `public int standingFrameCount;` |
| 242 | field | Terraria.Mount.MountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 211 | 3 | standingFrameDelay | int | `public int standingFrameDelay;` | `public int standingFrameDelay;` |
| 243 | field | Terraria.Mount.MountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 213 | 3 | runningFrameStart | int | `public int runningFrameStart;` | `public int runningFrameStart;` |
| 244 | field | Terraria.Mount.MountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 215 | 3 | runningFrameCount | int | `public int runningFrameCount;` | `public int runningFrameCount;` |
| 245 | field | Terraria.Mount.MountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 217 | 3 | runningFrameDelay | int | `public int runningFrameDelay;` | `public int runningFrameDelay;` |
| 252 | field | Terraria.Mount.MountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 231 | 3 | idleFrameStart | int | `public int idleFrameStart;` | `public int idleFrameStart;` |
| 253 | field | Terraria.Mount.MountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 233 | 3 | idleFrameCount | int | `public int idleFrameCount;` | `public int idleFrameCount;` |
| 254 | field | Terraria.Mount.MountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 235 | 3 | idleFrameDelay | int | `public int idleFrameDelay;` | `public int idleFrameDelay;` |
| 255 | field | Terraria.Mount.MountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 237 | 3 | idleFrameLoop | bool | `public bool idleFrameLoop;` | `public bool idleFrameLoop;` |

##### 属性（0）

无该类型成员记录。


#### 4.5.11 细分子系统：`MountAerialAndWaterAnimationFrames`

- 细分职责：飞行、空中和游泳动画帧目录。
- 边界角色：`definition/query`；最小 seam：Mount Animation Query；空中/水中帧目录只读消费。
- 成员文件数：1；声明类型数：1；字段：9；属性：0；合计：9。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（9）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 246 | field | Terraria.Mount.MountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 219 | 3 | flyingFrameStart | int | `public int flyingFrameStart;` | `public int flyingFrameStart;` |
| 247 | field | Terraria.Mount.MountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 221 | 3 | flyingFrameCount | int | `public int flyingFrameCount;` | `public int flyingFrameCount;` |
| 248 | field | Terraria.Mount.MountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 223 | 3 | flyingFrameDelay | int | `public int flyingFrameDelay;` | `public int flyingFrameDelay;` |
| 249 | field | Terraria.Mount.MountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 225 | 3 | inAirFrameStart | int | `public int inAirFrameStart;` | `public int inAirFrameStart;` |
| 250 | field | Terraria.Mount.MountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 227 | 3 | inAirFrameCount | int | `public int inAirFrameCount;` | `public int inAirFrameCount;` |
| 251 | field | Terraria.Mount.MountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 229 | 3 | inAirFrameDelay | int | `public int inAirFrameDelay;` | `public int inAirFrameDelay;` |
| 256 | field | Terraria.Mount.MountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 239 | 3 | swimFrameStart | int | `public int swimFrameStart;` | `public int swimFrameStart;` |
| 257 | field | Terraria.Mount.MountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 241 | 3 | swimFrameCount | int | `public int swimFrameCount;` | `public int swimFrameCount;` |
| 258 | field | Terraria.Mount.MountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 243 | 3 | swimFrameDelay | int | `public int swimFrameDelay;` | `public int swimFrameDelay;` |

##### 属性（0）

无该类型成员记录。


#### 4.5.12 细分子系统：`MountDashAnimationFrames`

- 细分职责：冲刺动画帧目录。
- 边界角色：`definition/query`；最小 seam：Mount Dash Animation Query；冲刺帧按移动状态读取。
- 成员文件数：1；声明类型数：1；字段：3；属性：0；合计：3。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（3）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 259 | field | Terraria.Mount.MountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 245 | 3 | dashingFrameStart | int | `public int dashingFrameStart;` | `public int dashingFrameStart;` |
| 260 | field | Terraria.Mount.MountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 247 | 3 | dashingFrameCount | int | `public int dashingFrameCount;` | `public int dashingFrameCount;` |
| 261 | field | Terraria.Mount.MountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 249 | 3 | dashingFrameDelay | int | `public int dashingFrameDelay;` | `public int dashingFrameDelay;` |

##### 属性（0）

无该类型成员记录。


#### 4.5.13 细分子系统：`MountMovementAndAbilityCatalog`

- 细分职责：坐骑速度、跳跃、飞行、疲劳和能力参数。
- 边界角色：`definition/query`；最小 seam：只读 Definition/Catalog view；能力系统不得反向修改定义。
- 成员文件数：1；声明类型数：1；字段：18；属性：0；合计：18。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（18）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 219 | field | Terraria.Mount.MountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 165 | 3 | flightTimeMax | int | `public int flightTimeMax;` | `public int flightTimeMax;` |
| 220 | field | Terraria.Mount.MountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 167 | 3 | usesHover | bool | `public bool usesHover;` | `public bool usesHover;` |
| 221 | field | Terraria.Mount.MountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 169 | 3 | runSpeed | float | `public float runSpeed;` | `public float runSpeed;` |
| 222 | field | Terraria.Mount.MountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 171 | 3 | dashSpeed | float | `public float dashSpeed;` | `public float dashSpeed;` |
| 223 | field | Terraria.Mount.MountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 173 | 3 | swimSpeed | float | `public float swimSpeed;` | `public float swimSpeed;` |
| 224 | field | Terraria.Mount.MountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 175 | 3 | acceleration | float | `public float acceleration;` | `public float acceleration;` |
| 225 | field | Terraria.Mount.MountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 177 | 3 | jumpSpeed | float | `public float jumpSpeed;` | `public float jumpSpeed;` |
| 226 | field | Terraria.Mount.MountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 179 | 3 | jumpHeight | int | `public int jumpHeight;` | `public int jumpHeight;` |
| 227 | field | Terraria.Mount.MountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 181 | 3 | fallDamage | float | `public float fallDamage;` | `public float fallDamage;` |
| 228 | field | Terraria.Mount.MountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 183 | 3 | extraFall | int | `public int extraFall;` | `public int extraFall;` |
| 229 | field | Terraria.Mount.MountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 185 | 3 | fatigueMax | int | `public int fatigueMax;` | `public int fatigueMax;` |
| 230 | field | Terraria.Mount.MountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 187 | 3 | constantJump | bool | `public bool constantJump;` | `public bool constantJump;` |
| 231 | field | Terraria.Mount.MountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 189 | 3 | blockExtraJumps | bool | `public bool blockExtraJumps;` | `public bool blockExtraJumps;` |
| 232 | field | Terraria.Mount.MountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 191 | 3 | abilityChargeMax | int | `public int abilityChargeMax;` | `public int abilityChargeMax;` |
| 233 | field | Terraria.Mount.MountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 193 | 3 | abilityDuration | int | `public int abilityDuration;` | `public int abilityDuration;` |
| 234 | field | Terraria.Mount.MountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 195 | 3 | abilityCooldown | int | `public int abilityCooldown;` | `public int abilityCooldown;` |
| 235 | field | Terraria.Mount.MountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 197 | 3 | walkingGraceTimeMax | int | `public int walkingGraceTimeMax;` | `public int walkingGraceTimeMax;` |
| 236 | field | Terraria.Mount.MountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 199 | 3 | dismountsOnItemUse | bool | `public bool dismountsOnItemUse;` | `public bool dismountsOnItemUse;` |

##### 属性（0）

无该类型成员记录。


#### 4.5.14 细分子系统：`MountVehicleAndPresentationCatalog`

- 细分职责：矿车轨道、坐骑增益、光照和生成表现定义。
- 边界角色：`definition/query`；最小 seam：只读 Definition/Catalog view；表现和车辆适配器单向消费。
- 成员文件数：1；声明类型数：1；字段：9；属性：0；合计：9。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（9）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 218 | field | Terraria.Mount.MountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 163 | 3 | buff | int | `public int buff;` | `public int buff;` |
| 237 | field | Terraria.Mount.MountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 201 | 3 | spawnDust | int | `public int spawnDust;` | `public int spawnDust;` |
| 238 | field | Terraria.Mount.MountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 203 | 3 | spawnDustNoGravity | bool | `public bool spawnDustNoGravity;` | `public bool spawnDustNoGravity;` |
| 262 | field | Terraria.Mount.MountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 251 | 3 | Minecart | bool | `public bool Minecart;` | `public bool Minecart;` |
| 263 | field | Terraria.Mount.MountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 253 | 3 | CanRideMinecartTracks | bool | `public bool CanRideMinecartTracks;` | `public bool CanRideMinecartTracks;` |
| 264 | field | Terraria.Mount.MountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 255 | 3 | CanUseWings | bool | `public bool CanUseWings;` | `public bool CanUseWings;` |
| 265 | field | Terraria.Mount.MountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 257 | 3 | lightColor | Vector3 | `public Vector3 lightColor = Vector3.One;` | `public Vector3 lightColor = Vector3.One;` |
| 266 | field | Terraria.Mount.MountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 259 | 3 | emitsLight | bool | `public bool emitsLight;` | `public bool emitsLight;` |
| 267 | field | Terraria.Mount.MountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 261 | 3 | delegations | Terraria.Mount.MountDelegatesData | `public MountDelegatesData delegations = new MountDelegatesData();` | `public MountDelegatesData delegations = new MountDelegatesData();` |

##### 属性（0）

无该类型成员记录。


#### 4.5.15 细分子系统：`MountDelegateContract`

- 细分职责：坐骑特化的尘土、声音、手/嘴和尺寸委托数据。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System 读取输入并通过显式 Command/CommitPort 写入；跨组只用事件、Query 或命令交接。
- 成员文件数：1；声明类型数：1；字段：8；属性：0；合计：8。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（8）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 202 | field | Terraria.Mount.MountDelegatesData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 104 | 3 | MinecartDust | System.Action<Vector2> | `public Action<Vector2> MinecartDust;` | `public Action<Vector2> MinecartDust;` |
| 203 | field | Terraria.Mount.MountDelegatesData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 106 | 3 | MinecartJumpingSound | System.Action<Terraria.Player, Vector2, int, int> | `public Action<Player, Vector2, int, int> MinecartJumpingSound;` | `public Action<Player, Vector2, int, int> MinecartJumpingSound;` |
| 204 | field | Terraria.Mount.MountDelegatesData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 108 | 3 | MinecartLandingSound | System.Action<Terraria.Player, Vector2, int, int> | `public Action<Player, Vector2, int, int> MinecartLandingSound;` | `public Action<Player, Vector2, int, int> MinecartLandingSound;` |
| 205 | field | Terraria.Mount.MountDelegatesData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 110 | 3 | MinecartBumperSound | System.Action<Terraria.Player, Vector2, int, int> | `public Action<Player, Vector2, int, int> MinecartBumperSound;` | `public Action<Player, Vector2, int, int> MinecartBumperSound;` |
| 206 | field | Terraria.Mount.MountDelegatesData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 112 | 3 | MouthPosition | Terraria.Mount.MountDelegatesData.OverridePositionMethod | `public OverridePositionMethod MouthPosition;` | `public OverridePositionMethod MouthPosition;` |
| 207 | field | Terraria.Mount.MountDelegatesData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 114 | 3 | HandPosition | Terraria.Mount.MountDelegatesData.OverridePositionMethod | `public OverridePositionMethod HandPosition;` | `public OverridePositionMethod HandPosition;` |
| 208 | field | Terraria.Mount.MountDelegatesData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 116 | 3 | PlayerSize | Terraria.Mount.MountDelegatesData.OverrideSizeMethod | `public OverrideSizeMethod PlayerSize;` | `public OverrideSizeMethod PlayerSize;` |
| 209 | field | Terraria.Mount.MountDelegatesData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 118 | 3 | DashDust | Terraria.Mount.MountDelegatesData.AdjustDashDustMethod | `public AdjustDashDustMethod DashDust;` | `public AdjustDashDustMethod DashDust;` |

##### 属性（0）

无该类型成员记录。


#### 4.5.16 细分子系统：`DrillMountRuntime`

- 细分职责：钻头坐骑的目标、光束、旋转和钻头冷却。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System 读取输入并通过显式 Command/CommitPort 写入；跨组只用事件、Query 或命令交接。
- 成员文件数：1；声明类型数：2；字段：9；属性：0；合计：9。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（9）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 188 | field | Terraria.Mount.DrillBeam | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 22 | 3 | curTileTarget | Terraria.DataStructures.Point16 | `public Point16 curTileTarget;` | `public Point16 curTileTarget;` |
| 189 | field | Terraria.Mount.DrillBeam | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 24 | 3 | cooldown | int | `public int cooldown;` | `public int cooldown;` |
| 190 | field | Terraria.Mount.DrillBeam | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 26 | 3 | lastPurpose | int | `public int lastPurpose;` | `public int lastPurpose;` |
| 191 | field | Terraria.Mount.DrillMountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 38 | 3 | diodeRotationTarget | float | `public float diodeRotationTarget;` | `public float diodeRotationTarget;` |
| 192 | field | Terraria.Mount.DrillMountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 40 | 3 | diodeRotation | float | `public float diodeRotation;` | `public float diodeRotation;` |
| 193 | field | Terraria.Mount.DrillMountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 42 | 3 | outerRingRotation | float | `public float outerRingRotation;` | `public float outerRingRotation;` |
| 194 | field | Terraria.Mount.DrillMountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 44 | 3 | beams | Terraria.Mount.DrillBeam[] | `public DrillBeam[] beams;` | `public DrillBeam[] beams;` |
| 195 | field | Terraria.Mount.DrillMountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 46 | 3 | beamCooldown | int | `public int beamCooldown;` | `public int beamCooldown;` |
| 196 | field | Terraria.Mount.DrillMountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 48 | 3 | crosshairPosition | Vector2 | `public Vector2 crosshairPosition;` | `public Vector2 crosshairPosition;` |

##### 属性（0）

无该类型成员记录。


#### 4.5.17 细分子系统：`MountVariantFlags`

- 细分职责：选择性飞行、额外帧和布尔特化状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System 读取输入并通过显式 Command/CommitPort 写入；跨组只用事件、Query 或命令交接。
- 成员文件数：1；声明类型数：3；字段：5；属性：0；合计：5。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（5）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 197 | field | Terraria.Mount.BooleanMountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 62 | 3 | boolean | bool | `public bool boolean;` | `public bool boolean;` |
| 198 | field | Terraria.Mount.SelectiveFlyingMountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 72 | 3 | showFlyingFrames | bool | `public bool showFlyingFrames;` | `public bool showFlyingFrames;` |
| 199 | field | Terraria.Mount.SelectiveFlyingMountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 74 | 3 | allowedToFly | bool | `public bool allowedToFly;` | `public bool allowedToFly;` |
| 200 | field | Terraria.Mount.ExtraFrameMountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 85 | 3 | frame | int | `public int frame;` | `public int frame;` |
| 201 | field | Terraria.Mount.ExtraFrameMountData | Terraria/Mount.cs | D:\TRbackup\Version4\Terraria\Mount.cs | 87 | 3 | frameCounter | float | `public float frameCounter;` | `public float frameCounter;` |

##### 属性（0）

无该类型成员记录。


## 8. 本分区自检

- 叶子子系统：17 个；指定组均已写入且没有重复分区。
- 字段/属性/成员：136 / 27 / 163。
- 来源序号范围：188..350；本分区内唯一序号：True。
- 成员声明表逐组从源报告提取，未改写来源序号、类型、路径、行列、成员、C# 类型、成员声明或原始声明。
- 运行时读写者、生命周期、持久化、网络和调度：未验证，保留为迁移前风险。
