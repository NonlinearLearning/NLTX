# ProjectileSimulation Component Design

## 1. 设计元数据

~~~text
subsystemId: ProjectileSimulation
taskNumber: 13
sourceReport: D:\TRbackup\NLTX\docs\research\2026-09-05-version4-projectile-simulation-public-decomposition.md
outputDesignPath: D:\TRbackup\NLTX\docs\design\2026-09-05-version4-projectile-simulation-component-design.md
designScope: component-only
designStatus: candidate
evidenceStatus: partial
nltxStatus: partial
verificationStatus: not-run
selectionMethod: current-session-explicit-report
~~~

本文件只从当前会话明确提供的研究报告整理 Component 组成。designStatus: candidate 的原因是 owner reference、持久化标识、raw AI 兼容状态、fishing 边界和当前宽 Definition 的拆分仍未完成整合裁决。

Component 数量：18。除标记型能力 Component 外，所有字段都必须有明确的实体范围、生命周期、状态分类和不变量；无法确认的类型或 owner 不以猜测补齐。

## 2. 设计范围与排除范围

本设计的范围是 Projectile 实体上可组合的状态列及其状态所有权：

- 投射物身份、owner reference、槽位兼容字段和 owner-scoped identity 的分离；
- type/behavior key 等定义资料与实例运行状态的分离；
- raw ai[3]、localAI[3]、旋转、方向和子步兼容状态；
- lifetime、damage payload、penetration、hit immunity 和碰撞策略；
- 网络 dirty/节流/section skip 的实例状态，但不把协议消息放入 Component；
- 来源元数据、trail 缓存和 sentry、minion、trap、bobber、counterweight 能力标记；
- Entity 与 Component 的必选/可选组合，以及 entity ID、网络 ID、外部 ID 和持久化 ID 的概念分离；
- 当前 NLTX 组件与候选 Component 的覆盖关系及其 evidence-gap。

本文件排除：运行时调用关系、调度阶段、主循环、网络收发与存档流程、客户端表现流程、测试计划、迁移步骤、代码文件或项目文件创建计划，以及其他子系统的完整状态设计。FishingAndCatchSimulation、LeashedEntitySimulation、CombatAndStatus、SpatialSimulation、SpawnLifecycleAndLoot 和 PlayerGameplay 只在边界 owner 需要时出现，不在此处展开。

## 3. 组件设计依据

### 3.1 Version4 事实

| 事实用途 | 实际证据 | 对组件设计的限制 | 状态 |
|---|---|---|---|
| Projectile 字段和默认成员 | D:\TRbackup\Version4\Terraria\Projectile.cs:90-270；包含 active、owner、type、ai/localAI、timeLeft、damage、penetrate、identity、免疫数组、网络字段、trail 数组和专用标记 | 定义、实例状态、缓存、网络状态和表现字段不能继续合并为巨型 Component | confirmed |
| 默认重置和类型覆盖 | D:\TRbackup\Version4\Terraria\Projectile.cs:444-556，SetDefaults(int Type)；重置 ai/localAI、trail、identity、owner、timeLeft、penetrate 和策略字段 | 默认值须区分 Version4 基线默认和 type-specific 覆盖 | confirmed |
| 槽位和 identity | D:\TRbackup\Version4\Terraria\Main.cs:944-946 声明 Projectile[1001] 与 projectileIdentity；Main.cs:3480-3484 初始化槽位并设置 whoAmI | slot/whoAmI、owner-scoped identity、runtime entity、replication ID 和 persistent ID 不得合并 | confirmed |
| 生成初始化字段 | D:\TRbackup\Version4\Terraria\Projectile.cs:10227-10343，FindOldestProjectile 与 NewProjectile；写入 defaults、position、velocity、owner、damage、identity、UUID 和来源字段 | Identity、来源和初始轨迹可共同初始化，但不是同一个长期 owner | confirmed |
| 轨迹和行为字段 | D:\TRbackup\Version4\Terraria\Projectile.cs:14693-14705、18706-18716；读取 extraUpdates、numUpdates、aiStyle、ai/localAI | raw AI 是兼容状态；行为 key 不是独立实体状态根；通用 position/velocity 不复制入 Projectile Component | confirmed |
| 碰撞和移动策略 | D:\TRbackup\Version4\Terraria\Projectile.cs:13654 及后续 Colliding；15513 及后续 HandleMovement；读取 trail、tile、fall-through、slope、liquid 和反射策略 | 几何缓存、碰撞策略和外部 Tile/World 状态必须分开 | confirmed |
| 命中、免疫和穿透 | D:\TRbackup\Version4\Terraria\Projectile.cs:11480-11538、11554-11656、11758-11767、11800-12849 | Damage payload、projectile-local immunity、penetration 和目标健康状态不得合并 | confirmed |
| lifetime、trail 和网络状态 | D:\TRbackup\Version4\Terraria\Projectile.cs:15199-15275；同一字段区域同时出现 trail、timeLeft、penetrate、netUpdate、netSpam 和 section resend 标志 | trail/cache 与权威 lifetime、网络状态拆开；网络 Component 不包含消息结构 | confirmed |
| 销毁清理和子关系 | D:\TRbackup\Version4\Terraria\Projectile.cs:46425-46654，Kill()；清理 owner+identity 反查、取消 channel，并存在 child/effect 分支 | active=false 不能代表全部生命周期；本文件只记录 Component 状态，不宣布销毁事务 owner | confirmed |
| Fishing 边界 | Version4 Projectile.cs:34073-34074 的 bobber 方法为空，47779-47786 仍有 bobber 入口；完整参考 D:\TRbackup\无任何删减通过编译\Terraria\Projectile.cs:51236-51552 仅补充同文件缺口 | Bobber 只携带 carrier/context；捕获结果和 Item/NPC 事务不归属本设计 | partial |
| LeashedEntity 边界 | D:\TRbackup\Version4\Terraria.GameContent\LeashedEntity.cs:44-306；registry、section、active/update/remove 和独立网络模块并列存在，233 和 290 当前为空 | Leashed anchor、section 和其生命周期不放入 Projectile Component | partial |

### 3.2 当前 NLTX 与外部结构参考

| 来源 | 实际证据 | 仅用于本设计的用途 | 状态 |
|---|---|---|---|
| NLTX 根组件 | D:\TRbackup\NLTX\src\Projectile 下 Behavior、Damage、Definition、Direction、Lifetime、Owner、Penetration 组件 | 识别已存在的局部状态和命名，不把现有局部覆盖误写成 Version4 等价 | partial |
| NLTX EntityReference | D:\TRbackup\NLTX\src\Relationships\EntityReference.cs:5-10，EntityReference(Guid, Scope) | 作为 owner/reference 候选；其与 Version4 owner index 和 dome PlayerHandle 的长期关系仍需整合 | confirmed |
| NLTX dome 组件 | D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Components\Projectile 下的 identity、network update、definition、lifetime、penetration、capability 和 source 组件 | 参考当前已经存在的拆分粒度；宽 Definition 只能标为 partial | partial |
| tModLoader 公开边界 | D:\TRbackup\tmodloader-api-docs-stable\index.html 页眉为 tModLoader v2026.07；class_mod_projectile.html#a83a2115efe34d20f142aaaec797d22d1（AI）、#a3a9d755be9e1f31ceecd8b99f53703de（Colliding）、#a1154c0c4a7cf829584f1fa97608ab470（OnKill）、#abed0119b6752f522f1ca54f3efd656f5（SendExtraAI） | 只交叉核对公开行为扩展、碰撞、销毁和额外 AI 状态边界；不替代 Version4 私有实现 | partial |
| Space Station 14 粒度参考 | C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\Weapons\Ranged\Components\TargetedProjectileComponent.cs、Weapons\Misc\GrapplingProjectileComponent.cs、GrapplingProjectileEmbedComponent.cs、Weapons\Marker\DamageMarkerOnCollideComponent.cs | 只参考“按能力组合”“关系字段独立”“命中附加状态独立”的粒度；不复制其名称、代码或领域语义 | partial |

## 4. Version4 成员到 Component 归属表

下表列出 Version4 Projectile 的真实字段、属性和直接状态入口。未纳入权威 Component 表示该成员是派生值、全局表、缓存或表现状态，而不是遗漏。

