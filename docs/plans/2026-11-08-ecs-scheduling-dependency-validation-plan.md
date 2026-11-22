# ECS Phase 依赖与调度验证执行计划

文档 ID：PLAN-2026-11-08-ECS-SCHEDULING-DEPENDENCY-VALIDATION
逻辑域：plans
产物类型：plan
状态：completed
范围：`EcsServerSystemRegistration`、`EcsServer`、`Program.BuildRegistrations` 及所有有顺序要求的 system registration
证据入口：`src/NSSLC/Components/Share/Entity/EcsServer.cs`；`src/NSSLC/Components/Share/Entity/EcsServerSystemRegistration.cs`；`src/NSSLC.Server/Program.cs`；Valence/Veloren/Terasology 参考调度源码
canonical 路径：`docs/plans/2026-11-08-ecs-scheduling-dependency-validation-plan.md`

## 1. 目标

把当前依赖注册列表位置和中文注释表达的顺序约束，提升为启动时可验证、运行时稳定的 phase 内依赖图。保留现有四 phase 和独立 system 的兼容顺序，不为了形式给所有系统强行声明依赖。

## 2. 必须表达的依赖

第一批至少覆盖当前 `Program.BuildRegistrations` 已明确写出的约束：

- Communication input/preparation 先于读取玩家输入的 Simulation 系统；
- Inventory attribute contribution 先于读取 AttributeSet 的 Combat/状态系统；
- NPC spawn 先于 NPC network identity；
- NPC network identity 先于 NPC network projection；
- AI intent 和 player/movement intent 先于 spatial simulation；
- World interaction/World commit 先于 projection 能观察的 committed facts；
- 所有 outbound projection 在 delivery projection 之前；
- CommandBuffer playback 在任何依赖最终结构的 Projection 之前。

实际名称以源码中的 registration identity 为准，不用类名猜测唯一实例。

## 3. 推荐的最小设计

扩展 `EcsServerSystemRegistration`：

- 稳定唯一 key；
- phase；
- `Before` / `After` 依赖 key 集合；
- 可选 human-readable reason；
- 后续可扩展 read/write set，但本批不要求为每个组件建立完整静态读写系统。

`EcsServer` 在构造或 `Initialize` 前按 phase 构建依赖图，执行稳定拓扑排序：依赖满足优先，独立节点保持原 registration 顺序。启动时拒绝重复 key、缺失依赖、跨 phase 依赖和环，并输出可诊断的路径。

## 4. 执行阶段

### Phase 1：图模型与验证器

- 实现 registration metadata、稳定拓扑排序和 cycle/missing/cross-phase diagnostics。
- 为没有显式依赖的系统保留插入序，防止无关行为变化。
- 验证器不能执行 system，也不能通过“排序成功”宣称行为等价。

### Phase 2：迁移组合根

- 为 `Program.BuildRegistrations` 中顺序敏感的注册项补 key 和依赖。
- 将原注释改为依赖 reason，保留必要的行为说明和 owner 说明。
- 把 phase 之间的顺序限制表达为 phase contract 或 barrier，不伪装成同 phase edge。
- 将 CommandBuffer playback node 纳入图或固定为 EcsServer 内建 barrier，避免被 projection 绕过。

### Phase 3：回归与诊断

- 添加排序结果快照或结构化日志，证明同一输入得到稳定顺序。
- 添加故意重复 key、缺失 key、环和跨 phase edge 的失败测试。
- 添加一个新 system 插入独立位置的测试，证明没有依赖的节点仍保留旧顺序。

## 5. 验收标准

- 关键顺序不再依赖相邻列表位置；删除一个依赖边或制造环时，server 初始化明确失败。
- 现有 server registration 的排序结果与改动前的有效执行顺序一致，除非计划中记录了必要改变。
- `EcsServer` 初始化不会偷偷并行执行 phase 内系统；本批只做验证和稳定排序。
- 依赖 metadata 的生命周期、phase、reason 和验证状态有文档或代码注释，避免未来把 dependency key 当作任意字符串。
- 构建、server startup 和既有 behavior probes 通过；失败时保留排序诊断。

## 6. 后续候选

若以后确实需要并行系统，再在单独计划中引入组件读写集、资源访问声明和 barrier 生成。本批不根据 QueryDescription 自动推断写集，因为 Arch query 不能证明组件内部引用对象的真实写入和外部副作用。

## 7. 本轮执行结果

- `EcsServerSystemRegistration` 已增加稳定 key、`Before` / `After` 和 reason；旧的
  `(phase, system)` 构造保留，并使用仅用于兼容展示的 synthetic key。
- `EcsServer` 已按固定 phase contract 执行 phase 内稳定 Kahn 拓扑排序；独立节点按原始
  registration index 保序。重复 key、缺失依赖、跨 phase 依赖和 cycle 均抛出带 key、phase、
  边或环路径的 `EcsServerSchedulingException`。
- `Program.BuildRegistrations`、通信工厂和战斗工厂已声明当前顺序敏感约束。跨工厂依赖由
  组合根显式传入，独立使用工厂时不会要求不存在的外部 key。
- `EcsServerCommandBuffer` 的现有 Commit → Projection playback barrier 被写入调度合同和
  `EcsServer` 文档；本轮没有扩大为完整 command-buffer migration。

### Verification

| 命令 | 项目/范围 | 结果 | 输出与未覆盖项 |
|---|---|---|---|
| `dotnet build src/NSSLC/Terraria.Components.csproj --no-restore --nologo -v:minimal` | `Terraria.Components` | exit 0；0 warning / 0 error | `Build/bin/Terraria.Components/Debug/net10.0/`; 不覆盖完整服务端组合根 |
| `dotnet build Test/Terraria.EcsScheduling.Verification/Terraria.EcsScheduling.Verification.csproj --no-restore --nologo -v:minimal` | 调度 verification | exit 0；0 warning / 0 error | `Build/bin/Terraria.EcsScheduling.Verification/Debug/net10.0/` |
| `dotnet run --project Test/Terraria.EcsScheduling.Verification/Terraria.EcsScheduling.Verification.csproj --no-build --no-restore` | 排序、Before/After、稳定顺序、重复/缺失/跨 phase/cycle | exit 0；输出 `PASS: ECS scheduling dependency verification` | 覆盖 fake systems，不证明所有领域行为等价 |
| `dotnet build src/NSSLC.Server/NSSLC.Server.csproj --no-restore --nologo -v:minimal` | 服务端组合根 | exit 0；19 warning / 0 error | `Build/bin/NSSLC.Server/Debug/net10.0/`; 19 个 warning 来自既有 WorldGeneration 源码 |
| `dotnet run --project src/NSSLC.Server/NSSLC.Server.csproj --no-build --no-restore -- Build/diagnostics/MinimalImplementation/FinalVerification-20261015-035951936/P08/SC091/World/primary.wld` | 真实 server startup probe | 到达 `ecs.server.ready`，推进到 `lastTick=1627`，Ctrl+C 后记录 stopped；包装进程 exit 1 仅因人工中断 | 验证真实 registration graph 和运行生命周期；未做客户端网络行为等价或长期负载测试 |
