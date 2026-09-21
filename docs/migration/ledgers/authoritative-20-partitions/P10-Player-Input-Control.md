# P10 玩家输入、控制、选择与建造交互 - 权威模拟系统成员组件候选分区

> 来源：`docs/migration/ledgers/Version4权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md`；本文件只做成员库存分区和候选组件边界记录。
> 这是候选组件拆分台账，不是运行时迁移完成、行为等价、API 兼容、网络闭合或持久化闭合证明。

## 1. 分区边界

本分区包含 11 个叶子子系统，字段 110 条、属性 8 条、成员合计 118 条；每个叶子子系统保持原正式父级归属，不跨分区拆行。

## 2. 分区覆盖概览

| 叶子子系统 | 正式父级 | 字段 | 属性 | 合计 | 源边界角色 |
|---|---|---:|---:|---:|---|
| `PlayerInteractionInputState` | `PlayerGameplay` | 14 | 0 | 14 | authoritative state/behavior |
| `PlayerControlAndReleaseInput` | `PlayerGameplay` | 20 | 0 | 20 | authoritative state/behavior |
| `PlayerItemUseAndChannelIntent` | `PlayerGameplay` | 16 | 0 | 16 | authoritative state/behavior |
| `PlayerInformationWorldAndMovementState` | `PlayerGameplay` | 5 | 0 | 5 | authoritative state/behavior |
| `PlayerInformationNavigationAndTimeState` | `PlayerGameplay` | 7 | 0 | 7 | authoritative state/behavior |
| `PlayerInformationDetectionAndWiringState` | `PlayerGameplay` | 8 | 0 | 8 | authoritative state/behavior |
| `PlayerBuilderInteractionDefinitions` | `PlayerGameplay` | 13 | 0 | 13 | definition/query |
| `PlayerSelectionState` | `PlayerGameplay` | 9 | 6 | 15 | registry/projection |
| `PlayerInputSyncAndMatch` | `PlayerGameplay` | 13 | 1 | 14 | registry/projection |
| `PlayerBuilderOverlayState` | `PlayerGameplay` | 2 | 0 | 2 | registry/projection |
| `PlayerItemSpaceAndSettings` | `PlayerGameplay` | 3 | 1 | 4 | authoritative state/behavior |

## 3. 候选组件归属与 seam