| 成员 | 声明类型 | 字段含义 | 权威/派生/缓存/快照/兼容 | 生命周期 | proposed Component | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|---|
| active | Projectile | 当前槽位是否有活跃投射物 | 派生/实体生命周期 | 槽位创建至清理 | ProjectileLifetimeComponent 的 IsActive 派生；不复制字段 | confirmed | D:\TRbackup\Version4\Terraria\Projectile.cs:90 |
| owner | Projectile | Version4 owner 数值，默认 sentinel 为 255 | 权威兼容身份 | 创建、更新、清理 | ProjectileIdentityComponent.OwnerReference | confirmed | Projectile.cs:126；SetDefaults:535 |
| identity | Projectile | owner-scoped identity | 权威网络/兼容身份 | 生成分配、销毁清理 | ProjectileIdentityComponent.Identity | confirmed | Projectile.cs:168；NewProjectile:10244-10343 |
| whoAmI | Entity 基类/槽位 | 数组槽位 token | 兼容字段 | 槽位分配、复用、清理 | ProjectileIdentityComponent.SlotIndex | confirmed | Main.cs:3480-3484 |
| projUUID | Projectile | 部分类型使用的 projectile UUID | 兼容/外部 ID | 某些生成关系至销毁 | ProjectileIdentityComponent.ProjectileUuid | confirmed | Projectile.cs:248；NewProjectile:10325-10343 |
| type | Projectile | 投射物定义类型 | 权威定义资料 | 创建时解析，实例存续 | ProjectileDefinitionComponent.ProjectileType | confirmed | Projectile.cs:118；SetDefaults:444-556 |
| aiStyle | Projectile | 旧行为分发键 | 权威定义键/兼容 | 创建后稳定，个别行为读取 | ProjectileDefinitionComponent.BehaviorKey | confirmed | Projectile.cs:136；AI:18706-18716 |
| ai[0]、ai[1]、ai[2] | float[3] | 行为专用共享状态 | 权威兼容状态 | 创建重置、每个子步读写 | ProjectileTrajectoryStateComponent.Ai0..Ai2 | confirmed | Projectile.cs:128；SetDefaults:487 |
| localAI[0]、localAI[1]、localAI[2] | float[3] | 本地行为状态 | 权威兼容状态 | 创建重置、每个子步读写 | ProjectileTrajectoryStateComponent.LocalAi0..LocalAi2 | confirmed | Projectile.cs:130；SetDefaults:488 |
| rotation | Projectile | 轨迹/方向相关旋转 | 权威实例状态 | 创建默认、行为更新 | ProjectileTrajectoryStateComponent.Rotation | confirmed | Projectile.cs:116 |
| spriteDirection | Projectile | 方向兼容值，可能同时影响行为和表现 | 权威兼容状态 | 创建、行为更新 | ProjectileTrajectoryStateComponent.SpriteDirection | partial | Projectile.cs:146 |
| stepSpeed | Projectile | 子步移动速度标量 | 权威实例状态 | 创建默认、行为更新 | ProjectileTrajectoryStateComponent.StepSpeed | confirmed | Projectile.cs:134 |
| numUpdates | Projectile | 当前 extra update 子步计数 | 权威临时状态 | 每主 tick 重置和递减 | ProjectileTrajectoryStateComponent.SubstepCounter | confirmed | Projectile.cs:200,14693-14705 |
| extraUpdates | Projectile | 每主 tick 额外子步数量 | 权威定义资料 | 创建设置，实例存续 | ProjectileDefinitionComponent.ExtraUpdates | confirmed | Projectile.cs:196,338-347 |
| timeLeft | Projectile | 剩余生命 tick | 权威生命周期状态 | 创建初始化、更新递减、终止归零 | ProjectileLifetimeComponent.RemainingTicks | confirmed | Projectile.cs:138,536,15228-15240 |
| damage | Projectile | 当前伤害 | 权威伤害载荷 | 创建初始化、命中前后可变 | ProjectileDamagePayloadComponent.CurrentDamage | confirmed | Projectile.cs:142,11480-12849 |
| originalDamage | Projectile | 原始伤害基线 | 权威伤害载荷 | 创建初始化、实例存续 | ProjectileDamagePayloadComponent.OriginalDamage | confirmed | Projectile.cs:144 |
| knockBack | Projectile | 击退载荷 | 权威伤害载荷 | 创建初始化、命中读取 | ProjectileDamagePayloadComponent.Knockback | confirmed | Projectile.cs:152 |
| armorPenetration | Projectile | 护甲穿透加成 | 权威伤害载荷 | 创建初始化、命中读取 | ProjectileDamagePayloadComponent.ArmorPenetration | confirmed | Projectile.cs:266 |
| bonusCritChance | Projectile | 额外暴击概率 | 权威伤害载荷 | 创建初始化、命中读取 | ProjectileDamagePayloadComponent.BonusCritChance | confirmed | Projectile.cs:268 |
| bonusTagDamage | Projectile | 标签伤害加成 | 权威伤害载荷 | 创建初始化、命中读取 | ProjectileDamagePayloadComponent.BonusTagDamage | confirmed | Projectile.cs:264 |
| tagEffectType | Projectile | 标签效果类型 | 权威伤害元数据 | 创建初始化、命中读取 | ProjectileDamagePayloadComponent.TagEffectType | confirmed | Projectile.cs:262 |
| melee、ranged、magic | Projectile | 伤害类别兼容布尔值 | 派生/兼容 | 从 damage class 设置或读取 | ProjectileDamagePayloadComponent.DamageClass 派生视图 | partial | Projectile.cs:224-228 |
| coldDamage | Projectile | 冷属性伤害标记 | 权威伤害元数据 | 创建初始化、命中读取 | ProjectileDamagePayloadComponent.IsColdDamage | confirmed | Projectile.cs:230 |
| arrow | Projectile | 箭类标记 | 权威伤害/定义元数据 | 创建初始化、实例存续 | ProjectileDamagePayloadComponent.IsArrow | confirmed | Projectile.cs:100 |
| hostileDamageScaling | Projectile | hostile 伤害缩放规则 | 权威伤害规则 | 创建初始化、命中读取 | ProjectileDamagePayloadComponent.HostileDamageScaling | confirmed | Projectile.cs:270 |
| penetrate | Projectile | 剩余穿透次数 | 权威命中状态 | 创建初始化、命中递减、终止 | ProjectilePenetrationStateComponent.RemainingHits | confirmed | Projectile.cs:156,12698-12849 |
| maxPenetrate | Projectile | 最大穿透次数 | 权威命中定义/基线 | 创建初始化、实例存续 | ProjectilePenetrationStateComponent.MaximumHits | confirmed | Projectile.cs:166 |
| numHits | Projectile | 已接受命中数 | 权威统计状态 | 创建、每次接受命中 | ProjectilePenetrationStateComponent.HitCount | confirmed | Projectile.cs:102 |
| stopsDealingDamageAfterPenetrateHits | Projectile | 穿透耗尽后的伤害资格规则 | 权威命中策略 | 创建初始化、命中读取 | ProjectilePenetrationStateComponent.StopsDealingDamageWhenDepleted | confirmed | Projectile.cs:198 |
| localNPCImmunity | int[] | 按 NPC 目标的本地免疫计数 | 权威实例免疫状态 | 创建清零、更新递减、命中写入 | ProjectileHitImmunityStateComponent.LocalNpcImmunityTicks | confirmed | Projectile.cs:158,11554-11592 |
| playerImmune | int[] | 按 Player 目标的免疫计数 | 权威实例免疫状态 | 创建清零、更新递减、命中写入 | ProjectileHitImmunityStateComponent.PlayerImmunityTicks | confirmed | Projectile.cs:220 |
| restrikeDelay | Projectile | 再次命中延迟 | 权威实例免疫状态 | 创建默认、更新递减、命中写入 | ProjectileHitImmunityStateComponent.RestrikeDelayTicks | confirmed | Projectile.cs:192 |
| usesLocalNPCImmunity | Projectile | 是否使用本地 NPC 免疫 | 权威免疫策略 | 创建初始化、命中读取 | ProjectileHitImmunityPolicyComponent.UsesLocalNpcImmunity | confirmed | Projectile.cs:160 |
| usesIDStaticNPCImmunity | Projectile | 是否使用 type/static NPC 免疫 | 权威免疫策略 | 创建初始化、命中读取 | ProjectileHitImmunityPolicyComponent.UsesStaticNpcImmunity | confirmed | Projectile.cs:162 |
| localNPCHitCooldown | Projectile | 本地/owner 免疫冷却默认值 | 权威免疫策略 | 创建初始化、命中读取 | ProjectileHitImmunityPolicyComponent.LocalNpcCooldownTicks | confirmed | Projectile.cs:256 |
| idStaticNPCHitCooldown | Projectile | static NPC 免疫冷却默认值 | 权威免疫策略 | 创建初始化、命中读取 | ProjectileHitImmunityPolicyComponent.StaticNpcCooldownTicks | confirmed | Projectile.cs:258 |
| appliesImmunityTimeOnSingleHits | Projectile | 单次命中是否施加免疫时间 | 权威免疫策略 | 创建初始化、命中读取 | ProjectileHitImmunityPolicyComponent.AppliesOnSingleHit | confirmed | Projectile.cs:164 |
| usesOwnerMeleeHitCD | Projectile | 是否读取 owner melee hit cooldown | 权威免疫策略 | 创建初始化、命中读取 | ProjectileHitImmunityPolicyComponent.UsesOwnerMeleeCooldown | confirmed | Projectile.cs:218,374-380 |
| ownerHitCheck、ownerHitCheckDistance | Projectile | owner self-hit 检查及距离 | 权威碰撞策略 | 创建初始化、命中读取 | ProjectileCollisionPolicyComponent.OwnerHitCheck、OwnerHitCheckDistance | confirmed | Projectile.cs:98,216,11554-11656 |
| tileCollide | Projectile | 是否与 Tile 碰撞 | 权威碰撞策略 | 创建初始化、移动读取 | ProjectileCollisionPolicyComponent.TileCollisionEnabled | confirmed | Projectile.cs:194,15513+ |
| ignoreWater | Projectile | 是否忽略水体影响 | 权威碰撞策略 | 创建初始化、移动读取 | ProjectileCollisionPolicyComponent.IgnoreWater | confirmed | Projectile.cs:202 |
| correctSlopeCollision | Projectile | 是否修正斜坡碰撞 | 权威碰撞策略 | 创建初始化、移动读取 | ProjectileCollisionPolicyComponent.CorrectSlopeCollision | confirmed | Projectile.cs:250,15513+ |
| decidesManualFallThrough | Projectile | 是否允许实例决定 fall-through | 权威碰撞策略 | 创建初始化、移动读取 | ProjectileCollisionPolicyComponent.DecidesManualFallThrough | confirmed | Projectile.cs:252 |
| shouldFallThrough | Projectile | 当前 fall-through 覆盖值 | 权威实例碰撞状态 | 更新、移动读取 | ProjectileCollisionPolicyComponent.ShouldFallThrough | confirmed | Projectile.cs:254,15522-15592 |
| manualDirectionChange | Projectile | 是否由行为手动改变方向 | 权威碰撞/轨迹策略 | 创建初始化、行为读取 | ProjectileCollisionPolicyComponent.ManualDirectionChange | confirmed | Projectile.cs:246 |
| reflected | Projectile | 是否已被反射 | 权威实例碰撞状态 | 命中/碰撞写入、实例存续 | ProjectileGeometryStateComponent.Reflected | confirmed | Projectile.cs:150,11758-11767 |
| scale | Projectile | 投射物缩放 | 权威几何状态 | 创建初始化、几何读取 | ProjectileGeometryStateComponent.Scale | confirmed | Projectile.cs:114 |
| netImportant | Projectile | 不应被普通槽位回收的网络重要标记 | 权威网络状态 | 创建初始化、槽位回收读取 | ProjectileNetworkStateComponent.NetworkImportant | confirmed | Projectile.cs:106,10227-10242 |
| netUpdate、netUpdate2 | Projectile | 两类网络 dirty 标志 | 权威网络状态 | 更新写入、复制读取、清理 | ProjectileNetworkStateComponent.PrimaryUpdatePending、SecondaryUpdatePending | confirmed | Projectile.cs:172-174,15241-15275 |
| netSpam | Projectile | 网络更新节流计数 | 权威网络状态 | 创建、更新、复制读取 | ProjectileNetworkStateComponent.NetSpam | confirmed | Projectile.cs:176 |
| netSyncSkippedForPlayer | bool[] | 按 player 的 section 同步跳过标记 | 权威网络状态 | 创建清零、网络边界更新 | ProjectileNetworkStateComponent.SectionSyncSkippedForPlayer | confirmed | Projectile.cs:178,15271-15275 |
| bobber | Projectile | bobber carrier 标记 | 权威能力标记 | 生成附加、销毁移除 | ProjectileBobberCapabilityComponent | confirmed | Projectile.cs:104,34073-34074 |
| sentry | Projectile | sentry carrier 标记 | 权威能力标记 | 生成附加、销毁移除 | ProjectileSentryCapabilityComponent | confirmed | Projectile.cs:122,314-324 |
| minion、minionSlots、minionPos | Projectile | minion 能力及容量/位置 | 权威能力状态 | 生成附加、owner 生命周期或销毁移除 | ProjectileMinionCapabilityComponent | confirmed | Projectile.cs:186-190,14711-14767 |
| trap | Projectile | trap 能力标记 | 权威能力标记 | 生成附加、销毁移除 | ProjectileTrapCapabilityComponent | confirmed | Projectile.cs:236,11594-11656 |
| counterweight | Projectile | counterweight 能力标记 | 权威能力标记 | 生成附加、销毁移除 | ProjectileCounterweightCapabilityComponent | confirmed | Projectile.cs:112 |
| npcProj | Projectile | NPC/非 Player 来源标记 | 权威来源分类 | 生成初始化、owner 关系存续 | ProjectileSourceMetadataComponent.IsNpcProjectile | confirmed | Projectile.cs:238,362-372 |
| bannerIdToRespondTo | Projectile | banner 响应来源 | 权威来源元数据 | 生成初始化、命中读取 | ProjectileSourceMetadataComponent.BannerIdToRespondTo | confirmed | Projectile.cs:260 |
| miscText | Projectile | spawn/source 附加文本 | 权威来源元数据 | 生成初始化、复制或交付读取 | ProjectileSourceMetadataComponent.MiscText | confirmed | Projectile.cs:222,10244-10343 |
| originatedFromActivableTile | Projectile | 是否来自可激活 Tile | 权威来源元数据 | 生成初始化、销毁/掉落边界读取 | ProjectileSourceMetadataComponent.OriginatedFromActivableTile | confirmed | Projectile.cs:240 |
| noDropItem | Projectile | 禁止掉落 Item 的来源/终止标记 | 权威来源元数据 | 生成初始化、销毁边界读取 | ProjectileSourceMetadataComponent.NoDropItem | confirmed | Projectile.cs:108 |
| MinionSpawnInfo | Projectile | minion 来源的复合资料 | 来源关系/类型 unresolved | 生成初始化、owner 生命周期 | ProjectileSourceMetadataComponent 的复合来源字段，具体类型 unresolved | partial | Projectile.cs:208 |
| oldPos | Vector2[10] | 旧位置轨迹缓存 | 缓存 | 创建清零、每 tick 更新、几何读取 | ProjectileTrailCacheComponent.OldPositions | confirmed | Projectile.cs:180,472-482 |
| oldRot | float[10] | 旧旋转轨迹缓存 | 缓存 | 创建清零、每 tick 更新、几何读取 | ProjectileTrailCacheComponent.OldRotations | confirmed | Projectile.cs:182,472-482 |
| oldSpriteDirection | int[10] | 旧方向轨迹缓存 | 缓存 | 创建初始化、轨迹读取 | ProjectileTrailCacheComponent.OldSpriteDirections | confirmed | Projectile.cs:184 |
| alpha、glowMask、gfxOffY、light | Projectile | 透明度、发光、绘制偏移、光照 | 表现/派生 | 表现生命周期 | 不创建权威 Projectile Component | confirmed | Projectile.cs:120,124,132,170,326-336 |
| drawLayer、usesOwnerLight、hide | Projectile | 绘制层和可见性控制 | 表现状态 | 表现生命周期 | 不创建权威 Projectile Component | confirmed | Projectile.cs:210-214 |
| frameCounter、frame | Projectile | 精灵帧状态 | 表现/临时 | 表现生命周期 | 不创建权威 Projectile Component | confirmed | Projectile.cs:242-244 |
| soundDelay | Projectile | 声音节流 | 表现/临时 | 表现边界生命周期 | 不创建权威 Projectile Component；当前 dome 有 partial 临时组件 | confirmed | Projectile.cs:140 |
| isAPreviewDummy、isAPreviewDisplayDoll | Projectile | 预览/展示对象标记 | 表现/工具状态 | 预览生命周期 | 不创建权威 Projectile Component | confirmed | Projectile.cs:204-206 |
| perIDStaticNPCImmunity | Projectile 静态字段 | 按 type 的共享 NPC 免疫表 | 全局共享状态 | 世界/内容表生命周期 | 不归属单个 Projectile；owner unresolved | confirmed | Projectile.cs:92 |
| WhipPointsForCollision | Projectile | whip 几何临时点列 | 缓存/几何输入 | 特殊几何期间 | ProjectileTrailCacheComponent 的可选几何缓存，非权威轨迹根 | confirmed | Projectile.cs:282,13654+ |
| maxAI、SentryLifeTime、ArrowLifeTime | Projectile 常量/静态 | 容量或定义常量 | 定义常量 | 进程/内容生命周期 | 不创建 Component 字段 | confirmed | Projectile.cs:94-110 |

