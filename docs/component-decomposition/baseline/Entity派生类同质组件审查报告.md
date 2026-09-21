# Entity 派生类同质组件审查报告

## 结论摘要

本次审查以只读参考源码 `D:\TRbackup\Version4\Terraria` 中的
`Entity`、`Player`、`Projectile`、`NPC` 为字段语义来源，并对照当前
`dome/src/Terraria.Dome.Simulation` 的 Arch ECS 实现。

结论是：`Entity` 的一组空间/运动/碰撞字段已经在当前实现中形成了真正的
同质组件族；`Player` 与 `NPC` 的生命、朝向、物理和生命周期字段存在高重叠，
适合共享“通用状态组件”，但生命周期不能直接合并成一个 `TimeLeft` 组件；
`Projectile` 与 `Player`/`NPC` 共享空间、速度、碰撞、朝向和部分网络概念，
但伤害、所有者、AI、穿透和过期策略是投射物特有状态。建议保留现有组合式
边界，仅补齐几个明确的公共组件/投影 seam，不建立新的 `BaseEntityComponent`
或把所有字段重新聚合成巨型组件。

当前实现已经具备较好的分解结果：

- `TransformComponent`、`VelocityComponent`、`ColliderComponent`、`FacingComponent`
  被 Player、NPC、Projectile 的创建路径共同使用。
- `HealthComponent` 被 Player 与 NPC 共同使用，Projectile 不应挂载该组件。
- `PhysicsStateComponent` 目前在 Player/NPC 创建时使用；Projectile 的移动由
  `TransformComponent + VelocityComponent` 和专用碰撞/生命周期系统驱动，不应为了
  名称相似而强行添加完整物理状态。
- `PlayerLifecycleComponent`、`NpcLifecycleComponent`、`ProjectileLifetimeComponent`
  都表达“可活动/时间推进”，但更新原因、终止语义、持久化和事件不同，不是同质组件，
  应通过统一查询/协议抽象而非共享可变存储来复用。

## 1. 证据与范围

### 1.1 来源记录

| source | version | query | hits/evidence | gaps | stop_reason |
| --- | --- | --- | --- | --- | --- |
| `D:\TRbackup\Version4\Terraria\Entity.cs` | 本地参考版本 | `Entity` 公共字段/属性 | `whoAmI`、`position`、`velocity`、历史位置/速度、`direction`、`width`、`height`、液体状态、几何属性 | 参考实现不是当前权威运行时 | 已获得完整基类字段语义 |
| `D:\TRbackup\Version4\Terraria\Player.cs` | 本地参考版本 | `Player : Entity` 重叠字段 | `active`、`rotation`、`timeLeft`、`gfxOffY` 以及输入、生命、背包、网络等大量玩家专属字段 | 参考文件是生成/兼容源码，不能直接修改 | 与当前 Player 创建路径交叉确认 |
| `D:\TRbackup\Version4\Terraria\Projectile.cs` | 本地参考版本 | `Projectile : Entity` 重叠字段 | `active`、`rotation`、`timeLeft`、`gfxOffY`、`stepSpeed`、`netUpdate`、`netSpam`、伤害/所有者/AI/穿透字段 | 预测/表现数组的权威性需按系统确认 | 与投射物生成、复制、生命周期系统交叉确认 |
| `D:\TRbackup\Version4\Terraria\NPC.cs` | 本地参考版本 | `NPC : Entity` 重叠字段 | `active`、`rotation`、`timeLeft`、`gfxOffY`、`stepSpeed`、`netUpdate`、`netSpam`、生命/防御/目标/AI 字段 | NPC 的 `timeLeft` 同时受自然消失策略影响 | 与 NPC 生成、生命周期、复制组件交叉确认 |
| `D:\TRbackup\tmodloader-api-docs-stable\class_entity.html` | 本地 stable API 镜像 | Entity 字段语义 | 文档确认 `position` 为左上角世界坐标、`velocity` 为每 tick 速度、`direction` 为朝向、`width/height` 为 hitbox 尺寸、`whoAmI` 为类型数组索引且跨客户端不稳定 | 文档未定义本仓库 ECS 所有权 | 关键基类字段语义已确认 |
| `D:\TRbackup\tmodloader-api-docs-stable\class_player.html` | 本地 stable API 镜像 | Player 公开 API/状态 | 文档确认 Player 有生命/伤害/输入/网络同步等独立语义 | 具体版本与本地参考版本可能漂移 | 仅用于语义交叉检查，不复制 API |
| `D:\TRbackup\tmodloader-api-docs-stable\class_projectile.html` | 本地 stable API 镜像 | Projectile 公开 API/状态 | 文档确认 Projectile 的 AI、伤害、所有者、穿透、网络更新、额外更新等专属语义 | 具体版本与本地参考版本可能漂移 | 仅用于语义交叉检查 |
| `D:\TRbackup\tmodloader-api-docs-stable\class_n_p_c.html` | 本地 stable API 镜像 | NPC 公开 API/状态 | 文档确认 NPC 的生命、目标、AI、增益、生成/消失等专属语义 | 具体版本与本地参考版本可能漂移 | 仅用于语义交叉检查 |
| `C:\Users\shan\Downloads\ECS\space-station-14-master` | 只读 SS14 | 组件粒度、System/Query 边界 | 采用按能力挂载组件、System 读取组件并显式改变状态的组织模式 | 不用于确认 Terraria 字段含义 | 仅支持组织模式判断 |