| 叶子子系统 | 候选组件边界 | 类型分类 | 最小 seam | 独立生命周期 | 证据状态 | 主要风险 |
|---|---|---|---|---|---|---|
| `PlayerInteractionInputState` | `PlayerInteractionInputStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerControlAndReleaseInput` | `PlayerControlAndReleaseInputComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerItemUseAndChannelIntent` | `PlayerItemUseAndChannelIntentComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerInformationWorldAndMovementState` | `PlayerInformationWorldAndMovementStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerInformationNavigationAndTimeState` | `PlayerInformationNavigationAndTimeStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerInformationDetectionAndWiringState` | `PlayerInformationDetectionAndWiringStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerBuilderInteractionDefinitions` | `PlayerBuilderInteractionDefinitionsDefinition`/`Query` | Definition/Query | 只读 Definition/Catalog view | 候选纯读边界；仍需核对失效/缓存条件 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerSelectionState` | `PlayerSelectionStateProjection`/`Adapter` | Projection/Adapter | 单向 Projection/Adapter seam；不得回写权威状态 | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerInputSyncAndMatch` | `PlayerInputSyncAndMatchProjection`/`Adapter` | Projection/Adapter | 单向 Projection/Adapter seam；不得回写权威状态 | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerBuilderOverlayState` | `PlayerBuilderOverlayStateProjection`/`Adapter` | Projection/Adapter | 单向 Projection/Adapter seam；不得回写权威状态 | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerItemSpaceAndSettings` | `PlayerItemSpaceAndSettingsComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |

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
- 本分区父级局部统计：字段 110；属性 8；合计 118；完整父级统计以源报告为准。

#### 4.13.27 细分子系统：`PlayerInteractionInputState`

- 细分职责：队伍、交互界面、物品复用和选中目标输入状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；输入/交互命令单向更新。
- 成员文件数：1；声明类型数：1；字段：14；属性：0；合计：14。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（14）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 691 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 978 | 2 | team | int | `public int team;` | `public int team;` |
| 694 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 984 | 2 | nameLen | int | `public static int nameLen = 20;` | `public static int nameLen = 20;` |
| 696 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 988 | 2 | sign | int | `public int sign = -1;` | `public int sign = -1;` |
| 697 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 991 | 2 | reuseDelay | int | `public int reuseDelay;` | `public int reuseDelay;` |
| 698 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 993 | 2 | aggro | int | `public int aggro;` | `public int aggro;` |
| 699 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 995 | 2 | nearbyActiveNPCs | float | `public float nearbyActiveNPCs;` | `public float nearbyActiveNPCs;` |
| 700 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 997 | 2 | creativeInterface | bool | `public bool creativeInterface;` | `public bool creativeInterface;` |
| 701 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 999 | 2 | mouseInterface | bool | `public bool mouseInterface;` | `public bool mouseInterface;` |
| 702 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1001 | 2 | lastMouseInterface | bool | `public bool lastMouseInterface;` | `public bool lastMouseInterface;` |
| 703 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1003 | 2 | noThrow | int | `public int noThrow;` | `public int noThrow;` |
| 704 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1005 | 2 | changeItem | int | `public int changeItem = -1;` | `public int changeItem = -1;` |
| 705 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1007 | 2 | pendingItemReuse | bool | `public bool pendingItemReuse;` | `public bool pendingItemReuse;` |
| 706 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1009 | 2 | selectedItemState | Terraria.Player.SelectedItemState | `public SelectedItemState selectedItemState;` | `public SelectedItemState selectedItemState;` |
| 707 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1011 | 2 | selectedKite | int | `public int selectedKite;` | `public int selectedKite;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.37 细分子系统：`PlayerControlAndReleaseInput`

