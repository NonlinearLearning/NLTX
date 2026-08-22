# Projectile ECS 迁移执行提案

> 状态：提案，未执行代码迁移。范围默认采用 Projectile-first，并以协议兼容作为第一阶段闸门。

## 1. 目标与约束

将 `D:\TRbackup\Version4物理删除了某些文件\Terraria\Projectile.cs` 中的投射物职责迁移到
`Terraria.Dome.Simulation` 的 ECS 组件和系统，最终移除投射物 God Object 对 `Main`、旧
`Player`/`NPC`、网络写入、音效和绘制的直接依赖。

本提案不把旧类按方法切成多个“Projectile 子类”。新模型使用实体组合：实体身份、通用运动
能力、投射物定义、行为状态、所有者、伤害和生命周期分别由组件表达，系统按显式顺序读取组件
并产生命令/事件。`Version4...Projectile.cs` 是只读事实源，不在迁移前物理删除、重命名或改写。

仓库约束：新增 C# 必须遵守 `约束/Google-CSharp-Style-Guide-约束.md`；构建使用仓库根目录的
`.NET 10` SDK 和 `-p:UseSharedCompilation=false`；生成物只能进入 `Build/`。

## 2. 现状证据

迁移事实源 `Projectile.cs` 当前约 54,800 行、约 1.44 MB，包含：

- 约 1,133 个 `aiStyle` 使用点和 14 个 `AI_###` 行为族入口；
- 约 6,203 个 `Main.` 引用，说明全局状态、实体存储和规则高度耦合；
- 约 619 个音频/音效引用、约 90 个 `NPC` 引用、约 65 个 `Player` 引用；
- `SetDefaults`、`NewProjectile`、`Update`、`AI`、`Damage`、`Kill`、碰撞参数和网络状态集中在同一类型。

当前仓库已有可复用的最小切片：

- `src/Terraria.Dome.Simulation/Components/ProjectileTagComponent.cs`
- `ProjectileOwnerComponent.cs`、`ProjectileDamageComponent.cs`、`ProjectileLifetimeComponent.cs`
- `src/Terraria.Dome.Simulation/Projectile/Systems/ProjectileCollisionSystem.cs`
- `Snapshots/ProjectileSnapshot.cs`、`ProjectileReplicationSnapshot.cs`
- `Commands/SpawnProjectileCommand.cs` 和 `DomeSimulation` 中的生成、移动、命中、销毁闭环。

这些代码证明 ECS 运行时边界已经存在，但当前投射物仍是单一行为族和单一伤害模型，不能声称已
覆盖旧文件的全部语义。

## 3. 目标架构

### 3.1 组件分层

通用组件由多个实体共享；投射物组件只表达投射物能力，不保存全局集合或协议字节布局。

| 组件 | 主要字段/职责 | 来源字段或语义 |
| --- | --- | --- |
| `EntityIdentityComponent` | ECS/稳定实体标识 | `Entity` 槽位语义 |
| `TransformComponent` | 位置 | `position` |
| `VelocityComponent` | 速度 | `velocity` |
| `ColliderComponent` | AABB/碰撞形状 | `width`、`height`、碰撞扩展 |
| `ProjectileTagComponent` | 投射物查询标记 | `Projectile` 类型身份 |
| `ProjectileDefinitionComponent` | 类型、行为定义 ID、默认规则 | `type`、`SetDefaults` 的静态部分 |
| `ProjectileNetworkIdentityComponent` | `owner`、`identity`、可选 `projUuid` | 消息 27/29 匹配键 |
| `ProjectileOwnerComponent` | 逻辑所有者句柄和阵营来源 | `owner`、来源实体 |
| `ProjectileBehaviorComponent` | 有类型行为状态、计时器、阶段和目标 | `ai[]`、`localAI[]`、`aiStyle` 的运行态 |
| `ProjectileLifetimeComponent` | 剩余 tick、额外更新次数、销毁原因 | `timeLeft`、`extraUpdates` |
| `ProjectileDamageComponent` | 当前/原始伤害、击退、伤害类别 | `damage`、`originalDamage`、`knockBack` |
| `ProjectilePenetrationComponent` | 当前/最大穿透、命中后规则 | `penetrate`、`maxPenetrate` |
| `HitImmunityComponent` | NPC/Player 局部和静态免疫计时 | `localNPCImmunity`、`playerImmune` |
| `ProjectileCollisionComponent` | 是否撞砖、液体、斜坡、穿透规则 | `tileCollide` 等碰撞开关 |
| `ProjectileFactionComponent` | friendly/hostile、反射和目标过滤 | `friendly`、`hostile`、`reflected` |
| `MinionComponent` / `SentryComponent` | 召唤槽位、驻留和 owner 约束 | `minion`、`sentry`、相关字段 |
| `WhipComponent` / `GrappleComponent` | 鞭子控制点、抓钩连接和特殊命中 | 特殊投射物行为族 |
| `ProjectilePresentationComponent` | 旋转、帧、光照、拖尾等渲染投影数据 | `rotation`、`frame`、`oldPos` 等 |