### 1.2 当前实现的关键创建证据

- Player 创建路径：`dome/src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs:2709-2733`
  同时挂载身份、Transform、Velocity、Facing、Collider、Physics、Health、Defense、
  Immunity 和 PlayerLifecycle。
- NPC 创建路径：`dome/src/Terraria.Dome.Simulation/Npc/Systems/NpcSpawnCommitSystem.cs:92-145`
  同时挂载 Transform、Velocity、Facing、Collider、Physics、Health、Defense、Immunity、
  NPC 行为/生命周期/复制等组件。
- 玩家投射物创建路径：`dome/src/Terraria.Dome.Simulation/Projectile/Systems/ProjectileSpawnSystem.cs:65-173`
  挂载 Projectile 定义、行为、网络身份、Transform、Velocity、方向、碰撞、伤害、穿透、
  生命周期和复制相关组件。
- NPC 投射物创建路径：`dome/src/Terraria.Dome.Simulation/Projectile/Systems/NpcProjectileSpawnSystem.cs:35-150`
  使用与玩家投射物相同的运动/碰撞/伤害/生命周期组件，但所有者和网络身份换为 NPC 语义。

## 2. 参考继承字段的重叠盘点

下表中的“参考字段”是 `Version4` 的 `Entity`/派生类字段；“当前归属”是本仓库
ECS 中的实际组件或明确的暂缓项。

