# Physics、Combat 与 Projectile 边界收敛实施记录

日期：2026-10-26

对应计划：[Physics、Combat 与 Projectile 边界收敛执行计划](../plans/2026-10-26-physics-combat-projectile-boundary-execution-plan.md)

本次实施将一次性事实从实体组件移到 world-scoped Tick buffer，并把结构性 Projectile 销毁
收敛到唯一生命周期提交点。范围只包括 NSSLC 本地 ECS；网络、客户端表现、连续 Sweep 和
外部宿主接线不在本次实现中。

## 已落地的边界

- `SpatialSimulationCoordinatorSystem` 只查询空间状态和能力，不读取 Life、Faction 或 Attributes。
- `PhysicsContactFact` 携带端点、Tick、`SpatialWorldRevision` 和 Substep；
  `PhysicsContactBuffer` 负责当前 Tick 捕获、去重、Drain 和 Clear。
- `PhysicsContactCaptureSystem` 统一产生几何接触，Projectile 不再枚举目标或重算碰撞。
- Projectile 只保存 `CombatAttackDefinitionId` 引用；攻击载荷在
  `CombatAttackRequestSystem` 转换意图时从封存 Catalog 解析。
- `CombatAttackIntentBuffer` 追加多条来源攻击并生成 source-local RequestId；
  `CombatDamageRequestBuffer` 按请求保存结果，支持按 Source/RequestId 消费。
- NPC AI 只输出目标和攻击定义键；Player 使用 `PlayerAttackIntentAdapter` 写入同一意图缓冲。
- `ProjectileTerminationBuffer` 收集到期、穿透耗尽、Owner 清理和管理员终止；
  `ProjectileTerminationCommitSystem` 稳定排序、去重并调用唯一的
  `ProjectileLifecycleSystem.CommitTermination`。
- `CombatAttackIntentComponent`、`ProjectileTerminationRequestComponent`、旧 Projectile 碰撞系统
  和实体接触缓冲已删除。

## 验证证据

以下项目均使用 `Build/bin/` 和 `Build/obj/` 输出：

```text
dotnet build src/NSSLC/Terraria.Components.csproj --no-restore --nologo -v:q \
  -p:BaseOutputPath=Build/bin/ -p:BaseIntermediateOutputPath=Build/obj/
dotnet build .agent-workplace/buff-smoke/Smoke.csproj --no-restore --nologo -v:q \
  -p:BaseOutputPath=Build/bin/ -p:BaseIntermediateOutputPath=Build/obj/
dotnet build .agent-workplace/combat-smoke/CombatSmoke.csproj --no-restore --nologo -v:q \
  -p:BaseOutputPath=Build/bin/ -p:BaseIntermediateOutputPath=Build/obj/
dotnet build .agent-workplace/boundary-smoke/BoundarySmoke.csproj --no-restore --nologo -v:q \
  -p:BaseOutputPath=Build/bin/ -p:BaseIntermediateOutputPath=Build/obj/
```

四个项目均为 0 warnings、0 errors。三个 smoke 运行均输出 `boundary-smoke: PASS`。
focused verifier 覆盖：

- 无 Life/Faction 的 Arch 实体完成空间移动；
- 版本化 Physics 接触和旧 Tick 拒绝；
- 实体接触捕获进入统一 Physics buffer；
- 多条攻击意图不覆盖；
- 请求边界解析定义并读取目标 Life；
- Combat 结果按 RequestId 独立消费；
- 重复 Projectile 终止只在 Commit 边界销毁一次；
- 旧句柄在 Registry 中不可解析。
- `CombatSystemRegistration` 的注册数组成功交给 `EcsServer.Create` 并初始化，局部
  Simulation/Commit 阶段顺序得到运行时检查。

静态门禁结果：Physics 目录对 Combat/Life/Faction/Attributes/payload 搜索为零命中；旧攻击
组件、旧接触组件、`ProjectileHitCandidate` 和 `DamagePayloadComponent` 为零命中；NPC 和
Projectile 请求适配器不调用 `EntityCollisionSystem.TryGetContact`；Projectile 目录中仅有
生命周期 owner 的结构销毁路径，创建失败回滚仍明确标注为创建事务清理。

## 保留的 partial / unknown

`CombatSystemRegistration` 已证明本地 Simulation/Commit 顺序，但真实游戏宿主是否使用该
入口仍为 partial。连续 Sweep、网络、客户端表现、反射/吸血效果的专门副作用 owner，以及
Projectile Registry 终止历史的有界清理仍是后续技术债。
