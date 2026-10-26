# GOAP 上游文件对应

固定提交：7fb29cfe59dc61aa8cf1f69880d3ef95fc29126c。

源仓库：<https://github.com/crashkonijn/GOAP/tree/7fb29cfe59dc61aa8cf1f69880d3ef95fc29126c>。

适配类型补充：`GoapActionBase.cs` 结合上游 `CrashKonijn.Goap.Runtime/Behaviours/GoapActionBase.cs`
和 `CrashKonijn.Agent.Runtime/AgentActionBase.cs`；`ActionConfig.cs` 与 `GoalBase.cs` 对应上游
`IActionConfig` / `GoalBase`。`ActionRunner.cs` 来自 `CrashKonijn.Agent.Runtime/ActionRunner.cs`，
去除 Unity Proxy 并接入本目录的 Agent 环境端口。`GraphResolverJob.cs` 按上游同名实现逐步骤
替换 Native 集合为 .NET 集合，保留打开集排序、G/H/P 代价、启发项、连接遍历和路径回溯；
`ResolveHandle.cs` 同步运行该搜索并映射结果。`GoapAgent.cs`、`GoapActionProvider.cs`、
`WorldData.cs`、`IAgentEnvironment.cs` 是项目适配层。

## 需要保留的适配差异

- Agent 与 GOAP 的 Core/Runtime 命名空间合并到 `Terraria.Ai.Goap.Core/Runtime`。
  `IMonoAgent` / `IActionReceiver` 由无 Unity 继承的 `IAgent` 接口承接；组件注入、Unity 事件、
  Logger 和 Transform 不作为兼容空壳保留，改由 `IAgentEnvironment` 提供观察和命令。
- GraphBuilder 的连接与冲突过滤逻辑沿用上游；GraphResolver 构建相同索引与两组映射。
  NativeMultiHashMap 改为 ILookup，NativeHashMap 改为 Dictionary，NativeArray/List 改为数组/List，
  float3 改为 System.Numerics.Vector3。移除 Burst/Jobs/Dispose 分配流程，StartResolve 同步完成。
  NativeHeap 未被此图搜索使用，不移植。等成本候选遍历顺序可能与 Native 容器不同。
- RunData 入参检查是项目补充；数组必须与该 resolver 对应、start 必须是目标索引、成本和距离倍率
  有限且非负。Handle 无池复用，结果数组属于调用方，不随下一次解析被覆盖。
- ActionRunner 保留两种移动模式与 ShouldContinue/BeforePerform/Perform 路径。修正原源码在更新
  RunState 时尚未刷新本次 DeltaTime/范围的顺序；现在先写本次 Context。
- AgentActionBase 保留 ActionData 创建及泛型生命周期分派；Stop/Complete 包装器增加 finally，
  保证业务停止/完成回调抛异常时仍调用 End。异常继续向调用方传播。
- WorldDataBase 的比较规则保留；删去 Unity Timer 与父级传感器调度。事实以规范 key.Name 存储，
  与图连接规则一致；WorldKeyBase 默认为类型全名，避免同名类型冲突，也支持 WorldKey 实例名称。
  原源码按 Type 存储的策略未原样保留。缺失事实不满足条件。
- GoapAgent 与 GoapActionProvider 是本项目执行适配器，不是完整 AgentBehaviour/AgentType 的移植。
  采用下一 tick 重规划、显式观察刷新和 StopMoving 清理。暂停会停止动作，恢复会重新解析。
  图模型和动作配置构建后视为不可变；当前不提供 Unity 的配置工厂、事件总线、传感器缓存或 Jobs。
- IGoal/IGoapAction/IAction 移除 Unity 配置查询和组件引用参数，保留实际执行所需契约。
  禁用由 GoapAgent.SetEnabled 管理；时间禁用器和旧 API 兼容入口未移植。
  ActionRunState 保留 Continue/ContinueOrResolve/Completed/Stop/三类 Wait，未引入 StopAndLog。
- 移植文件保留上游格式和旧可空约定，以 `#nullable disable` 显式声明；项目适配文件启用可空检查。
  一个核心公开类型对应一个文件，原合并文件在本地拆开。

## 补充文件对应

