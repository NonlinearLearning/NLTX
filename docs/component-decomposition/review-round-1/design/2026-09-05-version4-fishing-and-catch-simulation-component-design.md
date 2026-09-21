# FishingAndCatchSimulation Component Design

## 1. 设计元数据

| 字段 | 值 |
| --- | --- |
| `subsystemId` | `FishingAndCatchSimulation` |
| `taskNumber` | `15` |
| `sourceReport` | `D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-05-version4-fishing-and-catch-simulation-public-decomposition.md` |
| `outputDesignPath` | `D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\design\2026-09-05-version4-fishing-and-catch-simulation-component-design.md` |
| `designScope` | `component-only` |
| `designStatus` | `decision-required` |
| `evidenceStatus` | `partial` |
| `nltxStatus` | `partial` |
| `verificationStatus` | `not-run` |
| `selectionMethod` | 当前会话明确产生的研究报告；按 `-public-decomposition.md` 后缀自动推导唯一设计路径 |

```text
subsystemId: FishingAndCatchSimulation
taskNumber: 15
sourceReport: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-05-version4-fishing-and-catch-simulation-public-decomposition.md
outputDesignPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\design\2026-09-05-version4-fishing-and-catch-simulation-component-design.md
designScope: component-only
designStatus: decision-required
evidenceStatus: partial
nltxStatus: partial
verificationStatus: not-run
selectionMethod: current-session-explicit-research-report-and-filename-derivation
```

本文件只整理 Component 的组成、字段、状态分类、生命周期、组合关系和归属候选。所有新增 Component 均为 `status: proposed`，不表示代码已经创建。

## 2. 设计范围与排除范围

### 2.1 设计范围

本设计只处理 FishingAndCatchSimulation 自身需要保存的局部状态：

- 一次钓鱼尝试的身份、阶段和关联关系；
- 浮标等待、咬钩和收线所需的时间状态；
- 资格计算完成后需要保留到结果判定阶段的只读快照；
- 已经确定但尚未交给外部领域的 Item/NPC 捕获结果；
- 鱼饵选择、预留和一次性扣减所需的尝试侧状态；
- Item/NPC 结果提交的确认状态和幂等审计状态。

### 2.2 排除范围

以下对象不在本文件中设计为 Fishing Component：

- Projectile 的位置、速度、碰撞、通用存活时间、owner 槽位和网络脏标记；
- Player 的全部能力、装备、Buff、库存和 Item 实例；
- Liquid Tile、水体存储、液体流动和 Chum 实体；
- FishDropRule 的内容定义、Item 定义、NPC 定义和世界任务进度；
- 客户端表现、Sonar 文本、声音、粒子、网络包和存档格式；
- 任何行为执行结构、运行时调度结构、外部提交接口或验证实现。

这些排除项仍可能作为字段证据、关系字段或 owner 依赖出现，但不会在本文件中重新定义其组件。

## 3. 组件设计依据

### 3.1 研究报告与源码证据

本文件唯一的研究输入是：

`D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-05-version4-fishing-and-catch-simulation-public-decomposition.md`

针对 Component 字段又重新读取了以下实际证据：