| 概念 | Entity | Player | Projectile | NPC | 当前 ECS 归属 | 同质性判断 |
| --- | --- | --- | --- | --- | --- | --- |
| 实体数组索引/运行时句柄 | `whoAmI` | 继承 | 继承 | 继承 | Arch `Entity`；Player/Npc/Projectile 各自 Handle/Store；网络复制 ID 分开 | **部分同质**。实体句柄不能与持久化/网络 ID 合并 |
| 世界位置 | `position` | 继承 | 继承 | 继承 | `TransformComponent` | **确认同质**。三类都由 Movement/碰撞/投影读取 |
| 当前速度 | `velocity` | 继承 | 继承 | 继承 | `VelocityComponent` | **确认同质**。同一 MovementSystem 可处理；行为写入仍由专属 System 负责 |
| 历史位置/速度 | `oldPosition`、`oldVelocity` | 继承 | 继承 | 继承 | 当前没有公共历史组件；复制快照/局部行为按需保存 | **不宜默认同质**。Projectile extra updates 与 NPC/Player tick 语义不同 |
| 朝向 | `direction`、`oldDirection` | 继承/玩家输入 | `spriteDirection` 等 | 继承并有 `directionY` | `FacingComponent`；Projectile 另有 `ProjectileDirectionComponent`；NPC MovementState 保存 Y 方向 | **部分同质**。水平朝向可共享，Y 轴与精灵方向不可直接合并 |
| Hitbox 尺寸 | `width`、`height` | 继承 | 继承 | 继承 | `ColliderComponent` | **确认同质**。几何查询只依赖 Transform+Collider |
| 液体接触 | `wet`、`lavaWet` 等 | 继承并有玩家环境规则 | `ignoreWater` 等 | 继承并有移动速度/免疫 | Player `PlayerEnvironmentContactComponent`；Projectile 定义/系统中的液体策略；NPC `NpcMovementStateComponent`/策略 | **不确认同质**。同名“湿”是不同规则输入，禁止建公共 WetComponent |
| 活动标志 | 派生/无统一基类字段 | `active` | `active` | `active` | Player/NPC 生命周期组件；Projectile 由 `ProjectileLifetimeComponent`/Store 推导 | **概念相似但非同质存储**。建议统一 `IsActive` 查询，而非共享字段 |
| 时间倒计时 | 无 | `timeLeft`（Player 内部/表现也有同名） | `timeLeft` | `timeLeft`（自然消失） | PlayerLifecycle、ProjectileLifetime、NpcLifecycle | **不确认同质**。终止原因与恢复语义不同 |
| 旋转 | 派生/表现 | `rotation` | `rotation` | `rotation` | 当前未建立权威公共组件；复制快照按需表达 | **仅表现层相似**。需先确认是否权威，再考虑 `OrientationComponent` |
| 垂直绘制偏移 | 无 | `gfxOffY` | `gfxOffY` | `gfxOffY` | 当前未建模；应归表现/投影 | **不属于模拟同质组件** |
| 步进速度 | 无 | 继承/局部移动 | `stepSpeed` | `stepSpeed` | Projectile `ProjectileStepSpeedComponent`；NPC `NpcMovementStateComponent.StepSpeed`；Player 无同名权威组件 | **部分同质**。数值用途接近但更新者不同；保留分域组件 |
| 生命/最大生命 | 无 | Player 生命字段 | 无 | NPC `life`/`lifeMax` | `HealthComponent`（Player+NPC）；Combat System 写入 | **确认同质（Player/NPC）**。Projectile 不应挂载 |
| 防御 | 无 | 玩家装备计算后的防御 | 无 | `defense`/`defDamage` 等 | `DefenseComponent`；玩家装备系统/NPC 定义分别提供输入 | **状态可共享，来源不可共享** |
| AI 状态数组/样式 | 无 | 玩家控制/技能状态 | `ai`/`localAI`/`aiStyle` | `ai`/`localAI`/`aiStyle` | ProjectileBehavior；NpcAiState/NpcBehavior；Player ControlInput/技能组件 | **不确认同质**。都是“状态机”但状态转移和生命周期不同 |
| 伤害/击退/穿透 | 无 | 玩家攻击计算 | Projectile 专属 | NPC 接触/远程攻击 | ProjectileDamage、ProjectilePenetration；NPC CombatState；Player 命令/装备投影 | **不确认同质**。可共享纯计算策略，不共享组件字段 |
| 所有者/目标 | 无 | 玩家身份、NPC 目标 | 玩家/NPC 投射物所有者 | NPC 目标、玩家交互 | `ProjectileOwnerComponent`、`NpcProjectileOwnerComponent`、NpcTarget、PlayerTargeting | **关系概念相似但类型不同**；实体引用、网络 ID、账户 ID 必须分离 |
| 网络更新/复制 | 无 | Player 复制游标/快照 | `netUpdate`/`netSpam` | `netUpdate`/`netSpam` | PlayerReplicationCursor/State、ProjectileNetworkUpdate、NpcReplication | **协议目标相似，状态所有权不同**；不创建公共 NetUpdateComponent |
| 增益/免疫 | 液体等基础输入 | 玩家 Buff/免疫 | 投射物命中免疫 | NPC Buff/免疫 | BuffCollection、Immunity、HitImmunity 及 Player/NPC 专属状态 | **部分同质**。效果容器可复用，免疫时间键空间不同 |

## 3. 成员归属与状态所有权表

