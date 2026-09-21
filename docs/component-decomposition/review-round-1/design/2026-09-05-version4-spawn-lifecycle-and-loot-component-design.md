# Version4 Spawn Lifecycle and Loot Component-only Design

本文件是基于 Version4 公开分解研究结果形成的 Component-only Design。它用于收敛组件边界、字段、默认值、不变量、生命周期语义、实体范围、组合关系、ID 分类和组件 owner；它不是实现说明，也不是运行时处理方案。

## 1. 设计元数据

| 字段 | 值 |
|---|---|
| artifactType | Component-only Design |
| designStatus | decision-required |
| evidenceStatus | partial |
| nltxStatus | missing |
| verificationStatus | not-run |
| canonicalComponentCount | 8 |
| proposedCanonicalComponentCount | 8 |
| existingOrPartialCoverage | existing / partial |
| unresolvedOwnerCount | 7 |
| sourceResearch | D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-05-version4-spawn-lifecycle-and-loot-public-decomposition.md |
| taskPrompt | D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\design\2026-09-06-version4-component-only-design-session-prompt.md |
| repositoryRoot | D:\TRbackup\NLTX |
| designDate | 2026-09-06 |

状态解释：

- decision-required：组件候选已经收敛到可评审粒度，但跨子系统 owner、重复组件表示和 ID 分类仍有未决项，不能把本文件视为最终基线。
- evidenceStatus: partial：公开 Version4 证据和当前 NLTX 组件证据足以支持字段分组候选，但不足以证明完整生命周期、网络身份、持久化身份和跨域归属的一致契约。
- nltxStatus: missing：本文件不声称当前 NLTX 已经拥有这些 canonical Component 的完整实现；现有类型只按 existing 或 partial 记录覆盖关系。
- verificationStatus: not-run：本轮没有运行构建、测试或其他 compile-capable 命令，也没有以验证结果替代设计证据。

## 2. 设计范围与排除范围

### 2.1 纳入范围

本轮只处理以下数据责任：

- 生成候选是否被接纳、接纳来源和接纳尝试信息；
- 实体生命周期阶段、活跃性、终止原因和清理待决状态；
- 运行时身份、兼容槽位、网络身份、持久化身份和代际编号的分类；
- 实体与其父实体、拥有者或关联实体之间的一个明确关系；
- NPC 类型定义引用；
- NPC 的剩余寿命和人口计数相关状态；
- 掉落解析状态；
- 掉落归因，包括击杀者、可领取对象、幸运归属和掉落位置。

### 2.2 排除范围

下列内容不在本文件中定义：

- System、Query、Command、Event、Adapter 或 Projection；
- 任意处理顺序、调度顺序、线程模型、tick 顺序或网络发送顺序；
- NewNPC、NewProjectile、Kill、checkDead、NPCLoot 等 API 或行为实现；
- 战斗数值、伤害结算、生命值策略、AI、移动、碰撞和表现状态；
- 掉落规则算法、随机数算法、概率计算、物品生成算法和奖励分配算法；
- 网络 dirty 标记、客户端预测、协议包格式和消息处理；
- 存档格式、快照格式、持久化写入策略和恢复策略；
- 测试计划、迁移计划、发布计划或行为等价性声明；
- 将 NPC、Projectile、WorldItem 的全部状态合并成一个大型通用组件。

本文件中的组合关系只表示组件可以共同挂载于某类实体，不表示这些组件之间存在任何隐含的运行时执行顺序。

## 3. 组件设计依据

### 3.1 Version4 NPC 证据

研究报告核对了 Version4 Terraria.NPC.cs 中下列成员和区域：

- active、SpawnedFromStatue、CanBeReplacedByOtherNPCs；
- netUpdate、spawnNeedsSyncing、realLife；
- timeLeft、life、lifeMax、netID；
- NewNPC；
- checkDead；
- NPCLoot。

这些证据显示，NPC 的生成接纳、活跃生命周期、类型引用、关系引用、剩余寿命和掉落来源在原始模型中相互交织。组件设计将它们拆成不同的责任集合。life 和 lifeMax 属于战斗状态，不因为它们出现在死亡与掉落路径中就归入本轮组件。

### 3.2 Version4 Projectile 证据

研究报告核对了 Version4 Terraria.Projectile.cs 中下列成员和区域：

- active、owner、timeLeft、identity、netUpdate；
- NewProjectile；
- Kill。

这些成员分别提供了投射物活跃性、拥有者关系、剩余寿命、网络身份和终止路径的证据。投射物专属的现有 owner/lifetime 组件被视为 partial 覆盖；本文件只提出跨实体的语义候选，不把投射物所有运行时字段改写为统一组件。

### 3.3 Version4 Loot 证据

研究报告核对了以下掉落与奖励证据：

- CommonDrop 中的 itemId、chanceDenominator、amountDroppedMinimum、amountDroppedMaximum、chanceNumerator 和 ChainedRules；
- Player.ApplyDamageToNPC 与 Player.OnKillNPC；
- Player.DropItems 与 Player.TryDroppingSingleItem；
- CoinLossRevengeSystem 中的掉落来源、归因和位置相关数据。

CommonDrop 字段描述掉落规则定义，不是每个实体的解析状态，因此不直接变成 Component 字段。击杀者与掉落接收对象属于归因责任；规则是否已经解析、解析序列和提交状态属于 LootResolutionState，两者保持分离。

### 3.4 公开结构粒度参考

已核对的公开结构参考包括：

- tModLoader v2026.07 的 Main Page 以及 GlobalNPC、ModNPC、Recipe、ModPacket 类型页；
- Space Station 14 的 SpawnerSystem、SpawnOnDespawnSystem、ConditionalSpawnerSystem、EntitySpawnEntry 和 ContainerFillSystem；
- RobustToolbox 的可用结构范围。