| 来源 | 复核内容 | 对组件设计的限制 |
| --- | --- | --- |
| `D:\TRbackup\Version4\Terraria\Projectile.cs:90-138`、`:25363-25366`、`:34071-34074`、`:47779-47787` | `bobber`、`owner`、`ai`、`localAI`、`aiStyle`、`timeLeft`、网络标记、钓鱼入口、空方法和 Item 交付调用点 | 旧浮点槽位是兼容载体；Version4 空方法不能证明完整钓鱼状态语义 |
| `D:\TRbackup\Version4\Terraria\Player.cs:818-830`、`D:\TRbackup\Version4\Terraria\Item.cs:104-106` | 玩家能力字段和鱼竿/鱼饵 Item 字段 | Fishing 不重复拥有 Player 能力或 Item 内容属性 |
| `D:\TRbackup\Version4\Terraria.DataStructures\FishingAttempt.cs:3-52`、`PlayerFishingConditions.cs:3-16` | 资格、环境、稀有度、任务和滚动结果字段 | 旧混合数据需要按生命周期拆成多个 Component 或外部只读状态 |
| `D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\FishingContext.cs:6-26`、`FishDropRule.cs:3-13`、`FishDropRuleList.cs:6-26` | 规则上下文和规则声明字段 | ContentCatalog 继续拥有规则定义，Fishing 不复制规则目录 |
| `D:\TRbackup\无任何删减通过编译\Terraria\Projectile.cs:51236-51443`、`:19361-19520`、`:20374-20422` | 完整参考中的时序、资格构建和水池扫描 | 仅补足同路径空实现；对 Version4 整体状态保持 `partial` |
| `D:\TRbackup\无任何删减通过编译\Terraria\Player.cs:42678-42808`、`:52942-53105` | 玩家钓鱼条件、装备/库存扫描、鱼饵选择和消耗 | 鱼饵实际库存状态不由 Fishing Component 所有 |
| `D:\TRbackup\NLTX\src\Player\PlayerFishingCapabilityState.cs:3-27`、`src\Content\FishingDropRuleCatalog.cs:6-28` | 当前 NLTX 的玩家能力元数据和只读规则目录 | 已有组件/目录必须保留其原 owner，不得因为本设计重复建模 |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Fishing\FishingBobberPhase.cs:3-8`、`FishingBobberStateComponent.cs:5-26` | 当前 Dome 的局部阶段和浮标状态字段 | 当前模型为 `partial`；其混合字段需要进行组件归属评审 |

### 3.2 有限结构参考

Space Station 14 无直接对应 Terraria Fishing/Catch 的实现。本文件只参考以下已读取文件所表现出的组件粒度和实体关系表达方式：

- `C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\Interaction\InteractUsing.cs:11-44` 的 `InteractUsingEvent` 将用户、被使用对象、目标和点击位置分开表达；本设计因此把玩家、浮标和位置作为不同关系字段，不把它们揉成一个内容字段。
- `C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\Inventory\InventoryComponent.cs:8-41` 将库存布局状态集中在库存组件内；本设计因此不在 Fishing Component 中复制库存槽位或 Item 堆叠。
- `C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\Fluids\Components\PuddleComponent.cs:8-30` 将液体局部状态与实体关联；本设计因此只保存 Fishing 所需的资格快照，不拥有液体 Tile。

上述文件仅支持结构粒度参考，不支持任何 Terraria 领域语义、默认值或 owner 结论。

## 4. Version4 成员到 Component 归属表

| 成员 | 声明类型 | 字段含义 | 权威/派生/缓存/快照/兼容 | 生命周期 | proposed Component | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|---|
| `bobber` | `Terraria.Projectile` | 是否为浮标 Projectile | 兼容字段；通用载体事实 | Projectile 创建至销毁 | 不新增 Fishing Component 字段；由现有 Projectile 载体表达 | `confirmed` | `D:\TRbackup\Version4\Terraria\Projectile.cs:104` |
| `owner` | `Terraria.Projectile` | 浮标关联的玩家槽位 | 权威关系候选；网络槽位不是领域 ID | Projectile 生命周期 | `FishingAttemptStateComponent.Owner` 只保存转换后的关系，`crossSubsystemOwner: integration-review` | `confirmed` | `D:\TRbackup\Version4\Terraria\Projectile.cs:126` |
| `aiStyle` | `Terraria.Projectile` | 通用 Projectile AI 分派值 | 兼容字段 | Projectile 生命周期 | 不新增 Fishing Component 字段 | `confirmed` | `D:\TRbackup\Version4\Terraria\Projectile.cs:136`、`:25363-25366` |
| `timeLeft` | `Terraria.Projectile` | 通用 Projectile 存活时间 | Projectile 权威状态 | 每 Tick 更新至销毁 | 不复制到 Fishing；Fishing 只持有与尝试有关的阶段 | `confirmed` | `D:\TRbackup\Version4\Terraria\Projectile.cs:138`、完整参考 `:51240-51243` |
| `ai[0]` | `Terraria.Projectile` | 收线/浮标兼容状态哨兵 | 兼容字段，部分行为状态 | 浮标存活期 | `BobberTimingComponent` 的显式时间/状态字段 | `partial` | Version4 `D:\TRbackup\Version4\Terraria\Projectile.cs:128`；完整参考 `D:\TRbackup\无任何删减通过编译\Terraria\Projectile.cs:51274-51314` |
| `ai[1]` | `Terraria.Projectile` | 待交付结果或咬钩/收线哨兵 | 兼容字段，混合结果状态 | 判定至收线清理 | `FishingCatchDecisionComponent` 与 `FishingResultCommitStateComponent` 的显式字段 | `partial` | Version4 `D:\TRbackup\Version4\Terraria\Projectile.cs:47782-47786`；完整参考 `D:\TRbackup\无任何删减通过编译\Terraria\Projectile.cs:19539-19557` |
| `localAI[1]` | `Terraria.Projectile` | 等待累计值及旧结果载体 | 兼容字段；既有实现混合时序和结果 | 浮标存活期 | `BobberTimingComponent.ElapsedTicks` 或 `FishingCatchDecisionComponent` 的明确字段，不能继续一字段多义 | `partial` | 完整参考 `D:\TRbackup\无任何删减通过编译\Terraria\Projectile.cs:51406-51423`、`:19541-19542` |
| `localAI[2]` | `Terraria.Projectile` | 实际使用的鱼饵类型兼容值 | 兼容字段/快照值 | 判定至鱼饵提交完成 | `FishingBaitReservationComponent.BaitItemTypeId` | `partial` | 完整参考 `D:\TRbackup\无任何删减通过编译\Terraria\Projectile.cs:19540-19556`、`Player.cs:53027-53084` |
| `netUpdate`、`netUpdate2` | `Terraria.Projectile` | 旧网络状态脏标记 | 兼容字段；不是 Fishing 权威事实 | 状态变化到复制 | 不复制到 Fishing Component | `confirmed` / `partial` | `D:\TRbackup\Version4\Terraria\Projectile.cs:172-174`；完整参考 `:51440`、`:19543` |
| `fishingSkill` | `Terraria.Player` | 玩家基础钓鱼能力 | 玩家权威状态 | 玩家生命周期 | 复用现有 `PlayerFishingCapabilityState`，不新建 Fishing 字段 | `confirmed` | `D:\TRbackup\Version4\Terraria\Player.cs:818`；当前 `D:\TRbackup\NLTX\src\Player\PlayerFishingCapabilityState.cs:5` |
| `sonarPotion`、`accFishingLine`、`accFishingBobber`、`accTackleBox`、`accLavaFishing` | `Terraria.Player` | 玩家钓鱼辅助能力 | 玩家权威状态/派生能力 | 装备、Buff 和玩家生命周期 | 复用 Player 能力组件，不复制到 Fishing | `confirmed` | `D:\TRbackup\Version4\Terraria\Player.cs:820-830` |
| `fishingPole` | `Terraria.Item` | 鱼竿能力 | Item 内容/实例属性 | Item 生命周期 | 由 Item/Content owner 提供，不新建 Fishing Component | `confirmed` | `D:\TRbackup\Version4\Terraria\Item.cs:104` |
| `bait` | `Terraria.Item` | 鱼饵能力 | Item 内容/实例属性 | Item 生命周期 | 由 Item/Content owner 提供；Fishing 只保存尝试侧引用 | `confirmed` | `D:\TRbackup\Version4\Terraria\Item.cs:106` |
| `playerFishingConditions` | `FishingAttempt` | 鱼竿、鱼饵和最终等级的组合条件 | 单次尝试快照 | 资格构建至决策结束 | `FishingEligibilitySnapshotComponent` 的字段集合 | `confirmed`（完整参考补证） | `D:\TRbackup\Version4\Terraria.DataStructures\FishingAttempt.cs:5`；完整参考 `Player.cs:42678-42808` |
| `X`、`Y`、`bobberType` | `FishingAttempt` | 浮标位置和类型 | 单次尝试快照；坐标关系跨域 | 资格构建至结果提交 | `FishingEligibilitySnapshotComponent` 与结果来源关系 | `confirmed` | `D:\TRbackup\Version4\Terraria.DataStructures\FishingAttempt.cs:7-11` |
| `common`、`uncommon`、`rare`、`veryrare`、`legendary`、`crate`、`junk` | `FishingAttempt` | 水质/等级推导出的规则选择输入 | 派生值/快照 | 单次尝试 | `FishingEligibilitySnapshotComponent` 的候选标志或不透明快照值 | `confirmed`（完整参考补证） | `D:\TRbackup\Version4\Terraria.DataStructures\FishingAttempt.cs:13-25`；完整参考 `Projectile.cs:19516-19519` |
| `inLava`、`inHoney`、`waterTilesCount`、`waterNeededToFish`、`waterQuality`、`chumsInWater` | `FishingAttempt` | 水池环境和资格质量 | 外部状态的派生快照 | 水池读取至决策结束 | `FishingEligibilitySnapshotComponent` | `confirmed`（完整参考补证） | `D:\TRbackup\Version4\Terraria.DataStructures\FishingAttempt.cs:27-37`；完整参考 `Projectile.cs:20374-20422` |
| `fishingLevel`、`CanFishInLava`、`atmo`、`heightLevel` | `FishingAttempt` | 有效等级、液体资格、深度和大气派生值 | 派生值/快照 | 单次尝试 | `FishingEligibilitySnapshotComponent` | `confirmed`（完整参考补证） | `D:\TRbackup\Version4\Terraria.DataStructures\FishingAttempt.cs:39-47`；完整参考 `Projectile.cs:19415-19515` |
| `questFish` | `FishingAttempt` | 当前任务鱼候选 | 跨域快照 | 单次尝试 | `FishingEligibilitySnapshotComponent`，`crossSubsystemOwner: integration-review` | `partial` | `D:\TRbackup\Version4\Terraria.DataStructures\FishingAttempt.cs:45`；Version4 任务轮换 `Main.cs:1607-1675` |
| `rolledItemDrop`、`rolledEnemySpawn` | `FishingAttempt` | 已滚动的 Item/NPC 结果 | 单次尝试结果 | 判定至外部提交 | `FishingCatchDecisionComponent` | `partial` | `D:\TRbackup\Version4\Terraria.DataStructures\FishingAttempt.cs:49-51`；完整参考 `Projectile.cs:19523-19557` |
| `Random`、`Fisher`、`Player`、Biome 派生标志 | `FishingContext` | 规则匹配上下文 | 单次尝试快照与外部引用混合 | 规则判定期 | `FishingEligibilitySnapshotComponent` 只保存必要值；不复制对象引用 | `confirmed` / `partial` | `D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\FishingContext.cs:6-26` |
| `PossibleItems`、`ChanceNumerator`、`ChanceDenominator`、`Conditions`、`Rarity` | `FishDropRule` | 掉落规则定义 | ContentCatalog 权威定义 | 内容初始化至读取 | 不新增 Fishing Component | `confirmed` | `D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\FishDropRule.cs:3-13` |
| `Owner`、`BobberProjectileType`、`Phase`、`PendingItemType` | `FishingBobberStateComponent` | 当前 Dome 局部浮标状态 | 现有局部状态，字段生命周期混合 | 浮标实体生命周期 | 归入 `FishingAttemptStateComponent`、`BobberTimingComponent` 或 `FishingCatchDecisionComponent` | `partial` | `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Fishing\FishingBobberStateComponent.cs:5-18` |
| `WaterTilesCount`、`WaterNeededToFish`、`FishingLevel`、`IsInLava`、`IsInHoney` | `FishingBobberStateComponent` | 当前 Dome 保存的环境/等级快照 | 快照；不应与浮标时序共存 | 资格计算期 | `FishingEligibilitySnapshotComponent` | `partial` | `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Fishing\FishingBobberStateComponent.cs:18-23` |
| `FishingBobberPhase` | 当前 Dome 枚举 | Waiting/Biting/Resolving/Retracting 阶段 | 尝试阶段事实候选 | 浮标/尝试生命周期 | `FishingAttemptStateComponent.Phase`，但归属范围待裁决 | `partial` | `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Fishing\FishingBobberPhase.cs:3-8` |

## 5. Component 定义

本节的 Component 名称、字段和归属均为 `status: proposed`。字段默认值若没有 Version4 或当前 NLTX 的直接证据，使用“未绑定/未定义”而不是伪造 sentinel；`crossSubsystemOwner: integration-review` 表示不能由本会话宣布最终 owner。

### 5.1 FishingAttemptStateComponent

#### 职责

保存一次钓鱼尝试的身份、阶段、玩家关系、浮标关系和本地版本。它只保存尝试生命周期事实，不保存玩家库存、液体 Tile、Item 实例或 NPC 实例。

```text
componentId: FISHING-ATTEMPT-STATE
name: FishingAttemptStateComponent
status: proposed
designStatus: decision-required
componentOwner: FishingAndCatchSimulation
crossSubsystemOwner: integration-review for shared IDs and carrier relation
entityScope: one fishing-attempt entity, or the bobber entity if BD-COMP-01 selects carrier attachment
lifecycle: created when a valid fishing attempt begins; retained through resolution/retraction; cleared after terminal commit or rejection
```

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| `AttemptId` | `FishingAttemptId` | 创建时分配；具体 invalid 值未定义 | 权威身份 | 同一尝试全生命周期不变；重试不得生成新 ID | `missing` for concrete type | 研究报告提出的共享 ID；Version4 无对应显式字段 |
| `Owner` | `PlayerEntityId` | 未绑定 | 权威关系 | 必须指向发起该尝试的玩家；不能把网络 slot 直接当领域 ID | `partial` | Version4 `Projectile.owner`：`D:\TRbackup\Version4\Terraria\Projectile.cs:126` |
| `Bobber` | `ProjectileEntityId?` | 未绑定 | 关系/兼容引用 | 若选择 Projectile 承载，关联不能在尝试结束前静默改变 | `partial` | Version4 `bobber`/`owner`：`D:\TRbackup\Version4\Terraria\Projectile.cs:104-126` |
| `Phase` | `FishingAttemptPhase` | `Waiting` 候选 | 权威阶段 | 阶段与结果/预留状态一致；具体转移集合尚未锁定 | `partial` | 当前 Dome `FishingBobberPhase`：`D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Fishing\FishingBobberPhase.cs:3-8` |
| `Revision` | `uint` | `0` 候选 | 权威版本 | 每次权威改变递增；外部确认不得回退旧版本 | `missing` | Version4 没有显式尝试版本字段 |
| `TerminalReason` | `FishingTerminalReason?` | 未定义 | 终态审计 | 只有拒绝、取消、提交完成或过期时填写 | `missing` | Version4 有多条终止路径但无统一字段 |

#### 字段不变量

- `AttemptId`、`Owner` 和 `Phase` 必须共同标识同一个尝试；缺少任一项时不能把状态当作可提交尝试。
- `Bobber` 是关系，不是 Projectile 的复制对象；Projectile 的物理字段不得进入本 Component。
- `Phase` 与 `FishingCatchDecisionComponent`、`FishingBaitReservationComponent`、`FishingResultCommitStateComponent` 的存在关系必须由整合裁决，不能靠空值组合产生隐式状态。
- 该 Component 不保存共享 ID 的最终表示；所有跨子系统 ID 均带 `crossSubsystemOwner: integration-review`。

#### 生命周期

- 创建：钓竿使用已经形成合法浮标关系并开始一次尝试时。
- 初始化：写入 `AttemptId`、`Owner`、可选 `Bobber` 和初始 `Phase`。
- 更新：阶段或版本变化时更新；不因每个视觉帧复制 Projectile 状态而更新。
- 清理：外部结果提交确认、明确拒绝、取消或过期后清理；若 pending 状态需要断线恢复，持久化范围仍是未决事项。

#### Entity/World 范围

单个尝试范围。不是 Player、World、Tile 或 ContentCatalog 的全局组件。是否使用独立尝试实体与当前浮标实体绑定由 `BD-COMP-01` 决定。

#### ID 与关系字段

`AttemptId`、`Owner` 和 `Bobber` 都是跨边界候选。`PlayerEntityId` 与 `ProjectileEntityId` 的最终表示、持久化 ID 和网络 ID 必须由 `integration-review` 统一。

#### 当前 NLTX 映射

当前 `FishingBobberStateComponent.Owner`、`BobberProjectileType` 和 `Phase`（`D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Fishing\FishingBobberStateComponent.cs:5-18`）是局部、部分覆盖，不能直接视为本 Component 已存在。当前缺少 `AttemptId`、明确终态、版本和 NPC/Item 提交状态。

#### 证据

Version4 只有浮标 owner 和通用槽位证据，完整尝试生命周期来自同路径完整参考补证；因此本 Component 的字段组和 owner 仍是 `decision-required`。

### 5.2 BobberTimingComponent

#### 职责

保存浮标钓鱼专属的等待、咬钩和收线时间状态。它不拥有 Projectile 的位置、速度、碰撞、通用 `timeLeft` 或表现数据。

```text
componentId: FISHING-BOBBER-TIMING
name: BobberTimingComponent
status: proposed
designStatus: candidate
componentOwner: FishingAndCatchSimulation for fishing-specific timing
crossSubsystemOwner: integration-review for Projectile relation and time representation
entityScope: one bobber Projectile entity, or one fishing-attempt entity if BD-COMP-01 selects independent state
lifecycle: created with a fishing bobber; updated during bobber waiting/biting/retracting; removed with the carrier or terminal attempt
```

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| `ElapsedTicks` | `int` | `0` 候选 | 权威行为状态 | 不得为负；只表示钓鱼等待累计，不代替 Projectile `timeLeft` | `partial` | 完整参考使用 `localAI[1]`：`D:\TRbackup\无任何删减通过编译\Terraria\Projectile.cs:51406-51423` |
| `BiteThreshold` | `int` | `660` 仅为完整参考候选 | 配置/兼容值 | 阈值来源必须与规则版本一致；Version4 空方法未确认 | `partial` | 完整参考 `D:\TRbackup\无任何删减通过编译\Terraria\Projectile.cs:51419-51423` |
| `RetractRequested` | `bool` | `false` | 权威行为状态 | 请求收线后不能被重复请求改变结果语义 | `partial` | 完整参考 `Projectile.cs:51274-51284`、`Player.cs:52942-52973` |
| `BiteState` | `FishingBiteState` | `Waiting` 候选 | 权威阶段细节 | 与尝试 Phase 不得表达互相矛盾的咬钩状态 | `partial` | 当前 Dome 阶段：`D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Fishing\FishingBobberPhase.cs:3-8` |
| `LegacyAiState` | `float[?]` 或兼容值 | 未定义 | 兼容字段 | 只在兼容边界使用；不能成为新 Component 的业务 API | `partial` | Version4 `ai[0]`/`ai[1]`：`D:\TRbackup\Version4\Terraria\Projectile.cs:128`；完整参考 `:51274-51440` |

#### 字段不变量

- `ElapsedTicks` 是钓鱼时序，不拥有通用 Projectile 生命周期；两者失效条件不同，必须分开。
- `BiteThreshold` 的具体默认值只能在完整行为映射被接受后锁定；`660` 当前仅是 matched full-reference evidence，不是 Version4 已确认常量。
- `RetractRequested` 不保存 Item/NPC 结果；它只表达时序侧的收线事实。
- 兼容字段只允许单向映射，不能与新字段双写。

#### 生命周期

- 创建：浮标 Projectile 创建且被识别为钓鱼浮标时。
- 初始化：从浮标类型和兼容状态初始化；未有完整 Version4 默认值的字段保持未定义候选。
- 更新：等待/咬钩/收线时更新 `ElapsedTicks` 和收线标志。
- 清理：浮标移除或尝试进入终态时清理；不把客户端动画结束当作权威清理依据。

#### Entity/World 范围

优先按单个浮标实体范围表达；若 `BD-COMP-01` 选择独立尝试实体，该 Component 可以随尝试实体保存，并通过 `Bobber` 关系关联载体。

#### ID 与关系字段

本 Component 不重复保存 Player ID 或 Item ID；只依赖 `FishingAttemptStateComponent` 或载体关系。关联 Projectile 的 ID 是 `crossSubsystemOwner: integration-review`。

#### 当前 NLTX 映射

当前 `FishingBobberStateComponent.SwingCount`、`Phase` 提供局部时序线索，但没有等待累计值、收线请求或完整咬钩不变量；状态为 `partial`。

#### 证据

Version4 的 `aiStyle == 61` 入口存在，但 `AI_061_FishingBobber` 为空（`D:\TRbackup\Version4\Terraria\Projectile.cs:25363-25366`、`:34073`）。完整参考只用于补足时序形状。

### 5.3 FishingEligibilitySnapshotComponent

#### 职责

保存一次资格评估完成后、结果选择完成前需要复用的只读快照。它不拥有外部 Player、Liquid、World 或 Content 状态，也不保存对象引用。

```text
componentId: FISHING-ELIGIBILITY-SNAPSHOT
name: FishingEligibilitySnapshotComponent
status: proposed
designStatus: candidate
componentOwner: FishingAndCatchSimulation for attempt-scoped snapshot
crossSubsystemOwner: integration-review for player/world/liquid/content values
entityScope: one fishing-attempt entity; not a world or player component
lifecycle: created after qualification data is assembled; read during result selection; removed after decision finalization or rejection
```

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| `PondCoordinate` | `TileCoordinate` | 未绑定 | 快照/关系 | 必须对应本次浮标位置，坐标单位和边界检查由整合裁决 | `partial` | `FishingAttempt.X/Y`：`D:\TRbackup\Version4\Terraria.DataStructures\FishingAttempt.cs:7-9` |
| `BobberTypeId` | `int` 或内容 ID | `0` 候选 | 快照 | 必须来自关联浮标，不允许客户端任意替换 | `confirmed` / `partial` | `FishingAttempt.bobberType`：`D:\TRbackup\Version4\Terraria.DataStructures\FishingAttempt.cs:11` |
| `LiquidKind` | `FishingLiquidKind` | `Unknown` 候选 | 派生快照 | `IsInLava`、`IsInHoney` 等派生值必须与该值一致 | `partial` | `FishingAttempt.inLava/inHoney`：`D:\TRbackup\Version4\Terraria.DataStructures\FishingAttempt.cs:27-29` |
| `WaterTilesCount` | `int` | `0` | 派生快照 | 不得为负；扫描规则必须保持固体 Tile 排除语义 | `confirmed`（完整参考） | `D:\TRbackup\无任何删减通过编译\Terraria\Projectile.cs:20374-20422` |
| `WaterNeededToFish` | `int` | `0` | 派生快照 | 不得为负；与深度/大气修正版本一致 | `confirmed`（完整参考） | `FishingAttempt`：`D:\TRbackup\Version4\Terraria.DataStructures\FishingAttempt.cs:33`；完整参考 `Projectile.cs:19434-19447` |
| `WaterQuality` | `float` | `0` | 派生快照 | 必须是有限值；不能反向写入液体状态 | `confirmed`（完整参考） | 完整参考 `D:\TRbackup\无任何删减通过编译\Terraria\Projectile.cs:19447-19455` |
| `ChumsInWater` | `int` | `0` | 派生快照 | 不得为负；由环境扫描结果提供 | `confirmed`（完整参考） | `D:\TRbackup\无任何删减通过编译\Terraria\Projectile.cs:20382-20400` |
| `PolePower`、`PoleItemType` | `int` | `0` | 玩家能力快照 | 只能从合法玩家 Item/能力读取 | `confirmed`（完整参考） | `D:\TRbackup\无任何删减通过编译\Terraria\Player.cs:42678-42684` |
| `BaitPower`、`BaitItemType` | `int` | `0` | 玩家能力快照 | 只标识资格时使用的鱼饵；实际扣减需通过 Item owner | `confirmed`（完整参考） | `PlayerFishingConditions`：`D:\TRbackup\Version4\Terraria.DataStructures\PlayerFishingConditions.cs:5-15`；完整参考 `Player.cs:42760-42787` |
| `FishingLevel`、`FinalFishingLevel` | `int` | `0` | 派生快照 | 只能由同一快照中的能力和环境版本解释 | `confirmed`（完整参考） | 完整参考 `D:\TRbackup\无任何删减通过编译\Terraria\Projectile.cs:19415-19431`、`Player.cs:42706-42708` |
| `CanFishInLava` | `bool` | `false` | 派生快照 | 必须由鱼竿、鱼饵和玩家能力共同计算，不能单独覆盖液体 owner | `confirmed`（完整参考） | `D:\TRbackup\无任何删减通过编译\Terraria\Projectile.cs:19420` |
| `HeightLevel` | `int` | `0` 候选 | 派生快照 | 只表示该次评估的深度层级 | `confirmed`（完整参考） | `FishingAttempt.heightLevel`：`D:\TRbackup\Version4\Terraria.DataStructures\FishingAttempt.cs:47`；完整参考 `Projectile.cs:19468-19515` |
| `BiomeFlags` | 不透明只读值 | 未定义 | 跨域快照 | 不保存 Biome 实体或对象引用；版本必须可审计 | `partial` | `FishingContext`：`D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\FishingContext.cs:14-26` |
| `QuestFishType` | `int?` | 未绑定 | 跨域快照 | 只能读取当前任务状态，不能在 Fishing Component 中轮换任务 | `partial` | `FishingAttempt.questFish`：`D:\TRbackup\Version4\Terraria.DataStructures\FishingAttempt.cs:45`；`Main.cs:1607-1675` |
| `RarityFlags` | 不透明只读值 | 未定义 | 派生快照 | 只能表示当前资格计算出的规则输入，不承载规则定义 | `partial` | `FishingAttempt.common` 至 `crate`：`D:\TRbackup\Version4\Terraria.DataStructures\FishingAttempt.cs:13-25` |
| `IsJunkCandidate` | `bool` | `false` | 派生快照 | 只能由当前水质/等级规则产生 | `confirmed`（完整参考） | 完整参考 `D:\TRbackup\无任何删减通过编译\Terraria\Projectile.cs:19516` |
| `RulesetRevision` | `uint?` | 未绑定 | 快照审计 | 决策只能引用当时读取的规则版本 | `missing` | 当前 NLTX 仅有 `IsResolutionPathConfirmed`，无运行时修订号 |

#### 字段不变量

- 所有字段属于同一 `AttemptId` 和同一内容/世界读取时点。
- 快照是只读副本；改变 Player、Liquid、World 或 Content 的事实不属于该 Component 的职责。
- 外部值无法确认时保留 `unresolved`/未定义，不使用看似合理的默认值替代证据。
- `WaterQuality` 等浮点值必须为有限值，且不能通过快照反向修改水池。

#### 生命周期

- 创建：浮标位置、水体和玩家能力已经读取并形成一次资格结果时。
- 初始化：写入本次坐标、液体、水量、玩家能力和世界/内容派生值。
- 更新：只有在本次尝试被明确重新评估时替换整份快照；不逐字段镜像外部状态。
- 清理：CatchDecision 形成并记录版本后，或资格拒绝/尝试取消后移除。

#### Entity/World 范围

单个尝试实体范围。水体、Biome、天气、时间和任务本身不是该 Component 的实体范围。

#### ID 与关系字段

`PondCoordinate`、Player 关系、内容版本和任务鱼关系均涉及跨子系统 owner，标记 `crossSubsystemOwner: integration-review`。

#### 当前 NLTX 映射

当前 Dome `FishingBobberStateComponent` 有水量、等级和液体标记（`D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Fishing\FishingBobberStateComponent.cs:18-23`），但没有完整玩家能力、Biome、天气、任务和内容版本快照，状态为 `partial`。

#### 证据

Version4 同路径 `FishingAttempt` 给出字段形状；完整参考证明这些值由水池扫描和玩家条件组合产生。Version4 浮标主体为空，因此快照的最终 owner 仍需整合确认。

### 5.4 FishingCatchDecisionComponent

#### 职责

保存一次尝试已经确定的不可变捕获结果，直到外部结果被处理。它保存结果引用和数量，不保存已经创建的 Item/NPC 实例，也不保存规则定义。

```text
componentId: FISHING-CATCH-DECISION
name: FishingCatchDecisionComponent
status: proposed
designStatus: candidate
componentOwner: FishingAndCatchSimulation for the attempt-scoped decision value
crossSubsystemOwner: integration-review for Item/NPC definition IDs and result references
entityScope: one fishing-attempt entity
lifecycle: created once after eligibility and rule selection; immutable after creation; removed after terminal result handling
```

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| `OutcomeKind` | `FishingOutcomeKind` | `None` | 权威决策 | `Item`、`Npc`、`None`、`Junk`、`QuestFish` 的具体集合待整合锁定 | `partial` | 完整参考分别保存 `rolledItemDrop`/`rolledEnemySpawn`：`D:\TRbackup\无任何删减通过编译\Terraria\Projectile.cs:19523-19557` |
| `ItemTypeId` | `int?` | 未绑定 | 结果引用 | 只有 `OutcomeKind == Item` 或 Item 类结果时有效 | `confirmed`（结果字段形状） | `FishingAttempt.rolledItemDrop`：`D:\TRbackup\Version4\Terraria.DataStructures\FishingAttempt.cs:49` |
| `NpcTypeId` | `int?` | 未绑定 | 结果引用 | 只有 NPC 类结果时有效；不能提前分配 NPC 实体 ID | `confirmed`（结果字段形状） | `FishingAttempt.rolledEnemySpawn`：`D:\TRbackup\Version4\Terraria.DataStructures\FishingAttempt.cs:51` |
| `StackQuantity` | `int` | `0` | 结果值 | Item 结果数量不得为负；特殊 Item 堆叠规则尚未被 Version4 主基线闭合 | `partial` | 完整参考 `D:\TRbackup\无任何删减通过编译\Terraria\Projectile.cs:51500-51549` |
| `IsQuestFish` | `bool` | `false` | 结果派生值 | 只能来自任务/规则快照，不能更新任务进度 | `partial` | `FishingAttempt.questFish`：`D:\TRbackup\Version4\Terraria.DataStructures\FishingAttempt.cs:45` |
| `DecisionRevision` | `uint` | `0` 候选 | 权威版本 | 同一尝试只能有一个生效决策版本 | `missing` | Version4 无显式决策版本字段 |
| `RngAuditValue` | 不透明只读值 | 未定义 | 审计快照 | 不得让该字段在决策后重新滚动结果 | `missing` | Version4 使用全局随机源，但无尝试级审计字段 |

#### 字段不变量

- `ItemTypeId` 与 `NpcTypeId` 不得同时表示两个成功结果；若旧语义允许优先级，必须在 `OutcomeKind` 中明确。
- 该 Component 只保存类型引用和结果值，不创建 Item/NPC，不扣鱼饵，不修改库存或世界。
- 决策一旦形成不得因客户端重复请求、Sonar 表现或网络重放而重新滚动。

#### 生命周期

- 创建：资格快照通过并完成一次结果选择时。
- 初始化：写入结果种类、类型 ID、数量和决策版本。
- 更新：原则上不可变；仅允许在未提交且经明确版本裁决的兼容扩展阶段替换整份值。
- 清理：外部结果终态确认、拒绝或尝试终止后移除。

#### Entity/World 范围

单个尝试范围，不属于 Item、NPC、Player 或 World。

#### ID 与关系字段

Item/NPC 类型 ID 是内容引用。未来已经生成的 `ItemInstanceId` 或 `NpcEntityId` 不应在决策创建时填入；它们的 owner 分别是相邻领域，且 `crossSubsystemOwner: integration-review`。

#### 当前 NLTX 映射

当前 `FishingBobberStateComponent.PendingItemType`（`D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Fishing\FishingBobberStateComponent.cs:17`）只覆盖单一 Item pending 字段，没有 NPC 结果、结果种类、数量、版本或幂等语义，状态为 `partial`。

#### 证据

Version4 的 `rolledItemDrop`、`rolledEnemySpawn` 字段和完整参考的写入点确认结果值形状，但 Version4 的结果算法未闭合，因此本 Component 不锁定更细的掉落规则。

### 5.5 FishingBaitReservationComponent

#### 职责

保存一次尝试选定的鱼饵引用及其预留/消耗状态，以便避免重复收线重复扣减。它不拥有库存槽位、Item 堆叠或最终消费写入。

```text
componentId: FISHING-BAIT-RESERVATION
name: FishingBaitReservationComponent
status: proposed
designStatus: decision-required
componentOwner: integration-review candidate; Fishing owns attempt-side reference only
crossSubsystemOwner: integration-review
entityScope: one fishing-attempt entity; not the inventory entity
lifecycle: created when a bait candidate is bound to the attempt; retained until consumption success/rejection; cleared with attempt terminal state
```

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| `BaitItemInstanceId` | `ItemInstanceId` | 未绑定 | 跨域关系 | 必须指向资格时选中的实际鱼饵实例；不能只靠 Item 类型定位并忽略并发变化 | `missing` | 完整参考按类型扫描库存：`D:\TRbackup\无任何删减通过编译\Terraria\Player.cs:53027-53084`；Version4 无实例 ID |
| `BaitItemTypeId` | `int` | `0` 候选 | 快照/兼容值 | 必须与选中 Item 实例类型一致 | `confirmed`（完整参考） | 完整参考 `D:\TRbackup\Terraria\Projectile.cs:19542-19556` |
| `ExpectedQuantity` | `int` | `1` 候选 | 事务约束 | 当前鱼饵一次扣减候选为 1；特殊规则必须由 Item owner 确认 | `partial` | 完整参考 `D:\TRbackup\无任何删减通过编译\Terraria\Player.cs:53093-53105` |
| `ReservationState` | `BaitReservationState` | `Unbound` | 权威事务状态 | 不得从 `Consumed` 回到 `Reserved`；重复请求返回已有状态 | `missing` | Version4 无统一预留状态；当前 Dome 直接改库存 |
| `ReservationKey` | `FishingAttemptId` | 未绑定 | 幂等关系 | 同一尝试只能有一个生效的鱼饵扣减键 | `missing` | 研究报告提出尝试幂等边界；Version4 无显式 ID |
| `ConsumptionRevision` | `uint` | `0` 候选 | 审计值 | 外部确认版本不得覆盖更新的拒绝状态 | `missing` | Version4 无显式事务版本字段 |

#### 字段不变量

- Fishing 不能直接通过该 Component 改变库存数量。
- `BaitItemInstanceId`、`BaitItemTypeId` 和 `ReservationKey` 必须共同验证，避免同类型鱼饵被错误替换。
- `ReservationState` 的最终 owner 和原子语义尚未确定；在此之前不能使用 `baseline` 设计状态。

#### 生命周期

- 创建：资格阶段已经确定鱼饵候选，且需要把候选绑定到本次尝试时。
- 初始化：记录 Item 实例关系、类型和预留键。
- 更新：只记录外部确认的预留/扣减/拒绝状态，不直接改库存。
- 清理：ItemContainerAndEconomy 返回终态后随尝试清理；是否保留失败审计记录待整合决定。

#### Entity/World 范围

单个尝试范围。实际库存和 Item 实例属于 ItemContainerAndEconomy；该 Component 不是库存组件。

#### ID 与关系字段

`ItemInstanceId` 与 `FishingAttemptId` 均为跨子系统共享候选，必须 `crossSubsystemOwner: integration-review`。

#### 当前 NLTX 映射

当前 Dome 的鱼饵辅助路径在 `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Player\Systems\PlayerFishingUseSystem.cs:78-100` 直接通过 `InventoryComponent.SetSlot` 扣减；它没有尝试 ID、Item 实例 ID 或重复扣减状态，因此只标为 `partial`，不是本 Component 已存在实现。

#### 证据

完整参考证明鱼饵消费的扫描和稀有度条件，但 Version4 没有闭合方法。该 Component 的事务字段和 owner 必须等待 `BD-COMP-03` 裁决。

### 5.6 FishingResultCommitStateComponent

#### 职责

保存捕获结果从“已决定”到“外部结果已确认”的尝试侧提交状态。它不拥有 Item 或 NPC 实体，不保存外部领域的完整实体状态。

```text
componentId: FISHING-RESULT-COMMIT-STATE
name: FishingResultCommitStateComponent
status: proposed
designStatus: decision-required
componentOwner: integration-review candidate; Fishing owns only attempt-local acknowledgement state
crossSubsystemOwner: integration-review
entityScope: one fishing-attempt entity
lifecycle: created with a CatchDecision that requires an external result; retained until result acknowledgement or terminal failure; cleared with the attempt
```

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| `ResultKind` | `FishingOutcomeKind` | `None` | 权威提交上下文 | 必须与 `FishingCatchDecisionComponent.OutcomeKind` 一致 | `partial` | 完整参考 Item/NPC 分支：`D:\TRbackup\无任何删减通过编译\Terraria\Projectile.cs:19530-19558` |
| `CommitState` | `FishingCommitState` | `NotSubmitted` 候选 | 权威事务状态 | `Committed` 后不可再次提交；拒绝和重试状态必须可区分 | `missing` | Version4 只有 `ai[1] = 0` 清理：`D:\TRbackup\Version4\Terraria\Projectile.cs:47782-47786` |
| `CommitAttemptCount` | `ushort` | `0` | 审计/派生值 | 只统计同一尝试键的外部提交尝试 | `missing` | Version4 无尝试级重试计数 |
| `ExternalResultId` | `ItemInstanceId?` 或 `NpcEntityId?` | 未绑定 | 外部关系 | 只有外部创建成功后才可填写；Item/NPC 二者不可同时有效 | `missing` | 完整参考分别调用 Item 投放和 NPC 创建；没有统一结果 ID |
| `Origin` | `TileCoordinate` | 未绑定 | 来源快照 | 必须保持钓获来源；坐标类型 owner 待整合 | `partial` | Version4 消息 130 坐标和 `EntitySource_FishedOut`：`D:\TRbackup\Version4\Terraria\MessageBuffer.cs:3080-3104` |
| `IdempotencyKey` | `IdempotencyKey` | 未绑定 | 权威事务身份 | 同一尝试/结果只能有一个生效键 | `missing` | Version4 无显式幂等键；为 proposed 字段 |

#### 字段不变量

- `CommitState` 只记录尝试侧确认，不直接代表 Item/NPC 实体已经拥有全部生命周期。
- `ExternalResultId` 只能在相邻 owner 返回确认后填充；Fishing 不提前分配实体 ID。
- `ResultKind` 必须和决策一致，不能通过重复收线改成另一种结果。
- Item 与 NPC 的最终 owner、错误重试和原子边界由 `BD-COMP-03` 决定。

#### 生命周期

- 创建：有 Item/NPC 结果需要交给外部领域时。
- 初始化：关联决策、来源坐标和幂等键；初始状态为未提交候选。
- 更新：记录提交中、成功、拒绝或终止确认；不复制外部实体字段。
- 清理：终态确认并完成尝试关闭后清理；保留审计快照的范围待整合决定。

#### Entity/World 范围

单个尝试范围。Item、NPC、Tile 和网络会话不是该 Component 的状态范围。

#### ID 与关系字段

`ExternalResultId`、`Origin`、`IdempotencyKey` 和 `FishingAttemptId` 均涉及跨子系统 owner，统一 `crossSubsystemOwner: integration-review`。

#### 当前 NLTX 映射

当前 NLTX 有 `FishOutNpcPacket` 的协议数据形状，但没有当前 Component 对应的尝试提交状态；现有协议不能证明结果提交已闭合。

#### 证据

Version4 的 Item 交付调用点和消息 130 的 NPC 创建邻域证明需要独立的结果提交审计状态，但没有证明其具体 Component 组成或 owner。

## 6. Entity 与 Component 组合

下表只描述状态组合，不描述行为执行结构或运行时先后。

| Entity/World 对象 | 必需 Component | 可选 Component | 互斥 Component | 组合理由 |
|---|---|---|---|---|
| FishingAttempt entity（独立尝试方案） | `FishingAttemptStateComponent` | `FishingEligibilitySnapshotComponent`、`FishingCatchDecisionComponent`、`FishingBaitReservationComponent`、`FishingResultCommitStateComponent` | 与“仅附着在 Projectile 上的尝试状态方案”互斥 | 独立生命周期能承载等待决策、结果确认和断线策略；是否采用由 `BD-COMP-01` 决定 |
| Bobber Projectile entity（载体方案） | 现有 Projectile bobber 标识 | `BobberTimingComponent`、`FishingAttemptStateComponent`、`FishingEligibilitySnapshotComponent`、`FishingCatchDecisionComponent`、`FishingBaitReservationComponent`、`FishingResultCommitStateComponent` | 不应再添加把所有字段合并的 Fishing aggregate Component | Version4 和当前 Dome 都显示浮标状态从 Projectile 创建；但尝试状态与物理载体生命周期是否一致仍未决 |
| Player entity | 当前 Player 能力组件/状态 | 不新增 Fishing 资格 Component；可通过关系被 `FishingAttemptStateComponent` 引用 | 不复制 `PlayerFishingCapabilityState` 的能力字段到 Fishing | 玩家能力有独立生命周期和 owner；Fishing 只需要尝试快照 |
| Item instance/entity | 当前 Item 定义/实例和库存组件 | 不新增 Fishing Component | `FishingBaitReservationComponent` 不附着于 Item 实例 | 鱼饵能力属于 Item；尝试侧只保存引用和状态 |
| Liquid Tile/world storage | 当前 Liquid/World 组件 | 不新增 Fishing Component | `FishingEligibilitySnapshotComponent` 不附着于液体 Tile | 液体是外部环境权威；Fishing 只保存一次读取快照 |
| ContentCatalog/world rules | 当前 `FishingDropRuleCatalog`、规则定义和世界规则 | 不新增 Fishing Component | `FishingCatchDecisionComponent` 不复制规则表 | 内容定义与一次结果的生命周期不同 |
| NPC entity | 当前 NPC/Spawn 生命周期组件 | 不新增 Fishing Component | `FishingResultCommitStateComponent` 不附着于 NPC | NPC 只在外部结果确认后存在，Fishing 不拥有其实体生命周期 |

## 7. 组件拆分与合并决策

### 7.1 必须拆分

| 拆分 | 理由 |
| --- | --- |
| `FishingAttemptStateComponent` 与 `BobberTimingComponent` | 尝试身份/终态与每 Tick 浮标计时的失效条件、更新频率和可能 owner 不同；合并会把 Projectile 载体字段和事务状态揉成巨型组件 |
| `BobberTimingComponent` 与 `FishingEligibilitySnapshotComponent` | 时序持续变化，资格快照应在一个评估时点保持一致；把快照放进时序组件会产生隐式镜像 |
| `FishingEligibilitySnapshotComponent` 与 `FishingCatchDecisionComponent` | 前者是输入快照，后者是不可变结果；决策不应因外部环境变化而被重新解释 |
| `FishingBaitReservationComponent` 与 `FishingResultCommitStateComponent` | 鱼饵实际状态属于 ItemContainerAndEconomy，Item/NPC 结果属于不同外部提交边界；合并会掩盖部分失败和重试语义 |
| 外部实体引用与结果类型 | 类型 ID、实例 ID、网络 ID 和持久化 ID 的生命周期不同；不能用一个整数同时表示内容类型和实体身份 |

### 7.2 可以保持为同一 Component 的字段组

- `FishingAttemptStateComponent` 中的 `AttemptId`、`Owner`、`Bobber`、`Phase` 和 `Revision` 共同表达一次尝试的身份与生命周期，暂时保持同组；若独立尝试实体方案与载体方案的生命周期差异继续扩大，应由整合会话重新评估。
- `FishingEligibilitySnapshotComponent` 中的水池统计、玩家钓鱼能力和世界/Biome 派生值共同描述同一次资格读取结果，必须带同一内容/世界读取时点，不拆为多个外部镜像组件。
- `FishingCatchDecisionComponent` 中的结果种类、类型引用、数量和任务鱼标志共同维护“结果种类与字段有效性”的不可分割不变量。
- `FishingBaitReservationComponent` 中的实例引用、类型、预留键和状态共同维护一次鱼饵预留的幂等关系，但最终 owner 仍待裁决。

### 7.3 不以字段数量为依据

`FishingEligibilitySnapshotComponent` 字段较多，但它们共享单次资格评估生命周期和快照不变量；如果按每个环境因素单独拆分，会制造跨组件同步，而不是降低耦合。相反，旧 `FishingBobberStateComponent` 把时序、结果和环境字段混在一起，字段数量不多也仍然需要按生命周期拆分。

## 8. 不单独创建 Component 的对象

| 对象 | 处理方式与理由 |
| --- | --- |
| 单个 bobber Projectile | 现有 Projectile 实例/载体；只附加钓鱼时序或尝试关系，不为每个 Projectile 类型再造一级 Component |
| 单个 fish species | ContentCatalog 的 Item/NPC 定义或候选类型，不拥有一次尝试生命周期 |
| 单个 fish drop rule | 规则定义/策略；规则列表属于内容目录，不是实体状态 |
| 单个 bait field | Item 内容属性；实际堆叠和扣减属于 ItemContainerAndEconomy |
| 单个 fishing biome | 环境派生值/快照输入；Biome owner 不迁入 Fishing |
| 单个 catch animation 或 UI bobber | 客户端表现数据，不是权威状态 |
| 单个 network packet | 外部协议值，不是尝试状态；网络 ID 也不能替代领域 ID |
| 单个 Sonar value | 结果表现的派生值，不应与捕获决策共享写入生命周期 |
| 单个 water Tile | Liquid/WorldStorage 状态；Fishing 只保存统计快照 |
| 单个 Item/NPC 实例 | 分别由 ItemContainerAndEconomy 和 SpawnLifecycleAndLoot 拥有；Fishing 只保存类型引用和外部确认关系 |

## 9. 当前 NLTX 组件覆盖

| 当前组件/定义 | 当前状态 | 对目标组件的映射 | 不应做的处理 |
| --- | --- | --- | --- |
| `D:\TRbackup\NLTX\src\Player\PlayerFishingCapabilityState.cs:3-27` | `status: existing`，能力元数据 | 作为 Player owner 的输入，不复制到 Fishing | 不改名为 Fishing Component，不让 Fishing 写入玩家能力 |
| `D:\TRbackup\NLTX\src\Content\FishingDropRuleDefinition.cs:5-13` | `status: existing`，规则定义 | 作为 ContentCatalog 输入，不复制到 `FishingCatchDecisionComponent` | 不把每条规则实体化为 Component |
| `D:\TRbackup\NLTX\src\Content\FishingConditionDefinition.cs:5-10` | `status: existing`，条件定义 | 作为规则内容输入，不变成尝试状态 | 不把条件匹配结果当作规则定义状态 |
| `D:\TRbackup\NLTX\src\Content\FishingDropRuleCatalog.cs:6-28` | `status: existing`，不可变规则目录 | 作为内容目录 owner 候选；`IsResolutionPathConfirmed` 仍是缺口信号 | 不把目录直接当作 CatchDecision 或结果实例 |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Fishing\FishingBobberPhase.cs:3-8` | `status: partial` | 可作为 `FishingAttemptStateComponent.Phase` 的现状输入 | 不把枚举存在误报为完整生命周期 |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Fishing\FishingBobberStateComponent.cs:5-26` | `status: partial` | 字段分散映射到 5 个候选 Component | 不继续追加字段形成巨型 Fishing Component |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Player\Systems\PlayerFishingUseSystem.cs:57-100` | 非 Component 的局部鱼饵读取/直接扣减路径，`status: partial` | 只作为鱼饵字段生命周期证据 | 不把直接 `SetSlot` 路径当作本设计的权威预留状态 |