`ai[0..2]` 只允许出现在 `Protocol/Projectile` 的兼容投影中。行为系统内部必须使用
`ProjectileBehaviorState` 等有类型状态，不能把无类型数组继续扩散成公共 API。

### 3.2 系统与显式 tick 顺序

系统注册表必须显式声明顺序，禁止依赖目录枚举或反射顺序：

1. `ProjectileSpawnSystem`：消费 `SpawnProjectileCommand`，根据定义创建组件集合和网络身份。
2. `ProjectileDefinitionSystem`：解析类型定义，填充默认碰撞、伤害、生命周期和行为状态。
3. `ProjectileBehaviorSystem`：按 `BehaviorId` 执行有类型行为，产生目标/速度/特效命令。
4. `ProjectileOwnerConstraintSystem`：处理 owner 有效性、minion/sentry 槽位和阵营来源。
5. `MovementSystem`：推进位置，记录上一位置用于扫掠检测。
6. `ProjectileCollisionSystem`：处理砖块、液体、斜坡和特殊形状碰撞。
7. `ProjectileTargetQuerySystem`：从 NPC/Player 空间索引得到候选目标，不直接访问旧全局数组。
8. `ProjectileDamageSystem`：执行友敌过滤、免疫、穿透和伤害请求。
9. `HitImmunitySystem`：递减和清理局部/静态免疫状态。
10. `ProjectileLifetimeSystem`：递减 tick，产生 `DespawnEntityCommand` 和销毁事件。
11. `ProjectileReplicationSystem`：从组件生成不可变 `ProjectileReplicationSnapshot`。
12. `CommandCommitSystem`：一次性提交结构变化、伤害和销毁，保持确定性顺序。

所有系统只通过 `World` 查询、只读定义资源和命令队列通信；声音、粒子、绘制、网络发送由
下游适配器消费事件，不从 Simulation 反向调用宿主。

### 3.3 依赖边界

- `Terraria.Dome.Simulation` 不引用旧 `Main`、旧 `Projectile`、旧 `NPC`/`Player`、`NetMessage`、
  `SoundEngine`、XNA 绘制类型或 socket。
- `Terraria.Dome.Protocol.V1456` 只接收 `ProjectileReplicationSnapshot` 或专用投影 DTO，编码
  消息 27/29；协议层不得调用 `AI()`、`Update()`、`SetDefaults()`。
- `Terraria.Dome.Server` 负责会话、权限、网络身份分配和快照发布；不得把 ECS Entity 句柄暴露
  给客户端。
- `ProjectileDefinitionRegistry`、行为定义、免疫表和伤害曲线属于只读资源，不属于实体实例。

## 4. 旧职责到 ECS 的映射

