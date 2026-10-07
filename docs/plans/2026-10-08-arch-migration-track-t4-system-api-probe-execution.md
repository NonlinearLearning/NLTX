# Arch 迁移 T4-B4：Arch.System 生命周期与旧 System API 映射探针合同

文档 ID：DOC-2026-10-08-ARCH-TRACK-T4-B4  
状态：active；先做官方 System API 事实与旧入口映射，不代表生产 System 已切换  
主线：T4 查询、System、网络与物品；对应 A4  
前置：T1 partial API handoff；T2-B2 生产 World/token 合同完成前，生产接线保持 blocked  
使用方法：`ecs-system`、`ecs-system-api-splitting`、`public-decomposition`

## 1. 目标

验证 `Arch.System 1.1.0` 的 `ISystem<T>`、`BaseSystem<World,T>` 和 `Group<T>` 实际 API 与生命
周期，然后把现有 `IWorldSimulationTickPhase`/`WorldSimulationKernel` 映射为概念行为、owner、
读写集、阶段、barrier 和副作用，而不是按旧方法名机械替换。不能创建 `IEcsSystem`、自制
`EcsScheduler`、自制 Group 或只转发旧 phase 的兼容框架。

## 2. 官方 API 隔离探针

建立 `Test/Terraria.Arch.SystemVerification/`，固定 `Arch.System 1.1.0` 与其实际依赖，按仓库
SDK/net10.0 restore/build。验证约 10% 最高风险行为：

1. 一个 System 的 Initialize → BeforeUpdate → Update → AfterUpdate → Dispose 顺序；
2. 只调用 Group.Update 时前后钩子是否自动执行；
3. Group 注册顺序、嵌套 Group 和同一 World 绑定；
4. Update 中抛异常时后续 system/hook/Dispose 行为；
5. zero-tick create → initialize → dispose；
6. 两个 World 的 system/group 是否隔离，是否能把一个 World 的 Entity/组件 ref 带入另一个 World。

不运行完整 Arch.System API 矩阵，不把 package restore/build 成功当成生命周期行为通过。若
SourceGenerator 也被测试，单独记录它的版本和生成输出；手写 `World.Query` 是可接受的原生 API，
不因未用 generator 判为失败。

## 3. 旧 System 概念映射

针对 `WorldSimulationKernel`、`IWorldSimulationTickPhase` 和实际 phase caller，建立稳定的
`ConceptId` 映射，每个入口记录：

- 输入、权威写入者、读写组件/领域状态、阶段与提交点；
- 网络/存档/随机/时间/日志等副作用及其 owner；
- 旧入口可观察的返回、异常、顺序、可见时点、重试和取消语义；
- 新组合是官方 System、普通领域方法、命令、查询、Adapter 还是 Projection；
- `confirmed/partial/unknown/proposed` 状态和支持它的源码位置。

优先追踪一条真实路径：simulation host → kernel → phase → EntityRuntime `Match/TryEdit` →
domain effect。只读静态映射不能升级为行为等价；被清空或无实现的方法标 `unknown`。

## 4. 生产切换前置与禁止项

- T2 未交付完整 Arch World/token/identity 时，不修改生产 System 接口；只能提交隔离 probe 和映射审计。
- 不把网络队列、物品 reservation、session token 或 owner-thread admission 交给 Arch Group；
  这些仍由领域/宿主 owner 负责。
- 不把 Group 当作自动读写依赖 DAG、事务、并行调度器或异常恢复机制；阶段顺序必须由宿主明确接线。
- 不长期保留旧 phase facade。若某入口必须过渡，记录删除 commit、调用闭包和行为验证计划。

## 5. 证据与交接

证据目录：`Build/diagnostics/ArchMigration/T4-B4/`。记录 package/assets/source/DLL/PDB hash、
完整命令、退出码、warning/error、10% 选择/跳过项和旧→新 ConceptId 映射。生成
`handoff.md`，状态为 `done/partial/blocked/not-run` 之一并提交 scoped commit。

交给 T2：World 绑定与生命周期限制；交给 T4 生产线：正式 System API 组合与阶段顺序；交给 T5：
host 的 Initialize/BeforeUpdate/Update/AfterUpdate/Dispose 接线和 zero-tick/异常停止门禁。

