# NLTX 非通信模拟运行审查报告

**Project:** D:/TRbackup/NLTX（当前工作树，包含未提交的新实现）  
**Audit mode:** architecture, stability, release；限定无图形、非通信模拟运行链路  
**Date:** 2026-10-04，Asia/Shanghai  
**Reviewer:** Codex；宿主审查使用 gpt-6.1-sol / max 子代理  
**Source baseline:** HEAD 30e6c6cf2b2ebec1567f1c1e0d0ac243e5947839 + 当前工作树；关键文件指纹见 [source-snapshot.json](D:/TRbackup/NLTX/Build/diagnostics/NonCommunicationSimulationAudit/2026-10-04/source-snapshot.json)。审查期间部分文件仍在更新，行号对应本轮读取时的源码。  
**Changes:** 本轮仅保存审查报告、元数据和构建诊断；没有修复应用源码、测试、配置或依赖声明。

---

## 1. Executive Summary — 结论

**世界创建和静态保存/加载链路已经有实际成果，但当前仓库还不能作为完整的持续模拟系统启动。排除通信包设计后，仍然需要生产宿主、固定步总调度、运行实体初始化、内容定义发布、玩法适配器、运行期存档快照和端到端验收。** 缺口既包括接线，也包括尚未实现的运行算法；只增加一个定时循环还不够。

已有成果应保留：[10 月 4 日完整往返报告](D:/TRbackup/NLTX/docs/research/2026-10-04-generated-world-persistence-roundtrip.md:7)记录 Small 腐化经典、Medium 猩红专家世界通过正式保存、加载和 27 个区段 owner API，逐格比较 504 万 / 1152 万地块。它证明生成世界的持久化数据链路可用；验证目标是未发布 LoadedWorldSession，文档明确没有验证完整游戏模拟。修复前的 generated-world-load-attempt 文档是历史记录，不能据它重复认定 Tile codec 或完整加载 API 缺失。

本轮串行 restore 后，WorldGeneration、Player、Npc 均构建成功，玩家 traversal 和 NPC spawn 规则验证通过。Projectile 则因 Terraria.Npc 引用错误构建失败，连带阻塞 Application、Infrastructure.WorldStorage 和独立加载验证。根 Terraria.Dome.sln 的 52 个项目条目全部指向不存在的旧目录。这些是当前构建问题，与历史往返成功记录并不矛盾。世界生成先前的 EntityEcs/LiquidKind 错误在 restore 后消失，未列为源码缺陷。

### Score Dashboard

| 维度 | 得分 / 10 | 等级 | 判断依据 |
|---|---:|---|---|
| Stability | 4.0 | C | 局部规则和加载恢复已有保护，但无完整 tick/运行期保存闭环；覆盖 Medium。 |
| Maintainability | 6.0 | B | 领域、Application、Infrastructure 分层可复用，宿主与 owner 组合责任仍未落实；覆盖 High（本任务范围）。 |
| Design | 6.0 | B | 明确的 port、fresh session 和 Prepare/Commit 边界较好，内容发布与跨实体运行契约尚未完成。 |
| Release | 3.0 | C | 实际项目存在编译阻塞，solution 失效，缺少可运行的模拟 executable；覆盖 Medium。 |
| **Overall** | **4.8** | **C** | 仅评价本次范围的交付就绪程度；不是完成百分比，也不是全仓库质量评分。 |

Security、Performance、完整 Testing、商业部署/供应链未评分。各分值为判断性评分，越高越好。

### Finding Statistics

| Severity | Count | Confirmed | Suspected |
|---|---:|---:|---:|
| Critical | 0 | 0 | 0 |
| High | 10 | 10 | 0 |
| Medium | 3 | 2 | 1 |
| Low | 0 | 0 | 0 |
| Info | 0 | 0 | 0 |
| **Total** | **13** | **12** | **1** |

此处 High 表示阻塞用户要求的运行闭环，不代表已上线服务发生事故。发现之间存在依赖，不能简单相加工期。

## 2. Project Map — 当前代码与数据流

正式 src 共 26 个项目，Test 共 63 个项目。唯一正式 src executable 是 NSSLC.Tools.WorldGeneration；其他正式项目为库。参考目录中的宿主、旧 AI 或旧存档实现不计入生产能力。

| 层 / 路径 | 已承担的职责 | 运行边界 |
|---|---|---|
| src/NSSLC/Component | Player、Npc、Projectile、Content、WorldStorage、WorldSession、Physics 等状态和局部系统 | 多数调用由独立 verifier 组合；没有总运行宿主。 |
| src/NSSLC.Application/WorldStorage | 加载区段 API、LoadedWorldSession、保存/恢复/转换用例 | 存储用例存在；需要宿主提供发布、运行实体和快照 source。 |
| src/NSSLC.Infrastructure/WorldGeneration | 生成算法、正式编译的受限 Runtime、碰撞/液体等桥接 | MainHost 初始化和静态桥接不等于完整游戏 Main.Update。 |
| src/NSSLC.Infrastructure/WorldStorage | codec、文件 IO、factory、legacy 投影 | 最新代码已有 tile、核心世界设置、宝箱/标牌、压力板投影；仍需完整宿主组合。 |
| src/NSSLC.Infrastructure/Network | gateway、TCP transport、协议适配 | NetworkGatewayHost.Start 只启动网络；本报告不评估包设计完成度。 |
| src/NSSLC.Tools.WorldGeneration | 生成统计、--roundtrip 后退出 | 没有长期模拟、运行指令或退出存档循环。 |
| src/NSSLC.Infrastructure/分类参考 | 旧实现和分类材料 | 按架构约束，不允许直接编译为生产实现。 |

当前已验证的数据流是：生成结果 → GeneratedWorldDocumentProjection → WorldSaveCoordinator → .wld → WorldLoadCoordinator → 未发布 LoadedWorldSession → 数据比较。

需要补齐的目标数据流是：内容目录/配置 → 加载并发布活动世界 → 初始化实际运行实体 → 本地脚本输入 → 总 tick → owner 状态提交 → 一致快照 → 保存 → 新进程加载并恢复运行。通信以后接入同一输入/输出边界即可。

