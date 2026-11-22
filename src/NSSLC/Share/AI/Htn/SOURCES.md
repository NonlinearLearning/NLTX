# FluidHTN 上游文件对应

固定提交：e67af264cfdf240053f392d4e0e6c620c454eb97。

下表列出上游核心的每个类型。除命名空间替换、文件拆分、来源头和 nullable 指令外，
类型内代码 token 与固定版本相同。此为静态源码比对，不是运行行为验证。

| 本地文件 | 上游 Fluid-HTN 下路径 | 类型源码比对 |
| --- | --- | --- |
| BaseDomainBuilder.cs | BaseDomainBuilder.cs | 相同 |
| Domain.cs | Domain.cs | 相同 |
| DomainBuilder.cs | DomainBuilder.cs | 相同 |
| IDomain.cs | IDomain.cs | 相同 |
| Conditions/FuncCondition.cs | Conditions/FuncCondition.cs | 相同 |
| Conditions/ICondition.cs | Conditions/ICondition.cs | 相同 |
| Contexts/BaseContext.cs | Contexts/BaseContext.cs | 相同 |
| Contexts/ContextState.cs | Contexts/IContext.cs | 相同 |
| Contexts/PartialPlanEntry.cs | Contexts/IContext.cs | 相同 |
| Contexts/IContext.cs | Contexts/IContext.cs | 相同 |
| Debug/Debug.cs | Debug/DecompositionLogEntry.cs | 相同 |
| Debug/IBaseDecompositionLogEntry.cs | Debug/DecompositionLogEntry.cs | 相同 |
| Debug/IDecompositionLogEntry.cs | Debug/DecompositionLogEntry.cs | 相同 |
| Debug/DecomposedCompoundTaskEntry.cs | Debug/DecompositionLogEntry.cs | 相同 |
| Debug/DecomposedConditionEntry.cs | Debug/DecompositionLogEntry.cs | 相同 |
| Debug/DecomposedEffectEntry.cs | Debug/DecompositionLogEntry.cs | 相同 |
| Effects/ActionEffect.cs | Effects/ActionEffect.cs | 相同 |
| Effects/EffectType.cs | Effects/EffectType.cs | 相同 |
| Effects/IEffect.cs | Effects/IEffect.cs | 相同 |
| Factory/DefaultFactory.cs | Factory/DefaultFactory.cs | 相同 |
| Factory/IFactory.cs | Factory/IFactory.cs | 相同 |
| Operators/FuncOperator.cs | Operators/FuncOperator.cs | 相同 |
| Operators/IOperator.cs | Operators/IOperator.cs | 相同 |
| Planners/DefaultPlannerState.cs | Planners/DefaultPlannerState.cs | 相同 |
| Planners/IPlannerState.cs | Planners/IPlannerState.cs | 相同 |
| Planners/Planner.cs | Planners/Planner.cs | 相同 |
| Tasks/ITask.cs | Tasks/ITask.cs | 相同 |
| Tasks/TaskStatus.cs | Tasks/TaskStatus.cs | 相同 |
| Tasks/CompoundTasks/CompoundTask.cs | Tasks/CompoundTasks/CompoundTask.cs | 相同 |
| Tasks/CompoundTasks/DecompositionStatus.cs | Tasks/CompoundTasks/DecompositionStatus.cs | 相同 |
| Tasks/CompoundTasks/ICompoundTask.cs | Tasks/CompoundTasks/ICompoundTask.cs | 相同 |
| Tasks/CompoundTasks/IDecomposeAll.cs | Tasks/CompoundTasks/IDecomposeAll.cs | 相同 |
| Tasks/CompoundTasks/PausePlanTask.cs | Tasks/CompoundTasks/PausePlanTask.cs | 相同 |
| Tasks/CompoundTasks/Selector.cs | Tasks/CompoundTasks/Selector.cs | 相同 |
| Tasks/CompoundTasks/Sequence.cs | Tasks/CompoundTasks/Sequence.cs | 相同 |
| Tasks/CompoundTasks/TaskRoot.cs | Tasks/CompoundTasks/TaskRoot.cs | 相同 |
| Tasks/OtherTasks/Slot.cs | Tasks/OtherTasks/Slot.cs | 相同 |
| Tasks/PrimitiveTasks/IPrimitiveTask.cs | Tasks/PrimitiveTasks/IPrimitiveTask.cs | 相同 |
| Tasks/PrimitiveTasks/PrimitiveTask.cs | Tasks/PrimitiveTasks/PrimitiveTask.cs | 相同 |

本项目新增：`Contexts/EnumContext.cs` 提供 enum 访问入口；`HtnAgent.cs` 提供每实体生命周期适配。
`Properties/AssemblyInfo.cs` 由 SDK 项目管理，因此不移植。