## 5. Component 定义

以下定义中的 status: proposed 表示目标 Component 尚未按本文件落地；status: partial 表示当前 NLTX 已有同名或近似组件，但其语义覆盖仍不完整。

### 5.1 ProjectileIdentityComponent

#### 职责

保存一个 Projectile 的 owner reference、槽位兼容 token、owner-scoped identity 和可选 Version4 UUID。不同 ID 只在此处并列记录，不互相替代。

~~~text
componentId: BD-COMP-PROJ-01
name: ProjectileIdentityComponent
status: proposed
componentOwner: ProjectileSimulation
crossSubsystemOwner: integration-review
entityScope: 单个 Projectile entity
lifecycle: entity 创建时分配或接收；槽位/identity 释放时清理；runtime entity reference 不写入持久化或协议字段
~~~

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| OwnerReference | EntityReference | EntityReference.None | 权威关系 | reference scope 必须与最终 owner domain 一致；跨域 owner 未裁决 | partial | NLTX src/Relationships/EntityReference.cs:5-10；V4 Projectile.cs:126 |
| SlotIndex | int | -1（未绑定槽位） | 兼容字段 | 绑定槽位时非负；复用后不得推导出持久化身份 | confirmed | V4 Main.cs:944-946,3480-3484 |
| Identity | int | 0（未分配） | 权威 owner-scoped identity | 活跃实例一旦分配必须为正值，并与 OwnerReference 共同作为反查键 | confirmed | V4 Projectile.cs:168,10244-10343 |
| ProjectileUuid | int | -1 | 兼容/外部 ID | 只表示 Version4 projUUID；不得等同 runtime entity、replication 或 persistent ID | confirmed | V4 Projectile.cs:248,10325-10343 |

#### 字段不变量

SlotIndex、Identity、ProjectileUuid 和 runtime entity reference 是四个不同命名空间。跨子系统使用的 EntityReference、owner-kind、identity table key、replication ID 和未来 persistent ID 的最终 owner 为 integration-review。

#### 生命周期

创建时得到 owner 和 identity 的候选值；槽位 token 只在有兼容槽位绑定时存在。销毁时必须能撤销 owner+identity 关系，但撤销细节不在本文件宣布为 Component owner。

#### Entity/World 范围

字段属于单个 Projectile entity。owner+identity 反查表是外部关系索引，不是这个 Component 的数组字段。

#### ID 与关系字段

OwnerReference 是实体关系；SlotIndex 是可复用槽位 token；Identity 是 owner-scoped identity；ProjectileUuid 是 Version4 兼容外部 ID。persistent ID、network replication ID 和 runtime entity ID 不放入此 Component。

#### 当前 NLTX 映射

根 ProjectileOwnerComponent 使用 EntityReference，dome ProjectileNetworkIdentityComponent 使用 PlayerHandle、Identity 和 Guid?；两者均为 partial，不能宣布长期映射。

#### 证据

V4 Main.cs:944-946,3480-3484；V4 Projectile.cs:10227-10343,46425-46443。

### 5.2 ProjectileDefinitionComponent

#### 职责

只保存内容目录解析出的 type、behavior key、默认阵营、extra update 预算、基础定义版本和不可变内容规则。运行时计数、命中计数、网络 dirty 和表现数据不应继续塞入此 Component。

~~~text
componentId: BD-COMP-PROJ-02
name: ProjectileDefinitionComponent
status: partial
componentOwner: ProjectileSimulation 与 ContentCatalog 的边界
crossSubsystemOwner: integration-review
entityScope: 单个 Projectile entity 的定义快照
lifecycle: 生成时从内容目录取值并固定到实例；实例销毁时移除；目录 revision 可用于识别定义来源
~~~

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| ProjectileType | int | 0 | 权威定义资料 | 必须是已解析的 projectile type；具体有效范围由内容目录 owner 保证 | confirmed | V4 Projectile.cs:118；NLTX src/Projectile/ProjectileDefinitionComponent.cs:3-23 |
| BehaviorKey | int | 0 | 权威定义/兼容键 | 只选择行为契约，不承载行为可变状态 | confirmed | V4 Projectile.cs:136,18706-18716 |
| FriendlyDefault | bool | false | 定义默认值 | 只表示生成默认；若运行时可变，不能把它当作唯一当前阵营状态 | partial | V4 Projectile.cs:154,444-556 |
| HostileDefault | bool | false | 定义默认值 | 与 FriendlyDefault 独立；最终互斥/并存规则 unresolved | partial | V4 Projectile.cs:148,444-556 |
| ExtraUpdates | int | 0 | 权威定义资料 | 不得为负；MaxUpdates = ExtraUpdates + 1 | confirmed | V4 Projectile.cs:196,338-347 |
| CatalogRevision | int | 0 | 定义来源元数据 | revision 只标识内容版本，不改变实例身份 | partial | NLTX src/Projectile/ProjectileDefinitionComponent.cs:3-23 |
| NoEnchantments | bool | false | 定义规则 | 只控制伤害内容是否允许附魔；表现禁用字段另行排除 | confirmed | V4 Projectile.cs:232 |

#### 字段不变量

Definition Component 不拥有 RemainingTicks、HitCount、AI 数组、免疫计数或网络 dirty。FriendlyDefault/HostileDefault 是否还需要一个独立可变阵营 Component 是 BD-COMP-08 未决项。

#### 生命周期

由内容目录解析后随 Projectile 创建；类型覆盖只影响同一实例的定义快照，不应在更新中隐式重建整个 Component。

#### Entity/World 范围

字段属于单个 Projectile 的定义快照；内容目录、type registry 和 catalog revision 是外部共享资料，不嵌入实体 Component。

#### ID 与关系字段

ProjectileType 和 CatalogRevision 是内容/外部定义标识，不是 Entity ID 或网络 ID。

#### 当前 NLTX 映射

NLTX src/Projectile/ProjectileDefinitionComponent.cs:3-23 已有精简定义组件；dome 同名组件当前把 damage、碰撞、免疫、能力和 child spawn 等大量状态混在一个 record 中，整体标记 partial。

#### 证据

V4 Projectile.cs:444-556；NLTX dome Components/Projectile/ProjectileDefinitionComponent.cs:5-173。

### 5.3 ProjectileTrajectoryStateComponent

#### 职责

保存行为兼容所需的 raw AI/localAI、旋转、方向和子步临时状态。通用位置和速度由共享实体运动状态拥有，本 Component 不复制它们。

~~~text
componentId: BD-COMP-PROJ-03
name: ProjectileTrajectoryStateComponent
status: proposed
componentOwner: ProjectileSimulation
crossSubsystemOwner: integration-review
entityScope: 单个 Projectile entity
lifecycle: 生成时清零/初始化；每个行为子步更新；销毁时移除；raw 字段在 typed 行为覆盖完整前保留
~~~

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| Ai0、Ai1、Ai2 | float | 0.0f | 权威兼容状态 | 值必须为 finite；每个 behavior key 对槽位的解释必须稳定 | confirmed | V4 Projectile.cs:128,487 |
| LocalAi0、LocalAi1、LocalAi2 | float | 0.0f | 权威兼容状态 | 值必须为 finite；不得和网络快照字段混为另一份 authority | confirmed | V4 Projectile.cs:130,488 |
| Rotation | float | 0.0f | 权威实例状态 | 必须为 finite；单位由模拟数学边界统一 | confirmed | V4 Projectile.cs:116 |
| SpriteDirection | int | 1 | 兼容状态 | 只允许约定的方向值；表现读取不能反向改变轨迹 | partial | V4 Projectile.cs:146 |
| StepSpeed | float | 1.0f | 权威实例状态 | 必须为 finite 且不小于 0 | confirmed | V4 Projectile.cs:134 |
| SubstepCounter | int | 0 | 权威临时状态 | 不得为负；不能用它代替 Definition 的 ExtraUpdates | confirmed | V4 Projectile.cs:200,14693-14705 |

#### 字段不变量

raw AI 与 behavior key 同时存在是兼容性设计，不是重复镜像；只有当每个使用者的 typed 状态契约闭合后才可以重新评估删除。LocationComponent、VelocityComponent 和通用 ColliderComponent 不属于此 Component。

#### 生命周期

SetDefaults 等价初始化应清零 AI/localAI；行为更新修改 raw state；销毁时不保留为持久化 ID 或网络身份。

#### Entity/World 范围

单个 Projectile entity；空间位置、速度、Tile 和 World 状态是外部输入或共享状态。

#### ID 与关系字段

无身份字段。行为 key 在 Definition Component，AI 值在本 Component，二者不可合并成 type-specific entity ID。

#### 当前 NLTX 映射

根 ProjectileBehaviorComponent 只有 Style、State0..State3、LocalState0..LocalState1；dome Components/AI/ProjectileBehaviorComponent.cs:3-61 已有 typed ProjectileBehaviorState 和三个 local AI，但命名/槽位数量与 Version4 仍 partial。

#### 证据

V4 Projectile.cs:128-136,14693-14705,18706-18716。

### 5.4 ProjectileLifetimeComponent

#### 职责

保存剩余生命周期和终止原因。实体是否活跃是由实体存在和 lifetime 派生，不再复制一个独立 active 权威布尔值。

~~~text
componentId: BD-COMP-PROJ-04
name: ProjectileLifetimeComponent
status: partial
componentOwner: ProjectileSimulation
crossSubsystemOwner: integration-review
entityScope: 单个 Projectile entity
lifecycle: 创建时由定义/类型初始化；更新时递减；到期写入终止原因；结构清理由最终生命周期 owner 裁决
~~~

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| RemainingTicks | int | 3600（Version4 基线；type 可覆盖） | 权威生命周期状态 | 不得小于 0；为 0 时 IsExpired 为真 | confirmed | V4 Projectile.cs:138,536；NLTX ProjectileLifetimeComponent.cs:3-12 |
| EndReason | ProjectileEndReason | None | 权威终止元数据 | 未终止时为 None；终止原因只能由生命周期 owner 写入 | partial | NLTX ProjectileLifetimeComponent.cs:3-12；ProjectileEndReason.cs:3-10 |

