# FishingAndCatchSimulation Component Code Draft

## 1. 草案元数据

| 项目 | 值 |
|---|---|
| subsystemId | FishingAndCatchSimulation |
| taskNumber | 15 |
| sourceComponentDesign | D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\design\2026-09-05-version4-fishing-and-catch-simulation-component-design.md |
| sourceReport | D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-05-version4-fishing-and-catch-simulation-public-decomposition.md |
| outputDraftPath | D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\design\2026-09-05-version4-fishing-and-catch-simulation-component-code-draft.md |
| draftKind | actual-csharp-component-skeleton |
| designScope | component-code-skeleton |
| designStatus | decision-required |
| codeStatus | draft-not-implemented |
| evidenceStatus | partial |
| nltxStatus | partial |
| verificationStatus | not-run |
| compileStatus | not-run |

本文件把既有 Component-only Design 转换为接近实际 C# 文件的声明草案。本文档中的代码块只用于评审字段形状、访问修饰符、默认值、只读派生属性和不变量边界；它们不是已经创建的 .cs 文件，也不是可直接替换当前实现的完整代码。

本轮只新增本 Markdown 文件，未创建或修改生产源码、测试、项目文件、研究报告或已有 Component Design。

## 2. 草案范围与明确排除项

### 2.1 本草案包含

- 6 个 FishingAndCatchSimulation 候选 Component 的 C# 风格声明；
- 每个 Component 的建议文件名、候选 namespace、owner、实体范围和生命周期；
- 权威字段、快照字段、兼容字段和只读派生属性的区分；
- 字段默认值、不变量注释和当前 NLTX 映射；
- 尚未确认的共享 ID、枚举、值对象和跨子系统 owner 的 TODO 占位。

### 2.2 本草案不包含

- 实际 .cs 文件、.csproj 修改、命名空间迁移或旧类型删除；
- System、Query、Command、Event、Adapter、Projection 或调度顺序；
- 钓鱼资格计算、掉落规则匹配、随机数滚动、鱼饵扣减或 Item/NPC 创建算法；
- 网络发送、存档、日志、时钟、随机源、客户端渲染和表现层；
- 持久化格式、协议 DTO、主循环接线、迁移计划或 focused verifier；
- Version4 行为已经被完整恢复、代码已经编译或测试已经通过的声明。

代码片段中的 TODO、decision-required 和 proposed 是设计边界，不是允许实现者凭经验补齐的默认行为。

## 3. 建议的代码位置与代码形状

### 3.1 建议文件组织

6 个 Component 建议在稳定的 Fishing capability 边界下集中到：

~~~text
D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Fishing\Components\
~~~

建议文件清单：

| 建议文件 | 核心公共类型 | 草案状态 | 主要决策点 |
|---|---|---|---|
| FishingAttemptStateComponent.cs | FishingAttemptStateComponent | proposed | BD-COMP-01：独立 FishingAttempt entity 或 Bobber Projectile 载体 |
| BobberTimingComponent.cs | BobberTimingComponent | proposed | BD-COMP-01：时序状态的载体 |
| FishingEligibilitySnapshotComponent.cs | FishingEligibilitySnapshotComponent | proposed | BD-COMP-02：快照是否持续存储 |
| FishingCatchDecisionComponent.cs | FishingCatchDecisionComponent | proposed | Item/NPC 结果引用和规则版本 owner |
| FishingBaitReservationComponent.cs | FishingBaitReservationComponent | proposed | BD-COMP-03：预留状态的事务 owner |
| FishingResultCommitStateComponent.cs | FishingResultCommitStateComponent | proposed | BD-COMP-03：结果提交确认和幂等 owner |

当前 D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Fishing\FishingBobberStateComponent.cs 是 partial 的局部模型，不应和这些候选 Component 并行成为第二套权威状态。实际实现前必须先完成 owner、调用方和迁移审查。

### 3.2 代码形状规则

- 每个代码块只展示一个同名核心公共类型，遵循一个核心公共类型对应一个建议 .cs 文件。
- 组件只保存数据；构造函数只做初始字段赋值，不执行资格计算、随机滚动、库存写入或外部提交。
- 只读派生属性不能反向写入字段，也不能被当作第二份权威状态。
- struct 形状与当前 Dome Fishing 组件保持接近；包含关系、快照和事务状态的字段仍须在实现阶段评估复制语义。
- 所有新增 Component 和未确认支撑类型均标注 status: proposed；当前已有类型只在映射说明中复用，不在本文重复定义。
- crossSubsystemOwner: integration-review 的字段不能在实现阶段被某个单独子系统默认为自己所有。
- Player 的能力、Item 的库存/堆叠、Liquid Tile、FishDropRule 定义和 NPC 实体生命周期不复制进 Fishing Component。

## 4. 类型与外部依赖说明

### 4.1 当前已存在、可作为实现输入的类型

以下类型在当前 NLTX 中已检出，但是否能直接作为跨项目公共 API 仍需按各项目引用关系确认：

| 类型 | 当前位置 | 草案用法 | 限制 |
|---|---|---|---|
| ItemEntityRef | D:\TRbackup\NLTX\src\Player\PlayerValueTypes.cs:3-9 | 可作为 Item 实例关系的候选表示 | 不能自动等同于已裁决的 ItemInstanceId |
| TileCoordinate | D:\TRbackup\NLTX\src\Player\PlayerValueTypes.cs:13 | 资格快照和结果来源坐标候选 | 坐标 owner 与跨项目引用仍需 integration-review |
| SimulationTick | D:\TRbackup\NLTX\src\Player\PlayerValueTypes.cs:17 | 若以后需要记录评估 tick 的候选值对象 | 本草案不新增评估时间字段 |
| ContentId<TDefinition> | D:\TRbackup\NLTX\src\Player\PlayerValueTypes.cs:19 | 内容定义 ID 的候选包装 | Item/NPC/Projectile 的具体定义类型尚未统一 |
| FishingBobberPhase | D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Fishing\FishingBobberPhase.cs:3-8 | 当前 Dome 阶段字段的部分映射 | 现有枚举存在不等于完整 Fishing lifecycle 已接通 |
| PlayerFishingCapabilityState | D:\TRbackup\NLTX\src\Player\PlayerFishingCapabilityState.cs:3-27 | 资格快照的外部输入 | 不复制到 Fishing Component，也不由 Fishing 写入 |
| FishingDropRuleCatalog | D:\TRbackup\NLTX\src\Content\FishingDropRuleCatalog.cs:6-28 | 结果判定的外部规则输入 | 目录和规则定义不成为 CatchDecision 的字段集合 |