这些资料只支持“生成、实体关系、寿命、掉落配置和组件边界可以按责任拆开”的结构粒度结论。它们不被用来推导 NLTX 的引擎级 ID、删除、网络或持久化契约。

### 3.5 当前 NLTX 证据

已直接核对的当前 NLTX 类型包括：

- Relationships.EntityReference：Guid EntityId、EntityReferenceScope、None 和 IsEmpty；
- Items.WorldPosition：X、Y 和 Origin；
- Items.WorldVector：X、Y 和 Zero；
- NPC lifecycle、lifetime、identity、definition reference、parent relation 和 replication 类型；
- Projectile lifetime 与 owner 类型；
- LootSource、LootAttribution、WorldItem、WorldItemReservation 和 ItemInstance 类型；
- dome 下与上述类型同名但字段存在差异的并行版本。

这些证据支持 existing/partial 映射，但没有证明 src 与 dome 已经共享一个完成的 canonical 契约。因此所有新类型仍标记 status: proposed。

## 4. Version4 成员到 Component 归属表

下表描述数据责任归属，不定义任何处理行为。标记为排除的成员仍是研究证据，但不进入本轮 Component。

| Version4 成员或区域 | 组件归属 | 归属字段或结论 | 覆盖状态 |
|---|---|---|---|
| NPC.active | EntityLifecycleState | IsActive | partial |
| NPC.SpawnedFromStatue | SpawnAdmissionState | SourceKind = statue 等生成来源分类 | partial |
| NPC.CanBeReplacedByOtherNPCs | 不直接创建组件 | 这是生成/替换策略输入，不是本轮稳定实体状态 | excluded |
| NPC.netUpdate | 不直接创建组件 | 网络同步 dirty 语义排除 | excluded |
| NPC.spawnNeedsSyncing | 不直接创建组件 | 网络同步需求语义排除 | excluded |
| NPC.realLife | EntityRelationState | RelatedEntity + RelationKind = linked-life | partial |
| NPC.timeLeft | NpcPopulationLifetimeState | RemainingTicks | partial |
| NPC.life、NPC.lifeMax | 不直接创建组件 | 战斗数值责任排除 | excluded |
| NPC.netID | NpcDefinitionReferenceState | NetId；解释为 NPC 类型或定义引用，不当作实例身份 | partial |
| NPC.NewNPC | SpawnAdmissionState、EntityIdentityState、EntityLifecycleState、NpcDefinitionReferenceState | 生成过程涉及的状态集合，不定义 API | proposed mapping |
| NPC.checkDead | EntityLifecycleState、LootResolutionState、LootAttributionState | 终止与掉落状态的证据入口，不定义调用顺序 | proposed mapping |
| NPC.NPCLoot | LootResolutionState、LootAttributionState | 掉落解析与归因分离 | proposed mapping |
| Projectile.active | EntityLifecycleState | IsActive | partial |
| Projectile.owner | EntityRelationState | RelatedEntity + RelationKind = owner | partial |
| Projectile.timeLeft | EntityLifecycleState 的终止相关语义；现有 ProjectileLifetimeComponent 作为 partial 覆盖 | 不归入 NPC population lifetime | partial |
| Projectile.identity | EntityIdentityState | NetworkId 的实例网络身份分类；不作为 RuntimeEntityId | partial |
| Projectile.netUpdate | 不直接创建组件 | 网络同步 dirty 语义排除 | excluded |
| Projectile.NewProjectile | SpawnAdmissionState、EntityIdentityState、EntityLifecycleState | 生成过程涉及的状态集合，不定义 API | proposed mapping |
| Projectile.Kill | EntityLifecycleState | TerminationReason 与 PendingCleanup 的证据入口 | proposed mapping |
| CommonDrop.itemId | 不直接创建组件 | 掉落规则定义字段 | excluded |
| CommonDrop.chanceDenominator、chanceNumerator | 不直接创建组件 | 掉落规则定义字段 | excluded |
| CommonDrop.amountDroppedMinimum、amountDroppedMaximum | 不直接创建组件 | 掉落数量规则字段 | excluded |
| CommonDrop.ChainedRules | 不直接创建组件 | 链式规则定义字段 | excluded |
| Player.ApplyDamageToNPC、Player.OnKillNPC | LootAttributionState | Killer、EligibleRecipients 的证据入口 | proposed mapping |
| Player.DropItems、Player.TryDroppingSingleItem | LootResolutionState、LootAttributionState | 结果状态归属，不定义 API | proposed mapping |
| EquipmentLoadout | 不直接创建组件 | 装备装配域排除 | excluded |
| MessageBuffer、NetMessage | 不直接创建组件 | 协议与同步处理排除 | excluded |
| Terraria.IO.WorldFile | 不直接创建组件 | 持久化边界排除 | excluded |
| CoinLossRevengeSystem | LootResolutionState、LootAttributionState | 掉落来源、位置和归因的补充证据 | partial |

说明：

- “partial”表示现有类型或字段只覆盖候选组件的一部分语义，不能据此宣称已经存在 canonical Component。
- “proposed mapping”表示设计上的责任映射，不能据此宣称 Version4 已经按该组件模型实现。
- 同一个原始成员如果同时出现于生命周期和掉落路径，只按其数据责任拆分，不把调用路径当作组件边界。

## 5. Component 定义

以下 8 个类型均为设计提案，统一标记 status: proposed。字段名、字段类型和默认值是组件级设计约束，不是现有 NLTX API 的存在性声明。

### 5.1 EntityLifecycleState