### Coverage Matrix

| Dimension | Coverage | Evidence inspected | Exclusions / limits |
|---|---|---|---|
| architecture | High | 全部正式项目清单/依赖、exe 入口、调用方搜索、Player/Projectile coordinator、NPC ports、Content 发布、加载与快照边界 | 不逐行审查生成巨型算法；参考目录只用于定位对照。 |
| stability | Medium | fresh/complete/published 生命周期、load gate、snapshot source、世界时间、实体运行入口、2 个实际 verifier | 没有生产 host，无法做持续运行/并发/退出试验；不把潜在 race 当已复现事故。 |
| release | Medium | SDK 10.0.400、实际项目 restore/build、失效 solution、输出路径、现有运行报告 | 不覆盖外部发布、全部 CI/安装/升级场景；加载 verifier 被编译阻塞。 |

排除通信包布局/路由设计、客户端渲染、UI、音频、成就，以及 .git、生成输出和第三方内容。读取 Build 仅为核对诊断证据。宿主子代理完成了 NLTX 审查；玩法子代理的首次输出误用了其他仓库，已丢弃；纠正后的玩法审查和持久化子代理因服务额度错误未完成，其结论未计入本报告，相关项由主代理复核。

## 3. Top Risks — 优先缺口

| 顺序 | 发现 | Severity | 对运行的直接影响 |
|---:|---|---|---|
| 1 | F01 Projectile 编译失败 | High | 当前 Application/存储验证链路也无法重新构建。 |
| 2 | F03 缺生产宿主与总 tick | High | 无入口持续推进模拟。 |
| 3 | F04 加载数据尚未组合成运行实体 | High | 加载记录不等于有血量、运动、AI 的活实体。 |
| 4 | F05 内容目录不能完成正式验证/发布 | High | 实体定义查询和 hydration 缺生产启动链路。 |
| 5 | F06/F07/F08 玩家、NPC、投射物运行缺口 | High | 不能形成移动、生成、攻击、死亡的玩法闭环。 |
| 6 | F09 世界时间/天气推进 | High | 世界静态状态加载后不会自动推进。 |
| 7 | F11 运行快照 source 缺失 | High | 生成时可保存，游玩后的状态尚无正式捕获实现。 |
| 8 | F13 缺端到端模拟验收 | High | 局部绿色验证不能证明整合运行正确。 |
| 9 | F12 legacy 与 owner 镜像一致性 | Medium / Suspected | 接入旧世界算法后可能读到两份不同状态，需在实现时验证。 |

## 4. Detailed Findings — 证据与修复方向

### Finding: F01 — Projectile 对 NPC 身份的依赖未闭合

- Severity: High
- Confidence: High
- Category: Release
- Status: Confirmed
- Affected area: Projectile、Application、Infrastructure.WorldStorage、加载 verifier。
- Evidence: [ProjectileDerivedPropertiesQuery.cs](D:/TRbackup/NLTX/src/NSSLC/Component/Projectile/ProjectileDerivedPropertiesQuery.cs:3)导入 Terraria.Npc；[Terraria.Projectile.csproj](D:/TRbackup/NLTX/src/NSSLC/Component/Projectile/Terraria.Projectile.csproj:7)没有 Npc 项目引用。GetOwnerMinionAttackTargetNpcSlot 使用非限定 NpcSlot；Npc 与 WorldStorage 都定义该名称。Function / Module: ProjectileDerivedPropertiesQuery；Relevant behavior: restore 成功后 build 仍报 CS0234。
- Problem: 新属性查询导致正式项目无法编译。
- Why it matters: Application 引用 Projectile，所以存档/加载工具也受影响。
- Realistic failure scenario: 从当前源码构建模拟宿主或独立加载验证时，在进入运行前即失败。
- Minimal fix: 若返回 NPC 领域身份，补 Npc 项目引用并显式限定 Terraria.Npc.NpcSlot 或使用类型别名；若契约实际是 WorldStorage slot，则去掉错误 using 并统一返回语义。不能只加引用而忽略同名类型歧义。
- Better long-term fix: 保持一个明确的跨 owner slot 转换边界，避免调用方猜测两类 slot 的含义。
- Regression test suggestion: 构建 Projectile、Application、Infrastructure.WorldStorage、GeneratedWorldLoadVerification，随后运行现有投射物及加载 verifier。
- Estimated effort: 0.5–1 天，包含依赖/签名检查和回归。

### Finding: F02 — 根 solution 指向已不存在的旧项目

- Severity: Medium
- Confidence: High
- Category: Release
- Status: Confirmed
- Affected area: 仓库构建入口。
- Evidence: [Terraria.Dome.sln](D:/TRbackup/NLTX/Terraria.Dome.sln:7)引用旧 Simulation、Transport、Server 等路径。Function / Module: solution 项目图；Relevant behavior: 实际 build 返回 52 个 MSB3202，0 警告，exit 1。
- Problem: solution 不能代表当前 NSSLC 项目集合。
- Why it matters: IDE/自动验证通过根 solution 构建会失败，且其中 Server/Simulation 条目会造成宿主已存在的错觉。
- Realistic failure scenario: 开发者运行根构建命令，无法进入当前组件的编译。
- Minimal fix: 用现有正式项目重建有效 solution/solution filter；将参考项目排除。新增宿主后再登记真实路径。
- Better long-term fix: 维护一条明确的非通信模拟构建和验证入口。
- Regression test suggestion: 检查所有 solution 项目路径存在，构建有效项目图；不要创建旧名称空项目来让路径检查通过。
- Estimated effort: 1–3 小时。

### Finding: F03 — 没有生产模拟宿主、总 tick 和完整退出路径