| 本地 | 上游来源或性质 |
| --- | --- |
| Core/IValueCondition.cs、IReferenceCondition.cs | CrashKonijn.Goap.Core/Interfaces/ICondition.cs 拆分 |
| Core/IWorldKey.cs | CrashKonijn.Goap.Core/Interfaces/IWorldKey.cs，展开 IConfig.Name |
| Core/IKeyResolver.cs | CrashKonijn.Goap.Core/Interfaces/IKeyResolver.cs，去除未使用的世界数据注入 |
| Core/Comparison.cs | CrashKonijn.Goap.Core/Enums/Comparison.cs |
| Core/IGoal.cs、IGoapAction.cs | CrashKonijn.Goap.Core/Interfaces 下同名文件，缩减平台契约 |
| Core/IAction.cs、IActionState.cs、IActionData.cs、ITarget.cs | CrashKonijn.Agent.Core/Interfaces 下同名文件 |
| Core/IAgent.cs | CrashKonijn.Agent.Core/Interfaces/IAgent.cs、IActionReceiver.cs 的项目端口 |
| Core/IAgentEnvironment.cs | 本项目与感知、运动、日志领域系统的接口 |
| Core/IWorldData.cs、Runtime/WorldData.cs | CrashKonijn.Goap.Runtime/WorldDataBase.cs 的事实比较与存储适配 |
| Resolver/GraphResolver.cs | CrashKonijn.Goap.Resolver/GraphResolver.cs，使用 .NET 映射与同步 handle |
| Resolver/GraphResolverJob.cs、NodeData.cs、NodeSorter.cs、RunData.cs | CrashKonijn.Goap.Resolver/GraphResolverJob.cs 拆分与平台移植 |
| Resolver/ResolveHandle.cs、JobResult.cs | CrashKonijn.Goap.Resolver/ResolveHandle.cs 拆分与同步适配 |
| Runtime/ActionRunner.cs | CrashKonijn.Agent.Runtime/ActionRunner.cs |
| Runtime/GoalBase.cs | CrashKonijn.Goap.Runtime/Behaviours/GoalBase.cs，构造配置代替 Unity 配置 |
| Runtime/GoapActionBase.cs | CrashKonijn.Goap.Runtime/Behaviours/GoapActionBase.cs，平台与查询适配 |
| Runtime/ActionConfig.cs | CrashKonijn.Goap.Core/Interfaces/IActionConfig.cs 的不可变初始化配置 |
| Runtime/WorldKeyBase.cs | CrashKonijn.Goap.Runtime/Behaviours/WorldKeyBase.cs，默认使用类型全名 |
| Runtime/GoapAgent.cs | AgentBehaviour 生命周期的本项目宿主；不保留 MonoBehaviour |
| Runtime/GoapActionProvider.cs | AgentTypeJobRunner/GoapActionProvider 的事实组装与动作分派适配 |

## 直接移植文件