| 属性 | 值 |
|---|---|
| componentId | SLL-COMP-01 |
| status | proposed |
| owner | entity-lifecycle-authority，未决 |
| crossSubsystemOwner | integration-review |
| entityScope | 任何具有独立运行时生命周期的 NPC、Projectile 或可被生命周期管理的实体 |
| cardinality | 每个实体最多一个 |
| purpose | 表示实体从未初始化到终止清理的生命周期状态 |
| lifecycle | Uninitialized → Admitted → Active → Ending → Retired |

字段：

| 字段 | 设计类型 | 默认值 | 说明 |
|---|---|---|---|
| Phase | LifecyclePhase | Uninitialized | 生命周期阶段 |
| IsActive | bool | false | 当前实体是否处于活跃状态 |
| TerminationReason | TerminationReason | None | 终止原因；未终止时为 None |
| PendingCleanup | bool | false | 是否存在待完成的清理标记 |
| Revision | uint64 | 0 | 该组件状态版本；只用于区分状态版本，不是网络包序号 |

不变量：

- Phase = Active 时 IsActive 必须为 true。
- Phase = Uninitialized、Ending 或 Retired 时，IsActive 不得被解释为可参与正常实体活动。
- Phase = Retired 时不得重新解释为同一代实体的 Active；若槽位复用，必须由新的 Generation 区分。
- TerminationReason = None 只允许出现在尚未进入 Ending 的状态，或明确表示尚未记录终止原因。
- PendingCleanup 不能反向改变 Phase；它只是终止后的待清理状态。
- Revision 单调递增；默认值 0 表示没有发生过组件状态变更。

### 5.2 SpawnAdmissionState

| 属性 | 值 |
|---|---|
| componentId | SLL-COMP-02 |
| status | proposed |
| owner | spawn-admission-authority，未决 |
| crossSubsystemOwner | integration-review |
| entityScope | 待接纳的 NPC、Projectile 或 WorldItem 候选实体 |
| cardinality | 每个生成候选最多一个 |
| purpose | 表示生成候选的来源、接纳状态和接纳尝试信息 |
| lifecycle | NotRequested → Pending → Admitted 或 Rejected；Rejected 也可以被标记为 Cancelled |

字段：

| 字段 | 设计类型 | 默认值 | 说明 |
|---|---|---|---|
| Status | SpawnAdmissionStatus | NotRequested | 生成接纳状态 |
| SourceKind | SpawnSourceKind | Unknown | 来源分类，例如 direct、statue、replacement、despawn-replacement 或 external |
| AdmissionKey | string? | None | 用于识别一次接纳意图的稳定键；没有键时为空 |
| Authority | SpawnAuthorityKind | Unknown | 生成接纳权威类别 |
| AttemptCount | uint32 | 0 | 接纳尝试次数 |
| RejectionReason | SpawnRejectionReason | None | 被拒绝时的原因 |

不变量：

- Status = Admitted 时 RejectionReason 必须为 None。
- Status = Rejected 或 Cancelled 时，不能同时解释为已接纳。
- AttemptCount 只能递增，不能通过重置伪造新一轮生成。
- AdmissionKey 如果存在，必须在同一生成意图内保持不变。
- SourceKind 描述生成来源，不得替代 EntityIdentityState 中的 RuntimeEntityId 或 NetworkId。
- 该组件是接纳状态，不保存完整的生成规则，也不保存掉落规则。

### 5.3 EntityIdentityState

| 属性 | 值 |
|---|---|
| componentId | SLL-COMP-03 |
| status | proposed |
| owner | entity-identity-authority，未决 |
| crossSubsystemOwner | integration-review |
| entityScope | 任何需要被运行时、兼容层、网络层或持久化层分别识别的实体 |
| cardinality | 每个实体最多一个 |
| purpose | 明确区分不同 ID 类别，避免以一个 ID 承担所有身份责任 |
| lifecycle | Unassigned → RuntimeAssigned → OptionalCompatibilityAssigned / NetworkAssigned / PersistentAssigned → Retired |

字段：

| 字段 | 设计类型 | 默认值 | ID 分类 |
|---|---|---|---|
| RuntimeEntityId | Guid / EntityId | EntityReference.None | 运行时实体身份；在实体生命周期内唯一 |
| CompatibilitySlot | int? | None | 兼容旧模型的槽位或索引，例如 NPC 槽位 |
| NetworkId | NetworkEntityId? | None | 网络实例身份；不等同于 NPC 类型 NetId |
| PersistentId | Guid? | None | 持久化身份；没有持久化要求时为空 |
| Generation | uint32 | 0 | 兼容槽位或可复用身份的代际编号 |

不变量：

- RuntimeEntityId 是运行时主身份，不能用 CompatibilitySlot、NetworkId 或 PersistentId 替代。
- CompatibilitySlot 为空时，Generation 可以为 0；CompatibilitySlot 被复用时，Generation 必须区分不同实体代次。
- NetworkId 的存在不表示实体已经获得 PersistentId。
- NPC 类型 NetId 不进入 NetworkId；NPC 类型引用由 NpcDefinitionReferenceState 表示。
- PersistentId 的存在不表示实体当前仍为 Active。
- 除 EntityReference.None 外，引用不得使用未定义的零值或隐式魔数表达“无实体”。

### 5.4 EntityRelationState

| 属性 | 值 |
|---|---|
| componentId | SLL-COMP-04 |
| status | proposed |
| owner | entity-relation-authority，未决 |
| crossSubsystemOwner | integration-review |
| entityScope | 需要一个主要父实体、拥有者或关联实体的 NPC、Projectile 或掉落来源实体 |
| cardinality | 每个关系实例一个；本 canonical 类型只表达一个主要关系 |
| purpose | 保存被引用实体及其关系分类，不把关系误当作身份 |
| lifecycle | None → Attached → Detached；引用实体终止时关系可以进入 Detached |