### 4.2 仅作为代码占位、当前尚未确认的类型

下列名称来自 Component Design 的关系或字段语义，当前 NLTX 没有足够证据确认其最终声明。代码草案引用这些名字，是为了把边界显式化；本文件不私自添加它们的 .cs 定义：

| proposed 类型 | 作用 | owner 状态 |
|---|---|---|
| FishingAttemptId | 一次尝试的稳定身份 | integration-review |
| FishingTerminalReason | 尝试拒绝、取消、提交完成或过期的终态原因 | integration-review |
| PlayerEntityId | 玩家领域实体关系 | integration-review；不能直接把网络 slot 当领域 ID |
| ProjectileEntityId | 浮标 Projectile 领域关系 | integration-review；不能和网络 ID、槽位或持久化 ID 混用 |
| ItemInstanceId | 选中的实际鱼饵实例关系 | integration-review；当前仅有 ItemEntityRef 候选 |
| NpcEntityId | 已提交 NPC 实例关系 | integration-review；不应在 CatchDecision 创建时提前分配 |
| FishingExternalResultId | Item/NPC 外部结果关系的候选判别式包装 | integration-review；不得替代 Item/NPC 各自的实体 owner |
| IdempotencyKey | 鱼饵/结果事务的幂等身份 | integration-review |
| FishingAttemptPhase、FishingBiteState | 尝试和浮标时序的语义枚举 | FishingAndCatchSimulation 候选；与当前 FishingBobberPhase 的映射未裁决 |
| FishingLiquidKind | 资格快照中的液体类型 | integration-review；不能替代 Liquid owner |
| FishingOutcomeKind | None、Item、NPC 等结果类型 | integration-review；具体集合未锁定 |
| BaitReservationState、FishingCommitState | 鱼饵与结果提交状态机的值 | integration-review；Version4 无统一状态字段 |
| FishingBiomeFlags、FishingRarityFlags | 不透明资格输入快照 | integration-review；位宽和持久化表示未定 |
| FishingRngAuditValue | 决策时的 RNG 审计值 | integration-review；Version4 没有尝试级审计字段 |
| FishingLegacyBobberState | 兼容层的旧 ai/localAI 状态载体 | integration-review；不得成为新业务 API |

这些类型全部应在实现评审中继续保留 status: proposed，直到源代码、跨项目依赖和 owner 得到确认。

## 5. Component 代码骨架

### 5.1 FishingAttemptStateComponent.cs

建议路径：

~~~text
D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Fishing\Components\FishingAttemptStateComponent.cs
~~~

组件元数据：

| 项目 | 值 |
|---|---|
| componentId | FISHING-ATTEMPT-STATE |
| status | proposed |
| componentOwner | FishingAndCatchSimulation candidate |
| crossSubsystemOwner | integration-review for shared IDs and carrier relation |
| entityScope | 一个 FishingAttempt entity，或 BD-COMP-01 选择的 Bobber Projectile entity |
| lifecycle | 合法尝试开始时创建；持续到 resolving/retracting 的终态；提交确认、拒绝、取消或过期后清理 |

代码草案：

~~~csharp
namespace Terraria.Dome.Simulation.Fishing.Components;

// status: proposed
// componentId: FISHING-ATTEMPT-STATE
// designStatus: decision-required
// crossSubsystemOwner: integration-review
public struct FishingAttemptStateComponent
{
  public FishingAttemptStateComponent(
    FishingAttemptId attemptId,
    PlayerEntityId owner,
    ProjectileEntityId? bobber)
  {
    AttemptId = attemptId;
    Owner = owner;
    Bobber = bobber;
    Phase = FishingAttemptPhase.Waiting;
    Revision = 0;
    TerminalReason = null;
  }

  // TODO: proposed type; Version4 has no explicit attempt identity.
  public FishingAttemptId AttemptId;

  // TODO: proposed domain relation; do not store Projectile.owner's network slot directly.
  public PlayerEntityId Owner;

  // TODO: proposed domain relation; this is not a Projectile copy or network ID.
  public ProjectileEntityId? Bobber;

  // TODO: proposed semantic phase; map from current FishingBobberPhase after BD-COMP-01.
  public FishingAttemptPhase Phase;

  // Version4 has no explicit attempt revision; candidate optimistic/convergence field.
  public uint Revision;

  // TODO: proposed terminal audit value; null while the attempt is non-terminal.
  public FishingTerminalReason? TerminalReason;

  // Read-only projection; it does not create or clear a terminal reason.
  public bool IsTerminal => TerminalReason.HasValue;
}
~~~

字段与默认值：