#### 字段不变量

RemainingTicks 为 0 只表示生命周期耗尽或已进入终止状态，不表示 child、drop、channel 或网络 tombstone 已完成。active 是派生/实体生命周期表达，不复制入字段表。

#### 生命周期

生成初始化、每次 tick 递减、命中/碰撞等终止原因写入；清理阶段只移除 Component，不在此 Component 保存外部副作用结果。

#### Entity/World 范围

单个 Projectile entity。世界时间、持久化时间和网络 tombstone retention 不属于此 Component。

#### ID 与关系字段

无 ID 和关系字段。

#### 当前 NLTX 映射

根组件已有 RemainingTicks 与 EndReason；dome 组件已有 RemainingTicks、IsActive 和 IsExpired，但没有完整终止事务边界，故为 partial。

#### 证据

V4 Projectile.cs:138,15199-15240,46425-46443。

### 5.5 ProjectileDamagePayloadComponent

#### 职责

保存投射物携带的伤害值和伤害元数据，不拥有 NPC/Player 健康、死亡或状态结算。

~~~text
componentId: BD-COMP-PROJ-05
name: ProjectileDamagePayloadComponent
status: proposed
componentOwner: ProjectileSimulation 与 CombatAndStatus 的边界
crossSubsystemOwner: integration-review
entityScope: 单个可造成伤害的 Projectile entity
lifecycle: 生成时从来源和定义建立；命中前可调整；终止时移除；目标结算不写回本 Component 之外的目标状态
~~~

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| CurrentDamage | int | 0（由 spawn/definition 设置） | 权威载荷 | 最终合法范围由 Combat owner 定义；不直接代表目标实际损失 | confirmed | V4 Projectile.cs:142,11800-12849 |
| OriginalDamage | int | 0 | 权威基线 | 只记录初始基线，不随当前伤害变化 | confirmed | V4 Projectile.cs:144 |
| Knockback | float | 0.0f | 权威载荷 | 必须为 finite；单位和上限由 Combat owner 定义 | confirmed | V4 Projectile.cs:152 |
| ArmorPenetration | int | 0 | 权威载荷 | 不得为负，除非 Combat 规则明确允许特殊 sentinel | confirmed | V4 Projectile.cs:266 |
| BonusCritChance | int | 0 | 权威载荷 | 范围由 Combat 规则校验 | confirmed | V4 Projectile.cs:268 |
| BonusTagDamage | int | 0 | 权威载荷 | 不得把 tag bonus 当作目标健康 | confirmed | V4 Projectile.cs:264 |
| TagEffectType | int | 0 | 权威元数据 | 0 表示无 tag effect；具体内容 owner 为 integration-review | confirmed | V4 Projectile.cs:262 |
| DamageClass | ProjectileDamageClass | Generic | 权威定义/载荷 | 派生 melee/ranged/magic 视图，不维护多个可冲突布尔镜像 | partial | V4 Projectile.cs:224-228；dome ProjectileDamageClass.cs |
| IsColdDamage | bool | false | 权威元数据 | 只表示属性，不直接写目标状态 | confirmed | V4 Projectile.cs:230 |
| IsArrow | bool | false | 权威元数据 | 与 projectile type 的内容定义保持一致 | confirmed | V4 Projectile.cs:100 |
| HostileDamageScaling | ProjectileHostileDamageScaling | Default | 权威规则输入 | 只能作为伤害规则输入，不改写 HostileDefault | confirmed | V4 Projectile.cs:270 |

#### 字段不变量

CurrentDamage、OriginalDamage、Knockback、穿透和免疫是相关但不同的概念；本 Component 不保存 target ID、target health、最终减血值或跨目标免疫表。

#### 生命周期

生成时初始化；行为或来源规则可以更新载荷；命中结算读取并产生外部结果；Projectile 终止后移除。

#### Entity/World 范围

单个 Projectile entity。Combat target、NPC/Player resistance、banner registry 和 damage class catalog 为外部状态。

#### ID 与关系字段

本 Component 不拥有 target reference。命中目标 relation 的 owner 为 integration-review，与 payload 分离。

#### 当前 NLTX 映射

根 Damage Component 已有 current/original/knockback/armor penetration/critical chance；dome Damage Component 目前只保存 Amount 和 HitCount，而宽 Definition 仍承载大量 payload 字段，整体 partial。

#### 证据

V4 Projectile.cs:11480-11538,11773-11877,12698-12849。

### 5.6 ProjectilePenetrationStateComponent

#### 职责

保存穿透上限、剩余穿透和已接受命中数，以及穿透耗尽后的伤害资格规则。

~~~text
componentId: BD-COMP-PROJ-06
name: ProjectilePenetrationStateComponent
status: proposed
componentOwner: ProjectileSimulation
crossSubsystemOwner: integration-review
entityScope: 单个 Projectile entity
lifecycle: 生成初始化；每次接受命中更新；剩余次数达到终止条件时保留终止原因并随后清理
~~~

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| RemainingHits | int | 1 | 权威实例状态 | -1 为 Version4 无限穿透候选；不得出现其他未定义负值 | partial | V4 Projectile.cs:156,607；dome Penetration Component |
| MaximumHits | int | 1 | 权威基线 | 与生成时定义一致；可为 -1 表示无限候选 | partial | V4 Projectile.cs:166；dome Penetration Component |
| HitCount | int | 0 | 权威统计状态 | 不得为负；每个命中序列最多增加一次 | confirmed | V4 Projectile.cs:102,12698-12849 |
| StopsDealingDamageWhenDepleted | bool | false | 权威命中规则 | 只影响 Projectile damage eligibility，不直接销毁目标 | confirmed | V4 Projectile.cs:198 |

#### 字段不变量

穿透状态不包含 local/static/player immunity map；RemainingHits 变化必须保持 HitCount 的单调性和终止规则一致。

#### 生命周期

生成时从定义建立；接受命中时更新；到达耗尽条件时由生命周期边界读取；实体移除时清理。

#### Entity/World 范围

单个 Projectile entity。目标集合和全局 type immunity 不属于此 Component。

#### ID 与关系字段

无 ID；命中序列的 target identity 由跨子系统命中边界拥有。

#### 当前 NLTX 映射

根和 dome 均已有 Penetration Component，但 dome 还把 HitCount 放在 Damage Component 的近似字段中，需防止双写，故目标语义为 proposed、当前覆盖 partial。

#### 证据

V4 Projectile.cs:156,166,198,12698-12849。

### 5.7 ProjectileHitImmunityStateComponent

#### 职责

保存投射物实例针对 NPC/Player 目标的本地免疫计数和 restrike delay。静态 type 级共享免疫表不复制进实体。

~~~text
componentId: BD-COMP-PROJ-07
name: ProjectileHitImmunityStateComponent
status: proposed
componentOwner: ProjectileSimulation 与 CombatAndStatus 的边界
crossSubsystemOwner: integration-review
entityScope: 单个 Projectile entity；数组索引使用最终裁决的 target identity
lifecycle: 创建清零；每个 tick 递减；命中写入；实体清理时移除
~~~

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| LocalNpcImmunityTicks | int[] | 长度为 NPC 容量，全部 0 | 权威实例状态 | 索引必须是有效 NPC identity；计数不得小于 0 | confirmed | V4 Projectile.cs:158,11554-11592 |
| PlayerImmunityTicks | int[] | 长度为 255，全部 0 | 权威实例状态 | 索引必须是有效 Player identity；计数不得小于 0 | confirmed | V4 Projectile.cs:220 |
| RestrikeDelayTicks | int | 0 | 权威实例状态 | 不得小于 0；0 表示当前没有 restrike delay | confirmed | V4 Projectile.cs:192 |

#### 字段不变量

免疫计数只记录 projectile-local state；static、owner、type 和 Combat-wide immunity 的 owner 不迁入本 Component。计数数组的容量由整合后的 entity identity contract 决定。

#### 生命周期

创建分配并清零；更新递减；接受命中后按策略写入；销毁时释放，不作为持久化或网络快照的隐式全集。

#### Entity/World 范围

单个 Projectile entity，按外部 NPC/Player identity 索引。目标实体的免疫 Component 不由此处拥有。

#### ID 与关系字段

目标 identity 是外部关系键，类型、生命周期和反查方式为 integration-review。

#### 当前 NLTX 映射

dome 已有 HitImmunity 相关模块和 ProjectileRestrikeDelayComponent，但 Version4 的数组作用域、static/owner fallback 和组合规则未闭合，当前 partial。

#### 证据

V4 Projectile.cs:92,158,162,192,220,11554-11656。

### 5.8 ProjectileHitImmunityPolicyComponent

#### 职责

保存免疫策略和冷却默认值，与实际计数分离，防止策略改变时重置或镜像计数。

~~~text
componentId: BD-COMP-PROJ-08
name: ProjectileHitImmunityPolicyComponent
status: proposed
componentOwner: ProjectileSimulation 与 CombatAndStatus 的边界
crossSubsystemOwner: integration-review
entityScope: 单个 Projectile entity 的命中策略
lifecycle: 生成时由定义初始化；命中资格读取；实体清理时移除
~~~

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| UsesLocalNpcImmunity | bool | false | 权威策略 | 只选择 local 计数，不创建额外 target health | confirmed | V4 Projectile.cs:160 |
| UsesStaticNpcImmunity | bool | false | 权威策略 | static 表 owner 仍为共享边界 | confirmed | V4 Projectile.cs:162 |
| LocalNpcCooldownTicks | int | -2 | 权威策略默认值 | -2 的 sentinel 语义必须保持；其余值域由命中规则定义 | confirmed | V4 Projectile.cs:256 |
| StaticNpcCooldownTicks | int | -1 | 权威策略默认值 | -1 的 sentinel 语义必须保持 | confirmed | V4 Projectile.cs:258 |
| AppliesOnSingleHit | bool | false | 权威策略 | 只影响单次命中的免疫提交条件 | confirmed | V4 Projectile.cs:164 |
| UsesOwnerMeleeCooldown | bool | false | 权威策略 | owner 类型必须支持该规则，否则不得静默启用 | partial | V4 Projectile.cs:218,374-380 |
| CopiesOwnerCooldownOnSpawn | bool | false | 生成策略 | 只能在 owner 与来源可用时复制；owner 方案未裁决 | partial | NLTX dome Definition Component |

#### 字段不变量

策略字段不能与 immunity state 的计数重复；perIDStaticNPCImmunity 只作为外部共享表读取。

#### 生命周期

生成时冻结策略；类型或来源改变策略时必须显式替换该字段，不能依赖文件或目录隐式同步。

#### Entity/World 范围

单个 Projectile entity；static immunity 表和 owner melee cooldown 由相邻领域提供。

#### ID 与关系字段

不拥有 target ID 或 owner ID，但读取 owner reference；owner relation 的最终类型为 integration-review。

#### 当前 NLTX 映射

dome 宽 Definition 已包含 local/static/owner cooldown 字段；现有 ProjectileRestrikeDelayComponent 只覆盖计数，不覆盖完整 policy，故目标为 proposed、当前 partial。

#### 证据

V4 Projectile.cs:92,160-164,192,218,256-258。

### 5.9 ProjectileCollisionPolicyComponent

#### 职责

保存投射物自身的 Tile、液体、斜坡、fall-through、反射/反弹和 owner self-hit 策略；不拥有 WorldGrid、Tile storage 或其他实体的碰撞体。