字段：

| 字段 | 设计类型 | 默认值 | 说明 |
|---|---|---|---|
| RelatedEntity | EntityReference | EntityReference.None | 关联实体引用 |
| RelationKind | EntityRelationKind | None | 关系类别，例如 owner、parent、linked-life 或 source |
| ExpectedRevision | uint64 | 0 | 关联实体的期望状态版本；0 表示未设置 |
| AttachedAtTick | uint64? | None | 关系建立时的逻辑时间；未知时为空 |

不变量：

- RelationKind = None 时 RelatedEntity 必须为空。
- RelatedEntity 非空时 RelationKind 不得为 None。
- ExpectedRevision 只能用于检测关联实体版本变化，不得当作实体 ID。
- AttachedAtTick 只描述关系生命周期起点，不是执行顺序或网络时间戳协议。
- 一个实体需要多个独立关系时，不能把不同关系压入一个无类型字段；应由后续设计决定是否采用专门关系组件。本文件不新增这些专门组件。
- Detached 后不得继续把旧 RelatedEntity 当作当前有效关系。

### 5.5 NpcDefinitionReferenceState

| 属性 | 值 |
|---|---|
| componentId | SLL-COMP-05 |
| status | proposed |
| owner | npc-definition-domain |
| entityScope | NPC 实体 |
| cardinality | 每个 NPC 最多一个 |
| purpose | 保存 NPC 类型或定义目录引用，隔离定义身份与实例身份 |
| lifecycle | Unassigned → Assigned → Reclassified；实体 Retired 后不再修改当前代定义引用 |

字段：

| 字段 | 设计类型 | 默认值 | 说明 |
|---|---|---|---|
| TypeId | int | 0 | 当前 NPC 类型标识 |
| NetId | int | 0 | Version4 风格的 NPC 定义或类型网络标识；不表示 NetworkId |
| InitialTypeId | int | 0 | 初始类型标识；没有重分类时与 TypeId 相同 |
| CatalogRevision | uint64 | 0 | 类型目录版本；0 表示未关联目录版本 |

不变量：

- TypeId 和 NetId 是定义域标识，不得充当 EntityIdentityState.RuntimeEntityId。
- InitialTypeId 一旦写入当前实体代，不因运行时重分类而被覆盖。
- CatalogRevision 不能被解释为实体生命周期 Revision。
- 没有有效定义时，TypeId、NetId 和 CatalogRevision 使用明确的默认值 0，而不是借用实例 ID。
- 本组件不包含 NPC 战斗属性、AI 属性或掉落规则。

### 5.6 NpcPopulationLifetimeState

| 属性 | 值 |
|---|---|
| componentId | SLL-COMP-06 |
| status | proposed |
| owner | npc-population-domain，未决 |
| crossSubsystemOwner | integration-review |
| entityScope | 受 NPC 人口、生成上限或寿命约束影响的 NPC 实体 |
| cardinality | 每个受约束 NPC 最多一个；Projectile 不因拥有 timeLeft 而挂载此组件 |
| purpose | 保存 NPC 剩余寿命和人口计数相关状态，隔离通用生命周期 |
| lifecycle | NotTracked → Tracked → Expiring → Untracked |

字段：

| 字段 | 设计类型 | 默认值 | 说明 |
|---|---|---|---|
| RemainingTicks | int64 | 0 | 剩余寿命；0 表示没有可用的剩余寿命额度，而不是自动定义终止行为 |
| PopulationSlotCost | uint32 | 0 | 当前实体占用的人口槽位成本 |
| CountsAgainstPopulation | bool | false | 是否计入人口约束 |
| DespawnEncouraged | bool | false | 是否被标记为适合触发寿命结束 |
| PendingDespawnReason | NpcDespawnReason | None | 待处理的 NPC 寿命结束原因 |

不变量：

- RemainingTicks 不得为负数；未知寿命使用显式的未跟踪状态，而不是负数魔数。
- CountsAgainstPopulation = false 时，PopulationSlotCost 必须为 0。
- PopulationSlotCost 不是 EntityIdentityState.CompatibilitySlot，也不是 NetworkId。
- DespawnEncouraged 只是状态提示，不等同于 EntityLifecycleState.Phase = Ending。
- PendingDespawnReason 只表达待处理原因，不覆盖最终 TerminationReason。
- Projectile 的 timeLeft 不自动转换为 NPC 人口状态。

### 5.7 LootResolutionState

| 属性 | 值 |
|---|---|
| componentId | SLL-COMP-07 |
| status | proposed |
| owner | loot-resolution-domain，未决 |
| crossSubsystemOwner | integration-review |
| entityScope | 作为掉落来源的 NPC、Projectile 或其他可掉落实体；不挂在每一个生成出的 WorldItem 上 |
| cardinality | 每个掉落来源实体最多一个 |
| purpose | 表示掉落规则是否已经针对该来源完成解析，以及解析结果是否已经提交 |
| lifecycle | Unresolved → Resolving → Resolved → Committed；也可以进入 Skipped 或 Failed |

字段：

| 字段 | 设计类型 | 默认值 | 说明 |
|---|---|---|---|
| LootTableId | string? | None | 掉落规则目录引用 |
| SourceKind | LootSourceKind | Unknown | 掉落来源类别 |
| HasResolvedLoot | bool | false | 是否已完成本来源的掉落解析 |
| ResolutionSequence | uint64 | 0 | 同一来源的解析序列 |
| ResolvedAtTick | uint64? | None | 完成解析的逻辑时间；未完成时为空 |
| ResolutionKey | string? | None | 用于识别一次解析意图的稳定键 |
| CommitState | LootCommitState | Unresolved | 解析结果的提交状态 |

