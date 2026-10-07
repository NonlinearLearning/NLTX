# Arch 迁移 T2-B2：生产 World、身份与世界权威状态切换执行合同

文档 ID：DOC-2026-10-08-ARCH-TRACK-T2-B2  
状态：active；这是 T2 的生产切换合同，不是完成报告  
对应主计划：A2（World、身份及生产签名协调切换）  
前置交接：T1 `partial` handoff，commit `0928445dbebdade501bcb39bb944541835538883`  
上游审计：[Arch 原生 API 覆盖与自定义 ECS 退出审计](../reviews/audits/2026-10-08-arch-native-api-coverage-audit.md)  
原 T2 合同：[World、身份与生产签名](2026-10-08-arch-migration-track-t2-world-identity-signatures.md)  
总账：[Arch 迁移执行总账](../migration/ledgers/2026-10-07-arch-migration-execution-ledger.md)

## 1. 本批次唯一目标

把生产 World/session/identity 的权威边界从自定义 `EntityRuntime` 切换到 Arch 核心 API，形成
后续 T3–T5 可以消费的一套正式签名。一个已加载会话必须拥有且只拥有一个 Arch `World`；
一个实体的运行时组件必须只有一份 Arch 权威状态；领域 UUID、世界 session token、协议槽位
和存档 identity 继续由 NLTX 的领域 owner 负责，不能被 Arch `Entity.Id` 替代。

本批次不是“给旧 runtime 加一个 Arch 转发层”。不得保留第二套通用实体 allocator、组件
store/cell、通用 `TryEdit/Match/Snapshot` facade 或双写路径。必要的短期编译过渡必须限定在
本批次、列出删除位置，并在同一批次完成闭包迁移；不能把过渡类型提交为长期兼容架构。

## 2. 已确认的当前源码边界

开始前必须以当前源码重新核对下列事实，不得依赖历史路径或报告中的旧行号：

- `src/NSSLC.Application/WorldStorage/Loading/LoadedWorldSession.cs` 当前持有 `_entityRuntime`、
  `_storage`、`WorldSessionRestoreState` 和发布/释放状态；构造器直接 new `EntityRuntime`，
  `WorldRuntimeId` 当前来自 `_entityRuntime.RuntimeId`，`IsFresh` 当前要求 `EntityCount == 0`。
- `src/NSSLC/Component/Share/Entity/System/EntityRuntime.cs` 当前自建实体索引、generation、
  `Dictionary<Type, IComponentStore>`、BorrowCount、`TryEdit`/`TryInspect`/`Match` 和终止状态。
- `src/NSSLC/Component/Share/Entity/System/EntityIdentityRegistry.cs` 当前将
  `EntityUuid` 映射到 `RuntimeEntityHandle`，并以 `EntityRuntimeId` 做世界作用域。
- `src/NSSLC/Component/Relationships/System/RuntimeEntityHandle.cs` 当前表达
  `RuntimeId/local index/generation`，不能继续作为 Arch Entity 的长期别名。
- `src/NSSLC/Component/WorldStorage/System/WorldStorageRoot.cs` 当前接受 `EntityRuntime`，并暴露
  `ProjectileRuntime`、`TileEntityRuntime` 两个 compatibility alias；这些 alias 不能被改名后长期保留。
- `src/NSSLC/Component/WorldSession/System/WorldSessionRestoreState.cs` 当前在 Arch World 外持有
  `Descriptor`、`Rules`、`TimeWeather`、`Progression` 等可写世界状态；迁移后不能与 Arch 单例
  形成两份可写权威。

直接影响闭包至少包括：`Terraria.EntityEcs`、`Terraria.Relationships`、`Terraria.WorldStorage`、
`Terraria.WorldSession`、`NSSLC.Application`、`NSSLC.Infrastructure.WorldStorage`、
`NSSLC.Tools.Simulation` 以及所有直接构造/读取上述 owner 的生产 caller。`分类参考` 目录
只读排除，不得修改或加入生产编译。

## 3. 硬约束与禁止事项

1. 使用仓库 `global.json` 固定的 .NET SDK `10.0.400`，目标 `net10.0`；Arch 固定 `2.1.0`，
   不因迁移困难降级核心版本。