当前 NLTX 未发现以下 Component 的已闭合实现：独立 `AttemptId`、显式等待计时、完整资格快照、Item/NPC 双结果决策、尝试级鱼饵预留和结果提交确认。

## 10. 组件级 evidence-gap

| ID | evidence-gap | 影响的 Component | 状态 |
| --- | --- | --- | --- |
| `EG-COMP-01` | Version4 的 `AI_061_FishingBobber` 和 Item 交付方法为空；完整参考只能补足同路径成员级行为 | `FishingAttemptStateComponent`、`BobberTimingComponent`、`FishingCatchDecisionComponent` | `partial` |
| `EG-COMP-02` | Version4 未检出完整 `GetFishingConditions`、鱼饵扫描和鱼饵消耗方法；当前 Dome 只有局部直接扣减 | `FishingEligibilitySnapshotComponent`、`FishingBaitReservationComponent` | `partial` / `missing` |
| `EG-COMP-03` | Version4 FishDropRule 只有声明字段，规则匹配/停止条件成员缺失 | `FishingEligibilitySnapshotComponent`、`FishingCatchDecisionComponent` | `partial` |
| `EG-COMP-04` | Version4 用 `ai`/`localAI` 浮点哨兵混合表达时序、鱼饵和结果，显式字段边界尚未由目标版本完整确认 | `BobberTimingComponent`、`FishingCatchDecisionComponent`、`FishingBaitReservationComponent` | `partial` |
| `EG-COMP-05` | 任务鱼、Biome、天气、世界时间和 Liquid 的跨组件快照 owner 未最终裁决 | `FishingEligibilitySnapshotComponent` | `unresolved` |
| `EG-COMP-06` | Item/NPC 外部实例 ID、提交确认和失败恢复没有 Version4 统一状态字段 | `FishingResultCommitStateComponent` | `unresolved` |
| `EG-COMP-07` | 当前协议有 FishOutNpc 数据形状，但不能证明 Fishing 结果状态已在 active simulation path 接通 | `FishingResultCommitStateComponent` | `partial` |
| `EG-COMP-08` | 当前没有 Fishing-specific focused verifier；本文件只能保留设计状态，不能宣称组件验证通过 | 全部 proposed Component | `missing` |