状态类型按 public-decomposition 约定标记为：权威状态、派生值、缓存/游标、快照/投影、
适配/兼容或暂缓。这里的 `confirmed` 表示有本地创建/写入/读取证据；`partial` 表示
只有参考源码或命名证据；`missing` 表示不应在本报告中假定其权威性。

| 成员/概念 | 读者/写者 | 生命周期与副作用 | 状态类型 | 候选边界 | 证据状态 |
| --- | --- | --- | --- | --- | --- |
| Transform X/Y | Movement、碰撞、复制、快照；Movement/生成/恢复写入 | 实体创建至销毁，每 tick 可变 | 权威状态 | `TransformComponent` | confirmed |
| Velocity X/Y | Movement、行为、碰撞；Player/NPC/Projectile 行为写入 | 创建初始化，行为和碰撞更新 | 权威状态 | `VelocityComponent` | confirmed |
| Collider Width/Height | Geometry、碰撞、生成；定义/生成写入 | 创建时确定，少量恢复/变体调整 | 权威状态 | `ColliderComponent` | confirmed |
| Horizontal Facing | PlayerControl、NpcBehavior、ProjectileDirection、复制 | 行为 tick 更新 | 权威状态 | `FacingComponent`（投射物保留专用方向适配） | confirmed |
| Health Current/Maximum | Combat、死亡、快照；伤害/恢复系统写入 | Player/NPC 生命周期 | 权威状态 | `HealthComponent` | confirmed |
| Player IsActive/IsDead/Respawn | Player lifecycle/respawn/death systems | 可死亡、可复活；有事件副作用 | 权威状态 | `PlayerLifecycleComponent` | confirmed |
| NPC IsActive/TimeLeft/DespawnReason | NPC lifecycle/death/spawn systems | 自然消失、击杀、段节联动 | 权威状态 | `NpcLifecycleComponent` | confirmed |
| Projectile RemainingTicks | Projectile lifetime/collision/replication | 到期产生 tombstone/despawn | 权威状态 | `ProjectileLifetimeComponent` | confirmed |
| Projectile/NPC/Player ReplicationId | 复制系统、网络投影、游标 | 网络会话范围；不能代替实体句柄 | 网络权威/游标 | 各自 replication component/cursor | confirmed |
| rotation、gfxOffY、oldPos、oldRot | 表现、插值或兼容层（当前 ECS 未统一读写） | 可能是客户端表现或历史缓存 | 表现/缓存 | Projection/Presentation；暂不进入核心组件 | partial |
| wet/shimmerWet/lavaWet/honeyWet | 参考 Entity；当前分域环境系统 | 不同实体的液体规则不同 | 外部输入/派生状态 | Player/NPC/Projectile 各自策略 | partial |
| `whoAmI` | 参考数组索引；当前 Store/Handle/Replication ID | 网络环境下不稳定 | 兼容索引/运行时句柄 | Adapter + typed Handle | confirmed |

## 4. 同质组件判定

### 4.1 应视为同质、继续共享的组件

1. **TransformComponent**：三类实体均以世界坐标位置参与几何查询和移动。接口最小，
   可替换 seam 是 `MovementSystem` 与 `EntityGeometryQuery`。
2. **VelocityComponent**：三类实体均以每 tick 速度表达运动输入。写者不同，但数据语义
   一致；`MovementSystem` 只读写 Transform/Velocity，不需要知道实体种类。
3. **ColliderComponent**：宽高均作为 hitbox 几何输入。Projectile 定义在生成时缩放后
   写入，Player/NPC 从生成定义写入；不应把碰撞响应混入组件。
4. **FacingComponent（水平分量）**：Player 与 NPC 以及多数 Projectile 都有水平朝向。
   Projectile 的 `ProjectileDirectionComponent` 作为旧 API/特殊方向适配，不要强迫 NPC
   的 Y 方向进入同一组件。
5. **HealthComponent（仅 Player + NPC）**：两者都由 Combat System 维护 current/maximum，
   都触发死亡/受伤事件。Projectile 的“damage”是输出能力，不是自身生命。

### 4.2 语义相似但不应合并的组件

- `IsActive`：Player 可 respawn，NPC 有自然 despawn，Projectile 过期后保留 tombstone；
  合并会丢失终止原因和清理顺序。