- 细分职责：方向、跳跃、释放、悬停和连续输入窗口。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；输入帧通过显式命令交给行为系统。
- 成员文件数：1；声明类型数：1；字段：20；属性：0；合计：20。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（20）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 809 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1219 | 2 | controlLeft | bool | `public bool controlLeft;` | `public bool controlLeft;` |
| 810 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1221 | 2 | controlRight | bool | `public bool controlRight;` | `public bool controlRight;` |
| 811 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1223 | 2 | controlUp | bool | `public bool controlUp;` | `public bool controlUp;` |
| 812 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1225 | 2 | controlDown | bool | `public bool controlDown;` | `public bool controlDown;` |
| 813 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1227 | 2 | controlJump | bool | `public bool controlJump;` | `public bool controlJump;` |
| 816 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1235 | 2 | controlTorch | bool | `public bool controlTorch;` | `public bool controlTorch;` |
| 817 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1241 | 2 | controlDash | bool | `public bool controlDash;` | `public bool controlDash;` |
| 818 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1245 | 2 | releaseJump | bool | `public bool releaseJump;` | `public bool releaseJump;` |
| 819 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1247 | 2 | releaseUp | bool | `public bool releaseUp;` | `public bool releaseUp;` |
| 820 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1249 | 2 | releaseUseItem | bool | `public bool releaseUseItem;` | `public bool releaseUseItem;` |
| 821 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1251 | 2 | releaseUseTile | bool | `public bool releaseUseTile;` | `public bool releaseUseTile;` |
| 822 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1254 | 2 | releaseLeft | bool | `public bool releaseLeft;` | `public bool releaseLeft;` |
| 823 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1256 | 2 | releaseRight | bool | `public bool releaseRight;` | `public bool releaseRight;` |
| 824 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1260 | 2 | releaseDown | bool | `public bool releaseDown;` | `public bool releaseDown;` |
| 825 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1264 | 2 | releaseDash | bool | `public bool releaseDash;` | `public bool releaseDash;` |
| 827 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1270 | 2 | controlDownHold | bool | `public bool controlDownHold;` | `public bool controlDownHold;` |
| 831 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1280 | 2 | tryKeepingHoveringDown | bool | `public bool tryKeepingHoveringDown;` | `public bool tryKeepingHoveringDown;` |
| 832 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1282 | 2 | tryKeepingHoveringUp | bool | `public bool tryKeepingHoveringUp;` | `public bool tryKeepingHoveringUp;` |
| 834 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1288 | 2 | leftTimer | int | `public int leftTimer;` | `public int leftTimer;` |
| 835 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1290 | 2 | rightTimer | int | `public int rightTimer;` | `public int rightTimer;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.38 细分子系统：`PlayerItemUseAndChannelIntent`

- 细分职责：物品使用、交互、频道、法力消耗和行动意图状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；物品动作意图单向进入使用系统。
- 成员文件数：1；声明类型数：1；字段：16；属性：0；合计：16。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（16）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 814 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1229 | 2 | controlUseItem | bool | `public bool controlUseItem;` | `public bool controlUseItem;` |
| 815 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1231 | 2 | controlUseTile | bool | `public bool controlUseTile;` | `public bool controlUseTile;` |
| 826 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1268 | 2 | tileInteractAttempted | bool | `public bool tileInteractAttempted;` | `public bool tileInteractAttempted;` |
| 828 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1272 | 2 | isOperatingAnotherEntity | bool | `public bool isOperatingAnotherEntity;` | `public bool isOperatingAnotherEntity;` |
| 829 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1274 | 2 | lastItemUseAttemptSuccess | bool | `public bool lastItemUseAttemptSuccess;` | `public bool lastItemUseAttemptSuccess;` |
| 830 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1276 | 2 | autoReuseAllWeapons | bool | `public bool autoReuseAllWeapons;` | `public bool autoReuseAllWeapons;` |
| 833 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1284 | 2 | altFunctionUse | int | `public int altFunctionUse;` | `public int altFunctionUse;` |
| 836 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1292 | 2 | delayUseItem | bool | `public bool delayUseItem;` | `public bool delayUseItem;` |
| 844 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1314 | 2 | manaCost | float | `public float manaCost = 1f;` | `public float manaCost = 1f;` |
| 845 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1316 | 2 | fireWalk | bool | `public bool fireWalk;` | `public bool fireWalk;` |
| 846 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1318 | 2 | channel | bool | `public bool channel;` | `public bool channel;` |
| 847 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1321 | 2 | TagEffectState | Terraria.GameContent.Items.TagEffectState | `public TagEffectState TagEffectState;` | `public TagEffectState TagEffectState;` |
| 848 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1323 | 2 | IntentionGuesser | Terraria.DataStructures.PlayerIntentionGuesser | `public PlayerIntentionGuesser IntentionGuesser;` | `public PlayerIntentionGuesser IntentionGuesser;` |
| 849 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1325 | 2 | _channelShotCache | Terraria.Player.ChannelCancelKey | `private ChannelCancelKey _channelShotCache;` | `private ChannelCancelKey _channelShotCache;` |
| 851 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1329 | 2 | rabbitOrderFrame | Terraria.Player.RabbitOrderFrameHelper | `public RabbitOrderFrameHelper rabbitOrderFrame;` | `public RabbitOrderFrameHelper rabbitOrderFrame;` |
| 852 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1331 | 2 | creativeGodMode | bool | `public bool creativeGodMode;` | `public bool creativeGodMode;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.68 细分子系统：`PlayerInformationWorldAndMovementState`