以上缺口不阻止生成候选组件设计，但阻止将 `designStatus` 提升为 `baseline`。

## 11. 未决组件 owner

### `BD-COMP-01`：尝试状态的实体范围

- 冲突字段：`AttemptId`、`Owner`、`Bobber`、`Phase`、`BobberTimingComponent` 的附着范围。
- 候选 A：尝试状态附着在现有 Bobber Projectile 上。优点是贴近 Version4 和当前 Dome 的创建关系；缺点是 Projectile 销毁、网络复制和交易终态被强耦合。
- 候选 B：为每次尝试使用独立实体，Projectile 只保存关联关系。优点是尝试事务可以独立于物理载体；缺点是需要额外的关系、清理和复制语义。
- 影响：改变 `FishingAttemptStateComponent`、`BobberTimingComponent` 的 `entityScope` 和创建/清理生命周期。
- 当前不能裁决：Version4 只证明 Projectile 载体和浮标调用点，不能从空实现推导最终实体模型。

### `BD-COMP-02`：资格快照的存储范围

- 冲突字段：水池统计、玩家能力、世界/Biome/任务派生值是一次尝试快照，还是仅作为短期值传递。
- 候选 A：将完整资格快照作为尝试实体上的 `FishingEligibilitySnapshotComponent`，直到结果决策结束。
- 候选 B：只保留最小尝试字段，资格快照不作为持续 Component；需要重新评估时从外部 owner 重新读取。
- 影响：改变 Component 数量、快照生命周期、内容版本审计和环境变化时的行为保持风险。
- 当前不能裁决：完整参考有 `FishingAttempt`/`FishingContext`，但 Version4 主基线缺少闭合构建路径。