- Severity: High
- Confidence: High
- Category: Stability
- Status: Confirmed
- Subtype: ModuleBoundary
- Affected area: Host/Application composition。
- Evidence: [工具 Program.cs](D:/TRbackup/NLTX/src/NSSLC.Tools.WorldGeneration/Program.cs:7)仅执行生成/roundtrip，47 行返回；[NetworkGatewayHost.cs](D:/TRbackup/NLTX/src/NSSLC.Infrastructure/Network/NetworkGatewayHost.cs:29)只 Seal gateway 和启动 TCP；[Runtime/Main.cs](D:/TRbackup/NLTX/src/NSSLC.Infrastructure/WorldGeneration/Runtime/Main.cs:54)声明 GameUpdateCount，正式 src 未找到递增调用。Function / Module: executable inventory、Start；Relevant behavior: Player/Projectile coordinator 的实例化调用只在 Test 中。
- Problem: 没有代码组合世界与实体并持续推进全部阶段。
- Why it matters: 网络监听和世界加载都不能替代模拟进程。
- Realistic failure scenario: 世界文件读取成功，进程退出或仅监听端口，实体与时间保持静态。
- Minimal fix: 新建一个无图形可执行 Host，负责启动校验、世界加载/发布、内容与 owner 装配、固定步 tick、本地输入队列、停止及最终保存。明确 phase order 和同一线程写入责任。
- Better long-term fix: 保留可手动 Step 的同步内核；实时运行只负责计时、有限追帧和取消，便于未来接入网络。
- Regression test suggestion: 0/1/多本地模拟玩家运行 600/3600 ticks；tick 单调且各阶段恰好按约定执行，失败加载不启动，取消可退出。
- Estimated effort: 2–4 天建立宿主骨架；不包含下面尚未实现的玩法算法。

### Finding: F04 — 加载完成与实际运行实体激活之间仍缺宿主组合

- Severity: High
- Confidence: High
- Category: Stability
- Status: Confirmed
- Subtype: BoundaryContract
- Affected area: LoadedWorldSession、player/NPC owner、世界发布。
- Evidence: [LoadedWorldSession.cs](D:/TRbackup/NLTX/src/NSSLC.Application/WorldStorage/Loading/LoadedWorldSession.cs:8)保存未发布目标；[WorldNpcState.cs](D:/TRbackup/NLTX/src/NSSLC/Component/WorldStorage/WorldNpcState.cs:3)仅记录持久身份/位置/住房，无运动、血量或 AI 组件；[factory](D:/TRbackup/NLTX/src/NSSLC.Infrastructure/WorldStorage/WorldStorageCoordinatorFactory.cs:204)仍需要宿主 callback，正式调用方缺失。Function / Module: RegisterGeneratedWorldLoadHandlerWithRuntimeTileMap、WorldNpcRestoreSystem；Relevant behavior: loader 提交持久记录，不生成完整 NPC gameplay 状态。
- Problem: 新 owner API 已能恢复世界数据，但没有生产代码把它与实际活动玩家/NPC、slot 生命周期及运行依赖组合起来。
- Why it matters: 存档 NPC 记录数量正确，并不意味着 NPC 能寻路、受伤或死亡。
- Realistic failure scenario: 加载的两条 NPC 记录保留名称和住房，却没有被 NPC tick 枚举；玩家也没有通过非网络入口建立完整可更新状态。
- Minimal fix: 在 Host 中用现有 recovery/publication APIs 完成活动会话；对 NPC 类型执行定义 hydration、建立 runtime identity/slot/组件，再绑定玩家创建、出生、释放和世界切换。恢复前不得 tick。
- Better long-term fix: 持久记录与运行实体保持明确转换，不把 WorldNpcState 当完整 NPC owner。已有 tile/核心设置/宝箱/标牌/压力板投影继续复用。
- Regression test suggestion: fresh load → publish → 活 NPC/玩家参与 tick；重复切换世界没有旧实体，发布失败不留半活动世界，加载的名称/住房不丢失。
- Estimated effort: 2–5 天，取决于首批实体范围。

### Finding: F05 — ContentCatalog 的正式验证与启动数据源没有闭环

- Severity: High
- Confidence: High
- Category: Stability
- Status: Confirmed
- Subtype: BoundaryContract
- Affected area: Content / entity hydration。
- Evidence: [ContentCatalog.cs](D:/TRbackup/NLTX/src/NSSLC/Component/Content/ContentCatalog.cs:5)拒绝未验证 snapshot；[ContentCatalogSnapshot.cs](D:/TRbackup/NLTX/src/NSSLC/Component/Content/ContentCatalogSnapshot.cs:5)公共构造不能设置 isValidated，102 行 internal CreateValidated 没有正式调用方。Function / Module: ContentCatalog publication；Relevant behavior: 正式 src 未找到目录构建/校验/发布或载入定义集合的生产入口。
- Problem: Catalog、各 DefinitionCatalog 和 ProjectileDefinitionHydrationSystem 已存在，但没有可发布 validated catalog 的正式构建系统及宿主 bootstrap。
- Why it matters: 实体创建需要可靠的尺寸、生命、AI style、寿命、伤害等定义；只有类型结构不够。
- Realistic failure scenario: 宿主直接 new ContentCatalog(snapshot) 被拒绝；绕过它直接拼局部定义又无法确认所用类型是否完整。
- Minimal fix: 在 Content 所属程序集补正式校验/构建入口，调用现有 CreateValidated；由 Infrastructure 提供维护的数据源，Host 启动发布。先支持明确的最小类型集合，对缺失定义给出清晰拒绝。
- Better long-term fix: 按支持集合管理版本和 CatalogRevision，扩展类型时验证相应依赖，避免反射旧完整游戏 DLL 或直接编译分类参考。
- Regression test suggestion: 真实启动目录通过校验；重复 ID、缺失必需定义和未知类型拒绝；首批 NPC/Projectile/Item hydration 字段正确。
- Estimated effort: 2–4 天接通首批定义；全内容数据迁移另行估算。

### Finding: F06 — 玩家总更新缺移动碰撞与物品实际行为组合