~~~text
componentId: BD-COMP-PROJ-09
name: ProjectileCollisionPolicyComponent
status: proposed
componentOwner: ProjectileSimulation 与 SpatialSimulation 的边界
crossSubsystemOwner: integration-review
entityScope: 单个 Projectile entity
lifecycle: 创建时由定义初始化；移动/碰撞读取并可写入实例覆盖；销毁时移除
~~~

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| TileCollisionEnabled | bool | true | 权威策略 | false 时不产生普通 Tile 碰撞资格 | confirmed | V4 Projectile.cs:194；dome TileCollision Component |
| IgnoreWater | bool | false | 权威策略 | 只影响液体碰撞规则，不删除几何位置 | confirmed | V4 Projectile.cs:202 |
| CorrectSlopeCollision | bool | false（SetDefaults 后由定义覆盖） | 权威策略 | 只影响斜坡处理 | partial | V4 Projectile.cs:250,444-556 |
| DecidesManualFallThrough | bool | false | 权威策略 | false 时不得由实例 fall-through 覆盖 | confirmed | V4 Projectile.cs:252 |
| ShouldFallThrough | bool | false | 权威实例状态 | 只有 DecidesManualFallThrough 为真时才有效 | confirmed | V4 Projectile.cs:254；dome FallThrough Component |
| ReflectsFromTiles | bool | false | 权威策略 | 反射只改变 projectile-local 结果，Tile 不由此处写入 | partial | V4 Projectile.cs:15513+；dome Definition Component |
| MaximumBounces | int | 0 | 权威策略/上限 | 不得为负；0 表示无反弹能力候选 | partial | dome Bounce Component、Definition Component |
| BounceVelocityMultiplier | float | 1.0f | 权威策略 | 必须为 finite 且不小于 0 | partial | dome Definition Component |
| MinimumBounceSpeed | float | 0.0f | 权威策略 | 必须为 finite 且不小于 0 | partial | dome Definition Component |
| OwnerHitCheck | bool | false | 权威策略 | 只控制 owner self-hit qualification | confirmed | V4 Projectile.cs:216 |
| OwnerHitCheckDistance | float | 1000.0f | 权威策略 | 必须为 finite 且不小于 0 | confirmed | V4 Projectile.cs:98 |
| ManualDirectionChange | bool | false | 权威策略 | 只允许明确的行为契约改变方向 | confirmed | V4 Projectile.cs:246 |

#### 字段不变量

碰撞策略与 LocationComponent、VelocityComponent、Tile/World 状态分离。ShouldFallThrough 是实例覆盖，不应被错误合并为 definition-only 常量。

#### 生命周期

创建初始化；每个移动/碰撞判定读取；反射/反弹可更新实例结果或相关 Geometry state；销毁时移除。

#### Entity/World 范围

单个 Projectile entity。WorldGrid、Tile、liquid contact 和实体 broadphase 的 owner 为 SpatialSimulation 或 integration-review。

#### ID 与关系字段

owner self-hit 只引用 Identity Component 的 owner relation；不存储 target entity 集合。

#### 当前 NLTX 映射

dome 已有 TileCollision、FallThrough、Bounce、Reflection 等局部组件，且宽 Definition 仍包含其定义字段；当前可视为 partial，目标组合要求去除重复镜像。

#### 证据

V4 Projectile.cs:13654,15513-15957。

### 5.10 ProjectileGeometryStateComponent

#### 职责

保存 Projectile-specific 几何缩放和反射结果等实体几何状态。它不复制通用位置、速度或 collider；几何历史缓存另见 TrailCache。

~~~text
componentId: BD-COMP-PROJ-10
name: ProjectileGeometryStateComponent
status: proposed
componentOwner: ProjectileSimulation 与 SpatialSimulation 的边界
crossSubsystemOwner: integration-review
entityScope: 单个 Projectile entity
lifecycle: 创建时初始化；碰撞/反射时更新；销毁时移除
~~~

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| Scale | float | 1.0f | 权威几何状态 | 必须为 finite 且不小于 0；不得直接修改通用 collider owner | confirmed | V4 Projectile.cs:114 |
| Reflected | bool | false | 权威实例结果 | 一旦设置为真，除非有明确反射规则，不应静默恢复 false | confirmed | V4 Projectile.cs:150,11758-11767 |

#### 字段不变量

Scale 是 projectile-specific 几何输入，通用碰撞体仍属于共享实体几何；Reflected 是实例结果，不是 Definition policy 的镜像。

#### 生命周期

生成时取定义 scale；反射/特殊碰撞时写入结果；销毁时移除。

#### Entity/World 范围

单个 Projectile entity；Tile、target hitbox 和 World geometry 不属于此 Component。

#### ID 与关系字段

无 ID；几何 contact relation 由 SpatialSimulation 提供。

#### 当前 NLTX 映射

dome 目前以 ProjectileReflectionComponent 和 Definition.Scale 分散表达；根组件没有对应几何状态，当前 target 为 proposed。

#### 证据

V4 Projectile.cs:114,150,13654-13899。

### 5.11 ProjectileNetworkStateComponent

#### 职责

保存实例网络重要性、dirty 标志、节流计数和按 player 的 section skip 状态。协议包、连接状态、section registry 和编码字段不属于此 Component。

~~~text
componentId: BD-COMP-PROJ-11
name: ProjectileNetworkStateComponent
status: proposed
componentOwner: ProjectileSimulation 与 NetworkSessionAndSectionStreaming 的边界
crossSubsystemOwner: integration-review
entityScope: 单个 Projectile entity 的网络状态
lifecycle: 创建初始化；状态变化时置 dirty/节流；网络边界读取并清理相应标志；销毁时移除
~~~

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| NetworkImportant | bool | false | 权威网络策略 | 只影响槽位回收/网络重要性，不成为 persistent ID | confirmed | V4 Projectile.cs:106,10227-10242 |
| PrimaryUpdatePending | bool | false | 权威网络状态 | true 表示状态 dirty，不等于已发送 | confirmed | V4 Projectile.cs:172；dome NetworkUpdate Component |
| SecondaryUpdatePending | bool | false | 权威网络状态 | 与 Primary 标志独立，清理条件须由网络 owner 定义 | confirmed | V4 Projectile.cs:174；dome NetworkUpdate Component |
| NetSpam | int | 0 | 权威节流状态 | 值域保持 Version4/当前 dome 约束；不得为负 | confirmed | V4 Projectile.cs:176；dome NetworkUpdate Component |
| SectionSyncSkippedForPlayer | bool[] | 长度 255，全部 false | 权威临时网络状态 | 索引是连接/player slot，不能当 projectile identity | confirmed | V4 Projectile.cs:178,15271-15275 |
| SendRequested | bool | false | 权威网络意图状态 | 只表示请求/dirty，不表示协议收发完成 | partial | dome NetworkUpdate Component |

#### 字段不变量

本 Component 不含 ReplicationId、packet type、serialized bytes、section object 或客户端预测状态。所有共享 network ID 和 section owner 均为 integration-review。

#### 生命周期

实体创建时初始化；模拟状态改变时置 dirty；网络边界读取并根据结果更新节流/skip 状态；实体销毁时不自动变成 tombstone。

#### Entity/World 范围

单个 Projectile entity；连接、section/PVS 和 replication registry 是外部范围。

#### ID 与关系字段

ReplicationId、network identity 和 section coordinates 只通过外部快照/协议边界表达，不进入 Component。

#### 当前 NLTX 映射

dome 已有 ProjectileNetworkIdentityComponent 和 ProjectileNetworkUpdateComponent，但 identity、UUID、replication 和 section 字段仍混合，当前 partial。

#### 证据

V4 Projectile.cs:106,172-178,15241-15275。

### 5.12 ProjectileSourceMetadataComponent

#### 职责

保存生成来源的轻量元数据和销毁/掉落边界需要的标记；不保存完整外部 source object，也不拥有 child、drop 或 fishing outcome 事务。

~~~text
componentId: BD-COMP-PROJ-12
name: ProjectileSourceMetadataComponent
status: proposed
componentOwner: ProjectileSimulation 与 SpawnLifecycleAndLoot 的边界
crossSubsystemOwner: integration-review
entityScope: 单个 Projectile entity
lifecycle: 生成时由来源输入建立；实例期间只读或按明确规则更新；销毁边界读取后移除
~~~

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| BannerIdToRespondTo | int | 0 | 权威来源元数据 | 0 表示无 banner response；有效范围由内容目录定义 | confirmed | V4 Projectile.cs:260 |
| MiscText | string | 空字符串 | 权威来源元数据 | 不为 null；长度/编码限制由生成边界定义 | confirmed | V4 Projectile.cs:222,10244-10343 |
| OriginatedFromActivableTile | bool | false | 权威来源元数据 | 只记录来源事实，不拥有 Tile 实例 | confirmed | V4 Projectile.cs:240 |
| NoDropItem | bool | false | 权威来源/终止规则 | 只表达禁止掉落标志；掉落事务 owner 未在此宣布 | confirmed | V4 Projectile.cs:108 |
| IsNpcProjectile | bool | false | 权威来源分类 | true 时不能假设 owner 是 Player | confirmed | V4 Projectile.cs:238,362-372 |
| MinionSpawnItemType | ushort | 0 | 来源兼容字段 | 只有 minion 来源存在时非零；不能代替 ItemInstanceId | partial | dome MinionSpawnSource Component |
| MinionSpawnItemPrefix | int | 0 | 来源兼容字段 | 不得为负；内容语义由 Item owner 裁决 | partial | dome MinionSpawnSource Component |

#### 字段不变量

IEntitySource、MinionSpawnInfo 等完整 source object 不直接嵌入 Component；如需保留，必须先确定稳定 value object 和跨子系统 owner。NoDropItem 不能被解释为已经完成掉落决策。

#### 生命周期

生成初始化；来源元数据在实例存续期间供相邻边界读取；销毁后不保留为 persistent source，除非整合会话另行定义。

#### Entity/World 范围

单个 Projectile entity；Item、Tile、Player 和 spawn request 的完整对象由相邻领域拥有。

#### ID 与关系字段

Item type/prefix 是内容兼容字段，不是 ItemInstanceId；source relation 的长期 ID owner 为 integration-review。

#### 当前 NLTX 映射

dome 已有 BannerResponse、MiscText 和 MinionSpawnSource 组件；root 没有完整来源组件，当前整体 partial。

#### 证据

V4 Projectile.cs:10244-10343,208,222,238,240,260。

### 5.13 ProjectileTrailCacheComponent

#### 职责

保存用于特殊几何和表现读取的旧位置、旧旋转、旧方向及可选 whip points。它是可重建缓存，不是当前位置和速度的权威来源。

~~~text
componentId: BD-COMP-PROJ-13
name: ProjectileTrailCacheComponent
status: proposed
componentOwner: ProjectileSimulation 与 SpatialSimulation/Presentation 的边界
crossSubsystemOwner: integration-review
entityScope: 需要历史几何或表现轨迹的单个 Projectile entity
lifecycle: 特殊实体生成时附加并清零；每 tick 更新；轨迹失效时整体重建或清空；销毁时移除
~~~

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| OldPositions | Vector2[10] | 10 个零向量 | 缓存 | 长度由 trail 规则决定；不得被当作当前权威位置 | confirmed | V4 Projectile.cs:180,472-482 |
| OldRotations | float[10] | 10 个 0.0f | 缓存 | 长度与 OldPositions 的有效历史窗口一致 | confirmed | V4 Projectile.cs:182,472-482 |
| OldSpriteDirections | int[10] | 10 个 0 | 缓存 | 只作历史方向，不得覆盖当前方向 | confirmed | V4 Projectile.cs:184 |
| WhipPoints | List<Vector2> | 空列表 | 缓存/特殊几何输入 | 只在 whip 类几何有效；失效时清空 | confirmed | V4 Projectile.cs:282,13654+ |

#### 字段不变量

缓存必须有明确的重建/失效条件；任何消费者都不能用它替代通用 Location/Velocity 或把缓存快照回写为权威轨迹。

#### 生命周期

创建时清零；每个相关 tick 更新；类型/行为改变或 entity 复用时清空；销毁时释放。

#### Entity/World 范围

单个需要历史几何的 Projectile entity；不属于 World、Tile 或全局集合。

#### ID 与关系字段