### `BD-COMP-03`：鱼饵预留与结果提交的 owner

- 冲突字段：`BaitItemInstanceId`、`ReservationState`、`ExternalResultId`、`CommitState`。
- 候选 A：Fishing 持有尝试侧预留/提交协调字段，ItemContainerAndEconomy 和 SpawnLifecycleAndLoot 各自拥有实际实例写入。
- 候选 B：预留和提交状态分别归入对应外部领域，Fishing 只保存最小关联状态。
- 影响：改变 `FishingBaitReservationComponent` 和 `FishingResultCommitStateComponent` 是否存在于 Fishing 实体、失败重试边界和跨域幂等键布局。
- 当前不能裁决：完整参考直接修改库存并调用 Item/NPC 创建路径；Version4 没有统一事务状态，不能擅自选择架构 owner。

上述三项均为 `decision-required`，不是实现缺陷的临时默认值。

## 12. 最终 Component 清单

### 12.1 FishingAndCatchSimulation 候选 Component

| componentId | name | status | designStatus | entityScope | componentOwner |
| --- | --- | --- | --- | --- | --- |
| `FISHING-ATTEMPT-STATE` | `FishingAttemptStateComponent` | `proposed` | `decision-required` | FishingAttempt entity 或 Bobber Projectile，取决于 `BD-COMP-01` | FishingAndCatchSimulation 候选；共享 ID 为 `integration-review` |
| `FISHING-BOBBER-TIMING` | `BobberTimingComponent` | `proposed` | `candidate` | Bobber Projectile 或 FishingAttempt entity | FishingAndCatchSimulation；载体关系为 `integration-review` |
| `FISHING-ELIGIBILITY-SNAPSHOT` | `FishingEligibilitySnapshotComponent` | `proposed` | `candidate` | FishingAttempt entity；取决于 `BD-COMP-02` | FishingAndCatchSimulation；外部输入为 `integration-review` |
| `FISHING-CATCH-DECISION` | `FishingCatchDecisionComponent` | `proposed` | `candidate` | FishingAttempt entity | FishingAndCatchSimulation；Item/NPC 类型引用为 `integration-review` |
| `FISHING-BAIT-RESERVATION` | `FishingBaitReservationComponent` | `proposed` | `decision-required` | FishingAttempt entity | `integration-review`；ItemInstanceId 和库存 owner 未裁决 |
| `FISHING-RESULT-COMMIT-STATE` | `FishingResultCommitStateComponent` | `proposed` | `decision-required` | FishingAttempt entity | `integration-review`；外部结果确认 owner 未裁决 |