- 细分职责：敌对标志、移动声效、帧内位移和生物命中信息。
- 边界角色：`authoritative state/behavior`；最小 seam：Player Information System/CommitPort；帧内交互状态集中更新。
- 成员文件数：1；声明类型数：1；字段：5；属性：0；合计：5。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（5）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1166 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1968 | 2 | hostile | bool | `public bool hostile;` | `public bool hostile;` |
| 1167 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1970 | 2 | hermesStepSound | Terraria.DataStructures.SoundPlaySet | `public SoundPlaySet hermesStepSound = new SoundPlaySet();` | `public SoundPlaySet hermesStepSound = new SoundPlaySet();` |
| 1168 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1972 | 2 | instantMovementAccumulatedThisFrame | Vector2 | `public Vector2 instantMovementAccumulatedThisFrame;` | `public Vector2 instantMovementAccumulatedThisFrame;` |
| 1177 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1990 | 2 | lastCreatureHit | int | `public int lastCreatureHit = -1;` | `public int lastCreatureHit = -1;` |
| 1186 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2012 | 2 | ActuationRodLock | bool | `public bool ActuationRodLock;` | `public bool ActuationRodLock;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.69 细分子系统：`PlayerInformationNavigationAndTimeState`

- 细分职责：指南针、手表、深度计、天气和计时信息。
- 边界角色：`authoritative state/behavior`；最小 seam：Information Accessory System/CommitPort；信息配饰按观察周期更新。
- 成员文件数：1；声明类型数：1；字段：7；属性：0；合计：7。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（7）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1169 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1974 | 2 | accCompass | int | `public int accCompass;` | `public int accCompass;` |
| 1170 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1976 | 2 | accWatch | int | `public int accWatch;` | `public int accWatch;` |
| 1171 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1978 | 2 | accWatchTime | double | `public double accWatchTime;` | `public double accWatchTime;` |
| 1172 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1980 | 2 | accDepthMeter | int | `public int accDepthMeter;` | `public int accDepthMeter;` |
| 1174 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1984 | 2 | accWeatherRadio | bool | `public bool accWeatherRadio;` | `public bool accWeatherRadio;` |
| 1176 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1988 | 2 | accCalendar | bool | `public bool accCalendar;` | `public bool accCalendar;` |
| 1180 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1998 | 2 | accStopwatch | bool | `public bool accStopwatch;` | `public bool accStopwatch;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.70 细分子系统：`PlayerInformationDetectionAndWiringState`