- Severity: High
- Confidence: High
- Category: Stability
- Status: Confirmed
- Affected area: Player/Input/Physics/ItemUse。
- Evidence: [PlayerTickCoordinator.cs](D:/TRbackup/NLTX/src/NSSLC/Component/Player/PlayerTickCoordinator.cs:3)标记 isolated core，114–141 行仅装备/Buff/资源/防御；[PlayerTraversalPhysicsSystem.cs](D:/TRbackup/NLTX/src/NSSLC/Component/Player/Movement/PlayerTraversalPhysicsSystem.cs:19)计算并提交运动参数，没有积分位置和地块碰撞；[PlayerItemUseExecutionSystem.cs](D:/TRbackup/NLTX/src/NSSLC/Component/Player/PlayerItemUseExecutionSystem.cs:10)处理使用门控、动画及 tail，没有发射/挖掘/伤害执行。Function / Module: player update pipeline；Relevant behavior: 正式 src 无将这些局部步骤组合成完整玩家 tick 的调用方。
- Problem: Input、lifecycle、combat、inventory、mount 等局部系统存在，但完整 input → acceleration/jump → movement → tile collision → item action → damage/death/respawn 链路未建立。
- Why it matters: 已有 PlayerTickCoordinator 不能直接代表 Player.Update 完成。
- Realistic failure scenario: 输入被记录，Buff 倒计时正常，但玩家位置不变；使用武器只有计时，无投射物或近战命中。
- Minimal fix: 增加玩家运行 coordinator，明确 local scripted input 与 owner snapshots；补必要运动积分/地块碰撞执行和首批 Item action handler，接入现有生命/死亡/出生系统。
- Better long-term fix: 复用正式 Runtime/Collision 中已有碰撞算法时，经明确的状态适配边界提交结果；避免复制整份旧 Player 或把物理实现放进数据包处理器。
- Regression test suggestion: 走/停/跳/落地，平台与斜坡，接触伤害，首个武器发射，死亡后倒计时/重生；真实地图上位置与血量改变。
- Estimated effort: 4–8 天完成首个可玩切片；mount、特殊移动和全物品行为需要后续批次。

### Finding: F07 — NPC 生成 continuation 和常规 AI/update 尚无生产实现

- Severity: High
- Confidence: High
- Category: Stability
- Status: Confirmed
- Affected area: NPC spawning / gameplay。
- Evidence: [NpcSpawnSystem.cs](D:/TRbackup/NLTX/src/NSSLC/Component/Npc/NpcSpawnSystem.cs:151)调用 ContinueSpawnAttempt 后返回 CreationObservation.Unknown；[INpcSpawnPassPort.cs](D:/TRbackup/NLTX/src/NSSLC/Component/Npc/INpcSpawnPassPort.cs:3)未找到正式实现；[NpcAuthoritySystem.cs](D:/TRbackup/NLTX/src/NSSLC/Component/Npc/NpcAuthoritySystem.cs:9)仅提交死亡阶段决策。Function / Module: NPC natural spawn/update；Relevant behavior: 正式目录有生成选择、状态/重力/击退/战斗系统，但未找到常规 NPC AI 分派及总更新循环。
- Problem: 生成规则计算与实体分配、定义初始化、AI、运动、碰撞、接触伤害、despawn 尚未组合。
- Why it matters: eligibility/spawn-rate 验证通过不会自动产生可运行 NPC。
- Realistic failure scenario: 搜索到合法生成格，只提交 continuation；即便手动分配一个 NPC，也没有常规 AI 推进它。
- Minimal fix: 实现真实 spawn port/owner，接通已有准备/类型解析/slot 选择；提供首批 NPC AI handler 和 tick coordinator，接入状态、物理、伤害与释放。
- Better long-term fix: 用明确 AI 支持表逐批迁移，不用默认 no-op 冒充所有类型已支持。
- Regression test suggestion: 固定种子条件生成实际实体；首批怪物寻敌/移动/攻击/死亡/despawn，slot 满容量拒绝且释放后复用正确。
- Estimated effort: 4–8 天覆盖一个敌怪与一个城镇 NPC 的最小行为；全 AI 不是短期接线工作。

### Finding: F08 — ProjectileTick 的生产行为 adapter 仍缺失

- Severity: High
- Confidence: High
- Category: Stability
- Status: Confirmed
- Affected area: Projectile AI/motion/collision/combat。
- Evidence: [IProjectileTickAdapter.cs](D:/TRbackup/NLTX/src/NSSLC/Component/Projectile/IProjectileTickAdapter.cs:5)把行为委托给 adapter，生产 src 没有实现；[ProjectileTickCoordinator.cs](D:/TRbackup/NLTX/src/NSSLC/Component/Projectile/ProjectileTickCoordinator.cs:136)对 IntegrationRequired 抛异常；[ProjectileMotionAndAiSystem.cs](D:/TRbackup/NLTX/src/NSSLC/Component/Projectile/ProjectileMotionAndAiSystem.cs:8)目前只推进 GfxOffY。Function / Module: Tick/UpdateProjectileStep；Relevant behavior: shell 已有 slot/substep/免疫/trail/lifetime，AI/移动/碰撞/命中由调用方负责。
- Problem: 新增 AI19/AI137 查询、damage gate、生命周期等可复用，但还没有把它们执行到真实 owner 的生产 adapter。
- Why it matters: 定时调用 Tick 并提供空 adapter 会只消耗寿命；minion/sentry、owner trail 和 DD2 特例还要求专门快照。
- Realistic failure scenario: 创建普通弹体后不移动/不伤害；遇 minion/sentry 或 owner-dependent 行为直接拒绝更新。
- Minimal fix: 实现真实 Projectile gameplay adapter，包含支持 AI 分派、运动与tile/entity collision、damage commit、终止效果；显式提供 owner 和事件快照，保持 coordinator 的 substep 语义。
- Better long-term fix: 把首批射弹切片接完，再扩展不同 AI style；unsupported 行为显式暴露，不返回假成功。
- Regression test suggestion: 普通射弹运动命中，墙碰撞终止，extraUpdates 与寿命/免疫单位正确，minion/owner trail/DD2 输入可用，slot 重用没有旧 handle 误命中。
- Estimated effort: 3–6 天接通首批普通射弹；特殊类型继续分批。