不变量：

- HasResolvedLoot = false 时，CommitState 不得为 Committed。
- CommitState = Committed 时，HasResolvedLoot 必须为 true，ResolutionSequence 必须大于 0。
- ResolutionSequence 只能递增；重复解析不能静默覆盖既有序列。
- ResolutionKey 如果存在，必须在同一次解析与提交链路中保持稳定。
- 本组件保存解析状态，不保存 CommonDrop 的概率字段，也不保存 WorldItem 的完整实例状态。
- LootResolutionState 不包含 Killer 或 EligibleRecipients；归因统一由 LootAttributionState 表达。

### 5.8 LootAttributionState

| 属性 | 值 |
|---|---|
| componentId | SLL-COMP-08 |
| status | proposed |
| owner | loot-attribution-domain，未决 |
| crossSubsystemOwner | integration-review |
| entityScope | 作为掉落来源的 NPC、Projectile 或其他奖励来源实体 |
| cardinality | 每个掉落来源实体最多一个 |
| purpose | 保存掉落来源的责任归属和空间归属，供后续奖励结果解释 |
| lifecycle | Unattributed → Attributed → Finalized；也可以保留为 Unknown |

字段：

| 字段 | 设计类型 | 默认值 | 说明 |
|---|---|---|---|
| Killer | EntityReference | EntityReference.None | 主要击杀者或触发者；未知时为空 |
| EligibleRecipients | Set<EntityReference> | empty | 有资格接收奖励的实体集合 |
| LuckOwner | EntityReference | EntityReference.None | 幸运或掉落加成的归属实体；未知时为空 |
| SourcePosition | WorldPosition | WorldPosition.Origin | 掉落来源位置 |
| AttributionRevision | uint64 | 0 | 归因状态版本 |

不变量：

- EligibleRecipients 内的引用必须去重，且不能包含 EntityReference.None。
- Killer、LuckOwner 可以为空；空值表示未知或不适用，不得自动推断为世界实体。
- LuckOwner 不得被隐式替换为 Killer；两者是不同的归因类别。
- SourcePosition 是掉落来源位置快照，不是 WorldItem 的当前移动位置。
- AttributionRevision 只表示归因数据版本，不表示 LootResolutionState.ResolutionSequence。
- 归因可以在掉落解析之前存在，但不能被解释为掉落已经解析或已经提交。

## 6. Entity 与 Component 组合

组合只描述“某类实体可以拥有的组件集合”。它不规定处理步骤、调度顺序、消息顺序或组件更新顺序。

| 实体语义 | 组件组合 | 组合边界 |
|---|---|---|
| 生成候选 NPC | SpawnAdmissionState + EntityIdentityState + NpcDefinitionReferenceState + EntityLifecycleState | 接纳、身份、定义引用和基础生命周期分别保留 |
| 活跃 NPC | EntityLifecycleState + EntityIdentityState + NpcDefinitionReferenceState + NpcPopulationLifetimeState | NPC 人口寿命不替代通用生命周期 |
| 具有关联实体的 NPC | 活跃 NPC 组合 + EntityRelationState | realLife 或其他主要关系需要显式关系类别 |
| 生成候选 Projectile | SpawnAdmissionState + EntityIdentityState + EntityLifecycleState | 不因 NewProjectile 语义而复用 NPC 定义或人口组件 |
| 具拥有者的 Projectile | 生成候选 Projectile 组合 + EntityRelationState | owner 是关系，不是 Projectile 的实例身份 |
| 可掉落来源 NPC | NPC 组合 + LootAttributionState + LootResolutionState | 归因与解析状态独立；来源组件挂在来源实体 |
| 可掉落来源 Projectile | Projectile 组合 + LootAttributionState + LootResolutionState | Projectile 的终止寿命不自动变成 NPC 人口寿命 |
| WorldItem | 现有 WorldItemComponent + WorldItemReservationComponent + ItemInstanceComponent；按需引用掉落来源 | WorldItem 表示物品实例与世界状态，不承接来源实体的解析状态 |
| 不具掉落来源语义的实体 | 不挂载 LootResolutionState 或 LootAttributionState | 组件是否存在由实体责任决定，不由类型名称强制决定 |

组合约束：

- EntityLifecycleState、EntityIdentityState 和实体的主要定义引用可以共同出现，但三者职责不能合并。
- EntityRelationState 的存在依赖于确实需要表达一个主要关系；没有关系时保持缺省或不挂载，不能使用虚假的世界实体引用。
- LootResolutionState 与 LootAttributionState 必须分别表达“解析是否完成”和“归因是谁/在哪里”。
- NpcPopulationLifetimeState 只服务 NPC 人口寿命语义，不作为所有具有 RemainingTicks 的实体的通用容器。
- 现有 WorldItem 类型继续承担物品实例、世界位置和预留相关语义；本轮不把它改造成掉落来源实体的容器。
- 同名的 src 与 dome 组件若字段不同，仍视为 existing/partial 覆盖，不在本文件中假定二者已经可以无差别组合。

## 7. 组件拆分与合并决策

### 7.1 必须拆分的责任