无 ID。几何历史只关联当前 entity，不构成 target relation。

#### 当前 NLTX 映射

当前未见与 Version4 三个 trail 数组完全对应的权威 Component；目标为 proposed。

#### 证据

V4 Projectile.cs:180-184,13654-13899,15199-15227。

### 5.14 ProjectileSentryCapabilityComponent

#### 职责

以存在标记表示 Projectile 具有 sentry 能力；sentry 容量、放置规则和 owner 资源不归此标记拥有。

~~~text
componentId: BD-COMP-PROJ-14
name: ProjectileSentryCapabilityComponent
status: proposed
componentOwner: ProjectileSimulation 与 PlayerGameplay 的边界
crossSubsystemOwner: integration-review
entityScope: 单个 sentry Projectile entity
lifecycle: type/来源确认后附加；owner 生命周期和实体销毁时移除
~~~

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| Present | marker | 缺少 Component | 权威能力标记 | 有 Component 即表示 sentry；不重复保存 bool IsSentry | confirmed | V4 Projectile.cs:122,314-324 |

#### 字段不变量

能力标记不保存 owner capacity、炮塔数量或 persistent placement。

#### 生命周期

生成时按定义/来源附加；sentry 实例失效或销毁时移除。

#### Entity/World 范围

单个 Projectile entity；owner capacity 和 placement 是相邻领域状态。

#### ID 与关系字段

只通过 Identity Component 引用 owner，不拥有 owner ID。

#### 当前 NLTX 映射

dome ProjectileSentryComponent.cs 已存在，标记 existing；其与 owner persistence 的完整关系仍 partial。

#### 证据

V4 Projectile.cs:122,14736-14767。

### 5.15 ProjectileMinionCapabilityComponent

#### 职责

保存 minion 能力的容量和 owner 内位置；不保存 owner 的全部 minion accounting，也不把 owner target NPC 镜像进 Projectile。

~~~text
componentId: BD-COMP-PROJ-15
name: ProjectileMinionCapabilityComponent
status: proposed
componentOwner: ProjectileSimulation 与 PlayerGameplay 的边界
crossSubsystemOwner: integration-review
entityScope: 单个 minion Projectile entity
lifecycle: 生成时附加容量/位置；owner 生命周期或实体销毁时移除；位置变化由明确 owner 更新
~~~

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| MinionSlots | float | 0.0f | 权威能力状态 | 必须为 finite 且不小于 0 | confirmed | V4 Projectile.cs:188 |
| MinionPosition | int | 0 | 权威能力状态 | 位置值域由 PlayerGameplay owner 定义 | confirmed | V4 Projectile.cs:190 |

#### 字段不变量

OwnerMinionAttackTargetNPC 是派生关系，不复制为 Projectile-owned target 字段；owner 的 minion count 不在此 Component。

#### 生命周期

minion 生成时附加；owner 失效或 Projectile 清理时移除；容量变化不应改变 identity。

#### Entity/World 范围

单个 minion Projectile entity；owner 的 summon accounting 和目标 NPC 属于相邻领域。

#### ID 与关系字段

owner 只通过 Identity Component 关系表达；target NPC reference owner unresolved。

#### 当前 NLTX 映射

dome ProjectileMinionComponent.cs 和 SummonedProjectileStateComponent.cs 已存在，覆盖局部能力，标记 existing/partial；root 没有对应能力组件。

#### 证据

V4 Projectile.cs:186-190,350-360,14711-14767。

### 5.16 ProjectileTrapCapabilityComponent

#### 职责

以存在标记表示 trap projectile 能力；不拥有 Tile 激活、陷阱生成来源或 NPC/Player 目标结算。

~~~text
componentId: BD-COMP-PROJ-16
name: ProjectileTrapCapabilityComponent
status: proposed
componentOwner: ProjectileSimulation 与 WorldInteraction/CombatAndStatus 的边界
crossSubsystemOwner: integration-review
entityScope: 单个 trap Projectile entity
lifecycle: 来源/定义确认后附加；实体销毁时移除
~~~

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| Present | marker | 缺少 Component | 权威能力标记 | 有 Component 即表示 trap；不重复保存 bool IsTrap | confirmed | V4 Projectile.cs:236 |

#### 字段不变量

Trap marker 与 OriginatedFromActivableTile 来源字段独立；一个表示能力，一个表示来源事实。

#### 生命周期

生成时附加；终止或实体销毁时移除。

#### Entity/World 范围

单个 Projectile entity；Tile activation、world structure 和目标状态不归属此处。

#### ID 与关系字段

不拥有 Tile entity ID 或 target ID；相关关系由 integration-review 裁决。

#### 当前 NLTX 映射

dome ProjectileTrapComponent.cs 已存在，标记 existing；trap owner 与 NPC/world owner 仍 partial。

#### 证据

V4 Projectile.cs:236,11594-11656。

### 5.17 ProjectileBobberCapabilityComponent

#### 职责

以存在标记表示 Projectile 是 fishing bobber carrier，可选地保留最小 bobber type/context reference。捕获资格、FishingAttempt、结果、掉落和敌对生成不属于此 Component。

~~~text
componentId: BD-COMP-PROJ-17
name: ProjectileBobberCapabilityComponent
status: proposed
componentOwner: ProjectileSimulation 与 FishingAndCatchSimulation 的边界
crossSubsystemOwner: integration-review
entityScope: 单个 bobber Projectile entity
lifecycle: 生成时附加；bobber 终止或实体销毁时移除；Fishing context 引用的生命周期需整合裁决
~~~

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| Present | marker | 缺少 Component | 权威能力标记 | 有 Component 即表示 bobber carrier | confirmed | V4 Projectile.cs:104 |
| BobberType | int | 0 | 兼容 carrier 元数据 | 必须与 Definition ProjectileType 一致；不保存 fishing outcome | partial | dome FishingBobberStateComponent.cs:5-22 |
| FishingContextReference | unresolved value/reference | unresolved | 外部关系 | 必须由 FishingAndCatchSimulation 定义稳定类型后才能锁定 | unresolved | V4 Projectile.cs:34073-34074,47779-47786；full reference:51236-51552 |

#### 字段不变量

FishingBobberStateComponent 当前包含 phase、water count、fishing level、pending item 等结果相关字段；这些字段不能因为当前实现存在就回流为 Projectile carrier 的权威字段。

#### 生命周期

carrier 随 Projectile 创建/清理；Fishing context 的建立、消费和结果提交生命周期由相邻领域决定。

#### Entity/World 范围

单个 bobber Projectile entity；水体、钓鱼规则、物品结果和敌对生成是外部范围。

#### ID 与关系字段

owner reference 仍来自 Identity Component；Fishing context relation、ItemInstanceId 和 outcome owner 为 integration-review。

#### 当前 NLTX 映射

dome 有 ProjectileBobberComponent 和 FishingBobberStateComponent；前者可作为 carrier，后者当前过宽且跨域，分别标记 existing/partial。

#### 证据

V4 Projectile.cs:104,34073-34074,47779-47786。

### 5.18 ProjectileCounterweightCapabilityComponent

#### 职责

以存在标记表示 counterweight projectile 能力，不保存绳索/owner 集合或表现关系。

~~~text
componentId: BD-COMP-PROJ-18
name: ProjectileCounterweightCapabilityComponent
status: proposed
componentOwner: ProjectileSimulation 与 PlayerGameplay 的边界
crossSubsystemOwner: integration-review
entityScope: 单个 counterweight Projectile entity
lifecycle: 生成时附加；实体销毁时移除
~~~

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| Present | marker | 缺少 Component | 权威能力标记 | 有 Component 即表示 counterweight；不重复保存 bool IsCounterweight | confirmed | V4 Projectile.cs:112 |

#### 字段不变量

该标记不拥有其他绳索/挂接实体关系；若后续确认独立 attachment lifecycle，应由 integration-review 增加独立关系模型。

#### 生命周期

生成时附加，终止时移除。

#### Entity/World 范围

单个 Projectile entity；owner、绳索关系和表现状态为外部范围。

#### ID 与关系字段

不拥有关系 ID。

#### 当前 NLTX 映射

dome ProjectileCounterweightComponent.cs 已存在，标记 existing；其与玩家绳索/其他实体关系未闭合。

#### 证据

V4 Projectile.cs:112。

## 6. Entity 与 Component 组合

本节只描述组合，不描述处理顺序或运行时执行结构。

| Entity/World 对象 | 必需 Component | 可选 Component | 互斥 Component | 组合理由 |
|---|---|---|---|---|
| 普通 Projectile entity | Identity、Definition、TrajectoryState、Lifetime、DamagePayload、PenetrationState、CollisionPolicy、GeometryState、NetworkState、SourceMetadata | HitImmunityState/Policy、TrailCache、能力 Component | 本会话没有确认的硬互斥 | 普通投射物需要身份、定义、轨迹、生命、碰撞和复制相关权威状态；非伤害投射物可不附加免疫状态 |
| 可命中 NPC/Player 的 Projectile entity | 普通 Projectile 组合 | HitImmunityState、HitImmunityPolicy、TrailCache | 无已确认互斥 | 免疫计数只对有命中资格的实体存在，避免把所有 target map 塞入非伤害投射物 |
| 需要特殊 trail/whip/lance 几何的 Projectile entity | 普通 Projectile 组合 | TrailCache、GeometryState | 无已确认互斥 | 特殊几何需要历史缓存，但缓存不替代当前轨迹和通用 collider |
| Sentry Projectile entity | 普通 Projectile 组合 | SentryCapability | 无已确认互斥 | 能力标记与普通投射物状态正交；owner capacity 不复制进实体 |
| Minion Projectile entity | 普通 Projectile 组合 | MinionCapability、SourceMetadata 中的 minion 来源字段 | 与 Sentry/Trap 无由本会话证明的互斥 | minion capacity/position 只在需要时存在；owner target 关系不嵌入 |
| Trap Projectile entity | 普通 Projectile 组合 | TrapCapability | 无已确认互斥 | trap 是 projectile capability，不吞并 Tile/WorldInteraction 状态 |
| Bobber Projectile entity | 普通 Projectile 组合 | BobberCapability、Fishing context reference | 与其他能力无已确认互斥 | bobber 只作为 fishing carrier；fishing 结果字段留在相邻领域 |
| Counterweight Projectile entity | 普通 Projectile 组合 | CounterweightCapability | 无已确认互斥 | counterweight 只表达能力标记，不扩展为绳索/挂接实体 |
| WorldGrid、Tile、WorldSection | 无 Projectile Component | 外部几何/section 状态 | 不适用 | 世界、Tile 和 section 是独立状态范围，不复制到 Projectile entity |
| NPC、Player、Item、Fishing outcome、LeashedEntity | 无本设计的 Projectile Component | 通过已裁决的 reference/context 关联 | 不适用 | 目标健康、Item 事务、Fishing outcome 和 LeashedEntity lifecycle 不属于 Projectile 状态 owner |

组合约束：

- Definition 中的默认值不得与运行时状态 Component 形成无声明的双向镜像；若默认值需要改变，必须明确写入运行时 Component。
- Identity 只放关系和身份 token；NetworkState 不得反向拥有 identity。
- TrailCache 只在有历史几何需求时附加；其缺失不能被解释为当前位置缺失。
- BobberCapability 不得包含 fishing item、catch rarity、water qualification 或 enemy spawn outcome 的权威字段。
- 任何跨子系统 value object、ID 或 relation 的最终 owner 都保持 crossSubsystemOwner: integration-review。

## 7. 组件拆分与合并决策