### Finding: F09 — 世界时钟、天气和事件状态没有正式持续推进

- Severity: High
- Confidence: High
- Category: Stability
- Status: Confirmed
- Affected area: WorldSession/time/weather/event scheduling。
- Evidence: [WorldClockState.cs](D:/TRbackup/NLTX/src/NSSLC/Component/WorldSession/WorldClockState.cs:3)、[CalendarClockComponent.cs](D:/TRbackup/NLTX/src/NSSLC/Component/WorldSession/Calendar/CalendarClockComponent.cs:5)提供状态/校验；[MainHost.cs](D:/TRbackup/NLTX/src/NSSLC.Infrastructure/WorldGeneration/Runtime/MainHost.cs:98)UpdateTimeRate 为空；[WorldTickSnapshot.cs](D:/TRbackup/NLTX/src/NSSLC/Component/WorldSession/WorldTickSnapshot.cs:34)只有值对象工厂。Function / Module: world time update；Relevant behavior: 正式 src 未找到 committed tick snapshot 生产调用或完整昼夜/天气推进系统；载入投影只赋初值。
- Problem: 时间字段和事件状态存在，缺运行更新策略与调度调用。
- Why it matters: 生成、AI、spawn 与世界事件依赖一致的时间和天气输入。
- Realistic failure scenario: 运行 3600 帧后仍是加载时的时间，雨/事件计时不变，昼夜边界不触发。
- Minimal fix: 选定一个时钟 owner，补 fixed-tick advancement、暂停/dayRate、昼夜/月相过渡；在统一阶段提交快照。天气与事件按最小基线接入倒计时和变化。
- Better long-term fix: 首先消除并列时钟模型的使用歧义，后续再接季节、入侵、Boss 等完整规则。
- Regression test suggestion: 白天/夜晚最后一 tick、dayRate 0/1/快进、跨多边界、恢复存档时间、天气倒计时；同 tick 全实体读取同一快照。
- Estimated effort: 1–3 天基础时钟，3–6 天首批天气/事件；完整事件规则另计。

### Finding: F10 — 世界掉落物与 TileEntity 仍以状态为主，缺实际更新/副作用提交

- Severity: Medium
- Confidence: High
- Category: Stability
- Status: Confirmed
- Affected area: Items/WorldDrops/TileEntities/world interaction。
- Evidence: [WorldItemStateComponent.cs](D:/TRbackup/NLTX/src/NSSLC/Component/Items/WorldDrops/WorldItemStateComponent.cs:3)仅状态与到期查询；[PlayerInventoryPickupSystem.cs](D:/TRbackup/NLTX/src/NSSLC/Component/Player/PlayerInventoryPickupSystem.cs:5)依赖实际 inventory/coin/effect ports，正式组合未找到；[TileEntityUpdateSchedule.cs](D:/TRbackup/NLTX/src/NSSLC/Component/WorldStorage/TileEntityUpdateSchedule.cs:3)只有登记字段；[Runtime/TileEntity.cs](D:/TRbackup/NLTX/src/NSSLC.Infrastructure/WorldGeneration/Runtime/TileEntity.cs:43)PerformUpdates 为空，Runtime/WorldEntities.cs 的 WorldItem.OverrideWith 为空。Function / Module: item/TileEntity runtime pass；Relevant behavior: 持久化恢复与局部拾取算法存在，缺世界物品总更新与实体行为调用。
- Problem: 尚不能形成挖掘/死亡掉落 → 世界物品移动/保留 → 拾取 → 背包提交，TileEntity 也不会因“已加载”自动更新。
- Why it matters: 对基本移动示例可后置，对可玩的世界交互闭环必须实现。
- Realistic failure scenario: 挖掘只改地块，drop intent 没有创建实际物品；逻辑感应器/训练假人加载后不运行。
- Minimal fix: 增加 world item allocation/expiry/physics/pickup pass 和真实 inventory ports；实现首批 tile action/drop commit；TileEntity 按 kind 分派并管理更新登记。Wiring 已有局部 traversal/cooldown，不应重复从零写。
- Better long-term fix: 将表现效果 sink 与物品/地块提交分开；无图形 sink 可不渲染，但领域效果不能丢弃。
- Regression test suggestion: 单次掉落/拾取不会复制或丢失 stack，保留期限到期，满背包留下剩余量，首批 TileEntity 按帧运行并在移除后停止。
- Estimated effort: 3–6 天最小掉落/拾取；TileEntity 各类型和完整 Wiring 另行分批。

### Finding: F11 — 缺从运行 owner 捕获当前存档的 snapshot source

- Severity: High
- Confidence: High
- Category: Stability
- Status: Confirmed
- Subtype: BoundaryContract
- Affected area: runtime save/autosave/shutdown save。
- Evidence: [IWorldPersistenceSnapshotSource.cs](D:/TRbackup/NLTX/src/NSSLC.Application/WorldStorage/Persistence/IWorldPersistenceSnapshotSource.cs:8)只有契约；[WorldSaveSnapshotCoordinator.cs](D:/TRbackup/NLTX/src/NSSLC.Application/WorldStorage/Persistence/WorldSaveSnapshotCoordinator.cs:53)调用它捕获 document，正式 src 没有实现/实例化调用；[WorldPersistenceRoundTrip.cs](D:/TRbackup/NLTX/src/NSSLC.Tools.WorldGeneration/WorldPersistenceRoundTrip.cs:34)捕获的是 GeneratedWorld。Function / Module: Capture/Save；Relevant behavior: 现有 roundtrip 是生成时快照保存，不是运行后的状态保存。
- Problem: 文件写入和 encoder 已存在，缺运行地块、容器、NPC、时间进度、TileEntity 内容等到 document 的生产捕获实现。
- Why it matters: 不能把重复保存最初生成结果当 autosave。
- Realistic failure scenario: 世界已挖掘/拾取/过了一夜，仍保存最初 GeneratedWorld，重启后丢失运行修改。
- Minimal fix: 实现 owner snapshot source，沿用现有 DTO/codec/WorldSaveSnapshotCoordinator；与加载、转换共享 gate，并在 tick 提交边界获取一致数据。接入 autosave 和受控退出。
- Better long-term fix: 首先锁定 owner 真值与持久化语义；异步文件写入使用已冻结快照，避免后台枚举活动组件。玩家角色档案如需跨进程保留，应另走明确的玩家保存用例，不能混进 .wld。
- Regression test suggestion: 加载 → 改地块/宝箱/NPC住房/时间 → 保存 → 新会话加载比较；取消/写失败保留上一有效存档；正在转换时保存等待正确。
- Estimated effort: 3–6 天覆盖当前世界 owner；含动态 TileEntity 内容需补相应范围。