2. Arch PackageReference 只加入实际直接使用 Arch 类型/API 的项目；不得机械加入所有组件项目。
   每个新增引用必须在报告列出理由、恢复资产和 package/assets hash。
3. 不安装或假设 `Arch.Relationships` 已兼容 Arch 2.1.0；T3 另行处理关系扩展。T2 只使用
   `Arch.Core`/`Arch.Buffer` 的已验证核心 API；不在本批次接入 Arch.System、Arch-Events 或
   SourceGenerator。
4. `Arch.Entity` 只允许出现在运行时 World、领域 owner、身份 registry 和明确的槽位投影中；
   不得写入协议 DTO、网络包、world file、持久化 UUID、玩家/NPC/Projectile identity 或 TileEntity ID。
5. 外部/排队入口的解析顺序固定为：session token → `Entity.WorldId` 与 session World 一致 →
   `World.IsAlive(entity)` → 生命周期/能力 → 组件访问。不能只调用 `World.IsAlive`，因为 T1 已观察
   它可能对 foreign entity 的碰撞局部字段返回 true。
6. 不使用 Arch `Entity.Id` 代替 `EntityUuid`、槽位序号或协议 identity；不使用 `Entity.Version`
   代替跨会话 token。
7. 不把 CommandBuffer 用作本批次的身份事务或回滚工具。T1 记录的 malformed-target `Set`
   进程级 `AccessViolationException` 必须继续作为禁止假设；T2 的 World/identity 切换优先采用
   直接安全创建和显式 owner 清理。
8. 不修改 T3 关系实现、T4 System/查询/网络/物品生产逻辑、T5 性能基线或 A8 删除；如编译
   闭包发现这些路径必须变更，只能记录 precise next-owner action，不能留下双后端。
9. 保留主工作树和 worktree 既有用户变更；禁止 `git reset --hard`、`git clean`、广泛删除和
   全 solution Rebuild。

## 4. 分批执行

### T2-B2.0：依赖与签名冻结

1. 读取根 `AGENTS.md`、`Context/progress.md`、开发/构建约束、ECS 文件组织、Entity、基础
   设施边界、副作用、C# 风格、组件命名、本 T2 合同、主计划、Arch 研究和 T1 handoff。
2. 运行只读扫描，列出所有直接构造 `EntityRuntime`、调用 `TryEdit`/`TryInspect`/`Match`、
   保存 `RuntimeEntityHandle`、读取 `WorldRuntimeId` 和访问 `WorldSessionRestoreState` 的生产 caller。
3. 发布一份唯一 token 决定：可以将现有 `EntityRuntimeId` 明确改造成世界 session token，或
   新建具名 `WorldSessionToken`；不能同时保留两个语义相同的 token。报告必须说明字段、生成时机、
   候选/发布/卸载/重试时的代次和跨线程验证位置。
4. 发布一份唯一 registry 决定：`EntityUuid ↔ (session token, Arch.Entity)` 是唯一 live 映射；
   reverse lookup、issued UUID 历史、owner thread 和 unregister 顺序必须明示。不得以 Arch Entity
   作为 durable key。
5. 确认直接使用 Arch 类型的项目边界和 ProjectReference 闭包；添加最小 PackageReference，
   restore 后记录实际解析资产、包 hash、DLL/PDB 路径。未直接使用 Arch 的项目不能因为传递依赖而被
   机械修改。

### T2-B2.1：LoadedWorldSession 成为唯一 World owner

1. 在 `LoadedWorldSession` 中直接创建一个 Arch `World`；候选会话、正式会话和失败重试不得共享
   World。World 创建失败时不得注册 session/token；World 创建成功后才建立 identity registry 和
   领域 storage owner。
2. `LoadedWorldSession` 暴露经过 disposed/published 状态保护的 World 访问，以及正式 token；
   不再通过 `EntityRuntime` 间接暴露 ECS 存储。所有生产 caller 迁移到该入口，不能增加“兼容
   `EntityRuntime` 构造器”来隐藏未完成闭包。
3. 释放顺序必须可重入且可验证：停止发布/排队入口 → 撤销 UUID/live mapping → 清理槽位和领域
   投影 → 清理已登记缓冲/关系占位 → 释放 World → 标记 session disposed。World 释放失败必须保留
   失败证据，不把旧 session 伪装成已清理。
