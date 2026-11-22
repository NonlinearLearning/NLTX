# ECS CommandBuffer 与 Commit Barrier 执行计划

文档 ID：PLAN-2026-11-08-ECS-COMMANDBUFFER-COMMIT
逻辑域：plans
产物类型：plan
状态：completed
范围：EcsServer tick context、结构变更、实体定义、session/player/NPC/projectile/inventory 生命周期
证据入口：`src/NSSLC/Components/Share/Entity/EcsServer.cs`；`src/NSSLC/Components/Share/Entity/EcsServerCommandBuffer.cs`；`src/NSSLC.Server/Program.cs`；`src/NSSLC/Components/Share/Entity/ArchEntityDefinitionHost.cs`
canonical 路径：`docs/plans/2026-11-08-ecs-command-buffer-commit-plan.md`

## 1. 目标

将实体创建、销毁、组件 Add/Remove/结构性 Set 统一表示为 tick 内命令，在明确的 Commit barrier 一次 playback。普通域内值写入仍由组件 owner 直接完成；CommandBuffer 不替代所有字段更新。

## 2. 当前基线

`EcsServerCommandBuffer` 已封装 Arch `CommandBuffer`，但 `EcsServer.Update` 没有创建、传递或 playback 它。直接结构变化仍存在于通信 ingress、Program session callback、`ArchEntityDefinitionHost`、`ProjectileLifecycleSystem`、`InventorySystem`、`PlayerItemUseSystem` 和 `NpcNetworkIdentitySystem` 等路径。

## 3. 目标契约

- 每个 `EcsServer.Update` 建立一个与 tick 绑定的 command context；system 只能提交属于当前 tick 的结构命令。
- Commit barrier 在所有 Simulation/Commit 规则已完成后统一 playback；Projection 读取的是 playback 后状态。
- CommandBuffer 的实体句柄、命令顺序、重复 destroy、对已销毁实体的操作和 playback 失败必须有明确语义。
- 结构命令的 owner 记录在命令类型或提交 API 中，便于生命周期清理和诊断。
- 直接 `World.Create/Destroy/Add/Remove` 只允许在组合初始化、CommandBuffer playback 实现和明确的低层适配器中出现；业务 system 不得绕过提交点。

## 4. 执行阶段

### Phase 1：扩展 tick context 和 host barrier

- 设计 `EcsServerTickContext` 如何暴露 command sink；优先使用显式只写接口，避免 system 拿到可随意 playback 的对象。
- `EcsServer` 在 tick 开始创建 command buffer，在 Commit 结束前只允许一次 playback，并在异常/取消时释放未提交命令。
- 为结构命令关联 tick，拒绝跨 tick 使用、重复 playback 和已 disposed 使用。
- 明确 `Preparation` 是否允许提交命令；默认允许提交但统一到 Commit，不能在 worker 或 system 构造期直接修改 World。

### Phase 2：迁移实体创建与销毁

- `ArchEntityDefinitionHost` 改为通过受控 command/creation boundary 创建初始组合，保留定义校验和 registry 绑定。
- session/player open/close、NPC spawn、projectile spawn/termination、inventory item create/destroy 迁移到统一提交路径。
- `NpcNetworkIdentitySystem` 的 Add、`PlayerItemUseSystem` 的 state component Add、Projectile 生命周期 Add/Destroy 和 Inventory relation Remove/Destroy 必须逐处确认是结构变更还是域内值写入。
- 处理 command playback 后才能得到真实 Arch entity 的 API：使用临时 command handle、稳定 `EntityRuntimeHandle` 或两阶段结果，不把未提交句柄泄漏给跨域队列。

### Phase 3：清理旁路与回归

- 静态扫描所有 `World.Create/Destroy/Add/Remove`，每一处标注 allowed initialization/playback/adapter 或迁移到 command sink。
- 删除 `EcsServerCommandBuffer` 未使用的伪入口，或把其生命周期接入 `EcsServer`；不得保留“看似支持 deferred、实际没有调用者”的假边界。
- 对创建失败、部分命令失败、实体已销毁、shutdown 期间未 playback 的行为补测试和日志。

## 5. 验收标准

- 单 tick 结构命令只在一个 Commit barrier 发生；测试能证明 Simulation 期间尚未改变查询结构，而 Projection 能看到 playback 后结构。
- 现有实体定义、玩家连接、NPC spawn、Projectile termination、Inventory pickup/use 的结果与改动前保持一致；若可见顺序改变，必须有明确计划记录。
- 重入、重复 playback、跨 tick command、空 World、取消和 playback 异常都有可识别结果。
- 静态扫描不再发现业务 system/host callback 直接结构修改，除非有逐处注释和测试证明其属于允许的初始化或低层 playback。
- 构建和行为验证按总计划共同门槛记录，不能以一个 CommandBuffer 单测代替宿主回归。

## 6. 依赖与风险

本计划依赖线程所有权计划给出的 Preparation ingress。它会与调度依赖计划共享 `EcsServer`，因此先稳定 command barrier，再引入排序元数据。Arch command buffer 对实体句柄和组件类型的实际限制必须以当前包 API 和运行测试确认，不根据类型名猜测行为。