### Finding: F12 — 接入 legacy 世界算法后，双份 tile/time 状态可能发生漂移

- Severity: Medium
- Confidence: Medium
- Category: Stability
- Status: Suspected
- Subtype: StateOwnership
- Affected area: legacy runtime ↔ LoadedWorldSession owner consistency。
- Evidence: [LegacyWorldTileMapProjection.cs](D:/TRbackup/NLTX/src/NSSLC.Infrastructure/WorldStorage/Generation/LegacyWorldTileMapProjection.cs:114)分配 detached Tile 数组，再写 Main.tile；[LegacyWorldSessionProjection.cs](D:/TRbackup/NLTX/src/NSSLC.Infrastructure/WorldStorage/Generation/LegacyWorldSessionProjection.cs:131)把 owner 时间赋给 Main；[MainHost.cs](D:/TRbackup/NLTX/src/NSSLC.Infrastructure/WorldGeneration/Runtime/MainHost.cs:134)QueueMainThreadAction 和 RunOnMainThread 都直接执行。Function / Module: projection/main thread bridge；Relevant behavior: 此桥接没有建立真实调度队列，也未提供通用反向同步。
- Problem: 当前没有运行 Host，尚未复现漂移或 race；但后续若 legacy 算法改 Main、snapshot source 读 Storage/World，两个视图可能不一致。
- Why it matters: 会影响碰撞、快照和存档读取同一版本的状态。
- Realistic failure scenario: 将 WorldGen/Liquid 算法接入 tick 后修改 Main.tile，却从 session.Storage.TileMap 保存，重载回旧地块。
- Minimal fix: 明确每类状态的唯一写入 owner；兼容 Runtime 算法的 mutation 必须回到 owner commit 或显式写回。Host 实现真实 command drain 和调度线程检查，避免把当前 inline 方法当线程切换。
- Better long-term fix: legacy 表示作为受控视图；按实际需要逐步迁移，不为全部字段建立泛化同步框架。
- Regression test suggestion: 对 owner 和 legacy 各执行一类真实修改，tick 快照/碰撞/保存重载一致；后台输入或转换提交只能在指定阶段修改状态。
- Estimated effort: 1–3 天明确并验证首批状态同步边界；完整迁移随功能推进。

### Finding: F13 — 缺使用生产组合的持续模拟端到端验收

- Severity: High
- Confidence: High
- Category: Release
- Status: Confirmed
- Affected area: integration verification。
- Evidence: [独立加载验证 README](D:/TRbackup/NLTX/Test/NSSLC.WorldStorage.GeneratedWorldLoadVerification/README.md:62)明确只提交未发布会话，不运行发布、液体推进或 NPC；[ProjectileTickCoordinatorVerification.cs](D:/TRbackup/NLTX/Test/Terraria.ProjectileCombatVerification/ProjectileTickCoordinatorVerification.cs:46)手工注入 recording adapter；[PlayerTickCoordinator.cs](D:/TRbackup/NLTX/src/NSSLC/Component/Player/PlayerTickCoordinator.cs:3)只承诺局部 core。Function / Module: current verifiers；Relevant behavior: 未找到 load → production tick → gameplay mutation → runtime save → reload 验证入口。
- Problem: 当前测试的承诺边界是局部系统和静态存取，没有验收真正的模拟组合。
- Why it matters: 不能从多个单元/局部验证通过推导整体能运行。
- Realistic failure scenario: 所有 recording ports 测试绿色，真实宿主因未绑定 owner/定义/phase order 在第一帧失败。
- Minimal fix: 在宿主可 Step 后建立使用生产绑定的离线 smoke；固定输入脚本与明确断言，保留少量覆盖关键状态变化和失败恢复的场景。
- Better long-term fix: 无网络先达到同一验收基线，通信接入后复用场景。避免用空 adapter/no-op port 替换待验收的运行实现。
- Regression test suggestion: 600/3600 ticks 的 0/1/多玩家场景，移动碰撞、生成攻击、命中死亡、掉落拾取、时间变化、修改后存档重载、取消/失败加载和重复 world switch。
- Estimated effort: 1–3 天首个 smoke；随各玩法切片补充断言。

## 5. Architecture Concerns — 运行能力矩阵

- Coverage: High（本任务范围）。
- Inspected evidence: 正式项目图、入口清单、coordinator/port 调用方、Content、load publication、snapshot 接口。
- Exclusions / limits: 不评价客户端架构；没有把参考代码当已实现生产功能。