4. 用 Arch `World.Destroy` 或 `Dispose` 的实际固定 API，不能双重假设两者是两级生命周期；T1
   已确认稳定源码中 `Destroy(World)` 直接调用 `Dispose()`。
5. owner thread 检查属于 session/领域 owner，不假定 Arch 默认提供线程安全。跨线程请求只能携带
   token、UUID 和业务意图，不能携带长期组件 ref。

### T2-B2.2：Identity registry 切换

1. 将 registry 的 live value 从 `RuntimeEntityHandle` 改为完整 Arch `Entity`，并同时保存所属
   session token 或可证明的 World owner；不要保存裸 `Entity.Id`。
2. `Register` 必须只在构建阶段由明确 owner 调用，先确认 Entity 属于当前 World，再写入 identity
   component/领域 identity；任一 attach 或 registry 步骤失败，必须撤销已写入的一侧。
3. `TryResolve`/`TryGet` 必须先验证 token，再验证 `WorldId`，再验证 `World.IsAlive` 和领域状态；
   foreign World、旧 token、旧 Version、disposed World、terminating entity 都返回具名拒绝结果。
4. `Unregister` 必须能重复调用或返回明确的 already-absent 结果；不能因 slot/Entity.Id 复用而
   删除新实体的 mapping。
5. UUID 普通死亡/复活保持不变；真实销毁重建由 owner 新签 UUID。T2 只发布签名和基础行为，真实
   Player/NPC/Projectile 复活/重建由 T3 继续验收。

### T2-B2.3：WorldStorageRoot 与世界单例实体

1. `WorldStorageRoot` 不再以 `EntityRuntime` 为构造权威。需要实体存取的子 owner 接受 session
   World/identity access，或者接受一个明确的领域 owner 接口；不得新造 `RuntimeEntityFacade`。
2. 删除 `ProjectileRuntime`、`TileEntityRuntime` 这类同义 compatibility alias；若 T3/T4 仍有
   caller，报告精确文件和删除批次，不以 alias 继续扩大双后端。
3. 在 Arch World 中建立一个世界单例实体，使用已有明确领域组件组合承载 Rules、TimeWeather、
   Progression、Descriptor 等权威运行态；不得简单把整个 `WorldSessionRestoreState` 再包成一个新的
   通用 resource manager。加载输入可以保留为 DTO/snapshot，但运行时写入只能落在 Arch 单例组件。
4. 重新定义 `IsFresh`：World 单例已经存在不代表普通玩法实体已加载；使用“世界单例已就绪 + 没有
   已提交玩法实体/加载 section/领域投影”的条件，不能继续用 `EntityCount == 0`。
5. 槽位表只保留容量、协议顺序、投影和明确的复用控制；如果保存 Arch Entity，必须每次按 token、
   WorldId、Version、IsAlive 重新验证。槽位状态不能成为第二份通用组件状态。

### T2-B2.4：生产 caller 闭包与最小行为验证

按编译错误和调用扫描迁移所有 T2 闭包内的生产 caller，优先顺序为：

1. `LoadedWorldSession` 创建/发布/Dispose/候选失败路径；
2. `WorldStorageRoot`、TileEntity/slot owner 的构造和释放；
3. `EntityIdentityRegistry` 的注册、解析、撤销入口；
4. Application world-load/save coordinator 的 session 传递；
5. Simulation host 的 session/World 获取与一 tick 入口；
6. 仅为修复编译而触及的网络/关系 caller，不能顺便改变其业务规则。

所有旧 API 引用必须分为：已删除、由 T3/T4/T5 接手、只读参考排除；不得留下“以后再删”的
生产通用转发层。每个未迁移调用都要有文件、符号、owner 和下一批次。

## 5. 约 10% 核心测试预算

只运行以下代表性场景，不运行全量矩阵：