- 细分职责：探测、鱼类、第三只眼、矿石、图鉴和机械线路信息。
- 边界角色：`authoritative state/behavior`；最小 seam：Information Detection System/CommitPort；探测结果按扫描事件更新。
- 成员文件数：1；声明类型数：1；字段：8；属性：0；合计：8。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（8）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1173 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1982 | 2 | accFishFinder | bool | `public bool accFishFinder;` | `public bool accFishFinder;` |
| 1175 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1986 | 2 | accJarOfSouls | bool | `public bool accJarOfSouls;` | `public bool accJarOfSouls;` |
| 1178 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1992 | 2 | accThirdEye | bool | `public bool accThirdEye;` | `public bool accThirdEye;` |
| 1179 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1994 | 2 | accThirdEyeCounter | byte | `public byte accThirdEyeCounter;` | `public byte accThirdEyeCounter;` |
| 1181 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2000 | 2 | accOreFinder | bool | `public bool accOreFinder;` | `public bool accOreFinder;` |
| 1182 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2002 | 2 | accCritterGuide | bool | `public bool accCritterGuide;` | `public bool accCritterGuide;` |
| 1183 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2006 | 2 | accDreamCatcher | bool | `public bool accDreamCatcher;` | `public bool accDreamCatcher;` |
| 1187 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2016 | 2 | InfoAccMechShowWires | bool | `public bool InfoAccMechShowWires;` | `public bool InfoAccMechShowWires;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.99 细分子系统：`PlayerBuilderInteractionDefinitions`

- 细分职责：建筑工具切换项的静态定义。
- 边界角色：`definition/query`；最小 seam：只读 Definition/Catalog view；规则 System 消费，外部配置通过 Adapter 转换。
- 成员文件数：1；声明类型数：1；字段：13；属性：0；合计：13。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（13）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 391 | field | Terraria.Player.BuilderAccToggleIDs | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 47 | 3 | RulerLine | int | `public const int RulerLine = 0;` | `public const int RulerLine = 0;` |
| 392 | field | Terraria.Player.BuilderAccToggleIDs | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 49 | 3 | RulerGrid | int | `public const int RulerGrid = 1;` | `public const int RulerGrid = 1;` |
| 393 | field | Terraria.Player.BuilderAccToggleIDs | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 51 | 3 | AutoActuate | int | `public const int AutoActuate = 2;` | `public const int AutoActuate = 2;` |
| 394 | field | Terraria.Player.BuilderAccToggleIDs | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 53 | 3 | AutoPaint | int | `public const int AutoPaint = 3;` | `public const int AutoPaint = 3;` |
| 395 | field | Terraria.Player.BuilderAccToggleIDs | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 55 | 3 | WireVisibility_Red | int | `public const int WireVisibility_Red = 4;` | `public const int WireVisibility_Red = 4;` |
| 396 | field | Terraria.Player.BuilderAccToggleIDs | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 57 | 3 | WireVisibility_Green | int | `public const int WireVisibility_Green = 5;` | `public const int WireVisibility_Green = 5;` |
| 397 | field | Terraria.Player.BuilderAccToggleIDs | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 59 | 3 | WireVisibility_Blue | int | `public const int WireVisibility_Blue = 6;` | `public const int WireVisibility_Blue = 6;` |
| 398 | field | Terraria.Player.BuilderAccToggleIDs | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 61 | 3 | WireVisibility_Yellow | int | `public const int WireVisibility_Yellow = 7;` | `public const int WireVisibility_Yellow = 7;` |
| 399 | field | Terraria.Player.BuilderAccToggleIDs | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 63 | 3 | HideAllWires | int | `public const int HideAllWires = 8;` | `public const int HideAllWires = 8;` |
| 400 | field | Terraria.Player.BuilderAccToggleIDs | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 65 | 3 | WireVisibility_Actuators | int | `public const int WireVisibility_Actuators = 9;` | `public const int WireVisibility_Actuators = 9;` |
| 401 | field | Terraria.Player.BuilderAccToggleIDs | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 67 | 3 | BlockSwap | int | `public const int BlockSwap = 10;` | `public const int BlockSwap = 10;` |
| 402 | field | Terraria.Player.BuilderAccToggleIDs | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 69 | 3 | TorchBiome | int | `public const int TorchBiome = 11;` | `public const int TorchBiome = 11;` |
| 403 | field | Terraria.Player.BuilderAccToggleIDs | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 71 | 3 | Count | int | `public static readonly int Count = 12;` | `public static readonly int Count = 12;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.100 细分子系统：`PlayerSelectionState`

- 细分职责：热键/径向和选中物品的选择状态。
- 边界角色：`registry/projection`；最小 seam：Registry/Projection seam；提交后的事实单向输出，不把网络/UI/索引反写成权威状态。
- 成员文件数：1；声明类型数：2；字段：9；属性：6；合计：15。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（9）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 431 | field | Terraria.Player.SelectedItemState | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 321 | 3 | player | Terraria.Player | `private readonly Player player;` | `private readonly Player player;` |
| 432 | field | Terraria.Player.SelectedItemState | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 323 | 3 | selected | int | `private int selected;` | `private int selected;` |
| 433 | field | Terraria.Player.SelectedItemState | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 325 | 3 | hotbar | int | `private int hotbar;` | `private int hotbar;` |
| 434 | field | Terraria.Player.SelectedItemState | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 327 | 3 | buffered | int | `private int buffered;` | `private int buffered;` |
| 435 | field | Terraria.Player.SelectedItemState | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 329 | 3 | overridden | int | `private int overridden;` | `private int overridden;` |
| 436 | field | Terraria.Player.SelectionRadial | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 422 | 3 | _SelectedBinding | int | `private int _SelectedBinding = -1;` | `private int _SelectedBinding = -1;` |
| 437 | field | Terraria.Player.SelectionRadial | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 424 | 3 | RadialCount | int | `public int RadialCount;` | `public int RadialCount;` |
| 438 | field | Terraria.Player.SelectionRadial | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 426 | 3 | Bindings | int[] | `public int[] Bindings;` | `public int[] Bindings;` |
| 439 | field | Terraria.Player.SelectionRadial | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 428 | 3 | Mode | Terraria.Player.SelectionRadial.SelectionMode | `public SelectionMode Mode;` | `public SelectionMode Mode;` |