- `TimeLeft`：名称相同但 Player 的 `timeLeft` 还可能与表现/内部计时重名；NPC 的
  `EncourageDespawn/DiscourageDespawn` 改写策略与 Projectile 的严格非负 lifetime 不同。
- `netUpdate/netSpam`：网络目标类似，但 Player、NPC、Projectile 的复制快照字段和游标
  生命周期不同；统一字段会将协议状态和实体状态耦合。
- `rotation/gfxOffY/oldPos/oldRot`：参考类中的公共字段并不证明它们是服务器权威模拟状态；
  在当前 ECS 没有稳定写者/读者前，只能放在表现投影或兼容适配层。
- 液体字段：`wet` 等是基类名义上的共性，但 Player/NPC/Projectile 的速度、免疫、穿透
  与忽略水规则不同，应用层策略才是共享点。

## 5. 模块边界与调用方向

```text
Spawn Command/Definition
        |
        v
  Spawn System  ----> typed Handle/Store (运行时索引，不是网络 ID)
        |
        +--> Transform + Velocity + Collider + Facing
        |          |
        |          v
        |      Movement / Collision Systems
        |
        +--> Health (Player/NPC only) --> Combat / Death Systems
        |
        +--> Domain lifecycle (Player/NPC/Projectile 各自组件)
        |
        +--> Replication component/cursor --> Projection --> Terraria packet adapter
```

系统顺序建议继续显式保持：

1. 输入/行为系统产生意图或速度/朝向变更；
2. Movement/碰撞系统提交 Transform 变更；
3. Combat/生命周期系统消费命中、伤害、过期和死亡；
4. Replication 系统读取稳定状态生成快照；
5. 协议 Adapter 将快照编码为 Terraria 网络包。

复制和协议模块不得反向写入 Transform、Health 或生命周期组件；若收到客户端意图，
应先形成 Command，由权威 System 验证后写入。

## 6. 最小接口、Seam、Depth/Leverage/Locality

| 模块 | Interface（最小接口） | Implementation 隐藏内容 | Seam | Depth | Leverage/Locality |
| --- | --- | --- | --- | --- | --- |
| Transform/Velocity/Collider 状态 | `Get/Set` 对应单组件；Movement 只依赖三者 | Arch 存储、结构变更 | `MovementSystem`、`EntityGeometryQuery` | 中等：统一运动/几何语义 | 高复用；空间规则集中 |
| Health + Combat | `ApplyDamage`、`Restore`、`IsDead` 查询/命令 | 防御、免疫、难度缩放、死亡事件 | `DamageCommand`、Combat System | 深：调用方无需知道伤害细节 | Player/NPC 共享读写协议 |
| PlayerLifecycle | `Activate/MarkDead/BeginRespawn` 命令或 System | 复活计时、出生点、死亡掉落 | PlayerDeath/Respawn System | 深：封装复活不变量 | 规则集中在 Player 域 |
| NpcLifecycle | `EncourageDespawn/DiscourageDespawn/MarkKilled` | 自然消失、段节、替换和槽位 | NpcLifecycleSystem | 深：保留 NPC 特殊终止语义 | 不污染 Player/Projectile |
| ProjectileLifetime | `Advance`、`IsExpired`、tombstone 输出 | extra updates、过期原因、复制保留窗口 | ProjectileLifetimeSystem | 中等 | 投射物生命周期集中 |
| Replication Projection | 输入各域 snapshot，输出协议 record | revision、PVS、节流、兼容版本 | `PlayerStateProjection`、`NpcReplicationAssembler`、`ProjectileStateProjection` | 深：隔离网络协议 | 降低核心域与 Socket/编码耦合 |

## 7. 建议的增量动作

### P0：保持现状并补验证（推荐）

- 为 `Transform + Velocity + Collider` 增加跨三类实体的 focused verifier，验证移动、
  中心点/Hitbox 和碰撞边界完全一致。
- 为 `HealthComponent` 增加 Player/NPC 共享伤害/死亡测试，明确 Projectile 不具备 Health。
- 为三个生命周期组件分别验证：Player respawn、NPC despawn reason、Projectile expiry/tombstone。
- 在复制投影测试中断言 typed Handle、ReplicationId、网络/持久化 ID 不混用。