| 旧职责 | 新归属 | 迁移策略 |
| --- | --- | --- |
| `SetDefaults(int type)` | `ProjectileDefinitionRegistry` + `ProjectileDefinitionSystem` | 先迁移已覆盖的类型；未覆盖类型显式标为 unsupported，不静默使用默认值 |
| `NewProjectile(...)` | `SpawnProjectileCommand` + `ProjectileSpawnSystem` | 保留来源、owner、初始速度、伤害和 AI 初值的 typed 参数 |
| `Update(int i)` | tick pipeline | 拆成行为、运动、碰撞、伤害、生命周期系统，禁止保留万能 Update |
| `AI()` / `AI_###` | `ProjectileBehaviorSystem` 的行为策略 | 按行为族分批迁移，每族有定义、状态、输入/输出和回归夹具 |
| `Damage()` | `ProjectileDamageSystem` | 先产生 `DamageRequestedEvent`，由统一战斗系统结算，避免投射物直接改 NPC 生命 |
| `Kill()` | `DespawnEntityCommand` + `ProjectileDespawnedEvent` | 记录原因、owner、identity，提交后生成 tombstone/网络销毁 |
| `localNPCImmunity` / `playerImmune` | `HitImmunityComponent` + `HitImmunitySystem` | 以实体句柄/稳定网络 ID 为键，禁止固定依赖 `Main.maxNPCs` 数组 |
| `oldPos` / `oldRot` / `frame` | `ProjectilePresentationComponent` | 仅作为呈现快照，不影响服务器伤害和碰撞结果 |
| `netUpdate` / `netSpam` | replication cursor / dirty revision | 由快照修订号和会话 cursor 决定，禁止实体写 socket |
| `Minion` / `Sentry` / 鞭子 / 抓钩 | 专用能力组件 + 行为策略 | 先选一个代表类型做闭环，再扩展同族，不创建继承树 |
| 音效、粒子、绘制、掉落 | 领域事件 + Server/Client/Presentation adapter | Simulation 只发布事实事件，不持有宿主对象 |

## 5. 分阶段执行

### 阶段 0：基线与清单冻结

**写集**：只新增 `Build/diagnostics/projectile-migration-*` 证据和本提案，不改旧源。

**动作**：

1. 对旧 `Projectile.cs` 做 Roslyn 语法索引：字段、方法、`AI_###`、静态状态、旧类型引用。
2. 建立 `ProjectileBehaviorCoverage` 清单：行为 ID、输入组件、输出命令、协议字段、状态。
3. 从当前 `DomeSimulation` 生成 Projectile 基线快照和消息 27/29 字节样本。

**闸门**：清单可重复生成；未识别行为不能进入“已迁移”统计。

### 阶段 1：身份、生成、运动、生命周期

**目标**：完成一个无特殊 AI 的 server-authoritative 投射物闭环。

**建议写集**：

- 新增 `Components/ProjectileDefinitionComponent.cs`、`ProjectileBehaviorComponent.cs`、
  `ProjectileNetworkIdentityComponent.cs`、`ProjectilePenetrationComponent.cs`；
- 新增 `Projectile/Definitions/ProjectileDefinition.cs`、`ProjectileDefinitionRegistry.cs`；
- 新增 `Projectile/Systems/ProjectileSpawnSystem.cs`、`ProjectileLifetimeSystem.cs`、
  `ProjectileReplicationSystem.cs`；
- 收窄 `SpawnProjectileCommand`，保留旧调用方所需的兼容工厂；
- 扩展 `ProjectileReplicationSnapshot`，加入 `Owner + Identity + OptionalUuid`。

**闸门**：生成、推进、过期和显式销毁均确定性；同一 tick 的结构变化只在 commit 阶段发生。

### 阶段 2：碰撞、伤害、穿透与免疫

**建议写集**：

- 新增 `Combat/Events/DamageRequestedEvent.cs`、`DamageAppliedEvent.cs`；
- 新增 `ProjectileDamageSystem.cs`、`HitImmunitySystem.cs`；
- 将现有 `ProjectileCollisionSystem` 拆为几何检测和命中候选查询两个边界；
- 把 NPC/Player 伤害入口改为命令/事件，不允许投射物直接写生命值。

**闸门**：单目标命中、穿透次数、局部免疫、静态免疫、friendly/hostile 过滤和撞砖销毁均有
可重复 verifier；伤害提交顺序固定为 `EntityId`/网络 identity 排序。

### 阶段 3：行为族迁移