| 能力 | 当前状态 | 必须补的代码 | 对最小基线的要求 |
|---|---|---|---|
| 世界生成、标准 319 静态存取 | 已有实现和历史真实证据 | 修当前编译后复跑已有加载 verifier | 复用。 |
| Host / tick / shutdown | 缺组合入口 | executable、固定步循环、phase coordinator、取消/保存 | 第一批。 |
| load publication / entities | 部分正式 projection 已有 | 宿主注册、运行 NPC hydration、玩家创建与 slot lifecycle | 第一批。 |
| Content | catalog/definition 结构已有 | validated build API、真实数据源与 bootstrap | 第一批，先支持明确类型集合。 |
| 玩家 | 多局部规则已有 | 完整运动/碰撞/实际 item action 及阶段组合 | 首个可玩切片。 |
| NPC | spawn 选择/状态/战斗局部已有 | spawn port/owner、常规 AI/tick | 首个可玩切片。 |
| Projectile | 生命周期、substeps、局部查询已有 | gameplay adapter、AI/移动/碰撞/命中提交 | 首个可玩切片。 |
| 时间、天气、事件 | 状态/restore 有，持续推进未闭合 | clock system、边界事件、天气计时与快照提交 | 基础时钟第一批；完整事件后续。 |
| Items/TileEntity | 状态和部分 inventory 算法有 | 世界物品 pass、真实提交 ports、类型更新分派 | 掉落拾取需闭环；全 TileEntity 后续。 |
| Liquid/WorldGen/Wiring | 正式 Runtime 有液体/世界算法，Wiring 有局部系统 | 每 tick 调度、状态写回、真实效果端口 | 先接可验证子集，不能认定从零缺算法。 |
| autosave | 文件用例有，运行 source 无 | 当前 owner snapshot source、tick 边界 capture | 第一批持久化验收。 |

最小 Host 与 Application 组合可放在一个新 executable 加少量 coordinator 中，不需要预建大量空项目或重写 ECS。60 Hz 可以作为兼容旧规则的初始建议；最终频率须由各系统的 tick 单位和 reference 行为确认，不能简单将真实 delta time 传给以帧为单位的倒计时。

## 6. Stability Concerns — 生命周期与状态一致性

- Coverage: Medium。
- Inspected evidence: WorldLoadRecoveryCoordinator、RuntimeWorldLoadLifecycleProjection、WorldSaveSnapshotCoordinator、legacy projection、fresh session 拒绝规则。
- Exclusions / limits: 未复现持续运行或并发问题；F12 是设计接入风险。

Host 要明确状态：Created → Loading → Published/Ready → Running → Stopping → Stopped；Failed/Canceled 不得进入 Running。已有恢复协调器负责 retry/backup/reset，并非需要从零补恢复机制。宿主需要消费结果、保障单个世界调用串行化，并且让各运行阶段只在会话 ready 后执行。

运行期间建议一个模拟线程提交 owner 状态。网络、本地脚本和后台转换输入先排队，在规定阶段 drain；文件写入可后台执行，但必须使用一致、冻结的快照。每帧保存全世界不是要求，快照频率与开销应在实际 Host 可运行后测量。

## 7. Release Concerns — 本轮验证与实际限度

- Coverage: Medium。
- Inspected evidence: SDK/项目构建日志、2 个 verifier 运行日志、solution 路径、实际 Build/bin 产物。
- Exclusions / limits: 没有全项目回归/部署；独立加载 verifier 编译失败后未运行旧 DLL 伪造成功。

所有命令工作目录为 D:/TRbackup/NLTX。实际项目先逐个执行 dotnet restore <项目> --nologo -v:minimal，再串行执行 dotnet build <项目> --no-restore --nologo -v:minimal。下表路径是相应完整命令的 <项目> 参数。

| 项目 / 命令 | Restore exit | Build exit | 警告 / 错误 | 日志前缀 |
|---|---:|---:|---|---|
| src/NSSLC/Component/Projectile/Terraria.Projectile.csproj | 0 | 1 | 0 / 1，CS0234 | projectile |
| src/NSSLC.Infrastructure/WorldGeneration/NSSLC.WorldGeneration.csproj | 0 | 0 | 15 / 0，生成导入源码既有 warning | worldgen |
| src/NSSLC/Component/Npc/Terraria.Npc.csproj | 0 | 0 | 0 / 0 | npc |
| src/NSSLC/Component/Player/Terraria.Player.csproj | 0 | 0 | 0 / 0 | player |
| src/NSSLC.Application/NSSLC.Application.csproj | 0 | 1 | 0 / 1，Projectile 根因 | application |
| src/NSSLC.Infrastructure/WorldStorage/NSSLC.Infrastructure.WorldStorage.csproj | 0 | 1 | 0 / 1，Projectile 根因 | infra-worldstorage |
| Test/NSSLC.WorldStorage.GeneratedWorldLoadVerification/NSSLC.WorldStorage.GeneratedWorldLoadVerification.csproj | 0 | 1 | 0 / 1，Projectile 根因；未运行 | generated-load |
| Test/Terraria.Player.TraversalPhysics.Verification/Terraria.Player.TraversalPhysics.Verification.csproj | 0 | 0 | 0 / 0 | traversal |
| Test/Terraria.Npc.SpawnEligibility.Verification/Terraria.Npc.SpawnEligibility.Verification.csproj | 0 | 0 | 0 / 0 | npcspawn |
| dotnet build Terraria.Dome.sln --no-restore --nologo -v:minimal | 未执行 | 1 | 0 / 52，MSB3202 | solution |

最后一行是整仓库审查构建入口的诊断，不是一次增量源码变更的 solution build。早先并行构建产生的文件锁和错误验证项目路径属于诊断过程问题，未当作仓库缺陷。

运行命令与结果：

    dotnet run --project Test/Terraria.Player.TraversalPhysics.Verification/Terraria.Player.TraversalPhysics.Verification.csproj --no-build --no-restore
    # exit 0；PASS: player traversal physics, state commit and capability snapshot

    dotnet run --project Test/Terraria.Npc.SpawnEligibility.Verification/Terraria.Npc.SpawnEligibility.Verification.csproj --no-build --no-restore
    # exit 0；生成资格、rate、区域/搜索、容量、玩家顺序等 8 组 PASS