| 成员 | 类型 | 默认值 | 状态分类 | 草案约束 |
|---|---|---|---|---|
| AttemptId | FishingAttemptId | 构造时绑定 | 权威身份 | 同一尝试全生命周期不变；重试不得生成新 ID |
| Owner | PlayerEntityId | 构造时绑定 | 权威关系 | 不能直接使用 Projectile.owner 的网络/槽位值 |
| Bobber | ProjectileEntityId? | null 或构造时绑定 | 关系 | 只保存关系，不复制 Projectile 位置、速度、碰撞或 timeLeft |
| Phase | FishingAttemptPhase | Waiting 候选 | 权威阶段 | 与决策、鱼饵预留和提交状态的组合规则必须由整合裁决 |
| Revision | uint | 0 | 权威版本候选 | 仅在权威状态变化时递增；外部旧确认不得回退版本 |
| TerminalReason | FishingTerminalReason? | null | 终态审计候选 | 只有拒绝、取消、提交完成或过期时才有值 |
| IsTerminal | bool | false | 只读派生值 | 只由 TerminalReason 派生，不单独写入 |

当前 NLTX 映射：

- FishingBobberStateComponent.Owner、BobberProjectileType 和 Phase（D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Fishing\FishingBobberStateComponent.cs:5-18）仅提供部分载体状态。
- 当前没有 AttemptId、Revision 或统一终态字段，因此不能把现有 FishingBobberStateComponent 宣称为本 Component 的实现。
- Version4 的 Projectile.owner、bobber 和 aiStyle 只能证明兼容载体关系，不能证明独立尝试 entity 的最终方案。证据：D:\TRbackup\Version4\Terraria\Projectile.cs:90-138、:25363-25366。

不变量：

- AttemptId、Owner、Phase 必须共同属于同一个尝试；缺失关系时不能视为可提交状态。
- Bobber 是实体关系，不是旧 Projectile 的完整快照。
- Phase 不应通过多个可空字段的偶然组合隐式表达终态。
- BD-COMP-01 未裁决前，不得在实现中同时把该组件声明为独立尝试和 Bobber 载体两套权威状态。

### 5.2 BobberTimingComponent.cs

建议路径：

~~~text
D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Fishing\Components\BobberTimingComponent.cs
~~~

组件元数据：

| 项目 | 值 |
|---|---|
| componentId | FISHING-BOBBER-TIMING |
| status | proposed |
| componentOwner | FishingAndCatchSimulation for fishing-specific timing |
| crossSubsystemOwner | integration-review for Projectile relation and time representation |
| entityScope | 一个 Bobber Projectile entity，或 BD-COMP-01 选择的 FishingAttempt entity |
| lifecycle | 浮标创建时初始化；等待/咬钩/收线期间更新；载体移除或尝试终态后清理 |

代码草案：

~~~csharp
namespace Terraria.Dome.Simulation.Fishing.Components;

// status: proposed
// componentId: FISHING-BOBBER-TIMING
// designStatus: candidate
// crossSubsystemOwner: integration-review
public struct BobberTimingComponent
{
  public BobberTimingComponent(int biteThreshold)
  {
    ElapsedTicks = 0;
    BiteThreshold = biteThreshold;
    RetractRequested = false;
    BiteState = FishingBiteState.Waiting;
    LegacyState = null;
  }

  // Fishing-specific elapsed time. This is not Projectile.timeLeft.
  public int ElapsedTicks;

  // TODO: value is a candidate from the complete reference only; Version4 did not confirm it.
  public int BiteThreshold;

  public bool RetractRequested;

  // TODO: proposed semantic detail; must not contradict FishingAttemptStateComponent.Phase.
  public FishingBiteState BiteState;

  // Compatibility-only boundary for old ai/localAI values.
  // TODO: proposed type; do not expose it as a business API or double-write it.
  public FishingLegacyBobberState? LegacyState;

  public bool HasReachedBiteThreshold =>
    BiteThreshold >= 0 && ElapsedTicks >= BiteThreshold;
}
~~~

字段与默认值：

| 成员 | 类型 | 默认值 | 状态分类 | 草案约束 |
|---|---|---|---|---|
| ElapsedTicks | int | 0 | 权威 Fishing 时序 | 不得为负；不能代替 Projectile timeLeft |
| BiteThreshold | int | 构造参数；660 仅为完整参考候选 | 配置/兼容值 | 具体默认值须在 Version4 行为映射接受后锁定 |
| RetractRequested | bool | false | 权威收线事实 | 重复请求不能改变既有结果语义 |
| BiteState | FishingBiteState | Waiting 候选 | 时序细节 | 必须与尝试阶段保持可解释的一致关系 |
| LegacyState | FishingLegacyBobberState? | null | 兼容载体 | 只允许在兼容边界单向映射，不作为新业务读取 API |
| HasReachedBiteThreshold | bool | false | 只读派生值 | 只根据本组件字段计算，不触发咬钩、随机或结果写入 |

当前 NLTX 映射：

- FishingBobberStateComponent.SwingCount 和 Phase（D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Fishing\FishingBobberStateComponent.cs:5-26）只有局部时序线索。
- 当前没有显式 ElapsedTicks、BiteThreshold 或 RetractRequested。
- Version4 有 ai、localAI 和 Fishing AI 入口，但 AI_061_FishingBobber 为空：D:\TRbackup\Version4\Terraria\Projectile.cs:128、:25363-25366、:34071-34074。
- 660 只来自完整参考 D:\TRbackup\无任何删减通过编译\Terraria\Projectile.cs:51419-51423，不能报告为 Version4 已确认常量。

不变量：

- ElapsedTicks 的失效条件与通用 Projectile 生命周期不同，不能把 timeLeft 直接迁移进来。
- 收线状态不保存 Item/NPC 结果；结果必须进入 FishingCatchDecisionComponent。
- 兼容字段与规范字段不得双向双写，否则旧浮点槽位会重新成为第二份权威状态。

### 5.3 FishingEligibilitySnapshotComponent.cs

建议路径：

~~~text
D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Fishing\Components\FishingEligibilitySnapshotComponent.cs
~~~

组件元数据：