| 决策 | 结论 | 依据 |
|---|---|---|
| Definition 与运行时状态拆分 | 必须拆分 | type、behavior key、catalog revision 的生命周期接近创建，而 timeLeft、AI、hit count、net dirty 会持续变化；dome 宽 Definition 已显示巨型 Component 风险 |
| Damage payload 与 penetration 拆分 | 必须拆分 | damage 数值/类别和命中次数有不同变更原因；非伤害投射物可能拥有 lifetime/轨迹但不需要完整 payload |
| Damage payload 与 immunity state 拆分 | 必须拆分 | damage 是本体载荷，免疫是按目标计数；计数数组的失效条件与伤害载荷不同 |
| Immunity policy 与 immunity state 拆分 | 必须拆分 | policy 是创建时规则，state 是每 tick/每目标计数；合并会导致策略重建时意外清零计数 |
| Collision policy 与 Geometry state 拆分 | 必须拆分 | tile/liquid/fall-through 是规则，scale/reflected 是实例几何状态；两者变更原因不同 |
| Geometry state 与通用位置/速度拆分 | 必须保留边界 | Version4 Projectile 继承通用 Entity 几何；复制位置/速度会产生双 authority |
| Trajectory raw AI 与每种 type 独立 Component 拆分 | 暂不按 type 原子化 | 当前行为覆盖不完整；保留固定 raw slots 能降低兼容同步风险，待 typed 契约闭合后再评估 |
| Network state 与协议/快照拆分 | 必须拆分 | dirty、节流、section skip 是实例状态；replication snapshot 和 network slice 是外部表示，不能反向成为 authority |
| Trail cache 与 trajectory state 拆分 | 必须拆分 | trail 可重建、会失效且只服务特殊几何/表现；AI 状态是权威兼容状态 |
| 五类专用能力分别建 Component | 倾向分别保留 | sentry、minion、trap、bobber、counterweight 需要不同字段子集和不同相邻 owner；合并为 capability 巨型 Component 会重新聚合跨域状态 |
| 能力 marker 与能力规则合并 | 不合并 | marker 表示实体是否具备能力；owner capacity、Fishing outcome、Tile activation 等规则有独立生命周期 |
| active 与 lifetime 合并 | 只保留派生关系 | active 不应与 RemainingTicks 形成两个可写 authority；由实体存续/lifetime 表达 |
| ID 字段合并 | 禁止合并 | slot/whoAmI、owner-scoped identity、UUID、runtime entity、replication ID、persistent ID 的复用语义不同 |
| oldPos 与当前位置合并 | 禁止合并 | oldPos 的失效和重建规则与当前位置不同，且特殊碰撞只需历史窗口 |

粒度判断遵循：共同创建且共同维护一个不可分割不变量的字段才合并；定义、状态、计数、缓存、外部协议和表现字段只要 owner 或失效条件不同就拆分。当前没有证据支持把 LocationComponent、VelocityComponent、Tile storage 或 target health 放进 Projectile Component。

## 8. 不单独创建 Component 的对象

| 对象 | 处理 | 不单独创建的理由 |
|---|---|---|
| active、IsActive、IsExpired | 派生值 | 与实体存在和 Lifetime 的零值关联，不应产生第二个可写状态 |
| MaxUpdates | Definition 的派生值 | 只是 ExtraUpdates + 1，独立 Component 会制造重复镜像 |
| Name、Opacity | 派生/表现值 | Name 来自 type/catalog；Opacity 来自 alpha；均不是 Projectile authority |
| OwnedBySomeone、CareForAttackCD | 资格派生值 | 依赖 owner、npc/trap 标记和策略，不能脱离这些字段形成独立状态 |
| OwnerMinionAttackTargetNPC | 外部关系派生值 | 读取 owner 当前目标，不能把 NPC target 镜像到 Projectile |
| perIDStaticNPCImmunity | 世界/内容共享表 | 不是单个实体的状态；最终 owner 为 integration-review |
| maxAI、生命常量和 type 常量 | 常量/目录资料 | 不随 entity 变化，Component 只保存实例所需定义值 |
| WhipPointsForCollision 之外的临时几何列表 | 缓存内部值 | 若只服务一次计算，不形成独立长期状态；需要历史窗口时才进入 TrailCache |
| LocationComponent、VelocityComponent、通用 ColliderComponent | 共享实体基础状态 | Projectile 只引用，不复制；避免位置、速度和 collider 双 authority |
| WorldGrid、Tile、liquid、section registry | World/Section 状态 | 生命周期和 owner 不是单个 Projectile entity |
| NPC/Player health、death、status、Item inventory/drop 事务 | 相邻领域状态 | Projectile 只携带 payload、能力或来源关系，不拥有目标或掉落结果 |
| FishingAttempt、catch result、pending item 和 FishingContext 的完整对象 | FishingAndCatchSimulation 状态 | bobber 只是 carrier；当前 dome FishingBobberStateComponent 的结果字段不能成为 Projectile 巨型 Component |
| LeashedEntity registry、anchor、section list | LeashedEntitySimulation 状态 | Version4 有独立 registry/section/lifecycle/network 边界；不能因关系相近而并入 |
| ProjectileReplicationSnapshot、NetworkProjectileSlice | 快照/协议表示 | 只读表示不能反向成为权威 Component；当前快照字段过宽，须维持外部边界 |
| alpha、glow、frame、drawLayer、hide、soundDelay、preview dummy | 表现/工具状态 | 本 Component-only 设计不建立权威表现 Component；其客户端 owner 未在本会话裁决 |

## 9. 当前 NLTX 组件覆盖

本节状态标签严格按当前源码事实使用：源码中已存在、但尚未证明 Version4 完整覆盖的组件记为 status: existing；当前组件与目标语义只有局部覆盖、或需要拆分/整合的记为 status: partial。existing 只表示组件源码存在，不表示行为等价或验证通过。

明确标记：

- status: existing：dome 的 ProjectileSentryComponent、ProjectileMinionComponent、ProjectileTrapComponent、ProjectileBobberComponent、ProjectileCounterweightComponent、ProjectileBannerResponseComponent、ProjectileMiscTextComponent 和 ProjectileMinionSpawnSourceComponent。
- status: partial：根 src 的所有 Projectile 组件、dome 的宽 ProjectileDefinitionComponent、ProjectileNetworkIdentityComponent、ProjectileNetworkUpdateComponent、ProjectileLifetimeComponent、ProjectilePenetrationComponent，以及所有需要跨域 owner 裁决的当前映射。

| 当前组件 | 实际路径 | 目标 Component 映射 | 当前状态 | 说明 |
|---|---|---|---|---|
| ProjectileBehaviorComponent | D:\TRbackup\NLTX\src\Projectile\ProjectileBehaviorComponent.cs:3-30 | TrajectoryState + Definition.BehaviorKey | partial | 根组件保存 Style 和不完整 AI/localAI slots；不是 Version4 全覆盖 |
| ProjectileDamageComponent | D:\TRbackup\NLTX\src\Projectile\ProjectileDamageComponent.cs:3-23 | DamagePayload | partial | 已有 current/original/knockback/armor/crit，缺少完整 tag/class/属性边界 |
| ProjectileDefinitionComponent | D:\TRbackup\NLTX\src\Projectile\ProjectileDefinitionComponent.cs:3-23 | Definition | partial | 精简定义覆盖存在，但默认/运行时可变边界尚未闭合 |
| ProjectileDirectionComponent | D:\TRbackup\NLTX\src\Projectile\ProjectileDirectionComponent.cs:5-12 | TrajectoryState | partial | Vector2 Trajectory 不等于 Version4 的 direction、position 和 velocity 组合 |
| ProjectileLifetimeComponent | D:\TRbackup\NLTX\src\Projectile\ProjectileLifetimeComponent.cs:3-12 | Lifetime | partial | 已有 RemainingTicks/EndReason，未覆盖终止外部事务 |
| ProjectileOwnerComponent | D:\TRbackup\NLTX\src\Projectile\ProjectileOwnerComponent.cs:5-12 | Identity.OwnerReference | partial | EntityReference 已存在，但 Version4 255 sentinel、NPC/trap/world owner 和 dome PlayerHandle 未统一 |
| ProjectilePenetrationComponent | D:\TRbackup\NLTX\src\Projectile\ProjectilePenetrationComponent.cs:3-17 | PenetrationState | partial | 已有 remaining/maximum/depletion flag，命中计数仍需唯一 owner |
| dome ProjectileBehaviorComponent | D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Components\AI\ProjectileBehaviorComponent.cs:3-61 | TrajectoryState | partial | 已有 typed state 和 3 个 local AI；行为覆盖仍不完整 |
| dome ProjectileDefinitionComponent | D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Components\Projectile\ProjectileDefinitionComponent.cs:5-173 | Definition、DamagePayload、ImmunityPolicy、CollisionPolicy、Capability 和 Source | partial | 当前 record 太宽，不能直接作为最终 Component |
| ProjectileNetworkIdentityComponent | D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Components\Projectile\ProjectileNetworkIdentityComponent.cs:5-9 | Identity | partial | PlayerHandle/identity/Guid? 不能证明覆盖 Version4 owner/slot/identity/UUID 分离 |
| ProjectileNetworkUpdateComponent | D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Components\Projectile\ProjectileNetworkUpdateComponent.cs:4-24 | NetworkState | partial | 已有 pending/netSpam/send 字段，section skip 和 replication owner 未闭合 |
| dome ProjectileLifetimeComponent | D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Components\Projectile\ProjectileLifetimeComponent.cs:5-28 | Lifetime | partial | 已有 active/expired 派生属性，未有完整终止原因/外部提交边界 |
| dome ProjectilePenetrationComponent | D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Components\Projectile\ProjectilePenetrationComponent.cs:5-29 | PenetrationState | partial | 有 maximum/remaining，但 HitCount 目前出现在 Damage Component 近似字段 |
| ProjectileBannerResponseComponent、ProjectileMiscTextComponent | D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Components\Projectile\ProjectileBannerResponseComponent.cs、ProjectileMiscTextComponent.cs | SourceMetadata | existing/partial | 已有 banner 和 misc text；来源 object、NoDrop、NPC source 仍未完全归档 |
| ProjectileMinionSpawnSourceComponent | D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Components\Projectile\ProjectileMinionSpawnSourceComponent.cs:5-30 | SourceMetadata + MinionCapability | existing/partial | 已有 item type/prefix；不能代替 ItemInstanceId 或完整 MinionSpawnInfo |
| Tile/FallThrough/Bounce/Reflection/StepSpeed 组件 | D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Components\Projectile\ProjectileTileCollisionComponent.cs 等 | CollisionPolicy、GeometryState、TrajectoryState | existing/partial | 局部边界已存在，但 Definition 与运行时状态仍有重复镜像 |
| Sentry/Minion/Trap/Bobber/Counterweight marker 组件 | D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Components\Projectile\ProjectileSentryComponent.cs 等 | 对应五个能力 Component | existing/partial | 能力 marker 已有局部覆盖；相邻 owner 和组合约束未全闭合 |
| FishingBobberStateComponent | D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Fishing\FishingBobberStateComponent.cs:5-22 | 不并入 BobberCapability；保留为 Fishing 边界候选 | partial | 当前包含 phase、water、fishing level 和 pending item 等跨域结果字段 |
| ProjectileReplicationSnapshot | D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Snapshots\ProjectileReplicationSnapshot.cs:7-95 | 不映射到权威 Component | partial | owner、UUID、AI、damage、lifetime、section、能力和网络字段过宽，只能作为外部快照 |
| NetworkProjectileSlice | D:\TRbackup\NLTX\dome\src\Terraria.Dome.Protocol.V1456\Isolation\NetworkProjectileSlice.cs:5-43 | 不映射到权威 Component | partial | 已隔离少量协议字段，但不覆盖所有 Version4 网络边界 |
| ProjectileStore、ProjectileIdentityAllocator | D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Projectile\ProjectileStore.cs:7-49、ProjectileIdentityAllocator.cs:5-37 | 不创建 Component；分别是 registry/allocator 候选 | partial | store 只做 Entity→replicationId 映射且反查线性；不是 owner+identity table 等价物 |

## 10. 组件级 evidence-gap

以下缺口不会阻止候选设计输出，但会阻止把设计状态提升为 baseline：