这两项证明局部算法行为，不证明整体模拟。日志位于 [本轮诊断目录](D:/TRbackup/NLTX/Build/diagnostics/NonCommunicationSimulationAudit/2026-10-04)。成功产物已确认位于 Build/bin/：NSSLC.WorldGeneration、Terraria.Npc、Terraria.Player 及两个上述 verifier 各自 Debug/net10.0/*.dll；Build/obj、Build/generated、Build/packages 继续沿用仓库约定。

## 8. Principles Compliance — 应保留与应落实的原则

### Principles Violated / 待落实

| Principle | Violations | Severity | Affected Areas |
|---|---:|---|---|
| 明确组合根和完整 boundary contract | 3 组：F03/F04/F05 | High | Host、运行实体激活、Content publication |
| 可重现构建/交付入口 | 2：F01/F02 | High / Medium | 项目依赖、solution |
| 单一状态真值 | 1：F12，Suspected | Medium | owner 与 detached legacy 表示 |

### Principles Respected

加载使用 fresh、未发布目标，Prepare 与 Commit 分开，成功后才能发布；不支持 CreativePowers 时明确拒绝。保存/文件 codec 留在存储基础设施。局部系统用显式 snapshot/port，适合逐步组合。审查期间的世界加载改进已经包含更多 runtime projection，应继续在原边界补足。

## 9. Architecture Analysis — 修复责任归属

| Subtype | Count | Affected Areas | Recommended Action |
|---|---:|---|---|
| ModuleBoundary | 1 | F03 | Host 负责组合与调度，领域保留行为。 |
| DependencyDirection | 0 | 未发现本范围需独立立项的方向倒置 | F01 按真实 slot 契约补依赖，不做泛化拆分。 |
| StateOwnership | 1 suspected | F12 | 选定真值，验证 legacy 写回及 snapshot 来源。 |
| BoundaryContract | 3 | F04/F05/F11 | 完成实体激活、内容发布、运行快照接口实现。 |
| EvolutionRisk | 0 | 本范围不另行计数 | 首批类型明确支持范围，再逐步扩展。 |

算法（玩家运动、NPC AI、Projectile 行为、世界时间）归现有领域/行为系统；输入和生命周期协调归 Application；文件、clock provider、数据源与必要 legacy adapter 归 Infrastructure；最终实例化和实时循环归 Host。包处理器将来只提交输入/读取结果，不承担每帧游戏算法。

## 10. Recommended Fix Order — 可执行修复路线

### Fix Immediately

| 阶段 | 具体交付 | 依赖 / 工作量 | 验收出口 |
|---|---|---|---|
| P0 构建基线 | 修 F01，更新有效 solution；复跑已有加载/投射物验证 | 约 0.5–1 天 | 当前源码正式依赖可构建；历史加载证据在新源码复测。 |
| P1 无通信宿主 | Host executable、可 Step 内核、内容最小 bootstrap、load publication/生命周期、基础时钟 | 约 5–10 人天；复用现有 owner 和 recovery | 加载真实 .wld，0/1 scripted player 运行 3600 ticks，时钟与 tick 正确，可停止。 |
| P2 运行存档 | 当前 owner snapshot source、保存阶段与受控退出，镜像一致性边界 | 约 3–6 人天；可与 P1 协同推进 | 修改地块/宝箱/时间后 save → 新会话 load 一致，失败保留有效存档。 |

### Fix Before Stable Release

| 阶段 | 具体交付 | 依赖 / 工作量 | 验收出口 |
|---|---|---|---|
| P3 最小玩法切片 | 玩家走跳/碰撞；一个敌怪与一个 town NPC；一个武器/普通射弹；伤害死亡/重生；掉落拾取 | 粗估 10–20 人天；若部分 adapter 可直接复用可减少 | 在真实载入地形中完成输入 → 移动 → 战斗 → 掉落 → 拾取 → 保存重载。 |
| P4 模拟加固 | 0/1/多本地玩家，phase order、失败发布、重复切换、取消保存、支持表 | 约 2–5 人天，随切片补齐 | 600/3600 ticks smoke 使用生产绑定稳定通过。 |

估算是工作量区间，不是交付承诺；各 finding 的估算有重叠，不能相加。达到“离线持续模拟骨架”与达到“完整 Terraria 玩法兼容”是两个不同验收目标，全 AI/物品/事件无法在本次接线估算内承诺。

### Schedule Later

扩展完整 NPC/Projectile AI、特殊种子/难度、液体与生态/world update、Wiring/TileEntity、入侵与季节事件、全部 item 行为。液体/世界算法已有正式代码，应先验证运行调度和 owner 写回，再判断需补迁移的部分。通信接入可在 P1 后并行设计，最终仍以 P3/P4 的离线验收场景为准。

### Ignore for Now

无图形基线不要求渲染、声音、聊天 UI、视觉粒子和成就反馈；可以提供明确的表现 sink。不得将出生、伤害、掉落、拾取、库存或持久化效果同样 no-op。非空 CreativePowers、所有旧 .wld 版本和全动态 TileEntity 内容属于额外兼容范围，当前标准 319 生成世界成果不能推导它们均已支持。

建议首个新入口接受 world path、有限 tick 数、随机种子、输入脚本和 report/save path；这是建议接口，当前仓库没有对应可运行命令。默认先做离线 finite-run，而后增加实时运行。自动验收可手动 Step 3600 次，不必真实等待一分钟。

## 11. Quick Wins — 低成本先做

| 任务 | 价值 | 建议耗时 |
|---|---|---|
| 明确 Projectile 的 NpcSlot 返回身份并补正确依赖 | 解开当前整个 Application/存储构建阻塞 | 1–2 小时修改，加定向回归。 |
| 重建当前项目 solution/筛选入口 | 消除 52 个失效路径 | 1–3 小时。 |
| 把最小运行验收写成脚本规范和支持类型表 | 防止各模块以局部 PASS 代替整体完成 | 1–2 小时。 |
| 固定 snapshot 来源与 tick order 的设计记录 | 避免后续同时维护两份世界真值 | 1–2 小时初稿，实测随 P1/P2。 |

## 12. Long-term Refactor Plan — 只保留必要结构改进

优先补 Host 和真实 adapter，保留现有领域拆分。后续只有在实际运行证据表明 legacy projection 的维护成本或一致性风险不可接受时，才逐步迁移剩余世界算法到 owner 输入/输出边界；每批保留真实地图与保存重载验证。没有证据支持重新设计全部 ECS、重写生成器或创建大量空抽象项目。