| 项目 | 值 |
|---|---|
| componentId | FISHING-ELIGIBILITY-SNAPSHOT |
| status | proposed |
| componentOwner | FishingAndCatchSimulation for attempt-scoped snapshot |
| crossSubsystemOwner | integration-review for Player/World/Liquid/Content values |
| entityScope | 一个 FishingAttempt entity；不是 World、Player 或 Liquid entity |
| lifecycle | 资格数据组装完成后创建；结果选择期间只读；决策形成、拒绝或取消后清理 |

代码草案：

~~~csharp
using Terraria.Player;

namespace Terraria.Dome.Simulation.Fishing.Components;

// status: proposed
// componentId: FISHING-ELIGIBILITY-SNAPSHOT
// designStatus: candidate
// crossSubsystemOwner: integration-review
public struct FishingEligibilitySnapshotComponent
{
  public FishingEligibilitySnapshotComponent(
    TileCoordinate pondCoordinate,
    int bobberTypeId)
  {
    PondCoordinate = pondCoordinate;
    BobberTypeId = bobberTypeId;
    LiquidKind = FishingLiquidKind.Unknown;
    WaterTilesCount = 0;
    WaterNeededToFish = 0;
    WaterQuality = 0;
    ChumsInWater = 0;
    PolePower = 0;
    PoleItemType = 0;
    BaitPower = 0;
    BaitItemType = 0;
    FishingLevel = 0;
    FinalFishingLevel = 0;
    CanFishInLava = false;
    HeightLevel = 0;
    BiomeFlags = default;
    QuestFishType = null;
    RarityFlags = default;
    IsJunkCandidate = false;
    RulesetRevision = null;
  }

  // Existing NLTX value-object candidate; coordinate owner remains under review.
  public TileCoordinate PondCoordinate;

  // TODO: replace with the accepted ContentId<TDefinition> specialization if required.
  public int BobberTypeId;

  // TODO: proposed type; does not own Liquid Tile state.
  public FishingLiquidKind LiquidKind;

  public int WaterTilesCount;
  public int WaterNeededToFish;
  public float WaterQuality;
  public int ChumsInWater;

  // Snapshot of Player/Item inputs. These are not Player or Item ownership fields.
  public int PolePower;
  public int PoleItemType;
  public int BaitPower;
  public int BaitItemType;
  public int FishingLevel;
  public int FinalFishingLevel;
  public bool CanFishInLava;
  public int HeightLevel;

  // TODO: proposed opaque values; no Biome or rarity bit layout is confirmed.
  public FishingBiomeFlags BiomeFlags;
  public int? QuestFishType;
  public FishingRarityFlags RarityFlags;

  public bool IsJunkCandidate;

  // TODO: current NLTX catalog has no runtime ruleset revision.
  public uint? RulesetRevision;

  public bool HasEnoughWater =>
    WaterTilesCount >= 0 &&
    WaterNeededToFish >= 0 &&
    WaterTilesCount >= WaterNeededToFish;
}
~~~

字段与默认值：

| 成员 | 类型 | 默认值 | 状态分类 | 草案约束 |
|---|---|---|---|---|
| PondCoordinate | TileCoordinate | 构造时绑定 | 环境快照/关系 | 必须对应本次浮标位置；不拥有 Tile |
| BobberTypeId | int | 构造时绑定 | 内容快照 | 必须来自关联浮标；客户端不能任意替换 |
| LiquidKind | FishingLiquidKind | Unknown 候选 | 派生快照 | 必须和液体相关派生值保持一致 |
| WaterTilesCount | int | 0 | 环境快照 | 不得为负；扫描语义排除固体 Tile |
| WaterNeededToFish | int | 0 | 环境快照 | 不得为负；修正版本必须一致 |
| WaterQuality | float | 0 | 环境快照 | 必须是有限值；不能反写 Liquid owner |
| ChumsInWater | int | 0 | 环境快照 | 不得为负；由环境扫描输入 |
| PolePower、PoleItemType | int | 0 | Player/Item 输入快照 | 只保存本次资格评估使用的值 |
| BaitPower、BaitItemType | int | 0 | Player/Item 输入快照 | 实际鱼饵实例和扣减不归本组件 |
| FishingLevel、FinalFishingLevel | int | 0 | 派生快照 | 只能由同一能力/环境读取时点解释 |
| CanFishInLava | bool | false | 派生快照 | 不得单独覆盖 Liquid owner |
| HeightLevel | int | 0 候选 | 环境派生值 | 只表示本次评估的深度层级 |
| BiomeFlags | FishingBiomeFlags | default | 跨域快照 | 不保存 Biome 对象；位宽和 owner 未确认 |
| QuestFishType | int? | null | 任务快照 | 只读取当前任务；Fishing 不轮换任务 |
| RarityFlags | FishingRarityFlags | default | 规则输入快照 | 不承载规则定义本身 |
| IsJunkCandidate | bool | false | 派生快照 | 只反映当前资格规则输入 |
| RulesetRevision | uint? | null | 快照审计 | 缺失时不能用猜测版本代替 |
| HasEnoughWater | bool | true for zero-value struct | 只读派生值 | 仅为结构检查，不代表完整资格通过 |

当前 NLTX 映射：

- 当前 Dome FishingBobberStateComponent.WaterTilesCount、WaterNeededToFish、FishingLevel、IsInLava 和 IsInHoney（D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Fishing\FishingBobberStateComponent.cs:18-26）只能提供部分快照。
- PlayerFishingCapabilityState 已拥有玩家能力元数据，Fishing 只能读取并形成尝试快照，不应复制其全部字段：D:\TRbackup\NLTX\src\Player\PlayerFishingCapabilityState.cs:3-27。
- Version4 的 FishingAttempt 给出坐标、液体、等级、任务和结果字段形状：D:\TRbackup\Version4\Terraria.DataStructures\FishingAttempt.cs:3-52。
- 完整参考的水池扫描和玩家条件证据位于 D:\TRbackup\无任何删减通过编译\Terraria\Projectile.cs:19361-19520、:20374-20422 和 Player.cs:42678-42808。这些证据不能把 Version4 的缺失构建路径升级为已实现。