##### 属性（6）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1421 | property | Terraria.Player.SelectedItemState | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 331 | 3 | CanChangeSelectedItemImmediately | bool | `public bool CanChangeSelectedItemImmediately { get { if (!player.UsingOrReusingItem) { return player.ItemTimeIsZero; } return false; } }` | `public bool CanChangeSelectedItemImmediately { get { if (!player.UsingOrReusingItem) { return player.ItemTimeIsZero; } return false; } }` |
| 1422 | property | Terraria.Player.SelectedItemState | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 343 | 3 | Selected | int | `public int Selected => selected;` | `public int Selected => selected;` |
| 1423 | property | Terraria.Player.SelectedItemState | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 345 | 3 | Hotbar | int | `public int Hotbar => hotbar;` | `public int Hotbar => hotbar;` |
| 1424 | property | Terraria.Player.SelectedItemState | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 347 | 3 | HasActiveOverride | bool | `public bool HasActiveOverride => overridden >= 0;` | `public bool HasActiveOverride => overridden >= 0;` |
| 1425 | property | Terraria.Player.SelectedItemState | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 349 | 3 | HasBufferedChange | bool | `public bool HasBufferedChange => buffered >= 0;` | `public bool HasBufferedChange => buffered >= 0;` |
| 1426 | property | Terraria.Player.SelectedItemState | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 351 | 3 | LastNonOverridenSelection | int | `public int LastNonOverridenSelection { get { if (!HasBufferedChange) { if (HasActiveOverride) { return -1; } return selected; } return buffered; } }` | `public int LastNonOverridenSelection { get { if (!HasBufferedChange) { if (HasActiveOverride) { return -1; } return selected; } return buffered; } }` |


#### 4.13.101 细分子系统：`PlayerInputSyncAndMatch`

- 细分职责：输入同步快照、联机匹配外观请求和频道取消期望。
- 边界角色：`registry/projection`；最小 seam：Registry/Projection seam；提交后的事实单向输出，不把网络/UI/索引反写成权威状态。
- 成员文件数：1；声明类型数：3；字段：13；属性：1；合计：14。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（13）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 404 | field | Terraria.Player.PlayerInputSyncCache | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 76 | 3 | controlLeft | bool | `public bool controlLeft;` | `public bool controlLeft;` |
| 405 | field | Terraria.Player.PlayerInputSyncCache | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 78 | 3 | controlRight | bool | `public bool controlRight;` | `public bool controlRight;` |
| 406 | field | Terraria.Player.PlayerInputSyncCache | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 80 | 3 | controlUp | bool | `public bool controlUp;` | `public bool controlUp;` |
| 407 | field | Terraria.Player.PlayerInputSyncCache | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 82 | 3 | controlDown | bool | `public bool controlDown;` | `public bool controlDown;` |
| 408 | field | Terraria.Player.PlayerInputSyncCache | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 84 | 3 | controlJump | bool | `public bool controlJump;` | `public bool controlJump;` |
| 409 | field | Terraria.Player.ChannelCancelKey | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 110 | 3 | ProjectileTypeExpected | int | `public int ProjectileTypeExpected;` | `public int ProjectileTypeExpected;` |
| 410 | field | Terraria.Player.ChannelCancelKey | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 112 | 3 | ProjectileIndexExpected | int | `public int ProjectileIndexExpected;` | `public int ProjectileIndexExpected;` |
| 421 | field | Terraria.Player.SetMatchRequest | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 244 | 3 | Player | Terraria.Player | `public Player Player;` | `public Player Player;` |
| 422 | field | Terraria.Player.SetMatchRequest | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 246 | 3 | Head | int | `public int Head;` | `public int Head;` |
| 423 | field | Terraria.Player.SetMatchRequest | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 248 | 3 | Body | int | `public int Body;` | `public int Body;` |
| 424 | field | Terraria.Player.SetMatchRequest | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 250 | 3 | Legs | int | `public int Legs;` | `public int Legs;` |
| 425 | field | Terraria.Player.SetMatchRequest | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 252 | 3 | ArmorSlotRequested | int | `public int ArmorSlotRequested;` | `public int ArmorSlotRequested;` |
| 426 | field | Terraria.Player.SetMatchRequest | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 254 | 3 | Male | bool | `public bool Male;` | `public bool Male;` |