| 拆分边界 | 决策 | 原因 |
|---|---|---|
| EntityLifecycleState / SpawnAdmissionState | 拆分 | 生成候选是否被接纳与实体当前是否 Active 是两个状态轴；拒绝的候选不能伪装成已活跃实体 |
| EntityIdentityState / EntityRelationState | 拆分 | ID 是“我是谁”，关系是“我关联谁”；owner、parent 和 linked-life 不能成为实例身份 |
| EntityIdentityState / NpcDefinitionReferenceState | 拆分 | NPC 类型 NetId 是定义标识，不是 NPC 实例 NetworkId 或 RuntimeEntityId |
| EntityLifecycleState / NpcPopulationLifetimeState | 拆分 | 通用终止阶段和 NPC 人口寿命、人口槽位成本具有不同实体范围 |
| LootResolutionState / LootAttributionState | 拆分 | 掉落是否解析完成与奖励归属、击杀者、位置是不同的事实集合 |
| LootResolutionState / WorldItemComponent | 拆分 | 来源实体的掉落解析与生成后的物品实例具有不同 owner 和生命周期 |
| 权威状态 / 网络 dirty 状态 | 拆分 | netUpdate、spawnNeedsSyncing、NpcReplicationDirtyState 等同步信息不应污染权威生命周期事实 |

### 7.2 不合并的类型族

- 不创建一个包含 NPC、Projectile、WorldItem 全部字段的 EntityState 或 SpawnAndLootState。
- 不将 NPC 的生命值、类型、人口寿命、网络同步和掉落归因合并到单个 NPC Component。
- 不将 Projectile 的 owner、identity 和 timeLeft 作为三个无类型标量塞入通用掉落组件。
- 不提出 ReplicationProjectionState；网络复制字段继续按现有/部分现有类型记录，不能借由本轮设计制造新的 Projection 结构。
- 不把 WorldPosition、WorldVector 等值对象升级为承载实体生命周期的组件。
- 不把 CommonDrop 的规则定义字段放入 LootResolutionState；规则定义和一次实体解析结果分别归属不同层次。

### 7.3 允许的语义归一化

- NPC.active 与 Projectile.active 可以在语义上归入 EntityLifecycleState.IsActive，但现有专属组件只标记 partial。
- NPC.realLife 与 Projectile.owner 可以在语义上使用 EntityRelationState，但 RelationKind 必须区分 linked-life 与 owner。
- NPC.timeLeft 可以归入 NpcPopulationLifetimeState.RemainingTicks；Projectile.timeLeft 仅作为通用终止寿命的证据，不自动获得 NPC 人口语义。
- NPC.netID 可以归入 NpcDefinitionReferenceState.NetId；Projectile.identity 可以归入 EntityIdentityState.NetworkId；这两个字段不能合并成一个未分类 ID。

## 8. 不单独创建 Component 的对象

| 对象或字段 | 不单独创建 Component 的理由 |
|---|---|
| life、lifeMax | 它们属于战斗数值责任；本轮只记录生命周期和掉落边界，不设计战斗状态组件 |
| CanBeReplacedByOtherNPCs | 它更接近生成/替换策略输入，不是稳定的实体事实；本轮不增加策略组件 |
| netUpdate、spawnNeedsSyncing | 它们是网络同步需求或 dirty 语义；本轮不把协议状态混入权威组件 |
| NPC.NetId 以外的 CommonDrop 字段 | itemId、概率和数量范围是规则定义，不是来源实体的运行时解析状态 |
| CommonDrop.ChainedRules | 链式规则关系属于规则定义；不在本轮创建规则图组件 |
| NewNPC、NewProjectile、Kill、checkDead、NPCLoot | 这些是行为入口或过程区域，不是数据组件 |
| DropItems、TryDroppingSingleItem | 它们是物品掉落行为入口；其结果只映射到解析与归因状态 |
| EquipmentLoadout | 它是装备装配责任，与生成生命周期和掉落来源组件没有足够稳定的边界联系 |
| MessageBuffer、NetMessage | 它们表达协议处理，不是实体权威数据组件 |
| Terraria.IO.WorldFile | 它表达存档边界，不是本轮的持久化身份或快照组件 |
| CoinLossRevengeSystem 的处理逻辑 | 它提供掉落与归因证据，但处理逻辑本身不被改写成 Component |
| WorldPosition、WorldVector | 它们是位置/向量值类型；LootAttributionState 可以持有 SourcePosition，但不因此创建通用位置组件 |
| WorldItem 的完整物品实例字段 | 现有 ItemInstanceComponent 与 WorldItemComponent 已承接物品实例、世界表示和位置语义，本轮不复制它们 |
| 客户端表现字段 | 表现状态不属于权威 spawn、lifecycle 或 loot Component，本轮不新增表现组件 |

## 9. 当前 NLTX 组件覆盖

下表只说明当前仓库直接核对到的类型与 proposed canonical Component 的关系。existing 表示现有类型；partial 表示现有类型仅覆盖候选语义的一部分。它们都不表示已经完成迁移或行为等价。