不变量：

- 字段必须属于同一个 AttemptId、同一个读取时点和同一规则版本语义。
- 快照是只读副本；外部 Player、Liquid、World、Biome、任务和 Content 状态仍由原 owner 管理。
- 未确认的外部值必须保持未绑定或 default，不能用一个“合理”常量冒充证据。
- WaterQuality 必须为有限值；WaterTilesCount、WaterNeededToFish 和 ChumsInWater 不得为负。
- HasEnoughWater 是结构层辅助属性，不等同于完整资格判定，也不触发任何外部读取。

### 5.4 FishingCatchDecisionComponent.cs

建议路径：

~~~text
D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Fishing\Components\FishingCatchDecisionComponent.cs
~~~

组件元数据：

| 项目 | 值 |
|---|---|
| componentId | FISHING-CATCH-DECISION |
| status | proposed |
| componentOwner | FishingAndCatchSimulation for the attempt-scoped decision |
| crossSubsystemOwner | integration-review for Item/NPC definition IDs and result references |
| entityScope | 一个 FishingAttempt entity |
| lifecycle | 资格通过且完成一次结果选择后创建；创建后原则上不可变；外部结果终态处理后清理 |

代码草案：

~~~csharp
namespace Terraria.Dome.Simulation.Fishing.Components;

// status: proposed
// componentId: FISHING-CATCH-DECISION
// designStatus: candidate
// crossSubsystemOwner: integration-review
public struct FishingCatchDecisionComponent
{
  public FishingCatchDecisionComponent(
    FishingOutcomeKind outcomeKind,
    int? itemTypeId,
    int? npcTypeId,
    int stackQuantity,
    bool isQuestFish,
    uint decisionRevision)
  {
    OutcomeKind = outcomeKind;
    ItemTypeId = itemTypeId;
    NpcTypeId = npcTypeId;
    StackQuantity = stackQuantity;
    IsQuestFish = isQuestFish;
    DecisionRevision = decisionRevision;
    RngAuditValue = null;
  }

  public FishingOutcomeKind OutcomeKind;

  // Content definition references, not Item/NPC entity instances.
  public int? ItemTypeId;
  public int? NpcTypeId;

  public int StackQuantity;
  public bool IsQuestFish;

  // Version4 has no explicit attempt-level decision revision.
  public uint DecisionRevision;

  // TODO: proposed opaque audit value; it must not reroll the decision.
  public FishingRngAuditValue? RngAuditValue;

  public bool HasItemResult => ItemTypeId.HasValue;

  public bool HasNpcResult => NpcTypeId.HasValue;
}
~~~

字段与默认值：

| 成员 | 类型 | 默认值 | 状态分类 | 草案约束 |
|---|---|---|---|---|
| OutcomeKind | FishingOutcomeKind | None | 权威决策 | 具体成员集合和 Item/NPC 优先级待整合锁定 |
| ItemTypeId | int? | null | Item 内容引用 | 只有 Item 类结果时有效；不是 Item 实例 ID |
| NpcTypeId | int? | null | NPC 内容引用 | 只有 NPC 类结果时有效；不是 NPC 实例 ID |
| StackQuantity | int | 0 | 结果值 | Item 结果数量不能为负；特殊堆叠规则待确认 |
| IsQuestFish | bool | false | 结果派生/快照值 | 由任务/规则输入产生；不更新任务进度 |
| DecisionRevision | uint | 0 | 权威版本候选 | 同一尝试只能有一个生效决策 |
| RngAuditValue | FishingRngAuditValue? | null | 审计快照 | 只记录已发生的决策审计值，不允许在读取时重新滚动 |
| HasItemResult、HasNpcResult | bool | false | 只读派生值 | 只由对应类型引用是否存在派生 |

当前 NLTX 映射：

- 当前 Dome FishingBobberStateComponent.PendingItemType（D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Fishing\FishingBobberStateComponent.cs:17）只覆盖单一待处理 Item 类型。
- 现有局部字段没有 NPC 结果、结果类型、堆叠数量、决策版本或 RNG 审计值。
- Version4 FishingAttempt.rolledItemDrop 和 rolledEnemySpawn 给出结果值的形状：D:\TRbackup\Version4\Terraria.DataStructures\FishingAttempt.cs:49-51。
- 完整参考的 Item/NPC 分支位于 D:\TRbackup\无任何删减通过编译\Terraria\Projectile.cs:19523-19557，只能作为目标字段证据，不能被解释为 Version4 当前活动路径已闭合。

不变量：

- ItemTypeId 和 NpcTypeId 不得同时表达两个成功结果；必须由 OutcomeKind 明确结果语义。
- Item/NPC 类型引用不等同于已创建的 Item/NPC 实例，不能在此组件中分配或修改实体生命周期。
- 决策形成后不得因客户端重复请求、Sonar 表现或网络重放而重新滚动。
- FishingCatchDecisionComponent 不扣鱼饵、不写库存、不创建 Item/NPC，也不拥有规则目录。

### 5.5 FishingBaitReservationComponent.cs

建议路径：

~~~text
D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Fishing\Components\FishingBaitReservationComponent.cs
~~~

组件元数据：

| 项目 | 值 |
|---|---|
| componentId | FISHING-BAIT-RESERVATION |
| status | proposed |
| componentOwner | integration-review candidate；Fishing 仅候选拥有尝试侧引用 |
| crossSubsystemOwner | integration-review |
| entityScope | 一个 FishingAttempt entity；不是库存或 Item entity |
| lifecycle | 鱼饵候选绑定时创建；保留到预留/消费成功、拒绝或终止确认；尝试终态时清理 |

代码草案：

~~~csharp
namespace Terraria.Dome.Simulation.Fishing.Components;