### P1：只抽取纯查询/策略，不抽取共享可变状态

- 可增加 `EntityActivityQuery`（纯查询）：根据实体类型已有生命周期组件返回 `IsActive`，
  但不得写状态或持有全局可变集合。
- 可增加 `EntityGeometryQuery` 的统一入口，继续把中心点、Hitbox、距离计算留在 Query，
  不把 `Center/Hitbox` 作为可写缓存字段。
- 如确有多个系统需要历史位置，先定义 tick/extra-update 采样契约，再设计独立
  `MotionHistoryComponent`；在证据不足前不要添加。

### P2：兼容字段隔离

- 对 `whoAmI`、`rotation`、`gfxOffY`、`oldPos`、`oldRot` 等参考字段，使用 Adapter/Projection
  记录来源和失效条件；禁止直接映射成新的公共权威组件。
- 保留 `ProjectileOwnerComponent` 与 `NpcProjectileOwnerComponent` 的类型区分；若需要统一
  查询，提供只读 `OwnerRelationQuery`，而非把 PlayerHandle/NpcHandle 变成一个无类型整数。

## 8. 明确不拆分项与兼容策略

### 不拆分项

- 不创建包含 Transform、Velocity、Collider、Health、Lifecycle、Replication 的
  `BaseEntityComponent`：这会重新形成巨型基础组件，破坏 ECS 查询局部性。
- 不把 Player/NPC/Projectile 的 `timeLeft` 合并为 `GenericLifetimeComponent`：不同的
  终止事件、恢复、tombstone 保留和网络发送窗口无法由一个字段表达。
- 不把 AI 状态数组统一为 `AiComponent`：Projectile 的 `ai/localAI` 与 NPC 行为阶段及
  Player 输入技能不是同一状态机。
- 不把表现历史（oldPos/oldRot/gfxOffY）放入服务器权威组件，除非先补齐所有写者、读者、
  tick 采样与持久化/网络契约证据。

### 兼容策略

- 参考继承类的公共字段只通过兼容 Adapter/Projection 访问；内部核心代码使用 typed
  component 和 typed Handle。
- `whoAmI` 仅视为参考数组索引/兼容索引；网络复制使用各域的 ReplicationId，持久化使用
  明确的持久化标识，外部账户使用账户 UUID，三者不复用。
- 新组件迁移必须先保留旧投影字段，完成 focused verifier 后再删除兼容字段；迁移期间禁止
  双向隐式同步，统一由权威 System 产生投影。

## 9. 行为保持风险

| 风险 | 触发条件 | 缓解措施 |
| --- | --- | --- |
| Transform/Collider 单位不一致 | Player/NPC 使用世界单位，参考 Terraria hitbox 使用像素语义 | 在 Geometry Query 入口固定单位转换，并用边界测试锁定 |
| Projectile extra updates 改变历史采样 | 将 `oldPosition/oldVelocity` 机械抽成公共组件 | 明确 tick 与 extra-update 采样时机，未验证前保持投射物专用 |
| 生命周期清理顺序变化 | 合并 IsActive/TimeLeft 后共享系统 | 分域生命周期 System，测试死亡、despawn、tombstone 顺序 |
| 网络 ID 串用 | 将 `whoAmI` 当作 replication/persistence ID | 使用 Player/Npc/Projectile typed Handle 与独立 ReplicationId |
| 表现字段反向驱动模拟 | Projection 字段被系统直接读取写回 | 只允许 Projection 单向输出，核心系统不依赖 gfx/rotation 缓存 |
| 伤害语义泄漏 | 给 Projectile 添加 Health 或让 HealthComponent 承担 damage | 保持 ProjectileDamage/ Penetration 与 Health 分离，使用 Combat Policy 共享纯规则 |

## 10. Focused verifier / 测试计划与实际结果

### 计划

1. **几何/运动 verifier**：分别创建 Player、NPC、玩家 Projectile、NPC Projectile，验证
   Transform/Velocity/Collider 的移动、中心点、Hitbox、零速度和边界输入。