| 当前 NLTX 类型 | 状态 | 对应 canonical Component | 覆盖判断 |
|---|---|---|---|
| NpcLifecycleComponent | partial | EntityLifecycleState | 有 NPC 生命周期语义，但尚未证明覆盖通用实体阶段、终止原因、清理标记和 Revision |
| NpcLifetimeComponent | partial | NpcPopulationLifetimeState | 有 NPC 寿命语义，但尚未证明覆盖人口槽位成本、计数标志和待终止原因 |
| NpcEntityIdentityComponent | partial | EntityIdentityState | 有 NPC 实例身份相关语义，但 ID 类别和代际边界仍需统一 |
| NpcInstanceId | partial | EntityIdentityState.RuntimeEntityId 或兼容身份 | 现有名称不能单独证明其属于运行时身份还是兼容身份 |
| NpcSlot | partial | EntityIdentityState.CompatibilitySlot | 覆盖兼容槽位候选，但代际和复用不变量未闭合 |
| NpcNetId | partial | EntityIdentityState.NetworkId 或 NpcDefinitionReferenceState.NetId | 需要明确它是实例网络身份还是 NPC 定义身份 |
| NpcDefinitionReferenceComponent | partial | NpcDefinitionReferenceState | 已有定义引用候选，但 TypeId、InitialTypeId 和 CatalogRevision 契约未闭合 |
| NpcParentRelationComponent | partial | EntityRelationState | 覆盖 parent 关系候选，但与 realLife 等关系的 cardinality 和 owner 未闭合 |
| NpcReplicationFlags | existing | 不作为本轮 canonical Component | 保留为现有复制相关表示；本轮不重新定义 |
| NpcReplicationDirtyState | existing | 不作为本轮 canonical Component | 保留 dirty 语义；本轮不并入 EntityLifecycleState |
| NpcClientReplicationState | existing | 不作为本轮 canonical Component | 保留客户端复制语义；本轮不提出 Projection 组件 |
| ProjectileLifetimeComponent | partial | EntityLifecycleState | 覆盖 Projectile timeLeft/end reason 的一部分，不覆盖通用生命周期全部字段 |
| ProjectileOwnerComponent | partial | EntityRelationState | 覆盖 owner 关系候选，但不能证明支持统一关系分类与版本检查 |
| LootSourceComponent | partial | LootResolutionState | 覆盖来源类别候选，但没有完整的解析序列、键和提交状态契约 |
| LootAttributionComponent | partial | LootAttributionState | 覆盖部分归因候选，但 recipients、luck、位置和 Revision 语义需统一 |
| WorldItemComponent | existing | 不替换为本轮 canonical Component | 保留 WorldItem 的世界实体/物品表示语义 |
| WorldItemReservationComponent | existing | 不替换为本轮 canonical Component | 保留物品预留语义，不将预留等同于掉落解析提交 |
| ItemInstanceComponent | existing | 不替换为本轮 canonical Component | 保留物品实例语义，不将物品实例与掉落来源合并 |
| Relationships.EntityReference | existing | 被 EntityIdentityState、EntityRelationState、LootAttributionState 作为引用值使用 | 是引用基础值，不是本轮新增的通用关系 Component |
| Items.WorldPosition | existing | 被 LootAttributionState.SourcePosition 使用 | 是位置值，不是本轮新增的通用位置 Component |
| Items.WorldVector | existing | 不作为本轮 canonical Component | 是向量值，当前设计不扩大其责任 |

并行版本说明：

- dome/src/Terraria.Dome.Simulation/Npc/Components/NpcLifecycleComponent.cs、dome/src/Terraria.Dome.Simulation/Items/WorldItemComponent.cs、dome/src/Terraria.Dome.Simulation/Components/Projectile/ProjectileLifetimeComponent.cs 和 dome/src/Terraria.Dome.Simulation/Components/Projectile/ProjectileOwnerComponent.cs 存在字段差异。
- 这些差异使当前覆盖只能标为 partial 或 existing；本文件不选择某一侧字段作为已经统一的事实。
- proposed canonical Component 不是对现有类型的改名声明，也不是对 dome 与 src 的自动合并声明。

## 10. 组件级 evidence-gap

以下 8 个 gap 分别对应 8 个 proposed canonical Component。它们是设计评审待补证据，不是实现缺陷结论。

| Gap ID | Component | 当前证据 | 缺失证据或未决语义 | 状态 |
|---|---|---|---|---|
| BD-COMP-01 | EntityLifecycleState | NPC.active、Projectile.active、Kill、checkDead 提供活跃和终止路径证据 | 尚无统一证据证明 Uninitialized、Admitted、Active、Ending、Retired、PendingCleanup 和 Revision 的全实体契约 | open |
| BD-COMP-02 | SpawnAdmissionState | SpawnedFromStatue、CanBeReplacedByOtherNPCs、NewNPC、NewProjectile 提供来源与生成入口证据 | 尚无当前 NLTX 统一的接纳状态、Authority、AdmissionKey、重试和拒绝原因契约 | open |
| BD-COMP-03 | EntityIdentityState | NPC 的 NpcInstanceId、NpcSlot、NpcNetId 与 Projectile.identity 提供多种身份证据 | 尚无证据证明 RuntimeEntityId、CompatibilitySlot、NetworkId、PersistentId 和 Generation 的唯一性、复用和持久化边界 | open |
| BD-COMP-04 | EntityRelationState | NPC.realLife、NpcParentRelationComponent 和 Projectile.owner 提供关系证据 | 尚无统一的 RelationKind 枚举、单主要关系 cardinality、断开条件和 ExpectedRevision 契约 | open |
| BD-COMP-05 | NpcDefinitionReferenceState | NPC.netID 与 NpcDefinitionReferenceComponent 提供定义引用证据 | 尚无目录版本、重分类、初始类型保留和 NetId 与实例网络身份区分的完整契约 | open |
| BD-COMP-06 | NpcPopulationLifetimeState | NPC.timeLeft、NpcLifetimeComponent、生成/替换相关证据提供寿命边界 | 尚无证据证明人口槽位成本、是否计数、剩余寿命为 0 的含义和待终止原因的统一语义；Projectile timeLeft 不能补齐该证据 | open |
| BD-COMP-07 | LootResolutionState | NPCLoot、DropItems、CommonDrop 和 LootSourceComponent 提供规则与解析入口证据 | 尚无一次解析的幂等键、序列、解析完成与提交状态之间的统一事实模型 | open |
| BD-COMP-08 | LootAttributionState | ApplyDamageToNPC、OnKillNPC、CoinLossRevengeSystem 与 LootAttributionComponent 提供击杀者、来源和位置证据 | 尚无 EligibleRecipients、LuckOwner、未知归因、位置快照和 AttributionRevision 的统一契约 | open |

证据边界：