| 本地 | 上游 Package/Runtime 下路径 |
| --- | --- |
| Core/IConnectable.cs | CrashKonijn.Goap.Core/Interfaces/IConnectable.cs |
| Core/ICondition.cs | CrashKonijn.Goap.Core/Interfaces/ICondition.cs |
| Core/IEffect.cs | CrashKonijn.Goap.Core/Interfaces/IEffect.cs |
| Core/IGraph.cs | CrashKonijn.Goap.Core/Interfaces/IGraph.cs |
| Core/INode.cs | CrashKonijn.Goap.Core/Interfaces/INode.cs |
| Core/INodeCondition.cs | CrashKonijn.Goap.Core/Interfaces/INodeCondition.cs |
| Core/INodeEffect.cs | CrashKonijn.Goap.Core/Interfaces/INodeEffect.cs |
| Resolver/Graph.cs | CrashKonijn.Goap.Resolver/Models/Graph.cs |
| Resolver/Node.cs | CrashKonijn.Goap.Resolver/Models/Node.cs |
| Resolver/NodeCondition.cs | CrashKonijn.Goap.Resolver/Models/NodeCondition.cs |
| Resolver/NodeEffect.cs | CrashKonijn.Goap.Resolver/Models/NodeEffect.cs |
| Resolver/GraphBuilder.cs | CrashKonijn.Goap.Resolver/GraphBuilder.cs |
| Resolver/Extensions.cs | CrashKonijn.Goap.Resolver/Extensions.cs |
| Resolver/ConditionBuilder.cs | CrashKonijn.Goap.Resolver/ConditionBuilder.cs |
| Resolver/CostBuilder.cs | CrashKonijn.Goap.Resolver/CostBuilder.cs |
| Resolver/EnabledBuilder.cs | CrashKonijn.Goap.Resolver/EnabledBuilder.cs |
| Resolver/ExecutableBuilder.cs | CrashKonijn.Goap.Resolver/ExecutableBuilder.cs |
| Resolver/PositionBuilder.cs | CrashKonijn.Goap.Resolver/PositionBuilder.cs |
| Resolver/IConditionBuilder.cs | CrashKonijn.Goap.Resolver/Interfaces/IConditionBuilder.cs |
| Resolver/ICostBuilder.cs | CrashKonijn.Goap.Resolver/Interfaces/ICostBuilder.cs |
| Resolver/IEnabledBuilder.cs | CrashKonijn.Goap.Resolver/Interfaces/IEnabledBuilder.cs |
| Resolver/IExecutableBuilder.cs | CrashKonijn.Goap.Resolver/Interfaces/IExecutableBuilder.cs |
| Resolver/IPositionBuilder.cs | CrashKonijn.Goap.Resolver/Interfaces/IPositionBuilder.cs |
| Resolver/IGraphResolver.cs | CrashKonijn.Goap.Resolver/Interfaces/IGraphResolver.cs |
| Resolver/IResolveHandle.cs | CrashKonijn.Goap.Resolver/Interfaces/IResolveHandle.cs |
| Runtime/ValueCondition.cs | CrashKonijn.Goap.Runtime/Classes/ValueCondition.cs |
| Runtime/ReferenceCondition.cs | CrashKonijn.Goap.Runtime/Classes/ReferenceCondition.cs |
| Runtime/Effect.cs | CrashKonijn.Goap.Runtime/Classes/Effect.cs |
| Runtime/WorldKey.cs | CrashKonijn.Goap.Runtime/Configs/WorldKey.cs |
| Runtime/KeyResolver.cs | CrashKonijn.Goap.Runtime/Resolvers/KeyResolver.cs |
| Runtime/AgentActionBase.cs | CrashKonijn.Agent.Runtime/ActionBase.cs |
| Runtime/EmptyActionProperties.cs | CrashKonijn.Agent.Runtime/EmptyActionProperties.cs |
| Runtime/ActionState.cs | CrashKonijn.Agent.Runtime/ActionState.cs |
| Runtime/ActionContext.cs | CrashKonijn.Agent.Runtime/ActionContext.cs |
| Core/IActionProperties.cs | CrashKonijn.Agent.Core/Interfaces/IActionProperties.cs |
| Core/IActionContext.cs | CrashKonijn.Agent.Core/Interfaces/IActionContext.cs |
| Core/IActionRunState.cs | CrashKonijn.Agent.Core/Interfaces/IActionRunState.cs |
| Core/ActionMoveMode.cs | CrashKonijn.Agent.Core/Enums/ActionMoveMode.cs |
| Core/AgentState.cs | CrashKonijn.Agent.Core/Enums/AgentState.cs |
| Core/AgentMoveState.cs | CrashKonijn.Agent.Core/Enums/AgentMoveState.cs |
| Runtime/RunStates/ContinueActionRunState.cs | CrashKonijn.Goap.Runtime/Classes/RunStates/ContinueActionRunState.cs |
| Runtime/RunStates/ContinueOrResolveActionRunState.cs | CrashKonijn.Goap.Runtime/Classes/RunStates/ContinueOrResolveActionRunState.cs |
| Runtime/RunStates/CompletedActionRunState.cs | CrashKonijn.Goap.Runtime/Classes/RunStates/CompletedActionRunState.cs |
| Runtime/RunStates/StopActionRunState.cs | CrashKonijn.Goap.Runtime/Classes/RunStates/StopActionRunState.cs |
| Runtime/RunStates/WaitActionRunState.cs | CrashKonijn.Goap.Runtime/Classes/RunStates/WaitActionRunState.cs |
| Runtime/RunStates/WaitThenCompleteActionRunState.cs | CrashKonijn.Goap.Runtime/Classes/RunStates/WaitThenCompleteActionRunState.cs |
| Runtime/RunStates/WaitThenStopActionRunState.cs | CrashKonijn.Goap.Runtime/Classes/RunStates/WaitThenStopActionRunState.cs |
| Runtime/RunStates/ActionRunState.cs | CrashKonijn.Goap.Runtime/Classes/RunStates/ActionRunState.cs |