2. **共享生命 verifier**：Player/NPC 施加伤害、免疫、死亡；断言 Projectile 没有 Health。
3. **生命周期 verifier**：Player respawn、NPC `DespawnReason`、Projectile expiry/tombstone
   各自验证时间推进、清理和复制输出。
4. **关系与 ID verifier**：验证实体句柄、复制 ID、网络 UUID、账户 UUID 在投影和恢复过程
   中保持类型/值域隔离。
5. **纯查询 verifier**：验证几何/活动查询不修改组件、不持有全局可变状态。

### 实际结果

- 已完成只读源码盘点、当前创建路径交叉核对、SS14 组织模式参考和 tModLoader Entity/
  Player/Projectile/NPC 文档访问（HTTP 200）。
- 未修改 C# 实现，因此没有需要运行的编译型测试；本报告不宣称运行时行为已通过。
- 现有仓库测试中已发现相关验证入口（例如 `Terraria.Dome.Combat.Verification`、
  `Terraria.Dome.PlayerSimulation.Verification`、`Terraria.Dome.ProjectileHostile.Verification`），
  但本次仅做审查，未执行 `dotnet` 命令。

## 11. 最终判定

当前 ECS 的公共 Entity 组件边界总体正确。应确认并继续复用的同质组件是：

`TransformComponent`、`VelocityComponent`、`ColliderComponent`、水平朝向部分的
`FacingComponent`，以及仅限 Player/NPC 的 `HealthComponent`。

应保持分域的组件是：

`PlayerLifecycleComponent`、`NpcLifecycleComponent`、`ProjectileLifetimeComponent`、
各域 Replication 状态、Projectile 行为/伤害/所有者状态、NPC AI/目标状态和 Player 输入/技能状态。

本次不建议立即实施字段迁移；先按第 10 节补 focused verifier，再以单组件、单边界、
可回滚的方式推进任何后续重构。

## 12. 统一身份根补充审查（grill-with-docs 决策结果）

针对“替代 `whoAmI`、Projectile UUID 以及其他游戏系统唯一身份”的扩展要求，补充结论如下：

| 身份概念 | 当前证据 | 同质性判定 | 目标边界 |
| --- | --- | --- | --- |
| `EntityUuid` | Share `EntityIdentityComponent`；权威实体创建路径 | **确认同质（权威实体实例）** | Share 公共组件；服务器 Spawn/Commit 生成 |
| Arch `Entity` | Arch 当前进程行句柄 | **不合并** | 运行时临时句柄 |
| `PlayerHandle` / `NpcHandle` | Simulation typed record struct，值域为域内索引 | **不合并为裸 ID** | 保留 typed projection，经 Registry 映射 |
| Projectile `Identity` | Projectile replication consumer 按 owner + identity 校验 | **不作全局身份** | `ProjectileProtocolIdentity`，协议 Adapter 作用域 |
| Projectile UUID | Projectile network identity 组件中的可空 UUID | **不保留第二权威根** | 兼容/协议投影映射到 `EntityUuid` |
| `ReplicationId` | Session consumer/persistence cursor 按域使用 | **不作全局身份** | 各域 replication projection |
| Account UUID | Player identity component 的 canonical account 字段 | **不作实体实例身份** | `AccountUuid`，跨连接账户作用域 |
| `whoAmI` | Legacy MessageBuffer 连接槽位/数组索引 | **兼容索引** | 仅 Compatibility Adapter |

已确认的生命周期和安全约束：`EntityUuid` 由服务器唯一生成，绑定一个实体实例，销毁/重建即失效；
预测、表现和协议临时对象不挂载根组件。Registry 只管理根身份登记、解析、冲突检测和清理；
各 Adapter 拥有自己的 typed projection。`EntityUuid` 不进入现有 Terraria 协议，也不作为默认存档主键。
迁移采用单向投影，禁止新旧字段隐式双写。解析失败必须显式返回 `NotFound`、`Expired`、
`ScopeMismatch` 或 `Conflict` 等 typed 结果，按命令、事件、快照和持久化边界分别拒绝、隔离、
重同步或标记修复失败。