- open gap 不通过推测补齐。
- 公开 tModLoader 和 Space Station 14 资料只支持责任拆分的结构粒度，不能关闭 NLTX 的实现与 owner gap。
- dome 与 src 的并行类型字段差异被记录为覆盖不完整，不通过选择其中一个版本来伪造一致证据。
- 本文件没有因为 verificationStatus: not-run 而把设计候选升级为已验证类型。

## 11. 未决组件 owner

本节只登记 owner 决策，不定义 owner 的处理实现。以下 7 项都属于跨子系统边界，统一标记 crossSubsystemOwner: integration-review。

| Owner Gap ID | Component | 候选 owner | 未决边界 | crossSubsystemOwner | 状态 |
|---|---|---|---|---|---|
| BD-OWNER-01 | EntityLifecycleState | entity-lifecycle-authority | 实体生命周期事实、终止原因和清理待决状态由实体域持有，还是由各类型域分别持有 | integration-review | open |
| BD-OWNER-02 | SpawnAdmissionState | spawn-admission-authority | NPC 生成、Projectile 生成、WorldItem 生成和替换接纳是否共享同一接纳 authority | integration-review | open |
| BD-OWNER-03 | EntityIdentityState | entity-identity-authority | 运行时身份、兼容槽位、实例网络身份和持久化身份的分配与复用边界 | integration-review | open |
| BD-OWNER-04 | EntityRelationState | entity-relation-authority | parent、owner、linked-life 和 source 关系由实体域、NPC 域、Projectile 域还是掉落域维护 | integration-review | open |
| BD-OWNER-05 | NpcPopulationLifetimeState | npc-population-domain | NPC 人口约束与通用生命周期之间的写入边界、槽位成本 owner 和终止原因 owner | integration-review | open |
| BD-OWNER-06 | LootResolutionState | loot-resolution-domain | 掉落规则解析、来源生命周期和 WorldItem 生成承接之间的事实归属 | integration-review | open |
| BD-OWNER-07 | LootAttributionState | loot-attribution-domain | 战斗结果归因、奖励接收资格、幸运归属和来源位置之间的跨域归属 | integration-review | open |

已知的候选 owner：

- NpcDefinitionReferenceState 的候选 owner 是 npc-definition-domain；它仍需保持与 EntityIdentityState 的定义身份/实例身份边界。
- WorldItemComponent、WorldItemReservationComponent 和 ItemInstanceComponent 的现有 owner 不因本文件而改变。
- “候选 owner”不等于最终 owner；最终确认前仍需保留 proposed 和 decision-required 状态。

## 12. 最终 Component 清单

| componentId | Component | status | entityScope | 核心责任 | 当前 NLTX 覆盖 |
|---|---|---|---|---|---|
| SLL-COMP-01 | EntityLifecycleState | proposed | 通用运行时实体 | 生命周期阶段、活跃性、终止和待清理 | NpcLifecycleComponent、ProjectileLifetimeComponent 为 partial |
| SLL-COMP-02 | SpawnAdmissionState | proposed | NPC、Projectile 或 WorldItem 生成候选 | 来源、接纳状态、接纳 authority、尝试和拒绝原因 | 当前无统一 canonical 覆盖 |
| SLL-COMP-03 | EntityIdentityState | proposed | 需要多类身份的实体 | RuntimeEntityId、兼容槽位、NetworkId、PersistentId、Generation | NPC identity 类型与 Projectile.identity 为 partial |
| SLL-COMP-04 | EntityRelationState | proposed | 具有主要关联实体的实体 | 关联引用、关系类别、期望版本和关系起点 | NpcParentRelationComponent、ProjectileOwnerComponent 为 partial |
| SLL-COMP-05 | NpcDefinitionReferenceState | proposed | NPC | 类型、初始类型、定义 NetId 和目录版本 | NpcDefinitionReferenceComponent 为 partial |
| SLL-COMP-06 | NpcPopulationLifetimeState | proposed | 受人口约束的 NPC | RemainingTicks、人口槽位和待终止原因 | NpcLifetimeComponent 为 partial |
| SLL-COMP-07 | LootResolutionState | proposed | 掉落来源实体 | 掉落解析、序列、解析键和提交状态 | LootSourceComponent 为 partial |
| SLL-COMP-08 | LootAttributionState | proposed | 掉落来源实体 | 击杀者、接收对象、幸运归属和来源位置 | LootAttributionComponent 为 partial |

最终清单约束：

- 8 个 Component 全部是 proposed canonical Component。
- existing/partial 类型只作为当前覆盖记录，不被改写成新建项。
- 本轮不新增 ReplicationProjectionState，也不新增通用 WorldItem、ItemInstance 或位置组件。
- 8 个 Component 的 owner 尚未全部闭合；其中 7 个跨子系统候选已显式标记 crossSubsystemOwner: integration-review。
- designStatus 继续保持 decision-required，直到 owner gap、ID 分类 gap 和组件级 evidence-gap 被独立关闭。

## 13. 最终声明

本文件是 Component-only Design，唯一交付物是组件级数据设计候选。

本文件只定义 Component 的责任边界、字段、默认值、不变量、生命周期语义、实体范围、组合关系、ID 分类和 owner 状态。它不定义 System、Query、Command、Event、Adapter、Projection、调度顺序、运行时实现、测试计划或迁移计划。

SLL-COMP-01 至 SLL-COMP-08 是 proposed Component 设计提案，不是代码已创建的声明。当前 existing/partial 映射不表示已经迁移、不表示行为等价、不表示 src 与 dome 已统一，也不表示验证已经通过。由于存在跨子系统 owner、重复组件字段和 ID 类型缺口，designStatus 保持 decision-required；evidenceStatus 为 partial；nltxStatus 为 missing；verificationStatus 为 not-run。

本轮未启动子代理，未修改源码、测试、项目文件、研究报告或其他设计文件，未运行构建或测试。