// status: proposed
// componentId: FISHING-BAIT-RESERVATION
// designStatus: decision-required
// crossSubsystemOwner: integration-review
public struct FishingBaitReservationComponent
{
  public FishingBaitReservationComponent(
    ItemInstanceId baitItemInstanceId,
    int baitItemTypeId,
    FishingAttemptId reservationKey)
  {
    BaitItemInstanceId = baitItemInstanceId;
    BaitItemTypeId = baitItemTypeId;
    ExpectedQuantity = 1;
    ReservationState = BaitReservationState.Unbound;
    ReservationKey = reservationKey;
    ConsumptionRevision = 0;
  }

  // TODO: proposed Item instance identity; current ItemEntityRef is only a candidate mapping.
  public ItemInstanceId BaitItemInstanceId;

  // Content/type snapshot; it does not identify the instance by itself.
  public int BaitItemTypeId;

  public int ExpectedQuantity;

  // TODO: proposed transaction state; final owner and transitions are not locked.
  public BaitReservationState ReservationState;

  // The reservation key must remain stable for one fishing attempt.
  public FishingAttemptId ReservationKey;

  // Version4 has no explicit consumption revision.
  public uint ConsumptionRevision;

  public bool IsBound =>
    ReservationState != BaitReservationState.Unbound;
}
~~~

字段与默认值：

| 成员 | 类型 | 默认值 | 状态分类 | 草案约束 |
|---|---|---|---|---|
| BaitItemInstanceId | ItemInstanceId | 未绑定 | 跨域 Item 关系 | 必须指向资格时选中的实际实例，不能只靠类型号定位 |
| BaitItemTypeId | int | 0 候选 | 类型快照/兼容值 | 必须和实例类型一致 |
| ExpectedQuantity | int | 1 候选 | 事务约束 | 当前参考路径候选为一次扣减 1；特殊规则由 Item owner 确认 |
| ReservationState | BaitReservationState | Unbound | 权威事务状态候选 | 不得从已消费回到已预留；重复请求须保持幂等 |
| ReservationKey | FishingAttemptId | 构造时绑定 | 幂等关系 | 同一尝试只能有一个生效扣减键 |
| ConsumptionRevision | uint | 0 | 审计版本候选 | 外部旧确认不得覆盖较新的拒绝/终止状态 |
| IsBound | bool | false | 只读派生值 | 只由 ReservationState 派生 |

当前 NLTX 映射：

- 当前 Dome 的 PlayerFishingUseSystem 在 D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Player\Systems\PlayerFishingUseSystem.cs:78-100 直接通过 InventoryComponent.SetSlot 扣减。
- 该路径没有 FishingAttemptId、Item 实例身份、预留状态或重复扣减版本，因而只能标为 partial，不能视为本 Component 已存在实现。
- 完整参考的鱼饵扫描和消费证据位于 D:\TRbackup\无任何删减通过编译\Terraria\Player.cs:52942-53105；Version4 没有统一预留状态。

不变量：

- 本组件不能直接通过字段改变库存数量；实际 Item 事务仍由 ItemContainerAndEconomy owner 处理。
- BaitItemInstanceId、BaitItemTypeId 和 ReservationKey 必须共同校验，避免同类型鱼饵被并发替换后误扣。
- ReservationState 的最终 owner、原子边界、拒绝语义和重试语义由 BD-COMP-03 决定。
- 在 BD-COMP-03 未裁决前，ReservationState 不能被当作已批准的生产状态机。

### 5.6 FishingResultCommitStateComponent.cs

建议路径：

~~~text
D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Fishing\Components\FishingResultCommitStateComponent.cs
~~~

组件元数据：

| 项目 | 值 |
|---|---|
| componentId | FISHING-RESULT-COMMIT-STATE |
| status | proposed |
| componentOwner | integration-review candidate；Fishing 只候选拥有尝试侧确认状态 |
| crossSubsystemOwner | integration-review |
| entityScope | 一个 FishingAttempt entity |
| lifecycle | 有外部 Item/NPC 结果时创建；保留至确认或终止失败；尝试关闭后清理，审计保留范围未定 |

代码草案：

~~~csharp
using Terraria.Player;

namespace Terraria.Dome.Simulation.Fishing.Components;

// status: proposed
// componentId: FISHING-RESULT-COMMIT-STATE
// designStatus: decision-required
// crossSubsystemOwner: integration-review
public struct FishingResultCommitStateComponent
{
  public FishingResultCommitStateComponent(
    FishingOutcomeKind resultKind,
    TileCoordinate origin,
    IdempotencyKey idempotencyKey)
  {
    ResultKind = resultKind;
    CommitState = FishingCommitState.NotSubmitted;
    CommitAttemptCount = 0;
    ExternalResultId = null;
    Origin = origin;
    IdempotencyKey = idempotencyKey;
  }

  // Must agree with FishingCatchDecisionComponent.OutcomeKind.
  public FishingOutcomeKind ResultKind;

  // TODO: proposed commit state; Version4 has no unified field.
  public FishingCommitState CommitState;

  public ushort CommitAttemptCount;

  // TODO: proposed discriminated external relation; Item and NPC IDs are not interchangeable.
  public FishingExternalResultId? ExternalResultId;

  // Existing coordinate candidate; source owner remains under review.
  public TileCoordinate Origin;

  // TODO: proposed stable transaction identity; Version4 has no explicit idempotency key.
  public IdempotencyKey IdempotencyKey;

  public bool IsCommitted =>
    CommitState == FishingCommitState.Committed;
}
~~~

字段与默认值：