1. 新会话创建 → 建立世界单例 → 读取/修改一个规则组件 → Dispose，验证 owner thread 和重复 Dispose；
2. 两个不同 session World，使用相同局部实体字段构造 foreign lookup，验证 token/WorldId 拒绝；
3. UUID 注册 → resolve → entity destroy/unregister → slot/ID 复用，验证旧 UUID/旧 Version 不命中新实体；
4. 候选 World 建立世界单例后在发布前失败，验证 mapping、slot、单例和 World 全部清理；
5. 兼容输入的真实最小 session one-tick/exit，验证当前生产 caller 使用 Arch World，而非旧 `EntityRuntime`。

五项约占本批次高风险场景的 10%；不运行全 solution build/test，不运行 NPC 200/Projectile 1000
容量，不运行关系扩展、Arch.System、网络 packet 全矩阵或性能 benchmark。若某项因依赖阻塞，报告
必须写 `not-run / blocked-by-prerequisite`，不能用 T1 隔离 probe 代替生产验证。

## 6. 构建与证据要求

每条命令记录项目、完整命令、退出码、warning/error 数、输出路径、source hash、assets hash、
DLL/PDB hash 和输入 world/token hash。只构建受影响项目，输出进入 `Build/bin/`，诊断进入
`Build/diagnostics/ArchMigration/T2-B2/`。

建议命令顺序：

```powershell
dotnet restore <受影响的 Arch 直接使用项目>
dotnet build <受影响项目> --no-restore --nologo
dotnet build <T2 专用验证项目> --no-restore --nologo
dotnet run --project <T2 专用验证项目> --no-build --no-restore -- <五项选择参数>
dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore --nologo
dotnet Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll <固定兼容输入> 1 --players 1 --seed 12345 --report Build/diagnostics/ArchMigration/T2-B2/host-smoke.json
```

命令模板不是已运行证据；最终报告只能引用当前源码对应的输出和 hash。编译失败时保留第一条
完整错误及上下文，按 pua 做一次本质不同的验证方向；重复失败后交付 `partial`，不得以旧 DLL
或隔离 probe 伪造生产通过。

## 7. 完成条件与交接

T2-B2 只有满足以下条件才可标 `done`；否则必须标 `partial` 或 `blocked`：

- 受影响生产闭包在当前 SDK/net10.0 下构建；Arch 包资产和直接依赖可复核；
- `LoadedWorldSession` 是唯一 Arch World owner，候选/发布/卸载/失败清理有当前源码路径；
- registry 映射完整 `(token, Arch.Entity)`，所有入口先 token/WorldId/IsAlive，且 UUID 不被替代；
- 世界权威可写状态只有 Arch 单例组件一份，`WorldSessionRestoreState` 不再第二次写入同一状态；
- `WorldStorageRoot` 不再拥有通用自建 ECS，compatibility alias 有明确删除/交接记录；
- 五项约 10% 场景的选择、结果、跳过项、命令和 hash 完整；至少一条当前生产 one-tick/exit 证据；
- T3/T4/T5 得到精确的新签名、生命周期清理顺序、未迁移 caller 和禁止假设；
- 生成 `Build/diagnostics/ArchMigration/T2-B2/handoff.md`，提交 scoped commit；不声称 A2–A8 已完成。

交接给 T3：完整 Entity/UUID/token/World 访问签名、普通复活与真实重建的 owner 边界、失败清理入口。  
交接给 T4：原生组件访问入口、不能跨结构变化持有 ref 的边界、队列只能携带 token/UUID/意图。  
交接给 T5：World owner/Dispose/host one-tick 入口、世界单例初始化和 IsFresh 语义、保存输入边界。

## 8. PUA 自监督要求

连续两次失败、重复微调同一路径或准备将问题归因环境时，必须记录：完整错误、原始源码上下文、
SDK/包/项目/路径前置假设、反向假设、最小隔离结果和新方向。至少切换一次本质不同的方案，例如：

- 从“改构造器直到编译”切换为“先生成 caller/owner 闭包和 package asset 矩阵”；
- 从“用兼容 alias 保持旧 API”切换为“按真实 owner 一次性改签名并列出下一批次”；
- 从“用 `Entity.Id` 代替旧 handle”切换为“完整 Entity + token + WorldId/Version 解析”；
- 从“继续重跑崩溃 probe”切换为“保存原始进程证据并以固定源码/行为边界决定禁止假设”。

报告必须说明 PUA 检查改变了哪一个执行方向；不能只写“已使用 skill”。