| gapId | 缺口 | 影响的 Component | 已知证据 | 当前状态 |
|---|---|---|---|---|
| GAP-COMP-01 | Version4 owner=255、root EntityReference、dome PlayerHandle 的完整 owner domain 不一致 | Identity、Source、Minion、Sentry、Bobber | V4 Projectile.cs:126,238,362-372；root/dome owner 组件 | unresolved |
| GAP-COMP-02 | slot/whoAmI、identity、projUUID、runtime entity、replicationId 和 persistent ID 的长期映射未闭合 | Identity、Network | V4 Main.cs:944-946；dome snapshot/store | unresolved |
| GAP-COMP-03 | raw ai/localAI 是否长期保留、按何种 behavior key typed 化未闭合；root 只有部分 local slots，dome 只有局部 registry | Trajectory、Definition | V4 Projectile.cs:128-136,18706-18716；dome AI Component | partial |
| GAP-COMP-04 | friendly/hostile 既像定义默认又可能被行为修改；当前 dome 另有 FriendlyState | Definition、DamagePayload | V4 Projectile.cs:148,154；dome FriendlyState | unresolved |
| GAP-COMP-05 | SetDefaults 的全部 type-specific 默认、数组重置和特殊类型覆盖尚未逐 type 闭合 | Definition、Lifetime、Penetration、Collision | V4 Projectile.cs:444-556 | partial |
| GAP-COMP-06 | static NPC immunity 的共享表 owner、索引类型和与 local/owner fallback 的最终组合未闭合 | ImmunityPolicy、ImmunityState | V4 Projectile.cs:92,158-164,11554-11592 | unresolved |
| GAP-COMP-07 | MinionSpawnInfo、IEntitySource 的稳定 value object 和 ItemInstanceId 关系未闭合 | Source、Minion | V4 Projectile.cs:208；dome source component | unresolved |
| GAP-COMP-08 | whip/cone/lance 等特殊几何的完整历史缓存格式和失效条件未逐类闭合 | Geometry、TrailCache、CollisionPolicy | V4 Projectile.cs:13654-13899,282 | partial |
| GAP-COMP-09 | Version4 当前 bobber 方法为空，完整参考的 Fishing outcome 不能直接回填为 Version4 事实 | Bobber、Source | V4 Projectile.cs:34073-34074,47779-47786；full reference:51236-51552 | partial |
| GAP-COMP-10 | LeashedEntity anchor、section 和共享 reference/network ID 的 owner 未闭合；Remove/StreamNetUpdates 为空 | Identity 的跨域 relation | V4 LeashedEntity.cs:44-306 | unresolved |
| GAP-COMP-11 | 未找到 Projectile 持久化/恢复的完整契约，不能定义 persistent ID 默认值或生命周期 | Identity、Lifetime、Source | 研究报告和允许源码范围未闭合 | missing |
| GAP-COMP-12 | alpha、frame、soundDelay、snapshot 中的表现/协议字段是否需要独立非权威视图未裁决 | 不创建权威表现 Component；NetworkState | V4 Projectile.cs:120-140,210-244；dome snapshot | partial |

## 11. 未决组件 owner

每一项都可能改变 Component 组成或字段归属，因此当前会话只提供候选，不宣布最终 owner。

### BD-COMP-01：owner reference domain

- 冲突字段：Version4 owner、npcProj、trap，root ProjectileOwnerComponent.Owner: EntityReference，dome ProjectileNetworkIdentityComponent.Owner: PlayerHandle。
- 当前候选 owner：A 为 Player-only；B 为通用 EntityReference；C 为 owner-kind 加 typed reference。三者的候选 owner 都是 integration-review。
- 组成影响：A 可使 Identity 更简单但无法表达 NPC/trap/world/server；B 需要稳定跨域 reference 生命周期；C 会增加 Identity 的 owner-kind 字段并影响 Source、Minion、Trap 和 Network 的组合。
- 不能裁决原因：当前 Version4 证据明确存在非 Player 来源语义，但没有完整 owner domain registry 和生命周期契约。

### BD-COMP-02：identity、slot、UUID、runtime entity、replication 与 persistent ID

- 冲突字段：whoAmI、identity、projUUID、dome Arch.Entity、replicationId、可能的 persistent ID。
- 当前候选 owner：Identity Component 保存 slot/identity/UUID；NetworkSession 保存 replication key；Persistence 保存 persistent ID；最终共享映射由 integration-review 拥有。
- 组成影响：若 UUID 只是网络兼容字段，Identity 保持四字段；若 UUID 是长期 correlation ID，需要增加稳定关系字段；若 persistent ID 存在，必须新增持久化边界，不能复用 identity。
- 不能裁决原因：允许源码中没有闭合 Projectile persistence contract，且当前 dome Guid? 与 Version4 int projUUID 类型不同。

### BD-COMP-03：raw AI 兼容状态保留期限

- 冲突字段：Version4 ai[3]/localAI[3]、root State0..State3/LocalState0..LocalState1、dome typed ProjectileBehaviorState。
- 当前候选 owner：A 长期保留 raw state；B 按 behavior key 分阶段 typed 化并保留 fallback；C 完整覆盖后删除 raw state。
- 组成影响：A 保持 Trajectory Component 固定字段；B 增加可选行为状态 Component；C 最终可缩小兼容字段但会提高覆盖门槛。
- 不能裁决原因：研究证据只确认 dispatch 和代表性行为，未闭合全部 specialized AI 的读写契约。

### BD-COMP-04：bobber carrier 与 fishing outcome

- 冲突字段：Version4 bobber、miscText、MinionSpawnInfo 邻接来源，dome FishingBobberStateComponent 的 phase/water/fishing/pending item 字段。
- 当前候选 owner：A BobberCapability 只保存 carrier 和最小 context reference；B Fishing 领域拥有全部 context/result；C 短期保留兼容 marker，长期由 Fishing owner 收敛。
- 组成影响：A/B 保持 ProjectileBobberCapability 很小；把 outcome 放回 Projectile 会增加 item、water、fishing 和生成状态，形成跨域巨型 Component。
- 不能裁决原因：Version4 当前 bobber 方法为空，完整参考只能补充同文件缺口，不能证明完整 outcome 属于 Version4 当前行为。

### BD-COMP-05：LeashedEntity anchor/section relation

- 冲突字段：Projectile counterweight/attachment 语义与 LeashedEntity 的 whoAmI、section list、registry 和独立网络模块。
- 当前候选 owner：A LeashedEntitySimulation 拥有 anchor/section，Projectile 只保留 relation reference；B 由共享关系 owner 管理；C 按实体类型分别维护。
- 组成影响：A 保持 CounterweightCapability marker；B 需要共享 reference value object；C 可能新增不同 relation Component。无论方案，shared ID owner 为 integration-review。
- 不能裁决原因：Version4 Remove 与 StreamNetUpdates 为空，关系失效和网络生命周期证据不完整。

### BD-COMP-06：Projectile persistent ID

- 冲突字段：identity/projUUID 是否需要跨世界会话稳定，及 Lifetime/Source 是否需要恢复关系。
- 当前候选 owner：A 不设置 persistent ID；B Persistence 拥有独立 persistent ID；C 将 UUID 作为长期 correlation ID。
- 组成影响：A 不增加 Component；B 需要外部持久化映射且 Identity 保持隔离；C 会扩大 Identity 生命周期并影响 child/source relation。
- 不能裁决原因：允许范围内未找到完整 Projectile 存档/恢复路径，不能猜默认值或寿命。

### BD-COMP-07：dome 宽 Definition 的最终拆分

- 冲突字段：dome ProjectileDefinitionComponent 中的 damage、lifetime、collider、collision、immunity、network、minion/sentry/trap/bobber 和 child spawn 字段。
- 当前候选 owner：A 按本文件 Definition/Damage/Policy/Capability/Source 拆分；B 保留小型 immutable definition 并把所有实例字段迁出；C 按 content catalog 与 instance state 两层拆分。
- 组成影响：A 保持 18 个候选 Component；B/C 可能引入独立内容 value object，但都必须禁止运行时状态回流 Definition。
- 不能裁决原因：当前源码显示字段已存在但没有完整写者/恢复/跨领域 owner 矩阵，不能宣布哪一列属于最终内容 catalog。

### BD-COMP-08：friendly/hostile 当前状态 owner

- 冲突字段：Version4 friendly、hostile，root Definition flags，dome ProjectileFriendlyStateComponent。
- 当前候选 owner：A 只在 Definition 保存生成默认；B 增加独立当前 disposition Component；C 将 current flags 与 Definition 保持同一 Component 但明确可变。
- 组成影响：A 最小但要求所有运行时改写有其他归属；B 增加可选/必需状态列；C 保留巨型 Definition 风险。
- 不能裁决原因：当前会话未完成全部 AI 对两个 flag 的写入盘点，不能在 Definition-only 与独立 state 之间定案。

## 12. 最终 Component 清单

| componentId | name | status | 当前 NLTX 覆盖 | evidenceStatus |
|---|---|---|---|---|
| BD-COMP-PROJ-01 | ProjectileIdentityComponent | proposed | dome identity + root owner partial | partial |
| BD-COMP-PROJ-02 | ProjectileDefinitionComponent | partial | root/dome existing，dome 过宽 | partial |
| BD-COMP-PROJ-03 | ProjectileTrajectoryStateComponent | proposed | root/dome behavior partial | partial |
| BD-COMP-PROJ-04 | ProjectileLifetimeComponent | partial | root/dome existing | confirmed |
| BD-COMP-PROJ-05 | ProjectileDamagePayloadComponent | proposed | root damage partial，dome 混入 Definition | partial |
| BD-COMP-PROJ-06 | ProjectilePenetrationStateComponent | proposed | root/dome penetration partial | partial |
| BD-COMP-PROJ-07 | ProjectileHitImmunityStateComponent | proposed | dome hit/restrike partial | partial |
| BD-COMP-PROJ-08 | ProjectileHitImmunityPolicyComponent | proposed | dome Definition partial | partial |
| BD-COMP-PROJ-09 | ProjectileCollisionPolicyComponent | proposed | dome Tile/FallThrough/Bounce partial | partial |
| BD-COMP-PROJ-10 | ProjectileGeometryStateComponent | proposed | dome Reflection/scale partial | partial |
| BD-COMP-PROJ-11 | ProjectileNetworkStateComponent | proposed | dome network components partial | partial |
| BD-COMP-PROJ-12 | ProjectileSourceMetadataComponent | proposed | dome Banner/Misc/MinionSource partial | partial |
| BD-COMP-PROJ-13 | ProjectileTrailCacheComponent | proposed | 未见完整对应组件 | confirmed |
| BD-COMP-PROJ-14 | ProjectileSentryCapabilityComponent | proposed | dome marker existing | partial |
| BD-COMP-PROJ-15 | ProjectileMinionCapabilityComponent | proposed | dome minion/summoned existing | partial |
| BD-COMP-PROJ-16 | ProjectileTrapCapabilityComponent | proposed | dome marker existing | partial |
| BD-COMP-PROJ-17 | ProjectileBobberCapabilityComponent | proposed | dome carrier + Fishing state partial | partial |
| BD-COMP-PROJ-18 | ProjectileCounterweightCapabilityComponent | proposed | dome marker existing | partial |

最终候选组成不包含独立的 Presentation Component、persistent ID Component、replication snapshot Component、WorldGrid Component 或 LeashedEntity Component；这些对象的 owner 或状态范围不同，分别在第 8 节和第 11 节记录。

## 13. 最终声明

本文件是 Component-only Design。
本文件不定义 System、Query、Command、Event、Adapter、Projection、
调度顺序、运行时实现、测试计划或迁移计划。

所有 status: proposed 的 Component 都只是设计提案。
本文件不声明代码已创建、已迁移、行为等价或验证通过。

verificationStatus: not-run 保持不变。本文件生成过程中未修改 Version4、完整参考源码、tModLoader 文档、Space Station 14、NLTX 源码、测试、公共协议或源研究报告；未运行构建、测试或其他编译型命令，未启动子代理。