| 成员 | 类型 | 默认值 | 状态分类 | 草案约束 |
|---|---|---|---|---|
| ResultKind | FishingOutcomeKind | None | 权威提交上下文 | 必须与 CatchDecision 的结果类型一致 |
| CommitState | FishingCommitState | NotSubmitted 候选 | 权威事务状态候选 | 已提交后不可再次提交；拒绝、重试和终止必须可区分 |
| CommitAttemptCount | ushort | 0 | 审计值候选 | 只统计同一幂等键的外部提交尝试 |
| ExternalResultId | FishingExternalResultId? | null | 外部关系 | 只有外部创建成功后填写；不提前分配实体 ID |
| Origin | TileCoordinate | 构造时绑定 | 来源快照 | 必须保持钓获来源；不拥有 Tile |
| IdempotencyKey | IdempotencyKey | 构造时绑定 | 权威事务身份候选 | 同一尝试/结果只能有一个生效键 |
| IsCommitted | bool | false | 只读派生值 | 只根据 CommitState 派生，不替代状态字段 |

当前 NLTX 映射：

- 当前 NLTX 有 FishOutNpcPacket 的协议数据形状，但没有与尝试绑定的提交确认状态；协议 DTO 不能证明本 Component 已接入活动模拟路径。
- Version4 的 Item 交付调用点为 D:\TRbackup\Version4\Terraria\Projectile.cs:47779-47787，NPC 结果消息邻域为 D:\TRbackup\Version4\Terraria\MessageBuffer.cs:3080-3104。
- 完整参考分别拥有 Item 投放和 NPC 创建路径，但没有统一的 ExternalResultId、幂等键或尝试级重试计数：D:\TRbackup\无任何删减通过编译\Terraria\Projectile.cs:51496-51552。

不变量：

- 本组件只记录尝试侧确认，不代表 Item/NPC 实体全部生命周期归 Fishing 所有。
- ExternalResultId 必须由相邻 owner 返回确认后再填入；Fishing 不提前分配外部实体。
- ResultKind 不能因重复收线或网络重放变更。
- Item 与 NPC 的最终 owner、原子提交、错误恢复和审计保留范围由 BD-COMP-03 决定。

## 6. 组件组合草案

本节只表达实体上的状态组合，不表达 System、Query、Command、Event、调度顺序或网络流程。

| 实体/对象 | 必需状态 | 可选状态 | 组合限制 |
|---|---|---|---|
| FishingAttempt entity（独立尝试候选） | FishingAttemptStateComponent | FishingEligibilitySnapshotComponent、FishingCatchDecisionComponent、FishingBaitReservationComponent、FishingResultCommitStateComponent、BobberTimingComponent | 是否使用该 entity 由 BD-COMP-01 决定 |
| Bobber Projectile entity（载体候选） | 当前 ProjectileBobberComponent / 浮标标识 | BobberTimingComponent、FishingAttemptStateComponent 或其关系投影 | 不把所有 Fishing 字段合并成一个 aggregate Component |
| Player entity | 当前 PlayerFishingCapabilityState 或 Dome 玩家能力组件 | 无新的 Fishing 资格 Component | Fishing 只读取并快照，不复制或拥有玩家能力 |
| Item instance/entity | 当前 Item 定义、实例和库存状态 | 无新的 Fishing Component | FishingBaitReservationComponent 不附着到 Item entity；它只保存尝试侧关系 |
| Liquid Tile/world storage | 当前 Liquid/World 状态 | 无新的 Fishing Component | FishingEligibilitySnapshotComponent 不附着到 Liquid Tile |
| ContentCatalog/world rules | 当前 FishingDropRuleCatalog 和规则定义 | 无新的 Fishing Component | CatchDecision 不复制规则表 |
| NPC entity | 当前 NPC/Spawn 生命周期组件 | 无新的 Fishing Component | Commit state 不附着到 NPC entity |

组合不变量：

- FishingCatchDecisionComponent 只能解释同一尝试的 FishingEligibilitySnapshotComponent，不能在快照被替换后悄悄重算。
- FishingResultCommitStateComponent.ResultKind 必须与 CatchDecision 一致。
- FishingBaitReservationComponent.ReservationKey 必须与同一尝试身份一致。
- BobberTimingComponent 的 Fishing elapsed time 不得映射成 Projectile 通用 timeLeft。
- BD-COMP-01、BD-COMP-02 和 BD-COMP-03 未裁决前，组件组合只能作为候选，不是 baseline。

## 7. 当前 NLTX 映射总表

| 当前文件/类型 | 状态 | 目标组件映射 | 不应做的处理 |
|---|---|---|---|
| D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Fishing\FishingBobberStateComponent.cs:5-26 | partial | Owner/Phase/环境字段分别映射到多个候选 Component | 不继续向该类型追加所有字段形成巨型组件 |
| D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Fishing\FishingBobberPhase.cs:3-8 | partial | 作为尝试/浮标阶段枚举的现状输入 | 不把枚举存在当成完整生命周期实现 |
| D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Player\Components\PlayerSkillStateComponents.cs:3-6 | partial | 玩家 Fishing skill 作为资格输入候选 | 不把 Player 能力搬到 Fishing Component |
| D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Player\Systems\PlayerFishingUseSystem.cs:57-100 | partial | 鱼竿/鱼饵读取和现有扣减路径的证据 | 不把直接 SetSlot 当作预留和幂等实现 |
| D:\TRbackup\NLTX\src\Player\PlayerFishingCapabilityState.cs:3-27 | existing | 资格快照的 Player 能力输入 | 不由 Fishing Component 重新拥有能力字段 |
| D:\TRbackup\NLTX\src\Content\FishingDropRuleDefinition.cs:5-13 | existing | CatchDecision 的外部规则输入 | 不把规则定义复制成一次尝试状态 |
| D:\TRbackup\NLTX\src\Content\FishingDropRuleCatalog.cs:6-28 | existing | CatchDecision 的外部目录输入 | 不把目录直接当作 CatchDecision 或结果实例 |
| D:\TRbackup\NLTX\src\Player\PlayerValueTypes.cs:3-19 | existing | Item/坐标/时间/内容值对象候选 | 不把一个值对象同时当网络 ID、持久化 ID 和领域实体 ID |