按风险从低到高分批：直线/重力、反弹/穿透、追踪、爆炸/子弹簇、鞭子/抓钩、minion/sentry、
Boss 专属行为。每个行为族必须独立提交：

1. 定义和 typed state；
2. 行为系统分支；
3. 旧实现输入/输出对照夹具；
4. 协议 `ai[]` 投影映射；
5. 运行 verifier 和行为覆盖报告。

严禁把未迁移的 `AI_###` 直接转发到旧对象；未迁移行为只能被拒绝生成或进入显式兼容隔离层。

### 阶段 4：协议和服务器接入

实现 `ProjectileStateProjection`、`ProjectileSyncPacket`、`ProjectileSyncPacketCodec`，并接入
现有 V1456 复制游标：

- 消息 27：按 flags 稀疏编码 `position`、`velocity`、`owner`、`type`、typed behavior 的
  `ai[0..2]` 映射、banner、伤害和击退；
- 消息 29：只按稳定的 `owner + identity` 销毁，并为已销毁实体保留短期 tombstone，防止重复包；
- 客户端输入不能直接创建或销毁 server-owned Projectile；所有请求先经过权限验证和命令队列。

**闸门**：逐字段字节对照、owner 欺骗拒绝、重排/重复包处理、section 复制和断线清理均通过。

### 阶段 5：旧类隔离与下线

仅在前四阶段全部通过后执行：

1. 删除 Simulation 对旧 `Projectile` 的编译引用；
2. 保留一个只读兼容 facade（如确有外部调用者）并限制到协议/导入边界；
3. 完成旧文件调用点清单和迁移覆盖率报告；
4. 在独立提交中物理删除旧实现或关闭旧工程编译项。

物理删除不是行为完成标准；任何未通过的协议、行为或回归闸门都必须阻止删除提交。

## 6. 验证策略

验证按风险分层，所有命令从仓库根目录串行执行，并带
`-p:UseSharedCompilation=false`：

```powershell
dotnet build src\Terraria.Dome.Simulation\Terraria.Dome.Simulation.csproj -c Release -p:UseSharedCompilation=false
dotnet run --project Test\Terraria.Dome.Combat.Verification\Terraria.Dome.Combat.Verification.csproj -c Release -p:UseSharedCompilation=false
dotnet run --project Test\Terraria.Dome.Combat.Protocol.Verification\Terraria.Dome.Combat.Protocol.Verification.csproj -c Release -p:UseSharedCompilation=false
dotnet run --project Test\Terraria.Dome.Completion.Verification\Terraria.Dome.Completion.Verification.csproj -c Release -p:UseSharedCompilation=false
```

每个阶段必须记录：命令、退出码、警告/错误、输入 fixture、输出快照或字节证据。重点断言：

- 生成 -> 行为 -> 移动 -> 碰撞 -> 伤害 -> 穿透/免疫 -> 生命周期 -> commit 的顺序；
- 相同 snapshot/input/tick 得到相同输出；
- 消息 27 的 sparse flags 和消息 29 的 `owner + identity`；
- Projectile 不读取旧 `Main`、不调用旧 `AI`/`Update`，协议编码器不读取实体内部数组；
- 未覆盖行为被显式报告，不被计入迁移完成率。

## 7. 风险、回滚与完成定义

主要风险是 `Main` 全局依赖、`aiStyle` 隐式状态、静态免疫表、特殊投射物几何、旧客户端协议
字段和当前工作树的未提交改动相互干扰。每一阶段使用独立提交和新证据目录；回滚只撤销该阶段
新增文件/注册项，不回退其他未相关改动。

完成定义必须同时满足：

1. 所有声明为“已迁移”的行为族有 typed state、系统、fixture 和覆盖报告；
2. 消息 27/29 逐字段兼容，owner/identity 不可伪造或碰撞；
3. 服务器可在无旧 `Projectile` 类型的情况下完成生成、运动、命中、伤害、穿透和销毁；
4. 相关 verifier、Simulation 构建和协议回归均有当前源证据；
5. 旧文件物理删除前，调用点清单为空或仅剩明确批准的兼容 facade；
6. 文档明确列出仍未迁移的行为、非目标和残余风险。