##### 属性（1）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1419 | property | Terraria.Player.PlayerInputSyncCache | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 86 | 3 | PressingAnyInput | bool | `public bool PressingAnyInput { get { if (!controlLeft && !controlRight && !controlUp && !controlDown) { return controlJump; } return true; } }` | `public bool PressingAnyInput { get { if (!controlLeft && !controlRight && !controlUp && !controlDown) { return controlJump; } return true; } }` |


#### 4.13.58 细分子系统：`PlayerBuilderOverlayState`

- 细分职责：标尺网格和标尺线的建造者界面投影状态。
- 边界角色：`registry/projection`；最小 seam：Registry/Projection seam；建造界面只读消费。
- 成员文件数：1；声明类型数：1；字段：2；属性：0；合计：2。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（2）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1075 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1781 | 2 | rulerGrid | bool | `public bool rulerGrid;` | `public bool rulerGrid;` |
| 1076 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1783 | 2 | rulerLine | bool | `public bool rulerLine;` | `public bool rulerLine;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.109 细分子系统：`PlayerItemSpaceAndSettings`

- 细分职责：物品接纳/虚空袋资格和玩家设置。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System 读取输入并通过显式 Command/CommitPort 写入；跨组只用事件、Query 或命令交接。
- 成员文件数：1；声明类型数：2；字段：3；属性：1；合计：4。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（3）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 427 | field | Terraria.Player.ItemSpaceStatus | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 259 | 3 | CanTakeItem | bool | `public readonly bool CanTakeItem;` | `public readonly bool CanTakeItem;` |
| 428 | field | Terraria.Player.ItemSpaceStatus | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 261 | 3 | ItemIsGoingToVoidVault | bool | `public readonly bool ItemIsGoingToVoidVault;` | `public readonly bool ItemIsGoingToVoidVault;` |
| 430 | field | Terraria.Player.Settings | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 316 | 3 | DashControl | Terraria.Player.Settings.DashPreference | `public static DashPreference DashControl = DashPreference.AllowDoubleTap;` | `public static DashPreference DashControl = DashPreference.AllowDoubleTap;` |

##### 属性（1）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1420 | property | Terraria.Player.ItemSpaceStatus | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 263 | 3 | CanTakeItemToPersonalInventory | bool | `public bool CanTakeItemToPersonalInventory { get { if (CanTakeItem) { return !ItemIsGoingToVoidVault; } return false; } }` | `public bool CanTakeItemToPersonalInventory { get { if (CanTakeItem) { return !ItemIsGoingToVoidVault; } return false; } }` |


## 8. 本分区自检

- 叶子子系统：11 个；指定组均已写入且没有重复分区。
- 字段/属性/成员：110 / 8 / 118。
- 来源序号范围：391..1426；本分区内唯一序号：True。
- 成员声明表逐组从源报告提取，未改写来源序号、类型、路径、行列、成员、C# 类型、成员声明或原始声明。
- 运行时读写者、生命周期、持久化、网络和调度：未验证，保留为迁移前风险。