## 8. 未决项与 evidence-gap

### 8.1 必须保留的组件 owner 决策

#### BD-COMP-01：尝试状态附着范围

- 候选 A：附着在现有 Bobber Projectile 上。优点是贴近 Version4 和当前 Dome 的创建关系；缺点是把物理载体销毁、网络复制和结果事务耦合。
- 候选 B：为每次尝试创建独立 FishingAttempt entity，Projectile 只保存关系。优点是尝试事务可以独立于物理载体；缺点是需要额外关系、清理和复制语义。
- 影响：FishingAttemptStateComponent、BobberTimingComponent 的 entityScope、创建和清理生命周期都会改变。
- 当前不能裁决：Version4 只确认 Projectile 浮标载体和调用点；AI_061_FishingBobber 为空，不能从空实现推导最终实体模型。

#### BD-COMP-02：资格快照保存范围

- 候选 A：把完整资格快照作为尝试实体上的持续 Component，直到 CatchDecision 形成。
- 候选 B：只保留最小尝试字段，资格快照作为短期读取值，不在实体上持续保存。
- 影响：组件数量、环境变化时的行为保持、内容版本审计和重新评估语义都会改变。
- 当前不能裁决：完整参考有 FishingAttempt/FishingContext，但 Version4 主基线缺少闭合的快照构建路径。

#### BD-COMP-03：鱼饵预留与结果提交 owner

- 候选 A：Fishing 持有尝试侧预留/提交协调字段，ItemContainerAndEconomy 和 SpawnLifecycleAndLoot 各自拥有实际写入。
- 候选 B：预留和提交状态分别归入相邻领域，Fishing 只保存最小关联关系。
- 影响：两个事务 Component 是否保留在 Fishing entity、幂等键布局、失败重试和原子边界都会改变。
- 当前不能裁决：完整参考有直接库存修改和 Item/NPC 创建路径；Version4 没有统一事务状态，不能擅自选择。

### 8.2 当前证据缺口

| ID | 缺口 | 影响组件 | 当前状态 |
|---|---|---|---|
| EG-COMP-01 | Version4 的 Fishing AI 和 Item 交付方法为空；完整参考只能补足同路径成员级形状 | Attempt、BobberTiming、CatchDecision | partial |
| EG-COMP-02 | Version4 未闭合资格构建、鱼饵扫描和消费路径；当前 Dome 只有局部直接扣减 | Eligibility、BaitReservation | partial / missing |
| EG-COMP-03 | Version4 FishDropRule 只有声明字段，完整规则匹配和停止条件成员缺失 | Eligibility、CatchDecision | partial |
| EG-COMP-04 | ai/localAI 浮点槽位混合表达时序、鱼饵和结果 | BobberTiming、CatchDecision、BaitReservation | partial |
| EG-COMP-05 | 任务鱼、Biome、天气、世界时间和 Liquid 快照 owner 未最终裁决 | Eligibility | unresolved |
| EG-COMP-06 | Item/NPC 外部实例 ID、提交确认和失败恢复没有 Version4 统一字段 | ResultCommit | unresolved |
| EG-COMP-07 | 当前协议有 FishOutNpc 数据形状，但不能证明结果状态已接入 active simulation path | ResultCommit | partial |
| EG-COMP-08 | 当前没有 Fishing-specific focused verifier | 全部 proposed Component | missing |

## 9. 实现前检查清单

本清单用于后续实现评审，不是本轮执行的测试计划：

- [ ] 裁决 BD-COMP-01，只选择一种尝试实体/载体权威方案。
- [ ] 裁决 BD-COMP-02，确认资格快照是否需要持续 Component 生命周期。
- [ ] 裁决 BD-COMP-03，确认鱼饵预留和 Item/NPC 结果提交的事务 owner。
- [ ] 确认 FishingAttemptId、PlayerEntityId、ProjectileEntityId、ItemInstanceId、NpcEntityId 和 IdempotencyKey 的唯一声明位置。
- [ ] 确认 FishingAttemptPhase 与当前 FishingBobberPhase 的映射，避免同一阶段存在两套可写枚举。
- [ ] 确认 FishingOutcomeKind 的 Item/NPC/Junk/QuestFish 集合和字段互斥规则。
- [ ] 确认 BiteThreshold 的 Version4 来源；在此之前不能把 660 作为已验证常量。
- [ ] 确认 FishingBiomeFlags、FishingRarityFlags 和 RNG 审计值的位宽、版本和持久化语义。
- [ ] 结合目标项目引用关系决定是否采用 dome/.../Fishing/Components/，并遵守 Context/架构设计/ECS文件组织设计约束.md。
- [ ] 实际创建 .cs 前，按仓库 BUILD-CONCURRENCY-1 规则执行受约束的最小编译和 focused verifier。

## 10. 最终声明

本文档是 FishingAndCatchSimulation 的 actual C# component skeleton draft，不是实际代码提交。

6 个候选 Component 均保持 status: proposed：

- FISHING-ATTEMPT-STATE / FishingAttemptStateComponent
- FISHING-BOBBER-TIMING / BobberTimingComponent
- FISHING-ELIGIBILITY-SNAPSHOT / FishingEligibilitySnapshotComponent
- FISHING-CATCH-DECISION / FishingCatchDecisionComponent
- FISHING-BAIT-RESERVATION / FishingBaitReservationComponent
- FISHING-RESULT-COMMIT-STATE / FishingResultCommitStateComponent

designStatus: decision-required、codeStatus: draft-not-implemented、verificationStatus: not-run 和 compileStatus: not-run 均为本轮真实状态。未启动子代理，未创建 .cs，未修改生产代码，未运行构建或测试。