组件数量：**6 个 proposed Fishing Component 候选**。

### 12.2 复用但不新增的相邻 Component/定义

以下不是本设计新增的 Fishing Component：

- `PlayerFishingCapabilityState`：当前 `status: existing`，继续由 Player 能力范围拥有。
- `FishingDropRuleDefinition`、`FishingConditionDefinition`、`FishingDropRuleCatalog`：当前 `status: existing`，继续由 ContentCatalog 范围拥有。
- 当前 Dome 的 Projectile bobber 标识和玩家/库存状态：保留其相邻领域 owner，不在本文件复制。
- Liquid、Item、NPC、网络和世界状态：只通过关系或资格快照被引用，最终 owner 为 `integration-review` 或相邻子系统候选。

## 13. 最终声明

本文件是 Component-only Design。

本文件不定义 System、Query、Command、Event、Adapter、Projection、调度顺序、运行时实现、测试计划或迁移计划。

所有 `status: proposed` 的 Component 都只是设计提案。本文件不声明代码已创建、已迁移、行为等价或验证通过。

本文件仅新增于自动推导的设计路径，未修改来源研究报告、Version4、完整参考源码、tModLoader 文档、Space Station 14、NLTX 源码、测试文件或其他设计文档。本轮未启动子代理，未运行构建或测试，`verificationStatus: not-run`。
